# Bibliography TOC Link Fix

## Problem Description

When processing EPUB files to DocBook XML, bibliography/REFERENCES sections sometimes appear in the Table of Contents (TOC) without clickable links, showing as plain text instead:

**Before (Broken):**
```xml
<listitem role="contentsH3">
  <para><phrase>REFERENCES</phrase></para>
</listitem>
```

**Expected (Working):**
```xml
<listitem role="contentsH3">
  <para><ulink url="ch0019#ch0019s0002">REFERENCES</ulink></para>
</listitem>
```

## Root Cause

The issue occurs when `<bibliography>` elements are assigned incorrect IDs that don't follow the R2 Library linkend format:

**Incorrect:** `<bibliography id="ch0004s0002bib">`  
**Correct:** `<bibliography id="ch0004s0002">`

According to the [R2_LINKEND_AND_TOC_RULESET.md](R2_LINKEND_AND_TOC_RULESET.md#part-4-bibliography-structure-rules), the `<bibliography>` wrapper element must use an 11-character sect1-style ID format (`chnnnnsnnnn`). The `bib` suffix is ONLY for individual `<bibliomixed>` entries inside the bibliography:

```xml
<!-- CORRECT FORMAT -->
<bibliography id="ch0021s0014">  <!-- 11-char sect1-style ID -->
  <title>References</title>
  <bibliomixed id="ch0021s0014bib01">Author, A. (2023)...</bibliomixed>  <!-- bib suffix here -->
  <bibliomixed id="ch0021s0014bib02">Author, B. (2024)...</bibliomixed>
</bibliography>

<!-- INCORRECT FORMAT -->
<bibliography id="ch0021s0014bib">  <!-- ❌ WRONG: bib suffix on wrapper -->
  <title>References</title>
  <bibliomixed id="ch0021s0014bib01">Author, A. (2023)...</bibliomixed>
</bibliography>
```

### Why This Causes TOC Link Failures

1. The TOC generation code creates links to bibliography sections using the expected 11-character format (e.g., `ch0004s0002`)
2. If the actual bibliography has an incorrect ID (e.g., `ch0004s0002bib`), the link target cannot be resolved
3. Links with unresolved targets are automatically converted to `<phrase>` elements to avoid IDREF validation errors
4. Result: REFERENCES appears in the TOC as plain text without a link

## Solution

The `fix_bibliography_toc_links.py` script fixes this issue by:

1. Finding all `<bibliography>` elements with incorrect IDs (ending in 'bib')
2. Correcting the ID to the proper 11-character format
3. Updating all `linkend` and `url` attributes that reference the old ID

## Usage

### Basic Usage

```bash
python3 fix_bibliography_toc_links.py /path/to/xml/files/
```

This will:
- Scan all `.xml` files in the directory
- Fix bibliography IDs that incorrectly end with 'bib'
- Update all references (linkend, url) to the corrected IDs
- Save the modified files

### Example Output

```
INFO: Found 45 XML files in /path/to/xml/files
INFO: Fixed bibliography ID: ch0004s0002bib -> ch0004s0002 in ch0004.xml
INFO: Saved ch0004.xml: 1 bibliographies fixed, 3 references updated
INFO: Fixed bibliography ID: ch0019s0002bib -> ch0019s0002 in ch0019.xml
INFO: Saved ch0019.xml: 1 bibliographies fixed, 5 references updated

Summary:
  Bibliography IDs fixed: 2
  References updated: 8
  Cross-file references updated: 0

✓ Done! Processed 45 files
```

## Testing

Run the test suite to verify the fix works correctly:

```bash
python3 test_fix_bibliography_toc_links.py
```

The test suite covers:
- Fixing bibliography IDs with 'bib' suffix
- Updating linkend references
- Updating ulink URL references
- Not modifying correct bibliography IDs
- Handling multiple bibliographies in one file

## Related Documentation

- [R2_LINKEND_AND_TOC_RULESET.md](R2_LINKEND_AND_TOC_RULESET.md) - Complete linkend format rules
  - See Part 4: Bibliography Structure Rules
  - Section 4.3: Bibliography ID Format
- [toctransform.xsl](/pipeline_tester/xsl/toctransform.xsl) - XSL that generates TOC from book structure
- [ritttoc.xsl](/R2LibraryCode/App/Xsl/ritttoc.xsl) - XSL that renders TOC for display

## Technical Details

### ID Format Rules

| Element Type | ID Format | Example |
|--------------|-----------|---------|
| Bibliography wrapper | `{chapter_id}s{4-digits}` (11 chars) | `ch0021s0014` |
| Bibliography entry | `{sect1_id}bib{2-digits}` | `ch0021s0014bib01` |

### Detection Logic

The script identifies incorrect bibliography IDs by checking:

1. ID length > 11 characters
2. ID ends with 'bib'
3. Removing 'bib' results in valid 11-character format: `^[a-z]{2}\d{4}s\d{4}$`

### Update Logic

When a bibliography ID is corrected from `ch0004s0002bib` to `ch0004s0002`:

1. **Direct linkend references:**
   ```xml
   <link linkend="ch0004s0002bib">...</link>
   → <link linkend="ch0004s0002">...</link>
   ```

2. **URL references in ulink:**
   ```xml
   <ulink url="ch0004#ch0004s0002bib">...</ulink>
   → <ulink url="ch0004#ch0004s0002">...</ulink>
   ```

3. **Fragment-only URLs:**
   ```xml
   <ulink url="#ch0004s0002bib">...</ulink>
   → <ulink url="#ch0004s0002">...</ulink>
   ```

## Integration with Processing Pipeline

This fix should be run AFTER:
- EPUB to DocBook conversion (`epub_to_structured_v2.py`)
- Bibliography structure normalization
- Initial ID generation

This fix should be run BEFORE:
- Final DTD compliance validation
- TOC XSL transformation
- Publishing/packaging

### Recommended Pipeline Position

```
1. epub_to_structured_v2.py
2. comprehensive_dtd_fixer.py
3. fix_bibliography_toc_links.py  ← INSERT HERE
4. toc_nesting_fixer.py
5. dtd_compliance.py (validation)
6. package.py
```

## Troubleshooting

### Issue: Script reports "ID already in use"

**Cause:** The corrected ID conflicts with an existing element.

**Solution:** This indicates a deeper structural issue - there may be duplicate sections or incorrectly nested bibliographies. Manual review required.

### Issue: References still not updated

**Cause:** TOC links may be in a different file (cross-file references).

**Solution:** The script's `fix_cross_file_references()` function handles this. Check logs for "Cross-file references updated" count.

### Issue: Bibliography still shows as phrase after fix

**Cause:** The original link target may not have been the bibliography ID.

**Solution:** 
1. Check if the bibliography actually exists with the expected ID
2. Verify the TOC entry uses the correct link format
3. Run `fix_toc_links_by_title_match()` from `epub_to_structured_v2.py`

## See Also

- [toc_nesting_fixer.py](../toc_nesting_fixer.py) - Fixes TOC structure issues
- [toc_linkend_validator.py](../toc_linkend_validator.py) - Validates TOC linkend references
- [epub_to_structured_v2.py](../epub_to_structured_v2.py) - Main conversion pipeline
