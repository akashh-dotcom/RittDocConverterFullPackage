#!/usr/bin/env python3
"""
DocBook Builder - Validated XML Element Creation with DTD Content Model Awareness.

This module provides:
1. DTD Content Model Cache - Parsed DTD with all element rules
2. Element Factory - Creates elements with validation
3. Content Loss Prevention - Never discards content, finds alternatives
4. XHTML to DocBook Mapping - Transformation rules with fallbacks

Architecture:
    DTDContentModel (parses DTD, provides content model lookups)
    └── DocBookBuilder (validated element factory)
        └── XHTMLTransformer (XHTML→DocBook with content preservation)

Usage:
    from docbook_builder import get_builder

    builder = get_builder(dtd_path)

    # Create elements with validation
    chapter = builder.create_element('chapter', id='ch0001')
    title = builder.add_child(chapter, 'title', text='Introduction')
    para = builder.add_child(chapter, 'para', text='Content here...')

    # Auto-restructuring when needed
    # If figure can't go in para, it extracts and places correctly
    figure = builder.add_child(para, 'figure')  # Auto-extracts from para

See docs/DOCBOOK_BUILDER.md for complete documentation.
"""

import logging
import re
import threading
from dataclasses import dataclass, field
from enum import Enum, auto
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple, Any, Callable, Union
from lxml import etree

logger = logging.getLogger(__name__)


# ============================================================================
# R2 XSL REQUIREMENTS
# ============================================================================
# These constants define downstream XSL processing requirements from the
# R2 Library application. Based on analysis of App/Xsl/*.ritt.xsl files.

# Elements that MUST have IDs - XSL outputs these IDs in HTML
# See: RittBook.xsl, admon.ritt.xsl, footnote.ritt.xsl
XSL_ELEMENTS_REQUIRE_ID = frozenset({
    # Sections - IDs output in heading elements (h2-h6)
    'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'sect6',
    'sect7', 'sect8', 'sect9', 'sect10',
    # Formal objects - IDs output in wrapper divs
    'table', 'figure', 'equation', 'informalfigure', 'informaltable',
    # Admonitions - IDs output in featurebox divs
    'note', 'sidebar', 'important', 'warning', 'caution', 'tip',
    # Footnotes - IDs output in anchor elements
    'footnote',
    # Book components
    'chapter', 'appendix', 'preface', 'part', 'subpart',
    # Q&A - IDs used for cross-references
    'qandaset', 'qandaentry',
    # Bibliography
    'bibliomixed', 'biblioentry',
    # Glossary
    'glossentry',
})

# Elements where IDs are IGNORED by XSL (commented out anchor template)
# See: RittBook.xsl line 441-444 (commented anchor call)
XSL_ELEMENTS_ID_IGNORED = frozenset({
    'anchor',        # Anchor template is commented out in html.xsl
    'para',          # Para IDs not output
    'simpara',       # Same as para
    'title',         # Title IDs not output separately
    'bridgehead',    # Bridgehead IDs not output
    'beginpage',     # Pagebreak markers ignored
})

# Link prefix mappings for cross-references
# See: link.ritt.xsl for prefix extraction logic
XSL_LINKEND_PREFIXES = {
    'fg': 'figure',      # fg0001 -> figure
    'eq': 'equation',    # eq0001 -> equation
    'ta': 'table',       # ta0001 -> table
    'gl': 'glossary',    # gl0001 -> glossentry
    'bib': 'bibliography',  # bib0001 -> bibliomixed
    'qa': 'qandaset',    # qa0001 -> qandaentry
    'pr': 'preface',     # pr0001 -> preface
    'vd': 'video',       # vd0001 -> video section
    'ad': 'appendix',    # ad0001 -> appendix
    'ch': 'chapter',     # ch0001 -> chapter
    's': 'section',      # s0001 -> sect1/sect2/etc
    'fn': 'footnote',    # fn0001 -> footnote
}

# Ritt-specific custom elements (from rittcustomtags.mod)
RITT_CUSTOM_ELEMENTS = frozenset({
    'risindex', 'risterm', 'ristopic', 'ristype', 'risrule', 'risposid',
    'risinfo', 'chapterid', 'chaptertitle', 'booktitle', 'chapternumber',
    'primaryauthor', 'riscurrent', 'risnext', 'risprev', 'risempty',
})

# Elements that can contain risinfo metadata
RITT_RISINFO_CONTAINERS = frozenset({
    'sect1info', 'chapterinfo', 'bookinfo',
})

# Ritt-specific content models (from rittcustomtags.mod DTD)
# These are validated separately since they extend the base DocBook DTD
RITT_CONTENT_MODELS = {
    'risindex': {
        'required_children': frozenset({'risterm', 'ristopic', 'ristype', 'risrule', 'risposid'}),
        'valid_children': frozenset({'risterm', 'ristopic', 'ristype', 'risrule', 'risposid'}),
        'allows_pcdata': False,
        'sequence': ['risterm', 'ristopic', 'ristype', 'risrule', 'risposid'],
    },
    'risinfo': {
        'required_children': frozenset(),  # All optional
        'valid_children': frozenset({
            'author', 'authorgroup', 'booktitle', 'chapternumber', 'editor',
            'isbn', 'mediaobject', 'primaryauthor', 'pubdate', 'publisher',
            'riscurrent', 'risnext', 'risprev', 'chaptertitle', 'chapterid'
        }),
        'allows_pcdata': False,
    },
    'primaryauthor': {
        'required_children': frozenset(),  # personname or person.ident.mix
        'valid_children': frozenset({'personname', 'personblurb', 'email', 'address',
                                     'firstname', 'surname', 'othername', 'honorific',
                                     'lineage', 'affiliation'}),
        'allows_pcdata': False,
    },
    # PCDATA-only elements
    'risterm': {'allows_pcdata': True, 'valid_children': frozenset()},
    'ristopic': {'allows_pcdata': True, 'valid_children': frozenset()},
    'ristype': {'allows_pcdata': True, 'valid_children': frozenset()},
    'risrule': {'allows_pcdata': True, 'valid_children': frozenset()},
    'risposid': {'allows_pcdata': True, 'valid_children': frozenset()},
    'chapterid': {'allows_pcdata': True, 'valid_children': frozenset()},
    'chaptertitle': {'allows_pcdata': True, 'valid_children': frozenset()},
    'booktitle': {'allows_pcdata': True, 'valid_children': frozenset()},
    'chapternumber': {'allows_pcdata': True, 'valid_children': frozenset()},
    'riscurrent': {'allows_pcdata': True, 'valid_children': frozenset()},
    'risnext': {'allows_pcdata': True, 'valid_children': frozenset()},
    'risprev': {'allows_pcdata': True, 'valid_children': frozenset()},
}


# ============================================================================
# XSL REQUIREMENT HELPER FUNCTIONS
# ============================================================================

def element_requires_id(tag: str) -> bool:
    """
    Check if element MUST have an ID for downstream XSL processing.

    Elements in XSL_ELEMENTS_REQUIRE_ID have their IDs output in HTML
    for anchor targets, section navigation, and cross-referencing.

    Args:
        tag: Element tag name

    Returns:
        True if element requires an ID for XSL output
    """
    return tag in XSL_ELEMENTS_REQUIRE_ID


def is_id_ignored_by_xsl(tag: str) -> bool:
    """
    Check if element's ID is ignored by XSL transformations.

    Elements in XSL_ELEMENTS_ID_IGNORED have their IDs stripped or
    commented out in the XSL templates (e.g., anchor template is disabled).

    Args:
        tag: Element tag name

    Returns:
        True if element's ID will be ignored by XSL
    """
    return tag in XSL_ELEMENTS_ID_IGNORED


def get_linkend_element_type(linkend: str) -> Optional[str]:
    """
    Parse linkend ID to determine target element type.

    XSL uses ID prefixes to determine link rendering:
    - fg0001 -> figure
    - eq0001 -> equation
    - ta0001 -> table
    etc.

    Args:
        linkend: The linkend attribute value (e.g., "fg0001")

    Returns:
        Element type string (e.g., "figure") or None if no prefix match
    """
    if not linkend:
        return None

    # Check each prefix from longest to shortest
    for prefix, elem_type in sorted(XSL_LINKEND_PREFIXES.items(),
                                    key=lambda x: -len(x[0])):
        if linkend.startswith(prefix):
            return elem_type
    return None


def is_ritt_custom_element(tag: str) -> bool:
    """
    Check if element is a Ritt-specific custom extension.

    Ritt elements (risindex, risinfo, etc.) are defined in rittcustomtags.mod
    and extend the base DocBook DTD for R2 Library-specific metadata.

    Args:
        tag: Element tag name

    Returns:
        True if element is a Ritt custom element
    """
    return tag in RITT_CUSTOM_ELEMENTS


def get_ritt_content_model(tag: str) -> Optional[Dict]:
    """
    Get content model for a Ritt-specific element.

    Args:
        tag: Element tag name

    Returns:
        Content model dict or None if not a Ritt element
    """
    return RITT_CONTENT_MODELS.get(tag)


def validate_ritt_element(tag: str, children: List[str]) -> Tuple[bool, Optional[str]]:
    """
    Validate a Ritt-specific element's children.

    Args:
        tag: Element tag name
        children: List of child element tag names

    Returns:
        Tuple of (is_valid, error_message)
    """
    model = RITT_CONTENT_MODELS.get(tag)
    if not model:
        return True, None  # Not a Ritt element, skip validation

    valid_children = model.get('valid_children', frozenset())
    required_children = model.get('required_children', frozenset())
    sequence = model.get('sequence')

    # Check all children are valid
    child_set = set(children)
    invalid = child_set - valid_children
    if invalid and valid_children:  # Only check if model specifies valid children
        return False, f"Invalid children in <{tag}>: {', '.join(sorted(invalid))}"

    # Check required children present
    missing = required_children - child_set
    if missing:
        return False, f"Missing required children in <{tag}>: {', '.join(sorted(missing))}"

    # Check sequence order if specified
    if sequence:
        seq_positions = {name: i for i, name in enumerate(sequence)}
        last_pos = -1
        for child in children:
            if child in seq_positions:
                pos = seq_positions[child]
                if pos < last_pos:
                    return False, f"Wrong order in <{tag}>: <{child}> should come earlier"
                last_pos = pos

    return True, None


def get_id_prefix_for_element(tag: str) -> Optional[str]:
    """
    Get the recommended ID prefix for an element type.

    Reverse mapping from element types to XSL link prefixes.

    Args:
        tag: Element tag name

    Returns:
        ID prefix string (e.g., "fg" for figure) or None
    """
    # Build reverse mapping
    elem_to_prefix = {}
    for prefix, elem_type in XSL_LINKEND_PREFIXES.items():
        # Map specific DocBook elements to prefixes
        if elem_type == 'figure':
            elem_to_prefix['figure'] = prefix
            elem_to_prefix['informalfigure'] = prefix
        elif elem_type == 'equation':
            elem_to_prefix['equation'] = prefix
            elem_to_prefix['informalequation'] = prefix
        elif elem_type == 'table':
            elem_to_prefix['table'] = prefix
            elem_to_prefix['informaltable'] = prefix
        elif elem_type == 'section':
            for i in range(1, 11):
                elem_to_prefix[f'sect{i}'] = prefix
        elif elem_type == 'glossary':
            elem_to_prefix['glossentry'] = prefix
            elem_to_prefix['glossterm'] = prefix
        elif elem_type == 'bibliography':
            elem_to_prefix['bibliomixed'] = prefix
            elem_to_prefix['biblioentry'] = prefix
        elif elem_type == 'qandaset':
            elem_to_prefix['qandaset'] = prefix
            elem_to_prefix['qandaentry'] = prefix
        else:
            elem_to_prefix[elem_type] = prefix

    return elem_to_prefix.get(tag)


# ============================================================================
# CONTENT MODEL TYPES
# ============================================================================

class ContentType(Enum):
    """Types of content an element can contain."""
    EMPTY = auto()          # Element must be empty (e.g., anchor, beginpage)
    PCDATA = auto()         # Text content only
    ELEMENT = auto()        # Child elements only
    MIXED = auto()          # Text and elements mixed
    ANY = auto()            # Any content allowed


class ElementCategory(Enum):
    """Semantic categories for elements."""
    BLOCK = auto()          # Block-level (para, figure, table, etc.)
    INLINE = auto()         # Inline (emphasis, ulink, subscript, etc.)
    STRUCTURAL = auto()     # Structural (chapter, sect1, book, etc.)
    METADATA = auto()       # Metadata (bookinfo, title, author, etc.)
    LIST = auto()           # List elements (itemizedlist, orderedlist, etc.)
    TABLE = auto()          # Table elements (table, tgroup, row, etc.)
    MEDIA = auto()          # Media (mediaobject, imageobject, etc.)
    ADMONITION = auto()     # Admonitions (note, warning, tip, etc.)
    WRAPPER = auto()        # Wrapper elements (blockquote, sidebar, etc.)


class ContentOperator(Enum):
    """Occurrence operators in DTD content models."""
    ONCE = auto()       # Exactly once (no suffix)
    OPT = auto()        # Zero or one (?)
    MULT = auto()       # Zero or more (*)
    PLUS = auto()       # One or more (+)


class SequenceType(Enum):
    """Type of content group."""
    SEQ = auto()        # Sequence - elements must appear in order (a, b, c)
    CHOICE = auto()     # Choice - one of the elements (a | b | c)


@dataclass
class ContentGroup:
    """A group of elements in a DTD content model."""
    elements: List[Union[str, 'ContentGroup']]
    group_type: SequenceType
    occurrence: ContentOperator = ContentOperator.ONCE

    def is_required(self) -> bool:
        """Check if this group is required (must appear at least once)."""
        return self.occurrence in (ContentOperator.ONCE, ContentOperator.PLUS)

    def allows_multiple(self) -> bool:
        """Check if this group can appear multiple times."""
        return self.occurrence in (ContentOperator.MULT, ContentOperator.PLUS)


@dataclass
class ElementDef:
    """Definition of a DocBook element from DTD."""
    name: str
    content_type: ContentType
    category: ElementCategory
    valid_children: Set[str] = field(default_factory=set)
    required_children: Set[str] = field(default_factory=set)
    optional_children: Set[str] = field(default_factory=set)
    valid_parents: Set[str] = field(default_factory=set)
    required_attrs: Set[str] = field(default_factory=set)
    optional_attrs: Set[str] = field(default_factory=set)
    default_attrs: Dict[str, str] = field(default_factory=dict)
    allows_pcdata: bool = False
    is_empty: bool = False
    sequence_children: List[str] = field(default_factory=list)  # For ordered content
    choice_groups: List[Set[str]] = field(default_factory=list)  # For choice content
    content_structure: Optional[ContentGroup] = None  # Full parsed structure


@dataclass
class ContentModelViolation:
    """Details of a content model violation."""
    parent_element: str
    child_element: str
    violation_type: str  # 'invalid_child', 'missing_required', 'wrong_order', etc.
    message: str
    suggested_fix: Optional[str] = None
    alternative_parent: Optional[str] = None


# ============================================================================
# DTD CONTENT MODEL - Parsed from DTD
# ============================================================================

class DTDContentModel:
    """
    Parses and caches DTD content model for fast lookup during XML building.

    This class extracts all element definitions, content models, and rules
    from the DocBook DTD to enable validation during element creation.
    """

    def __init__(self, dtd_path: Path):
        self.dtd_path = Path(dtd_path)
        self.dtd: Optional[etree.DTD] = None
        self.elements: Dict[str, ElementDef] = {}
        self._parent_cache: Dict[str, Set[str]] = {}  # child -> valid parents
        self._loaded = False

        # Parse DTD
        self._load_dtd()
        self._build_content_model()

    def _load_dtd(self) -> None:
        """Load and parse the DTD file."""
        try:
            self.dtd = etree.DTD(str(self.dtd_path))
            self._loaded = True
            logger.info(f"Loaded DTD from {self.dtd_path}")
        except Exception as e:
            logger.error(f"Failed to load DTD: {e}")
            raise

    def _build_content_model(self) -> None:
        """Build content model from parsed DTD."""
        if not self.dtd:
            return

        # First pass: Create all element definitions
        for elem in self.dtd.elements():
            self._parse_element(elem)

        # Second pass: Build parent-child relationships
        self._build_parent_cache()

        # Third pass: Apply known categorizations
        self._categorize_elements()

        logger.info(f"Built content model with {len(self.elements)} elements")

    def _parse_element(self, dtd_elem) -> None:
        """Parse a single element from DTD."""
        name = dtd_elem.name
        if not name:
            return

        # Determine content type
        content = dtd_elem.content
        content_type = self._determine_content_type(content)

        # Get valid children from content model
        valid_children = self._extract_children(content)

        # Get attributes
        required_attrs = set()
        optional_attrs = set()
        default_attrs = {}

        for attr in dtd_elem.attributes():
            attr_name = attr.name
            # lxml uses attr.default for the declaration type ('required', 'implied', 'fixed', etc.)
            # and attr.default_value for actual default value if specified
            attr_default = getattr(attr, 'default', None)
            attr_default_value = getattr(attr, 'default_value', None)

            if attr_default == 'required':
                required_attrs.add(attr_name)
            else:
                optional_attrs.add(attr_name)
                # Store actual default value if present
                if attr_default_value:
                    default_attrs[attr_name] = attr_default_value

        # Create element definition
        elem_def = ElementDef(
            name=name,
            content_type=content_type,
            category=ElementCategory.BLOCK,  # Will be refined in categorization
            valid_children=valid_children,
            required_attrs=required_attrs,
            optional_attrs=optional_attrs,
            default_attrs=default_attrs,
            allows_pcdata=(content_type in (ContentType.PCDATA, ContentType.MIXED)),
            is_empty=(content_type == ContentType.EMPTY)
        )

        # Parse content structure for required children and sequence detection
        if content is not None:
            self._determine_required_children(content, elem_def)
            elem_def.content_structure = self._parse_content_structure(content)

        self.elements[name] = elem_def

    def _determine_content_type(self, content) -> ContentType:
        """Determine content type from DTD content model."""
        # Empty elements have content=None in lxml DTD parser
        if content is None:
            return ContentType.EMPTY

        # Check the type attribute of the content declaration
        content_type = getattr(content, 'type', None)

        if content_type == 'empty':
            return ContentType.EMPTY
        if content_type == 'any':
            return ContentType.ANY
        if content_type == 'pcdata':
            return ContentType.PCDATA
        if content_type in ('or', 'seq'):
            # Check if PCDATA is in the mix (making it MIXED)
            if self._has_pcdata(content):
                return ContentType.MIXED
            return ContentType.ELEMENT

        return ContentType.ELEMENT

    def _has_pcdata(self, content_decl) -> bool:
        """Check if content declaration includes PCDATA."""
        if content_decl is None:
            return False

        content_type = getattr(content_decl, 'type', None)
        if content_type == 'pcdata':
            return True

        # Check children
        left = getattr(content_decl, 'left', None)
        right = getattr(content_decl, 'right', None)

        return self._has_pcdata(left) or self._has_pcdata(right)

    def _extract_children(self, content) -> Set[str]:
        """Extract child element names from DTD content model tree."""
        children = set()
        if content is None:
            return children

        # Recursively extract element names from the content declaration tree
        self._extract_children_recursive(content, children)
        return children

    def _extract_children_recursive(self, content_decl, children: Set[str]) -> None:
        """Recursively traverse DTD content declaration tree to extract element names."""
        if content_decl is None:
            return

        # Get direct name if present (leaf node)
        name = getattr(content_decl, 'name', None)
        if name:
            children.add(name)

        # Traverse left subtree (for or/seq binary tree structure)
        left = getattr(content_decl, 'left', None)
        if left is not None:
            self._extract_children_recursive(left, children)

        # Traverse right subtree
        right = getattr(content_decl, 'right', None)
        if right is not None:
            self._extract_children_recursive(right, children)

    def _parse_content_structure(self, content) -> Optional[ContentGroup]:
        """
        Parse DTD content model into structured ContentGroup.

        Extracts:
        - Sequence vs choice groups
        - Occurrence operators (?, *, +)
        - Required vs optional elements
        """
        if content is None:
            return None

        return self._parse_content_node(content)

    def _parse_content_node(self, node) -> Optional[ContentGroup]:
        """Recursively parse a content model node."""
        if node is None:
            return None

        # Get node properties
        node_type = getattr(node, 'type', None)
        node_occur = getattr(node, 'occur', None)
        node_name = getattr(node, 'name', None)

        # Map occurrence strings to enum
        occur_map = {
            'once': ContentOperator.ONCE,
            'opt': ContentOperator.OPT,
            'mult': ContentOperator.MULT,
            'plus': ContentOperator.PLUS,
            None: ContentOperator.ONCE
        }
        occurrence = occur_map.get(node_occur, ContentOperator.ONCE)

        # If it's a named element (leaf node)
        if node_name and node_type not in ('or', 'seq'):
            return ContentGroup(
                elements=[node_name],
                group_type=SequenceType.SEQ,
                occurrence=occurrence
            )

        # If it's a group (or, seq)
        if node_type in ('or', 'seq'):
            group_type = SequenceType.CHOICE if node_type == 'or' else SequenceType.SEQ
            elements = []

            # Collect all elements from the binary tree
            self._collect_group_elements(node, elements, node_type)

            return ContentGroup(
                elements=elements,
                group_type=group_type,
                occurrence=occurrence
            )

        # For PCDATA or other types
        return None

    def _collect_group_elements(self, node, elements: List, expected_type: str) -> None:
        """Collect all elements from a sequence or choice group."""
        if node is None:
            return

        node_type = getattr(node, 'type', None)
        node_name = getattr(node, 'name', None)
        node_occur = getattr(node, 'occur', None)

        # Map occurrence
        occur_map = {
            'once': ContentOperator.ONCE,
            'opt': ContentOperator.OPT,
            'mult': ContentOperator.MULT,
            'plus': ContentOperator.PLUS,
            None: ContentOperator.ONCE
        }
        occurrence = occur_map.get(node_occur, ContentOperator.ONCE)

        # If this is a leaf (named element)
        if node_name and node_type not in ('or', 'seq'):
            elements.append((node_name, occurrence))
            return

        # If this is the same group type, flatten it
        if node_type == expected_type:
            left = getattr(node, 'left', None)
            right = getattr(node, 'right', None)
            self._collect_group_elements(left, elements, expected_type)
            self._collect_group_elements(right, elements, expected_type)
        else:
            # Different group type - this is a nested group
            nested = self._parse_content_node(node)
            if nested:
                elements.append(nested)

    def _determine_required_children(self, content, elem_def: ElementDef) -> None:
        """
        Analyze content model to determine required vs optional children.

        Required elements are those that:
        1. Appear directly in a sequence with no occurrence operator (once)
        2. Appear with + operator (one or more)

        Optional elements are those that:
        1. Appear with ? operator (zero or one)
        2. Appear with * operator (zero or more)
        3. Appear in a choice group (only one must be selected)
        """
        if content is None:
            return

        # Track what we find at each level
        self._analyze_requirements_recursive(
            content,
            elem_def.required_children,
            elem_def.optional_children,
            elem_def.sequence_children,
            elem_def.choice_groups,
            is_in_choice=False
        )

    def _analyze_requirements_recursive(
        self,
        node,
        required: Set[str],
        optional: Set[str],
        sequence: List[str],
        choice_groups: List[Set[str]],
        is_in_choice: bool
    ) -> None:
        """Recursively analyze content model for requirements."""
        if node is None:
            return

        node_type = getattr(node, 'type', None)
        node_occur = getattr(node, 'occur', None)
        node_name = getattr(node, 'name', None)

        # Is this element/group required?
        is_required = node_occur in ('once', 'plus', None) and not is_in_choice

        # If this is a named element (leaf)
        if node_name and node_type not in ('or', 'seq'):
            if node_type == 'pcdata':
                return  # Skip PCDATA

            if is_in_choice:
                # Elements in choice are optional (only one needs to be selected)
                optional.add(node_name)
            elif is_required:
                required.add(node_name)
                sequence.append(node_name)
            else:
                optional.add(node_name)
                # Still track in sequence for ordering
                sequence.append(node_name)
            return

        # If this is a sequence group
        if node_type == 'seq':
            # Collect elements maintaining order
            self._traverse_sequence(
                node, required, optional, sequence, choice_groups, is_in_choice
            )
            return

        # If this is a choice group
        if node_type == 'or':
            # Collect the choice group
            current_choice = set()
            self._collect_choice_elements(node, current_choice)

            if current_choice:
                # Remove PCDATA from choice groups (it's not an element)
                current_choice.discard('#PCDATA')
                if current_choice:
                    choice_groups.append(current_choice)
                    # All choice elements are technically optional
                    # (only one needs to be present if the group is required)
                    optional.update(current_choice)

    def _traverse_sequence(
        self,
        node,
        required: Set[str],
        optional: Set[str],
        sequence: List[str],
        choice_groups: List[Set[str]],
        is_in_choice: bool
    ) -> None:
        """Traverse sequence maintaining order."""
        if node is None:
            return

        node_type = getattr(node, 'type', None)
        left = getattr(node, 'left', None)
        right = getattr(node, 'right', None)

        if node_type == 'seq':
            # Process left then right (maintains sequence order)
            self._traverse_sequence(left, required, optional, sequence, choice_groups, is_in_choice)
            self._traverse_sequence(right, required, optional, sequence, choice_groups, is_in_choice)
        else:
            # This is either a leaf or a different group type
            self._analyze_requirements_recursive(
                node, required, optional, sequence, choice_groups, is_in_choice
            )

    def _collect_choice_elements(self, node, elements: Set[str]) -> None:
        """Collect all element names from a choice group."""
        if node is None:
            return

        node_type = getattr(node, 'type', None)
        node_name = getattr(node, 'name', None)

        if node_name:
            elements.add(node_name)
            return

        if node_type == 'or':
            left = getattr(node, 'left', None)
            right = getattr(node, 'right', None)
            self._collect_choice_elements(left, elements)
            self._collect_choice_elements(right, elements)

    def _build_parent_cache(self) -> None:
        """Build reverse lookup: child -> valid parents."""
        self._parent_cache.clear()

        for parent_name, elem_def in self.elements.items():
            for child_name in elem_def.valid_children:
                if child_name not in self._parent_cache:
                    self._parent_cache[child_name] = set()
                self._parent_cache[child_name].add(parent_name)

        # Update element definitions with valid_parents
        for child_name, parents in self._parent_cache.items():
            if child_name in self.elements:
                self.elements[child_name].valid_parents = parents

    def _categorize_elements(self) -> None:
        """Categorize elements by semantic type."""
        # Inline elements (can appear in text flow)
        inline_elements = {
            'emphasis', 'literal', 'ulink', 'link', 'xref', 'anchor',
            'subscript', 'superscript', 'trademark', 'quote', 'phrase',
            'citation', 'citetitle', 'abbrev', 'acronym', 'firstterm',
            'glossterm', 'foreignphrase', 'wordasword', 'productname',
            'inlinemediaobject', 'inlineequation', 'menuchoice', 'guimenu',
            'guibutton', 'guiicon', 'guilabel', 'guimenuitem', 'guisubmenu',
            'keycap', 'keycode', 'keycombo', 'keysym', 'shortcut',
            'mousebutton', 'prompt', 'envar', 'filename', 'command',
            'computeroutput', 'userinput', 'replaceable', 'option',
            'optional', 'parameter', 'function', 'varname', 'returnvalue',
            'type', 'classname', 'methodname', 'interfacename', 'property',
            'constant', 'errorcode', 'errorname', 'errortype', 'email',
            'footnote', 'footnoteref', 'indexterm', 'co', 'coref',
            'remark', 'sgmltag', 'token', 'symbol', 'database', 'application',
            'hardware', 'markup', 'medialabel', 'package', 'structfield',
            'structname', 'systemitem'
        }

        # Block elements (paragraph-level)
        block_elements = {
            'para', 'simpara', 'formalpara', 'blockquote', 'epigraph',
            'attribution', 'programlisting', 'screen', 'literallayout',
            'synopsis', 'cmdsynopsis', 'funcsynopsis', 'address'
        }

        # Structural elements (sections/divisions)
        structural_elements = {
            'book', 'chapter', 'appendix', 'preface', 'article', 'part',
            'partintro', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
            'section', 'simplesect', 'dedication', 'colophon', 'glossary',
            'glossdiv', 'bibliography', 'bibliodiv', 'index', 'indexdiv',
            'setindex', 'toc', 'lot', 'reference', 'refentry'
        }

        # Metadata elements
        metadata_elements = {
            'bookinfo', 'chapterinfo', 'appendixinfo', 'prefaceinfo',
            'articleinfo', 'partinfo', 'sectioninfo', 'sect1info',
            'sect2info', 'sect3info', 'sect4info', 'sect5info',
            'title', 'subtitle', 'titleabbrev', 'author', 'authorgroup',
            'editor', 'collab', 'corpauthor', 'publisher', 'publishername',
            'copyright', 'year', 'holder', 'isbn', 'issn', 'pubdate',
            'date', 'releaseinfo', 'revhistory', 'revision', 'abstract',
            'legalnotice', 'subjectset', 'keywordset', 'keyword'
        }

        # List elements
        list_elements = {
            'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
            'listitem', 'varlistentry', 'term', 'member', 'segmentedlist',
            'seglistitem', 'seg', 'segtitle', 'calloutlist', 'callout',
            'glosslist', 'glossentry', 'glossdef', 'glosssee', 'glossseealso',
            'qandaset', 'qandadiv', 'qandaentry', 'question', 'answer'
        }

        # Table elements
        table_elements = {
            'table', 'informaltable', 'tgroup', 'colspec', 'spanspec',
            'thead', 'tfoot', 'tbody', 'row', 'entry', 'entrytbl'
        }

        # Media elements
        media_elements = {
            'figure', 'informalfigure', 'mediaobject', 'inlinemediaobject',
            'imageobject', 'imagedata', 'videoobject', 'videodata',
            'audioobject', 'audiodata', 'textobject', 'caption',
            'screenshot', 'screeninfo', 'graphic', 'inlinegraphic'
        }

        # Admonition elements
        admonition_elements = {
            'note', 'tip', 'warning', 'caution', 'important'
        }

        # Wrapper elements
        wrapper_elements = {
            'sidebar', 'example', 'informalexample', 'equation',
            'informalequation', 'procedure', 'step', 'substeps',
            'stepalternatives', 'task', 'tasksummary', 'taskprerequisites',
            'taskrelated', 'biblioentry', 'bibliomixed', 'bibliomisc',
            'bibliomset', 'biblioset'
        }

        # Apply categorizations
        for name, elem_def in self.elements.items():
            if name in inline_elements:
                elem_def.category = ElementCategory.INLINE
            elif name in block_elements:
                elem_def.category = ElementCategory.BLOCK
            elif name in structural_elements:
                elem_def.category = ElementCategory.STRUCTURAL
            elif name in metadata_elements:
                elem_def.category = ElementCategory.METADATA
            elif name in list_elements:
                elem_def.category = ElementCategory.LIST
            elif name in table_elements:
                elem_def.category = ElementCategory.TABLE
            elif name in media_elements:
                elem_def.category = ElementCategory.MEDIA
            elif name in admonition_elements:
                elem_def.category = ElementCategory.ADMONITION
            elif name in wrapper_elements:
                elem_def.category = ElementCategory.WRAPPER

    # -------------------------------------------------------------------------
    # Public API - Content Model Queries
    # -------------------------------------------------------------------------

    def is_valid_child(self, parent: str, child: str) -> bool:
        """Check if child element is valid inside parent."""
        if parent not in self.elements:
            return True  # Unknown parent - allow (will be caught by DTD validation)
        return child in self.elements[parent].valid_children

    def get_valid_children(self, parent: str) -> Set[str]:
        """Get all valid child elements for parent."""
        if parent not in self.elements:
            return set()
        return self.elements[parent].valid_children.copy()

    def get_valid_parents(self, child: str) -> Set[str]:
        """Get all valid parent elements for child."""
        return self._parent_cache.get(child, set()).copy()

    def get_required_children(self, parent: str) -> Set[str]:
        """Get required child elements for parent."""
        if parent not in self.elements:
            return set()
        return self.elements[parent].required_children.copy()

    def get_required_attrs(self, element: str) -> Set[str]:
        """Get required attributes for element."""
        if element not in self.elements:
            return set()
        return self.elements[element].required_attrs.copy()

    def is_block_element(self, element: str) -> bool:
        """Check if element is block-level."""
        if element not in self.elements:
            return True  # Assume block for unknown
        return self.elements[element].category in (
            ElementCategory.BLOCK,
            ElementCategory.STRUCTURAL,
            ElementCategory.LIST,
            ElementCategory.TABLE,
            ElementCategory.MEDIA,
            ElementCategory.ADMONITION,
            ElementCategory.WRAPPER
        )

    def is_inline_element(self, element: str) -> bool:
        """Check if element is inline."""
        if element not in self.elements:
            return False
        return self.elements[element].category == ElementCategory.INLINE

    def allows_text(self, element: str) -> bool:
        """Check if element allows text content."""
        if element not in self.elements:
            return True
        return self.elements[element].allows_pcdata

    def is_empty_element(self, element: str) -> bool:
        """Check if element must be empty."""
        if element not in self.elements:
            return False
        return self.elements[element].is_empty

    def get_element_category(self, element: str) -> Optional[ElementCategory]:
        """Get the semantic category of an element."""
        if element not in self.elements:
            return None
        return self.elements[element].category

    def get_sequence_children(self, parent: str) -> List[str]:
        """Get the ordered sequence of children for parent."""
        if parent not in self.elements:
            return []
        return self.elements[parent].sequence_children.copy()

    def get_choice_groups(self, parent: str) -> List[Set[str]]:
        """Get all choice groups for parent."""
        if parent not in self.elements:
            return []
        return [group.copy() for group in self.elements[parent].choice_groups]

    def is_child_required(self, parent: str, child: str) -> bool:
        """Check if child is required in parent."""
        if parent not in self.elements:
            return False
        return child in self.elements[parent].required_children

    def is_child_optional(self, parent: str, child: str) -> bool:
        """Check if child is optional in parent."""
        if parent not in self.elements:
            return True  # Unknown = assume optional
        elem_def = self.elements[parent]
        # Optional if explicitly optional or in a choice group
        if child in elem_def.optional_children:
            return True
        # Also optional if in valid_children but not required
        return child in elem_def.valid_children and child not in elem_def.required_children

    def get_missing_required_children(self, parent: str, present_children: Set[str]) -> Set[str]:
        """Get required children that are missing from parent."""
        required = self.get_required_children(parent)
        return required - present_children

    def validate_child_order(self, parent: str, child_sequence: List[str]) -> Tuple[bool, Optional[str]]:
        """
        Validate that children appear in correct order for parent.

        Args:
            parent: Parent element name
            child_sequence: List of child element names in order they appear

        Returns:
            Tuple of (is_valid, error_message)
        """
        if parent not in self.elements:
            return True, None

        elem_def = self.elements[parent]
        expected_sequence = elem_def.sequence_children

        if not expected_sequence:
            # No sequence requirement (all children are in choice groups)
            return True, None

        # Build position map for expected elements
        position_map = {name: idx for idx, name in enumerate(expected_sequence)}

        last_position = -1
        last_element = None

        for child in child_sequence:
            if child not in position_map:
                # Child not in sequence - might be in a choice group, skip
                continue

            current_position = position_map[child]
            if current_position < last_position:
                return False, f"<{child}> must come before <{last_element}> in <{parent}>"

            last_position = current_position
            last_element = child

        return True, None

    def get_element_info(self, element: str) -> Optional[Dict[str, Any]]:
        """Get comprehensive information about an element."""
        if element not in self.elements:
            return None

        elem_def = self.elements[element]
        return {
            'name': elem_def.name,
            'content_type': elem_def.content_type.name,
            'category': elem_def.category.name,
            'valid_children': sorted(elem_def.valid_children),
            'required_children': sorted(elem_def.required_children),
            'optional_children': sorted(elem_def.optional_children),
            'sequence_children': elem_def.sequence_children,
            'choice_groups': [sorted(g) for g in elem_def.choice_groups],
            'valid_parents': sorted(elem_def.valid_parents),
            'required_attrs': sorted(elem_def.required_attrs),
            'optional_attrs': sorted(elem_def.optional_attrs),
            'allows_pcdata': elem_def.allows_pcdata,
            'is_empty': elem_def.is_empty
        }

    def find_valid_ancestor(self, child: str, context_element: etree._Element) -> Optional[etree._Element]:
        """
        Find the nearest ancestor that can contain the child element.
        Used for auto-extraction when an element can't be placed in current context.
        """
        valid_parents = self.get_valid_parents(child)
        if not valid_parents:
            return None

        # Walk up the tree to find a valid parent
        current = context_element
        while current is not None:
            if current.tag in valid_parents:
                return current
            current = current.getparent()

        return None

    # -------------------------------------------------------------------------
    # Ritt Element Support
    # -------------------------------------------------------------------------

    def is_ritt_element(self, element: str) -> bool:
        """Check if element is a Ritt-specific custom element."""
        return is_ritt_custom_element(element)

    def get_ritt_valid_children(self, parent: str) -> Set[str]:
        """Get valid children for a Ritt element."""
        model = get_ritt_content_model(parent)
        if model:
            return set(model.get('valid_children', frozenset()))
        return set()

    def is_valid_child_ritt(self, parent: str, child: str) -> bool:
        """Check if child is valid in Ritt parent element."""
        model = get_ritt_content_model(parent)
        if model:
            valid = model.get('valid_children', frozenset())
            return child in valid or not valid  # Empty means any
        return True

    def ritt_allows_text(self, element: str) -> bool:
        """Check if Ritt element allows PCDATA."""
        model = get_ritt_content_model(element)
        if model:
            return model.get('allows_pcdata', False)
        return False

    def validate_ritt_children(self, parent: str, children: List[str]) -> Tuple[bool, Optional[str]]:
        """Validate children of a Ritt element."""
        return validate_ritt_element(parent, children)

    # -------------------------------------------------------------------------
    # XSL Requirement Support
    # -------------------------------------------------------------------------

    def requires_id_for_xsl(self, element: str) -> bool:
        """Check if element requires ID for XSL output."""
        return element_requires_id(element)

    def id_ignored_by_xsl(self, element: str) -> bool:
        """Check if element's ID is ignored by XSL."""
        return is_id_ignored_by_xsl(element)

    def get_recommended_id_prefix(self, element: str) -> Optional[str]:
        """Get recommended ID prefix for element type."""
        return get_id_prefix_for_element(element)


# ============================================================================
# XHTML TO DOCBOOK MAPPING RULES
# ============================================================================

@dataclass
class TransformRule:
    """Rule for transforming XHTML element to DocBook."""
    xhtml_tag: str
    xhtml_class: Optional[str]  # CSS class to match (None = any)
    docbook_tag: str
    valid_parents: Set[str]  # Valid parent elements in DocBook
    invalid_parents: Set[str]  # Must extract from these parents
    fallback_tag: Optional[str]  # Fallback if can't place
    preserve_text: bool = True
    preserve_children: bool = True
    attrs_to_copy: Set[str] = field(default_factory=set)
    attrs_to_transform: Dict[str, Callable] = field(default_factory=dict)


# Parent sets for common element placement contexts
_SECTION_PARENTS = {'sect1', 'sect2', 'sect3', 'sect4', 'sect5', 'chapter', 'appendix',
                    'preface', 'article', 'sidebar', 'simplesect'}
_BLOCK_PARENTS = _SECTION_PARENTS | {'blockquote', 'listitem', 'footnote', 'note',
                                      'warning', 'tip', 'caution', 'important',
                                      'entry', 'glossdef', 'answer', 'step', 'example'}
_PARA_LIKE = {'para', 'simpara', 'title', 'subtitle', 'footnote'}

# Default transformation rules - comprehensive mapping from XHTML to DocBook
XHTML_TO_DOCBOOK_RULES: Dict[str, List[TransformRule]] = {
    # =========================================================================
    # BLOCK ELEMENTS
    # =========================================================================
    'div': [
        # Semantic div classes mapped to specific DocBook elements
        TransformRule('div', 'figure', 'figure',
                      _SECTION_PARENTS, _PARA_LIKE, 'informalfigure'),
        TransformRule('div', 'sidebar', 'sidebar',
                      _SECTION_PARENTS, _PARA_LIKE | {'sidebar'}, None),
        TransformRule('div', 'note', 'note',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'tip', 'tip',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'warning', 'warning',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'caution', 'caution',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'important', 'important',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'example', 'example',
                      _SECTION_PARENTS, _PARA_LIKE, 'informalexample'),
        TransformRule('div', 'callout', 'note',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'box', 'sidebar',
                      _SECTION_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'infobox', 'sidebar',
                      _SECTION_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'highlight', 'sidebar',
                      _SECTION_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'pullquote', 'epigraph',
                      _SECTION_PARENTS, _PARA_LIKE, 'blockquote'),
        TransformRule('div', 'blockquote', 'blockquote',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'abstract', 'abstract',
                      {'bookinfo', 'chapterinfo', 'sect1info', 'articleinfo'}, _PARA_LIKE, 'blockquote'),
        TransformRule('div', 'dedication', 'dedication',
                      {'book', 'part'}, _PARA_LIKE, 'sidebar'),
        TransformRule('div', 'epigraph', 'epigraph',
                      _SECTION_PARENTS, _PARA_LIKE, 'blockquote'),
        TransformRule('div', 'equation', 'equation',
                      _SECTION_PARENTS, _PARA_LIKE, 'informalequation'),
        TransformRule('div', 'procedure', 'procedure',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('div', 'glossary', 'glossary',
                      _SECTION_PARENTS, _PARA_LIKE, 'glosslist'),
        TransformRule('div', 'bibliography', 'bibliography',
                      _SECTION_PARENTS, _PARA_LIKE, 'bibliomixed'),
        TransformRule('div', 'chapter', 'chapter',
                      {'book', 'part'}, _PARA_LIKE, 'sect1'),
        TransformRule('div', 'section', 'sect1',
                      _SECTION_PARENTS, _PARA_LIKE, 'simplesect'),
        # Default div -> para (last resort)
        TransformRule('div', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'p': [
        TransformRule('p', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'figure': [
        TransformRule('figure', None, 'figure',
                      _SECTION_PARENTS, _PARA_LIKE | {'entry'}, 'informalfigure'),
    ],
    'figcaption': [
        TransformRule('figcaption', None, 'title', {'figure', 'informalfigure'}, set(), 'caption'),
    ],
    'table': [
        TransformRule('table', None, 'table',
                      _SECTION_PARENTS, _PARA_LIKE, 'informaltable'),
    ],
    'blockquote': [
        TransformRule('blockquote', None, 'blockquote',
                      _BLOCK_PARENTS, _PARA_LIKE, None),
    ],

    # =========================================================================
    # HEADINGS - Map to title or section structure
    # =========================================================================
    'h1': [
        TransformRule('h1', 'chapter-title', 'title', {'chapter'}, set(), None),
        TransformRule('h1', 'book-title', 'title', {'book', 'bookinfo'}, set(), None),
        TransformRule('h1', None, 'title', _SECTION_PARENTS | {'chapter', 'appendix', 'preface'}, set(), 'bridgehead'),
    ],
    'h2': [
        TransformRule('h2', 'section-title', 'title', {'sect1'}, set(), None),
        TransformRule('h2', None, 'title', _SECTION_PARENTS, set(), 'bridgehead'),
    ],
    'h3': [
        TransformRule('h3', None, 'title', {'sect2', 'simplesect', 'sidebar'}, set(), 'bridgehead'),
    ],
    'h4': [
        TransformRule('h4', None, 'title', {'sect3', 'simplesect', 'sidebar', 'note', 'warning'}, set(), 'bridgehead'),
    ],
    'h5': [
        TransformRule('h5', None, 'title', {'sect4', 'simplesect'}, set(), 'bridgehead'),
    ],
    'h6': [
        TransformRule('h6', None, 'title', {'sect5', 'simplesect'}, set(), 'bridgehead'),
    ],

    # =========================================================================
    # HTML5 SEMANTIC ELEMENTS
    # =========================================================================
    'section': [
        TransformRule('section', 'chapter', 'chapter', {'book', 'part'}, set(), 'sect1'),
        TransformRule('section', 'appendix', 'appendix', {'book', 'part'}, set(), 'sect1'),
        TransformRule('section', None, 'sect1', _SECTION_PARENTS, _PARA_LIKE, 'simplesect'),
    ],
    'article': [
        TransformRule('article', None, 'article', {'book', 'part'}, set(), 'sect1'),
    ],
    'aside': [
        TransformRule('aside', 'note', 'note', _BLOCK_PARENTS, _PARA_LIKE, None),
        TransformRule('aside', None, 'sidebar', _SECTION_PARENTS, _PARA_LIKE, 'note'),
    ],
    'nav': [
        # Navigation typically maps to TOC or list
        TransformRule('nav', 'toc', 'toc', _SECTION_PARENTS, _PARA_LIKE, None),
        TransformRule('nav', None, 'itemizedlist', _BLOCK_PARENTS, _PARA_LIKE, None),
    ],
    'header': [
        # Header content typically wraps title/metadata
        TransformRule('header', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'footer': [
        # Footer often contains copyright/attribution
        TransformRule('footer', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'main': [
        # Main content area - pass through
        TransformRule('main', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'address': [
        TransformRule('address', None, 'address', _BLOCK_PARENTS, set(), 'para'),
    ],
    'details': [
        # Collapsible content -> sidebar or note
        TransformRule('details', None, 'sidebar', _SECTION_PARENTS, _PARA_LIKE, 'note'),
    ],
    'summary': [
        # Summary of details -> title
        TransformRule('summary', None, 'title', {'sidebar', 'note'}, set(), 'para'),
    ],

    # =========================================================================
    # INLINE TEXT FORMATTING
    # =========================================================================
    'span': [
        TransformRule('span', 'emphasis', 'emphasis', set(), set(), 'phrase'),
        TransformRule('span', 'bold', 'emphasis', set(), set(), 'phrase'),
        TransformRule('span', 'strong', 'emphasis', set(), set(), 'phrase'),
        TransformRule('span', 'italic', 'emphasis', set(), set(), 'phrase'),
        TransformRule('span', 'underline', 'emphasis', set(), set(), 'phrase'),
        TransformRule('span', 'strikethrough', 'phrase', set(), set(), None),
        TransformRule('span', 'code', 'literal', set(), set(), 'phrase'),
        TransformRule('span', 'monospace', 'literal', set(), set(), 'phrase'),
        TransformRule('span', 'smallcaps', 'phrase', set(), set(), None),
        TransformRule('span', 'superscript', 'superscript', set(), set(), None),
        TransformRule('span', 'subscript', 'subscript', set(), set(), None),
        TransformRule('span', 'footnote', 'footnote', set(), set(), None),
        TransformRule('span', 'term', 'glossterm', set(), set(), 'firstterm'),
        TransformRule('span', 'glossterm', 'glossterm', set(), set(), 'firstterm'),
        TransformRule('span', 'firstterm', 'firstterm', set(), set(), 'glossterm'),
        TransformRule('span', 'acronym', 'acronym', set(), set(), 'phrase'),
        TransformRule('span', 'abbreviation', 'abbrev', set(), set(), 'phrase'),
        TransformRule('span', 'foreign', 'foreignphrase', set(), set(), 'phrase'),
        TransformRule('span', 'quote', 'quote', set(), set(), 'phrase'),
        TransformRule('span', 'citation', 'citation', set(), set(), 'phrase'),
        TransformRule('span', 'filename', 'filename', set(), set(), 'literal'),
        TransformRule('span', 'command', 'command', set(), set(), 'literal'),
        TransformRule('span', 'userinput', 'userinput', set(), set(), 'literal'),
        TransformRule('span', 'computeroutput', 'computeroutput', set(), set(), 'literal'),
        TransformRule('span', 'email', 'email', set(), set(), 'ulink'),
        TransformRule('span', 'trademark', 'trademark', set(), set(), 'phrase'),
        TransformRule('span', None, 'phrase', set(), set(), None),  # Default
    ],
    'em': [
        TransformRule('em', None, 'emphasis', set(), set(), 'phrase'),
    ],
    'strong': [
        TransformRule('strong', None, 'emphasis', set(), set(), 'phrase'),
    ],
    'i': [
        TransformRule('i', None, 'emphasis', set(), set(), 'phrase'),
    ],
    'b': [
        TransformRule('b', None, 'emphasis', set(), set(), 'phrase'),
    ],
    'u': [
        TransformRule('u', None, 'emphasis', set(), set(), 'phrase'),
    ],
    's': [
        # Strikethrough - no direct equivalent, use phrase
        TransformRule('s', None, 'phrase', set(), set(), None),
    ],
    'del': [
        TransformRule('del', None, 'phrase', set(), set(), None),
    ],
    'ins': [
        TransformRule('ins', None, 'phrase', set(), set(), None),
    ],
    'mark': [
        TransformRule('mark', None, 'emphasis', set(), set(), 'phrase'),
    ],
    'a': [
        TransformRule('a', None, 'ulink', set(), set(), 'phrase',
                      attrs_to_copy={'href'}),
    ],
    'sub': [
        TransformRule('sub', None, 'subscript', set(), set(), None),
    ],
    'sup': [
        TransformRule('sup', None, 'superscript', set(), set(), None),
    ],
    'code': [
        TransformRule('code', None, 'literal', set(), set(), 'phrase'),
    ],
    'kbd': [
        TransformRule('kbd', None, 'keycap', set(), set(), 'literal'),
    ],
    'samp': [
        TransformRule('samp', None, 'computeroutput', set(), set(), 'literal'),
    ],
    'var': [
        TransformRule('var', None, 'varname', set(), set(), 'replaceable'),
    ],
    'q': [
        TransformRule('q', None, 'quote', set(), set(), 'phrase'),
    ],
    'cite': [
        TransformRule('cite', None, 'citetitle', set(), set(), 'citation'),
    ],
    'abbr': [
        TransformRule('abbr', None, 'abbrev', set(), set(), 'phrase'),
    ],
    'acronym': [
        TransformRule('acronym', None, 'acronym', set(), set(), 'phrase'),
    ],
    'dfn': [
        TransformRule('dfn', None, 'firstterm', set(), set(), 'glossterm'),
    ],
    'time': [
        TransformRule('time', None, 'date', set(), set(), 'phrase'),
    ],
    'data': [
        TransformRule('data', None, 'phrase', set(), set(), None),
    ],
    'small': [
        TransformRule('small', None, 'phrase', set(), set(), None),
    ],
    'big': [
        TransformRule('big', None, 'phrase', set(), set(), None),
    ],
    'tt': [
        TransformRule('tt', None, 'literal', set(), set(), 'phrase'),
    ],

    # =========================================================================
    # CODE AND PREFORMATTED TEXT
    # =========================================================================
    'pre': [
        TransformRule('pre', 'code', 'programlisting', _SECTION_PARENTS | {'example'}, _PARA_LIKE, 'screen'),
        TransformRule('pre', 'screen', 'screen', _SECTION_PARENTS | {'example'}, _PARA_LIKE, None),
        TransformRule('pre', 'literallayout', 'literallayout', _BLOCK_PARENTS, _PARA_LIKE, 'screen'),
        TransformRule('pre', None, 'programlisting', _SECTION_PARENTS | {'example'}, _PARA_LIKE, 'screen'),
    ],

    # =========================================================================
    # LIST ELEMENTS
    # =========================================================================
    'ul': [
        TransformRule('ul', None, 'itemizedlist', _BLOCK_PARENTS, _PARA_LIKE | {'emphasis'}, None),
    ],
    'ol': [
        TransformRule('ol', None, 'orderedlist', _BLOCK_PARENTS, _PARA_LIKE | {'emphasis'}, None),
    ],
    'li': [
        TransformRule('li', None, 'listitem', {'itemizedlist', 'orderedlist'}, set(), None),
    ],
    'dl': [
        TransformRule('dl', None, 'variablelist', _SECTION_PARENTS, _PARA_LIKE, None),
    ],
    'dt': [
        TransformRule('dt', None, 'term', {'varlistentry'}, set(), None),
    ],
    'dd': [
        TransformRule('dd', None, 'listitem', {'varlistentry'}, set(), None),
    ],
    'menu': [
        # Deprecated in HTML5, treat as list
        TransformRule('menu', None, 'itemizedlist', _BLOCK_PARENTS, _PARA_LIKE, None),
    ],

    # =========================================================================
    # TABLE ELEMENTS
    # =========================================================================
    'table': [
        TransformRule('table', None, 'table', _SECTION_PARENTS, _PARA_LIKE, 'informaltable'),
    ],
    'caption': [
        TransformRule('caption', None, 'title', {'table', 'informaltable'}, set(), None),
    ],
    'thead': [
        TransformRule('thead', None, 'thead', {'tgroup'}, set(), None),
    ],
    'tbody': [
        TransformRule('tbody', None, 'tbody', {'tgroup'}, set(), None),
    ],
    'tfoot': [
        TransformRule('tfoot', None, 'tfoot', {'tgroup'}, set(), None),
    ],
    'tr': [
        TransformRule('tr', None, 'row', {'thead', 'tbody', 'tfoot'}, set(), None),
    ],
    'th': [
        TransformRule('th', None, 'entry', {'row'}, set(), None),
    ],
    'td': [
        TransformRule('td', None, 'entry', {'row'}, set(), None),
    ],
    'colgroup': [
        # Colgroup maps to multiple colspecs
        TransformRule('colgroup', None, 'colspec', {'tgroup'}, set(), None),
    ],
    'col': [
        TransformRule('col', None, 'colspec', {'tgroup'}, set(), None),
    ],

    # =========================================================================
    # MEDIA ELEMENTS
    # =========================================================================
    'img': [
        TransformRule('img', None, 'imagedata', {'imageobject'}, set(), None,
                      attrs_to_copy={'src', 'alt', 'width', 'height'}),
    ],
    'picture': [
        # Picture element -> mediaobject
        TransformRule('picture', None, 'mediaobject', {'figure', 'informalfigure', 'para'}, set(), 'inlinemediaobject'),
    ],
    'source': [
        # Source within picture -> imagedata
        TransformRule('source', None, 'imagedata', {'imageobject'}, set(), None),
    ],
    'video': [
        TransformRule('video', None, 'videoobject', {'mediaobject'}, set(), None),
    ],
    'audio': [
        TransformRule('audio', None, 'audioobject', {'mediaobject'}, set(), None),
    ],
    'track': [
        # Subtitles/captions -> textobject
        TransformRule('track', None, 'textobject', {'videoobject', 'audioobject'}, set(), None),
    ],
    'object': [
        TransformRule('object', None, 'mediaobject', {'figure', 'informalfigure', 'para'}, set(), 'inlinemediaobject'),
    ],
    'embed': [
        TransformRule('embed', None, 'mediaobject', {'figure', 'informalfigure', 'para'}, set(), 'inlinemediaobject'),
    ],
    'iframe': [
        # iframe -> ulink with note about embedded content
        TransformRule('iframe', None, 'ulink', set(), set(), 'phrase'),
    ],
    'svg': [
        # SVG -> imagedata (inline or in figure)
        TransformRule('svg', None, 'imagedata', {'imageobject'}, set(), None),
    ],

    # =========================================================================
    # MATH ELEMENTS (MathML)
    # =========================================================================
    'math': [
        TransformRule('math', None, 'inlineequation', set(), set(), None),
    ],

    # =========================================================================
    # LINE BREAKS AND HORIZONTAL RULES
    # =========================================================================
    'br': [
        # Line break -> empty processing instruction or literallayout
        TransformRule('br', None, 'beginpage', set(), set(), None),
    ],
    'hr': [
        # Horizontal rule -> can be bridgehead or use beginpage
        TransformRule('hr', None, 'beginpage', _SECTION_PARENTS, _PARA_LIKE, None),
    ],
    'wbr': [
        # Word break opportunity - no DocBook equivalent, ignore
        TransformRule('wbr', None, 'phrase', set(), set(), None),
    ],

    # =========================================================================
    # RUBY ANNOTATIONS (East Asian text)
    # =========================================================================
    'ruby': [
        TransformRule('ruby', None, 'phrase', set(), set(), None),
    ],
    'rt': [
        TransformRule('rt', None, 'phrase', set(), set(), None),
    ],
    'rp': [
        TransformRule('rp', None, 'phrase', set(), set(), None),
    ],

    # =========================================================================
    # FORM ELEMENTS (rarely used in ePub but may appear)
    # =========================================================================
    'form': [
        TransformRule('form', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'input': [
        TransformRule('input', None, 'phrase', set(), set(), None),
    ],
    'button': [
        TransformRule('button', None, 'guibutton', set(), set(), 'phrase'),
    ],
    'select': [
        TransformRule('select', None, 'menuchoice', set(), set(), 'phrase'),
    ],
    'option': [
        TransformRule('option', None, 'guimenuitem', set(), set(), 'phrase'),
    ],
    'textarea': [
        TransformRule('textarea', None, 'programlisting', _BLOCK_PARENTS, _PARA_LIKE, 'screen'),
    ],
    'label': [
        TransformRule('label', None, 'phrase', set(), set(), None),
    ],
    'fieldset': [
        TransformRule('fieldset', None, 'sidebar', _SECTION_PARENTS, _PARA_LIKE, 'para'),
    ],
    'legend': [
        TransformRule('legend', None, 'title', {'sidebar'}, set(), 'para'),
    ],

    # =========================================================================
    # SCRIPTING ELEMENTS (should be stripped or converted to comments)
    # =========================================================================
    'script': [
        # Script content should be stripped - return None/empty
        TransformRule('script', None, 'remark', set(), set(), None, preserve_text=False, preserve_children=False),
    ],
    'noscript': [
        # Noscript content may be preserved
        TransformRule('noscript', None, 'para', _BLOCK_PARENTS, set(), 'simpara'),
    ],
    'template': [
        # Template elements should be stripped
        TransformRule('template', None, 'remark', set(), set(), None, preserve_text=False, preserve_children=False),
    ],
    'canvas': [
        # Canvas -> placeholder image reference
        TransformRule('canvas', None, 'mediaobject', {'figure', 'para'}, set(), 'inlinemediaobject'),
    ],

    # =========================================================================
    # METADATA ELEMENTS (often in head, but may appear in body)
    # =========================================================================
    'title': [
        TransformRule('title', None, 'title', _SECTION_PARENTS | {'bookinfo', 'chapterinfo'}, set(), 'bridgehead'),
    ],
    'meta': [
        # Meta tags should be processed for metadata extraction, not converted
        TransformRule('meta', None, 'remark', set(), set(), None, preserve_text=False),
    ],
    'link': [
        # Link tags (stylesheet etc) - skip in body
        TransformRule('link', None, 'remark', set(), set(), None, preserve_text=False),
    ],
    'style': [
        # Style tags - skip
        TransformRule('style', None, 'remark', set(), set(), None, preserve_text=False, preserve_children=False),
    ],
    'base': [
        TransformRule('base', None, 'remark', set(), set(), None, preserve_text=False),
    ],
}


# ============================================================================
# XHTML TRANSFORMATION HELPERS
# ============================================================================

def get_transform_rule(xhtml_tag: str, css_class: Optional[str] = None,
                       parent_tag: Optional[str] = None) -> Optional[TransformRule]:
    """
    Get the appropriate transformation rule for an XHTML element.

    Args:
        xhtml_tag: XHTML tag name (lowercase)
        css_class: Optional CSS class to match specific rules
        parent_tag: Optional parent DocBook tag for context-aware selection

    Returns:
        TransformRule or None if no rule found
    """
    tag = xhtml_tag.lower()
    if tag not in XHTML_TO_DOCBOOK_RULES:
        return None

    rules = XHTML_TO_DOCBOOK_RULES[tag]

    # First pass: Look for class-specific match
    if css_class:
        for rule in rules:
            if rule.xhtml_class == css_class:
                return rule

    # Second pass: Look for rules that work with given parent
    if parent_tag:
        for rule in rules:
            if rule.xhtml_class is None:  # Generic rule
                if not rule.valid_parents or parent_tag in rule.valid_parents:
                    return rule

    # Third pass: Return first generic (class=None) rule
    for rule in rules:
        if rule.xhtml_class is None:
            return rule

    # Fallback: Return first rule
    return rules[0] if rules else None


def get_docbook_tag(xhtml_tag: str, css_class: Optional[str] = None,
                    parent_tag: Optional[str] = None) -> str:
    """
    Get the DocBook tag for an XHTML element.

    Returns the mapped DocBook tag name, or the original tag if no mapping exists.
    """
    rule = get_transform_rule(xhtml_tag, css_class, parent_tag)
    if rule:
        return rule.docbook_tag
    # Return original tag if no mapping (will likely be rejected by DTD)
    return xhtml_tag


def get_all_xhtml_tags() -> Set[str]:
    """Get all XHTML tags that have transformation rules."""
    return set(XHTML_TO_DOCBOOK_RULES.keys())


def get_rules_for_docbook_tag(docbook_tag: str) -> List[TransformRule]:
    """Get all transformation rules that produce a given DocBook tag."""
    result = []
    for rules in XHTML_TO_DOCBOOK_RULES.values():
        for rule in rules:
            if rule.docbook_tag == docbook_tag:
                result.append(rule)
    return result


# ============================================================================
# DOCBOOK BUILDER - Validated Element Factory
# ============================================================================

class DocBookBuilder:
    """
    Validated XML element factory for DocBook.

    Creates elements with content model validation and auto-restructuring.
    Never loses content - finds alternative placements when needed.
    """

    def __init__(self, content_model: DTDContentModel):
        self.model = content_model
        self.pending_elements: List[Tuple[etree._Element, Set[str]]] = []
        self.content_buffer: List[Tuple[Any, etree._Element]] = []  # (content, intended_parent)
        self.violations: List[ContentModelViolation] = []
        self.audit_log: List[str] = []

    def create_element(self, tag: str, parent: Optional[etree._Element] = None,
                       text: Optional[str] = None, **attrs) -> etree._Element:
        """
        Create a DocBook element with validation.

        Args:
            tag: Element tag name
            parent: Optional parent element (if None, creates root)
            text: Optional text content
            **attrs: Element attributes

        Returns:
            Created element (may be restructured if needed)
        """
        # Add required attributes with defaults
        required = self.model.get_required_attrs(tag)
        for attr in required:
            if attr not in attrs:
                elem_def = self.model.elements.get(tag)
                if elem_def and attr in elem_def.default_attrs:
                    attrs[attr] = elem_def.default_attrs[attr]
                elif attr == 'id':
                    pass  # ID will be generated by caller
                else:
                    self._log(f"Warning: Required attribute '{attr}' missing for <{tag}>")

        # Create element
        if parent is None:
            elem = etree.Element(tag, **attrs)
        else:
            # Validate parent-child relationship
            if not self.model.is_valid_child(parent.tag, tag):
                # Try to find valid placement
                elem = self._handle_invalid_child(parent, tag, text, attrs)
                if elem is not None:
                    return elem
                # Fall through to create anyway (will be fixed later)
                self._record_violation(parent.tag, tag, 'invalid_child',
                                       f"<{tag}> not valid in <{parent.tag}>")

            elem = etree.SubElement(parent, tag, **attrs)

        # Add text content
        if text:
            if self.model.allows_text(tag):
                elem.text = text
            else:
                self._record_violation(tag, '#PCDATA', 'text_not_allowed',
                                       f"<{tag}> does not allow text content")
                self._log(f"Warning: Text not allowed in <{tag}>, wrapping in child element")
                # Wrap text in appropriate child
                self._wrap_text_in_child(elem, text)

        return elem

    def add_child(self, parent: etree._Element, tag: str,
                  text: Optional[str] = None, **attrs) -> etree._Element:
        """
        Add a child element with validation and auto-restructuring.

        If the child can't be placed in parent, finds alternative placement.
        """
        return self.create_element(tag, parent, text, **attrs)

    def add_text(self, parent: etree._Element, text: str) -> None:
        """Add text content to element, handling restrictions."""
        if not text:
            return

        if self.model.allows_text(parent.tag):
            if len(parent) == 0:
                parent.text = (parent.text or '') + text
            else:
                # Add to tail of last child
                last_child = parent[-1]
                last_child.tail = (last_child.tail or '') + text
        else:
            # Element doesn't allow text - wrap in para or similar
            self._wrap_text_in_child(parent, text)

    def finalize_element(self, elem: etree._Element) -> List[ContentModelViolation]:
        """
        Finalize an element, checking all required children are present.

        Returns list of violations found.
        """
        violations = []
        required = self.model.get_required_children(elem.tag)

        for req_child in required:
            if elem.find(req_child) is None:
                violation = ContentModelViolation(
                    parent_element=elem.tag,
                    child_element=req_child,
                    violation_type='missing_required',
                    message=f"<{elem.tag}> missing required child <{req_child}>",
                    suggested_fix=f"Add empty <{req_child}> element"
                )
                violations.append(violation)
                # Auto-add required child
                self._add_required_child(elem, req_child)

        return violations

    def _handle_invalid_child(self, parent: etree._Element, child_tag: str,
                              text: Optional[str], attrs: Dict) -> Optional[etree._Element]:
        """
        Handle case where child is not valid in parent.

        Strategies:
        1. Find valid ancestor and place there
        2. Use fallback element
        3. Extract from parent and place after
        """
        # Strategy 1: Find valid ancestor
        valid_ancestor = self.model.find_valid_ancestor(child_tag, parent)
        if valid_ancestor is not None and valid_ancestor is not parent:
            self._log(f"Extracting <{child_tag}> from <{parent.tag}> to <{valid_ancestor.tag}>")
            elem = etree.SubElement(valid_ancestor, child_tag, **attrs)
            if text:
                elem.text = text
            return elem

        # Strategy 2: Check if this is block in inline - extract to after parent
        if self.model.is_block_element(child_tag) and self.model.is_inline_element(parent.tag):
            grandparent = parent.getparent()
            if grandparent is not None and self.model.is_valid_child(grandparent.tag, child_tag):
                self._log(f"Extracting block <{child_tag}> from inline <{parent.tag}>")
                # Insert after parent in grandparent
                parent_idx = list(grandparent).index(parent)
                elem = etree.Element(child_tag, **attrs)
                if text:
                    elem.text = text
                grandparent.insert(parent_idx + 1, elem)
                return elem

        # Strategy 3: Use fallback if block element in para-like
        if parent.tag in ('para', 'simpara', 'title') and self.model.is_block_element(child_tag):
            grandparent = parent.getparent()
            if grandparent is not None:
                self._log(f"Extracting <{child_tag}> from <{parent.tag}>, placing after")
                parent_idx = list(grandparent).index(parent)
                elem = etree.Element(child_tag, **attrs)
                if text:
                    elem.text = text
                grandparent.insert(parent_idx + 1, elem)
                return elem

        return None  # Could not find valid placement

    def _wrap_text_in_child(self, parent: etree._Element, text: str) -> None:
        """Wrap text in appropriate child element when parent doesn't allow text."""
        # Determine best wrapper based on parent type
        if parent.tag in ('sect1', 'sect2', 'sect3', 'sect4', 'sect5',
                          'chapter', 'appendix', 'preface', 'sidebar'):
            wrapper = etree.SubElement(parent, 'para')
            wrapper.text = text
        elif parent.tag in ('itemizedlist', 'orderedlist'):
            listitem = etree.SubElement(parent, 'listitem')
            para = etree.SubElement(listitem, 'para')
            para.text = text
        elif parent.tag == 'variablelist':
            entry = etree.SubElement(parent, 'varlistentry')
            term = etree.SubElement(entry, 'term')
            term.text = text
            listitem = etree.SubElement(entry, 'listitem')
            etree.SubElement(listitem, 'para')
        else:
            # Generic: try to add para
            valid_children = self.model.get_valid_children(parent.tag)
            if 'para' in valid_children:
                para = etree.SubElement(parent, 'para')
                para.text = text
            elif 'simpara' in valid_children:
                simpara = etree.SubElement(parent, 'simpara')
                simpara.text = text
            else:
                # Last resort: store in audit log for manual review
                self._log(f"Warning: Could not place text in <{parent.tag}>: {text[:50]}...")
                self.content_buffer.append((text, parent))

    def _add_required_child(self, parent: etree._Element, child_tag: str) -> None:
        """Add a required child element with minimal content."""
        child = etree.SubElement(parent, child_tag)

        # Add minimal content based on child type
        if child_tag == 'title':
            child.text = ''  # Empty title - should be filled by caller
        elif child_tag == 'tgroup':
            # Tables need tgroup with cols
            child.set('cols', '1')
            tbody = etree.SubElement(child, 'tbody')
            row = etree.SubElement(tbody, 'row')
            etree.SubElement(row, 'entry')
        elif child_tag == 'mediaobject':
            imageobj = etree.SubElement(child, 'imageobject')
            etree.SubElement(imageobj, 'imagedata')
        elif child_tag == 'para':
            pass  # Empty para is valid
        elif child_tag == 'listitem':
            para = etree.SubElement(child, 'para')

    def _record_violation(self, parent: str, child: str,
                          violation_type: str, message: str) -> None:
        """Record a content model violation for reporting."""
        violation = ContentModelViolation(
            parent_element=parent,
            child_element=child,
            violation_type=violation_type,
            message=message
        )
        self.violations.append(violation)

    def _log(self, message: str) -> None:
        """Add to audit log."""
        self.audit_log.append(message)
        logger.debug(message)

    def get_violations(self) -> List[ContentModelViolation]:
        """Get all recorded violations."""
        return self.violations.copy()

    def get_audit_log(self) -> List[str]:
        """Get audit log of all transformations."""
        return self.audit_log.copy()

    def get_lost_content(self) -> List[Tuple[Any, str]]:
        """Get any content that couldn't be placed."""
        return [(content, parent.tag) for content, parent in self.content_buffer]

    # -------------------------------------------------------------------------
    # XSL-Aware ID Handling
    # -------------------------------------------------------------------------

    def ensure_xsl_required_id(self, elem: etree._Element,
                               id_generator: Optional[Callable[[str], str]] = None) -> Optional[str]:
        """
        Ensure element has an ID if required by XSL.

        If element requires an ID for XSL output and doesn't have one,
        generates and assigns an ID using the provided generator.

        Args:
            elem: Element to check
            id_generator: Function(tag) -> id string. If None, uses default pattern.

        Returns:
            Assigned ID string, or None if ID not needed/already present
        """
        tag = elem.tag
        if not self.model.requires_id_for_xsl(tag):
            return None

        if elem.get('id'):
            return None  # Already has ID

        # Generate ID
        if id_generator:
            new_id = id_generator(tag)
        else:
            # Default: use recommended prefix + auto-increment
            prefix = self.model.get_recommended_id_prefix(tag) or tag[:2]
            new_id = f"{prefix}{len(self.audit_log):04d}"

        elem.set('id', new_id)
        self._log(f"Generated XSL-required ID for <{tag}>: {new_id}")
        return new_id

    def validate_xsl_ids(self, root: etree._Element) -> List[Dict[str, str]]:
        """
        Validate all XSL-required IDs are present in document.

        Args:
            root: Document root element

        Returns:
            List of dicts with 'element', 'tag', 'issue' for each problem
        """
        issues = []

        for elem in root.iter():
            tag = elem.tag
            if isinstance(tag, str):  # Skip comments/PIs
                if self.model.requires_id_for_xsl(tag) and not elem.get('id'):
                    issues.append({
                        'element': elem,
                        'tag': tag,
                        'issue': f"<{tag}> requires ID for XSL output"
                    })

        return issues

    def fix_xsl_ids(self, root: etree._Element,
                    id_generator: Optional[Callable[[str, int], str]] = None) -> int:
        """
        Fix all missing XSL-required IDs in document.

        Args:
            root: Document root element
            id_generator: Function(tag, index) -> id string

        Returns:
            Number of IDs added
        """
        count = 0
        counters: Dict[str, int] = {}

        for elem in root.iter():
            tag = elem.tag
            if isinstance(tag, str) and self.model.requires_id_for_xsl(tag):
                if not elem.get('id'):
                    prefix = self.model.get_recommended_id_prefix(tag) or tag[:2]
                    counters[prefix] = counters.get(prefix, 0) + 1

                    if id_generator:
                        new_id = id_generator(tag, counters[prefix])
                    else:
                        new_id = f"{prefix}{counters[prefix]:04d}"

                    elem.set('id', new_id)
                    count += 1
                    self._log(f"Fixed missing XSL ID: <{tag}> -> {new_id}")

        return count

    # -------------------------------------------------------------------------
    # Ritt Element Creation
    # -------------------------------------------------------------------------

    def create_ritt_element(self, tag: str, parent: Optional[etree._Element] = None,
                            text: Optional[str] = None, **attrs) -> Optional[etree._Element]:
        """
        Create a Ritt-specific element with content model validation.

        Args:
            tag: Ritt element tag name
            parent: Optional parent element
            text: Optional text content
            **attrs: Element attributes

        Returns:
            Created element or None if not a valid Ritt element
        """
        if not is_ritt_custom_element(tag):
            self._log(f"Warning: <{tag}> is not a Ritt element, using standard creation")
            return self.create_element(tag, parent, text, **attrs)

        model = get_ritt_content_model(tag)

        # Create element
        if parent is None:
            elem = etree.Element(tag, **attrs)
        else:
            elem = etree.SubElement(parent, tag, **attrs)

        # Add text if allowed
        if text:
            if model and model.get('allows_pcdata', False):
                elem.text = text
            else:
                self._log(f"Warning: Text not allowed in Ritt element <{tag}>")

        return elem

    def finalize_ritt_element(self, elem: etree._Element) -> List[ContentModelViolation]:
        """
        Finalize a Ritt element, checking all requirements.

        Returns list of violations found.
        """
        tag = elem.tag
        if not is_ritt_custom_element(tag):
            return []

        violations = []
        model = get_ritt_content_model(tag)

        if model:
            children = [child.tag for child in elem]
            is_valid, error_msg = validate_ritt_element(tag, children)

            if not is_valid:
                violation = ContentModelViolation(
                    parent_element=tag,
                    child_element='',
                    violation_type='ritt_validation',
                    message=error_msg or f"Ritt element <{tag}> validation failed"
                )
                violations.append(violation)

        return violations


# ============================================================================
# SINGLETON INSTANCE
# ============================================================================

_content_model: Optional[DTDContentModel] = None
_builder: Optional[DocBookBuilder] = None
_lock = threading.Lock()


def get_content_model(dtd_path: Optional[Path] = None) -> DTDContentModel:
    """Get the singleton DTD content model instance."""
    global _content_model
    with _lock:
        if _content_model is None:
            if dtd_path is None:
                # Default DTD path
                dtd_path = Path(__file__).parent / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
            _content_model = DTDContentModel(dtd_path)
        return _content_model


def get_builder(dtd_path: Optional[Path] = None) -> DocBookBuilder:
    """Get a DocBook builder instance."""
    model = get_content_model(dtd_path)
    # Return new builder each time (stateful)
    return DocBookBuilder(model)


def reset_builder() -> None:
    """Reset the singleton instances."""
    global _content_model, _builder
    with _lock:
        _content_model = None
        _builder = None


# ============================================================================
# CONVENIENCE FUNCTIONS
# ============================================================================

def is_valid_child(parent: str, child: str, dtd_path: Optional[Path] = None) -> bool:
    """Check if child element is valid inside parent."""
    return get_content_model(dtd_path).is_valid_child(parent, child)


def get_valid_children(parent: str, dtd_path: Optional[Path] = None) -> Set[str]:
    """Get all valid child elements for parent."""
    return get_content_model(dtd_path).get_valid_children(parent)


def is_block_element(element: str, dtd_path: Optional[Path] = None) -> bool:
    """Check if element is block-level."""
    return get_content_model(dtd_path).is_block_element(element)


def is_inline_element(element: str, dtd_path: Optional[Path] = None) -> bool:
    """Check if element is inline."""
    return get_content_model(dtd_path).is_inline_element(element)


def get_xhtml_mapping_stats() -> Dict[str, Any]:
    """Get statistics about XHTML to DocBook mappings."""
    total_tags = len(XHTML_TO_DOCBOOK_RULES)
    total_rules = sum(len(rules) for rules in XHTML_TO_DOCBOOK_RULES.values())

    # Count by category
    docbook_targets = set()
    class_specific = 0
    generic = 0

    for rules in XHTML_TO_DOCBOOK_RULES.values():
        for rule in rules:
            docbook_targets.add(rule.docbook_tag)
            if rule.xhtml_class:
                class_specific += 1
            else:
                generic += 1

    return {
        'xhtml_tags': total_tags,
        'total_rules': total_rules,
        'class_specific_rules': class_specific,
        'generic_rules': generic,
        'docbook_targets': len(docbook_targets),
        'docbook_target_list': sorted(docbook_targets)
    }


# ============================================================================
# XSL REQUIREMENT CONVENIENCE FUNCTIONS
# ============================================================================

def check_xsl_id_requirements(root: etree._Element) -> List[Dict[str, Any]]:
    """
    Check document for XSL ID requirements.

    Returns list of elements missing required IDs.
    """
    builder = get_builder()
    return builder.validate_xsl_ids(root)


def fix_missing_xsl_ids(root: etree._Element,
                        id_generator: Optional[Callable[[str, int], str]] = None) -> int:
    """
    Fix all missing XSL-required IDs in document.

    Returns number of IDs added.
    """
    builder = get_builder()
    return builder.fix_xsl_ids(root, id_generator)


def get_xsl_requirements_summary() -> Dict[str, Any]:
    """Get summary of XSL requirements configuration."""
    return {
        'elements_require_id': sorted(XSL_ELEMENTS_REQUIRE_ID),
        'elements_id_ignored': sorted(XSL_ELEMENTS_ID_IGNORED),
        'link_prefixes': dict(XSL_LINKEND_PREFIXES),
        'ritt_elements': sorted(RITT_CUSTOM_ELEMENTS),
        'ritt_risinfo_containers': sorted(RITT_RISINFO_CONTAINERS),
    }


# ============================================================================
# RITT ELEMENT CONVENIENCE FUNCTIONS
# ============================================================================

def create_risindex(parent: etree._Element,
                    term: str, topic: str, type_: str,
                    rule: str, posid: str, **attrs) -> etree._Element:
    """
    Create a complete risindex element with required children.

    Args:
        parent: Parent element
        term: risterm text
        topic: ristopic text
        type_: ristype text
        rule: risrule text
        posid: risposid text
        **attrs: Additional attributes for risindex
    """
    risindex = etree.SubElement(parent, 'risindex', **attrs)
    etree.SubElement(risindex, 'risterm').text = term
    etree.SubElement(risindex, 'ristopic').text = topic
    etree.SubElement(risindex, 'ristype').text = type_
    etree.SubElement(risindex, 'risrule').text = rule
    etree.SubElement(risindex, 'risposid').text = posid
    return risindex


def create_risinfo(parent: etree._Element, **kwargs) -> etree._Element:
    """
    Create a risinfo element with optional metadata.

    Args:
        parent: Parent element (should be sect1info, chapterinfo, or bookinfo)
        **kwargs: Metadata values (booktitle, chapternumber, etc.)
    """
    risinfo = etree.SubElement(parent, 'risinfo')

    for key, value in kwargs.items():
        if key in RITT_CONTENT_MODELS.get('risinfo', {}).get('valid_children', set()):
            child = etree.SubElement(risinfo, key)
            if isinstance(value, str):
                child.text = value

    return risinfo


def validate_ritt_document(root: etree._Element) -> List[ContentModelViolation]:
    """
    Validate all Ritt elements in document.

    Returns list of violations found.
    """
    builder = get_builder()
    violations = []

    for elem in root.iter():
        if isinstance(elem.tag, str) and is_ritt_custom_element(elem.tag):
            elem_violations = builder.finalize_ritt_element(elem)
            violations.extend(elem_violations)

    return violations
