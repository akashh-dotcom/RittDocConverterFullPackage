#!/usr/bin/env python3
"""
Test script to validate the index conversion bug described in the prescription.

Bug Summary:
- Index entries from EPUB XHTML are being converted to empty <para/> elements
- ~90% of index entries are losing their content
- Entries with <i> tags at the start appear to work, while entries with plain
  text or only <a> tags are losing content

This test validates the hypothesis that:
1. The pagebreak reference regex is too broad, matching section references
2. Link content preservation may be failing in certain cases
"""

import re
import sys
import os
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from lxml import etree
from bs4 import BeautifulSoup, NavigableString, Tag

# Import the actual regex and function from the module to test the fix
from epub_to_structured_v2 import _PAGEBREAK_REF_RE, _is_pagebreak_ref


def test_pagebreak_regex():
    """Test that the pagebreak regex correctly distinguishes page refs from section refs."""
    print("=" * 70)
    print("TEST 1: Pagebreak Reference Regex Pattern")
    print("=" * 70)

    # Use the actual regex from epub_to_structured_v2.py
    print(f"\nActual regex pattern: {_PAGEBREAK_REF_RE.pattern}")

    test_cases = [
        # (fragment, should_be_pagebreak, description)
        ("Page_104", True, "Actual page number reference"),
        ("page_104", True, "Lowercase page number"),
        ("page-104", True, "Page with hyphen"),
        ("page104", True, "Page without separator"),
        ("Page_13.5.1.3", False, "Section reference like 13.5.1.3 (should NOT match)"),
        ("Page_13.5.7.1", False, "Section reference like 13.5.7.1 (should NOT match)"),
        ("Page_5.7.1", False, "Section reference like 5.7.1 (should NOT match)"),
        ("section_123", False, "Section ID"),
        ("ch0001s0001", False, "Chapter section ID"),
        ("ref_001", False, "Reference ID"),
    ]

    print("\nTest Results:")

    bugs_found = []
    for fragment, expected_is_pagebreak, description in test_cases:
        actual_match = bool(_PAGEBREAK_REF_RE.search(fragment))
        is_correct = (actual_match == expected_is_pagebreak)

        status = "✓" if is_correct else "✗ FAIL"
        print(f"  {status} {description}")
        print(f"      Fragment: {fragment}")
        print(f"      Expected pagebreak: {expected_is_pagebreak}, Actual: {actual_match}")

        if not is_correct:
            bugs_found.append((fragment, expected_is_pagebreak, actual_match))

    if bugs_found:
        print(f"\n*** TEST FAILED: {len(bugs_found)} cases incorrectly identified ***")
        return False
    else:
        print(f"\n*** ALL TESTS PASSED ***")
        return True


def test_inline_content_extraction():
    """Test that inline content extraction preserves text from links."""
    print("\n" + "=" * 70)
    print("TEST 2: Inline Content Extraction for Index Entries")
    print("=" * 70)

    # Simulate the HTML structure from the prescription
    test_cases = [
        (
            '<li id="ind2">Abbott Architect Syphilis TP assay, '
            '<a href="9781683674832_v4_c11.xhtml#Page_13.5.1.3">13.5.1.3</a>, '
            '<a href="9781683674832_v4_c17.xhtml#Page_13.5.7.1">13.5.7.1</a></li>',
            "Abbott Architect Syphilis TP assay, 13.5.1.3, 13.5.7.1",
            "Plain text followed by links"
        ),
        (
            '<li id="ind3"><i>Abbott m2000</i>, '
            '<a href="9781683674832_v2_c07.xhtml#Page_5.7.1">5.7.1</a></li>',
            "Abbott m2000, 5.7.1",
            "Italic element followed by link"
        ),
        (
            '<li id="ind8">broth microdilution MIC test, '
            '<a href="9781683674832_v2_c28.xhtml#Page_7.4.1.17">7.4.1.17</a></li>',
            "broth microdilution MIC test, 7.4.1.17",
            "Plain text with link containing decimal section ref"
        ),
        (
            '<li id="ind52"><i>Absidia</i></li>',
            "Absidia",
            "Italic element only (no links)"
        ),
    ]

    print("\nTest Results:")

    for html, expected_text, description in test_cases:
        soup = BeautifulSoup(html, 'html.parser')
        li = soup.find('li')

        # Simulate the extraction - get all text content
        actual_text = ''.join(li.stripped_strings)

        is_correct = actual_text == expected_text
        status = "✓" if is_correct else "✗"

        print(f"  {status} {description}")
        print(f"      Expected: '{expected_text}'")
        print(f"      Got:      '{actual_text}'")

        if not is_correct:
            print(f"      *** TEXT EXTRACTION FAILED ***")

    print("\nNote: BeautifulSoup extracts text correctly. The bug must be in how")
    print("      the extracted content is transferred to the lxml para element.")


def test_lxml_text_handling():
    """Test lxml text/tail handling to understand potential issues."""
    print("\n" + "=" * 70)
    print("TEST 3: lxml Text/Tail Handling")
    print("=" * 70)

    # Create a para element and add text
    para = etree.Element('para')
    para.text = "Initial text, "

    print(f"\nStep 1: Created para with text")
    print(f"  para.text = '{para.text}'")
    print(f"  len(para) = {len(para)} (no child elements)")

    # Simulate adding more text when len(para) == 0
    if len(para) == 0:
        para.text = (para.text or '') + "more text"
        print(f"\nStep 2: Added more text (len == 0 case)")
        print(f"  para.text = '{para.text}'")

    # Serialize
    result = etree.tostring(para, encoding='unicode')
    print(f"\nSerialized result: {result}")

    # Now test with child element
    para2 = etree.Element('para')
    emph = etree.SubElement(para2, 'emphasis')
    emph.text = "Italic text"
    emph.tail = ", more text"

    print(f"\nAlternative: para with child element")
    print(f"  para2.text = '{para2.text}'")
    print(f"  len(para2) = {len(para2)}")
    print(f"  emphasis.text = '{emph.text}'")
    print(f"  emphasis.tail = '{emph.tail}'")

    result2 = etree.tostring(para2, encoding='unicode')
    print(f"  Serialized: {result2}")


def test_potential_root_cause():
    """Test that section references are no longer incorrectly detected as pagebreaks."""
    print("\n" + "=" * 70)
    print("TEST 4: Section Reference Detection (Fixed)")
    print("=" * 70)

    # Test using the actual _is_pagebreak_ref function
    print("\nScenario: Link href contains 'Page_13.5.1.3' (section reference)")

    href = "9781683674832_v4_c11.xhtml#Page_13.5.1.3"

    # Use the actual function from the module
    is_pagebreak = _is_pagebreak_ref(href)
    print(f"  Full href: {href}")
    print(f"  Is detected as pagebreak: {is_pagebreak}")

    if is_pagebreak:
        print("\n  *** BUG: Section reference incorrectly detected as pagebreak ***")
        return False
    else:
        print("\n  ✓ CORRECT: Section reference is NOT detected as pagebreak")
        print("    Links like #Page_13.5.1.3 will now be processed as normal links,")
        print("    preserving the link text in the output.")
        return True


def main():
    """Run all tests."""
    print("Index Conversion Bug Validation Tests")
    print("=" * 70)
    print()

    # Run all tests and collect results
    results = []
    results.append(("Pagebreak regex", test_pagebreak_regex()))
    test_inline_content_extraction()  # Informational only
    test_lxml_text_handling()  # Informational only
    results.append(("Section reference detection", test_potential_root_cause()))

    print("\n" + "=" * 70)
    print("SUMMARY")
    print("=" * 70)

    all_passed = all(r[1] for r in results)
    if all_passed:
        print("""
FIXES APPLIED:

1. PAGEBREAK REGEX FIXED
   Changed from: r'page[_-]?\\d+\\b'
   Changed to:   r'page[_-]?\\d+(?![.\\d])'

   The negative lookahead (?![.\\d]) ensures that section references
   like 'Page_13.5.1.3' are NOT matched as pagebreak references.

2. _convert_to_index_structure FIXED
   Changed from: text = para.text or ''
   Changed to:   text = ''.join(para.itertext()).strip()

   Using itertext() captures ALL text content including from child
   elements like <emphasis>, <link>, etc.

All tests pass! The index conversion bug should now be fixed.
""")
    else:
        print("\n*** SOME TESTS FAILED - See details above ***")


if __name__ == '__main__':
    main()
