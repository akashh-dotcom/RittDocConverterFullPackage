#!/usr/bin/env python3
"""
Test suite for fix_ulink_urls.py
"""

import tempfile
import shutil
from pathlib import Path
from lxml import etree

from fix_ulink_urls import fix_ulink_urls_in_file


def test_fix_ulink_with_xml_extension():
    """Test fixing ulink URLs with .xml extension."""
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        # Create test XML
        xml_file = temp_dir / "test.xml"
        xml_content = """<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0001">
    <table>
        <row>
            <entry>
                <para><ulink url="ch0007.xml#ch0007s0005ta03">Table 3.3</ulink></para>
            </entry>
        </row>
        <row>
            <entry>
                <para><ulink url="ch0005.xml#ch0005s0002ta01">Table 1.1</ulink></para>
            </entry>
        </row>
    </table>
</chapter>"""
        xml_file.write_text(xml_content)
        
        # Fix URLs
        total, fixed = fix_ulink_urls_in_file(xml_file)
        
        assert total == 2, f"Expected 2 ulinks, got {total}"
        assert fixed == 2, f"Expected 2 fixed, got {fixed}"
        
        # Read back and verify
        tree = etree.parse(str(xml_file))
        root = tree.getroot()
        
        ulinks = root.findall('.//ulink')
        assert len(ulinks) == 2
        
        assert ulinks[0].get('url') == 'ch0007#ch0007s0005ta03', \
            f"Expected 'ch0007#ch0007s0005ta03', got {ulinks[0].get('url')}"
        
        assert ulinks[1].get('url') == 'ch0005#ch0005s0002ta01', \
            f"Expected 'ch0005#ch0005s0002ta01', got {ulinks[1].get('url')}"
        
        print("✓ Test passed: fix_ulink_with_xml_extension")
    
    finally:
        shutil.rmtree(temp_dir)


def test_dont_change_correct_urls():
    """Test that correctly formatted URLs are not changed."""
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        xml_file = temp_dir / "test.xml"
        xml_content = """<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0001">
    <para>
        <ulink url="ch0007#ch0007s0005ta03">Already correct</ulink>
        <ulink url="http://example.com/page.xml#anchor">External link</ulink>
    </para>
</chapter>"""
        xml_file.write_text(xml_content)
        
        # Try to fix (should find nothing to fix)
        total, fixed = fix_ulink_urls_in_file(xml_file)
        
        assert total == 2, f"Expected 2 ulinks, got {total}"
        assert fixed == 0, f"Expected 0 fixed (already correct), got {fixed}"
        
        print("✓ Test passed: dont_change_correct_urls")
    
    finally:
        shutil.rmtree(temp_dir)


def test_mixed_urls():
    """Test file with both correct and incorrect URLs."""
    temp_dir = Path(tempfile.mkdtemp())
    
    try:
        xml_file = temp_dir / "test.xml"
        xml_content = """<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0001">
    <para>
        <ulink url="ch0007.xml#ch0007s0005ta03">Needs fix</ulink>
        <ulink url="ch0008#ch0008s0001">Already correct</ulink>
        <ulink url="ch0009.xml#ch0009s0001ta01">Needs fix</ulink>
    </para>
</chapter>"""
        xml_file.write_text(xml_content)
        
        total, fixed = fix_ulink_urls_in_file(xml_file)
        
        assert total == 3
        assert fixed == 2, f"Expected 2 fixed, got {fixed}"
        
        # Verify
        tree = etree.parse(str(xml_file))
        ulinks = tree.getroot().findall('.//ulink')
        
        assert ulinks[0].get('url') == 'ch0007#ch0007s0005ta03'
        assert ulinks[1].get('url') == 'ch0008#ch0008s0001'
        assert ulinks[2].get('url') == 'ch0009#ch0009s0001ta01'
        
        print("✓ Test passed: mixed_urls")
    
    finally:
        shutil.rmtree(temp_dir)


def run_all_tests():
    """Run all tests."""
    tests = [
        test_fix_ulink_with_xml_extension,
        test_dont_change_correct_urls,
        test_mixed_urls,
    ]
    
    print("Running tests for fix_ulink_urls.py\n")
    
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
