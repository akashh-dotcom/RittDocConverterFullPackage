"""
XML Utility Functions

Shared utility functions for XML analysis that work on structured.xml
regardless of source format (EPUB or PDF).

These functions were extracted from pdf_mapper_wrapper.py to be used
by the EPUB processing pipeline.
"""

import logging
from pathlib import Path
from typing import Dict

from lxml import etree

logger = logging.getLogger(__name__)


def count_resources(structured_xml_path: Path) -> Dict[str, int]:
    """
    Count resources in structured.xml for tracking purposes.

    Args:
        structured_xml_path: Path to structured.xml

    Returns:
        Dict with counts: num_chapters, num_images, num_tables, etc.
    """
    counts = {
        'num_chapters': 0,
        'num_images': 0,
        'num_tables': 0,
        'num_equations': 0,
        'num_sections': 0,
    }

    if not structured_xml_path.exists():
        return counts

    try:
        parser = etree.XMLParser(resolve_entities=False, no_network=True)
        tree = etree.parse(str(structured_xml_path), parser)
        root = tree.getroot()

        counts['num_chapters'] = len(root.findall('.//chapter'))
        counts['num_images'] = len(root.findall('.//imagedata'))
        counts['num_tables'] = len(root.findall('.//table'))
        counts['num_equations'] = len(root.findall('.//equation'))
        counts['num_sections'] = len(root.findall('.//section'))

    except Exception as e:
        logger.error(f"Failed to count resources: {e}")

    return counts


def detect_template_type(structured_xml_path: Path) -> str:
    """
    Detect if document is single-column, double-column, or mixed.

    This is a simple heuristic based on typical patterns.

    Args:
        structured_xml_path: Path to structured.xml

    Returns:
        Template type string: "Single Column", "Double Column", "Mixed", or "Unknown"
    """
    # For now, return Unknown - this could be enhanced with actual column detection
    # from the PDF layout analysis
    return "Unknown"
