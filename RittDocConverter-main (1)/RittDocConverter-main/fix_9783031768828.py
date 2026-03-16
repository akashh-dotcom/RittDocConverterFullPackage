#!/usr/bin/env python3
"""Post-QA XML Fixer for ISBN 9783031768828 (Pediatric Surgical Oncology)

Fixes issues identified in QA report dated 2026-03-09.
Operates on a copy of converted XML files.

Phase 1: Text/structural fixes (no source files needed)
  - C-006: Double periods before DOI URLs
  - C-011: Missing spaces around <emphasis> tags
  - C-013: Period-space in DOIs/abbreviations
  - C-014: ContactOf Author markers
  - C-017: Ordered list misclassification (itemizedlist → orderedlist)

Phase 2: Source-aware fixes (needs source XHTML)
  - C-012/C-023: Reconstruct bibliography links
  - C-025: Reconstruct footnotes from source
  - C-016: Recover BibSection content
  - C-026: Fix keyword separators
"""

import os
import re
import sys
import glob
import logging
from pathlib import Path
from lxml import etree
from copy import deepcopy
from collections import defaultdict

# Try to import BeautifulSoup for source parsing
try:
    from bs4 import BeautifulSoup
    HAS_BS4 = True
except ImportError:
    HAS_BS4 = False

logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
logger = logging.getLogger(__name__)

# =============================================================================
# CONFIGURATION
# =============================================================================

ISBN = '9783031768828'
CONVERTED_DIR = f'/Users/jagadishcu/Downloads/9783031768828_xml_fixed/{ISBN}/'
SOURCE_DIR = '/tmp/book_qa/source/OEBPS/html/'
SOURCE_NCX = '/tmp/book_qa/source/OEBPS/toc.ncx'

# Leading number pattern for ordered list stripping
LEADING_NUMBER_RE = re.compile(
    r'^[\s]*(\d+[\.\):]|\(\d+\)|\d+\.\d+[\.\)]?|[a-zA-Z][\.\)]|\([a-zA-Z]\)|[ivxIVX]+[\.\)])\s*'
)

# =============================================================================
# PHASE 1: TEXT AND STRUCTURAL FIXES
# =============================================================================

class FixStats:
    """Track fix statistics."""
    def __init__(self):
        self.counts = defaultdict(int)
        self.file_counts = defaultdict(set)

    def record(self, fix_id, filename):
        self.counts[fix_id] += 1
        self.file_counts[fix_id].add(filename)

    def summary(self):
        lines = []
        for fix_id in sorted(self.counts.keys()):
            lines.append(f"  {fix_id}: {self.counts[fix_id]} fixes in {len(self.file_counts[fix_id])} files")
        return '\n'.join(lines)


def fix_double_periods(xml_text, filename, stats):
    """C-006: Fix double periods before DOI URLs."""
    # Pattern: text ending with period, then another period before URL
    # "..53.. https://doi.org/..." → "..53. https://doi.org/..."
    pattern = re.compile(r'(?<!\.)\.\.(\s*https?://)')
    new_text = pattern.sub(r'.\1', xml_text)
    count = len(pattern.findall(xml_text))
    if count > 0:
        for _ in range(count):
            stats.record('C-006', filename)
    return new_text


def fix_doi_spacing(xml_text, filename, stats):
    """C-013: Fix 'doi. org' → 'doi.org' and other DOI/abbreviation spacing."""
    fixes = [
        (re.compile(r'doi\.\s+org'), 'doi.org'),
        (re.compile(r'\be\.\s+g\.'), 'e.g.'),
        (re.compile(r'\bi\.\s+e\.'), 'i.e.'),
        (re.compile(r'\ba\.\s+k\.\s+a\.'), 'a.k.a.'),
        (re.compile(r'\bvs\.\s+'), 'vs. '),  # keep single space after vs.
    ]
    for pattern, replacement in fixes:
        matches = pattern.findall(xml_text)
        if matches:
            xml_text = pattern.sub(replacement, xml_text)
            for _ in matches:
                stats.record('C-013', filename)
    return xml_text


def fix_contact_of_author(tree, filename, stats):
    """C-014: Remove 'ContactOf Author N' text from emphasis in superscript."""
    root = tree.getroot()
    elements_to_remove = []

    for emphasis in root.iter('emphasis'):
        text = (emphasis.text or '').strip()
        if re.match(r'^ContactOf\s+Author\s*\d*$', text):
            parent = emphasis.getparent()
            if parent is not None:
                # Remove the emphasis element, preserve surrounding text
                # The emphasis is usually inside <superscript>
                idx = list(parent).index(emphasis)
                # Move tail to previous sibling or parent text
                if emphasis.tail:
                    if idx > 0:
                        prev = parent[idx - 1]
                        prev.tail = (prev.tail or '') + emphasis.tail
                    else:
                        parent.text = (parent.text or '') + emphasis.tail
                elements_to_remove.append((parent, emphasis))
                stats.record('C-014', filename)

    for parent, elem in elements_to_remove:
        parent.remove(elem)


def fix_emphasis_spacing(tree, filename, stats):
    """C-011: Add missing spaces before/after <emphasis> elements."""
    root = tree.getroot()

    for emphasis in root.iter('emphasis'):
        parent = emphasis.getparent()
        if parent is None:
            continue

        idx = list(parent).index(emphasis)

        # Fix missing space BEFORE emphasis (previous sibling's tail or parent's text)
        if idx > 0:
            prev = parent[idx - 1]
            if prev.tail and prev.tail[-1:].isalnum() and (emphasis.text or '')[0:1].isalnum():
                prev.tail = prev.tail + ' '
                stats.record('C-011', filename)
        elif parent.text and parent.text[-1:].isalnum() and (emphasis.text or '')[0:1].isalnum():
            parent.text = parent.text + ' '
            stats.record('C-011', filename)

        # Fix missing space AFTER emphasis (emphasis's tail)
        if emphasis.tail and emphasis.tail[0:1].isalnum():
            # Check last char inside emphasis
            last_text = emphasis.text or ''
            # Also check last child's tail
            if len(emphasis) > 0:
                last_child = emphasis[-1]
                last_text = last_child.tail or last_child.text or ''
            if last_text and last_text[-1:].isalnum():
                emphasis.tail = ' ' + emphasis.tail
                stats.record('C-011', filename)


def fix_ordered_lists(tree, filename, stats):
    """C-017: Convert itemizedlist mark='none' with numbered items to orderedlist."""
    root = tree.getroot()
    changes = []

    for itemizedlist in root.iter('itemizedlist'):
        mark = itemizedlist.get('mark', '')
        if mark != 'none':
            continue

        # Check if items start with numbers
        listitems = itemizedlist.findall('listitem')
        if not listitems:
            continue

        numbered_count = 0
        items_checked = min(3, len(listitems))
        for li in listitems[:items_checked]:
            para = li.find('para')
            if para is not None:
                text = (para.text or '').strip()
                if LEADING_NUMBER_RE.match(text):
                    numbered_count += 1

        # If majority of checked items have numbering, convert to orderedlist
        if numbered_count >= items_checked / 2 and numbered_count > 0:
            changes.append(itemizedlist)

    for itemizedlist in changes:
        parent = itemizedlist.getparent()
        if parent is None:
            continue

        # Create new orderedlist element
        orderedlist = etree.Element('orderedlist')
        # Copy attributes except mark and role="none"
        for attr, val in itemizedlist.attrib.items():
            if attr == 'mark':
                continue
            if attr == 'role' and val == 'none':
                continue
            orderedlist.set(attr, val)

        # Move all children
        for child in list(itemizedlist):
            orderedlist.append(child)

        # Strip leading numbers from listitem paras
        for li in orderedlist.findall('listitem'):
            para = li.find('para')
            if para is not None and para.text:
                new_text = LEADING_NUMBER_RE.sub('', para.text, count=1)
                if new_text != para.text:
                    para.text = new_text

        # Preserve tail
        orderedlist.tail = itemizedlist.tail

        # Replace in parent
        idx = list(parent).index(itemizedlist)
        parent.remove(itemizedlist)
        parent.insert(idx, orderedlist)
        stats.record('C-017', filename)


def fix_zero_width_spaces_in_urls(xml_text, filename, stats):
    """Fix zero-width spaces (U+200B) in DOI URLs within ulink url attributes."""
    # Pattern: url="https://doi.​org/..." with zero-width spaces
    zwsp = '\u200b'
    if zwsp in xml_text:
        # Only remove ZWSP from url="..." attributes and DOI text
        # Replace ZWSP in url attributes
        def clean_url_attr(m):
            url = m.group(1).replace(zwsp, '')
            return f'url="{url}"'
        new_text = re.sub(r'url="([^"]*)"', clean_url_attr, xml_text)

        # Also remove ZWSP from DOI display text within ulink elements
        # Pattern: >https://​doi.​org/​... </ulink>
        def clean_doi_display(m):
            return m.group(0).replace(zwsp, '')
        new_text = re.sub(r'>https?://[^<]*</ulink>', clean_doi_display, new_text)

        if new_text != xml_text:
            stats.record('ZWSP-cleanup', filename)
            xml_text = new_text
    return xml_text


# =============================================================================
# PHASE 2: SOURCE-AWARE FIXES
# =============================================================================

def build_source_chapter_map():
    """Build mapping from source chapter files to converted chapter numbers.

    Parses toc.ncx to get chapter ordering. The first body chapter maps to ch0004,
    incrementing for each subsequent chapter.
    """
    if not os.path.exists(SOURCE_NCX):
        logger.warning(f"Source NCX not found at {SOURCE_NCX}, skipping source-aware fixes")
        return {}

    tree = etree.parse(SOURCE_NCX)
    root = tree.getroot()
    ns = {'ncx': 'http://www.daisy.org/z3986/2005/ncx/'}

    chapter_map = {}  # source_filename → converted_ch_number (e.g., 'ch0004')
    chapter_counter = 4  # First body chapter is ch0004

    # Walk all navPoints looking for chapter files
    for navpoint in root.iter('{http://www.daisy.org/z3986/2005/ncx/}navPoint'):
        content = navpoint.find('ncx:content', ns)
        if content is None:
            continue
        src = content.get('src', '')

        # Only map top-level chapter files (not section anchors within chapters)
        if '_Chapter.xhtml' in src and '#' not in src:
            filename = os.path.basename(src)
            ch_id = f'ch{chapter_counter:04d}'
            chapter_map[filename] = ch_id
            chapter_counter += 1

    logger.info(f"Built source→converted mapping: {len(chapter_map)} chapters")
    return chapter_map


def fix_bibliography_links(tree, filename, stats):
    """C-012/C-023: Fix concatenated PubMed/Crossref labels in bibliography.

    Approach: Parse each bibliomixed entry. Find concatenated label patterns
    and separate them. Also wrap DOI URLs in <ulink> if not already wrapped.
    """
    root = tree.getroot()
    nsmap = root.nsmap if hasattr(root, 'nsmap') else {}

    for bib in root.iter('bibliomixed'):
        # Get all text content (including tails of children)
        full_text = etree.tostring(bib, method='text', encoding='unicode')

        # Check for concatenated labels at end of text
        # Patterns: CrossrefPubMed, CrossrefPubMedPubMed Central, PubMed, etc.
        label_pattern = re.compile(
            r'\s*(Crossref|CrossRef)(PubMed(?:\s*Central)?)?(?:\s*)(PubMed(?:\s*Central)?)?$'
        )

        # Work on the last text node of bibliomixed
        last_text_holder = bib
        if len(bib) > 0:
            last_child = bib[-1]
            if last_child.tail:
                last_text_holder = None  # We'll work with last_child.tail
                text_to_check = last_child.tail
            else:
                text_to_check = bib.text or ''
        else:
            text_to_check = bib.text or ''

        if not text_to_check:
            continue

        # Find and fix concatenated labels
        # Pattern 1: "CrossrefPubMed" → " Crossref | PubMed"
        # Pattern 2: "CrossrefPubMedPubMed Central" → " Crossref | PubMed | PubMed Central"
        # Pattern 3: "PubMed" (standalone, at end)
        replacements = [
            ('CrossrefPubMedPubMed Central', ' Crossref | PubMed | PubMed Central'),
            ('CrossRefPubMedPubMed Central', ' Crossref | PubMed | PubMed Central'),
            ('CrossrefPubMed Central', ' Crossref | PubMed Central'),
            ('CrossRefPubMed Central', ' Crossref | PubMed Central'),
            ('CrossrefPubMed', ' Crossref | PubMed'),
            ('CrossRefPubMed', ' Crossref | PubMed'),
            ('PubMedPubMed Central', ' PubMed | PubMed Central'),
            ('PubMedCentral', ' PubMed Central'),
        ]

        modified = False
        for old, new in replacements:
            if old in text_to_check:
                text_to_check = text_to_check.replace(old, new)
                modified = True
                break

        if modified:
            if last_text_holder is None and len(bib) > 0:
                bib[-1].tail = text_to_check
            else:
                bib.text = text_to_check
            stats.record('C-012/C-023', filename)

        # Also fix DOI URL display text: wrap bare DOI URLs in ulink if not wrapped
        # The DOI URLs are already in the text, just not wrapped in <ulink>
        # This is harder to do via lxml text manipulation, so we skip inline ulink creation
        # and just fix the label separation above.


def fix_keywords_from_source(tree, filename, ch_id, chapter_map, stats):
    """C-026: Fix keyword separators using source data."""
    root = tree.getroot()

    # Only process abstract/keyword section files (typically s0002)
    if 's0002' not in filename:
        return

    # Find Keywords title
    for title in root.iter('title'):
        if (title.text or '').strip() == 'Keywords':
            # Found keyword section — get the para after it
            parent = title.getparent()
            if parent is None:
                continue
            idx = list(parent).index(title)
            if idx + 1 < len(parent):
                next_elem = parent[idx + 1]
                if next_elem.tag == 'para' and next_elem.text:
                    # Check if already has commas
                    if ', ' in next_elem.text:
                        continue

                    # Try to get keywords from source
                    source_keywords = _get_source_keywords(ch_id, chapter_map)
                    if source_keywords:
                        next_elem.text = ', '.join(source_keywords)
                        stats.record('C-026', filename)
                    # If no source, we can't reliably split the text
                    break


def _get_source_keywords(ch_id, chapter_map):
    """Extract keywords from source XHTML for given chapter."""
    if not HAS_BS4:
        return []

    # Find source file for this chapter
    source_file = None
    for src_filename, mapped_ch_id in chapter_map.items():
        if mapped_ch_id == ch_id:
            source_file = os.path.join(SOURCE_DIR, src_filename)
            break

    if not source_file or not os.path.exists(source_file):
        return []

    try:
        with open(source_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')

        keyword_group = soup.find('div', class_='KeywordGroup')
        if not keyword_group:
            return []

        keywords = []
        for span in keyword_group.find_all('span', class_='Keyword'):
            kw = span.get_text(strip=True)
            if kw:
                keywords.append(kw)
        return keywords
    except Exception as e:
        logger.warning(f"Error reading source keywords for {ch_id}: {e}")
        return []


def fix_footnotes_from_source(ch_id, chapter_map, stats):
    """C-025: Reconstruct footnotes from source XHTML.

    Algorithm:
    1. Read source chapter for footnote section data
    2. For each footnote, find the matching <superscript>N</superscript> in converted XML
    3. Create <footnote> element inline at that position
    4. Remove any consolidated footnote section at the end
    """
    if not HAS_BS4:
        logger.warning("BeautifulSoup not available, skipping footnote reconstruction")
        return

    # Find source file
    source_file = None
    for src_filename, mapped_ch_id in chapter_map.items():
        if mapped_ch_id == ch_id:
            source_file = os.path.join(SOURCE_DIR, src_filename)
            break

    if not source_file or not os.path.exists(source_file):
        return

    # Parse source for footnotes
    try:
        with open(source_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')
    except Exception as e:
        logger.warning(f"Error reading source {source_file}: {e}")
        return

    fn_section = soup.find('aside', class_='FootnoteSection')
    if not fn_section:
        fn_section = soup.find(attrs={'epub:type': 'footnotes'})
    if not fn_section:
        return

    # Extract individual footnotes
    fn_divs = fn_section.find_all('div', class_='Footnote')
    if not fn_divs:
        return

    footnotes = []  # (number, text_content)
    for fn_div in fn_divs:
        num_span = fn_div.find(class_='FootnoteNumber')
        content_div = fn_div.find(class_='FootnoteContent')
        if not num_span or not content_div:
            continue
        fn_num = num_span.get_text(strip=True)
        # Get text content from all paragraphs
        paragraphs = content_div.find_all('p')
        fn_texts = []
        for p in paragraphs:
            fn_texts.append(p.get_text(strip=True))
        if fn_texts:
            footnotes.append((fn_num, fn_texts))

    if not footnotes:
        return

    logger.info(f"  Found {len(footnotes)} footnotes in source for {ch_id}")

    # Find all converted sect1 files for this chapter
    pattern = os.path.join(CONVERTED_DIR, f'sect1.{ISBN}.{ch_id}s*.xml')
    sect_files = sorted(glob.glob(pattern))

    for sect_file in sect_files:
        sect_filename = os.path.basename(sect_file)
        try:
            parser = etree.XMLParser(remove_blank_text=False)
            tree = etree.parse(sect_file, parser)
            root = tree.getroot()
        except Exception as e:
            logger.warning(f"Error parsing {sect_filename}: {e}")
            continue

        modified = False

        for fn_num, fn_texts in footnotes:
            # Find <superscript>N</superscript> with matching number
            for sup in root.iter('superscript'):
                sup_text = (sup.text or '').strip()
                if sup_text == fn_num and len(sup) == 0:
                    sup_parent = sup.getparent()
                    if sup_parent is None or sup_parent.tag not in ('para', 'simpara'):
                        continue

                    # Create footnote element
                    # Extract section ID from filename for footnote ID
                    sect_match = re.search(r'(ch\d+s\d+)', sect_filename)
                    sect_id = sect_match.group(1) if sect_match else ch_id
                    fn_id = f'{sect_id}fn{int(fn_num):04d}'

                    footnote_elem = etree.Element('footnote')
                    footnote_elem.set('id', fn_id)

                    for text in fn_texts:
                        para = etree.SubElement(footnote_elem, 'para')
                        para.text = text

                    # Replace superscript with footnote
                    footnote_elem.tail = sup.tail
                    sup_index = list(sup_parent).index(sup)
                    sup_parent.remove(sup)
                    sup_parent.insert(sup_index, footnote_elem)
                    modified = True
                    stats.record('C-025', sect_filename)
                    break  # Only replace first match per footnote number

        if modified:
            # Write back
            tree.write(sect_file, xml_declaration=True, encoding='UTF-8',
                       pretty_print=False)


def fix_bibsection_from_source(ch_id, chapter_map, stats):
    """C-016: Recover BibSection (Recommended/Further Reading) from source.

    Finds the source chapter's BibSection content and creates a <bibliodiv>
    element in the converted bibliography file.
    """
    if not HAS_BS4:
        return

    # Find source file
    source_file = None
    for src_filename, mapped_ch_id in chapter_map.items():
        if mapped_ch_id == ch_id:
            source_file = os.path.join(SOURCE_DIR, src_filename)
            break

    if not source_file or not os.path.exists(source_file):
        return

    try:
        with open(source_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')
    except Exception as e:
        logger.warning(f"Error reading source {source_file}: {e}")
        return

    bib_sections = soup.find_all('div', class_='BibSection')
    if not bib_sections:
        return

    # Find the converted bibliography file for this chapter
    # It's typically the last sect1 file (or second-to-last before copyright)
    pattern = os.path.join(CONVERTED_DIR, f'sect1.{ISBN}.{ch_id}s*.xml')
    sect_files = sorted(glob.glob(pattern))

    # Find the file containing <bibliography>
    bib_file = None
    for sf in sect_files:
        try:
            with open(sf, 'r', encoding='utf-8') as f:
                content = f.read()
            if '<bibliography' in content:
                bib_file = sf
                break
        except:
            continue

    if not bib_file:
        logger.warning(f"No bibliography file found for {ch_id}")
        return

    try:
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(bib_file, parser)
        root = tree.getroot()
    except Exception as e:
        logger.warning(f"Error parsing {bib_file}: {e}")
        return

    # Find the <bibliography> element
    bib_elem = root.find('.//bibliography')
    if bib_elem is None:
        # Try finding it at any depth
        for elem in root.iter('bibliography'):
            bib_elem = elem
            break
    if bib_elem is None:
        return

    for bib_section in bib_sections:
        heading = bib_section.find(class_='Heading')
        section_title = heading.get_text(strip=True) if heading else 'Recommended Reading'

        # Get citations from BibSection
        citations = bib_section.find_all('li', class_='Citation')
        if not citations:
            continue

        # Create <bibliodiv> element
        bibliodiv = etree.SubElement(bib_elem, 'bibliodiv')
        title = etree.SubElement(bibliodiv, 'title')
        title.text = section_title

        for citation in citations:
            content_div = citation.find(class_='CitationContent')
            if not content_div:
                continue
            # Extract text content
            cite_text = content_div.get_text(strip=True)
            if cite_text:
                bibliomixed = etree.SubElement(bibliodiv, 'bibliomixed')
                cite_id = content_div.get('id', '')
                if cite_id:
                    sect_match = re.search(r'(ch\d+s\d+)', os.path.basename(bib_file))
                    sect_id = sect_match.group(1) if sect_match else ch_id
                    bibliomixed.set('id', f'{sect_id}bib{cite_id}')
                bibliomixed.text = cite_text

        stats.record('C-016', os.path.basename(bib_file))
        logger.info(f"  Recovered BibSection '{section_title}' with {len(citations)} entries for {ch_id}")

    # Write back
    tree.write(bib_file, xml_declaration=True, encoding='UTF-8', pretty_print=False)


# =============================================================================
# MAIN PIPELINE
# =============================================================================

def process_file(filepath, stats, chapter_map=None):
    """Apply all fixes to a single XML file."""
    filename = os.path.basename(filepath)

    # Read raw XML text
    with open(filepath, 'r', encoding='utf-8') as f:
        xml_text = f.read()

    original_text = xml_text

    # Phase 1a: Text-level fixes (operate on raw XML string)
    xml_text = fix_double_periods(xml_text, filename, stats)
    xml_text = fix_doi_spacing(xml_text, filename, stats)
    xml_text = fix_zero_width_spaces_in_urls(xml_text, filename, stats)

    # Write back if text changed (before tree parsing)
    if xml_text != original_text:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(xml_text)

    # Phase 1b: Tree-level fixes (operate on parsed XML)
    try:
        parser = etree.XMLParser(remove_blank_text=False)
        tree = etree.parse(filepath, parser)
    except etree.XMLSyntaxError as e:
        logger.error(f"XML parse error in {filename}: {e}")
        return

    tree_modified = False

    # C-014: Remove ContactOf Author markers
    before = etree.tostring(tree.getroot(), encoding='unicode')
    fix_contact_of_author(tree, filename, stats)
    if etree.tostring(tree.getroot(), encoding='unicode') != before:
        tree_modified = True

    # C-011: Fix emphasis spacing
    before = etree.tostring(tree.getroot(), encoding='unicode')
    fix_emphasis_spacing(tree, filename, stats)
    if etree.tostring(tree.getroot(), encoding='unicode') != before:
        tree_modified = True

    # C-017: Fix ordered list misclassification
    before = etree.tostring(tree.getroot(), encoding='unicode')
    fix_ordered_lists(tree, filename, stats)
    if etree.tostring(tree.getroot(), encoding='unicode') != before:
        tree_modified = True

    # C-012/C-023: Fix bibliography link concatenation
    before = etree.tostring(tree.getroot(), encoding='unicode')
    fix_bibliography_links(tree, filename, stats)
    if etree.tostring(tree.getroot(), encoding='unicode') != before:
        tree_modified = True

    # C-026: Fix keyword separators (needs source)
    if chapter_map:
        ch_match = re.search(r'(ch\d+)', filename)
        if ch_match:
            ch_id = ch_match.group(1)
            before = etree.tostring(tree.getroot(), encoding='unicode')
            fix_keywords_from_source(tree, filename, ch_id, chapter_map, stats)
            if etree.tostring(tree.getroot(), encoding='unicode') != before:
                tree_modified = True

    # Write back tree if modified
    if tree_modified:
        tree.write(filepath, xml_declaration=True, encoding='UTF-8',
                   pretty_print=False)


def main():
    """Run all fixes."""
    logger.info(f"=" * 70)
    logger.info(f"Post-QA XML Fixer for ISBN {ISBN}")
    logger.info(f"Working directory: {CONVERTED_DIR}")
    logger.info(f"=" * 70)

    if not os.path.isdir(CONVERTED_DIR):
        logger.error(f"Converted directory not found: {CONVERTED_DIR}")
        sys.exit(1)

    stats = FixStats()

    # Build source-to-converted chapter mapping
    chapter_map = build_source_chapter_map()

    # Get all XML files
    xml_files = sorted(glob.glob(os.path.join(CONVERTED_DIR, '*.xml')))
    logger.info(f"Processing {len(xml_files)} XML files...")

    # Phase 1 + Phase 2 (per-file fixes)
    for i, filepath in enumerate(xml_files):
        if (i + 1) % 100 == 0:
            logger.info(f"  Progress: {i + 1}/{len(xml_files)} files...")
        process_file(filepath, stats, chapter_map)

    # Phase 2: Source-aware fixes that need cross-file operations
    if chapter_map:
        # C-025: Footnote reconstruction
        logger.info("\nPhase 2: Footnote reconstruction from source...")
        # Find chapters with footnotes in source
        for src_filename, ch_id in chapter_map.items():
            source_path = os.path.join(SOURCE_DIR, src_filename)
            if not os.path.exists(source_path):
                continue
            try:
                with open(source_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                if 'FootnoteSection' in content or 'epub:type="footnotes"' in content:
                    logger.info(f"  Processing footnotes for {ch_id} ({src_filename})")
                    fix_footnotes_from_source(ch_id, chapter_map, stats)
            except Exception as e:
                logger.warning(f"Error checking {src_filename}: {e}")

        # C-016: BibSection recovery
        logger.info("\nPhase 2: BibSection recovery from source...")
        for src_filename, ch_id in chapter_map.items():
            source_path = os.path.join(SOURCE_DIR, src_filename)
            if not os.path.exists(source_path):
                continue
            try:
                with open(source_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                if 'BibSection' in content:
                    logger.info(f"  Processing BibSection for {ch_id} ({src_filename})")
                    fix_bibsection_from_source(ch_id, chapter_map, stats)
            except Exception as e:
                logger.warning(f"Error checking {src_filename}: {e}")

    # Print summary
    logger.info(f"\n{'=' * 70}")
    logger.info(f"FIX SUMMARY")
    logger.info(f"{'=' * 70}")
    logger.info(stats.summary())
    logger.info(f"\nTotal fixes: {sum(stats.counts.values())}")
    logger.info(f"Total files modified: {len(set().union(*stats.file_counts.values()) if stats.file_counts else set())}")


if __name__ == '__main__':
    main()
