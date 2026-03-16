---
name: r2-diagnosis
description: >
  Root-cause diagnosis for R2 Digital Library book conversion issues.
  Compares publisher source XHTML against converted DocBook XML to identify
  why each issue occurred and whether it's a Converter, Publisher, or Platform problem.
  Use after r2-book-qa has identified issues, or with a manual list of issues to investigate.
argument-hint: "[source_xhtml.zip] [converted_xml.zip] [optional: QA report or issue list]"
---

# R2 Conversion Diagnosis Skill

You are a conversion diagnostics specialist. Your job is to compare publisher source XHTML files against converted DocBook XML files to determine the root cause of each issue and produce a detailed diagnosis report.

## Prerequisites
- A QA report or list of issues to investigate (from `r2-book-qa` skill or manual input)
- Publisher source XHTML zip file (the original EPUB content)
- Converted XML zip file (the pipeline output rendered on the platform)

## Workflow

### Step 1: Extract & Identify Files

1. Create temp working directory: `mkdir -p /tmp/r2_diagnosis`
2. Extract both zips:
   ```bash
   unzip -o <zip1> -d /tmp/r2_diagnosis/zip1
   unzip -o <zip2> -d /tmp/r2_diagnosis/zip2
   ```
3. **Auto-detect which zip is source vs converted**:
   - Source (publisher XHTML): Contains `.xhtml` files, typically under `OEBPS/html/`, has `content.opf` or `toc.ncx`
   - Converted (DocBook XML): Contains files matching `sect1.{ISBN}.ch*.xml`, `preface.{ISBN}.pr*.xml`, `book.{ISBN}.xml`, `toc.{ISBN}.xml`
   - If both have similar names, check file content: source has HTML tags (`<div>`, `<span>`, `<h2>`), converted has DocBook tags (`<sect1>`, `<para>`, `<emphasis>`)
4. Set variables for source_dir and converted_dir
5. Extract ISBN from converted filenames (e.g., `sect1.9783032032409.ch0001s0002.xml` -> ISBN is `9783032032409`)

### Step 2: Build File Correspondence Map

1. List all source XHTML files and converted XML files
2. Build a mapping between them:
   - Converted `preface.{ISBN}.pr0001.xml` -> Source `Cover.xhtml` or equivalent
   - Converted `preface.{ISBN}.pr0002.xml` -> Source title page XHTML
   - Converted `sect1.{ISBN}.ch0001s*.xml` -> Source Chapter 1 XHTML
   - Converted `sect1.{ISBN}.ch0002s*.xml` -> Source Chapter 2 XHTML
3. For publisher-specific file naming, reference `../_shared/publisher-patterns.md`
4. Log the mapping for the report

### Step 3: Automated Comparison Checks

Run ALL of these automated checks across all files. Use parallel agents where possible for speed.

#### 3a. Content Loss Detection
```bash
# Count total text characters in source vs converted
# Use Python for accurate text extraction:
python3 -c "
from lxml import etree
import glob, os

# Source text extraction
source_chars = 0
for f in glob.glob('/tmp/r2_diagnosis/source/**/*.xhtml', recursive=True):
    tree = etree.parse(f, etree.HTMLParser())
    text = ' '.join(tree.xpath('//body//text()'))
    source_chars += len(text.strip())

# Converted text extraction
converted_chars = 0
for f in glob.glob('/tmp/r2_diagnosis/converted/**/*.xml', recursive=True):
    if 'book.' not in os.path.basename(f) and 'toc.' not in os.path.basename(f):
        try:
            tree = etree.parse(f)
            text = ' '.join(tree.xpath('//text()'))
            converted_chars += len(text.strip())
        except: pass

print(f'Source: {source_chars} chars, Converted: {converted_chars} chars')
print(f'Delta: {converted_chars - source_chars} ({(converted_chars-source_chars)/max(source_chars,1)*100:.1f}%)')
"
```

#### 3b. Element Count Comparison
Count and compare these element types between source and converted:

| Element Type | Source (XHTML) | Converted (XML) |
|-------------|----------------|-----------------|
| Tables | `<table>` | `<table>` or `<informaltable>` |
| Figures/Images | `<img>` or `<figure>` | `<figure>` or `<mediaobject>` |
| Bold text | `<b>` or `<strong>` | `<emphasis role="bold">` |
| Italic text | `<i>` or `<em>` | `<emphasis>` (no role) |
| Superscript | `<sup>` | `<superscript>` |
| Subscript | `<sub>` | `<subscript>` |
| Links (external) | `<a href="http...">` | `<ulink url="...">` |
| Links (internal) | `<a href="#...">` | `<link linkend="...">` or `<xref>` |
| Bibliography entries | `<li class="Citation">` or `<div class="bib">` | `<bibliomixed>` |
| Callout/admonition boxes | Publisher-specific (see patterns) | `<tip>`, `<note>`, `<important>`, `<sidebar>` |
| Footnotes | `<aside epub:type="footnote">` or `<a class="Footnote">` | `<footnote>` |

For each, report the count and any significant delta (>5% loss = flag as issue).

#### 3c. Whitespace Analysis
```bash
# Check for missing spaces around emphasis/inline elements in converted XML
grep -r '<emphasis[^>]*>[^<]*</emphasis>[A-Z]' /tmp/r2_diagnosis/converted/ --include="*.xml" -c
# This catches patterns like: </emphasis>When (missing space)
```

#### 3d. Encoding Issue Scan
```bash
# Check for UTF-8 double-encoding artifacts
grep -rP '\xC3\x82' /tmp/r2_diagnosis/converted/ --include="*.xml" -l  # "A" from double-encoded NBSP
grep -rP '\xC3\x83' /tmp/r2_diagnosis/converted/ --include="*.xml" -l  # Other double-encoding

# Check for zero-width space artifacts (split compound words)
grep -r 'Stat Pearls\|doi\. org\|Cross Ref\|Pub Med' /tmp/r2_diagnosis/converted/ --include="*.xml" -c
```

#### 3e. Reference Integrity
```bash
# Double periods in bibliography
grep -c '\.\.' /tmp/r2_diagnosis/converted/**/*.xml 2>/dev/null | grep -v ':0$'

# Broken URLs
grep -rP 'url="https?://\."' /tmp/r2_diagnosis/converted/ --include="*.xml"
grep -rP 'url="https?://[^"]*\s' /tmp/r2_diagnosis/converted/ --include="*.xml"
```

#### 3f. Link Label Preservation
```bash
# Count PubMed/CrossRef labels in both
echo "Source PubMed links:"
grep -rc 'PubMed' /tmp/r2_diagnosis/source/ --include="*.xhtml" | awk -F: '{sum+=$2} END {print sum}'
echo "Converted PubMed links:"
grep -rc 'PubMed' /tmp/r2_diagnosis/converted/ --include="*.xml" | awk -F: '{sum+=$2} END {print sum}'
# Same for CrossRef
```

#### 3g. Formatting Preservation
```bash
# Count bold, italic, superscript, subscript in both
for tag in "b>" "strong>" "i>" "em>" "sup>" "sub>"; do
  echo "Source <$tag:"
  grep -rc "<$tag" /tmp/r2_diagnosis/source/ --include="*.xhtml" | awk -F: '{sum+=$2} END {print sum}'
done

for pattern in 'role="bold"' '<emphasis>' '<superscript>' '<subscript>'; do
  echo "Converted $pattern:"
  grep -rc "$pattern" /tmp/r2_diagnosis/converted/ --include="*.xml" | awk -F: '{sum+=$2} END {print sum}'
done
```

### Step 4: Per-Issue Root-Cause Analysis

For each issue from the QA report (or each issue found in automated checks), perform targeted diagnosis:

1. **Locate the issue in converted XML**: Use grep/xpath to find the exact element and file
2. **Find the corresponding source XHTML**: Use the file correspondence map from Step 2
3. **Extract side-by-side markup**: Show the source markup and converted markup
4. **Determine the root cause**:
   - **If issue exists in source XHTML** -> Publisher Source Issue (P-NNN)
   - **If source is correct but converted is wrong** -> Converter Issue (C-NNN)
   - **If both source and converted are correct but browser shows wrong** -> Platform Issue (R-NNN)
5. **Identify the conversion stage** where the issue was introduced:
   - Stage 1: XHTML->DocBook conversion (`epub_to_structured_v2.py`)
   - Stage 2: XSLT compliance transformation (`xslt_transformer.py`)
   - Stage 3: Packaging/splitting (`package.py`)
   - Stage 4-11: Post-processing fixes (`comprehensive_dtd_fixer.py`, `manual_postprocessor.py`, etc.)
6. **Check known issues catalog**: Reference `../_shared/known-issues.md` to see if this matches an existing known issue

### Step 5: Generate Diagnosis Report

Produce a structured diagnosis report:

```markdown
## Diagnosis Report: [Book Title]
**ISBN:** [ISBN]
**Publisher:** [Publisher]
**Date:** [Date]
**Issues Investigated:** [N]

### Automated Comparison Summary

| Metric | Source | Converted | Delta | Status |
|--------|--------|-----------|-------|--------|
| Text characters | N | N | N (N%) | OK / LOSS |
| Tables | N | N | N | OK / LOSS |
| Figures | N | N | N | OK / LOSS |
| ... | ... | ... | ... | ... |

### Issue Diagnoses

#### Issue 1: [Title]
- **QA Finding:** [What was observed in browser]
- **Source XHTML:**
  ```html
  [exact source markup]
  ```
- **Converted XML:**
  ```xml
  [exact converted markup]
  ```
- **Root Cause:** [Converter / Publisher / Platform]
- **Conversion Stage:** [Stage N: description]
- **Known Issue ID:** [C-NNN / P-NNN / R-NNN or "NEW"]
- **Recommended Fix:**
  - [ ] XML Fix (r2-xml-fixer) -- [brief description of XML-level fix]
  - [ ] Code Fix (r2-code-fixer) -- [brief description of code-level fix]
  - [ ] Publisher Escalation -- [if publisher source issue]
  - [ ] Platform Fix -- [if platform rendering issue]

### New Issues Found (not in QA report)
[Issues discovered during automated comparison that weren't visible in browser]

### Summary
| # | Issue | Root Cause | Stage | Known ID | Fix Type |
|---|-------|-----------|-------|----------|----------|
| 1 | ... | Converter | 1 | C-001 | Code Fix |
```

## Important Reminders

- **Parallel agents**: Launch comparison checks in parallel for speed (Steps 3a-3g can all run simultaneously)
- **File detection**: Always auto-detect source vs converted -- users may provide zips with confusing names
- **Known issues**: Always check `../_shared/known-issues.md` before reporting as new
- **Known non-issues**: Search links (`/search?q=term`) are INTENTIONAL. Missing index/backmatter is INTENTIONAL.
- **Publisher patterns**: Reference `../_shared/publisher-patterns.md` for publisher-specific markup patterns
- **Be precise with evidence**: Always show exact file paths, line numbers, and markup snippets in the diagnosis

## Codebase References

These existing utilities can be used for programmatic comparison:
- `content_diff.py` -- `compare_content(epub_path, rittdoc_path)` for automated content comparison
- `publisher_config.py` -- `detect_publisher()`, `get_css_mapping()` for publisher-aware analysis
- `epub_metadata.py` -- `detect_publisher(book, epub_path)` with built-in patterns for 8 publishers
- `validation_report.py` -- `ValidationReportGenerator` for structured error reporting
