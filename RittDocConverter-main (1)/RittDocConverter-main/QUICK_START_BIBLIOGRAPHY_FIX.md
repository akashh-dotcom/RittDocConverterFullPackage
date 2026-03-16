# Quick Start: Fix Bibliography TOC Links

## The Problem

REFERENCES sections appear in your TOC as plain text instead of clickable links:

```xml
<!-- Wrong: No link -->
<listitem role="contentsH3">
  <para><phrase>REFERENCES</phrase></para>
</listitem>

<!-- Right: With link -->
<listitem role="contentsH3">
  <para><ulink url="ch0019#ch0019s0002">REFERENCES</ulink></para>
</listitem>
```

## The Cause

Your bibliography elements have incorrect IDs:
- ❌ Wrong: `<bibliography id="ch0004s0002bib">`
- ✅ Right: `<bibliography id="ch0004s0002">`

The `bib` suffix should only be on `<bibliomixed>` entries, not the wrapper.

## The Fix

Run this ONE command:

```bash
python3 fix_bibliography_toc_links.py /path/to/your/xml/files/
```

That's it! The script will:
1. Find all bibliography elements with wrong IDs
2. Fix them to the correct format
3. Update all TOC links automatically
4. Save the corrected files

## Verify the Fix

Run the tests:

```bash
python3 test_fix_bibliography_toc_links.py
```

You should see:
```
✓ All 5 tests passed!
```

## Example

**Before running the fix:**
```xml
<bibliography id="ch0004s0002bib">
  <title>References</title>
  ...
</bibliography>

<!-- TOC has no link -->
<para><phrase>REFERENCES</phrase></para>
```

**After running the fix:**
```xml
<bibliography id="ch0004s0002">
  <title>References</title>
  ...
</bibliography>

<!-- TOC now has working link -->
<para><link linkend="ch0004s0002">REFERENCES</link></para>
```

## Need More Details?

See the full documentation: [docs/BIBLIOGRAPHY_TOC_LINK_FIX.md](docs/BIBLIOGRAPHY_TOC_LINK_FIX.md)
