"""
Plugin Architecture for RittDocConverter

Provides a flexible plugin system for validators, fixers, and post-processors.
Plugins can be registered programmatically or auto-discovered from directories.

Plugin Types:
1. ValidatorPlugin: Validate XML against custom rules
2. FixerPlugin: Apply fixes to XML content
3. PostProcessorPlugin: Post-process converted content

Usage:
    from plugin_architecture import (
        PluginManager, ValidatorPlugin, FixerPlugin, PostProcessorPlugin
    )

    # Register a validator
    class MyValidator(ValidatorPlugin):
        name = "my-validator"

        def validate(self, tree, context):
            errors = []
            # validation logic
            return ValidationResult(errors=errors)

    # Use the manager
    manager = get_plugin_manager()
    manager.register(MyValidator())

    # Run all validators
    results = manager.run_validators(xml_tree, context)
"""

import importlib
import importlib.util
import logging
import sys
from abc import ABC, abstractmethod
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Any, Callable, Dict, List, Optional, Set, Type, TypeVar

from lxml import etree

logger = logging.getLogger(__name__)


# =============================================================================
# Data Classes
# =============================================================================

@dataclass
class ValidationError:
    """A single validation error."""
    file_path: str
    line_number: Optional[int]
    element_tag: str
    element_id: Optional[str]
    error_type: str
    message: str
    severity: str = "error"  # 'error', 'warning', 'info'
    suggestion: Optional[str] = None

    def __str__(self) -> str:
        loc = f"{self.file_path}"
        if self.line_number:
            loc += f":{self.line_number}"
        return f"[{self.severity.upper()}] {loc} - {self.error_type}: {self.message}"


@dataclass
class ValidationResult:
    """Result from a validator plugin."""
    plugin_name: str = ""
    errors: List[ValidationError] = field(default_factory=list)
    warnings: List[ValidationError] = field(default_factory=list)
    info: List[ValidationError] = field(default_factory=list)
    metadata: Dict[str, Any] = field(default_factory=dict)
    execution_time_ms: float = 0.0

    @property
    def has_errors(self) -> bool:
        return len(self.errors) > 0

    @property
    def total_issues(self) -> int:
        return len(self.errors) + len(self.warnings) + len(self.info)

    def add_error(self, **kwargs) -> None:
        """Add an error."""
        kwargs.setdefault('severity', 'error')
        self.errors.append(ValidationError(**kwargs))

    def add_warning(self, **kwargs) -> None:
        """Add a warning."""
        kwargs.setdefault('severity', 'warning')
        self.warnings.append(ValidationError(**kwargs))


@dataclass
class FixResult:
    """Result from a fixer plugin."""
    plugin_name: str = ""
    fixes_applied: List[str] = field(default_factory=list)
    elements_modified: int = 0
    files_modified: Set[str] = field(default_factory=set)
    metadata: Dict[str, Any] = field(default_factory=dict)
    execution_time_ms: float = 0.0

    @property
    def total_fixes(self) -> int:
        return len(self.fixes_applied)

    def add_fix(self, description: str, file_path: Optional[str] = None) -> None:
        """Record a fix."""
        self.fixes_applied.append(description)
        if file_path:
            self.files_modified.add(file_path)


@dataclass
class PostProcessResult:
    """Result from a post-processor plugin."""
    plugin_name: str = ""
    changes: List[str] = field(default_factory=list)
    files_modified: Set[str] = field(default_factory=set)
    metadata: Dict[str, Any] = field(default_factory=dict)
    execution_time_ms: float = 0.0


@dataclass
class PluginContext:
    """Context passed to plugins during execution."""
    file_path: Optional[Path] = None
    chapter_id: Optional[str] = None
    book_title: Optional[str] = None
    isbn: Optional[str] = None
    publisher: Optional[str] = None
    temp_dir: Optional[Path] = None
    output_dir: Optional[Path] = None
    config: Dict[str, Any] = field(default_factory=dict)
    shared_data: Dict[str, Any] = field(default_factory=dict)


# =============================================================================
# Base Plugin Classes
# =============================================================================

class BasePlugin(ABC):
    """Base class for all plugins."""

    # Plugin metadata (override in subclasses)
    name: str = "base-plugin"
    version: str = "1.0.0"
    description: str = ""
    author: str = ""
    priority: int = 100  # Lower = runs first

    # Lifecycle hooks
    def on_register(self, manager: 'PluginManager') -> None:
        """Called when plugin is registered with manager."""
        pass

    def on_unregister(self, manager: 'PluginManager') -> None:
        """Called when plugin is unregistered."""
        pass

    def on_conversion_start(self, context: PluginContext) -> None:
        """Called at the start of a conversion."""
        pass

    def on_conversion_end(self, context: PluginContext) -> None:
        """Called at the end of a conversion."""
        pass

    @property
    def plugin_id(self) -> str:
        """Unique identifier for this plugin."""
        return f"{self.name}@{self.version}"


class ValidatorPlugin(BasePlugin):
    """
    Base class for validation plugins.

    Validators examine XML content and report errors/warnings.
    They should NOT modify the content.
    """

    # When to run this validator
    run_on_chapters: bool = True
    run_on_book_xml: bool = False
    run_on_toc: bool = False

    @abstractmethod
    def validate(self, tree: etree._ElementTree, context: PluginContext) -> ValidationResult:
        """
        Validate an XML tree.

        Args:
            tree: Parsed XML tree (lxml)
            context: Plugin context with file info

        Returns:
            ValidationResult with any errors/warnings found
        """
        pass


class FixerPlugin(BasePlugin):
    """
    Base class for fixer plugins.

    Fixers examine and modify XML content to fix issues.
    They run after validators identify problems.
    """

    # When to run this fixer
    run_on_chapters: bool = True
    run_on_book_xml: bool = False

    # If True, the tree is modified in-place and must be saved
    modifies_tree: bool = True

    @abstractmethod
    def fix(self, tree: etree._ElementTree, context: PluginContext) -> FixResult:
        """
        Apply fixes to an XML tree.

        Args:
            tree: Parsed XML tree (lxml) - modified in-place
            context: Plugin context with file info

        Returns:
            FixResult with list of fixes applied
        """
        pass


class PostProcessorPlugin(BasePlugin):
    """
    Base class for post-processor plugins.

    Post-processors run after all chapters are converted and packaged.
    They can modify the final ZIP or perform cleanup tasks.
    """

    @abstractmethod
    def process(self, zip_path: Path, context: PluginContext) -> PostProcessResult:
        """
        Post-process a conversion result.

        Args:
            zip_path: Path to the output ZIP file
            context: Plugin context

        Returns:
            PostProcessResult with changes made
        """
        pass


# =============================================================================
# Plugin Manager
# =============================================================================

T = TypeVar('T', bound=BasePlugin)


class PluginManager:
    """
    Central manager for all plugins.

    Handles registration, discovery, and execution of plugins.
    """

    def __init__(self):
        self._validators: Dict[str, ValidatorPlugin] = {}
        self._fixers: Dict[str, FixerPlugin] = {}
        self._post_processors: Dict[str, PostProcessorPlugin] = {}
        self._all_plugins: Dict[str, BasePlugin] = {}
        self._plugin_dirs: List[Path] = []

    # =========================================================================
    # Registration
    # =========================================================================

    def register(self, plugin: BasePlugin) -> None:
        """Register a plugin."""
        plugin_id = plugin.plugin_id

        if plugin_id in self._all_plugins:
            logger.warning(f"Plugin {plugin_id} already registered, replacing")

        self._all_plugins[plugin_id] = plugin

        if isinstance(plugin, ValidatorPlugin):
            self._validators[plugin_id] = plugin
            logger.info(f"Registered validator: {plugin_id}")
        elif isinstance(plugin, FixerPlugin):
            self._fixers[plugin_id] = plugin
            logger.info(f"Registered fixer: {plugin_id}")
        elif isinstance(plugin, PostProcessorPlugin):
            self._post_processors[plugin_id] = plugin
            logger.info(f"Registered post-processor: {plugin_id}")

        plugin.on_register(self)

    def unregister(self, plugin_id: str) -> bool:
        """Unregister a plugin by ID."""
        if plugin_id not in self._all_plugins:
            return False

        plugin = self._all_plugins[plugin_id]
        plugin.on_unregister(self)

        del self._all_plugins[plugin_id]
        self._validators.pop(plugin_id, None)
        self._fixers.pop(plugin_id, None)
        self._post_processors.pop(plugin_id, None)

        logger.info(f"Unregistered plugin: {plugin_id}")
        return True

    def register_class(self, plugin_class: Type[BasePlugin]) -> None:
        """Register a plugin class (instantiates it)."""
        plugin = plugin_class()
        self.register(plugin)

    # =========================================================================
    # Plugin Discovery
    # =========================================================================

    def add_plugin_directory(self, directory: Path) -> None:
        """Add a directory to search for plugins."""
        if directory.exists() and directory.is_dir():
            self._plugin_dirs.append(directory)
            logger.info(f"Added plugin directory: {directory}")

    def discover_plugins(self) -> int:
        """
        Discover and load plugins from registered directories.

        Plugin files must:
        - End with '_plugin.py' or be in a 'plugins' subdirectory
        - Define classes that inherit from BasePlugin
        - Have 'name' and 'version' class attributes

        Returns:
            Number of plugins discovered
        """
        discovered = 0

        for plugin_dir in self._plugin_dirs:
            # Look for *_plugin.py files
            for plugin_file in plugin_dir.glob("*_plugin.py"):
                try:
                    count = self._load_plugins_from_file(plugin_file)
                    discovered += count
                except Exception as e:
                    logger.error(f"Failed to load plugin from {plugin_file}: {e}")

            # Look in plugins/ subdirectory
            plugins_subdir = plugin_dir / "plugins"
            if plugins_subdir.exists():
                for plugin_file in plugins_subdir.glob("*.py"):
                    if plugin_file.name.startswith("_"):
                        continue
                    try:
                        count = self._load_plugins_from_file(plugin_file)
                        discovered += count
                    except Exception as e:
                        logger.error(f"Failed to load plugin from {plugin_file}: {e}")

        logger.info(f"Discovered {discovered} plugins from {len(self._plugin_dirs)} directories")
        return discovered

    def _load_plugins_from_file(self, file_path: Path) -> int:
        """Load all plugin classes from a Python file."""
        spec = importlib.util.spec_from_file_location(
            f"plugin_{file_path.stem}",
            file_path
        )
        if spec is None or spec.loader is None:
            return 0

        module = importlib.util.module_from_spec(spec)
        sys.modules[spec.name] = module
        spec.loader.exec_module(module)

        count = 0
        for name in dir(module):
            obj = getattr(module, name)
            if (
                isinstance(obj, type)
                and issubclass(obj, BasePlugin)
                and obj not in (BasePlugin, ValidatorPlugin, FixerPlugin, PostProcessorPlugin)
                and hasattr(obj, 'name')
                and obj.name != "base-plugin"
            ):
                try:
                    self.register_class(obj)
                    count += 1
                except Exception as e:
                    logger.error(f"Failed to register plugin {name}: {e}")

        return count

    # =========================================================================
    # Plugin Execution
    # =========================================================================

    def run_validators(
        self,
        tree: etree._ElementTree,
        context: PluginContext,
        include: Optional[List[str]] = None,
        exclude: Optional[List[str]] = None
    ) -> List[ValidationResult]:
        """
        Run all registered validators on an XML tree.

        Args:
            tree: XML tree to validate
            context: Plugin context
            include: Only run these validators (by name)
            exclude: Skip these validators (by name)

        Returns:
            List of ValidationResult from each validator
        """
        results = []
        validators = self._get_sorted_plugins(
            self._validators,
            include,
            exclude
        )

        for plugin_id, validator in validators:
            try:
                start_time = datetime.now()
                result = validator.validate(tree, context)
                result.plugin_name = validator.name
                result.execution_time_ms = (datetime.now() - start_time).total_seconds() * 1000
                results.append(result)

                if result.has_errors:
                    logger.debug(
                        f"Validator {validator.name} found {len(result.errors)} errors"
                    )
            except Exception as e:
                logger.error(f"Validator {validator.name} failed: {e}", exc_info=True)
                # Create error result
                error_result = ValidationResult(plugin_name=validator.name)
                error_result.add_error(
                    file_path=str(context.file_path or "unknown"),
                    line_number=None,
                    element_tag="",
                    element_id=None,
                    error_type="PluginError",
                    message=f"Validator crashed: {e}"
                )
                results.append(error_result)

        return results

    def run_fixers(
        self,
        tree: etree._ElementTree,
        context: PluginContext,
        include: Optional[List[str]] = None,
        exclude: Optional[List[str]] = None
    ) -> List[FixResult]:
        """
        Run all registered fixers on an XML tree.

        Args:
            tree: XML tree to fix (modified in-place)
            context: Plugin context
            include: Only run these fixers (by name)
            exclude: Skip these fixers (by name)

        Returns:
            List of FixResult from each fixer
        """
        results = []
        fixers = self._get_sorted_plugins(self._fixers, include, exclude)

        for plugin_id, fixer in fixers:
            try:
                start_time = datetime.now()
                result = fixer.fix(tree, context)
                result.plugin_name = fixer.name
                result.execution_time_ms = (datetime.now() - start_time).total_seconds() * 1000
                results.append(result)

                if result.total_fixes > 0:
                    logger.debug(
                        f"Fixer {fixer.name} applied {result.total_fixes} fixes"
                    )
            except Exception as e:
                logger.error(f"Fixer {fixer.name} failed: {e}", exc_info=True)
                # Create error result
                error_result = FixResult(plugin_name=fixer.name)
                error_result.metadata['error'] = str(e)
                results.append(error_result)

        return results

    def run_post_processors(
        self,
        zip_path: Path,
        context: PluginContext,
        include: Optional[List[str]] = None,
        exclude: Optional[List[str]] = None
    ) -> List[PostProcessResult]:
        """
        Run all registered post-processors on a ZIP file.

        Args:
            zip_path: Path to output ZIP
            context: Plugin context
            include: Only run these post-processors (by name)
            exclude: Skip these post-processors (by name)

        Returns:
            List of PostProcessResult from each post-processor
        """
        results = []
        processors = self._get_sorted_plugins(
            self._post_processors,
            include,
            exclude
        )

        for plugin_id, processor in processors:
            try:
                start_time = datetime.now()
                result = processor.process(zip_path, context)
                result.plugin_name = processor.name
                result.execution_time_ms = (datetime.now() - start_time).total_seconds() * 1000
                results.append(result)

                if result.changes:
                    logger.debug(
                        f"Post-processor {processor.name} made {len(result.changes)} changes"
                    )
            except Exception as e:
                logger.error(f"Post-processor {processor.name} failed: {e}", exc_info=True)
                error_result = PostProcessResult(plugin_name=processor.name)
                error_result.metadata['error'] = str(e)
                results.append(error_result)

        return results

    def _get_sorted_plugins(
        self,
        plugins: Dict[str, T],
        include: Optional[List[str]],
        exclude: Optional[List[str]]
    ) -> List[tuple]:
        """Get plugins sorted by priority, with filtering."""
        filtered = []

        for plugin_id, plugin in plugins.items():
            # Check include filter
            if include is not None and plugin.name not in include:
                continue

            # Check exclude filter
            if exclude is not None and plugin.name in exclude:
                continue

            filtered.append((plugin_id, plugin))

        # Sort by priority (lower = first)
        filtered.sort(key=lambda x: x[1].priority)

        return filtered

    # =========================================================================
    # Lifecycle Events
    # =========================================================================

    def notify_conversion_start(self, context: PluginContext) -> None:
        """Notify all plugins that a conversion is starting."""
        for plugin in self._all_plugins.values():
            try:
                plugin.on_conversion_start(context)
            except Exception as e:
                logger.error(f"Plugin {plugin.name} failed on_conversion_start: {e}")

    def notify_conversion_end(self, context: PluginContext) -> None:
        """Notify all plugins that a conversion has ended."""
        for plugin in self._all_plugins.values():
            try:
                plugin.on_conversion_end(context)
            except Exception as e:
                logger.error(f"Plugin {plugin.name} failed on_conversion_end: {e}")

    # =========================================================================
    # Introspection
    # =========================================================================

    def list_plugins(self) -> Dict[str, List[Dict[str, Any]]]:
        """List all registered plugins by type."""
        return {
            'validators': [
                {
                    'id': p.plugin_id,
                    'name': p.name,
                    'version': p.version,
                    'description': p.description,
                    'priority': p.priority
                }
                for p in self._validators.values()
            ],
            'fixers': [
                {
                    'id': p.plugin_id,
                    'name': p.name,
                    'version': p.version,
                    'description': p.description,
                    'priority': p.priority
                }
                for p in self._fixers.values()
            ],
            'post_processors': [
                {
                    'id': p.plugin_id,
                    'name': p.name,
                    'version': p.version,
                    'description': p.description,
                    'priority': p.priority
                }
                for p in self._post_processors.values()
            ]
        }

    def get_plugin(self, plugin_id: str) -> Optional[BasePlugin]:
        """Get a plugin by ID."""
        return self._all_plugins.get(plugin_id)

    @property
    def validator_count(self) -> int:
        return len(self._validators)

    @property
    def fixer_count(self) -> int:
        return len(self._fixers)

    @property
    def post_processor_count(self) -> int:
        return len(self._post_processors)

    @property
    def total_plugins(self) -> int:
        return len(self._all_plugins)


# =============================================================================
# Global Instance
# =============================================================================

_plugin_manager: Optional[PluginManager] = None


def get_plugin_manager() -> PluginManager:
    """Get the global plugin manager instance."""
    global _plugin_manager
    if _plugin_manager is None:
        _plugin_manager = PluginManager()
    return _plugin_manager


def reset_plugin_manager() -> PluginManager:
    """Reset and return a fresh plugin manager."""
    global _plugin_manager
    _plugin_manager = PluginManager()
    return _plugin_manager


# =============================================================================
# Built-in Plugin Examples (can be moved to separate files)
# =============================================================================

class EmptyElementValidator(ValidatorPlugin):
    """Validates that required elements are not empty."""

    name = "empty-element-validator"
    version = "1.0.0"
    description = "Checks for empty elements that should have content"
    priority = 10

    # Elements that must have content
    REQUIRED_CONTENT = {
        'title', 'para', 'chapter', 'sect1', 'sect2', 'sect3',
        'listitem', 'entry', 'bibliomixed'
    }

    def validate(self, tree: etree._ElementTree, context: PluginContext) -> ValidationResult:
        result = ValidationResult()
        root = tree.getroot()

        for elem in root.iter():
            # Skip non-element nodes (comments, processing instructions) where tag is not a string
            if not isinstance(elem.tag, str):
                continue
            tag = elem.tag.split('}')[-1] if '}' in elem.tag else elem.tag

            if tag in self.REQUIRED_CONTENT:
                # Check if element has text content or children
                has_text = bool(elem.text and elem.text.strip())
                has_children = len(elem) > 0

                if not has_text and not has_children:
                    result.add_warning(
                        file_path=str(context.file_path or "unknown"),
                        line_number=elem.sourceline if hasattr(elem, 'sourceline') else None,
                        element_tag=tag,
                        element_id=elem.get('id'),
                        error_type="EmptyElement",
                        message=f"Element <{tag}> is empty but should have content",
                        suggestion=f"Add content to <{tag}> or remove it"
                    )

        return result


class DuplicateIdFixer(FixerPlugin):
    """Fixes duplicate IDs by appending suffixes."""

    name = "duplicate-id-fixer"
    version = "1.0.0"
    description = "Resolves duplicate ID attributes"
    priority = 5

    def fix(self, tree: etree._ElementTree, context: PluginContext) -> FixResult:
        result = FixResult()
        root = tree.getroot()

        seen_ids: Dict[str, int] = {}

        for elem in root.iter():
            elem_id = elem.get('id')
            if elem_id:
                if elem_id in seen_ids:
                    # Duplicate found - rename
                    seen_ids[elem_id] += 1
                    new_id = f"{elem_id}_dup{seen_ids[elem_id]}"
                    elem.set('id', new_id)
                    result.add_fix(
                        f"Renamed duplicate ID '{elem_id}' to '{new_id}'",
                        str(context.file_path)
                    )
                    result.elements_modified += 1
                else:
                    seen_ids[elem_id] = 1

        return result


# Register built-in plugins
def register_builtin_plugins(manager: Optional[PluginManager] = None) -> None:
    """Register the built-in plugins."""
    if manager is None:
        manager = get_plugin_manager()

    manager.register(EmptyElementValidator())
    manager.register(DuplicateIdFixer())
