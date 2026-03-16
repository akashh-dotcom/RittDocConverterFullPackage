# DocBook XML Processing Ruleset for R2 Library

**Version:** 2.1  
**Last Updated:** January 21, 2026  
**Purpose:** Comprehensive validation and correction rules for converting ePub files to DocBook XML format for the R2 Library platform

---

## Table of Contents

1. [File Structure Overview](#1-file-structure-overview)
2. [File Naming Conventions](#2-file-naming-conventions)
3. [The Cardinal Rule](#3-the-cardinal-rule)
4. [Part Files](#4-part-files)
5. [Chapter Files](#5-chapter-files)
6. [Section IDs](#6-section-ids)
   - 6A. [Cross-File Linkend Processing](#6a-cross-file-linkend-processing)
7. [Element IDs](#7-element-ids)
   - 7.1A. [Malformed ID Detection & Reconstruction](#71a-malformed-id-detection-and-reconstruction)
8. [TOC Validation](#8-toc-validation)
9. [book.xml Structure](#9-bookxml-structure)
10. [Validation Checklist](#10-validation-checklist)
11. [Common Issues & Fixes](#11-common-issues--fixes)

---

## 1. File Structure Overview

A complete book package consists of:

| File Type | Naming Pattern | Purpose |
|-----------|----------------|---------|
| Book file | `book.{ISBN}.xml` | Master file with entity declarations and structure |
| TOC file | `toc.{ISBN}.xml` | Table of contents with all linkends |
| Preface files | `preface.{ISBN}.{ID}.xml` | Front matter (cover, copyright, dedication) |
| Part files | `sect1.{ISBN}.pt####.xml` | Part title pages |
| Chapter files | `sect1.{ISBN}.ch####s0000.xml` | Main content chapters |

**Variables:**
- `{ISBN}` = The book's 13-digit ISBN (e.g., `9781234567890`)
- `####` = 4-digit zero-padded number (e.g., `0001`, `0012`)

**Expected file count formula:**
```
Total = 1 (book) + 1 (TOC) + N (prefaces) + N (parts) + N (chapters)
```

---

## 2. File Naming Conventions

### 2.1 Preface Files
```
Pattern: preface.{ISBN}.pr####.xml
Example: preface.9781234567890.pr0001.xml

Root ID must match: <preface id="pr####"> or similar
```

### 2.2 Part Files
```
Pattern: sect1.{ISBN}.pt####.xml
Example: sect1.9781234567890.pt0001.xml

Root ID must match: <sect1 id="pt####">
```

**Part numbering:**
- pt0001 = Part I
- pt0002 = Part II
- pt0003 = Part III
- etc.

### 2.3 Chapter Files
```
Pattern: sect1.{ISBN}.ch####s0000.xml
Example: sect1.9781234567890.ch0011s0000.xml

Root ID must match: <sect1 id="ch####s0000">
```

### 2.4 Validation Rule
```
RULE: Filename fragment MUST equal root element ID

✅ CORRECT: sect1.{ISBN}.ch0011s0000.xml contains <sect1 id="ch0011s0000">
❌ WRONG:   sect1.{ISBN}.ch0011s0000.xml contains <sect1 id="ch0011">
```

---

## 3. The Cardinal Rule

> **CARDINAL RULE:** For any file with root ID `{ROOT_ID}`, ALL internal structural IDs MUST be prefixed with `{ROOT_ID}`

This is the most critical rule. Every sect2, sect3, figure, table, and bibliography ID inside a file must start with that file's root ID.

### Example for file with root ID `ch0005s0000`:

| Element | Correct ID | Wrong ID |
|---------|------------|----------|
| sect2 #1 | `ch0005s0000s0001` | `ch0004s0001` |
| sect2 #2 | `ch0005s0000s0002` | `ch0005s0001` |
| sect3 under s0002 | `ch0005s0000s0002s01` | `ch0004s0002s0001` |
| Figure in s0002 | `ch0005s0000s0002fg0001` | `ch0004s0002fg0001` |
| Table in s0002 | `ch0005s0000s0002ta0001` | `ch0004s0002ta0001` |
| Sidebar in s0002 | `ch0005s0000s0002ad0001` | `ch0004s0002sb0001` |
| Bibliography | `ch0005s0000s0006bib0001` | `ch0004s0006bib0001` |

### Why This Matters
- Ensures all cross-references resolve correctly
- Prevents ID collisions across files
- Enables proper navigation in R2 Library viewer

---

## 4. Part Files

### 4.1 Structure
Part files are "divider pages" that contain only:
- Title (e.g., "Part I Introduction")
- Empty paragraph or minimal content

```xml
<?xml version="1.0" encoding="UTF-8"?>
<sect1 id="pt0001">
   <sect1info>
      <risinfo>
         <risprev>sect1.{ISBN}.ch0009s0000</risprev>
         <riscurrent>sect1.{ISBN}.pt0001</riscurrent>
         <risnext>sect1.{ISBN}.ch0011s0000</risnext>
         ...
      </risinfo>
   </sect1info>
   <title>Part I Introduction</title>
   <para/>
</sect1>
```

### 4.2 Part ID Format
```
Pattern: pt#### (4 digits, zero-padded)
Examples: pt0001, pt0002, pt0003
```

### 4.3 Common Mistake
```
❌ WRONG: Part files using ch####s0000 format
✅ CORRECT: Part files using pt#### format
```

---

## 5. Chapter Files

### 5.1 Root Element
```xml
<sect1 id="ch####s0000">
```

The `s0000` suffix indicates this is the main chapter file (section 0).

### 5.2 Chapter-to-File Mapping
Chapter file numbers are assigned sequentially. When a book has Parts, the Part files use `pt####` format while chapters use `ch####s0000` format:

| Content | File ID | Notes |
|---------|---------|-------|
| Front Matter | pr0001, pr0002, etc. | Preface files |
| Part I | pt0001 | Part title page |
| Chapter 1 | ch0011s0000 | First chapter after Part I |
| Chapter 2 | ch0012s0000 | |
| Part II | pt0002 | Part title page |
| Chapter 5 | ch0017s0000 | First chapter after Part II |
| Back Matter | ch00XXs0000 | Index, Appendices |

**Note:** Chapter numbers may skip values when Parts are present. The converter assigns sequential IDs to all content units.

---

## 6. Section IDs

### 6.1 sect2 IDs (Level 2 Sections)
```
Pattern: {ROOT_ID}s#### (4 digits, zero-padded)
Example: ch0005s0000s0001, ch0005s0000s0002

For file ch0005s0000.xml:
  sect2 #1 → ch0005s0000s0001
  sect2 #2 → ch0005s0000s0002
  sect2 #3 → ch0005s0000s0003
```

### 6.2 sect3 IDs (Level 3 Sections)
```
Pattern: {PARENT_SECT2_ID}s## (2 digits, zero-padded to stay within 25-char limit)
Example: ch0005s0000s0002s01, ch0005s0000s0002s02

For sect3 under ch0005s0000s0002:
  sect3 #1 → ch0005s0000s0002s01
  sect3 #2 → ch0005s0000s0002s02
```

### 6.3 sect4 IDs (Level 4 Sections)
```
Pattern: {PARENT_SECT3_ID}s## (2 digits, zero-padded to stay within 25-char limit)
Example: ch0005s0000s0002s01s01

For sect4 under ch0005s0000s0002s01:
  sect4 #1 → ch0005s0000s0002s01s01
  sect4 #2 → ch0005s0000s0002s01s02
```

### 6.4 ID Length Reference
| Element | Pattern | Example | Length |
|---------|---------|---------|--------|
| Root (chapter) | ch####s#### | ch0005s0000 | 11 chars |
| sect2 | {root}s#### | ch0005s0000s0001 | 16 chars |
| sect3 | {sect2}s## | ch0005s0000s0001s01 | 19 chars |
| sect4 | {sect3}s## | ch0005s0000s0001s01s01 | 22 chars |
| sect5 | {sect4}s## | ch0005s0000s0001s01s01s01 | 25 chars |

> **Note:** Levels 1-2 use 4-digit counters (s0001), while levels 3-5 use 2-digit counters (s01) to ensure IDs stay within the 25-character maximum.

### 6.5 Section ID Hierarchy Visualization
```
ch0005s0000                       ← sect1 (file root)
├── ch0005s0000s0001              ← sect2 #1
│   ├── ch0005s0000s0001s01       ← sect3 #1 under sect2 #1
│   └── ch0005s0000s0001s02       ← sect3 #2 under sect2 #1
├── ch0005s0000s0002              ← sect2 #2
│   ├── ch0005s0000s0002s01       ← sect3 #1 under sect2 #2
│   │   └── ch0005s0000s0002s01s01  ← sect4 #1
│   └── ch0005s0000s0002s02       ← sect3 #2 under sect2 #2
└── ch0005s0000s0003              ← sect2 #3 (References)
    └── ch0005s0000s0003bib0001   ← Bibliography entry #1
```

---

## 6A. Cross-File Linkend Processing

### 6A.1 How the XSL Handles Cross-File Links (IMPORTANT)

> **KEY INSIGHT:** The downstream XSL stylesheets (`link.ritt.xsl`) **automatically handle cross-file link URL generation**. You do NOT need to embed `#` in linkend attributes - the XSL does this for you.

The XSL uses the following logic (from `link.ritt.xsl` line 70):
```xsl
<xsl:value-of select="substring(@linkend,1,11)"/>#goto=<xsl:value-of select="@linkend"/>
```

This means:
- The XSL extracts the **first 11 characters** of the linkend as the file identifier
- It automatically adds `#goto=<full linkend>` for element navigation
- **All you need to do is ensure IDs follow the correct pattern so the first 11 chars identify the file**

### 6A.2 Linkend Format (Simple!)

| Link Type | Format | Example |
|-----------|--------|---------|
| Any link | `linkend="{targetID}"` | `linkend="ch0007s0000s0002fg0001"` |

The XSL will automatically convert this to:
- URL: `ch0007s0000#goto=ch0007s0000s0002fg0001`

### 6A.3 Why This Works

The ID pattern is designed so that:
- **Chapter files** (ch####s####): First 11 chars = file identifier (e.g., `ch0007s0000`)
- **Part files** (pt####): First 6 chars = file identifier (e.g., `pt0001`)
- **Preface files** (pr####): First 6 chars = file identifier (e.g., `pr0001`)

Example ID breakdown:
```
ch0007s0000s0002fg0001
├───────────┤├───────┤
│           │└── Element within the file
│           └── Parent section
└── File identifier (first 11 chars)
```

### 6A.4 Linkend Examples

```xml
<!-- Link to a section in another file - just use the ID -->
<para>This is discussed in <link linkend="ch0007s0000s0002">Chapter 7, Section 2</link>.</para>
<!-- XSL generates: ch0007s0000#goto=ch0007s0000s0002 -->

<!-- Link to a figure in another file -->
<para>See <link linkend="ch0007s0000s0003fg0001">Figure 7.1</link>.</para>
<!-- XSL generates: ch0007s0000#goto=ch0007s0000s0003fg0001 -->

<!-- Link to just a chapter root -->
<para>Refer to <link linkend="ch0007s0000">Chapter 7</link>.</para>
<!-- XSL generates: ch0007s0000 (no #goto needed for root) -->
```

### 6A.5 The Critical Requirement

> **RULE:** All IDs must follow the pattern where the first 11 characters (for chapters) identify the file. This is enforced by the Cardinal Rule.

If IDs don't follow this pattern, the XSL will extract incorrect file identifiers and links will break.

```
✅ CORRECT: ch0005s0000s0002fg0001 → file ch0005s0000, element fg0001
✅ CORRECT: pt0001 → file pt0001 (no element suffix)

❌ WRONG: ch05-s02-fg01 → XSL extracts "ch05-s02-fg" (gibberish)
❌ WRONG: figure_5_1 → XSL extracts "figure_5_1" (not a valid file)
```

### 6A.6 Validation: ID Pattern Compliance

```python
import re

def validate_linkend_ids(work_dir, isbn):
    """
    Validate that all IDs follow patterns where first N chars identify the file.
    This ensures the XSL can correctly generate cross-file URLs.
    """
    errors = []

    # Valid patterns for IDs
    VALID_PATTERNS = [
        r'^ch\d{4}s\d{4}$',                     # Chapter root (11 chars)
        r'^ch\d{4}s\d{4}(s\d{4})+$',            # Section
        r'^ch\d{4}s\d{4}(s\d{4})*[a-z]{2,4}\d{4}$',  # Element
        r'^pt\d{4}$',                           # Part
        r'^pr\d{4}$',                           # Preface
        r'^ap\d{4}$',                           # Appendix
    ]

    for f in work_dir.glob("*.xml"):
        if f.name.startswith(("toc.", "book.")):
            continue

        content = f.read_text()
        for match in re.finditer(r'id="([^"]+)"', content):
            id_value = match.group(1)

            is_valid = any(re.match(p, id_value) for p in VALID_PATTERNS)
            if not is_valid:
                errors.append(f"{f.name}: Invalid ID pattern: {id_value}")

    return errors
```

---

## 7. Element IDs

### 7.1 Universal ID Pattern Rule

> **UNIVERSAL RULE:** ALL element IDs must follow the same hierarchical pattern structure, regardless of whether the element has an XSL-recognized prefix or not.

```
Pattern: {PARENT_SECTION_ID}{prefix}{####}

Where:
  - {PARENT_SECTION_ID} = The ID of the containing section (e.g., ch0005s0000s0002)
  - {prefix} = Element type prefix (2-3 characters)
  - {####} = 4-digit zero-padded sequential number
```

**This pattern applies to ALL elements**, including:
- Elements with XSL-recognized prefixes (fg, ta, eq, etc.)
- Elements with custom/internal prefixes (box, cs, ex, etc.)
- Any other identifiable content elements

#### Examples of Consistent ID Patterns
```
ch0005s0000s0002fg0001    ← Figure (XSL-recognized)
ch0005s0000s0002ta0001    ← Table (XSL-recognized)
ch0005s0000s0002eq0001    ← Equation (XSL-recognized)
ch0005s0000s0002ad0001    ← Sidebar/Admonition (XSL-recognized)
ch0005s0000s0002box0001   ← Box element (custom prefix)
ch0005s0000s0002cs0001    ← Case study (custom prefix)
ch0005s0000s0002ex0001    ← Example (custom prefix)
ch0005s0000s0002pt0001    ← Pro tip (custom prefix)
ch0005s0000s0002li0001    ← List item (custom prefix)
```

#### Why Consistency Matters
- Predictable ID structure across the entire book
- Easier automated validation and processing
- Prevents ID collisions
- Maintains Cardinal Rule compliance
- Simplifies debugging and troubleshooting

### 7.1A Malformed ID Detection and Reconstruction

> **CRITICAL:** Any ID that does not follow the standard pattern MUST be completely reconstructed. Malformed IDs cannot simply be "fixed" - they must be regenerated from scratch based on the element's position in the document hierarchy.

#### Common Malformed ID Patterns (INVALID)

| Malformed Pattern | Example | Problem |
|-------------------|---------|---------|
| Short/abbreviated | `ch01-f-c` | Missing proper structure |
| Hyphen separators | `ch0005-s0002-fg0001` | Wrong separator (should have none) |
| Underscore separators | `ch0005_s0002_fg0001` | Wrong separator |
| Missing section chain | `ch0005fg0001` | No parent section ID |
| Wrong digit padding | `ch5s0s2fg1` | Not zero-padded to 4 digits |
| Mixed formats | `chapter5-figure1` | Human-readable instead of coded |
| Legacy formats | `c5-tbl-001` | Old/incompatible format |
| Random strings | `fig_abc123` | No hierarchical structure |
| Source file IDs | `epub-ch5-img2` | Carried over from ePub |
| UUID-style | `id-a1b2c3d4` | Non-hierarchical |

#### Valid vs Invalid ID Comparison

```
❌ INVALID IDs (must be reconstructed):
   ch01-f-c
   ch5s2fg1
   chapter5_figure1
   tbl-5-1
   fig.5.2.1
   c0005-s0002-f0001
   ch0005-box1
   sidebar-intro
   my-custom-id

✅ VALID IDs (follow the pattern):
   ch0005s0000s0002fg0001
   ch0005s0000s0002ta0001
   ch0005s0000s0002ad0001
   ch0005s0000s0002box0001
   ch0011s0000s0003s0001eq0001
```

#### ID Reconstruction Process

When a malformed ID is detected, follow this process:

1. **Identify the element's location** in the document hierarchy
2. **Determine the parent section ID** (sect1 → sect2 → sect3 → etc.)
3. **Identify the element type** and appropriate prefix
4. **Count existing elements** of the same type in the same section
5. **Generate new ID** following the pattern `{parentSectionID}{prefix}{####}`

```python
def reconstruct_malformed_id(element, parent_section_id, element_type, sequence_num):
    """
    Reconstruct a malformed ID to follow standard pattern.
    
    Args:
        element: The XML element with malformed ID
        parent_section_id: ID of the containing section (e.g., 'ch0005s0000s0002')
        element_type: Type of element ('figure', 'table', 'sidebar', etc.)
        sequence_num: Sequential number of this element type in the section
    
    Returns:
        Properly formatted ID string
    """
    # Map element types to prefixes
    prefix_map = {
        'figure': 'fg',
        'table': 'ta',
        'equation': 'eq',
        'sidebar': 'ad',      # Note: sidebar uses 'ad', not 'sb'
        'note': 'ad',
        'warning': 'ad',
        'tip': 'ad',
        'important': 'ad',
        'caution': 'ad',
        'glossentry': 'gl',
        'bibliomixed': 'bib',
        'qandaentry': 'qa',
        'procedure': 'pr',
        'video': 'vd',
        'box': 'box',
        'example': 'ex',
        'casestudy': 'cs',
    }
    
    prefix = prefix_map.get(element_type, element_type[:3])
    
    # Generate properly formatted ID
    new_id = f"{parent_section_id}{prefix}{sequence_num:04d}"
    
    return new_id

# Example usage:
# Old malformed ID: "ch01-f-c" on a figure in section ch0005s0000s0002
# New correct ID: "ch0005s0000s0002fg0001"
```

#### Malformed ID Detection Script

```python
import re
from pathlib import Path

# Valid ID patterns
VALID_PATTERNS = {
    'chapter_root': r'^ch\d{4}s\d{4}$',
    'section': r'^ch\d{4}s\d{4}(s\d{4})+$',
    'element': r'^ch\d{4}s\d{4}(s\d{4})*[a-z]{2,4}\d{4}$',
    'part': r'^pt\d{4}$',
    'preface': r'^pr\d{4}$',
}

def is_valid_id(id_string):
    """Check if an ID follows valid patterns."""
    for pattern_name, pattern in VALID_PATTERNS.items():
        if re.match(pattern, id_string):
            return True, pattern_name
    return False, None

def find_malformed_ids(work_dir, isbn):
    """Find all malformed IDs in the book."""
    malformed = []
    
    for f in work_dir.glob(f"*.xml"):
        if f.name.startswith("toc."):
            continue
            
        content = f.read_text()
        
        # Find all IDs
        for match in re.finditer(r'id="([^"]+)"', content):
            id_value = match.group(1)
            is_valid, pattern_type = is_valid_id(id_value)
            
            if not is_valid:
                # Get line number
                line_num = content[:match.start()].count('\n') + 1
                malformed.append({
                    'file': f.name,
                    'line': line_num,
                    'id': id_value,
                    'context': content[max(0, match.start()-50):match.end()+50]
                })
    
    return malformed

def report_malformed_ids(work_dir, isbn):
    """Generate report of malformed IDs that need reconstruction."""
    malformed = find_malformed_ids(work_dir, isbn)
    
    if not malformed:
        print("✅ No malformed IDs found!")
        return
    
    print(f"❌ Found {len(malformed)} malformed IDs requiring reconstruction:\n")
    
    for item in malformed:
        print(f"  File: {item['file']}, Line: {item['line']}")
        print(f"  Malformed ID: {item['id']}")
        print(f"  Context: ...{item['context'].strip()}...")
        print()

if __name__ == "__main__":
    import sys
    isbn = sys.argv[1] if len(sys.argv) > 1 else "9781234567890"
    work_dir = Path(sys.argv[2]) if len(sys.argv) > 2 else Path(".")
    report_malformed_ids(work_dir, isbn)
```

#### Updating Linkends After ID Reconstruction

When an ID is reconstructed, ALL references to that ID must also be updated:

```python
def update_linkends_after_reconstruction(work_dir, isbn, old_id, new_id):
    """
    Update all linkends that reference a reconstructed ID.
    """
    updated_files = []
    
    for f in work_dir.glob("*.xml"):
        content = f.read_text()
        
        # Update simple linkends
        new_content = content.replace(f'linkend="{old_id}"', f'linkend="{new_id}"')
        
        # Update cross-file linkends (with # separator)
        # Old format might be: someroot#{old_id}
        new_content = re.sub(
            rf'linkend="([^"#]+)#{re.escape(old_id)}"',
            rf'linkend="\1#{new_id}"',
            new_content
        )
        
        if new_content != content:
            f.write_text(new_content)
            updated_files.append(f.name)
    
    return updated_files
```

### 7.2 XSL-Recognized ID Prefixes

> **CRITICAL:** The R2 Library XSL stylesheets (`link.ritt.xsl`) recognize specific ID prefixes for generating special link behavior (popups, cross-references, etc.). **You MUST use these exact prefixes** for the corresponding elements to ensure proper functionality.

| Prefix | Element Type | DocBook Elements |
|--------|--------------|------------------|
| `fg` | Figure | `<figure>` |
| `eq` | Equation | `<equation>`, `<informalequation>` |
| `ta` | Table | `<table>`, `<informaltable>` |
| `gl` | Glossary | `<glossentry>`, `<glossterm>` |
| `bib` | Bibliography | `<bibliomixed>`, `<biblioentry>` |
| `qa` | Q&A Set | `<qandaset>`, `<qandaentry>` |
| `pr` | Procedure/Preface | `<procedure>`, `<preface>` |
| `vd` | Video | `<videoobject>`, custom video elements |
| `ad` | Admonition | `<note>`, `<sidebar>`, `<important>`, `<warning>`, `<caution>`, `<tip>` |

### 7.3 Custom/Internal ID Prefixes

For elements not in the XSL-recognized list, use descriptive 2-4 character prefixes while maintaining the same pattern structure:

| Prefix | Element Type | Usage |
|--------|--------------|-------|
| `box` | Box/Callout | Highlighted content boxes |
| `cs` | Case Study | Case study sections |
| `ex` | Example | Example blocks |
| `pt` | Pro Tip | Pro tip callouts |
| `fea` | Feature | Feature boxes |
| `sum` | Summary | Summary sections |
| `rev` | Review | Review questions |
| `obj` | Objective | Learning objectives |
| `key` | Key Point | Key point callouts |
| `def` | Definition | Definition blocks |

> **Note:** Custom prefixes won't trigger special XSL link behavior, but maintaining the pattern ensures consistency and prevents ID collisions.

### 7.4 Figure IDs
```
Pattern: {PARENT_SECTION_ID}fg#### (4 digits, zero-padded)
Example: ch0005s0000s0002fg0001

For a figure in sect2 ch0005s0000s0002:
  Figure 1 → ch0005s0000s0002fg0001
  Figure 2 → ch0005s0000s0002fg0002
```

### 7.5 Table IDs
```
Pattern: {PARENT_SECTION_ID}ta#### (4 digits, zero-padded)
Example: ch0005s0000s0005ta0001

For a table in sect2 ch0005s0000s0005:
  Table 1 → ch0005s0000s0005ta0001
  Table 2 → ch0005s0000s0005ta0002
```

### 7.6 Bibliography IDs
```
Pattern: {PARENT_SECTION_ID}bib#### (4 digits, zero-padded)
Example: ch0005s0000s0006bib0001

For bibliography entries in References section ch0005s0000s0006:
  Entry 1 → ch0005s0000s0006bib0001
  Entry 2 → ch0005s0000s0006bib0002
```

### 7.7 Equation IDs
```
Pattern: {PARENT_SECTION_ID}eq#### (4 digits, zero-padded)
Example: ch0005s0000s0003eq0001

For equations in sect2 ch0005s0000s0003:
  Equation 1 → ch0005s0000s0003eq0001
  Equation 2 → ch0005s0000s0003eq0002
```

### 7.8 Glossary IDs
```
Pattern: {PARENT_SECTION_ID}gl#### (4 digits, zero-padded)
Example: ch0005s0000s0007gl0001

For glossary entries:
  Term 1 → ch0005s0000s0007gl0001
  Term 2 → ch0005s0000s0007gl0002
```

### 7.9 Q&A Set IDs
```
Pattern: {PARENT_SECTION_ID}qa#### (4 digits, zero-padded)
Example: ch0005s0000s0004qa0001

For Q&A entries:
  Q&A 1 → ch0005s0000s0004qa0001
  Q&A 2 → ch0005s0000s0004qa0002
```

### 7.10 Procedure IDs
```
Pattern: {PARENT_SECTION_ID}pr#### (4 digits, zero-padded)
Example: ch0005s0000s0002pr0001

For procedures:
  Procedure 1 → ch0005s0000s0002pr0001
  Procedure 2 → ch0005s0000s0002pr0002
```

### 7.11 Video IDs
```
Pattern: {PARENT_SECTION_ID}vd#### (4 digits, zero-padded)
Example: ch0005s0000s0002vd0001

For video elements:
  Video 1 → ch0005s0000s0002vd0001
  Video 2 → ch0005s0000s0002vd0002
```

### 7.12 Admonition IDs (Critical - Includes Sidebars)

> **⚠️ IMPORTANT:** Sidebars MUST use the `ad` prefix, NOT `sb`. The XSL treats sidebars as admonitions along with note, important, warning, caution, and tip.

```
Pattern: {PARENT_SECTION_ID}ad#### (4 digits, zero-padded)
Example: ch0005s0000s0002ad0001

For admonitions (note, sidebar, important, warning, caution, tip):
  Sidebar 1  → ch0005s0000s0002ad0001
  Note 1     → ch0005s0000s0002ad0002
  Warning 1  → ch0005s0000s0002ad0003
  Tip 1      → ch0005s0000s0002ad0004
```

#### Why `ad` for Sidebars (Not `sb`)
The R2 Library XSL (`admon.ritt.xsl`) processes sidebars together with other admonitions:
```xsl
<xsl:template match="note|sidebar|important|warning|caution|tip" mode="anticipated">
```

Using `sb` instead of `ad` will cause:
- Broken popup link behavior
- Cross-references not resolving correctly
- Inconsistent navigation in the R2 Library viewer

### 7.13 Summary Table

| Element | Prefix | Digits | Example ID |
|---------|--------|--------|------------|
| Figure | `fg` | 4 | `ch0005s0000s0002fg0001` |
| Equation | `eq` | 4 | `ch0005s0000s0003eq0001` |
| Table | `ta` | 4 | `ch0005s0000s0005ta0001` |
| Glossary | `gl` | 4 | `ch0005s0000s0007gl0001` |
| Bibliography | `bib` | 4 | `ch0005s0000s0006bib0001` |
| Q&A Set | `qa` | 4 | `ch0005s0000s0004qa0001` |
| Procedure | `pr` | 4 | `ch0005s0000s0002pr0001` |
| Video | `vd` | 4 | `ch0005s0000s0002vd0001` |
| Admonition* | `ad` | 4 | `ch0005s0000s0002ad0001` |

*Admonition includes: `<note>`, `<sidebar>`, `<important>`, `<warning>`, `<caution>`, `<tip>`

### 7.14 Element ID Placement (Critical)

> **RULE:** Element IDs MUST be placed on the actual element, NOT on a `<para>` caption element above it.

#### ❌ WRONG: ID on para caption
```xml
<para id="ch0005s0000s0004ta0001">
   <phrase role="figureLabel">
      <emphasis role="strong">Table 1.1</emphasis>
   </phrase> Table caption text.
</para>
<table>
   <title>Table 1</title>
   ...
</table>
```

#### ✅ CORRECT: ID on the element itself
```xml
<para>
   <phrase role="figureLabel">
      <emphasis role="strong">Table 1.1</emphasis>
   </phrase> Table caption text.
</para>
<table id="ch0005s0000s0004ta0001">
   <title>Table 1.1</title>
   ...
</table>
```

This applies to ALL element types: tables, figures, equations, sidebars, etc.

#### Detection Pattern
```python
# Find para elements with element IDs (wrong)
wrong_patterns = [
    r'<para id="([^"]*ta\d+)">',   # Tables
    r'<para id="([^"]*fg\d+)">',   # Figures
    r'<para id="([^"]*eq\d+)">',   # Equations
    r'<para id="([^"]*ad\d+)">',   # Admonitions
]
```

#### Fix Script Pattern
```python
import re

def fix_element_ids(content, element_tag, prefix):
    """Move element IDs from para caption to the actual element."""
    
    pattern = rf'(<para) id="([^"]*{prefix}\d+)"(>.*?</para>\s*)(<{element_tag}>)'
    
    def replace_func(match):
        para_open = match.group(1)
        element_id = match.group(2)
        para_rest = match.group(3)
        element_tag_str = match.group(4)
        return f'{para_open}{para_rest}<{element_tag} id="{element_id}">'
    
    return re.sub(pattern, replace_func, content, flags=re.DOTALL)

# Usage:
content = fix_element_ids(content, 'table', 'ta')
content = fix_element_ids(content, 'figure', 'fg')
content = fix_element_ids(content, 'sidebar', 'ad')
```

---

## 8. TOC Validation

### 8.1 TOC Structure Elements
```xml
<toc>
  <tocfront linkend="...">Front Matter Item</tocfront>
  <tocpart>
    <tocentry linkend="pt0001">Part I Title</tocentry>
    <tocchap>
      <tocentry linkend="ch0011s0000">Chapter 1 Title</tocentry>
      <toclevel1>
        <tocentry linkend="ch0011s0000s0001">1.1 Section Title</tocentry>
      </toclevel1>
    </tocchap>
  </tocpart>
  <tocback linkend="ch0033s0000">Index</tocback>
</toc>
```

### 8.2 Linkend Validation Rules

1. **Every linkend must point to a valid ID** that exists in some XML file
2. **Part linkends** must use `pt####` format
3. **Chapter linkends** must use `ch####s0000` format
4. **Section linkends** must use the full hierarchical ID

### 8.3 Common TOC Issues

| Issue | Wrong | Correct |
|-------|-------|---------|
| Part linkend using chapter format | `linkend="ch0010s0000"` | `linkend="pt0001"` |
| Subsection linkend wrong prefix | `linkend="ch0004s0006"` | `linkend="ch0005s0000s0006"` |
| Missing s0000 suffix | `linkend="ch0011"` | `linkend="ch0011s0000"` |

### 8.4 Validation Script Pattern
```python
# Collect all valid IDs from all files
all_ids = set()
for file in xml_files:
    ids = extract_ids(file)
    all_ids.update(ids)

# Check all TOC linkends
for linkend in toc_linkends:
    if linkend not in all_ids:
        print(f"Broken linkend: {linkend}")
```

---

## 9. book.xml Structure

### 9.1 Entity Declarations
```xml
<!DOCTYPE book [
  <!ENTITY sect1.{ISBN}.pt0001 SYSTEM "sect1.{ISBN}.pt0001.xml">
  <!ENTITY sect1.{ISBN}.ch0011s0000 SYSTEM "sect1.{ISBN}.ch0011s0000.xml">
  ...
]>
```

### 9.2 Part Structure
```xml
<part id="pt0001">
   <title/> &sect1.{ISBN}.pt0001;
   <chapter id="ch0011s0000">
      <title/> &sect1.{ISBN}.ch0011s0000;
   </chapter>
   <chapter id="ch0012s0000">
      <title/> &sect1.{ISBN}.ch0012s0000;
   </chapter>
</part>
```

### 9.3 Critical Rules

1. **Part ID must match TOC linkend**: `<part id="pt0001">` matches `linkend="pt0001"`
2. **No duplicate IDs**: Part ID and chapter ID must be different
3. **Part content directly in part element**: Don't wrap Part title page as a `<chapter>`
4. **Chapters grouped under correct Part**

### 9.4 Common book.xml Issues

| Issue | Wrong | Correct |
|-------|-------|---------|
| Part ID mismatch | `<part id="ch0010-part">` | `<part id="pt0001">` |
| Part title as chapter | `<part><chapter id="pt0001">` | `<part id="pt0001"><title/>` |
| Duplicate IDs | Part and chapter both `id="ch0010s0000"` | Part=`pt0001`, Chapter=`ch0011s0000` |

---

## 10. Validation Checklist

Use this checklist when processing any book:

### Phase 1: File Inventory
- [ ] Count total XML files
- [ ] Verify book.xml exists
- [ ] Verify toc.xml exists
- [ ] List all preface files
- [ ] List all part files (should use pt#### format)
- [ ] List all chapter files

### Phase 2: Filename ↔ Root ID Match
- [ ] Every preface filename matches its root ID
- [ ] Every part filename matches its root ID
- [ ] Every chapter filename matches its root ID

### Phase 3: Part Files
- [ ] All Part files use pt#### naming
- [ ] Part root IDs are pt0001, pt0002, etc.
- [ ] Part content is just title + empty para

### Phase 4: Cardinal Rule (Internal IDs)
- [ ] All sect2 IDs prefixed with file root ID
- [ ] All sect3 IDs prefixed with parent sect2 ID
- [ ] All figure IDs use `fg` prefix with correct pattern
- [ ] All table IDs use `ta` prefix with correct pattern
- [ ] All equation IDs use `eq` prefix with correct pattern
- [ ] All glossary IDs use `gl` prefix with correct pattern
- [ ] All bibliography IDs use `bib` prefix with correct pattern
- [ ] All Q&A IDs use `qa` prefix with correct pattern
- [ ] All procedure IDs use `pr` prefix with correct pattern
- [ ] All video IDs use `vd` prefix with correct pattern
- [ ] All admonition IDs (including sidebars) use `ad` prefix with correct pattern
- [ ] All custom elements (boxes, case studies, etc.) follow same `{section}{prefix}{####}` pattern
- [ ] **NO malformed IDs** (check for hyphens, underscores, wrong padding, missing sections)

### Phase 5: Element ID Placement
- [ ] All table IDs are on `<table>` elements (not on `<para>` captions)
- [ ] All figure IDs are on `<figure>` elements (not on `<para>` captions)
- [ ] All sidebar IDs are on `<sidebar>` elements with `ad` prefix
- [ ] No duplicate IDs between caption para and element

### Phase 6: TOC Validation
- [ ] All linkends point to valid IDs
- [ ] Part linkends use pt#### format
- [ ] No duplicate/orphan entries
- [ ] Structure matches actual book content

### Phase 6A: Cross-File Linkend Validation (XSL Handles Automatically)
- [ ] All IDs follow correct patterns (first 11 chars = file identifier for chapters)
- [ ] No malformed IDs that would confuse XSL file extraction
- [ ] All linkends point to valid IDs (XSL handles URL generation)

> **Note:** RIS Navigation (risprev, riscurrent, risnext) is generated by downstream XSL (`RISChunker.xsl`) - not by this converter.

### Phase 7: book.xml Structure
- [ ] Entity declarations match actual files
- [ ] Part IDs match TOC linkends
- [ ] Chapters grouped under correct Parts
- [ ] No duplicate IDs

### Phase 8: Content Integrity
- [ ] No empty tables (check for `<para/>` in table entries)
- [ ] All bibliography links resolve to valid entries
- [ ] All internal cross-references resolve
- [ ] All figures have proper content
- [ ] All sidebars use `ad` prefix (not `sb`)
- [ ] All custom element IDs follow universal pattern `{section}{prefix}{####}`

---

## 11. Common Issues & Fixes

### Issue 1: Part files using ch#### format
**Symptom:** Clicking Part in TOC shows first chapter instead of Part title page
**Fix:** Rename Part files to pt#### format, update all references

### Issue 2: Internal IDs have wrong prefix
**Symptom:** Broken cross-references, "link not found" errors
**Fix:** Rebuild all internal IDs based on file root ID

### Issue 3: TOC linkends point to non-existent IDs
**Symptom:** TOC links don't work
**Fix:** Validate all linkends against actual IDs, correct mismatches

### Issue 4: book.xml has duplicate Part/Chapter IDs
**Symptom:** XML validation errors, navigation issues
**Fix:** Ensure Part IDs use pt#### format, Chapter IDs use ch####s0000

### Issue 5: Element IDs use inconsistent format
**Symptom:** Some IDs use 2-digit format, others use 4-digit
**Fix:** Standardize all numeric suffixes to 4-digit format (s####, fg####, ta####, bib####, etc.)

### Issue 6: Element ID on para caption instead of actual element
**Symptom:** Cross-references don't navigate correctly; links broken in R2 Library viewer
**Detection:** 
```bash
# Find wrong pattern: para with element ID
grep -n '<para id="[^"]*\(ta\|fg\|eq\|ad\)[0-9]*">' *.xml
```
**Fix:** Move the ID from the `<para>` caption element to the actual element (table, figure, etc.)

### Issue 7: Empty tables (list content not extracted)
**Symptom:** Tables appear empty with only `<para/>` in entries
**Root Cause:** Converter fails when source `<td>` contains ONLY a `<ul>` or `<ol>` list with no surrounding text
**Fix:** Extract list content from source XHTML/ePub and convert to DocBook

### Issue 8: Citation links not resolving
**Symptom:** `[1]`, `[2]` etc. in text don't link to bibliography
**Fix:** Convert citation references to proper link elements:
```xml
<!-- Wrong -->
<citation>1</citation>

<!-- Correct -->
<link linkend="ch0005s0000s0006bib0001">[1]</link>
```

### Issue 9: Duplicate IDs across files
**Symptom:** XML validation errors, unpredictable navigation
**Fix:** Ensure Cardinal Rule is followed - all IDs must be prefixed with file root ID

### Issue 10: Sidebar using wrong ID prefix (`sb` instead of `ad`)
**Symptom:** Sidebar links don't work; popup behavior broken in R2 Library viewer
**Root Cause:** The XSL stylesheets expect `ad` prefix for all admonitions including sidebars
**Detection:**
```bash
# Find sidebars with wrong prefix
grep -n 'id="[^"]*sb[0-9]*"' *.xml
```
**Fix:** Change `sb` prefix to `ad` for all sidebar IDs:
```python
import re

def fix_sidebar_ids(content):
    """Change sidebar IDs from sb#### to ad####."""
    # Fix sidebar element IDs
    content = re.sub(
        r'(<sidebar[^>]*id="[^"]*?)sb(\d+)',
        r'\1ad\2',
        content
    )
    # Fix linkends pointing to sidebars
    content = re.sub(
        r'(linkend="[^"]*?)sb(\d+)',
        r'\1ad\2',
        content
    )
    return content
```

### Issue 11: Non-standard element prefix used
**Symptom:** Links to element don't trigger special popup/navigation behavior
**Detection:** Check all IDs match the XSL-recognized prefixes
**Fix:** Use only these prefixes for XSL elements: `fg`, `eq`, `ta`, `gl`, `bib`, `qa`, `pr`, `vd`, `ad`

### Issue 12: Inconsistent ID pattern for custom elements
**Symptom:** Some IDs don't follow the hierarchical pattern; validation scripts miss them
**Root Cause:** Custom elements (boxes, case studies, etc.) using non-standard ID formats
**Detection:**
```python
import re

# Pattern: should be {section_id}{prefix}{4_digits}
valid_pattern = r'^ch\d{4}s\d{4}(s\d{4})*[a-z]{2,4}\d{4}$'

for id in all_ids:
    if not re.match(valid_pattern, id):
        print(f"Non-standard ID: {id}")
```
**Fix:** Ensure ALL element IDs follow the pattern `{PARENT_SECTION_ID}{prefix}{####}`:
```xml
<!-- Wrong -->
<sidebar id="sidebar-1">
<sidebar id="ch0005-box1">
<sidebar id="mybox">

<!-- Correct -->
<sidebar id="ch0005s0000s0002box0001">
```

### Issue 13: Cross-file linkend missing `#` separator
**Symptom:** Links to elements in other chapters don't work; navigation fails
**Root Cause:** Cross-file links using simple ID instead of `{fileRoot}#{targetID}` format
**Detection:**
```bash
# Find linkends that reference IDs from other files without #
grep -n 'linkend="ch[0-9]*s[0-9]*s' *.xml | grep -v '#'
```
**Fix:** Add file root and `#` separator for cross-file links:
```xml
<!-- Wrong: linking from ch0005 to element in ch0007 -->
<link linkend="ch0007s0000s0002fg0001">Figure 7.1</link>

<!-- Correct -->
<link linkend="ch0007s0000#ch0007s0000s0002fg0001">Figure 7.1</link>
```

### Issue 14: Mismatched file root in cross-file linkend
**Symptom:** Link navigates to wrong file or fails entirely
**Root Cause:** File root before `#` doesn't match the file containing the target element
**Example:**
```xml
<!-- Wrong: file root says ch0005 but target is in ch0007 -->
<link linkend="ch0005s0000#ch0007s0000s0002">Section 7.2</link>

<!-- Correct -->
<link linkend="ch0007s0000#ch0007s0000s0002">Section 7.2</link>
```
**Fix:** Ensure the file root before `#` matches the first 11 characters of the target ID (for chapter files)

### Issue 15: Malformed IDs requiring complete reconstruction
**Symptom:** IDs don't match any valid pattern; validation fails; links broken throughout
**Root Cause:** Source ePub used non-standard ID formats, or converter produced malformed IDs
**Common Malformed Patterns:**
```
ch01-f-c           ← Short/abbreviated
ch0005-s0002-fg1   ← Hyphen separators
chapter5_figure1   ← Human-readable format
tbl-5-1            ← Legacy format
fig_abc123         ← Random strings
```
**Detection:**
```python
import re

# Valid patterns
valid_patterns = [
    r'^ch\d{4}s\d{4}$',                    # Chapter root
    r'^ch\d{4}s\d{4}(s\d{4})+$',           # Section
    r'^ch\d{4}s\d{4}(s\d{4})*[a-z]{2,4}\d{4}$',  # Element
    r'^pt\d{4}$',                          # Part
    r'^pr\d{4}$',                          # Preface
]

def is_malformed(id_string):
    return not any(re.match(p, id_string) for p in valid_patterns)

# Find all malformed IDs
for id in all_ids:
    if is_malformed(id):
        print(f"MALFORMED: {id}")
```
**Fix:** Malformed IDs CANNOT be patched - they must be completely reconstructed:
1. Identify element's position in document hierarchy
2. Determine parent section ID
3. Identify element type and correct prefix
4. Generate new ID: `{parentSectionID}{prefix}{####}`
5. Update ALL linkends referencing the old ID

```python
# Example reconstruction
old_id = "ch01-f-c"  # Malformed
# Element is a figure in ch0005s0000s0002, first figure in section
new_id = "ch0005s0000s0002fg0001"  # Reconstructed

# Update all references
content = content.replace(f'id="{old_id}"', f'id="{new_id}"')
content = content.replace(f'linkend="{old_id}"', f'linkend="{new_id}"')
```

---

## Appendix A: Quick Reference

### Universal ID Pattern (MUST FOLLOW)
```
Pattern: {PARENT_SECTION_ID}{prefix}{####}

ALL IDs must follow this pattern - both XSL-recognized and custom prefixes!
```

### Valid vs Malformed ID Examples
```
✅ VALID IDs:
   ch0005s0000              (chapter root)
   ch0005s0000s0001         (sect2)
   ch0005s0000s0001s0001    (sect3)
   ch0005s0000s0002fg0001   (figure)
   ch0005s0000s0002ta0001   (table)
   ch0005s0000s0002ad0001   (sidebar)
   ch0005s0000s0002box0001  (custom box)
   pt0001                   (part)
   pr0001                   (preface)

❌ MALFORMED IDs (must reconstruct):
   ch01-f-c                 (abbreviated, hyphens)
   ch5s2fg1                 (wrong padding)
   chapter5_figure1         (human-readable)
   ch0005-s0002-fg0001      (hyphen separators)
   tbl-5-1                  (legacy format)
   fig_abc123               (random string)
   sidebar-intro            (no hierarchy)
```

### XSL-Recognized ID Prefixes (MUST USE FOR THESE ELEMENTS)
```
fg   → figure
eq   → equation
ta   → table
gl   → glossary
bib  → bibliography
qa   → qandaset
pr   → procedure/preface
vd   → video
ad   → admonition (note, sidebar, important, warning, caution, tip)
```

### Custom/Internal Prefixes (Use Same Pattern)
```
box  → box/callout
cs   → case study
ex   → example
pt   → pro tip
fea  → feature box
sum  → summary
rev  → review questions
obj  → learning objective
key  → key point
def  → definition
```

### ID Format Cheat Sheet
```
Part:           pt#### (pt0001)
Chapter root:   ch####s#### (ch0011s0000)
sect2:          {root}s#### (ch0011s0000s0001)
sect3:          {sect2}s## (ch0011s0000s0001s01)   ← 2-digit for 25-char limit
sect4:          {sect3}s## (ch0011s0000s0001s01s01)
sect5:          {sect4}s## (ch0011s0000s0001s01s01s01)

XSL-Recognized Elements:
  Figure:       {section}fg#### (ch0011s0000s0001fg0001)
  Equation:     {section}eq#### (ch0011s0000s0001eq0001)
  Table:        {section}ta#### (ch0011s0000s0001ta0001)
  Glossary:     {section}gl#### (ch0011s0000s0007gl0001)
  Bibliography: {section}bib#### (ch0011s0000s0006bib0001)
  Q&A:          {section}qa#### (ch0011s0000s0004qa0001)
  Procedure:    {section}pr#### (ch0011s0000s0002pr0001)
  Video:        {section}vd#### (ch0011s0000s0002vd0001)
  Admonition:   {section}ad#### (ch0011s0000s0002ad0001)

Custom Elements (same pattern):
  Box:          {section}box#### (ch0011s0000s0002box0001)
  Case Study:   {section}cs#### (ch0011s0000s0002cs0001)
  Example:      {section}ex#### (ch0011s0000s0002ex0001)
  Pro Tip:      {section}pt#### (ch0011s0000s0002pt0001)
```

### Element ID Placement
```
✅ CORRECT: <table id="...ta0001">
❌ WRONG:   <para id="...ta0001">...<table>

✅ CORRECT: <figure id="...fg0001">
❌ WRONG:   <para id="...fg0001">...<figure>

✅ CORRECT: <sidebar id="...ad0001">
❌ WRONG:   <sidebar id="...sb0001">
```

### Linkend Format (XSL Handles Cross-File URLs Automatically)
```
All links use simple format:
  linkend="{targetID}"
  Example: linkend="ch0007s0000s0002fg0001"

XSL automatically generates URL:
  ch0007s0000#goto=ch0007s0000s0002fg0001
  ├───────────┤     ├──────────────────────┤
  First 11 chars    Full linkend
  (file identifier)
```

> **Note:** You do NOT need to put `#` in linkend attributes. The XSL extracts
> the first 11 characters as the file identifier and generates the proper URL.

### File Naming Cheat Sheet
```
Book:     book.{ISBN}.xml
TOC:      toc.{ISBN}.xml
Preface:  preface.{ISBN}.pr####.xml
Part:     sect1.{ISBN}.pt####.xml
Chapter:  sect1.{ISBN}.ch####s0000.xml
```

> **Note:** RIS Navigation (risprev, riscurrent, risnext) is generated by
> downstream XSL (`RISChunker.xsl`) - not by this converter.

### Source-to-DocBook Element Mapping
| Source (XHTML/ePub) | DocBook XML |
|---------------------|-------------|
| `<ul>` | `<itemizedlist>` |
| `<ol>` | `<orderedlist>` |
| `<li>` | `<listitem><para>...</para></listitem>` |
| `<table>` | `<table>` with `<tgroup>`, `<tbody>`, `<row>`, `<entry>` |
| `<figure>` | `<figure>` with `<mediaobject>` |
| `<aside>`, `<div class="sidebar">` | `<sidebar>` (use `ad` prefix!) |
| `<b>`, `<strong>` | `<emphasis role="strong">` |
| `<i>`, `<em>` | `<emphasis>` |
| `<a href="#id">` | `<link linkend="id">` |
| `<sup>` | `<superscript>` |
| `<sub>` | `<subscript>` |

---

## Appendix B: Validation Script Template

```python
#!/usr/bin/env python3
"""
DocBook XML Validation Script Template
Validates any ePub-to-DocBook conversion for R2 Library
"""

import re
from pathlib import Path

# XSL-recognized element prefixes
VALID_PREFIXES = {
    'fg': 'figure',
    'eq': 'equation', 
    'ta': 'table',
    'gl': 'glossary',
    'bib': 'bibliography',
    'qa': 'qandaset',
    'pr': 'procedure',
    'vd': 'video',
    'ad': 'admonition',
}

# Invalid prefixes that should be replaced
INVALID_PREFIXES = {
    'sb': 'ad',  # sidebar should use 'ad' not 'sb'
}

def validate_book(work_dir, isbn):
    """
    Validate a DocBook XML book package.
    
    Args:
        work_dir: Path to directory containing XML files
        isbn: The book's ISBN (used in filenames)
    
    Returns:
        List of error messages
    """
    errors = []
    
    # 1. File inventory
    files = list(work_dir.glob("*.xml"))
    
    # 2. Collect all valid IDs
    all_ids = set()
    for f in files:
        if f.name.startswith("toc."):
            continue
        content = f.read_text()
        ids = re.findall(r'id="([^"]+)"', content)
        all_ids.update(ids)
    
    # 3. Validate TOC linkends
    toc_file = work_dir / f"toc.{isbn}.xml"
    if toc_file.exists():
        toc = toc_file.read_text()
        linkends = re.findall(r'linkend="([^"]+)"', toc)
        for lid in linkends:
            if lid not in all_ids:
                errors.append(f"Broken TOC linkend: {lid}")
    else:
        errors.append(f"Missing TOC file: toc.{isbn}.xml")
    
    # 4. Validate filename ↔ root ID
    for f in files:
        if f.name.startswith("sect1."):
            file_id = f.stem.split('.')[-1]
            content = f.read_text()
            root_id = re.search(r'<sect1[^>]*id="([^"]+)"', content)
            if root_id and root_id.group(1) != file_id:
                errors.append(f"Mismatch: {f.name} has root ID {root_id.group(1)}")
    
    # 5. Validate Cardinal Rule
    for f in work_dir.glob(f"sect1.{isbn}.ch*.xml"):
        file_id = f.stem.split('.')[-1]
        content = f.read_text()
        
        for tag in ['sect2', 'sect3', 'figure', 'table', 'sidebar', 'equation', 'bibliomixed']:
            ids = re.findall(rf'<{tag}[^>]*id="([^"]+)"', content)
            for sid in ids:
                if not sid.startswith(file_id):
                    errors.append(f"Cardinal Rule: {f.name} has {tag} id={sid}")
    
    # 6. Validate Element ID Placement (IDs should not be on para captions)
    for f in work_dir.glob(f"sect1.{isbn}.ch*.xml"):
        content = f.read_text()
        
        for prefix in VALID_PREFIXES.keys():
            wrong_ids = re.findall(rf'<para id="([^"]*{prefix}\d+)">', content)
            for tid in wrong_ids:
                errors.append(f"Element ID on para: {f.name} has para id={tid}")
    
    # 7. Check for invalid prefixes (e.g., sb instead of ad)
    for f in work_dir.glob(f"sect1.{isbn}.ch*.xml"):
        content = f.read_text()
        
        for invalid, correct in INVALID_PREFIXES.items():
            wrong_ids = re.findall(rf'id="[^"]*{invalid}\d+"', content)
            for tid in wrong_ids:
                errors.append(f"Invalid prefix '{invalid}' (should be '{correct}'): {f.name} - {tid}")
    
    # 8. Check for empty tables
    for f in work_dir.glob(f"sect1.{isbn}.ch*.xml"):
        content = f.read_text()
        
        empty_tables = re.findall(
            r'<table[^>]*>\s*<title>([^<]+)</title>.*?<entry>\s*<para/>\s*</entry>',
            content, re.DOTALL
        )
        for title in empty_tables:
            errors.append(f"Empty table: {f.name} - {title}")
    
    # 9. Check for duplicate IDs
    all_ids_list = []
    for f in files:
        if f.name.startswith("toc."):
            continue
        content = f.read_text()
        ids = re.findall(r'id="([^"]+)"', content)
        for id in ids:
            all_ids_list.append((id, f.name))
    
    seen = {}
    for id, filename in all_ids_list:
        if id in seen:
            errors.append(f"Duplicate ID: {id} in {seen[id]} and {filename}")
        else:
            seen[id] = filename
    
    return errors

def fix_element_ids(content, element_tag, prefix):
    """Move element IDs from para caption to the actual element."""
    pattern = rf'(<para) id="([^"]*{prefix}\d+)"(>.*?</para>\s*)(<{element_tag}>)'
    
    def replace_func(match):
        return f'{match.group(1)}{match.group(3)}<{element_tag} id="{match.group(2)}">'
    
    return re.sub(pattern, replace_func, content, flags=re.DOTALL)

def fix_sidebar_prefix(content):
    """Change sidebar IDs from sb#### to ad####."""
    # Fix sidebar element IDs
    content = re.sub(r'(<sidebar[^>]*id="[^"]*?)sb(\d+)', r'\1ad\2', content)
    # Fix linkends pointing to sidebars  
    content = re.sub(r'(linkend="[^"]*?)sb(\d+)', r'\1ad\2', content)
    return content

def fix_all_issues(work_dir, isbn):
    """Fix all common issues in chapter files."""
    fixed_count = 0
    
    for f in work_dir.glob(f"sect1.{isbn}.ch*.xml"):
        content = f.read_text()
        original = content
        
        # Fix table IDs
        content = fix_element_ids(content, 'table', 'ta')
        # Fix figure IDs
        content = fix_element_ids(content, 'figure', 'fg')
        # Fix equation IDs
        content = fix_element_ids(content, 'equation', 'eq')
        # Fix sidebar IDs (both placement and prefix)
        content = fix_element_ids(content, 'sidebar', 'ad')
        content = fix_sidebar_prefix(content)
        
        if content != original:
            f.write_text(content)
            fixed_count += 1
            print(f"Fixed: {f.name}")
    
    return fixed_count

if __name__ == "__main__":
    import sys
    
    if len(sys.argv) < 2:
        print("Usage: python validate.py <ISBN> [directory] [--fix]")
        print("Example: python validate.py 9781234567890 ./xml_files")
        print("         python validate.py 9781234567890 ./xml_files --fix")
        sys.exit(1)
    
    isbn = sys.argv[1]
    work_dir = Path(sys.argv[2]) if len(sys.argv) > 2 and not sys.argv[2].startswith('--') else Path(".")
    do_fix = '--fix' in sys.argv
    
    print(f"Validating book {isbn} in {work_dir}")
    print("=" * 60)
    
    errors = validate_book(work_dir, isbn)
    
    if errors:
        print(f"\n❌ Found {len(errors)} errors:\n")
        for e in errors:
            print(f"  - {e}")
        
        if do_fix:
            print("\n" + "=" * 60)
            print("Attempting automatic fixes...")
            fixed = fix_all_issues(work_dir, isbn)
            print(f"Fixed {fixed} files. Re-run validation to check.")
    else:
        print("\n✅ All validations passed!")
```

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-20 | Initial version |
| 1.1 | 2026-01-21 | Updated section IDs to use 4-digit format (s####) instead of 2-digit (s##) |
| 1.3 | 2026-01-21 | Updated element suffixes (fg, ta, bib) to 4-digit format; Added cross-file linkend convention |
| 1.4 | 2026-01-21 | Clarified linkend convention applies to ALL files; Added TOC/book.xml examples |
| 1.5 | 2026-01-21 | Verified all numeric suffixes use 4-digit format; Added ulink URL transformation rule |
| 1.6 | 2026-01-21 | Added Table ID Placement rule; Added Issues 7 & 8; Updated validation checklist |
| 1.7 | 2026-01-21 | Generalized for any ePub conversion with `{ISBN}` placeholder; Added source-to-DocBook mapping |
| 1.8 | 2026-01-21 | Added all XSL-recognized ID prefixes (Section 7.2): `fg`, `eq`, `ta`, `gl`, `bib`, `qa`, `pr`, `vd`, `ad`; Critical: Sidebars must use `ad` prefix (not `sb`); Added Issues 11 & 12; Enhanced validation script |
| 1.9 | 2026-01-21 | Added Universal ID Pattern Rule (Section 7.1); Added Section 7.3 for custom/internal prefixes; Added Issue 13 |
| 2.0 | 2026-01-21 | Added Cross-File Linkend Convention (Section 6A); Added sect3/sect4 ID patterns; Added Issues 14 & 15 |
| 2.1 | 2026-01-21 | **Added Malformed ID Detection & Reconstruction (Section 7.1A)**: Complete rules for identifying invalid ID patterns (hyphens, underscores, wrong padding, legacy formats) and reconstructing them; Added detection scripts and linkend update process; Added Issue 16; Updated checklist and Quick Reference with valid vs malformed examples |
| 2.2 | 2026-01-21 | **XSL Analysis Update**: Clarified that downstream XSL (`link.ritt.xsl`) automatically handles cross-file URL generation by extracting first 11 chars as file identifier; Removed RIS Navigation section (generated by `RISChunker.xsl`); Updated Section 6A to explain XSL processing; Renumbered sections and issues |

---

*This ruleset should be updated as new edge cases are discovered during processing of additional books.*
