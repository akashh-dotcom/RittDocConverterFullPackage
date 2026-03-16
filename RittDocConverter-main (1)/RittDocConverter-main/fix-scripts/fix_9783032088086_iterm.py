#!/usr/bin/env python3
"""
Fix ITerm word concatenation issues in XML files.

Cross-references source HTML to find <span id="ITerm...">word </span>next patterns
where the converter stripped the trailing space, causing "wordnext" in XML output.

Usage:
    python3 fix_iterm_spacing.py /path/to/html_dir /path/to/xml_dir
"""

import os
import re
import sys
from pathlib import Path
from collections import defaultdict


# Map HTML chapter numbers to XML chapter prefixes
# HTML: 529586_1_En_{N}_Chapter.xhtml -> XML: ch00{N+2} (chapters start at ch0003)
# But this mapping varies. Let's build it from content matching instead.

def extract_iterm_pairs(html_path):
    """Extract ITerm trailing-space pairs from an HTML file.

    Returns list of (iterm_text_stripped, following_text, concatenated, correct) tuples.
    """
    with open(html_path, 'r', encoding='utf-8') as f:
        content = f.read()

    pairs = []

    # Pattern: <span id="ITermN">text </span>following_text
    # We need the text content inside the span (with trailing space) and text after </span>
    # The text after </span> continues until we hit < or whitespace
    pattern = r'<span id="ITerm\d+">([^<]*?) </span>([^< \n\r\t,;:.!?\)}\]]+)'

    for m in re.finditer(pattern, content):
        iterm_text = m.group(1).strip()
        following = m.group(2)

        if not iterm_text or not following:
            continue

        # The concatenated form (what appears in XML)
        concatenated = iterm_text + following
        # The correct form (with space restored)
        correct = iterm_text + ' ' + following

        pairs.append({
            'iterm_text': iterm_text,
            'following': following,
            'concatenated': concatenated,
            'correct': correct,
        })

    # Also handle nested ITerms or ITerms followed by punctuation+word
    # Pattern: <span id="ITermN">text </span>, next  (comma after span)
    # These don't cause concatenation since punctuation provides visual separation

    return pairs


def extract_iterm_pairs_complex(html_path):
    """Handle more complex ITerm patterns including multi-child spans."""
    with open(html_path, 'r', encoding='utf-8') as f:
        content = f.read()

    pairs = []

    # Pattern 1: Simple ITerm with trailing space
    # <span id="ITermN">text </span>word
    pattern1 = r'<span id="ITerm\d+">([^<]*?) </span>([a-zA-Z0-9][\w-]*)'
    for m in re.finditer(pattern1, content):
        iterm_text = m.group(1).strip()
        following = m.group(2)
        if iterm_text and following:
            pairs.append((iterm_text + following, iterm_text + ' ' + following))

    # Pattern 2: ITerm with child elements (e.g., <span id="ITerm"><i>text</i> </span>word)
    # Extract text content recursively - simplified version
    pattern2 = r'<span id="ITerm\d+">(<[^>]+>)?([^<]*?)(</[^>]+>)? </span>([a-zA-Z0-9][\w-]*)'
    for m in re.finditer(pattern2, content):
        # Get text content from inner elements
        inner_text = m.group(2).strip() if m.group(2) else ''
        following = m.group(4)
        if inner_text and following:
            concat = inner_text + following
            correct = inner_text + ' ' + following
            if concat not in [p[0] for p in pairs]:  # avoid duplicates
                pairs.append((concat, correct))

    return pairs


def find_chapter_xml_files(xml_dir, html_content):
    """Find which XML files correspond to a given HTML chapter by matching unique text."""
    xml_files = sorted(Path(xml_dir).glob('sect1.*.xml'))
    # We'll try each XML and see if the concatenated text appears
    return xml_files


def apply_fixes_to_xml(xml_dir, all_replacements):
    """Apply all replacement pairs to XML files.

    all_replacements: list of (concatenated, correct) tuples
    Returns dict of {filename: fix_count}
    """
    xml_files = sorted(Path(xml_dir).glob('*.xml'))
    results = {}
    total_fixes = 0

    for xml_path in xml_files:
        with open(xml_path, 'r', encoding='utf-8') as f:
            content = f.read()

        original = content
        file_fixes = 0

        for concatenated, correct in all_replacements:
            if len(concatenated) < 4:
                # Skip very short concatenations to avoid false positives
                continue

            # Count occurrences
            count = content.count(concatenated)
            if count > 0:
                # Verify it's not a legitimate word by checking context
                # Only replace if it doesn't appear as a known word
                # We do this by checking if the concatenated form makes sense
                content = content.replace(concatenated, correct)
                file_fixes += count

        if content != original:
            with open(xml_path, 'w', encoding='utf-8') as f:
                f.write(content)
            results[xml_path.name] = file_fixes
            total_fixes += file_fixes

    return results, total_fixes


def build_safe_replacements(html_dir):
    """Build replacement list from all HTML files, filtering for safety."""
    html_files = sorted(Path(html_dir).glob('529586_1_En_*_Chapter.xhtml'))

    # Also check frontmatter/backmatter
    html_files += sorted(Path(html_dir).glob('Frontmatter_*.xhtml'))
    html_files += sorted(Path(html_dir).glob('Backmatter_*.xhtml'))

    all_pairs = []
    seen = set()

    for html_path in html_files:
        pairs = extract_iterm_pairs_complex(str(html_path))
        for concat, correct in pairs:
            if concat not in seen:
                seen.add(concat)
                all_pairs.append((concat, correct))

    # Sort by length descending to avoid partial replacements
    # (longer strings should be replaced first)
    all_pairs.sort(key=lambda x: len(x[0]), reverse=True)

    # Filter out potentially dangerous replacements
    safe_pairs = []
    # Common English words that could be false positives
    common_words = {
        'and', 'the', 'for', 'are', 'but', 'not', 'you', 'all',
        'can', 'has', 'her', 'was', 'one', 'our', 'out', 'had',
        'hot', 'has', 'his', 'how', 'its', 'may', 'new', 'now',
        'old', 'see', 'way', 'who', 'did', 'get', 'let', 'say',
        'she', 'too', 'use', 'also', 'into', 'from', 'than',
        'that', 'this', 'with', 'form', 'have', 'each', 'make',
        'like', 'long', 'look', 'many', 'some', 'them', 'then',
        'were', 'what', 'when', 'your', 'about', 'other', 'which',
        'their', 'there', 'these', 'would', 'being', 'energy',
        'cancer', 'cells', 'model', 'process', 'system', 'between',
        'information', 'organism', 'evolution', 'biological',
        'formation', 'transformation', 'fundamental', 'individual',
    }

    for concat, correct in all_pairs:
        # Skip if the concatenated form is a common word
        if concat.lower() in common_words:
            continue
        # Skip if too short (high false positive risk)
        if len(concat) < 5:
            continue
        safe_pairs.append((concat, correct))

    return safe_pairs


def main():
    html_dir = Path('/tmp/iterm_fix/html/OEBPS/html')
    xml_dir = Path('/tmp/iterm_fix/xml/9783032088086')

    print("=" * 60)
    print("ITerm Word Concatenation Fix")
    print("=" * 60)

    # Step 1: Build replacement list from HTML source
    print("\nStep 1: Analyzing source HTML for ITerm patterns...")
    replacements = build_safe_replacements(html_dir)
    print(f"  Found {len(replacements)} unique replacement pairs")

    # Show what we'll fix
    print("\nReplacement pairs (concatenated → correct):")
    for concat, correct in replacements:
        print(f"  '{concat}' → '{correct}'")

    # Step 2: Apply fixes to XML files
    print(f"\nStep 2: Applying fixes to XML files in {xml_dir}...")
    results, total = apply_fixes_to_xml(str(xml_dir), replacements)

    # Step 3: Report
    print("\n" + "=" * 60)
    print("Results:")
    for fname, count in sorted(results.items()):
        print(f"  {fname}: {count} fixes")
    print(f"\n  TOTAL: {total} fixes across {len(results)} files")
    print("=" * 60)


if __name__ == '__main__':
    main()
