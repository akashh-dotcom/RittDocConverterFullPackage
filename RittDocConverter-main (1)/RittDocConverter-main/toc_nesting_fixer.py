#!/usr/bin/env python3
"""
TOC Nesting Validator and Fixer for RittDoc XML Files

This module validates and fixes TOC nesting to ensure compliance with the
RittDoc DTD and XSL transformation rules (R2_LINKEND_AND_TOC_RULESET.md).

Key Rules:
1. Hierarchy: toc -> tocfront*/tocpart*/tocchap*/tocback*
2. tocfront: Contains text/ulink only, requires @linkend
3. tocback: Contains text/ulink (simple) or tocentry+toclevel* (with sections)
4. tocchap: Contains tocentry + toclevel1*
5. toclevel1-5: Contains tocentry + toclevel(N+1)*
6. tocentry: Contains text/ulink, requires @linkend

Nesting Fixes:
- Ensure proper parent-child relationships
- Fix tocfront/tocback that incorrectly have nested children
- Ensure all entries have proper linkend attributes
- Fix entries with children that should be tocchap instead of tocfront/tocback

Usage:
    python toc_nesting_fixer.py /path/to/package.zip
    python toc_nesting_fixer.py /path/to/package.zip --dry-run
"""

import argparse
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
# TOC ELEMENT RULES FROM R2_LINKEND_AND_TOC_RULESET.md
# =============================================================================

# Allowed children for each TOC element
TOC_CHILDREN_RULES = {
    'toc': {'tocinfo', 'title', 'tocfront', 'tocpart', 'tocchap', 'tocback'},
    'tocinfo': {'risinfo'},
    'tocfront': set(),  # Text/ulink only, no structural children
    'tocpart': {'tocentry', 'tocsubpart', 'tocchap', 'tocback'},
    'tocsubpart': {'tocentry', 'tocchap', 'tocback'},
    'tocchap': {'tocentry', 'toclevel1'},
    'tocback': {'tocentry', 'toclevel1'},  # Can have sections for appendices
    'tocentry': set(),  # Text/ulink only
    'toclevel1': {'tocentry', 'toclevel2'},
    'toclevel2': {'tocentry', 'toclevel3'},
    'toclevel3': {'tocentry', 'toclevel4'},
    'toclevel4': {'tocentry', 'toclevel5'},
    'toclevel5': {'tocentry'},  # Terminal level
}

# Elements that MUST have linkend attribute
LINKEND_REQUIRED = {'tocfront', 'tocentry'}

# Elements where linkend is optional (can be on element or child tocentry)
LINKEND_OPTIONAL = {'tocback', 'tocpart', 'tocchap'}

# Front matter detection by linkend prefix
FRONT_MATTER_PREFIXES = {'dd', 'pr', 'ak', 'co'}  # dedication, preface, acknowledgments, colophon

# Back matter detection by linkend prefix
# Note: 'in' is for index chapters; 'ix' is for indexterm elements (not chapters)
BACK_MATTER_PREFIXES = {'ap', 'gl', 'bi', 'in'}  # appendix, glossary, bibliography, index


# =============================================================================
# DATA CLASSES
# =============================================================================

@dataclass
class TocNestingIssue:
    """Record of a TOC nesting issue."""
    element_tag: str
    element_linkend: str
    issue_type: str
    description: str
    severity: str  # 'error', 'warning'
    fixed: bool = False
    fix_description: Optional[str] = None
    line_number: Optional[int] = None


@dataclass
class TocNestingReport:
    """Report of all TOC nesting validation and fixes."""
    package_file: str
    issues: List[TocNestingIssue] = field(default_factory=list)
    elements_checked: int = 0
    fixes_applied: int = 0
    errors: List[str] = field(default_factory=list)

    def has_issues(self) -> bool:
        return bool(self.issues)

    def unfixed_errors(self) -> int:
        return len([i for i in self.issues if i.severity == 'error' and not i.fixed])


# =============================================================================
# VALIDATION FUNCTIONS
# =============================================================================

def get_linkend(elem: etree._Element) -> Optional[str]:
    """Get linkend attribute from element, checking element and child tocentry."""
    linkend = elem.get('linkend')
    if linkend:
        return linkend

    # Check for child tocentry with linkend
    tocentry = elem.find('tocentry')
    if tocentry is not None:
        return tocentry.get('linkend')

    return None


def get_entry_type_from_linkend(linkend: str) -> str:
    """
    Determine entry type (front/body/back) from linkend prefix.

    Args:
        linkend: The linkend attribute value

    Returns:
        'front', 'back', or 'body'
    """
    if not linkend:
        return 'body'

    # Extract prefix (first 2 characters)
    prefix = linkend[:2].lower() if len(linkend) >= 2 else ''

    if prefix in FRONT_MATTER_PREFIXES:
        return 'front'
    elif prefix in BACK_MATTER_PREFIXES:
        return 'back'
    else:
        return 'body'


def validate_toc_children(
    elem: etree._Element,
    report: TocNestingReport
) -> List[TocNestingIssue]:
    """
    Validate that an element's children are allowed by the DTD.

    Args:
        elem: The element to validate
        report: Report to add issues to

    Returns:
        List of issues found
    """
    issues = []
    elem_tag = elem.tag if isinstance(elem.tag, str) else ''
    linkend = get_linkend(elem) or 'unknown'
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    if elem_tag not in TOC_CHILDREN_RULES:
        return issues

    allowed_children = TOC_CHILDREN_RULES[elem_tag]

    for child in elem:
        child_tag = child.tag if isinstance(child.tag, str) else ''

        # Skip text nodes, comments, processing instructions
        if not child_tag:
            continue

        # Skip allowed inline elements (ulink, emphasis, etc.)
        if child_tag in {'ulink', 'emphasis', 'link', 'sub', 'sup'}:
            continue

        if child_tag not in allowed_children:
            issue = TocNestingIssue(
                element_tag=elem_tag,
                element_linkend=linkend,
                issue_type='invalid_child',
                description=f"<{elem_tag}> cannot contain <{child_tag}> (allowed: {', '.join(sorted(allowed_children)) or 'text only'})",
                severity='error',
                line_number=line_num
            )
            issues.append(issue)
            report.issues.append(issue)

    return issues


def validate_linkend_presence(
    elem: etree._Element,
    report: TocNestingReport
) -> Optional[TocNestingIssue]:
    """
    Validate that required linkend attributes are present.

    Args:
        elem: The element to validate
        report: Report to add issues to

    Returns:
        Issue if linkend is missing, None otherwise
    """
    elem_tag = elem.tag if isinstance(elem.tag, str) else ''
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    if elem_tag in LINKEND_REQUIRED:
        linkend = elem.get('linkend')
        if not linkend:
            issue = TocNestingIssue(
                element_tag=elem_tag,
                element_linkend='MISSING',
                issue_type='missing_linkend',
                description=f"<{elem_tag}> requires @linkend attribute",
                severity='error',
                line_number=line_num
            )
            report.issues.append(issue)
            return issue

    return None


def validate_linkend_format(
    elem: etree._Element,
    report: TocNestingReport
) -> Optional[TocNestingIssue]:
    """
    Validate linkend format follows the ID naming convention.

    Format: {prefix}{4-digits}s{4-digits}[{element_code}{sequence}]
    Examples: ch0001s0001, pr0001s0000, ch0001s0001fg0001

    Args:
        elem: The element to validate
        report: Report to add issues to

    Returns:
        Issue if linkend format is invalid, None otherwise
    """
    elem_tag = elem.tag if isinstance(elem.tag, str) else ''
    linkend = get_linkend(elem)
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    if not linkend:
        return None

    # Valid patterns
    # Chapter/section base: ch0001, ch0001s0001, ch0001s0001s0001
    # Element IDs: ch0001s0001fg0001
    # Part IDs: pt0001, pt0001s0001

    valid_base_pattern = re.compile(r'^[a-z]{2}\d{4}(s\d{4})*([a-z]{1,3}\d{1,4})?$')

    if not valid_base_pattern.match(linkend):
        # Check for common issues
        issues_found = []

        if re.search(r'[A-Z]', linkend):
            issues_found.append("contains uppercase characters")
        if re.search(r'[-_]', linkend):
            issues_found.append("contains dash/underscore separators")
        if not re.match(r'^[a-z]{2}\d{4}', linkend):
            issues_found.append("doesn't start with valid prefix (e.g., ch0001)")

        if issues_found:
            issue = TocNestingIssue(
                element_tag=elem_tag,
                element_linkend=linkend,
                issue_type='invalid_linkend_format',
                description=f"linkend '{linkend}' {'; '.join(issues_found)}",
                severity='warning',
                line_number=line_num
            )
            report.issues.append(issue)
            return issue

    return None


def validate_entry_type_placement(
    elem: etree._Element,
    parent: etree._Element,
    report: TocNestingReport
) -> Optional[TocNestingIssue]:
    """
    Validate that front/back matter is in the correct container.

    Args:
        elem: The element to validate
        parent: Parent element
        report: Report to add issues to

    Returns:
        Issue if placement is wrong, None otherwise
    """
    elem_tag = elem.tag if isinstance(elem.tag, str) else ''
    parent_tag = parent.tag if isinstance(parent.tag, str) else ''
    linkend = get_linkend(elem)
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    if not linkend:
        return None

    expected_type = get_entry_type_from_linkend(linkend)

    # Check if element type matches linkend type
    if expected_type == 'front' and elem_tag != 'tocfront':
        if elem_tag in {'tocchap', 'tocback'}:
            issue = TocNestingIssue(
                element_tag=elem_tag,
                element_linkend=linkend,
                issue_type='wrong_container',
                description=f"Front matter linkend '{linkend}' should be in <tocfront>, not <{elem_tag}>",
                severity='warning',
                line_number=line_num
            )
            report.issues.append(issue)
            return issue

    if expected_type == 'back' and elem_tag not in {'tocback'}:
        if elem_tag in {'tocfront', 'tocchap'} and parent_tag == 'toc':
            issue = TocNestingIssue(
                element_tag=elem_tag,
                element_linkend=linkend,
                issue_type='wrong_container',
                description=f"Back matter linkend '{linkend}' should be in <tocback>, not <{elem_tag}>",
                severity='warning',
                line_number=line_num
            )
            report.issues.append(issue)
            return issue

    return None


# =============================================================================
# FIX FUNCTIONS
# =============================================================================

def fix_tocfront_with_children(
    tocfront: etree._Element,
    parent: etree._Element,
    report: TocNestingReport
) -> bool:
    """
    Fix a tocfront element that incorrectly has structural children.

    Strategy: Convert to tocchap if it has children.

    Args:
        tocfront: The tocfront element
        parent: Parent (toc) element
        report: Report to add fixes to

    Returns:
        True if fix was applied
    """
    # Check if tocfront has any structural children
    structural_children = [c for c in tocfront if c.tag in TOC_CHILDREN_RULES]

    if not structural_children:
        return False

    linkend = get_linkend(tocfront) or 'unknown'
    line_num = tocfront.sourceline if hasattr(tocfront, 'sourceline') else None

    # Create new tocchap
    tocchap = etree.Element('tocchap')

    # Create tocentry with the tocfront content
    tocentry = etree.SubElement(tocchap, 'tocentry')
    if tocfront.get('linkend'):
        tocentry.set('linkend', tocfront.get('linkend'))

    # Copy text/inline content to tocentry
    tocentry.text = tocfront.text
    for child in list(tocfront):
        if child.tag in {'ulink', 'emphasis', 'link'}:
            tocentry.append(child)
        elif child.tag in {'toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5'}:
            tocchap.append(child)

    # Replace tocfront with tocchap
    idx = list(parent).index(tocfront)
    parent.remove(tocfront)
    parent.insert(idx, tocchap)

    issue = TocNestingIssue(
        element_tag='tocfront',
        element_linkend=linkend,
        issue_type='tocfront_with_children',
        description=f"<tocfront> with children converted to <tocchap>",
        severity='error',
        fixed=True,
        fix_description="Converted tocfront to tocchap to allow nested sections",
        line_number=line_num
    )
    report.issues.append(issue)
    report.fixes_applied += 1

    return True


def fix_missing_tocentry_in_toclevel(
    toclevel: etree._Element,
    report: TocNestingReport
) -> bool:
    """
    Fix a toclevel element that's missing a tocentry child.

    Strategy: Create tocentry from first child's content or use placeholder.

    Args:
        toclevel: The toclevel element
        report: Report to add fixes to

    Returns:
        True if fix was applied
    """
    elem_tag = toclevel.tag if isinstance(toclevel.tag, str) else ''

    # Check if already has tocentry
    if toclevel.find('tocentry') is not None:
        return False

    line_num = toclevel.sourceline if hasattr(toclevel, 'sourceline') else None

    # Create tocentry
    tocentry = etree.Element('tocentry')

    # Try to get content from first child toclevel
    first_child = None
    for child in toclevel:
        if child.tag and child.tag.startswith('toclevel'):
            first_child = child
            break

    if first_child is not None:
        child_entry = first_child.find('tocentry')
        if child_entry is not None:
            # Use parent level naming
            linkend = child_entry.get('linkend', '')
            if linkend:
                # Try to derive parent linkend
                # e.g., ch0001s0001s0001 -> ch0001s0001
                parent_linkend = re.sub(r's\d{4}$', '', linkend)
                if parent_linkend != linkend:
                    tocentry.set('linkend', parent_linkend)

    tocentry.text = "(Section)"  # Placeholder

    # Insert at beginning
    toclevel.insert(0, tocentry)

    issue = TocNestingIssue(
        element_tag=elem_tag,
        element_linkend=tocentry.get('linkend', 'unknown'),
        issue_type='missing_tocentry',
        description=f"<{elem_tag}> missing required <tocentry> child",
        severity='error',
        fixed=True,
        fix_description="Added placeholder tocentry element",
        line_number=line_num
    )
    report.issues.append(issue)
    report.fixes_applied += 1

    return True


def fix_toclevel_depth(
    elem: etree._Element,
    expected_level: int,
    report: TocNestingReport
) -> bool:
    """
    Fix incorrect toclevel depth.

    Args:
        elem: The toclevel element
        expected_level: Expected level (1-5)
        report: Report to add fixes to

    Returns:
        True if fix was applied
    """
    elem_tag = elem.tag if isinstance(elem.tag, str) else ''

    if not elem_tag.startswith('toclevel'):
        return False

    # Extract current level
    try:
        current_level = int(elem_tag[-1])
    except ValueError:
        return False

    if current_level == expected_level:
        return False

    # Clamp to valid range
    new_level = max(1, min(5, expected_level))
    new_tag = f'toclevel{new_level}'

    linkend = get_linkend(elem) or 'unknown'
    line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

    # Change the tag
    elem.tag = new_tag

    issue = TocNestingIssue(
        element_tag=elem_tag,
        element_linkend=linkend,
        issue_type='wrong_toclevel',
        description=f"<{elem_tag}> changed to <{new_tag}> for proper nesting",
        severity='warning',
        fixed=True,
        fix_description=f"Changed toclevel{current_level} to toclevel{new_level}",
        line_number=line_num
    )
    report.issues.append(issue)
    report.fixes_applied += 1

    return True


# =============================================================================
# MAIN PROCESSING FUNCTIONS
# =============================================================================

def validate_and_fix_toc(
    toc: etree._Element,
    report: TocNestingReport,
    fix_issues: bool = True
) -> bool:
    """
    Validate and optionally fix TOC nesting issues.

    Args:
        toc: The <toc> element
        report: Report to add issues/fixes to
        fix_issues: If True, attempt to fix issues

    Returns:
        True if any changes were made
    """
    modified = False

    if toc.tag != 'toc':
        report.errors.append(f"Expected <toc> element, got <{toc.tag}>")
        return False

    # First pass: Validate all elements
    def validate_element(elem: etree._Element, parent: Optional[etree._Element] = None, depth: int = 0):
        elem_tag = elem.tag if isinstance(elem.tag, str) else ''
        if not elem_tag:
            return

        report.elements_checked += 1

        # Validate children
        validate_toc_children(elem, report)

        # Validate linkend presence
        validate_linkend_presence(elem, report)

        # Validate linkend format
        validate_linkend_format(elem, report)

        # Validate placement
        if parent is not None:
            validate_entry_type_placement(elem, parent, report)

        # Recurse into children
        for child in list(elem):
            validate_element(child, elem, depth + 1)

    validate_element(toc)

    # Second pass: Apply fixes if requested
    if fix_issues:
        # Fix tocfront with children
        for tocfront in toc.findall('tocfront'):
            if fix_tocfront_with_children(tocfront, toc, report):
                modified = True

        # Fix missing tocentry in toclevel elements
        for toclevel_tag in ['toclevel1', 'toclevel2', 'toclevel3', 'toclevel4', 'toclevel5']:
            for toclevel in toc.iter(toclevel_tag):
                if fix_missing_tocentry_in_toclevel(toclevel, report):
                    modified = True

        # Fix toclevel nesting depth
        def fix_toclevel_recursively(elem: etree._Element, expected_level: int = 1):
            nonlocal modified

            elem_tag = elem.tag if isinstance(elem.tag, str) else ''

            if elem_tag.startswith('toclevel'):
                if fix_toclevel_depth(elem, expected_level, report):
                    modified = True

                # Recurse with incremented level
                for child in list(elem):
                    child_tag = child.tag if isinstance(child.tag, str) else ''
                    if child_tag.startswith('toclevel'):
                        fix_toclevel_recursively(child, expected_level + 1)
            else:
                # For tocchap, tocpart, start toclevel at 1
                for child in list(elem):
                    child_tag = child.tag if isinstance(child.tag, str) else ''
                    if child_tag.startswith('toclevel'):
                        fix_toclevel_recursively(child, 1)
                    elif child_tag in {'tocchap', 'tocpart', 'tocback'}:
                        fix_toclevel_recursively(child, 1)

        fix_toclevel_recursively(toc)

    return modified


def process_book_xml(
    book_xml_path: Path,
    report: TocNestingReport,
    dry_run: bool = False
) -> bool:
    """
    Process Book.XML to validate and fix TOC nesting.

    Args:
        book_xml_path: Path to Book.XML
        report: Report to add issues/fixes to
        dry_run: If True, don't modify files

    Returns:
        True if any changes were made
    """
    try:
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(str(book_xml_path), parser)
        root = tree.getroot()
    except etree.XMLSyntaxError as e:
        report.errors.append(f"Failed to parse Book.XML: {e}")
        return False

    # Find TOC element
    toc = root.find('.//toc')
    if toc is None:
        report.errors.append("No <toc> element found in Book.XML")
        return False

    # Validate and fix
    modified = validate_and_fix_toc(toc, report, fix_issues=not dry_run)

    # Write changes if not dry run
    if modified and not dry_run:
        tree.write(str(book_xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)

    return modified


def process_zip_package(
    zip_path: Path,
    output_path: Optional[Path] = None,
    dry_run: bool = False
) -> TocNestingReport:
    """
    Process a ZIP package to validate and fix TOC nesting.

    Args:
        zip_path: Path to the input ZIP package
        output_path: Path for output ZIP (if None, modifies in place)
        dry_run: If True, don't modify any files

    Returns:
        TocNestingReport with all issues and fixes
    """
    report = TocNestingReport(package_file=str(zip_path))

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

        # Find Book.XML
        book_xml_path = temp_path / 'Book.XML'
        if not book_xml_path.exists():
            book_xml_path = temp_path / 'book.xml'

        if not book_xml_path.exists():
            report.errors.append("Book.XML not found in package")
            return report

        # Process Book.XML
        modified = process_book_xml(book_xml_path, report, dry_run)

        # Repackage if changes were made and not dry run
        if modified and not dry_run:
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


def print_report(report: TocNestingReport, verbose: bool = False) -> None:
    """Print a TOC nesting report to the console."""
    print(f"\n{'=' * 70}")
    print(f"TOC Nesting Validation Report: {Path(report.package_file).name}")
    print(f"{'=' * 70}")

    print(f"\nElements checked: {report.elements_checked}")
    print(f"Issues found: {len(report.issues)}")
    print(f"Fixes applied: {report.fixes_applied}")

    if report.errors:
        print(f"\nERRORS ({len(report.errors)}):")
        for error in report.errors:
            print(f"  - {error}")

    if report.issues:
        # Group by issue type
        by_type = {}
        for issue in report.issues:
            if issue.issue_type not in by_type:
                by_type[issue.issue_type] = []
            by_type[issue.issue_type].append(issue)

        print(f"\nISSUES BY TYPE:")
        for issue_type, issues in sorted(by_type.items()):
            fixed_count = len([i for i in issues if i.fixed])
            print(f"  {issue_type}: {len(issues)} ({fixed_count} fixed)")

            if verbose:
                for issue in issues[:5]:
                    status = "[FIXED]" if issue.fixed else "[OPEN]"
                    print(f"    {status} {issue.element_tag} linkend='{issue.element_linkend}'")
                    print(f"           {issue.description}")
                if len(issues) > 5:
                    print(f"    ... and {len(issues) - 5} more")

    if not report.has_issues() and not report.errors:
        print("\n  TOC nesting is fully compliant!")

    print(f"\nSUMMARY:")
    print(f"  Total issues:     {len(report.issues)}")
    print(f"  Fixed:            {report.fixes_applied}")
    print(f"  Unfixed errors:   {report.unfixed_errors()}")
    print()


# =============================================================================
# CLI ENTRY POINT
# =============================================================================

def main():
    parser = argparse.ArgumentParser(
        description='Validate and fix TOC nesting in RittDoc packages',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
TOC Nesting Rules (from R2_LINKEND_AND_TOC_RULESET.md):

  toc
  ├── tocfront (front matter - text only)
  ├── tocpart (part container)
  │   ├── tocentry
  │   └── tocchap...
  ├── tocchap (chapter container)
  │   ├── tocentry
  │   └── toclevel1...
  └── tocback (back matter)
      ├── tocentry (optional)
      └── toclevel1... (for appendix sections)

Examples:
    python toc_nesting_fixer.py ./Output/Successful/9781234.zip
    python toc_nesting_fixer.py ./Output/Successful/9781234.zip --dry-run -v
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
    total_issues = 0
    total_fixes = 0
    total_errors = 0

    for zip_file in sorted(zip_files):
        logger.info(f"Processing {zip_file.name}...")
        output_path = args.output if len(zip_files) == 1 else None
        report = process_zip_package(zip_file, output_path, dry_run=args.dry_run)

        total_issues += len(report.issues)
        total_fixes += report.fixes_applied
        total_errors += len(report.errors)

        if not args.quiet or report.errors:
            print_report(report, verbose=args.verbose)

    # Print overall summary
    print("=" * 70)
    print("OVERALL SUMMARY")
    print("=" * 70)
    print(f"Files processed:  {len(zip_files)}")
    print(f"Total issues:     {total_issues}")
    print(f"Total fixes:      {total_fixes}")
    print(f"Errors:           {total_errors}")

    if args.dry_run:
        print("\n*** DRY RUN - No files were modified ***")

    sys.exit(0 if total_errors == 0 else 1)


if __name__ == '__main__':
    main()
