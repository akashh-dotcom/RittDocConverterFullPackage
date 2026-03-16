#!/usr/bin/env python3
"""
TOC Cleanup Script for R2 Library XML Files

Processes Successful/{ISBN}.zip files and applies cleanup rules:
1. Front/Back Matter Classification - Reclassify TOC entries based on title patterns
2. Number Prefix Cleanup - Strip leading numbers from TOC entry titles
3. Generic Title Replacement - Flag placeholder titles for review
4. Spacing Fix - Fix missing spaces between numbers and words

Usage:
    python cleanup_toc.py /path/to/Successful/           # Process all zips in folder
    python cleanup_toc.py /path/to/Successful/ISBN.zip   # Process single zip
    python cleanup_toc.py /path/to/Successful/ --dry-run # Preview changes without modifying
"""

import argparse
import logging
import os
import re
import shutil
import sys
import tempfile
import zipfile
from dataclasses import dataclass, field
from pathlib import Path
from typing import List, Optional, Tuple
from xml.etree import ElementTree as ET

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# =============================================================================
# RULE 1: Front/Back Matter Classification
# =============================================================================

FRONT_MATTER_PATTERNS = [
    r'^contributors?$',
    r'^preface$',
    r'^table\s+of\s+contents?$',
    r'^contents?$',
    r'^toc$',
    r'^list\s+of\s+(illustrations?|figures?)$',
    r'^list\s+of\s+tables?$',
    r'^acknowledge?ments?(\s+acknowledge?ments?)?$',
    r'^foreword$',
    r'^introduction$',
    r'^about\s+the\s+authors?$',
    r'^about\s+this\s+book$',
    r'^how\s+to\s+use\s+this\s+book$',
    r'^dedication$',
    r'^copyright$',
    r'^copyright\s+page$',
    r'^title\s+page$',
    r'^half\s*title$',
    r'^series\s+page$',
    r'^frontispiece$',
    r'^epigraph$',
    r'^prologue$',
    r'^notes?\s+to\s+(the\s+)?readers?$',
    r'^guide\s+to\s+(the\s+)?readers?$',
    r'^conventions?\s+used$',
    r'^list\s+of\s+abbreviations?$',
    r'^abbreviations?$',
    r'^list\s+of\s+contributors?$',
    r'^editorial\s+board$',
    r'^about\s+the\s+companion\s+website$',
    r'^cover\s+page$',
    r'^about$',
    r'^author\s+biograph(y|ies)$',
    r'^biograph(y|ies)$',
    r'^about\s+the\s+editors?$',
    r'^editors?$',
    r'^list\s+of\s+authors?$',
]

BACK_MATTER_PATTERNS = [
    r'^index$',
    r'^subject\s+index$',
    r'^author\s+index$',
    r'^name\s+index$',
    r'^glossary$',
    r'^glossary\s+of\s+terms?$',
    r'^eula$',
    r'^end\s*user\s*license\s*agreement$',
    r'^wiley\s+end\s*user\s*license\s*agreement$',
    r'^appendix(\s+[a-z0-9]+)?$',
    r'^appendices$',
    r'^bibliography$',
    r'^references?$',
    r'^works?\s+cited$',
    r'^further\s+reading$',
    r'^suggested\s+reading$',
    r'^additional\s+resources?$',
    r'^answers?\s+(to\s+)?(exercises?|questions?|problems?)$',
    r'^solutions?(\s+manual)?$',
    r'^colophon$',
    r'^afterword$',
    r'^epilogue$',
    r'^endnotes?$',
    r'^credits?$',
    r'^permissions?$',
    r'^photo\s+credits?$',
    r'^about\s+the\s+cd(-rom)?$',
    r'^about\s+the\s+dvd$',
]

# Compile patterns for efficiency
FRONT_MATTER_COMPILED = [re.compile(p, re.IGNORECASE) for p in FRONT_MATTER_PATTERNS]
BACK_MATTER_COMPILED = [re.compile(p, re.IGNORECASE) for p in BACK_MATTER_PATTERNS]


def normalize_title(title: str) -> str:
    """
    Normalize title for pattern matching.

    Steps:
    1. Casefold and strip
    2. Remove chapter numbers like "1:" or "20:"
    3. Remove "CHAPTER X" prefix
    4. Remove punctuation
    5. Normalize whitespace
    """
    if not title:
        return ""

    # 1. Casefold and strip
    normalized = title.casefold().strip()

    # 2. Remove chapter numbers like "1:" or "20:"
    normalized = re.sub(r'^\d+\s*:\s*', '', normalized)

    # 3. Remove "CHAPTER X" prefix
    normalized = re.sub(r'^chapter\s+\d+\s*', '', normalized)

    # 4. Remove punctuation
    normalized = re.sub(r'[^\w\s]', '', normalized)

    # 5. Normalize whitespace
    normalized = ' '.join(normalized.split())

    return normalized


def classify_title(title: str) -> str:
    """
    Classify a title as FRONT, BACK, MAIN, or UNKNOWN.

    Priority order (matching user specification):
    1. "Chapter X" pattern -> MAIN (checked first on original title)
    2. "Part X" pattern -> MAIN (checked first on original title)
    3. Front matter patterns (checked on normalized title)
    4. Back matter patterns (checked on normalized title)
    5. "N:" pattern -> MAIN (fallback for numbered chapters without explicit "Chapter")
    6. UNKNOWN
    """
    if not title:
        return "UNKNOWN"

    title_lower = title.casefold().strip()

    # 1. Check explicit "Chapter X" and "Part X" patterns FIRST
    # These are strong indicators that override everything else
    if re.match(r'^chapter\s+\d+', title_lower):
        return "MAIN"
    if re.match(r'^part\s+[ivxlcdm\d]+', title_lower):
        return "MAIN"

    # 2. Normalize title for front/back matter matching
    # This removes "N:" prefixes and "CHAPTER X" prefixes before pattern matching
    normalized = normalize_title(title)

    # 3. Check front matter
    for pattern in FRONT_MATTER_COMPILED:
        if pattern.match(normalized):
            return "FRONT"

    # 4. Check back matter
    for pattern in BACK_MATTER_COMPILED:
        if pattern.match(normalized):
            return "BACK"

    # 5. Check "N:" pattern as fallback (numbered chapters without "Chapter" keyword)
    if re.match(r'^\d+\s*[:\s]', title_lower):
        return "MAIN"

    return "UNKNOWN"


# =============================================================================
# RULE 5: Number Prefix Cleanup
# =============================================================================

NUMBER_PREFIX_PATTERN = re.compile(r'^\s*(\d+)\s*:?\s*(.+)$', re.DOTALL)


def clean_number_prefix(text: str) -> Tuple[str, bool]:
    """
    Remove leading number prefix from text.

    Examples:
        "1: Author Biography" -> "Author Biography"
        "1 Index" -> "Index"
        "4: CHAPTER 1 Title" -> "CHAPTER 1 Title"
        "1\n\n         History" -> "History"
        "  2:  Some Text" -> "Some Text"

    Returns:
        Tuple of (cleaned_text, was_modified)
    """
    if not text:
        return (text, False)

    match = NUMBER_PREFIX_PATTERN.match(text)

    if match:
        cleaned = match.group(2).strip()
        return (cleaned, True)

    return (text.strip(), False)


# =============================================================================
# RULE 6: Generic Title Replacement
# =============================================================================

GENERIC_TITLE_PATTERNS = [
    r'^sect\d*$',           # sect1, sect2, sect
    r'^section$',           # section
    r'^content$',           # content
    r'^chapter$',           # chapter (without number)
    r'^part$',              # part (without number)
    r'^[a-z]{2}\d{4}.*$',   # IDs like ch0001, pr0002, dd0001
    r'^title$',             # title
    r'^untitled$',          # untitled
    r'^key\s*points?$',     # key points, key point
]

GENERIC_TITLE_COMPILED = [re.compile(p, re.IGNORECASE) for p in GENERIC_TITLE_PATTERNS]


def is_generic_title(title: str) -> bool:
    """
    Check if a title is generic/placeholder that should be replaced.

    Returns True for:
        - "Sect1", "sect2", "sect"
        - "Section", "Content", "Chapter", "Part", "Title", "Untitled"
        - "ch0001", "pr0002", "dd0001" (ID-like strings)
        - "Key Points", "key point"
        - Very short titles (1-2 characters)
        - Empty titles
    """
    if not title:
        return True

    title_lower = title.strip().lower()

    # Check against generic patterns
    for pattern in GENERIC_TITLE_COMPILED:
        if pattern.match(title_lower):
            return True

    # Very short titles are suspicious
    if len(title.strip()) <= 2:
        return True

    return False


# =============================================================================
# RULE 7: Spacing Fix (Enhanced)
# =============================================================================

# Pattern: digit followed by uppercase letter starting a word (missing space)
SPACING_DIGIT_UPPER = re.compile(r'(\d)([A-Z][a-z])')

# Pattern: digit followed by all-caps word
SPACING_DIGIT_ALLCAPS = re.compile(r'(\d)([A-Z]{2,})')

# Pattern: multiple consecutive spaces
EXTRA_SPACES = re.compile(r'[ \t]{2,}')

# Pattern: space before punctuation
SPACE_BEFORE_PUNCT = re.compile(r'\s+([.,;:!?])')

# Pattern: no space after punctuation followed by letter
NO_SPACE_AFTER_PUNCT = re.compile(r'([.,;:!?])([A-Za-z])')

# Pattern: tabs to spaces
TAB_TO_SPACE = re.compile(r'\t+')


def fix_spacing(text: str) -> Tuple[str, bool]:
    """
    Fix spacing issues in text.

    Applies:
    1. Add missing space between digit and uppercase letter
    2. Add missing space between digit and all-caps word
    3. Collapse multiple consecutive spaces to single space
    4. Remove space before punctuation
    5. Add space after punctuation if missing
    6. Convert tabs to spaces

    Examples:
        "CHAPTER 1Regulation" -> "CHAPTER 1 Regulation"
        "Part 2Methods" -> "Part 2 Methods"
        "Section 3Overview" -> "Section 3 Overview"
        "Multiple   spaces" -> "Multiple spaces"
        "Text ,with space" -> "Text, with space"
        "Text.next" -> "Text. next"

    Returns:
        Tuple of (fixed_text, was_modified)
    """
    if not text:
        return (text, False)

    original = text.strip()
    fixed = text

    # 1. Convert tabs to single spaces first
    fixed = TAB_TO_SPACE.sub(' ', fixed)

    # 2. Fix missing space between digit and uppercase letter
    fixed = SPACING_DIGIT_UPPER.sub(r'\1 \2', fixed)

    # 3. Fix missing space between digit and all-caps word
    fixed = SPACING_DIGIT_ALLCAPS.sub(r'\1 \2', fixed)

    # 4. Remove space before punctuation
    fixed = SPACE_BEFORE_PUNCT.sub(r'\1', fixed)

    # 5. Add space after punctuation if missing (and followed by letter)
    fixed = NO_SPACE_AFTER_PUNCT.sub(r'\1 \2', fixed)

    # 6. Collapse multiple spaces (do this last)
    fixed = EXTRA_SPACES.sub(' ', fixed)

    # 7. Strip leading/trailing whitespace
    fixed = fixed.strip()

    return (fixed, fixed != original)


# =============================================================================
# XML Processing
# =============================================================================

@dataclass
class CleanupChange:
    """Record of a single cleanup change."""
    file: str
    element_tag: str
    linkend: str
    rule: str
    original: str
    modified: str


@dataclass
class CleanupReport:
    """Report of all changes made during cleanup."""
    zip_file: str
    changes: List[CleanupChange] = field(default_factory=list)
    generic_titles: List[Tuple[str, str, str]] = field(default_factory=list)  # (file, linkend, title)
    reclassifications: List[Tuple[str, str, str, str, str]] = field(default_factory=list)  # (file, linkend, title, from, to)
    errors: List[str] = field(default_factory=list)

    def has_changes(self) -> bool:
        return bool(self.changes) or bool(self.reclassifications)


def get_element_text(elem: ET.Element) -> str:
    """Get all text content from an element, including nested elements."""
    return ''.join(elem.itertext())


def set_element_text(elem: ET.Element, text: str) -> None:
    """Set text content of an element, preserving structure for simple cases."""
    # If element has no children, just set text
    if len(elem) == 0:
        elem.text = text
    else:
        # For elements with children, only set leading text
        # This is a simplified approach - complex nested structures may need special handling
        elem.text = text
        for child in elem:
            child.tail = None


def find_tocentry(elem: ET.Element) -> Optional[ET.Element]:
    """Find the tocentry element within a TOC container."""
    tocentry = elem.find('tocentry')
    if tocentry is not None:
        return tocentry
    return None


def get_toc_title(elem: ET.Element) -> str:
    """Get the title text from a TOC element."""
    tocentry = find_tocentry(elem)
    if tocentry is not None:
        return get_element_text(tocentry)
    # Some elements have text directly
    return get_element_text(elem)


def process_toc_element(elem: ET.Element, report: CleanupReport, file_path: str) -> bool:
    """
    Process a single TOC element and apply cleanup rules.
    Returns True if any changes were made.
    """
    modified = False
    linkend = elem.get('linkend', '')
    tocentry = find_tocentry(elem)

    if tocentry is None:
        # Element might have direct text content
        tocentry = elem

    original_text = get_element_text(tocentry)
    current_text = original_text

    # Rule 7: Spacing Fix (apply first to normalize text)
    fixed_text, spacing_changed = fix_spacing(current_text)
    if spacing_changed:
        report.changes.append(CleanupChange(
            file=file_path,
            element_tag=elem.tag,
            linkend=linkend,
            rule="Rule 7: Spacing Fix",
            original=current_text,
            modified=fixed_text
        ))
        current_text = fixed_text
        modified = True

    # Rule 5: Number Prefix Cleanup (only for top-level entries)
    if elem.tag in ('tocfront', 'tocback', 'tocchap', 'tocpart', 'tocsubpart'):
        cleaned_text, prefix_removed = clean_number_prefix(current_text)
        if prefix_removed:
            report.changes.append(CleanupChange(
                file=file_path,
                element_tag=elem.tag,
                linkend=linkend,
                rule="Rule 5: Number Prefix Cleanup",
                original=current_text,
                modified=cleaned_text
            ))
            current_text = cleaned_text
            modified = True

    # Rule 6: Generic Title Detection (flag but don't auto-replace)
    if is_generic_title(current_text):
        report.generic_titles.append((file_path, linkend, current_text))

    # Apply text changes if any
    if modified:
        if tocentry is not elem:
            # Update tocentry text
            if len(tocentry) == 0:
                tocentry.text = current_text
            else:
                # More complex structure - clear and set
                tocentry.text = current_text
                for child in list(tocentry):
                    tocentry.remove(child)
        else:
            # Direct element text
            if len(elem) == 0:
                elem.text = current_text

    return modified


def should_be_tocfront(elem: ET.Element, title: str) -> bool:
    """Check if element should be reclassified as tocfront."""
    classification = classify_title(title)
    return classification == "FRONT" and elem.tag != 'tocfront'


def should_be_tocback(elem: ET.Element, title: str) -> bool:
    """Check if element should be reclassified as tocback."""
    classification = classify_title(title)
    return classification == "BACK" and elem.tag != 'tocback'


def reclassify_element(parent: ET.Element, elem: ET.Element, new_tag: str) -> ET.Element:
    """
    Reclassify a TOC element by changing its tag.
    Returns the new element.
    """
    # Create new element with same attributes and content
    new_elem = ET.Element(new_tag)
    new_elem.attrib = elem.attrib.copy()
    new_elem.text = elem.text
    new_elem.tail = elem.tail

    # Copy all children
    for child in elem:
        new_elem.append(child)

    # Replace in parent
    idx = list(parent).index(elem)
    parent.remove(elem)
    parent.insert(idx, new_elem)

    return new_elem


def process_toc_file(xml_path: Path, report: CleanupReport, dry_run: bool = False) -> bool:
    """
    Process a TOC XML file and apply all cleanup rules.
    Returns True if any changes were made.
    """
    try:
        # Parse with preservation of original formatting where possible
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except ET.ParseError as e:
        report.errors.append(f"Failed to parse {xml_path}: {e}")
        return False

    modified = False
    file_name = xml_path.name

    # Find all TOC elements that need processing
    toc_elements = ['tocfront', 'tocback', 'tocchap', 'tocpart', 'tocsubpart',
                    'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5']

    # Process in document order
    for tag in toc_elements:
        for elem in root.iter(tag):
            if process_toc_element(elem, report, file_name):
                modified = True

    # Rule 1: Front/Back Matter Reclassification
    # We need to handle this carefully - changing element tags requires parent access
    toc_root = root.find('.//toc') or root

    # Collect elements to reclassify (can't modify during iteration)
    to_reclassify = []

    for parent in toc_root.iter():
        for elem in list(parent):
            if elem.tag in ('tocchap', 'toclevel1', 'tocpart', 'tocsubpart'):
                title = get_toc_title(elem)
                linkend = elem.get('linkend', '')

                if should_be_tocfront(elem, title):
                    to_reclassify.append((parent, elem, 'tocfront', title, linkend))
                elif should_be_tocback(elem, title):
                    to_reclassify.append((parent, elem, 'tocback', title, linkend))

    # Apply reclassifications
    for parent, elem, new_tag, title, linkend in to_reclassify:
        old_tag = elem.tag
        reclassify_element(parent, elem, new_tag)
        report.reclassifications.append((file_name, linkend, title, old_tag, new_tag))
        modified = True

    # Write changes if not dry run
    if modified and not dry_run:
        tree.write(xml_path, encoding='UTF-8', xml_declaration=True)

    return modified


def process_zip_file(zip_path: Path, dry_run: bool = False) -> CleanupReport:
    """
    Process a single ZIP file, applying cleanup rules to all TOC XMLs.
    """
    report = CleanupReport(zip_file=str(zip_path))

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

        # Find all TOC XML files (typically toc.*.xml)
        toc_files = list(temp_path.rglob('toc.*.xml'))

        # Also check for any XML that might be a TOC
        for xml_file in temp_path.rglob('*.xml'):
            if xml_file not in toc_files:
                # Quick check if it's a TOC file
                try:
                    with open(xml_file, 'r', encoding='utf-8') as f:
                        content = f.read(500)
                        if '<toc>' in content or '<toc ' in content:
                            toc_files.append(xml_file)
                except Exception:
                    pass

        if not toc_files:
            logger.info(f"No TOC files found in {zip_path.name}")
            return report

        # Process each TOC file
        any_modified = False
        for toc_file in toc_files:
            logger.debug(f"Processing {toc_file.name}")
            if process_toc_file(toc_file, report, dry_run):
                any_modified = True

        # Repackage ZIP if changes were made and not dry run
        if any_modified and not dry_run:
            # Create new ZIP with updated content
            new_zip_path = zip_path.with_suffix('.zip.new')

            with zipfile.ZipFile(new_zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in temp_path.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(temp_path)
                        zf.write(file_path, arcname)

            # Replace original with new
            shutil.move(new_zip_path, zip_path)
            logger.info(f"Updated {zip_path.name}")

    return report


def print_report(report: CleanupReport, verbose: bool = False) -> None:
    """Print a cleanup report to the console."""
    print(f"\n{'='*60}")
    print(f"Report for: {Path(report.zip_file).name}")
    print(f"{'='*60}")

    if report.errors:
        print(f"\nERRORS ({len(report.errors)}):")
        for error in report.errors:
            print(f"  - {error}")

    if report.reclassifications:
        print(f"\nRECLASSIFICATIONS ({len(report.reclassifications)}):")
        for file, linkend, title, old_tag, new_tag in report.reclassifications:
            print(f"  [{old_tag} -> {new_tag}] {title[:50]}{'...' if len(title) > 50 else ''}")
            if verbose:
                print(f"      linkend: {linkend}")

    if report.changes:
        print(f"\nTEXT CHANGES ({len(report.changes)}):")
        for change in report.changes:
            print(f"  [{change.rule}]")
            print(f"      Before: {change.original[:60]}{'...' if len(change.original) > 60 else ''}")
            print(f"      After:  {change.modified[:60]}{'...' if len(change.modified) > 60 else ''}")

    if report.generic_titles:
        print(f"\nGENERIC TITLES DETECTED ({len(report.generic_titles)}):")
        print("  (These may need manual review/replacement)")
        for file, linkend, title in report.generic_titles:
            print(f"  - \"{title}\" (linkend: {linkend})")

    if not report.has_changes() and not report.generic_titles and not report.errors:
        print("\n  No changes needed.")

    print()


def main():
    parser = argparse.ArgumentParser(
        description='Clean up TOC XMLs in Successful ZIP files',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    python cleanup_toc.py ./Output/Successful/           # Process all ZIPs
    python cleanup_toc.py ./Output/Successful/9781234.zip  # Single ZIP
    python cleanup_toc.py ./Output/Successful/ --dry-run   # Preview only
    python cleanup_toc.py ./Output/Successful/ -v          # Verbose output
        """
    )

    parser.add_argument(
        'path',
        type=Path,
        help='Path to Successful/ folder or individual ZIP file'
    )

    parser.add_argument(
        '--dry-run', '-n',
        action='store_true',
        help='Preview changes without modifying files'
    )

    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show detailed output'
    )

    parser.add_argument(
        '--quiet', '-q',
        action='store_true',
        help='Only show errors and summary'
    )

    args = parser.parse_args()

    if args.verbose:
        logger.setLevel(logging.DEBUG)
    elif args.quiet:
        logger.setLevel(logging.WARNING)

    path = args.path

    # Determine what to process
    if path.is_file() and path.suffix == '.zip':
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

    # Process each ZIP
    total_changes = 0
    total_reclassifications = 0
    total_generic = 0
    total_errors = 0

    for zip_file in sorted(zip_files):
        logger.info(f"Processing {zip_file.name}...")
        report = process_zip_file(zip_file, dry_run=args.dry_run)

        total_changes += len(report.changes)
        total_reclassifications += len(report.reclassifications)
        total_generic += len(report.generic_titles)
        total_errors += len(report.errors)

        if not args.quiet or report.errors:
            print_report(report, verbose=args.verbose)

    # Print summary
    print("=" * 60)
    print("SUMMARY")
    print("=" * 60)
    print(f"Files processed:     {len(zip_files)}")
    print(f"Text changes:        {total_changes}")
    print(f"Reclassifications:   {total_reclassifications}")
    print(f"Generic titles:      {total_generic}")
    print(f"Errors:              {total_errors}")

    if args.dry_run:
        print("\n*** DRY RUN - No files were modified ***")

    sys.exit(0 if total_errors == 0 else 1)


if __name__ == '__main__':
    main()
