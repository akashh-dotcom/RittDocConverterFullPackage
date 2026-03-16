# Lean XML Transformation Pipeline Tester

Tests RittDocConverter XML output through the LoadBook XSL transformation chain **WITHOUT** requiring:
- Database connections
- Drug linking
- Disease linking
- PMID linking
- Content loading
- R2Utilities.exe / .NET runtime

## Purpose

This tool catches XSL transformation errors **before** running the full `LoadBook.bat`, saving significant time (minutes → seconds).

## Usage

```bash
# From project root
python test_pipeline.py <input_zip_or_xml>

# With options
python test_pipeline.py output.zip --verbose
python test_pipeline.py Book.xml --skip-html --keep-temp
python test_pipeline.py input.zip --json-output results.json

# Or directly
python pipeline_tester/pipeline_tester.py <input>
```

## What It Tests

### Stage 1: Input Preparation
- Extracts ZIP if needed
- Locates Book.XML

### Stage 2: Pre-flight Validation
Checks for issues that will cause LoadBook to fail:
- **MISSING_AUTHOR** - AddRISInfo.xsl will TERMINATE without author/editor
- **MISSING_ISBN** - Required for chunking and file naming
- **MISSING_TITLE** - Required metadata
- **DUPLICATE_IDS** - Will break linking and references
- **CHAPTERS_NO_ID** - Will break TOC generation
- **SECT1_NO_ID** - Will break chunking

### Stage 3: AddRISInfo.xsl
Adds RIS metadata to sect1info elements:
- `<risinfo>` with book metadata
- `<primaryauthor>` from book authors
- Chapter info references

### Stage 4: Chunking
Splits book into separate sect1 XML files:
- Creates `sect1.{isbn}.{id}.xml` files
- Validates sect1 structure

### Stage 5: TOC Transform
Generates table of contents structure:
- Creates TOC XML with tocchap, toclevel1, etc.
- Validates chapter/section linking

### Stage 6: RittBook.xsl (Optional)
Tests HTML rendering:
- DocBook → HTML transformation
- Catches unhandled elements
- Use `--skip-html` to skip this slower stage

## Example Output

```
============================================================
LEAN PIPELINE TESTER
============================================================
Input: 9781234567890.zip
Time: 2025-01-17T10:30:00

STAGE 1: Input Preparation
----------------------------------------
✓ Found XML: Book.XML

STAGE 2: Pre-flight Validation
----------------------------------------
  ✓ Pre-flight checks passed

STAGE 3: AddRISInfo.xsl
----------------------------------------
✓ AddRISInfo transformation completed
  Duration: 15ms

STAGE 4: Chunking (sect1 splitting)
----------------------------------------
✓ Created 25 chunk files
  Duration: 45ms

STAGE 5: TOC Transform
----------------------------------------
✓ TOC transformation completed
  Generated 156 TOC entries
  Duration: 12ms

============================================================
PIPELINE TEST SUMMARY
============================================================
Status: ✓ PASSED
Duration: 523ms (0.5s)
Summary: All transformation stages passed
============================================================
```

## XSL Files

The following XSL files are used (copied from Job.zip/App.zip):

| File | Source | Purpose |
|------|--------|---------|
| `AddRISInfo.xsl` | Job/rules/ | Adds RIS metadata to sections |
| `RISChunker.xsl` | Job/rules/ | Reference for chunking logic |
| `toctransform.xsl` | Job/rules/ | TOC generation |
| `RittBook.xsl` | App/Xsl/ | HTML rendering |
| `*.ritt.xsl` | App/Xsl/ | Custom overrides |
| `common/` | App/Xsl/ | Localization and utilities |
| `html/` | App/Xsl/ | DocBook HTML templates |

## Command Line Options

```
positional arguments:
  input                 Input ZIP or XML file to test

optional arguments:
  -h, --help            Show help message
  --skip-html           Skip RittBook.xsl HTML rendering test
  --keep-temp           Keep temporary/output files after completion
  -v, --verbose         Enable verbose output
  --json-output FILE    Write results to JSON file
```

## Error Codes

| Code | Level | Description |
|------|-------|-------------|
| `XML_PARSE_ERROR` | error | Input XML is malformed |
| `INVALID_ROOT` | error | Root element is not `<book>` |
| `MISSING_BOOKINFO` | error | No `<bookinfo>` element |
| `MISSING_AUTHOR` | error | No author/editor - **FATAL in AddRISInfo** |
| `MISSING_ISBN` | error | No ISBN in bookinfo |
| `MISSING_TITLE` | error | No title in bookinfo |
| `DUPLICATE_IDS` | error | Duplicate id attributes |
| `INVALID_ISBN` | warning | ISBN format may be invalid |
| `NO_CHAPTERS` | warning | No chapter elements found |
| `CHAPTERS_NO_ID` | warning | Chapters missing id attribute |
| `SECT1_NO_ID` | warning | sect1 elements missing id |

## Comparison: Full LoadBook vs Lean Tester

| Metric | Full LoadBook | Lean Tester |
|--------|---------------|-------------|
| Runtime | 5-15 minutes | 0.5-2 seconds |
| Database | Required | Not needed |
| Java | Required | Optional (Saxon) |
| .NET | Required | Not needed |
| Catches XSL errors | Yes | Yes |
| Catches DB errors | Yes | No (by design) |
| Drug/Disease linking | Yes | No (skipped) |
| PMID linking | Yes | No (skipped) |
