#!/usr/bin/env python3
"""
Centralized ID Authority for EPUB to DocBook XML Conversion.

This module provides the SINGLE SOURCE OF TRUTH for all ID management:
- Authoritative definitions of valid prefixes, codes, and formats
- Strict validation with clear error messages
- Atomic chapter registry with cascading updates
- Linkend resolution with full audit trail

Architecture:
    IDAuthority (facade)
    ├── IDDefinitions (enums and constants)
    ├── IDParser (parse and validate IDs)
    ├── ChapterRegistry (atomic chapter/ID management)
    └── LinkendResolver (resolve linkends with audit trail)

Usage:
    from id_authority import get_authority

    authority = get_authority()
    authority.register_chapter("chapter1.xhtml", "ch0001")
    authority.map_id("ch0001", "Fig1", "ch0001s0001fg0001")
    resolved = authority.resolve_linkend("Fig1", "ch0001")

See docs/ID_AUTHORITY_ARCHITECTURE.md for complete documentation.
"""

import json
import logging
import re
import threading
from dataclasses import dataclass, field, asdict
from datetime import datetime
from enum import Enum
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple, Any, Callable

logger = logging.getLogger(__name__)


# ============================================================================
# AUTHORITATIVE DEFINITIONS - SINGLE SOURCE OF TRUTH
# ============================================================================

class ChapterPrefix(Enum):
    """
    Valid chapter type prefixes.

    These are the ONLY allowed 2-letter prefixes for chapter IDs.
    Each has a separate counter during conversion.
    """
    CHAPTER = "ch"
    APPENDIX = "ap"
    PREFACE = "pr"
    INDEX = "in"
    GLOSSARY = "gl"
    BIBLIOGRAPHY = "bi"
    DEDICATION = "dd"
    PART = "pt"
    SUBPART = "sp"
    TOC = "tc"
    COPYRIGHT = "cp"
    FRONTMATTER = "fm"
    BACKMATTER = "bm"


class ElementCode(Enum):
    """
    Valid element codes with XSL recognition status.

    XSL-recognized codes will resolve properly in link.ritt.xsl for
    popup/modal functionality. Non-recognized codes create basic links
    but won't have special handling.

    Format: (code, xsl_recognized, description)
    """
    # XSL-RECOGNIZED - These work with link.ritt.xsl popupLink
    FIGURE = ("fg", True, "Figures and informalfigures")
    TABLE = ("ta", True, "Tables and informaltables")
    BIBLIOGRAPHY = ("bib", True, "Bibliography entries (bibliomixed)")
    EQUATION = ("eq", True, "Equations and informalequations")
    GLOSSARY = ("gl", True, "Glossary entries")
    QA = ("qa", True, "Q&A entries (qandaset, qandaentry)")
    PROCEDURE = ("pr", True, "Procedures")
    VIDEO = ("vd", True, "Video content")
    ADMONITION = ("ad", True, "Admonitions (note, warning, tip, caution, important) and sidebars")

    # NOT XSL-RECOGNIZED - Basic links only, no popup/modal
    ANCHOR = ("a", False, "Generic anchors")
    LIST = ("l", False, "Lists")
    PARAGRAPH = ("p", False, "Paragraphs")
    EXAMPLE = ("ex", False, "Examples")
    FOOTNOTE = ("fn", False, "Footnotes")
    MEDIAOBJECT = ("mo", False, "Media objects")
    STEP = ("st", False, "Procedure steps")
    SUBSTEP = ("ss", False, "Procedure substeps")
    BLOCKQUOTE = ("bq", False, "Block quotes")
    TERM = ("tm", False, "Terms")
    INDEXTERM = ("ix", False, "Index terms")

    def __init__(self, code: str, xsl_recognized: bool, description: str):
        self._code = code
        self._xsl_recognized = xsl_recognized
        self._description = description

    @property
    def code(self) -> str:
        return self._code

    @property
    def xsl_recognized(self) -> bool:
        return self._xsl_recognized

    @property
    def description(self) -> str:
        return self._description


# Derived constants for quick lookup
VALID_CHAPTER_PREFIXES: Set[str] = {p.value for p in ChapterPrefix}
VALID_ELEMENT_CODES: Set[str] = {e.code for e in ElementCode}
XSL_RECOGNIZED_CODES: Set[str] = {e.code for e in ElementCode if e.xsl_recognized}
NON_XSL_CODES: Set[str] = {e.code for e in ElementCode if not e.xsl_recognized}

# Element type to code mapping - SINGLE SOURCE OF TRUTH
# All other modules should import this from id_authority
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
    'audio': 'ad',  # Audio uses admonition prefix
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
    # NOTE: Section elements (sect1-sect5) are intentionally NOT included here.
    # Section IDs must be generated using generate_section_id() or next_available_sect1_id(),
    # NOT the element code pattern. Using element codes like 's1' for sect1 produces
    # malformed IDs like ch0005s0001s10002 which break downstream XSL processing.
    # See commits 0fc63be, 4cd86f3 for detailed root cause analysis.
    'simplesect': 'sc',
}

# Alias for backward compatibility - other modules import this
ELEMENT_CODES = ELEMENT_TYPE_TO_CODE

# Default code for unknown element types
DEFAULT_ELEMENT_CODE = 'a'


def get_element_code(element_type: str) -> str:
    """
    Get the ID code for an element type.

    This is the canonical function for looking up element codes.
    All modules should use this instead of maintaining their own mappings.

    Args:
        element_type: DocBook element type name (e.g., 'figure', 'table')

    Returns:
        Two or three letter code (e.g., 'fg', 'ta', 'bib')
    """
    return ELEMENT_TYPE_TO_CODE.get(element_type, DEFAULT_ELEMENT_CODE)


class SectionFormat:
    """Section ID format constants."""
    # Level 1-2: 4-digit counters (s0001)
    # Level 3-5: 2-digit counters (s01) to stay within 25 char limit
    LEVEL_1_2_DIGITS = 4
    LEVEL_3_5_DIGITS = 2
    MAX_LEVEL = 5
    MAX_ID_LENGTH = 25


# ============================================================================
# COMPILED PATTERNS - Built from definitions, not hardcoded
# ============================================================================

def _build_chapter_pattern() -> re.Pattern:
    """Build chapter ID pattern from valid prefixes."""
    prefixes = "|".join(sorted(VALID_CHAPTER_PREFIXES, key=len, reverse=True))
    return re.compile(rf'^({prefixes})(\d{{4}})$')


def _build_section_pattern() -> re.Pattern:
    """
    Build section ID pattern supporting mixed digit formats.

    Valid formats:
    - ch0001s0001 (sect1)
    - ch0001s0001s0002 (sect2)
    - ch0001s0001s0001s01 (sect3)
    - ch0001s0001s0001s01s02 (sect4)
    - ch0001s0001s0001s01s01s03 (sect5)
    """
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


# Compile patterns at module load
CHAPTER_PATTERN = _build_chapter_pattern()
SECTION_PATTERN = _build_section_pattern()
ELEMENT_PATTERN = _build_element_pattern()

# Looser pattern for partial/malformed ID detection
GENERATED_ID_PATTERN = re.compile(
    rf'^[a-z]{{2}}\d{{4}}'  # Any 2-letter prefix + 4 digits
    rf'(?:s\d{{2,4}})*'      # Zero or more section parts
    rf'(?:[a-z]{{1,3}}\d+)?$'  # Optional element code + number
)


# ============================================================================
# ID PARSER
# ============================================================================

@dataclass
class ParsedID:
    """Parsed components of any valid ID."""
    raw: str
    id_type: str  # 'chapter', 'section', 'element', 'invalid'
    chapter_prefix: str
    chapter_number: int
    section_numbers: Tuple[Optional[int], ...] = ()  # (sect1, sect2, sect3, sect4, sect5)
    element_code: Optional[str] = None
    element_number: Optional[int] = None
    is_valid: bool = True
    validation_error: Optional[str] = None

    @property
    def chapter_id(self) -> str:
        """Get the chapter ID portion."""
        if not self.chapter_prefix:
            return ""
        return f"{self.chapter_prefix}{self.chapter_number:04d}"

    @property
    def section_id(self) -> Optional[str]:
        """Get the full section ID (chapter + sections)."""
        if not self.section_numbers or self.section_numbers[0] is None:
            return None

        result = self.chapter_id
        for i, num in enumerate(self.section_numbers):
            if num is None:
                break
            digits = SectionFormat.LEVEL_1_2_DIGITS if i < 2 else SectionFormat.LEVEL_3_5_DIGITS
            result += f"s{num:0{digits}d}"
        return result

    @property
    def section_depth(self) -> int:
        """Get the section nesting depth (1-5)."""
        if not self.section_numbers:
            return 0
        return sum(1 for n in self.section_numbers if n is not None)

    @property
    def parent_section_id(self) -> Optional[str]:
        """Get the parent section ID (one level up)."""
        if self.section_depth <= 1:
            return None

        result = self.chapter_id
        for i, num in enumerate(self.section_numbers[:-1]):
            if num is None:
                break
            digits = SectionFormat.LEVEL_1_2_DIGITS if i < 2 else SectionFormat.LEVEL_3_5_DIGITS
            result += f"s{num:0{digits}d}"
        return result

    @property
    def full_id(self) -> str:
        """Get the complete ID."""
        if self.element_code and self.section_id:
            return f"{self.section_id}{self.element_code}{self.element_number:04d}"
        elif self.section_id:
            return self.section_id
        return self.chapter_id

    @property
    def is_xsl_resolvable(self) -> bool:
        """Check if this ID's element code is XSL-recognized."""
        if not self.element_code:
            return True  # Chapters and sections are always resolvable
        return self.element_code in XSL_RECOGNIZED_CODES

    def to_dict(self) -> dict:
        """Convert to dictionary for serialization."""
        return {
            'raw': self.raw,
            'id_type': self.id_type,
            'chapter_prefix': self.chapter_prefix,
            'chapter_number': self.chapter_number,
            'section_numbers': self.section_numbers,
            'element_code': self.element_code,
            'element_number': self.element_number,
            'is_valid': self.is_valid,
            'validation_error': self.validation_error,
            'chapter_id': self.chapter_id,
            'section_id': self.section_id,
            'full_id': self.full_id,
        }


class IDParser:
    """Parse and validate IDs with strict rules."""

    @staticmethod
    def parse(id_string: str) -> ParsedID:
        """
        Parse any ID string into components.

        Args:
            id_string: The ID to parse

        Returns:
            ParsedID with all components extracted
        """
        if not id_string:
            return ParsedID(
                raw="",
                id_type='invalid',
                chapter_prefix='',
                chapter_number=0,
                is_valid=False,
                validation_error="Empty ID"
            )

        # Normalize to lowercase
        id_lower = id_string.lower()

        # Helper to validate section numbers (s0000 only allowed as first section)
        def _validate_sections(sections: tuple) -> Optional[str]:
            """Validate section numbers. Returns error message if invalid, None if valid."""
            for i, sec_num in enumerate(sections):
                if sec_num is None:
                    break
                # s0000 is only allowed as first section (chapter root)
                if sec_num == 0 and i > 0:
                    return f"Section number 0000 is only valid as first section (chapter root), found at position {i+1}"
            return None

        # Try element pattern first (most specific)
        match = ELEMENT_PATTERN.match(id_lower)
        if match:
            groups = match.groups()
            sections = tuple(
                int(g[1:]) if g else None
                for g in groups[2:7]
            )
            # Validate section numbers
            error = _validate_sections(sections)
            if error:
                return ParsedID(
                    raw=id_string,
                    id_type='invalid',
                    chapter_prefix='',
                    chapter_number=0,
                    is_valid=False,
                    validation_error=error
                )
            return ParsedID(
                raw=id_string,
                id_type='element',
                chapter_prefix=groups[0],
                chapter_number=int(groups[1]),
                section_numbers=sections,
                element_code=groups[7],
                element_number=int(groups[8])
            )

        # Try section pattern
        match = SECTION_PATTERN.match(id_lower)
        if match:
            groups = match.groups()
            sections = tuple(
                int(g[1:]) if g else None
                for g in groups[2:7]
            )
            # Validate section numbers
            error = _validate_sections(sections)
            if error:
                return ParsedID(
                    raw=id_string,
                    id_type='invalid',
                    chapter_prefix='',
                    chapter_number=0,
                    is_valid=False,
                    validation_error=error
                )
            return ParsedID(
                raw=id_string,
                id_type='section',
                chapter_prefix=groups[0],
                chapter_number=int(groups[1]),
                section_numbers=sections
            )

        # Try chapter pattern
        match = CHAPTER_PATTERN.match(id_lower)
        if match:
            return ParsedID(
                raw=id_string,
                id_type='chapter',
                chapter_prefix=match.group(1),
                chapter_number=int(match.group(2)),
                section_numbers=()
            )

        # Invalid ID - provide helpful error
        return ParsedID(
            raw=id_string,
            id_type='invalid',
            chapter_prefix='',
            chapter_number=0,
            is_valid=False,
            validation_error=IDParser._diagnose_error(id_string)
        )

    @staticmethod
    def _diagnose_error(id_string: str) -> str:
        """Provide helpful error message for invalid ID."""
        id_lower = id_string.lower()

        if len(id_string) < 6:
            return f"ID too short: {len(id_string)} chars (minimum 6 for chapter ID)"

        # Check prefix
        if len(id_string) >= 2:
            prefix = id_lower[:2]
            if prefix not in VALID_CHAPTER_PREFIXES:
                return f"Invalid chapter prefix '{prefix}'. Valid prefixes: {sorted(VALID_CHAPTER_PREFIXES)}"

        # Check for common issues
        if re.search(r'[A-Z]', id_string):
            return "ID contains uppercase characters (must be lowercase)"

        if re.search(r'[^a-z0-9]', id_string):
            invalid_chars = set(re.findall(r'[^a-z0-9]', id_string))
            return f"ID contains invalid characters: {invalid_chars} (only a-z, 0-9 allowed)"

        # s0000 is only valid as the first section segment (chapter root, before sect1)
        # e.g., ch0001s0000 is valid, but ch0001s0001s0000 is not
        if 's0000' in id_lower:
            # Allow s0000 only if it's the first section segment
            match = re.match(r'^[a-z]{2}\d{4}s0000', id_lower)
            if not match or id_lower.count('s0000') > 1:
                return "Section number s0000 is only valid as first section (chapter root)"

        # Check for looks-like-generated but malformed
        if GENERATED_ID_PATTERN.match(id_lower):
            # It matches loosely but not strictly
            if len(id_string) > SectionFormat.MAX_ID_LENGTH:
                return f"ID exceeds maximum length ({len(id_string)} > {SectionFormat.MAX_ID_LENGTH})"

            # Check element code if present
            elem_match = re.search(r'([a-z]{1,3})(\d+)$', id_lower)
            if elem_match:
                code = elem_match.group(1)
                if code not in VALID_ELEMENT_CODES:
                    return f"Invalid element code '{code}'. Valid codes: {sorted(VALID_ELEMENT_CODES)}"

        return f"ID format not recognized: '{id_string}'"

    @staticmethod
    def is_valid(id_string: str) -> bool:
        """Quick check if an ID is valid."""
        return IDParser.parse(id_string).is_valid

    @staticmethod
    def is_chapter_id(id_string: str) -> bool:
        """Check if ID is a chapter-level ID."""
        parsed = IDParser.parse(id_string)
        return parsed.is_valid and parsed.id_type == 'chapter'

    @staticmethod
    def is_section_id(id_string: str) -> bool:
        """Check if ID is a section-level ID."""
        parsed = IDParser.parse(id_string)
        return parsed.is_valid and parsed.id_type == 'section'

    @staticmethod
    def is_element_id(id_string: str) -> bool:
        """Check if ID is an element-level ID."""
        parsed = IDParser.parse(id_string)
        return parsed.is_valid and parsed.id_type == 'element'

    @staticmethod
    def extract_chapter_id(id_string: str) -> Optional[str]:
        """Extract chapter ID from any valid ID."""
        parsed = IDParser.parse(id_string)
        if parsed.is_valid:
            return parsed.chapter_id
        return None

    @staticmethod
    def extract_section_id(id_string: str) -> Optional[str]:
        """Extract section ID (including chapter) from any valid ID."""
        parsed = IDParser.parse(id_string)
        if parsed.is_valid:
            return parsed.section_id
        return None

    @staticmethod
    def get_element_code(element_type: str) -> str:
        """
        Get the element code for an element type.

        Args:
            element_type: Element type name (e.g., 'figure', 'table')

        Returns:
            Element code (e.g., 'fg', 'ta') or default 'a' for unknown types
        """
        return ELEMENT_TYPE_TO_CODE.get(element_type.lower(), DEFAULT_ELEMENT_CODE)

    @staticmethod
    def is_xsl_resolvable(id_string: str) -> bool:
        """Check if an ID's element code is XSL-recognized."""
        parsed = IDParser.parse(id_string)
        return parsed.is_xsl_resolvable


# ============================================================================
# ID STATE TRACKING
# ============================================================================

class IDState(Enum):
    """State of an ID through the pipeline."""
    PRESCANNED = "prescanned"      # Found in source EPUB during pre-scan
    REGISTERED = "registered"      # Chapter mapping registered
    MAPPED = "mapped"              # Source → Generated mapping created
    FINALIZED = "finalized"        # Final ID assigned (post-processing complete)
    DROPPED = "dropped"            # Intentionally dropped (pagebreak, invalid, etc.)


@dataclass
class IDRecord:
    """Complete record of an ID's lifecycle."""
    source_id: str
    source_file: str
    chapter_id: str
    element_type: str = ""
    generated_id: Optional[str] = None
    state: IDState = IDState.PRESCANNED
    drop_reason: Optional[str] = None
    history: List[str] = field(default_factory=list)
    created_at: str = field(default_factory=lambda: datetime.now().isoformat())

    def transition(self, new_state: IDState, details: str = "") -> None:
        """Record state transition with audit trail."""
        timestamp = datetime.now().isoformat()
        entry = f"[{timestamp}] {self.state.value} → {new_state.value}"
        if details:
            entry += f": {details}"
        self.history.append(entry)
        self.state = new_state

    def to_dict(self) -> dict:
        """Convert to dictionary for serialization."""
        return {
            'source_id': self.source_id,
            'source_file': self.source_file,
            'chapter_id': self.chapter_id,
            'element_type': self.element_type,
            'generated_id': self.generated_id,
            'state': self.state.value,
            'drop_reason': self.drop_reason,
            'history': self.history,
            'created_at': self.created_at,
        }

    @classmethod
    def from_dict(cls, data: dict) -> 'IDRecord':
        """Create from dictionary."""
        record = cls(
            source_id=data['source_id'],
            source_file=data['source_file'],
            chapter_id=data['chapter_id'],
            element_type=data.get('element_type', ''),
            generated_id=data.get('generated_id'),
            drop_reason=data.get('drop_reason'),
            history=data.get('history', []),
            created_at=data.get('created_at', datetime.now().isoformat()),
        )
        record.state = IDState(data.get('state', 'prescanned'))
        return record


class LinkendResolutionQuality(Enum):
    """
    Quality level of linkend resolution.

    Used to track how linkends were resolved and identify potential degradation.
    """
    EXACT = "exact"                    # Direct match to target ID
    MAPPED = "mapped"                  # Resolved via source→generated ID mapping
    CASE_INSENSITIVE = "case_insensitive"  # Resolved via case-insensitive match
    FUZZY = "fuzzy"                    # Resolved via fuzzy/Levenshtein matching
    DOWNGRADED_CHAPTER = "downgraded_chapter"  # Downgraded to chapter-level link
    DOWNGRADED_SECTION = "downgraded_section"  # Downgraded to section-level link
    RECORD_LOOKUP = "record_lookup"    # Resolved via ID record transformation lookup
    LOST = "lost"                      # Could not be resolved


@dataclass
class LinkendResolution:
    """Details of a single linkend resolution."""
    original_linkend: str
    resolved_id: Optional[str]
    quality: LinkendResolutionQuality
    strategy_details: str = ""

    @property
    def is_downgraded(self) -> bool:
        """True if resolution was downgraded to a parent element."""
        return self.quality in (
            LinkendResolutionQuality.DOWNGRADED_CHAPTER,
            LinkendResolutionQuality.DOWNGRADED_SECTION
        )

    @property
    def is_exact(self) -> bool:
        """True if resolution was exact or mapped (high confidence)."""
        return self.quality in (
            LinkendResolutionQuality.EXACT,
            LinkendResolutionQuality.MAPPED
        )


@dataclass
class LinkendValidationReport:
    """Report from linkend validation with quality tracking."""
    total_linkends: int = 0
    resolved: int = 0
    unresolved: List[Dict[str, str]] = field(default_factory=list)
    pointing_to_dropped: List[Dict[str, str]] = field(default_factory=list)
    converted_to_phrase: int = 0

    # Quality tracking
    resolutions: List[LinkendResolution] = field(default_factory=list)
    quality_counts: Dict[str, int] = field(default_factory=dict)

    def add_resolution(self, resolution: LinkendResolution) -> None:
        """Add a resolution and update quality counts."""
        self.resolutions.append(resolution)
        quality_name = resolution.quality.value
        self.quality_counts[quality_name] = self.quality_counts.get(quality_name, 0) + 1

    @property
    def exact_resolutions(self) -> int:
        """Count of exact/high-confidence resolutions."""
        return (
            self.quality_counts.get(LinkendResolutionQuality.EXACT.value, 0) +
            self.quality_counts.get(LinkendResolutionQuality.MAPPED.value, 0)
        )

    @property
    def downgraded_resolutions(self) -> int:
        """Count of downgraded resolutions (linked to parent instead of target)."""
        return (
            self.quality_counts.get(LinkendResolutionQuality.DOWNGRADED_CHAPTER.value, 0) +
            self.quality_counts.get(LinkendResolutionQuality.DOWNGRADED_SECTION.value, 0)
        )

    @property
    def lost_resolutions(self) -> int:
        """Count of lost/unresolved linkends."""
        return self.quality_counts.get(LinkendResolutionQuality.LOST.value, 0)

    def is_valid(self) -> bool:
        return len(self.unresolved) == 0 and len(self.pointing_to_dropped) == 0

    def get_quality_summary(self) -> str:
        """Get a human-readable quality summary."""
        total = sum(self.quality_counts.values())
        if total == 0:
            return "No linkends processed"

        exact = self.exact_resolutions
        downgraded = self.downgraded_resolutions
        lost = self.lost_resolutions

        exact_pct = (exact / total * 100) if total > 0 else 0
        downgraded_pct = (downgraded / total * 100) if total > 0 else 0
        lost_pct = (lost / total * 100) if total > 0 else 0

        return (
            f"Linkend Resolution Quality: {total} total | "
            f"EXACT: {exact} ({exact_pct:.1f}%) | "
            f"DOWNGRADED: {downgraded} ({downgraded_pct:.1f}%) | "
            f"LOST: {lost} ({lost_pct:.1f}%)"
        )


@dataclass
class SourceID:
    """Information about an ID found in source EPUB during pre-scan."""
    id_value: str           # The original ID value (e.g., "CR1", "Fig1")
    element_type: str       # figure, table, bibliography, section, anchor, etc.
    element_tag: str        # Original HTML tag (figure, li, div, section, etc.)
    css_class: str = ""     # CSS class if present
    epub_type: str = ""     # epub:type attribute if present
    context: str = ""       # Parent element context
    mapped: bool = False    # Whether this ID has been mapped to XML
    xml_id: Optional[str] = None  # The generated XML ID
    dropped: bool = False   # Whether intentionally dropped
    drop_reason: str = ""   # Reason for dropping

    def to_dict(self) -> dict:
        return asdict(self)

    @classmethod
    def from_dict(cls, data: dict) -> 'SourceID':
        return cls(**data)


# Tag name to element type mapping for prescan
TAG_TO_TYPE: Dict[str, str] = {
    'figure': 'figure',
    'table': 'table',
    'img': 'figure',
    'video': 'video',
    'audio': 'audio',
    'aside': 'sidebar',
    'blockquote': 'blockquote',
    'section': 'section',
    'article': 'section',
    'nav': 'navigation',
    'ol': 'list',
    'ul': 'list',
    'dl': 'list',
    'li': 'listitem',
    'p': 'para',
    'h1': 'section',
    'h2': 'section',
    'h3': 'section',
    'h4': 'section',
    'h5': 'section',
    'h6': 'section',
}

# Patterns for detecting element types from class/epub:type
ELEMENT_TYPE_PATTERNS: Dict[str, List[str]] = {
    'figure': ['figure', 'fig', 'image', 'illustration'],
    'table': ['table', 'tbl'],
    'bibliography': ['biblio', 'reference', 'citation', 'bib-'],
    'footnote': ['footnote', 'fn', 'note'],
    'sidebar': ['sidebar', 'aside', 'box', 'callout'],
    'glossary': ['glossary', 'gloss', 'definition'],
    'index': ['index', 'idx'],
    'section': ['section', 'chapter', 'part', 'sect'],
    'equation': ['equation', 'formula', 'math'],
}


# ============================================================================
# CHAPTER REGISTRY
# ============================================================================

@dataclass
class ChapterMapping:
    """Mapping between EPUB file and chapter."""
    epub_file: str
    chapter_id: str
    xml_file: str
    element_type: str = "chapter"  # chapter, appendix, index, etc.
    created_at: str = field(default_factory=lambda: datetime.now().isoformat())

    def to_dict(self) -> dict:
        return asdict(self)


class ChapterRegistry:
    """
    Central registry for chapter ID management with atomic updates.

    Ensures that when a chapter ID changes (e.g., ch0007 → in0001),
    ALL dependent IDs and mappings are updated atomically.
    """

    def __init__(self):
        self._lock = threading.RLock()

        # Primary mappings
        self._epub_to_chapter: Dict[str, ChapterMapping] = {}  # epub_file → mapping
        self._chapter_to_epub: Dict[str, str] = {}  # chapter_id → epub_file

        # ID records: (chapter_id, source_id) → IDRecord
        self._id_records: Dict[Tuple[str, str], IDRecord] = {}

        # Quick lookup: (chapter_id, source_id) → generated_id
        self._id_lookup: Dict[Tuple[str, str], str] = {}

        # Valid generated IDs in final output
        self._valid_ids: Set[str] = set()

        # Listeners for cascading updates
        self._update_listeners: List[Callable[[str, str], None]] = []

        # Statistics
        self._stats = {
            'chapters_registered': 0,
            'ids_mapped': 0,
            'ids_dropped': 0,
            'chapter_updates': 0,
        }

    def register_chapter(self, epub_file: str, chapter_id: str,
                        xml_file: str = None, element_type: str = "chapter") -> None:
        """
        Register or update chapter mapping.

        If chapter_id changes for an existing epub_file, cascades updates
        to all dependent ID records.

        Args:
            epub_file: EPUB filename (e.g., "chapter1.xhtml")
            chapter_id: Chapter ID (e.g., "ch0001")
            xml_file: Output XML filename (defaults to "{chapter_id}.xml")
            element_type: Element type (chapter, appendix, index, etc.)
        """
        with self._lock:
            if xml_file is None:
                xml_file = f"{chapter_id}.xml"

            old_mapping = self._epub_to_chapter.get(epub_file)
            old_chapter_id = old_mapping.chapter_id if old_mapping else None

            if old_chapter_id and old_chapter_id != chapter_id:
                # Chapter ID is changing - cascade updates
                self._cascade_chapter_update(epub_file, old_chapter_id, chapter_id)
                self._stats['chapter_updates'] += 1

            # Create/update mapping
            mapping = ChapterMapping(
                epub_file=epub_file,
                chapter_id=chapter_id,
                xml_file=xml_file,
                element_type=element_type,
            )
            self._epub_to_chapter[epub_file] = mapping
            self._chapter_to_epub[chapter_id] = epub_file

            # Remove stale reverse mapping
            if old_chapter_id and old_chapter_id != chapter_id:
                if old_chapter_id in self._chapter_to_epub:
                    del self._chapter_to_epub[old_chapter_id]

            if not old_mapping:
                self._stats['chapters_registered'] += 1

            logger.debug(f"Registered chapter: {epub_file} → {chapter_id} ({element_type})")

    def _cascade_chapter_update(self, epub_file: str, old_id: str, new_id: str) -> None:
        """Cascade chapter ID change to all dependent records."""
        logger.info(f"Cascading chapter ID update: {old_id} → {new_id} for {epub_file}")

        # Find all ID records for old chapter
        records_to_update = [
            (key, record) for key, record in self._id_records.items()
            if key[0] == old_id
        ]

        for (_, source_id), record in records_to_update:
            # Remove old key
            del self._id_records[(old_id, source_id)]
            if (old_id, source_id) in self._id_lookup:
                old_generated = self._id_lookup.pop((old_id, source_id))
            else:
                old_generated = None

            # Update record
            record.chapter_id = new_id
            if record.generated_id and record.generated_id.startswith(old_id):
                # Update generated ID prefix
                old_gen = record.generated_id
                record.generated_id = new_id + record.generated_id[len(old_id):]
                record.transition(
                    record.state,
                    f"Chapter ID changed: {old_id}→{new_id}, Generated: {old_gen}→{record.generated_id}"
                )

                # Update lookup
                self._id_lookup[(new_id, source_id)] = record.generated_id

                # Update valid_ids set
                if old_gen in self._valid_ids:
                    self._valid_ids.discard(old_gen)
                    self._valid_ids.add(record.generated_id)

            # Add with new key
            self._id_records[(new_id, source_id)] = record

        # Notify listeners
        for listener in self._update_listeners:
            try:
                listener(old_id, new_id)
            except Exception as e:
                logger.error(f"Update listener error: {e}")

    def get_chapter_id(self, epub_file: str) -> Optional[str]:
        """Get chapter ID for an EPUB file."""
        with self._lock:
            mapping = self._epub_to_chapter.get(epub_file)
            if mapping:
                return mapping.chapter_id

            # Try basename
            basename = Path(epub_file).name
            if basename != epub_file:
                mapping = self._epub_to_chapter.get(basename)
                if mapping:
                    return mapping.chapter_id

            # Try without leading path components
            normalized = epub_file.lstrip('./')
            if normalized != epub_file:
                mapping = self._epub_to_chapter.get(normalized)
                if mapping:
                    return mapping.chapter_id

            return None

    def get_epub_file(self, chapter_id: str) -> Optional[str]:
        """Get EPUB file for a chapter ID."""
        with self._lock:
            return self._chapter_to_epub.get(chapter_id)

    def get_chapter_mapping(self, epub_file: str) -> Optional[ChapterMapping]:
        """Get full chapter mapping for an EPUB file."""
        with self._lock:
            return self._epub_to_chapter.get(epub_file)

    def map_id(self, chapter_id: str, source_id: str, generated_id: str,
               element_type: str = "", source_file: str = "") -> None:
        """
        Map a source ID to its generated ID.

        Args:
            chapter_id: Chapter ID
            source_id: Original source ID from EPUB
            generated_id: Generated XML ID
            element_type: Element type (figure, table, etc.)
            source_file: Source EPUB filename
        """
        with self._lock:
            key = (chapter_id, source_id)

            if key in self._id_records:
                record = self._id_records[key]
                old_gen = record.generated_id
                record.generated_id = generated_id
                record.element_type = element_type or record.element_type
                record.transition(IDState.MAPPED, f"{source_id} → {generated_id}")
            else:
                record = IDRecord(
                    source_id=source_id,
                    source_file=source_file,
                    chapter_id=chapter_id,
                    element_type=element_type,
                    generated_id=generated_id,
                    state=IDState.MAPPED,
                )
                record.history.append(f"[{datetime.now().isoformat()}] Mapped: {source_id} → {generated_id}")
                self._id_records[key] = record

            # Update lookup
            self._id_lookup[key] = generated_id
            self._stats['ids_mapped'] += 1

            logger.debug(f"Mapped ID: {chapter_id}:{source_id} → {generated_id}")

    def mark_dropped(self, chapter_id: str, source_id: str, reason: str,
                     element_type: str = "", source_file: str = "") -> None:
        """
        Mark an ID as intentionally dropped.

        Args:
            chapter_id: Chapter ID
            source_id: Source ID being dropped
            reason: Reason for dropping
            element_type: Element type (figure, table, etc.)
            source_file: Source EPUB filename
        """
        with self._lock:
            key = (chapter_id, source_id)

            if key in self._id_records:
                record = self._id_records[key]
                record.drop_reason = reason
                record.element_type = element_type or record.element_type
                record.transition(IDState.DROPPED, reason)
            else:
                record = IDRecord(
                    source_id=source_id,
                    source_file=source_file,
                    chapter_id=chapter_id,
                    element_type=element_type,
                    state=IDState.DROPPED,
                    drop_reason=reason,
                )
                record.history.append(f"[{datetime.now().isoformat()}] Dropped: {reason}")
                self._id_records[key] = record

            self._stats['ids_dropped'] += 1
            logger.debug(f"Dropped ID: {chapter_id}:{source_id} ({element_type}) - {reason}")

    def is_dropped(self, chapter_id: str, source_id: str) -> bool:
        """Check if an ID is marked as dropped."""
        with self._lock:
            key = (chapter_id, source_id)
            record = self._id_records.get(key)
            return record is not None and record.state == IDState.DROPPED

    def get_drop_reason(self, chapter_id: str, source_id: str) -> Optional[str]:
        """Get the drop reason for an ID."""
        with self._lock:
            key = (chapter_id, source_id)
            record = self._id_records.get(key)
            if record and record.state == IDState.DROPPED:
                return record.drop_reason
            return None

    def resolve(self, chapter_id: str, source_id: str) -> Optional[str]:
        """
        Resolve source ID to generated ID.

        Args:
            chapter_id: Chapter ID to look in
            source_id: Source ID to resolve

        Returns:
            Generated ID or None if not found/dropped
        """
        with self._lock:
            key = (chapter_id, source_id)

            # Check if dropped
            record = self._id_records.get(key)
            if record and record.state == IDState.DROPPED:
                return None

            # Return from lookup
            return self._id_lookup.get(key)

    def get_record(self, chapter_id: str, source_id: str) -> Optional[IDRecord]:
        """Get the full ID record."""
        with self._lock:
            key = (chapter_id, source_id)
            return self._id_records.get(key)

    def get_audit_trail(self, chapter_id: str, source_id: str) -> List[str]:
        """Get audit trail for an ID."""
        with self._lock:
            key = (chapter_id, source_id)
            record = self._id_records.get(key)
            return record.history.copy() if record else []

    def register_valid_id(self, generated_id: str) -> None:
        """Register a valid ID that exists in the final output."""
        with self._lock:
            self._valid_ids.add(generated_id)

    def is_valid_id(self, generated_id: str) -> bool:
        """Check if a generated ID exists in the final output."""
        with self._lock:
            return generated_id in self._valid_ids

    @property
    def valid_ids(self) -> Set[str]:
        """Get a copy of all valid IDs in the final output."""
        with self._lock:
            return self._valid_ids.copy()

    def build_valid_ids_from_xml(self, root_element) -> int:
        """
        Build valid ID cache from XML tree.

        Args:
            root_element: lxml root element

        Returns:
            Number of IDs found
        """
        with self._lock:
            self._valid_ids.clear()
            for elem in root_element.iter():
                elem_id = elem.get('id')
                if elem_id:
                    self._valid_ids.add(elem_id)
            return len(self._valid_ids)

    def add_update_listener(self, listener: Callable[[str, str], None]) -> None:
        """Add listener for chapter ID changes."""
        self._update_listeners.append(listener)

    def remove_update_listener(self, listener: Callable[[str, str], None]) -> None:
        """Remove a listener."""
        if listener in self._update_listeners:
            self._update_listeners.remove(listener)

    def get_stats(self) -> dict:
        """Get registry statistics."""
        with self._lock:
            return {
                **self._stats,
                'total_records': len(self._id_records),
                'valid_ids': len(self._valid_ids),
            }

    def reset(self) -> None:
        """Reset all state for a new conversion."""
        with self._lock:
            self._epub_to_chapter.clear()
            self._chapter_to_epub.clear()
            self._id_records.clear()
            self._id_lookup.clear()
            self._valid_ids.clear()
            self._stats = {
                'chapters_registered': 0,
                'ids_mapped': 0,
                'ids_dropped': 0,
                'chapter_updates': 0,
            }
            logger.debug("Chapter registry reset")

    def apply_exact_source_id_mappings(self, root_elem) -> int:
        """
        Update linkend attributes using exact source ID mappings only.

        This performs a strict 1:1 mapping from source IDs observed in the EPUB
        to generated XML IDs. No regex, normalization, or heuristic matching is used.
        If a source ID maps to multiple XML IDs, it is treated as ambiguous and skipped.

        Args:
            root_elem: lxml root element

        Returns:
            Number of linkends updated
        """
        with self._lock:
            # Collect all actual IDs in the document for quick validity checks
            actual_ids: Set[str] = set()
            for elem in root_elem.iter():
                elem_id = elem.get('id')
                if elem_id:
                    actual_ids.add(elem_id)

            # Build unique source_id -> xml_id map (skip ambiguous source IDs)
            source_id_map: Dict[str, str] = {}
            duplicates: Set[str] = set()
            for key, gen_id in self._id_lookup.items():
                source_id = key[1]  # (chapter_id, source_id) tuple
                if source_id in source_id_map and source_id_map[source_id] != gen_id:
                    duplicates.add(source_id)
                else:
                    source_id_map[source_id] = gen_id

            for source_id in duplicates:
                source_id_map.pop(source_id, None)

            updated = 0
            for elem in root_elem.iter():
                linkend = elem.get('linkend')
                if not linkend or linkend in actual_ids:
                    continue

                mapped = source_id_map.get(linkend)
                if mapped and mapped in actual_ids:
                    elem.set('linkend', mapped)
                    updated += 1

            return updated

    def _try_fallback_resolution(self, linkend: str, actual_ids: Set[str],
                                  source_id_map: Dict[str, str]) -> Tuple[Optional[str], LinkendResolutionQuality, str]:
        """
        Try multiple fallback strategies to resolve an invalid linkend.

        Resolution strategies (in order):
        1. Direct source ID → generated ID mapping
        2. Case-insensitive source ID matching
        3. Fuzzy matching (Levenshtein distance ≤ 2 for short IDs)
        4. Chapter-level fallback (extract chapter prefix, link to chapter)
        5. Section-level fallback (find parent section ID)
        6. ID record transformation lookup

        Args:
            linkend: The linkend value to resolve
            actual_ids: Set of valid IDs in the document
            source_id_map: Mapping of source IDs to generated IDs

        Returns:
            Tuple of (resolved_id, quality, strategy_details)
            - resolved_id: The resolved ID or None
            - quality: LinkendResolutionQuality enum value
            - strategy_details: Human-readable description of the resolution
        """
        # Strategy 1: Direct source ID mapping
        if linkend in source_id_map:
            mapped = source_id_map[linkend]
            if mapped in actual_ids:
                details = f"Direct mapping: {linkend} → {mapped}"
                logger.debug(f"Fallback resolved '{linkend}' via direct mapping → '{mapped}'")
                return mapped, LinkendResolutionQuality.MAPPED, details

        # Strategy 2: Case-insensitive source ID matching
        linkend_lower = linkend.lower()
        for source_id, gen_id in source_id_map.items():
            if source_id.lower() == linkend_lower and gen_id in actual_ids:
                details = f"Case-insensitive: {linkend} (via {source_id}) → {gen_id}"
                logger.debug(f"Fallback resolved '{linkend}' via case-insensitive → '{gen_id}'")
                return gen_id, LinkendResolutionQuality.CASE_INSENSITIVE, details

        # Strategy 3: Fuzzy matching for short IDs (avoid expensive computation for long IDs)
        # CRITICAL: Only fuzzy match within the same chapter to prevent cross-chapter errors
        # e.g., ch0019s0001s10003 should NOT match ch0014s0001s10003 (different chapter!)
        if len(linkend) <= 20:
            best_match = None
            best_distance = 3  # Max Levenshtein distance to consider

            # Extract chapter prefix to restrict fuzzy matching to same chapter
            linkend_chapter_match = re.match(r'^([a-z]{2}\d{4})', linkend)
            linkend_chapter_prefix = linkend_chapter_match.group(1) if linkend_chapter_match else None

            for actual_id in actual_ids:
                if len(actual_id) <= 30:  # Only compare with reasonably sized IDs
                    # If linkend has a chapter prefix, only fuzzy match within same chapter
                    if linkend_chapter_prefix:
                        actual_chapter_match = re.match(r'^([a-z]{2}\d{4})', actual_id)
                        if actual_chapter_match:
                            actual_chapter_prefix = actual_chapter_match.group(1)
                            if actual_chapter_prefix != linkend_chapter_prefix:
                                # Skip IDs from different chapters - prevents dangerous cross-chapter matching
                                continue

                    distance = self._levenshtein_distance(linkend, actual_id)
                    if distance < best_distance:
                        best_distance = distance
                        best_match = actual_id

            if best_match and best_distance <= 2:
                details = f"Fuzzy match (distance={best_distance}): {linkend} → {best_match}"
                logger.debug(f"Fallback resolved '{linkend}' via fuzzy match → '{best_match}' (distance={best_distance})")
                return best_match, LinkendResolutionQuality.FUZZY, details

        # Strategy 4: Chapter-level fallback (DOWNGRADE WARNING)
        # If linkend looks like an element ID (e.g., "ch0001s0001fg0001"), try the chapter
        chapter_match = re.match(r'^([a-z]{2}\d{4})', linkend)
        if chapter_match:
            chapter_id = chapter_match.group(1)
            if chapter_id in actual_ids:
                details = f"DOWNGRADED to chapter: {linkend} → {chapter_id} (original target lost)"
                logger.warning(f"Linkend '{linkend}' DOWNGRADED to chapter-level '{chapter_id}' - original target not found")
                return chapter_id, LinkendResolutionQuality.DOWNGRADED_CHAPTER, details

        # Strategy 5: Section-level fallback (DOWNGRADE WARNING)
        # e.g., "ch0001s0001fg0001" → try "ch0001s0001"
        section_match = re.match(r'^([a-z]{2}\d{4}s\d{4})', linkend)
        if section_match:
            section_id = section_match.group(1)
            if section_id in actual_ids:
                details = f"DOWNGRADED to section: {linkend} → {section_id} (original target lost)"
                logger.warning(f"Linkend '{linkend}' DOWNGRADED to section-level '{section_id}' - original target not found")
                return section_id, LinkendResolutionQuality.DOWNGRADED_SECTION, details

        # Strategy 6: Check ID records for source ID that may have been transformed
        # CRITICAL: Scope the search to the same chapter to prevent cross-chapter errors
        # e.g., c04-fig-0001 in ch0008 should NOT match c04-fig-0001 in ch0009
        linkend_chapter_match = re.match(r'^([a-z]{2}\d{4})', linkend)
        target_chapter_prefix = linkend_chapter_match.group(1) if linkend_chapter_match else None

        for (chapter_id, source_id), record in self._id_records.items():
            if record.state != IDState.DROPPED:
                # If linkend has a chapter prefix, only search within that chapter
                if target_chapter_prefix and chapter_id != target_chapter_prefix:
                    continue

                # Check if linkend matches source_id with common transformations
                if self._ids_match_fuzzy(linkend, source_id):
                    gen_id = record.generated_id
                    if gen_id and gen_id in actual_ids:
                        # Additional safety: verify generated ID is in the expected chapter
                        if target_chapter_prefix:
                            gen_chapter_match = re.match(r'^([a-z]{2}\d{4})', gen_id)
                            if gen_chapter_match and gen_chapter_match.group(1) != target_chapter_prefix:
                                # Skip - this would be a cross-chapter match
                                logger.warning(
                                    f"Strategy 6: Rejected cross-chapter match {linkend} → {gen_id} "
                                    f"(expected {target_chapter_prefix})"
                                )
                                continue
                        details = f"Record lookup: {linkend} (via source {source_id}) → {gen_id}"
                        logger.debug(f"Fallback resolved '{linkend}' via record lookup → '{gen_id}'")
                        return gen_id, LinkendResolutionQuality.RECORD_LOOKUP, details

        # All strategies failed
        logger.debug(f"All fallback strategies failed for linkend '{linkend}'")
        return None, LinkendResolutionQuality.LOST, f"No resolution found for: {linkend}"

    def _levenshtein_distance(self, s1: str, s2: str) -> int:
        """Calculate Levenshtein distance between two strings."""
        if len(s1) < len(s2):
            return self._levenshtein_distance(s2, s1)

        if len(s2) == 0:
            return len(s1)

        previous_row = range(len(s2) + 1)
        for i, c1 in enumerate(s1):
            current_row = [i + 1]
            for j, c2 in enumerate(s2):
                insertions = previous_row[j + 1] + 1
                deletions = current_row[j] + 1
                substitutions = previous_row[j] + (c1 != c2)
                current_row.append(min(insertions, deletions, substitutions))
            previous_row = current_row

        return previous_row[-1]

    def _ids_match_fuzzy(self, id1: str, id2: str) -> bool:
        """Check if two IDs match with common transformations."""
        # Normalize both IDs
        n1 = re.sub(r'[_\-\s]', '', id1.lower())
        n2 = re.sub(r'[_\-\s]', '', id2.lower())

        if n1 == n2:
            return True

        # Check if one is a prefix of the other
        if n1.startswith(n2) or n2.startswith(n1):
            return True

        # Check common HTML ID transformations
        # e.g., "sec-1.2" vs "sec12", "fig_01" vs "fig01"
        n1_clean = re.sub(r'[^a-z0-9]', '', n1)
        n2_clean = re.sub(r'[^a-z0-9]', '', n2)

        return n1_clean == n2_clean

    def validate_linkends_in_document(self, root_elem) -> LinkendValidationReport:
        """
        Validate all linkend attributes in the document and fix invalid ones.

        Resolution strategy:
        1. Check if linkend exists in actual IDs (direct match)
        2. Try fallback resolution strategies (source ID mapping, fuzzy match, etc.)
        3. Only if all strategies fail:
           - Links pointing to dropped IDs are converted to phrase elements
           - Links with unresolved linkends are converted to phrase elements
        4. Empty/self-closing anchors are removed

        Args:
            root_elem: Root element of the XML document

        Returns:
            LinkendValidationReport with statistics and issues
        """
        # Import etree locally to avoid circular import
        from lxml import etree

        with self._lock:
            report = LinkendValidationReport()

            # Collect all actual IDs in the document
            actual_ids = set()
            for elem in root_elem.iter():
                elem_id = elem.get('id')
                if elem_id:
                    actual_ids.add(elem_id)

            # Build source ID → generated ID map for fallback resolution
            source_id_map: Dict[str, str] = {}
            for key, gen_id in self._id_lookup.items():
                source_id = key[1]  # (chapter_id, source_id) tuple
                # Keep first mapping (don't overwrite with duplicates)
                if source_id not in source_id_map:
                    source_id_map[source_id] = gen_id

            # Track fallback resolutions for reporting
            fallback_resolved = 0
            downgraded_count = 0

            # Process all elements with linkend attributes
            for elem in list(root_elem.iter()):
                linkend = elem.get('linkend')
                if not linkend:
                    continue

                report.total_linkends += 1

                # Check if linkend target exists directly
                if linkend in actual_ids:
                    report.resolved += 1
                    # Track as exact resolution
                    resolution = LinkendResolution(
                        original_linkend=linkend,
                        resolved_id=linkend,
                        quality=LinkendResolutionQuality.EXACT,
                        strategy_details=f"Direct match: {linkend}"
                    )
                    report.add_resolution(resolution)
                    continue

                # Try fallback resolution strategies before giving up
                fallback_id, quality, details = self._try_fallback_resolution(
                    linkend, actual_ids, source_id_map
                )

                if fallback_id and quality != LinkendResolutionQuality.LOST:
                    # Update the linkend to the resolved ID
                    elem.set('linkend', fallback_id)
                    report.resolved += 1
                    fallback_resolved += 1

                    # Track quality
                    resolution = LinkendResolution(
                        original_linkend=linkend,
                        resolved_id=fallback_id,
                        quality=quality,
                        strategy_details=details
                    )
                    report.add_resolution(resolution)

                    # Count downgrades separately for warnings
                    if resolution.is_downgraded:
                        downgraded_count += 1

                    logger.info(f"Resolved linkend via fallback: '{linkend}' → '{fallback_id}' ({quality.value})")
                    continue

                # Check if it's a dropped ID (for reporting purposes)
                is_dropped = False
                drop_reason = None
                for key, record in self._id_records.items():
                    if record.state == IDState.DROPPED:
                        if key[1] == linkend or record.generated_id == linkend:
                            is_dropped = True
                            drop_reason = record.drop_reason
                            report.pointing_to_dropped.append({
                                'linkend': linkend,
                                'element': elem.tag,
                                'reason': record.drop_reason or 'unknown'
                            })
                            break

                if not is_dropped:
                    report.unresolved.append({
                        'linkend': linkend,
                        'element': elem.tag
                    })

                # Track as lost resolution
                resolution = LinkendResolution(
                    original_linkend=linkend,
                    resolved_id=None,
                    quality=LinkendResolutionQuality.LOST,
                    strategy_details=details if details else f"Could not resolve: {linkend}"
                )
                report.add_resolution(resolution)

                # All resolution strategies failed - convert to phrase (or remove if xref)
                self._convert_invalid_link_to_phrase(elem, linkend)
                report.converted_to_phrase += 1

            # Remove empty/self-closing anchors
            self._remove_empty_anchors(root_elem)

            # Log summary with quality information
            logger.info(f"Linkend validation: {report.resolved} resolved "
                       f"({fallback_resolved} via fallback), "
                       f"{report.converted_to_phrase} converted/removed")

            # Log quality summary
            quality_summary = report.get_quality_summary()
            logger.info(quality_summary)

            # Warn about downgrades
            if downgraded_count > 0:
                logger.warning(
                    f"{downgraded_count} linkend(s) were DOWNGRADED to parent element - "
                    "readers will be taken to chapter/section instead of specific target"
                )

            return report

    def _convert_invalid_link_to_phrase(self, elem, linkend: str) -> None:
        """Convert a link/xref with invalid linkend to a phrase element."""
        from lxml import etree

        parent = elem.getparent()
        if parent is None:
            return

        # Skip non-element nodes (comments, processing instructions) where tag is not a string
        if not isinstance(elem.tag, str):
            return
        tag_name = elem.tag.split('}')[-1] if '}' in elem.tag else elem.tag

        if tag_name == 'xref':
            # xref has no content - preserve tail only
            tail = elem.tail or ''
            idx = list(parent).index(elem)
            parent.remove(elem)

            if tail:
                if idx > 0:
                    prev = parent[idx - 1]
                    prev.tail = (prev.tail or '') + tail
                else:
                    parent.text = (parent.text or '') + tail

            logger.debug(f"Removed xref with invalid linkend: {linkend}")

        elif tag_name == 'link':
            # Create phrase element
            phrase = etree.Element('phrase')
            phrase.text = elem.text
            phrase.tail = elem.tail

            # Copy children
            for child in list(elem):
                phrase.append(child)

            # Replace link with phrase
            idx = list(parent).index(elem)
            parent.remove(elem)
            parent.insert(idx, phrase)

            logger.debug(f"Converted link with invalid linkend to phrase: {linkend}")

        else:
            # For other elements, just remove the linkend attribute
            del elem.attrib['linkend']
            logger.debug(f"Removed invalid linkend attribute from {tag_name}: {linkend}")

    def _remove_empty_anchors(self, root_elem) -> int:
        """Remove empty/self-closing anchor elements."""
        removed_count = 0

        for anchor in list(root_elem.iter('anchor')):
            # Check if anchor is empty (no content)
            has_content = anchor.text and anchor.text.strip()
            has_children = len(anchor) > 0

            if not has_content and not has_children:
                parent = anchor.getparent()
                if parent is not None:
                    # Preserve tail text
                    tail = anchor.tail or ''
                    idx = list(parent).index(anchor)
                    parent.remove(anchor)

                    if tail:
                        if idx > 0:
                            prev = parent[idx - 1]
                            prev.tail = (prev.tail or '') + tail
                        else:
                            parent.text = (parent.text or '') + tail

                    removed_count += 1

        return removed_count

    # -------------------------------------------------------------------------
    # Pre-scan Methods
    # -------------------------------------------------------------------------

    def prescan_epub_file(self, filepath: Path, content: Optional[str] = None) -> Dict[str, SourceID]:
        """
        Pre-scan an EPUB XHTML file and extract all IDs.

        Args:
            filepath: Path to the XHTML file
            content: Optional file content (if already loaded)

        Returns:
            Dictionary of id_value -> SourceID
        """
        try:
            from bs4 import BeautifulSoup
        except ImportError:
            logger.warning("BeautifulSoup not available, skipping pre-scan")
            return {}

        epub_filename = filepath.name if isinstance(filepath, Path) else Path(filepath).name

        if content is None:
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
            except (IOError, OSError, UnicodeDecodeError) as e:
                # IOError/OSError: file access issues
                # UnicodeDecodeError: encoding issues
                logger.error(f"Failed to read {filepath}: {type(e).__name__}: {e}")
                return {}

        # Parse the HTML/XML content
        # Try lxml-xml first (faster, better for well-formed XML)
        # Fall back to html.parser (more lenient, handles malformed HTML)
        soup = None
        parser_used = None
        try:
            soup = BeautifulSoup(content, 'lxml-xml')
            parser_used = 'lxml-xml'
        except (ValueError, TypeError, LookupError) as e:
            # ValueError: invalid input
            # TypeError: unexpected input type
            # LookupError: lxml-xml parser not found (FeatureNotFound is subclass)
            logger.debug(f"lxml-xml parser failed for {filepath}, trying html.parser: {type(e).__name__}: {e}")
            try:
                soup = BeautifulSoup(content, 'html.parser')
                parser_used = 'html.parser'
            except (ValueError, TypeError) as e2:
                logger.error(f"Both parsers failed for {filepath}: {type(e2).__name__}: {e2}")
                return {}

        if soup is None:
            logger.error(f"Failed to create parser for {filepath}")
            return {}

        logger.debug(f"Parsed {filepath} with {parser_used}")

        source_ids = {}

        # Find all elements with id attribute
        for elem in soup.find_all(attrs={'id': True}):
            id_value = elem.get('id')
            if not id_value:
                continue

            # Handle list-type id attributes
            if isinstance(id_value, list):
                id_value = id_value[0] if id_value else ''

            id_value = str(id_value).strip()
            if not id_value:
                continue

            # Detect element type
            element_type = self._detect_element_type(elem, id_value)

            # Get additional context
            css_class = elem.get('class', '')
            if isinstance(css_class, list):
                css_class = ' '.join(css_class)

            epub_type = elem.get('epub:type', '') or elem.get('data-type', '')

            # Get parent context
            parent = elem.parent
            parent_info = f"{parent.name}" if parent else ""
            if parent and parent.get('class'):
                parent_class = parent.get('class')
                if isinstance(parent_class, list):
                    parent_class = ' '.join(parent_class)
                parent_info += f".{parent_class}"

            # Check if this should be dropped
            dropped, drop_reason = self._should_drop_id(id_value, elem, epub_type)

            source_id = SourceID(
                id_value=id_value,
                element_type=element_type,
                element_tag=elem.name,
                css_class=css_class,
                epub_type=epub_type,
                context=parent_info,
                dropped=dropped,
                drop_reason=drop_reason,
            )

            source_ids[id_value] = source_id
            logger.debug(f"Pre-scanned ID: {id_value} ({element_type}) in {epub_filename}")

        with self._lock:
            self._stats['epub_files_scanned'] = self._stats.get('epub_files_scanned', 0) + 1
            self._stats['total_source_ids'] = self._stats.get('total_source_ids', 0) + len(source_ids)

        return source_ids

    def _detect_element_type(self, elem, id_value: str) -> str:
        """Detect the semantic element type from tag, class, and ID patterns."""
        tag_name = elem.name.lower() if elem.name else ''

        # Check tag name first
        if tag_name in TAG_TO_TYPE:
            return TAG_TO_TYPE[tag_name]

        # Check epub:type
        epub_type = (elem.get('epub:type', '') or elem.get('data-type', '')).lower()
        if epub_type:
            for elem_type, patterns in ELEMENT_TYPE_PATTERNS.items():
                if any(p in epub_type for p in patterns):
                    return elem_type

        # Check CSS class
        css_class = elem.get('class', '')
        if isinstance(css_class, list):
            css_class = ' '.join(css_class)
        css_class = css_class.lower()

        if css_class:
            for elem_type, patterns in ELEMENT_TYPE_PATTERNS.items():
                if any(p in css_class for p in patterns):
                    return elem_type

        # Check ID value patterns
        id_lower = id_value.lower()
        for elem_type, patterns in ELEMENT_TYPE_PATTERNS.items():
            if any(p in id_lower for p in patterns):
                return elem_type

        # Default to anchor
        return 'anchor'

    def _should_drop_id(self, id_value: str, elem, epub_type: str) -> Tuple[bool, str]:
        """Determine if an ID should be dropped during conversion."""
        tag_name = elem.name.lower() if elem.name else ''

        # Pagebreak IDs are always dropped
        if epub_type and 'pagebreak' in epub_type.lower():
            return True, "pagebreak"

        # Empty spans with only ID are often navigation anchors - drop
        if tag_name == 'span':
            has_content = elem.string and elem.string.strip()
            has_children = len(list(elem.children)) > 1  # More than text
            if not has_content and not has_children:
                return True, "empty-span-anchor"

        # IDs that are just numbers are often auto-generated
        if id_value.isdigit():
            return True, "numeric-only-id"

        return False, ""

    def register_prescanned_file(self, epub_filename: str, source_ids: Dict[str, SourceID]) -> None:
        """
        Register pre-scanned IDs from an EPUB file.

        Args:
            epub_filename: Name of the EPUB file
            source_ids: Dictionary of id_value -> SourceID from prescan
        """
        with self._lock:
            for id_value, source_id in source_ids.items():
                # Create IDRecord in PRESCANNED state
                key = (epub_filename, id_value)
                record = IDRecord(
                    source_id=id_value,
                    source_file=epub_filename,
                    chapter_id="",  # Will be set during conversion
                    element_type=source_id.element_type,
                    state=IDState.PRESCANNED,
                    drop_reason=source_id.drop_reason if source_id.dropped else None,
                )
                if source_id.dropped:
                    record.state = IDState.DROPPED

                self._id_records[key] = record

    def export_json(self, path: Path) -> None:
        """Export registry state to JSON."""
        with self._lock:
            data = {
                'chapters': {
                    epub: mapping.to_dict()
                    for epub, mapping in self._epub_to_chapter.items()
                },
                'id_records': {
                    f"{k[0]}:{k[1]}": v.to_dict()
                    for k, v in self._id_records.items()
                },
                'stats': self.get_stats(),
                'exported_at': datetime.now().isoformat(),
            }
            with open(path, 'w', encoding='utf-8') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            logger.info(f"Exported registry to {path}")


# ============================================================================
# LINKEND RESOLVER
# ============================================================================

@dataclass
class ResolutionResult:
    """Result of linkend resolution attempt."""
    success: bool
    resolved_id: Optional[str]
    resolution_type: str = ""  # direct, mapped, cross_chapter
    source_id: str = ""
    target_chapter: str = ""
    error: Optional[str] = None
    is_xsl_resolvable: bool = True
    audit_trail: List[str] = field(default_factory=list)

    def to_dict(self) -> dict:
        return {
            'success': self.success,
            'resolved_id': self.resolved_id,
            'resolution_type': self.resolution_type,
            'source_id': self.source_id,
            'target_chapter': self.target_chapter,
            'error': self.error,
            'is_xsl_resolvable': self.is_xsl_resolvable,
            'audit_trail': self.audit_trail,
        }


class LinkendResolver:
    """
    Resolve linkends with strict validation and clear error reporting.

    NO regex-based guessing or similarity matching.
    Either the ID exists in the mapping or it doesn't.
    """

    # Citation-style IDs that should only resolve within their own chapter
    CITATION_PATTERN = re.compile(r'^(CR|Ref|ref|bib|Bib|fn|FN|note|Note)\d+$', re.IGNORECASE)

    def __init__(self, registry: ChapterRegistry):
        self.registry = registry
        self._resolution_log: List[ResolutionResult] = []

    def resolve(self, source_id: str, source_chapter: str,
                target_chapter: Optional[str] = None) -> ResolutionResult:
        """
        Resolve a linkend value.

        Args:
            source_id: Original ID to resolve
            target_chapter: Target chapter ID (if known)
            source_chapter: Chapter containing the link

        Returns:
            ResolutionResult with success/failure info
        """
        if not source_id:
            return ResolutionResult(
                success=False,
                resolved_id=None,
                source_id=source_id,
                target_chapter=target_chapter or source_chapter,
                error="Empty source ID"
            )

        # Determine effective target chapter
        effective_target = target_chapter or source_chapter

        # Check if this is a citation-style ID (should not cross chapters)
        is_citation = bool(self.CITATION_PATTERN.match(source_id))

        # Build audit trail
        audit = [f"Resolving '{source_id}' in chapter '{effective_target}'"]
        if is_citation:
            audit.append("Detected citation-style ID (chapter-local only)")

        # 1. Check if source_id is already a valid generated ID
        if self.registry.is_valid_id(source_id):
            parsed = IDParser.parse(source_id)
            result = ResolutionResult(
                success=True,
                resolved_id=source_id,
                resolution_type="direct",
                source_id=source_id,
                target_chapter=effective_target,
                is_xsl_resolvable=parsed.is_xsl_resolvable,
                audit_trail=audit + ["Direct match: ID exists in valid_ids"]
            )
            self._resolution_log.append(result)
            return result

        # 2. Try to resolve via registry in target chapter
        resolved = self.registry.resolve(effective_target, source_id)
        if resolved:
            parsed = IDParser.parse(resolved)

            # For citations, verify it's in the correct chapter
            if is_citation and not resolved.startswith(effective_target):
                audit.append(f"Rejected cross-chapter citation: {resolved}")
            else:
                result = ResolutionResult(
                    success=True,
                    resolved_id=resolved,
                    resolution_type="mapped",
                    source_id=source_id,
                    target_chapter=effective_target,
                    is_xsl_resolvable=parsed.is_xsl_resolvable,
                    audit_trail=audit + [f"Mapped: {source_id} → {resolved}"]
                )
                self._resolution_log.append(result)
                return result

        # 3. Check if dropped
        if self.registry.is_dropped(effective_target, source_id):
            reason = self.registry.get_drop_reason(effective_target, source_id)
            result = ResolutionResult(
                success=False,
                resolved_id=None,
                resolution_type="dropped",
                source_id=source_id,
                target_chapter=effective_target,
                error=f"ID dropped: {reason}",
                audit_trail=audit + [f"Dropped: {reason}"]
            )
            self._resolution_log.append(result)
            return result

        # 4. For non-citations, try source chapter if different
        if not is_citation and source_chapter != effective_target:
            resolved = self.registry.resolve(source_chapter, source_id)
            if resolved:
                parsed = IDParser.parse(resolved)
                result = ResolutionResult(
                    success=True,
                    resolved_id=resolved,
                    resolution_type="cross_chapter",
                    source_id=source_id,
                    target_chapter=source_chapter,
                    is_xsl_resolvable=parsed.is_xsl_resolvable,
                    audit_trail=audit + [f"Cross-chapter: {source_id} → {resolved} (in {source_chapter})"]
                )
                self._resolution_log.append(result)
                return result

        # 5. Try with common transformations
        transformations = [
            (source_id.lower(), "lowercase"),
            (re.sub(r'[^a-z0-9]', '', source_id.lower()), "sanitized"),
        ]

        # Strip chapter prefix if present
        if source_id.startswith(f"{effective_target}-"):
            transformations.append((source_id[len(effective_target)+1:], "unprefixed"))

        for transformed, transform_name in transformations:
            if transformed != source_id:
                resolved = self.registry.resolve(effective_target, transformed)
                if resolved:
                    parsed = IDParser.parse(resolved)
                    result = ResolutionResult(
                        success=True,
                        resolved_id=resolved,
                        resolution_type=f"transformed_{transform_name}",
                        source_id=source_id,
                        target_chapter=effective_target,
                        is_xsl_resolvable=parsed.is_xsl_resolvable,
                        audit_trail=audit + [f"Transformed ({transform_name}): {transformed} → {resolved}"]
                    )
                    self._resolution_log.append(result)
                    return result

        # Failed to resolve
        audit.append(f"No mapping found in chapters: [{effective_target}, {source_chapter}]")

        # Get any audit trail from registry
        registry_audit = self.registry.get_audit_trail(effective_target, source_id)
        if registry_audit:
            audit.extend(registry_audit)

        result = ResolutionResult(
            success=False,
            resolved_id=None,
            source_id=source_id,
            target_chapter=effective_target,
            error=f"No mapping found for '{source_id}'",
            audit_trail=audit
        )
        self._resolution_log.append(result)
        return result

    def get_resolution_log(self) -> List[ResolutionResult]:
        """Get all resolution attempts."""
        return self._resolution_log.copy()

    def get_failed_resolutions(self) -> List[ResolutionResult]:
        """Get only failed resolution attempts."""
        return [r for r in self._resolution_log if not r.success]

    def get_non_xsl_resolutions(self) -> List[ResolutionResult]:
        """Get resolutions to non-XSL-recognized IDs."""
        return [r for r in self._resolution_log if r.success and not r.is_xsl_resolvable]

    def clear_log(self) -> None:
        """Clear resolution log."""
        self._resolution_log.clear()

    def export_log(self, path: Path) -> None:
        """Export resolution log to JSON."""
        data = {
            'total': len(self._resolution_log),
            'successful': sum(1 for r in self._resolution_log if r.success),
            'failed': sum(1 for r in self._resolution_log if not r.success),
            'non_xsl_resolvable': sum(1 for r in self._resolution_log if r.success and not r.is_xsl_resolvable),
            'resolutions': [r.to_dict() for r in self._resolution_log],
            'exported_at': datetime.now().isoformat(),
        }
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        logger.info(f"Exported resolution log to {path}")


# ============================================================================
# ID AUTHORITY FACADE
# ============================================================================

class IDAuthority:
    """
    Facade for the centralized ID management system.

    Provides a unified interface for all ID operations:
    - Chapter registration
    - ID mapping
    - Linkend resolution
    - Validation
    - Export/import
    """

    def __init__(self):
        self.registry = ChapterRegistry()
        self.resolver = LinkendResolver(self.registry)
        self.parser = IDParser()

    # -------------------------------------------------------------------------
    # Chapter Management
    # -------------------------------------------------------------------------

    def register_chapter(self, epub_file: str, chapter_id: str,
                        xml_file: str = None, element_type: str = "chapter") -> None:
        """Register a chapter mapping."""
        self.registry.register_chapter(epub_file, chapter_id, xml_file, element_type)

    def get_chapter_id(self, epub_file: str) -> Optional[str]:
        """Get chapter ID for an EPUB file."""
        return self.registry.get_chapter_id(epub_file)

    # -------------------------------------------------------------------------
    # ID Mapping
    # -------------------------------------------------------------------------

    def map_id(self, chapter_id: str, source_id: str, generated_id: str,
               element_type: str = "", source_file: str = "") -> None:
        """Map source ID to generated ID."""
        self.registry.map_id(chapter_id, source_id, generated_id, element_type, source_file)

    def mark_dropped(self, chapter_id: str, source_id: str, reason: str,
                     element_type: str = "", source_file: str = "") -> None:
        """Mark an ID as dropped."""
        self.registry.mark_dropped(chapter_id, source_id, reason, element_type, source_file)

    def is_dropped(self, chapter_id: str, source_id: str) -> bool:
        """Check if an ID is dropped."""
        return self.registry.is_dropped(chapter_id, source_id)

    def get_drop_reason(self, chapter_id: str, source_id: str) -> Optional[str]:
        """Get the reason why an ID was dropped."""
        return self.registry.get_drop_reason(chapter_id, source_id)

    # -------------------------------------------------------------------------
    # Resolution
    # -------------------------------------------------------------------------

    def resolve(self, chapter_id: str, source_id: str) -> Optional[str]:
        """
        Resolve source ID to generated ID (direct registry lookup).

        Args:
            chapter_id: Chapter ID to look in
            source_id: Source ID to resolve

        Returns:
            Generated ID or None if not found/dropped
        """
        return self.registry.resolve(chapter_id, source_id)

    def resolve_linkend(self, source_id: str, source_chapter: str,
                        target_chapter: Optional[str] = None) -> str:
        """
        Resolve a linkend value (legacy-compatible).

        Returns resolved ID or empty string if not found.
        """
        result = self.resolver.resolve(source_id, source_chapter, target_chapter)
        return result.resolved_id or ""

    def resolve_linkend_full(self, source_id: str, source_chapter: str,
                             target_chapter: Optional[str] = None) -> ResolutionResult:
        """Resolve a linkend with full result details."""
        return self.resolver.resolve(source_id, source_chapter, target_chapter)

    # -------------------------------------------------------------------------
    # Validation
    # -------------------------------------------------------------------------

    def validate_id(self, id_string: str) -> Tuple[bool, Optional[str]]:
        """
        Validate an ID string.

        Returns (is_valid, error_message).
        """
        parsed = self.parser.parse(id_string)
        return (parsed.is_valid, parsed.validation_error)

    def parse_id(self, id_string: str) -> ParsedID:
        """Parse an ID into components."""
        return self.parser.parse(id_string)

    def is_xsl_resolvable(self, id_string: str) -> bool:
        """Check if an ID is XSL-resolvable."""
        return self.parser.is_xsl_resolvable(id_string)

    def get_element_code(self, element_type: str) -> str:
        """Get element code for an element type."""
        return self.parser.get_element_code(element_type)

    # -------------------------------------------------------------------------
    # Valid ID Management
    # -------------------------------------------------------------------------

    def register_valid_id(self, generated_id: str) -> None:
        """Register a valid ID that exists in output."""
        self.registry.register_valid_id(generated_id)

    def is_valid_id(self, generated_id: str) -> bool:
        """Check if a generated ID exists in output."""
        return self.registry.is_valid_id(generated_id)

    def build_valid_ids_from_xml(self, root_element) -> int:
        """Build valid ID cache from XML tree."""
        return self.registry.build_valid_ids_from_xml(root_element)

    # -------------------------------------------------------------------------
    # Linkend Validation & Fixing
    # -------------------------------------------------------------------------

    def apply_exact_source_id_mappings(self, root_element) -> int:
        """
        Update linkend attributes using exact source ID mappings only.

        Args:
            root_element: lxml root element

        Returns:
            Number of linkends updated
        """
        return self.registry.apply_exact_source_id_mappings(root_element)

    def validate_linkends_in_document(self, root_element) -> LinkendValidationReport:
        """
        Validate all linkend attributes and fix invalid ones.

        Args:
            root_element: lxml root element

        Returns:
            LinkendValidationReport with statistics
        """
        return self.registry.validate_linkends_in_document(root_element)

    # -------------------------------------------------------------------------
    # Pre-scan
    # -------------------------------------------------------------------------

    def prescan_epub_file(self, filepath: Path, content: Optional[str] = None) -> Dict[str, SourceID]:
        """
        Pre-scan an EPUB XHTML file and extract all IDs.

        Args:
            filepath: Path to the XHTML file
            content: Optional file content (if already loaded)

        Returns:
            Dictionary of id_value -> SourceID
        """
        return self.registry.prescan_epub_file(filepath, content)

    def register_prescanned_file(self, epub_filename: str, source_ids: Dict[str, SourceID]) -> None:
        """
        Register pre-scanned IDs from an EPUB file.

        Args:
            epub_filename: Name of the EPUB file
            source_ids: Dictionary of id_value -> SourceID from prescan
        """
        self.registry.register_prescanned_file(epub_filename, source_ids)

    # -------------------------------------------------------------------------
    # Audit & Export
    # -------------------------------------------------------------------------

    def get_audit_trail(self, chapter_id: str, source_id: str) -> List[str]:
        """Get audit trail for an ID."""
        return self.registry.get_audit_trail(chapter_id, source_id)

    def get_stats(self) -> dict:
        """Get statistics."""
        return self.registry.get_stats()

    def get_failed_resolutions(self) -> List[ResolutionResult]:
        """Get failed resolution attempts."""
        return self.resolver.get_failed_resolutions()

    def get_non_xsl_resolutions(self) -> List[ResolutionResult]:
        """Get resolutions to non-XSL-recognized IDs."""
        return self.resolver.get_non_xsl_resolutions()

    def export_registry(self, path: Path) -> None:
        """Export registry to JSON."""
        self.registry.export_json(path)

    def export_resolution_log(self, path: Path) -> None:
        """Export resolution log to JSON."""
        self.resolver.export_log(path)

    # -------------------------------------------------------------------------
    # Lifecycle
    # -------------------------------------------------------------------------

    def reset(self) -> None:
        """Reset all state for a new conversion."""
        self.registry.reset()
        self.resolver.clear_log()

    def add_chapter_update_listener(self, listener: Callable[[str, str], None]) -> None:
        """Add listener for chapter ID changes."""
        self.registry.add_update_listener(listener)


# ============================================================================
# SINGLETON INSTANCE
# ============================================================================

_authority: Optional[IDAuthority] = None
_lock = threading.Lock()


def get_authority() -> IDAuthority:
    """Get the singleton IDAuthority instance."""
    global _authority
    with _lock:
        if _authority is None:
            _authority = IDAuthority()
        return _authority


def reset_authority() -> None:
    """Reset the singleton instance."""
    global _authority
    with _lock:
        if _authority is not None:
            _authority.reset()
        _authority = None


# ============================================================================
# CONVENIENCE FUNCTIONS (for compatibility)
# ============================================================================

def register_chapter(epub_file: str, chapter_id: str,
                    xml_file: str = None, element_type: str = "chapter") -> None:
    """Register a chapter mapping."""
    get_authority().register_chapter(epub_file, chapter_id, xml_file, element_type)


def get_chapter_id(epub_file: str) -> Optional[str]:
    """Get chapter ID for an EPUB file."""
    return get_authority().get_chapter_id(epub_file)


def map_id(chapter_id: str, source_id: str, generated_id: str,
           element_type: str = "", source_file: str = "") -> None:
    """Map source ID to generated ID."""
    get_authority().map_id(chapter_id, source_id, generated_id, element_type, source_file)


def resolve_linkend(source_id: str, source_chapter: str,
                    target_chapter: Optional[str] = None) -> str:
    """Resolve a linkend value."""
    return get_authority().resolve_linkend(source_id, source_chapter, target_chapter)


def validate_id(id_string: str) -> Tuple[bool, Optional[str]]:
    """Validate an ID string."""
    return get_authority().validate_id(id_string)


def parse_id(id_string: str) -> ParsedID:
    """Parse an ID into components."""
    return get_authority().parse_id(id_string)


def get_element_code(element_type: str) -> str:
    """Get element code for an element type."""
    return get_authority().get_element_code(element_type)


def is_xsl_resolvable(id_string: str) -> bool:
    """Check if an ID is XSL-resolvable."""
    return get_authority().is_xsl_resolvable(id_string)


def prescan_file(filepath: Path, content: Optional[str] = None) -> Dict[str, SourceID]:
    """Pre-scan an EPUB file for all IDs."""
    return get_authority().prescan_epub_file(filepath, content)


def register_prescanned_file(epub_filename: str, source_ids: Dict[str, SourceID]) -> None:
    """Register pre-scanned IDs from an EPUB file."""
    get_authority().register_prescanned_file(epub_filename, source_ids)


# ============================================================================
# DEPRECATION MONITORING
# ============================================================================

class DeprecationMonitor:
    """
    Monitor and log usage of deprecated ID management patterns.

    This helps track if any external code is still using old patterns
    that should be migrated to id_authority.

    Usage:
        from id_authority import deprecation_monitor

        # Enable monitoring (disabled by default)
        deprecation_monitor.enable()

        # Check for deprecated patterns in a module
        deprecation_monitor.check_module(some_module)

        # Get report of deprecated usage
        report = deprecation_monitor.get_report()
    """

    def __init__(self):
        self._enabled = False
        self._warnings: List[Dict[str, Any]] = []
        self._lock = threading.Lock()

    def enable(self) -> None:
        """Enable deprecation monitoring."""
        self._enabled = True
        logger.info("Deprecation monitoring enabled for id_authority")

    def disable(self) -> None:
        """Disable deprecation monitoring."""
        self._enabled = False

    def is_enabled(self) -> bool:
        """Check if monitoring is enabled."""
        return self._enabled

    def log_deprecated_usage(self, pattern: str, location: str, suggestion: str) -> None:
        """
        Log a deprecated usage pattern.

        Args:
            pattern: The deprecated pattern detected (e.g., "id_tracker import")
            location: Where it was detected (e.g., "module_name:line_number")
            suggestion: What to use instead
        """
        if not self._enabled:
            return

        with self._lock:
            warning = {
                'pattern': pattern,
                'location': location,
                'suggestion': suggestion,
                'timestamp': datetime.now().isoformat(),
            }
            self._warnings.append(warning)
            logger.warning(
                f"DEPRECATED: {pattern} at {location}. "
                f"Use: {suggestion}"
            )

    def check_module(self, module) -> List[Dict[str, Any]]:
        """
        Check a module for deprecated patterns.

        Args:
            module: Python module to check

        Returns:
            List of deprecated patterns found
        """
        if not self._enabled:
            return []

        import inspect
        findings = []

        try:
            source = inspect.getsource(module)
            module_name = getattr(module, '__name__', str(module))

            # Check for deprecated imports
            deprecated_patterns = [
                ('from id_tracker import', 'from id_authority import get_authority'),
                ('import id_tracker', 'from id_authority import get_authority'),
                ('from id_mapper import', 'from id_authority import prescan_file, get_authority'),
                ('import id_mapper', 'from id_authority import prescan_file, get_authority'),
                ('IDTracker()', 'get_authority()'),
                ('IDMapper()', 'get_authority()'),
                ('get_tracker()', 'get_authority()'),
                ('get_mapper()', 'get_authority()'),
            ]

            for pattern, suggestion in deprecated_patterns:
                if pattern in source:
                    finding = {
                        'pattern': pattern,
                        'location': module_name,
                        'suggestion': suggestion,
                    }
                    findings.append(finding)
                    self.log_deprecated_usage(pattern, module_name, suggestion)

        except (TypeError, OSError):
            # Can't get source for built-in modules
            pass

        return findings

    def get_report(self) -> Dict[str, Any]:
        """
        Get a report of all deprecated usage detected.

        Returns:
            Dictionary with summary and details
        """
        with self._lock:
            return {
                'enabled': self._enabled,
                'total_warnings': len(self._warnings),
                'warnings': self._warnings.copy(),
                'summary': self._get_summary(),
            }

    def _get_summary(self) -> Dict[str, int]:
        """Get summary counts by pattern type."""
        summary: Dict[str, int] = {}
        for warning in self._warnings:
            pattern = warning['pattern']
            summary[pattern] = summary.get(pattern, 0) + 1
        return summary

    def clear(self) -> None:
        """Clear all recorded warnings."""
        with self._lock:
            self._warnings.clear()

    def export_report(self, path: Path) -> None:
        """Export deprecation report to JSON file."""
        report = self.get_report()
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(report, f, indent=2, ensure_ascii=False)
        logger.info(f"Exported deprecation report to {path}")


# Singleton instance
deprecation_monitor = DeprecationMonitor()


# ============================================================================
# ID GENERATOR FUNCTIONS (consolidated from id_generator.py)
# ============================================================================

# Maximum ID length - canonical definition
MAX_ID_LENGTH = SectionFormat.MAX_ID_LENGTH  # 25

# Pre-compiled regex patterns for ID validation
ID_PATTERN = re.compile(r'^[a-z0-9]+$')
CHAPTER_ID_PATTERN_SIMPLE = re.compile(r'^[a-z]{2}\d{4}$')
SECTION_ID_PATTERN_SIMPLE = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})+$')
_SECT1_ID_4DIGIT_PATTERN = re.compile(r'^([a-z]{2}\d{4})s(\d{4})$')

# Valid base ID pattern: {prefix}{4-digits}s{4-digits} = 11 characters minimum
VALID_BASE_ID_PATTERN = re.compile(r'^([a-z]{2})(\d{4})s(\d{4})')

# Valid element ID pattern: {base_id}{element_code}{sequence}
VALID_ELEMENT_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4}s\d{4})([a-z]{1,3})(\d{1,4})$')

# Valid section ID pattern: can have multiple s{4-digits} segments
VALID_SECTION_ID_PATTERN_SIMPLE = re.compile(r'^([a-z]{2}\d{4})(s\d{4})+$')

# Pattern for malformed IDs (common issues from EPUB inheritance)
MALFORMED_PATTERNS = {
    'missing_s_prefix': re.compile(r'^([a-z]{2}\d{4})(\d{4})'),
    'dash_separator': re.compile(r'^([a-z]{2}\d{4})-'),
    'underscore_separator': re.compile(r'^([a-z]{2}\d{4})_'),
    'uppercase_chars': re.compile(r'[A-Z]'),
    'special_chars': re.compile(r'[^a-z0-9]'),
    'short_section': re.compile(r'^([a-z]{2}\d{4})s(\d{1,3})(?![0-9])'),
    'no_section': re.compile(r'^([a-z]{2}\d{4})([a-z]{1,3}\d+)$'),
}


@dataclass
class IDComplianceResult:
    """Result of ID compliance check."""
    id_value: str
    is_compliant: bool
    issues: List[str]
    suggested_fix: Optional[str] = None
    id_type: Optional[str] = None  # 'chapter', 'section', 'element'


class HierarchicalIDGenerator:
    """
    Centralized ID generator following hierarchical naming conventions.

    ID Format (1-based section numbering):
    - Chapter: ch0001
    - Sect1: ch0001s0001 (4-digit counter)
    - Sect2: ch0001s0001s0001 (4-digit counter)
    - Sect3: ch0001s0001s0001s01 (2-digit counter to stay within 25 chars)
    - Sect4: ch0001s0001s0001s01s01 (2-digit counter)
    - Sect5: ch0001s0001s0001s01s01s01 (2-digit counter)
    - Elements: {parent_section_id}{element_code}{sequence}

    Note: Sect3+ use 2-digit counters (max 99 sections) to keep IDs ≤ 25 chars.
    """

    def __init__(self, chapter_id: str):
        """
        Initialize ID generator for a chapter.

        Args:
            chapter_id: Chapter ID (e.g., "ch0001")
        """
        self.chapter_id = chapter_id
        self.section_stack: List[Tuple[int, int]] = []
        self.level_counters: Dict[int, int] = {}
        self.element_counters: Dict[str, Dict[str, int]] = {}

    def _calculate_section_digits(self, level: int, section_num: int) -> int:
        """Calculate how many digits to use for a section counter."""
        base_id = self.chapter_id
        for num, digits in self.section_stack:
            base_id += f"s{num:0{digits}d}"

        if level <= 2:
            return 4

        potential_4digit_len = len(base_id) + 5
        if potential_4digit_len > 21:
            return 2
        if section_num < 100:
            return 2
        return 4

    def get_current_section_id(self) -> str:
        """Get the current section ID based on the section stack."""
        if not self.section_stack:
            return self.chapter_id

        section_id = self.chapter_id
        for section_num, digits in self.section_stack:
            section_id += f"s{section_num:0{digits}d}"
        return section_id

    def enter_section(self, level: int) -> str:
        """Enter a new section at the specified level and return its ID."""
        level = max(1, min(level, 5))

        while len(self.section_stack) >= level:
            self.section_stack.pop()
            # Reset only DEEPER levels, not the current level
            # This preserves the counter when returning to a previous level
            for deeper_level in range(level + 1, 6):
                self.level_counters[deeper_level] = 0

        if level not in self.level_counters:
            # Initialize to 0 so first increment gives 1 (1-based numbering)
            self.level_counters[level] = 0

        self.level_counters[level] += 1
        section_num = self.level_counters[level]
        digits = self._calculate_section_digits(level, section_num)
        self.section_stack.append((section_num, digits))
        return self.get_current_section_id()

    def generate_element_id(self, element_type: str) -> str:
        """Generate a unique ID for an element within the current section.

        Warning: At deep section levels (sect4+), element IDs may not fit within
        the 25-character limit with full 4-digit counters. The function will use
        shorter counters (3 or 2 digits) when necessary.

        At sect5 level (25 chars), there is NO room for element IDs. The function
        will log a warning and truncate the section ID, which produces non-standard
        IDs. Consider restructuring content to avoid elements at sect5 depth.
        """
        section_id = self.get_current_section_id()
        code = ELEMENT_TYPE_TO_CODE.get(element_type.lower(), 'x')

        if section_id not in self.element_counters:
            self.element_counters[section_id] = {}
        if element_type not in self.element_counters[section_id]:
            self.element_counters[section_id][element_type] = 0

        self.element_counters[section_id][element_type] += 1
        counter = self.element_counters[section_id][element_type]

        available = MAX_ID_LENGTH - len(section_id) - len(code)
        if available >= 4:
            element_id = f"{section_id}{code}{counter:04d}"
        elif available >= 3:
            element_id = f"{section_id}{code}{counter:03d}"
        elif available >= 2:
            element_id = f"{section_id}{code}{counter:02d}"
        else:
            # Not enough space - must truncate section ID (produces non-standard ID)
            logger.warning(
                f"Element ID for '{element_type}' at section '{section_id}' exceeds "
                f"25-char limit. Section ID will be truncated, producing non-standard ID. "
                f"Consider moving element to a shallower section level."
            )
            max_section_len = MAX_ID_LENGTH - len(code) - 2
            element_id = f"{section_id[:max_section_len]}{code}{counter:02d}"

        return element_id

    def reset(self):
        """Reset the generator state for a new chapter."""
        self.section_stack = []
        self.level_counters = {}
        self.element_counters = {}


# Legacy alias
class IDGenerator(HierarchicalIDGenerator):
    """Legacy alias for HierarchicalIDGenerator."""
    pass


# Global generator instance
_generator: Optional[HierarchicalIDGenerator] = None


def get_generator(chapter_id: str = None) -> HierarchicalIDGenerator:
    """Get or create the global ID generator."""
    global _generator
    if _generator is None or (chapter_id and _generator.chapter_id != chapter_id):
        _generator = HierarchicalIDGenerator(chapter_id or "ch0001")
    return _generator


def reset_generator():
    """Reset the global generator."""
    global _generator
    _generator = None


def generate_chapter_id(chapter_index: int) -> str:
    """
    Generate a chapter ID.

    Args:
        chapter_index: 0-based chapter index

    Returns:
        Chapter ID like "ch0001"
    """
    return f"ch{chapter_index + 1:04d}"


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
            # Cap counter at 9999 to prevent 5-digit section numbers
            if counter > 9999:
                logger.warning(f"Section counter {counter} at level {l} exceeds 9999, capping to {counter % 10000}")
                counter = counter % 10000
            section_id += f"s{counter:04d}"
        else:
            if counter > 99:
                logger.warning(f"Section counter {counter} at level {l} exceeds 99, truncating to {counter % 100}")
                counter = counter % 100
            section_id += f"s{counter:02d}"

    if len(section_id) > MAX_ID_LENGTH:
        logger.error(f"Section ID exceeds max length ({len(section_id)} > {MAX_ID_LENGTH}): {section_id}")
        section_id = section_id[:MAX_ID_LENGTH]

    return section_id


def generate_element_id_func(chapter_id: str, element_type: str, section_id: str = None) -> str:
    """
    Generate an element ID within a section.

    Args:
        chapter_id: Chapter ID
        element_type: Type of element
        section_id: Optional section ID (uses chapter_id + s0001 if not provided)

    Returns:
        Element ID
    """
    generator = get_generator(chapter_id)
    return generator.generate_element_id(element_type)


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


def validate_id_simple(id_value: str) -> Tuple[bool, Optional[str]]:
    """
    Validate an ID against naming rules.

    Args:
        id_value: ID to validate

    Returns:
        Tuple of (is_valid, error_message)
    """
    if len(id_value) > MAX_ID_LENGTH:
        return False, f"ID exceeds {MAX_ID_LENGTH} characters: {id_value} ({len(id_value)} chars)"
    if not re.match(r'^[a-z0-9]+$', id_value):
        return False, f"ID contains invalid characters: {id_value}"
    return True, None


def extract_chapter_from_id(id_value: str) -> Optional[str]:
    """
    Extract chapter ID from an element ID.

    Args:
        id_value: Element ID (e.g., "ch0001s0001f0001")

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
        id_value: Element ID (e.g., "ch0001s0001s0002f0001")

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


def next_available_sect1_id(chapter_id: str, existing_ids) -> str:
    """
    Generate a unique, convention-compliant sect1 ID for a chapter.

    This avoids legacy wrapper patterns and uses: {chapter_id}s#### (e.g., ch0003s0001)

    Args:
        chapter_id: Chapter ID
        existing_ids: Iterable of existing IDs

    Returns:
        Unique sect1 ID
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
    if VALID_SECTION_ID_PATTERN_SIMPLE.match(id_value):
        return 'section'
    if VALID_ELEMENT_ID_PATTERN.match(id_value):
        return 'element'
    return None


def check_id_compliance(id_value: str) -> IDComplianceResult:
    """
    Check if an ID is compliant with the hierarchical naming convention.

    Args:
        id_value: The ID value to check

    Returns:
        IDComplianceResult with compliance status and issues
    """
    issues = []
    suggested_fix = None

    if not id_value:
        return IDComplianceResult(
            id_value=id_value,
            is_compliant=False,
            issues=["Empty ID value"],
            id_type=None
        )

    if len(id_value) > MAX_ID_LENGTH:
        issues.append(f"Exceeds maximum length of {MAX_ID_LENGTH} characters ({len(id_value)} chars)")

    if MALFORMED_PATTERNS['uppercase_chars'].search(id_value):
        issues.append("Contains uppercase characters (must be lowercase only)")
        suggested_fix = id_value.lower()

    if MALFORMED_PATTERNS['special_chars'].search(id_value):
        issues.append("Contains invalid characters (only lowercase letters and digits allowed)")
        suggested_fix = re.sub(r'[^a-z0-9]', '', id_value.lower())

    if MALFORMED_PATTERNS['dash_separator'].search(id_value):
        issues.append("Uses dash separator (not allowed in IDs)")

    if MALFORMED_PATTERNS['underscore_separator'].search(id_value):
        issues.append("Uses underscore separator (not allowed in IDs)")

    match = MALFORMED_PATTERNS['missing_s_prefix'].match(id_value)
    if match and not VALID_BASE_ID_PATTERN.match(id_value):
        issues.append("Missing 's' prefix for section number")
        suggested_fix = f"{match.group(1)}s{match.group(2)}"

    if not VALID_SECTION_ID_PATTERN_SIMPLE.match(id_value) and not VALID_ELEMENT_ID_PATTERN.match(id_value):
        match = MALFORMED_PATTERNS['no_section'].match(id_value)
        if match:
            issues.append("Missing section suffix (required for all element IDs, e.g., s0001)")
            suggested_fix = f"{match.group(1)}s0001{match.group(2)}"

    match = MALFORMED_PATTERNS['short_section'].match(id_value)
    if match:
        section_num = match.group(2)
        if len(section_num) < 4:
            issues.append(f"Section number has {len(section_num)} digits (must be 4)")
            padded = section_num.zfill(4)
            suggested_fix = id_value.replace(f"s{section_num}", f"s{padded}", 1)

    id_type = detect_id_type(id_value)

    if id_type is None and not issues:
        issues.append("Does not match any valid ID pattern")
        if len(id_value) == 6 and re.match(r'^[a-z]{2}\d{4}$', id_value):
            id_type = 'chapter'
        elif 's' in id_value:
            id_type = 'section'
        else:
            id_type = 'element'

    is_compliant = len(issues) == 0

    return IDComplianceResult(
        id_value=id_value,
        is_compliant=is_compliant,
        issues=issues,
        suggested_fix=suggested_fix,
        id_type=id_type
    )


def generate_compliant_id(
    element_type: str,
    chapter_id: str,
    section_id: Optional[str] = None,
    existing_ids: Optional[Set[str]] = None,
    original_id: Optional[str] = None
) -> str:
    """
    Generate a fully compliant hierarchical ID for an element.

    Args:
        element_type: Type of element (e.g., 'figure', 'table', 'anchor')
        chapter_id: Chapter ID (e.g., 'ch0001')
        section_id: Optional section ID (e.g., 'ch0001s0001'). If None, uses s0001.
        existing_ids: Set of existing IDs to avoid duplicates
        original_id: Original EPUB ID (for tracking/mapping)

    Returns:
        Compliant element ID (e.g., 'ch0001s0001fg0001')
    """
    if existing_ids is None:
        existing_ids = set()

    if not CHAPTER_ID_PATTERN_SIMPLE.match(chapter_id):
        logger.warning(f"Invalid chapter_id '{chapter_id}', using ch0001")
        chapter_id = 'ch0001'

    if section_id:
        if VALID_SECTION_ID_PATTERN_SIMPLE.match(section_id):
            base_id = section_id
        elif VALID_BASE_ID_PATTERN.match(section_id):
            base_id = section_id
        else:
            base_id = f"{chapter_id}s0001"
    else:
        base_id = f"{chapter_id}s0001"

    elem_code = ELEMENT_TYPE_TO_CODE.get(element_type.lower(), 'x')
    prefix = f"{base_id}{elem_code}"
    max_seq = 0

    for existing_id in existing_ids:
        if existing_id.startswith(prefix):
            seq_str = existing_id[len(prefix):]
            if seq_str.isdigit():
                seq_num = int(seq_str)
                if seq_num > max_seq:
                    max_seq = seq_num

    next_seq = max_seq + 1
    available_digits = MAX_ID_LENGTH - len(base_id) - len(elem_code)

    if available_digits >= 4:
        new_id = f"{prefix}{next_seq:04d}"
    elif available_digits >= 3:
        new_id = f"{prefix}{next_seq:03d}"
    elif available_digits >= 2:
        new_id = f"{prefix}{next_seq:02d}"
    else:
        max_base_len = MAX_ID_LENGTH - len(elem_code) - 2
        new_id = f"{base_id[:max_base_len]}{elem_code}{next_seq:02d}"

    while new_id in existing_ids:
        next_seq += 1
        if available_digits >= 4:
            new_id = f"{prefix}{next_seq:04d}"
        elif available_digits >= 3:
            new_id = f"{prefix}{next_seq:03d}"
        else:
            new_id = f"{prefix}{next_seq:02d}"

    return new_id


def fix_malformed_id(
    malformed_id: str,
    element_type: str,
    chapter_id: str,
    section_id: Optional[str] = None,
    existing_ids: Optional[Set[str]] = None
) -> Tuple[str, str]:
    """
    Fix a malformed ID to be compliant with naming conventions.

    Args:
        malformed_id: The original malformed ID
        element_type: Type of element
        chapter_id: Chapter ID
        section_id: Optional section ID
        existing_ids: Set of existing IDs

    Returns:
        Tuple of (new_compliant_id, fix_description)
    """
    if existing_ids is None:
        existing_ids = set()

    compliance = check_id_compliance(malformed_id)

    if compliance.is_compliant:
        return malformed_id, "ID is already compliant"

    new_id = generate_compliant_id(
        element_type=element_type,
        chapter_id=chapter_id,
        section_id=section_id,
        existing_ids=existing_ids,
        original_id=malformed_id
    )

    issues_desc = "; ".join(compliance.issues)
    fix_desc = f"Fixed malformed ID '{malformed_id}' -> '{new_id}' ({issues_desc})"

    return new_id, fix_desc


def validate_all_ids_in_tree(root, chapter_id: str) -> List[IDComplianceResult]:
    """
    Validate all IDs in an XML element tree.

    Args:
        root: lxml Element root
        chapter_id: Expected chapter ID for this file

    Returns:
        List of IDComplianceResult for each ID found
    """
    results = []

    for elem in root.iter():
        elem_id = elem.get('id')
        if elem_id:
            result = check_id_compliance(elem_id)

            if result.is_compliant:
                id_chapter = extract_chapter_from_id(elem_id)
                if id_chapter and id_chapter != chapter_id:
                    result.is_compliant = False
                    result.issues.append(f"ID belongs to {id_chapter}, not {chapter_id}")

            results.append(result)

    return results
