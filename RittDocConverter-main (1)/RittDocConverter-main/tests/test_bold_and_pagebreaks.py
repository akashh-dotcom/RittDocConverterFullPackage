"""
Tests for bold formatting and pagebreak anchor handling in EPUB to DocBook conversion.

These tests verify the fixes for:
1. Bold formatting in list items (Issue 1)
2. Page anchors in headings and list items (Issue 2)

Based on specification: Handling of Bold Formatting and Page Anchors (v1.0)
"""

import sys
import os

# Add project root to path
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

# Try to import pytest, but allow running without it
try:
    import pytest
    HAS_PYTEST = True
except ImportError:
    HAS_PYTEST = False

from bs4 import BeautifulSoup
from lxml import etree

from epub_to_structured_v2 import (
    extract_inline_content,
)

# Note: get_nested_pagebreak_ids and get_nested_anchor_ids were removed because
# R2 XSL has the anchor template output commented out (html.xsl), so preserving
# these IDs as anchors would produce no HTML output. Tests for these functions
# have been removed.
from reference_mapper import ReferenceMapper


def create_mapper():
    """Create a ReferenceMapper for testing."""
    return ReferenceMapper()


if HAS_PYTEST:
    @pytest.fixture
    def mapper():
        """Create a ReferenceMapper for testing."""
        return ReferenceMapper()


class TestBoldFormattingInListItems:
    """Test cases for Issue 1: Bold Formatting in List Items"""

    def test_simple_bold_list_item(self, mapper):
        """Test Case 1: Simple Bold List Item

        Input: <li id="test-1"><b>Bold Header</b></li>
        Expected: <para><emphasis role="strong">Bold Header</emphasis></para>
        """
        html = '<b>Bold Header</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')

        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)

        # Check that emphasis element was created
        emphasis = para.find('emphasis')
        assert emphasis is not None, "emphasis element should be created"
        assert emphasis.get('role') == 'strong', "emphasis should have role='strong'"
        assert emphasis.text == 'Bold Header', "emphasis should contain the bold text"

    def test_bold_with_following_text(self, mapper):
        """Test bold followed by inline text (no paragraph)

        Input: <li><b>Header</b> - Some inline description</li>
        Expected: <para><emphasis role="strong">Header</emphasis> - Some inline description</para>
        """
        html = '<li><b>Header</b> - Some inline description</li>'
        soup = BeautifulSoup(html, 'html.parser')
        li_elem = soup.find('li')

        para = etree.Element('para')
        extract_inline_content(li_elem, para, 'test.xhtml', 'ch0001', mapper)

        # Check structure
        emphasis = para.find('emphasis')
        assert emphasis is not None, "emphasis element should be created"
        assert emphasis.get('role') == 'strong'
        assert emphasis.text == 'Header'
        # Check tail text
        assert emphasis.tail is not None
        assert '- Some inline description' in emphasis.tail

    def test_bold_with_nested_italic(self, mapper):
        """Test bold with nested inline elements

        Input: <b>Header with <i>italic</i> text</b>
        Expected: <emphasis role="strong">Header with <emphasis>italic</emphasis> text</emphasis>
        """
        html = '<b>Header with <i>italic</i> text</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')

        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)

        # Check outer emphasis
        outer_emphasis = para.find('emphasis')
        assert outer_emphasis is not None
        assert outer_emphasis.get('role') == 'strong'
        assert outer_emphasis.text == 'Header with '

        # Check inner emphasis (italic)
        inner_emphasis = outer_emphasis.find('emphasis')
        assert inner_emphasis is not None
        assert inner_emphasis.get('role') is None, "italic should have no role"
        assert inner_emphasis.text == 'italic'
        assert inner_emphasis.tail == ' text'

    def test_multiple_bold_elements(self, mapper):
        """Test multiple bold elements in list item

        Input: <li><b>First</b> and <b>Second</b> items</li>
        Expected: Both bold elements should be preserved
        """
        html = '<li><b>First</b> and <b>Second</b> items</li>'
        soup = BeautifulSoup(html, 'html.parser')
        li_elem = soup.find('li')

        para = etree.Element('para')
        extract_inline_content(li_elem, para, 'test.xhtml', 'ch0001', mapper)

        # Check both emphasis elements
        emphasis_elems = para.findall('emphasis')
        assert len(emphasis_elems) == 2, "Both bold elements should be converted"
        assert emphasis_elems[0].text == 'First'
        assert emphasis_elems[1].text == 'Second'

    def test_strong_tag_same_as_b(self, mapper):
        """Test that <strong> is treated the same as <b>"""
        html = '<strong>Strong Header</strong>'
        soup = BeautifulSoup(html, 'html.parser')
        strong_elem = soup.find('strong')

        para = etree.Element('para')
        extract_inline_content(strong_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)

        emphasis = para.find('emphasis')
        assert emphasis is not None
        assert emphasis.get('role') == 'strong'
        assert emphasis.text == 'Strong Header'


class TestIncludeRootParameter:
    """Test cases for the include_root parameter in extract_inline_content"""

    def test_include_root_true_for_bold(self, mapper):
        """Test that include_root=True wraps the root element"""
        html = '<b>Bold text</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')

        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)

        # With include_root=True, <b> should be wrapped in emphasis
        emphasis = para.find('emphasis')
        assert emphasis is not None
        assert emphasis.get('role') == 'strong'
        assert emphasis.text == 'Bold text'

    def test_include_root_false_for_bold(self, mapper):
        """Test that include_root=False (default) only processes children"""
        html = '<b>Bold text</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')

        para = etree.Element('para')
        # Default behavior (include_root=False) - processes children only
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=False)

        # With include_root=False, <b>'s children are processed but <b> itself is not
        emphasis = para.find('emphasis')
        assert emphasis is None, "Without include_root, emphasis should not be created"
        assert para.text == 'Bold text'

    def test_include_root_for_italic(self, mapper):
        """Test include_root for italic elements"""
        html = '<em>Italic text</em>'
        soup = BeautifulSoup(html, 'html.parser')
        em_elem = soup.find('em')

        para = etree.Element('para')
        extract_inline_content(em_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)

        emphasis = para.find('emphasis')
        assert emphasis is not None
        assert emphasis.get('role') is None  # Italic has no role
        assert emphasis.text == 'Italic text'

    def test_include_root_for_nested_formatting(self, mapper):
        """Test include_root with nested formatting elements"""
        html = '<b>Bold with <i>italic</i> inside</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')

        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)

        # Outer emphasis (bold)
        outer = para.find('emphasis')
        assert outer is not None
        assert outer.get('role') == 'strong'
        assert outer.text == 'Bold with '

        # Inner emphasis (italic)
        inner = outer.find('emphasis')
        assert inner is not None
        assert inner.text == 'italic'
        assert inner.tail == ' inside'


def run_standalone_tests():
    """Run tests without pytest."""
    mapper = create_mapper()
    passed = 0
    failed = 0

    # Test 1: Simple bold
    try:
        html = '<b>Bold Header</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')
        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)
        emphasis = para.find('emphasis')
        assert emphasis is not None
        assert emphasis.get('role') == 'strong'
        assert emphasis.text == 'Bold Header'
        print('PASS: Simple bold list item')
        passed += 1
    except Exception as e:
        print(f'FAIL: Simple bold list item - {e}')
        failed += 1

    # Test 2: Bold with following text
    try:
        html = '<li><b>Header</b> - Some inline description</li>'
        soup = BeautifulSoup(html, 'html.parser')
        li_elem = soup.find('li')
        para = etree.Element('para')
        extract_inline_content(li_elem, para, 'test.xhtml', 'ch0001', mapper)
        emphasis = para.find('emphasis')
        assert emphasis is not None
        assert emphasis.get('role') == 'strong'
        assert emphasis.text == 'Header'
        assert emphasis.tail is not None and '- Some inline description' in emphasis.tail
        print('PASS: Bold with following text')
        passed += 1
    except Exception as e:
        print(f'FAIL: Bold with following text - {e}')
        failed += 1

    # Test 3: include_root=True wraps root element
    try:
        html = '<b>Bold text</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')
        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)
        emphasis = para.find('emphasis')
        assert emphasis is not None
        print('PASS: include_root=True wraps root element')
        passed += 1
    except Exception as e:
        print(f'FAIL: include_root=True wraps root element - {e}')
        failed += 1

    # Test 4: include_root=False only processes children
    try:
        html = '<b>Bold text</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')
        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=False)
        emphasis = para.find('emphasis')
        assert emphasis is None
        assert para.text == 'Bold text'
        print('PASS: include_root=False only processes children')
        passed += 1
    except Exception as e:
        print(f'FAIL: include_root=False only processes children - {e}')
        failed += 1

    # Test 5: Nested bold with italic
    try:
        html = '<b>Header with <i>italic</i> text</b>'
        soup = BeautifulSoup(html, 'html.parser')
        b_elem = soup.find('b')
        para = etree.Element('para')
        extract_inline_content(b_elem, para, 'test.xhtml', 'ch0001', mapper, include_root=True)
        outer_emphasis = para.find('emphasis')
        assert outer_emphasis is not None
        assert outer_emphasis.get('role') == 'strong'
        inner_emphasis = outer_emphasis.find('emphasis')
        assert inner_emphasis is not None
        assert inner_emphasis.text == 'italic'
        print('PASS: Bold with nested italic')
        passed += 1
    except Exception as e:
        print(f'FAIL: Bold with nested italic - {e}')
        failed += 1

    print(f'\n{passed} passed, {failed} failed')
    return failed == 0


if __name__ == '__main__':
    if HAS_PYTEST:
        import pytest
        pytest.main([__file__, '-v'])
    else:
        print('Running tests without pytest...\n')
        success = run_standalone_tests()
        sys.exit(0 if success else 1)
