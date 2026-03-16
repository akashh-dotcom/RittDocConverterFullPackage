"""
EPUB Conversion Pipeline
========================

Orchestrates three independent codebases into a single pipeline:
  1. RittDocConverter  – EPUB → DocBook XML
  2. Bookloader        – DocBook XML → sectioned XML files
  3. R2Library Viewer   – sectioned XML → rendered HTML (browser)

Usage:
    python pipeline.py                   # Start web interface on port 8080
    python pipeline.py --port 9000       # Custom port
    python pipeline.py --cli input.epub  # CLI mode (process and start viewer)
"""

import os
import sys
import uuid
import shutil
import zipfile
import subprocess
import threading
import argparse
import glob as globmod
import re
from pathlib import Path

from flask import Flask, request, jsonify, render_template

# ---------------------------------------------------------------------------
# Path detection
# ---------------------------------------------------------------------------
SCRIPT_DIR = Path(__file__).resolve().parent


def _find_repo_root():
    """Find the main repository root (handles git worktree case)."""
    # If script is at the repo root, SCRIPT_DIR already is the repo root
    # If inside a worktree (.claude/worktrees/*/), go up to the real repo root
    d = SCRIPT_DIR
    for _ in range(10):
        # Check if this looks like the repo root (has the three codebases)
        has_rittdoc = any((d / p).exists() for p in [
            "RittDocConverter-main (1)", "RittDocConverter-main"
        ])
        has_bookloader = any((d / p).exists() for p in [
            "Bookloader-main (2)", "Bookloader-main"
        ])
        if has_rittdoc or has_bookloader:
            return d
        d = d.parent
    return SCRIPT_DIR


REPO_ROOT = _find_repo_root()


def _find_dir(patterns):
    """Find first existing directory matching any of the patterns."""
    # Search both SCRIPT_DIR and REPO_ROOT
    for base in [SCRIPT_DIR, REPO_ROOT]:
        for p in patterns:
            path = base / p
            if path.exists():
                return path
    return None


RITTDOC_DIR = _find_dir([
    "RittDocConverter-main (1)/RittDocConverter-main",
    "RittDocConverter-main/RittDocConverter-main",
    "RittDocConverter-main (1)",
    "RittDocConverter-main",
])

BOOKLOADER_DIR = _find_dir([
    "Bookloader-main (2)/Bookloader-main",
    "Bookloader-main/Bookloader-main",
    "Bookloader-main (2)",
    "Bookloader-main",
])

R2LIBRARY_DIR = _find_dir([
    "R2Library-main/R2Library-main/src/R2V2.Web",
    "R2Library-main/src/R2V2.Web",
])

CONTENT_DIR = SCRIPT_DIR / "viewer" / "content"
UPLOAD_DIR = SCRIPT_DIR / "uploads"

# ---------------------------------------------------------------------------
# Job tracking
# ---------------------------------------------------------------------------
jobs = {}  # job_id -> dict with status, progress, stage, log, isbn, error

def _log(job_id, msg):
    """Append a log message to a job."""
    if job_id in jobs:
        jobs[job_id]["log"] += msg + "\n"
    print(msg)


# ---------------------------------------------------------------------------
# Stage 1: RittDocConverter
# ---------------------------------------------------------------------------
def run_rittdoc_converter(job_id, epub_path):
    """Run RittDocConverter to convert EPUB → DocBook XML."""
    _log(job_id, "[Stage 1] Starting RittDocConverter...")
    jobs[job_id]["stage"] = "Stage 1: RittDocConverter (EPUB → XML)"
    jobs[job_id]["progress"] = 10

    if RITTDOC_DIR is None:
        raise RuntimeError("RittDocConverter directory not found")

    pipeline_script = RITTDOC_DIR / "epub_pipeline.py"
    if not pipeline_script.exists():
        raise RuntimeError(f"epub_pipeline.py not found at {pipeline_script}")

    output_dir = RITTDOC_DIR / "Output"
    output_dir.mkdir(exist_ok=True)

    # Clean old output ZIPs to avoid picking up stale results
    for old_zip in output_dir.glob("*.zip"):
        old_zip.unlink()

    # Find the right Python executable
    python_exe = sys.executable

    cmd = [
        python_exe,
        str(pipeline_script),
        str(epub_path),
        str(output_dir),
        "--no-interactive",
    ]

    _log(job_id, f"  Running: {' '.join(cmd)}")

    proc = subprocess.run(
        cmd,
        cwd=str(RITTDOC_DIR),
        capture_output=True,
        text=True,
        timeout=600,
    )

    if proc.stdout:
        # Log last 20 lines to avoid flooding
        lines = proc.stdout.strip().split("\n")
        for line in lines[-20:]:
            _log(job_id, f"  [rittdoc] {line}")

    if proc.returncode != 0:
        _log(job_id, f"  [rittdoc] STDERR: {proc.stderr[-500:] if proc.stderr else 'none'}")
        raise RuntimeError(f"RittDocConverter failed (exit {proc.returncode})")

    jobs[job_id]["progress"] = 30

    # Find the output ZIP (prefer _all_fixes.zip)
    zip_files = sorted(output_dir.glob("*_all_fixes.zip"), key=os.path.getmtime, reverse=True)
    if not zip_files:
        zip_files = sorted(output_dir.glob("*.zip"), key=os.path.getmtime, reverse=True)
    if not zip_files:
        raise RuntimeError(f"No output ZIP found in {output_dir}")

    result_zip = zip_files[0]
    _log(job_id, f"  Output ZIP: {result_zip.name}")
    return result_zip


# ---------------------------------------------------------------------------
# Stage 2: Bookloader
# ---------------------------------------------------------------------------
def _ensure_bookloader_built():
    """Build Bookloader JAR if not already built."""
    if BOOKLOADER_DIR is None:
        raise RuntimeError("Bookloader directory not found")

    jar_path = BOOKLOADER_DIR / "build" / "RISBackend.jar"
    if jar_path.exists():
        return jar_path

    # Build with javac directly (handles UTF-8 encoding)
    build_dir = BOOKLOADER_DIR / "build" / "classes"
    build_dir.mkdir(parents=True, exist_ok=True)

    # Collect classpath JARs
    cp_parts = [
        "lib/jdom.jar",
        "lib/jdbc/Opta2000.jar",
        "lib/jakarta/commons-lang-2.0.jar",
        "lib/jakarta/commons-collections-3.1.jar",
        "lib/jakarta/commons-pool-1.2.jar",
        "lib/jakarta/commons-dbcp-1.2.1.jar",
        "lib/jakarta/commons-httpclient-2.0.2.jar",
        "lib/saxon/saxon.jar",
        "lib/log4j.jar",
        "lib/xalan/xalan.jar",
        "lib/xerces/xercesImpl.jar",
        "lib/xerces/xml-apis.jar",
        "lib/textml/textmlserver.jar",
        "lib/textml/textmlserverrmi.jar",
        "lib/textml/textmlserverrmiInterfaces.jar",
        "lib/textml/textmlserverrmiStub.jar",
        "lib/DoctypeChanger.jar",
        "lib/concurrent.jar",
    ]
    classpath = ";".join(str(BOOKLOADER_DIR / p) for p in cp_parts)

    # Find all Java source files (excluding test)
    src_dir = BOOKLOADER_DIR / "src"
    java_files = []
    for root, dirs, files in os.walk(src_dir):
        # Skip test directories
        if "test" in Path(root).parts:
            continue
        for f in files:
            if f.endswith(".java"):
                java_files.append(str(Path(root) / f))

    if not java_files:
        raise RuntimeError("No Java source files found in Bookloader")

    cmd = [
        "javac",
        "-encoding", "UTF-8",
        "-d", str(build_dir),
        "-cp", classpath,
    ] + java_files

    proc = subprocess.run(cmd, cwd=str(BOOKLOADER_DIR), capture_output=True, text=True, timeout=120)
    if proc.returncode != 0:
        raise RuntimeError(f"Bookloader compilation failed: {proc.stderr[-500:]}")

    # Package into JAR
    jar_path = BOOKLOADER_DIR / "build" / "RISBackend.jar"
    cmd = ["jar", "cf", str(jar_path), "-C", str(build_dir), "."]
    proc = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
    if proc.returncode != 0:
        raise RuntimeError(f"JAR packaging failed: {proc.stderr}")

    return jar_path


def run_bookloader(job_id, xml_dir, isbn):
    """Run Bookloader to split XML into sections."""
    _log(job_id, "[Stage 2] Starting Bookloader...")
    jobs[job_id]["stage"] = "Stage 2: Bookloader (XML → Sections)"
    jobs[job_id]["progress"] = 40

    if BOOKLOADER_DIR is None:
        raise RuntimeError("Bookloader directory not found")

    jar_path = _ensure_bookloader_built()
    _log(job_id, f"  Bookloader JAR: {jar_path}")

    # Prepare Bookloader directories
    bl_input = BOOKLOADER_DIR / "test" / "input"
    bl_output = BOOKLOADER_DIR / "test" / "output"
    bl_temp = BOOKLOADER_DIR / "test" / "temp"
    bl_media = BOOKLOADER_DIR / "test" / "media"

    for d in [bl_input, bl_output, bl_temp, bl_media]:
        d.mkdir(parents=True, exist_ok=True)

    # Clean previous input/output
    for f in bl_input.iterdir():
        if f.is_file():
            f.unlink()
    for f in bl_output.iterdir():
        if f.is_file():
            f.unlink()
    for f in bl_temp.iterdir():
        if f.is_file():
            f.unlink()

    # Copy XML files from extracted ZIP to Bookloader input
    copied = 0
    for f in Path(xml_dir).iterdir():
        if f.suffix.lower() == ".xml":
            shutil.copy2(str(f), str(bl_input / f.name))
            copied += 1
    # Also copy images to media dir
    img_dirs = [Path(xml_dir) / "Images", Path(xml_dir) / "images", Path(xml_dir) / "Media"]
    for img_dir in img_dirs:
        if img_dir.exists():
            for img in img_dir.iterdir():
                if img.is_file():
                    shutil.copy2(str(img), str(bl_media / img.name))

    _log(job_id, f"  Copied {copied} XML files to Bookloader input")

    if copied == 0:
        raise RuntimeError("No XML files found to process")

    jobs[job_id]["progress"] = 50

    # Build classpath for running Bookloader
    cp_parts = [str(jar_path)]
    for p in [
        "lib/jdom.jar",
        "lib/jdbc/Opta2000.jar",
        "lib/jakarta/commons-lang-2.0.jar",
        "lib/jakarta/commons-collections-3.1.jar",
        "lib/jakarta/commons-pool-1.2.jar",
        "lib/jakarta/commons-dbcp-1.2.1.jar",
        "lib/jakarta/commons-httpclient-2.0.2.jar",
        "lib/saxon/saxon.jar",
        "lib/log4j.jar",
        "lib/xalan/xalan.jar",
        "lib/xerces/xercesImpl.jar",
        "lib/xerces/xml-apis.jar",
        "lib/textml/textmlserver.jar",
        "lib/textml/textmlserverrmi.jar",
        "lib/textml/textmlserverrmiInterfaces.jar",
        "lib/textml/textmlserverrmiStub.jar",
        "lib/DoctypeChanger.jar",
        "lib/concurrent.jar",
    ]:
        cp_parts.append(str(BOOKLOADER_DIR / p))

    classpath = ";".join(cp_parts)

    cmd = [
        "java",
        "-cp", classpath,
        "com.rittenhouse.RIS.Main",
        "--noDB",
        "--skipLinks",
    ]

    _log(job_id, f"  Running Bookloader with --noDB --skipLinks")

    proc = subprocess.run(
        cmd,
        cwd=str(BOOKLOADER_DIR),
        capture_output=True,
        text=True,
        timeout=600,
    )

    if proc.stdout:
        lines = proc.stdout.strip().split("\n")
        for line in lines[-20:]:
            _log(job_id, f"  [bookloader] {line}")
    if proc.stderr:
        lines = proc.stderr.strip().split("\n")
        for line in lines[-10:]:
            _log(job_id, f"  [bookloader:err] {line}")

    jobs[job_id]["progress"] = 70

    # Check output
    output_files = list(bl_output.glob("*.xml"))
    if not output_files:
        # Also check temp dir in case files ended up there
        output_files = list(bl_temp.glob("*.xml"))
        if output_files:
            bl_output = bl_temp

    _log(job_id, f"  Bookloader produced {len(output_files)} files")

    if not output_files:
        _log(job_id, "  WARNING: Bookloader produced no output files.")
        _log(job_id, "  Falling back to using RittDocConverter XML directly.")
        return xml_dir, isbn

    # Detect Bookloader's internal ISBN from output filenames
    bl_isbn = isbn
    for f in output_files:
        m = re.match(r'(?:sect1|book|toc|preface)\.([\w]+)\.', f.name)
        if m:
            bl_isbn = m.group(1)
            break

    if bl_isbn != isbn:
        _log(job_id, f"  Bookloader internal ISBN: {bl_isbn} (real: {isbn})")

    # Fix empty sect1 files (Bookloader bug: RISChunker depends on sect1info
    # elements that AddRISInfo may not have inserted)
    _fix_bookloader_output(job_id, bl_output, bl_temp, bl_isbn)

    return str(bl_output), isbn


def _fix_bookloader_output(job_id, output_dir, temp_dir, isbn):
    """Fix empty sect1 files produced by Bookloader.

    The Bookloader's RISChunker.xsl uses ``./sect1info/following-sibling::*``
    to copy section content, but AddRISInfo may not insert ``sect1info``,
    leaving output sect1 files empty. Re-extracts from the resolved temp book XML.
    """
    from lxml import etree

    sect_files = sorted(output_dir.glob("sect1.*.xml"))
    if not sect_files:
        return

    nonempty = sum(1 for f in sect_files if f.stat().st_size > 200)
    if nonempty > 0:
        _log(job_id, f"  sect1 files look fine ({nonempty}/{len(sect_files)} have content)")
        return

    _log(job_id, f"  Detected {len(sect_files)} empty sect1 files - re-extracting from temp book XML")

    book_xmls = sorted(temp_dir.glob("book.*.xml"))
    if not book_xmls:
        _log(job_id, "  WARNING: No temp book XML found; cannot fix empty sections")
        return

    parser = etree.XMLParser(recover=True, dtd_validation=False, load_dtd=False,
                             no_network=True, huge_tree=True)
    try:
        tree = etree.parse(str(book_xmls[0]), parser)
    except Exception as e:
        _log(job_id, f"  WARNING: Failed to parse temp book XML: {e}")
        return

    root = tree.getroot()
    fixed = 0

    for sect1 in root.xpath("//sect1"):
        sect1_id = sect1.get("id", "")
        if not sect1_id:
            continue
        target = output_dir / f"sect1.{isbn}.{sect1_id}.xml"
        if not target.exists():
            continue
        if target.stat().st_size > 200:
            continue
        content = etree.tostring(sect1, xml_declaration=True, encoding="UTF-8",
                                 pretty_print=True)
        target.write_bytes(content)
        fixed += 1

    _log(job_id, f"  Fixed {fixed} sect1 files with content from temp book XML")

    # Generate TOC XML if missing
    toc_file = output_dir / f"toc.{isbn}.xml"
    if not toc_file.exists():
        _generate_toc_xml(job_id, root, isbn, toc_file)


def _generate_toc_xml(job_id, book_root, isbn, toc_path):
    """Generate a TOC XML file from the book structure."""
    from lxml import etree

    toc = etree.Element("toc")
    toc.set("id", f"toc.{isbn}")

    bookinfo = book_root.find(".//bookinfo")
    if bookinfo is not None:
        title_el = bookinfo.find("title")
        if title_el is not None and title_el.text:
            toc_title = etree.SubElement(toc, "title")
            toc_title.text = title_el.text

    for preface in book_root.findall(".//preface"):
        pref_id = preface.get("id", "")
        title = preface.findtext("title") or "Preface"
        entry = etree.SubElement(toc, "tocentry")
        entry.set("linkend", pref_id)
        entry_title = etree.SubElement(entry, "title")
        entry_title.text = title

    for chapter in book_root.findall(".//chapter"):
        ch_id = chapter.get("id", "")
        ch_label = chapter.get("label", "")
        ch_title = chapter.findtext("title") or f"Chapter {ch_label}"
        ch_entry = etree.SubElement(toc, "tocchap")
        ch_entry.set("linkend", ch_id)
        ch_entry.set("label", ch_label)
        ch_entry_title = etree.SubElement(ch_entry, "title")
        ch_entry_title.text = ch_title
        for sect1 in chapter.findall("sect1"):
            s1_id = sect1.get("id", "")
            s1_title = sect1.findtext("title") or s1_id
            s1_entry = etree.SubElement(ch_entry, "tocsect1")
            s1_entry.set("linkend", s1_id)
            s1_entry_title = etree.SubElement(s1_entry, "title")
            s1_entry_title.text = s1_title

    content = etree.tostring(toc, xml_declaration=True, encoding="UTF-8", pretty_print=True)
    toc_path.write_bytes(content)
    _log(job_id, f"  Generated TOC file: {toc_path.name}")


# ---------------------------------------------------------------------------
# Stage 3: Copy to viewer content
# ---------------------------------------------------------------------------
def setup_viewer_content(job_id, sections_dir, isbn, original_xml_dir=None):
    """Copy sectioned XMLs and images to the viewer content directory."""
    _log(job_id, "[Stage 3] Setting up R2Library viewer content...")
    jobs[job_id]["stage"] = "Stage 3: Setting up viewer"
    jobs[job_id]["progress"] = 80

    target_dir = CONTENT_DIR / isbn
    target_dir.mkdir(parents=True, exist_ok=True)

    # Detect Bookloader's internal ISBN from filenames (may differ from real ISBN)
    bl_isbn = None
    sections_path = Path(sections_dir)
    for f in sections_path.iterdir():
        if f.suffix.lower() == ".xml":
            m = re.match(r'(?:sect1|book|toc|preface)\.([\w]+)\.', f.name)
            if m:
                bl_isbn = m.group(1)
                break

    # Copy section XML files, renaming from internal ISBN to real ISBN
    copied = 0
    for f in sections_path.iterdir():
        if f.suffix.lower() == ".xml":
            dest_name = f.name
            if bl_isbn and bl_isbn != isbn and bl_isbn in dest_name:
                dest_name = dest_name.replace(bl_isbn, isbn)
            shutil.copy2(str(f), str(target_dir / dest_name))
            copied += 1

    # Also copy TOC from Bookloader output/temp if not already copied
    if BOOKLOADER_DIR:
        for toc_src in [BOOKLOADER_DIR / "test" / "output", BOOKLOADER_DIR / "test" / "temp"]:
            for toc_f in toc_src.glob("toc.*.xml"):
                dest_name = toc_f.name
                if bl_isbn and bl_isbn != isbn and bl_isbn in dest_name:
                    dest_name = dest_name.replace(bl_isbn, isbn)
                if not (target_dir / dest_name).exists():
                    shutil.copy2(str(toc_f), str(target_dir / dest_name))
                    copied += 1

    _log(job_id, f"  Copied {copied} XML files to viewer/content/{isbn}/")

    # Copy images from various possible locations
    image_sources = [
        Path(original_xml_dir) / "Images" if original_xml_dir else None,
        Path(original_xml_dir) / "images" if original_xml_dir else None,
        Path(original_xml_dir) / "MultiMedia" if original_xml_dir else None,
        Path(original_xml_dir) / "multimedia" if original_xml_dir else None,
        Path(original_xml_dir) / "Media" if original_xml_dir else None,
        BOOKLOADER_DIR / "test" / "media" if BOOKLOADER_DIR else None,
    ]

    img_copied = 0
    for img_src in image_sources:
        if img_src and img_src.exists():
            for img in img_src.iterdir():
                if img.is_file():
                    shutil.copy2(str(img), str(target_dir / img.name))
                    img_copied += 1

    _log(job_id, f"  Copied {img_copied} image files")
    jobs[job_id]["progress"] = 90

    return str(target_dir)


# ---------------------------------------------------------------------------
# Full pipeline
# ---------------------------------------------------------------------------
def run_pipeline(job_id, epub_path):
    """Execute the full EPUB → viewer pipeline."""
    try:
        jobs[job_id]["status"] = "running"

        # Stage 1: RittDocConverter
        result_zip = run_rittdoc_converter(job_id, epub_path)

        # Extract ZIP to a temp dir
        extract_dir = UPLOAD_DIR / f"extracted_{job_id}"
        extract_dir.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(str(result_zip), "r") as zf:
            zf.extractall(str(extract_dir))

        _log(job_id, f"  Extracted ZIP to {extract_dir}")

        # Find extracted XML files (may be in a subdirectory)
        xml_dir = str(extract_dir)
        # Check if files are in a subdirectory
        subdirs = [d for d in extract_dir.iterdir() if d.is_dir()]
        xml_files = list(extract_dir.glob("*.xml"))
        if not xml_files and subdirs:
            # Files might be in a subdirectory
            for sd in subdirs:
                if list(sd.glob("*.xml")):
                    xml_dir = str(sd)
                    break

        # Detect ISBN from XML filenames or content
        isbn = _detect_isbn(xml_dir, epub_path)
        _log(job_id, f"  Detected ISBN: {isbn}")
        jobs[job_id]["isbn"] = isbn

        # Stage 2: Bookloader
        try:
            sections_dir, isbn = run_bookloader(job_id, xml_dir, isbn)
        except Exception as e:
            _log(job_id, f"  Bookloader error: {e}")
            _log(job_id, "  Falling back to RittDocConverter output directly")
            sections_dir = xml_dir

        # Stage 3: Setup viewer content
        setup_viewer_content(job_id, sections_dir, isbn, original_xml_dir=xml_dir)

        jobs[job_id]["status"] = "complete"
        jobs[job_id]["progress"] = 100
        jobs[job_id]["stage"] = "Complete!"
        _log(job_id, f"\nPipeline complete! View at: /viewer/{isbn}/")

    except Exception as e:
        jobs[job_id]["status"] = "error"
        jobs[job_id]["error"] = str(e)
        _log(job_id, f"\nERROR: {e}")


def _detect_isbn(xml_dir, epub_path):
    """Detect ISBN, preferring the EPUB filename over RittDocConverter's internal ID."""
    # 1. EPUB filename is the most reliable source (publishers name files by ISBN)
    epub_stem = Path(epub_path).stem.replace(" ", "").replace("-", "")
    # Strip the job_id prefix added during upload (e.g. "aa2a320c_9783031866173")
    if "_" in epub_stem:
        epub_stem = epub_stem.split("_", 1)[1].replace(" ", "").replace("-", "")
    isbn_match = re.search(r'(97[89]\d{10}|\d{10})', epub_stem)
    if isbn_match:
        return isbn_match.group(1)

    # 2. Fall back to Book.XML <isbn> element (may be RittDocConverter's internal ID)
    xml_dir_path = Path(xml_dir)
    book_xml = xml_dir_path / "Book.XML"
    if not book_xml.exists():
        book_xml = xml_dir_path / "Book.xml"
    if not book_xml.exists():
        candidates = list(xml_dir_path.glob("book*.xml")) + list(xml_dir_path.glob("Book*.xml"))
        if candidates:
            book_xml = candidates[0]

    if book_xml.exists():
        try:
            from lxml import etree
            parser = etree.XMLParser(recover=True, dtd_validation=False)
            tree = etree.parse(str(book_xml), parser)
            isbn_el = tree.find(".//isbn")
            if isbn_el is not None and isbn_el.text:
                return isbn_el.text.strip().replace("-", "")
        except Exception:
            pass

    # 3. Fallback: use epub filename as-is
    return Path(epub_path).stem.replace(" ", "_")


# ---------------------------------------------------------------------------
# Flask app
# ---------------------------------------------------------------------------
def create_app():
    """Create and configure the Flask application."""
    app = Flask(
        __name__,
        template_folder=str(SCRIPT_DIR / "viewer" / "templates"),
    )

    app.config["CONTENT_DIR"] = str(CONTENT_DIR)
    UPLOAD_DIR.mkdir(exist_ok=True)
    CONTENT_DIR.mkdir(exist_ok=True)

    # Register viewer blueprint
    from viewer.app import viewer_bp, init_viewer
    app.register_blueprint(viewer_bp)
    init_viewer(app)

    @app.route("/upload")
    def upload_page():
        return render_template("upload.html")

    @app.route("/api/process", methods=["POST"])
    def api_process():
        if "epub" not in request.files:
            return jsonify({"error": "No EPUB file provided"}), 400

        epub_file = request.files["epub"]
        if epub_file.filename == "":
            return jsonify({"error": "No file selected"}), 400

        if not epub_file.filename.lower().endswith((".epub", ".epub3")):
            return jsonify({"error": "File must be .epub or .epub3"}), 400

        # Save uploaded file
        job_id = str(uuid.uuid4())[:8]
        epub_path = UPLOAD_DIR / f"{job_id}_{epub_file.filename}"
        epub_file.save(str(epub_path))

        # Initialize job
        jobs[job_id] = {
            "status": "queued",
            "progress": 0,
            "stage": "Queued",
            "log": "",
            "isbn": None,
            "error": None,
        }

        # Start processing in background thread
        thread = threading.Thread(target=run_pipeline, args=(job_id, str(epub_path)))
        thread.daemon = True
        thread.start()

        return jsonify({"job_id": job_id})

    @app.route("/api/status/<job_id>")
    def api_status(job_id):
        if job_id not in jobs:
            return jsonify({"error": "Job not found"}), 404
        return jsonify(jobs[job_id])

    @app.route("/api/upload-converted", methods=["POST"])
    def api_upload_converted():
        """Upload pre-converted files (ZIP) directly into viewer content."""
        uploaded = request.files.get("zipfile")
        if not uploaded or not uploaded.filename.lower().endswith(".zip"):
            return jsonify({"error": "Please upload a .zip file"}), 400

        isbn = request.form.get("isbn", "").strip()
        if not isbn:
            # Try to detect ISBN from ZIP filename
            stem = Path(uploaded.filename).stem.replace(" ", "").replace("-", "")
            m = re.search(r'(97[89]\d{10}|\d{10,13})', stem)
            if m:
                isbn = m.group(1)
            else:
                isbn = stem  # Use filename as folder name

        content_dir = CONTENT_DIR / isbn
        content_dir.mkdir(parents=True, exist_ok=True)

        import io
        zip_data = io.BytesIO(uploaded.read())
        count = 0
        with zipfile.ZipFile(zip_data, "r") as zf:
            for member in zf.namelist():
                basename = Path(member).name
                if not basename or basename.startswith("."):
                    continue
                target = content_dir / basename
                target.write_bytes(zf.read(member))
                count += 1

        return jsonify({
            "ok": True,
            "isbn": isbn,
            "files": count,
            "message": f"Uploaded {count} files to {isbn}",
        })

    return app


# ---------------------------------------------------------------------------
# CLI mode
# ---------------------------------------------------------------------------
def cli_process(epub_path, port):
    """Process EPUB via CLI and start viewer."""
    epub_path = Path(epub_path).resolve()
    if not epub_path.exists():
        print(f"Error: File not found: {epub_path}")
        sys.exit(1)

    job_id = "cli"
    jobs[job_id] = {
        "status": "running",
        "progress": 0,
        "stage": "",
        "log": "",
        "isbn": None,
        "error": None,
    }

    run_pipeline(job_id, str(epub_path))

    if jobs[job_id]["status"] == "error":
        print(f"\nPipeline failed: {jobs[job_id]['error']}")
        sys.exit(1)

    isbn = jobs[job_id]["isbn"]
    print(f"\nStarting viewer at http://localhost:{port}/viewer/{isbn}/")

    app = create_app()
    app.run(host="0.0.0.0", port=port, debug=False)


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="EPUB Conversion Pipeline")
    parser.add_argument("--cli", metavar="EPUB_FILE", help="Process a specific EPUB file (CLI mode)")
    parser.add_argument("--port", type=int, default=8080, help="Port for web server (default: 8080)")
    args = parser.parse_args()

    # Validate paths
    missing = []
    if RITTDOC_DIR is None:
        missing.append("RittDocConverter")
    if BOOKLOADER_DIR is None:
        missing.append("Bookloader")
    if missing:
        print(f"WARNING: Could not find: {', '.join(missing)}")
        print(f"Expected them as sibling directories to {SCRIPT_DIR}")
        print("The pipeline may not work fully without these.\n")

    if args.cli:
        cli_process(args.cli, args.port)
    else:
        print(f"Starting EPUB Pipeline web interface...")
        print(f"  Upload page: http://localhost:{args.port}/upload")
        print(f"  Books list:  http://localhost:{args.port}/")
        print(f"\n  RittDocConverter: {RITTDOC_DIR or 'NOT FOUND'}")
        print(f"  Bookloader:       {BOOKLOADER_DIR or 'NOT FOUND'}")
        print(f"  R2Library XSL:    {R2LIBRARY_DIR or 'NOT FOUND'}")
        print()

        app = create_app()
        app.run(host="0.0.0.0", port=args.port, debug=True)
