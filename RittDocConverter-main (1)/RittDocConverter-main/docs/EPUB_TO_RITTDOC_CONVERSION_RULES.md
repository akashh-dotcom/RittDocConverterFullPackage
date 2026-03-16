# EPUB to RittDoc XML Conversion Rules

## Comprehensive Specification for Document Conversion

This document provides detailed rules for converting EPUB files to RittDoc XML format, which is based on DocBook V4.3. These rules are derived from analysis of multiple converted book packages and their source EPUBs.

---

## Table of Contents

1. [Output Structure Overview](#1-output-structure-overview)
2. [DOCTYPE and DTD Declaration](#2-doctype-and-dtd-declaration)
3. [File Organization](#3-file-organization)
4. [Book-Level Structure](#4-book-level-structure)
5. [Chapter File Structure](#5-chapter-file-structure)
6. [CSS Class to XML Element Mapping](#6-css-class-to-xml-element-mapping)
7. [Text Formatting Rules](#7-text-formatting-rules)
8. [Section Hierarchy](#8-section-hierarchy)
9. [Special Content Handling](#9-special-content-handling)
10. [Bibliography Formatting](#10-bibliography-formatting)
11. [Figure and Image Handling](#11-figure-and-image-handling)
12. [Table Handling](#12-table-handling)
13. [Footnote and Endnote Handling](#13-footnote-and-endnote-handling)
14. [Link Handling](#14-link-handling)
15. [Character Encoding](#15-character-encoding)
16. [ID Generation Rules](#16-id-generation-rules)
17. [Processing Instructions](#17-processing-instructions)
18. [Validation Requirements](#18-validation-requirements)

---

## 1. Output Structure Overview

Each converted book produces a ZIP package containing:

```
[ISBN]/
├── book.xml           # Master document with entity declarations
├── ch0000.xml         # Front matter (preface, dedication, etc.)
├── ch0001.xml         # Chapter 1
├── ch0002.xml         # Chapter 2
├── ...
├── ap0001.xml         # Appendix 1 (if applicable)
├── bm0001.xml         # Back matter (bibliography, index)
└── multimedia/        # Images folder
    ├── image1.jpg
    ├── image2.jpg
    └── ...
```

---

## 2. DOCTYPE and DTD Declaration

### Standard DOCTYPE Declaration

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd"[
    <!ENTITY ch0000 SYSTEM "ch0000.xml">
    <!ENTITY ch0001 SYSTEM "ch0001.xml">
    <!ENTITY ch0002 SYSTEM "ch0002.xml">
    <!-- Additional entity declarations for each chapter file -->
]>
```

### Entity Declaration Rules

| Content Type | Entity Pattern | File Pattern |
|-------------|----------------|--------------|
| Front matter | `ch0000` | `ch0000.xml` |
| Chapters | `ch0001`, `ch0002`, etc. | `ch0001.xml`, `ch0002.xml` |
| Appendices | `ap0001`, `ap0001a`, etc. | `ap0001.xml`, `ap0001a.xml` |
| Back matter | `bm0001` | `bm0001.xml` |

---

## 3. File Organization

### Naming Conventions

| Content | File Name Pattern | Notes |
|---------|------------------|-------|
| Master file | `book.xml` | Contains all entity references |
| Front matter | `ch0000.xml` | Preface, dedication, acknowledgments |
| Chapters | `ch####.xml` | Zero-padded 4-digit numbers |
| Appendices | `ap####.xml` or `ap####a.xml` | Letter suffix for sub-appendices |
| Back matter | `bm0001.xml` | Bibliography, index, etc. |
| Images | `multimedia/*.jpg` | Convert PNG to JPG |

### Image Conversion Rules

1. Convert all PNG images to JPG format
2. Store all images in `multimedia/` subdirectory
3. Maintain original aspect ratios
4. Use descriptive filenames (e.g., `AuthorName_Fig_01.jpg`)

---

## 4. Book-Level Structure

### Master book.xml Template

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd"[
    <!ENTITY ch0000 SYSTEM "ch0000.xml">
    <!ENTITY ch0001 SYSTEM "ch0001.xml">
]>
<book id="b001">
<title>Book Title Here</title>
<subtitle>Book Subtitle Here</subtitle>
<bookinfo>
<author><firstname>First</firstname> <surname>Last</surname> <degree>, PhD</degree></author>
<publisher><publishername>Publisher Name</publishername></publisher>
<copyright><year>2024</year><holder>Copyright Holder</holder></copyright>
<isbn>9781234567890</isbn>
<legalnotice><para>Legal notice text...</para></legalnotice>
</bookinfo>
&ch0000;
<part id="p001">
<title>Part One</title>
<partintro><para>Introduction to this part...</para></partintro>
&ch0001;
</part>
</book>
```

### Bookinfo Elements

| Element | Required | Description |
|---------|----------|-------------|
| `<author>` | Yes | Contains `<firstname>`, `<surname>`, optionally `<degree>` |
| `<publisher>` | Yes | Contains `<publishername>` |
| `<copyright>` | Yes | Contains `<year>` and `<holder>` |
| `<isbn>` | Yes | 13-digit ISBN without hyphens |
| `<legalnotice>` | No | Copyright and legal text |
| `<edition>` | No | Edition information |

### Multiple Authors

```xml
<bookinfo>
<author><firstname>John</firstname> <surname>Smith</surname> <degree>, MD</degree></author>
<author><firstname>Jane</firstname> <surname>Doe</surname> <degree>, PhD</degree></author>
</bookinfo>
```

---

## 5. Chapter File Structure

### Basic Chapter Template

```xml
<chapter id="ch0001">
<title>Chapter Title</title>
<sect1 id="ch0001s0001">
<title>Section Title</title>
<para>Paragraph text here...</para>
<sect2 id="ch0001s0001s0001">
<title>Subsection Title</title>
<para>More text...</para>
</sect2>
</sect1>
</chapter>
```

### Front Matter Elements

| EPUB Content | XML Element | ID Pattern |
|-------------|-------------|------------|
| Preface | `<preface id="pr0001">` | pr#### |
| Dedication | `<dedication id="ded001">` | ded### |
| Acknowledgments | `<preface id="pr0002">` | pr#### |
| Foreword | `<preface id="pr0003">` | pr#### |
| Introduction | `<preface id="pr0004">` or `<chapter>` | pr#### or ch#### |

### Back Matter Elements

| EPUB Content | XML Element | ID Pattern |
|-------------|-------------|------------|
| Bibliography | `<bibliography>` | (within bm####) |
| Appendix | `<appendix id="ap0001">` | ap#### |
| Index | `<index>` | (within bm####) |
| Glossary | `<glossary>` | (within bm####) |

---

## 6. CSS Class to XML Element Mapping

### Heading Classes

| CSS Class | XML Element | Description |
|-----------|-------------|-------------|
| `cn` | `<chapter><title>` (first part) | Chapter number |
| `ct` | `<chapter><title>` (content) | Chapter title |
| `ah` | `<sect1><title>` | A-head (level 1 section) |
| `bh` | `<sect2><title>` | B-head (level 2 section) |
| `ch` | `<sect3><title>` | C-head (level 3 section) |
| `dh` | `<sect4><title>` | D-head (level 4 section) |

### Paragraph Classes

| CSS Class | XML Element | Description |
|-----------|-------------|-------------|
| `p` | `<para>` | Standard paragraph |
| `paft` | `<para>` | Paragraph after heading (no indent) |
| `pcon` | `<para>` | Continued paragraph |
| `pfirst` | `<para>` | First paragraph |
| `plast` | `<para>` | Last paragraph |
| `pcenter` | `<para role="center">` | Centered paragraph |

### Quote and Extract Classes

| CSS Class | XML Element | Description |
|-----------|-------------|-------------|
| `quot` | `<blockquote><para>` | Block quotation |
| `quotfirst` | `<blockquote><para>` | First quote paragraph |
| `quotlast` | `<blockquote><para>` | Last quote paragraph |
| `ext` | `<blockquote><para>` | Extract/extended quote |
| `extfirst` | `<blockquote><para>` | First extract paragraph |
| `extlast` | `<blockquote><para>` | Last extract paragraph |
| `poetry` | `<literallayout>` | Poetry/verse |

### List Classes

| CSS Class | XML Element | Description |
|-----------|-------------|-------------|
| `nl` | `<orderedlist><listitem>` | Numbered list item |
| `bl` | `<itemizedlist><listitem>` | Bulleted list item |
| `nla` | `<orderedlist numeration="loweralpha">` | Alphabetic list |
| `nlr` | `<orderedlist numeration="lowerroman">` | Roman numeral list |

### Special Content Classes

**Important DTD Constraint:** The `<sidebar>` element requires at least one content element (para, etc.) after the optional title. Individual paragraphs with special CSS classes cannot be converted to standalone sidebar elements.

**Recommended approach:** Preserve CSS classes as `role` attributes on `<para>` elements. A post-processing step can group consecutive related paragraphs into `<sidebar>` elements if needed.

| CSS Class | XML Element | Description |
|-----------|-------------|-------------|
| `exh` | `<para role="exh">` | Exercise header (preserve for grouping) |
| `exs` | `<para role="exs">` | Exercise step/content |
| `sth` | `<para role="sth">` | Story/case study header |
| `sts` | `<para role="sts">` | Story/case study content |
| `stf` | `<para role="stf">` | Story footer |
| `boxh` | `<para role="boxh">` | Box header |
| `boxp` | `<para role="boxp">` | Box paragraph |
| `tip` | `<para role="tip">` | Tip content |
| `note` | `<para role="note">` | Note content |
| `warning` | `<para role="warning">` | Warning content |
| `caution` | `<para role="caution">` | Caution content |

**Sidebar Grouping (Post-Processing):**

To create valid `<sidebar>` elements, group consecutive paragraphs:

```xml
<!-- Input: consecutive paras with related roles -->
<para role="exh">Exercise 1.1: Finding Your Values</para>
<para role="exs">Step 1: List three things...</para>
<para role="exs">Step 2: Reflect on...</para>

<!-- Output: grouped into sidebar -->
<sidebar>
  <title>Exercise 1.1: Finding Your Values</title>
  <para>Step 1: List three things...</para>
  <para>Step 2: Reflect on...</para>
</sidebar>
```

### Figure and Table Classes

| CSS Class | XML Element | Description |
|-----------|-------------|-------------|
| `figcap` | `<figure><title>` | Figure caption |
| `fignum` | `<figure><title><emphasis>` | Figure number |
| `tabcap` | `<table><title>` | Table caption |
| `tabnum` | `<table><title><emphasis>` | Table number |
| `tabsrc` | `<para role="source">` | Table source note |

---

## 7. Text Formatting Rules

### Inline Element Mapping

| EPUB Element/Class | XML Element | Example |
|-------------------|-------------|---------|
| `<span class="i">` or `<em>` | `<emphasis>` | `<emphasis>italic text</emphasis>` |
| `<span class="b">` or `<strong>` | `<emphasis role="strong">` | `<emphasis role="strong">bold</emphasis>` |
| `<span class="bi">` | `<emphasis role="strong"><emphasis>` | Nested bold-italic |
| `<span class="sc">` | `<emphasis role="smallcaps">` | Small caps |
| `<span class="sup">` or `<sup>` | `<superscript>` | Superscript |
| `<span class="sub">` or `<sub>` | `<subscript>` | Subscript |
| `<span class="u">` or `<u>` | `<emphasis role="underline">` | Underline |
| `<code>` | `<literal>` | Code/literal text |

### Line Break Handling

| EPUB Element | XML Output | Description |
|-------------|------------|-------------|
| `<br/>` | `<?lb?>` | Line break processing instruction |
| `<br class="softbreak"/>` | (remove) | Soft break - remove |

### Whitespace Rules

1. Preserve single spaces between words
2. Collapse multiple spaces to single space
3. Trim leading/trailing whitespace from paragraphs
4. Preserve line breaks within `<literallayout>` elements

---

## 8. Section Hierarchy

### Hierarchy Mapping

```
EPUB CSS Classes          XML Structure
─────────────────         ─────────────
cn/ct (Chapter)     →     <chapter>
  ah (A-head)       →       <sect1>
    bh (B-head)     →         <sect2>
      ch (C-head)   →           <sect3>
        dh (D-head) →             <sect4>
```

### Section ID Generation

```
Chapter 1, Section 1:           ch0001s0001
Chapter 1, Section 1, Sub 1:    ch0001s0001s0001
Chapter 1, Section 2:           ch0001s0002
```

### Nesting Rules

1. `<sect2>` must be inside `<sect1>`
2. `<sect3>` must be inside `<sect2>`
3. `<sect4>` must be inside `<sect3>`
4. Empty titles are allowed: `<title/>`
5. Sections can have multiple subsections

---

## 9. Special Content Handling

### Sidebar/Box Content

```xml
<sidebar id="ch0001sb0001">
<title>Exercise: Finding Your Inner Strength</title>
<para>Step 1: Begin by sitting comfortably...</para>
<para>Step 2: Close your eyes and breathe deeply...</para>
</sidebar>
```

### Epigraph Handling

```xml
<epigraph>
<para>Quote text here...</para>
<attribution>Author Name</attribution>
</epigraph>
```

### Tip/Note/Warning Handling

```xml
<tip>
<para>This is helpful advice...</para>
</tip>

<note>
<para>Important information to remember...</para>
</note>

<warning>
<para>Be careful about this...</para>
</warning>
```

---

## 10. Bibliography Formatting

### Bibliography Structure

```xml
<bibliography>
<title>References</title>
<bibliodiv>
<title>Chapter 1 References</title>
<bibliomixed id="ch0001bib0001">
<authorgroup>
<author><personname><surname>Smith,</surname> <firstname>J.</firstname></personname></author>
<author><personname><surname>Jones,</surname> <firstname>M.</firstname></personname></author>
</authorgroup>. <pubdate>2020</pubdate>. <title>Article Title</title>. <publishername>Journal Name</publishername>, <volumenum>15</volumenum>, <artpagenums>123-145</artpagenums>.
</bibliomixed>
</bibliodiv>
</bibliography>
```

### Bibliography Elements

| Element | Description | Example |
|---------|-------------|---------|
| `<authorgroup>` | Contains all authors | Wrapper for `<author>` elements |
| `<author>` | Individual author | Contains `<personname>` |
| `<personname>` | Author name parts | Contains `<surname>`, `<firstname>` |
| `<surname>` | Author last name | `<surname>Smith,</surname>` |
| `<firstname>` | Author first name/initials | `<firstname>J.</firstname>` |
| `<pubdate>` | Publication year | `<pubdate>2020</pubdate>` |
| `<title>` | Article/book title | In italics by default |
| `<publishername>` | Publisher or journal | |
| `<volumenum>` | Volume number | |
| `<artpagenums>` | Page numbers | |

### Citation Reference Format

In-text citations use `<xref>` or `<link>`:

```xml
<para>According to research <xref linkend="ch0001bib0001"/>, the findings show...</para>
```

---

## 11. Figure and Image Handling

### Figure Structure

```xml
<figure id="fig0001" float="0">
<title><emphasis>Figure 1.</emphasis> Description of the figure</title>
<mediaobject>
<imageobject>
<imagedata fileref="AuthorName_Fig_01.jpg"/>
</imageobject>
</mediaobject>
</figure>
```

### Figure Attributes

| Attribute | Required | Values | Description |
|-----------|----------|--------|-------------|
| `id` | Yes | `fig####` | Unique figure identifier |
| `float` | No | `0`, `1` | 0 = inline, 1 = floating |

### Image Path Rules

1. All image paths are relative to the XML file location
2. Images stored in `multimedia/` subfolder
3. Use forward slashes for paths
4. Convert PNG to JPG format

### Inline Images

For small inline images (icons, etc.):

```xml
<inlinemediaobject>
<imageobject>
<imagedata fileref="multimedia/icon.jpg"/>
</imageobject>
</inlinemediaobject>
```

---

## 12. Table Handling

### Basic Table Structure

```xml
<table id="tab0001" frame="all">
<title><emphasis>Table 1.</emphasis> Table Description</title>
<tgroup cols="3">
<colspec colnum="1" colname="c1" colwidth="1*"/>
<colspec colnum="2" colname="c2" colwidth="2*"/>
<colspec colnum="3" colname="c3" colwidth="1*"/>
<thead>
<row>
<entry>Header 1</entry>
<entry>Header 2</entry>
<entry>Header 3</entry>
</row>
</thead>
<tbody>
<row>
<entry>Cell 1</entry>
<entry>Cell 2</entry>
<entry>Cell 3</entry>
</row>
</tbody>
</tgroup>
</table>
```

### Table Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `frame` | `all`, `none`, `topbot`, `sides` | Border display |
| `colsep` | `0`, `1` | Column separator |
| `rowsep` | `0`, `1` | Row separator |
| `align` | `left`, `center`, `right` | Content alignment |

### Cell Spanning

```xml
<entry namest="c1" nameend="c3">Spanning cell</entry>  <!-- Column span -->
<entry morerows="2">Spanning cell</entry>              <!-- Row span -->
```

---

## 13. Footnote and Endnote Handling

### Footnote Inline

```xml
<para>Main text with a footnote<footnote id="fn0001"><para>Footnote content here.</para></footnote> continues here.</para>
```

### Endnote Collection

For books using endnotes (collected at chapter or book end):

```xml
<sect1 id="ch0001notes">
<title>Notes</title>
<orderedlist>
<listitem id="en0001"><para>First endnote content.</para></listitem>
<listitem id="en0002"><para>Second endnote content.</para></listitem>
</orderedlist>
</sect1>
```

### Endnote References

```xml
<para>Main text with reference<superscript><xref linkend="en0001"/></superscript> to endnote.</para>
```

---

## 14. Link Handling

### External URLs

```xml
<ulink url="https://www.example.com">Link Text</ulink>
```

### Internal Cross-References

```xml
<xref linkend="ch0002"/>           <!-- Reference to chapter -->
<xref linkend="fig0001"/>          <!-- Reference to figure -->
<xref linkend="tab0001"/>          <!-- Reference to table -->
<link linkend="ch0002s0001">see Section 2.1</link>  <!-- Custom link text -->
```

### Email Links

```xml
<ulink url="mailto:info@example.com">info@example.com</ulink>
```

---

## 15. Character Encoding

### Required Entity Conversions

| Character | XML Entity | Description |
|-----------|------------|-------------|
| & | `&amp;` | Ampersand |
| < | `&lt;` | Less than |
| > | `&gt;` | Greater than |
| " | `&quot;` or `&#x201C;`/`&#x201D;` | Quote marks |
| ' | `&#x2019;` | Apostrophe/single quote |
| — | `&#x2014;` | Em dash |
| – | `&#x2013;` | En dash |
| … | `&#x2026;` | Ellipsis |
| © | `&#xA9;` | Copyright |
| ® | `&#xAE;` | Registered trademark |
| ™ | `&#x2122;` | Trademark |
| ° | `&#xB0;` | Degree symbol |

### Smart Quote Conversion

| Input | Output |
|-------|--------|
| " (left double quote) | `&#x201C;` |
| " (right double quote) | `&#x201D;` |
| ' (left single quote) | `&#x2018;` |
| ' (right single quote/apostrophe) | `&#x2019;` |

### Encoding Declaration

Always use UTF-8 encoding:
```xml
<?xml version="1.0" encoding="UTF-8"?>
```

---

## 16. ID Generation Rules

### ID Patterns by Element Type

| Element Type | Pattern | Example |
|-------------|---------|---------|
| Book | `b###` | `b001` |
| Part | `p###` | `p001`, `p002` |
| Chapter | `ch####` | `ch0001`, `ch0002` |
| Preface | `pr####` | `pr0001` |
| Dedication | `ded###` | `ded001` |
| Appendix | `ap####` or `ap####a` | `ap0001`, `ap0001a` |
| Section (level 1) | `[parent]s####` | `ch0001s0001` |
| Section (level 2) | `[parent]s####` | `ch0001s0001s0001` |
| Figure | `fig####` or `fig#` | `fig0001`, `fig1` |
| Table | `tab####` | `tab0001` |
| Sidebar | `[parent]sb####` | `ch0001sb0001` |
| Bibliography entry | `[parent]bib####` | `ch0001bib0001` |
| Footnote | `fn####` | `fn0001` |
| Endnote | `en####` | `en0001` |

### ID Requirements

1. All IDs must be unique within the document
2. IDs must start with a letter
3. IDs can contain letters, numbers, hyphens, underscores
4. Use zero-padding for numeric portions
5. Maintain parent-child relationships in nested IDs

---

## 17. Processing Instructions

### Line Break

```xml
<?lb?>
```
Used for soft line breaks within paragraphs (replaces `<br/>`).

### Page Break (if applicable)

```xml
<?pb?>
```
Used to indicate page breaks from print layout.

### Custom Processing Instructions

```xml
<?rittdoc instruction="value"?>
```
For vendor-specific handling instructions.

---

## 18. Validation Requirements

### XML Well-Formedness

1. All elements must be properly nested
2. All elements must be closed
3. Attribute values must be quoted
4. Special characters must be escaped
5. Document must have single root element

### DTD Validation

1. Document must validate against RittDocBook.dtd
2. All required elements must be present
3. Element content must match DTD declarations
4. Attribute values must match allowed values

### Content Quality Checks

1. All IDs must be unique
2. All internal references (`<xref>`, `<link>`) must have valid targets
3. All image file references must exist
4. No empty paragraphs (unless intentional)
5. Proper section nesting hierarchy

---

## Appendix A: Complete Element Reference

### Block Elements

| Element | Description | Parent Elements |
|---------|-------------|-----------------|
| `<book>` | Root element | (document root) |
| `<bookinfo>` | Book metadata | `<book>` |
| `<part>` | Major division | `<book>` |
| `<partintro>` | Part introduction | `<part>` |
| `<chapter>` | Chapter | `<book>`, `<part>` |
| `<preface>` | Front matter section | `<book>` |
| `<dedication>` | Dedication | `<book>` |
| `<appendix>` | Appendix | `<book>`, `<part>` |
| `<bibliography>` | Bibliography | `<book>`, `<chapter>` |
| `<glossary>` | Glossary | `<book>` |
| `<index>` | Index | `<book>` |
| `<sect1>` - `<sect4>` | Sections | Various |
| `<para>` | Paragraph | Various |
| `<blockquote>` | Block quotation | Various |
| `<sidebar>` | Sidebar/box | Various |
| `<figure>` | Figure container | Various |
| `<table>` | Table container | Various |
| `<orderedlist>` | Numbered list | Various |
| `<itemizedlist>` | Bulleted list | Various |
| `<literallayout>` | Preformatted text | Various |
| `<epigraph>` | Epigraph | Various |
| `<tip>`, `<note>`, `<warning>`, `<caution>` | Admonitions | Various |

### Inline Elements

| Element | Description |
|---------|-------------|
| `<emphasis>` | Emphasized text (italic) |
| `<emphasis role="strong">` | Strong emphasis (bold) |
| `<emphasis role="smallcaps">` | Small caps |
| `<superscript>` | Superscript |
| `<subscript>` | Subscript |
| `<literal>` | Literal/code text |
| `<ulink>` | External URL link |
| `<xref>` | Internal cross-reference |
| `<link>` | Internal link with custom text |
| `<footnote>` | Inline footnote |

---

## Appendix B: Sample Conversion Workflow

### 1. Parse EPUB

```
1. Extract EPUB archive
2. Parse container.xml to find OPF file
3. Parse OPF to get spine order and manifest
4. Extract metadata from OPF
5. Read each XHTML file in spine order
```

### 2. Analyze Structure

```
1. Identify document structure from CSS classes
2. Build section hierarchy
3. Map chapters and parts
4. Identify special content (figures, tables, sidebars)
5. Collect bibliography entries
```

### 3. Transform Content

```
1. Create book.xml with metadata
2. Create entity declarations
3. For each chapter:
   a. Create chapter file
   b. Transform headings to sections
   c. Transform paragraphs
   d. Handle inline formatting
   e. Process figures and tables
   f. Handle cross-references
4. Process back matter (bibliography, index)
```

### 4. Post-Processing

```
1. Generate unique IDs
2. Resolve cross-references
3. Copy and convert images
4. Validate against DTD
5. Create ZIP package
```

---

## Appendix C: Common CSS Class Patterns by Publisher

### Pattern Set A (Academic/Professional)

| Class | Meaning |
|-------|---------|
| `cn`, `ct` | Chapter number, Chapter title |
| `ah`, `bh`, `ch`, `dh` | Heading levels A-D |
| `p`, `paft`, `pcon` | Paragraphs |
| `nl`, `bl` | Numbered/Bulleted lists |
| `fig`, `tab` | Figures, Tables |

### Pattern Set B (Trade/Consumer)

| Class | Meaning |
|-------|---------|
| `chapter-number`, `chapter-title` | Chapter headings |
| `section-head`, `subsection-head` | Section headings |
| `body-text`, `first-para` | Paragraphs |
| `numbered-list`, `bullet-list` | Lists |

### Pattern Set C (Alternative)

| Class | Meaning |
|-------|---------|
| `ChapNum`, `ChapTitle` | Chapter headings |
| `HeadA`, `HeadB`, `HeadC` | Section headings |
| `BodyText`, `BodyFirst` | Paragraphs |

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024 | Initial specification |

---

*This document provides comprehensive rules for converting EPUB files to RittDoc XML format. For questions or updates, contact the conversion team.*
