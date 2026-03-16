---
name: r2-code-fixer
description: >
  Fix the R2 EPUB-to-DocBook converter source code to prevent recurring conversion issues.
  Uses a diagnosis report to identify which code paths cause each issue, implements
  targeted fixes, adds regression tests, and validates with the full pipeline.
argument-hint: "[diagnosis report or issue list] [optional: test_epub.epub for regression testing]"
---

# R2 Code Fixer Skill

You are a converter pipeline specialist. Your job is to fix the EPUB-to-DocBook conversion source code so that issues found during QA are permanently resolved for all future conversions.

## Prerequisites
- A diagnosis report (from `r2-diagnosis` skill) identifying root causes
- Access to the converter codebase at the project root
- Optionally: an EPUB file for regression testing the fixes

## IMPORTANT CONSTRAINTS
- **Never break existing tests** — run `pytest tests/ -v` before and after changes
- **Minimal changes** — fix only what's needed, don't refactor unrelated code
- **Add regression tests** for every fix
- **Publisher-agnostic when possible** — fixes should work for all publishers, not just the one where the issue was found
- **If publisher-specific**: add the fix to `config/publishers/{publisher}.yaml` or `manual_postprocessor.py` rules, NOT hardcoded in the main converter
- **Preserve the pipeline architecture** — don't restructure the overall flow

## Converter Architecture Overview

The conversion pipeline has these stages (in `epub_pipeline.py`):

```
EPUB Input
  | Step 1: epub_to_structured_v2.py — XHTML -> structured DocBook XML
  | Step 2: xslt_transformer.py — XSLT compliance transformation
  | Step 3: package.py — Split into chapters, build TOC, package
  | Steps 4-11: Validation & post-processing
  |   - comprehensive_dtd_fixer.py — Auto-fix DTD violations
  |   - manual_postprocessor.py — Publisher-specific rules
  |   - toc_nesting_fixer.py — TOC hierarchy
  |   - title_synchronizer.py — Title consistency
  |   - validate_rittdoc.py — Final validation
ZIP Output
```

## Issue -> Code Path Mapping

Use this reference to find the right code to fix:

| Issue Category | Module | Key Functions |
|---------------|--------|---------------|
| Whitespace/spacing | `epub_to_structured_v2.py` | `extract_inline_content()` (~line 11782), `_normalize_inline_whitespace()` (~line 11134), `process_node()` |
| Metadata concatenation | `epub_to_structured_v2.py` | Copyright/metadata block handling — search for `ChapterCopyright`, `CopyrightHolderName` |
| Callout boxes | `epub_to_structured_v2.py` | `transform_xhtml_element()` — search for `FormalPara`, `ParaType` |
| Table numbering | `epub_to_structured_v2.py` | Table processing — search for `Tab1`, `Taba`, `CaptionNumber` |
| Zero-width space | `epub_to_structured_v2.py` | Text normalization — search for `\u200b`, `normalize`, `whitespace` |
| Double periods | `epub_to_structured_v2.py` or `package.py` | Bibliography processing — search for `bibliomixed`, `CitationContent`, `BibliographyWrapper` |
| Encoding issues | `epub_to_structured_v2.py` | Encoding handling — search for `encoding`, `decode`, `latin`, `nbsp` |
| Lost link labels | `epub_to_structured_v2.py` | External ref processing — search for `ExternalRef`, `RefSource`, `PubMed`, `CrossRef` |
| DTD structure | `comprehensive_dtd_fixer.py` | Fix methods — search by element type |
| Publisher-specific | `manual_postprocessor.py` + `config/publishers/*.yaml` | Publisher rules and CSS class mappings |
| ID/reference | `id_authority.py` | `IDAuthority`, `ChapterRegistry`, `LinkendResolver` |
| Packaging/TOC | `package.py` | `_split_root()`, `_populate_toc_element()`, `_collect_known_ids()` |

## Workflow

### Step 1: Understand the Issue

For each issue from the diagnosis report:
1. Read the diagnosis including source markup, converted markup, and identified root cause
2. Understand exactly what transformation SHOULD have happened
3. Note whether this is publisher-specific or generic

### Step 2: Locate the Code Path

1. **Search for the relevant function** using the mapping table above:
   ```bash
   # Example: finding heading number handling
   grep -n 'HeadingNumber\|heading_number\|extract_inline' epub_to_structured_v2.py | head -20
   ```

2. **Read the function and understand the data flow**:
   - What input does it receive? (HTML element, attributes, parent context)
   - What output does it produce? (DocBook element, text content)
   - What intermediate transformations happen?

3. **Check for existing publisher config**:
   ```bash
   # Check if there's already a CSS mapping for the class
   grep -r 'FormalPara\|ParaTypeTip' config/publishers/
   grep -r 'FormalPara\|ParaTypeTip' publisher_config.py
   ```

4. **Check if there's an existing test**:
   ```bash
   grep -r 'HeadingNumber\|heading.*space' tests/ --include="*.py"
   ```

### Step 3: Implement the Fix

Follow these principles:

1. **Read surrounding code** — understand the full context (at least 50 lines above and below the fix point)
2. **Make minimal changes** — don't refactor, just fix the specific issue
3. **Add comments** — explain WHY the fix is needed, reference the known issue ID:
   ```python
   # Fix C-001: Preserve trailing space in HeadingNumber spans
   # Springer XHTML uses <span class="HeadingNumber">1.1 </span> with trailing space
   # that was being stripped by whitespace normalization
   ```
4. **Handle edge cases**:
   - Empty text content
   - Missing elements/attributes
   - Different publishers with different patterns
5. **Use publisher config when publisher-specific**:
   ```python
   # If the fix is Springer-specific, add to config not code:
   # config/publishers/springer.yaml:
   #   css_class_mappings:
   #     ParaTypeTip:
   #       element: tip
   #       role: null
   ```

### Step 4: Add Regression Tests

For each fix, add a test in the appropriate test file under `tests/`:

```python
# tests/test_heading_space_preservation.py

import pytest
from lxml import etree

class TestHeadingSpacePreservation:
    """Regression tests for C-001: Missing spaces in section headings."""

    def test_heading_number_trailing_space_preserved(self):
        """HeadingNumber span trailing space should be preserved in conversion."""
        source_html = '''
        <h2 class="Heading">
          <span class="HeadingNumber">1.1 </span>When to Suspect
        </h2>
        '''
        # Convert using the relevant function
        result = convert_heading(source_html)

        # Assert space is preserved
        emphasis = result.find(".//emphasis[@role='HeadingNumber']")
        assert emphasis is not None
        assert emphasis.text.endswith(' '), f"Trailing space lost: '{emphasis.text}'"
        # OR assert there's a space between emphasis and following text
        assert emphasis.tail is None or emphasis.tail.startswith(' ') or emphasis.text.endswith(' ')

    def test_heading_number_without_space_not_modified(self):
        """HeadingNumber without trailing space should not get one added."""
        # Edge case test
        pass
```

Test file naming convention:
- `tests/test_{module}_{issue}.py` for new test files
- Or add to existing `tests/test_{module}.py` if appropriate

### Step 5: Run Tests

```bash
# Run the specific new test
pytest tests/test_heading_space_preservation.py -v

# Run the full test suite to check for regressions
pytest tests/ -v --tb=short

# If any tests fail, fix them before proceeding
```

### Step 6: Integration Test (Optional but Recommended)

If an EPUB file is available for regression testing:

```bash
# Run the full pipeline on the test EPUB
python epub_pipeline.py <test_epub> --output /tmp/r2_code_fix_test/

# Compare output against previous conversion
python -c "
from content_diff import compare_content
result = compare_content('<test_epub>', '/tmp/r2_code_fix_test/output.zip')
print(f'Similarity: {result.similarity_scores}')
print(f'Content loss: {result.content_loss_percentage:.2f}%')
if result.content_loss_detected:
    print('WARNING: Content loss detected!')
"

# Verify the specific issue is fixed in the new output
grep -r '</emphasis>[A-Z]' /tmp/r2_code_fix_test/ --include="*.xml" -c
# Should show 0 for heading space fix
```

### Step 7: Generate Fix Report

```markdown
## Code Fix Report: [Issue IDs]
**Date:** [Date]
**Modules Modified:** [List]
**Tests Added:** [List]

### Changes Made

#### Fix 1: [Issue Title] (Known Issue [C-NNN])

**File:** `epub_to_structured_v2.py`
**Function:** `extract_inline_content()` (line ~11782)
**Change:** [Description of what was changed]

**Before:**
\```python
[original code]
\```

**After:**
\```python
[fixed code]
\```

**Rationale:** [Why this fix resolves the issue]
**Test:** `tests/test_heading_space_preservation.py::test_heading_number_trailing_space_preserved`

### Test Results
- New tests: N passed
- Full suite: N passed, 0 failed
- Integration test: [PASS/FAIL]

### Remaining Issues
[Any issues that could NOT be fixed in code, requiring XML-level fix or publisher escalation]
```

## Codebase Module Reference

| Module | Lines | Purpose |
|--------|-------|---------|
| `epub_to_structured_v2.py` | ~14,000 | Primary XHTML -> DocBook conversion |
| `package.py` | ~157,000 | Chapter splitting, TOC building, ZIP packaging |
| `comprehensive_dtd_fixer.py` | ~320,000 | 40+ DTD violation fix strategies |
| `manual_postprocessor.py` | ~68,000 | Publisher-specific post-processing rules |
| `docbook_builder.py` | ~92,000 | Content model validation, DTD rules |
| `publisher_config.py` | -- | Publisher CSS -> DocBook mapping config |
| `id_authority.py` | -- | Centralized ID generation and mapping |
| `epub_pipeline.py` | -- | Pipeline orchestration (12 steps) |
| `xslt_transformer.py` | -- | XSLT compliance transformation |
| `content_diff.py` | -- | Source vs converted content comparison |
| `validation_report.py` | -- | Validation error reporting (Excel) |

## Skills Chain

This skill is the final step in the R2 QA lifecycle:
1. **r2-book-qa** -> Find issues in the browser
2. **r2-diagnosis** -> Identify root causes by comparing source vs converted
3. **r2-xml-fixer** -> Fix the current book's XML output (immediate remediation)
4. **r2-code-fixer** (this skill) -> Fix the converter code (permanent fix for all future books)
