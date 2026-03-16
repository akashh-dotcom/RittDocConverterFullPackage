#!/usr/bin/env python3
"""
Test suite for fix_bibliography_toc_links.py
"""

import tempfile
import shutil
from pathlib import Path
from lxml import etree

from fix_bibliography_toc_links import fix_bibliography_ids_in_file, fix_cross_file_references


def create_test_xml(content: str) -> Path:
    """Create a temporary XML file for testing."""
    temp_dir = Path(tempfile.mkdtemp())
    xml_file = temp_dir / "test.xml"
    xml_file.write_text(content)
    return xml_file


def test_fix_bibliography_id_with_bib_suffix():
    """Test fixing a bibliography element with incorrect 'bib' suffix."""
    xml_content = '''<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0004">
    <title>Chapter 4</title>
    <sect1 id="ch0004s0001">
        <title>Introduction</title>
        <para>Some content</para>
    </sect1>
    <bibliography id="ch0004s0002bib">
        <title>References</title>
        <bibliomixed id="ch0004s0002bib01">Author, A. (2023). Article Title.</bibliomixed>
    </bibliography>
</chapter>'''
    
    xml_file = create_test_xml(xml_content)
    
    try:
        bibs_fixed, refs_updated = fix_bibliography_ids_in_file(xml_file)
        
        # Should fix 1 bibliography
        assert bibs_fixed == 1, f"Expected 1 bibliography fixed, got {bibs_fixed}"
        
        # Read the result
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        # Check that bibliography ID was corrected
        bib = root.find('.//bibliography')
        assert bib is not None
        assert bib.get('id') == 'ch0004s0002', f"Expected 'ch0004s0002', got {bib.get('id')}"
        
        print("✓ Test passed: Bibliography ID with 'bib' suffix was corrected")
    finally:
        shutil.rmtree(xml_file.parent)


def test_fix_linkend_references():
    """Test that linkend references are updated when bibliography ID changes."""
    xml_content = '''<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0004">
    <title>Chapter 4</title>
    <itemizedlist role="contentsH3">
        <listitem>
            <para><link linkend="ch0004s0002bib">REFERENCES</link></para>
        </listitem>
    </itemizedlist>
    <bibliography id="ch0004s0002bib">
        <title>References</title>
    </bibliography>
</chapter>'''
    
    xml_file = create_test_xml(xml_content)
    
    try:
        bibs_fixed, refs_updated = fix_bibliography_ids_in_file(xml_file)
        
        # Should fix 1 bibliography and 1 reference
        assert bibs_fixed == 1
        assert refs_updated == 1, f"Expected 1 reference updated, got {refs_updated}"
        
        # Read the result
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        # Check that link was updated
        link = root.find('.//link')
        assert link is not None
        assert link.get('linkend') == 'ch0004s0002', f"Expected 'ch0004s0002', got {link.get('linkend')}"
        
        print("✓ Test passed: Linkend references were updated")
    finally:
        shutil.rmtree(xml_file.parent)


def test_fix_ulink_url_references():
    """Test that ulink url attributes are updated when bibliography ID changes."""
    xml_content = '''<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0019">
    <title>Chapter 19</title>
    <itemizedlist role="contentsH3">
        <listitem>
            <para><ulink url="ch0019#ch0019s0001">CASE DISCUSSION</ulink></para>
        </listitem>
        <listitem>
            <para><ulink url="ch0019#ch0019s0002bib">REFERENCES</ulink></para>
        </listitem>
    </itemizedlist>
    <sect1 id="ch0019s0001">
        <title>Case Discussion</title>
    </sect1>
    <bibliography id="ch0019s0002bib">
        <title>References</title>
    </bibliography>
</chapter>'''
    
    xml_file = create_test_xml(xml_content)
    
    try:
        bibs_fixed, refs_updated = fix_bibliography_ids_in_file(xml_file)
        
        assert bibs_fixed == 1
        assert refs_updated == 1, f"Expected 1 reference updated, got {refs_updated}"
        
        # Read the result
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        # Check that ulink was updated
        ulinks = root.findall('.//ulink')
        refs_ulink = [u for u in ulinks if 'REFERENCES' in (u.text or '')]
        assert len(refs_ulink) == 1
        assert refs_ulink[0].get('url') == 'ch0019#ch0019s0002', \
            f"Expected 'ch0019#ch0019s0002', got {refs_ulink[0].get('url')}"
        
        print("✓ Test passed: Ulink URL references were updated")
    finally:
        shutil.rmtree(xml_file.parent)


def test_dont_fix_correct_bibliography_id():
    """Test that correctly formatted bibliography IDs are not changed."""
    xml_content = '''<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0004">
    <title>Chapter 4</title>
    <bibliography id="ch0004s0002">
        <title>References</title>
        <bibliomixed id="ch0004s0002bib01">Author, A. (2023). Article Title.</bibliomixed>
    </bibliography>
</chapter>'''
    
    xml_file = create_test_xml(xml_content)
    
    try:
        bibs_fixed, refs_updated = fix_bibliography_ids_in_file(xml_file)
        
        # Should not fix anything
        assert bibs_fixed == 0, f"Expected 0 bibliographies fixed, got {bibs_fixed}"
        
        # Read the result
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        # Check that bibliography ID was not changed
        bib = root.find('.//bibliography')
        assert bib is not None
        assert bib.get('id') == 'ch0004s0002'
        
        print("✓ Test passed: Correct bibliography ID was not changed")
    finally:
        shutil.rmtree(xml_file.parent)


def test_fix_multiple_bibliographies():
    """Test fixing multiple bibliography elements in one file."""
    xml_content = '''<?xml version="1.0" encoding="UTF-8"?>
<book id="book0001">
    <chapter id="ch0001">
        <title>Chapter 1</title>
        <bibliography id="ch0001s0005bib">
            <title>References</title>
        </bibliography>
    </chapter>
    <chapter id="ch0002">
        <title>Chapter 2</title>
        <bibliography id="ch0002s0003bib">
            <title>References</title>
        </bibliography>
    </chapter>
</book>'''
    
    xml_file = create_test_xml(xml_content)
    
    try:
        bibs_fixed, refs_updated = fix_bibliography_ids_in_file(xml_file)
        
        # Should fix 2 bibliographies
        assert bibs_fixed == 2, f"Expected 2 bibliographies fixed, got {bibs_fixed}"
        
        # Read the result
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        # Check both bibliography IDs were corrected
        bibs = root.findall('.//bibliography')
        assert len(bibs) == 2
        assert bibs[0].get('id') == 'ch0001s0005'
        assert bibs[1].get('id') == 'ch0002s0003'
        
        print("✓ Test passed: Multiple bibliography IDs were fixed")
    finally:
        shutil.rmtree(xml_file.parent)


def run_all_tests():
    """Run all tests."""
    tests = [
        test_fix_bibliography_id_with_bib_suffix,
        test_fix_linkend_references,
        test_fix_ulink_url_references,
        test_dont_fix_correct_bibliography_id,
        test_fix_multiple_bibliographies,
    ]
    
    print("Running tests for fix_bibliography_toc_links.py\n")
    
    failed = 0
    for test in tests:
        try:
            test()
        except AssertionError as e:
            print(f"✗ {test.__name__} FAILED: {e}")
            failed += 1
        except Exception as e:
            print(f"✗ {test.__name__} ERROR: {e}")
            failed += 1
    
    print(f"\n{'='*60}")
    if failed == 0:
        print(f"✓ All {len(tests)} tests passed!")
    else:
        print(f"✗ {failed}/{len(tests)} tests failed")
    
    return failed == 0


if __name__ == '__main__':
    success = run_all_tests()
    exit(0 if success else 1)
