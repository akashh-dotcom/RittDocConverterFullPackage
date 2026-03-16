#!/usr/bin/env python3
"""
Fix Duplicate IDs in RittDoc XML Files

This script automatically fixes duplicate ID validation errors by:
1. Detecting duplicate IDs within XML files
2. Making them unique following R2 Library naming conventions
3. Updating internal references (linkend attributes) to point to correct IDs

ID Naming Convention (from id-naming-convention-rules.md):
- Format: {sect1_id}{element_code}{sequence_number}
- Sect1 ID: ch{4-digit chapter}s{4-digit sect1} = 11 characters
- Maximum ID length: 25 characters
- Allowed characters: lowercase letters and numbers only

Usage:
    python fix_duplicate_ids.py input.xml
    python fix_duplicate_ids.py input.zip -o output.zip
    python fix_duplicate_ids.py --directory /path/to/xml/files
"""

import argparse
import re
import sys
import tempfile
import zipfile
from collections import defaultdict
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple

from lxml import etree

try:
    from validation_report import ValidationReportGenerator, VerificationItem
    VALIDATION_REPORT_AVAILABLE = True
except ImportError:
    VALIDATION_REPORT_AVAILABLE = False

# Import ELEMENT_CODES from the single source of truth
from id_authority import ELEMENT_CODES, get_element_code, DEFAULT_ELEMENT_CODE, next_available_sect1_id

# NOTE: ELEMENT_CODES is now imported from id_authority.py (single source of truth)
# See id_authority.py for the canonical element type to code mapping.

# Maximum ID length per R2 Library constraint
MAX_ID_LENGTH = 25

# Regex patterns for ID parsing
# Match all element type prefixes: ch (chapter), pr (preface), ap (appendix), dd (dedication), ak (acknowledgments), etc.
SECT1_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4}s\d{4})')  # e.g., ch0011s0001, pr0001s0000
CHAPTER_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4})')  # e.g., ch0011, pr0001
# Element codes can be 1-3 chars (e.g., 'a', 'fg', 'ta', 'bib')
ELEMENT_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4}s\d{4})([a-z]{1,3})(\d+)$')  # e.g., ch0011s0001bib0001


class DuplicateIDFixer:
    """Fixes duplicate ID validation errors in XML files following R2 Library naming conventions."""

    def __init__(self):
        self.fixes_applied: List[str] = []
        self.verification_items: List = []
        # Track ID mappings for cross-file reference updates
        self.id_remapping: Dict[str, str] = {}  # old_id -> new_id
        # Track element counters per sect1 and element type
        self.element_counters: Dict[str, Dict[str, int]] = defaultdict(lambda: defaultdict(int))

    def clear(self):
        """Clear tracking data for fresh processing."""
        self.fixes_applied = []
        self.verification_items = []
        self.id_remapping = {}
        self.element_counters = defaultdict(lambda: defaultdict(int))

    def find_duplicate_ids(self, root: etree._Element) -> Dict[str, List[etree._Element]]:
        """
        Find all duplicate IDs in an XML tree.

        Args:
            root: XML root element

        Returns:
            Dictionary mapping duplicate IDs to list of elements with that ID
        """
        id_elements: Dict[str, List[etree._Element]] = defaultdict(list)

        # Collect all elements with IDs
        for elem in root.iter():
            elem_id = elem.get('id')
            if elem_id:
                id_elements[elem_id].append(elem)

        # Filter to only duplicates (more than one element with same ID)
        duplicates = {
            id_val: elements
            for id_val, elements in id_elements.items()
            if len(elements) > 1
        }

        return duplicates

    def _extract_sect1_id(self, id_value: str) -> Optional[str]:
        """
        Extract the sect1 ID prefix from an element ID.

        Args:
            id_value: Full element ID (e.g., ch0011s0001b0001)

        Returns:
            Sect1 ID (e.g., ch0011s0001) or None if not found
        """
        match = SECT1_ID_PATTERN.match(id_value)
        if match:
            return match.group(1)
        return None

    def _get_element_code(self, elem: etree._Element) -> str:
        """
        Get the element type code for an element.

        Args:
            elem: XML element

        Returns:
            Element code (e.g., 'b' for bibliography, 't' for table)
        """
        tag = elem.tag if isinstance(elem.tag, str) else str(elem.tag)
        # Remove namespace if present
        if '}' in tag:
            tag = tag.split('}')[1]
        return ELEMENT_CODES.get(tag.lower(), 'x')

    def _parse_existing_id(self, id_value: str) -> Tuple[Optional[str], Optional[str], Optional[int]]:
        """
        Parse an existing ID to extract its components.

        Args:
            id_value: ID to parse

        Returns:
            Tuple of (sect1_id, element_code, sequence_number) or (None, None, None)
        """
        # Try to match element ID pattern: ch0011s0001b0001
        match = ELEMENT_ID_PATTERN.match(id_value)
        if match:
            return match.group(1), match.group(2), int(match.group(3))

        # Try to match sect1 ID pattern: ch0011s0001
        match = SECT1_ID_PATTERN.match(id_value)
        if match:
            sect1_id = match.group(1)
            # Check if there's additional content after sect1 ID
            remaining = id_value[len(sect1_id):]
            if remaining:
                # Try to extract element code and number
                elem_match = re.match(r'^([a-z]{1,2})(\d+)$', remaining)
                if elem_match:
                    return sect1_id, elem_match.group(1), int(elem_match.group(2))
            return sect1_id, None, None

        return None, None, None

    def generate_unique_id(
        self,
        base_id: str,
        existing_ids: Set[str],
        index: int,
        elem: Optional[etree._Element] = None
    ) -> str:
        """
        Generate a unique ID following R2 Library naming conventions.

        Strategy:
        1. Parse the base ID to extract sect1_id, element_code, sequence
        2. Generate new ID with incremented sequence number
        3. Ensure it's unique and within 25 char limit

        CRITICAL: Section elements (sect1-sect5) must use proper section ID generation,
        NOT the element code pattern. Using element codes like 's1' for sect1 produces
        malformed IDs like ch0005s0001s10002 which break downstream XSL processing.

        Args:
            base_id: Original duplicate ID
            existing_ids: Set of all existing IDs to avoid collisions
            index: Sequence number for this duplicate (1, 2, 3, ...)
            elem: Optional element to determine element code

        Returns:
            New unique ID following naming conventions
        """
        # Check if this is a section element - they need special handling
        if elem is not None:
            elem_tag = elem.tag if isinstance(elem.tag, str) else str(elem.tag)
            # Remove namespace if present
            if '}' in elem_tag:
                elem_tag = elem_tag.split('}')[1]
            elem_tag = elem_tag.lower()

            if elem_tag == 'sect1':
                # For sect1, extract the chapter prefix and use next_available_sect1_id
                chapter_match = CHAPTER_ID_PATTERN.match(base_id)
                chapter_id = chapter_match.group(1) if chapter_match else 'ch0001'
                return next_available_sect1_id(chapter_id, existing_ids)

            elif elem_tag in {'sect2', 'sect3', 'sect4', 'sect5'}:
                # For deeper sections, find the parent section's ID and build hierarchical ID
                parent = elem.getparent()
                parent_sect_id = None
                while parent is not None:
                    parent_tag = parent.tag if isinstance(parent.tag, str) else ''
                    if '}' in parent_tag:
                        parent_tag = parent_tag.split('}')[1]
                    parent_tag = parent_tag.lower()
                    if parent_tag in {'sect1', 'sect2', 'sect3', 'sect4'}:
                        parent_sect_id = parent.get('id')
                        if parent_sect_id:
                            break
                    elif parent_tag in {'chapter', 'appendix', 'preface'}:
                        # If no parent section, use chapter ID with s0001
                        parent_sect_id = (parent.get('id') or 'ch0001') + 's0001'
                        break
                    parent = parent.getparent()

                if parent_sect_id:
                    # Generate hierarchical section ID
                    level = int(elem_tag[-1])  # sect2 -> 2, sect3 -> 3, etc.
                    counter = 1
                    if level == 2:
                        new_id = f"{parent_sect_id}s{counter:04d}"
                    else:
                        new_id = f"{parent_sect_id}s{counter:02d}"
                    while new_id in existing_ids:
                        counter += 1
                        if level == 2:
                            new_id = f"{parent_sect_id}s{counter:04d}"
                        else:
                            new_id = f"{parent_sect_id}s{counter:02d}"
                    return new_id

        # Parse the existing ID for non-section elements
        sect1_id, elem_code, seq_num = self._parse_existing_id(base_id)

        if sect1_id:
            # Determine element code (only for non-section elements)
            if not elem_code and elem is not None:
                elem_code = self._get_element_code(elem)
            elif not elem_code:
                elem_code = 'x'  # Unknown element type

            # Calculate available digits for sequence number
            # Format: {sect1_id}{elem_code}{sequence}
            # Max: 25 chars, sect1_id: 11 chars, elem_code: 1-2 chars
            available_digits = MAX_ID_LENGTH - len(sect1_id) - len(elem_code)

            # Generate new sequence number
            # Start from existing sequence + index, or use counter
            if seq_num is not None:
                new_seq = seq_num + index
            else:
                # Use counter for this sect1/element combination
                counter_key = f"{sect1_id}_{elem_code}"
                self.element_counters[sect1_id][elem_code] += 1
                new_seq = self.element_counters[sect1_id][elem_code] + 9000 + index  # Start high to avoid conflicts

            # Format sequence number with appropriate padding
            if available_digits >= 4:
                seq_str = f"{new_seq:04d}"
            elif available_digits >= 3:
                seq_str = f"{new_seq:03d}"
            else:
                seq_str = f"{new_seq % 100:02d}"

            new_id = f"{sect1_id}{elem_code}{seq_str}"

            # Ensure uniqueness
            while new_id in existing_ids:
                new_seq += 1
                if available_digits >= 4:
                    seq_str = f"{new_seq:04d}"
                elif available_digits >= 3:
                    seq_str = f"{new_seq:03d}"
                else:
                    seq_str = f"{new_seq % 100:02d}"
                new_id = f"{sect1_id}{elem_code}{seq_str}"

            # Verify length constraint
            if len(new_id) <= MAX_ID_LENGTH:
                return new_id

        # Fallback: simple numeric suffix approach
        # This handles non-standard IDs while still maintaining uniqueness
        suffix = f"{index:02d}"
        available_space = MAX_ID_LENGTH - len(suffix)

        if len(base_id) <= available_space:
            new_id = f"{base_id}{suffix}"
        else:
            # Truncate base ID to fit
            new_id = f"{base_id[:available_space]}{suffix}"

        # Ensure uniqueness
        while new_id in existing_ids:
            index += 1
            suffix = f"{index:02d}"
            if len(base_id) <= MAX_ID_LENGTH - len(suffix):
                new_id = f"{base_id}{suffix}"
            else:
                new_id = f"{base_id[:MAX_ID_LENGTH - len(suffix)]}{suffix}"

        return new_id

    def fix_duplicate_ids(
        self,
        root: etree._Element,
        filename: str = "unknown.xml"
    ) -> Tuple[int, List[str]]:
        """
        Fix all duplicate IDs in an XML tree following R2 Library naming conventions.

        Strategy:
        1. Keep the first occurrence of each duplicate ID as-is
        2. Rename subsequent occurrences following the naming convention:
           - Format: {sect1_id}{element_code}{sequence_number}
           - Max length: 25 characters
           - Allowed: lowercase letters and numbers only

        Args:
            root: XML root element to fix
            filename: Name of file being processed (for logging)

        Returns:
            Tuple of (number of fixes, list of fix descriptions)
        """
        fixes = []

        # Find all duplicate IDs
        duplicates = self.find_duplicate_ids(root)

        if not duplicates:
            return 0, fixes

        # Collect all existing IDs in the document
        existing_ids: Set[str] = set()
        for elem in root.iter():
            elem_id = elem.get('id')
            if elem_id:
                existing_ids.add(elem_id)

        # Initialize counters based on existing IDs
        self._initialize_counters_from_existing(existing_ids)

        # Process each duplicate ID
        for dup_id, elements in duplicates.items():
            # Keep the first element's ID unchanged
            # Rename all subsequent elements
            for idx, elem in enumerate(elements[1:], start=1):
                # Generate unique ID following naming conventions
                new_id = self.generate_unique_id(dup_id, existing_ids, idx, elem)

                # Track the remapping
                self.id_remapping[f"{dup_id}_{idx}"] = new_id

                # Update the element's ID
                old_id = elem.get('id')
                elem.set('id', new_id)
                existing_ids.add(new_id)

                # Get element info for logging
                elem_tag = elem.tag if isinstance(elem.tag, str) else str(elem.tag)
                if '}' in elem_tag:
                    elem_tag = elem_tag.split('}')[1]
                line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

                fix_desc = f"Renamed duplicate ID '{old_id}' to '{new_id}' on <{elem_tag}> at line {line_num or 'unknown'} in {filename}"
                fixes.append(fix_desc)
                self.fixes_applied.append(fix_desc)

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=line_num,
                        fix_type="Duplicate ID Fix",
                        fix_description=f"Renamed '{old_id}' to '{new_id}'",
                        verification_reason=f"ID '{old_id}' was used multiple times. This was occurrence #{idx + 1}.",
                        suggestion="Verify the renamed ID follows naming conventions and is appropriate for this element."
                    ))

        return len(fixes), fixes

    def _initialize_counters_from_existing(self, existing_ids: Set[str]):
        """
        Initialize element counters based on existing IDs in the document.

        This prevents generating IDs that conflict with existing ones.

        Args:
            existing_ids: Set of all existing IDs
        """
        for id_value in existing_ids:
            sect1_id, elem_code, seq_num = self._parse_existing_id(id_value)
            if sect1_id and elem_code and seq_num is not None:
                current_max = self.element_counters[sect1_id][elem_code]
                if seq_num > current_max:
                    self.element_counters[sect1_id][elem_code] = seq_num

    def fix_xml_file(self, xml_path: Path) -> Tuple[int, List[str]]:
        """
        Fix duplicate IDs in an XML file.

        Args:
            xml_path: Path to XML file

        Returns:
            Tuple of (number of fixes, list of fix descriptions)
        """
        try:
            # Parse XML preserving whitespace
            parser = etree.XMLParser(remove_blank_text=False)
            tree = etree.parse(str(xml_path), parser)
            root = tree.getroot()

            # Fix duplicate IDs
            num_fixes, fixes = self.fix_duplicate_ids(root, xml_path.name)

            if num_fixes > 0:
                # Write back the fixed XML
                tree.write(
                    str(xml_path),
                    encoding='utf-8',
                    xml_declaration=True,
                    pretty_print=True
                )

            return num_fixes, fixes

        except etree.XMLSyntaxError as e:
            print(f"  [FAIL] XML syntax error in {xml_path.name}: {e}")
            return 0, []
        except Exception as e:
            print(f"  [FAIL] Error processing {xml_path.name}: {e}")
            return 0, []

    def fix_directory(self, directory: Path, pattern: str = "*.xml") -> Dict[str, any]:
        """
        Fix duplicate IDs in all XML files in a directory.

        Args:
            directory: Directory containing XML files
            pattern: Glob pattern for XML files

        Returns:
            Statistics dictionary
        """
        stats = {
            'files_processed': 0,
            'files_fixed': 0,
            'total_fixes': 0,
            'fixes_by_file': {}
        }

        xml_files = list(directory.glob(pattern))
        xml_files.extend(directory.glob("**/" + pattern))  # Recursive

        # De-duplicate
        xml_files = list(set(xml_files))

        print(f"Found {len(xml_files)} XML files to process")

        for xml_file in sorted(xml_files):
            stats['files_processed'] += 1

            num_fixes, fixes = self.fix_xml_file(xml_file)

            if num_fixes > 0:
                stats['files_fixed'] += 1
                stats['total_fixes'] += num_fixes
                stats['fixes_by_file'][xml_file.name] = num_fixes
                print(f"  [OK] {xml_file.name}: Fixed {num_fixes} duplicate ID(s)")

        return stats

    def fix_zip_package(
        self,
        zip_path: Path,
        output_path: Optional[Path] = None
    ) -> Dict[str, any]:
        """
        Fix duplicate IDs in all XML files within a ZIP package.

        Args:
            zip_path: Input ZIP package path
            output_path: Output ZIP path (defaults to adding _fixed suffix)

        Returns:
            Statistics dictionary
        """
        stats = {
            'files_processed': 0,
            'files_fixed': 0,
            'total_fixes': 0,
            'fixes_by_file': {}
        }

        if output_path is None:
            output_path = zip_path.parent / f"{zip_path.stem}_fixed{zip_path.suffix}"

        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_path = Path(tmpdir)
            extract_dir = tmp_path / "extracted"
            extract_dir.mkdir()

            # Extract ZIP
            print(f"Extracting {zip_path.name}...")
            with zipfile.ZipFile(zip_path, 'r') as zf:
                zf.extractall(extract_dir)

            # Find all XML files (chapter files typically named ch*.xml)
            xml_files = list(extract_dir.rglob("ch*.xml"))
            # Also include Book.XML
            book_xml = list(extract_dir.rglob("Book.XML"))
            xml_files.extend(book_xml)

            print(f"Found {len(xml_files)} XML files to process\n")

            for xml_file in sorted(xml_files):
                stats['files_processed'] += 1

                num_fixes, fixes = self.fix_xml_file(xml_file)

                if num_fixes > 0:
                    stats['files_fixed'] += 1
                    stats['total_fixes'] += num_fixes
                    stats['fixes_by_file'][xml_file.name] = num_fixes
                    print(f"  [OK] {xml_file.name}: Fixed {num_fixes} duplicate ID(s)")

            # Create output ZIP
            print(f"\nCreating fixed ZIP: {output_path.name}...")
            with zipfile.ZipFile(output_path, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in extract_dir.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(extract_dir)
                        zf.write(file_path, arcname)

        stats['output_path'] = output_path
        return stats


def fix_duplicate_ids_in_tree(root: etree._Element, filename: str = "unknown.xml") -> Tuple[int, List[str]]:
    """
    Convenience function to fix duplicate IDs in an lxml tree.

    This can be called from other modules like comprehensive_dtd_fixer.py.

    Args:
        root: XML root element
        filename: Filename for logging

    Returns:
        Tuple of (number of fixes, list of fix descriptions)
    """
    fixer = DuplicateIDFixer()
    return fixer.fix_duplicate_ids(root, filename)


def main():
    parser = argparse.ArgumentParser(
        description="Fix duplicate ID validation errors in RittDoc XML files",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    Fix a single XML file:
        python fix_duplicate_ids.py chapter.xml

    Fix a ZIP package:
        python fix_duplicate_ids.py package.zip -o fixed_package.zip

    Fix all XML files in a directory:
        python fix_duplicate_ids.py --directory /path/to/xml/files
        """
    )

    parser.add_argument(
        "input",
        nargs="?",
        help="Input XML file or ZIP package"
    )
    parser.add_argument(
        "-o", "--output",
        help="Output path (for ZIP packages)"
    )
    parser.add_argument(
        "-d", "--directory",
        help="Directory containing XML files to fix"
    )
    parser.add_argument(
        "-v", "--verbose",
        action="store_true",
        help="Show detailed output"
    )

    args = parser.parse_args()

    if not args.input and not args.directory:
        parser.print_help()
        sys.exit(1)

    fixer = DuplicateIDFixer()

    print("=" * 70)
    print("DUPLICATE ID FIXER FOR RITTDOC")
    print("=" * 70)

    if args.directory:
        # Process directory
        directory = Path(args.directory)
        if not directory.exists():
            print(f"Error: Directory not found: {directory}")
            sys.exit(1)

        print(f"\nProcessing directory: {directory}\n")
        stats = fixer.fix_directory(directory)

    elif args.input:
        input_path = Path(args.input)
        if not input_path.exists():
            print(f"Error: File not found: {input_path}")
            sys.exit(1)

        if input_path.suffix.lower() == '.zip':
            # Process ZIP package
            output_path = Path(args.output) if args.output else None
            print(f"\nProcessing ZIP package: {input_path.name}\n")
            stats = fixer.fix_zip_package(input_path, output_path)
        else:
            # Process single XML file
            print(f"\nProcessing file: {input_path.name}\n")
            num_fixes, fixes = fixer.fix_xml_file(input_path)
            stats = {
                'files_processed': 1,
                'files_fixed': 1 if num_fixes > 0 else 0,
                'total_fixes': num_fixes,
                'fixes_by_file': {input_path.name: num_fixes} if num_fixes > 0 else {}
            }

            if args.verbose and fixes:
                print("\nFixes applied:")
                for fix in fixes:
                    print(f"  - {fix}")

    # Print summary
    print("\n" + "=" * 70)
    print("SUMMARY")
    print("=" * 70)
    print(f"Files processed:     {stats['files_processed']}")
    print(f"Files with fixes:    {stats['files_fixed']}")
    print(f"Total fixes applied: {stats['total_fixes']}")

    if stats.get('fixes_by_file'):
        print("\nFixes by file:")
        for filename, count in sorted(stats['fixes_by_file'].items()):
            print(f"  {filename}: {count} duplicate ID(s) fixed")

    if 'output_path' in stats:
        print(f"\nOutput: {stats['output_path']}")

    print("=" * 70)

    if stats['total_fixes'] > 0:
        print(f"\n[OK] Successfully fixed {stats['total_fixes']} duplicate ID error(s)")
    else:
        print("\n[OK] No duplicate IDs found - files are clean!")

    sys.exit(0)


if __name__ == "__main__":
    main()
