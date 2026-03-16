# RittDoc ID Naming Convention (v2.1)

> **Last Updated**: January 2026
> **For**: PDF and EPUB Conversion Pipelines

This document defines the comprehensive ID naming conventions used in RittDoc XML output. All IDs must conform to these rules for proper cross-reference resolution and XSL processing.

---

## 1. File/Chapter-Level IDs

| Element Type | Prefix | Example | Notes |
|-------------|--------|---------|-------|
| Chapter | `ch` | `ch0001` | Default for body content |
| Preface | `pr` | `pr0001` | Foreword, introduction, copyright, cover, etc. |
| Appendix | `ap` | `ap0001` | Also for epilogue, afterword |
| Dedication | `dd` | `dd0001` | |
| Glossary | `gl` | `gl0001` | Standalone glossary chapters |
| Bibliography | `bi` | `bi0001` | Standalone bibliography chapters |
| Index | `in` | `in0001` | |
| Part | `pt` | `pt0001` | Part containers |
| Subpart | `sp` | `sp0001` | Subparts/part intros |
| Table of Contents | `toc` | `toc0001` | TOC element in Book.XML |
| Table of Contents (file) | `tc` | `tc0001` | Standalone TOC chapter files |
| Copyright Page | `cp` | `cp0001` | |
| Front Matter | `fm` | `fm0001` | Generic front matter |
| Back Matter | `bm` | `bm0001` | Generic back matter |

**Format**: `{prefix}{4-digit-counter}` → Always 6 characters

**Note**: Colophon, Acknowledgments, Contributors, and About pages use `ch` prefix for XSL compatibility.

---

## 2. Section IDs (Hierarchical)

| Level | Format | Max Value | Example |
|-------|--------|-----------|---------|
| Sect1 | `{chapter}s{4-digit}` | 9999 | `ch0001s0001` |
| Sect2 | `{sect1}s{4-digit}` | 9999 | `ch0001s0001s0001` |
| Sect3 | `{sect2}s{2-digit}` | 99 | `ch0001s0001s0001s01` |
| Sect4 | `{sect3}s{2-digit}` | 99 | `ch0001s0001s0001s01s01` |
| Sect5 | `{sect4}s{2-digit}` | 99 | `ch0001s0001s0001s01s01s01` |

**Constraint**: Maximum ID length is **25 characters**. Sect3+ use 2-digit counters to stay within this limit.

---

## 3. Element IDs (Within Sections)

**Format**: `{section_id}{element_code}{sequence}`

### 3.1 XSL-Recognized Prefixes (work for cross-references)

These element codes are recognized by `link.ritt.xsl` and will properly resolve in cross-references:

| Element | Code | Example | Notes |
|---------|------|---------|-------|
| Figure | `fg` | `ch0001s0001fg0001` | Images, diagrams |
| Table | `ta` | `ch0001s0001ta0001` | |
| Equation | `eq` | `ch0001s0001eq0001` | |
| Glossary Entry | `gl` | `gl0001s0001gl0001` | Terms in glossary |
| Bibliography Entry | `bib` | `bi0001s0001bib0001` | Citations |
| Q&A Entry | `qa` | `ch0001s0001qa0001` | Questions/answers |
| Procedure | `pr` | `ch0001s0001pr0001` | Step procedures |
| Video | `vd` | `ch0001s0001vd0001` | |
| Admonition/Sidebar | `ad` | `ch0001s0001ad0001` | note, warning, tip, caution, important, sidebar |

### 3.2 Non-XSL Prefixes (valid anchors but won't resolve in popup links)

These are valid for internal anchors but cross-references using these won't generate popup links:

| Element | Code | Example | Notes |
|---------|------|---------|-------|
| Anchor | `a` | `ch0001s0001a0001` | Generic anchors |
| Paragraph | `p` | `ch0001s0001p0001` | |
| List | `l` | `ch0001s0001l0001` | |
| Example | `ex` | `ch0001s0001ex0001` | |
| Footnote | `fn` | `ch0001s0001fn0001` | |
| Mediaobject | `mo` | `ch0001s0001mo0001` | |
| Index Term | `ix` | `ch0001s0001ix0001` | Markers, not index chapters |
| Step | `st` | `ch0001s0001st0001` | Procedure steps |
| Substep | `ss` | `ch0001s0001ss0001` | |
| Blockquote | `bq` | `ch0001s0001bq0001` | |
| Term | `tm` | `ch0001s0001tm0001` | Definition terms |

---

## 4. ID Structure Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    COMPLETE ID STRUCTURE                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ch0001s0001s0002fg0001                                         │
│  ├─────┼────┼────┼──┼───┤                                       │
│  │     │    │    │  │   └── Sequence (4-digit, 1-based)         │
│  │     │    │    │  └────── Element code (fg=figure)            │
│  │     │    │    └───────── Sect2 number (4-digit)              │
│  │     │    └────────────── Sect1 number (4-digit)              │
│  │     └─────────────────── Section marker 's'                  │
│  └───────────────────────── Chapter prefix + number             │
│                                                                 │
│  Maximum length: 25 characters                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Examples by Depth

| ID | Description |
|----|-------------|
| `ch0001` | Chapter 1 |
| `ch0001s0001` | Chapter 1, Section 1 |
| `ch0001s0001fg0001` | Chapter 1, Section 1, Figure 1 |
| `ch0001s0001s0002` | Chapter 1, Section 1, Subsection 2 |
| `ch0001s0001s0002ta0003` | Chapter 1, Section 1, Subsection 2, Table 3 |
| `ap0001s0001bib0001` | Appendix 1, Section 1, Bibliography Entry 1 |
| `pt0001` | Part 1 |
| `pr0001s0001` | Preface 1, Section 1 |

---

## 5. Valid ID Patterns (Regex)

Use these patterns for validation:

```python
import re

# Chapter/File ID (6 characters)
CHAPTER_ID_PATTERN = re.compile(r'^[a-z]{2}\d{4}$')
# Examples: ch0001, pr0001, ap0001, in0001

# Section ID (any depth)
SECTION_ID_PATTERN = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})+$')
# Examples: ch0001s0001, ch0001s0001s0001, ch0001s0001s0001s01

# Element ID (section + element code + sequence)
ELEMENT_ID_PATTERN = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})+[a-z]{1,3}\d{2,4}$')
# Examples: ch0001s0001fg0001, ch0001s0001s0001ta0001

# Base ID (first 11 chars - used for file resolution from linkend)
BASE_ID_PATTERN = re.compile(r'^([a-z]{2}\d{4})s(\d{4})')
# Extract: chapter_id = group(1), first_section = group(2)

# Full ID validation (any valid ID)
VALID_ID_PATTERN = re.compile(r'^[a-z]{2}\d{4}(s\d{2,4})*([a-z]{1,3}\d{2,4})?$')
```

---

## 6. Important Rules

### 6.1 Character Rules
- **All IDs are lowercase alphanumeric only** - no hyphens, underscores, or special characters
- **Maximum ID length is 25 characters** - enforced by R2 Library constraints

### 6.2 Counter Rules
- **All counters are 1-based** - first section is `s0001`, first figure is `fg0001`
- **Exception**: Chapter-level section marker `s0000` represents the chapter root (before any sect1)
- **Counters reset per section** - each sect1 starts fresh with `fg0001`, `ta0001`, etc.
- **Each element type has its own counter** - figures and tables count independently

### 6.3 Depth Rules
- **Sect1-2 use 4-digit counters** (max 9999 sections)
- **Sect3+ use 2-digit counters** (max 99 sections) to stay within 25-char limit
- **Deep nesting may truncate** - avoid sect4+ when possible

### 6.4 XSL Compatibility
- **Use XSL-recognized codes for cross-references**: `fg`, `ta`, `eq`, `gl`, `bib`, `qa`, `pr`, `vd`, `ad`
- **Non-recognized codes** work as anchors but won't generate popup links in R2 Library

---

## 7. ID Mapping System

When converting from EPUB/PDF, the system maintains mappings from source IDs to generated IDs.

### 7.1 Chapter Mapping

Maps original source files to chapter IDs:

```
chapter_map = {
    "chapter16.xhtml": "ch0016",
    "preface.xhtml": "pr0001",
    "appendix-a.xhtml": "ap0001",
    "OEBPS/Text/chapter16.xhtml": "ch0016",  # Multiple path variations
}
```

### 7.2 ID Mapping

Maps original element IDs to generated IDs (scoped by chapter):

```
id_mappings = {
    "ch0016:fig5-2": "ch0016s0001fg0003",
    "ch0016:table1": "ch0016s0001ta0001",
    "pr0001:intro": "pr0001s0001",
}
```

### 7.3 Link Resolution Example

```
Source: <a href="chapter16.xhtml#fig5-2">See Figure 5-2</a>
   ↓
Step 1: chapter_map["chapter16.xhtml"] → "ch0016"
Step 2: id_mappings["ch0016:fig5-2"] → "ch0016s0001fg0003"
   ↓
Result: <link linkend="ch0016s0001fg0003">See Figure 5-2</link>
```

---

## 8. Common Mistakes to Avoid

| Wrong | Correct | Issue |
|-------|---------|-------|
| `ch0001-s0001` | `ch0001s0001` | No hyphens allowed |
| `CH0001S0001` | `ch0001s0001` | Must be lowercase |
| `ch1s1` | `ch0001s0001` | Must use zero-padded numbers |
| `ch0001s0000fg0001` | `ch0001s0001fg0001` | Sections are 1-based (s0000 is chapter root only) |
| `ch0001f0001` | `ch0001s0001fg0001` | Elements must be in a section |
| `ch0001s0001f0001` | `ch0001s0001fg0001` | Use `fg` not `f` for figures |
| `ch0001s0001t0001` | `ch0001s0001ta0001` | Use `ta` not `t` for tables |
| `ch0001s0001b0001` | `ch0001s0001bib0001` | Use `bib` not `b` for bibliography |

---

## 9. File Naming Convention

XML files follow the chapter ID:

| Chapter ID | Filename |
|------------|----------|
| `ch0001` | `ch0001.xml` |
| `pr0001` | `pr0001.xml` |
| `ap0001` | `ap0001.xml` |
| `pt0001` | `pt0001.xml` |

Entity declarations in `Book.XML`:
```xml
<!DOCTYPE book [
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY ch0002 SYSTEM "ch0002.xml">
  <!ENTITY ap0001 SYSTEM "ap0001.xml">
]>
```

---

## 10. Quick Reference Card

```
CHAPTER PREFIXES:
  ch = chapter      pr = preface      ap = appendix
  dd = dedication   gl = glossary     bi = bibliography
  in = index        pt = part         sp = subpart
  toc = toc element tc = toc chapter

ELEMENT CODES (XSL-recognized):
  fg = figure       ta = table       eq = equation
  gl = glossentry   bib = biblio     qa = Q&A
  pr = procedure    vd = video       ad = admonition/sidebar

ELEMENT CODES (non-XSL):
  a = anchor        p = paragraph    l = list
  ex = example      fn = footnote    mo = mediaobject
  st = step         ss = substep     bq = blockquote

ID FORMAT:
  {prefix}{4-digit}s{4-digit}[s{2-4-digit}...]{code}{4-digit}
  └──────┬────────┘└───────────┬───────────┘└──────┬───────┘
       chapter              sections              element

MAX LENGTH: 25 characters
CASE: lowercase only
CHARACTERS: a-z, 0-9 only (no hyphens, underscores)
COUNTERS: 1-based (s0001, fg0001)
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.1 | Jan 2026 | Added source file tracking, TOC filtering for non-spine items |
| 2.0 | Jan 2026 | Complete rewrite with XSL-compatible element codes |
| 1.0 | Dec 2025 | Initial version |
