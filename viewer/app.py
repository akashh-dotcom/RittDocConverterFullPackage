"""
Lightweight R2Library-style viewer.

Uses the actual R2Library XSLT files (RittBook.xsl, ritttoc.xsl) to transform
Bookloader-produced section XML files into HTML, and serves them with
R2Library CSS styling via Flask.
"""

import io
import os
import glob
import re
import zipfile
from pathlib import Path
from lxml import etree
from flask import (Blueprint, render_template, abort, send_from_directory,
                   send_file, request, jsonify, current_app)

viewer_bp = Blueprint("viewer", __name__, template_folder="templates")

# ---------------------------------------------------------------------------
# Paths resolved at import time (overridable via app.config)
# ---------------------------------------------------------------------------
BASE_DIR = Path(__file__).resolve().parent.parent          # script root
CONTENT_DIR = BASE_DIR / "viewer" / "content"              # sectioned XMLs


def _find_repo_root():
    """Find the main repository root (handles git worktree case)."""
    d = BASE_DIR
    for _ in range(10):
        has_r2 = any((d / p).exists() for p in [
            "R2Library-main/R2Library-main", "R2Library-main"
        ])
        if has_r2:
            return d
        d = d.parent
    return BASE_DIR


REPO_ROOT = _find_repo_root()

# R2Library paths (relative to repo root)
R2_ROOT = None  # set in init_viewer()
XSL_DIR = None
CSS_DIR = None

# Cached XSLT transforms
_xslt_book = None
_xslt_toc = None
_dtd_dir = None


def _find_r2library_root():
    """Locate R2Library-main directory."""
    candidates = [
        REPO_ROOT / "R2Library-main" / "R2Library-main" / "src" / "R2V2.Web",
        REPO_ROOT / "R2Library-main" / "src" / "R2V2.Web",
        BASE_DIR / "R2Library-main" / "R2Library-main" / "src" / "R2V2.Web",
        BASE_DIR / "R2Library-main" / "src" / "R2V2.Web",
    ]
    for c in candidates:
        if (c / "_Static" / "Xsl" / "RittBook.xsl").exists():
            return c
    return None


def _find_dtd_dir():
    """Find DTD directory from Bookloader or RittDocConverter."""
    candidates = [
        REPO_ROOT / "Bookloader-main (2)" / "Bookloader-main" / "dtd" / "v1.1",
        REPO_ROOT / "Bookloader-main" / "dtd" / "v1.1",
        REPO_ROOT / "RittDocConverter-main (1)" / "RittDocConverter-main" / "RITTDOCdtd" / "v1.1",
        REPO_ROOT / "RittDocConverter-main" / "RITTDOCdtd" / "v1.1",
        BASE_DIR / "Bookloader-main (2)" / "Bookloader-main" / "dtd" / "v1.1",
        BASE_DIR / "Bookloader-main" / "dtd" / "v1.1",
    ]
    for c in candidates:
        if c.exists():
            return c
    return None


def init_viewer(app):
    """Initialize the viewer with the Flask app context."""
    global R2_ROOT, XSL_DIR, CSS_DIR, _xslt_book, _xslt_toc, _dtd_dir

    R2_ROOT = _find_r2library_root()
    if R2_ROOT is None:
        app.logger.warning("R2Library _Static directory not found. Viewer will use raw XML.")
        return

    XSL_DIR = R2_ROOT / "_Static" / "Xsl"
    CSS_DIR = R2_ROOT / "_Static" / "Css"
    _dtd_dir = _find_dtd_dir()

    # Pre-compile XSLT stylesheets
    try:
        _xslt_book = _compile_xslt(XSL_DIR / "RittBook.xsl")
        app.logger.info("Compiled RittBook.xsl successfully")
    except Exception as e:
        app.logger.warning(f"Failed to compile RittBook.xsl: {e}")
        _xslt_book = None

    try:
        _xslt_toc = _compile_xslt(XSL_DIR / "ritttoc.xsl")
        app.logger.info("Compiled ritttoc.xsl successfully")
    except Exception as e:
        app.logger.warning(f"Failed to compile ritttoc.xsl: {e}")
        _xslt_toc = None


def _compile_xslt(xsl_path):
    """Compile an XSLT stylesheet from file.

    Handles trailing whitespace in xsl:include/xsl:import href attributes
    that exist in the R2Library source XSL files.
    """
    # Read and fix trailing spaces in href attributes
    content = xsl_path.read_text(encoding="utf-8")
    # Fix: href="file.xsl " → href="file.xsl"
    content = re.sub(
        r'(href\s*=\s*"[^"]*?)\s+"',
        r'\1"',
        content,
    )

    parser = etree.XMLParser(
        dtd_validation=False,
        load_dtd=True,
        no_network=True,
        resolve_entities=True,
    )
    xsl_doc = etree.fromstring(content.encode("utf-8"), parser)
    # Set base URL so relative includes resolve correctly
    xsl_tree = xsl_doc.getroottree()
    xsl_doc.base = str(xsl_path)
    return etree.XSLT(etree.ElementTree(xsl_doc))


def _parse_xml(xml_path):
    """Parse an XML file, handling DTD entity resolution."""
    parser = etree.XMLParser(
        dtd_validation=False,
        load_dtd=True,
        no_network=True,
        resolve_entities=True,
        recover=True,
    )

    # If DTD dir exists, create a custom resolver
    if _dtd_dir and _dtd_dir.exists():
        class DTDResolver(etree.Resolver):
            def resolve(self, system_url, public_id, context):
                if system_url:
                    name = os.path.basename(system_url)
                    local = _dtd_dir / name
                    if local.exists():
                        return self.resolve_filename(str(local), context)
                return None
        parser.resolvers.add(DTDResolver())

    return etree.parse(str(xml_path), parser)


def _transform_content(xml_path, isbn, section=""):
    """Apply XSLT transformation to produce HTML."""
    try:
        doc = _parse_xml(xml_path)
    except Exception as e:
        return f'<div class="error">Error parsing XML: {e}</div>'

    xslt = _xslt_book
    params = {
        "baseUrl": etree.XSLT.strparam(""),
        "imageBaseUrl": etree.XSLT.strparam("/images"),
        "isbndir": etree.XSLT.strparam(isbn),
        "email": etree.XSLT.strparam("0"),
        "version": etree.XSLT.strparam("2.0.0.0"),
    }

    if not xslt:
        # Fallback: return raw XML as preformatted text
        raw = etree.tostring(doc, pretty_print=True, encoding="unicode")
        return f"<pre>{raw}</pre>"

    try:
        result = xslt(doc, **params)
        # XSLT output may contain Latin-1 bytes (e.g. © 0xa9, NBSP 0xa0)
        raw_bytes = bytes(result)
        try:
            html = raw_bytes.decode("utf-8")
        except UnicodeDecodeError:
            html = raw_bytes.decode("windows-1252")
        # Clean up XML/HTML declarations that may appear
        html = re.sub(r'<\?xml[^?]*\?>', '', html)
        html = re.sub(r'<!DOCTYPE[^>]*>', '', html)
        return html
    except Exception as e:
        # Fallback to raw XML
        raw = etree.tostring(doc, pretty_print=True, encoding="unicode")
        return f'<div class="error">XSLT error: {e}</div><pre>{raw}</pre>'


def _transform_toc(xml_path, isbn):
    """Apply TOC XSLT transformation."""
    try:
        doc = _parse_xml(xml_path)
    except Exception as e:
        return f'<div class="error">Error parsing TOC XML: {e}</div>'

    if not _xslt_toc:
        raw = etree.tostring(doc, pretty_print=True, encoding="unicode")
        return f"<pre>{raw}</pre>"

    params = {
        "baseUrl": etree.XSLT.strparam(""),
        "contentlinks": etree.XSLT.strparam("1"),
        "disablelinks": etree.XSLT.strparam(""),
        "email": etree.XSLT.strparam("0"),
    }

    try:
        result = _xslt_toc(doc, **params)
        raw_bytes = bytes(result)
        try:
            html = raw_bytes.decode("utf-8")
        except UnicodeDecodeError:
            html = raw_bytes.decode("windows-1252")
        html = re.sub(r'<\?xml[^?]*\?>', '', html)
        html = re.sub(r'<!DOCTYPE[^>]*>', '', html)
        return html
    except Exception as e:
        raw = etree.tostring(doc, pretty_print=True, encoding="unicode")
        return f'<div class="error">TOC XSLT error: {e}</div><pre>{raw}</pre>'


def _get_books():
    """List all processed books (by ISBN directory)."""
    content_dir = current_app.config.get("CONTENT_DIR", CONTENT_DIR)
    books = []
    if not Path(content_dir).exists():
        return books
    for d in sorted(Path(content_dir).iterdir()):
        if d.is_dir():
            isbn = d.name
            meta = _get_book_metadata(d, isbn)
            toc_files = list(d.glob(f"toc.*.xml")) + list(d.glob(f"toc.{isbn}.xml"))
            # Group files into Images and XMLs subfolders
            image_exts = {'.jpg', '.jpeg', '.png', '.gif', '.bmp', '.svg'}
            xml_files = []
            image_files = []
            for f in sorted(d.iterdir()):
                if f.is_file():
                    entry = {
                        "name": f.name,
                        "size": f.stat().st_size,
                        "ext": f.suffix.lower(),
                    }
                    if f.suffix.lower() in image_exts:
                        image_files.append(entry)
                    else:
                        xml_files.append(entry)
            books.append({
                "isbn": isbn,
                "title": meta.get("title") or isbn,
                "cover_image": meta.get("cover_image"),
                "authors": meta.get("authors", []),
                "xml_files": xml_files,
                "image_files": image_files,
                "xml_count": len(xml_files),
                "image_count": len(image_files),
                "file_count": len(xml_files) + len(image_files),
                "has_toc": len(toc_files) > 0,
            })
    return books


def _get_book_metadata(book_dir, isbn):
    """Extract book metadata from the book XML file."""
    book_files = list(book_dir.glob(f"book.{isbn}.xml")) + list(book_dir.glob("book.*.xml"))
    meta = {
        "title": "",
        "authors": [],
        "publisher": "",
        "pubdate": "",
        "edition": "",
        "isbn13": isbn if len(isbn) == 13 else "",
        "isbn10": isbn if len(isbn) == 10 else "",
        "copyright": "",
        "cover_image": None,
    }
    if not book_files:
        return meta
    try:
        parser = etree.XMLParser(recover=True, dtd_validation=False, load_dtd=False,
                                 no_network=True, huge_tree=True)
        tree = etree.parse(str(book_files[0]), parser)
        root = tree.getroot()
        bookinfo = root.find(".//bookinfo")
        if bookinfo is None:
            return meta
        title_el = bookinfo.find("title")
        if title_el is not None and title_el.text:
            meta["title"] = title_el.text.strip()
        isbn_el = bookinfo.find("isbn")
        if isbn_el is not None and isbn_el.text:
            raw_isbn = isbn_el.text.strip().replace("-", "")
            if len(raw_isbn) == 13:
                meta["isbn13"] = raw_isbn
            elif len(raw_isbn) == 10:
                meta["isbn10"] = raw_isbn
        publisher_el = bookinfo.find(".//publishername")
        if publisher_el is not None and publisher_el.text:
            meta["publisher"] = publisher_el.text.strip()
        pubdate_el = bookinfo.find("pubdate")
        if pubdate_el is not None and pubdate_el.text:
            meta["pubdate"] = pubdate_el.text.strip()
        edition_el = bookinfo.find("edition")
        if edition_el is not None and edition_el.text:
            meta["edition"] = edition_el.text.strip()
        copyright_el = bookinfo.find(".//holder")
        if copyright_el is not None and copyright_el.text:
            meta["copyright"] = copyright_el.text.strip()
        for author in bookinfo.findall(".//author"):
            fn = author.findtext(".//firstname") or ""
            ln = author.findtext(".//surname") or ""
            name = f"{fn} {ln}".strip()
            if name:
                meta["authors"].append(name)
        # Check for cover image
        cover_patterns = ["CoverImage.jpg", "CoverImage.png", "cover.jpg", "cover.png",
                          "Coverf01.jpg", "Coverf01.png", "Cover.jpg", "Cover.png"]
        for img_name in cover_patterns:
            if (book_dir / img_name).exists():
                meta["cover_image"] = img_name
                break
    except Exception:
        pass
    return meta


def _parse_toc_structure(toc_xml_path, isbn):
    """Parse TOC XML into a structured list for template rendering."""
    try:
        parser = etree.XMLParser(recover=True, dtd_validation=False, load_dtd=False,
                                 no_network=True)
        tree = etree.parse(str(toc_xml_path), parser)
        root = tree.getroot()
    except Exception:
        return None

    toc = {"title": "", "entries": []}
    title_el = root.find("title")
    if title_el is not None and title_el.text:
        toc["title"] = title_el.text.strip()

    # Group preface entries under "Front Matter"
    preface_entries = root.findall("tocentry")
    if preface_entries:
        preface_children = []
        first_id = ""
        for entry in preface_entries:
            linkend = entry.get("linkend", "")
            title = entry.findtext("title") or linkend
            preface_children.append({"id": linkend, "title": title})
            if not first_id:
                first_id = linkend
        toc["entries"].append({
            "type": "chapter",
            "id": first_id,
            "label": "",
            "title": "Front Matter",
            "display_title": "FRONT MATTER",
            "children": preface_children,
        })

    # Chapter entries with sect1 children
    for chap in root.findall("tocchap"):
        linkend = chap.get("linkend", "")
        label = chap.get("label", "")
        title = chap.findtext("title") or f"Chapter {label}"
        children = []
        for sect in chap.findall("tocsect1"):
            s_id = sect.get("linkend", "")
            s_title = sect.findtext("title") or s_id
            children.append({"id": s_id, "title": s_title})
        clean_title = title
        if label:
            clean_title = re.sub(r'^\d+[\.\:]\s*', '', title)
            display_title = f"{label}: {clean_title}"
        else:
            display_title = title
        toc["entries"].append({
            "type": "chapter",
            "id": linkend,
            "label": label,
            "title": title,
            "display_title": display_title.upper(),
            "children": children,
        })

    return toc


def _get_sections(isbn):
    """List all sections for a given ISBN."""
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn
    if not book_dir.exists():
        return []

    sections = []
    for f in sorted(book_dir.glob("*.xml")):
        name = f.stem
        # Extract section id from filename like sect1.{isbn}.{section}
        if name.startswith("sect1."):
            parts = name.split(".", 2)
            if len(parts) >= 3:
                section_id = parts[2]
                sections.append({
                    "id": section_id,
                    "file": f.name,
                    "type": "section",
                })
        elif name.startswith("appendix."):
            parts = name.split(".", 2)
            if len(parts) >= 3:
                section_id = parts[2]
                sections.append({
                    "id": section_id,
                    "file": f.name,
                    "type": "appendix",
                })
        elif name.startswith("preface."):
            parts = name.split(".", 2)
            if len(parts) >= 3:
                section_id = parts[2]
                sections.append({
                    "id": section_id,
                    "file": f.name,
                    "type": "preface",
                })
        elif name.startswith("dedication."):
            parts = name.split(".", 2)
            if len(parts) >= 3:
                section_id = parts[2]
                sections.append({
                    "id": section_id,
                    "file": f.name,
                    "type": "dedication",
                })
    return sections


# ---------------------------------------------------------------------------
# Routes
# ---------------------------------------------------------------------------

@viewer_bp.route("/")
def index():
    """List all processed books."""
    books = _get_books()
    return render_template("index.html", books=books)


@viewer_bp.route("/viewer/<isbn>/")
def book_toc(isbn):
    """Show table of contents for a book."""
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn

    if not book_dir.exists():
        abort(404)

    book_meta = _get_book_metadata(book_dir, isbn)

    # Find TOC file
    toc_files = list(book_dir.glob(f"toc.{isbn}.xml")) + list(book_dir.glob("toc.*.xml"))
    toc_html = ""
    toc_data = None
    if toc_files:
        toc_data = _parse_toc_structure(toc_files[0], isbn)
        toc_html = _transform_toc(toc_files[0], isbn)

    sections = _get_sections(isbn)
    return render_template("toc.html", isbn=isbn, toc_html=toc_html,
                           sections=sections, book_meta=book_meta, toc_data=toc_data)


@viewer_bp.route("/viewer/<isbn>/<section>")
@viewer_bp.route("/resource/detail/<isbn>/<section>")
def section_view(isbn, section):
    """Display a single section."""
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn

    if not book_dir.exists():
        abort(404)

    # Determine XML filename from section code
    xml_file = _resolve_section_file(book_dir, isbn, section)
    if xml_file is None or not xml_file.exists():
        abort(404)

    content_html = _transform_content(xml_file, isbn, section)
    sections = _get_sections(isbn)

    # Find prev/next sections
    section_ids = [s["id"] for s in sections]
    current_idx = section_ids.index(section) if section in section_ids else -1
    prev_section = section_ids[current_idx - 1] if current_idx > 0 else None
    next_section = section_ids[current_idx + 1] if 0 <= current_idx < len(section_ids) - 1 else None

    return render_template(
        "section.html",
        isbn=isbn,
        section=section,
        content_html=content_html,
        sections=sections,
        prev_section=prev_section,
        next_section=next_section,
    )


@viewer_bp.route("/resource/title/<isbn>")
def title_page(isbn):
    """Redirect title page to TOC."""
    from flask import redirect, url_for
    return redirect(url_for("viewer.book_toc", isbn=isbn))


@viewer_bp.route("/images/<isbn>/<path:filename>")
def serve_image(isbn, filename):
    """Serve images from the content directory."""
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    img_dir = content_dir / isbn
    if not img_dir.exists():
        abort(404)
    return send_from_directory(str(img_dir), filename)


@viewer_bp.route("/static/r2css/<path:filename>")
def serve_r2_css(filename):
    """Serve R2Library CSS files."""
    if CSS_DIR and CSS_DIR.exists():
        return send_from_directory(str(CSS_DIR), filename)
    abort(404)


# ---------------------------------------------------------------------------
# Download & Replace routes
# ---------------------------------------------------------------------------

IMAGE_EXTS = {'.jpg', '.jpeg', '.png', '.gif', '.bmp', '.svg'}


def _files_for_type(book_dir, folder_type):
    """Return list of Path objects matching folder_type filter."""
    files = [f for f in sorted(book_dir.iterdir()) if f.is_file()]
    if folder_type == "xml":
        return [f for f in files if f.suffix.lower() not in IMAGE_EXTS]
    elif folder_type == "images":
        return [f for f in files if f.suffix.lower() in IMAGE_EXTS]
    return files  # "all"


@viewer_bp.route("/download/<isbn>/<folder_type>")
def download_files(isbn, folder_type):
    """Download book files as ZIP. folder_type: all, xml, or images."""
    if folder_type not in ("all", "xml", "images"):
        abort(400)
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn
    if not book_dir.exists():
        abort(404)

    files = _files_for_type(book_dir, folder_type)
    buf = io.BytesIO()
    with zipfile.ZipFile(buf, "w", zipfile.ZIP_DEFLATED) as zf:
        for f in files:
            zf.write(f, f.name)
    buf.seek(0)
    zip_name = f"{isbn}_{folder_type}.zip"
    return send_file(buf, mimetype="application/zip", as_attachment=True,
                     download_name=zip_name)


@viewer_bp.route("/replace/<isbn>/<folder_type>", methods=["POST"])
def replace_files(isbn, folder_type):
    """Replace book files from uploaded ZIP. folder_type: all, xml, or images."""
    if folder_type not in ("all", "xml", "images"):
        return jsonify({"error": "Invalid folder type"}), 400
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn
    if not book_dir.exists():
        return jsonify({"error": "Book not found"}), 404

    uploaded = request.files.get("file")
    if not uploaded or not uploaded.filename.lower().endswith(".zip"):
        return jsonify({"error": "Please upload a .zip file"}), 400

    # Delete existing files matching the folder type
    old_files = _files_for_type(book_dir, folder_type)
    for f in old_files:
        f.unlink()

    # Extract new files from ZIP
    zip_data = io.BytesIO(uploaded.read())
    with zipfile.ZipFile(zip_data, "r") as zf:
        for member in zf.namelist():
            # Skip directories and hidden/system files
            basename = Path(member).name
            if not basename or basename.startswith("."):
                continue
            # For xml/images type, only extract matching files
            ext = Path(basename).suffix.lower()
            if folder_type == "xml" and ext in IMAGE_EXTS:
                continue
            if folder_type == "images" and ext not in IMAGE_EXTS:
                continue
            # Extract flat into book_dir (ignore ZIP subdirectories)
            target = book_dir / basename
            target.write_bytes(zf.read(member))

    return jsonify({"ok": True, "message": f"Replaced {folder_type} files for {isbn}"})


@viewer_bp.route("/download-file/<isbn>/<filename>")
def download_single_file(isbn, filename):
    """Download a single file from the book directory."""
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn
    filepath = book_dir / filename
    if not book_dir.exists() or not filepath.exists():
        abort(404)
    return send_from_directory(str(book_dir), filename, as_attachment=True)


@viewer_bp.route("/replace-file/<isbn>/<filename>", methods=["POST"])
def replace_single_file(isbn, filename):
    """Replace a single file in the book directory."""
    content_dir = Path(current_app.config.get("CONTENT_DIR", CONTENT_DIR))
    book_dir = content_dir / isbn
    if not book_dir.exists():
        return jsonify({"error": "Book not found"}), 404

    uploaded = request.files.get("file")
    if not uploaded:
        return jsonify({"error": "No file uploaded"}), 400

    # Save uploaded file, using the original filename (not the uploaded name)
    target = book_dir / filename
    uploaded.save(str(target))
    return jsonify({"ok": True, "message": f"Replaced {filename}"})


def _resolve_section_file(book_dir, isbn, section):
    """Resolve section code to XML file path."""
    prefix = section[:2] if len(section) >= 2 else ""

    if prefix == "ap":
        candidates = [
            book_dir / f"appendix.{isbn}.{section}.xml",
            book_dir / f"sect1.{isbn}.{section}.xml",
        ]
    elif prefix in ("dd", "de"):
        candidates = [
            book_dir / f"dedication.{isbn}.{section}.xml",
            book_dir / f"sect1.{isbn}.{section}.xml",
        ]
    elif prefix == "pr":
        candidates = [
            book_dir / f"preface.{isbn}.{section}.xml",
            book_dir / f"sect1.{isbn}.{section}.xml",
        ]
    elif prefix == "gl":
        candidates = [
            book_dir / f"book.{isbn}.xml",
            book_dir / f"sect1.{isbn}.{section}.xml",
        ]
    elif prefix == "pt":
        candidates = [
            book_dir / f"book.{isbn}.xml",
            book_dir / f"sect1.{isbn}.{section}.xml",
        ]
    elif prefix == "bi":
        candidates = [
            book_dir / f"sect1.{isbn}.{section}.xml",
            book_dir / f"book.{isbn}.xml",
        ]
    else:
        candidates = [
            book_dir / f"sect1.{isbn}.{section}.xml",
        ]

    for c in candidates:
        if c.exists():
            return c

    # Fallback: glob for any file containing the section id
    matches = list(book_dir.glob(f"*.{isbn}.{section}.xml"))
    if matches:
        return matches[0]

    return None
