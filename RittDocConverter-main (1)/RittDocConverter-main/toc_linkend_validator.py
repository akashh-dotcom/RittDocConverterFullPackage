#!/usr/bin/env python3
"""
TOC Linkend Validator for RittDoc XML Files

This module validates that TOC linkend attributes reference valid targets:
1. The first 11 characters (base ID) must correspond to an existing file
2. The full ID must correspond to an actual element in that file

ID Format Requirements (from R2_LINKEND_AND_TOC_RULESET.md):
- Base ID (11 chars): {prefix}{4-digits}s{4-digits}
  Examples: ch0001s0001, pr0001s0000, ap0001s0001
- Full ID: {base_id}{element_code}{sequence}
  Examples: ch0001s0001fg0001, ch0001s0001ta0002

File Resolution:
- ch0001s0001 -> ch0001.xml
- pr0001s0000 -> pr0001.xml
- ap0001s0001 -> ap0001.xml

Usage:
    python toc_linkend_validator.py /path/to/package.zip
    python toc_linkend_validator.py /path/to/package.zip --fix
"""

import argparse
import logging
import re
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
# ID PATTERN CONSTANTS
# =============================================================================

# Base ID pattern: 2-letter prefix + 4 digits + 's' + 4 digits = 11 characters
# Examples: ch0001s0001, pr0001s0000, ap0001s0001, dd0001s0000
BASE_ID_PATTERN = re.compile(r'^([a-z]{2})(\d{4})s(\d{4})')

# Full element ID pattern: base_id + element_code (1-3 chars) + sequence (digits)
# Examples: ch0001s0001fg0001, ch0001s0001ta0002, ch0001s0001bib0001
FULL_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4}s\d{4})([a-z]{1,3})(\d+)$')

# Section ID pattern: base_id or base_id with additional section levels
# Examples: ch0001s0001, ch0001s0001s0001 (sect2), ch0001s0001s0001s0001 (sect3)
SECTION_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4})(s\d{4})+$')

# Valid file prefixes that can be resolved
VALID_FILE_PREFIXES = {'ch', 'pr', 'ap', 'dd', 'ak', 'bi', 'gl', 'in', 'pt'}

# XSL-recognized element prefixes for cross-references
XSL_RECOGNIZED_PREFIXES = {'fg', 'eq', 'ta', 'gl', 'bib', 'qa', 'pr', 'vd', 'ad'}


# =============================================================================
# DATA CLASSES
# =============================================================================

@dataclass
class LinkendIssue:
    """Record of a linkend validation issue."""
    file: str
    element_tag: str
    linkend: str
    issue_type: str
    description: str
    severity: str  # 'error', 'warning'
    suggested_fix: Optional[str] = None
    line_number: Optional[int] = None


@dataclass
class ValidationReport:
    """Report of all linkend validation issues."""
    package_file: str
    issues: List[LinkendIssue] = field(default_factory=list)
    valid_count: int = 0
    invalid_count: int = 0
    files_checked: int = 0

    def add_issue(self, issue: LinkendIssue):
        self.issues.append(issue)
        self.invalid_count += 1

    def add_valid(self):
        self.valid_count += 1


# =============================================================================
# ID PARSING FUNCTIONS
# =============================================================================

def parse_base_id(linkend: str) -> Optional[Tuple[str, str, str]]:
    """
    Parse the base ID components from a linkend.

    The base ID is the first 11 characters: {prefix}{4-digits}s{4-digits}

    Args:
        linkend: The linkend attribute value

    Returns:
        Tuple of (prefix, chapter_num, section_num) or None if invalid
    """
    match = BASE_ID_PATTERN.match(linkend)
    if match:
        return (match.group(1), match.group(2), match.group(3))
    return None


def extract_file_id(linkend: str) -> Optional[str]:
    """
    Extract the file ID (chapter/section file) from a linkend.

    The file ID is the first 6 characters: {prefix}{4-digits}

    Args:
        linkend: The linkend attribute value

    Returns:
        File ID like 'ch0001' or None if invalid
    """
    base = parse_base_id(linkend)
    if base:
        prefix, chapter_num, _ = base
        return f"{prefix}{chapter_num}"
    return None


def get_expected_filename(linkend: str) -> Optional[str]:
    """
    Get the expected XML filename for a linkend.

    Args:
        linkend: The linkend attribute value

    Returns:
        Expected filename like 'ch0001.xml' or None if invalid
    """
    file_id = extract_file_id(linkend)
    if file_id:
        return f"{file_id}.xml"
    return None


def is_valid_base_id(linkend: str) -> bool:
    """
    Check if a linkend has a valid 11-character base ID.

    Args:
        linkend: The linkend attribute value

    Returns:
        True if the base ID format is valid
    """
    return parse_base_id(linkend) is not None


def is_section_id(linkend: str) -> bool:
    """
    Check if a linkend is a section ID (ends with section pattern).

    Args:
        linkend: The linkend attribute value

    Returns:
        True if the ID is a section ID
    """
    return SECTION_ID_PATTERN.match(linkend) is not None


def is_element_id(linkend: str) -> bool:
    """
    Check if a linkend is an element ID (has element code suffix).

    Args:
        linkend: The linkend attribute value

    Returns:
        True if the ID is an element ID
    """
    return FULL_ID_PATTERN.match(linkend) is not None


def detect_id_issues(linkend: str) -> List[str]:
    """
    Detect formatting issues with an ID.

    Args:
        linkend: The linkend attribute value

    Returns:
        List of issue descriptions
    """
    issues = []

    if not linkend:
        issues.append("Empty linkend value")
        return issues

    # Check for invalid characters
    if not re.match(r'^[a-z0-9]+$', linkend):
        issues.append(f"Contains invalid characters (only lowercase letters and digits allowed)")

    # Check minimum length
    if len(linkend) < 11:
        issues.append(f"Too short ({len(linkend)} chars, minimum 11 for base ID)")

    # Check maximum length
    if len(linkend) > 25:
        issues.append(f"Too long ({len(linkend)} chars, maximum 25)")

    # Check base ID pattern
    if not is_valid_base_id(linkend):
        issues.append("Does not start with valid base ID pattern (e.g., ch0001s0001)")

    # Check for missing s0000 suffix (common mistake)
    if re.match(r'^[a-z]{2}\d{4}[a-z]', linkend) and 's' not in linkend:
        issues.append("Missing 's0000' section suffix (required for all IDs)")

    return issues


# =============================================================================
# FILE AND SECTION VALIDATION
# =============================================================================

def build_id_index(extract_dir: Path) -> Tuple[Dict[str, Set[str]], Dict[str, Path]]:
    """
    Build an index of all IDs in each XML file.

    Args:
        extract_dir: Directory containing extracted XML files

    Returns:
        Tuple of:
        - Dict mapping filename to set of IDs in that file
        - Dict mapping file_id (e.g., 'ch0001') to file path
    """
    file_ids: Dict[str, Set[str]] = {}
    file_paths: Dict[str, Path] = {}

    # Find all XML files
    xml_files = list(extract_dir.rglob('*.xml'))
    xml_files.extend(list(extract_dir.rglob('*.XML')))

    for xml_path in xml_files:
        filename = xml_path.name

        # Extract file_id from filename (e.g., 'ch0001' from 'ch0001.xml')
        file_match = re.match(r'^([a-z]{2}\d{4})', filename.lower())
        if file_match:
            file_id = file_match.group(1)
            file_paths[file_id] = xml_path

        try:
            parser = etree.XMLParser(remove_blank_text=False)
            tree = etree.parse(str(xml_path), parser)
            root = tree.getroot()

            ids_in_file = set()
            for elem in root.iter():
                elem_id = elem.get('id')
                if elem_id:
                    ids_in_file.add(elem_id)

            file_ids[filename] = ids_in_file

        except Exception as e:
            logger.warning(f"Could not parse {filename}: {e}")
            file_ids[filename] = set()

    return file_ids, file_paths


def validate_linkend(linkend: str, file_ids: Dict[str, Set[str]],
                     file_paths: Dict[str, Path]) -> Tuple[bool, List[str]]:
    """
    Validate a linkend against the file and ID index.

    Checks:
    1. Base ID (first 11 chars) corresponds to an existing file
    2. Full ID exists as an element in that file

    Args:
        linkend: The linkend attribute value
        file_ids: Dict mapping filename to set of IDs
        file_paths: Dict mapping file_id to file path

    Returns:
        Tuple of (is_valid, list of issues)
    """
    issues = []

    # First check format issues
    format_issues = detect_id_issues(linkend)
    if format_issues:
        issues.extend(format_issues)

    # Get expected file
    expected_file = get_expected_filename(linkend)
    if not expected_file:
        issues.append("Cannot determine target file from linkend")
        return (False, issues)

    file_id = extract_file_id(linkend)

    # Check if file exists
    if file_id not in file_paths:
        issues.append(f"Target file '{expected_file}' does not exist")
        return (False, issues)

    # Get IDs in that file
    file_path = file_paths[file_id]
    ids_in_file = file_ids.get(file_path.name, set())

    # Check if the full ID exists in the file
    if linkend not in ids_in_file:
        # Try to find similar IDs for suggestion
        similar_ids = find_similar_ids(linkend, ids_in_file)
        issues.append(f"ID '{linkend}' not found in '{expected_file}'")
        if similar_ids:
            issues.append(f"  Similar IDs found: {', '.join(similar_ids[:3])}")

    return (len(issues) == 0, issues)


def find_similar_ids(target_id: str, ids_in_file: Set[str]) -> List[str]:
    """
    Find similar IDs that might be the intended target.

    Args:
        target_id: The ID we're looking for
        ids_in_file: Set of IDs that exist in the file

    Returns:
        List of similar IDs
    """
    similar = []

    # Extract base ID from target
    base_id = target_id[:11] if len(target_id) >= 11 else target_id

    for id_val in ids_in_file:
        # Check if same base ID
        if id_val.startswith(base_id):
            similar.append(id_val)
        # Check if similar element type
        elif len(target_id) > 11 and len(id_val) > 11:
            if target_id[11:13] == id_val[11:13]:  # Same element code
                similar.append(id_val)

    return sorted(similar)[:5]  # Return top 5


# =============================================================================
# TOC VALIDATION
# =============================================================================

def validate_toc_linkends(toc_root: etree._Element, file_ids: Dict[str, Set[str]],
                          file_paths: Dict[str, Path], report: ValidationReport,
                          file_name: str) -> None:
    """
    Validate all linkend attributes in a TOC element.

    Args:
        toc_root: The TOC element (or Book.XML root)
        file_ids: Dict mapping filename to set of IDs
        file_paths: Dict mapping file_id to file path
        report: Report to add issues to
        file_name: Name of the file being validated
    """
    # TOC elements that have linkend attributes
    toc_tags = {'tocentry', 'tocfront', 'tocback', 'tocchap', 'tocpart',
                'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5'}

    # Find all TOC elements with linkend
    for elem in toc_root.iter():
        elem_tag = elem.tag if isinstance(elem.tag, str) else ''

        linkend = elem.get('linkend')
        if not linkend:
            continue

        line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

        # Validate the linkend
        is_valid, issues = validate_linkend(linkend, file_ids, file_paths)

        if is_valid:
            report.add_valid()
        else:
            # Determine severity based on issue type
            severity = 'error' if 'does not exist' in ' '.join(issues) else 'warning'

            # Try to find a suggested fix
            suggested_fix = None
            file_id = extract_file_id(linkend)
            if file_id and file_id in file_paths:
                ids_in_file = file_ids.get(file_paths[file_id].name, set())
                similar = find_similar_ids(linkend, ids_in_file)
                if similar:
                    suggested_fix = similar[0]

            report.add_issue(LinkendIssue(
                file=file_name,
                element_tag=elem_tag,
                linkend=linkend,
                issue_type='invalid_linkend',
                description='; '.join(issues),
                severity=severity,
                suggested_fix=suggested_fix,
                line_number=line_num
            ))


def validate_all_linkends(root: etree._Element, file_ids: Dict[str, Set[str]],
                          file_paths: Dict[str, Path], report: ValidationReport,
                          file_name: str) -> None:
    """
    Validate all linkend attributes in any XML file.

    Args:
        root: Root element of the XML tree
        file_ids: Dict mapping filename to set of IDs
        file_paths: Dict mapping file_id to file path
        report: Report to add issues to
        file_name: Name of the file being validated
    """
    # Elements that commonly have linkend attributes
    linkend_tags = {'link', 'xref', 'tocentry', 'tocfront', 'tocback', 'tocchap',
                    'tocpart', 'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4',
                    'toclevel5', 'footnoteref', 'glossterm'}

    for elem in root.iter():
        linkend = elem.get('linkend')
        if not linkend:
            continue

        elem_tag = elem.tag if isinstance(elem.tag, str) else ''
        line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

        # Validate the linkend
        is_valid, issues = validate_linkend(linkend, file_ids, file_paths)

        if is_valid:
            report.add_valid()
        else:
            # Determine severity
            severity = 'error' if 'does not exist' in ' '.join(issues) else 'warning'

            # Try to find suggested fix
            suggested_fix = None
            file_id = extract_file_id(linkend)
            if file_id and file_id in file_paths:
                ids_in_file = file_ids.get(file_paths[file_id].name, set())
                similar = find_similar_ids(linkend, ids_in_file)
                if similar:
                    suggested_fix = similar[0]

            report.add_issue(LinkendIssue(
                file=file_name,
                element_tag=elem_tag,
                linkend=linkend,
                issue_type='invalid_linkend',
                description='; '.join(issues),
                severity=severity,
                suggested_fix=suggested_fix,
                line_number=line_num
            ))


# =============================================================================
# MAIN PROCESSING
# =============================================================================

def process_zip_package(zip_path: Path, fix_issues: bool = False) -> ValidationReport:
    """
    Validate all TOC linkends in a ZIP package.

    Args:
        zip_path: Path to the ZIP package
        fix_issues: If True, attempt to auto-fix issues

    Returns:
        ValidationReport with all issues found
    """
    report = ValidationReport(package_file=str(zip_path))

    if not zip_path.exists():
        report.issues.append(LinkendIssue(
            file='',
            element_tag='',
            linkend='',
            issue_type='file_not_found',
            description=f"ZIP file not found: {zip_path}",
            severity='error'
        ))
        return report

    with tempfile.TemporaryDirectory() as temp_dir:
        temp_path = Path(temp_dir)

        # Extract ZIP
        try:
            with zipfile.ZipFile(zip_path, 'r') as zf:
                zf.extractall(temp_path)
        except zipfile.BadZipFile as e:
            report.issues.append(LinkendIssue(
                file='',
                element_tag='',
                linkend='',
                issue_type='extract_error',
                description=f"Failed to extract ZIP: {e}",
                severity='error'
            ))
            return report

        # Build ID index
        logger.info("Building ID index...")
        file_ids, file_paths = build_id_index(temp_path)
        logger.info(f"Indexed {len(file_paths)} files with IDs")

        # Find Book.XML (contains TOC)
        book_xml_files = list(temp_path.rglob('Book.XML'))
        book_xml_files.extend(list(temp_path.rglob('book.xml')))

        # Also find standalone TOC files
        toc_files = list(temp_path.rglob('toc.*.xml'))
        toc_files.extend(list(temp_path.rglob('toc*.xml')))

        files_to_check = book_xml_files + toc_files

        # Also check all chapter files for linkend attributes
        chapter_files = list(temp_path.rglob('ch*.xml'))
        chapter_files.extend(list(temp_path.rglob('ap*.xml')))
        chapter_files.extend(list(temp_path.rglob('pr*.xml')))
        files_to_check.extend(chapter_files)

        # Remove duplicates
        files_to_check = list(set(files_to_check))

        logger.info(f"Checking {len(files_to_check)} XML files for linkend validation")

        # Validate each file
        for xml_file in sorted(files_to_check):
            report.files_checked += 1

            try:
                parser = etree.XMLParser(remove_blank_text=False)
                tree = etree.parse(str(xml_file), parser)
                root = tree.getroot()

                # Validate all linkends
                validate_all_linkends(root, file_ids, file_paths, report, xml_file.name)

            except Exception as e:
                report.issues.append(LinkendIssue(
                    file=xml_file.name,
                    element_tag='',
                    linkend='',
                    issue_type='parse_error',
                    description=f"Failed to parse: {e}",
                    severity='error'
                ))

    return report


def print_report(report: ValidationReport, verbose: bool = False) -> None:
    """Print validation report to console."""
    print(f"\n{'=' * 70}")
    print(f"TOC Linkend Validation Report: {Path(report.package_file).name}")
    print(f"{'=' * 70}")

    print(f"\nFiles checked: {report.files_checked}")
    print(f"Valid linkends: {report.valid_count}")
    print(f"Invalid linkends: {report.invalid_count}")

    if report.issues:
        # Group issues by type
        errors = [i for i in report.issues if i.severity == 'error']
        warnings = [i for i in report.issues if i.severity == 'warning']

        if errors:
            print(f"\nERRORS ({len(errors)}):")
            for issue in errors[:20]:
                print(f"  [{issue.file}:{issue.line_number or '?'}] <{issue.element_tag}> linkend='{issue.linkend}'")
                print(f"      {issue.description}")
                if issue.suggested_fix:
                    print(f"      Suggested: {issue.suggested_fix}")
            if len(errors) > 20:
                print(f"  ... and {len(errors) - 20} more errors")

        if warnings and verbose:
            print(f"\nWARNINGS ({len(warnings)}):")
            for issue in warnings[:10]:
                print(f"  [{issue.file}:{issue.line_number or '?'}] <{issue.element_tag}> linkend='{issue.linkend}'")
                print(f"      {issue.description}")
            if len(warnings) > 10:
                print(f"  ... and {len(warnings) - 10} more warnings")

    else:
        print("\n  All linkends are valid!")

    print()


# =============================================================================
# CLI ENTRY POINT
# =============================================================================

def main():
    parser = argparse.ArgumentParser(
        description='Validate TOC linkend attributes in RittDoc packages',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Validation Rules:
    1. Base ID (first 11 chars) must match pattern: {prefix}{4-digits}s{4-digits}
       Examples: ch0001s0001, pr0001s0000, ap0001s0001

    2. File Resolution: Base ID prefix determines target file
       ch0001s0001 -> ch0001.xml
       pr0001s0000 -> pr0001.xml
       ap0001s0001 -> ap0001.xml

    3. The full ID must exist as an @id attribute in the target file

Examples:
    python toc_linkend_validator.py ./Output/Successful/9781234.zip
    python toc_linkend_validator.py ./Output/Successful/9781234.zip -v
        """
    )

    parser.add_argument(
        'path',
        type=Path,
        help='Path to ZIP package to validate'
    )

    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show detailed output including warnings'
    )

    parser.add_argument(
        '--fix',
        action='store_true',
        help='Attempt to auto-fix issues (not yet implemented)'
    )

    args = parser.parse_args()

    path = args.path

    if not path.exists():
        print(f"Error: File not found: {path}")
        sys.exit(1)

    # Validate package
    report = process_zip_package(path, fix_issues=args.fix)
    print_report(report, verbose=args.verbose)

    # Exit with error code if issues found
    error_count = len([i for i in report.issues if i.severity == 'error'])
    sys.exit(0 if error_count == 0 else 1)


if __name__ == '__main__':
    main()
