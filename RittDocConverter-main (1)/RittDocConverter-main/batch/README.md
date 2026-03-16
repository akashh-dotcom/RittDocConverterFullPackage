# Batch Processing Module

This module provides batch processing capabilities for the document conversion pipeline.
It is designed to be **extracted and used by the UI project** to handle batch conversions
for both EPUB and PDF processing pipelines.

## Structure

```
batch/
├── batch_processor.py      # Main batch processor entry point
└── README.md               # This file

../src/
├── batch/                  # Core batch processing infrastructure
│   ├── processor.py        # BatchProcessor class
│   ├── scheduler.py        # JobScheduler (APScheduler integration)
│   ├── registry.py         # FileRegistry (SQLite tracking)
│   ├── s3_client.py        # S3/SFTP client
│   └── __init__.py
├── config/                 # Configuration management
│   ├── settings.py         # Config dataclasses
│   └── __init__.py
└── utils/                  # Utilities
    ├── logger.py           # Logging setup
    └── __init__.py
```

## Usage

### CLI Mode (Local Files)
```bash
python batch/batch_processor.py --mode cli --files file1.epub file2.epub --publisher "MyPublisher"
```

### Automated Mode (S3/SFTP)
```bash
# Single run
python batch/batch_processor.py --mode automated --config config.yaml --run-once

# Scheduled runs
python batch/batch_processor.py --mode automated --config config.yaml --schedule
```

### Utility Commands
```bash
# View registry statistics
python batch/batch_processor.py --stats --config config.yaml

# Reset failed files for retry
python batch/batch_processor.py --reset-failed --config config.yaml
```

## Integration with UI Project

To integrate this batch processing module with the UI project:

1. Copy the entire `batch/` folder and `src/` folder to your UI project
2. Update the subprocess call in `processor.py` to point to the appropriate pipeline:
   - For EPUB: `epub_pipeline.py`
   - For PDF: Your separate PDF pipeline
3. Configure the API endpoints in your UI project to call the BatchProcessor

## Configuration

See the configuration examples in the root directory:
- `config.yaml.example` - Full configuration template
- `config.cli.yaml.example` - CLI-specific configuration
- `config.production.yaml.example` - Production settings

## Dependencies

- `boto3` - AWS S3 integration
- `APScheduler` - Job scheduling
- `PyYAML` - Configuration parsing
- `openpyxl` - Excel report generation
