#!/usr/bin/env python3
"""
Input Validation Module for RittDocConverter

Provides comprehensive validation for file uploads and inputs:
- Magic byte validation (file type verification)
- File size limits
- ZIP structure validation (EPUB is ZIP)
- Path traversal prevention
- Content safety checks

Usage:
    from input_validator import validate_epub_file, FileValidationError

    try:
        validate_epub_file(file_path)
    except FileValidationError as e:
        print(f"Invalid file: {e}")
"""

import logging
import os
import struct
import zipfile
from dataclasses import dataclass
from pathlib import Path
from typing import BinaryIO, Optional, Tuple, Union

logger = logging.getLogger(__name__)


# =============================================================================
# CONFIGURATION
# =============================================================================

# Maximum file sizes (in bytes)
MAX_EPUB_SIZE_MB = 500  # 500 MB max for EPUB files
MAX_EPUB_SIZE_BYTES = MAX_EPUB_SIZE_MB * 1024 * 1024

MAX_ZIP_ENTRIES = 10000  # Maximum number of entries in the ZIP
MAX_UNCOMPRESSED_SIZE_MB = 2000  # 2 GB max uncompressed size (ZIP bomb protection)
MAX_UNCOMPRESSED_SIZE_BYTES = MAX_UNCOMPRESSED_SIZE_MB * 1024 * 1024

MAX_PATH_LENGTH = 255  # Maximum path length for files inside ZIP
MAX_COMPRESSION_RATIO = 100  # Max compression ratio (ZIP bomb protection)

# Magic bytes for known file types
MAGIC_BYTES = {
    'zip': b'PK\x03\x04',  # ZIP file (also EPUB, DOCX, etc.)
    'zip_empty': b'PK\x05\x06',  # Empty ZIP
    'zip_spanned': b'PK\x07\x08',  # Spanned ZIP
    'pdf': b'%PDF',
    'xml': b'<?xml',
    'html': b'<!DOCTYPE html',
    'html_lower': b'<!doctype html',
}

# Required EPUB files
EPUB_REQUIRED_FILES = [
    'META-INF/container.xml',
]

# Forbidden path patterns (path traversal prevention)
FORBIDDEN_PATH_PATTERNS = [
    '..',
    '~',
    '/etc/',
    '/var/',
    '/usr/',
    '/bin/',
    '/root/',
    'C:\\',
    '\\\\',
]


# =============================================================================
# EXCEPTIONS
# =============================================================================

class FileValidationError(Exception):
    """Base exception for file validation errors."""
    pass


class FileSizeError(FileValidationError):
    """File exceeds size limits."""
    pass


class FileTypeError(FileValidationError):
    """File type is invalid or unsupported."""
    pass


class ZipStructureError(FileValidationError):
    """ZIP structure is invalid or potentially malicious."""
    pass


class PathTraversalError(FileValidationError):
    """Potential path traversal attack detected."""
    pass


class EPUBStructureError(FileValidationError):
    """EPUB structure is invalid."""
    pass


# =============================================================================
# VALIDATION RESULTS
# =============================================================================

@dataclass
class ValidationResult:
    """Result of file validation."""
    is_valid: bool
    file_path: str
    file_size_bytes: int
    file_type: str
    errors: list
    warnings: list
    metadata: dict

    @property
    def file_size_mb(self) -> float:
        return self.file_size_bytes / (1024 * 1024)


# =============================================================================
# MAGIC BYTE VALIDATION
# =============================================================================

def read_magic_bytes(file_path: Union[str, Path], num_bytes: int = 16) -> bytes:
    """
    Read the first N bytes of a file (magic bytes).

    Args:
        file_path: Path to the file
        num_bytes: Number of bytes to read

    Returns:
        First num_bytes of the file
    """
    with open(file_path, 'rb') as f:
        return f.read(num_bytes)


def validate_magic_bytes(file_path: Union[str, Path],
                         expected_type: str = 'zip') -> Tuple[bool, str]:
    """
    Validate file type using magic bytes.

    Args:
        file_path: Path to the file
        expected_type: Expected file type ('zip', 'pdf', 'xml', etc.)

    Returns:
        Tuple of (is_valid, detected_type)
    """
    try:
        magic = read_magic_bytes(file_path, 16)

        # Check for ZIP (EPUB, DOCX, etc.)
        if magic[:4] == MAGIC_BYTES['zip']:
            return (expected_type == 'zip', 'zip')
        elif magic[:4] == MAGIC_BYTES['zip_empty']:
            return (expected_type == 'zip', 'zip_empty')
        elif magic[:4] == MAGIC_BYTES['zip_spanned']:
            return (expected_type == 'zip', 'zip_spanned')

        # Check for PDF
        if magic[:4] == MAGIC_BYTES['pdf']:
            return (expected_type == 'pdf', 'pdf')

        # Check for XML
        if magic[:5] == MAGIC_BYTES['xml']:
            return (expected_type == 'xml', 'xml')

        # Check for HTML
        if magic[:15].lower() == MAGIC_BYTES['html'].lower() or \
           magic[:15].lower() == MAGIC_BYTES['html_lower']:
            return (expected_type == 'html', 'html')

        return (False, 'unknown')

    except IOError as e:
        logger.error(f"Failed to read file {file_path}: {e}")
        return (False, 'error')


# =============================================================================
# SIZE VALIDATION
# =============================================================================

def validate_file_size(file_path: Union[str, Path],
                       max_size_bytes: int = MAX_EPUB_SIZE_BYTES) -> Tuple[bool, int]:
    """
    Validate file size.

    Args:
        file_path: Path to the file
        max_size_bytes: Maximum allowed size in bytes

    Returns:
        Tuple of (is_valid, actual_size_bytes)
    """
    try:
        size = os.path.getsize(file_path)
        return (size <= max_size_bytes, size)
    except OSError as e:
        logger.error(f"Failed to get file size for {file_path}: {e}")
        return (False, 0)


# =============================================================================
# ZIP STRUCTURE VALIDATION
# =============================================================================

def validate_zip_structure(file_path: Union[str, Path]) -> Tuple[bool, list, dict]:
    """
    Validate ZIP file structure for security.

    Checks for:
    - Valid ZIP structure
    - ZIP bomb attacks (high compression ratio)
    - Path traversal in filenames
    - Excessive number of entries
    - Excessive uncompressed size

    Args:
        file_path: Path to the ZIP file

    Returns:
        Tuple of (is_valid, errors, metadata)
    """
    errors = []
    metadata = {
        'entry_count': 0,
        'total_compressed_size': 0,
        'total_uncompressed_size': 0,
        'compression_ratio': 0,
        'has_encrypted_files': False,
    }

    try:
        with zipfile.ZipFile(file_path, 'r') as zf:
            # Check for corruption
            bad_file = zf.testzip()
            if bad_file:
                errors.append(f"Corrupted file in ZIP: {bad_file}")
                return (False, errors, metadata)

            entries = zf.infolist()
            metadata['entry_count'] = len(entries)

            # Check entry count
            if len(entries) > MAX_ZIP_ENTRIES:
                errors.append(f"Too many entries in ZIP: {len(entries)} (max: {MAX_ZIP_ENTRIES})")

            total_compressed = 0
            total_uncompressed = 0

            for entry in entries:
                # Check for path traversal
                if any(pattern in entry.filename for pattern in FORBIDDEN_PATH_PATTERNS):
                    errors.append(f"Potential path traversal in ZIP entry: {entry.filename}")

                # Check path length
                if len(entry.filename) > MAX_PATH_LENGTH:
                    errors.append(f"Path too long in ZIP: {entry.filename[:50]}...")

                # Check for encrypted files
                if entry.flag_bits & 0x1:
                    metadata['has_encrypted_files'] = True

                total_compressed += entry.compress_size
                total_uncompressed += entry.file_size

            metadata['total_compressed_size'] = total_compressed
            metadata['total_uncompressed_size'] = total_uncompressed

            # Check total uncompressed size
            if total_uncompressed > MAX_UNCOMPRESSED_SIZE_BYTES:
                errors.append(
                    f"Uncompressed size too large: {total_uncompressed / (1024*1024):.1f}MB "
                    f"(max: {MAX_UNCOMPRESSED_SIZE_MB}MB)"
                )

            # Check compression ratio (ZIP bomb protection)
            if total_compressed > 0:
                ratio = total_uncompressed / total_compressed
                metadata['compression_ratio'] = ratio

                if ratio > MAX_COMPRESSION_RATIO:
                    errors.append(
                        f"Suspicious compression ratio: {ratio:.1f}x (max: {MAX_COMPRESSION_RATIO}x)"
                    )

            return (len(errors) == 0, errors, metadata)

    except zipfile.BadZipFile as e:
        errors.append(f"Invalid ZIP file: {e}")
        return (False, errors, metadata)
    except Exception as e:
        errors.append(f"Error validating ZIP: {e}")
        return (False, errors, metadata)


# =============================================================================
# EPUB-SPECIFIC VALIDATION
# =============================================================================

def validate_epub_structure(file_path: Union[str, Path]) -> Tuple[bool, list, dict]:
    """
    Validate EPUB-specific structure.

    Checks for:
    - Required EPUB files (container.xml)
    - Valid OPF file
    - Proper mimetype file

    Args:
        file_path: Path to the EPUB file

    Returns:
        Tuple of (is_valid, errors, metadata)
    """
    errors = []
    metadata = {
        'has_container_xml': False,
        'has_opf': False,
        'has_mimetype': False,
        'opf_path': None,
        'epub_version': None,
    }

    try:
        with zipfile.ZipFile(file_path, 'r') as zf:
            names = zf.namelist()

            # Check for mimetype (should be first uncompressed entry)
            if 'mimetype' in names:
                metadata['has_mimetype'] = True
                try:
                    mimetype = zf.read('mimetype').decode('utf-8').strip()
                    if mimetype != 'application/epub+zip':
                        errors.append(f"Invalid mimetype: {mimetype}")
                except Exception:
                    errors.append("Failed to read mimetype file")

            # Check for container.xml
            if 'META-INF/container.xml' in names:
                metadata['has_container_xml'] = True
            else:
                errors.append("Missing META-INF/container.xml")

            # Find and validate OPF file
            opf_files = [n for n in names if n.endswith('.opf')]
            if opf_files:
                metadata['has_opf'] = True
                metadata['opf_path'] = opf_files[0]
            else:
                errors.append("No .opf file found in EPUB")

            return (len(errors) == 0, errors, metadata)

    except zipfile.BadZipFile as e:
        errors.append(f"Invalid ZIP/EPUB file: {e}")
        return (False, errors, metadata)
    except Exception as e:
        errors.append(f"Error validating EPUB structure: {e}")
        return (False, errors, metadata)


# =============================================================================
# PATH VALIDATION
# =============================================================================

def validate_path_safety(path: Union[str, Path]) -> Tuple[bool, Optional[str]]:
    """
    Validate path for security (prevent path traversal).

    Args:
        path: Path to validate

    Returns:
        Tuple of (is_safe, error_message)
    """
    path_str = str(path)

    # Check for forbidden patterns
    for pattern in FORBIDDEN_PATH_PATTERNS:
        if pattern in path_str:
            return (False, f"Forbidden pattern in path: {pattern}")

    # Check for null bytes
    if '\x00' in path_str:
        return (False, "Null byte in path")

    # Resolve and compare to detect traversal
    try:
        original = Path(path).resolve()
        # Path should not escape working directory for relative paths
        if path_str.startswith('.'):
            cwd = Path.cwd().resolve()
            if not str(original).startswith(str(cwd)):
                return (False, "Path escapes working directory")
    except Exception as e:
        return (False, f"Path resolution error: {e}")

    return (True, None)


# =============================================================================
# COMPREHENSIVE VALIDATION
# =============================================================================

def validate_epub_file(file_path: Union[str, Path],
                       max_size_mb: Optional[int] = None) -> ValidationResult:
    """
    Comprehensive validation of an EPUB file.

    Performs all security and structure checks:
    1. Path safety validation
    2. File size validation
    3. Magic byte validation
    4. ZIP structure validation
    5. EPUB structure validation

    Args:
        file_path: Path to the EPUB file
        max_size_mb: Optional custom max size in MB

    Returns:
        ValidationResult with all findings

    Raises:
        FileValidationError: If critical validation fails
    """
    file_path = Path(file_path)
    errors = []
    warnings = []
    metadata = {}

    # 1. Check file exists
    if not file_path.exists():
        raise FileValidationError(f"File not found: {file_path}")

    if not file_path.is_file():
        raise FileValidationError(f"Not a file: {file_path}")

    # 2. Path safety validation
    is_safe, path_error = validate_path_safety(file_path)
    if not is_safe:
        raise PathTraversalError(path_error)

    # 3. File size validation
    max_size = (max_size_mb * 1024 * 1024) if max_size_mb else MAX_EPUB_SIZE_BYTES
    size_valid, file_size = validate_file_size(file_path, max_size)
    metadata['file_size_bytes'] = file_size

    if not size_valid:
        raise FileSizeError(
            f"File too large: {file_size / (1024*1024):.1f}MB "
            f"(max: {max_size / (1024*1024):.0f}MB)"
        )

    # 4. Magic byte validation
    magic_valid, detected_type = validate_magic_bytes(file_path, 'zip')
    metadata['detected_type'] = detected_type

    if not magic_valid:
        raise FileTypeError(
            f"Invalid file type: expected EPUB (ZIP), detected {detected_type}"
        )

    # 5. ZIP structure validation
    zip_valid, zip_errors, zip_metadata = validate_zip_structure(file_path)
    metadata.update(zip_metadata)

    if not zip_valid:
        # Some ZIP errors are critical, others are warnings
        for error in zip_errors:
            if 'corruption' in error.lower() or 'traversal' in error.lower():
                raise ZipStructureError(error)
            elif 'compression ratio' in error.lower():
                raise ZipStructureError(f"Potential ZIP bomb: {error}")
            else:
                errors.append(error)

    if zip_metadata.get('has_encrypted_files'):
        warnings.append("EPUB contains encrypted files")

    # 6. EPUB structure validation
    epub_valid, epub_errors, epub_metadata = validate_epub_structure(file_path)
    metadata.update(epub_metadata)

    if not epub_valid:
        for error in epub_errors:
            if 'container.xml' in error:
                raise EPUBStructureError(error)
            else:
                warnings.append(error)

    # Determine overall validity
    is_valid = len(errors) == 0

    return ValidationResult(
        is_valid=is_valid,
        file_path=str(file_path),
        file_size_bytes=file_size,
        file_type='epub',
        errors=errors,
        warnings=warnings,
        metadata=metadata
    )


def validate_file_upload(file_obj: BinaryIO,
                         filename: str,
                         max_size_mb: Optional[int] = None) -> ValidationResult:
    """
    Validate a file upload from a web request.

    This reads from a file-like object (e.g., Flask's FileStorage)
    and performs comprehensive validation.

    Args:
        file_obj: File-like object to validate
        filename: Original filename
        max_size_mb: Optional max size in MB

    Returns:
        ValidationResult with all findings
    """
    import tempfile

    errors = []
    warnings = []

    # Validate filename
    if not filename:
        raise FileValidationError("No filename provided")

    # Check extension
    ext = Path(filename).suffix.lower()
    if ext not in {'.epub', '.epub3'}:
        raise FileTypeError(f"Invalid file extension: {ext}")

    # Save to temp file for full validation
    with tempfile.NamedTemporaryFile(suffix=ext, delete=False) as tmp:
        tmp_path = Path(tmp.name)
        try:
            # Copy content to temp file
            file_obj.seek(0)
            chunk_size = 8192
            total_size = 0
            max_size = (max_size_mb * 1024 * 1024) if max_size_mb else MAX_EPUB_SIZE_BYTES

            while True:
                chunk = file_obj.read(chunk_size)
                if not chunk:
                    break
                total_size += len(chunk)
                if total_size > max_size:
                    raise FileSizeError(
                        f"File too large during upload (>{max_size / (1024*1024):.0f}MB)"
                    )
                tmp.write(chunk)

            tmp.flush()

            # Perform full validation
            result = validate_epub_file(tmp_path, max_size_mb)
            result.file_path = filename  # Use original filename in result

            return result

        finally:
            # Clean up temp file
            try:
                tmp_path.unlink()
            except Exception:
                pass


# =============================================================================
# CONVENIENCE FUNCTIONS
# =============================================================================

def is_valid_epub(file_path: Union[str, Path]) -> bool:
    """
    Quick check if a file is a valid EPUB.

    Args:
        file_path: Path to check

    Returns:
        True if valid, False otherwise
    """
    try:
        result = validate_epub_file(file_path)
        return result.is_valid
    except FileValidationError:
        return False


def get_epub_info(file_path: Union[str, Path]) -> dict:
    """
    Get information about an EPUB file.

    Args:
        file_path: Path to the EPUB file

    Returns:
        Dictionary with file information
    """
    try:
        result = validate_epub_file(file_path)
        return {
            'valid': result.is_valid,
            'size_mb': result.file_size_mb,
            'errors': result.errors,
            'warnings': result.warnings,
            **result.metadata
        }
    except FileValidationError as e:
        return {
            'valid': False,
            'error': str(e)
        }
