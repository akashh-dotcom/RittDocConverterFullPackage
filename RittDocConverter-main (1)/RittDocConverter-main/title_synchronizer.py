#!/usr/bin/env python3
"""
Title Synchronizer for RittDoc XML Files

This module synchronizes section titles between chapter XMLs and TOC entries,
using the EPUB TOC structure as the source of truth for correct titles.

Key functionality:
1. Detect synthetic sect1s with generic/wrong titles (e.g., "Content", chapter title copies)
2. Match sections with EPUB TOC entries by position, ID, or content analysis
3. Update both sect1 titles in chapter XMLs AND corresponding TOC linkend titles

The problem being solved:
- comprehensive_dtd_fixer creates synthetic sect1 wrappers with placeholder titles
- These titles are often the chapter title or empty
- The actual correct titles exist in the EPUB TOC (toc_structure.json)
- This module matches them up and fixes the inconsistencies

Usage:
    python title_synchronizer.py /path/to/package.zip
    python title_synchronizer.py /path/to/package.zip --dry-run
"""

import argparse
import json
import logging
import re
import shutil
import sys
import tempfile
import zipfile
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple

from lxml import etree

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


# =============================================================================
# GENERIC TITLE PATTERNS (titles that should be replaced)
# =============================================================================

# Patterns that indicate a generic/placeholder title
GENERIC_TITLE_PATTERNS = [
    re.compile(r'^content$', re.IGNORECASE),
    re.compile(r'^section\s*\d*$', re.IGNORECASE),
    re.compile(r'^sect\d+$', re.IGNORECASE),
    re.compile(r'^chapter\s*\d*$', re.IGNORECASE),
    # NOTE: "Introduction" was previously listed here as generic, but it is a
    # legitimate and common section title in academic/medical texts.  Treating
    # it as a placeholder caused the title synchronizer to replace it with the
    # first <emphasis> text from body content (e.g., "treat", "why didn't they
    # teach us this in graduate school?").  Removed to preserve real titles.
    re.compile(r'^untitled$', re.IGNORECASE),
    re.compile(r'^$'),  # Empty title
]

# ID patterns for synthetic sections (created by comprehensive_dtd_fixer)
# Format: ch0001s0000, ch0001s0001, etc.
SYNTHETIC_SECTION_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4})(s\d{4})$')


# =============================================================================
# DATA CLASSES
# =============================================================================

@dataclass
class TitleFix:
    """Record of a title fix."""
    file: str
    element_tag: str
    element_id: str
    old_title: str
    new_title: str
    source: str  # 'epub_toc', 'content_analysis', 'bridgehead'
    line_number: Optional[int] = None


@dataclass
class TitleSyncReport:
    """Report of all title synchronization changes."""
    package_file: str
    section_fixes: List[TitleFix] = field(default_factory=list)
    toc_fixes: List[TitleFix] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)
    sections_checked: int = 0
    toc_entries_checked: int = 0

    def has_changes(self) -> bool:
        return bool(self.section_fixes) or bool(self.toc_fixes)

    def total_fixes(self) -> int:
        return len(self.section_fixes) + len(self.toc_fixes)


@dataclass
class TocEntry:
    """Parsed TOC entry from EPUB."""
    title: str
    href: str
    entry_type: str
    target_id: Optional[str] = None  # Extracted section ID
    target_file: Optional[str] = None  # Target XML file
    children: List['TocEntry'] = field(default_factory=list)
    depth: int = 1


# =============================================================================
# TOC STRUCTURE PARSING
# =============================================================================

def load_toc_structure(toc_json_path: Path) -> List[TocEntry]:
    """
    Load and parse the toc_structure.json file.

    Args:
        toc_json_path: Path to toc_structure.json

    Returns:
        List of TocEntry objects representing the TOC hierarchy
    """
    if not toc_json_path.exists():
        return []

    try:
        with open(toc_json_path, 'r', encoding='utf-8') as f:
            toc_data = json.load(f)
    except (json.JSONDecodeError, IOError) as e:
        logger.warning(f"Could not load toc_structure.json: {e}")
        return []

    def parse_entry(entry: dict, depth: int = 1) -> TocEntry:
        """Parse a single TOC entry recursively."""
        href = entry.get('href', '')
        target_file = None
        target_id = None

        # Parse href to extract file and section ID
        # Formats: "ch0001.xml#s0001", "ch0001s0001", "#s0001", etc.
        if href:
            if '#' in href:
                file_part, anchor = href.split('#', 1)
                target_file = file_part if file_part else None
                target_id = anchor
            elif href.endswith('.xml'):
                target_file = href
            else:
                # Might be just an ID
                target_id = href

        children = []
        for child in entry.get('children', []):
            children.append(parse_entry(child, depth + 1))

        return TocEntry(
            title=entry.get('title', '').strip(),
            href=href,
            entry_type=entry.get('type', 'unknown'),
            target_id=target_id,
            target_file=target_file,
            children=children,
            depth=depth
        )

    entries = []
    for entry in toc_data:
        entries.append(parse_entry(entry))

    return entries


def flatten_toc_entries(entries: List[TocEntry]) -> List[TocEntry]:
    """Flatten nested TOC entries into a single list with depth preserved."""
    result = []

    def flatten(entry_list: List[TocEntry]):
        for entry in entry_list:
            result.append(entry)
            if entry.children:
                flatten(entry.children)

    flatten(entries)
    return result


def build_toc_title_map(entries: List[TocEntry]) -> Dict[str, str]:
    """
    Build a mapping from section IDs to TOC titles.

    Args:
        entries: List of TocEntry objects

    Returns:
        Dict mapping section_id -> title
    """
    title_map = {}
    flat_entries = flatten_toc_entries(entries)

    for entry in flat_entries:
        if entry.target_id and entry.title:
            # Store by various ID formats for matching
            title_map[entry.target_id] = entry.title

            # Also store normalized versions
            normalized_id = entry.target_id.lower().replace('-', '').replace('_', '')
            title_map[normalized_id] = entry.title

    return title_map


def build_file_toc_map(entries: List[TocEntry]) -> Dict[str, List[TocEntry]]:
    """
    Build a mapping from target file to list of TOC entries.

    Args:
        entries: List of TocEntry objects

    Returns:
        Dict mapping filename -> list of entries for that file
    """
    file_map: Dict[str, List[TocEntry]] = {}
    flat_entries = flatten_toc_entries(entries)

    for entry in flat_entries:
        if entry.target_file:
            filename = entry.target_file
            if filename not in file_map:
                file_map[filename] = []
            file_map[filename].append(entry)

    return file_map


# =============================================================================
# TITLE DETECTION AND MATCHING
# =============================================================================

def is_generic_title(title: str, chapter_title: Optional[str] = None) -> bool:
    """
    Check if a title is generic/placeholder that should be replaced.

    Args:
        title: The title text to check
        chapter_title: The parent chapter's title (to detect copies)

    Returns:
        True if the title is generic and should be replaced
    """
    if not title or not title.strip():
        return True

    title_clean = title.strip()

    # Check against generic patterns
    for pattern in GENERIC_TITLE_PATTERNS:
        if pattern.match(title_clean):
            return True

    # Check if it's just copying the chapter title
    if chapter_title:
        chapter_clean = chapter_title.strip().lower()
        title_lower = title_clean.lower()

        # Exact match or very similar
        if title_lower == chapter_clean:
            return True

        # Check if it's a shortened version of chapter title
        if len(title_clean) > 5 and title_lower in chapter_clean:
            return True

    return False


def normalize_title_for_matching(title: str) -> str:
    """
    Normalize a title for fuzzy matching.

    Args:
        title: The title text

    Returns:
        Normalized title string
    """
    if not title:
        return ''

    # Lowercase
    normalized = title.lower()

    # Remove leading numbers (e.g., "1.2 Title" -> "title")
    normalized = re.sub(r'^[\d.]+\s*', '', normalized)

    # Remove special characters
    normalized = re.sub(r'[^\w\s]', '', normalized)

    # Collapse whitespace
    normalized = ' '.join(normalized.split())

    return normalized


def find_best_title_match(
    section_id: str,
    current_title: str,
    toc_entries: List[TocEntry],
    toc_title_map: Dict[str, str]
) -> Optional[Tuple[str, str]]:
    """
    Find the best matching TOC title for a section.

    Matching strategies:
    1. Direct ID match
    2. Normalized ID match
    3. Position-based match (for synthetic sections)
    4. Content similarity match

    Args:
        section_id: The section's ID
        current_title: Current title of the section
        toc_entries: List of all TOC entries
        toc_title_map: Pre-built ID -> title map

    Returns:
        Tuple of (new_title, match_source) or None if no match found
    """
    # Strategy 1: Direct ID match
    if section_id in toc_title_map:
        return (toc_title_map[section_id], 'direct_id_match')

    # Strategy 2: Normalized ID match
    normalized_id = section_id.lower().replace('-', '').replace('_', '')
    if normalized_id in toc_title_map:
        return (toc_title_map[normalized_id], 'normalized_id_match')

    # Strategy 3: Try matching by extracting base chapter ID
    # e.g., ch0001s0001 -> look for entries with target_file ch0001.xml
    match = SYNTHETIC_SECTION_ID_PATTERN.match(section_id)
    if match:
        chapter_id = match.group(1)  # e.g., 'ch0001'
        section_suffix = match.group(2)  # e.g., 's0001'

        # Look for TOC entries pointing to this chapter
        for entry in flatten_toc_entries(toc_entries):
            if entry.target_file and entry.target_file.startswith(chapter_id):
                # Check if the target_id matches our section suffix pattern
                if entry.target_id:
                    # Direct match with target_id
                    if entry.target_id == section_id or entry.target_id.endswith(section_suffix):
                        return (entry.title, 'chapter_section_match')

    # Strategy 4: Fuzzy title match (if current title is similar to any TOC entry)
    if current_title and len(current_title.strip()) > 3:
        current_normalized = normalize_title_for_matching(current_title)
        if current_normalized:
            for entry in flatten_toc_entries(toc_entries):
                entry_normalized = normalize_title_for_matching(entry.title)
                if entry_normalized and len(entry_normalized) > 3:
                    # Check for substring match
                    if current_normalized in entry_normalized or entry_normalized in current_normalized:
                        # Return the TOC version as it's likely more complete
                        if len(entry.title) > len(current_title):
                            return (entry.title, 'fuzzy_match')

    return None


# =============================================================================
# CONTENT ANALYSIS FOR TITLE EXTRACTION
# =============================================================================

def extract_title_from_content(section: etree._Element) -> Optional[str]:
    """
    Extract a potential title from section content.

    Looks for:
    1. Bridgehead elements
    2. Bold/emphasis at start of first para
    3. Formalpara title

    Args:
        section: The section element

    Returns:
        Extracted title or None
    """
    # Check for bridgehead
    bridgehead = section.find('.//bridgehead')
    if bridgehead is not None:
        text = ''.join(bridgehead.itertext()).strip()
        if text:
            return text

    # Check for formalpara with title
    formalpara = section.find('.//formalpara')
    if formalpara is not None:
        fp_title = formalpara.find('title')
        if fp_title is not None:
            text = ''.join(fp_title.itertext()).strip()
            if text:
                return text

    # Check first para for bold/emphasis at start
    first_para = section.find('para')
    if first_para is not None:
        # Check if first child is emphasis or bold
        for child in first_para:
            if child.tag in ('emphasis', 'bold', 'strong'):
                text = ''.join(child.itertext()).strip()
                # Only if it looks like a title (short, no ending punctuation)
                if text and len(text) < 100 and not text.endswith(('.', ',', ';', ':')):
                    return text
            break  # Only check first element

    return None


# =============================================================================
# MAIN PROCESSING FUNCTIONS
# =============================================================================

def process_chapter_file(
    xml_path: Path,
    toc_entries: List[TocEntry],
    toc_title_map: Dict[str, str],
    report: TitleSyncReport,
    dry_run: bool = False
) -> bool:
    """
    Process a single chapter XML file to fix section titles.

    Args:
        xml_path: Path to the chapter XML file
        toc_entries: TOC entries from EPUB
        toc_title_map: ID -> title mapping
        report: Report to add fixes to
        dry_run: If True, don't modify files

    Returns:
        True if any changes were made
    """
    try:
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(str(xml_path), parser)
        root = tree.getroot()
    except etree.XMLSyntaxError as e:
        report.errors.append(f"Failed to parse {xml_path.name}: {e}")
        return False

    filename = xml_path.name
    modified = False

    # Get chapter title for comparison
    chapter_title = None
    root_title = root.find('title')
    if root_title is not None:
        chapter_title = ''.join(root_title.itertext()).strip()

    # Find all sect1 elements
    for sect1 in root.iter('sect1'):
        report.sections_checked += 1

        sect1_id = sect1.get('id', '')
        title_elem = sect1.find('title')

        if title_elem is None:
            continue

        current_title = ''.join(title_elem.itertext()).strip()
        line_num = title_elem.sourceline if hasattr(title_elem, 'sourceline') else None

        # Check if this is a generic/placeholder title
        if is_generic_title(current_title, chapter_title):
            # Track whether this is just a chapter-title copy.
            matches_chapter_title = False
            if chapter_title:
                chapter_clean = chapter_title.strip().lower()
                title_clean = current_title.strip().lower()
                if title_clean == chapter_clean:
                    matches_chapter_title = True
                elif len(current_title.strip()) > 5 and title_clean in chapter_clean:
                    matches_chapter_title = True

            # Try to find better title from TOC
            match_result = find_best_title_match(
                sect1_id, current_title, toc_entries, toc_title_map
            )

            if match_result:
                new_title, source = match_result

                # Don't replace with another generic title
                if not is_generic_title(new_title):
                    report.section_fixes.append(TitleFix(
                        file=filename,
                        element_tag='sect1',
                        element_id=sect1_id,
                        old_title=current_title or '(empty)',
                        new_title=new_title,
                        source=source,
                        line_number=line_num
                    ))

                    if not dry_run:
                        # Clear existing title content
                        title_elem.text = new_title
                        for child in list(title_elem):
                            title_elem.remove(child)

                    modified = True
                    continue

            # If this title is just copying the chapter title and we don't have
            # a TOC match, keep it. Synthetic wrapper sections should retain the
            # chapter title instead of deriving a title from drop-caps or content.
            if matches_chapter_title:
                continue

            # Fallback: Try to extract title from content
            content_title = extract_title_from_content(sect1)
            if content_title and not is_generic_title(content_title):
                report.section_fixes.append(TitleFix(
                    file=filename,
                    element_tag='sect1',
                    element_id=sect1_id,
                    old_title=current_title or '(empty)',
                    new_title=content_title,
                    source='content_analysis',
                    line_number=line_num
                ))

                if not dry_run:
                    title_elem.text = content_title
                    for child in list(title_elem):
                        title_elem.remove(child)

                modified = True

    # Write changes if not dry run
    if modified and not dry_run:
        tree.write(str(xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)

    return modified


def process_book_xml_toc(
    book_xml_path: Path,
    section_fixes: List[TitleFix],
    report: TitleSyncReport,
    dry_run: bool = False
) -> bool:
    """
    Update TOC entries in Book.XML to match fixed section titles.

    Args:
        book_xml_path: Path to Book.XML
        section_fixes: List of section title fixes already applied
        report: Report to add TOC fixes to
        dry_run: If True, don't modify files

    Returns:
        True if any changes were made
    """
    if not book_xml_path.exists():
        return False

    try:
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(str(book_xml_path), parser)
        root = tree.getroot()
    except etree.XMLSyntaxError as e:
        report.errors.append(f"Failed to parse Book.XML: {e}")
        return False

    # Build a map of section_id -> new_title from fixes
    fix_map = {fix.element_id: fix.new_title for fix in section_fixes}

    modified = False

    # Find TOC element
    toc = root.find('.//toc')
    if toc is None:
        return False

    # Process all TOC entries with linkend
    toc_elements = ['tocentry', 'tocfront', 'tocback', 'tocchap', 'tocpart',
                    'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5']

    for elem in toc.iter():
        if not isinstance(elem.tag, str):
            continue

        if elem.tag not in toc_elements:
            continue

        report.toc_entries_checked += 1

        linkend = elem.get('linkend')
        if not linkend:
            continue

        # Check if we have a fix for this linkend
        if linkend in fix_map:
            new_title = fix_map[linkend]

            # Get current TOC entry text
            # TOC entries can have text directly or in a ulink
            current_text = None
            ulink = elem.find('ulink')

            if ulink is not None:
                current_text = ''.join(ulink.itertext()).strip()
            else:
                current_text = ''.join(elem.itertext()).strip()

            # Only fix if different
            if current_text != new_title:
                line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

                report.toc_fixes.append(TitleFix(
                    file='Book.XML',
                    element_tag=elem.tag,
                    element_id=linkend,
                    old_title=current_text or '(empty)',
                    new_title=new_title,
                    source='section_sync',
                    line_number=line_num
                ))

                if not dry_run:
                    if ulink is not None:
                        # Update ulink text
                        ulink.text = new_title
                        for child in list(ulink):
                            ulink.remove(child)
                    else:
                        # Update element text directly
                        elem.text = new_title
                        for child in list(elem):
                            if child.tag != 'ulink':
                                elem.remove(child)

                modified = True

    # Write changes if not dry run
    if modified and not dry_run:
        tree.write(str(book_xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)

    return modified


def process_zip_package(
    zip_path: Path,
    output_path: Optional[Path] = None,
    dry_run: bool = False
) -> TitleSyncReport:
    """
    Process a ZIP package to synchronize titles.

    Args:
        zip_path: Path to the input ZIP package
        output_path: Path for output ZIP (if None, modifies in place)
        dry_run: If True, don't modify any files

    Returns:
        TitleSyncReport with all changes made
    """
    report = TitleSyncReport(package_file=str(zip_path))

    if not zip_path.exists():
        report.errors.append(f"ZIP file not found: {zip_path}")
        return report

    with tempfile.TemporaryDirectory() as temp_dir:
        temp_path = Path(temp_dir)

        # Extract ZIP
        try:
            with zipfile.ZipFile(zip_path, 'r') as zf:
                zf.extractall(temp_path)
        except zipfile.BadZipFile as e:
            report.errors.append(f"Failed to extract ZIP: {e}")
            return report

        # Load TOC structure if available
        toc_json_path = temp_path / 'toc_structure.json'
        toc_entries = load_toc_structure(toc_json_path)
        toc_title_map = build_toc_title_map(toc_entries)

        if not toc_entries:
            logger.info("No toc_structure.json found, using content analysis only")

        # Find all chapter XML files
        xml_files = []
        for pattern in ['ch*.xml', 'pr*.xml', 'ap*.xml', 'dd*.xml']:
            xml_files.extend(temp_path.glob(pattern))

        logger.info(f"Processing {len(xml_files)} chapter files...")

        # Process each chapter file
        any_modified = False
        for xml_file in sorted(xml_files):
            if process_chapter_file(xml_file, toc_entries, toc_title_map, report, dry_run):
                any_modified = True

        # Update Book.XML TOC entries to match
        book_xml_path = temp_path / 'Book.XML'
        if not book_xml_path.exists():
            book_xml_path = temp_path / 'book.xml'

        if book_xml_path.exists() and report.section_fixes:
            if process_book_xml_toc(book_xml_path, report.section_fixes, report, dry_run):
                any_modified = True

        # Repackage ZIP if changes were made and not dry run
        if any_modified and not dry_run:
            if output_path is None:
                output_path = zip_path

            new_zip_path = temp_path / 'output.zip'

            with zipfile.ZipFile(new_zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in temp_path.rglob('*'):
                    if file_path.is_file() and file_path.name != 'output.zip':
                        arcname = file_path.relative_to(temp_path)
                        zf.write(file_path, arcname)

            shutil.move(str(new_zip_path), str(output_path))
            logger.info(f"Updated package: {output_path}")

    return report


def print_report(report: TitleSyncReport, verbose: bool = False) -> None:
    """Print a title sync report to the console."""
    print(f"\n{'=' * 70}")
    print(f"Title Synchronization Report: {Path(report.package_file).name}")
    print(f"{'=' * 70}")

    print(f"\nSections checked: {report.sections_checked}")
    print(f"TOC entries checked: {report.toc_entries_checked}")

    if report.errors:
        print(f"\nERRORS ({len(report.errors)}):")
        for error in report.errors:
            print(f"  - {error}")

    if report.section_fixes:
        print(f"\nSECTION TITLE FIXES ({len(report.section_fixes)}):")
        for fix in report.section_fixes[:20]:
            print(f"  [{fix.file}] {fix.element_id}")
            print(f"    Old: \"{fix.old_title[:50]}{'...' if len(fix.old_title) > 50 else ''}\"")
            print(f"    New: \"{fix.new_title[:50]}{'...' if len(fix.new_title) > 50 else ''}\"")
            print(f"    Source: {fix.source}")
        if len(report.section_fixes) > 20:
            print(f"  ... and {len(report.section_fixes) - 20} more")

    if report.toc_fixes:
        print(f"\nTOC ENTRY FIXES ({len(report.toc_fixes)}):")
        for fix in report.toc_fixes[:10]:
            print(f"  [{fix.element_tag}] linkend='{fix.element_id}'")
            print(f"    Old: \"{fix.old_title[:50]}{'...' if len(fix.old_title) > 50 else ''}\"")
            print(f"    New: \"{fix.new_title[:50]}{'...' if len(fix.new_title) > 50 else ''}\"")
        if len(report.toc_fixes) > 10:
            print(f"  ... and {len(report.toc_fixes) - 10} more")

    if not report.has_changes() and not report.errors:
        print("\n  No title synchronization needed.")

    print(f"\nSUMMARY:")
    print(f"  Section title fixes: {len(report.section_fixes)}")
    print(f"  TOC entry fixes:     {len(report.toc_fixes)}")
    print(f"  Total fixes:         {report.total_fixes()}")
    print()


# =============================================================================
# CLI ENTRY POINT
# =============================================================================

def main():
    parser = argparse.ArgumentParser(
        description='Synchronize section titles with EPUB TOC structure',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
This tool fixes generic/placeholder titles in synthetic sect1 elements by
matching them with the correct titles from the EPUB TOC structure.

Common issues fixed:
  - Sections titled "Content" (generic placeholder)
  - Sections with chapter title copies (wrong level)
  - Empty section titles
  - Sections with sect2/sect3 headings used as sect1 titles

Examples:
    python title_synchronizer.py ./Output/Successful/9781234.zip
    python title_synchronizer.py ./Output/Successful/9781234.zip --dry-run
    python title_synchronizer.py ./Output/Successful/ -v
        """
    )

    parser.add_argument(
        'path',
        type=Path,
        help='Path to ZIP package or directory containing ZIP files'
    )

    parser.add_argument(
        '-o', '--output',
        type=Path,
        help='Output path for fixed package (default: modify in place)'
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

    # Process each ZIP
    total_section_fixes = 0
    total_toc_fixes = 0
    total_errors = 0

    for zip_file in sorted(zip_files):
        logger.info(f"Processing {zip_file.name}...")
        output_path = args.output if len(zip_files) == 1 else None
        report = process_zip_package(zip_file, output_path, dry_run=args.dry_run)

        total_section_fixes += len(report.section_fixes)
        total_toc_fixes += len(report.toc_fixes)
        total_errors += len(report.errors)

        if not args.quiet or report.errors:
            print_report(report, verbose=args.verbose)

    # Print overall summary
    print("=" * 70)
    print("OVERALL SUMMARY")
    print("=" * 70)
    print(f"Files processed:       {len(zip_files)}")
    print(f"Section title fixes:   {total_section_fixes}")
    print(f"TOC entry fixes:       {total_toc_fixes}")
    print(f"Total fixes:           {total_section_fixes + total_toc_fixes}")
    print(f"Errors:                {total_errors}")

    if args.dry_run:
        print("\n*** DRY RUN - No files were modified ***")

    sys.exit(0 if total_errors == 0 else 1)


if __name__ == '__main__':
    main()
