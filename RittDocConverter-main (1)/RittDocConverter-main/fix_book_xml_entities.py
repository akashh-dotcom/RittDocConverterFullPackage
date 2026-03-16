#!/usr/bin/env python3
"""
Fix Book.XML Entity Reference Errors

This script fixes the error: "Entity 'pr0001' not defined"

The issue occurs when Book.XML contains entity references like &pr0001; but
the entities are not declared in the DOCTYPE header.

Book.XML should have this structure:

<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  ...
]>
<book>
  &pr0001;
  &ch0001;
  ...
</book>

This script:
1. Scans Book.XML for all entity references (&xxx;)
2. Checks which entities are missing from DOCTYPE
3. Adds missing entity declarations
4. Optionally removes orphan references (entities without corresponding .xml files)
"""

import re
import sys
from pathlib import Path
from typing import Set, Tuple, List
import logging

logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
logger = logging.getLogger(__name__)


def find_entity_references(xml_content: str) -> Set[str]:
    """
    Find all entity references in XML content.
    
    Returns set of entity names (without & and ;)
    Example: &pr0001; -> 'pr0001'
    """
    # Pattern matches &entityname; where entityname is alphanumeric
    pattern = r'&([a-zA-Z0-9_]+);'
    matches = re.findall(pattern, xml_content)
    
    # Filter out standard XML entities
    standard_entities = {'lt', 'gt', 'amp', 'quot', 'apos'}
    entity_refs = {m for m in matches if m not in standard_entities}
    
    return entity_refs


def find_declared_entities(xml_content: str) -> Set[str]:
    """
    Find all entities declared in DOCTYPE.
    
    Returns set of entity names
    Example: <!ENTITY pr0001 SYSTEM "pr0001.xml"> -> 'pr0001'
    """
    # Pattern matches <!ENTITY name SYSTEM "file">
    pattern = r'<!ENTITY\s+([a-zA-Z0-9_]+)\s+SYSTEM\s+"[^"]*">'
    matches = re.findall(pattern, xml_content)
    return set(matches)


def extract_doctype_and_body(xml_content: str) -> Tuple[str, str, str, str]:
    """
    Extract DOCTYPE declaration and body separately.
    
    Returns:
        (before_doctype, doctype, after_doctype_before_root, body)
    """
    # Find DOCTYPE declaration
    doctype_match = re.search(
        r'(<!DOCTYPE\s+\w+[^>]*(?:\[[^\]]*\])?>)',
        xml_content,
        re.DOTALL
    )
    
    if not doctype_match:
        # No DOCTYPE found - content starts immediately
        xml_decl_match = re.search(r'(<\?xml[^>]*\?>)', xml_content)
        if xml_decl_match:
            before = xml_decl_match.group(0)
            after = xml_content[xml_decl_match.end():]
            return before, '', '', after
        else:
            return '', '', '', xml_content
    
    doctype = doctype_match.group(1)
    before_doctype = xml_content[:doctype_match.start()]
    after_doctype = xml_content[doctype_match.end():]
    
    # Split after_doctype into whitespace/comments and the actual body
    root_match = re.search(r'(<\w+[^>]*>)', after_doctype)
    if root_match:
        before_root = after_doctype[:root_match.start()]
        body = after_doctype[root_match.start():]
        return before_doctype, doctype, before_root, body
    else:
        return before_doctype, doctype, '', after_doctype


def build_doctype_with_entities(entities: List[str], 
                                public_id: str = "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN",
                                system_id: str = "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd",
                                root_element: str = "book") -> str:
    """
    Build a complete DOCTYPE declaration with entity declarations.
    
    Args:
        entities: List of entity names (e.g., ['pr0001', 'ch0001'])
        public_id: PUBLIC identifier
        system_id: SYSTEM identifier
        root_element: Root element name (usually 'book')
    
    Returns:
        Complete DOCTYPE string
    """
    if not entities:
        # No entities - simple DOCTYPE
        return f'<!DOCTYPE {root_element} PUBLIC "{public_id}" "{system_id}">'
    
    lines = []
    lines.append(f'<!DOCTYPE {root_element} PUBLIC "{public_id}" "{system_id}" [')
    
    # Add entity declarations in sorted order
    for entity in sorted(entities):
        lines.append(f'\t<!ENTITY {entity} SYSTEM "{entity}.xml">')
    
    lines.append(']>')
    
    return '\n'.join(lines)


def fix_book_xml_entities(book_xml_path: Path, remove_orphans: bool = False) -> Tuple[int, int]:
    """
    Fix entity reference errors in Book.XML.
    
    Args:
        book_xml_path: Path to Book.XML file
        remove_orphans: If True, remove entity references without corresponding .xml files
    
    Returns:
        Tuple of (entities_added, entities_removed)
    """
    logger.info(f"Processing {book_xml_path}")
    
    # Read the file
    try:
        with open(book_xml_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        logger.error(f"Failed to read {book_xml_path}: {e}")
        return 0, 0
    
    # Find all entity references in the body
    entity_refs = find_entity_references(content)
    logger.info(f"Found {len(entity_refs)} entity reference(s) in body: {sorted(entity_refs)}")
    
    # Find declared entities
    declared = find_declared_entities(content)
    logger.info(f"Found {len(declared)} declared entit(ies) in DOCTYPE: {sorted(declared)}")
    
    # Check for orphan entities (references without corresponding .xml files)
    removed_orphans = set()
    if remove_orphans:
        directory = book_xml_path.parent
        for entity in entity_refs:
            xml_file = directory / f"{entity}.xml"
            if not xml_file.exists():
                logger.warning(f"Entity &{entity}; has no corresponding file {entity}.xml")
                removed_orphans.add(entity)
        
        if removed_orphans:
            # Remove orphan references from content
            for orphan in removed_orphans:
                pattern = f'&{orphan};'
                content = content.replace(pattern, '')
                logger.info(f"Removed orphan entity reference &{orphan};")
            
            # Update entity_refs to exclude removed orphans
            entity_refs = entity_refs - removed_orphans
    
    # Find missing entities (after orphan removal)
    missing = entity_refs - declared
    
    if not missing and not removed_orphans:
        logger.info("[OK] All entity references are properly declared")
        return 0, 0
    
    if missing:
        logger.info(f"Missing {len(missing)} entit(ies): {sorted(missing)}")
    
    # Extract DOCTYPE and body
    before_doctype, old_doctype, before_root, body = extract_doctype_and_body(content)
    
    # Build new DOCTYPE with all required entities
    all_entities = sorted(entity_refs)
    new_doctype = build_doctype_with_entities(all_entities)
    
    # Reconstruct the file
    new_content = before_doctype + '\n' + new_doctype + before_root + body
    
    # Write back
    try:
        with open(book_xml_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        logger.info(f"[OK] Updated {book_xml_path}")
        logger.info(f"  Added {len(missing)} entit(ies)")
        if removed_orphans:
            logger.info(f"  Removed {len(removed_orphans)} orphan(s)")
    except Exception as e:
        logger.error(f"Failed to write {book_xml_path}: {e}")
        return 0, 0
    
    return len(missing), len(removed_orphans)


def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description='Fix entity reference errors in Book.XML'
    )
    parser.add_argument(
        'book_xml',
        type=str,
        help='Path to Book.XML file'
    )
    parser.add_argument(
        '--remove-orphans',
        action='store_true',
        help='Remove entity references without corresponding .xml files'
    )
    
    args = parser.parse_args()
    
    book_xml_path = Path(args.book_xml)
    if not book_xml_path.exists():
        logger.error(f"File not found: {book_xml_path}")
        sys.exit(1)
    
    if book_xml_path.name.lower() != 'book.xml':
        logger.warning(f"File name is '{book_xml_path.name}', expected 'Book.XML'")
    
    added, removed = fix_book_xml_entities(book_xml_path, args.remove_orphans)
    
    print("\n" + "="*60)
    if added > 0 or removed > 0:
        print(f"[OK] Fixed Book.XML:")
        if added > 0:
            print(f"  Added {added} entity declaration(s)")
        if removed > 0:
            print(f"  Removed {removed} orphan reference(s)")
    else:
        print("[OK] No changes needed - all entities properly declared")
    print("="*60)


if __name__ == '__main__':
    main()
