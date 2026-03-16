# XML ID Naming Convention Rules for R2 Library

> **DEPRECATED:** This document is deprecated and no longer maintained.
>
> **Please refer to the authoritative ruleset:**
> [`docs/R2_LINKEND_AND_TOC_RULESET.md`](docs/R2_LINKEND_AND_TOC_RULESET.md)
>
> Key differences in the authoritative ruleset:
> - Element codes updated to match XSLT processor (`f`→`fg`, `t`→`ta`, `b`→`bib`)
> - Section numbering uses 4-digit format (`s0001`), not 6-digit (`s000001`)
> - Includes bibliography structure rules (lists → bibliomixed conversion)
> - Complete TOC nesting rules
>
> *Deprecated: January 2026*

---

## Overview

This document defines the ID naming conventions for converting PDF and ePub documents to XML format for the R2 Library platform. Following these rules ensures that internal cross-references (`<link>`) resolve correctly.

---

## Core Constraint

| Rule | Value |
|------|-------|
| **Maximum ID length** | 25 characters |
| **Allowed characters** | Lowercase letters, numbers only |
| **Prohibited characters** | Hyphens (`-`), underscores (`_`), spaces, special characters |

---

## Why This Matters

R2 Library **parses the `linkend` value** to determine which file to load. It extracts the sect1 ID from the beginning of the linkend string.

```
linkend="ch0011s0000bib0001"
         └─────────┘└──────┘
         sect1 ID   element identifier
         (file name)(fragment)
```

If the sect1 ID is not correctly embedded at the start, R2 will navigate to the wrong file or fail entirely.

---

## File Structure Reminder

Documents are split at the **sect1 level**:

```
book/
├── sect1.{isbn}.ch0001s000000.xml    ← Chapter 1, Section 0
├── sect1.{isbn}.ch0001s001000.xml    ← Chapter 1, Section 1
├── sect1.{isbn}.ch0002s000000.xml    ← Chapter 2, Section 0
└── ...
```

The sect1 ID (e.g., `ch0001s000000`) determines the filename and must prefix all element IDs within that file.

---

## ID Format

```
{sect1_id}{element_code}{sequence_number}
```

| Component | Description | Example |
|-----------|-------------|---------|
| `sect1_id` | The sect1 ID where the element lives | `ch0011s000000` |
| `element_code` | Short code identifying element type | `b`, `t`, `f`, etc. |
| `sequence_number` | 3-4 digit sequential number | `001`, `0001` |

---

## Element Type Codes

| Element | Code | Example ID | Character Count |
|---------|------|------------|-----------------|
| Bibliography/Reference | `bib` | `ch0011s0000bib0001` | 19 |
| Table | `ta` | `ch0011s0000ta0001` | 18 |
| Figure | `fg` | `ch0011s0000fg0001` | 18 |
| Sidebar/Box | `ad` | `ch0011s0000ad0001` | 18 |
| Anchor | `a` | `ch0011s0000a0001` | 17 |
| Glossary Entry | `gl` | `ch0011s0000gl0001` | 18 |
| Paragraph | `p` | `ch0011s0000p0001` | 17 |
| Equation | `eq` | `ch0011s0000eq0001` | 18 |
| Q&A Set | `qa` | `ch0011s0000qa0001` | 18 |
| Admonition | `ad` | `ch0011s0000ad0001` | 18 |
| Section 2 | `s02` | `ch0011s0000s02` | 15 |
| Section 3 | `s03` | `ch0011s0000s02s03` | 18 |

---

## Sequence Numbering

### Standard (4 digits)
- Use when sect1 ID ≤ 13 characters
- Allows 0001-9999 per element type
- Example: `ch0011s0000bib0001`

### Compact (3 digits)
- Use when sect1 ID > 13 characters
- Allows 001-999 per element type
- Example: `ch0011s000000b001`

### Character Budget Calculation

```
Maximum ID length:           25 characters
Sect1 ID:                   -13 characters (typical)
Element code:                -2 characters (max)
                            ───
Available for number:        10 characters
```

Always calculate: `25 - len(sect1_id) - len(element_code) = digits available`

---

## Sect1 ID Format

The sect1 ID itself should follow this pattern:

```
ch{chapter_number}s{section_number}
```

| Component | Format | Example |
|-----------|--------|---------|
| Chapter prefix | `ch` | `ch` |
| Chapter number | 4 digits, zero-padded | `0011` |
| Section prefix | `s` | `s` |
| Section number | 6 digits, zero-padded | `000000` |

**Result:** `ch0011s000000` (13 characters)

---

## Complete Examples

### Bibliography Entry

```xml
<!-- In file: sect1.9781394266074.ch0011s000000.xml -->

<!-- Definition -->
<bibliomixed id="ch0011s0000bib0001">
   Marocchino, K.D. (2011). In the shadow of a rainbow...
</bibliomixed>

<!-- Reference (can be in same file or different file) -->
<link linkend="ch0011s0000bib0001">[1]</link>
```

### Table

```xml
<!-- In file: sect1.9781394266074.ch0011s000000.xml -->

<!-- Definition -->
<table id="ch0011s0000ta0001">
   <title>Caregiver psychosocial considerations</title>
   ...
</table>

<!-- Reference -->
See <link linkend="ch0011s0000ta0001">Table 1.1</link> for details.
```

### Figure

```xml
<!-- In file: sect1.9781394266074.ch0002s000000.xml -->

<!-- Definition -->
<figure id="ch0002s0000fg0001">
   <title>Anatomy diagram</title>
   <mediaobject>...</mediaobject>
</figure>

<!-- Reference from another section -->
As shown in <link linkend="ch0002s0000fg0001">Figure 2.1</link>...
```

### Sidebar/Box

```xml
<!-- Definition -->
<sidebar id="ch0011s0000ad0001">
   <title>Pro Tip 1.1</title>
   ...
</sidebar>

<!-- Reference -->
See <link linkend="ch0011s0000ad0001">Pro Tip 1.1</link>.
```

### Cross-Section Reference

```xml
<!-- In file: ch0011s000000.xml, referencing table in ch0011s003000.xml -->

<link linkend="ch0011s0030ta0001">Table 3.1</link>
```

---

## Link Type Usage

| Scenario | Element | Example |
|----------|---------|---------|
| Same-file reference | `link` | `<link linkend="ch0011s0000bib0001">[1]</link>` |
| Cross-section reference | `link` | `<link linkend="ch0012s0000ta0001">Table 2.1</link>` |
| Chapter-level navigation | `ulink` | `<ulink url="ch0020">Chapter 9</ulink>` |
| External website | `ulink` | `<ulink url="https://example.com">Example</ulink>` |
| Disease/drug lookup | `ulink` | `<ulink type="disease" url="link.aspx?id=4522">pain</ulink>` |

### Key Rule

> **Use `<link linkend="...">` for all internal element references (tables, figures, bibliography, sidebars, etc.)**
>
> **Use `<ulink url="...">` only for chapter navigation and external URLs.**

---

## Validation Rules

Implement these validation checks in the conversion pipeline:

### 1. Length Check
```python
def validate_id_length(id_value):
    if len(id_value) > 25:
        raise ValueError(f"ID exceeds 25 characters: {id_value} ({len(id_value)} chars)")
```

### 2. Character Check
```python
import re

def validate_id_characters(id_value):
    if not re.match(r'^[a-z0-9]+$', id_value):
        raise ValueError(f"ID contains invalid characters: {id_value}")
```

### 3. Prefix Check
```python
def validate_id_prefix(id_value, current_sect1_id):
    if not id_value.startswith(current_sect1_id):
        raise ValueError(f"ID {id_value} does not start with sect1 ID {current_sect1_id}")
```

### 4. Uniqueness Check
```python
def validate_unique_ids(all_ids):
    duplicates = [id for id in all_ids if all_ids.count(id) > 1]
    if duplicates:
        raise ValueError(f"Duplicate IDs found: {set(duplicates)}")
```

### 5. Link Target Check
```python
def validate_link_targets(links, all_ids):
    for linkend in links:
        if linkend not in all_ids:
            raise ValueError(f"Broken link: {linkend} not found in document")
```

---

## Migration: Converting Old IDs

When converting documents with existing non-compliant IDs:

### Old Format (Broken)
```xml
<bibliomixed id="ch0011-c1-bib-0001">...</bibliomixed>
<link linkend="ch0011-c1-bib-0001">[1]</link>
```

### New Format (Compliant)
```xml
<bibliomixed id="ch0011s0000bib0001">...</bibliomixed>
<link linkend="ch0011s0000bib0001">[1]</link>
```

### Conversion Steps

1. Build a mapping of old IDs → new IDs
2. Update all `id` attributes in element definitions
3. Update all `linkend` attributes in `<link>` elements
4. Validate all links resolve correctly

---

## Edge Cases

### 1. Long Chapter Numbers
If chapter numbers exceed 4 digits, adjust padding:
- Standard: `ch0011` (6 chars)
- Extended: `ch00011` (7 chars) — recalculate character budget

### 2. Multiple Element Types with Same Number
Each element type has its own sequence:
```xml
<table id="ch0011s0000ta0001">       <!-- Table 1 -->
<figure id="ch0011s0000fg0001">      <!-- Figure 1 -->
<bibliomixed id="ch0011s0000bib0001"> <!-- Bib 1 -->
```

### 3. Nested Sections
Even for elements inside sect2 or sect3, the ID still uses the **sect1 ID** as the prefix (since files are split at sect1 level):

```xml
<!-- File: ch0011s000000.xml -->
<sect1 id="ch0011s000000">
   <sect2 id="ch0011s000000s20001">        <!-- sect2 ID -->
      <table id="ch0011s0000ta0001">      <!-- table still uses sect1 prefix -->
      </table>
   </sect2>
</sect1>
```

### 4. Bibliography in Separate Section
If bibliography is in its own sect1 file (e.g., `ch0001s0031.xml`), use that sect1 ID:

```xml
<!-- File: ch0001s0031.xml (bibliography section) -->
<bibliomixed id="ch0001s0031bib0001">...</bibliomixed>

<!-- File: ch0001s0026.xml (content section) -->
<link linkend="ch0001s0031bib0001">[1]</link>
```

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────┐
│                  ID NAMING QUICK REFERENCE              │
├─────────────────────────────────────────────────────────┤
│ Format:     {sect1_id}{element_code}{sequence}          │
│ Max length: 25 characters                               │
│ Characters: lowercase letters and numbers only          │
├─────────────────────────────────────────────────────────┤
│ Element Codes:                                          │
│   b=bibliography  t=table     f=figure    sb=sidebar    │
│   a=anchor        l=list      p=para      eq=equation   │
│   ex=example      n=note      s2=sect2    s3=sect3      │
├─────────────────────────────────────────────────────────┤
│ Examples:                                               │
│   ch0011s0000bib0001  (bibliography)                    │
│   ch0011s0000ta0001  (table)                           │
│   ch0011s0000fg0001  (figure)                          │
├─────────────────────────────────────────────────────────┤
│ Links:                                                  │
│   Internal: <link linkend="ch0011s0000bib0001">         │
│   Chapter:  <ulink url="ch0020">                        │
│   External: <ulink url="https://...">                   │
└─────────────────────────────────────────────────────────┘
```

---

## Checklist for Code Review

- [ ] All IDs ≤ 25 characters
- [ ] All IDs contain only lowercase letters and numbers
- [ ] All IDs start with the correct sect1 ID
- [ ] All element types use correct codes
- [ ] All sequence numbers are properly padded
- [ ] All `<link linkend="...">` targets exist
- [ ] `<link>` used for internal refs, `<ulink>` for external/chapter
- [ ] No duplicate IDs within the document

---

## Document Version

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2026 | Zentrovia Solutions | Initial release |

