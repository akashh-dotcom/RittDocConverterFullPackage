"""
Storage Abstraction Layer

Provides a unified interface for file storage with multiple backends:
- GridFSBackend: MongoDB GridFS (default, recommended)
- S3Backend: AWS S3 (future-ready)
- LocalBackend: Local filesystem (fallback)

All files are organized by ISBN for easy retrieval.
"""

import io
import logging
import os
import shutil
from abc import ABC, abstractmethod
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Any, BinaryIO, Dict, Generator, List, Optional, Union

logger = logging.getLogger(__name__)


@dataclass
class StoredFile:
    """Represents a stored file with metadata."""
    filename: str
    isbn: str
    size: int
    content_type: str
    created_at: datetime
    metadata: Dict[str, Any]
    storage_id: str  # Backend-specific identifier


@dataclass
class StorageStats:
    """Storage statistics."""
    total_files: int
    total_size_bytes: int
    total_isbns: int
    backend: str
    connected: bool
    details: Dict[str, Any]


class StorageBackend(ABC):
    """Abstract base class for storage backends."""

    @abstractmethod
    def connect(self) -> bool:
        """Establish connection to storage backend."""
        pass

    @abstractmethod
    def is_connected(self) -> bool:
        """Check if connected to storage backend."""
        pass

    @abstractmethod
    def save_file(
        self,
        isbn: str,
        filename: str,
        data: Union[bytes, BinaryIO],
        content_type: str = "application/octet-stream",
        metadata: Optional[Dict[str, Any]] = None
    ) -> StoredFile:
        """
        Save a file to storage.

        Args:
            isbn: ISBN identifier for grouping files
            filename: Name of the file
            data: File content (bytes or file-like object)
            content_type: MIME type of the file
            metadata: Additional metadata to store

        Returns:
            StoredFile object with storage details
        """
        pass

    @abstractmethod
    def get_file(self, isbn: str, filename: str) -> Optional[bytes]:
        """
        Retrieve a file from storage.

        Args:
            isbn: ISBN identifier
            filename: Name of the file

        Returns:
            File content as bytes, or None if not found
        """
        pass

    @abstractmethod
    def get_file_stream(self, isbn: str, filename: str) -> Optional[BinaryIO]:
        """
        Get a streaming file handle for large files.

        Args:
            isbn: ISBN identifier
            filename: Name of the file

        Returns:
            File-like object for streaming, or None if not found
        """
        pass

    @abstractmethod
    def delete_file(self, isbn: str, filename: str) -> bool:
        """
        Delete a file from storage.

        Args:
            isbn: ISBN identifier
            filename: Name of the file

        Returns:
            True if deleted, False if not found
        """
        pass

    @abstractmethod
    def list_files(self, isbn: str) -> List[StoredFile]:
        """
        List all files for an ISBN.

        Args:
            isbn: ISBN identifier

        Returns:
            List of StoredFile objects
        """
        pass

    @abstractmethod
    def file_exists(self, isbn: str, filename: str) -> bool:
        """
        Check if a file exists.

        Args:
            isbn: ISBN identifier
            filename: Name of the file

        Returns:
            True if exists, False otherwise
        """
        pass

    @abstractmethod
    def get_stats(self) -> StorageStats:
        """Get storage statistics."""
        pass

    @abstractmethod
    def delete_isbn(self, isbn: str) -> int:
        """
        Delete all files for an ISBN.

        Args:
            isbn: ISBN identifier

        Returns:
            Number of files deleted
        """
        pass

    def save_directory(
        self,
        isbn: str,
        directory: Path,
        prefix: str = ""
    ) -> List[StoredFile]:
        """
        Save all files from a directory to storage.

        Args:
            isbn: ISBN identifier
            directory: Path to directory
            prefix: Optional prefix for filenames

        Returns:
            List of StoredFile objects
        """
        saved = []
        directory = Path(directory)

        if not directory.exists():
            logger.warning(f"Directory not found: {directory}")
            return saved

        for file_path in directory.rglob("*"):
            if file_path.is_file():
                relative_path = file_path.relative_to(directory)
                filename = f"{prefix}{relative_path}" if prefix else str(relative_path)

                # Determine content type
                content_type = self._guess_content_type(file_path.suffix)

                with open(file_path, "rb") as f:
                    stored = self.save_file(
                        isbn=isbn,
                        filename=filename,
                        data=f,
                        content_type=content_type,
                        metadata={"original_path": str(file_path)}
                    )
                    saved.append(stored)

        return saved

    def _guess_content_type(self, suffix: str) -> str:
        """Guess MIME type from file extension."""
        content_types = {
            ".xml": "application/xml",
            ".zip": "application/zip",
            ".pdf": "application/pdf",
            ".epub": "application/epub+zip",
            ".png": "image/png",
            ".jpg": "image/jpeg",
            ".jpeg": "image/jpeg",
            ".gif": "image/gif",
            ".svg": "image/svg+xml",
            ".html": "text/html",
            ".css": "text/css",
            ".js": "application/javascript",
            ".json": "application/json",
            ".txt": "text/plain",
            ".xlsx": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        }
        return content_types.get(suffix.lower(), "application/octet-stream")


class GridFSBackend(StorageBackend):
    """
    MongoDB GridFS storage backend.

    Stores files in MongoDB GridFS, organized by ISBN.
    Recommended for production use with MongoDB infrastructure.
    """

    def __init__(
        self,
        uri: Optional[str] = None,
        database: str = "rittdoc_storage",
        bucket_name: str = "files"
    ):
        """
        Initialize GridFS backend.

        Args:
            uri: MongoDB connection URI
            database: Database name
            bucket_name: GridFS bucket name
        """
        self.uri = uri or os.getenv("MONGODB_URI", "mongodb://mongodb:27017")
        self.database_name = database
        self.bucket_name = bucket_name

        self._client = None
        self._db = None
        self._bucket = None
        self._connected = False

    def connect(self) -> bool:
        """Connect to MongoDB and initialize GridFS bucket."""
        try:
            from pymongo import MongoClient
            from gridfs import GridFS, GridFSBucket

            self._client = MongoClient(self.uri, serverSelectionTimeoutMS=5000)
            # Test connection
            self._client.admin.command('ping')

            self._db = self._client[self.database_name]
            self._bucket = GridFSBucket(self._db, bucket_name=self.bucket_name)
            self._connected = True

            logger.info(f"GridFS connected to {self.database_name}/{self.bucket_name}")
            return True

        except Exception as e:
            logger.error(f"GridFS connection failed: {e}")
            self._connected = False
            return False

    def is_connected(self) -> bool:
        """Check if connected to MongoDB."""
        if not self._connected or not self._client:
            return False

        try:
            self._client.admin.command('ping')
            return True
        except Exception:
            self._connected = False
            return False

    def save_file(
        self,
        isbn: str,
        filename: str,
        data: Union[bytes, BinaryIO],
        content_type: str = "application/octet-stream",
        metadata: Optional[Dict[str, Any]] = None
    ) -> StoredFile:
        """Save file to GridFS."""
        if not self.is_connected():
            raise ConnectionError("Not connected to GridFS")

        # Build metadata
        file_metadata = {
            "isbn": isbn,
            "content_type": content_type,
            "created_at": datetime.utcnow(),
            **(metadata or {})
        }

        # Delete existing file with same name/isbn if exists
        self._delete_existing(isbn, filename)

        # Convert bytes to file-like object if needed
        if isinstance(data, bytes):
            data = io.BytesIO(data)

        # Get file size
        data.seek(0, 2)
        size = data.tell()
        data.seek(0)

        # Upload to GridFS
        file_id = self._bucket.upload_from_stream(
            filename=f"{isbn}/{filename}",
            source=data,
            metadata=file_metadata
        )

        return StoredFile(
            filename=filename,
            isbn=isbn,
            size=size,
            content_type=content_type,
            created_at=file_metadata["created_at"],
            metadata=file_metadata,
            storage_id=str(file_id)
        )

    def _delete_existing(self, isbn: str, filename: str) -> None:
        """Delete existing file if it exists."""
        try:
            full_filename = f"{isbn}/{filename}"
            cursor = self._bucket.find({"filename": full_filename})
            for doc in cursor:
                self._bucket.delete(doc._id)
        except Exception:
            pass

    def get_file(self, isbn: str, filename: str) -> Optional[bytes]:
        """Retrieve file from GridFS."""
        if not self.is_connected():
            return None

        try:
            full_filename = f"{isbn}/{filename}"
            grid_out = self._bucket.open_download_stream_by_name(full_filename)
            return grid_out.read()
        except Exception as e:
            logger.debug(f"File not found: {isbn}/{filename}: {e}")
            return None

    def get_file_stream(self, isbn: str, filename: str) -> Optional[BinaryIO]:
        """Get streaming file handle from GridFS."""
        if not self.is_connected():
            return None

        try:
            full_filename = f"{isbn}/{filename}"
            return self._bucket.open_download_stream_by_name(full_filename)
        except Exception:
            return None

    def delete_file(self, isbn: str, filename: str) -> bool:
        """Delete file from GridFS."""
        if not self.is_connected():
            return False

        try:
            full_filename = f"{isbn}/{filename}"
            cursor = self._bucket.find({"filename": full_filename})
            deleted = False
            for doc in cursor:
                self._bucket.delete(doc._id)
                deleted = True
            return deleted
        except Exception as e:
            logger.error(f"Failed to delete {isbn}/{filename}: {e}")
            return False

    def list_files(self, isbn: str) -> List[StoredFile]:
        """List all files for an ISBN in GridFS."""
        if not self.is_connected():
            return []

        files = []
        try:
            # Find files with ISBN prefix
            cursor = self._bucket.find({"filename": {"$regex": f"^{isbn}/"}})
            for doc in cursor:
                # Extract filename without ISBN prefix
                filename = doc.filename.split("/", 1)[1] if "/" in doc.filename else doc.filename
                metadata = doc.metadata or {}

                files.append(StoredFile(
                    filename=filename,
                    isbn=isbn,
                    size=doc.length,
                    content_type=metadata.get("content_type", "application/octet-stream"),
                    created_at=metadata.get("created_at", doc.upload_date),
                    metadata=metadata,
                    storage_id=str(doc._id)
                ))
        except Exception as e:
            logger.error(f"Failed to list files for {isbn}: {e}")

        return files

    def file_exists(self, isbn: str, filename: str) -> bool:
        """Check if file exists in GridFS."""
        if not self.is_connected():
            return False

        try:
            full_filename = f"{isbn}/{filename}"
            cursor = self._bucket.find({"filename": full_filename})
            return cursor.count() > 0
        except Exception:
            return False

    def get_stats(self) -> StorageStats:
        """Get GridFS storage statistics."""
        if not self.is_connected():
            return StorageStats(
                total_files=0,
                total_size_bytes=0,
                total_isbns=0,
                backend="gridfs",
                connected=False,
                details={"error": "Not connected"}
            )

        try:
            # Get file stats
            files_collection = self._db[f"{self.bucket_name}.files"]
            chunks_collection = self._db[f"{self.bucket_name}.chunks"]

            total_files = files_collection.count_documents({})

            # Aggregate total size
            pipeline = [{"$group": {"_id": None, "total": {"$sum": "$length"}}}]
            result = list(files_collection.aggregate(pipeline))
            total_size = result[0]["total"] if result else 0

            # Count unique ISBNs
            isbn_pipeline = [
                {"$group": {"_id": "$metadata.isbn"}},
                {"$count": "total"}
            ]
            isbn_result = list(files_collection.aggregate(isbn_pipeline))
            total_isbns = isbn_result[0]["total"] if isbn_result else 0

            return StorageStats(
                total_files=total_files,
                total_size_bytes=total_size,
                total_isbns=total_isbns,
                backend="gridfs",
                connected=True,
                details={
                    "database": self.database_name,
                    "bucket": self.bucket_name,
                    "chunks_count": chunks_collection.count_documents({})
                }
            )
        except Exception as e:
            return StorageStats(
                total_files=0,
                total_size_bytes=0,
                total_isbns=0,
                backend="gridfs",
                connected=False,
                details={"error": str(e)}
            )

    def delete_isbn(self, isbn: str) -> int:
        """Delete all files for an ISBN from GridFS."""
        if not self.is_connected():
            return 0

        deleted = 0
        try:
            cursor = self._bucket.find({"filename": {"$regex": f"^{isbn}/"}})
            for doc in cursor:
                self._bucket.delete(doc._id)
                deleted += 1
        except Exception as e:
            logger.error(f"Failed to delete ISBN {isbn}: {e}")

        return deleted


class S3Backend(StorageBackend):
    """
    AWS S3 storage backend.

    Stores files in S3, organized by ISBN prefix.
    Future-ready for cloud migration.
    """

    def __init__(
        self,
        bucket_name: Optional[str] = None,
        region: str = "us-east-1",
        access_key: Optional[str] = None,
        secret_key: Optional[str] = None
    ):
        """
        Initialize S3 backend.

        Args:
            bucket_name: S3 bucket name
            region: AWS region
            access_key: AWS access key (or use env/IAM role)
            secret_key: AWS secret key (or use env/IAM role)
        """
        self.bucket_name = bucket_name or os.getenv("S3_BUCKET", "rittdoc-storage")
        self.region = region or os.getenv("AWS_REGION", "us-east-1")
        self.access_key = access_key or os.getenv("AWS_ACCESS_KEY_ID")
        self.secret_key = secret_key or os.getenv("AWS_SECRET_ACCESS_KEY")

        self._client = None
        self._connected = False

    def connect(self) -> bool:
        """Connect to S3."""
        try:
            import boto3
            from botocore.exceptions import ClientError

            self._client = boto3.client(
                's3',
                region_name=self.region,
                aws_access_key_id=self.access_key,
                aws_secret_access_key=self.secret_key
            )

            # Test connection by checking bucket exists
            self._client.head_bucket(Bucket=self.bucket_name)
            self._connected = True

            logger.info(f"S3 connected to bucket: {self.bucket_name}")
            return True

        except ImportError:
            logger.error("boto3 not installed. Install with: pip install boto3")
            return False
        except Exception as e:
            logger.error(f"S3 connection failed: {e}")
            self._connected = False
            return False

    def is_connected(self) -> bool:
        """Check if connected to S3."""
        return self._connected and self._client is not None

    def save_file(
        self,
        isbn: str,
        filename: str,
        data: Union[bytes, BinaryIO],
        content_type: str = "application/octet-stream",
        metadata: Optional[Dict[str, Any]] = None
    ) -> StoredFile:
        """Save file to S3."""
        if not self.is_connected():
            raise ConnectionError("Not connected to S3")

        key = f"{isbn}/{filename}"

        # Convert to bytes if file-like
        if hasattr(data, 'read'):
            data.seek(0)
            file_data = data.read()
        else:
            file_data = data

        size = len(file_data)

        # Build S3 metadata (must be string values)
        s3_metadata = {"isbn": isbn}
        if metadata:
            for k, v in metadata.items():
                s3_metadata[k] = str(v)

        self._client.put_object(
            Bucket=self.bucket_name,
            Key=key,
            Body=file_data,
            ContentType=content_type,
            Metadata=s3_metadata
        )

        created_at = datetime.utcnow()

        return StoredFile(
            filename=filename,
            isbn=isbn,
            size=size,
            content_type=content_type,
            created_at=created_at,
            metadata=metadata or {},
            storage_id=key
        )

    def get_file(self, isbn: str, filename: str) -> Optional[bytes]:
        """Retrieve file from S3."""
        if not self.is_connected():
            return None

        try:
            key = f"{isbn}/{filename}"
            response = self._client.get_object(Bucket=self.bucket_name, Key=key)
            return response['Body'].read()
        except Exception as e:
            logger.debug(f"File not found: {isbn}/{filename}: {e}")
            return None

    def get_file_stream(self, isbn: str, filename: str) -> Optional[BinaryIO]:
        """Get streaming file handle from S3."""
        if not self.is_connected():
            return None

        try:
            key = f"{isbn}/{filename}"
            response = self._client.get_object(Bucket=self.bucket_name, Key=key)
            return response['Body']
        except Exception:
            return None

    def delete_file(self, isbn: str, filename: str) -> bool:
        """Delete file from S3."""
        if not self.is_connected():
            return False

        try:
            key = f"{isbn}/{filename}"
            self._client.delete_object(Bucket=self.bucket_name, Key=key)
            return True
        except Exception as e:
            logger.error(f"Failed to delete {isbn}/{filename}: {e}")
            return False

    def list_files(self, isbn: str) -> List[StoredFile]:
        """List all files for an ISBN in S3."""
        if not self.is_connected():
            return []

        files = []
        try:
            prefix = f"{isbn}/"
            paginator = self._client.get_paginator('list_objects_v2')

            for page in paginator.paginate(Bucket=self.bucket_name, Prefix=prefix):
                for obj in page.get('Contents', []):
                    key = obj['Key']
                    filename = key.split("/", 1)[1] if "/" in key else key

                    files.append(StoredFile(
                        filename=filename,
                        isbn=isbn,
                        size=obj['Size'],
                        content_type="application/octet-stream",  # Would need HEAD request for actual type
                        created_at=obj['LastModified'],
                        metadata={},
                        storage_id=key
                    ))
        except Exception as e:
            logger.error(f"Failed to list files for {isbn}: {e}")

        return files

    def file_exists(self, isbn: str, filename: str) -> bool:
        """Check if file exists in S3."""
        if not self.is_connected():
            return False

        try:
            key = f"{isbn}/{filename}"
            self._client.head_object(Bucket=self.bucket_name, Key=key)
            return True
        except Exception:
            return False

    def get_stats(self) -> StorageStats:
        """Get S3 storage statistics."""
        if not self.is_connected():
            return StorageStats(
                total_files=0,
                total_size_bytes=0,
                total_isbns=0,
                backend="s3",
                connected=False,
                details={"error": "Not connected"}
            )

        try:
            total_files = 0
            total_size = 0
            isbns = set()

            paginator = self._client.get_paginator('list_objects_v2')
            for page in paginator.paginate(Bucket=self.bucket_name):
                for obj in page.get('Contents', []):
                    total_files += 1
                    total_size += obj['Size']
                    # Extract ISBN from key prefix
                    if '/' in obj['Key']:
                        isbn = obj['Key'].split('/')[0]
                        isbns.add(isbn)

            return StorageStats(
                total_files=total_files,
                total_size_bytes=total_size,
                total_isbns=len(isbns),
                backend="s3",
                connected=True,
                details={
                    "bucket": self.bucket_name,
                    "region": self.region
                }
            )
        except Exception as e:
            return StorageStats(
                total_files=0,
                total_size_bytes=0,
                total_isbns=0,
                backend="s3",
                connected=False,
                details={"error": str(e)}
            )

    def delete_isbn(self, isbn: str) -> int:
        """Delete all files for an ISBN from S3."""
        if not self.is_connected():
            return 0

        deleted = 0
        try:
            prefix = f"{isbn}/"
            paginator = self._client.get_paginator('list_objects_v2')

            objects_to_delete = []
            for page in paginator.paginate(Bucket=self.bucket_name, Prefix=prefix):
                for obj in page.get('Contents', []):
                    objects_to_delete.append({'Key': obj['Key']})

            if objects_to_delete:
                # S3 batch delete (max 1000 at a time)
                for i in range(0, len(objects_to_delete), 1000):
                    batch = objects_to_delete[i:i+1000]
                    self._client.delete_objects(
                        Bucket=self.bucket_name,
                        Delete={'Objects': batch}
                    )
                    deleted += len(batch)

        except Exception as e:
            logger.error(f"Failed to delete ISBN {isbn}: {e}")

        return deleted


class LocalBackend(StorageBackend):
    """
    Local filesystem storage backend.

    Stores files in a local directory, organized by ISBN.
    Used as fallback when other backends are unavailable.
    """

    def __init__(self, base_path: Optional[str] = None):
        """
        Initialize local storage backend.

        Args:
            base_path: Base directory for storage
        """
        self.base_path = Path(base_path or os.getenv("LOCAL_STORAGE_PATH", "/app/storage"))
        self.base_path.mkdir(parents=True, exist_ok=True)
        self._connected = True

    def connect(self) -> bool:
        """Ensure storage directory exists."""
        try:
            self.base_path.mkdir(parents=True, exist_ok=True)
            self._connected = True
            logger.info(f"Local storage initialized at: {self.base_path}")
            return True
        except Exception as e:
            logger.error(f"Local storage initialization failed: {e}")
            self._connected = False
            return False

    def is_connected(self) -> bool:
        """Check if storage directory is accessible."""
        return self._connected and self.base_path.exists()

    def _get_file_path(self, isbn: str, filename: str) -> Path:
        """Get full path for a file."""
        isbn_dir = self.base_path / isbn
        isbn_dir.mkdir(parents=True, exist_ok=True)
        return isbn_dir / filename

    def save_file(
        self,
        isbn: str,
        filename: str,
        data: Union[bytes, BinaryIO],
        content_type: str = "application/octet-stream",
        metadata: Optional[Dict[str, Any]] = None
    ) -> StoredFile:
        """Save file to local filesystem."""
        file_path = self._get_file_path(isbn, filename)

        # Ensure parent directories exist
        file_path.parent.mkdir(parents=True, exist_ok=True)

        # Write file
        if isinstance(data, bytes):
            file_path.write_bytes(data)
            size = len(data)
        else:
            data.seek(0)
            file_path.write_bytes(data.read())
            size = file_path.stat().st_size

        created_at = datetime.utcnow()

        # Save metadata as JSON sidecar file
        if metadata:
            import json
            meta_path = file_path.with_suffix(file_path.suffix + ".meta.json")
            meta_path.write_text(json.dumps({
                "content_type": content_type,
                "created_at": created_at.isoformat(),
                **metadata
            }))

        return StoredFile(
            filename=filename,
            isbn=isbn,
            size=size,
            content_type=content_type,
            created_at=created_at,
            metadata=metadata or {},
            storage_id=str(file_path)
        )

    def get_file(self, isbn: str, filename: str) -> Optional[bytes]:
        """Retrieve file from local filesystem."""
        file_path = self._get_file_path(isbn, filename)
        if file_path.exists():
            return file_path.read_bytes()
        return None

    def get_file_stream(self, isbn: str, filename: str) -> Optional[BinaryIO]:
        """Get file handle from local filesystem."""
        file_path = self._get_file_path(isbn, filename)
        if file_path.exists():
            return open(file_path, 'rb')
        return None

    def delete_file(self, isbn: str, filename: str) -> bool:
        """Delete file from local filesystem."""
        file_path = self._get_file_path(isbn, filename)
        if file_path.exists():
            file_path.unlink()
            # Also delete metadata file if exists
            meta_path = file_path.with_suffix(file_path.suffix + ".meta.json")
            if meta_path.exists():
                meta_path.unlink()
            return True
        return False

    def list_files(self, isbn: str) -> List[StoredFile]:
        """List all files for an ISBN in local filesystem."""
        files = []
        isbn_dir = self.base_path / isbn

        if not isbn_dir.exists():
            return files

        for file_path in isbn_dir.rglob("*"):
            if file_path.is_file() and not file_path.suffix.endswith(".meta.json"):
                # Try to load metadata
                metadata = {}
                content_type = self._guess_content_type(file_path.suffix)
                created_at = datetime.fromtimestamp(file_path.stat().st_mtime)

                meta_path = file_path.with_suffix(file_path.suffix + ".meta.json")
                if meta_path.exists():
                    import json
                    try:
                        meta = json.loads(meta_path.read_text())
                        content_type = meta.pop("content_type", content_type)
                        if "created_at" in meta:
                            created_at = datetime.fromisoformat(meta.pop("created_at"))
                        metadata = meta
                    except Exception:
                        pass

                relative_path = file_path.relative_to(isbn_dir)

                files.append(StoredFile(
                    filename=str(relative_path),
                    isbn=isbn,
                    size=file_path.stat().st_size,
                    content_type=content_type,
                    created_at=created_at,
                    metadata=metadata,
                    storage_id=str(file_path)
                ))

        return files

    def file_exists(self, isbn: str, filename: str) -> bool:
        """Check if file exists in local filesystem."""
        return self._get_file_path(isbn, filename).exists()

    def get_stats(self) -> StorageStats:
        """Get local storage statistics."""
        total_files = 0
        total_size = 0
        isbns = set()

        try:
            for isbn_dir in self.base_path.iterdir():
                if isbn_dir.is_dir():
                    isbns.add(isbn_dir.name)
                    for file_path in isbn_dir.rglob("*"):
                        if file_path.is_file() and not file_path.suffix.endswith(".meta.json"):
                            total_files += 1
                            total_size += file_path.stat().st_size

            return StorageStats(
                total_files=total_files,
                total_size_bytes=total_size,
                total_isbns=len(isbns),
                backend="local",
                connected=True,
                details={
                    "base_path": str(self.base_path),
                    "free_space_gb": shutil.disk_usage(self.base_path).free / (1024**3)
                }
            )
        except Exception as e:
            return StorageStats(
                total_files=0,
                total_size_bytes=0,
                total_isbns=0,
                backend="local",
                connected=False,
                details={"error": str(e)}
            )

    def delete_isbn(self, isbn: str) -> int:
        """Delete all files for an ISBN from local filesystem."""
        isbn_dir = self.base_path / isbn
        if not isbn_dir.exists():
            return 0

        deleted = 0
        for file_path in list(isbn_dir.rglob("*")):
            if file_path.is_file():
                file_path.unlink()
                deleted += 1

        # Remove empty directories
        try:
            shutil.rmtree(isbn_dir)
        except Exception:
            pass

        return deleted


# Storage factory
_storage_instance: Optional[StorageBackend] = None


def get_storage(backend: Optional[str] = None) -> StorageBackend:
    """
    Get or create storage backend singleton.

    Args:
        backend: Backend type ("gridfs", "s3", "local") or None for auto-detect

    Returns:
        StorageBackend instance
    """
    global _storage_instance

    if _storage_instance is not None:
        return _storage_instance

    backend = backend or os.getenv("STORAGE_BACKEND", "gridfs")

    if backend == "gridfs":
        _storage_instance = GridFSBackend()
        if not _storage_instance.connect():
            logger.warning("GridFS connection failed, falling back to local storage")
            _storage_instance = LocalBackend()
            _storage_instance.connect()

    elif backend == "s3":
        _storage_instance = S3Backend()
        if not _storage_instance.connect():
            logger.warning("S3 connection failed, falling back to local storage")
            _storage_instance = LocalBackend()
            _storage_instance.connect()

    else:  # local
        _storage_instance = LocalBackend()
        _storage_instance.connect()

    return _storage_instance


def reset_storage() -> None:
    """Reset storage singleton (for testing)."""
    global _storage_instance
    _storage_instance = None
