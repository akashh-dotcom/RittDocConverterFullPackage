#!/usr/bin/env python3
"""
DocBook XML Processing Rules Export
===================================

This module exports all ID generation, DTD validation, and post-processing rules
from the RittDocConverter pipeline for reuse in PDF processing pipelines.

Exported Components:
1. ID GENERATION RULES - Section/Element ID formats and generation
2. DTD VALIDATION RULES - Content model definitions and validation
3. POST-PROCESSING RULES - Spacing fixes, number stripping, content transformations

Usage:
    from exports.docbook_processing_rules import (
        # ID Generation
        generate_section_id, generate_element_id, next_available_sect1_id,
        ELEMENT_TYPE_TO_CODE, MAX_ID_LENGTH,

        # DTD Validation
        ELEMENTS_REQUIRING_CONTENT, DIVCOMPONENT_MIX, LEGALNOTICE_MIX,
        validate_element_content, fix_empty_element,

        # Post-Processing
        fix_spacing_in_text, strip_leading_numbers,
        SPACING_PATTERNS, PUBLISHER_DETECTION_PATTERNS,
    )

IMPORTANT - Stateful ID Management:
===================================

This module contains STATELESS rules and standalone generation functions.
For the FULL stateful ID management system used in EPUB-to-DocBook conversion,
see the IDAuthority system in the main codebase:

    from id_authority import get_authority, IDAuthority

The IDAuthority provides:
- Chapter registration and mapping (register_chapter, get_chapter_id)
- ID lifecycle tracking (PRESCANNED → REGISTERED → MAPPED → FINALIZED/DROPPED)
- Cascading updates when chapter IDs change
- Linkend resolution with quality tracking
- Thread-safe operations for parallel processing
- Audit trail for debugging

See exports/ID_AUTHORITY_REFERENCE.md for complete documentation.
"""

import re
import logging
from dataclasses import dataclass, field
from enum import Enum
from typing import Dict, List, Optional, Set, Tuple, Any

logger = logging.getLogger(__name__)


# =============================================================================
# PART 1: ID GENERATION RULES
# =============================================================================

# -----------------------------------------------------------------------------
# 1.1 CORE CONSTANTS
# -----------------------------------------------------------------------------

MAX_ID_LENGTH = 25  # Maximum allowed ID length (R2 Library constraint)

# Valid chapter type prefixes (2 characters)
VALID_CHAPTER_PREFIXES = {
    'ch',   # Chapter
    'ap',   # Appendix
    'pr',   # Preface
    'in',   # Index
    'gl',   # Glossary
    'bi',   # Bibliography
    'dd',   # Dedication
    'pt',   # Part
    'sp',   # Subpart
    'tc',   # TOC
    'cp',   # Copyright
    'fm',   # Front matter
    'bm',   # Back matter
}

# XSL-Recognized element codes (support cross-reference popups in link.ritt.xsl)
XSL_RECOGNIZED_CODES = {'fg', 'ta', 'bib', 'eq', 'gl', 'qa', 'pr', 'vd', 'ad'}

# All valid element codes
VALID_ELEMENT_CODES = {
    # XSL-recognized (popup/modal support)
    'fg',   # Figure, informalfigure
    'ta',   # Table, informaltable
    'bib',  # Bibliography entries
    'eq',   # Equation, informalequation
    'gl',   # Glossary entries
    'qa',   # Q&A entries
    'pr',   # Procedure
    'vd',   # Video content
    'ad',   # Admonitions (note, warning, tip, caution, important), sidebar, audio

    # Non-XSL-recognized (basic anchors only)
    'a',    # Generic anchor
    'l',    # Lists
    'li',   # List items
    'p',    # Paragraphs
    'ex',   # Examples
    'fn',   # Footnotes
    'mo',   # Media objects
    'st',   # Procedure steps
    'ss',   # Procedure substeps
    'sa',   # Step alternatives
    'bq',   # Block quotes
    'tm',   # Terms
    'ix',   # Index terms
    'sc',   # Simple sections
}


# -----------------------------------------------------------------------------
# 1.2 ELEMENT TYPE TO CODE MAPPING (Single Source of Truth)
# -----------------------------------------------------------------------------

ELEMENT_TYPE_TO_CODE: Dict[str, str] = {
    # =========================================================================
    # XSL-RECOGNIZED - These work with link.ritt.xsl for popups/modals
    # =========================================================================
    'figure': 'fg',
    'informalfigure': 'fg',
    'table': 'ta',
    'informaltable': 'ta',
    'bibliography': 'bib',
    'bibliomixed': 'bib',
    'biblioentry': 'bib',
    'equation': 'eq',
    'informalequation': 'eq',
    'glossentry': 'gl',
    'glossterm': 'gl',
    'qandaset': 'qa',
    'qandaentry': 'qa',
    'question': 'qa',
    'answer': 'qa',
    'procedure': 'pr',
    'video': 'vd',
    'videoobject': 'vd',
    'audio': 'ad',
    'audioobject': 'ad',
    'sidebar': 'ad',
    'note': 'ad',
    'warning': 'ad',
    'caution': 'ad',
    'important': 'ad',
    'tip': 'ad',
    'admonition': 'ad',

    # =========================================================================
    # NOT XSL-RECOGNIZED - Basic links only, no popup/modal
    # =========================================================================
    'anchor': 'a',
    'list': 'l',
    'itemizedlist': 'l',
    'orderedlist': 'l',
    'variablelist': 'l',
    'simplelist': 'l',
    'listitem': 'li',
    'para': 'p',
    'paragraph': 'p',
    'simpara': 'p',
    'formalpara': 'p',
    'example': 'ex',
    'informalexample': 'ex',
    'footnote': 'fn',
    'mediaobject': 'mo',
    'imageobject': 'mo',
    'inlinemediaobject': 'mo',
    'step': 'st',
    'substep': 'ss',
    'stepalternatives': 'sa',
    'blockquote': 'bq',
    'epigraph': 'bq',
    'term': 'tm',
    'indexterm': 'ix',
    'simplesect': 'sc',
    # NOTE: Section elements (sect1-sect5) are NOT included here.
    # Section IDs use generate_section_id(), NOT element codes.
}

DEFAULT_ELEMENT_CODE = 'a'  # Fallback for unknown element types


def get_element_code(element_type: str) -> str:
    """
    Get the ID code for an element type.

    Args:
        element_type: DocBook element type name (e.g., 'figure', 'table')

    Returns:
        Two or three letter code (e.g., 'fg', 'ta', 'bib')
    """
    return ELEMENT_TYPE_TO_CODE.get(element_type, DEFAULT_ELEMENT_CODE)


# -----------------------------------------------------------------------------
# 1.3 SECTION ID FORMAT RULES
# -----------------------------------------------------------------------------

class SectionFormat:
    """Section ID format constants."""
    # Level 1-2: 4-digit counters (s0001)
    # Level 3-5: 2-digit counters (s01) to stay within 25 char limit
    LEVEL_1_2_DIGITS = 4
    LEVEL_3_5_DIGITS = 2
    MAX_LEVEL = 5


# ID Format Examples:
# ------------------
# Chapter:  ch0001 (6 chars)
# Sect1:    ch0001s0001 (11 chars)
# Sect2:    ch0001s0001s0002 (16 chars)
# Sect3:    ch0001s0001s0001s01 (19 chars)
# Sect4:    ch0001s0001s0001s01s02 (22 chars)
# Sect5:    ch0001s0001s0001s01s01s03 (25 chars) - MAX
# Element:  ch0001s0001fg0001 (17 chars) - figure in sect1


# -----------------------------------------------------------------------------
# 1.4 ID VALIDATION PATTERNS
# -----------------------------------------------------------------------------

def _build_chapter_pattern() -> re.Pattern:
    """Build chapter ID pattern from valid prefixes."""
    prefixes = "|".join(sorted(VALID_CHAPTER_PREFIXES, key=len, reverse=True))
    return re.compile(rf'^({prefixes})(\d{{4}})$')


def _build_section_pattern() -> re.Pattern:
    """Build section ID pattern supporting mixed digit formats."""
    prefixes = "|".join(sorted(VALID_CHAPTER_PREFIXES, key=len, reverse=True))
    return re.compile(
        rf'^({prefixes})(\d{{4}})'   # Chapter prefix + number
        rf'(s\d{{4}})'               # sect1 (required, 4 digits)
        rf'(?:(s\d{{4}})'            # sect2 (optional, 4 digits)
        rf'(?:(s\d{{2}})'            # sect3 (optional, 2 digits)
        rf'(?:(s\d{{2}})'            # sect4 (optional, 2 digits)
        rf'(?:(s\d{{2}})'            # sect5 (optional, 2 digits)
        rf')?)?)?)?$'
    )


def _build_element_pattern() -> re.Pattern:
    """Build element ID pattern from valid codes."""
    prefixes = "|".join(sorted(VALID_CHAPTER_PREFIXES, key=len, reverse=True))
    codes = "|".join(sorted(VALID_ELEMENT_CODES, key=len, reverse=True))
    return re.compile(
        rf'^({prefixes})(\d{{4}})'   # Chapter prefix + number
        rf'(s\d{{4}})'               # sect1 (4 digits)
        rf'(?:(s\d{{4}})'            # sect2 (optional, 4 digits)
        rf'(?:(s\d{{2}})'            # sect3 (optional, 2 digits)
        rf'(?:(s\d{{2}})'            # sect4 (optional, 2 digits)
        rf'(?:(s\d{{2}})'            # sect5 (optional, 2 digits)
        rf')?)?)?)?'
        rf'({codes})'                # Element code (validated)
        rf'(\d{{1,4}})$'             # Element number (1-4 digits)
    )


# Compiled patterns at module load
CHAPTER_PATTERN = _build_chapter_pattern()
SECTION_PATTERN = _build_section_pattern()
ELEMENT_PATTERN = _build_element_pattern()

# Looser pattern for detecting generated IDs
GENERATED_ID_PATTERN = re.compile(
    rf'^[a-z]{{2}}\d{{4}}'      # Any 2-letter prefix + 4 digits
    rf'(?:s\d{{2,4}})*'          # Zero or more section parts
    rf'(?:[a-z]{{1,3}}\d+)?$'    # Optional element code + number
)

# Simple patterns for quick validation
CHAPTER_ID_PATTERN_SIMPLE = re.compile(r'^[a-z]{2}\d{4}$')
SECTION_ID_PATTERN_SIMPLE = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})+$')
ELEMENT_ID_PATTERN_SIMPLE = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})*[a-z]{1,3}\d{1,4}$')


# -----------------------------------------------------------------------------
# 1.5 ID GENERATION FUNCTIONS
# -----------------------------------------------------------------------------

def generate_chapter_id(chapter_index: int, prefix: str = 'ch') -> str:
    """
    Generate a chapter ID.

    Args:
        chapter_index: 0-based chapter index
        prefix: Chapter prefix (default: 'ch')

    Returns:
        Chapter ID like "ch0001"
    """
    return f"{prefix}{chapter_index + 1:04d}"


def generate_section_id(chapter_id: str, level: int, section_counters: Dict[str, int]) -> str:
    """
    Generate a hierarchical section ID using 1-based numbering.

    Format (designed to stay within 25 character limit):
    - Level 1 (sect1): ch0001s0001 (4-digit counter)
    - Level 2 (sect2): ch0001s0001s0001 (4-digit counter)
    - Level 3 (sect3): ch0001s0001s0001s01 (2-digit counter)
    - Level 4 (sect4): ch0001s0001s0001s01s01 (2-digit counter)
    - Level 5 (sect5): ch0001s0001s0001s01s01s01 (2-digit counter)

    Args:
        chapter_id: Chapter ID (e.g., "ch0001")
        level: Section level (1-5)
        section_counters: Dict tracking section counters at each level

    Returns:
        Hierarchical section ID (max 25 characters)
    """
    level = max(1, min(level, 5))

    level_key = f'level_{level}'
    # Reset deeper level counters when entering a new section
    for deeper in range(level + 1, 7):
        section_counters[f'level_{deeper}'] = 0

    if level_key not in section_counters:
        section_counters[level_key] = 0
    section_counters[level_key] += 1

    section_id = chapter_id
    for l in range(1, level + 1):
        counter = section_counters.get(f'level_{l}', 0)
        if counter < 0:
            counter = 0

        # Levels 1-2 use 4-digit counters (s0001, s0001s0001)
        # Levels 3-5 use 2-digit counters to stay within 25-char limit
        if l <= 2:
            if counter > 9999:
                logger.warning(f"Section counter {counter} at level {l} exceeds 9999")
                counter = counter % 10000
            section_id += f"s{counter:04d}"
        else:
            if counter > 99:
                logger.warning(f"Section counter {counter} at level {l} exceeds 99")
                counter = counter % 100
            section_id += f"s{counter:02d}"

    if len(section_id) > MAX_ID_LENGTH:
        logger.error(f"Section ID exceeds max length ({len(section_id)}): {section_id}")
        section_id = section_id[:MAX_ID_LENGTH]

    return section_id


def generate_element_id(section_id: str, element_type: str,
                        element_counters: Dict[str, int]) -> str:
    """
    Generate an element ID within a section.

    Args:
        section_id: Section ID (e.g., "ch0001s0001")
        element_type: DocBook element type (e.g., 'figure', 'table')
        element_counters: Dict tracking element counters by type within section

    Returns:
        Element ID (e.g., "ch0001s0001fg0001")
    """
    code = get_element_code(element_type)

    # Create key for counter (section + code)
    counter_key = f"{section_id}_{code}"
    if counter_key not in element_counters:
        element_counters[counter_key] = 0
    element_counters[counter_key] += 1
    counter = element_counters[counter_key]

    # Calculate available space for counter
    available_space = MAX_ID_LENGTH - len(section_id) - len(code)

    if available_space >= 4:
        element_id = f"{section_id}{code}{counter:04d}"
    elif available_space >= 2:
        element_id = f"{section_id}{code}{counter:02d}"
    else:
        # Truncate section ID to fit
        max_section_len = MAX_ID_LENGTH - len(code) - 2
        element_id = f"{section_id[:max_section_len]}{code}{counter:02d}"
        logger.warning(f"Truncated section ID to fit element ID within {MAX_ID_LENGTH} chars")

    return element_id


# Pattern for matching sect1 IDs with 4-digit counter
_SECT1_ID_4DIGIT_PATTERN = re.compile(r'^([a-z]{2}\d{4})s(\d{4})$')


def next_available_sect1_id(chapter_id: str, existing_ids) -> str:
    """
    Generate a unique, convention-compliant sect1 ID for a chapter.

    Args:
        chapter_id: Chapter ID (e.g., "ch0003")
        existing_ids: Iterable of existing IDs

    Returns:
        Unique sect1 ID (e.g., "ch0003s0001")
    """
    existing: Set[str] = set(existing_ids)
    used_nums: List[int] = []
    prefix = f"{chapter_id}s"

    for id_val in existing:
        if not id_val.startswith(prefix):
            continue
        m = _SECT1_ID_4DIGIT_PATTERN.match(id_val)
        if m and m.group(1) == chapter_id:
            used_nums.append(int(m.group(2)))

    candidate_num = (max(used_nums) + 1) if used_nums else 1

    while True:
        if candidate_num > 9999:
            candidate_num = 9999
        candidate = f"{chapter_id}s{candidate_num:04d}"
        if candidate not in existing:
            return candidate
        if candidate_num == 9999:
            suffix = 0
            while True:
                fallback = f"{chapter_id}s{candidate_num:04d}{suffix}"
                if fallback not in existing and len(fallback) <= MAX_ID_LENGTH:
                    return fallback
                suffix += 1
        candidate_num += 1


# -----------------------------------------------------------------------------
# 1.6 ID VALIDATION AND UTILITY FUNCTIONS
# -----------------------------------------------------------------------------

def sanitize_id(raw_id: str) -> str:
    """
    Sanitize an ID to be compliant with naming rules.

    - Convert to lowercase
    - Remove hyphens, underscores, and special characters
    - Truncate to max length

    Args:
        raw_id: Original ID string

    Returns:
        Sanitized ID string
    """
    sanitized = raw_id.lower()
    sanitized = re.sub(r'[^a-z0-9]', '', sanitized)
    if len(sanitized) > MAX_ID_LENGTH:
        sanitized = sanitized[:MAX_ID_LENGTH]
    return sanitized


def validate_id(id_value: str) -> Tuple[bool, Optional[str]]:
    """
    Validate an ID against naming rules.

    Args:
        id_value: ID to validate

    Returns:
        Tuple of (is_valid, error_message)
    """
    if not id_value:
        return False, "Empty ID value"
    if len(id_value) > MAX_ID_LENGTH:
        return False, f"ID exceeds {MAX_ID_LENGTH} characters: {id_value} ({len(id_value)} chars)"
    if not re.match(r'^[a-z0-9]+$', id_value):
        return False, f"ID contains invalid characters: {id_value}"
    return True, None


def detect_id_type(id_value: str) -> Optional[str]:
    """
    Detect the type of ID based on its structure.

    Returns:
        'chapter', 'section', 'element', or None if unrecognized
    """
    if not id_value:
        return None
    if CHAPTER_ID_PATTERN_SIMPLE.match(id_value):
        return 'chapter'
    if SECTION_ID_PATTERN_SIMPLE.match(id_value):
        return 'section'
    if ELEMENT_ID_PATTERN_SIMPLE.match(id_value):
        return 'element'
    return None


def extract_chapter_from_id(id_value: str) -> Optional[str]:
    """
    Extract chapter ID from an element ID.

    Args:
        id_value: Element ID (e.g., "ch0001s0001fg0001")

    Returns:
        Chapter ID (e.g., "ch0001") or None if not found
    """
    match = re.match(r'^([a-z]{2}\d{4})', id_value)
    if match:
        return match.group(1)
    return None


def extract_section_hierarchy(id_value: str) -> List[str]:
    """
    Extract the section hierarchy from an ID.

    Args:
        id_value: Element ID (e.g., "ch0001s0001s0002fg0001")

    Returns:
        List of section IDs from outermost to innermost
    """
    hierarchy = []
    chapter_match = re.match(r'^([a-z]{2}\d{4})', id_value)
    if not chapter_match:
        return hierarchy

    current = chapter_match.group(1)
    remaining = id_value[len(current):]
    section_pattern = re.compile(r'^(s\d{2,4})')

    while remaining:
        section_match = section_pattern.match(remaining)
        if section_match:
            current += section_match.group(1)
            hierarchy.append(current)
            remaining = remaining[len(section_match.group(1)):]
        else:
            break

    return hierarchy


# =============================================================================
# PART 2: DTD VALIDATION RULES
# =============================================================================

# -----------------------------------------------------------------------------
# 2.1 ELEMENTS REQUIRING CONTENT
# -----------------------------------------------------------------------------

ELEMENTS_REQUIRING_CONTENT = {
    # Hierarchical elements (require title + content)
    'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect',
    'chapter', 'appendix', 'preface', 'part', 'dedication', 'colophon',

    # Lists (require at least one item)
    'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
    'segmentedlist', 'calloutlist', 'glosslist',

    # List items and entries (require content)
    'listitem', 'varlistentry', 'callout', 'glossentry',

    # Admonitions (require content)
    'note', 'warning', 'caution', 'important', 'tip',

    # Block elements
    'blockquote', 'epigraph', 'footnote', 'sidebar',
    'abstract', 'authorblurb', 'personblurb', 'legalnotice',

    # Figures and examples
    'figure', 'informalfigure', 'example', 'informalexample',

    # Media objects
    'mediaobject', 'inlinemediaobject', 'imageobject', 'videoobject', 'audioobject',

    # Tables
    'table', 'informaltable', 'tgroup', 'thead', 'tfoot', 'tbody', 'row',

    # Bibliography/Glossary
    'bibliography', 'bibliodiv', 'glossary', 'glossdiv', 'glossdef',

    # Index
    'indexdiv', 'indexentry',

    # TOC
    'tocpart', 'tocchap', 'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5',

    # Reference entries
    'refentry', 'refnamediv', 'refsynopsisdiv', 'refsection',

    # Procedures
    'procedure', 'step', 'substeps', 'stepalternatives',

    # Q&A
    'question', 'qandaentry',

    # Messages
    'msgset', 'msgentry', 'simplemsgentry', 'msg', 'msgmain', 'msgexplan',

    # Other
    'highlights', 'revhistory', 'authorgroup', 'copyright',
}

# Elements that CAN be empty
ELEMENTS_CAN_BE_EMPTY = {
    'para', 'simpara', 'title', 'titleabbrev', 'subtitle',
    'programlisting', 'literallayout', 'screen', 'synopsis',
    'caption', 'remark', 'address', 'entry',
    'phrase', 'emphasis', 'literal', 'code', 'command',
}

# Elements with EMPTY content model (no children allowed)
EMPTY_ELEMENTS = {
    'colspec', 'spanspec', 'area', 'co', 'coref',
    'graphic', 'inlinegraphic', 'sbr', 'void', 'varargs',
    'footnoteref', 'xref', 'anchor', 'beginpage',
    'videodata', 'audiodata', 'imagedata', 'textdata',
}


# -----------------------------------------------------------------------------
# 2.2 CONTENT MODEL DEFINITIONS
# -----------------------------------------------------------------------------

# Non-content elements (don't count as "valid content" for sections)
NON_CONTENT_ELEMENTS = {
    'title', 'titleabbrev', 'subtitle', 'anchor', 'indexterm', 'beginpage',
    'sect1info', 'sect2info', 'sect3info', 'sect4info', 'sect5info',
    'sectioninfo', 'chapterinfo', 'appendixinfo', 'prefaceinfo',
    'blockinfo', 'objectinfo',
}

# Valid content elements for sections (divcomponent.mix)
DIVCOMPONENT_MIX = {
    'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
    'screen', 'synopsis', 'address', 'blockquote', 'epigraph',
    'figure', 'informalfigure', 'table', 'informaltable',
    'example', 'informalexample', 'equation', 'informalequation',
    'procedure', 'sidebar', 'mediaobject', 'graphic',
    'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
    'segmentedlist', 'calloutlist', 'glosslist',
    'note', 'warning', 'caution', 'important', 'tip',
    'bridgehead', 'remark', 'highlights', 'abstract',
    'cmdsynopsis', 'funcsynopsis', 'classsynopsis',
    'revhistory', 'task', 'productionset', 'constraintdef',
    'msgset', 'qandaset', 'anchor', 'indexterm', 'beginpage',
}

# Valid content for list items (component.mix)
COMPONENT_MIX = DIVCOMPONENT_MIX.copy()

# Valid content for admonitions (admon.mix)
ADMON_MIX = {
    'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
    'screen', 'synopsis', 'address', 'blockquote', 'epigraph',
    'figure', 'informalfigure', 'table', 'informaltable',
    'example', 'informalexample', 'equation', 'informalequation',
    'procedure', 'sidebar', 'mediaobject', 'graphic',
    'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
    'segmentedlist', 'calloutlist', 'glosslist',
    'bridgehead', 'remark', 'highlights', 'abstract',
    'cmdsynopsis', 'funcsynopsis', 'classsynopsis',
    'revhistory', 'task', 'productionset', 'constraintdef',
    'anchor', 'indexterm', 'beginpage',
}

# Valid content for dedication and legalnotice (legalnotice.mix) - VERY LIMITED
# Note: anchor is NOT allowed in legalnotice.mix
LEGALNOTICE_MIX = {
    # list.class
    'glosslist', 'itemizedlist', 'orderedlist',
    # admon.class
    'caution', 'important', 'note', 'tip', 'warning',
    # linespecific.class
    'literallayout', 'programlisting', 'screen', 'synopsis', 'address',
    # para.class
    'formalpara', 'para', 'simpara',
    # blockquote
    'blockquote',
    # ndxterm.class
    'indexterm',
    # beginpage
    'beginpage',
}

# Elements NOT allowed in dedication: anchor, sect1, figure, table, sidebar,
# mediaobject, graphic, example, procedure, bridgehead


# -----------------------------------------------------------------------------
# 2.3 ELEMENT ORDERING REQUIREMENTS
# -----------------------------------------------------------------------------

# Element -> [(child_tag, required, position_constraint)]
ELEMENT_ORDERING = {
    'chapter': [
        ('beginpage', False, 'first'),
        ('chapterinfo', False, 'early'),
        ('title', True, 'after:beginpage'),
    ],
    'appendix': [
        ('beginpage', False, 'first'),
        ('appendixinfo', False, 'early'),
        ('title', True, 'after:beginpage'),
    ],
    'preface': [
        ('beginpage', False, 'first'),
        ('prefaceinfo', False, 'early'),
        ('title', True, 'after:beginpage'),
    ],
    'part': [
        ('beginpage', False, 'first'),
        ('partinfo', False, 'early'),
        ('title', True, 'after:beginpage'),
    ],
    # Dedication: no beginpage per DTD
    'dedication': [
        ('risinfo', False, 'first'),
        ('title', False, 'after:risinfo'),
        ('subtitle', False, 'after:title'),
        ('titleabbrev', False, 'after:subtitle'),
    ],
    # Sections: beginpage is in divcomponent.mix AFTER title, not before
    'sect1': [
        ('sect1info', False, 'first'),
        ('title', True, 'early'),
        ('subtitle', False, 'after:title'),
        ('titleabbrev', False, 'after:subtitle'),
    ],
    'sect2': [
        ('sect2info', False, 'first'),
        ('title', True, 'early'),
    ],
    'sect3': [
        ('sect3info', False, 'first'),
        ('title', True, 'early'),
    ],
    'sect4': [
        ('sect4info', False, 'first'),
        ('title', True, 'early'),
    ],
    'sect5': [
        ('sect5info', False, 'first'),
        ('title', True, 'early'),
    ],
    'figure': [
        ('blockinfo', False, 'first'),
        ('title', True, 'early'),
    ],
    'table': [
        ('blockinfo', False, 'first'),
        ('title', False, 'early'),
        ('tgroup', True, None),
    ],
    'tgroup': [
        ('colspec', False, 'first'),
        ('spanspec', False, 'after:colspec'),
        ('thead', False, 'before:tbody'),
        ('tfoot', False, 'before:tbody'),
        ('tbody', True, None),
    ],
}


# -----------------------------------------------------------------------------
# 2.4 COMMON DTD ERROR PATTERNS AND FIXES
# -----------------------------------------------------------------------------

DTD_ERROR_FIXES = {
    'bridgehead-in-indexdiv': {
        'description': 'bridgehead not allowed in indexdiv',
        'fix': 'Convert to para with emphasis, or move outside indexdiv',
    },
    'bridgehead-in-abstract': {
        'description': 'bridgehead not allowed in abstract',
        'fix': 'Convert to para with emphasis',
    },
    'para-in-table': {
        'description': 'para not allowed directly in row',
        'fix': 'Wrap para in entry element',
    },
    'sect1-in-indexdiv': {
        'description': 'sect1 not allowed in indexdiv',
        'fix': 'Remove sect1 wrapper, keep content',
    },
    'anchor-in-dedication': {
        'description': 'anchor not allowed in dedication (not in legalnotice.mix)',
        'fix': 'Wrap anchor in para element',
    },
    'nested-table': {
        'description': 'table cannot be nested in entry',
        'fix': 'Move inner table outside, or convert to informaltable',
    },
    'block-after-sect1': {
        'description': 'Block content after sect1 in chapter',
        'fix': 'Move content before sect1, or wrap in new sect1',
    },
    'empty-indexdiv': {
        'description': 'indexdiv requires at least one indexentry',
        'fix': 'Add indexentry or remove empty indexdiv',
    },
    'para-in-bridgehead': {
        'description': 'para not allowed in bridgehead',
        'fix': 'Extract text content only',
    },
    'para-in-subtitle': {
        'description': 'para not allowed in subtitle',
        'fix': 'Extract text content only',
    },
}


# -----------------------------------------------------------------------------
# 2.5 XSL ELEMENTS REQUIRING IDS
# -----------------------------------------------------------------------------

# Elements that MUST have IDs for XSL processing
XSL_ELEMENTS_REQUIRE_ID = frozenset({
    # Sections
    'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
    # Formal objects
    'table', 'figure', 'equation', 'informalfigure', 'informaltable',
    # Admonitions
    'note', 'sidebar', 'important', 'warning', 'caution', 'tip',
    # Footnotes
    'footnote',
    # Book components
    'chapter', 'appendix', 'preface', 'part', 'subpart',
    # Q&A/Bibliography/Glossary
    'qandaset', 'qandaentry', 'bibliomixed', 'biblioentry', 'glossentry',
})

# Elements where IDs are ignored by XSL
XSL_ELEMENTS_ID_IGNORED = frozenset({
    'anchor',           # Anchor template commented out in html.xsl
    'para',             # Para IDs not output
    'simpara',          # Same as para
    'title',            # Title IDs not output separately
    'bridgehead',       # Bridgehead IDs not output
    'beginpage',        # Pagebreak markers ignored
})


# =============================================================================
# PART 3: POST-PROCESSING RULES
# =============================================================================

# -----------------------------------------------------------------------------
# 3.1 SPACING FIX PATTERNS
# -----------------------------------------------------------------------------

SPACING_PATTERNS = {
    # Pattern: digit followed by uppercase letter starting a word
    # Example: "Chapter 1Introduction" -> "Chapter 1 Introduction"
    'digit_upper': re.compile(r'(\d)([A-Z][a-z])'),

    # Pattern: digit followed by all-caps word
    # Example: "Section 2DNA" -> "Section 2 DNA"
    'digit_allcaps': re.compile(r'(\d)([A-Z]{2,})'),

    # Pattern: lowercase letter followed by digit
    # Example: "Check1" -> "Check 1", "page5" -> "page 5"
    'lower_digit': re.compile(r'([a-z])(\d)'),

    # Pattern: lowercase followed by uppercase (merged words)
    # Example: "endChapter" -> "end Chapter"
    'lower_upper': re.compile(r'([a-z])([A-Z][a-z]{3,})'),

    # Pattern: lowercase followed by common document reference words
    # Example: "seeChapter 10" -> "see Chapter 10"
    'before_reference': re.compile(
        r'([a-z])((?:Chapter|Section|Figure|Table|Box|Page|Part|Appendix|Exhibit|'
        r'Example|Equation|Theorem|Lemma|Corollary|Definition|Proof|Remark|Note|'
        r'Case|Step|Item|Line|Verse|Column|Row|Panel|Entry|Volume|Article|Clause|'
        r'Paragraph|Graph|Chart|Diagram|Map|Photo|Image|Picture|Illustration|'
        r'Footnote|Endnote|Reference|Bibliography|Index|Glossary|Preface|'
        r'Introduction|Conclusion|Summary|Abstract|Acknowledgment|Dedication|'
        r'Foreword|Afterword|Prologue|Epilogue)\b)'
    ),

    # Pattern: multiple consecutive spaces
    'extra_spaces': re.compile(r'[ \t]{2,}'),

    # Pattern: space before punctuation
    'space_before_punct': re.compile(r'\s+([.,;:!?])'),

    # Pattern: no space after punctuation followed by letter
    'no_space_after_punct': re.compile(r'([.,;:!?])([A-Za-z])'),

    # Pattern: tabs to be replaced with single space
    'tabs': re.compile(r'\t+'),
}


def fix_spacing_in_text(text: str) -> Tuple[str, bool, List[str]]:
    """
    Fix spacing issues in text content.

    Applies:
    1. Convert tabs to single spaces
    2. Add missing space between digit and uppercase letter
    3. Add missing space between digit and all-caps word
    4. Add missing space between lowercase and digit
    5. Add missing space before reference words (Chapter, Section, etc.)
    6. Add missing space between lowercase and uppercase word
    7. Remove space before punctuation
    8. Add space after punctuation if missing
    9. Collapse multiple spaces

    Args:
        text: The text to fix

    Returns:
        Tuple of (fixed_text, was_modified, list of changes)
    """
    if not text:
        return (text, False, [])

    original = text
    changes = []

    # 1. Convert tabs to single spaces
    text = SPACING_PATTERNS['tabs'].sub(' ', text)
    if text != original:
        changes.append("Converted tabs to spaces")

    # 2. Fix missing space between digit and uppercase letter
    before = text
    text = SPACING_PATTERNS['digit_upper'].sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between digit and uppercase letter")

    # 3. Fix missing space between digit and all-caps word
    before = text
    text = SPACING_PATTERNS['digit_allcaps'].sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between digit and all-caps word")

    # 4. Fix missing space between lowercase letter and digit
    before = text
    text = SPACING_PATTERNS['lower_digit'].sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between lowercase and digit")

    # 5. Fix missing space before common reference words
    before = text
    text = SPACING_PATTERNS['before_reference'].sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space before reference word")

    # 6. Fix missing space between lowercase and uppercase
    before = text
    text = SPACING_PATTERNS['lower_upper'].sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between lowercase and uppercase word")

    # 7. Remove space before punctuation
    before = text
    text = SPACING_PATTERNS['space_before_punct'].sub(r'\1', text)
    if text != before:
        changes.append("Removed space before punctuation")

    # 8. Add space after punctuation if missing
    before = text
    text = SPACING_PATTERNS['no_space_after_punct'].sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space after punctuation")

    # 9. Collapse multiple spaces
    before = text
    text = SPACING_PATTERNS['extra_spaces'].sub(' ', text)
    if text != before:
        changes.append("Collapsed multiple spaces")

    # Strip leading/trailing whitespace
    text = text.strip()

    was_modified = text != original.strip()
    return (text, was_modified, changes)


# -----------------------------------------------------------------------------
# 3.2 LEADING NUMBER STRIPPING PATTERNS
# -----------------------------------------------------------------------------

# Pattern: Leading number at start of text content
# Matches: "1. Text", "1: Text", "1 Text", "1) Text", "(1) Text", "[1] Text"
# Also matches: "1.2.3 Text", "1.2.3. Text"
LEADING_NUMBER_PATTERN = re.compile(
    r'^[\s]*'                           # Optional leading whitespace
    r'(?:\(?\d+(?:\.\d+)*\)?'          # Number(s) with optional parens
    r'[\.\:\)\]\s]+)'                   # Followed by separator
    r'(.+)$',                           # Capture remaining text
    re.DOTALL
)

# Pattern: Simple leading number like "1.", "2.", "(1)", "[2]"
SIMPLE_LEADING_NUMBER = re.compile(
    r'^[\s]*'                           # Optional leading whitespace
    r'[\[\(]?'                          # Optional [ or (
    r'(\d+)'                            # The number
    r'[\]\)]?'                          # Optional ] or )
    r'[\.\:\)\]\s]*'                    # Separator punctuation
    r'(.*)$',                           # Rest of content
    re.DOTALL
)

# Pattern: Complex numbering like "1.2.3" or "1.2.3."
COMPLEX_NUMBERING = re.compile(
    r'^[\s]*'                           # Optional leading whitespace
    r'(\d+(?:\.\d+)+)'                  # Complex number like 1.2.3
    r'[\.\s]*'                          # Optional trailing dot/space
    r'(.*)$',                           # Rest of content
    re.DOTALL
)


def strip_leading_numbers(text: str) -> Tuple[str, bool, Optional[str]]:
    """
    Strip leading numbers from text (for auto-numbered elements).

    Used for:
    - orderedlist/listitem: numbers are auto-generated by XSL
    - bibliography/bibliomixed: numbers are auto-generated

    Args:
        text: The text to process

    Returns:
        Tuple of (stripped_text, was_modified, stripped_number)
    """
    if not text:
        return (text, False, None)

    # Try complex numbering first (1.2.3)
    match = COMPLEX_NUMBERING.match(text)
    if match:
        return (match.group(2).strip(), True, match.group(1))

    # Try simple leading number
    match = SIMPLE_LEADING_NUMBER.match(text)
    if match:
        return (match.group(2).strip(), True, match.group(1))

    # Try general leading number pattern
    match = LEADING_NUMBER_PATTERN.match(text)
    if match:
        return (match.group(1).strip(), True, 'leading_number')

    return (text, False, None)


# -----------------------------------------------------------------------------
# 3.3 PUBLISHER DETECTION PATTERNS
# -----------------------------------------------------------------------------

PUBLISHER_DETECTION_PATTERNS = {
    'wiley': ['wiley', 'john wiley', 'jossey-bass', 'wiley-blackwell', 'wiley-vch'],
    'springer': ['springer', 'springer nature', 'springer-verlag'],
    'elsevier': ['elsevier', 'academic press', 'saunders', 'mosby'],
    'pearson': ['pearson', 'prentice hall', 'addison-wesley'],
    'mcgraw-hill': ['mcgraw-hill', 'mcgraw hill', 'mcgrawhill'],
    'oxford': ['oxford university press', 'oxford'],
    'cambridge': ['cambridge university press', 'cambridge'],
    'taylor-francis': ['taylor & francis', 'taylor and francis', 'routledge', 'crc press'],
}


def detect_publisher(publisher_text: str) -> Optional[str]:
    """
    Detect publisher from metadata text.

    Args:
        publisher_text: Publisher name from metadata

    Returns:
        Normalized publisher config name or None
    """
    if not publisher_text:
        return None

    pub_lower = publisher_text.lower()
    for config_name, patterns in PUBLISHER_DETECTION_PATTERNS.items():
        for pattern in patterns:
            if pattern in pub_lower:
                return config_name
    return None


# -----------------------------------------------------------------------------
# 3.4 INLINE ELEMENTS FOR BOUNDARY SPACING
# -----------------------------------------------------------------------------

# Elements that might have missing space before them at boundaries
INLINE_ELEMENTS = {
    'link', 'xref', 'ulink', 'emphasis', 'phrase', 'literal',
    'citetitle', 'quote', 'foreignphrase', 'wordasword',
    'firstterm', 'glossterm', 'acronym', 'abbrev', 'citation',
    'subscript', 'superscript',
}


# -----------------------------------------------------------------------------
# 3.5 CONTENT PRESERVATION VALIDATION
# -----------------------------------------------------------------------------

def normalize_for_comparison(text: str) -> str:
    """
    Normalize text for content comparison by removing all whitespace.
    """
    if not text:
        return ""
    return re.sub(r'\s+', '', text)


def validate_content_preserved(original: str, modified: str,
                               operation: str = "transformation") -> Tuple[bool, Optional[str]]:
    """
    Validate that no content was lost during a text transformation.

    Args:
        original: The original text before transformation
        modified: The modified text after transformation
        operation: Description of the operation for error messages

    Returns:
        Tuple of (is_valid, error_message)
    """
    if not original:
        return (True, None)

    orig_normalized = normalize_for_comparison(original)
    mod_normalized = normalize_for_comparison(modified)

    if orig_normalized == mod_normalized:
        return (True, None)

    orig_len = len(orig_normalized)
    mod_len = len(mod_normalized)

    # Content was added (not lost) - OK for spacing fixes
    if mod_len >= orig_len:
        return (True, None)

    # Content was lost
    diff_len = orig_len - mod_len
    lost_percentage = (diff_len / orig_len) * 100 if orig_len > 0 else 0

    error_msg = (
        f"Content loss during {operation}: "
        f"{diff_len} characters lost ({lost_percentage:.1f}%)"
    )

    return (False, error_msg)


# =============================================================================
# PART 4: FILE NAMING CONVENTIONS
# =============================================================================

# XML File Naming Rules
FILE_NAMING = {
    'chapter': 'ch{:04d}.xml',       # ch0001.xml
    'preface': 'pr{:04d}.xml',       # pr0001.xml
    'appendix': 'ap{:04d}.xml',      # ap0001.xml
    'dedication': 'dd{:04d}.xml',    # dd0001.xml
    'part': 'pt{:04d}.xml',          # pt0001.xml
    'glossary': 'gl{:04d}.xml',      # gl0001.xml
    'bibliography': 'bi{:04d}.xml',  # bi0001.xml
    'index': 'in{:04d}.xml',         # in0001.xml
    'toc': 'toc.{}.xml',             # toc.{isbn}.xml
}


def generate_filename(content_type: str, index: int, isbn: str = None) -> str:
    """
    Generate a filename following naming conventions.

    Args:
        content_type: Type of content ('chapter', 'preface', etc.)
        index: 1-based index
        isbn: ISBN for TOC files

    Returns:
        Formatted filename
    """
    if content_type == 'toc':
        return FILE_NAMING['toc'].format(isbn or 'unknown')

    pattern = FILE_NAMING.get(content_type, FILE_NAMING['chapter'])
    return pattern.format(index)


# =============================================================================
# EXPORTED SUMMARY
# =============================================================================

__all__ = [
    # ID Generation
    'MAX_ID_LENGTH',
    'VALID_CHAPTER_PREFIXES',
    'VALID_ELEMENT_CODES',
    'XSL_RECOGNIZED_CODES',
    'ELEMENT_TYPE_TO_CODE',
    'DEFAULT_ELEMENT_CODE',
    'get_element_code',
    'SectionFormat',
    'generate_chapter_id',
    'generate_section_id',
    'generate_element_id',
    'next_available_sect1_id',
    'sanitize_id',
    'validate_id',
    'detect_id_type',
    'extract_chapter_from_id',
    'extract_section_hierarchy',

    # ID Patterns
    'CHAPTER_PATTERN',
    'SECTION_PATTERN',
    'ELEMENT_PATTERN',
    'GENERATED_ID_PATTERN',
    'CHAPTER_ID_PATTERN_SIMPLE',
    'SECTION_ID_PATTERN_SIMPLE',
    'ELEMENT_ID_PATTERN_SIMPLE',

    # DTD Validation
    'ELEMENTS_REQUIRING_CONTENT',
    'ELEMENTS_CAN_BE_EMPTY',
    'EMPTY_ELEMENTS',
    'NON_CONTENT_ELEMENTS',
    'DIVCOMPONENT_MIX',
    'COMPONENT_MIX',
    'ADMON_MIX',
    'LEGALNOTICE_MIX',
    'ELEMENT_ORDERING',
    'DTD_ERROR_FIXES',
    'XSL_ELEMENTS_REQUIRE_ID',
    'XSL_ELEMENTS_ID_IGNORED',

    # Post-Processing
    'SPACING_PATTERNS',
    'fix_spacing_in_text',
    'LEADING_NUMBER_PATTERN',
    'SIMPLE_LEADING_NUMBER',
    'COMPLEX_NUMBERING',
    'strip_leading_numbers',
    'PUBLISHER_DETECTION_PATTERNS',
    'detect_publisher',
    'INLINE_ELEMENTS',
    'normalize_for_comparison',
    'validate_content_preserved',

    # File Naming
    'FILE_NAMING',
    'generate_filename',
]
