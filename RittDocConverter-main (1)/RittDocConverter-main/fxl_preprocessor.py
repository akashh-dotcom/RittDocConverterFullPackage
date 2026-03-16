"""Fixed-Layout EPUB Preprocessor.

Detects fixed-layout (FXL) EPUBs and converts them to reflowable format
before the main epub_to_structured_v2 pipeline processes them.

Supports all major FXL EPUB variants:
- Adobe InDesign FXL exports (_idGen* classes, scale transforms)
- Apple iBooks Author (-ibooks-* properties)
- Calibre FXL (clean CSS positioning)
- Generic EPUB3 pre-paginated (rendition:layout)
- Any tool producing absolutely-positioned text spans
"""

import html
import logging
import re
import shutil
import zipfile
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from xml.etree import ElementTree as ET

import warnings

from bs4 import BeautifulSoup, Tag, XMLParsedAsHTMLWarning

warnings.filterwarnings("ignore", category=XMLParsedAsHTMLWarning)

logger = logging.getLogger(__name__)

# ---------------------------------------------------------------------------
# Data classes
# ---------------------------------------------------------------------------

EPUB_NS = "http://www.idpf.org/2007/ops"
OPF_NS = "http://www.idpf.org/2007/opf"
DC_NS = "http://purl.org/dc/elements/1.1/"
CONTAINER_NS = "urn:oasis:names:tc:opendocument:xmlns:container"


@dataclass
class FXLDetectionResult:
    is_fixed_layout: bool
    confidence: float = 0.0
    viewport_width: int = 0
    viewport_height: int = 0
    scale_factor: float = 1.0
    page_count: int = 0
    source_tool: str = "unknown"
    signals: List[str] = field(default_factory=list)


@dataclass
class FXLSpan:
    text: str
    top: float
    left: float
    is_bold: bool = False
    is_italic: bool = False
    font_size: float = 0.0


@dataclass
class FXLImage:
    src: str
    width: float = 0.0
    height: float = 0.0
    alt: str = ""
    top: float = 0.0
    left: float = 0.0
    keep: bool = True


@dataclass
class FXLParagraph:
    css_class: str
    full_text: str
    spans: List[FXLSpan] = field(default_factory=list)
    role: str = "body"
    heading_level: int = 0
    is_bold: bool = False
    is_italic: bool = False
    top: float = 0.0
    images: List[FXLImage] = field(default_factory=list)


@dataclass
class FXLPage:
    spine_index: int
    filename: str
    page_number: str = ""
    running_header: str = ""
    paragraphs: List[FXLParagraph] = field(default_factory=list)
    images: List[FXLImage] = field(default_factory=list)
    is_chapter_start: bool = False
    chapter_title: str = ""
    chapter_number: str = ""


@dataclass
class ChapterRange:
    start_idx: int
    end_idx: int  # exclusive
    title: str
    element_type: str = "chapter"
    toc_children: List[Tuple[str, str]] = field(default_factory=list)


@dataclass
class AssembledChapter:
    title: str
    element_type: str
    pages: List[FXLPage]
    paragraphs: List[FXLParagraph]
    images: List[FXLImage]
    page_range: Tuple[int, int] = (0, 0)


# ---------------------------------------------------------------------------
# Phase 1 — Detection
# ---------------------------------------------------------------------------

def detect_fixed_layout(epub_path: Path) -> FXLDetectionResult:
    """Detect whether an EPUB is fixed-layout. Works with all FXL variants."""
    result = FXLDetectionResult(is_fixed_layout=False)
    signals: List[str] = []

    try:
        with zipfile.ZipFile(epub_path, "r") as zf:
            opf_path = _find_opf_path(zf)
            if not opf_path:
                return result

            opf_content = zf.read(opf_path).decode("utf-8", errors="replace")

            # Signal 1: OPF metadata rendition:layout
            if "pre-paginated" in opf_content:
                signals.append("opf_rendition_layout")

            # Signal 2: OPF fixed-layout meta (EPUB2 variant)
            if 'name="fixed-layout"' in opf_content and 'content="true"' in opf_content:
                signals.append("opf_fixed_layout_meta")

            # Signal 3: Apple iBooks display options
            if "META-INF/com.apple.ibooks.display-options.xml" in zf.namelist():
                try:
                    apple_opts = zf.read("META-INF/com.apple.ibooks.display-options.xml").decode("utf-8", errors="replace")
                    if "fixed-layout" in apple_opts.lower() and "true" in apple_opts.lower():
                        signals.append("apple_ibooks_fixed_layout")
                except Exception:
                    pass

            # Detect source tool from OPF
            source_tool = _detect_source_tool(opf_content, zf)
            result.source_tool = source_tool

            # Get spine XHTML files
            spine_files = _get_spine_xhtml_files(opf_content, opf_path, zf)
            result.page_count = len(spine_files)

            # Sample pages for heuristic detection
            sample_size = min(10, len(spine_files))
            if sample_size > 0:
                viewport_count = 0
                abs_pos_count = 0
                body_dim_count = 0
                scale_factors = []

                for fname in spine_files[:sample_size]:
                    try:
                        content = zf.read(fname).decode("utf-8", errors="replace")
                    except (KeyError, Exception):
                        continue

                    # Viewport meta
                    vp = re.search(
                        r'<meta\s+name=["\']viewport["\']\s+content=["\']width=(\d+)\s*,\s*height=(\d+)',
                        content, re.I,
                    )
                    if vp:
                        viewport_count += 1
                        result.viewport_width = int(vp.group(1))
                        result.viewport_height = int(vp.group(2))

                    # Body fixed dimensions
                    if re.search(r'<body[^>]+style=["\'][^"\']*width:\s*\d+px', content, re.I):
                        body_dim_count += 1

                    # Absolute positioning
                    abs_spans = len(re.findall(r'position:\s*absolute', content, re.I))
                    if abs_spans > 5:
                        abs_pos_count += 1

                    # Scale transforms
                    scales = re.findall(r'scale\(\s*([\d.]+)\s*\)', content)
                    for s in scales:
                        try:
                            sv = float(s)
                            if sv < 0.5:
                                scale_factors.append(sv)
                        except ValueError:
                            pass

                ratio = sample_size
                if viewport_count / ratio > 0.8:
                    signals.append("viewport_meta")
                if body_dim_count / ratio > 0.8:
                    signals.append("body_fixed_dimensions")
                if abs_pos_count / ratio > 0.8:
                    signals.append("absolute_positioned_text")
                if scale_factors:
                    from statistics import median
                    result.scale_factor = median(scale_factors)
                    signals.append("scale_transform")

    except (zipfile.BadZipFile, Exception) as exc:
        logger.warning("FXL detection failed: %s", exc)
        return result

    result.signals = signals
    if signals:
        result.is_fixed_layout = True
        # Confidence based on signal count and type
        authoritative = {"opf_rendition_layout", "opf_fixed_layout_meta", "apple_ibooks_fixed_layout"}
        if authoritative & set(signals):
            result.confidence = 1.0
        else:
            result.confidence = min(1.0, len(signals) * 0.3)

    return result


def _find_opf_path(zf: zipfile.ZipFile) -> Optional[str]:
    try:
        container = zf.read("META-INF/container.xml").decode("utf-8", errors="replace")
        m = re.search(r'full-path=["\']([^"\']+\.opf)', container)
        if m:
            return m.group(1)
    except (KeyError, Exception):
        pass
    # Fallback: find .opf in archive
    for name in zf.namelist():
        if name.endswith(".opf"):
            return name
    return None


def _detect_source_tool(opf_content: str, zf: zipfile.ZipFile) -> str:
    names = zf.namelist()
    names_lower = " ".join(names).lower()
    opf_lower = opf_content.lower()

    if "idgeneratedstyles" in names_lower or "_idgen" in names_lower:
        return "indesign"
    if "indesign" in opf_lower:
        return "indesign"
    if "ibooks" in opf_lower or "com.apple.ibooks" in names_lower:
        return "ibooks"
    if "calibre" in opf_lower:
        return "calibre"
    if "sigil" in opf_lower:
        return "sigil"
    return "unknown"


def _get_spine_xhtml_files(opf_content: str, opf_path: str, zf: zipfile.ZipFile) -> List[str]:
    """Extract ordered list of spine XHTML file paths from OPF."""
    try:
        root = ET.fromstring(opf_content)
    except ET.ParseError:
        return []

    ns = {"opf": OPF_NS, "dc": DC_NS}
    opf_dir = str(Path(opf_path).parent)
    if opf_dir == ".":
        opf_dir = ""

    # Build manifest id->href map
    manifest = {}
    for item in root.iter():
        tag = item.tag.split("}")[-1] if "}" in item.tag else item.tag
        if tag == "item":
            item_id = item.get("id", "")
            href = item.get("href", "")
            media = item.get("media-type", "")
            if "xhtml" in media or "html" in media:
                full_path = f"{opf_dir}/{href}" if opf_dir else href
                manifest[item_id] = full_path

    # Follow spine order
    spine_files = []
    for itemref in root.iter():
        tag = itemref.tag.split("}")[-1] if "}" in itemref.tag else itemref.tag
        if tag == "itemref":
            idref = itemref.get("idref", "")
            if idref in manifest:
                spine_files.append(manifest[idref])

    return spine_files


# ---------------------------------------------------------------------------
# Phase 2 — CSS Parsing
# ---------------------------------------------------------------------------

def parse_fxl_css(css_content: str) -> Dict[str, Dict[str, str]]:
    """Parse CSS into a dict of class_name -> {property: value}."""
    result: Dict[str, Dict[str, str]] = {}
    # Remove comments
    css_clean = re.sub(r'/\*.*?\*/', '', css_content, flags=re.DOTALL)
    # Match rule blocks
    for m in re.finditer(r'([^{}]+)\{([^}]*)\}', css_clean):
        selectors_raw = m.group(1).strip()
        props_raw = m.group(2).strip()
        props = {}
        for prop_m in re.finditer(r'([\w-]+)\s*:\s*([^;]+)', props_raw):
            props[prop_m.group(1).strip().lower()] = prop_m.group(2).strip()

        for sel in selectors_raw.split(","):
            sel = sel.strip()
            # Extract class name
            cls_m = re.search(r'\.([\w-]+)', sel)
            if cls_m:
                cls_name = cls_m.group(1)
                if cls_name not in result:
                    result[cls_name] = {}
                result[cls_name].update(props)
    return result


def parse_fxl_css_ids(css_content: str) -> Dict[str, Dict[str, str]]:
    """Parse CSS ID selectors into a dict of element_id -> {property: value}.

    InDesign FXL EPUBs use ``#_idContainerNNN`` rules in external CSS to
    position figures (images/charts) via CSS ``transform``.  These transforms
    contain the translate/scale needed for correct image placement.
    """
    result: Dict[str, Dict[str, str]] = {}
    css_clean = re.sub(r'/\*.*?\*/', '', css_content, flags=re.DOTALL)
    for m in re.finditer(r'([^{}]+)\{([^}]*)\}', css_clean):
        selectors_raw = m.group(1).strip()
        props_raw = m.group(2).strip()
        props = {}
        for prop_m in re.finditer(r'([\w-]+)\s*:\s*([^;]+)', props_raw):
            props[prop_m.group(1).strip().lower()] = prop_m.group(2).strip()

        for sel in selectors_raw.split(","):
            sel = sel.strip()
            id_m = re.search(r'#([\w-]+)', sel)
            if id_m:
                elem_id = id_m.group(1)
                if elem_id not in result:
                    result[elem_id] = {}
                result[elem_id].update(props)
    return result


def build_char_format_map(css_classes: Dict[str, Dict[str, str]]) -> Dict[str, Dict]:
    """Build a map of CSS class -> bold/italic from parsed CSS."""
    fmt_map: Dict[str, Dict] = {}
    for cls, props in css_classes.items():
        is_bold = False
        is_italic = False
        font_size = 0.0

        weight = props.get("font-weight", "")
        if weight in ("bold", "700", "800", "900"):
            is_bold = True

        style = props.get("font-style", "")
        if style in ("italic", "oblique"):
            is_italic = True

        family = props.get("font-family", "").lower()
        if "bold" in family:
            is_bold = True
        if "italic" in family or "oblique" in family:
            is_italic = True

        size_str = props.get("font-size", "")
        size_m = re.match(r'([\d.]+)', size_str)
        if size_m:
            try:
                font_size = float(size_m.group(1))
            except ValueError:
                pass

        fmt_map[cls] = {"bold": is_bold, "italic": is_italic, "font_size": font_size}
    return fmt_map


# ---------------------------------------------------------------------------
# Phase 2 — Page-Level Text Extraction
# ---------------------------------------------------------------------------

# Paragraph classification: known CSS class -> (role, heading_level)
# This handles InDesign classes AND common generic patterns
PARAGRAPH_CLASS_MAP = {
    # Page furniture (STRIP)
    "Running-Header": ("running_header", 0),
    "Page-Number": ("page_number", 0),
    "running-header": ("running_header", 0),
    "page-number": ("page_number", 0),
    "runninghead": ("running_header", 0),
    "pagenum": ("page_number", 0),

    # Headings
    "Chapter-Number": ("chapter_number", 0),
    "Chapter-Title": ("chapter_title", 1),
    "chapter-number": ("chapter_number", 0),
    "chapter-title": ("chapter_title", 1),
    "h1-centered": ("heading", 1),
    "h1-centered-serif": ("heading", 1),
    "h1-Left": ("heading", 1),
    "h1-Left-NO-SPACE": ("heading", 1),
    "h1-PP": ("heading", 1),
    "h2-Left": ("heading", 2),
    "h2-Centered": ("heading", 2),
    "h-2-small": ("heading", 2),
    "Sub-H-2": ("heading", 2),
    "h3": ("heading", 3),
    "h3-numbered": ("heading", 3),
    "Sub-H-3": ("heading", 3),
    "Matter": ("heading", 1),

    # Body text
    "p": ("body", 0),
    "p1": ("body", 0),
    "text": ("body", 0),
    "text-2": ("body", 0),
    "temp-01": ("body", 0),
    "temp-2": ("body", 0),
    "p-Inset": ("body_indented", 0),
    "p-rev-indent": ("body", 0),
    "p-space": ("body", 0),
    "p-sans-mokoko-center": ("body_centered", 0),
    "Marketing-sans": ("body", 0),

    # Lists
    "ul": ("unordered_list", 0),
    "ul-inset": ("unordered_list", 0),
    "ul-2nd": ("unordered_list_nested", 0),
    "bullet-list": ("unordered_list", 0),
    "Marketing-UL": ("unordered_list", 0),
    "Marketing-UL-Inset": ("unordered_list", 0),
    "ol": ("ordered_list", 0),
    "ol-abc": ("ordered_list_alpha", 0),
    "ol-text": ("ordered_list", 0),
    "Chapter-Bullets": ("chapter_bullets", 0),

    # Tables
    "Table-Head": ("table_head", 0),
    "Table-h1": ("table_header", 0),
    "Table-h1-JMD": ("table_header", 0),
    "Table-h1-reg": ("table_header", 0),
    "Table-Text": ("table_cell", 0),
    "Table-P": ("table_cell", 0),
    "Table-P-Narrow": ("table_cell", 0),
    "Table-Sans-JMD": ("table_cell", 0),
    "Table-Sans-ul-JMD": ("table_cell_list", 0),
    "Table-UL": ("table_cell_list", 0),
    "Tabke-UL-Reg": ("table_cell_list", 0),
    "Yable-UL-no-header": ("table_cell_list", 0),
    "Mokoko-Table-ul": ("table_cell_list", 0),
    "Table-Checkbox-ul": ("table_checkbox", 0),
    "Checkbox-Table": ("table_checkbox", 0),

    # Sidebars / Examples
    "Tab-Text-REV": ("sidebar_tab", 0),
    "Proceedure-SB-h1": ("sidebar_heading", 0),
    "Example-SB-p": ("sidebar_body", 0),
    "Example-p": ("example_body", 0),
    "Example-p-OL": ("example_list", 0),

    # TOC
    "TOC-h1": ("toc_section", 0),
    "TOC-Chapter-no": ("toc_chapter_number", 0),
    "TOC-Chapter-Title": ("toc_chapter_title", 0),
    "TOC-matter": ("toc_matter", 0),

    # Index
    "Index-h1": ("index_heading", 2),
    "Index-p": ("index_entry", 0),
    "Index-sub1": ("index_subentry", 0),
    "Index-sub2": ("index_subentry2", 0),

    # References
    "References": ("reference", 0),

    # Captions
    "Caption": ("caption", 0),
    "Caption-small": ("caption", 0),

    # Other
    "Copyright": ("copyright", 0),
}

# Roles that should be stripped from reflowable output
STRIP_ROLES = {"running_header", "page_number", "chapter_number"}

# Roles that indicate body content (for stitching)
BODY_CONTENT_ROLES = {
    "body", "body_indented", "body_centered",
    "unordered_list", "unordered_list_nested", "ordered_list",
    "ordered_list_alpha", "reference", "index_entry", "index_subentry",
    "index_subentry2", "example_body", "example_list",
    "sidebar_body", "table_cell", "table_cell_list", "table_checkbox",
}


def _cleanup_hyphenation(text: str) -> str:
    """Final pass: fix any remaining hyphenation artifacts in assembled text.

    Catches patterns like 'organi- zation' where a word was split with a
    hyphen across a line/page break in the fixed-layout source.
    """
    # Common short words that are never hyphenated compound parts
    _NOT_COMPOUND_STEMS = {
        "the", "a", "an", "and", "or", "of", "in", "on", "at", "to",
        "for", "is", "it", "as", "by", "be", "if", "do", "no", "so",
        "up", "he", "we", "my", "un", "re", "pre", "non",
    }

    def _replace_hyphen(m: re.Match) -> str:
        before = m.group(1)  # e.g. 'organi'
        after = m.group(2)   # e.g. 'zation'
        # Skip if before already contains a hyphen (compound word)
        if '-' in before:
            return m.group(0)
        # If after starts with uppercase, only merge if stem is a
        # common short word that would never form a compound
        if after and after[0].isupper():
            if before.lower() not in _NOT_COMPOUND_STEMS:
                return m.group(0)
        return before + after

    return re.sub(r'(\w+)- (\w+)', _replace_hyphen, text)


def _join_lines_with_dehyphenation(line_texts: List[str]) -> str:
    """Join line texts with space, applying dehyphenation at line boundaries.

    If a line ends with a hyphenated word fragment (e.g. 'organi-') and the
    next line starts with a lowercase continuation (e.g. 'zation'), they are
    merged into one word ('organization').
    """
    if not line_texts:
        return ""
    result_parts: List[str] = [line_texts[0]]
    for i in range(1, len(line_texts)):
        prev = result_parts[-1]
        curr = line_texts[i]
        if not prev or not curr:
            result_parts.append(curr)
            continue

        # Check if previous line ends with a hyphenated word
        prev_words = prev.split()
        curr_words = curr.split()
        if prev_words and curr_words:
            joined = dehyphenate(prev_words[-1], curr_words[0])
            if joined is not None:
                # Replace last word of prev with joined word, skip first word of curr
                prev_words[-1] = joined
                result_parts[-1] = " ".join(prev_words)
                remaining = " ".join(curr_words[1:]) if len(curr_words) > 1 else ""
                if remaining:
                    result_parts[-1] += " " + remaining
                continue

        result_parts.append(curr)
    return " ".join(result_parts)


class FXLPageExtractor:
    """Extracts structured content from a single FXL XHTML page.

    Works generically with ANY FXL variant by finding all absolutely-
    positioned elements and computing effective coordinates.
    """

    def __init__(self, css_classes: Dict[str, Dict[str, str]], char_fmt: Dict[str, Dict],
                 fxl_info: FXLDetectionResult,
                 id_css_rules: Optional[Dict[str, Dict[str, str]]] = None):
        self.css_classes = css_classes
        self.char_fmt = char_fmt
        self.fxl_info = fxl_info
        self.id_css_rules: Dict[str, Dict[str, str]] = id_css_rules or {}
        self.viewport_w = fxl_info.viewport_width or 504
        self.viewport_h = fxl_info.viewport_height or 720

    def extract_page(self, xhtml_bytes: bytes, filename: str, spine_index: int) -> FXLPage:
        """Extract structured content from one FXL XHTML page."""
        try:
            soup = BeautifulSoup(xhtml_bytes, "lxml")
        except Exception:
            soup = BeautifulSoup(xhtml_bytes, "html.parser")

        page = FXLPage(spine_index=spine_index, filename=filename)

        # Detect viewport from this page
        vp_meta = soup.find("meta", attrs={"name": "viewport"})
        if vp_meta:
            vp_content = vp_meta.get("content", "")
            m = re.search(r'width=(\d+)', vp_content)
            if m:
                self.viewport_w = int(m.group(1))
            m = re.search(r'height=(\d+)', vp_content)
            if m:
                self.viewport_h = int(m.group(1))

        body = soup.find("body")
        if not body:
            return page

        # Strategy depends on source tool
        if self.fxl_info.source_tool == "indesign":
            self._extract_indesign(body, page)
        else:
            self._extract_generic(body, page)

        # Post-process: classify paragraphs, extract page metadata
        self._post_process_page(page)
        return page

    def _extract_indesign(self, body: Tag, page: FXLPage):
        """InDesign-specific extraction: _idContainer divs with scale transforms."""
        containers = body.find_all("div", recursive=False)
        if not containers:
            containers = body.find_all("div")

        for container in containers:
            # Skip disabled full-page fallback images (pure rasterised fallbacks)
            classes = " ".join(container.get("class", []))
            if self._is_disabled_container(container, classes):
                continue

            # Find transform div (inner div with transform style)
            tx, ty, sx, sy = 0.0, 0.0, 1.0, 1.0
            inner = container
            found_inner_transform = False
            for child in container.find_all("div", recursive=False):
                style = child.get("style", "")
                if "transform" in style or "scale" in style:
                    t = self._parse_transform(style)
                    tx, ty, sx, sy = t
                    inner = child
                    found_inner_transform = True
                    break

            # Fallback: if no inner transform div, use container's CSS
            # position (from the external stylesheet) for image placement.
            if not found_inner_transform:
                cid = container.get("id", "")
                if cid and cid in self.id_css_rules:
                    css = self.id_css_rules[cid]
                    transform_val = css.get("transform", "")
                    if transform_val:
                        ctx, cty, _, _ = self._parse_transform(transform_val)
                        tx, ty = ctx, cty

            # Extract paragraphs
            for p_elem in inner.find_all("p", recursive=True):
                para = self._extract_paragraph(p_elem, tx, ty, sx, sy)
                if para and para.full_text.strip():
                    page.paragraphs.append(para)

            # Extract images (with figure-level CSS positioning)
            self._extract_images_from_container(inner, page, tx, ty, sx)

        # Also extract top-level <figure> elements (not nested in divs)
        for fig in body.find_all("figure", recursive=False):
            fig_id = fig.get("id", "")
            fig_classes = " ".join(fig.get("class", []))
            fig_tx, fig_ty = 0.0, 0.0
            fig_scale = 1.0
            if fig_id and fig_id in self.id_css_rules:
                css = self.id_css_rules[fig_id]
                transform_val = css.get("transform", "")
                if transform_val:
                    ftx, fty, fsx, _ = self._parse_transform(transform_val)
                    fig_tx, fig_ty = ftx, fty
            for img in fig.find_all("img"):
                fxl_img = self._parse_image(img, fig_tx, fig_ty, fig_scale)
                if fxl_img:
                    # Get dimensions from CSS if not on img attributes
                    if fig_id and fig_id in self.id_css_rules:
                        self._apply_css_dimensions(fxl_img, self.id_css_rules[fig_id])
                    if fxl_img.keep:
                        page.images.append(fxl_img)

    def _extract_generic(self, body: Tag, page: FXLPage):
        """Generic extraction for any FXL variant.

        Finds all positioned elements and reconstructs reading order from coordinates.
        """
        # Collect all elements with position:absolute or transform
        positioned_elems = []

        for elem in body.descendants:
            if not isinstance(elem, Tag):
                continue
            style = elem.get("style", "")
            if "position" in style and "absolute" in style:
                positioned_elems.append(elem)

        if positioned_elems:
            self._extract_from_positioned_elements(positioned_elems, page)
        else:
            # Fallback: treat entire body as sequential content
            self._extract_sequential(body, page)

    def _extract_from_positioned_elements(self, elements: List[Tag], page: FXLPage):
        """Extract text from a list of absolutely-positioned elements."""
        raw_spans: List[FXLSpan] = []

        for elem in elements:
            style = elem.get("style", "")
            top = self._parse_css_value(style, "top")
            left = self._parse_css_value(style, "left")

            text = elem.get_text(strip=False)
            if not text or not text.strip():
                continue

            is_bold, is_italic = self._detect_formatting_from_elem(elem)
            font_size = self._parse_css_value(style, "font-size")

            raw_spans.append(FXLSpan(
                text=text, top=top, left=left,
                is_bold=is_bold, is_italic=is_italic, font_size=font_size,
            ))

        if not raw_spans:
            return

        # Group spans into lines by top position
        lines = self._group_into_lines(raw_spans, tolerance=3.0)

        # Group lines into paragraphs by vertical gaps
        paragraphs = self._group_lines_into_paragraphs(lines)

        for para_spans, para_top in paragraphs:
            # Group into lines, then dehyphenate across line boundaries
            para_lines = self._group_into_lines(para_spans, tolerance=3.0)
            line_texts = []
            for ln in para_lines:
                ln.sort(key=lambda s: s.left)
                line_texts.append(" ".join(s.text.strip() for s in ln if s.text.strip()))
            text = _join_lines_with_dehyphenation(line_texts) if line_texts else ""
            text = _cleanup_hyphenation(text)
            if text:
                bold_count = sum(1 for s in para_spans if s.is_bold)
                italic_count = sum(1 for s in para_spans if s.is_italic)
                total = len(para_spans) or 1

                role, heading_level = self._classify_paragraph("", para_spans, text)

                page.paragraphs.append(FXLParagraph(
                    css_class="",
                    full_text=text,
                    spans=para_spans,
                    role=role,
                    heading_level=heading_level,
                    is_bold=bold_count > total * 0.5,
                    is_italic=italic_count > total * 0.5,
                    top=para_top,
                ))

        # Extract images
        body = elements[0].find_parent("body") if elements else None
        if body:
            for img in body.find_all("img"):
                fxl_img = self._parse_image(img, 0, 0, 1.0)
                if fxl_img and fxl_img.keep:
                    page.images.append(fxl_img)

    def _extract_sequential(self, body: Tag, page: FXLPage):
        """Fallback: extract body content sequentially (minimal FXL)."""
        for elem in body.children:
            if not isinstance(elem, Tag):
                continue
            tag = elem.name.lower() if elem.name else ""
            if tag in ("p", "div", "span", "h1", "h2", "h3", "h4", "h5", "h6"):
                text = elem.get_text(strip=True)
                if text:
                    role = "body"
                    hlevel = 0
                    if tag.startswith("h") and len(tag) == 2 and tag[1].isdigit():
                        role = "heading"
                        hlevel = int(tag[1])
                    is_bold, is_italic = self._detect_formatting_from_elem(elem)
                    page.paragraphs.append(FXLParagraph(
                        css_class=tag,
                        full_text=text,
                        role=role,
                        heading_level=hlevel,
                        is_bold=is_bold,
                        is_italic=is_italic,
                    ))
            elif tag == "img":
                fxl_img = self._parse_image(elem, 0, 0, 1.0)
                if fxl_img and fxl_img.keep:
                    page.images.append(fxl_img)

    def _extract_paragraph(self, p_elem: Tag, tx: float, ty: float,
                           sx: float, sy: float) -> Optional[FXLParagraph]:
        """Extract a paragraph from a <p> element inside a transform container."""
        css_class = " ".join(p_elem.get("class", []))
        spans: List[FXLSpan] = []

        # Collect all text-bearing children
        span_elems = p_elem.find_all("span", recursive=True)
        if span_elems:
            for span_el in span_elems:
                text = span_el.string or span_el.get_text(strip=False)
                if not text:
                    continue
                style = span_el.get("style", "")
                top = self._parse_css_value(style, "top") * sy + ty
                left = self._parse_css_value(style, "left") * sx + tx
                is_bold, is_italic = self._detect_formatting_from_elem(span_el)
                font_size = self._parse_css_value(style, "font-size")
                spans.append(FXLSpan(
                    text=text, top=top, left=left,
                    is_bold=is_bold, is_italic=is_italic, font_size=font_size,
                ))
        else:
            # No spans — paragraph text is direct
            text = p_elem.get_text(strip=False)
            if text and text.strip():
                is_bold, is_italic = self._detect_formatting_from_elem(p_elem)
                spans.append(FXLSpan(text=text, top=ty, left=tx,
                                     is_bold=is_bold, is_italic=is_italic))

        if not spans:
            return None

        # Sort by reading order
        spans.sort(key=lambda s: (round(s.top, 1), s.left))

        # Group into lines and join
        lines = self._group_into_lines(spans, tolerance=2.0)
        line_texts = []
        for line_spans in lines:
            line_spans.sort(key=lambda s: s.left)
            parts = []
            for s in line_spans:
                t = s.text
                if parts and not parts[-1].endswith((" ", "\t")) and not t.startswith((" ", "\t")):
                    parts.append(" ")
                parts.append(t)
            line_texts.append("".join(parts).strip())

        # Dehyphenate across line boundaries within this paragraph
        full_text = _join_lines_with_dehyphenation(line_texts)
        full_text = _cleanup_hyphenation(full_text)

        # Classify
        role, heading_level = self._classify_paragraph(css_class, spans, full_text)
        bold_count = sum(1 for s in spans if s.is_bold)
        italic_count = sum(1 for s in spans if s.is_italic)
        total = len(spans) or 1

        avg_top = sum(s.top for s in spans) / total if spans else 0.0

        return FXLParagraph(
            css_class=css_class,
            full_text=full_text,
            spans=spans,
            role=role,
            heading_level=heading_level,
            is_bold=bold_count > total * 0.5,
            is_italic=italic_count > total * 0.5,
            top=avg_top,
        )

    def _classify_paragraph(self, css_class: str, spans: List[FXLSpan],
                            text: str) -> Tuple[str, int]:
        """Classify paragraph role. Uses known class map, then heuristics."""
        # Tier 1: known class lookup
        first_class = css_class.split()[0] if css_class else ""
        for cls in css_class.split():
            if cls in PARAGRAPH_CLASS_MAP:
                return PARAGRAPH_CLASS_MAP[cls]

        # Tier 2: heuristic classification
        if not text.strip():
            return ("body", 0)

        # Position-based: running headers are near top, page numbers near bottom
        if spans:
            avg_top = sum(s.top for s in spans) / len(spans)
            vp_h = self.viewport_h
            # Top 8% of page -> likely running header
            if avg_top < vp_h * 0.08 and len(text) < 80:
                return ("running_header", 0)
            # Bottom 5% of page -> likely page number
            if avg_top > vp_h * 0.92 and len(text) < 10 and re.match(r'^[\divxlc]+$', text.strip(), re.I):
                return ("page_number", 0)

        # Font-size-based heading detection
        if spans:
            avg_size = sum(s.font_size for s in spans if s.font_size > 0) / max(1, sum(1 for s in spans if s.font_size > 0))
            if avg_size > 20 and len(text) < 100:
                return ("heading", 1)
            if avg_size > 16 and len(text) < 100:
                return ("heading", 2)

        # Bold short text -> likely heading
        bold_ratio = sum(1 for s in spans if s.is_bold) / max(1, len(spans))
        if bold_ratio > 0.8 and len(text) < 80:
            return ("heading", 2)

        # List detection by content
        stripped = text.strip()
        if stripped.startswith(("•", "●", "○", "■", "□", "▪", "–", "—")):
            return ("unordered_list", 0)
        if re.match(r'^\d+[\.\)]\s', stripped):
            return ("ordered_list", 0)
        if re.match(r'^[a-z][\.\)]\s', stripped):
            return ("ordered_list_alpha", 0)

        # CSS class name pattern matching (generic)
        cls_lower = first_class.lower()
        # Check table/cell/grid FIRST (before heading) so "Table-header" -> table, not heading
        if any(t in cls_lower for t in ("table", "tabel", "tabke", "yable", "cell", "grid")):
            if "head" in cls_lower or "header" in cls_lower or "h1" in cls_lower:
                return ("table_header", 0)
            if "ul" in cls_lower or "list" in cls_lower or "check" in cls_lower:
                return ("table_cell_list", 0)
            return ("table_cell", 0)
        # Check for list patterns
        if "list" in cls_lower or cls_lower.startswith(("ul", "ol", "bullet")):
            return ("unordered_list", 0)
        # Heading detection — only if class is specifically about headings/titles,
        # not incidentally containing "header" (like "no-header", "Running-Header")
        if any(h in cls_lower for h in ("heading", "title")):
            if "sub" in cls_lower or "2" in cls_lower:
                return ("heading", 2)
            if "3" in cls_lower:
                return ("heading", 3)
            return ("heading", 1)
        if "caption" in cls_lower:
            return ("caption", 0)
        if "footnote" in cls_lower or "note" in cls_lower:
            return ("footnote", 0)
        if "sidebar" in cls_lower:
            return ("sidebar_body", 0)
        if "example" in cls_lower:
            return ("example_body", 0)

        return ("body", 0)

    def _detect_formatting_from_elem(self, elem: Tag) -> Tuple[bool, bool]:
        """Detect bold/italic from element's classes and inline style."""
        is_bold = False
        is_italic = False

        # Check inline style
        style = elem.get("style", "")
        if re.search(r'font-weight\s*:\s*(bold|[7-9]00)', style, re.I):
            is_bold = True
        if re.search(r'font-style\s*:\s*(italic|oblique)', style, re.I):
            is_italic = True

        # Check CSS classes
        for cls in elem.get("class", []):
            # Known semantic classes
            cls_lower = cls.lower()
            if cls_lower in ("bold", "b", "strong"):
                is_bold = True
            if cls_lower in ("italic", "italics", "i", "em"):
                is_italic = True

            # Lookup in parsed CSS
            if cls in self.char_fmt:
                fmt = self.char_fmt[cls]
                if fmt.get("bold"):
                    is_bold = True
                if fmt.get("italic"):
                    is_italic = True

        # Check parent elements (e.g. <b>, <strong>, <i>, <em>)
        for parent in elem.parents:
            if not isinstance(parent, Tag):
                break
            pname = (parent.name or "").lower()
            if pname in ("b", "strong"):
                is_bold = True
            elif pname in ("i", "em"):
                is_italic = True

        return is_bold, is_italic

    def _is_disabled_container(self, container: Tag, classes: str) -> bool:
        """Check if a container is a disabled/fallback element to skip.

        InDesign uses ``_idGenObjectStyle-Disabled`` to override decorative
        styling (borders, backgrounds) on containers.  This does NOT mean the
        content should be hidden.  Only skip if the container is a *pure*
        fallback element — i.e. ``_idGenObjectStyle-Disabled`` is the only
        meaningful class, the container has no text, and it only holds a
        single image (a full-page rasterised fallback).
        """
        if "_idGenObjectStyle-Disabled" in classes:
            # Keep containers that have additional content classes
            # (e.g. "Examples-with-Border", "Slides", "Graph")
            class_list = container.get("class", [])
            content_classes = [
                c for c in class_list
                if c != "_idGenObjectStyle-Disabled"
                and not c.startswith("_idGen")
            ]
            if content_classes:
                return False  # Has content-related classes — keep it

            # Keep containers that have actual text content (paragraphs)
            if container.find("p") and container.get_text(strip=True):
                return False

            # Pure disabled container with only an image — likely a fallback
            return True
        # Generic: check for visibility:hidden or display:none
        style = container.get("style", "")
        if "display:none" in style.replace(" ", "") or "visibility:hidden" in style.replace(" ", ""):
            return True
        return False

    def _extract_images_from_container(self, container: Tag, page: FXLPage,
                                       tx: float, ty: float, scale: float):
        for img in container.find_all("img"):
            img_tx, img_ty = tx, ty
            # Check if img is inside a <figure> with its own CSS positioning
            figure_parent = img.find_parent("figure")
            fig_id = figure_parent.get("id", "") if figure_parent else ""
            if fig_id and fig_id in self.id_css_rules:
                css = self.id_css_rules[fig_id]
                transform_val = css.get("transform", "")
                if transform_val:
                    ftx, fty, _, _ = self._parse_transform(transform_val)
                    # Figure's translate is in parent's coordinate space;
                    # convert to viewport coords using parent's scale.
                    img_tx = ftx * scale + tx
                    img_ty = fty * scale + ty

            fxl_img = self._parse_image(img, img_tx, img_ty, scale)
            if fxl_img:
                # Get dimensions from figure CSS if not on img attributes
                if fig_id and fig_id in self.id_css_rules:
                    self._apply_css_dimensions(fxl_img, self.id_css_rules[fig_id])
                if fxl_img.keep:
                    page.images.append(fxl_img)

    def _parse_image(self, img: Tag, tx: float, ty: float, scale: float) -> Optional[FXLImage]:
        src = img.get("src", "") or img.get("xlink:href", "") or img.get("data-src", "")
        if not src:
            return None

        alt = img.get("alt", "")
        width = 0.0
        height = 0.0

        w_attr = img.get("width", "")
        h_attr = img.get("height", "")
        if w_attr:
            try:
                width = float(re.sub(r'[^\d.]', '', str(w_attr)))
            except (ValueError, TypeError):
                pass
        if h_attr:
            try:
                height = float(re.sub(r'[^\d.]', '', str(h_attr)))
            except (ValueError, TypeError):
                pass

        # Filter: skip tiny/decorative images
        keep = True
        if height <= 2 and height > 0:
            keep = False  # Horizontal rule
        if width > 0 and height > 0 and width < 10 and height < 10:
            keep = False  # Truly tiny decorative (dots, bullets)
        # 1px spacer images
        if (width == 1 and height == 1) or "spacer" in src.lower():
            keep = False

        return FXLImage(src=src, width=width, height=height, alt=alt,
                        top=ty, left=tx, keep=keep)

    @staticmethod
    def _apply_css_dimensions(fxl_img: FXLImage, css: Dict[str, str]) -> None:
        """Fill in image width/height from CSS figure dimensions if missing."""
        if fxl_img.width <= 0:
            w_str = css.get("width", "")
            m = re.match(r'([\d.]+)', w_str)
            if m:
                try:
                    fxl_img.width = float(m.group(1))
                except ValueError:
                    pass
        if fxl_img.height <= 0:
            h_str = css.get("height", "")
            m = re.match(r'([\d.]+)', h_str)
            if m:
                try:
                    fxl_img.height = float(m.group(1))
                except ValueError:
                    pass
        # Re-evaluate keep: with CSS dimensions, check size filter again
        if fxl_img.width > 0 and fxl_img.height > 0:
            if fxl_img.height <= 2:
                fxl_img.keep = False
            elif fxl_img.width < 5 and fxl_img.height < 5:
                fxl_img.keep = False
            else:
                fxl_img.keep = True

    def _parse_transform(self, style: str) -> Tuple[float, float, float, float]:
        """Parse CSS transform: translate(tx, ty) scale(s) -> (tx, ty, sx, sy)."""
        tx, ty, sx, sy = 0.0, 0.0, 1.0, 1.0

        # translate(Xpx, Ypx)
        m = re.search(r'translate\(\s*([-\d.]+)\s*(?:px)?\s*,\s*([-\d.]+)\s*(?:px)?\s*\)', style)
        if m:
            tx = float(m.group(1))
            ty = float(m.group(2))

        # scale(S) or scale(Sx, Sy)
        m = re.search(r'scale\(\s*([-\d.]+)\s*(?:,\s*([-\d.]+))?\s*\)', style)
        if m:
            sx = float(m.group(1))
            sy = float(m.group(2)) if m.group(2) else sx

        return tx, ty, sx, sy

    @staticmethod
    def _parse_css_value(style: str, prop: str) -> float:
        """Extract a numeric CSS value (in px) from an inline style string."""
        m = re.search(rf'{prop}\s*:\s*([-\d.]+)', style)
        if m:
            try:
                return float(m.group(1))
            except ValueError:
                pass
        return 0.0

    @staticmethod
    def _group_into_lines(spans: List[FXLSpan], tolerance: float = 2.0) -> List[List[FXLSpan]]:
        """Group spans into lines based on similar top positions."""
        if not spans:
            return []
        sorted_spans = sorted(spans, key=lambda s: (s.top, s.left))
        lines: List[List[FXLSpan]] = []
        current_line: List[FXLSpan] = [sorted_spans[0]]
        current_top = sorted_spans[0].top

        for span in sorted_spans[1:]:
            if abs(span.top - current_top) <= tolerance:
                current_line.append(span)
            else:
                lines.append(current_line)
                current_line = [span]
                current_top = span.top

        if current_line:
            lines.append(current_line)
        return lines

    @staticmethod
    def _group_lines_into_paragraphs(lines: List[List[FXLSpan]],
                                      line_gap_threshold: float = 8.0
                                      ) -> List[Tuple[List[FXLSpan], float]]:
        """Group lines into paragraphs based on vertical gaps between lines."""
        if not lines:
            return []

        paragraphs: List[Tuple[List[FXLSpan], float]] = []
        current_spans: List[FXLSpan] = list(lines[0])
        para_top = lines[0][0].top if lines[0] else 0.0
        prev_top = para_top

        for line_spans in lines[1:]:
            line_top = line_spans[0].top if line_spans else prev_top
            gap = line_top - prev_top

            if gap > line_gap_threshold:
                paragraphs.append((current_spans, para_top))
                current_spans = list(line_spans)
                para_top = line_top
            else:
                current_spans.extend(line_spans)
            prev_top = line_top

        if current_spans:
            paragraphs.append((current_spans, para_top))
        return paragraphs

    def _post_process_page(self, page: FXLPage):
        """Extract page metadata and detect chapter starts."""
        new_paras = []
        for para in page.paragraphs:
            if para.role == "page_number":
                page.page_number = para.full_text.strip()
            elif para.role == "running_header":
                page.running_header = para.full_text.strip()
            elif para.role == "chapter_number":
                page.chapter_number = para.full_text.strip()
                page.is_chapter_start = True
            elif para.role == "chapter_title":
                page.chapter_title = para.full_text.strip()
                page.is_chapter_start = True
            new_paras.append(para)
        page.paragraphs = new_paras

        # NOTE: We do NOT heuristically set is_chapter_start for h1 headings here.
        # Only explicit chapter_title / chapter_number CSS classes set it.
        # When no TOC is available, build_chapter_map_from_pages uses its own
        # heuristic to detect chapter boundaries from the extracted pages.


# ---------------------------------------------------------------------------
# Phase 3 — Chapter Assembly
# ---------------------------------------------------------------------------

def build_chapter_map_from_toc(toc_content: str, spine_files: List[str],
                               opf_dir: str) -> List[ChapterRange]:
    """Build chapter ranges from the EPUB navigation TOC."""
    try:
        soup = BeautifulSoup(toc_content, "lxml")
    except Exception:
        soup = BeautifulSoup(toc_content, "html.parser")

    nav = soup.find("nav", attrs={"epub:type": "toc"}) or soup.find("nav")
    if not nav:
        return []

    # Build filename -> spine index map
    spine_map: Dict[str, int] = {}
    for i, f in enumerate(spine_files):
        basename = Path(f).name
        spine_map[basename] = i
        spine_map[f] = i

    top_entries: List[Tuple[str, str, int, List[Tuple[str, str]]]] = []

    top_ol = nav.find("ol")
    if not top_ol:
        return []

    for li in top_ol.find_all("li", recursive=False):
        a = li.find("a")
        if not a:
            continue
        href = a.get("href", "")
        title = a.get_text(strip=True)
        # Resolve href to spine index
        href_file = href.split("#")[0]
        idx = spine_map.get(href_file, spine_map.get(Path(href_file).name, -1))
        if idx < 0:
            continue

        children = []
        child_ol = li.find("ol")
        if child_ol:
            for child_li in child_ol.find_all("li", recursive=False):
                child_a = child_li.find("a")
                if child_a:
                    children.append((child_a.get_text(strip=True), child_a.get("href", "")))

        top_entries.append((title, href, idx, children))

    if not top_entries:
        return []

    # Merge InDesign dual-entry pattern before building ranges.
    # Pattern: "chapter one" at page N, "Actual Title" at page N or N+1.
    # The label entry ("chapter one/two/...") should be merged into the
    # real title entry so we don't create empty or duplicate chapters.
    merged_entries: List[Tuple[str, str, int, List[Tuple[str, str]]]] = []
    skip_next = False
    for i, (title, href, start_idx, children) in enumerate(top_entries):
        if skip_next:
            skip_next = False
            continue
        # Detect "chapter <word/number>" label entries
        if (re.match(r'^chapter\s+', title, re.I) and
                i + 1 < len(top_entries) and
                abs(top_entries[i + 1][2] - start_idx) <= 1):
            # Merge: use next entry's title but this entry's start_idx
            next_title, next_href, next_idx, next_children = top_entries[i + 1]
            merged_start = min(start_idx, next_idx)
            merged_children = children + next_children
            merged_entries.append((next_title, href, merged_start, merged_children))
            skip_next = True
        else:
            merged_entries.append((title, href, start_idx, children))

    # Convert to ChapterRange
    chapters: List[ChapterRange] = []
    for i, (title, href, start_idx, children) in enumerate(merged_entries):
        # End is start of next chapter, or end of spine
        if i + 1 < len(merged_entries):
            end_idx = merged_entries[i + 1][2]
        else:
            end_idx = len(spine_files)

        element_type = _classify_chapter_type(title)

        chapters.append(ChapterRange(
            start_idx=start_idx,
            end_idx=end_idx,
            title=title,
            element_type=element_type,
            toc_children=children,
        ))

    return chapters


def build_chapter_map_from_pages(pages: List[FXLPage]) -> List[ChapterRange]:
    """Fallback: build chapter ranges from page-level detection.

    Uses two signals to detect chapter boundaries:
    1. Pages with explicit chapter_title/chapter_number CSS classes
    2. Pages whose first content paragraph is a level-1 heading
    """
    chapters: List[ChapterRange] = []
    current_start = 0
    current_title = _find_page_title(pages[0]) if pages else "Untitled"

    for i, page in enumerate(pages):
        is_start = page.is_chapter_start
        # Also detect chapter start from first h1 heading on the page
        if not is_start:
            for p in page.paragraphs:
                if p.role in STRIP_ROLES:
                    continue
                if p.role in ("heading", "chapter_title") and p.heading_level <= 1:
                    is_start = True
                break

        if is_start and i > 0:
            chapters.append(ChapterRange(
                start_idx=current_start,
                end_idx=i,
                title=current_title,
                element_type=_classify_chapter_type(current_title),
            ))
            current_start = i
            current_title = _find_page_title(page) or f"Chapter {len(chapters) + 1}"

    # Last chapter
    if pages:
        chapters.append(ChapterRange(
            start_idx=current_start,
            end_idx=len(pages),
            title=current_title,
            element_type=_classify_chapter_type(current_title),
        ))

    # If no chapter boundaries found, treat whole book as one chapter
    if not chapters and pages:
        chapters.append(ChapterRange(
            start_idx=0, end_idx=len(pages),
            title="Content", element_type="chapter",
        ))

    return chapters


def _find_page_title(page: FXLPage) -> str:
    """Extract a title from a page, preferring chapter_title, then first h1."""
    if page.chapter_title:
        return page.chapter_title
    for p in page.paragraphs:
        if p.role in STRIP_ROLES:
            continue
        if p.role in ("heading", "chapter_title") and p.heading_level <= 1:
            return p.full_text.strip()
        break
    return "Untitled"


def _classify_chapter_type(title: str) -> str:
    t = title.lower().strip()
    if any(w in t for w in ("acknowledgment", "dedication", "foreword", "preface",
                             "about the author", "contributor", "about the")):
        return "preface"
    if "introduction" in t:
        return "preface"
    if any(w in t for w in ("reference", "bibliograph", "suggested reading")):
        return "bibliography"
    if "index" in t:
        return "index"
    if any(w in t for w in ("appendix", "glossary")):
        return "appendix"
    if any(w in t for w in ("table of contents", "contents")):
        return "toc"
    if "copyright" in t:
        return "colophon"
    return "chapter"


# ---------------------------------------------------------------------------
# Cross-page stitching
# ---------------------------------------------------------------------------

def dehyphenate(last_word: str, next_word: str) -> Optional[str]:
    """Join hyphenated word fragments across line/page breaks."""
    if not last_word.endswith("-"):
        return None
    stem = last_word[:-1]
    # Compound hyphen: keep as-is
    if "-" in stem:
        return None
    # Next word starts with uppercase -> probably new sentence
    if next_word and next_word[0].isupper():
        return None
    return stem + next_word


def _is_compatible_for_merge(last_para: FXLParagraph, first_para: FXLParagraph) -> bool:
    """Check if two paragraphs can be merged across a page boundary."""
    if last_para.role not in BODY_CONTENT_ROLES:
        return False
    if first_para.role not in BODY_CONTENT_ROLES:
        return False

    # Same role or compatible roles
    if last_para.role == first_para.role:
        pass  # Compatible
    elif {last_para.role, first_para.role} <= {"body", "body_indented", "body_centered"}:
        pass  # Body variants are compatible
    else:
        return False

    # Last paragraph shouldn't end with terminal punctuation
    last_text = last_para.full_text.rstrip()
    if last_text and last_text[-1] in '.?!:;"\u201d\u2019':
        return False

    return True


def _get_last_content_para(page: FXLPage) -> Optional[FXLParagraph]:
    for p in reversed(page.paragraphs):
        if p.role not in STRIP_ROLES:
            return p
    return None


def _get_first_content_para(page: FXLPage) -> Optional[FXLParagraph]:
    for p in page.paragraphs:
        if p.role not in STRIP_ROLES:
            return p
    return None


def _remove_decorative_images(pages: List[FXLPage],
                              threshold_ratio: float = 0.1) -> None:
    """Remove decorative images that appear on many pages.

    Images repeated on more than ``threshold_ratio`` of all pages are almost
    certainly page furniture (borders, decorative backgrounds) rather than
    content figures.  Removing them avoids bloating the reflowable output
    with duplicated decorative images.
    """
    from collections import Counter
    total_pages = len(pages)
    if total_pages < 5:
        return

    # Count how many pages each image src appears on
    page_counts: Counter = Counter()
    for page in pages:
        seen_on_page: set = set()
        for img in page.images:
            if img.src not in seen_on_page:
                page_counts[img.src] += 1
                seen_on_page.add(img.src)

    threshold = max(5, int(total_pages * threshold_ratio))
    decorative_srcs = {src for src, count in page_counts.items()
                       if count > threshold}

    if decorative_srcs:
        logger.info("Filtering %d decorative image(s) (appear on >%d pages): %s",
                    len(decorative_srcs), threshold,
                    ", ".join(sorted(decorative_srcs)[:5]))
        for page in pages:
            page.images = [img for img in page.images
                           if img.src not in decorative_srcs]


def _build_page_content_stream(page: FXLPage) -> List[FXLParagraph]:
    """Build an interleaved stream of paragraphs and images for a page.

    Images are inserted as pseudo-paragraphs (role='image') at the position
    matching their vertical coordinate on the page, so they appear inline
    with the text content rather than being dumped at the end.

    Chapter-title and chapter-number paragraphs are promoted to the front
    so they always appear before body content and images.
    """
    content_paras = [p for p in page.paragraphs if p.role not in STRIP_ROLES]

    if not page.images:
        # Still reorder: chapter_title/chapter_number first
        return _promote_chapter_header_paras(content_paras)

    # Deduplicate images by src on the same page
    seen_srcs: set = set()
    unique_images = []
    for img in page.images:
        if img.src not in seen_srcs:
            seen_srcs.add(img.src)
            unique_images.append(img)

    # Build a merged list sorted by top position
    items: List[Tuple[float, str, object]] = []
    for p in content_paras:
        items.append((p.top, "para", p))
    for img in unique_images:
        # Create a pseudo-paragraph for the image
        img_para = FXLParagraph(
            css_class="", full_text="",
            role="image", top=img.top,
            images=[img],
        )
        items.append((img.top, "image", img_para))

    items.sort(key=lambda x: x[0])
    result = [item[2] for item in items]
    return _promote_chapter_header_paras(result)


def _promote_chapter_header_paras(paras: List[FXLParagraph]) -> List[FXLParagraph]:
    """Move chapter_title and chapter_number paragraphs to the front.

    On chapter start pages, the chapter number/title may be positioned lower
    on the page than decorative images, but they should be emitted first
    in the reflowable output.
    """
    header_roles = {"chapter_title", "chapter_number", "chapter_bullets"}
    headers = [p for p in paras if p.role in header_roles]
    rest = [p for p in paras if p.role not in header_roles]
    if not headers:
        return paras
    # Order headers: chapter_number first, then chapter_title, then bullets
    role_order = {"chapter_number": 0, "chapter_title": 1, "chapter_bullets": 2}
    headers.sort(key=lambda p: role_order.get(p.role, 9))
    return headers + rest


def assemble_chapters(pages: List[FXLPage],
                      chapter_ranges: List[ChapterRange]) -> List[AssembledChapter]:
    """Stitch pages into chapters with cross-page paragraph merging."""
    assembled: List[AssembledChapter] = []

    for ch_range in chapter_ranges:
        ch_pages = pages[ch_range.start_idx:ch_range.end_idx]
        if not ch_pages:
            continue

        all_paras: List[FXLParagraph] = []
        all_images: List[FXLImage] = []
        pending_pagebreak: Optional[str] = None

        for page_idx, page in enumerate(ch_pages):
            # Insert pagebreak marker between pages (not before first)
            if page_idx > 0 and page.page_number:
                pending_pagebreak = page.page_number

            page_stream = _build_page_content_stream(page)
            all_images.extend(page.images)

            # Try merging last paragraph of prev page with first of this page
            if page_idx > 0 and all_paras and page_stream:
                last_para = all_paras[-1]
                # Find first non-image content paragraph
                first_content = None
                first_content_idx = 0
                for ci, cp in enumerate(page_stream):
                    if cp.role not in ("image", "pagebreak") and cp.role not in STRIP_ROLES:
                        first_content = cp
                        first_content_idx = ci
                        break

                if first_content and _is_compatible_for_merge(last_para, first_content):
                    # Merge: append text with dehyphenation
                    merged_text = last_para.full_text.rstrip()
                    next_text = first_content.full_text.lstrip()
                    words_last = merged_text.split()
                    words_next = next_text.split()
                    if words_last and words_next:
                        joined = dehyphenate(words_last[-1], words_next[0])
                        if joined:
                            words_last[-1] = joined
                            words_next = words_next[1:]
                            merged_text = " ".join(words_last)
                            next_text = " ".join(words_next)

                    last_para.full_text = _cleanup_hyphenation(merged_text + " " + next_text)
                    last_para.spans.extend(first_content.spans)

                    # Add remaining items from this page (skip the merged one)
                    if pending_pagebreak:
                        all_paras.append(FXLParagraph(
                            css_class="", full_text=pending_pagebreak,
                            role="pagebreak",
                        ))
                        pending_pagebreak = None

                    for ci, cp in enumerate(page_stream):
                        if ci == first_content_idx:
                            continue  # Skip merged paragraph
                        all_paras.append(cp)
                    continue

            # Normal case: add all content from this page
            if pending_pagebreak:
                all_paras.append(FXLParagraph(
                    css_class="", full_text=pending_pagebreak,
                    role="pagebreak",
                ))
                pending_pagebreak = None

            all_paras.extend(page_stream)

        title = ch_range.title
        if not title and ch_pages:
            title = ch_pages[0].chapter_title or f"Section {ch_range.start_idx}"

        # Deduplicate image paragraphs across pages within the chapter
        seen_img_srcs: set = set()
        deduped_paras: List[FXLParagraph] = []
        for p in all_paras:
            if p.role == "image" and p.images:
                src = p.images[0].src
                if src in seen_img_srcs:
                    continue
                seen_img_srcs.add(src)
            deduped_paras.append(p)
        all_paras = deduped_paras

        # Final dehyphenation pass on all assembled paragraphs
        for p in all_paras:
            if p.full_text and p.role not in ("pagebreak", "image"):
                p.full_text = _cleanup_hyphenation(p.full_text)

        assembled.append(AssembledChapter(
            title=title,
            element_type=ch_range.element_type,
            pages=ch_pages,
            paragraphs=all_paras,
            images=all_images,
            page_range=(ch_range.start_idx, ch_range.end_idx),
        ))

    return assembled


# ---------------------------------------------------------------------------
# Phase 4 — Reflowable XHTML Generation
# ---------------------------------------------------------------------------

def _deduplicate_headings(paras: List[FXLParagraph]) -> List[FXLParagraph]:
    """Remove consecutive duplicate headings with the same text.

    InDesign FXL often has a Chapter-Title on one page and h1-Left repeating
    the same text on the very next page.  Only remove a heading if it
    immediately follows another heading (possibly separated by pagebreaks or
    images) with the same normalized text.  Non-consecutive duplicates like
    repeated "Example:" headings throughout a chapter are kept.
    """
    if not paras:
        return paras

    heading_roles = {"chapter_title", "heading"}
    result: List[FXLParagraph] = []
    # Track the last heading text we emitted (for consecutive dedup only)
    last_heading_norm: Optional[str] = None

    for p in paras:
        if p.role in heading_roles:
            norm = _normalize_heading_text(p.full_text)
            if norm and norm == last_heading_norm:
                # Skip — consecutive duplicate heading
                continue
            last_heading_norm = norm
            result.append(p)
        elif p.role in ("pagebreak", "image", "chapter_bullets",
                        "chapter_number"):
            # Transparent roles — don't reset the last heading tracker.
            # chapter_bullets/chapter_number sit between chapter_title and
            # the repeated h1 heading on the following page.
            result.append(p)
        else:
            # Any non-heading, non-transparent paragraph resets the tracker
            last_heading_norm = None
            result.append(p)

    return result


def _normalize_heading_text(text: str) -> str:
    """Normalize heading text for comparison (lowercase, strip whitespace/punct)."""
    return re.sub(r'[^a-z0-9]', '', text.lower().strip())


def generate_reflowable_xhtml(chapter: AssembledChapter) -> str:
    """Generate clean reflowable XHTML from an assembled chapter."""
    parts: List[str] = []
    parts.append('<?xml version="1.0" encoding="UTF-8"?>\n')
    parts.append('<!DOCTYPE html>\n')
    parts.append('<html xmlns="http://www.w3.org/1999/xhtml" '
                 'xmlns:epub="http://www.idpf.org/2007/ops">\n')
    parts.append('<head>\n')
    parts.append(f'  <meta charset="utf-8"/>\n')
    parts.append(f'  <title>{html.escape(chapter.title)}</title>\n')
    parts.append('  <link rel="stylesheet" type="text/css" href="css/reflowable.css"/>\n')
    parts.append('</head>\n')
    parts.append('<body>\n')

    # Track state for grouping list items and table cells
    in_list = False
    list_type = ""
    in_table = False
    in_sidebar = False
    sidebar_paras: List[FXLParagraph] = []

    # Deduplicate consecutive identical headings
    paras = _deduplicate_headings(chapter.paragraphs)

    # First pass: emit title heading
    title_emitted = False

    i = 0
    while i < len(paras):
        para = paras[i]
        role = para.role

        # Close open structures if role changes
        if in_list and role not in ("unordered_list", "unordered_list_nested",
                                     "ordered_list", "ordered_list_alpha",
                                     "chapter_bullets"):
            parts.append(f"</{list_type}>\n")
            in_list = False

        if in_table and role not in ("table_head", "table_header",
                                      "table_cell", "table_cell_list",
                                      "table_checkbox"):
            parts.append("</tbody></table>\n")
            in_table = False

        if in_sidebar and role not in ("sidebar_heading", "sidebar_body",
                                        "sidebar_tab"):
            parts.append("</aside>\n")
            in_sidebar = False

        # Emit content by role
        if role == "pagebreak":
            pb_id = re.sub(r'[^a-zA-Z0-9]', '', para.full_text)
            parts.append(f'<span epub:type="pagebreak" id="page{pb_id}" '
                         f'title="{html.escape(para.full_text)}"/>\n')

        elif role in ("chapter_title",):
            if not title_emitted:
                parts.append(f"<h1>{_build_inline(para)}</h1>\n")
                title_emitted = True
            else:
                # Subsequent chapter_title roles -> demote to h2
                parts.append(f"<h2>{_build_inline(para)}</h2>\n")

        elif role == "heading":
            level = para.heading_level
            if level < 1:
                level = 2
            if not title_emitted and level == 1:
                # First h1 heading becomes the chapter title
                title_emitted = True
            elif title_emitted and level == 1:
                # After title emitted, demote h1 -> h2
                level = 2
            # Shift all sub-headings down by 1 when title has been emitted
            # h2 -> h3, h3 -> h4, etc. (but only if title was emitted)
            elif title_emitted and level >= 2:
                level = level + 1
            tag = f"h{min(level, 6)}"
            parts.append(f"<{tag}>{_build_inline(para)}</{tag}>\n")

        elif role in ("body", "body_indented", "body_centered"):
            cls_attr = ""
            if role == "body_indented":
                cls_attr = ' class="indent"'
            elif role == "body_centered":
                cls_attr = ' class="center"'
            parts.append(f"<p{cls_attr}>{_build_inline(para)}</p>\n")

        elif role in ("unordered_list", "unordered_list_nested",
                       "chapter_bullets"):
            if not in_list:
                parts.append("<ul>\n")
                in_list = True
                list_type = "ul"
            parts.append(f"  <li>{_build_inline(para)}</li>\n")

        elif role in ("ordered_list", "ordered_list_alpha"):
            if not in_list:
                ol_type = ' type="a"' if role == "ordered_list_alpha" else ""
                parts.append(f"<ol{ol_type}>\n")
                in_list = True
                list_type = "ol"
            parts.append(f"  <li>{_build_inline(para)}</li>\n")

        elif role in ("table_head", "table_header"):
            if not in_table:
                parts.append("<table>\n<thead>\n")
                in_table = True
            parts.append(f"  <tr><th>{_build_inline(para)}</th></tr>\n")
            # Check if next para is not a header -> close thead
            next_role = paras[i + 1].role if i + 1 < len(paras) else ""
            if next_role not in ("table_head", "table_header"):
                parts.append("</thead>\n<tbody>\n")

        elif role in ("table_cell", "table_cell_list", "table_checkbox"):
            if not in_table:
                parts.append("<table>\n<tbody>\n")
                in_table = True
            parts.append(f"  <tr><td>{_build_inline(para)}</td></tr>\n")

        elif role == "sidebar_tab":
            if not in_sidebar:
                label = html.escape(para.full_text.strip())
                parts.append(f'<aside class="sidebar" data-label="{label}">\n')
                in_sidebar = True

        elif role == "sidebar_heading":
            if not in_sidebar:
                parts.append('<aside class="sidebar">\n')
                in_sidebar = True
            parts.append(f'<p class="sidebar-heading"><b>{_build_inline(para)}</b></p>\n')

        elif role == "sidebar_body":
            if not in_sidebar:
                parts.append('<aside class="sidebar">\n')
                in_sidebar = True
            parts.append(f"<p>{_build_inline(para)}</p>\n")

        elif role in ("example_body", "example_list"):
            parts.append(f'<p class="example">{_build_inline(para)}</p>\n')

        elif role == "caption":
            parts.append(f'<p class="caption">{_build_inline(para)}</p>\n')

        elif role == "reference":
            parts.append(f'<p class="reference">{_build_inline(para)}</p>\n')

        elif role in ("index_heading",):
            parts.append(f"<h2>{_build_inline(para)}</h2>\n")

        elif role in ("index_entry",):
            parts.append(f'<p class="index-entry">{_build_inline(para)}</p>\n')

        elif role in ("index_subentry", "index_subentry2"):
            indent = "index-sub1" if role == "index_subentry" else "index-sub2"
            parts.append(f'<p class="{indent}">{_build_inline(para)}</p>\n')

        elif role == "copyright":
            parts.append(f'<p class="copyright">{_build_inline(para)}</p>\n')

        elif role in ("toc_section", "toc_chapter_number", "toc_chapter_title", "toc_matter"):
            parts.append(f'<p class="toc">{_build_inline(para)}</p>\n')

        elif role == "image":
            # Inline image placement
            for img in para.images:
                parts.append(f'<figure><img src="{html.escape(img.src)}" alt="{html.escape(img.alt)}"/></figure>\n')

        else:
            # Fallback: emit as paragraph
            if para.full_text.strip():
                parts.append(f"<p>{_build_inline(para)}</p>\n")

        i += 1

    # Close any open structures
    if in_list:
        parts.append(f"</{list_type}>\n")
    if in_table:
        parts.append("</tbody></table>\n")
    if in_sidebar:
        parts.append("</aside>\n")

    parts.append('</body>\n</html>\n')
    return "".join(parts)


def _build_inline(para: FXLParagraph) -> str:
    """Build inline HTML with bold/italic formatting from spans."""
    if not para.spans:
        return html.escape(para.full_text)

    # If all spans have same formatting as the paragraph level, just return text
    if not any(s.is_bold for s in para.spans) and not any(s.is_italic for s in para.spans):
        return html.escape(para.full_text)

    # Build from spans with formatting transitions
    result: List[str] = []
    current_bold = False
    current_italic = False

    # Group spans by line, then join
    sorted_spans = sorted(para.spans, key=lambda s: (round(s.top, 1), s.left))

    prev_top = None
    for span in sorted_spans:
        text = span.text
        if not text:
            continue

        # Add space between spans on same line or newline between lines
        if prev_top is not None:
            if abs(span.top - prev_top) > 2.0:
                result.append(" ")  # Line break -> space
            elif result and not result[-1].endswith((" ", "\t")) and not text.startswith((" ", "\t")):
                result.append(" ")
        prev_top = span.top

        # Handle bold transitions
        if span.is_bold and not current_bold:
            result.append("<b>")
            current_bold = True
        elif not span.is_bold and current_bold:
            result.append("</b>")
            current_bold = False

        # Handle italic transitions
        if span.is_italic and not current_italic:
            result.append("<i>")
            current_italic = True
        elif not span.is_italic and current_italic:
            result.append("</i>")
            current_italic = False

        result.append(html.escape(text))

    # Close open tags
    if current_italic:
        result.append("</i>")
    if current_bold:
        result.append("</b>")

    return "".join(result)


# ---------------------------------------------------------------------------
# Phase 5 — EPUB Reconstruction
# ---------------------------------------------------------------------------

REFLOWABLE_CSS = """\
body { margin: 1em; font-family: serif; line-height: 1.6; }
h1, h2, h3 { margin-top: 1.5em; margin-bottom: 0.5em; }
p { margin: 0.5em 0; text-indent: 1.5em; }
p.indent { margin-left: 2em; }
p.center { text-align: center; text-indent: 0; }
ul, ol { margin: 0.5em 0 0.5em 2em; }
table { border-collapse: collapse; margin: 1em 0; width: 100%; }
th, td { border: 1px solid #666; padding: 0.4em; text-align: left; }
th { background: #f0f0f0; font-weight: bold; }
aside.sidebar { border: 1px solid #999; padding: 1em; margin: 1em 0; background: #fafafa; }
figure { margin: 1em 0; text-align: center; }
figure img { max-width: 100%; height: auto; }
p.caption { text-align: center; font-style: italic; margin-top: 0.3em; }
p.reference { margin-left: 2em; text-indent: -2em; font-size: 0.9em; }
p.index-entry { margin-left: 1em; }
p.index-sub1 { margin-left: 2em; }
p.index-sub2 { margin-left: 3em; }
p.copyright { font-size: 0.85em; }
p.toc { margin-left: 1em; }
span[epub\\:type="pagebreak"] { display: none; }
"""


def rewrite_epub_as_reflowable(
    epub_path: Path,
    work_dir: Path,
    chapters: List[AssembledChapter],
    fxl_info: FXLDetectionResult,
    spine_files: List[str],
) -> Path:
    """Rewrite the EPUB as a reflowable EPUB in work_dir, return new EPUB path."""
    out_epub = work_dir / "reflowable.epub"
    extract_dir = work_dir / "fxl_extract"
    extract_dir.mkdir(parents=True, exist_ok=True)

    # Extract original EPUB
    with zipfile.ZipFile(epub_path, "r") as zf:
        zf.extractall(extract_dir)
        opf_path = _find_opf_path(zf)

    if not opf_path:
        raise ValueError("Cannot find OPF in EPUB")

    opf_full = extract_dir / opf_path
    opf_dir = opf_full.parent

    # Write reflowable CSS
    css_dir = opf_dir / "css"
    css_dir.mkdir(exist_ok=True)
    (css_dir / "reflowable.css").write_text(REFLOWABLE_CSS, encoding="utf-8")

    # Generate chapter XHTML files
    chapter_files: List[Tuple[str, str, str]] = []  # (id, href, title)
    for idx, ch in enumerate(chapters):
        etype = ch.element_type
        if etype == "toc":
            continue  # Skip TOC chapters — nav handles this

        if etype in ("preface",):
            slug = re.sub(r'[^a-z0-9]+', '_', ch.title.lower()).strip('_')[:30]
            fname = f"preface_{slug}.xhtml"
            item_id = f"preface_{slug}"
        elif etype == "bibliography":
            ch_num = idx + 1
            fname = f"chapter_{ch_num:02d}.xhtml"
            item_id = f"ch{ch_num:02d}"
        elif etype == "index":
            fname = "index_content.xhtml"
            item_id = "index_content"
        elif etype == "appendix":
            slug = re.sub(r'[^a-z0-9]+', '_', ch.title.lower()).strip('_')[:30]
            fname = f"appendix_{slug}.xhtml"
            item_id = f"appendix_{slug}"
        elif etype == "colophon":
            fname = "colophon.xhtml"
            item_id = "colophon"
        else:
            ch_num = idx + 1
            fname = f"chapter_{ch_num:02d}.xhtml"
            item_id = f"ch{ch_num:02d}"

        # Ensure unique filenames
        if (opf_dir / fname).exists():
            fname = f"fxl_{fname}"
            item_id = f"fxl_{item_id}"

        xhtml = generate_reflowable_xhtml(ch)
        (opf_dir / fname).write_text(xhtml, encoding="utf-8")
        chapter_files.append((item_id, fname, ch.title))

    # Remove old FXL XHTML files
    for sf in spine_files:
        old_file = extract_dir / sf
        if old_file.exists():
            old_file.unlink()

    # Rewrite OPF
    _rewrite_opf(opf_full, chapter_files, spine_files)

    # Rewrite toc.xhtml if it exists
    _rewrite_toc_nav(opf_dir, chapter_files)

    # Remove images that weren't referenced by any chapter
    kept_images = set()
    for ch in chapters:
        for img in ch.images:
            kept_images.add(img.src)

    # Package as EPUB zip
    _package_epub_zip(extract_dir, out_epub)

    return out_epub


def _rewrite_opf(opf_path: Path, chapter_files: List[Tuple[str, str, str]],
                 old_spine_files: List[str]):
    """Rewrite OPF to remove FXL metadata and update spine/manifest."""
    # Parse with ET
    try:
        tree = ET.parse(str(opf_path))
        root = tree.getroot()
    except ET.ParseError:
        # Fallback: regex-based rewrite
        _rewrite_opf_regex_fallback(opf_path, chapter_files, old_spine_files)
        return

    # Register namespaces to preserve prefixes on output
    ns_map = {
        "opf": OPF_NS,
        "dc": DC_NS,
    }
    for prefix, uri in ns_map.items():
        ET.register_namespace(prefix, uri)

    # --- Bug 2 fix: Remove rendition metadata via tree, not regex ---
    metadata = root.find(".//{%s}metadata" % OPF_NS) or root.find(".//metadata")
    if metadata is not None:
        metas_to_remove = []
        for meta in list(metadata):
            tag = meta.tag.split("}")[-1] if "}" in meta.tag else meta.tag
            if tag != "meta":
                continue
            prop = meta.get("property", "")
            name = meta.get("name", "")
            # Remove rendition:layout, rendition:orientation, rendition:spread
            if prop.startswith("rendition:"):
                metas_to_remove.append(meta)
            # Remove EPUB2 fixed-layout meta
            if name == "fixed-layout":
                metas_to_remove.append(meta)
            # Remove viewport meta
            if name == "viewport":
                metas_to_remove.append(meta)
        for meta in metas_to_remove:
            metadata.remove(meta)

    # Find manifest and spine
    manifest = root.find(".//{%s}manifest" % OPF_NS) or root.find(".//manifest")
    spine = root.find(".//{%s}spine" % OPF_NS) or root.find(".//spine")

    if manifest is None or spine is None:
        _rewrite_opf_regex_fallback(opf_path, chapter_files, old_spine_files)
        return

    # Remove old spine items for FXL pages
    old_basenames = {Path(f).name for f in old_spine_files}
    items_to_remove = []
    id_to_href = {}
    for item in manifest:
        tag = item.tag.split("}")[-1] if "}" in item.tag else item.tag
        if tag == "item":
            href = item.get("href", "")
            id_to_href[item.get("id", "")] = href
            if Path(href).name in old_basenames:
                items_to_remove.append(item)

    for item in items_to_remove:
        manifest.remove(item)

    # --- Bug 1 fix: Use namespace-qualified element names ---
    for item_id, href, title in chapter_files:
        new_item = ET.SubElement(manifest, "{%s}item" % OPF_NS)
        new_item.set("id", item_id)
        new_item.set("href", href)
        new_item.set("media-type", "application/xhtml+xml")

    # Add CSS item
    css_item = ET.SubElement(manifest, "{%s}item" % OPF_NS)
    css_item.set("id", "reflowable-css")
    css_item.set("href", "css/reflowable.css")
    css_item.set("media-type", "text/css")

    # Rebuild spine: remove old FXL page refs
    old_idrefs = set()
    for itemref in list(spine):
        tag = itemref.tag.split("}")[-1] if "}" in itemref.tag else itemref.tag
        if tag == "itemref":
            idref = itemref.get("idref", "")
            href = id_to_href.get(idref, "")
            if Path(href).name in old_basenames:
                spine.remove(itemref)
                old_idrefs.add(idref)

    # Add new spine entries with proper namespace
    for item_id, href, title in chapter_files:
        new_ref = ET.SubElement(spine, "{%s}itemref" % OPF_NS)
        new_ref.set("idref", item_id)

    tree.write(str(opf_path), xml_declaration=True, encoding="UTF-8")


def _rewrite_opf_regex_fallback(opf_path: Path, chapter_files: List[Tuple[str, str, str]],
                                 old_spine_files: List[str]):
    """Fallback OPF rewrite using regex when ET parsing fails."""
    content = opf_path.read_text(encoding="utf-8")

    # Remove rendition metadata (handle both prefixed and unprefixed)
    for prop in ("rendition:layout", "rendition:orientation", "rendition:spread"):
        content = re.sub(
            rf'<[^>]*property=["\']' + re.escape(prop) + r'["\'][^>]*>[^<]*</[^>]*>\s*',
            '', content
        )
    content = re.sub(
        r'<[^>]*name=["\']fixed-layout["\'][^>]*/?\s*>\s*',
        '', content
    )
    content = re.sub(
        r'<[^>]*name=["\']viewport["\'][^>]*/?\s*>\s*',
        '', content
    )
    opf_path.write_text(content, encoding="utf-8")


def _rewrite_toc_nav(opf_dir: Path, chapter_files: List[Tuple[str, str, str]]):
    """Rewrite toc.xhtml navigation to point to new chapter files."""
    # Find toc file
    toc_path = None
    for name in ("toc.xhtml", "nav.xhtml", "toc.html"):
        candidate = opf_dir / name
        if candidate.exists():
            toc_path = candidate
            break

    if not toc_path:
        # Create a minimal nav document
        toc_path = opf_dir / "toc.xhtml"
        nav_html = _build_nav_xhtml(chapter_files)
        toc_path.write_text(nav_html, encoding="utf-8")
        return

    # Rewrite existing nav
    content = toc_path.read_text(encoding="utf-8")
    try:
        soup = BeautifulSoup(content, "lxml")
    except Exception:
        soup = BeautifulSoup(content, "html.parser")

    nav = soup.find("nav", attrs={"epub:type": "toc"}) or soup.find("nav")
    if nav:
        # Replace the nav contents
        nav.clear()
        nav["epub:type"] = "toc"
        heading = soup.new_tag("h1")
        heading.string = "Table of Contents"
        nav.append(heading)

        ol = soup.new_tag("ol")
        for item_id, href, title in chapter_files:
            li = soup.new_tag("li")
            a = soup.new_tag("a", href=href)
            a.string = title
            li.append(a)
            ol.append(li)
        nav.append(ol)

    # Bug 3 fix: Remove page-list nav (references deleted FXL page files)
    page_list = soup.find("nav", attrs={"epub:type": "page-list"})
    if page_list:
        page_list.decompose()

    # Also remove landmarks nav (may reference deleted files)
    landmarks = soup.find("nav", attrs={"epub:type": "landmarks"})
    if landmarks:
        landmarks.decompose()

    toc_path.write_text(str(soup), encoding="utf-8")


def _build_nav_xhtml(chapter_files: List[Tuple[str, str, str]]) -> str:
    parts = ['<?xml version="1.0" encoding="UTF-8"?>\n',
             '<!DOCTYPE html>\n',
             '<html xmlns="http://www.w3.org/1999/xhtml" '
             'xmlns:epub="http://www.idpf.org/2007/ops">\n',
             '<head><meta charset="utf-8"/><title>Table of Contents</title></head>\n',
             '<body>\n',
             '<nav epub:type="toc">\n',
             '<h1>Table of Contents</h1>\n',
             '<ol>\n']
    for item_id, href, title in chapter_files:
        parts.append(f'  <li><a href="{html.escape(href)}">{html.escape(title)}</a></li>\n')
    parts.append('</ol>\n</nav>\n</body>\n</html>\n')
    return "".join(parts)


def _package_epub_zip(extract_dir: Path, out_epub: Path):
    """Package extracted directory as a valid EPUB zip."""
    with zipfile.ZipFile(out_epub, "w", zipfile.ZIP_DEFLATED) as zf:
        # mimetype must be first and uncompressed
        mimetype_path = extract_dir / "mimetype"
        if mimetype_path.exists():
            zf.write(mimetype_path, "mimetype", compress_type=zipfile.ZIP_STORED)

        for file_path in sorted(extract_dir.rglob("*")):
            if file_path.is_dir():
                continue
            arc_name = str(file_path.relative_to(extract_dir))
            if arc_name == "mimetype":
                continue  # Already added
            zf.write(file_path, arc_name)


# ---------------------------------------------------------------------------
# Main entry point
# ---------------------------------------------------------------------------

def preprocess_fxl_epub(
    epub_path: Path,
    work_dir: Path,
    fxl_info: FXLDetectionResult,
    tracker=None,
) -> Path:
    """Main entry: preprocess a fixed-layout EPUB into a reflowable one.

    Args:
        epub_path: Path to original FXL EPUB.
        work_dir: Working directory for temp files.
        fxl_info: Detection result from detect_fixed_layout().
        tracker: Optional progress tracker.

    Returns:
        Path to the new reflowable EPUB file.
    """
    logger.info("FXL Preprocessing: starting (%s pages, source=%s)",
                fxl_info.page_count, fxl_info.source_tool)

    fxl_work = work_dir / "fxl_work"
    fxl_work.mkdir(parents=True, exist_ok=True)

    with zipfile.ZipFile(epub_path, "r") as zf:
        opf_path = _find_opf_path(zf)
        if not opf_path:
            raise ValueError("Cannot find OPF file in EPUB")

        opf_content = zf.read(opf_path).decode("utf-8", errors="replace")
        opf_dir = str(Path(opf_path).parent)
        if opf_dir == ".":
            opf_dir = ""

        spine_files = _get_spine_xhtml_files(opf_content, opf_path, zf)

        # Phase 2a: Parse CSS
        css_classes: Dict[str, Dict[str, str]] = {}
        id_css_rules: Dict[str, Dict[str, str]] = {}
        for name in zf.namelist():
            if name.endswith(".css"):
                try:
                    css_text = zf.read(name).decode("utf-8", errors="replace")
                    css_classes.update(parse_fxl_css(css_text))
                    id_css_rules.update(parse_fxl_css_ids(css_text))
                except Exception:
                    pass

        char_fmt = build_char_format_map(css_classes)
        extractor = FXLPageExtractor(css_classes, char_fmt, fxl_info, id_css_rules)

        # Phase 2b: Extract all pages
        if tracker:
            tracker.update_progress(10)
        print(f"  Extracting {len(spine_files)} FXL pages...")

        pages: List[FXLPage] = []
        for idx, sf in enumerate(spine_files):
            try:
                xhtml_bytes = zf.read(sf)
            except KeyError:
                logger.warning("Spine file not found in EPUB: %s", sf)
                continue
            page = extractor.extract_page(xhtml_bytes, sf, idx)
            pages.append(page)

        if tracker:
            tracker.update_progress(25)
        logger.info("FXL Preprocessing: extracted %d pages", len(pages))

        # Phase 2c: Detect and remove decorative images
        # Images appearing on many pages are page furniture (borders, backgrounds)
        _remove_decorative_images(pages)

        # Phase 3a: Build chapter map
        toc_content = None
        for name in zf.namelist():
            basename = Path(name).name.lower()
            if basename in ("toc.xhtml", "nav.xhtml"):
                try:
                    toc_content = zf.read(name).decode("utf-8", errors="replace")
                except Exception:
                    pass
                break

    # Build chapters from TOC or page-level detection
    if toc_content:
        chapter_ranges = build_chapter_map_from_toc(toc_content, spine_files, opf_dir)
        print(f"  Built {len(chapter_ranges)} chapter ranges from TOC")
    else:
        chapter_ranges = []

    if not chapter_ranges:
        chapter_ranges = build_chapter_map_from_pages(pages)
        print(f"  Built {len(chapter_ranges)} chapter ranges from page analysis")

    if tracker:
        tracker.update_progress(30)

    # Phase 3b: Assemble chapters
    assembled = assemble_chapters(pages, chapter_ranges)
    print(f"  Assembled {len(assembled)} chapters")

    if tracker:
        tracker.update_progress(35)

    # Phase 5: Rewrite as reflowable EPUB
    print("  Generating reflowable EPUB...")
    reflowable_path = rewrite_epub_as_reflowable(
        epub_path, fxl_work, assembled, fxl_info, spine_files,
    )

    if tracker:
        tracker.update_progress(45)

    total_paras = sum(len(ch.paragraphs) for ch in assembled)
    total_images = sum(len(ch.images) for ch in assembled)
    logger.info("FXL Preprocessing complete: %d chapters, %d paragraphs, %d images",
                len(assembled), total_paras, total_images)
    print(f"  FXL -> Reflowable: {len(assembled)} chapters, "
          f"{total_paras} paragraphs, {total_images} images")

    return reflowable_path
