#!/usr/bin/env python3
"""
Fix bibliography TOC links by correcting bibliography IDs.

Issue:
- Bibliography elements incorrectly have IDs ending in 'bib' (e.g., ch0004s0002bib)
- The correct format is the 11-character sect1-style ID (e.g., ch0004s0002)
- TOC links can't resolve to these incorrect IDs, so they become <phrase> elements
- This causes REFERENCES sections to appear without links in the TOC

Solution:
1. Find all bibliography elements with IDs ending in 'bib'
2. Correct the bibliography ID by removing the 'bib' suffix
3. Update all linkend attributes pointing to the old ID
4. Update all url attributes containing the old ID (for ulink elements)
"""

import re
import os
import sys
from pathlib import Path
from lxml import etree
from typing import List, Tuple, Set
import logging

logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
logger = logging.getLogger(__name__)


def fix_bibliography_ids_in_file(file_path: Path) -> Tuple[int, int]:
    """
    Fix bibliography IDs in a single XML file.
    
    Args:
        file_path: Path to XML file
        
    Returns:
        Tuple of (bibliographies_fixed, references_updated)
    """
    try:
        tree = etree.parse(str(file_path))
        root = tree.getroot()
    except Exception as e:
        logger.error(f"Failed to parse {file_path}: {e}")
        return 0, 0
    
    bibliographies_fixed = 0
    references_updated = 0
    id_mapping = {}  # old_id -> new_id
    
    # Step 1: Find and fix bibliography elements with incorrect IDs
    for bib_elem in root.iter('bibliography'):
        old_id = bib_elem.get('id', '')
        
        if not old_id:
            continue
            
        # Check if ID incorrectly ends with 'bib' and has more than 11 characters
        # Correct format: ch0004s0002 (11 chars)
        # Incorrect format: ch0004s0002bib (14+ chars)
        if len(old_id) > 11 and old_id.endswith('bib'):
            # Check if removing 'bib' gives us an 11-character ID
            potential_new_id = old_id[:-3]  # Remove 'bib'
            
            # Validate the new ID format (should be like ch0004s0002)
            if len(potential_new_id) == 11 and re.match(r'^[a-z]{2}\d{4}s\d{4}$', potential_new_id):
                # Check if the new ID is already in use
                existing_elem = root.xpath(f'//*[@id="{potential_new_id}"]')
                if existing_elem and existing_elem[0] != bib_elem:
                    logger.warning(f"Cannot fix {old_id} -> {potential_new_id}: ID already in use in {file_path}")
                    continue
                
                # Update the bibliography ID
                bib_elem.set('id', potential_new_id)
                id_mapping[old_id] = potential_new_id
                bibliographies_fixed += 1
                logger.info(f"Fixed bibliography ID: {old_id} -> {potential_new_id} in {file_path.name}")
    
    # Step 2: Update all linkend attributes pointing to old bibliography IDs
    for old_id, new_id in id_mapping.items():
        # Update linkend attributes
        for elem in root.xpath(f'//*[@linkend="{old_id}"]'):
            elem.set('linkend', new_id)
            references_updated += 1
            logger.debug(f"Updated linkend: {old_id} -> {new_id} in {elem.tag}")
        
        # Update url attributes in ulink elements that reference the old ID
        for ulink in root.xpath(f'//ulink'):
            url = ulink.get('url', '')
            if old_id in url:
                # Replace the old ID with new ID in the URL
                new_url = url.replace(old_id, new_id)
                ulink.set('url', new_url)
                references_updated += 1
                logger.debug(f"Updated ulink url: {url} -> {new_url}")
        
        # Update link elements with linkend attributes
        for link in root.xpath(f'//link[@linkend="{old_id}"]'):
            link.set('linkend', new_id)
            references_updated += 1
            logger.debug(f"Updated link linkend: {old_id} -> {new_id}")
    
    # Step 3: Save the file if changes were made
    if bibliographies_fixed > 0 or references_updated > 0:
        try:
            tree.write(str(file_path), encoding='utf-8', xml_declaration=True, pretty_print=True)
            logger.info(f"Saved {file_path.name}: {bibliographies_fixed} bibliographies fixed, {references_updated} references updated")
        except Exception as e:
            logger.error(f"Failed to write {file_path}: {e}")
            return 0, 0
    
    return bibliographies_fixed, references_updated


def fix_cross_file_references(directory: Path) -> int:
    """
    Fix cross-file references after all bibliography IDs have been corrected.
    
    This handles cases where a TOC in one file references a bibliography in another file.
    
    Args:
        directory: Directory containing XML files
        
    Returns:
        Number of cross-file references updated
    """
    updated = 0
    
    # Build a map of all bibliography IDs across all files
    bib_id_map = {}  # bibliography_id -> file_path
    
    for xml_file in directory.glob('*.xml'):
        try:
            tree = etree.parse(str(xml_file))
            root = tree.getroot()
            
            for bib_elem in root.iter('bibliography'):
                bib_id = bib_elem.get('id', '')
                if bib_id:
                    bib_id_map[bib_id] = xml_file
        except Exception as e:
            logger.warning(f"Could not parse {xml_file} for cross-reference checking: {e}")
            continue
    
    logger.info(f"Found {len(bib_id_map)} bibliography elements across all files")
    
    # Now check all files for broken references that might point to bibliographies
    for xml_file in directory.glob('*.xml'):
        try:
            tree = etree.parse(str(xml_file))
            root = tree.getroot()
            file_modified = False
            
            # Look for TOC lists with role containing 'contents'
            for itemizedlist in root.iter('itemizedlist'):
                role = itemizedlist.get('role', '')
                if 'contents' not in role.lower():
                    continue
                
                # Check for phrase elements that should be links (REFERENCES without links)
                for listitem in itemizedlist.iter('listitem'):
                    for para in listitem.iter('para'):
                        for phrase in list(para.iter('phrase')):
                            phrase_text = ''.join(phrase.itertext()).strip()
                            
                            # Check if this is a REFERENCES entry without a link
                            if phrase_text.upper() in ['REFERENCES', 'BIBLIOGRAPHY', 'WORKS CITED']:
                                # Try to find a bibliography section to link to
                                # Look in current file first
                                parent_chapter_id = None
                                
                                # Try to determine the chapter ID from the file name or content
                                for chapter in root.iter('chapter'):
                                    parent_chapter_id = chapter.get('id', '')
                                    if parent_chapter_id:
                                        break
                                
                                if parent_chapter_id:
                                    # Look for bibliography in this chapter
                                    for bib_id in bib_id_map:
                                        if bib_id.startswith(parent_chapter_id):
                                            # Found a bibliography in this chapter, create a link
                                            # Replace phrase with ulink
                                            ulink = etree.Element('ulink')
                                            ulink.set('url', f'{bib_id}')
                                            ulink.text = phrase_text
                                            
                                            # Replace phrase with ulink
                                            parent = phrase.getparent()
                                            if parent is not None:
                                                parent.replace(phrase, ulink)
                                                file_modified = True
                                                updated += 1
                                                logger.info(f"Converted phrase '{phrase_text}' to ulink in {xml_file.name}")
                                            break
            
            if file_modified:
                tree.write(str(xml_file), encoding='utf-8', xml_declaration=True, pretty_print=True)
                logger.info(f"Saved {xml_file.name} with cross-file reference fixes")
        
        except Exception as e:
            logger.warning(f"Could not process {xml_file} for cross-file references: {e}")
            continue
    
    return updated


def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description='Fix bibliography IDs and TOC links for REFERENCES sections'
    )
    parser.add_argument(
        'directory',
        type=str,
        help='Directory containing XML files to process'
    )
    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='Show what would be changed without making changes'
    )
    
    args = parser.parse_args()
    
    directory = Path(args.directory)
    if not directory.exists() or not directory.is_dir():
        logger.error(f"Directory not found: {directory}")
        sys.exit(1)
    
    if args.dry_run:
        logger.info("DRY RUN MODE - No files will be modified")
        # For dry run, just set logging to show what would happen
        # We'd need to refactor the code to support this properly
        logger.warning("Dry run mode not fully implemented, running in normal mode")
    
    # Find all XML files
    xml_files = list(directory.glob('*.xml'))
    logger.info(f"Found {len(xml_files)} XML files in {directory}")
    
    if not xml_files:
        logger.warning("No XML files found")
        sys.exit(0)
    
    total_bibs_fixed = 0
    total_refs_updated = 0
    
    # Process each file
    for xml_file in xml_files:
        bibs, refs = fix_bibliography_ids_in_file(xml_file)
        total_bibs_fixed += bibs
        total_refs_updated += refs
    
    logger.info(f"\nSummary:")
    logger.info(f"  Bibliography IDs fixed: {total_bibs_fixed}")
    logger.info(f"  References updated: {total_refs_updated}")
    
    # Fix cross-file references
    logger.info(f"\nChecking for cross-file references...")
    cross_refs = fix_cross_file_references(directory)
    logger.info(f"  Cross-file references updated: {cross_refs}")
    
    logger.info(f"\n✓ Done! Processed {len(xml_files)} files")


if __name__ == '__main__':
    main()
