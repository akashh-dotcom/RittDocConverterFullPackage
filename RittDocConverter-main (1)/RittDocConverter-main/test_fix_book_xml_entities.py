#!/usr/bin/env python3
"""
Test suite for fix_book_xml_entities.py
"""

import tempfile
import shutil
from pathlib import Path

from fix_book_xml_entities import (
    find_entity_references,
    find_declared_entities,
    extract_doctype_and_body,
    build_doctype_with_entities,
    fix_book_xml_entities
)


def test_find_entity_references():
    """Test finding entity references in XML content."""
    xml = """<?xml version="1.0"?>
<book>
  &pr0001;
  &ch0001;
  &ch0002;
  &lt;  <!-- standard entity, should be ignored -->
</book>"""
    
    refs = find_entity_references(xml)
    assert refs == {'pr0001', 'ch0001', 'ch0002'}, f"Got {refs}"
    print("✓ Test passed: find_entity_references")


def test_find_declared_entities():
    """Test finding declared entities in DOCTYPE."""
    xml = """<?xml version="1.0"?>
<!DOCTYPE book [
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
]>
<book>
  &pr0001;
</book>"""
    
    declared = find_declared_entities(xml)
    assert declared == {'pr0001', 'ch0001'}, f"Got {declared}"
    print("✓ Test passed: find_declared_entities")


def test_build_doctype_with_entities():
    """Test building DOCTYPE with entity declarations."""
    entities = ['pr0001', 'ch0001', 'ch0002']
    doctype = build_doctype_with_entities(entities)
    
    # Check structure
    assert '<!DOCTYPE book PUBLIC' in doctype
    assert '<!ENTITY pr0001 SYSTEM "pr0001.xml">' in doctype
    assert '<!ENTITY ch0001 SYSTEM "ch0001.xml">' in doctype
    assert '<!ENTITY ch0002 SYSTEM "ch0002.xml">' in doctype
    assert doctype.endswith(']>')
    
    print("✓ Test passed: build_doctype_with_entities")


def test_fix_missing_entities():
    """Test fixing Book.XML with missing entity declarations."""
    # Create temporary directory
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        # Create Book.XML with missing entity declarations
        book_xml = temp_dir / "Book.XML"
        book_xml.write_text("""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
\t<!ENTITY ch0001 SYSTEM "ch0001.xml">
]>
<book id="book0001">
  <title>Test Book</title>
  &pr0001;
  &ch0001;
  &ch0002;
</book>""")
        
        # Create corresponding XML files
        (temp_dir / "pr0001.xml").write_text("<?xml version='1.0'?>\n<preface id='pr0001'><title>Preface</title></preface>")
        (temp_dir / "ch0001.xml").write_text("<?xml version='1.0'?>\n<chapter id='ch0001'><title>Chapter 1</title></chapter>")
        (temp_dir / "ch0002.xml").write_text("<?xml version='1.0'?>\n<chapter id='ch0002'><title>Chapter 2</title></chapter>")
        
        # Fix the entities
        added, removed = fix_book_xml_entities(book_xml, remove_orphans=False)
        
        # Should have added 2 entities (pr0001 and ch0002)
        assert added == 2, f"Expected 2 entities added, got {added}"
        assert removed == 0, f"Expected 0 entities removed, got {removed}"
        
        # Read the fixed file
        fixed_content = book_xml.read_text()
        
        # Check that all entities are now declared
        assert '<!ENTITY pr0001 SYSTEM "pr0001.xml">' in fixed_content
        assert '<!ENTITY ch0001 SYSTEM "ch0001.xml">' in fixed_content
        assert '<!ENTITY ch0002 SYSTEM "ch0002.xml">' in fixed_content
        
        # Check that references are still there
        assert '&pr0001;' in fixed_content
        assert '&ch0001;' in fixed_content
        assert '&ch0002;' in fixed_content
        
        print("✓ Test passed: fix_missing_entities")
    
    finally:
        shutil.rmtree(temp_dir)


def test_remove_orphan_entities():
    """Test removing orphan entity references (no corresponding .xml file)."""
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        # Create Book.XML with orphan entity reference
        book_xml = temp_dir / "Book.XML"
        book_xml.write_text("""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd">
<book id="book0001">
  <title>Test Book</title>
  &pr0001;
  &ch0001;
  &orphan;
</book>""")
        
        # Create files for pr0001 and ch0001, but NOT for orphan
        (temp_dir / "pr0001.xml").write_text("<?xml version='1.0'?>\n<preface id='pr0001'><title>Preface</title></preface>")
        (temp_dir / "ch0001.xml").write_text("<?xml version='1.0'?>\n<chapter id='ch0001'><title>Chapter 1</title></chapter>")
        
        # Fix with remove_orphans=True
        added, removed = fix_book_xml_entities(book_xml, remove_orphans=True)
        
        # Should have added 2 entities and removed 1 orphan
        assert added == 2, f"Expected 2 entities added, got {added}"
        assert removed == 1, f"Expected 1 entity removed, got {removed}"
        
        # Read the fixed file
        fixed_content = book_xml.read_text()
        
        # Check that valid entities are declared
        assert '<!ENTITY pr0001 SYSTEM "pr0001.xml">' in fixed_content
        assert '<!ENTITY ch0001 SYSTEM "ch0001.xml">' in fixed_content
        
        # Check that orphan entity is NOT declared and NOT referenced
        assert 'orphan' not in fixed_content
        
        print("✓ Test passed: remove_orphan_entities")
    
    finally:
        shutil.rmtree(temp_dir)


def test_no_changes_needed():
    """Test when all entities are already properly declared."""
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        # Create Book.XML with all entities properly declared
        book_xml = temp_dir / "Book.XML"
        original_content = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
\t<!ENTITY ch0001 SYSTEM "ch0001.xml">
\t<!ENTITY pr0001 SYSTEM "pr0001.xml">
]>
<book id="book0001">
  <title>Test Book</title>
  &pr0001;
  &ch0001;
</book>"""
        book_xml.write_text(original_content)
        
        # Fix the entities
        added, removed = fix_book_xml_entities(book_xml, remove_orphans=False)
        
        # Should have made no changes
        assert added == 0, f"Expected 0 entities added, got {added}"
        assert removed == 0, f"Expected 0 entities removed, got {removed}"
        
        print("✓ Test passed: no_changes_needed")
    
    finally:
        shutil.rmtree(temp_dir)


def run_all_tests():
    """Run all tests."""
    tests = [
        test_find_entity_references,
        test_find_declared_entities,
        test_build_doctype_with_entities,
        test_fix_missing_entities,
        test_remove_orphan_entities,
        test_no_changes_needed,
    ]
    
    print("Running tests for fix_book_xml_entities.py\n")
    
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
