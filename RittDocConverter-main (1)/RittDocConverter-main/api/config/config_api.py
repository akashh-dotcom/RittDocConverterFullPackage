"""
Configuration API Endpoints

Provides REST API endpoints for managing shared configuration.
These endpoints are designed to be used by the UI project.

The configuration can be stored in:
1. MongoDB (recommended for UI project)
2. Local YAML file (fallback)
"""

import json
import logging
import os
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional

from flask import Blueprint, jsonify, request

logger = logging.getLogger(__name__)

# Create Blueprint for config routes
config_bp = Blueprint('config', __name__, url_prefix='/api/v1/config')

# Path to schema file
SCHEMA_PATH = Path(__file__).parent / 'shared_config_schema.json'


class ConfigManager:
    """
    Manages shared configuration storage and retrieval.

    Supports both MongoDB and local file storage.
    """

    def __init__(self, use_mongodb: bool = True):
        self.use_mongodb = use_mongodb
        self._config_cache: Optional[Dict] = None
        self._cache_time: Optional[datetime] = None
        self._cache_ttl_seconds = 60  # Cache for 1 minute

    def _get_mongodb_collection(self):
        """Get MongoDB config collection."""
        try:
            from src.db.mongodb_client import (MONGODB_AVAILABLE,
                                               get_mongodb_client)
            if not MONGODB_AVAILABLE:
                return None
            client = get_mongodb_client()
            if client.connect():
                return client._db['config']
        except Exception as e:
            logger.warning(f"MongoDB not available: {e}")
        return None

    def get_config(self, section: Optional[str] = None) -> Dict[str, Any]:
        """
        Get configuration.

        Args:
            section: Optional section name (e.g., 'storage', 'processing')

        Returns:
            Configuration dictionary
        """
        # Check cache
        if self._config_cache and self._cache_time:
            age = (datetime.now() - self._cache_time).total_seconds()
            if age < self._cache_ttl_seconds:
                config = self._config_cache
                if section:
                    return config.get(section, {})
                return config

        config = {}

        # Try MongoDB first
        if self.use_mongodb:
            collection = self._get_mongodb_collection()
            if collection:
                doc = collection.find_one({'_id': 'shared_config'})
                if doc:
                    config = doc.get('config', {})

        # Fallback to defaults from schema
        if not config:
            config = self._get_defaults_from_schema()

        # Update cache
        self._config_cache = config
        self._cache_time = datetime.now()

        if section:
            return config.get(section, {})
        return config

    def update_config(self, updates: Dict[str, Any], section: Optional[str] = None) -> bool:
        """
        Update configuration.

        Args:
            updates: Configuration updates
            section: Optional section to update

        Returns:
            True if successful
        """
        try:
            current = self.get_config()

            if section:
                if section not in current:
                    current[section] = {}
                current[section].update(updates)
            else:
                current.update(updates)

            # Save to MongoDB
            if self.use_mongodb:
                collection = self._get_mongodb_collection()
                if collection:
                    collection.update_one(
                        {'_id': 'shared_config'},
                        {
                            '$set': {
                                'config': current,
                                'updated_at': datetime.now().isoformat()
                            }
                        },
                        upsert=True
                    )
                    # Invalidate cache
                    self._config_cache = None
                    return True

            logger.warning("MongoDB not available for config storage")
            return False

        except Exception as e:
            logger.error(f"Failed to update config: {e}")
            return False

    def _get_defaults_from_schema(self) -> Dict[str, Any]:
        """Extract default values from schema."""
        defaults = {}

        try:
            with open(SCHEMA_PATH, 'r') as f:
                schema = json.load(f)

            def extract_defaults(props: Dict, target: Dict):
                for key, prop in props.items():
                    if 'default' in prop:
                        target[key] = prop['default']
                    elif prop.get('type') == 'object' and 'properties' in prop:
                        target[key] = {}
                        extract_defaults(prop['properties'], target[key])

            if 'properties' in schema:
                extract_defaults(schema['properties'], defaults)

        except Exception as e:
            logger.error(f"Failed to load schema defaults: {e}")

        return defaults

    def get_schema(self) -> Dict[str, Any]:
        """Get the configuration schema."""
        try:
            with open(SCHEMA_PATH, 'r') as f:
                return json.load(f)
        except Exception as e:
            logger.error(f"Failed to load schema: {e}")
            return {}


# Singleton manager instance
_config_manager: Optional[ConfigManager] = None


def get_config_manager() -> ConfigManager:
    """Get singleton config manager."""
    global _config_manager
    if _config_manager is None:
        _config_manager = ConfigManager()
    return _config_manager


# =============================================================================
# API Endpoints
# =============================================================================

@config_bp.route('/schema', methods=['GET'])
def get_schema():
    """
    Get the configuration schema.

    The schema includes field definitions, types, defaults, and UI hints.
    Use this to dynamically build configuration forms in the UI.

    Response:
        Full JSON schema with UI widget hints
    """
    manager = get_config_manager()
    schema = manager.get_schema()

    if not schema:
        return jsonify({'error': 'Schema not found'}), 500

    return jsonify(schema)


@config_bp.route('/dropdown-options', methods=['GET'])
def get_dropdown_options():
    """
    Get all dropdown options for the configuration UI.

    Returns a flat list of all fields with dropdown/select options.

    Response:
        {
            "fields": [
                {
                    "path": "storage.type",
                    "title": "Storage Type",
                    "options": [...]
                },
                ...
            ]
        }
    """
    manager = get_config_manager()
    schema = manager.get_schema()

    if not schema:
        return jsonify({'error': 'Schema not found'}), 500

    fields = []

    def extract_dropdowns(props: Dict, path_prefix: str = ''):
        for key, prop in props.items():
            current_path = f"{path_prefix}.{key}" if path_prefix else key

            ui = prop.get('ui', {})
            widget = ui.get('widget', '')

            if widget in ['dropdown', 'multi_select', 'slider']:
                field_info = {
                    'path': current_path,
                    'title': prop.get('title', key),
                    'widget': widget,
                    'default': prop.get('default')
                }

                if 'options' in ui:
                    field_info['options'] = ui['options']
                elif 'enum' in prop:
                    field_info['options'] = [
                        {'value': v, 'label': v} for v in prop['enum']
                    ]

                if widget == 'slider':
                    field_info['min'] = ui.get('min', prop.get('minimum', 0))
                    field_info['max'] = ui.get('max', prop.get('maximum', 100))
                    field_info['step'] = ui.get('step', 1)

                fields.append(field_info)

            # Recurse into nested objects
            if prop.get('type') == 'object' and 'properties' in prop:
                extract_dropdowns(prop['properties'], current_path)

    if 'properties' in schema:
        extract_dropdowns(schema['properties'])

    return jsonify({'fields': fields})


@config_bp.route('', methods=['GET'])
def get_config():
    """
    Get current configuration.

    Query parameters:
        section: Optional section name (storage, processing, etc.)

    Response:
        Current configuration values
    """
    manager = get_config_manager()
    section = request.args.get('section')

    config = manager.get_config(section)

    return jsonify({
        'config': config,
        'section': section
    })


@config_bp.route('', methods=['PUT', 'PATCH'])
def update_config():
    """
    Update configuration.

    Request body:
        {
            "section": "storage" (optional),
            "config": {
                "key": "value",
                ...
            }
        }

    Response:
        {
            "success": true,
            "message": "Configuration updated"
        }
    """
    data = request.get_json()

    if not data or 'config' not in data:
        return jsonify({'error': 'config is required'}), 400

    manager = get_config_manager()
    section = data.get('section')
    updates = data['config']

    success = manager.update_config(updates, section)

    if success:
        return jsonify({
            'success': True,
            'message': 'Configuration updated'
        })
    else:
        return jsonify({
            'success': False,
            'error': 'Failed to save configuration'
        }), 500


@config_bp.route('/reset', methods=['POST'])
def reset_config():
    """
    Reset configuration to defaults.

    Request body:
        {
            "section": "storage" (optional - reset specific section)
        }

    Response:
        {
            "success": true,
            "config": {...}
        }
    """
    data = request.get_json() or {}
    section = data.get('section')

    manager = get_config_manager()
    defaults = manager._get_defaults_from_schema()

    if section:
        if section in defaults:
            success = manager.update_config(defaults[section], section)
        else:
            return jsonify({'error': f'Unknown section: {section}'}), 400
    else:
        # Reset entire config
        collection = manager._get_mongodb_collection()
        if collection:
            collection.delete_one({'_id': 'shared_config'})
            manager._config_cache = None
            success = True
        else:
            success = False

    if success:
        return jsonify({
            'success': True,
            'config': manager.get_config(section)
        })
    else:
        return jsonify({
            'success': False,
            'error': 'Failed to reset configuration'
        }), 500


@config_bp.route('/validate', methods=['POST'])
def validate_config():
    """
    Validate configuration without saving.

    Request body:
        {
            "config": {...}
        }

    Response:
        {
            "valid": true,
            "errors": []
        }
    """
    data = request.get_json()

    if not data or 'config' not in data:
        return jsonify({'error': 'config is required'}), 400

    config = data['config']
    errors = []

    # Basic validation
    manager = get_config_manager()
    schema = manager.get_schema()

    def validate_section(cfg: Dict, props: Dict, path: str = ''):
        for key, value in cfg.items():
            current_path = f"{path}.{key}" if path else key

            if key not in props:
                errors.append({
                    'path': current_path,
                    'error': 'Unknown configuration key'
                })
                continue

            prop = props[key]

            # Type validation
            expected_type = prop.get('type')
            if expected_type == 'string' and not isinstance(value, str):
                errors.append({
                    'path': current_path,
                    'error': f'Expected string, got {type(value).__name__}'
                })
            elif expected_type == 'integer' and not isinstance(value, int):
                errors.append({
                    'path': current_path,
                    'error': f'Expected integer, got {type(value).__name__}'
                })
            elif expected_type == 'boolean' and not isinstance(value, bool):
                errors.append({
                    'path': current_path,
                    'error': f'Expected boolean, got {type(value).__name__}'
                })

            # Enum validation
            if 'enum' in prop and value not in prop['enum']:
                errors.append({
                    'path': current_path,
                    'error': f'Value must be one of: {prop["enum"]}'
                })

            # Range validation
            if expected_type == 'integer':
                if 'minimum' in prop and value < prop['minimum']:
                    errors.append({
                        'path': current_path,
                        'error': f'Value must be >= {prop["minimum"]}'
                    })
                if 'maximum' in prop and value > prop['maximum']:
                    errors.append({
                        'path': current_path,
                        'error': f'Value must be <= {prop["maximum"]}'
                    })

            # Recurse into objects
            if expected_type == 'object' and isinstance(value, dict):
                if 'properties' in prop:
                    validate_section(value, prop['properties'], current_path)

    if 'properties' in schema:
        validate_section(config, schema['properties'])

    return jsonify({
        'valid': len(errors) == 0,
        'errors': errors
    })


# =============================================================================
# Publisher Management Endpoints
# =============================================================================

@config_bp.route('/publishers', methods=['GET'])
def get_publishers():
    """
    Get all publisher profiles.

    Response:
        {
            "publishers": [
                {
                    "name": "O'Reilly Media",
                    "confidence_base": 95,
                    "aliases": [...],
                    ...
                }
            ]
        }
    """
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        publishers = []

        # Try MongoDB first
        if MONGODB_AVAILABLE:
            client = get_mongodb_client()
            if client.connect():
                collection = client._db['publishers']
                publishers = list(collection.find({}, {'_id': 0}))

        # Fallback to YAML file
        if not publishers:
            yaml_path = Path(__file__).parent.parent.parent / 'epub_publishers.yaml'
            example_path = Path(__file__).parent.parent.parent / 'epub_publishers.yaml.example'

            import yaml

            for path in [yaml_path, example_path]:
                if path.exists():
                    with open(path, 'r') as f:
                        data = yaml.safe_load(f)
                    if data and 'publishers' in data:
                        publishers = [
                            {'name': name, **config}
                            for name, config in data['publishers'].items()
                        ]
                    break

        return jsonify({'publishers': publishers})

    except Exception as e:
        logger.exception("Failed to get publishers")
        return jsonify({'error': str(e), 'publishers': []}), 500


@config_bp.route('/publishers/<name>', methods=['GET'])
def get_publisher(name: str):
    """Get a specific publisher profile."""
    result = get_publishers()
    data = result.get_json()

    for pub in data.get('publishers', []):
        if pub.get('name') == name:
            return jsonify(pub)

    return jsonify({'error': 'Publisher not found'}), 404


@config_bp.route('/publishers', methods=['POST'])
def create_publisher():
    """
    Create a new publisher profile.

    Request body:
        {
            "name": "Publisher Name",
            "confidence_base": 85,
            "aliases": [],
            "isbn_prefixes": [],
            "known_issues": []
        }
    """
    data = request.get_json()

    if not data or 'name' not in data:
        return jsonify({'error': 'name is required'}), 400

    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({'error': 'MongoDB required for publisher management'}), 503

        client = get_mongodb_client()
        if not client.connect():
            return jsonify({'error': 'MongoDB connection failed'}), 503

        collection = client._db['publishers']

        # Check if exists
        if collection.find_one({'name': data['name']}):
            return jsonify({'error': 'Publisher already exists'}), 409

        # Set defaults
        publisher = {
            'name': data['name'],
            'aliases': data.get('aliases', []),
            'isbn_prefixes': data.get('isbn_prefixes', []),
            'confidence_base': data.get('confidence_base', 70),
            'known_issues': data.get('known_issues', []),
            'notes': data.get('notes', ''),
            'success_rate': 0,
            'total_processed': 0,
            'created_at': datetime.now().isoformat()
        }

        collection.insert_one(publisher)

        # Remove MongoDB _id for response
        publisher.pop('_id', None)

        return jsonify({
            'success': True,
            'publisher': publisher
        }), 201

    except Exception as e:
        logger.exception("Failed to create publisher")
        return jsonify({'error': str(e)}), 500


@config_bp.route('/publishers/<name>', methods=['PUT'])
def update_publisher(name: str):
    """Update a publisher profile."""
    data = request.get_json()

    if not data:
        return jsonify({'error': 'Request body required'}), 400

    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({'error': 'MongoDB required'}), 503

        client = get_mongodb_client()
        if not client.connect():
            return jsonify({'error': 'MongoDB connection failed'}), 503

        collection = client._db['publishers']

        # Don't allow name change through update
        data.pop('name', None)
        data['updated_at'] = datetime.now().isoformat()

        result = collection.update_one(
            {'name': name},
            {'$set': data}
        )

        if result.matched_count == 0:
            return jsonify({'error': 'Publisher not found'}), 404

        return jsonify({
            'success': True,
            'message': 'Publisher updated'
        })

    except Exception as e:
        logger.exception("Failed to update publisher")
        return jsonify({'error': str(e)}), 500


@config_bp.route('/publishers/<name>', methods=['DELETE'])
def delete_publisher(name: str):
    """Delete a publisher profile."""
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({'error': 'MongoDB required'}), 503

        client = get_mongodb_client()
        if not client.connect():
            return jsonify({'error': 'MongoDB connection failed'}), 503

        collection = client._db['publishers']
        result = collection.delete_one({'name': name})

        if result.deleted_count == 0:
            return jsonify({'error': 'Publisher not found'}), 404

        return jsonify({
            'success': True,
            'message': 'Publisher deleted'
        })

    except Exception as e:
        logger.exception("Failed to delete publisher")
        return jsonify({'error': str(e)}), 500
