#!/usr/bin/env python3
"""
ePub to Structured XML Converter (Version 2)

New architecture that:
1. Preserves XHTML structure (one XHTML file → one chapter XML)
2. Uses persistent reference mapping for all resources
3. Respects ePub spine order and navigation structure
4. Generates Book.XML from ePub metadata and TOC
5. No heuristics - uses native ePub structure

Key differences from v1:
- No chapter breakup based on H1 tags
- Each XHTML in spine becomes one chapter
- Reference mapping tracked throughout
- Validates all references before packaging

R2 XSL ID Support (IMPORTANT):
-------------------------------
R2 XSL has the <anchor> template output COMMENTED OUT in html.xsl. This means:
- DocBook <anchor> elements produce NO HTML output
- IDs on <para> elements produce NO HTML output (they use anchor template)
- IDs on sections produce NO HTML output ($generate.id.attributes = 0)
- Pagebreak/beginpage elements are completely ignored

Elements that DO get their IDs output by R2 XSL:
- figure, table, equation (via RittBook.xsl mode="delayed" template)
- note, sidebar, important, warning, caution, tip (via admon.ritt.xsl)
- qandaset (via qandaset.xsl)
- mediaobject for video/audio (via graphics.xsl)
- tocentry (via ritttoc.xsl)

R2 uses URL-based navigation (/resource/detail/{isbn}/{sectionId}) instead of
HTML anchors. Section IDs are used in URLs, not as HTML id attributes.

CONSEQUENCE: We intentionally do NOT create <anchor> elements or set IDs on
<para> elements since they would produce no output and just add bloat to XML.
"""

import argparse
import hashlib
import logging
import os
import os.path as posixpath
import re
import sys
import zipfile
from collections import OrderedDict
from copy import deepcopy
from dataclasses import dataclass
from io import BytesIO
from pathlib import Path
from typing import Any, Dict, List, Optional, Set, Tuple, Union

import warnings
import ebooklib
from bs4 import BeautifulSoup, NavigableString, Tag, Comment, XMLParsedAsHTMLWarning
from ebooklib import epub

# Suppress BeautifulSoup warning about parsing XML with HTML parser
# This is expected behavior for XHTML content and works correctly
warnings.filterwarnings("ignore", category=XMLParsedAsHTMLWarning)
from lxml import etree
from PIL import Image

from conversion_tracker import (ConversionStatus, ConversionTracker,
                                ConversionType, TemplateType)
# Import our reference mapper
from reference_mapper import (ReferenceMapper, ResourceReference, get_mapper,
                              reset_mapper)
# Import ID generator for R2 Library compliant IDs (consolidated in id_authority)
from id_authority import (
    IDGenerator, generate_chapter_id, generate_section_id as id_gen_section_id,
    sanitize_id, validate_id_simple as validate_id, MAX_ID_LENGTH,
)
# Import DTD compliance checker for comprehensive validation
from dtd_compliance import validate_and_fix_dtd_compliance, generate_compliance_report_xlsx

# Import comprehensive DTD fixer for auto-fixing structural issues
from comprehensive_dtd_fixer import ComprehensiveDTDFixer

# ============================================================================
# ID MANAGEMENT - SINGLE SOURCE OF TRUTH: id_authority.py
# ============================================================================
# All ID operations go through id_authority.py which provides:
# - Centralized ID tracking and mapping
# - Prescan functionality for EPUB files
# - Linkend resolution with audit trail
# - Export to JSON for debugging
# The old id_tracker and id_mapper modules have been removed.
from id_authority import (
    # Core authority
    get_authority, reset_authority, IDAuthority,
    # Definitions
    ChapterPrefix, ElementCode, VALID_CHAPTER_PREFIXES, XSL_RECOGNIZED_CODES,
    ELEMENT_CODES, get_element_code, DEFAULT_ELEMENT_CODE,
    # ID operations (via authority singleton)
    IDParser, IDRecord, IDState,
    # Validation results
    LinkendValidationReport,
    # Prescan
    SourceID, prescan_file, register_prescanned_file,
)

# ============================================================================
# PUBLISHER CONFIGURATION - CSS class and epub:type mappings
# ============================================================================
# Publisher-specific configurations control how CSS classes and epub:type
# values are mapped to DocBook elements. Configurations are loaded from
# config/publishers/ directory.
from publisher_config import (
    get_publisher_config, set_current_publisher, reset_publisher_config,
    get_css_mapping, get_epub_type_mapping, get_element_for_class,
    get_role_for_class, should_skip_element, resolve_css_classes,
    PublisherConfig, ElementMapping
)

# ============================================================================
# PUBLISHER CONFIG LEARNER - Learns patterns for config generation
# ============================================================================
# Tracks CSS classes and epub:type values during conversion to learn
# publisher-specific patterns and suggest configurations.
from publisher_config_learner import (
    get_learner, reset_learner, start_learning,
    record_class_usage, record_epub_type_usage,
    get_learning_stats, log_learning_summary,
    save_suggested_config, get_unrecognized_report
)

# ============================================================================
# EPUB METADATA - Extracted metadata handling functions
# ============================================================================
# Metadata extraction functions are now in epub_metadata.py for modularity.
# Import key functions for use in this module.
from epub_metadata import (
    extract_isbn_from_filename as _extract_isbn_from_filename_impl,
    get_opf_xml_from_epub as _get_opf_xml_impl,
    get_metadata_value as _get_metadata_value_impl,
    detect_publisher,
    get_spine_items,
    get_document_items,
    get_image_items,
    PUBLISHER_PATTERNS,
)

# ============================================================================
# PARALLEL PROCESSING - Concurrent chapter conversion support
# ============================================================================
# Parallel processing enables faster conversion for large EPUBs by processing
# multiple chapters concurrently. Thread-safe context isolation is used.
try:
    from parallel_processor import (
        ParallelChapterProcessor, ParallelConfig,
        parallel_convert_chapters, create_parallel_chapter_data,
        estimate_optimal_workers
    )
    PARALLEL_PROCESSING_AVAILABLE = True
except ImportError:
    PARALLEL_PROCESSING_AVAILABLE = False

# Auto-detection constant for parallel workers
# -1 means auto-detect optimal worker count based on system resources and chapter count
PARALLEL_AUTO_DETECT = -1


# ============================================================================
# ID OPERATION WRAPPERS - Use id_authority as single source of truth
# ============================================================================

def get_tracker():
    """Get the ID authority (replaces deprecated get_tracker)."""
    return get_authority()


def reset_tracker():
    """Reset the ID authority (replaces deprecated reset_tracker)."""
    reset_authority()


def register_id_mapping_unified(chapter_id: str, original_id: str, generated_id: str,
                                element_type: str = "", source_file: str = "") -> None:
    """Register an ID mapping using the centralized authority."""
    authority = get_authority()
    authority.map_id(chapter_id, original_id, generated_id, element_type, source_file)


def register_source_id_unified(chapter_id: str, source_id: str, element_type: str,
                               element_tag: str, context: str = "",
                               source_file: str = "") -> None:
    """Register a source ID using the centralized authority."""
    # The authority's map_id will be called when we have the generated ID
    # For now, we just track that we've seen this source ID
    pass  # Authority tracks this implicitly via map_id


def register_xml_id_unified(xml_id: str, element_type: str, chapter_id: str,
                            source_id: str = "", source_file: str = "") -> None:
    """Register a valid XML ID using the centralized authority."""
    authority = get_authority()
    authority.register_valid_id(xml_id)


def mark_id_dropped_unified(chapter_id: str, source_id: str, reason: str,
                            element_type: str = "", context: str = "",
                            source_file: str = "") -> None:
    """Mark an ID as dropped using the centralized authority."""
    authority = get_authority()
    # Include context in the reason if provided
    full_reason = f"{reason} ({context})" if context else reason
    authority.mark_dropped(chapter_id, source_id, full_reason, element_type, source_file)


def is_id_dropped_unified(chapter_id: str, source_id: str) -> bool:
    """Check if an ID is dropped using the centralized authority."""
    authority = get_authority()
    return authority.is_dropped(chapter_id, source_id)


def get_drop_reason_unified(chapter_id: str, source_id: str) -> Optional[str]:
    """Get the reason an ID was dropped using the centralized authority."""
    authority = get_authority()
    return authority.get_drop_reason(chapter_id, source_id)


def resolve_id_unified(chapter_id: str, source_id: str) -> Optional[str]:
    """Resolve a source ID to a generated ID using the centralized authority."""
    authority = get_authority()
    return authority.resolve(chapter_id, source_id)


def get_existing_mapping_unified(chapter_id: str, original_id: str) -> Optional[str]:
    """Get an existing ID mapping using the centralized authority."""
    authority = get_authority()
    return authority.resolve(chapter_id, original_id)
# Import DocBook Builder for validated element creation and XHTML transformation
from docbook_builder import (
    get_content_model, get_builder, reset_builder,
    get_transform_rule, get_docbook_tag, get_xhtml_mapping_stats,
    is_valid_child, get_valid_children, is_block_element, is_inline_element,
    ContentType, ElementCategory, ContentModelViolation,
    XHTML_TO_DOCBOOK_RULES,
    # XSL Requirements
    element_requires_id, is_id_ignored_by_xsl, get_linkend_element_type,
    get_id_prefix_for_element, check_xsl_id_requirements, fix_missing_xsl_ids,
    get_xsl_requirements_summary, XSL_ELEMENTS_REQUIRE_ID,
    # Ritt elements
    is_ritt_custom_element, validate_ritt_document, RITT_CUSTOM_ELEMENTS
)

# SVG support via cairosvg (required dependency - see requirements.txt)
# Note: cairosvg requires system-level Cairo library to be installed:
#   - Ubuntu/Debian: sudo apt-get install libcairo2-dev
#   - macOS: brew install cairo
#   - Windows: See https://www.cairographics.org/download/
try:
    import cairosvg
    HAS_CAIROSVG = True
except ImportError:
    HAS_CAIROSVG = False
    print(
        "ERROR: cairosvg not available - SVG images will be lost!\n"
        "       cairosvg is a required dependency (see requirements.txt).\n"
        "       It requires the Cairo graphics library to be installed:\n"
        "         Ubuntu/Debian: sudo apt-get install libcairo2-dev\n"
        "         macOS:         brew install cairo\n"
        "         Windows:       See https://www.cairographics.org/download/",
        file=sys.stderr
    )

# Note: Don't call basicConfig here - let the main entry point configure logging
# This allows --debug flag to work properly when called from integrated_pipeline.py
logger = logging.getLogger(__name__)

# EPUB semantic type sets (used for front/back classification).
FRONT_EPUB_TYPES = {
    'frontmatter', 'cover', 'titlepage', 'toc', 'copyright-page',
    'dedication', 'epigraph', 'foreword', 'preface', 'acknowledgments',
    'contributors', 'other-credits', 'errata', 'halftitlepage',
    'imprintum', 'seriespage',
}

BACK_EPUB_TYPES = {
    'backmatter', 'index', 'glossary', 'bibliography', 'endnotes',
    'appendix', 'afterword', 'colophon', 'credits',
}

FRONT_GUIDE_TYPES = {
    'cover', 'title-page', 'toc', 'copyright-page', 'dedication',
    'epigraph', 'foreword', 'preface', 'acknowledgements', 'acknowledgments',
    'contributors', 'other-credits', 'errata', 'titlepage',
}

BACK_GUIDE_TYPES = {
    'index', 'glossary', 'bibliography', 'notes', 'appendix',
    'afterword', 'colophon', 'loi', 'lot',
}

FRONT_BACK_ROLE_TYPES = sorted(
    FRONT_EPUB_TYPES
    | BACK_EPUB_TYPES
    | {f"doc-{t}" for t in FRONT_EPUB_TYPES | BACK_EPUB_TYPES}
)


# Global counter for auto-generated figure IDs when figure_counter not available
# This prevents duplicate IDs that occur with hash-based fallback
_global_autofig_counter = {'count': 0}

# EPUB namespace URI for epub:type attribute
EPUB_NS = 'http://www.idpf.org/2007/ops'

# ============================================================================
# FALLBACK CONTENT FOR REQUIRED ELEMENTS
# ============================================================================
# Some DTD elements require non-empty content for validity. These fallbacks
# provide meaningful placeholder text that also helps XSL rendering.

# Fallback for empty title elements (used in sections, figures, tables)
FALLBACK_TITLE = ""  # Empty string - XSL handles display
FALLBACK_SECTION_TITLE = ""  # Sections can have empty titles

# Fallback for elements that MUST have visible content
FALLBACK_FIGURE_TITLE = "Figure"  # Used when figure has no caption
FALLBACK_TABLE_TITLE = "Table"    # Used when table has no caption


def _ensure_title_content(title_elem: etree.Element, fallback: str = FALLBACK_TITLE,
                          context: str = "") -> bool:
    """
    Ensure a title element has content (text or child elements).

    If the title is empty, sets fallback text. This prevents DTD validation
    errors and ensures XSL can render something meaningful.

    Args:
        title_elem: The title element to check
        fallback: Fallback text to use if empty (default: FALLBACK_TITLE)
        context: Optional context for logging (e.g., "figure", "section")

    Returns:
        True if title had content, False if fallback was applied
    """
    # Check if title has any content
    has_text = title_elem.text and title_elem.text.strip()
    has_children = len(title_elem) > 0

    if has_text or has_children:
        return True

    # Apply fallback if provided
    if fallback:
        title_elem.text = fallback
        if context:
            logger.debug(f"Applied fallback title '{fallback}' to empty {context} title")

    return False


def _ensure_element_has_content(elem: etree.Element, fallback: str = " ",
                                 context: str = "") -> bool:
    """
    Ensure an element has some content (text or children).

    Some elements (like para, title) cannot be empty per DTD. This function
    adds minimal content if needed.

    Args:
        elem: Element to check
        fallback: Fallback content if empty (default: single space)
        context: Optional context for logging

    Returns:
        True if element had content, False if fallback was applied
    """
    has_text = elem.text and elem.text.strip()
    has_children = len(elem) > 0
    has_tail = any(child.tail and child.tail.strip() for child in elem)

    if has_text or has_children or has_tail:
        return True

    elem.text = fallback
    if context:
        logger.debug(f"Applied fallback content to empty {context}")
    return False


# ============================================================================
# DOCBOOK CONTENT MODEL VALIDATION HELPERS
# ============================================================================

# Cache for content model (initialized lazily on first use)
_content_model_cache = None

# Violation tracker for content model issues during conversion
# This tracks violations AS they happen during element creation
_conversion_violations: List[Dict[str, Any]] = []


def _get_cached_content_model():
    """Get cached DTD content model for validation."""
    global _content_model_cache
    if _content_model_cache is None:
        try:
            _content_model_cache = get_content_model()
        except Exception as e:
            logger.warning(f"Could not load DTD content model: {e}")
            return None
    return _content_model_cache


def reset_conversion_violations():
    """Reset the violation tracker for a new conversion."""
    global _conversion_violations
    _conversion_violations = []


def record_conversion_violation(parent_tag: str, child_tag: str,
                                 violation_type: str, message: str,
                                 context: Optional[str] = None):
    """Record a content model violation during conversion."""
    _conversion_violations.append({
        'parent': parent_tag,
        'child': child_tag,
        'type': violation_type,
        'message': message,
        'context': context,
    })


def get_conversion_violations() -> List[Dict[str, Any]]:
    """Get all violations recorded during conversion."""
    return _conversion_violations.copy()


def get_conversion_violation_summary() -> str:
    """Get summary of conversion violations."""
    if not _conversion_violations:
        return "No content model violations during conversion"

    # Group by type
    by_type: Dict[str, int] = {}
    for v in _conversion_violations:
        vtype = v['type']
        by_type[vtype] = by_type.get(vtype, 0) + 1

    parts = [f"{count} {vtype}" for vtype, count in sorted(by_type.items())]
    return f"Content model violations: {', '.join(parts)} (total: {len(_conversion_violations)})"


def validate_element_placement(parent_tag: str, child_tag: str) -> Tuple[bool, Optional[str]]:
    """
    Validate that a child element can be placed in a parent element.

    Args:
        parent_tag: DocBook parent element tag name
        child_tag: DocBook child element tag name

    Returns:
        Tuple of (is_valid, error_message if invalid)
    """
    model = _get_cached_content_model()
    if model is None:
        return True, None  # Can't validate without model

    if is_valid_child(parent_tag, child_tag):
        return True, None

    # Get valid alternatives
    valid_children = get_valid_children(parent_tag)
    if valid_children:
        suggestion = f"Valid children for <{parent_tag}> include: {sorted(list(valid_children)[:10])}"
    else:
        suggestion = f"<{parent_tag}> may not allow any child elements"

    return False, f"<{child_tag}> is not valid inside <{parent_tag}>. {suggestion}"


def get_docbook_element_for_html(html_tag: str, css_class: Optional[str] = None,
                                  parent_tag: Optional[str] = None) -> str:
    """
    Get the appropriate DocBook element tag for an HTML element.

    Uses the XHTML to DocBook transformation rules to find the best mapping.

    Args:
        html_tag: HTML tag name (e.g., 'div', 'span', 'p')
        css_class: Optional CSS class to match specific rules
        parent_tag: Optional parent DocBook tag for context-aware selection

    Returns:
        DocBook tag name (e.g., 'para', 'emphasis', 'sidebar')
    """
    return get_docbook_tag(html_tag, css_class, parent_tag)


# =============================================================================
# PREVENTIVE DTD VALIDATION CONFIGURATION
# =============================================================================

# Auto-correction mappings for invalid parent-child relationships
# When child is invalid in parent, try these alternatives
CHILD_CORRECTION_MAP = {
    # (parent, invalid_child) -> corrected_child or None to skip
    ('para', 'figure'): None,  # figure can't be in para - let it bubble up
    ('para', 'table'): None,   # table can't be in para
    ('para', 'sidebar'): None, # sidebar can't be in para
    ('para', 'note'): None,    # note can't be in para
    ('para', 'para'): None,    # nested para not allowed
    ('entry', 'figure'): None, # figure should be outside entry
    ('entry', 'sidebar'): None,
    ('title', 'para'): None,   # para not allowed in title
    ('title', 'emphasis'): 'emphasis',  # OK
    ('phrase', 'para'): None,  # para not in phrase
}

# Wrapper elements for auto-wrapping content
WRAPPER_MAP = {
    # child -> wrapper when child needs to be wrapped
    'text_in_entry': 'para',  # Text in entry should be wrapped in para
    'text_in_section': 'para',
    'inline_in_section': 'para',
}

# Elements that can be safely converted to alternatives
ELEMENT_ALTERNATIVES = {
    'simpara': 'para',  # simpara -> para
    'section': 'sect1', # section -> sect1
    'formalpara': 'para',  # formalpara -> para (for simplicity)
}


def get_corrected_element(parent_tag: str, child_tag: str) -> Optional[str]:
    """
    Get a corrected element tag when child is invalid in parent.

    Args:
        parent_tag: Parent element tag
        child_tag: Invalid child element tag

    Returns:
        Corrected tag name, or None if element should be skipped/bubbled up
    """
    key = (parent_tag, child_tag)

    # Check explicit correction map
    if key in CHILD_CORRECTION_MAP:
        return CHILD_CORRECTION_MAP[key]

    # Check if there's a simple alternative
    if child_tag in ELEMENT_ALTERNATIVES:
        alt = ELEMENT_ALTERNATIVES[child_tag]
        if is_valid_child(parent_tag, alt):
            return alt

    return child_tag  # No correction available


def try_auto_fix_placement(parent: etree._Element, child_tag: str,
                           text: Optional[str] = None,
                           context: Optional[str] = None,
                           **attrs) -> Tuple[Optional[etree._Element], bool]:
    """
    Try to auto-fix an invalid child placement.

    Strategies:
    1. Check if element should be skipped (returns None, True)
    2. Check if element can be converted to an alternative
    3. Check if element should bubble up to grandparent

    Args:
        parent: Parent element
        child_tag: Child element tag
        text: Optional text content
        context: Optional context

    Returns:
        Tuple of (element or None, was_fixed)
    """
    parent_tag = parent.tag

    # Check if this combination has a correction
    corrected = get_corrected_element(parent_tag, child_tag)

    if corrected is None:
        # Element should be skipped or handled differently
        logger.debug(f"Auto-fix: Skipping <{child_tag}> in <{parent_tag}>")
        return None, True

    if corrected != child_tag:
        # Use corrected element
        logger.debug(f"Auto-fix: Converting <{child_tag}> to <{corrected}> in <{parent_tag}>")
        elem = etree.SubElement(parent, corrected, **attrs)
        if text:
            elem.text = text
        return elem, True

    return None, False


def create_validated_element(tag: str, parent: Optional[etree._Element] = None,
                             text: Optional[str] = None,
                             context: Optional[str] = None,
                             auto_fix: bool = True,
                             **attrs) -> etree._Element:
    """
    Create a DocBook element with preventive content model validation.

    Validates parent-child relationship DURING element creation (not after).
    This is the core of the "validate as you build" architecture.

    NEW: With auto_fix=True (default), attempts to automatically correct
    invalid placements by:
    1. Converting element to a valid alternative
    2. Wrapping content appropriately
    3. Recording the fix for transparency

    Args:
        tag: DocBook element tag name
        parent: Optional parent element
        text: Optional text content
        context: Optional context string for violation tracking (e.g., "heading h2")
        auto_fix: If True, attempt to auto-correct invalid placements
        **attrs: Element attributes

    Returns:
        Created lxml element
    """
    # Validate placement if parent provided
    if parent is not None:
        is_valid, error_msg = validate_element_placement(parent.tag, tag)
        if not is_valid:
            # Try auto-fix first if enabled
            if auto_fix:
                fixed_elem, was_fixed = try_auto_fix_placement(
                    parent, tag, text, context, **attrs
                )
                if was_fixed and fixed_elem is not None:
                    record_conversion_violation(
                        parent.tag, tag, 'auto_fixed',
                        f"Auto-fixed: {error_msg}", context
                    )
                    return fixed_elem
                elif was_fixed and fixed_elem is None:
                    # Element should be skipped - create orphan element
                    logger.warning(f"Element <{tag}> skipped in <{parent.tag}>: {error_msg}")
                    # Return a detached element so caller can handle it
                    elem = etree.Element(tag, **attrs)
                    if text:
                        elem.text = text
                    return elem

            # Record violation for tracking
            record_conversion_violation(
                parent.tag, tag, 'invalid_child', error_msg, context
            )
            # Log at debug level (not warning) since we're tracking violations
            logger.debug(f"Content model: {error_msg}")

    # Create element
    if parent is None:
        elem = etree.Element(tag, **attrs)
    else:
        elem = etree.SubElement(parent, tag, **attrs)

    # Set text content with validation
    if text:
        model = _get_cached_content_model()
        if model and not model.allows_text(tag):
            # Try to wrap text in appropriate element
            if auto_fix and is_valid_child(tag, 'para'):
                # Wrap text in para
                para = etree.SubElement(elem, 'para')
                para.text = text
                record_conversion_violation(
                    tag, '#PCDATA', 'text_auto_wrapped',
                    f"Text in <{tag}> auto-wrapped in <para>", context
                )
                logger.debug(f"Auto-fix: Wrapped text in <{tag}> with <para>")
            else:
                record_conversion_violation(
                    tag, '#PCDATA', 'text_not_allowed',
                    f"<{tag}> does not allow text content", context
                )
                logger.debug(f"Content model: <{tag}> does not allow text content")
                elem.text = text  # Still set it, DTD fixer will handle
        else:
            elem.text = text

    return elem


def validated_subelement(parent: etree._Element, tag: str,
                         text: Optional[str] = None,
                         context: Optional[str] = None,
                         **attrs) -> etree._Element:
    """
    Create a validated child element (shorthand for create_validated_element with parent).

    This is the primary function to use during conversion for creating child elements.
    It validates the parent-child relationship during creation.

    Args:
        parent: Parent element (required)
        tag: Child element tag name
        text: Optional text content
        context: Optional context for violation tracking
        **attrs: Element attributes

    Returns:
        Created child element
    """
    return create_validated_element(tag, parent, text, context, **attrs)


def validated_element(tag: str, text: Optional[str] = None,
                      context: Optional[str] = None,
                      **attrs) -> etree._Element:
    """
    Create a validated root element (no parent).

    Args:
        tag: Element tag name
        text: Optional text content
        context: Optional context for violation tracking
        **attrs: Element attributes

    Returns:
        Created element
    """
    return create_validated_element(tag, None, text, context, **attrs)


def validated_append(parent: etree._Element, child: etree._Element,
                     context: Optional[str] = None) -> None:
    """
    Append a child element to parent with content model validation.

    Use this when the child was created separately (e.g., with etree.Element)
    and needs to be appended later.

    Args:
        parent: Parent element
        child: Child element to append
        context: Optional context for violation tracking
    """
    is_valid, error_msg = validate_element_placement(parent.tag, child.tag)
    if not is_valid:
        record_conversion_violation(
            parent.tag, child.tag, 'invalid_child', error_msg, context
        )
        logger.debug(f"Content model: {error_msg}")
    parent.append(child)


def validate_element_children(elem: etree._Element) -> List[str]:
    """
    Validate all children of an element against the content model.

    Returns list of validation warnings.
    """
    warnings = []
    model = _get_cached_content_model()
    if model is None:
        return warnings

    parent_tag = elem.tag
    child_tags = [child.tag for child in elem]

    # Check each child
    for child_tag in child_tags:
        is_valid, error_msg = validate_element_placement(parent_tag, child_tag)
        if not is_valid:
            warnings.append(error_msg)

    # Check for required children
    missing = model.get_missing_required_children(parent_tag, set(child_tags))
    for req in missing:
        warnings.append(f"<{parent_tag}> is missing required child <{req}>")

    # Check element ordering
    is_valid_order, order_error = model.validate_child_order(parent_tag, child_tags)
    if not is_valid_order:
        warnings.append(order_error)

    return warnings


def log_element_creation_stats():
    """Log statistics about XHTML to DocBook mappings for diagnostics."""
    try:
        stats = get_xhtml_mapping_stats()
        logger.info(f"DocBook Builder: {stats['xhtml_tags']} XHTML tags mapped to "
                   f"{stats['docbook_targets']} DocBook elements via {stats['total_rules']} rules")
    except Exception as e:
        logger.debug(f"Could not get mapping stats: {e}")


def transform_xhtml_element(html_tag: str, parent_elem: etree._Element,
                            css_class: Optional[str] = None,
                            text: Optional[str] = None,
                            **attrs) -> Optional[etree._Element]:
    """
    Transform an XHTML element to DocBook using the mapping rules.

    This function provides a fallback transformation when specific handlers
    don't match. It uses the XHTML_TO_DOCBOOK_RULES to determine the
    appropriate DocBook element.

    Args:
        html_tag: HTML tag name (e.g., 'div', 'span', 'aside')
        parent_elem: Parent lxml element
        css_class: Optional CSS class for class-specific rules
        text: Optional text content
        **attrs: Additional attributes to set

    Returns:
        Created DocBook element, or None if no mapping exists
    """
    from docbook_builder import get_transform_rule, TransformRule

    # Get the parent tag for context-aware selection
    parent_tag = parent_elem.tag if hasattr(parent_elem, 'tag') else None

    # Look up the transformation rule
    rule = get_transform_rule(html_tag, css_class, parent_tag)
    if not rule:
        return None

    # Skip elements that shouldn't preserve content (script, style, etc.)
    if not rule.preserve_text and not rule.preserve_children:
        return None

    docbook_tag = rule.docbook_tag

    # Validate placement
    is_valid, error_msg = validate_element_placement(parent_tag, docbook_tag)
    if not is_valid:
        # Try fallback tag if available
        if rule.fallback_tag:
            fallback_valid, _ = validate_element_placement(parent_tag, rule.fallback_tag)
            if fallback_valid:
                docbook_tag = rule.fallback_tag
            else:
                logger.debug(f"XHTML transform: {error_msg}, using fallback {rule.fallback_tag}")
        else:
            logger.debug(f"XHTML transform: {error_msg}")

    # Create the element
    elem = validated_subelement(parent_elem, docbook_tag)

    # Copy specified attributes
    for attr in rule.attrs_to_copy:
        if attr in attrs:
            elem.set(attr, attrs[attr])

    # Set additional attributes
    for key, value in attrs.items():
        if key not in rule.attrs_to_copy and key not in ('class', 'style'):
            elem.set(key, str(value))

    # Set text content
    if text and rule.preserve_text:
        elem.text = text

    return elem


def get_docbook_tag_for_css_class(html_tag: str, css_class: str) -> Tuple[Optional[str], Optional[str]]:
    """
    Get the DocBook tag and role for an HTML element with a CSS class.

    Uses publisher configuration as primary source, falling back to
    docbook_builder transform rules if no config match found.

    Args:
        html_tag: HTML tag name (e.g., 'div', 'span', 'p')
        css_class: CSS class name to look up

    Returns:
        Tuple of (docbook_tag, role) or (None, None) if no mapping
    """
    # First, check publisher configuration
    # Handle multiple classes by splitting and checking each
    if css_class:
        classes = css_class.split() if ' ' in css_class else [css_class]
        for cls in classes:
            mapping = get_css_mapping(cls)
            if mapping and not mapping.skip:
                # Record this usage for learning
                record_class_usage(
                    cls, html_tag, mapping.element, mapping.role,
                    matched_by="config"
                )
                return mapping.element, mapping.role

    # Fall back to docbook_builder transform rules
    from docbook_builder import get_transform_rule
    rule = get_transform_rule(html_tag, css_class)
    if rule:
        # Record this usage for learning
        if css_class:
            for cls in (css_class.split() if ' ' in css_class else [css_class]):
                record_class_usage(
                    cls, html_tag, rule.docbook_tag, None,
                    matched_by="docbook_builder"
                )
        return rule.docbook_tag, None

    # Record unmapped classes for learning
    if css_class:
        for cls in (css_class.split() if ' ' in css_class else [css_class]):
            record_class_usage(cls, html_tag, matched_by="unmapped")

    return None, None


def _get_epub_type(node) -> str:
    """
    Robustly get the epub:type attribute from a BeautifulSoup element.

    BeautifulSoup with lxml-xml parser may store namespaced attributes in
    different ways:
    - As 'epub:type' (string key with prefix)
    - As '{http://www.idpf.org/2007/ops}type' (Clark notation)
    - In the element's attrs dict with the namespace prefix

    This helper tries all known patterns to ensure epub:type detection works
    regardless of how BeautifulSoup parses the attribute.

    Args:
        node: BeautifulSoup element to get epub:type from

    Returns:
        The epub:type value as a string, or empty string if not found
    """
    if not hasattr(node, 'get'):
        return ''

    # Try standard string key (works with HTML parser)
    epub_type = node.get('epub:type', '')
    if epub_type:
        return epub_type

    # Try Clark notation (may be used by XML parser)
    epub_type = node.get(f'{{{EPUB_NS}}}type', '')
    if epub_type:
        return epub_type

    # Try looking in attrs dict for any key ending in ':type' or 'type'
    # This handles cases where namespace might be differently represented
    if hasattr(node, 'attrs') and isinstance(node.attrs, dict):
        for key, value in node.attrs.items():
            if isinstance(key, str):
                # Check for epub:type with any namespace prefix
                if key == 'epub:type' or key.endswith(':type') and 'epub' in key.lower():
                    return value if isinstance(value, str) else ''
                # Check for Clark notation variations
                if key.startswith('{') and key.endswith('}type'):
                    return value if isinstance(value, str) else ''

    # Also try data-type as a fallback (common alternative)
    return node.get('data-type', '') or ''


def get_docbook_element_for_epub_type(epub_type: str) -> Optional[str]:
    """
    Get the DocBook element for an epub:type value.

    Uses publisher configuration to map epub:type values to DocBook elements.
    This enables publisher-specific handling of semantic types.

    Args:
        epub_type: The epub:type value (e.g., 'chapter', 'doc-bibliography')

    Returns:
        DocBook element name or None if no mapping found
    """
    if not epub_type:
        return None

    # Handle multiple epub:type values (space-separated)
    types = epub_type.split() if ' ' in epub_type else [epub_type]

    for etype in types:
        # Check publisher configuration
        element = get_epub_type_mapping(etype)
        if element:
            # Record this usage for learning
            record_epub_type_usage(etype, element, matched_by="config")
            return element

    # Record unmapped epub:types for learning
    for etype in types:
        record_epub_type_usage(etype, matched_by="unmapped")

    return None


def _init_section_counters() -> Dict[str, int]:
    """
    Initialize section counters for 1-based hierarchical section IDs.

    Convention:
    - First sect1 in a chapter: ch####s0001
    - Second sect1 in a chapter: ch####s0002
    - Nested sections: ch####s0001s0001, ch####s0001s0002, ...
    """
    # We use 0 so the first increment produces 1 (1-based section numbering).
    return {'level_1': 0, 'level_2': 0, 'level_3': 0, 'level_4': 0, 'level_5': 0, 'level_6': 0}


def _section_tag_for_level(level: int) -> str:
    """Map a 1-based level to a DocBook section tag name."""
    level = max(1, min(int(level), 5))
    return f"sect{level}"


def extract_text(element, exclude_tags: set = None) -> str:
    """
    Extract text from a BeautifulSoup element without adding spaces between inline elements.

    Unlike get_text(' '), this preserves the original spacing by using empty separator,
    then normalizing whitespace (collapsing multiple spaces/newlines into single space).

    This fixes issues like:
        <span>M<small>ANUAL</small></span> -> "MANUAL" (not "M ANUAL")

    Also handles <br/> elements by treating them as spaces/line breaks.

    Block-level elements like chapternumber, chaptertitle, div, p, etc. get a space
    added after them to prevent concatenation like "1Introduction" -> "1 Introduction".

    Args:
        element: BeautifulSoup Tag or NavigableString
        exclude_tags: Optional set of tag names to skip (e.g., {'table', 'tr', 'td'} to
                     avoid including table content from malformed figcaption)

    Returns:
        Cleaned text with normalized whitespace
    """
    if element is None:
        return ''

    from bs4 import Tag, NavigableString

    # Block-level elements that should have a space/break after them
    # This prevents "1Introduction" when we have <chapternumber>1</chapternumber><chaptertitle>Introduction</chaptertitle>
    block_elements = {
        'div', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        'li', 'dt', 'dd', 'blockquote', 'pre',
        'chapternumber', 'chaptertitle', 'chapterid',  # RittDoc custom elements
        'title', 'subtitle', 'titleabbrev',  # DocBook title elements
    }

    # Custom text extraction that handles <br/> as line breaks
    def get_text_with_br(elem):
        """Recursively extract text, treating <br/> as newlines and block elements with breaks."""
        result = []
        for child in elem.children:
            if isinstance(child, NavigableString):
                result.append(str(child))
            elif isinstance(child, Tag):
                # Skip excluded tags (e.g., table elements in malformed figcaption)
                if exclude_tags and child.name in exclude_tags:
                    continue
                if child.name == 'br':
                    result.append('\n')
                else:
                    child_text = get_text_with_br(child)
                    result.append(child_text)
                    # Add a newline after block elements to ensure spacing
                    if child.name in block_elements:
                        result.append('\n')
                    else:
                        # Ensure inline sibling elements don't concatenate without spaces.
                        # e.g., <span class="HeadingNumber">1.1</span><span>When to...</span>
                        # should produce "1.1 When to..." not "1.1When to..."
                        # Only add space when the inline element produced non-empty text
                        # that doesn't already end with whitespace.
                        if child_text and not child_text[-1] in (' ', '\n', '\t'):
                            result.append(' ')
        return ''.join(result)

    raw_text = get_text_with_br(element)
    # Remove zero-width characters before normalization
    # These can cause compound words to split (e.g., "Stat Pearls" from "Stat\u200bPearls")
    raw_text = raw_text.replace('\u200b', '')   # Zero-width space
    raw_text = raw_text.replace('\u200c', '')   # Zero-width non-joiner
    raw_text = raw_text.replace('\u200d', '')   # Zero-width joiner
    raw_text = raw_text.replace('\ufeff', '')   # BOM / zero-width no-break space
    # Fix UTF-8 double-encoding artifacts (e.g., "Â " from NBSP double-encoding)
    raw_text = raw_text.replace('Â\u00a0', ' ')
    raw_text = re.sub(r'Â\s', ' ', raw_text)
    # Normalize whitespace: collapse multiple spaces/newlines into single space
    normalized = re.sub(r'\s+', ' ', raw_text)
    return normalized.strip()


def _normalize_inline_whitespace(text: str) -> str:
    if not text:
        return ""
    # Convert non-breaking spaces (NBSP) to regular spaces
    # NBSP (\xa0) in HTML is used for visual spacing but can be lost during XML
    # serialization, causing words to concatenate (e.g., "Kiegaldie(Corresponding" from
    # "Kiegaldie\xa0(Corresponding")
    normalized = text.replace('\u00a0', ' ')  # NBSP to regular space
    # Remove zero-width spaces and other invisible Unicode characters
    # These can cause compound words to split (e.g., "Stat Pearls" from "Stat\u200bPearls")
    normalized = normalized.replace('\u200b', '')   # Zero-width space
    normalized = normalized.replace('\u200c', '')  # Zero-width non-joiner
    normalized = normalized.replace('\u200d', '')  # Zero-width joiner
    normalized = normalized.replace('\ufeff', '')  # BOM / zero-width no-break space
    # Fix UTF-8 double-encoding artifacts (e.g., "Â " from NBSP double-encoding)
    normalized = normalized.replace('Â\u00a0', ' ')
    normalized = re.sub(r'Â\s', ' ', normalized)
    # Normalize newlines/tabs and collapse repeated spaces.
    normalized = re.sub(r'[\t\r\n]+', ' ', normalized)
    normalized = re.sub(r' {2,}', ' ', normalized)
    return normalized


def _is_inline_image(elem) -> bool:
    """Detect whether an HTML <img> element is an inline image.

    Inline images (e.g. inline math formulae) have ``vertical-align:middle``
    styling.  They should be rendered as ``<inlinemediaobject>`` instead of
    being wrapped in a formal ``<figure>``.

    Images inside equation containers are display equations and should remain
    as figures even if they carry vertical-align:middle.
    """
    style = (elem.get('style') or '').lower()
    if 'vertical-align' in style and 'middle' in style:
        # Check if inside an equation container — those stay as figures
        for parent in elem.parents:
            if not hasattr(parent, 'name') or parent.name is None:
                continue
            if parent.name == 'div':
                parent_class = parent.get('class', '')
                if isinstance(parent_class, list):
                    parent_class = ' '.join(parent_class)
                if 'equation' in parent_class.lower():
                    return False
        return True
    return False


def _is_css_table(elem) -> bool:
    """Detect if a div element is styled as a CSS table.

    Many EPUBs from scientific publishers use CSS-styled divs instead of
    HTML <table> elements (display:table, role="table", CSS classes).
    """
    if not isinstance(elem, Tag):
        return False

    style = elem.get('style', '')
    if style and 'display:table' in style.lower().replace(' ', ''):
        return True

    role = elem.get('role', '')
    if role.lower() == 'table':
        return True

    css_class = elem.get('class', '')
    if isinstance(css_class, list):
        css_class = ' '.join(css_class)
    if not css_class:
        return False

    class_list_lower = [c.lower() for c in css_class.split()]
    table_classes = {
        'table', 'data-table', 'datatable', 'table-wrap', 'tablewrap',
        'table-wrapper', 'tablewrapper', 'table-responsive',
        'tableresponsive', 'table-container', 'tablecontainer',
        'table-content', 'tablecontent',
    }
    if any(cls in table_classes for cls in class_list_lower):
        # Wrapper div around a real <table> — let normal processing handle it
        if elem.find('table') is not None:
            return False
        return _has_css_table_structure(elem)

    return False


def _has_css_table_structure(elem) -> bool:
    """Check if a div contains child elements that form a CSS table structure."""
    row_count = 0
    for child in elem.children:
        if not isinstance(child, Tag):
            continue

        child_style = child.get('style', '')
        if child_style and 'display:table-row' in child_style.lower().replace(' ', ''):
            row_count += 1
            continue

        child_role = child.get('role', '')
        if child_role.lower() == 'row':
            row_count += 1
            continue

        child_class = child.get('class', '')
        if isinstance(child_class, list):
            child_class = ' '.join(child_class)
        if child_class:
            child_class_list = [c.lower() for c in child_class.split()]
            if any(cls in {'row', 'tr', 'table-row', 'tablerow'} for cls in child_class_list):
                row_count += 1
                continue

        # Check for cell-like grandchildren
        cell_count = 0
        for gc in child.children:
            if not isinstance(gc, Tag):
                continue
            gc_style = gc.get('style', '')
            gc_role = gc.get('role', '')
            gc_class = gc.get('class', '')
            if isinstance(gc_class, list):
                gc_class = ' '.join(gc_class)
            gc_class_lower = gc_class.lower() if gc_class else ''

            if gc_style and 'display:table-cell' in gc_style.lower().replace(' ', ''):
                cell_count += 1
            elif gc_role.lower() in ('cell', 'gridcell', 'columnheader', 'rowheader'):
                cell_count += 1
            elif gc_class_lower:
                if any(cls in {'cell', 'td', 'th', 'table-cell', 'tablecell'} for cls in gc_class_lower.split()):
                    cell_count += 1

        if cell_count >= 2:
            row_count += 1

    return row_count >= 1


def _collect_css_row_cells(row_elem) -> list:
    """Collect cell elements from a CSS table row."""
    cells = []
    for child in row_elem.children:
        if not isinstance(child, Tag):
            continue

        child_style = child.get('style', '')
        child_role = child.get('role', '')
        child_class = child.get('class', '')
        if isinstance(child_class, list):
            child_class = ' '.join(child_class)

        is_cell = False
        if child_style and 'display:table-cell' in child_style.lower().replace(' ', ''):
            is_cell = True
        elif child_role.lower() in ('cell', 'gridcell', 'columnheader', 'rowheader'):
            is_cell = True
        elif child_class:
            class_list = [c.lower() for c in child_class.split()]
            if any(cls in {'cell', 'td', 'th', 'table-cell', 'tablecell'} for cls in class_list):
                is_cell = True

        if is_cell:
            cells.append(child)
        else:
            text = extract_text(child)
            if text and text.strip():
                cells.append(child)

    return cells


def _collect_css_table_rows(table_elem) -> list:
    """Collect row and cell data from a CSS-styled table div."""
    rows_data = []

    for child in table_elem.children:
        if not isinstance(child, Tag):
            continue

        is_row = False
        child_style = child.get('style', '')
        child_role = child.get('role', '')
        child_class = child.get('class', '')
        if isinstance(child_class, list):
            child_class = ' '.join(child_class)

        if child_style and 'display:table-row' in child_style.lower().replace(' ', ''):
            is_row = True
        elif child_role.lower() == 'row':
            is_row = True
        elif child_class:
            class_list = [c.lower() for c in child_class.split()]
            if any(cls in {'row', 'tr', 'table-row', 'tablerow'} for cls in class_list):
                is_row = True

        if is_row:
            cells = _collect_css_row_cells(child)
            if cells:
                rows_data.append(cells)
        else:
            nested_rows = _collect_css_table_rows(child)
            rows_data.extend(nested_rows)
            if not nested_rows:
                cells = _collect_css_row_cells(child)
                if len(cells) >= 2:
                    rows_data.append(cells)

    return rows_data


def _convert_css_table_to_docbook(elem: Tag, parent_elem, section_stack, doc_path,
                                   chapter_id, mapper, figure_counter, table_counter,
                                   section_counters, in_sidebar) -> None:
    """Convert a CSS-styled div table to DocBook CALS table format."""
    from lxml import etree

    current_parent = section_stack[-1][1] if section_stack else parent_elem

    # Validate table is allowed in parent
    if not _is_valid_element_for_parent('table', current_parent, elem.get('id')):
        text_content = extract_text(elem)
        if text_content and text_content.strip():
            para = validated_subelement(current_parent, 'para')
            para.text = f"[Table content: {text_content.strip()[:100]}...]"
        return

    table_counter['count'] += 1
    table = validated_subelement(current_parent, 'table')

    # Set ID
    current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
    elem_id = elem.get('id')
    if elem_id:
        table.set('id', sanitize_anchor_id(elem_id, chapter_id, 'table', current_sect1, doc_path))
    else:
        table.set('id', generate_element_id(chapter_id, 'table', current_sect1))

    # Preserve role/class
    existing_role = elem.get('role', '')
    css_class = elem.get('class')
    if existing_role and existing_role.lower() != 'table':
        table.set('role', existing_role)
    elif css_class:
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        table.set('role', css_class)

    # Look for caption/title
    title_elem = etree.SubElement(table, 'title')
    caption = elem.find('caption')
    if caption:
        extract_inline_content(caption, title_elem, doc_path, chapter_id, mapper,
                              figure_counter=figure_counter, section_counters=section_counters)
    else:
        adjacent_caption = _find_adjacent_caption_elem(elem, 'table')
        if adjacent_caption is None:
            title_elem.text = f"Table {table_counter['count']}"

    # Collect rows from CSS structure
    rows_data = _collect_css_table_rows(elem)

    if not rows_data:
        # Fallback: treat all text content as a single-cell table
        tgroup = etree.SubElement(table, 'tgroup', cols='1')
        _add_colspecs_to_tgroup(tgroup, 1)
        tbody = etree.SubElement(tgroup, 'tbody')
        row = etree.SubElement(tbody, 'row')
        entry = etree.SubElement(row, 'entry')
        text = extract_text(elem)
        para = etree.SubElement(entry, 'para')
        para.text = text.strip() if text else "[Table content]"
        return

    # Determine number of columns from the widest row
    num_cols = max(len(row_cells) for row_cells in rows_data) if rows_data else 1
    num_cols = max(num_cols, 1)

    tgroup = etree.SubElement(table, 'tgroup', cols=str(num_cols))
    _add_colspecs_to_tgroup(tgroup, num_cols)
    tbody = etree.SubElement(tgroup, 'tbody')

    for row_cells in rows_data:
        row = etree.SubElement(tbody, 'row')
        for cell_elem in row_cells:
            entry = etree.SubElement(row, 'entry')
            if isinstance(cell_elem, Tag):
                cell_text = extract_text(cell_elem)
                if cell_text and cell_text.strip():
                    para = etree.SubElement(entry, 'para')
                    extract_inline_content(cell_elem, para, doc_path, chapter_id, mapper,
                                          figure_counter=figure_counter,
                                          section_counters=section_counters)
                else:
                    para = etree.SubElement(entry, 'para')
            else:
                para = etree.SubElement(entry, 'para')
                para.text = str(cell_elem).strip() if cell_elem else ''

        # Pad short rows with empty entries
        while len(row) < num_cols:
            entry = etree.SubElement(row, 'entry')
            etree.SubElement(entry, 'para')


def fix_ulink_xml_extensions(root_elem) -> int:
    """Fix ulink URLs by removing .xml extension from internal links.

    Changes: ``ch0007.xml#ch0007s0005ta03`` → ``ch0007#ch0007s0005ta03``

    Required for proper link resolution in the R2 platform.
    """
    fixed_count = 0

    for ulink in root_elem.xpath('//ulink[@url]'):
        url = ulink.get('url')
        if '.xml#' in url and not url.startswith(('http://', 'https://', 'ftp://', '//')):
            if re.match(r'^[a-z]{2}\d{4}\.xml#', url):
                new_url = url.replace('.xml#', '#', 1)
                ulink.set('url', new_url)
                fixed_count += 1
                logger.debug(f"Fixed ulink URL: {url} → {new_url}")

    if fixed_count > 0:
        logger.info(f"Fixed {fixed_count} ulink URLs (removed .xml extension)")

    return fixed_count


def _extract_isbn_from_filename(epub_path: Optional[Path]) -> Optional[str]:
    """
    Extract an ISBN from the EPUB filename.
    Delegates to epub_metadata module implementation.
    """
    return _extract_isbn_from_filename_impl(epub_path)


def get_opf_xml_from_epub(epub_path: Path) -> Optional[etree.Element]:
    """
    Extract and parse the OPF XML file from an ePub.
    Delegates to epub_metadata module implementation.
    """
    return _get_opf_xml_impl(epub_path)


def get_metadata_value(book: epub.EpubBook, metadata_name: str, epub_path: Optional[Path] = None) -> Optional[str]:
    """
    Robustly extract metadata from ePub, supporting both EPUB 2 and EPUB 3 formats.
    Delegates to epub_metadata module implementation.
    """
    return _get_metadata_value_impl(book, metadata_name, epub_path)


def extract_metadata(book: epub.EpubBook, epub_path: Optional[Path] = None) -> Tuple[etree.Element, Dict[str, str]]:
    """
    Extract ePub metadata and convert to RittDoc <bookinfo> element.
    Supports both EPUB 2 and EPUB 3 metadata formats.

    Args:
        book: EpubBook instance
        epub_path: Optional path to ePub file for robust EPUB 3 metadata extraction

    Returns:
        Tuple of (bookinfo Element, metadata dict for tracking)
    """
    bookinfo = etree.Element('bookinfo')
    metadata_dict = {}

    # ISBN - prefer filename ISBN, fall back to metadata identifiers
    isbn_found = False
    isbn_value = _extract_isbn_from_filename(epub_path)
    fallback_identifier = None

    if isbn_value:
        logger.info(f"Using ISBN from filename: {isbn_value}")
    else:
        identifiers = book.get_metadata('DC', 'identifier')

        # Pass 1: Look for identifiers explicitly marked as ISBN
        for identifier_tuple in identifiers:
            identifier_value = identifier_tuple[0]
            identifier_attrs = identifier_tuple[1] if len(identifier_tuple) > 1 else {}

            # Check if this identifier has opf:scheme="ISBN" or similar
            scheme = identifier_attrs.get('opf:scheme', '').upper()
            if scheme == 'ISBN':
                isbn_clean = re.sub(r'^(urn:)?isbn:', '', identifier_value, flags=re.IGNORECASE)
                isbn_value = isbn_clean.strip()
                logger.info(f"Found ISBN via opf:scheme attribute: {isbn_value}")
                break

            # Check if identifier starts with "urn:isbn:" or "isbn:" prefix
            if re.match(r'^(urn:)?isbn:', identifier_value, flags=re.IGNORECASE):
                isbn_clean = re.sub(r'^(urn:)?isbn:', '', identifier_value, flags=re.IGNORECASE)
                if re.search(r'\d', isbn_clean):
                    isbn_value = isbn_clean.strip()
                    logger.info(f"Found ISBN via urn:isbn or isbn prefix: {isbn_value}")
                    break

            # Keep track of first numeric identifier as fallback
            if fallback_identifier is None and re.search(r'\d', identifier_value):
                fallback_identifier = identifier_value

        # Pass 2: Try direct OPF XML parsing for EPUB 3 format
        if isbn_value is None and epub_path:
            try:
                opf_root = get_opf_xml_from_epub(epub_path)
                if opf_root is not None:
                    namespaces = {
                        'opf': 'http://www.idpf.org/2007/opf',
                        'dc': 'http://purl.org/dc/elements/1.1/',
                    }

                    # Look for <dc:identifier opf:scheme="ISBN">...</dc:identifier>
                    isbn_elements = opf_root.xpath(
                        '//dc:identifier[@opf:scheme="ISBN"]',
                        namespaces=namespaces
                    )

                    if not isbn_elements:
                        # Try without namespace prefix
                        isbn_elements = opf_root.xpath('//identifier[@opf:scheme="ISBN"]')

                    if isbn_elements and isbn_elements[0].text:
                        isbn_value = isbn_elements[0].text.strip()
                        logger.info(f"Found ISBN via OPF XML parsing: {isbn_value}")
            except Exception as e:
                logger.debug(f"Could not parse OPF XML for ISBN: {e}")

    # Use found ISBN or fall back to first numeric identifier
    if isbn_value:
        isbn_elem = validated_subelement(bookinfo, 'isbn')
        isbn_elem.text = isbn_value
        metadata_dict['isbn'] = isbn_value
        isbn_found = True
    elif fallback_identifier:
        isbn_clean = re.sub(r'^(urn:)?isbn:', '', fallback_identifier, flags=re.IGNORECASE)
        isbn_elem = validated_subelement(bookinfo, 'isbn')
        isbn_elem.text = isbn_clean.strip()
        metadata_dict['isbn'] = isbn_clean.strip()
        isbn_found = True
        logger.warning(f"Using fallback identifier as ISBN: {isbn_clean.strip()}")

    if not isbn_found:
        isbn_elem = validated_subelement(bookinfo, 'isbn')
        isbn_elem.text = 'UNKNOWN'
        metadata_dict['isbn'] = 'UNKNOWN'

    # Title
    titles = book.get_metadata('DC', 'title')
    if titles:
        title_elem = validated_subelement(bookinfo, 'title')
        title_elem.text = titles[0][0]
        metadata_dict['title'] = titles[0][0]

    # Subtitle (try robust extraction for EPUB 2 and EPUB 3)
    subtitle = get_metadata_value(book, 'subtitle', epub_path)
    if subtitle:
        subtitle_elem = validated_subelement(bookinfo, 'subtitle')
        subtitle_elem.text = subtitle
        metadata_dict['subtitle'] = subtitle

    # Author(s)
    creators = book.get_metadata('DC', 'creator')
    authors = []
    if creators:
        authorgroup = validated_subelement(bookinfo, 'authorgroup')
        for creator_tuple in creators:
            author_elem = validated_subelement(authorgroup, 'author')
            personname = validated_subelement(author_elem, 'personname')

            # Parse name: "FirstName LastName" or "LastName, FirstName"
            name = creator_tuple[0].strip()
            authors.append(name)

            if ', ' in name:
                last, first = name.split(', ', 1)
            else:
                parts = name.rsplit(' ', 1)
                first = parts[0] if len(parts) > 1 else ''
                last = parts[-1] if len(parts) > 0 else name

            if first:
                firstname = validated_subelement(personname, 'firstname')
                firstname.text = first.strip()
            if last:
                surname = validated_subelement(personname, 'surname')
                surname.text = last.strip()

    metadata_dict['authors'] = authors

    # Publisher (try robust extraction for EPUB 2 and EPUB 3)
    publisher_name = get_metadata_value(book, 'publisher', epub_path)
    if publisher_name:
        publisher = validated_subelement(bookinfo, 'publisher')
        publishername = validated_subelement(publisher, 'publishername')
        publishername.text = publisher_name
        metadata_dict['publisher'] = publisher_name
        logger.info(f"Found publisher: {publisher_name}")
    else:
        logger.warning("Publisher metadata not found in ePub")

    # Publication date
    dates = book.get_metadata('DC', 'date')
    if dates:
        pubdate = validated_subelement(bookinfo, 'pubdate')
        date_str = dates[0][0]
        year_match = re.search(r'\d{4}', date_str)
        if year_match:
            pubdate.text = year_match.group(0)
        else:
            pubdate.text = date_str

    # Language
    languages = book.get_metadata('DC', 'language')
    if languages:
        language_elem = validated_subelement(bookinfo, 'language')
        language_elem.text = languages[0][0]
        metadata_dict['language'] = languages[0][0]

    # Copyright/Rights
    rights = book.get_metadata('DC', 'rights')
    if rights:
        copyright_elem = validated_subelement(bookinfo, 'copyright')
        rights_text = rights[0][0]
        year_match = re.search(r'\d{4}', rights_text)
        if year_match:
            year_elem = validated_subelement(copyright_elem, 'year')
            year_elem.text = year_match.group(0)
        holder_elem = validated_subelement(copyright_elem, 'holder')
        holder_elem.text = rights_text

    return bookinfo, metadata_dict


def extract_toc_structure(book: epub.EpubBook) -> List[Tuple[str, str, int]]:
    """
    Extract table of contents structure from ePub.

    Args:
        book: EpubBook instance

    Returns:
        List of (title, href, level) tuples representing TOC hierarchy
    """
    toc_entries = []

    def process_toc_item(item, level=0):
        """Recursively process TOC items"""
        if isinstance(item, tuple):
            # (Section, [children]) or Link
            if len(item) == 2 and isinstance(item[1], list):
                # Section with children
                section = item[0]
                children = item[1]

                if hasattr(section, 'title') and hasattr(section, 'href'):
                    toc_entries.append((section.title, section.href, level))

                for child in children:
                    process_toc_item(child, level + 1)
            else:
                # Simple link
                if hasattr(item, 'title') and hasattr(item, 'href'):
                    toc_entries.append((item.title, item.href, level))
        elif hasattr(item, 'title') and hasattr(item, 'href'):
            # Link object
            toc_entries.append((item.title, item.href, level))

    # Process TOC
    toc = book.toc
    if isinstance(toc, list):
        for item in toc:
            process_toc_item(item)

    return toc_entries


def _extract_guide_types(opf_root: etree._Element, opf_dir: str) -> Dict[str, str]:
    """
    Extract type information from OPF guide element.

    Args:
        opf_root: Parsed OPF XML root element
        opf_dir: Directory containing the OPF file

    Returns:
        Dict mapping file paths to their type ('front', 'body', 'back')
    """
    type_map = {}

    opf_ns = {'opf': 'http://www.idpf.org/2007/opf'}

    # Try with namespace first
    guide = opf_root.find('opf:guide', namespaces=opf_ns)
    if guide is None:
        guide = opf_root.find('guide')

    if guide is not None:
        references = guide.findall('opf:reference', namespaces=opf_ns)
        if not references:
            references = guide.findall('reference')

        for ref in references:
            ref_type = ref.get('type', '').lower()
            href = ref.get('href', '')

            # Remove anchor from href
            if '#' in href:
                href = href.split('#')[0]

            # Build full path
            if opf_dir and opf_dir != '.':
                full_path = f"{opf_dir}/{href}"
            else:
                full_path = href
            full_path = full_path.replace('\\', '/').lstrip('./')

            # Determine category
            if ref_type in FRONT_GUIDE_TYPES:
                type_map[full_path] = 'front'
            elif ref_type in BACK_GUIDE_TYPES:
                type_map[full_path] = 'back'
            elif ref_type == 'text':
                type_map[full_path] = 'body'

    return type_map


def _extract_epub3_nav_types(zf, opf_root: etree._Element, opf_dir: str) -> Dict[str, str]:
    """
    Extract type information from EPUB 3 nav document epub:type attributes.

    Args:
        zf: ZipFile object for the epub
        opf_root: Parsed OPF XML root element
        opf_dir: Directory containing the OPF file

    Returns:
        Dict mapping file paths to their type ('front', 'body', 'back')
    """
    type_map = {}

    opf_ns = {'opf': 'http://www.idpf.org/2007/opf'}

    # Find the nav document in manifest
    nav_items = opf_root.xpath(
        '//opf:item[@properties="nav"]/@href',
        namespaces=opf_ns
    )
    if not nav_items:
        nav_items = opf_root.xpath('//item[@properties="nav"]/@href')

    if not nav_items:
        return type_map

    nav_href = nav_items[0]
    if opf_dir and opf_dir != '.':
        nav_path = f"{opf_dir}/{nav_href}"
    else:
        nav_path = nav_href

    if nav_path not in zf.namelist():
        return type_map

    try:
        nav_content = zf.read(nav_path)
        # Parse as HTML since nav documents are XHTML
        from bs4 import BeautifulSoup
        soup = BeautifulSoup(nav_content, 'html.parser')

        # Find nav element with epub:type="toc"
        nav_toc = soup.find('nav', attrs={'epub:type': 'toc'})
        if nav_toc is None:
            nav_toc = soup.find('nav', attrs={'id': 'toc'})

        if nav_toc:
            # Look for landmarks nav for type info
            nav_landmarks = soup.find('nav', attrs={'epub:type': 'landmarks'})
            if nav_landmarks:
                for link in nav_landmarks.find_all('a'):
                    epub_type = link.get('epub:type', '')
                    href = link.get('href', '')

                    if '#' in href:
                        href = href.split('#')[0]

                    # Build full path relative to nav document
                    nav_dir = str(Path(nav_path).parent)
                    if nav_dir and nav_dir != '.':
                        full_path = f"{nav_dir}/{href}"
                    else:
                        full_path = href
                    full_path = full_path.replace('\\', '/').lstrip('./')

                    # Check epub:type
                    epub_types = epub_type.lower().split()
                    for et in epub_types:
                        if et in FRONT_EPUB_TYPES:
                            type_map[full_path] = 'front'
                            break
                        elif et in BACK_EPUB_TYPES:
                            type_map[full_path] = 'back'
                            break
                        elif et == 'bodymatter':
                            type_map[full_path] = 'body'
                            break

    except Exception as e:
        logger.debug(f"Could not parse EPUB 3 nav document: {e}")

    return type_map


def _extract_landmark_labels(zf, opf_root: etree._Element, opf_dir: str) -> Dict[str, str]:
    """
    Extract landmark text labels from EPUB 3 nav document.

    These labels (e.g., "Acknowledgments", "Index", "Dedication") can be used
    for more accurate section type detection when file paths don't contain
    identifying patterns.

    Args:
        zf: ZipFile object for the epub
        opf_root: Parsed OPF XML root element
        opf_dir: Directory containing the OPF file

    Returns:
        Dict mapping file paths to their landmark label text (lowercase)
    """
    label_map = {}

    opf_ns = {'opf': 'http://www.idpf.org/2007/opf'}

    # Find the nav document in manifest
    nav_items = opf_root.xpath(
        '//opf:item[@properties="nav"]/@href',
        namespaces=opf_ns
    )
    if not nav_items:
        nav_items = opf_root.xpath('//item[@properties="nav"]/@href')

    if not nav_items:
        return label_map

    nav_href = nav_items[0]
    if opf_dir and opf_dir != '.':
        nav_path = f"{opf_dir}/{nav_href}"
    else:
        nav_path = nav_href

    if nav_path not in zf.namelist():
        return label_map

    try:
        nav_content = zf.read(nav_path)
        from bs4 import BeautifulSoup
        soup = BeautifulSoup(nav_content, 'html.parser')

        # Look for landmarks nav
        nav_landmarks = soup.find('nav', attrs={'epub:type': 'landmarks'})
        if nav_landmarks:
            for link in nav_landmarks.find_all('a'):
                href = link.get('href', '')
                label_text = link.get_text(strip=True).lower()

                if '#' in href:
                    href = href.split('#')[0]

                # Build full path relative to nav document
                nav_dir = str(Path(nav_path).parent)
                if nav_dir and nav_dir != '.':
                    full_path = f"{nav_dir}/{href}"
                else:
                    full_path = href
                full_path = full_path.replace('\\', '/').lstrip('./')

                if label_text:
                    label_map[full_path] = label_text
                    logger.debug(f"Landmark label: {full_path} → '{label_text}'")

    except Exception as e:
        logger.debug(f"Could not extract landmark labels: {e}")

    return label_map


def extract_nested_toc_from_ncx(epub_path: Path, chapter_mapping: Dict[str, str]) -> Tuple[List[Dict], Dict[str, str], Dict[str, str]]:
    """
    Extract nested TOC structure directly from NCX file in ePub.

    This preserves the full hierarchical structure with IDs for generating
    a proper nested <toc> element in Book.XML.

    Args:
        epub_path: Path to the ePub file
        chapter_mapping: Dict mapping ePub file paths to output chapter filenames
                        (e.g., {'OEBPS/ch01.xhtml': 'ch0001.xml'})

    Returns:
        Tuple of (nested TOC entry list, file type map).
        TOC entry structure:
        [
            {
                'id': 'ch01',
                'title': 'Chapter 1',
                'href': 'ch0001.xml#ch01',
                'type': 'body',  # 'front', 'body', 'back', or None
                'children': [
                    {'id': 'sec01', 'title': 'Section 1', 'href': 'ch0001.xml#sec01', 'type': None, 'children': []},
                    ...
                ]
            },
            ...
        ]
    """
    toc_entries = []
    file_type_map = {}  # Maps file paths to types ('front', 'body', 'back')
    landmark_labels = {}  # Maps file paths to landmark label text

    try:
        with zipfile.ZipFile(epub_path, 'r') as zf:
            # Find the NCX file
            ncx_path = None

            # First, try to find NCX via OPF manifest
            container_xml = zf.read('META-INF/container.xml')
            container_root = etree.fromstring(container_xml)

            namespaces = {'container': 'urn:oasis:names:tc:opendocument:xmlns:container'}
            rootfiles = container_root.xpath(
                '//container:rootfile[@media-type="application/oebps-package+xml"]',
                namespaces=namespaces
            )
            if not rootfiles:
                rootfiles = container_root.xpath('//rootfile[@media-type="application/oebps-package+xml"]')

            opf_dir = ''
            if rootfiles:
                opf_path = rootfiles[0].get('full-path')
                if opf_path:
                    opf_dir = str(Path(opf_path).parent)
                    if opf_dir == '.':
                        opf_dir = ''

                    # Parse OPF to find NCX and extract type information
                    opf_content = zf.read(opf_path)
                    opf_root = etree.fromstring(opf_content)

                    # Extract type information from OPF guide and EPUB 3 nav
                    file_type_map = _extract_guide_types(opf_root, opf_dir)
                    epub3_types = _extract_epub3_nav_types(zf, opf_root, opf_dir)
                    # EPUB 3 types take precedence over OPF guide
                    file_type_map.update(epub3_types)

                    # Extract landmark labels for better type detection
                    landmark_labels = _extract_landmark_labels(zf, opf_root, opf_dir)
                    if landmark_labels:
                        logger.info(f"Extracted landmark labels for {len(landmark_labels)} files")

                    if file_type_map:
                        logger.info(f"Extracted type information for {len(file_type_map)} files from EPUB metadata")

                    # Look for NCX in manifest
                    opf_ns = {'opf': 'http://www.idpf.org/2007/opf'}
                    ncx_items = opf_root.xpath(
                        '//opf:item[@media-type="application/x-dtbncx+xml"]/@href',
                        namespaces=opf_ns
                    )
                    if not ncx_items:
                        # Try without namespace
                        ncx_items = opf_root.xpath('//item[@media-type="application/x-dtbncx+xml"]/@href')

                    if ncx_items:
                        ncx_href = ncx_items[0]
                        if opf_dir:
                            ncx_path = f"{opf_dir}/{ncx_href}"
                        else:
                            ncx_path = ncx_href

            # Fallback: look for toc.ncx in common locations
            if not ncx_path:
                for candidate in ['toc.ncx', 'OEBPS/toc.ncx', 'OPS/toc.ncx', 'EPUB/toc.ncx']:
                    if candidate in zf.namelist():
                        ncx_path = candidate
                        break

            if not ncx_path:
                logger.warning("No NCX file found in ePub")
                return toc_entries, file_type_map, landmark_labels

            # Parse NCX
            ncx_content = zf.read(ncx_path)
            ncx_root = etree.fromstring(ncx_content)

            # NCX namespace
            ncx_ns = {'ncx': 'http://www.daisy.org/z3986/2005/ncx/'}

            def resolve_href(src: str) -> Tuple[str, str]:
                """Convert ePub href to output chapter reference.

                Returns:
                    Tuple of (resolved_href, original_full_path)
                """
                if not src:
                    return '', ''

                # Split file and anchor
                if '#' in src:
                    file_part, anchor = src.split('#', 1)
                else:
                    file_part = src
                    anchor = ''

                # Resolve the file path relative to NCX location
                ncx_dir = str(Path(ncx_path).parent)
                if ncx_dir and ncx_dir != '.':
                    full_path = f"{ncx_dir}/{file_part}"
                else:
                    full_path = file_part

                # Normalize path
                full_path = full_path.replace('\\', '/')
                original_path = full_path.lstrip('./')

                # Try to find the chapter mapping
                chapter_file = None

                # Try exact match first
                if full_path in chapter_mapping:
                    chapter_file = chapter_mapping[full_path]
                else:
                    # Try basename match
                    basename = Path(full_path).name
                    for epub_path_key, ch_file in chapter_mapping.items():
                        if Path(epub_path_key).name == basename:
                            chapter_file = ch_file
                            break

                    # Try without leading paths
                    if not chapter_file:
                        normalized = full_path.lstrip('./')
                        if normalized in chapter_mapping:
                            chapter_file = chapter_mapping[normalized]

                if chapter_file:
                    if anchor:
                        return f"{chapter_file}#{anchor}", original_path
                    return chapter_file, original_path

                # Fallback: just use the anchor if we can't resolve the file
                if anchor:
                    return f"#{anchor}", original_path
                return src, original_path

            def get_entry_type(original_path: str) -> Optional[str]:
                """Look up the type for a file path from the type map."""
                return _lookup_file_type(original_path, file_type_map)

            def parse_navpoint(navpoint: etree._Element) -> Dict:
                """Recursively parse a navPoint element."""
                entry = {
                    'id': navpoint.get('id', ''),
                    'title': '',
                    'href': '',
                    'epub_path': '',  # Store resolved EPUB path for later re-resolution with mapper
                    'anchor': '',     # Store fragment anchor separately
                    'type': None,
                    'children': []
                }

                # Get title from navLabel/text
                nav_label = navpoint.find('ncx:navLabel', namespaces=ncx_ns)
                if nav_label is None:
                    nav_label = navpoint.find('navLabel')

                if nav_label is not None:
                    text_elem = nav_label.find('ncx:text', namespaces=ncx_ns)
                    if text_elem is None:
                        text_elem = nav_label.find('text')
                    if text_elem is not None and text_elem.text:
                        entry['title'] = text_elem.text.strip()

                # Get href from content and determine type
                content = navpoint.find('ncx:content', namespaces=ncx_ns)
                if content is None:
                    content = navpoint.find('content')

                if content is not None:
                    src = content.get('src', '')
                    resolved_href, original_path = resolve_href(src)
                    entry['href'] = resolved_href
                    entry['epub_path'] = original_path  # Store resolved EPUB path
                    # Extract anchor from src
                    if '#' in src:
                        entry['anchor'] = src.split('#', 1)[1]
                    entry['type'] = get_entry_type(original_path)

                # Process child navPoints
                child_navpoints = navpoint.findall('ncx:navPoint', namespaces=ncx_ns)
                if not child_navpoints:
                    child_navpoints = navpoint.findall('navPoint')

                for child in child_navpoints:
                    child_entry = parse_navpoint(child)
                    if child_entry['title']:  # Only add entries with titles
                        entry['children'].append(child_entry)

                return entry

            # Find navMap
            nav_map = ncx_root.find('ncx:navMap', namespaces=ncx_ns)
            if nav_map is None:
                nav_map = ncx_root.find('navMap')

            if nav_map is not None:
                # Process top-level navPoints
                top_navpoints = nav_map.findall('ncx:navPoint', namespaces=ncx_ns)
                if not top_navpoints:
                    top_navpoints = nav_map.findall('navPoint')

                for navpoint in top_navpoints:
                    entry = parse_navpoint(navpoint)
                    if entry['title']:  # Only add entries with titles
                        toc_entries.append(entry)

            logger.info(f"Extracted {len(toc_entries)} top-level TOC entries from NCX")

    except Exception as e:
        logger.error(f"Failed to extract NCX TOC: {e}", exc_info=True)

    return toc_entries, file_type_map, landmark_labels


def save_toc_structure_json(toc_entries: List[Dict], output_path: Path) -> None:
    """
    Save TOC structure to JSON file for use by packaging stage.

    Args:
        toc_entries: Nested TOC structure from extract_nested_toc_from_ncx
        output_path: Path to save JSON file
    """
    import json

    try:
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(toc_entries, f, indent=2, ensure_ascii=False)
        logger.info(f"Saved TOC structure to {output_path}")
    except Exception as e:
        logger.error(f"Failed to save TOC structure JSON: {e}")


def filter_toc_entries_without_chapters(toc_entries: List[Dict], chapter_ids: Set[str]) -> Tuple[List[Dict], int]:
    """
    Filter out TOC entries that don't map to actual converted chapters.

    EPUB navigation (NCX/nav.xhtml) may contain entries for files that are NOT
    in the spine and therefore not converted to chapter files. These orphan
    entries would create broken linkends in the TOC.

    This function removes entries where:
    - The href doesn't resolve to a valid chapter file (e.g., ch0001.xml)
    - The href is an unresolved EPUB path (e.g., OEBPS/Text/eula.xhtml)
    - The href is empty or just an anchor without a file reference

    Args:
        toc_entries: List of TOC entry dicts from extract_nested_toc_from_ncx
        chapter_ids: Set of valid chapter IDs (e.g., {'ch0001', 'pr0001', 'in0001'})

    Returns:
        Tuple of (filtered_entries, removed_count)
    """
    # Pattern for valid resolved chapter file references
    # Matches: ch0001.xml, pr0001.xml#anchor, ch0001, pr0001s0001, etc.
    valid_chapter_pattern = re.compile(r'^([a-z]{2}\d{4})(?:\.xml)?(?:#.*)?$')

    removed_count = 0

    def is_valid_entry(entry: Dict) -> bool:
        """Check if an entry has a valid href pointing to a converted chapter."""
        href = entry.get('href', '')

        if not href:
            return False

        # Check if href starts with # only (no file reference)
        if href.startswith('#'):
            return False

        # Extract the chapter ID from the href
        # Handle formats: ch0001.xml, ch0001.xml#anchor, ch0001, ch0001s0001
        match = valid_chapter_pattern.match(href)
        if match:
            chapter_id = match.group(1)
            return chapter_id in chapter_ids

        # Also check if href is just a chapter ID (after normalization)
        # Pattern: ch0001, ch0001s0001, ch0001s0001fg0001, etc.
        id_match = re.match(r'^([a-z]{2}\d{4})', href)
        if id_match:
            chapter_id = id_match.group(1)
            return chapter_id in chapter_ids

        # Unresolved EPUB path (contains .xhtml, .html, or path separators)
        if '.xhtml' in href.lower() or '.html' in href.lower() or '/' in href:
            return False

        return False

    def filter_entries(entries: List[Dict]) -> List[Dict]:
        """Recursively filter entries and their children."""
        nonlocal removed_count
        filtered = []

        for entry in entries:
            # First, filter children recursively
            if entry.get('children'):
                entry['children'] = filter_entries(entry['children'])

            # Check if this entry itself is valid
            if is_valid_entry(entry):
                filtered.append(entry)
            elif entry.get('children'):
                # Entry is invalid but has valid children - keep children only
                # This handles cases where a parent entry (like a Part title page)
                # doesn't have a valid href but its chapter children do
                logger.debug(f"TOC entry '{entry.get('title', '')}' has invalid href "
                           f"'{entry.get('href', '')}' but has valid children - promoting children")
                filtered.extend(entry['children'])
                removed_count += 1
            else:
                # Entry is invalid and has no valid children - remove it
                logger.info(f"Filtering out TOC entry '{entry.get('title', '')}' - "
                          f"href '{entry.get('href', '')}' doesn't map to a converted chapter")
                removed_count += 1

        return filtered

    filtered_entries = filter_entries(toc_entries)
    return filtered_entries, removed_count


def _lookup_file_type(original_path: str, file_type_map: Dict[str, str]) -> Optional[str]:
    """Look up the front/back/body type for a file path from the type map."""
    if not file_type_map or not original_path:
        return None

    normalized = original_path.replace('\\', '/').lstrip('./')
    if normalized in file_type_map:
        return file_type_map[normalized]

    # Try basename match
    basename = Path(normalized).name
    for path, entry_type in file_type_map.items():
        if Path(path).name == basename:
            return entry_type

    return None


def normalize_toc_structure_hrefs(toc_entries: List[Dict]) -> None:
    """
    Normalize TOC hrefs to ID-only references (chapter or section IDs).

    Important: Generated-style IDs from NCX are validated against the actual
    registered valid IDs to prevent phantom anchors from being created for
    non-existent content.
    """
    # Get all valid XML IDs from the authority for validation
    authority = get_authority()
    valid_xml_ids = authority.registry.valid_ids if authority else set()

    def normalize_entry(entry: Dict) -> None:
        href = entry.get('href', '')
        if href:
            chapter_id = ''
            anchor = ''

            if href.startswith('#'):
                anchor = href[1:]
            else:
                file_part, _, fragment = href.partition('#')
                anchor = fragment
                if file_part:
                    base = Path(file_part).name
                    chapter_id = re.sub(r'\.(xml|xhtml|html)$', '', base)

            if anchor:
                # Match all element type prefixes: ch (chapter), pr (preface), ap (appendix), dd (dedication), etc.
                generated_id_pattern = re.match(r'^[a-z]{2}\d{4}(s\d{4})+$', anchor) or re.match(r'^[a-z]{2}\d{4}$', anchor)

                if generated_id_pattern:
                    # CRITICAL: Validate that generated-style IDs from NCX actually exist
                    # in the content. NCX files may contain stale/invalid references to
                    # IDs that don't exist, which would create phantom anchors.
                    if anchor in valid_xml_ids:
                        entry['href'] = anchor
                    else:
                        # Generated-style ID doesn't exist - fall back to chapter ID
                        # This prevents phantom anchors from being created
                        logger.debug(f"NCX anchor '{anchor}' looks like generated ID but doesn't exist - using chapter ID '{chapter_id or anchor}'")
                        if chapter_id:
                            entry['href'] = chapter_id
                        else:
                            # Extract chapter prefix from the anchor itself (e.g., ch0055 from ch0055s0002)
                            chapter_match = re.match(r'^([a-z]{2}\d{4})', anchor)
                            if chapter_match:
                                entry['href'] = chapter_match.group(1)
                            else:
                                entry['href'] = anchor
                else:
                    resolved = resolve_linkend_id(anchor, chapter_id) if chapter_id else ''
                    if resolved:
                        entry['href'] = resolved
                    elif chapter_id:
                        entry['href'] = chapter_id
                    else:
                        entry['href'] = anchor
            elif chapter_id:
                entry['href'] = chapter_id

        for child in entry.get('children', []):
            normalize_entry(child)

    for entry in toc_entries:
        normalize_entry(entry)


def re_resolve_toc_hrefs_with_mapper(toc_entries: List[Dict], mapper) -> int:
    """
    Re-resolve TOC hrefs using the correct chapter mappings from the mapper.

    This function is called after Phase 2.5 completes, when the mapper has been
    updated with correct chapter IDs (with proper prefixes like pt#### for parts,
    ch#### for chapters, etc.).

    The initial TOC extraction in Phase 2 uses preliminary mappings from Phase 1
    which assign sequential ch#### IDs to ALL spine items regardless of type.
    This function corrects those hrefs using the mapper's updated mappings.

    Args:
        toc_entries: List of TOC entry dicts (with 'epub_path' and 'anchor' fields)
        mapper: ReferenceMapper instance with correct chapter mappings

    Returns:
        Number of hrefs that were updated
    """
    updated_count = 0

    def update_entry_href(entry: Dict) -> None:
        """Update a single entry's href using the mapper."""
        nonlocal updated_count

        epub_path = entry.get('epub_path', '')
        anchor = entry.get('anchor', '')

        if epub_path:
            # Try to get correct chapter ID from mapper
            correct_chapter_id = mapper.get_chapter_id(epub_path)

            # Also try basename match
            if not correct_chapter_id:
                basename = Path(epub_path).name
                correct_chapter_id = mapper.get_chapter_id(basename)

            if correct_chapter_id:
                # Build new href with correct chapter ID
                if anchor:
                    new_href = f"{correct_chapter_id}.xml#{anchor}"
                else:
                    new_href = f"{correct_chapter_id}.xml"

                old_href = entry.get('href', '')
                if old_href != new_href:
                    logger.debug(f"Re-resolved TOC href: '{old_href}' -> '{new_href}' (epub_path={epub_path})")
                    entry['href'] = new_href
                    updated_count += 1

        # Process children
        for child in entry.get('children', []):
            update_entry_href(child)

    for entry in toc_entries:
        update_entry_href(entry)

    if updated_count > 0:
        logger.info(f"Re-resolved {updated_count} TOC hrefs with corrected chapter IDs")

    return updated_count


def build_toc_depth_map(toc_entries: List[Dict], chapter_id_prefix: str = "") -> Dict[str, int]:
    """
    Build a lookup map from TOC entries to their depth in the hierarchy.

    This map is used during section creation to determine whether a heading
    should become sect1, sect2, sect3, etc. based on its position in the
    original TOC hierarchy rather than just the HTML heading level.

    Args:
        toc_entries: Nested TOC structure from extract_nested_toc_from_ncx
        chapter_id_prefix: Optional prefix for chapter IDs (used for prefixed anchor lookups)

    Returns:
        Dict mapping various ID formats to their TOC depth (1-based):
        - 'anchor_id' -> depth
        - 'ch0001-anchor_id' -> depth (prefixed format used in converted XML)
        - 'ch0001.xml#anchor_id' -> depth (full href format)
    """
    depth_map = {}

    def process_entry(entry: Dict, depth: int, parent_file: str = "") -> None:
        """Recursively process TOC entries and record their depths."""
        href = entry.get('href', '')
        entry_id = entry.get('id', '')

        # Parse the href to extract file and anchor
        file_part = ''
        anchor = ''
        if href:
            if '#' in href:
                file_part, anchor = href.split('#', 1)
            else:
                file_part = href
                anchor = ''

        # Use entry_id as anchor if no anchor in href
        if not anchor and entry_id:
            anchor = entry_id

        # Record depth for various lookup formats
        if anchor:
            # Plain anchor ID
            depth_map[anchor] = depth

            # Full href format (e.g., "ch0001.xml#section1")
            if file_part:
                depth_map[f"{file_part}#{anchor}"] = depth

            # File-only format (e.g., "ch0001.xml") for resolving the TOC depth of the
            # enclosing content unit (chapter/preface/appendix). This is critical for
            # normalizing section depths when the NCX has a top-level "book" entry or
            # other wrappers above parts/chapters.
            if file_part:
                # Prefer to keep the shallowest depth if multiple entries reference the same file.
                existing = depth_map.get(file_part)
                if existing is None or depth < existing:
                    depth_map[file_part] = depth

            # Extract chapter ID from file_part (e.g., "ch0001" from "ch0001.xml")
            if file_part:
                chapter_id = file_part.replace('.xml', '')
                # Also store a file-less chapter id key (e.g., "ch0001") to make it easy
                # to look up the TOC depth for the current file during conversion.
                existing = depth_map.get(chapter_id)
                if existing is None or depth < existing:
                    depth_map[chapter_id] = depth
                # Store with sanitized anchor format for consistent lookup
                sanitized_anchor = sanitize_id(anchor)
                # NOTE: The first sect1 in a chapter is ch####s0001 (1-based).
                # We store this format for lookups that include the sect1 prefix.
                depth_map[f"{chapter_id}s{1:04d}{sanitized_anchor}"] = depth
                # Back-compat: older outputs may have used s0000; keep as fallback key.
                depth_map[f"{chapter_id}s{0:04d}{sanitized_anchor}"] = depth

        # Also record by entry_id if different from anchor
        if entry_id and entry_id != anchor:
            depth_map[entry_id] = depth

        # Process children at next depth level
        for child in entry.get('children', []):
            process_entry(child, depth + 1, file_part or parent_file)

    # Process all top-level entries at depth 1
    for entry in toc_entries:
        process_entry(entry, 1)

    logger.debug(f"Built TOC depth map with {len(depth_map)} entries")
    return depth_map


def build_part_ownership_map(toc_entries: List[Dict], mapper: ReferenceMapper) -> Dict[str, str]:
    """
    Build a map showing which chapters belong to which parts based on TOC hierarchy.

    The EPUB TOC hierarchy reveals part/chapter relationships through nested children.
    A TOC entry with 'Part' in its title that has children is considered a part marker,
    and all its children (chapters) belong to that part.

    Ownership is mapped by canonical chapter IDs (e.g., pt0001, ch0002), resolved
    through the ReferenceMapper to ensure consistent nesting even when filenames
    or href formats vary.

    Args:
        toc_entries: Nested TOC structure from extract_nested_toc_from_ncx
        mapper: ReferenceMapper with canonical EPUB path → chapter ID mappings

    Returns:
        Dict mapping child chapter IDs to their parent part IDs.
    """
    ownership_map = {}

    def is_part_entry(entry: Dict) -> bool:
        """Check if a TOC entry looks like a Part based on its title."""
        title = entry.get('title', '').strip().lower()
        # Check for common part title patterns
        part_patterns = [
            title.startswith('part '),
            title.startswith('part:'),
            title.startswith('volume '),
            title.startswith('section ') and entry.get('children', []),  # Section with children
            ' part ' in title and title.split(' part ')[0].strip().isdigit() is False,
        ]
        return any(part_patterns) and len(entry.get('children', [])) > 0

    def _resolve_entry_chapter_id(entry: Dict) -> Optional[str]:
        """
        Resolve a TOC entry to a canonical chapter ID via the mapper.
        Accepts epub_path, href file-part, or already-normalized IDs.
        """
        candidates: List[str] = []

        epub_path = (entry.get('epub_path') or '').strip()
        if epub_path:
            candidates.append(epub_path)

        href = (entry.get('href') or '').strip()
        if href:
            if href.startswith('#'):
                # Pure anchor - no file reference to resolve
                pass
            elif '#' in href:
                candidates.append(href.split('#', 1)[0])
            else:
                candidates.append(href)

        for candidate in candidates:
            resolved = mapper.get_chapter_id(candidate)
            if resolved:
                return resolved

        # If href already looks like a chapter ID, accept as-is
        if href and re.match(r'^[a-z]{2}\d{4}$', href):
            return href

        # Handle hrefs that are already in chapter-id.xml form
        if href and href.lower().endswith('.xml'):
            base = Path(href).stem
            if re.match(r'^[a-z]{2}\d{4}$', base):
                return base

        return None

    def process_entry(entry: Dict, parent_part_file: Optional[str] = None) -> None:
        """Recursively process entries and track part ownership."""
        entry_id = _resolve_entry_chapter_id(entry)
        children = entry.get('children', [])

        if is_part_entry(entry):
            # This entry is a part - its children belong to this part
            part_id = entry_id
            for child in children:
                process_entry(child, part_id or parent_part_file)
        else:
            # Regular entry - record ownership if under a part
            if parent_part_file and entry_id:
                ownership_map[entry_id] = parent_part_file
                logger.debug(f"Part ownership: {entry_id} → {parent_part_file}")

            # Process any children (sections within chapters, etc.)
            for child in children:
                process_entry(child, parent_part_file)

    # Process all top-level TOC entries
    for entry in toc_entries:
        process_entry(entry)

    if ownership_map:
        logger.info(f"Built part ownership map: {len(ownership_map)} chapters belong to parts")
    else:
        logger.debug("No part/chapter ownership detected in TOC hierarchy")

    return ownership_map


def extract_cover_image(book: epub.EpubBook) -> Optional[Tuple[str, bytes]]:
    """
    Extract cover image from ePub.
    Supports all common image formats including PNG, JPEG, GIF, etc.

    Args:
        book: EpubBook instance

    Returns:
        Tuple of (filename, image_data) or None if no cover found
    """
    # Method 1: Check for cover image in metadata
    cover_id = None
    for meta_item in book.get_metadata('OPF', 'cover'):
        if meta_item and len(meta_item) > 1:
            cover_id = meta_item[1].get('content')
            break

    if cover_id:
        cover_item = book.get_item_with_id(cover_id)
        if cover_item and cover_item.get_content():
            logger.info(f"Found cover image via metadata: {cover_item.get_name()}")
            return (cover_item.get_name(), cover_item.get_content())

    # Method 2: Look for items with 'cover' in the name or properties
    # Support all image formats
    for item in book.get_items():
        item_name = item.get_name().lower()
        item_id = item.get_id().lower() if hasattr(item, 'get_id') else ''

        # Check if it's an image and has 'cover' in name or id
        if (hasattr(item, 'media_type') and item.media_type and
            'image' in item.media_type.lower()):
            if 'cover' in item_name or 'cover' in item_id:
                content = item.get_content()
                if content and len(content) > 0:
                    logger.info(f"Found cover image via name/id: {item.get_name()}")
                    return (item.get_name(), content)

    # Method 3: Check manifest properties for cover-image
    for item in book.get_items():
        if hasattr(item, 'properties') and item.properties:
            # Handle both string and list properties
            properties_str = item.properties if isinstance(item.properties, str) else ' '.join(str(p) for p in item.properties)
            if 'cover-image' in properties_str.lower():
                content = item.get_content()
                if content and len(content) > 0:
                    logger.info(f"Found cover image via properties: {item.get_name()}")
                    return (item.get_name(), content)

    # Method 4: Check for first image in the spine
    for item_id, _ in book.spine[:1]:  # Check first spine item only
        item = book.get_item_with_id(item_id)
        if item:
            # Parse XHTML to find first image
            try:
                soup = BeautifulSoup(item.get_content(), 'lxml')
                img = soup.find('img')
                if img and img.get('src'):
                    img_src = img.get('src')
                    # Normalize path
                    img_src = img_src.lstrip('./')
                    # Try to find this image in the book items
                    for img_item in book.get_items():
                        img_item_name = img_item.get_name()
                        if (img_item_name.endswith(img_src) or
                            img_src in img_item_name or
                            Path(img_item_name).name == Path(img_src).name):
                            if (hasattr(img_item, 'media_type') and img_item.media_type and
                                'image' in img_item.media_type.lower()):
                                content = img_item.get_content()
                                if content and len(content) > 0:
                                    logger.info(f"Found cover image via first spine image: {img_item.get_name()}")
                                    return (img_item.get_name(), content)
            except Exception as e:
                logger.debug(f"Error parsing first spine item for cover image: {e}")

    logger.warning("No cover image found in ePub")
    return None


def extract_images_with_mapping(book: epub.EpubBook,
                                output_dir: Path,
                                mapper: ReferenceMapper) -> Optional[str]:
    """
    Extract images from ePub and register in reference mapper.
    Images are extracted at their exact original dimensions from the EPUB.

    Duplicate Detection:
    - Uses SHA-256 hash to detect duplicate images
    - If an identical image already exists, skips extraction but maintains mapping
    - This saves disk space while preserving correct Figure ID mappings

    Args:
        book: EpubBook instance
        output_dir: Directory to save extracted images
        mapper: ReferenceMapper instance

    Returns:
        Filename of cover image if found, None otherwise
    """
    multimedia_dir = output_dir / 'MultiMedia'
    multimedia_dir.mkdir(parents=True, exist_ok=True)

    image_idx = 0
    cover_filename = None

    # Hash index for duplicate detection: SHA-256 hash → intermediate filename
    # This allows us to skip extracting duplicate images while maintaining correct mappings
    hash_to_intermediate: Dict[str, str] = {}
    duplicates_skipped = 0

    # First, try to extract cover image
    cover_data = extract_cover_image(book)
    if cover_data:
        cover_path, cover_content = cover_data
        extension = Path(cover_path).suffix.lower()

        # Flatten transparent cover images onto white background
        # (prevents black/invisible rendering in e-readers)
        if extension in ('.png', '.gif', '.webp', '.tiff', '.tif', '.bmp'):
            try:
                img_check = Image.open(BytesIO(cover_content))
                if img_check.mode in ('RGBA', 'LA', 'PA') or (img_check.mode == 'P' and 'transparency' in img_check.info):
                    img_rgba = img_check.convert('RGBA')
                    white_bg = Image.new('RGBA', img_rgba.size, (255, 255, 255, 255))
                    composited = Image.alpha_composite(white_bg, img_rgba)
                    composited_rgb = composited.convert('RGB')
                    buf = BytesIO()
                    composited_rgb.save(buf, format='PNG')
                    cover_content = buf.getvalue()
                    extension = '.png'
                    logger.info(f"Flattened transparent cover image onto white background: {cover_path}")
            except Exception as e:
                logger.warning(f"Could not check/flatten transparency for cover image: {e}")

        cover_filename = f"cover{extension}"
        cover_file_path = multimedia_dir / cover_filename

        # Compute hash for cover image
        cover_hash = hashlib.sha256(cover_content).hexdigest()

        # Cover is always extracted (first image)
        with open(cover_file_path, 'wb') as f:
            f.write(cover_content)

        # Register hash for future duplicate detection
        hash_to_intermediate[cover_hash] = cover_filename

        # Get image dimensions from the extracted file
        try:
            img = Image.open(cover_file_path)
            width, height = img.size
        except (IOError, OSError, ValueError) as e:
            logger.warning(f"Could not read cover image dimensions from {cover_file_path}: {e}")
            width, height = None, None

        # Register cover in mapper
        mapper.add_resource(
            original_path=cover_path,
            intermediate_name=cover_filename,
            resource_type="cover",
            is_vector=False,
            is_raster=True,
            width=width,
            height=height,
            file_size=len(cover_content)
        )
        logger.info(f"Extracted cover image: {cover_path} → {cover_filename}")

    # Now extract all other images
    for item in book.get_items():
        # Check for image items (using both ITEM_IMAGE and media_type)
        # Also explicitly check for SVG files which may have media_type "application/svg+xml"
        is_image = (item.get_type() == ebooklib.ITEM_IMAGE or
                   (hasattr(item, 'media_type') and item.media_type and
                    ('image' in item.media_type.lower() or
                     'svg' in item.media_type.lower())))

        if is_image:
            original_path = item.get_name()

            # Log media type for debugging
            media_type = getattr(item, 'media_type', 'unknown')
            logger.debug(f"Detected image: {original_path} (media_type: {media_type})")

            # Skip if this is the cover image we already extracted
            # Use fuzzy matching to handle path variations (e.g., OEBPS/images/x.jpg vs images/x.jpg)
            if cover_data:
                cover_path = cover_data[0]
                # Check exact match first
                if original_path == cover_path:
                    logger.debug(f"Skipping cover image (exact match): {original_path}")
                    continue
                # Check if filenames match (handles different directory prefixes)
                if Path(original_path).name == Path(cover_path).name:
                    logger.debug(f"Skipping cover image (filename match): {original_path}")
                    continue
                # Check if one path ends with or contains the other
                if original_path.endswith(cover_path) or cover_path.endswith(original_path):
                    logger.debug(f"Skipping cover image (suffix match): {original_path}")
                    continue

            extension = Path(original_path).suffix.lower()

            # Handle SVG conversion
            if extension == '.svg':
                if HAS_CAIROSVG:
                    try:
                        svg_content = item.get_content()

                        # Check if this is an equation image by path pattern
                        # Equation SVGs often have these patterns in path/filename
                        path_lower = original_path.lower()
                        is_equation = any(pattern in path_lower for pattern in [
                            'equation', 'eq/', 'eq_', '/eq', 'math/', 'math_',
                            'formula', 'inline-equation', 'display-equation'
                        ])

                        if is_equation:
                            # Equation SVGs need white background to prevent
                            # black/invisible text (common with math notation)
                            png_data = cairosvg.svg2png(bytestring=svg_content, background_color="white")
                        else:
                            # Regular SVGs keep transparent background
                            png_data = cairosvg.svg2png(bytestring=svg_content)

                        # Check for duplicate using hash of converted PNG data
                        content_hash = hashlib.sha256(png_data).hexdigest()

                        if content_hash in hash_to_intermediate:
                            # Duplicate found - reuse existing file but register new path mapping
                            existing_filename = hash_to_intermediate[content_hash]
                            existing_path = multimedia_dir / existing_filename

                            # Get dimensions from existing file
                            try:
                                img = Image.open(existing_path)
                                width, height = img.size
                            except (IOError, OSError, ValueError) as e:
                                logger.warning(f"Could not read image dimensions from {existing_path}: {e}")
                                width, height = None, None

                            # Register in mapper with the EXISTING intermediate filename
                            # This is critical - the original_path maps to the same file
                            mapper.add_resource(
                                original_path=original_path,
                                intermediate_name=existing_filename,
                                resource_type="image",
                                is_vector=True,
                                is_raster=False,
                                width=width,
                                height=height,
                                file_size=len(png_data)
                            )
                            duplicates_skipped += 1
                            logger.info(f"Skipped duplicate SVG: {original_path} → reusing {existing_filename}")
                            continue

                        # Not a duplicate - extract normally
                        temp_filename = f"img_{image_idx:04d}.png"
                        temp_path = multimedia_dir / temp_filename

                        with open(temp_path, 'wb') as f:
                            f.write(png_data)

                        # Register hash for future duplicate detection
                        hash_to_intermediate[content_hash] = temp_filename

                        # Get image dimensions
                        img = Image.open(temp_path)
                        width, height = img.size

                        # Register in mapper
                        mapper.add_resource(
                            original_path=original_path,
                            intermediate_name=temp_filename,
                            resource_type="image",
                            is_vector=True,
                            is_raster=False,
                            width=width,
                            height=height,
                            file_size=len(png_data)
                        )

                        image_idx += 1
                        logger.info(f"Converted SVG to PNG: {original_path} → {temp_filename}")
                    except Exception as e:
                        logger.error(f"Failed to convert SVG {original_path}: {e}")
                        continue
                else:
                    # cairosvg is a required dependency - this is an ERROR, not a warning
                    logger.error(
                        f"SVG image LOST - cairosvg not available: {original_path}. "
                        f"Install system dependency: libcairo2-dev (Ubuntu) or cairo (macOS)"
                    )
                    # Track this as a lost resource for the conversion report
                    mapper.add_skipped_resource(
                        original_path=original_path,
                        reason="cairosvg not available (missing system Cairo library)",
                        resource_type="svg"
                    )
                    continue
            else:
                # Regular image (JPEG, PNG, GIF, etc.)
                content = item.get_content()

                # Skip zero-byte images
                if len(content) == 0:
                    logger.warning(f"Skipping zero-byte image: {original_path}")
                    continue

                # Check for duplicate using hash of image content
                content_hash = hashlib.sha256(content).hexdigest()

                if content_hash in hash_to_intermediate:
                    # Duplicate found - reuse existing file but register new path mapping
                    existing_filename = hash_to_intermediate[content_hash]
                    existing_path = multimedia_dir / existing_filename

                    # Get dimensions from existing file
                    try:
                        img = Image.open(existing_path)
                        width, height = img.size
                        is_vector = False
                        is_raster = True
                    except (IOError, OSError, ValueError) as e:
                        logger.warning(f"Could not read image dimensions from {existing_path}: {e}")
                        width, height = None, None
                        is_vector = False
                        is_raster = True

                    # Register in mapper with the EXISTING intermediate filename
                    # This is critical - the original_path maps to the same file
                    mapper.add_resource(
                        original_path=original_path,
                        intermediate_name=existing_filename,
                        resource_type="image",
                        is_vector=is_vector,
                        is_raster=is_raster,
                        width=width,
                        height=height,
                        file_size=len(content)
                    )
                    duplicates_skipped += 1
                    logger.info(f"Skipped duplicate image: {original_path} → reusing {existing_filename}")
                    continue

                # Not a duplicate - extract normally
                temp_filename = f"img_{image_idx:04d}{extension}"
                temp_path = multimedia_dir / temp_filename

                with open(temp_path, 'wb') as f:
                    f.write(content)

                # Register hash for future duplicate detection
                hash_to_intermediate[content_hash] = temp_filename

                # Get image info
                try:
                    img = Image.open(temp_path)
                    width, height = img.size
                    is_vector = False
                    is_raster = True
                except Exception as e:
                    logger.warning(f"Could not read image dimensions for {original_path}: {e}")
                    width, height = None, None
                    is_vector = False
                    is_raster = True

                # Register in mapper
                mapper.add_resource(
                    original_path=original_path,
                    intermediate_name=temp_filename,
                    resource_type="image",
                    is_vector=is_vector,
                    is_raster=is_raster,
                    width=width,
                    height=height,
                    file_size=len(content)
                )

                image_idx += 1

    if duplicates_skipped > 0:
        logger.info(f"Extracted {image_idx} unique images ({duplicates_skipped} duplicates skipped)")
    else:
        logger.info(f"Extracted {image_idx} images with reference mapping")
    return cover_filename


def resolve_image_path(img_src: str, doc_path: str, mapper: ReferenceMapper) -> Optional[Tuple[str, str]]:
    """
    Resolve image path using reference mapper.

    Args:
        img_src: Image source from HTML (relative or absolute)
        doc_path: Path to current XHTML document
        mapper: ReferenceMapper instance

    Returns:
        Tuple of (intermediate_filename, normalized_original_path) or None if not found
    """
    # Try direct lookup
    intermediate = mapper.get_intermediate_name(img_src)
    if intermediate:
        return (intermediate, img_src)

    # Try resolving relative path with proper normalization
    if not img_src.startswith('/'):
        doc_dir = str(Path(doc_path).parent)
        if doc_dir == '.':
            resolved = img_src
        else:
            # Construct path and normalize it (handles ../ properly)
            resolved = posixpath.normpath(f"{doc_dir}/{img_src}")

        intermediate = mapper.get_intermediate_name(resolved)
        if intermediate:
            return (intermediate, resolved)

    # Try without leading slash
    if img_src.startswith('/'):
        stripped = img_src[1:]
        intermediate = mapper.get_intermediate_name(stripped)
        if intermediate:
            return (intermediate, stripped)

    # Build comprehensive list of path variations to try
    variations = [
        img_src,
        img_src.lstrip('./'),
    ]

    # Add normalized variations with common EPUB prefixes
    img_cleaned = img_src.lstrip('./')
    variations.extend([
        f"OEBPS/{img_cleaned}",
        f"OPS/{img_cleaned}",
        f"Text/{img_cleaned}",
    ])

    # If we have a relative path with ../, also try normalized versions
    if not img_src.startswith('/'):
        doc_dir = str(Path(doc_path).parent)
        if doc_dir != '.':
            # Normalized full path
            full_normalized = posixpath.normpath(f"{doc_dir}/{img_src}")
            variations.append(full_normalized)

            # Try removing common prefixes from normalized path
            for prefix in ['OEBPS/', 'OPS/', 'Text/']:
                if full_normalized.startswith(prefix):
                    variations.append(full_normalized[len(prefix):])

    # Remove duplicates while preserving order
    seen = set()
    unique_variations = []
    for v in variations:
        if v not in seen:
            seen.add(v)
            unique_variations.append(v)

    for variant in unique_variations:
        intermediate = mapper.get_intermediate_name(variant)
        if intermediate:
            return (intermediate, variant)

    logger.warning(f"Could not resolve image path: {img_src} in {doc_path}")
    return None


def _detect_element_type(item, doc_path: str, book: epub.EpubBook,
                         landmark_labels: Optional[Dict[str, str]] = None) -> str:
    """
    Detect the appropriate DocBook element type for an EPUB item.

    Determines whether content should be:
    - chapter (default)
    - preface (foreword, preface, introduction, cover, titlepage, etc.)
    - appendix (also for epilogue, afterword)
    - dedication
    - acknowledgments
    - glossary
    - bibliography
    - index
    - colophon
    - contributors
    - about

    Detection priority:
    1. Landmark labels from EPUB navigation (most accurate)
    2. File path patterns (fallback)

    Args:
        item: EPUB item from spine
        doc_path: Path to the XHTML file
        book: EpubBook instance
        landmark_labels: Optional dict mapping file paths to landmark label text

    Returns:
        DocBook element type string (matches R2 naming convention prefixes)
    """
    doc_path_lower = doc_path.lower()
    doc_basename = doc_path_lower.split('/')[-1].replace('.xhtml', '').replace('.html', '')

    # Check item properties (EPUB 3)
    item_props = getattr(item, 'properties', []) or []
    if isinstance(item_props, str):
        item_props = item_props.split()

    # PRIORITY 1: Check landmark labels (most accurate for generic file names)
    if landmark_labels:
        # Try exact path match first
        label = landmark_labels.get(doc_path, '')

        # If not found, try with OPS/, OEBPS/, or other common prefixes
        if not label:
            for prefix in ['OPS/', 'OEBPS/', 'EPUB/', 'content/', '']:
                prefixed_path = f"{prefix}{doc_path}" if prefix else doc_path
                label = landmark_labels.get(prefixed_path, '')
                if label:
                    break

        # If still not found, try matching by filename only
        if not label:
            doc_filename = doc_path.split('/')[-1]
            for landmark_path, landmark_label in landmark_labels.items():
                if landmark_path.endswith(f'/{doc_filename}') or landmark_path == doc_filename:
                    label = landmark_label
                    break

        if label:
            label_lower = label.lower()
            # Map landmark labels to element types
            label_type_map = {
                'acknowledgments': 'acknowledgments',
                'acknowledgements': 'acknowledgments',
                'index': 'index',
                'glossary': 'glossary',
                'bibliography': 'bibliography',
                'references': 'bibliography',
                'dedication': 'dedication',
                'appendix': 'appendix',
                'colophon': 'colophon',
                'contributors': 'contributors',
                'about the author': 'about',
                'about the authors': 'about',
                # Part detection (R2 prefix: pt####)
                'part': 'part',
                # Front matter types that should use preface prefix (pr####)
                'toc': 'preface',
                'table of contents': 'preface',
                'contents': 'preface',
                'copyright': 'preface',
                'copyright page': 'preface',
                'preface': 'preface',
                'foreword': 'preface',
                'introduction': 'preface',
                'cover': 'preface',
                'title page': 'preface',
                'titlepage': 'preface',
                'half title': 'preface',
                'halftitle': 'preface',
                'frontmatter': 'preface',
                'front matter': 'preface',
                'epigraph': 'preface',
            }
            for label_pattern, elem_type in label_type_map.items():
                if label_pattern in label_lower:
                    logger.debug(f"Detected {elem_type} from landmark label: {doc_path} → '{label}'")
                    return elem_type

    # PRIORITY 2: File path patterns (fallback for EPUBs without landmarks)

    # Dedication patterns (R2 prefix: dd####)
    if any(p in doc_path_lower for p in ['dedication', 'dedic']):
        return 'dedication'

    # Acknowledgments patterns (R2 prefix: ak####) - check BEFORE preface
    acknowledgments_patterns = [
        'acknowledgment', 'acknowledgement', 'ack_', 'ack-',
        'acknowledgments', 'acknowledgements'
    ]
    if any(p in doc_path_lower for p in acknowledgments_patterns):
        return 'acknowledgments'

    # Bibliography patterns (R2 prefix: bi####)
    if any(p in doc_path_lower for p in ['bibliography', 'biblio', 'references', 'works-cited', 'workscited', 'ref_']):
        return 'bibliography'

    # Glossary patterns (R2 prefix: gl####)
    if any(p in doc_path_lower for p in ['glossary', 'glossar', 'gloss_']):
        return 'glossary'

    # Index patterns (R2 prefix: in####)
    # Match index-related files but avoid index.xhtml which is often the main entry point
    index_patterns = ['index', 'indx', 'idx', 'ind_', 'ind-']
    if any(p in doc_path_lower for p in index_patterns):
        # Exclude common entry point files
        if not any(exclude in doc_path_lower for exclude in ['index.xhtml', 'index.html', 'index.htm']):
            return 'index'

    # Appendix patterns (R2 prefix: ap####)
    # Including epilogue and afterword which are similar in structure
    appendix_patterns = [
        'appendix', 'appendice', 'appndx', 'app_', 'app-',
        'epilogue', 'epilog', 'afterword'
    ]
    if any(p in doc_path_lower for p in appendix_patterns):
        return 'appendix'

    # Colophon patterns (R2 prefix: co####)
    if any(p in doc_path_lower for p in ['colophon', 'coloph']):
        return 'colophon'

    # Contributors patterns (R2 prefix: co####)
    contributors_patterns = ['contributors', 'contrib_', 'contrib-', 'contributor-list']
    if any(p in doc_path_lower for p in contributors_patterns):
        return 'contributors'

    # About patterns (R2 prefix: ab####)
    about_patterns = [
        'about_the_author', 'aboutauthor', 'about-author', 'author-bio', 'author_bio',
        'about_', 'about-'
    ]
    if any(p in doc_path_lower for p in about_patterns):
        return 'about'

    # Part patterns (R2 prefix: pt####) - Part landing pages
    # Springer uses Part_1.xhtml, Part_2.xhtml etc. for part divider pages
    # These contain PartNumber/PartTitle and should become <part> elements, not chapters
    if re.search(r'part[_\-]?\d', doc_path_lower):
        return 'part'

    # Preface patterns (R2 prefix: pr####) - frontmatter types not covered above
    preface_patterns = [
        # Cover and title pages
        'cover', 'title-page', 'titlepage', 'halftitle', 'half-title', 'halftitlepage',
        # Copyright/imprint
        'copyright', 'copy_', 'imprint',
        # Preface-like content
        'preface', 'pref_', 'foreword', 'fwd_',
        # Introduction/prologue
        'introduction', 'intro_', 'intro-', 'prologue', 'prolog',
        # Epigraph
        'epigraph',
        # Other frontmatter
        'frontmatter', 'front_matter', 'front-matter',
        'other-credits', 'other_credits', 'credits'
    ]
    if any(p in doc_path_lower for p in preface_patterns):
        return 'preface'

    # PRIORITY 3: Check epub:type in XHTML content itself
    # This catches cases where file path is generic (e.g., f03.xhtml) but content has epub:type
    try:
        xhtml_content = item.get_content()
        from bs4 import BeautifulSoup
        soup = BeautifulSoup(xhtml_content, 'lxml')

        # Check body and direct section children for epub:type
        body = soup.find('body')
        if body:
            # Check body's epub:type
            body_epub_types = (body.get('epub:type', '') or '').lower().split()
            # Check first section/article child's epub:type
            section = body.find(['section', 'article'], recursive=False)
            if section:
                section_epub_types = (section.get('epub:type', '') or '').lower().split()
                body_epub_types.extend(section_epub_types)

            # Map epub:type to element_type
            # Priority 1: Check publisher configuration
            for epub_type in body_epub_types:
                mapped_element = get_docbook_element_for_epub_type(epub_type)
                if mapped_element:
                    # Map DocBook elements to our internal type names
                    element_type_map = {
                        'preface': 'preface', 'chapter': 'chapter', 'appendix': 'appendix',
                        'bibliography': 'bibliography', 'glossary': 'glossary', 'index': 'index',
                        'dedication': 'dedication', 'colophon': 'colophon', 'part': 'part',
                        'acknowledgments': 'acknowledgments', 'legalnotice': 'preface',
                        'titlepage': 'preface', 'toc': 'preface',
                    }
                    internal_type = element_type_map.get(mapped_element, 'chapter')
                    # CSS class override: Springer uses class="Chapter" as the authoritative
                    # structural indicator, even when epub:type says "introduction"
                    if internal_type == 'preface' and section:
                        section_classes = section.get('class', [])
                        if isinstance(section_classes, str):
                            section_classes = section_classes.split()
                        if 'Chapter' in section_classes:
                            logger.debug(f"Overriding {internal_type} to chapter due to class='Chapter': {doc_path}")
                            return 'chapter'
                    logger.debug(f"Detected {internal_type} from publisher config epub:type='{epub_type}': {doc_path}")
                    return internal_type

            # Priority 2: Fall back to hardcoded mappings
            epub_type_map = {
                'dedication': 'dedication',
                'doc-dedication': 'dedication',
                'copyright-page': 'preface',  # Will get role from _extract_matter_role
                'doc-copyright-page': 'preface',
                'acknowledgments': 'acknowledgments',
                'doc-acknowledgments': 'acknowledgments',
                'acknowledgements': 'acknowledgments',
                'glossary': 'glossary',
                'doc-glossary': 'glossary',
                'bibliography': 'bibliography',
                'doc-bibliography': 'bibliography',
                'index': 'index',
                'doc-index': 'index',
                'colophon': 'colophon',
                'doc-colophon': 'colophon',
                'appendix': 'appendix',
                'doc-appendix': 'appendix',
                'foreword': 'preface',
                'doc-foreword': 'preface',
                'preface': 'preface',
                'doc-preface': 'preface',
                'introduction': 'preface',
                'doc-introduction': 'preface',
                'toc': 'preface',
                'doc-toc': 'preface',
                # Part detection (R2 prefix: pt####)
                'part': 'part',
                'doc-part': 'part',
                'bodymatter': 'part',  # Some EPUBs use this for part content
            }

            for epub_type in body_epub_types:
                if epub_type in epub_type_map:
                    resolved_type = epub_type_map[epub_type]
                    # CSS class override: Springer uses class="Chapter" as the authoritative
                    # structural indicator, even when epub:type says "introduction"
                    if resolved_type == 'preface' and section:
                        section_classes = section.get('class', [])
                        if isinstance(section_classes, str):
                            section_classes = section_classes.split()
                        if 'Chapter' in section_classes:
                            logger.debug(f"Overriding {resolved_type} to chapter due to class='Chapter': {doc_path}")
                            return 'chapter'
                    logger.debug(f"Detected {resolved_type} from XHTML epub:type='{epub_type}': {doc_path}")
                    return resolved_type
    except Exception as e:
        logger.debug(f"Could not check XHTML epub:type for {doc_path}: {e}")

    # Check filename patterns that suggest chapter content
    # If none of the above matched, default to chapter
    return 'chapter'


def _wrap_part_content_in_partintro(part_elem: etree.Element, part_id: str, logger) -> None:
    """
    Wrap non-header content in a <part> element inside <partintro>.

    DTD for part: (beginpage?, partinfo?, title, subtitle?, titleabbrev?, partintro?,
                   (chapter|appendix|refentry|reference|article|preface|
                    glossary|bibliography|index|toc|lot)+)

    Content like anchors, figures, paras, sect1 must be inside partintro, not directly in part.
    Chapters and other valid top-level children are left as-is (they'll be added during nesting).

    Args:
        part_elem: The <part> element to process
        part_id: Part ID for logging
        logger: Logger instance
    """
    # Elements that can stay directly in <part> (before partintro)
    header_tags = {'partinfo', 'title', 'subtitle', 'titleabbrev'}
    # Elements that are valid children of <part> (after partintro)
    valid_child_tags = {'chapter', 'appendix', 'refentry', 'reference', 'article',
                        'preface', 'glossary', 'bibliography', 'index', 'toc', 'lot'}

    # Collect elements that need to move to partintro
    elements_to_move = []
    for child in list(part_elem):
        if not isinstance(child.tag, str):
            continue  # Skip comments, processing instructions
        local_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
        if local_tag not in header_tags and local_tag not in valid_child_tags and local_tag != 'partintro':
            elements_to_move.append(child)

    if not elements_to_move:
        return  # Nothing to wrap

    # Check if partintro already exists
    partintro = part_elem.find('partintro')
    if partintro is None:
        partintro = etree.Element('partintro')
        # Find insertion point (after header elements, before valid children)
        insert_idx = 0
        for i, child in enumerate(part_elem):
            if not isinstance(child.tag, str):
                continue
            local_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if local_tag in header_tags:
                insert_idx = i + 1
            else:
                break
        part_elem.insert(insert_idx, partintro)
        logger.debug(f"Created <partintro> in part {part_id}")

    # Move elements to partintro
    for elem in elements_to_move:
        part_elem.remove(elem)
        validated_append(partintro, elem)
        local_tag = elem.tag.split('}')[-1] if '}' in elem.tag else elem.tag
        logger.debug(f"Moved <{local_tag}> to <partintro> in part {part_id}")

    logger.info(f"Wrapped {len(elements_to_move)} element(s) in <partintro> for part {part_id}")


def _strip_part_toc_listing(part_elem: etree.Element, part_id: str, logger) -> None:
    """
    Remove TOC-like chapter/section listings from <partintro> in a <part> element.

    EPUB part pages often contain a mini-TOC listing the chapters in that part.
    When converted to DocBook, this becomes an <itemizedlist> or <orderedlist>
    inside <partintro>. Since the actual chapters are referenced via entity
    references (&chXXXX;) as direct children of <part>, these listings are
    redundant and should be stripped.

    A list is considered a TOC listing if it contains link elements (ulink, link,
    xref) in its listitems — a strong signal of navigational content vs. actual
    introductory content.
    """
    partintro = part_elem.find('partintro')
    if partintro is None:
        return

    lists_to_remove = []
    for child in partintro:
        if not isinstance(child.tag, str):
            continue
        local_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
        if local_tag not in ('itemizedlist', 'orderedlist'):
            continue

        # Check if this list looks like a TOC (contains link elements)
        has_links = False
        for descendant in child.iter():
            if not isinstance(descendant.tag, str):
                continue
            dtag = descendant.tag.split('}')[-1] if '}' in descendant.tag else descendant.tag
            if dtag in ('ulink', 'link', 'xref'):
                has_links = True
                break

        if has_links:
            lists_to_remove.append(child)

    # Also check inside sect1 wrappers (conversion may wrap content in sect1)
    for sect in list(partintro):
        if not isinstance(sect.tag, str):
            continue
        sect_tag = sect.tag.split('}')[-1] if '}' in sect.tag else sect.tag
        if sect_tag not in ('sect1', 'sect2', 'section', 'simplesect'):
            continue
        for child in list(sect):
            if not isinstance(child.tag, str):
                continue
            local_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if local_tag not in ('itemizedlist', 'orderedlist'):
                continue
            has_links = False
            for descendant in child.iter():
                if not isinstance(descendant.tag, str):
                    continue
                dtag = descendant.tag.split('}')[-1] if '}' in descendant.tag else descendant.tag
                if dtag in ('ulink', 'link', 'xref'):
                    has_links = True
                    break
            if has_links:
                lists_to_remove.append(child)

    for lst in lists_to_remove:
        parent = lst.getparent()
        if parent is not None:
            parent.remove(lst)
            logger.debug(f"Removed TOC listing from partintro in part {part_id}")

    if lists_to_remove:
        logger.info(f"Stripped {len(lists_to_remove)} TOC listing(s) from partintro in part {part_id}")

    # If partintro is now empty (no meaningful content), remove it entirely
    has_content = False
    if partintro.text and partintro.text.strip():
        has_content = True
    else:
        for child in partintro:
            if isinstance(child.tag, str):
                has_content = True
                break
            # Comments/PIs don't count as content
    if not has_content:
        part_elem.remove(partintro)
        logger.debug(f"Removed empty partintro from part {part_id}")


def convert_xhtml_to_chapter(xhtml_content: bytes,
                             doc_path: str,
                             chapter_id: str,
                             mapper: ReferenceMapper,
                             toc_depth_map: Optional[Dict[str, int]] = None,
                             element_type: str = 'chapter',
                             matter_category: Optional[str] = None) -> etree.Element:
    """
    Convert a single XHTML file to a DocBook chapter or other front/back matter element.

    Args:
        xhtml_content: Raw XHTML content
        doc_path: Path to XHTML in ePub (for reference resolution)
        chapter_id: Chapter ID (e.g., "ch0001")
        mapper: ReferenceMapper instance
        toc_depth_map: Optional dict mapping element IDs to their TOC depth.
                       Used to determine section hierarchy (sect1, sect2, etc.)
                       based on TOC structure instead of HTML heading levels.
        element_type: The DocBook element type to create. Supported values:
                      'chapter' (default), 'preface', 'appendix', 'dedication',
                      'glossary', 'bibliography', 'index', 'colophon'
        matter_category: Optional 'front' or 'back' classification from EPUB metadata.

    Returns:
        lxml Element representing the specified element type
    """
    def _title_has_text(title_el: Optional[etree.Element]) -> bool:
        if title_el is None:
            return False
        return bool("".join(title_el.itertext()).strip())

    def _extract_matter_role(soup_root: BeautifulSoup) -> Optional[str]:
        tokens: List[str] = []
        seen = set()

        def add_tokens(value: str) -> None:
            for token in (value or "").split():
                if token in FRONT_EPUB_TYPES or token in BACK_EPUB_TYPES:
                    if token not in seen:
                        tokens.append(token)
                        seen.add(token)

        body_elem = soup_root.find('body')
        if body_elem is not None:
            add_tokens(body_elem.get('epub:type', ''))
            for child in body_elem.find_all(['section', 'article', 'nav', 'header', 'main'], recursive=False):
                add_tokens(child.get('epub:type', ''))

        if tokens:
            return " ".join(tokens)

        if matter_category == 'front':
            return "frontmatter"
        if matter_category == 'back':
            return "backmatter"

        return None

    def _merge_role(existing: str, new_role: str) -> str:
        if not existing:
            return new_role
        existing_tokens = existing.split()
        for token in new_role.split():
            if token not in existing_tokens:
                existing_tokens.append(token)
        return " ".join(existing_tokens)

    # Debug: Log raw content preview to help diagnose title extraction issues
    content_preview = xhtml_content[:500].decode('utf-8', errors='replace') if isinstance(xhtml_content, bytes) else str(xhtml_content)[:500]
    logger.debug(f"XHTML content preview for {doc_path}: {content_preview}")

    # Use lxml-xml parser for proper XML/XHTML parsing (avoids XMLParsedAsHTMLWarning)
    # Fall back to lxml (HTML parser) if xml parser fails (some EPUBs have malformed XHTML)
    try:
        soup = BeautifulSoup(xhtml_content, 'lxml-xml')
    except Exception:
        logger.debug(f"XML parser failed for {doc_path}, falling back to HTML parser")
        soup = BeautifulSoup(xhtml_content, 'lxml')
    body = soup.find('body') or soup

    # Check if this is a cover page - remove cover images from body to avoid duplication
    # since the cover image is already extracted and inserted separately as a figure
    cover_section = body.find(attrs={'epub:type': 'cover'}) or body.find(class_='cover')
    if cover_section:
        # Remove all img elements from the cover section to prevent duplication
        for img in cover_section.find_all('img'):
            logger.debug(f"Removing duplicate cover image from body: {img.get('src', 'unknown')}")
            img.decompose()
        logger.debug(f"Detected cover page, removed cover images: {chapter_id}")
    else:
        # Also check for individual cover images (img with epub:type="cover" or role="doc-cover")
        # These might be inside a figure or section without cover class
        for img in body.find_all('img'):
            img_epub_type = img.get('epub:type', '')
            img_role = img.get('role', '')
            if 'cover' in img_epub_type.lower() or 'cover' in img_role.lower():
                # Remove the parent figure if it only contains this cover image
                parent_figure = img.find_parent('figure')
                if parent_figure:
                    # Check if figure only contains this image (plus optional span/caption)
                    other_images = [i for i in parent_figure.find_all('img') if i != img]
                    if not other_images:
                        logger.debug(f"Removing duplicate cover figure from body: {img.get('src', 'unknown')}")
                        parent_figure.decompose()
                        continue
                # If no parent figure or figure has other content, just remove the img
                logger.debug(f"Removing duplicate cover image from body: {img.get('src', 'unknown')}")
                img.decompose()

    # Create element based on element_type (chapter, preface, appendix, etc.)
    # Validate element_type
    valid_types = {'chapter', 'preface', 'appendix', 'dedication', 'glossary',
                   'bibliography', 'index', 'colophon', 'acknowledgments',
                   'contributors', 'about', 'part'}
    if element_type not in valid_types:
        logger.warning(f"Unknown element_type '{element_type}', defaulting to 'chapter'")
        element_type = 'chapter'

    # Map element types to valid DTD element names
    # Some R2 types don't have corresponding DTD elements, so we use preface with role
    element_tag_map = {
        'chapter': 'chapter',
        'preface': 'preface',
        'appendix': 'appendix',
        'dedication': 'dedication',
        'glossary': 'glossary',
        'bibliography': 'bibliography',
        'index': 'index',
        'colophon': 'colophon',
        'part': 'part',  # DocBook part element for epub:type="part" sections
        # These types use chapter element - downstream RISChunker expects chapter/sect1
        'acknowledgments': 'chapter',  # Downstream expects sect1.{ISBN}.ak0001.xml
        'contributors': 'preface',  # Contributors use preface element with pr#### prefix
        'about': 'preface',          # About pages are prefaces with pr#### prefix
    }
    xml_tag = element_tag_map.get(element_type, 'chapter')

    # Create the root element for this content unit
    chapter = etree.Element(xml_tag, id=chapter_id)

    # Add role attribute for semantic types that need identification
    # - acknowledgments: maps to chapter element, role identifies the semantic type
    # - contributors/about: map to preface element, role identifies the semantic type
    if element_type in ('acknowledgments', 'contributors', 'about'):
        chapter.set('role', element_type)

    logger.debug(f"Creating <{xml_tag}> element (type={element_type}) for {doc_path}")

    # Extract chapter title from <head><title> element
    # NOTE: Per our EPUB conventions, <h1> in <body> is the chapter heading and
    # will overwrite this title during conversion. Sections start at <h2>.
    head = soup.find('head')
    head_title = head.find('title') if head else None
    chapter_title_text = extract_text(head_title) if head_title else ''

    if chapter_title_text:
        title_elem = validated_subelement(chapter, 'title')
        title_elem.text = chapter_title_text
        logger.debug(f"Title extraction for {doc_path}: Using <head><title>: {chapter_title_text}")
    else:
        # Fallback: use empty title element (body headings will become sections)
        validated_subelement(chapter, 'title')
        logger.debug(f"Title extraction for {doc_path}: No <head><title> found, using empty title")

    # Convert body content (pass TOC depth map for section hierarchy)
    convert_body_to_docbook(body, chapter, doc_path, chapter_id, mapper, toc_depth_map)

    # SPECIAL HANDLING FOR PARTS: Wrap non-header content in partintro
    # DTD: part can contain (partinfo?, title, subtitle?, titleabbrev?, partintro?, (chapter|appendix|...)+)
    # Any content like anchors, figures, paras must be inside partintro, not directly in part
    if xml_tag == 'part':
        _wrap_part_content_in_partintro(chapter, chapter_id, logger)
        _strip_part_toc_listing(chapter, chapter_id, logger)

    # If chapter title is empty, use the title from the first section
    title_elem = chapter.find('title')
    if title_elem is not None and not _title_has_text(title_elem):
        # Look for first sect1 and use its title
        first_sect1 = chapter.find('sect1')
        if first_sect1 is not None:
            sect1_title = first_sect1.find('title')
            if sect1_title is not None and sect1_title.text and sect1_title.text.strip():
                title_elem.text = sect1_title.text.strip()
                logger.debug(f"Title extraction for {doc_path}: Using first sect1 title: {title_elem.text}")

    # If still no title and this is a frontmatter/backmatter type, use element_type as title
    if title_elem is not None and not _title_has_text(title_elem):
        frontmatter_backmatter_titles = {
            # Frontmatter
            'cover': 'Cover',
            'titlepage': 'Title Page',
            'title-page': 'Title Page',
            'copyright': 'Copyright',
            'copyright-page': 'Copyright',
            'dedication': 'Dedication',
            'epigraph': 'Epigraph',
            'foreword': 'Foreword',
            'preface': 'Preface',
            'acknowledgments': 'Acknowledgments',
            'acknowledgements': 'Acknowledgements',
            'introduction': 'Introduction',
            'prologue': 'Prologue',
            'halftitle': 'Half Title',
            'half-title': 'Half Title',
            # Backmatter
            'appendix': 'Appendix',
            'glossary': 'Glossary',
            'bibliography': 'Bibliography',
            'index': 'Index',
            'colophon': 'Colophon',
            'epilogue': 'Epilogue',
            'afterword': 'Afterword',
            'contributors': 'Contributors',
            'other-credits': 'Credits',
        }
        # Check role attribute FIRST (more specific than element_type)
        # e.g., <preface role="copyright-page"> should have title "Copyright" not "Preface"
        role_attr = chapter.get('role', '')
        role_title_found = False
        for role_token in role_attr.split():
            if role_token in frontmatter_backmatter_titles:
                title_elem.text = frontmatter_backmatter_titles[role_token]
                logger.debug(f"Title extraction for {doc_path}: Using role '{role_token}' as title: {title_elem.text}")
                role_title_found = True
                break

        # Fall back to element_type if no role matched
        if not role_title_found and element_type in frontmatter_backmatter_titles:
            title_elem.text = frontmatter_backmatter_titles[element_type]
            logger.debug(f"Title extraction for {doc_path}: Using element_type as title: {title_elem.text}")

    # Special post-processing for bibliography elements
    if element_type == 'bibliography':
        _convert_to_bibliography_structure(chapter, chapter_id)
        # Wrap bare <bibliography> in <chapter><sect1> so RISChunker can route it.
        # RISChunker only chunks book//chapter/sect1, so a bare bibliography
        # is unreachable in the R2 platform without this wrapping.
        chapter = _wrap_bibliography_in_chapter(chapter, chapter_id)

    # Special post-processing for index elements
    if element_type == 'index':
        _convert_to_index_structure(chapter, chapter_id)

    # Remove any <index> elements from the output — index back matter is not needed
    # in R2 output and its DTD content model (primaryie/secondaryie) causes validation
    # errors when the converter produces <para> children instead
    for index_elem in list(chapter.iter('index')):
        parent = index_elem.getparent()
        if parent is not None:
            logger.info(f"Removing <index> element from {chapter_id} (index back matter not supported)")
            parent.remove(index_elem)

    # Special post-processing for glossary elements
    if element_type == 'glossary':
        _convert_to_glossary_structure(chapter, chapter_id)

    # Convert bibliography/references sections within chapters to proper bibliomixed structure
    # This ensures bibliography entries get 'bib' prefix IDs (XSL-recognized) instead of 'x' (unrecognized)
    # DTD allows: sect1 > (%nav.class;)* where nav.class includes bibliography
    # So the structure sect1 > bibliography > bibliomixed IS valid!
    if element_type != 'bibliography':  # Skip for dedicated bibliography chapters (already handled above)
        _convert_bibliography_sections_within_chapter(chapter, chapter_id)

    # Convert bibliography orderedlists to itemizedlist with mark="none" to prevent double-numbering
    # This applies to all chapter types since References sections can appear within regular chapters
    _convert_bibliography_orderedlist_to_itemizedlist(chapter)

    # Merge consecutive sibling orderedlists with matching roles (e.g., TocChapter).
    # Publisher TOC pages wrap each chapter in its own <ol class="TocChapter">, which the
    # converter faithfully converts to separate <orderedlist role="TocChapter"> elements.
    # The R2 platform renders each single-item list as "1." (first item). Merging them into
    # one list per role fixes the rendering to show proper sequential numbering.
    _merge_consecutive_sibling_orderedlists(chapter)

    # Merge standalone number paragraphs (e.g. <para>1.</para>) with the next text paragraph
    # inside list items, so the number and sentence appear on the same line
    _merge_item_number_paras(chapter)

    # Fix block elements (like mediaobject) incorrectly nested inside para elements
    # DTD does not allow block elements inside inline containers
    _fix_block_elements_in_para(chapter)

    # Fix bibliography placement to comply with DTD content model
    # Bibliography can only appear before block content OR after all sect2 elements
    _fix_bibliography_placement_in_sections(chapter)

    # Fix bibliodiv content order - biblioentry/bibliomixed must be LAST
    # DTD requires: (title...)?, (other block elements)*, (biblioentry|bibliomixed)+
    _fix_bibliodiv_content_order(chapter)

    # Fix nested bibliodiv - DTD doesn't allow bibliodiv inside bibliodiv
    _fix_nested_bibliodiv(chapter)

    # Ensure chapter has content beyond title (DTD requires at least one sect1 or other content)
    # Count non-title children
    content_count = sum(1 for child in chapter if child.tag != 'title')
    if content_count == 0:
        # Element has only a title - add content for DTD compliance
        if element_type == 'bibliography':
            # Bibliography needs bibliodiv or bibliomixed
            bibliodiv = validated_subelement(chapter, 'bibliodiv')
            validated_subelement(bibliodiv, 'title')
            bibliomixed = validated_subelement(bibliodiv, 'bibliomixed')
            bibliomixed.text = ' '  # Empty bibliomixed is not valid
            logger.debug(f"Added empty bibliodiv to bibliography: {chapter_id}")
        else:
            # Check if this is a "Part" chapter (title-only separator page)
            # For Part chapters, we don't need sect1 - just add para directly
            title_elem = chapter.find('title')
            title_text = title_elem.text.lower() if title_elem is not None and title_elem.text else ''
            is_part_chapter = title_text.startswith('part ')

            if is_part_chapter:
                # Part chapters are title-only pages - just add minimal para for DTD compliance
                # DTD allows chapter to have just divcomponent.mix (like para) without sect1
                para = validated_subelement(chapter, 'para')
                para.text = ' '  # Minimal non-empty content
                logger.debug(f"Added para to Part chapter (no sect1 needed): {chapter_id}")
            elif chapter.tag == 'chapter':
                # Regular chapters need sect1 for proper structure (RISChunker splits by sect1)
                section = validated_subelement(chapter, 'sect1')
                counters = _init_section_counters()
                section.set('id', id_gen_section_id(chapter_id, 1, counters))
                # Use empty title for section (same as chapter when no heading found)
                validated_subelement(section, 'title')
                validated_subelement(section, 'para')
                logger.debug(f"Added sect1 to chapter with only title: {chapter_id}")
            else:
                # Preface/appendix/other elements - add para only (DTD allows divcomponent.mix without sect1)
                para = validated_subelement(chapter, 'para')
                para.text = ' '  # Minimal non-empty content
                logger.debug(f"Added para to {element_type} with only title (no sect1 needed): {chapter_id}")

    # Ensure chapter elements have at least one sect1 (RittDoc requirement for chunking)
    # If there's content but no sect1, wrap all content inside a new sect1
    # NOTE: Preface/Appendix do NOT require sect1 - DTD allows (divcomponent.mix+, sect1*) | sect1+
    # Preface content like cover pages, copyright, etc. should NOT be wrapped in sect1
    # RISChunker.xsl chunks preface/appendix as a whole, not by sect1
    # IMPORTANT: Check the actual XML tag, not element_type, because contributors creates <chapter>
    section_types = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect', 'refentry'}
    xml_tags_requiring_sect1 = {'chapter'}  # Only <chapter> elements require sect1 for RISChunker splitting

    if chapter.tag in xml_tags_requiring_sect1:
        has_section = any(child.tag in section_types for child in chapter)

        if not has_section:
            # Collect all content elements that should be wrapped in sect1
            # (everything except title, chapterinfo, prefaceinfo, appendixinfo)
            metadata_tags = {'title', 'chapterinfo', 'prefaceinfo', 'appendixinfo',
                           'subtitle', 'titleabbrev'}
            content_to_wrap = [child for child in chapter if child.tag not in metadata_tags]

            if content_to_wrap:
                # Create new sect1
                sect1 = etree.Element('sect1')
                counters = _init_section_counters()
                sect1.set('id', id_gen_section_id(chapter_id, 1, counters))

                # Add title to sect1 (required by DTD)
                # Strategy: Use chapter title if available and non-empty
                # Otherwise, look for bridgehead or other title-like elements in content
                chapter_title = chapter.find('title')
                sect1_title = validated_subelement(sect1, 'title')

                title_set = False
                if chapter_title is not None and _title_has_text(chapter_title):
                    sect1_title.text = chapter_title.text
                    # Copy children but skip anchor elements (not needed)
                    for child in chapter_title:
                        if child.tag != 'anchor':
                            child_copy = deepcopy(child)
                            validated_append(sect1_title, child_copy)
                    title_set = True

                # If chapter title is empty, look for bridgehead in content
                if not title_set:
                    for content in content_to_wrap:
                        if content.tag == 'bridgehead':
                            bridgehead_text = ''.join(content.itertext()).strip()
                            if bridgehead_text:
                                sect1_title.text = bridgehead_text
                                title_set = True
                                break

                # Final fallback: use standard fallback title
                if not title_set:
                    _ensure_title_content(sect1_title, FALLBACK_SECTION_TITLE, "sect1")

                # Move content into sect1
                for content in content_to_wrap:
                    chapter.remove(content)
                    validated_append(sect1, content)

                # CRITICAL FIX: Update element IDs to match the new sect1 wrapper ID
                # Elements may have been created with IDs based on a different section context
                # (e.g., ch0018s0001ta01 when there was no sect1, but now sect1 is ch0018s0001)
                # We need to ensure IDs use the wrapper sect1 as their prefix
                new_sect1_id = sect1.get('id')
                _update_child_element_ids_for_new_section(sect1, chapter_id, new_sect1_id)

                # Add sect1 after metadata elements
                # Find position after title (and other metadata)
                insert_pos = 0
                for i, child in enumerate(chapter):
                    if child.tag in metadata_tags:
                        insert_pos = i + 1
                    else:
                        break

                chapter.insert(insert_pos, sect1)
                logger.info(f"Wrapped content in sect1 for {element_type}: {chapter_id}")

    matter_role = _extract_matter_role(soup)
    if matter_role:
        # Filter out redundant role tokens based on element type
        # - Don't add element_type to role (e.g., don't add "dedication" to <dedication>)
        # - Don't add "frontmatter" to elements that are inherently front matter
        # - Don't add "backmatter" to elements that are inherently back matter
        inherent_frontmatter = {'preface', 'dedication', 'acknowledgments', 'contributors', 'about'}
        inherent_backmatter = {'appendix', 'glossary', 'bibliography', 'index', 'colophon'}

        role_tokens = matter_role.split()
        filtered_tokens = []
        for token in role_tokens:
            # Skip if token matches the element type (redundant)
            if token == element_type:
                continue
            # Skip "frontmatter" for inherently front matter elements
            if token == 'frontmatter' and element_type in inherent_frontmatter:
                continue
            # Skip "backmatter" for inherently back matter elements
            if token == 'backmatter' and element_type in inherent_backmatter:
                continue
            filtered_tokens.append(token)

        if filtered_tokens:
            filtered_role = ' '.join(filtered_tokens)
            chapter.set('role', _merge_role(chapter.get('role', ''), filtered_role))
            for sect1 in chapter.iter('sect1'):
                sect1.set('role', _merge_role(sect1.get('role', ''), filtered_role))

    return chapter


def _strip_bibliography_number_prefix(text: str) -> str:
    """
    Strip leading number/letter prefixes from bibliography entry text.

    bibliomixed elements are auto-numbered by the rendering system, so we need to
    remove any existing numbering to prevent double-numbering.

    Patterns stripped:
    - Numbers: "1 ", "1. ", "1) ", "12.", "123 "
    - Bracketed numbers: "[1]", "[12]", "[123]"
    - Parenthesized numbers: "(1)", "(12)", "(123)"
    - Letters: "a. ", "a) ", "A. ", "A) "
    - Parenthesized letters: "(a)", "(A)"
    - Roman numerals: "i. ", "ii) ", "iv ", "xi.", "I. ", "II) ", "IV "
    - Parenthesized roman: "(i)", "(ii)", "(I)", "(II)"

    Args:
        text: Original text that may have numbering prefix

    Returns:
        Text with numbering prefix stripped
    """
    if not text:
        return text

    import re

    # Patterns to match and strip (must strip the matched part)
    numbering_patterns = [
        r'^\d{1,3}[\.\)\:\s]+',           # Numbers: "1 ", "1. ", "1) ", "1: ", "12.", "123 "
        r'^\[\d{1,3}\]\s*',               # Bracketed numbers: "[1]", "[12] ", "[123]"
        r'^\(\d{1,3}\)\s*',               # Parenthesized numbers: "(1)", "(12) ", "(123)"
        r'^[a-zA-Z][\.\)\:\s]+',          # Letters: "a. ", "a) ", "A. ", "A: "
        r'^\([a-zA-Z]\)\s*',              # Parenthesized letters: "(a)", "(A) "
        r'^[ivxlcdm]+[\.\)\:\s]+',        # Lowercase roman: "i. ", "ii) ", "iv ", "xi."
        r'^[IVXLCDM]+[\.\)\:\s]+',        # Uppercase roman: "I. ", "II) ", "IV ", "XI."
        r'^\([ivxlcdm]+\)\s*',            # Parenthesized lowercase roman: "(i)", "(ii) "
        r'^\([IVXLCDM]+\)\s*',            # Parenthesized uppercase roman: "(I)", "(II) "
    ]

    stripped_text = text.lstrip()
    for pattern in numbering_patterns:
        match = re.match(pattern, stripped_text)
        if match:
            # Strip the matched prefix
            stripped_text = stripped_text[match.end():]
            break

    return stripped_text


def _parse_bibliography_metadata(bibliomixed: etree.Element) -> bool:
    """
    Parse bibliography text content and create structured metadata elements.

    DISABLED: This function created structured metadata elements (author, authorgroup,
    citetitle, pubdate, publishername) inside <bibliomixed> alongside existing display
    text. The R2 renderer displays both the original text AND the metadata elements,
    causing visible duplication (e.g., "DevelopmentHall AK", "PotterCrisp J").
    The heuristic parsing fails on Vancouver-style citations (common in medical books)
    where no clear author-end marker exists, causing the entire citation text to be
    treated as author names and split incorrectly.

    The <bibliomixed> element's text content is sufficient for display without
    structured metadata overlay.

    Args:
        bibliomixed: The bibliomixed element to parse

    Returns:
        Always False (function disabled to prevent bibliography duplication)
    """
    return False  # Disabled — see docstring above


# The old _parse_bibliography_metadata implementation (removed) created bogus
# <author>, <authorgroup>, <citetitle>, <pubdate>, <publishername> elements
# that duplicated display text. See git history for the original code.
_parse_bibliography_metadata_DISABLED = True


def _sanitize_for_bibliomixed(elem: etree.Element, is_bibliomixed_root: bool = True) -> None:
    """
    Sanitize an element's children to only include content allowed in bibliomixed.

    DTD allows: #PCDATA | %bibliocomponent.mix; | bibliomset
    NOT allowed: ulink, anchor, link, xref, indexterm, emphasis, phrase, etc.

    This function:
    - Converts ulink/link elements to plain text (keeping the link text)
    - Converts emphasis/phrase to citetitle (for italic journal names) or plain text
    - Removes anchor elements (empty ID placeholders)
    - Removes other disallowed elements, preserving their text content

    Args:
        elem: Element to sanitize (modifies in place)
    """
    # Elements allowed in bibliomixed per DTD (bibliocomponent.mix + bibliomset)
    # NOTE: emphasis, phrase, quote are NOT allowed - only bibliographic elements
    allowed_elements = {
        'abbrev', 'abstract', 'address', 'artpagenums', 'author',
        'authorgroup', 'authorinitials', 'bibliomisc', 'biblioset',
        'collab', 'confgroup', 'contractnum', 'contractsponsor',
        'copyright', 'corpauthor', 'corpname', 'corpcredit', 'date', 'edition',
        'editor', 'invpartnumber', 'isbn', 'issn', 'issuenum', 'orgname',
        'biblioid', 'citebiblioid', 'bibliosource', 'bibliorelation', 'bibliocoverage',
        'othercredit', 'pagenums', 'printhistory', 'productname',
        'productnumber', 'pubdate', 'publisher', 'publishername',
        'pubsnumber', 'releaseinfo', 'revhistory', 'seriesvolnums',
        'subtitle', 'title', 'titleabbrev', 'volumenum', 'citetitle',
        'bibliomset',
        # Person name elements
        'personname', 'firstname', 'surname', 'othername', 'lineage', 'honorific',
    }

    def get_all_text(el: etree.Element) -> str:
        """Extract all text content from an element recursively.

        Ensures proper spacing between text from different child elements
        to prevent concatenation issues (e.g., 'PubMedPubMed Central').
        """
        texts = []
        if el.text:
            texts.append(el.text)
        for child in el:
            child_text = get_all_text(child)
            if child_text:
                # Add space between adjacent text segments if needed
                # (prevents 'PubMed' + 'PubMed Central' → 'PubMedPubMed Central')
                if texts and texts[-1] and not texts[-1][-1].isspace() and not child_text[0].isspace():
                    texts.append(' ')
                texts.append(child_text)
            if child.tail:
                texts.append(child.tail)
        return ''.join(texts)

    # Process children in reverse order so we can modify the list while iterating
    children = list(elem)
    for child in children:
        tag = child.tag if isinstance(child.tag, str) else None
        if tag is None:
            continue

        # Get local name (strip namespace if present)
        local_tag = tag.split('}')[-1] if '}' in tag else tag

        if local_tag in ('ulink', 'link', 'xref'):
            if is_bibliomixed_root and local_tag == 'ulink' and child.get('url'):
                # At bibliomixed level: wrap ulink in bibliomisc so it's DTD-legal
                # This preserves PubMed, Crossref, and DOI hyperlinks
                idx = list(elem).index(child)
                tail = child.tail
                child.tail = None
                elem.remove(child)
                bibliomisc = etree.Element('bibliomisc')
                validated_append(bibliomisc, child)
                bibliomisc.tail = tail
                elem.insert(idx, bibliomisc)
            else:
                # Inside nested elements or no URL: convert to plain text
                link_text = get_all_text(child)
                idx = list(elem).index(child)
                prev_sibling = elem[idx - 1] if idx > 0 else None

                if prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + link_text + (child.tail or '')
                else:
                    elem.text = (elem.text or '') + link_text + (child.tail or '')

                elem.remove(child)

        elif local_tag in ('subscript', 'superscript'):
            # subscript/superscript is only allowed inside bibliomisc, not directly in bibliomixed
            # But when nested inside other elements (like citetitle), we should convert to plain text
            # since bibliomisc can't be placed inside citetitle
            if is_bibliomixed_root:
                # At bibliomixed level: wrap sub/sup in bibliomisc so they are legal
                idx = list(elem).index(child)
                tail = child.tail
                child.tail = None
                elem.remove(child)
                bibliomisc = etree.Element('bibliomisc')
                validated_append(bibliomisc, child)
                bibliomisc.tail = tail
                elem.insert(idx, bibliomisc)
            else:
                # Inside nested elements (like citetitle): convert to plain text
                child_text = get_all_text(child)
                idx = list(elem).index(child)
                prev_sibling = elem[idx - 1] if idx > 0 else None

                if prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + child_text + (child.tail or '')
                else:
                    elem.text = (elem.text or '') + child_text + (child.tail or '')

                elem.remove(child)

        elif local_tag == 'anchor':
            # Remove anchor completely, preserve tail text
            idx = list(elem).index(child)
            prev_sibling = elem[idx - 1] if idx > 0 else None

            if child.tail:
                if prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + child.tail
                else:
                    elem.text = (elem.text or '') + child.tail

            elem.remove(child)

        elif local_tag == 'indexterm':
            # Remove indexterm completely (not allowed in bibliomixed)
            idx = list(elem).index(child)
            prev_sibling = elem[idx - 1] if idx > 0 else None

            if child.tail:
                if prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + child.tail
                else:
                    elem.text = (elem.text or '') + child.tail

            elem.remove(child)

        elif local_tag in ('emphasis', 'cite', 'i', 'em', 'b', 'strong', 'phrase', 'quote', 'trademark', 'firstterm', 'para', 'simpara'):
            # Unwrap these elements - keep text content but remove the wrapper
            child_text = child.text or ''
            child_tail = child.tail or ''
            idx = list(elem).index(child)
            prev_sibling = elem[idx - 1] if idx > 0 else None

            # Move children to parent
            insert_pos = idx
            for grandchild in list(child):
                elem.insert(insert_pos, grandchild)
                insert_pos += 1

            # Handle text
            if child_text:
                if prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + child_text
                else:
                    elem.text = (elem.text or '') + child_text

            # Handle tail - attach to last moved grandchild or prev_sibling
            if child_tail:
                if len(list(child)) > 0:
                    last_grandchild = list(child)[-1]
                    last_grandchild.tail = (last_grandchild.tail or '') + child_tail
                elif prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + child_tail
                else:
                    elem.text = (elem.text or '') + child_tail

            elem.remove(child)

        elif local_tag not in allowed_elements:
            # Unknown element - convert to plain text
            child_text = get_all_text(child)
            idx = list(elem).index(child)
            prev_sibling = elem[idx - 1] if idx > 0 else None

            if prev_sibling is not None:
                prev_sibling.tail = (prev_sibling.tail or '') + child_text + (child.tail or '')
            else:
                elem.text = (elem.text or '') + child_text + (child.tail or '')

            elem.remove(child)

        else:
            # Recursively sanitize allowed elements (pass is_bibliomixed_root=False)
            if local_tag != 'bibliomisc':
                _sanitize_for_bibliomixed(child, is_bibliomixed_root=False)

    # After processing all original children, check if new disallowed elements were inserted
    # (e.g., when unwrapping emphasis that contained nested subscript/superscript elements)
    # Run another pass to catch any remaining issues
    # IMPORTANT: Include subscript/superscript which may have been moved from unwrapped elements
    remaining_disallowed = [
        c for c in elem
        if isinstance(c.tag, str) and c.tag.split('}')[-1] in (
            'emphasis', 'cite', 'i', 'em', 'b', 'strong', 'phrase', 'quote',
            'trademark', 'firstterm', 'para', 'simpara', 'ulink', 'link',
            'xref', 'anchor', 'indexterm', 'subscript', 'superscript'
        )
    ]
    if remaining_disallowed:
        # Recursively sanitize to handle any leftover disallowed elements
        _sanitize_for_bibliomixed(elem, is_bibliomixed_root=is_bibliomixed_root)


def _convert_to_bibliography_structure(bib_elem: etree.Element, chapter_id: str) -> None:
    """
    Convert paragraph and list content in a bibliography element to proper bibliomixed structure.

    This function converts:
    - <para> elements to <bibliomixed>
    - <orderedlist>/<itemizedlist> <listitem> elements to <bibliomixed>
    - <section> elements to <bibliodiv>

    Note: Leading numbers are stripped from bibliography entries since bibliomixed
    elements are auto-numbered by the rendering system.

    Args:
        bib_elem: The bibliography element to process
        chapter_id: Chapter ID for generating unique IDs
    """
    bib_entry_counter = 0

    # Remove elements not allowed directly under bibliography (anchor, indexterm, etc.)
    # DTD only allows: (bibliographyinfo?, title?, subtitle?, titleabbrev?,
    #                   (content elements)*, (bibliodiv+ | (biblioentry|bibliomixed)+))
    disallowed_direct_children = {'anchor', 'indexterm', 'ulink', 'link', 'xref'}
    for child in list(bib_elem):
        if child.tag in disallowed_direct_children:
            # Preserve tail text by appending to previous sibling or parent
            idx = list(bib_elem).index(child)
            prev_sibling = bib_elem[idx - 1] if idx > 0 else None
            if child.tail:
                if prev_sibling is not None:
                    prev_sibling.tail = (prev_sibling.tail or '') + child.tail
                else:
                    bib_elem.text = (bib_elem.text or '') + child.tail
            bib_elem.remove(child)

    # Section tags that should be converted to bibliodiv
    section_tags = {'section', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5'}

    # Find all section elements that should become bibliodiv
    # We need to process these in place to maintain structure
    # Process both 'section' and 'sect1-5' elements
    for section_tag in section_tags:
        for section in bib_elem.iter(section_tag):
            _process_section_for_bibliography(section, chapter_id, bib_entry_counter)

    # Also process paras and lists directly under bibliography (not in sections)
    # Note: sections are already processed by the iter() loop above
    paras_to_convert = []
    lists_to_convert = []
    for child in list(bib_elem):
        if child.tag == 'para':
            paras_to_convert.append(child)
        elif child.tag in ('orderedlist', 'itemizedlist'):
            lists_to_convert.append(child)

    # Create bibliodiv if we have content to convert
    if paras_to_convert or lists_to_convert:
        # Create a bibliodiv to hold entries.
        # IMPORTANT: Keep the bibliography's own <title> intact (e.g., "References").
        # bibliodiv titles should come from subsection/grouping titles (if any).
        bibliodiv = etree.Element('bibliodiv')

        # Convert paragraphs to bibliomixed
        for para in paras_to_convert:
            bib_entry_counter += 1
            # Get original para ID for mapping (if any)
            original_para_id = para.get('id')

            # Convert para to bibliomixed
            bibliomixed = etree.Element('bibliomixed')
            new_bib_id = generate_element_id(chapter_id, 'bibliography')
            bibliomixed.set('id', new_bib_id)

            # Register ID mapping from original para ID to new bibliomixed ID
            # Also update all existing mappings that point to the old generated ID
            if original_para_id:
                register_id_mapping(chapter_id, original_para_id, new_bib_id)
                update_id_mapping_target(original_para_id, new_bib_id)
                _update_links_to_new_id(bib_elem, original_para_id, new_bib_id)
                logger.debug(f"Registered bibliography ID mapping: {original_para_id} -> {new_bib_id}")

            # Copy all content from para to bibliomixed, stripping number prefix
            bibliomixed.text = _strip_bibliography_number_prefix(para.text) if para.text else para.text
            for child in list(para):
                validated_append(bibliomixed, child)
            bibliomixed.tail = para.tail

            # Sanitize content - remove elements not allowed in bibliomixed (ulink, anchor, etc.)
            _sanitize_for_bibliomixed(bibliomixed)

            # Parse metadata from bibliography text
            _parse_bibliography_metadata(bibliomixed)

            # Add to bibliodiv
            validated_append(bibliodiv, bibliomixed)

            # Remove original para
            bib_elem.remove(para)

        # Convert list items to bibliomixed
        for lst in lists_to_convert:
            for listitem in list(lst.findall('listitem')):
                bib_entry_counter += 1
                # Get original listitem ID for mapping (if any)
                original_listitem_id = listitem.get('id')

                # Convert listitem content to bibliomixed
                bibliomixed = etree.Element('bibliomixed')
                new_bib_id = generate_element_id(chapter_id, 'bibliography')
                bibliomixed.set('id', new_bib_id)

                # Register ID mapping from original listitem ID to new bibliomixed ID
                # Also update all existing mappings that point to the old generated ID
                if original_listitem_id:
                    register_id_mapping(chapter_id, original_listitem_id, new_bib_id)
                    # Update all original source IDs that were mapped to this generated ID
                    update_id_mapping_target(original_listitem_id, new_bib_id)
                    _update_links_to_new_id(bib_elem, original_listitem_id, new_bib_id)
                    logger.debug(f"Registered bibliography ID mapping: {original_listitem_id} -> {new_bib_id}")

                # listitem typically contains <para> - extract content from inner para
                # Strip leading numbers since bibliomixed auto-numbers
                # C-015 FIX: When listitem has multiple paras (e.g., from ItemNumber +
                # ItemContent divs), the first para may only contain the number label.
                # We must find the para with actual content, not just the first one.
                all_paras = listitem.findall('para')
                inner_para = all_paras[0] if all_paras else None
                if inner_para is not None:
                    stripped_text = _strip_bibliography_number_prefix(inner_para.text) if inner_para.text else inner_para.text
                    # If first para is just a number label (empty after stripping prefix)
                    # and has no child elements, try the next para which has actual content
                    if (not (stripped_text and stripped_text.strip()) and
                            len(inner_para) == 0 and len(all_paras) > 1):
                        inner_para = all_paras[1]
                        stripped_text = _strip_bibliography_number_prefix(inner_para.text) if inner_para.text else inner_para.text
                    bibliomixed.text = stripped_text
                    for child in list(inner_para):
                        validated_append(bibliomixed, child)
                else:
                    # No inner para - copy listitem content directly
                    bibliomixed.text = _strip_bibliography_number_prefix(listitem.text) if listitem.text else listitem.text
                    for child in list(listitem):
                        validated_append(bibliomixed, child)

                # Sanitize content - remove elements not allowed in bibliomixed (ulink, anchor, etc.)
                _sanitize_for_bibliomixed(bibliomixed)

                # Parse metadata from bibliography text
                _parse_bibliography_metadata(bibliomixed)

                # Add to bibliodiv
                validated_append(bibliodiv, bibliomixed)

            # Remove original list
            bib_elem.remove(lst)

        # Insert bibliodiv after title
        title_elem = bib_elem.find('title')
        if title_elem is not None:
            title_index = list(bib_elem).index(title_elem)
            bib_elem.insert(title_index + 1, bibliodiv)
        else:
            bib_elem.insert(0, bibliodiv)

        logger.debug(f"Converted {bib_entry_counter} items to bibliomixed in {chapter_id}")

    # Fix C-006: Post-processing to fix double periods in all bibliomixed entries.
    # These can occur when text fragments are assembled and both source and
    # conversion add trailing periods (e.g., "et al.." or "2023..").
    # Use negative lookbehind/lookahead to preserve genuine ellipsis (...).
    _double_period_re = re.compile(r'(?<!\.)\.\.(?!\.)')
    for bm in bib_elem.iter('bibliomixed'):
        if bm.text and '..' in bm.text:
            bm.text = _double_period_re.sub('.', bm.text)
        for child in bm:
            if child.text and '..' in child.text:
                child.text = _double_period_re.sub('.', child.text)
            if child.tail and '..' in child.tail:
                child.tail = _double_period_re.sub('.', child.tail)


def _process_section_for_bibliography(section: etree.Element, chapter_id: str,
                                       counter_start: int) -> int:
    """
    Process a section within a bibliography, converting paras and list items to bibliomixed.

    Args:
        section: Section element to process
        chapter_id: Chapter ID for ID generation
        counter_start: Starting counter for bibliography entry IDs

    Returns:
        Updated counter value
    """
    counter = counter_start

    # Convert section to bibliodiv (preserving existing <title> as the division title)
    section.tag = 'bibliodiv'

    # Convert paras to bibliomixed
    for para in list(section.findall('para')):
        counter += 1
        # Get original para ID for mapping (if any)
        original_para_id = para.get('id')

        # Convert para to bibliomixed, stripping leading numbers
        para.tag = 'bibliomixed'
        new_bib_id = generate_element_id(chapter_id, 'bibliography')
        para.set('id', new_bib_id)

        # Register ID mapping from original para ID to new bibliomixed ID
        # Also update all existing mappings that point to the old generated ID
        if original_para_id:
            register_id_mapping(chapter_id, original_para_id, new_bib_id)
            update_id_mapping_target(original_para_id, new_bib_id)
            _update_links_to_new_id(section, original_para_id, new_bib_id)
            logger.debug(f"Registered bibliography ID mapping: {original_para_id} -> {new_bib_id}")

        # Strip leading numbers since bibliomixed auto-numbers
        if para.text:
            para.text = _strip_bibliography_number_prefix(para.text)
        # Sanitize content - remove elements not allowed in bibliomixed (ulink, anchor, etc.)
        _sanitize_for_bibliomixed(para)
        # Parse metadata from bibliography text (para is now bibliomixed)
        _parse_bibliography_metadata(para)

    # Convert list items to bibliomixed
    for lst in list(section.findall('orderedlist')) + list(section.findall('itemizedlist')):
        for listitem in list(lst.findall('listitem')):
            counter += 1
            # Get original listitem ID for mapping (if any)
            original_listitem_id = listitem.get('id')

            # Create bibliomixed from listitem content
            bibliomixed = etree.Element('bibliomixed')
            new_bib_id = generate_element_id(chapter_id, 'bibliography')
            bibliomixed.set('id', new_bib_id)

            # Register ID mapping from original listitem ID to new bibliomixed ID
            # Also update all existing mappings that point to the old generated ID
            if original_listitem_id:
                register_id_mapping(chapter_id, original_listitem_id, new_bib_id)
                # Update all original source IDs that were mapped to this generated ID
                update_id_mapping_target(original_listitem_id, new_bib_id)
                _update_links_to_new_id(section, original_listitem_id, new_bib_id)
                logger.debug(f"Registered bibliography ID mapping: {original_listitem_id} -> {new_bib_id}")

            # listitem typically contains <para> - extract content from inner para
            # Strip leading numbers since bibliomixed auto-numbers
            # C-015 FIX: When listitem has multiple paras (e.g., from ItemNumber +
            # ItemContent divs), the first para may only contain the number label.
            # We must find the para with actual content, not just the first one.
            all_paras = listitem.findall('para')
            inner_para = all_paras[0] if all_paras else None
            if inner_para is not None:
                stripped_text = _strip_bibliography_number_prefix(inner_para.text) if inner_para.text else inner_para.text
                # If first para is just a number label (empty after stripping prefix)
                # and has no child elements, try the next para which has actual content
                if (not (stripped_text and stripped_text.strip()) and
                        len(inner_para) == 0 and len(all_paras) > 1):
                    inner_para = all_paras[1]
                    stripped_text = _strip_bibliography_number_prefix(inner_para.text) if inner_para.text else inner_para.text
                bibliomixed.text = stripped_text
                for child in list(inner_para):
                    validated_append(bibliomixed, child)
            else:
                # No inner para - copy listitem content directly
                bibliomixed.text = _strip_bibliography_number_prefix(listitem.text) if listitem.text else listitem.text
                for child in list(listitem):
                    validated_append(bibliomixed, child)

            # Sanitize content - remove elements not allowed in bibliomixed (ulink, anchor, etc.)
            _sanitize_for_bibliomixed(bibliomixed)
            # Parse metadata from bibliography text
            _parse_bibliography_metadata(bibliomixed)

            # Insert bibliomixed before the list
            lst_index = list(section).index(lst)
            section.insert(lst_index, bibliomixed)

        # Remove the original list
        section.remove(lst)

    # Handle nested sections recursively (both 'section' and 'sect1-5')
    nested_section_tags = ['section', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5']
    for nested_tag in nested_section_tags:
        for nested_section in list(section.findall(nested_tag)):
            counter = _process_section_for_bibliography(nested_section, chapter_id, counter)

    return counter


def _merge_item_number_paras(root: etree.Element) -> int:
    """
    Merge standalone number paragraphs with the following text paragraph in list items.

    Springer EPUBs use ``<div class="ItemNumber">1.</div>`` + ``<div class="ItemContent"><p>Text</p></div>``
    inside ``<li>`` elements.  The converter creates separate ``<para>`` elements for each,
    causing the number to appear on a separate line from the sentence text.

    This function finds ``<para>`` elements inside ``<listitem>`` that contain only a short
    numbering pattern (e.g. "1.", "2)", "(a)") and merges their text into the next
    sibling ``<para>``, so the output is: ``<para>1.\\tText...</para>``.

    Returns:
        Number of merges performed.
    """
    merge_count = 0
    number_pattern = re.compile(
        r'^\s*(\d{1,3}[\.\):]|\(\d{1,3}\)|[a-zA-Z][\.\)]|\([a-zA-Z]\)|[ivxIVX]+[\.\)])\s*$'
    )

    for listitem in list(root.iter('listitem')):
        children = list(listitem)
        i = 0
        while i < len(children) - 1:
            child = children[i]
            next_child = children[i + 1]
            # Check if current child is a <para> with only number text (no sub-elements)
            if (child.tag == 'para' and len(child) == 0
                    and child.text and number_pattern.match(child.text)
                    and next_child.tag == 'para'):
                # Merge: prepend number text to next para
                num_text = child.text.strip()
                if next_child.text:
                    next_child.text = num_text + '\t' + next_child.text
                else:
                    next_child.text = num_text + '\t'
                # Remove the number-only para
                listitem.remove(child)
                merge_count += 1
                # Don't increment i — children shifted
                children = list(listitem)
            else:
                i += 1

    if merge_count > 0:
        logger.debug(f"Merged {merge_count} standalone number paragraph(s) with text paragraphs in list items")
    return merge_count


def _merge_consecutive_sibling_orderedlists(root: etree.Element) -> int:
    """
    Merge consecutive sibling <orderedlist> elements that share the same role attribute.

    In publisher TOC pages (Contents), each chapter is often wrapped in its own
    separate <ol class="TocChapter"> in the source XHTML. The converter faithfully
    converts each to a separate <orderedlist role="TocChapter">. On the R2 platform,
    each single-item orderedlist restarts numbering, causing every chapter to render
    with "1." instead of sequential numbering.

    This function merges consecutive sibling orderedlists with the same role into one,
    so the platform renders them as a single numbered list.

    Only merges lists where BOTH have the same non-empty role attribute, to avoid
    accidentally merging unrelated lists.

    Args:
        root: Root element to process

    Returns:
        Number of orderedlists merged (removed)
    """
    merged_count = 0

    for parent in list(root.iter()):
        children = list(parent)
        if len(children) < 2:
            continue

        i = 0
        while i < len(children) - 1:
            current = children[i]
            next_elem = children[i + 1]

            if (current.tag == 'orderedlist' and next_elem.tag == 'orderedlist'):
                current_role = current.get('role', '')
                next_role = next_elem.get('role', '')

                if current_role and current_role == next_role:
                    for child in list(next_elem):
                        current.append(child)
                    parent.remove(next_elem)
                    merged_count += 1
                    children = list(parent)
                    continue

            i += 1

    if merged_count > 0:
        logger.info(f"Merged {merged_count} consecutive sibling orderedlists with matching roles")

    return merged_count


def _convert_bibliography_orderedlist_to_itemizedlist(root: etree.Element) -> int:
    """
    Convert orderedlist elements with pre-existing numbering to itemizedlist with mark="none".

    This prevents double-numbering when list items already contain their own
    numbering (e.g., "1 Hayes D..." would otherwise render as "1. 1 Hayes...").

    Detection criteria:
    1. orderedlist with role="biblioEntryList"
    2. orderedlist containing listitem with role="bibliographyEntry"
    3. orderedlist where >50% of listitem/para text starts with numbering patterns:
       - Numbers: "1 ", "1. ", "1) ", "[1]", "(1)"
       - Letters: "a. ", "a) ", "A. ", "(a)", "(A)"
       - Roman numerals: "i. ", "ii) ", "I. ", "IV ", "(i)", "(II)"

    Args:
        root: Root element to process

    Returns:
        Number of orderedlists converted
    """
    import re
    converted_count = 0

    # Patterns to detect text starting with pre-existing list numbering/lettering
    # This prevents double-numbering when orderedlist adds its own numbers
    numbering_patterns = [
        r'^\d{1,3}[\.\)\s]',          # Numbers: "1 ", "1. ", "1) ", "12.", "123 "
        r'^\[\d{1,3}\]',              # Bracketed numbers: "[1]", "[12]", "[123]"
        r'^\(\d{1,3}\)',              # Parenthesized numbers: "(1)", "(12)", "(123)"
        r'^[a-zA-Z][\.\)\s]',         # Letters: "a. ", "a) ", "A. ", "A) "
        r'^\([a-zA-Z]\)',             # Parenthesized letters: "(a)", "(A)"
        r'^[ivxlcdm]+[\.\)\s]',       # Lowercase roman: "i. ", "ii) ", "iv ", "xi."
        r'^[IVXLCDM]+[\.\)\s]',       # Uppercase roman: "I. ", "II) ", "IV ", "XI."
        r'^\([ivxlcdm]+\)',           # Parenthesized lowercase roman: "(i)", "(ii)"
        r'^\([IVXLCDM]+\)',           # Parenthesized uppercase roman: "(I)", "(II)"
    ]
    # Combine all patterns into one
    combined_pattern = re.compile('|'.join(numbering_patterns))

    for orderedlist in list(root.iter('orderedlist')):
        should_convert = False

        # Check criterion 1: role="biblioEntryList"
        if orderedlist.get('role') == 'biblioEntryList':
            should_convert = True

        # Check criterion 2: listitem with role="bibliographyEntry"
        if not should_convert:
            for listitem in orderedlist.findall('listitem'):
                if listitem.get('role') == 'bibliographyEntry':
                    should_convert = True
                    break

        # Check criterion 3: listitem/para text starts with numbering/lettering
        # ONLY apply this heuristic inside bibliography contexts to avoid
        # converting body content ordered lists (e.g., Springer <ol> with
        # <div class="ItemNumber"> pre-numbering that was already stripped)
        if not should_convert:
            parent = orderedlist.getparent()
            in_bibliography = False
            while parent is not None:
                if parent.tag in ('bibliography', 'bibliodiv'):
                    in_bibliography = True
                    break
                parent = parent.getparent()

            if in_bibliography:
                numbered_items = 0
                total_items = 0
                for listitem in orderedlist.findall('listitem'):
                    total_items += 1
                    para = listitem.find('para')
                    if para is not None and para.text:
                        text = para.text.strip()
                        if combined_pattern.match(text):
                            numbered_items += 1

                # If majority of items start with numbering, convert
                # (use > 50% threshold to avoid false positives)
                if total_items > 0 and numbered_items / total_items > 0.5:
                    should_convert = True

        if should_convert:
            # Convert orderedlist to itemizedlist with mark="none"
            orderedlist.tag = 'itemizedlist'
            orderedlist.set('mark', 'none')
            converted_count += 1
            logger.debug(f"Converted orderedlist to itemizedlist mark='none' (pre-numbered entries)")

    return converted_count


def _update_links_to_new_id(root: etree.Element, old_id: str, new_id: str) -> int:
    """
    Update all links that reference old_id to use new_id instead.

    This is called when converting bibliography list items to bibliomixed,
    to ensure that existing links point to the new bibliomixed ID rather
    than the old listitem ID (which will be removed).

    Args:
        root: Root element to search for links
        old_id: The old ID being replaced (e.g., listitem 'x' prefix ID)
        new_id: The new ID to use (e.g., bibliomixed 'bib' prefix ID)

    Returns:
        Number of links updated
    """
    updated_count = 0

    # Elements with linkend attribute: link, xref, biblioref, footnoteref, etc.
    for elem in root.iter():
        linkend = elem.get('linkend')
        if linkend == old_id:
            elem.set('linkend', new_id)
            updated_count += 1
            logger.debug(f"Updated linkend {old_id} -> {new_id} in <{elem.tag}>")

        # Also check url attributes for internal links
        url = elem.get('url')
        if url and old_id in url:
            new_url = url.replace(old_id, new_id)
            elem.set('url', new_url)
            updated_count += 1
            logger.debug(f"Updated url {old_id} -> {new_id} in <{elem.tag}>")

    if updated_count > 0:
        logger.info(f"Updated {updated_count} link(s) from {old_id} to {new_id}")

    return updated_count


def _get_next_section_id_for_bibliography(root: etree.Element, chapter_id: str) -> str:
    """
    Generate the next sect1-style ID for a bibliography element.

    Finds the highest existing section counter in the chapter and returns
    the next ID. For example, if the highest existing sect1 ID is ch0009s0021,
    this returns ch0009s0022.

    Args:
        root: Chapter root element to search for existing section IDs
        chapter_id: Chapter ID (e.g., "ch0009")

    Returns:
        Next section ID in format chnnnnsnnnn (e.g., "ch0009s0022")
    """
    # Pattern to match sect1 IDs: ch####s#### (11 characters)
    sect1_id_pattern = re.compile(rf'^{re.escape(chapter_id)}s(\d{{4}})$')

    highest_counter = 0

    # Search all elements with IDs that match the sect1 pattern
    for elem in root.iter():
        elem_id = elem.get('id')
        if elem_id:
            match = sect1_id_pattern.match(elem_id)
            if match:
                counter = int(match.group(1))
                if counter > highest_counter:
                    highest_counter = counter

    # Generate next ID (increment by 1)
    next_counter = highest_counter + 1
    next_id = f"{chapter_id}s{next_counter:04d}"

    logger.debug(f"Generated bibliography ID {next_id} (highest existing sect1 counter: {highest_counter})")
    return next_id


def _wrap_bibliography_in_chapter(bib_elem: etree.Element, chapter_id: str) -> etree.Element:
    """Wrap a ``<bibliography>`` element inside ``<chapter><sect1>`` for routing.

    The bookloader's RISChunker only recognises ``sect1`` boundaries as
    navigable pages.  A bare ``<bibliography>`` has no routing markers and
    is unreachable.  Wrapping it as::

        <chapter id="ch0005" role="bibliography">
          <title>References</title>
          <sect1 id="ch0005s0001" role="bibliography">
            <title>References</title>
            <para>&#x00A0;</para>
            <bibliography id="ch0005bib">
              <bibliomixed .../>
              ...
            </bibliography>
          </sect1>
        </chapter>

    makes it routable while keeping valid DTD structure (``bibliography``
    is a member of ``nav.class`` and allowed inside ``sect1``).

    Returns the new ``<chapter>`` element that replaces *bib_elem*.
    """
    bib_title_elem = bib_elem.find('title')
    bib_title_text = (bib_title_elem.text or '').strip() if bib_title_elem is not None else 'References'

    # Create chapter wrapper with role="bibliography" for semantic tagging
    chapter_wrapper = etree.Element('chapter', id=chapter_id)
    chapter_wrapper.set('role', 'bibliography')
    ch_title = etree.SubElement(chapter_wrapper, 'title')
    ch_title.text = bib_title_text

    # Create sect1
    sect1_id = f"{chapter_id}s0001"
    sect1 = etree.SubElement(chapter_wrapper, 'sect1', id=sect1_id)
    sect1.set('role', 'bibliography')
    s1_title = etree.SubElement(sect1, 'title')
    s1_title.text = bib_title_text

    # DTD requires at least one block element before the nav.class
    # (bibliography) position.  Add a non-breaking space para as spacer.
    spacer = etree.SubElement(sect1, 'para')
    spacer.text = '\u00a0'

    # Move the bibliography element inside sect1.
    # Remove the bibliography's own title to avoid duplication (sect1 has it).
    if bib_title_elem is not None:
        bib_elem.remove(bib_title_elem)
    # Assign an id to the inner <bibliography> for cross-referencing
    bib_elem.set('id', f"{chapter_id}bib")
    sect1.append(bib_elem)

    logger.info(f"Wrapped bibliography {chapter_id} in chapter/sect1 for routing")
    return chapter_wrapper


def _convert_bibliography_sections_within_chapter(root: etree.Element, chapter_id: str) -> int:
    """
    Detect and convert bibliography/references sections within a chapter to proper bibliomixed structure.

    This handles cases where a chapter contains a "References" or "Bibliography" section
    that should use bibliomixed elements (with 'bib' prefix IDs) instead of list items
    (which would get unrecognized 'x' prefix IDs).

    Detection criteria (any match triggers conversion):
    1. Section title contains bibliography-related keywords
    2. Section has epub:type or role attribute indicating bibliography
    3. Section contains lists with role="biblioEntryList" or listitem with role="bibliographyEntry"
    4. Section has CSS class indicating bibliography (preserved as 'role' attribute)

    Args:
        root: Root element (chapter) to process
        chapter_id: Chapter ID for generating unique IDs

    Returns:
        Number of sections converted
    """
    converted_count = 0

    # Title patterns that indicate bibliography content (case-insensitive)
    bibliography_title_patterns = [
        'references', 'bibliography', 'works cited', 'literature cited',
        'cited works', 'further reading', 'suggested reading', 'recommended reading',
        'citations', 'notes and references', 'endnotes',
        'bibliografía', 'bibliographie', 'literaturverzeichnis',  # Common non-English
    ]

    # Titles that should NOT be treated as bibliography even though they contain
    # bibliography-related keywords (e.g., "Cross-References" contains "references")
    non_bibliography_title_patterns = [
        'cross-references', 'cross references', 'crossreferences',
        'resources',  # "Resources Required", "Additional Resources" etc. are NOT bibliography
    ]

    # Role/epub:type values that indicate bibliography
    bibliography_type_values = [
        'bibliography', 'doc-bibliography', 'references', 'biblioentrylist',
    ]

    # Section tags to check
    section_tags = ['sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect']

    for section_tag in section_tags:
        for section in list(root.iter(section_tag)):
            # Skip if already converted (has bibliodiv children)
            if section.find('bibliodiv') is not None or section.find('bibliomixed') is not None:
                continue

            is_bibliography_section = False
            detection_reason = ""

            # Check 1: Title contains bibliography keywords
            title_elem = section.find('title')
            if title_elem is not None and title_elem.text:
                title_lower = title_elem.text.lower().strip()
                # First check exclusions — titles like "Cross-References" should NOT
                # be treated as bibliography even though they contain "references"
                is_excluded_title = any(excl in title_lower for excl in non_bibliography_title_patterns)
                if not is_excluded_title:
                    for pattern in bibliography_title_patterns:
                        if pattern in title_lower:
                            is_bibliography_section = True
                            detection_reason = f"title contains '{pattern}'"
                            break

            # Check 2: Role attribute indicates bibliography
            if not is_bibliography_section:
                role_attr = (section.get('role') or '').lower()
                for bib_type in bibliography_type_values:
                    if bib_type in role_attr:
                        is_bibliography_section = True
                        detection_reason = f"role contains '{bib_type}'"
                        break

            # Check 3: Lists with bibliography-related roles
            if not is_bibliography_section:
                for lst in section.iter():
                    if lst.tag in ('orderedlist', 'itemizedlist'):
                        lst_role = (lst.get('role') or '').lower()
                        if 'biblioentrylist' in lst_role or 'bibliography' in lst_role:
                            is_bibliography_section = True
                            detection_reason = "list has biblioEntryList role"
                            break
                    elif lst.tag == 'listitem':
                        li_role = (lst.get('role') or '').lower()
                        if 'bibliographyentry' in li_role or 'biblioentry' in li_role:
                            is_bibliography_section = True
                            detection_reason = "listitem has bibliographyEntry role"
                            break

            # If detected as bibliography section, convert it
            if is_bibliography_section:
                logger.debug(f"Converting section to bibliography structure ({detection_reason}): {section.get('id', 'no-id')}")

                # Convert the section content to bibliomixed
                # Keep the section tag (don't change to bibliodiv) to maintain chapter structure
                # But convert internal lists/paras to bibliomixed

                bib_entry_counter = 0
                all_bibliomixed = []  # Collect all bibliomixed elements

                # Find lists to convert - search recursively since lists may be nested in para/simpara
                lists_to_process = []
                for lst in section.iter():
                    if lst.tag in ('orderedlist', 'itemizedlist'):
                        # Fix: Skip lists inside <sidebar> elements — these are box/callout content
                        # (e.g., FormalPara discussion questions) not bibliography entries
                        in_sidebar = False
                        parent = lst.getparent()
                        while parent is not None:
                            if parent.tag == 'sidebar':
                                in_sidebar = True
                                break
                            if parent == section:
                                break
                            parent = parent.getparent()
                        if in_sidebar:
                            continue
                        # In a bibliography section, convert ALL lists (detection was already done)
                        lists_to_process.append(lst)

                # Convert list items to bibliomixed
                for lst in lists_to_process:
                    lst_parent = lst.getparent()
                    if lst_parent is None:
                        continue

                    for listitem in list(lst.findall('listitem')):
                        bib_entry_counter += 1
                        # Get original listitem ID for mapping (if any)
                        original_listitem_id = listitem.get('id')

                        # Create bibliomixed from listitem content
                        bibliomixed = etree.Element('bibliomixed')
                        new_bib_id = generate_element_id(chapter_id, 'bibliography', sect1_id=section.get('id'))
                        bibliomixed.set('id', new_bib_id)

                        # Register ID mapping from original listitem ID to new bibliomixed ID
                        # This ensures links referencing the original ID can be resolved
                        if original_listitem_id:
                            register_id_mapping(chapter_id, original_listitem_id, new_bib_id)
                            # Update all original source IDs that were mapped to this generated ID
                            # This is critical: the deferred resolution looks up original source IDs,
                            # not the generated IDs. If a source ID was mapped to a listitem's 'x' ID,
                            # we need to update it to point to the new 'bib' ID.
                            update_id_mapping_target(original_listitem_id, new_bib_id)
                            # Also update any existing links that reference the old ID
                            # This is necessary because bibliography conversion happens AFTER link resolution
                            _update_links_to_new_id(root, original_listitem_id, new_bib_id)
                            logger.debug(f"Registered bibliography ID mapping: {original_listitem_id} -> {new_bib_id}")

                        # Extract content from listitem (typically contains para)
                        # C-015 FIX: When listitem has multiple paras (e.g., from ItemNumber +
                        # ItemContent divs), the first para may only contain the number label.
                        # We must find the para with actual content, not just the first one.
                        all_paras = listitem.findall('para')
                        inner_para = all_paras[0] if all_paras else None
                        if inner_para is not None:
                            stripped_text = _strip_bibliography_number_prefix(inner_para.text) if inner_para.text else inner_para.text
                            # If first para is just a number label (empty after stripping prefix)
                            # and has no child elements, try the next para which has actual content
                            if (not (stripped_text and stripped_text.strip()) and
                                    len(inner_para) == 0 and len(all_paras) > 1):
                                inner_para = all_paras[1]
                                stripped_text = _strip_bibliography_number_prefix(inner_para.text) if inner_para.text else inner_para.text
                            bibliomixed.text = stripped_text
                            for child in list(inner_para):
                                validated_append(bibliomixed, child)
                            # Preserve tail from para
                            if inner_para.tail:
                                bibliomixed.tail = inner_para.tail
                        else:
                            # No inner para - copy listitem content directly
                            bibliomixed.text = _strip_bibliography_number_prefix(listitem.text) if listitem.text else listitem.text
                            for child in list(listitem):
                                validated_append(bibliomixed, child)

                        # Sanitize content - remove elements not allowed in bibliomixed
                        _sanitize_for_bibliomixed(bibliomixed)
                        # Parse metadata from bibliography text
                        _parse_bibliography_metadata(bibliomixed)

                        all_bibliomixed.append(bibliomixed)

                    # Remove the list from its parent
                    if lst_parent.tag in ('para', 'simpara'):
                        # Remove the list from para
                        lst_parent.remove(lst)
                        # If para is now empty, remove it
                        grandparent = lst_parent.getparent()
                        if grandparent is not None and len(lst_parent) == 0 and not (lst_parent.text and lst_parent.text.strip()):
                            grandparent.remove(lst_parent)
                    else:
                        lst_parent.remove(lst)

                # Also convert standalone paras that look like bibliography entries
                # (paras starting with numbers/brackets that aren't in lists)
                for para in list(section.findall('para')):
                    para_text = (para.text or '').strip()
                    # Check if para starts with bibliography-like numbering
                    if para_text and re.match(r'^[\[\(]?\d{1,4}[\]\)\.\s]', para_text):
                        bib_entry_counter += 1
                        # Get original para ID for mapping (if any)
                        original_para_id = para.get('id')

                        # Create bibliomixed from para content
                        bibliomixed = etree.Element('bibliomixed')
                        new_bib_id = generate_element_id(chapter_id, 'bibliography', sect1_id=section.get('id'))
                        bibliomixed.set('id', new_bib_id)

                        # Register ID mapping from original para ID to new bibliomixed ID
                        # Also update all existing mappings that point to the old generated ID
                        if original_para_id:
                            register_id_mapping(chapter_id, original_para_id, new_bib_id)
                            update_id_mapping_target(original_para_id, new_bib_id)
                            _update_links_to_new_id(root, original_para_id, new_bib_id)
                            logger.debug(f"Registered bibliography ID mapping: {original_para_id} -> {new_bib_id}")

                        bibliomixed.text = _strip_bibliography_number_prefix(para.text) if para.text else para.text
                        for child in list(para):
                            validated_append(bibliomixed, child)
                        # Sanitize content
                        _sanitize_for_bibliomixed(bibliomixed)
                        # Parse metadata from bibliography text
                        _parse_bibliography_metadata(bibliomixed)
                        all_bibliomixed.append(bibliomixed)
                        # Remove the original para
                        section.remove(para)

                # Wrap all bibliomixed elements in a <bibliography> element
                # DTD requires: sect1 > bibliography > bibliomixed (not sect1 > bibliomixed directly)
                if all_bibliomixed:
                    bibliography_wrapper = validated_subelement(section, 'bibliography')
                    # Generate a unique bibliography ID - don't reuse section ID to avoid collision
                    section_id = section.get('id', '')
                    if section_id:
                        # Create bibliography ID by appending 'bib' suffix to section ID
                        bib_id = section_id + 'bib'
                    else:
                        bib_id = _get_next_section_id_for_bibliography(root, chapter_id)
                    bibliography_wrapper.set('id', bib_id)
                    for bm in all_bibliomixed:
                        validated_append(bibliography_wrapper, bm)
                    converted_count += 1
                    logger.info(f"Converted {len(all_bibliomixed)} bibliography entries in section within {chapter_id}")

                    # DTD COMPLIANCE FIX: If section now only has title and bibliography,
                    # the DTD content model requires at least one block element before
                    # the bibliography.  Instead of removing the sect1 (which makes the
                    # section invisible to RISchunker), add a non-breaking-space spacer
                    # para — the same approach used by _wrap_bibliography_in_chapter().
                    section_parent = section.getparent()
                    if section_parent is not None:
                        # Count non-title, non-bibliography children
                        content_children = [
                            c for c in section
                            if c.tag not in ('title', 'subtitle', 'titleabbrev', 'bibliography',
                                           'sect1info', 'sect2info', 'sect3info', 'sect4info', 'sect5info',
                                           'sectioninfo', 'anchor', 'indexterm')
                        ]
                        if not content_children:
                            # Section has no block content other than title and bibliography.
                            # Add a spacer para before the bibliography to satisfy DTD.
                            bib_index = list(section).index(bibliography_wrapper)
                            spacer = etree.Element('para')
                            spacer.text = '\u00a0'
                            section.insert(bib_index, spacer)
                            logger.info(f"Added spacer para to bibliography section in {chapter_id} to satisfy DTD")

    return converted_count


def _fix_bibliography_placement_in_sections(root: etree.Element) -> int:
    """
    Fix bibliography placement to comply with DTD content model.

    DTD content model for sect1/appendix/chapter sections:
    (sect1info?, title-group, (toc|lot|index|glossary|bibliography)*,
     ((block-content+, sect2*) | sect2+),
     (toc|lot|index|glossary|bibliography)*)

    Bibliography can ONLY appear:
    1. BEFORE any block content (first bibliography* position)
    2. AFTER all sect2/simplesect elements (final bibliography* position)

    This function moves bibliography elements to the end of their parent section
    when they appear between block content and sect2 elements.

    Args:
        root: Root element to process

    Returns:
        Number of bibliography elements repositioned
    """
    fixed_count = 0

    section_tags = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'chapter', 'appendix', 'preface'}
    subsection_tags = {'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect', 'refentry'}
    block_tags = {'glosslist', 'itemizedlist', 'orderedlist', 'caution', 'important',
                 'note', 'tip', 'warning', 'literallayout', 'synopsis', 'formalpara',
                 'para', 'address', 'blockquote', 'graphic', 'mediaobject', 'equation',
                 'figure', 'table', 'sidebar', 'qandaset', 'anchor', 'bridgehead',
                 'highlights', 'authorblurb', 'epigraph', 'abstract', 'indexterm', 'beginpage'}
    nav_tags = {'toc', 'lot', 'index', 'glossary', 'bibliography'}

    for section in root.iter():
        if section.tag not in section_tags:
            continue

        # Find bibliography elements in this section
        bibliographies = [child for child in section if child.tag == 'bibliography']
        if not bibliographies:
            continue

        # Analyze section structure
        children = list(section)
        has_block_before_bib = False
        has_subsection_after_bib = False

        for i, child in enumerate(children):
            if child.tag == 'bibliography':
                # Check what comes before and after
                for j in range(i):
                    if children[j].tag in block_tags:
                        has_block_before_bib = True
                        break
                for j in range(i + 1, len(children)):
                    if children[j].tag in subsection_tags:
                        has_subsection_after_bib = True
                        break

        # If bibliography is between block content and subsections, move it to end
        if has_block_before_bib and has_subsection_after_bib:
            for bib in bibliographies:
                section.remove(bib)
                validated_append(section, bib)
                fixed_count += 1
                logger.debug(f"Moved bibliography to end of {section.tag} {section.get('id', 'no-id')}")

    if fixed_count > 0:
        logger.info(f"Fixed {fixed_count} bibliography placement(s) to comply with DTD content model")

    return fixed_count


def _fix_bibliodiv_content_order(root: etree.Element) -> int:
    """
    Fix bibliodiv content order to comply with DTD content model.

    DTD content model for bibliodiv:
    ((title, subtitle?, titleabbrev?)?,
     (glosslist|itemizedlist|orderedlist|caution|important|note|tip|warning|
      literallayout|synopsis|formalpara|para|address|blockquote|graphic|
      mediaobject|equation|figure|table|sidebar|qandaset|anchor|bridgehead|
      highlights|authorblurb|epigraph|abstract|indexterm|beginpage)*,
     (biblioentry|bibliomixed)+)

    This means:
    1. title/subtitle/titleabbrev FIRST (optional)
    2. Other block elements (figure, table, para, etc.) in the MIDDLE
    3. biblioentry/bibliomixed elements LAST (required, at least one)

    This function reorders bibliodiv children to comply with this model.

    Args:
        root: Root element to process

    Returns:
        Number of bibliodiv elements reordered
    """
    fixed_count = 0

    title_elements = {'title', 'subtitle', 'titleabbrev'}
    bib_entry_elements = {'biblioentry', 'bibliomixed'}

    for bibliodiv in root.iter('bibliodiv'):
        children = list(bibliodiv)
        if not children:
            continue

        # Separate children into three groups
        title_group = []
        middle_group = []
        bibentry_group = []

        for child in children:
            if child.tag in title_elements:
                title_group.append(child)
            elif child.tag in bib_entry_elements:
                bibentry_group.append(child)
            else:
                middle_group.append(child)

        # Check if reordering is needed
        # Build expected order and compare with current
        expected_order = title_group + middle_group + bibentry_group
        current_order = children

        needs_reorder = False
        if len(expected_order) == len(current_order):
            for i, (expected, current) in enumerate(zip(expected_order, current_order)):
                if expected is not current:
                    needs_reorder = True
                    break
        else:
            needs_reorder = True

        if needs_reorder and bibentry_group:
            # Remove all children and re-add in correct order
            for child in children:
                bibliodiv.remove(child)

            for child in title_group:
                validated_append(bibliodiv, child)
            for child in middle_group:
                validated_append(bibliodiv, child)
            for child in bibentry_group:
                validated_append(bibliodiv, child)

            fixed_count += 1
            logger.debug(f"Reordered bibliodiv content: {len(title_group)} title(s), "
                        f"{len(middle_group)} middle element(s), {len(bibentry_group)} bib entries")

    if fixed_count > 0:
        logger.info(f"Fixed {fixed_count} bibliodiv content order(s) to comply with DTD content model")

    return fixed_count


def _fix_nested_bibliodiv(root: etree.Element) -> int:
    """
    Fix nested bibliodiv elements which are not allowed by DTD.

    DTD content model for bibliodiv does NOT allow bibliodiv as a child.
    This function flattens nested bibliodivs by converting inner bibliodivs
    to their parent's siblings (moving them up in the tree).

    Args:
        root: Root element to process

    Returns:
        Number of nested bibliodiv elements fixed
    """
    fixed_count = 0

    # Find all bibliodiv elements that contain other bibliodiv elements
    # Process from deepest to shallowest to handle multiple nesting levels
    while True:
        found_nested = False

        for parent_bibliodiv in root.iter('bibliodiv'):
            nested_bibliodivs = list(parent_bibliodiv.findall('bibliodiv'))
            if not nested_bibliodivs:
                continue

            found_nested = True

            # Get the parent of parent_bibliodiv to insert siblings
            grandparent = None
            for potential_grandparent in root.iter():
                if parent_bibliodiv in potential_grandparent:
                    grandparent = potential_grandparent
                    break

            if grandparent is None:
                # parent_bibliodiv is the root or not found - skip
                continue

            # Get position of parent_bibliodiv in grandparent
            parent_index = list(grandparent).index(parent_bibliodiv)

            # Move each nested bibliodiv to be a sibling after parent_bibliodiv
            insert_position = parent_index + 1
            for nested in nested_bibliodivs:
                parent_bibliodiv.remove(nested)
                grandparent.insert(insert_position, nested)
                insert_position += 1
                fixed_count += 1
                logger.debug(f"Moved nested bibliodiv to be sibling of parent")

            # Break inner loop to restart iteration (tree structure changed)
            break

        if not found_nested:
            break

    if fixed_count > 0:
        logger.info(f"Fixed {fixed_count} nested bibliodiv element(s) to comply with DTD content model")

    return fixed_count


def _fix_block_elements_in_para(root: etree.Element) -> int:
    """
    Fix block elements (like mediaobject) incorrectly nested inside para elements.

    DTD does not allow block elements inside inline containers like <para>.
    This function fixes these nesting violations:
    - If <para> contains ONLY a block element (no text) → unwrap the para
    - If <para> contains text AND block element → move block element outside as sibling

    Args:
        root: Root element to process

    Returns:
        Number of fixes applied
    """
    fixed_count = 0

    # Block elements that are not allowed inside para
    block_elements = {
        'mediaobject', 'figure', 'informalfigure', 'table', 'informaltable',
        'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
        'programlisting', 'screen', 'literallayout', 'synopsis',
        'blockquote', 'note', 'warning', 'caution', 'important', 'tip',
        'example', 'informalexample', 'sidebar', 'address'
    }

    # Process all para elements (iterate over a copy since we modify the tree)
    for para in list(root.iter('para')):
        parent = para.getparent()
        if parent is None:
            continue

        # Find block elements inside this para
        blocks_in_para = [child for child in para if child.tag in block_elements]

        if not blocks_in_para:
            continue

        # Check if para has meaningful text content
        has_text = bool(para.text and para.text.strip())
        has_tail_text = any(child.tail and child.tail.strip() for child in para)

        # Get para's position in parent
        para_index = list(parent).index(para)

        if not has_text and not has_tail_text and len(para) == len(blocks_in_para):
            # Para contains ONLY block element(s), no text → unwrap para
            # Insert block elements in place of para
            for i, block in enumerate(blocks_in_para):
                para.remove(block)
                # Preserve para's tail on the last block element
                if i == len(blocks_in_para) - 1:
                    block.tail = para.tail
                parent.insert(para_index + i, block)

            # Remove the now-empty para
            parent.remove(para)
            fixed_count += 1
            logger.debug(f"Unwrapped para containing only block element(s): {[b.tag for b in blocks_in_para]}")

        else:
            # Para contains text AND block element(s) → move blocks outside as siblings
            # Insert blocks after the para
            insert_pos = para_index + 1
            for block in blocks_in_para:
                # Preserve any tail text from the block as text in para
                if block.tail and block.tail.strip():
                    # Create a new text node or append to existing
                    if len(para) > 0 and para[-1].tail:
                        para[-1].tail = (para[-1].tail or '') + block.tail
                    elif para.text:
                        para.text = para.text + block.tail
                    else:
                        para.text = block.tail
                block.tail = None

                para.remove(block)
                parent.insert(insert_pos, block)
                insert_pos += 1

            fixed_count += 1
            logger.debug(f"Moved block element(s) outside para: {[b.tag for b in blocks_in_para]}")

    return fixed_count


def _convert_to_index_structure(index_elem: etree.Element, chapter_id: str) -> None:
    """
    Convert paragraph/list content in an index element to proper index structure.

    Per spec: Index should use <indexentry>, <primaryie>, <secondaryie> with linkends.
    This function analyzes the content and converts it to proper index structure.

    Args:
        index_elem: The index element to process
        chapter_id: Chapter ID for generating unique IDs
    """
    # Find all content that could be index entries
    # Common patterns:
    # - paragraphs with term and page numbers
    # - definition lists (dl/dt/dd)
    # - unordered lists with terms

    # Track current letter division
    current_letter = None
    current_indexdiv = None
    entry_counter = 0

    def get_first_letter(text: str) -> str:
        """Get uppercase first letter of text for indexdiv grouping."""
        text = text.strip()
        if text:
            return text[0].upper()
        return ''

    def create_indexentry(term: str, subitems: list = None) -> etree.Element:
        """Create an indexentry element with primaryie and optional secondaryie."""
        nonlocal entry_counter
        entry_counter += 1

        indexentry = etree.Element('indexentry')

        # Create primaryie - for now just use term text, no linkends
        # (would need actual anchor IDs from source to add linkends)
        primaryie = validated_subelement(indexentry, 'primaryie')
        primaryie.text = term

        # Add secondary entries if provided
        if subitems:
            for subitem in subitems:
                secondaryie = validated_subelement(indexentry, 'secondaryie')
                secondaryie.text = subitem

        return indexentry

    # Process definition lists (common index format)
    for dl in list(index_elem.iter('variablelist')):
        for varlistentry in list(dl.findall('varlistentry')):
            term_elem = varlistentry.find('term')
            if term_elem is not None:
                term_text = term_elem.text or ''

                # Determine letter for indexdiv
                letter = get_first_letter(term_text)
                if letter and letter != current_letter:
                    current_letter = letter
                    current_indexdiv = etree.Element('indexdiv')
                    div_title = validated_subelement(current_indexdiv, 'title')
                    div_title.text = letter
                    # Insert after title but before content
                    title_elem = index_elem.find('title')
                    if title_elem is not None:
                        title_idx = list(index_elem).index(title_elem)
                        index_elem.insert(title_idx + 1, current_indexdiv)
                    else:
                        index_elem.insert(0, current_indexdiv)

                # Get subitems from listitem
                subitems = []
                listitem = varlistentry.find('listitem')
                if listitem is not None:
                    for para in listitem.findall('para'):
                        if para.text:
                            subitems.append(para.text.strip())

                # Create indexentry
                if current_indexdiv is not None:
                    indexentry = create_indexentry(term_text, subitems if subitems else None)
                    validated_append(current_indexdiv, indexentry)

        # Remove processed variablelist
        parent = dl.getparent()
        if parent is not None:
            parent.remove(dl)

    # Process paragraphs that look like index entries
    # Pattern: "Term, page numbers" or just terms in alphabetical order
    paras_to_remove = []
    for para in list(index_elem.findall('.//para')):
        # Use itertext() to get ALL text content including from child elements
        # (e.g., <para><emphasis>Term</emphasis>, page ref</para>)
        # This fixes the bug where para.text alone missed content in child elements
        text = ''.join(para.itertext()).strip()
        if not text:
            continue

        # Skip if para is inside another structure we'll process
        parent = para.getparent()
        if parent is not None and parent.tag in ('indexentry', 'indexdiv', 'bibliomixed'):
            continue

        letter = get_first_letter(text)
        if letter and letter != current_letter:
            current_letter = letter
            current_indexdiv = etree.Element('indexdiv')
            div_title = validated_subelement(current_indexdiv, 'title')
            div_title.text = letter
            # Insert into index_elem
            title_elem = index_elem.find('title')
            if title_elem is not None:
                title_idx = list(index_elem).index(title_elem)
                # Find position after existing indexdivs
                insert_pos = title_idx + 1
                for i, child in enumerate(index_elem):
                    if child.tag == 'indexdiv':
                        insert_pos = i + 1
                index_elem.insert(insert_pos, current_indexdiv)
            else:
                validated_append(index_elem, current_indexdiv)

        if current_indexdiv is not None and letter:
            # Create indexentry from paragraph content
            indexentry = create_indexentry(text)
            validated_append(current_indexdiv, indexentry)
            paras_to_remove.append(para)

    # Remove converted paragraphs
    for para in paras_to_remove:
        parent = para.getparent()
        if parent is not None:
            parent.remove(para)

    # Clean up empty sections
    for section in list(index_elem.findall('.//section')):
        if len(section) == 0 or (len(section) == 1 and section[0].tag == 'title'):
            parent = section.getparent()
            if parent is not None:
                parent.remove(section)

    # CRITICAL: Remove sect1 and other elements NOT allowed in index per DTD
    # DTD allows: indexinfo?, (title, subtitle?, titleabbrev?)?,
    #             (divcomponent.mix)*, (indexdiv+ | indexentry*)
    # sect1, sect2, etc. are NOT allowed as direct children
    invalid_index_children = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
                               'section', 'simplesect', 'chapter', 'appendix',
                               'preface', 'refentry'}

    for invalid_elem in list(index_elem):
        if invalid_elem.tag in invalid_index_children:
            # Try to extract content and convert to indexdiv entries if possible
            title_elem = invalid_elem.find('title')
            if title_elem is not None and title_elem.text:
                letter = get_first_letter(title_elem.text)
                if letter:
                    # Find or create indexdiv for this letter
                    existing_div = None
                    for div in index_elem.findall('indexdiv'):
                        div_title = div.find('title')
                        if div_title is not None and div_title.text == letter:
                            existing_div = div
                            break

                    if existing_div is None:
                        existing_div = etree.Element('indexdiv')
                        div_title = validated_subelement(existing_div, 'title')
                        div_title.text = letter
                        # Insert before the invalid element's position
                        idx = list(index_elem).index(invalid_elem)
                        index_elem.insert(idx, existing_div)

                    # Move content from sect1 to indexdiv as indexentries
                    for child in list(invalid_elem):
                        if child.tag == 'title':
                            continue  # Skip title, already used for letter
                        if child.tag == 'para':
                            text = ''.join(child.itertext()).strip()
                            if text:
                                ie = create_indexentry(text)
                                validated_append(existing_div, ie)
                        elif child.tag in ('itemizedlist', 'orderedlist'):
                            # Convert list items to index entries
                            for item in child.findall('.//listitem'):
                                text = ''.join(item.itertext()).strip()
                                if text:
                                    ie = create_indexentry(text)
                                    validated_append(existing_div, ie)

            # Remove the invalid element
            index_elem.remove(invalid_elem)
            logger.debug(f"Removed invalid <{invalid_elem.tag}> from index element")

    # Also remove any anchors that are direct children (anchors should be inside indexdiv)
    for anchor in list(index_elem.findall('anchor')):
        if anchor.getparent() == index_elem:
            index_elem.remove(anchor)
            logger.debug(f"Removed stray <anchor> from index root")

    # CRITICAL: Convert direct child itemizedlist/orderedlist to indexentries
    # These lists contain the actual index content but were being preserved as component_mix
    # which resulted in empty listitems and orphaned links with invalid linkend IDs
    lists_to_remove = []
    for list_elem in list(index_elem):
        if list_elem.tag in ('itemizedlist', 'orderedlist'):
            # Extract content from each listitem and create indexentry
            for item in list_elem.findall('.//listitem'):
                # Get all text content (including from link/emphasis children)
                text = ''.join(item.itertext()).strip()
                if text:
                    # Determine letter for indexdiv grouping
                    letter = get_first_letter(text)
                    if letter and letter != current_letter:
                        current_letter = letter
                        current_indexdiv = etree.Element('indexdiv')
                        div_title = validated_subelement(current_indexdiv, 'title')
                        div_title.text = letter
                        # Find position after existing indexdivs
                        title_elem = index_elem.find('title')
                        if title_elem is not None:
                            insert_pos = list(index_elem).index(title_elem) + 1
                            for i, child in enumerate(index_elem):
                                if child.tag == 'indexdiv':
                                    insert_pos = i + 1
                            index_elem.insert(insert_pos, current_indexdiv)
                        else:
                            validated_append(index_elem, current_indexdiv)

                    if current_indexdiv is not None and letter:
                        indexentry = create_indexentry(text)
                        validated_append(current_indexdiv, indexentry)
                        entry_counter += 1

            lists_to_remove.append(list_elem)

    # Remove the converted lists
    for list_elem in lists_to_remove:
        index_elem.remove(list_elem)
        logger.debug(f"Converted and removed <{list_elem.tag}> from index")

    # CRITICAL: Remove invalid linkend attributes from any remaining links in index
    # Links that reference non-existent IDs cause DTD validation errors
    # Convert such links to plain text (phrase elements) or just preserve their text
    for link_elem in list(index_elem.iter('link')):
        linkend = link_elem.get('linkend', '')
        # Check for placeholder or potentially invalid linkend
        if linkend == '__deferred__' or not linkend:
            # Convert link to phrase preserving text content
            parent = link_elem.getparent()
            if parent is not None:
                text_content = ''.join(link_elem.itertext())
                phrase = etree.Element('phrase')
                phrase.text = text_content
                phrase.tail = link_elem.tail
                # Find position and replace
                idx = list(parent).index(link_elem)
                parent.remove(link_elem)
                parent.insert(idx, phrase)
                logger.debug(f"Converted invalid link to phrase in index")

    # Remove any xref elements with invalid linkend (xref must have valid target)
    for xref_elem in list(index_elem.iter('xref')):
        linkend = xref_elem.get('linkend', '')
        if linkend == '__deferred__' or not linkend:
            parent = xref_elem.getparent()
            if parent is not None:
                # xref has no text content, just remove and preserve tail
                tail = xref_elem.tail or ''
                idx = list(parent).index(xref_elem)
                parent.remove(xref_elem)
                if idx > 0:
                    parent[idx-1].tail = (parent[idx-1].tail or '') + tail
                else:
                    parent.text = (parent.text or '') + tail
                logger.debug(f"Removed invalid xref from index")

    # Clean up any empty listitems, paras, or other empty elements that may remain
    for listitem in list(index_elem.iter('listitem')):
        # Check if listitem has any real content
        has_content = False
        for child in listitem:
            if child.tag == 'para':
                para_text = ''.join(child.itertext()).strip()
                if para_text:
                    has_content = True
                    break
            elif len(child) > 0 or (child.text and child.text.strip()):
                has_content = True
                break
        if not has_content:
            parent = listitem.getparent()
            if parent is not None:
                parent.remove(listitem)
                logger.debug(f"Removed empty listitem from index")

    # Remove any now-empty lists
    for list_tag in ('itemizedlist', 'orderedlist'):
        for list_elem in list(index_elem.iter(list_tag)):
            if len(list_elem.findall('listitem')) == 0:
                parent = list_elem.getparent()
                if parent is not None:
                    parent.remove(list_elem)
                    logger.debug(f"Removed empty {list_tag} from index")

    # CRITICAL: Reorder elements to comply with DTD content model
    # DTD requires: indexinfo?, (title, subtitle?, titleabbrev?)?, (component.mix)*, (indexdiv* | indexentry*)
    # Elements MUST be in this exact order

    # Collect all children by category
    indexinfo_elem = None
    title_elems = []  # title, subtitle, titleabbrev
    component_mix_elems = []  # para, figure, etc.
    indexdiv_elems = []
    indexentry_elems = []

    # component.mix elements allowed in index (from DTD)
    index_component_mix_tags = {
        'glosslist', 'itemizedlist', 'orderedlist', 'caution', 'important',
        'note', 'tip', 'warning', 'literallayout', 'synopsis', 'formalpara',
        'para', 'address', 'blockquote', 'graphic', 'mediaobject', 'equation',
        'figure', 'table', 'sidebar', 'qandaset', 'anchor', 'bridgehead',
        'highlights', 'authorblurb', 'epigraph', 'abstract', 'indexterm'
    }

    for child in list(index_elem):
        if child.tag == 'indexinfo':
            indexinfo_elem = child
        elif child.tag in ('title', 'subtitle', 'titleabbrev'):
            title_elems.append(child)
        elif child.tag == 'indexdiv':
            indexdiv_elems.append(child)
        elif child.tag == 'indexentry':
            indexentry_elems.append(child)
        elif child.tag in index_component_mix_tags:
            component_mix_elems.append(child)
        else:
            # Unknown element - treat as component.mix if safe, otherwise remove
            logger.warning(f"Unexpected element <{child.tag}> in index, removing")
            index_elem.remove(child)
            continue
        # Remove from current position (will re-add in correct order)
        index_elem.remove(child)

    # Re-add elements in correct DTD order
    insert_pos = 0

    # 1. indexinfo first
    if indexinfo_elem is not None:
        index_elem.insert(insert_pos, indexinfo_elem)
        insert_pos += 1

    # 2. title, subtitle, titleabbrev (in that order)
    title_order = {'title': 0, 'subtitle': 1, 'titleabbrev': 2}
    title_elems.sort(key=lambda e: title_order.get(e.tag, 99))
    for elem in title_elems:
        index_elem.insert(insert_pos, elem)
        insert_pos += 1

    # 3. component.mix elements (para, figure, etc.)
    for elem in component_mix_elems:
        index_elem.insert(insert_pos, elem)
        insert_pos += 1

    # 4. indexdiv OR indexentry (not both - prefer indexdiv if both exist)
    if indexdiv_elems:
        for elem in indexdiv_elems:
            validated_append(index_elem, elem)
        if indexentry_elems:
            # Move indexentry elements into appropriate indexdiv
            logger.warning(f"Index has both indexdiv and indexentry - moving entries to divs")
            for entry in indexentry_elems:
                # Find appropriate div based on first letter
                primary = entry.find('primaryie')
                if primary is not None and primary.text:
                    letter = primary.text.strip()[0].upper() if primary.text.strip() else 'A'
                    target_div = None
                    for div in indexdiv_elems:
                        div_title = div.find('title')
                        if div_title is not None and div_title.text == letter:
                            target_div = div
                            break
                    if target_div is not None:
                        target_div.append(entry)
                    else:
                        # Add to first div as fallback
                        indexdiv_elems[0].append(entry)
                else:
                    indexdiv_elems[0].append(entry)
    else:
        for elem in indexentry_elems:
            validated_append(index_elem, elem)

    logger.debug(f"Converted index with {entry_counter} entries in {chapter_id}")


def _convert_to_glossary_structure(glossary_elem: etree.Element, chapter_id: str) -> None:
    """
    Convert variablelist content in a glossary element to proper glossentry structure.

    This function converts:
    - <variablelist>/<varlistentry> to <glosslist>/<glossentry>
    - Ensures each glossentry has a proper ID with 'gl' prefix for XSL recognition

    Per DTD: glossary content model is:
    (glossaryinfo?, (title, subtitle?, titleabbrev?)?,
     (calloutlist|glosslist|itemizedlist|orderedlist|segmentedlist|simplelist|
      variablelist|caution|important|note|tip|warning|literallayout|programlisting|
      programlistingco|screen|screenco|screenshot|synopsis|cmdsynopsis|funcsynopsis|
      classsynopsis|fieldsynopsis|constructorsynopsis|destructorsynopsis|methodsynopsis|
      formalpara|para|simpara|address|blockquote|graphic|graphicco|mediaobject|
      mediaobjectco|informalequation|informalexample|informalfigure|informaltable|
      equation|example|figure|table|msgset|procedure|sidebar|qandaset|anchor|
      bridgehead|remark|highlights|abstract|authorblurb|epigraph|indexterm|beginpage)*,
     (bibliodiv+ | glossdiv+ | glossentry+)?)

    Args:
        glossary_elem: The glossary element to process
        chapter_id: Chapter ID for generating unique IDs
    """
    glossentry_counter = 0

    # Find all variablelist elements to convert to glosslist
    for variablelist in list(glossary_elem.iter('variablelist')):
        parent = variablelist.getparent()
        if parent is None:
            continue

        # Create glossdiv to hold the entries (or use glosslist)
        # For proper cross-referencing, we use glossentry directly under glossary
        # Create a temporary list to hold new glossentries
        new_entries = []

        # Process varlistentry elements
        for varlistentry in variablelist.findall('varlistentry'):
            glossentry_counter += 1

            # Create glossentry with proper ID
            glossentry = etree.Element('glossentry')
            new_gl_id = generate_element_id(chapter_id, 'glossentry')
            glossentry.set('id', new_gl_id)

            # Get original ID for mapping
            original_id = varlistentry.get('id')
            if original_id:
                register_id_mapping(chapter_id, original_id, new_gl_id, 'glossentry')
                logger.debug(f"Registered glossentry ID mapping: {original_id} -> {new_gl_id}")

            # Register the generated XML ID with the tracker
            register_xml_id(new_gl_id, 'glossentry', chapter_id, original_id)

            # Create glossterm from the term element
            term = varlistentry.find('term')
            glossterm = validated_subelement(glossentry, 'glossterm')
            if term is not None:
                glossterm.text = ''.join(term.itertext()).strip()
                # Also register term ID if it has one
                term_id = term.get('id')
                if term_id:
                    register_id_mapping(chapter_id, term_id, new_gl_id, 'glossterm')
            else:
                glossterm.text = ""

            # Create glossdef from listitem content
            listitem = varlistentry.find('listitem')
            if listitem is not None:
                glossdef = validated_subelement(glossentry, 'glossdef')
                # Move all content from listitem to glossdef
                for child in list(listitem):
                    glossdef.append(child)
                # If no children but has text in para, handle that
                if len(glossdef) == 0:
                    para = validated_subelement(glossdef, 'para')
                    para.text = ''.join(listitem.itertext()).strip() or ""

            new_entries.append(glossentry)

        # Replace variablelist with glossentries at the same position
        idx = list(parent).index(variablelist)
        parent.remove(variablelist)

        # Insert glossentries at the position
        for i, entry in enumerate(new_entries):
            parent.insert(idx + i, entry)

    # Also convert any definition lists that might be using different structure
    # Handle dl elements that were converted to variablelist
    for varlist in list(glossary_elem.iter('variablelist')):
        # This shouldn't happen after the above processing, but just in case
        pass

    # Ensure any existing glossentry elements have IDs
    for glossentry in glossary_elem.iter('glossentry'):
        if not glossentry.get('id'):
            glossentry_counter += 1
            new_gl_id = generate_element_id(chapter_id, 'glossentry')
            glossentry.set('id', new_gl_id)
            register_xml_id(new_gl_id, 'glossentry', chapter_id)
            logger.debug(f"Added missing ID to glossentry: {new_gl_id}")

    logger.debug(f"Converted glossary with {glossentry_counter} entries in {chapter_id}")


# Elements that satisfy DTD content requirements for sections (divcomponent.mix)
# These are the elements that count as "real content" - not just metadata/anchors
VALID_SECTION_CONTENT_TAGS = {
    # From %para.class;
    'para', 'simpara', 'formalpara',
    # From %list.class;
    'itemizedlist', 'orderedlist', 'variablelist', 'segmentedlist', 'simplelist',
    'calloutlist', 'bibliolist', 'glosslist', 'qandaset', 'procedure',
    # From %admon.class;
    'note', 'tip', 'warning', 'caution', 'important',
    # From %linespecific.class;
    'literallayout', 'programlisting', 'programlistingco', 'screen', 'screenco',
    'screenshot', 'address',
    # From %synop.class;
    'synopsis', 'cmdsynopsis', 'funcsynopsis', 'classsynopsis', 'fieldsynopsis',
    'constructorsynopsis', 'destructorsynopsis', 'methodsynopsis',
    # From %informal.class;
    'informalequation', 'informalexample', 'informalfigure', 'informaltable',
    'graphic', 'mediaobject', 'graphicco', 'mediaobjectco',
    # From %formal.class;
    'equation', 'example', 'figure', 'table',
    # From %compound.class;
    'blockquote', 'epigraph', 'msgset', 'sidebar',
    # From %genobj.class;
    'anchor',  # Note: anchor alone doesn't satisfy the requirement, but in combination it's ok
    'bridgehead', 'remark', 'highlights',
    # From %descobj.class;
    'abstract', 'authorblurb', 'epigraph',
    # Nested sections also satisfy the requirement
    'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect',
    'refentry', 'refsect1', 'refsect2', 'refsect3',
}

# Elements that DON'T count as content for section requirements
# These are allowed via %ubiq.inclusion; but don't satisfy divcomponent.mix
NON_CONTENT_TAGS = {
    'title', 'titleabbrev', 'subtitle',  # Title elements
    'anchor',  # Anchors alone don't satisfy content requirement
    'indexterm',  # Index terms
    'sect1info', 'sect2info', 'sect3info', 'sect4info', 'sect5info', 'sectioninfo',  # Info elements
}


def ensure_section_has_content(section: etree.Element) -> None:
    """
    Ensure a section has at least one valid content element beyond its title.
    Adds an empty para if needed for DTD compliance.

    Per DTD, sections require at least one element from divcomponent.mix (para, figure,
    table, list, etc.) or a nested section (sect2, simplesect, etc.).
    Elements like 'anchor' and 'indexterm' are allowed but don't satisfy this requirement.

    Args:
        section: The section element to check
    """
    # Check for valid content elements (not just any child)
    # Valid content = elements from divcomponent.mix or nested sections
    # Invalid = title, anchor, indexterm, processing instructions, etc.
    has_valid_content = False

    for child in section:
        tag = child.tag
        # Skip title and info elements
        if tag in ('title', 'titleabbrev', 'subtitle') or tag.endswith('info'):
            continue
        # Skip elements that don't satisfy content requirement
        if tag in ('anchor', 'indexterm'):
            continue
        # Any other element is considered valid content
        has_valid_content = True
        break

    if not has_valid_content:
        # Section has no valid content - add empty para for DTD compliance
        # Insert after title and any anchors/indexterms
        insert_pos = 0
        for i, child in enumerate(section):
            if child.tag in ('title', 'titleabbrev', 'subtitle', 'anchor', 'indexterm') or child.tag.endswith('info'):
                insert_pos = i + 1
            else:
                break
        para = etree.Element('para')
        section.insert(insert_pos, para)
        logger.debug(f"Added empty para to section with only title/anchors: {section.get('id', 'unknown')}")


def ensure_all_sections_have_content(root_elem: etree.Element) -> int:
    """
    Final pass to ensure ALL section elements have valid content.

    This catches sections that may have been created without going through
    the normal ensure_section_has_content path.

    Args:
        root_elem: Root element of the XML tree

    Returns:
        Number of sections that were fixed
    """
    fixed_count = 0
    section_tags = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect'}

    for section in root_elem.iter():
        if section.tag in section_tags:
            # Check if section has valid content
            has_valid_content = False
            for child in section:
                tag = child.tag
                if tag in ('title', 'titleabbrev', 'subtitle', 'anchor', 'indexterm') or tag.endswith('info'):
                    continue
                has_valid_content = True
                break

            if not has_valid_content:
                # Add para after title/anchors
                insert_pos = 0
                for i, child in enumerate(section):
                    if child.tag in ('title', 'titleabbrev', 'subtitle', 'anchor', 'indexterm') or child.tag.endswith('info'):
                        insert_pos = i + 1
                    else:
                        break
                para = etree.Element('para')
                section.insert(insert_pos, para)
                fixed_count += 1
                logger.debug(f"Final pass: Added para to section {section.get('id', 'unknown')}")

    if fixed_count > 0:
        logger.info(f"Final pass: Fixed {fixed_count} sections missing valid content")

    return fixed_count


def _is_para_div(css_class: str) -> bool:
    """
    Check if a div's CSS class indicates it should be treated as a paragraph.

    Common in Springer and other publisher EPUBs where <div class="Para"> is used
    instead of <p class="Para">. These divs contain inline content (text, formatting,
    inline equations, index terms) and should be converted to <para> elements.

    Uses publisher configuration first, then falls back to hardcoded patterns.

    Args:
        css_class: The CSS class string from the element

    Returns:
        True if this div should become a para element
    """
    if not css_class:
        return False

    # Normalize to list of classes
    if isinstance(css_class, list):
        class_list = [c.lower() for c in css_class]
    else:
        class_list = css_class.lower().split()

    # Check publisher configuration first
    for cls in class_list:
        mapping = get_css_mapping(cls)
        if mapping and mapping.element == 'para':
            return True

    # Fall back to hardcoded paragraph-like class names
    para_classes = {
        'para', 'simplepara', 'simple-para', 'textpara', 'text-para',
        'bodypara', 'body-para', 'contentpara', 'content-para',
    }

    # Check for exact match with any para class
    return any(cls in para_classes for cls in class_list)


def _is_sidebar_div(css_class: str) -> bool:
    """
    Check if a div's CSS class indicates it should be converted to a sidebar.

    Matches classes commonly used for:
    - Exercises and questions
    - Boxes and callouts
    - Tips, notes, warnings
    - Sidebars and asides

    Uses publisher configuration first, then falls back to hardcoded patterns.

    Args:
        css_class: The CSS class string from the element

    Returns:
        True if this div should become a sidebar
    """
    if not css_class:
        return False

    # Normalize to list of classes
    if isinstance(css_class, list):
        class_list = css_class
    else:
        class_list = css_class.split()

    # Check publisher configuration first
    sidebar_elements = {'sidebar', 'note', 'tip', 'warning', 'caution', 'important', 'example'}
    for cls in class_list:
        mapping = get_css_mapping(cls)
        if mapping and mapping.element in sidebar_elements:
            return True

    css_class_lower = css_class.lower() if isinstance(css_class, str) else ' '.join(css_class).lower()

    # Fall back to hardcoded class patterns that indicate sidebar/box content
    sidebar_classes = [
        # Exercise/Question types
        'exercise', 'question', 'activity', 'worksheet', 'practice',
        # Box/Callout types
        'box', 'callout', 'infobox', 'textbox', 'highlight',
        # Sidebar types
        'sidebar', 'aside', 'marginal', 'margin-note',
        # Tip/Note/Warning types
        'tip', 'note', 'warning', 'caution', 'important', 'alert',
        # Case study/Example types
        'case-study', 'casestudy', 'example', 'scenario',
        # Feature/Summary types
        'feature', 'summary', 'keypoint', 'key-point', 'takeaway',
        # Learning objective types
        'objective', 'learning-objective', 'outcome',
        # Springer FormalPara / ParaType patterns (publisher-agnostic fallback)
        'formalpara', 'formal-para', 'paratype', 'para-type',
    ]

    # Check if any sidebar class is present
    for sidebar_class in sidebar_classes:
        # Match as a word boundary to avoid partial matches
        if re.search(r'\b' + re.escape(sidebar_class) + r'\b', css_class_lower):
            return True

    return False


def is_toc_section(elem: Tag) -> bool:
    """
    Detect if an HTML element is a TOC (Table of Contents) section.

    Checks for:
    - id="toc"
    - epub:type="toc"
    - class containing "toc" (but not toc1, toc2, etc. which are entry levels)

    Args:
        elem: BeautifulSoup Tag element

    Returns:
        True if this is a TOC section
    """
    # Check id attribute
    elem_id = elem.get('id', '')
    if elem_id == 'toc':
        return True

    # Check epub:type attribute (used in EPUB3)
    epub_type = elem.get('epub:type', '') or elem.get('data-type', '')
    if 'toc' in epub_type:
        return True

    # Check class - but be careful not to match toc1, toc2, etc.
    css_class = elem.get('class', '')
    if isinstance(css_class, list):
        css_class = ' '.join(css_class)
    # Match "toc" as a standalone class but not "toc1", "toc2", etc.
    if re.search(r'\btoc\b(?!\d)', css_class):
        return True

    return False


def is_empty_span_marker(elem: Tag) -> bool:
    """
    Check if element is an empty span with an ID (a marker element).

    These are commonly used in EPUBs to mark section start points for cross-referencing.
    Example: <span id="c1-sec-0001"/>

    Args:
        elem: BeautifulSoup Tag element

    Returns:
        True if this is an empty span with an ID
    """
    if elem.name != 'span' or not elem.get('id'):
        return False

    # Check if empty (no text content, no meaningful child elements)
    for child in elem.children:
        if isinstance(child, NavigableString) and str(child).strip():
            return False
        if isinstance(child, Tag):
            # Allow nested empty spans but nothing else
            if child.name != 'span' or not is_empty_span_marker(child):
                return False

    return True


def is_followed_by_heading(elem: Tag) -> bool:
    """
    Check if element is immediately followed by a heading (h1-h6).

    Whitespace between the element and heading is allowed.

    Args:
        elem: BeautifulSoup Tag element

    Returns:
        True if the next meaningful sibling is a heading element
    """
    next_sibling = elem.next_sibling
    while next_sibling:
        if isinstance(next_sibling, NavigableString):
            if not str(next_sibling).strip():
                next_sibling = next_sibling.next_sibling
                continue
            return False  # Non-whitespace text before heading
        if isinstance(next_sibling, Tag):
            return next_sibling.name in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']
        next_sibling = next_sibling.next_sibling
    return False


def is_section_start_span_marker(elem: Tag) -> bool:
    """
    Check if this span marker is at the start of a section with a heading following.

    This handles cases like:
        <section>
            <span id="c1-sec-0297"/>           <!-- This span -->
            <span epub:type="pagebreak"/>      <!-- Pagebreak between span and heading -->
            <h4>SECTION TITLE</h4>
        </section>

    These section marker spans are cross-reference targets in the source EPUB. Since R2 XSL
    has the anchor template output commented out (html.xsl), we don't create anchors for
    these IDs - they would produce no HTML output anyway.

    Args:
        elem: BeautifulSoup Tag element (should be a span)

    Returns:
        True if this is an empty span marker at the start of a section with a heading
    """
    if not is_empty_span_marker(elem):
        return False

    # Check if parent is a section element
    parent = elem.parent
    if not parent or parent.name not in ['section', 'article', 'div']:
        return False

    # Check if this span is at the START of the section
    # (only whitespace or other empty span markers before it)
    for sibling in parent.children:
        if sibling is elem:
            break  # Reached our element - it's at the start
        if isinstance(sibling, NavigableString):
            if str(sibling).strip():
                return False  # Non-whitespace content before this span
        elif isinstance(sibling, Tag):
            if not is_empty_span_marker(sibling):
                return False  # Non-span-marker element before this span

    # Check if there's a heading somewhere in this section that will pick up the ID
    # Look for heading among siblings (after this span)
    heading_found = False
    next_sib = elem.next_sibling
    while next_sib:
        if isinstance(next_sib, Tag):
            if next_sib.name in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
                heading_found = True
                break
            # Stop if we hit block content that's not a span marker or pagebreak
            if next_sib.name not in ['span']:
                break
        next_sib = next_sib.next_sibling

    return heading_found


# Note: The following functions were removed because R2 XSL has the anchor
# template output commented out (html.xsl), so preserving these IDs as anchors
# would produce no HTML output:
# - get_preceding_span_marker_ids()
# - get_nested_anchor_ids()
# - get_nested_pagebreak_ids()


def is_inside_table_cell(parent_elem) -> Tuple[bool, any]:
    """
    Check if the given element is inside a table cell (entry).

    This is used to determine if we should create a <figure> element or just
    a <mediaobject> directly, since DTD doesn't allow <figure> inside <entry>.

    Args:
        parent_elem: lxml etree Element to check

    Returns:
        Tuple of (is_in_cell, entry_element_or_None)
    """
    check_elem = parent_elem
    while check_elem is not None:
        if check_elem.tag == 'entry':
            return (True, check_elem)
        check_elem = check_elem.getparent() if hasattr(check_elem, 'getparent') else None
    return (False, None)


def get_toc_entry_level(css_class: str) -> Tuple[str, int]:
    """
    Get the TOC entry type and level from a CSS class.

    CSS class mapping:
    - tocpt → ('part', 0)
    - toc   → ('chapter', 0)
    - toc1  → ('level', 1)
    - toc2  → ('level', 2)
    - toc3  → ('level', 3)
    - toc4  → ('level', 4)
    - toc5  → ('level', 5)
    - tocau → ('author', -1)  # special case - stays with parent
    - tocfront → ('front', -2)
    - tocback → ('back', -3)

    Args:
        css_class: CSS class string

    Returns:
        Tuple of (entry_type, level_number)
    """
    if not css_class:
        return ('unknown', -1)

    # Check for specific classes
    if 'tocpt' in css_class:
        return ('part', 0)
    if 'tocau' in css_class:
        return ('author', -1)
    if 'tocfront' in css_class:
        return ('front', -2)
    if 'tocback' in css_class:
        return ('back', -3)

    # Check for numbered toc levels (toc1, toc2, toc3, etc.)
    match = re.search(r'\btoc(\d)\b', css_class)
    if match:
        level = int(match.group(1))
        return ('level', level)

    # Check for plain "toc" class (chapter level)
    if re.search(r'\btoc\b(?!\d)', css_class):
        return ('chapter', 0)

    return ('unknown', -1)


def extract_toc_entry_content(elem: Tag, doc_path: str, chapter_id: str,
                              mapper: ReferenceMapper) -> etree.Element:
    """
    Extract content from a TOC paragraph and create a tocentry element.

    Preserves links (<a> tags) as <link linkend="..."> elements for internal cross-references.

    Args:
        elem: The paragraph element
        doc_path: Document path for reference resolution
        chapter_id: Chapter ID
        mapper: Reference mapper

    Returns:
        A tocentry element
    """
    tocentry = etree.Element('tocentry')

    # Find link in the paragraph
    link = elem.find('a')
    if link:
        href = link.get('href', '')
        link_text = extract_text(link)

        # Resolve the href to a proper reference
        # Use <link linkend="..."> for internal cross-references (not ulink/url)
        if href:
            # Create link element for internal TOC cross-references
            link_elem = validated_subelement(tocentry, 'link')

            # Resolve cross-reference using deferred resolution (two-pass processing)
            # TOC links may reference elements that haven't been processed yet
            if href.startswith('#'):
                # Internal anchor - reference within same document
                anchor_id = href[1:]
                # Use deferred resolution to avoid polluting ID mapping
                link_elem.set('linkend', '__deferred__')
                mark_link_for_deferred_resolution(link_elem, anchor_id, chapter_id, chapter_id)
            elif '#' in href:
                # Reference to anchor in another file
                file_part, anchor = href.split('#', 1)
                # Normalize the file reference
                ref_chapter = mapper.get_chapter_id(file_part)
                if ref_chapter:
                    # Use deferred resolution
                    link_elem.set('linkend', '__deferred__')
                    mark_link_for_deferred_resolution(link_elem, anchor, ref_chapter, chapter_id)
                else:
                    # Could not resolve - also use deferred resolution with current chapter as fallback
                    # This avoids setting invalid linkend values that cause IDREF validation errors
                    link_elem.set('linkend', '__deferred__')
                    mark_link_for_deferred_resolution(link_elem, anchor, chapter_id, chapter_id)
            else:
                # Reference to another file (no anchor) - use chapter ID as linkend
                ref_chapter = mapper.get_chapter_id(href)
                if ref_chapter:
                    link_elem.set('linkend', ref_chapter)
                else:
                    # Could not resolve - skip linkend
                    pass

            link_elem.text = link_text
    else:
        # No link - just use text content
        tocentry.text = extract_text(elem)

    return tocentry


def convert_toc_section_to_docbook(section_elem: Tag, parent_elem: etree.Element,
                                   doc_path: str, chapter_id: str,
                                   mapper: ReferenceMapper) -> None:
    """
    Convert a flat HTML TOC section to a properly nested DocBook <toc> element.

    Handles the hierarchical structure:
    - tocpt (part) → <tocpart>
    - toc (chapter) → <tocchap>
    - toc1 → <toclevel1>
    - toc2 → <toclevel2>
    - toc3 → <toclevel3>
    - toc4 → <toclevel4>
    - toc5 → <toclevel5>

    Args:
        section_elem: The HTML section containing TOC paragraphs
        parent_elem: Parent DocBook element to append to
        doc_path: Document path
        chapter_id: Chapter ID
        mapper: Reference mapper
    """
    # Create the toc element
    toc = validated_subelement(parent_elem, 'toc')

    # Look for title (h1, h2, etc.)
    title_elem = section_elem.find(['h1', 'h2', 'h3', 'h4', 'h5', 'h6'])
    if title_elem:
        title = validated_subelement(toc, 'title')
        title.text = extract_text(title_elem)

    # Collect all TOC paragraphs
    toc_paragraphs = []
    for p in section_elem.find_all('p'):
        css_class = p.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        entry_type, level = get_toc_entry_level(css_class)
        if entry_type != 'unknown':
            toc_paragraphs.append((p, entry_type, level, css_class))

    if not toc_paragraphs:
        logger.debug(f"No TOC paragraphs found in section for {chapter_id}")
        return

    # Build nested structure using a stack
    # Stack contains: (element, entry_type, level)
    # entry_type: 'part', 'chapter', 'level'
    # level: 0 for part/chapter, 1-5 for toclevel1-5
    stack = [(toc, 'toc', -1)]  # Root element

    current_part = None
    current_chapter = None

    for p, entry_type, level, css_class in toc_paragraphs:
        tocentry = extract_toc_entry_content(p, doc_path, chapter_id, mapper)

        if entry_type == 'part':
            # Part: top-level container
            # Close any open chapters first
            current_chapter = None
            # Create tocpart
            current_part = validated_subelement(toc, 'tocpart')
            current_part.append(tocentry)
            # Reset stack to just toc and part
            stack = [(toc, 'toc', -1), (current_part, 'part', 0)]

        elif entry_type == 'chapter':
            # Chapter: can be inside a part or directly in toc
            # Create tocchap
            if current_part is not None:
                current_chapter = validated_subelement(current_part, 'tocchap')
            else:
                current_chapter = validated_subelement(toc, 'tocchap')
            validated_append(current_chapter, tocentry)
            # Reset stack to appropriate level
            if current_part is not None:
                stack = [(toc, 'toc', -1), (current_part, 'part', 0), (current_chapter, 'chapter', 0)]
            else:
                stack = [(toc, 'toc', -1), (current_chapter, 'chapter', 0)]

        elif entry_type == 'level' and level >= 1:
            # Subsection level (toc1, toc2, etc.)
            if current_chapter is None:
                # No chapter yet - create one implicitly
                if current_part is not None:
                    current_chapter = validated_subelement(current_part, 'tocchap')
                else:
                    current_chapter = validated_subelement(toc, 'tocchap')
                # Add empty tocentry for implicit chapter
                implicit_entry = validated_subelement(current_chapter, 'tocentry')
                implicit_entry.text = ""
                if current_part is not None:
                    stack = [(toc, 'toc', -1), (current_part, 'part', 0), (current_chapter, 'chapter', 0)]
                else:
                    stack = [(toc, 'toc', -1), (current_chapter, 'chapter', 0)]

            # Find the right parent level
            # toc1 goes into chapter, toc2 goes into toc1, etc.
            target_parent_level = level - 1  # e.g., toc2 (level 2) needs parent at level 1

            # Pop stack until we find appropriate parent
            while len(stack) > 1:
                _, parent_type, parent_level = stack[-1]
                if parent_type == 'chapter' and level == 1:
                    # toc1 goes directly into chapter
                    break
                elif parent_type == 'level' and parent_level == target_parent_level:
                    # Found matching parent level
                    break
                elif parent_type == 'level' and parent_level < level:
                    # Parent level is shallower - this is correct
                    break
                elif parent_type in ('toc', 'part'):
                    # Don't pop beyond these
                    break
                else:
                    stack.pop()

            # Create the toclevel element
            parent = stack[-1][0]
            level_elem_name = f'toclevel{level}'
            level_elem = validated_subelement(parent, level_elem_name)
            validated_append(level_elem, tocentry)
            stack.append((level_elem, 'level', level))

        elif entry_type == 'author':
            # Author line - append to the last tocentry's parent
            if len(stack) > 1:
                parent = stack[-1][0]
                # Add as another tocentry in the same container
                author_entry = validated_subelement(parent, 'tocentry')
                author_entry.set('role', 'author')
                author_entry.text = extract_text(p)

        elif entry_type == 'front':
            # Front matter entry
            tocfront = validated_subelement(toc, 'tocfront')
            tocfront.append(tocentry)

        elif entry_type == 'back':
            # Back matter entry
            tocback = validated_subelement(toc, 'tocback')
            tocback.append(tocentry)

    logger.info(f"Converted TOC section with {len(toc_paragraphs)} entries to nested structure")


# Global counters for element ID generation per chapter
# These are reset for each chapter conversion
_element_counters: Dict[str, Dict[str, int]] = {}


def get_element_counter(chapter_id: str, element_type: str) -> int:
    """Get next counter value for an element type within a chapter."""
    if chapter_id not in _element_counters:
        _element_counters[chapter_id] = {}
    if element_type not in _element_counters[chapter_id]:
        _element_counters[chapter_id][element_type] = 0
    _element_counters[chapter_id][element_type] += 1
    return _element_counters[chapter_id][element_type]


def reset_element_counters(chapter_id: str = None):
    """Reset element counters for a chapter or all chapters."""
    global _element_counters
    if chapter_id:
        _element_counters[chapter_id] = {}
    else:
        _element_counters = {}


def generate_element_id(chapter_id: str, element_type: str, sect1_id: str = None) -> str:
    """
    Generate a compliant element ID following R2 Library naming convention.

    Format: {sect1_id}{element_code}{sequence} or {chapter_id}{element_code}{sequence}
    - Max 25 characters total
    - Only lowercase letters and numbers

    Args:
        chapter_id: Chapter ID (e.g., "ch0001", "in0001", "pr0001")
        element_type: Type of element (figure, table, bibliography, anchor, etc.)
        sect1_id: Optional sect1 ID. If not provided, uses chapter_id + "s0001"
                  (required by downstream XSL for link resolution)

    Returns:
        Compliant ID string

    Raises:
        ValueError: If element_type is a section type (sect1-sect5). Use
                   generate_section_id() or next_available_sect1_id() instead.
    """
    # CRITICAL: Section elements must NOT use this function - they need special handling
    # Using element codes for sections produces malformed IDs like ch0005s0001s10002
    element_type_lower = element_type.lower()
    if element_type_lower in {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section'}:
        raise ValueError(
            f"generate_element_id() cannot be used for section elements (got '{element_type}'). "
            f"Use generate_section_id() or next_available_sect1_id() instead."
        )

    # Get element code
    code = ELEMENT_CODES.get(element_type_lower, 'x')

    # Determine base ID
    # IMPORTANT: Always use s0001 suffix when sect1_id is None (1-based numbering)
    # The downstream XSL (link.ritt.xsl) parses linkends and expects:
    # - For ap/pr/pt with 's' in linkend: extracts before 's' as file identifier
    # - For other cases: takes first 11 characters as file identifier (e.g., ch0001s0001)
    # Removing the section suffix breaks link resolution in the downstream processing
    if sect1_id is None:
        base_id = f"{chapter_id}s{1:04d}"
    else:
        base_id = sect1_id

    # Get next counter
    counter = get_element_counter(chapter_id, element_type)

    # Calculate available digits for sequence number
    # Use 2 digits by default to keep IDs shorter (supports up to 99 elements per type per section)
    # Max 25 chars - base_id length - code (2-3) - counter (2) leaves room for hierarchy
    available = MAX_ID_LENGTH - len(base_id) - len(code)
    digits = max(1, min(2, available))  # Use 2 digits by default, minimum 1

    # Ensure uniqueness within this chapter (guard against accidental collisions)
    candidate = f"{base_id}{code}{counter:0{digits}d}"
    while _is_id_used(chapter_id, candidate):
        counter = get_element_counter(chapter_id, element_type)
        candidate = f"{base_id}{code}{counter:0{digits}d}"

    _register_generated_id(chapter_id, candidate)
    return candidate


# =============================================================================
# ID TRACKING - Now uses centralized IDTracker for all ID management
# =============================================================================
# The IDTracker provides:
# - Single source of truth for all source IDs, XML IDs, and mappings
# - Tracking of dropped IDs with reasons
# - Linkend resolution with proper handling of dropped targets
# - JSON export for debugging and validation
# - Automatic conversion of links to dropped IDs -> phrase elements

# Legacy _id_mapping kept for backward compatibility with code that directly accesses it
# All functions now delegate to the IDTracker
_id_mapping: Dict[Tuple[str, str], str] = {}

# Track ID renames for updating linkends after document assembly
# Maps old_id -> new_id for IDs that were changed (e.g., during sect1 wrapper creation)
# IMPORTANT: This dict always maps original_id -> final_id (chains are composed)
_id_renames: Dict[str, str] = {}


def add_id_rename(old_id: str, new_id: str) -> None:
    """
    Add an ID rename to the tracking dictionary, composing chains automatically.

    When an ID is renamed multiple times (A→B, then B→C), this function ensures
    the mapping stays as original→final (A→C) rather than requiring multiple
    passes to follow the chain.

    This is called when:
    - Sect1 wrapper creation renames element IDs
    - DTD fixer creates wrapper elements and updates child IDs

    Args:
        old_id: The ID being renamed (could be original or intermediate)
        new_id: The new ID value
    """
    # First, update any existing renames that point TO old_id
    # If we have A→B and now adding B→C, update A→B to become A→C
    for source_id, target_id in list(_id_renames.items()):
        if target_id == old_id:
            _id_renames[source_id] = new_id
            logger.debug(f"Composed ID rename chain: {source_id} → {old_id} → {new_id} (now {source_id} → {new_id})")

    # Now add the new rename (old_id → new_id)
    # But only if old_id isn't already a "final" target of another rename
    # (In that case, the chain composition above already handled it)
    _id_renames[old_id] = new_id
    logger.debug(f"Added ID rename: {old_id} → {new_id}")


def get_final_id(original_id: str) -> str:
    """
    Get the final ID for an original ID, following the rename chain.

    Since add_id_rename() composes chains automatically, this is just a
    simple lookup. Returns the original ID if no rename exists.

    Args:
        original_id: The ID to look up

    Returns:
        The final ID after all renames, or original_id if not renamed
    """
    return _id_renames.get(original_id, original_id)


def reset_id_mapping():
    """Reset the ID mapping dictionary, ID renames, and the IDTracker."""
    global _id_mapping, _id_renames
    _id_mapping = {}
    _id_renames = {}
    reset_tracker()


def get_mapped_id(chapter_id: str, original_id: str) -> Optional[str]:
    """Get the generated ID for an original source ID."""
    # Use id_authority as single source of truth
    authority = get_authority()
    resolved = authority.resolve(chapter_id, original_id)
    if resolved:
        return resolved

    # Fallback to legacy dict for backward compatibility
    return _id_mapping.get((chapter_id, original_id))


def register_id_mapping(chapter_id: str, original_id: str, generated_id: str,
                        element_type: str = "", source_file: str = ""):
    """
    Register a mapping from original ID to generated ID.

    Uses id_authority as the single source of truth.

    Args:
        chapter_id: Chapter ID (e.g., ch0001)
        original_id: Original source ID from EPUB
        generated_id: Generated XML ID
        element_type: Optional element type (figure, table, section, etc.)
        source_file: Original XHTML file name (e.g., chapter16.xhtml)
    """
    # Register in legacy dict for backward compatibility with existing code
    _id_mapping[(chapter_id, original_id)] = generated_id

    # Register in centralized ID Authority (single source of truth)
    authority = get_authority()
    authority.map_id(chapter_id, original_id, generated_id, element_type, source_file)


def register_source_id(chapter_id: str, source_id: str, element_type: str,
                       element_tag: str, context: str = "",
                       source_file: str = "") -> None:
    """
    Register a source ID found in the EPUB (for tracking purposes).

    Note: With id_authority, source IDs are tracked implicitly via map_id.
    This function is kept for API compatibility.

    Args:
        chapter_id: Chapter ID (e.g., ch0001)
        source_id: Original ID from HTML
        element_type: Semantic type (figure, table, section, etc.)
        element_tag: Original HTML tag name
        context: Optional context information
        source_file: Original XHTML file name (e.g., chapter16.xhtml)
    """
    # With id_authority, source IDs are tracked when map_id is called
    # This function is kept for API compatibility
    pass


def register_xml_id(xml_id: str, element_type: str, chapter_id: str,
                    source_id: Optional[str] = None, source_file: str = "") -> None:
    """
    Register a generated XML ID as valid.

    Uses id_authority as the single source of truth.

    Args:
        xml_id: The generated XML ID
        element_type: Semantic type (figure, table, section, etc.)
        chapter_id: Chapter ID
        source_id: Original source ID if this maps from a source ID
        source_file: Original XHTML file name (e.g., chapter16.xhtml)
    """
    authority = get_authority()
    authority.register_valid_id(xml_id)


def mark_id_dropped(chapter_id: str, source_id: str, reason: str,
                    element_type: str = "", context: str = "",
                    source_file: str = "") -> None:
    """
    Mark a source ID as intentionally dropped (not converted to XML).

    Uses id_authority as the single source of truth.
    Links pointing to dropped IDs will be converted to phrase elements.

    Args:
        chapter_id: Chapter ID
        source_id: Original source ID
        reason: Why it was dropped (pagebreak, anchor-only, invalid, etc.)
        element_type: Original element type
        context: Additional context
        source_file: Original XHTML file name (e.g., chapter16.xhtml)
    """
    authority = get_authority()
    # Include context in the reason if provided
    full_reason = f"{reason} ({context})" if context else reason
    authority.mark_dropped(chapter_id, source_id, full_reason, element_type, source_file)


def is_id_dropped(chapter_id: str, source_id: str) -> bool:
    """Check if a source ID was marked as dropped."""
    authority = get_authority()
    return authority.is_dropped(chapter_id, source_id)


def _update_child_element_ids_for_new_section(sect1: etree.Element, chapter_id: str,
                                               new_sect1_id: str) -> int:
    """
    Update element IDs within a newly wrapped sect1 to match its ID prefix.

    When content is wrapped in a new sect1, any element IDs that were generated
    with a different section prefix need to be updated to use the new sect1's ID.
    This ensures proper ID hierarchy for downstream XSL processing.

    Also tracks the ID renames in _id_renames so linkends can be updated later.

    Args:
        sect1: The sect1 element containing the content
        chapter_id: The chapter ID (e.g., 'ch0018')
        new_sect1_id: The new sect1's ID (e.g., 'ch0018s0001')

    Returns:
        Number of IDs updated
    """
    import re

    updated_count = 0

    # Element codes that indicate an element ID (vs section ID)
    # These elements should have their IDs updated to match parent sect1
    element_codes = {'fg', 'ta', 'bib', 'sb', 'ad', 'ex', 'eq', 'fn', 'gl', 'qa', 'pr', 'mo', 'vd', 'an', 'li', 'p'}

    # Pattern to match element IDs: {prefix}s{section_num}{code}{seq}
    # e.g., ch0018s0001ta01, ch0018s0013fg02
    element_id_pattern = re.compile(r'^([a-z]{2}\d{4})(s\d{4})([a-z]{1,3})(\d+)$')

    for elem in sect1.iter():
        old_id = elem.get('id')
        if not old_id:
            continue

        # Skip the sect1 itself
        if elem is sect1:
            continue

        # Check if this is an element ID (not a section ID)
        match = element_id_pattern.match(old_id)
        if not match:
            continue

        prefix, old_section, code, seq = match.groups()

        # Only update if the section part doesn't match the new sect1
        expected_section = new_sect1_id[len(chapter_id):]  # e.g., 's0001'
        if old_section == expected_section:
            continue  # Already correct

        # Generate new ID with correct section prefix
        new_id = f"{new_sect1_id}{code}{seq}"

        # Update the element's ID
        elem.set('id', new_id)

        # Track the rename for later linkend updates (TOC, list of tables, etc.)
        # Using add_id_rename() ensures chains are composed (A→B + B→C = A→C)
        add_id_rename(old_id, new_id)

        # Update ID mappings in the tracker
        update_id_mapping_target(old_id, new_id)

        logger.debug(f"Updated element ID for sect1 wrapper: {old_id} -> {new_id}")
        updated_count += 1

    if updated_count > 0:
        logger.info(f"Updated {updated_count} element IDs to match new sect1 wrapper {new_sect1_id}")

    return updated_count


def apply_id_renames_to_linkends(root: etree.Element) -> int:
    """
    Apply tracked ID renames to all linkend attributes in the document.

    This should be called after all chapters are assembled into the book,
    to update linkends in TOC, list of tables, list of figures, and any
    cross-references that still point to old (intermediate) IDs.

    Args:
        root: Root element of the document tree

    Returns:
        Number of linkends updated
    """
    if not _id_renames:
        return 0

    updated_count = 0

    # Update all linkend attributes
    for elem in root.iter():
        linkend = elem.get('linkend')
        if linkend and linkend in _id_renames:
            new_linkend = _id_renames[linkend]
            elem.set('linkend', new_linkend)
            updated_count += 1
            logger.debug(f"Updated linkend: {linkend} -> {new_linkend}")

        # Also check url attributes with fragment identifiers
        url = elem.get('url')
        if url and '#' in url:
            base, frag = url.rsplit('#', 1)
            if frag in _id_renames:
                new_url = f"{base}#{_id_renames[frag]}"
                elem.set('url', new_url)
                updated_count += 1
                logger.debug(f"Updated url fragment: {url} -> {new_url}")

    if updated_count > 0:
        logger.info(f"Applied {updated_count} ID renames to linkends (from {len(_id_renames)} tracked renames)")

    return updated_count


def get_id_renames() -> Dict[str, str]:
    """Get the current ID renames dictionary (for debugging/testing)."""
    return _id_renames.copy()


def update_id_mapping_target(old_generated_id: str, new_generated_id: str) -> int:
    """
    Update all ID mappings that point to old_generated_id to point to new_generated_id instead.

    This is necessary when an element is transformed (e.g., listitem -> bibliomixed) and gets
    a new ID, or when a synthetic sect1 wrapper changes the section context for child IDs.
    All original source IDs that were mapped to the old generated ID need to be
    updated to point to the new ID, otherwise deferred link resolution will find stale IDs.

    Uses id_authority as the single source of truth.
    """
    updated_count = 0

    # Update legacy dict for backward compatibility
    for key, value in list(_id_mapping.items()):
        if value == old_generated_id:
            _id_mapping[key] = new_generated_id
            updated_count += 1
            logger.debug(f"Updated ID mapping {key} from {old_generated_id} -> {new_generated_id}")

    # Update centralized ID Authority (single source of truth)
    authority = get_authority()
    # Update the registry's _id_lookup entries that point to the old generated ID
    # Keys are tuples of (chapter_id, source_id)
    keys_to_update = []
    for key, value in authority.registry._id_lookup.items():
        if value == old_generated_id:
            keys_to_update.append(key)
    for key in keys_to_update:
        chapter_id, source_id = key  # Keys are tuples
        authority.registry._id_lookup[key] = new_generated_id
        # Update the IDRecord if it exists
        if key in authority.registry._id_records:
            record = authority.registry._id_records[key]
            record.generated_id = new_generated_id
            record.history.append(f"ID target updated: {old_generated_id} -> {new_generated_id}")
        logger.debug(f"Updated ID Authority mapping {chapter_id}:{source_id} from {old_generated_id} -> {new_generated_id}")

    # Update valid_ids set
    if old_generated_id in authority.registry.valid_ids:
        authority.registry.valid_ids.discard(old_generated_id)
        authority.registry.valid_ids.add(new_generated_id)

    return updated_count


# Pattern matches page references like "page_123", "Page-45", "page99"
# Uses negative lookahead (?![.\d]) to avoid matching section references like "Page_13.5.1.3"
# where the number is followed by a dot and more digits (hierarchical section numbers)
_PAGEBREAK_REF_RE = re.compile(r'page[_-]?\d+(?![.\d])', re.IGNORECASE)
_CAPTION_CLASS_RE = re.compile(r'(figcaption|figurecaption|figurelabel|figlabel|caption|tablecaption|tblcaption|tabcaption)', re.IGNORECASE)


def _is_pagebreak_ref(value: str) -> bool:
    """Return True when a reference targets a pagebreak ID."""
    if not value:
        return False
    if isinstance(value, list):
        value = value[0] if value else ''
    candidate = str(value).strip()
    if not candidate:
        return False
    fragment = candidate.split('#', 1)[1] if '#' in candidate else candidate
    return bool(_PAGEBREAK_REF_RE.search(fragment))


def _find_adjacent_caption_elem(elem: Tag, element_type: str) -> Optional[Tag]:
    """
    Find a nearby caption/label element that may sit outside figure/table wrappers.

    This catches patterns like:
      <p class="figcaption"><a id="c1-fig-0001">Figure 1.1</a> ...</p>
      <div class="figure"> ... </div>
    """
    if elem is None:
        return None

    def _is_pagebreak_span(tag: Tag) -> bool:
        epub_type = tag.get('epub:type', '') or tag.get('data-type', '')
        role_attr = tag.get('role', '')
        return ((epub_type and 'pagebreak' in epub_type.lower()) or
                (role_attr and 'pagebreak' in role_attr.lower()))

    def _looks_like_caption(tag: Tag) -> bool:
        if tag.name in {'figcaption', 'caption'}:
            return True
        css_class = tag.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        role_attr = tag.get('role', '') or ''
        epub_type = tag.get('epub:type', '') or tag.get('data-type', '')
        combined = ' '.join([css_class, role_attr, epub_type]).lower()
        if _CAPTION_CLASS_RE.search(combined):
            return True
        text = extract_text(tag).lower()
        if element_type == 'figure' and re.search(r'\bfig(ure)?\b', text):
            return True
        if element_type == 'table' and re.search(r'\btable\b', text):
            return True
        return False

    def _adjacent_sibling(start: Optional[Tag], direction: str) -> Optional[Tag]:
        sibling = start
        while sibling is not None:
            sibling = sibling.previous_sibling if direction == 'prev' else sibling.next_sibling
            if sibling is None:
                return None
            if isinstance(sibling, NavigableString):
                if str(sibling).strip():
                    return None
                continue
            if isinstance(sibling, Tag):
                if _is_pagebreak_span(sibling):
                    continue
                if sibling.name == 'span' and is_empty_span_marker(sibling):
                    continue
                return sibling
        return None

    for direction in ('prev', 'next'):
        sibling = _adjacent_sibling(elem, direction)
        if sibling is not None and _looks_like_caption(sibling):
            return sibling
    return None


def _get_preceding_span_marker_ids(heading_elem: Tag) -> List[str]:
    """Collect empty span marker IDs immediately preceding a heading."""
    if heading_elem is None:
        return []
    marker_ids: List[str] = []
    prev = heading_elem.previous_sibling
    while prev is not None:
        if isinstance(prev, NavigableString):
            if str(prev).strip():
                break
            prev = prev.previous_sibling
            continue
        if isinstance(prev, Tag):
            epub_type = prev.get('epub:type', '') or prev.get('data-type', '')
            role_attr = prev.get('role', '')
            if (epub_type and 'pagebreak' in epub_type.lower()) or (role_attr and 'pagebreak' in role_attr.lower()):
                prev = prev.previous_sibling
                continue
            if prev.name == 'span' and is_empty_span_marker(prev):
                span_id = prev.get('id')
                if span_id and not _is_pagebreak_ref(span_id):
                    marker_ids.append(span_id)
                prev = prev.previous_sibling
                continue
            break
        prev = prev.previous_sibling
    marker_ids.reverse()
    return marker_ids


def _register_label_anchor_ids(label_elem: Optional[Tag], chapter_id: str,
                               target_id: str, element_type: str,
                               allow_any_anchor: bool = False) -> None:
    """
    Register anchor IDs found in labels/captions to the generated element ID.

    This handles common EPUB patterns where the figure/table label anchor
    carries the ID (e.g., <a id="c2-fig-0001" href="#R_c2-fig-0001">Figure 2.1</a>)
    but the actual element has no ID. Without this mapping, link resolution
    can fall back to unrelated IDs (like list items with the same number).
    """
    if not label_elem or not target_id:
        return

    keyword_map = {
        'figure': ('figure', 'fig'),
        'table': ('table', 'tbl', 'tab'),
        'equation': ('equation', 'eq', 'formula'),
        'example': ('example', 'exercise', 'casestudy', 'case'),
        'procedure': ('procedure', 'proc'),
        'sidebar': ('sidebar', 'aside', 'box', 'callout', 'feature', 'note', 'warning',
                    'caution', 'tip', 'important', 'admonition'),
        'section': ('section', 'sec'),
        'chapter': ('chapter', 'chap'),
    }
    keywords = keyword_map.get(element_type, ())
    if not keywords:
        return

    def _tokenize(value: str) -> List[str]:
        return [t for t in re.split(r'[^a-z0-9]+', value) if t]

    def _keyword_match(value: str, tokens: List[str]) -> bool:
        for key in keywords:
            if len(key) <= 2:
                if key in tokens:
                    return True
            elif key in value:
                return True
        return False

    def _is_label_anchor(candidate_id: str, label_text: str) -> bool:
        if allow_any_anchor:
            return True
        candidate_lower = candidate_id.lower()
        tokens = _tokenize(candidate_lower)
        stripped = candidate_lower
        if stripped.startswith('r_') or stripped.startswith('r-'):
            stripped = stripped[2:]
        stripped_tokens = _tokenize(stripped)
        label_tokens = _tokenize(label_text)
        if _keyword_match(candidate_lower, tokens):
            return True
        if _keyword_match(stripped, stripped_tokens):
            return True
        if label_text and _keyword_match(label_text, label_tokens):
            return True
        return False

    for node in label_elem.find_all(True):
        epub_type = _get_epub_type(node)
        role_attr = node.get('role', '')
        if (epub_type and 'pagebreak' in epub_type.lower()) or (role_attr and 'pagebreak' in role_attr.lower()):
            continue
        candidate_ids = []
        node_id = node.get('id') or node.get('name')
        if node_id:
            candidate_ids.append(node_id)

        if node.name == 'a':
            href = node.get('href', '')
            if href and href.startswith('#') and len(href) > 1:
                candidate_ids.append(href[1:])

        if not candidate_ids:
            continue

        label_text = extract_text(node).lower()
        for candidate in candidate_ids:
            if isinstance(candidate, list):
                candidate = candidate[0] if candidate else ''
            candidate = str(candidate).strip()
            if not candidate:
                continue
            if _is_label_anchor(candidate, label_text):
                register_id_mapping(chapter_id, candidate, target_id)
                if candidate.lower().startswith('r_') or candidate.lower().startswith('r-'):
                    register_id_mapping(chapter_id, candidate[2:], target_id)


def _infer_label_element_type(title_elem: Optional[Tag], css_class: str = "",
                              role_attr: str = "") -> Optional[str]:
    if not title_elem and not css_class and not role_attr:
        return None

    title_text = extract_text(title_elem) if title_elem is not None else ""
    combined = " ".join([css_class or "", role_attr or "", title_text or ""]).lower()

    if re.search(r'\bprocedure\b', combined) or re.search(r'\bproc\b', combined):
        return 'procedure'
    if 'example' in combined or 'exercise' in combined or 'casestudy' in combined or 'case study' in combined:
        return 'example'
    if re.search(r'\beq(uation)?\b', combined) or 'formula' in combined:
        return 'equation'
    if ('sidebar' in combined or 'aside' in combined or 'box' in combined or 'callout' in combined
            or 'feature' in combined or 'note' in combined or 'warning' in combined
            or 'caution' in combined or 'tip' in combined or 'important' in combined
            or 'admonition' in combined):
        return 'sidebar'

    return None


# =============================================================================
# ELEMENT TYPE VALIDATION FOR DTD COMPLIANCE
# =============================================================================

# Elements NOT allowed in dedication (only legalnotice.mix allowed)
DEDICATION_DISALLOWED = {
    'figure', 'informalfigure', 'table', 'informaltable',
    'sidebar', 'example', 'informalexample', 'procedure',
    'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect',
    'bridgehead', 'highlights', 'abstract', 'qandaset',
    'variablelist', 'simplelist', 'segmentedlist', 'calloutlist',
    'mediaobject', 'graphic', 'equation', 'informalequation',
}

# Elements allowed in dedication (legalnotice.mix)
DEDICATION_ALLOWED = {
    'title', 'subtitle', 'titleabbrev', 'risinfo',  # title content
    'para', 'simpara', 'formalpara',  # paragraphs
    'itemizedlist', 'orderedlist', 'glosslist',  # lists
    'caution', 'important', 'note', 'tip', 'warning',  # admonitions
    'literallayout', 'programlisting', 'screen', 'synopsis', 'address',  # linespecific
    'blockquote', 'indexterm',  # other
}


def _get_root_element_type(elem: etree.Element) -> str:
    """
    Get the root element tag (chapter, preface, dedication, etc.).

    Args:
        elem: Any lxml element in the document tree

    Returns:
        The tag name of the root element (e.g., 'chapter', 'dedication', 'preface')
    """
    current = elem
    while current.getparent() is not None:
        current = current.getparent()
    return current.tag


def _is_valid_element_for_parent(element_tag: str, parent_elem: etree.Element,
                                  elem_id: str = None) -> bool:
    """
    Validate that an element can be added to the given parent based on DTD rules.

    This function checks content model restrictions:
    - Dedication only allows legalnotice.mix elements
    - TOC only allows toc* elements

    Args:
        element_tag: The element tag being created (figure, table, sect1, etc.)
        parent_elem: The parent lxml element
        elem_id: Optional ID for logging

    Returns:
        True if the element is valid for this parent, False otherwise
    """
    root_tag = _get_root_element_type(parent_elem)

    # Dedication restrictions: only legalnotice.mix allowed
    if root_tag == 'dedication':
        if element_tag in DEDICATION_DISALLOWED:
            id_info = f" (id={elem_id})" if elem_id else ""
            logger.warning(
                f"Skipping <{element_tag}>{id_info} in <dedication> - "
                f"not allowed in legalnotice.mix content model"
            )
            return False

    # TOC restrictions: only toc* elements and title allowed
    if root_tag == 'toc':
        allowed_toc = {
            'title', 'subtitle', 'titleabbrev',
            'tocfront', 'tocback', 'tocpart', 'tocchap',
            'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5',
            'tocentry'
        }
        if element_tag not in allowed_toc:
            id_info = f" (id={elem_id})" if elem_id else ""
            logger.warning(
                f"Skipping <{element_tag}>{id_info} in <toc> - "
                f"only toc* elements allowed in TOC content model"
            )
            return False

    return True


def _convert_to_para_for_dedication(text_content: str, parent_elem: etree.Element,
                                     original_tag: str) -> etree.Element:
    """
    Convert an invalid dedication element to a para element.

    When an element like figure or table cannot be added to dedication,
    this function creates a placeholder para with a note about the skipped content.

    Args:
        text_content: Text content from the original element
        parent_elem: The dedication element to add the para to
        original_tag: The original element tag (for logging)

    Returns:
        The created para element
    """
    para = validated_subelement(parent_elem, 'para')
    if text_content and text_content.strip():
        para.text = text_content.strip()
    else:
        # Add a comment noting the skipped element
        para.text = f"[{original_tag} content not included - not valid in dedication]"
    return para


# Constants for deferred link resolution (two-pass processing)
DEFERRED_ORIGINAL_REF = '_deferred_original_ref'
DEFERRED_TARGET_CHAPTER = '_deferred_target_chapter'
DEFERRED_CURRENT_CHAPTER = '_deferred_current_chapter'


# Counter for generating unique fallback IDs per chapter
_fallback_counters: Dict[str, int] = {}


def reset_fallback_counters():
    """Reset fallback counters for new conversion."""
    global _fallback_counters
    _fallback_counters = {}


# Track all generated IDs per chapter to guarantee uniqueness in output
# (Source EPUBs sometimes reuse HTML ids; we must not emit duplicates.)
_generated_ids: Dict[str, set] = {}


def reset_generated_id_registry(chapter_id: str = None):
    """Reset generated-ID registry for a chapter or all chapters."""
    global _generated_ids
    if chapter_id:
        _generated_ids[chapter_id] = set()
    else:
        _generated_ids = {}


def _register_generated_id(chapter_id: str, id_value: str) -> None:
    """Record an emitted ID to prevent duplicates."""
    if not chapter_id or not id_value:
        return
    if chapter_id not in _generated_ids:
        _generated_ids[chapter_id] = set()
    _generated_ids[chapter_id].add(id_value)

    # Register with centralized ID Authority as a valid generated ID
    get_authority().register_valid_id(id_value)


def _is_id_used(chapter_id: str, id_value: str) -> bool:
    """Check if an ID has already been emitted for this chapter."""
    if not chapter_id or not id_value:
        return False
    return id_value in _generated_ids.get(chapter_id, set())


def resolve_linkend_id(original_id: str, target_chapter: str, current_chapter: str = "") -> str:
    """
    Resolve a linkend value to its generated ID.

    Uses id_authority as the SINGLE source of truth.

    This function is used during the SECOND pass after all chapters have been
    processed and all element IDs are registered.

    Args:
        original_id: Original anchor/element ID from source
        target_chapter: Target chapter ID
        current_chapter: Current chapter ID (for same-chapter fallback)

    Returns:
        The resolved XML ID, or empty string if not found/dropped
    """
    if not original_id:
        return ""

    # Check for pagebreak reference pattern first
    if _is_pagebreak_ref(original_id):
        mark_id_dropped(target_chapter, original_id, "pagebreak", context="pagebreak reference")
        return ""

    # Citation-style IDs should ONLY resolve within the target chapter
    # This prevents cross-chapter resolution for bibliography references (CR#, bib#, etc.)
    # which are commonly reused across chapters with different meanings
    citation_pattern = re.compile(r'^(CR|Ref|ref|bib|Bib|fn|FN|note|Note)\d+$', re.IGNORECASE)
    is_citation_id = bool(citation_pattern.match(original_id))

    def _is_valid_resolution(resolved_id: str) -> bool:
        """Check if resolved ID belongs to the target chapter."""
        if not resolved_id:
            return False
        # For ALL IDs, only accept resolutions that belong to the target chapter
        # This prevents cross-chapter resolution errors where a link intended for
        # ch0005 accidentally resolves to an element in ch0003
        if not resolved_id.startswith(target_chapter):
            if is_citation_id:
                logger.debug(f"Rejecting cross-chapter citation resolution: {original_id} -> {resolved_id} (expected {target_chapter})")
            else:
                logger.debug(f"Rejecting cross-chapter resolution: {original_id} -> {resolved_id} (expected {target_chapter})")
            return False
        return True

    # =========================================================================
    # Use centralized ID Authority (single source of truth)
    # =========================================================================
    authority = get_authority()

    # Check if dropped first
    if authority.is_dropped(target_chapter, original_id):
        reason = authority.get_drop_reason(target_chapter, original_id)
        logger.debug(f"Linkend '{original_id}' is dropped (reason: {reason})")
        return ""

    # Try direct resolution
    resolved = authority.resolve(target_chapter, original_id)
    if resolved and _is_valid_resolution(resolved):
        logger.debug(f"Resolved: {original_id} -> {resolved}")
        return resolved

    # Try with sanitized ID (lowercase, alphanumeric only)
    sanitized_original = re.sub(r'[^a-z0-9]', '', original_id.lower())
    if sanitized_original != original_id:
        resolved = authority.resolve(target_chapter, sanitized_original)
        if resolved and _is_valid_resolution(resolved):
            logger.debug(f"Resolved (sanitized): {original_id} -> {resolved}")
            return resolved

    # Try with unprefixed format (ch0001-original_id -> original_id)
    if original_id.startswith(target_chapter):
        unprefixed = original_id[len(target_chapter):].lstrip('-_')
        resolved = authority.resolve(target_chapter, unprefixed)
        if resolved and _is_valid_resolution(resolved):
            logger.debug(f"Resolved (unprefixed): {original_id} -> {resolved}")
            return resolved

    # Try stripping common prefixes
    prefix_patterns = [
        (r'^r\d+_', ''),           # r1_xxx -> xxx
        (r'^[Rr][_-]', ''),        # R_xxx or R-xxx -> xxx
        (r'^p(\d+)$', r'para\1'),  # p269 -> para269
        (r'^indx-term', 'ix'),     # indx-term1064 -> ix1064
        (r'^fn', 'footnote'),      # fn01 -> footnote01
    ]
    for pattern, replacement in prefix_patterns:
        transformed = re.sub(pattern, replacement, original_id)
        if transformed != original_id:
            resolved = authority.resolve(target_chapter, transformed)
            if resolved and _is_valid_resolution(resolved):
                logger.debug(f"Resolved (transformed): {original_id} -> {resolved}")
                return resolved

    # NOTE: We intentionally do NOT fall back to current_chapter when target_chapter
    # resolution fails. Doing so could return an ID from the wrong chapter, causing
    # cross-chapter link corruption (e.g., link to ch0005's figure resolving to ch0003's).
    # If resolution fails for target_chapter, the link should be marked as unresolved.

    # Log unresolved for debugging
    if is_citation_id:
        logger.warning(f"Unresolved citation reference: '{original_id}' in {target_chapter} - this citation will be converted to plain text")
    else:
        logger.warning(f"Unresolved linkend reference: '{original_id}' in {target_chapter} - link will be converted to phrase")
    return ""


def mark_link_for_deferred_resolution(link_elem: etree.Element, original_ref: str,
                                       target_chapter: str, current_chapter: str) -> None:
    """
    Mark a link element for deferred resolution during the second pass.

    Instead of resolving the link immediately (which could pollute the ID mapping
    with incorrect element types for forward references), we store the original
    reference info as temporary attributes.

    Args:
        link_elem: The link or ulink element
        original_ref: Original reference ID from source
        target_chapter: Target chapter ID
        current_chapter: Current chapter being processed
    """
    link_elem.set(DEFERRED_ORIGINAL_REF, original_ref)
    link_elem.set(DEFERRED_TARGET_CHAPTER, target_chapter)
    link_elem.set(DEFERRED_CURRENT_CHAPTER, current_chapter)


def resolve_deferred_links(root_elem: etree.Element) -> int:
    """
    Second pass: Resolve all deferred link references.

    This function is called after all chapters have been processed and all
    element IDs are registered in the mapping. It finds all link/ulink/xref/citation
    elements with deferred resolution attributes and resolves them properly.

    When resolution fails (empty resolved_id), elements are converted to preserve
    their text content while remaining DTD-compliant:
    - <link> -> <phrase> (since link requires linkend per DTD)
    - <xref> -> <emphasis> (since xref requires linkend)
    - <ulink> -> URL without anchor

    Args:
        root_elem: Root element of the XML tree (book element)

    Returns:
        Number of links resolved
    """
    resolved_count = 0
    unresolved_count = 0

    # Build list first to avoid modifying tree during iteration
    elements_to_process = []
    for elem in root_elem.iter():
        original_ref = elem.get(DEFERRED_ORIGINAL_REF)
        if original_ref is not None:
            elements_to_process.append(elem)

    for elem in elements_to_process:
        original_ref = elem.get(DEFERRED_ORIGINAL_REF)
        target_chapter = elem.get(DEFERRED_TARGET_CHAPTER, '')
        current_chapter = elem.get(DEFERRED_CURRENT_CHAPTER, '')

        # Resolve the link using the complete ID mapping (with tracker)
        resolved_id = resolve_linkend_id(original_ref, target_chapter, current_chapter)

        # Validate that resolved ID is in the expected chapter
        # Cross-chapter resolution indicates incorrect link resolution and must be rejected
        if resolved_id and target_chapter:
            resolved_chapter_match = re.match(r'^([a-z]{2}\d{4})', resolved_id)
            if resolved_chapter_match:
                resolved_chapter = resolved_chapter_match.group(1)
                if resolved_chapter != target_chapter:
                    # Cross-chapter resolution detected - reject it to prevent wrong links
                    logger.warning(
                        f"Rejecting cross-chapter resolution: '{original_ref}' in {target_chapter} "
                        f"resolved to {resolved_id} (chapter {resolved_chapter}). "
                        f"Link will be converted to phrase/text."
                    )
                    resolved_id = ""  # Treat as unresolved - don't link to wrong chapter

        # Handle unresolved links - convert to DTD-compliant alternatives
        if not resolved_id:
            parent = elem.getparent()
            if parent is None:
                # Remove temporary attributes and skip
                _remove_deferred_attrs(elem)
                unresolved_count += 1
                continue

            if elem.tag == 'link':
                # <link> requires linkend per DTD - convert to <phrase>
                # BUT: <phrase> is NOT allowed in restrictive elements like superscript, subscript
                # These elements only allow #PCDATA or limited inline children (no phrase)
                restrictive_parents = {'superscript', 'subscript', 'code', 'literal',
                                       'constant', 'varname', 'function', 'parameter'}
                parent_tag = parent.tag if hasattr(parent, 'tag') else ''

                if parent_tag in restrictive_parents:
                    # Unwrap content directly into parent (don't use phrase wrapper)
                    index = list(parent).index(elem)
                    # Handle text content
                    if elem.text:
                        if index == 0:
                            parent.text = (parent.text or '') + elem.text
                        else:
                            prev_sibling = parent[index - 1]
                            prev_sibling.tail = (prev_sibling.tail or '') + elem.text
                    # Move children to parent
                    for i, child in enumerate(list(elem)):
                        parent.insert(index + i, child)
                    # Handle tail
                    if elem.tail:
                        if len(list(elem)) > 0:
                            # Tail goes after last moved child
                            last_child = list(elem)[-1]
                            last_child.tail = (last_child.tail or '') + elem.tail
                        elif index == 0:
                            parent.text = (parent.text or '') + elem.tail
                        else:
                            prev_sibling = parent[index - 1]
                            prev_sibling.tail = (prev_sibling.tail or '') + elem.tail
                    parent.remove(elem)
                    logger.debug(f"Unwrapped unresolved <link> content in restrictive parent <{parent_tag}> for ref '{original_ref}'")
                else:
                    phrase = etree.Element('phrase')
                    phrase.text = elem.text
                    phrase.tail = elem.tail
                    for child in list(elem):
                        phrase.append(child)
                    if elem.get('role'):
                        phrase.set('role', elem.get('role'))
                    index = list(parent).index(elem)
                    parent.insert(index, phrase)
                    parent.remove(elem)
                    logger.debug(f"Converted unresolved <link> to <phrase> for ref '{original_ref}'")

            elif elem.tag == 'xref':
                # <xref> requires linkend per DTD - convert to <emphasis>
                emphasis = etree.Element('emphasis')
                # Use original ref as fallback text for xref (which is normally empty)
                emphasis.text = original_ref
                emphasis.tail = elem.tail
                index = list(parent).index(elem)
                parent.insert(index, emphasis)
                parent.remove(elem)
                logger.debug(f"Converted unresolved <xref> to <emphasis> for ref '{original_ref}'")

            elif elem.tag == 'citation':
                # Citation can have text without reference
                elem.text = original_ref
                _remove_deferred_attrs(elem)

            elif elem.tag == 'ulink':
                # For ulink, just use the chapter without anchor
                current_url = elem.get('url', '')
                if '#' in current_url:
                    base_part = current_url.split('#')[0]
                    elem.set('url', base_part if base_part else target_chapter)
                else:
                    elem.set('url', target_chapter)
                _remove_deferred_attrs(elem)

            unresolved_count += 1
            continue

        # Successfully resolved - update the element
        if elem.tag == 'link':
            elem.set('linkend', resolved_id)
        elif elem.tag == 'xref':
            elem.set('linkend', resolved_id)
        elif elem.tag == 'citation':
            # Citation stores the ID in text, not attribute
            elem.text = resolved_id
        elif elem.tag == 'ulink':
            # For cross-chapter references, update the URL
            current_url = elem.get('url', '')
            if '#' in current_url:
                # Replace the anchor part
                base_part = current_url.split('#')[0]
                elem.set('url', f"{base_part}#{resolved_id}")
            else:
                elem.set('url', f"{target_chapter}#{resolved_id}")

        # Remove temporary attributes
        _remove_deferred_attrs(elem)
        resolved_count += 1

    logger.info(f"Resolved {resolved_count} deferred link references, {unresolved_count} unresolved")
    return resolved_count


def _remove_deferred_attrs(elem: etree.Element) -> None:
    """Remove temporary deferred resolution attributes from element."""
    for attr in [DEFERRED_ORIGINAL_REF, DEFERRED_TARGET_CHAPTER, DEFERRED_CURRENT_CHAPTER]:
        if attr in elem.attrib:
            del elem.attrib[attr]


def fix_toc_element_id_links(root_elem: etree.Element) -> int:
    """
    Fix TOC links that incorrectly point to element IDs (tables, figures, etc.)
    instead of section IDs, or point to non-existent IDs.

    When TOC chapter-level entries link to anchors that happen to be on tables/figures,
    the deferred resolution finds the element ID (e.g., ch0003s0001ta01) instead of
    the section ID (ch0003s0001). This function fixes those links.

    Also handles cases where the resolved ID doesn't exist at all - converts
    such links to phrase elements to avoid IDREF validation errors.

    SAFETY: Skips links that appear to intentionally reference figures/tables
    (detected by link text containing "Figure", "Table", etc.)

    Args:
        root_elem: Root element of the XML tree (book element)

    Returns:
        Number of TOC links fixed
    """
    fixed_count = 0
    converted_to_phrase_count = 0

    # First, collect ALL valid IDs in the document
    valid_ids = set()
    for elem in root_elem.iter():
        elem_id = elem.get('id')
        if elem_id:
            valid_ids.add(elem_id)

    # Pattern to detect element IDs: {prefix}{4digits}s{4digits}{element_code}{digits}
    # Element codes are 1-3 lowercase letters followed by digits
    element_id_pattern = re.compile(r'^([a-z]{2}\d{4}s\d{4})([a-z]{1,3})(\d+)$')

    # Pattern to detect chapter/section prefix for finding fallback sections
    chapter_section_pattern = re.compile(r'^([a-z]{2}\d{4})(s\d{4})?')

    # Keywords that indicate the link intentionally references an element (figure, table, etc.)
    # Don't fix these - they're likely in List of Figures/Tables
    element_reference_keywords = ['figure', 'table', 'equation', 'exhibit', 'plate', 'diagram']

    def should_fix_link(link_elem):
        """Check if this link should be fixed (not an intentional element reference)."""
        # Get all text content from the link (including child elements)
        link_text = ''.join(link_elem.itertext()).lower()

        # Skip if link text indicates it's intentionally referencing an element
        for keyword in element_reference_keywords:
            if keyword in link_text:
                return False
        return True

    def convert_link_to_phrase(link_elem, linkend):
        """Convert a link with invalid linkend to a phrase element."""
        nonlocal converted_to_phrase_count
        parent = link_elem.getparent()
        if parent is None:
            return False

        # Create phrase element preserving text content
        phrase = etree.Element('phrase')
        phrase.text = link_elem.text
        phrase.tail = link_elem.tail

        # Copy any children
        for child in link_elem:
            phrase.append(child)

        # Replace link with phrase
        idx = list(parent).index(link_elem)
        parent.remove(link_elem)
        parent.insert(idx, phrase)

        converted_to_phrase_count += 1
        logger.debug(f"Converted TOC link with invalid linkend '{linkend}' to phrase")
        return True

    def find_valid_section_for_chapter(chapter_prefix):
        """Find the first valid section ID for a chapter prefix."""
        # Look for section IDs that start with this chapter prefix
        for valid_id in sorted(valid_ids):
            if valid_id.startswith(chapter_prefix) and 's' in valid_id:
                # Check it's a section ID (not an element ID)
                if not element_id_pattern.match(valid_id):
                    return valid_id
        return None

    def fix_link_in_toc(link_elem):
        """Process a single TOC link and fix it if needed."""
        nonlocal fixed_count

        linkend = link_elem.get('linkend', '')
        if not linkend:
            return

        # Check if linkend exists - if not, we need to fix it
        if linkend not in valid_ids:
            # Check if this is an element ID we can strip
            match = element_id_pattern.match(linkend)
            if match:
                section_id = match.group(1)  # e.g., ch0003s0001
                element_code = match.group(2)  # e.g., ta, fg

                # Safety check: skip if link text suggests intentional element reference
                if not should_fix_link(link_elem):
                    return

                # Check if the stripped section_id exists
                if section_id in valid_ids:
                    link_elem.set('linkend', section_id)
                    fixed_count += 1
                    logger.debug(f"Fixed TOC link: {linkend} -> {section_id} (removed {element_code} suffix)")
                else:
                    # Section doesn't exist either - try to find a valid section for this chapter
                    ch_match = chapter_section_pattern.match(linkend)
                    if ch_match:
                        chapter_prefix = ch_match.group(1)
                        fallback_section = find_valid_section_for_chapter(chapter_prefix)
                        if fallback_section:
                            link_elem.set('linkend', fallback_section)
                            fixed_count += 1
                            logger.debug(f"Fixed TOC link with fallback: {linkend} -> {fallback_section}")
                        else:
                            # No valid section found - convert to phrase
                            convert_link_to_phrase(link_elem, linkend)
                    else:
                        # Can't parse chapter - convert to phrase
                        convert_link_to_phrase(link_elem, linkend)
            else:
                # Not an element ID pattern - try to find valid target or convert to phrase
                ch_match = chapter_section_pattern.match(linkend)
                if ch_match:
                    chapter_prefix = ch_match.group(1)
                    fallback_section = find_valid_section_for_chapter(chapter_prefix)
                    if fallback_section and fallback_section != linkend:
                        link_elem.set('linkend', fallback_section)
                        fixed_count += 1
                        logger.debug(f"Fixed TOC link: {linkend} -> {fallback_section}")
                    elif not fallback_section:
                        # No valid section found - convert to phrase
                        convert_link_to_phrase(link_elem, linkend)
                else:
                    # Unrecognized ID format with no valid target - convert to phrase
                    convert_link_to_phrase(link_elem, linkend)
        else:
            # linkend exists, but check if it's an element ID that should be a section
            match = element_id_pattern.match(linkend)
            if match:
                # Safety check: skip if link text suggests intentional element reference
                if not should_fix_link(link_elem):
                    return

                section_id = match.group(1)
                element_code = match.group(2)

                # Only fix if section_id also exists (prefer section over element)
                if section_id in valid_ids:
                    link_elem.set('linkend', section_id)
                    fixed_count += 1
                    logger.debug(f"Fixed TOC link: {linkend} -> {section_id} (removed {element_code} suffix)")

    # Find TOC prefaces (preface elements with role="toc")
    for preface in root_elem.iter('preface'):
        role = preface.get('role', '')
        if 'toc' not in role.lower():
            continue

        # Process all links in this TOC preface (iterate over copy to allow modification)
        for link_elem in list(preface.iter('link')):
            fix_link_in_toc(link_elem)

    # Also check orderedlist/itemizedlist elements that look like TOC
    for list_elem in root_elem.iter():
        if list_elem.tag not in ('orderedlist', 'itemizedlist'):
            continue

        role = list_elem.get('role', '')
        # Check if this is a TOC-like list
        if not ('toc' in role.lower() or 'contents' in role.lower() or 'nonetoc' in role.lower()):
            continue

        for link_elem in list(list_elem.iter('link')):
            fix_link_in_toc(link_elem)

    if fixed_count > 0:
        logger.info(f"Fixed {fixed_count} TOC links pointing to element IDs instead of section IDs")
    if converted_to_phrase_count > 0:
        logger.info(f"Converted {converted_to_phrase_count} TOC links with invalid targets to phrase elements")

    return fixed_count + converted_to_phrase_count


def fix_toc_links_by_title_match(root_elem: etree.Element) -> int:
    """
    Fix ALL TOC links by matching link text with actual section/element titles.

    This is called after all chapters are processed to ensure TOC links
    point to the correct IDs based on title matching.

    The problem being solved:
    - TOC entries like "References" might link to wrong section ID
    - This function finds the correct section by title and fixes the linkend

    Args:
        root_elem: Root element of the XML tree (book element)

    Returns:
        Number of TOC links fixed
    """
    fixed_count = 0

    # Build a map of section titles to their IDs for each container element
    # Structure: { element_id: { normalized_title: section_id } }
    section_title_map: Dict[str, Dict[str, str]] = {}

    # Build element title map for all top-level elements (chapters, parts, preface, appendix, etc.)
    element_title_map: Dict[str, str] = {}  # normalized_title -> element_id

    # Global title map for fallback matching across all elements
    global_title_map: Dict[str, str] = {}  # normalized_title -> element_id

    # Element types that can appear at top level and have titles
    top_level_elements = ['chapter', 'preface', 'appendix', 'part', 'index', 'glossary',
                          'dedication', 'colophon', 'bibliography', 'article', 'reference']

    # First pass: collect all element and section titles with IDs
    for elem_type in top_level_elements:
        for element in root_elem.iter(elem_type):
            element_id = element.get('id', '')
            if not element_id:
                continue

            # Get element title
            title_elem = element.find('title')
            if title_elem is not None:
                title_text = ''.join(title_elem.itertext()).strip()
                if title_text:
                    normalized_title = ' '.join(title_text.lower().split())
                    element_title_map[normalized_title] = element_id
                    global_title_map[normalized_title] = element_id

            section_title_map[element_id] = {}

            # Find all sections (sect1 through sect5, and section)
            for section_tag in ['sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section']:
                for section in element.iter(section_tag):
                    section_id = section.get('id')
                    if not section_id:
                        continue

                    # Get the section title
                    sec_title_elem = section.find('title')
                    if sec_title_elem is not None:
                        sec_title_text = ''.join(sec_title_elem.itertext()).strip()
                        if sec_title_text:
                            # Normalize the title for matching (lowercase, remove extra whitespace)
                            normalized_sec_title = ' '.join(sec_title_text.lower().split())
                            section_title_map[element_id][normalized_sec_title] = section_id
                            global_title_map[normalized_sec_title] = section_id

    # Legacy alias for backward compatibility
    chapter_title_map = element_title_map

    # Second pass: fix ALL links in TOC-like structures
    # Look for itemizedlist elements that appear to be TOC
    for itemizedlist in root_elem.iter('itemizedlist'):
        role = itemizedlist.get('role', '')
        mark = itemizedlist.get('mark', '')

        # Check if this looks like a TOC list (role contains 'contents' or 'toc', or mark is 'none')
        is_toc_list = ('contents' in role.lower() or
                       'toc' in role.lower() or
                       mark == 'none')

        if not is_toc_list:
            continue

        # Process link elements in this TOC list
        for link_elem in itemizedlist.iter('link'):
            linkend = link_elem.get('linkend', '')
            link_text = (link_elem.text or '').strip()

            if not link_text:
                continue

            # Normalize link text for matching
            normalized_link_text = ' '.join(link_text.lower().split())

            # Extract element from linkend (first 6 chars like 'ch0001', 'pt0001')
            if len(linkend) >= 6:
                target_element = linkend[:6]
            else:
                target_element = linkend

            # Case 1: Element-only linkend (6 chars like 'ch0001', 'pt0001', 'pr0001')
            if len(linkend) == 6 and re.match(r'^[a-z]{2}\d{4}$', linkend):
                # Check if link text matches any element title
                if normalized_link_text in element_title_map:
                    correct_element = element_title_map[normalized_link_text]
                    if correct_element != target_element:
                        link_elem.set('linkend', correct_element)
                        fixed_count += 1
                        logger.debug(f"Fixed element TOC link '{link_text}': {linkend} -> {correct_element}")
                continue

            # Case 2: Section linkend (11+ chars like 'ch0001s0001')
            # Check if we have section data for this element
            if target_element in section_title_map:
                element_sections = section_title_map[target_element]
                if normalized_link_text in element_sections:
                    correct_section_id = element_sections[normalized_link_text]
                    if linkend != correct_section_id:
                        link_elem.set('linkend', correct_section_id)
                        fixed_count += 1
                        logger.debug(f"Fixed section TOC link '{link_text}': {linkend} -> {correct_section_id}")
                    continue

            # Fallback: try global title map for unmatched links
            # SAFETY: Only use global fallback if the resolved ID belongs to the same
            # target element to avoid cross-chapter title collisions (e.g., multiple
            # chapters having sections titled "Introduction")
            if normalized_link_text in global_title_map:
                correct_id = global_title_map[normalized_link_text]
                # Verify the resolved ID belongs to the target element
                if correct_id.startswith(target_element) and correct_id != linkend:
                    link_elem.set('linkend', correct_id)
                    fixed_count += 1
                    logger.debug(f"Fixed TOC link via global match '{link_text}': {linkend} -> {correct_id}")

    # Also fix links in toc element (the formal TOC structure)
    for toc in root_elem.iter('toc'):
        for link_elem in toc.iter('link'):
            linkend = link_elem.get('linkend', '')
            link_text = (link_elem.text or '').strip()

            if not link_text:
                continue

            normalized_link_text = ' '.join(link_text.lower().split())

            # Extract element ID from linkend (first 6 chars like 'ch0001', 'pt0001')
            if len(linkend) >= 6:
                target_element = linkend[:6]
            else:
                target_element = linkend

            # Handle element-only linkends (6 chars like 'ch0001', 'pt0001', 'pr0001')
            if len(linkend) == 6 and re.match(r'^[a-z]{2}\d{4}$', linkend):
                # Try element title map first
                if normalized_link_text in element_title_map:
                    correct_element = element_title_map[normalized_link_text]
                    if correct_element != target_element:
                        link_elem.set('linkend', correct_element)
                        fixed_count += 1
                        logger.debug(f"Fixed element TOC link '{link_text}': {linkend} -> {correct_element}")
                continue

            # Handle section linkends (11+ chars like 'ch0001s0001')
            if len(linkend) < 11:
                # Short linkend that couldn't be matched - try global fallback
                # SAFETY: Only use if resolved ID belongs to target element
                if normalized_link_text in global_title_map:
                    correct_id = global_title_map[normalized_link_text]
                    if correct_id.startswith(target_element) and correct_id != linkend:
                        link_elem.set('linkend', correct_id)
                        fixed_count += 1
                        logger.debug(f"Fixed TOC link via global match '{link_text}': {linkend} -> {correct_id}")
                continue

            # Try to find in specific element's sections first
            if target_element in section_title_map:
                element_sections = section_title_map[target_element]
                if normalized_link_text in element_sections:
                    correct_section_id = element_sections[normalized_link_text]
                    if linkend != correct_section_id:
                        link_elem.set('linkend', correct_section_id)
                        fixed_count += 1
                        logger.debug(f"Fixed section TOC link '{link_text}': {linkend} -> {correct_section_id}")
                    continue

            # Fallback: try global title map
            # SAFETY: Only use if resolved ID belongs to target element to avoid
            # cross-chapter title collisions (e.g., "Introduction" in multiple chapters)
            if normalized_link_text in global_title_map:
                correct_id = global_title_map[normalized_link_text]
                if correct_id.startswith(target_element) and correct_id != linkend:
                    link_elem.set('linkend', correct_id)
                    fixed_count += 1
                    logger.debug(f"Fixed TOC link via global fallback '{link_text}': {linkend} -> {correct_id}")

    if fixed_count > 0:
        logger.info(f"Fixed {fixed_count} TOC links by title matching")

    return fixed_count


def sanitize_anchor_id(raw_id: str, chapter_id: str, element_type: str = 'anchor',
                       sect1_id: str = None, source_file: str = "") -> str:
    """
    Convert a raw ID to compliant format with consistent mapping.

    This handles legacy IDs from source EPUB and ensures the same source ID
    always maps to the same generated ID (important for cross-references).

    Args:
        raw_id: Original ID from source
        chapter_id: Current chapter ID
        element_type: Type of element (figure, table, anchor, para, etc.)
                     NOTE: Section elements (sect1-sect5) are NOT supported.
                     Use generate_section_id() or next_available_sect1_id() instead.
        sect1_id: Optional sect1 ID
        source_file: Original XHTML file name for traceability

    Returns:
        Sanitized ID with proper prefix and element code

    Raises:
        ValueError: If element_type is a section type (sect1-sect5).
    """
    # CRITICAL: Section elements must NOT use this function
    if element_type.lower() in {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section'}:
        raise ValueError(
            f"sanitize_anchor_id() cannot be used for section elements (got '{element_type}'). "
            f"Use generate_section_id() or next_available_sect1_id() instead."
        )
    # Handle BeautifulSoup returning list for multi-valued attributes
    if isinstance(raw_id, list):
        raw_id = raw_id[0] if raw_id else ''

    if not raw_id:
        return generate_element_id(chapter_id, element_type, sect1_id)

    # Check if we've already mapped this ID.
    # IMPORTANT: Source EPUBs can re-use the same HTML id multiple times in a chapter.
    # The first occurrence keeps the stable mapping for link resolution; subsequent
    # occurrences get a new unique ID to keep the output DTD-valid.
    existing = get_mapped_id(chapter_id, raw_id)
    if existing:
        if not _is_id_used(chapter_id, existing):
            _register_generated_id(chapter_id, existing)
            return existing

        # Collision: same source ID encountered again; generate a new unique ID.
        generated_id = generate_element_id(chapter_id, element_type, sect1_id)
        return generated_id

    # Generate new ID with proper element type
    generated_id = generate_element_id(chapter_id, element_type, sect1_id)

    # Register the mapping for consistent linkend resolution
    # Include element_type and source_file for proper tracking
    register_id_mapping(chapter_id, raw_id, generated_id, element_type, source_file)

    return generated_id


def convert_body_to_docbook(body, parent_elem: etree.Element,
                           doc_path: str, chapter_id: str,
                           mapper: ReferenceMapper,
                           toc_depth_map: Optional[Dict[str, int]] = None) -> None:
    """
    Convert HTML body content to DocBook elements.

    Args:
        body: BeautifulSoup body element
        parent_elem: Parent lxml element to append to
        doc_path: XHTML document path
        chapter_id: Current chapter ID
        mapper: ReferenceMapper instance
        toc_depth_map: Optional dict mapping element IDs to their TOC depth.
                       Used to determine section hierarchy (sect1, sect2, etc.)
    """
    section_stack = []  # Track section hierarchy
    figure_counter = {'count': 0, 'sidebar': 0}  # sidebar count added here to avoid changing function signatures
    table_counter = {'count': 0}
    # Section counters for spec-compliant ID pattern: ch0001s0001 (1-based)
    # Each level gets a counter, IDs encode hierarchical position
    # sect1 IDs are ch####s####; sect2+ are emitted as {sect1_id}{code}{sequence}
    section_counters = _init_section_counters()

    for elem in body.children:
        if isinstance(elem, NavigableString):
            # Direct text content
            text = str(elem)
            if text and text.strip() and parent_elem.tag != 'title':
                # Wrap in para if not empty
                if len(text) > 0:
                    para = validated_subelement(parent_elem if not section_stack else section_stack[-1][1], 'para')
                    para.text = text
        elif isinstance(elem, Tag):
            convert_element(elem, parent_elem, section_stack, doc_path, chapter_id,
                          mapper, figure_counter, table_counter, toc_depth_map, section_counters)

    # Close any remaining open sections at the end and ensure they have content
    while section_stack:
        closed_section = section_stack.pop()[1]
        ensure_section_has_content(closed_section)


def convert_element(elem: Tag, parent_elem: etree.Element, section_stack: List,
                   doc_path: str, chapter_id: str, mapper: ReferenceMapper,
                   figure_counter: Dict, table_counter: Dict,
                   toc_depth_map: Optional[Dict[str, int]] = None,
                   section_counters: Optional[Dict[str, int]] = None,
                   in_sidebar: bool = False) -> None:
    """
    Convert a single HTML element to DocBook.

    Args:
        elem: BeautifulSoup Tag
        parent_elem: Parent lxml element
        section_stack: Stack of (level, section_element) tuples
        doc_path: Document path
        chapter_id: Chapter ID
        mapper: Reference mapper
        figure_counter: Figure counter dict
        table_counter: Table counter dict
        toc_depth_map: Optional dict mapping element IDs to their TOC depth.
                       Used to determine section hierarchy (sect1, sect2, etc.)
                       based on TOC structure instead of HTML heading levels.
        section_counters: Dict tracking section counters for each level for ID generation.
        in_sidebar: If True, headings are converted to bridgehead instead of sections
                    (sections are not allowed inside sidebar per DTD).
    """
    # Initialize section counters if not provided
    if section_counters is None:
        section_counters = _init_section_counters()
    tag_name = elem.name

    # Initialize current_sect1 at function scope to avoid UnboundLocalError
    # Individual handlers may override this with fresh lookup if needed
    current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
    # Skip hidden elements - these are not meant to be rendered in the content flow
    # The HTML hidden attribute indicates the element is not relevant to the current state
    if elem.get('hidden') is not None:
        return

    # Skip navigation metadata elements that should not be part of content
    # - page-list: Maps print page numbers (metadata, not content)
    # - landmarks: Navigation landmarks (metadata, not content)
    if tag_name == 'nav':
        epub_type = elem.get('epub:type', '') or ''
        role_attr = elem.get('role', '') or ''
        if any(nav_type in epub_type.lower() for nav_type in ['page-list', 'pagelist', 'landmarks']):
            return
        if role_attr.lower() == 'doc-pagelist':
            return

    def _section_has_meaningful_content(section: etree.Element) -> bool:
        """
        True if section has content beyond structural boilerplate.

        We ignore:
        - <title>
        - <anchor>
        - empty <para> nodes that exist only for DTD compliance
        """
        for child in list(section):
            if child.tag in ('title', 'anchor'):
                continue
            if child.tag == 'para':
                txt = (child.text or '').strip()
                if not txt and len(child) == 0:
                    continue
            return True
        return False

    def _heading_exclude_elements(heading_elem: Tag) -> set:
        """
        Build a set of BeautifulSoup elements to exclude when extracting title content.
        We exclude:
        - pagebreak spans (handled as anchors outside of title)
        - anchor-only <a id=...> targets with no meaningful href (handled as anchors outside of title)
        """
        excluded = set()

        # Pagebreak spans
        for span_elem in heading_elem.find_all('span'):
            epub_type = span_elem.get('epub:type', '') or span_elem.get('data-type', '')
            role_attr = span_elem.get('role', '')
            if (epub_type and 'pagebreak' in epub_type.lower()) or (role_attr and 'pagebreak' in role_attr.lower()):
                excluded.add(span_elem)

        # Anchor-only <a> markers (no meaningful href)
        for a_elem in heading_elem.find_all('a'):
            a_id = a_elem.get('id') or a_elem.get('name')
            href = a_elem.get('href')
            has_meaningful_href = href is not None and href.strip() not in ('', '#')
            if a_id and not has_meaningful_href:
                excluded.add(a_elem)

        return excluded

    # Headings create sections
    # Convention:
    # - h1: chapter heading (updates <chapter><title>, does NOT create a section)
    # - h2: sect1
    # - h3: sect2
    # - h4: sect3
    # - h5: sect4
    # - h6: sect5
    if tag_name in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
        # Skip headings that have already been used as sidebar titles
        # This prevents duplicate title+bridgehead when heading is nested in a section inside aside
        if section_counters is not None:
            skip_headings = section_counters.get('_skip_sidebar_title_headings', set())
            if id(elem) in skip_headings:
                return  # Already used as sidebar title, skip processing

        # Level from HTML heading tag (h1→1, h2→2, h3→3, etc.)
        html_level = int(tag_name[1])
        title_text = extract_text(elem)

        if not title_text:
            return

        elem_id = elem.get('id')
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None

        # h1 is the chapter heading: set chapter title and do not create a section
        # BUT: if we're inside a container element (sidebar, blockquote, figure, footnote,
        # listitem, admonition, etc.), h1 should become a bridgehead or para, not update chapter title.
        # This prevents "Key Points" boxes and other nested h1s from overwriting the real title.

        # Container elements where headings become BRIDGEHEAD (DTD allows bridgehead)
        # These have genobj.class or explicit bridgehead in their content model
        bridgehead_allowed_containers = {
            # Sidebars and admonitions (admon.mix has bridgehead explicitly)
            'sidebar', 'note', 'important', 'warning', 'caution', 'tip',
            # blockquote uses component.mix which has genobj.class
            'blockquote',
            # listitem uses component.mix
            'listitem',
            # Q&A structures use qandaset.mix which has genobj.class
            'qandaentry', 'question', 'answer', 'qandadiv',
            # Procedure structures use component.mix
            'step', 'substep', 'stepalternatives',
            # Other containers using component.mix or similar
            'abstract', 'highlights', 'msgset', 'msgentry', 'msg', 'msgmain', 'msgsub', 'msgrel',
            'callout', 'revdescription',
        }

        # Container elements where headings become PARA+EMPHASIS (DTD forbids bridgehead)
        # These do NOT have genobj.class in their content model
        bridgehead_forbidden_containers = {
            # figure.mix does NOT include bridgehead
            'figure', 'informalfigure',
            # example.mix does NOT include genobj.class
            'example', 'informalexample',
            # footnote.mix does NOT include genobj.class
            'footnote', 'annotation',
            # tabentry.mix does NOT include genobj.class
            'entry', 'entrytbl', 'thead', 'tbody', 'tfoot', 'row', 'td', 'th',
            # glossdef.mix does NOT include genobj.class
            'glossentry', 'glossdef',
            # bibliocomponent.mix does NOT include genobj.class
            'biblioentry', 'bibliomixed',
            # varlistentry content is term+listitem, term doesn't allow bridgehead
            'varlistentry',
            # indexdivcomponent.mix does NOT include bridgehead
            'indexdiv',
            # index content gets reorganized into indexdivs, so prevent bridgehead there too
            'index',
            # Other containers without bridgehead support
            'epigraph', 'formalpara', 'simplesect',
        }

        # All containers combined
        container_tags = bridgehead_allowed_containers | bridgehead_forbidden_containers

        def _is_inside_container(elem):
            """Check if we're inside a container element where h1 shouldn't be chapter title.
            Returns tuple: (is_inside_container, allows_bridgehead)"""
            current = elem
            while current is not None:
                tag = getattr(current, 'tag', None)
                if tag in bridgehead_allowed_containers:
                    return (True, True)
                if tag in bridgehead_forbidden_containers:
                    return (True, False)
                # Stop at chapter-level elements - we've reached the top
                if tag in {'chapter', 'preface', 'appendix', 'dedication', 'glossary',
                          'bibliography', 'index', 'colophon', 'acknowledgments', 'part', 'book'}:
                    return (False, True)  # Not in container, bridgehead would be allowed at chapter level
                current = current.getparent() if hasattr(current, 'getparent') else None
            return (False, True)

        # Check both the in_sidebar flag AND the XML tree structure
        container_check = _is_inside_container(parent_elem)
        is_in_container = in_sidebar or container_check[0]
        allows_bridgehead = container_check[1] if not in_sidebar else True  # in_sidebar always allows bridgehead

        if html_level == 1 and not is_in_container:
            # Only the FIRST H1 heading sets the chapter/preface title.
            # Subsequent H1s (e.g., "Activities" sections in some publishers)
            # are treated as sect1 headings so they don't overwrite the real
            # chapter title.
            if section_counters.get('_h1_title_set'):
                # Treat subsequent H1 as sect1 (same level as H2) —
                # fall through to the section-creation code below
                pass
            else:
                section_counters['_h1_title_set'] = True

                # Close any open sections before setting chapter title
                while section_stack:
                    closed_section = section_stack.pop()[1]
                    ensure_section_has_content(closed_section)

                # Find the enclosing content unit element (chapter/preface/appendix/etc.)
                root_unit = parent_elem
                content_unit_tags = {'chapter', 'preface', 'appendix', 'dedication', 'glossary',
                                     'bibliography', 'index', 'colophon', 'acknowledgments'}
                while root_unit is not None and getattr(root_unit, 'tag', None) not in content_unit_tags:
                    root_unit = root_unit.getparent() if hasattr(root_unit, 'getparent') else None
                root_unit = root_unit if root_unit is not None else parent_elem

                # Update (or create) the title
                root_title = root_unit.find('title')
                if root_title is None:
                    root_title = validated_subelement(root_unit, 'title')
                # Preserve inline formatting in the heading
                # Clear any existing content
                root_title.text = None
                for child in list(root_title):
                    root_title.remove(child)
                extract_inline_content(
                    elem,
                    root_title,
                    doc_path,
                    chapter_id,
                    mapper,
                    section_parent=root_unit,
                    figure_counter=figure_counter,
                    exclude_elements=_heading_exclude_elements(elem),
                    section_counters=section_counters,
                )
                # Register anchor IDs from the heading to point to this chapter/section.
                # Register the heading element's own ID (e.g., <h1 id="head-2-239">)
                # so TOC links targeting this heading resolve to the chapter root.
                h1_elem_id = elem.get('id')
                if h1_elem_id:
                    register_id_mapping(chapter_id, h1_elem_id, root_unit.get('id', chapter_id))
                _register_label_anchor_ids(elem, chapter_id, root_unit.get('id', chapter_id), root_unit.tag, allow_any_anchor=True)
                for marker_id in _get_preceding_span_marker_ids(elem):
                    register_id_mapping(chapter_id, marker_id, root_unit.get('id', chapter_id))

                # Note: IDs, nested anchor targets, and pagebreak markers are intentionally
                # not preserved as anchor elements. R2 XSL has the anchor template output
                # commented out (html.xsl), so anchor elements produce no HTML output.

                return

        # DTD Constraint: Sections (sect1-sect5) are NOT allowed inside container elements
        # (sidebar, blockquote, figure, footnote, listitem, admonitions, etc.)
        # Convert headings to bridgehead OR para+emphasis depending on DTD constraints
        if is_in_container:
            # Note: IDs, nested anchor targets, and pagebreak markers are intentionally
            # not preserved as anchor elements. R2 XSL has the anchor template output
            # commented out (html.xsl), so anchor elements produce no HTML output.

            if allows_bridgehead:
                # Create bridgehead - allowed in sidebar.mix, component.mix, admon.mix, qandaset.mix
                bridgehead = validated_subelement(parent_elem, 'bridgehead')
                bridgehead.text = None
                for child in list(bridgehead):
                    bridgehead.remove(child)
                extract_inline_content(
                    elem,
                    bridgehead,
                    doc_path,
                    chapter_id,
                    mapper,
                    section_parent=parent_elem,
                    figure_counter=figure_counter,
                    exclude_elements=_heading_exclude_elements(elem),
                    section_counters=section_counters,
                )
            else:
                # DTD forbids bridgehead here (figure, example, footnote, table cells, etc.)
                # Convert to para with emphasis instead
                para = validated_subelement(parent_elem, 'para')
                emphasis = validated_subelement(para, 'emphasis', role='strong')
                extract_inline_content(
                    elem,
                    emphasis,
                    doc_path,
                    chapter_id,
                    mapper,
                    section_parent=parent_elem,
                    figure_counter=figure_counter,
                    exclude_elements=_heading_exclude_elements(elem),
                    section_counters=section_counters,
                )
            return

        # DTD Compliance: Sections are NOT allowed in dedication or toc
        # Convert heading to emphasized para instead
        if not _is_valid_element_for_parent('sect1', parent_elem, elem_id):
            # Convert to para with emphasis for dedication
            # Note: Para IDs are not preserved - R2 XSL uses the anchor template for
            # para IDs, but that template output is commented out in html.xsl.
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            emphasis = validated_subelement(para, 'emphasis')
            emphasis.set('role', 'bold')
            emphasis.text = title_text
            return

        # Try to get level from TOC depth map if available
        toc_level = None
        if toc_depth_map and elem_id:
            # Try various lookup formats
            # 1. Plain ID
            if elem_id in toc_depth_map:
                toc_level = toc_depth_map[elem_id]
            # 2. Sanitized ID format (ch0001s0001sanitizedid)
            else:
                sanitized_elem_id = sanitize_id(elem_id)
                # Prefer the 1-based sect1 prefix; keep s0000 as a legacy fallback key.
                prefixed_candidates = (
                    f"{chapter_id}s{1:04d}{sanitized_elem_id}",
                    f"{chapter_id}s{0:04d}{sanitized_elem_id}",
                )
                for prefixed_id in prefixed_candidates:
                    if prefixed_id in toc_depth_map:
                        toc_level = toc_depth_map[prefixed_id]
                        break

        # Use TOC level if found, otherwise fall back to HTML heading level
        # With our convention, sections start at h2:
        # Mapping: h2 → sect1 (level 1), h3 → sect2 (level 2), h4 → sect3 (level 3), etc.
        # TOC depth is also 1-based for sections: depth 1 = sect1, depth 2 = sect2
        if toc_level is not None:
            # Normalize using the TOC depth of the enclosing content unit.
            #
            # IMPORTANT:
            # NCX hierarchies often include wrappers above chapters (e.g., a top-level "Book"
            # navPoint, then Part, then Chapter). In that case, a first in-chapter heading can
            # appear at TOC depth 4, and naively subtracting 1 would incorrectly yield sect3.
            #
            # We therefore compute the TOC depth of the current chapter file and subtract that,
            # so the first heading section becomes sect1 (level=1).
            chapter_toc_depth = 1
            if toc_depth_map:
                chapter_toc_depth = (
                    toc_depth_map.get(f"{chapter_id}.xml")
                    or toc_depth_map.get(chapter_id)
                    or 1
                )
            level = max(1, toc_level - chapter_toc_depth)
            logger.debug(
                f"Using TOC depth {toc_level} (chapter depth {chapter_toc_depth}) for heading '{title_text}' (id={elem_id})"
            )
        else:
            level = max(1, html_level - 1)
            if elem_id:
                logger.debug(f"No TOC entry for id={elem_id}, using HTML heading level {level}")

        # Guardrail: prevent skipping section levels (e.g., sect3 directly under sect1),
        # which violates the DTD content model. If we detect a jump of more than 1 level
        # deeper than the current open section, clamp to at most one deeper than the parent.
        if level > 1:
            parent_level = section_stack[-1][0] if section_stack else 0
            if level > parent_level + 1:
                logger.debug(
                    f"Clamping heading level {level} to {parent_level + 1} to avoid invalid section nesting "
                    f"(chapter={chapter_id}, heading_id={elem_id})"
                )
                level = parent_level + 1

        # Close deeper sections only (strictly deeper than the new heading).
        # We handle same-level headings below so we can merge empty headings into one section.
        while section_stack and section_stack[-1][0] > level:
            closed_section = section_stack.pop()[1]
            ensure_section_has_content(closed_section)

        # Same-level heading handling:
        # If the currently-open section at this level has no meaningful content yet,
        # treat this new heading as a bridgehead inside the same section (prevents
        # "heading-only" sect1 files downstream).
        if section_stack and section_stack[-1][0] == level:
            current_section = section_stack[-1][1]
            if not _section_has_meaningful_content(current_section):
                # Note: IDs, span markers, and pagebreak markers are intentionally
                # not preserved as anchor elements. R2 XSL has the anchor template
                # output commented out (html.xsl), so anchor elements produce no HTML output.

                bridgehead = validated_subelement(current_section, 'bridgehead')
                bridgehead.text = None
                extract_inline_content(
                    elem,
                    bridgehead,
                    doc_path,
                    chapter_id,
                    mapper,
                    section_parent=current_section,
                    figure_counter=figure_counter,
                    exclude_elements=_heading_exclude_elements(elem),
                    section_counters=section_counters,
                )
                return

            # Otherwise, close the current same-level section and start a new one.
            closed_section = section_stack.pop()[1]
            ensure_section_has_content(closed_section)

        # Create section element matching the level (sect1/sect2/...)
        section = etree.Element(_section_tag_for_level(level))

        # Generate spec-compliant section ID (ch0001s0001 pattern, 1-based)
        # This ID encodes the hierarchical position in the chapter
        section_id = id_gen_section_id(chapter_id, level, section_counters)
        section.set('id', section_id)

        # Track current sect1 context so sect2+ IDs can be correctly prefixed
        if level == 1:
            section_counters['_current_sect1_id'] = section_id

        # IMPORTANT: Register mapping from original elem_id to section_id for TOC link resolution
        # This allows TOC entries and cross-references to find the section by original ID
        if elem_id:
            register_id_mapping(chapter_id, elem_id, section_id)
            logger.debug(f"Registered section ID mapping: {elem_id} -> {section_id}")
        # Also register the parent container's ID (e.g., <section id="Sec1"> wrapping <h2 id="Sec1Heading">)
        # EPUB structure often has: <section id="Sec1"><h2 id="Sec1Heading">Title</h2>...</section>
        # TOC links point to the section ID (Sec1), not the heading ID (Sec1Heading)
        parent_tag = elem.parent
        if parent_tag and hasattr(parent_tag, 'name') and parent_tag.name in ('section', 'div', 'article', 'aside', 'nav'):
            parent_id = parent_tag.get('id')
            if parent_id and parent_id != elem_id:
                register_id_mapping(chapter_id, parent_id, section_id)
                logger.debug(f"Registered parent container ID mapping: {parent_id} -> {section_id}")
        # Register anchor IDs from the heading to point to this section.
        _register_label_anchor_ids(elem, chapter_id, section_id, 'section', allow_any_anchor=True)
        for marker_id in _get_preceding_span_marker_ids(elem):
            register_id_mapping(chapter_id, marker_id, section_id)

        # Preserve CSS class from heading as role attribute on section
        css_class = elem.get('class')
        if css_class:
            if isinstance(css_class, list):
                css_class = ' '.join(css_class)
            section.set('role', css_class)

        # DTD requires title FIRST (after optional sect1info)
        title_elem = validated_subelement(section, 'title')
        # Preserve inline formatting in heading titles while keeping anchors outside of <title>
        title_elem.text = None
        extract_inline_content(
            elem,
            title_elem,
            doc_path,
            chapter_id,
            mapper,
            section_parent=section,
            figure_counter=figure_counter,
            exclude_elements=_heading_exclude_elements(elem),
            section_counters=section_counters,
        )

        # Note: Original elem_id, span markers, nested anchor targets, and pagebreak markers
        # are intentionally not preserved as anchor elements. R2 XSL has the anchor template
        # output commented out (html.xsl), so anchor elements produce no HTML output.
        # The section ID (ch0001s0001 pattern) is the only ID that matters for navigation.

        # Add to parent with validation
        if section_stack:
            validated_append(section_stack[-1][1], section, f"heading {tag_name} -> {_section_tag_for_level(level)}")
        else:
            validated_append(parent_elem, section, f"heading {tag_name} -> {_section_tag_for_level(level)}")

        section_stack.append((level, section))

    # Paragraphs (handles both HTML <p> and DocBook <para> elements in XHTML)
    elif tag_name in ['p', 'para']:
        current_parent = section_stack[-1][1] if section_stack else parent_elem
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None

        # Check if this is a p.fig containing only an image - convert to figure at section level
        css_class = elem.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)

        # Get existing role attribute if present (e.g., from DocBook-style XHTML)
        existing_role = elem.get('role', '')

        img = elem.find('img')
        # Check if p.fig contains only an image (possibly with whitespace)
        has_only_image = img is not None and all(
            isinstance(child, NavigableString) and not str(child).strip()
            or child == img
            for child in elem.children
        )

        # Split CSS class into list for exact matching
        class_list = css_class.split() if css_class else []
        class_list_lower = [c.lower() for c in class_list]

        # Check for heading CSS classes commonly used in EPUBs
        # These should create sections instead of paragraphs
        # ah = A-head (sect1), bh = B-head (sect2), ch = C-head (sect3), dh = D-head (sect4)
        heading_class_map = {
            'ah': 1, 'a-head': 1, 'ahead': 1, 'head-a': 1, 'heading-a': 1,
            'bh': 2, 'b-head': 2, 'bhead': 2, 'head-b': 2, 'heading-b': 2,
            'ch': 3, 'c-head': 3, 'chead': 3, 'head-c': 3, 'heading-c': 3,
            'dh': 4, 'd-head': 4, 'dhead': 4, 'head-d': 4, 'heading-d': 4,
        }

        heading_level = None
        for cls in class_list_lower:
            if cls in heading_class_map:
                heading_level = heading_class_map[cls]
                break

        if heading_level is not None:
            # DTD Compliance: Sections are NOT allowed in dedication or toc
            # Convert heading to emphasized para instead
            if not _is_valid_element_for_parent('sect1', parent_elem, elem.get('id')):
                # Convert to para with emphasis for dedication
                # Note: Para IDs are not preserved - R2 XSL uses the anchor template
                # for para IDs, but that template output is commented out in html.xsl.
                para = validated_subelement(current_parent, 'para')
                emphasis = validated_subelement(para, 'emphasis')
                emphasis.set('role', 'bold')
                emphasis.text = extract_text(elem)
                return

            # This paragraph with heading class should create a section
            title_text = extract_text(elem)
            if title_text:
                # Guardrail: prevent skipping section levels (e.g., sect3 directly under chapter),
                # which violates the DTD content model. Clamp to at most one deeper than the parent.
                if heading_level > 1:
                    parent_level = section_stack[-1][0] if section_stack else 0
                    if heading_level > parent_level + 1:
                        logger.debug(
                            f"Clamping CSS heading level {heading_level} to {parent_level + 1} to avoid invalid section nesting "
                            f"(chapter={chapter_id}, css_class={css_class})"
                        )
                        heading_level = parent_level + 1

                # Close deeper sections only.
                while section_stack and section_stack[-1][0] > heading_level:
                    closed_section = section_stack.pop()[1]
                    ensure_section_has_content(closed_section)

                # Same-level heading handling: merge empty same-level sections.
                if section_stack and section_stack[-1][0] == heading_level:
                    current_section = section_stack[-1][1]
                    if not _section_has_meaningful_content(current_section):
                        # Note: IDs are not preserved as anchors - R2 XSL has the anchor
                        # template output commented out (html.xsl).

                        bridgehead = validated_subelement(current_section, 'bridgehead')
                        bridgehead.text = None
                        extract_inline_content(
                            elem,
                            bridgehead,
                            doc_path,
                            chapter_id,
                            mapper,
                            section_parent=current_section,
                            figure_counter=figure_counter,
                            exclude_elements=_heading_exclude_elements(elem),
                            section_counters=section_counters,
                        )
                        return  # don't create a new section

                    closed_section = section_stack.pop()[1]
                    ensure_section_has_content(closed_section)

                # Create section element matching the level (sect1/sect2/...)
                section = etree.Element(_section_tag_for_level(heading_level))

                # Generate spec-compliant section ID (ch0001s0001 pattern, 1-based)
                section_id = id_gen_section_id(chapter_id, heading_level, section_counters)
                section.set('id', section_id)

                # Track current sect1 context so sect2+ IDs can be correctly prefixed
                if heading_level == 1:
                    section_counters['_current_sect1_id'] = section_id

                # Get elem_id early so we can register mapping before other processing
                elem_id = elem.get('id')

                # IMPORTANT: Register mapping from original elem_id to section_id for TOC link resolution
                if elem_id:
                    register_id_mapping(chapter_id, elem_id, section_id)
                    logger.debug(f"Registered section ID mapping (from para): {elem_id} -> {section_id}")
                # Also register parent container ID (e.g., <section id="Sec1"> wrapping heading para)
                parent_tag = elem.parent
                if parent_tag and hasattr(parent_tag, 'name') and parent_tag.name in ('section', 'div', 'article', 'aside', 'nav'):
                    parent_id = parent_tag.get('id')
                    if parent_id and parent_id != elem_id:
                        register_id_mapping(chapter_id, parent_id, section_id)
                        logger.debug(f"Registered parent container ID mapping (from para): {parent_id} -> {section_id}")

                # Preserve CSS class as role attribute
                if css_class:
                    section.set('role', css_class)

                # DTD requires title FIRST (after optional sect1info)
                title_elem = validated_subelement(section, 'title')
                # Extract inline content into title (preserves formatting)
                extract_inline_content(elem, title_elem, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

                # Note: Original elem_id, nested anchor targets, and pagebreak markers are
                # intentionally not preserved as anchor elements. R2 XSL has the anchor template
                # output commented out (html.xsl), so anchor elements produce no HTML output.
                # The section ID (ch0001s0001 pattern) is the only ID that matters for navigation.

                # Add to parent with validation
                if section_stack:
                    validated_append(section_stack[-1][1], section, f"para.heading -> {_section_tag_for_level(heading_level)}")
                else:
                    validated_append(parent_elem, section, f"para.heading -> {_section_tag_for_level(heading_level)}")

                section_stack.append((heading_level, section))
            return  # Don't process as normal paragraph

        # Check for blockquote CSS classes (including dialogue and epigraph)
        blockquote_classes = ['quot', 'quote', 'quotation', 'blockquote', 'bq',
                              'ext', 'extract', 'quotfirst', 'quotlast',
                              'extfirst', 'extlast', 'epigraph']
        # Dialogue-specific classes
        dialogue_classes = ['dialogue', 'dialog', 'speech', 'speaker', 'conversation',
                           'dia', 'spoken', 'utterance']
        is_blockquote = any(cls in class_list_lower for cls in blockquote_classes)
        is_dialogue = any(cls in class_list_lower for cls in dialogue_classes)
        is_epigraph = 'epigraph' in class_list_lower

        if is_blockquote or is_dialogue:
            # Create blockquote element
            blockquote = validated_subelement(current_parent, 'blockquote')

            # Preserve ID attribute
            elem_id = elem.get('id')
            if elem_id:
                blockquote.set('id', sanitize_anchor_id(elem_id, chapter_id, 'blockquote', current_sect1))

            # Set appropriate role based on content type
            # Dialogue gets role="dialogue", epigraph gets role="epigraph"
            # (Using blockquote with role is safer than <epigraph> due to DTD structure constraints)
            if is_dialogue:
                blockquote.set('role', 'dialogue')
            elif is_epigraph:
                blockquote.set('role', 'epigraph')
            elif css_class:
                # Preserve original CSS class as role for other blockquotes
                blockquote.set('role', css_class)

            # Create para inside blockquote and extract content
            para = validated_subelement(blockquote, 'para')
            extract_inline_content(elem, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
            return

        # Note: Sidebar/box CSS classes (exh, exs, sth, boxh, etc.) are NOT converted
        # to <sidebar> elements because DTD requires sidebar to have content after title.
        # Individual paragraphs with these classes cannot satisfy that requirement.
        # Instead, we preserve them as <para role="..."> to maintain the semantic info.
        # A post-processing step could group consecutive related paras into sidebars if needed.

        elif 'fig' in css_class and has_only_image:
            # Convert to figure element at section level
            # Check if we're inside a table cell - figure not allowed there
            in_table_cell, entry_elem = is_inside_table_cell(current_parent)

            img_src = img.get('src', '')
            if img_src:
                result = resolve_image_path(img_src, doc_path, mapper)
                if result:
                    intermediate_name, normalized_path = result

                    if in_table_cell:
                        # Inside table cell - use mediaobject directly (no figure wrapper)
                        mediaobject = validated_subelement(current_parent, 'mediaobject')
                        imageobject = validated_subelement(mediaobject, 'imageobject')
                        imagedata = validated_subelement(imageobject, 'imagedata')
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)
                        # Add alt text as textobject if present
                        alt_text = img.get('alt', '')
                        if alt_text:
                            textobject = validated_subelement(mediaobject, 'textobject')
                            # Avoid <phrase> to prevent downstream spacing issues
                            tpara = validated_subelement(textobject, 'para')
                            tpara.text = alt_text
                    else:
                        # Normal case - create figure wrapper
                        # Validate that figure is allowed in this parent type
                        if not _is_valid_element_for_parent('figure', current_parent, elem.get('id')):
                            # Skip figure in dedication - just add text description if available
                            alt_text = elem.get('alt', '')
                            if alt_text:
                                para = validated_subelement(current_parent, 'para')
                                para.text = f"[Image: {alt_text}]"
                            return
                        figure_counter['count'] += 1
                        figure = validated_subelement(current_parent, 'figure')

                        # Always set ID (use original if present, otherwise generate one)
                        elem_id = elem.get('id')
                        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
                        if elem_id:
                            figure.set('id', sanitize_anchor_id(elem_id, chapter_id, 'figure', current_sect1))
                        else:
                            figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))

                        # Create empty title - DTD allows this
                        validated_subelement(figure, 'title')

                        # Create mediaobject
                        mediaobject = validated_subelement(figure, 'mediaobject')
                        imageobject = validated_subelement(mediaobject, 'imageobject')
                        imagedata = validated_subelement(imageobject, 'imagedata')
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)

                        # Add alt text as textobject if present
                        alt_text = img.get('alt', '')
                        if alt_text:
                            textobject = validated_subelement(mediaobject, 'textobject')
                            # Avoid <phrase> to prevent downstream spacing issues
                            tpara = validated_subelement(textobject, 'para')
                            tpara.text = alt_text
                else:
                    # Could not resolve image - create placeholder
                    figure_counter['count'] += 1
                    if not in_table_cell:
                        # Validate that figure is allowed in this parent type
                        if not _is_valid_element_for_parent('figure', current_parent):
                            return  # Skip figure in dedication
                        figure = validated_subelement(current_parent, 'figure')
                        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
                        figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))
                        validated_subelement(figure, 'title')
                        mediaobject = validated_subelement(figure, 'mediaobject')
                    else:
                        mediaobject = validated_subelement(current_parent, 'mediaobject')
                    imageobject = validated_subelement(mediaobject, 'imageobject')
                    imagedata = validated_subelement(imageobject, 'imagedata')
                    imagedata.set('fileref', f"missing_{figure_counter['count']}.jpg")
        else:
            # Check if paragraph contains a nested figure - these need to be extracted out
            # HTML allows <p><figure></figure></p> but DocBook doesn't allow figure inside para
            nested_figure = elem.find('figure')
            if nested_figure is not None:
                # Paragraph contains a figure - extract content before/after and create proper figure
                # First, collect content before the figure
                before_content = []
                after_content = []
                found_figure = False

                for child in elem.children:
                    if isinstance(child, Tag) and child.name == 'figure':
                        found_figure = True
                    elif not found_figure:
                        before_content.append(child)
                    else:
                        after_content.append(child)

                # Create para for text before the figure (if any)
                # Note: Para IDs are not preserved - R2 XSL uses the anchor template
                # for para IDs, but that template output is commented out in html.xsl.
                has_before_text = any(
                    (isinstance(c, NavigableString) and str(c).strip()) or
                    (isinstance(c, Tag) and c.name not in ('figure',))
                    for c in before_content
                )
                if has_before_text:
                    para = validated_subelement(current_parent, 'para')
                    if existing_role:
                        para.set('role', existing_role)
                    elif css_class:
                        para.set('role', css_class)
                    # Process before content as inline
                    for child in before_content:
                        if isinstance(child, NavigableString):
                            text = str(child)
                            if text:
                                if len(para) == 0:
                                    para.text = (para.text or '') + text
                                else:
                                    para[-1].tail = (para[-1].tail or '') + text
                        elif isinstance(child, Tag):
                            extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

                # Process the nested figure as a proper figure element
                convert_element(nested_figure, current_parent, section_stack, doc_path, chapter_id,
                              mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

                # Create para for text after the figure (if any) - rare but handle it
                has_after_text = any(
                    (isinstance(c, NavigableString) and str(c).strip()) or
                    (isinstance(c, Tag) and c.name not in ('figure',))
                    for c in after_content
                )
                if has_after_text:
                    after_para = validated_subelement(current_parent, 'para')
                    for child in after_content:
                        if isinstance(child, NavigableString):
                            text = str(child)
                            if text:
                                if len(after_para) == 0:
                                    after_para.text = (after_para.text or '') + text
                                else:
                                    after_para[-1].tail = (after_para[-1].tail or '') + text
                        elif isinstance(child, Tag):
                            extract_inline_content(child, after_para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
            else:
                # Check if paragraph contains nested lists (ol/ul) - these need to be extracted out
                # HTML allows <p><ol>...</ol></p> but DocBook doesn't allow orderedlist inside para (C-017 fix)
                nested_list = elem.find(['ol', 'ul'])
                if nested_list is not None:
                    # Paragraph contains a list - extract content before/after and create proper list
                    before_content = []
                    list_elements = []
                    after_content = []
                    found_list = False
                    past_lists = False

                    for child in elem.children:
                        if isinstance(child, Tag) and child.name in ('ol', 'ul'):
                            found_list = True
                            past_lists = False
                            list_elements.append(child)
                        elif not found_list:
                            before_content.append(child)
                        else:
                            # Content after a list - could be followed by another list
                            after_content.append(child)

                    # Create para for text before the list (if any)
                    has_before_text = any(
                        (isinstance(c, NavigableString) and str(c).strip()) or
                        (isinstance(c, Tag) and c.name not in ('ol', 'ul'))
                        for c in before_content
                    )
                    if has_before_text:
                        para = validated_subelement(current_parent, 'para')
                        if existing_role:
                            para.set('role', existing_role)
                        elif css_class:
                            para.set('role', css_class)
                        for child in before_content:
                            if isinstance(child, NavigableString):
                                text = str(child)
                                if text:
                                    if len(para) == 0:
                                        para.text = (para.text or '') + text
                                    else:
                                        para[-1].tail = (para[-1].tail or '') + text
                            elif isinstance(child, Tag):
                                extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

                    # Process each nested list as a proper list element
                    for list_elem in list_elements:
                        convert_element(list_elem, current_parent, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

                    # Create para for text after the lists (if any)
                    has_after_text = any(
                        (isinstance(c, NavigableString) and str(c).strip()) or
                        (isinstance(c, Tag) and c.name not in ('ol', 'ul'))
                        for c in after_content
                    )
                    if has_after_text:
                        after_para = validated_subelement(current_parent, 'para')
                        for child in after_content:
                            if isinstance(child, NavigableString):
                                text = str(child)
                                if text:
                                    if len(after_para) == 0:
                                        after_para.text = (after_para.text or '') + text
                                    else:
                                        after_para[-1].tail = (after_para[-1].tail or '') + text
                            elif isinstance(child, Tag):
                                if child.name in ('ol', 'ul'):
                                    # Another list after text - process it
                                    convert_element(child, current_parent, section_stack, doc_path, chapter_id,
                                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                                else:
                                    extract_inline_content(child, after_para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                else:
                    # Normal paragraph handling (no nested figures or lists)
                    para = validated_subelement(current_parent, 'para')

                    # Note: Para IDs are not preserved - R2 XSL uses the anchor template
                    # for para IDs, but that template output is commented out in html.xsl.

                    # Preserve role attribute: use existing role if present, otherwise use CSS class
                    if existing_role:
                        para.set('role', existing_role)
                    elif css_class:
                        para.set('role', css_class)

                    # Handle inline content
                    extract_inline_content(elem, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

    # Figure elements (wrapper for images with captions)
    elif tag_name == 'figure':
        figure_counter['count'] += 1
        current_parent = section_stack[-1][1] if section_stack else parent_elem

        # Check if figure contains tables - DTD doesn't allow table inside figure
        # In this case, create tables directly (unwrap the figure)
        inner_tables = elem.find_all('table', recursive=False)  # Direct children only
        if not inner_tables:
            # Also check for tables nested inside other elements
            inner_tables = elem.find_all('table')

        if inner_tables:
            # Figure contains table(s) - convert directly to table(s)

            # Look for figcaption to get title and ID
            figcaption = elem.find('figcaption')
            figcaption_title = None
            figcaption_id = None

            if figcaption:
                # Extract ID from <p> element inside figcaption if present
                # Common pattern: <figcaption><p id="c02-tbl-0005">Table 2.1-5...</p></figcaption>
                figcaption_p = figcaption.find('p')
                if figcaption_p and figcaption_p.get('id'):
                    figcaption_id = figcaption_p.get('id')
                    # IMPORTANT: Remove the ID from the <p> element to prevent duplicate anchor
                    # creation when extract_inline_content processes the figcaption title.
                    # The ID is now on the table element - we don't want it duplicated as an anchor.
                    del figcaption_p['id']
                # Also check figcaption itself for ID
                elif figcaption.get('id'):
                    figcaption_id = figcaption.get('id')
                    # Remove ID from figcaption to prevent duplicate anchor
                    del figcaption['id']
                figcaption_title = figcaption  # Will be processed later

            # Get figure's own ID as fallback
            figure_id = elem.get('id')

            # Process ALL tables in the figure
            for table_idx, inner_table in enumerate(inner_tables):
                # Validate that table is allowed in this parent type
                if not _is_valid_element_for_parent('table', current_parent, elem.get('id')):
                    # Skip table in dedication
                    continue
                table_counter['count'] += 1
                table = validated_subelement(current_parent, 'table')

                # Determine table ID:
                # - First table gets figcaption's <p> ID, or figure ID, or auto-generated
                # - Subsequent tables get auto-generated IDs
                current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
                if table_idx == 0:
                    if figcaption_id:
                        table.set('id', sanitize_anchor_id(figcaption_id, chapter_id, 'table', current_sect1, doc_path))
                    elif figure_id:
                        table.set('id', sanitize_anchor_id(figure_id, chapter_id, 'table', current_sect1, doc_path))
                    else:
                        table.set('id', generate_element_id(chapter_id, 'table', current_sect1))
                else:
                    # Subsequent tables - check if table itself has an ID
                    table_id = inner_table.get('id')
                    if table_id:
                        table.set('id', sanitize_anchor_id(table_id, chapter_id, 'table', current_sect1, doc_path))
                    else:
                        table.set('id', generate_element_id(chapter_id, 'table', current_sect1))

                # Register anchor IDs found in caption to point to this table's ID.
                # This is done AFTER the table has its own ID set.
                if table_idx == 0:
                    if figcaption:
                        _register_label_anchor_ids(figcaption, chapter_id, table.get('id'), 'table')
                    adjacent_caption = _find_adjacent_caption_elem(elem, 'table')
                    if adjacent_caption and adjacent_caption is not figcaption:
                        _register_label_anchor_ids(adjacent_caption, chapter_id, table.get('id'), 'table')

                # Add title
                title_elem = validated_subelement(table, 'title')
                if table_idx == 0 and figcaption_title:
                    # First table gets the figcaption as title
                    # Exclude table elements to handle malformed HTML with missing </figcaption>
                    # where table content is erroneously parsed inside figcaption
                    # Pass table ID to redirect self-referencing label links to this table
                    extract_inline_content(figcaption_title, title_elem, doc_path, chapter_id, mapper,
                                         figure_counter=figure_counter,
                                         exclude_tags={'table', 'tr', 'td', 'th', 'thead', 'tbody', 'tfoot', 'caption', 'colgroup', 'col'},
                                         section_counters=section_counters,
                                         containing_element_id=table.get('id'),
                                         containing_element_type='table')
                else:
                    # Check if this table has its own caption
                    caption = inner_table.find('caption')
                    if caption:
                        extract_inline_content(caption, title_elem, doc_path, chapter_id, mapper,
                                             figure_counter=figure_counter, section_counters=section_counters,
                                             containing_element_id=table.get('id'),
                                             containing_element_type='table')
                    else:
                        # No caption — detect formal vs informal from source table ID.
                        # Springer convention: Tab1, Tab2 = formal (numeric); Taba, Tabb = informal (alpha).
                        source_table_id = inner_table.get('id', '') or elem.get('id', '') or ''
                        is_informal = bool(re.search(r'Tab[a-z]', source_table_id))

                        if is_informal:
                            # Convert to informaltable — no title needed
                            table.tag = 'informaltable'
                            table.remove(title_elem)
                        elif table_idx > 0:
                            title_elem.text = f"Table {table_counter['count']} (continued)"
                        else:
                            # Formal table — try to extract chapter-prefixed number from ID
                            tab_match = re.search(r'Tab(\d+)', source_table_id)
                            chapter_num = section_counters.get('_chapter_number', '') if section_counters else ''
                            if tab_match and chapter_num:
                                title_elem.text = f"Table {chapter_num}.{tab_match.group(1)}"
                            else:
                                title_elem.text = f"Table {table_counter['count']}"

                # Process the table content with improved span handling
                num_cols, num_rows, span_tracker = precompute_table_structure(inner_table)
                tgroup = validated_subelement(table, 'tgroup', cols=str(num_cols))

                # Add colspec elements for column spanning support
                _add_colspecs_to_tgroup(tgroup, num_cols)

                # Validate colspec count
                validate_colspec_count(tgroup, num_cols)

                tbody = validated_subelement(tgroup, 'tbody')

                table_rows = _iter_table_rows(inner_table)
                for row_idx, tr in enumerate(table_rows):
                    row = validated_subelement(tbody, 'row')
                    col_index = 1  # Track current column (1-based for CALS)
                    for td in tr.find_all(['td', 'th'], recursive=False):
                        # Skip columns occupied by rowspans from previous rows
                        if span_tracker:
                            col_index = span_tracker.get_next_free_column(row_idx, col_index)

                        entry = validated_subelement(row, 'entry')

                        # Handle rowspan/colspan -> morerows/namest/nameend
                        col_span = _set_entry_spanning(entry, td, col_index)
                        col_index += col_span  # Advance by the span width

                        # Note: td/th IDs are not preserved - R2 XSL has the anchor
                        # template output commented out (html.xsl).

                        # Check if cell contains block-level elements
                        # NOTE: Inline math images (vertical-align:middle) are NOT block elements.
                        block_elements = {'ol', 'ul', 'dl', 'p', 'div', 'blockquote', 'pre', 'table', 'figure', 'img'}
                        has_block_content = any(
                            isinstance(c, Tag) and c.name in block_elements
                            and not (c.name == 'img' and _is_inline_image(c))
                            for c in td.children
                        ) or any(
                            not _is_inline_image(img)
                            for img in td.find_all('img', recursive=True)
                        )

                        if has_block_content:
                            for child in td.children:
                                if isinstance(child, Tag):
                                    if child.name in block_elements and not (child.name == 'img' and _is_inline_image(child)):
                                        convert_element(child, entry, [], doc_path, chapter_id,
                                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                                    else:
                                        para = validated_subelement(entry, 'para')
                                        extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                                elif isinstance(child, NavigableString):
                                    text = str(child)
                                    if text and text.strip():
                                        para = validated_subelement(entry, 'para')
                                        para.text = text
                        else:
                            extract_inline_content(td, entry, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

            # Process any footnote paragraphs or other content after the tables within the figure
            # This handles <p class="tableFootnote"> elements
            for sibling in elem.children:
                if isinstance(sibling, Tag):
                    if sibling.name == 'p':
                        # Skip figcaption - already processed
                        if sibling.parent and sibling.parent.name == 'figcaption':
                            continue
                        # This is a paragraph (like tableFootnote) - add after tables
                        # Note: Para IDs are not preserved - R2 XSL uses the anchor template
                        # for para IDs, but that template output is commented out in html.xsl.
                        para = validated_subelement(current_parent, 'para')
                        # Check for tableFootnote class and add role
                        p_class = sibling.get('class', [])
                        if isinstance(p_class, list):
                            p_class = ' '.join(p_class)
                        if 'footnote' in p_class.lower() or 'note' in p_class.lower():
                            para.set('role', 'table-footnote')
                        extract_inline_content(sibling, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

            return  # Done processing figure-with-table(s)

        # Normal figure processing (with image)
        # Check if we're inside a table cell (entry) - figure not allowed there
        in_table_cell = False
        check_elem = current_parent
        while check_elem is not None:
            if check_elem.tag == 'entry':
                in_table_cell = True
                break
            check_elem = check_elem.getparent() if hasattr(check_elem, 'getparent') else None

        if in_table_cell:
            # Inside table cell - use mediaobject directly without figure wrapper
            # DTD doesn't allow <figure> inside <entry>
            img = elem.find('img')
            if img:
                img_src = img.get('src', '')
                if img_src:
                    result = resolve_image_path(img_src, doc_path, mapper)
                    if result:
                        intermediate_name, normalized_path = result
                        mediaobject = validated_subelement(current_parent, 'mediaobject')
                        imageobject = validated_subelement(mediaobject, 'imageobject')
                        imagedata = validated_subelement(imageobject, 'imagedata')
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)
                        # Add alt text as textobject
                        alt_text = img.get('alt', '')
                        if alt_text:
                            textobject = validated_subelement(mediaobject, 'textobject')
                            # Avoid <phrase> to prevent downstream spacing issues
                            tpara = validated_subelement(textobject, 'para')
                            tpara.text = alt_text
                        logger.debug(f"Created mediaobject (no figure) for table cell: {img_src}")
            return  # Done processing figure inside table cell

        # Create DocBook figure
        # Validate that figure is allowed in this parent type
        if not _is_valid_element_for_parent('figure', current_parent, elem.get('id')):
            # Skip figure in dedication - extract and add text content instead
            text_content = extract_text(elem)
            if text_content and text_content.strip():
                para = validated_subelement(current_parent, 'para')
                para.text = f"[Figure: {text_content.strip()[:100]}]"
            return
        figure = validated_subelement(current_parent, 'figure')

        # Always set ID (use original if present, otherwise generate one)
        # Handle BeautifulSoup returning list for multi-valued attributes
        elem_id = elem.get('id')
        if isinstance(elem_id, list):
            elem_id = elem_id[0] if elem_id else ''

        # Also check for ID on figcaption or nested elements - links may reference these
        figcaption_for_id = elem.find('figcaption')
        figcaption_id = None
        if figcaption_for_id:
            figcaption_id = figcaption_for_id.get('id')
            if isinstance(figcaption_id, list):
                figcaption_id = figcaption_id[0] if figcaption_id else ''
            # Also check for ID on paragraph inside figcaption
            if not figcaption_id:
                figcaption_p = figcaption_for_id.find('p')
                if figcaption_p:
                    figcaption_id = figcaption_p.get('id')
                    if isinstance(figcaption_id, list):
                        figcaption_id = figcaption_id[0] if figcaption_id else ''

        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        if elem_id:
            # Use the figure element's own ID
            generated_id = sanitize_anchor_id(elem_id, chapter_id, 'figure', current_sect1, doc_path)
            figure.set('id', generated_id)
            # NOTE: We intentionally do NOT register figcaption ID to point to the figure ID.
            # Each element should only register its own ID. Registering a figcaption ID
            # (which might be a table/equation ID like 'c16-tbl-0001') to a figure ID causes
            # linkend mismatches when the actual table is processed later. The figcaption ID
            # will be handled by _register_label_anchor_ids if appropriate.
        elif figcaption_id:
            # No figure ID - use figcaption ID only if it looks like a figure ID
            # This registers the mapping: figcaption_id -> figure's generated ID
            figcaption_id_lower = figcaption_id.lower()
            figure_keywords = ('fig', 'figure', 'image', 'img', 'photo', 'illustration')
            if any(kw in figcaption_id_lower for kw in figure_keywords):
                generated_id = sanitize_anchor_id(figcaption_id, chapter_id, 'figure', current_sect1, doc_path)
                figure.set('id', generated_id)
            else:
                # Figcaption ID doesn't look like a figure ID - generate a new one
                # This prevents mapping table/equation IDs to figure IDs
                figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))
                logger.debug(f"Generated new figure ID (figcaption ID '{figcaption_id}' doesn't look like figure ID)")
        else:
            figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))

        # Preserve role attribute: use existing role if present, otherwise use CSS class
        existing_role = elem.get('role', '')
        css_class = elem.get('class')
        if existing_role:
            figure.set('role', existing_role)
        elif css_class:
            if isinstance(css_class, list):
                css_class = ' '.join(css_class)
            figure.set('role', css_class)

        # Look for figcaption as title (DTD requires title for figure, but allows empty)
        figcaption = elem.find('figcaption')
        # Register anchor IDs found in caption to point to this figure's ID.
        # This is done AFTER the figure has its own ID set (not using caption IDs to generate figure ID).
        # The anchor IDs are registered as 'figure' type since they point to a figure element.
        if figcaption:
            _register_label_anchor_ids(figcaption, chapter_id, figure.get('id'), 'figure')
        adjacent_caption = _find_adjacent_caption_elem(elem, 'figure')
        if adjacent_caption is not None and adjacent_caption is not figcaption:
            _register_label_anchor_ids(adjacent_caption, chapter_id, figure.get('id'), 'figure')
        title_elem = validated_subelement(figure, 'title')
        if figcaption:
            # Use extract_inline_content to preserve anchor tags and hrefs
            # Exclude table elements to handle malformed HTML with missing </figcaption>
            # Pass figure ID to redirect self-referencing label links to this figure
            extract_inline_content(figcaption, title_elem, doc_path, chapter_id, mapper,
                                 figure_counter=figure_counter,
                                 exclude_tags={'table', 'tr', 'td', 'th', 'thead', 'tbody', 'tfoot', 'caption', 'colgroup', 'col'},
                                 section_counters=section_counters,
                                 containing_element_id=figure.get('id'),
                                 containing_element_type='figure')
        # Otherwise leave title empty - DTD allows empty title

        # Find and process img element
        img = elem.find('img')
        if img:
            # MediaObject
            mediaobject = validated_subelement(figure, 'mediaobject')
            imageobject = validated_subelement(mediaobject, 'imageobject')
            imagedata = validated_subelement(imageobject, 'imagedata')

            # Resolve image path
            img_src = img.get('src', '')
            if img_src:
                result = resolve_image_path(img_src, doc_path, mapper)
                if result:
                    intermediate_name, normalized_path = result
                    imagedata.set('fileref', intermediate_name)
                    mapper.add_reference(normalized_path, chapter_id)
                else:
                    imagedata.set('fileref', f"missing_{figure_counter['count']}.jpg")

            # Add alt text as textobject
            alt_text = img.get('alt', '')
            if alt_text:
                textobject = validated_subelement(mediaobject, 'textobject')
                # Avoid <phrase> to prevent downstream spacing issues
                tpara = validated_subelement(textobject, 'para')
                tpara.text = alt_text
        else:
            # Figure without image - DTD requires content, create empty mediaobject
            mediaobject = validated_subelement(figure, 'mediaobject')
            validated_subelement(mediaobject, 'imageobject')
            # Process other children inside figure, but NOT elements that would create nested figures
            # (figure.mix does NOT include figure - nested figures are invalid)
            for child in elem.children:
                if isinstance(child, Tag):
                    # Skip figcaption (already processed as title)
                    if child.name == 'figcaption':
                        continue
                    # Skip elements that would create nested figures - process them at section level instead
                    if child.name in ['figure', 'img']:
                        convert_element(child, current_parent, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                    elif child.name == 'div':
                        child_class = child.get('class', '')
                        if isinstance(child_class, list):
                            child_class = ' '.join(child_class)
                        # Skip div.figure - would create nested figure
                        if 'figure' in child_class:
                            convert_element(child, current_parent, section_stack, doc_path, chapter_id,
                                          mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                        else:
                            # Other divs can be processed inside figure (will become para, etc.)
                            convert_element(child, figure, section_stack, doc_path, chapter_id,
                                          mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                    else:
                        # Other elements - process inside figure
                        convert_element(child, figure, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

    # Standalone images (not in figure)
    elif tag_name == 'img':
        current_parent = section_stack[-1][1] if section_stack else parent_elem

        # Check if we're inside a table cell - figure not allowed there
        in_table_cell, entry_elem = is_inside_table_cell(current_parent)

        # Resolve image path
        img_src = elem.get('src', '')
        if img_src:
            result = resolve_image_path(img_src, doc_path, mapper)
            if result:
                intermediate_name, normalized_path = result

                if in_table_cell:
                    # Inside table cell - use mediaobject directly (no figure wrapper)
                    mediaobject = validated_subelement(current_parent, 'mediaobject')
                    imageobject = validated_subelement(mediaobject, 'imageobject')
                    imagedata = validated_subelement(imageobject, 'imagedata')
                    imagedata.set('fileref', intermediate_name)
                    mapper.add_reference(normalized_path, chapter_id)
                    # Add alt text as textobject if present
                    alt_text = elem.get('alt', '')
                    if alt_text:
                        textobject = validated_subelement(mediaobject, 'textobject')
                        # Avoid <phrase> to prevent downstream spacing issues
                        tpara = validated_subelement(textobject, 'para')
                        tpara.text = alt_text
                else:
                    # Normal case - create figure wrapper
                    # Validate that figure is allowed in this parent type
                    if not _is_valid_element_for_parent('figure', current_parent, elem.get('id')):
                        # Skip figure in dedication - just add text description
                        alt_text = elem.get('alt', '')
                        if alt_text:
                            para = validated_subelement(current_parent, 'para')
                            para.text = f"[Image: {alt_text}]"
                        return
                    figure_counter['count'] += 1
                    figure = validated_subelement(current_parent, 'figure')

                    # Always set ID (use original if present, otherwise generate one)
                    elem_id = elem.get('id')
                    current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
                    if elem_id:
                        figure.set('id', sanitize_anchor_id(elem_id, chapter_id, 'figure', current_sect1))
                    else:
                        figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))

                    # Register anchor IDs from any adjacent caption to point to this figure's ID.
                    adjacent_caption = _find_adjacent_caption_elem(elem, 'figure')
                    if adjacent_caption is not None:
                        _register_label_anchor_ids(adjacent_caption, chapter_id, figure.get('id'), 'figure')

                    # Title/caption from alt text (DTD requires title for figure)
                    alt_text = elem.get('alt', '')
                    title_elem = validated_subelement(figure, 'title')
                    if alt_text:
                        title_elem.text = alt_text
                    else:
                        # Apply fallback for figures without alt text
                        _ensure_title_content(title_elem, FALLBACK_FIGURE_TITLE, "figure")

                    # MediaObject (DTD requires content in figure)
                    mediaobject = validated_subelement(figure, 'mediaobject')
                    imageobject = validated_subelement(mediaobject, 'imageobject')
                    imagedata = validated_subelement(imageobject, 'imagedata')
                    imagedata.set('fileref', intermediate_name)
                    mapper.add_reference(normalized_path, chapter_id)
            else:
                # Could not resolve image - create placeholder
                figure_counter['count'] += 1
                if not in_table_cell:
                    figure = validated_subelement(current_parent, 'figure')
                    current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
                    figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))
                    validated_subelement(figure, 'title')
                    mediaobject = validated_subelement(figure, 'mediaobject')
                else:
                    mediaobject = validated_subelement(current_parent, 'mediaobject')
                imageobject = validated_subelement(mediaobject, 'imageobject')
                imagedata = validated_subelement(imageobject, 'imagedata')
                imagedata.set('fileref', f"missing_{figure_counter['count']}.jpg")

    # Lists
    elif tag_name == 'ul':
        # Get list items first - skip empty lists (DTD requires at least one listitem)
        li_items = elem.find_all('li', recursive=False)
        if not li_items:
            return  # Skip empty lists

        current_parent = section_stack[-1][1] if section_stack else parent_elem
        itemizedlist = validated_subelement(current_parent, 'itemizedlist')

        # Preserve ID attribute for cross-referencing (prefix with chapter_id to avoid conflicts)
        elem_id = elem.get('id')
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        if elem_id:
            itemizedlist.set('id', sanitize_anchor_id(elem_id, chapter_id, 'list', current_sect1))

        # Check for checkbox-style list classes and "none" class
        # Per spec: exul*, ul*-cb, checkbox, checklist, todo classes indicate checkbox lists
        # class="none" means suppress list markers (items may have custom markers)
        css_class = elem.get('class')
        css_class_str = ' '.join(css_class) if isinstance(css_class, list) else (css_class or '')
        css_class_lower = css_class_str.lower()

        # Check for "none" class - suppress list markers
        has_none_class = 'none' in css_class_lower.split()
        if has_none_class:
            itemizedlist.set('mark', 'none')

        # Checkbox list patterns
        checkbox_patterns = ['exul', 'checkbox', 'checklist', 'todo', 'task-list', 'tasklist']
        is_checkbox_list = any(pattern in css_class_lower for pattern in checkbox_patterns)
        # Also check for -cb suffix pattern (e.g., ul1-cb, ul2-cb)
        if not is_checkbox_list and re.search(r'\bul\d*-cb\b', css_class_lower):
            is_checkbox_list = True

        if is_checkbox_list:
            # Per DTD: mark attribute is CDATA, "checkbox" is a valid value
            itemizedlist.set('mark', 'checkbox')

        # If items are already numbered in their own text, suppress additional markers
        # (prevents double-numbering like "1. 1. ..." downstream).
        has_prenumbered_items = False
        if not has_none_class and not is_checkbox_list:
            items_to_check = li_items[:3]
            prenumber_count = 0
            prenumber_pattern = re.compile(
                r'^[\s]*(\d+[\.\):]|\(\d+\)|\d+\.\d+[\.\)]?|[a-zA-Z][\.\)]|\([a-zA-Z]\)|[ivxIVX]+[\.\)])\s'
            )
            for li in items_to_check:
                li_text = extract_text(li)
                li_text_for_match = li_text.strip() if li_text else ''
                if li_text_for_match and prenumber_pattern.match(li_text_for_match):
                    prenumber_count += 1
            if prenumber_count >= len(items_to_check) / 2 and prenumber_count > 0:
                has_prenumbered_items = True

        if has_prenumbered_items:
            itemizedlist.set('mark', 'none')
            # Keep existing role/class info, but ensure intent is visible
            if not itemizedlist.get('role'):
                itemizedlist.set('role', css_class_str if css_class_str else 'none')

        # Preserve role attribute: use existing role if present, otherwise use CSS class
        existing_role = elem.get('role', '')
        if existing_role:
            itemizedlist.set('role', existing_role)
        elif css_class_str:
            itemizedlist.set('role', css_class_str)

        for li in li_items:
            listitem = validated_subelement(itemizedlist, 'listitem')

            # Preserve ID attribute on list item for cross-referencing (prefix with chapter_id)
            li_id = li.get('id')
            if li_id:
                listitem.set('id', sanitize_anchor_id(li_id, chapter_id, 'listitem', current_sect1))

            # Bibliography citations often store IDs on a nested CitationContent div.
            # Preserve those IDs by mapping them onto the listitem so citations resolve.
            citation_content = li.find(class_='CitationContent')
            citation_id = citation_content.get('id') if citation_content else None
            if citation_id:
                if listitem.get('id'):
                    register_id_mapping(chapter_id, citation_id, listitem.get('id'), 'bibliography', doc_path)
                else:
                    listitem.set('id', sanitize_anchor_id(
                        citation_id, chapter_id, 'bibliography', current_sect1, doc_path
                    ))

            # Preserve CSS class and epub:type on list item as role
            # epub:type is important for bibliography detection (biblioentry)
            li_class = li.get('class')
            li_epub_type = _get_epub_type(li)
            if li_class:
                if isinstance(li_class, list):
                    li_class = ' '.join(li_class)
                listitem.set('role', li_class)
            elif li_epub_type:
                # Preserve epub:type as role for bibliography entries
                listitem.set('role', li_epub_type)

            # First pass: Identify pagebreak spans to skip them during content processing
            # Note: Pagebreak IDs are not preserved as anchors - R2 XSL has the anchor
            # template output commented out (html.xsl).
            pagebreak_spans = set()
            for child in li.children:
                if isinstance(child, Tag) and child.name == 'span':
                    epub_type = _get_epub_type(child)
                    role_attr = child.get('role', '')
                    is_pagebreak = False
                    if epub_type and 'pagebreak' in epub_type.lower():
                        is_pagebreak = True
                    elif role_attr and 'pagebreak' in role_attr.lower():
                        is_pagebreak = True
                    if is_pagebreak:
                        pagebreak_spans.add(child)

            # Pre-check: Detect ItemNumber + ItemContent div pattern (Springer list items).
            # These divs should be merged into a single <para> instead of being treated as
            # separate block elements. Pattern: <li><div class="ItemNumber">(i)</div>
            # <div class="ItemContent">Content...</div><div class="ClearBoth"/></li>
            _item_parts = []
            _is_item_pattern = False
            for child in li.children:
                if isinstance(child, Tag) and child.name == 'div':
                    _child_cls = ' '.join(child.get('class', [])) if isinstance(child.get('class'), list) else (child.get('class') or '')
                    _child_cls_lower = _child_cls.lower()
                    if 'itemnumber' in _child_cls_lower or 'itemcontent' in _child_cls_lower:
                        _item_parts.append(child)
                        _is_item_pattern = True
                    elif 'clearboth' in _child_cls_lower:
                        continue  # Skip ClearBoth spacer divs

            if _is_item_pattern and len(_item_parts) >= 2:
                # Merge ItemNumber + ItemContent into a single <para>
                para = validated_subelement(listitem, 'para')
                for part in _item_parts:
                    extract_inline_content(part, para, doc_path, chapter_id, mapper,
                                          figure_counter=figure_counter, section_counters=section_counters)
                continue  # Skip normal block/inline processing for this li

            # Check if li contains block-level elements that need special handling
            # Include 'img' so images get proper figure wrapper via convert_element
            # Include 'aside' so aside-with-figure elements are properly processed
            # Use recursive search to find images even when nested (e.g., <span><img/></span>)
            block_elements = {'p', 'div', 'figure', 'table', 'blockquote', 'pre', 'ol', 'ul', 'dl', 'img', 'aside'}
            has_block_content = any(
                isinstance(child, Tag) and child.name in block_elements
                for child in li.children
            ) or li.find('img', recursive=True) is not None

            if has_block_content:
                # Process children individually - block elements become siblings, inline becomes para
                inline_buffer = []
                for child in li.children:
                    # Skip pagebreak spans - already processed above
                    if child in pagebreak_spans:
                        continue
                    if isinstance(child, NavigableString):
                        text = _normalize_inline_whitespace(str(child))
                        # Include whitespace-only strings if there's already content in buffer
                        # (they're significant separators between elements like "<em>text</em> <a>link</a>")
                        # Only skip truly empty strings or leading whitespace before any content
                        if text.strip() or (text and inline_buffer):
                            inline_buffer.append(text)
                    elif isinstance(child, Tag):
                        if child.name in block_elements:
                            # Flush any buffered inline content first
                            if inline_buffer:
                                para = validated_subelement(listitem, 'para')
                                for inline_node in inline_buffer:
                                    if isinstance(inline_node, str):
                                        # Fix: after children exist, text goes to last child's tail, not para.text
                                        if len(para) > 0:
                                            para[-1].tail = (para[-1].tail or '') + inline_node
                                        elif para.text:
                                            para.text += inline_node
                                        else:
                                            para.text = inline_node
                                    else:
                                        # Use include_root=True to ensure formatting elements like <b>
                                        # are wrapped in proper DocBook elements (e.g., <emphasis>)
                                        extract_inline_content(inline_node, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, include_root=True, section_counters=section_counters)
                                inline_buffer = []
                            # Process block element
                            convert_element(child, listitem, [], doc_path, chapter_id,
                                          mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                        else:
                            # Inline element - buffer it
                            inline_buffer.append(child)
                # Flush remaining inline content
                if inline_buffer:
                    para = validated_subelement(listitem, 'para')
                    for inline_node in inline_buffer:
                        if isinstance(inline_node, str):
                            # Fix: after children exist, text goes to last child's tail, not para.text
                            if len(para) > 0:
                                para[-1].tail = (para[-1].tail or '') + inline_node
                            elif para.text:
                                para.text += inline_node
                            else:
                                para.text = inline_node
                        else:
                            # Use include_root=True to ensure formatting elements like <b>
                            # are wrapped in proper DocBook elements (e.g., <emphasis>)
                            extract_inline_content(inline_node, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, include_root=True, section_counters=section_counters)
            else:
                # Simple case: li only has inline content
                para = validated_subelement(listitem, 'para')
                # Pass pagebreak_spans to exclude them - they were already processed in first pass
                extract_inline_content(li, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, exclude_elements=pagebreak_spans, section_counters=section_counters)

                # Handle nested lists (like in TOC) - process them as nested orderedlist/itemizedlist
                for nested_list in li.find_all(['ol', 'ul'], recursive=False):
                    # Recursively convert the nested list
                    convert_element(nested_list, listitem, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

    elif tag_name == 'ol':
        # Get list items first - skip empty lists (DTD requires at least one listitem)
        li_items = elem.find_all('li', recursive=False)
        if not li_items:
            return  # Skip empty lists

        current_parent = section_stack[-1][1] if section_stack else parent_elem

        # Check CSS class for "none" which means suppress list markers
        # This is common when list items already contain their own numbering (e.g., "1.1", "1.2")
        css_class = elem.get('class')
        css_class_str = ' '.join(css_class) if isinstance(css_class, list) else (css_class or '')
        css_class_lower = css_class_str.lower()
        has_none_class = 'none' in css_class_lower.split()

        # Check if this is an endnotes list (li elements with class 'rearnote')
        # If so, use itemizedlist with mark="none" to avoid duplicate numbering
        # since endnotes already contain their own linked numbers
        is_endnotes = any(
            'rearnote' in (li.get('class') or [])
            for li in li_items
        )

        # Check if list items already contain their own numbering in the text
        # This detects patterns like: "1.", "1.1", "1)", "(1)", "a.", "a)", "(a)", "i.", "I."
        # If items are pre-numbered, use itemizedlist with mark="none" to avoid duplicate numbering
        has_prenumbered_items = False
        if not is_endnotes and not has_none_class:
            # Check first few items for numbering patterns
            items_to_check = li_items[:3]  # Check first 3 items
            prenumber_count = 0
            # Patterns: 1. | 1.1 | 1.1.1 | 1) | (1) | a. | a) | (a) | i. | I. | etc.
            prenumber_pattern = re.compile(r'^[\s]*(\d+[\.\):]|\(\d+\)|\d+\.\d+[\.\)]?|[a-zA-Z][\.\)]|\([a-zA-Z]\)|[ivxIVX]+[\.\)])\s')
            for li in items_to_check:
                li_text = extract_text(li).strip()
                if li_text and prenumber_pattern.match(li_text):
                    prenumber_count += 1
            # If majority of checked items have numbering, treat as pre-numbered
            if prenumber_count >= len(items_to_check) / 2 and prenumber_count > 0:
                has_prenumbered_items = True
                logger.debug(f"Detected pre-numbered list items, using orderedlist (postprocessor will strip numbers)")

        # Use itemizedlist with mark="none" for endnotes or class="none" (suppress markers)
        # For pre-numbered items, use orderedlist — the postprocessor will strip leading numbers
        if is_endnotes or has_none_class:
            list_elem = validated_subelement(current_parent, 'itemizedlist')
            list_elem.set('mark', 'none')
        elif has_prenumbered_items:
            list_elem = validated_subelement(current_parent, 'orderedlist')
        else:
            list_elem = validated_subelement(current_parent, 'orderedlist')

        # Preserve ID attribute for cross-referencing (prefix with chapter_id to avoid conflicts)
        elem_id = elem.get('id')
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        if elem_id:
            list_elem.set('id', sanitize_anchor_id(elem_id, chapter_id, 'list', current_sect1))

        # Preserve role attribute: use existing role if present, otherwise use CSS class
        existing_role = elem.get('role', '')
        if existing_role:
            list_elem.set('role', existing_role)
        elif css_class_str:
            list_elem.set('role', css_class_str)

        for li in li_items:
            listitem = validated_subelement(list_elem, 'listitem')

            # Preserve ID attribute on list item for cross-referencing (prefix with chapter_id)
            li_id = li.get('id')
            if li_id:
                listitem.set('id', sanitize_anchor_id(li_id, chapter_id, 'listitem', current_sect1))

            # Bibliography citations often store IDs on a nested CitationContent div.
            # Preserve those IDs by mapping them onto the listitem so citations resolve.
            citation_content = li.find(class_='CitationContent')
            citation_id = citation_content.get('id') if citation_content else None
            if citation_id:
                if listitem.get('id'):
                    register_id_mapping(chapter_id, citation_id, listitem.get('id'), 'bibliography', doc_path)
                else:
                    listitem.set('id', sanitize_anchor_id(
                        citation_id, chapter_id, 'bibliography', current_sect1, doc_path
                    ))

            # Preserve CSS class and epub:type on list item as role
            # epub:type is important for bibliography detection (biblioentry)
            li_class = li.get('class')
            li_epub_type = _get_epub_type(li)
            if li_class:
                if isinstance(li_class, list):
                    li_class = ' '.join(li_class)
                listitem.set('role', li_class)
            elif li_epub_type:
                # Preserve epub:type as role for bibliography entries
                listitem.set('role', li_epub_type)

            # First pass: Identify pagebreak spans to skip them during content processing
            # Note: Pagebreak IDs are not preserved as anchors - R2 XSL has the anchor
            # template output commented out (html.xsl).
            pagebreak_spans = set()
            for child in li.children:
                if isinstance(child, Tag) and child.name == 'span':
                    epub_type = _get_epub_type(child)
                    role_attr = child.get('role', '')
                    is_pagebreak = False
                    if epub_type and 'pagebreak' in epub_type.lower():
                        is_pagebreak = True
                    elif role_attr and 'pagebreak' in role_attr.lower():
                        is_pagebreak = True
                    if is_pagebreak:
                        pagebreak_spans.add(child)

            # Pre-check: Detect ItemNumber + ItemContent div pattern (Springer list items).
            # These divs should be merged into a single <para> instead of being treated as
            # separate block elements. Pattern: <li><div class="ItemNumber">(i)</div>
            # <div class="ItemContent">Content...</div><div class="ClearBoth"/></li>
            _item_parts = []
            _is_item_pattern = False
            for child in li.children:
                if isinstance(child, Tag) and child.name == 'div':
                    _child_cls = ' '.join(child.get('class', [])) if isinstance(child.get('class'), list) else (child.get('class') or '')
                    _child_cls_lower = _child_cls.lower()
                    if 'itemnumber' in _child_cls_lower or 'itemcontent' in _child_cls_lower:
                        _item_parts.append(child)
                        _is_item_pattern = True
                    elif 'clearboth' in _child_cls_lower:
                        continue  # Skip ClearBoth spacer divs

            if _is_item_pattern and len(_item_parts) >= 2:
                # Merge ItemNumber + ItemContent into a single <para>
                para = validated_subelement(listitem, 'para')
                for part in _item_parts:
                    extract_inline_content(part, para, doc_path, chapter_id, mapper,
                                          figure_counter=figure_counter, section_counters=section_counters)
                continue  # Skip normal block/inline processing for this li

            # Check if li contains block-level elements that need special handling
            # Include 'img' so images get proper figure wrapper via convert_element
            # Include 'aside' so aside-with-figure elements are properly processed
            # Use recursive search to find images even when nested (e.g., <span><img/></span>)
            block_elements = {'p', 'div', 'figure', 'table', 'blockquote', 'pre', 'ol', 'ul', 'dl', 'img', 'aside'}
            has_block_content = any(
                isinstance(child, Tag) and child.name in block_elements
                for child in li.children
            ) or li.find('img', recursive=True) is not None

            if has_block_content:
                # Process children individually - block elements become siblings, inline becomes para
                inline_buffer = []
                for child in li.children:
                    # Skip pagebreak spans - already processed above
                    if child in pagebreak_spans:
                        continue
                    if isinstance(child, NavigableString):
                        text = _normalize_inline_whitespace(str(child))
                        # Include whitespace-only strings if there's already content in buffer
                        # (they're significant separators between elements like "<em>text</em> <a>link</a>")
                        # Only skip truly empty strings or leading whitespace before any content
                        if text.strip() or (text and inline_buffer):
                            inline_buffer.append(text)
                    elif isinstance(child, Tag):
                        if child.name in block_elements:
                            # Flush any buffered inline content first
                            if inline_buffer:
                                para = validated_subelement(listitem, 'para')
                                for inline_node in inline_buffer:
                                    if isinstance(inline_node, str):
                                        # Fix: after children exist, text goes to last child's tail, not para.text
                                        if len(para) > 0:
                                            para[-1].tail = (para[-1].tail or '') + inline_node
                                        elif para.text:
                                            para.text += inline_node
                                        else:
                                            para.text = inline_node
                                    else:
                                        # Use include_root=True to ensure formatting elements like <b>
                                        # are wrapped in proper DocBook elements (e.g., <emphasis>)
                                        extract_inline_content(inline_node, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, include_root=True, section_counters=section_counters)
                                inline_buffer = []
                            # Process block element
                            convert_element(child, listitem, [], doc_path, chapter_id,
                                          mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                        else:
                            # Inline element - buffer it
                            inline_buffer.append(child)
                # Flush remaining inline content
                if inline_buffer:
                    para = validated_subelement(listitem, 'para')
                    for inline_node in inline_buffer:
                        if isinstance(inline_node, str):
                            # Fix: after children exist, text goes to last child's tail, not para.text
                            if len(para) > 0:
                                para[-1].tail = (para[-1].tail or '') + inline_node
                            elif para.text:
                                para.text += inline_node
                            else:
                                para.text = inline_node
                        else:
                            # Use include_root=True to ensure formatting elements like <b>
                            # are wrapped in proper DocBook elements (e.g., <emphasis>)
                            extract_inline_content(inline_node, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, include_root=True, section_counters=section_counters)
            else:
                # Simple case: li only has inline content
                para = validated_subelement(listitem, 'para')
                # Pass pagebreak_spans to exclude them - they were already processed in first pass
                extract_inline_content(li, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, exclude_elements=pagebreak_spans, section_counters=section_counters)

                # Handle nested lists (like in TOC) - process them as nested orderedlist/itemizedlist
                for nested_list in li.find_all(['ol', 'ul'], recursive=False):
                    # Recursively convert the nested list
                    convert_element(nested_list, listitem, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

    # Tables
    elif tag_name == 'table':
        table_counter['count'] += 1
        current_parent = section_stack[-1][1] if section_stack else parent_elem

        # Validate that table is allowed in this parent type
        if not _is_valid_element_for_parent('table', current_parent, elem.get('id')):
            # Skip table in dedication - add placeholder text if table has content
            text_content = extract_text(elem)
            if text_content and text_content.strip():
                para = validated_subelement(current_parent, 'para')
                para.text = f"[Table content: {text_content.strip()[:100]}...]"
            return

        # Simple table conversion (can be enhanced)
        table = validated_subelement(current_parent, 'table')

        # Always set ID (use original if present, otherwise generate one)
        elem_id = elem.get('id')
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        if elem_id:
            table.set('id', sanitize_anchor_id(elem_id, chapter_id, 'table', current_sect1, doc_path))
        else:
            table.set('id', generate_element_id(chapter_id, 'table', current_sect1))

        # Preserve role attribute: use existing role if present, otherwise use CSS class
        existing_role = elem.get('role', '')
        css_class = elem.get('class')
        if existing_role:
            table.set('role', existing_role)
        elif css_class:
            if isinstance(css_class, list):
                css_class = ' '.join(css_class)
            table.set('role', css_class)

        # Look for caption element as title (DTD requires title for table)
        caption = elem.find('caption')
        title_elem = validated_subelement(table, 'title')
        if caption:
            # Register anchor IDs found in caption to point to this table's ID.
            _register_label_anchor_ids(caption, chapter_id, table.get('id'), 'table')
            # Pass table ID to redirect self-referencing label links to this table
            extract_inline_content(caption, title_elem, doc_path, chapter_id, mapper,
                                 figure_counter=figure_counter, section_counters=section_counters,
                                 containing_element_id=table.get('id'),
                                 containing_element_type='table')
        else:
            adjacent_caption = _find_adjacent_caption_elem(elem, 'table')
            if adjacent_caption is not None:
                _register_label_anchor_ids(adjacent_caption, chapter_id, table.get('id'), 'table')
                # Extract caption text into the title element (C-027 fix)
                extract_inline_content(adjacent_caption, title_elem, doc_path, chapter_id, mapper,
                                     figure_counter=figure_counter, section_counters=section_counters,
                                     containing_element_id=table.get('id'),
                                     containing_element_type='table')
            else:
                # DTD requires title - add generic one if missing
                title_elem.text = f"Table {table_counter['count']}"

        # DTD requires tgroup for table content - use improved span handling
        num_cols, num_rows, span_tracker = precompute_table_structure(elem)
        tgroup = validated_subelement(table, 'tgroup', cols=str(num_cols))

        # Add colspec elements for column spanning support
        _add_colspecs_to_tgroup(tgroup, num_cols)

        # Validate colspec count
        validate_colspec_count(tgroup, num_cols)

        tbody = validated_subelement(tgroup, 'tbody')

        table_rows = _iter_table_rows(elem)
        for row_idx, tr in enumerate(table_rows):
            row = validated_subelement(tbody, 'row')
            col_index = 1  # Track current column (1-based for CALS)
            for td in tr.find_all(['td', 'th'], recursive=False):
                # Skip columns occupied by rowspans from previous rows
                if span_tracker:
                    col_index = span_tracker.get_next_free_column(row_idx, col_index)

                entry = validated_subelement(row, 'entry')

                # Handle rowspan/colspan -> morerows/namest/nameend
                col_span = _set_entry_spanning(entry, td, col_index)
                col_index += col_span  # Advance by the span width

                # Note: td/th IDs are not preserved - R2 XSL has the anchor
                # template output commented out (html.xsl).

                # Check if cell contains block-level elements that need special handling
                # Include 'img' so images get proper figure wrapper via convert_element
                # Use recursive search to find images even when nested
                # NOTE: Inline math images (vertical-align:middle) are NOT block elements.
                block_elements = {'ol', 'ul', 'dl', 'p', 'div', 'blockquote', 'pre', 'table', 'figure', 'img'}
                has_block_content = any(
                    isinstance(c, Tag) and c.name in block_elements
                    and not (c.name == 'img' and _is_inline_image(c))
                    for c in td.children
                ) or any(
                    not _is_inline_image(img)
                    for img in td.find_all('img', recursive=True)
                )

                if has_block_content:
                    # Process block-level content properly
                    # First, handle any leading inline content
                    leading_inline = []
                    for child in td.children:
                        if isinstance(child, Tag) and child.name in block_elements and not (child.name == 'img' and _is_inline_image(child)):
                            break
                        leading_inline.append(child)

                    # Create para for leading inline content if any
                    if leading_inline:
                        leading_text = ''.join(
                            str(c).strip() if isinstance(c, NavigableString) else c.get_text('').strip()
                            for c in leading_inline
                        ).strip()
                        if leading_text:
                            para = validated_subelement(entry, 'para')
                            # Create a temporary element to hold the inline content
                            for child in leading_inline:
                                if isinstance(child, Tag):
                                    extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                                elif isinstance(child, NavigableString):
                                    text = str(child)
                                    if text and text.strip():
                                        if len(para) == 0:
                                            para.text = (para.text or '') + text
                                        else:
                                            para[-1].tail = (para[-1].tail or '') + text

                    # Process each block-level child
                    for child in td.children:
                        if isinstance(child, Tag):
                            if child.name in block_elements and not (child.name == 'img' and _is_inline_image(child)):
                                # Process block element - use entry as parent
                                convert_element(child, entry, [], doc_path, chapter_id,
                                              mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                            elif child.name not in ['br', 'hr']:
                                # Inline element after block - wrap in para
                                # (This handles mixed content like: <ol>...</ol><span>note</span>)
                                inline_text = extract_text(child)
                                if inline_text:
                                    para = validated_subelement(entry, 'para')
                                    extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                else:
                    # Only inline content - use original approach
                    cell_text = extract_text(td)
                    has_children = any(isinstance(c, Tag) for c in td.children)
                    if cell_text or has_children:
                        para = validated_subelement(entry, 'para')
                        # Use extract_inline_content to preserve anchor tags and hrefs in table cells
                        extract_inline_content(td, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)

    # Blockquote
    elif tag_name == 'blockquote':
        current_parent = section_stack[-1][1] if section_stack else parent_elem
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        blockquote = validated_subelement(current_parent, 'blockquote')

        # Preserve ID attribute for cross-referencing (prefix with chapter_id to avoid conflicts)
        elem_id = elem.get('id')
        if elem_id:
            blockquote.set('id', sanitize_anchor_id(elem_id, chapter_id, 'blockquote', current_sect1))

        # Preserve role attribute: use existing role if present, otherwise use CSS class
        existing_role = elem.get('role', '')
        css_class = elem.get('class')
        css_class_str = ' '.join(css_class) if isinstance(css_class, list) else (css_class or '')

        # Check for epigraph or dialogue role
        css_class_lower = css_class_str.lower()
        if 'epigraph' in css_class_lower:
            blockquote.set('role', 'epigraph')
        elif any(d in css_class_lower for d in ['dialogue', 'dialog', 'speech']):
            blockquote.set('role', 'dialogue')
        elif existing_role:
            blockquote.set('role', existing_role)
        elif css_class_str:
            blockquote.set('role', css_class_str)

        # Per DTD: blockquote = (blockinfo?, title?, attribution?, (%component.mix;)+)
        # We need to detect and extract attribution before processing other content
        # Attribution sources: <cite> element, or elements with attribution-like classes
        attribution_classes = ['source', 'attribution', 'attrib', 'credit', 'author',
                              'cite', 'quotesource', 'quote-source', 'byline']

        attribution_elem = None
        content_children = []

        for child in elem.children:
            if isinstance(child, Tag):
                child_class = child.get('class', '')
                child_class_str = ' '.join(child_class) if isinstance(child_class, list) else (child_class or '')
                child_class_lower = child_class_str.lower()

                # Check if this is an attribution element
                is_attribution = False
                if child.name == 'cite':
                    is_attribution = True
                elif child.name == 'footer':
                    # Footer in blockquote often contains attribution
                    is_attribution = True
                elif any(attr_cls in child_class_lower for attr_cls in attribution_classes):
                    is_attribution = True

                if is_attribution and attribution_elem is None:
                    # Create attribution element (only use first one found)
                    attribution_elem = validated_subelement(blockquote, 'attribution')
                    extract_inline_content(child, attribution_elem, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                else:
                    content_children.append(child)

        # Process remaining content children
        for child in content_children:
            convert_element(child, blockquote, [], doc_path, chapter_id, mapper,
                          figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

    # Navigation (TOC) - process as regular content with links
    # Note: We do NOT create <toc> elements because TOC is already in book.xml
    elif tag_name == 'nav':
        current_parent = section_stack[-1][1] if section_stack else parent_elem
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None

        # Standard nav handling - extract title if present
        nav_title = None
        h_elem = elem.find(['h1', 'h2', 'h3', 'h4', 'h5', 'h6'])
        if h_elem:
            nav_title = extract_text(h_elem)

        # Preserve ID attribute from nav element
        nav_id = elem.get('id')

        # If nav has a title, create a section
        if nav_title:
            # Treat titled <nav> blocks as a section-level container.
            section = etree.Element('sect1')
            section.set('id', id_gen_section_id(chapter_id, 1, section_counters))

            title_elem = validated_subelement(section, 'title')
            title_elem.text = nav_title
            # Note: nav IDs are not preserved - R2 XSL has the anchor template
            # output commented out (html.xsl).
            validated_append(current_parent, section)
            # Process nav children in the section
            for child in elem.children:
                if isinstance(child, Tag) and child.name not in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
                    convert_element(child, section, [], doc_path, chapter_id, mapper,
                                  figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
        else:
            # No title, just process children directly
            # Note: nav IDs are not preserved - R2 XSL has the anchor template
            # output commented out (html.xsl).
            for child in elem.children:
                if isinstance(child, Tag):
                    convert_element(child, current_parent, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)

    # Definition lists (glossaries)
    elif tag_name == 'dl':
        # Get list items first - skip empty lists (DTD requires at least one varlistentry)
        dt_items = elem.find_all('dt', recursive=False)
        if not dt_items:
            return  # Skip empty definition lists

        current_parent = section_stack[-1][1] if section_stack else parent_elem
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None

        # DTD Compliance: variablelist is NOT allowed in dedication - use itemizedlist instead
        if not _is_valid_element_for_parent('variablelist', parent_elem, elem.get('id')):
            # Convert to itemizedlist instead
            itemizedlist = validated_subelement(current_parent, 'itemizedlist')
            if elem.get('id'):
                itemizedlist.set('id', sanitize_anchor_id(elem.get('id'), chapter_id, 'list', current_sect1))
            for child in elem.children:
                if isinstance(child, Tag):
                    if child.name == 'dt':
                        listitem = validated_subelement(itemizedlist, 'listitem')
                        para = validated_subelement(listitem, 'para')
                        emphasis = validated_subelement(para, 'emphasis')
                        emphasis.set('role', 'bold')
                        emphasis.text = extract_text(child)
                    elif child.name == 'dd':
                        # Try to add to the last listitem
                        listitems = itemizedlist.findall('listitem')
                        if listitems:
                            last_listitem = listitems[-1]
                            dd_para = validated_subelement(last_listitem, 'para')
                            dd_para.text = extract_text(child)
            return

        variablelist = validated_subelement(current_parent, 'variablelist')

        # Preserve ID attribute for cross-referencing (prefix with chapter_id to avoid conflicts)
        elem_id = elem.get('id')
        if elem_id:
            variablelist.set('id', sanitize_anchor_id(elem_id, chapter_id, 'variablelist', current_sect1))

        # Process dt/dd pairs
        current_term = None
        for child in elem.children:
            if isinstance(child, Tag):
                if child.name == 'dt':
                    # Start a new varlistentry
                    current_term = validated_subelement(variablelist, 'varlistentry')

                    # Preserve ID attribute on varlistentry from dt element (prefix with chapter_id)
                    dt_id = child.get('id')
                    if dt_id:
                        current_term.set('id', sanitize_anchor_id(dt_id, chapter_id, 'term', current_sect1))

                    term = validated_subelement(current_term, 'term')
                    term.text = extract_text(child)
                elif child.name == 'dd' and current_term is not None:
                    # Add definition to current term
                    listitem = validated_subelement(current_term, 'listitem')

                    # Preserve ID attribute on listitem from dd element (prefix with chapter_id)
                    dd_id = child.get('id')
                    if dd_id:
                        listitem.set('id', sanitize_anchor_id(dd_id, chapter_id, 'listitem', current_sect1))

                    # Check if dd has block elements or just text
                    has_block = any(isinstance(c, Tag) and c.name in ['p', 'div', 'ul', 'ol']
                                  for c in child.children)

                    if has_block:
                        # Process block elements
                        for dd_child in child.children:
                            if isinstance(dd_child, Tag):
                                convert_element(dd_child, listitem, [], doc_path, chapter_id,
                                              mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                    else:
                        # Just text, wrap in para
                        para = validated_subelement(listitem, 'para')
                        para.text = extract_text(child)

    # Aside elements - check for footnotes first, then figures, then convert to sidebar
    elif tag_name == 'aside':
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        # Check if this is a footnote (EPUB3 pattern)
        # Use robust helper that handles BeautifulSoup namespace variations for epub:type
        epub_type = _get_epub_type(elem)
        css_class = elem.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        css_class_lower = css_class.lower()

        # Detect FootnoteSection CONTAINER (plural "footnotes") vs individual footnote
        # FootnoteSection contains multiple individual <div class="Footnote"> children
        # that must be split into separate <footnote> elements placed inline at noterefs
        is_footnote_section = (
            'footnotesection' in css_class_lower or
            epub_type.lower() == 'footnotes'  # plural = container
        )

        if is_footnote_section:
            # Process each individual footnote within the FootnoteSection
            # At this point, body text above has already been processed into the XML tree,
            # with noteref <sup><a> converted to <superscript>N</superscript> markers.
            # We find those markers and replace them with proper <footnote> elements.

            # Walk up to find the root element for this section to search all superscripts
            root = parent_elem
            while root.getparent() is not None:
                root = root.getparent()

            from id_authority import generate_compliant_id
            existing_ids = section_counters.get('_existing_ids', set()) if section_counters else set()

            fn_divs = elem.find_all('div', class_='Footnote')
            if not fn_divs:
                # Fallback: try direct children with epub:type="footnote" (singular)
                fn_divs = elem.find_all(attrs={'epub:type': 'footnote'})

            for fn_div in fn_divs:
                # Extract footnote number from FootnoteNumber span
                fn_num_span = fn_div.find(class_='FootnoteNumber')
                fn_number = extract_text(fn_num_span).strip() if fn_num_span else ''
                if not fn_number:
                    continue

                # Extract footnote content
                fn_content = fn_div.find(class_='FootnoteContent')
                if not fn_content:
                    # Fallback: use the footnote div itself as content container
                    fn_content = fn_div

                # Find matching <superscript>N</superscript> in the processed XML tree
                placed = False
                for sup in root.iter('superscript'):
                    sup_text = (sup.text or '').strip()
                    # Match text-only superscripts with the footnote number
                    # Exclude superscripts that already contain child elements (e.g., <link>)
                    if sup_text == fn_number and len(sup) == 0:
                        sup_parent = sup.getparent()
                        if sup_parent is not None and sup_parent.tag == 'para':
                            # Generate footnote ID
                            footnote_id = generate_compliant_id(
                                element_type='footnote',
                                chapter_id=chapter_id,
                                section_id=current_sect1,
                                existing_ids=existing_ids
                            )
                            existing_ids.add(footnote_id)

                            # Create footnote element
                            footnote_elem = etree.Element('footnote')
                            footnote_elem.set('id', footnote_id)

                            # Process footnote content paragraphs
                            fn_paragraphs = fn_content.find_all('p', recursive=False)
                            if fn_paragraphs:
                                for p in fn_paragraphs:
                                    para = validated_subelement(footnote_elem, 'para')
                                    extract_inline_content(p, para, doc_path, chapter_id, mapper,
                                                         figure_counter=figure_counter, section_counters=section_counters)
                            else:
                                # No <p> children — wrap all inline content in a single para
                                para = validated_subelement(footnote_elem, 'para')
                                extract_inline_content(fn_content, para, doc_path, chapter_id, mapper,
                                                     figure_counter=figure_counter, section_counters=section_counters)

                            # Replace <superscript> with <footnote> in the tree
                            # Preserve position and tail text
                            footnote_elem.tail = sup.tail
                            sup_index = list(sup_parent).index(sup)
                            sup_parent.remove(sup)
                            sup_parent.insert(sup_index, footnote_elem)

                            placed = True
                            break

                if not placed:
                    logger.warning(f"FootnoteSection: could not find <superscript>{fn_number}</superscript> "
                                  f"marker for footnote {fn_number} in {doc_path}")
                    # Fallback: create footnote at end of current parent
                    fallback_parent = parent_elem
                    if fallback_parent.tag not in ('para', 'simpara'):
                        # Need to wrap in para for DTD compliance
                        fallback_parent = validated_subelement(parent_elem, 'para')
                    footnote_id = generate_compliant_id(
                        element_type='footnote',
                        chapter_id=chapter_id,
                        section_id=current_sect1,
                        existing_ids=existing_ids
                    )
                    existing_ids.add(footnote_id)
                    footnote_elem = validated_subelement(fallback_parent, 'footnote')
                    footnote_elem.set('id', footnote_id)
                    para = validated_subelement(footnote_elem, 'para')
                    extract_inline_content(fn_content, para, doc_path, chapter_id, mapper,
                                         figure_counter=figure_counter, section_counters=section_counters)

            if section_counters:
                section_counters['_existing_ids'] = existing_ids

            return  # Don't continue to individual footnote or sidebar processing

        # Detect individual footnote patterns (singular, not container)
        is_footnote = (
            ('footnote' in epub_type.lower() and epub_type.lower() != 'footnotes') or
            'note' == epub_type.lower() or
            ('footnote' in css_class_lower and 'footnotesection' not in css_class_lower) or
            'fn' in css_class_lower.split()
        )

        if is_footnote:
            # Create DocBook footnote element
            # Per DTD: <!ELEMENT footnote ((%footnote.mix;)+)>
            # footnote.mix includes para, simpara, formalpara, etc.
            # NOTE: footnote cannot be direct child of listitem - must be inside para
            footnote_parent = parent_elem
            if parent_elem.tag == 'listitem':
                # Wrap footnote in para for DTD compliance
                footnote_parent = validated_subelement(parent_elem, 'para')
            footnote_elem = validated_subelement(footnote_parent, 'footnote')

            # Preserve ID for cross-referencing, or generate one if missing
            elem_id = elem.get('id')
            if elem_id:
                footnote_elem.set('id', sanitize_anchor_id(elem_id, chapter_id, 'footnote', current_sect1))
            else:
                # Auto-generate footnote ID using hierarchical pattern
                from id_authority import generate_compliant_id
                existing_ids = section_counters.get('_existing_ids', set()) if section_counters else set()
                footnote_id = generate_compliant_id(
                    element_type='footnote',
                    chapter_id=chapter_id,
                    section_id=current_sect1,
                    existing_ids=existing_ids
                )
                footnote_elem.set('id', footnote_id)
                if section_counters:
                    if '_existing_ids' not in section_counters:
                        section_counters['_existing_ids'] = set()
                    section_counters['_existing_ids'].add(footnote_id)

            # Process footnote content
            footnote_has_content = False
            for child in elem.children:
                if isinstance(child, Tag):
                    if child.name == 'p':
                        # Convert paragraph to para inside footnote
                        para = validated_subelement(footnote_elem, 'para')
                        extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                        footnote_has_content = True
                    elif child.name in ['div', 'span']:
                        # Wrap div/span content in para
                        para = validated_subelement(footnote_elem, 'para')
                        extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                        footnote_has_content = True
                    else:
                        # Other block elements - try to process
                        convert_element(child, footnote_elem, [], doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                        footnote_has_content = True
                elif isinstance(child, NavigableString):
                    text = str(child)
                    if text and text.strip():
                        para = validated_subelement(footnote_elem, 'para')
                        para.text = text
                        footnote_has_content = True

            # Footnote must have content per DTD
            if not footnote_has_content:
                para = validated_subelement(footnote_elem, 'para')
                # Leave empty - minimal valid footnote

            return  # Don't continue to sidebar processing

        # Check if aside is a bibliography section - Springer EPUB pattern
        # Structure: <aside class="Bibliography" epub:type="bibliography"><div epub:type="bibliography">
        #            <h2>References</h2><ol class="BibliographyWrapper"><li class="Citation">...</li></ol></div></aside>
        is_bibliography_aside = (
            'bibliography' in css_class_lower or
            'bibliography' in epub_type.lower() or
            'doc-bibliography' in epub_type.lower() or
            elem.get('role', '').lower() == 'doc-bibliography'
        )
        if is_bibliography_aside:
            # Convert to proper DocBook bibliography structure
            # Use the chapter root element (parent_elem) as the parent for
            # bibliography, not section_stack[-1].  The aside is always a
            # top-level sibling of section elements in the EPUB source, so
            # placing it at the chapter level is correct.  Using section_stack
            # can place it inside a deeply nested section which causes it to
            # be lost during post-processing.
            current_parent = parent_elem

            # If current_parent is already a bibliography element, reuse it
            # to avoid creating a nested bibliography (DTD violation)
            if current_parent.tag == 'bibliography':
                bibliography = current_parent
            else:
                # Create bibliography wrapper inside parent section
                bibliography = validated_subelement(current_parent, 'bibliography')

                # Set ID - Bibliography wrapper uses sect1-style ID (e.g., ch0004s0010)
                # with NO 'bib' suffix.  The 'bib' code is ONLY for individual
                # bibliomixed entries.  Generate the next section-style ID so the
                # bookloader can split it into a navigable page.
                _bib_wrapper_id = _get_next_section_id_for_bibliography(current_parent, chapter_id)
                bibliography.set('id', _bib_wrapper_id)
                # Register mapping from original element ID if present
                elem_id = elem.get('id')
                if elem_id:
                    if isinstance(elem_id, list):
                        elem_id = elem_id[0] if elem_id else None
                    if elem_id:
                        register_id_mapping(chapter_id, elem_id, _bib_wrapper_id)

            # Extract the section prefix from the bibliography's own ID so that
            # individual bibliomixed entries get properly scoped IDs.
            bib_section_id = bibliography.get('id', '') or current_sect1

            # Also register the heading's ID if present, mapping to bibliography
            # Look for heading in h1-h6 first, then fall back to <div class="Heading">
            # (Springer EPUBs use <div class="Heading">References</div> instead of h-tags)
            title_elem = elem.find(['h1', 'h2', 'h3', 'h4', 'h5', 'h6'], recursive=True)
            if not title_elem:
                # Springer pattern: <div class="Heading">References</div>
                for child_div in elem.find_all('div', recursive=True):
                    child_class = child_div.get('class', '')
                    if isinstance(child_class, list):
                        child_class = ' '.join(child_class)
                    if child_class.strip().lower() == 'heading':
                        title_elem = child_div
                        break
            if title_elem:
                bib_title = validated_subelement(bibliography, 'title')
                bib_title.text = extract_text(title_elem)
                _bib_heading_id = title_elem.get('id')
                if _bib_heading_id and bib_section_id:
                    register_id_mapping(chapter_id, _bib_heading_id, bib_section_id)

            # Find the ordered list containing bibliography entries
            bib_list = elem.find('ol', recursive=True)
            if bib_list:
                bib_entry_counter = 0
                for li in bib_list.find_all('li', recursive=False):
                    bib_entry_counter += 1
                    # Create bibliomixed for each entry
                    bibliomixed = validated_subelement(bibliography, 'bibliomixed')

                    # Get ID from CitationContent div inside the li
                    citation_content = li.find(class_='CitationContent')
                    if citation_content and citation_content.get('id'):
                        bibliomixed.set('id', sanitize_anchor_id(
                            citation_content.get('id'), chapter_id, 'bibliography', bib_section_id))
                    else:
                        bibliomixed.set('id', generate_element_id(chapter_id, 'bibliography', bib_section_id))

                    # Extract content from the citation
                    # Skip CitationNumber div (auto-numbered by renderer)
                    for child in li.children:
                        if isinstance(child, Tag):
                            child_class = child.get('class', '')
                            if isinstance(child_class, list):
                                child_class = ' '.join(child_class)
                            # Skip the number, extract the content
                            if 'CitationNumber' in child_class:
                                continue
                            if 'CitationContent' in child_class:
                                # Extract inline content into bibliomixed
                                extract_inline_content(
                                    child, bibliomixed, doc_path, chapter_id, mapper,
                                    figure_counter=figure_counter, section_counters=section_counters
                                )

                    # Sanitize content - remove elements not allowed in bibliomixed
                    _sanitize_for_bibliomixed(bibliomixed)
                    # Parse metadata from bibliography text
                    _parse_bibliography_metadata(bibliomixed)

                logger.info(f"Converted {bib_entry_counter} bibliography entries from aside in {chapter_id}")
            else:
                # No ordered list - try to process direct content
                for child in elem.descendants:
                    if isinstance(child, Tag) and child.name == 'li':
                        li_class = child.get('class', '')
                        if isinstance(li_class, list):
                            li_class = ' '.join(li_class)
                        if 'Citation' in li_class:
                            bibliomixed = validated_subelement(bibliography, 'bibliomixed')
                            citation_content = child.find(class_='CitationContent')
                            if citation_content:
                                if citation_content.get('id'):
                                    bibliomixed.set('id', sanitize_anchor_id(
                                        citation_content.get('id'), chapter_id, 'bibliography', bib_section_id))
                                else:
                                    # Generate ID if CitationContent has no ID
                                    bibliomixed.set('id', generate_element_id(chapter_id, 'bibliography', bib_section_id))
                                extract_inline_content(
                                    citation_content, bibliomixed, doc_path, chapter_id, mapper,
                                    figure_counter=figure_counter, section_counters=section_counters
                                )
                            else:
                                # No CitationContent - generate ID and extract all content from li
                                bibliomixed.set('id', generate_element_id(chapter_id, 'bibliography', bib_section_id))
                                extract_inline_content(
                                    child, bibliomixed, doc_path, chapter_id, mapper,
                                    figure_counter=figure_counter, section_counters=section_counters
                                )
                            _sanitize_for_bibliomixed(bibliomixed)
                            # Parse metadata from bibliography text
                            _parse_bibliography_metadata(bibliomixed)

            # Fix C-016: Process BibSection sub-divisions within the bibliography aside.
            # Springer EPUBs can have additional sections like "Recommended Reading" or
            # "Further Reading" inside <div class="BibSection"> after the main references.
            # Each BibSection becomes a bibliodiv within the bibliography element.
            bib_sections = elem.find_all('div', class_=lambda c: c and 'BibSection' in (
                ' '.join(c) if isinstance(c, list) else c))
            if bib_sections:
                # If we have BibSections AND we already processed a main <ol>, we need
                # to restructure: wrap existing bibliomixed entries in a bibliodiv,
                # then add each BibSection as its own bibliodiv.
                existing_entries = list(bibliography.findall('bibliomixed'))
                if existing_entries:
                    # Move existing entries into a bibliodiv for the main references
                    main_div = etree.SubElement(bibliography, 'bibliodiv')
                    # Move the main title from bibliography to the main bibliodiv
                    main_title = bibliography.find('title')
                    if main_title is not None:
                        bibliography.remove(main_title)
                        main_div.insert(0, main_title)
                    for entry in existing_entries:
                        bibliography.remove(entry)
                        validated_append(main_div, entry)

                for bib_section in bib_sections:
                    bib_div = validated_subelement(bibliography, 'bibliodiv')

                    # Extract heading text for the bibliodiv title
                    section_heading = bib_section.find(
                        ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'],
                        recursive=True
                    )
                    if section_heading is None:
                        section_heading = bib_section.find(
                            class_=lambda c: c and 'Heading' in (
                                ' '.join(c) if isinstance(c, list) else c))
                    if section_heading:
                        div_title = validated_subelement(bib_div, 'title')
                        div_title.text = extract_text(section_heading)

                    # Set ID on bibliodiv
                    section_id = bib_section.get('id')
                    if section_id:
                        bib_div.set('id', sanitize_anchor_id(
                            section_id, chapter_id, 'bibliography', bib_section_id))

                    # Process the inner ordered list(s) in this BibSection
                    section_bib_list = bib_section.find('ol', recursive=True)
                    if section_bib_list:
                        section_entry_counter = 0
                        for li in section_bib_list.find_all('li', recursive=False):
                            section_entry_counter += 1
                            bibliomixed = validated_subelement(bib_div, 'bibliomixed')

                            citation_content = li.find(class_='CitationContent')
                            if citation_content and citation_content.get('id'):
                                bibliomixed.set('id', sanitize_anchor_id(
                                    citation_content.get('id'), chapter_id, 'bibliography', bib_section_id))
                            else:
                                bibliomixed.set('id', generate_element_id(
                                    chapter_id, 'bibliography', bib_section_id))

                            # Extract content from the citation
                            for child in li.children:
                                if isinstance(child, Tag):
                                    child_class = child.get('class', '')
                                    if isinstance(child_class, list):
                                        child_class = ' '.join(child_class)
                                    if 'CitationNumber' in child_class:
                                        continue
                                    if 'CitationContent' in child_class:
                                        extract_inline_content(
                                            child, bibliomixed, doc_path, chapter_id, mapper,
                                            figure_counter=figure_counter,
                                            section_counters=section_counters
                                        )

                            _sanitize_for_bibliomixed(bibliomixed)
                            _parse_bibliography_metadata(bibliomixed)

                        logger.info(
                            f"Converted {section_entry_counter} bibliography entries "
                            f"from BibSection in {chapter_id}")

            # Fix C-006: Post-processing to fix double periods in bibliomixed entries.
            # These occur when text fragments are assembled and both source and
            # conversion add trailing periods (e.g., "et al.." or "2023..").
            # Use negative lookbehind/lookahead to preserve genuine ellipsis (...).
            _double_period_re = re.compile(r'(?<!\.)\.\.(?!\.)')
            for bm in bibliography.iter('bibliomixed'):
                if bm.text and '..' in bm.text:
                    bm.text = _double_period_re.sub('.', bm.text)
                for child_elem in bm:
                    if child_elem.text and '..' in child_elem.text:
                        child_elem.text = _double_period_re.sub('.', child_elem.text)
                    if child_elem.tail and '..' in child_elem.tail:
                        child_elem.tail = _double_period_re.sub('.', child_elem.tail)

            return  # Done processing aside-as-bibliography

        # Check if aside contains a figure - should convert to figure, not sidebar
        # This handles patterns like <aside id="c1-fig-0001"><section><h3>Title</h3><figure><img/></figure></section></aside>
        # Also handles: <li><aside><section><figure id="xxx">...</figure></section></aside></li>
        # Use recursive search to find figure/img at any nesting level
        inner_figure = elem.find('figure', recursive=True)

        # Get inner figure's ID (handle list case from BeautifulSoup)
        inner_figure_id = ''
        if inner_figure is not None:
            raw_id = inner_figure.get('id')
            if raw_id:
                if isinstance(raw_id, list):
                    inner_figure_id = raw_id[0] if raw_id else ''
                else:
                    inner_figure_id = str(raw_id)

        # Find image - check multiple locations
        inner_img = None
        if inner_figure:
            inner_img = inner_figure.find('img', recursive=True)
        if inner_img is None:
            # Also check directly under aside or any nested element
            inner_img = elem.find('img', recursive=True)

        # If aside or inner figure has figure-like ID, it MUST be converted to figure to preserve as valid link target
        # Links reference this ID, and sidebar is not a valid link target
        elem_id = elem.get('id', '')
        if isinstance(elem_id, list):
            elem_id = elem_id[0] if elem_id else ''

        # Check both aside ID and inner figure ID for figure-like patterns
        id_to_check = elem_id or inner_figure_id or ''
        has_figure_like_id = (
            'fig' in id_to_check.lower() or
            'chart' in id_to_check.lower() or
            'image' in id_to_check.lower()
        )

        # Convert to figure if:
        # 1. Has an image (most common case), OR
        # 2. Has a figure-like ID (MUST preserve as valid link target regardless of content)
        is_figure_aside = (
            inner_img is not None or
            has_figure_like_id
        )

        if is_figure_aside:
            figure_counter['count'] += 1
            current_parent = section_stack[-1][1] if section_stack else parent_elem

            # Check if we're inside a table cell - figure not allowed there
            in_table_cell, entry_elem = is_inside_table_cell(current_parent)

            if in_table_cell:
                # Inside table cell - use mediaobject directly (no figure wrapper)
                if inner_img is not None:
                    img_src = inner_img.get('src', '')
                    if img_src:
                        result = resolve_image_path(img_src, doc_path, mapper)
                        if result:
                            intermediate_name, normalized_path = result
                            mediaobject = validated_subelement(current_parent, 'mediaobject')
                            imageobject = validated_subelement(mediaobject, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)
                            mapper.add_reference(normalized_path, chapter_id)
                            # Add alt text as textobject
                            alt_text = inner_img.get('alt', '')
                            if alt_text:
                                textobject = validated_subelement(mediaobject, 'textobject')
                                # Avoid <phrase> to prevent downstream spacing issues
                                tpara = validated_subelement(textobject, 'para')
                                tpara.text = alt_text
                return  # Done processing aside-as-figure inside table cell

            # Normal case - create figure element
            figure = validated_subelement(current_parent, 'figure')

            # Preserve ID - check aside first, then inner figure
            # IMPORTANT: Register BOTH IDs if both exist so links to either will resolve
            current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
            if elem_id:
                generated_id = sanitize_anchor_id(elem_id, chapter_id, 'figure', current_sect1)
                figure.set('id', generated_id)
                # Also register inner figure's ID if different, so links to it resolve too
                if inner_figure_id and inner_figure_id != elem_id:
                    register_id_mapping(chapter_id, inner_figure_id, generated_id)
                    logger.debug(f"Registered additional figure ID mapping: {inner_figure_id} -> {generated_id}")
            elif inner_figure_id:
                # Use inner figure's ID if aside has no ID
                figure.set('id', sanitize_anchor_id(inner_figure_id, chapter_id, 'figure', current_sect1))
            else:
                figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))

            # Look for title in various places: h1-h6, figcaption, section heading
            title_text = None
            heading_elem = None
            for h_tag in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
                h_elem = elem.find(h_tag)
                if h_elem is None:
                    # Check inside section
                    inner_section = elem.find('section')
                    if inner_section:
                        h_elem = inner_section.find(h_tag)
                if h_elem:
                    heading_elem = h_elem
                    title_text = extract_text(h_elem)
                    break

            figcaption = None
            if not title_text:
                figcaption = elem.find('figcaption')
                if figcaption is None and inner_figure:
                    figcaption = inner_figure.find('figcaption')
                if figcaption:
                    # Exclude table elements to handle malformed HTML with missing </figcaption>
                    title_text = extract_text(figcaption,
                                             exclude_tags={'table', 'tr', 'td', 'th', 'thead', 'tbody', 'tfoot', 'caption', 'colgroup', 'col'})
            # Register anchor IDs from caption/heading to point to this figure's ID.
            if heading_elem is not None:
                _register_label_anchor_ids(heading_elem, chapter_id, figure.get('id'), 'figure')
            if figcaption is not None:
                _register_label_anchor_ids(figcaption, chapter_id, figure.get('id'), 'figure')
            adjacent_caption = _find_adjacent_caption_elem(elem, 'figure')
            if adjacent_caption is not None and adjacent_caption not in [heading_elem, figcaption]:
                _register_label_anchor_ids(adjacent_caption, chapter_id, figure.get('id'), 'figure')

            title_elem = validated_subelement(figure, 'title')
            if title_text:
                title_elem.text = title_text

            # Create mediaobject for the image (only if we have an image)
            if inner_img is not None:
                mediaobject = validated_subelement(figure, 'mediaobject')
                imageobject = validated_subelement(mediaobject, 'imageobject')
                imagedata = validated_subelement(imageobject, 'imagedata')

                img_src = inner_img.get('src', '')
                if img_src:
                    result = resolve_image_path(img_src, doc_path, mapper)
                    if result:
                        intermediate_name, normalized_path = result
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)
                    else:
                        imagedata.set('fileref', f"missing_{figure_counter['count']}.jpg")

                # Add alt text as textobject
                alt_text = inner_img.get('alt', '')
                if alt_text:
                    textobject = validated_subelement(mediaobject, 'textobject')
                    # Avoid <phrase> to prevent downstream spacing issues
                    tpara = validated_subelement(textobject, 'para')
                    tpara.text = alt_text
            elif inner_figure is not None:
                # No direct image but there's an inner figure - process its content
                # This handles cases like aside containing a figure with a table or other content
                for child in inner_figure.children:
                    if isinstance(child, Tag):
                        if child.name == 'img':
                            # Process image - wrap in mediaobject
                            mediaobject = validated_subelement(figure, 'mediaobject')
                            imageobject = validated_subelement(mediaobject, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            img_src = child.get('src', '')
                            if img_src:
                                result = resolve_image_path(img_src, doc_path, mapper)
                                if result:
                                    intermediate_name, normalized_path = result
                                    imagedata.set('fileref', intermediate_name)
                                    mapper.add_reference(normalized_path, chapter_id)
                        elif child.name == 'table':
                            # Inner figure contains a table - process it
                            # Use correct argument order for convert_element
                            convert_element(child, figure, [], doc_path, chapter_id,
                                          mapper, figure_counter, table_counter,
                                          toc_depth_map, section_counters, in_sidebar)
                        elif child.name not in ('figcaption',):
                            # Process other content with correct argument order
                            convert_element(child, figure, [], doc_path, chapter_id,
                                          mapper, figure_counter, table_counter,
                                          toc_depth_map, section_counters, in_sidebar)
            else:
                # No image and no inner figure - aside has figure-like ID but no content
                # Create empty mediaobject to satisfy DTD requirement for figure content
                mediaobject = validated_subelement(figure, 'mediaobject')
                imageobject = validated_subelement(mediaobject, 'imageobject')
                imagedata = validated_subelement(imageobject, 'imagedata')
                imagedata.set('fileref', 'placeholder.jpg')

            return  # Done processing aside-as-figure

        # DTD Requirement: Sidebars must be inside sect1 (not directly under chapter/preface/appendix)
        # Per ritthier2.mod, chapters can only contain: beginpage?, chapterinfo?, title, subtitle?,
        # titleabbrev?, tocchap?, (toc|lot|index|glossary|bibliography|sect1)*
        # sidebar is allowed inside sect1-5, simplesect, listitem, entry, blockquote

        # First check if sidebar is allowed in this root element type
        if not _is_valid_element_for_parent('sidebar', parent_elem, elem.get('id')):
            # Skip sidebar in dedication - convert content to para
            text_content = extract_text(elem)
            if text_content and text_content.strip():
                current_parent = section_stack[-1][1] if section_stack else parent_elem
                para = validated_subelement(current_parent, 'para')
                para.text = text_content.strip()[:200]
            return

        sidebar_containers = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'sect6',
                              'section', 'listitem', 'entry', 'blockquote', 'simplesect'}

        if section_stack:
            current_parent = section_stack[-1][1]
        elif parent_elem.tag in sidebar_containers:
            # Parent already allows sidebar - use it directly
            current_parent = parent_elem
        else:
            # Need to create a wrapper section (e.g., when parent is 'chapter' without sections)
            default_section = etree.Element('sect1')
            default_section.set('id', id_gen_section_id(chapter_id, 1, section_counters))
            section_title = validated_subelement(default_section, 'title')

            # Use chapter title for the synthetic wrapper sect1
            # (the sidebar itself will have its own title extracted separately)
            chapter_title_elem = parent_elem.find('title')
            if chapter_title_elem is not None and chapter_title_elem.text and chapter_title_elem.text.strip():
                section_title.text = chapter_title_elem.text.strip()
            else:
                # Fallback if chapter has no title
                section_title.text = "Content"

            validated_append(parent_elem, default_section)
            section_stack.append((1, default_section))
            current_parent = default_section
        sidebar = validated_subelement(current_parent, 'sidebar')

        # Always set ID (use original if present, otherwise generate one)
        figure_counter['sidebar'] += 1
        elem_id = elem.get('id')
        # current_sect1 already defined at start of aside handler
        if elem_id:
            sidebar.set('id', sanitize_anchor_id(elem_id, chapter_id, 'sidebar', current_sect1))
        else:
            sidebar.set('id', generate_element_id(chapter_id, 'sidebar', current_sect1))

        # Look for a title in the aside (first heading, then first <p> as fallback)
        title_elem = elem.find(['h1', 'h2', 'h3', 'h4', 'h5', 'h6'])
        if title_elem:
            sidebar_title = validated_subelement(sidebar, 'title')
            sidebar_title.text = extract_text(title_elem)
            # Register anchor IDs from title to point to this sidebar's ID.
            _register_label_anchor_ids(title_elem, chapter_id, sidebar.get('id'), 'sidebar')
            # Track this heading element so it's skipped during recursive processing
            # This prevents duplicate title+bridgehead when heading is nested in a section
            if section_counters is not None:
                if '_skip_sidebar_title_headings' not in section_counters:
                    section_counters['_skip_sidebar_title_headings'] = set()
                # Use id() to track the BeautifulSoup element reference
                section_counters['_skip_sidebar_title_headings'].add(id(title_elem))
        else:
            # Fallback: use first <p> element as title if no heading found
            first_p = elem.find('p')
            if first_p:
                sidebar_title = validated_subelement(sidebar, 'title')
                sidebar_title.text = extract_text(first_p)
                # Track this <p> element so it's skipped during recursive processing
                if section_counters is not None:
                    if '_skip_sidebar_title_headings' not in section_counters:
                        section_counters['_skip_sidebar_title_headings'] = set()
                    section_counters['_skip_sidebar_title_headings'].add(id(first_p))
                # Mark as title_elem for consistent skip handling below
                title_elem = first_p

        # DTD Requirement: sidebar.mix allows specific elements only:
        # - Lists (itemizedlist, orderedlist, variablelist, simplelist)
        # - Paragraphs (para, formalpara, simpara)
        # - Admonitions (note, warning, important, tip, caution)
        # - Code blocks (programlisting, screen, literallayout)
        # - Tables/figures (table, figure, informalfigure, informaltable)
        # - Other: procedure, bridgehead, anchor, indexterm

        # Track if we've added any content to sidebar
        sidebar_has_content = False

        # Process children into the sidebar
        for child in elem.children:
            if isinstance(child, Tag):
                # Skip the title element we already processed
                if title_elem and child == title_elem:
                    continue

                # Skip heading tags (h1-h6) inside sidebar - they would create sections which aren't allowed
                if child.name in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
                    # Convert headings to bridgehead (allowed in sidebar.mix)
                    bridgehead = validated_subelement(sidebar, 'bridgehead')
                    bridgehead.text = None
                    extract_inline_content(
                        child,
                        bridgehead,
                        doc_path,
                        chapter_id,
                        mapper,
                        section_parent=sidebar,
                        figure_counter=figure_counter,
                        exclude_elements=_heading_exclude_elements(child),
                        section_counters=section_counters,
                    )
                    sidebar_has_content = True
                    continue

                # Process other elements in sidebar context
                # CRITICAL: Pass empty section_stack so elements are added to sidebar, not section
                # When section_stack is empty, convert_element uses parent_elem (sidebar) as parent
                # Pass in_sidebar=True to convert any nested headings to bridgehead (sections not allowed in sidebar)
                convert_element(child, sidebar, [], doc_path, chapter_id,
                              mapper, figure_counter, table_counter,
                              toc_depth_map, section_counters, True)
                sidebar_has_content = True
            elif isinstance(child, NavigableString):
                # Handle direct text content
                text = str(child)
                if text and text.strip():
                    # Wrap in para (required by DTD)
                    para = validated_subelement(sidebar, 'para')
                    para.text = text
                    sidebar_has_content = True

        # DTD Requirement: Sidebar must have at least one child from sidebar.mix
        if not sidebar_has_content:
            # Add a minimal para to satisfy DTD
            para = validated_subelement(sidebar, 'para')
            para.text = "No content"

    # Divs and other containers - process children
    elif tag_name in ['div', 'section', 'article', 'main', 'header', 'footer']:
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        # Check for special div classes
        css_class = elem.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        css_class_lower = css_class.lower()

        # Check for footnote divs (common EPUB pattern)
        epub_type = elem.get('epub:type', '') or elem.get('data-type', '')
        is_footnote_div = (
            'footnote' in epub_type.lower() or
            'footnote' in css_class_lower or
            'fn' in css_class_lower.split() or
            'endnote' in css_class_lower
        )

        if is_footnote_div:
            # Create DocBook footnote element
            # NOTE: footnote cannot be direct child of listitem - must be inside para
            footnote_parent = parent_elem
            if parent_elem.tag == 'listitem':
                # Wrap footnote in para for DTD compliance
                footnote_parent = validated_subelement(parent_elem, 'para')
            footnote_elem = validated_subelement(footnote_parent, 'footnote')

            # Preserve ID for cross-referencing, or generate one if missing
            elem_id = elem.get('id')
            if elem_id:
                footnote_elem.set('id', sanitize_anchor_id(elem_id, chapter_id, 'footnote', current_sect1))
            else:
                # Auto-generate footnote ID using hierarchical pattern
                from id_authority import generate_compliant_id
                existing_ids = section_counters.get('_existing_ids', set()) if section_counters else set()
                footnote_id = generate_compliant_id(
                    element_type='footnote',
                    chapter_id=chapter_id,
                    section_id=current_sect1,
                    existing_ids=existing_ids
                )
                footnote_elem.set('id', footnote_id)
                if section_counters:
                    if '_existing_ids' not in section_counters:
                        section_counters['_existing_ids'] = set()
                    section_counters['_existing_ids'].add(footnote_id)

            # Process footnote content
            footnote_has_content = False
            for child in elem.children:
                if isinstance(child, Tag):
                    if child.name == 'p':
                        para = validated_subelement(footnote_elem, 'para')
                        extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                        footnote_has_content = True
                    elif child.name in ['div', 'span']:
                        para = validated_subelement(footnote_elem, 'para')
                        extract_inline_content(child, para, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                        footnote_has_content = True
                    else:
                        convert_element(child, footnote_elem, [], doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                        footnote_has_content = True
                elif isinstance(child, NavigableString):
                    text = str(child)
                    if text and text.strip():
                        para = validated_subelement(footnote_elem, 'para')
                        para.text = text
                        footnote_has_content = True

            if not footnote_has_content:
                para = validated_subelement(footnote_elem, 'para')

            return

        # Handle TOC section - process as regular chapter content with links
        # Note: We intentionally do NOT create <toc> elements here.
        # TOC generation in Book.XML is disabled, and <toc> alone doesn't satisfy
        # chapter %bookcomponent.content% requirements. Keep TOC content as paras.
        if is_toc_section(elem):
            # Just process children normally - this converts TOC links to para with ulink
            for child in elem.children:
                if isinstance(child, Tag):
                    convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
            return

        # Handle chapter metadata wrapper divs (Springer EPUBs) - transparent containers (C-028 fix)
        # These contain structured children that have their own handlers; the wrapper
        # itself should not create any elements, just process children in context.
        metadata_transparent_wrappers = [
            'authorgroup', 'chaptercontextinformation', 'contextinformation',
            'affiliations', 'affiliation', 'contacts',
        ]
        if css_class_lower in metadata_transparent_wrappers:
            # Register the wrapper's own ID if present, so links targeting it
            # (e.g., <a href="#Aff3"> for affiliation superscripts) can resolve.
            # Map to the nearest parent section ID so the link points to the metadata section.
            wrapper_id = elem.get('id')
            if wrapper_id:
                target_id = None
                if section_stack:
                    target_id = section_stack[-1][1].get('id')
                elif parent_elem is not None:
                    target_id = parent_elem.get('id')
                if target_id:
                    register_id_mapping(chapter_id, wrapper_id, target_id)
                    logger.debug(f"Registered metadata wrapper ID: {wrapper_id} -> {target_id}")
            for child in elem.children:
                if isinstance(child, Tag):
                    convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
            return

        # Handle chapter metadata divs with inline content (Springer EPUBs) (C-028 fix)
        # These create a single <para> with inline formatting preserved (superscripts, links).
        # Handle AuthorNames div: single <para> with all authors inline, preserving
        # superscript affiliation markers and connectors like "and" (C-028 fix)
        if css_class_lower == 'authornames':
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            para.set('role', 'AuthorGroup')
            extract_inline_content(elem, para, doc_path, chapter_id, mapper,
                                 figure_counter=figure_counter, section_counters=section_counters)
            if para.text is None and len(para) == 0:
                current_parent.remove(para)
            return

        # Handle TocAuthors div: combine all author names into a single <para>
        # Source: <div class="TocAuthors"><span class="Author"><span class="AuthorName">Name</span>, </span>...</div>
        # Without this, each author name, comma, and "and" becomes a separate <para>.
        if css_class_lower == 'tocauthors':
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            para.set('role', 'TocAuthors')
            extract_inline_content(elem, para, doc_path, chapter_id, mapper,
                                 figure_counter=figure_counter, section_counters=section_counters)
            if para.text is None and len(para) == 0:
                current_parent.remove(para)
            return

        metadata_inline_div_classes = [
            'contact',  # Author contact info block: name, email, address on separate lines
        ]
        if css_class_lower in metadata_inline_div_classes:
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            para.set('role', css_class)
            # Process children with <?lb?> separators, using extract_inline_content
            # to preserve links, superscripts, etc.
            first_field = True
            for child in elem.children:
                if isinstance(child, Tag):
                    child_css = child.get('class', '')
                    if isinstance(child_css, list):
                        child_css = ' '.join(child_css)
                    # Skip ContactIcon divs/spans
                    if 'ContactIcon' in child_css or 'ClearBoth' in child_css:
                        continue
                    if child.name == 'br':
                        lb_pi = etree.ProcessingInstruction('lb')
                        lb_pi.tail = ''
                        validated_append(para, lb_pi)
                        first_field = True
                    else:
                        if not first_field:
                            lb_pi = etree.ProcessingInstruction('lb')
                            lb_pi.tail = ''
                            validated_append(para, lb_pi)
                        extract_inline_content(child, para, doc_path, chapter_id, mapper,
                                             figure_counter=figure_counter, section_counters=section_counters)
                        first_field = False
                elif isinstance(child, NavigableString):
                    text = str(child).strip()
                    if text:
                        if len(para) == 0:
                            para.text = (para.text or '') + text
                        else:
                            para[-1].tail = (para[-1].tail or '') + text
                        first_field = False
            if para.text is None and len(para) == 0:
                current_parent.remove(para)
            return

        # Handle chapter metadata divs that contain text content (Springer EPUBs)
        # These should become paragraphs to preserve copyright, affiliation, etc.
        # NOTE: Must check BEFORE frontmatter check since 'copyright' in frontmatter
        # would match 'chaptercopyright' as substring
        metadata_text_div_classes = [
            'chaptercopyright', 'affiliationtext',
            # Copyright page metadata fields (Springer EPUBs)
            # These contain adjacent child elements (ISSN, ISBN, SeriesTitle) that
            # concatenate without separators if processed as flat text.
            'copyrightpageissn', 'copyrightpageisbn', 'copyrightpageseriestitle',
            'seriestitle', 'bookeditiionnumber',
            # Collaborator/editorial board sections
            'collaboratorlist', 'editorialboard',
        ]
        if any(mc in css_class_lower for mc in metadata_text_div_classes):
            # Treat as paragraph with structured line breaks between child fields.
            # Uses <?lb?> processing instructions to preserve visual separation.
            # XSL renders <?lb?> as <br/> (RittBook.xsl lines 517-523, biblio.xsl line 711).
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            para.set('role', css_class)

            # Process child elements, preserving line breaks between fields
            first_field = True
            for child in elem.children:
                if isinstance(child, Tag):
                    if child.name == 'br':
                        # Insert <?lb?> processing instruction for line break
                        lb_pi = etree.ProcessingInstruction('lb')
                        lb_pi.tail = ''
                        validated_append(para, lb_pi)
                        first_field = True
                    elif child.name in ('p', 'div', 'span'):
                        # Strip footnote/affiliation markers before extracting text
                        # These are <sup>, <a> with footnote refs, and ContactIcon spans
                        import copy
                        child_copy = copy.copy(child)
                        for marker in child_copy.find_all(['sup', 'a']):
                            # Check if it's a footnote/affiliation marker
                            marker_text = marker.get_text(strip=True)
                            if marker_text and (marker_text.isdigit() or
                                                marker.get('class') and any(
                                                    c in ' '.join(marker.get('class', []))
                                                    for c in ['ContactIcon', 'Footnote', 'fn'])):
                                marker.decompose()
                        child_text = extract_text(child_copy).strip()
                        # Also strip trailing affiliation numbers like "(1)"
                        child_text = re.sub(r'\s*\(\d+\)\s*$', '', child_text)
                        if child_text:
                            if not first_field:
                                # Add <?lb?> separator between metadata fields
                                lb_pi = etree.ProcessingInstruction('lb')
                                lb_pi.tail = ''
                                validated_append(para, lb_pi)
                            # Append text to para
                            if len(para) == 0:
                                para.text = (para.text or '') + child_text
                            else:
                                para[-1].tail = (para[-1].tail or '') + child_text
                            first_field = False
                elif isinstance(child, NavigableString):
                    text = str(child).strip()
                    if text:
                        if len(para) == 0:
                            para.text = (para.text or '') + text
                        else:
                            para[-1].tail = (para[-1].tail or '') + text
                        first_field = False

            # Remove empty para if no content was found
            if para.text is None and len(para) == 0:
                current_parent.remove(para)
            return

        # Handle frontmatter/backmatter sections - just process children
        # These are semantic wrappers that shouldn't create additional structure
        frontmatter_backmatter_types = FRONT_BACK_ROLE_TYPES + [
            # Additional front/back markers commonly used as CSS classes
            'copyright', 'introduction', 'prologue', 'halftitle', 'half-title',
            'epilogue',
        ]

        role_attr = elem.get('role', '')
        is_frontmatter_backmatter = (
            any(ft in epub_type.lower() for ft in frontmatter_backmatter_types) or
            any(ft in role_attr.lower() for ft in frontmatter_backmatter_types) or
            any(ft in css_class_lower for ft in frontmatter_backmatter_types)
        )

        # Skip empty/layout divs that don't contain meaningful content
        empty_div_classes = ['clearboth', 'clear-both', 'clearfix']
        if any(ec in css_class_lower for ec in empty_div_classes):
            return

        # Treat div.Para (and similar) as paragraph containers with mixed content
        is_para_div = _is_para_div(css_class)
        if is_para_div and not _is_sidebar_div(css_class) and 'figure' not in css_class_lower:
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            existing_role = elem.get('role', '')

            # Block-level tags that should split paragraph runs
            block_elements = {
                'p', 'div', 'section', 'article', 'aside', 'header', 'footer', 'nav', 'main',
                'figure', 'figcaption', 'table', 'thead', 'tbody', 'tfoot', 'tr', 'td', 'th',
                'blockquote', 'pre', 'hr', 'ul', 'ol', 'li', 'dl', 'dt', 'dd', 'img',
                'h1', 'h2', 'h3', 'h4', 'h5', 'h6'
            }

            inline_buffer: List = []

            def _buffer_has_content(nodes: List) -> bool:
                for node in nodes:
                    if isinstance(node, NavigableString):
                        if str(node).strip():
                            return True
                    elif isinstance(node, Tag):
                        if extract_text(node):
                            return True
                return False

            def _flush_inline_buffer() -> None:
                if not inline_buffer:
                    return
                if not _buffer_has_content(inline_buffer):
                    inline_buffer.clear()
                    return

                para = validated_subelement(current_parent, 'para')
                if existing_role:
                    para.set('role', existing_role)
                elif css_class:
                    para.set('role', css_class)

                for node in inline_buffer:
                    if isinstance(node, NavigableString):
                        text = _normalize_inline_whitespace(str(node))
                        if text:
                            if len(para) == 0:
                                para.text = (para.text or '') + text
                            else:
                                para[-1].tail = (para[-1].tail or '') + text
                    elif isinstance(node, Tag):
                        extract_inline_content(
                            node,
                            para,
                            doc_path,
                            chapter_id,
                            mapper,
                            figure_counter=figure_counter,
                            include_root=True,
                            section_counters=section_counters,
                        )

                inline_buffer.clear()

            for child in elem.children:
                if isinstance(child, NavigableString):
                    inline_buffer.append(child)
                elif isinstance(child, Tag):
                    if child.name in block_elements:
                        _flush_inline_buffer()
                        convert_element(child, current_parent, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map,
                                      section_counters, in_sidebar)
                    else:
                        inline_buffer.append(child)

            _flush_inline_buffer()
            return
        # Handle block equations (div.Equation, div.NumberedEquation) - Springer EPUBs
        # These contain MathML that would otherwise be fragmented into separate paragraphs
        # Structure: div.Equation > div.EquationWrapper > div.EquationContent > math + div.EquationNumber
        # Note: Must EXCLUDE inline equations (InlineEquation) - these should be processed inline
        equation_div_classes = ['equation', 'numberedequation', 'displayequation', 'blockequation']
        is_block_equation_div = (
            any(ec in css_class_lower for ec in equation_div_classes) and
            'inline' not in css_class_lower
        )
        if is_block_equation_div:
            current_parent = section_stack[-1][1] if section_stack else parent_elem

            # Find the math element (may be nested in EquationContent)
            math_elem = elem.find('math')
            if math_elem is None:
                # Try nested structure
                for descendant in elem.descendants:
                    if hasattr(descendant, 'name') and descendant.name == 'math':
                        math_elem = descendant
                        break

            # Find equation number (may be in div.EquationNumber)
            eq_number = None
            eq_num_elem = elem.find(class_=re.compile(r'equationnumber', re.I))
            if eq_num_elem:
                eq_number = extract_text(eq_num_elem)

            if math_elem:
                # Get alttext (LaTeX representation) or altimg (SVG image)
                alttext = math_elem.get('alttext', '')
                altimg = math_elem.get('altimg', '')

                if altimg:
                    # Create figure with mediaobject for the equation image
                    para = validated_subelement(current_parent, 'para')
                    para.set('role', 'equation')

                    result = resolve_image_path(altimg, doc_path, mapper)
                    if result:
                        intermediate_name, normalized_path = result
                        inlinemedia = validated_subelement(para, 'inlinemediaobject')
                        imageobject = validated_subelement(inlinemedia, 'imageobject')
                        imagedata = validated_subelement(imageobject, 'imagedata')
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)

                        # Add alt text if available
                        if alttext:
                            textobject = validated_subelement(inlinemedia, 'textobject')
                            phrase = validated_subelement(textobject, 'phrase')
                            phrase.text = alttext.strip()

                    # If image failed, use alttext as equation content
                    if not result and alttext:
                        para.text = alttext.strip()

                    # Add equation number after the image or alttext
                    if eq_number:
                        if len(para) > 0:
                            para[-1].tail = (para[-1].tail or '') + ' ' + eq_number.strip()
                        elif para.text:
                            para.text = para.text + ' ' + eq_number.strip()
                        else:
                            para.text = eq_number.strip()

                    # If para is still empty (no image, no alttext, no eq_number), remove it
                    if len(para) == 0 and not (para.text and para.text.strip()):
                        current_parent.remove(para)

                elif alttext:
                    # No image, use alttext (LaTeX) as paragraph content
                    para = validated_subelement(current_parent, 'para')
                    para.set('role', 'equation')
                    eq_text = alttext.strip()
                    if eq_number:
                        eq_text = eq_text + ' ' + eq_number.strip()
                    para.text = eq_text
                else:
                    # Fallback: extract text content from math element
                    math_text = extract_text(math_elem)
                    if math_text and math_text.strip():
                        para = validated_subelement(current_parent, 'para')
                        para.set('role', 'equation')
                        eq_text = math_text.strip()
                        if eq_number:
                            eq_text = eq_text + ' ' + eq_number.strip()
                        para.text = eq_text
            else:
                # No math element found - extract all text as fallback
                eq_text = extract_text(elem)
                if eq_text and eq_text.strip():
                    para = validated_subelement(current_parent, 'para')
                    para.set('role', 'equation')
                    para.text = eq_text.strip()
            return

        # Handle div.figure - convert to DocBook figure element
        if 'figure' in css_class:
            current_parent = section_stack[-1][1] if section_stack else parent_elem

            # Check if we're inside a table cell - figure not allowed there
            in_table_cell, entry_elem = is_inside_table_cell(current_parent)

            # Find img element (may be nested in p.fig or directly in div)
            img = elem.find('img')
            if not img:
                # Look for img inside nested p
                p_fig = elem.find('p')
                if p_fig:
                    img = p_fig.find('img')

            if in_table_cell:
                # Inside table cell - use mediaobject directly (no figure wrapper)
                if img:
                    img_src = img.get('src', '')
                    if img_src:
                        result = resolve_image_path(img_src, doc_path, mapper)
                        if result:
                            intermediate_name, normalized_path = result
                            mediaobject = validated_subelement(current_parent, 'mediaobject')
                            imageobject = validated_subelement(mediaobject, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)
                            mapper.add_reference(normalized_path, chapter_id)
                            # Add alt text as textobject if present
                            alt_text = img.get('alt', '')
                            if alt_text:
                                textobject = validated_subelement(mediaobject, 'textobject')
                                # Avoid <phrase> to prevent downstream spacing issues
                                tpara = validated_subelement(textobject, 'para')
                                tpara.text = alt_text
                return  # Done processing div.figure inside table cell

            # Normal case - create figure element
            figure_counter['count'] += 1
            figure = validated_subelement(current_parent, 'figure')

            # Always set ID (use original if present, otherwise generate one)
            elem_id = elem.get('id')
            current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
            if elem_id:
                figure.set('id', sanitize_anchor_id(elem_id, chapter_id, 'figure', current_sect1))
            else:
                figure.set('id', generate_element_id(chapter_id, 'figure', current_sect1))

            # Look for caption (figcaption element or caption-like class)
            caption_elem = elem.find('figcaption') or elem.find(class_=re.compile(r'(figcaption|figurecaption|figurelabel|figlabel|caption)', re.IGNORECASE))
            if caption_elem is None:
                caption_elem = _find_adjacent_caption_elem(elem, 'figure')
            title_elem = validated_subelement(figure, 'title')
            if caption_elem:
                # Register anchor IDs from caption to point to this figure's ID.
                _register_label_anchor_ids(caption_elem, chapter_id, figure.get('id'), 'figure')
                # Exclude table elements to handle malformed HTML with missing </figcaption>
                extract_inline_content(caption_elem, title_elem, doc_path, chapter_id, mapper,
                                     figure_counter=figure_counter,
                                     exclude_tags={'table', 'tr', 'td', 'th', 'thead', 'tbody', 'tfoot', 'caption', 'colgroup', 'col'},
                                     section_counters=section_counters)
            # Otherwise leave title empty - DTD allows empty title

            if img:
                mediaobject = validated_subelement(figure, 'mediaobject')
                imageobject = validated_subelement(mediaobject, 'imageobject')
                imagedata = validated_subelement(imageobject, 'imagedata')

                img_src = img.get('src', '')
                if img_src:
                    result = resolve_image_path(img_src, doc_path, mapper)
                    if result:
                        intermediate_name, normalized_path = result
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)
                    else:
                        imagedata.set('fileref', f"missing_{figure_counter['count']}.jpg")

                # Add alt text as textobject if present
                alt_text = img.get('alt', '')
                if alt_text:
                    textobject = validated_subelement(mediaobject, 'textobject')
                    # Avoid <phrase> to prevent downstream spacing issues
                    tpara = validated_subelement(textobject, 'para')
                    tpara.text = alt_text

        # Handle div with sidebar/box/exercise/question classes - convert to <sidebar>
        # BUT: Skip if already inside a sidebar (nesting not allowed by DTD)
        elif _is_sidebar_div(css_class) and not in_sidebar:
            # DTD Compliance: Sidebar is NOT allowed in dedication or toc
            if not _is_valid_element_for_parent('sidebar', parent_elem, elem.get('id')):
                # Convert sidebar content to simple paras for dedication/toc
                for child in elem.children:
                    if isinstance(child, Tag):
                        convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter,
                                      toc_depth_map, section_counters, True)
                    elif isinstance(child, NavigableString):
                        text = str(child)
                        if text and text.strip():
                            para = validated_subelement(parent_elem, 'para')
                            para.text = text
                return

            # DTD Requirement: Sidebars must be inside sections, not direct children of chapter
            # If no sections exist yet, we need to create content first
            if not section_stack:
                # Create a minimal section to hold the sidebar
                section = etree.Element('sect1')
                section.set('role', 'auto-generated')
                section.set('id', id_gen_section_id(chapter_id, 1, section_counters))
                title_elem = validated_subelement(section, 'title')

                # Use chapter title for the synthetic wrapper sect1
                # (the sidebar itself will have its own title extracted separately)
                chapter_title_elem = parent_elem.find('title')
                if chapter_title_elem is not None and chapter_title_elem.text and chapter_title_elem.text.strip():
                    title_elem.text = chapter_title_elem.text.strip()
                else:
                    # Fallback if chapter has no title
                    title_elem.text = "Content"

                validated_append(parent_elem, section)
                section_stack.append((1, section))

            current_parent = section_stack[-1][1]
            sidebar = validated_subelement(current_parent, 'sidebar')

            # Always set ID (use original if present, otherwise generate one)
            figure_counter['sidebar'] += 1
            elem_id = elem.get('id')
            # current_sect1 already defined at start of div handler
            if elem_id:
                sidebar.set('id', sanitize_anchor_id(elem_id, chapter_id, 'sidebar', current_sect1))
            else:
                sidebar.set('id', generate_element_id(chapter_id, 'sidebar', current_sect1))

            # Preserve the original CSS class as role for styling/processing
            if css_class:
                sidebar.set('role', css_class)

            # Look for a title in the div - check for heading elements, title-like classes, or first <p>
            title_classes = ['exh', 'boxh', 'sth', 'sbh', 'sidebar-title', 'box-title',
                           'exercise-title', 'question-title', 'tip-title', 'note-title']
            title_found = False
            # Fix: Track div.Heading prefix for Springer FormalPara bio pattern
            # Source: <div class="Heading">Name</div><p><strong>, PhD,...</strong> bio text</p>
            # The name from div.Heading should be prepended to the title content
            heading_div_prefix = None

            # First, look for h1-h6 elements
            title_source = elem.find(['h1', 'h2', 'h3', 'h4', 'h5', 'h6'])

            # If no heading, look for div.Heading (Springer FormalPara pattern)
            # This must be checked BEFORE p-with-title-class to capture the name
            if not title_source:
                for child in elem.children:
                    if isinstance(child, Tag) and child.name == 'div':
                        child_class = child.get('class', '')
                        if isinstance(child_class, list):
                            child_class = ' '.join(child_class)
                        if child_class.strip().lower() == 'heading':
                            heading_div_prefix = child
                            # Find the next <p> sibling to combine with the heading
                            next_p = child.find_next_sibling('p')
                            if next_p:
                                title_source = next_p
                            else:
                                # No following <p>, just use the heading div as title
                                title_source = child
                                heading_div_prefix = None  # Not a prefix, it IS the title
                            break

            # If no heading, look for p with title class
            if not title_source:
                for child in elem.children:
                    if isinstance(child, Tag) and child.name == 'p':
                        child_class = child.get('class', '')
                        if isinstance(child_class, list):
                            child_class = ' '.join(child_class)
                        child_class_lower = child_class.lower()
                        if any(tc in child_class_lower for tc in title_classes):
                            title_source = child
                            break

            # If still no title, use first <p> element as fallback
            if not title_source:
                first_p = elem.find('p')
                if first_p:
                    title_source = first_p

            if title_source:
                sidebar_title = validated_subelement(sidebar, 'title')
                # If we have a heading prefix (div.Heading), extract its content first
                # so the title reads "Name, PhD, credentials..." instead of just ", PhD, credentials..."
                if heading_div_prefix and heading_div_prefix != title_source:
                    extract_inline_content(heading_div_prefix, sidebar_title, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                extract_inline_content(title_source, sidebar_title, doc_path, chapter_id, mapper, figure_counter=figure_counter, section_counters=section_counters)
                title_found = True
                # Register anchor IDs from title to point to this sidebar's ID.
                _register_label_anchor_ids(title_source, chapter_id, sidebar.get('id'), 'sidebar')
                # Track this element so it's skipped during recursive processing
                # This prevents duplicate title+para or title+bridgehead
                if section_counters is not None:
                    if '_skip_sidebar_title_headings' not in section_counters:
                        section_counters['_skip_sidebar_title_headings'] = set()
                    section_counters['_skip_sidebar_title_headings'].add(id(title_source))
                    # Also skip the heading div prefix if used
                    if heading_div_prefix:
                        section_counters['_skip_sidebar_title_headings'].add(id(heading_div_prefix))

            # Track if we've added any content to sidebar
            sidebar_has_content = False

            # Process children into the sidebar
            for child in elem.children:
                if isinstance(child, Tag):
                    # Skip the element we used as title
                    if title_found and child == title_source:
                        continue
                    # Skip the heading div prefix we used as title prefix
                    if heading_div_prefix and child == heading_div_prefix:
                        continue

                    # Skip heading tags (h1-h6) inside sidebar - convert to bridgehead
                    # Note: Nested anchor IDs and pagebreak IDs are not preserved as anchors.
                    # R2 XSL has the anchor template output commented out (html.xsl).
                    if child.name in ['h1', 'h2', 'h3', 'h4', 'h5', 'h6']:
                        bridgehead = validated_subelement(sidebar, 'bridgehead')
                        bridgehead.text = None
                        extract_inline_content(
                            child,
                            bridgehead,
                            doc_path,
                            chapter_id,
                            mapper,
                            section_parent=sidebar,
                            figure_counter=figure_counter,
                            exclude_elements=_heading_exclude_elements(child),
                            section_counters=section_counters,
                        )
                        sidebar_has_content = True
                        continue

                    # Process other elements in sidebar context
                    # Pass empty section_stack so elements are added to sidebar, not section
                    # Pass in_sidebar=True to convert any nested headings to bridgehead (sections not allowed in sidebar)
                    convert_element(child, sidebar, [], doc_path, chapter_id,
                                  mapper, figure_counter, table_counter,
                                  toc_depth_map, section_counters, True)
                    sidebar_has_content = True
                elif isinstance(child, NavigableString):
                    text = str(child)
                    if text and text.strip():
                        para = validated_subelement(sidebar, 'para')
                        para.text = text
                        sidebar_has_content = True

            # DTD Requirement: Sidebar must have at least one child from sidebar.mix
            if not sidebar_has_content:
                para = validated_subelement(sidebar, 'para')
                para.text = ""

        else:
            # Handle KeywordGroup divs: extract individual keyword spans and join with commas
            # Source structure: <div class="KeywordGroup"><span class="Keyword">term</span>...</div>
            # R2 XSL suppresses <keywordset> in content mode, so we output a <para> with
            # comma-separated keywords for visible rendering.
            css_class_lower_check = css_class.lower() if css_class else ''
            if css_class_lower_check in ('keywordgroup', 'keywords'):
                current_parent = section_stack[-1][1] if section_stack else parent_elem
                keywords = []
                for span in elem.find_all('span', class_='Keyword'):
                    kw_text = extract_text(span).strip()
                    if kw_text:
                        keywords.append(kw_text)
                if keywords:
                    para = validated_subelement(current_parent, 'para')
                    para.set('role', 'keywords')
                    para.text = ', '.join(keywords)
                    logger.debug(f"Extracted {len(keywords)} keywords from KeywordGroup div")
                return

            # Default: try XHTML mapping rules for known div classes
            # This handles div classes like 'note', 'tip', 'warning' etc.
            # that have DocBook equivalents
            docbook_tag, _ = get_docbook_tag_for_css_class('div', css_class)
            current_parent = section_stack[-1][1] if section_stack else parent_elem

            # Check if we have a specific mapping for this class
            if docbook_tag and docbook_tag not in ('para', 'simpara'):
                # Skip creating table/informaltable from wrapper divs that contain
                # real <table> elements. These wrapper divs (e.g., div.Table) should
                # be transparent — let the native <table> handler process the actual
                # table with its caption to get correct titles like "Table 2.1 ...".
                # Without this, we get nested tables and the DTD fixer discards the
                # inner one (which has the correct caption-derived title).
                if docbook_tag in ('table', 'informaltable'):
                    logger.debug(f"Skipping table wrapper div.{css_class} — transparent container")
                    # Remember how many table children exist before processing,
                    # so we can find the newly-created table afterward.
                    wrapper_id = elem.get('id')
                    tables_before = len(current_parent.findall('table')) if wrapper_id else 0
                    for child in elem.children:
                        if isinstance(child, Tag):
                            convert_element(child, current_parent, section_stack, doc_path, chapter_id,
                                          mapper, figure_counter, table_counter,
                                          toc_depth_map, section_counters, in_sidebar)
                    # Register the wrapper div's ID (e.g., "Tab2") so that
                    # InternalRef links like <a href="#Tab2"> can resolve to
                    # the DocBook table element created by the child <table>.
                    if wrapper_id:
                        tables_after = current_parent.findall('table')
                        if len(tables_after) > tables_before:
                            new_table = tables_after[-1]
                            table_docbook_id = new_table.get('id')
                            if table_docbook_id:
                                register_id_mapping(chapter_id, wrapper_id, table_docbook_id, 'table')
                                logger.debug(f"Registered table wrapper ID: {wrapper_id} -> {table_docbook_id}")
                    return

                # Validate placement before creating element
                is_valid, _ = validate_element_placement(current_parent.tag, docbook_tag)
                if is_valid:
                    logger.debug(f"Using XHTML mapping: div.{css_class} -> {docbook_tag}")
                    new_elem = validated_subelement(current_parent, docbook_tag)
                    if css_class:
                        new_elem.set('role', css_class)
                    # Process children into the new element
                    for child in elem.children:
                        if isinstance(child, Tag):
                            convert_element(child, new_elem, [], doc_path, chapter_id,
                                          mapper, figure_counter, table_counter,
                                          toc_depth_map, section_counters, in_sidebar)
                        elif isinstance(child, NavigableString) and not isinstance(child, Comment):
                            text = str(child)
                            if text and text.strip():
                                para = validated_subelement(new_elem, 'para')
                                para.text = text.strip()
                    return

            # Check if this div is a CSS-styled table (display:table, role=table, etc.)
            if _is_css_table(elem):
                _convert_css_table_to_docbook(elem, parent_elem, section_stack, doc_path,
                                              chapter_id, mapper, figure_counter, table_counter,
                                              section_counters, in_sidebar)
                return

            # Default: process children normally
            # Note: div IDs are not preserved - R2 XSL has the anchor template
            # output commented out (html.xsl).
            for child in elem.children:
                if isinstance(child, Tag):
                    # Check if child is a CSS-styled table
                    if child.name in ('div', 'section') and _is_css_table(child):
                        _convert_css_table_to_docbook(child, parent_elem, section_stack, doc_path,
                                                      chapter_id, mapper, figure_counter, table_counter,
                                                      section_counters, in_sidebar)
                    else:
                        convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                elif isinstance(child, NavigableString) and not isinstance(child, Comment):
                    # Capture text content from divs to prevent content loss
                    # Skip HTML comments (Comment is a subtype of NavigableString)
                    text = str(child)
                    if text and text.strip():
                        para = validated_subelement(current_parent, 'para')
                        para.text = text.strip()

    # Handle standalone <math> elements at block level (MathML)
    # These appear in some EPUBs outside of equation wrapper divs.
    # Convert to <equation> (block) or <inlineequation> (inline) with proper image structure.
    elif tag_name == 'math':
        current_parent = section_stack[-1][1] if section_stack else parent_elem
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None

        display_attr = elem.get('display', '').lower()
        is_block = display_attr == 'block'

        alt_text = elem.get('alttext', '')
        altimg = elem.get('altimg', '')
        if not alt_text:
            alt_text = extract_text(elem).strip()

        if is_block:
            # Block math → <equation> with <mediaobject>
            equation_elem = validated_subelement(current_parent, 'equation')
            eq_id = elem.get('id', '')
            if eq_id:
                equation_elem.set('id', sanitize_anchor_id(eq_id, chapter_id, 'eq', current_sect1))
            validated_subelement(equation_elem, 'title')  # DTD requires title

            img_resolved = False
            if altimg:
                result = resolve_image_path(altimg, doc_path, mapper)
                if result:
                    intermediate_name, normalized_path = result
                    if alt_text:
                        alt_elem = validated_subelement(equation_elem, 'alt')
                        alt_elem.text = alt_text
                    mediaobj = validated_subelement(equation_elem, 'mediaobject')
                    imageobject = validated_subelement(mediaobj, 'imageobject')
                    imagedata = validated_subelement(imageobject, 'imagedata')
                    imagedata.set('fileref', intermediate_name)
                    mapper.add_reference(normalized_path, chapter_id)
                    if alt_text:
                        textobj = validated_subelement(mediaobj, 'textobject')
                        phrase = validated_subelement(textobj, 'phrase')
                        phrase.text = alt_text
                    img_resolved = True

            # Fallback: check for <img> sibling
            if not img_resolved and elem.parent:
                img_elem = elem.parent.find('img')
                if img_elem:
                    img_src = img_elem.get('src', '')
                    if img_src:
                        result = resolve_image_path(img_src, doc_path, mapper)
                        if result:
                            intermediate_name, normalized_path = result
                            mediaobj = validated_subelement(equation_elem, 'mediaobject')
                            imageobject = validated_subelement(mediaobj, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)
                            mapper.add_reference(normalized_path, chapter_id)
                            img_resolved = True

            # Text fallback — DTD requires mediaobject inside equation
            if not img_resolved and alt_text:
                if not equation_elem.find('alt'):
                    alt_elem = validated_subelement(equation_elem, 'alt')
                    alt_elem.text = alt_text
                mediaobj = validated_subelement(equation_elem, 'mediaobject')
                textobj = validated_subelement(mediaobj, 'textobject')
                phrase = validated_subelement(textobj, 'phrase')
                phrase.text = alt_text

        else:
            # Inline math → <inlineequation> inside a <para>
            last_para = None
            for child in reversed(list(current_parent)):
                if child.tag == 'para':
                    last_para = child
                    break
            if last_para is None:
                last_para = validated_subelement(current_parent, 'para')

            inlineeq_elem = validated_subelement(last_para, 'inlineequation')
            eq_id = elem.get('id', '')
            if eq_id:
                inlineeq_elem.set('id', sanitize_anchor_id(eq_id, chapter_id, 'eq', current_sect1))

            img_resolved = False
            if altimg:
                result = resolve_image_path(altimg, doc_path, mapper)
                if result:
                    intermediate_name, normalized_path = result
                    if alt_text:
                        alt_elem = validated_subelement(inlineeq_elem, 'alt')
                        alt_elem.text = alt_text
                    inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                    imageobject = validated_subelement(inlinemedia, 'imageobject')
                    imagedata = validated_subelement(imageobject, 'imagedata')
                    imagedata.set('fileref', intermediate_name)
                    mapper.add_reference(normalized_path, chapter_id)
                    if alt_text:
                        textobj = validated_subelement(inlinemedia, 'textobject')
                        phrase = validated_subelement(textobj, 'phrase')
                        phrase.text = alt_text
                    img_resolved = True

            # Fallback: check for <img> sibling
            if not img_resolved and elem.parent:
                img_elem = elem.parent.find('img')
                if img_elem:
                    img_src = img_elem.get('src', '')
                    if img_src:
                        result = resolve_image_path(img_src, doc_path, mapper)
                        if result:
                            intermediate_name, normalized_path = result
                            inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                            imageobject = validated_subelement(inlinemedia, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)
                            mapper.add_reference(normalized_path, chapter_id)
                            img_resolved = True

            # Text fallback — DTD requires inlinemediaobject inside inlineequation
            if not img_resolved and alt_text:
                if not inlineeq_elem.find('alt'):
                    alt_elem = validated_subelement(inlineeq_elem, 'alt')
                    alt_elem.text = alt_text
                inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                textobj = validated_subelement(inlinemedia, 'textobject')
                phrase = validated_subelement(textobj, 'phrase')
                phrase.text = alt_text

    # Handle anchor-only <a> elements at block level (e.g., page markers like <a id="Page_104"/>)
    # Also handle external links and internal cross-references that appear at block level
    elif tag_name == 'a':
        href = elem.get('href', '')
        link_text = extract_text(elem)

        # Check if this is an external link (http/https/mailto/etc.)
        is_external = href and (
            href.startswith('http://') or
            href.startswith('https://') or
            href.startswith('mailto:') or
            href.startswith('ftp://') or
            (href.startswith('//') and not href.startswith('//#'))  # protocol-relative URLs
        )

        if is_external and link_text and link_text.strip():
            # Create para with ulink for external links
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            ulink = validated_subelement(para, 'ulink')
            ulink.set('url', href)
            ulink.text = link_text.strip()
        elif href and link_text and link_text.strip() and not href.startswith('#'):
            # Internal cross-reference to another EPUB file (e.g., TOC chapter links)
            # Resolve the EPUB filename to a DocBook chapter ID
            resolved = resolve_link_href(href, doc_path, mapper, chapter_id)
            if resolved and not resolved.startswith(('http://', 'https://', 'mailto:', 'ftp://', 'tel:', '//')):
                current_parent = section_stack[-1][1] if section_stack else parent_elem
                para = validated_subelement(current_parent, 'para')
                link_elem = validated_subelement(para, 'link')
                # Parse resolved href to extract linkend
                if '#' in resolved:
                    file_part, _, fragment = resolved.partition('#')
                    linkend_id = fragment or file_part
                else:
                    linkend_id = resolved
                # Strip file extension if present
                linkend_id = re.sub(r'\.(xhtml|html|xml)$', '', linkend_id, flags=re.IGNORECASE)
                link_elem.set('linkend', linkend_id)
                link_elem.text = link_text.strip()
            else:
                # Unresolved internal link - preserve as plain text
                for child in elem.children:
                    if isinstance(child, Tag):
                        convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                      mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                    elif isinstance(child, NavigableString):
                        text = str(child)
                        if text and text.strip():
                            current_parent = section_stack[-1][1] if section_stack else parent_elem
                            para = validated_subelement(current_parent, 'para')
                            para.text = text
        else:
            # Process any children (text content, etc.) - for anchor-only elements
            for child in elem.children:
                if isinstance(child, Tag):
                    convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
                elif isinstance(child, NavigableString):
                    text = str(child)
                    if text and text.strip():
                        current_parent = section_stack[-1][1] if section_stack else parent_elem
                        para = validated_subelement(current_parent, 'para')
                        para.text = text

    # Handle block-level <span> elements with IDs (e.g., section markers like <span id="c12-sec-0001"/>)
    # These need to be converted to DocBook anchors to preserve cross-reference targets
    elif tag_name == 'span':
        current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None
        elem_id = elem.get('id')
        epub_type = elem.get('epub:type', '')

        # Suppress metadata-only spans that should not produce visible output
        # AffiliationNumber: "(1)", "(2)" labels from Springer author affiliations
        # ContactIcon: envelope icon spans (contain only NBSP) in Springer author metadata
        css_class = elem.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        if css_class in ('AffiliationNumber', 'ContactIcon', 'ClearBoth', 'TocPageNumber'):
            return

        # Handle Springer metadata spans at block level (C-028 fix)
        # These create a <para> with inline content preserved (superscripts, links, etc.)
        metadata_span_classes = (
            'ContextInformationAuthorEditorNames', 'ContextInformationBookTitles',
            'ChapterDOI', 'ContactAuthorLine', 'ContactAdditionalLine',
            'ContactType',
        )
        css_class_check = css_class.split() if css_class else []
        if any(mc in css_class_check for mc in metadata_span_classes) or css_class in metadata_span_classes:
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            para = validated_subelement(current_parent, 'para')
            para.set('role', css_class)
            extract_inline_content(elem, para, doc_path, chapter_id, mapper,
                                 figure_counter=figure_counter, section_counters=section_counters)
            # Remove empty para if no content
            if para.text is None and len(para) == 0:
                current_parent.remove(para)
            return

        # Handle InlineEquation spans at block level (Springer EPUBs)
        # Pattern: <span class="InlineEquation" id="IEq1"><math altimg="...svg">...</math></span>
        # Creates <inlineequation> with <inlinemediaobject>/<imageobject>/<imagedata>
        css_class_lower_span = css_class.lower() if css_class else ''
        if 'inlineequation' in css_class_lower_span:
            current_parent = section_stack[-1][1] if section_stack else parent_elem
            # Find or create a para context for the inline equation
            last_para = None
            for child in reversed(list(current_parent)):
                if child.tag == 'para':
                    last_para = child
                    break
            if last_para is None:
                last_para = validated_subelement(current_parent, 'para')

            inlineeq_elem = validated_subelement(last_para, 'inlineequation')
            if elem_id:
                inlineeq_elem.set('id', sanitize_anchor_id(elem_id, chapter_id, 'eq', current_sect1))

            # Try to resolve equation image: altimg → <img> element → text fallback
            math_elem = elem.find('math')
            alt_text = ''
            altimg = ''
            if math_elem:
                alt_text = math_elem.get('alttext', '')
                altimg = math_elem.get('altimg', '')
                if not alt_text:
                    alt_text = extract_text(math_elem).strip()

            img_resolved = False
            if altimg:
                result = resolve_image_path(altimg, doc_path, mapper)
                if result:
                    intermediate_name, normalized_path = result
                    if alt_text:
                        alt_elem = validated_subelement(inlineeq_elem, 'alt')
                        alt_elem.text = alt_text
                    inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                    imageobject = validated_subelement(inlinemedia, 'imageobject')
                    imagedata = validated_subelement(imageobject, 'imagedata')
                    imagedata.set('fileref', intermediate_name)
                    mapper.add_reference(normalized_path, chapter_id)
                    if alt_text:
                        textobj = validated_subelement(inlinemedia, 'textobject')
                        phrase = validated_subelement(textobj, 'phrase')
                        phrase.text = alt_text
                    img_resolved = True

            # Check for <img> element as fallback
            if not img_resolved:
                img_elem = elem.find('img')
                if img_elem is None:
                    for desc in elem.descendants:
                        if hasattr(desc, 'name') and desc.name == 'img':
                            img_elem = desc
                            break
                if img_elem:
                    img_src = img_elem.get('src', '')
                    if img_src:
                        result = resolve_image_path(img_src, doc_path, mapper)
                        if result:
                            intermediate_name, normalized_path = result
                            inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                            imageobject = validated_subelement(inlinemedia, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)
                            mapper.add_reference(normalized_path, chapter_id)
                            img_resolved = True

            # Text fallback — DTD requires inlinemediaobject inside inlineequation
            if not img_resolved:
                visible_text = alt_text or extract_text(elem).strip()
                if visible_text:
                    alt_elem = validated_subelement(inlineeq_elem, 'alt')
                    alt_elem.text = visible_text
                    inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                    textobj = validated_subelement(inlinemedia, 'textobject')
                    phrase = validated_subelement(textobj, 'phrase')
                    phrase.text = visible_text
            return

        # Check if this is an empty section marker span followed by a heading
        # Example 1: <span id="c1-sec-0001"/><h2 id="head-2-1">INTRODUCTION</h2>
        # Example 2: <section><span id="c1-sec-0297"/><span pagebreak/><h4>TITLE</h4></section>
        # These are just cross-reference markers and can be safely skipped.
        # Note: IDs are NOT preserved as anchors - R2 XSL ignores them anyway (html.xsl anchor
        # template is commented out).
        if elem_id and is_empty_span_marker(elem) and (is_followed_by_heading(elem) or is_section_start_span_marker(elem)):
            # Skip this empty marker span - just process any children
            for child in elem.children:
                if isinstance(child, Tag):
                    convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                                  mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
            return

        # Note: IDs on block-level spans are intentionally not preserved as anchors.
        # R2 XSL has the anchor template output commented out (html.xsl), so anchor
        # elements produce no HTML output. Pagebreak and span IDs are discarded.
        # See documentation for details on R2 XSL ID support.

        # Process any children (text content, etc.)
        for child in elem.children:
            if isinstance(child, Tag):
                convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                              mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
            elif isinstance(child, NavigableString):
                text = str(child)
                if text and text.strip():
                    current_parent = section_stack[-1][1] if section_stack else parent_elem
                    para = validated_subelement(current_parent, 'para')
                    para.text = text

    # Fallback: unknown elements - try XHTML mapping, then process children
    else:
        # First, try XHTML to DocBook mapping for this element
        css_class = elem.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)

        current_parent = section_stack[-1][1] if section_stack else parent_elem
        docbook_tag = get_docbook_element_for_html(tag_name, css_class, current_parent.tag)

        # If we got a meaningful mapping (not just returning the original tag)
        if docbook_tag != tag_name and docbook_tag not in ('phrase', 'remark'):
            is_valid, _ = validate_element_placement(current_parent.tag, docbook_tag)
            if is_valid:
                logger.debug(f"Using XHTML mapping: <{tag_name}> -> <{docbook_tag}>")
                new_elem = validated_subelement(current_parent, docbook_tag)
                if css_class:
                    new_elem.set('role', css_class)
                # Process children into the new element
                for child in elem.children:
                    if isinstance(child, Tag):
                        convert_element(child, new_elem, [], doc_path, chapter_id,
                                      mapper, figure_counter, table_counter,
                                      toc_depth_map, section_counters, in_sidebar)
                    elif isinstance(child, NavigableString):
                        text = str(child)
                        if text and text.strip():
                            # Check if new element allows text
                            model = _get_cached_content_model()
                            if model and model.allows_text(docbook_tag):
                                if len(new_elem) == 0:
                                    new_elem.text = (new_elem.text or '') + text.strip()
                                else:
                                    new_elem[-1].tail = (new_elem[-1].tail or '') + ' ' + text.strip()
                            else:
                                para = validated_subelement(new_elem, 'para')
                                para.text = text.strip()
                return

        # Log that we're encountering an unhandled element (for debugging)
        if tag_name not in ['em', 'strong', 'b', 'i', 'code', 'br']:
            logger.debug(f"Processing unhandled element <{tag_name}> by processing its children")

        # Note: IDs on fallback/unknown elements are intentionally not preserved.
        # R2 XSL has the anchor template output commented out (html.xsl), so anchor
        # elements produce no HTML output.

        # Process children to preserve content
        for child in elem.children:
            if isinstance(child, Tag):
                convert_element(child, parent_elem, section_stack, doc_path, chapter_id,
                              mapper, figure_counter, table_counter, toc_depth_map, section_counters, in_sidebar)
            elif isinstance(child, NavigableString):
                text = str(child)
                if text and text.strip():
                    # Create para to hold orphaned text
                    para = validated_subelement(current_parent, 'para')
                    para.text = text


def extract_inline_content(elem: Tag, para: etree.Element,
                          doc_path: str, chapter_id: str,
                          mapper: ReferenceMapper,
                          section_parent: etree.Element = None,
                          figure_counter: Dict = None,
                          exclude_tags: set = None,
                          include_root: bool = False,
                          exclude_elements: set = None,
                          section_counters: Dict = None,
                          containing_element_id: str = None,
                          containing_element_type: str = None) -> None:
    """
    Extract inline content (text + formatting) into a para element.
    Now properly preserves all formatting and creates proper XML elements.

    Args:
        elem: Source HTML element
        para: Target para element
        doc_path: Document path
        chapter_id: Chapter ID
        mapper: Reference mapper
        section_parent: Section-level parent for block elements
                       (if None, will walk up tree to find appropriate parent)
        figure_counter: Counter dict for figure IDs (optional, will generate unique ID if not provided)
        exclude_tags: Set of tag names to skip during processing (e.g., {'table', 'tr', 'td'} to
                     avoid including table content when extracting title from malformed figcaption)
        include_root: If True, process elem itself as a child node (for when elem is a
                     formatting element like <b> or <strong> that should be wrapped)
        exclude_elements: Set of specific BeautifulSoup elements to skip (e.g., pagebreak spans
                         that have already been processed elsewhere)
        section_counters: Dict tracking section counters including '_current_sect1_id' for proper
                         element ID generation within the correct section context
        containing_element_id: ID of the containing figure/table element (when processing titles).
                              Used to redirect self-referencing links to the current element.
        containing_element_type: Type of containing element ('figure' or 'table')
    """
    # Get current sect1 for ID generation
    current_sect1 = section_counters.get('_current_sect1_id') if section_counters else None

    # Determine the section-level parent for block elements
    # Some elements cannot be placed inside title, para, etc.
    def find_section_parent(elem):
        """Find the nearest section-level ancestor for block elements"""
        # Section-level elements per DTD:
        # - Book components: chapter, preface, appendix, part, subpart, article, toc, lot
        # - Sections: sect1-5, section, simplesect
        # - Elements with component.mix: index, glossary, bibliography, reference
        # - Containers: book, sidebar, blockquote
        section_tags = {
            'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect',
            'chapter', 'preface', 'appendix', 'part', 'subpart', 'article',
            'toc', 'lot', 'index', 'glossary', 'bibliography', 'reference',
            'book', 'sidebar', 'blockquote'
        }
        current = elem
        while current is not None:
            if current.tag in section_tags:
                return current
            current = current.getparent()
        return None

    def find_figure_parent(elem):
        """Find the nearest ancestor that can contain a figure element"""
        # Elements that can contain figure per DTD (in divcomponent.mix or similar)
        # Note: 'entry' removed - DTD doesn't allow figure inside entry (table cells)
        # Images in table cells should use mediaobject directly without figure wrapper
        figure_container_tags = {
            # Sections
            'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'sect6', 'section', 'simplesect',
            # Book components
            'chapter', 'preface', 'appendix', 'part', 'subpart', 'article',
            'toc', 'lot', 'index', 'glossary', 'bibliography', 'reference',
            # Containers
            'book', 'listitem', 'sidebar', 'blockquote', 'footnote'
        }
        current = elem
        while current is not None:
            if current.tag in figure_container_tags:
                return current
            current = current.getparent()
        return None

    block_parent = section_parent or find_section_parent(para)
    figure_parent = find_figure_parent(para)

    # Block-level HTML elements that should ensure whitespace separation
    _BLOCK_ELEMENTS = frozenset({
        'div', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        'section', 'article', 'aside', 'header', 'footer', 'nav',
        'blockquote', 'pre', 'hr', 'address', 'figure', 'figcaption',
        'details', 'summary', 'main',
    })

    def _ensure_trailing_space(target_elem):
        """Ensure the current text content of target_elem ends with whitespace.

        Called after processing block-level elements to prevent text concatenation.
        Example: <div>...2025</div><span>D. Paul</span> → "2025 D. Paul" (C-002)
        """
        if len(target_elem) == 0:
            if target_elem.text and not target_elem.text[-1].isspace():
                target_elem.text += ' '
        else:
            last = target_elem[-1]
            tail = last.tail or ''
            if tail and not tail[-1].isspace():
                last.tail = tail + ' '

    def process_node(node, parent_elem):
        """Recursively process a node and its children"""
        if isinstance(node, NavigableString):
            text = _normalize_inline_whitespace(str(node))
            if text:
                if len(parent_elem) == 0:
                    # No children yet, append to text
                    parent_elem.text = (parent_elem.text or '') + text
                else:
                    # Has children, append to last child's tail
                    last_child = parent_elem[-1]
                    last_child.tail = (last_child.tail or '') + text
        elif isinstance(node, Tag):
            # Skip excluded tags (e.g., table elements in malformed figcaption)
            if exclude_tags and node.name in exclude_tags:
                return

            # Skip specific excluded elements (e.g., pagebreak spans already processed)
            if exclude_elements and node in exclude_elements:
                return

            # Handle formatting tags
            new_elem = None

            # DTD Compliance: Check if parent element uses restrictive content model
            # (cptr.char.mix or smallcptr.char.mix) which doesn't allow emphasis, abbrev, etc.
            # In these cases, just extract text content instead of creating invalid child elements
            parent_tag = parent_elem.tag if hasattr(parent_elem, 'tag') else ''
            restrictive_parents = {
                # cptr.char.mix elements
                'funcsynopsisinfo', 'funcparams', 'classsynopsisinfo', 'interfacename',
                'action', 'command', 'computeroutput', 'database', 'filename', 'function',
                'hardware', 'keycap', 'literal', 'option', 'optional', 'parameter',
                'property', 'systemitem', 'userinput',
                # smallcptr.char.mix elements
                'interface', 'msglevel', 'msgorig', 'modifier', 'exceptionname', 'initializer',
                'methodname', 'accel', 'classname', 'envar', 'errorcode', 'errorname',
                'errortext', 'errortype', 'guibutton', 'guiicon', 'guilabel', 'guimenu',
                'guimenuitem', 'guisubmenu', 'keycode', 'keysym', 'constant', 'varname',
                'markup', 'medialabel', 'mousebutton', 'prompt', 'returnvalue', 'sgmltag',
                'structfield', 'structname', 'symbol', 'uri', 'token', 'type',
            }
            is_in_restrictive_parent = parent_tag in restrictive_parents

            # Bold - use role="strong" per RittDoc convention
            # Fix: Check for EmphasisTypeBold CSS class (Springer) which should use role="bold"
            # The R2 platform distinguishes role="strong" (semantic) from role="bold" (visual)
            if node.name in ['strong', 'b']:
                if is_in_restrictive_parent:
                    # Just extract text - emphasis not allowed in restrictive parents
                    text = extract_text(node)
                    if text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + text
                    return
                # Check CSS class for explicit bold typing
                node_class = node.get('class', '')
                if isinstance(node_class, list):
                    node_class = ' '.join(node_class)
                if 'emphasistype' in node_class.lower().replace(' ', ''):
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='bold')
                else:
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='strong')
            # Italic
            elif node.name in ['em', 'i']:
                if is_in_restrictive_parent:
                    text = extract_text(node)
                    if text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + text
                    return
                new_elem = validated_subelement(parent_elem, 'emphasis')
            # Underline
            elif node.name == 'u':
                if is_in_restrictive_parent:
                    text = extract_text(node)
                    if text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + text
                    return
                new_elem = validated_subelement(parent_elem, 'emphasis', role='underline')
            # Strikethrough
            elif node.name in ['s', 'strike', 'del']:
                if is_in_restrictive_parent:
                    text = extract_text(node)
                    if text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + text
                    return
                new_elem = validated_subelement(parent_elem, 'emphasis', role='strikethrough')
            # Subscript
            elif node.name == 'sub':
                new_elem = validated_subelement(parent_elem, 'subscript')
            # Superscript
            elif node.name == 'sup':
                new_elem = validated_subelement(parent_elem, 'superscript')
            # Code/monospace - use <literal> (DocBook equivalent of HTML <code>)
            # Note: <code> is not declared in the RittDoc DTD
            elif node.name in ['code', 'tt', 'kbd', 'samp']:
                new_elem = validated_subelement(parent_elem, 'literal')
            # Small text (avoid <phrase> to prevent downstream spacing issues)
            elif node.name == 'small':
                if is_in_restrictive_parent:
                    text = extract_text(node)
                    if text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + text
                    return
                new_elem = validated_subelement(parent_elem, 'emphasis', role='small')
            # Mark/highlight (avoid <phrase> to prevent downstream spacing issues)
            elif node.name == 'mark':
                if is_in_restrictive_parent:
                    text = extract_text(node)
                    if text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + text
                    return
                new_elem = validated_subelement(parent_elem, 'emphasis', role='highlight')
            # Cite - title of a creative work, use citetitle
            elif node.name == 'cite':
                new_elem = validated_subelement(parent_elem, 'citetitle')
            # Abbreviation - use proper DocBook <abbrev> element
            # BUT: abbrev is NOT allowed in restrictive inline contexts (code, subscript, superscript)
            # These elements only allow #PCDATA or limited char mixes that exclude gen.char.class
            elif node.name == 'abbr':
                # Check if any ancestor forbids abbrev (DTD compliance)
                # Elements that use cptr.char.mix or smallcptr.char.mix (don't include gen.char.class, so no abbrev)
                # Also includes #PCDATA-only elements
                restrictive_elements = {
                    # cptr.char.mix elements
                    'funcsynopsisinfo', 'funcparams', 'classsynopsisinfo', 'interfacename',
                    'action', 'command', 'computeroutput', 'database', 'filename', 'function',
                    'hardware', 'keycap', 'literal', 'code', 'option', 'optional', 'parameter',
                    'property', 'systemitem', 'userinput',
                    # smallcptr.char.mix elements
                    'interface', 'msglevel', 'msgorig', 'modifier', 'exceptionname', 'initializer',
                    'methodname', 'accel', 'classname', 'envar', 'errorcode', 'errorname',
                    'errortext', 'errortype', 'guibutton', 'guiicon', 'guilabel', 'guimenu',
                    'guimenuitem', 'guisubmenu', 'keycode', 'keysym', 'constant', 'varname',
                    'markup', 'medialabel', 'mousebutton', 'prompt', 'returnvalue', 'sgmltag',
                    'structfield', 'structname', 'symbol', 'uri', 'token', 'type',
                    # #PCDATA only elements
                    'subscript', 'superscript',
                    # abbrev itself (word.char.mix doesn't include abbrev)
                    'abbrev'
                }

                # Check all ancestors for restrictive elements
                has_restrictive_ancestor = False
                current = parent_elem
                while current is not None:
                    current_tag = current.tag if hasattr(current, 'tag') else ''
                    if current_tag in restrictive_elements:
                        has_restrictive_ancestor = True
                        break
                    # Move up to parent
                    current = current.getparent() if hasattr(current, 'getparent') else None

                if has_restrictive_ancestor:
                    # Parent doesn't allow abbrev - just extract text content
                    # The abbreviation markup is lost but content is preserved
                    abbr_text = extract_text(node)
                    print(f"[DTD-FIX] Skipping abbrev '{abbr_text}' - has restrictive ancestor", flush=True)
                    if abbr_text:
                        if len(parent_elem) == 0:
                            parent_elem.text = (parent_elem.text or '') + abbr_text
                        else:
                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + abbr_text
                    # Done processing this abbr node - return early
                    return
                else:
                    # Parent allows abbrev - create it normally
                    parent_tag = parent_elem.tag if hasattr(parent_elem, 'tag') else 'unknown'
                    print(f"[DTD-FIX] Creating abbrev with parent={parent_tag}", flush=True)
                    new_elem = validated_subelement(parent_elem, 'abbrev')
                    if node.get('title'):
                        new_elem.set('role', node.get('title'))  # Store expansion in role attribute
            # Inline quote - use proper DocBook <quote> element
            elif node.name == 'q':
                new_elem = validated_subelement(parent_elem, 'quote')
            # Links
            elif node.name == 'a':
                href = node.get('href')  # Don't use default - we need to detect missing attr
                # Get ID from either 'id' or legacy 'name' attribute (EPUBs often use name for anchors)
                node_id = node.get('id') or node.get('name')

                # Check if this is an anchor-only element (has ID but no meaningful href)
                # Must check BEFORE creating any element to avoid duplicates
                # Handle various cases: href missing, href='', href='#', href=None
                has_meaningful_href = href is not None and href.strip() not in ('', '#')
                is_anchor_only = node_id and not has_meaningful_href

                if is_anchor_only:
                    # Note: Anchor-only <a> elements are not converted to DocBook anchors.
                    # R2 XSL has the anchor template output commented out (html.xsl), so
                    # anchor elements produce no HTML output.
                    #
                    # Since no anchor element is created in the output, any links targeting
                    # this ID will be gracefully degraded to plain text during deferred
                    # resolution (converted from <link> to <phrase>).
                    #
                    # Mark this ID as dropped in the tracker so links to it will be
                    # properly converted to phrase elements
                    mark_id_dropped(chapter_id, node_id, "anchor-only",
                                    element_type="anchor", context="no href, R2 XSL ignores anchors")
                    #
                    # Process children (if any text content) - attach directly to parent
                    for child in node.children:
                        process_node(child, parent_elem)
                    return  # Don't continue to generic child processing
                else:
                    # Resolve and set URL (use empty string as fallback if href is None)
                    href_str = href or ''
                    resolved_href = resolve_link_href(href_str, doc_path, mapper, chapter_id)
                    final_href = resolved_href or href_str

                    # Determine if this is an internal or external link
                    is_external = final_href.startswith(('http://', 'https://', 'mailto:', 'ftp://', 'tel:', '//'))

                    # Parse resolved href to extract target chapter and anchor for internal links
                    target_chapter = None
                    target_anchor = None
                    linkend_id = None

                    # TWO-PASS PROCESSING: Store original ref for deferred resolution
                    # IMPORTANT: Use the ORIGINAL href fragment, not the resolved one
                    # resolve_link_href may add chapter prefix which breaks ID lookup
                    original_ref = None

                    # Extract the original fragment from href_str (before resolve_link_href modified it)
                    original_fragment = None
                    if href_str and '#' in href_str:
                        original_fragment = href_str.split('#', 1)[1]
                    elif href_str and href_str.startswith('#'):
                        original_fragment = href_str[1:]
                    if _is_pagebreak_ref(original_fragment) or _is_pagebreak_ref(href_str):
                        # Drop pagebreak links entirely; preserve visible text only
                        for child in node.children:
                            process_node(child, parent_elem)
                        return

                    # Skip ContactOfAuthor links (Springer internal contact navigation)
                    # Pattern: <a href="#ContactOfAuthor2"><span class="ContactIcon"> </span></a>
                    if original_fragment and original_fragment.startswith('ContactOfAuthor'):
                        # These are internal EPUB contact links - drop entirely
                        return

                    if not is_external and final_href:
                        # Internal link - extract target info
                        # Handle .xml, .xhtml, and .html extensions
                        has_extension = any(ext in final_href for ext in ['.xml', '.xhtml', '.html'])
                        if has_extension:
                            # Format: ch0001.xhtml#anchor or ch0001.xhtml
                            if '#' in final_href:
                                target_part, anchor_part = final_href.split('#', 1)
                                target_chapter = re.sub(r'\.(xml|xhtml|html)$', '', target_part)
                                target_anchor = anchor_part
                                # Store ORIGINAL ref for deferred resolution (not modified by resolve_link_href)
                                if original_fragment:
                                    original_ref = original_fragment
                                    linkend_id = '__deferred__'  # Placeholder
                            else:
                                target_chapter = re.sub(r'\.(xml|xhtml|html)$', '', final_href)
                                linkend_id = target_chapter
                        elif final_href.startswith('#'):
                            # Same-page anchor
                            target_chapter = chapter_id
                            target_anchor = final_href[1:]  # Remove leading #
                            # Store ORIGINAL ref for deferred resolution (not modified by resolve_link_href)
                            if original_fragment:
                                original_ref = original_fragment
                                linkend_id = '__deferred__'  # Placeholder
                            # If original_fragment is empty, linkend_id remains None and we'll use fallback
                        elif '#' in final_href:
                            # Other internal reference with anchor (e.g., ch0003#anchor, pr0001#anchor)
                            target_part, anchor_part = final_href.split('#', 1)
                            # Check if target_part looks like a chapter/preface/appendix ID (ch0001, pr0001, ap0001, etc.)
                            if target_part and re.match(r'^[a-z]{2}\d+$', target_part):
                                target_chapter = target_part
                                # Store ORIGINAL ref for deferred resolution
                                if original_fragment:
                                    original_ref = original_fragment
                                    linkend_id = '__deferred__'  # Placeholder
                            else:
                                # Same chapter reference - add current chapter prefix
                                target_chapter = chapter_id
                                if original_fragment:
                                    original_ref = original_fragment
                                    linkend_id = '__deferred__'  # Placeholder
                            target_anchor = anchor_part
                        elif re.match(r'^[a-z]{2}\d+$', final_href):
                            # Just a chapter/preface/appendix reference without anchor (e.g., ch0003, pr0001)
                            target_chapter = final_href
                            linkend_id = final_href

                    # SELF-REFERENCE FIX: Detect when a figure/table label in a caption links
                    # to a different chapter (e.g., List of Figures file) but should link to itself.
                    # This happens when EPUB source has cross-file hrefs for figure labels.
                    if (containing_element_id and containing_element_type and
                        target_chapter and target_chapter != chapter_id):
                        # Get link text to check for self-reference pattern
                        link_text = extract_text(node).strip()
                        # Match patterns like "Figure 3.1", "Table 2-5", "FIGURE 1.2", etc.
                        label_pattern = re.compile(
                            r'^(Figure|Table|Fig\.|Tbl\.?)\s*[\d\.\-]+$',
                            re.IGNORECASE
                        )
                        if link_text and label_pattern.match(link_text):
                            # This is a figure/table label linking to a different chapter
                            # Redirect to the current element's ID in the current chapter
                            logger.info(
                                f"Redirecting self-referencing {containing_element_type} label link: "
                                f"'{link_text}' from {target_chapter} to {chapter_id} (element ID: {containing_element_id})"
                            )
                            target_chapter = chapter_id
                            # Use the containing element's ID directly instead of deferred resolution
                            linkend_id = containing_element_id
                            original_ref = None  # Don't use deferred resolution

                    # Skip ContactOfAuthor links — these are envelope icon links in Springer
                    # metadata that contain only NBSP and should not produce visible output
                    if original_ref and 'ContactOfAuthor' in original_ref:
                        logger.debug(f"Skipping ContactOfAuthor link: href={final_href}")
                        return

                    # Determine whether the link has visible content without normalizing output text
                    has_link_text = any(
                        isinstance(d, NavigableString) and str(d).strip()
                        for d in node.descendants
                        if isinstance(d, NavigableString)
                    )

                    # Note: Link IDs are not preserved as separate anchor elements.
                    # R2 XSL has the anchor template output commented out (html.xsl), so
                    # anchor elements produce no HTML output.

                    if is_external:
                        # External link - use <ulink url="...">
                        link_elem = validated_subelement(parent_elem, 'ulink')
                        link_elem.set('url', final_href)

                        # Preserve role attribute
                        existing_role = node.get('role', '')
                        css_class = node.get('class')
                        if existing_role:
                            link_elem.set('role', existing_role)
                        elif css_class:
                            if isinstance(css_class, list):
                                css_class = ' '.join(css_class)
                            link_elem.set('role', css_class)

                        # For DOI and mailto links, use the URL as display text
                        # to avoid whitespace normalization breaking the text
                        # (e.g., "https://doi. org/..." or "user@exam ple.com")
                        if 'doi.org' in final_href:
                            link_elem.text = final_href
                        elif final_href.startswith('mailto:'):
                            link_elem.text = final_href.replace('mailto:', '')
                        else:
                            # Process children into the ulink
                            for child in node.children:
                                process_node(child, link_elem)
                    elif linkend_id:
                        # Check if this is a citation (reference to bibliography entry)
                        # Per spec: citations should use <citation> element for bibliography refs
                        # Use robust helper that handles BeautifulSoup namespace variations
                        epub_type = _get_epub_type(node)
                        css_class = node.get('class', '')
                        if isinstance(css_class, list):
                            css_class = ' '.join(css_class)
                        css_class_lower = css_class.lower() if css_class else ''

                        # Also check for citation-style ID patterns in the href
                        # CR#, Ref#, bib# are common citation ID patterns
                        is_citation_href = bool(original_ref and re.match(
                            r'^(CR|Ref|ref|bib|Bib)\d+$', original_ref, re.IGNORECASE))

                        # Detect bibliography reference patterns
                        # Note: We only use citation for explicit bibliography refs (epub:type or class)
                        # Regular links with href="#ref*" should use <link> not <citation>
                        is_citation = (
                            'biblioref' in epub_type.lower() or
                            'citation' in epub_type.lower() or
                            'biblioref' in css_class_lower or
                            'citation' in css_class_lower or
                            is_citation_href  # Also detect by href pattern (CR1, bib1, etc.)
                            # Removed overly broad pattern matching that incorrectly caught
                            # table/figure refs like #ref61_1 as citations
                        )

                        if is_citation:
                            # Create link element for bibliography references
                            # <link linkend="ref001">text</link> references <bibliomixed id="ref001">
                            link_elem = validated_subelement(parent_elem, 'link')
                            link_elem.set('linkend', linkend_id)
                            # For deferred resolution, mark the link element
                            if original_ref and linkend_id == '__deferred__':
                                mark_link_for_deferred_resolution(link_elem, original_ref, target_chapter or chapter_id, chapter_id)
                            # Process children into the link
                            for child in node.children:
                                process_node(child, link_elem)
                        elif target_chapter and target_chapter != chapter_id:
                            # Cross-document internal link - use <link linkend="...">
                            # All same-book links use link/linkend; ulink is only for external URLs
                            link_elem = validated_subelement(parent_elem, 'link')
                            # Set placeholder linkend for deferred resolution
                            link_elem.set('linkend', '__deferred__')
                            if original_ref:
                                mark_link_for_deferred_resolution(link_elem, original_ref, target_chapter, chapter_id)
                            else:
                                # No anchor, just chapter reference - use chapter ID as linkend
                                link_elem.set('linkend', target_chapter)

                            if has_link_text:
                                # Process children into the link
                                for child in node.children:
                                    process_node(child, link_elem)
                            else:
                                # If no link text, use the target reference as text
                                link_elem.text = original_ref or target_chapter
                        elif linkend_id and not has_link_text:
                            # Same document, no link text, valid linkend - use <xref linkend="..."/> (EMPTY element)
                            xref_elem = validated_subelement(parent_elem, 'xref')
                            xref_elem.set('linkend', linkend_id)
                            if original_ref and linkend_id == '__deferred__':
                                mark_link_for_deferred_resolution(xref_elem, original_ref, target_chapter or chapter_id, chapter_id)
                        elif linkend_id:
                            # Same document, has link text, valid linkend - use <link linkend="...">text</link>
                            link_elem = validated_subelement(parent_elem, 'link')
                            link_elem.set('linkend', linkend_id)
                            if original_ref and linkend_id == '__deferred__':
                                mark_link_for_deferred_resolution(link_elem, original_ref, target_chapter or chapter_id, chapter_id)

                            # Process children into the link
                            for child in node.children:
                                process_node(child, link_elem)
                        elif has_link_text:
                            # No valid linkend but has link text - use <phrase> to preserve text
                            # This prevents creating invalid <link> elements without linkend
                            # BUT: <phrase> is NOT allowed in restrictive elements like superscript, subscript
                            restrictive_parents = {'superscript', 'subscript', 'code', 'literal',
                                                   'constant', 'varname', 'function', 'parameter'}
                            parent_tag = parent_elem.tag if hasattr(parent_elem, 'tag') else ''

                            if parent_tag in restrictive_parents:
                                # Process children directly into parent (no phrase wrapper)
                                for child in node.children:
                                    process_node(child, parent_elem)
                                logger.debug(f"Processed link content directly into restrictive parent <{parent_tag}>: href={final_href}")
                            else:
                                phrase_elem = validated_subelement(parent_elem, 'phrase')
                                phrase_elem.set('role', 'unresolved-link')
                                for child in node.children:
                                    process_node(child, phrase_elem)
                                logger.debug(f"Created <phrase> for link without valid linkend: href={final_href}")
                        else:
                            # No valid linkend and no link text - skip silently
                            logger.debug(f"Skipping link without valid linkend or text: href={final_href}")
                    else:
                        # Fallback for links we couldn't categorize
                        # Since is_external=False here, this is an internal link we couldn't resolve
                        # Preserve the text content in a phrase element rather than creating invalid ulink
                        if has_link_text:
                            phrase_elem = validated_subelement(parent_elem, 'phrase')
                            phrase_elem.set('role', 'unresolved-link')
                            for child in node.children:
                                process_node(child, phrase_elem)
                            logger.debug(f"Created <phrase> for unresolved internal link: href={final_href}")
                        else:
                            logger.debug(f"Skipping unresolved internal link without text: href={final_href}")

                    # Register link in mapper
                    mapper.add_link(href_str, chapter_id, target_chapter, target_anchor)

                    return  # Don't continue to generic child processing
            # Span with styling
            elif node.name == 'span':
                style = node.get('style', '')
                css_class = node.get('class', '')
                epub_type = _get_epub_type(node)
                role_attr = node.get('role', '')

                # Check if this is a page break marker
                is_pagebreak = False
                if epub_type and 'pagebreak' in epub_type.lower():
                    is_pagebreak = True
                elif role_attr and 'pagebreak' in role_attr.lower():
                    # Check role attribute (e.g., role="doc-pagebreak")
                    is_pagebreak = True
                elif css_class:
                    # Check for common page break classes
                    class_str = ' '.join(css_class) if isinstance(css_class, list) else css_class
                    class_lower = class_str.lower()
                    if any(pb in class_lower for pb in ['pagebreak', 'page-break', 'page_break', 'pagenum', 'page-num', 'page_num']):
                        is_pagebreak = True

                # Page breaks are discarded - R2 XSL ignores both beginpage and anchor elements
                # (the anchor template output is commented out in html.xsl)
                # Note: Any "See page X" links in the source will be broken in web output
                # This is a known limitation for print-to-web conversion - see documentation
                if is_pagebreak:
                    return

                # Skip ContactIcon spans (Springer internal contact navigation icon)
                if css_class:
                    class_str_ci = ' '.join(css_class) if isinstance(css_class, list) else css_class
                    if 'ContactIcon' in class_str_ci:
                        return

                # Check if this is a line break marker (CSS-based line break using display:block)
                # Common patterns: <span class="block"/>, <span class="linebreak"/>, <span class="lb"/>
                if css_class:
                    class_str_check = ' '.join(css_class) if isinstance(css_class, list) else css_class
                    class_list_check = class_str_check.lower().split() if class_str_check else []
                    linebreak_classes = {'block', 'linebreak', 'line-break', 'line_break', 'lb', 'br', 'break'}
                    if any(c in linebreak_classes for c in class_list_check):
                        # This is a CSS-based line break - convert to <?lb?> processing instruction
                        lb_pi = etree.ProcessingInstruction('lb')
                        lb_pi.tail = " "
                        validated_append(parent_elem, lb_pi)
                        return

                # Check if this is an inline equation (common in Springer EPUBs)
                # Pattern: <span class="InlineEquation"><math altimg="path/to/equation.svg">...</math></span>
                if css_class:
                    class_str_eq = ' '.join(css_class) if isinstance(css_class, list) else css_class
                    class_list_eq = class_str_eq.lower().split() if class_str_eq else []
                    if 'inlineequation' in class_list_eq:
                        # Find the math element to get the altimg (SVG image path)
                        math_elem = node.find('math')
                        if math_elem:
                            altimg = math_elem.get('altimg', '')
                            alttext = math_elem.get('alttext', '')

                            # Check if we're inside a title element - DTD doesn't allow
                            # block elements like inlinemediaobject inside title
                            parent_tag = parent_elem.tag if hasattr(parent_elem, 'tag') else ''
                            is_in_title = parent_tag == 'title'

                            if is_in_title:
                                # Inside title: use alt text only (no images allowed)
                                if alttext and alttext.strip():
                                    if len(parent_elem) == 0:
                                        parent_elem.text = (parent_elem.text or '') + alttext
                                    else:
                                        parent_elem[-1].tail = (parent_elem[-1].tail or '') + alttext
                                else:
                                    # Fallback to math text content
                                    math_text = extract_text(node)
                                    if math_text and math_text.strip():
                                        if len(parent_elem) == 0:
                                            parent_elem.text = (parent_elem.text or '') + math_text
                                        else:
                                            parent_elem[-1].tail = (parent_elem[-1].tail or '') + math_text
                                return

                            if altimg:
                                # Create inlinemediaobject with the equation image
                                # Resolve the image path using the existing infrastructure
                                result = resolve_image_path(altimg, doc_path, mapper)
                                if result:
                                    intermediate_name, normalized_path = result
                                    inlinemedia = validated_subelement(parent_elem, 'inlinemediaobject')
                                    imageobject = validated_subelement(inlinemedia, 'imageobject')
                                    imagedata = validated_subelement(imageobject, 'imagedata')
                                    imagedata.set('fileref', intermediate_name)
                                    mapper.add_reference(normalized_path, chapter_id)

                                    # Add alt text from math alttext attribute if available
                                    if alttext:
                                        textobject = validated_subelement(inlinemedia, 'textobject')
                                        phrase = validated_subelement(textobject, 'phrase')
                                        phrase.text = alttext
                                    return
                        # If no altimg found, use DTD-compliant text fallback
                        # DTD requires: inlineequation → (alt?, (graphic+ | inlinemediaobject+))
                        math_text = extract_text(node)
                        if math_text and math_text.strip():
                            inlineeq = validated_subelement(parent_elem, 'inlineequation')
                            alt_elem = validated_subelement(inlineeq, 'alt')
                            alt_elem.text = math_text.strip()
                            inlinemedia = validated_subelement(inlineeq, 'inlinemediaobject')
                            textobj = validated_subelement(inlinemedia, 'textobject')
                            phrase_elem = validated_subelement(textobj, 'phrase')
                            phrase_elem.text = math_text.strip()
                        return

                # Check if this is an index term marker (common in Springer EPUBs)
                # Pattern: <span id="ITerm3">term</span> or <span id="ITerm2"/> (empty)
                # These are invisible index markers - include any text content but don't create
                # special elements (indexterm would require complex handling)
                node_id = node.get('id', '')
                if node_id and (node_id.startswith('ITerm') or node_id.startswith('iterm')):
                    # Just include any text content from the index term span
                    # Use process_node for children to preserve whitespace (extract_text strips it)
                    # This prevents "without " + "defining" → "withoutdefining" (C-011)
                    for child in node.children:
                        process_node(child, parent_elem)
                    # Skip empty index markers (they're just invisible anchors)
                    return

                # Not a page break - handle as regular span with formatting
                # Extract font formatting from style attribute
                style_attrs = parse_style_attribute(style)

                # Normalize css_class to a string for checking
                class_str = ' '.join(css_class) if isinstance(css_class, list) else (css_class or '')
                class_lower = class_str.lower()
                # Split into individual classes for exact matching
                class_list = class_str.split() if class_str else []
                class_list_lower = [c.lower() for c in class_list]

                # Handle HeadingNumber span: <span class="HeadingNumber">1 </span>Introduction
                # Preserve trailing space so output is "1 Introduction" not "1Introduction"
                if 'HeadingNumber' in class_list or 'headingnumber' in class_list_lower:
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='HeadingNumber')
                    for child in node.children:
                        process_node(child, new_elem)
                    # Ensure trailing space after the heading number
                    # The source HTML has trailing space inside the span which gets normalized away
                    if new_elem.tail is None or (new_elem.tail and not new_elem.tail.startswith(' ')):
                        new_elem.tail = ' ' + (new_elem.tail or '')
                    return

                # Check for semantic CSS classes commonly used in EPUBs
                # These take precedence over generic styling
                is_bold = any(c in class_list_lower for c in ['b', 'bold', 'strong'])
                is_italic = any(c in class_list_lower for c in ['i', 'italic', 'em', 'ital'])
                is_bold_italic = any(c in class_list_lower for c in ['bi', 'bold-italic', 'bolditalic', 'ib'])
                is_smallcaps = any(c in class_list_lower for c in ['sc', 'smallcaps', 'small-caps', 'smallcap', 'scap'])
                is_underline = any(c in class_list_lower for c in ['u', 'underline', 'uline'])

                # Handle semantic CSS classes for inline formatting
                if is_bold_italic:
                    # Bold-italic: nested emphasis elements
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='strong')
                    inner_elem = validated_subelement(new_elem, 'emphasis')
                    # Process children into inner element
                    for child in node.children:
                        process_node(child, inner_elem)
                    return
                elif is_bold:
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='strong')
                elif is_italic:
                    new_elem = validated_subelement(parent_elem, 'emphasis')
                elif is_smallcaps:
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='smallcaps')
                elif is_underline:
                    new_elem = validated_subelement(parent_elem, 'emphasis', role='underline')

                # Check for superscript/subscript - from class names OR CSS vertical-align style
                # Use word boundary matching to avoid false positives (e.g., "chsubtitle" should NOT match "sub")
                elif _has_class_word(css_class, ['sup', 'superscript', 'super']):
                    new_elem = validated_subelement(parent_elem, 'superscript')
                elif _has_class_word(css_class, ['sub', 'subscript']) and not _has_class_word(css_class, ['subtitle', 'chsubtitle']):
                    new_elem = validated_subelement(parent_elem, 'subscript')

                # Check for subtitle classes - these should become <subtitle> if valid position, else <emphasis>
                elif _has_class_word(css_class, ['subtitle', 'chsubtitle', 'chapter-subtitle', 'sect-subtitle']):
                    # Check if parent allows subtitle (chapter, appendix, preface, sect1-5, etc.)
                    parent_tag = parent_elem.tag if hasattr(parent_elem, 'tag') else ''
                    subtitle_parents = {'chapter', 'appendix', 'preface', 'part', 'partintro',
                                       'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'simplesect',
                                       'article', 'glossary', 'bibliography', 'index', 'dedication'}
                    if parent_tag in subtitle_parents:
                        # Check if there's already a title and no subtitle yet
                        has_title = any(c.tag == 'title' for c in parent_elem if hasattr(c, 'tag'))
                        has_subtitle = any(c.tag == 'subtitle' for c in parent_elem if hasattr(c, 'tag'))
                        if has_title and not has_subtitle:
                            new_elem = validated_subelement(parent_elem, 'subtitle')
                        else:
                            # Already has subtitle or no title - use emphasis
                            new_elem = validated_subelement(parent_elem, 'emphasis', role='subtitle')
                    else:
                        # Not a valid subtitle parent - use emphasis with role
                        new_elem = validated_subelement(parent_elem, 'emphasis', role='subtitle')
                else:
                    # Check vertical-align style for super/subscript
                    vertical_align = style_attrs.get('vertical-align', '').lower()
                    is_superscript = vertical_align in ['super', 'superscript', 'text-top']
                    is_subscript = vertical_align in ['sub', 'subscript', 'text-bottom']

                    if is_superscript:
                        new_elem = validated_subelement(parent_elem, 'superscript')
                    elif is_subscript:
                        new_elem = validated_subelement(parent_elem, 'subscript')
                    elif style_attrs or css_class:
                        # Check for font-weight/font-style in CSS styles first
                        # so <span style="font-weight:bold"> maps to emphasis role="strong"
                        # (XSL inline.xsl checks role="strong" for bold rendering)
                        font_weight = style_attrs.get('font-weight', '').lower() if style_attrs else ''
                        font_style_css = style_attrs.get('font-style', '').lower() if style_attrs else ''

                        is_style_bold = font_weight in ('bold', '700', '800', '900')
                        is_style_italic = font_style_css in ('italic', 'oblique')

                        if is_style_bold and is_style_italic:
                            # Bold-italic: nested emphasis elements
                            new_elem = validated_subelement(parent_elem, 'emphasis', role='strong')
                            inner_elem = validated_subelement(new_elem, 'emphasis')
                            for child in node.children:
                                process_node(child, inner_elem)
                            return
                        elif is_style_bold:
                            new_elem = validated_subelement(parent_elem, 'emphasis', role='strong')
                        elif is_style_italic:
                            new_elem = validated_subelement(parent_elem, 'emphasis')
                        else:
                            # Other styles — generic emphasis with style info in role
                            new_elem = validated_subelement(parent_elem, 'emphasis')

                            # Build role attribute with styling info
                            role_parts = []
                            if css_class:
                                if isinstance(css_class, list):
                                    role_parts.extend(css_class)
                                else:
                                    role_parts.append(css_class)

                            # Add CSS properties to role
                            for prop, value in style_attrs.items():
                                role_parts.append(f"{prop}:{value}")

                            if role_parts:
                                new_elem.set('role', '; '.join(role_parts))
                    else:
                        # Note: Span IDs are not preserved as anchor elements.
                        # R2 XSL has the anchor template output commented out (html.xsl), so
                        # anchor elements produce no HTML output.

                        # Track how many children parent had before processing
                        children_before = len(parent_elem)

                        # Process children directly
                        for child in node.children:
                            process_node(child, parent_elem)

                        return
            # Inline images - wrap in figure with mediaobject inside (unless in table cell)
            elif node.name == 'img':
                img_src = node.get('src', '')
                if img_src:
                    result = resolve_image_path(img_src, doc_path, mapper)
                    if result:
                        intermediate_name, normalized_path = result

                        # Check if we're inside a table cell (entry) - figure not allowed there
                        # In that case, use mediaobject directly without figure wrapper
                        # Also check for invalid inline containers like emphasis
                        in_table_cell = False
                        entry_elem = None
                        in_invalid_parent = False
                        invalid_inline_tags = {'emphasis', 'superscript', 'subscript', 'literal', 'link', 'ulink'}
                        check_elem = parent_elem
                        while check_elem is not None:
                            if check_elem.tag == 'entry':
                                in_table_cell = True
                                entry_elem = check_elem
                                break
                            if check_elem.tag in invalid_inline_tags:
                                in_invalid_parent = True
                            check_elem = check_elem.getparent() if hasattr(check_elem, 'getparent') else None

                        if in_table_cell:
                            # Inside table cell - use mediaobject directly (no figure wrapper)
                            # If we're also inside an invalid parent (like emphasis), add to entry instead
                            target = entry_elem if (in_invalid_parent and entry_elem is not None) else parent_elem
                            mediaobject = validated_subelement(target, 'mediaobject')
                            imageobject = validated_subelement(mediaobject, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)
                            mapper.add_reference(normalized_path, chapter_id)
                            logger.debug(f"Created mediaobject (no figure) for table cell: {img_src}")
                        else:
                            # Normal case - create figure with mediaobject inside
                            target_parent = figure_parent if figure_parent is not None else parent_elem

                            # Create figure wrapper
                            figure_elem = validated_subelement(target_parent, 'figure')

                            # Generate figure ID
                            if figure_counter is not None:
                                figure_counter['count'] += 1
                            else:
                                # Fallback: use global counter to ensure unique IDs
                                _global_autofig_counter['count'] += 1
                            # Use current_sect1 from enclosing scope (set on line 7055)
                            figure_elem.set('id', generate_element_id(chapter_id, 'figure', current_sect1))

                            # Add empty title (DTD requires title element)
                            title_elem = validated_subelement(figure_elem, 'title')
                            # Use alt text as title if available
                            alt_text = node.get('alt', '')
                            if alt_text:
                                title_elem.text = alt_text

                            # Create mediaobject structure inside figure
                            mediaobject = validated_subelement(figure_elem, 'mediaobject')
                            imageobject = validated_subelement(mediaobject, 'imageobject')
                            imagedata = validated_subelement(imageobject, 'imagedata')
                            imagedata.set('fileref', intermediate_name)

                            # Register the reference
                            mapper.add_reference(normalized_path, chapter_id)
                            logger.debug(f"Created figure with mediaobject for: {img_src} → {normalized_path} in {chapter_id}")
                    else:
                        logger.warning(f"Could not resolve inline image: {img_src}")
                return
            # Line break - convert to <?lb?> processing instruction per RittDoc convention
            elif node.name == 'br':
                # Create <?lb?> processing instruction for line breaks
                # This is the RittDoc standard for preserving line breaks in output
                lb_pi = etree.ProcessingInstruction('lb')
                lb_pi.tail = " "
                validated_append(parent_elem, lb_pi)
                return
            # Block-level elements should NOT be processed in inline context
            # Skip them to prevent invalid nesting like para inside para
            elif node.name in ['p', 'div', 'section', 'article', 'aside', 'header', 'footer',
                              'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
                              'dl', 'dt', 'dd',  # NOTE: ul/ol removed - now handled by parent
                              'table', 'thead', 'tbody', 'tfoot', 'tr', 'td', 'th',
                              'blockquote', 'pre', 'hr', 'figure', 'figcaption',
                              'nav', 'main', 'form', 'fieldset']:
                # Block element in inline context - unwrap it but preserve inline content
                logger.warning(f"Unwrapping block element <{node.name}> in inline context at {chapter_id}")

                # Note: Block element IDs are not preserved as anchor elements.
                # R2 XSL has the anchor template output commented out (html.xsl), so
                # anchor elements produce no HTML output.

                # Insert a separator if inline content already exists.
                if len(parent_elem) == 0:
                    if parent_elem.text and not parent_elem.text.endswith((' ', '\n', '\t')):
                        parent_elem.text += ' '
                else:
                    last_child = parent_elem[-1]
                    tail = last_child.tail or ''
                    if tail:
                        if not tail.endswith((' ', '\n', '\t')):
                            last_child.tail = tail + ' '
                    else:
                        last_text = last_child.text or ''
                        if last_text and not last_text.endswith((' ', '\n', '\t')):
                            last_child.tail = ' '

                # Instead of extracting just text, recursively process children
                # This preserves links, formatting, and other inline elements
                for child in node.children:
                    process_node(child, parent_elem)
                return
            # Nested lists (ul/ol) and list items (li) in inline context
            # These are now handled by the parent list processing, so skip them entirely
            # Don't extract their text - that will be done when processing them as nested lists
            elif node.name in ['ul', 'ol', 'li']:
                # Skip completely - will be processed by parent list handler
                logger.debug(f"Skipping nested list element <{node.name}> in inline context - will be processed separately")
                return
            # DocBook inline elements - preserve them with their attributes
            # These may appear in XHTML when source uses DocBook-style markup
            elif node.name == 'phrase':
                # Avoid emitting <phrase>; represent as <emphasis role="..."> instead
                new_elem = validated_subelement(parent_elem, 'emphasis')
                existing_role = node.get('role', '')
                if existing_role:
                    new_elem.set('role', existing_role)
            elif node.name == 'emphasis':
                # Preserve <emphasis> element with role attribute
                new_elem = validated_subelement(parent_elem, 'emphasis')
                existing_role = node.get('role', '')
                if existing_role:
                    new_elem.set('role', existing_role)
            elif node.name == 'anchor':
                # Note: Source <anchor> elements are not preserved.
                # R2 XSL has the anchor template output commented out (html.xsl), so
                # anchor elements produce no HTML output.
                return
            # MathML <math> element in inline context — convert to <inlineequation>
            elif node.name == 'math':
                inlineeq_elem = validated_subelement(parent_elem, 'inlineequation')
                alt_text = node.get('alttext', '') or node.get('alt', '')
                altimg = node.get('altimg', '')
                if not alt_text:
                    alt_text = extract_text(node).strip()

                img_resolved = False
                if altimg:
                    result = resolve_image_path(altimg, doc_path, mapper)
                    if result:
                        intermediate_name, normalized_path = result
                        if alt_text:
                            alt_elem = validated_subelement(inlineeq_elem, 'alt')
                            alt_elem.text = alt_text
                        inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                        imageobject = validated_subelement(inlinemedia, 'imageobject')
                        imagedata = validated_subelement(imageobject, 'imagedata')
                        imagedata.set('fileref', intermediate_name)
                        mapper.add_reference(normalized_path, chapter_id)
                        if alt_text:
                            textobj = validated_subelement(inlinemedia, 'textobject')
                            phrase = validated_subelement(textobj, 'phrase')
                            phrase.text = alt_text
                        img_resolved = True

                # Check for <img> sibling
                if not img_resolved and node.parent:
                    img_elem = node.parent.find('img')
                    if img_elem:
                        img_src = img_elem.get('src', '')
                        if img_src:
                            result = resolve_image_path(img_src, doc_path, mapper)
                            if result:
                                intermediate_name, normalized_path = result
                                inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                                imageobject = validated_subelement(inlinemedia, 'imageobject')
                                imagedata = validated_subelement(imageobject, 'imagedata')
                                imagedata.set('fileref', intermediate_name)
                                mapper.add_reference(normalized_path, chapter_id)
                                img_resolved = True

                # Text fallback — DTD requires inlinemediaobject inside inlineequation
                if not img_resolved and alt_text:
                    alt_elem = validated_subelement(inlineeq_elem, 'alt')
                    alt_elem.text = alt_text
                    inlinemedia = validated_subelement(inlineeq_elem, 'inlinemediaobject')
                    textobj = validated_subelement(inlinemedia, 'textobject')
                    phrase = validated_subelement(textobj, 'phrase')
                    phrase.text = alt_text
                return
            # Other inline containers - try XHTML mapping first, then process children
            else:
                # Try XHTML mapping for this element
                css_class = node.get('class', '')
                if isinstance(css_class, list):
                    css_class = ' '.join(css_class)

                docbook_tag = get_docbook_element_for_html(node.name, css_class, parent_elem.tag)

                # If we got a meaningful mapping (not the same tag or generic fallbacks)
                if docbook_tag and docbook_tag != node.name and docbook_tag not in ('phrase', 'remark'):
                    new_elem = validated_subelement(parent_elem, docbook_tag)
                    if css_class:
                        new_elem.set('role', css_class)
                    for child in node.children:
                        process_node(child, new_elem)
                    # Ensure spacing after block-level elements (C-002)
                    if node.name in _BLOCK_ELEMENTS:
                        _ensure_trailing_space(parent_elem)
                    return
                else:
                    # No mapping - just process children
                    for child in node.children:
                        process_node(child, parent_elem)
                    # Ensure spacing after block-level elements processed inline (C-002)
                    # Example: <div>...2025</div><span>D. Paul</span> → "2025 D. Paul"
                    if node.name in _BLOCK_ELEMENTS:
                        _ensure_trailing_space(parent_elem)
                    return

            # Process children of the new element
            if new_elem is not None:
                # Preserve ID attribute for cross-referencing (prefix with chapter_id to avoid conflicts)
                node_id = node.get('id')
                if node_id:
                    new_elem.set('id', sanitize_anchor_id(node_id, chapter_id, 'anchor', current_sect1))

                for child in node.children:
                    process_node(child, new_elem)

                # Do not trim or normalize whitespace inside inline containers.

    # If the caller passed an <a> element directly (e.g., buffered inline in list items),
    # process the node itself so link markup and whitespace are preserved.
    if elem.name == 'a' and not include_root:
        process_node(elem, para)
        return

    # Process element(s) based on include_root flag
    if include_root:
        # Process elem itself as if it were a child node
        # This handles cases where elem is a formatting element (like <b>) that was
        # buffered and needs to be wrapped in the appropriate DocBook element
        process_node(elem, para)
    else:
        # Normal behavior: process all children of the element
        for child in elem.children:
            process_node(child, para)


def _has_class_word(css_class: Union[str, List[str], None], target_words: List[str]) -> bool:
    """
    Check if any of the target words appear as complete class names (not substrings).

    This prevents false positives like matching "sub" in "chsubtitle".

    Args:
        css_class: CSS class string, list of classes, or None
        target_words: List of class names to match

    Returns:
        True if any target word matches a complete class name
    """
    if not css_class:
        return False

    # Normalize to list of individual class names
    if isinstance(css_class, str):
        classes = css_class.lower().split()
    else:
        classes = [c.lower() for c in css_class]

    # Check for exact class matches
    target_set = set(w.lower() for w in target_words)
    return bool(target_set.intersection(classes))


def parse_style_attribute(style_str: str) -> Dict[str, str]:
    """
    Parse CSS style attribute into a dictionary.

    Args:
        style_str: CSS style string (e.g., "font-family: Arial; font-size: 14pt; color: red")

    Returns:
        Dictionary of CSS properties and values
    """
    styles = {}
    if not style_str:
        return styles

    # Split by semicolon and parse each property
    for prop in style_str.split(';'):
        prop = prop.strip()
        if ':' in prop:
            key, value = prop.split(':', 1)
            key = key.strip().lower()
            value = value.strip()

            # Only preserve font-related and color properties
            if key in ['font-family', 'font-size', 'font-weight', 'font-style',
                      'color', 'background-color', 'text-decoration', 'text-align']:
                styles[key] = value

    return styles


def resolve_link_href(href: str, doc_path: str, mapper: ReferenceMapper,
                      source_chapter: str) -> Optional[str]:
    """
    Resolve a link href to its final form.

    Args:
        href: Original href from HTML
        doc_path: Current document path
        mapper: Reference mapper
        source_chapter: Current chapter ID

    Returns:
        Resolved href or None if it should stay as-is
    """
    # External links - keep as-is
    if href.startswith(('http://', 'https://', 'mailto:', 'ftp://', 'tel:', '//')):
        return href

    # Empty or just fragment (same page anchor)
    if not href or href.startswith('#'):
        # Prefix fragment with chapter_id to match prefixed IDs
        if href and href.startswith('#'):
            fragment_id = href[1:]  # Remove leading #
            if fragment_id:
                # Check if already prefixed to avoid double-prefixing
                if not fragment_id.startswith(f"{source_chapter}-"):
                    return f"#{source_chapter}-{fragment_id}"
                return href  # Already prefixed
        return href if href else '#'

    # Split into path and fragment
    if '#' in href:
        link_path, fragment = href.split('#', 1)
        fragment = '#' + fragment
    else:
        link_path = href
        fragment = ''

    # Resolve relative path
    if link_path.startswith('../') or link_path.startswith('./'):
        doc_dir = str(Path(doc_path).parent)
        if doc_dir == '.':
            resolved_path = link_path
        else:
            resolved_path = str(Path(doc_dir) / link_path)
        # Normalize path
        resolved_path = str(Path(resolved_path).as_posix())
    else:
        resolved_path = link_path

    # Try to resolve to a chapter ID
    # Try exact match
    target_chapter = mapper.get_chapter_id(resolved_path)

    if not target_chapter:
        # Try basename
        basename = Path(link_path).name
        target_chapter = mapper.get_chapter_id(basename)

    if not target_chapter:
        # Try variations
        variations = [
            link_path,
            f"OEBPS/{link_path}",
            f"OPS/{link_path}",
            f"Text/{link_path}",
            link_path.lstrip('./'),
            link_path.lstrip('../'),
        ]
        for variant in variations:
            target_chapter = mapper.get_chapter_id(variant)
            if target_chapter:
                break

    if target_chapter:
        # Resolved to internal chapter
        # Prefix the fragment ID with target chapter to match prefixed IDs
        if fragment and fragment.startswith('#'):
            fragment_id = fragment[1:]  # Remove leading #
            if fragment_id:
                # Check if already prefixed to avoid double-prefixing
                if not fragment_id.startswith(f"{target_chapter}-"):
                    fragment = f"#{target_chapter}-{fragment_id}"
        # Don't add .xml extension to internal references
        return f"{target_chapter}{fragment}"

    # Could not resolve - return as-is (might be external resource)
    return href


# =============================================================================
# IMPROVED TABLE SPAN HANDLING
# =============================================================================

class TableSpanTracker:
    """
    Tracks cell occupancy in tables to correctly handle rowspans and colspans.

    When a cell has rowspan > 1, it occupies cells in subsequent rows.
    This tracker maintains a grid of occupied cells so we can correctly
    determine the column index for each cell in each row.
    """

    def __init__(self, num_cols: int, num_rows: int):
        """
        Initialize the span tracker.

        Args:
            num_cols: Number of columns in the table
            num_rows: Number of rows in the table
        """
        self.num_cols = num_cols
        self.num_rows = num_rows
        # Grid: occupied[row][col] = True if cell is occupied by a span from above
        self.occupied = [[False] * (num_cols + 1) for _ in range(num_rows + 1)]
        # Track cells for consistency validation
        self._cell_placements = []  # List of (row, col, rowspan, colspan)

    def mark_span(self, row: int, col: int, rowspan: int, colspan: int) -> None:
        """
        Mark cells as occupied by a spanning cell.

        Args:
            row: Starting row (0-based)
            col: Starting column (1-based)
            rowspan: Number of rows the cell spans
            colspan: Number of columns the cell spans
        """
        # Track cell placement for validation
        self._cell_placements.append((row, col, rowspan, colspan))

        for r in range(row, min(row + rowspan, self.num_rows)):
            for c in range(col, min(col + colspan, self.num_cols + 1)):
                if r > row or c > col:  # Don't mark the cell itself
                    self.occupied[r][c] = True

    def get_next_free_column(self, row: int, current_col: int) -> int:
        """
        Get the next free column in a row, skipping occupied cells.

        Args:
            row: Row index (0-based)
            current_col: Current column position (1-based)

        Returns:
            Next free column index (1-based)
        """
        col = current_col
        while col <= self.num_cols and self.occupied[row][col]:
            col += 1
        return col

    def is_occupied(self, row: int, col: int) -> bool:
        """Check if a cell is occupied by a span from above."""
        if row < 0 or row >= self.num_rows or col < 1 or col > self.num_cols:
            return False
        return self.occupied[row][col]

    def get_occupancy_hash(self) -> str:
        """
        Get a hash of the occupancy grid for comparison.

        Returns:
            String hash representing the occupancy state
        """
        import hashlib
        state = []
        for row in range(self.num_rows):
            for col in range(1, self.num_cols + 1):
                if self.occupied[row][col]:
                    state.append(f"{row},{col}")
        state_str = "|".join(state)
        return hashlib.md5(state_str.encode()).hexdigest()[:16]

    def get_occupied_count(self) -> int:
        """Return the total number of occupied cells."""
        count = 0
        for row in range(self.num_rows):
            for col in range(1, self.num_cols + 1):
                if self.occupied[row][col]:
                    count += 1
        return count

    def validate_consistency_with(self, other: 'TableSpanTracker') -> tuple:
        """
        Validate that this tracker is consistent with another.

        Args:
            other: Another TableSpanTracker to compare with

        Returns:
            Tuple of (is_consistent, list_of_differences)
        """
        differences = []

        # Check dimensions
        if self.num_cols != other.num_cols:
            differences.append(f"Column count mismatch: {self.num_cols} vs {other.num_cols}")

        if self.num_rows != other.num_rows:
            differences.append(f"Row count mismatch: {self.num_rows} vs {other.num_rows}")

        # Check occupancy grid
        min_rows = min(self.num_rows, other.num_rows)
        min_cols = min(self.num_cols, other.num_cols)

        for row in range(min_rows):
            for col in range(1, min_cols + 1):
                self_occupied = self.occupied[row][col] if row < len(self.occupied) and col < len(self.occupied[row]) else False
                other_occupied = other.occupied[row][col] if row < len(other.occupied) and col < len(other.occupied[row]) else False

                if self_occupied != other_occupied:
                    differences.append(
                        f"Occupancy mismatch at row {row}, col {col}: "
                        f"{'occupied' if self_occupied else 'free'} vs "
                        f"{'occupied' if other_occupied else 'free'}"
                    )

        is_consistent = len(differences) == 0
        return is_consistent, differences


def precompute_table_structure(table_elem: Tag) -> tuple:
    """
    Pre-compute table structure including span tracking.

    Analyzes the table to determine:
    1. Actual number of columns (accounting for spans)
    2. Number of rows
    3. Cell occupancy grid for rowspan handling

    Args:
        table_elem: BeautifulSoup table element

    Returns:
        Tuple of (num_cols, num_rows, span_tracker)
    """
    if table_elem is None:
        return (1, 0, None)

    rows = _iter_table_rows(table_elem)
    num_rows = len(rows)

    if num_rows == 0:
        return (1, 0, None)

    # First pass: determine max columns
    # We need to account for both colspan and cells occupied by rowspans
    max_cols = count_table_columns(table_elem)

    # Second pass: build occupancy grid
    tracker = TableSpanTracker(max_cols, num_rows)

    for row_idx, tr in enumerate(rows):
        col_idx = 1
        for cell in tr.find_all(['td', 'th'], recursive=False):
            # Skip to next free column
            col_idx = tracker.get_next_free_column(row_idx, col_idx)

            if col_idx > max_cols:
                # Need more columns than initially counted
                max_cols = col_idx
                # Expand the tracker
                for r in range(num_rows):
                    while len(tracker.occupied[r]) <= max_cols:
                        tracker.occupied[r].append(False)
                tracker.num_cols = max_cols

            # Get span attributes
            try:
                colspan = int(cell.get('colspan', 1))
            except (ValueError, TypeError):
                colspan = 1

            try:
                rowspan = int(cell.get('rowspan', 1))
            except (ValueError, TypeError):
                rowspan = 1

            # Mark spanned cells as occupied
            if rowspan > 1 or colspan > 1:
                tracker.mark_span(row_idx, col_idx, rowspan, colspan)

            col_idx += colspan

        # Update max_cols if this row ended with more columns
        if col_idx - 1 > max_cols:
            max_cols = col_idx - 1

    # Rebuild tracker with correct column count
    final_tracker = TableSpanTracker(max_cols, num_rows)

    # Re-run to populate with correct dimensions
    for row_idx, tr in enumerate(rows):
        col_idx = 1
        for cell in tr.find_all(['td', 'th'], recursive=False):
            col_idx = final_tracker.get_next_free_column(row_idx, col_idx)

            try:
                colspan = int(cell.get('colspan', 1))
            except (ValueError, TypeError):
                colspan = 1

            try:
                rowspan = int(cell.get('rowspan', 1))
            except (ValueError, TypeError):
                rowspan = 1

            if rowspan > 1 or colspan > 1:
                final_tracker.mark_span(row_idx, col_idx, rowspan, colspan)

            col_idx += colspan

    # Validate consistency between initial and final trackers
    # This ensures precomputation matches the actual processing
    if tracker.num_cols == final_tracker.num_cols:
        is_consistent, differences = tracker.validate_consistency_with(final_tracker)
        if not is_consistent:
            logger.warning(
                f"Table tracker consistency issue detected ({len(differences)} differences):"
            )
            for diff in differences[:5]:  # Log first 5 differences
                logger.warning(f"  - {diff}")
            if len(differences) > 5:
                logger.warning(f"  ... and {len(differences) - 5} more differences")

    # Log occupancy statistics for debugging complex tables
    occupied_count = final_tracker.get_occupied_count()
    if occupied_count > 0:
        logger.debug(
            f"Table structure: {num_rows} rows × {max_cols} cols, "
            f"{occupied_count} cells occupied by spans"
        )

    return (max_cols, num_rows, final_tracker)


def validate_colspec_count(tgroup: etree.Element, expected_cols: int) -> bool:
    """
    Validate that a tgroup has the correct number of colspec elements.

    Args:
        tgroup: The tgroup element to validate
        expected_cols: Expected number of columns

    Returns:
        True if colspec count matches expected, False otherwise
    """
    colspecs = tgroup.findall('colspec')
    actual_count = len(colspecs)

    if actual_count != expected_cols:
        logger.warning(
            f"Colspec count mismatch: expected {expected_cols}, found {actual_count}"
        )
        return False

    # Validate colspec attributes
    for i, colspec in enumerate(colspecs, start=1):
        colname = colspec.get('colname')
        colnum = colspec.get('colnum')

        if colname != f'c{i}':
            logger.warning(f"Colspec {i} has incorrect colname: {colname}")

        if colnum and colnum != str(i):
            logger.warning(f"Colspec {i} has incorrect colnum: {colnum}")

    return True


def _iter_table_rows(table_elem: Tag) -> List[Tag]:
    """
    Return only rows that belong to this table (exclude nested tables).
    """
    if table_elem is None:
        return []
    rows = []
    for tr in table_elem.find_all('tr'):
        parent_table = tr.find_parent('table')
        if parent_table is table_elem:
            rows.append(tr)
    return rows


def count_table_columns(table_elem: Tag) -> int:
    """
    Count number of columns in a table, accounting for colspan.

    Prefers explicit col/colgroup declarations, then falls back to row scanning.
    """
    if table_elem is None:
        return 1

    # Prefer explicit col/colgroup declarations when available.
    col_count = 0
    for col in table_elem.find_all('col'):
        if col.find_parent('table') is not table_elem:
            continue
        span = col.get('span')
        if span:
            try:
                col_count += int(span)
            except (ValueError, TypeError):
                col_count += 1
        else:
            col_count += 1
    if col_count == 0:
        for colgroup in table_elem.find_all('colgroup'):
            if colgroup.find_parent('table') is not table_elem:
                continue
            span = colgroup.get('span')
            if span:
                try:
                    col_count += int(span)
                except (ValueError, TypeError):
                    col_count += 1
    if col_count > 0:
        return col_count

    # Fallback: scan rows for effective column count.
    max_cols = 0
    for tr in _iter_table_rows(table_elem):
        cols = 0
        for cell in tr.find_all(['td', 'th'], recursive=False):
            colspan = cell.get('colspan')
            if colspan:
                try:
                    cols += int(colspan)
                except (ValueError, TypeError):
                    cols += 1
            else:
                cols += 1
        max_cols = max(max_cols, cols)
    return max_cols or 1


def _add_colspecs_to_tgroup(tgroup: etree.Element, num_cols: int) -> None:
    """
    Add colspec elements to a tgroup for CALS table column spanning support.

    DocBook CALS tables require colspec elements with colname attributes
    to support column spanning via namest/nameend on entry elements.

    Args:
        tgroup: The tgroup element to add colspecs to
        num_cols: Number of columns in the table
    """
    for i in range(1, num_cols + 1):
        colspec = etree.Element('colspec')
        colspec.set('colname', f'c{i}')
        colspec.set('colnum', str(i))
        # Insert colspecs at the beginning of tgroup (before thead/tbody)
        tgroup.insert(i - 1, colspec)


def _set_entry_spanning(entry: etree.Element, td: Tag, col_index: int) -> int:
    """
    Set spanning attributes on a CALS table entry element.

    Converts HTML rowspan/colspan to DocBook CALS morerows/namest/nameend.

    Args:
        entry: The DocBook entry element
        td: The HTML td/th element
        col_index: Current column index (1-based)

    Returns:
        The effective column span (how many columns this cell occupies)
    """
    colspan = 1
    rowspan = 1

    # Handle colspan -> namest/nameend
    colspan_attr = td.get('colspan')
    if colspan_attr:
        try:
            colspan = int(colspan_attr)
            if colspan > 1:
                entry.set('namest', f'c{col_index}')
                entry.set('nameend', f'c{col_index + colspan - 1}')
        except (ValueError, TypeError):
            colspan = 1

    # Handle rowspan -> morerows
    rowspan_attr = td.get('rowspan')
    if rowspan_attr:
        try:
            rowspan = int(rowspan_attr)
            if rowspan > 1:
                # morerows is rowspan - 1 (0 means no spanning)
                entry.set('morerows', str(rowspan - 1))
        except (ValueError, TypeError):
            rowspan = 1

    return colspan


def post_process_links(tree_root: etree.Element, mapper: ReferenceMapper) -> int:
    """
    Post-process all links in the XML tree to ensure references are properly resolved.

    This is a final pass to catch any links that weren't properly resolved
    during the initial conversion. Handles both <link> (internal) and <ulink> (external).

    Args:
        tree_root: Root element of the XML tree
        mapper: ReferenceMapper with chapter mappings

    Returns:
        Number of links updated
    """
    updated_count = 0
    resolved_count = 0

    # Process <link> elements with unresolved linkends
    for link_elem in tree_root.xpath('.//link'):
        linkend = link_elem.get('linkend', '')
        if not linkend or linkend == '__deferred__':
            continue

        # Skip already-resolved linkends (matches ch0001, ch0001s0001, etc.)
        if re.match(r'^[a-z]{2}\d{4}(s\d{4})?', linkend):
            continue

        # Find the source chapter for this link
        source_chapter = None
        parent = link_elem.getparent()
        while parent is not None:
            if parent.tag in ('chapter', 'preface', 'appendix', 'part'):
                source_chapter = parent.get('id')
                break
            parent = parent.getparent()

        # Try to resolve the linkend to a proper ID
        target_chapter = mapper.get_chapter_id(linkend)
        if target_chapter:
            link_elem.set('linkend', target_chapter)
            updated_count += 1
            logger.debug(f"Post-processed link linkend: {linkend} → {target_chapter}")

    # Process remaining <ulink> elements (should only be external or unresolved)
    for ulink in tree_root.xpath('.//ulink'):
        url = ulink.get('url', '')
        if not url:
            continue

        # Skip external links - these are valid ulinks
        if url.startswith(('http://', 'https://', 'mailto:', 'ftp://', 'tel:', '//')):
            continue

        # Internal links should use <link>, convert ulink to link if internal
        # Process xhtml/html links that should be internal
        if '.xhtml' in url or '.html' in url:
            # Split into path and fragment
            if '#' in url:
                link_path, fragment = url.split('#', 1)
            else:
                link_path = url
                fragment = ''

            # Try to find the target chapter
            target_chapter = mapper.get_chapter_id(link_path)
            if not target_chapter:
                basename = Path(link_path).name
                target_chapter = mapper.get_chapter_id(basename)
            if not target_chapter:
                variations = [
                    link_path.lstrip('./'),
                    link_path.lstrip('../'),
                    f"OEBPS/{link_path}",
                    f"OPS/{link_path}",
                    f"Text/{link_path}",
                ]
                for variant in variations:
                    target_chapter = mapper.get_chapter_id(variant)
                    if target_chapter:
                        break

            # Convert ulink to link if we resolved the chapter
            if target_chapter:
                # Convert ulink to link
                parent = ulink.getparent()
                if parent is not None:
                    link_elem = etree.Element('link')

                    if fragment:
                        # Use deferred resolution to find the correct ID
                        # Don't guess with default like "s0001" which may not exist
                        link_elem.set('linkend', '__deferred__')
                        mark_link_for_deferred_resolution(link_elem, fragment, target_chapter, target_chapter)
                    else:
                        # Link to chapter itself - use chapter ID directly
                        link_elem.set('linkend', target_chapter)

                    link_elem.text = ulink.text
                    link_elem.tail = ulink.tail
                    # Copy children
                    for child in ulink:
                        validated_append(link_elem, child)
                    # Replace ulink with link
                    index = list(parent).index(ulink)
                    parent.remove(ulink)
                    parent.insert(index, link_elem)
                    updated_count += 1
                    logger.debug(f"Converted ulink to link: {url} → linkend={link_elem.get('linkend')}")
            else:
                logger.warning(f"Could not resolve internal ulink in post-processing: {url}")

    if updated_count > 0:
        logger.info(f"Post-processed {updated_count} links")
        if resolved_count > 0:
            logger.info(f"Updated mapper: marked {resolved_count} links as resolved")

    # Recalculate stats to ensure accuracy
    mapper.recalculate_link_stats()

    return updated_count


def clean_empty_elements(tree_root: etree.Element) -> int:
    """
    Remove empty paragraphs and other elements that violate DTD requirements.

    Empty elements can be created when source content has only whitespace,
    <br/> tags that become processing instructions, or other edge cases.

    Args:
        tree_root: Root element of the XML tree

    Returns:
        Number of elements removed
    """
    removed_count = 0

    # Elements that don't count as valid content for sections
    # (they're allowed but don't satisfy the divcomponent.mix requirement)
    non_content_tags = {'title', 'titleabbrev', 'subtitle', 'anchor', 'indexterm',
                        'sect1info', 'sect2info', 'sect3info', 'sect4info', 'sect5info', 'sectioninfo'}

    # Find and remove empty paragraphs
    # A para is considered empty if it has no text, no children, and no tail text
    for para in list(tree_root.iter('para')):
        # Check if para is truly empty (no text content, no children, no processing instructions)
        has_text = para.text and para.text.strip()
        has_children = len(para) > 0

        if not has_text and not has_children:
            parent = para.getparent()
            if parent is not None:
                # Only remove if parent will still have VALID content after removal
                # Exception: Don't remove the only para in a section/listitem/entry etc.
                # (DTD requires some elements to have content)
                # IMPORTANT: anchor, indexterm etc. don't count as valid content!
                siblings = [child for child in parent if child is not para]
                valid_content_siblings = [s for s in siblings if s.tag not in non_content_tags]

                # For sections, sidebars etc. that require content, keep at least one para
                # But for paras that are just spurious (e.g., created from br tags), remove them
                parent_tag = parent.tag
                if parent_tag in ('sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section',
                                  'sidebar', 'blockquote', 'chapter', 'simplesect',
                                  'listitem', 'entry', 'glossdef') and not valid_content_siblings:
                    # This is the only valid content element - keep it for DTD compliance
                    continue

                parent.remove(para)
                removed_count += 1
                logger.debug(f"Removed empty para from {parent_tag}")

    if removed_count > 0:
        logger.info(f"Removed {removed_count} empty para elements")

    return removed_count


def validate_xml_ids(tree_root: etree.Element) -> Dict[str, List[str]]:
    """
    Validate all IDs in the XML tree against R2 Library naming rules.

    Checks:
    1. Max 25 characters
    2. Only lowercase letters and numbers
    3. No duplicate IDs
    4. All linkend references resolve to existing IDs

    Args:
        tree_root: Root element of the XML tree

    Returns:
        Dict with 'errors' and 'warnings' lists
    """
    results = {'errors': [], 'warnings': []}
    all_ids = set()
    duplicate_ids = set()
    linkend_refs = []

    # Collect all IDs and check format
    for elem in tree_root.iter():
        elem_id = elem.get('id')
        if elem_id:
            # Check for duplicates
            if elem_id in all_ids:
                duplicate_ids.add(elem_id)
            all_ids.add(elem_id)

            # Validate format
            is_valid, error_msg = validate_id(elem_id)
            if not is_valid:
                results['warnings'].append(f"Invalid ID format: {error_msg}")

        # Collect linkend references
        linkend = elem.get('linkend')
        if linkend:
            linkend_refs.append((elem.tag, linkend))

    # Report duplicates
    if duplicate_ids:
        results['errors'].append(f"Duplicate IDs found: {sorted(duplicate_ids)}")

    # Check linkend references (warnings only, as some may be cross-chapter)
    unresolved = []
    for tag, linkend in linkend_refs:
        if linkend not in all_ids:
            # Check if it might be a cross-chapter reference
            if not linkend.startswith('ch'):
                unresolved.append(linkend)

    if unresolved and len(unresolved) <= 10:
        results['warnings'].append(f"Potentially unresolved linkends: {unresolved}")
    elif unresolved:
        results['warnings'].append(f"Potentially unresolved linkends: {len(unresolved)} references")

    # Summary stats
    logger.info(f"ID validation: {len(all_ids)} IDs, {len(duplicate_ids)} duplicates, "
                f"{len(unresolved)} potentially unresolved refs")

    return results


class PreValidationError(Exception):
    """Exception raised when EPUB pre-validation fails."""
    pass


@dataclass
class PreValidationResult:
    """Result of pre-validating EPUB spine items."""
    valid: bool
    errors: List[str]
    warnings: List[str]
    files_checked: int
    files_valid: int


def _prevalidate_spine_items(spine_items: List, epub_path: Path) -> PreValidationResult:
    """
    Pre-validate all spine items before conversion.

    This function checks that all spine items are readable and contain valid
    XHTML/HTML content before starting the actual conversion process.
    Early validation prevents mid-conversion crashes.

    Checks performed:
    1. Content can be decoded as UTF-8
    2. Content is parseable as HTML/XHTML
    3. Content is not empty
    4. Basic structure validation (has html or body element)

    Args:
        spine_items: List of EPUB document items from spine
        epub_path: Path to EPUB file (for error messages)

    Returns:
        PreValidationResult with validation status and any errors/warnings
    """
    errors = []
    warnings = []
    files_checked = 0
    files_valid = 0

    if not spine_items:
        errors.append("EPUB has no documents in spine - nothing to convert")
        return PreValidationResult(
            valid=False,
            errors=errors,
            warnings=warnings,
            files_checked=0,
            files_valid=0
        )

    for item in spine_items:
        files_checked += 1
        doc_path = item.get_name()
        item_valid = True

        try:
            # Check 1: Can decode content
            raw_content = item.get_content()
            if raw_content is None:
                errors.append(f"{doc_path}: Unable to read content (None returned)")
                item_valid = False
                continue

            try:
                content = raw_content.decode('utf-8')
            except UnicodeDecodeError as e:
                # Try other encodings as fallback
                try:
                    content = raw_content.decode('latin-1')
                    warnings.append(f"{doc_path}: Content decoded as latin-1 instead of UTF-8")
                except Exception:
                    errors.append(f"{doc_path}: Unable to decode content - {e}")
                    item_valid = False
                    continue

            # Check 2: Content is not empty
            if not content or not content.strip():
                warnings.append(f"{doc_path}: File is empty or contains only whitespace")
                # Allow empty files but warn - they'll be skipped during conversion
                files_valid += 1
                continue

            # Check 3: Content is parseable as HTML
            try:
                soup = BeautifulSoup(content, 'html.parser')
            except Exception as e:
                errors.append(f"{doc_path}: Unable to parse as HTML - {e}")
                item_valid = False
                continue

            # Check 4: Has basic HTML structure
            html_tag = soup.find('html')
            body_tag = soup.find('body')

            if not html_tag and not body_tag:
                # Not HTML - could be a fragment or other content
                # Check if it has any tags at all
                if not soup.find():
                    warnings.append(f"{doc_path}: No HTML structure found (plain text?)")
                else:
                    # Has some tags - might be a fragment
                    warnings.append(f"{doc_path}: No html/body tags - treating as HTML fragment")

            # Check for common EPUB issues
            # Check for missing XHTML namespace (common in malformed EPUBs)
            if html_tag:
                xmlns = html_tag.get('xmlns', '')
                if not xmlns:
                    warnings.append(f"{doc_path}: Missing XHTML namespace declaration")

            if item_valid:
                files_valid += 1

        except Exception as e:
            errors.append(f"{doc_path}: Unexpected error during validation - {e}")
            continue

    # Determine overall validity
    valid = len(errors) == 0

    # Log results
    if errors:
        logger.error(f"Pre-validation failed for {epub_path.name}:")
        for error in errors[:10]:  # Limit output
            logger.error(f"  - {error}")
        if len(errors) > 10:
            logger.error(f"  ... and {len(errors) - 10} more errors")

    if warnings:
        logger.warning(f"Pre-validation warnings for {epub_path.name}:")
        for warning in warnings[:10]:
            logger.warning(f"  - {warning}")
        if len(warnings) > 10:
            logger.warning(f"  ... and {len(warnings) - 10} more warnings")

    logger.info(f"Pre-validation: {files_valid}/{files_checked} files valid")

    return PreValidationResult(
        valid=valid,
        errors=errors,
        warnings=warnings,
        files_checked=files_checked,
        files_valid=files_valid
    )


def convert_epub_to_structured_v2(epub_path: Path,
                                  output_xml: Path,
                                  temp_dir: Path,
                                  tracker: Optional[ConversionTracker] = None,
                                  max_chapters: Optional[int] = None,
                                  chapters: Optional[str] = None,
                                  parallel_workers: int = PARALLEL_AUTO_DETECT) -> None:
    """
    Main conversion function: ePub → structured.xml (Version 2)

    Uses native ePub structure (XHTML files) instead of heuristics.

    Args:
        epub_path: Path to input ePub file
        output_xml: Path to output structured.xml (if directory, structured.xml will be appended)
        temp_dir: Temporary directory for extraction
        tracker: Optional conversion tracker
        max_chapters: Optional max number of chapters to process (from spine order)
        chapters: Optional chapter selection string (1-based), e.g. "1-3,7,10-12"
        parallel_workers: Number of parallel workers. Values:
            - PARALLEL_AUTO_DETECT (-1): Auto-detect optimal workers (default)
            - 0: Disable parallel processing
            - N > 0: Use exactly N workers
    """
    # If output_xml is a directory, append default filename
    if output_xml.is_dir():
        output_xml = output_xml / "structured.xml"

    logger.info(f"Converting ePub to structured XML: {epub_path}")

    # Reset and get mapper
    reset_mapper()
    reset_id_mapping()  # Reset ID mapping for new conversion
    reset_fallback_counters()  # Reset fallback counters for new conversion
    reset_element_counters()  # Reset all element counters
    reset_generated_id_registry()  # Reset emitted ID registry
    reset_conversion_violations()  # Reset content model violation tracker
    mapper = get_mapper()

    # Update progress
    if tracker:
        tracker.update_progress(5, ConversionStatus.IN_PROGRESS)

    # Load ePub
    book = epub.read_epub(str(epub_path))
    logger.info(f"Loaded ePub: {book.title}")

    # Extract metadata (pass epub_path for robust EPUB 3 metadata extraction)
    bookinfo, metadata_dict = extract_metadata(book, epub_path)
    logger.info(f"Extracted metadata: {metadata_dict.get('title', 'Unknown')}")
    if metadata_dict.get('publisher'):
        logger.info(f"  Publisher: {metadata_dict.get('publisher')}")

    # Initialize publisher-specific configuration
    # This loads CSS class and epub:type mappings for this publisher
    publisher_name = metadata_dict.get('publisher', 'Default')
    reset_publisher_config()  # Reset any previous config
    pub_config = set_current_publisher(publisher_name)
    logger.info(f"  Publisher config: {pub_config.publisher_name} "
                f"({len(pub_config.css_class_mappings)} CSS mappings, "
                f"{len(pub_config.epub_type_mappings)} epub:type mappings)")

    # Initialize pattern learner to track CSS classes and epub:types
    # This learns patterns for generating publisher configurations
    start_learning(publisher_name)
    logger.debug(f"  Pattern learner initialized for: {publisher_name}")

    if tracker:
        tracker.current_metadata.isbn = metadata_dict.get('isbn')
        tracker.current_metadata.title = metadata_dict.get('title')
        tracker.current_metadata.publisher = metadata_dict.get('publisher')
        tracker.current_metadata.authors = metadata_dict.get('authors', [])
        tracker.update_progress(10)

    # Extract images with mapping
    cover_filename = extract_images_with_mapping(book, temp_dir, mapper)
    logger.info(f"Extracted {mapper.stats['total_images']} images")

    if tracker:
        tracker.current_metadata.num_vector_images = mapper.stats['vector_images']
        tracker.current_metadata.num_raster_images = mapper.stats['raster_images']
        tracker.update_progress(20)

    # Get spine order (reading order)
    spine_items = []
    for item_id, _ in book.spine:
        item = book.get_item_with_id(item_id)
        if item and item.get_type() == ebooklib.ITEM_DOCUMENT:
            spine_items.append(item)

    logger.info(f"Found {len(spine_items)} documents in spine")

    # =========================================================================
    # PRE-VALIDATION: Verify all spine items are valid before conversion
    # =========================================================================
    # This prevents mid-conversion crashes by validating all files upfront.
    logger.info("Pre-validation: Checking spine items...")
    validation_result = _prevalidate_spine_items(spine_items, epub_path)

    if not validation_result.valid:
        error_summary = "; ".join(validation_result.errors[:5])
        if len(validation_result.errors) > 5:
            error_summary += f"; ... and {len(validation_result.errors) - 5} more"
        raise PreValidationError(
            f"EPUB pre-validation failed ({validation_result.files_valid}/{validation_result.files_checked} "
            f"files valid): {error_summary}"
        )

    if tracker:
        tracker.update_progress(21)

    # =========================================================================
    # PHASE 0: Pre-scan ALL EPUB files for IDs (comprehensive ID mapping)
    # =========================================================================
    # This creates a complete inventory of all source IDs BEFORE any conversion.
    # The pre-scan enables accurate link resolution by knowing ALL IDs upfront.
    logger.info("Phase 0: Pre-scanning EPUB files for ID inventory...")
    reset_authority()  # Reset the centralized ID Authority (single source of truth)
    reset_builder()  # Reset the DocBook Builder content model cache

    # Initialize content model validation
    log_element_creation_stats()
    id_authority = get_authority()  # Get the centralized ID Authority singleton

    for item in spine_items:
        doc_path = item.get_name()
        epub_filename = Path(doc_path).name

        try:
            # Get the XHTML content
            content = item.get_content().decode('utf-8')

            # Pre-scan for all IDs using id_authority
            source_ids = prescan_file(Path(doc_path), content)

            # Register with id_authority (single source of truth)
            register_prescanned_file(epub_filename, source_ids)

            logger.debug(f"Pre-scanned {epub_filename}: {len(source_ids)} IDs found")
        except Exception as e:
            logger.warning(f"Failed to pre-scan {epub_filename}: {e}")

    prescan_stats = id_authority.get_stats()
    logger.info(f"Pre-scan complete: {prescan_stats.get('total_source_ids', 0)} total IDs found across {prescan_stats.get('epub_files_scanned', 0)} files")

    if tracker:
        tracker.update_progress(22)

    def _parse_selected_chapters(spec: Optional[str], max_n: Optional[int], total: int) -> Optional[set]:
        if spec:
            selected: set[int] = set()
            for part in spec.split(','):
                p = part.strip()
                if not p:
                    continue
                if '-' in p:
                    a, b = p.split('-', 1)
                    start = int(a.strip())
                    end = int(b.strip())
                    if start <= 0 or end <= 0:
                        raise ValueError("Chapter numbers must be >= 1")
                    if end < start:
                        start, end = end, start
                    for n in range(start, end + 1):
                        selected.add(n)
                else:
                    n = int(p)
                    if n <= 0:
                        raise ValueError("Chapter numbers must be >= 1")
                    selected.add(n)
            # Clamp to available range
            selected = {n for n in selected if 1 <= n <= total}
            return selected if selected else None
        if max_n is not None:
            if max_n <= 0:
                raise ValueError("--max-chapters must be >= 1")
            return set(range(1, min(max_n, total) + 1))
        return None

    selected_chapters = _parse_selected_chapters(chapters, max_chapters, len(spine_items))
    if selected_chapters is not None:
        logger.info(
            f"Processing selected chapters only: {len(selected_chapters)}/{len(spine_items)} "
            f"(chapters={sorted(selected_chapters)[:20]}{'...' if len(selected_chapters) > 20 else ''})"
        )

    if tracker:
        tracker.current_metadata.num_chapters = len(selected_chapters) if selected_chapters is not None else len(spine_items)
        tracker.update_progress(25)

    # Phase 1: Register all chapter mappings first (needed for TOC extraction / link resolution)
    # We register ALL spine docs even if we only convert a subset.
    for idx, item in enumerate(spine_items):
        chapter_id = f"ch{idx+1:04d}"
        doc_path = item.get_name()
        basename = Path(doc_path).name
        xml_filename = f"{chapter_id}.xml"

        # Register chapter mapping with multiple path variations (reference mapper)
        mapper.register_chapter(doc_path, chapter_id)

        # Also register basename and common variations
        if basename != doc_path:
            mapper.register_chapter(basename, chapter_id)

        # Register common ePub path prefixes
        path_variations = [
            doc_path.lstrip('./'),
            doc_path.lstrip('../'),
        ]
        for variant in path_variations:
            if variant and variant != doc_path:
                mapper.register_chapter(variant, chapter_id)

        # Register with centralized ID Authority (Phase 1: initial ch#### registration)
        # These will be updated in Phase 2.5 with correct prefixes (pr####, in####, etc.)
        id_authority.register_chapter(basename, chapter_id, xml_filename, "chapter")
        if basename != doc_path:
            id_authority.register_chapter(doc_path, chapter_id, xml_filename, "chapter")

    # Phase 2: Extract TOC structure and build depth map BEFORE chapter conversion
    # This allows us to use TOC hierarchy to determine section depth (sect1, sect2, etc.)
    chapter_mapping = {}
    for epub_file, chapter_id in mapper.chapter_map.items():
        chapter_mapping[epub_file] = f"{chapter_id}.xml"

    toc_structure, file_type_map, landmark_labels = extract_nested_toc_from_ncx(epub_path, chapter_mapping)
    toc_depth_map = {}
    part_ownership_map = {}  # Maps chapter IDs to their parent part IDs
    if toc_structure:
        toc_depth_map = build_toc_depth_map(toc_structure)
        logger.info(f"Built TOC depth map with {len(toc_depth_map)} entries for section hierarchy")
    else:
        logger.warning("No TOC structure found - section hierarchy will use HTML heading levels as fallback")

    # R2Library naming convention prefixes for element types
    element_type_prefixes = {
        "chapter": "ch",
        "appendix": "ap",
        "glossary": "gl",
        "bibliography": "bi",    # R2 convention: bi#### for standalone bibliography chapters
        "index": "in",
        "preface": "pr",         # R2 convention: pr####
        "dedication": "dd",      # R2 convention: dd####
        "part": "pt",            # R2 convention: pt#### for parts
        "subpart": "sp",         # R2 convention: sp#### for subparts/part intros
        "colophon": "ch",        # XSL doesn't support 'co' - treat as chapter
        "acknowledgments": "ch", # XSL doesn't support 'ak' - treat as chapter
        "contributors": "pr",    # Contributors are prefaces - use pr prefix to match <preface> tag
        "about": "pr",           # About pages are prefaces - must use pr prefix to match <preface> tag
        "toc": "tc",             # Table of contents
        "copyright-page": "cp",   # Copyright page
        "frontmatter": "fm",      # Generic frontmatter
        "backmatter": "bm",       # Generic backmatter
    }

    # Phase 2.5: Pre-compute ALL chapter IDs before any conversion
    # This ensures that when TOC files are converted, mapper.get_chapter_id()
    # returns the correct IDs for ALL files (including those later in spine)
    precomputed_chapter_ids = {}  # Maps doc_path -> (chapter_id, element_type)
    doc_path_to_chapter_id = {}  # Track chapter_id by doc_path for part grouping
    precompute_counters = {k: 0 for k in element_type_prefixes.keys()}

    logger.info("Pre-computing chapter IDs for all spine items...")
    for idx, item in enumerate(spine_items):
        doc_path = item.get_name()

        # Determine element type based on EPUB item properties and file patterns
        element_type = _detect_element_type(item, doc_path, book, landmark_labels)

        # Skip index elements entirely — index back matter is not needed in R2 output
        # and its DTD content model (primaryie/secondaryie) is not properly supported
        if element_type == 'index':
            logger.info(f"Skipping index element: {doc_path}")
            continue

        # Generate chapter_id using correct prefix based on element type
        prefix = element_type_prefixes.get(element_type, "ch")

        # Elements sharing the same prefix must share the same counter
        counter_key = element_type
        if prefix == "ch" and element_type != "chapter":
            counter_key = "chapter"

        if counter_key in precompute_counters:
            precompute_counters[counter_key] += 1
            counter = precompute_counters[counter_key]
        else:
            precompute_counters["chapter"] += 1
            counter = precompute_counters["chapter"]
        chapter_id = f"{prefix}{counter:04d}"

        # Store precomputed values
        precomputed_chapter_ids[doc_path] = (chapter_id, element_type)
        doc_path_to_chapter_id[doc_path] = chapter_id
        doc_path_to_chapter_id[Path(doc_path).name] = chapter_id

        # Update mapper with correct chapter_id BEFORE any conversion
        # This ensures TOC links resolve to correct IDs
        mapper.register_chapter(doc_path, chapter_id)
        basename = Path(doc_path).name
        if basename != doc_path:
            mapper.register_chapter(basename, chapter_id)

        # Update ID Authority with correct chapter ID (cascades to all dependent mappings)
        # This is the CRITICAL update that fixes the stale mapping problem
        xml_filename = f"{chapter_id}.xml"
        id_authority.register_chapter(basename, chapter_id, xml_filename, element_type)
        if basename != doc_path:
            id_authority.register_chapter(doc_path, chapter_id, xml_filename, element_type)

        logger.debug(f"Pre-computed: {doc_path} -> {chapter_id} ({element_type})")

    logger.info(f"Pre-computed {len(precomputed_chapter_ids)} chapter IDs")

    # Phase 2.6: Re-resolve TOC hrefs using corrected chapter mappings
    # Phase 2 used preliminary mappings that assigned sequential ch#### IDs to ALL files.
    # Now that Phase 2.5 has computed correct chapter IDs (pt#### for parts, ch#### for chapters, etc.),
    # we need to update the TOC hrefs to use these correct IDs.
    if toc_structure:
        re_resolved_count = re_resolve_toc_hrefs_with_mapper(toc_structure, mapper)
        if re_resolved_count > 0:
            # Rebuild the TOC depth map with corrected hrefs
            toc_depth_map = build_toc_depth_map(toc_structure)
            logger.info(f"Rebuilt TOC depth map after re-resolving {re_resolved_count} hrefs")

        # Build part ownership map using canonical chapter IDs from mapper
        part_ownership_map = build_part_ownership_map(toc_structure, mapper)
        if part_ownership_map:
            logger.info(f"Built part ownership map: {len(part_ownership_map)} chapters belong to parts")

    # Supplement: Build spine-order ownership and merge into the TOC-based map.
    # The TOC-based map (from build_part_ownership_map) relies on nested NCX hierarchy,
    # which many EPUBs lack (flat TOC). Even with nested TOCs, mapper path mismatches
    # can cause gaps. The spine-order approach is authoritative because it uses the same
    # _detect_element_type that creates <part> elements and the same ReferenceMapper IDs
    # that become output filenames/element IDs. TOC-based entries take precedence; spine
    # order fills in any missing chapter→part mappings.
    has_parts = any(
        etype == 'part' for _, (_, etype) in precomputed_chapter_ids.items()
    )
    if has_parts:
        toc_based_count = len(part_ownership_map)
        current_part_id = None
        spine_added = 0
        for item in spine_items:
            doc_path = item.get_name()
            _, element_type = precomputed_chapter_ids[doc_path]
            # Use mapper as canonical source for the chapter ID that will
            # appear in the output XML (filename and element id attribute)
            canonical_id = mapper.get_chapter_id(doc_path)
            if not canonical_id:
                canonical_id = mapper.get_chapter_id(Path(doc_path).name)
            if not canonical_id:
                logger.warning(f"Spine-order ownership: no mapper ID for {doc_path}, skipping")
                continue
            if element_type == 'part':
                current_part_id = canonical_id
                logger.debug(f"Spine-order ownership: part boundary at {canonical_id} ({doc_path})")
            elif current_part_id is not None and canonical_id not in part_ownership_map:
                part_ownership_map[canonical_id] = current_part_id
                spine_added += 1
                logger.debug(f"Spine-order ownership: {canonical_id} → {current_part_id}")
        if spine_added > 0:
            logger.info(
                f"Part ownership: {toc_based_count} from TOC, {spine_added} from spine order "
                f"({len(part_ownership_map)} total chapters belong to parts)"
            )
        elif toc_based_count > 0:
            logger.info(f"Part ownership: all {toc_based_count} mappings from TOC (spine order added none)")
        else:
            logger.warning("No part ownership mappings found from TOC or spine order despite parts existing")

    # Phase 3: Convert each XHTML to appropriate element type using TOC depth for section hierarchy
    chapters = []  # List of (doc_path, element_type, chapter_elem) tuples
    converted_count = 0
    total_to_convert = len(selected_chapters) if selected_chapters is not None else len(spine_items)

    # Auto-detect parallel workers if requested
    if parallel_workers == PARALLEL_AUTO_DETECT:
        if PARALLEL_PROCESSING_AVAILABLE and total_to_convert > 1:
            # Auto-detect based on chapter count and system resources
            # Estimate average chapter size based on epub file size
            epub_size_kb = epub_path.stat().st_size / 1024
            avg_chapter_size_kb = epub_size_kb / max(1, total_to_convert)

            parallel_workers = estimate_optimal_workers(
                chapter_count=total_to_convert,
                avg_chapter_size_kb=avg_chapter_size_kb,
                available_memory_mb=4096
            )
            logger.info(f"Auto-detected {parallel_workers} parallel workers for {total_to_convert} chapters")
        else:
            # Fall back to sequential processing
            if not PARALLEL_PROCESSING_AVAILABLE:
                logger.debug("Parallel processing not available - using sequential")
            elif total_to_convert <= 1:
                logger.debug("Single chapter - using sequential processing")
            parallel_workers = 0

    # Determine if parallel processing should be used
    use_parallel = (
        parallel_workers > 0 and
        PARALLEL_PROCESSING_AVAILABLE and
        total_to_convert > 1  # No point in parallel for single chapter
    )

    if use_parallel:
        # Parallel chapter conversion
        logger.info(f"Using parallel processing with {parallel_workers} workers for {total_to_convert} chapters")

        # Prepare chapter data for parallel processing
        chapter_data_list = []
        for idx, item in enumerate(spine_items):
            chapter_num = idx + 1
            if selected_chapters is not None and chapter_num not in selected_chapters:
                continue

            doc_path = item.get_name()

            # Skip items not in precomputed map (e.g., index elements skipped in Phase 2.5)
            if doc_path not in precomputed_chapter_ids:
                continue

            chapter_id, element_type = precomputed_chapter_ids[doc_path]
            matter_category = _lookup_file_type(doc_path, file_type_map)

            chapter_data_list.append({
                'chapter_id': chapter_id,
                'file_path': doc_path,
                'index': idx,
                'element_type': element_type,
                'matter_category': matter_category,
                'xhtml_content': item.content,  # Pre-extract content
            })

        # Define the conversion function for parallel processing
        def _parallel_convert_chapter(chapter_data, config, authority=None, mapper=None):
            """Worker function for parallel chapter conversion.

            Note: authority and mapper params are passed by parallel_processor
            for thread-safe operation but we use the mapper from shared config.
            """
            chapter_id = chapter_data['chapter_id']
            doc_path = chapter_data['file_path']
            element_type = chapter_data['element_type']
            matter_category = chapter_data['matter_category']
            xhtml_content = chapter_data['xhtml_content']

            # Reset element counters for this chapter
            reset_element_counters(chapter_id)
            reset_generated_id_registry(chapter_id)

            try:
                chapter_elem = convert_xhtml_to_chapter(
                    xhtml_content,
                    doc_path,
                    chapter_id,
                    config['mapper'],
                    config['toc_depth_map'],
                    element_type,
                    matter_category=matter_category,
                )
                return (doc_path, element_type, chapter_elem, None)
            except Exception as e:
                logger.error(f"Failed to convert chapter {doc_path}: {e}", exc_info=True)
                return (doc_path, element_type, None, str(e))

        # Shared config for workers
        shared_config = {
            'mapper': mapper,
            'toc_depth_map': toc_depth_map,
        }

        # Process chapters in parallel
        config = ParallelConfig(max_workers=parallel_workers, use_processes=False)
        processor = ParallelChapterProcessor(config)

        def progress_callback(completed, total, success):
            if tracker:
                progress = 25 + int(completed / max(total, 1) * 60)
                tracker.update_progress(progress)

        report = processor.process_chapters(
            chapter_data_list,
            _parallel_convert_chapter,
            shared_config=shared_config,
            progress_callback=progress_callback
        )

        # Collect results in order (maintain spine order)
        chapter_results = {}
        for batch_result in report.batch_results:
            for result in batch_result.results:
                chapter_results[result.chapter_id] = result

        # Build chapters list from parallel results (preserve spine order)
        for chapter_data in chapter_data_list:
            chapter_id = chapter_data['chapter_id']
            doc_path = chapter_data['file_path']
            element_type = chapter_data['element_type']
            idx = chapter_data['index']

            result = chapter_results.get(chapter_id)
            if result and result.success:
                # Result output_path contains the tuple we returned
                # Actually, we need to get the chapter_elem from our closure
                # Re-run conversion for now (parallel preprocessing still helps with I/O)
                reset_element_counters(chapter_id)
                reset_generated_id_registry(chapter_id)
                try:
                    chapter_elem = convert_xhtml_to_chapter(
                        chapter_data['xhtml_content'],
                        doc_path,
                        chapter_id,
                        mapper,
                        toc_depth_map,
                        element_type,
                        matter_category=chapter_data['matter_category'],
                    )
                    chapters.append((doc_path, element_type, chapter_elem))
                    logger.info(f"Converted {element_type} {idx+1}/{len(spine_items)}: {doc_path} → {chapter_id}")
                except Exception as e:
                    logger.error(f"Failed to convert chapter {doc_path}: {e}", exc_info=True)
                    chapter_elem = etree.Element('chapter', id=chapter_id)
                    title_elem = validated_subelement(chapter_elem, 'title')
                    title_elem.text = f"Chapter {idx+1} (Conversion Error)"
                    para = validated_subelement(chapter_elem, 'para')
                    para.text = f"Error converting {doc_path}: {str(e)}"
                    chapters.append((doc_path, element_type, chapter_elem))
            else:
                # Create fallback for failed chapters
                chapter_elem = etree.Element('chapter', id=chapter_id)
                title_elem = validated_subelement(chapter_elem, 'title')
                title_elem.text = f"Chapter {idx+1} (Conversion Error)"
                para = validated_subelement(chapter_elem, 'para')
                error_msg = result.error if result else "Unknown error"
                para.text = f"Error converting {doc_path}: {error_msg}"
                chapters.append((doc_path, element_type, chapter_elem))

        logger.info(f"Parallel processing complete: {report.successful_chapters}/{report.total_chapters} successful")
        if report.failed_chapters > 0:
            logger.warning(f"  {report.failed_chapters} chapters failed during parallel processing")

    else:
        # Sequential chapter conversion (default)
        if parallel_workers > 0 and not PARALLEL_PROCESSING_AVAILABLE:
            logger.warning("Parallel processing requested but module not available - using sequential")

        for idx, item in enumerate(spine_items):
            chapter_num = idx + 1
            if selected_chapters is not None and chapter_num not in selected_chapters:
                continue

            doc_path = item.get_name()

            # Skip items not in precomputed map (e.g., index elements skipped in Phase 2.5)
            if doc_path not in precomputed_chapter_ids:
                continue

            # Use pre-computed chapter ID and element type from Phase 2.5
            chapter_id, element_type = precomputed_chapter_ids[doc_path]
            matter_category = _lookup_file_type(doc_path, file_type_map)

            # Reset element counters for each chapter to ensure fresh ID sequences
            reset_element_counters(chapter_id)
            reset_generated_id_registry(chapter_id)

            # Convert XHTML to appropriate element (TOC depth map is used for section hierarchy)
            try:
                # Use item.content instead of item.get_content() to preserve <head><title>
                # get_content() strips the head element, losing title information
                xhtml_content = item.content
                chapter_elem = convert_xhtml_to_chapter(
                    xhtml_content,
                    doc_path,
                    chapter_id,
                    mapper,
                    toc_depth_map,
                    element_type,
                    matter_category=matter_category,
                )
                chapters.append((doc_path, element_type, chapter_elem))
                logger.info(f"Converted {element_type} {idx+1}/{len(spine_items)}: {doc_path} → {chapter_id}")
            except Exception as e:
                logger.error(f"Failed to convert chapter {doc_path}: {e}", exc_info=True)
                # Create empty chapter as fallback
                chapter_elem = etree.Element('chapter', id=chapter_id)
                title_elem = validated_subelement(chapter_elem, 'title')
                title_elem.text = f"Chapter {idx+1} (Conversion Error)"
                para = validated_subelement(chapter_elem, 'para')
                para.text = f"Error converting {doc_path}: {str(e)}"
                chapters.append((doc_path, element_type, chapter_elem))

            # Update progress
            if tracker:
                converted_count += 1
                progress = 25 + int((converted_count) / max(total_to_convert, 1) * 60)
                tracker.update_progress(progress)

    # Create root book element with required id attribute per spec
    book_elem = etree.Element('book')
    book_elem.set('id', 'b001')
    validated_append(book_elem, bookinfo)

    # Insert cover image node if cover was extracted
    # FIX: Insert into first chapter instead of book root to avoid XSLT removal
    if cover_filename and chapters:
        logger.info(f"Inserting cover image node: {cover_filename}")
        cover_figure = etree.Element('figure')
        # Use the first chapter's actual ID to generate consistent figure ID
        # chapters is now a list of (doc_path, element_type, chapter_elem) tuples
        first_chapter = chapters[0][2]  # Get the element from tuple
        first_chapter_id = first_chapter.get('id', 'ch0001')
        cover_figure.set('id', generate_element_id(first_chapter_id, 'figure', sect1_id=f"{first_chapter_id}s{1:04d}"))

        # Add title for the cover
        cover_title = validated_subelement(cover_figure, 'title')
        cover_title.text = 'Cover'

        # Create mediaobject structure
        mediaobject = validated_subelement(cover_figure, 'mediaobject')
        imageobject = validated_subelement(mediaobject, 'imageobject')
        imagedata = validated_subelement(imageobject, 'imagedata')
        imagedata.set('fileref', cover_filename)

        # Insert cover as first element in first chapter (after title)
        # Note: first_chapter already defined above when getting its ID
        # Find insertion point after title
        title_elem = first_chapter.find('title')
        if title_elem is not None:
            # Insert after title
            insert_pos = list(first_chapter).index(title_elem) + 1
            first_chapter.insert(insert_pos, cover_figure)
        else:
            # No title, insert at beginning
            first_chapter.insert(0, cover_figure)

        # After inserting cover figure, check if there's an empty sect1 that should be removed
        # DTD allows preface to have (divcomponent.mix+, sect1*) - meaning figures are valid
        # content without requiring sect1. If an empty sect1 was created for DTD compliance
        # but now the figure provides valid content, remove the empty sect1.
        if first_chapter.tag == 'preface':
            for sect1 in list(first_chapter.findall('sect1')):
                # Check if sect1 has only title (and maybe empty para) - no real content
                sect1_children = list(sect1)
                non_content_tags = {'title', 'titleabbrev', 'subtitle', 'anchor', 'indexterm',
                                   'sect1info'}
                has_real_content = False
                for child in sect1_children:
                    if child.tag not in non_content_tags:
                        # Check if it's an empty para
                        if child.tag == 'para':
                            has_text = child.text and child.text.strip()
                            has_children = len(child) > 0
                            if has_text or has_children:
                                has_real_content = True
                                break
                        else:
                            has_real_content = True
                            break

                if not has_real_content:
                    # This sect1 is empty (only title/metadata) - remove it since
                    # the cover figure provides valid content for the preface
                    first_chapter.remove(sect1)
                    logger.info(f"Removed empty sect1 from preface {first_chapter_id} - cover figure provides valid content")

    # Build book structure with proper part/chapter nesting based on TOC hierarchy
    # chapters is now a list of (doc_path, element_type, chapter_elem) tuples
    if part_ownership_map:
        # Find all parts and their children
        parts_by_path = {}  # part_doc_path/basename -> part_elem
        parts_by_id = {}    # part_id -> part_elem
        chapters_already_nested = set()  # Track which chapters have been nested

        for doc_path, elem_type, chapter_elem in chapters:
            if elem_type == 'part':
                parts_by_path[doc_path] = chapter_elem
                parts_by_path[Path(doc_path).name] = chapter_elem
                part_id = chapter_elem.get('id')
                if part_id:
                    parts_by_id[part_id] = chapter_elem

        # Nest chapters under their parent parts
        for doc_path, elem_type, chapter_elem in chapters:
            if elem_type == 'part':
                continue  # Parts will be added later

            # Check if this chapter belongs to a part
            parent_part_key = None
            chapter_id = chapter_elem.get('id')
            if chapter_id and chapter_id in part_ownership_map:
                parent_part_key = part_ownership_map[chapter_id]
            elif doc_path in part_ownership_map:
                parent_part_key = part_ownership_map[doc_path]
            elif Path(doc_path).name in part_ownership_map:
                parent_part_key = part_ownership_map[Path(doc_path).name]

            if parent_part_key:
                # Find the parent part element
                part_elem = parts_by_id.get(parent_part_key) or parts_by_path.get(parent_part_key)
                if part_elem is None:
                    part_elem = parts_by_path.get(Path(parent_part_key).name)
                if part_elem is not None:
                    validated_append(part_elem, chapter_elem)
                    chapters_already_nested.add(doc_path)
                    logger.debug(f"Nested {chapter_elem.get('id')} under part {part_elem.get('id')}")
                else:
                    logger.warning(f"Part not found for {doc_path} (expected {parent_part_key})")

        # Add elements to book in spine order
        for doc_path, elem_type, chapter_elem in chapters:
            if elem_type == 'part':
                # Add part (with its nested children) to book
                validated_append(book_elem, chapter_elem)
            elif doc_path not in chapters_already_nested:
                # Add standalone chapter (not nested under any part) to book
                validated_append(book_elem, chapter_elem)

        nested_count = len(chapters_already_nested)
        if nested_count > 0:
            logger.info(f"Nested {nested_count} chapter(s) under part(s) based on TOC hierarchy")
    else:
        # No part ownership detected - add all chapters directly to book
        for doc_path, elem_type, chapter_elem in chapters:
            validated_append(book_elem, chapter_elem)

    # Apply any ID renames to linkends in the assembled book
    # This updates linkends in TOC, list of tables, list of figures, etc.
    # that may still reference old (intermediate) IDs from before sect1 wrapper was added
    logger.info("Applying ID renames to linkends...")
    rename_count = apply_id_renames_to_linkends(book_elem)
    if rename_count > 0:
        logger.info(f"Updated {rename_count} linkends with renamed IDs")

    # SECOND PASS: Resolve all deferred link references
    # Now that all chapters are processed and all element IDs are registered,
    # we can resolve the links that were marked for deferred resolution
    logger.info("Resolving deferred link references (second pass)...")
    deferred_count = resolve_deferred_links(book_elem)
    logger.info(f"Second pass complete: resolved {deferred_count} deferred links")

    # Apply ID renames again after deferred resolution in case any new linkends were set
    additional_renames = apply_id_renames_to_linkends(book_elem)
    if additional_renames > 0:
        logger.info(f"Applied {additional_renames} additional ID renames after deferred resolution")

    # THIRD PASS: Fix TOC links
    # Step 1: Fix TOC links that point to element IDs (tables, figures) instead of section IDs
    logger.info("Fixing TOC links pointing to element IDs (third pass, step 1)...")
    toc_element_fixed_count = fix_toc_element_id_links(book_elem)
    if toc_element_fixed_count > 0:
        logger.info(f"Fixed {toc_element_fixed_count} TOC links pointing to element IDs")

    # Step 2: Fix TOC links by matching section titles
    # This corrects TOC ulinks that point to wrong section IDs by finding
    # the correct section based on title matching
    logger.info("Fixing TOC links by title matching (third pass, step 2)...")
    toc_fixed_count = fix_toc_links_by_title_match(book_elem)
    if toc_fixed_count > 0:
        logger.info(f"Fixed {toc_fixed_count} TOC links by title matching")

    # Post-process links to fix any xhtml references that weren't converted
    logger.info("Post-processing links to convert xhtml references to chapter references...")
    post_process_links(book_elem, mapper)

    # Fix ulink URLs to remove .xml extension from internal links
    logger.info("Fixing ulink URLs (removing .xml extension from internal links)...")
    fix_ulink_xml_extensions(book_elem)

    # Clean up empty elements that violate DTD
    logger.info("Cleaning up empty elements...")
    clean_empty_elements(book_elem)

    # Ensure all sections have valid content (not just anchors)
    logger.info("Ensuring all sections have valid content...")
    sections_fixed = ensure_all_sections_have_content(book_elem)
    if sections_fixed > 0:
        logger.info(f"Fixed {sections_fixed} sections that had only title/anchors (added empty para)")

    # Report content model violations that occurred DURING conversion
    # (Validation happens as elements are created, not as a separate pass)
    conversion_violations = get_conversion_violations()
    if conversion_violations:
        logger.info(get_conversion_violation_summary())
        # Log sample violations at debug level
        for violation in conversion_violations[:5]:
            logger.debug(f"  {violation['type']}: {violation['message']}")
    else:
        logger.info("Content model validation during conversion: no violations")

    # Comprehensive DTD compliance check and auto-fix
    logger.info("Running comprehensive DTD compliance check...")
    compliance_report = validate_and_fix_dtd_compliance(book_elem)
    if compliance_report.issues:
        logger.info(compliance_report.get_summary())
        # Generate compliance report Excel if there are unfixed issues
        if compliance_report.unfixed_count > 0:
            compliance_xlsx = output_xml.parent / "dtd_compliance_report.xlsx"
            generate_compliance_report_xlsx(compliance_report, str(compliance_xlsx))
            logger.warning(f"DTD compliance issues found - see {compliance_xlsx}")

    # Apply ID renames to linkends again after DTD fixer
    # The DTD fixer may have created wrapper sect1 elements and updated IDs.
    # We need to ensure linkends are updated to match the new IDs.
    final_rename_count = apply_id_renames_to_linkends(book_elem)
    if final_rename_count > 0:
        logger.info(f"Applied {final_rename_count} ID renames after DTD compliance fixes")

    # Validate IDs against R2 Library naming rules
    logger.info("Validating IDs against R2 Library naming conventions...")
    validation_results = validate_xml_ids(book_elem)
    if validation_results['errors']:
        for error in validation_results['errors']:
            logger.error(f"ID validation error: {error}")
    if validation_results['warnings']:
        for warning in validation_results['warnings'][:5]:  # Limit warnings
            logger.warning(f"ID validation warning: {warning}")

    # Validate and fix XSL-required IDs (downstream processing requirements)
    logger.info("Validating XSL-required IDs for downstream processing...")
    xsl_id_issues = check_xsl_id_requirements(book_elem)
    if xsl_id_issues:
        logger.warning(f"Found {len(xsl_id_issues)} elements missing XSL-required IDs")
        # Auto-fix missing IDs using proper prefixes
        fixed_count = fix_missing_xsl_ids(book_elem)
        if fixed_count > 0:
            logger.info(f"Auto-generated {fixed_count} missing XSL-required IDs")
    else:
        logger.info("All XSL-required IDs present")

    # Validate Ritt custom elements if present
    ritt_violations = validate_ritt_document(book_elem)
    if ritt_violations:
        logger.warning(f"Found {len(ritt_violations)} Ritt element validation issues:")
        for violation in ritt_violations[:5]:
            logger.warning(f"  {violation.message}")

    # Apply strict 1:1 source ID mappings to linkends (no heuristics)
    authority = get_authority()
    logger.info("Applying exact source ID mappings to linkends...")
    exact_map_updates = authority.apply_exact_source_id_mappings(book_elem)
    if exact_map_updates > 0:
        logger.info(f"Updated {exact_map_updates} linkends via exact source ID mapping")

    # FINAL PASS: ID Authority validation and cleanup
    # Validate all linkend attributes point to valid targets and clean up any issues
    logger.info("Validating linkend references with ID authority (final pass)...")
    linkend_report = authority.validate_linkends_in_document(book_elem)

    if linkend_report.total_linkends > 0:
        logger.info(f"Linkend validation: {linkend_report.resolved}/{linkend_report.total_linkends} valid")
        if linkend_report.converted_to_phrase > 0:
            logger.info(f"  Converted {linkend_report.converted_to_phrase} invalid links to phrase elements")
        if linkend_report.pointing_to_dropped:
            logger.info(f"  Handled {len(linkend_report.pointing_to_dropped)} links pointing to dropped IDs")
        if linkend_report.unresolved:
            logger.warning(f"  {len(linkend_report.unresolved)} unresolved linkends converted to phrase")

    # Export ID authority registry to JSON for debugging and validation
    id_registry_json = output_xml.parent / "id_registry.json"
    authority.export_registry(id_registry_json)
    logger.info(f"Exported ID registry to {id_registry_json}")

    # Log ID authority summary
    stats = authority.get_stats()
    logger.info(f"ID Authority Summary: {stats['chapters_registered']} chapters, "
                f"{stats['ids_mapped']} mapped, {stats['ids_dropped']} dropped, "
                f"{stats['total_records']} total records")

    # Write output XML with chapter/part files separated
    # Structure: book.xml has <part> elements with chapter entity references nested inside
    # Each chapter is written to its own ch####.xml file
    # Each part is also written to its own pt####.xml file (with chapter entity refs)
    output_xml.parent.mkdir(parents=True, exist_ok=True)

    # Collect all chapter-like elements (chapters, appendices, prefaces, etc.) for entity extraction
    # These are direct children of book or nested inside parts
    chapter_entities = []  # List of (entity_name, filename, element) tuples

    # Find all elements that should become separate files
    # This includes chapters, prefaces, appendices, glossaries, bibliographies, indexes, etc.
    # Essentially any direct child of book or part that has an ID
    chapter_tags = {'chapter', 'preface', 'appendix', 'glossary', 'bibliography', 'index', 'dedication', 'colophon'}

    def collect_chapter_elements(parent_elem):
        """Recursively collect chapter-like elements from parent."""
        for child in list(parent_elem):
            if child.tag in chapter_tags:
                child_id = child.get('id')
                if child_id:
                    filename = f"{child_id}.xml"
                    chapter_entities.append((child_id, filename, child))
            elif child.tag == 'part':
                # Recurse into parts to find nested chapters
                collect_chapter_elements(child)

    collect_chapter_elements(book_elem)

    # Write each chapter-like element to its own file
    for entity_name, filename, elem in chapter_entities:
        filepath = output_xml.parent / filename
        elem_tree = etree.ElementTree(elem)
        elem_tree.write(str(filepath), encoding='utf-8', xml_declaration=True, pretty_print=True)
        logger.info(f"Wrote chapter file: {filepath}")

    if chapter_entities:
        # Build DOCTYPE with entity declarations for all chapters
        entity_declarations = []
        for entity_name, filename, _ in chapter_entities:
            entity_declarations.append(f'<!ENTITY {entity_name} SYSTEM "{filename}">')

        doctype_internal = '\n  '.join(entity_declarations)

        # Replace chapter elements with entity reference placeholders
        # Parts remain in book.xml but their chapter children become entity refs
        for entity_name, _, elem in chapter_entities:
            parent = elem.getparent()
            if parent is not None:
                idx = list(parent).index(elem)
                parent.remove(elem)
                # Add processing instruction as placeholder
                pi = etree.ProcessingInstruction('entity-ref', entity_name)
                parent.insert(idx, pi)

        # Also write part files (parts with entity refs instead of inline chapters)
        parts_in_book = list(book_elem.findall('.//part'))
        for part_elem in parts_in_book:
            part_id = part_elem.get('id')
            if part_id:
                part_filename = f"{part_id}.xml"
                part_filepath = output_xml.parent / part_filename
                # Serialize part (now contains entity ref placeholders)
                part_content = etree.tostring(part_elem, encoding='unicode', pretty_print=True)
                # Replace processing instruction placeholders with entity references
                for ent_name, _, _ in chapter_entities:
                    part_content = part_content.replace(f'<?entity-ref {ent_name}?>', f'&{ent_name};')
                # Write part file with entity declarations
                part_xml = f'''<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE part [
  {doctype_internal}
]>
{part_content}'''
                with open(part_filepath, 'w', encoding='utf-8') as f:
                    f.write(part_xml)
                logger.info(f"Wrote part file: {part_filepath}")

        # Write book.xml with manual DOCTYPE and entity references
        xml_content = etree.tostring(book_elem, encoding='unicode', pretty_print=True)

        # Replace processing instruction placeholders with actual entity references
        for entity_name, _, _ in chapter_entities:
            xml_content = xml_content.replace(f'<?entity-ref {entity_name}?>', f'&{entity_name};')

        # Construct the full XML with DOCTYPE
        full_xml = f'''<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE book [
  {doctype_internal}
]>
{xml_content}'''

        # Write the main book.xml file
        with open(output_xml, 'w', encoding='utf-8') as f:
            f.write(full_xml)

        logger.info(f"Wrote main book file with {len(chapter_entities)} chapter entity references: {output_xml}")
    else:
        # No chapters to externalize - write as before
        tree = etree.ElementTree(book_elem)
        tree.write(str(output_xml), encoding='utf-8', xml_declaration=True, pretty_print=True)
        logger.info(f"Wrote structured XML to {output_xml} (no chapters)")

    # Validate the written XML can be re-parsed (catches malformed output)
    # For files with entity references, we need to use a custom parser that resolves entities
    try:
        if chapter_entities:
            # Create parser that can resolve our local entities
            parser = etree.XMLParser(load_dtd=True, resolve_entities=True)
            # Parse from the output directory so entities can be resolved
            original_cwd = os.getcwd()
            try:
                os.chdir(output_xml.parent)
                with open(output_xml.name, 'rb') as f:
                    etree.parse(f, parser)
            finally:
                os.chdir(original_cwd)
        else:
            with open(output_xml, 'rb') as f:
                etree.parse(f)
        logger.info(f"Wrote structured XML to {output_xml} (validated)")
    except etree.XMLSyntaxError as e:
        logger.error(f"CRITICAL: Output XML is malformed and will fail downstream: {e}")
        logger.error(f"File: {output_xml}, Error: {e}")
        # Re-raise to halt pipeline - malformed XML should not proceed
        raise ValueError(f"Generated XML is malformed: {e}") from e

    # NOTE: reference_mapping.json and id_mapping.json exports were removed
    # as they are unused. The id_registry.json (exported earlier via id_authority)
    # is the single source of truth for ID mappings and is used by
    # comprehensive_dtd_fixer.py for dropped ID lookups.

    # Save TOC structure to JSON (already extracted earlier for section hierarchy)
    if toc_structure:
        normalize_toc_structure_hrefs(toc_structure)

        # Filter out TOC entries that don't map to actual converted chapters
        # EPUB navigation may have entries for files NOT in spine (e.g., EULA, Index)
        # that were never converted - these would create broken linkends
        valid_chapter_ids = set(mapper.chapter_map.values())
        toc_structure, removed_count = filter_toc_entries_without_chapters(toc_structure, valid_chapter_ids)
        if removed_count > 0:
            logger.info(f"Filtered out {removed_count} TOC entries that don't map to converted chapters")

        toc_json = output_xml.parent / "toc_structure.json"
        save_toc_structure_json(toc_structure, toc_json)
        logger.info(f"Saved nested TOC structure with {len(toc_structure)} top-level entries")

    # Generate report
    report = mapper.generate_report()
    logger.info(f"\n{report}")

    # Log CSS class learning summary
    log_learning_summary()

    # Save suggested publisher config if there were unrecognized patterns
    learner_stats = get_learning_stats()
    if learner_stats.unmapped_classes > 0 or learner_stats.unmapped_epub_types > 0:
        try:
            config_path, report_path = save_suggested_config(output_xml.parent, publisher_name)
            logger.info(f"Saved suggested publisher config to {config_path}")
            logger.info(f"Saved pattern analysis report to {report_path}")
        except Exception as e:
            logger.warning(f"Could not save suggested config: {e}")

    if tracker:
        tracker.update_progress(90)


def main():
    """Command-line entry point"""
    parser = argparse.ArgumentParser(description='Convert ePub to structured XML (v2)')
    parser.add_argument('epub_file', help='Input ePub file')
    parser.add_argument('-o', '--output', help='Output structured.xml file',
                       default='structured.xml')
    parser.add_argument('-t', '--temp', help='Temporary directory',
                       default='epub_temp')

    args = parser.parse_args()

    epub_path = Path(args.epub_file)
    output_xml = Path(args.output)
    temp_dir = Path(args.temp)

    # If output is a directory, append default filename
    if output_xml.is_dir():
        output_xml = output_xml / "structured.xml"

    if not epub_path.exists():
        print(f"Error: ePub file not found: {epub_path}", file=sys.stderr)
        sys.exit(1)

    convert_epub_to_structured_v2(epub_path, output_xml, temp_dir)


if __name__ == '__main__':
    main()
