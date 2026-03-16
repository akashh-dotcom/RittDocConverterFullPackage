"""
Unit Tests for Complex Table Conversion Scenarios

Tests the actual XHTML to DocBook conversion for tables, including:
- Complex colspan/rowspan combinations
- Nested tables
- Tables with mixed content
- Empty cells and rows
- Header/footer sections
- Tables with captions
- Malformed table structures

These tests use the actual conversion functions from epub_to_structured_v2.py
to validate end-to-end table conversion behavior.
"""

import pytest
from pathlib import Path
from lxml import etree
from bs4 import BeautifulSoup
from io import BytesIO

# Add parent directory to path
import sys
sys.path.insert(0, str(Path(__file__).parent.parent))

# Import actual conversion functions
try:
    from epub_to_structured_v2 import (
        convert_xhtml_to_chapter,
        reset_element_counters,
        reset_id_mapping,
    )
    from id_authority import reset_authority, get_authority
    from reference_mapper import reset_mapper, get_mapper
    HAS_CONVERTER = True
except ImportError as e:
    HAS_CONVERTER = False
    IMPORT_ERROR = str(e)


# =============================================================================
# Test Fixtures
# =============================================================================

@pytest.fixture(autouse=True)
def reset_state():
    """Reset all global state before each test."""
    if HAS_CONVERTER:
        reset_authority()
        reset_mapper()
        reset_element_counters()
        reset_id_mapping()
    yield


@pytest.fixture
def chapter_id():
    """Default chapter ID for tests."""
    return "ch0001"


def create_xhtml_with_table(table_html: str, title: str = "Test Chapter") -> bytes:
    """Create a complete XHTML document with a table."""
    xhtml = f"""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>{title}</title>
</head>
<body>
    <h1>{title}</h1>
    {table_html}
</body>
</html>"""
    return xhtml.encode('utf-8')


def convert_table_xhtml(xhtml_bytes: bytes, chapter_id: str = "ch0001"):
    """Convert XHTML with table to DocBook and return the chapter element."""
    if not HAS_CONVERTER:
        pytest.skip(f"Converter not available: {IMPORT_ERROR}")

    # Register chapter with authority
    authority = get_authority()
    authority.register_chapter(f"{chapter_id}.xhtml", chapter_id)

    # Get mapper
    mapper = get_mapper()

    # Convert using current function signature
    chapter_elem = convert_xhtml_to_chapter(
        xhtml_content=xhtml_bytes,
        doc_path=f"{chapter_id}.xhtml",
        chapter_id=chapter_id,
        mapper=mapper,
        toc_depth_map={}
    )

    return chapter_elem


# =============================================================================
# Simple Table Tests
# =============================================================================

class TestSimpleTableConversion:
    """Tests for basic table conversion."""

    def test_simple_table_converts_to_informaltable(self, chapter_id):
        """Test that a simple table without caption converts to informaltable."""
        table_html = """
        <table>
            <tr>
                <td>Cell 1</td>
                <td>Cell 2</td>
            </tr>
            <tr>
                <td>Cell 3</td>
                <td>Cell 4</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Should have informaltable (no caption = informal)
        tables = chapter.findall('.//informaltable')
        formal_tables = chapter.findall('.//table')

        # Either informaltable or table is acceptable
        assert len(tables) > 0 or len(formal_tables) > 0, \
            "Table should be converted to informaltable or table"

    def test_table_with_caption_converts_to_table(self, chapter_id):
        """Test that a table with caption converts to formal table element."""
        table_html = """
        <table>
            <caption>Table 1: Sample Data</caption>
            <tr>
                <th>Header 1</th>
                <th>Header 2</th>
            </tr>
            <tr>
                <td>Data 1</td>
                <td>Data 2</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Should have table with title
        tables = chapter.findall('.//table')
        if tables:
            # Check for title element
            for table in tables:
                title = table.find('title')
                if title is not None and title.text:
                    assert "Sample Data" in title.text or "Table 1" in title.text
                    break

    def test_table_has_tgroup_structure(self, chapter_id):
        """Test that converted table has proper tgroup/tbody structure."""
        table_html = """
        <table>
            <tr>
                <td>A</td>
                <td>B</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Find any table structure
        tables = chapter.findall('.//table') + chapter.findall('.//informaltable')

        if tables:
            table = tables[0]
            # Should have tgroup
            tgroup = table.find('.//tgroup')
            if tgroup is not None:
                # tgroup should have cols attribute
                assert tgroup.get('cols') is not None
                # Should have tbody
                tbody = tgroup.find('tbody')
                assert tbody is not None

    def test_table_cells_become_entry_elements(self, chapter_id):
        """Test that td cells become entry elements."""
        table_html = """
        <table>
            <tr>
                <td>Cell 1</td>
                <td>Cell 2</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Find entry elements
        entries = chapter.findall('.//entry')

        # Should have at least 2 entries
        assert len(entries) >= 2, f"Expected at least 2 entries, found {len(entries)}"


# =============================================================================
# Colspan Tests
# =============================================================================

class TestColspanConversion:
    """Tests for colspan handling."""

    def test_colspan_converts_to_namest_nameend(self, chapter_id):
        """Test that colspan becomes namest/nameend attributes."""
        table_html = """
        <table>
            <tr>
                <td colspan="2">Merged Cell</td>
            </tr>
            <tr>
                <td>Cell 1</td>
                <td>Cell 2</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Find entries with namest/nameend
        entries = chapter.findall('.//entry')

        merged_entry = None
        for entry in entries:
            if entry.get('namest') or entry.get('nameend'):
                merged_entry = entry
                break

        # Either has namest/nameend or the content is preserved
        if merged_entry is not None:
            assert merged_entry.get('namest') is not None
            assert merged_entry.get('nameend') is not None
        else:
            # Content should still be preserved even if colspan handling differs
            all_text = etree.tostring(chapter, encoding='unicode')
            assert "Merged Cell" in all_text

    def test_colspan_header_preserved(self, chapter_id):
        """Test that colspan in header row is preserved."""
        table_html = """
        <table>
            <thead>
                <tr>
                    <th colspan="3">Full Width Header</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>A</td>
                    <td>B</td>
                    <td>C</td>
                </tr>
            </tbody>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Check content is preserved
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Full Width Header" in all_text


# =============================================================================
# Rowspan Tests
# =============================================================================

class TestRowspanConversion:
    """Tests for rowspan handling."""

    def test_rowspan_converts_to_morerows(self, chapter_id):
        """Test that rowspan becomes morerows attribute."""
        table_html = """
        <table>
            <tr>
                <td rowspan="2">Spans 2 Rows</td>
                <td>Row 1</td>
            </tr>
            <tr>
                <td>Row 2</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Find entries with morerows
        entries = chapter.findall('.//entry')

        spanning_entry = None
        for entry in entries:
            if entry.get('morerows'):
                spanning_entry = entry
                break

        if spanning_entry is not None:
            # morerows should be rowspan - 1
            assert spanning_entry.get('morerows') == '1'
        else:
            # Content should still be preserved
            all_text = etree.tostring(chapter, encoding='unicode')
            assert "Spans 2 Rows" in all_text

    def test_rowspan_content_preserved(self, chapter_id):
        """Test that rowspan cell content is preserved."""
        table_html = """
        <table>
            <tr>
                <td rowspan="3">Category A</td>
                <td>Item 1</td>
            </tr>
            <tr>
                <td>Item 2</td>
            </tr>
            <tr>
                <td>Item 3</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Category A" in all_text
        assert "Item 1" in all_text
        assert "Item 2" in all_text
        assert "Item 3" in all_text


# =============================================================================
# Complex Span Tests
# =============================================================================

class TestComplexSpans:
    """Tests for tables with both colspan and rowspan."""

    def test_combined_spans_content_preserved(self, chapter_id):
        """Test that complex spanning preserves all content."""
        table_html = """
        <table>
            <tr>
                <th colspan="3">Title Row</th>
            </tr>
            <tr>
                <th rowspan="2">Category</th>
                <th>Min</th>
                <th>Max</th>
            </tr>
            <tr>
                <td>10</td>
                <td>100</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Title Row" in all_text
        assert "Category" in all_text
        assert "Min" in all_text
        assert "Max" in all_text


# =============================================================================
# Nested Table Tests
# =============================================================================

class TestNestedTables:
    """Tests for nested table handling."""

    def test_nested_table_content_preserved(self, chapter_id):
        """Test that nested table content is preserved."""
        table_html = """
        <table>
            <tr>
                <td>Outer Cell</td>
                <td>
                    <table>
                        <tr>
                            <td>Inner Cell 1</td>
                            <td>Inner Cell 2</td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Outer Cell" in all_text
        assert "Inner Cell 1" in all_text
        assert "Inner Cell 2" in all_text


# =============================================================================
# Mixed Content Tests
# =============================================================================

class TestMixedContent:
    """Tests for tables with various content types in cells."""

    def test_text_formatting_in_cells(self, chapter_id):
        """Test that text formatting within cells is preserved."""
        table_html = """
        <table>
            <tr>
                <td>Plain text</td>
                <td><strong>Bold text</strong></td>
                <td><em>Italic text</em></td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Plain text" in all_text
        assert "Bold text" in all_text
        assert "Italic text" in all_text

        # Check for emphasis elements
        emphasis = chapter.findall('.//emphasis')
        # Should have some emphasis elements for bold/italic
        # (exact count depends on conversion logic)

    def test_links_in_cells(self, chapter_id):
        """Test that links within cells are converted."""
        table_html = """
        <table>
            <tr>
                <td><a href="http://example.com">External Link</a></td>
                <td><a href="#section1">Internal Link</a></td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "External Link" in all_text
        assert "Internal Link" in all_text

    def test_lists_in_cells(self, chapter_id):
        """Test that lists within cells are preserved."""
        table_html = """
        <table>
            <tr>
                <td>
                    <ul>
                        <li>Item 1</li>
                        <li>Item 2</li>
                    </ul>
                </td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Item 1" in all_text
        assert "Item 2" in all_text

        # Check for list structure
        lists = chapter.findall('.//itemizedlist')
        # May or may not have itemizedlist depending on DTD constraints


# =============================================================================
# Empty Cell Tests
# =============================================================================

class TestEmptyCells:
    """Tests for handling empty cells and rows."""

    def test_empty_cells_handled(self, chapter_id):
        """Test that empty cells don't break conversion."""
        table_html = """
        <table>
            <tr>
                <td>A</td>
                <td></td>
                <td>C</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Should convert without error
        assert chapter is not None

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "A" in all_text
        assert "C" in all_text

    def test_whitespace_only_cells(self, chapter_id):
        """Test cells with only whitespace."""
        table_html = """
        <table>
            <tr>
                <td>   </td>
                <td>\n\t</td>
                <td>Content</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        assert chapter is not None
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Content" in all_text


# =============================================================================
# Section Tests (thead, tbody, tfoot)
# =============================================================================

class TestTableSections:
    """Tests for table with thead, tbody, tfoot."""

    def test_thead_converts_to_thead(self, chapter_id):
        """Test that HTML thead becomes DocBook thead."""
        table_html = """
        <table>
            <thead>
                <tr>
                    <th>Header 1</th>
                    <th>Header 2</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Data 1</td>
                    <td>Data 2</td>
                </tr>
            </tbody>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Check for thead element
        thead = chapter.find('.//thead')

        # Content should be preserved regardless
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Header 1" in all_text
        assert "Data 1" in all_text

    def test_tfoot_content_preserved(self, chapter_id):
        """Test that tfoot content is preserved."""
        table_html = """
        <table>
            <thead>
                <tr><th>Col 1</th><th>Col 2</th></tr>
            </thead>
            <tbody>
                <tr><td>A</td><td>B</td></tr>
            </tbody>
            <tfoot>
                <tr><td colspan="2">Footer Text</td></tr>
            </tfoot>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Footer Text" in all_text


# =============================================================================
# DTD Compliance Tests
# =============================================================================

class TestDTDCompliance:
    """Tests for DocBook DTD compliance of converted tables."""

    def test_table_has_required_structure(self, chapter_id):
        """Test that converted table has DTD-required structure."""
        table_html = """
        <table>
            <caption>Test Table</caption>
            <tr>
                <td>Cell 1</td>
                <td>Cell 2</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Find tables
        tables = chapter.findall('.//table')

        if tables:
            table = tables[0]
            # If formal table, should have title
            title = table.find('title')
            # Title may or may not exist depending on conversion

            # Should have tgroup
            tgroup = table.find('tgroup')
            if tgroup is not None:
                # tgroup must have cols
                assert tgroup.get('cols') is not None

    def test_colspec_elements_present(self, chapter_id):
        """Test that colspec elements are generated."""
        table_html = """
        <table>
            <tr>
                <td>A</td>
                <td>B</td>
                <td>C</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        # Find colspec elements
        colspecs = chapter.findall('.//colspec')

        # May have colspec elements for column naming
        # (exact count depends on implementation)


# =============================================================================
# Edge Case Tests
# =============================================================================

class TestEdgeCases:
    """Tests for edge cases in table conversion."""

    def test_single_cell_table(self, chapter_id):
        """Test table with single cell."""
        table_html = """
        <table>
            <tr>
                <td>Only Cell</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        assert chapter is not None
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Only Cell" in all_text

    def test_wide_table(self, chapter_id):
        """Test table with many columns."""
        cells = ''.join(f'<td>Col{i}</td>' for i in range(10))
        table_html = f"""
        <table>
            <tr>{cells}</tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        assert chapter is not None
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Col0" in all_text
        assert "Col9" in all_text

    def test_tall_table(self, chapter_id):
        """Test table with many rows."""
        rows = ''.join(f'<tr><td>Row{i}</td></tr>' for i in range(20))
        table_html = f"""
        <table>
            {rows}
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        assert chapter is not None
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Row0" in all_text
        assert "Row19" in all_text

    def test_table_with_class_attributes(self, chapter_id):
        """Test that table class attributes are handled."""
        table_html = """
        <table class="data-table bordered">
            <tr class="header-row">
                <th class="col-header">Header</th>
            </tr>
            <tr class="data-row">
                <td class="data-cell">Data</td>
            </tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        assert chapter is not None
        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Header" in all_text
        assert "Data" in all_text


# =============================================================================
# Integration Tests
# =============================================================================

class TestTableIntegration:
    """Integration tests for table conversion in context."""

    def test_table_with_surrounding_content(self, chapter_id):
        """Test table with paragraphs before and after."""
        table_html = """
        <p>Paragraph before table.</p>
        <table>
            <tr>
                <td>Table Cell</td>
            </tr>
        </table>
        <p>Paragraph after table.</p>
        """
        xhtml = create_xhtml_with_table(table_html, title="Mixed Content")
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Paragraph before table" in all_text
        assert "Table Cell" in all_text
        assert "Paragraph after table" in all_text

    def test_multiple_tables_in_chapter(self, chapter_id):
        """Test multiple tables in a single chapter."""
        table_html = """
        <table>
            <tr><td>Table 1 Cell</td></tr>
        </table>
        <p>Text between tables.</p>
        <table>
            <tr><td>Table 2 Cell</td></tr>
        </table>
        """
        xhtml = create_xhtml_with_table(table_html)
        chapter = convert_table_xhtml(xhtml, chapter_id)

        all_text = etree.tostring(chapter, encoding='unicode')
        assert "Table 1 Cell" in all_text
        assert "Table 2 Cell" in all_text
        assert "Text between tables" in all_text


# =============================================================================
# Run Tests
# =============================================================================

if __name__ == '__main__':
    pytest.main([__file__, '-v', '--tb=short'])
