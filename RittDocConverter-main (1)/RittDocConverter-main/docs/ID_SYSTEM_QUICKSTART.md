# ID System Quick Start Guide

> **For developers new to the RittDocConverter ID system**
>
> Last Updated: February 2026

---

## 5-Minute Overview

The ID system manages all identifier operations during EPUB to DocBook XML conversion:

```
EPUB Source IDs  →  ID Authority  →  Generated XML IDs
   "Fig1"              maps to        "ch0001s0001fg0001"
   "CR1"               maps to        "ch0001bib0001"
   "Table_1"           maps to        "ch0001s0002ta0001"
```

**One module to remember:** `id_authority.py`

---

## Quick Start Code

### Basic Usage

```python
from id_authority import get_authority

# Get the singleton authority instance
authority = get_authority()

# Register a chapter
authority.register_chapter("chapter1.xhtml", "ch0001")

# Map an ID (source → generated)
authority.map_id("ch0001", "Fig1", "ch0001s0001fg0001", "figure")

# Resolve a linkend
resolved = authority.resolve_linkend("Fig1", "ch0001")
# Returns: "ch0001s0001fg0001"

# Mark an ID as intentionally dropped
authority.mark_dropped("ch0001", "pagebreak1", "pagebreak element")

# Check if an ID was dropped
if authority.is_dropped("ch0001", "pagebreak1"):
    print("This ID was dropped")
```

### Pre-scanning EPUB Files

```python
from id_authority import prescan_file, register_prescanned_file
from pathlib import Path

# Scan an EPUB file for all IDs
source_ids = prescan_file(Path("chapter1.xhtml"), xhtml_content)

# Register scanned IDs with authority
register_prescanned_file("chapter1.xhtml", source_ids)
```

### Validated Element Creation

```python
from epub_to_structured_v2 import validated_subelement, validated_append

# Create elements with content model validation
para = validated_subelement(section, 'para', text="Hello world")
figure = validated_subelement(section, 'figure')
title = validated_subelement(figure, 'title', text="Figure 1")

# Append existing elements with validation
validated_append(section, existing_element)
```

### Export for Debugging

```python
from id_authority import get_authority
from pathlib import Path

authority = get_authority()

# Export to JSON
authority.export_registry(Path("id_registry.json"))

# Get statistics
stats = authority.get_stats()
print(f"Mapped: {stats['ids_mapped']}, Dropped: {stats['ids_dropped']}")
```

---

## Element Codes Reference

Common element type → code mappings:

| Element Type | Code | XSL Popup? |
|-------------|------|------------|
| figure | `fg` | ✓ Yes |
| table | `ta` | ✓ Yes |
| bibliography | `bib` | ✓ Yes |
| footnote | `fn` | No |
| para | `p` | No |
| anchor | `a` | No |
| section | `sc` | No |
| sidebar | `ad` | ✓ Yes |

Get the code for any element:
```python
from id_authority import get_element_code
code = get_element_code('figure')  # Returns: 'fg'
```

---

## ID Format

Generated IDs follow this format:

```
ch0001s0001fg0001
│    │    │    │
│    │    │    └── Element number (0001)
│    │    └─────── Element code (fg = figure)
│    └──────────── Section number (s0001)
└───────────────── Chapter ID (ch0001)
```

Chapter prefixes:
- `ch` = Chapter
- `ap` = Appendix
- `pr` = Preface
- `in` = Index
- `gl` = Glossary
- `bi` = Bibliography

---

## Common Tasks

### Task 1: Handle a link reference

```python
from id_authority import get_authority

def process_link(href, current_chapter):
    authority = get_authority()

    # Extract fragment ID from href
    if '#' in href:
        fragment = href.split('#')[1]

        # Resolve to generated ID
        resolved = authority.resolve_linkend(fragment, current_chapter)

        if resolved:
            return resolved
        elif authority.is_dropped(current_chapter, fragment):
            return None  # Link target was dropped
        else:
            return fragment  # Keep original if not mapped
    return None
```

### Task 2: Create elements safely

```python
from epub_to_structured_v2 import validated_subelement

def create_figure(parent, fig_id, title_text, image_src):
    # All these are validated against DTD content model
    figure = validated_subelement(parent, 'figure', id=fig_id)
    title = validated_subelement(figure, 'title', text=title_text)
    mediaobj = validated_subelement(figure, 'mediaobject')
    imageobj = validated_subelement(mediaobj, 'imageobject')
    imagedata = validated_subelement(imageobj, 'imagedata', fileref=image_src)
    return figure
```

### Task 3: Debug ID resolution

```python
from id_authority import get_authority

authority = get_authority()

# Get full resolution details
result = authority.resolve_linkend_full("Fig1", "ch0001")
print(f"Success: {result.success}")
print(f"Resolved: {result.resolved_id}")
print(f"Type: {result.resolution_type}")
print(f"Audit: {result.audit_trail}")
```

---

## Deprecation Monitoring

Track if any code uses deprecated patterns:

```python
from id_authority import deprecation_monitor

# Enable monitoring
deprecation_monitor.enable()

# Check a module
import some_module
deprecation_monitor.check_module(some_module)

# Get report
report = deprecation_monitor.get_report()
print(f"Deprecated patterns found: {report['total_warnings']}")
```

---

## Files Reference

| File | Purpose |
|------|---------|
| `id_authority.py` | **Single source of truth** - all ID operations |
| `epub_to_structured_v2.py` | Main converter with validated element creation |
| `docs/ID_AUTHORITY_ARCHITECTURE.md` | Full architecture documentation |

### Removed (Deprecated)

These modules have been removed. Importing them raises an error with migration instructions:

- ~~`id_tracker.py`~~ → Use `id_authority`
- ~~`id_mapper.py`~~ → Use `id_authority`

---

## Troubleshooting

### "No mapping found for ID"

The source ID wasn't registered during conversion:
```python
# Check if it was dropped
if authority.is_dropped(chapter_id, source_id):
    reason = authority.get_drop_reason(chapter_id, source_id)
    print(f"Dropped: {reason}")
```

### "Content model violation"

An element was created in an invalid location:
```python
# Check what children are valid
from docbook_builder import get_valid_children
valid = get_valid_children('section')
print(f"Valid children: {valid}")
```

### Debug the full audit trail

```python
authority = get_authority()
trail = authority.get_audit_trail(chapter_id, source_id)
for entry in trail:
    print(entry)
```

---

## Next Steps

1. Read the full [ID Authority Architecture](ID_AUTHORITY_ARCHITECTURE.md) for deep dives
2. Check [ID Naming Convention](ID_NAMING_CONVENTION.md) for format rules
3. See [Content Model Requirements](CONTENT_MODEL_REQUIREMENTS.md) for DTD validation

---

*Questions? Check the architecture docs or search for examples in `epub_to_structured_v2.py`*
