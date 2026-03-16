#!/usr/bin/env python3
"""
Renumber Section IDs to Match Document Order

This script fixes the issue where REFERENCES sections have inconsistent IDs
(like s0002) even though they appear at the end of chapters.

Problem:
- Section IDs are assigned sequentially during conversion (s0001, s0002, s0003)
- Bibliography sections are then moved to the end (DTD compliance)
- But IDs don't get renumbered, so REFERENCES keeps s0002 even at the end

Solution:
- Scan all sections in document order
- Renumber them sequentially (s0001, s0002, s0003, ...)
- Update all linkend and url references to the new IDs

This ensures section IDs match the final document structure.
"""

import re
import sys
from pathlib import Path
from lxml import etree
from typing import Dict, List, Tuple, Set
import logging

logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
logger = logging.getLogger(__name__)


def get_section_level(section_id: str) -> int:
    """
    Get the section level from a section ID.
    
    Examples:
        ch0001s0001 -> 1 (sect1)
        ch0001s0001s0001 -> 2 (sect2)
        ch0001s0001s0001s01 -> 3 (sect3)
    
    Returns:
        Section level (1-5)
    """
    # Count 's' occurrences after chapter prefix
    chapter_part = section_id[:6]  # e.g., 'ch0001'
    rest = section_id[6:]  # Everything after chapter ID
    
    # Count sections by counting 's' followed by digits
    level = rest.count('s')
    return max(1, min(level, 5))


def extract_chapter_id(section_id: str) -> str:
    """Extract chapter ID from section ID (first 6 chars)."""
    return section_id[:6] if len(section_id) >= 6 else section_id


def build_new_section_id(chapter_id: str, parent_id: str, level: int, counter: int) -> str:
    """
    Build a new section ID based on parent and counter.
    
    Args:
        chapter_id: Chapter ID (e.g., 'ch0001')
        parent_id: Parent section ID (empty for sect1)
        level: Section level (1-5)
        counter: Section counter at this level (0-based)
    
    Returns:
        New section ID
    """
    if level == 1:
        # sect1: ch0001s0001
        return f"{chapter_id}s{counter + 1:04d}"
    elif level == 2:
        # sect2: ch0001s0001s0001
        return f"{parent_id}s{counter + 1:04d}"
    else:
        # sect3+: Use 2-digit counters to stay within 25 char limit
        return f"{parent_id}s{counter + 1:02d}"


def renumber_sections_in_chapter(chapter: etree.Element) -> Dict[str, str]:
    """
    Renumber all sections in a chapter to match document order.
    
    Args:
        chapter: Chapter element
    
    Returns:
        Dict mapping old IDs to new IDs
    """
    chapter_id = chapter.get('id', '')
    if not chapter_id:
        return {}
    
    id_mapping = {}
    
    # Section tags in order
    section_tags = ['sect1', 'sect2', 'sect3', 'sect4', 'sect5']
    
    # Track counters for each level
    # Key: parent_id, Value: counter
    counters: Dict[str, int] = {}
    
    def process_section(section: etree.Element, parent_id: str = '', level: int = 1):
        """Recursively process sections and assign new IDs."""
        old_id = section.get('id', '')
        if not old_id:
            return
        
        # Get or initialize counter for this parent
        counter_key = parent_id if parent_id else chapter_id
        if counter_key not in counters:
            counters[counter_key] = 0
        else:
            counters[counter_key] += 1
        
        counter = counters[counter_key]
        
        # Build new ID
        new_id = build_new_section_id(chapter_id, parent_id, level, counter)
        
        # Store mapping
        if old_id != new_id:
            id_mapping[old_id] = new_id
            logger.debug(f"Renumber: {old_id} -> {new_id}")
        
        # Update the section's ID
        section.set('id', new_id)
        
        # Process child sections
        if level < 5:
            child_tag = section_tags[level]  # Next level
            for child_section in section.findall(child_tag):
                process_section(child_section, new_id, level + 1)
    
    # Process all sect1 elements in order
    for sect1 in chapter.findall('sect1'):
        process_section(sect1, '', 1)
    
    return id_mapping


def update_references(root: etree.Element, id_mapping: Dict[str, str]) -> int:
    """
    Update all linkend and url attributes with new IDs.
    
    Args:
        root: Root element
        id_mapping: Dict mapping old IDs to new IDs
    
    Returns:
        Number of references updated
    """
    updated = 0
    
    # Update linkend attributes
    for elem in root.xpath('//*[@linkend]'):
        old_linkend = elem.get('linkend')
        
        # Check if this linkend references a remapped ID
        if old_linkend in id_mapping:
            new_linkend = id_mapping[old_linkend]
            elem.set('linkend', new_linkend)
            updated += 1
            logger.debug(f"Updated linkend: {old_linkend} -> {new_linkend}")
        else:
            # Check if linkend contains a remapped ID (like ch0001s0002fg01)
            for old_id, new_id in id_mapping.items():
                if old_linkend.startswith(old_id):
                    # This is an element ID based on old section ID
                    new_linkend = old_linkend.replace(old_id, new_id, 1)
                    elem.set('linkend', new_linkend)
                    updated += 1
                    logger.debug(f"Updated element linkend: {old_linkend} -> {new_linkend}")
                    break
    
    # Update url attributes in ulink elements
    for ulink in root.xpath('//ulink[@url]'):
        old_url = ulink.get('url')
        
        # URL formats: "ch0001#ch0001s0002" or "ch0001s0002" or "#ch0001s0002"
        for old_id, new_id in id_mapping.items():
            if old_id in old_url:
                new_url = old_url.replace(old_id, new_id)
                ulink.set('url', new_url)
                updated += 1
                logger.debug(f"Updated url: {old_url} -> {new_url}")
                break
    
    return updated


def renumber_xml_file(xml_path: Path) -> Tuple[int, int]:
    """
    Renumber sections in an XML file.
    
    Args:
        xml_path: Path to XML file
    
    Returns:
        Tuple of (sections_renumbered, references_updated)
    """
    logger.info(f"Processing {xml_path.name}")
    
    try:
        tree = etree.parse(str(xml_path))
        root = tree.getroot()
    except Exception as e:
        logger.error(f"Failed to parse {xml_path}: {e}")
        return 0, 0
    
    # Get all chapters (or single element if it's a chapter file)
    chapters = []
    if root.tag == 'chapter':
        chapters = [root]
    else:
        chapters = root.findall('.//chapter')
    
    if not chapters:
        logger.debug(f"No chapters found in {xml_path.name}")
        return 0, 0
    
    total_mapping = {}
    
    # Renumber sections in each chapter
    for chapter in chapters:
        chapter_mapping = renumber_sections_in_chapter(chapter)
        total_mapping.update(chapter_mapping)
    
    if not total_mapping:
        logger.debug(f"No sections to renumber in {xml_path.name}")
        return 0, 0
    
    # Update all references
    refs_updated = update_references(root, total_mapping)
    
    # Save the file
    try:
        tree.write(str(xml_path), encoding='utf-8', xml_declaration=True, pretty_print=True)
        logger.info(f"[OK] Saved {xml_path.name}: {len(total_mapping)} sections renumbered, {refs_updated} references updated")
    except Exception as e:
        logger.error(f"Failed to write {xml_path}: {e}")
        return 0, 0
    
    return len(total_mapping), refs_updated


def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description='Renumber section IDs to match document order'
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
        xml_files = [f for f in xml_files if f.name.lower() != 'book.xml']
    
    if not xml_files:
        logger.warning(f"No XML files found in {path}")
        sys.exit(0)
    
    logger.info(f"Found {len(xml_files)} XML file(s)")
    
    total_sections = 0
    total_refs = 0
    
    for xml_file in sorted(xml_files):
        sections, refs = renumber_xml_file(xml_file)
        total_sections += sections
        total_refs += refs
    
    print("\n" + "="*60)
    print(f"[OK] Renumbering complete:")
    print(f"  Sections renumbered: {total_sections}")
    print(f"  References updated: {total_refs}")
    print(f"  Files processed: {len(xml_files)}")
    print("="*60)


if __name__ == '__main__':
    main()
