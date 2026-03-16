#!/usr/bin/env python3
"""
Publisher Configuration Loader
==============================

Loads and manages publisher-specific configurations for EPUB to DocBook conversion.
Configurations control CSS class mappings, epub:type interpretations, and other
publisher-specific conversion rules.

Configuration files are stored in config/publishers/:
- _default.yaml: Default configuration used for all publishers
- {publisher_name}.yaml: Publisher-specific overrides

Usage:
    from publisher_config import get_publisher_config, get_css_mapping, get_epub_type_mapping

    # Load config for a specific publisher
    config = get_publisher_config("Springer")

    # Get DocBook element for a CSS class
    mapping = get_css_mapping("ChapterTitle")
    # Returns: {'element': 'title', 'role': 'chapter-title'}

    # Get DocBook element for epub:type
    element = get_epub_type_mapping("doc-bibliography")
    # Returns: 'bibliography'
"""

import logging
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, List, Optional, Set, Any, Union

# Try to import yaml, fall back to json if not available
try:
    import yaml
    HAS_YAML = True
except ImportError:
    HAS_YAML = False
    import json

logger = logging.getLogger(__name__)

# ============================================================================
# CONFIGURATION DATA STRUCTURES
# ============================================================================

@dataclass
class ElementMapping:
    """Mapping from CSS class or epub:type to DocBook element."""
    element: str
    role: Optional[str] = None
    context: Optional[str] = None  # e.g., 'figure' for caption elements
    skip: bool = False  # If True, element is skipped during conversion


@dataclass
class PublisherConfig:
    """Complete configuration for a publisher."""
    publisher_name: str
    publisher_patterns: List[str] = field(default_factory=list)

    # CSS class mappings: class_name -> ElementMapping
    css_class_mappings: Dict[str, ElementMapping] = field(default_factory=dict)

    # epub:type mappings: type_value -> element_name
    epub_type_mappings: Dict[str, str] = field(default_factory=dict)

    # ID prefix patterns: element_type -> prefix
    id_patterns: Dict[str, str] = field(default_factory=dict)

    # Normalization rules
    normalization: Dict[str, bool] = field(default_factory=dict)

    # Elements/classes to skip
    skip_elements: Set[str] = field(default_factory=set)


# ============================================================================
# GLOBAL STATE
# ============================================================================

_config_cache: Dict[str, PublisherConfig] = {}
_default_config: Optional[PublisherConfig] = None
_current_config: Optional[PublisherConfig] = None

# Default config directory (relative to this file)
CONFIG_DIR = Path(__file__).parent / "config" / "publishers"


# ============================================================================
# CONFIGURATION LOADING
# ============================================================================

def _load_yaml_or_json(file_path: Path) -> Dict[str, Any]:
    """Load configuration from YAML or JSON file."""
    if not file_path.exists():
        return {}

    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    if file_path.suffix in ('.yaml', '.yml'):
        if HAS_YAML:
            return yaml.safe_load(content) or {}
        else:
            logger.warning(f"PyYAML not installed, cannot load {file_path}")
            return {}
    else:
        return json.loads(content) if content.strip() else {}


def _parse_element_mapping(value: Union[str, Dict[str, Any]]) -> ElementMapping:
    """Parse a mapping value into an ElementMapping."""
    if isinstance(value, str):
        return ElementMapping(element=value)
    elif isinstance(value, dict):
        return ElementMapping(
            element=value.get('element', ''),
            role=value.get('role'),
            context=value.get('context'),
            skip=value.get('skip', False)
        )
    else:
        return ElementMapping(element=str(value))


def _load_config_from_dict(data: Dict[str, Any]) -> PublisherConfig:
    """Create a PublisherConfig from a dictionary."""
    config = PublisherConfig(
        publisher_name=data.get('publisher_name', 'Unknown'),
        publisher_patterns=data.get('publisher_patterns', [])
    )

    # Parse CSS class mappings
    css_mappings = data.get('css_class_mappings', {})
    for class_name, mapping_value in css_mappings.items():
        config.css_class_mappings[class_name.lower()] = _parse_element_mapping(mapping_value)

    # Parse epub:type mappings
    epub_mappings = data.get('epub_type_mappings', {})
    for type_name, element in epub_mappings.items():
        config.epub_type_mappings[type_name.lower()] = element

    # Parse ID patterns
    config.id_patterns = data.get('id_patterns', {})

    # Parse normalization rules
    config.normalization = data.get('normalization', {})

    # Parse skip elements
    skip_list = data.get('skip_elements', [])
    config.skip_elements = {s.lower() for s in skip_list}

    return config


def _merge_configs(base: PublisherConfig, override: PublisherConfig) -> PublisherConfig:
    """Merge two configs, with override taking precedence."""
    merged = PublisherConfig(
        publisher_name=override.publisher_name or base.publisher_name,
        publisher_patterns=override.publisher_patterns or base.publisher_patterns
    )

    # Merge CSS mappings (override wins)
    merged.css_class_mappings = {**base.css_class_mappings, **override.css_class_mappings}

    # Merge epub:type mappings (override wins)
    merged.epub_type_mappings = {**base.epub_type_mappings, **override.epub_type_mappings}

    # Merge ID patterns (override wins)
    merged.id_patterns = {**base.id_patterns, **override.id_patterns}

    # Merge normalization (override wins)
    merged.normalization = {**base.normalization, **override.normalization}

    # Merge skip elements (union)
    merged.skip_elements = base.skip_elements | override.skip_elements

    return merged


def load_default_config() -> PublisherConfig:
    """Load the default configuration."""
    global _default_config

    if _default_config is not None:
        return _default_config

    default_path = CONFIG_DIR / "_default.yaml"
    if not default_path.exists():
        default_path = CONFIG_DIR / "_default.json"

    if default_path.exists():
        data = _load_yaml_or_json(default_path)
        _default_config = _load_config_from_dict(data)
        logger.info(f"Loaded default publisher config from {default_path}")
    else:
        logger.warning(f"No default config found at {CONFIG_DIR}/_default.yaml")
        _default_config = PublisherConfig(publisher_name="Default")

    return _default_config


def load_publisher_config(publisher_name: str) -> PublisherConfig:
    """
    Load configuration for a specific publisher.

    Looks for a publisher-specific config file and merges with defaults.

    Args:
        publisher_name: Name of the publisher (e.g., "Springer", "Wiley")

    Returns:
        PublisherConfig with merged settings
    """
    # Normalize publisher name for cache key and filename
    cache_key = publisher_name.lower().strip()

    if cache_key in _config_cache:
        return _config_cache[cache_key]

    # Load default config first
    base_config = load_default_config()

    # Try to find publisher-specific config
    publisher_config = None

    # Try exact match first
    for suffix in ['.yaml', '.yml', '.json']:
        config_path = CONFIG_DIR / f"{cache_key}{suffix}"
        if config_path.exists():
            data = _load_yaml_or_json(config_path)
            publisher_config = _load_config_from_dict(data)
            logger.info(f"Loaded publisher config for '{publisher_name}' from {config_path}")
            break

    # Try pattern matching on existing configs
    if publisher_config is None:
        for config_file in CONFIG_DIR.glob("*.yaml"):
            if config_file.name.startswith('_'):
                continue
            data = _load_yaml_or_json(config_file)
            patterns = data.get('publisher_patterns', [])
            for pattern in patterns:
                if re.search(pattern, publisher_name, re.IGNORECASE):
                    publisher_config = _load_config_from_dict(data)
                    logger.info(f"Matched publisher '{publisher_name}' to config {config_file.name}")
                    break
            if publisher_config:
                break

    # Merge with defaults or use defaults alone
    if publisher_config:
        merged = _merge_configs(base_config, publisher_config)
    else:
        merged = base_config
        logger.debug(f"No specific config for '{publisher_name}', using defaults")

    _config_cache[cache_key] = merged
    return merged


def get_publisher_config(publisher_name: Optional[str] = None) -> PublisherConfig:
    """
    Get the configuration for a publisher.

    If publisher_name is None, returns the currently active config
    or the default config if none is active.

    Args:
        publisher_name: Optional publisher name to load config for

    Returns:
        PublisherConfig for the publisher
    """
    global _current_config

    if publisher_name:
        config = load_publisher_config(publisher_name)
        _current_config = config
        return config

    if _current_config is not None:
        return _current_config

    return load_default_config()


def set_current_publisher(publisher_name: str) -> PublisherConfig:
    """
    Set the current publisher configuration.

    This affects subsequent calls to get_css_mapping, get_epub_type_mapping, etc.

    Args:
        publisher_name: Name of the publisher

    Returns:
        The loaded PublisherConfig
    """
    global _current_config
    _current_config = load_publisher_config(publisher_name)
    logger.info(f"Set current publisher to '{publisher_name}'")
    return _current_config


def reset_publisher_config() -> None:
    """Reset the publisher configuration state."""
    global _config_cache, _default_config, _current_config
    _config_cache = {}
    _default_config = None
    _current_config = None


# ============================================================================
# MAPPING LOOKUP FUNCTIONS
# ============================================================================

def get_css_mapping(css_class: str, publisher: Optional[str] = None) -> Optional[ElementMapping]:
    """
    Get the DocBook element mapping for a CSS class.

    Args:
        css_class: The CSS class name (case-insensitive)
        publisher: Optional publisher name (uses current config if None)

    Returns:
        ElementMapping if found, None otherwise
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()
    return config.css_class_mappings.get(css_class.lower())


def get_epub_type_mapping(epub_type: str, publisher: Optional[str] = None) -> Optional[str]:
    """
    Get the DocBook element for an epub:type value.

    Args:
        epub_type: The epub:type value (case-insensitive)
        publisher: Optional publisher name (uses current config if None)

    Returns:
        DocBook element name if found, None otherwise
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()
    return config.epub_type_mappings.get(epub_type.lower())


def get_id_prefix(element_type: str, publisher: Optional[str] = None) -> Optional[str]:
    """
    Get the ID prefix for an element type.

    Args:
        element_type: The element type (e.g., 'figure', 'table')
        publisher: Optional publisher name (uses current config if None)

    Returns:
        ID prefix if found, None otherwise
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()
    return config.id_patterns.get(element_type.lower())


def should_skip_element(class_name: str, publisher: Optional[str] = None) -> bool:
    """
    Check if an element with the given class should be skipped.

    Args:
        class_name: The CSS class name
        publisher: Optional publisher name (uses current config if None)

    Returns:
        True if element should be skipped
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()

    # Check skip list
    if class_name.lower() in config.skip_elements:
        return True

    # Check if mapping says to skip
    mapping = config.css_class_mappings.get(class_name.lower())
    if mapping and mapping.skip:
        return True

    return False


def get_element_for_class(css_class: str, publisher: Optional[str] = None) -> Optional[str]:
    """
    Convenience function to get just the element name for a CSS class.

    Args:
        css_class: The CSS class name
        publisher: Optional publisher name

    Returns:
        DocBook element name if found, None otherwise
    """
    mapping = get_css_mapping(css_class, publisher)
    return mapping.element if mapping and not mapping.skip else None


def get_role_for_class(css_class: str, publisher: Optional[str] = None) -> Optional[str]:
    """
    Get the role attribute value for a CSS class.

    Args:
        css_class: The CSS class name
        publisher: Optional publisher name

    Returns:
        Role value if defined, None otherwise
    """
    mapping = get_css_mapping(css_class, publisher)
    return mapping.role if mapping else None


# ============================================================================
# BATCH LOOKUP FOR MULTIPLE CLASSES
# ============================================================================

def resolve_css_classes(classes: List[str], publisher: Optional[str] = None) -> Dict[str, Any]:
    """
    Resolve multiple CSS classes to a single element/role.

    When an element has multiple CSS classes, this function determines
    the best DocBook mapping by priority.

    Args:
        classes: List of CSS class names
        publisher: Optional publisher name

    Returns:
        Dict with 'element', 'role', 'context', 'skip' keys
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()

    result = {
        'element': None,
        'role': None,
        'context': None,
        'skip': False,
        'matched_class': None
    }

    # Priority order: more specific classes first
    # (longer class names are often more specific)
    sorted_classes = sorted(classes, key=len, reverse=True)

    for css_class in sorted_classes:
        lower_class = css_class.lower()

        # Check skip list first
        if lower_class in config.skip_elements:
            result['skip'] = True
            result['matched_class'] = css_class
            return result

        mapping = config.css_class_mappings.get(lower_class)
        if mapping:
            if mapping.skip:
                result['skip'] = True
                result['matched_class'] = css_class
                return result

            # Found a mapping - use it if we don't have an element yet
            if result['element'] is None and mapping.element:
                result['element'] = mapping.element
                result['matched_class'] = css_class

            # Collect role if we don't have one
            if result['role'] is None and mapping.role:
                result['role'] = mapping.role

            # Collect context if we don't have one
            if result['context'] is None and mapping.context:
                result['context'] = mapping.context

    return result


# ============================================================================
# DIAGNOSTICS AND DEBUGGING
# ============================================================================

def get_config_summary(publisher: Optional[str] = None) -> Dict[str, Any]:
    """
    Get a summary of the current configuration.

    Args:
        publisher: Optional publisher name

    Returns:
        Dict with configuration summary
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()

    return {
        'publisher_name': config.publisher_name,
        'css_class_count': len(config.css_class_mappings),
        'epub_type_count': len(config.epub_type_mappings),
        'id_pattern_count': len(config.id_patterns),
        'skip_element_count': len(config.skip_elements),
        'normalization_rules': config.normalization
    }


def list_all_css_mappings(publisher: Optional[str] = None) -> Dict[str, str]:
    """
    List all CSS class to element mappings.

    Args:
        publisher: Optional publisher name

    Returns:
        Dict of class_name -> element_name
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()
    return {k: v.element for k, v in config.css_class_mappings.items() if not v.skip}


def list_all_epub_type_mappings(publisher: Optional[str] = None) -> Dict[str, str]:
    """
    List all epub:type to element mappings.

    Args:
        publisher: Optional publisher name

    Returns:
        Dict of type_value -> element_name
    """
    config = get_publisher_config(publisher) if publisher else get_publisher_config()
    return dict(config.epub_type_mappings)
