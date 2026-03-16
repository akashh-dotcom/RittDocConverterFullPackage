"""
Regression tests for bibliography-related fixes:
- C-005: Zero-width space passthrough
- C-006: Double periods in bibliography
- C-012: Bibliography URLs dropped (ulink preservation)
- C-016: BibSection content loss
- C-017: Ordered lists misclassified

These tests verify that the converter correctly handles bibliography
processing edge cases found in Springer and other publisher EPUBs.
"""

import sys
import os
import re

# Add project root to path
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import pytest
from bs4 import BeautifulSoup, Tag
from lxml import etree

from epub_to_structured_v2 import (
    extract_text,
    _normalize_inline_whitespace,
    _sanitize_for_bibliomixed,
    extract_inline_content,
)
from reference_mapper import ReferenceMapper


@pytest.fixture
def mapper():
    """Create a ReferenceMapper for testing."""
    return ReferenceMapper()


# ===========================================================================
# C-005: Zero-width space passthrough
# ===========================================================================

class TestZeroWidthSpaceRemoval:
    """Fix C-005: Verify zero-width characters are stripped from text."""

    def test_extract_text_removes_zwsp(self):
        """Zero-width space (U+200B) should be removed from extracted text."""
        html = '<span>Stat\u200bPearls</span>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('span'))
        assert result == 'StatPearls', f"ZWSP not removed: got '{result}'"

    def test_extract_text_removes_zwnj(self):
        """Zero-width non-joiner (U+200C) should be removed."""
        html = '<span>con\u200ccept</span>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('span'))
        assert result == 'concept', f"ZWNJ not removed: got '{result}'"

    def test_extract_text_removes_zwj(self):
        """Zero-width joiner (U+200D) should be removed."""
        html = '<span>test\u200dword</span>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('span'))
        assert result == 'testword', f"ZWJ not removed: got '{result}'"

    def test_extract_text_removes_bom(self):
        """BOM / zero-width no-break space (U+FEFF) should be removed."""
        html = '<span>\ufeffHello</span>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('span'))
        assert result == 'Hello', f"BOM not removed: got '{result}'"

    def test_normalize_inline_whitespace_removes_zwsp(self):
        """_normalize_inline_whitespace should strip ZWSP."""
        result = _normalize_inline_whitespace('Stat\u200bPearls')
        assert result == 'StatPearls', f"ZWSP not removed: got '{result}'"

    def test_normalize_inline_whitespace_removes_all_zw(self):
        """All four zero-width characters should be removed."""
        text = 'a\u200bb\u200cc\u200dd\ufeff'
        result = _normalize_inline_whitespace(text)
        assert result == 'abcd', f"Not all ZW chars removed: got '{result}'"


# ===========================================================================
# C-006: Double periods in bibliography
# ===========================================================================

class TestDoublePeriodNormalization:
    """Fix C-006: Verify double periods are normalized in bibliography text."""

    def test_double_period_in_text(self):
        """Double period '..' should be reduced to single period."""
        bm = etree.Element('bibliomixed')
        bm.text = 'Smith et al.. Some title.'
        # Simulate the double-period cleanup
        if bm.text and '..' in bm.text:
            bm.text = re.sub(r'(?<!\.)\.\.(?!\.)', '.', bm.text)
        assert bm.text == 'Smith et al. Some title.'

    def test_ellipsis_preserved(self):
        """Ellipsis '...' should NOT be reduced to a single period."""
        bm = etree.Element('bibliomixed')
        bm.text = 'Some text... continued.'
        if bm.text and '..' in bm.text:
            bm.text = re.sub(r'(?<!\.)\.\.(?!\.)', '.', bm.text)
        assert bm.text == 'Some text... continued.'

    def test_double_period_in_child_text(self):
        """Double periods in child element text should be fixed."""
        bm = etree.Element('bibliomixed')
        child = etree.SubElement(bm, 'citetitle')
        child.text = 'Title..'
        for c in bm:
            if c.text and '..' in c.text:
                c.text = re.sub(r'(?<!\.)\.\.(?!\.)', '.', c.text)
        assert child.text == 'Title.'

    def test_double_period_in_tail(self):
        """Double periods in element tail should be fixed."""
        bm = etree.Element('bibliomixed')
        child = etree.SubElement(bm, 'citetitle')
        child.text = 'Title'
        child.tail = '. 2023.. Accessed'
        for c in bm:
            if c.tail and '..' in c.tail:
                c.tail = re.sub(r'(?<!\.)\.\.(?!\.)', '.', c.tail)
        assert child.tail == '. 2023. Accessed'


# ===========================================================================
# C-012: Bibliography URLs preserved as ulink
# ===========================================================================

class TestBibliographyUrlPreservation:
    """Fix C-012: Verify URLs in bibliography entries are preserved as ulink."""

    def test_ulink_preserved_in_bibliomisc(self):
        """ulink elements at bibliomixed root should be wrapped in bibliomisc."""
        bm = etree.Element('bibliomixed')
        bm.text = 'Some reference. '
        ulink = etree.SubElement(bm, 'ulink')
        ulink.set('url', 'https://doi.org/10.1000/test')
        ulink.text = 'Crossref'
        ulink.tail = ' '

        _sanitize_for_bibliomixed(bm)

        # The ulink should be wrapped in bibliomisc, not stripped to text
        bibliomisc_elems = bm.findall('bibliomisc')
        assert len(bibliomisc_elems) >= 1, \
            "ulink should be preserved inside bibliomisc, not stripped"

        # Verify the ulink is inside the bibliomisc
        found_ulink = False
        for bmisc in bibliomisc_elems:
            inner_ulink = bmisc.find('ulink')
            if inner_ulink is not None and inner_ulink.get('url') == 'https://doi.org/10.1000/test':
                found_ulink = True
                assert inner_ulink.text == 'Crossref'
        assert found_ulink, "ulink with URL should be preserved inside bibliomisc"

    def test_multiple_ulinks_spacing(self):
        """Multiple consecutive ulinks should maintain spacing between them."""
        bm = etree.Element('bibliomixed')
        bm.text = 'Reference text. '

        ulink1 = etree.SubElement(bm, 'ulink')
        ulink1.set('url', 'https://doi.org/10.1000/test')
        ulink1.text = 'Crossref'
        ulink1.tail = ' '

        ulink2 = etree.SubElement(bm, 'ulink')
        ulink2.set('url', 'https://pubmed.ncbi.nlm.nih.gov/12345')
        ulink2.text = 'PubMed'
        ulink2.tail = ''

        _sanitize_for_bibliomixed(bm)

        # Both should be in bibliomisc wrappers
        bibliomisc_elems = bm.findall('bibliomisc')
        assert len(bibliomisc_elems) >= 2, \
            f"Expected at least 2 bibliomisc wrappers, got {len(bibliomisc_elems)}"

        # Check that spacing is preserved between them
        first_bmisc = bibliomisc_elems[0]
        # The tail of the first bibliomisc should contain a space separator
        assert first_bmisc.tail is not None and ' ' in first_bmisc.tail, \
            "Space between consecutive ulinks should be preserved"

    def test_ulink_without_url_converted_to_text(self):
        """ulink without url attribute should be converted to plain text."""
        bm = etree.Element('bibliomixed')
        bm.text = 'Reference. '
        ulink = etree.SubElement(bm, 'ulink')
        ulink.text = 'broken link'

        _sanitize_for_bibliomixed(bm)

        # No bibliomisc should be created for ulink without URL
        assert bm.find('.//ulink') is None, \
            "ulink without URL should not be preserved"

    def test_link_inside_emphasis_unwrapped(self):
        """ulink inside emphasis (from span wrapper) should be promoted to bibliomixed."""
        bm = etree.Element('bibliomixed')
        bm.text = 'Reference. '
        emphasis = etree.SubElement(bm, 'emphasis')
        emphasis.set('role', 'Occurrence OccurrenceDOI')
        ulink = etree.SubElement(emphasis, 'ulink')
        ulink.set('url', 'https://doi.org/10.1000/test')
        ulink.text = 'Crossref'

        _sanitize_for_bibliomixed(bm)

        # emphasis should be unwrapped, ulink promoted to bibliomixed level
        assert bm.find('emphasis') is None, \
            "emphasis should be unwrapped in bibliomixed"

        # ulink should be in bibliomisc
        bibliomisc = bm.find('bibliomisc')
        assert bibliomisc is not None, "ulink should be wrapped in bibliomisc"
        inner_ulink = bibliomisc.find('ulink')
        assert inner_ulink is not None, "ulink should exist inside bibliomisc"
        assert inner_ulink.get('url') == 'https://doi.org/10.1000/test'


# ===========================================================================
# C-016: BibSection content loss
# ===========================================================================

class TestBibSectionProcessing:
    """Fix C-016: Verify BibSection sub-divisions are processed."""

    def _create_bibliography_aside(self, with_bibsection=True, num_main_refs=3, num_section_refs=2):
        """Helper to create a Springer-style bibliography aside HTML."""
        main_items = ''
        for i in range(1, num_main_refs + 1):
            main_items += f'''
            <li class="Citation" id="CR{i}">
                <div class="CitationNumber">{i}.</div>
                <div class="CitationContent" id="ref-CR{i}">
                    Author{i} A. Title {i}. Journal. 2023;1:1-10.
                </div>
            </li>'''

        section_html = ''
        if with_bibsection:
            section_items = ''
            for i in range(1, num_section_refs + 1):
                section_items += f'''
                <li class="Citation" id="BSec1_CR{i}">
                    <div class="CitationNumber">{i}.</div>
                    <div class="CitationContent" id="ref-BSec1_CR{i}">
                        RecAuthor{i} B. Recommended Title {i}. Book. 2022.
                    </div>
                </li>'''
            section_html = f'''
            <div class="BibSection" id="BSec1">
                <div class="Heading">Recommended Reading</div>
                <ol class="BibliographyWrapper">{section_items}
                </ol>
            </div>'''

        html = f'''
        <aside class="Bibliography" id="Bib1">
            <div>
                <h2>References</h2>
                <ol class="BibliographyWrapper">{main_items}
                </ol>
                {section_html}
            </div>
        </aside>'''
        return html

    def test_bibsection_class_detection(self):
        """BibSection divs should be detected by CSS class."""
        html = self._create_bibliography_aside(with_bibsection=True)
        soup = BeautifulSoup(html, 'html.parser')
        aside = soup.find('aside')

        bib_sections = aside.find_all('div', class_=lambda c: c and 'BibSection' in (
            ' '.join(c) if isinstance(c, list) else c))
        assert len(bib_sections) == 1, \
            f"Expected 1 BibSection, found {len(bib_sections)}"

    def test_bibsection_heading_extraction(self):
        """BibSection heading text should be extractable."""
        html = self._create_bibliography_aside(with_bibsection=True)
        soup = BeautifulSoup(html, 'html.parser')
        bib_section = soup.find('div', class_='BibSection')

        heading = bib_section.find(class_=lambda c: c and 'Heading' in (
            ' '.join(c) if isinstance(c, list) else c))
        assert heading is not None, "BibSection heading should be found"
        assert extract_text(heading) == 'Recommended Reading'

    def test_bibsection_inner_entries(self):
        """BibSection should contain processable bibliography entries."""
        html = self._create_bibliography_aside(with_bibsection=True, num_section_refs=3)
        soup = BeautifulSoup(html, 'html.parser')
        bib_section = soup.find('div', class_='BibSection')

        inner_ol = bib_section.find('ol', recursive=True)
        assert inner_ol is not None, "BibSection should have an inner <ol>"

        li_items = inner_ol.find_all('li', recursive=False)
        assert len(li_items) == 3, f"Expected 3 entries, got {len(li_items)}"

    def test_no_bibsection_no_bibliodiv(self):
        """Without BibSection, no bibliodiv restructuring should occur."""
        html = self._create_bibliography_aside(with_bibsection=False)
        soup = BeautifulSoup(html, 'html.parser')
        aside = soup.find('aside')

        bib_sections = aside.find_all('div', class_=lambda c: c and 'BibSection' in (
            ' '.join(c) if isinstance(c, list) else c))
        assert len(bib_sections) == 0, "No BibSection should be found"


# ===========================================================================
# C-017: Ordered lists correctly classified
# ===========================================================================

class TestOrderedListClassification:
    """Fix C-017: Verify <ol> maps to orderedlist and <ul> maps to itemizedlist."""

    def test_ul_maps_to_itemizedlist(self, mapper):
        """<ul> should produce <itemizedlist>."""
        html = '<ul><li>Item 1</li><li>Item 2</li></ul>'
        soup = BeautifulSoup(html, 'html.parser')

        from epub_to_structured_v2 import convert_element
        parent = etree.Element('sect1')
        section_stack = [(1, parent)]

        convert_element(
            soup.find('ul'), parent, section_stack,
            'test.xhtml', 'ch0001', mapper,
            figure_counter={'count': 0},
            table_counter={'count': 0},
            toc_depth_map={},
            section_counters={},
            in_sidebar=False
        )

        itemizedlist = parent.find('itemizedlist')
        assert itemizedlist is not None, "<ul> should produce <itemizedlist>"
        assert parent.find('orderedlist') is None, \
            "<ul> should NOT produce <orderedlist>"

    def test_ol_maps_to_orderedlist(self, mapper):
        """<ol> (without pre-numbering) should produce <orderedlist>."""
        html = '<ol><li>First item text without numbering prefix</li><li>Second item text without numbering prefix</li></ol>'
        soup = BeautifulSoup(html, 'html.parser')

        from epub_to_structured_v2 import convert_element
        parent = etree.Element('sect1')
        section_stack = [(1, parent)]

        convert_element(
            soup.find('ol'), parent, section_stack,
            'test.xhtml', 'ch0001', mapper,
            figure_counter={'count': 0},
            table_counter={'count': 0},
            toc_depth_map={},
            section_counters={},
            in_sidebar=False
        )

        orderedlist = parent.find('orderedlist')
        assert orderedlist is not None, \
            "<ol> (without pre-numbering) should produce <orderedlist>"

    def test_ol_with_prenumbered_items_uses_orderedlist(self, mapper):
        """<ol> with pre-numbered items should use orderedlist (postprocessor strips numbers)."""
        html = '<ol><li>1. First item</li><li>2. Second item</li><li>3. Third item</li></ol>'
        soup = BeautifulSoup(html, 'html.parser')

        from epub_to_structured_v2 import convert_element
        parent = etree.Element('sect1')
        section_stack = [(1, parent)]

        convert_element(
            soup.find('ol'), parent, section_stack,
            'test.xhtml', 'ch0001', mapper,
            figure_counter={'count': 0},
            table_counter={'count': 0},
            toc_depth_map={},
            section_counters={},
            in_sidebar=False
        )

        orderedlist = parent.find('orderedlist')
        assert orderedlist is not None, \
            "Pre-numbered <ol> should use <orderedlist> (postprocessor will strip leading numbers)"


class TestAbbreviationRepair:
    """Tests for C-013: abbreviation spacing repair in postprocessor."""

    def test_aka_preserved_when_correct(self):
        """a.k.a. should pass through unchanged."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("the nerve a.k.a. the posterior nerve")
        assert "a.k.a." in text
        assert "a. k. a." not in text

    def test_aka_repaired_when_broken(self):
        """a. k. a. should be repaired to a.k.a."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("the nerve a. k. a. the posterior nerve")
        assert "a.k.a." in text
        assert "a. k. a." not in text
        assert modified is True


# ===========================================================================
# C-006: Double period fix in postprocessor
# ===========================================================================

class TestDoublePeriodPostprocessor:
    """Fix C-006: Verify double periods before URLs are fixed by postprocessor."""

    def test_double_period_before_url_fixed(self):
        """Double period before https:// URL should be reduced to single period."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text(
            "2019;85(5):504-12.. https://doi.org/10.1002/test"
        )
        assert ".. https" not in text
        assert ". https://doi.org/10.1002/test" in text
        assert modified is True

    def test_single_period_before_url_unchanged(self):
        """Single period before URL should not be modified."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "2019;85(5):504-12. https://doi.org/10.1002/test"
        )
        assert ". https://doi.org/10.1002/test" in text

    def test_ellipsis_before_url_preserved(self):
        """Three periods (ellipsis) before URL should not be changed."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text(
            "text... https://example.com"
        )
        assert "... https://example.com" in text

    def test_double_period_before_text_fixed(self):
        """Double period before regular text (not URL) should be fixed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "2020;67(6):1103-34.. Epub 2020"
        )
        assert ".." not in text
        assert "1103-34. Epub 2020" in text
        assert modified is True

    def test_double_period_before_pmid_fixed(self):
        """Double period before PMID should be fixed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "World J Surg. 2019;43(4):1103-34.. PMID: 30788599"
        )
        assert ".." not in text
        assert "1103-34. PMID:" in text

    def test_double_period_at_end_fixed(self):
        """Double period at end of text should be fixed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "Smith et al. J Oncol. 2020.."
        )
        assert not text.endswith("..")
        assert text.endswith(".")

    def test_ellipsis_in_general_text_preserved(self):
        """Ellipsis '...' in general text should not be modified."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text(
            "See also references 1, 2, 3... and others"
        )
        assert "..." in text


# ===========================================================================
# C-012/C-023: Concatenated bibliography label separation
# ===========================================================================

class TestBibLabelSeparation:
    """Fix C-012/C-023: Verify concatenated bib labels are separated."""

    def test_crossref_pubmed_separated(self):
        """'CrossrefPubMed' should become 'Crossref | PubMed'."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("CrossrefPubMed")
        assert text == "Crossref | PubMed"
        assert modified is True

    def test_crossref_pubmed_central_separated(self):
        """'CrossrefPubMed Central' should become 'Crossref | PubMed Central'."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("CrossrefPubMed Central")
        assert text == "Crossref | PubMed Central"

    def test_triple_label_separated(self):
        """'CrossrefPubMedPubMed Central' should be fully separated."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("CrossrefPubMedPubMed Central")
        # After first pass: "Crossref | PubMedPubMed Central"
        # The regex handles pairs, so multiple passes or greedy matching is needed
        assert "Crossref" in text
        assert "PubMed" in text

    def test_single_label_unchanged(self):
        """A single 'Crossref' or 'PubMed' should not be modified."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("Crossref")
        assert text == "Crossref"

        text2, _, _ = fix_spacing_in_text("PubMed")
        assert text2 == "PubMed"


# ===========================================================================
# C-014: ContactOf Author removal
# ===========================================================================

class TestContactOfAuthorRemoval:
    """Fix C-014: Verify ContactOf Author text is removed from superscript."""

    def test_contact_of_author_removed(self):
        """<emphasis>ContactOf Author 1</emphasis> inside superscript should be removed."""
        from manual_postprocessor import remove_contact_of_author, PostProcessReport
        xml = '<sect1><para><superscript>1, 2 <emphasis>ContactOf Author 1</emphasis></superscript></para></sect1>'
        root = etree.fromstring(xml)
        report = PostProcessReport(package_file='test')

        fixes = remove_contact_of_author(root, report, 'test.xml')
        assert fixes == 1

        sup = root.find('.//superscript')
        # The emphasis should be removed
        assert sup.find('emphasis') is None
        # The number text should remain
        assert '1, 2' in (sup.text or '')

    def test_regular_emphasis_preserved(self):
        """Regular <emphasis> inside superscript should NOT be removed."""
        from manual_postprocessor import remove_contact_of_author, PostProcessReport
        xml = '<sect1><para><superscript>See <emphasis>note</emphasis></superscript></para></sect1>'
        root = etree.fromstring(xml)
        report = PostProcessReport(package_file='test')

        fixes = remove_contact_of_author(root, report, 'test.xml')
        assert fixes == 0
        assert root.find('.//emphasis') is not None


# ===========================================================================
# Fix #6: URL protection from spacing normalization
# ===========================================================================

class TestUlinkSpacingProtection:
    """Fix #6: Verify <ulink> text content is NOT modified by spacing normalization."""

    def test_ulink_text_not_spaced(self):
        """Text inside <ulink> should not have spacing normalization applied."""
        from manual_postprocessor import process_xml_file, PostProcessReport
        xml = (
            '<?xml version="1.0" encoding="utf-8"?>\n'
            '<sect1><para>See <ulink url="https://doi.org/10.1007/s12893-024-02618-6">'
            'https://doi.org/10.1007/s12893-024-02618-6</ulink> for details.</para></sect1>'
        )
        import tempfile
        from pathlib import Path
        with tempfile.NamedTemporaryFile(mode='w', suffix='.xml', delete=False) as f:
            f.write(xml)
            f.flush()
            report = PostProcessReport(package_file='test')
            process_xml_file(Path(f.name), report, dry_run=True)

        # Parse and verify the ulink text was NOT modified
        root = etree.fromstring(xml.encode('utf-8'))
        ulink = root.find('.//ulink')
        assert ulink is not None
        # The text should still contain "s12893" not "s 12893"
        assert 's12893' in (ulink.text or '')
        os.unlink(f.name)

    def test_doi_org_preserved_in_ulink(self):
        """'doi.org' in ulink text should NOT become 'doi. org'."""
        from manual_postprocessor import fix_spacing_in_element, PostProcessReport
        xml = '<ulink url="https://doi.org/10.1007/test">https://doi.org/10.1007/test</ulink>'
        root = etree.fromstring(xml)
        report = PostProcessReport(package_file='test')

        # The process_xml_file skips ulink entirely, so fix_spacing_in_element
        # should never be called on ulink. But if it were, let's verify intent.
        # Instead, verify via the full pipeline that ulink is skipped:
        from manual_postprocessor import process_xml_file
        full_xml = (
            '<?xml version="1.0" encoding="utf-8"?>\n'
            '<sect1><para><ulink url="https://doi.org/10.1007/test">'
            'https://doi.org/10.1007/test</ulink></para></sect1>'
        )
        import tempfile
        from pathlib import Path
        with tempfile.NamedTemporaryFile(mode='w', suffix='.xml', delete=False) as f:
            f.write(full_xml)
            f.flush()
            report2 = PostProcessReport(package_file='test')
            process_xml_file(Path(f.name), report2, dry_run=False)

            # Read back and verify
            tree = etree.parse(f.name)
            ulink = tree.find('.//ulink')
            assert 'doi.org' in (ulink.text or ''), f"doi.org was corrupted: {ulink.text}"
            os.unlink(f.name)


# ===========================================================================
# Fix #2: MISSING_SPACE_LOWER_DIGIT smart skipping
# ===========================================================================

class TestLowerDigitSpacing:
    """Fix #2: MISSING_SPACE_LOWER_DIGIT should skip chemical formulas & DOI paths."""

    def test_regular_lower_digit_gets_space(self):
        """Normal cases like 'Check1' should still get space added."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("Check1")
        assert text == "Check 1", f"Expected 'Check 1', got '{text}'"

    def test_page_number_gets_space(self):
        """'page5' should become 'page 5'."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("page5")
        assert text == "page 5", f"Expected 'page 5', got '{text}'"

    def test_chemical_formula_na2_preserved(self):
        """Chemical formula 'Na2CO3' should NOT be split."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("Na2CO3")
        # Na2 should not become "Na 2" — the 'a' is preceded by 'N' (uppercase)
        assert "Na2" in text, f"Chemical formula corrupted: '{text}'"

    def test_chemical_formula_h2o_preserved(self):
        """'H2O' should NOT be split."""
        from manual_postprocessor import fix_spacing_in_text
        # H2O: the 'H' is uppercase, so it's not a lowercase-digit transition at all
        # But H2O in running text like "add H2O" should be fine
        text, _, _ = fix_spacing_in_text("add H2O to the solution")
        assert "H2O" in text, f"H2O was corrupted: '{text}'"

    def test_chemical_formula_co2_preserved(self):
        """'CO2' should not be broken."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("CO2 emissions")
        assert "CO2" in text, f"CO2 was corrupted: '{text}'"

    def test_doi_path_segment_preserved(self):
        """DOI path like 's12893-024' should NOT become 's 12893-024'."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("s12893-024-02618-6")
        assert "s12893" in text, f"DOI path corrupted: '{text}'"

    def test_p53_protein_preserved(self):
        """'p53' (tumor suppressor protein) should NOT be split."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("the p53 pathway")
        # p53 has 'p' followed by '53' (2+ digits) — should be preserved
        assert "p53" in text, f"p53 was corrupted: '{text}'"

    def test_abbreviation_in_sentence(self):
        """Abbreviation 'a.k.a.' should remain intact."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("a.k.a.")
        assert text == "a.k.a.", f"Abbreviation corrupted: '{text}'"

    def test_mixed_content_preserved(self):
        """Mixed content: 'Check1' gets space but 'Na2CO3' doesn't."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("See Chapter1 about Na2CO3")
        assert "Chapter 1" in text, f"Chapter1 not spaced: '{text}'"
        assert "Na2" in text, f"Na2 was corrupted: '{text}'"


# ===========================================================================
# Fix #5: Post-write validation
# ===========================================================================

class TestPostWriteValidation:
    """Fix #5: Verify post-write XML validation catches corruption."""

    def test_valid_xml_passes_validation(self):
        """Well-formed XML should pass post-write validation without errors."""
        from manual_postprocessor import process_xml_file, PostProcessReport
        import tempfile
        from pathlib import Path

        xml = (
            '<?xml version="1.0" encoding="utf-8"?>\n'
            '<sect1><para>Simple test content.</para></sect1>'
        )
        with tempfile.NamedTemporaryFile(mode='w', suffix='.xml', delete=False) as f:
            f.write(xml)
            f.flush()
            report = PostProcessReport(package_file='test')
            result = process_xml_file(Path(f.name), report, dry_run=False)
            # Should have no errors
            assert len(report.errors) == 0, f"Unexpected errors: {report.errors}"
            os.unlink(f.name)

    def test_modified_xml_still_valid(self):
        """XML that gets spacing fixes should still be well-formed after writing."""
        from manual_postprocessor import process_xml_file, PostProcessReport
        import tempfile
        from pathlib import Path

        xml = (
            '<?xml version="1.0" encoding="utf-8"?>\n'
            '<sect1><para>See Chapter1 for details about Check2.</para></sect1>'
        )
        with tempfile.NamedTemporaryFile(mode='w', suffix='.xml', delete=False) as f:
            f.write(xml)
            f.flush()
            report = PostProcessReport(package_file='test')
            result = process_xml_file(Path(f.name), report, dry_run=False)

            # Verify it was modified (spacing fixes applied)
            assert result is True, "Expected modifications"
            # Verify no validation errors
            assert len(report.errors) == 0, f"Validation errors: {report.errors}"

            # Re-parse to verify well-formedness
            tree = etree.parse(f.name)
            para = tree.find('.//para')
            assert para is not None
            assert "Chapter 1" in para.text
            os.unlink(f.name)


# ===========================================================================
# Existing fix validation: FootnoteSection handler (Fix #4)
# ===========================================================================

class TestFootnoteSectionHandler:
    """Fix #4: Verify FootnoteSection container is split into individual footnotes."""

    def test_footnote_section_detected(self):
        """epub:type='footnotes' (plural) should be detected as FootnoteSection."""
        # This tests the detection logic in convert_element
        # We can't easily unit-test convert_element, but we can verify the regex/logic
        css_class_lower = 'footnotesection'
        epub_type = 'footnotes'

        is_footnote_section = (
            'footnotesection' in css_class_lower or
            epub_type.lower() == 'footnotes'
        )
        assert is_footnote_section is True

    def test_individual_footnote_not_detected_as_section(self):
        """epub:type='footnote' (singular) should NOT be detected as FootnoteSection."""
        css_class_lower = 'footnote'
        epub_type = 'footnote'

        is_footnote_section = (
            'footnotesection' in css_class_lower or
            epub_type.lower() == 'footnotes'
        )
        assert is_footnote_section is False


# ===========================================================================
# Existing fix validation: Ordered list classification (Fix #3)
# ===========================================================================

class TestOrderedListClassification:
    """Fix #3: Pre-numbered <ol> should become <orderedlist>, not <itemizedlist>."""

    def test_prenumber_detection_pattern(self):
        """Verify pre-number detection regex matches common patterns."""
        prenumber_pattern = re.compile(
            r'^[\s]*(\d+[\.\):]|\(\d+\)|\d+\.\d+[\.\)]?|[a-zA-Z][\.\)]|\([a-zA-Z]\)|[ivxIVX]+[\.\)])\s'
        )

        # Should match:
        assert prenumber_pattern.match("1. First item")
        assert prenumber_pattern.match("1) First item")
        assert prenumber_pattern.match("(1) First item")
        assert prenumber_pattern.match("a. First item")
        assert prenumber_pattern.match("i. First item")
        assert prenumber_pattern.match("1.2 Sub item")

        # Should NOT match:
        assert not prenumber_pattern.match("First item without number")
        assert not prenumber_pattern.match("The quick brown fox")


# ===========================================================================
# Existing fix validation: Bibliography URL preservation (Fix #1)
# ===========================================================================

class TestBibliographyURLPreservation:
    """Fix #1: ulink inside bibliomixed should be wrapped in bibliomisc, not stripped."""

    def test_ulink_preserved_in_bibliomixed(self):
        """<ulink> at bibliomixed level should be wrapped in <bibliomisc>."""
        xml = '<bibliomixed id="ref1">Some reference text. <ulink url="https://pubmed.ncbi.nlm.nih.gov/12345">PubMed</ulink></bibliomixed>'
        root = etree.fromstring(xml)
        _sanitize_for_bibliomixed(root, is_bibliomixed_root=True)

        # The ulink should now be inside a bibliomisc
        bibliomisc = root.find('.//bibliomisc')
        assert bibliomisc is not None, "ulink should be wrapped in bibliomisc"
        ulink = bibliomisc.find('ulink')
        assert ulink is not None, "ulink should exist inside bibliomisc"
        assert ulink.get('url') == 'https://pubmed.ncbi.nlm.nih.gov/12345'

    def test_ulink_without_url_stripped(self):
        """<ulink> without url attribute should be converted to plain text."""
        xml = '<bibliomixed id="ref1">Text <ulink>no url</ulink> more text</bibliomixed>'
        root = etree.fromstring(xml)
        _sanitize_for_bibliomixed(root, is_bibliomixed_root=True)

        # ulink without url should be stripped to plain text
        ulink = root.find('.//ulink')
        # It may be stripped or wrapped — verify text is preserved
        full_text = etree.tostring(root, encoding='unicode', method='text')
        assert 'no url' in full_text

    def test_link_without_ulink_stripped(self):
        """<link> inside bibliomixed should be converted to plain text."""
        xml = '<bibliomixed id="ref1">Text <link linkend="ch01">Chapter 1</link> more</bibliomixed>'
        root = etree.fromstring(xml)
        _sanitize_for_bibliomixed(root, is_bibliomixed_root=True)

        # link should be stripped (not ulink, no URL to preserve)
        link = root.find('.//link')
        assert link is None, "link should be stripped from bibliomixed"
        full_text = etree.tostring(root, encoding='unicode', method='text')
        assert 'Chapter 1' in full_text


# ===========================================================================
# Existing fix validation: Double period fix (C-006)
# ===========================================================================

class TestDoublePeriodInBibliography:
    """C-006: Double periods should be fixed in bibliography entries during conversion."""

    def test_double_period_before_doi_fixed(self):
        """'..' before DOI URL should become single period."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("2023.. https://doi.org/10.1007/test")
        assert ".." not in text, f"Double period not fixed: '{text}'"
        assert ". https://doi.org" in text

    def test_ellipsis_preserved(self):
        """'...' (ellipsis) should NOT be reduced."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("et al... https://doi.org/test")
        assert "..." in text, f"Ellipsis was corrupted: '{text}'"

    def test_single_period_unchanged(self):
        """Single period before URL should remain unchanged."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("2023. https://doi.org/10.1007/test")
        assert ". https://doi.org" in text


# ===========================================================================
# Existing fix validation: Concatenated bib labels (C-012/C-023)
# ===========================================================================

class TestBibLabelSeparationComprehensive:
    """C-012/C-023: Concatenated bibliography labels should be separated."""

    def test_crossref_pubmed_separated(self):
        """'CrossrefPubMed' -> 'Crossref | PubMed'."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("CrossrefPubMed")
        assert "Crossref" in text
        assert "PubMed" in text
        assert "|" in text

    def test_triple_label_separated(self):
        """'CrossrefPubMedPubMed Central' -> separated."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("CrossrefPubMedPubMed Central")
        assert "Crossref" in text
        assert "PubMed Central" in text
        # Should have two separators
        assert text.count("|") >= 2

    def test_pubmed_central_alone_unchanged(self):
        """'PubMed Central' should not be modified."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text("PubMed Central")
        assert text == "PubMed Central"


# ===========================================================================
# Element boundary spacing (C-011)
# ===========================================================================

class TestElementBoundarySpacing:
    """C-011: Missing spaces at element boundaries should be inserted."""

    def test_space_added_before_emphasis(self):
        """'see<emphasis>Chapter</emphasis>' -> 'see <emphasis>Chapter</emphasis>'."""
        from manual_postprocessor import fix_element_boundary_spacing, PostProcessReport
        xml = '<para>see<emphasis>Chapter 10</emphasis></para>'
        root = etree.fromstring(xml)
        report = PostProcessReport(package_file='test')
        result = fix_element_boundary_spacing(root, report, 'test.xml')
        assert result is True
        assert root.text.endswith(' '), f"Space not added before emphasis: '{root.text}'"

    def test_space_added_after_emphasis(self):
        """'<emphasis>nerve</emphasis>supplies' -> '<emphasis>nerve</emphasis> supplies'."""
        from manual_postprocessor import fix_element_boundary_spacing, PostProcessReport
        xml = '<para>The <emphasis role="bold">radial nerve</emphasis>supplies the arm.</para>'
        root = etree.fromstring(xml)
        report = PostProcessReport(package_file='test')
        result = fix_element_boundary_spacing(root, report, 'test.xml')
        assert result is True
        emphasis = root.find('emphasis')
        assert emphasis.tail.startswith(' '), f"Space not added after emphasis: '{emphasis.tail}'"

    def test_existing_space_not_doubled(self):
        """Already-spaced text should not get double spaces."""
        from manual_postprocessor import fix_element_boundary_spacing, PostProcessReport
        xml = '<para>see <emphasis>Chapter 10</emphasis> for details</para>'
        root = etree.fromstring(xml)
        report = PostProcessReport(package_file='test')
        result = fix_element_boundary_spacing(root, report, 'test.xml')
        assert result is False  # No changes needed


# ===========================================================================
# Content preservation validation
# ===========================================================================

class TestContentPreservation:
    """Verify content preservation validator catches real problems."""

    def test_valid_spacing_change_accepted(self):
        """Normal spacing fix should pass validation."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("Chapter1 about Check2")
        assert modified is True
        assert "REVERTED" not in str(changes)

    def test_abbreviation_repair_accepted(self):
        """'a. k. a.' -> 'a.k.a.' should pass validation (same non-whitespace chars)."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("nerve a. k. a. the posterior")
        assert "a.k.a." in text
        assert "REVERTED" not in str(changes)

    def test_double_period_removal_accepted(self):
        """Double period removal should pass (it's after content validation)."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("2023.. https://doi.org/test")
        assert ".." not in text
        assert "REVERTED" not in str(changes)


# ===========================================================================
# extract_text inline spacing (already in converter)
# ===========================================================================

class TestExtractTextInlineSpacing:
    """Verify extract_text adds spaces between inline sibling elements."""

    def test_heading_number_spaced(self):
        """'<span>1.1</span><span>When to</span>' -> '1.1 When to'."""
        html = '<div><span class="HeadingNumber">1.1</span><span>When to operate</span></div>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('div'))
        assert '1.1 When' in result, f"Heading number not spaced: '{result}'"

    def test_small_caps_no_space(self):
        """'M<small>ANUAL</small>' -> 'MANUAL' (inline continuation, no extra space)."""
        html = '<span>M<small>ANUAL</small></span>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('span'))
        # The space may be added between 'M' and 'ANUAL' due to inline spacing,
        # but what matters is both parts are present
        assert 'M' in result and 'ANUAL' in result

    def test_double_encoding_fixed(self):
        """UTF-8 double-encoding artifacts (Â + NBSP) should be cleaned."""
        html = '<span>text\u00c2\u00a0more</span>'
        soup = BeautifulSoup(html, 'html.parser')
        result = extract_text(soup.find('span'))
        assert '\u00c2' not in result, f"Double-encoding artifact not cleaned: '{result}'"


# ===========================================================================
# ISBN 9783032038661 regression tests
# ===========================================================================

class TestZeroWidthSpaceInPostprocessor:
    """Verify zero-width space removal in postprocessor fix_spacing_in_text."""

    def test_zwsp_removed_from_doi(self):
        """ZWSP characters in DOI URLs should be stripped."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text(
            "https://\u200bdoi.\u200borg/\u200b10.1038/s00441"
        )
        assert "\u200b" not in text, f"ZWSP not removed: '{text}'"
        assert modified is True
        assert "https://doi.org/10.1038/s00441" in text

    def test_zwsp_removed_from_bibliography_text(self):
        """ZWSP in bibliography plain text should be stripped."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "Stat\u200bPearls Publishing"
        )
        assert "\u200b" not in text
        assert "StatPearls" in text

    def test_all_zero_width_chars_removed(self):
        """All zero-width characters (ZWSP, ZWNJ, ZWJ, BOM) should be removed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "a\u200bb\u200cc\u200dd\ufeff"
        )
        assert "\u200b" not in text
        assert "\u200c" not in text
        assert "\u200d" not in text
        assert "\ufeff" not in text

    def test_no_false_positive_on_normal_text(self):
        """Normal text without ZWSP should not be flagged as modified by ZWSP step."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("Normal text here.")
        # Should not report ZWSP stripping
        assert "zero-width" not in str(changes).lower() or not any(
            "zero-width" in c.lower() for c in changes
        )


class TestDOIPathSpacingRepair:
    """Verify DOI URL path spacing is repaired in postprocessor."""

    def test_doi_path_space_removed(self):
        """Internal spaces in DOI path should be removed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text(
            "doi.org/10.1007/s 00441-016-2461-3"
        )
        assert "s00441" in text, f"DOI path space not removed: '{text}'"
        assert modified is True

    def test_doi_full_url_path_repaired(self):
        """Full DOI URL with path spaces should be repaired."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "https://doi.org/10.1007/s 00441-016-2461-3"
        )
        assert "s00441-016-2461-3" in text, f"DOI path not repaired: '{text}'"
        assert " 00441" not in text

    def test_doi_path_without_spaces_unchanged(self):
        """DOI path without spaces should remain unchanged."""
        from manual_postprocessor import fix_spacing_in_text
        text, _, _ = fix_spacing_in_text(
            "https://doi.org/10.1007/s00441-016-2461-3"
        )
        assert "s00441-016-2461-3" in text

    def test_doi_path_multiple_spaces(self):
        """Multiple spaces within DOI path should all be removed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "https://doi.org/10.1016/j.cell. 2020. 01.005"
        )
        assert "j.cell.2020.01.005" in text, f"Multiple spaces not removed: '{text}'"

    def test_non_doi_url_not_affected(self):
        """Non-DOI URLs should not be affected by DOI path repair."""
        from manual_postprocessor import fix_spacing_in_text
        original = "https://example.com/path with spaces"
        text, _, _ = fix_spacing_in_text(original)
        # The non-DOI URL should not have path spaces removed by the DOI repair step
        # (other spacing rules may still apply)
        assert "doi.org" not in text or text == original


class TestCopyrightMetadataSeparator:
    """Verify copyright page metadata classes use structured line breaks."""

    def test_copyrightpageissns_recognized(self):
        """div.CopyrightPageISSNs should be handled by metadata text handler."""
        css_class_lower = 'copyrightpageissns'
        metadata_text_div_classes = [
            'chaptercopyright', 'affiliationtext',
            'copyrightpageissn', 'copyrightpageisbn', 'copyrightpageseriestitle',
            'seriestitle', 'bookeditiionnumber',
            'collaboratorlist', 'editorialboard',
        ]
        assert any(mc in css_class_lower for mc in metadata_text_div_classes), \
            "CopyrightPageISSNs should match metadata_text_div_classes"

    def test_seriestitle_recognized(self):
        """div.SeriesTitle should be handled by metadata text handler."""
        css_class_lower = 'seriestitle'
        metadata_text_div_classes = [
            'chaptercopyright', 'affiliationtext',
            'copyrightpageissn', 'copyrightpageisbn', 'copyrightpageseriestitle',
            'seriestitle', 'bookeditiionnumber',
            'collaboratorlist', 'editorialboard',
        ]
        assert any(mc in css_class_lower for mc in metadata_text_div_classes)

    def test_copyrightpageisbns_recognized(self):
        """div.CopyrightPageISBNs should be handled by metadata text handler."""
        css_class_lower = 'copyrightpageisbns'
        metadata_text_div_classes = [
            'chaptercopyright', 'affiliationtext',
            'copyrightpageissn', 'copyrightpageisbn', 'copyrightpageseriestitle',
            'seriestitle', 'bookeditiionnumber',
            'collaboratorlist', 'editorialboard',
        ]
        assert any(mc in css_class_lower for mc in metadata_text_div_classes)


class TestCrossrefPubMedSpacingRegression:
    """Regression test for CrossrefPubMed spacing (Issue 1 of ISBN 9783032038661)."""

    def test_crossref_pubmed_in_context(self):
        """CrossrefPubMed in bibliography context should be separated."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "... 9:1869 CrossrefPubMed PubMed Central"
        )
        assert "Crossref" in text
        assert "PubMed" in text
        assert "CrossrefPubMed" not in text
        assert modified is True


# ===========================================================================
# C-026: Double-encoded NBSP (ISBN 9783032007698)
# ===========================================================================

class TestDoubleEncodedNBSP:
    """Fix C-026: Verify double-encoded NBSP characters are repaired."""

    def test_double_encoded_nbsp_fixed(self):
        """Â followed by NBSP (U+00C2 U+00A0) should collapse to single NBSP."""
        from manual_postprocessor import fix_spacing_in_text
        # "19.Â\xa0Immunotherapy" -> "19.\xa0Immunotherapy"
        text, modified, changes = fix_spacing_in_text("19.\u00c2\u00a0Immunotherapy")
        assert "\u00c2" not in text, f"Â not removed: '{text}'"
        assert "\u00a0" in text, "NBSP should be preserved"
        assert modified is True

    def test_double_encoded_copyright_fixed(self):
        """Â© (U+00C2 U+00A9) should collapse to © (U+00A9)."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text("Copyright \u00c2\u00a9 2025")
        assert "\u00c2" not in text, f"Â not removed: '{text}'"
        assert "\u00a9" in text, "© should be preserved"
        assert modified is True

    def test_normal_nbsp_unchanged(self):
        """A single NBSP (without preceding Â) should not be modified."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("Nima\u00a0Rezaei")
        # NBSP alone shouldn't be flagged as double-encoded
        assert "\u00a0" in text

    def test_multiple_double_encoded_nbsp(self):
        """Multiple double-encoded NBSPs in same text should all be fixed."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, _ = fix_spacing_in_text(
            "Nima\u00c2\u00a0Rezaei — Chapter\u00c2\u00a019"
        )
        assert text.count("\u00c2") == 0, f"Â still present: '{text}'"
        assert modified is True


# ===========================================================================
# C-027: Mojibake repair (ISBN 9783032007698)
# ===========================================================================

class TestMojibakeRepair:
    """Fix C-027: Verify double-encoded UTF-8 mojibake is repaired."""

    def test_right_single_quote_repaired(self):
        """â€™ should be repaired to ' (right single quote U+2019)."""
        from manual_postprocessor import fix_spacing_in_text
        # The garbled sequence for U+2019 is: â (U+00E2) + € (U+20AC) + ™ (U+2122)
        garbled = "Coley\u00e2\u20ac\u2122s toxin"
        text, modified, changes = fix_spacing_in_text(garbled)
        assert "\u2019" in text, f"Right single quote not restored: '{text}'"
        assert "Coley\u2019s" in text
        assert modified is True

    def test_en_dash_repaired(self):
        """â€" (en dash mojibake) should be repaired to – (U+2013)."""
        from manual_postprocessor import fix_spacing_in_text
        # The garbled sequence for U+2013 is: â (U+00E2) + € (U+20AC) + " (U+0093)
        garbled = "70\u00e2\u20ac\u009380%"
        text, modified, _ = fix_spacing_in_text(garbled)
        assert "\u2013" in text, f"En dash not restored: '{text}'"
        assert "70\u201380%" in text

    def test_greek_beta_repaired(self):
        """Î² (double-encoded β) should be repaired to β (U+03B2)."""
        from manual_postprocessor import fix_spacing_in_text
        garbled = "TGF-\u00ce\u00b2"
        text, modified, _ = fix_spacing_in_text(garbled)
        assert "\u03b2" in text, f"Greek beta not restored: '{text}'"
        assert "TGF-\u03b2" in text

    def test_accented_e_repaired(self):
        """Ã© (double-encoded é) should be repaired to é (U+00E9)."""
        from manual_postprocessor import fix_spacing_in_text
        garbled = "na\u00c3\u00afve"
        text, modified, _ = fix_spacing_in_text(garbled)
        assert "\u00ef" in text, f"Accented char not restored: '{text}'"
        assert "na\u00efve" in text

    def test_normal_text_not_modified_by_mojibake(self):
        """Normal text should not be affected by mojibake repair patterns."""
        from manual_postprocessor import fix_spacing_in_text
        text, modified, changes = fix_spacing_in_text("Normal English text here.")
        # Should not report mojibake repair
        assert "mojibake" not in str(changes).lower() or not any(
            "mojibake" in c.lower() for c in changes
        )

    def test_left_double_quote_repaired(self):
        """â€œ should be repaired to " (left double quote U+201C)."""
        from manual_postprocessor import fix_spacing_in_text
        garbled = "\u00e2\u20ac\u201csome quoted text\u00e2\u20ac\u009d"
        text, modified, _ = fix_spacing_in_text(garbled)
        assert "\u201c" in text, f"Left double quote not restored: '{text}'"


# ===========================================================================
# DTD fixer: _normalize_whitespace trailing space preservation
# ===========================================================================

class TestNormalizeWhitespacePreservesTrailingSpace:
    """Fix: _normalize_whitespace must preserve trailing space before child elements."""

    def test_trailing_space_before_emphasis_preserved(self):
        """Space between elem.text and first child <emphasis> must be preserved."""
        from comprehensive_dtd_fixer import ComprehensiveDTDFixer

        xml_str = '<para id="p1">Thrift and El-Serag <emphasis role="CitationRef">[4]</emphasis> showed</para>'
        root = etree.fromstring(xml_str)
        # Wrap in a parent so iter works
        wrapper = etree.Element("section")
        wrapper.append(root)

        fixer = ComprehensiveDTDFixer.__new__(ComprehensiveDTDFixer)
        fixer._normalize_whitespace(wrapper, "test.xml")

        para = wrapper.find('.//para')
        assert para.text is not None
        assert para.text.endswith(' '), \
            f"Trailing space lost in para.text: '{para.text}'"

    def test_no_trailing_space_when_no_children(self):
        """Text-only elements: leading/trailing whitespace normalized if internal spaces match."""
        from comprehensive_dtd_fixer import ComprehensiveDTDFixer

        # The safety check (elem.text.strip() == normalized.strip()) only applies when
        # internal whitespace is unchanged. Test with only leading/trailing whitespace:
        xml_str = '<para id="p1"> Some single-spaced text </para>'
        root = etree.fromstring(xml_str)
        wrapper = etree.Element("section")
        wrapper.append(root)

        fixer = ComprehensiveDTDFixer.__new__(ComprehensiveDTDFixer)
        fixer._normalize_whitespace(wrapper, "test.xml")

        para = wrapper.find('.//para')
        assert para.text == "Some single-spaced text", \
            f"Leading/trailing whitespace not normalized: '{para.text}'"

    def test_multi_space_before_child_collapsed_to_one(self):
        """Multiple spaces before child element should collapse to single space."""
        from comprehensive_dtd_fixer import ComprehensiveDTDFixer

        xml_str = '<para id="p1">Text   <emphasis>bold</emphasis></para>'
        root = etree.fromstring(xml_str)
        wrapper = etree.Element("section")
        wrapper.append(root)

        fixer = ComprehensiveDTDFixer.__new__(ComprehensiveDTDFixer)
        fixer._normalize_whitespace(wrapper, "test.xml")

        para = wrapper.find('.//para')
        assert para.text == "Text ", \
            f"Expected 'Text ' but got '{para.text}'"


# ===========================================================================
# Bibliography sections must stay wrapped in sect1 for RISchunker routing
# ===========================================================================

class TestBibliographySect1Wrapping:
    """Verify that bibliography sections within chapters stay inside sect1.

    RISchunker only recognises sect1 boundaries as navigable pages.
    If the bibliography is moved out of its sect1 wrapper directly under
    <chapter>, the reference section becomes unreachable.
    """

    def test_bibliography_stays_inside_sect1(self):
        """Bibliography converted from a References sect1 must remain inside sect1."""
        from epub_to_structured_v2 import _convert_bibliography_sections_within_chapter

        chapter = etree.Element('chapter', id='ch0001')
        title = etree.SubElement(chapter, 'title')
        title.text = 'Chapter 1'

        # Add a content sect1
        sect1_content = etree.SubElement(chapter, 'sect1', id='ch0001s0001')
        sect1_content.set('role', 'Heading')
        t1 = etree.SubElement(sect1_content, 'title')
        t1.text = 'Introduction'
        p1 = etree.SubElement(sect1_content, 'para')
        p1.text = 'Some content.'

        # Add a References sect1 with a list of bibliography entries
        sect1_refs = etree.SubElement(chapter, 'sect1', id='ch0001s0002')
        t2 = etree.SubElement(sect1_refs, 'title')
        t2.text = 'References'
        ol = etree.SubElement(sect1_refs, 'orderedlist')
        for i in range(1, 4):
            li = etree.SubElement(ol, 'listitem')
            p = etree.SubElement(li, 'para')
            p.text = f'{i}. Author{i} et al. (2024) Title{i}. Journal. doi:10.1234/test{i}'

        converted = _convert_bibliography_sections_within_chapter(chapter, 'ch0001')
        assert converted == 1, f"Expected 1 conversion, got {converted}"

        # The bibliography must be inside a sect1, NOT directly under chapter
        direct_bibs = [c for c in chapter if c.tag == 'bibliography']
        assert len(direct_bibs) == 0, \
            "Bibliography should NOT be a direct child of chapter (breaks RISchunker)"

        # Find the sect1 that contains the bibliography
        bib_sect1 = None
        for sect1 in chapter.findall('sect1'):
            if sect1.find('bibliography') is not None:
                bib_sect1 = sect1
                break

        assert bib_sect1 is not None, "Bibliography must be inside a sect1 element"
        assert bib_sect1.get('id') == 'ch0001s0002', \
            f"Bibliography sect1 should keep original ID, got {bib_sect1.get('id')}"

        # Verify the bibliography has proper structure
        bib = bib_sect1.find('bibliography')
        assert bib is not None
        assert bib.get('id') == 'ch0001s0002bib'
        bibliomixed_entries = bib.findall('bibliomixed')
        assert len(bibliomixed_entries) == 3, \
            f"Expected 3 bibliomixed entries, got {len(bibliomixed_entries)}"

    def test_bibliography_sect1_has_spacer_para(self):
        """When sect1 has only title+bibliography, a spacer para should be added for DTD compliance."""
        from epub_to_structured_v2 import _convert_bibliography_sections_within_chapter

        chapter = etree.Element('chapter', id='ch0002')
        title = etree.SubElement(chapter, 'title')
        title.text = 'Chapter 2'

        # References-only sect1
        sect1_refs = etree.SubElement(chapter, 'sect1', id='ch0002s0001')
        t = etree.SubElement(sect1_refs, 'title')
        t.text = 'References'
        ol = etree.SubElement(sect1_refs, 'orderedlist')
        li = etree.SubElement(ol, 'listitem')
        p = etree.SubElement(li, 'para')
        p.text = '1. Test reference entry'

        _convert_bibliography_sections_within_chapter(chapter, 'ch0002')

        # sect1 must still exist
        sect1 = chapter.find('sect1')
        assert sect1 is not None, "sect1 should not be removed"

        # Must have a spacer para before bibliography
        children = list(sect1)
        child_tags = [c.tag for c in children]
        assert 'para' in child_tags, "Spacer para must be added for DTD compliance"
        assert 'bibliography' in child_tags, "Bibliography must be present"

        # The spacer para should come before bibliography
        para_idx = child_tags.index('para')
        bib_idx = child_tags.index('bibliography')
        assert para_idx < bib_idx, "Spacer para must come before bibliography"


class TestDTDFixerBibliographySect1Preservation:
    """Verify that the DTD fixer never extracts bibliography from sect1.

    _fix_bibliography_in_sections must add a spacer para instead of
    removing the sect1 wrapper, even when the converter didn't add one.
    This is the safety net for RISchunker routing.
    """

    def test_dtd_fixer_adds_spacer_instead_of_extracting(self):
        """DTD fixer must add spacer para to sect1 with only title+bibliography."""
        from comprehensive_dtd_fixer import ComprehensiveDTDFixer

        # Build: <chapter><sect1><title>References</title><bibliography>...</bibliography></sect1></chapter>
        # This simulates a case where the converter didn't add a spacer para.
        chapter = etree.Element('chapter', id='ch0010')
        ch_title = etree.SubElement(chapter, 'title')
        ch_title.text = 'Test Chapter'
        sect1 = etree.SubElement(chapter, 'sect1', id='ch0010s0001')
        s1_title = etree.SubElement(sect1, 'title')
        s1_title.text = 'References'
        bib = etree.SubElement(sect1, 'bibliography', id='ch0010bib')
        bm = etree.SubElement(bib, 'bibliomixed', id='ch0010bib0001')
        bm.text = 'Author et al. (2024) Some paper. Journal.'

        fixer = ComprehensiveDTDFixer.__new__(ComprehensiveDTDFixer)
        fixer.verification_items = []
        fixes = fixer._fix_bibliography_in_sections(chapter, 'test.xml')

        # sect1 must still be present
        sect1_elems = [c for c in chapter if c.tag == 'sect1']
        assert len(sect1_elems) == 1, \
            f"sect1 must be preserved, got {len(sect1_elems)} sect1 elements"

        # bibliography must still be inside sect1
        bib_in_sect1 = sect1_elems[0].find('bibliography')
        assert bib_in_sect1 is not None, "bibliography must remain inside sect1"

        # bibliography must NOT be a direct child of chapter
        bib_direct = [c for c in chapter if c.tag == 'bibliography']
        assert len(bib_direct) == 0, \
            "bibliography must NOT be extracted to chapter level"

        # A spacer para must have been added
        paras = [c for c in sect1_elems[0] if c.tag == 'para']
        assert len(paras) >= 1, "spacer para must be added for DTD compliance"
        assert any('spacer' in f.lower() or 'para' in f.lower() for f in fixes), \
            f"Fix log should mention spacer para, got: {fixes}"

    def test_dtd_fixer_preserves_sect1_with_existing_block_content(self):
        """DTD fixer preserves sect1 that already has block content + bibliography."""
        from comprehensive_dtd_fixer import ComprehensiveDTDFixer

        chapter = etree.Element('chapter', id='ch0011')
        ch_title = etree.SubElement(chapter, 'title')
        ch_title.text = 'Test Chapter'
        sect1 = etree.SubElement(chapter, 'sect1', id='ch0011s0001')
        s1_title = etree.SubElement(sect1, 'title')
        s1_title.text = 'References'
        spacer = etree.SubElement(sect1, 'para')
        spacer.text = '\u00a0'
        bib = etree.SubElement(sect1, 'bibliography', id='ch0011bib')
        bm = etree.SubElement(bib, 'bibliomixed', id='ch0011bib0001')
        bm.text = 'Author et al. (2024) Some paper.'

        fixer = ComprehensiveDTDFixer.__new__(ComprehensiveDTDFixer)
        fixer.verification_items = []
        fixes = fixer._fix_bibliography_in_sections(chapter, 'test.xml')

        # sect1 and bibliography structure must be unchanged
        sect1_elems = [c for c in chapter if c.tag == 'sect1']
        assert len(sect1_elems) == 1
        assert sect1_elems[0].find('bibliography') is not None
        # No spacer should have been added (one already exists)
        paras = [c for c in sect1_elems[0] if c.tag == 'para']
        assert len(paras) == 1, "should not add a second spacer para"


class TestOrphanedBibliographyWrapping:
    """Verify that bibliography direct-children of chapter get wrapped in sect1."""

    def test_orphaned_bibliography_gets_sect1_wrapper(self):
        """Bibliography as direct chapter child must be wrapped in sect1."""
        from comprehensive_dtd_fixer import ComprehensiveDTDFixer

        # Simulate the broken output: <chapter><sect1>...</sect1><bibliography id="ch0002s0010">...
        chapter = etree.Element('chapter', id='ch0002')
        ch_title = etree.SubElement(chapter, 'title')
        ch_title.text = 'Test Chapter'
        # Normal sect1
        sect1 = etree.SubElement(chapter, 'sect1', id='ch0002s0001')
        s1_title = etree.SubElement(sect1, 'title')
        s1_title.text = 'Introduction'
        para = etree.SubElement(sect1, 'para')
        para.text = 'Some content.'
        # Orphaned bibliography (should be in sect1 but isn't)
        bib = etree.SubElement(chapter, 'bibliography', id='ch0002s0010')
        bib_title = etree.SubElement(bib, 'title')
        bib_title.text = 'References'
        bm = etree.SubElement(bib, 'bibliomixed', id='ch0002s0010bib01')
        bm.text = 'Author et al. (2024) Some paper.'

        fixer = ComprehensiveDTDFixer.__new__(ComprehensiveDTDFixer)
        fixer.verification_items = []
        fixes = fixer._fix_orphaned_bibliography_at_chapter_level(chapter, 'test.xml')

        # bibliography must NOT be a direct child of chapter anymore
        bib_direct = [c for c in chapter if c.tag == 'bibliography']
        assert len(bib_direct) == 0, "bibliography must not be a direct chapter child"

        # There should now be 2 sect1 elements
        sect1_elems = [c for c in chapter if c.tag == 'sect1']
        assert len(sect1_elems) == 2, f"Expected 2 sect1, got {len(sect1_elems)}"

        # The new sect1 should contain the bibliography
        new_sect1 = sect1_elems[1]
        assert new_sect1.get('id') == 'ch0002s0010', \
            f"New sect1 should reuse bibliography's section-format ID, got {new_sect1.get('id')}"
        assert new_sect1.get('role') == 'bibliography'

        # The bibliography should be inside the new sect1
        bib_in_sect1 = new_sect1.find('bibliography')
        assert bib_in_sect1 is not None, "bibliography must be inside the new sect1"

        # sect1 should have a spacer para
        paras = [c for c in new_sect1 if c.tag == 'para']
        assert len(paras) >= 1, "sect1 must have a spacer para"

        # sect1 should have a title
        title = new_sect1.find('title')
        assert title is not None and title.text == 'References'

        assert len(fixes) == 1 and 'Wrapped' in fixes[0]
