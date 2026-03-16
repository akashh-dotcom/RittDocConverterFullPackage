"""
Test DTD content model fixes in comprehensive_dtd_fixer.py.

Tests for:
- bridgehead in indexdiv (should convert to para)
- para in table (should move outside)
"""

import pytest
from lxml import etree
from pathlib import Path
import sys
import os

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from comprehensive_dtd_fixer import ComprehensiveDTDFixer


@pytest.fixture
def fixer():
    """Create a fixer instance."""
    dtd_path = Path(__file__).parent.parent / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
    return ComprehensiveDTDFixer(dtd_path)


class TestBridgeheadInIndexdiv:
    """Test fixing bridgehead elements inside indexdiv."""

    def test_bridgehead_converted_to_para(self, fixer):
        """Bridgehead in indexdiv should be converted to para with emphasis."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <index>
            <indexdiv>
                <title>A</title>
                <bridgehead>Section Header</bridgehead>
                <para>Some content</para>
                <itemizedlist>
                    <listitem><para>Item 1</para></listitem>
                </itemizedlist>
            </indexdiv>
        </index>
        """
        root = etree.fromstring(xml.encode())

        # Verify bridgehead exists before fix
        bridgeheads_before = list(root.iter('bridgehead'))
        assert len(bridgeheads_before) == 1

        # Apply fix
        fixes = fixer._fix_bridgehead_in_indexdiv(root, "test.xml")

        # Verify bridgehead is gone
        bridgeheads_after = list(root.iter('bridgehead'))
        assert len(bridgeheads_after) == 0

        # Verify para with emphasis was created
        indexdiv = root.find('indexdiv')
        paras = indexdiv.findall('para')
        assert len(paras) >= 2  # At least the original + the converted one

        # Find the para with emphasis (the converted bridgehead)
        converted_para = None
        for para in paras:
            emphasis = para.find('emphasis')
            if emphasis is not None and emphasis.get('role') == 'bold':
                converted_para = para
                break

        assert converted_para is not None
        assert converted_para.find('emphasis').text == 'Section Header'

        # Verify fix was reported
        assert len(fixes) == 1
        assert 'bridgehead' in fixes[0].lower()

    def test_multiple_bridgeheads_converted(self, fixer):
        """Multiple bridgeheads in indexdiv should all be converted."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <index>
            <indexdiv>
                <title>A</title>
                <bridgehead>Header 1</bridgehead>
                <para>Content 1</para>
                <bridgehead>Header 2</bridgehead>
                <para>Content 2</para>
            </indexdiv>
        </index>
        """
        root = etree.fromstring(xml.encode())

        # Apply fix
        fixes = fixer._fix_bridgehead_in_indexdiv(root, "test.xml")

        # Verify all bridgeheads are gone
        bridgeheads = list(root.iter('bridgehead'))
        assert len(bridgeheads) == 0

        # Verify fixes were reported
        assert len(fixes) == 2


class TestBridgeheadInIndex:
    """Test fixing bridgehead elements directly inside index (not in indexdiv)."""

    def test_bridgehead_in_index_converted(self, fixer):
        """Bridgehead directly in index should be converted to para with emphasis."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <index>
            <title>Index</title>
            <bridgehead>Section A</bridgehead>
            <para>Some content</para>
        </index>
        """
        root = etree.fromstring(xml.encode())

        # Verify bridgehead exists before fix
        bridgeheads_before = list(root.iter('bridgehead'))
        assert len(bridgeheads_before) == 1

        # Apply fix
        fixes = fixer._fix_bridgehead_in_index(root, "test.xml")

        # Verify bridgehead is gone
        bridgeheads_after = list(root.iter('bridgehead'))
        assert len(bridgeheads_after) == 0

        # Verify para with emphasis was created
        paras = root.findall('para')
        assert len(paras) >= 2  # Original + converted

        # Find the para with emphasis (the converted bridgehead)
        converted_para = None
        for para in paras:
            emphasis = para.find('emphasis')
            if emphasis is not None and emphasis.get('role') == 'bold':
                converted_para = para
                break

        assert converted_para is not None
        assert converted_para.find('emphasis').text == 'Section A'

        # Verify fix was reported
        assert len(fixes) == 1

    def test_bridgehead_in_indexdiv_not_touched(self, fixer):
        """Bridgehead inside indexdiv should NOT be converted by _fix_bridgehead_in_index."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <index>
            <title>Index</title>
            <indexdiv>
                <title>A</title>
                <bridgehead>This is in indexdiv</bridgehead>
                <para>Content</para>
            </indexdiv>
        </index>
        """
        root = etree.fromstring(xml.encode())

        # Apply _fix_bridgehead_in_index (not _fix_bridgehead_in_indexdiv)
        fixes = fixer._fix_bridgehead_in_index(root, "test.xml")

        # Bridgehead in indexdiv should NOT be touched by this function
        # (it's handled by _fix_bridgehead_in_indexdiv)
        bridgeheads = list(root.iter('bridgehead'))
        assert len(bridgeheads) == 1
        assert len(fixes) == 0


class TestParaInTable:
    """Test fixing para elements inside table."""

    def test_para_moved_outside_table(self, fixer):
        """Para in table should be moved after the table."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <chapter>
            <table id="tbl1">
                <title>Test Table</title>
                <tgroup cols="2">
                    <tbody>
                        <row><entry>A</entry><entry>B</entry></row>
                    </tbody>
                </tgroup>
                <para>This para should be moved out</para>
            </table>
        </chapter>
        """
        root = etree.fromstring(xml.encode())

        # Verify para is inside table before fix
        table = root.find('table')
        paras_in_table = [c for c in table if c.tag == 'para']
        assert len(paras_in_table) == 1

        # Apply fix
        fixes = fixer._fix_para_in_table(root, "test.xml")

        # Verify para is no longer in table
        paras_in_table_after = [c for c in table if c.tag == 'para']
        assert len(paras_in_table_after) == 0

        # Verify para is now after the table
        children = list(root)
        table_idx = children.index(table)
        assert table_idx + 1 < len(children)
        assert children[table_idx + 1].tag == 'para'
        assert children[table_idx + 1].text == 'This para should be moved out'

        # Verify fix was reported
        assert len(fixes) == 1

    def test_multiple_paras_moved(self, fixer):
        """Multiple paras in table should all be moved out."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <chapter>
            <table id="tbl1">
                <title>Test Table</title>
                <tgroup cols="2">
                    <tbody>
                        <row><entry>A</entry><entry>B</entry></row>
                    </tbody>
                </tgroup>
                <para>Para 1</para>
                <para>Para 2</para>
                <para>Para 3</para>
            </table>
        </chapter>
        """
        root = etree.fromstring(xml.encode())

        # Apply fix
        fixes = fixer._fix_para_in_table(root, "test.xml")

        # Verify all paras are out of table
        table = root.find('table')
        paras_in_table = [c for c in table if c.tag == 'para']
        assert len(paras_in_table) == 0

        # Verify all paras are now after the table
        children = list(root)
        table_idx = children.index(table)
        assert children[table_idx + 1].tag == 'para'
        assert children[table_idx + 1].text == 'Para 1'
        assert children[table_idx + 2].tag == 'para'
        assert children[table_idx + 2].text == 'Para 2'
        assert children[table_idx + 3].tag == 'para'
        assert children[table_idx + 3].text == 'Para 3'

        # Verify fixes were reported
        assert len(fixes) == 3

    def test_para_between_tgroups_moved(self, fixer):
        """Para elements between tgroups should be moved out."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <chapter>
            <table id="tbl1">
                <title>Test Table</title>
                <tgroup cols="2">
                    <tbody>
                        <row><entry>A</entry><entry>B</entry></row>
                    </tbody>
                </tgroup>
                <para>Para between tgroups 1</para>
                <para>Para between tgroups 2</para>
                <tgroup cols="2">
                    <tbody>
                        <row><entry>C</entry><entry>D</entry></row>
                    </tbody>
                </tgroup>
            </table>
        </chapter>
        """
        root = etree.fromstring(xml.encode())

        # Verify structure before fix
        table = root.find('table')
        tgroups_before = [c for c in table if c.tag == 'tgroup']
        paras_before = [c for c in table if c.tag == 'para']
        assert len(tgroups_before) == 2
        assert len(paras_before) == 2

        # Apply fix
        fixes = fixer._fix_para_in_table(root, "test.xml")

        # Verify paras moved out
        paras_in_table = [c for c in table if c.tag == 'para']
        assert len(paras_in_table) == 0

        # Verify tgroups remain in table
        tgroups_after = [c for c in table if c.tag == 'tgroup']
        assert len(tgroups_after) == 2

        # Verify paras are now after the table
        children = list(root)
        table_idx = children.index(table)
        assert children[table_idx + 1].tag == 'para'
        assert children[table_idx + 2].tag == 'para'

        # Verify fixes were reported
        assert len(fixes) == 2


class TestIndexdivMissingIndexentry:
    """Test fixing indexdiv without required indexentry."""

    def test_indexentry_added(self, fixer):
        """Indexdiv without indexentry should get one added."""
        xml = """<?xml version="1.0" encoding="UTF-8"?>
        <index>
            <indexdiv>
                <title>A</title>
                <para>Some content</para>
            </indexdiv>
        </index>
        """
        root = etree.fromstring(xml.encode())

        # Verify no indexentry before fix
        indexdiv = root.find('indexdiv')
        indexentries_before = indexdiv.findall('indexentry')
        assert len(indexentries_before) == 0

        # Apply fix
        fixes = fixer._fix_indexdiv_missing_indexentry(root, "test.xml")

        # Verify indexentry was added
        indexentries_after = indexdiv.findall('indexentry')
        assert len(indexentries_after) >= 1

        # Verify fix was reported
        assert len(fixes) >= 1


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
