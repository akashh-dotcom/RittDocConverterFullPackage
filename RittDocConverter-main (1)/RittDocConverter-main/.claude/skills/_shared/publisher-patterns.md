# Publisher XHTML Source Patterns

Multi-publisher reference guide for EPUB/XHTML markup patterns encountered during R2 Digital Library book conversions.

---

## How to Use This Reference

This is a **multi-publisher guide** covering EPUB source patterns and their expected converted output across all publishers processed through the RittDoc conversion pipeline.

**Workflow:**
1. **Identify the publisher** first -- check the title page metadata on the R2 platform, or use `epub_metadata.py`'s `detect_publisher()` function.
2. **Reference the generic section** (Section 1) for patterns common to all EPUB sources.
3. **Reference the publisher-specific section** (Sections 3+) for known markup quirks, CSS class names, and file naming conventions unique to that publisher.
4. **Reference the output section** (Section 2) for the expected DocBook/RittDoc converted output structure.

Publishers with detailed pattern documentation: **Springer Nature** (Section 3).
Publishers with partial documentation: **Wiley** (Section 4).
Publishers with placeholders: **Elsevier** (Section 5), others (Section 6).

---

## 1. Generic EPUB/XHTML Patterns (All Publishers)

Common patterns found across most EPUB sources regardless of publisher.

### Standard EPUB Structure

```
book.epub (ZIP archive)
  META-INF/
    container.xml              -> Points to content.opf
  OEBPS/ (or OPS/, or root)
    content.opf                -> Package manifest, metadata, spine
    toc.ncx                    -> NCX table of contents (EPUB 2)
    toc.xhtml                  -> XHTML table of contents (EPUB 3)
    html/ (or xhtml/, text/)
      cover.xhtml              -> Cover image page
      chapter01.xhtml          -> Body content
      ...
    images/ (or img/, media/)
      cover.jpg                -> Cover image
      fig01.png                -> Inline figures
      ...
    styles/ (or css/)
      stylesheet.css           -> Publisher stylesheet
```

**Note**: Directory names vary by publisher. The OPF manifest is the authoritative source for locating all content files.

### Common HTML Elements

| Element | Usage | Notes |
|---------|-------|-------|
| `<h1>` - `<h6>` | Section headings | Nesting depth varies by publisher |
| `<p>` | Body paragraphs | Often has publisher-specific CSS classes |
| `<ol>`, `<ul>` | Ordered/unordered lists | May be nested |
| `<dl>`, `<dt>`, `<dd>` | Definition lists | Used for glossaries, key-value pairs |
| `<table>` | Tabular data | May or may not include `<caption>` |
| `<figure>` / `<div>` | Figure containers | EPUB 3 uses `<figure>`, older EPUBs use `<div>` |
| `<figcaption>` / `<div>` | Figure captions | EPUB 3 uses `<figcaption>`, older use CSS-classed `<div>` |
| `<a>` | Hyperlinks / cross-refs | Internal anchors (`#id`) and external URLs |
| `<img>` | Images | `src` paths are relative to the XHTML file |
| `<blockquote>` | Block quotations | Sometimes used for callout boxes |
| `<aside>` | Sidebars / notes | EPUB 3 semantic element |
| `<sup>`, `<sub>` | Superscript/subscript | Common in scientific/medical texts |

### epub:type Attributes (EPUB 3)

Semantic inflection attributes used in EPUB 3 for structural identification:

```html
<body epub:type="bodymatter">
<section epub:type="chapter">
<section epub:type="bibliography">
<section epub:type="index">
<nav epub:type="toc">
<section epub:type="cover">
<section epub:type="titlepage">
<section epub:type="copyright-page">
<section epub:type="foreword">
<section epub:type="preface">
<section epub:type="introduction">
<section epub:type="appendix">
<section epub:type="glossary">
<section epub:type="loi">  <!-- list of illustrations -->
<section epub:type="lot">  <!-- list of tables -->
<aside epub:type="sidebar">
<aside epub:type="footnote">
<a epub:type="noteref">     <!-- footnote reference -->
<a epub:type="biblioref">   <!-- bibliography reference -->
```

### Common Encoding Issues

| Issue | Character | Unicode | Symptom |
|-------|-----------|---------|---------|
| UTF-8 BOM | `\uFEFF` | U+FEFF | Invisible bytes at file start; can cause XML parse errors |
| Non-breaking space | `\u00A0` | U+00A0 | If double-encoded: visible "A" characters appear |
| Zero-width space | `\u200B` | U+200B | If converted to regular space: words split incorrectly |
| Smart left quote | `\u2018` / `\u201C` | U+2018/201C | May be lost or converted to straight quotes |
| Smart right quote | `\u2019` / `\u201D` | U+2019/201D | May be lost or converted to straight quotes |
| Em dash | `\u2014` | U+2014 | May be converted to hyphen or lost |
| En dash | `\u2013` | U+2013 | May be converted to hyphen or lost |
| Ellipsis | `\u2026` | U+2026 | May be converted to three periods |
| Soft hyphen | `\u00AD` | U+00AD | Invisible; may cause unexpected line breaks |

### Image Patterns

**Cover images:**
```html
<!-- Typical cover page -->
<div class="cover">
  <img src="../images/cover.jpg" alt="Cover" />
</div>
```

**Inline figures with captions:**
```html
<!-- EPUB 3 style -->
<figure id="fig1">
  <img src="../images/fig01.png" alt="Description" />
  <figcaption>Figure 1.1 Description of the figure</figcaption>
</figure>

<!-- EPUB 2 / publisher-specific style -->
<div class="figure" id="fig1">
  <img src="../images/fig01.png" alt="Description" />
  <div class="caption">Figure 1.1 Description of the figure</div>
</div>
```

### Table Patterns

**Formal table (with caption):**
```html
<table>
  <caption>Table 1.1 Summary of Results</caption>
  <thead>
    <tr><th>Parameter</th><th>Value</th></tr>
  </thead>
  <tbody>
    <tr><td>Score</td><td>85</td></tr>
  </tbody>
</table>
```

**Layout/informal table (no caption):**
```html
<table>
  <tbody>
    <tr><td>Key term</td><td>Definition text</td></tr>
  </tbody>
</table>
```

**Key distinction**: Formal tables have captions and should be numbered in output. Layout tables lack captions and should NOT receive table numbers.

### Bibliography Patterns

```html
<!-- Numbered reference list -->
<section epub:type="bibliography">
  <h2>References</h2>
  <ol>
    <li id="ref1">
      <p>Author A, Author B. Title of Article.
        <i>Journal Name</i>. 2024;10(2):100-110.
        <a href="https://doi.org/10.1234/example">doi:10.1234/example</a>
      </p>
    </li>
  </ol>
</section>
```

**Common external reference links:**
- DOI links: `https://doi.org/10.xxxx/...`
- PubMed links: `http://www.ncbi.nlm.nih.gov/pubmed/NNNNNNN`
- CrossRef links: `https://doi.org/...` (with label "CrossRef")
- Google Scholar links: `https://scholar.google.com/...`

### Metadata Locations

**OPF metadata (content.opf):**
```xml
<metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
  <dc:title>Book Title</dc:title>
  <dc:creator>Author Name</dc:creator>
  <dc:publisher>Publisher Name</dc:publisher>
  <dc:identifier id="isbn">urn:isbn:978-0-123456-78-9</dc:identifier>
  <dc:language>en</dc:language>
  <dc:date>2024-01-01</dc:date>
  <dc:rights>Copyright text</dc:rights>
  <meta property="dcterms:modified">2024-01-01T00:00:00Z</meta>
</metadata>
```

**In-book metadata** is publisher-specific -- see individual publisher sections for CSS class names used to mark up titles, authors, ISBNs, DOIs, and copyright text within XHTML content.

---

## 2. Converted XML Output Patterns (DocBook/RittDoc)

Standard structure of the converted output files.

### File Naming Convention

```
{ISBN}/
  book.{ISBN}.xml                       -> Root book element
  toc.{ISBN}.xml                        -> Table of contents
  preface.{ISBN}.pr0001.xml             -> Preface
  preface.{ISBN}.pr0002.xml             -> Second preface (if any)
  sect1.{ISBN}.ch0001s0001.xml          -> Chapter 1, Section 1
  sect1.{ISBN}.ch0001s0002.xml          -> Chapter 1, Section 2
  sect1.{ISBN}.ch0002s0001.xml          -> Chapter 2, Section 1
  appendix.{ISBN}.ap0001.xml            -> Appendix A
  part.{ISBN}.pt0001.xml                -> Part I (if book has parts)
  ...
```

### Section ID Patterns

| Pattern | Meaning | Example |
|---------|---------|---------|
| `pr####` | Preface section | `pr0001` |
| `ch####s####` | Chapter N, Section M | `ch0001s0001` |
| `ch####s####s##` | Chapter N, Section M, Sub-section K | `ch0001s0001s01` |
| `ap####` | Appendix | `ap0001` |
| `pt####` | Part | `pt0001` |

### Key DocBook Elements

```xml
<!-- Root structure -->
<book>
  <bookinfo>...</bookinfo>
  <preface id="pr0001">...</preface>
  <chapter id="ch0001">
    <sect1 id="ch0001s0001">
      <title>Section Title</title>
      <sect2 id="ch0001s0001s01">
        <title>Sub-section Title</title>
        <para>Body text...</para>
      </sect2>
    </sect1>
  </chapter>
  <appendix id="ap0001">...</appendix>
</book>
```

### Common DocBook Body Elements

| Element | Usage |
|---------|-------|
| `<para>` | Body paragraph |
| `<emphasis>` | Inline emphasis (italic by default) |
| `<emphasis role="bold">` | Bold text |
| `<emphasis role="HeadingNumber">` | Section number prefix (e.g., "1.1") |
| `<table>` | Formal table (CALS model) |
| `<informaltable>` | Informal table (no title) |
| `<figure>` | Formal figure with title |
| `<informalfigure>` | Informal figure (no title) |
| `<mediaobject>` | Image container within a figure |
| `<imageobject>` | Image reference within mediaobject |
| `<imagedata>` | Image file path attribute |
| `<bibliomixed>` | Single bibliography entry |
| `<bibliography>` | Bibliography section container |
| `<sidebar>` | Sidebar/callout box |
| `<tip>` | Tip admonition |
| `<note>` | Note admonition |
| `<important>` | Important admonition |
| `<caution>` | Caution admonition |
| `<warning>` | Warning admonition |
| `<bridgehead>` | Free-floating heading (not in section hierarchy) |
| `<itemizedlist>` | Bulleted list |
| `<orderedlist>` | Numbered list |
| `<variablelist>` | Definition list |
| `<blockquote>` | Block quotation |
| `<programlisting>` | Code block |
| `<superscript>` | Superscript text |
| `<subscript>` | Subscript text |
| `<footnote>` | Footnote |

### Role Attributes

```xml
<emphasis role="bold">Bold text</emphasis>
<emphasis role="italic">Italic text</emphasis>
<emphasis role="HeadingNumber">1.1 </emphasis>
<para role="ChapterCopyright">Copyright text...</para>
<para role="ChapterSubTitle">Subtitle text</para>
```

### R2 Custom Elements

Elements specific to the RittDoc/R2 platform output:

| Element | Purpose |
|---------|---------|
| `<risindex>` | Index entry marker |
| `<risterm>` | Index term text |
| `<ristopic>` | Index topic reference |
| `<ristype>` | Index entry type classification |
| `<risrule>` | Horizontal rule / separator |
| `<risposid>` | Position identifier for content linking |
| `<risinfo>` | Supplementary metadata block |
| `<chapterid>` | Chapter identifier for platform linking |
| `<chaptertitle>` | Chapter title for platform display |

### URL Patterns on Platform

```
Title page:    /Resource/Title/{ISBN}
Section view:  /resource/detail/{ISBN}/{sectionId}

Examples:
  /Resource/Title/9780123456789
  /resource/detail/9780123456789/ch0001s0001
  /resource/detail/9780123456789/pr0001
```

---

## 3. Springer Nature

Detailed patterns for Springer Nature EPUB sources. Springer has the most well-documented patterns due to high volume of conversions.

### File Structure

Springer Nature EPUBs typically contain:

```
OEBPS/
  html/
    Cover.xhtml                         -> Cover image page
    Frontmatter_001.xhtml               -> Title page / half-title
    Frontmatter_002.xhtml               -> Copyright page
    657658_1_En_BookFrontmatter_OnlinePDF.xhtml  -> Full front matter
    657658_1_En_1_Chapter.xhtml         -> Chapter 1
    657658_1_En_2_Chapter.xhtml         -> Chapter 2
    ...
    657658_1_En_BookBackmatter_OnlinePDF.xhtml   -> Back matter / index
  images/
    MediaObjects/
      657658_1_En_1_Fig1_HTML.png       -> Chapter 1, Figure 1
      657658_1_En_1_Figa_HTML.png       -> Chapter 1, informal figure
      ...
  styles/
    stylesheet.css
  toc.ncx                              -> NCX table of contents
  content.opf                           -> OPF package manifest
```

### File Naming Convention

- `657658_1_En` = internal Springer book identifier
- `_1_` = Chapter number
- `_Chapter.xhtml` = Chapter body content
- `_BookFrontmatter_OnlinePDF.xhtml` = Compiled front matter
- `_BookBackmatter_OnlinePDF.xhtml` = Compiled back matter / index

### Section Headings

#### HeadingNumber Spans

```html
<h2 class="Heading">
  <span class="HeadingNumber">1.1 </span>When to Suspect Acute Coronary Syndrome
</h2>
```

**Key detail**: The trailing space is INSIDE the `<span>` element after the number. This space is frequently lost during conversion to:
```xml
<emphasis role="HeadingNumber">1.1</emphasis>When to Suspect...
```

#### Sub-section Headings

```html
<h3 class="Heading">
  <span class="HeadingNumber">1.1.1 </span>Clinical Presentation
</h3>
```

Same trailing-space pattern applies at all heading levels.

### Table of Contents

#### In-chapter TOC (ArticleOrChapterToc)

```html
<nav class="ArticleOrChapterToc">
  <ol class="defined">
    <li>
      <a href="#Sec1">
        <span class="HeadingNumber">1.1</span>
        <span>When to Suspect Acute Coronary Syndrome</span>
      </a>
    </li>
    <li>
      <a href="#Sec7">
        <span class="HeadingNumber">1.2</span>
        <span>Diagnostic Approach [, ]</span>  <!-- Empty brackets = stripped CitationRefs -->
      </a>
    </li>
  </ol>
</nav>
```

**Known issue**: Springer's own TOC navigation strips `CitationRef` content but leaves the surrounding bracket punctuation behind. This produces empty brackets like `[]`, `[,]`, `[,.]` in TOC entries. This is a **publisher source issue**, not a converter bug.

#### Body Heading with CitationRef (resolved)

```html
<h2 class="Heading">
  <span class="HeadingNumber">1.2 </span>Diagnostic Approach
  <span class="CitationRef">
    [<a class="CitationRef" epub:type="biblioref" href="#CR4">4</a>,
     <a class="CitationRef" epub:type="biblioref" href="#CR5">5</a>]
  </span>
</h2>
```

In the body, citation references ARE resolved with link text. Only the TOC copy has them stripped.

### Callout Boxes / Admonitions

#### FormalPara with RenderingStyle

```html
<div class="FormalPara FormalParaRenderingStyle3 ParaTypeTip" id="FPar1">
  <p class="FormalParaTitle">
    <b>Hospitalist Tip</b>
  </p>
  <p class="Para">
    Always check troponin levels at 0 and 3 hours...
  </p>
</div>
```

#### Common ParaType Classes

| Class | Display Name | Expected DocBook Mapping |
|-------|-------------|------------------------|
| `ParaTypeTip` | Hospitalist Tip | `<tip>` or `<sidebar>` |
| `ParaTypeOverview` | Pearl | `<note>` or `<sidebar>` |
| `ParaTypeImportant` | Important | `<important>` |
| `ParaTypeNote` | Note | `<note>` |
| `ParaTypeCaution` | Caution | `<caution>` |
| `ParaTypeWarning` | Warning | `<warning>` |

#### RenderingStyle Classes

| Class | Meaning |
|-------|---------|
| `FormalParaRenderingStyle1` | Standard box |
| `FormalParaRenderingStyle2` | Highlighted box |
| `FormalParaRenderingStyle3` | Bordered/accented box |

**Conversion issue**: These are currently converted to bare `<para>Title</para>` followed by content, losing all callout/admonition styling. Should map to DocBook `<tip>`, `<note>`, `<important>`, `<sidebar>`, etc.

### Tables

#### Formal Tables (with caption)

```html
<div class="Table" id="Tab1">
  <table class="Table">
    <caption>
      <span class="CaptionNumber">Table 1.1</span>
      <span class="CaptionContent">Risk Stratification Criteria</span>
    </caption>
    <thead>
      <tr><th>Parameter</th><th>Low Risk</th><th>High Risk</th></tr>
    </thead>
    <tbody>
      <tr><td>Troponin</td><td>Normal</td><td>Elevated</td></tr>
    </tbody>
  </table>
</div>
```

**Key**: Formal tables have `id="Tab1"`, `id="Tab2"`, etc. (numeric suffix) and include `<caption>` with chapter-prefixed numbering like "Table 1.1".

#### Informal Tables (no caption, layout purposes)

```html
<div class="Table" id="Taba">
  <table class="Table">
    <tbody>
      <tr><td class="SimplePara">Hospitalist Tip</td></tr>
      <tr><td class="SimplePara">Always verify medication...</td></tr>
    </tbody>
  </table>
</div>
```

**Key**: Informal tables have `id="Taba"`, `id="Tabb"`, etc. (alphabetic suffix) and have NO `<caption>`. They are used for layout purposes (e.g., callout box rendering) and should NOT receive table numbers in the converted output.

**Conversion issue**: Both formal and informal tables get flat sequential numbering (Table 1, Table 2, etc.) instead of:
- Formal tables: Chapter-prefixed numbering (Table 1.1) with captions preserved
- Informal tables: No numbering at all

### Copyright / Metadata Elements

#### Copyright Block

```html
<div class="ChapterCopyright">
  <div class="ChapterCopyrightText">
    <span class="CopyrightHolderName">M. Salih</span>
    ...
  </div>
</div>
```

#### Author/Editor Names

```html
<span class="ContextInformationAuthorEditorNames">Mohammed Salih</span>
```

#### Book Title Reference

```html
<span class="BookTitle">Practical Hospitalist Guide to Inpatient Cardiology</span>
```

#### DOI

```html
<span class="ChapterDOI">
  <a href="https://doi.org/10.1007/978-3-032-03240-9_1">
    https://doi.org/10.1007/978-3-032-03240-9_1
  </a>
</span>
```

#### ISBN Fields (Copyright Page)

```html
<span class="CopyrightPagePrintISBN">Print ISBN: 978-3-032-03239-3</span>
<span class="CopyrightPageElectronicISBN">Electronic ISBN: 978-3-032-03240-9</span>
```

**Conversion issue**: These separate elements get flattened into a single `<para role="ChapterCopyright">` without whitespace separators, producing concatenated text like:
- `2025M. Salih` (year + author name)
- `978-3-032-03239-3978-3-032-03240-9` (print ISBN + electronic ISBN)

### Bibliography / References

#### BibliographyWrapper

```html
<div class="BibliographyWrapper">
  <ol class="BibliographyList">
    <li class="Citation" id="CR1">
      <span class="CitationNumber">1.</span>
      <p class="CitationContent">
        Amsterdam EA, Wenger NK, Brindis RG, et al. 2014 AHA/ACC guideline...
        <i>Circulation</i>. 2014;130(25):e199-e267.
        <span class="ExternalRef">
          <a class="ExternalRef" href="https://doi.org/10.1161/CIR.0000000000000134">
            <span class="RefSource">https://doi.org/10.1161/CIR.0000000000000134</span>
          </a>
        </span>
      </p>
    </li>
  </ol>
</div>
```

#### PubMed / CrossRef Links

```html
<span class="ExternalRef">
  <a class="ExternalRef" href="http://www.ncbi.nlm.nih.gov/pubmed/25085961">
    <span class="RefSource">PubMed</span>
  </a>
</span>
<span class="ExternalRef">
  <a class="ExternalRef" href="https://doi.org/10.1161/CIR.0000000000000134">
    <span class="RefSource">CrossRef</span>
  </a>
</span>
```

**Conversion issue**: Some PubMed/CrossRef link labels lost during conversion. Check by counting `PubMed` and `CrossRef` labels in both source and converted files.

### Zero-Width Spaces

Springer uses zero-width space characters (`\u200b`, Unicode U+200B) as line-break hints inside compound words and URLs:

```
Stat\u200bPearls    -> Should render as "StatPearls"
doi\u200b.org       -> Should render as "doi.org"
```

**Conversion issue**: If the zero-width space is converted to a regular space, it produces:
- `Stat Pearls` instead of `StatPearls`
- `doi. org` instead of `doi.org`

### Images / Figures

#### Formal Figures

```html
<div class="Figure" id="Fig1">
  <div class="MediaObject">
    <img src="../images/MediaObjects/657658_1_En_1_Fig1_HTML.png"
         alt="Description of figure" />
  </div>
  <div class="FigureCaption">
    <span class="CaptionNumber">Fig. 1.1</span>
    <span class="CaptionContent">ECG showing ST elevation...</span>
  </div>
</div>
```

#### Informal Figures

```html
<div class="Figure" id="Figa">
  <div class="MediaObject">
    <img src="../images/MediaObjects/657658_1_En_1_Figa_HTML.png" />
  </div>
  <!-- No FigureCaption -->
</div>
```

Same formal/informal pattern as tables: `Fig1` (numeric) = formal with caption, `Figa` (alphabetic) = informal without caption.

### Encoding Considerations

#### Non-Breaking Spaces (NBSP)

- Springer uses NBSP (`\u00A0`) between author first/last names and in other places
- If the conversion pipeline double-encodes UTF-8, NBSP bytes `C2 A0` get interpreted as Latin-1, producing visible "A" characters
- Example: `Mohammed\u00A0Salih` -> `MohammedA Salih`

#### Em/En Dashes

- Springer uses proper Unicode em-dash (`\u2014`) and en-dash (`\u2013`)
- Check these are preserved in conversion, not converted to hyphens or lost

### Springer-Specific CSS Classes Summary

| Class | Element Type | Purpose |
|-------|-------------|---------|
| `Heading` | `<h2>`, `<h3>`, etc. | Section heading |
| `HeadingNumber` | `<span>` | Section number prefix |
| `ArticleOrChapterToc` | `<nav>` | In-chapter table of contents |
| `CitationRef` | `<span>`, `<a>` | Bibliography citation reference |
| `FormalPara` | `<div>` | Callout box container |
| `FormalParaTitle` | `<p>` | Callout box title |
| `ParaTypeTip` | `<div>` (modifier) | Tip callout type |
| `ParaTypeOverview` | `<div>` (modifier) | Overview/pearl callout type |
| `ParaTypeImportant` | `<div>` (modifier) | Important callout type |
| `FormalParaRenderingStyle1-3` | `<div>` (modifier) | Visual rendering variant |
| `Table` | `<div>`, `<table>` | Table wrapper and element |
| `CaptionNumber` | `<span>` | Table/figure number |
| `CaptionContent` | `<span>` | Table/figure caption text |
| `SimplePara` | `<td>` | Simple paragraph in table cell |
| `ChapterCopyright` | `<div>` | Copyright block |
| `ChapterCopyrightText` | `<div>` | Copyright text container |
| `CopyrightHolderName` | `<span>` | Copyright holder |
| `ContextInformationAuthorEditorNames` | `<span>` | Author/editor names |
| `BookTitle` | `<span>` | Book title reference |
| `ChapterDOI` | `<span>` | Chapter DOI link |
| `CopyrightPagePrintISBN` | `<span>` | Print ISBN |
| `CopyrightPageElectronicISBN` | `<span>` | Electronic ISBN |
| `BibliographyWrapper` | `<div>` | Bibliography section |
| `BibliographyList` | `<ol>` | Reference list |
| `Citation` | `<li>` | Single reference entry |
| `CitationNumber` | `<span>` | Reference number |
| `CitationContent` | `<p>` | Reference text |
| `ExternalRef` | `<span>`, `<a>` | External link (DOI, PubMed, CrossRef) |
| `RefSource` | `<span>` | External link label/URL |
| `Figure` | `<div>` | Figure wrapper |
| `MediaObject` | `<div>` | Image container |
| `FigureCaption` | `<div>` | Figure caption |
| `Para` | `<p>` | Body paragraph |

---

## 4. Wiley

Patterns for Wiley (John Wiley & Sons) EPUB sources. Wiley EPUBs vary somewhat by book series and imprint (Wiley-Blackwell, Wiley-VCH, Jossey-Bass).

**Status**: Expanding -- add patterns as Wiley books are scanned.

### File Structure

```
OEBPS/
  xhtml/ (or text/)
    cover.xhtml                -> Cover page
    toc.xhtml                  -> Table of contents
    title.xhtml                -> Title page
    copyright.xhtml            -> Copyright page
    ch01.xhtml                 -> Chapter 1
    ch02.xhtml                 -> Chapter 2
    ...
    bm01.xhtml                 -> Back matter (bibliography, index)
    app01.xhtml                -> Appendix A
  images/
    cover.jpg
    f01-01.jpg                 -> Chapter 1, Figure 1
    ...
  styles/
    wiley.css
  content.opf
  toc.ncx
```

### File Naming Convention

- Chapters: `ch01.xhtml`, `ch02.xhtml`, etc.
- Appendices: `app01.xhtml`, `app02.xhtml`, etc.
- Back matter: `bm01.xhtml`, `bm02.xhtml`, etc.
- Front matter: `fm01.xhtml` or named pages (`title.xhtml`, `copyright.xhtml`)
- Figures: `f01-01.jpg` (chapter-figure numbering) or descriptive names

### Heading Patterns

Wiley tends to use `epub:type` attributes for semantic markup rather than relying exclusively on CSS classes:

```html
<section epub:type="chapter" id="c01">
  <h1 class="chapter-title">Chapter Title</h1>
  <section id="c01-sec-0001">
    <h2>Section Heading</h2>
  </section>
</section>
```

Section IDs often follow the pattern `c01-sec-0001` (chapter-section numbering).

### Table Patterns

```html
<div class="table" id="c01-tbl-0001">
  <table>
    <caption>
      <p><b>Table 1.1</b> Description of the table</p>
    </caption>
    <thead>
      <tr><th>Header</th></tr>
    </thead>
    <tbody>
      <tr><td>Data</td></tr>
    </tbody>
  </table>
</div>
```

Table IDs typically follow the pattern `c01-tbl-0001`.

### Bibliography

```html
<section epub:type="bibliography" id="c01-bibl-0001">
  <h2>References</h2>
  <ol>
    <li id="c01-bib-0001">
      <cite>Author A. Title. <i>Journal</i>. 2024;10:100-110.</cite>
    </li>
  </ol>
</section>
```

Wiley uses `<cite>` elements within bibliography entries and `epub:type="bibliography"` on the containing section.

### Wiley-Specific CSS Classes

Wiley CSS class names vary by book series and production workflow. Common classes include:

| Class | Purpose |
|-------|---------|
| `chapter-title` | Chapter heading |
| `section-title` | Section heading |
| `table` | Table wrapper |
| `figure` | Figure wrapper |
| `note` | Note/callout box |
| `sidebar` | Sidebar content |

**Note**: Wiley class names are less standardized than Springer's. Always inspect the actual EPUB stylesheet for the specific book being processed.

### Callout Boxes / Admonitions

Wiley uses `epub:type` and class-based callout containers:

```html
<aside epub:type="sidebar" class="sidebar">
  <h3 class="sidebar-title">Key Point</h3>
  <p>Important information for clinical practice...</p>
</aside>

<div class="note">
  <p class="note-title">Note</p>
  <p>Additional detail or clarification...</p>
</div>

<div class="boxed-text" id="c01-fea-0001">
  <h3>Clinical Pearl</h3>
  <p>Pearl content...</p>
</div>
```

Common Wiley callout classes: `sidebar`, `note`, `boxed-text`, `case-study`, `learning-objectives`, `summary`

### Copyright / Metadata

```html
<section epub:type="copyright-page">
  <p class="copy">Copyright © 2024 John Wiley &amp; Sons, Ltd</p>
  <p class="copy">ISBN: 978-1-XXXX-XXXX-X (hardback)</p>
  <p class="copy">ISBN: 978-1-XXXX-XXXX-X (ePDF)</p>
  <p class="copy">ISBN: 978-1-XXXX-XXXX-X (ePub)</p>
</section>
```

### Figure Patterns

```html
<div class="figure" id="c01-fig-0001">
  <img src="../images/f01-01.jpg" alt="Description" />
  <p class="caption"><b>Figure 1.1</b> Description of the figure</p>
</div>
```

Figure IDs follow `c01-fig-0001` (chapter-figure-number). Caption is typically in a `<p class="caption">` with bold figure number.

### Footnotes

```html
<aside epub:type="footnote" id="c01-note-0001">
  <p>Footnote text explaining the referenced content.</p>
</aside>
<!-- Reference in text: -->
<a epub:type="noteref" href="#c01-note-0001"><sup>1</sup></a>
```

### Cross-References

```html
<!-- Internal section reference -->
<a href="ch03.xhtml#c03-sec-0002">see Section 3.2</a>
<!-- Figure reference -->
<a href="#c01-fig-0001">Figure 1.1</a>
<!-- Table reference -->
<a href="#c01-tbl-0001">Table 1.1</a>
```

### Known Wiley Patterns To Watch For

- Section numbering may be embedded directly in heading text rather than in a separate `<span>`
- Footnotes often use `epub:type="footnote"` with `<aside>` elements
- Cross-references use `epub:type="noteref"` links
- Some Wiley books use `data-*` attributes for additional semantic information
- Wiley-Blackwell medical books may have different class names from Wiley engineering books
- Some series use `<cite>` elements for bibliography entries, others use `<p>` with class `bib-entry`
- Learning objectives sections (`class="learning-objectives"`) should map to sidebar/note
- Case study boxes (`class="case-study"`) should map to sidebar
- Index terms may use `data-indexterm` attributes on `<span>` elements

---

## 5. Elsevier

Patterns for Elsevier EPUB sources. Elsevier uses an XML-first production workflow (often JATS-influenced), producing consistently structured EPUBs.

**Status**: Expanding -- add patterns as Elsevier books are scanned.

**Key Imprints** (patterns may vary by imprint):
- **Academic Press** -- science and technology
- **Butterworth-Heinemann** -- engineering
- **Morgan Kaufmann** -- computing and information technology
- **Saunders** -- medical and health sciences
- **Mosby** -- nursing and health professions
- **Churchill Livingstone** -- medical

### File Structure

```
OEBPS/
  xhtml/
    cover.xhtml
    front-matter.xhtml (or fm01.xhtml)
    copyright.xhtml
    B978XXXXXXXXXXXXX_00001.xhtml    -> Chapter 1 (ISBN-based naming)
    B978XXXXXXXXXXXXX_00002.xhtml    -> Chapter 2
    ...
    B978XXXXXXXXXXXXX_00099.xhtml    -> Back matter
  images/
    cover.jpg
    f01-01-BXXXXXXXXXX.jpg           -> Chapter 1, Figure 1
    t01-01-BXXXXXXXXXX.jpg           -> Chapter 1, Table 1 (image tables)
    ...
  styles/
    stylesheet.css
  content.opf
  toc.ncx
```

### File Naming Convention

Elsevier uses ISBN-based file naming: `B978{ISBN13_digits}_{NNNNN}.xhtml`

Alternative patterns in some imprints:
- `chapter-1.xhtml`, `chapter-2.xhtml` (descriptive naming)
- `c01.xhtml`, `c02.xhtml` (abbreviated)

### Heading Patterns

```html
<h1 class="chapter-title" id="s0005">
  <span class="chapter-number">Chapter 1</span>
  Introduction to Cardiology
</h1>
<h2 id="s0010">History and Physical Examination</h2>
<h3 id="s0015">Cardiac Auscultation</h3>
```

**Key features**:
- Section IDs use `s0005`, `s0010`, etc. (sequential, padded)
- Chapter number may be in a `<span class="chapter-number">` or embedded in text
- Sub-headings often lack explicit class names (identified by `<h2>`, `<h3>` level)

### Callout Boxes / Clinical Features

```html
<div class="textbox" id="b0010">
  <div class="textbox-head">
    <p class="textbox-title">Key Concept</p>
  </div>
  <div class="textbox-body">
    <p>Important concept text...</p>
  </div>
</div>

<div class="clinicalbox">
  <p class="boxtitle">Clinical Feature</p>
  <p>Clinical observation text...</p>
</div>
```

Common callout classes: `textbox`, `clinicalbox`, `keypoints`, `learning-objectives`, `case-study`, `practice-point`

### Table Patterns

```html
<div class="table" id="t0010">
  <p class="table-title">
    <span class="label">Table 1.1</span> Summary of findings
  </p>
  <table>
    <thead><tr><th>Parameter</th><th>Value</th></tr></thead>
    <tbody><tr><td>Heart rate</td><td>72 bpm</td></tr></tbody>
  </table>
  <div class="table-footnote">
    <p><sup>a</sup> Measured at rest.</p>
  </div>
</div>
```

**Key features**: Table IDs use `t0010`, `t0020`, etc. Table footnotes are common and appear in `<div class="table-footnote">` after the `<table>` element.

### Figure Patterns

```html
<div class="figure" id="f0010">
  <img src="../images/f01-01-BXXXXXXXXXX.jpg" alt="Description" />
  <p class="figure-title">
    <span class="label">Fig. 1.1</span> Description of figure
  </p>
</div>
```

### Bibliography / References

```html
<section class="bibliography" id="s0100">
  <h2>References</h2>
  <p class="bib-entry" id="bib1">
    <span class="bib-label">1.</span>
    Author A, Author B. Article title.
    <i>Journal</i>. 2024;10:100-110.
    <a href="https://doi.org/10.xxxx/example">doi:10.xxxx/example</a>
  </p>
  <p class="bib-entry" id="bib2">
    <span class="bib-label">2.</span>
    ...
  </p>
</section>
```

**Key difference from Springer**: Elsevier uses `<p class="bib-entry">` instead of `<li class="Citation">` in an `<ol>`. References are paragraphs, not list items.

### Cross-References

```html
<!-- Figure reference -->
<a class="fig-ref" href="#f0010">Fig. 1.1</a>
<!-- Table reference -->
<a class="tbl-ref" href="#t0010">Table 1.1</a>
<!-- Section reference -->
<a class="sec-ref" href="#s0015">see p. XX</a>
<!-- Citation reference -->
<a class="bib-ref" href="#bib1">[1]</a>
```

Elsevier typically uses descriptive class names for cross-references (`fig-ref`, `tbl-ref`, `sec-ref`, `bib-ref`), making them easier to identify programmatically than some other publishers.

### Copyright / Metadata

```html
<div class="copyright">
  <p>Copyright © 2024 Elsevier Inc. All rights reserved.</p>
  <p>ISBN: 978-0-XXXX-XXXX-X</p>
  <p>ISBN: 978-0-XXXX-XXXX-X (ebook)</p>
  <p class="imprint">
    Imprint: Saunders, an imprint of Elsevier Inc.
  </p>
</div>
```

### Elsevier-Specific CSS Classes Summary

| Class | Element Type | Purpose |
|-------|-------------|---------|
| `chapter-title` | `<h1>` | Chapter heading |
| `chapter-number` | `<span>` | Chapter number label |
| `textbox` | `<div>` | Callout box container |
| `textbox-title` | `<p>` | Callout box title |
| `clinicalbox` | `<div>` | Clinical feature box |
| `table` | `<div>` | Table wrapper |
| `table-title` | `<p>` | Table caption |
| `table-footnote` | `<div>` | Table footnote container |
| `figure` | `<div>` | Figure wrapper |
| `figure-title` | `<p>` | Figure caption |
| `label` | `<span>` | Number label |
| `bibliography` | `<section>` | References section |
| `bib-entry` | `<p>` | Single reference |
| `bib-label` | `<span>` | Reference number |
| `fig-ref` | `<a>` | Figure cross-reference |
| `tbl-ref` | `<a>` | Table cross-reference |
| `sec-ref` | `<a>` | Section cross-reference |
| `bib-ref` | `<a>` | Citation cross-reference |
| `copyright` | `<div>` | Copyright block |

### Known Elsevier Patterns To Watch For

- MathML may be embedded directly in XHTML for mathematical formulas
- Some Saunders/Mosby books use `<span class="small-caps">` for author names in references
- Table footnotes (`<div class="table-footnote">`) must be preserved in conversion
- Cross-reference links use descriptive class names (`fig-ref`, `tbl-ref`) that aid detection
- Some Elsevier books include "Further Reading" sections as separate `<section>` elements after references
- Image-based tables (where the table is rendered as a JPG/PNG image) are common in older imprints

---

## 5b. Thieme Medical Publishers

Patterns for Thieme Medical Publishers EPUB sources. Thieme is a major medical/scientific publisher with a distinct production workflow. Key imprints include Thieme (medical textbooks), Thieme Connect (online platform), and Georg Thieme Verlag (German-language).

**Status**: Expanding -- add patterns as Thieme books are scanned.

### File Structure

```
OEBPS/
  Text/ (or xhtml/)
    cover.xhtml
    titlepage.xhtml
    copyright.xhtml
    toc.xhtml
    preface.xhtml
    chapter01.xhtml (or c01.xhtml)
    chapter02.xhtml
    ...
    bibliography.xhtml
    index.xhtml
  Images/
    cover.jpg
    ch01_fig001.jpg          -> Chapter 1, Figure 1
    ch01_tbl001.jpg          -> Chapter 1, Table 1 (some tables as images)
    ...
  Styles/
    thieme.css (or stylesheet.css)
  content.opf
  toc.ncx
```

### File Naming Convention

- Chapters: `chapter01.xhtml` or `c01.xhtml` (varies by book)
- Appendices: `appendix01.xhtml` or `app01.xhtml`
- Front matter: Named pages (`titlepage.xhtml`, `copyright.xhtml`, `preface.xhtml`)
- Figures: `ch01_fig001.jpg` or descriptive filenames

### Heading Patterns

Thieme typically uses class-based heading structure:

```html
<h1 class="chapterTitle">1 Introduction to Clinical Medicine</h1>
<h2 class="heading2">1.1 History Taking</h2>
<h3 class="heading3">1.1.1 Chief Complaint</h3>
```

**Key difference from Springer**: Thieme embeds the section number directly in the heading text rather than using a separate `<span class="HeadingNumber">`. This means the heading-number-space issue (C-001) typically does NOT apply to Thieme books.

### Callout Boxes / Clinical Notes

Thieme uses distinctive clinical note containers:

```html
<div class="clinicalNote" data-type="note">
  <div class="noteTitle">Clinical Pearl</div>
  <div class="noteBody">
    <p>Important clinical observation text...</p>
  </div>
</div>

<div class="clinicalNote" data-type="tip">
  <div class="noteTitle">Practical Tip</div>
  <div class="noteBody">
    <p>Practical advice text...</p>
  </div>
</div>
```

Common note types: `note`, `tip`, `caution`, `definition`, `pearl`, `keypoint`

**Expected mapping**: `clinicalNote` → DocBook `<sidebar>`, `<tip>`, `<note>`, etc. based on `data-type` attribute.

### Table Patterns

```html
<div class="tableWrap" id="t001">
  <div class="tableTitle">
    <span class="label">Table 1.1</span>
    <span class="caption">Classification of Heart Failure</span>
  </div>
  <table>
    <thead><tr><th>Class</th><th>Definition</th></tr></thead>
    <tbody><tr><td>I</td><td>No limitation</td></tr></tbody>
  </table>
</div>
```

**Key**: Table IDs use `t001`, `t002` etc. (no formal/informal distinction by ID suffix like Springer's Tab1/Taba).

### Figure Patterns

```html
<div class="figureWrap" id="f001">
  <img src="../Images/ch01_fig001.jpg" alt="Description of figure" />
  <div class="figureTitle">
    <span class="label">Fig. 1.1</span>
    <span class="caption">Anatomy of the cardiac chambers</span>
  </div>
</div>
```

### Bibliography / References

Thieme often uses a different bibliography structure from Springer:

```html
<div class="bibliography">
  <h2>References</h2>
  <div class="bibEntry" id="bib001">
    <span class="bibNumber">[1]</span>
    <span class="bibText">
      Author A, Author B. Title of article.
      <em>Journal Name</em>. 2024;10(2):100-110
    </span>
  </div>
  <div class="bibEntry" id="bib002">
    <span class="bibNumber">[2]</span>
    <span class="bibText">...</span>
  </div>
</div>
```

**Key differences from Springer**:
- Uses `<div class="bibEntry">` instead of `<li class="Citation">` inside `<ol>`
- Uses `<span class="bibNumber">` instead of `<span class="CitationNumber">`
- May not use `<ol>` wrapper (uses `<div>` containers instead)
- External reference links (DOI, PubMed) may be structured differently

### Copyright / Metadata

```html
<div class="copyrightPage">
  <p class="publisher">Thieme Medical Publishers, Inc.</p>
  <p class="address">333 Seventh Avenue, New York, NY 10001</p>
  <p class="isbn">ISBN 978-1-XXXX-XXXX-X</p>
  <p class="eisbn">eISBN 978-1-XXXX-XXXX-X</p>
  <p class="copyright">© 2025 Thieme. All rights reserved.</p>
</div>
```

### Thieme-Specific CSS Classes Summary

| Class | Element Type | Purpose |
|-------|-------------|---------|
| `chapterTitle` | `<h1>` | Chapter heading |
| `heading2`, `heading3` | `<h2>`, `<h3>` | Sub-section headings |
| `clinicalNote` | `<div>` | Callout box container |
| `noteTitle` | `<div>` | Callout box title |
| `noteBody` | `<div>` | Callout box content |
| `tableWrap` | `<div>` | Table wrapper |
| `tableTitle` | `<div>` | Table caption container |
| `figureWrap` | `<div>` | Figure wrapper |
| `figureTitle` | `<div>` | Figure caption container |
| `label` | `<span>` | Number label (Table/Figure) |
| `caption` | `<span>` | Caption text |
| `bibliography` | `<div>` | References section |
| `bibEntry` | `<div>` | Single reference entry |
| `bibNumber` | `<span>` | Reference number |
| `bibText` | `<span>` | Reference text content |
| `copyrightPage` | `<div>` | Copyright block |

### Known Thieme Patterns To Watch For

- Some Thieme books render tables as **images** rather than HTML tables (especially complex clinical tables). Check for `<img>` inside table wrappers.
- Thieme's cross-references may use `data-href` attributes instead of standard `<a href>`.
- Some Thieme EPUBs use numbered `<div>` containers for list-like content instead of `<ol>`/`<ul>`.
- Thieme frequently uses color-coded callout boxes (blue for notes, green for tips, red for warnings). The color information is in the CSS, not in the HTML structure.

---

## 6. Other Publishers

Placeholder sections for publishers not yet extensively scanned. Add detailed patterns as books from these publishers are processed through the conversion pipeline.

### Oxford University Press

**Status**: Expanding -- add patterns as OUP books are scanned.

Known identifiers in `PUBLISHER_PATTERNS`: `'oxford university press'`, `'oxford'`, `'oup'`

#### Expected File Structure

```
OEBPS/
  xhtml/
    cover.xhtml
    halftitle.xhtml
    title.xhtml
    copyright.xhtml
    dedication.xhtml
    toc.xhtml
    chapter01.xhtml (or ch01.xhtml)
    ...
    bibliography.xhtml
    index.xhtml
  images/
    ...
  styles/
    oup.css
  content.opf
  toc.ncx
```

#### Known Heading Patterns

OUP typically uses clean semantic HTML:

```html
<section epub:type="chapter" id="chapter-1">
  <h1>1. Introduction</h1>
  <section id="sec-1-1">
    <h2>1.1 Background</h2>
  </section>
</section>
```

Section numbers are typically embedded in heading text (like Thieme), not in separate spans (unlike Springer).

#### Known Bibliography Patterns

OUP academic books often use author-date (Harvard) citation style:

```html
<section epub:type="bibliography">
  <h2>Bibliography</h2>
  <p class="bib" id="bib-smith2020">
    Smith, J. and Jones, A. (2020). <i>Title of Book</i>. Oxford University Press.
  </p>
</section>
```

In-text citations: `<a href="#bib-smith2020">(Smith and Jones, 2020)</a>`

#### Known OUP CSS Classes

| Class | Purpose |
|-------|---------|
| `chapter-title` | Chapter heading |
| `bib` | Bibliography entry |
| `box` / `textbox` | Callout boxes |
| `fig` | Figure wrapper |
| `tbl` | Table wrapper |

#### Known OUP Patterns To Watch For

- OUP often uses Harvard (author-date) citation style rather than numbered references
- Endnotes may be in a separate file (`notes.xhtml`) rather than inline
- Cross-references to endnotes use `epub:type="noteref"`
- Some OUP books have extensive front matter (List of Contributors, Abbreviations, etc.)
- Index entries may use `data-indexterm` attributes

---

### Cambridge University Press

**Status**: Expanding -- add patterns as CUP books are scanned.

Known identifiers in `PUBLISHER_PATTERNS`: `'cambridge university press'`, `'cambridge'`

#### Expected File Structure

```
OEBPS/
  xhtml/
    cover.xhtml
    frontmatter.xhtml
    chapter1.xhtml (or c01.xhtml)
    ...
    backmatter.xhtml
    index.xhtml
  images/
    ...
  styles/
    cambridge.css
  content.opf
  toc.ncx
```

#### Known Heading Patterns

CUP uses clean heading structure, often with `epub:type`:

```html
<section epub:type="chapter" id="ch1">
  <header>
    <h1><span class="chapter-num">1</span> Introduction</h1>
  </header>
  <section id="ch1-sec1">
    <h2>1.1 Scope and Purpose</h2>
  </section>
</section>
```

May use `<header>` wrapper around chapter title (EPUB 3 pattern).

#### Known Bibliography Patterns

CUP supports both numbered and author-date citation styles:

```html
<!-- Numbered style -->
<ol class="references">
  <li id="ref1" class="reference">
    Author A. Title. <i>Journal</i>. 2024;10:100-110.
  </li>
</ol>

<!-- Author-date style -->
<p class="reference" id="ref-smith2020">
  Smith, J. (2020). <i>Book Title</i>. Cambridge University Press.
</p>
```

#### Known CUP CSS Classes

| Class | Purpose |
|-------|---------|
| `chapter-num` | Chapter number label |
| `reference` | Bibliography entry |
| `references` | Bibliography list |
| `extract` | Block quotation / extract |
| `box` | Callout / text box |
| `figure` | Figure container |
| `table-wrap` | Table container |

#### Known CUP Patterns To Watch For

- CUP may use `<header>` elements around chapter titles
- Some books use `<details>` and `<summary>` for expandable content (EPUB 3 interactive)
- Cross-references often use descriptive `epub:type` attributes
- CUP academic monographs may have extensive endnotes in separate files
- Appendices may be structured differently from chapters (different heading levels)

---

### McGraw-Hill

**Status**: Placeholder -- add patterns as McGraw-Hill books are scanned.

Known identifiers in `PUBLISHER_PATTERNS`: `'mcgraw-hill'`, `'mcgraw hill'`, `'mgh'`

Patterns to document when available:
- File naming conventions
- CSS class patterns
- Heading structure
- Bibliography format
- Cross-reference patterns

---

### Taylor & Francis / Routledge

**Status**: Placeholder -- add patterns as T&F/Routledge books are scanned.

Known identifiers in `PUBLISHER_PATTERNS`: `'routledge'`, `'taylor & francis'`, `'taylor and francis'`, `'crc press'`

Note: CRC Press is a major imprint for scientific/technical content. Patterns may differ between Routledge (humanities/social sciences) and CRC Press (STEM).

Patterns to document when available:
- File naming conventions
- CSS class patterns
- Heading structure
- Bibliography format
- Cross-reference patterns

---

### Pearson

**Status**: Placeholder -- add patterns as Pearson books are scanned.

Known identifiers in `PUBLISHER_PATTERNS`: `'pearson'`, `'prentice hall'`, `'addison-wesley'`, `'informit'`

Patterns to document when available:
- File naming conventions
- CSS class patterns
- Heading structure
- Bibliography format
- Cross-reference patterns

---

## 7. Publisher Detection

How to identify the publisher for a given book.

### From the R2 Platform

Look at the **publisher field** on the title page in the R2 Digital Library interface. This is the most reliable manual identification method.

### From EPUB Metadata

The `<dc:publisher>` element in `content.opf`:

```xml
<dc:publisher>Springer Nature Switzerland AG</dc:publisher>
<dc:publisher>John Wiley &amp; Sons, Ltd</dc:publisher>
<dc:publisher>Elsevier Inc.</dc:publisher>
```

### From File Patterns

Publishers have distinctive file naming conventions:

| Publisher | Chapter File Pattern | Example |
|-----------|---------------------|---------|
| Springer | `{bookid}_1_En_{N}_Chapter.xhtml` | `657658_1_En_1_Chapter.xhtml` |
| Wiley | `ch{NN}.xhtml` | `ch01.xhtml` |
| Elsevier | `chapter-{N}.xhtml` or `B978..._{NNNNN}.xhtml` | `chapter-1.xhtml` |
| Thieme | `chapter{NN}.xhtml` or `c{NN}.xhtml` | `chapter01.xhtml` |
| Others | Varies | Inspect OPF manifest |

### From CSS Class Names

Publishers use distinctive CSS classes that can help identify the source:

| Publisher | Distinctive Classes |
|-----------|-------------------|
| Springer | `FormalPara`, `HeadingNumber`, `CitationRef`, `BibliographyWrapper`, `ArticleOrChapterToc` |
| Wiley | `chapter-title`, `section-title`, series-specific classes |
| Elsevier | Varies by imprint |
| Thieme | `chapterTitle`, `clinicalNote`, `noteTitle`, `noteBody`, `tableWrap`, `figureWrap`, `bibEntry` |

### Programmatic Detection

Use `epub_metadata.py` for automated publisher detection:

```python
from epub_metadata import detect_publisher, PUBLISHER_PATTERNS

# From an EPUB file path
publisher = detect_publisher(epub_path=Path("book.epub"))
# Returns: 'springer', 'wiley', 'elsevier', etc. or None

# From pre-extracted metadata
publisher = detect_publisher(metadata_dict={'publisher': 'Springer Nature'})
# Returns: 'springer'
```

### Built-in Publisher Patterns

The `PUBLISHER_PATTERNS` dictionary in `epub_metadata.py` contains the following publisher keys and their matching strings:

| Key | Match Strings |
|-----|--------------|
| `wiley` | `wiley`, `john wiley`, `wiley-blackwell`, `wiley-vch`, `jossey-bass` |
| `springer` | `springer`, `springer nature`, `springer-verlag` |
| `elsevier` | `elsevier`, `academic press`, `butterworth-heinemann`, `morgan kaufmann` |
| `pearson` | `pearson`, `prentice hall`, `addison-wesley`, `informit` |
| `mcgraw-hill` | `mcgraw-hill`, `mcgraw hill`, `mgh` |
| `oxford` | `oxford university press`, `oxford`, `oup` |
| `cambridge` | `cambridge university press`, `cambridge` |
| `routledge` | `routledge`, `taylor & francis`, `taylor and francis`, `crc press` |
| `thieme` | `thieme`, `thieme medical`, `georg thieme` |

Detection is case-insensitive substring matching against the `<dc:publisher>` metadata value.
