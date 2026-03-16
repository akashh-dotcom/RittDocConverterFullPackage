#!/usr/bin/env python3
"""
Fix Chapter DTD Validation Violations (Simple Version - No lxml required)

This script fixes DTD validation errors in chapter XML files by wrapping
disallowed content elements in sect1 sections.

According to the RittDoc DTD (ritthier2.mod), chapters can only contain:
- beginpage?, chapterinfo?, title, subtitle?, titleabbrev?, tocchap?, (toc|lot|index|glossary|bibliography|sect1)*

GENERIC APPROACH: Uses a whitelist of allowed elements. ALL other elements are wrapped in sect1.
"""

import re
import sys
import tempfile
import zipfile
from pathlib import Path
from typing import List, Tuple

from id_authority import next_available_sect1_id

try:
    from validation_report import ValidationReportGenerator, VerificationItem
    VALIDATION_REPORT_AVAILABLE = True
except ImportError:
    VALIDATION_REPORT_AVAILABLE = False
    print("Note: validation_report module not found. Verification tracking disabled.")


def fix_chapter_content(xml_content: str, chapter_id: str) -> Tuple[str, int, List[Tuple[int, str]]]:
    """
    Fix chapter content by wrapping disallowed elements in sect1.

    GENERIC APPROACH: Uses a whitelist of allowed elements per ritthier2.mod DTD.
    ALL other elements that appear as direct children of chapter are wrapped in sect1.

    Returns:
        Tuple of (fixed_content, num_fixes, wrapper_line_numbers)
    """
    lines = xml_content.split('\n')
    fixed_lines = []
    fixes = 0
    wrapper_infos: List[Tuple[int, str]] = []  # (line_number, wrapper_sect1_id)

    # Elements ALLOWED as direct chapter children (per ritthier2.mod DTD)
    # Everything else needs to be wrapped in sect1
    ALLOWED_BEFORE_BODY = ['beginpage', 'chapterinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap']
    ALLOWED_BODY = ['toc', 'lot', 'index', 'glossary', 'bibliography', 'sect1']

    # Extract chapter title for use in sect1 wrapper
    # Use the actual chapter title so TOC displays correctly (not a generic "Content" label)
    chapter_title = ""  # Empty if no title found - better than misleading "Content"
    chapter_role = None

    # Try to extract chapter title - handle titles with inline formatting
    # First, find the title element and get all text content
    title_match = re.search(r'<title>(.*?)</title>', xml_content, re.DOTALL)
    if title_match:
        title_content = title_match.group(1)
        # Strip XML tags to get plain text (handles <emphasis>, <phrase>, etc.)
        chapter_title = re.sub(r'<[^>]+>', '', title_content).strip()

    # Find chapter start
    in_chapter = False
    after_title = False
    wrapper_added = False
    wrapper_content = []
    wrapper_start_line = None
    indent = "    "
    wrapper_sect1_id = None

    for i, line in enumerate(lines):
        # Detect chapter start
        if '<chapter' in line and not in_chapter:
            in_chapter = True
            role_match = re.search(r'\brole=["\']([^"\']+)["\']', line)
            if role_match:
                chapter_role = role_match.group(1).strip()
            fixed_lines.append(line)
            continue

        # Detect title
        if in_chapter and '<title>' in line and not after_title:
            after_title = True
            fixed_lines.append(line)
            continue

        # Detect chapter end
        if '</chapter>' in line:
            # Close wrapper if open
            if wrapper_added:
                fixed_lines.append(f"{indent}</sect1>")
                fixes += len(wrapper_content)
                if wrapper_start_line:
                    wrapper_infos.append((wrapper_start_line, wrapper_sect1_id or ""))
            fixed_lines.append(line)
            break

        # Check if this is a disallowed element (direct child of chapter)
        if in_chapter and after_title:
            stripped = line.strip()

            # Check if it's an allowed element
            is_allowed_before = any(stripped.startswith(f'<{elem}') for elem in ALLOWED_BEFORE_BODY)
            is_allowed_body = any(stripped.startswith(f'<{elem}') for elem in ALLOWED_BODY)
            is_allowed = is_allowed_before or is_allowed_body

            # Check if it's a closing tag or comment
            is_closing_or_comment = stripped.startswith('</') or stripped.startswith('<!--')

            # If it's an opening tag that's not allowed, it's violating
            is_violating = stripped.startswith('<') and not is_allowed and not is_closing_or_comment

            if is_violating and not wrapper_added:
                # Start wrapper for first disallowed element
                wrapper_start_line = len(fixed_lines) + 1  # Track where wrapper starts
                # Generate a convention-compliant sect1 ID (avoid legacy "{chapter_id}-intro")
                if wrapper_sect1_id is None:
                    existing_ids = re.findall(r'\bid=["\']([^"\']+)["\']', xml_content)
                    wrapper_sect1_id = next_available_sect1_id(chapter_id, existing_ids)
                role_attr = f' role="{chapter_role}"' if chapter_role else ''
                fixed_lines.append(f'{indent}<sect1 id="{wrapper_sect1_id}"{role_attr}>')
                fixed_lines.append(f'{indent}  <title>{chapter_title}</title>')
                wrapper_added = True
                wrapper_content.append(line)
                fixed_lines.append(line)
            elif is_violating and wrapper_added:
                # Continue adding disallowed elements to wrapper
                wrapper_content.append(line)
                fixed_lines.append(line)
            elif is_allowed_body and wrapper_added:
                # Close wrapper before allowed body element (sect1, toc, etc.)
                fixed_lines.append(f"{indent}</sect1>")
                fixes += len(wrapper_content)
                if wrapper_start_line:
                    wrapper_infos.append((wrapper_start_line, wrapper_sect1_id or ""))
                wrapper_content = []
                wrapper_added = False
                wrapper_start_line = None
                fixed_lines.append(line)
            else:
                fixed_lines.append(line)
        else:
            fixed_lines.append(line)

    return '\n'.join(fixed_lines), fixes, wrapper_infos


def update_child_ids_for_wrapper(xml_content: str, chapter_id: str, wrapper_sect1_id: str) -> Tuple[str, int]:
    """
    Update IDs of child elements within a wrapper sect1 to use the wrapper's ID prefix.

    When elements are moved into a wrapper sect1, their IDs should reflect
    their new parent section. For example:
    - Old: ch0010s0000a0003 (anchor in sect1 s0000)
    - New: ch0010s0006a0003 (anchor now in wrapper sect1 s0006)

    Also updates cross-references (linkend, url) that point to the old IDs.

    Args:
        xml_content: The XML content (as string)
        chapter_id: The chapter ID (e.g., 'ch0010')
        wrapper_sect1_id: The ID of the wrapper sect1 (e.g., 'ch0010s0006')

    Returns:
        Tuple of (updated_content, num_ids_updated)
    """
    # Extract the sect1 suffix from the wrapper ID (e.g., 's0006' from 'ch0010s0006')
    new_sect1_suffix = wrapper_sect1_id[len(chapter_id):]  # e.g., 's0006'

    # Find the wrapper sect1 in the content
    wrapper_pattern = rf'<sect1\s+id="{re.escape(wrapper_sect1_id)}"[^>]*>'
    wrapper_match = re.search(wrapper_pattern, xml_content)
    if not wrapper_match:
        return xml_content, 0

    # Find the end of this sect1
    wrapper_start = wrapper_match.start()
    # Simple approach: find matching </sect1> by counting nesting
    sect1_depth = 1
    pos = wrapper_match.end()
    while sect1_depth > 0 and pos < len(xml_content):
        next_open = xml_content.find('<sect1', pos)
        next_close = xml_content.find('</sect1>', pos)

        if next_close == -1:
            break

        if next_open != -1 and next_open < next_close:
            sect1_depth += 1
            pos = next_open + 6
        else:
            sect1_depth -= 1
            if sect1_depth == 0:
                wrapper_end = next_close + len('</sect1>')
            pos = next_close + 9

    if sect1_depth != 0:
        # Couldn't find matching end tag
        return xml_content, 0

    # Extract wrapper content
    wrapper_section = xml_content[wrapper_start:wrapper_end]

    # Pattern to match IDs that belong to this chapter with a sect1 prefix
    # e.g., ch0010s0000a0003, ch0010s0001f0001
    id_pattern = re.compile(
        rf'\bid=["\']({re.escape(chapter_id)})(s\d{{4}})([^"\']+)["\']'
    )

    # Collect all existing IDs to avoid duplicates
    all_ids = set(re.findall(r'\bid=["\']([^"\']+)["\']', xml_content))

    # Track ID mappings for cross-reference updates
    id_mapping = {}
    num_updated = 0

    def replace_id(match):
        nonlocal num_updated
        full_match = match.group(0)
        quote_char = '"' if '"' in full_match else "'"
        ch_part = match.group(1)
        old_sect_part = match.group(2)
        suffix = match.group(3)

        # Don't update if it's already using the wrapper's sect ID
        if old_sect_part == new_sect1_suffix:
            return full_match

        old_id = f"{ch_part}{old_sect_part}{suffix}"
        new_id = f"{ch_part}{new_sect1_suffix}{suffix}"

        # Handle duplicates
        if new_id in all_ids and new_id != old_id:
            counter = 1
            base_new_id = new_id
            while new_id in all_ids:
                new_id = f"{base_new_id}{counter}"
                counter += 1

        id_mapping[old_id] = new_id
        all_ids.add(new_id)
        num_updated += 1
        return f'id={quote_char}{new_id}{quote_char}'

    # Update IDs within the wrapper section
    updated_wrapper = id_pattern.sub(replace_id, wrapper_section)

    # Reconstruct content with updated wrapper
    updated_content = xml_content[:wrapper_start] + updated_wrapper + xml_content[wrapper_end:]

    # Update cross-references (linkend and url) throughout the document
    for old_id, new_id in id_mapping.items():
        # Update linkend attributes
        updated_content = re.sub(
            rf'\blinkend=["\']' + re.escape(old_id) + r'["\']',
            f'linkend="{new_id}"',
            updated_content
        )
        # Update url attributes with fragment references
        updated_content = re.sub(
            rf'(url=["\'][^"\']*?)#' + re.escape(old_id) + r'(["\'])',
            rf'\1#{new_id}\2',
            updated_content
        )

    return updated_content, num_updated


def process_zip_package(zip_path: Path, output_path: Path, generate_verification_report: bool = True) -> dict:
    """
    Process all chapter files in a ZIP package.

    Args:
        zip_path: Path to input ZIP
        output_path: Path to output ZIP
        generate_verification_report: If True, generate Excel report with verification items

    Returns:
        Dictionary with statistics
    """
    stats = {
        'files_processed': 0,
        'files_fixed': 0,
        'total_fixes': 0
    }

    # Create validation report generator if available
    report = None
    if VALIDATION_REPORT_AVAILABLE and generate_verification_report:
        report = ValidationReportGenerator()

    with tempfile.TemporaryDirectory() as tmpdir:
        tmp_path = Path(tmpdir)
        extract_dir = tmp_path / "extracted"
        extract_dir.mkdir()

        # Extract ZIP
        print(f"Extracting {zip_path.name}...")
        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(extract_dir)

        # Find all chapter XML files
        chapter_files = list(extract_dir.rglob("ch*.xml"))
        print(f"Found {len(chapter_files)} chapter files to process")

        for chapter_file in sorted(chapter_files):
            stats['files_processed'] += 1

            # Read file
            content = chapter_file.read_text(encoding='utf-8')

            # Extract chapter ID from filename or content
            chapter_id = chapter_file.stem
            id_match = re.search(r'<chapter\s+id="([^"]+)"', content)
            if id_match:
                chapter_id = id_match.group(1)

            # Fix content
            fixed_content, num_fixes, wrapper_infos = fix_chapter_content(content, chapter_id)

            if num_fixes > 0:
                # Update child element IDs to use wrapper sect1 ID prefix
                # This ensures IDs like ch0010s0000a0003 become ch0010s0006a0003
                total_ids_updated = 0
                for line_num, wrapper_id in wrapper_infos:
                    if wrapper_id:
                        fixed_content, ids_updated = update_child_ids_for_wrapper(
                            fixed_content, chapter_id, wrapper_id
                        )
                        total_ids_updated += ids_updated

                # Write back fixed content
                chapter_file.write_text(fixed_content, encoding='utf-8')
                stats['files_fixed'] += 1
                stats['total_fixes'] += num_fixes
                if total_ids_updated > 0:
                    print(f"  [OK] Fixed {chapter_file.name}: {num_fixes} elements wrapped, {total_ids_updated} IDs updated")
                else:
                    print(f"  [OK] Fixed {chapter_file.name}: {num_fixes} elements wrapped")

                # Add verification items for each wrapper
                if report and wrapper_infos:
                    for line_num, wrapper_id in wrapper_infos:
                        report.add_verification_item(VerificationItem(
                            xml_file=chapter_file.name,
                            line_number=line_num,
                            fix_type="Content Model Fix - Wrapped Elements",
                            fix_description=f'Wrapped violating elements in <sect1 id="{wrapper_id}"> section',
                            verification_reason="Section wrapper was auto-created using chapter title.",
                            suggestion="Review the wrapped content and update the <title> element if a more specific title is appropriate."
                        ))

        # Recreate ZIP
        print(f"\nCreating fixed ZIP: {output_path.name}...")
        with zipfile.ZipFile(output_path, 'w', zipfile.ZIP_DEFLATED) as zf:
            for file_path in extract_dir.rglob('*'):
                if file_path.is_file():
                    arcname = file_path.relative_to(extract_dir)
                    zf.write(file_path, arcname)

    # Return verification items for external use
    stats['verification_items'] = report.verification_items if report else []

    # Generate verification report if we have items
    if report and report.verification_items and generate_verification_report:
        report_path = output_path.parent / f"{output_path.stem}_verification_report.xlsx"
        print(f"\nGenerating verification report: {report_path.name}...")
        report.generate_excel_report(report_path, "Chapter Content Model Fixes")
        print(f"[OK] Verification report saved: {report_path}")
        print(f"  -> {len(report.verification_items)} items require manual content verification")
        stats['verification_report'] = str(report_path)

    return stats


def main():
    import argparse

    parser = argparse.ArgumentParser(
        description="Fix DTD validation violations in RittDoc chapter XML files"
    )
    parser.add_argument("input", help="Input ZIP package")
    parser.add_argument("-o", "--output", help="Output ZIP path (default: add _fixed suffix)")

    args = parser.parse_args()

    input_path = Path(args.input)
    if not input_path.exists():
        print(f"Error: Input file not found: {input_path}")
        sys.exit(1)

    # Determine output path
    if args.output:
        output_path = Path(args.output)
    else:
        output_path = input_path.parent / f"{input_path.stem}_fixed{input_path.suffix}"

    # Process ZIP
    stats = process_zip_package(input_path, output_path)

    # Print summary
    print("\n" + "=" * 70)
    print("FIX SUMMARY")
    print("=" * 70)
    print(f"Files processed:        {stats['files_processed']}")
    print(f"Files fixed:            {stats['files_fixed']}")
    print(f"Total elements wrapped: {stats['total_fixes']}")
    print(f"\nOutput: {output_path}")
    print("=" * 70)


if __name__ == "__main__":
    main()
