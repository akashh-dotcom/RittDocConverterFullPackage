# Quick Fix: Section IDs Not Matching Document Order

## The Problem You're Seeing

REFERENCES is always `s0002` even though it appears at the end of your chapters:

```xml
<!-- TOC shows this order: -->
<ulink url="ch0013#ch0013s0001">CASE DISCUSSION</ulink>  <!--  ✓ First -->
<ulink url="ch0013#ch0013s0002">REFERENCES</ulink>        <!-- ❌ Says s0002 but is last! -->
<ulink url="ch0013#ch0013s0003">KEY LEARNING POINTS</ulink> <!-- ✓ Middle -->

<!-- But in the actual file, sections are: -->
<sect1 id="ch0013s0001">CASE DISCUSSION</sect1>
<sect1 id="ch0013s0003">KEY LEARNING POINTS</sect1>  
<sect1 id="ch0013s0002">REFERENCES</sect1>  <!-- ❌ Out of sequence! -->
```

## Why This Happens

1. During conversion, section IDs are assigned in the order they appear in the EPUB (s0001, s0002, s0003)
2. Then the DTD compliance code **moves** REFERENCES to the end (required by DTD)
3. But the IDs don't get renumbered after the move
4. Result: REFERENCES keeps `s0002` even though it's the last section

## The One-Command Fix

```bash
python3 renumber_section_ids.py /path/to/your/xml/files/
```

This will:
1. Scan all sections in each chapter
2. Renumber them to match their **actual position** (s0001, s0002, s0003, ...)
3. Update all TOC links and references automatically

## What You'll See

```bash
$ python3 renumber_section_ids.py extracted/

INFO: Found 45 XML file(s)
INFO: Processing ch0013.xml
INFO: ✓ Saved ch0013.xml: 2 sections renumbered, 3 references updated
INFO: Processing ch0014.xml
INFO: ✓ Saved ch0014.xml: 1 sections renumbered, 2 references updated

============================================================
✓ Renumbering complete:
  Sections renumbered: 89
  References updated: 267
  Files processed: 45
============================================================
```

## Result

**Before:**
```xml
<sect1 id="ch0013s0001">CASE DISCUSSION</sect1>
<sect1 id="ch0013s0003">KEY LEARNING POINTS</sect1>  
<sect1 id="ch0013s0002">REFERENCES</sect1>  <!-- ❌ Wrong -->

<ulink url="ch0013#ch0013s0002">REFERENCES</ulink>  <!-- ❌ Points to s0002 -->
```

**After:**
```xml
<sect1 id="ch0013s0001">CASE DISCUSSION</sect1>
<sect1 id="ch0013s0002">KEY LEARNING POINTS</sect1>  <!-- ✓ Now s0002 -->
<sect1 id="ch0013s0003">REFERENCES</sect1>  <!-- ✓ Now s0003 -->

<ulink url="ch0013#ch0013s0003">REFERENCES</ulink>  <!-- ✓ Updated -->
```

## When to Run This

Run this script:
- **After** EPUB to DocBook conversion
- **After** DTD compliance fixes
- **Before** final packaging/validation

## Technical Details

The script:
- Renumbers all sect1-sect5 elements in document order
- Updates all `linkend` attributes (for `<link>` elements)
- Updates all `url` attributes (for `<ulink>` elements)
- Updates element IDs that reference sections (like `ch0013s0002fig01`)
- Preserves the chapter structure and hierarchy

## See Also

- Full documentation: [docs/SECTION_RENUMBERING.md](docs/SECTION_RENUMBERING.md)
- Related: [R2_LINKEND_AND_TOC_RULESET.md](docs/R2_LINKEND_AND_TOC_RULESET.md)
