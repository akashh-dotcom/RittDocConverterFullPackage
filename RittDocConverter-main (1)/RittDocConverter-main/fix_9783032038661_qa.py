#!/usr/bin/env python3
"""
QA Fix Script for ISBN 9783032038661 - Cancer Treatment Modalities
==================================================================
Fixes systematic conversion issues found during QA:
  A1. PubMedPubMed Central concatenation (1,139 occurrences)
  A2. ContactOfAuthor metadata leaking (38 occurrences)
  A3. HeadingNumber missing space (525 occurrences)
  A4. DOI display text spacing (24 files)
  A5. Email display text spacing (22 files)
  A6. Copyright text run-together (21 files)
  A7. Empty risprev in title sections (23 files)
  A8. UTF-8 Â character (2 files)
  A9. TOC heading number spacing + empty linkend

All fixes preserve RittDoc DTD v1.1 compliance.
"""

import os
import re
import sys
import glob
from lxml import etree


ISBN = '9783032038661'


def fix_pubmed_spacing(xml_dir: str) -> int:
    """A1: Fix 'PubMedPubMed Central' → 'PubMed PubMed Central' in bibliomixed elements."""
    count = 0
    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if 'PubMedPubMed' not in content:
            continue

        new_content = content.replace('PubMedPubMed Central', 'PubMed PubMed Central')
        # Also handle case where PubMedCentral appears as one word (from source EPUB)
        new_content = new_content.replace('PubMedPubMedCentral', 'PubMed PubMedCentral')

        if new_content != content:
            occurrences = content.count('PubMedPubMed') - new_content.count('PubMedPubMed')
            count += occurrences
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)

    return count


def fix_contact_of_author(xml_dir: str) -> int:
    """A2: Strip <emphasis>ContactOf Author N</emphasis> from superscript elements."""
    count = 0
    pattern = re.compile(r'ContactOf\s*Author\s*\d+', re.IGNORECASE)

    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if 'ContactOf' not in content:
            continue

        try:
            tree = etree.fromstring(content.encode('utf-8'))
        except etree.XMLSyntaxError:
            continue

        modified = False
        # Find all emphasis elements with ContactOfAuthor text
        for emphasis in tree.iter('emphasis'):
            text = (emphasis.text or '').strip()
            if pattern.match(text):
                parent = emphasis.getparent()
                if parent is not None:
                    # Preserve tail text
                    tail = emphasis.tail or ''
                    prev = emphasis.getprevious()
                    if prev is not None:
                        prev.tail = (prev.tail or '') + tail
                    else:
                        parent.text = (parent.text or '') + tail
                    parent.remove(emphasis)
                    modified = True
                    count += 1

        if modified:
            result = etree.tostring(tree, xml_declaration=True, encoding='UTF-8').decode('utf-8')
            # Preserve original formatting style
            result = _restore_xml_formatting(result)
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(result)

    return count


def fix_heading_number_spacing(xml_dir: str) -> int:
    """A3: Add space after <emphasis role="HeadingNumber">N</emphasis> before title text."""
    count = 0
    # Pattern: </emphasis> immediately followed by a letter (no space)
    pattern = re.compile(
        r'(<emphasis role="HeadingNumber">[^<]*</emphasis>)([A-Za-z\u00C0-\u024F])'
    )

    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        if 'toc.' in os.path.basename(filepath):
            continue  # TOC handled separately

        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if 'HeadingNumber' not in content:
            continue

        new_content = pattern.sub(r'\1 \2', content)
        if new_content != content:
            occurrences = len(pattern.findall(content))
            count += occurrences
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)

    return count


def fix_toc_heading_spacing(xml_dir: str) -> int:
    """A9: Fix heading number spacing in TOC entries and empty linkend."""
    toc_file = os.path.join(xml_dir, f'toc.{ISBN}.xml')
    if not os.path.exists(toc_file):
        return 0

    with open(toc_file, 'r', encoding='utf-8') as f:
        content = f.read()

    count = 0

    # Fix section number followed immediately by letter: "1Introduction" → "1 Introduction"
    # Match patterns like: >1Introduction< or >3.2Fluorescence< inside tocentry elements
    def fix_toc_number_spacing(match):
        nonlocal count
        count += 1
        return match.group(1) + ' ' + match.group(2)

    # Pattern: digit(s) optionally followed by .digit(s) immediately followed by uppercase letter
    new_content = re.sub(
        r'(\d+(?:\.\d+)*)\s*([A-Z])',
        fix_toc_number_spacing,
        content
    )

    # Fix empty linkend - set to empty or remove the problematic entry
    # <tocfront linkend="">About</tocfront> → remove the empty linkend entry
    new_content = re.sub(
        r'\s*<tocfront linkend="">About</tocfront>\s*\n?',
        '\n',
        new_content
    )

    # Clean up excessive blank lines (more than 2 consecutive)
    new_content = re.sub(r'\n{4,}', '\n\n', new_content)

    if new_content != content:
        with open(toc_file, 'w', encoding='utf-8') as f:
            f.write(new_content)

    return count


def fix_doi_display_text(xml_dir: str) -> int:
    """A4: Fix DOI display text by using the url attribute value."""
    count = 0

    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if 'doi.' not in content or 'ulink' not in content:
            continue

        try:
            tree = etree.fromstring(content.encode('utf-8'))
        except etree.XMLSyntaxError:
            continue

        modified = False
        for ulink in tree.iter('ulink'):
            url = ulink.get('url', '')
            text = ulink.text or ''
            if 'doi.org' in url and '. ' in text:
                # Replace display text with the clean URL
                ulink.text = url
                # Remove any children that might contain broken text
                for child in list(ulink):
                    ulink.remove(child)
                modified = True
                count += 1

        if modified:
            result = etree.tostring(tree, xml_declaration=True, encoding='UTF-8').decode('utf-8')
            result = _restore_xml_formatting(result)
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(result)

    return count


def fix_email_display_text(xml_dir: str) -> int:
    """A5: Fix email display text by using the url attribute value."""
    count = 0

    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if 'mailto:' not in content:
            continue

        try:
            tree = etree.fromstring(content.encode('utf-8'))
        except etree.XMLSyntaxError:
            continue

        modified = False
        for ulink in tree.iter('ulink'):
            url = ulink.get('url', '')
            text = ulink.text or ''
            if url.startswith('mailto:') and '. ' in text:
                # Extract clean email from url attribute
                email = url.replace('mailto:', '')
                ulink.text = email
                for child in list(ulink):
                    ulink.remove(child)
                modified = True
                count += 1

        if modified:
            result = etree.tostring(tree, xml_declaration=True, encoding='UTF-8').decode('utf-8')
            result = _restore_xml_formatting(result)
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(result)

    return count


def fix_copyright_run_together(xml_dir: str) -> int:
    """A6: Fix copyright text where year runs into editor name (e.g., '2025N.' → '2025 N.')."""
    count = 0

    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if 'ChapterCopyright' not in content:
            continue

        # Pattern: 4-digit year immediately followed by uppercase letter
        new_content = re.sub(r'(202\d)([A-Z])', r'\1 \2', content)
        if new_content != content:
            count += 1
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)

    return count


def fix_empty_risprev(xml_dir: str) -> int:
    """A7: Populate empty risprev tags in chapter title sections."""
    count = 0

    # Build ordering of sections from the file listing
    # Title sections are the highest-numbered sXXXX per chapter
    sect_files = sorted(glob.glob(os.path.join(xml_dir, f'sect1.{ISBN}.ch*.xml')))

    # Group files by chapter
    chapters = {}
    for f in sect_files:
        basename = os.path.basename(f)
        match = re.search(r'ch(\d{4})', basename)
        if match:
            ch_num = match.group(1)
            if ch_num not in chapters:
                chapters[ch_num] = []
            chapters[ch_num].append(basename)

    # Sort chapters and find each chapter's title section (highest sXXXX)
    sorted_chapters = sorted(chapters.keys())

    for i, ch_num in enumerate(sorted_chapters):
        sections = sorted(chapters[ch_num])
        # Title section is the last one (highest numbered)
        title_section = sections[-1]
        title_path = os.path.join(xml_dir, title_section)

        with open(title_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # Check for empty risprev
        empty_risprev = f'<risprev>sect1.{ISBN}.</risprev>'
        if empty_risprev not in content:
            continue

        # Determine correct previous section
        if i == 0:
            # First chapter - previous is the last preface section
            preface_files = sorted(glob.glob(os.path.join(xml_dir, f'preface.{ISBN}.pr*.xml')))
            if preface_files:
                prev_ref = os.path.basename(preface_files[-1]).replace('.xml', '')
            else:
                prev_ref = f'preface.{ISBN}.ch0001'  # About the Editor
        else:
            # Previous chapter's title section (last section of previous chapter)
            prev_ch = sorted_chapters[i - 1]
            prev_sections = sorted(chapters[prev_ch])
            prev_ref = prev_sections[-1].replace('.xml', '')

        new_risprev = f'<risprev>{prev_ref}</risprev>'
        new_content = content.replace(empty_risprev, new_risprev)
        if new_content != content:
            count += 1
            with open(title_path, 'w', encoding='utf-8') as f:
                f.write(new_content)

    return count


def fix_utf8_a_character(xml_dir: str) -> int:
    """A8: Fix double-encoded non-breaking space (Â followed by space)."""
    count = 0

    for filepath in glob.glob(os.path.join(xml_dir, '*.xml')):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        if '\u00c2\u00a0' in content or 'Â ' in content:
            # Replace Â followed by space (or NBSP) with single space
            new_content = content.replace('\u00c2\u00a0', ' ')  # Â + NBSP
            new_content = new_content.replace('Â ', ' ')  # Â + regular space
            if new_content != content:
                count += 1
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(new_content)

    return count


def _restore_xml_formatting(xml_string: str) -> str:
    """Restore XML formatting after lxml serialization.

    lxml's tostring produces slightly different output than the original.
    This function normalizes common differences.
    """
    # Ensure proper XML declaration
    if not xml_string.startswith('<?xml'):
        xml_string = '<?xml version="1.0" encoding="UTF-8"?>\n' + xml_string
    # lxml may output <?xml version='1.0' encoding='UTF-8'?> with single quotes
    xml_string = xml_string.replace("version='1.0'", 'version="1.0"')
    xml_string = xml_string.replace("encoding='UTF-8'", 'encoding="UTF-8"')
    return xml_string


def verify_wellformedness(xml_dir: str) -> list:
    """Verify all XML files are well-formed after fixes."""
    errors = []
    for filepath in sorted(glob.glob(os.path.join(xml_dir, '*.xml'))):
        basename = os.path.basename(filepath)
        if basename.startswith('book.'):
            continue  # book.xml has entity refs that need special handling
        try:
            etree.parse(filepath)
        except etree.XMLSyntaxError as e:
            errors.append((basename, str(e)))
    return errors


def main():
    if len(sys.argv) < 2:
        xml_dir = f'/tmp/qa_{ISBN}/{ISBN}'
    else:
        xml_dir = sys.argv[1]

    if not os.path.isdir(xml_dir):
        print(f"Error: Directory not found: {xml_dir}")
        sys.exit(1)

    print(f"Fixing XML package in: {xml_dir}")
    print("=" * 60)

    # Run all fixes
    fixes = [
        ("A1: PubMedPubMed Central spacing", fix_pubmed_spacing),
        ("A2: ContactOfAuthor metadata removal", fix_contact_of_author),
        ("A3: HeadingNumber spacing", fix_heading_number_spacing),
        ("A4: DOI display text", fix_doi_display_text),
        ("A5: Email display text", fix_email_display_text),
        ("A6: Copyright text run-together", fix_copyright_run_together),
        ("A7: Empty risprev population", fix_empty_risprev),
        ("A8: UTF-8 Â character", fix_utf8_a_character),
        ("A9: TOC heading spacing + cleanup", fix_toc_heading_spacing),
    ]

    total = 0
    for name, func in fixes:
        result = func(xml_dir)
        status = f"  {result} fixes" if result > 0 else "  no changes needed"
        print(f"  {name}: {status}")
        total += result

    print(f"\nTotal fixes applied: {total}")

    # Verify well-formedness
    print("\nVerifying XML well-formedness...")
    errors = verify_wellformedness(xml_dir)
    if errors:
        print(f"  ERRORS in {len(errors)} files:")
        for fname, err in errors:
            print(f"    {fname}: {err}")
    else:
        print("  All files well-formed ✓")

    print("\nDone.")


if __name__ == '__main__':
    main()
