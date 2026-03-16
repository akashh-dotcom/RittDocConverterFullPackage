#!/usr/bin/env python3
"""
Post-QA XML Fixer for ISBN 9783031975134 ("Anatomy of the Upper and Lower Limbs")

Fixes three issues found during post-fix verification QA:
  A1: "a. k. a." → "a.k.a." abbreviation spacing (5 files)
  A2: Duplicate ordered list numbering — strip leading numbers from orderedlist items (17 files)
  A3: Footnote consolidation — split merged footnotes and place inline (14 chapters)

Usage:
  python fix_9783031975134_post_qa.py /path/to/9783031975134/ [--dry-run]
"""

import argparse
import copy
import re
import sys
from pathlib import Path
from lxml import etree

# ============================================================================
# CONFIGURATION
# ============================================================================

ISBN = '9783031975134'

# Known abbreviation repairs
ABBREVIATION_REPAIRS = {
    'a. k. a.': 'a.k.a.',
}

# Leading number patterns for ordered list items
SIMPLE_LEADING_NUMBER = re.compile(
    r'^[\s]*'
    r'[\[\(]?'
    r'(\d+)'
    r'[\]\)]?'
    r'[\.\:\)\]\s]*'
    r'(.*)$',
    re.DOTALL
)

COMPLEX_NUMBERING = re.compile(
    r'^[\s]*'
    r'(\d+(?:\.\d+)+)'
    r'[\.\s]*'
    r'(.*)$',
    re.DOTALL
)

# ============================================================================
# FIX A1: Abbreviation spacing
# ============================================================================

def fix_abbreviation_spacing(xml_dir: Path, dry_run: bool = False) -> list:
    """Fix 'a. k. a.' → 'a.k.a.' in all XML files."""
    fixes = []

    for xml_file in sorted(xml_dir.glob('*.xml')):
        content = xml_file.read_text(encoding='utf-8')
        new_content = content
        for broken, correct in ABBREVIATION_REPAIRS.items():
            if broken in new_content:
                count = new_content.count(broken)
                new_content = new_content.replace(broken, correct)
                fixes.append(f"  {xml_file.name}: replaced {count}x '{broken}' → '{correct}'")

        if new_content != content and not dry_run:
            xml_file.write_text(new_content, encoding='utf-8')

    return fixes


# ============================================================================
# FIX A2: Ordered list duplicate numbering
# ============================================================================

def strip_leading_number(text: str):
    """Strip leading number/enumeration from text. Returns (stripped, was_modified, number)."""
    if not text:
        return (text, False, None)

    original = text.strip()

    # Try complex numbering first (1.2.3)
    match = COMPLEX_NUMBERING.match(original)
    if match:
        stripped_number = match.group(1)
        remaining = match.group(2).strip()
        if remaining:
            return (remaining, True, stripped_number)

    # Try simple leading number
    match = SIMPLE_LEADING_NUMBER.match(original)
    if match:
        stripped_number = match.group(1)
        remaining = match.group(2).strip()
        if remaining:
            return (remaining, True, stripped_number)

    return (original, False, None)


def fix_ordered_list_numbering(xml_dir: Path, dry_run: bool = False) -> list:
    """Strip leading numbers from orderedlist items and clean up attributes."""
    fixes = []
    parser = etree.XMLParser(remove_blank_text=False)

    for xml_file in sorted(xml_dir.glob('*.xml')):
        try:
            tree = etree.parse(str(xml_file), parser)
        except etree.ParseError:
            continue

        root = tree.getroot()
        file_modified = False

        for orderedlist in root.iter('orderedlist'):
            # Clean up mark="none" and role="none" attributes
            if orderedlist.get('mark') == 'none':
                del orderedlist.attrib['mark']
                file_modified = True
            if orderedlist.get('role') == 'none':
                del orderedlist.attrib['role']
                file_modified = True

            for listitem in orderedlist.findall('listitem'):
                for para in listitem.findall('para'):
                    if para.text:
                        stripped, was_modified, number = strip_leading_number(para.text)
                        if was_modified:
                            fixes.append(f"  {xml_file.name}: stripped '{number}.' from orderedlist item")
                            para.text = stripped
                            file_modified = True
                    break  # Only fix first para per listitem

        if file_modified and not dry_run:
            tree.write(str(xml_file), encoding='utf-8', xml_declaration=True)

    return fixes


# ============================================================================
# FIX A3: Footnote consolidation — split and place inline
# ============================================================================

def extract_footnote_texts(footnote_para):
    """
    Extract individual footnote texts from a consolidated footnote para.

    The consolidated para has structure like:
      <para>Footnotes <emphasis role="FootnoteNumber"><phrase>1</phrase></emphasis>
       Text of footnote 1... <emphasis role="FootnoteNumber"><phrase>2</phrase></emphasis>
       Text of footnote 2...</para>

    Returns dict: {number_str: text_content}
    """
    footnotes = {}

    # Serialize the para to string for easier parsing
    para_str = etree.tostring(footnote_para, encoding='unicode')

    # Find all FootnoteNumber markers and extract text between them
    # Pattern: <emphasis role="FootnoteNumber">...<phrase>N</phrase>...</emphasis> TEXT
    # or: <emphasis role="FootnoteNumber">...<link ...>N</link>...</emphasis> TEXT

    # Find positions of all FootnoteNumber emphasis elements
    marker_pattern = re.compile(
        r'<emphasis\s+role="FootnoteNumber">\s*'
        r'(?:<phrase>(\d+)</phrase>|<link[^>]*>(\d+)</link>)\s*'
        r'</emphasis>\s*',
        re.DOTALL
    )

    matches = list(marker_pattern.finditer(para_str))
    if not matches:
        return footnotes

    for i, match in enumerate(matches):
        fn_num = match.group(1) or match.group(2)
        text_start = match.end()

        if i + 1 < len(matches):
            text_end = matches[i + 1].start()
        else:
            # Last footnote — text goes to end of para (before </para>)
            text_end = para_str.rfind('</para>')
            if text_end == -1:
                text_end = len(para_str)

        fn_text = para_str[text_start:text_end].strip()

        # Clean up: remove trailing XML artifacts, leading/trailing spaces
        fn_text = re.sub(r'\s+', ' ', fn_text).strip()

        # Remove any trailing period+bracket artifacts from concatenation
        # but preserve legitimate content
        if fn_text:
            footnotes[fn_num] = fn_text

    return footnotes


def text_from_html_fragment(html_str: str) -> str:
    """Extract plain text from an XML/HTML fragment string."""
    # Wrap in a temporary element to parse
    try:
        wrapped = f'<tmp>{html_str}</tmp>'
        elem = etree.fromstring(wrapped)
        return ''.join(elem.itertext()).strip()
    except etree.XMLSyntaxError:
        # Fall back to regex stripping
        return re.sub(r'<[^>]+>', '', html_str).strip()


def create_footnote_element(fn_id: str, fn_text_xml: str) -> etree._Element:
    """
    Create a <footnote> element from extracted text content.

    The fn_text_xml may contain XML markup (emphasis, link, etc.)
    that needs to be preserved.
    """
    footnote = etree.Element('footnote')
    footnote.set('id', fn_id)

    # Try to parse the text as XML fragment (it may contain inline markup)
    try:
        para_xml = f'<para>{fn_text_xml}</para>'
        para = etree.fromstring(para_xml)
        footnote.append(para)
    except etree.XMLSyntaxError:
        # If XML parsing fails, use plain text
        para = etree.SubElement(footnote, 'para')
        plain_text = re.sub(r'<[^>]+>', '', fn_text_xml).strip()
        para.text = plain_text

    return footnote


def find_superscript_for_footnote(root, fn_number: str, footnote_elem):
    """
    Find a <superscript> element in the body text that matches the footnote number.
    Returns the superscript element and its parent, or (None, None) if not found.

    We need to find <superscript> elements that:
    1. Contain text matching fn_number
    2. Are NOT inside the consolidated <footnote> element itself
    """
    for superscript in root.iter('superscript'):
        # Skip superscripts inside the consolidated footnote
        parent = superscript.getparent()
        is_inside_footnote = False
        p = parent
        while p is not None:
            if p.tag == 'footnote':
                is_inside_footnote = True
                break
            p = p.getparent()

        if is_inside_footnote:
            continue

        # Check if the text content matches the footnote number
        sup_text = ''.join(superscript.itertext()).strip()
        if sup_text == fn_number:
            return superscript, parent

    return None, None


def replace_superscript_with_footnote(superscript_elem, parent_elem, new_footnote):
    """
    Replace a <superscript>N</superscript> with the new <footnote> element.

    The superscript may be:
    - A direct child of parent (para.text ... <superscript>1</superscript> ... next text)
    - After some text in parent.text or a sibling's .tail
    """
    # Find index of superscript in parent
    idx = list(parent_elem).index(superscript_elem)

    # The superscript's tail (text after </superscript>) should become the footnote's tail
    new_footnote.tail = superscript_elem.tail
    superscript_elem.tail = None

    # Replace superscript with footnote
    parent_elem.remove(superscript_elem)
    parent_elem.insert(idx, new_footnote)


def remove_consolidated_footnote(root):
    """
    Remove the consolidated <footnote> element and clean up its parent <para>.

    The consolidated footnote sits inside a <para> like:
      <para>Some text...<footnote id="...">...</footnote></para>

    After removing the footnote, if the para only has trailing text from the
    footnote, clean that up too.
    """
    removed = 0
    for footnote in root.iter('footnote'):
        # Check if this is a consolidated footnote (contains FootnoteNumber emphasis)
        has_fn_markers = False
        for emphasis in footnote.iter('emphasis'):
            if emphasis.get('role') == 'FootnoteNumber':
                has_fn_markers = True
                break

        if not has_fn_markers:
            continue

        parent = footnote.getparent()
        if parent is None:
            continue

        # Transfer footnote's tail to previous sibling or parent text
        if footnote.tail and footnote.tail.strip():
            # Find preceding sibling
            idx = list(parent).index(footnote)
            if idx > 0:
                prev = parent[idx - 1]
                prev.tail = (prev.tail or '') + footnote.tail
            else:
                parent.text = (parent.text or '') + footnote.tail

        parent.remove(footnote)
        removed += 1

        # If parent para is now empty (or just whitespace), remove it too
        if parent.tag == 'para':
            all_text = ''.join(parent.itertext()).strip()
            if not all_text and len(parent) == 0:
                grandparent = parent.getparent()
                if grandparent is not None:
                    grandparent.remove(parent)

    return removed


def fix_footnote_consolidation(xml_dir: Path, dry_run: bool = False) -> list:
    """
    Split consolidated footnotes and place them inline at reference points.

    Footnote references (<superscript>N</superscript>) may be in DIFFERENT section
    files than the consolidated <footnote>. We need to search across all files in
    the same chapter.
    """
    fixes = []
    parser = etree.XMLParser(remove_blank_text=False)

    # Group files by chapter: ch0002 → [ch0002s0002.xml, ch0002s0003.xml, ...]
    chapter_files = {}
    for xml_file in sorted(xml_dir.glob('*.xml')):
        ch_match = re.search(r'(ch\d{4})', xml_file.name)
        if ch_match:
            chapter = ch_match.group(1)
            if chapter not in chapter_files:
                chapter_files[chapter] = []
            chapter_files[chapter].append(xml_file)

    # Parse all chapter files into a cache
    file_trees = {}
    for xml_file in sorted(xml_dir.glob('*.xml')):
        try:
            tree = etree.parse(str(xml_file), parser)
            file_trees[xml_file] = tree
        except etree.ParseError:
            continue

    # Process each chapter
    for chapter, files in sorted(chapter_files.items()):
        # Find which file(s) have consolidated footnotes
        for fn_file in files:
            if fn_file not in file_trees:
                continue
            tree = file_trees[fn_file]
            root = tree.getroot()

            consolidated_footnotes = []
            for footnote in root.iter('footnote'):
                has_fn_markers = False
                for emphasis in footnote.iter('emphasis'):
                    if emphasis.get('role') == 'FootnoteNumber':
                        has_fn_markers = True
                        break
                if has_fn_markers:
                    consolidated_footnotes.append(footnote)

            if not consolidated_footnotes:
                continue

            for consolidated in consolidated_footnotes:
                fn_paras = consolidated.findall('para')
                if not fn_paras:
                    continue

                fn_texts = extract_footnote_texts(fn_paras[0])
                if not fn_texts:
                    fixes.append(f"  {fn_file.name}: WARNING - could not parse consolidated footnote")
                    continue

                # Search ALL files in this chapter for superscript references
                placed_count = 0
                modified_files = set()

                for fn_num, fn_text_xml in sorted(fn_texts.items(), key=lambda x: int(x[0])):
                    placed = False

                    # Search across all section files in the chapter
                    for search_file in files:
                        if search_file not in file_trees:
                            continue
                        search_tree = file_trees[search_file]
                        search_root = search_tree.getroot()

                        # Extract section ID for the target file
                        sec_match = re.search(r'(ch\d+s\d+)', search_file.name)
                        section_id = sec_match.group(1) if sec_match else 'unknown'
                        fn_id = f"{section_id}fn{int(fn_num):04d}"

                        new_footnote = create_footnote_element(fn_id, fn_text_xml)
                        superscript, sup_parent = find_superscript_for_footnote(
                            search_root, fn_num, consolidated)

                        if superscript is not None and sup_parent is not None:
                            replace_superscript_with_footnote(superscript, sup_parent, new_footnote)
                            placed_count += 1
                            placed = True
                            modified_files.add(search_file)
                            fixes.append(f"  {search_file.name}: placed footnote {fn_num} inline (id={fn_id})")
                            break  # Found in this file, move to next footnote

                    if not placed:
                        fixes.append(f"  {fn_file.name}: WARNING - no <superscript>{fn_num}</superscript> "
                                    f"found in any {chapter}s*.xml file")

                # Remove the consolidated footnote
                if placed_count > 0:
                    removed = remove_consolidated_footnote(root)
                    if removed > 0:
                        modified_files.add(fn_file)
                        status = "" if placed_count == len(fn_texts) else f" (partial: {placed_count}/{len(fn_texts)})"
                        fixes.append(f"  {fn_file.name}: removed consolidated footnote{status}")

                # Write all modified files
                if not dry_run:
                    for mod_file in modified_files:
                        if mod_file in file_trees:
                            file_trees[mod_file].write(str(mod_file), encoding='utf-8', xml_declaration=True)

    return fixes


# ============================================================================
# MAIN
# ============================================================================

def main():
    parser = argparse.ArgumentParser(description=f'Post-QA XML fixer for ISBN {ISBN}')
    parser.add_argument('xml_dir', type=Path, help='Path to the XML directory')
    parser.add_argument('--dry-run', action='store_true', help='Show what would change without modifying files')
    parser.add_argument('--fix', choices=['all', 'a1', 'a2', 'a3'], default='all',
                       help='Which fix to apply (default: all)')
    args = parser.parse_args()

    if not args.xml_dir.is_dir():
        print(f"Error: {args.xml_dir} is not a directory")
        sys.exit(1)

    mode = "DRY RUN" if args.dry_run else "APPLYING"
    print(f"\n{'='*60}")
    print(f"Post-QA XML Fixer for ISBN {ISBN}")
    print(f"Mode: {mode}")
    print(f"Directory: {args.xml_dir}")
    print(f"{'='*60}\n")

    total_fixes = 0

    # A1: Abbreviation spacing
    if args.fix in ('all', 'a1'):
        print("--- A1: Fixing abbreviation spacing ---")
        a1_fixes = fix_abbreviation_spacing(args.xml_dir, args.dry_run)
        if a1_fixes:
            for f in a1_fixes:
                print(f)
            total_fixes += len(a1_fixes)
        else:
            print("  No abbreviation issues found.")
        print()

    # A2: Ordered list numbering
    if args.fix in ('all', 'a2'):
        print("--- A2: Fixing ordered list duplicate numbering ---")
        a2_fixes = fix_ordered_list_numbering(args.xml_dir, args.dry_run)
        if a2_fixes:
            for f in a2_fixes:
                print(f)
            total_fixes += len(a2_fixes)
        else:
            print("  No ordered list issues found.")
        print()

    # A3: Footnote consolidation
    if args.fix in ('all', 'a3'):
        print("--- A3: Fixing footnote consolidation ---")
        a3_fixes = fix_footnote_consolidation(args.xml_dir, args.dry_run)
        if a3_fixes:
            for f in a3_fixes:
                print(f)
            total_fixes += len(a3_fixes)
        else:
            print("  No consolidated footnotes found.")
        print()

    print(f"{'='*60}")
    print(f"Total fixes: {total_fixes}")
    if args.dry_run:
        print("(Dry run — no files were modified)")
    print(f"{'='*60}")


if __name__ == '__main__':
    main()
