#!/usr/bin/env python3
"""
XML Fix Script for ISBN 9783032088086
Book: Cancer: An Integrative Approach
Author: Doru Paul
Publisher: Springer Nature, 2026

Fixes identified in QA Report dated 2026-03-09:
  C-001: HeadingNumber spacing (231 instances)
  C-002/C-010: Copyright metadata concatenation (15+ instances)
  C-006: Double periods in references (455+ instances)
  C-007: UTF-8 double-encoding Â character (1 instance)
  C-011: Missing spaces around emphasis (653 instances)
  C-012/C-023: Concatenated PubMed/Crossref labels (3,956 instances)
  C-013: Period-space in DOIs/abbreviations (289 instances)
  C-014: ContactOf Author markers (15 instances)

Usage:
  python fix_9783032088086.py /path/to/9783032088086/
"""

import os
import re
import sys
from pathlib import Path


def fix_file(filepath: str) -> dict:
    """Apply all fixes to a single XML file. Returns dict of fix counts."""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content
    counts = {
        'C-001': 0,  # HeadingNumber spacing
        'C-002': 0,  # Copyright metadata concatenation
        'C-006': 0,  # Double periods
        'C-007': 0,  # UTF-8 Â character
        'C-011': 0,  # Emphasis spacing
        'C-012': 0,  # Concatenated PubMed/Crossref labels
        'C-013': 0,  # Period-space in DOIs/abbreviations
        'C-014': 0,  # ContactOf Author markers
    }

    # ===== C-001: HeadingNumber spacing =====
    # Add space after HeadingNumber emphasis when followed by uppercase letter
    # Pattern: </emphasis>A → </emphasis> A (only when preceded by HeadingNumber role)
    # We handle this by finding emphasis elements with HeadingNumber role
    # and ensuring the closing tag is followed by a space before the next word
    def fix_heading_number(m):
        counts['C-001'] += 1
        return m.group(1) + '</emphasis> ' + m.group(2)
    # Match: role="HeadingNumber">NUMBER</emphasis>LETTER
    # Space goes AFTER </emphasis>, not inside the element
    content = re.sub(
        r'(role="HeadingNumber">[^<]*?)\s*</emphasis>([A-Z])',
        fix_heading_number,
        content
    )
    # Also fix heading numbers in TOC entries: "1Introduction" → "1 Introduction"
    # Only apply within <tocentry> elements to avoid false positives
    if '<tocentry' in content:
        def fix_toc_heading(m):
            inner = m.group(2)
            fixed = re.sub(r'(\d)([A-Z])', r'\1 \2', inner)
            n_fixes = len(re.findall(r'\d[A-Z]', inner))
            counts['C-001'] += n_fixes
            return m.group(1) + fixed + m.group(3)
        content = re.sub(
            r'(<tocentry[^>]*>)(.*?)(</tocentry>)',
            fix_toc_heading,
            content,
            flags=re.DOTALL
        )

    # ===== C-014: ContactOf Author markers =====
    # Remove <emphasis>ContactOf Author N</emphasis> from superscript elements
    c014_pattern = r'\s*<emphasis>ContactOf Author \d+</emphasis>'
    c014_count = len(re.findall(c014_pattern, content))
    if c014_count > 0:
        content = re.sub(c014_pattern, '', content)
        counts['C-014'] = c014_count

    # ===== C-012/C-023: Concatenated PubMed/Crossref labels =====
    # Fix various concatenation patterns in bibliography entries
    label_fixes = [
        # Triple: CrossrefPubMedPubMed Central → (remove - these are unlinked labels)
        (r' ?CrossrefPubMedPubMed Central(?=</bibliomixed>)', ''),
        # Double: CrossrefPubMed Central (Crossref + PubMed Central) → (remove)
        (r' ?CrossrefPubMed Central(?=</bibliomixed>)', ''),
        # Double: CrossrefPubMed (no Central) → (remove)
        (r' ?CrossrefPubMed(?=</bibliomixed>)', ''),
        # Standalone concatenated: PubMedPubMed Central → (remove)
        (r' ?PubMedPubMed Central(?=</bibliomixed>)', ''),
        # Standalone PubMed Central before closing tag → (remove)
        (r' ?PubMed Central(?=</bibliomixed>)', ''),
        # Standalone PubMed before closing tag → (remove)
        (r' ?PubMed(?=</bibliomixed>)', ''),
        # Standalone Crossref before closing tag → (remove)
        (r' ?Crossref(?=</bibliomixed>)', ''),
    ]
    for pattern, replacement in label_fixes:
        found = len(re.findall(pattern, content))
        if found > 0:
            content = re.sub(pattern, replacement, content)
            counts['C-012'] += found

    # Clean up trailing whitespace before </bibliomixed> after label removal
    content = re.sub(r'\s+</bibliomixed>', '</bibliomixed>', content)

    # ===== C-006: Double periods =====
    # Fix double periods in bibliography entries: N.. → N.
    def fix_double_period(m):
        counts['C-006'] += 1
        return m.group(1) + '. ' + m.group(2)
    content = re.sub(
        r'(\w)\.\. (PMID|Epub|Erratum|http|https|doi|Available|Accessed|Retrieved|Published)',
        fix_double_period,
        content
    )
    # Also catch: N.. followed by space and uppercase
    def fix_double_period2(m):
        counts['C-006'] += 1
        return m.group(1) + '. '
    content = re.sub(
        r'(\d)\.\. (?=[A-Z])',
        fix_double_period2,
        content
    )

    # ===== C-013: Period-space fixes =====
    # Fix doi. org → doi.org
    c013_doi = len(re.findall(r'doi\. org', content))
    content = re.sub(r'doi\. org', 'doi.org', content)
    counts['C-013'] += c013_doi

    # Fix e. g. → e.g.
    c013_eg = len(re.findall(r'(?<!\w)e\. g\.', content))
    content = re.sub(r'(?<!\w)e\. g\.', 'e.g.', content)
    counts['C-013'] += c013_eg

    # Fix i. e. → i.e.
    c013_ie = len(re.findall(r'(?<!\w)i\. e\.', content))
    content = re.sub(r'(?<!\w)i\. e\.', 'i.e.', content)
    counts['C-013'] += c013_ie

    # Fix email spacing: med. cornell. edu → med.cornell.edu
    c013_email = len(re.findall(r'med\. cornell\. edu', content))
    content = re.sub(r'med\. cornell\. edu', 'med.cornell.edu', content)
    counts['C-013'] += c013_email

    # Fix vs. space patterns: p. value → p.value (only in specific contexts)
    # Be conservative — only fix known abbreviation patterns

    # ===== C-011: Missing spaces around emphasis =====
    # Fix missing space BEFORE <emphasis: lowercase letter immediately before
    def fix_emphasis_before(m):
        counts['C-011'] += 1
        return m.group(1) + ' <emphasis'
    content = re.sub(
        r'([a-z])<emphasis(?=[ >])',
        fix_emphasis_before,
        content
    )
    # Fix missing space AFTER </emphasis>: lowercase letter immediately after
    def fix_emphasis_after(m):
        counts['C-011'] += 1
        return '</emphasis> ' + m.group(1)
    content = re.sub(
        r'</emphasis>([a-z])',
        fix_emphasis_after,
        content
    )

    # ===== C-002/C-010: Copyright metadata concatenation =====
    # Fix year+author: "2025D. Paul" → "2025 D. Paul"
    def fix_year_author(m):
        counts['C-002'] += 1
        return m.group(1) + ' ' + m.group(2)
    content = re.sub(
        r'(202[0-9])([A-Z])',
        fix_year_author,
        content
    )
    # Fix country+author: "USADoru" → "USA Doru"
    def fix_country_author(m):
        counts['C-002'] += 1
        return m.group(1) + ' ' + m.group(2)
    content = re.sub(
        r'(USA|UK|Canada|Australia|Germany|France|Switzerland|Netherlands|India|China|Japan)([A-Z][a-z])',
        fix_country_author,
        content
    )

    # ===== C-007: UTF-8 double-encoding =====
    # When UTF-8 text is double-encoded, characters U+0080–U+00FF produce
    # a 2-byte sequence where the first byte decodes as one of:
    #   Â (U+00C2, from byte 0xC2) or Ã (U+00C3, from byte 0xC3)
    # followed by a character in U+0080–U+00BF range.
    # Common patterns:
    #   Â\xa0 = double-encoded NBSP (most common)
    #   Â© = double-encoded ©
    #   Ã¶ = double-encoded ö, etc.
    # Fix: decode the double-encoded bytes back to single characters
    def fix_double_encoding(content_text):
        """Fix all UTF-8 double-encoding artifacts in text."""
        count = 0
        result = []
        i = 0
        while i < len(content_text):
            ch = content_text[i]
            if ch in ('Â', 'Ã') and i + 1 < len(content_text):
                next_ch = content_text[i + 1]
                next_ord = ord(next_ch)
                # Check if next char is in the continuation byte range (0x80-0xBF)
                if 0x80 <= next_ord <= 0xBF:
                    # Reconstruct the original byte pair and decode
                    first_byte = ord(ch)  # 0xC2 or 0xC3
                    second_byte = next_ord
                    try:
                        original_char = bytes([first_byte, second_byte]).decode('utf-8')
                        result.append(original_char)
                        count += 1
                        i += 2
                        continue
                    except (UnicodeDecodeError, ValueError):
                        pass
            result.append(ch)
            i += 1
        return ''.join(result), count

    content, c007_count = fix_double_encoding(content)
    counts['C-007'] = c007_count

    # ===== Zero-width spaces in DOI URLs =====
    # Remove zero-width spaces (\u200b) from DOI display text
    zws_count = content.count('\u200b')
    if zws_count > 0:
        content = content.replace('\u200b', '')

    # Only write if changes were made
    if content != original:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)

    return counts


def main():
    if len(sys.argv) < 2:
        print(f"Usage: {sys.argv[0]} /path/to/9783032088086/")
        sys.exit(1)

    xml_dir = Path(sys.argv[1])
    if not xml_dir.is_dir():
        print(f"Error: {xml_dir} is not a directory")
        sys.exit(1)

    xml_files = sorted(xml_dir.glob('*.xml'))
    if not xml_files:
        print(f"Error: No XML files found in {xml_dir}")
        sys.exit(1)

    print(f"Processing {len(xml_files)} XML files in {xml_dir}")
    print("=" * 60)

    total_counts = {}
    files_modified = 0

    for filepath in xml_files:
        counts = fix_file(str(filepath))
        file_total = sum(counts.values())
        if file_total > 0:
            files_modified += 1
            # Aggregate counts
            for key, val in counts.items():
                total_counts[key] = total_counts.get(key, 0) + val
            # Print per-file summary if significant
            if file_total >= 5:
                print(f"  {filepath.name}: {file_total} fixes "
                      f"({', '.join(f'{k}:{v}' for k, v in counts.items() if v > 0)})")

    print("=" * 60)
    print(f"\nTotal fixes by issue:")
    grand_total = 0
    for key in sorted(total_counts.keys()):
        val = total_counts[key]
        grand_total += val
        print(f"  {key}: {val:,} fixes")

    print(f"\n  GRAND TOTAL: {grand_total:,} fixes across {files_modified} files")
    print(f"  (out of {len(xml_files)} total XML files)")


if __name__ == '__main__':
    main()
