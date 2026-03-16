#!/usr/bin/env python3
"""
Convenience wrapper for the Lean XML Transformation Pipeline Tester.

This script tests RittDocConverter XML output through the LoadBook transformation
chain WITHOUT requiring database connections, drug linking, or other slow processes.

Usage:
    python test_pipeline.py <input_zip_or_xml>
    python test_pipeline.py output.zip --verbose
    python test_pipeline.py Book.xml --keep-temp --json-output results.json
"""

import sys
from pathlib import Path

# Add pipeline_tester to path
sys.path.insert(0, str(Path(__file__).parent / "pipeline_tester"))

from pipeline_tester import main

if __name__ == "__main__":
    main()
