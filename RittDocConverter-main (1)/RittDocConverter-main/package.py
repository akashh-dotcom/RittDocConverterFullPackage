from __future__ import annotations

import csv
import hashlib
import logging
import re
import shutil
import string
import tempfile
import zipfile
from copy import deepcopy
from dataclasses import dataclass
from io import BytesIO
from pathlib import Path
from typing import (Any, Callable, Dict, Iterable, List, Optional, Sequence,
                    Set, Tuple)

from lxml import etree
from PIL import Image

logger = logging.getLogger(__name__)

# Reuse hierarchical section ID generation for fragments that need IDs (consolidated in id_authority)
from id_authority import generate_section_id as id_gen_section_id

# Import reference mapper for tracking resource transformations
try:
    from reference_mapper import ReferenceMapper, get_mapper
    HAS_REFERENCE_MAPPER = True
except ImportError:
    HAS_REFERENCE_MAPPER = False

# Import centralized ID Authority for ID management
try:
    from id_authority import (
        get_authority, VALID_CHAPTER_PREFIXES, CHAPTER_PATTERN
    )
    HAS_ID_AUTHORITY = True
except ImportError:
    HAS_ID_AUTHORITY = False
    get_authority = None
    VALID_CHAPTER_PREFIXES = set()
    CHAPTER_PATTERN = None

# Import TOC cleanup for title-based front/back matter reclassification
try:
    from cleanup_toc import process_zip_file as cleanup_toc_in_zip
    HAS_TOC_CLEANUP = True
except ImportError:
    HAS_TOC_CLEANUP = False
    logger.debug("cleanup_toc module not available - TOC reclassification will be skipped")

BOOK_DOCTYPE_PUBLIC_DEFAULT = "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN"
BOOK_DOCTYPE_SYSTEM_DEFAULT = "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd"

FRONT_SECTION_TYPES = {
    "cover",
    "titlepage",
    "title-page",
    "halftitle",
    "half-title",
    "copyright",
    "copyright-page",
    "dedication",
    "epigraph",
    "foreword",
    "preface",
    "acknowledgments",
    "acknowledgements",
    "introduction",
    "prologue",
    "contributors",
    "other-credits",
    "credits",
}

BACK_SECTION_TYPES = {
    "appendix",
    "glossary",
    "bibliography",
    "index",
    "colophon",
    "epilogue",
    "afterword",
}

WILEY_PUBLISHER_ALIASES = {
    "john wiley & sons",
    "john wiley and sons",
    "wiley",
    "wiley-blackwell",
    "wiley blackwell",
    "wiley publishing",
}

WILEY_EULA_TITLE_SNIPPETS = {
    "eula",
    "end user license agreement",
    "end user licence agreement",
}

WILEY_INDEX_TITLES = {
    "index",
    "subject index",
    "author index",
    "name index",
}


MediaFetcher = Callable[[str], Optional[bytes]]


@dataclass
class ChapterFragment:
    """Representation of an extracted chapter fragment."""

    entity: str
    filename: str
    element: etree._Element
    kind: str = "chapter"
    title: str = ""
    section_type: str = ""


@dataclass
class ImageMetadata:
    """Captured metadata for a content image."""

    filename: str
    original_filename: str
    chapter: str
    figure_number: str
    caption: str
    alt_text: str
    referenced_in_text: bool
    width: int
    height: int
    file_size: str
    format: str



def _local_name(element: etree._Element) -> str:
    tag = element.tag
    if not isinstance(tag, str):
        return ""
    if tag.startswith("{"):
        return tag.split("}", 1)[1]
    return tag


def _is_chapter_node(element: etree._Element) -> bool:
    tag = _local_name(element)
    return tag in {
        "chapter",
        "appendix",
        "part",
        "subpart",
        "article",
        "index",
        "preface",
        "glossary",
        "dedication",
        "bibliography",
        "colophon",
        "acknowledgments",
    }


def _is_toc_node(element: etree._Element) -> bool:
    if _local_name(element) != "chapter":
        return False
    role = (element.get("role") or "").lower()
    if role.startswith("toc"):
        return True
    title = element.find("title")
    if title is not None:
        text = "".join(title.itertext()).strip().lower()
        if text == "table of contents":
            return True
    return False


_CHAPTER_PREFIX_RE = re.compile(r'^([a-z]{2}\d{4})')


def _dedupe_fragments_by_entity(
    fragments: Sequence["ChapterFragment"],
) -> List["ChapterFragment"]:
    """Ensure each entity id appears only once (preserve first occurrence)."""
    seen: Set[str] = set()
    unique: List["ChapterFragment"] = []
    duplicates: List[str] = []

    for fragment in fragments:
        entity_id = fragment.entity
        if entity_id in seen:
            duplicates.append(entity_id)
            continue
        seen.add(entity_id)
        unique.append(fragment)

    if duplicates:
        logger.warning(
            "Duplicate entity ids detected in fragments: %s",
            ", ".join(sorted(set(duplicates))),
        )

    return unique


def _dedupe_entity_references(book_root: etree._Element) -> int:
    """Remove duplicate entity references in Book.XML to avoid ID collisions.

    Prefers keeping entity references that live inside <part>/<subpart>
    wrappers over those at the book root level.
    """
    removed = 0

    # First, find entity names that already appear inside part/subpart wrappers.
    part_entity_names: Set[str] = set()
    for elem in book_root.iter():
        if not isinstance(elem.tag, str):
            continue
        if _local_name(elem) not in {"part", "subpart"}:
            continue
        for node in elem.iter():
            if isinstance(node, etree._Entity):
                name = getattr(node, "name", None)
                if name:
                    part_entity_names.add(name)

    # Prefer keeping part/subpart references; remove duplicates at book root.
    for child in list(book_root):
        if isinstance(child, etree._Entity):
            name = getattr(child, "name", None)
            if name in part_entity_names:
                book_root.remove(child)
                removed += 1

    # Remove any remaining duplicates (keep first occurrence).
    seen: Set[str] = set()
    for parent in book_root.iter():
        for child in list(parent):
            if not isinstance(child, etree._Entity):
                continue
            name = getattr(child, "name", None)
            if not name:
                continue
            if name in seen:
                parent.remove(child)
                removed += 1
                continue
            seen.add(name)

    if removed:
        logger.warning("Removed %d duplicate entity reference(s) from Book.XML", removed)

    return removed


def _convert_cross_file_linkends_to_ulink(
    fragments: List["ChapterFragment"],
) -> None:
    """
    Convert cross-file linkend references to <ulink url="file#id">.

    Each fragment will become a standalone XML file validated against the DTD.
    The DTD's linkend attribute is IDREF, which must reference an id within the
    *same* document.  Any linkend whose target id lives in a different fragment
    file therefore causes a validation error.

    This function detects such cross-file linkend values and rewrites the
    element from ``<link linkend="xx">`` / ``<xref linkend="xx"/>`` to
    ``<ulink url="target#xx">``, which uses a CDATA url attribute and is
    valid in standalone files.

    For TOC fragments, use just the ID without the filename prefix since TOC
    entries are rendered differently by the XSL.
    """
    # Build per-fragment id sets and a global id->filename lookup.
    id_to_file: Dict[str, str] = {}
    fragment_ids: Dict[str, Set[str]] = {}  # entity_id -> set of ids

    for frag in fragments:
        ids_in_frag: Set[str] = set()
        for elem in frag.element.iter():
            eid = elem.get("id")
            if eid:
                ids_in_frag.add(eid)
                id_to_file[eid] = frag.filename
        fragment_ids[frag.entity] = ids_in_frag

    converted = 0
    for frag in fragments:
        local_ids = fragment_ids[frag.entity]
        # Always use simplified URL format (without .xml extension) for internal
        # cross-references. All fragment types (TOC, preface, chapters, appendices,
        # etc.) should generate URLs like "ch0002#ch0002" not "ch0002.xml#ch0002".
        # The R2 platform XSL expects this format for proper link resolution.

        # Detect self-remap prefix to avoid double-remapping
        frag_prefix_match = re.match(r'^([a-z]{2}\d{4})', frag.entity)
        frag_prefix = frag_prefix_match.group(1) if frag_prefix_match else None

        for elem in list(frag.element.iter()):
            if not isinstance(elem.tag, str):
                continue
            linkend = elem.get("linkend")
            if not linkend:
                continue
            # If linkend resolves to an id in the same fragment, keep it.
            if linkend in local_ids:
                continue
            # Determine target filename from the linkend prefix (e.g. ch0031).
            target_file = id_to_file.get(linkend)
            if not target_file:
                prefix_m = _CHAPTER_PREFIX_RE.match(linkend)
                if prefix_m:
                    target_file = f"{prefix_m.group(1)}.xml"
                else:
                    # Cannot determine target – leave as-is; DTD fixer may
                    # convert to phrase later.
                    continue

            parent = elem.getparent()
            if parent is None:
                continue

            # Cross-chapter references: convert to <ulink> without .xml extension
            # e.g., "ch0002#ch0002s0001fg04" not "ch0002.xml#ch0002s0001fg04"
            file_base = target_file.replace(".xml", "")

            # Skip self-remap (already done) to prevent double-remapping
            old_pfx = re.match(r'^([a-z]{2}\d{4})', linkend)
            if old_pfx and old_pfx.group(1) == frag_prefix:
                continue

            # If linkend is a bare chapter ID (e.g., "ch0002") with no section,
            # point to the first section (ch0002s0001) for proper navigation
            anchor = linkend

            # For part references, use the anchor as both file reference and
            # anchor for proper navigation:
            # pt0003s0001#pt0003s0001 instead of pt0003#pt0003s0001
            if file_base.startswith('pt') and anchor != file_base:
                url_value = f"{anchor}#{anchor}"
            else:
                url_value = f"{file_base}#{anchor}"

            if elem.tag in ("link", "xref"):
                # Build replacement <ulink url="...">
                ulink = etree.Element("ulink")
                ulink.set("url", url_value)
                if elem.tag == "xref":
                    # xref is empty – use linkend as visible text placeholder
                    ulink.text = linkend
                else:
                    # Preserve link's text and children
                    ulink.text = elem.text
                    for child in elem:
                        ulink.append(child)
                ulink.tail = elem.tail

                # Replace in parent
                idx = list(parent).index(elem)
                parent.remove(elem)
                parent.insert(idx, ulink)
                converted += 1
            # Other elements with linkend (e.g. tocentry) – leave as-is;
            # the downstream XSL handles those via entity expansion.

    if converted > 0:
        logger.info(
            f"Converted {converted} cross-file linkend reference(s) to "
            f"<ulink url='...'> for standalone DTD compliance"
        )


def _extract_isbn(root: etree._Element) -> Optional[str]:
    isbn_elements = root.xpath(".//isbn")
    for node in isbn_elements:
        if isinstance(node, etree._Element):
            text = (node.text or "").strip()
            if text:
                cleaned = re.sub(r"[^0-9A-Za-z]", "", text)
                if cleaned:
                    return cleaned
    return None


def _extract_isbn_from_filename(path: Path) -> Optional[str]:
    """
    Extract an ISBN from the filename (prefers 13-digit over 10-digit).
    Returns only the digit/X sequence.
    """
    stem = path.stem.upper()
    if not stem:
        return None

    cleaned = re.sub(r"[^0-9X]", "", stem)
    if not cleaned:
        return None

    match = re.search(r"\d{13}", cleaned)
    if match:
        return match.group(0)

    match = re.search(r"\d{9}[0-9X]", cleaned)
    if match:
        return match.group(0)

    return None


def _sanitise_basename(name: str) -> str:
    cleaned = re.sub(r"[^0-9A-Za-z_-]", "", name)
    return cleaned or "book"


def _extract_title_text(element: etree._Element) -> str:
    title = element.find("title")
    if title is not None:
        text = "".join(title.itertext()).strip()
        if text:
            return text
    return ""


def _normalize_title_for_match(title: str) -> str:
    if not title:
        return ""
    normalized = title.casefold().strip()
    normalized = re.sub(r"[^\w\s]", " ", normalized)
    normalized = re.sub(r"\s+", " ", normalized).strip()
    return normalized


def _is_wiley_publisher(publisher_name: Optional[str]) -> bool:
    if not publisher_name:
        return False
    normalized = publisher_name.casefold().strip()
    normalized = re.sub(r"\s+", " ", normalized)
    if "wiley" in normalized:
        return True
    return any(alias in normalized for alias in WILEY_PUBLISHER_ALIASES)


def _is_wiley_eula_title(title: str) -> bool:
    normalized = _normalize_title_for_match(title)
    if not normalized:
        return False
    if normalized == "eula":
        return True
    return any(snippet in normalized for snippet in WILEY_EULA_TITLE_SNIPPETS)


def _is_index_fragment(fragment: ChapterFragment) -> bool:
    section_type = (fragment.section_type or "").casefold().strip()
    if section_type == "index":
        return True
    role = (fragment.element.get("role") or "").casefold().strip()
    if role == "index":
        return True
    if _local_name(fragment.element) == "index":
        return True
    title = fragment.title or _extract_title_text(fragment.element)
    normalized = _normalize_title_for_match(title)
    if normalized in WILEY_INDEX_TITLES:
        return True
    return bool(re.search(r"\bindex\b", normalized))


def _remove_entity_references(root: etree._Element, entities: Set[str]) -> int:
    removed = 0
    for parent in root.iter():
        for child in list(parent):
            if isinstance(child, etree._Entity):
                name = getattr(child, "name", None)
                if name in entities:
                    parent.remove(child)
                    removed += 1
    return removed


def _filter_wiley_fragments(
    book_root: etree._Element,
    fragments: Sequence[ChapterFragment],
) -> Tuple[List[ChapterFragment], List[ChapterFragment], int]:
    to_remove: List[ChapterFragment] = []
    for fragment in fragments:
        title = fragment.title or _extract_title_text(fragment.element)
        if _is_wiley_eula_title(title) or _is_index_fragment(fragment):
            to_remove.append(fragment)

    if not to_remove:
        return list(fragments), [], 0

    entity_names = {frag.entity for frag in to_remove}
    removed_refs = _remove_entity_references(book_root, entity_names)
    remaining = [frag for frag in fragments if frag.entity not in entity_names]
    return remaining, to_remove, removed_refs


def _qualified_tag(element: etree._Element, local_name: str) -> str:
    """Return a tag name that preserves the namespace (if any) of the given element."""
    namespace = etree.QName(element).namespace
    if namespace:
        return f"{{{namespace}}}{local_name}"
    return local_name


def _looks_like_part_title(title: str) -> bool:
    """Heuristic to decide if a chapter title denotes a Part heading."""
    normalized = re.sub(r"\s+", " ", title or "").strip().lower()
    return bool(re.match(r"^part\b", normalized))

def _looks_like_volume_title(title: str) -> bool:
    """Heuristic to decide if a chapter title denotes a Volume heading."""
    normalized = re.sub(r"\s+", " ", title or "").strip().lower()
    return bool(re.match(r"^volume\b", normalized))


def _looks_like_section_title(title: str) -> bool:
    """Heuristic to decide if a chapter title denotes a SECTION heading."""
    normalized = re.sub(r"\s+", " ", title or "").strip().lower()
    return bool(re.match(r"^section\b", normalized))


def _update_partintro_ids(partintro: etree._Element, old_prefix: str, new_prefix: str) -> Dict[str, str]:
    """
    Update all IDs in partintro content from old chapter prefix to new part/subpart prefix.

    When a chapter is converted to a part or subpart, any content moved into partintro
    still has IDs using the original chapter prefix (e.g., ch0012s0001). This function
    updates those IDs to use the new prefix (e.g., pt0012s0001 or sp0012s0001).

    Args:
        partintro: The partintro element containing content to update
        old_prefix: Original chapter prefix (e.g., "ch0012")
        new_prefix: New part/subpart prefix (e.g., "pt0012" or "sp0012")

    Returns:
        Dict mapping old IDs to new IDs (for global cross-reference updates)
    """
    id_mappings: Dict[str, str] = {}

    if old_prefix == new_prefix:
        return id_mappings

    # Pattern to match IDs starting with the old prefix
    # e.g., ch0012s0001f0001 -> pt0012s0001f0001
    old_pattern = re.compile(rf'^{re.escape(old_prefix)}(.*)')

    for elem in partintro.iter():
        if not isinstance(elem.tag, str):
            continue

        # Update id attribute and track mapping
        elem_id = elem.get("id")
        if elem_id:
            match = old_pattern.match(elem_id)
            if match:
                new_id = f"{new_prefix}{match.group(1)}"
                elem.set("id", new_id)
                id_mappings[elem_id] = new_id

        # Update linkend attribute (cross-references within partintro)
        linkend = elem.get("linkend")
        if linkend:
            match = old_pattern.match(linkend)
            if match:
                new_linkend = f"{new_prefix}{match.group(1)}"
                elem.set("linkend", new_linkend)

        # Update url attributes that reference internal IDs
        url = elem.get("url")
        if url and url.startswith(old_prefix):
            match = old_pattern.match(url)
            if match:
                new_url = f"{new_prefix}{match.group(1)}"
                elem.set("url", new_url)

    return id_mappings


def _apply_id_mappings_globally(root: etree._Element, id_mappings: Dict[str, str]) -> int:
    """
    Update all cross-references in the root tree using the provided ID mappings.

    When IDs are changed (e.g., ch0012s0001 -> pt0012s0001), any linkend or url
    attributes pointing to the old IDs need to be updated throughout the document.

    Args:
        root: Root element of the document tree
        id_mappings: Dict mapping old IDs to new IDs

    Returns:
        Number of cross-references updated
    """
    if not id_mappings:
        return 0

    updates = 0
    for elem in root.iter():
        if not isinstance(elem.tag, str):
            continue

        # Update linkend attribute
        linkend = elem.get("linkend")
        if linkend and linkend in id_mappings:
            elem.set("linkend", id_mappings[linkend])
            updates += 1

        # Update url attribute
        url = elem.get("url")
        if url and url in id_mappings:
            elem.set("url", id_mappings[url])
            updates += 1

    return updates


def _convert_chapter_to_subpart(chapter: etree._Element) -> Tuple[etree._Element, Dict[str, str]]:
    """Transform a chapter element that represents a section marker into a <subpart> element.

    Returns:
        Tuple of (subpart element, dict of ID mappings for global cross-reference updates)
    """
    subpart_tag = _qualified_tag(chapter, "subpart")
    partintro_tag = _qualified_tag(chapter, "partintro")
    title_tag = _qualified_tag(chapter, "title")

    id_mappings: Dict[str, str] = {}

    attrib = dict(chapter.attrib)
    raw_id = (attrib.get("id") or "").strip()
    old_prefix = raw_id if raw_id and re.match(r"^ch\d{4}$", raw_id) else None
    new_prefix = None
    if old_prefix:
        new_prefix = f"sp{raw_id[2:]}"
        attrib["id"] = new_prefix

    subpart = etree.Element(subpart_tag, attrib=attrib, nsmap=chapter.nsmap)

    # Move beginpage FIRST if present (DTD: subpart = beginpage?, partinfo?, title, ...)
    for child in list(chapter):
        if isinstance(child.tag, str) and _local_name(child) == "beginpage":
            chapter.remove(child)
            subpart.append(child)
            break

    # Move the title if present; otherwise create one from the extracted title text
    title_elem = None
    for child in list(chapter):
        if isinstance(child.tag, str) and _local_name(child) == "title":
            title_elem = child
            chapter.remove(child)
            break
    if title_elem is None:
        title_elem = etree.Element(title_tag)
        title_elem.text = _extract_title_text(chapter) or "Section"
    subpart.append(title_elem)

    # Preserve any introductory content as partintro
    intro_children = list(chapter)
    intro_text = chapter.text
    if intro_children or (intro_text and intro_text.strip()):
        partintro = etree.Element(partintro_tag, nsmap=chapter.nsmap)
        partintro.text = intro_text
        for node in intro_children:
            partintro.append(node)
        # Update IDs inside partintro from ch#### to sp#### format
        if old_prefix and new_prefix:
            id_mappings = _update_partintro_ids(partintro, old_prefix, new_prefix)
        subpart.append(partintro)

    subpart.tail = chapter.tail
    return subpart, id_mappings


def _convert_chapter_to_part(chapter: etree._Element) -> Tuple[etree._Element, Dict[str, str]]:
    """Transform a chapter element that represents a part heading into a <part> element.

    Returns:
        Tuple of (part element, dict of ID mappings for global cross-reference updates)
    """
    part_tag = _qualified_tag(chapter, "part")
    partintro_tag = _qualified_tag(chapter, "partintro")
    title_tag = _qualified_tag(chapter, "title")

    id_mappings: Dict[str, str] = {}

    # IMPORTANT: Avoid ID collisions with chapter fragments.
    # Part marker chapters typically have IDs like ch0012. During packaging, chapter fragments
    # are also assigned entity IDs in the same sequence (ch0012.xml with <chapter id="ch0012">).
    # If we keep the part wrapper ID as ch####, we can end up with BOTH:
    #   <part id="ch0012"> ... &ch0012; ... </part>
    # and the referenced chapter fragment also having id="ch0012".
    # To keep IDs unique, rewrite part wrapper IDs to a separate namespace (pt####) when possible.
    attrib = dict(chapter.attrib)
    raw_id = (attrib.get("id") or "").strip()
    old_prefix = raw_id if raw_id and re.match(r"^ch\d{4}$", raw_id) else None
    new_prefix = None
    if old_prefix:
        new_prefix = f"pt{raw_id[2:]}"
        attrib["id"] = new_prefix

    part = etree.Element(part_tag, attrib=attrib, nsmap=chapter.nsmap)

    # Move beginpage FIRST if present (DTD: part = beginpage?, partinfo?, title, ...)
    for child in list(chapter):
        if isinstance(child.tag, str) and _local_name(child) == "beginpage":
            chapter.remove(child)
            part.append(child)
            break

    # Move the title if present; otherwise create one from the extracted title text
    title_elem = None
    for child in list(chapter):
        if isinstance(child.tag, str) and _local_name(child) == "title":
            title_elem = child
            chapter.remove(child)
            break
    if title_elem is None:
        title_elem = etree.Element(title_tag)
        title_elem.text = _extract_title_text(chapter) or "Part"
    part.append(title_elem)

    # Preserve any introductory content from the part marker chapter
    intro_children = list(chapter)
    intro_text = chapter.text
    if intro_children or (intro_text and intro_text.strip()):
        partintro = etree.Element(partintro_tag, nsmap=chapter.nsmap)
        partintro.text = intro_text
        for node in intro_children:
            partintro.append(node)
        # Update IDs inside partintro from ch#### to pt#### format
        if old_prefix and new_prefix:
            id_mappings = _update_partintro_ids(partintro, old_prefix, new_prefix)
        part.append(partintro)

    part.tail = chapter.tail
    return part, id_mappings


def _auto_group_chapters_into_parts(root: etree._Element) -> bool:
    """Wrap chapters under synthetic <part> elements when titles look like parts.

    Note: For books that have Volume/Section marker pages, prefer
    `_auto_group_chapters_into_volumes_and_sections()`.
    """
    children = list(root)

    # Skip if parts already exist
    if any(isinstance(child.tag, str) and _local_name(child) in {"part", "subpart"} for child in children):
        return False

    part_marker_indices: Set[int] = set()
    part_titles: List[str] = []
    for idx, child in enumerate(children):
        if not (isinstance(child.tag, str) and _local_name(child) == "chapter"):
            continue
        title_text = _extract_title_text(child)
        if not _looks_like_part_title(title_text):
            continue
        has_following_chapter = any(
            isinstance(sibling.tag, str) and _local_name(sibling) == "chapter"
            for sibling in children[idx + 1 :]
        )
        if has_following_chapter:
            part_marker_indices.add(idx)
            part_titles.append(title_text or "Part")

    if not part_marker_indices:
        return False

    root_text = root.text
    root[:] = []
    root.text = root_text

    # Collect all ID mappings from part conversions for global cross-reference updates
    all_id_mappings: Dict[str, str] = {}

    current_part: Optional[etree._Element] = None
    for idx, child in enumerate(children):
        if idx in part_marker_indices:
            current_part, id_mappings = _convert_chapter_to_part(child)
            all_id_mappings.update(id_mappings)
            root.append(current_part)
            continue

        if current_part is not None:
            if isinstance(child.tag, str) and _local_name(child) == "chapter":
                current_part.append(child)
                continue
            elif not isinstance(child.tag, str):
                current_part.append(child)
                continue
            else:
                current_part = None

        root.append(child)

    # Apply ID mappings globally to update cross-references in all chapters
    if all_id_mappings:
        updates = _apply_id_mappings_globally(root, all_id_mappings)
        if updates > 0:
            logger.info(f"Updated {updates} cross-reference(s) after part ID changes")

    logger.info(
        "Auto-grouped chapters into %d part(s) based on titles: %s",
        len(part_marker_indices),
        "; ".join(part_titles),
    )
    return True


def _extract_part_number(title_text: str) -> Optional[int]:
    """Extract the part number from a title like 'Part 2 Materials' -> 2."""
    if not title_text:
        return None
    normalized = re.sub(r"\s+", " ", title_text).strip()
    m = re.match(r"(?i)^part\s+(\d+)", normalized)
    if m:
        return int(m.group(1))
    return None


def _promote_orphan_part_chapters(root: etree._Element) -> bool:
    """Promote <chapter> elements with part-like titles to <part> when parts already exist.

    When some spine items are detected as 'part' during EPUB conversion but
    others are missed (detected as 'chapter'), the auto-grouping functions
    skip entirely because parts already exist.  This leaves orphan chapters
    with titles like "Part 2 Clinical Practice" at the book level OR nested
    inside an existing part, causing entity numbering mismatches.

    This function:
    1. Extracts part-titled chapters that are nested inside existing parts
       and places them at the book level as new <part> elements
    2. Promotes book-level chapters with part-like titles to <part> elements
    3. Nests subsequent non-part chapters under the new parts
    """
    children = list(root)

    # Only needed when parts already exist (otherwise auto-grouping handles it)
    has_parts = any(
        isinstance(c.tag, str) and _local_name(c) in {"part", "subpart"}
        for c in children
    )
    if not has_parts:
        return False

    # Phase 1: Extract part-titled chapters nested inside existing parts
    extracted_any = False
    for child in list(root):
        if not isinstance(child.tag, str) or _local_name(child) not in {"part", "subpart"}:
            continue
        to_extract: List[etree._Element] = []
        for inner in list(child):
            if not isinstance(inner.tag, str):
                continue
            if _local_name(inner) != "chapter":
                continue
            title_text = _extract_title_text(inner)
            if _looks_like_part_title(title_text):
                to_extract.append(inner)
                logger.info(
                    "Found part-titled chapter nested inside %s: '%s' (id=%s)",
                    _local_name(child), title_text, (inner.get("id") or ""),
                )
        if to_extract:
            parent_idx = list(root).index(child)
            for offset, inner in enumerate(to_extract):
                child.remove(inner)
                root.insert(parent_idx + 1 + offset, inner)
            extracted_any = True

    if extracted_any:
        children = list(root)

    # Phase 2: Find book-level chapter children with part-like titles
    orphan_indices: List[int] = []
    for idx, child in enumerate(children):
        if not isinstance(child.tag, str):
            continue
        if _local_name(child) != "chapter":
            continue
        title_text = _extract_title_text(child)
        if _looks_like_part_title(title_text):
            orphan_indices.append(idx)
            logger.info(
                "Found orphan part-titled chapter at book level index %d: '%s' (id=%s)",
                idx, title_text, (child.get("id") or ""),
            )

    if not orphan_indices and not extracted_any:
        return False

    if not orphan_indices:
        return extracted_any

    # Collect all ID mappings for global cross-reference updates
    all_id_mappings: Dict[str, str] = {}

    # Phase 3: Rebuild root, promoting orphan chapters to parts
    root_text = root.text
    root[:] = []
    root.text = root_text

    current_new_part: Optional[etree._Element] = None
    orphan_set = set(orphan_indices)
    promoted_titles: List[str] = []

    for idx, child in enumerate(children):
        if idx in orphan_set:
            current_new_part, id_mappings = _convert_chapter_to_part(child)
            all_id_mappings.update(id_mappings)
            root.append(current_new_part)
            promoted_titles.append(_extract_title_text(child) or "Part")
            continue

        if not isinstance(child.tag, str):
            if current_new_part is not None:
                current_new_part.append(child)
            else:
                root.append(child)
            continue

        local_name = _local_name(child)

        if local_name in {"part", "subpart"}:
            current_new_part = None
            root.append(child)
            continue

        if current_new_part is not None and local_name == "chapter":
            current_new_part.append(child)
            continue

        if current_new_part is not None:
            current_new_part.append(child)
        else:
            root.append(child)

    if all_id_mappings:
        updates = _apply_id_mappings_globally(root, all_id_mappings)
        if updates > 0:
            logger.info(f"Updated {updates} cross-reference(s) after promoting orphan part chapters")

    logger.info(
        "Promoted %d orphan chapter(s) with part-like titles to <part>: %s",
        len(orphan_indices),
        "; ".join(promoted_titles),
    )
    return True


def _insert_missing_part_one(root: etree._Element) -> bool:
    """Create a synthetic Part 1 when the first <part> title starts at Part 2+.

    Some EPUBs have Part 1 content as loose chapters at the book level with
    no explicit "Part 1" title page.  The first <part> element is "Part 2",
    making all pt entity IDs off by 1 (pt0001 = "Part 2", pt0002 = "Part 3").

    This function detects the gap and creates a synthetic <part> with title
    "Part 1" that wraps the loose <chapter> elements that appear immediately
    before the first existing <part>.  Front-matter elements (preface,
    dedication, etc.) are left at the book level.
    """
    children = list(root)

    # Find the first <part> element
    first_part_idx = None
    for idx, child in enumerate(children):
        if isinstance(child.tag, str) and _local_name(child) in {"part", "subpart"}:
            first_part_idx = idx
            break

    if first_part_idx is None:
        return False

    first_part = children[first_part_idx]
    first_title = _extract_title_text(first_part)
    first_part_num = _extract_part_number(first_title)

    if first_part_num is None or first_part_num <= 1:
        return False  # First part is Part 1 or title doesn't have a number

    logger.info(
        "First <part> title is '%s' (Part %d) — Part 1 is missing, creating synthetic wrapper",
        first_title, first_part_num,
    )

    # Collect loose <chapter> elements before the first part
    chapters_for_part1: List[Tuple[int, etree._Element]] = []
    for idx in range(first_part_idx):
        child = children[idx]
        if not isinstance(child.tag, str):
            continue
        local = _local_name(child)
        if local == "chapter":
            chapters_for_part1.append((idx, child))

    if not chapters_for_part1:
        logger.info("No loose chapters found before first part; skipping Part 1 creation")
        return False

    # Create the synthetic Part 1 element
    part_tag = _qualified_tag(first_part, "part")
    title_tag = _qualified_tag(first_part, "title")

    synthetic_part = etree.Element(part_tag, nsmap=first_part.nsmap)
    title_elem = etree.SubElement(synthetic_part, title_tag)
    title_elem.text = "Part 1"
    synthetic_part.tail = "\n"

    # Move the loose chapters into the synthetic Part 1
    for _, ch_elem in chapters_for_part1:
        root.remove(ch_elem)
        synthetic_part.append(ch_elem)

    # Insert the synthetic Part 1 just before the first existing part
    new_first_part_idx = list(root).index(first_part)
    root.insert(new_first_part_idx, synthetic_part)

    logger.info(
        "Created synthetic Part 1 with %d chapter(s) before '%s'",
        len(chapters_for_part1), first_title,
    )
    return True


def _auto_group_chapters_into_volumes_and_sections(root: etree._Element) -> bool:
    """
    Group books that contain "VOLUME ..." and "SECTION ..." marker pages.

    Structure emitted:
    - volume marker chapters -> <part role="volume"> ... </part>
    - section marker chapters inside a part -> <subpart role="section"> ... </subpart>

    This keeps the hierarchy DTD-legal (RittDoc adds <subpart> between part and chapters).
    """
    children = list(root)

    # Don't try to regroup if hierarchy already exists.
    if any(isinstance(child.tag, str) and _local_name(child) in {"part", "subpart"} for child in children):
        return False

    # Detect whether we have any volume markers with following chapters.
    volume_marker_indices: Set[int] = set()
    for idx, child in enumerate(children):
        if not (isinstance(child.tag, str) and _local_name(child) == "chapter"):
            continue
        title_text = _extract_title_text(child)
        if not _looks_like_volume_title(title_text):
            continue
        has_following_chapter = any(
            isinstance(sibling.tag, str) and _local_name(sibling) == "chapter"
            for sibling in children[idx + 1 :]
        )
        if has_following_chapter:
            volume_marker_indices.add(idx)

    # If no volume markers, fall back to the existing Part grouping logic.
    if not volume_marker_indices:
        return False

    root_text = root.text
    root[:] = []
    root.text = root_text

    # Collect all ID mappings from part/subpart conversions for global cross-reference updates
    all_id_mappings: Dict[str, str] = {}

    current_part: Optional[etree._Element] = None
    current_subpart: Optional[etree._Element] = None

    for idx, child in enumerate(children):
        if not isinstance(child.tag, str):
            # Preserve comments/PIs under the active container if present.
            if current_subpart is not None:
                current_subpart.append(deepcopy(child))
            elif current_part is not None:
                current_part.append(deepcopy(child))
            else:
                root.append(deepcopy(child))
            continue

        local_name = _local_name(child)
        if local_name == "chapter":
            title_text = _extract_title_text(child)

            # Start a new volume (<part role="volume">)
            if idx in volume_marker_indices:
                current_part, id_mappings = _convert_chapter_to_part(child)
                all_id_mappings.update(id_mappings)
                current_part.set("role", "volume")
                root.append(current_part)
                current_subpart = None
                continue

            # Start a new section (<subpart role="section">) within the current volume.
            if current_part is not None and _looks_like_section_title(title_text):
                current_subpart, id_mappings = _convert_chapter_to_subpart(child)
                all_id_mappings.update(id_mappings)
                current_subpart.set("role", "section")
                current_part.append(current_subpart)
                continue

            # If this looks like a "Part ..." marker inside a volume, treat it as a subpart.
            if current_part is not None and _looks_like_part_title(title_text):
                current_subpart, id_mappings = _convert_chapter_to_subpart(child)
                all_id_mappings.update(id_mappings)
                current_subpart.set("role", "part")
                current_part.append(current_subpart)
                continue

            # Regular chapter: place under the most specific active container.
            if current_subpart is not None:
                current_subpart.append(child)
            elif current_part is not None:
                current_part.append(child)
            else:
                root.append(child)
            continue

        # Non-chapter element: append under current container if any.
        if current_subpart is not None:
            current_subpart.append(child)
        elif current_part is not None:
            current_part.append(child)
        else:
            root.append(child)

    # Apply ID mappings globally to update cross-references in all chapters
    if all_id_mappings:
        updates = _apply_id_mappings_globally(root, all_id_mappings)
        if updates > 0:
            logger.info(f"Updated {updates} cross-reference(s) after part/subpart ID changes")

    logger.info("Auto-grouped chapters into volumes (%d) and sections (subparts).", len(volume_marker_indices))
    return True

def _extract_bookinfo(root: etree._Element) -> Dict:
    """
    Extract book metadata from XML for BookInfo section.
    
    Returns dict with ISBN, title, authors, publisher, date, edition, copyright.
    Uses placeholders for missing data to ensure validation passes.
    """
    bookinfo = {
        'isbn': None,
        'title': None,
        'subtitle': None,
        'authors': [],
        'publisher': None,
        'pubdate': None,
        'edition': None,
        'copyright_holder': None,
        'copyright_year': None
    }
    
    # Look for existing bookinfo or info element
    info_elem = root.find('.//bookinfo') or root.find('.//info')
    
    if info_elem is not None:
        # Extract ISBN
        isbn_elem = info_elem.find('.//isbn')
        if isbn_elem is not None and isbn_elem.text:
            isbn_clean = re.sub(r'[^0-9X]', '', isbn_elem.text.strip())
            if isbn_clean:
                bookinfo['isbn'] = isbn_clean
        
        # Extract Title
        title_elem = info_elem.find('.//title')
        if title_elem is not None:
            bookinfo['title'] = ''.join(title_elem.itertext()).strip()
        
        # Extract Subtitle
        subtitle_elem = info_elem.find('.//subtitle')
        if subtitle_elem is not None:
            bookinfo['subtitle'] = ''.join(subtitle_elem.itertext()).strip()
        
        # Extract Authors - Multiple formats supported
        author_elems = info_elem.findall('.//authorgroup/author') or info_elem.findall('.//author')
        for author_elem in author_elems:
            personname_elem = author_elem.find('.//personname')
            if personname_elem is not None:
                firstname = ''
                surname = ''
                firstname_elem = personname_elem.find('.//firstname')
                if firstname_elem is not None and firstname_elem.text:
                    firstname = firstname_elem.text.strip()
                surname_elem = personname_elem.find('.//surname')
                if surname_elem is not None and surname_elem.text:
                    surname = surname_elem.text.strip()
                if firstname or surname:
                    bookinfo['authors'].append(f"{firstname} {surname}".strip())
        
        # Collaborative authors (collab/collabname)
        collab_elems = info_elem.findall('.//collab/collabname')
        for collab_elem in collab_elems:
            if collab_elem.text:
                bookinfo['authors'].append(collab_elem.text.strip())
        
        # Fallback: Check for editor if no authors
        if not bookinfo['authors']:
            editor_elems = info_elem.findall('.//editor')
            for editor_elem in editor_elems:
                personname_elem = editor_elem.find('.//personname')
                if personname_elem is not None:
                    firstname = ''
                    surname = ''
                    firstname_elem = personname_elem.find('.//firstname')
                    if firstname_elem is not None and firstname_elem.text:
                        firstname = firstname_elem.text.strip()
                    surname_elem = personname_elem.find('.//surname')
                    if surname_elem is not None and surname_elem.text:
                        surname = surname_elem.text.strip()
                    if firstname or surname:
                        bookinfo['authors'].append(f"{firstname} {surname} (Editor)".strip())
        
        # Extract Publisher
        publisher_elem = info_elem.find('.//publisher/publishername')
        if publisher_elem is not None and publisher_elem.text:
            bookinfo['publisher'] = publisher_elem.text.strip()
        
        # Extract Publication Date
        pubdate_elem = info_elem.find('.//pubdate')
        if pubdate_elem is not None and pubdate_elem.text:
            bookinfo['pubdate'] = pubdate_elem.text.strip()
        
        # Extract Edition
        edition_elem = info_elem.find('.//edition')
        if edition_elem is not None and edition_elem.text:
            bookinfo['edition'] = edition_elem.text.strip()
        
        # Extract Copyright
        copyright_elem = info_elem.find('.//copyright')
        if copyright_elem is not None:
            year_elem = copyright_elem.find('.//year')
            if year_elem is not None and year_elem.text:
                bookinfo['copyright_year'] = year_elem.text.strip()
            holder_elem = copyright_elem.find('.//holder')
            if holder_elem is not None and holder_elem.text:
                bookinfo['copyright_holder'] = holder_elem.text.strip()
    
    # If no authors, use publisher as fallback
    if not bookinfo['authors'] and bookinfo['publisher']:
        bookinfo['authors'].append(bookinfo['publisher'])
    
    return bookinfo


def _create_bookinfo_element(bookinfo: Dict) -> etree._Element:
    """
    Create a complete <bookinfo> element with all metadata.
    Uses placeholders for missing fields to ensure validation passes.
    """
    bookinfo_elem = etree.Element('bookinfo')
    
    # ISBN (use placeholder if not found)
    isbn_elem = etree.SubElement(bookinfo_elem, 'isbn')
    isbn_elem.text = bookinfo.get('isbn') or '0000000000000'
    
    # Title (use placeholder if not found)
    title_elem = etree.SubElement(bookinfo_elem, 'title')
    title_elem.text = bookinfo.get('title') or 'Untitled Book'
    
    # Subtitle (optional - only add if exists)
    if bookinfo.get('subtitle'):
        subtitle_elem = etree.SubElement(bookinfo_elem, 'subtitle')
        subtitle_elem.text = bookinfo['subtitle']
    
    # Authors
    authorgroup_elem = etree.SubElement(bookinfo_elem, 'authorgroup')
    authors = bookinfo.get('authors', [])
    if not authors:
        authors = ['Unknown Author']
    
    for author_name in authors:
        author_elem = etree.SubElement(authorgroup_elem, 'author')
        personname_elem = etree.SubElement(author_elem, 'personname')
        
        # Try to split name into firstname/surname
        parts = author_name.split()
        if len(parts) >= 2:
            firstname_elem = etree.SubElement(personname_elem, 'firstname')
            firstname_elem.text = ' '.join(parts[:-1])
            surname_elem = etree.SubElement(personname_elem, 'surname')
            surname_elem.text = parts[-1]
        else:
            surname_elem = etree.SubElement(personname_elem, 'surname')
            surname_elem.text = author_name
    
    # Publisher
    publisher_elem = etree.SubElement(bookinfo_elem, 'publisher')
    publishername_elem = etree.SubElement(publisher_elem, 'publishername')
    publishername_elem.text = bookinfo.get('publisher') or 'Unknown Publisher'
    
    # Publication Date
    pubdate_elem = etree.SubElement(bookinfo_elem, 'pubdate')
    pubdate_elem.text = bookinfo.get('pubdate') or '2024'
    
    # Edition
    edition_elem = etree.SubElement(bookinfo_elem, 'edition')
    edition_elem.text = bookinfo.get('edition') or '1st Edition'
    
    # Copyright
    copyright_elem = etree.SubElement(bookinfo_elem, 'copyright')
    year_elem = etree.SubElement(copyright_elem, 'year')
    year_elem.text = bookinfo.get('copyright_year') or bookinfo.get('pubdate') or '2024'
    holder_elem = etree.SubElement(copyright_elem, 'holder')
    holder_elem.text = bookinfo.get('copyright_holder') or bookinfo.get('publisher') or 'Copyright Holder'
    
    return bookinfo_elem


def _ensure_chapterinfo_authors(fragment_root: etree._Element) -> None:
    """
    Ensure chapter-level author information is preserved in chapterinfo.

    Gap 3 fix: AddRISInfo.xsl looks for chapter/chapterinfo/authorgroup to add
    chapter-specific authors to the first sect1's risinfo.

    This function:
    1. Preserves existing chapterinfo with authorgroup if present
    2. Creates chapterinfo if chapter has author metadata but no chapterinfo
    """
    if not isinstance(fragment_root.tag, str):
        return

    local_tag = _local_name(fragment_root)
    if local_tag not in {"chapter", "appendix"}:
        return

    # Check if chapterinfo already exists with authorgroup
    chapterinfo = fragment_root.find("chapterinfo")
    if chapterinfo is not None:
        authorgroup = chapterinfo.find("authorgroup")
        if authorgroup is not None and len(authorgroup):
            # Already has chapter-level authors, nothing to do
            return
        author = chapterinfo.find("author")
        if author is not None:
            # Has author but not in authorgroup, wrap it
            if authorgroup is None:
                authorgroup = etree.SubElement(chapterinfo, "authorgroup")
            authorgroup.append(author)
            return

    # Check for appendixinfo (for appendix elements)
    if local_tag == "appendix":
        appendixinfo = fragment_root.find("appendixinfo")
        if appendixinfo is not None:
            authorgroup = appendixinfo.find("authorgroup")
            if authorgroup is not None and len(authorgroup):
                # Convert appendixinfo author to chapterinfo for consistency
                if chapterinfo is None:
                    chapterinfo = etree.Element("chapterinfo")
                    # Insert after title if present, else at start
                    title_elem = fragment_root.find("title")
                    if title_elem is not None:
                        title_idx = list(fragment_root).index(title_elem)
                        fragment_root.insert(title_idx + 1, chapterinfo)
                    else:
                        fragment_root.insert(0, chapterinfo)
                chapterinfo.append(deepcopy(authorgroup))
                return

    # Look for inline author patterns that could be extracted
    # (This is a simplified heuristic - real EPUB author extraction is complex)
    # For now, we just ensure the structure exists for manual population


def _calculate_chapter_label(entity_id: str, section_type: str) -> str:
    """
    Calculate the @label attribute value for a chapter-like element.

    Gap 1 fix: AddRISInfo.xsl requires @label to populate chapternumber in risinfo.

    Args:
        entity_id: The entity ID (e.g., "ch0001", "ap0001", "pf0001")
        section_type: The section type (e.g., "chapter", "appendix", "preface")

    Returns:
        Label string for the element (e.g., "1", "A", "Preface")
    """
    section_type_lower = (section_type or "").lower()

    # TOC and special sections get descriptive labels
    if section_type_lower == "toc":
        return "TOC"
    if section_type_lower == "index":
        return "Index"
    if section_type_lower == "preface":
        return "Preface"
    if section_type_lower == "dedication":
        return "Dedication"
    if section_type_lower == "glossary":
        return "Glossary"
    if section_type_lower == "bibliography":
        return "Bibliography"
    if section_type_lower == "colophon":
        return "Colophon"
    if section_type_lower in {"acknowledgments", "acknowledgements"}:
        return "Acknowledgments"

    # Appendix: extract letter from ID (ap0001 -> A, ap0002 -> B)
    if section_type_lower == "appendix":
        match = re.match(r"ap(\d+)", entity_id, re.IGNORECASE)
        if match:
            num = int(match.group(1))
            # Convert to letter (1->A, 2->B, etc.)
            if 1 <= num <= 26:
                return chr(ord('A') + num - 1)
            return str(num)
        return "A"

    # Part: extract number from ID (pt0001 -> 1, pt0002 -> 2)
    if section_type_lower == "part":
        match = re.match(r"pt(\d+)", entity_id, re.IGNORECASE)
        if match:
            return str(int(match.group(1)))
        return "1"

    # Subpart: extract number from ID (sp0001 -> 1, sp0002 -> 2)
    if section_type_lower == "subpart":
        match = re.match(r"sp(\d+)", entity_id, re.IGNORECASE)
        if match:
            return str(int(match.group(1)))
        return "1"

    # Chapter: extract number from ID (ch0001 -> 1, ch0012 -> 12)
    match = re.match(r"ch(\d+)", entity_id, re.IGNORECASE)
    if match:
        return str(int(match.group(1)))

    # Default fallback
    return "1"


def _split_root(root: etree._Element) -> Tuple[etree._Element, List[ChapterFragment]]:
    root_copy = etree.Element(root.tag, attrib=dict(root.attrib), nsmap=root.nsmap)
    root_copy.text = root.text
    fragments: List[ChapterFragment] = []
    # Track prefix remappings for cross-file linkend fixup.
    # When _ensure_fragment_ids renames ch0093 -> ch0101, cross-references
    # in OTHER fragments still use the old prefix and must be updated.
    prefix_remap: Dict[str, str] = {}  # old_prefix -> new_prefix
    # front_matter_wrapper: Optional[etree._Element] = None
    # front_matter_entity_id: Optional[str] = None
    chapter_index = 1

    # Separate counters for each element type (Gap 2 fix)
    element_type_counters: Dict[str, int] = {
        "chapter": 0,
        "appendix": 0,
        "glossary": 0,
        "bibliography": 0,
        "index": 0,
        "preface": 0,
        "dedication": 0,
        "colophon": 0,
        "acknowledgments": 0,
        "contributors": 0,
        "about": 0,
        "part": 0,
        "subpart": 0,
    }

    # Prefix mapping for element types (Gap 2 fix)
    # Prefixes per R2Library naming conventions:
    # https://docs.google.com/document/d/CHAPTER_NAMING_CONVENTIONS.md
    element_type_prefixes: Dict[str, str] = {
        "chapter": "ch",
        "appendix": "ap",
        "glossary": "gl",
        "bibliography": "bi",    # R2 convention: bi#### — must match conversion prefix
        "index": "in",
        "preface": "pr",       # R2 convention: pr#### (not pf)
        "dedication": "dd",    # R2 convention: dd#### (de is alternate)
        "colophon": "ch",        # XSL doesn't support 'co' - treat as chapter
        "acknowledgments": "ch", # XSL doesn't support 'ak' - treat as chapter
        "contributors": "pr",    # Contributors are prefaces - use pr prefix to match <preface> tag
        "about": "pr",           # About pages are prefaces - use pr prefix to match <preface> tag
        "part": "pt",
        "subpart": "sp",
    }

    # GLOBAL PRE-SCAN: Update counters for ALL existing chapter-like IDs in the entire book
    # before any processing happens. This prevents ID collisions when partintro structural
    # content extraction generates new IDs that might otherwise collide with existing IDs
    # in other parts of the book.
    def _global_prescan_update_counters(root_elem: etree._Element) -> None:
        """Pre-scan entire book to update counters for all existing chapter-like IDs."""
        for elem in root_elem.iter():
            if not isinstance(elem.tag, str):
                continue
            local_name = _local_name(elem)
            if not _is_chapter_node(elem) and local_name not in {"part", "subpart"}:
                continue

            existing_id = (elem.get("id") or "").strip()
            if not existing_id:
                continue

            # Check if ID matches the pattern: 2 letters + 4 digits
            id_match = re.match(r'^([a-z]{2})(\d{4})$', existing_id)
            if not id_match:
                continue

            prefix = id_match.group(1)
            number = int(id_match.group(2))

            # Find the counter key for this prefix
            counter_key = None
            for stype, pfix in element_type_prefixes.items():
                if pfix == prefix:
                    counter_key = stype
                    break

            if counter_key is None:
                continue

            # For "ch" prefix elements, use "chapter" counter
            if prefix == "ch" and counter_key != "chapter":
                counter_key = "chapter"
            # For "pr" prefix elements, use "preface" counter
            elif prefix == "pr" and counter_key != "preface":
                counter_key = "preface"

            # Update counter if existing ID's number is >= current counter
            if counter_key in element_type_counters:
                if number >= element_type_counters[counter_key]:
                    element_type_counters[counter_key] = number

    # Run the global pre-scan
    _global_prescan_update_counters(root)

    def _update_id_prefixes(fragment_root: etree._Element, old_prefix: str, new_prefix: str) -> int:
        """
        Update all IDs and ID references that use old_prefix to use new_prefix.

        When packaging renumbers chapters (e.g., ch0015 -> ch0018), all internal
        IDs must be updated to maintain consistency. This includes:
        - id attributes on all elements
        - linkend attributes (cross-references)
        - url attributes that reference internal IDs

        Args:
            fragment_root: Root element of the fragment
            old_prefix: Original chapter prefix (e.g., "ch0015")
            new_prefix: New chapter prefix (e.g., "ch0018")

        Returns:
            Number of IDs updated
        """
        if old_prefix == new_prefix:
            return 0

        updates = 0
        # Pattern to match IDs starting with the old prefix
        # e.g., ch0015s0000a0001 -> ch0018s0000a0001
        old_pattern = re.compile(rf'^{re.escape(old_prefix)}(.*)')

        for elem in fragment_root.iter():
            if not isinstance(elem.tag, str):
                continue

            # Update id attribute
            elem_id = elem.get("id")
            if elem_id:
                match = old_pattern.match(elem_id)
                if match:
                    new_id = f"{new_prefix}{match.group(1)}"
                    elem.set("id", new_id)
                    updates += 1

            # Update linkend attribute (cross-references)
            linkend = elem.get("linkend")
            if linkend:
                match = old_pattern.match(linkend)
                if match:
                    new_linkend = f"{new_prefix}{match.group(1)}"
                    elem.set("linkend", new_linkend)
                    updates += 1

            # Update url attributes that reference internal IDs
            url = elem.get("url")
            if url and url.startswith(old_prefix):
                match = old_pattern.match(url)
                if match:
                    new_url = f"{new_prefix}{match.group(1)}"
                    elem.set("url", new_url)
                    updates += 1

        return updates

    def _ensure_fragment_ids(fragment_root: etree._Element, entity_id: str, section_type: str = "chapter") -> None:
        """
        Ensure fragment root and section elements have IDs and labels.

        Some fragments are synthesized during packaging (e.g., Part title pages, TOC wrappers)
        and may not carry IDs from upstream conversion. IDs are required/recommended for
        cross-referencing and for downstream validation/reporting.

        Gap 1 fix: Also sets @label attribute required by AddRISInfo.xsl for chapternumber.

        IMPORTANT: When the entity_id differs from the current root ID, all internal IDs
        that use the old chapter prefix are updated to use the new prefix. This ensures
        consistency when chapters are renumbered during packaging (e.g., when parts are
        inserted and chapter positions shift).
        """
        if not isinstance(fragment_root.tag, str):
            return

        # Get the current root ID and determine if we need to update prefixes
        current_id = (fragment_root.get("id") or "").strip()

        # Extract chapter prefix from current ID (e.g., "ch0015" from "ch0015" or "ch0015s0000")
        old_prefix = None
        if current_id:
            prefix_match = re.match(r'^([a-z]{2}\d{4})', current_id)
            if prefix_match:
                old_prefix = prefix_match.group(1)

        # Extract prefix from new entity_id
        new_prefix = None
        new_prefix_match = re.match(r'^([a-z]{2}\d{4})', entity_id)
        if new_prefix_match:
            new_prefix = new_prefix_match.group(1)

        # If prefixes differ, update all internal IDs and record the remapping
        # for cross-file linkend fixup later.
        if old_prefix and new_prefix and old_prefix != new_prefix:
            updates = _update_id_prefixes(fragment_root, old_prefix, new_prefix)
            if updates > 0:
                logger.debug(f"Updated {updates} ID(s) from prefix {old_prefix} to {new_prefix}")
            prefix_remap[old_prefix] = new_prefix

        # Ensure the fragment root has the correct entity_id
        # Always set to entity_id to ensure consistency (even if it had an old ID)
        fragment_root.set("id", entity_id)

        # Gap 1 fix: Set @label attribute for chapter elements
        # AddRISInfo.xsl uses ancestor-or-self::chapter/@label for chapternumber
        # Note: Only certain elements have label attribute per DTD (chapter, appendix, part, sect1-5)
        # preface, dedication, glossary, bibliography, index, colophon, acknowledgments do NOT have label
        local_tag = _local_name(fragment_root)
        elements_with_label = {"chapter", "appendix", "part", "sect1", "sect2", "sect3", "sect4", "sect5", "section"}
        if local_tag in elements_with_label:
            current_label = (fragment_root.get("label") or "").strip()
            if not current_label:
                # Calculate label based on section_type and entity_id
                label = _calculate_chapter_label(entity_id, section_type)
                if label:
                    fragment_root.set("label", label)

        # Gap 3 fix: Ensure chapter-level author info is preserved in chapterinfo
        _ensure_chapterinfo_authors(fragment_root)

        # Ensure section-level IDs exist at least for sect1..sect5/simplesect.
        # We only fill missing IDs to avoid breaking upstream link resolution.
        section_counters: Dict[str, int] = {}
        for elem in fragment_root.iter():
            if not isinstance(elem.tag, str):
                continue
            lname = _local_name(elem)
            if lname.startswith("sect") and len(lname) == 5 and lname[-1].isdigit():
                level = int(lname[-1])
            elif lname == "simplesect":
                level = 1
            else:
                continue

            elem_id = (elem.get("id") or "").strip()
            if elem_id:
                continue
            elem.set("id", id_gen_section_id(entity_id, level, section_counters))

    def _extract_existing_entity_id(elem: etree._Element, section_type: str = "chapter") -> Optional[str]:
        """
        Extract and validate an existing entity ID from an element.

        If the element has an existing ID that matches the expected pattern for its
        section type (e.g., ch####, pt####, ap####), return it. Otherwise return None.

        This preserves IDs assigned by upstream conversion (epub_to_structured_v2.py)
        instead of regenerating them.
        """
        existing_id = (elem.get("id") or "").strip()
        if not existing_id:
            return None

        # Check if the ID matches the expected pattern (2 letters + 4 digits)
        id_match = re.match(r'^([a-z]{2})(\d{4})$', existing_id)
        if not id_match:
            return None

        prefix = id_match.group(1)
        section_type_lower = (section_type or "chapter").lower()
        expected_prefix = element_type_prefixes.get(section_type_lower, "ch")

        # Accept the existing ID if its prefix matches the expected prefix for this section type
        if prefix == expected_prefix:
            return existing_id

        # Also accept if both use the same counter pool (e.g., "ch" prefix types)
        if prefix == "ch" and expected_prefix == "ch":
            return existing_id

        return None

    def _update_counter_for_existing_id(existing_id: str, section_type: str = "chapter") -> None:
        """
        Update the counter to account for an existing ID being used.

        This ensures that if we use an existing ID (e.g., ch0015), the counter
        is advanced past that number to avoid generating duplicate IDs.
        """
        id_match = re.match(r'^([a-z]{2})(\d{4})$', existing_id)
        if not id_match:
            return

        prefix = id_match.group(1)
        number = int(id_match.group(2))

        # Find the counter key for this prefix
        counter_key = None
        for stype, pfix in element_type_prefixes.items():
            if pfix == prefix:
                counter_key = stype
                break

        if counter_key is None:
            return

        # For "ch" prefix elements, use "chapter" counter
        if prefix == "ch" and counter_key != "chapter":
            counter_key = "chapter"

        # Update the counter if the existing ID's number is >= current counter
        if counter_key in element_type_counters:
            if number >= element_type_counters[counter_key]:
                element_type_counters[counter_key] = number
                logger.debug(f"Updated {counter_key} counter to {number} for existing ID {existing_id}")

    def _next_entity_id(section_type: str = "chapter") -> str:
        """
        Generate next entity ID with type-specific prefix.

        Gap 2 fix: Use correct prefixes for different element types.
        - chapter -> ch#### (6 chars)
        - appendix -> ap####
        - glossary -> gl####
        - bibliography -> ch#### (uses ch prefix; bib IDs inside use 'bib' element code)
        - index -> in####
        - preface -> pr####
        - dedication -> dd####
        - part -> pt####

        Note: Chapter element IDs are ch0001, ch0002, etc. (6 chars).
        Section IDs within chapters follow the ch0001s0001 pattern (11 chars),
        which is what the XSL extracts from linkends.
        """
        nonlocal chapter_index

        section_type_lower = (section_type or "chapter").lower()

        # Get prefix for this element type (default to "ch" for unknown types)
        prefix = element_type_prefixes.get(section_type_lower, "ch")

        # IMPORTANT: Elements sharing the same prefix must share the same counter
        # to avoid duplicate IDs (e.g., colophon uses "ch" prefix like chapter,
        # contributors/about use "pr" prefix like preface)
        counter_key = section_type_lower
        if prefix == "ch" and section_type_lower != "chapter":
            # All "ch" prefix elements share the chapter counter
            counter_key = "chapter"
        elif prefix == "pr" and section_type_lower != "preface":
            # All "pr" prefix elements share the preface counter
            counter_key = "preface"

        # Increment counter for this element type
        if counter_key in element_type_counters:
            element_type_counters[counter_key] += 1
            counter = element_type_counters[counter_key]
        else:
            # Fallback to chapter_index for unknown types
            chapter_index += 1
            counter = chapter_index

        # Generate entity ID with consistent format
        # Entity IDs are chapter/file identifiers: ch0001, ap0001, etc. (6 chars)
        # The 11-char format (ch0001s0001) is for sect1 IDs WITHIN chapters,
        # not for the chapter element itself.
        entity_id = f"{prefix}{counter:04d}"
        return entity_id

    def _get_or_generate_entity_id(elem: etree._Element, section_type: str = "chapter") -> str:
        """
        Get existing entity ID from element or generate a new one.

        This preserves IDs from upstream conversion (epub_to_structured_v2.py)
        instead of always generating new IDs, which fixes the ID mismatch
        between structured.xml and the final package output.
        """
        # Try to use existing ID
        existing_id = _extract_existing_entity_id(elem, section_type)
        if existing_id:
            _update_counter_for_existing_id(existing_id, section_type)
            logger.debug(f"Preserving existing ID {existing_id} for {section_type}")
            return existing_id

        # Generate new ID
        return _next_entity_id(section_type)

    def _emit_container_copy(
        container_elem: etree._Element, container_kind: str
    ) -> etree._Element:
        """
        Emit a <part> or <subpart> element inline in Book.XML with entity
        references for child chapters.

        This function creates a container copy with entity references for child
        chapters (e.g., &ch0004;) to be placed inline in Book.XML.

        NOTE: We do NOT create separate part fragment files (pt0001.xml, etc.)
        because the DocBook DTD requires <part> to have at least one child
        structural element (chapter, appendix, etc.). Since chapters are in
        separate files referenced via entities declared only in Book.XML,
        a standalone part file would be invalid.

        Returns the container copy element (for inline placement in Book.XML).
        """
        container_copy = etree.Element(
            container_elem.tag, attrib=dict(container_elem.attrib), nsmap=container_elem.nsmap
        )
        container_copy.text = container_elem.text

        pre_content_tags = {"beginpage", "partinfo", "title", "partintro", "subtitle", "titleabbrev"}
        # Structural tags that indicate substantive content requiring extraction
        # into a separate chapter file from partintro.
        _structural_tags = {"sect1", "sect2", "section", "simplesect"}
        partintro_chapter_entity = None  # Track if we extracted structural content

        # Note: Global pre-scan at start of _split_root() already updated counters
        # for all existing chapter-like IDs in the book, preventing ID collisions
        # when partintro structural content extraction generates new IDs.

        # Get the container (Part) title for use in synthetic chapter
        container_title_elem = container_elem.find("title")
        container_title_text = ""
        if container_title_elem is not None:
            container_title_text = "".join(container_title_elem.itertext()).strip()

        for inner in container_elem:
            if not isinstance(inner.tag, str):
                continue
            inner_local = _local_name(inner)
            if inner_local == "partintro":
                # Check if partintro contains structural content (sect1, etc.)
                structural_children = [
                    c for c in inner
                    if isinstance(c.tag, str) and _local_name(c) in _structural_tags
                ]
                if structural_children:
                    # Extract structural content into a chapter fragment file.
                    # Keep only brief content (para, emphasis, etc.) in the inline partintro.
                    sanitized_partintro = etree.Element(
                        inner.tag, attrib=dict(inner.attrib), nsmap=inner.nsmap
                    )
                    sanitized_partintro.text = inner.text
                    for child in inner:
                        if not isinstance(child.tag, str):
                            sanitized_partintro.append(deepcopy(child))
                            continue
                        child_local = _local_name(child)
                        if child_local not in _structural_tags:
                            sanitized_partintro.append(deepcopy(child))
                    # Only add sanitized partintro if it has meaningful content
                    has_pi_content = bool(sanitized_partintro.text and sanitized_partintro.text.strip())
                    if not has_pi_content:
                        for c in sanitized_partintro:
                            if isinstance(c.tag, str):
                                has_pi_content = True
                                break
                    if has_pi_content:
                        container_copy.append(sanitized_partintro)

                    # Create a chapter element wrapping the structural content
                    container_id = container_elem.get("id", "pt0000")
                    ch_entity_id = _next_entity_id("chapter")
                    chapter_frag = etree.Element("chapter", nsmap=container_elem.nsmap)
                    # Mark as synthetic chapter extracted from partintro
                    chapter_frag.set("role", "partintro-content")

                    # Use the SAME title as the parent Part for the synthetic chapter
                    title_tag = _qualified_tag(container_elem, "title")
                    chapter_title = etree.Element(title_tag, nsmap=container_elem.nsmap)
                    chapter_title.text = container_title_text or "Part Introduction"
                    chapter_frag.append(chapter_title)

                    # Move all structural children into the chapter
                    for sect in structural_children:
                        chapter_frag.append(deepcopy(sect))

                    # Detect the old prefix from structural content IDs and update them.
                    # Structural content may have IDs like pt0001s0001 (from part conversion)
                    # that need to be updated to ch####s0001 (new chapter prefix).
                    old_prefix_from_content = None
                    for elem in chapter_frag.iter():
                        if not isinstance(elem.tag, str):
                            continue
                        elem_id = elem.get("id")
                        if elem_id:
                            id_match = re.match(r'^([a-z]{2}\d{4})', elem_id)
                            if id_match:
                                old_prefix_from_content = id_match.group(1)
                                break

                    # Update IDs if old prefix differs from new chapter prefix
                    if old_prefix_from_content and old_prefix_from_content != ch_entity_id:
                        updates = _update_id_prefixes(chapter_frag, old_prefix_from_content, ch_entity_id)
                        if updates > 0:
                            logger.debug(
                                f"Updated {updates} ID(s) in partintro extraction from "
                                f"{old_prefix_from_content} to {ch_entity_id}"
                            )
                            # Record the remapping for cross-file linkend fixup
                            prefix_remap[old_prefix_from_content] = ch_entity_id

                    # Now set the chapter ID and ensure all IDs are consistent
                    chapter_frag.set("id", ch_entity_id)
                    _ensure_fragment_ids(chapter_frag, ch_entity_id, "chapter")
                    fragments.append(
                        ChapterFragment(
                            ch_entity_id,
                            f"{ch_entity_id}.xml",
                            chapter_frag,
                            kind="chapter",
                            title=container_title_text or "Part Introduction",
                            section_type="chapter",
                        )
                    )
                    partintro_chapter_entity = etree.Entity(ch_entity_id)
                    partintro_chapter_entity.tail = "\n    "
                    logger.info(
                        f"Extracted {len(structural_children)} structural section(s) from "
                        f"partintro in {container_id} into chapter fragment {ch_entity_id} "
                        f"with title '{container_title_text}'"
                    )
                else:
                    # No structural content - keep partintro as-is
                    container_copy.append(deepcopy(inner))
            elif inner_local in pre_content_tags:
                container_copy.append(deepcopy(inner))

        # If structural content was extracted from partintro, add its entity
        # reference as the FIRST chapter child of this part (before other chapters).
        if partintro_chapter_entity is not None:
            container_copy.append(partintro_chapter_entity)

        for inner in container_elem:
            if not isinstance(inner.tag, str):
                container_copy.append(deepcopy(inner))
                continue
            inner_local = _local_name(inner)
            if inner_local in pre_content_tags:
                continue

            if inner_local in ("part", "subpart"):
                nested_copy = _emit_container_copy(inner, inner_local)
                nested_copy.tail = inner.tail
                container_copy.append(nested_copy)
                continue

            if _is_chapter_node(inner):
                # Determine section_type from tag name and role attribute
                inner_section_type = inner_local or "chapter"
                role = (inner.get("role") or "").lower()
                role_based_types = {
                    "acknowledgments": "acknowledgments",
                    "acknowledgements": "acknowledgments",
                    "contributors": "contributors",
                    "about": "about",
                }
                if role in role_based_types:
                    inner_section_type = role_based_types[role]
                # Preserve existing ID from upstream conversion if available
                ch_entity_id = _get_or_generate_entity_id(inner, inner_section_type)
                fragment_elem = deepcopy(inner)
                _ensure_fragment_ids(fragment_elem, ch_entity_id, inner_section_type)
                fragments.append(
                    ChapterFragment(
                        ch_entity_id,
                        f"{ch_entity_id}.xml",
                        fragment_elem,
                        kind="chapter",
                        title=_extract_title_text(inner),
                        section_type=inner_section_type,
                    )
                )
                ch_entity_node = etree.Entity(ch_entity_id)
                ch_entity_node.tail = inner.tail
                container_copy.append(ch_entity_node)
                continue

            container_copy.append(deepcopy(inner))

        container_copy.tail = container_elem.tail

        # Use existing ID from upstream conversion or generate a new one
        # This preserves IDs assigned by epub_to_structured_v2.py (e.g., pt0001)
        part_entity_id = _get_or_generate_entity_id(container_elem, container_kind)
        container_copy.set("id", part_entity_id)
        _ensure_fragment_ids(container_copy, part_entity_id, container_kind)

        # NOTE: We do NOT create separate part fragment files (pt0001.xml, etc.)
        # because the DocBook DTD requires <part> to have at least one child
        # structural element (chapter, appendix, etc.), but those are referenced
        # via entities that are only declared in Book.XML. A standalone part file
        # without chapters would be invalid according to the DTD.
        # Parts exist only inline in Book.XML with their entity references.
        logger.info(f"Part {part_entity_id} added inline to Book.XML (no separate fragment file)")

        # Return the container copy for inline placement in Book.XML
        return container_copy

    def _process_children(parent_copy: etree._Element, source_parent: etree._Element) -> None:
        for child in source_parent:
            if not isinstance(child.tag, str):
                parent_copy.append(deepcopy(child))
                continue

            if _is_toc_node(child):
                title_text = _extract_title_text(child)
                # Each TOC file gets its own entity ID and fragment
                # (previously some TOCs shared entities via special_entities caching,
                # causing duplicate content when multiple TOCs had similar titles/roles)
                # Preserve existing ID from upstream conversion if available
                entity_id = _get_or_generate_entity_id(child, "chapter")
                filename = f"{entity_id}.xml"
                fragment_elem = deepcopy(child)
                _ensure_fragment_ids(fragment_elem, entity_id, "toc")
                fragments.append(
                    ChapterFragment(
                        entity_id,
                        filename,
                        fragment_elem,
                        kind="toc",
                        title=title_text,
                        section_type="toc",
                    )
                )
                entity_node = etree.Entity(entity_id)
                entity_node.tail = child.tail
                parent_copy.append(entity_node)
                continue

            local_name = _local_name(child)
            if local_name in {"part", "subpart"}:
                parent_copy.append(_emit_container_copy(child, local_name))
                continue

            if _is_chapter_node(child):
                is_index_chapter = False
                if local_name == "chapter":
                    role = (child.get("role") or "").lower()
                    """
                    if role == "front-matter":
                        special_key = "front_matter"
                        entity_id = special_entities.get(special_key)
                        if entity_id is None:
                            entity_id = _next_entity_id()
                            special_entities[special_key] = entity_id
                            front_matter_entity_id = entity_id
                            front_matter_wrapper = etree.Element(
                                child.tag, attrib=dict(child.attrib), nsmap=child.nsmap
                            )
                            front_matter_wrapper.text = child.text
                            for descendant in child:
                                front_matter_wrapper.append(deepcopy(descendant))
                            filename = "FrontMatter.xml"
                            fragments.append(
                                ChapterFragment(
                                    entity_id,
                                    filename,
                                    front_matter_wrapper,
                                    kind="chapter",
                                    title=_extract_title_text(child) or "Front Matter",
                                    section_type="front-matter",
                                )
                            )
                            entity_node = etree.Entity(entity_id)
                            entity_node.tail = child.tail
                            parent_copy.append(entity_node)
                        else:
                            if front_matter_wrapper is not None:
                                for descendant in child:
                                    front_matter_wrapper.append(deepcopy(descendant))
                            if parent_copy:
                                parent_copy[-1].tail = child.tail
                        continue
                    """
                    if role == "index":
                        is_index_chapter = True
                    else:
                        title_text = _extract_title_text(child).strip().lower()
                        if title_text == "index":
                            is_index_chapter = True
                elif local_name == "index":
                    is_index_chapter = True

                # Determine section_type from tag name and role attribute
                # Special role types that should override the tag name
                section_type = local_name or "chapter"
                role = (child.get("role") or "").lower()
                role_based_types = {
                    "acknowledgments": "acknowledgments",
                    "acknowledgements": "acknowledgments",
                    "contributors": "contributors",
                    "about": "about",
                }
                if role in role_based_types:
                    section_type = role_based_types[role]
                if is_index_chapter:
                    # Each index file gets its own entity ID and fragment
                    # (previously all indexes shared one entity, causing duplicate content)
                    # Preserve existing ID from upstream conversion if available
                    entity_id = _get_or_generate_entity_id(child, "index")
                    filename = f"{entity_id}.xml"
                    fragment_elem = deepcopy(child)
                    _ensure_fragment_ids(fragment_elem, entity_id, "index")
                    fragments.append(
                        ChapterFragment(
                            entity_id,
                            filename,
                            fragment_elem,
                            kind="chapter",
                            title=_extract_title_text(child) or "Index",
                            section_type="index",
                        )
                    )
                else:
                    # Preserve existing ID from upstream conversion if available
                    entity_id = _get_or_generate_entity_id(child, section_type)
                    filename = f"{entity_id}.xml"
                    fragment_elem = deepcopy(child)
                    _ensure_fragment_ids(fragment_elem, entity_id, section_type)
                    fragments.append(
                        ChapterFragment(
                            entity_id,
                            filename,
                            fragment_elem,
                            kind="chapter",
                            title=_extract_title_text(child),
                            section_type=section_type,
                        )
                    )
                entity_node = etree.Entity(entity_id)
                entity_node.tail = child.tail
                parent_copy.append(entity_node)
                continue

            parent_copy.append(deepcopy(child))

    _process_children(root_copy, root)

    if not fragments:
        # Fallback: treat non-metadata children as a single chapter to ensure
        # downstream consumers receive at least one fragment.
        preserved = []
        extracted = []
        for child in root:
            if not isinstance(child.tag, str):
                preserved.append(deepcopy(child))
                continue
            if _local_name(child) in {"bookinfo", "info"}:
                preserved.append(deepcopy(child))
            else:
                extracted.append(deepcopy(child))

        entity_id = _next_entity_id("chapter")
        filename = f"{entity_id}.xml"
        wrapper = etree.Element("chapter")
        for node in extracted:
            wrapper.append(node)
        _ensure_fragment_ids(wrapper, entity_id, "chapter")
        fragments.append(
            ChapterFragment(entity_id, filename, wrapper, title="", section_type="chapter")
        )

        root_copy[:] = []
        root_copy.text = root.text
        for node in preserved:
            root_copy.append(node)
        entity_node = etree.Entity(entity_id)
        root_copy.append(entity_node)

    root_copy.tail = root.tail

    # Cross-file linkend fixup: When chapters were renumbered (e.g., ch0093 -> ch0101),
    # _update_id_prefixes updated IDs and linkends WITHIN each fragment. But cross-file
    # references (e.g., pr0002.xml linking to ch0093s0001fg01) still use old prefixes.
    # Apply all prefix remappings across ALL fragments to fix these.
    if prefix_remap:
        logger.info(f"Applying cross-file linkend fixup for {len(prefix_remap)} prefix remapping(s)")
        # Build compiled patterns for efficiency (many fragments to scan)
        remap_patterns = [
            (re.compile(rf'^{re.escape(old_pfx)}(.*)'), old_pfx, new_pfx)
            for old_pfx, new_pfx in prefix_remap.items()
        ]
        cross_file_updates = 0
        for fragment in fragments:
            for elem in fragment.element.iter():
                if not isinstance(elem.tag, str):
                    continue
                # Fix linkend attributes
                linkend = elem.get("linkend")
                if linkend:
                    for pattern, old_pfx, new_pfx in remap_patterns:
                        match = pattern.match(linkend)
                        if match:
                            new_linkend = f"{new_pfx}{match.group(1)}"
                            elem.set("linkend", new_linkend)
                            cross_file_updates += 1
                            break
                # Fix url attributes referencing internal IDs
                url = elem.get("url")
                if url:
                    for pattern, old_pfx, new_pfx in remap_patterns:
                        match = pattern.match(url)
                        if match:
                            new_url = f"{new_pfx}{match.group(1)}"
                            elem.set("url", new_url)
                            cross_file_updates += 1
                            break
        # Also fix linkend references in the Book.XML root (inline part content
        # like partintro may reference chapter IDs that were remapped)
        for elem in root_copy.iter():
            if not isinstance(elem.tag, str):
                continue
            linkend = elem.get("linkend")
            if linkend:
                for pattern, old_pfx, new_pfx in remap_patterns:
                    match = pattern.match(linkend)
                    if match:
                        new_linkend = f"{new_pfx}{match.group(1)}"
                        elem.set("linkend", new_linkend)
                        cross_file_updates += 1
                        break
        if cross_file_updates > 0:
            logger.info(f"Fixed {cross_file_updates} cross-file linkend/url reference(s) after chapter renumbering")

    # Convert cross-file linkend references to ulink.
    # Individual chapter XML files are validated standalone against the DTD.
    # The DTD requires linkend (IDREF) values to reference an id in the SAME
    # document.  Cross-file linkend values therefore cause IDREF validation
    # errors.  Convert them to <ulink url="target.xml#id"> which uses a CDATA
    # url attribute and is DTD-valid in standalone files.  The semantic link is
    # preserved for downstream tooling that expands entities in Book.XML.
    _convert_cross_file_linkends_to_ulink(fragments)

    return root_copy, fragments


def _ensure_toc_element(root: etree._Element) -> etree._Element:
    for child in root:
        if isinstance(child.tag, str) and _is_toc_node(child):
            return child

    toc = etree.Element("chapter")
    toc.set("role", "toc")
    title_el = etree.SubElement(toc, "title")
    title_el.text = "Table of Contents"

    insert_at = 0
    for idx, child in enumerate(root):
        if not isinstance(child.tag, str):
            continue
        if _local_name(child) in BOOKINFO_NODES:
            insert_at = idx + 1
        else:
            break

    root.insert(insert_at, toc)
    return toc


def _populate_toc_element(
    toc_element: etree._Element, chapter_fragments: Sequence[ChapterFragment]
) -> None:
    title_el = toc_element.find("title")
    if title_el is None:
        title_el = etree.SubElement(toc_element, "title")
    desired_title = "".join(title_el.itertext()).strip() or "Table of Contents"
    title_el.text = desired_title

    for child in list(toc_element):
        if child is title_el:
            continue
        toc_element.remove(child)

    itemized = etree.SubElement(toc_element, "itemizedlist")
    for fragment in chapter_fragments:
        listitem = etree.SubElement(itemized, "listitem")
        para = etree.SubElement(listitem, "para")
        link = etree.SubElement(para, "ulink")
        link.set("url", fragment.filename)
        chapter_title = fragment.title or fragment.filename
        link.text = chapter_title
        if fragment.title:
            link.tail = f" ({fragment.filename})"


def _build_structure_toc(
    book_root: etree._Element, fragments: Sequence[ChapterFragment]
) -> Optional[etree._Element]:
    """Generate a nested TOC from the Book.XML structure and fragments."""
    if book_root is None:
        return None

    entity_lookup = {fragment.entity: fragment for fragment in fragments}

    def _make_tocentry(title: str, *, linkend: Optional[str] = None,
                       url: Optional[str] = None) -> etree._Element:
        tocentry = etree.Element("tocentry")
        if linkend:
            tocentry.set("linkend", linkend)
        if url:
            ulink = etree.SubElement(tocentry, "ulink")
            ulink.set("url", url)
            ulink.text = title
        else:
            tocentry.text = title
        return tocentry

    def _make_front_back_entry(parent: etree._Element, tag: str, title: str,
                               *, linkend: Optional[str] = None,
                               url: Optional[str] = None) -> None:
        entry = etree.SubElement(parent, tag)
        if linkend:
            entry.set("linkend", linkend)
        if url:
            ulink = etree.SubElement(entry, "ulink")
            ulink.set("url", url)
            ulink.text = title
        else:
            entry.text = title

    def _add_chapter_entry(parent: etree._Element, fragment: ChapterFragment,
                           *, allow_front_back: bool) -> None:
        if fragment.kind != "chapter":
            return
        if fragment.section_type in {"part", "subpart"}:
            return
        chapter_id = (fragment.element.get("id") or "").strip()
        entry_title = fragment.title or fragment.filename
        if allow_front_back and fragment.section_type in FRONT_SECTION_TYPES:
            _make_front_back_entry(
                parent,
                "tocfront",
                entry_title,
                linkend=chapter_id or None,
                url=chapter_id or None,
            )
            return
        if allow_front_back and fragment.section_type in BACK_SECTION_TYPES:
            _make_front_back_entry(
                parent,
                "tocback",
                entry_title,
                linkend=chapter_id or None,
                url=chapter_id or None,
            )
            return
        tocchap = etree.SubElement(parent, "tocchap")
        tocchap.append(
            _make_tocentry(
                entry_title,
                linkend=chapter_id or None,
                url=chapter_id or None,
            )
        )

    toc = etree.Element("toc")
    toc.set("id", "toc0001")
    title_elem = etree.SubElement(toc, "title")
    title_elem.text = "Table of Contents"

    def _iter_entity_children(parent_elem: etree._Element) -> Iterable[etree._Entity]:
        for child in parent_elem:
            if isinstance(child, etree._Entity):
                yield child

    def _entity_fragment(entity_node: etree._Entity) -> Optional[ChapterFragment]:
        entity_name = getattr(entity_node, "name", None)
        if not entity_name:
            return None
        return entity_lookup.get(entity_name)

    entries_added = 0
    for child in book_root:
        if isinstance(child, etree._Entity):
            fragment = _entity_fragment(child)
            if fragment is not None:
                _add_chapter_entry(toc, fragment, allow_front_back=True)
                entries_added += 1
            continue

        if not isinstance(child.tag, str):
            continue

        local_name = _local_name(child)
        if local_name not in {"part", "subpart"}:
            continue

        tocpart = etree.SubElement(toc, "tocpart")
        part_id = (child.get("id") or "").strip()
        part_title = _extract_title_text(child) or local_name.capitalize()
        tocpart.append(_make_tocentry(part_title, linkend=part_id or None))

        for entity_child in _iter_entity_children(child):
            fragment = _entity_fragment(entity_child)
            if fragment is None:
                continue
            # Skip partintro chapters — toctransform.xsl handles the part
            # linkend via chapter[@role='partintro']/sect1[1]/@id
            if fragment.section_type == "partintro":
                continue
            _add_chapter_entry(tocpart, fragment, allow_front_back=False)
            entries_added += 1

        entries_added += 1

    if entries_added == 0:
        return None

    return toc


def _collect_known_ids(
    book_root: etree._Element, fragments: Sequence[ChapterFragment]
) -> Set[str]:
    ids: Set[str] = set()
    for elem in book_root.iter():
        if not isinstance(elem.tag, str):
            continue
        val = (elem.get("id") or "").strip()
        if val:
            ids.add(val)
    for fragment in fragments:
        for elem in fragment.element.iter():
            if not isinstance(elem.tag, str):
                continue
            val = (elem.get("id") or "").strip()
            if val:
                ids.add(val)
    return ids


def _generate_nested_toc_element(
    toc_structure: List[Dict],
    *,
    valid_linkends: Optional[Set[str]] = None,
) -> Optional[etree._Element]:
    """
    Generate a properly nested <toc> element from the TOC structure.

    Uses DocBook DTD-compliant elements:
    - <toc> as container
    - <tocfront> for front matter entries (text only, no nesting)
    - <tocpart> for Part-level entries
    - <tocchap> for Chapter-level entries (supports nesting)
    - <toclevel1> through <toclevel5> for nested sections
    - <tocback> for back matter entries (text only, no nesting)
    - <tocentry> with <ulink> for the actual links

    DTD Constraints:
    - <tocfront> and <tocback> can only contain text/ulink, not structural children
    - Entries with children MUST use <tocchap> regardless of semantic type

    Args:
        toc_structure: Nested list of TOC entry dicts from toc_structure.json
                      Each entry: {'id': str, 'title': str, 'href': str,
                                   'type': str|None, 'children': [...]}

    Returns:
        <toc> element or None if no structure provided
    """
    if not toc_structure:
        return None

    toc = etree.Element("toc")
    toc.set("id", "toc0001")
    title_elem = etree.SubElement(toc, "title")
    title_elem.text = "Table of Contents"

    # Collect entries by type for proper ordering (front, body/chapters, back)
    front_entries = []
    body_entries = []
    back_entries = []

    for entry in toc_structure:
        entry_type = entry.get('type')
        has_children = bool(entry.get('children'))

        # Entries with children must go to body (tocchap) for proper nesting
        if has_children:
            body_entries.append(entry)
        elif entry_type == 'front':
            front_entries.append(entry)
        elif entry_type == 'back':
            back_entries.append(entry)
        else:
            # Default to body/chapter
            body_entries.append(entry)

    def is_part_entry(entry: Dict) -> bool:
        """Check if an entry represents a Part (has children that are chapters)."""
        title_lower = entry.get('title', '').lower()
        id_lower = entry.get('id', '').lower()
        # Check for explicit Part markers
        if title_lower.startswith('part ') or title_lower.startswith('part:'):
            return True
        if id_lower.startswith('pt') or id_lower.startswith('part'):
            return True
        return False

    def _linkend_from_href(href: str) -> Optional[str]:
        """Extract linkend ID from href.

        Returns just the element ID for linkend attribute:
        - 'ch0009.xml' -> 'ch0009' (chapter ID)
        - 'ch0009.xml#ch0009s0001' -> 'ch0009s0001' (section ID from fragment)
        - 'pr0005#pr0005s0001a0007' -> 'pr0005s0001a0007' (element ID from fragment)
        - 'ch0009.xml#ch1' -> 'ch0009' (original EPUB anchor, fall back to chapter)

        If fragment looks like a valid generated ID (e.g., ch####s####...), use it.
        Otherwise use the chapter/file ID.
        """
        if not href:
            return None

        # If no # and no file extension, href is already an ID
        if '#' not in href and not re.search(r'\.(xml|xhtml|html)$', href, re.IGNORECASE):
            return href

        file_part, _, fragment = href.partition("#")

        # Extract base chapter ID from file part
        base = file_part.rsplit("/", 1)[-1]
        if base.lower().endswith(".xml"):
            base = base[:-4]
        elif base.lower().endswith(".xhtml"):
            base = base[:-6]
        elif base.lower().endswith(".html"):
            base = base[:-5]

        if fragment:
            # Check if fragment is a valid generated ID pattern
            # Pattern: 2-letter prefix + 4 digits, optionally followed by section/element suffixes
            # Examples: ch0009, ch0009s0001, pr0005s0001a0007
            if re.match(r'^[a-z]{2}\d{4}(s\d{4})?([a-z]{1,3}\d{4})?$', fragment):
                return fragment
            # Fragment is an original EPUB anchor (like 'ch1', 'ack'), use chapter ID
            return base if base else None

        return base if base else None

    def _maybe_set_linkend(elem: etree._Element, href: str) -> None:
        linkend = _linkend_from_href(href)
        if not linkend:
            return
        if valid_linkends is None or linkend in valid_linkends:
            elem.set("linkend", linkend)

    def _is_linkend_valid(linkend: str) -> bool:
        """Check if a linkend is valid (either no validation required or exists in valid_linkends)."""
        if valid_linkends is None:
            return True
        if linkend in valid_linkends:
            return True
        # Also check if the base chapter ID exists (for section-level IDs like ch0001s0001)
        base_match = re.match(r'^([a-z]{2}\d{4})', linkend)
        if base_match and base_match.group(1) in valid_linkends:
            return True
        return False

    def create_tocentry_with_link(entry: Dict) -> etree._Element:
        """Create a <tocentry> element with <link linkend="..."> inside.

        Uses link/linkend for internal cross-references (not ulink/url).
        The linkend is just the element ID (e.g., 'pr0001s0001'), no file prefix.
        Validates linkend against valid_linkends to prevent broken references.
        """
        tocentry = etree.Element("tocentry")
        href = entry.get("href", "")
        linkend = _linkend_from_href(href)
        if linkend and _is_linkend_valid(linkend):
            tocentry.set("linkend", linkend)
            link = etree.SubElement(tocentry, "link")
            link.set("linkend", linkend)
            link.text = entry.get('title', 'Untitled')
        else:
            # No valid linkend - just use text (no link element)
            if linkend and not _is_linkend_valid(linkend):
                logger.debug(f"TOC entry '{entry.get('title', '')}' has invalid linkend '{linkend}' - using text only")
            tocentry.text = entry.get('title', 'Untitled')
        return tocentry

    def create_tocfront_element(entry: Dict) -> etree._Element:
        """Create a <tocfront> element with link inside (no tocentry wrapper).

        DTD: <!ELEMENT tocfront (%para.char.mix;)*>
        tocfront contains text/link directly, not tocentry.
        Uses link/linkend for internal cross-references.
        Validates linkend against valid_linkends to prevent broken references.
        """
        tocfront = etree.Element("tocfront")
        href = entry.get("href", "")
        linkend = _linkend_from_href(href)
        if linkend and _is_linkend_valid(linkend):
            tocfront.set("linkend", linkend)
            link = etree.SubElement(tocfront, "link")
            link.set("linkend", linkend)
            link.text = entry.get('title', 'Untitled')
        else:
            if linkend and not _is_linkend_valid(linkend):
                logger.debug(f"TOC front entry '{entry.get('title', '')}' has invalid linkend '{linkend}' - using text only")
            tocfront.text = entry.get('title', 'Untitled')
        return tocfront

    def create_tocback_element(entry: Dict) -> etree._Element:
        """Create a <tocback> element with link inside (no tocentry wrapper).

        DTD: <!ELEMENT tocback (%para.char.mix;)*>
        tocback contains text/link directly, not tocentry.
        Uses link/linkend for internal cross-references.
        Validates linkend against valid_linkends to prevent broken references.
        """
        tocback = etree.Element("tocback")
        href = entry.get("href", "")
        linkend = _linkend_from_href(href)
        if linkend and _is_linkend_valid(linkend):
            tocback.set("linkend", linkend)
            link = etree.SubElement(tocback, "link")
            link.set("linkend", linkend)
            link.text = entry.get('title', 'Untitled')
        else:
            if linkend and not _is_linkend_valid(linkend):
                logger.debug(f"TOC back entry '{entry.get('title', '')}' has invalid linkend '{linkend}' - using text only")
            tocback.text = entry.get('title', 'Untitled')
        return tocback

    def add_children_at_level(parent: etree._Element, children: List[Dict], level: int) -> None:
        """Recursively add children at the appropriate TOC level."""
        if level > 5:
            # DTD only supports up to toclevel5, flatten remaining
            level = 5

        for child in children:
            if level == 0:
                # Should not happen at this point
                continue

            # Create toclevelN element
            toclevel = etree.SubElement(parent, f"toclevel{level}")
            toclevel.append(create_tocentry_with_link(child))

            # Recursively add grandchildren
            if child.get('children'):
                add_children_at_level(toclevel, child['children'], level + 1)

    def process_body_entry(entry: Dict) -> etree._Element:
        """Process a body/chapter entry and return the appropriate element."""
        if is_part_entry(entry):
            # Create tocpart for Part entries
            tocpart = etree.Element("tocpart")
            tocpart.append(create_tocentry_with_link(entry))

            # Part children become tocchap elements
            for child in entry.get('children', []):
                tocchap = etree.SubElement(tocpart, "tocchap")
                tocchap.append(create_tocentry_with_link(child))

                # Chapter children become toclevel1, etc.
                if child.get('children'):
                    add_children_at_level(tocchap, child['children'], 1)

            return tocpart
        else:
            # Create tocchap for Chapter entries
            tocchap = etree.Element("tocchap")
            tocchap.append(create_tocentry_with_link(entry))

            # Children become toclevel1, etc.
            if entry.get('children'):
                add_children_at_level(tocchap, entry['children'], 1)

            return tocchap

    # Add front matter entries first
    for entry in front_entries:
        toc.append(create_tocfront_element(entry))

    # Add body/chapter entries
    for entry in body_entries:
        toc.append(process_body_entry(entry))

    # Add back matter entries last
    for entry in back_entries:
        toc.append(create_tocback_element(entry))

    # Log statistics
    if front_entries or back_entries:
        logger.info(f"TOC categorization: {len(front_entries)} front, "
                   f"{len(body_entries)} body, {len(back_entries)} back")

    return toc


def _load_toc_structure_json(work_dir: Path) -> Optional[List[Dict]]:
    """
    Load TOC structure from JSON file.

    Args:
        work_dir: Directory containing toc_structure.json

    Returns:
        List of nested TOC entries or None if file not found
    """
    import json

    toc_json_path = work_dir / "toc_structure.json"
    if not toc_json_path.exists():
        logger.debug(f"No TOC structure JSON found at {toc_json_path}")
        return None

    try:
        with open(toc_json_path, 'r', encoding='utf-8') as f:
            toc_structure = json.load(f)
        logger.info(f"Loaded TOC structure with {len(toc_structure)} top-level entries")
        return toc_structure
    except Exception as e:
        logger.error(f"Failed to load TOC structure JSON: {e}")
        return None


def _iter_imagedata(element: etree._Element) -> Iterable[etree._Element]:
    for node in element.iter():
        if isinstance(node.tag, str) and _local_name(node) in {"imagedata", "graphic"}:
            if node.get("fileref"):
                yield node


def _extract_caption_text(figure: Optional[etree._Element]) -> str:
    if figure is None:
        return ""
    caption = figure.find("caption")
    if caption is not None:
        text = "".join(caption.itertext()).strip()
        if text:
            return text
    title = figure.find("title")
    if title is not None:
        text = "".join(title.itertext()).strip()
        if text:
            return text
    return ""


def _has_caption_or_label(
    figure: Optional[etree._Element], image_node: etree._Element
) -> bool:
    if figure is not None:
        if _extract_caption_text(figure):
            return True
        for attr in ("label", "id"):
            value = (figure.get(attr) or "").strip()
            if value:
                return True
        label_node = figure.find("label")
        if label_node is not None:
            text = "".join(label_node.itertext()).strip()
            if text:
                return True

    mediaobject = next(
        (ancestor for ancestor in image_node.iterancestors() if _local_name(ancestor) == "mediaobject"),
        None,
    )
    if mediaobject is not None:
        caption_node = mediaobject.find("caption")
        if caption_node is not None:
            text = "".join(caption_node.itertext()).strip()
            if text:
                return True

    for attr in ("label", "id"):
        value = (image_node.get(attr) or "").strip()
        if value:
            return True

    return False


def _extract_alt_text(image_node: etree._Element) -> str:
    alt = image_node.get("alt") or image_node.get("xlink:title")
    if alt:
        return alt.strip()

    mediaobject = next(
        (ancestor for ancestor in image_node.iterancestors() if _local_name(ancestor) == "mediaobject"),
        None,
    )
    if mediaobject is not None:
        for textobject in mediaobject.findall("textobject"):
            text = "".join(textobject.itertext()).strip()
            if text:
                return text
    return ""


DECORATIVE_KEYWORDS = {"logo", "watermark", "copyright", "trademark", "tm", "brand", "icon"}
BACKGROUND_KEYWORDS = {"background", "texture", "gradient", "border", "pattern", "header", "footer"}
BOOKINFO_NODES = {"bookinfo", "info", "titlepage"}
# Keywords for cover images (treated specially - always saved as decorative)
COVER_KEYWORDS = {"cover"}


def _is_full_page_image(image_node: etree._Element, image_data: Optional[bytes] = None) -> bool:
    """
    Detect if an image is a full-page image without meaningful text.
    
    Full-page images are typically:
    - Very large dimensions (close to page size)
    - Have no or minimal caption
    - Not part of a figure with meaningful label
    - Used as section dividers or decorative pages
    """
    # Check dimensions if we have the image data
    if image_data:
        width, height, _ = _inspect_image_bytes(image_data, ".jpg")
        if width > 0 and height > 0:
            # Typical page is ~600-800 pixels wide, ~800-1000 tall at 72dpi
            # Full page would be close to these dimensions
            is_large = (width > 500 and height > 700)
            aspect_ratio = height / width if width > 0 else 0
            # Page aspect ratio is typically 1.2-1.4 (letter/A4)
            is_page_like = 1.1 < aspect_ratio < 1.5
            
            if is_large and is_page_like:
                return True
    
    # Check if it's alone on a page (no siblings with substantial content)
    parent = image_node.getparent()
    if parent is not None:
        # Count text content near this image
        text_content = "".join(parent.itertext()).strip()
        # If very little text (< 100 chars), might be full page
        if len(text_content) < 100:
            return True
    
    return False


def _classify_image(
    image_node: etree._Element, 
    figure: Optional[etree._Element],
    image_data: Optional[bytes] = None
) -> str:
    """
    Classify image as 'content', 'decorative', or 'background'.
    
    Enhanced to detect:
    - Full-page images without text
    - Repeated decorative elements
    - Background images
    """
    original = image_node.get("fileref", "")
    name = Path(original).name.lower()
    
    # Images in figures are always content (they have captions/labels)
    if figure is not None:
        return "content"

    # Check for full-page images
    if _is_full_page_image(image_node, image_data):
        logger.info(f"Detected full-page image: {name} - treating as decorative")
        return "decorative"

    # Check ancestry - images in metadata sections are decorative
    ancestors = {_local_name(ancestor) for ancestor in image_node.iterancestors()}
    if ancestors & BOOKINFO_NODES:
        return "decorative"

    # Check for cover images - always treat as decorative so they get saved
    if any(keyword in name for keyword in COVER_KEYWORDS):
        logger.info(f"Detected cover image: {name} - treating as decorative to ensure it's saved")
        return "decorative"

    # Check filename for decorative keywords
    if any(keyword in name for keyword in DECORATIVE_KEYWORDS):
        return "decorative"

    # Check filename for background keywords
    if any(keyword in name for keyword in BACKGROUND_KEYWORDS):
        return "background"

    # Check role attribute
    role = (image_node.get("role") or "").lower()
    if role in {"decorative", "background"}:
        return "background" if role == "background" else "decorative"

    # Check size - very small images are likely decorative
    if image_data:
        width, height, _ = _inspect_image_bytes(image_data, ".jpg")
        if 0 < width < 50 or 0 < height < 50:
            logger.info(f"Detected small decorative image: {name} ({width}x{height}px)")
            return "decorative"

    return "content"


def _format_file_size(size_bytes: int) -> str:
    if size_bytes >= 1024 * 1024:
        return f"{size_bytes / (1024 * 1024):.1f}MB"
    if size_bytes >= 1024:
        return f"{size_bytes / 1024:.1f}KB"
    return f"{size_bytes}B"


def _inspect_image_bytes(data: bytes, fallback_suffix: str) -> Tuple[int, int, str]:
    if data.startswith(b"\x89PNG\r\n\x1a\n") and len(data) >= 24:
        width = int.from_bytes(data[16:20], "big", signed=False)
        height = int.from_bytes(data[20:24], "big", signed=False)
        return width, height, "PNG"

    if data.startswith(b"GIF87a") or data.startswith(b"GIF89a"):
        if len(data) >= 10:
            width = int.from_bytes(data[6:8], "little", signed=False)
            height = int.from_bytes(data[8:10], "little", signed=False)
            return width, height, "GIF"

    if data.startswith(b"\xff\xd8"):
        offset = 2
        length = len(data)
        while offset + 1 < length:
            if data[offset] != 0xFF:
                break
            marker = data[offset + 1]
            offset += 2
            if marker in {0xD8, 0xD9}:  # SOI/EOI
                continue
            if offset + 1 >= length:
                break
            block_length = int.from_bytes(data[offset : offset + 2], "big", signed=False)
            if marker in {0xC0, 0xC1, 0xC2, 0xC3, 0xC5, 0xC6, 0xC7, 0xC9, 0xCA, 0xCB, 0xCD, 0xCE, 0xCF}:
                if offset + 7 <= length:
                    height = int.from_bytes(data[offset + 3 : offset + 5], "big", signed=False)
                    width = int.from_bytes(data[offset + 5 : offset + 7], "big", signed=False)
                    return width, height, "JPEG"
                break
            offset += block_length

    suffix = fallback_suffix.lstrip(".")
    return 0, 0, suffix.upper() if suffix else ""


def _ensure_jpeg_bytes(data: bytes) -> bytes:
    """Convert arbitrary image bytes to baseline JPEG."""
    with Image.open(BytesIO(data)) as img:
        if img.mode not in ("RGB",):
            img = img.convert("RGB")
        buffer = BytesIO()
        img.save(buffer, format="JPEG", quality=92)
        return buffer.getvalue()


def _chapter_code(fragment: ChapterFragment) -> Tuple[str, str]:
    section_type = (fragment.section_type or "").lower()
    if fragment.kind == "toc" or section_type == "toc":
        return "TOC", "TOC"
    if section_type == "index":
        return "Index", "Index"
    if section_type in ("preface", "contributors", "about"):
        # Check role to give cover/titlepage their own image codes
        # so they don't consume Preface image numbers
        role = (fragment.element.get("role") or "").lower() if fragment.element is not None else ""
        if role == "cover":
            return "Cover", "Cover"
        if role in ("titlepage", "seriespage", "copyright-page"):
            return "Titlepage", "Titlepage"
        return "Preface", "Preface"

    if section_type == "appendix":
        title = fragment.title or ""
        match = re.search(r"appendix\s+([A-Z])", title, re.IGNORECASE)
        letter = match.group(1).upper() if match else "A"
        return f"Appendix{letter}", f"Appendix {letter}"

    match = re.match(r"pr(\d+)", fragment.entity, re.IGNORECASE)
    if match:
        return "Preface", "Preface"

    match = re.match(r"ch(\d+)", fragment.entity, re.IGNORECASE)
    if match:
        chapter_num = int(match.group(1))
        return f"Ch{chapter_num:04d}", str(chapter_num)

    return "Ch0001", "1"


def extract_bookinfo(root):
    """Extract book metadata from XML."""
    bookinfo = {
        'isbn': None,
        'title': None,
        'authors': [],
        'publisher': None,
        'pubdate': None,
        'edition': None,
        'copyright_holder': None,
        'copyright_year': None
    }
    
    info_elem = root.find('.//bookinfo') or root.find('.//info')
    
    if info_elem is not None:
        # ISBN
        isbn_elem = info_elem.find('.//isbn')
        if isbn_elem is not None and isbn_elem.text:
            isbn_clean = re.sub(r'[^0-9X]', '', isbn_elem.text.strip())
            if isbn_clean:
                bookinfo['isbn'] = isbn_clean
        
        # Title
        title_elem = info_elem.find('.//title')
        if title_elem is not None:
            bookinfo['title'] = ''.join(title_elem.itertext()).strip()
        
        # Authors
        author_elems = info_elem.findall('.//authorgroup/author') or info_elem.findall('.//author')
        for author_elem in author_elems:
            personname_elem = author_elem.find('.//personname')
            if personname_elem is not None:
                firstname = ''
                surname = ''
                firstname_elem = personname_elem.find('.//firstname')
                if firstname_elem is not None and firstname_elem.text:
                    firstname = firstname_elem.text.strip()
                surname_elem = personname_elem.find('.//surname')
                if surname_elem is not None and surname_elem.text:
                    surname = surname_elem.text.strip()
                if firstname or surname:
                    bookinfo['authors'].append(f"{firstname} {surname}".strip())
        
        # Publisher
        publisher_elem = info_elem.find('.//publisher/publishername')
        if publisher_elem is not None and publisher_elem.text:
            bookinfo['publisher'] = publisher_elem.text.strip()
        
        # Date
        pubdate_elem = info_elem.find('.//pubdate')
        if pubdate_elem is not None and pubdate_elem.text:
            bookinfo['pubdate'] = pubdate_elem.text.strip()
        
        # Edition
        edition_elem = info_elem.find('.//edition')
        if edition_elem is not None and edition_elem.text:
            bookinfo['edition'] = edition_elem.text.strip()
        
        # Copyright
        copyright_elem = info_elem.find('.//copyright')
        if copyright_elem is not None:
            year_elem = copyright_elem.find('.//year')
            if year_elem is not None and year_elem.text:
                bookinfo['copyright_year'] = year_elem.text.strip()
            holder_elem = copyright_elem.find('.//holder')
            if holder_elem is not None and holder_elem.text:
                bookinfo['copyright_holder'] = holder_elem.text.strip()
    
    return bookinfo

def create_bookinfo_element(bookinfo):
    """Create <bookinfo> element with placeholders for missing data."""
    bookinfo_elem = etree.Element('bookinfo')
    
    # ISBN
    isbn_elem = etree.SubElement(bookinfo_elem, 'isbn')
    isbn_elem.text = bookinfo.get('isbn') or '0000000000000'
    
    # Title
    title_elem = etree.SubElement(bookinfo_elem, 'title')
    title_elem.text = bookinfo.get('title') or 'Untitled Book'
    
    # Authors
    authorgroup_elem = etree.SubElement(bookinfo_elem, 'authorgroup')
    authors = bookinfo.get('authors', []) or ['Unknown Author']
    for author_name in authors:
        author_elem = etree.SubElement(authorgroup_elem, 'author')
        personname_elem = etree.SubElement(author_elem, 'personname')
        parts = author_name.split()
        if len(parts) >= 2:
            firstname_elem = etree.SubElement(personname_elem, 'firstname')
            firstname_elem.text = ' '.join(parts[:-1])
            surname_elem = etree.SubElement(personname_elem, 'surname')
            surname_elem.text = parts[-1]
        else:
            surname_elem = etree.SubElement(personname_elem, 'surname')
            surname_elem.text = author_name
    
    # Publisher
    publisher_elem = etree.SubElement(bookinfo_elem, 'publisher')
    publishername_elem = etree.SubElement(publisher_elem, 'publishername')
    publishername_elem.text = bookinfo.get('publisher') or 'Unknown Publisher'
    
    # Date
    pubdate_elem = etree.SubElement(bookinfo_elem, 'pubdate')
    pubdate_elem.text = bookinfo.get('pubdate') or '2024'
    
    # Edition
    edition_elem = etree.SubElement(bookinfo_elem, 'edition')
    edition_elem.text = bookinfo.get('edition') or '1st Edition'
    
    # Copyright
    copyright_elem = etree.SubElement(bookinfo_elem, 'copyright')
    year_elem = etree.SubElement(copyright_elem, 'year')
    year_elem.text = bookinfo.get('copyright_year') or bookinfo.get('pubdate') or '2024'
    holder_elem = etree.SubElement(copyright_elem, 'holder')
    holder_elem.text = bookinfo.get('copyright_holder') or bookinfo.get('publisher') or 'Copyright Holder'
    
    return bookinfo_elem

def _write_metadata_files(metadata_dir: Path, entries: List[ImageMetadata]) -> None:
    metadata_dir.mkdir(parents=True, exist_ok=True)

    catalog_path = metadata_dir / "image_catalog.xml"
    root = etree.Element("images")
    for entry in entries:
        image_el = etree.SubElement(root, "image")
        etree.SubElement(image_el, "filename").text = entry.filename
        etree.SubElement(image_el, "original_filename").text = entry.original_filename
        etree.SubElement(image_el, "chapter").text = entry.chapter
        etree.SubElement(image_el, "figure_number").text = entry.figure_number
        etree.SubElement(image_el, "caption").text = entry.caption
        etree.SubElement(image_el, "alt_text").text = entry.alt_text
        etree.SubElement(image_el, "referenced_in_text").text = str(entry.referenced_in_text).lower()
        etree.SubElement(image_el, "width").text = str(entry.width)
        etree.SubElement(image_el, "height").text = str(entry.height)
        etree.SubElement(image_el, "file_size").text = entry.file_size
        etree.SubElement(image_el, "format").text = entry.format

    catalog_path.write_bytes(
        etree.tostring(root, encoding="UTF-8", pretty_print=True, xml_declaration=True)
    )

    manifest_path = metadata_dir / "image_manifest.csv"
    with manifest_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(
            [
                "Filename",
                "Chapter",
                "Figure",
                "Caption",
                "Alt-Text",
                "Original_Name",
                "File_Size",
                "Format",
            ]
        )
        for entry in entries:
            writer.writerow(
                [
                    entry.filename,
                    entry.chapter,
                    entry.figure_number,
                    entry.caption,
                    entry.alt_text,
                    entry.original_filename,
                    entry.file_size,
                    entry.format,
                ]
            )


def _has_non_media_content(element: etree._Element) -> bool:
    if (element.text or "").strip():
        return True
    for child in element:
        if not isinstance(child.tag, str):
            continue
        if _local_name(child) == "mediaobject":
            continue
        if "".join(child.itertext()).strip():
            return True
    return False


def _prune_empty_media_branch(start: Optional[etree._Element]) -> None:
    current = start
    while current is not None and isinstance(current.tag, str):
        parent = current.getparent()
        local = _local_name(current)

        if local == "imageobject":
            if len(current) == 0 and not (current.text or "").strip() and not current.attrib:
                if parent is not None:
                    parent.remove(current)
                    current = parent
                    continue
            break

        if local == "mediaobject":
            has_visual_child = any(
                isinstance(child.tag, str)
                and _local_name(child) in {"imageobject", "imageobjectco", "graphic", "videoobject", "audioobject"}
                for child in current
            )
            if not has_visual_child:
                if parent is not None:
                    parent.remove(current)
                    current = parent
                    continue
            break

        if local == "figure":
            has_mediaobject = any(
                isinstance(child.tag, str) and _local_name(child) == "mediaobject" for child in current
            )
            if not has_mediaobject and not _has_non_media_content(current):
                if parent is not None:
                    parent.remove(current)
                    current = parent
                    continue
            break

        current = parent


def _remove_image_node(image_node: etree._Element) -> None:
    parent = image_node.getparent()
    if parent is not None:
        parent.remove(image_node)
        _prune_empty_media_branch(parent)


def _handle_decorative_image(
    image_node: etree._Element,
    decor_dir: Path,
    shared_dir: Path,
    decor_cache: Dict[str, Path],
    media_fetcher: Optional[MediaFetcher],
    hash_index: Optional[Dict[str, Dict[str, Any]]] = None,
) -> None:
    """
    Handle decorative/repeated images by:
    1. Detecting duplicates using SHA-256 hash
    2. Storing unique assets in the decorative folder and promoting duplicates to SharedImages
    3. Updating fileref to point to reusable location
    
    Args:
        image_node: The image element to process
        decor_dir: Directory for decorative/reused images
        decor_cache: Cache mapping original filename to saved path
        media_fetcher: Function to fetch image bytes
        hash_index: Optional dict mapping image hash to metadata (for duplicate detection)
    """
    original = image_node.get("fileref", "")
    if not original:
        return
    
    filename = Path(original).name or original
    decor_dir.mkdir(parents=True, exist_ok=True)
    shared_dir.mkdir(parents=True, exist_ok=True)
    
    # Check if we already processed this exact filename
    target_path = decor_cache.get(filename)
    if target_path is not None:
        # Already cached - ensure we point at current location
        rel = target_path.relative_to(decor_dir.parent)
        image_node.set("fileref", rel.name)
        logger.debug(f"Reusing cached decorative image: {filename}")
        return
    
    # Fetch the image data
    data = media_fetcher(original) if media_fetcher else None

    # If not found by full path, try just the filename in MultiMedia folder
    if data is None and media_fetcher:
        # Try fetching with just MultiMedia/filename
        alt_path = f"MultiMedia/{filename}"
        if alt_path != original:
            data = media_fetcher(alt_path)
            if data:
                logger.debug(f"Found image via alternate path: {alt_path}")

    if data is None or len(data) == 0:
        logger.warning("Skipping decorative image %s because it is missing or empty", original)
        _remove_image_node(image_node)
        return
    
    # Calculate SHA-256 hash for duplicate detection
    if hash_index is not None:
        image_hash = hashlib.sha256(data).hexdigest()
        entry = hash_index.get(image_hash)
        if entry:
            # Move original asset into SharedImages if this is the first duplicate encountered.
            if not entry["stored_in_shared"]:
                shared_path = shared_dir / entry["filename"]
                try:
                    entry["path"].rename(shared_path)
                except OSError:
                    shared_path.write_bytes(entry["path"].read_bytes())
                    entry["path"].unlink(missing_ok=True)
                entry["path"] = shared_path
                entry["stored_in_shared"] = True
                for node in entry["nodes"]:
                    node.set("fileref", entry['filename'])
                decor_cache[entry["filename"]] = shared_path
            entry["nodes"].append(image_node)
            image_node.set("fileref", entry['filename'])
            decor_cache[filename] = entry["path"]
            logger.info(
                "Detected duplicate decorative image: %s -> reusing %s in SharedImages",
                filename,
                entry["filename"],
            )
            return

        hash_index[image_hash] = {
            "filename": filename,
            "path": decor_dir / filename,
            "nodes": [image_node],
            "stored_in_shared": False,
        }

    # Save the image to decorative directory
    target_path = decor_dir / filename
    target_path.write_bytes(data)
    decor_cache[filename] = target_path
    final_name = filename
    image_node.set("fileref", final_name)

    # Track mapping in reference mapper (decorative images keep original name)
    if HAS_REFERENCE_MAPPER:
        try:
            mapper = get_mapper()
            for orig_path, ref in mapper.resources.items():
                if ref.intermediate_name == filename:
                    mapper.update_final_name(orig_path, final_name)
                    break
        except Exception as e:
            logger.debug(f"Could not update reference mapper for decorative image: {e}")
    logger.debug("Saved decorative image: %s", filename)

def _write_book_xml(
    target: Path,
    root_element: etree._Element,
    root_name: str,
    dtd_system: str,
    fragments: Sequence[ChapterFragment],
    *,
    processing_instructions: Sequence[Tuple[str, str]] = (),
    book_doctype_public: str = BOOK_DOCTYPE_PUBLIC_DEFAULT,
    book_doctype_system: Optional[str] = BOOK_DOCTYPE_SYSTEM_DEFAULT,
) -> None:
    header_lines = ['<?xml version="1.0" encoding="UTF-8"?>']
    for target_name, data in processing_instructions:
        header_lines.append(f"<?{target_name} {data}?>")

    doctype_system = book_doctype_system or dtd_system
    header_lines.append(
        f'<!DOCTYPE {root_name} PUBLIC "{book_doctype_public}" "{doctype_system}" ['  # noqa: E501
    )
    for fragment in fragments:
        header_lines.append(f'\t<!ENTITY {fragment.entity} SYSTEM "{fragment.filename}">')
    header_lines.append("]>")
    header_text = "\n".join(header_lines) + "\n\n"

    body = etree.tostring(root_element, encoding="UTF-8", pretty_print=True, xml_declaration=False)
    # BUG-13: Use write_bytes to guarantee LF line endings on all platforms.
    target.write_bytes((header_text + body.decode("utf-8")).encode("utf-8"))


def _write_fragment_xml(
    target: Path,
    element: etree._Element,
    *,
    processing_instructions: Sequence[Tuple[str, str]] = (),
) -> None:
    header_lines: List[str] = []

    # Add XML declaration
    header_lines.append('<?xml version="1.0" encoding="UTF-8"?>')

    for target_name, data in processing_instructions:
        header_lines.append(f"<?{target_name} {data}?>")

    header = "\n".join(header_lines) + "\n\n"

    body = etree.tostring(element, encoding="UTF-8", pretty_print=True, xml_declaration=False)
    xml_content = header + body.decode("utf-8")
    # BUG-13: Use write_bytes to guarantee LF line endings on all platforms.
    target.write_bytes(xml_content.encode("utf-8"))

    # Validate written XML can be re-parsed (catches malformed output early)
    # Skip validation for fragments containing entity references. Entity references
    # should only appear in Book.XML (not in individual part/chapter files), but
    # this check remains as a safeguard.
    body_str = body.decode("utf-8")
    has_entity_refs = re.search(r'&[a-z]{2}\d{4};', body_str)
    if not has_entity_refs:
        try:
            etree.fromstring(body)
        except etree.XMLSyntaxError as e:
            logger.error(f"CRITICAL: Fragment XML is malformed: {target} - {e}")
            raise ValueError(f"Generated fragment XML is malformed: {target} - {e}") from e


def package_docbook(
    root: etree._Element,
    root_name: str,
    dtd_system: str,
    zip_path: str,
    *,
    processing_instructions: Sequence[Tuple[str, str]] = (),
    assets: Sequence[Tuple[str, Path]] = (),
    media_fetcher: Optional[MediaFetcher] = None,
    book_doctype_public: str = BOOK_DOCTYPE_PUBLIC_DEFAULT,
    book_doctype_system: str = BOOK_DOCTYPE_SYSTEM_DEFAULT,
    source_format: str = "pdf",
    toc_json_path: Optional[Path] = None,
) -> Path:
    """Package the DocBook tree into a chapterised ZIP bundle.

    Args:
        source_format: Format of the source document ('pdf' or 'epub').
                      For ePub sources, ALL images are retained without filtering or categorization.
        toc_json_path: Optional path to toc_structure.json for nested TOC generation.
    """
    
    print("\n" + "="*70)
    print("STEP 5: Packaging DocBook to ZIP")
    print("="*70)

    # Check if we should bypass image filtering (for ePub sources)
    is_epub = source_format.lower() == "epub"
    if is_epub:
        print("  [OK] ePub source detected: ALL images will be retained without filtering")
        logger.info("ePub mode: Bypassing all image filtering and categorization")

    # Auto-detect and group chapters into parts when titles look like "Part X"
    parts_added = _auto_group_chapters_into_volumes_and_sections(root) or _auto_group_chapters_into_parts(root)
    if parts_added:
        print("  -> Detected volume/section/part marker chapters; inserted <part>/<subpart> wrappers for nested TOC")

    # Promote orphan chapters with part-like titles when some parts already exist
    # (handles case where EPUB detection misses some part files)
    orphans_promoted = _promote_orphan_part_chapters(root)
    if orphans_promoted:
        print("  -> Promoted orphan chapter(s) with part titles to <part> elements")

    # Create synthetic Part 1 when the first <part> starts at Part 2+
    # (handles EPUBs where Part 1 has no title page)
    part1_created = _insert_missing_part_one(root)
    if part1_created:
        print("  -> Created synthetic Part 1 for loose chapters before first part")

    print("  -> Splitting book into chapters...")
    book_root, fragments = _split_root(root)
    print(f"  -> Found {len(fragments)} fragments")

    # TOC generation disabled to keep Book.XML clean and DTD-compliant
    # toc_element = _ensure_toc_element(book_root)
    # chapter_fragments = [fragment for fragment in fragments if fragment.kind == "chapter"]
    # _populate_toc_element(toc_element, chapter_fragments)

    print("  -> Extracting book metadata...")
    filename_isbn = _extract_isbn_from_filename(Path(zip_path))
    isbn = filename_isbn or _extract_isbn(root)
    # Extract book metadata from original root
    bookinfo_data = _extract_bookinfo(root)
    
    # Prefer ISBN from filename; fall back to metadata if missing
    if filename_isbn:
        bookinfo_data['isbn'] = filename_isbn
    elif not bookinfo_data['isbn']:
        bookinfo_data['isbn'] = isbn
    
    # Log extracted metadata for verification
    logger.info("Book Metadata Extracted:")
    logger.info(f"  ISBN: {bookinfo_data.get('isbn') or '[Using placeholder]'}")
    logger.info(f"  Title: {bookinfo_data.get('title') or '[Using placeholder]'}")
    if bookinfo_data.get('authors'):
        logger.info(f"  Authors: {', '.join(bookinfo_data['authors'])}")
    else:
        logger.info("  Authors: [Using placeholder]")
    logger.info(f"  Publisher: {bookinfo_data.get('publisher') or '[Using placeholder]'}")
    logger.info(f"  Date: {bookinfo_data.get('pubdate') or '[Using placeholder]'}")
    logger.info(f"  Edition: {bookinfo_data.get('edition') or '[Using placeholder]'}")

    if _is_wiley_publisher(bookinfo_data.get('publisher')):
        fragments, removed_fragments, removed_refs = _filter_wiley_fragments(book_root, fragments)
        if removed_fragments:
            removed_labels = ", ".join(
                f"{frag.entity}:{(frag.title or _extract_title_text(frag.element) or 'Untitled')}"
                for frag in removed_fragments
            )
            logger.info(
                "Wiley cleanup: removed %d fragment(s) and %d Book.XML entity reference(s): %s",
                len(removed_fragments),
                removed_refs,
                removed_labels,
            )
            print(f"  -> Wiley cleanup removed {len(removed_fragments)} fragment(s)")
        else:
            logger.info("Wiley cleanup: no EULA/Index fragments found to remove")
    
    # Remove any existing bookinfo/info elements from book_root
    for elem in list(book_root.findall('.//bookinfo')):
        book_root.remove(elem)
    for elem in list(book_root.findall('.//info')):
        book_root.remove(elem)
    
    # Create new bookinfo element with all metadata
    bookinfo_elem = _create_bookinfo_element(bookinfo_data)
    
    # Insert bookinfo at the beginning of book_root
    # (after any existing title if present, otherwise at position 0)
    title_elem = book_root.find('.//title')
    if title_elem is not None and title_elem.getparent() is book_root:
        # Insert after title
        title_index = list(book_root).index(title_elem)
        book_root.insert(title_index + 1, bookinfo_elem)
    else:
        # Insert at beginning
        book_root.insert(0, bookinfo_elem)

    # TOC generation disabled: Book.XML consumers do not use <toc>.
    # (Nested TOC from toc_structure.json and fallback TOC are intentionally skipped.)

    base = _sanitise_basename(isbn or Path(zip_path).stem or "book")
    # Create two zip files: one with pre_fixes prefix and one with just ISBN
    pre_fixes_zip_path = Path(zip_path).parent / f"pre_fixes_{base}.zip"
    final_zip_path = Path(zip_path).parent / f"{base}.zip"

    with tempfile.TemporaryDirectory() as tmpdir:
        tmp_path = Path(tmpdir)
        book_path = tmp_path / "Book.XML"

        multi_media_dir = tmp_path / "MultiMedia"
        multi_media_dir.mkdir(parents=True, exist_ok=True)
        decorative_dir = multi_media_dir / "Decorative"
        shared_dir = multi_media_dir / "SharedImages"

        asset_paths: List[Tuple[str, Path]] = []
        for href, source in assets:
            try:
                data = Path(source).read_bytes()
            except OSError as exc:
                logger.warning("Failed to read stylesheet asset %s: %s", source, exc)
                continue
            target_path = (tmp_path / href).resolve()
            try:
                target_path.parent.mkdir(parents=True, exist_ok=True)
            except OSError as exc:
                logger.warning("Failed to create directory for stylesheet %s: %s", href, exc)
                continue
            target_path.write_bytes(data)
            asset_paths.append((href, target_path))

        chapter_paths: List[Tuple[ChapterFragment, Path]] = []
        metadata_entries: List[ImageMetadata] = []
        decor_cache: Dict[str, Path] = {}
        decor_hash_index: Dict[str, Dict[str, Any]] = {}  # SHA-256 hash -> metadata for duplicate detection

        # Track intermediate -> final name mapping to avoid duplicate image extraction
        intermediate_to_final: Dict[str, str] = {}
        duplicate_images_skipped = 0

        logger.info("Starting image processing with duplicate detection...")
        print("  -> Processing images and media...")

        # Track figure counters per chapter_code so fragments sharing the same
        # code (e.g., multiple "Preface" fragments) get sequential numbers
        # instead of all starting at 1 (which causes filename collisions).
        shared_figure_counters: Dict[str, int] = {}

        for frag_idx, fragment in enumerate(fragments, 1):
            if frag_idx % 10 == 0 or frag_idx == len(fragments):
                print(f"     Progress: {frag_idx}/{len(fragments)} fragments processed...")

            chapter_path = tmp_path / fragment.filename
            chapter_code, chapter_label = _chapter_code(fragment)
            figure_counter = shared_figure_counters.get(chapter_code, 0) + 1
            processed_nodes: Set[int] = set()

            # Debug: Count figures in this fragment
            figures_in_fragment = fragment.element.findall(".//figure")
            logger.debug(f"Fragment {fragment.filename}: Found {len(figures_in_fragment)} figure(s)")

            section_index: Dict[int, List[int]] = {}

            def _index_sections(node: etree._Element, prefix: List[int]) -> None:
                counter = 0
                for child in node:
                    if not isinstance(child.tag, str):
                        continue
                    if _local_name(child) == "section":
                        counter += 1
                        path = prefix + [counter]
                        section_index[id(child)] = path
                        _index_sections(child, path)
                    else:
                        _index_sections(child, prefix)

            _index_sections(fragment.element, [])

            def _section_suffix_for(node: etree._Element) -> str:
                ancestor = next(
                    (ancestor for ancestor in node.iterancestors() if _local_name(ancestor) == "section"),
                    None,
                )
                if ancestor is None:
                    return ""
                path = section_index.get(id(ancestor))
                if not path:
                    return ""
                return "s" + "".join(f"{value:02d}" for value in path)

            for figure in fragment.element.findall(".//figure"):
                caption_text = _extract_caption_text(figure)
                images = list(_iter_imagedata(figure))
                if not images:
                    logger.debug(f"  Figure with no images in {fragment.filename}")
                    continue
                logger.debug(f"  Figure in {fragment.filename} has {len(images)} image(s)")
                if len(images) == 1:
                    suffixes = [""]
                else:
                    suffixes = [
                        string.ascii_lowercase[idx]
                        if idx < len(string.ascii_lowercase)
                        else f"_{idx}"
                        for idx in range(len(images))
                    ]
                current_index = figure_counter
                saved_any = False
                for idx, image_node in enumerate(images):
                    processed_nodes.add(id(image_node))
                    original = image_node.get("fileref")
                    if not original:
                        logger.debug(f"    Image node has no fileref")
                        continue

                    logger.debug(f"    Fetching media for: {original}")
                    data = media_fetcher(original) if media_fetcher else None
                    if data is None:
                        logger.warning(f"    Media fetcher returned None for: {original}")

                    # Extract intermediate name for deduplication check
                    intermediate_name = Path(original).name if 'MultiMedia/' in original else original

                    # For ePub sources, bypass ALL filtering and include every image
                    if not is_epub:
                        classification = _classify_image(image_node, figure, data)

                        if classification == "background":
                            parent = image_node.getparent()
                            if parent is not None:
                                parent.remove(image_node)
                            logger.debug(f"Removed background image: {original}")
                            continue

                        if classification == "decorative":
                            _handle_decorative_image(
                                image_node,
                                decorative_dir,
                                shared_dir,
                                decor_cache,
                                media_fetcher,
                                decor_hash_index,
                            )
                            continue

                        # Check if image is referenced in mapper - if so, keep it even without caption
                        is_referenced = False
                        if HAS_REFERENCE_MAPPER:
                            try:
                                mapper = get_mapper()
                                # Check if this image is in the reference mapper
                                intermediate_name = Path(original).name if 'MultiMedia/' in original else original
                                for orig_path, ref in mapper.resources.items():
                                    if ref.intermediate_name == intermediate_name and ref.referenced_in:
                                        is_referenced = True
                                        logger.info(f"Image {intermediate_name} is referenced in chapters: {ref.referenced_in}")
                                        break
                            except (AttributeError, KeyError, TypeError) as e:
                                # Reference mapper lookup failed - treat as not referenced
                                logger.debug(f"Reference mapper lookup failed: {e}")
                                pass

                        if not _has_caption_or_label(figure, image_node) and not is_referenced:
                            logger.warning(
                                "Skipping media asset for %s because it lacks caption or label", original
                            )
                            _remove_image_node(image_node)
                            continue

                    # Check if we've already processed this image (deduplication)
                    if intermediate_name in intermediate_to_final:
                        existing_final_name = intermediate_to_final[intermediate_name]
                        image_node.set("fileref", existing_final_name)
                        duplicate_images_skipped += 1
                        logger.info(f"Reusing existing image: {intermediate_name} -> {existing_final_name}")
                        saved_any = True
                        continue

                    extension = ".jpg"
                    letter = suffixes[idx]
                    section_suffix = _section_suffix_for(image_node)
                    name_base = f"{chapter_code}{section_suffix}f{current_index:02d}{letter}"
                    new_filename = f"{name_base}{extension}"
                    target_path = multi_media_dir / new_filename

                    if data is None:
                        logger.warning("Missing media asset for %s; skipping", original)
                        _remove_image_node(image_node)
                        continue

                    if len(data) == 0:
                        logger.warning("Skipping media asset for %s because it is empty", original)
                        _remove_image_node(image_node)
                        continue

                    try:
                        jpeg_bytes = _ensure_jpeg_bytes(data)
                        target_path.write_bytes(jpeg_bytes)
                        width, height, fmt = _inspect_image_bytes(jpeg_bytes, extension)
                    except Exception as e:
                        logger.error(f"Failed to process/write image {original}: {e}")
                        _remove_image_node(image_node)
                        continue
                    file_size = _format_file_size(len(jpeg_bytes))
                    if width and height and (width < 72 or height < 72):
                        logger.warning(
                            "Low resolution image %s detected (%dx%d)", original, width, height
                        )

                    alt_text = _extract_alt_text(image_node)
                    if not alt_text:
                        logger.warning("Missing alt text for image %s", original)
                    referenced = bool((figure.get("id") or "").strip())
                    if not referenced and caption_text:
                        if re.search(r"figure\s+\d", caption_text, re.IGNORECASE):
                            referenced = True

                    metadata_entries.append(
                        ImageMetadata(
                            filename=new_filename,
                            original_filename=Path(original).name or original,
                            chapter=chapter_label,
                            figure_number=f"{current_index}{letter}",
                            caption=caption_text or "",
                            alt_text=alt_text,
                            referenced_in_text=referenced,
                            width=width,
                            height=height,
                            file_size=file_size,
                            format=fmt,
                        )
                    )
                    image_node.set("fileref", new_filename)

                    # Track intermediate -> final mapping in reference mapper
                    if HAS_REFERENCE_MAPPER:
                        try:
                            mapper = get_mapper()
                            # Extract intermediate name from original fileref
                            intermediate_name_for_mapper = Path(original).name if 'MultiMedia/' in original else original
                            # Update mapper with final name
                            for orig_path, ref in mapper.resources.items():
                                if ref.intermediate_name == intermediate_name_for_mapper:
                                    mapper.update_final_name(orig_path, new_filename)
                                    break
                        except Exception as e:
                            logger.debug(f"Could not update reference mapper: {e}")

                    # Track intermediate -> final mapping for deduplication
                    intermediate_to_final[intermediate_name] = new_filename

                    saved_any = True
                if saved_any:
                    figure_counter += 1

            for image_node in _iter_imagedata(fragment.element):
                if id(image_node) in processed_nodes:
                    continue
                original = image_node.get("fileref")
                if not original:
                    continue

                data = media_fetcher(original) if media_fetcher else None

                # Extract intermediate name for deduplication check
                intermediate_name = Path(original).name if 'MultiMedia/' in original else original

                # For ePub sources, bypass ALL filtering and include every image
                if not is_epub:
                    classification = _classify_image(image_node, None, data)

                    if classification == "background":
                        parent = image_node.getparent()
                        if parent is not None:
                            parent.remove(image_node)
                        logger.debug(f"Removed background image: {original}")
                        continue

                    if classification == "decorative":
                        _handle_decorative_image(
                            image_node,
                            decorative_dir,
                            shared_dir,
                            decor_cache,
                            media_fetcher,
                            decor_hash_index,
                        )
                        continue

                    if not _has_caption_or_label(None, image_node):
                        logger.warning(
                            "Skipping media asset for %s because it lacks caption or label", original
                        )
                        _remove_image_node(image_node)
                        continue

                # Check if we've already processed this image (deduplication)
                if intermediate_name in intermediate_to_final:
                    existing_final_name = intermediate_to_final[intermediate_name]
                    image_node.set("fileref", existing_final_name)
                    duplicate_images_skipped += 1
                    logger.info(f"Reusing existing image: {intermediate_name} -> {existing_final_name}")
                    continue

                extension = ".jpg"
                current_index = figure_counter
                section_suffix = _section_suffix_for(image_node)
                name_base = f"{chapter_code}{section_suffix}f{current_index:02d}"
                new_filename = f"{name_base}{extension}"
                target_path = multi_media_dir / new_filename

                if data is None:
                    logger.warning("Missing media asset for %s; skipping", original)
                    _remove_image_node(image_node)
                    continue

                if len(data) == 0:
                    logger.warning("Skipping media asset for %s because it is empty", original)
                    _remove_image_node(image_node)
                    continue

                try:
                    jpeg_bytes = _ensure_jpeg_bytes(data)
                    target_path.write_bytes(jpeg_bytes)
                    width, height, fmt = _inspect_image_bytes(jpeg_bytes, extension)
                except Exception as e:
                    logger.error(f"Failed to process/write image {original}: {e}")
                    _remove_image_node(image_node)
                    continue
                file_size = _format_file_size(len(jpeg_bytes))
                if width and height and (width < 72 or height < 72):
                    logger.warning(
                        "Low resolution image %s detected (%dx%d)", original, width, height
                    )
                alt_text = _extract_alt_text(image_node)
                if not alt_text:
                    logger.warning("Missing alt text for image %s", original)
                placeholder_caption = f"Figure {chapter_label}.{current_index:02d} (Unlabeled)"
                metadata_entries.append(
                    ImageMetadata(
                        filename=new_filename,
                        original_filename=Path(original).name or original,
                        chapter=chapter_label,
                        figure_number=str(current_index),
                        caption=placeholder_caption,
                        alt_text=alt_text,
                        referenced_in_text=False,
                        width=width,
                        height=height,
                        file_size=file_size,
                        format=fmt,
                    )
                )
                image_node.set("fileref", new_filename)

                # Track intermediate -> final mapping for deduplication
                intermediate_to_final[intermediate_name] = new_filename

                figure_counter += 1

            # Save the figure counter back to the shared tracker so the next
            # fragment sharing the same chapter_code continues numbering.
            shared_figure_counters[chapter_code] = figure_counter - 1

            # DTD fix: <preface> requires at least one block element.
            # Content model: (title, ..., ((block+ , sect*) | sect+), (bibliography)*)
            # A preface with only (title bibliography) is invalid.
            frag_local = _local_name(fragment.element)
            if frag_local == "preface":
                block_tags = {
                    "para", "simpara", "formalpara", "sect1", "section", "simplesect",
                    "itemizedlist", "orderedlist", "glosslist", "figure", "table",
                    "mediaobject", "blockquote", "sidebar", "qandaset", "anchor",
                    "literallayout", "synopsis", "address", "graphic", "equation",
                    "caution", "important", "note", "tip", "warning", "bridgehead",
                    "highlights", "authorblurb", "epigraph", "abstract", "indexterm",
                }
                has_block_or_sect = False
                for child_elem in fragment.element:
                    if isinstance(child_elem.tag, str) and _local_name(child_elem) in block_tags:
                        has_block_or_sect = True
                        break
                if not has_block_or_sect:
                    header_tags = {"title", "subtitle", "titleabbrev", "prefaceinfo", "beginpage"}
                    insert_idx = 0
                    for i, child_elem in enumerate(fragment.element):
                        if isinstance(child_elem.tag, str) and _local_name(child_elem) in header_tags:
                            insert_idx = i + 1
                        else:
                            break
                    para_tag = _qualified_tag(fragment.element, "para")
                    para_elem = etree.Element(para_tag)
                    para_elem.text = "\u00a0"  # non-breaking space
                    fragment.element.insert(insert_idx, para_elem)
                    logger.info(
                        "DTD fix: added <para> to preface %s (had no block content)",
                        fragment.filename,
                    )

            _write_fragment_xml(
                chapter_path,
                fragment.element,
                processing_instructions=processing_instructions,
            )
            chapter_paths.append((fragment, chapter_path))

        for image_node in _iter_imagedata(book_root):
            original = image_node.get("fileref")
            if not original:
                continue

            # Fetch data for classification
            data = media_fetcher(original) if media_fetcher else None

            # For ePub sources, bypass ALL filtering - handle all root images as decorative to preserve them
            if is_epub:
                _handle_decorative_image(
                    image_node,
                    decorative_dir,
                    shared_dir,
                    decor_cache,
                    media_fetcher,
                    decor_hash_index,
                )
            else:
                classification = _classify_image(image_node, None, data)

                if classification == "background":
                    parent = image_node.getparent()
                    if parent is not None:
                        parent.remove(image_node)
                    logger.debug(f"Removed background image from root: {original}")
                    continue

                if classification == "decorative":
                    _handle_decorative_image(
                        image_node,
                        decorative_dir,
                        shared_dir,
                        decor_cache,
                        media_fetcher,
                        decor_hash_index,
                    )
                else:
                    logger.warning(
                        "Unexpected content image in root document: %s; treating as decorative",
                        original,
                    )
                    _handle_decorative_image(
                        image_node,
                        decorative_dir,
                        shared_dir,
                        decor_cache,
                        media_fetcher,
                        decor_hash_index,
                    )

        # Deduplicate entity references to avoid ID_REDEFINED DTD errors
        fragments = _dedupe_fragments_by_entity(fragments)
        _dedupe_entity_references(book_root)

        _write_book_xml(
            book_path,
            book_root,
            root_name,
            dtd_system,
            fragments,
            processing_instructions=processing_instructions,
            book_doctype_public=book_doctype_public,
            book_doctype_system=book_doctype_system,
        )
        
        # Log image processing summary
        content_images = len(metadata_entries)
        decorative_files = sum(1 for path in set(decor_cache.values()) if path.parent == decorative_dir and path.exists())
        shared_files = sum(1 for path in set(decor_cache.values()) if path.parent == shared_dir and path.exists())
        duplicates_detected = sum(1 for entry in decor_hash_index.values() if entry.get("stored_in_shared"))
        
        logger.info(f"\n{'='*60}")
        logger.info("IMAGE PROCESSING SUMMARY")
        logger.info(f"{'='*60}")
        logger.info(f"Content images (in chapters): {content_images}")
        logger.info(f"Content image duplicates skipped: {duplicate_images_skipped}")
        logger.info(f"Decorative images: {decorative_files}")
        logger.info(f"Shared images: {shared_files}")
        logger.info(f"Decorative duplicates detected: {duplicates_detected}")
        logger.info(f"{'='*60}\n")
        
        print(f"  -> Content images: {content_images}")
        if duplicate_images_skipped > 0:
            print(f"  -> Content image duplicates skipped: {duplicate_images_skipped}")
        print(f"  -> Decorative images: {decorative_files}")
        print(f"  -> Shared images: {shared_files}")
        if duplicates_detected > 0:
            print(f"  -> Decorative duplicates detected: {duplicates_detected}")

        print("  -> Creating ZIP archives...")
        pre_fixes_zip_path.parent.mkdir(parents=True, exist_ok=True)
        # First create the pre_fixes ZIP
        with zipfile.ZipFile(pre_fixes_zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
            zf.write(book_path, "Book.XML")
            print(f"     Added: Book.XML")
            
            for fragment, chapter_path in chapter_paths:
                zf.write(chapter_path, fragment.filename)
            print(f"     Added: {len(chapter_paths)} chapter files")

            # Create MultiMedia folder in ZIP
            zf.writestr("MultiMedia/", "")

            media_count = 0
            media_errors = 0
            if multi_media_dir.exists():
                for media_file in sorted(multi_media_dir.rglob("*")):
                    if media_file.is_dir():
                        continue
                    rel_path = media_file.relative_to(multi_media_dir)
                    arcname = f"MultiMedia/{rel_path.as_posix()}"
                    try:
                        # Verify file is readable before adding to ZIP
                        if not media_file.exists() or media_file.stat().st_size == 0:
                            logger.warning(f"Skipping invalid media file: {arcname}")
                            media_errors += 1
                            continue
                        zf.write(media_file, arcname)
                        media_count += 1
                        logger.debug(f"Added media file to ZIP: {arcname}")
                    except Exception as e:
                        logger.error(f"Failed to add media file {arcname} to ZIP: {e}")
                        media_errors += 1
            print(f"     Added: {media_count} media files")
            if media_errors > 0:
                print(f"     Warning: {media_errors} media file(s) failed to add to ZIP")
            
            for href, asset_path in asset_paths:
                arcname = Path(href).as_posix()
                zf.write(asset_path, arcname)
            if asset_paths:
                print(f"     Added: {len(asset_paths)} asset files")

            # DTD files are NOT included in the package - they will be hosted on the production server
            # The DOCTYPE declaration points to http://LOCALHOST/dtd/V1.1/RittDocBook.dtd
            logger.info("DTD files excluded from package (hosted on production server)")
            print(f"     [INFO] DTD files excluded (production server will provide them)")

        # Create a copy of the ZIP with just the ISBN name
        print(f"  -> Creating second ZIP (clean package)...")
        shutil.copy2(pre_fixes_zip_path, final_zip_path)

        print(f"[OK] Created pre_fixes ZIP -> {pre_fixes_zip_path}")
        print(f"[OK] Created final ZIP -> {final_zip_path}")

    # Run TOC cleanup for title-based front/back matter reclassification
    # This ensures tocfront/tocback elements are properly classified even when
    # source EPUB lacks proper epub:type landmarks
    if HAS_TOC_CLEANUP:
        print("  -> Running TOC cleanup (front/back matter reclassification)...")
        try:
            toc_report = cleanup_toc_in_zip(final_zip_path, dry_run=False)
            if toc_report.reclassifications:
                print(f"     Reclassified {len(toc_report.reclassifications)} TOC entries:")
                for file_name, linkend, title, old_tag, new_tag in toc_report.reclassifications[:5]:
                    print(f"       - '{title}': {old_tag} -> {new_tag}")
                if len(toc_report.reclassifications) > 5:
                    print(f"       ... and {len(toc_report.reclassifications) - 5} more")
            else:
                print("     No TOC reclassifications needed")
            if toc_report.errors:
                for error in toc_report.errors:
                    logger.warning(f"TOC cleanup error: {error}")
        except Exception as e:
            logger.warning(f"TOC cleanup failed (non-fatal): {e}")
            print(f"     [WARN] TOC cleanup skipped: {e}")

    # Export reference mapping and validate
    if HAS_REFERENCE_MAPPER:
        try:
            mapper = get_mapper()
            # Export mapping to JSON
            mapping_path = final_zip_path.parent / f"{base}_reference_mapping.json"
            mapper.export_to_json(mapping_path)
            print(f"[OK] Exported reference mapping -> {mapping_path}")

            # Validate references
            is_valid, errors = mapper.validate(final_zip_path.parent)
            if not is_valid:
                print(f"[WARN] Reference validation warnings ({len(errors)} issues):")
                for error in errors[:10]:  # Show first 10 errors
                    print(f"    - {error}")
                if len(errors) > 10:
                    print(f"    ... and {len(errors) - 10} more")
            else:
                print(f"Reference validation passed")

            # Generate report
            report = mapper.generate_report()
            logger.info(f"\n{report}")
        except Exception as e:
            logger.warning(f"Could not export/validate reference mapping: {e}")

    # Return the final (clean) ZIP path - the pre_fixes ZIP is also created but this returns the clean one
    return final_zip_path


def make_file_fetcher(search_paths: Sequence[Path], reference_mapper=None) -> MediaFetcher:
    paths = [Path(p) for p in search_paths]
    logger.info(f"Media fetcher search paths: {[str(p) for p in paths]}")

    # Log mapper status for debugging
    if reference_mapper is None:
        logger.warning("MediaFetcher: reference_mapper is None! Will not be able to resolve final->intermediate names")
    elif not HAS_REFERENCE_MAPPER:
        logger.warning("MediaFetcher: HAS_REFERENCE_MAPPER is False! Reference mapper module not available")
    else:
        logger.info(f"MediaFetcher: Reference mapper has {len(reference_mapper.resources)} resources")
        # Log first few mappings for verification
        sample = list(reference_mapper.resources.items())[:3]
        for orig_path, ref in sample:
            logger.info(f"  Sample mapping: {ref.intermediate_name} -> {ref.final_name or 'NOT_SET'}")

    def _fetch(name: str) -> Optional[bytes]:
        # Build list of candidate paths
        candidates = []

        # First, try to resolve through reference mapper if available
        # This handles the case where name is a final name (e.g., Ch0017s0201f01.jpg)
        # but the actual file has an intermediate name (e.g., img_0000.png)
        search_name = name
        resolved_via_mapper = False
        if reference_mapper is not None and HAS_REFERENCE_MAPPER:
            # Remove MultiMedia/ prefix if present for mapping lookup
            lookup_name = name
            if name.startswith('MultiMedia/') or name.startswith('MultiMedia\\'):
                lookup_name = name.split('/', 1)[1] if '/' in name else name.split('\\', 1)[1]

            # Check if this is a final name in the mapping
            for orig_path, ref in reference_mapper.resources.items():
                if ref.final_name == lookup_name:
                    # Found it! Use the intermediate name instead
                    search_name = ref.intermediate_name
                    resolved_via_mapper = True
                    logger.debug(f"MediaFetcher: Resolved {name} -> {search_name} via reference mapper")
                    break

            # If we didn't find it as a final name, check if it's already an intermediate name
            if not resolved_via_mapper:
                for orig_path, ref in reference_mapper.resources.items():
                    if ref.intermediate_name == lookup_name:
                        # It's already an intermediate name
                        # Preserve MultiMedia/ prefix if original name had it
                        if name.startswith('MultiMedia/') or name.startswith('MultiMedia\\'):
                            search_name = name  # Keep original with prefix
                        else:
                            search_name = lookup_name  # Use without prefix
                        logger.debug(f"MediaFetcher: {name} is an intermediate name, using {search_name}")
                        break

        # If absolute path, try it directly
        if Path(search_name).is_absolute():
            candidates.append(Path(search_name))

        # Try each base path
        for base in paths:
            candidates.append(base / search_name)

            # Also try without MultiMedia prefix if present
            if search_name.startswith('MultiMedia/') or search_name.startswith('MultiMedia\\'):
                name_without_prefix = search_name.split('/', 1)[1] if '/' in search_name else search_name.split('\\', 1)[1]
                candidates.append(base / 'MultiMedia' / name_without_prefix)
                # Also try directly in base without MultiMedia subdirectory
                candidates.append(base / name_without_prefix)
            else:
                # For intermediate names (e.g., img_0000.png) that were resolved via mapper,
                # also try in MultiMedia subdirectory since ePub images are stored there
                if resolved_via_mapper or not Path(search_name).is_absolute():
                    candidates.append(base / 'MultiMedia' / search_name)

        # Try to read from each candidate
        for candidate in candidates:
            if candidate.exists():
                try:
                    data = candidate.read_bytes()
                    if len(data) > 0:
                        logger.debug(f"Media fetcher found: {name} -> {candidate}")
                        return data
                    else:
                        logger.warning(f"Media file is empty: {candidate}")
                except OSError as exc:
                    logger.warning("Failed reading media %s: %s", candidate, exc)

        # If not found, log all attempted paths for debugging
        logger.warning(f"Media fetcher could not find: {name}")
        logger.debug(f"  Original name: {name}")
        logger.debug(f"  Search name after mapper: {search_name}")
        logger.debug(f"  Resolved via mapper: {resolved_via_mapper}")
        logger.debug(f"  Total candidates tried: {len(candidates)}")
        logger.debug(f"  Attempted paths:")
        for idx, candidate in enumerate(candidates, 1):
            exists_status = "EXISTS" if candidate.exists() else "NOT FOUND"
            logger.debug(f"    {idx}. {exists_status}: {candidate}")
        return None

    return _fetch