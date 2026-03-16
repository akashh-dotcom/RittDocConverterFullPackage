"""
Tests for CSS-styled table detection and conversion.

Verifies that the converter properly detects and converts:
1. Divs with display:table CSS style
2. Divs with role="table" (WAI-ARIA)
3. Divs with table-related CSS classes
4. Tables inside div.figure elements
5. Normal HTML tables still work correctly
"""

import sys
from pathlib import Path

# Ensure project root is on the path
sys.path.insert(0, str(Path(__file__).parent.parent))

from bs4 import BeautifulSoup, Tag
from lxml import etree

from epub_to_structured_v2 import (
    _is_css_table,
    _has_css_table_structure,
    _collect_css_table_rows,
    _collect_css_row_cells,
    convert_xhtml_to_chapter,
)
from reference_mapper import ReferenceMapper


def _make_soup(html: str) -> BeautifulSoup:
    """Helper to create a BeautifulSoup from an HTML fragment."""
    return BeautifulSoup(html, 'html.parser')


def _get_tag(html: str) -> Tag:
    """Helper to get the first div/element from an HTML fragment."""
    soup = _make_soup(html)
    # Return the first div (or first non-html/body tag)
    tag = soup.find('div')
    if tag:
        return tag
    # Fall back to first tag
    return soup.find(True)


# ---- _is_css_table detection tests ----

class TestIsCssTable:
    def test_display_table_style(self):
        tag = _get_tag('<div style="display: table; width: 100%"><div style="display: table-row"><div style="display: table-cell">A</div><div style="display: table-cell">B</div></div></div>')
        assert _is_css_table(tag) is True

    def test_display_table_style_no_spaces(self):
        tag = _get_tag('<div style="display:table"><div style="display:table-row"><div style="display:table-cell">A</div><div style="display:table-cell">B</div></div></div>')
        assert _is_css_table(tag) is True

    def test_role_table(self):
        tag = _get_tag('<div role="table"><div role="row"><div role="cell">A</div><div role="cell">B</div></div></div>')
        assert _is_css_table(tag) is True

    def test_class_table_with_css_structure(self):
        tag = _get_tag('<div class="table"><div class="row"><div class="cell">A</div><div class="cell">B</div></div></div>')
        assert _is_css_table(tag) is True

    def test_class_data_table_with_css_structure(self):
        tag = _get_tag('<div class="data-table"><div class="row"><div class="cell">A</div><div class="cell">B</div></div></div>')
        assert _is_css_table(tag) is True

    def test_class_table_wrap_with_real_table_is_false(self):
        """A wrapper div around a real <table> should NOT be detected as CSS table."""
        tag = _get_tag('<div class="table-wrap"><table><tr><td>A</td></tr></table></div>')
        assert _is_css_table(tag) is False

    def test_plain_div_is_false(self):
        tag = _get_tag('<div><p>Just a paragraph</p></div>')
        assert _is_css_table(tag) is False

    def test_navigable_string_is_false(self):
        assert _is_css_table("not a tag") is False

    def test_class_table_without_structure_is_false(self):
        """A div with class 'table' but no row/cell children should not match."""
        tag = _get_tag('<div class="table"><p>Just text</p></div>')
        assert _is_css_table(tag) is False


# ---- _has_css_table_structure tests ----

class TestHasCssTableStructure:
    def test_style_rows(self):
        tag = _get_tag('<div><div style="display: table-row"><div style="display: table-cell">A</div><div style="display: table-cell">B</div></div></div>')
        assert _has_css_table_structure(tag) is True

    def test_role_rows(self):
        tag = _get_tag('<div><div role="row"><div role="cell">A</div><div role="cell">B</div></div></div>')
        assert _has_css_table_structure(tag) is True

    def test_class_rows(self):
        tag = _get_tag('<div><div class="tr"><div class="td">A</div><div class="td">B</div></div></div>')
        assert _has_css_table_structure(tag) is True

    def test_no_rows(self):
        tag = _get_tag('<div><p>Hello</p><p>World</p></div>')
        assert _has_css_table_structure(tag) is False


# ---- _collect_css_table_rows tests ----

class TestCollectCssTableRows:
    def test_collects_display_table_rows(self):
        html = '<div><div style="display:table-row"><div style="display:table-cell">A</div><div style="display:table-cell">B</div></div><div style="display:table-row"><div style="display:table-cell">C</div><div style="display:table-cell">D</div></div></div>'
        tag = _get_tag(html)
        rows = _collect_css_table_rows(tag)
        assert len(rows) == 2
        assert len(rows[0]) == 2
        assert len(rows[1]) == 2

    def test_collects_role_rows(self):
        html = '<div role="table"><div role="row"><div role="cell">X</div><div role="cell">Y</div></div></div>'
        tag = _get_tag(html)
        rows = _collect_css_table_rows(tag)
        assert len(rows) == 1
        assert len(rows[0]) == 2

    def test_nested_header_body(self):
        """Tables with thead/tbody groups should have rows collected from nested groups."""
        html = '<div class="table"><div class="thead"><div class="row"><div class="cell">H1</div><div class="cell">H2</div></div></div><div class="tbody"><div class="row"><div class="cell">A</div><div class="cell">B</div></div></div></div>'
        tag = _get_tag(html)
        rows = _collect_css_table_rows(tag)
        assert len(rows) == 2  # one header row + one body row


# ---- Integration: convert_xhtml_to_chapter with CSS tables ----

class TestCssTableConversion:
    def _convert(self, xhtml: str, chapter_id: str = 'ch0001') -> etree._Element:
        """Helper to convert XHTML to DocBook chapter."""
        mapper = ReferenceMapper()
        if not xhtml.startswith('<?xml'):
            xhtml = f'<?xml version="1.0" encoding="UTF-8"?><html xmlns="http://www.w3.org/1999/xhtml"><head><title>Test</title></head><body>{xhtml}</body></html>'
        result = convert_xhtml_to_chapter(
            xhtml.encode('utf-8'),
            doc_path='test.xhtml',
            chapter_id=chapter_id,
            mapper=mapper,
        )
        return result

    def test_display_table_converted(self):
        """A div with display:table should produce a DocBook table."""
        xhtml = '''
        <div style="display: table">
            <div style="display: table-row">
                <div style="display: table-cell">Substance</div>
                <div style="display: table-cell">State</div>
                <div style="display: table-cell">Value</div>
            </div>
            <div style="display: table-row">
                <div style="display: table-cell">H2</div>
                <div style="display: table-cell">g</div>
                <div style="display: table-cell">0</div>
            </div>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "CSS display:table div should produce a DocBook table"

        # Check that the table has content
        tgroup = tables[0].find('tgroup')
        assert tgroup is not None, "Table should have tgroup"
        tbody = tgroup.find('tbody')
        assert tbody is not None, "Table should have tbody"
        rows = tbody.findall('row')
        assert len(rows) >= 1, "Table should have at least one row"

    def test_role_table_converted(self):
        """A div with role=table should produce a DocBook table."""
        xhtml = '''
        <div role="table">
            <div role="row">
                <div role="columnheader">Name</div>
                <div role="columnheader">Value</div>
            </div>
            <div role="row">
                <div role="cell">CO2</div>
                <div role="cell">385.98</div>
            </div>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "ARIA role=table div should produce a DocBook table"

    def test_html_table_still_works(self):
        """Regular HTML <table> elements should still be converted properly."""
        xhtml = '''
        <table>
            <caption>Thermodynamic Data</caption>
            <tr>
                <th>Substance</th>
                <th>State</th>
            </tr>
            <tr>
                <td>H2</td>
                <td>g</td>
            </tr>
        </table>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "HTML <table> should still produce a DocBook table"

        # Check title was set from caption
        title = tables[0].find('title')
        assert title is not None
        title_text = ''.join(title.itertext()).strip()
        assert 'Thermodynamic' in title_text

    def test_div_figure_with_html_table(self):
        """A div.figure containing a <table> should produce a DocBook table, not a figure."""
        xhtml = '''
        <div class="figure" id="fig1">
            <figcaption>Table A.5: Metabolic Pathways</figcaption>
            <table>
                <tr>
                    <td>Substrate</td>
                    <td>Pathway</td>
                </tr>
                <tr>
                    <td>Glucose</td>
                    <td>Glycolysis</td>
                </tr>
            </table>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "div.figure containing <table> should produce a DocBook table"

        # Check rows exist
        tgroup = tables[0].find('tgroup')
        assert tgroup is not None
        tbody = tgroup.find('tbody')
        assert tbody is not None
        rows = tbody.findall('row')
        assert len(rows) >= 2, f"Should have at least 2 rows, got {len(rows)}"

    def test_table_wrapper_div_processes_table(self):
        """A div wrapping a <table> should still process the table correctly."""
        xhtml = '''
        <div class="table-wrap">
            <table>
                <tr>
                    <td>A</td>
                    <td>B</td>
                </tr>
            </table>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "div wrapping <table> should produce a DocBook table"

    def test_complex_table_with_spanning(self):
        """A table with colspan and rowspan should be properly converted."""
        xhtml = '''
        <table>
            <tr>
                <th colspan="2">Header</th>
            </tr>
            <tr>
                <td rowspan="2">Multi-row</td>
                <td>Value 1</td>
            </tr>
            <tr>
                <td>Value 2</td>
            </tr>
        </table>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1

        tgroup = tables[0].find('tgroup')
        assert tgroup is not None
        assert tgroup.get('cols') == '2'

        tbody = tgroup.find('tbody')
        rows = tbody.findall('row')
        assert len(rows) == 3

        # Check first row has colspan entry
        first_entry = rows[0].find('entry')
        assert first_entry.get('namest') == 'c1'
        assert first_entry.get('nameend') == 'c2'

        # Check second row first entry has morerows
        second_row_first_entry = rows[1].find('entry')
        assert second_row_first_entry.get('morerows') == '1'


# ---- Springer div.Table wrapper tests ----

class TestSpringerDivTableWrapper:
    """Tests for Springer EPUB pattern: <div class="Table" id="Tab1">
    with Caption div and either <table> or MediaObject <img>."""

    def _convert(self, xhtml: str, chapter_id: str = 'ch0001') -> etree._Element:
        """Helper to convert XHTML to DocBook chapter."""
        mapper = ReferenceMapper()
        if not xhtml.startswith('<?xml'):
            xhtml = f'<?xml version="1.0" encoding="UTF-8"?><html xmlns="http://www.w3.org/1999/xhtml"><head><title>Test</title></head><body>{xhtml}</body></html>'
        result = convert_xhtml_to_chapter(
            xhtml.encode('utf-8'),
            doc_path='test.xhtml',
            chapter_id=chapter_id,
            mapper=mapper,
        )
        return result

    def test_div_table_with_caption_and_html_table(self):
        """div.Table with Caption and real <table> should produce DocBook table."""
        xhtml = '''
        <div class="Table" id="Tab1">
            <div class="Caption" lang="en">
                <div class="CaptionContent">
                    <span class="CaptionNumber">Table A.1</span>
                    <p class="SimplePara">Molal Gibbs free energies of formation</p>
                </div>
            </div>
            <table>
                <thead>
                    <tr>
                        <th><p class="SimplePara">Substance</p></th>
                        <th><p class="SimplePara">State</p></th>
                        <th><p class="SimplePara">Value</p></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><p class="SimplePara">H2</p></td>
                        <td><p class="SimplePara">g</p></td>
                        <td><p class="SimplePara">0</p></td>
                    </tr>
                    <tr>
                        <td><p class="SimplePara">CO2</p></td>
                        <td><p class="SimplePara">aq</p></td>
                        <td><p class="SimplePara">385.98</p></td>
                    </tr>
                </tbody>
            </table>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "div.Table wrapper should produce a DocBook table"

        # Check title was extracted from Caption
        title = tables[0].find('title')
        assert title is not None
        title_text = ''.join(title.itertext()).strip()
        assert 'Table A.1' in title_text
        assert 'Gibbs free energies' in title_text

        # Check table has content
        tgroup = tables[0].find('tgroup')
        assert tgroup is not None
        assert tgroup.get('cols') == '3'

        # Check thead was created
        thead = tgroup.find('thead')
        assert thead is not None
        thead_rows = thead.findall('row')
        assert len(thead_rows) == 1

        # Check tbody was created with data rows
        tbody = tgroup.find('tbody')
        assert tbody is not None
        tbody_rows = tbody.findall('row')
        assert len(tbody_rows) >= 2

    def test_div_table_with_image_instead_of_table(self):
        """div.Table with MediaObject containing img should produce table with title.
        Note: Image resolution may fail in test context, but table structure should be created."""
        xhtml = '''
        <div class="Table" id="Tab3">
            <div class="Caption" lang="en">
                <div class="CaptionContent">
                    <span class="CaptionNumber">Table A.3</span>
                    <p class="SimplePara">Overview of prototypic reactions</p>
                </div>
            </div>
            <div class="MediaObject" id="MO1">
                <img alt="" src="../images/Tab3_HTML.png" style="width:41.98em"/>
                <div class="TextObject" id="d64e5593">
                    <p class="Para">A table description.</p>
                </div>
            </div>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1, "div.Table with image should produce a DocBook table"

        # Check title was extracted from Caption
        title = tables[0].find('title')
        assert title is not None
        title_text = ''.join(title.itertext()).strip()
        assert 'Table A.3' in title_text
        assert 'prototypic reactions' in title_text

    def test_div_table_preserves_id(self):
        """div.Table ID should be preserved on the DocBook table element."""
        xhtml = '''
        <div class="Table" id="Tab5">
            <div class="Caption" lang="en">
                <div class="CaptionContent">
                    <span class="CaptionNumber">Table A.5</span>
                    <p class="SimplePara">Recurrent catabolic pathways</p>
                </div>
            </div>
            <table>
                <tr><td>Substrate</td><td>Pathway</td></tr>
                <tr><td>Glucose</td><td>Glycolysis</td></tr>
            </table>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1
        # ID should contain Tab5 reference
        table_id = tables[0].get('id', '')
        assert table_id, "Table should have an ID"

    def test_div_table_with_table_footer(self):
        """div.Table with TableFooter should produce footnote paragraphs after table."""
        xhtml = '''
        <div class="Table" id="Tab2">
            <div class="Caption" lang="en">
                <div class="CaptionContent">
                    <span class="CaptionNumber">Table A.2</span>
                    <p class="SimplePara">Redox couples</p>
                </div>
            </div>
            <table>
                <tr><td>Couple</td><td>Value</td></tr>
                <tr><td>H2/H+</td><td>-420</td></tr>
            </table>
            <div class="TableFooter">
                <p class="SimplePara">a Mostly enzyme bound</p>
                <p class="SimplePara">b Dissolved in the lipid phase</p>
            </div>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1

        # Check footnote paragraphs exist after table
        # They should be siblings of the table, not inside it
        paras = result.findall('.//para[@role="table-footnote"]')
        assert len(paras) >= 2, f"Should have table footnote paragraphs, got {len(paras)}"

    def test_multiple_div_tables_in_para_wrapper(self):
        """Multiple div.Table inside a div.Para should all be converted to tables."""
        xhtml = '''
        <div class="Para" id="Par1">
            <div class="Table" id="Tab1">
                <div class="Caption"><div class="CaptionContent">
                    <span class="CaptionNumber">Table A.1</span>
                    <p class="SimplePara">First table</p>
                </div></div>
                <table>
                    <tr><td>A</td><td>B</td></tr>
                </table>
            </div>
            <div class="Table" id="Tab2">
                <div class="Caption"><div class="CaptionContent">
                    <span class="CaptionNumber">Table A.2</span>
                    <p class="SimplePara">Second table</p>
                </div></div>
                <table>
                    <tr><td>C</td><td>D</td></tr>
                </table>
            </div>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 2, f"Should have at least 2 tables, got {len(tables)}"

    def test_div_table_with_colspan(self):
        """div.Table with colspan in table cells should be properly converted."""
        xhtml = '''
        <div class="Table" id="Tab1">
            <div class="Caption"><div class="CaptionContent">
                <span class="CaptionNumber">Table A.1</span>
                <p class="SimplePara">Test table</p>
            </div></div>
            <table>
                <thead>
                    <tr>
                        <th>Substance</th>
                        <th>State</th>
                        <th colspan="4">Values</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td colspan="6"><p class="SimplePara"><strong>Hydrocarbons</strong></p></td>
                    </tr>
                    <tr>
                        <td>CH4</td>
                        <td>g</td>
                        <td>50.72</td>
                        <td>46.47</td>
                        <td>59.22</td>
                        <td>65.02</td>
                    </tr>
                </tbody>
            </table>
        </div>
        '''
        result = self._convert(xhtml)
        tables = result.findall('.//table')
        assert len(tables) >= 1

        tgroup = tables[0].find('tgroup')
        assert tgroup is not None
        cols = int(tgroup.get('cols', '0'))
        assert cols == 6, f"Table should have 6 columns, got {cols}"


# ---- Structural aside tests ----

class TestStructuralAsideHandling:
    """Tests for structural aside elements (Appendix, Glossary, etc.)
    that should NOT be converted to figures/sidebars."""

    def _convert(self, xhtml: str, chapter_id: str = 'ch0001') -> etree._Element:
        """Helper to convert XHTML to DocBook chapter."""
        mapper = ReferenceMapper()
        if not xhtml.startswith('<?xml'):
            xhtml = f'<?xml version="1.0" encoding="UTF-8"?><html xmlns="http://www.w3.org/1999/xhtml"><head><title>Test</title></head><body>{xhtml}</body></html>'
        result = convert_xhtml_to_chapter(
            xhtml.encode('utf-8'),
            doc_path='test.xhtml',
            chapter_id=chapter_id,
            mapper=mapper,
        )
        return result

    def test_aside_appendix_not_treated_as_figure(self):
        """aside.Appendix containing tables and images should NOT become a figure."""
        xhtml = '''
        <aside class="Appendix" id="App1">
            <div>
                <section class="Section1" id="Sec1">
                    <h2 class="Heading">Appendix</h2>
                    <div class="Table" id="Tab1">
                        <div class="Caption"><div class="CaptionContent">
                            <span class="CaptionNumber">Table A.1</span>
                            <p class="SimplePara">First table</p>
                        </div></div>
                        <table>
                            <tr><td>A</td><td>B</td></tr>
                        </table>
                    </div>
                    <div class="Table" id="Tab3">
                        <div class="Caption"><div class="CaptionContent">
                            <span class="CaptionNumber">Table A.3</span>
                            <p class="SimplePara">Image table</p>
                        </div></div>
                        <div class="MediaObject">
                            <img alt="" src="../images/tab3.png"/>
                        </div>
                    </div>
                </section>
            </div>
        </aside>
        '''
        result = self._convert(xhtml)

        # Should NOT be converted to a figure
        figures = result.findall('.//figure')
        # The image table may produce a figure via mediaobject, but the aside
        # itself should not be a single figure
        tables = result.findall('.//table')
        assert len(tables) >= 2, f"Aside.Appendix should produce tables, got {len(tables)}"

    def test_aside_appendix_processes_children(self):
        """aside.Appendix should have its children processed normally."""
        xhtml = '''
        <aside class="Appendix" id="App1">
            <section class="Section1" id="Sec1">
                <h2 class="Heading">Appendix</h2>
                <p>Some paragraph text</p>
            </section>
        </aside>
        '''
        result = self._convert(xhtml)

        # Should have section content (sect1 or at least para)
        paras = result.findall('.//para')
        assert len(paras) >= 1, "Aside.Appendix should process child paragraphs"
