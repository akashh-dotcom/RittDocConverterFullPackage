#!/usr/bin/env python3
"""
Fix ulink URLs to remove .xml extension

Changes:
  <ulink url="ch0007.xml#ch0007s0005ta03"> 
to:
  <ulink url="ch0007#ch0007s0005ta03">

This is needed for proper link resolution in the R2 platform.
"""

import re
import sys
from pathlib import Path
from lxml import etree
from typing import Tuple
import logging

logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
logger = logging.getLogger(__name__)


def fix_ulink_urls_in_file(xml_path: Path) -> Tuple[int, int]:
    """
    Fix ulink URLs by removing .xml extension.
    
    Args:
        xml_path: Path to XML file
        
    Returns:
        Tuple of (total_ulinks, fixed_count)
    """
    try:
        tree = etree.parse(str(xml_path))
        root = tree.getroot()
    except Exception as e:
        logger.error(f"Failed to parse {xml_path}: {e}")
        return 0, 0
    
    total_ulinks = 0
    fixed_count = 0
    
    # Find all ulink elements with url attributes
    for ulink in root.xpath('//ulink[@url]'):
        total_ulinks += 1
        url = ulink.get('url')
        
        # Only fix internal links (not external HTTP/HTTPS URLs)
        # Pattern: ch0007.xml#ch0007s0005ta03 -> ch0007#ch0007s0005ta03
        # Skip: http://example.com/page.xml#anchor (external link)
        if '.xml#' in url and not url.startswith(('http://', 'https://', 'ftp://', '//')):
            # Check if this looks like an internal chapter reference
            # Internal format: ch####.xml# or pr####.xml# or ap####.xml#
            if re.match(r'^[a-z]{2}\d{4}\.xml#', url):
                # Remove .xml extension
                new_url = url.replace('.xml#', '#', 1)  # Replace only first occurrence
                ulink.set('url', new_url)
                fixed_count += 1
                logger.debug(f"Fixed: {url} -> {new_url}")
    
    # Save the file if changes were made
    if fixed_count > 0:
        try:
            tree.write(str(xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)
            logger.info(f"[OK] {xml_path.name}: {fixed_count}/{total_ulinks} ulink URLs fixed")
            return total_ulinks, fixed_count
        except Exception as e:
            logger.error(f"Failed to write {xml_path}: {e}")
            return total_ulinks, 0
    
    return total_ulinks, 0


def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description='Fix ulink URLs by removing .xml extension'
    )
    parser.add_argument(
        'path',
        type=str,
        help='Path to XML file or directory containing XML files'
    )
    parser.add_argument(
        '--verbose',
        action='store_true',
        help='Show detailed debug output'
    )
    
    args = parser.parse_args()
    
    if args.verbose:
        logger.setLevel(logging.DEBUG)
    
    path = Path(args.path)
    if not path.exists():
        logger.error(f"Path not found: {path}")
        sys.exit(1)
    
    # Collect XML files
    xml_files = []
    if path.is_file():
        if path.suffix.lower() == '.xml':
            xml_files = [path]
    else:
        xml_files = list(path.glob('*.xml'))
    
    if not xml_files:
        logger.warning(f"No XML files found in {path}")
        sys.exit(0)
    
    logger.info(f"Found {len(xml_files)} XML file(s)")
    
    total_ulinks = 0
    total_fixed = 0
    files_modified = 0
    
    for xml_file in sorted(xml_files):
        ulinks, fixed = fix_ulink_urls_in_file(xml_file)
        total_ulinks += ulinks
        total_fixed += fixed
        if fixed > 0:
            files_modified += 1
    
    print("\n" + "="*60)
    print(f"[OK] URL fix complete:")
    print(f"  Total ulink elements: {total_ulinks}")
    print(f"  URLs fixed: {total_fixed}")
    print(f"  Files modified: {files_modified}/{len(xml_files)}")
    print("="*60)


if __name__ == '__main__':
    main()
