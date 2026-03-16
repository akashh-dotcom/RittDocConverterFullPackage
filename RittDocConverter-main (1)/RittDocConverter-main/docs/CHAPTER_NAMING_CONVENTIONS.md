# R2Library Chapter & Content Naming Convention Rules

## Overview

This document defines the naming conventions required for R2Library content to be correctly parsed and transformed. The system uses **section ID prefixes** to determine content types and select appropriate XML source files and XSL transformations.

---

## 1. Section ID Prefix Reference Table

| Prefix | Content Type | XML Source File | Description |
|--------|--------------|-----------------|-------------|
| `dd` | Dedication | `dedication.{isbn}.{sectionId}.xml` | Dedication pages |
| `de` | Dedication | `dedication.{isbn}.{sectionId}.xml` | Alternative dedication prefix |
| `pr` | Preface | `preface.{isbn}.{sectionId}.xml` | Preface/foreword content |
| `ap` | Appendix | `appendix.{isbn}.{sectionId}.xml` | Appendix sections |
| `gl` | Glossary | `book.{isbn}.xml` | Glossary definitions |
| `bi` | Bibliography | `book.{isbn}.xml` | Bibliography/references |
| `pt` | Part | `book.{isbn}.xml` | Part introductions |
| `in` | Index | N/A | Index (excluded from navigation) |
| `ch` | Chapter | `ch####.xml` | Chapter fragment (contains sect1..sect5) |
| `s` | Section | `ch####.xml` | Sections live inside the chapter file |

---

## 2. Section ID Format Specifications

### 2.1 General Format

```
{prefix}{number}[{subprefix}{subnumber}]...
```

- **Prefix**: 2-letter content type identifier (lowercase)
- **Number**: 4-digit zero-padded number
- **Subprefix**: Optional nested identifier
- **Subnumber**: 4-digit zero-padded number for nested items

### 2.2 Specific Formats by Content Type

#### Front Matter

| Type | Format | Example | Notes |
|------|--------|---------|-------|
| Dedication | `dd####` | `dd0001` | Single dedication |
| Dedication (alt) | `de####` | `de0001` | Alternative prefix |
| Preface | `pr####` | `pr0001` | Preface/foreword |
| About | `ab####` | `ab0001` | About sections |
| Contributors | `co####` | `co0001` | Contributor lists |
| Acknowledgments | `ak####` | `ak0001` | Acknowledgment pages |

#### Main Content

| Type | Format | Example | Notes |
|------|--------|---------|-------|
| Part | `pt####` | `pt0001` | Part container |
| Part with Subpart | `pt####sp####` | `pt0001sp0001` | Part with subpart intro |
| Part Entry | `pte####` | `pte0001` | Part entry point |
| Chapter | `ch####` | `ch0001` | Chapter container |
| Section Level 1 | `ch####s####` | `ch0001s0001` | First-level section |
| Section Level 2 | `ch####s####s####` | `ch0001s0001s0001` | Second-level section |
| Section Level 3+ | Continue pattern | `ch0001s0001s0001s0001` | Deeper nesting |

#### Back Matter

| Type | Format | Example | Notes |
|------|--------|---------|-------|
| Appendix | `ap####` | `ap0001` | Single appendix |
| Appendix Section | `ap####s####` | `ap0001s0001` | Appendix subsection |
| Glossary | `gl####` | `gl0001` | Glossary section |
| Bibliography | `bi####` | `bi0001` | Bibliography section |
| Bibliography (sect) | `bibs####` | `bibs0001` | Sectioned bibliography |
| Index | `in####` | `in0001` | Index (not rendered) |

---

### 2.3 Core ID Constraints (All IDs)

| Rule | Value |
|------|-------|
| **Maximum ID length** | 25 characters |
| **Allowed characters** | Lowercase letters, numbers only |
| **Prohibited characters** | Hyphens (`-`), underscores (`_`), spaces, special characters |

These constraints apply to **section IDs** and **all element IDs**.

---

### 2.4 Element ID Format (Inside Sect1 Files)

All element IDs inside a `ch####.xml` chapter file **must start with the sect1 ID** for that file.

```
{sect1_id}{element_code}{sequence_number}
```

Example:

```
sect1_id = ch0001s0001
table_id = ch0001s0001ta0001
```

---

### 2.4.1 Why Element IDs Must Use Sect1 Prefix (R2 Library Navigation)

**Critical:** R2 Library **parses the `linkend` value** to determine which file to load. It extracts the sect1 ID from the beginning of the linkend string.

```
linkend="ch0011s000000b0001"
         └──────────┘└─────┘
         sect1 ID    element identifier
         (file name) (fragment)
```

**How R2 Library resolves links:**
1. Extracts the sect1 ID prefix from `linkend` (first 11-15 characters matching `{prefix}{4-digits}s{4-digits}`)
2. Uses this sect1 ID to locate the correct XML file/chunk
3. Navigates to the specific element fragment within that file

**If the sect1 ID is not correctly embedded at the start, R2 will navigate to the wrong file or fail entirely.**

Example - Elements inside nested sections:
```xml
<sect1 id="ch0011s000000">
   <sect2 id="ch0011s000000s20001">        <!-- sect2 ID includes hierarchy -->
      <table id="ch0011s000000t0001">      <!-- table uses sect1 prefix ONLY -->
      </table>
      <figure id="ch0011s000000f0001">     <!-- figure uses sect1 prefix ONLY -->
      </figure>
   </sect2>
</sect1>
```

**Rule:** Even for elements inside `sect2`, `sect3`, or deeper, the element ID always uses the **sect1 ID** as the prefix (since files are split at sect1 level for chunking).

---

### 2.5 Element Type Codes

| Element | Code | Example ID |
|---------|------|------------|
| Bibliography/Reference | `bib` | `ch0001s0001bib0001` |
| Table | `ta` | `ch0001s0001ta0001` |
| Figure | `fg` | `ch0001s0001fg0001` |
| Sidebar/Box | `ad` | `ch0001s0001ad0001` |
| Anchor | `a` | `ch0001s0001a0001` |
| Glossary Entry | `gl` | `ch0001s0001gl0001` |
| Paragraph | `p` | `ch0001s0001p0001` |
| Equation | `eq` | `ch0001s0001eq0001` |
| Q&A Set | `qa` | `ch0001s0001qa0001` |
| Admonition | `ad` | `ch0001s0001ad0001` |

---

### 2.6 Sequence Numbering

**Standard (4 digits)**
- Use when `sect1_id` is short enough to stay within 25 characters.
- Example: `ch0001s0001bib0001`

**Compact (2-3 digits)**
- Use only if the ID would exceed 25 characters.
- Example: `ch0001s0001bib01`

**Character budget rule:**
```
digits_available = 25 - len(sect1_id) - len(element_code)
```

---

### 2.7 Nested Sections

Section IDs **always** follow the nested `s####` pattern from section 2.2:

```
<sect1 id="ch0001s0001">
    <sect2 id="ch0001s0001s0001">
        <figure id="ch0001s0001fg0001">...</figure>
    </sect2>
</sect1>
```

Even inside `sect2` or deeper, **element IDs still use the sect1 prefix**.

---

## 3. XML File Naming Conventions

### 3.1 File Name Patterns

```
{contentType}.{isbn}.{sectionId}.xml
```

**Exception (current packaging output):**

```
ch####.xml
```

Chapter/section content is emitted as `ch####.xml` fragments and is **not** split into sect1 files.

### 3.2 Complete File Name Reference

| Content Type | File Pattern | Example |
|--------------|--------------|---------|
| Table of Contents | `toc.{isbn}.xml` | `toc.9781234567890.xml` |
| Dedication | `dedication.{isbn}.{sectionId}.xml` | `dedication.9781234567890.dd0001.xml` |
| Preface | `preface.{isbn}.{sectionId}.xml` | `preface.9781234567890.pr0001.xml` |
| Appendix | `appendix.{isbn}.{sectionId}.xml` | `appendix.9781234567890.ap0001.xml` |
| Glossary | `book.{isbn}.xml` | `book.9781234567890.xml` |
| Bibliography | `book.{isbn}.xml` | `book.9781234567890.xml` |
| Part | `book.{isbn}.xml` | `book.9781234567890.xml` |
| Chapter/Section | `ch####.xml` | `ch0001.xml` |

### 3.3 ISBN Format

- Use **ISBN-13** format (13 digits)
- No hyphens or spaces
- Example: `9781234567890`

---

## 4. Table of Contents (TOC) Structure

### 4.1 TOC File Name

```
toc.{isbn}.xml
```

### 4.2 TOC XML Element Hierarchy

```xml
<toc>
    <title>Book Title</title>

    <!-- FRONT MATTER -->
    <tocfront linkend="{sectionId}">{Title}</tocfront>

    <!-- MAIN CONTENT -->
    <tocpart role="partintro">
        <tocentry linkend="{partId}">{Part Title}</tocentry>
        <tocsubpart role="partintro">
            <tocentry linkend="{subpartId}">{Subpart Title}</tocentry>
            <tocchap>
                <tocentry linkend="{chapterId}">{Chapter Title}</tocentry>
                <toclevel1>
                    <tocentry linkend="{sectionId}">{Section Title}</tocentry>
                    <toclevel2>...</toclevel2>
                </toclevel1>
            </tocchap>
        </tocsubpart>
    </tocpart>

    <!-- BACK MATTER -->
    <tocback linkend="{sectionId}">{Title}</tocback>
</toc>
```

### 4.3 TOC Element Reference

| Element | Purpose | Attributes | Used For |
|---------|---------|------------|----------|
| `<toc>` | Root element | — | Container |
| `<title>` | Book title | — | Main title |
| `<tocfront>` | Front matter entry | `linkend` | Dedication, Preface, About |
| `<tocpart>` | Part container | `role="partintro"` (optional) | Parts |
| `<tocsubpart>` | Subpart container | `role="partintro"` (optional) | Subparts |
| `<tocchap>` | Chapter container | — | Chapters |
| `<tocentry>` | Entry with link | `linkend` | Any linked item |
| `<toclevel1>` | Section level 1 | — | Sect1 |
| `<toclevel2>` | Section level 2 | — | Sect2 |
| `<toclevel3>` | Section level 3 | — | Sect3 |
| `<toclevel4>` | Section level 4 | — | Sect4 |
| `<toclevel5>` | Section level 5 | — | Sect5 |
| `<tocback>` | Back matter entry | `linkend` | Appendix, Glossary, Bibliography |

### 4.4 TOC linkend Rules

- **MUST** match the `id` attribute in corresponding content XML
- **MUST** follow section ID prefix conventions
- **CASE-SENSITIVE**
- **MUST NOT** include `.xml` extensions

---

### 4.5 Link Type Usage (Outside TOC)

| Scenario | Element | Example |
|----------|---------|---------|
| Internal element reference | `link` | `<link linkend="ch0001s0001bib0001">[1]</link>` |
| Chapter navigation | `ulink` | `<ulink url="ch0002">Chapter 2</ulink>` |
| External URL | `ulink` | `<ulink url="https://example.com">Example</ulink>` |

**Rule:** Use `<link linkend="...">` for all internal element references; use `<ulink url="...">` only for chapter navigation and external URLs.

---

## 5. Content XML Structure Templates

### 5.1 Dedication

**File:** `dedication.{isbn}.dd0001.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<dedication id="dd0001">
    <title>Dedication</title>
    <para>Dedication text content...</para>
</dedication>
```

### 5.2 Preface

**File:** `preface.{isbn}.pr0001.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<preface id="pr0001">
    <prefaceinfo>
        <authorgroup>
            <author>
                <firstname>John</firstname>
                <surname>Doe</surname>
            </author>
        </authorgroup>
    </prefaceinfo>
    <title>Preface</title>
    <para>Preface content...</para>
</preface>
```

### 5.3 Chapter Section

**File:** `ch0001.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<chapter id="ch0001">
    <title>Chapter 1 Title</title>

    <sect1 id="ch0001s0001">
        <title>Section Title</title>
        <para>Section content...</para>

        <!-- Nested sections -->
        <sect2 id="ch0001s0001s0001">
            <title>Subsection Title</title>
            <para>Subsection content...</para>

            <sect3 id="ch0001s0001s0001s0001">
                <title>Sub-subsection</title>
                <para>Content...</para>
            </sect3>
        </sect2>
    </sect1>
</chapter>
```

### 5.4 Appendix

**File:** `appendix.{isbn}.ap0001.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<appendix id="ap0001">
    <appendixinfo>
        <authorgroup>
            <author>
                <firstname>John</firstname>
                <surname>Doe</surname>
            </author>
        </authorgroup>
    </appendixinfo>
    <title>Appendix A: Additional Information</title>
    <para>Appendix content...</para>

    <sect1 id="ap0001s0001">
        <title>Appendix Section</title>
        <para>Section content...</para>
    </sect1>
</appendix>
```

### 5.5 Glossary

**File:** `book.{isbn}.xml` (embedded in book)

```xml
<glossary id="gl0001">
    <glossaryinfo>
        <title>Glossary</title>
    </glossaryinfo>
    <title>Glossary</title>

    <glossdiv>
        <title>A</title>
        <glossentry id="gl0001term0001">
            <glossterm>Aardvark</glossterm>
            <glossdef>
                <para>Definition of aardvark...</para>
            </glossdef>
        </glossentry>
    </glossdiv>

    <glossdiv>
        <title>B</title>
        <glossentry id="gl0001term0002">
            <glossterm>Bibliography</glossterm>
            <glossdef>
                <para>Definition of bibliography...</para>
            </glossdef>
        </glossentry>
    </glossdiv>
</glossary>
```

### 5.6 Bibliography

**File:** `book.{isbn}.xml` (embedded in book)

```xml
<!-- Bibliography element uses sect1-style 11-character ID format -->
<bibliography id="ch0021s0014">
    <title>Bibliography</title>

    <bibliomixed id="ch0021s0014bib01">
        <authorgroup>
            <author>
                <firstname>John</firstname>
                <surname>Smith</surname>
            </author>
        </authorgroup>
        <title>Article Title</title>
        <volumenum>Vol. 25</volumenum>
        <address>Publisher Location</address>
        <biblioid otherclass="PubMedID">12345678</biblioid>
    </bibliomixed>

    <bibliomixed id="ch0021s0014bib02">
        <author>
            <firstname>Jane</firstname>
            <surname>Doe</surname>
        </author>
        <title>Book Title</title>
        <publishername>Publisher Name</publishername>
        <pubdate>2023</pubdate>
    </bibliomixed>
</bibliography>
```

### 5.7 Part Introduction

**File:** `book.{isbn}.xml` (embedded in book)

```xml
<part id="pt0001">
    <partinfo>
        <authorgroup>
            <author>
                <firstname>Editor</firstname>
                <surname>Name</surname>
            </author>
        </authorgroup>
    </partinfo>
    <title>Part I: Foundation Concepts</title>

    <partintro id="pt0001sp0001">
        <title>Introduction to Part I</title>
        <para>Overview of this part...</para>
    </partintro>

    <!-- Chapters within part reference external files -->
</part>
```

---

## 6. Media & Asset Naming

### 6.1 Image Files

**Location:** `images/{isbn}/`

**Naming Pattern:** `{descriptive-name}.{extension}`

| Type | Allowed Extensions | Example |
|------|-------------------|---------|
| Photographs | `.jpg`, `.jpeg` | `figure-1-1.jpg` |
| Diagrams | `.png`, `.gif` | `diagram-2-3.png` |
| Inline | `.jpg`, `.png`, `.gif` | `inline-symbol.png` |

**XML Reference:**
```xml
<figure id="ch0001s0001f0001">
    <mediaobject>
        <imageobject>
            <imagedata fileref="figure-1-1.jpg"/>
        </imageobject>
    </mediaobject>
    <title>Figure 1-1: Description</title>
</figure>
```

### 6.2 Video Files

**Location:** `videos/{isbn}/`

**XML Reference:**
```xml
<mediaobject>
    <videoobject>
        <videodata fileref="video-1-1.mp4"/>
    </videoobject>
</mediaobject>
```

### 6.3 Audio Files

**Location:** `audio/{isbn}/`

**XML Reference:**
```xml
<mediaobject>
    <audioobject>
        <audiodata fileref="audio-1-1.mp3"/>
    </audioobject>
</mediaobject>
```

---

## 7. Special Cases & Edge Cases

### 7.1 Part Detection Logic

The system determines if a `pt` prefix is a Part intro or Book content:

```
IF sectionId starts with "pt" THEN:
    IF length > 6 AND characters 5-6 == "sp" THEN → Part (partintro)
    ELSE IF starts with "pte" THEN → Part
    ELSE → Book (regular content)
```

**Examples:**
- `pt0001sp0001` → Part (partintro)
- `pte0001` → Part
- `pt0001ch0001` → Book (chapter within part)

### 7.2 Bibliography Sectioned Detection

```
IF sectionId starts with "bibs" THEN → Sect1 (sectioned bibliography)
ELSE IF starts with "bi" THEN → Bibliography (book-level)
```

### 7.3 Auto-Capitalized TOC Entries

The following entries are automatically UPPERCASED in the TOC:
- "Dedication"
- "About"

### 7.4 Excluded from Navigation

Section IDs starting with `in` (Index) are excluded from navigation rendering.

---

## 8. Validation Checklist

Before submitting content, verify:

### File Naming
- [ ] ISBN is 13 digits, no hyphens
- [ ] File prefix matches content type
- [ ] Section ID follows correct format
- [ ] Zero-padding is 4 digits

### TOC Integrity
- [ ] All `linkend` attributes match content file `id` attributes
- [ ] Hierarchy is correct (tocpart > tocchap > toclevel1 > toclevel2...)
- [ ] Front matter uses `tocfront`
- [ ] Back matter uses `tocback`

### Content XML
- [ ] Root element `id` matches filename section ID
- [ ] ISBN embedded in `risinfo` matches filename ISBN
- [ ] All nested section IDs follow numbering convention
- [ ] All figure/table IDs are unique
- [ ] All IDs are lowercase letters and numbers only
- [ ] All IDs are ≤ 25 characters
- [ ] All element IDs start with the correct sect1 ID prefix

### Media Assets
- [ ] Images in correct folder: `images/{isbn}/`
- [ ] `fileref` paths match actual filenames
- [ ] No spaces in filenames

---

## 9. Complete Example: Book Structure

```
/content/
├── toc.9781234567890.xml
├── book.9781234567890.xml
├── dedication.9781234567890.dd0001.xml
├── preface.9781234567890.pr0001.xml
├── ch0001.xml
├── ch0002.xml
├── appendix.9781234567890.ap0001.xml
└── /images/9781234567890/
    ├── figure-1-1.jpg
    ├── figure-1-2.png
    └── table-1-1.png
```

**Corresponding TOC:**
```xml
<toc>
    <title>Medical Textbook, 5th Edition</title>
    <tocfront linkend="dd0001">DEDICATION</tocfront>
    <tocfront linkend="pr0001">Preface</tocfront>
    <tocpart>
        <tocentry linkend="pt0001">Part I: Basics</tocentry>
        <tocchap>
            <tocentry linkend="ch0001">Chapter 1: Introduction</tocentry>
            <toclevel1>
                <tocentry linkend="ch0001s0001">Overview</tocentry>
            </toclevel1>
            <toclevel1>
                <tocentry linkend="ch0001s0002">History</tocentry>
            </toclevel1>
        </tocchap>
        <tocchap>
            <tocentry linkend="ch0002">Chapter 2: Foundations</tocentry>
            <toclevel1>
                <tocentry linkend="ch0002s0001">Core Concepts</tocentry>
            </toclevel1>
        </tocchap>
    </tocpart>
    <tocback linkend="ap0001">Appendix A: Reference Data</tocback>
    <tocback linkend="gl0001">Glossary</tocback>
    <tocback linkend="bi0001">Bibliography</tocback>
</toc>
```

---

## 10. Error Reference

| Error | Cause | Solution |
|-------|-------|----------|
| Section not found | `linkend` doesn't match any `id` | Verify TOC linkend matches content id |
| Wrong content type | Incorrect prefix used | Use correct 2-letter prefix |
| File not loading | Filename doesn't match pattern | Check ISBN and sectionId in filename |
| Images not displaying | Wrong path in fileref | Verify images in `images/{isbn}/` folder |
| Part not recognized | Missing "sp" in partintro | Use `pt####sp####` for part intros |

---

*Document Version: 1.0*
*Based on R2Library codebase analysis*
