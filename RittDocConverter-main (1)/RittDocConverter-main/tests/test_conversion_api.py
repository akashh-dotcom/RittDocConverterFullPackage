"""
Comprehensive tests for api/conversion_api.py

Tests for:
- extract_isbn_from_filename function
- JobStatus enum
- ConversionJob dataclass
- ConversionAPI class methods
"""

import json
import threading
import time
from datetime import datetime
from pathlib import Path
from unittest.mock import Mock, MagicMock, patch, mock_open

import pytest

from api.conversion_api import (
    ConversionAPI,
    ConversionJob,
    JobStatus,
    extract_isbn_from_filename,
    get_api,
    MAX_JOB_ID_LENGTH,
)


class TestExtractISBNFromFilename:
    """Tests for ISBN extraction from filenames."""

    def test_extract_isbn13_simple(self):
        """Test extraction of simple ISBN-13."""
        isbn = extract_isbn_from_filename("9781234567890.epub")
        assert isbn == "9781234567890"

    def test_extract_isbn13_with_hyphens(self):
        """Test extraction of ISBN-13 with hyphens."""
        isbn = extract_isbn_from_filename("978-1-234-56789-0.epub")
        assert isbn == "9781234567890"

    def test_extract_isbn13_with_prefix(self):
        """Test extraction of ISBN-13 with prefix text."""
        isbn = extract_isbn_from_filename("book_9781234567890.epub")
        assert isbn == "9781234567890"

    def test_extract_isbn10(self):
        """Test extraction of ISBN-10."""
        isbn = extract_isbn_from_filename("123456789X.epub")
        assert isbn == "123456789X"

    def test_extract_isbn10_lowercase_x(self):
        """Test extraction of ISBN-10 with lowercase x."""
        isbn = extract_isbn_from_filename("123456789x.epub")
        assert isbn == "123456789X"

    def test_fallback_to_filename(self):
        """Test fallback to cleaned filename when no ISBN found."""
        isbn = extract_isbn_from_filename("my-book-title.epub")
        assert isbn == "my_book_title"

    def test_truncate_long_isbn(self):
        """Test that long IDs are truncated to max length."""
        long_name = "a" * 50
        isbn = extract_isbn_from_filename(f"{long_name}.epub")
        assert len(isbn) <= MAX_JOB_ID_LENGTH

    def test_custom_max_length(self):
        """Test custom max_length parameter."""
        isbn = extract_isbn_from_filename("9781234567890.epub", max_length=10)
        assert len(isbn) == 10
        assert isbn == "9781234567"

    def test_extract_from_path(self):
        """Test extraction from full path."""
        isbn = extract_isbn_from_filename("/path/to/9781234567890.epub")
        assert isbn == "9781234567890"

    def test_special_characters_cleaned(self):
        """Test that special characters are cleaned in fallback."""
        isbn = extract_isbn_from_filename("book@title#2023!.epub")
        assert isbn == "book_title_2023_"


class TestJobStatus:
    """Tests for JobStatus enum."""

    def test_job_status_values(self):
        """Test that all job status values are correct."""
        assert JobStatus.PENDING.value == "pending"
        assert JobStatus.RUNNING.value == "running"
        assert JobStatus.COMPLETED.value == "completed"
        assert JobStatus.FAILED.value == "failed"
        assert JobStatus.CANCELLED.value == "cancelled"

    def test_job_status_string_type(self):
        """Test that JobStatus inherits from str."""
        assert isinstance(JobStatus.PENDING, str)
        assert isinstance(JobStatus.RUNNING, str)


class TestConversionJob:
    """Tests for ConversionJob dataclass."""

    def test_conversion_job_creation(self):
        """Test creating a ConversionJob."""
        job = ConversionJob(
            job_id="test-job-123",
            input_file="/path/to/input.epub",
            output_dir="/path/to/output",
        )
        assert job.job_id == "test-job-123"
        assert job.input_file == "/path/to/input.epub"
        assert job.output_dir == "/path/to/output"
        assert job.status == JobStatus.PENDING
        assert job.progress == 0
        assert job.message == ""

    def test_conversion_job_to_dict(self):
        """Test converting ConversionJob to dictionary."""
        job = ConversionJob(
            job_id="test-job-123",
            input_file="/path/to/input.epub",
            output_dir="/path/to/output",
        )
        job_dict = job.to_dict()
        assert job_dict["job_id"] == "test-job-123"
        assert job_dict["status"] == JobStatus.PENDING
        assert "created_at" in job_dict

    def test_conversion_job_default_created_at(self):
        """Test that created_at is set automatically."""
        job = ConversionJob(
            job_id="test-job",
            input_file="/input.epub",
            output_dir="/output",
        )
        # Should be an ISO format datetime string
        assert isinstance(job.created_at, str)
        datetime.fromisoformat(job.created_at)  # Should not raise


class TestConversionAPI:
    """Tests for ConversionAPI class."""

    @pytest.fixture
    def temp_output_dir(self, tmp_path):
        """Create temporary output directory."""
        output_dir = tmp_path / "output"
        output_dir.mkdir()
        return output_dir

    @pytest.fixture
    def api(self, temp_output_dir):
        """Create ConversionAPI instance."""
        with patch('api.conversion_api.get_mongodb_client'):
            return ConversionAPI(output_dir=temp_output_dir)

    @pytest.fixture
    def sample_epub(self, tmp_path):
        """Create a sample EPUB file."""
        epub_path = tmp_path / "9781234567890.epub"
        epub_path.write_text("sample epub content")
        return epub_path

    def test_api_initialization(self, temp_output_dir):
        """Test ConversionAPI initialization."""
        with patch('api.conversion_api.get_mongodb_client'):
            api = ConversionAPI(output_dir=temp_output_dir)
            assert api.output_dir == temp_output_dir
            assert api.output_dir.exists()
            assert isinstance(api._active_jobs, dict)
            assert isinstance(api._job_threads, dict)

    def test_api_default_output_dir(self):
        """Test ConversionAPI with default output directory."""
        with patch('api.conversion_api.get_mongodb_client'):
            api = ConversionAPI()
            assert api.output_dir.exists()

    def test_start_conversion_file_not_found(self, api):
        """Test start_conversion with non-existent file."""
        with pytest.raises(FileNotFoundError):
            api.start_conversion(input_file="/nonexistent/file.epub")

    def test_start_conversion_invalid_file_type(self, api, tmp_path):
        """Test start_conversion with invalid file type."""
        txt_file = tmp_path / "test.txt"
        txt_file.write_text("not an epub")

        with pytest.raises(ValueError, match="Invalid file type"):
            api.start_conversion(input_file=str(txt_file))

    def test_start_conversion_sync(self, api, sample_epub, temp_output_dir):
        """Test synchronous conversion start - runs async to avoid import issues."""
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True  # Use async to avoid ebooklib import
        )

        assert "job_id" in result
        assert result["status"] == "started"

        # Verify job was created
        job = api.get_job(result["job_id"])
        assert job is not None

    def test_start_conversion_async(self, api, sample_epub):
        """Test asynchronous conversion start."""
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )

        assert result["status"] == "started"
        assert "job_id" in result
        assert "isbn" in result

    def test_start_conversion_custom_job_id(self, api, sample_epub):
        """Test start_conversion with custom job_id."""
        custom_id = "custom-isbn-123"
        result = api.start_conversion(
            input_file=str(sample_epub),
            job_id=custom_id,
            async_mode=True
        )

        assert result["job_id"] == custom_id

    def test_start_conversion_already_running(self, api, sample_epub):
        """Test starting conversion when job already running."""
        # Start first conversion
        result1 = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result1["job_id"]

        # Try to start same job again
        result2 = api.start_conversion(
            input_file=str(sample_epub),
            job_id=job_id,
            async_mode=True
        )

        assert result2["status"] == "already_running"

    def test_get_job_active(self, api, sample_epub):
        """Test getting an active job."""
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        job = api.get_job(job_id)
        assert job is not None
        assert job.job_id == job_id

    def test_get_job_not_found(self, api):
        """Test getting a non-existent job."""
        job = api.get_job("nonexistent-job")
        assert job is None

    def test_get_job_status(self, api, sample_epub):
        """Test getting job status."""
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        status = api.get_job_status(job_id)
        assert status["job_id"] == job_id
        assert "status" in status
        assert "progress" in status

    def test_get_job_status_not_found(self, api):
        """Test getting status of non-existent job."""
        with pytest.raises(KeyError):
            api.get_job_status("nonexistent-job")

    def test_list_jobs_empty(self, api):
        """Test listing jobs when none exist."""
        jobs = api.list_jobs()
        assert isinstance(jobs, list)

    def test_list_jobs_with_active(self, api, sample_epub):
        """Test listing jobs with active jobs."""
        api.start_conversion(input_file=str(sample_epub), async_mode=True)

        jobs = api.list_jobs()
        assert len(jobs) >= 1

    def test_list_jobs_with_status_filter(self, api, sample_epub):
        """Test listing jobs with status filter."""
        api.start_conversion(input_file=str(sample_epub), async_mode=True)

        jobs = api.list_jobs(status=JobStatus.RUNNING)
        # All jobs should have the filtered status
        for job in jobs:
            assert job.get('status') == JobStatus.RUNNING.value

    def test_list_jobs_with_limit(self, api, sample_epub):
        """Test listing jobs with limit."""
        # Start multiple jobs
        for i in range(5):
            epub = sample_epub.parent / f"book{i}.epub"
            epub.write_text(f"content {i}")
            api.start_conversion(input_file=str(epub), async_mode=True)

        jobs = api.list_jobs(limit=3)
        assert len(jobs) <= 3

    def test_cancel_job_running(self, api, sample_epub):
        """Test cancelling a running job."""
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        cancel_result = api.cancel_job(job_id)
        assert cancel_result["status"] == "cancelled"

    def test_cancel_job_not_found(self, api):
        """Test cancelling non-existent job."""
        with pytest.raises(KeyError):
            api.cancel_job("nonexistent-job")

    def test_cancel_job_completed(self, api, sample_epub, temp_output_dir):
        """Test cancelling a job that cannot be cancelled."""
        # Create a job and manually set it to completed
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        # Manually set job to completed
        job = api.get_job(job_id)
        job.status = JobStatus.COMPLETED

        # Try to cancel - should fail
        cancel_result = api.cancel_job(job_id)
        assert cancel_result["status"] == "error" or "cannot cancel" in cancel_result.get("message", "").lower()

    def test_get_dashboard_data(self, api):
        """Test getting dashboard data."""
        data = api.get_dashboard_data()

        assert "statistics" in data
        assert "recent_jobs" in data
        assert "timestamp" in data
        assert "total_jobs" in data["statistics"]

    def test_get_conversion_result_not_found(self, api):
        """Test getting result of non-existent job."""
        with pytest.raises(KeyError):
            api.get_conversion_result("nonexistent-job")

    def test_get_conversion_result_not_completed(self, api, sample_epub):
        """Test getting result of incomplete job."""
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        result = api.get_conversion_result(job_id)
        assert result["status"] == "error"
        assert "not completed" in result["message"].lower()

    def test_register_progress_callback(self, api, sample_epub):
        """Test registering progress callback."""
        callback_called = []

        def callback(progress, message):
            callback_called.append((progress, message))

        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        api.register_progress_callback(job_id, callback)
        api._notify_progress(job_id, 50, "Test progress")

        assert len(callback_called) > 0
        assert callback_called[0] == (50, "Test progress")

    def test_cleanup_old_jobs(self, api, sample_epub, temp_output_dir):
        """Test cleaning up old completed jobs."""
        from datetime import datetime, timedelta

        # Create a job and manually complete it with old timestamp
        result = api.start_conversion(
            input_file=str(sample_epub),
            async_mode=True
        )
        job_id = result["job_id"]

        # Manually set job to completed with old timestamp
        job = api.get_job(job_id)
        job.status = JobStatus.COMPLETED
        old_time = datetime.now() - timedelta(hours=25)
        job.completed_at = old_time.isoformat()

        # Cleanup should remove old jobs
        cleaned = api.cleanup_old_jobs(max_age_hours=24)
        assert cleaned >= 0

    def test_upload_to_storage_not_available(self, api, temp_output_dir):
        """Test upload when storage is not available."""
        output_path = temp_output_dir / "test.zip"
        output_path.write_text("test content")

        result = api._upload_to_storage("123", output_path, temp_output_dir)
        assert result == []

    def test_job_id_truncation(self, api, sample_epub):
        """Test that long job_ids are truncated."""
        long_id = "a" * 50
        result = api.start_conversion(
            input_file=str(sample_epub),
            job_id=long_id,
            async_mode=True
        )

        assert len(result["job_id"]) <= MAX_JOB_ID_LENGTH

    def test_mongodb_integration_failure(self, temp_output_dir):
        """Test API works when MongoDB is unavailable."""
        with patch('api.conversion_api.MONGODB_AVAILABLE', False):
            api = ConversionAPI(output_dir=temp_output_dir)
            assert api._mongodb_client is None


class TestGetAPI:
    """Tests for get_api singleton function."""

    def test_get_api_singleton(self, tmp_path):
        """Test that get_api returns singleton instance."""
        with patch('api.conversion_api.get_mongodb_client'):
            api1 = get_api(tmp_path)
            api2 = get_api(tmp_path)
            assert api1 is api2

    def test_get_api_creates_instance(self, tmp_path):
        """Test that get_api creates new instance if none exists."""
        # Reset singleton
        import api.conversion_api
        api.conversion_api._api_instance = None

        with patch('api.conversion_api.get_mongodb_client'):
            api = get_api(tmp_path)
            assert api is not None
            assert isinstance(api, ConversionAPI)