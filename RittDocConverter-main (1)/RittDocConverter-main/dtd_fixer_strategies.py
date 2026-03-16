"""
DTD Fixer Strategy Pattern Module

Provides a strategy pattern implementation for DTD fixing operations.
Each fix type is encapsulated in its own strategy class, making it easy
to add, remove, or modify individual fixes.

Strategy Categories:
1. ID Fixes - Duplicate IDs, missing IDs, non-compliant IDs
2. Structure Fixes - Nested elements, content order, missing required elements
3. Content Fixes - Empty elements, missing content, invalid content
4. Link Fixes - Missing linkends, broken references
5. Element-Specific Fixes - Figure, table, footnote, bibliography, etc.

Usage:
    from dtd_fixer_strategies import (
        FixerStrategy, StrategyRegistry, run_fix_strategies
    )

    # Register custom strategy
    class MyFix(FixerStrategy):
        name = "my-fix"
        priority = 50

        def fix(self, root, filename, context):
            fixes = []
            # fix logic
            return fixes

    registry = StrategyRegistry()
    registry.register(MyFix())

    # Run all strategies
    all_fixes = run_fix_strategies(root, filename, registry)
"""

import logging
from abc import ABC, abstractmethod
from dataclasses import dataclass, field
from typing import Any, Callable, Dict, List, Optional, Set, Type

from lxml import etree

logger = logging.getLogger(__name__)


# =============================================================================
# Data Classes
# =============================================================================

@dataclass
class FixContext:
    """Context passed to fix strategies."""
    filename: str
    chapter_id: Optional[str] = None
    all_ids: Set[str] = field(default_factory=set)
    id_mapping: Dict[str, str] = field(default_factory=dict)
    dropped_ids: Set[str] = field(default_factory=set)
    config: Dict[str, Any] = field(default_factory=dict)
    shared_state: Dict[str, Any] = field(default_factory=dict)

    def get_next_duplicate_suffix(self, base_id: str) -> str:
        """Get next available suffix for a duplicate ID."""
        suffix = 2
        while f"{base_id}_{suffix}" in self.all_ids:
            suffix += 1
        return f"{base_id}_{suffix}"


@dataclass
class FixResult:
    """Result from a single fix operation."""
    strategy_name: str
    description: str
    element_tag: str
    element_id: Optional[str] = None
    line_number: Optional[int] = None
    old_value: Optional[str] = None
    new_value: Optional[str] = None


# =============================================================================
# Base Strategy Class
# =============================================================================

class FixerStrategy(ABC):
    """
    Base class for DTD fixer strategies.

    Subclasses implement the `fix` method to apply specific fixes.
    Strategies are run in priority order (lower = first).
    """

    # Strategy metadata
    name: str = "base-strategy"
    description: str = ""
    priority: int = 100  # Lower runs first
    category: str = "general"

    # Control flags
    enabled: bool = True
    run_multiple_times: bool = False  # If True, can be re-run in multi-pass

    @abstractmethod
    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        """
        Apply fixes to the XML tree.

        Args:
            root: XML root element (modified in place)
            filename: Source filename for error messages
            context: Fix context with IDs and configuration

        Returns:
            List of FixResult describing changes made
        """
        pass

    def should_run(self, context: FixContext) -> bool:
        """Check if this strategy should run given the context."""
        return self.enabled

    def __str__(self) -> str:
        return f"{self.name} (priority={self.priority})"


# =============================================================================
# Strategy Registry
# =============================================================================

class StrategyRegistry:
    """
    Registry for fix strategies.

    Manages strategy registration, ordering, and execution.
    """

    def __init__(self):
        self._strategies: Dict[str, FixerStrategy] = {}
        self._category_map: Dict[str, List[str]] = {}

    def register(self, strategy: FixerStrategy) -> None:
        """Register a strategy."""
        self._strategies[strategy.name] = strategy

        # Track by category
        if strategy.category not in self._category_map:
            self._category_map[strategy.category] = []
        if strategy.name not in self._category_map[strategy.category]:
            self._category_map[strategy.category].append(strategy.name)

        logger.debug(f"Registered fixer strategy: {strategy.name}")

    def unregister(self, name: str) -> bool:
        """Unregister a strategy by name."""
        if name in self._strategies:
            strategy = self._strategies.pop(name)
            if strategy.category in self._category_map:
                self._category_map[strategy.category].remove(name)
            return True
        return False

    def get(self, name: str) -> Optional[FixerStrategy]:
        """Get a strategy by name."""
        return self._strategies.get(name)

    def get_by_category(self, category: str) -> List[FixerStrategy]:
        """Get all strategies in a category."""
        names = self._category_map.get(category, [])
        return [self._strategies[n] for n in names if n in self._strategies]

    def get_all_sorted(self) -> List[FixerStrategy]:
        """Get all strategies sorted by priority."""
        return sorted(
            self._strategies.values(),
            key=lambda s: s.priority
        )

    def get_enabled(self) -> List[FixerStrategy]:
        """Get all enabled strategies sorted by priority."""
        return sorted(
            [s for s in self._strategies.values() if s.enabled],
            key=lambda s: s.priority
        )

    @property
    def categories(self) -> List[str]:
        """Get all categories."""
        return list(self._category_map.keys())

    def __len__(self) -> int:
        return len(self._strategies)


# =============================================================================
# Strategy Execution
# =============================================================================

def run_fix_strategies(
    root: etree._Element,
    filename: str,
    registry: StrategyRegistry,
    context: Optional[FixContext] = None,
    categories: Optional[List[str]] = None,
    exclude: Optional[List[str]] = None
) -> List[FixResult]:
    """
    Run fix strategies on an XML tree.

    Args:
        root: XML root element (modified in place)
        filename: Source filename
        registry: Strategy registry
        context: Optional fix context
        categories: Only run strategies in these categories
        exclude: Exclude these strategy names

    Returns:
        List of all FixResults from all strategies
    """
    if context is None:
        context = FixContext(filename=filename)

    exclude = set(exclude or [])
    all_results = []

    strategies = registry.get_enabled()

    for strategy in strategies:
        # Check exclusions
        if strategy.name in exclude:
            continue

        # Check category filter
        if categories and strategy.category not in categories:
            continue

        # Check if strategy should run
        if not strategy.should_run(context):
            continue

        try:
            results = strategy.fix(root, filename, context)
            all_results.extend(results)

            if results:
                logger.debug(
                    f"Strategy {strategy.name} applied {len(results)} fixes"
                )
        except Exception as e:
            logger.error(f"Strategy {strategy.name} failed: {e}", exc_info=True)

    return all_results


# =============================================================================
# Built-in Strategies
# =============================================================================

class DuplicateIdFix(FixerStrategy):
    """Fix duplicate ID attributes by appending numeric suffixes."""

    name = "duplicate-id-fix"
    description = "Resolves duplicate ID attributes"
    priority = 10
    category = "id"

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []
        seen_ids: Dict[str, int] = {}

        for elem in root.iter():
            elem_id = elem.get('id')
            if not elem_id:
                continue

            if elem_id in seen_ids:
                # Duplicate found - rename
                seen_ids[elem_id] += 1
                new_id = f"{elem_id}_{seen_ids[elem_id]}"

                # Ensure new ID is unique
                while new_id in seen_ids or new_id in context.all_ids:
                    seen_ids[elem_id] += 1
                    new_id = f"{elem_id}_{seen_ids[elem_id]}"

                elem.set('id', new_id)
                context.all_ids.add(new_id)
                context.id_mapping[elem_id] = new_id

                results.append(FixResult(
                    strategy_name=self.name,
                    description=f"Renamed duplicate ID '{elem_id}' to '{new_id}'",
                    element_tag=elem.tag,
                    element_id=new_id,
                    old_value=elem_id,
                    new_value=new_id
                ))
            else:
                seen_ids[elem_id] = 1
                context.all_ids.add(elem_id)

        return results


class EmptyElementFix(FixerStrategy):
    """Remove or fix empty elements that should have content."""

    name = "empty-element-fix"
    description = "Handles empty elements"
    priority = 50
    category = "content"

    # Elements that must have content
    MUST_HAVE_CONTENT = {
        'para', 'title', 'chapter', 'sect1', 'sect2', 'sect3',
        'listitem', 'entry', 'bibliomixed', 'glossdef'
    }

    # Elements that can be empty
    CAN_BE_EMPTY = {
        'anchor', 'xref', 'imagedata', 'colspec', 'beginpage'
    }

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []
        to_remove = []

        for elem in root.iter():
            # Skip non-element nodes (comments, processing instructions) where tag is not a string
            if not isinstance(elem.tag, str):
                continue
            tag = elem.tag.split('}')[-1] if '}' in elem.tag else elem.tag

            # Skip elements that can be empty
            if tag in self.CAN_BE_EMPTY:
                continue

            # Check if element has content
            has_text = bool(elem.text and elem.text.strip())
            has_children = len(elem) > 0
            has_tail = bool(elem.tail and elem.tail.strip())

            if not has_text and not has_children:
                if tag in self.MUST_HAVE_CONTENT:
                    # For structural elements, add placeholder
                    if tag == 'para':
                        elem.text = ' '  # Non-breaking space
                        results.append(FixResult(
                            strategy_name=self.name,
                            description=f"Added placeholder to empty <{tag}>",
                            element_tag=tag,
                            element_id=elem.get('id')
                        ))
                    elif tag == 'title':
                        # Leave empty — XSL handles display of empty titles.
                        # Do NOT insert visible placeholder text like '[Title]'
                        # which would render as stray text in the final output.
                        elem.text = ''
                    elif tag == 'listitem':
                        # Add empty para
                        para = etree.SubElement(elem, 'para')
                        para.text = ' '
                        results.append(FixResult(
                            strategy_name=self.name,
                            description=f"Added para to empty <{tag}>",
                            element_tag=tag,
                            element_id=elem.get('id')
                        ))

        return results


class NestedParaFix(FixerStrategy):
    """Fix nested para elements by unwrapping inner paras."""

    name = "nested-para-fix"
    description = "Unwraps nested para elements"
    priority = 30
    category = "structure"

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []

        # Find all nested paras (para inside para)
        for outer_para in root.iter('para'):
            inner_paras = list(outer_para.findall('.//para'))

            for inner_para in inner_paras:
                # Only process direct children
                if inner_para.getparent() != outer_para:
                    continue

                # Get position in parent
                parent = inner_para.getparent()
                index = list(parent).index(inner_para)

                # Move inner para's content to parent
                if inner_para.text:
                    if index > 0:
                        prev = parent[index - 1]
                        prev.tail = (prev.tail or '') + inner_para.text
                    else:
                        parent.text = (parent.text or '') + inner_para.text

                # Move children
                for child in list(inner_para):
                    inner_para.remove(child)
                    parent.insert(index, child)
                    index += 1

                # Handle tail
                if inner_para.tail:
                    if index > 0:
                        prev = parent[index - 1]
                        prev.tail = (prev.tail or '') + inner_para.tail
                    else:
                        parent.text = (parent.text or '') + inner_para.tail

                # Remove inner para
                parent.remove(inner_para)

                results.append(FixResult(
                    strategy_name=self.name,
                    description=f"Unwrapped nested para",
                    element_tag='para',
                    element_id=inner_para.get('id')
                ))

        return results


class AnchorBeforeTitleFix(FixerStrategy):
    """Move anchors that appear before titles to appropriate locations.

    For sections/chapters: Move anchor after the title (anchors allowed inside).
    For figures/tables: Move anchor BEFORE the element (DTD doesn't allow anchor inside).
    """

    name = "anchor-before-title-fix"
    description = "Moves anchors from before title to valid DTD location"
    priority = 25
    category = "structure"

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []

        # Elements that have title as first required child and ALLOW anchors inside
        ANCHOR_ALLOWED_ELEMENTS = {
            'chapter', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
            'appendix', 'preface', 'example'
        }

        # Elements that have title but DO NOT allow anchors inside
        # (DTD only allows specific content after title)
        ANCHOR_FORBIDDEN_ELEMENTS = {
            'figure', 'informalfigure', 'table', 'informaltable'
        }

        ALL_TITLED_ELEMENTS = ANCHOR_ALLOWED_ELEMENTS | ANCHOR_FORBIDDEN_ELEMENTS

        for parent in root.iter():
            # Skip non-element nodes (comments, processing instructions) where tag is not a string
            if not isinstance(parent.tag, str):
                continue
            tag = parent.tag.split('}')[-1] if '}' in parent.tag else parent.tag
            if tag not in ALL_TITLED_ELEMENTS:
                continue

            children = list(parent)
            if len(children) < 2:
                continue

            # Check if first child is anchor and second is title
            first = children[0]
            first_tag = first.tag.split('}')[-1] if '}' in first.tag else first.tag

            if first_tag != 'anchor':
                continue

            # Find title
            title_idx = None
            for i, child in enumerate(children[1:], 1):
                child_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if child_tag == 'title':
                    title_idx = i
                    break

            if title_idx is None:
                continue

            anchor = children[0]
            parent.remove(anchor)

            if tag in ANCHOR_FORBIDDEN_ELEMENTS:
                # Move anchor to BEFORE the figure/table element
                grandparent = parent.getparent()
                if grandparent is not None:
                    parent_index = list(grandparent).index(parent)
                    grandparent.insert(parent_index, anchor)
                    results.append(FixResult(
                        strategy_name=self.name,
                        description=f"Moved anchor to before <{tag}> (DTD forbids anchor inside {tag})",
                        element_tag='anchor',
                        element_id=anchor.get('id')
                    ))
                else:
                    # No grandparent, put anchor back after title as best effort
                    parent.insert(title_idx, anchor)
                    results.append(FixResult(
                        strategy_name=self.name,
                        description=f"Moved anchor after title in <{tag}> (no parent to move to)",
                        element_tag='anchor',
                        element_id=anchor.get('id')
                    ))
            else:
                # Move anchor after title (standard behavior for sections)
                parent.insert(title_idx, anchor)  # After title (title moved up)
                results.append(FixResult(
                    strategy_name=self.name,
                    description=f"Moved anchor after title in <{tag}>",
                    element_tag='anchor',
                    element_id=anchor.get('id')
                ))

        return results


class AnchorInFigureTableFix(FixerStrategy):
    """Move anchors that are incorrectly inside figure/table elements to before the element.

    The DocBook DTD does not allow anchor elements inside figure/table.
    This strategy finds any anchors inside these elements and moves them to be siblings
    immediately before the figure/table.
    """

    name = "anchor-in-figure-table-fix"
    description = "Moves anchors out of figure/table elements (DTD violation)"
    priority = 24  # Run after anchor-before-title-fix (25)
    category = "structure"

    # Elements that DO NOT allow anchors inside (DTD restriction)
    ANCHOR_FORBIDDEN_ELEMENTS = {
        'figure', 'informalfigure', 'table', 'informaltable'
    }

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []

        for parent in root.iter():
            # Skip non-element nodes
            if not isinstance(parent.tag, str):
                continue
            tag = parent.tag.split('}')[-1] if '}' in parent.tag else parent.tag
            if tag not in self.ANCHOR_FORBIDDEN_ELEMENTS:
                continue

            # Find all anchor elements that are direct children
            anchors_to_move = []
            for child in list(parent):
                if not isinstance(child.tag, str):
                    continue
                child_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if child_tag == 'anchor':
                    anchors_to_move.append(child)

            if not anchors_to_move:
                continue

            # Get grandparent to insert anchors before the figure/table
            grandparent = parent.getparent()
            if grandparent is None:
                continue

            parent_index = list(grandparent).index(parent)

            # Move anchors (in reverse order to maintain their relative order)
            for anchor in reversed(anchors_to_move):
                parent.remove(anchor)
                grandparent.insert(parent_index, anchor)
                results.append(FixResult(
                    strategy_name=self.name,
                    description=f"Moved anchor from inside <{tag}> to before it (DTD violation fix)",
                    element_tag='anchor',
                    element_id=anchor.get('id')
                ))

        return results


class MissingTitleFix(FixerStrategy):
    """Add missing title elements to elements that require them."""

    name = "missing-title-fix"
    description = "Adds missing title elements"
    priority = 20
    category = "structure"

    # Elements that require title
    REQUIRES_TITLE = {
        'chapter', 'sect1', 'sect2', 'sect3', 'sect4', 'sect5',
        'appendix', 'preface', 'figure', 'table', 'example',
        'sidebar', 'note', 'warning', 'tip', 'caution', 'important'
    }

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []

        for elem in root.iter():
            # Skip non-element nodes (comments, processing instructions) where tag is not a string
            if not isinstance(elem.tag, str):
                continue
            tag = elem.tag.split('}')[-1] if '}' in elem.tag else elem.tag
            if tag not in self.REQUIRES_TITLE:
                continue

            # Check if has title
            has_title = False
            for child in elem:
                # Skip non-element children
                if not isinstance(child.tag, str):
                    continue
                child_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if child_tag == 'title':
                    has_title = True
                    break

            if not has_title:
                # Add title as first element (or after beginpage/chapterinfo)
                title = etree.Element('title')
                title.text = f'[{tag.title()}]'

                insert_idx = 0
                for i, child in enumerate(elem):
                    child_tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                    if child_tag in ('beginpage', 'chapterinfo', 'sectioninfo'):
                        insert_idx = i + 1
                    else:
                        break

                elem.insert(insert_idx, title)

                results.append(FixResult(
                    strategy_name=self.name,
                    description=f"Added missing title to <{tag}>",
                    element_tag=tag,
                    element_id=elem.get('id')
                ))

        return results


class LinkMissingLinkendFix(FixerStrategy):
    """Fix link elements missing linkend attribute."""

    name = "link-missing-linkend-fix"
    description = "Converts links without linkend to phrases or ulinks"
    priority = 60
    category = "link"

    def fix(
        self,
        root: etree._Element,
        filename: str,
        context: FixContext
    ) -> List[FixResult]:
        results = []

        for link in root.iter('link'):
            linkend = link.get('linkend')

            if not linkend:
                # Convert to phrase or ulink
                href = link.get('{http://www.w3.org/1999/xlink}href')

                if href:
                    # Has href - convert to ulink
                    link.tag = 'ulink'
                    link.set('url', href)
                    # Remove xlink:href
                    for attr in list(link.attrib):
                        if 'xlink' in attr:
                            del link.attrib[attr]

                    results.append(FixResult(
                        strategy_name=self.name,
                        description=f"Converted link to ulink",
                        element_tag='link',
                        element_id=link.get('id'),
                        new_value=href
                    ))
                else:
                    # No href - convert to phrase
                    link.tag = 'phrase'

                    results.append(FixResult(
                        strategy_name=self.name,
                        description=f"Converted link without linkend to phrase",
                        element_tag='link',
                        element_id=link.get('id')
                    ))

        return results


# =============================================================================
# Global Registry
# =============================================================================

_global_registry: Optional[StrategyRegistry] = None


def get_global_registry() -> StrategyRegistry:
    """Get the global strategy registry."""
    global _global_registry
    if _global_registry is None:
        _global_registry = StrategyRegistry()
        register_builtin_strategies(_global_registry)
    return _global_registry


def register_builtin_strategies(registry: StrategyRegistry) -> None:
    """Register all built-in fixer strategies."""
    strategies = [
        DuplicateIdFix(),
        EmptyElementFix(),
        NestedParaFix(),
        AnchorBeforeTitleFix(),
        AnchorInFigureTableFix(),
        MissingTitleFix(),
        LinkMissingLinkendFix(),
    ]

    for strategy in strategies:
        registry.register(strategy)

    logger.info(f"Registered {len(strategies)} built-in fixer strategies")


def register_strategy(strategy: FixerStrategy) -> None:
    """Register a strategy with the global registry."""
    get_global_registry().register(strategy)


def create_strategy_from_function(
    name: str,
    fix_func: Callable[[etree._Element, str, FixContext], List[FixResult]],
    priority: int = 100,
    category: str = "custom",
    description: str = ""
) -> FixerStrategy:
    """
    Create a strategy from a simple function.

    Args:
        name: Strategy name
        fix_func: Function implementing the fix
        priority: Strategy priority
        category: Strategy category
        description: Strategy description

    Returns:
        FixerStrategy instance
    """
    class FunctionStrategy(FixerStrategy):
        pass

    FunctionStrategy.name = name
    FunctionStrategy.priority = priority
    FunctionStrategy.category = category
    FunctionStrategy.description = description

    def fix(self, root, filename, context):
        return fix_func(root, filename, context)

    FunctionStrategy.fix = fix

    return FunctionStrategy()
