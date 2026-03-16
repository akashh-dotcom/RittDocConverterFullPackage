#!/usr/bin/env python3
"""
Manual Post-Processor for RittDoc XML Files

This module provides comprehensive post-processing fixes:

1. Spacing fixes - across all content including TOC
   - Missing spaces between numbers and words (e.g., "Chapter 1Introduction" -> "Chapter 1 Introduction")
   - Missing spaces between words and numbers (e.g., "page5" -> "page 5")
   - Missing spaces before reference words (e.g., "seeChapter 10" -> "see Chapter 10")
   - Missing spaces between lowercase and uppercase words (e.g., "endChapter" -> "end Chapter")
   - Missing spaces at element boundaries (e.g., "see<link>Chapter</link>" -> "see <link>Chapter</link>")
   - Extra spaces to be trimmed

2. Leading number stripping
   - For ordered list items (listitem) - numbers are auto-generated
   - For bibliography entries (bibliomixed) - numbers are auto-generated

3. Publisher-specific rules (loaded from config/publishers/*.yaml)
   - Chapter removal based on title patterns (e.g., EULA, Index)
   - Element removal by XPath
   - Content transformations
   - Entity declaration cleanup in book.xml

Usage:
    python manual_postprocessor.py /path/to/package.zip --dry-run
    python manual_postprocessor.py /path/to/package.zip --output fixed.zip
    python manual_postprocessor.py /path/to/package.zip --publisher wiley
"""

import argparse
import logging
import os
import re
import shutil
import sys
import tempfile
import zipfile
import yaml
from dataclasses import dataclass, field
from pathlib import Path
from typing import List, Optional, Tuple, Set, Dict, Any

from lxml import etree

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Publisher config directory
CONFIG_DIR = Path(__file__).parent / "config" / "publishers"


# =============================================================================
# PUBLISHER CONFIGURATION LOADING
# =============================================================================

def load_publisher_config(publisher_name: Optional[str] = None) -> Dict[str, Any]:
    """
    Load publisher-specific configuration.

    Args:
        publisher_name: Name of publisher (e.g., 'wiley', 'springer')
                       If None, loads only defaults

    Returns:
        Merged configuration dictionary with post_processing rules
    """
    config = {
        'publisher_name': 'Default',
        'post_processing': {
            'remove_chapters': [],
            'remove_elements': [],
            'transform_content': []
        }
    }

    # Load default config
    default_config_path = CONFIG_DIR / "_default.yaml"
    if default_config_path.exists():
        try:
            with open(default_config_path, 'r', encoding='utf-8') as f:
                default_config = yaml.safe_load(f) or {}
                if 'post_processing' in default_config:
                    config['post_processing'].update(default_config['post_processing'])
        except Exception as e:
            logger.warning(f"Failed to load default config: {e}")

    # Load publisher-specific config if specified
    if publisher_name:
        publisher_config_path = CONFIG_DIR / f"{publisher_name.lower()}.yaml"
        if publisher_config_path.exists():
            try:
                with open(publisher_config_path, 'r', encoding='utf-8') as f:
                    pub_config = yaml.safe_load(f) or {}
                    config['publisher_name'] = pub_config.get('publisher_name', publisher_name)
                    if 'post_processing' in pub_config:
                        # Merge post-processing rules (publisher rules take precedence)
                        for key in ['remove_chapters', 'remove_elements', 'transform_content']:
                            if key in pub_config['post_processing']:
                                config['post_processing'][key] = pub_config['post_processing'][key]
                    logger.info(f"Loaded publisher config: {publisher_name}")
            except Exception as e:
                logger.warning(f"Failed to load publisher config '{publisher_name}': {e}")
        else:
            logger.warning(f"Publisher config not found: {publisher_config_path}")

    return config


def detect_publisher_from_package(temp_path: Path) -> Optional[str]:
    """
    Detect publisher from package metadata.

    Checks book.xml or other metadata files for publisher information.

    Args:
        temp_path: Path to extracted package contents

    Returns:
        Publisher name if detected, None otherwise
    """
    # Known publisher patterns and their config names
    publisher_patterns = {
        'wiley': ['wiley', 'john wiley', 'jossey-bass', 'wiley-blackwell', 'wiley-vch'],
        'springer': ['springer', 'springer nature', 'springer-verlag'],
        'elsevier': ['elsevier', 'academic press', 'saunders', 'mosby'],
        'pearson': ['pearson', 'prentice hall', 'addison-wesley'],
        'mcgraw-hill': ['mcgraw-hill', 'mcgraw hill', 'mcgrawhill'],
        'oxford': ['oxford university press', 'oxford'],
        'cambridge': ['cambridge university press', 'cambridge'],
        'taylor-francis': ['taylor & francis', 'taylor and francis', 'routledge', 'crc press'],
    }

    # Try to find book.xml
    book_xml_paths = list(temp_path.rglob('book.xml'))
    if not book_xml_paths:
        book_xml_paths = list(temp_path.rglob('Book.xml'))

    for book_xml_path in book_xml_paths:
        try:
            tree = etree.parse(str(book_xml_path))
            root = tree.getroot()

            # Check publisher element
            for pub_elem in root.iter('publisher'):
                pub_name_elem = pub_elem.find('.//publishername')
                if pub_name_elem is not None and pub_name_elem.text:
                    pub_text = pub_name_elem.text.lower()
                    for config_name, patterns in publisher_patterns.items():
                        for pattern in patterns:
                            if pattern in pub_text:
                                logger.info(f"Detected publisher: {config_name} (from '{pub_name_elem.text}')")
                                return config_name

            # Check bookinfo/publisher
            for bookinfo in root.iter('bookinfo'):
                for pub_elem in bookinfo.iter('publisher'):
                    pub_text = etree.tostring(pub_elem, encoding='unicode', method='text').lower()
                    for config_name, patterns in publisher_patterns.items():
                        for pattern in patterns:
                            if pattern in pub_text:
                                logger.info(f"Detected publisher: {config_name}")
                                return config_name

        except Exception as e:
            logger.debug(f"Error reading {book_xml_path}: {e}")

    return None


# =============================================================================
# SPACING FIX PATTERNS
# =============================================================================

# Pattern: digit followed by uppercase letter starting a word (missing space)
# Example: "Chapter 1Regulation" -> "Chapter 1 Regulation"
MISSING_SPACE_DIGIT_UPPER = re.compile(r'(\d)([A-Z][a-z])')

# Pattern: digit followed by uppercase word (all caps)
# Example: "Section 2DNA" -> "Section 2 DNA"
MISSING_SPACE_DIGIT_ALLCAPS = re.compile(r'(\d)([A-Z]{2,})')

# Pattern: lowercase letter followed by digit (missing space)
# Example: "Check1" -> "Check 1", "page5" -> "page 5"
# IMPORTANT: This is applied selectively — see _should_add_space_lower_digit()
# to avoid corrupting chemical formulas (Na2CO3), DOI paths (s12893), etc.
MISSING_SPACE_LOWER_DIGIT = re.compile(r'([a-z])(\d)')

# Words/patterns where lowercase-digit transition is INTENTIONAL (no space needed)
# Chemical formulas, measurement units, common abbreviations
_LOWER_DIGIT_EXCEPTIONS = re.compile(
    r'(?:'
    # Chemical formulas: Na2, CO2, H2O, Ca2+, Fe3+, O2, N2, etc.
    r'[A-Z][a-z]?\d'  # Element symbol (1-2 chars, starts uppercase) + digit
    r'|[a-z]\d+[a-z]'  # digit sandwiched in lowercase (e.g., s12893 in DOIs)
    r'|p\d+|v\d+|x\d+'  # Common prefixes: p53, v2, x64
    r'|mp3|mp4|h264|utf8|iso\d'  # Common tech terms
    r')'
)

# Pattern: lowercase followed by uppercase (missing space in merged words)
# Example: "endChapter" -> "end Chapter" (be careful with camelCase)
# Only apply when the uppercase part is long enough to be a word
MISSING_SPACE_LOWER_UPPER = re.compile(r'([a-z])([A-Z][a-z]{3,})')

# Pattern: lowercase followed by common document reference words (case-sensitive)
# These are specific words that commonly appear after "see", "in", etc. without proper spacing
# Example: "seeChapter 10" -> "see Chapter 10", "inTable 5" -> "in Table 5"
# This catches shorter words that MISSING_SPACE_LOWER_UPPER might miss
MISSING_SPACE_BEFORE_REFERENCE = re.compile(
    r'([a-z])((?:Chapter|Section|Figure|Table|Box|Page|Part|Appendix|Exhibit|'
    r'Example|Equation|Theorem|Lemma|Corollary|Definition|Proof|Remark|Note|'
    r'Case|Step|Item|Line|Verse|Column|Row|Panel|Entry|Volume|Article|Clause|'
    r'Paragraph|Graph|Chart|Diagram|Map|Photo|Image|Picture|Illustration|'
    r'Footnote|Endnote|Reference|Bibliography|Index|Glossary|Preface|'
    r'Introduction|Conclusion|Summary|Abstract|Acknowledgment|Dedication|'
    r'Foreword|Afterword|Prologue|Epilogue)\b)'
)

# Pattern: multiple consecutive spaces
EXTRA_SPACES = re.compile(r'[ \t]{2,}')

# Pattern: space before punctuation (should not exist)
SPACE_BEFORE_PUNCT = re.compile(r'\s+([.,;:!?])')

# Pattern: no space after punctuation followed by letter
# For PERIODS: only add space before UPPERCASE letters (sentence boundaries),
# NOT before lowercase (preserves "i.e.", "e.g.", "doi.org", "vs.", "et al.name")
# For OTHER punctuation (,;:!?): add space before any letter
NO_SPACE_AFTER_PUNCT = re.compile(r'([,;:!?])([A-Za-z])|\.([A-Z])')

# Pattern: tabs to be replaced with single space
TAB_TO_SPACE = re.compile(r'\t+')

# Pattern: broken abbreviation repairs (fix artifacts from previous processing versions)
# These patterns match abbreviations that were incorrectly split by older period-space logic
ABBREVIATION_REPAIRS = [
    (re.compile(r'\ba\.\s+k\.\s+a\.'), 'a.k.a.'),
]

# Pattern: double periods in text (C-006)
# Matches ".." NOT part of ellipsis "...", anywhere in text
# Covers both ".." before URLs and ".." before regular text
# Example: "504-12.. https://doi.org/" -> "504-12. https://doi.org/"
# Example: "2020;67(6):1103–34.. Epub 2020" -> "2020;67(6):1103–34. Epub 2020"
DOUBLE_PERIOD_GENERAL = re.compile(r'(?<!\.)\.\.(?!\.)')

# Pattern: zero-width characters that should be stripped from text
# U+200B (ZWSP), U+200C (ZWNJ), U+200D (ZWJ), U+FEFF (BOM/ZWNBS)
# These appear in Springer EPUBs and other publisher content, causing invisible
# breaks inside DOI paths and bibliography text.
ZERO_WIDTH_CHARS = re.compile('[\u200b\u200c\u200d\ufeff]')

# Pattern: double-encoded NBSP (C-026)
# When UTF-8 NBSP (0xC2 0xA0) is read as Windows-1252, it produces
# U+00C2 (Â) + U+00A0 (NBSP), which re-encoded to UTF-8 becomes visible "Â".
# Also handles double-encoded copyright: Â© → ©
DOUBLE_ENCODED_NBSP = re.compile('\u00c2\u00a0')  # Â followed by NBSP → just NBSP
DOUBLE_ENCODED_COPYRIGHT = re.compile('\u00c2\u00a9')  # Â© → ©

# Mojibake repair patterns (C-027)
# When UTF-8 multi-byte sequences are read as Windows-1252 and re-encoded,
# they produce characteristic garbled sequences. These map back to the
# original Unicode characters.
MOJIBAKE_REPAIRS = [
    # Smart quotes and dashes (3-byte UTF-8 sequences double-encoded)
    ('\u00e2\u20ac\u2122', '\u2019'),  # â€™ → ' (right single quote)
    ('\u00e2\u20ac\u2018', '\u2018'),  # â€˜ → ' (left single quote)
    ('\u00e2\u20ac\u201c', '\u201c'),  # â€œ → " (left double quote)
    ('\u00e2\u20ac\u009d', '\u201d'),  # â€\x9d → " (right double quote)
    ('\u00e2\u20ac\u201d', '\u2014'),  # â€" → — (em dash)
    ('\u00e2\u20ac\u201c', '\u201c'),  # â€" variant check
    ('\u00e2\u20ac\u0093', '\u2013'),  # â€" → – (en dash)
    # Greek letters (2-byte UTF-8 double-encoded)
    ('\u00ce\u00b1', '\u03b1'),  # Î± → α (alpha)
    ('\u00ce\u00b2', '\u03b2'),  # Î² → β (beta)
    ('\u00ce\u00b3', '\u03b3'),  # Î³ → γ (gamma)
    ('\u00ce\u00b4', '\u03b4'),  # Î´ → δ (delta)
    ('\u00ce\u00b5', '\u03b5'),  # Îµ → ε (epsilon)
    ('\u00ce\u00ba', '\u03ba'),  # Îº → κ (kappa)
    # Accented Latin characters (2-byte UTF-8 double-encoded)
    ('\u00c3\u00a9', '\u00e9'),  # Ã© → é
    ('\u00c3\u00a8', '\u00e8'),  # Ã¨ → è
    ('\u00c3\u00af', '\u00ef'),  # Ã¯ → ï
    ('\u00c3\u00bc', '\u00fc'),  # Ã¼ → ü
    ('\u00c3\u00b6', '\u00f6'),  # Ã¶ → ö
    ('\u00c3\u00a4', '\u00e4'),  # Ã¤ → ä
    ('\u00c3\u00b1', '\u00f1'),  # Ã± → ñ
]

# Pattern: DOI URL with internal spaces in the path segment
# After spacing normalization, DOI paths may get spaces inserted, e.g.:
#   "s 00441-016-2461-3" should be "s00441-016-2461-3"
# This pattern finds DOI URLs and removes internal spaces within the path.
DOI_PATH_SPACE_REPAIR = re.compile(
    r'((?:https?://)?doi\.org/10\.\d{4,}/)'  # DOI prefix: [https://]doi.org/10.NNNN/
    r'(.+?)(?=\s*$|\s+[A-Z]|\s*[,;)\]])'    # DOI path suffix until end/delimiter/sentence
)

# Pattern: concatenated bibliography labels (C-012/C-023)
# Matches labels like "CrossrefPubMed", "CrossrefPubMedPubMed Central", etc.
# These should be separated with " | " delimiter
CONCAT_BIB_LABELS = re.compile(
    r'(Crossref|CrossRef)(PubMed Central|PubMed)|'
    r'(PubMed)(PubMed Central)|'
    r'(PubMed Central|PubMed|Crossref|CrossRef)(Crossref|CrossRef|PubMed Central|PubMed)'
)


# =============================================================================
# CONTENT PRESERVATION VALIDATION
# =============================================================================

class ContentIntegrityError(Exception):
    """Raised when content is lost during processing."""
    pass


def normalize_for_comparison(text: str) -> str:
    """
    Normalize text for content comparison by removing all whitespace.

    This allows us to compare text content regardless of spacing changes.
    """
    if not text:
        return ""
    # Remove all whitespace (spaces, tabs, newlines)
    return re.sub(r'\s+', '', text)


def validate_content_preserved(original: str, modified: str,
                               operation: str = "spacing fix",
                               strict: bool = True) -> Tuple[bool, Optional[str]]:
    """
    Validate that no content was lost during a text transformation.

    Compares text content by normalizing whitespace and counting non-whitespace characters.

    Args:
        original: The original text before transformation
        modified: The modified text after transformation
        operation: Description of the operation for error messages
        strict: If True, raises ContentIntegrityError on content loss

    Returns:
        Tuple of (is_valid, error_message)
        - is_valid: True if content was preserved
        - error_message: Description of content loss, or None if valid

    Raises:
        ContentIntegrityError: If strict=True and content was lost
    """
    if not original:
        return (True, None)

    # Normalize both strings (remove all whitespace)
    orig_normalized = normalize_for_comparison(original)
    mod_normalized = normalize_for_comparison(modified)

    # Check if normalized content matches
    if orig_normalized == mod_normalized:
        return (True, None)

    # Content differs - check what was lost
    orig_chars = set(orig_normalized)
    mod_chars = set(mod_normalized)
    lost_chars = orig_chars - mod_chars

    # Calculate how much content was lost
    orig_len = len(orig_normalized)
    mod_len = len(mod_normalized)
    diff_len = orig_len - mod_len

    # Check if content was added (not a problem, could be space additions)
    if mod_len > orig_len:
        # Content was added - check if it's only whitespace-related
        # This is OK for spacing fixes
        return (True, None)

    # Content was lost - this is a problem
    if diff_len > 0:
        lost_percentage = (diff_len / orig_len) * 100 if orig_len > 0 else 0

        error_msg = (
            f"Content loss during {operation}: "
            f"{diff_len} characters lost ({lost_percentage:.1f}%). "
            f"Original: '{original[:50]}...', Modified: '{modified[:50]}...'"
        )

        if strict and lost_percentage > 0.1:  # More than 0.1% loss is an error
            logger.error(error_msg)
            raise ContentIntegrityError(error_msg)

        logger.warning(error_msg)
        return (False, error_msg)

    return (True, None)


def safe_text_transform(original: str, transform_func,
                        operation: str = "text transform") -> Tuple[str, bool, List[str]]:
    """
    Safely apply a text transformation with content preservation validation.

    Args:
        original: The original text
        transform_func: Function that takes text and returns (modified_text, was_modified, changes)
        operation: Description for error messages

    Returns:
        Tuple of (result_text, was_modified, changes)
        If content loss is detected, returns original text unchanged.
    """
    if not original:
        return (original, False, [])

    try:
        modified, was_modified, changes = transform_func(original)

        if was_modified:
            is_valid, error = validate_content_preserved(
                original, modified, operation, strict=False
            )

            if not is_valid:
                # Content was lost - revert to original
                logger.warning(f"Reverting {operation} due to content loss: {error}")
                return (original, False, [f"REVERTED: {error}"])

        return (modified, was_modified, changes)

    except ContentIntegrityError as e:
        logger.error(f"Content integrity error in {operation}: {e}")
        return (original, False, [f"ERROR: {str(e)}"])

    except Exception as e:
        logger.error(f"Unexpected error in {operation}: {e}")
        return (original, False, [f"ERROR: {str(e)}"])


# =============================================================================
# LEADING NUMBER PATTERNS FOR LISTS AND BIBLIOGRAPHY
# =============================================================================

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

# Pattern: Just a leading number like "1.", "2.", "(1)", "[2]"
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


# =============================================================================
# DATA CLASSES FOR REPORTING
# =============================================================================

@dataclass
class PostProcessChange:
    """Record of a single post-processing change."""
    file: str
    element_tag: str
    element_id: str
    rule: str
    original: str
    modified: str
    line_number: Optional[int] = None


@dataclass
class ChapterRemoval:
    """Record of a chapter removal."""
    chapter_file: str
    chapter_title: str
    rule_pattern: str
    reason: str


@dataclass
class ElementRemoval:
    """Record of an element removal."""
    file: str
    xpath: str
    element_count: int
    reason: str


@dataclass
class PostProcessReport:
    """Report of all changes made during post-processing."""
    package_file: str
    publisher: str = "Unknown"
    spacing_fixes: List[PostProcessChange] = field(default_factory=list)
    number_strip_fixes: List[PostProcessChange] = field(default_factory=list)
    chapter_removals: List[ChapterRemoval] = field(default_factory=list)
    element_removals: List[ElementRemoval] = field(default_factory=list)
    entity_cleanups: List[str] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)

    def has_changes(self) -> bool:
        return (bool(self.spacing_fixes) or bool(self.number_strip_fixes) or
                bool(self.chapter_removals) or bool(self.element_removals) or
                bool(self.entity_cleanups))

    def total_changes(self) -> int:
        return (len(self.spacing_fixes) + len(self.number_strip_fixes) +
                len(self.chapter_removals) + len(self.element_removals) +
                len(self.entity_cleanups))


# =============================================================================
# SPACING FIX FUNCTIONS
# =============================================================================

def _separate_bib_labels(m: re.Match) -> str:
    """Separate concatenated bibliography labels with ' | ' delimiter."""
    full = m.group(0)
    # Known labels in priority order (longest first to avoid partial matches)
    labels = ['PubMed Central', 'Crossref', 'CrossRef', 'PubMed']
    result = []
    remaining = full
    while remaining:
        matched = False
        for label in labels:
            if remaining.startswith(label):
                result.append(label)
                remaining = remaining[len(label):]
                matched = True
                break
        if not matched:
            # Shouldn't happen with our regex, but be safe
            result.append(remaining)
            break
    return ' | '.join(result)


def fix_spacing_in_text(text: str) -> Tuple[str, bool, List[str]]:
    """
    Fix spacing issues in text content.

    Applies:
    1. Add missing space between digit and uppercase letter
    2. Trim multiple consecutive spaces to single space
    3. Remove space before punctuation
    4. Add space after punctuation if missing
    5. Convert tabs to spaces

    Args:
        text: The text to fix

    Returns:
        Tuple of (fixed_text, was_modified, list of changes)
    """
    if not text:
        return (text, False, [])

    original = text
    changes = []

    # 0. Repair known broken abbreviations (artifacts from older processing)
    for pattern, replacement in ABBREVIATION_REPAIRS:
        before_abbr = text
        text = pattern.sub(replacement, text)
        if text != before_abbr:
            changes.append(f"Repaired abbreviation: '{replacement}'")

    # 1. Convert tabs to single spaces first
    text = TAB_TO_SPACE.sub(' ', text)
    if text != original:
        changes.append("Converted tabs to spaces")

    # 2. Fix missing space between digit and uppercase letter
    before = text
    text = MISSING_SPACE_DIGIT_UPPER.sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between digit and uppercase letter")

    # 3. Fix missing space between digit and all-caps word
    before = text
    text = MISSING_SPACE_DIGIT_ALLCAPS.sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between digit and all-caps word")

    # 4. Fix missing space between lowercase letter and digit
    # Example: "Check1" -> "Check 1"
    # Skip chemical formulas, DOI path segments, and technical terms
    before = text
    def _lower_digit_replacer(m):
        start = max(0, m.start() - 2)
        end = min(len(text), m.end() + 4)
        context = text[start:end]
        # Skip if context looks like a chemical formula (uppercase letter before lowercase)
        # e.g., "Na2" — the 'a' before '2' is part of element symbol "Na"
        if m.start() > 0 and text[m.start() - 1:m.start()].isupper():
            return m.group(0)  # Part of element symbol like Na2, Fe3, Ca2
        # Skip if digit is followed by lowercase (DOI path segment like s12893)
        if m.end() < len(text) and text[m.end():m.end()+1].isdigit():
            # Multi-digit number after lowercase — likely DOI/ID (e.g., s12893)
            # Check if there are 2+ digits following
            rest = text[m.end()-1:]
            if re.match(r'\d{2,}', rest):
                return m.group(0)
        return m.group(1) + ' ' + m.group(2)
    text = MISSING_SPACE_LOWER_DIGIT.sub(_lower_digit_replacer, text)
    if text != before:
        changes.append("Added space between lowercase and digit")

    # 5. Fix missing space before common reference words (Chapter, Section, etc.)
    # Apply this before general lowercase-uppercase to catch specific cases like "seeBox"
    before = text
    text = MISSING_SPACE_BEFORE_REFERENCE.sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space before reference word")

    # 6. Fix missing space between lowercase and uppercase (general case)
    # Example: "endChapter" -> "end Chapter"
    before = text
    text = MISSING_SPACE_LOWER_UPPER.sub(r'\1 \2', text)
    if text != before:
        changes.append("Added space between lowercase and uppercase word")

    # 7. Remove space before punctuation
    before = text
    text = SPACE_BEFORE_PUNCT.sub(r'\1', text)
    if text != before:
        changes.append("Removed space before punctuation")

    # 8. Add space after punctuation if missing (and followed by letter)
    # For periods: only before uppercase (sentence boundaries) — preserves i.e., e.g., doi.org
    # For other punctuation (,;:!?): before any letter
    before = text
    def _punct_space_replacer(m):
        if m.group(1):  # Non-period punctuation (,;:!?) followed by letter
            return m.group(1) + ' ' + m.group(2)
        elif m.group(3):  # Period followed by uppercase letter
            return '. ' + m.group(3)
        return m.group(0)
    text = NO_SPACE_AFTER_PUNCT.sub(_punct_space_replacer, text)
    if text != before:
        changes.append("Added space after punctuation")

    # 9. Collapse multiple spaces (do this last)
    before = text
    text = EXTRA_SPACES.sub(' ', text)
    if text != before:
        changes.append("Collapsed multiple spaces")

    # 10. Preserve boundary whitespace (leading/trailing single space)
    # This is critical for inline element boundaries — stripping all whitespace
    # can cause "El-Serag<emphasis>Bold" concatenation at element boundaries.
    had_leading_space = original[0:1] in (' ', '\t', '\n', '\r') if original else False
    had_trailing_space = original[-1:] in (' ', '\t', '\n', '\r') if original else False
    text = text.strip()
    if had_leading_space and text:
        text = ' ' + text
    if had_trailing_space and text:
        text = text + ' '

    was_modified = text != original

    # 11. CONTENT PRESERVATION VALIDATION
    # Ensure no non-whitespace content was lost during spacing fixes
    if was_modified:
        is_valid, error_msg = validate_content_preserved(
            original, text, operation="spacing fix", strict=False
        )
        if not is_valid:
            # Content was lost - revert to original (with stripped whitespace)
            logger.warning(f"Reverting spacing fix due to content loss: {error_msg}")
            changes.append(f"REVERTED: {error_msg}")
            text = original.strip()  # Revert but keep whitespace stripping
            was_modified = False

    # 12. Strip zero-width characters (ZWSP, ZWNJ, ZWJ, BOM)
    # Applied AFTER content validation since removing zero-width chars is intentional
    # and would otherwise be flagged as "content loss" by the validator.
    # These invisible characters appear in Springer and other publisher EPUBs,
    # causing invisible breaks inside DOI paths and bibliography text.
    before = text
    text = ZERO_WIDTH_CHARS.sub('', text)
    if text != before:
        changes.append("Stripped zero-width characters")
        was_modified = True

    # 13. Fix double periods in text (C-006)
    # Applied AFTER content validation since removing a duplicate period is intentional
    # Catches ".." anywhere (before URLs, before text like "Epub", at end of string)
    # Preserves ellipsis "..."
    before = text
    text = DOUBLE_PERIOD_GENERAL.sub('.', text)
    if text != before:
        changes.append("Fixed double period in text")
        was_modified = True

    # 14. Separate concatenated bibliography labels (C-012/C-023)
    # Applied AFTER content validation since label separation is intentional
    # Apply repeatedly: "CrossrefPubMedPubMed Central" needs two passes
    # (first splits CrossrefPubMed, second splits PubMed|PubMed Central)
    before = text
    for _ in range(3):  # Max 3 iterations for safety
        new_text = CONCAT_BIB_LABELS.sub(_separate_bib_labels, text)
        if new_text == text:
            break
        text = new_text
    if text != before:
        changes.append("Separated concatenated bibliography labels")
        was_modified = True

    # 15. Repair DOI URL paths with internal spaces
    # After spacing normalization, DOI path segments may get spaces inserted,
    # e.g., "s 00441-016-2461-3" should be "s00441-016-2461-3"
    # This fixes spaces within the DOI path (after doi.org/10.NNNN/)
    before = text
    def _repair_doi_path(m):
        prefix = m.group(1)  # e.g., "https://doi.org/10.1007/"
        suffix = m.group(2)  # e.g., "s 00441-016-2461-3"
        # Remove internal spaces from the DOI path suffix
        return prefix + suffix.replace(' ', '')
    text = DOI_PATH_SPACE_REPAIR.sub(_repair_doi_path, text)
    if text != before:
        changes.append("Repaired DOI URL path (removed internal spaces)")
        was_modified = True

    # 16. Fix double-encoded NBSP characters (C-026)
    # When UTF-8 NBSP (0xC2 0xA0) is read as Windows-1252, it produces
    # visible "Â" (U+00C2) + NBSP (U+00A0). Fix by collapsing back to single NBSP.
    # Also fix double-encoded copyright symbol: Â© → ©
    before = text
    text = DOUBLE_ENCODED_NBSP.sub('\u00a0', text)
    text = DOUBLE_ENCODED_COPYRIGHT.sub('\u00a9', text)
    if text != before:
        changes.append("Fixed double-encoded NBSP/copyright characters")
        was_modified = True

    # 17. Fix mojibake from double-encoded UTF-8 (C-027)
    # When UTF-8 multi-byte chars are read as Windows-1252 and re-encoded,
    # they produce characteristic garbled sequences (e.g., â€™ for right quote).
    # Map these back to their correct Unicode characters.
    before = text
    for garbled, correct in MOJIBAKE_REPAIRS:
        text = text.replace(garbled, correct)
    if text != before:
        changes.append("Repaired mojibake (double-encoded UTF-8)")
        was_modified = True

    return (text, was_modified, changes)


def fix_spacing_in_element(elem: etree._Element, report: PostProcessReport,
                           file_name: str) -> bool:
    """
    Fix spacing issues in an element's text and tail.

    Args:
        elem: The element to fix
        report: Report to add changes to
        file_name: Name of the file being processed

    Returns:
        True if any changes were made
    """
    modified = False
    elem_id = elem.get('id', 'unknown')
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    # Fix element's direct text
    if elem.text:
        fixed_text, was_fixed, changes = fix_spacing_in_text(elem.text)
        if was_fixed and fixed_text != elem.text:
            report.spacing_fixes.append(PostProcessChange(
                file=file_name,
                element_tag=elem.tag if isinstance(elem.tag, str) else str(elem.tag),
                element_id=elem_id,
                rule="Spacing Fix: " + "; ".join(changes),
                original=elem.text[:100] + ('...' if len(elem.text) > 100 else ''),
                modified=fixed_text[:100] + ('...' if len(fixed_text) > 100 else ''),
                line_number=line_num
            ))
            elem.text = fixed_text
            modified = True

    # Fix element's tail (text after closing tag)
    if elem.tail:
        fixed_tail, was_fixed, changes = fix_spacing_in_text(elem.tail)
        if was_fixed and fixed_tail != elem.tail:
            report.spacing_fixes.append(PostProcessChange(
                file=file_name,
                element_tag=f"{elem.tag}(tail)",
                element_id=elem_id,
                rule="Spacing Fix (tail): " + "; ".join(changes),
                original=elem.tail[:100] + ('...' if len(elem.tail) > 100 else ''),
                modified=fixed_tail[:100] + ('...' if len(fixed_tail) > 100 else ''),
                line_number=line_num
            ))
            elem.tail = fixed_tail
            modified = True

    return modified


def fix_element_boundary_spacing(elem: etree._Element, report: PostProcessReport,
                                  file_name: str) -> bool:
    """
    Fix missing spaces at element boundaries.

    This handles cases where inline elements (links, emphasis, etc.) are adjacent
    to text without proper spacing, like:
        <para>see<link>Chapter 10</link></para>  ->  <para>see <link>Chapter 10</link></para>
        <para>in<emphasis>Table 5</emphasis></para>  ->  <para>in <emphasis>Table 5</emphasis></para>

    The check looks for:
    1. Element text ending with word char, followed by child element starting with uppercase
    2. Element tail ending with word char, followed by next sibling starting with uppercase

    Args:
        elem: The element to check
        report: Report to add changes to
        file_name: Name of the file being processed

    Returns:
        True if any changes were made
    """
    modified = False
    elem_id = elem.get('id', 'unknown')
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    # Common inline elements that might have missing space before them
    inline_tags = {'link', 'xref', 'ulink', 'emphasis', 'phrase', 'literal',
                   'citetitle', 'quote', 'foreignphrase', 'wordasword',
                   'firstterm', 'glossterm', 'acronym', 'abbrev', 'citation',
                   'subscript', 'superscript'}

    # Check element.text -> first child boundary
    if elem.text and len(elem) > 0:
        first_child = elem[0]
        child_tag = first_child.tag if isinstance(first_child.tag, str) else ''

        # Only fix for inline elements or elements that commonly contain links
        if child_tag in inline_tags or child_tag:
            child_text = first_child.text or ''

            # Check if parent text ends without trailing space
            # and child text starts with a letter/digit — add space to prevent concatenation
            # Examples: "the<emphasis>flexor" → "the <emphasis>flexor"
            #           "muscle.<emphasis>The" → "muscle. <emphasis>The"
            if (elem.text and
                not elem.text.endswith((' ', '\t', '\n', '\r')) and
                child_text and
                child_text[0:1].isalnum()):

                original = elem.text
                elem.text = elem.text + ' '
                report.spacing_fixes.append(PostProcessChange(
                    file=file_name,
                    element_tag=f"{elem.tag}->{child_tag}",
                    element_id=elem_id,
                    rule="Element Boundary: Added space before inline element",
                    original=f"...{original[-20:]}<{child_tag}>{child_text[:20]}...",
                    modified=f"...{elem.text[-21:]}<{child_tag}>{child_text[:20]}...",
                    line_number=line_num
                ))
                modified = True

    # Check tail -> next sibling boundary for child elements
    for i, child in enumerate(elem):
        child_tag = child.tag if isinstance(child.tag, str) else ''
        child_id = child.get('id', 'unknown')

        # Check 1: child.tail ends without space AND next sibling starts with letter/digit
        # Examples: "</emphasis>and<emphasis>" → "</emphasis> and<emphasis>"
        if child.tail and i + 1 < len(elem):
            next_sibling = elem[i + 1]
            next_tag = next_sibling.tag if isinstance(next_sibling.tag, str) else ''
            next_text = next_sibling.text or ''

            if (not child.tail.endswith((' ', '\t', '\n', '\r')) and
                next_text and
                next_text[0:1].isalnum()):

                original = child.tail
                child.tail = child.tail + ' '
                report.spacing_fixes.append(PostProcessChange(
                    file=file_name,
                    element_tag=f"{child_tag}(tail)->{next_tag}",
                    element_id=child_id,
                    rule="Element Boundary: Added space between sibling elements",
                    original=f"</{child_tag}>{original[-20:]}<{next_tag}>{next_text[:15]}...",
                    modified=f"</{child_tag}>{child.tail[-21:]}<{next_tag}>{next_text[:15]}...",
                    line_number=line_num
                ))
                modified = True

        # Check 2 (C-011): Inline element's tail starts with alnum but no leading space
        # Examples: "</emphasis>the next" → "</emphasis> the next"
        # This catches cases where the child is an inline element (emphasis, link, etc.)
        # and its tail text runs into the element text without a space
        if (child_tag in inline_tags and child.tail and
                child.tail[0:1].isalnum()):
            # Get the last text content of the child to check if it ends with alnum
            last_text = ''
            if len(child) > 0:
                # Has sub-elements — get last descendant's tail or text
                for desc in reversed(list(child.iter())):
                    if desc.tail and desc is not child:
                        last_text = desc.tail
                        break
                    if desc.text and desc is not child:
                        last_text = desc.text
                        break
                if not last_text and child.text:
                    last_text = child.text
            else:
                last_text = child.text or ''

            last_char = last_text[-1:] if last_text else ''
            needs_space = last_char.isalnum() or last_char in (',', '.', ':', ';', ')', ']', '!')
            if needs_space:
                original = child.tail
                child.tail = ' ' + child.tail
                report.spacing_fixes.append(PostProcessChange(
                    file=file_name,
                    element_tag=f"{child_tag}(tail-start)",
                    element_id=child_id,
                    rule="Element Boundary: Added space after closing inline element (C-011)",
                    original=f"</{child_tag}>{original[:20]}...",
                    modified=f"</{child_tag}>{child.tail[:21]}...",
                    line_number=line_num
                ))
                modified = True

    return modified


# =============================================================================
# LEADING NUMBER STRIPPING FUNCTIONS
# =============================================================================

def strip_leading_number(text: str) -> Tuple[str, bool, Optional[str]]:
    """
    Strip leading number/enumeration from text.

    Examples:
        "1. Introduction" -> "Introduction"
        "1: First item" -> "First item"
        "(1) Entry one" -> "Entry one"
        "[2] Second entry" -> "Second entry"
        "1.2.3 Complex" -> "Complex"
        "  3.  Spaced" -> "Spaced"

    Args:
        text: The text to process

    Returns:
        Tuple of (stripped_text, was_modified, stripped_number)
    """
    if not text:
        return (text, False, None)

    original = text.strip()

    # Try complex numbering first (1.2.3)
    match = COMPLEX_NUMBERING.match(original)
    if match:
        stripped_number = match.group(1)
        remaining = match.group(2).strip()
        if remaining:  # Only strip if there's content after the number
            return (remaining, True, stripped_number)

    # Try simple leading number
    match = SIMPLE_LEADING_NUMBER.match(original)
    if match:
        stripped_number = match.group(1)
        remaining = match.group(2).strip()
        if remaining:  # Only strip if there's content after the number
            return (remaining, True, stripped_number)

    return (original, False, None)


def get_element_text_content(elem: etree._Element) -> str:
    """Get all text content from an element including children."""
    return ''.join(elem.itertext())


def strip_leading_number_from_element(elem: etree._Element,
                                      report: PostProcessReport,
                                      file_name: str,
                                      element_type: str) -> bool:
    """
    Strip leading number from an element's text content.

    For ordered lists and bibliography entries, the numbering is auto-generated
    by the rendering system, so any leading numbers in the content should be removed.

    Args:
        elem: The element to process
        report: Report to add changes to
        file_name: Name of the file being processed
        element_type: Type of element (for reporting)

    Returns:
        True if a number was stripped
    """
    elem_id = elem.get('id', 'unknown')
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    # Get the first text node (direct text content)
    if elem.text:
        stripped_text, was_modified, stripped_number = strip_leading_number(elem.text)
        if was_modified:
            report.number_strip_fixes.append(PostProcessChange(
                file=file_name,
                element_tag=elem.tag if isinstance(elem.tag, str) else str(elem.tag),
                element_id=elem_id,
                rule=f"Leading Number Strip ({element_type}): removed '{stripped_number}'",
                original=elem.text[:100] + ('...' if len(elem.text) > 100 else ''),
                modified=stripped_text[:100] + ('...' if len(stripped_text) > 100 else ''),
                line_number=line_num
            ))
            elem.text = stripped_text
            return True

    # If no direct text, check first child's text
    if len(elem) > 0:
        first_child = elem[0]
        if first_child.text:
            stripped_text, was_modified, stripped_number = strip_leading_number(first_child.text)
            if was_modified:
                report.number_strip_fixes.append(PostProcessChange(
                    file=file_name,
                    element_tag=f"{elem.tag}/{first_child.tag}",
                    element_id=elem_id,
                    rule=f"Leading Number Strip ({element_type}): removed '{stripped_number}'",
                    original=first_child.text[:100] + ('...' if len(first_child.text) > 100 else ''),
                    modified=stripped_text[:100] + ('...' if len(stripped_text) > 100 else ''),
                    line_number=line_num
                ))
                first_child.text = stripped_text
                return True

    return False


def process_ordered_lists(root: etree._Element, report: PostProcessReport,
                          file_name: str) -> int:
    """
    Process all ordered list items to strip leading numbers.

    Ordered lists (<orderedlist>) automatically generate numbers for each <listitem>,
    so any leading numbers in the content text should be removed to avoid duplication.

    Args:
        root: Root element of the XML tree
        report: Report to add changes to
        file_name: Name of the file being processed

    Returns:
        Number of items fixed
    """
    fixes = 0

    # Find all orderedlist elements
    for orderedlist in root.iter('orderedlist'):
        # Process each listitem in this ordered list
        for listitem in orderedlist.findall('listitem'):
            # Check para elements inside listitem
            for para in listitem.findall('para'):
                if strip_leading_number_from_element(para, report, file_name, 'orderedlist/listitem'):
                    fixes += 1
                    break  # Only fix the first para in each listitem

            # If no para, check direct text in listitem
            if listitem.text and listitem.text.strip():
                stripped_text, was_modified, stripped_number = strip_leading_number(listitem.text)
                if was_modified:
                    elem_id = listitem.get('id', 'unknown')
                    line_num = listitem.sourceline if hasattr(listitem, 'sourceline') else None
                    report.number_strip_fixes.append(PostProcessChange(
                        file=file_name,
                        element_tag='listitem',
                        element_id=elem_id,
                        rule=f"Leading Number Strip (orderedlist/listitem): removed '{stripped_number}'",
                        original=listitem.text[:100] + ('...' if len(listitem.text) > 100 else ''),
                        modified=stripped_text[:100] + ('...' if len(stripped_text) > 100 else ''),
                        line_number=line_num
                    ))
                    listitem.text = stripped_text
                    fixes += 1

    return fixes


def process_bibliography(root: etree._Element, report: PostProcessReport,
                         file_name: str) -> int:
    """
    Process bibliography entries to strip leading numbers.

    Bibliography entries (<bibliomixed>) in a <bibliography> container
    automatically generate numbers, so leading numbers in content should be removed.

    Args:
        root: Root element of the XML tree
        report: Report to add changes to
        file_name: Name of the file being processed

    Returns:
        Number of entries fixed
    """
    fixes = 0

    # Find all bibliomixed elements
    for bibliomixed in root.iter('bibliomixed'):
        if strip_leading_number_from_element(bibliomixed, report, file_name, 'bibliomixed'):
            fixes += 1

    # Also check biblioentry elements (another bibliography format)
    for biblioentry in root.iter('biblioentry'):
        # biblioentry has structured children, check for title or any text-bearing child
        for child in biblioentry:
            if child.tag == 'title' or child.tag == 'citetitle':
                # Don't strip from titles
                continue
            if strip_leading_number_from_element(child, report, file_name, 'biblioentry'):
                fixes += 1
                break

    return fixes


def remove_contact_of_author(root: etree._Element, report: PostProcessReport,
                              file_name: str) -> int:
    """
    Remove "ContactOf Author N" text from superscript elements (C-014).

    Springer source files have CSS class "ContactOfAuthor" which the converter
    incorrectly renders as visible text. This function finds and removes these
    artifacts from <superscript> elements.

    Args:
        root: Root element of the XML tree
        report: Report to add changes to
        file_name: Name of the file being processed

    Returns:
        Number of elements fixed
    """
    fixes = 0

    for sup in root.iter('superscript'):
        for child in list(sup):
            if child.tag == 'emphasis' and child.text:
                text = child.text.strip()
                if re.match(r'^ContactOf\s+Author\s+\d+$', text, re.IGNORECASE):
                    # Remove this emphasis element, preserving any tail text
                    tail = child.tail or ''
                    prev = child.getprevious()
                    if prev is not None:
                        prev.tail = (prev.tail or '') + tail
                    else:
                        sup.text = (sup.text or '') + tail
                    sup.remove(child)

                    report.spacing_fixes.append(PostProcessChange(
                        file=file_name,
                        element_tag='superscript/emphasis',
                        element_id=sup.get('id', 'unknown'),
                        rule=f"ContactOf Author removal (C-014): removed '{text}'",
                        original=text,
                        modified='(removed)',
                        line_number=sup.sourceline if hasattr(sup, 'sourceline') else None
                    ))
                    fixes += 1

    return fixes


# =============================================================================
# PUBLISHER-SPECIFIC PROCESSING FUNCTIONS
# =============================================================================

def match_chapter_title(title: str, rule: Dict[str, Any]) -> bool:
    """
    Check if a chapter title matches a removal rule.

    Args:
        title: The chapter title to check
        rule: Rule dictionary with 'pattern', 'match_type', 'case_sensitive'

    Returns:
        True if the title matches the rule
    """
    pattern = rule.get('pattern', '')
    match_type = rule.get('match_type', 'contains')
    case_sensitive = rule.get('case_sensitive', False)

    if not pattern:
        return False

    check_title = title if case_sensitive else title.lower()
    check_pattern = pattern if case_sensitive else pattern.lower()

    if match_type == 'exact':
        return check_title.strip() == check_pattern.strip()
    elif match_type == 'startswith':
        return check_title.strip().startswith(check_pattern)
    elif match_type == 'endswith':
        return check_title.strip().endswith(check_pattern)
    elif match_type == 'regex':
        try:
            flags = 0 if case_sensitive else re.IGNORECASE
            return bool(re.search(pattern, title, flags))
        except re.error:
            logger.warning(f"Invalid regex pattern: {pattern}")
            return False
    else:  # 'contains' (default)
        return check_pattern in check_title


def get_chapter_title(xml_path: Path) -> Optional[str]:
    """
    Extract the title from a chapter XML file.

    Args:
        xml_path: Path to the XML file

    Returns:
        Chapter title if found, None otherwise
    """
    try:
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(str(xml_path), parser)
        root = tree.getroot()

        # Look for title element (direct child or within info)
        title_elem = root.find('.//title')
        if title_elem is not None:
            # Get all text content including from child elements
            title_text = ''.join(title_elem.itertext()).strip()
            return title_text

        # Try to get from first heading
        for tag in ['bridgehead', 'para']:
            elem = root.find(f'.//{tag}')
            if elem is not None:
                text = ''.join(elem.itertext()).strip()
                if text:
                    return text

    except Exception as e:
        logger.debug(f"Error extracting title from {xml_path}: {e}")

    return None


def process_chapter_removals(temp_path: Path, config: Dict[str, Any],
                             report: PostProcessReport, dry_run: bool = False) -> List[str]:
    """
    Process chapter removal rules from publisher config.

    Removes chapter XML files matching the configured patterns and updates book.xml.

    Args:
        temp_path: Path to extracted package contents
        config: Publisher configuration dictionary
        report: Report to add changes to
        dry_run: If True, don't actually remove files

    Returns:
        List of removed chapter filenames
    """
    remove_rules = config.get('post_processing', {}).get('remove_chapters', [])
    if not remove_rules:
        return []

    removed_chapters = []

    # Find all XML files (potential chapters)
    xml_files = list(temp_path.rglob('*.xml'))

    for xml_path in xml_files:
        # Skip book.xml and other non-chapter files
        if xml_path.name.lower() in ['book.xml', 'toc.xml', 'metadata.xml']:
            continue

        title = get_chapter_title(xml_path)
        if not title:
            continue

        # Check against all removal rules
        for rule in remove_rules:
            if match_chapter_title(title, rule):
                reason = rule.get('reason', f"Matched pattern: {rule.get('pattern')}")
                logger.info(f"Removing chapter: {xml_path.name} (title: '{title}') - {reason}")

                report.chapter_removals.append(ChapterRemoval(
                    chapter_file=xml_path.name,
                    chapter_title=title,
                    rule_pattern=rule.get('pattern', ''),
                    reason=reason
                ))

                if not dry_run:
                    try:
                        xml_path.unlink()
                        removed_chapters.append(xml_path.name)
                    except Exception as e:
                        logger.error(f"Failed to remove {xml_path}: {e}")
                        report.errors.append(f"Failed to remove {xml_path.name}: {e}")
                else:
                    removed_chapters.append(xml_path.name)

                break  # Only match first rule

    return removed_chapters


def update_book_xml_entities(temp_path: Path, removed_chapters: List[str],
                             report: PostProcessReport, dry_run: bool = False) -> int:
    """
    Update book.xml to remove entity declarations and references for removed chapters.

    Args:
        temp_path: Path to extracted package contents
        removed_chapters: List of removed chapter filenames
        report: Report to add changes to
        dry_run: If True, don't actually modify files

    Returns:
        Number of entity references updated
    """
    if not removed_chapters:
        return 0

    # Find book.xml
    book_xml_paths = list(temp_path.rglob('book.xml'))
    if not book_xml_paths:
        book_xml_paths = list(temp_path.rglob('Book.xml'))

    if not book_xml_paths:
        logger.warning("book.xml not found - skipping entity cleanup")
        return 0

    updates = 0

    for book_xml_path in book_xml_paths:
        try:
            # Read the raw file content to handle entity declarations
            with open(book_xml_path, 'r', encoding='utf-8') as f:
                content = f.read()

            original_content = content
            modified = False

            for chapter_file in removed_chapters:
                # Remove entity declaration: <!ENTITY chXX SYSTEM "chXX.xml">
                chapter_base = chapter_file.replace('.xml', '').replace('.XML', '')

                # Pattern for entity declaration in DOCTYPE
                entity_pattern = re.compile(
                    rf'<!ENTITY\s+{re.escape(chapter_base)}\s+SYSTEM\s+["\'][^"\']*["\']>\s*',
                    re.IGNORECASE
                )
                new_content = entity_pattern.sub('', content)
                if new_content != content:
                    content = new_content
                    modified = True
                    updates += 1
                    report.entity_cleanups.append(
                        f"Removed entity declaration for {chapter_base}"
                    )
                    logger.debug(f"Removed entity declaration: {chapter_base}")

                # Remove entity reference: &chXX;
                entity_ref_pattern = re.compile(
                    rf'&{re.escape(chapter_base)};[ \t]*\n?',
                    re.IGNORECASE
                )
                new_content = entity_ref_pattern.sub('', content)
                if new_content != content:
                    content = new_content
                    modified = True
                    updates += 1
                    report.entity_cleanups.append(
                        f"Removed entity reference for {chapter_base}"
                    )
                    logger.debug(f"Removed entity reference: &{chapter_base};")

            # Also try to parse and remove from tree structure
            if modified or removed_chapters:
                try:
                    # Parse the modified content
                    parser = etree.XMLParser(remove_blank_text=False, resolve_entities=False)
                    tree = etree.fromstring(content.encode('utf-8'), parser)

                    # Remove any elements that reference removed chapters
                    for chapter_file in removed_chapters:
                        chapter_base = chapter_file.replace('.xml', '').replace('.XML', '')

                        # Find and remove elements with matching linkend or fileref
                        for attr in ['linkend', 'fileref', 'href']:
                            for elem in tree.xpath(f'//*[@{attr}]'):
                                attr_val = elem.get(attr, '')
                                if chapter_base in attr_val or chapter_file in attr_val:
                                    parent = elem.getparent()
                                    if parent is not None:
                                        # Move tail text to previous sibling or parent
                                        if elem.tail:
                                            prev = elem.getprevious()
                                            if prev is not None:
                                                prev.tail = (prev.tail or '') + elem.tail
                                            else:
                                                parent.text = (parent.text or '') + elem.tail
                                        parent.remove(elem)
                                        updates += 1
                                        logger.debug(f"Removed element with {attr}={attr_val}")

                    # Serialize back
                    content = etree.tostring(tree, encoding='unicode', xml_declaration=True)

                except Exception as e:
                    logger.debug(f"Could not parse book.xml for element removal: {e}")
                    # Continue with text-based modifications

            if modified and not dry_run:
                with open(book_xml_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                logger.info(f"Updated {book_xml_path.name}: {updates} entity/reference cleanups")

        except Exception as e:
            logger.error(f"Error updating {book_xml_path}: {e}")
            report.errors.append(f"Error updating book.xml: {e}")

    return updates


def process_element_removals(temp_path: Path, config: Dict[str, Any],
                             report: PostProcessReport, dry_run: bool = False) -> int:
    """
    Process element removal rules from publisher config.

    Removes elements matching XPath patterns from all XML files.

    Args:
        temp_path: Path to extracted package contents
        config: Publisher configuration dictionary
        report: Report to add changes to
        dry_run: If True, don't actually modify files

    Returns:
        Number of elements removed
    """
    remove_rules = config.get('post_processing', {}).get('remove_elements', [])
    if not remove_rules:
        return 0

    total_removed = 0
    xml_files = list(temp_path.rglob('*.xml'))

    for xml_path in xml_files:
        try:
            parser = etree.XMLParser(remove_blank_text=False)
            tree = etree.parse(str(xml_path), parser)
            root = tree.getroot()
            file_modified = False

            for rule in remove_rules:
                xpath = rule.get('xpath', '')
                reason = rule.get('reason', f"XPath rule: {xpath}")

                if not xpath:
                    continue

                try:
                    elements = root.xpath(xpath)
                    if elements:
                        for elem in elements:
                            parent = elem.getparent()
                            if parent is not None:
                                # Preserve tail text
                                if elem.tail:
                                    prev = elem.getprevious()
                                    if prev is not None:
                                        prev.tail = (prev.tail or '') + elem.tail
                                    else:
                                        parent.text = (parent.text or '') + elem.tail
                                parent.remove(elem)
                                total_removed += 1
                                file_modified = True

                        report.element_removals.append(ElementRemoval(
                            file=xml_path.name,
                            xpath=xpath,
                            element_count=len(elements),
                            reason=reason
                        ))
                        logger.debug(f"Removed {len(elements)} elements matching '{xpath}' from {xml_path.name}")

                except etree.XPathError as e:
                    logger.warning(f"Invalid XPath '{xpath}': {e}")

            if file_modified and not dry_run:
                tree.write(str(xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)

        except Exception as e:
            logger.debug(f"Error processing {xml_path} for element removal: {e}")

    return total_removed


def apply_content_transforms(temp_path: Path, config: Dict[str, Any],
                             report: PostProcessReport, dry_run: bool = False) -> int:
    """
    Apply content transformation rules from publisher config.

    Args:
        temp_path: Path to extracted package contents
        config: Publisher configuration dictionary
        report: Report to add changes to
        dry_run: If True, don't actually modify files

    Returns:
        Number of transformations applied
    """
    transform_rules = config.get('post_processing', {}).get('transform_content', [])
    if not transform_rules:
        return 0

    total_transforms = 0
    xml_files = list(temp_path.rglob('*.xml'))

    for xml_path in xml_files:
        try:
            parser = etree.XMLParser(remove_blank_text=False)
            tree = etree.parse(str(xml_path), parser)
            root = tree.getroot()
            file_modified = False

            for rule in transform_rules:
                pattern = rule.get('pattern', '')
                replacement = rule.get('replacement', '')
                target_elements = rule.get('elements', [])  # Empty = all elements

                if not pattern:
                    continue

                try:
                    regex = re.compile(pattern)
                except re.error as e:
                    logger.warning(f"Invalid transform pattern '{pattern}': {e}")
                    continue

                # Process all text nodes
                for elem in root.iter():
                    if target_elements and elem.tag not in target_elements:
                        continue

                    # Transform element text
                    if elem.text:
                        new_text = regex.sub(replacement, elem.text)
                        if new_text != elem.text:
                            elem.text = new_text
                            total_transforms += 1
                            file_modified = True

                    # Transform tail text
                    if elem.tail:
                        new_tail = regex.sub(replacement, elem.tail)
                        if new_tail != elem.tail:
                            elem.tail = new_tail
                            total_transforms += 1
                            file_modified = True

            if file_modified and not dry_run:
                tree.write(str(xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)

        except Exception as e:
            logger.debug(f"Error processing {xml_path} for content transforms: {e}")

    return total_transforms


# =============================================================================
# MAIN PROCESSING FUNCTIONS
# =============================================================================

def process_xml_file(xml_path: Path, report: PostProcessReport,
                     dry_run: bool = False) -> bool:
    """
    Process a single XML file and apply all post-processing fixes.

    Args:
        xml_path: Path to the XML file
        report: Report to add changes to
        dry_run: If True, don't modify the file

    Returns:
        True if any changes were made
    """
    try:
        # Parse XML preserving whitespace for accurate changes
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(str(xml_path), parser)
        root = tree.getroot()
    except etree.ParseError as e:
        report.errors.append(f"Failed to parse {xml_path.name}: {e}")
        return False

    file_name = xml_path.name
    modified = False

    # 1. Fix spacing in all text elements
    for elem in root.iter():
        # Skip non-element nodes (comments, processing instructions)
        if not isinstance(elem.tag, str):
            continue

        # Skip certain elements where spacing shouldn't be modified
        skip_tags = {'programlisting', 'screen', 'literallayout', 'code',
                     'literal', 'computeroutput', 'userinput'}
        if elem.tag in skip_tags:
            continue

        # For ulink elements: skip elem.text (URL/link text) but still process
        # elem.tail (regular text after closing </ulink> tag)
        # This prevents: "doi.org" -> "doi. org", "s12893" -> "s 12893" in link text
        # while still fixing spacing in text that follows the link
        if elem.tag == 'ulink':
            if elem.tail:
                fixed_tail, was_fixed, changes = fix_spacing_in_text(elem.tail)
                if was_fixed and fixed_tail != elem.tail:
                    report.spacing_fixes.append(PostProcessChange(
                        file=file_name,
                        element_tag="ulink(tail)",
                        element_id=elem.get('id', 'unknown'),
                        rule="Spacing Fix: " + "; ".join(changes),
                        original=elem.tail[:100] + ('...' if len(elem.tail) > 100 else ''),
                        modified=fixed_tail[:100] + ('...' if len(fixed_tail) > 100 else ''),
                        line_number=elem.sourceline if hasattr(elem, 'sourceline') else None
                    ))
                    elem.tail = fixed_tail
                    modified = True
            continue

        if fix_spacing_in_element(elem, report, file_name):
            modified = True

    # 2. Fix element boundary spacing (space lost between text and inline elements)
    # This handles cases like: see<link>Chapter 10</link> -> see <link>Chapter 10</link>
    for elem in root.iter():
        if not isinstance(elem.tag, str):
            continue

        # Skip code blocks
        skip_tags = {'programlisting', 'screen', 'literallayout', 'code',
                     'literal', 'computeroutput', 'userinput'}
        if elem.tag in skip_tags:
            continue

        if fix_element_boundary_spacing(elem, report, file_name):
            modified = True

    # 3. Strip leading numbers from ordered list items
    list_fixes = process_ordered_lists(root, report, file_name)
    if list_fixes > 0:
        modified = True

    # 4. Strip leading numbers from bibliography entries
    bib_fixes = process_bibliography(root, report, file_name)
    if bib_fixes > 0:
        modified = True

    # 5. Remove "ContactOf Author" artifacts from superscript elements (C-014)
    contact_fixes = remove_contact_of_author(root, report, file_name)
    if contact_fixes > 0:
        modified = True

    # Write changes if not dry run
    if modified and not dry_run:
        tree.write(str(xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)

        # 6. Post-write validation: verify the output is still well-formed XML
        # This catches any corruption introduced by postprocessor fixes
        try:
            validation_parser = etree.XMLParser(recover=False)
            etree.parse(str(xml_path), validation_parser)
        except etree.XMLSyntaxError as e:
            error_msg = f"POST-WRITE VALIDATION FAILED for {file_name}: {e}"
            logger.error(error_msg)
            report.errors.append(error_msg)
            # Attempt to restore from the original parse tree
            try:
                tree_backup = etree.parse(str(xml_path), etree.XMLParser(remove_blank_text=False))
                logger.warning(f"File {file_name} is recoverable despite validation warning")
            except Exception:
                logger.error(f"File {file_name} may be corrupted — manual review needed")

    return modified


def process_zip_package(zip_path: Path, output_path: Optional[Path] = None,
                        dry_run: bool = False,
                        publisher: Optional[str] = None) -> PostProcessReport:
    """
    Process a ZIP package, applying post-processing fixes to all XML files.

    Args:
        zip_path: Path to the input ZIP package
        output_path: Path for output ZIP (if None, modifies in place)
        dry_run: If True, don't modify any files
        publisher: Publisher name for loading specific config (auto-detected if None)

    Returns:
        PostProcessReport with all changes made
    """
    report = PostProcessReport(package_file=str(zip_path))

    if not zip_path.exists():
        report.errors.append(f"ZIP file not found: {zip_path}")
        return report

    # Create temp directory for extraction
    with tempfile.TemporaryDirectory() as temp_dir:
        temp_path = Path(temp_dir)

        # Extract ZIP
        try:
            with zipfile.ZipFile(zip_path, 'r') as zf:
                zf.extractall(temp_path)
        except zipfile.BadZipFile as e:
            report.errors.append(f"Failed to extract ZIP: {e}")
            return report

        # Detect or use provided publisher
        detected_publisher = publisher or detect_publisher_from_package(temp_path)
        if detected_publisher:
            report.publisher = detected_publisher
            logger.info(f"Using publisher config: {detected_publisher}")
        else:
            report.publisher = "Default"
            logger.info("No publisher detected, using default config")

        # Load publisher configuration
        config = load_publisher_config(detected_publisher)

        # =================================================================
        # PHASE 1: Publisher-specific chapter/element removal
        # =================================================================
        # These must happen BEFORE standard processing since they remove files

        # 1a. Process chapter removals (EULA, Index, etc.)
        removed_chapters = process_chapter_removals(temp_path, config, report, dry_run)
        if removed_chapters:
            logger.info(f"Removed {len(removed_chapters)} chapters based on publisher rules")

        # 1b. Update book.xml to remove entity declarations for removed chapters
        if removed_chapters:
            entity_updates = update_book_xml_entities(temp_path, removed_chapters, report, dry_run)
            if entity_updates:
                logger.info(f"Cleaned up {entity_updates} entity references in book.xml")

        # 1c. Process element removals by XPath
        elements_removed = process_element_removals(temp_path, config, report, dry_run)
        if elements_removed:
            logger.info(f"Removed {elements_removed} elements by XPath rules")

        # 1d. Apply content transformations
        transforms_applied = apply_content_transforms(temp_path, config, report, dry_run)
        if transforms_applied:
            logger.info(f"Applied {transforms_applied} content transformations")

        # =================================================================
        # PHASE 2: Standard post-processing (spacing, number stripping)
        # =================================================================

        # Find all remaining XML files
        xml_files = list(temp_path.rglob('*.xml'))
        xml_files.extend(list(temp_path.rglob('*.XML')))

        logger.info(f"Processing {len(xml_files)} XML files for spacing/number fixes")

        # Process each XML file
        any_modified = False
        for xml_file in sorted(xml_files):
            if process_xml_file(xml_file, report, dry_run):
                any_modified = True
                logger.debug(f"Modified: {xml_file.name}")

        # =================================================================
        # PHASE 2b: Post-processing validation
        # =================================================================
        # Validate book.xml with entity resolution (catches DTD issues before repackaging)
        # book.xml has DOCTYPE with entity declarations that include chapter files
        if any_modified and not dry_run:
            book_xml_candidates = list(temp_path.rglob('book.*.xml')) + list(temp_path.rglob('Book.xml'))
            for book_xml in book_xml_candidates:
                try:
                    # Parse with entity resolution — this validates that all chapter
                    # files are well-formed and that entity references resolve correctly
                    val_parser = etree.XMLParser(
                        load_dtd=True,
                        resolve_entities=True,
                        recover=False
                    )
                    # Need to parse from the file's directory so relative entity paths resolve
                    import os
                    old_cwd = os.getcwd()
                    try:
                        os.chdir(str(book_xml.parent))
                        etree.parse(str(book_xml), val_parser)
                        logger.info(f"Post-processing validation passed: {book_xml.name}")
                    finally:
                        os.chdir(old_cwd)
                except etree.XMLSyntaxError as e:
                    error_msg = f"POST-PROCESSING VALIDATION FAILED for {book_xml.name}: {e}"
                    logger.error(error_msg)
                    report.errors.append(error_msg)

        # =================================================================
        # PHASE 3: Repackage
        # =================================================================

        # Check if any changes were made
        has_publisher_changes = (
            bool(report.chapter_removals) or
            bool(report.element_removals) or
            bool(report.entity_cleanups)
        )

        # Repackage ZIP if changes were made and not dry run
        if (any_modified or has_publisher_changes) and not dry_run:
            # Determine output path
            if output_path is None:
                output_path = zip_path  # Modify in place

            # Create temp file first
            new_zip_path = temp_path / 'output.zip'

            with zipfile.ZipFile(new_zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in temp_path.rglob('*'):
                    if file_path.is_file() and file_path.name != 'output.zip':
                        arcname = file_path.relative_to(temp_path)
                        zf.write(file_path, arcname)

            # Move to final destination
            shutil.move(str(new_zip_path), str(output_path))
            logger.info(f"Updated package: {output_path}")

    return report


def print_report(report: PostProcessReport, verbose: bool = False) -> None:
    """Print a post-processing report to the console."""
    print(f"\n{'=' * 60}")
    print(f"Post-Processing Report: {Path(report.package_file).name}")
    print(f"Publisher: {report.publisher}")
    print(f"{'=' * 60}")

    if report.errors:
        print(f"\nERRORS ({len(report.errors)}):")
        for error in report.errors:
            print(f"  - {error}")

    # Publisher-specific changes
    if report.chapter_removals:
        print(f"\nCHAPTER REMOVALS ({len(report.chapter_removals)}):")
        for removal in report.chapter_removals:
            print(f"  - {removal.chapter_file}: \"{removal.chapter_title}\"")
            if verbose:
                print(f"      Pattern: {removal.rule_pattern}")
                print(f"      Reason: {removal.reason}")

    if report.element_removals:
        print(f"\nELEMENT REMOVALS ({len(report.element_removals)}):")
        for removal in report.element_removals:
            print(f"  - {removal.file}: {removal.element_count} elements matching '{removal.xpath}'")
            if verbose:
                print(f"      Reason: {removal.reason}")

    if report.entity_cleanups:
        print(f"\nENTITY CLEANUPS ({len(report.entity_cleanups)}):")
        for cleanup in report.entity_cleanups[:10]:
            print(f"  - {cleanup}")
        if len(report.entity_cleanups) > 10:
            print(f"  ... and {len(report.entity_cleanups) - 10} more")

    # Standard post-processing changes
    if report.spacing_fixes:
        print(f"\nSPACING FIXES ({len(report.spacing_fixes)}):")
        for change in report.spacing_fixes[:20]:  # Limit output
            print(f"  [{change.file}:{change.line_number or '?'}] {change.rule}")
            if verbose:
                print(f"      Before: {change.original[:60]}{'...' if len(change.original) > 60 else ''}")
                print(f"      After:  {change.modified[:60]}{'...' if len(change.modified) > 60 else ''}")
        if len(report.spacing_fixes) > 20:
            print(f"  ... and {len(report.spacing_fixes) - 20} more")

    if report.number_strip_fixes:
        print(f"\nLEADING NUMBER STRIPS ({len(report.number_strip_fixes)}):")
        for change in report.number_strip_fixes[:20]:  # Limit output
            print(f"  [{change.file}:{change.line_number or '?'}] {change.rule}")
            if verbose:
                print(f"      Before: {change.original[:60]}{'...' if len(change.original) > 60 else ''}")
                print(f"      After:  {change.modified[:60]}{'...' if len(change.modified) > 60 else ''}")
        if len(report.number_strip_fixes) > 20:
            print(f"  ... and {len(report.number_strip_fixes) - 20} more")

    if not report.has_changes() and not report.errors:
        print("\n  No changes needed.")

    print(f"\nSUMMARY:")
    print(f"  Chapter removals:     {len(report.chapter_removals)}")
    print(f"  Element removals:     {len(report.element_removals)}")
    print(f"  Entity cleanups:      {len(report.entity_cleanups)}")
    print(f"  Spacing fixes:        {len(report.spacing_fixes)}")
    print(f"  Number strip fixes:   {len(report.number_strip_fixes)}")
    print(f"  Total changes:        {report.total_changes()}")
    print()


# =============================================================================
# CLI ENTRY POINT
# =============================================================================

def main():
    parser = argparse.ArgumentParser(
        description='Manual post-processor for RittDoc XML packages',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    python manual_postprocessor.py ./Output/Successful/9781234.zip --dry-run
    python manual_postprocessor.py ./Output/Successful/9781234.zip -o fixed.zip
    python manual_postprocessor.py ./Output/Successful/9781234.zip --publisher wiley
    python manual_postprocessor.py ./Output/Successful/ -v

Rules Applied:
    1. Publisher-Specific Rules (from config/publishers/*.yaml):
       - Chapter removal based on title patterns (e.g., EULA, Index)
       - Element removal by XPath expressions
       - Content transformations using regex
       - Entity declaration cleanup in book.xml

    2. Spacing Fixes:
       - Add missing space between digit and uppercase letter (e.g., "Chapter 1Introduction")
       - Add missing space between digit and all-caps word (e.g., "Section 2DNA")
       - Add missing space between lowercase letter and digit (e.g., "page5", "Check1")
       - Add missing space before reference words (e.g., "seeChapter", "inTable")
       - Add missing space between lowercase and uppercase words (e.g., "endChapter")
       - Fix element boundary spacing (e.g., text<link>Chapter</link>)
       - Collapse multiple consecutive spaces
       - Remove space before punctuation
       - Add space after punctuation followed by letter
       - Convert tabs to single spaces

    3. Leading Number Stripping:
       - Remove leading numbers from ordered list items (listitem)
       - Remove leading numbers from bibliography entries (bibliomixed)

Available Publishers:
    wiley, springer (auto-detected from package metadata or specify with --publisher)
        """
    )

    parser.add_argument(
        'path',
        type=Path,
        nargs='?',  # Make optional for --list-publishers
        help='Path to ZIP package or directory containing ZIP files'
    )

    parser.add_argument(
        '-o', '--output',
        type=Path,
        help='Output path for fixed package (default: modify in place)'
    )

    parser.add_argument(
        '-p', '--publisher',
        type=str,
        help='Publisher name for config (e.g., wiley, springer). Auto-detected if not specified.'
    )

    parser.add_argument(
        '--dry-run', '-n',
        action='store_true',
        help='Preview changes without modifying files'
    )

    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show detailed output including before/after text'
    )

    parser.add_argument(
        '--quiet', '-q',
        action='store_true',
        help='Only show errors and summary'
    )

    parser.add_argument(
        '--list-publishers',
        action='store_true',
        help='List available publisher configurations and exit'
    )

    args = parser.parse_args()

    # List publishers and exit if requested
    if args.list_publishers:
        print("\nAvailable Publisher Configurations:")
        print("=" * 40)
        if CONFIG_DIR.exists():
            for config_file in sorted(CONFIG_DIR.glob('*.yaml')):
                if config_file.name.startswith('_'):
                    continue  # Skip default
                pub_name = config_file.stem
                try:
                    with open(config_file, 'r') as f:
                        config = yaml.safe_load(f) or {}
                        display_name = config.get('publisher_name', pub_name)
                        rules = config.get('post_processing', {})
                        chapter_rules = len(rules.get('remove_chapters', []))
                        element_rules = len(rules.get('remove_elements', []))
                        print(f"  {pub_name}: {display_name}")
                        print(f"      - {chapter_rules} chapter removal rules")
                        print(f"      - {element_rules} element removal rules")
                except Exception as e:
                    print(f"  {pub_name}: (error loading: {e})")
        else:
            print("  No publisher configs found.")
        print()
        sys.exit(0)

    # Require path for normal operation
    if args.path is None:
        parser.error("path is required unless using --list-publishers")

    if args.verbose:
        logger.setLevel(logging.DEBUG)
    elif args.quiet:
        logger.setLevel(logging.WARNING)

    path = args.path

    # Determine what to process
    if path.is_file() and path.suffix.lower() == '.zip':
        zip_files = [path]
    elif path.is_dir():
        zip_files = list(path.glob('*.zip'))
    else:
        logger.error(f"Invalid path: {path}")
        sys.exit(1)

    if not zip_files:
        logger.error(f"No ZIP files found in {path}")
        sys.exit(1)

    if args.dry_run:
        print("\n*** DRY RUN - No files will be modified ***\n")

    print(f"Processing {len(zip_files)} ZIP file(s)...")
    if args.publisher:
        print(f"Using publisher config: {args.publisher}")

    # Process each ZIP
    total_chapters = 0
    total_elements = 0
    total_entities = 0
    total_spacing = 0
    total_numbers = 0
    total_errors = 0

    for zip_file in sorted(zip_files):
        logger.info(f"Processing {zip_file.name}...")
        output_path = args.output if len(zip_files) == 1 else None
        report = process_zip_package(
            zip_file,
            output_path,
            dry_run=args.dry_run,
            publisher=args.publisher
        )

        total_chapters += len(report.chapter_removals)
        total_elements += len(report.element_removals)
        total_entities += len(report.entity_cleanups)
        total_spacing += len(report.spacing_fixes)
        total_numbers += len(report.number_strip_fixes)
        total_errors += len(report.errors)

        if not args.quiet or report.errors:
            print_report(report, verbose=args.verbose)

    # Print summary
    print("=" * 60)
    print("OVERALL SUMMARY")
    print("=" * 60)
    print(f"Files processed:        {len(zip_files)}")
    print(f"Chapter removals:       {total_chapters}")
    print(f"Element removals:       {total_elements}")
    print(f"Entity cleanups:        {total_entities}")
    print(f"Spacing fixes:          {total_spacing}")
    print(f"Number strip fixes:     {total_numbers}")
    total_all = total_chapters + total_elements + total_entities + total_spacing + total_numbers
    print(f"Total changes:          {total_all}")
    print(f"Errors:                 {total_errors}")

    if args.dry_run:
        print("\n*** DRY RUN - No files were modified ***")

    sys.exit(0 if total_errors == 0 else 1)


if __name__ == '__main__':
    main()
