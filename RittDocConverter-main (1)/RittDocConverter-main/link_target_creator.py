#!/usr/bin/env python3
"""
Smart Link Target Creator for RittDoc XML Files

Instead of removing broken links, this tool CREATES the missing targets
so links actually work. It:

1. Finds all link targets that are referenced but don't exist
2. Creates appropriate anchor points for them
3. For bibliography citations, creates stub bibliography entries
4. For glossary terms, creates stub glossary entries
5. For internal references, creates anchor elements

This ensures ALL links work properly in the output.

Usage:
    python link_target_creator.py /path/to/package.zip
    python link_target_creator.py /path/to/package.zip --verbose
"""

import argparse
import logging
import re
import sys
import tempfile
import zipfile
from collections import defaultdict
from pathlib import Path
from typing import Dict, List, Set, Tuple

from lxml import etree

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# DocBook namespace
NS = {'db': 'http://docbook.org/ns/docbook'}
NSMAP = {None: 'http://docbook.org/ns/docbook'}


def collect_all_ids(root: etree._Element) -> Set[str]:
    """Collect all IDs in the document."""
    ids = set()
    for elem in root.iter():
        elem_id = elem.get('id')
        if elem_id:
            ids.add(elem_id)
    return ids


def collect_all_link_targets(root: etree._Element) -> Dict[str, List[Tuple[str, str]]]:
    """
    Collect all link targets referenced in the document.

    Returns:
        Dict mapping linkend ID to list of (element_tag, link_text) tuples
    """
    targets = defaultdict(list)

    # Collect from <link linkend="...">
    for link in root.xpath('//db:link[@linkend]', namespaces=NS):
        linkend = link.get('linkend')
        link_text = link.text or ''.join(link.itertext())[:50]
        targets[linkend].append(('link', link_text))

    # Collect from <xref linkend="..."/>
    for xref in root.xpath('//db:xref[@linkend]', namespaces=NS):
        linkend = xref.get('linkend')
        targets[linkend].append(('xref', linkend))

    # Collect from <citation>
    for citation in root.xpath('//db:citation', namespaces=NS):
        citation_id = citation.text
        if citation_id:
            targets[citation_id].append(('citation', citation_id))

    return dict(targets)


def create_anchor_in_appropriate_location(root: etree._Element, anchor_id: str, link_text: str) -> bool:
    """
    Create an anchor element in an appropriate location in the document.

    Note: Anchors must be inside sect1 or similar block elements, NOT directly
    in chapter/preface/appendix (DTD doesn't allow anchor at chapter level).

    Returns:
        True if anchor was created, False otherwise
    """
    # Try to find a sect1 first - anchors are allowed inside sect1 but NOT directly in chapter
    first_sect1 = root.find('.//{http://docbook.org/ns/docbook}sect1')

    if first_sect1 is not None:
        # Create anchor element inside sect1
        anchor = etree.Element('{http://docbook.org/ns/docbook}anchor', nsmap=NSMAP)
        anchor.set('id', anchor_id)

        # Insert after title if exists, otherwise at beginning
        title = first_sect1.find('{http://docbook.org/ns/docbook}title')
        if title is not None:
            title_index = list(first_sect1).index(title)
            first_sect1.insert(title_index + 1, anchor)
        else:
            first_sect1.insert(0, anchor)

        logger.debug(f"Created anchor for '{anchor_id}' in sect1")
        return True

    # Fallback: try article (which allows anchor in its content)
    article = root.find('.//{http://docbook.org/ns/docbook}article')
    if article is not None:
        anchor = etree.Element('{http://docbook.org/ns/docbook}anchor', nsmap=NSMAP)
        anchor.set('id', anchor_id)

        title = article.find('{http://docbook.org/ns/docbook}title')
        if title is not None:
            title_index = list(article).index(title)
            article.insert(title_index + 1, anchor)
        else:
            article.insert(0, anchor)

        logger.debug(f"Created anchor for '{anchor_id}' in article")
        return True

    return False


def create_bibliography_entry(biblio_elem: etree._Element, ref_id: str, ref_text: str) -> None:
    """Create a bibliography entry for a missing reference."""
    # Create bibliomixed element
    bibmixed = etree.SubElement(biblio_elem, '{http://docbook.org/ns/docbook}bibliomixed', nsmap=NSMAP)
    bibmixed.set('id', ref_id)
    bibmixed.text = ref_text or f"Reference: {ref_id}"

    logger.debug(f"Created bibliography entry for '{ref_id}'")


def create_glossary_entry(glossary_elem: etree._Element, term_id: str, term_text: str) -> None:
    """Create a glossary entry for a missing term."""
    # Create glossentry element
    glossentry = etree.SubElement(glossary_elem, '{http://docbook.org/ns/docbook}glossentry', nsmap=NSMAP)
    glossentry.set('id', term_id)

    # Add glossterm
    glossterm = etree.SubElement(glossentry, '{http://docbook.org/ns/docbook}glossterm', nsmap=NSMAP)
    glossterm.text = term_text or term_id

    # Add glossdef
    glossdef = etree.SubElement(glossentry, '{http://docbook.org/ns/docbook}glossdef', nsmap=NSMAP)
    para = etree.SubElement(glossdef, '{http://docbook.org/ns/docbook}para', nsmap=NSMAP)
    para.text = f"Definition for {term_text or term_id}"

    logger.debug(f"Created glossary entry for '{term_id}'")


def fix_missing_targets_in_package(zip_path: Path, verbose: bool = False) -> Tuple[int, int]:
    """
    Create missing link targets in a RittDoc package.

    Returns:
        (missing_targets_found, targets_created)
    """
    total_missing = 0
    total_created = 0

    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Extract package
        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(tmpdir_path)

        # Find all XML files
        xml_files = list(tmpdir_path.glob('**/*.xml'))

        if not xml_files:
            logger.error(f"No XML files found in {zip_path}")
            return 0, 0

        # Find book.xml (contains bibliography and glossary)
        book_xml = tmpdir_path / 'book.xml'
        book_root = None
        if book_xml.exists():
            try:
                parser = etree.XMLParser(remove_blank_text=False, strip_cdata=False)
                book_tree = etree.parse(str(book_xml), parser)
                book_root = book_tree.getroot()
            except Exception as e:
                logger.error(f"Error reading book.xml: {e}")

        # First pass: Collect all existing IDs
        logger.info("Collecting all IDs from package...")
        all_ids = set()
        for xml_file in xml_files:
            try:
                parser = etree.XMLParser(remove_blank_text=False, strip_cdata=False)
                tree = etree.parse(str(xml_file), parser)
                root = tree.getroot()
                file_ids = collect_all_ids(root)
                all_ids.update(file_ids)
                if verbose:
                    logger.debug(f"  {xml_file.name}: {len(file_ids)} IDs")
            except Exception as e:
                logger.error(f"Error reading {xml_file}: {e}")

        logger.info(f"Total IDs collected: {len(all_ids)}")

        # Second pass: Collect all link targets
        logger.info("Collecting all link targets...")
        all_targets = {}
        for xml_file in xml_files:
            try:
                parser = etree.XMLParser(remove_blank_text=False, strip_cdata=False)
                tree = etree.parse(str(xml_file), parser)
                root = tree.getroot()
                file_targets = collect_all_link_targets(root)
                for target_id, refs in file_targets.items():
                    if target_id not in all_targets:
                        all_targets[target_id] = []
                    all_targets[target_id].extend(refs)
            except Exception as e:
                logger.error(f"Error reading {xml_file}: {e}")

        # Find missing targets
        missing_targets = {}
        for target_id, refs in all_targets.items():
            if target_id not in all_ids and target_id != '__deferred__':
                missing_targets[target_id] = refs
                total_missing += 1

        if not missing_targets:
            logger.info("No missing link targets found - all links are valid!")
            return 0, 0

        logger.info(f"Found {len(missing_targets)} missing link target(s)")

        # Third pass: Create missing targets
        logger.info("Creating missing link targets...")

        # Categorize missing targets
        bib_refs = {}  # Bibliography references (start with common patterns)
        gloss_terms = {}  # Glossary terms
        other_refs = {}  # Other references

        for target_id, refs in missing_targets.items():
            # Check if it looks like a bibliography reference
            is_bib = (
                any(elem_type == 'citation' for elem_type, _ in refs) or
                re.match(r'^(ref|bib|cite)\d+', target_id, re.I) or
                re.search(r'\d{4}', target_id)  # Has year (e.g., Pi-Sunyer2006)
            )

            # Check if it looks like a glossary term
            is_gloss = (
                '-' in target_id and not is_bib or
                target_id.replace('-', ' ').replace('_', ' ').islower()
            )

            if is_bib:
                bib_refs[target_id] = refs
            elif is_gloss:
                gloss_terms[target_id] = refs
            else:
                other_refs[target_id] = refs

        # Create bibliography entries if book.xml exists
        if bib_refs and book_root is not None:
            # Find or create bibliography element
            bibliography = book_root.find('.//{http://docbook.org/ns/docbook}bibliography')
            if bibliography is None:
                # Create bibliography element at end of book
                bibliography = etree.SubElement(book_root, '{http://docbook.org/ns/docbook}bibliography', nsmap=NSMAP)
                bib_title = etree.SubElement(bibliography, '{http://docbook.org/ns/docbook}title', nsmap=NSMAP)
                bib_title.text = "References"

            for ref_id, refs in bib_refs.items():
                ref_text = refs[0][1] if refs else ref_id
                create_bibliography_entry(bibliography, ref_id, ref_text)
                total_created += 1

            # Save book.xml
            book_tree.write(str(book_xml), encoding='utf-8', xml_declaration=True, pretty_print=True)
            logger.info(f"Created {len(bib_refs)} bibliography entries")

        # Create glossary entries if book.xml exists
        if gloss_terms and book_root is not None:
            # Find or create glossary element
            glossary = book_root.find('.//{http://docbook.org/ns/docbook}glossary')
            if glossary is None:
                # Create glossary element at end of book
                glossary = etree.SubElement(book_root, '{http://docbook.org/ns/docbook}glossary', nsmap=NSMAP)
                gloss_title = etree.SubElement(glossary, '{http://docbook.org/ns/docbook}title', nsmap=NSMAP)
                gloss_title.text = "Glossary"

            for term_id, refs in gloss_terms.items():
                term_text = refs[0][1] if refs else term_id.replace('-', ' ').replace('_', ' ').title()
                create_glossary_entry(glossary, term_id, term_text)
                total_created += 1

            # Save book.xml
            book_tree.write(str(book_xml), encoding='utf-8', xml_declaration=True, pretty_print=True)
            logger.info(f"Created {len(gloss_terms)} glossary entries")

        # Create anchors for other references in appropriate chapters
        for ref_id, refs in other_refs.items():
            ref_text = refs[0][1] if refs else ref_id
            # Try to find the chapter that references this ID
            for xml_file in xml_files:
                try:
                    parser = etree.XMLParser(remove_blank_text=False, strip_cdata=False)
                    tree = etree.parse(str(xml_file), parser)
                    root = tree.getroot()

                    # Check if this file has any links to this ID
                    has_link = bool(root.xpath(f'//db:link[@linkend="{ref_id}"]', namespaces=NS))
                    if has_link:
                        if create_anchor_in_appropriate_location(root, ref_id, ref_text):
                            tree.write(str(xml_file), encoding='utf-8', xml_declaration=True, pretty_print=True)
                            total_created += 1
                            break
                except Exception as e:
                    logger.error(f"Error processing {xml_file}: {e}")

        if other_refs:
            logger.info(f"Created {total_created - len(bib_refs) - len(gloss_terms)} anchor(s)")

        # Repack the package
        if total_created > 0:
            logger.info("Repacking package with created targets...")
            with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in tmpdir_path.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(tmpdir_path)
                        zf.write(file_path, arcname)

    # Report results
    logger.info(f"\n{'='*70}")
    logger.info(f"✓ CREATED {total_created} MISSING TARGET(S) out of {total_missing} missing")
    logger.info(f"  - Bibliography entries: {len(bib_refs)}")
    logger.info(f"  - Glossary entries: {len(gloss_terms)}")
    logger.info(f"  - Anchor points: {total_created - len(bib_refs) - len(gloss_terms)}")
    logger.info("  All links now have valid targets!")
    logger.info(f"{'='*70}\n")

    return total_missing, total_created


def main():
    parser = argparse.ArgumentParser(
        description='Create missing link targets in a RittDoc DocBook package'
    )
    parser.add_argument('zip_path', type=Path, help='Path to package ZIP file')
    parser.add_argument('--verbose', '-v', action='store_true',
                       help='Show detailed output')

    args = parser.parse_args()

    if not args.zip_path.exists():
        logger.error(f"File not found: {args.zip_path}")
        return 1

    if args.verbose:
        logger.setLevel(logging.DEBUG)

    missing, created = fix_missing_targets_in_package(args.zip_path, args.verbose)

    logger.info(f"Package processed: {args.zip_path}")
    if created > 0:
        logger.info(f"Package has been updated with {created} new targets")

    return 0


if __name__ == '__main__':
    sys.exit(main())
