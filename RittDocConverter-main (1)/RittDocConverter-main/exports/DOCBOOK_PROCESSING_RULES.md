# DocBook XML Processing Rules Reference

This document provides a complete reference for all ID generation, DTD validation, and post-processing rules used in the RittDocConverter pipeline.

> **Note**: This document covers **stateless rules and formats**. For the full **stateful ID management system** (IDAuthority) that tracks chapter mappings, ID lifecycles, cascading updates, and linkend resolution, see [ID_AUTHORITY_REFERENCE.md](./ID_AUTHORITY_REFERENCE.md).

---

## Table of Contents

1. [ID Generation Rules](#1-id-generation-rules)
2. [DTD Validation Rules](#2-dtd-validation-rules)
3. [Post-Processing Rules](#3-post-processing-rules)
4. [File Naming Conventions](#4-file-naming-conventions)

---

## 1. ID Generation Rules

### 1.1 Core Constraints

| Constraint | Value | Notes |
|------------|-------|-------|
| Maximum ID Length | **25 characters** | R2 Library hard limit |
| Allowed Characters | `a-z`, `0-9` | Lowercase only, no special chars |
| Counter Base | 1-based | All counters start at 1 (s0001, fg0001) |

### 1.2 Chapter ID Format

**Format**: `{prefix}{4-digit-number}`
**Length**: Always 6 characters

| Prefix | Content Type | Example |
|--------|--------------|---------|
| `ch` | Chapter | `ch0001` |
| `ap` | Appendix | `ap0001` |
| `pr` | Preface | `pr0001` |
| `dd` | Dedication | `dd0001` |
| `pt` | Part | `pt0001` |
| `gl` | Glossary | `gl0001` |
| `bi` | Bibliography | `bi0001` |
| `in` | Index | `in0001` |
| `tc` | TOC | `tc0001` |
| `cp` | Copyright | `cp0001` |
| `fm` | Front matter | `fm0001` |
| `bm` | Back matter | `bm0001` |

### 1.3 Section ID Hierarchy

Section IDs are hierarchical, building on the parent section ID:

| Level | Format | Digits | Max Value | Example |
|-------|--------|--------|-----------|---------|
| Sect1 | `{chapter}s{4-digit}` | 4 | 9999 | `ch0001s0001` |
| Sect2 | `{sect1}s{4-digit}` | 4 | 9999 | `ch0001s0001s0001` |
| Sect3 | `{sect2}s{2-digit}` | 2 | 99 | `ch0001s0001s0001s01` |
| Sect4 | `{sect3}s{2-digit}` | 2 | 99 | `ch0001s0001s0001s01s01` |
| Sect5 | `{sect4}s{2-digit}` | 2 | 99 | `ch0001s0001s0001s01s01s01` |

**Rationale**: Levels 3-5 use 2-digit counters to stay within the 25-character limit.

### 1.4 Element ID Format

**Format**: `{section_id}{element_code}{sequence}`

```
ch0001s0001fg0001
└──┬──┘└─┬─┘└┬┘└─┬─┘
   │     │   │   └── 4-digit sequence number
   │     │   └────── Element code (2-3 chars)
   │     └────────── Section suffix (sect1)
   └──────────────── Chapter prefix
```

### 1.5 Element Type to Code Mapping

#### XSL-Recognized Codes (support cross-reference popups)

| Element Type | Code | Notes |
|--------------|------|-------|
| `figure`, `informalfigure` | `fg` | Figures |
| `table`, `informaltable` | `ta` | Tables |
| `bibliomixed`, `biblioentry` | `bib` | Bibliography entries |
| `equation`, `informalequation` | `eq` | Equations |
| `glossentry`, `glossterm` | `gl` | Glossary entries |
| `qandaset`, `qandaentry` | `qa` | Q&A entries |
| `procedure` | `pr` | Procedures |
| `video`, `videoobject` | `vd` | Video content |
| `note`, `warning`, `tip`, `caution`, `important`, `sidebar` | `ad` | Admonitions |

#### Non-XSL-Recognized Codes (basic anchors only)

| Element Type | Code | Notes |
|--------------|------|-------|
| `anchor` | `a` | Generic anchors |
| `itemizedlist`, `orderedlist`, `variablelist` | `l` | Lists |
| `listitem` | `li` | List items |
| `para`, `simpara`, `formalpara` | `p` | Paragraphs |
| `example`, `informalexample` | `ex` | Examples |
| `footnote` | `fn` | Footnotes |
| `mediaobject`, `imageobject` | `mo` | Media objects |
| `step` | `st` | Procedure steps |
| `substep` | `ss` | Procedure substeps |
| `blockquote`, `epigraph` | `bq` | Block quotes |
| `term` | `tm` | Terms |
| `indexterm` | `ix` | Index terms |
| `simplesect` | `sc` | Simple sections |

**Important**: Section elements (`sect1`-`sect5`) do NOT use element codes. They use hierarchical section IDs.

### 1.6 ID Validation Patterns

```python
# Chapter: 2-letter prefix + 4 digits
CHAPTER_PATTERN = r'^[a-z]{2}\d{4}$'

# Section: chapter + one or more section suffixes
SECTION_PATTERN = r'^[a-z]{2}\d{4}(s\d{2,4})+$'

# Element: section + element code + sequence
ELEMENT_PATTERN = r'^[a-z]{2}\d{4}(s\d{2,4})*[a-z]{1,3}\d{1,4}$'
```

---

## 2. DTD Validation Rules

### 2.1 Elements Requiring Content

These elements MUST NOT be empty:

```
Hierarchical:     sect1-sect5, section, simplesect, chapter, appendix,
                  preface, part, dedication, colophon

Lists:            itemizedlist, orderedlist, variablelist, simplelist,
                  segmentedlist, calloutlist, glosslist

List Items:       listitem, varlistentry, callout, glossentry

Admonitions:      note, warning, caution, important, tip

Block Elements:   blockquote, epigraph, footnote, sidebar, abstract,
                  authorblurb, personblurb, legalnotice

Formal Objects:   figure, informalfigure, example, informalexample,
                  mediaobject, inlinemediaobject

Tables:           table, informaltable, tgroup, thead, tfoot, tbody, row

Bibliography:     bibliography, bibliodiv, glossary, glossdiv, glossdef

Index:            indexdiv, indexentry

Procedures:       procedure, step, substeps, stepalternatives

Q&A:              question, qandaentry
```

### 2.2 Content Model Definitions

#### divcomponent.mix (Valid for sections)

```
para, simpara, formalpara, programlisting, literallayout, screen, synopsis,
address, blockquote, epigraph, figure, informalfigure, table, informaltable,
example, informalexample, equation, informalequation, procedure, sidebar,
mediaobject, graphic, itemizedlist, orderedlist, variablelist, simplelist,
segmentedlist, calloutlist, glosslist, note, warning, caution, important, tip,
bridgehead, remark, highlights, abstract, cmdsynopsis, funcsynopsis,
classsynopsis, revhistory, task, productionset, constraintdef, msgset,
qandaset, anchor, indexterm, beginpage
```

#### legalnotice.mix (Valid for dedication)

**Very limited content allowed:**

```
Lists:      glosslist, itemizedlist, orderedlist
Admon:      caution, important, note, tip, warning
Code:       literallayout, programlisting, screen, synopsis, address
Text:       formalpara, para, simpara, blockquote
Markers:    indexterm, beginpage
```

**NOT allowed in dedication**: `anchor`, `sect1`, `figure`, `table`, `sidebar`, `mediaobject`, `graphic`, `example`, `procedure`, `bridgehead`

### 2.3 Element Ordering Requirements

#### Chapter/Preface/Appendix Content Model

```
Element
├── beginpage?              (optional, MUST be first if present)
├── *info?                  (chapterinfo/prefaceinfo/etc.)
├── title                   (REQUIRED)
├── subtitle?               (optional)
├── titleabbrev?            (optional)
└── CONTENT
    ├── Pattern A: (divcomponent.mix+, sect1*)  -- Blocks THEN sections
    └── Pattern B: (sect1+)                      -- Sections only
```

**Critical Rule**: Once a `sect1` appears, NO block content can appear after it.

#### Section Content Model

```
sect1
├── sect1info?              (optional, MUST be first)
├── title                   (REQUIRED)
├── subtitle?               (optional)
├── titleabbrev?            (optional)
└── CONTENT (divcomponent.mix + sect2/simplesect)
```

### 2.4 Common DTD Errors and Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `bridgehead` in `indexdiv` | DTD doesn't allow bridgehead in indexdiv | Convert to `para` with `emphasis` |
| `bridgehead` in `abstract` | DTD doesn't allow bridgehead in abstract | Convert to `para` with `emphasis` |
| `para` in `table/row` | para not allowed directly in row | Wrap in `entry` element |
| `sect1` in `indexdiv` | DTD doesn't allow sections in indexdiv | Remove sect1, keep content |
| `anchor` in `dedication` | anchor not in legalnotice.mix | Wrap in `para` element |
| Nested `table` | Tables cannot be nested in entry | Move outside or use informaltable |
| Block after `sect1` | Content order violation | Move before sect1 or wrap in new sect1 |
| Empty `indexdiv` | indexdiv requires indexentry | Add indexentry or remove |
| `para` in `bridgehead` | bridgehead only allows #PCDATA/inline | Extract text only |
| `para` in `subtitle` | subtitle only allows #PCDATA/inline | Extract text only |

### 2.5 XSL Elements Requiring IDs

These elements MUST have IDs for XSL processing:

```
Sections:       sect1, sect2, sect3, sect4, sect5
Formal:         table, figure, equation, informalfigure, informaltable
Admonitions:    note, sidebar, important, warning, caution, tip
Footnotes:      footnote
Book Parts:     chapter, appendix, preface, part, subpart
Q&A/Bib/Gloss:  qandaset, qandaentry, bibliomixed, biblioentry, glossentry
```

---

## 3. Post-Processing Rules

### 3.1 Spacing Fix Patterns

| Pattern | Regex | Example | Result |
|---------|-------|---------|--------|
| Digit + Uppercase | `(\d)([A-Z][a-z])` | `Chapter 1Introduction` | `Chapter 1 Introduction` |
| Digit + AllCaps | `(\d)([A-Z]{2,})` | `Section 2DNA` | `Section 2 DNA` |
| Lowercase + Digit | `([a-z])(\d)` | `page5` | `page 5` |
| Before Reference | `([a-z])(Chapter\|Section\|...)` | `seeChapter 10` | `see Chapter 10` |
| Lowercase + Uppercase | `([a-z])([A-Z][a-z]{3,})` | `endChapter` | `end Chapter` |
| Multiple Spaces | `[ \t]{2,}` | Multiple spaces | Single space |
| Space Before Punct | `\s+([.,;:!?])` | ` ,` | `,` |
| No Space After Punct | `([.,;:!?])([A-Za-z])` | `,text` | `, text` |
| Tabs | `\t+` | Tab characters | Single space |

**Application Order**: Patterns are applied in specific order to avoid conflicts.

### 3.2 Leading Number Stripping

Applied to auto-numbered elements:
- `orderedlist/listitem` - numbers are auto-generated
- `bibliography/bibliomixed` - numbers are auto-generated

**Patterns Matched**:

```
1. Text           →  Text
1: First item     →  First item
(1) Entry one     →  Entry one
[2] Second entry  →  Second entry
1.2.3 Complex     →  Complex
(1.2) Numbered    →  Numbered
```

### 3.3 Element Boundary Spacing

Fixes missing spaces at inline element boundaries:

```xml
<!-- Before -->
<para>see<link>Chapter 10</link></para>

<!-- After -->
<para>see <link>Chapter 10</link></para>
```

**Inline Elements Checked**:
```
link, xref, ulink, emphasis, phrase, literal, citetitle, quote,
foreignphrase, wordasword, firstterm, glossterm, acronym, abbrev,
citation, subscript, superscript
```

### 3.4 Content Preservation Validation

All transformations validate that content is preserved:

1. Normalize both original and modified text (remove whitespace)
2. Compare character counts
3. Allow additions (new spaces)
4. Reject if > 0.1% content is lost
5. Revert transformation if content loss detected

### 3.5 Publisher Detection

**Supported Publishers**:

| Config Name | Detection Patterns |
|-------------|-------------------|
| `wiley` | wiley, john wiley, jossey-bass, wiley-blackwell, wiley-vch |
| `springer` | springer, springer nature, springer-verlag |
| `elsevier` | elsevier, academic press, saunders, mosby |
| `pearson` | pearson, prentice hall, addison-wesley |
| `mcgraw-hill` | mcgraw-hill, mcgraw hill, mcgrawhill |
| `oxford` | oxford university press, oxford |
| `cambridge` | cambridge university press, cambridge |
| `taylor-francis` | taylor & francis, taylor and francis, routledge, crc press |

---

## 4. File Naming Conventions

### 4.1 XML File Names

| Content Type | Pattern | Example |
|--------------|---------|---------|
| Chapter | `ch{4-digit}.xml` | `ch0001.xml` |
| Preface | `pr{4-digit}.xml` | `pr0001.xml` |
| Appendix | `ap{4-digit}.xml` | `ap0001.xml` |
| Dedication | `dd{4-digit}.xml` | `dd0001.xml` |
| Part | `pt{4-digit}.xml` | `pt0001.xml` |
| Glossary | `gl{4-digit}.xml` | `gl0001.xml` |
| Bibliography | `bi{4-digit}.xml` | `bi0001.xml` |
| Index | `in{4-digit}.xml` | `in0001.xml` |
| TOC | `toc.{isbn}.xml` | `toc.9781234567890.xml` |

### 4.2 Entity Declarations in book.xml

```xml
<!DOCTYPE book [
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY ch0002 SYSTEM "ch0002.xml">
  <!ENTITY ap0001 SYSTEM "ap0001.xml">
]>

<book>
  &pr0001;
  &ch0001;
  &ch0002;
  &ap0001;
</book>
```

---

## 5. Quick Reference

### ID Length Calculation

```
Chapter:  6 chars  (ch0001)
+ Sect1:  5 chars  (s0001)     = 11 chars
+ Sect2:  5 chars  (s0001)     = 16 chars
+ Sect3:  3 chars  (s01)       = 19 chars
+ Sect4:  3 chars  (s01)       = 22 chars
+ Sect5:  3 chars  (s01)       = 25 chars (MAX)

Element code + number: 6-7 chars (fg0001, bib0001)
```

### Validation Checklist

- [ ] All IDs are lowercase alphanumeric only
- [ ] All IDs are <= 25 characters
- [ ] All sect1-5 elements have unique IDs
- [ ] All figures, tables, footnotes have IDs
- [ ] No block content after first sect1 in chapter
- [ ] All required elements have title
- [ ] No empty containers (empty lists, empty sections)
- [ ] beginpage before title in chapter/preface/appendix
- [ ] No anchor directly in dedication (wrap in para)

---

## Usage in PDF Pipeline

```python
from exports.docbook_processing_rules import (
    # ID Generation
    generate_section_id,
    generate_element_id,
    next_available_sect1_id,
    ELEMENT_TYPE_TO_CODE,
    MAX_ID_LENGTH,

    # DTD Validation
    ELEMENTS_REQUIRING_CONTENT,
    DIVCOMPONENT_MIX,
    LEGALNOTICE_MIX,
    ELEMENT_ORDERING,

    # Post-Processing
    fix_spacing_in_text,
    strip_leading_numbers,
    validate_content_preserved,
)

# Generate IDs
section_counters = {}
sect1_id = generate_section_id('ch0001', 1, section_counters)  # ch0001s0001

element_counters = {}
fig_id = generate_element_id(sect1_id, 'figure', element_counters)  # ch0001s0001fg0001

# Fix spacing
text, modified, changes = fix_spacing_in_text("Chapter 1Introduction to DNA")
# Result: "Chapter 1 Introduction to DNA"

# Strip leading numbers
text, modified, number = strip_leading_numbers("1. First item in list")
# Result: "First item in list"
```
