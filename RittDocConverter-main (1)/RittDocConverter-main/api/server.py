"""
REST API Server for EPUB Processing

This module provides a Flask-based REST API for the EPUB processing pipeline.
It enables the UI project to communicate with this processing pipeline.

Usage:
    python -m api.server [--port 5001] [--host 0.0.0.0]

    Or import and use programmatically:
        from api import create_app
        app = create_app()
        app.run()
"""

import json
import logging
import os
import sys
from functools import wraps
from pathlib import Path
from typing import Optional

from flask import Blueprint, Flask, Response, jsonify, request, send_file
from werkzeug.utils import secure_filename

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from api.conversion_api import ConversionAPI, JobStatus, get_api, MAX_JOB_ID_LENGTH

# Input validation imports
try:
    from input_validator import (
        validate_epub_file, validate_file_upload, is_valid_epub,
        FileValidationError, FileSizeError, FileTypeError,
        ZipStructureError, PathTraversalError, EPUBStructureError,
        MAX_EPUB_SIZE_MB
    )
    HAS_INPUT_VALIDATOR = True
except ImportError:
    HAS_INPUT_VALIDATOR = False
    MAX_EPUB_SIZE_MB = 500  # Default fallback

# Authentication imports
try:
    from api.auth import (
        require_auth, require_admin, optional_auth,
        create_auth_management_blueprint, get_auth_config
    )
    HAS_AUTH = True
except ImportError:
    HAS_AUTH = False
    # Provide no-op decorators if auth module not available
    def require_auth(f):
        return f
    def require_admin(f):
        return f
    def optional_auth(f):
        return f

logger = logging.getLogger(__name__)

# Create Blueprint for API routes
api_bp = Blueprint('api', __name__, url_prefix='/api/v1')

# Allowed file extensions
ALLOWED_EXTENSIONS = {'.epub', '.epub3'}


def validate_id(id_value: str, max_length: int = MAX_JOB_ID_LENGTH) -> str:
    """
    Validate and sanitize an ID value.
    Ensures IDs don't exceed max_length characters.
    """
    if not id_value:
        return id_value
    return id_value[:max_length]


# Default bounds for query parameters
QUERY_PARAM_BOUNDS = {
    'limit': {'min': 1, 'max': 1000, 'default': 100},
    'skip': {'min': 0, 'max': 100000, 'default': 0},
}


def get_bounded_int(
    param_name: str,
    default: Optional[int] = None,
    min_val: Optional[int] = None,
    max_val: Optional[int] = None
) -> int:
    """
    Get an integer query parameter with bounds validation.

    Prevents DoS attacks from extremely large values and validates input.

    Args:
        param_name: Name of the query parameter
        default: Default value if not provided (uses QUERY_PARAM_BOUNDS if None)
        min_val: Minimum allowed value (uses QUERY_PARAM_BOUNDS if None)
        max_val: Maximum allowed value (uses QUERY_PARAM_BOUNDS if None)

    Returns:
        Validated integer value within bounds
    """
    # Get bounds from defaults if not specified
    bounds = QUERY_PARAM_BOUNDS.get(param_name, {})
    if default is None:
        default = bounds.get('default', 0)
    if min_val is None:
        min_val = bounds.get('min', 0)
    if max_val is None:
        max_val = bounds.get('max', 10000)

    # Get the parameter value
    try:
        value = request.args.get(param_name, default, type=int)
        if value is None:
            value = default
    except (ValueError, TypeError):
        logger.warning(f"Invalid integer value for parameter '{param_name}', using default {default}")
        value = default

    # Apply bounds
    original_value = value
    value = max(min_val, min(max_val, value))

    # Log if value was clamped
    if value != original_value:
        logger.debug(f"Parameter '{param_name}' value {original_value} clamped to {value} (bounds: {min_val}-{max_val})")

    return value


def allowed_file(filename: str) -> bool:
    """Check if the file extension is allowed."""
    return Path(filename).suffix.lower() in ALLOWED_EXTENSIONS


def validate_uploaded_file(file_storage, filename: str) -> tuple:
    """
    Validate an uploaded file with comprehensive security checks.

    Args:
        file_storage: Flask FileStorage object
        filename: Original filename

    Returns:
        Tuple of (is_valid, error_message, validation_result)
    """
    if not allowed_file(filename):
        return (False, f"Invalid file extension. Allowed: {', '.join(ALLOWED_EXTENSIONS)}", None)

    if not HAS_INPUT_VALIDATOR:
        # Fallback: basic validation only
        logger.warning("Input validator not available, using basic validation only")
        return (True, None, None)

    try:
        result = validate_file_upload(file_storage, filename, MAX_EPUB_SIZE_MB)

        if not result.is_valid:
            error_msg = "; ".join(result.errors) if result.errors else "Validation failed"
            return (False, error_msg, result)

        if result.warnings:
            logger.warning(f"File validation warnings for {filename}: {result.warnings}")

        return (True, None, result)

    except FileSizeError as e:
        return (False, f"File size error: {e}", None)
    except FileTypeError as e:
        return (False, f"File type error: {e}", None)
    except ZipStructureError as e:
        return (False, f"ZIP structure error: {e}", None)
    except PathTraversalError as e:
        return (False, f"Security error: {e}", None)
    except EPUBStructureError as e:
        return (False, f"EPUB structure error: {e}", None)
    except FileValidationError as e:
        return (False, f"Validation error: {e}", None)
    except Exception as e:
        logger.error(f"Unexpected validation error: {e}")
        return (False, f"Validation error: {e}", None)


def require_json(f):
    """Decorator to require valid JSON body for POST/PUT requests."""
    @wraps(f)
    def decorated(*args, **kwargs):
        if request.method in ['POST', 'PUT']:
            # Allow file uploads without JSON
            if request.files:
                return f(*args, **kwargs)

            # Check Content-Type header
            if not request.is_json:
                return jsonify({
                    'error': 'Content-Type must be application/json'
                }), 415

            # Validate JSON body is not empty/None
            try:
                json_data = request.get_json(silent=True)
                if json_data is None:
                    return jsonify({
                        'error': 'Request body must contain valid JSON'
                    }), 400
            except Exception as e:
                return jsonify({
                    'error': f'Invalid JSON body: {str(e)}'
                }), 400

        return f(*args, **kwargs)
    return decorated


# =============================================================================
# Health Check & Info
# =============================================================================

@api_bp.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint with storage status."""
    health = {
        'status': 'healthy',
        'service': 'epub-processor',
        'version': '2.0.0',
        'storage': {
            'available': False,
            'connected': False,
            'backend': 'none'
        }
    }

    # Check storage status
    try:
        from src.storage import get_storage
        storage = get_storage()
        health['storage'] = {
            'available': True,
            'connected': storage.is_connected(),
            'backend': storage.get_stats().backend
        }
    except ImportError:
        pass
    except Exception as e:
        health['storage']['error'] = str(e)

    return jsonify(health)


@api_bp.route('/service-info', methods=['GET'])
def get_service_info():
    """
    Service discovery endpoint.

    Returns information about this service for service discovery and
    dynamic configuration. The UI project should call this endpoint
    to discover service capabilities and URLs.

    Response:
        {
            "service": {
                "name": "epub-processor",
                "version": "2.0.0",
                "type": "conversion"
            },
            "endpoints": {...},
            "capabilities": [...],
            "dependencies": {...},
            "environment": "development"
        }
    """
    import os

    # Get environment info
    environment = os.getenv('ENVIRONMENT', 'development')
    external_url = os.getenv('EPUB_SERVICE_EXTERNAL_URL', '')

    return jsonify({
        'service': {
            'name': 'epub-processor',
            'display_name': 'EPUB Processing Service',
            'version': '2.0.0',
            'type': 'conversion',
            'description': 'Converts EPUB files to RittDoc DocBook XML format'
        },
        'environment': environment,
        'external_url': external_url,
        'endpoints': {
            'health': '/api/v1/health',
            'info': '/api/v1/info',
            'service_info': '/api/v1/service-info',
            'convert': '/api/v1/convert',
            'jobs': '/api/v1/jobs',
            'config': '/api/v1/config',
            'dashboard': '/api/v1/mongodb/dashboard'
        },
        'capabilities': [
            'epub_to_docbook',
            'epub3_support',
            'batch_conversion',
            'async_processing',
            'progress_tracking',
            'validation_reports',
            'xml_editing',
            'reprocessing'
        ],
        'supported_formats': {
            'input': ['.epub', '.epub3'],
            'output': ['docbook-xml', 'rittdoc-zip']
        },
        'dependencies': {
            'mongodb': {
                'required': False,
                'status_endpoint': '/api/v1/mongodb/status'
            }
        },
        'limits': {
            'max_file_size_mb': int(os.getenv('MAX_FILE_SIZE_MB', 100)),
            'max_concurrent_jobs': int(os.getenv('MAX_CONCURRENT_JOBS', 4)),
            'job_timeout_seconds': int(os.getenv('JOB_TIMEOUT_SECONDS', 3600))
        }
    })


@api_bp.route('/info', methods=['GET'])
def get_info():
    """Get API information and available endpoints."""
    return jsonify({
        'service': 'EPUB Processing API',
        'version': '2.0.0',
        'description': 'REST API for converting EPUB files to RittDoc format',
        'openapi': '/api/v1/openapi.yaml',
        'endpoints': {
            'health': '/api/v1/health',
            'info': '/api/v1/info',
            'service_info': '/api/v1/service-info',
            'openapi': '/api/v1/openapi.yaml',
            'convert': '/api/v1/convert',
            'reprocess': '/api/v1/reprocess',
            'jobs': '/api/v1/jobs',
            'job_status': '/api/v1/jobs/<job_id>',
            'job_result': '/api/v1/jobs/<job_id>/result',
            'dashboard': '/api/v1/dashboard',
            'download': '/api/v1/download/<job_id>',
            'mongodb': {
                'status': '/api/v1/mongodb/status',
                'dashboard': '/api/v1/mongodb/dashboard',
                'conversions': '/api/v1/mongodb/conversions',
                'statistics': '/api/v1/mongodb/statistics',
                'recent': '/api/v1/mongodb/recent',
                'failed': '/api/v1/mongodb/failed'
            },
            'config': {
                'schema': '/api/v1/config/schema',
                'dropdown_options': '/api/v1/config/dropdown-options',
                'get_config': '/api/v1/config',
                'update_config': '/api/v1/config (PUT/PATCH)',
                'reset_config': '/api/v1/config/reset',
                'validate_config': '/api/v1/config/validate',
                'publishers': '/api/v1/config/publishers'
            }
        }
    })


@api_bp.route('/openapi.yaml', methods=['GET'])
def get_openapi_spec():
    """
    Get OpenAPI 3.0 specification.

    Returns the complete API specification in YAML format.
    Use this to generate client SDKs or view in Swagger UI.
    """
    openapi_path = Path(__file__).parent / 'openapi.yaml'
    if openapi_path.exists():
        return send_file(
            openapi_path,
            mimetype='application/x-yaml',
            as_attachment=False
        )
    return jsonify({'error': 'OpenAPI specification not found'}), 404


@api_bp.route('/openapi.json', methods=['GET'])
def get_openapi_spec_json():
    """
    Get OpenAPI 3.0 specification in JSON format.
    """
    import yaml
    openapi_path = Path(__file__).parent / 'openapi.yaml'
    if openapi_path.exists():
        with open(openapi_path, 'r') as f:
            spec = yaml.safe_load(f)
        return jsonify(spec)
    return jsonify({'error': 'OpenAPI specification not found'}), 404


# =============================================================================
# Conversion Endpoints
# =============================================================================

@api_bp.route('/convert', methods=['POST'])
@require_auth
def start_conversion():
    """
    Start an EPUB conversion job.

    Request body (JSON):
        {
            "input_file": "/path/to/file.epub",
            "output_dir": "/path/to/output" (optional),
            "job_id": "custom-id" (optional, for UI integration),
            "async": true (optional, default true),
            "debug": false (optional)
        }

    Or upload a file:
        Content-Type: multipart/form-data
        file: <epub file>
        output_dir: /path/to/output (optional)
        job_id: custom-id (optional, for UI integration)

    Response:
        {
            "job_id": "uuid",
            "status": "started",
            "message": "..."
        }
    """
    api = get_api()

    try:
        job_id = None  # Optional custom job ID from UI

        # Handle file upload
        if 'file' in request.files:
            file = request.files['file']
            if file.filename == '':
                return jsonify({'error': 'No file selected'}), 400

            # Comprehensive file validation (magic bytes, size, structure)
            is_valid, error_msg, validation_result = validate_uploaded_file(
                file, file.filename
            )

            if not is_valid:
                return jsonify({
                    'error': error_msg,
                    'validation': validation_result.metadata if validation_result else None
                }), 400

            # Log validation info
            if validation_result:
                logger.info(
                    f"File validated: {file.filename} "
                    f"({validation_result.file_size_mb:.1f}MB, "
                    f"{validation_result.metadata.get('entry_count', 0)} entries)"
                )
                if validation_result.warnings:
                    for warning in validation_result.warnings:
                        logger.warning(f"File warning: {warning}")

            # Save uploaded file
            filename = secure_filename(file.filename)
            upload_dir = api.output_dir / 'uploads'
            upload_dir.mkdir(parents=True, exist_ok=True)
            input_path = upload_dir / filename

            # Reset file position after validation and save
            file.seek(0)
            file.save(str(input_path))

            output_dir = request.form.get('output_dir')
            job_id = request.form.get('job_id')  # Get custom job_id from form
            async_mode = request.form.get('async', 'true').lower() == 'true'
            debug = request.form.get('debug', 'false').lower() == 'true'

        # Handle JSON request
        elif request.is_json:
            data = request.get_json()
            input_path = data.get('input_file')
            output_dir = data.get('output_dir')
            job_id = data.get('job_id')  # Get custom job_id from JSON
            async_mode = data.get('async', True)
            debug = data.get('debug', False)

            if not input_path:
                return jsonify({'error': 'input_file is required'}), 400

            # Validate file path for security and EPUB validity
            input_path = Path(input_path)

            if HAS_INPUT_VALIDATOR:
                try:
                    validation_result = validate_epub_file(input_path)
                    if not validation_result.is_valid:
                        return jsonify({
                            'error': f"Invalid EPUB file: {'; '.join(validation_result.errors)}",
                            'validation': validation_result.metadata
                        }), 400
                    logger.info(
                        f"Path validated: {input_path} "
                        f"({validation_result.file_size_mb:.1f}MB)"
                    )
                except FileValidationError as e:
                    return jsonify({'error': f"File validation error: {e}"}), 400

        else:
            return jsonify({
                'error': 'Request must be JSON or multipart/form-data with file'
            }), 415

        # Validate custom job_id if provided
        if job_id:
            job_id = validate_id(job_id)

        # Start conversion
        result = api.start_conversion(
            input_file=str(input_path),
            output_dir=output_dir,
            async_mode=async_mode,
            debug=debug,
            job_id=job_id  # Pass custom job_id to conversion API
        )

        return jsonify(result), 202 if async_mode else 200

    except FileNotFoundError as e:
        return jsonify({'error': str(e)}), 404
    except ValueError as e:
        return jsonify({'error': str(e)}), 400
    except Exception as e:
        logger.exception("Conversion failed")
        return jsonify({'error': str(e)}), 500


@api_bp.route('/convert/batch', methods=['POST'])
@require_auth
@require_json
def start_batch_conversion():
    """
    Start batch conversion of multiple EPUB files.

    Request body:
        {
            "files": ["/path/to/file1.epub", "/path/to/file2.epub"],
            "output_dir": "/path/to/output" (optional)
        }

    Response:
        {
            "jobs": [
                {"job_id": "uuid1", "file": "file1.epub", "status": "started"},
                {"job_id": "uuid2", "file": "file2.epub", "status": "started"}
            ],
            "total": 2
        }
    """
    api = get_api()
    data = request.get_json()

    files = data.get('files', [])
    output_dir = data.get('output_dir')

    if not files:
        return jsonify({'error': 'files array is required'}), 400

    results = []
    for file_path in files:
        try:
            result = api.start_conversion(
                input_file=file_path,
                output_dir=output_dir,
                async_mode=True
            )
            results.append({
                'file': Path(file_path).name,
                **result
            })
        except Exception as e:
            results.append({
                'file': Path(file_path).name,
                'status': 'error',
                'error': str(e)
            })

    return jsonify({
        'jobs': results,
        'total': len(results)
    }), 202


@api_bp.route('/reprocess', methods=['POST'])
@require_auth
@require_json
def reprocess_package_endpoint():
    """
    Reprocess an edited RittDoc package.

    This endpoint takes an existing package (after editing) and re-runs:
    - XSLT transformation
    - Repackaging
    - DTD fixing (multi-pass)
    - Validation

    Request body:
        {
            "package_path": "/path/to/package.zip",  // Path to ZIP or extracted dir
            "output_dir": "/path/to/output" (optional),
            "apply_xslt": true (optional, default true),
            "apply_dtd_fixes": true (optional, default true),
            "validate": true (optional, default true)
        }

    Response:
        {
            "success": true,
            "output_zip": "/path/to/output.zip",
            "validation_passed": true,
            "errors_fixed": 10,
            "remaining_errors": 0,
            "message": "..."
        }
    """
    try:
        from reprocess import reprocess_package
    except ImportError:
        return jsonify({
            'error': 'Reprocessing module not available'
        }), 500

    data = request.get_json()

    package_path = data.get('package_path')
    if not package_path:
        return jsonify({'error': 'package_path is required'}), 400

    package_path = Path(package_path)
    if not package_path.exists():
        return jsonify({'error': f'Package not found: {package_path}'}), 404

    output_dir = Path(data.get('output_dir')) if data.get('output_dir') else None
    apply_xslt = data.get('apply_xslt', True)
    apply_dtd_fixes = data.get('apply_dtd_fixes', True)
    validate = data.get('validate', True)

    try:
        import tempfile
        import zipfile

        # If it's a ZIP file, extract it first
        if package_path.is_file() and package_path.suffix == '.zip':
            temp_dir = tempfile.mkdtemp(prefix='reprocess_api_')
            extracted_dir = Path(temp_dir) / 'extracted'
            with zipfile.ZipFile(package_path, 'r') as zf:
                zf.extractall(extracted_dir)
        else:
            extracted_dir = package_path

        result = reprocess_package(
            extracted_dir=extracted_dir,
            original_zip_path=package_path if package_path.suffix == '.zip' else package_path.parent / f"{package_path.name}.zip",
            output_dir=output_dir,
            apply_xslt=apply_xslt,
            apply_dtd_fixes=apply_dtd_fixes,
            validate=validate
        )

        return jsonify(result), 200 if result['success'] else 500

    except Exception as e:
        logger.exception("Reprocessing failed")
        return jsonify({
            'success': False,
            'error': str(e),
            'message': f'Reprocessing failed: {str(e)}'
        }), 500


# =============================================================================
# Job Management Endpoints
# =============================================================================

@api_bp.route('/jobs', methods=['GET'])
def list_jobs():
    """
    List conversion jobs.

    Query parameters:
        status: Filter by status (pending, running, completed, failed, cancelled)
        limit: Maximum number of jobs to return (default: 100)

    Response:
        {
            "jobs": [...],
            "total": 10
        }
    """
    api = get_api()

    status = request.args.get('status')
    limit = get_bounded_int('limit', default=100, max_val=500)

    if status:
        try:
            status = JobStatus(status)
        except ValueError:
            return jsonify({
                'error': f'Invalid status. Valid values: {", ".join([s.value for s in JobStatus])}'
            }), 400

    jobs = api.list_jobs(status=status, limit=limit)

    return jsonify({
        'jobs': jobs,
        'total': len(jobs)
    })


@api_bp.route('/jobs/<job_id>', methods=['GET'])
def get_job_status(job_id: str):
    """
    Get the status of a specific job.

    Response:
        {
            "job_id": "uuid",
            "status": "running",
            "progress": 50,
            "message": "...",
            ...
        }
    """
    job_id = validate_id(job_id)
    api = get_api()

    try:
        status = api.get_job_status(job_id)
        return jsonify(status)
    except KeyError:
        return jsonify({'error': 'Job not found'}), 404


@api_bp.route('/jobs/<job_id>/result', methods=['GET'])
def get_job_result(job_id: str):
    """
    Get detailed result of a completed job.

    Response:
        {
            "job_id": "uuid",
            "input_file": "...",
            "output_file": "...",
            "validation_report": "...",
            ...
        }
    """
    job_id = validate_id(job_id)
    api = get_api()

    try:
        result = api.get_conversion_result(job_id)
        return jsonify(result)
    except KeyError:
        return jsonify({'error': 'Job not found'}), 404


@api_bp.route('/jobs/<job_id>', methods=['DELETE'])
def cancel_job(job_id: str):
    """
    Cancel a running or pending job.

    Response:
        {
            "status": "cancelled",
            "message": "..."
        }
    """
    job_id = validate_id(job_id)
    api = get_api()

    try:
        result = api.cancel_job(job_id)
        return jsonify(result)
    except KeyError:
        return jsonify({'error': 'Job not found'}), 404


# =============================================================================
# Dashboard Endpoints
# =============================================================================

# MongoDB Dashboard Endpoints (for UI project integration)
# These endpoints query MongoDB for conversion data

@api_bp.route('/mongodb/status', methods=['GET'])
def mongodb_status():
    """
    Check MongoDB connection status.

    Response:
        {
            "available": true,
            "connected": true,
            "database": "rittdoc_converter",
            "collection": "conversions"
        }
    """
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({
                'available': False,
                'connected': False,
                'message': 'pymongo not installed'
            })

        client = get_mongodb_client()
        connected = client.connect()

        return jsonify({
            'available': True,
            'connected': connected,
            'database': client.database_name,
            'collection': client.collection_name
        })

    except Exception as e:
        return jsonify({
            'available': False,
            'connected': False,
            'error': str(e)
        })


@api_bp.route('/mongodb/conversions', methods=['GET'])
def mongodb_get_conversions():
    """
    Get conversions from MongoDB.

    Query parameters:
        status: Filter by status (Success, Failure, In Progress)
        type: Filter by conversion type (ePub, PDF)
        start_date: Filter by start date (ISO format, from)
        end_date: Filter by start date (ISO format, to)
        limit: Maximum records (default: 100)
        skip: Records to skip for pagination (default: 0)

    Response:
        {
            "conversions": [...],
            "total": 50,
            "limit": 100,
            "skip": 0
        }
    """
    try:
        from datetime import datetime

        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({
                'error': 'MongoDB not available',
                'conversions': []
            }), 503

        client = get_mongodb_client()

        status = request.args.get('status')
        conversion_type = request.args.get('type')
        start_date_str = request.args.get('start_date')
        end_date_str = request.args.get('end_date')
        limit = get_bounded_int('limit', default=100, max_val=1000)
        skip = get_bounded_int('skip', default=0, max_val=100000)

        # Parse dates
        start_date = None
        end_date = None
        if start_date_str:
            start_date = datetime.fromisoformat(start_date_str.replace('Z', '+00:00'))
        if end_date_str:
            end_date = datetime.fromisoformat(end_date_str.replace('Z', '+00:00'))

        conversions = client.get_conversions(
            status=status,
            conversion_type=conversion_type,
            start_date=start_date,
            end_date=end_date,
            limit=limit,
            skip=skip
        )

        return jsonify({
            'conversions': conversions,
            'total': len(conversions),
            'limit': limit,
            'skip': skip
        })

    except Exception as e:
        logger.exception("Failed to get conversions from MongoDB")
        return jsonify({
            'error': str(e),
            'conversions': []
        }), 500


@api_bp.route('/mongodb/statistics', methods=['GET'])
def mongodb_get_statistics():
    """
    Get aggregated statistics from MongoDB.

    Response:
        {
            "total_conversions": 100,
            "successful": 90,
            "failed": 5,
            "in_progress": 5,
            "total_images": 500,
            "total_tables": 50,
            "pdf_conversions": 30,
            "epub_conversions": 70
        }
    """
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({
                'error': 'MongoDB not available'
            }), 503

        client = get_mongodb_client()
        stats = client.get_statistics()

        return jsonify(stats)

    except Exception as e:
        logger.exception("Failed to get statistics from MongoDB")
        return jsonify({
            'error': str(e)
        }), 500


@api_bp.route('/mongodb/recent', methods=['GET'])
def mongodb_get_recent():
    """
    Get most recent conversions from MongoDB.

    Query parameters:
        limit: Maximum records (default: 10)

    Response:
        {
            "conversions": [...],
            "total": 10
        }
    """
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({
                'error': 'MongoDB not available',
                'conversions': []
            }), 503

        client = get_mongodb_client()
        limit = get_bounded_int('limit', default=10, max_val=100)

        conversions = client.get_recent_conversions(limit=limit)

        return jsonify({
            'conversions': conversions,
            'total': len(conversions)
        })

    except Exception as e:
        logger.exception("Failed to get recent conversions")
        return jsonify({
            'error': str(e),
            'conversions': []
        }), 500


@api_bp.route('/mongodb/failed', methods=['GET'])
def mongodb_get_failed():
    """
    Get failed conversions from MongoDB for review.

    Query parameters:
        limit: Maximum records (default: 50)

    Response:
        {
            "conversions": [...],
            "total": 5
        }
    """
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({
                'error': 'MongoDB not available',
                'conversions': []
            }), 503

        client = get_mongodb_client()
        limit = get_bounded_int('limit', default=50, max_val=200)

        conversions = client.get_failed_conversions(limit=limit)

        return jsonify({
            'conversions': conversions,
            'total': len(conversions)
        })

    except Exception as e:
        logger.exception("Failed to get failed conversions")
        return jsonify({
            'error': str(e),
            'conversions': []
        }), 500


@api_bp.route('/mongodb/dashboard', methods=['GET'])
def mongodb_dashboard():
    """
    Get full dashboard data from MongoDB.

    Combines statistics with recent and failed conversions for a complete
    dashboard view.

    Response:
        {
            "statistics": {...},
            "recent_conversions": [...],
            "failed_conversions": [...],
            "source": "mongodb"
        }
    """
    try:
        from src.db.mongodb_client import MONGODB_AVAILABLE, get_mongodb_client

        if not MONGODB_AVAILABLE:
            return jsonify({
                'error': 'MongoDB not available',
                'source': 'none'
            }), 503

        client = get_mongodb_client()

        stats = client.get_statistics()
        recent = client.get_recent_conversions(limit=10)
        failed = client.get_failed_conversions(limit=10)

        return jsonify({
            'statistics': stats,
            'recent_conversions': recent,
            'failed_conversions': failed,
            'source': 'mongodb'
        })

    except Exception as e:
        logger.exception("Failed to get dashboard from MongoDB")
        return jsonify({
            'error': str(e),
            'source': 'error'
        }), 500


# Excel-based Dashboard Endpoints (legacy, uses local Excel file)

@api_bp.route('/dashboard', methods=['GET'])
def get_dashboard():
    """
    Get conversion dashboard data.

    Response:
        {
            "statistics": {
                "total_jobs": 100,
                "completed": 90,
                "failed": 5,
                ...
            },
            "recent_jobs": [...],
            "dashboard_records": [...]
        }
    """
    api = get_api()
    data = api.get_dashboard_data()
    return jsonify(data)


@api_bp.route('/dashboard/statistics', methods=['GET'])
def get_statistics():
    """
    Get just the statistics from the dashboard.

    Response:
        {
            "total_jobs": 100,
            "completed": 90,
            ...
        }
    """
    api = get_api()
    data = api.get_dashboard_data()
    return jsonify(data['statistics'])


# =============================================================================
# Download Endpoints
# =============================================================================

@api_bp.route('/download/<job_id>', methods=['GET'])
def download_result(job_id: str):
    """
    Download the output ZIP file for a completed job.

    Response:
        Binary file download
    """
    job_id = validate_id(job_id)
    api = get_api()

    try:
        result = api.get_conversion_result(job_id)

        if result.get('status') == 'error':
            return jsonify(result), 400

        output_file = result.get('output_file')
        if not output_file or not Path(output_file).exists():
            return jsonify({'error': 'Output file not found'}), 404

        return send_file(
            output_file,
            mimetype='application/zip',
            as_attachment=True,
            download_name=Path(output_file).name
        )

    except KeyError:
        return jsonify({'error': 'Job not found'}), 404


@api_bp.route('/download/<job_id>/report', methods=['GET'])
def download_report(job_id: str):
    """
    Download the validation report for a completed job.

    Response:
        Binary file download (Excel)
    """
    job_id = validate_id(job_id)
    api = get_api()

    try:
        result = api.get_conversion_result(job_id)

        if result.get('status') == 'error':
            return jsonify(result), 400

        report_file = result.get('validation_report')
        if not report_file or not Path(report_file).exists():
            return jsonify({'error': 'Validation report not found'}), 404

        return send_file(
            report_file,
            mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            as_attachment=True,
            download_name=Path(report_file).name
        )

    except KeyError:
        return jsonify({'error': 'Job not found'}), 404


@api_bp.route('/files/<isbn>/package', methods=['GET'])
def download_package_by_isbn(isbn: str):
    """
    Download the output ZIP package by ISBN.

    Looks for files in the Output directory matching the ISBN pattern.
    Prefers _all_fixes.zip, falls back to other variants.

    Response:
        Binary file download (ZIP)
    """
    output_dir = Path(__file__).parent.parent / 'Output'

    # Try different naming patterns in order of preference
    patterns = [
        f'{isbn}_all_fixes.zip',
        f'{isbn}_fixed.zip',
        f'{isbn}.zip',
        f'{isbn}_NEEDS_REVIEW.zip',
    ]

    for pattern in patterns:
        zip_path = output_dir / pattern
        if zip_path.exists():
            return send_file(
                zip_path,
                mimetype='application/zip',
                as_attachment=True,
                download_name=zip_path.name
            )

    # Try glob pattern for variations
    matches = list(output_dir.glob(f'{isbn}*.zip'))
    if matches:
        # Sort by modification time, get most recent
        matches.sort(key=lambda p: p.stat().st_mtime, reverse=True)
        return send_file(
            matches[0],
            mimetype='application/zip',
            as_attachment=True,
            download_name=matches[0].name
        )

    return jsonify({'error': f'No package found for ISBN: {isbn}'}), 404


@api_bp.route('/files/<isbn>/report', methods=['GET'])
def download_report_by_isbn(isbn: str):
    """
    Download the validation report Excel file by ISBN.

    Response:
        Binary file download (Excel)
    """
    output_dir = Path(__file__).parent.parent / 'Output'

    # Try different naming patterns
    patterns = [
        f'{isbn}_validation_report.xlsx',
        f'{isbn}_report.xlsx',
    ]

    for pattern in patterns:
        report_path = output_dir / pattern
        if report_path.exists():
            return send_file(
                report_path,
                mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                as_attachment=True,
                download_name=report_path.name
            )

    # Try glob pattern
    matches = list(output_dir.glob(f'{isbn}*report*.xlsx'))
    if matches:
        matches.sort(key=lambda p: p.stat().st_mtime, reverse=True)
        return send_file(
            matches[0],
            mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            as_attachment=True,
            download_name=matches[0].name
        )

    return jsonify({'error': f'No validation report found for ISBN: {isbn}'}), 404


@api_bp.route('/files/<isbn>', methods=['GET'])
def get_file_info_by_isbn(isbn: str):
    """
    Get information about available files for an ISBN.

    Response:
        {
            "isbn": "9781234567890",
            "files": {
                "package": {"exists": true, "filename": "9781234567890_all_fixes.zip", "size_mb": 2.5},
                "report": {"exists": true, "filename": "9781234567890_validation_report.xlsx", "size_mb": 0.1}
            },
            "download_urls": {
                "package": "/api/v1/files/9781234567890/package",
                "report": "/api/v1/files/9781234567890/report"
            }
        }
    """
    output_dir = Path(__file__).parent.parent / 'Output'

    result = {
        'isbn': isbn,
        'files': {
            'package': {'exists': False, 'filename': None, 'size_mb': None},
            'report': {'exists': False, 'filename': None, 'size_mb': None}
        },
        'download_urls': {
            'package': f'/api/v1/files/{isbn}/package',
            'report': f'/api/v1/files/{isbn}/report'
        }
    }

    # Find package
    package_patterns = [f'{isbn}_all_fixes.zip', f'{isbn}_fixed.zip', f'{isbn}.zip']
    for pattern in package_patterns:
        pkg_path = output_dir / pattern
        if pkg_path.exists():
            result['files']['package'] = {
                'exists': True,
                'filename': pkg_path.name,
                'size_mb': round(pkg_path.stat().st_size / (1024 * 1024), 2)
            }
            break

    # Find report
    report_patterns = [f'{isbn}_validation_report.xlsx', f'{isbn}_report.xlsx']
    for pattern in report_patterns:
        rpt_path = output_dir / pattern
        if rpt_path.exists():
            result['files']['report'] = {
                'exists': True,
                'filename': rpt_path.name,
                'size_mb': round(rpt_path.stat().st_size / (1024 * 1024), 2)
            }
            break

    return jsonify(result)


# =============================================================================
# Utility Endpoints
# =============================================================================

@api_bp.route('/cleanup', methods=['POST'])
@require_json
def cleanup_jobs():
    """
    Clean up old completed/failed jobs.

    Request body:
        {
            "max_age_hours": 24 (optional)
        }

    Response:
        {
            "cleaned": 10,
            "message": "Cleaned up 10 old jobs"
        }
    """
    api = get_api()
    data = request.get_json() or {}
    max_age_hours = data.get('max_age_hours', 24)

    cleaned = api.cleanup_old_jobs(max_age_hours)

    return jsonify({
        'cleaned': cleaned,
        'message': f'Cleaned up {cleaned} old jobs'
    })


# =============================================================================
# Storage Endpoints
# =============================================================================

@api_bp.route('/storage/stats', methods=['GET'])
def storage_stats():
    """
    Get storage statistics.

    Response:
        {
            "total_files": 150,
            "total_size_bytes": 1073741824,
            "total_size_mb": 1024.0,
            "total_isbns": 25,
            "backend": "gridfs",
            "connected": true,
            "details": {...}
        }
    """
    try:
        from src.storage import get_storage

        storage = get_storage()
        stats = storage.get_stats()

        return jsonify({
            'total_files': stats.total_files,
            'total_size_bytes': stats.total_size_bytes,
            'total_size_mb': round(stats.total_size_bytes / (1024 * 1024), 2),
            'total_isbns': stats.total_isbns,
            'backend': stats.backend,
            'connected': stats.connected,
            'details': stats.details
        })

    except ImportError:
        return jsonify({
            'error': 'Storage module not available',
            'total_files': 0,
            'total_size_bytes': 0,
            'backend': 'none',
            'connected': False
        }), 503
    except Exception as e:
        logger.exception("Storage stats error")
        return jsonify({'error': str(e)}), 500


@api_bp.route('/storage/isbn/<isbn>', methods=['GET'])
def storage_list_isbn(isbn: str):
    """
    List all files for a specific ISBN.

    Response:
        {
            "isbn": "9781234567890",
            "files": [
                {
                    "filename": "9781234567890_all_fixes.zip",
                    "size": 1048576,
                    "size_mb": 1.0,
                    "content_type": "application/zip",
                    "created_at": "2025-01-01T00:00:00"
                },
                ...
            ],
            "total_files": 5,
            "total_size_bytes": 5242880
        }
    """
    isbn = validate_id(isbn)
    try:
        from src.storage import get_storage

        storage = get_storage()
        files = storage.list_files(isbn)

        file_list = []
        total_size = 0

        for f in files:
            file_list.append({
                'filename': f.filename,
                'size': f.size,
                'size_mb': round(f.size / (1024 * 1024), 2),
                'content_type': f.content_type,
                'created_at': f.created_at.isoformat() if f.created_at else None,
                'storage_id': f.storage_id
            })
            total_size += f.size

        return jsonify({
            'isbn': isbn,
            'files': file_list,
            'total_files': len(file_list),
            'total_size_bytes': total_size,
            'total_size_mb': round(total_size / (1024 * 1024), 2)
        })

    except ImportError:
        return jsonify({'error': 'Storage module not available'}), 503
    except Exception as e:
        logger.exception(f"Storage list error for ISBN {isbn}")
        return jsonify({'error': str(e)}), 500


@api_bp.route('/storage/isbn/<isbn>/<path:filename>', methods=['GET'])
def storage_download_file(isbn: str, filename: str):
    """
    Download a specific file from storage.

    Returns the file as a binary download.
    """
    isbn = validate_id(isbn)
    try:
        from src.storage import get_storage

        storage = get_storage()

        # Check if file exists
        if not storage.file_exists(isbn, filename):
            return jsonify({'error': 'File not found'}), 404

        # Get file stream
        stream = storage.get_file_stream(isbn, filename)
        if not stream:
            return jsonify({'error': 'Could not open file'}), 500

        # Determine content type
        files = storage.list_files(isbn)
        content_type = 'application/octet-stream'
        for f in files:
            if f.filename == filename:
                content_type = f.content_type
                break

        return send_file(
            stream,
            mimetype=content_type,
            as_attachment=True,
            download_name=filename
        )

    except ImportError:
        return jsonify({'error': 'Storage module not available'}), 503
    except Exception as e:
        logger.exception(f"Storage download error for {isbn}/{filename}")
        return jsonify({'error': str(e)}), 500


@api_bp.route('/storage/isbn/<isbn>/<path:filename>', methods=['DELETE'])
def storage_delete_file(isbn: str, filename: str):
    """
    Delete a specific file from storage.

    Response:
        {
            "deleted": true,
            "isbn": "9781234567890",
            "filename": "file.zip"
        }
    """
    isbn = validate_id(isbn)
    try:
        from src.storage import get_storage

        storage = get_storage()
        deleted = storage.delete_file(isbn, filename)

        if deleted:
            return jsonify({
                'deleted': True,
                'isbn': isbn,
                'filename': filename
            })
        else:
            return jsonify({
                'deleted': False,
                'error': 'File not found'
            }), 404

    except ImportError:
        return jsonify({'error': 'Storage module not available'}), 503
    except Exception as e:
        logger.exception(f"Storage delete error for {isbn}/{filename}")
        return jsonify({'error': str(e)}), 500


@api_bp.route('/storage/isbn/<isbn>', methods=['DELETE'])
def storage_delete_isbn(isbn: str):
    """
    Delete all files for an ISBN from storage.

    Response:
        {
            "deleted_count": 5,
            "isbn": "9781234567890"
        }
    """
    isbn = validate_id(isbn)
    try:
        from src.storage import get_storage

        storage = get_storage()
        deleted_count = storage.delete_isbn(isbn)

        return jsonify({
            'deleted_count': deleted_count,
            'isbn': isbn
        })

    except ImportError:
        return jsonify({'error': 'Storage module not available'}), 503
    except Exception as e:
        logger.exception(f"Storage delete ISBN error for {isbn}")
        return jsonify({'error': str(e)}), 500


# =============================================================================
# App Factory
# =============================================================================

def create_app(config: Optional[dict] = None) -> Flask:
    """
    Create and configure the Flask application.

    Args:
        config: Optional configuration dictionary

    Returns:
        Configured Flask application
    """
    app = Flask(__name__)

    # Default configuration
    app.config.update({
        'MAX_CONTENT_LENGTH': 100 * 1024 * 1024,  # 100MB max upload
        'JSON_SORT_KEYS': False,
    })

    # Override with provided config
    if config:
        app.config.update(config)

    # Register blueprints
    app.register_blueprint(api_bp)

    # Register config blueprint
    try:
        from api.config import config_bp
        app.register_blueprint(config_bp)
        logger.info("Config API blueprint registered")
    except ImportError as e:
        logger.warning(f"Config API not available: {e}")

    # Register authentication blueprint
    if HAS_AUTH:
        try:
            auth_bp = create_auth_management_blueprint()
            app.register_blueprint(auth_bp)
            auth_config = get_auth_config()
            if auth_config.enabled:
                logger.info("Authentication enabled - API key or JWT required")
            else:
                logger.warning("Authentication DISABLED - all endpoints are public")
        except Exception as e:
            logger.warning(f"Auth blueprint registration failed: {e}")
    else:
        logger.warning("Authentication module not available - endpoints are unprotected")

    # Add CORS headers for cross-origin requests
    @app.after_request
    def add_cors_headers(response):
        response.headers['Access-Control-Allow-Origin'] = '*'
        response.headers['Access-Control-Allow-Methods'] = 'GET, POST, PUT, DELETE, OPTIONS'
        response.headers['Access-Control-Allow-Headers'] = 'Content-Type, Authorization, X-API-Key'
        return response

    # Handle OPTIONS requests for CORS preflight
    @app.route('/', defaults={'path': ''}, methods=['OPTIONS'])
    @app.route('/<path:path>', methods=['OPTIONS'])
    def handle_options(path):
        return '', 204

    # Root endpoint
    @app.route('/')
    def index():
        return jsonify({
            'service': 'EPUB Processing API',
            'docs': '/api/v1/info',
            'health': '/api/v1/health'
        })

    logger.info("Flask application created")
    return app


# =============================================================================
# CLI Entry Point
# =============================================================================

def main():
    """Run the API server."""
    import argparse

    parser = argparse.ArgumentParser(description='EPUB Processing API Server')
    parser.add_argument('--host', default='127.0.0.1', help='Host to bind to')
    parser.add_argument('--port', type=int, default=5001, help='Port to bind to')
    parser.add_argument('--debug', action='store_true', help='Enable debug mode')
    args = parser.parse_args()

    # Configure logging
    logging.basicConfig(
        level=logging.DEBUG if args.debug else logging.INFO,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )

    # Suppress verbose third-party library logs (pymongo heartbeats, etc.)
    from src.utils.logger import configure_third_party_loggers
    configure_third_party_loggers()

    app = create_app()

    print(f"\n{'='*60}")
    print(f"  EPUB Processing API Server")
    print(f"{'='*60}")
    print(f"  Running on: http://{args.host}:{args.port}")
    print(f"  API docs:   http://{args.host}:{args.port}/api/v1/info")
    print(f"  Health:     http://{args.host}:{args.port}/api/v1/health")
    print(f"{'='*60}\n")

    app.run(host=args.host, port=args.port, debug=args.debug)


if __name__ == '__main__':
    main()
