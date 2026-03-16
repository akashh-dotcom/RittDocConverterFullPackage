"""
MongoDB Client Module

Handles connection to MongoDB for storing and retrieving conversion dashboard data.
Designed to be used by the UI project for dashboard and reporting.
"""

import logging
import os
from dataclasses import asdict
from datetime import datetime
from typing import Any, Dict, List, Optional

logger = logging.getLogger(__name__)

# Suppress verbose pymongo logs (heartbeats, topology changes)
logging.getLogger('pymongo').setLevel(logging.WARNING)
logging.getLogger('pymongo.topology').setLevel(logging.WARNING)
logging.getLogger('pymongo.connection').setLevel(logging.WARNING)
logging.getLogger('pymongo.serverSelection').setLevel(logging.WARNING)

# MongoDB is optional - only import if available
try:
    from pymongo import DESCENDING, MongoClient
    from pymongo.errors import ConnectionFailure, ServerSelectionTimeoutError
    MONGODB_AVAILABLE = True
except ImportError:
    MONGODB_AVAILABLE = False
    logger.warning("pymongo not installed. MongoDB integration disabled.")


class MongoDBClient:
    """
    MongoDB client for conversion dashboard data.

    Stores conversion metadata for dashboard and reporting in the UI project.
    """

    # Default configuration
    DEFAULT_URI = "mongodb+srv://cluster0.lk4msmt.mongodb.net/RittenhouseXMLConverter?appName=Cluster0"
    DEFAULT_DATABASE = "RittenhouseXMLConverter"
    DEFAULT_COLLECTION = "conversion_dashboard"

    def __init__(
        self,
        uri: Optional[str] = None,
        database: Optional[str] = None,
        collection: Optional[str] = None,
        connect_timeout_ms: int = 5000
    ):
        """
        Initialize MongoDB client.

        Args:
            uri: MongoDB connection URI (or set MONGODB_URI env var)
            database: Database name (or set MONGODB_DATABASE env var)
            collection: Collection name (or set MONGODB_COLLECTION env var)
            connect_timeout_ms: Connection timeout in milliseconds
        """
        self.uri = uri or os.getenv("MONGODB_URI", self.DEFAULT_URI)
        self.database_name = database or os.getenv("MONGODB_DATABASE", self.DEFAULT_DATABASE)
        self.collection_name = collection or os.getenv("MONGODB_COLLECTION", self.DEFAULT_COLLECTION)
        self.connect_timeout_ms = connect_timeout_ms

        self._client: Optional[Any] = None
        self._db: Optional[Any] = None
        self._collection: Optional[Any] = None
        self._connected = False

    def connect(self) -> bool:
        """
        Establish connection to MongoDB.

        Returns:
            True if connection successful, False otherwise
        """
        if not MONGODB_AVAILABLE:
            logger.warning("MongoDB not available - pymongo not installed")
            return False

        try:
            self._client = MongoClient(
                self.uri,
                serverSelectionTimeoutMS=self.connect_timeout_ms
            )

            # Test connection
            self._client.admin.command('ping')

            self._db = self._client[self.database_name]
            self._collection = self._db[self.collection_name]
            self._connected = True

            logger.info(f"Connected to MongoDB: {self.database_name}.{self.collection_name}")
            return True

        except (ConnectionFailure, ServerSelectionTimeoutError) as e:
            logger.warning(f"Failed to connect to MongoDB: {e}")
            self._connected = False
            return False
        except Exception as e:
            logger.error(f"MongoDB connection error: {e}")
            self._connected = False
            return False

    def disconnect(self) -> None:
        """Close MongoDB connection."""
        if self._client:
            self._client.close()
            self._connected = False
            logger.info("Disconnected from MongoDB")

    def is_connected(self) -> bool:
        """Check if connected to MongoDB."""
        return self._connected and self._client is not None

    def ensure_indexes(self) -> None:
        """Create indexes for efficient querying."""
        if not self.is_connected():
            return

        try:
            # Index on filename and start_time for unique identification
            self._collection.create_index(
                [("filename", 1), ("start_time", 1)],
                unique=True,
                name="filename_start_time_idx"
            )

            # Index for status filtering
            self._collection.create_index("status", name="status_idx")

            # Index for date range queries
            self._collection.create_index(
                [("start_time", DESCENDING)],
                name="start_time_desc_idx"
            )

            # Index for conversion type
            self._collection.create_index("conversion_type", name="type_idx")

            logger.info("MongoDB indexes created/verified")

        except Exception as e:
            logger.warning(f"Failed to create indexes: {e}")

    def push_conversion(self, metadata: Dict[str, Any]) -> Optional[str]:
        """
        Push conversion metadata to MongoDB.

        Args:
            metadata: Conversion metadata dictionary

        Returns:
            Inserted document ID or None if failed
        """
        if not self.is_connected():
            if not self.connect():
                logger.warning("Cannot push to MongoDB - not connected")
                return None

        try:
            # Prepare document
            doc = self._prepare_document(metadata)

            # Upsert based on filename and start_time
            result = self._collection.update_one(
                {
                    "filename": doc["filename"],
                    "start_time": doc["start_time"]
                },
                {"$set": doc},
                upsert=True
            )

            if result.upserted_id:
                logger.info(f"Inserted conversion record: {doc['filename']}")
                return str(result.upserted_id)
            else:
                logger.info(f"Updated conversion record: {doc['filename']}")
                return "updated"

        except Exception as e:
            logger.error(f"Failed to push conversion to MongoDB: {e}")
            return None

    def _prepare_document(self, metadata: Dict[str, Any]) -> Dict[str, Any]:
        """
        Prepare metadata for MongoDB storage.

        Converts datetime objects and enums to serializable formats.
        """
        doc = {}

        for key, value in metadata.items():
            if isinstance(value, datetime):
                doc[key] = value.isoformat()
            elif hasattr(value, 'value'):  # Enum
                doc[key] = value.value
            elif isinstance(value, list):
                doc[key] = value
            else:
                doc[key] = value

        # Add metadata timestamp
        doc["_updated_at"] = datetime.now().isoformat()

        return doc

    def get_conversions(
        self,
        status: Optional[str] = None,
        conversion_type: Optional[str] = None,
        start_date: Optional[datetime] = None,
        end_date: Optional[datetime] = None,
        limit: int = 100,
        skip: int = 0
    ) -> List[Dict[str, Any]]:
        """
        Retrieve conversions from MongoDB.

        Args:
            status: Filter by status (Success, Failure, In Progress)
            conversion_type: Filter by type (PDF, ePub)
            start_date: Filter by start date (from)
            end_date: Filter by start date (to)
            limit: Maximum records to return
            skip: Number of records to skip (for pagination)

        Returns:
            List of conversion records
        """
        if not self.is_connected():
            if not self.connect():
                return []

        try:
            query = {}

            if status:
                query["status"] = status

            if conversion_type:
                query["conversion_type"] = conversion_type

            if start_date or end_date:
                query["start_time"] = {}
                if start_date:
                    query["start_time"]["$gte"] = start_date.isoformat()
                if end_date:
                    query["start_time"]["$lte"] = end_date.isoformat()

            cursor = self._collection.find(
                query,
                {"_id": 0}  # Exclude MongoDB _id from results
            ).sort("start_time", DESCENDING).skip(skip).limit(limit)

            return list(cursor)

        except Exception as e:
            logger.error(f"Failed to retrieve conversions: {e}")
            return []

    def get_statistics(self) -> Dict[str, Any]:
        """
        Get aggregated statistics from MongoDB.

        Returns:
            Statistics dictionary for dashboard
        """
        if not self.is_connected():
            if not self.connect():
                return {}

        try:
            # Aggregation pipeline
            pipeline = [
                {
                    "$group": {
                        "_id": None,
                        "total_conversions": {"$sum": 1},
                        "successful": {
                            "$sum": {"$cond": [{"$eq": ["$status", "Success"]}, 1, 0]}
                        },
                        "failed": {
                            "$sum": {"$cond": [{"$eq": ["$status", "Failure"]}, 1, 0]}
                        },
                        "in_progress": {
                            "$sum": {"$cond": [{"$eq": ["$status", "In Progress"]}, 1, 0]}
                        },
                        "total_images": {"$sum": {"$add": [
                            {"$ifNull": ["$num_vector_images", 0]},
                            {"$ifNull": ["$num_raster_images", 0]}
                        ]}},
                        "total_tables": {"$sum": {"$ifNull": ["$num_tables", 0]}},
                        "pdf_conversions": {
                            "$sum": {"$cond": [{"$eq": ["$conversion_type", "PDF"]}, 1, 0]}
                        },
                        "epub_conversions": {
                            "$sum": {"$cond": [{"$eq": ["$conversion_type", "ePub"]}, 1, 0]}
                        }
                    }
                }
            ]

            result = list(self._collection.aggregate(pipeline))

            if result:
                stats = result[0]
                del stats["_id"]
                return stats

            return {
                "total_conversions": 0,
                "successful": 0,
                "failed": 0,
                "in_progress": 0,
                "total_images": 0,
                "total_tables": 0,
                "pdf_conversions": 0,
                "epub_conversions": 0
            }

        except Exception as e:
            logger.error(f"Failed to get statistics: {e}")
            return {}

    def get_recent_conversions(self, limit: int = 10) -> List[Dict[str, Any]]:
        """Get most recent conversions."""
        return self.get_conversions(limit=limit)

    def get_failed_conversions(self, limit: int = 50) -> List[Dict[str, Any]]:
        """Get failed conversions for review."""
        return self.get_conversions(status="Failure", limit=limit)

    def delete_conversion(self, filename: str, start_time: str) -> bool:
        """
        Delete a conversion record.

        Args:
            filename: Original filename
            start_time: Start time (ISO format)

        Returns:
            True if deleted, False otherwise
        """
        if not self.is_connected():
            return False

        try:
            result = self._collection.delete_one({
                "filename": filename,
                "start_time": start_time
            })
            return result.deleted_count > 0
        except Exception as e:
            logger.error(f"Failed to delete conversion: {e}")
            return False


# Singleton instance
_mongodb_client: Optional[MongoDBClient] = None


def get_mongodb_client() -> MongoDBClient:
    """
    Get singleton MongoDB client instance.

    Returns:
        MongoDBClient instance
    """
    global _mongodb_client

    if _mongodb_client is None:
        _mongodb_client = MongoDBClient()

    return _mongodb_client


def push_to_mongodb(metadata: Dict[str, Any]) -> bool:
    """
    Convenience function to push conversion data to MongoDB.

    Args:
        metadata: Conversion metadata dictionary

    Returns:
        True if push successful
    """
    client = get_mongodb_client()
    result = client.push_conversion(metadata)
    return result is not None
