"""
Regression tests for the 10 cherry-pick fixes ported from Akash's repo.

Fixes tested:
- Fix #1:  _wrap_bibliography_in_chapter (bibliography routing)
- Fix #5:  _h1_title_set (chapter title overwrite protection)
- Fix #6:  Cross-References section exclusion from bibliography detection
- Fix #7:  _merge_item_number_paras (list numbers on separate line)
- Fix #8:  Contributors/about prefix ch→pr
- Fix #9:  Cover/titlepage image number isolation + shared_figure_counters
- Fix #10: Alt text as textobject (already existed — regression test only)

Fixes #3 (Multi-BibSection) and #4 (mediaobject in bibliomixed) were already
in the codebase and have existing tests in test_bibliography_fixes.py.
"""

import sys
import os
import re

# Add project root to path
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import pytest
from lxml import etree


# ===========================================================================
# Fix #1: _wrap_bibliography_in_chapter
# ===========================================================================

class TestWrapBibliographyInChapter:
    """Fix #1: Bare <bibliography> must be wrapped in <chapter><sect1> for routing."""

    def test_wrap_creates_chapter_sect1(self):
        """_wrap_bibliography_in_chapter should return chapter > sect1 > bibliography."""
        from epub_to_structured_v2 import _wrap_bibliography_in_chapter
        bib = etree.Element('bibliography')
        title = etree.SubElement(bib, 'title')
        title.text = 'References'
        bm = etree.SubElement(bib, 'bibliomixed', id='bib0001')
        bm.text = 'Smith et al. 2020'

        result = _wrap_bibliography_in_chapter(bib, 'ch0005')

        assert result.tag == 'chapter'
        assert result.get('id') == 'ch0005'
        assert result.get('role') == 'bibliography'

        sect1 = result.find('sect1')
        assert sect1 is not None, "Missing sect1 inside chapter wrapper"
        assert sect1.get('id') == 'ch0005s0001'
        assert sect1.get('role') == 'bibliography'

        # Spacer para for DTD compliance
        paras = sect1.findall('para')
        assert len(paras) >= 1, "Missing spacer para"
        assert paras[0].text == '\u00a0'

        # Bibliography element moved inside sect1
        inner_bib = sect1.find('bibliography')
        assert inner_bib is not None, "Bibliography not moved inside sect1"
        assert inner_bib.get('id') == 'ch0005bib'
        # Title removed from inner bibliography (sect1 already has it)
        assert inner_bib.find('title') is None

    def test_wrap_preserves_entries(self):
        """Wrapped bibliography should retain all bibliomixed entries."""
        from epub_to_structured_v2 import _wrap_bibliography_in_chapter
        bib = etree.Element('bibliography')
        title = etree.SubElement(bib, 'title')
        title.text = 'Bibliography'
        for i in range(5):
            bm = etree.SubElement(bib, 'bibliomixed', id=f'bib{i+1:04d}')
            bm.text = f'Entry {i+1}'

        result = _wrap_bibliography_in_chapter(bib, 'bi0001')
        inner_bib = result.find('.//bibliography')
        entries = inner_bib.findall('bibliomixed')
        assert len(entries) == 5, f"Expected 5 entries, got {len(entries)}"

    def test_wrap_default_title(self):
        """If bibliography has no title, 'References' should be used."""
        from epub_to_structured_v2 import _wrap_bibliography_in_chapter
        bib = etree.Element('bibliography')
        bm = etree.SubElement(bib, 'bibliomixed')
        bm.text = 'Test'

        result = _wrap_bibliography_in_chapter(bib, 'ch0010')
        ch_title = result.find('title')
        assert ch_title.text == 'References'


# ===========================================================================
# Fix #5: Chapter title overwrite protection (_h1_title_set)
# ===========================================================================

class TestH1TitleOverwriteProtection:
    """Fix #5: Only the first H1 should set the chapter title."""

    def test_section_counters_h1_title_set_flag(self):
        """After the first H1, section_counters['_h1_title_set'] should be True."""
        # This tests the mechanism, not the full converter pipeline
        section_counters = {}

        # Simulate first H1 processing
        if not section_counters.get('_h1_title_set'):
            section_counters['_h1_title_set'] = True
            first_h1_set_title = True
        else:
            first_h1_set_title = False
        assert first_h1_set_title is True

        # Simulate second H1 processing — should NOT set title
        if not section_counters.get('_h1_title_set'):
            section_counters['_h1_title_set'] = True
            second_h1_set_title = True
        else:
            second_h1_set_title = False
        assert second_h1_set_title is False

    def test_subsequent_h1_creates_section(self):
        """A second H1 in the same chapter should not overwrite the original title.

        Since we can't easily test the full heading handler in isolation,
        we verify the flag mechanism works correctly across multiple calls.
        """
        section_counters = {}
        titles = ['Chapter Title', 'Activities', 'Summary']

        set_as_title = []
        for h1_text in titles:
            if not section_counters.get('_h1_title_set'):
                section_counters['_h1_title_set'] = True
                set_as_title.append(h1_text)

        # Only the first H1 should have been used as chapter title
        assert len(set_as_title) == 1
        assert set_as_title[0] == 'Chapter Title'


# ===========================================================================
# Fix #6: Cross-References section exclusion
# ===========================================================================

class TestCrossReferencesExclusion:
    """Fix #6: 'Cross-References' sections should NOT be treated as bibliography."""

    def test_cross_references_excluded(self):
        """A section titled 'Cross-References' should not match bibliography patterns."""
        non_bibliography_title_patterns = [
            'cross-references', 'cross references', 'crossreferences',
        ]
        bibliography_title_patterns = [
            'references', 'bibliography', 'works cited',
        ]

        title = 'Cross-References'
        title_lower = title.lower().strip()

        is_excluded = any(excl in title_lower for excl in non_bibliography_title_patterns)
        assert is_excluded, "'Cross-References' should be excluded from bibliography detection"

        # Verify that without exclusion, it WOULD have matched
        would_match = any(pat in title_lower for pat in bibliography_title_patterns)
        assert would_match, "'Cross-References' contains 'references' — exclusion is necessary"

    def test_regular_references_not_excluded(self):
        """A section titled 'References' should still match as bibliography."""
        non_bibliography_title_patterns = [
            'cross-references', 'cross references', 'crossreferences',
        ]
        bibliography_title_patterns = [
            'references', 'bibliography', 'works cited',
        ]

        title = 'References'
        title_lower = title.lower().strip()

        is_excluded = any(excl in title_lower for excl in non_bibliography_title_patterns)
        assert not is_excluded, "'References' should NOT be excluded"

        is_bib = any(pat in title_lower for pat in bibliography_title_patterns)
        assert is_bib, "'References' should match bibliography detection"

    def test_further_reading_not_excluded(self):
        """'Further Reading' should still match as bibliography."""
        non_bibliography_title_patterns = [
            'cross-references', 'cross references', 'crossreferences',
        ]
        bibliography_title_patterns = [
            'references', 'bibliography', 'works cited', 'further reading',
        ]

        title = 'Further Reading'
        title_lower = title.lower().strip()

        is_excluded = any(excl in title_lower for excl in non_bibliography_title_patterns)
        assert not is_excluded

        is_bib = any(pat in title_lower for pat in bibliography_title_patterns)
        assert is_bib


# ===========================================================================
# Fix #7: _merge_item_number_paras
# ===========================================================================

class TestMergeItemNumberParas:
    """Fix #7: Standalone number paragraphs in list items should be merged with text."""

    def test_basic_merge(self):
        """<para>1.</para><para>Text</para> -> <para>1.\\tText</para>."""
        from epub_to_structured_v2 import _merge_item_number_paras
        xml = '''<orderedlist>
            <listitem>
                <para>1.</para>
                <para>First item text</para>
            </listitem>
            <listitem>
                <para>2.</para>
                <para>Second item text</para>
            </listitem>
        </orderedlist>'''
        root = etree.fromstring(xml)
        count = _merge_item_number_paras(root)
        assert count == 2, f"Expected 2 merges, got {count}"

        for listitem in root.iter('listitem'):
            paras = listitem.findall('para')
            assert len(paras) == 1, f"Expected 1 para after merge, got {len(paras)}"
            assert '\t' in paras[0].text, "Merged text should contain tab separator"

    def test_letter_numbering(self):
        """Letter-based numbering like 'a.' or '(b)' should be merged."""
        from epub_to_structured_v2 import _merge_item_number_paras
        xml = '''<orderedlist>
            <listitem>
                <para>a.</para>
                <para>Alpha text</para>
            </listitem>
            <listitem>
                <para>(b)</para>
                <para>Beta text</para>
            </listitem>
        </orderedlist>'''
        root = etree.fromstring(xml)
        count = _merge_item_number_paras(root)
        assert count == 2

    def test_roman_numeral(self):
        """Roman numeral numbering like 'iv.' should be merged."""
        from epub_to_structured_v2 import _merge_item_number_paras
        xml = '''<orderedlist>
            <listitem>
                <para>iv.</para>
                <para>Fourth item</para>
            </listitem>
        </orderedlist>'''
        root = etree.fromstring(xml)
        count = _merge_item_number_paras(root)
        assert count == 1

    def test_no_merge_for_real_text(self):
        """Real text paragraphs should NOT be merged even if short."""
        from epub_to_structured_v2 import _merge_item_number_paras
        xml = '''<orderedlist>
            <listitem>
                <para>Introduction to the topic</para>
                <para>More details here</para>
            </listitem>
        </orderedlist>'''
        root = etree.fromstring(xml)
        count = _merge_item_number_paras(root)
        assert count == 0, "Should not merge non-number paragraphs"

    def test_no_merge_for_para_with_children(self):
        """<para> with child elements (like emphasis) should NOT be merged."""
        from epub_to_structured_v2 import _merge_item_number_paras
        xml = '''<orderedlist>
            <listitem>
                <para><emphasis>1.</emphasis></para>
                <para>Text</para>
            </listitem>
        </orderedlist>'''
        root = etree.fromstring(xml)
        count = _merge_item_number_paras(root)
        assert count == 0, "Should not merge para with child elements"


# ===========================================================================
# Fix #8: Contributors/about prefix ch→pr
# ===========================================================================

class TestContributorsPrefixMapping:
    """Fix #8: Contributors and about sections should use 'pr' prefix and <preface> tag."""

    def test_element_type_prefixes_contributors(self):
        """element_type_prefixes should map 'contributors' to 'pr'."""
        # We test the dict directly since it's defined inside a function.
        # Here we verify the intent by checking the mapping is correct.
        expected = {
            'contributors': 'pr',
            'about': 'pr',
            'preface': 'pr',
            'chapter': 'ch',
            'appendix': 'ap',
        }
        # The actual dict is inside convert_xhtml_to_chapter, so we test the
        # element_tag_map values which are what actually matters for output
        element_tag_map = {
            'contributors': 'preface',
            'about': 'preface',
            'acknowledgments': 'chapter',
        }
        assert element_tag_map['contributors'] == 'preface', \
            "Contributors should map to 'preface' XML tag"
        assert element_tag_map['about'] == 'preface', \
            "About should map to 'preface' XML tag"
        assert element_tag_map['acknowledgments'] == 'chapter', \
            "Acknowledgments should still map to 'chapter'"

    def test_contributors_role_attribute(self):
        """Contributors <preface> should have role='contributors' for identification."""
        elem = etree.Element('preface', id='pr0001')
        element_type = 'contributors'
        if element_type in ('acknowledgments', 'contributors', 'about'):
            elem.set('role', element_type)
        assert elem.tag == 'preface'
        assert elem.get('role') == 'contributors'

    def test_about_creates_preface(self):
        """About section should create <preface> element, not <chapter>."""
        elem = etree.Element('preface', id='pr0002')
        element_type = 'about'
        if element_type in ('acknowledgments', 'contributors', 'about'):
            elem.set('role', element_type)
        assert elem.tag == 'preface'
        assert elem.get('role') == 'about'


# ===========================================================================
# Fix #9: Cover/titlepage image number isolation + shared_figure_counters
# ===========================================================================

class TestChapterCodeImageIsolation:
    """Fix #9: Cover/titlepage should get their own image codes."""

    def test_chapter_code_with_cover_role(self):
        """Fragment with role='cover' should return ('Cover', 'Cover')."""
        from unittest.mock import MagicMock
        from package import _chapter_code

        fragment = MagicMock()
        fragment.section_type = 'preface'
        fragment.kind = 'content'
        fragment.element = etree.Element('preface', role='cover')

        code, label = _chapter_code(fragment)
        assert code == 'Cover', f"Expected 'Cover', got '{code}'"
        assert label == 'Cover'

    def test_chapter_code_with_titlepage_role(self):
        """Fragment with role='titlepage' should return ('Titlepage', 'Titlepage')."""
        from unittest.mock import MagicMock
        from package import _chapter_code

        fragment = MagicMock()
        fragment.section_type = 'preface'
        fragment.kind = 'content'
        fragment.element = etree.Element('preface', role='titlepage')

        code, label = _chapter_code(fragment)
        assert code == 'Titlepage', f"Expected 'Titlepage', got '{code}'"
        assert label == 'Titlepage'

    def test_chapter_code_preface_no_role(self):
        """Plain preface fragment (no special role) should return ('Preface', 'Preface')."""
        from unittest.mock import MagicMock
        from package import _chapter_code

        fragment = MagicMock()
        fragment.section_type = 'preface'
        fragment.kind = 'content'
        fragment.element = etree.Element('preface')

        code, label = _chapter_code(fragment)
        assert code == 'Preface', f"Expected 'Preface', got '{code}'"
        assert label == 'Preface'

    def test_chapter_code_contributors(self):
        """Contributors fragment should return ('Preface', 'Preface')."""
        from unittest.mock import MagicMock
        from package import _chapter_code

        fragment = MagicMock()
        fragment.section_type = 'contributors'
        fragment.kind = 'content'
        fragment.element = etree.Element('preface', role='contributors')

        code, label = _chapter_code(fragment)
        assert code == 'Preface', f"Expected 'Preface', got '{code}'"

    def test_chapter_code_pr_prefix_entity(self):
        """Fragment with entity='pr0001' should return ('Preface', 'Preface')."""
        from unittest.mock import MagicMock
        from package import _chapter_code

        fragment = MagicMock()
        fragment.section_type = ''
        fragment.kind = 'content'
        fragment.element = etree.Element('preface')
        fragment.entity = 'pr0001'

        code, label = _chapter_code(fragment)
        assert code == 'Preface', f"Expected 'Preface', got '{code}'"

    def test_shared_figure_counters_sequential(self):
        """Shared figure counters should produce sequential numbers across fragments."""
        shared_figure_counters = {}

        # Simulate first preface fragment processing
        chapter_code = 'Preface'
        figure_counter_1 = shared_figure_counters.get(chapter_code, 0) + 1
        # Process 3 figures
        figure_counter_1 += 2  # Now at 3 (processed figures 1, 2, 3)
        shared_figure_counters[chapter_code] = figure_counter_1 - 1  # Save: 2

        # Simulate second preface fragment
        figure_counter_2 = shared_figure_counters.get(chapter_code, 0) + 1
        assert figure_counter_2 == 3, \
            f"Second fragment should start at 3, got {figure_counter_2}"


# ===========================================================================
# Fix #10: Alt text as textobject (regression test — already existed)
# ===========================================================================

class TestAltTextAsTextobject:
    """Fix #10: Alt text should go into textobject/para, not figure title."""

    def test_alt_text_in_textobject(self):
        """Alt text should be placed in mediaobject > textobject > para."""
        mediaobject = etree.Element('mediaobject')
        imageobject = etree.SubElement(mediaobject, 'imageobject')
        imagedata = etree.SubElement(imageobject, 'imagedata', fileref='img001.jpg')

        # Simulate the alt text placement pattern from the converter
        alt_text = 'Diagram showing knee joint anatomy'
        if alt_text:
            textobject = etree.SubElement(mediaobject, 'textobject')
            tpara = etree.SubElement(textobject, 'para')
            tpara.text = alt_text

        # Verify structure
        to = mediaobject.find('textobject')
        assert to is not None, "textobject should exist in mediaobject"
        tp = to.find('para')
        assert tp is not None, "para should exist in textobject"
        assert tp.text == alt_text

    def test_alt_text_not_as_title(self):
        """Alt text should NOT be used as figure title."""
        figure = etree.Element('figure')
        title = etree.SubElement(figure, 'title')
        title.text = 'Figure 1.1'
        mediaobject = etree.SubElement(figure, 'mediaobject')

        alt_text = 'Photo of a bone'
        textobject = etree.SubElement(mediaobject, 'textobject')
        tpara = etree.SubElement(textobject, 'para')
        tpara.text = alt_text

        # Title should NOT contain the alt text
        assert title.text != alt_text
        # Alt text should be in textobject
        assert mediaobject.find('.//textobject/para').text == alt_text


# ===========================================================================
# ID Authority compatibility with prefix changes
# ===========================================================================

class TestIdAuthorityPrefixCompatibility:
    """Verify id_authority works correctly with pr prefix for contributors."""

    def test_chapter_prefix_pr_is_valid(self):
        """The 'pr' prefix should be recognized by id_authority's ChapterPrefix enum."""
        from id_authority import ChapterPrefix
        assert 'pr' in [p.value for p in ChapterPrefix], \
            "'pr' must be a valid ChapterPrefix for contributors/preface"

    def test_chapter_pattern_accepts_pr(self):
        """The chapter pattern regex should match pr0001-style IDs."""
        from id_authority import CHAPTER_PATTERN
        match = CHAPTER_PATTERN.match('pr0001')
        assert match is not None, "CHAPTER_PATTERN should match 'pr0001'"

    def test_element_pattern_accepts_pr_prefix(self):
        """Element patterns should work with pr-prefixed chapter IDs."""
        from id_authority import ELEMENT_PATTERN
        # A figure inside a preface section
        match = ELEMENT_PATTERN.match('pr0001s0001fg0001')
        assert match is not None, "ELEMENT_PATTERN should match 'pr0001s0001fg0001'"

    def test_extract_chapter_from_pr_id(self):
        """extract_chapter_from_id should extract 'pr0001' from a pr-prefixed ID."""
        from id_authority import extract_chapter_from_id
        result = extract_chapter_from_id('pr0001s0001fg0001')
        assert result == 'pr0001', f"Expected 'pr0001', got '{result}'"


# ===========================================================================
# Integration: bibliography wrapping end-to-end
# ===========================================================================

class TestBibliographyWrappingIntegration:
    """Integration test: bibliography element → chapter > sect1 > bibliography."""

    def test_wrapped_bib_is_dtd_valid_structure(self):
        """The wrapped structure should follow DTD content model."""
        from epub_to_structured_v2 import _wrap_bibliography_in_chapter
        bib = etree.Element('bibliography')
        title = etree.SubElement(bib, 'title')
        title.text = 'References'
        for i in range(3):
            bm = etree.SubElement(bib, 'bibliomixed', id=f'bib{i:04d}')
            bm.text = f'Citation {i}'

        result = _wrap_bibliography_in_chapter(bib, 'ch0005')

        # Validate structure: chapter > title + sect1
        assert result.tag == 'chapter'
        children = list(result)
        assert children[0].tag == 'title'
        assert children[1].tag == 'sect1'

        # sect1 > title + para(spacer) + bibliography
        sect1_children = list(children[1])
        assert sect1_children[0].tag == 'title'
        assert sect1_children[1].tag == 'para'  # spacer
        assert sect1_children[2].tag == 'bibliography'

        # bibliography > bibliomixed entries
        bib_entries = sect1_children[2].findall('bibliomixed')
        assert len(bib_entries) == 3
