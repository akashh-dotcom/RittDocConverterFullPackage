"""
Storage module for RittDocConverter.

Provides abstract storage interface with multiple backends:
- GridFSBackend: MongoDB GridFS (default)
- S3Backend: AWS S3 (future-ready)
- LocalBackend: Local filesystem (fallback)
"""

from .storage import (
    StorageBackend,
    StoredFile,
    StorageStats,
    GridFSBackend,
    S3Backend,
    LocalBackend,
    get_storage,
    reset_storage,
)

__all__ = [
    'StorageBackend',
    'StoredFile',
    'StorageStats',
    'GridFSBackend',
    'S3Backend',
    'LocalBackend',
    'get_storage',
    'reset_storage',
]
