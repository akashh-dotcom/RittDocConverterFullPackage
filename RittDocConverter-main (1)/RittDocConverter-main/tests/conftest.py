"""
Pytest configuration and fixtures for EPUB processing tests.
"""

import sys
from pathlib import Path

import pytest

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))


@pytest.fixture
def sample_epub_path(tmp_path):
    """
    Create a sample EPUB file for testing.
    Returns the path to the sample EPUB.
    """
    # Note: Actual EPUB creation would be more complex
    # This is a placeholder for test setup
    epub_path = tmp_path / "sample.epub"
    # Create minimal EPUB structure if needed
    return epub_path


@pytest.fixture
def output_dir(tmp_path):
    """
    Create a temporary output directory for tests.
    """
    output = tmp_path / "output"
    output.mkdir()
    return output


@pytest.fixture
def dtd_path():
    """
    Return the path to the RittDoc DTD.
    """
    return Path(__file__).parent.parent / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
