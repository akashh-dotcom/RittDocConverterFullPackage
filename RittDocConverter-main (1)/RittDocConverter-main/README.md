# RittDocConverter - EPUB Processing Pipeline

A comprehensive EPUB to RittDoc DocBook XML conversion pipeline with full DTD validation and REST API support.

## Features

- **EPUB Processing**: Convert EPUB files to DocBook XML format
- **Automated DTD compliance**: Multi-pass fixing ensures output validates against RittDoc DTD
- **Cover image handling**: Extracts and properly embeds cover images as `<figure id="cover-image">`
- **Image conversion**: Handles inline images, figures, and media objects
- **Reference mapping**: Tracks all internal links and cross-references
- **Validation reporting**: Generates Excel reports for manual verification items
- **REST API**: Full API support for UI integration
- **Batch processing**: Support for S3/SFTP batch operations (can be extracted for separate UI project)

## Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/JCZentrovia/RittDocConverter.git
cd RittDocConverter

# Create and activate virtual environment
python3 -m venv venv
source venv/bin/activate  # Linux/Mac
# or: .\venv\Scripts\Activate.ps1  # Windows

# Install dependencies
pip install -r requirements.txt
```

### Basic Usage

```bash
# Convert an EPUB file (CLI)
python epub_pipeline.py input.epub

# Specify output directory
python epub_pipeline.py input.epub ./my_output

# Run in non-interactive mode (for automation)
python epub_pipeline.py input.epub --no-interactive

# Enable debug logging
python epub_pipeline.py input.epub --debug
```

### REST API Usage

```bash
# Start the API server
python -m api.server --port 5001

# Or with debug mode
python -m api.server --port 5001 --debug
```

API endpoints:
- `POST /api/v1/convert` - Start a conversion job
- `GET /api/v1/jobs` - List all jobs
- `GET /api/v1/jobs/{job_id}` - Get job status
- `GET /api/v1/dashboard` - Get conversion dashboard data
- `GET /api/v1/download/{job_id}` - Download result

### Output

Results are saved to `./Output/` (or specified directory):
- `{isbn}.zip` - Initial conversion package
- `{isbn}_all_fixes.zip` - **Final DTD-compliant package** (use this!)
- `{isbn}_validation_report.xlsx` - Manual verification items
- `{isbn}_intermediate/` - Intermediate files for debugging

## Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      epub_pipeline.py                           │
│                      (Main Entry Point)                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │  EPUB Input       │
                    │  (.epub/.epub3)   │
                    └─────────┬─────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │epub_to_structured │
                    │      _v2.py       │
                    │                   │
                    │ • Cover extraction│
                    │ • Chapter parsing │
                    │ • Image handling  │
                    │ • TOC processing  │
                    └─────────┬─────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │ xslt_transformer  │
                    │ (DTD Compliance)  │
                    └─────────┬─────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │    package.py     │
                    │ (ZIP Packaging)   │
                    └─────────┬─────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │comprehensive_dtd  │
                    │    _fixer.py      │
                    │ (Multi-pass Fix)  │
                    └─────────┬─────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │  validate_with    │
                    │ _entity_tracking  │
                    │ (DTD Validation)  │
                    └─────────┬─────────┘
                              │
                              ▼
                       ┌───────────┐
                       │  Output   │
                       │  (.zip)   │
                       └───────────┘
```

## Project Structure

```
RittDocConverter/
├── epub_pipeline.py          # Main entry point - run this!
├── requirements.txt          # Python dependencies
│
├── # EPUB Processing (Core)
├── epub_to_structured_v2.py  # EPUB → DocBook converter
├── epub_diagnostics.py       # EPUB analysis tool
├── reference_mapper.py       # Cross-reference tracking
├── xml_utils.py              # Shared XML utilities
│
├── # DTD Compliance & Validation
├── xslt_transformer.py       # XSLT transformations
├── comprehensive_dtd_fixer.py# Multi-pass DTD fixes
├── fix_chapters_simple.py    # Chapter-level fixes
├── fix_misclassified_figures.py # Figure classification
├── validate_with_entity_tracking.py # DTD validation
├── validate_rittdoc.py       # Standalone validator
│
├── # Packaging & Output
├── package.py                # ZIP package creation
├── conversion_tracker.py     # Conversion progress tracking
├── validation_report.py      # Excel report generation
├── pipeline_controller.py    # Interactive pipeline control
│
├── # REST API (for UI integration)
├── api/
│   ├── __init__.py
│   ├── server.py             # Flask REST API server
│   └── conversion_api.py     # Conversion API class
│
├── # Batch Processing (extractable for UI project)
├── batch/
│   ├── batch_processor.py    # Batch job runner
│   └── README.md             # Batch integration guide
├── src/
│   ├── batch/                # Batch infrastructure
│   │   ├── processor.py
│   │   ├── scheduler.py
│   │   ├── registry.py
│   │   └── s3_client.py
│   ├── config/               # Configuration management
│   └── utils/                # Utilities
│
├── # Web Editor (EPUB-specific)
├── editor/
│   ├── server.py             # Editor Flask server
│   ├── templates/
│   └── static/
│
├── # XSLT Templates
├── xslt/
│   └── rittdoc_compliance.xslt
│
├── # DTD Files
├── RITTDOCdtd/
│   └── v1.1/
│       └── RittDocBook.dtd
│
├── # Configuration
├── config/
│   ├── labels.expanded.json
│   ├── mapping.default.json
│   └── publishers/
│
├── # Tests
├── tests/
│   ├── conftest.py
│   └── test_*.py
│
├── # Documentation
├── docs/
│   ├── QUICK_START.md
│   ├── SETUP_GUIDE.md
│   ├── VALIDATION_GUIDE.md
│   └── ...
│
└── # Legacy (PDF processing - deprecated)
    └── legacy/
        ├── integrated_pipeline.py
        └── pdf_processing/
            ├── grid_reading_order.py
            ├── flow_builder.py
            ├── heuristics_Nov3.py
            └── ...
```

## REST API Reference

### Start Conversion

```bash
# JSON request
curl -X POST http://localhost:5001/api/v1/convert \
  -H "Content-Type: application/json" \
  -d '{"input_file": "/path/to/book.epub"}'

# File upload
curl -X POST http://localhost:5001/api/v1/convert \
  -F "file=@book.epub"
```

### Check Job Status

```bash
curl http://localhost:5001/api/v1/jobs/{job_id}
```

### Get Dashboard Data

```bash
curl http://localhost:5001/api/v1/dashboard
```

### Download Result

```bash
curl -O http://localhost:5001/api/v1/download/{job_id}
```

## Batch Processing

The batch processing module can be extracted and used in a separate UI project:

```bash
# CLI mode - process local files
python batch/batch_processor.py --mode cli --files book1.epub book2.epub

# Automated mode with S3
python batch/batch_processor.py --mode automated --config config.yaml --run-once
```

See `batch/README.md` for integration details.

## Validation & Diagnostics

### Standalone Validation

```bash
# Validate a converted package
python validate_rittdoc.py output.zip
```

### EPUB Diagnostics

```bash
# Analyze EPUB structure before conversion
python epub_diagnostics.py input.epub
```

## Configuration

### Publisher-specific Settings

Create `epub_publishers.yaml` for publisher-specific conversion rules:

```yaml
publishers:
  - name: "Publisher Name"
    patterns:
      - "publisher-identifier"
    settings:
      # Custom settings here
```

### Configuration Files

- `config.yaml.example` - Full batch processing config
- `config.cli.yaml.example` - CLI-specific settings
- `config.production.yaml.example` - Production settings

## Requirements

- Python 3.10+
- lxml
- BeautifulSoup4
- EbookLib
- Pillow
- Flask (for API)
- See `requirements.txt` for complete list

## Documentation

- [Quick Start Guide](docs/QUICK_START.md) - Get started in 5 minutes
- [Setup Guide](docs/SETUP_GUIDE.md) - Detailed installation instructions
- [Validation Guide](docs/VALIDATION_GUIDE.md) - Understanding validation reports
- [XSLT/DTD Guide](docs/XSLT_DTD_VALIDATION.md) - DTD compliance details
- [Batch Processing](docs/BATCH_PROCESSING.md) - S3/SFTP batch operations

## Legacy Code

PDF processing modules have been moved to `legacy/pdf_processing/`. If you need PDF processing, use a separate PDF pipeline project.

## License

See LICENSE file for details.
