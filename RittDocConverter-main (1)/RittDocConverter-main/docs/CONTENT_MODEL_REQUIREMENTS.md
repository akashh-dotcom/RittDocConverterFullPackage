# RittDoc DTD Content Model Requirements

## Overview

This document defines the exact content model requirements for XML elements processed by LoadBook.bat and R2Utilities `splitContentFiles`. Following these specifications ensures XML files pass validation and chunking without errors.

**Critical Process:** `Main.splitContentFiles` splits Book.XML into individual files:
- `sect1.{isbn}.{sect1_id}.xml` - For each sect1 inside chapters
- `preface.{isbn}.{id}.xml` - For each preface element
- `appendix.{isbn}.{id}.xml` - For each appendix element
- `dedication.{isbn}.{id}.xml` - For each dedication element
- `toc.{isbn}.xml` - For table of contents

---

## Quick Reference Table

| Element | Title Required | Sect1 Allowed | Content Type | @id Required |
|---------|----------------|---------------|--------------|--------------|
| **chapter** | Yes | Yes | bookcomponent.content | Yes |
| **preface** | Yes | Yes | bookcomponent.content | Recommended |
| **appendix** | Yes | Yes | bookcomponent.content | Recommended |
| **dedication** | No | **NO** | legalnotice.mix ONLY | Recommended |
| **toc** | No | **NO** | toc* elements ONLY | No |
| **sect1** | Yes | N/A | divcomponent.mix + sect2* | **Yes (REQUIRED)** |

---

## 1. CHAPTER / PREFACE / APPENDIX Content Model

These three elements share the same content model (`bookcomponent.content`).

### 1.1 Element Structure

```
chapter/preface/appendix
├── beginpage?              (optional, MUST be first if present)
├── chapterinfo/prefaceinfo/appendixinfo?   (optional metadata)
├── title                   (REQUIRED - must have text content)
├── subtitle?               (optional)
├── titleabbrev?            (optional)
├── (toc|lot|index|glossary|bibliography)*  (nav.class - before content)
├── tocchap?                (optional)
├── CONTENT: bookcomponent.content (see below)
└── (toc|lot|index|glossary|bibliography)*  (nav.class - after content)
```

### 1.2 bookcomponent.content - CRITICAL RULE

The content model is defined as:

```
bookcomponent.content =
    (divcomponent.mix+, sect1*) |    Pattern A: Block content THEN sections
    (sect1+)                          Pattern B: Sections only (no loose blocks)
```

**CRITICAL:** Once a `sect1` element appears, NO block content can appear after it at the same level.

#### Valid Patterns:

```xml
<!-- Pattern A: Block content first, then sections -->
<preface id="pr0001">
    <title>Preface</title>
    <para>Introduction paragraph...</para>      <!-- Block content BEFORE sect1 -->
    <figure id="pr0001f0001">...</figure>       <!-- Block content BEFORE sect1 -->
    <sect1 id="pr0001s0001">
        <title>Section 1</title>
        <para>Content...</para>
    </sect1>
    <sect1 id="pr0001s0002">
        <title>Section 2</title>
        <para>Content...</para>
    </sect1>
</preface>

<!-- Pattern B: Sections only (no loose block content) -->
<appendix id="ap0001">
    <title>Appendix A</title>
    <sect1 id="ap0001s0001">
        <title>First Section</title>
        <para>All content inside sect1...</para>
    </sect1>
</appendix>
```

#### INVALID Patterns (cause splitContentFiles failure):

```xml
<!-- INVALID: Block content AFTER sect1 -->
<preface id="pr0001">
    <title>Preface</title>
    <sect1 id="pr0001s0001">
        <title>Section 1</title>
        <para>Content...</para>
    </sect1>
    <para>Orphan paragraph after sect1</para>   <!-- INVALID! -->
    <figure id="pr0001f0001">...</figure>       <!-- INVALID! -->
</preface>

<!-- INVALID: Block content BETWEEN sect1 elements -->
<chapter id="ch0001">
    <title>Chapter 1</title>
    <sect1 id="ch0001s0001">...</sect1>
    <para>Content between sections</para>       <!-- INVALID! -->
    <sect1 id="ch0001s0002">...</sect1>
</chapter>
```

### 1.3 divcomponent.mix (Allowed Block Elements)

```python
DIVCOMPONENT_MIX = {
    # Paragraphs
    'para', 'simpara', 'formalpara',

    # Code/Literal
    'programlisting', 'literallayout', 'screen', 'synopsis', 'address',

    # Quotes
    'blockquote', 'epigraph',

    # Figures/Tables
    'figure', 'informalfigure', 'table', 'informaltable',

    # Examples/Equations
    'example', 'informalexample', 'equation', 'informalequation',

    # Lists
    'itemizedlist', 'orderedlist', 'variablelist', 'simplelist',
    'segmentedlist', 'calloutlist', 'glosslist',

    # Admonitions
    'note', 'warning', 'caution', 'important', 'tip',

    # Other blocks
    'procedure', 'sidebar', 'mediaobject', 'graphic',
    'bridgehead', 'remark', 'highlights', 'abstract',

    # Markers
    'anchor', 'indexterm', 'beginpage',

    # Technical
    'cmdsynopsis', 'funcsynopsis', 'classsynopsis',
    'revhistory', 'task', 'productionset', 'constraintdef',
    'msgset', 'qandaset',
}
```

### 1.4 Element Order Requirements

| Position | Element | Required | Notes |
|----------|---------|----------|-------|
| 1 | beginpage | No | MUST be first if present |
| 2 | *info element | No | chapterinfo/prefaceinfo/appendixinfo |
| 3 | title | **Yes** | Must have text content |
| 4 | subtitle | No | After title |
| 5 | titleabbrev | No | After subtitle |
| 6 | nav.class | No | toc, lot, index, glossary, bibliography |
| 7 | tocchap | No | Optional TOC wrapper |
| 8 | content | **Yes** | divcomponent.mix and/or sect1 |
| 9 | nav.class | No | After content |

---

## 2. DEDICATION Content Model

Dedication has a **restricted content model** - only `legalnotice.mix` elements are allowed.

### 2.1 Element Structure

```
dedication
├── risinfo?                (RIT-specific, optional, MUST be first)
├── title?                  (optional - unlike chapter/preface/appendix)
├── subtitle?               (optional)
├── titleabbrev?            (optional)
└── (legalnotice.mix)+      (REQUIRED - at least one element)
```

### 2.2 legalnotice.mix (VERY LIMITED - Dedication Content)

```python
LEGALNOTICE_MIX = {
    # Lists (NO variablelist, simplelist, segmentedlist, calloutlist)
    'glosslist', 'itemizedlist', 'orderedlist',

    # Admonitions
    'caution', 'important', 'note', 'tip', 'warning',

    # Line-specific
    'literallayout', 'programlisting', 'screen', 'synopsis', 'address',

    # Paragraphs
    'formalpara', 'para', 'simpara',

    # Other
    'blockquote',
    'indexterm',
    'beginpage',
}
```

### 2.3 Elements NOT Allowed in Dedication

| Element | Status | Workaround |
|---------|--------|------------|
| `anchor` | **NOT ALLOWED** | Wrap in `<para><anchor id="..."/></para>` |
| `sect1` | **NOT ALLOWED** | Dedication cannot have sections |
| `figure` | **NOT ALLOWED** | Cannot include figures |
| `informalfigure` | **NOT ALLOWED** | Cannot include figures |
| `table` | **NOT ALLOWED** | Cannot include tables |
| `informaltable` | **NOT ALLOWED** | Cannot include tables |
| `sidebar` | **NOT ALLOWED** | Cannot include sidebars |
| `mediaobject` | **NOT ALLOWED** | Wrap in para if needed |
| `graphic` | **NOT ALLOWED** | Wrap in para if needed |
| `example` | **NOT ALLOWED** | Use para instead |
| `procedure` | **NOT ALLOWED** | Use orderedlist instead |
| `bridgehead` | **NOT ALLOWED** | Use para with emphasis |
| `highlights` | **NOT ALLOWED** | Use para instead |
| `abstract` | **NOT ALLOWED** | Use para/blockquote |
| `qandaset` | **NOT ALLOWED** | Not for dedication |
| `variablelist` | **NOT ALLOWED** | Use itemizedlist/orderedlist |
| `simplelist` | **NOT ALLOWED** | Use itemizedlist |
| `segmentedlist` | **NOT ALLOWED** | Use itemizedlist |
| `calloutlist` | **NOT ALLOWED** | Use orderedlist |

### 2.4 Valid Dedication Examples

```xml
<!-- Valid: Simple dedication -->
<dedication id="dd0001">
    <title>Dedication</title>
    <para>To my family, who supported me throughout this journey.</para>
</dedication>

<!-- Valid: Dedication with anchor (wrapped in para) -->
<dedication id="dd0001">
    <title>Dedication</title>
    <para>
        <anchor id="dd0001a0001"/>
        To everyone who made this possible.
    </para>
    <blockquote>
        <para>"The only way to do great work is to love what you do."</para>
    </blockquote>
</dedication>

<!-- Valid: Dedication with list -->
<dedication id="dd0001">
    <para>Special thanks to:</para>
    <itemizedlist>
        <listitem><para>The development team</para></listitem>
        <listitem><para>Our beta testers</para></listitem>
    </itemizedlist>
</dedication>
```

### 2.5 INVALID Dedication Examples

```xml
<!-- INVALID: anchor as direct child -->
<dedication id="dd0001">
    <title>Dedication</title>
    <anchor id="dd0001a0001"/>              <!-- INVALID! Must wrap in para -->
    <para>To my family...</para>
</dedication>

<!-- INVALID: figure not allowed -->
<dedication id="dd0001">
    <title>Dedication</title>
    <figure id="dd0001f0001">               <!-- INVALID! Not in legalnotice.mix -->
        <mediaobject>...</mediaobject>
    </figure>
</dedication>

<!-- INVALID: sect1 not allowed -->
<dedication id="dd0001">
    <title>Dedication</title>
    <sect1 id="dd0001s0001">                <!-- INVALID! No sections in dedication -->
        <title>Section</title>
        <para>Content...</para>
    </sect1>
</dedication>
```

---

## 3. TOC (Table of Contents) Content Model

TOC uses a completely different element hierarchy - NOT structural elements.

### 3.1 Element Structure

```
toc
├── beginpage?              (optional)
├── title?                  (optional)
├── tocfront*               (front matter entries - dedication, preface, etc.)
├── (tocpart | tocchap)*    (main content)
│   ├── tocpart → (tocentry+, tocchap*)
│   └── tocchap → (tocentry+, toclevel1*)
│       └── toclevel1 → (tocentry+, toclevel2*)
│           └── toclevel2 → (tocentry+, toclevel3*)
│               └── ... up to toclevel5
└── tocback*                (back matter entries - appendix, glossary, etc.)
```

### 3.2 TOC Element Definitions

| Element | Content | Attributes |
|---------|---------|------------|
| `tocentry` | Character content (para.char.mix) | `linkend` (points to section @id) |
| `tocfront` | Character content | `linkend`, `pagenum` |
| `tocback` | Character content | `linkend`, `pagenum` |
| `tocpart` | tocentry+, tocchap* | role |
| `tocchap` | tocentry+, toclevel1* | - |
| `toclevel1-5` | tocentry+, toclevel[N+1]* | - |

### 3.3 Elements NOT Allowed in TOC

- `sect1`, `sect2`, etc. - NO structural sections
- `para`, `simpara` - NO paragraph elements
- `figure`, `table` - NO content elements
- Any element from divcomponent.mix

### 3.4 Valid TOC Example

```xml
<toc>
    <title>Contents</title>

    <!-- Front matter -->
    <tocfront linkend="dd0001">Dedication</tocfront>
    <tocfront linkend="pr0001">Preface</tocfront>

    <!-- Main content -->
    <tocpart>
        <tocentry linkend="pt0001">Part I: Fundamentals</tocentry>
        <tocchap>
            <tocentry linkend="ch0001">Chapter 1: Introduction</tocentry>
            <toclevel1>
                <tocentry linkend="ch0001s0001">1.1 Getting Started</tocentry>
                <toclevel2>
                    <tocentry linkend="ch0001s0001s0001">1.1.1 Prerequisites</tocentry>
                </toclevel2>
            </toclevel1>
            <toclevel1>
                <tocentry linkend="ch0001s0002">1.2 Basic Concepts</tocentry>
            </toclevel1>
        </tocchap>
    </tocpart>

    <!-- Back matter -->
    <tocback linkend="ap0001">Appendix A: Reference</tocback>
    <tocback linkend="gl0001">Glossary</tocback>
    <tocback linkend="bi0001">Bibliography</tocback>
</toc>
```

### 3.5 TOC Validation Rules

1. Every `linkend` attribute MUST point to an existing `@id` in the document
2. `tocentry` text should match the corresponding section/chapter title
3. Nesting must follow hierarchy: tocpart → tocchap → toclevel1 → toclevel2 → ...
4. Cannot skip levels (no toclevel3 inside toclevel1)

---

## 4. SECT1 Content Model (Inside Chapters/Preface/Appendix)

### 4.1 Element Structure

```
sect1
├── beginpage?              (optional, MUST be first)
├── sect1info?              (optional metadata)
├── title                   (REQUIRED - must have text)
├── subtitle?               (optional)
├── titleabbrev?            (optional)
├── (divcomponent.mix)*     (block content)
├── (sect2 | simplesect)*   (nested sections)
└── (toc | lot | index | glossary | bibliography | refentry)*
```

### 4.2 Sect1 ID Requirements

**CRITICAL:** All sect1 elements MUST have an `@id` attribute.

| Context | ID Format | Example |
|---------|-----------|---------|
| Chapter | `ch{4-digit}s{4-digit}` | `ch0001s0001` |
| Preface | `pr{4-digit}s{4-digit}` | `pr0001s0001` |
| Appendix | `ap{4-digit}s{4-digit}` | `ap0001s0001` |
| Dedication | N/A | Sect1 not allowed in dedication |

### 4.3 Sect1 Title Requirement

Every sect1 MUST have a `<title>` element with text content. Empty titles cause TOC generation issues.

---

## 5. Validation Error Reference

### 5.1 splitContentFiles Failures

| Error Pattern | Element | Cause | Solution |
|---------------|---------|-------|----------|
| Invalid content model | preface/appendix/chapter | Block content after sect1 | Move content before sect1 or wrap in new sect1 |
| Element not allowed | dedication | Using anchor/figure/table directly | Use only legalnotice.mix elements |
| Element not allowed | dedication | Using sect1 | Remove sect1; dedication cannot have sections |
| Missing required element | preface/appendix/chapter | No title or empty title | Add `<title>` with text content |
| Attribute required | sect1 | Missing @id attribute | Add unique @id to all sect1 elements |
| Invalid content | toc | Using para/sect1 | Use tocchap/toclevel1-5/tocentry only |
| ID not found | toc | tocentry/@linkend invalid | Ensure all linkend targets exist |
| Duplicate ID | any | Same @id used multiple times | Ensure all IDs are unique |

### 5.2 Common Conversion Errors

| Source Issue | Result | Fix |
|--------------|--------|-----|
| EPUB has `<a>` without href | `<anchor>` in dedication | Wrap in `<para>` |
| EPUB has inline image | `<mediaobject>` in dedication | Move to different element or wrap |
| Bridgehead after sect1 | Block content after sections | Move before sect1 or create new sect1 |
| Unstructured content | No sect1 in chapter | Create synthetic sect1 wrapper |
| Mixed content | para mixed with sect1 | Group para before sect1 elements |

---

## 6. Validation Checklist

### Before splitContentFiles:

- [ ] All chapter/preface/appendix elements have `<title>` with text
- [ ] All sect1 elements have unique `@id` attributes
- [ ] No block content appears AFTER sect1 elements
- [ ] No block content appears BETWEEN sect1 elements
- [ ] Dedication only contains legalnotice.mix elements
- [ ] Dedication has no `<anchor>` as direct child (wrap in para)
- [ ] Dedication has no `<figure>`, `<table>`, `<sidebar>`, `<sect1>`
- [ ] TOC only contains toc* elements (tocfront, tocchap, toclevel1-5, tocentry, tocback)
- [ ] All TOC linkend attributes point to valid IDs
- [ ] No duplicate IDs in entire document

---

## 7. Implementation Notes

### 7.1 Synthetic Sect1 Creation

When content must be wrapped in a synthetic sect1:

1. **ID Generation:** Use pattern `{parent_id}s{next_available}`
2. **Title:** Use parent element's title (chapter/preface/appendix title)
3. **Position:** Insert after existing sect1 elements, before nav.class

### 7.2 Content Relocation

When block content appears after sect1:

1. **Between sections:** Move into preceding sect1
2. **After all sections:** Create new synthetic sect1 and move content into it
3. **Never leave orphan block content** at chapter/preface/appendix level after sect1

### 7.3 Dedication Fixes

When invalid elements found in dedication:

1. **anchor:** Wrap in `<para><anchor id="..."/></para>`
2. **indexterm:** Wrap in `<para><indexterm>...</indexterm></para>`
3. **figure/table:** Cannot fix - must be removed or moved to different element
4. **sect1:** Cannot fix - must be removed; dedication cannot have sections

---

## 8. Element Ordering Rules (Per DTD - Critical for XSL Processing)

The XSL transformations (AddRISInfo.xsl, RittBook.xsl, etc.) expect elements in a specific order. Incorrect ordering causes processing failures.

### 8.1 Chapter Element Ordering

```
chapter
├── 1. beginpage?           (optional, MUST be absolutely first)
├── 2. chapterinfo?         (optional, MUST come before title)
├── 3. title                (REQUIRED)
├── 4. subtitle?            (optional, after title)
├── 5. titleabbrev?         (optional, after subtitle)
├── 6. (toc|lot|index|glossary|bibliography)*  (nav.class, optional)
├── 7. tocchap?             (optional)
├── 8. CONTENT              (bookcomponent.content)
│   ├── (divcomponent.mix)+, sect1*   -- Pattern A
│   └── sect1+                         -- Pattern B
└── 9. (toc|lot|index|glossary|bibliography)*  (nav.class, optional)
```

**DTD Definition:**
```dtd
<!ELEMENT chapter (beginpage?, chapterinfo?,
    (%bookcomponent.title.content;),
    (%nav.class;)*, tocchap?,
    (%bookcomponent.content;),
    (%nav.class;)*)>
```

### 8.2 Preface Element Ordering

```
preface
├── 1. beginpage?           (optional, MUST be absolutely first)
├── 2. prefaceinfo?         (optional, MUST come before title)
├── 3. title                (REQUIRED)
├── 4. subtitle?            (optional)
├── 5. titleabbrev?         (optional)
├── 6. (nav.class)*         (optional)
├── 7. tocchap?             (optional)
├── 8. CONTENT              (bookcomponent.content)
└── 9. (nav.class)*         (optional)
```

### 8.3 Appendix Element Ordering

```
appendix
├── 1. beginpage?           (optional, MUST be absolutely first)
├── 2. appendixinfo?        (optional, MUST come before title)
├── 3. title                (REQUIRED)
├── 4. subtitle?            (optional)
├── 5. titleabbrev?         (optional)
├── 6. (nav.class)*         (optional)
├── 7. tocchap?             (optional)
├── 8. CONTENT              (bookcomponent.content)
└── 9. (nav.class)*         (optional)
```

### 8.4 Sect1 Element Ordering

```
sect1
├── 1. sect1info?           (optional, MUST be before title)
│   └── Contains: (risindex|risinfo|info.class)+
├── 2. title                (REQUIRED)
├── 3. subtitle?            (optional)
├── 4. titleabbrev?         (optional)
├── 5. (nav.class)*         (optional)
├── 6. CONTENT
│   ├── (divcomponent.mix)+, (sect2*|simplesect*)  -- Pattern A
│   └── sect2+ | simplesect+                        -- Pattern B
└── 7. (nav.class)*         (optional)
```

**DTD Definition:**
```dtd
<!ELEMENT sect1 (sect1info?, (%sect.title.content;), (%nav.class;)*,
    (((%divcomponent.mix;)+, ((%refentry.class;)* | sect2* | simplesect*))
    | (%refentry.class;)+ | sect2+ | simplesect+), (%nav.class;)*)>
```

### 8.5 Dedication Element Ordering (Special - No Sections)

```
dedication
├── 1. risinfo?             (RIT-specific, optional, MUST be first)
├── 2. title?               (optional - NOT required like chapter)
├── 3. subtitle?            (optional)
├── 4. titleabbrev?         (optional)
└── 5. (legalnotice.mix)+   (REQUIRED - at least one)
```

**DTD Definition:**
```dtd
<!ELEMENT dedication ((%sect.title.content;)?, (%legalnotice.mix;)+)>
```

### 8.6 TOC Element Ordering (Special - No Standard Content)

```
toc
├── 1. beginpage?           (optional)
├── 2. title?               (optional)
├── 3. subtitle?            (optional)
├── 4. titleabbrev?         (optional)
├── 5. tocfront*            (front matter entries)
├── 6. (tocpart|tocchap)*   (main content)
│   ├── tocpart → tocentry+, tocchap*
│   └── tocchap → tocentry+, toclevel1*
│       └── toclevel1 → tocentry+, toclevel2*
│           └── ... up to toclevel5
└── 7. tocback*             (back matter entries)
```

### 8.7 Info Element Content (chapterinfo, prefaceinfo, appendixinfo, sect1info)

All *info elements share similar content model:

```
*info
└── (info.class)+
    ├── authorgroup | author | editor | othercredit
    ├── corpauthor | corpcredit | collabname | collab
    ├── abstract | address | bibliomisc
    ├── copyright | legalnotice | pubdate
    ├── revhistory | edition | printhistory
    ├── isbn | issn | biblioid | bibliosource
    ├── date | releaseinfo | contractnum | contractsponsor
    └── ... and more metadata elements
```

**RIT Extensions (sect1info):**
```
sect1info
└── (risindex | risinfo | info.class)+
```

---

## 9. Critical Ordering Violations and Fixes

### 9.1 beginpage Position Violations

| Violation | Fix |
|-----------|-----|
| beginpage after title | Move beginpage to position 0 (before info element) |
| beginpage after info | Move beginpage to position 0 |
| Multiple beginpage elements | Remove duplicates, keep first |

**Fix Code Pattern:**
```python
def fix_beginpage_position(elem):
    beginpage = elem.find('beginpage')
    if beginpage is not None:
        elem.remove(beginpage)
        elem.insert(0, beginpage)  # Always first
```

### 9.2 Info Element Position Violations

| Violation | Fix |
|-----------|-----|
| chapterinfo after title | Move chapterinfo to position 1 (after beginpage, before title) |
| Info element after content | Move before title |
| Empty info element | Remove (optional element) |

**Fix Code Pattern:**
```python
def fix_info_position(elem, info_tag):
    info_elem = elem.find(info_tag)
    if info_elem is not None:
        elem.remove(info_elem)
        # Insert after beginpage if present, else at position 0
        beginpage = elem.find('beginpage')
        insert_pos = 1 if beginpage is not None else 0
        elem.insert(insert_pos, info_elem)
```

### 9.3 Title Position Violations

| Violation | Fix |
|-----------|-----|
| title after subtitle | Swap order |
| title after content | Move title to correct position (after info) |
| Missing title | Add empty title element |

**Expected Position:**
- After beginpage (if present)
- After *info element (if present)
- Before subtitle, titleabbrev

### 9.4 Complete Ordering Fix Sequence

When fixing element order, process in this sequence:

```
1. Move beginpage to position 0 (if present)
2. Move *info to position 1 (after beginpage, if present)
3. Move/add title to position 2 (after *info)
4. Move subtitle to position 3 (after title, if present)
5. Move titleabbrev to position 4 (after subtitle, if present)
6. Ensure all nav.class elements are properly positioned
7. Ensure content is in correct position
8. Ensure trailing nav.class elements are at end
```

---

## 10. XSL Processing Dependencies

### 10.1 AddRISInfo.xsl Expectations

This transformation expects:
- `chapterinfo/authorgroup` to exist for author extraction (line 187)
- `chapterinfo/author` as fallback (line 188)
- `chapterinfo/editor` as second fallback (line 189)
- Proper `sect1` structure for `sect1info` generation

### 10.2 toctransform.xsl Expectations

- Expects chapter/preface/appendix to have proper `@id`
- Expects sect1 elements to have `@id` for TOC entry generation
- Uses title text for TOC display

### 10.3 RISChunker.xsl Expectations

- Chapter sect1 elements must have `@id` for filename generation
- Preface/appendix/dedication must have `@id` for their file generation
- Pattern: `sect1.{isbn}.{sect1_id}.xml` or `{element}.{isbn}.{id}.xml`

---

## 11. Known Issues and Discussion Points

### 11.1 beginpage in preface - Java Processor Conflict

**Issue:** The DTD allows `beginpage` as the first optional element in preface:
```
preface = (beginpage?, prefaceinfo?, title, ...)
```

However, the Java processor (LoadBook/R2Utilities) automatically adds `prefaceinfo` to preface elements during processing. When we send:
```xml
<preface id="pr0001">
    <beginpage pagenum="1"/>
    <title>Preface</title>
    ...
</preface>
```

The Java processor transforms it to:
```xml
<preface id="pr0001">
    <prefaceinfo>...</prefaceinfo>    <!-- Added by Java processor -->
    <beginpage pagenum="1"/>          <!-- Now INVALID - must be before prefaceinfo -->
    <title>Preface</title>
    ...
</preface>
```

**Result:** DTD validation failure because `beginpage` must come BEFORE `prefaceinfo`.

**Current Solution:** Remove ALL `beginpage` elements from preface files. Page break information is preserved via `anchor` elements where needed.

**Open for Discussion:**
1. Can the Java processor be modified to check for `beginpage` and insert `prefaceinfo` AFTER it?
2. Should we request a DTD modification to allow `beginpage` after `prefaceinfo`?
3. Is there a different approach to preserve page break information in preface content?

**Affected Elements:** This may also apply to other elements where the Java processor adds `*info` elements:
- `chapter` (adds `chapterinfo`)
- `appendix` (adds `appendixinfo`)

**Note:** Currently, only `preface` has this issue flagged. Monitor for similar issues in chapter/appendix.

---

*Document Version: 1.2*
*Last Updated: January 2026*
*Based on RittDoc DTD v1.1 and LoadBook/R2Utilities requirements*
