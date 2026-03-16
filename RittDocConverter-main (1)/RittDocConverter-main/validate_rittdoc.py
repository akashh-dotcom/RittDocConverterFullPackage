#!/usr/bin/env python3
"""
RittDoc DTD Validation Script

This script validates RittDoc XML packages and files against the RittDoc DTD.
It can validate:
1. Packaged ZIP files containing Book.XML
2. Individual XML files
3. Apply XSLT transformation and then validate
4. LoadBook/R2Utilities compatibility requirements

Usage:
    python validate_rittdoc.py package.zip
    python validate_rittdoc.py book.xml
    python validate_rittdoc.py --transform input.xml output.xml
    python validate_rittdoc.py --loadbook package.zip  # Full LoadBook validation
"""

import argparse
import re
import sys
import tempfile
import zipfile
from dataclasses import dataclass, field
from pathlib import Path
from typing import List, Optional, Set

from lxml import etree

# Import validation and transformation functions
from xslt_transformer import transform_to_rittdoc_compliance

ROOT = Path(__file__).resolve().parent
DTD_PATH = ROOT / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"


# =============================================================================
# LoadBook/R2Utilities Validation
# =============================================================================

@dataclass
class ValidationResult:
    """Container for validation results."""
    passed: bool = True
    errors: List[str] = field(default_factory=list)
    warnings: List[str] = field(default_factory=list)
    info: List[str] = field(default_factory=list)

    def add_error(self, msg: str):
        self.errors.append(msg)
        self.passed = False

    def add_warning(self, msg: str):
        self.warnings.append(msg)

    def add_info(self, msg: str):
        self.info.append(msg)

    def merge(self, other: "ValidationResult"):
        self.errors.extend(other.errors)
        self.warnings.extend(other.warnings)
        self.info.extend(other.info)
        if not other.passed:
            self.passed = False

    def to_report(self) -> str:
        lines = []
        if self.errors:
            lines.append("ERRORS:")
            for e in self.errors:
                lines.append(f"  [FAIL] {e}")
        if self.warnings:
            lines.append("WARNINGS:")
            for w in self.warnings:
                lines.append(f"  [WARN] {w}")
        if self.info:
            lines.append("INFO:")
            for i in self.info:
                lines.append(f"  [INFO] {i}")
        return "\n".join(lines)


def _local_name(element: etree._Element) -> str:
    """Get local name without namespace."""
    tag = element.tag
    if not isinstance(tag, str):
        return ""
    if tag.startswith("{"):
        return tag.split("}", 1)[1]
    return tag


def validate_loadbook_requirements(
    tree: etree._ElementTree,
    check_images: bool = True,
    image_files: Optional[Set[str]] = None
) -> ValidationResult:
    """
    Validate XML against LoadBook.bat and R2Utilities requirements.

    These checks go beyond DTD validation to ensure the package will
    successfully process through the RISBackend and R2Utilities pipeline.

    Args:
        tree: Parsed XML tree
        check_images: Whether to validate image references
        image_files: Set of available image filenames (for cross-reference)

    Returns:
        ValidationResult with errors, warnings, and info
    """
    result = ValidationResult()
    root = tree.getroot()

    # =========================================================================
    # 1. BOOK-LEVEL REQUIREMENTS (from AddRISInfo.xsl)
    # =========================================================================

    result.add_info("Checking book-level requirements...")

    # 1.1 bookinfo/isbn - REQUIRED
    isbn_elem = root.find(".//bookinfo/isbn")
    if isbn_elem is None or not (isbn_elem.text or "").strip():
        result.add_error("Missing /book/bookinfo/isbn - REQUIRED by AddRISInfo.xsl")
    else:
        isbn_text = (isbn_elem.text or "").strip()
        # Check ISBN format (should be digits only after stripping)
        isbn_clean = re.sub(r"[^0-9X]", "", isbn_text.upper())
        if len(isbn_clean) not in (10, 13):
            result.add_warning(f"ISBN '{isbn_text}' may be invalid (expected 10 or 13 digits)")
        result.add_info(f"ISBN: {isbn_clean}")

    # 1.2 bookinfo/title - REQUIRED
    title_elem = root.find(".//bookinfo/title")
    if title_elem is None:
        result.add_error("Missing /book/bookinfo/title - REQUIRED by AddRISInfo.xsl")
    else:
        title_text = "".join(title_elem.itertext()).strip()
        if not title_text:
            result.add_error("/book/bookinfo/title is empty - REQUIRED by AddRISInfo.xsl")
        else:
            result.add_info(f"Title: {title_text[:50]}{'...' if len(title_text) > 50 else ''}")

    # 1.3 Author/Editor - REQUIRED (AddRISInfo.xsl terminates if missing!)
    author_found = False
    author_paths = [
        ".//bookinfo/authorgroup/author",
        ".//bookinfo/author",
        ".//bookinfo/authorgroup/editor",
        ".//bookinfo/editor",
    ]
    for path in author_paths:
        elems = root.findall(path)
        for elem in elems:
            # Check if author has content
            if elem.text and elem.text.strip():
                author_found = True
                break
            # Check for personname or name parts
            personname = elem.find("personname")
            if personname is not None:
                name_text = "".join(personname.itertext()).strip()
                if name_text:
                    author_found = True
                    break
            # Check for firstname/surname directly
            firstname = elem.find("firstname")
            surname = elem.find("surname")
            if (firstname is not None and firstname.text) or (surname is not None and surname.text):
                author_found = True
                break
        if author_found:
            break

    if not author_found:
        result.add_error(
            "No author or editor found - AddRISInfo.xsl will TERMINATE with 'No Author Found' error. "
            "Required: /book/bookinfo/authorgroup/author or /book/bookinfo/author or editors"
        )
    else:
        result.add_info("Author/editor found [OK]")

    # 1.4 pubdate or copyright/year - recommended
    pubdate = root.find(".//bookinfo/pubdate")
    copyright_year = root.find(".//bookinfo/copyright/year")
    if (pubdate is None or not (pubdate.text or "").strip()) and \
       (copyright_year is None or not (copyright_year.text or "").strip()):
        result.add_warning("No pubdate or copyright/year found - risinfo will have empty pubdate")

    # 1.5 publisher - recommended
    publisher = root.find(".//bookinfo/publisher/publishername")
    if publisher is None or not (publisher.text or "").strip():
        result.add_warning("No publisher found - risinfo will have empty publisher")

    # =========================================================================
    # 2. CHAPTER REQUIREMENTS (from AddRISInfo.xsl)
    # =========================================================================

    result.add_info("Checking chapter requirements...")

    chapters = root.findall(".//chapter")
    chapter_count = 0
    for chapter in chapters:
        chapter_count += 1
        chapter_id = chapter.get("id", "")
        chapter_label = chapter.get("label", "")

        # 2.1 chapter/@id - REQUIRED
        if not chapter_id:
            result.add_error(f"Chapter {chapter_count} missing @id attribute - REQUIRED")

        # 2.2 chapter/@label - REQUIRED for chapternumber
        if not chapter_label:
            result.add_error(
                f"Chapter '{chapter_id or chapter_count}' missing @label attribute - "
                "REQUIRED by AddRISInfo.xsl for chapternumber"
            )

        # 2.3 chapter/title - REQUIRED
        title = chapter.find("title")
        if title is None or not "".join(title.itertext()).strip():
            result.add_error(f"Chapter '{chapter_id or chapter_count}' missing <title> - REQUIRED")

    if chapter_count > 0:
        result.add_info(f"Found {chapter_count} chapter(s)")

    # =========================================================================
    # 3. SECT1 REQUIREMENTS (from RISChunker.xsl)
    # =========================================================================

    result.add_info("Checking sect1 requirements...")

    sect1s = root.findall(".//sect1")
    sect1_count = 0
    sect1_ids_seen: Set[str] = set()

    for sect1 in sect1s:
        sect1_count += 1
        sect1_id = sect1.get("id", "")

        # 3.1 sect1/@id - REQUIRED for chunking
        if not sect1_id:
            # Try to get parent chapter for context
            parent = sect1.getparent()
            parent_id = parent.get("id", "unknown") if parent is not None else "unknown"
            result.add_error(f"sect1 in '{parent_id}' missing @id attribute - REQUIRED for chunking")
        else:
            # 3.2 Check for spaces in ID (will be converted to underscores)
            if " " in sect1_id:
                result.add_warning(f"sect1 @id '{sect1_id}' contains spaces - will be converted to underscores")

            # 3.3 Check for duplicate IDs
            if sect1_id in sect1_ids_seen:
                result.add_error(f"Duplicate sect1 @id '{sect1_id}' - all IDs must be unique")
            sect1_ids_seen.add(sect1_id)

        # 3.4 sect1/title - REQUIRED
        title = sect1.find("title")
        if title is None or not "".join(title.itertext()).strip():
            result.add_error(
                f"sect1 '{sect1_id or sect1_count}' missing <title> - "
                "REQUIRED (TOC will show element name as fallback)"
            )

    if sect1_count > 0:
        result.add_info(f"Found {sect1_count} sect1 element(s)")

    # =========================================================================
    # 4. FRONTMATTER/BACKMATTER REQUIREMENTS (from toctransform.xsl)
    # =========================================================================

    result.add_info("Checking frontmatter/backmatter requirements...")

    # Elements that need @id for TOC generation
    toc_elements = [
        ("preface", "pf"),
        ("dedication", "de"),
        ("appendix", "ap"),
        ("glossary", "gl"),
        ("bibliography", "bi"),
        ("index", "in"),
    ]

    for elem_name, expected_prefix in toc_elements:
        elements = root.findall(f".//{elem_name}")
        for i, elem in enumerate(elements, 1):
            elem_id = elem.get("id", "")
            if not elem_id:
                result.add_error(f"{elem_name} #{i} missing @id attribute - REQUIRED for TOC generation")
            else:
                # Check ID prefix convention
                if not elem_id.startswith(expected_prefix):
                    result.add_warning(
                        f"{elem_name} @id '{elem_id}' doesn't use expected prefix '{expected_prefix}####' - "
                        "may affect TOC categorization"
                    )

    # =========================================================================
    # 5. PART/SUBPART REQUIREMENTS
    # =========================================================================

    parts = root.findall(".//part")
    for i, part in enumerate(parts, 1):
        part_id = part.get("id", "")
        if not part_id:
            result.add_error(f"part #{i} missing @id attribute - REQUIRED")
        elif not part_id.startswith("pt"):
            result.add_warning(f"part @id '{part_id}' doesn't use expected prefix 'pt####'")

    subparts = root.findall(".//subpart")
    for i, subpart in enumerate(subparts, 1):
        subpart_id = subpart.get("id", "")
        if not subpart_id:
            result.add_error(f"subpart #{i} missing @id attribute - REQUIRED")

    # =========================================================================
    # 6. IMAGE REQUIREMENTS (from graphics.ritt.xsl)
    # =========================================================================

    if check_images:
        result.add_info("Checking image references...")

        imagedata_elems = root.findall(".//imagedata[@fileref]")
        image_count = 0
        missing_images = []

        for imgdata in imagedata_elems:
            fileref = imgdata.get("fileref", "")
            if not fileref:
                continue

            image_count += 1

            # Check if fileref contains path (should be filename only)
            if "/" in fileref or "\\" in fileref:
                # Extract just filename for the warning
                filename = Path(fileref).name
                result.add_warning(
                    f"imagedata fileref '{fileref}' contains path - "
                    f"should be filename only: '{filename}'"
                )
                fileref = filename

            # Check if image exists (if image_files provided)
            if image_files is not None:
                if fileref not in image_files and fileref.lower() not in {f.lower() for f in image_files}:
                    missing_images.append(fileref)

        if image_count > 0:
            result.add_info(f"Found {image_count} image reference(s)")

        if missing_images:
            result.add_error(
                f"Missing images in MultiMedia folder: {', '.join(missing_images[:5])}"
                f"{f' and {len(missing_images)-5} more' if len(missing_images) > 5 else ''}"
            )

    # =========================================================================
    # 7. GLOBAL ID UNIQUENESS CHECK
    # =========================================================================

    result.add_info("Checking ID uniqueness...")

    all_ids: Set[str] = set()
    duplicate_ids: Set[str] = set()

    for elem in root.iter():
        if not isinstance(elem.tag, str):
            continue
        elem_id = elem.get("id", "")
        if elem_id:
            if elem_id in all_ids:
                duplicate_ids.add(elem_id)
            all_ids.add(elem_id)

    if duplicate_ids:
        result.add_error(f"Duplicate IDs found: {', '.join(sorted(duplicate_ids)[:10])}")
    else:
        result.add_info(f"All {len(all_ids)} IDs are unique [OK]")

    # =========================================================================
    # 8. IDREF VALIDATION (Java Xerces is strict about this!)
    # =========================================================================

    result.add_info("Checking IDREF validity (linkend references)...")

    broken_idrefs = []
    for elem in root.iter():
        if not isinstance(elem.tag, str):
            continue
        linkend = elem.get("linkend")
        if linkend and linkend not in all_ids:
            parent = elem.getparent()
            context = f"{elem.tag} in {parent.tag if parent is not None else 'root'}"
            broken_idrefs.append(f"linkend='{linkend}' ({context})")

    if broken_idrefs:
        result.add_error(
            f"Broken IDREF references (Java Xerces will reject these): "
            f"{', '.join(broken_idrefs[:5])}"
            f"{f' and {len(broken_idrefs)-5} more' if len(broken_idrefs) > 5 else ''}"
        )
    else:
        result.add_info("All linkend references are valid [OK]")

    # =========================================================================
    # 9. JAVA XSLT COMPATIBILITY CHECKS
    # =========================================================================

    result.add_info("Checking Java XSLT compatibility...")

    # 9.1 Check for XML declaration (Java expects it)
    # This is handled at parse time, but we can check the original file

    # 9.2 Check for problematic whitespace in critical elements
    # Java/XSLT may handle whitespace differently than Python
    for isbn_elem in root.findall(".//bookinfo/isbn"):
        isbn_text = isbn_elem.text or ""
        if isbn_text != isbn_text.strip():
            result.add_warning(
                f"ISBN has leading/trailing whitespace: '{isbn_text}' - "
                "Java may process this differently"
            )

    # 9.3 Check for empty elements that XSLT expects to have content
    critical_elements = [
        (".//chapter/title", "chapter title"),
        (".//sect1/title", "sect1 title"),
        (".//bookinfo/isbn", "bookinfo/isbn"),
    ]
    for xpath, name in critical_elements:
        for elem in root.findall(xpath):
            text_content = "".join(elem.itertext()).strip()
            if not text_content:
                parent = elem.getparent()
                parent_id = parent.get("id", "unknown") if parent is not None else "unknown"
                result.add_error(
                    f"Empty {name} in '{parent_id}' - "
                    "Java XSLT may fail or produce incorrect output"
                )

    # 9.4 Check for special characters in IDs that Java may not handle well
    for elem_id in all_ids:
        if any(c in elem_id for c in ['<', '>', '&', '"', "'"]):
            result.add_error(
                f"ID '{elem_id}' contains special characters - "
                "Java XML parser may reject or mishandle this"
            )

    return result


# =============================================================================
# RULE SET 1: Component Rules (preface/appendix content ordering)
# =============================================================================

def validate_component_rules(tree: etree._ElementTree) -> ValidationResult:
    """
    Validate DTD content model for component-level elements (preface, appendix, chapter).

    DTD Pattern: (divcomponent.mix+, sect1*) | sect1+

    This means:
    - Either: block content followed by optional sect1 elements
    - Or: one or more sect1 elements only
    - Once sect1 starts, NO MORE block content at component level

    Returns:
        ValidationResult with component ordering violations
    """
    result = ValidationResult()
    root = tree.getroot()

    result.add_info("Checking component rules (preface/appendix/chapter content ordering)...")

    # Block content elements from divcomponent.mix
    block_content_tags = {
        'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
        'screen', 'synopsis', 'address', 'blockquote', 'epigraph',
        'figure', 'informalfigure', 'table', 'informaltable',
        'example', 'informalexample', 'equation', 'informalequation',
        'procedure', 'sidebar', 'mediaobject', 'graphic',
        'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
        'segmentedlist', 'calloutlist', 'glosslist',
        'note', 'warning', 'caution', 'important', 'tip',
        'bridgehead', 'remark', 'highlights', 'abstract', 'authorblurb',
        'qandaset', 'anchor', 'indexterm', 'beginpage',
    }

    # Section elements
    section_tags = {'sect1', 'simplesect', 'section', 'risempty'}

    # Metadata/navigation elements (allowed anywhere)
    meta_tags = {'title', 'subtitle', 'titleabbrev', 'chapterinfo', 'prefaceinfo',
                 'appendixinfo', 'beginpage', 'tocchap', 'toc', 'lot', 'index',
                 'glossary', 'bibliography'}

    # Components to check
    component_tags = {'chapter', 'preface', 'appendix'}

    violations = []

    # Elements allowed before title per DTD
    before_title_tags = {'beginpage', 'prefaceinfo', 'chapterinfo', 'appendixinfo'}

    for component in root.iter():
        if not isinstance(component.tag, str):
            continue

        component_name = component.tag.split('}')[-1] if '}' in component.tag else component.tag
        if component_name not in component_tags:
            continue

        component_id = component.get('id', 'unknown')
        children = list(component)

        # Check 1: Find title position and verify nothing invalid comes before it
        title_idx = None
        for i, child in enumerate(children):
            child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if child_name == 'title':
                title_idx = i
                break

        if title_idx is None:
            violations.append(
                f"<{component_name} id='{component_id}'>: Missing required <title> element"
            )
        else:
            # Check for invalid elements before title
            for i in range(title_idx):
                child = children[i]
                child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if child_name not in before_title_tags:
                    violations.append(
                        f"<{component_name} id='{component_id}'>: "
                        f"<{child_name}> appears BEFORE <title> at position {i} - "
                        "DTD requires (beginpage?, *info?, title, ...) order"
                    )

        # Check 2: Find first section position
        first_sect_idx = None
        for i, child in enumerate(children):
            child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if child_name in section_tags:
                first_sect_idx = i
                break

        if first_sect_idx is None:
            continue  # No sections - any structure is valid

        # Check 3: Block content AFTER first section
        for i in range(first_sect_idx + 1, len(children)):
            child = children[i]
            child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag

            if child_name in section_tags:
                continue  # Sections are allowed
            if child_name in meta_tags:
                continue  # Metadata is allowed
            if child_name in block_content_tags:
                violations.append(
                    f"<{component_name} id='{component_id}'>: "
                    f"<{child_name}> appears AFTER sect1 at position {i} - "
                    "DTD violation (once sect1 starts, no more block content)"
                )

    if violations:
        for v in violations[:10]:  # Limit output
            result.add_error(v)
        if len(violations) > 10:
            result.add_error(f"... and {len(violations) - 10} more component ordering violations")
    else:
        result.add_info("Component content ordering is valid [OK]")

    return result


# =============================================================================
# RULE SET 2: Section Rules (sect1/sect2/sect3... ordering)
# =============================================================================

def validate_section_rules(tree: etree._ElementTree) -> ValidationResult:
    """
    Validate DTD content model for section elements at all levels.

    DTD Pattern for each section level:
    - sect1: (divcomponent.mix+, sect2*) | sect2+
    - sect2: (divcomponent.mix+, sect3*) | sect3+
    - sect3: (divcomponent.mix+, sect4*) | sect4+
    - sect4: (divcomponent.mix+, sect5*) | sect5+

    Rule: Once child sections start, NO MORE block content at parent level.

    Returns:
        ValidationResult with section ordering violations
    """
    result = ValidationResult()
    root = tree.getroot()

    result.add_info("Checking section rules (sect1->sect2->sect3... content ordering)...")

    # Block content elements
    block_content_tags = {
        'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
        'screen', 'synopsis', 'address', 'blockquote', 'epigraph',
        'figure', 'informalfigure', 'table', 'informaltable',
        'example', 'informalexample', 'equation', 'informalequation',
        'procedure', 'sidebar', 'mediaobject', 'graphic',
        'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
        'segmentedlist', 'calloutlist', 'glosslist',
        'note', 'warning', 'caution', 'important', 'tip',
        'bridgehead', 'remark', 'highlights', 'abstract', 'authorblurb',
        'qandaset', 'anchor', 'indexterm', 'beginpage',
    }

    # Define parent -> child section relationships
    section_hierarchy = [
        ('sect1', {'sect2', 'simplesect'}, {'title', 'subtitle', 'titleabbrev', 'sect1info', 'beginpage'}),
        ('sect2', {'sect3', 'simplesect'}, {'title', 'subtitle', 'titleabbrev', 'sect2info', 'beginpage'}),
        ('sect3', {'sect4', 'simplesect'}, {'title', 'subtitle', 'titleabbrev', 'sect3info', 'beginpage'}),
        ('sect4', {'sect5', 'simplesect'}, {'title', 'subtitle', 'titleabbrev', 'sect4info', 'beginpage'}),
    ]

    violations = []

    for parent_tag, child_section_tags, meta_tags in section_hierarchy:
        for parent in root.iter(parent_tag):
            parent_id = parent.get('id', 'unknown')
            children = list(parent)

            # Find first child section position
            first_child_sect_idx = None
            for i, child in enumerate(children):
                if not isinstance(child.tag, str):
                    continue  # Skip comments, processing instructions
                child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if child_name in child_section_tags:
                    first_child_sect_idx = i
                    break

            if first_child_sect_idx is None:
                continue  # No child sections - any structure is valid

            # Check for block content AFTER first child section
            for i in range(first_child_sect_idx + 1, len(children)):
                child = children[i]
                if not isinstance(child.tag, str):
                    continue  # Skip comments, processing instructions
                child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag

                if child_name in child_section_tags:
                    continue  # Child sections are allowed
                if child_name in meta_tags:
                    continue  # Metadata is allowed
                if child_name in block_content_tags:
                    # Get first child section name for better error message
                    first_child = children[first_child_sect_idx]
                    first_child_name = first_child.tag.split('}')[-1] if '}' in first_child.tag else first_child.tag
                    violations.append(
                        f"<{parent_tag} id='{parent_id}'>: "
                        f"<{child_name}> appears AFTER <{first_child_name}> at position {i} - "
                        f"DTD violation (once {first_child_name} starts, no more block content in {parent_tag})"
                    )

    if violations:
        for v in violations[:10]:  # Limit output
            result.add_error(v)
        if len(violations) > 10:
            result.add_error(f"... and {len(violations) - 10} more section ordering violations")
    else:
        result.add_info("Section content ordering is valid [OK]")

    return result


# =============================================================================
# RULE SET 3: Post-Split Robustness (linker safety + missing node guards)
# =============================================================================

def validate_post_split_robustness(tree: etree._ElementTree) -> ValidationResult:
    """
    Validate that XML will remain valid after RISChunker.xsl splits it.

    RISChunker.xsl splits:
    - book//chapter/sect1 -> separate files
    - book/appendix, book/preface, book/toc, book/dedication -> separate files

    This validation checks:
    1. Linker safety: Cross-references (linkend) should reference IDs that will
       be in the same split file OR use proper external references
    2. Missing node guards: Required elements must be present in each split unit
    3. ID format: IDs must be valid for file naming (no spaces, special chars)

    Returns:
        ValidationResult with post-split robustness issues
    """
    result = ValidationResult()
    root = tree.getroot()

    result.add_info("Checking post-split robustness (linker safety + missing nodes)...")

    # Collect all IDs and their containing split unit
    id_to_split_unit: dict = {}  # id -> split_unit_id
    split_units: dict = {}  # split_unit_id -> set of IDs in that unit

    # Find all split units (sect1 in chapters, appendix, preface, etc.)

    # 1. sect1 elements in chapters (these become separate files)
    for chapter in root.findall('.//chapter'):
        chapter_id = chapter.get('id', 'unknown_chapter')
        for sect1 in chapter.findall('./sect1'):
            sect1_id = sect1.get('id')
            if sect1_id:
                split_units[sect1_id] = set()
                # Collect all IDs within this sect1
                for elem in sect1.iter():
                    elem_id = elem.get('id')
                    if elem_id:
                        id_to_split_unit[elem_id] = sect1_id
                        split_units[sect1_id].add(elem_id)

    # 2. Appendix, preface, toc, dedication (these become separate files)
    for tag in ['appendix', 'preface', 'toc', 'dedication']:
        for elem in root.findall(f'.//{tag}'):
            elem_id = elem.get('id')
            if elem_id:
                split_units[elem_id] = set()
                for child in elem.iter():
                    child_id = child.get('id')
                    if child_id:
                        id_to_split_unit[child_id] = elem_id
                        split_units[elem_id].add(child_id)

    # 3. Book-level IDs (stay in Book.XML)
    book_level_ids = set()
    for elem in root.iter():
        elem_id = elem.get('id')
        if elem_id and elem_id not in id_to_split_unit:
            book_level_ids.add(elem_id)
            id_to_split_unit[elem_id] = '__BOOK__'

    violations = []
    warnings = []

    # ==========================================================================
    # CHECK 1: Cross-reference safety (linkend references)
    # ==========================================================================

    for elem in root.iter():
        if not isinstance(elem.tag, str):
            continue

        linkend = elem.get('linkend')
        if not linkend:
            continue

        # Find which split unit this element is in
        source_unit = None
        parent = elem
        while parent is not None:
            parent_id = parent.get('id')
            if parent_id and parent_id in split_units:
                source_unit = parent_id
                break
            parent = parent.getparent()

        if source_unit is None:
            source_unit = '__BOOK__'

        # Check if target is in the same split unit
        target_unit = id_to_split_unit.get(linkend)

        if target_unit is None:
            # Broken reference - already caught by IDREF validation
            continue

        if target_unit != source_unit and target_unit != '__BOOK__':
            # Cross-unit reference - after split, this becomes a cross-file reference
            # The XSLT should handle this, but let's warn
            warnings.append(
                f"Cross-file reference: linkend='{linkend}' in '{source_unit}' "
                f"references ID in '{target_unit}' - verify XSLT handles this"
            )

    # ==========================================================================
    # CHECK 2: Missing node guards (required elements in each split unit)
    # ==========================================================================

    # Each sect1 that becomes a file must have a title
    for chapter in root.findall('.//chapter'):
        for sect1 in chapter.findall('./sect1'):
            sect1_id = sect1.get('id', 'unknown')
            title = sect1.find('title')
            if title is None:
                violations.append(
                    f"<sect1 id='{sect1_id}'> missing <title> - "
                    "required for split file (will show as blank in TOC)"
                )
            elif not "".join(title.itertext()).strip():
                violations.append(
                    f"<sect1 id='{sect1_id}'> has empty <title> - "
                    "will show as blank in TOC after split"
                )

    # Each appendix/preface must have required elements
    for tag in ['appendix', 'preface']:
        for elem in root.findall(f'.//{tag}'):
            elem_id = elem.get('id', 'unknown')

            # Must have title
            title = elem.find('title')
            if title is None:
                violations.append(
                    f"<{tag} id='{elem_id}'> missing <title> - required for split file"
                )

            # Must have either content or sect1
            has_content = False
            for child in elem:
                child_name = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if child_name not in {'title', 'subtitle', 'titleabbrev', f'{tag}info'}:
                    has_content = True
                    break

            if not has_content:
                violations.append(
                    f"<{tag} id='{elem_id}'> has no content (only title/metadata) - "
                    "split file will be essentially empty"
                )

    # ==========================================================================
    # CHECK 3: ID format for file naming
    # ==========================================================================

    # RISChunker uses IDs in filenames: translate(@id,' ','_')
    # But other special characters can cause issues

    problematic_chars = set('<>:"/\\|?*')  # Invalid in Windows filenames

    for split_id in split_units.keys():
        if any(c in split_id for c in problematic_chars):
            violations.append(
                f"ID '{split_id}' contains characters invalid for filenames - "
                "RISChunker will create problematic file names"
            )
        if split_id.startswith('.') or split_id.startswith('-'):
            warnings.append(
                f"ID '{split_id}' starts with '.' or '-' - "
                "may cause issues with file handling"
            )

    # Report results
    if violations:
        for v in violations[:10]:
            result.add_error(v)
        if len(violations) > 10:
            result.add_error(f"... and {len(violations) - 10} more post-split issues")

    if warnings:
        for w in warnings[:5]:
            result.add_warning(w)
        if len(warnings) > 5:
            result.add_warning(f"... and {len(warnings) - 5} more cross-reference warnings")

    if not violations and not warnings:
        result.add_info(f"Post-split robustness checks passed [OK] ({len(split_units)} split units)")

    return result


def validate_entity_declarations(book_xml_path: Path) -> ValidationResult:
    """
    Validate Book.XML for duplicate ENTITY declarations.

    The DOCTYPE section of Book.XML contains entity declarations like:
        <!ENTITY ch0001 SYSTEM "ch0001.xml">
        <!ENTITY ch0002 SYSTEM "ch0002.xml">

    If two elements share the same prefix and have separate counters, they may
    generate duplicate entity names (e.g., two ch0001 declarations), which causes
    XML parsing failures downstream.

    Args:
        book_xml_path: Path to Book.XML file

    Returns:
        ValidationResult with any duplicate entity errors
    """
    result = ValidationResult()

    if not book_xml_path.exists():
        result.add_error(f"Book.XML not found: {book_xml_path}")
        return result

    try:
        # Read raw content to parse DOCTYPE entities
        content = book_xml_path.read_text(encoding="utf-8")

        # Extract DOCTYPE section - look for internal subset between [ and ]
        doctype_match = re.search(r"<!DOCTYPE\s+\w+[^[]*\[(.*?)\]>", content, re.DOTALL)
        if not doctype_match:
            result.add_info("No internal DTD subset found in Book.XML")
            return result

        doctype_content = doctype_match.group(1)

        # Find all ENTITY declarations: <!ENTITY name SYSTEM "file.xml">
        entity_pattern = re.compile(r"<!ENTITY\s+(\w+)\s+SYSTEM\s+", re.IGNORECASE)
        entities = entity_pattern.findall(doctype_content)

        if not entities:
            result.add_info("No entity declarations found in Book.XML")
            return result

        # Check for duplicates
        seen = {}
        duplicates = []
        for entity_name in entities:
            if entity_name in seen:
                seen[entity_name] += 1
                if seen[entity_name] == 2:  # Only add to duplicates on first duplicate
                    duplicates.append(entity_name)
            else:
                seen[entity_name] = 1

        if duplicates:
            for dup in duplicates:
                count = seen[dup]
                result.add_error(
                    f"Duplicate ENTITY declaration: '{dup}' appears {count} times in Book.XML. "
                    f"This will cause XML parsing failures. Check ID generation - elements with "
                    f"same prefix must share counter."
                )
        else:
            result.add_info(f"Entity declarations valid: {len(entities)} unique entities [OK]")

    except Exception as e:
        result.add_error(f"Failed to validate entity declarations: {e}")

    return result


def validate_invalid_element_nesting(tree: etree._ElementTree) -> ValidationResult:
    """
    Validate that block-level elements are not nested inside inline containers.

    Per DocBook DTD, certain elements like beginpage, figure, table, etc.
    cannot appear inside para, title, emphasis, or other inline elements.

    This catches issues that will fail DTD validation like:
    - <para>text <beginpage/> more text</para>  (INVALID)
    - <title>Title <figure>...</figure></title>  (INVALID)

    Args:
        tree: Parsed XML tree

    Returns:
        ValidationResult with findings
    """
    result = ValidationResult()
    result.add_info("Checking for invalid element nesting (block elements inside inline containers)...")

    root = tree.getroot()

    # Inline containers that should NOT contain block-level elements
    # These elements only allow inline content per DTD
    inline_containers = {
        'para', 'simpara', 'formalpara',  # Paragraph elements
        'title', 'subtitle', 'titleabbrev',  # Title elements
        'emphasis', 'phrase', 'literal', 'code', 'computeroutput',  # Inline formatting
        'firstterm', 'glossterm', 'foreignphrase', 'wordasword',  # Inline semantic
        'link', 'ulink', 'xref', 'olink',  # Link elements
        'citetitle', 'citation', 'quote',  # Citation elements
        'subscript', 'superscript',  # Script elements
        'trademark', 'productname', 'corpname', 'orgname',  # Name elements
        'author', 'editor', 'othercredit',  # Credit elements (inline context)
        'term',  # Definition list term
    }

    # Block-level elements that should NOT appear inside inline containers
    # These are elements that require block-level context per DTD
    block_elements = {
        # Page markers
        'beginpage',
        # Figures and media
        'figure', 'informalfigure', 'mediaobject', 'inlinemediaobject',
        'graphic', 'screenshot',
        # Tables
        'table', 'informaltable',
        # Lists
        'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
        'segmentedlist', 'calloutlist', 'glosslist', 'bibliolist',
        # Examples and equations
        'example', 'informalexample', 'equation', 'informalequation',
        # Code blocks
        'programlisting', 'literallayout', 'screen', 'synopsis',
        # Admonitions
        'note', 'warning', 'caution', 'important', 'tip',
        # Other block elements
        'blockquote', 'epigraph', 'sidebar', 'procedure', 'qandaset',
        'address', 'bridgehead', 'highlights', 'abstract',
        # Sections (definitely block)
        'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect',
    }

    # Special cases: some elements are allowed in specific inline contexts
    # For example, inlinemediaobject IS allowed inside para
    inline_allowed_in_para = {'inlinemediaobject', 'anchor', 'indexterm'}

    violations = []

    # Check all inline containers for invalid block children
    for container in root.iter():
        if not isinstance(container.tag, str):
            continue

        container_tag = container.tag.split('}')[-1] if '}' in container.tag else container.tag

        if container_tag not in inline_containers:
            continue

        # Check all descendants (not just direct children) for block elements
        for descendant in container.iter():
            if descendant is container:
                continue  # Skip self

            if not isinstance(descendant.tag, str):
                continue

            desc_tag = descendant.tag.split('}')[-1] if '}' in descendant.tag else descendant.tag

            # Check if this is a block element
            if desc_tag in block_elements:
                # Check for special allowed cases
                if container_tag == 'para' and desc_tag in inline_allowed_in_para:
                    continue

                # Get parent info for better error message
                parent = container.getparent()
                parent_id = parent.get('id', 'unknown') if parent is not None else 'root'
                parent_tag = parent.tag if parent is not None else 'root'
                if isinstance(parent_tag, str) and '}' in parent_tag:
                    parent_tag = parent_tag.split('}')[-1]

                violations.append(
                    f"<{desc_tag}> inside <{container_tag}> in <{parent_tag} id='{parent_id}'>: "
                    f"DTD does not allow block element <{desc_tag}> inside inline container <{container_tag}>"
                )

    if violations:
        # Group by violation type for cleaner output
        violation_counts = {}
        for v in violations:
            # Extract the element types for grouping
            key = v.split(':')[0].split(' in ')[0]  # e.g., "<beginpage> inside <para>"
            violation_counts[key] = violation_counts.get(key, 0) + 1

        for violation_type, count in sorted(violation_counts.items(), key=lambda x: -x[1]):
            result.add_error(f"{violation_type}: {count} occurrence(s) - will fail DTD validation")

        # Show first few detailed examples
        for v in violations[:5]:
            result.add_error(f"  Example: {v}")
        if len(violations) > 5:
            result.add_error(f"  ... and {len(violations) - 5} more invalid nesting violations")
    else:
        result.add_info("No invalid element nesting found [OK]")

    return result


def validate_package_for_loadbook(zip_path: Path) -> ValidationResult:
    """
    Validate a ZIP package for LoadBook.bat compatibility.

    This performs both DTD validation and LoadBook/R2Utilities requirements.

    Args:
        zip_path: Path to ZIP package

    Returns:
        ValidationResult with all findings
    """
    result = ValidationResult()

    if not zip_path.exists():
        result.add_error(f"Package not found: {zip_path}")
        return result

    # Check ZIP filename matches ISBN format
    zip_stem = zip_path.stem
    if not re.match(r"^\d{10,13}$", zip_stem):
        result.add_warning(
            f"ZIP filename '{zip_path.name}' may not match expected ISBN format - "
            "LoadBook.bat expects {{ISBN}}.zip"
        )

    try:
        with tempfile.TemporaryDirectory(prefix="validate_lb_") as tmp:
            extract_dir = Path(tmp)

            # Extract ZIP
            with zipfile.ZipFile(zip_path, "r") as zf:
                zf.extractall(extract_dir)
                namelist = zf.namelist()

            # Check for Book.XML (case-sensitive check)
            book_xml = extract_dir / "Book.XML"
            book_xml_lower = extract_dir / "book.xml"

            if book_xml.exists():
                result.add_info("Book.XML found at root [OK]")
            elif book_xml_lower.exists():
                result.add_warning("Found 'book.xml' but expected 'Book.XML' (case may matter)")
                book_xml = book_xml_lower
            else:
                result.add_error("Book.XML not found in package root")
                return result

            # Check for MultiMedia folder
            multimedia_dir = extract_dir / "MultiMedia"
            if multimedia_dir.exists() and multimedia_dir.is_dir():
                image_files = {f.name for f in multimedia_dir.iterdir() if f.is_file()}
                result.add_info(f"MultiMedia folder found with {len(image_files)} file(s) [OK]")
            else:
                # Check for alternate casing
                for item in extract_dir.iterdir():
                    if item.is_dir() and item.name.lower() == "multimedia":
                        result.add_warning(f"Found '{item.name}' but expected 'MultiMedia' (case may matter)")
                        image_files = {f.name for f in item.iterdir() if f.is_file()}
                        break
                else:
                    result.add_warning("MultiMedia folder not found - images may not be included")
                    image_files = set()

            # Check for chapter XML files
            chapter_files = [f for f in namelist if re.match(r"ch\d{4}\.xml$", f, re.IGNORECASE)]
            if chapter_files:
                result.add_info(f"Found {len(chapter_files)} chapter file(s)")

            # Validate entity declarations for duplicates (critical check)
            entity_result = validate_entity_declarations(book_xml)
            result.merge(entity_result)

            # Parse and validate Book.XML
            try:
                parser = etree.XMLParser(load_dtd=False, resolve_entities=False)
                tree = etree.parse(str(book_xml), parser)

                # Run LoadBook requirements validation
                lb_result = validate_loadbook_requirements(tree, check_images=True, image_files=image_files)
                result.merge(lb_result)

                # Run the three rule sets for preface/appendix issues
                # Rule Set 1: Component rules (preface/appendix content ordering)
                component_result = validate_component_rules(tree)
                result.merge(component_result)

                # Rule Set 2: Section rules (sect1->sect2 ordering)
                section_result = validate_section_rules(tree)
                result.merge(section_result)

                # Rule Set 3: Post-split robustness (linker safety + missing nodes)
                split_result = validate_post_split_robustness(tree)
                result.merge(split_result)

                # Rule Set 4: Invalid element nesting (block elements inside inline)
                # This catches issues like <beginpage> inside <para> that fail DTD
                nesting_result = validate_invalid_element_nesting(tree)
                result.merge(nesting_result)

                # Also check ISBN in XML matches ZIP filename
                isbn_elem = tree.find(".//bookinfo/isbn")
                if isbn_elem is not None and isbn_elem.text:
                    xml_isbn = re.sub(r"[^0-9X]", "", isbn_elem.text.upper())
                    if xml_isbn != zip_stem and xml_isbn not in zip_stem:
                        result.add_warning(
                            f"ISBN in Book.XML ({xml_isbn}) may not match ZIP filename ({zip_stem})"
                        )

            except etree.XMLSyntaxError as e:
                result.add_error(f"XML parsing failed: {e}")

    except zipfile.BadZipFile:
        result.add_error(f"Invalid ZIP file: {zip_path}")
    except Exception as e:
        result.add_error(f"Package validation error: {e}")

    return result


def validate_xml_file(xml_path: Path, dtd_path: Path) -> tuple[bool, str]:
    """
    Validate an XML file against a DTD.

    Args:
        xml_path: Path to XML file
        dtd_path: Path to DTD file

    Returns:
        Tuple of (is_valid, report_message)
    """
    if not xml_path.exists():
        return False, f"XML file not found: {xml_path}"

    if not dtd_path.exists():
        return False, f"DTD file not found: {dtd_path}"

    try:
        # Parse XML with DTD loading enabled
        parser = etree.XMLParser(load_dtd=True, resolve_entities=True, no_network=True)
        tree = etree.parse(str(xml_path), parser)

        # Load DTD
        dtd = etree.DTD(str(dtd_path))

        # Validate
        is_valid = dtd.validate(tree)

        if is_valid:
            return True, "DTD validation passed [OK]"
        else:
            error_lines = "\n".join(f"  - {err}" for err in dtd.error_log)
            return False, f"DTD validation failed:\n{error_lines}"

    except etree.XMLSyntaxError as e:
        return False, f"XML parsing failed: {e}"
    except Exception as e:
        return False, f"Validation error: {e}"


def validate_package(zip_path: Path, dtd_path: Path) -> tuple[bool, str]:
    """
    Validate Book.XML inside a ZIP package against the RittDoc DTD.

    Args:
        zip_path: Path to ZIP package
        dtd_path: Path to DTD file (falls back to package DTD if available)

    Returns:
        Tuple of (is_valid, report_message)
    """
    if not zip_path.exists():
        return False, f"Package not found: {zip_path}"

    try:
        with tempfile.TemporaryDirectory(prefix="validate_") as tmp:
            extract_dir = Path(tmp)

            # Extract ZIP
            with zipfile.ZipFile(zip_path, "r") as zf:
                zf.extractall(extract_dir)

            # Find Book.XML
            book_xml = extract_dir / "Book.XML"
            if not book_xml.exists():
                return False, "Book.XML not found in package"

            # Try to use DTD from package first
            package_dtd = extract_dir / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
            if package_dtd.exists():
                dtd_to_use = package_dtd
                print(f"Using DTD from package: {package_dtd}")
            else:
                dtd_to_use = dtd_path
                print(f"Using external DTD: {dtd_path}")

            # Validate
            return validate_xml_file(book_xml, dtd_to_use)

    except zipfile.BadZipFile:
        return False, f"Invalid ZIP file: {zip_path}"
    except Exception as e:
        return False, f"Package validation error: {e}"


def transform_and_validate(
    input_xml: Path,
    output_xml: Path,
    dtd_path: Path
) -> tuple[bool, str]:
    """
    Apply XSLT transformation and validate the result.

    Args:
        input_xml: Path to input XML file
        output_xml: Path to write transformed XML
        dtd_path: Path to DTD file for validation

    Returns:
        Tuple of (is_valid, report_message)
    """
    if not input_xml.exists():
        return False, f"Input XML not found: {input_xml}"

    try:
        # Apply XSLT transformation
        print(f"Transforming {input_xml} -> {output_xml}")
        transform_to_rittdoc_compliance(input_xml, output_xml)
        print("[OK] XSLT transformation completed")

        # Validate transformed XML
        print(f"Validating against DTD: {dtd_path}")
        is_valid, message = validate_xml_file(output_xml, dtd_path)

        if is_valid:
            return True, f"Transformation and validation successful [OK]\nOutput: {output_xml}\n{message}"
        else:
            return False, f"Transformation completed but validation failed:\n{message}"

    except Exception as e:
        return False, f"Transformation error: {e}"


def main():
    parser = argparse.ArgumentParser(
        description="Validate RittDoc XML packages and files against RittDoc DTD",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  Validate a ZIP package (DTD only):
    python validate_rittdoc.py Output/mybook.zip

  Validate for LoadBook/R2Utilities compatibility (RECOMMENDED):
    python validate_rittdoc.py --loadbook Output/9781234567890.zip

  Validate an XML file:
    python validate_rittdoc.py book.xml

  Transform and validate:
    python validate_rittdoc.py --transform input.xml output.xml

  Use custom DTD:
    python validate_rittdoc.py --dtd custom.dtd book.xml

LoadBook Validation Checks:
  The --loadbook option validates against LoadBook.bat and R2Utilities requirements:
  - Book-level: isbn, title, author/editor (REQUIRED - will fail in RISBackend!)
  - Chapter: @id, @label (for chapternumber), title
  - Sect1: @id (for chunking), title (for TOC)
  - Frontmatter/Backmatter: @id with correct prefix (ap, gl, bi, in, pf, de)
  - Images: fileref format, existence in MultiMedia folder
  - Global: ID uniqueness across entire document
        """
    )

    parser.add_argument(
        "input",
        help="Input file (ZIP package or XML file)"
    )
    parser.add_argument(
        "output",
        nargs="?",
        help="Output file (required with --transform)"
    )
    parser.add_argument(
        "--transform",
        action="store_true",
        help="Apply XSLT transformation before validation"
    )
    parser.add_argument(
        "--loadbook",
        action="store_true",
        help="Validate for LoadBook.bat/R2Utilities compatibility (recommended for ZIP packages)"
    )
    parser.add_argument(
        "--dtd",
        help="Path to DTD file (default: RITTDOCdtd/v1.1/RittDocBook.dtd)"
    )
    parser.add_argument(
        "--verbose", "-v",
        action="store_true",
        help="Show detailed validation output including INFO messages"
    )
    parser.add_argument(
        "--quiet", "-q",
        action="store_true",
        help="Only show errors (suppress warnings and info)"
    )

    args = parser.parse_args()

    # Set up DTD path
    if args.dtd:
        dtd_path = Path(args.dtd).resolve()
    else:
        dtd_path = DTD_PATH

    input_path = Path(args.input).resolve()

    # Handle LoadBook validation mode
    if args.loadbook:
        if input_path.suffix.lower() != ".zip":
            print("Error: --loadbook requires a ZIP package", file=sys.stderr)
            sys.exit(1)

        print(f"Validating package for LoadBook/R2Utilities: {input_path}")
        print("=" * 70)

        result = validate_package_for_loadbook(input_path)

        # Print results
        print("\n" + "=" * 70)
        if result.passed:
            print("LOADBOOK VALIDATION: PASSED [OK]")
            print(f"  {len(result.errors)} errors, {len(result.warnings)} warnings")
        else:
            print("LOADBOOK VALIDATION: FAILED [FAIL]")
            print(f"  {len(result.errors)} errors, {len(result.warnings)} warnings")
        print("=" * 70)

        # Print details based on verbosity
        if args.quiet:
            # Only errors
            if result.errors:
                print("\nERRORS:")
                for e in result.errors:
                    print(f"  [FAIL] {e}")
        elif args.verbose:
            # Everything
            print(result.to_report())
        else:
            # Errors and warnings (default)
            if result.errors:
                print("\nERRORS:")
                for e in result.errors:
                    print(f"  [FAIL] {e}")
            if result.warnings:
                print("\nWARNINGS:")
                for w in result.warnings:
                    print(f"  [WARN] {w}")

        sys.exit(0 if result.passed else 1)

    # Handle transformation mode
    if args.transform:
        if not args.output:
            print("Error: --transform requires both input and output arguments", file=sys.stderr)
            sys.exit(1)

        if not dtd_path.exists():
            print(f"Error: DTD file not found: {dtd_path}", file=sys.stderr)
            sys.exit(1)

        output_path = Path(args.output).resolve()
        is_valid, message = transform_and_validate(input_path, output_path, dtd_path)

    # Handle package validation (DTD only)
    elif input_path.suffix.lower() == ".zip":
        if not dtd_path.exists():
            print(f"Error: DTD file not found: {dtd_path}", file=sys.stderr)
            sys.exit(1)

        print(f"Validating package: {input_path}")
        print("(Tip: Use --loadbook for full LoadBook/R2Utilities compatibility checks)")
        is_valid, message = validate_package(input_path, dtd_path)

    # Handle XML file validation
    else:
        if not dtd_path.exists():
            print(f"Error: DTD file not found: {dtd_path}", file=sys.stderr)
            sys.exit(1)

        print(f"Validating XML file: {input_path}")
        is_valid, message = validate_xml_file(input_path, dtd_path)

    # Print results for non-loadbook modes
    print("\n" + "=" * 70)
    if is_valid:
        print("VALIDATION RESULT: PASSED [OK]")
    else:
        print("VALIDATION RESULT: FAILED [FAIL]")
    print("=" * 70)

    if args.verbose or not is_valid:
        print(message)

    sys.exit(0 if is_valid else 1)


if __name__ == "__main__":
    main()
