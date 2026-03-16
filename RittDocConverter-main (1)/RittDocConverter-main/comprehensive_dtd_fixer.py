#!/usr/bin/env python3
"""
Comprehensive DTD Fixer for RittDoc

ARCHITECTURE ROLE:
    This module is used for POST-PROCESSING on individual chapter files
    AFTER they have been written to disk (typically during the chunking phase).

    For IN-MEMORY validation during conversion, see:
    dtd_compliance.py (used in epub_to_structured_v2.py Phase 3)

USAGE:
    from comprehensive_dtd_fixer import ComprehensiveDTDFixer
    fixer = ComprehensiveDTDFixer(dtd_path)
    fixes_count, fix_list = fixer.fix_chapter_file(chapter_path, chapter_name)

This script applies comprehensive fixes to resolve ALL common DTD validation errors:
1. Invalid content models (wrap/reorder elements)
2. Missing required elements (add defaults)
3. Invalid/undeclared elements (remove or convert)
4. Empty elements (add minimal content or remove)
5. Missing required attributes (add defaults)
6. Invalid attribute values (fix or remove)

The fixer runs validation before and after fixes to show improvement.
"""

import logging
import re
import sys
import tempfile
import zipfile
from collections import defaultdict
from copy import deepcopy
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple

from lxml import etree

logger = logging.getLogger(__name__)
from id_authority import (
    MAX_ID_LENGTH, next_available_sect1_id,
    ELEMENT_CODES, get_element_code, DEFAULT_ELEMENT_CODE,
)

try:
    from validation_report import (ValidationError, ValidationReportGenerator,
                                   VerificationItem)
    VALIDATION_REPORT_AVAILABLE = True
except ImportError:
    VALIDATION_REPORT_AVAILABLE = False

try:
    from validate_with_entity_tracking import EntityTrackingValidator
    VALIDATION_AVAILABLE = True
except ImportError:
    VALIDATION_AVAILABLE = False

try:
    from fix_duplicate_ids import fix_duplicate_ids_in_tree
    DUPLICATE_ID_FIX_AVAILABLE = True
except ImportError:
    DUPLICATE_ID_FIX_AVAILABLE = False

# Try to import tracker update function for updating ID mappings
try:
    from epub_to_structured_v2 import update_id_mapping_target, add_id_rename
    ID_TRACKER_AVAILABLE = True
except ImportError:
    ID_TRACKER_AVAILABLE = False
    update_id_mapping_target = None
    add_id_rename = None

# Try to import centralized ID Authority for ID management
try:
    from id_authority import get_authority, XSL_RECOGNIZED_CODES
    ID_AUTHORITY_AVAILABLE = True
except ImportError:
    ID_AUTHORITY_AVAILABLE = False
    get_authority = None
    XSL_RECOGNIZED_CODES = set()

# Import strategy pattern for extensible fixing
try:
    from dtd_fixer_strategies import (
        FixerStrategy, StrategyRegistry, FixContext, FixResult,
        run_fix_strategies, get_global_registry, register_strategy,
        create_strategy_from_function
    )
    STRATEGY_PATTERN_AVAILABLE = True
except ImportError:
    STRATEGY_PATTERN_AVAILABLE = False
    FixerStrategy = None
    StrategyRegistry = None


class ComprehensiveDTDFixer:
    """Comprehensive DTD fixer that handles all common validation errors"""

    def __init__(self, dtd_path: Path):
        self.dtd_path = dtd_path
        self.dtd = etree.DTD(str(dtd_path))
        self.fixes_applied = []
        self.verification_items = []
        self.id_renames: Dict[str, str] = {}
        # Initialize strategy registry for plugin-based fixes
        self._strategy_registry = None
        if STRATEGY_PATTERN_AVAILABLE:
            self._strategy_registry = get_global_registry()

    def _add_id_rename(self, old_id: str, new_id: str) -> None:
        """
        Add an ID rename to the local tracking dictionary with chain composition.

        When an ID is renamed multiple times (A->B, then B->C), this method ensures
        the mapping stays as original->final (A->C) rather than requiring multiple
        passes to follow the chain.

        This mirrors the behavior of add_id_rename() in epub_to_structured_v2.py
        but operates on the local self.id_renames dict for standalone processing.

        Args:
            old_id: The ID being renamed
            new_id: The new ID value
        """
        if old_id == new_id:
            return  # No-op for identity renames

        # First, update any existing renames that point TO old_id
        # If we have A->B and now adding B->C, update A->B to become A->C
        for source_id, target_id in list(self.id_renames.items()):
            if target_id == old_id:
                self.id_renames[source_id] = new_id
                logger.debug(f"Composed local ID rename chain: {source_id} -> {old_id} -> {new_id}")

        # Add the new rename
        self.id_renames[old_id] = new_id

        # Also update the global tracker if available (for main pipeline integration)
        if ID_TRACKER_AVAILABLE and add_id_rename is not None:
            try:
                add_id_rename(old_id, new_id)
            except (TypeError, AttributeError) as e:
                logger.debug(f"Global add_id_rename skipped for {old_id}->{new_id}: {type(e).__name__}")

    def _add_id_renames_batch(self, id_mapping: Dict[str, str]) -> None:
        """
        Add multiple ID renames with chain composition.

        Args:
            id_mapping: Dictionary of old_id -> new_id mappings
        """
        for old_id, new_id in id_mapping.items():
            self._add_id_rename(old_id, new_id)

    def _run_strategy_fixes(
        self,
        root: etree._Element,
        filename: str,
        valid_ids: Optional[Set[str]] = None
    ) -> List[str]:
        """
        Run strategy-based fixes from the plugin system.

        This allows external code to register custom fix strategies that
        will be executed during the fixing process.

        Args:
            root: XML root element
            filename: Chapter filename
            valid_ids: Optional set of valid IDs

        Returns:
            List of fix descriptions
        """
        if not STRATEGY_PATTERN_AVAILABLE or self._strategy_registry is None:
            return []

        if len(self._strategy_registry) == 0:
            return []

        # Create fix context
        context = FixContext(
            filename=filename,
            all_ids=valid_ids or set(),
            id_mapping=self.id_renames,
            config={'dtd_path': str(self.dtd_path)}
        )

        # Run all registered strategies
        results = run_fix_strategies(
            root=root,
            filename=filename,
            registry=self._strategy_registry,
            context=context
        )

        # Convert FixResult objects to fix description strings
        fix_descriptions = []
        for result in results:
            desc = f"{filename}: {result.description}"
            if result.element_id:
                desc += f" (id={result.element_id})"
            fix_descriptions.append(desc)

        return fix_descriptions

    def register_strategy(self, strategy: 'FixerStrategy') -> None:
        """
        Register a custom fix strategy.

        Args:
            strategy: FixerStrategy instance to register
        """
        if STRATEGY_PATTERN_AVAILABLE and self._strategy_registry:
            self._strategy_registry.register(strategy)

    @staticmethod
    def _local_name(element: etree._Element) -> str:
        """Extract local name from element tag."""
        tag = element.tag
        if not isinstance(tag, str):
            return ""
        if tag.startswith("{"):
            return tag.split("}", 1)[1]
        return tag

    @staticmethod
    def _get_element_text(element: etree._Element, max_length: Optional[int] = None) -> str:
        """
        Efficiently extract and normalize text from an element.

        This helper avoids repeated ''.join(elem.itertext()).strip() calls
        which create intermediate tuples and strings.

        Args:
            element: The XML element to extract text from
            max_length: Optional maximum length to return (truncates if longer)

        Returns:
            Normalized text content of the element
        """
        if element is None:
            return ""
        text = ''.join(element.itertext()).strip()
        if max_length and len(text) > max_length:
            return text[:max_length]
        return text

    @staticmethod
    def _is_inline_only(element: etree._Element) -> bool:
        """
        Check if an element contains only inline content (no block elements).

        Block elements include: itemizedlist, orderedlist, figure, table, sect*, etc.
        Inline elements include: emphasis, ulink, subscript, superscript, anchor, etc.
        """
        BLOCK_ELEMENTS = {
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
            'figure', 'informalfigure', 'table', 'informaltable',
            'example', 'informalexample',
            'programlisting', 'screen', 'literallayout',
            'blockquote', 'note', 'warning', 'caution', 'important', 'tip',
            'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section',
            'para'  # Nested para is also a block element
        }

        # Check all child elements
        for child in element:
            if isinstance(child.tag, str):
                local = ComprehensiveDTDFixer._local_name(child)
                if local in BLOCK_ELEMENTS:
                    return False
                # Recursively check children
                if not ComprehensiveDTDFixer._is_inline_only(child):
                    return False

        return True

    def _update_child_ids_for_wrapper(self, wrapper_sect1: etree._Element, chapter_id: str,
                                      new_sect1_id: str, root: etree._Element) -> Tuple[List[str], Dict[str, str]]:
        """
        Update IDs of child elements to use the new wrapper sect1 ID prefix.

        When elements are moved into a wrapper sect1, their IDs should reflect
        their new parent section. For example:
        - Old: ch0010s0000a0003 (anchor in sect1 s0000)
        - New: ch0010s0006a0003 (anchor now in wrapper sect1 s0006)

        Also updates any cross-references (linkend, url) that point to the old IDs.

        Args:
            wrapper_sect1: The wrapper sect1 element containing moved elements
            chapter_id: The chapter ID (e.g., 'ch0010')
            new_sect1_id: The ID of the wrapper sect1 (e.g., 'ch0010s0006')
            root: The root element for updating cross-references

        Returns:
            Tuple of (list of fix descriptions, dict mapping old_id -> new_id)
        """
        import re
        fixes = []
        id_mapping = {}  # old_id -> new_id

        # Extract the sect1 number from the new ID (e.g., 's0006' from 'ch0010s0006')
        new_sect1_suffix = new_sect1_id[len(chapter_id):]  # e.g., 's0006'

        # Pattern to match IDs that belong to this chapter and have a sect1 prefix
        # e.g., ch0010s0000a0003, ch0010s0001f0001
        sect_id_pattern = re.compile(
            rf'^({re.escape(chapter_id)})(s\d{{4}})(.+)$'
        )

        # Collect all IDs in the document to avoid duplicates
        all_ids = {e.get('id') for e in root.iter() if e.get('id')}

        # First pass: collect all IDs that need to be updated
        for elem in wrapper_sect1.iter():
            old_id = elem.get('id')
            if not old_id:
                continue

            # Skip the wrapper itself
            if old_id == new_sect1_id:
                continue

            match = sect_id_pattern.match(old_id)
            if match:
                ch_part, old_sect_part, suffix = match.groups()
                # Only update if the sect part is different from the new wrapper
                if old_sect_part != new_sect1_suffix:
                    new_id = f"{ch_part}{new_sect1_suffix}{suffix}"

                    # Check for duplicates
                    if new_id in all_ids and new_id != old_id:
                        # Append a unique suffix to avoid collision
                        counter = 1
                        base_new_id = new_id
                        while new_id in all_ids:
                            new_id = f"{base_new_id}{counter}"
                            counter += 1

                    id_mapping[old_id] = new_id
                    all_ids.add(new_id)

        # Second pass: update the IDs and tracker mappings
        for elem in wrapper_sect1.iter():
            old_id = elem.get('id')
            if old_id and old_id in id_mapping:
                new_id = id_mapping[old_id]
                elem.set('id', new_id)
                fixes.append(f"Updated ID {old_id} -> {new_id}")

                # Also update the ID tracker mappings if available
                # This ensures linkend resolution uses the new IDs
                if ID_TRACKER_AVAILABLE and update_id_mapping_target:
                    try:
                        update_id_mapping_target(old_id, new_id)
                    except (TypeError, AttributeError, RuntimeError) as e:
                        # Tracker might not be initialized in standalone mode
                        # TypeError: update_id_mapping_target not callable
                        # AttributeError: tracker not fully initialized
                        # RuntimeError: tracker in invalid state
                        logger.debug(f"ID tracker update skipped for {old_id}->{new_id}: {type(e).__name__}")

                # Track ID rename in both local and global systems with chain composition
                # This ensures standalone processing and main pipeline both work correctly
                self._add_id_rename(old_id, new_id)

        # Third pass: update cross-references in the entire document
        if id_mapping:
            for elem in root.iter():
                # Update linkend attributes
                linkend = elem.get('linkend')
                if linkend and linkend in id_mapping:
                    elem.set('linkend', id_mapping[linkend])
                    fixes.append(f"Updated linkend {linkend} -> {id_mapping[linkend]}")

                # Update url attributes that reference IDs (internal links)
                url = elem.get('url')
                if url:
                    for old_id, new_id in id_mapping.items():
                        if f'#{old_id}' in url:
                            new_url = url.replace(f'#{old_id}', f'#{new_id}')
                            elem.set('url', new_url)
                            fixes.append(f"Updated url reference {old_id} -> {new_id}")
                            break

        return fixes, id_mapping

    def fix_chapter_file(self, chapter_path: Path, chapter_filename: str, valid_ids: Optional[Set[str]] = None) -> Tuple[int, List[str]]:
        """
        Apply comprehensive fixes to a chapter XML file.

        Args:
            chapter_path: Path to the chapter XML file
            chapter_filename: Name of the chapter file
            valid_ids: Optional set of all valid IDs across all chapters (for cross-reference fixing)

        Returns:
            Tuple of (num_fixes, list_of_fix_descriptions)
        """
        fixes = []

        try:
            # Parse XML - preserve whitespace to avoid damaging formatted content
            # Use recover=True to handle malformed XML and resolve_entities=False
            # for files with external entity references (e.g., part/subpart files
            # referencing &ch0004;)
            parser = etree.XMLParser(remove_blank_text=False, resolve_entities=False, recover=True)
            tree = etree.parse(str(chapter_path), parser)
            root = tree.getroot()

            # Apply fixes in order
            # FIRST: Fix duplicate IDs (critical DTD validation error)
            fixes.extend(self._fix_duplicate_ids(root, chapter_filename))

            # First remove misclassified figures and empty mediaobjects
            fixes.extend(self._remove_empty_mediaobjects(root, chapter_filename))
            fixes.extend(self._remove_misclassified_table_figures(root, chapter_filename))
            # Remove empty table rows
            fixes.extend(self._remove_empty_rows(root, chapter_filename))
            # Ensure tables have at least one row for DTD compliance
            fixes.extend(self._fix_empty_table_bodies(root, chapter_filename))
            # Fix duplicate titles EARLY (before other fixes that rely on proper structure)
            fixes.extend(self._fix_duplicate_titles(root, chapter_filename))
            # Fix nested para elements (important for preserving links)
            fixes.extend(self._fix_nested_para_elements(root, chapter_filename))
            # Then apply other fixes
            fixes.extend(self._fix_missing_titles(root, chapter_filename))
            fixes.extend(self._fix_empty_chapter_titles(root, chapter_filename))
            fixes.extend(self._fix_anchor_before_title(root, chapter_filename))
            fixes.extend(self._fix_beginpage_position(root, chapter_filename))
            fixes.extend(self._fix_dedication_invalid_elements(root, chapter_filename))
            fixes.extend(self._fix_dedication_content(root, chapter_filename))
            fixes.extend(self._fix_content_after_sections(root, chapter_filename))
            fixes.extend(self._fix_nested_figures(root, chapter_filename))
            fixes.extend(self._fix_nested_sidebars(root, chapter_filename))
            fixes.extend(self._fix_invalid_content_models(root, chapter_filename))
            # Re-run title/anchor ordering after structural fixes
            fixes.extend(self._fix_duplicate_titles(root, chapter_filename))
            fixes.extend(self._fix_anchor_before_title(root, chapter_filename))
            fixes.extend(self._fix_anchor_content(root, chapter_filename))
            fixes.extend(self._fix_anchors_in_chapter(root, chapter_filename))
            fixes.extend(self._fix_empty_elements(root, chapter_filename))
            fixes.extend(self._fix_missing_required_attributes(root, chapter_filename))
            fixes.extend(self._fix_invalid_elements(root, chapter_filename))
            fixes.extend(self._normalize_whitespace(root, chapter_filename))
            # Fix link elements missing required linkend attribute
            fixes.extend(self._fix_links_missing_linkend(root, chapter_filename))
            # Fix sect1 elements missing @id attribute (required for chunking)
            fixes.extend(self._fix_sect1_missing_ids(root, chapter_filename))
            # Fix section IDs (sect1-5) that don't match required format
            fixes.extend(self._fix_sect1_noncompliant_ids(root, chapter_filename))
            # Fix element IDs (figures, tables, etc.) that don't match required format
            fixes.extend(self._fix_element_noncompliant_ids(root, chapter_filename))
            # Fix element IDs incorrectly placed on para caption elements
            fixes.extend(self._fix_element_id_on_para_caption(root, chapter_filename))
            # Final duplicate ID sweep after all ID mutations
            fixes.extend(self._fix_duplicate_ids(root, chapter_filename))
            # Fix block elements incorrectly nested inside para (DTD violation)
            fixes.extend(self._fix_block_elements_in_para(root, chapter_filename))
            # Fix footnotes: remove invalid children (anchor, indexterm, figure) and fix placement
            fixes.extend(self._fix_footnote_invalid_children(root, chapter_filename))
            fixes.extend(self._fix_footnote_placement(root, chapter_filename))
            # Fix bibliography issues: figures in bibliography, bibliography in sections
            fixes.extend(self._fix_figure_in_bibliography(root, chapter_filename))
            fixes.extend(self._fix_bibliography_in_sections(root, chapter_filename))
            # Wrap orphaned bibliography elements (direct chapter children) in sect1
            # so RISchunker can route them as navigable pages.
            fixes.extend(self._fix_orphaned_bibliography_at_chapter_level(root, chapter_filename))
            # Fix nested bibliography elements (bibliography cannot contain bibliography)
            fixes.extend(self._fix_nested_bibliography(root, chapter_filename))
            # Fix sections missing required block content (e.g., sect1 with only title + bibliography)
            fixes.extend(self._fix_section_missing_block_content(root, chapter_filename))
            # Fix author elements with both personname and affiliation (DTD violation)
            fixes.extend(self._fix_author_personname_affiliation(root, chapter_filename))
            # Fix phrase elements inside restrictive elements (superscript, subscript, etc.)
            fixes.extend(self._fix_phrase_in_restrictive_elements(root, chapter_filename))
            # Fix footnotes in glossary/bibliography (not allowed by DTD)
            fixes.extend(self._fix_footnote_in_glossary_bibliography(root, chapter_filename))

            # === NEW COMPREHENSIVE CONTENT MODEL FIXES ===
            # Fix empty/incomplete list structures
            fixes.extend(self._fix_listitem_missing_content(root, chapter_filename))
            fixes.extend(self._fix_varlistentry_structure(root, chapter_filename))

            # Fix Q&A and glossary structures
            fixes.extend(self._fix_qandaentry_structure(root, chapter_filename))
            fixes.extend(self._fix_glossentry_structure(root, chapter_filename))

            # Fix empty admonitions (note, warning, caution, important, tip)
            fixes.extend(self._fix_empty_admonitions(root, chapter_filename))

            # Fix empty/incomplete formal objects
            fixes.extend(self._fix_example_missing_content(root, chapter_filename))
            fixes.extend(self._fix_figure_missing_content(root, chapter_filename))
            fixes.extend(self._fix_table_missing_content(root, chapter_filename))

            # Fix empty containers (blockquote, sidebar, bibliodiv)
            fixes.extend(self._fix_blockquote_missing_content(root, chapter_filename))
            fixes.extend(self._fix_sidebar_missing_content(root, chapter_filename))
            fixes.extend(self._fix_glossary_bare_paras(root, chapter_filename))
            fixes.extend(self._fix_bibliodiv_missing_entries(root, chapter_filename))

            # Fix media object issues
            fixes.extend(self._fix_imageobject_missing_imagedata(root, chapter_filename))

            # === ADDITIONAL BLOCK ELEMENT FIXES ===
            # Fix procedure/step structures
            fixes.extend(self._fix_procedure_missing_steps(root, chapter_filename))
            fixes.extend(self._fix_step_missing_content(root, chapter_filename))
            fixes.extend(self._fix_substeps_missing_steps(root, chapter_filename))

            # Fix callout structures
            fixes.extend(self._fix_calloutlist_missing_callouts(root, chapter_filename))
            fixes.extend(self._fix_callout_missing_content(root, chapter_filename))

            # Fix abstract/formalpara/legalnotice
            fixes.extend(self._fix_abstract_missing_content(root, chapter_filename))
            fixes.extend(self._fix_formalpara_structure(root, chapter_filename))
            fixes.extend(self._fix_legalnotice_missing_content(root, chapter_filename))

            # Fix invalid nesting issues (new fixes for DTD compliance)
            fixes.extend(self._fix_sect_in_indexdiv(root, chapter_filename))
            fixes.extend(self._fix_bridgehead_in_index(root, chapter_filename))
            fixes.extend(self._fix_bridgehead_in_indexdiv(root, chapter_filename))
            fixes.extend(self._fix_bridgehead_in_abstract(root, chapter_filename))
            fixes.extend(self._fix_para_in_bridgehead(root, chapter_filename))
            fixes.extend(self._fix_para_in_subtitle(root, chapter_filename))
            fixes.extend(self._fix_nested_tables(root, chapter_filename))
            fixes.extend(self._fix_para_in_table(root, chapter_filename))
            fixes.extend(self._fix_indexdiv_missing_indexentry(root, chapter_filename))

            # Fix epigraph/highlights/simplesect
            fixes.extend(self._fix_epigraph_missing_content(root, chapter_filename))
            fixes.extend(self._fix_highlights_missing_content(root, chapter_filename))
            fixes.extend(self._fix_simplesect_missing_content(root, chapter_filename))

            # Fix informal elements (informalfigure, informalexample, informalequation)
            fixes.extend(self._fix_informalfigure_missing_content(root, chapter_filename))
            fixes.extend(self._fix_informalexample_missing_content(root, chapter_filename))
            fixes.extend(self._fix_informalequation_missing_content(root, chapter_filename))

            # Fix msgset/segmentedlist structures
            fixes.extend(self._fix_msgset_missing_entries(root, chapter_filename))
            fixes.extend(self._fix_segmentedlist_structure(root, chapter_filename))
            fixes.extend(self._fix_seglistitem_missing_seg(root, chapter_filename))

            # Strip escaped MathML markup leaked from EPUB comments
            fixes.extend(self._strip_escaped_mathml(root, chapter_filename))
            # Fix mediaobject inside bibliomixed (not allowed by DTD)
            fixes.extend(self._fix_mediaobject_in_bibliomixed(root, chapter_filename))

            # Fix broken cross-references if valid_ids provided
            if valid_ids is not None:
                fixes.extend(self._fix_broken_cross_references(root, chapter_filename, valid_ids))

            # Run strategy-based fixes (plugin system)
            if STRATEGY_PATTERN_AVAILABLE:
                strategy_fixes = self._run_strategy_fixes(root, chapter_filename, valid_ids)
                fixes.extend(strategy_fixes)

            # Always write back to ensure XML declaration is present
            # (even if no other fixes were needed)
            tree.write(
                str(chapter_path),
                encoding='utf-8',
                xml_declaration=True,
                pretty_print=True
            )

            return len(fixes), fixes

        except Exception as e:
            print(f"  [FAIL] Error fixing {chapter_filename}: {e}")
            return 0, []

    def _fix_duplicate_ids(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix duplicate ID attributes in the XML tree following R2 Library naming conventions.

        DTD requires all ID attributes to be unique within a document.
        Common issue: Same ID used multiple times (e.g., ch0010s0001t0001 appears 8 times)

        ID Naming Convention (from id-naming-convention-rules.md):
        - Format: {sect1_id}{element_code}{sequence_number}
        - Sect1 ID: ch{4-digit chapter}s{4-digit sect1} = 11 characters
        - Maximum ID length: 25 characters
        - Allowed characters: lowercase letters and numbers only

        Strategy:
        1. Find all duplicate IDs
        2. Keep the first occurrence as-is
        3. Rename subsequent occurrences following naming conventions
        """
        fixes = []

        # ELEMENT_CODES imported from id_authority.py (single source of truth)
        # MAX_ID_LENGTH imported from id_generator.py
        # Match all element type prefixes: ch (chapter), pr (preface), ap (appendix), dd (dedication), ak (acknowledgments), etc.
        SECT1_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4}s\d{4})')
        # Element codes can be 1-3 chars (e.g., 'a', 'fg', 'bib')
        ELEMENT_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4}s\d{4})([a-z]{1,3})(\d+)$')

        # Collect all elements with IDs
        id_elements: Dict[str, List[etree._Element]] = defaultdict(list)
        for elem in root.iter():
            elem_id = elem.get('id')
            if elem_id:
                id_elements[elem_id].append(elem)

        # Find duplicates (IDs used more than once)
        duplicates = {
            id_val: elements
            for id_val, elements in id_elements.items()
            if len(elements) > 1
        }

        if not duplicates:
            return fixes

        # Collect all existing IDs for collision avoidance
        existing_ids = set(id_elements.keys())

        # Track max sequence numbers per sect1/element_code
        max_sequences: Dict[str, int] = defaultdict(int)
        for id_val in existing_ids:
            match = ELEMENT_ID_PATTERN.match(id_val)
            if match:
                key = f"{match.group(1)}_{match.group(2)}"
                seq = int(match.group(3))
                if seq > max_sequences[key]:
                    max_sequences[key] = seq

        # Process each duplicate ID
        for dup_id, elements in duplicates.items():
            # Parse the duplicate ID to extract components
            sect1_match = SECT1_ID_PATTERN.match(dup_id)
            elem_match = ELEMENT_ID_PATTERN.match(dup_id)

            sect1_id = sect1_match.group(1) if sect1_match else None

            # Keep first element unchanged, rename others
            for idx, elem in enumerate(elements[1:], start=1):
                elem_tag = self._local_name(elem)

                # CRITICAL: Section elements (sect1-sect5) need proper section ID generation,
                # NOT the element code pattern. Using element codes like 's1' for sect1 produces
                # malformed IDs like ch0005s0001s10002 which break downstream XSL processing.
                if elem_tag == 'sect1':
                    # For sect1, extract the chapter prefix and use next_available_sect1_id
                    chapter_match = re.match(r'^([a-z]{2}\d{4})', dup_id)
                    chapter_id = chapter_match.group(1) if chapter_match else 'ch0001'
                    new_id = next_available_sect1_id(chapter_id, existing_ids)
                elif elem_tag in {'sect2', 'sect3', 'sect4', 'sect5'}:
                    # For deeper sections, find the parent section's ID and build hierarchical ID
                    parent = elem.getparent()
                    parent_sect_id = None
                    while parent is not None:
                        parent_tag = self._local_name(parent) if hasattr(parent, 'tag') else ''
                        if parent_tag in {'sect1', 'sect2', 'sect3', 'sect4'}:
                            parent_sect_id = parent.get('id')
                            if parent_sect_id:
                                break
                        elif parent_tag in {'chapter', 'appendix', 'preface'}:
                            # If no parent section, use chapter ID with s0001
                            parent_sect_id = parent.get('id', 'ch0001') + 's0001'
                            break
                        parent = parent.getparent()

                    if parent_sect_id:
                        # Generate hierarchical section ID
                        level = int(elem_tag[-1])  # sect2 -> 2, sect3 -> 3, etc.
                        counter = 1
                        if level == 2:
                            new_id = f"{parent_sect_id}s{counter:04d}"
                        else:
                            new_id = f"{parent_sect_id}s{counter:02d}"
                        while new_id in existing_ids:
                            counter += 1
                            if level == 2:
                                new_id = f"{parent_sect_id}s{counter:04d}"
                            else:
                                new_id = f"{parent_sect_id}s{counter:02d}"
                    else:
                        # Fallback: append numeric suffix
                        new_id = f"{dup_id}{idx:02d}"
                        while new_id in existing_ids:
                            idx += 1
                            new_id = f"{dup_id}{idx:02d}"
                else:
                    # Regular elements: use element code pattern
                    elem_code = ELEMENT_CODES.get(elem_tag.lower(), 'x')

                    if sect1_id:
                        # Generate ID following naming convention
                        key = f"{sect1_id}_{elem_code}"
                        max_sequences[key] += 1
                        new_seq = max_sequences[key]

                        # Calculate available digits
                        available_digits = MAX_ID_LENGTH - len(sect1_id) - len(elem_code)

                        if available_digits >= 4:
                            seq_str = f"{new_seq:04d}"
                        elif available_digits >= 3:
                            seq_str = f"{new_seq:03d}"
                        else:
                            seq_str = f"{new_seq % 100:02d}"

                        new_id = f"{sect1_id}{elem_code}{seq_str}"

                        # Ensure uniqueness
                        while new_id in existing_ids:
                            max_sequences[key] += 1
                            new_seq = max_sequences[key]
                            if available_digits >= 4:
                                seq_str = f"{new_seq:04d}"
                            elif available_digits >= 3:
                                seq_str = f"{new_seq:03d}"
                            else:
                                seq_str = f"{new_seq % 100:02d}"
                            new_id = f"{sect1_id}{elem_code}{seq_str}"
                    else:
                        # Fallback for non-standard IDs: append numeric suffix
                        suffix = f"{idx:02d}"
                        available_space = MAX_ID_LENGTH - len(suffix)

                        if len(dup_id) <= available_space:
                            new_id = f"{dup_id}{suffix}"
                        else:
                            new_id = f"{dup_id[:available_space]}{suffix}"

                        # Ensure uniqueness
                        while new_id in existing_ids:
                            idx += 1
                            suffix = f"{idx:02d}"
                            if len(dup_id) <= MAX_ID_LENGTH - len(suffix):
                                new_id = f"{dup_id}{suffix}"
                            else:
                                new_id = f"{dup_id[:MAX_ID_LENGTH - len(suffix)]}{suffix}"

                # Update the element
                elem.set('id', new_id)
                existing_ids.add(new_id)

                line_num = elem.sourceline if hasattr(elem, 'sourceline') else None

                fixes.append(f"Renamed duplicate ID '{dup_id}' to '{new_id}' on <{elem_tag}> at line {line_num or 'unknown'} in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=line_num,
                        fix_type="Duplicate ID Fix",
                        fix_description=f"Renamed '{dup_id}' to '{new_id}'",
                        verification_reason=f"ID '{dup_id}' was used {len(elements)} times. This was occurrence #{idx + 1}.",
                        suggestion="Verify the renamed ID follows naming conventions (max 25 chars, lowercase alphanumeric only)."
                    ))

        return fixes

    def _fix_duplicate_titles(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix sections with duplicate title elements.

        DTD expects: sect1/sect2/etc. to have exactly ONE title element.
        Common issue: (title anchor title sect2) - two titles with elements between them.

        Strategy:
        1. Find sections with multiple titles
        2. Keep the best title (first non-empty)
        3. Remove duplicate titles
        4. Move misplaced elements (like anchors) to proper positions
        """
        fixes = []

        # Elements that should have exactly one title
        section_elements = ['chapter', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
                           'appendix', 'figure', 'table', 'example', 'sidebar']

        for elem_name in section_elements:
            for elem in root.iter(elem_name):
                # Find all title children (direct children only)
                titles = [child for child in elem if child.tag == 'title']

                if len(titles) > 1:
                    elem_id = elem.get('id', f'{elem_name}')

                    # Keep the best title - prefer real content over placeholder patterns
                    best_title = titles[0]

                    # First pass: look for title that doesn't match placeholder pattern
                    placeholder_pattern = re.compile(r'^Section\s+\w+$')
                    for title in titles:
                        title_text = (title.text or '').strip()
                        if title_text and not placeholder_pattern.match(title_text):
                            # Found a real title (not a placeholder)
                            best_title = title
                            break

                    # Second pass: if no real title found, keep last title with any content
                    if not best_title.text or placeholder_pattern.match(best_title.text.strip()):
                        for title in reversed(titles):
                            if title.text and title.text.strip():
                                best_title = title
                                break

                    # Get the index of the best title to ensure it's first
                    best_title_index = list(elem).index(best_title)

                    # Collect elements between titles that should be moved after the title
                    elements_to_move = []
                    in_between = False
                    for child in list(elem):
                        if child.tag == 'title':
                            if child == best_title:
                                in_between = True
                                continue
                            # This is a duplicate title, will be removed
                            in_between = False
                        elif in_between:
                            # Element between titles - should be moved after best title
                            if child.tag in ['anchor', 'indexterm']:  # Only move safe elements
                                elements_to_move.append(child)

                    # Remove duplicate titles
                    for title in titles:
                        if title != best_title:
                            elem.remove(title)
                            fixes.append(f"Removed duplicate <title> from <{elem_name} id='{elem_id}'> in {filename}")

                    # Move anchors/indexterms to after the best title
                    if elements_to_move:
                        # Ensure best_title is first (or after info element if present)
                        elem.remove(best_title)
                        insert_pos = 0
                        for i, child in enumerate(elem):
                            if child.tag in ['chapterinfo', 'sect1info', 'sect2info', 'sect3info',
                                           'sect4info', 'sect5info', 'appendixinfo']:
                                insert_pos = i + 1
                        elem.insert(insert_pos, best_title)

                        # Move collected elements to after title
                        for moved_elem in elements_to_move:
                            elem.remove(moved_elem)
                            elem.insert(insert_pos + 1, moved_elem)
                            insert_pos += 1

                        fixes.append(f"Moved {len(elements_to_move)} element(s) to proper position after title in <{elem_name} id='{elem_id}'> in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=elem.sourceline if hasattr(elem, 'sourceline') else None,
                            fix_type="Duplicate Title Fix",
                            fix_description=f"Fixed {len(titles)} duplicate titles in <{elem_name} id='{elem_id}'>. Kept: '{best_title.text[:50] if best_title.text else '(empty)'}'",
                            verification_reason=f"DTD requires exactly one title per {elem_name}. Found {len(titles)} titles.",
                            suggestion="Verify the kept title is correct."
                        ))

        return fixes

    def _fix_missing_titles(self, root: etree._Element, filename: str) -> List[str]:
        """Fix elements that require <title> but are missing it.

        Strategy: Extract title from actual content before falling back to generic text:
        - For chapters: Use first sect1 title, first bridgehead, or first para text
        - For sections: Look for bridgehead or emphasized text within
        - For figures: Extract from figcaption, alt text, or textobject
        - For tables: Extract from caption or first meaningful content
        """
        fixes = []

        # Elements that require titles in RittDoc DTD
        title_required = ['chapter', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
                          'figure', 'table', 'example', 'appendix']

        for elem_name in title_required:
            for elem in root.iter(elem_name):
                # Check if title exists anywhere in the element's direct children
                has_title = any(child.tag == 'title' for child in elem)

                if not has_title:
                    # Create and insert title for elements that require them
                    title = etree.Element('title')
                    extracted_title = None

                    # Try to extract title from actual content
                    if elem_name in ('chapter', 'appendix'):
                        # For chapters: look for first sect1 title or bridgehead
                        first_sect1 = elem.find('sect1')
                        if first_sect1 is not None:
                            sect1_title = first_sect1.find('title')
                            if sect1_title is not None and sect1_title.text:
                                extracted_title = sect1_title.text.strip()
                        if not extracted_title:
                            bridgehead = elem.find('.//bridgehead')
                            if bridgehead is not None:
                                extracted_title = ''.join(bridgehead.itertext()).strip()

                    elif elem_name.startswith('sect'):
                        # For sections: look for bridgehead or emphasized heading
                        bridgehead = elem.find('bridgehead')
                        if bridgehead is not None:
                            extracted_title = ''.join(bridgehead.itertext()).strip()
                        if not extracted_title:
                            # Look for formalpara/title (often used for inline headings)
                            formalpara_title = elem.find('.//formalpara/title')
                            if formalpara_title is not None:
                                extracted_title = ''.join(formalpara_title.itertext()).strip()
                        if not extracted_title:
                            # Look for first para with emphasis that looks like a heading
                            for para in elem.iter('para'):
                                for emp in para.iter('emphasis'):
                                    role = (emp.get('role') or '').lower()
                                    if role in {'bold', 'strong', 'heading', 'title'} or not role:
                                        extracted_title = ''.join(emp.itertext()).strip()
                                        if extracted_title:
                                            break
                                if extracted_title:
                                    break
                        if not extracted_title:
                            # Last fallback: use a short opening line from the first para
                            for para in elem.iter('para'):
                                para_text = ''.join(para.itertext()).strip()
                                if para_text and len(para_text.split()) <= 8:
                                    extracted_title = para_text
                                    break

                    elif elem_name == 'figure':
                        # For figures: look for caption or textobject
                        caption = elem.find('.//caption')
                        if caption is not None:
                            extracted_title = ''.join(caption.itertext()).strip()
                        if not extracted_title:
                            textobject = elem.find('.//textobject/phrase')
                            if textobject is not None:
                                extracted_title = ''.join(textobject.itertext()).strip()
                        if not extracted_title:
                            # Try alt attribute on imagedata
                            imagedata = elem.find('.//imagedata')
                            if imagedata is not None and imagedata.get('alt'):
                                extracted_title = imagedata.get('alt').strip()

                    elif elem_name == 'table':
                        # For tables: look for caption
                        caption = elem.find('caption')
                        if caption is not None:
                            extracted_title = ''.join(caption.itertext()).strip()

                    # Use extracted title or fall back to generic
                    if extracted_title:
                        title.text = extracted_title[:200]  # Limit length
                    else:
                        # Fallback: generate unique ID-based title
                        elem_id = elem.get('id', elem_name)
                        title.text = f"Untitled {elem_name.replace('sect', 'Section ')} ({elem_id})"

                    # Insert as first child
                    elem.insert(0, title)

                    fix_desc = f"Added {'extracted' if extracted_title else 'generated'} title to <{elem_name}>"
                    fixes.append(f"{fix_desc} in {filename}")

                    # Add verification item
                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=elem.sourceline if hasattr(elem, 'sourceline') else None,
                            fix_type="Missing Title Fix",
                            fix_description=f"Added title: '{title.text[:60]}...' " if len(title.text or '') > 60 else f"Added title: '{title.text}'",
                            verification_reason="Title extracted from content" if extracted_title else "Title auto-generated (no content found)",
                            suggestion="Review title accuracy" if extracted_title else "Update title to describe content"
                        ))

        return fixes

    def _fix_empty_chapter_titles(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix chapter/preface/appendix elements that have empty titles.

        If the chapter title is empty but the first sect1 has a title,
        copy the sect1 title to the chapter title.
        """
        fixes = []

        # Parent elements that can have sect1 children
        parent_elements = ['chapter', 'preface', 'appendix']

        for elem_name in parent_elements:
            for elem in root.iter(elem_name):
                # Find the title element
                title_elem = None
                for child in elem:
                    if isinstance(child.tag, str) and self._local_name(child) == 'title':
                        title_elem = child
                        break

                if title_elem is None:
                    continue

                # Check if title is empty
                title_text = ''.join(title_elem.itertext()).strip()
                if title_text:
                    continue  # Title already has content

                # Look for first sect1 with a title
                first_sect1 = None
                for child in elem:
                    if isinstance(child.tag, str) and self._local_name(child) == 'sect1':
                        first_sect1 = child
                        break

                if first_sect1 is None:
                    continue

                # Get sect1's title
                sect1_title = None
                for child in first_sect1:
                    if isinstance(child.tag, str) and self._local_name(child) == 'title':
                        sect1_title = child
                        break

                if sect1_title is None:
                    continue

                sect1_title_text = ''.join(sect1_title.itertext()).strip()
                if sect1_title_text:
                    title_elem.text = sect1_title_text
                    fixes.append(f"Fixed empty <{elem_name}> title using first sect1 title: '{sect1_title_text[:50]}...' in {filename}")

        return fixes

    def _fix_anchor_before_title(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix anchor elements that appear before title in sections.

        Per DTD, sect1-sect5 content model is:
          (sect1info?, title, subtitle?, titleabbrev?, nav*, (divcomponent.mix+, ...))

        The title MUST come first (after optional sect1info). Anchor is part of
        divcomponent.mix and must come AFTER the title.
        """
        fixes = []

        # Elements where anchor might incorrectly precede title
        section_elements = ['chapter', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
                           'sect6', 'sect7', 'sect8', 'sect9', 'sect10',
                           'appendix', 'preface', 'section']

        for elem_name in section_elements:
            for elem in root.iter(elem_name):
                children = list(elem)
                if len(children) < 2:
                    continue

                # Find title position
                title_idx = None
                for i, child in enumerate(children):
                    if isinstance(child.tag, str) and self._local_name(child) == 'title':
                        title_idx = i
                        break

                if title_idx is None:
                    continue

                # Find any anchors that appear before title
                anchors_before_title = []
                for i in range(title_idx):
                    child = children[i]
                    if isinstance(child.tag, str) and self._local_name(child) == 'anchor':
                        anchors_before_title.append((i, child))

                # Move anchors to after the title
                # Process in reverse order to maintain correct indices while removing
                for i, anchor in reversed(anchors_before_title):
                    elem.remove(anchor)
                    # Insert right after title
                    elem.insert(title_idx, anchor)
                    fixes.append(f"Moved <anchor> after <title> in <{elem_name}> in {filename}")

        return fixes

    def _fix_anchor_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Ensure <anchor> elements are EMPTY as required by the DTD.

        Any text or child nodes are moved to the surrounding context while
        preserving the anchor element as an empty ID target.
        """
        fixes = []
        for anchor in root.iter('anchor'):
            has_children = len(anchor) > 0
            has_text = anchor.text is not None
            if not has_children and not (has_text and anchor.text.strip()):
                continue

            parent = anchor.getparent()
            if parent is None:
                continue

            orig_text = anchor.text or ''
            orig_tail = anchor.tail or ''
            children = list(anchor)

            # Clear anchor content
            anchor.text = None
            anchor.tail = None

            insert_index = list(parent).index(anchor) + 1
            moved_children = []
            for child in children:
                anchor.remove(child)
                parent.insert(insert_index, child)
                moved_children.append(child)
                insert_index += 1

            if orig_text:
                anchor.tail = (anchor.tail or '') + orig_text

            if orig_tail:
                if moved_children:
                    last = moved_children[-1]
                    last.tail = (last.tail or '') + orig_tail
                else:
                    anchor.tail = (anchor.tail or '') + orig_tail

            fixes.append(f"Stripped content from <anchor> in {filename}")

        return fixes

    def _fix_anchors_in_chapter(self, root: etree._Element, filename: str) -> List[str]:
        """
        Move anchor elements that are direct children of chapter into the first sect1.

        Per DTD, chapter content model is:
          (beginpage?, chapterinfo?, (title, subtitle?, titleabbrev?), tocchap?,
           (toc | lot | index | glossary | bibliography | sect1)*)

        Anchor is NOT allowed as a direct child of chapter. It must be inside
        sect1, para, or other block elements that allow anchor in their content model.
        """
        fixes = []

        # Find chapter elements (could be root or nested)
        chapters = []
        if self._local_name(root) == 'chapter':
            chapters.append(root)
        chapters.extend(root.iter('chapter'))

        # Also handle appendix and preface which have similar restrictions
        for elem_name in ['appendix', 'preface']:
            if self._local_name(root) == elem_name:
                chapters.append(root)
            chapters.extend(root.iter(elem_name))

        for chapter in chapters:
            # Find anchor elements that are direct children of chapter
            anchors_to_move = []
            for child in list(chapter):
                if isinstance(child.tag, str) and self._local_name(child) == 'anchor':
                    anchors_to_move.append(child)

            if not anchors_to_move:
                continue

            # Find the first sect1 to move anchors into
            first_sect1 = None
            for child in chapter:
                if isinstance(child.tag, str) and self._local_name(child) == 'sect1':
                    first_sect1 = child
                    break

            # If no sect1 exists, create one
            if first_sect1 is None:
                # Find where to insert the new sect1 (after title, subtitle, titleabbrev, tocchap)
                insert_pos = 0
                for i, child in enumerate(list(chapter)):
                    local = self._local_name(child) if isinstance(child.tag, str) else ''
                    if local in ('beginpage', 'chapterinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap'):
                        insert_pos = i + 1

                # Get chapter ID for generating sect1 ID
                chapter_id = chapter.get('id', 'ch0001')

                # Collect existing IDs to avoid duplicates
                existing_ids = set()
                for elem in chapter.iter():
                    if isinstance(elem.tag, str) and elem.get('id'):
                        existing_ids.add(elem.get('id'))

                # Create a wrapper sect1 with next available ID
                first_sect1 = etree.Element('sect1')
                first_sect1.set('id', next_available_sect1_id(chapter_id, existing_ids))

                # Add a title (required for sect1)
                sect_title = etree.SubElement(first_sect1, 'title')
                # Try to copy title text from chapter title
                chapter_title = chapter.find('title')
                if chapter_title is not None and chapter_title.text:
                    sect_title.text = chapter_title.text
                else:
                    sect_title.text = "Content"

                chapter.insert(insert_pos, first_sect1)
                fixes.append(f"Created sect1 wrapper for orphan anchors in <{self._local_name(chapter)}> in {filename}")

            # Find the title position in sect1 to insert after
            sect1_title = first_sect1.find('title')
            if sect1_title is not None:
                insert_idx = list(first_sect1).index(sect1_title) + 1
            else:
                insert_idx = 0

            # Move all anchors to the first sect1
            for anchor in anchors_to_move:
                chapter.remove(anchor)
                first_sect1.insert(insert_idx, anchor)
                insert_idx += 1
                anchor_id = anchor.get('id', '(no id)')
                fixes.append(f"Moved <anchor id='{anchor_id}'> from <{self._local_name(chapter)}> to <sect1> in {filename}")

        return fixes

    def _apply_id_renames(self, root: etree._Element, filename: str) -> int:
        """Apply recorded ID renames to linkend/url attributes in a document."""
        if not self.id_renames:
            return 0
        updated = 0
        for elem in root.iter():
            linkend = elem.get('linkend')
            if linkend and linkend in self.id_renames:
                elem.set('linkend', self.id_renames[linkend])
                updated += 1
            url = elem.get('url')
            if url and '#' in url:
                base, frag = url.split('#', 1)
                if frag in self.id_renames:
                    elem.set('url', f"{base}#{self.id_renames[frag]}")
                    updated += 1
        if updated:
            print(f"    Updated {updated} link references in {filename} based on ID renames")
        return updated

    def _fix_beginpage_position(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix beginpage elements that are in the wrong position relative to title.

        DTD rules differ by element type:
        - Chapter/preface/appendix/etc: beginpage comes BEFORE title
          <!ELEMENT chapter %ho; (beginpage?, chapterinfo?, (%bookcomponent.title.content;), ...
        - Sect1-5/section: beginpage is part of content mix, comes AFTER title
          <!ELEMENT sect1 (sect1info?, (title, subtitle?, titleabbrev?), ..., beginpage in content mix)
        """
        fixes = []

        # Book-component elements: beginpage comes BEFORE title
        book_component_tags = {'chapter', 'preface', 'appendix', 'dedication', 'glossary',
                               'bibliography', 'index', 'colophon', 'acknowledgments'}

        # Section elements: beginpage comes AFTER title (part of content mix)
        # Also includes sidebar which has optional title then content mix
        section_tags = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'sect6',
                        'sect7', 'sect8', 'sect9', 'sect10', 'section', 'sidebar'}

        for elem in root.iter():
            elem_name = self._local_name(elem)
            if not elem_name:
                continue

            title_elem = None
            for child in elem:
                if isinstance(child.tag, str) and self._local_name(child) == 'title':
                    title_elem = child
                    break

            if title_elem is None:
                continue

            # Find beginpage elements that are direct children
            beginpages = [child for child in elem
                         if isinstance(child.tag, str) and self._local_name(child) == 'beginpage']
            if not beginpages:
                continue

            children = list(elem)
            title_idx = children.index(title_elem)

            if elem_name in book_component_tags:
                # For book components: beginpage should be BEFORE title
                for bp in beginpages:
                    bp_idx = children.index(bp)
                    if bp_idx > title_idx:
                        elem.remove(bp)
                        children = list(elem)
                        title_idx = children.index(title_elem)
                        elem.insert(title_idx, bp)
                        fixes.append(f"Moved <beginpage> before <title> in <{elem_name}> in {filename}")

            elif elem_name in section_tags:
                # For sections: beginpage should be AFTER title
                for bp in beginpages:
                    children = list(elem)
                    bp_idx = children.index(bp)
                    title_idx = children.index(title_elem)
                    if bp_idx < title_idx:
                        elem.remove(bp)
                        children = list(elem)
                        title_idx = children.index(title_elem)
                        # Insert after title (and any subtitle/titleabbrev)
                        insert_pos = title_idx + 1
                        for i, child in enumerate(children[insert_pos:], start=insert_pos):
                            if isinstance(child.tag, str) and self._local_name(child) in ('subtitle', 'titleabbrev'):
                                insert_pos = i + 1
                            else:
                                break
                        elem.insert(insert_pos, bp)
                        fixes.append(f"Moved <beginpage> after <title> in <{elem_name}> in {filename}")

        return fixes

    def _fix_dedication_invalid_elements(self, root: etree._Element, filename: str) -> List[str]:
        """
        Remove or report invalid elements inside dedication.

        Per RittDoc DTD, dedication ONLY allows legalnotice.mix content:
        - glosslist, itemizedlist, orderedlist (lists)
        - caution, important, note, tip, warning (admonitions)
        - literallayout, programlisting, screen, synopsis, address (linespecific)
        - formalpara, para, simpara (paragraphs)
        - blockquote, indexterm, beginpage

        Elements NOT allowed (and cannot be fixed):
        - sect1, section, simplesect (structural)
        - figure, informalfigure, table, informaltable
        - sidebar, mediaobject, graphic, example, procedure, bridgehead
        """
        fixes = []

        # Elements that are NEVER valid in dedication
        invalid_elements = {
            'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect',
            'figure', 'informalfigure', 'table', 'informaltable',
            'sidebar', 'example', 'informalexample', 'procedure',
            'bridgehead', 'highlights', 'abstract', 'qandaset',
            'variablelist', 'simplelist', 'segmentedlist', 'calloutlist'
        }

        # Title elements (allowed)
        title_elements = {'title', 'subtitle', 'titleabbrev', 'risinfo'}

        for dedication in root.iter('dedication'):
            ded_id = dedication.get('id', 'unknown')
            elements_to_remove = []

            for child in list(dedication):
                if not isinstance(child.tag, str):
                    continue
                child_tag = self._local_name(child)

                # Skip allowed elements
                if child_tag in title_elements:
                    continue

                if child_tag in invalid_elements:
                    elements_to_remove.append(child)

            # Remove invalid elements and report
            for elem in elements_to_remove:
                elem_tag = self._local_name(elem)
                elem_id = elem.get('id', '')
                dedication.remove(elem)
                fixes.append(f"Removed invalid <{elem_tag}> from <dedication id='{ded_id}'> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=elem.sourceline if hasattr(elem, 'sourceline') else None,
                        fix_type="Dedication Invalid Element",
                        fix_description=f"Removed <{elem_tag}> element (not allowed in dedication)",
                        verification_reason=f"<{elem_tag}> is not in legalnotice.mix - dedication cannot contain this element type",
                        suggestion=f"MANUAL REVIEW: Content from <{elem_tag} id='{elem_id}'> was removed. May need to relocate."
                    ))

        return fixes

    def _fix_dedication_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix invalid content inside dedication elements.

        Per RittDoc DTD, dedication content model is:
          (risinfo?, (%sect.title.content;)?, (%legalnotice.mix;)+)

        legalnotice.mix does NOT include 'anchor', so anchors inside dedication must be wrapped.
        Also wraps indexterm elements for safety (though technically in legalnotice.mix as direct child).

        Strategy:
        1. Find anchor/indexterm elements that are direct children of dedication
        2. Wrap them in <para> elements (para is valid in legalnotice.mix)
        """
        fixes = []

        # Elements that need to be wrapped in para
        elements_to_wrap = {'anchor'}  # indexterm is in legalnotice.mix but safer in para

        for dedication in root.iter('dedication'):
            elements_needing_wrap = []
            for child in list(dedication):
                if isinstance(child.tag, str) and self._local_name(child) in elements_to_wrap:
                    elements_needing_wrap.append(child)

            for elem in elements_needing_wrap:
                elem_tag = self._local_name(elem)
                # Create a para element to wrap the element
                para = etree.Element('para')
                elem_idx = list(dedication).index(elem)
                dedication.remove(elem)
                para.append(elem)
                dedication.insert(elem_idx, para)
                fixes.append(f"Wrapped <{elem_tag}> in <para> inside <dedication> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                        fix_type="Dedication Content Fix",
                        fix_description=f"Wrapped {elem_tag} element in para inside dedication",
                        verification_reason=f"{elem_tag} is not valid directly inside dedication (not in legalnotice.mix)",
                        suggestion="No action needed - element preserved for cross-references."
                    ))

        return fixes

    def _fix_content_after_sections(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix block content appearing AFTER or BETWEEN section elements at all levels.

        Per DTD content models:
          - chapter/preface/appendix: (divcomponent.mix+, sect1*) | sect1+
          - sect1: (divcomponent.mix+, sect2*) | sect2+
          - sect2: (divcomponent.mix+, sect3*) | sect3+
          - sect3: (divcomponent.mix+, sect4*) | sect4+
          - sect4: (divcomponent.mix+, sect5*) | sect5+

        This means: Once child sections start, NO more block content (para, figure,
        table, anchor, etc.) can appear after them at the same level.

        Strategy:
        1. Process chapter/preface/appendix level (sect1 children) - existing logic
        2. Process sect1 level (sect2 children) - NEW
        3. Process sect2 level (sect3 children) - NEW
        4. Process sect3 level (sect4 children) - NEW
        5. Process sect4 level (sect5 children) - NEW

        For chapter/preface/appendix: create new sect1 for trailing content
        For nested sections: move trailing content into the last child section (simpler)
        """
        fixes = []

        # Block content elements from divcomponent.mix
        block_content_tags = {
            'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
            'screen', 'synopsis', 'address', 'blockquote', 'epigraph',
            'figure', 'informalfigure', 'table', 'informaltable',
            'example', 'informalexample', 'equation', 'informalequation',
            'procedure', 'sidebar', 'mediaobject', 'graphic',
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
            'segmentedlist', 'calloutlist', 'glosslist',
            'note', 'warning', 'caution', 'important', 'tip',
            'bridgehead', 'remark', 'highlights', 'abstract', 'authorblurb',
            'qandaset', 'anchor', 'indexterm', 'beginpage',
        }

        # Elements to skip (allowed after sections at chapter level)
        nav_tags = {'toc', 'lot', 'index', 'glossary', 'bibliography'}

        # Define the parent-child section relationships and their metadata
        # Format: (parent_tags, child_section_tags, metadata_tags, create_wrapper_for_trailing)
        section_levels = [
            # Level 1: chapter/preface/appendix -> sect1
            (
                {'chapter', 'preface', 'appendix'},
                {'sect1', 'simplesect', 'section', 'risempty'},
                {'title', 'subtitle', 'titleabbrev', 'chapterinfo', 'prefaceinfo',
                 'appendixinfo', 'beginpage', 'tocchap'},
                True  # Create new sect1 for trailing content
            ),
            # Level 2: sect1 -> sect2
            (
                {'sect1'},
                {'sect2', 'simplesect'},
                {'title', 'subtitle', 'titleabbrev', 'sect1info', 'beginpage'},
                False  # Move trailing content into last sect2
            ),
            # Level 3: sect2 -> sect3
            (
                {'sect2'},
                {'sect3', 'simplesect'},
                {'title', 'subtitle', 'titleabbrev', 'sect2info', 'beginpage'},
                False  # Move trailing content into last sect3
            ),
            # Level 4: sect3 -> sect4
            (
                {'sect3'},
                {'sect4', 'simplesect'},
                {'title', 'subtitle', 'titleabbrev', 'sect3info', 'beginpage'},
                False  # Move trailing content into last sect4
            ),
            # Level 5: sect4 -> sect5
            (
                {'sect4'},
                {'sect5', 'simplesect'},
                {'title', 'subtitle', 'titleabbrev', 'sect4info', 'beginpage'},
                False  # Move trailing content into last sect5
            ),
        ]

        for parent_tags, child_section_tags, meta_tags, create_wrapper in section_levels:
            fixes.extend(self._fix_content_after_sections_at_level(
                root, filename, block_content_tags, nav_tags,
                parent_tags, child_section_tags, meta_tags, create_wrapper
            ))

        return fixes

    def _fix_content_after_sections_at_level(
        self,
        root: etree._Element,
        filename: str,
        block_content_tags: set,
        nav_tags: set,
        parent_tags: set,
        child_section_tags: set,
        meta_tags: set,
        create_wrapper: bool
    ) -> List[str]:
        """
        Fix content model violations at a specific section level.

        Args:
            root: Root element of the XML tree
            filename: Name of the file being processed
            block_content_tags: Set of block content element tags
            nav_tags: Set of navigation element tags (allowed after sections)
            parent_tags: Set of parent element tags to process
            child_section_tags: Set of child section element tags
            meta_tags: Set of metadata element tags to skip
            create_wrapper: If True, create new wrapper section for trailing content;
                          if False, move trailing content into last child section
        """
        fixes = []

        # Build ID set once at the start (performance optimization)
        # This avoids O(n²) complexity from rebuilding inside nested loops
        existing_ids = {elem.get('id') for elem in root.iter() if elem.get('id')}

        for parent in root.iter():
            parent_name = self._local_name(parent)
            if parent_name not in parent_tags:
                continue

            # Keep fixing until no more issues (may need multiple passes)
            max_iterations = 100  # Safety limit
            iteration = 0

            while iteration < max_iterations:
                iteration += 1
                children = list(parent)
                if not children:
                    break

                # Find all child section positions
                section_positions = []
                for i, child in enumerate(children):
                    child_name = self._local_name(child)
                    if child_name in child_section_tags:
                        section_positions.append(i)

                if not section_positions:
                    break  # No child sections - nothing to fix

                first_sect_idx = section_positions[0]

                # Find content appearing BETWEEN sections or AFTER all sections
                content_to_move = []  # List of (index, element, target_section_or_None)

                for i in range(first_sect_idx + 1, len(children)):
                    child = children[i]
                    child_name = self._local_name(child)

                    # Skip allowed elements
                    if child_name in nav_tags or child_name in meta_tags:
                        continue

                    if child_name in child_section_tags:
                        continue  # This is a section, skip

                    if child_name in block_content_tags:
                        # Find which section this content is after
                        prev_sect = None
                        for sect_idx in section_positions:
                            if sect_idx < i:
                                prev_sect = children[sect_idx]
                            else:
                                break
                        content_to_move.append((i, child, prev_sect))

                if not content_to_move:
                    break  # Nothing to fix

                # Process content - move into preceding section or create new one
                # Group content by their target section
                content_by_target = defaultdict(list)
                trailing_content = []  # Content after all sections

                last_sect_idx = section_positions[-1]
                last_section = children[last_sect_idx]

                for i, elem, target_sect in content_to_move:
                    if i > last_sect_idx:
                        # This is trailing content (after all sections)
                        trailing_content.append((i, elem))
                    elif target_sect is not None:
                        # This is content between sections - move to preceding section
                        content_by_target[id(target_sect)].append((i, elem, target_sect))

                # Determine the child section tag name for logging
                child_sect_name = self._local_name(children[section_positions[0]])

                # Move content between sections into preceding section
                for sect_id, items in content_by_target.items():
                    for i, elem, target_sect in reversed(items):
                        parent.remove(elem)
                        target_sect.append(elem)
                        fixes.append(f"Moved <{self._local_name(elem)}> into preceding <{self._local_name(target_sect)}> in <{parent_name}> in {filename}")

                # Handle trailing content (after all sections)
                if trailing_content:
                    if create_wrapper:
                        # Create a new wrapper section (used for chapter/preface/appendi level)
                        parent_id = parent.get('id', 'unknown')

                        # Use pre-built existing_ids set (built once at method start)
                        new_sect1_id = next_available_sect1_id(parent_id, existing_ids)

                        new_sect1 = etree.Element('sect1')
                        new_sect1.set('id', new_sect1_id)
                        # Update the ID set to include the new ID
                        existing_ids.add(new_sect1_id)

                        # Add a title for the new sect1 (required by DTD)
                        title = etree.SubElement(new_sect1, 'title')
                        title_set = False

                        # Try to get title from parent element
                        parent_title_elem = parent.find('title')
                        if parent_title_elem is not None:
                            parent_title_text = ''.join(parent_title_elem.itertext()).strip()
                            if parent_title_text:
                                title.text = parent_title_text
                                for child in list(parent_title_elem):
                                    title.append(deepcopy(child))
                                title_set = True

                        # Fallback: Try to get title from first bridgehead
                        has_bridgehead_title = False
                        if not title_set:
                            for _, elem in trailing_content:
                                if self._local_name(elem) == 'bridgehead':
                                    title.text = elem.text or ''
                                    for child in list(elem):
                                        title.append(child)
                                    has_bridgehead_title = True
                                    title_set = True
                                    break

                        if not title_set:
                            title.text = ''  # Empty title as last resort

                        # Move trailing content into the new sect1
                        bridgehead_removed = False
                        for i, elem in reversed(trailing_content):
                            parent.remove(elem)
                            if has_bridgehead_title and self._local_name(elem) == 'bridgehead' and not bridgehead_removed:
                                bridgehead_removed = True
                                continue
                            new_sect1.append(elem)

                        # Reorder children in new_sect1 so title is first
                        sect1_children = list(new_sect1)
                        new_sect1[:] = []
                        new_sect1.append(title)
                        for child in sect1_children:
                            if child is not title:
                                new_sect1.append(child)

                        # Insert new sect1 after the last existing section
                        children = list(parent)
                        insert_pos = len(children)
                        for i, child in enumerate(children):
                            if self._local_name(child) in nav_tags:
                                insert_pos = i
                                break

                        parent.insert(insert_pos, new_sect1)

                        fixes.append(f"Wrapped {len(trailing_content)} trailing content elements in new <sect1 id='{new_sect1_id}'> in <{parent_name}> in {filename}")

                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=new_sect1.sourceline if hasattr(new_sect1, 'sourceline') else None,
                                fix_type="Content After Sections Fix",
                                fix_description=f"Created new sect1 to wrap {len(trailing_content)} content elements appearing after sections",
                                verification_reason="DTD requires block content before sections, not after. Content appearing after sect1 is invalid.",
                                suggestion="Review the new section - it may need a proper title or the content may need restructuring."
                            ))
                    else:
                        # Move trailing content into the last child section (simpler approach)
                        # This is used for nested sections (sect1->sect2, sect2->sect3, etc.)
                        for i, elem in reversed(trailing_content):
                            parent.remove(elem)
                            last_section.append(elem)

                        fixes.append(f"Moved {len(trailing_content)} trailing content elements into last <{self._local_name(last_section)}> in <{parent_name}> in {filename}")

                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=last_section.sourceline if hasattr(last_section, 'sourceline') else None,
                                fix_type="Content After Sections Fix",
                                fix_description=f"Moved {len(trailing_content)} content elements into last {self._local_name(last_section)}",
                                verification_reason=f"DTD requires block content before {child_sect_name} elements, not after.",
                                suggestion="Content was moved to last child section for DTD compliance."
                            ))

                # If we moved content between sections only, continue to check for more
                if not trailing_content and content_by_target:
                    continue
                else:
                    break

        return fixes

    def _fix_nested_figures(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix nested figure elements by unwrapping them.

        Per DTD, figure.mix does NOT include figure, so nested figures are invalid:
          figure = (blockinfo?, title, (figure.mix | link.char.class)+)
          figure.mix = linespecific.class | synop.class | informal.class | ndxterm.class | beginpage

        Strategy: Move nested figures to be siblings of their parent figure.
        """
        fixes = []

        # Keep processing until no more nested figures found (handles deeply nested)
        while True:
            nested_found = False

            for outer_figure in list(root.iter('figure')):
                # Find any figure children (direct or nested)
                for inner_figure in list(outer_figure.iter('figure')):
                    if inner_figure is outer_figure:
                        continue  # Skip self

                    nested_found = True
                    parent_of_outer = outer_figure.getparent()
                    if parent_of_outer is None:
                        continue

                    # Get position of outer figure in its parent
                    outer_idx = list(parent_of_outer).index(outer_figure)

                    # Remove inner figure from wherever it is
                    inner_parent = inner_figure.getparent()
                    if inner_parent is not None:
                        inner_parent.remove(inner_figure)

                    # Insert as sibling after outer figure
                    parent_of_outer.insert(outer_idx + 1, inner_figure)
                    fixes.append(f"Unwrapped nested <figure> in {filename}")

            if not nested_found:
                break

        return fixes

    def _fix_nested_sidebars(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix nested sidebar elements by flattening them into the parent sidebar.

        Per DTD, sidebar content model does not include sidebar itself, so nested
        sidebars are invalid. Preserve the inner sidebar title as a bridgehead
        and move its content up to the parent.
        """
        fixes = []

        for outer_sidebar in list(root.iter('sidebar')):
            for inner_sidebar in list(outer_sidebar.iter('sidebar')):
                if inner_sidebar is outer_sidebar:
                    continue

                parent = inner_sidebar.getparent()
                if parent is None:
                    continue

                insert_index = list(parent).index(inner_sidebar)
                inner_tail = inner_sidebar.tail

                title_elem = None
                titleabbrev_elem = None
                for child in list(inner_sidebar):
                    child_name = self._local_name(child)
                    if child_name == 'title':
                        title_elem = child
                    elif child_name == 'titleabbrev':
                        titleabbrev_elem = child

                if title_elem is not None:
                    bridgehead = etree.Element('bridgehead')
                    bridgehead.text = title_elem.text
                    for child in list(title_elem):
                        bridgehead.append(deepcopy(child))
                    inner_sidebar.remove(title_elem)
                    parent.insert(insert_index, bridgehead)
                    insert_index += 1

                if titleabbrev_elem is not None:
                    inner_sidebar.remove(titleabbrev_elem)

                for child in list(inner_sidebar):
                    child_name = self._local_name(child)
                    if child_name in {'title', 'titleabbrev'}:
                        inner_sidebar.remove(child)
                        continue
                    if child_name == 'sidebarinfo':
                        info_text = ''.join(child.itertext()).strip()
                        inner_sidebar.remove(child)
                        if info_text:
                            info_para = etree.Element('para')
                            info_para.text = info_text
                            parent.insert(insert_index, info_para)
                            insert_index += 1
                        continue

                    inner_sidebar.remove(child)
                    parent.insert(insert_index, child)
                    insert_index += 1

                parent.remove(inner_sidebar)
                if inner_tail:
                    if insert_index > 0 and len(parent) >= insert_index:
                        prev = parent[insert_index - 1]
                        prev.tail = (prev.tail or '') + inner_tail
                    else:
                        parent.text = (parent.text or '') + inner_tail

                fixes.append(f"Flattened nested <sidebar> in {filename}")

        return fixes

    def _fix_nested_para_elements(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix nested para elements by intelligently unwrapping or flattening them.

        Strategy:
        1. If nested para contains ONLY inline elements (links, emphasis, etc.):
           -> Unwrap it: merge its content into the parent para
        2. If nested para contains block elements (lists, tables, etc.):
           -> Flatten it: create sibling para elements

        This preserves links and formatting while fixing DTD violations.
        """
        fixes = []
        # Find all para elements that contain other para elements
        for para in list(root.iter('para')):
            nested_paras = [child for child in para if isinstance(child.tag, str) and self._local_name(child) == "para"]

            if not nested_paras:
                continue

            parent = para.getparent()
            if parent is None:
                continue

            # Check if we can unwrap (inline-only) or need to flatten (has block content)
            for nested_para in nested_paras:
                if self._is_inline_only(nested_para):
                    # UNWRAP: Nested para has only inline content (links, emphasis, etc.)
                    # Move its content directly into parent para, preserving all inline elements

                    # Get position of nested para within parent para
                    nested_index = list(para).index(nested_para)

                    # Insert nested para's children before it
                    for i, child in enumerate(list(nested_para)):
                        para.insert(nested_index + i, child)

                    # Handle text content
                    if nested_para.text:
                        if nested_index > 0:
                            # Add to previous sibling's tail
                            prev = para[nested_index - 1]
                            prev.tail = (prev.tail or '') + nested_para.text
                        else:
                            # Add to parent para's text
                            para.text = (para.text or '') + nested_para.text

                    # Handle tail text (text after nested para)
                    if nested_para.tail:
                        # Find last inserted child and append to its tail
                        if len(nested_para) > 0:
                            last_child = para[nested_index + len(nested_para) - 1]
                            last_child.tail = (last_child.tail or '') + nested_para.tail
                        elif nested_index > 0:
                            prev = para[nested_index - 1]
                            prev.tail = (prev.tail or '') + nested_para.tail
                        else:
                            para.text = (para.text or '') + nested_para.tail

                    # Remove the nested para element itself
                    para.remove(nested_para)
                    fixes.append(f"Unwrapped inline-only nested para in {filename}")

                else:
                    # FLATTEN: Nested para has block content - create sibling paras
                    para_index = list(parent).index(para)

                    # Create new para element at parent level
                    new_para = etree.Element("para")
                    if nested_para.get("id"):
                        new_para.set("id", nested_para.get("id"))

                    # Copy text and children
                    new_para.text = nested_para.text
                    for child in nested_para:
                        new_para.append(deepcopy(child))

                    # Insert after current para
                    para_index += 1
                    parent.insert(para_index, new_para)

                    # Handle tail text: if there's text after the nested para,
                    # wrap it in a new para element
                    if nested_para.tail and nested_para.tail.strip():
                        tail_para = etree.Element("para")
                        tail_para.text = nested_para.tail
                        para_index += 1
                        parent.insert(para_index, tail_para)

                    # Remove from original para
                    para.remove(nested_para)
                    fixes.append(f"Flattened block-content nested para in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                            fix_type="Nested Para Fix",
                            fix_description="Fixed nested para element",
                            verification_reason="Nested para elements are not allowed per DTD.",
                            suggestion="Verify that content and links are preserved correctly."
                        ))

        return fixes

    def _remove_empty_mediaobjects(self, root: etree._Element, filename: str) -> List[str]:
        """Remove empty/placeholder mediaobjects from anywhere in the document"""
        fixes = []

        # Find all mediaobjects
        for mediaobj in list(root.iter('mediaobject')):
            # Check if this is an empty/placeholder mediaobject
            is_placeholder = False
            has_text = False
            has_meaningful_text = False
            placeholder_patterns = (
                r'not available',
                r'no image',
                r'image not available',
                r'^\s*n/a\s*$',
            )

            # Check for "Image not available" or similar placeholder text
            for textobj in mediaobj.iter('textobject'):
                for text_node in textobj.itertext():
                    if not text_node:
                        continue
                    text_val = text_node.strip()
                    if not text_val:
                        continue
                    has_text = True
                    if any(re.search(pattern, text_val, re.IGNORECASE) for pattern in placeholder_patterns):
                        is_placeholder = True
                    else:
                        has_meaningful_text = True

            # Also check if mediaobject has no real content (no imagedata/videodata/audiodata)
            has_real_media = (mediaobj.find('.//imagedata') is not None or
                            mediaobj.find('.//videodata') is not None or
                            mediaobj.find('.//audiodata') is not None)

            if has_real_media:
                continue

            parent = mediaobj.getparent()
            parent_tag = self._local_name(parent) if parent is not None else ''
            parent_id = parent.get('id') if parent is not None else ''
            keep_with_placeholder = parent_tag in {'figure', 'informalfigure'} or bool(parent_id)

            if keep_with_placeholder or has_meaningful_text:
                # Preserve mediaobject and add a placeholder image if missing.
                imageobject = mediaobj.find('imageobject')
                if imageobject is None:
                    imageobject = etree.SubElement(mediaobj, 'imageobject')
                imagedata = imageobject.find('imagedata')
                if imagedata is None:
                    imagedata = etree.SubElement(imageobject, 'imagedata')
                if not imagedata.get('fileref'):
                    imagedata.set('fileref', 'missing_media.jpg')
                if not has_meaningful_text:
                    textobject = mediaobj.find('textobject')
                    if textobject is None:
                        textobject = etree.SubElement(mediaobj, 'textobject')
                    if textobject.find('para') is None:
                        tpara = etree.SubElement(textobject, 'para')
                        tpara.text = "[Media not available]"
                fixes.append(f"Added placeholder media to mediaobject in <{parent_tag}> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=mediaobj.sourceline if hasattr(mediaobj, 'sourceline') else None,
                        fix_type="Missing Media Placeholder",
                        fix_description="Added placeholder media to preserve structure",
                        verification_reason="Mediaobject had no real media; kept to preserve IDs and DTD structure.",
                        suggestion="Verify the missing media and replace placeholder."
                    ))
                continue

            if is_placeholder or not has_text:
                if parent is not None:
                    parent.remove(mediaobj)
                    fixes.append(f"Removed empty/placeholder mediaobject from <{parent_tag}> in {filename}")

                    # Check if parent is now empty and remove it too
                    parent_has_content = (
                        len(parent) > 0 or
                        (parent.text and parent.text.strip()) or
                        any(child.tail and child.tail.strip() for child in parent)
                    )
                    if not parent_has_content:
                        grandparent = parent.getparent()
                        if grandparent is not None:
                            grandparent.remove(parent)
                            fixes.append(f"Removed empty parent <{parent_tag}> after mediaobject removal in {filename}")

                            if VALIDATION_REPORT_AVAILABLE:
                                self.verification_items.append(VerificationItem(
                                    xml_file=filename,
                                    line_number=grandparent.sourceline if hasattr(grandparent, 'sourceline') else None,
                                    fix_type="Empty Parent Removal",
                                    fix_description=f"Removed empty <{parent_tag}> element after mediaobject removal",
                                    verification_reason="Parent element had no remaining content after mediaobject was removed.",
                                    suggestion="Verify document structure is still correct."
                                ))
                    else:
                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=parent.sourceline if hasattr(parent, 'sourceline') else None,
                                fix_type="Empty Mediaobject Removal",
                                fix_description="Removed placeholder mediaobject with no real media content",
                                verification_reason="Mediaobject contained only placeholder text or no media.",
                                suggestion="Verify that removing this mediaobject didn't affect document structure."
                            ))

        return fixes

    def _remove_misclassified_table_figures(self, root: etree._Element, filename: str) -> List[str]:
        """Convert or remove invalid figure elements"""
        fixes = []

        # Process all figures
        for figure in list(root.iter('figure')):
            title_elem = figure.find('title')

            # Check if figure has real media content
            has_real_image = (figure.find('.//imagedata') is not None or
                             figure.find('.//videodata') is not None or
                             figure.find('.//audiodata') is not None)

            # Check for placeholder text
            has_placeholder = False
            for phrase in figure.iter('phrase'):
                if phrase.text and 'not available' in phrase.text.lower():
                    has_placeholder = True
                    break

            # Skip figures that have real media content
            if has_real_image and not has_placeholder:
                continue

            # Get title text (if any)
            title_text = ''
            if title_elem is not None:
                title_text = ''.join(title_elem.itertext()).strip()
            figure_id = figure.get('id', '')
            has_id = bool(figure_id)

            # Case 1: Figure with no title or empty/meaningless title - REMOVE completely
            if not title_text or title_text.lower() in ['untitled', 'no title', 'n/a']:
                if has_id:
                    # Preserve the figure to keep ID targets valid; add placeholder media.
                    mediaobject = figure.find('mediaobject')
                    if mediaobject is None:
                        mediaobject = etree.SubElement(figure, 'mediaobject')
                    imageobject = mediaobject.find('imageobject')
                    if imageobject is None:
                        imageobject = etree.SubElement(mediaobject, 'imageobject')
                    imagedata = imageobject.find('imagedata')
                    if imagedata is None:
                        imagedata = etree.SubElement(imageobject, 'imagedata')
                    if not imagedata.get('fileref'):
                        imagedata.set('fileref', 'missing_image.jpg')
                    fixes.append(f"Added placeholder media to empty figure '{figure_id}' in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                            fix_type="Missing Figure Media",
                            fix_description="Added placeholder media to preserve referenced figure",
                            verification_reason="Figure had no real media content but carried an ID.",
                            suggestion="Locate and add the missing image file."
                        ))
                else:
                    parent = figure.getparent()
                    if parent is not None:
                        parent.remove(figure)
                        fixes.append(f"Removed empty figure with no meaningful title in {filename}")

                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                                fix_type="Empty Figure Removal",
                                fix_description="Removed figure with no media and no meaningful title",
                                verification_reason="Figure had no real media content and title was empty or 'Untitled'.",
                                suggestion="Verify this empty figure was not needed."
                            ))

            # Case 2: Figure with "table" in title - CONVERT to para
            elif 'table' in title_text.lower():
                parent = figure.getparent()
                if parent is not None:
                    # Get figure's position in parent
                    fig_index = list(parent).index(figure)
                    table = etree.Element('table')
                    for attr, value in figure.attrib.items():
                        table.set(attr, value)
                    title = etree.SubElement(table, 'title')
                    title.text = title_text

                    # Populate minimal table structure to remain DTD-compliant.
                    tgroup = etree.SubElement(table, 'tgroup', cols='1')
                    tbody = etree.SubElement(tgroup, 'tbody')
                    row = etree.SubElement(tbody, 'row')
                    entry = etree.SubElement(row, 'entry')
                    entry_text = ''
                    textobject = figure.find('.//textobject')
                    if textobject is not None:
                        entry_text = ''.join(textobject.itertext()).strip()
                    entry.text = entry_text or "[Table content missing]"

                    parent.insert(fig_index, table)
                    parent.remove(figure)

                    fixes.append(f"Converted misclassified figure (table label) to table in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                            fix_type="Misclassified Figure Conversion",
                            fix_description=f"Converted figure with 'table' in title to table: '{title_text[:60]}'",
                            verification_reason="Figure had 'table' in title but no real image content.",
                            suggestion="Verify table content or replace placeholder."
                        ))

            # Case 3: Figure with other title but no media
            # Be CONSERVATIVE: If figure has an ID that looks like a valid figure reference,
            # don't remove it - just add a placeholder mediaobject
            else:
                # Check if this looks like an intentional figure (has fig/figure in ID)
                is_intentional_figure = bool(figure_id) or bool(re.search(r'fig|figure', figure_id, re.IGNORECASE))

                if is_intentional_figure:
                    # Don't remove - add placeholder mediaobject to make it DTD-compliant
                    mediaobject = figure.find('mediaobject')
                    if mediaobject is None:
                        mediaobject = etree.SubElement(figure, 'mediaobject')
                        imageobject = etree.SubElement(mediaobject, 'imageobject')
                        imagedata = etree.SubElement(imageobject, 'imagedata')
                        imagedata.set('fileref', 'missing_image.jpg')
                        # Add textobject to indicate missing image
                        textobject = etree.SubElement(mediaobject, 'textobject')
                        phrase = etree.SubElement(textobject, 'phrase')
                        phrase.text = f"[Image not available: {title_text[:60]}]"
                        fixes.append(f"Added placeholder for figure '{figure_id}' with missing media in {filename}")

                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                                fix_type="Missing Figure Media",
                                fix_description=f"Added placeholder for figure with missing media: '{title_text[:60]}'",
                                verification_reason="Figure appears intentional but had no media content.",
                                suggestion="Locate and add the missing image file."
                            ))
                else:
                    # Not an intentional figure - remove it
                    parent = figure.getparent()
                    if parent is not None:
                        parent.remove(figure)
                        fixes.append(f"Removed empty figure '{title_text[:40]}' with no media in {filename}")

                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                                fix_type="Empty Figure Removal",
                                fix_description=f"Removed figure with no media: '{title_text[:60]}'",
                                verification_reason="Figure had no real media content and ID didn't indicate an intentional figure.",
                                suggestion="Verify this figure was not needed or check if media is missing."
                            ))

        return fixes

    def _fix_invalid_content_models(self, root: etree._Element, filename: str) -> List[str]:
        """Fix elements with invalid content model (wrong child sequence)"""
        fixes = []

        # Fix chapters with disallowed content as direct children (need sect1 wrapper)
        # GENERIC APPROACH: Use whitelist per ritthier2.mod DTD
        # Allowed direct children: beginpage, chapterinfo, title, subtitle, titleabbrev, tocchap,
        #                         toc, lot, index, glossary, bibliography, sect1
        ALLOWED_CHAPTER_CHILDREN = {
            'beginpage', 'chapterinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap',
            'toc', 'lot', 'index', 'glossary', 'bibliography', 'sect1'
        }

        for chapter in root.iter():
            if self._local_name(chapter) != 'chapter':
                continue

            violating_elements = []

            # Capture any disallowed direct children (regardless of position)
            for child in list(chapter):
                if not isinstance(child.tag, str):
                    continue
                child_tag = self._local_name(child)
                if child_tag not in ALLOWED_CHAPTER_CHILDREN:
                    violating_elements.append(child)

            # If we have violating elements, handle them appropriately
            if violating_elements:
                chapter_id = chapter.get('id', 'chapter')

                # Special case: if ALL violating elements are anchors, try to move them
                # to the correct section based on their ID prefix, or first sect1 as fallback
                all_anchors = all(self._local_name(elem) == 'anchor' for elem in violating_elements)
                first_sect1 = None
                for child in chapter:
                    if self._local_name(child) == 'sect1':
                        first_sect1 = child
                        break

                if all_anchors and first_sect1 is not None:
                    first_sect1_id = first_sect1.get('id', '')
                    moved_count = 0
                    removed_count = 0
                    removed_anchor_ids = []  # Track removed anchors for verification item cleanup

                    for elem in list(violating_elements):
                        anchor_id = elem.get('id', '')
                        # Extract section ID from anchor ID (first 11 chars like ch0021s0014)
                        section_id_match = re.match(r'^([a-z]{2}\d{4}s\d{4})', anchor_id)

                        if section_id_match:
                            section_id = section_id_match.group(1)
                            # Try to find the matching section or bibliography
                            target_section = chapter.find(f'.//*[@id="{section_id}"]')

                            if target_section is not None:
                                # Move anchor to its correct section
                                chapter.remove(elem)
                                title_elem = None
                                for t_child in target_section:
                                    if self._local_name(t_child) == 'title':
                                        title_elem = t_child
                                        break
                                if title_elem is not None:
                                    title_index = list(target_section).index(title_elem)
                                    target_section.insert(title_index + 1, elem)
                                else:
                                    target_section.insert(0, elem)
                                moved_count += 1
                            elif section_id == first_sect1_id:
                                # Anchor belongs to first sect1, move it there
                                chapter.remove(elem)
                                sect1_title = None
                                for t_child in first_sect1:
                                    if self._local_name(t_child) == 'title':
                                        sect1_title = t_child
                                        break
                                insert_pos = 0
                                if sect1_title is not None:
                                    insert_pos = list(first_sect1).index(sect1_title) + 1
                                first_sect1.insert(insert_pos, elem)
                                moved_count += 1
                            else:
                                # Section doesn't exist - remove the orphan anchor
                                # (better than having anchor with wrong section ID in first sect1)
                                chapter.remove(elem)
                                removed_count += 1
                                removed_anchor_ids.append(anchor_id)
                                print(f"    Removed orphan anchor '{anchor_id}' (section {section_id} not found) in {filename}")
                        else:
                            # Anchor ID doesn't have section format, move to first sect1
                            chapter.remove(elem)
                            sect1_title = None
                            for t_child in first_sect1:
                                if self._local_name(t_child) == 'title':
                                    sect1_title = t_child
                                    break
                            insert_pos = 0
                            if sect1_title is not None:
                                insert_pos = list(first_sect1).index(sect1_title) + 1
                            first_sect1.insert(insert_pos, elem)
                            moved_count += 1

                    if moved_count > 0:
                        fixes.append(f"Moved {moved_count} anchor(s) to correct sections in chapter {chapter_id}")
                    if removed_count > 0:
                        fixes.append(f"Removed {removed_count} orphan anchor(s) in chapter {chapter_id}")

                        # Clean up stale verification items for removed anchors
                        # These were added by _create_missing_ids_in_chapter() but the anchors
                        # are now removed as orphans - the verification items are misleading
                        if VALIDATION_REPORT_AVAILABLE and removed_anchor_ids:
                            self.verification_items = [
                                item for item in self.verification_items
                                if not (item.fix_type == "Anchor Creation for Missing ID"
                                        and any(f"id='{aid}'" in item.fix_description for aid in removed_anchor_ids))
                            ]

                    if VALIDATION_REPORT_AVAILABLE and (moved_count > 0 or removed_count > 0):
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=chapter.sourceline if hasattr(chapter, 'sourceline') else None,
                            fix_type="Content Model Fix",
                            fix_description=f"Processed {moved_count + removed_count} anchor(s): {moved_count} moved, {removed_count} removed",
                            verification_reason="Anchors were at chapter level. Moved to correct sections or removed if orphaned.",
                            suggestion="Verify cross-references still work correctly."
                        ))
                else:
                    # Standard case: wrap violating elements in a new sect1

                    # Synthetic wrapper sections should always use the chapter title.
                    # Avoid deriving titles from leading content (e.g., drop-cap "F").
                    chapter_title_elem = None
                    for child in chapter:
                        if self._local_name(child) == 'title':
                            chapter_title_elem = child
                            break
                    section_title = ''
                    if chapter_title_elem is not None:
                        section_title = ''.join(chapter_title_elem.itertext()).strip()

                    # Create wrapper sect1
                    sect1 = etree.Element('sect1')
                    existing_ids = [e.get('id') for e in chapter.iter() if e.get('id')]
                    sect1_id = next_available_sect1_id(chapter_id, existing_ids)
                    sect1.set('id', sect1_id)

                    # Add title - always use chapter title (may be empty).
                    title = etree.Element('title')
                    title.text = section_title
                    sect1.append(title)

                    # Move violating elements into sect1
                    for elem in violating_elements:
                        chapter.remove(elem)
                        sect1.append(elem)

                    # Insert sect1 after title and header elements
                    insert_index = 0
                    for i, child in enumerate(chapter):
                        child_name = self._local_name(child)
                        if child_name in ['beginpage', 'chapterinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap']:
                            insert_index = i + 1

                    chapter.insert(insert_index, sect1)

                    # Update child element IDs to use the new wrapper sect1 ID prefix
                    # This ensures IDs like ch0010s0000a0003 become ch0010s0006a0003
                    id_update_fixes, id_mapping = self._update_child_ids_for_wrapper(
                        sect1, chapter_id, sect1_id, root
                    )
                    fixes.extend(id_update_fixes)
                    if id_mapping:
                        # Use compose-aware batch add to properly handle chained renames
                        self._add_id_renames_batch(id_mapping)

                    fixes.append(f"Wrapped {len(violating_elements)} violating elements in <sect1> in chapter {chapter_id}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=chapter.sourceline if hasattr(chapter, 'sourceline') else None,
                            fix_type="Content Model Fix",
                            fix_description=f"Wrapped {len(violating_elements)} elements in <sect1 id=\"{sect1_id}\">",
                            verification_reason="Section wrapper was auto-created using chapter title.",
                            suggestion="Review content structure if needed."
                        ))

        # Fix preface elements with disallowed content as direct children (need sect1 wrapper)
        # Allowed direct children per DTD: beginpage, prefaceinfo, title, subtitle, titleabbrev, tocchap,
        #                                  toc, lot, index, glossary, bibliography, sect1, and block content
        # BUT: footnote is NOT allowed as direct child - needs to be inside para
        ALLOWED_PREFACE_CHILDREN = {
            'beginpage', 'prefaceinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap',
            'toc', 'lot', 'index', 'glossary', 'bibliography', 'sect1', 'risempty',
            'simplesect', 'section',
            # Block content from divcomponent.mix
            'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
            'screen', 'synopsis', 'address', 'blockquote', 'epigraph',
            'figure', 'informalfigure', 'table', 'informaltable',
            'example', 'informalexample', 'equation', 'informalequation',
            'procedure', 'sidebar', 'mediaobject', 'graphic',
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
            'segmentedlist', 'calloutlist', 'glosslist',
            'note', 'warning', 'caution', 'important', 'tip',
            'bridgehead', 'remark', 'highlights', 'abstract', 'authorblurb',
            'qandaset', 'anchor', 'indexterm', 'beginpage',
        }

        for preface in root.iter():
            if self._local_name(preface) != 'preface':
                continue

            violating_elements = []

            # Capture any disallowed direct children (e.g., footnote)
            for child in list(preface):
                if not isinstance(child.tag, str):
                    continue
                child_tag = self._local_name(child)
                if child_tag not in ALLOWED_PREFACE_CHILDREN:
                    violating_elements.append(child)

            if violating_elements:
                preface_id = preface.get('id', 'preface')

                # For footnotes: move them into the previous para
                footnotes_moved = 0
                for elem in list(violating_elements):
                    if self._local_name(elem) == 'footnote':
                        idx = list(preface).index(elem)
                        # Find previous para to move footnote into
                        prev_para = None
                        for i in range(idx - 1, -1, -1):
                            sibling = preface[i]
                            if self._local_name(sibling) == 'para':
                                prev_para = sibling
                                break

                        if prev_para is not None:
                            preface.remove(elem)
                            prev_para.append(elem)
                            violating_elements.remove(elem)
                            footnotes_moved += 1
                        else:
                            # Wrap footnote in para
                            wrapper_para = etree.Element('para')
                            preface.remove(elem)
                            wrapper_para.append(elem)
                            preface.insert(idx, wrapper_para)
                            violating_elements.remove(elem)
                            footnotes_moved += 1

                if footnotes_moved > 0:
                    fixes.append(f"Moved {footnotes_moved} footnote(s) into para in preface {preface_id}")

                # If there are still violating elements, wrap them in sect1
                if violating_elements:
                    preface_title_elem = None
                    for child in preface:
                        if self._local_name(child) == 'title':
                            preface_title_elem = child
                            break
                    section_title = ''
                    if preface_title_elem is not None:
                        section_title = ''.join(preface_title_elem.itertext()).strip()

                    # Create wrapper sect1
                    sect1 = etree.Element('sect1')
                    existing_ids = [e.get('id') for e in preface.iter() if e.get('id')]
                    sect1_id = next_available_sect1_id(preface_id, existing_ids)
                    sect1.set('id', sect1_id)

                    # Add title
                    title = etree.Element('title')
                    title.text = section_title
                    sect1.append(title)

                    # Move violating elements into sect1
                    for elem in violating_elements:
                        preface.remove(elem)
                        sect1.append(elem)

                    # Insert sect1 after title and header elements
                    insert_index = 0
                    for i, child in enumerate(preface):
                        child_name = self._local_name(child)
                        if child_name in ['beginpage', 'prefaceinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap']:
                            insert_index = i + 1

                    preface.insert(insert_index, sect1)
                    fixes.append(f"Wrapped {len(violating_elements)} violating elements in <sect1> in preface {preface_id}")

        # Fix lists that need listitem children
        for list_elem in root.iter('orderedlist', 'itemizedlist'):
            # Check if list has any non-listitem children
            for child in list_elem:
                if child.tag not in ['listitem', 'title']:
                    # Wrap in listitem
                    listitem = etree.Element('listitem')
                    list_elem.remove(child)
                    listitem.append(child)
                    list_elem.append(listitem)
                    fixes.append(f"Wrapped <{child.tag}> in <listitem> in {filename}")

        return fixes

    def _fix_empty_elements(self, root: etree._Element, filename: str) -> List[str]:
        """Fix elements that shouldn't be empty"""
        # Note: DTD allows empty elements for title, para, and term
        # (all defined with * meaning zero or more content)
        # No placeholder text needed - empty elements are valid
        return []

    def _remove_empty_rows(self, root: etree._Element, filename: str) -> List[str]:
        """Remove empty row elements from tables"""
        fixes = []

        # Find all row elements
        for row in list(root.iter('row')):
            # Check if row has no entry children (completely empty)
            entries = list(row.iter('entry'))
            if len(entries) == 0:
                parent = row.getparent()
                if parent is not None:
                    parent.remove(row)
                    fixes.append(f"Removed empty <row/> element in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=row.sourceline if hasattr(row, 'sourceline') else None,
                            fix_type="Empty Row Removal",
                            fix_description="Removed row element with no entry children",
                            verification_reason="Row elements must contain at least one entry element per DTD.",
                            suggestion="Check source EPUB for empty table rows."
                        ))

        return fixes

    def _fix_empty_table_bodies(self, root: etree._Element, filename: str) -> List[str]:
        """
        Ensure table bodies contain at least one row.

        DTD requires row+ inside tbody; empty tbody triggers validation errors.
        If a table has no rows after cleanup, add a minimal placeholder row/entry.

        NOTE: DTD allows tables to use EITHER tgroup+ OR mediaobject+/graphic+
        content model. Tables using mediaobject/graphic should NOT have tgroup added.
        """
        fixes = []

        for table in root.iter('table'):
            # Skip tables that use mediaobject or graphic content model
            # DTD: table content is (tgroup+) | (mediaobject+) | (graphic+)
            # Adding tgroup to a mediaobject-based table would violate the DTD
            has_media = any(
                isinstance(child.tag, str) and self._local_name(child) in ('mediaobject', 'graphic')
                for child in table
            )
            if has_media:
                continue  # Valid table using mediaobject/graphic content model

            # Find first tgroup or create if missing
            tgroup = None
            for child in table:
                if isinstance(child.tag, str) and self._local_name(child) == 'tgroup':
                    tgroup = child
                    break
            if tgroup is None:
                tgroup = etree.SubElement(table, 'tgroup', cols='1')

            # Find tbody or create
            tbody = None
            for child in tgroup:
                if isinstance(child.tag, str) and self._local_name(child) == 'tbody':
                    tbody = child
                    break
            if tbody is None:
                tbody = etree.SubElement(tgroup, 'tbody')

            # Check for existing rows
            has_row = any(
                isinstance(child.tag, str) and self._local_name(child) == 'row'
                for child in tbody
            )
            if has_row:
                continue

            # Add placeholder row/entry
            row = etree.SubElement(tbody, 'row')
            entry = etree.SubElement(row, 'entry')
            para = etree.SubElement(entry, 'para')
            para.text = "[Table content missing]"

            fixes.append(f"Added placeholder row to empty <tbody> in {filename}")

        return fixes

    def _fix_missing_required_attributes(self, root: etree._Element, filename: str) -> List[str]:
        """Fix elements missing required attributes"""
        fixes = []

        # Table tgroup requires cols attribute
        for tgroup in root.iter('tgroup'):
            if 'cols' not in tgroup.attrib:
                # Count actual columns from first row
                cols = 0
                for tbody in tgroup.iter('tbody'):
                    for row in tbody.iter('row'):
                        cols = len(list(row.iter('entry')))
                        break
                    break

                if cols == 0:
                    # Try thead
                    for thead in tgroup.iter('thead'):
                        for row in thead.iter('row'):
                            cols = len(list(row.iter('entry')))
                            break
                        break

                if cols == 0:
                    cols = 1  # Default

                tgroup.set('cols', str(cols))
                fixes.append(f"Added cols=\"{cols}\" to <tgroup> in {filename}")

        # Imagedata should have fileref
        for imagedata in root.iter('imagedata'):
            if 'fileref' not in imagedata.attrib:
                # Try to find nearby file reference or use placeholder
                imagedata.set('fileref', 'image-placeholder.png')
                fixes.append(f"Added placeholder fileref to <imagedata> in {filename}")

        return fixes

    def _fix_invalid_elements(self, root: etree._Element, filename: str) -> List[str]:
        """Remove or convert invalid/undeclared elements"""
        fixes = []

        # Common invalid elements to remove or convert
        invalid_to_remove = ['html', 'body', 'div', 'span', 'br', 'hr', 'style', 'script']
        invalid_to_para = ['p']  # Convert <p> to <para>

        # Remove invalid elements but keep their content
        for elem_name in invalid_to_remove:
            for elem in root.iter(elem_name):
                # Preserve text content
                if elem.text:
                    # Try to add text to previous sibling
                    parent = elem.getparent()
                    if parent is not None:
                        index = list(parent).index(elem)
                        if index > 0:
                            prev = parent[index - 1]
                            if prev.tail:
                                prev.tail += elem.text
                            else:
                                prev.tail = elem.text

                # Move children up to parent
                parent = elem.getparent()
                if parent is not None:
                    index = list(parent).index(elem)
                    for child in reversed(list(elem)):
                        elem.remove(child)
                        parent.insert(index + 1, child)

                    parent.remove(elem)
                    fixes.append(f"Removed invalid element <{elem_name}> in {filename}")

        # Convert <p> to <para>
        for p_elem in root.iter('p'):
            p_elem.tag = 'para'
            fixes.append(f"Converted <p> to <para> in {filename}")

        return fixes

    def _normalize_whitespace(self, root: etree._Element, filename: str) -> List[str]:
        """Normalize whitespace in text content.

        IMPORTANT: Preserves trailing whitespace in elem.text when the element
        has child elements, because that trailing space serves as the separator
        between the parent's text and the first child's rendered content.
        E.g., <para>Thrift and El-Serag <emphasis>...</emphasis></para>
        Without the trailing space, "El-Serag" and the emphasis text would merge.

        Similarly preserves leading whitespace in child.tail when the tail text
        follows a closing tag and begins with a letter/digit.
        """
        fixes = []

        # Elements where we should normalize whitespace
        normalize_in = ['title', 'para', 'term', 'entry']

        for elem_name in normalize_in:
            for elem in root.iter(elem_name):
                if elem.text:
                    # Normalize whitespace (collapse multiple spaces)
                    normalized = ' '.join(elem.text.split())
                    if normalized != elem.text and elem.text.strip() == normalized.strip():
                        # Preserve trailing space if element has children
                        # The trailing space separates elem.text from the first child element
                        if len(elem) > 0 and elem.text and elem.text != elem.text.rstrip():
                            normalized = normalized + ' '
                        elem.text = normalized
                        # Don't report this as it's minor

        return fixes

    def _is_pagebreak_id(self, id_value: str) -> bool:
        """Detect pagebreak-style IDs (e.g., Page_30, ch0011-Page_30)."""
        if not id_value:
            return False
        candidate = id_value.strip()
        if '#' in candidate:
            candidate = candidate.split('#', 1)[1]
        return bool(re.search(r'page[_-]?\d+\b', candidate, re.IGNORECASE))

    def _parse_id_to_readable_text(self, id_value: str) -> str:
        """
        Parse an ID to generate human-readable text.

        Examples:
            ch0010-head-2-5 -> Section 2.5 in Chapter 10
            ch0012-c3-tbl-0001 -> Table 1 in Chapter 12
            ch0011-c2-fig-0011 -> Figure 11 in Chapter 11
            ch0010-Page_10 -> Page 10 in Chapter 10
        """
        # Extract chapter number
        chapter_match = re.search(r'ch(\d+)', id_value)
        chapter_num = int(chapter_match.group(1)) if chapter_match else None

        # Check for table
        if 'tbl' in id_value.lower() or 'table' in id_value.lower():
            table_match = re.search(r'tbl[-_]?(\d+)', id_value, re.IGNORECASE)
            if table_match:
                table_num = int(table_match.group(1))
                if chapter_num:
                    return f"Table {table_num} in Chapter {chapter_num}"
                return f"Table {table_num}"

        # Check for figure
        if 'fig' in id_value.lower() or 'figure' in id_value.lower():
            fig_match = re.search(r'fig[-_]?(\d+)', id_value, re.IGNORECASE)
            if fig_match:
                fig_num = int(fig_match.group(1))
                if chapter_num:
                    return f"Figure {fig_num} in Chapter {chapter_num}"
                return f"Figure {fig_num}"

        # Check for page reference
        if 'page' in id_value.lower():
            page_match = re.search(r'page[-_]?(\d+)', id_value, re.IGNORECASE)
            if page_match:
                page_num = int(page_match.group(1))
                if chapter_num:
                    return f"Page {page_num} in Chapter {chapter_num}"
                return f"Page {page_num}"

        # Check for section/heading
        if 'head' in id_value.lower() or 'sect' in id_value.lower():
            # Try to extract section numbers like "2-5" or "2.5"
            section_match = re.search(r'(\d+)[-.](\d+)', id_value)
            if section_match:
                sect_major = section_match.group(1)
                sect_minor = section_match.group(2)
                if chapter_num:
                    return f"Section {sect_major}.{sect_minor} in Chapter {chapter_num}"
                return f"Section {sect_major}.{sect_minor}"

        # Default: just mention the chapter if we found one
        if chapter_num:
            return f"Chapter {chapter_num}"

        # Last resort: return the ID itself
        return id_value

    def _create_missing_ids_in_chapter(self, root: etree._Element, filename: str, missing_ids: List[str],
                                        dropped_ids: Optional[Set[str]] = None) -> Tuple[int, Dict[str, str]]:
        """
        Create missing IDs in a chapter to make cross-references work.

        Args:
            root: XML root element of the target chapter
            filename: Name of the chapter file
            missing_ids: List of missing IDs that should be created in this chapter
            dropped_ids: Optional set of IDs that were intentionally dropped during conversion
                         (anchor-only elements, pagebreaks, etc.) - these should not be recreated

        Returns:
            Tuple of (number of IDs successfully created, dict mapping stale IDs to correct IDs)
        """
        ids_created = 0
        stale_ref_mappings = {}  # Maps stale ID -> correct ID for linkend updates

        # Pattern for generated-style IDs (e.g., ch0055s0002, ch0055s0003fg01)
        # These IDs are structured: {prefix}{chapter}{section}[{element_code}{number}]
        generated_id_pattern = re.compile(r'^[a-z]{2}\d{4}(s\d{4})+(fg|ta|sb|ex|eq|fn|bib|gl|qa|pr|mo|vd|ad|an|li|p)?\d*$')

        for missing_id in missing_ids:
            # Skip if ID already exists
            if root.find(f'.//*[@id="{missing_id}"]') is not None:
                continue
            if self._is_pagebreak_id(missing_id):
                # Pagebreak anchors are intentionally dropped; do not create them.
                continue
            if dropped_ids and missing_id in dropped_ids:
                # This ID was intentionally dropped during conversion (anchor-only element, etc.)
                # Do not recreate it - links to it should have been converted to phrase
                continue

            # Check ID Authority: If the ID was never registered during conversion,
            # it's likely a phantom reference that shouldn't have an anchor created
            if ID_AUTHORITY_AVAILABLE and get_authority:
                authority = get_authority()
                # Check if this generated ID is known to the authority as a valid ID
                if not authority.is_valid_id(missing_id):
                    # The ID was never registered - likely a stale or phantom reference
                    # Log for debugging but continue with other validation
                    pass  # Continue with pattern-based validation below

            # Skip generated-style IDs that reference non-existent content
            # These typically come from stale NCX references that don't have corresponding
            # elements in the actual chapter content. Creating anchors for them would
            # result in phantom anchors pointing to nothing meaningful.
            if generated_id_pattern.match(missing_id):
                # Check if this is a section ID (ends with section pattern only)
                if re.match(r'^[a-z]{2}\d{4}(s\d{4})+$', missing_id):
                    # It's a section ID - check if any corresponding section exists
                    # Extract section depth and number from the ID
                    section_parts = re.findall(r's(\d{4})', missing_id)
                    expected_sect_level = len(section_parts)  # e.g., s0001s0002 -> sect2
                    expected_sect_number = int(section_parts[-1]) if section_parts else 0

                    # Look for sections at this level
                    section_tag = f'sect{expected_sect_level}'
                    sections = list(root.iter(section_tag))

                    # Check if enough sections exist (the ID suggests section N, so we need N sections)
                    if len(sections) < expected_sect_number:
                        # Not enough sections exist - skip creating anchor
                        print(f"    Skipping phantom section anchor '{missing_id}' "
                              f"(only {len(sections)} {section_tag} elements, but ID suggests #{expected_sect_number})")
                        continue
                else:
                    # It's an element ID (figure, table, etc.) - check if element type exists
                    # Extract element code and number from the ID
                    elem_match = re.search(r'(fg|ta|sb|ex|eq|fn|bib|gl|qa|pr|mo|vd|ad|an|li|p)(\d+)$', missing_id)
                    if elem_match:
                        elem_code = elem_match.group(1)
                        elem_number = int(elem_match.group(2))

                        # SKIP phantom check for bibliography entries ('bib')
                        # Bibliography IDs carry the SOURCE citation number (e.g., CR121 -> bib121)
                        # NOT a sequential counter. So bib121 doesn't mean "121st bib entry".
                        if elem_code != 'bib':
                            elem_type_map = {
                                'fg': ['figure', 'informalfigure'],
                                'ta': ['table', 'informaltable'],
                                'sb': ['sidebar'],
                                'ex': ['example'],
                                'eq': ['equation', 'informalequation'],
                                'fn': ['footnote'],
                                'gl': ['glossentry'],
                                'qa': ['qandaentry'],
                                'pr': ['procedure'],
                                'mo': ['mediaobject'],
                            }
                            elem_tags = elem_type_map.get(elem_code, [])
                            if elem_tags:
                                total_elements = 0
                                for tag in elem_tags:
                                    total_elements += len(list(root.iter(tag)))

                                # Check if enough elements exist (ID suggests element N, so we need N elements)
                                if total_elements < elem_number:
                                    # Not enough elements exist - skip creating anchor
                                    print(f"    Skipping phantom element anchor '{missing_id}' "
                                          f"(only {total_elements} {elem_tags} elements, but ID suggests #{elem_number})")
                                    continue

            # Parse the ID to understand what it references
            id_lower = missing_id.lower()

            # Strategy 1: Try to find and add ID to existing content
            target_elem = None

            if 'tbl' in id_lower or 'table' in id_lower or re.search(r'ta\d+$', id_lower):
                # Look for table or informaltable elements
                # Handle both legacy patterns (tbl-1, table_2) and generated patterns (ch0007s0010ta05)
                table_match = re.search(r'tbl[-_]?(\d+)', missing_id, re.IGNORECASE)
                if not table_match:
                    # Try generated ID pattern: ta followed by number at end of ID
                    table_match = re.search(r'ta(\d+)$', missing_id, re.IGNORECASE)
                if table_match:
                    table_num = int(table_match.group(1))
                    # Find tables without IDs or with wrong IDs
                    tables = list(root.iter('table')) + list(root.iter('informaltable'))
                    if 0 < table_num <= len(tables):
                        target_elem = tables[table_num - 1]

            elif 'fig' in id_lower or 'figure' in id_lower or re.search(r'fg\d+$', id_lower):
                # Look for figure or informalfigure elements
                # Handle both legacy patterns (fig-1, figure_2) and generated patterns (ch0007s0010fg18)
                fig_match = re.search(r'fig[-_]?(\d+)', missing_id, re.IGNORECASE)
                if not fig_match:
                    # Try generated ID pattern: fg followed by number at end of ID
                    fig_match = re.search(r'fg(\d+)$', missing_id, re.IGNORECASE)
                if fig_match:
                    fig_num = int(fig_match.group(1))
                    figures = list(root.iter('figure')) + list(root.iter('informalfigure'))
                    if 0 < fig_num <= len(figures):
                        target_elem = figures[fig_num - 1]

            elif 'sect' in id_lower or 'head' in id_lower:
                # Look for section elements
                sections = []
                for sect_level in ['sect1', 'sect2', 'sect3', 'sect4', 'sect5']:
                    sections.extend(root.iter(sect_level))

                if sections:
                    # SPECIAL CASE: Handle head-2-X pattern from TOC (e.g., head-2-5 = 5th sect2)
                    head_match = re.search(r'head[-_](\d+)[-_](\d+)', missing_id)
                    if head_match:
                        level = int(head_match.group(1))  # e.g., 2 for head-2-5
                        position = int(head_match.group(2))  # e.g., 5 for head-2-5

                        # Map level to sect element
                        sect_tag = f'sect{level}'

                        # Find all sections of this level
                        level_sections = list(root.iter(sect_tag))

                        # Get the Nth section (1-indexed)
                        if 0 < position <= len(level_sections):
                            sect_elem = level_sections[position - 1]

                            if not sect_elem.get('id'):
                                # Section has no ID, add it directly
                                target_elem = sect_elem
                            else:
                                # Section already has an ID, create anchor inside it
                                anchor = etree.Element('anchor')
                                anchor.set('id', missing_id)

                                # Insert after the section's title
                                title = sect_elem.find('title')
                                if title is not None:
                                    title_index = list(sect_elem).index(title)
                                    sect_elem.insert(title_index + 1, anchor)
                                else:
                                    sect_elem.insert(0, anchor)

                                ids_created += 1
                                print(f"    Created anchor '{missing_id}' in {self._local_name(sect_elem)} in {filename}")

                                if VALIDATION_REPORT_AVAILABLE:
                                    self.verification_items.append(VerificationItem(
                                        xml_file=filename,
                                        line_number=sect_elem.sourceline if hasattr(sect_elem, 'sourceline') else None,
                                        fix_type="TOC Link Anchor Creation",
                                        fix_description=f"Created <anchor id='{missing_id}'/> inside {sect_tag} for TOC link",
                                        verification_reason=f"Section already had an ID. Created anchor for TOC link 'head-{level}-{position}'.",
                                        suggestion="This anchor makes TOC subchapter links work correctly."
                                    ))

                                # Skip to next missing_id since we handled this one
                                continue

                    # Try to match by section level (e.g., ch010-sect1 -> sect1)
                    if target_elem is None:
                        for sect_level in ['sect5', 'sect4', 'sect3', 'sect2', 'sect1']:
                            if sect_level in missing_id:
                                for sect in root.iter(sect_level):
                                    if not sect.get('id'):
                                        target_elem = sect
                                        break
                                if target_elem is not None:
                                    break

                    # If no match yet, try to match by position with section numbers
                    if target_elem is None:
                        section_match = re.search(r'(\d+)[-.](\d+)', missing_id)
                        if section_match:
                            # Use the first section without an ID
                            for sect in sections:
                                if not sect.get('id'):
                                    target_elem = sect
                                    break

            # If we found a target element, add the ID or create anchor inside it
            if target_elem is not None:
                if not target_elem.get('id'):
                    # Element has no ID, add it directly
                    target_elem.set('id', missing_id)
                    ids_created += 1
                    print(f"    Created ID '{missing_id}' on {self._local_name(target_elem)} in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=target_elem.sourceline if hasattr(target_elem, 'sourceline') else None,
                            fix_type="Missing ID Creation",
                            fix_description=f"Added missing ID '{missing_id}' to <{self._local_name(target_elem)}> element",
                            verification_reason=f"ID was missing but referenced from another chapter. Auto-matched to existing {self._local_name(target_elem)} element.",
                            suggestion="Verify this ID assignment is correct."
                        ))
                else:
                    # Element already has an ID - check if section prefixes match
                    existing_id = target_elem.get('id')

                    # Extract section prefixes (e.g., ch0007s0004 from ch0007s0004fg12)
                    missing_section_match = re.match(r'^([a-z]{2}\d{4}s\d{4})', missing_id)
                    existing_section_match = re.match(r'^([a-z]{2}\d{4}s\d{4})', existing_id)

                    missing_section = missing_section_match.group(1) if missing_section_match else None
                    existing_section = existing_section_match.group(1) if existing_section_match else None

                    # If both have section prefixes and they DON'T match, this is a stale reference
                    # The position-matched element was assigned to a different section - don't create anchor
                    if missing_section and existing_section and missing_section != existing_section:
                        print(f"    Skipping stale reference '{missing_id}' - position-matched element has ID '{existing_id}' (different section)")

                        # Record the stale->correct ID mapping for linkend updates
                        stale_ref_mappings[missing_id] = existing_id

                        # Track this for potential linkend update (existing_id is the correct target)
                        if VALIDATION_REPORT_AVAILABLE:
                            self.verification_items.append(VerificationItem(
                                xml_file=filename,
                                line_number=target_elem.sourceline if hasattr(target_elem, 'sourceline') else None,
                                fix_type="Stale Reference Skipped",
                                fix_description=f"Skipped creating anchor for '{missing_id}' - element has ID '{existing_id}'",
                                verification_reason=f"Section prefix mismatch: {missing_section} vs {existing_section}. The linkend should be updated to use '{existing_id}' instead.",
                                suggestion=f"Update linkends pointing to '{missing_id}' to use '{existing_id}' instead."
                            ))
                        continue

                    # Section prefixes match (or can't determine) - create anchor
                    anchor = etree.Element('anchor')
                    anchor.set('id', missing_id)

                    # DTD does NOT allow anchor inside figure/table elements
                    # These elements only allow: blockinfo?, title, titleabbrev?, (content-mix)+
                    # Anchor must be placed as sibling BEFORE the figure/table
                    target_tag = self._local_name(target_elem)
                    if target_tag in ('figure', 'informalfigure', 'table', 'informaltable'):
                        # Place anchor as sibling before the figure/table
                        parent = target_elem.getparent()
                        if parent is not None:
                            elem_index = list(parent).index(target_elem)
                            parent.insert(elem_index, anchor)
                            ids_created += 1
                            print(f"    Created anchor '{missing_id}' before {target_tag} (has ID '{existing_id}') in {filename}")
                        else:
                            print(f"    Warning: Cannot create anchor for '{missing_id}' - {target_tag} has no parent")
                            continue
                    else:
                        # For sections and other elements, insert after title as before
                        title = target_elem.find('title')
                        if title is not None:
                            title_index = list(target_elem).index(title)
                            target_elem.insert(title_index + 1, anchor)
                        else:
                            target_elem.insert(0, anchor)
                        ids_created += 1
                        print(f"    Created anchor '{missing_id}' inside {target_tag} (has ID '{existing_id}') in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        if target_tag in ('figure', 'informalfigure', 'table', 'informaltable'):
                            fix_desc = f"Created <anchor id='{missing_id}'/> before <{target_tag}> (existing ID: '{existing_id}')"
                            verify_reason = f"Element matched by pattern but already had ID '{existing_id}'. Anchor placed before element (DTD doesn't allow anchor inside {target_tag})."
                        else:
                            fix_desc = f"Created <anchor id='{missing_id}'/> inside <{target_tag}> (existing ID: '{existing_id}')"
                            verify_reason = f"Element matched by pattern but already had ID '{existing_id}'. Created anchor for cross-reference."
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=target_elem.sourceline if hasattr(target_elem, 'sourceline') else None,
                            fix_type="Anchor for Matched Element",
                            fix_description=fix_desc,
                            verification_reason=verify_reason,
                            suggestion="Verify this cross-reference points to the correct element."
                        ))
                continue

            # Strategy 2: Create an anchor element as fallback
            # Try to place anchor in the correct section based on ID prefix
            chapter_elem = root if self._local_name(root) == 'chapter' else root.find('.//chapter')
            if chapter_elem is not None:
                # Create anchor element
                anchor = etree.Element('anchor')
                anchor.set('id', missing_id)

                # Extract section ID from the missing_id (e.g., ch0021s0014 from ch0021s0014x212)
                # ID format: {chapter_id}s{nnnn}{element_code}{counter}
                # Section ID is the first 11 characters (ch0021s0014)
                target_container = None
                section_id_match = re.match(r'^([a-z]{2}\d{4}s\d{4})', missing_id)
                if section_id_match:
                    section_id = section_id_match.group(1)
                    # Find the section or bibliography with this ID
                    target_container = chapter_elem.find(f'.//*[@id="{section_id}"]')

                # If we found the target section, place anchor there
                if target_container is not None:
                    container_tag = self._local_name(target_container)
                    # DTD doesn't allow anchor inside figure/table - place before instead
                    if container_tag in ('figure', 'informalfigure', 'table', 'informaltable'):
                        parent = target_container.getparent()
                        if parent is not None:
                            elem_index = list(parent).index(target_container)
                            parent.insert(elem_index, anchor)
                            print(f"    Created anchor with ID '{missing_id}' before {container_tag} in {filename}")
                        else:
                            # Fallback to sect1
                            first_sect1 = chapter_elem.find('sect1')
                            if first_sect1 is not None:
                                sect1_title = first_sect1.find('title')
                                if sect1_title is not None:
                                    title_index = list(first_sect1).index(sect1_title)
                                    first_sect1.insert(title_index + 1, anchor)
                                else:
                                    first_sect1.insert(0, anchor)
                                print(f"    Created anchor with ID '{missing_id}' in first sect1 (fallback) in {filename}")
                    else:
                        # Insert after the section's title
                        title_elem = target_container.find('title')
                        if title_elem is not None:
                            title_index = list(target_container).index(title_elem)
                            target_container.insert(title_index + 1, anchor)
                        else:
                            target_container.insert(0, anchor)
                        print(f"    Created anchor with ID '{missing_id}' in section '{section_id}' in {filename}")
                else:
                    # Fallback: Insert in first sect1 (anchor not allowed directly in chapter per DTD)
                    first_sect1 = chapter_elem.find('sect1')
                    if first_sect1 is not None:
                        # Insert after sect1's title
                        sect1_title = first_sect1.find('title')
                        if sect1_title is not None:
                            title_index = list(first_sect1).index(sect1_title)
                            first_sect1.insert(title_index + 1, anchor)
                        else:
                            first_sect1.insert(0, anchor)
                        print(f"    Created anchor with ID '{missing_id}' in first sect1 in {filename}")
                    else:
                        # No sect1 exists - create a wrapper sect1 for the anchor
                        chapter_id = chapter_elem.get('id', 'ch0001')

                        # Collect existing IDs to avoid duplicates
                        existing_ids = set()
                        for elem in chapter_elem.iter():
                            if isinstance(elem.tag, str) and elem.get('id'):
                                existing_ids.add(elem.get('id'))

                        wrapper_sect1 = etree.Element('sect1')
                        wrapper_sect1.set('id', next_available_sect1_id(chapter_id, existing_ids))
                        wrapper_title = etree.SubElement(wrapper_sect1, 'title')
                        chapter_title_elem = chapter_elem.find('title')
                        if chapter_title_elem is not None and chapter_title_elem.text:
                            wrapper_title.text = chapter_title_elem.text
                        else:
                            wrapper_title.text = "Content"
                        wrapper_sect1.append(anchor)

                        # Find where to insert sect1 (after title, subtitle, titleabbrev, tocchap)
                        insert_pos = 0
                        for i, child in enumerate(list(chapter_elem)):
                            local_name = child.tag if isinstance(child.tag, str) else ''
                            if local_name in ('beginpage', 'chapterinfo', 'title', 'subtitle', 'titleabbrev', 'tocchap'):
                                insert_pos = i + 1
                        chapter_elem.insert(insert_pos, wrapper_sect1)
                        print(f"    Created sect1 wrapper with anchor ID '{missing_id}' in {filename}")

                ids_created += 1

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=anchor.sourceline if hasattr(anchor, 'sourceline') else None,
                        fix_type="Anchor Creation for Missing ID",
                        fix_description=f"Created <anchor id='{missing_id}'/> element",
                        verification_reason=f"Could not find matching content for ID '{missing_id}'. Created anchor to make cross-reference valid.",
                        suggestion="Locate the actual target content and move this ID to the correct element."
                    ))

        return (ids_created, stale_ref_mappings)

    def _find_similar_id(self, broken_id: str, valid_ids: Set[str], threshold: int = 3) -> Optional[str]:
        """
        Find a similar valid ID using simple string distance.

        Args:
            broken_id: The broken ID to match
            valid_ids: Set of all valid IDs
            threshold: Maximum edit distance to consider a match

        Returns:
            The most similar valid ID, or None if no close match found
        """
        # First, try deterministic normalization for element IDs that differ only
        # by zero-padding on the numeric suffix (e.g., fg10 vs fg0010).
        # Support hierarchical section IDs: ch0001s0001fg01, ch0001s0001s0002fg01, etc.
        # Section patterns: s0001 (sect1), s0001s0002 (sect2), s0001s0002s01 (sect3), etc.
        element_match = re.match(r'^([a-z]{2}\d{4})((?:s\d{2,4})+)([a-z]{1,3})(\d+)$', broken_id)
        if element_match:
            chapter_prefix, section_part, elem_code, elem_num = element_match.groups()
            try:
                elem_num_int = int(elem_num)
            except ValueError:
                elem_num_int = None

            if elem_num_int is not None:
                # Check common zero-padding variants first
                for pad in (4, 3, 2, 1):
                    candidate = f"{chapter_prefix}{section_part}{elem_code}{elem_num_int:0{pad}d}"
                    if candidate in valid_ids:
                        return candidate

                # Otherwise, search for matching element IDs by number (ignore padding)
                # Only return exact section matches - don't fallback to different sections
                # as this could silently link to the wrong element (e.g., figure in wrong section)
                # Pattern matches chapter + any section hierarchy + element code + number
                pattern = re.compile(rf'^{re.escape(chapter_prefix)}((?:s\d{{2,4}})+)([a-z]{{1,3}})(\d+)$')
                for valid_id in valid_ids:
                    match = pattern.match(valid_id)
                    if not match:
                        continue
                    valid_section, valid_code, valid_num = match.groups()
                    if valid_code != elem_code:
                        continue
                    if valid_section != section_part:
                        # Don't accept elements from different sections - could be wrong element
                        continue
                    try:
                        valid_num_int = int(valid_num)
                    except ValueError:
                        continue
                    if valid_num_int == elem_num_int:
                        return valid_id

        def levenshtein_distance(s1: str, s2: str) -> int:
            """Calculate Levenshtein distance between two strings"""
            if len(s1) < len(s2):
                return levenshtein_distance(s2, s1)
            if len(s2) == 0:
                return len(s1)

            previous_row = range(len(s2) + 1)
            for i, c1 in enumerate(s1):
                current_row = [i + 1]
                for j, c2 in enumerate(s2):
                    insertions = previous_row[j + 1] + 1
                    deletions = current_row[j] + 1
                    substitutions = previous_row[j] + (c1 != c2)
                    current_row.append(min(insertions, deletions, substitutions))
                previous_row = current_row

            return previous_row[-1]

        best_match = None
        best_distance = float('inf')

        for valid_id in valid_ids:
            # Quick filter: only consider IDs with similar prefixes
            # Chapter IDs are 6 chars (ch0001), so use 6 for comparison
            if broken_id[:6] == valid_id[:6]:  # Same chapter prefix
                distance = levenshtein_distance(broken_id, valid_id)
                if distance < best_distance and distance <= threshold:
                    best_distance = distance
                    best_match = valid_id

        return best_match

    def _fix_links_missing_linkend(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix <link> elements that are missing the required linkend attribute.

        Per DTD, <link> requires linkend (IDREF #REQUIRED). Elements without
        linkend are invalid and must be converted to preserve content.

        Strategy: Convert <link> without linkend to <phrase> to preserve text.
        """
        fixes = []

        # Find all link elements without linkend (iterate over copy to avoid modification issues)
        links_to_fix = []
        for elem in root.iter('link'):
            linkend = elem.get('linkend')
            # Check if linkend is missing or empty
            if not linkend or not linkend.strip():
                links_to_fix.append(elem)

        for link in links_to_fix:
            parent = link.getparent()
            if parent is None:
                continue

            # Check if parent is a restrictive element that doesn't allow <phrase>
            # (superscript, subscript, code, etc. only allow #PCDATA or limited inline children)
            restrictive_parents = {'superscript', 'subscript', 'code', 'literal',
                                   'constant', 'varname', 'function', 'parameter'}
            parent_tag = parent.tag if hasattr(parent, 'tag') else ''

            if parent_tag in restrictive_parents:
                # Unwrap content directly into parent (don't use phrase wrapper)
                index = list(parent).index(link)
                # Handle text content
                if link.text:
                    if index == 0:
                        parent.text = (parent.text or '') + link.text
                    else:
                        prev_sibling = parent[index - 1]
                        prev_sibling.tail = (prev_sibling.tail or '') + link.text
                # Move children to parent
                for i, child in enumerate(list(link)):
                    parent.insert(index + i, child)
                # Handle tail
                if link.tail:
                    if len(list(link)) > 0:
                        last_child = list(link)[-1]
                        last_child.tail = (last_child.tail or '') + link.tail
                    elif index == 0:
                        parent.text = (parent.text or '') + link.tail
                    else:
                        prev_sibling = parent[index - 1]
                        prev_sibling.tail = (prev_sibling.tail or '') + link.tail
                parent.remove(link)

                fixes.append(f"Unwrapped <link> content in restrictive parent <{parent_tag}> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=link.sourceline if hasattr(link, 'sourceline') else None,
                        fix_type="Missing Linkend Fix",
                        fix_description=f"Unwrapped <link> content in restrictive parent <{parent_tag}>",
                        verification_reason="DTD requires linkend for <link> and <phrase> is not allowed in restrictive elements",
                        suggestion="Verify the unwrapped content is appropriate."
                    ))
            else:
                # Create phrase element to replace link
                phrase = etree.Element('phrase')
                phrase.text = link.text
                phrase.tail = link.tail

                # Copy any children
                for child in list(link):
                    phrase.append(child)

                # Preserve role attribute if present
                if link.get('role'):
                    phrase.set('role', link.get('role'))

                # Replace link with phrase
                index = list(parent).index(link)
                parent.insert(index, phrase)
                parent.remove(link)

                fixes.append(f"Converted <link> without linkend to <phrase> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=phrase.sourceline if hasattr(phrase, 'sourceline') else None,
                        fix_type="Missing Linkend Fix",
                        fix_description="Converted <link> without linkend attribute to <phrase>",
                        verification_reason="DTD requires linkend attribute for <link> elements (IDREF #REQUIRED)",
                        suggestion="Verify the converted text is appropriate."
                    ))

        return fixes

    def _fix_sect1_missing_ids(self, root: etree._Element, filename: str) -> List[str]:
        """
        Add @id attributes to sect1 elements that are missing them.

        Per DTD and splitContentFiles requirements:
        - All sect1 elements MUST have @id for chunking
        - ID format: {parent_id}s{4-digit} (e.g., ch0001s0001, pr0001s0001)

        Strategy:
        1. Find all sect1 elements without @id
        2. Determine parent chapter/preface/appendix ID
        3. Generate sequential sect1 ID
        4. Add @id attribute
        """
        import re
        fixes = []

        # Collect all existing IDs to avoid duplicates
        existing_ids = {elem.get('id') for elem in root.iter() if elem.get('id')}

        # Find all sect1 elements
        for sect1 in root.iter('sect1'):
            if sect1.get('id'):
                continue  # Already has ID

            # Find parent chapter/preface/appendix
            parent = sect1.getparent()
            parent_id = None
            while parent is not None:
                if self._local_name(parent) in {'chapter', 'preface', 'appendix', 'dedication'}:
                    parent_id = parent.get('id')
                    break
                parent = parent.getparent()

            if not parent_id:
                # Use filename-based ID generation
                # Extract prefix from filename (e.g., ch0001 from ch0001.xml)
                filename_match = re.match(r'^([a-z]{2}\d{4})', filename)
                if filename_match:
                    parent_id = filename_match.group(1)
                else:
                    parent_id = 'ch0001'  # Default fallback

            # Generate new sect1 ID
            new_id = next_available_sect1_id(parent_id, existing_ids)
            sect1.set('id', new_id)
            existing_ids.add(new_id)

            # Get sect1 title for logging
            title_elem = sect1.find('title')
            title_text = ''.join(title_elem.itertext()).strip()[:30] if title_elem is not None else 'untitled'

            fixes.append(f"Added @id='{new_id}' to <sect1> '{title_text}...' in {filename}")

            if VALIDATION_REPORT_AVAILABLE:
                self.verification_items.append(VerificationItem(
                    xml_file=filename,
                    line_number=sect1.sourceline if hasattr(sect1, 'sourceline') else None,
                    fix_type="Sect1 Missing ID",
                    fix_description=f"Added @id='{new_id}' to sect1 element",
                    verification_reason="Sect1 elements require @id for chunking and cross-reference linking",
                    suggestion="Verify ID follows naming convention and is correct."
                ))

        return fixes

    def _fix_sect1_noncompliant_ids(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix ALL section IDs (sect1-5) that don't match the required format.

        Required formats:
        - sect1: {chapter_prefix}{4-digits}s{4-digits} (e.g., ch0001s0001)
        - sect2+: {parent_sect_id}s{2 or 4 digits} (e.g., ch0001s0001s01)

        Non-compliant IDs will be replaced and all linkend references updated.

        Returns:
            List of fix descriptions
        """
        import re
        fixes = []

        # Patterns for compliant section IDs
        # sect1: {prefix}{4-digits}s{4-digits}
        # Example: ch0001s0001
        COMPLIANT_SECT1_PATTERN = re.compile(r'^[a-z]{2}\d{4}s\d{4}$')
        # sect2+: must start with compliant sect1 pattern, then have additional sNN or sNNNN markers
        # Example: ch0001s0001s01 (sect2), ch0001s0001s01s01 (sect3), etc.
        COMPLIANT_SECT2_PATTERN = re.compile(r'^[a-z]{2}\d{4}s\d{4}(s\d{2,4}){1,}$')
        # Element ID pattern: {sect_id}{element_code}{digits}
        COMPLIANT_ELEMENT_PATTERN = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})+[a-z]{1,3}\d+$')

        # Collect all existing IDs to avoid duplicates
        existing_ids = {elem.get('id') for elem in root.iter() if elem.get('id')}

        # Find parent chapter/preface/appendix ID from filename
        filename_match = re.match(r'^([a-z]{2}\d{4})', filename)
        default_parent_id = filename_match.group(1) if filename_match else 'ch0001'

        # Track section counters for generating new IDs
        section_counters = {'sect1': 0, 'sect2': {}, 'sect3': {}, 'sect4': {}, 'sect5': {}}

        # Process all sect1 elements first
        for sect1 in root.iter('sect1'):
            old_id = sect1.get('id')
            if not old_id:
                continue  # Handled by _fix_sect1_missing_ids

            # Check if ID is compliant and within length constraints
            if COMPLIANT_SECT1_PATTERN.match(old_id) and len(old_id) <= MAX_ID_LENGTH:
                continue  # Already compliant

            # Find parent chapter/preface/appendix
            parent = sect1.getparent()
            parent_id = None
            while parent is not None:
                if self._local_name(parent) in {'chapter', 'preface', 'appendix', 'dedication', 'glossary', 'bibliography', 'index', 'part'}:
                    parent_id = parent.get('id')
                    break
                parent = parent.getparent()

            if not parent_id:
                parent_id = default_parent_id

            # Generate new compliant ID
            new_id = next_available_sect1_id(parent_id, existing_ids)
            existing_ids.add(new_id)

            # Update the sect1 ID
            sect1.set('id', new_id)

            # Update all linkend references to this ID
            updated_refs = self._update_linkend_references(root, old_id, new_id)

            # Get sect1 title for logging
            title_elem = sect1.find('title')
            title_text = ''.join(title_elem.itertext()).strip()[:30] if title_elem is not None else 'untitled'

            fixes.append(f"Renamed non-compliant sect1 @id='{old_id}' -> '{new_id}' ({updated_refs} refs) in {filename}")

        # Process sect2-5 elements
        # Each level appends another section segment to the parent ID
        # sect2: ch0001s0001s0001 (4-digit), sect3: ch0001s0001s0001s01 (2-digit), etc.
        for level in range(2, 6):
            sect_tag = f'sect{level}'

            for sect in root.iter(sect_tag):
                old_id = sect.get('id')
                if not old_id:
                    continue

                # Check if ID looks compliant (has proper structure) and is within length constraints
                if ((COMPLIANT_SECT2_PATTERN.match(old_id) or COMPLIANT_ELEMENT_PATTERN.match(old_id))
                        and len(old_id) <= MAX_ID_LENGTH):
                    continue

                # Find parent section ID
                parent = sect.getparent()
                parent_sect_id = None
                while parent is not None:
                    parent_tag = self._local_name(parent)
                    if parent_tag in {'sect1', 'sect2', 'sect3', 'sect4', 'sect5'}:
                        parent_sect_id = parent.get('id')
                        if parent_sect_id:
                            break
                    parent = parent.getparent()

                if not parent_sect_id:
                    continue  # Can't fix without parent

                # Generate new ID: {parent_id}s{counter}
                # Levels 1-2 use 4-digit counters (per ID_NAMING_CONVENTION.md)
                # Levels 3-5 use 2-digit counters to stay within 25-char limit
                counter_key = parent_sect_id
                if counter_key not in section_counters[sect_tag]:
                    section_counters[sect_tag][counter_key] = 0
                section_counters[sect_tag][counter_key] += 1
                counter = section_counters[sect_tag][counter_key]

                # Use 4-digit for sect2, 2-digit for sect3-5
                if level == 2:
                    new_id = f"{parent_sect_id}s{counter:04d}"
                else:
                    new_id = f"{parent_sect_id}s{counter:02d}"

                # Ensure unique
                while new_id in existing_ids:
                    section_counters[sect_tag][counter_key] += 1
                    counter = section_counters[sect_tag][counter_key]
                    if level == 2:
                        new_id = f"{parent_sect_id}s{counter:04d}"
                    else:
                        new_id = f"{parent_sect_id}s{counter:02d}"

                existing_ids.add(new_id)
                sect.set('id', new_id)

                updated_refs = self._update_linkend_references(root, old_id, new_id)
                fixes.append(f"Renamed non-compliant {sect_tag} @id='{old_id}' -> '{new_id}' ({updated_refs} refs) in {filename}")

        return fixes

    def _update_linkend_references(self, root: etree._Element, old_id: str, new_id: str) -> int:
        """Update all linkend attributes referencing old_id to new_id."""
        updated = 0
        for elem in root.iter():
            linkend = elem.get('linkend')
            if linkend == old_id:
                elem.set('linkend', new_id)
                updated += 1
            url = elem.get('url')
            if url and f'#{old_id}' in url:
                elem.set('url', url.replace(f'#{old_id}', f'#{new_id}'))
                updated += 1
        # Track rename for cross-file updates (with chain composition)
        if old_id != new_id:
            self._add_id_rename(old_id, new_id)
        return updated

    def _fix_element_noncompliant_ids(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix element IDs (figures, tables, etc.) that don't match the required format.

        Required format: {sect1_id}{element_code}{4-digits}
        Examples: ch0001s0001fg0001, ch0001s0001ta0001, ch0001s0001bib0001

        Returns:
            List of fix descriptions
        """
        import re
        fixes = []

        # Pattern for compliant element IDs
        # Must have: {2-char prefix}{4-digits}s{4-digits}{element_code}{digits}
        # Allow 2-digit segments for deeper sections (sNN) to keep IDs <= 25 chars.
        COMPLIANT_PATTERN = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})+[a-z]{1,3}\d+$')
        SECT1_BASE_PATTERN = re.compile(r'^[a-z]{2}\d{4}s\d{4}$')

        # ELEMENT_CODES imported from id_authority.py (single source of truth)

        # Collect all existing IDs
        existing_ids = {elem.get('id') for elem in root.iter() if elem.get('id')}

        # Find default sect1 ID from filename (1-based section numbering)
        filename_match = re.match(r'^([a-z]{2}\d{4})', filename)
        default_base = f"{filename_match.group(1)}s0001" if filename_match else 'ch0001s0001'

        # Element counters per type per sect1
        element_counters = {}

        # Elements that SHOULD have IDs for proper cross-referencing
        ELEMENTS_NEEDING_IDS = {'glossentry', 'biblioentry', 'bibliomixed', 'qandaentry', 'footnote'}

        # Process elements that should have compliant IDs
        for elem_tag, elem_code in ELEMENT_CODES.items():
            for elem in root.iter(elem_tag):
                old_id = elem.get('id')
                has_old_id = bool(old_id)
                if not has_old_id:
                    # For certain elements, generate an ID if missing (recommended for cross-referencing)
                    if elem_tag not in ELEMENTS_NEEDING_IDS:
                        continue  # No ID to fix, and not an element that needs one

                # Skip bibliography elements with section-style IDs (ch####s####).
                # The converter intentionally assigns section-style IDs to bibliography
                # so the downstream XSL treats them as navigable pages.  Renaming them
                # to element-code format (ch####s####bib####) breaks XSL routing.
                if elem_tag == 'bibliography' and has_old_id and SECT1_BASE_PATTERN.match(old_id):
                    continue

                # Check if already compliant (format, code, and length)
                if has_old_id and COMPLIANT_PATTERN.match(old_id):
                    if len(old_id) <= MAX_ID_LENGTH:
                        # Verify the element code matches expected code for this element
                        # Extract element code from ID: ch0001s0001{code}{digits}
                        if len(old_id) > 11:
                            id_after_sect = old_id[11:]  # e.g., "gl0001" or "tm01"
                            id_code = ''
                            for c in id_after_sect:
                                if c.isalpha():
                                    id_code += c
                                else:
                                    break
                            if id_code == elem_code:
                                continue
                        else:
                            continue  # Too short to have element code, skip

                # Find parent sect1 ID
                parent = elem.getparent()
                sect1_id = None
                while parent is not None:
                    parent_tag = self._local_name(parent)
                    if parent_tag == 'sect1':
                        sect1_id = parent.get('id')
                        if sect1_id and SECT1_BASE_PATTERN.match(sect1_id):
                            break
                    parent = parent.getparent()

                if not sect1_id:
                    sect1_id = default_base

                # Ensure sect1_id is 11 chars (e.g., ch0001s0000)
                if len(sect1_id) < 11:
                    sect1_id = default_base

                # Generate new ID
                counter_key = f"{sect1_id}_{elem_code}"
                if counter_key not in element_counters:
                    element_counters[counter_key] = 0
                element_counters[counter_key] += 1
                counter = element_counters[counter_key]

                new_id = f"{sect1_id}{elem_code}{counter:04d}"

                # Ensure unique
                while new_id in existing_ids:
                    element_counters[counter_key] += 1
                    counter = element_counters[counter_key]
                    new_id = f"{sect1_id}{elem_code}{counter:04d}"

                # Check length constraint (max 25 chars)
                if len(new_id) > 25:
                    # Truncate counter digits if needed
                    max_counter_digits = 25 - len(sect1_id) - len(elem_code)
                    if max_counter_digits > 0:
                        new_id = f"{sect1_id}{elem_code}{counter:0{max_counter_digits}d}"[-25:]

                existing_ids.add(new_id)
                elem.set('id', new_id)

                if has_old_id:
                    updated_refs = self._update_linkend_references(root, old_id, new_id)
                    fixes.append(
                        f"Renamed non-compliant {elem_tag} @id='{old_id}' -> '{new_id}' "
                        f"({updated_refs} refs) in {filename}"
                    )
                else:
                    fixes.append(f"Added missing {elem_tag} @id='{new_id}' in {filename}")

        return fixes

    def _fix_block_elements_in_para(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix block elements (like mediaobject) incorrectly nested inside para elements.

        DTD does not allow block elements inside inline containers like <para>.
        This function fixes these nesting violations:
        - If <para> contains ONLY a block element (no text) -> unwrap the para
        - If <para> contains text AND block element -> move block element outside as sibling

        Args:
            root: XML root element
            filename: Name of the file being fixed

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Block elements that are not allowed inside para
        block_elements = {
            'mediaobject', 'figure', 'informalfigure', 'table', 'informaltable',
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
            'programlisting', 'screen', 'literallayout', 'synopsis',
            'blockquote', 'note', 'warning', 'caution', 'important', 'tip',
            'example', 'informalexample', 'sidebar', 'address'
        }

        # Process all para elements (iterate over a copy since we modify the tree)
        for para in list(root.iter('para')):
            parent = para.getparent()
            if parent is None:
                continue

            # Find block elements inside this para
            blocks_in_para = [child for child in para if self._local_name(child) in block_elements]

            if not blocks_in_para:
                continue

            # Check if para has meaningful text content
            has_text = bool(para.text and para.text.strip())
            has_tail_text = any(child.tail and child.tail.strip() for child in para)

            # Get para's position in parent
            para_index = list(parent).index(para)

            if not has_text and not has_tail_text and len(para) == len(blocks_in_para):
                # Para contains ONLY block element(s), no text -> unwrap para
                block_tags = [self._local_name(b) for b in blocks_in_para]

                # Insert block elements in place of para
                for i, block in enumerate(blocks_in_para):
                    para.remove(block)
                    # Preserve para's tail on the last block element
                    if i == len(blocks_in_para) - 1:
                        block.tail = para.tail
                    parent.insert(para_index + i, block)

                # Remove the now-empty para
                parent.remove(para)
                fixes.append(f"Unwrapped <para> containing only block element(s) {block_tags} in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                        fix_type="Block in Para",
                        fix_description=f"Unwrapped para containing only {block_tags}",
                        verification_reason="DTD does not allow block elements inside para",
                        suggestion="Verify content structure is correct after unwrapping."
                    ))

            else:
                # Para contains text AND block element(s) -> move blocks outside as siblings
                block_tags = [self._local_name(b) for b in blocks_in_para]

                # Insert blocks after the para
                insert_pos = para_index + 1
                for block in blocks_in_para:
                    # Preserve any tail text from the block as text in para
                    if block.tail and block.tail.strip():
                        if len(para) > 0 and para[-1].tail:
                            para[-1].tail = (para[-1].tail or '') + block.tail
                        elif para.text:
                            para.text = para.text + block.tail
                        else:
                            para.text = block.tail
                    block.tail = None

                    para.remove(block)
                    parent.insert(insert_pos, block)
                    insert_pos += 1

                fixes.append(f"Moved block element(s) {block_tags} outside <para> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                        fix_type="Block in Para",
                        fix_description=f"Moved {block_tags} outside para as sibling",
                        verification_reason="DTD does not allow block elements inside para",
                        suggestion="Verify text and block element order is correct."
                    ))

        return fixes

    def _fix_element_id_on_para_caption(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix element IDs that are incorrectly placed on para caption elements.

        Per ruleset Section 7.14: Element IDs MUST be placed on the actual element,
        NOT on a <para> caption element above it.

        Wrong:
          <para id="ch0005s0000s0004ta0001">Table caption text.</para>
          <table>...</table>

        Correct:
          <para>Table caption text.</para>
          <table id="ch0005s0000s0004ta0001">...</table>

        XSL Recognition Note (link.ritt.xsl):
        The XSL uses ID prefixes to determine link behavior. IDs with prefixes like
        ta, fg, eq, ad should be on the corresponding elements to ensure proper
        cross-reference resolution.

        Strategy:
        1. Find para elements with IDs containing element-type prefixes
        2. Look at the next sibling element
        3. If it's the matching element type, move the ID
        4. Update linkend references if needed

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Map of ID prefixes to expected element types
        # These prefixes should NOT be on para elements
        PREFIX_TO_ELEMENTS = {
            'ta': {'table', 'informaltable'},
            'fg': {'figure', 'informalfigure'},
            'eq': {'equation', 'informalequation'},
            'ad': {'sidebar', 'note', 'tip', 'warning', 'caution', 'important'},
            'gl': {'glossentry', 'glossterm'},
            'bib': {'bibliomixed', 'biblioentry'},
            'qa': {'qandaentry', 'qandaset'},
            'pr': {'procedure'},
            'vd': {'videoobject', 'mediaobject'},
        }

        # Find all para elements with IDs
        for para in list(root.iter('para')):
            para_id = para.get('id')
            if not para_id:
                continue

            # Check if ID contains an element-type prefix
            detected_prefix = None
            for prefix, element_types in PREFIX_TO_ELEMENTS.items():
                # Look for prefix pattern in the ID (e.g., ...s0001ta0001)
                if prefix in para_id:
                    # Verify it's actually an element prefix pattern (prefix followed by digits)
                    import re
                    if re.search(rf'{prefix}\d+', para_id):
                        detected_prefix = prefix
                        expected_elements = element_types
                        break

            if not detected_prefix:
                continue

            # Find the next sibling element
            parent = para.getparent()
            if parent is None:
                continue

            siblings = list(parent)
            para_index = siblings.index(para)

            # Look for the matching element after this para
            target_element = None
            for sibling_idx in range(para_index + 1, min(para_index + 3, len(siblings))):
                sibling = siblings[sibling_idx]
                sibling_tag = self._local_name(sibling)

                if sibling_tag in expected_elements:
                    # Found the matching element
                    target_element = sibling
                    break
                elif sibling_tag not in {'para', 'beginpage'}:
                    # Stop searching if we hit a different block element
                    break

            if target_element is None:
                # Log a warning but don't fix if we can't find the target
                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                        fix_type="ID Placement Warning",
                        fix_description=f"Para has {detected_prefix} ID but no matching element found",
                        verification_reason=f"ID '{para_id}' has {detected_prefix} prefix suggesting it belongs on a {expected_elements} element",
                        suggestion="Manually verify ID placement and move to correct element if needed."
                    ))
                continue

            # Check if target already has an ID
            target_id = target_element.get('id')
            if target_id:
                # Target already has ID - log conflict but don't overwrite
                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                        fix_type="ID Placement Conflict",
                        fix_description=f"Cannot move ID from para to {self._local_name(target_element)}",
                        verification_reason=f"Para has ID '{para_id}' but target already has ID '{target_id}'",
                        suggestion="Manually resolve duplicate ID assignment."
                    ))
                continue

            # Move the ID from para to target element
            target_element.set('id', para_id)
            del para.attrib['id']

            target_tag = self._local_name(target_element)
            fixes.append(f"Moved ID '{para_id}' from <para> to <{target_tag}> in {filename}")

            if VALIDATION_REPORT_AVAILABLE:
                self.verification_items.append(VerificationItem(
                    xml_file=filename,
                    line_number=para.sourceline if hasattr(para, 'sourceline') else None,
                    fix_type="ID Placement Fix",
                    fix_description=f"Moved ID from para caption to {target_tag} element",
                    verification_reason=f"ID '{para_id}' has {detected_prefix} prefix and belongs on {target_tag}",
                    suggestion="Verify cross-references still resolve correctly."
                ))

        return fixes

    def _fix_footnote_invalid_children(self, root: etree._Element, filename: str) -> List[str]:
        """
        Remove invalid child elements from footnotes.

        DTD allows footnote to contain: (footnote.mix)+
        footnote.mix = list.class | linespecific.class | synop.class | para.class | informal.class

        NOT allowed: anchor, indexterm, ulink, link, xref, etc.

        Strategy:
        1. Find all footnote elements
        2. Remove anchor, indexterm elements (preserving tail text)
        3. Convert ulink/link to plain text

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Elements not allowed directly in footnote (only block elements allowed)
        disallowed_in_footnote = {'anchor', 'indexterm', 'ulink', 'link', 'xref', 'emphasis'}

        for footnote in root.iter('footnote'):
            children_to_process = list(footnote)

            for child in children_to_process:
                tag = self._local_name(child)

                if tag == 'anchor':
                    # Remove anchor, preserve tail text
                    self._remove_element_preserve_tail(footnote, child)
                    fixes.append(f"Removed <anchor> from <footnote> in {filename}")

                elif tag == 'indexterm':
                    # Remove indexterm, preserve tail text
                    self._remove_element_preserve_tail(footnote, child)
                    fixes.append(f"Removed <indexterm> from <footnote> in {filename}")

                elif tag in ('ulink', 'link', 'xref'):
                    # These should be inside para, not directly in footnote
                    # If found directly in footnote, wrap in para
                    para = etree.Element('para')
                    idx = list(footnote).index(child)
                    footnote.remove(child)
                    para.append(child)
                    footnote.insert(idx, para)
                    fixes.append(f"Wrapped <{tag}> in <para> inside <footnote> in {filename}")

            # Also fix anchors inside para elements within footnote
            for para in footnote.iter('para'):
                para_children = list(para)
                for child in para_children:
                    tag = self._local_name(child)
                    if tag == 'anchor':
                        self._remove_element_preserve_tail(para, child)
                        fixes.append(f"Removed <anchor> from <para> inside <footnote> in {filename}")
                    elif tag == 'indexterm':
                        self._remove_element_preserve_tail(para, child)
                        fixes.append(f"Removed <indexterm> from <para> inside <footnote> in {filename}")

            # Fix table/informaltable inside footnote — DTD does not allow <table>
            # inside <footnote>.  Extract the table and place it after the
            # footnote's containing <para> in the enclosing section element.
            tables_to_extract = [
                child for child in list(footnote)
                if isinstance(child.tag, str) and self._local_name(child) in ('table', 'informaltable')
            ]
            for table in tables_to_extract:
                footnote_parent = footnote.getparent()
                if footnote_parent is None:
                    continue
                section = footnote_parent.getparent()
                if section is None:
                    section = footnote_parent

                table_tag = self._local_name(table)
                footnote.remove(table)

                # Insert table after the footnote's parent element in the section
                try:
                    insert_idx = list(section).index(footnote_parent) + 1
                except ValueError:
                    ancestor = footnote_parent
                    while ancestor is not None and ancestor.getparent() is not section:
                        ancestor = ancestor.getparent()
                    if ancestor is not None:
                        insert_idx = list(section).index(ancestor) + 1
                    else:
                        insert_idx = len(section)

                section.insert(insert_idx, table)
                fixes.append(
                    f"Extracted <{table_tag}> from <footnote> to parent <{section.tag}> in {filename}"
                )

            # Fix figure elements inside footnote - DTD only allows mediaobject, not figure
            # Strategy: Convert figure to mediaobject (unwrap the figure, keep mediaobject)
            for figure in list(footnote.iter('figure')):
                figure_parent = figure.getparent()
                if figure_parent is None:
                    continue

                # Find mediaobject or graphic inside the figure
                mediaobject = figure.find('.//mediaobject')
                graphic = figure.find('.//graphic')

                if mediaobject is not None:
                    # Replace figure with its mediaobject
                    idx = list(figure_parent).index(figure)
                    figure_parent.remove(figure)
                    # Preserve any tail text from figure
                    mediaobject.tail = figure.tail
                    figure_parent.insert(idx, mediaobject)
                    fixes.append(f"Converted <figure> to <mediaobject> inside <footnote> in {filename}")
                elif graphic is not None:
                    # Replace figure with its graphic
                    idx = list(figure_parent).index(figure)
                    figure_parent.remove(figure)
                    graphic.tail = figure.tail
                    figure_parent.insert(idx, graphic)
                    fixes.append(f"Converted <figure> to <graphic> inside <footnote> in {filename}")
                else:
                    # No mediaobject or graphic - remove the figure entirely
                    self._remove_element_preserve_tail(figure_parent, figure)
                    fixes.append(f"Removed empty <figure> from <footnote> in {filename}")

        return fixes

    def _fix_footnote_placement(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix footnotes that are direct children of sect1/sect2/etc.

        DTD: Footnotes should be INSIDE para elements, not as block siblings.

        Strategy:
        1. Find footnotes that are direct children of section elements
        2. Move them inside the previous para (at the end)
        3. If no previous para, wrap in a new para

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Section elements where footnotes should not be direct children
        section_tags = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'chapter', 'appendix', 'preface'}

        for section_tag in section_tags:
            for section in root.iter(section_tag):
                # Find footnotes that are direct children
                footnotes_to_move = []
                for child in list(section):
                    if self._local_name(child) == 'footnote':
                        footnotes_to_move.append(child)

                for footnote in footnotes_to_move:
                    idx = list(section).index(footnote)

                    # Find previous para to move footnote into
                    prev_para = None
                    for i in range(idx - 1, -1, -1):
                        sibling = section[i]
                        if self._local_name(sibling) == 'para':
                            prev_para = sibling
                            break

                    if prev_para is not None:
                        # Move footnote to end of previous para
                        section.remove(footnote)
                        prev_para.append(footnote)
                        fixes.append(f"Moved <footnote> inside previous <para> in <{section_tag}> in {filename}")
                    else:
                        # No previous para - wrap footnote in a new para
                        wrapper_para = etree.Element('para')
                        section.remove(footnote)
                        wrapper_para.append(footnote)
                        section.insert(idx, wrapper_para)
                        fixes.append(f"Wrapped orphan <footnote> in <para> in <{section_tag}> in {filename}")

        return fixes

    def _remove_element_preserve_tail(self, parent: etree._Element, child: etree._Element) -> None:
        """Remove an element while preserving its tail text."""
        if child.tail:
            idx = list(parent).index(child)
            if idx > 0:
                prev = parent[idx - 1]
                prev.tail = (prev.tail or '') + child.tail
            else:
                parent.text = (parent.text or '') + child.tail
        parent.remove(child)

    def _fix_figure_in_bibliography(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix figure elements inside bibliography - not allowed by DTD.

        DTD allows bibliography to contain:
        (bibliographyinfo?, (title, subtitle?, titleabbrev?)?,
         (block_content)*, (bibliodiv+ | (biblioentry | bibliomixed)+))

        So figure is only allowed as block content BEFORE the bibliomixed/biblioentry elements,
        not mixed among them.

        Strategy:
        1. Find all figure elements inside bibliography
        2. Convert them to bibliomixed entries with the figure content as inline mediaobject
        3. Or remove them if they have no useful content

        Returns:
            List of fix descriptions
        """
        fixes = []

        for bibliography in root.iter('bibliography'):
            # Find all figure elements that are direct children of bibliography
            figures_to_fix = [child for child in list(bibliography)
                             if self._local_name(child) == 'figure']

            for figure in figures_to_fix:
                idx = list(bibliography).index(figure)
                figure_id = figure.get('id', '')

                # Try to find mediaobject inside the figure
                mediaobject = figure.find('.//mediaobject')

                if mediaobject is not None:
                    # Convert figure to bibliomixed containing the mediaobject content
                    bibliomixed = etree.Element('bibliomixed')
                    if figure_id:
                        bibliomixed.set('id', figure_id)

                    # Get any title from the figure to use as text before the media
                    title_elem = figure.find('title')
                    if title_elem is not None:
                        title_text = ''.join(title_elem.itertext()).strip()
                        if title_text:
                            bibliomixed.text = title_text + ' '

                    # Move the mediaobject into bibliomixed (as allowed inline content)
                    # Note: bibliomixed allows inline content including mediaobject
                    bibliomixed.append(mediaobject)

                    bibliography.remove(figure)
                    bibliography.insert(idx, bibliomixed)
                    fixes.append(f"Converted <figure> to <bibliomixed> in <bibliography> in {filename}")
                else:
                    # No mediaobject - just remove the figure
                    bibliography.remove(figure)
                    fixes.append(f"Removed <figure> from <bibliography> (no mediaobject content) in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                        fix_type="Bibliography Figure Fix",
                        fix_description=f"Fixed figure in bibliography (ID: {figure_id or 'none'})",
                        verification_reason="DTD does not allow <figure> as direct child of <bibliography>",
                        suggestion="Verify bibliography entry is properly formatted."
                    ))

        return fixes

    def _fix_bibliography_in_sections(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix bibliography elements that are in invalid positions inside sections.

        DTD sect1 content model DOES allow bibliography as part of nav.class:
          (sect1info?, title-group, (nav.class)*, (block-content+, sect2*)|sect2+), (nav.class)*)
        where nav.class = toc|lot|index|glossary|bibliography.

        Bibliography is valid in sect1 when:
        - In the trailing (nav.class)* position (after all block content / sect2 elements)
        - The section also has at least one block content element (para, etc.)

        This function only extracts bibliography from sections where it is truly
        in an invalid position.  It must NOT remove sect1 wrappers that contain
        bibliography, because RISchunker depends on sect1 IDs for page routing.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Block-level tags that satisfy the DTD's required block-content group
        block_tags = {'glosslist', 'itemizedlist', 'orderedlist', 'caution', 'important',
                      'note', 'tip', 'warning', 'literallayout', 'synopsis', 'formalpara',
                      'para', 'address', 'blockquote', 'graphic', 'mediaobject', 'equation',
                      'figure', 'table', 'sidebar', 'qandaset', 'anchor', 'bridgehead',
                      'highlights', 'authorblurb', 'epigraph', 'abstract', 'indexterm', 'beginpage'}

        section_tags = ['sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section', 'simplesect']

        for section_tag in reversed(section_tags):
            for section in list(root.iter(section_tag)):
                bibliographies = [child for child in list(section)
                                 if self._local_name(child) == 'bibliography']

                if not bibliographies:
                    continue

                # Skip sections with role="bibliography" — intentional routing
                # wrappers for standalone bibliography chapters.
                section_role = (section.get('role') or '').lower()
                if section_role == 'bibliography':
                    continue

                # Check if bibliography is in a valid trailing nav.class position:
                # the section has at least one block-content child AND the
                # bibliography is at or near the end (after all block content).
                children = list(section)
                has_block_content = any(
                    self._local_name(c) in block_tags for c in children
                )
                if has_block_content:
                    # Bibliography after block content is DTD-valid (trailing
                    # nav.class position).  The sect1 wrapper is needed for
                    # RISchunker routing — leave it alone.
                    continue

                section_id = section.get('id', '')

                for bibliography in bibliographies:
                    valid_ancestor = None
                    ancestor = section.getparent()
                    while ancestor is not None:
                        ancestor_tag = self._local_name(ancestor)
                        if ancestor_tag in {'chapter', 'preface', 'appendix', 'part', 'book'}:
                            valid_ancestor = ancestor
                            break
                        ancestor = ancestor.getparent()

                    if valid_ancestor is None:
                        continue

                    valid_ancestor_tag = self._local_name(valid_ancestor)

                    # Section has ONLY title + bibliography (no block content) — DTD-invalid.
                    # Move bibliography to valid ancestor.
                    section_children = [c for c in section if self._local_name(c) not in {'title', 'subtitle', 'titleabbrev'}]

                    if len(section_children) == 1 and section_children[0] is bibliography:
                        section_title = section.find('title')
                        bib_title = bibliography.find('title')

                        if section_title is not None and bib_title is None:
                            section.remove(section_title)
                            bibliography.insert(0, section_title)

                        if section_id and not bibliography.get('id'):
                            bibliography.set('id', section_id)

                        section_parent = section.getparent()
                        if section_parent is not None:
                            section_idx = list(section_parent).index(section)
                            section_parent.remove(section)

                            if section_parent is valid_ancestor:
                                valid_ancestor.insert(section_idx, bibliography)
                            else:
                                valid_ancestor.append(bibliography)

                            fixes.append(f"Replaced <{section_tag}> with <bibliography> in <{valid_ancestor_tag}> in {filename}")
                    else:
                        section.remove(bibliography)
                        valid_ancestor.append(bibliography)
                        fixes.append(f"Moved <bibliography> from <{section_tag}> to <{valid_ancestor_tag}> in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=bibliography.sourceline if hasattr(bibliography, 'sourceline') else None,
                            fix_type="Bibliography Placement Fix",
                            fix_description=f"Fixed bibliography inside {section_tag} (ID: {section_id or 'none'})",
                            verification_reason=f"Bibliography was in invalid DTD position inside <{section_tag}>",
                            suggestion="Verify bibliography placement is correct."
                        ))

        return fixes

    def _fix_section_missing_block_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix sect1-sect5 elements that have no required block content.

        Per DTD, the sect1 content model is:
          (sect1info?, (title, subtitle?, titleabbrev?),
           (toc|lot|index|glossary|bibliography)*,
           (((block-elements+, (risempty*|sect2*|simplesect*))
             | risempty+ | sect2+ | simplesect+)),
           (toc|lot|index|glossary|bibliography)*)

        The third group is MANDATORY — at least one block element, risempty,
        sect2, or simplesect is required. When a section contains only
        (title, bibliography), the bibliography satisfies the optional nav
        group but the mandatory block-content group is empty.

        Strategy: Insert an empty <para/> after the title/nav elements
        to satisfy the required block content.
        """
        fixes = []

        info_tags = {'sect1info', 'sect2info', 'sect3info', 'sect4info', 'sect5info', 'sectioninfo'}
        title_tags = {'title', 'subtitle', 'titleabbrev'}
        nav_tags = {'toc', 'lot', 'index', 'glossary', 'bibliography'}
        section_child_tags = {'sect2', 'sect3', 'sect4', 'sect5', 'simplesect'}

        # Block elements that satisfy the mandatory content group
        block_tags = {
            'glosslist', 'itemizedlist', 'orderedlist', 'caution', 'important',
            'note', 'tip', 'warning', 'literallayout', 'synopsis', 'formalpara',
            'para', 'address', 'blockquote', 'graphic', 'mediaobject', 'equation',
            'figure', 'table', 'sidebar', 'qandaset', 'anchor', 'bridgehead',
            'highlights', 'authorblurb', 'epigraph', 'abstract', 'indexterm',
            'beginpage', 'risempty',
        }

        for section_tag in ['sect1', 'sect2', 'sect3', 'sect4', 'sect5']:
            for section in root.iter(section_tag):
                children = list(section)
                if not children:
                    continue

                has_block_content = False
                has_child_section = False
                last_pre_block_idx = -1

                for i, child in enumerate(children):
                    if not isinstance(child.tag, str):
                        continue
                    tag = self._local_name(child)

                    if tag in info_tags or tag in title_tags or tag in nav_tags:
                        last_pre_block_idx = i
                    elif tag in block_tags:
                        has_block_content = True
                        break
                    elif tag in section_child_tags:
                        has_child_section = True
                        break

                if has_block_content or has_child_section:
                    continue

                # Section has no block content — insert empty <para/>
                insert_pos = last_pre_block_idx + 1
                para = etree.SubElement(section, 'para')
                # Move para to the correct position (SubElement appends at end)
                section.remove(para)
                section.insert(insert_pos, para)

                section_id = section.get('id', '')
                fixes.append(
                    f"Added <para/> to <{section_tag}> missing required block content "
                    f"(ID: {section_id or 'none'}) in {filename}"
                )

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=section.sourceline if hasattr(section, 'sourceline') else None,
                        fix_type="Section Content Model Fix",
                        fix_description=f"Added empty <para/> to {section_tag} with no block content (ID: {section_id})",
                        verification_reason="DTD requires at least one block element in section content",
                        suggestion="Verify section structure is correct."
                    ))

        return fixes

    def _fix_orphaned_bibliography_at_chapter_level(self, root: etree._Element, filename: str) -> List[str]:
        """
        Wrap bibliography elements that are direct children of chapter/preface/appendix in sect1.

        The RISchunker XSL only emits begin.section / end.section markers for
        sect1-sect5 elements.  A bare <bibliography> at the chapter level renders
        into the preceding section's page but has no routing markers and is
        invisible to the platform's page navigation.

        Strategy: For each bibliography that is a direct child of a chapter-level
        container, wrap it in a new <sect1 role="bibliography"> with a spacer <para>.

        Returns:
            List of fix descriptions
        """
        fixes = []
        container_tags = {'chapter', 'preface', 'appendix'}

        # Collect existing IDs for generating unique sect1 IDs
        existing_ids = {elem.get('id') for elem in root.iter() if elem.get('id')}

        for container in root.iter():
            if self._local_name(container) not in container_tags:
                continue

            # Find bibliography elements that are direct children (not inside sect1)
            for bib in list(container):
                if self._local_name(bib) != 'bibliography':
                    continue

                # bibliography is a direct child of chapter — wrap it in sect1
                container_id = container.get('id', 'unknown')
                bib_idx = list(container).index(bib)

                # Create sect1 wrapper
                sect1 = etree.Element('sect1')
                sect1_id = bib.get('id', '') or next_available_sect1_id(container_id, existing_ids)
                # If bibliography already has a section-format ID (e.g. ch0002s0010),
                # use it for the sect1 and give bibliography a 'bib' suffixed ID.
                if re.match(r'^[a-z]{2}\d{4}s\d{4}', sect1_id):
                    sect1.set('id', sect1_id)
                    bib.set('id', sect1_id + 'bib')
                else:
                    new_sect1_id = next_available_sect1_id(container_id, existing_ids)
                    sect1.set('id', new_sect1_id)
                    sect1_id = new_sect1_id

                existing_ids.add(sect1.get('id'))
                sect1.set('role', 'bibliography')

                # Create title from bibliography's title or default
                bib_title_elem = bib.find('title')
                bib_title_text = 'References'
                if bib_title_elem is not None:
                    bib_title_text = ''.join(bib_title_elem.itertext()).strip() or 'References'

                sect1_title = etree.SubElement(sect1, 'title')
                sect1_title.text = bib_title_text

                # Add spacer para for DTD compliance (required block content before nav.class)
                spacer = etree.SubElement(sect1, 'para')
                spacer.text = '\u00a0'

                # Move bibliography into sect1
                container.remove(bib)
                sect1.append(bib)

                # Remove duplicate title from bibliography (sect1 already has it)
                if bib_title_elem is not None:
                    bib.remove(bib_title_elem)

                # Insert sect1 at the same position
                container.insert(bib_idx, sect1)

                container_tag = self._local_name(container)
                fixes.append(
                    f"Wrapped orphaned <bibliography> in <sect1 id='{sect1.get('id')}'> "
                    f"inside <{container_tag}> in {filename}"
                )

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=bib.sourceline if hasattr(bib, 'sourceline') else None,
                        fix_type="Orphaned Bibliography Wrapping",
                        fix_description=f"Wrapped bibliography in sect1 for RISchunker routing (ID: {sect1.get('id')})",
                        verification_reason="Bibliography as direct chapter child is invisible to RISchunker",
                        suggestion="Verify bibliography page is navigable."
                    ))

        return fixes

    def _fix_nested_bibliography(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix nested bibliography elements - bibliography cannot contain bibliography.

        DTD: bibliography content model is (bibliographyinfo?, (title, subtitle?, titleabbrev?)?,
             (component.mix)*, (bibliodiv+ | (biblioentry|bibliomixed)+))

        Strategy: unwrap inner bibliography by moving its children up to the parent.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for bibliography in list(root.iter('bibliography')):
            nested_bibs = [child for child in list(bibliography)
                          if self._local_name(child) == 'bibliography']

            for nested_bib in nested_bibs:
                idx = list(bibliography).index(nested_bib)
                for child in list(nested_bib):
                    bibliography.insert(idx, child)
                    idx += 1
                bibliography.remove(nested_bib)
                fixes.append(f"Unwrapped nested <bibliography> inside <bibliography> in {filename}")

        return fixes

    def _fix_author_personname_affiliation(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix author elements that have both personname AND affiliation as children.

        DTD: author content model is ((personname | (honorific|firstname|surname|lineage|
             othername|affiliation|authorblurb|contrib|degree)+), (personblurb|email|address)*)

        Either personname alone OR individual name components with affiliation, not both.
        Strategy: unwrap personname children to author level when affiliation is also present.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for author in list(root.iter('author')):
            personname = author.find('personname')
            has_affiliation = author.find('affiliation') is not None
            if personname is not None and has_affiliation:
                insert_idx = list(author).index(personname)
                for child in list(personname):
                    author.insert(insert_idx, child)
                    insert_idx += 1
                author.remove(personname)
                fixes.append(f"Unwrapped <personname> in <author> with <affiliation> in {filename}")

        return fixes

    def _fix_phrase_in_restrictive_elements(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix phrase elements inside restrictive elements (superscript, subscript, etc.).

        DTD: superscript and subscript only allow:
             #PCDATA | link.char.class | emphasis | replaceable | symbol | inlinegraphic | inlinemediaobject

        phrase is NOT in this list, so it must be unwrapped (keep text content only).

        Strategy:
        1. Find all phrase elements inside restrictive parents
        2. Unwrap them, preserving their text content

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Elements that have restrictive content models not allowing phrase
        restrictive_parents = {'superscript', 'subscript', 'literal', 'code',
                              'constant', 'varname', 'function', 'parameter',
                              'computeroutput', 'userinput', 'filename', 'command',
                              'option', 'replaceable', 'symbol'}

        for phrase in list(root.iter('phrase')):
            parent = phrase.getparent()
            if parent is None:
                continue

            parent_tag = self._local_name(parent)
            if parent_tag not in restrictive_parents:
                continue

            # Unwrap phrase: move text and children to parent
            idx = list(parent).index(phrase)

            # Handle phrase's text content
            phrase_text = phrase.text or ''

            # Get preceding sibling to append text to
            if idx > 0:
                prev_sibling = parent[idx - 1]
                prev_sibling.tail = (prev_sibling.tail or '') + phrase_text
            else:
                parent.text = (parent.text or '') + phrase_text

            # Move any children of phrase to parent (should be rare)
            for i, child in enumerate(list(phrase)):
                phrase.remove(child)
                parent.insert(idx + i, child)

            # Preserve phrase's tail text
            phrase_tail = phrase.tail or ''
            if len(list(phrase)) > 0:
                # Append to last moved child
                last_child = parent[idx + len(list(phrase)) - 1]
                last_child.tail = (last_child.tail or '') + phrase_tail
            elif idx > 0:
                prev_sibling = parent[idx - 1]
                prev_sibling.tail = (prev_sibling.tail or '') + phrase_tail
            else:
                parent.text = (parent.text or '') + phrase_tail

            # Remove the phrase element
            parent.remove(phrase)

            fixes.append(f"Unwrapped <phrase> inside <{parent_tag}> in {filename}")

            if VALIDATION_REPORT_AVAILABLE:
                self.verification_items.append(VerificationItem(
                    xml_file=filename,
                    line_number=phrase.sourceline if hasattr(phrase, 'sourceline') else None,
                    fix_type="Phrase Unwrap Fix",
                    fix_description=f"Unwrapped phrase inside {parent_tag}",
                    verification_reason=f"DTD does not allow <phrase> inside <{parent_tag}>",
                    suggestion="Verify text content is preserved correctly."
                ))

        return fixes

    def _fix_footnote_in_glossary_bibliography(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix footnote elements that appear directly inside glossary or bibliography.

        DTD: glossary allows (glossaryinfo?, (title, subtitle?, titleabbrev?)?,
             (glosslist | itemizedlist | ...), (glossdiv+ | glossentry+), bibliography?)

        DTD: bibliography allows (bibliographyinfo?, (title, subtitle?, titleabbrev?)?,
             (bibliodiv+ | (bibliomixed|biblioentry)+))

        Footnotes are NOT allowed as direct children. They must be inside para elements
        within bibliomixed or glossdef.

        Strategy:
        1. Find footnotes that are direct children of glossary/bibliography
        2. Move them inside the nearest appropriate container (para, bibliomixed, glossdef)
        3. Or wrap in a para if needed

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Fix footnotes directly inside glossary
        for glossary in list(root.iter('glossary')):
            footnotes = [child for child in list(glossary)
                        if self._local_name(child) == 'footnote']

            for footnote in footnotes:
                idx = list(glossary).index(footnote)

                # Find a preceding glossentry or glossdiv to move the footnote into
                target_container = None
                for i in range(idx - 1, -1, -1):
                    sibling = glossary[i]
                    sibling_tag = self._local_name(sibling)
                    if sibling_tag == 'glossentry':
                        # Find glossdef inside glossentry
                        glossdef = sibling.find('glossdef')
                        if glossdef is not None:
                            # Find or create a para inside glossdef
                            para = glossdef.find('para')
                            if para is not None:
                                target_container = para
                                break
                        # No glossdef - create one with para
                        glossdef = etree.Element('glossdef')
                        para = etree.SubElement(glossdef, 'para')
                        sibling.append(glossdef)
                        target_container = para
                        break
                    elif sibling_tag == 'glossdiv':
                        # Find last glossentry in glossdiv
                        entries = list(sibling.iter('glossentry'))
                        if entries:
                            glossdef = entries[-1].find('glossdef')
                            if glossdef is not None:
                                para = glossdef.find('para')
                                if para is not None:
                                    target_container = para
                                    break

                if target_container is not None:
                    glossary.remove(footnote)
                    target_container.append(footnote)
                    fixes.append(f"Moved <footnote> from <glossary> into <para> in {filename}")
                else:
                    # No suitable container - wrap in para and add to glossary end
                    # But actually footnote isn't allowed directly in glossary at all
                    # Best option: remove the footnote or convert to para text
                    footnote_text = self._get_element_text(footnote)
                    glossary.remove(footnote)
                    # Add as a note in a new para if there's any content
                    if footnote_text.strip():
                        # Create a bibliomixed at the end if bibliography section exists
                        # Otherwise just log it as removed
                        fixes.append(f"Removed orphan <footnote> from <glossary> (content: '{footnote_text[:50]}...') in {filename}")
                    else:
                        fixes.append(f"Removed empty <footnote> from <glossary> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=footnote.sourceline if hasattr(footnote, 'sourceline') else None,
                        fix_type="Footnote Placement Fix",
                        fix_description="Fixed footnote inside glossary",
                        verification_reason="DTD does not allow <footnote> as direct child of <glossary>",
                        suggestion="Verify footnote content is preserved correctly."
                    ))

        # Fix footnotes directly inside bibliography
        for bibliography in list(root.iter('bibliography')):
            footnotes = [child for child in list(bibliography)
                        if self._local_name(child) == 'footnote']

            for footnote in footnotes:
                idx = list(bibliography).index(footnote)

                # Find a preceding bibliomixed/biblioentry to move the footnote into
                target_container = None
                for i in range(idx - 1, -1, -1):
                    sibling = bibliography[i]
                    sibling_tag = self._local_name(sibling)
                    if sibling_tag == 'bibliomixed':
                        # bibliomixed can contain para.char.mix which includes footnote
                        target_container = sibling
                        break
                    elif sibling_tag == 'bibliodiv':
                        # Find last bibliomixed in bibliodiv
                        entries = list(sibling.iter('bibliomixed'))
                        if entries:
                            target_container = entries[-1]
                            break

                if target_container is not None:
                    bibliography.remove(footnote)
                    target_container.append(footnote)
                    fixes.append(f"Moved <footnote> from <bibliography> into <bibliomixed> in {filename}")
                else:
                    # No suitable container - convert footnote to bibliomixed
                    footnote_text = self._get_element_text(footnote)
                    bibliography.remove(footnote)
                    if footnote_text.strip():
                        # Create a new bibliomixed with the footnote content
                        bibliomixed = etree.Element('bibliomixed')
                        bibliomixed.text = f"[Note: {footnote_text}]"
                        bibliography.insert(idx, bibliomixed)
                        fixes.append(f"Converted orphan <footnote> to <bibliomixed> in <bibliography> in {filename}")
                    else:
                        fixes.append(f"Removed empty <footnote> from <bibliography> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=footnote.sourceline if hasattr(footnote, 'sourceline') else None,
                        fix_type="Footnote Placement Fix",
                        fix_description="Fixed footnote inside bibliography",
                        verification_reason="DTD does not allow <footnote> as direct child of <bibliography>",
                        suggestion="Verify footnote content is preserved correctly."
                    ))

        # Fix footnotes directly inside bibliodiv
        for bibliodiv in list(root.iter('bibliodiv')):
            footnotes = [child for child in list(bibliodiv)
                        if self._local_name(child) == 'footnote']

            for footnote in footnotes:
                idx = list(bibliodiv).index(footnote)

                # Find a preceding bibliomixed to move the footnote into
                target_container = None
                for i in range(idx - 1, -1, -1):
                    sibling = bibliodiv[i]
                    if self._local_name(sibling) == 'bibliomixed':
                        target_container = sibling
                        break

                if target_container is not None:
                    bibliodiv.remove(footnote)
                    target_container.append(footnote)
                    fixes.append(f"Moved <footnote> from <bibliodiv> into <bibliomixed> in {filename}")
                else:
                    # Convert to bibliomixed
                    footnote_text = self._get_element_text(footnote)
                    bibliodiv.remove(footnote)
                    if footnote_text.strip():
                        bibliomixed = etree.Element('bibliomixed')
                        bibliomixed.text = f"[Note: {footnote_text}]"
                        bibliodiv.insert(idx, bibliomixed)
                        fixes.append(f"Converted orphan <footnote> to <bibliomixed> in <bibliodiv> in {filename}")
                    else:
                        fixes.append(f"Removed empty <footnote> from <bibliodiv> in {filename}")

        # Fix footnotes directly inside glossdiv
        for glossdiv in list(root.iter('glossdiv')):
            footnotes = [child for child in list(glossdiv)
                        if self._local_name(child) == 'footnote']

            for footnote in footnotes:
                idx = list(glossdiv).index(footnote)

                # Find a preceding glossentry to move the footnote into
                target_container = None
                for i in range(idx - 1, -1, -1):
                    sibling = glossdiv[i]
                    if self._local_name(sibling) == 'glossentry':
                        glossdef = sibling.find('glossdef')
                        if glossdef is not None:
                            para = glossdef.find('para')
                            if para is not None:
                                target_container = para
                                break

                if target_container is not None:
                    glossdiv.remove(footnote)
                    target_container.append(footnote)
                    fixes.append(f"Moved <footnote> from <glossdiv> into <para> in {filename}")
                else:
                    # Remove the footnote
                    footnote_text = self._get_element_text(footnote)
                    glossdiv.remove(footnote)
                    if footnote_text.strip():
                        fixes.append(f"Removed orphan <footnote> from <glossdiv> (content: '{footnote_text[:50]}...') in {filename}")
                    else:
                        fixes.append(f"Removed empty <footnote> from <glossdiv> in {filename}")

        return fixes

    def _strip_escaped_mathml(self, root: etree._Element, filename: str) -> List[str]:
        """Strip escaped MathML markup from text content.

        EPUB sources often embed MathML inside HTML comments alongside <img>
        fallback elements.  If BeautifulSoup's Comment nodes are not filtered
        during conversion, the raw MathML markup leaks into text content as
        escaped angle brackets (e.g. ``<math ...><mi>x</mi></math>``).  This
        method removes those fragments so the final XML is clean.
        """
        fixes = []
        mathml_re = re.compile(r'<math\b[^>]*>.*?</math>', re.DOTALL)

        for elem in root.iter():
            if not isinstance(elem.tag, str):
                continue
            for attr_name in ('text', 'tail'):
                value = getattr(elem, attr_name)
                if value and '<math' in value:
                    cleaned = mathml_re.sub('', value)
                    if cleaned != value:
                        setattr(elem, attr_name, cleaned)
                        fixes.append(f"Stripped escaped MathML from <{elem.tag}> {attr_name} in {filename}")
        return fixes

    def _fix_mediaobject_in_bibliomixed(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix mediaobject elements inside bibliomixed - not allowed by DTD.

        DTD: bibliomixed allows (#PCDATA | bibliocomponent.mix | bibliomset)*
        mediaobject is NOT in bibliocomponent.mix.

        Strategy: Extract mediaobject from bibliomixed, wrap it in a <figure>,
        and place it immediately after the containing bibliography/bibliodiv element
        so it appears below that reference section.
        """
        fixes = []

        for bibliomixed in list(root.iter('bibliomixed')):
            mediaobjects = [c for c in list(bibliomixed) if self._local_name(c) == 'mediaobject']
            for mo in mediaobjects:
                # Find the containing bibliography or bibliodiv
                container = bibliomixed.getparent()
                while container is not None and self._local_name(container) not in ('bibliography', 'bibliodiv'):
                    container = container.getparent()

                if container is None:
                    continue

                container_parent = container.getparent()
                if container_parent is None:
                    continue

                # Create a figure to wrap the mediaobject
                figure = etree.Element('figure')
                title_elem = etree.SubElement(figure, 'title')
                title_elem.text = ''

                # Preserve tail text
                mo_tail = mo.tail
                mo.tail = None
                figure.tail = mo_tail

                bibliomixed.remove(mo)
                figure.append(mo)

                # Place figure right after the bibliography/bibliodiv container
                container_idx = list(container_parent).index(container)
                container_parent.insert(container_idx + 1, figure)
                fixes.append(f"Moved <mediaobject> from <bibliomixed> to <figure> after <{self._local_name(container)}> in {filename}")

        return fixes

    def _fix_listitem_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix listitem elements that have no content.

        DTD: listitem must contain (list.class | admon.class | linespecific.class |
             synop.class | para.class | informal.class | formal.class)+

        Strategy: Add an empty <para/> as placeholder content.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for listitem in list(root.iter('listitem')):
            # Check if listitem has any element children
            children = [c for c in listitem if isinstance(c.tag, str)]

            if not children:
                # No element children - add placeholder para
                para = etree.Element('para')
                # Preserve any text content
                if listitem.text and listitem.text.strip():
                    para.text = listitem.text
                    listitem.text = None
                listitem.append(para)
                fixes.append(f"Added <para> to empty <listitem> in {filename}")

        return fixes

    def _fix_varlistentry_structure(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix varlistentry elements with invalid structure.

        DTD: varlistentry must contain (term+, listitem)
        - At least one term is REQUIRED
        - Exactly one listitem is REQUIRED

        Strategy:
        - Add empty <term/> if missing
        - Add <listitem><para/></listitem> if missing

        Returns:
            List of fix descriptions
        """
        fixes = []

        for varlistentry in list(root.iter('varlistentry')):
            terms = [c for c in varlistentry if self._local_name(c) == 'term']
            listitems = [c for c in varlistentry if self._local_name(c) == 'listitem']

            # Check for missing term
            if not terms:
                term = etree.Element('term')
                # Insert term at the beginning
                varlistentry.insert(0, term)
                fixes.append(f"Added missing <term> to <varlistentry> in {filename}")

            # Check for missing listitem
            if not listitems:
                listitem = etree.Element('listitem')
                para = etree.Element('para')
                listitem.append(para)
                varlistentry.append(listitem)
                fixes.append(f"Added missing <listitem> to <varlistentry> in {filename}")

        return fixes

    def _fix_qandaentry_structure(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix qandaentry elements with invalid structure.

        DTD: qandaentry must contain (blockinfo?, revhistory?, question, answer*)
        - question is REQUIRED

        Strategy: Add empty <question><para/></question> if missing

        Returns:
            List of fix descriptions
        """
        fixes = []

        for qandaentry in list(root.iter('qandaentry')):
            questions = [c for c in qandaentry if self._local_name(c) == 'question']

            if not questions:
                # Create question with placeholder para
                question = etree.Element('question')
                para = etree.Element('para')
                question.append(para)

                # Insert after blockinfo/revhistory if present, else at start
                insert_pos = 0
                for i, child in enumerate(qandaentry):
                    if self._local_name(child) in ('blockinfo', 'revhistory'):
                        insert_pos = i + 1

                qandaentry.insert(insert_pos, question)
                fixes.append(f"Added missing <question> to <qandaentry> in {filename}")

        return fixes

    def _fix_glossentry_structure(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix glossentry elements with invalid structure.

        DTD: glossentry must contain (glossterm, acronym?, abbrev?, ndxterm*,
             revhistory?, (glosssee | glossdef+))
        - glossterm is REQUIRED
        - Either glosssee OR glossdef+ is REQUIRED

        Strategy:
        - Add empty <glossterm/> if missing
        - Add <glossdef><para/></glossdef> if neither glosssee nor glossdef present

        Returns:
            List of fix descriptions
        """
        fixes = []

        for glossentry in list(root.iter('glossentry')):
            glossterms = [c for c in glossentry if self._local_name(c) == 'glossterm']
            glosssees = [c for c in glossentry if self._local_name(c) == 'glosssee']
            glossdefs = [c for c in glossentry if self._local_name(c) == 'glossdef']

            # Check for missing glossterm
            if not glossterms:
                glossterm = etree.Element('glossterm')
                glossentry.insert(0, glossterm)
                fixes.append(f"Added missing <glossterm> to <glossentry> in {filename}")

            # Check for missing glosssee/glossdef
            if not glosssees and not glossdefs:
                glossdef = etree.Element('glossdef')
                para = etree.Element('para')
                glossdef.append(para)
                glossentry.append(glossdef)
                fixes.append(f"Added missing <glossdef> to <glossentry> in {filename}")

        return fixes

    def _fix_empty_admonitions(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix admonition elements (note, warning, caution, important, tip) with no content.

        DTD: (title?, admon.mix+)
        - Must have at least one admon.mix element

        Strategy: Add empty <para/> as placeholder content.

        Returns:
            List of fix descriptions
        """
        fixes = []

        admonition_tags = {'note', 'warning', 'caution', 'important', 'tip'}

        for tag in admonition_tags:
            for admon in list(root.iter(tag)):
                # Get non-title children
                content_children = [c for c in admon
                                   if isinstance(c.tag, str) and self._local_name(c) != 'title']

                if not content_children:
                    # No content - add placeholder para
                    para = etree.Element('para')
                    admon.append(para)
                    fixes.append(f"Added <para> to empty <{tag}> in {filename}")

        return fixes

    def _fix_imageobject_missing_imagedata(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix imageobject elements missing required imagedata child.

        DTD: imageobject must contain (objectinfo?, imagedata)
        - imagedata is REQUIRED

        Strategy: Add <imagedata fileref="missing.png"/> placeholder

        Returns:
            List of fix descriptions
        """
        fixes = []

        for imageobject in list(root.iter('imageobject')):
            imagedatas = [c for c in imageobject if self._local_name(c) == 'imagedata']

            if not imagedatas:
                imagedata = etree.Element('imagedata')
                imagedata.set('fileref', 'MultiMedia/missing.png')
                imageobject.append(imagedata)
                fixes.append(f"Added missing <imagedata> to <imageobject> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=imageobject.sourceline if hasattr(imageobject, 'sourceline') else None,
                        fix_type="Missing Imagedata",
                        fix_description="Added placeholder imagedata with fileref='MultiMedia/missing.png'",
                        verification_reason="DTD requires imagedata inside imageobject",
                        suggestion="Replace placeholder with actual image reference."
                    ))

        return fixes

    def _fix_blockquote_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix blockquote elements with no content.

        DTD: blockquote must contain (blockinfo?, title?, attribution?, component.mix+)
        - At least one component.mix element is REQUIRED

        Strategy: Add empty <para/> as placeholder content.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for blockquote in list(root.iter('blockquote')):
            # Get content children (not blockinfo, title, attribution)
            meta_tags = {'blockinfo', 'title', 'attribution'}
            content_children = [c for c in blockquote
                               if isinstance(c.tag, str) and self._local_name(c) not in meta_tags]

            if not content_children:
                para = etree.Element('para')
                blockquote.append(para)
                fixes.append(f"Added <para> to empty <blockquote> in {filename}")

        return fixes

    def _fix_sidebar_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix sidebar elements with no content.

        DTD: sidebar must contain (sidebarinfo?, title?, sidebar.mix+)
        - At least one sidebar.mix element is REQUIRED

        Strategy: Add empty <para/> as placeholder content.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for sidebar in list(root.iter('sidebar')):
            # Get content children (not sidebarinfo, title)
            meta_tags = {'sidebarinfo', 'title'}
            content_children = [c for c in sidebar
                               if isinstance(c.tag, str) and self._local_name(c) not in meta_tags]

            if not content_children:
                para = etree.Element('para')
                sidebar.append(para)
                fixes.append(f"Added <para> to empty <sidebar> in {filename}")

        return fixes

    def _fix_glossary_bare_paras(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix glossary elements that contain bare <para> elements instead of <glossentry>.

        DTD requires: glossary -> (glossdiv+ | glossentry+)
        But converter sometimes produces: glossary -> bridgehead, para, para, ...

        Strategy: Convert <glossary> to <sect1 role="Glossary"> (or sect2/sect3 if nested)
        since the content is just terms as paragraphs, not structured glossentry elements.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for glossary in list(root.iter('glossary')):
            # Check if this glossary has bare <para> children (invalid)
            has_para = any(
                isinstance(c.tag, str) and self._local_name(c) == 'para'
                for c in glossary
            )
            has_glossentry = any(
                isinstance(c.tag, str) and self._local_name(c) in ('glossentry', 'glossdiv')
                for c in glossary
            )

            if has_para and not has_glossentry:
                # Determine correct target tag based on parent context
                # sect1 cannot nest inside another sect1 — use sect2 if parent is sect1
                parent = glossary.getparent()
                parent_tag = parent.tag if parent is not None and isinstance(parent.tag, str) else ''
                parent_local = self._local_name(parent) if parent is not None and isinstance(parent.tag, str) else ''

                if parent_local == 'sect1':
                    target_tag = 'sect2'
                elif parent_local == 'sect2':
                    target_tag = 'sect3'
                else:
                    target_tag = 'sect1'

                role = glossary.get('role', 'Glossary')
                glossary.tag = target_tag
                glossary.set('role', role)

                # Fix title content: convert bridgehead to title, and
                # unwrap any <para> inside <title> (not allowed by DTD)
                for child in list(glossary):
                    local = self._local_name(child) if isinstance(child.tag, str) else ''
                    if local == 'bridgehead':
                        child.tag = 'title'
                        if 'role' in child.attrib:
                            del child.attrib['role']
                        local = 'title'  # Update local so the next check matches
                    if local == 'title':
                        # Unwrap <para> inside <title> — <para> is not allowed in <title>
                        for title_child in list(child):
                            tc_local = self._local_name(title_child) if isinstance(title_child.tag, str) else ''
                            if tc_local == 'para':
                                # Move para's text content to the title element
                                para_text = title_child.text or ''
                                child.text = (child.text or '') + para_text
                                # Move para's children to title
                                for pc in list(title_child):
                                    child.append(pc)
                                # Preserve tail text
                                if title_child.tail:
                                    if len(child) > 0:
                                        last = child[-1]
                                        last.tail = (last.tail or '') + title_child.tail
                                    else:
                                        child.text = (child.text or '') + title_child.tail
                                child.remove(title_child)

                # Add an id if missing
                if not glossary.get('id'):
                    parent_id = parent.get('id', 'unknown') if parent is not None else 'unknown'
                    glossary.set('id', f"{parent_id}_glossary")

                fixes.append(f"Converted <glossary> with bare <para> to <{target_tag} role='{role}'> in {filename}")

        return fixes

    def _fix_bibliodiv_missing_entries(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix bibliodiv elements with no bibliography entries.

        DTD: bibliodiv must contain (title?, component.mix*, (biblioentry | bibliomixed)+)
        - At least one biblioentry or bibliomixed is REQUIRED

        Strategy: Add empty <bibliomixed/> as placeholder.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for bibliodiv in list(root.iter('bibliodiv')):
            entries = [c for c in bibliodiv
                      if self._local_name(c) in ('biblioentry', 'bibliomixed')]

            if not entries:
                bibliomixed = etree.Element('bibliomixed')
                bibliodiv.append(bibliomixed)
                fixes.append(f"Added <bibliomixed> to empty <bibliodiv> in {filename}")

        return fixes

    def _fix_example_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix example elements with no content or missing title.

        DTD: example must contain (blockinfo?, title, example.mix+)
        - title is REQUIRED
        - At least one example.mix element is REQUIRED

        Strategy:
        - Add empty <title/> if missing
        - Add empty <para/> as placeholder content if no content

        Returns:
            List of fix descriptions
        """
        fixes = []

        for example in list(root.iter('example')):
            titles = [c for c in example if self._local_name(c) == 'title']

            # Get content children (not blockinfo, title)
            meta_tags = {'blockinfo', 'title'}
            content_children = [c for c in example
                               if isinstance(c.tag, str) and self._local_name(c) not in meta_tags]

            # Add title if missing
            if not titles:
                title = etree.Element('title')
                # Insert after blockinfo if present
                insert_pos = 0
                for i, child in enumerate(example):
                    if self._local_name(child) == 'blockinfo':
                        insert_pos = i + 1
                        break
                example.insert(insert_pos, title)
                fixes.append(f"Added missing <title> to <example> in {filename}")

            # Add content if missing
            if not content_children:
                para = etree.Element('para')
                example.append(para)
                fixes.append(f"Added <para> to empty <example> in {filename}")

        return fixes

    def _fix_figure_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix figure elements with no media content.

        DTD: figure must contain (blockinfo?, title, (figure.mix | link.char.class)+)
        - title is REQUIRED
        - At least one figure.mix element (mediaobject, graphic) is REQUIRED

        Strategy:
        - Add empty <title/> if missing
        - Add placeholder <mediaobject> if no media content

        Returns:
            List of fix descriptions
        """
        fixes = []

        for figure in list(root.iter('figure')):
            titles = [c for c in figure if self._local_name(c) == 'title']
            media = [c for c in figure
                    if self._local_name(c) in ('mediaobject', 'graphic', 'inlinemediaobject')]

            # Add title if missing
            if not titles:
                title = etree.Element('title')
                insert_pos = 0
                for i, child in enumerate(figure):
                    if self._local_name(child) == 'blockinfo':
                        insert_pos = i + 1
                        break
                figure.insert(insert_pos, title)
                fixes.append(f"Added missing <title> to <figure> in {filename}")

            # Add mediaobject if missing
            if not media:
                mediaobject = etree.Element('mediaobject')
                imageobject = etree.Element('imageobject')
                imagedata = etree.Element('imagedata')
                imagedata.set('fileref', 'MultiMedia/missing.png')
                imageobject.append(imagedata)
                mediaobject.append(imageobject)
                figure.append(mediaobject)
                fixes.append(f"Added placeholder <mediaobject> to <figure> in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=figure.sourceline if hasattr(figure, 'sourceline') else None,
                        fix_type="Missing Figure Content",
                        fix_description="Added placeholder mediaobject with missing.png",
                        verification_reason="DTD requires media content inside figure",
                        suggestion="Replace placeholder with actual image."
                    ))

        return fixes

    def _fix_table_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix table elements with missing required structure.

        DTD (CALS): table must contain tgroup+ (or graphic+/mediaobject+)
        DTD (HTML): table must contain tbody+ or tr+

        Strategy: Add minimal valid table structure if missing.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for table in list(root.iter('table')):
            # Check for CALS tgroup or HTML tbody/tr
            tgroups = [c for c in table if self._local_name(c) == 'tgroup']
            tbodies = [c for c in table if self._local_name(c) == 'tbody']
            rows = [c for c in table if self._local_name(c) in ('tr', 'row')]
            graphics = [c for c in table if self._local_name(c) in ('graphic', 'mediaobject')]

            if not tgroups and not tbodies and not rows and not graphics:
                # Add minimal CALS table structure
                tgroup = etree.Element('tgroup')
                tgroup.set('cols', '1')
                tbody = etree.Element('tbody')
                row = etree.Element('row')
                entry = etree.Element('entry')
                para = etree.Element('para')
                entry.append(para)
                row.append(entry)
                tbody.append(row)
                tgroup.append(tbody)
                table.append(tgroup)
                fixes.append(f"Added minimal <tgroup> structure to empty <table> in {filename}")

        # Also fix informaltable
        for table in list(root.iter('informaltable')):
            tgroups = [c for c in table if self._local_name(c) == 'tgroup']
            tbodies = [c for c in table if self._local_name(c) == 'tbody']
            rows = [c for c in table if self._local_name(c) in ('tr', 'row')]
            graphics = [c for c in table if self._local_name(c) in ('graphic', 'mediaobject')]

            if not tgroups and not tbodies and not rows and not graphics:
                tgroup = etree.Element('tgroup')
                tgroup.set('cols', '1')
                tbody = etree.Element('tbody')
                row = etree.Element('row')
                entry = etree.Element('entry')
                para = etree.Element('para')
                entry.append(para)
                row.append(entry)
                tbody.append(row)
                tgroup.append(tbody)
                table.append(tgroup)
                fixes.append(f"Added minimal <tgroup> structure to empty <informaltable> in {filename}")

        return fixes

    # =========================================================================
    # PROCEDURE / STEP / CALLOUT HANDLERS
    # =========================================================================

    def _fix_procedure_missing_steps(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix procedure elements that have no step children.

        DTD: procedure must contain (blockinfo?, formalobject.title.content?, component.mix*, step+)
        - At least one step is REQUIRED at the end

        Strategy: Add a minimal <step><para/></step> if no steps exist.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for procedure in list(root.iter('procedure')):
            steps = [c for c in procedure if self._local_name(c) == 'step']

            if not steps:
                # Create minimal step with para content
                step = etree.Element('step')
                para = etree.Element('para')
                step.append(para)
                procedure.append(step)
                fixes.append(f"Added missing <step> to <procedure> in {filename}")

        return fixes

    def _fix_step_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix step elements that have no valid content.

        DTD: step must contain (title?, ((component.mix+, (substeps|stepalternatives, component.mix*)?)
             | ((substeps|stepalternatives), component.mix*)))
        - Must have either component.mix content OR substeps/stepalternatives

        Strategy: Add an empty <para/> if step has no block content or substeps.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Valid block content elements for step (component.mix)
        block_elements = {
            'para', 'simpara', 'formalpara', 'note', 'warning', 'caution', 'important', 'tip',
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist', 'segmentedlist',
            'calloutlist', 'glosslist', 'bibliolist', 'programlisting', 'literallayout',
            'screen', 'screenshot', 'synopsis', 'cmdsynopsis', 'funcsynopsis',
            'informalequation', 'informalexample', 'informalfigure', 'informaltable',
            'equation', 'example', 'figure', 'table', 'msgset', 'procedure', 'sidebar',
            'qandaset', 'anchor', 'bridgehead', 'remark', 'highlights', 'abstract',
            'authorblurb', 'epigraph', 'indexterm', 'mediaobject', 'graphic', 'blockquote'
        }

        for step in list(root.iter('step')):
            # Check for valid content
            has_block_content = False
            has_substeps = False

            for child in step:
                child_name = self._local_name(child)
                if child_name in block_elements:
                    has_block_content = True
                    break
                elif child_name in ('substeps', 'stepalternatives'):
                    has_substeps = True
                    break

            if not has_block_content and not has_substeps:
                # Add placeholder para after title if present
                para = etree.Element('para')
                # Preserve any direct text content
                if step.text and step.text.strip():
                    para.text = step.text
                    step.text = None

                # Find insert position (after title)
                insert_pos = 0
                for i, child in enumerate(step):
                    if self._local_name(child) == 'title':
                        insert_pos = i + 1
                        break

                step.insert(insert_pos, para)
                fixes.append(f"Added <para> to empty <step> in {filename}")

        return fixes

    def _fix_substeps_missing_steps(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix substeps elements that have no step children.

        DTD: substeps must contain (step+)
        - At least one step is REQUIRED

        Strategy: Add a minimal <step><para/></step> if no steps exist.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for substeps in list(root.iter('substeps')):
            steps = [c for c in substeps if self._local_name(c) == 'step']

            if not steps:
                step = etree.Element('step')
                para = etree.Element('para')
                step.append(para)
                substeps.append(step)
                fixes.append(f"Added missing <step> to <substeps> in {filename}")

        # Also fix stepalternatives (same content model)
        for stepalts in list(root.iter('stepalternatives')):
            steps = [c for c in stepalts if self._local_name(c) == 'step']

            if not steps:
                step = etree.Element('step')
                para = etree.Element('para')
                step.append(para)
                stepalts.append(step)
                fixes.append(f"Added missing <step> to <stepalternatives> in {filename}")

        return fixes

    def _fix_calloutlist_missing_callouts(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix calloutlist elements that have no callout children.

        DTD: calloutlist must contain (formalobject.title.content?, callout+)
        - At least one callout is REQUIRED

        Strategy: Add a minimal <callout arearefs=""><para/></callout> if none exist.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for calloutlist in list(root.iter('calloutlist')):
            callouts = [c for c in calloutlist if self._local_name(c) == 'callout']

            if not callouts:
                callout = etree.Element('callout')
                callout.set('arearefs', '')  # Required attribute
                para = etree.Element('para')
                callout.append(para)
                calloutlist.append(callout)
                fixes.append(f"Added missing <callout> to <calloutlist> in {filename}")

        return fixes

    def _fix_callout_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix callout elements that have no content.

        DTD: callout must contain (component.mix+)
        - At least one block element is REQUIRED

        Strategy: Add an empty <para/> if callout has no block content.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for callout in list(root.iter('callout')):
            # Check for any element children (block content)
            children = [c for c in callout if isinstance(c.tag, str)]

            if not children:
                para = etree.Element('para')
                # Preserve any text content
                if callout.text and callout.text.strip():
                    para.text = callout.text
                    callout.text = None
                callout.append(para)
                fixes.append(f"Added <para> to empty <callout> in {filename}")

        return fixes

    # =========================================================================
    # ABSTRACT / FORMALPARA / LEGALNOTICE HANDLERS
    # =========================================================================

    def _fix_abstract_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix abstract elements that have no para content.

        DTD: abstract must contain (title?, para.class+)
        - At least one para/simpara/formalpara is REQUIRED

        Strategy: Add an empty <para/> if no para content exists.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for abstract in list(root.iter('abstract')):
            # Check for para.class content
            para_elements = [c for c in abstract if self._local_name(c) in ('para', 'simpara', 'formalpara')]

            if not para_elements:
                para = etree.Element('para')
                # Preserve any text content
                if abstract.text and abstract.text.strip():
                    para.text = abstract.text
                    abstract.text = None
                # Insert after title if present
                insert_pos = 0
                for i, child in enumerate(abstract):
                    if self._local_name(child) == 'title':
                        insert_pos = i + 1
                        break
                abstract.insert(insert_pos, para)
                fixes.append(f"Added <para> to empty <abstract> in {filename}")

        return fixes

    def _fix_formalpara_structure(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix formalpara elements with invalid structure.

        DTD: formalpara must contain (title, ndxterm*, para)
        - title is REQUIRED
        - para is REQUIRED (exactly one)

        Strategy:
        - Add empty <title/> if missing
        - Add empty <para/> if missing

        Returns:
            List of fix descriptions
        """
        fixes = []

        for formalpara in list(root.iter('formalpara')):
            titles = [c for c in formalpara if self._local_name(c) == 'title']
            paras = [c for c in formalpara if self._local_name(c) == 'para']

            # Check for missing title
            if not titles:
                title = etree.Element('title')
                formalpara.insert(0, title)
                fixes.append(f"Added missing <title> to <formalpara> in {filename}")

            # Check for missing para
            if not paras:
                para = etree.Element('para')
                # Preserve any text content not in title
                if formalpara.text and formalpara.text.strip():
                    para.text = formalpara.text
                    formalpara.text = None
                formalpara.append(para)
                fixes.append(f"Added missing <para> to <formalpara> in {filename}")

        return fixes

    def _fix_legalnotice_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix legalnotice elements that have no content.

        DTD: legalnotice must contain (blockinfo?, title?, legalnotice.mix+)
        - legalnotice.mix = para.class | itemizedlist | orderedlist | variablelist | ...
        - At least one content element is REQUIRED

        Strategy: Add an empty <para/> if no content exists.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Content elements valid in legalnotice (legalnotice.mix)
        content_elements = {
            'para', 'simpara', 'formalpara', 'itemizedlist', 'orderedlist', 'variablelist',
            'simplelist', 'programlisting', 'literallayout', 'screen', 'screenco',
            'screenshot', 'blockquote', 'sidebar', 'note', 'warning', 'caution',
            'important', 'tip', 'mediaobject', 'graphic', 'anchor', 'remark',
            'bridgehead', 'indexterm'
        }

        for legalnotice in list(root.iter('legalnotice')):
            has_content = False
            for child in legalnotice:
                if self._local_name(child) in content_elements:
                    has_content = True
                    break

            if not has_content:
                para = etree.Element('para')
                # Preserve any text content
                if legalnotice.text and legalnotice.text.strip():
                    para.text = legalnotice.text
                    legalnotice.text = None
                # Insert after blockinfo/title if present
                insert_pos = 0
                for i, child in enumerate(legalnotice):
                    if self._local_name(child) in ('blockinfo', 'title'):
                        insert_pos = i + 1
                legalnotice.insert(insert_pos, para)
                fixes.append(f"Added <para> to empty <legalnotice> in {filename}")

        return fixes

    # =========================================================================
    # EPIGRAPH / HIGHLIGHTS / SIMPLESECT HANDLERS
    # =========================================================================

    def _fix_epigraph_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix epigraph elements that have no content.

        DTD: epigraph must contain (attribution?, (para.class | literallayout)+)
        - At least one para/simpara/formalpara OR literallayout is REQUIRED

        Strategy: Add an empty <para/> if no content exists.

        Returns:
            List of fix descriptions
        """
        fixes = []

        content_elements = {'para', 'simpara', 'formalpara', 'literallayout'}

        for epigraph in list(root.iter('epigraph')):
            has_content = False
            for child in epigraph:
                if self._local_name(child) in content_elements:
                    has_content = True
                    break

            if not has_content:
                para = etree.Element('para')
                # Preserve any text content
                if epigraph.text and epigraph.text.strip():
                    para.text = epigraph.text
                    epigraph.text = None
                # Insert after attribution if present
                insert_pos = 0
                for i, child in enumerate(epigraph):
                    if self._local_name(child) == 'attribution':
                        insert_pos = i + 1
                        break
                epigraph.insert(insert_pos, para)
                fixes.append(f"Added <para> to empty <epigraph> in {filename}")

        return fixes

    def _fix_highlights_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix highlights elements that have no content.

        DTD: highlights must contain (highlights.mix+)
        - highlights.mix = para.class | itemizedlist | orderedlist | calloutlist | note...
        - At least one element is REQUIRED

        Strategy: Add an empty <para/> if no content exists.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # highlights.mix elements
        content_elements = {
            'para', 'simpara', 'formalpara', 'itemizedlist', 'orderedlist',
            'calloutlist', 'note', 'warning', 'caution', 'important', 'tip',
            'indexterm'
        }

        for highlights in list(root.iter('highlights')):
            has_content = False
            for child in highlights:
                if self._local_name(child) in content_elements:
                    has_content = True
                    break

            if not has_content:
                para = etree.Element('para')
                # Preserve any text content
                if highlights.text and highlights.text.strip():
                    para.text = highlights.text
                    highlights.text = None
                highlights.append(para)
                fixes.append(f"Added <para> to empty <highlights> in {filename}")

        return fixes

    def _fix_simplesect_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix simplesect elements that have no content.

        DTD: simplesect must contain (sect.title.content, divcomponent.mix+)
        - title is REQUIRED
        - At least one block element is REQUIRED after title

        Strategy:
        - Add empty <title/> if missing
        - Add empty <para/> if no block content exists

        Returns:
            List of fix descriptions
        """
        fixes = []

        # divcomponent.mix elements
        block_elements = {
            'para', 'simpara', 'formalpara', 'note', 'warning', 'caution', 'important', 'tip',
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist', 'segmentedlist',
            'calloutlist', 'glosslist', 'bibliolist', 'programlisting', 'literallayout',
            'screen', 'screenshot', 'synopsis', 'cmdsynopsis', 'funcsynopsis',
            'informalequation', 'informalexample', 'informalfigure', 'informaltable',
            'equation', 'example', 'figure', 'table', 'msgset', 'procedure', 'sidebar',
            'qandaset', 'anchor', 'bridgehead', 'remark', 'highlights', 'abstract',
            'authorblurb', 'epigraph', 'indexterm', 'beginpage', 'blockquote', 'mediaobject'
        }

        for simplesect in list(root.iter('simplesect')):
            titles = [c for c in simplesect if self._local_name(c) == 'title']
            has_block_content = False

            for child in simplesect:
                if self._local_name(child) in block_elements:
                    has_block_content = True
                    break

            # Add missing title
            if not titles:
                title = etree.Element('title')
                simplesect.insert(0, title)
                fixes.append(f"Added missing <title> to <simplesect> in {filename}")

            # Add missing block content
            if not has_block_content:
                para = etree.Element('para')
                # Preserve any text content
                if simplesect.text and simplesect.text.strip():
                    para.text = simplesect.text
                    simplesect.text = None
                # Insert after title/subtitle/titleabbrev
                insert_pos = 0
                for i, child in enumerate(simplesect):
                    if self._local_name(child) in ('title', 'subtitle', 'titleabbrev'):
                        insert_pos = i + 1
                simplesect.insert(insert_pos, para)
                fixes.append(f"Added <para> to empty <simplesect> in {filename}")

        return fixes

    # =========================================================================
    # INFORMAL ELEMENT HANDLERS
    # =========================================================================

    def _fix_informalfigure_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix informalfigure elements that have no content.

        DTD: informalfigure must contain (blockinfo?, figure.mix+)
        - figure.mix = graphic | mediaobject | link.char.class
        - At least one graphic/mediaobject is REQUIRED

        Strategy: Add placeholder <mediaobject><imageobject><imagedata/></imageobject></mediaobject>

        Returns:
            List of fix descriptions
        """
        fixes = []

        content_elements = {'graphic', 'mediaobject', 'inlinemediaobject', 'link', 'olink', 'ulink'}

        for informalfigure in list(root.iter('informalfigure')):
            has_content = False
            for child in informalfigure:
                if self._local_name(child) in content_elements:
                    has_content = True
                    break

            if not has_content:
                # Create minimal mediaobject structure
                mediaobject = etree.Element('mediaobject')
                imageobject = etree.Element('imageobject')
                imagedata = etree.Element('imagedata')
                imagedata.set('fileref', '')
                imageobject.append(imagedata)
                mediaobject.append(imageobject)
                # Insert after blockinfo if present
                insert_pos = 0
                for i, child in enumerate(informalfigure):
                    if self._local_name(child) == 'blockinfo':
                        insert_pos = i + 1
                        break
                informalfigure.insert(insert_pos, mediaobject)
                fixes.append(f"Added <mediaobject> to empty <informalfigure> in {filename}")

        return fixes

    def _fix_informalexample_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix informalexample elements that have no content.

        DTD: informalexample must contain (blockinfo?, example.mix+)
        - example.mix = divcomponent.mix subset
        - At least one block element is REQUIRED

        Strategy: Add an empty <para/> if no content exists.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # example.mix elements
        block_elements = {
            'para', 'simpara', 'formalpara', 'programlisting', 'literallayout',
            'screen', 'screenco', 'screenshot', 'synopsis', 'cmdsynopsis', 'funcsynopsis',
            'mediaobject', 'graphic', 'graphicco', 'informalequation', 'informalfigure',
            'informaltable', 'procedure', 'sidebar', 'blockquote', 'calloutlist',
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist', 'segmentedlist',
            'glosslist', 'bibliolist', 'qandaset', 'anchor', 'bridgehead', 'remark',
            'indexterm', 'beginpage'
        }

        for informalexample in list(root.iter('informalexample')):
            has_content = False
            for child in informalexample:
                if self._local_name(child) in block_elements:
                    has_content = True
                    break

            if not has_content:
                para = etree.Element('para')
                # Preserve any text content
                if informalexample.text and informalexample.text.strip():
                    para.text = informalexample.text
                    informalexample.text = None
                # Insert after blockinfo if present
                insert_pos = 0
                for i, child in enumerate(informalexample):
                    if self._local_name(child) == 'blockinfo':
                        insert_pos = i + 1
                        break
                informalexample.insert(insert_pos, para)
                fixes.append(f"Added <para> to empty <informalexample> in {filename}")

        return fixes

    def _fix_informalequation_missing_content(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix informalequation elements that have no content.

        DTD: informalequation must contain (blockinfo?, equation.content)
        - equation.content = (alt?, (graphic+|mediaobject+))
        - At least one graphic or mediaobject element is REQUIRED

        Strategy: Add placeholder <mediaobject><imageobject><imagedata/></imageobject></mediaobject>

        Returns:
            List of fix descriptions
        """
        fixes = []

        content_elements = {'graphic', 'mediaobject', 'inlinemediaobject', 'alt'}

        for informalequation in list(root.iter('informalequation')):
            has_content = False
            for child in informalequation:
                if self._local_name(child) in content_elements:
                    has_content = True
                    break

            if not has_content:
                # Create minimal mediaobject structure for equation placeholder
                mediaobject = etree.Element('mediaobject')
                imageobject = etree.Element('imageobject')
                imagedata = etree.Element('imagedata')
                imagedata.set('fileref', '')
                imageobject.append(imagedata)
                mediaobject.append(imageobject)
                # Insert after blockinfo if present
                insert_pos = 0
                for i, child in enumerate(informalequation):
                    if self._local_name(child) == 'blockinfo':
                        insert_pos = i + 1
                        break
                informalequation.insert(insert_pos, mediaobject)
                fixes.append(f"Added <mediaobject> to empty <informalequation> in {filename}")

        return fixes

    # =========================================================================
    # MSGSET / SEGMENTEDLIST HANDLERS
    # =========================================================================

    def _fix_msgset_missing_entries(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix msgset elements that have no msgentry/simplemsgentry children.

        DTD: msgset must contain (blockinfo?, formalobject.title.content?, (msgentry+ | simplemsgentry+))
        - At least one msgentry OR simplemsgentry is REQUIRED

        Strategy: Add minimal <simplemsgentry><msgtext><para/></msgtext></simplemsgentry>

        Returns:
            List of fix descriptions
        """
        fixes = []

        for msgset in list(root.iter('msgset')):
            msgentries = [c for c in msgset if self._local_name(c) in ('msgentry', 'simplemsgentry')]

            if not msgentries:
                # Create minimal simplemsgentry structure
                simplemsgentry = etree.Element('simplemsgentry')
                msgtext = etree.Element('msgtext')
                para = etree.Element('para')
                msgtext.append(para)
                simplemsgentry.append(msgtext)
                msgset.append(simplemsgentry)
                fixes.append(f"Added missing <simplemsgentry> to <msgset> in {filename}")

        return fixes

    def _fix_segmentedlist_structure(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix segmentedlist elements with invalid structure.

        DTD: segmentedlist must contain (formalobject.title.content?, segtitle+, seglistitem+)
        - At least one segtitle is REQUIRED
        - At least one seglistitem is REQUIRED

        Strategy:
        - Add empty <segtitle/> if missing
        - Add <seglistitem><seg/></seglistitem> if missing

        Returns:
            List of fix descriptions
        """
        fixes = []

        for segmentedlist in list(root.iter('segmentedlist')):
            segtitles = [c for c in segmentedlist if self._local_name(c) == 'segtitle']
            seglistitems = [c for c in segmentedlist if self._local_name(c) == 'seglistitem']

            # Find position after title elements
            insert_pos = 0
            for i, child in enumerate(segmentedlist):
                if self._local_name(child) in ('title', 'subtitle', 'titleabbrev'):
                    insert_pos = i + 1

            # Add missing segtitle
            if not segtitles:
                segtitle = etree.Element('segtitle')
                segmentedlist.insert(insert_pos, segtitle)
                insert_pos += 1
                fixes.append(f"Added missing <segtitle> to <segmentedlist> in {filename}")

            # Add missing seglistitem
            if not seglistitems:
                seglistitem = etree.Element('seglistitem')
                seg = etree.Element('seg')
                seglistitem.append(seg)
                segmentedlist.append(seglistitem)
                fixes.append(f"Added missing <seglistitem> to <segmentedlist> in {filename}")

        return fixes

    def _fix_seglistitem_missing_seg(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix seglistitem elements that have no seg children.

        DTD: seglistitem must contain (seg+)
        - At least one seg is REQUIRED

        Strategy: Add an empty <seg/> if no seg exists.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for seglistitem in list(root.iter('seglistitem')):
            segs = [c for c in seglistitem if self._local_name(c) == 'seg']

            if not segs:
                seg = etree.Element('seg')
                # Preserve any text content
                if seglistitem.text and seglistitem.text.strip():
                    seg.text = seglistitem.text
                    seglistitem.text = None
                seglistitem.append(seg)
                fixes.append(f"Added missing <seg> to <seglistitem> in {filename}")

        return fixes

    def _fix_broken_cross_references(self, root: etree._Element, filename: str, valid_ids: Set[str]) -> List[str]:
        """
        Fix broken cross-references (linkend attributes pointing to non-existent IDs).

        Strategy:
        1. Try fuzzy matching to find similar valid ID and auto-correct
        2. Parse ID to generate human-readable text
        3. Convert to <emphasis> element (valid DocBook) with readable text

        Args:
            root: XML root element
            filename: Name of the file being fixed
            valid_ids: Set of all valid IDs across all chapters in the package

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Find all elements with linkend attributes (iterate over a copy to avoid modification issues)
        elements_to_fix = []
        for elem in root.iter():
            linkend = elem.get('linkend')
            if linkend and (linkend not in valid_ids or self._is_pagebreak_id(linkend)):
                elements_to_fix.append((elem, linkend))

        for elem, linkend in elements_to_fix:
            tag_name = self._local_name(elem)
            is_pagebreak_link = self._is_pagebreak_id(linkend)

            if not is_pagebreak_link:
                similar_id = self._find_similar_id(linkend, valid_ids)
                if similar_id:
                    elem.set('linkend', similar_id)
                    fixes.append(f"Fixed broken linkend '{linkend}' -> '{similar_id}' in {filename}")
                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=elem.sourceline if hasattr(elem, 'sourceline') else None,
                            fix_type="Cross-Reference Auto-Correction",
                            fix_description=f"Auto-corrected linkend '{linkend}' -> '{similar_id}'",
                            verification_reason=f"Original target '{linkend}' not found, auto-matched to similar ID '{similar_id}'",
                            suggestion="Verify this correction is accurate."
                        ))
                    continue

            if tag_name == 'xref':
                parent = elem.getparent()
                if parent is None:
                    continue

                # Strategy 2: Parse ID to generate readable text
                readable_text = self._parse_id_to_readable_text(linkend)

                # Use xref's own text if available, otherwise use parsed text
                if elem.text and elem.text.strip():
                    replacement_text = elem.text
                elif elem.get('endterm'):
                    replacement_text = elem.get('endterm')
                else:
                    replacement_text = readable_text

                # Convert to emphasis to preserve that it was special
                emphasis = etree.Element('emphasis')
                emphasis.text = replacement_text
                emphasis.tail = elem.tail

                # Replace xref with emphasis
                index = list(parent).index(elem)
                parent.insert(index, emphasis)
                parent.remove(elem)

                fixes.append(f"Converted broken <xref> to <emphasis>: '{linkend}' -> '{replacement_text}' in {filename}")

                if VALIDATION_REPORT_AVAILABLE:
                    self.verification_items.append(VerificationItem(
                        xml_file=filename,
                        line_number=emphasis.sourceline if hasattr(emphasis, 'sourceline') else None,
                        fix_type="Broken Cross-Reference Conversion",
                        fix_description=f"Converted broken <xref linkend='{linkend}'/> to <emphasis>{replacement_text}</emphasis>",
                        verification_reason=f"Target ID '{linkend}' not found. Generated readable text: '{readable_text}'",
                        suggestion="Verify the content is appropriate or locate the correct target."
                    ))

            elif tag_name == 'link':
                # <link> requires linkend per DTD - convert to <phrase> to preserve text
                parent = elem.getparent()
                if parent is None:
                    continue

                # Check if parent is a restrictive element that doesn't allow <phrase>
                # (superscript, subscript, code, etc. only allow #PCDATA or limited inline children)
                restrictive_parents = {'superscript', 'subscript', 'code', 'literal',
                                       'constant', 'varname', 'function', 'parameter'}
                parent_tag = parent.tag if hasattr(parent, 'tag') else ''

                if parent_tag in restrictive_parents:
                    # Unwrap content directly into parent (don't use phrase wrapper)
                    index = list(parent).index(elem)
                    # Handle text content
                    if elem.text:
                        if index == 0:
                            parent.text = (parent.text or '') + elem.text
                        else:
                            prev_sibling = parent[index - 1]
                            prev_sibling.tail = (prev_sibling.tail or '') + elem.text
                    # Move children to parent
                    for i, child in enumerate(list(elem)):
                        parent.insert(index + i, deepcopy(child))
                    # Handle tail
                    if elem.tail:
                        if len(list(elem)) > 0:
                            last_child = parent[index + len(list(elem)) - 1]
                            last_child.tail = (last_child.tail or '') + elem.tail
                        elif index == 0:
                            parent.text = (parent.text or '') + elem.tail
                        else:
                            prev_sibling = parent[index - 1]
                            prev_sibling.tail = (prev_sibling.tail or '') + elem.tail
                    parent.remove(elem)

                    fixes.append(f"Unwrapped broken <link linkend='{linkend}'> in restrictive parent <{parent_tag}> in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=elem.sourceline if hasattr(elem, 'sourceline') else None,
                            fix_type="Broken Link Conversion",
                            fix_description=f"Unwrapped <link linkend='{linkend}'> in restrictive parent <{parent_tag}>",
                            verification_reason=f"Target ID '{linkend}' not found; <phrase> not allowed in <{parent_tag}>",
                            suggestion="Verify the unwrapped content is appropriate."
                        ))
                else:
                    # Create phrase element to replace link
                    phrase = etree.Element('phrase')
                    phrase.text = elem.text
                    phrase.tail = elem.tail

                    # Copy any children
                    for child in elem:
                        phrase.append(deepcopy(child))

                    # Preserve role attribute if present
                    if elem.get('role'):
                        phrase.set('role', elem.get('role'))

                    # Replace link with phrase
                    index = list(parent).index(elem)
                    parent.insert(index, phrase)
                    parent.remove(elem)

                    fixes.append(f"Converted broken <link linkend='{linkend}'> to <phrase> in {filename}")

                    if VALIDATION_REPORT_AVAILABLE:
                        self.verification_items.append(VerificationItem(
                            xml_file=filename,
                            line_number=phrase.sourceline if hasattr(phrase, 'sourceline') else None,
                            fix_type="Broken Link Conversion",
                            fix_description=f"Converted <link linkend='{linkend}'> to <phrase>",
                            verification_reason=f"Target ID '{linkend}' not found and <link> requires valid linkend",
                            suggestion="Verify the converted text is appropriate."
                        ))

            elif tag_name == 'ulink':
                # For ulink elements, remove the linkend attribute but keep the element
                del elem.attrib['linkend']
                fixes.append(f"Removed broken linkend attribute '{linkend}' from <{tag_name}> in {filename}")

            else:
                # For other elements with linkend, remove the attribute
                del elem.attrib['linkend']
                fixes.append(f"Removed broken linkend attribute '{linkend}' from <{tag_name}> in {filename}")

        return fixes

    def _fix_sect_in_indexdiv(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix sect1/sect2/etc elements inside indexdiv (not allowed by DTD).

        DTD: indexdiv can contain (title?, (indexdivcomponent.mix)*, indexentry+)
        indexdivcomponent.mix does NOT include sect1/sect2/etc.

        Strategy: Unwrap sections, keeping their content. Title becomes bridgehead.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for indexdiv in list(root.iter('indexdiv')):
            for section_tag in ['sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'section']:
                for section in list(indexdiv.findall(section_tag)):
                    parent = section.getparent()
                    if parent is not None:
                        idx = list(parent).index(section)
                        # Convert title to para with bold emphasis (bridgehead is NOT allowed in indexdiv)
                        # indexdivcomponent.mix allows para but not bridgehead
                        title = section.find('title')
                        insert_offset = 0
                        if title is not None:
                            para = etree.Element('para')
                            emphasis = etree.SubElement(para, 'emphasis')
                            emphasis.set('role', 'bold')
                            emphasis.text = title.text or ''
                            # Copy any inline children from title
                            for child in list(title):
                                emphasis.append(child)
                            if title.tail:
                                para.tail = title.tail
                            parent.insert(idx, para)
                            insert_offset = 1
                        # Move other children to parent
                        for i, child in enumerate(list(section)):
                            if child.tag != 'title':
                                parent.insert(idx + insert_offset + i, child)
                        parent.remove(section)
                        fixes.append(f"Unwrapped <{section_tag}> from <indexdiv> in {filename}")

        return fixes

    def _fix_bridgehead_in_indexdiv(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix bridgehead elements inside indexdiv (not allowed by DTD).

        DTD: indexdiv can contain (title?, (indexdivcomponent.mix)*, indexentry+)
        indexdivcomponent.mix does NOT include bridgehead.

        Strategy: Convert bridgehead to para with bold emphasis.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Find all indexdiv elements
        indexdivs = list(root.iter('indexdiv'))
        if indexdivs:
            logger.debug(f"[DEBUG] Found {len(indexdivs)} indexdiv elements in {filename}")

        for indexdiv in indexdivs:
            # Find all bridgehead elements within this indexdiv
            bridgeheads = list(indexdiv.iter('bridgehead'))
            if bridgeheads:
                logger.debug(f"[DEBUG] Found {len(bridgeheads)} bridgehead elements in indexdiv in {filename}")

            for bridgehead in bridgeheads:
                parent = bridgehead.getparent()
                if parent is not None:
                    idx = list(parent).index(bridgehead)
                    # Convert bridgehead to para with emphasis
                    para = etree.Element('para')
                    emphasis = etree.SubElement(para, 'emphasis')
                    emphasis.set('role', 'bold')
                    emphasis.text = bridgehead.text or ''
                    # Copy any children
                    for child in list(bridgehead):
                        emphasis.append(child)
                    para.tail = bridgehead.tail
                    parent.remove(bridgehead)
                    parent.insert(idx, para)
                    fixes.append(f"Converted <bridgehead> to <para> with emphasis in <indexdiv> in {filename}")
                    logger.debug(f"[DEBUG] Converted bridgehead to para in {filename}")

        return fixes

    def _fix_bridgehead_in_index(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix bridgehead elements inside index (preventive measure).

        While bridgehead is technically allowed in index (via component.mix),
        content often gets reorganized into indexdiv where bridgehead is NOT allowed.
        Converting bridgeheads to para+emphasis prevents DTD errors when content
        is moved into indexdivs.

        Strategy: Convert bridgehead to para with bold emphasis.

        Returns:
            List of fix descriptions
        """
        fixes = []

        # Find all index elements
        indexes = list(root.iter('index'))
        if indexes:
            logger.debug(f"[DEBUG] Found {len(indexes)} index elements in {filename}")

        for index in indexes:
            # Find all bridgehead elements within this index (but not already in indexdiv)
            # First, get all bridgeheads in index
            all_bridgeheads = list(index.iter('bridgehead'))
            # Filter out those already inside indexdiv (handled by _fix_bridgehead_in_indexdiv)
            bridgeheads = []
            for bh in all_bridgeheads:
                in_indexdiv = False
                parent = bh.getparent()
                while parent is not None:
                    if self._local_name(parent) == 'indexdiv':
                        in_indexdiv = True
                        break
                    if parent == index:
                        break
                    parent = parent.getparent()
                if not in_indexdiv:
                    bridgeheads.append(bh)

            if bridgeheads:
                logger.debug(f"[DEBUG] Found {len(bridgeheads)} bridgehead elements directly in index in {filename}")

            for bridgehead in bridgeheads:
                parent = bridgehead.getparent()
                if parent is not None:
                    idx = list(parent).index(bridgehead)
                    # Convert bridgehead to para with emphasis
                    para = etree.Element('para')
                    emphasis = etree.SubElement(para, 'emphasis')
                    emphasis.set('role', 'bold')
                    emphasis.text = bridgehead.text or ''
                    # Copy any children
                    for child in list(bridgehead):
                        emphasis.append(child)
                    para.tail = bridgehead.tail
                    parent.remove(bridgehead)
                    parent.insert(idx, para)
                    fixes.append(f"Converted <bridgehead> to <para> with emphasis in <index> in {filename}")
                    logger.debug(f"[DEBUG] Converted bridgehead to para in index in {filename}")

        return fixes

    def _fix_bridgehead_in_abstract(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix bridgehead elements inside abstract (not allowed by DTD).

        DTD: abstract can contain (title?, para.class+)
        bridgehead is NOT in para.class.

        Strategy: Convert bridgehead to para with bold emphasis.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for abstract in list(root.iter('abstract')):
            for bridgehead in list(abstract.iter('bridgehead')):
                parent = bridgehead.getparent()
                if parent is not None:
                    idx = list(parent).index(bridgehead)
                    # Convert bridgehead to para with emphasis
                    para = etree.Element('para')
                    emphasis = etree.SubElement(para, 'emphasis')
                    emphasis.set('role', 'bold')
                    emphasis.text = bridgehead.text or ''
                    # Copy any children
                    for child in list(bridgehead):
                        emphasis.append(child)
                    para.tail = bridgehead.tail
                    parent.remove(bridgehead)
                    parent.insert(idx, para)
                    fixes.append(f"Converted <bridgehead> to <para> with emphasis in <abstract> in {filename}")

        return fixes

    def _fix_para_in_bridgehead(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix para elements inside bridgehead (not allowed by DTD).

        DTD: bridgehead can only contain title.char.mix (inline content).
        para is NOT inline content.

        Strategy: Flatten para, extracting text and inline children.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for bridgehead in list(root.iter('bridgehead')):
            for para in list(bridgehead.iter('para')):
                parent = para.getparent()
                if parent is not None:
                    # Flatten para: extract text and inline content
                    if para.text:
                        if parent == bridgehead:
                            if bridgehead.text:
                                bridgehead.text += ' ' + para.text
                            else:
                                bridgehead.text = para.text
                        else:
                            prev = para.getprevious()
                            if prev is not None:
                                if prev.tail:
                                    prev.tail += ' ' + para.text
                                else:
                                    prev.tail = para.text
                            elif parent.text:
                                parent.text += ' ' + para.text
                            else:
                                parent.text = para.text
                    # Move inline children up
                    idx = list(parent).index(para)
                    for i, child in enumerate(list(para)):
                        parent.insert(idx + i, child)
                    # Preserve tail
                    if para.tail:
                        children = list(para)
                        if children:
                            last = children[-1]
                            if last.tail:
                                last.tail += para.tail
                            else:
                                last.tail = para.tail
                        else:
                            if parent.text:
                                parent.text += para.tail
                            else:
                                parent.text = para.tail
                    parent.remove(para)
                    fixes.append(f"Flattened <para> content in <bridgehead> in {filename}")

        return fixes

    def _fix_para_in_subtitle(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix para elements inside subtitle (not allowed by DTD).

        DTD: subtitle can only contain title.char.mix (inline content).
        para is NOT inline content.

        Strategy: Flatten para, extracting text and inline children.

        Returns:
            List of fix descriptions
        """
        fixes = []

        for subtitle in list(root.iter('subtitle')):
            for para in list(subtitle.iter('para')):
                parent = para.getparent()
                if parent is not None:
                    # Flatten para: extract text and inline content
                    if para.text:
                        if parent == subtitle:
                            if subtitle.text:
                                subtitle.text += ' ' + para.text
                            else:
                                subtitle.text = para.text
                        else:
                            prev = para.getprevious()
                            if prev is not None:
                                if prev.tail:
                                    prev.tail += ' ' + para.text
                                else:
                                    prev.tail = para.text
                            elif parent.text:
                                parent.text += ' ' + para.text
                            else:
                                parent.text = para.text
                    # Move inline children up
                    idx = list(parent).index(para)
                    for i, child in enumerate(list(para)):
                        parent.insert(idx + i, child)
                    # Preserve tail
                    if para.tail:
                        children = list(para)
                        if children:
                            last = children[-1]
                            if last.tail:
                                last.tail += para.tail
                            else:
                                last.tail = para.tail
                        else:
                            if parent.text:
                                parent.text += para.tail
                            else:
                                parent.text = para.tail
                    parent.remove(para)
                    fixes.append(f"Flattened <para> content in <subtitle> in {filename}")

        return fixes

    def _fix_nested_tables(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix nested table elements (table inside table, not allowed by DTD).

        DTD: table contains (blockinfo?, title, titleabbrev?, indexterm*, textobject*, tgroup+)
        A child <table> is NOT allowed.

        Strategy: Unwrap nested table, keeping its tgroup(s).

        Returns:
            List of fix descriptions
        """
        fixes = []

        for table_tag in ['table', 'informaltable']:
            for table in list(root.iter(table_tag)):
                # Find direct child tables
                nested_tables = [c for c in table if self._local_name(c) in ('table', 'informaltable')]
                for nested_table in nested_tables:
                    nested_tag = self._local_name(nested_table)
                    idx = list(table).index(nested_table)
                    # Move tgroups from nested table to parent
                    tgroups = [c for c in nested_table if self._local_name(c) == 'tgroup']
                    for i, tgroup in enumerate(tgroups):
                        table.insert(idx + i, tgroup)
                    table.remove(nested_table)
                    fixes.append(f"Unwrapped nested <{nested_tag}> from <{table_tag}> in {filename}")

        return fixes

    def _fix_para_in_table(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix para elements directly inside table/informaltable (not allowed by DTD).

        DTD: table contains (blockinfo?, title, titleabbrev?, indexterm*, textobject*, tgroup+)
        para is NOT allowed as a direct child of table.

        Strategy: Move para elements outside the table (after it).

        Returns:
            List of fix descriptions
        """
        fixes = []

        for table_tag in ['table', 'informaltable']:
            tables = list(root.iter(table_tag))
            for table in tables:
                # Find direct child para elements (not in entries)
                paras_to_move = [c for c in table if self._local_name(c) == 'para']
                if not paras_to_move:
                    continue

                logger.debug(f"[DEBUG] Found {len(paras_to_move)} para elements directly inside <{table_tag}> in {filename}")
                # Log the table's direct children for debugging
                direct_children = [self._local_name(c) for c in table]
                logger.debug(f"[DEBUG] Table direct children: {direct_children}")

                parent = table.getparent()
                if parent is None:
                    logger.debug(f"[DEBUG] Table has no parent, skipping")
                    continue

                table_idx = list(parent).index(table)

                # Move all paras to after the table
                for i, para in enumerate(paras_to_move):
                    table.remove(para)
                    parent.insert(table_idx + 1 + i, para)
                    fixes.append(f"Moved <para> from inside <{table_tag}> to after table in {filename}")
                    logger.debug(f"[DEBUG] Moved para to after table in {filename}")

        return fixes

    def _fix_indexdiv_missing_indexentry(self, root: etree._Element, filename: str) -> List[str]:
        """
        Fix indexdiv elements that don't end with indexentry+ or segmentedlist (required by DTD).

        DTD: indexdiv can contain (title?, (indexdivcomponent.mix)*, (indexentry+ | segmentedlist))
        The indexentry+ or segmentedlist is REQUIRED, not optional.

        Strategy:
        - If indexdiv has indexentry children, ensure they're at the end
        - If no indexentry, try to convert suitable content (itemizedlist) to indexentry format
        - Otherwise, create minimal indexentry placeholder

        Returns:
            List of fix descriptions
        """
        fixes = []

        for indexdiv in list(root.iter('indexdiv')):
            # Check if indexdiv already has indexentry or segmentedlist children
            has_indexentry = any(self._local_name(c) == 'indexentry' for c in indexdiv)
            has_segmentedlist = any(self._local_name(c) == 'segmentedlist' for c in indexdiv)

            if has_indexentry or has_segmentedlist:
                # Has required elements - but make sure indexentry/segmentedlist are at the end
                children = list(indexdiv)
                last_valid_idx = -1
                for i, child in enumerate(children):
                    tag = self._local_name(child)
                    if tag in ('indexentry', 'segmentedlist'):
                        last_valid_idx = i

                # Check if there are non-indexentry/segmentedlist elements after the last valid one
                if last_valid_idx >= 0 and last_valid_idx < len(children) - 1:
                    # Move trailing non-index elements before the first indexentry
                    first_index_idx = None
                    for i, child in enumerate(children):
                        if self._local_name(child) in ('indexentry', 'segmentedlist'):
                            first_index_idx = i
                            break

                    if first_index_idx is not None:
                        trailing = children[last_valid_idx + 1:]
                        for elem in trailing:
                            indexdiv.remove(elem)
                            indexdiv.insert(first_index_idx, elem)
                            first_index_idx += 1
                            fixes.append(f"Reordered elements in <indexdiv> to put indexentry at end in {filename}")
                continue

            # No indexentry or segmentedlist - need to add one
            # Try to convert itemizedlist to indexentry format if present
            itemizedlist = indexdiv.find('itemizedlist')
            if itemizedlist is not None:
                # Convert itemizedlist items to indexentry elements
                for listitem in list(itemizedlist.findall('listitem')):
                    indexentry = etree.Element('indexentry')
                    primaryie = etree.SubElement(indexentry, 'primaryie')

                    # Get text from listitem's para or direct text
                    para = listitem.find('para')
                    if para is not None:
                        primaryie.text = ''.join(para.itertext()).strip() or 'Index entry'
                    else:
                        primaryie.text = ''.join(listitem.itertext()).strip() or 'Index entry'

                    indexdiv.append(indexentry)

                # Remove the converted itemizedlist
                indexdiv.remove(itemizedlist)
                fixes.append(f"Converted <itemizedlist> to <indexentry> elements in <indexdiv> in {filename}")
            else:
                # No itemizedlist - create a minimal placeholder indexentry
                indexentry = etree.Element('indexentry')
                primaryie = etree.SubElement(indexentry, 'primaryie')
                # Try to get text from existing content
                existing_text = ''.join(indexdiv.itertext()).strip()
                if existing_text:
                    # Use first 50 chars as index term
                    primaryie.text = existing_text[:50].strip()
                else:
                    primaryie.text = 'Index'
                indexdiv.append(indexentry)
                fixes.append(f"Added placeholder <indexentry> to <indexdiv> in {filename}")

        return fixes


def fix_toc_links(book_xml_path: Path, chapters_dir: Path) -> int:
    """
    Fix all TOC links to point to actual section IDs in chapters.

    This function:
    1. Parses Book.XML TOC entries
    2. Loads each chapter to find actual section IDs and titles
    3. Checks if TOC fragment IDs exist in the chapter
    4. If ID doesn't exist, matches by title to find correct section
    5. Updates TOC URLs to use correct section IDs

    Args:
        book_xml_path: Path to Book.XML file
        chapters_dir: Directory containing chapter XML files

    Returns:
        Number of TOC links fixed
    """
    print("\n=== Fixing Table of Contents Links ===")

    # Parse Book.XML
    parser = etree.XMLParser(remove_blank_text=True, resolve_entities=False, load_dtd=False)
    try:
        tree = etree.parse(str(book_xml_path), parser)
        root = tree.getroot()
    except Exception as e:
        print(f"  ! Warning: Could not parse Book.XML: {e}")
        return 0

    fixes_count = 0
    modified_chapters = set()

    # Find all TOC entries with URLs (including all levels)
    toc = root.find('.//toc')
    if toc is None:
        print("  ! No TOC found in Book.XML")
        return 0

    # Process ALL ulink elements in TOC
    for toc_entry in toc.findall('.//ulink[@url]'):
        toc_url = toc_entry.get('url', '')
        toc_text = ''.join(toc_entry.itertext()).strip()

        if not toc_url or '#' not in toc_url:
            continue

        # Extract chapter file and fragment ID
        toc_chapter, toc_fragment = toc_url.split('#', 1)
        chapter_path = chapters_dir / toc_chapter

        if not chapter_path.exists():
            continue

        # Load chapter to check if ID exists
        try:
            chapter_tree = etree.parse(str(chapter_path), parser)
            chapter_root = chapter_tree.getroot()
        except Exception as e:
            print(f"  ! Warning: Could not parse {toc_chapter}: {e}")
            continue

        # Check if the fragment ID exists in the chapter
        id_exists = False
        for elem in chapter_root.iter():
            if elem.get('id') == toc_fragment:
                id_exists = True
                break

        # If ID doesn't exist, try to find the correct section by title matching
        if not id_exists:
            # Normalize TOC text for matching
            toc_title_normalized = toc_text.lower().strip()
            # Remove numbering (e.g., "1.1 ", "2.3.4 ")
            toc_title_normalized = re.sub(r'^\d+(\.\d+)*\s+', '', toc_title_normalized)
            # Remove special characters
            toc_title_normalized = re.sub(r'[^\w\s]', '', toc_title_normalized)

            # Try to find matching section by title in sect1 and sect2
            found_match = False
            for section_tag in ['sect2', 'sect1']:
                if found_match:
                    break

                for section in chapter_root.iter(section_tag):
                    section_title = section.find('title')
                    if section_title is not None and section_title.text:
                        sect_title_normalized = section_title.text.lower().strip()
                        sect_title_normalized = re.sub(r'^\d+(\.\d+)*\s+', '', sect_title_normalized)
                        sect_title_normalized = re.sub(r'[^\w\s]', '', sect_title_normalized)

                        # Check for title match
                        if sect_title_normalized == toc_title_normalized or \
                           (len(toc_title_normalized) > 5 and toc_title_normalized in sect_title_normalized) or \
                           (len(sect_title_normalized) > 5 and sect_title_normalized in toc_title_normalized):

                            actual_id = section.get('id')
                            if actual_id:
                                # Update TOC URL to point to correct section ID
                                new_url = f"{toc_chapter}#{actual_id}"
                                toc_entry.set('url', new_url)
                                fixes_count += 1
                                print(f"  [OK] Fixed TOC link: '{toc_text[:50]}...' -> {new_url}")

                                # Mark chapter as modified
                                modified_chapters.add(chapter_path)
                                found_match = True
                                break

            if not found_match:
                print(f"  ! Warning: Could not find section for TOC entry '{toc_text[:50]}...' (ID: {toc_fragment})")

    # Save updated Book.XML
    if fixes_count > 0:
        try:
            tree.write(str(book_xml_path), encoding='utf-8', xml_declaration=True, pretty_print=False)
            print(f"\n  Total TOC links fixed: {fixes_count}")
        except Exception as e:
            print(f"  ! Warning: Could not save Book.XML: {e}")
    else:
        print("  No TOC links needed fixing")

    return fixes_count


def process_zip_package(
    zip_path: Path,
    output_path: Path,
    dtd_path: Path,
    generate_reports: bool = True,
    id_registry_path: Path = None
) -> dict:
    """
    Apply comprehensive DTD fixes to all chapter files in a ZIP package.

    Args:
        zip_path: Input ZIP package
        output_path: Output ZIP package
        dtd_path: Path to DTD file
        generate_reports: Generate before/after validation reports
        id_registry_path: Path to id_registry.json file from ID Authority (optional)
                         If provided, used to identify dropped IDs that should not be recreated

    Returns:
        Dictionary with statistics and validation results
    """
    stats = {
        'files_processed': 0,
        'files_fixed': 0,
        'total_fixes': 0,
        'errors_before': 0,
        'errors_after': 0,
        'improvement': 0
    }

    # Run pre-fix validation if available
    if VALIDATION_AVAILABLE and generate_reports:
        print("\n=== PRE-FIX VALIDATION ===")
        validator = EntityTrackingValidator(dtd_path)
        pre_report = validator.validate_zip_package(zip_path, output_report_path=None)
        stats['errors_before'] = pre_report.get_error_count()
        print(f"Found {stats['errors_before']} validation errors before fixes")

        # Show error breakdown by type
        error_types = {}
        for error in pre_report.errors:
            error_type = error.error_type
            error_types[error_type] = error_types.get(error_type, 0) + 1

        print("\nError types:")
        for error_type, count in sorted(error_types.items(), key=lambda x: -x[1]):
            print(f"  {error_type}: {count}")

    # Initialize fixer
    fixer = ComprehensiveDTDFixer(dtd_path)

    with tempfile.TemporaryDirectory() as tmpdir:
        tmp_path = Path(tmpdir)
        extract_dir = tmp_path / "extracted"
        extract_dir.mkdir()

        # Extract ZIP
        print(f"\n=== APPLYING COMPREHENSIVE FIXES ===")
        print(f"Extracting {zip_path.name}...")
        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(extract_dir)

        # Find all chapter-type XML files (ch, ap, pr, bi, dd, gl, in, pt, sp prefixes).
        # Previously only ch*.xml and ap*.xml were processed, which left
        # preface (pr), bibliography (bi), dedication (dd), glossary (gl),
        # index (in), part (pt), and subpart (sp) files unfixed — causing
        # IDREF validation errors in those files.
        chapter_files: List[Path] = []
        for prefix in ("ch", "ap", "pr", "bi", "dd", "gl", "in", "pt", "sp"):
            chapter_files.extend(extract_dir.rglob(f"{prefix}*.xml"))
        # Deduplicate (rglob may overlap) and sort
        chapter_files = sorted(set(chapter_files))
        chapter_map = {cf.stem: cf for cf in chapter_files}  # Map ID to file path

        # Also find Book.XML (contains TOC with many references)
        book_xml_files = list(extract_dir.rglob("Book.XML"))
        if book_xml_files:
            book_xml = book_xml_files[0]
        else:
            book_xml = None

        # Collect all XML files (case-insensitive) for linkend updates
        all_xml_files = sorted({
            path for path in extract_dir.rglob("*")
            if path.is_file() and path.suffix.lower() == ".xml"
        })

        print(f"Found {len(chapter_files)} chapter/component files to fix\n")

        # PASS 1: Collect all valid IDs from all chapters for cross-reference validation
        print("Pass 1: Collecting all valid IDs for cross-reference validation...")
        valid_ids = set()
        for chapter_file in sorted(chapter_files):
            try:
                parser = etree.XMLParser(remove_blank_text=False, resolve_entities=False, recover=True)
                tree = etree.parse(str(chapter_file), parser)
                root = tree.getroot()
                for elem in root.iter():
                    elem_id = elem.get('id')
                    if elem_id:
                        valid_ids.add(elem_id)
            except Exception as e:
                print(f"  ! Warning: Could not parse {chapter_file.name} for ID collection: {e}")

        print(f"  Found {len(valid_ids)} valid IDs across all chapters\n")

        # Load dropped IDs from id_registry.json if available (from id_authority)
        # These are IDs that were intentionally not preserved (anchor-only elements, pagebreaks, etc.)
        dropped_ids: Set[str] = set()

        # First try the explicitly provided path, then fall back to searching in extract_dir
        registry_file_to_use = None
        if id_registry_path and Path(id_registry_path).exists():
            registry_file_to_use = Path(id_registry_path)
        else:
            # Fall back to searching in ZIP contents (legacy behavior)
            id_registry_files = list(extract_dir.rglob("id_registry.json"))
            if id_registry_files:
                registry_file_to_use = id_registry_files[0]

        if registry_file_to_use:
            try:
                import json
                with open(registry_file_to_use, 'r', encoding='utf-8') as f:
                    registry_data = json.load(f)
                    # Extract dropped IDs from id_records where state == 'dropped'
                    if 'id_records' in registry_data:
                        for key, record in registry_data['id_records'].items():
                            if record.get('state') == 'dropped':
                                # Key format: "chapter_id:source_id" or use source_id from record
                                source_id = record.get('source_id', '')
                                if source_id:
                                    dropped_ids.add(source_id)
                print(f"  Loaded {len(dropped_ids)} dropped IDs from {registry_file_to_use.name}")
            except Exception as e:
                print(f"  ! Warning: Could not load id_registry.json: {e}")
        else:
            print(f"  Note: id_registry.json not available - dropped ID filtering disabled")

        # PASS 2: Collect broken references and create missing IDs in target chapters
        print("Pass 2: Analyzing broken references and creating missing IDs...")
        missing_ids_created = 0
        broken_refs_by_chapter = {}  # chapter_id -> list of missing IDs

        # Check all files including Book.XML (which has TOC)
        files_to_check = list(chapter_files)
        if book_xml:
            files_to_check.append(book_xml)

        for chapter_file in sorted(files_to_check):
            try:
                parser = etree.XMLParser(remove_blank_text=False, resolve_entities=False, recover=True)
                tree = etree.parse(str(chapter_file), parser)
                root = tree.getroot()

                # Find broken references in this chapter
                for elem in root.iter():
                    # Check linkend attributes
                    linkend = elem.get('linkend')
                    if linkend and linkend not in valid_ids:
                        # Parse the target chapter/component from the ID
                        chapter_match = re.search(r'([a-z]{2}\d{4})', linkend)
                        if chapter_match:
                            target_chapter = chapter_match.group(1)
                            if target_chapter not in broken_refs_by_chapter:
                                broken_refs_by_chapter[target_chapter] = []
                            broken_refs_by_chapter[target_chapter].append(linkend)

                    # ALSO check url attributes (for TOC links like "ch0010.xml#head-2-5")
                    url = elem.get('url')
                    if url and '#' in url:
                        # Extract the fragment ID from URL
                        parts = url.split('#')
                        if len(parts) == 2:
                            fragment_id = parts[1]
                            # Check if this ID exists
                            if fragment_id and fragment_id not in valid_ids:
                                # Parse target chapter/component from the URL filename
                                chapter_match = re.search(r'([a-z]{2}\d{4})', url)
                                if chapter_match:
                                    target_chapter = chapter_match.group(1)
                                    if target_chapter not in broken_refs_by_chapter:
                                        broken_refs_by_chapter[target_chapter] = []
                                    broken_refs_by_chapter[target_chapter].append(fragment_id)
            except Exception as e:
                print(f"  ! Warning: Could not analyze {chapter_file.name}: {e}")

        # Deduplicate the broken references
        total_broken_refs = 0
        for target_chapter in broken_refs_by_chapter:
            unique_ids = list(set(broken_refs_by_chapter[target_chapter]))
            total_broken_refs += len(unique_ids)
            broken_refs_by_chapter[target_chapter] = unique_ids

        print(f"  Found {total_broken_refs} unique broken references to create")

        # Create missing IDs in target chapters
        all_stale_ref_mappings = {}  # Collect all stale ID -> correct ID mappings
        for target_chapter, missing_ids in broken_refs_by_chapter.items():
            if target_chapter not in chapter_map:
                print(f"  ! Warning: Target chapter '{target_chapter}' not found in chapter_map")
                continue

            target_file = chapter_map[target_chapter]
            try:
                parser = etree.XMLParser(remove_blank_text=False, resolve_entities=False, recover=True)
                tree = etree.parse(str(target_file), parser)
                root = tree.getroot()

                ids_created_in_chapter, stale_ref_mappings = fixer._create_missing_ids_in_chapter(root, target_file.name, missing_ids, dropped_ids)
                all_stale_ref_mappings.update(stale_ref_mappings)
                if ids_created_in_chapter > 0:
                    missing_ids_created += ids_created_in_chapter
                    # Write back the modified chapter
                    tree.write(str(target_file), encoding='utf-8', xml_declaration=True, pretty_print=True)
                    # Add created IDs to valid_ids set
                    for elem in root.iter():
                        elem_id = elem.get('id')
                        if elem_id:
                            valid_ids.add(elem_id)
            except Exception as e:
                print(f"  ! Warning: Could not add IDs to {target_file.name}: {e}")

        if missing_ids_created > 0:
            print(f"  Created {missing_ids_created} missing IDs to preserve working links\n")

        # PASS 2.5: Update stale linkends to point to correct IDs
        if all_stale_ref_mappings:
            print(f"Pass 2.5: Updating {len(all_stale_ref_mappings)} stale linkends to correct IDs...")
            stale_updates = 0
            files_to_update = list(all_xml_files)
            for target_file in sorted(files_to_update):
                try:
                    parser = etree.XMLParser(remove_blank_text=False)
                    tree = etree.parse(str(target_file), parser)
                    root = tree.getroot()
                    file_updated = False
                    for elem in root.iter():
                        # Update linkend attributes
                        linkend = elem.get('linkend')
                        if linkend and linkend in all_stale_ref_mappings:
                            correct_id = all_stale_ref_mappings[linkend]
                            elem.set('linkend', correct_id)
                            stale_updates += 1
                            file_updated = True
                        # Also update url attributes with fragment IDs
                        url = elem.get('url')
                        if url and '#' in url:
                            for stale_id, correct_id in all_stale_ref_mappings.items():
                                if f'#{stale_id}' in url:
                                    elem.set('url', url.replace(f'#{stale_id}', f'#{correct_id}'))
                                    stale_updates += 1
                                    file_updated = True
                                    break
                    if file_updated:
                        tree.write(str(target_file), encoding='utf-8', xml_declaration=True, pretty_print=True)
                except Exception as e:
                    print(f"  ! Warning: Could not update stale linkends in {target_file.name}: {e}")
            if stale_updates > 0:
                print(f"  Updated {stale_updates} stale linkends to point to correct IDs\n")

        # PASS 3: Apply fixes to each chapter
        print("Pass 3: Applying fixes to each chapter...")
        for chapter_file in sorted(chapter_files):
            stats['files_processed'] += 1

            num_fixes, fix_descriptions = fixer.fix_chapter_file(chapter_file, chapter_file.name, valid_ids)

            if num_fixes > 0:
                stats['files_fixed'] += 1
                stats['total_fixes'] += num_fixes
                print(f"  [OK] {chapter_file.name}: Applied {num_fixes} fix(es)")

        # PASS 3.5: Update link references across all files after ID renames
        if fixer.id_renames:
            print("\nPass 3.5: Updating link references for renamed IDs...")
            files_to_update = list(all_xml_files)
            for target_file in sorted(files_to_update):
                try:
                    parser = etree.XMLParser(remove_blank_text=False)
                    tree = etree.parse(str(target_file), parser)
                    root = tree.getroot()
                    updated = fixer._apply_id_renames(root, target_file.name)
                    if updated > 0:
                        tree.write(str(target_file), encoding='utf-8', xml_declaration=True, pretty_print=True)
                except Exception as e:
                    print(f"  ! Warning: Could not update references in {target_file.name}: {e}")

        # PASS 4: Fix TOC links to point to actual section IDs
        if book_xml and book_xml.exists():
            # Chapters are in the same directory as Book.XML
            chapters_dir = book_xml.parent
            toc_fixes = fix_toc_links(book_xml, chapters_dir)
            stats['total_fixes'] += toc_fixes

        # Recreate ZIP
        print(f"\nCreating fixed ZIP: {output_path.name}...")
        with zipfile.ZipFile(output_path, 'w', zipfile.ZIP_DEFLATED) as zf:
            for file_path in extract_dir.rglob('*'):
                if file_path.is_file():
                    arcname = file_path.relative_to(extract_dir)
                    zf.write(file_path, arcname)

    # Run post-fix validation
    if VALIDATION_AVAILABLE and generate_reports:
        print("\n=== POST-FIX VALIDATION ===")
        validator = EntityTrackingValidator(dtd_path)
        post_report = validator.validate_zip_package(output_path, output_report_path=None)
        stats['errors_after'] = post_report.get_error_count()
        stats['improvement'] = stats['errors_before'] - stats['errors_after']
        stats['improvement_pct'] = (stats['improvement'] / stats['errors_before'] * 100) if stats['errors_before'] > 0 else 0

        print(f"Found {stats['errors_after']} validation errors after fixes")

        if stats['errors_after'] > 0:
            # Show remaining error breakdown by type
            error_types = {}
            for error in post_report.errors:
                error_type = error.error_type
                error_types[error_type] = error_types.get(error_type, 0) + 1

            print("\nRemaining error types:")
            for error_type, count in sorted(error_types.items(), key=lambda x: -x[1])[:10]:
                print(f"  {error_type}: {count}")

            # Show sample errors
            print("\nSample remaining errors (first 5):")
            for i, error in enumerate(post_report.errors[:5]):
                print(f"  {i+1}. {error.xml_file}:{error.line_number} - {error.error_description[:80]}")

        # Store reports for later use
        stats['pre_report'] = pre_report
        stats['post_report'] = post_report

    # Collect verification items
    stats['verification_items'] = fixer.verification_items

    return stats


def main():
    import argparse

    parser = argparse.ArgumentParser(
        description="Comprehensive DTD fixer for RittDoc packages"
    )
    parser.add_argument("input", help="Input ZIP package")
    parser.add_argument("-o", "--output", help="Output ZIP path (default: add _comprehensive_fixed suffix)")
    parser.add_argument("--dtd", default="RITTDOCdtd/v1.1/RittDocBook.dtd", help="Path to DTD file")
    parser.add_argument("--no-reports", action="store_true", help="Skip validation reports")

    args = parser.parse_args()

    input_path = Path(args.input)
    if not input_path.exists():
        print(f"Error: Input file not found: {input_path}")
        sys.exit(1)

    dtd_path = Path(args.dtd)
    if not dtd_path.exists():
        print(f"Error: DTD file not found: {dtd_path}")
        sys.exit(1)

    # Determine output path
    if args.output:
        output_path = Path(args.output)
    else:
        output_path = input_path.parent / f"{input_path.stem}_comprehensive_fixed{input_path.suffix}"

    # Process ZIP
    print("=" * 70)
    print("COMPREHENSIVE DTD FIXER FOR RITTDOC")
    print("=" * 70)

    stats = process_zip_package(input_path, output_path, dtd_path, generate_reports=not args.no_reports)

    # Print summary
    print("\n" + "=" * 70)
    print("FIX SUMMARY")
    print("=" * 70)
    print(f"Files processed:        {stats['files_processed']}")
    print(f"Files with fixes:       {stats['files_fixed']}")
    print(f"Total fixes applied:    {stats['total_fixes']}")

    if 'errors_before' in stats:
        print(f"\nValidation Results:")
        print(f"  Errors before:        {stats['errors_before']}")
        print(f"  Errors after:         {stats['errors_after']}")
        print(f"  Errors fixed:         {stats['improvement']}")
        if 'improvement_pct' in stats:
            print(f"  Improvement:          {stats['improvement_pct']:.1f}%")

    print(f"\nOutput: {output_path}")
    print("=" * 70)

    if stats.get('errors_after', 0) > 0:
        print(f"\n[WARN] Warning: {stats['errors_after']} validation errors remain")
        print("These may require manual review or additional fixes")
        sys.exit(1)
    else:
        print("\n[OK] Success: All DTD validation errors fixed!")
        sys.exit(0)


if __name__ == "__main__":
    main()
