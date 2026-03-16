#!/usr/bin/env python3
"""
Test suite for renumber_section_ids.py
"""

import tempfile
import shutil
from pathlib import Path
from lxml import etree

from renumber_section_ids import (
    get_section_level,
    extract_chapter_id,
    build_new_section_id,
    renumber_sections_in_chapter,
    update_references,
    renumber_xml_file
)


def test_get_section_level():
    """Test extracting section level from ID."""
    assert get_section_level('ch0001s0001') == 1
    assert get_section_level('ch0001s0001s0001') == 2
    assert get_section_level('ch0001s0001s0001s01') == 3
    assert get_section_level('ch0001s0001s0001s01s01') == 4
    print("✓ Test passed: get_section_level")


def test_extract_chapter_id():
    """Test extracting chapter ID."""
    assert extract_chapter_id('ch0001s0001') == 'ch0001'
    assert extract_chapter_id('ch0001s0001s0002') == 'ch0001'
    assert extract_chapter_id('pr0001s0000') == 'pr0001'
    print("✓ Test passed: extract_chapter_id")


def test_build_new_section_id():
    """Test building new section IDs."""
    # sect1
    assert build_new_section_id('ch0001', '', 1, 0) == 'ch0001s0001'
    assert build_new_section_id('ch0001', '', 1, 1) == 'ch0001s0002'
    
    # sect2
    assert build_new_section_id('ch0001', 'ch0001s0001', 2, 0) == 'ch0001s0001s0001'
    assert build_new_section_id('ch0001', 'ch0001s0001', 2, 1) == 'ch0001s0001s0002'
    
    # sect3 (2-digit)
    assert build_new_section_id('ch0001', 'ch0001s0001s0001', 3, 0) == 'ch0001s0001s0001s01'
    assert build_new_section_id('ch0001', 'ch0001s0001s0001', 3, 1) == 'ch0001s0001s0001s02'
    
    print("✓ Test passed: build_new_section_id")


def test_renumber_sections():
    """Test renumbering sections with out-of-order IDs."""
    # Create chapter with sections in wrong order
    chapter = etree.Element('chapter', id='ch0013')
    title = etree.SubElement(chapter, 'title')
    title.text = 'Chapter 13'
    
    # Sect1 elements - REFERENCES has s0002 but should be s0003
    sect1_1 = etree.SubElement(chapter, 'sect1', id='ch0013s0001')
    title1 = etree.SubElement(sect1_1, 'title')
    title1.text = 'CASE DISCUSSION'
    
    sect1_3 = etree.SubElement(chapter, 'sect1', id='ch0013s0003')
    title3 = etree.SubElement(sect1_3, 'title')
    title3.text = 'KEY LEARNING POINTS'
    
    # This one is out of sequence - should be s0003 but is s0002
    sect1_2 = etree.SubElement(chapter, 'sect1', id='ch0013s0002')
    title2 = etree.SubElement(sect1_2, 'title')
    title2.text = 'REFERENCES'
    
    # Renumber
    id_mapping = renumber_sections_in_chapter(chapter)
    
    # Check mapping
    assert 'ch0013s0003' in id_mapping
    assert id_mapping['ch0013s0003'] == 'ch0013s0002', f"Expected ch0013s0002, got {id_mapping['ch0013s0003']}"
    
    assert 'ch0013s0002' in id_mapping
    assert id_mapping['ch0013s0002'] == 'ch0013s0003', f"Expected ch0013s0003, got {id_mapping['ch0013s0002']}"
    
    # Check actual IDs were updated
    sections = chapter.findall('sect1')
    assert sections[0].get('id') == 'ch0013s0001'  # First section stays s0001
    assert sections[1].get('id') == 'ch0013s0002'  # Second section now s0002 (was s0003)
    assert sections[2].get('id') == 'ch0013s0003'  # Third section now s0003 (was s0002)
    
    print("✓ Test passed: renumber_sections")


def test_update_references():
    """Test updating references after renumbering."""
    # Create XML with links
    root = etree.Element('book')
    chapter = etree.SubElement(root, 'chapter', id='ch0013')
    
    # TOC with links to old IDs
    toc_list = etree.SubElement(chapter, 'itemizedlist')
    
    item1 = etree.SubElement(toc_list, 'listitem')
    para1 = etree.SubElement(item1, 'para')
    ulink1 = etree.SubElement(para1, 'ulink', url='ch0013#ch0013s0002')
    ulink1.text = 'REFERENCES'
    
    item2 = etree.SubElement(toc_list, 'listitem')
    para2 = etree.SubElement(item2, 'para')
    link2 = etree.SubElement(para2, 'link', linkend='ch0013s0003')
    link2.text = 'KEY LEARNING POINTS'
    
    # ID mapping (s0002 and s0003 swapped)
    id_mapping = {
        'ch0013s0002': 'ch0013s0003',
        'ch0013s0003': 'ch0013s0002'
    }
    
    # Update references
    updated = update_references(root, id_mapping)
    assert updated == 2, f"Expected 2 updates, got {updated}"
    
    # Check updates
    assert ulink1.get('url') == 'ch0013#ch0013s0003', f"Expected ch0013#ch0013s0003, got {ulink1.get('url')}"
    assert link2.get('linkend') == 'ch0013s0002', f"Expected ch0013s0002, got {link2.get('linkend')}"
    
    print("✓ Test passed: update_references")


def test_full_file_renumber():
    """Test renumbering a complete XML file."""
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        # Create XML file
        xml_file = temp_dir / "ch0013.xml"
        xml_content = """<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0013">
  <title>CASE 10</title>
  <itemizedlist role="contents">
    <listitem>
      <para><ulink url="ch0013#ch0013s0001">CASE DISCUSSION</ulink></para>
    </listitem>
    <listitem>
      <para><ulink url="ch0013#ch0013s0002">REFERENCES</ulink></para>
    </listitem>
    <listitem>
      <para><ulink url="ch0013#ch0013s0003">KEY LEARNING POINTS</ulink></para>
    </listitem>
  </itemizedlist>
  <sect1 id="ch0013s0001">
    <title>CASE DISCUSSION</title>
    <para>Some content</para>
  </sect1>
  <sect1 id="ch0013s0003">
    <title>KEY LEARNING POINTS</title>
    <para>Some content</para>
  </sect1>
  <sect1 id="ch0013s0002">
    <title>REFERENCES</title>
    <para>Bibliography content</para>
  </sect1>
</chapter>"""
        xml_file.write_text(xml_content)
        
        # Renumber
        sections, refs = renumber_xml_file(xml_file)
        
        assert sections == 2, f"Expected 2 sections renumbered, got {sections}"
        assert refs == 2, f"Expected 2 references updated, got {refs}"
        
        # Read back and check
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        sections = root.findall('.//sect1')
        # Check order: CASE DISCUSSION, KEY LEARNING POINTS, REFERENCES
        assert sections[0].get('id') == 'ch0013s0001'
        assert sections[1].get('id') == 'ch0013s0002'  # Was s0003
        assert sections[2].get('id') == 'ch0013s0003'  # Was s0002
        
        # Check TOC links updated
        ulinks = root.findall('.//ulink')
        assert ulinks[0].get('url') == 'ch0013#ch0013s0001'
        assert ulinks[1].get('url') == 'ch0013#ch0013s0003'  # Was s0002, now s0003
        assert ulinks[2].get('url') == 'ch0013#ch0013s0002'  # Was s0003, now s0002
        
        print("✓ Test passed: full_file_renumber")
    
    finally:
        shutil.rmtree(temp_dir)


def run_all_tests():
    """Run all tests."""
    tests = [
        test_get_section_level,
        test_extract_chapter_id,
        test_build_new_section_id,
        test_renumber_sections,
        test_update_references,
        test_full_file_renumber,
    ]
    
    print("Running tests for renumber_section_ids.py\n")
    
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
