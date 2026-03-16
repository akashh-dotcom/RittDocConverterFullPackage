# Known Issues — R2 Digital Library Converter & Platform

This document catalogs known issues found during QA scans of books on the R2 Digital Library platform. Issues are categorized by source: **Converter** (XML conversion pipeline), **Publisher** (source XHTML), or **Platform** (R2 rendering engine).

This is a **multi-publisher** registry. Each issue includes a **Publisher** field indicating which publisher's content first surfaced the problem and whether it is expected to affect other publishers.

---

## How to Add New Issues

Use the following naming convention for issue IDs:

| Prefix | Category | Example |
|--------|----------|---------|
| `C-NNN` | **Converter** — Issues introduced by the XML conversion pipeline (`epub_to_structured_v2.py`) | C-010 |
| `P-NNN` | **Publisher** — Issues present in the publisher's source XHTML files | P-002 |
| `R-NNN` | **Platform** — Issues in the R2 rendering engine (not the XML data) | R-006 |

When adding a new issue:
1. Find the next available number in the appropriate category (C, P, or R).
2. Fill in **all** fields from the Issue Tracking Template at the bottom of this file.
3. Always include the **Publisher** and **First Found In** fields.
4. If the issue is likely to affect books from other publishers, note that in the Publisher field (e.g., "Springer Nature (may apply to others)").

---

## Known Non-Issues (DO NOT REPORT)

These are intentional behaviors — do not flag them as bugs:

| Behavior | Reason |
|----------|--------|
| Search/index term links (`/search?q=term`) for drugs, diseases, PMID, MeSH headings | Intentional platform feature — makes platform search intelligent |
| Missing index / backmatter | Intentionally dropped during conversion |
| Zero-width spaces in original source | Used as line-break hints by Springer (but DO report if they become visible spaces in converted output) |

---

## Converter Issues

### C-001: Missing Spaces in Section Headings
- **Severity**: HIGH
- **Scope**: Systemic (all sections with numbered headings)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Trailing space inside `<span class="HeadingNumber">1.1 </span>` is stripped during conversion, producing `<emphasis role="HeadingNumber">1.1</emphasis>When to Suspect...`
- **Root Cause**: Whitespace normalization in the conversion pipeline (`epub_to_structured_v2.py`) strips trailing spaces from inline elements. Suspected locations: `_normalize_inline_whitespace()`, `_normalize_whitespace()`, or interaction between XSLT compliance transformation and re-parsing stages.
- **Fix Area**: `epub_to_structured_v2.py` — whitespace handling around `<emphasis role="HeadingNumber">` elements
- **Impact**: Affects heading readability and breadcrumb display across all chapters

### C-002: Copyright/Metadata Line Concatenation
- **Severity**: HIGH
- **Scope**: Systemic (all 14 chapters in tested book)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Separate HTML elements (`<div class="ChapterCopyright">`, `<span class="ContextInformationAuthorEditorNames">`, `<span class="BookTitle">`, `<span class="ChapterDOI">`) are flattened into a single `<para role="ChapterCopyright">` without inserting whitespace separators.
- **Example**: `2025M. Salih` (year concatenated with author name), `978-3-032-03239-3978-3-032-03240-9` (ISBNs concatenated)
- **Root Cause**: Converter collapses multiple block/inline elements into one `<para>` without emitting spaces between them.
- **Fix Area**: `epub_to_structured_v2.py` — copyright/metadata block handling

### C-003: Callout Boxes Unstyled (FormalPara → bare para)
- **Severity**: MEDIUM-HIGH
- **Scope**: Systemic (273 callout boxes in tested book)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Springer's `<div class="FormalPara FormalParaRenderingStyle3 ParaTypeTip">` (and `ParaTypeOverview`, `ParaTypeImportant`) are converted to plain `<para>Hospitalist Tip</para>` followed by bare content, losing all callout/admonition styling.
- **Expected**: Should map to DocBook `<tip>`, `<note>`, `<important>`, `<sidebar>`, etc.
- **Root Cause**: Missing mapping from Springer's FormalPara CSS classes to DocBook admonition elements.
- **Fix Area**: `epub_to_structured_v2.py` — FormalPara class handling

### C-004: Table Numbering Issues
- **Severity**: MEDIUM
- **Scope**: Systemic
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Two issues:
  1. Formal tables lose chapter-prefixed numbering (e.g., "Table 1.1" → "Table 1")
  2. Informal tables (used for layout, `id="Taba"`) incorrectly receive table numbers
- **Root Cause**: Converter doesn't distinguish between formal (`id="Tab1"`) and informal (`id="Taba"`) tables, and doesn't preserve chapter-prefixed numbering.
- **Fix Area**: `epub_to_structured_v2.py` — table ID parsing and caption handling

### C-005: Zero-Width Space → Visible Space
- **Severity**: MEDIUM
- **Scope**: Targeted (~24 "StatPearls" → "Stat Pearls", ~15 "doi.org" → "doi. org")
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Unicode zero-width space (`\u200b`) used by Springer as line-break hints gets converted to regular spaces, splitting compound words and breaking URLs.
- **Root Cause**: Text normalization replacing or expanding zero-width spaces.
- **Fix Area**: `epub_to_structured_v2.py` — text normalization / Unicode handling

### C-006: Double Periods in References
- **Severity**: MEDIUM
- **Scope**: 195 instances across bibliography sections in tested book
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Double periods (`..`) appear in reference entries, typically after page ranges (e.g., `1450–62..`).
- **Root Cause**: Converter appends a period to reference entries that already end with one from the source.
- **Fix Area**: `epub_to_structured_v2.py` — bibliography/`<bibliomixed>` processing

### C-007: UTF-8 Double-Encoding (NBSP → "Â")
- **Severity**: MEDIUM
- **Scope**: Isolated (author name fields)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Non-breaking space (`\u00A0`) bytes `C2 A0` interpreted as Latin-1, producing visible "Â" character. Example: `Mohammed Salih` → `MohammedÂ Salih`.
- **Root Cause**: Character encoding mismatch during conversion — UTF-8 bytes read as Latin-1/ISO-8859-1.
- **Fix Area**: `epub_to_structured_v2.py` — encoding declaration / byte handling

### C-008: Lost PubMed/CrossRef Link Labels
- **Severity**: LOW-MEDIUM
- **Scope**: ~30 lost link labels in tested book
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Some `<a>PubMed</a>` and `<a>CrossRef</a>` labels in bibliography entries are lost during conversion, though the underlying URLs may be preserved.
- **Root Cause**: Reference link processing not fully preserving display text for external reference links.
- **Fix Area**: `epub_to_structured_v2.py` — bibliography external link handling

### C-009: Broken URLs
- **Severity**: LOW
- **Scope**: Isolated (1 instance in tested book, Chapter 13)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Malformed URLs like `https://.` appearing in converted output.
- **Root Cause**: URL extraction/reconstruction error during conversion.
- **Fix Area**: `epub_to_structured_v2.py` — URL handling

### C-010: ISBN Field Concatenation on Copyright Page
- **Severity**: HIGH
- **Scope**: Isolated (copyright page)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Print ISBN and e-ISBN fields are concatenated without separators, producing confusing text like `97512-7e-ISBN`.
- **Root Cause**: Same as C-002 — copyright handler flattens all child elements into one `<para>` without `<?lb?>` PIs.
- **Fix Area**: `epub_to_structured_v2.py` — copyright metadata handling

### C-011: Missing Spaces Before/After Inline `<emphasis>` Elements
- **Severity**: HIGH
- **Scope**: Systemic (hundreds of instances per book — 242+ in tested book, 89 of 150 files)
- **Publisher**: Springer Nature (likely affects all publishers)
- **First Found In**: Anatomy of the Upper and Lower Limbs (9783031975134)
- **Description**: Text runs directly into `<emphasis>` tags without spaces, e.g., `the<emphasis>flexor digitorum</emphasis>` renders as "theflexor digitorum" instead of "the flexor digitorum". Also occurs after closing tags: `</emphasis>and` renders as "and" running into the preceding italic/bold text.
- **Root Cause**: The `extract_inline_content()` function in `epub_to_structured_v2.py` does not ensure whitespace between text nodes and inline child elements when building the XML tree. When source HTML has `the <i>flexor</i>`, the space before `<i>` may be lost during tree construction.
- **Fix Area**: `epub_to_structured_v2.py` — `extract_inline_content()` text/tail whitespace handling
- **Example**:
  - Source: `The <i>flexor digitorum longus</i> (FDL)`
  - Converted: `The<emphasis>flexor digitorum longus</emphasis> (FDL)`
  - Rendered: "Theflexor digitorum longus (FDL)"

### C-012: PubMed/Crossref Link URLs Dropped (Plain Text Only)
- **Severity**: MEDIUM
- **Scope**: Systemic (all PubMed/Crossref links in bibliography — 33+ in tested book across 10 files)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Anatomy of the Upper and Lower Limbs (9783031975134)
- **Description**: PubMed, PubMed Central, and Crossref links in bibliography entries lose their hyperlink URLs during conversion. The display text is preserved as plain text but not wrapped in `<ulink>`. Labels also concatenate without separators: "CrossrefPubMedPubMed Central".
- **Root Cause**: The bibliography processing in `epub_to_structured_v2.py` does not preserve `<a href="...">` elements for external reference links. Only DOI links are converted to `<ulink>`.
- **Fix Area**: `epub_to_structured_v2.py` — bibliography `<a>` tag handling

### C-013: Period-Space in Abbreviations and DOI URLs
- **Severity**: MEDIUM-HIGH
- **Scope**: Systemic (45+ abbreviation splits + 18 DOI URL splits in tested book)
- **Publisher**: Springer Nature (likely affects all publishers)
- **First Found In**: Anatomy of the Upper and Lower Limbs (9783031975134)
- **Description**: A post-processing regex adds spaces after periods, breaking abbreviations ("i.e." → "i. e.", "e.g." → "e. g.") and DOI display text ("doi.org" → "doi. org").
- **Root Cause**: A period-space normalization regex in `manual_postprocessor.py` (stage S9a) that adds spaces after ALL periods followed by word characters, without excluding abbreviations and URLs.
- **Fix Area**: `manual_postprocessor.py` — period-space normalization regex
- **Example**:
  - Source: `(i.e. the articular surface)`
  - Converted: `(i. e. the articular surface)`

### C-014: Stray Footnote/Affiliation Markers in Copyright Blocks
- **Severity**: MEDIUM
- **Scope**: Systemic (all chapter copyright blocks — 17 in tested book)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Anatomy of the Upper and Lower Limbs (9783031975134)
- **Description**: Affiliation superscript markers and contact icons appear as visible artifacts in copyright paragraphs: `<superscript>1<emphasis role="ContactIcon"/></superscript>(1)`. These render as "¹(1)" after the author name.
- **Root Cause**: The converter preserves footnote/affiliation markup from the source that should be stripped or handled separately in the copyright context.
- **Fix Area**: `epub_to_structured_v2.py` — copyright block processing (strip affiliation markers)

### C-015: Empty Bibliography Entries (Content Lost)
- **Severity**: HIGH
- **Scope**: Isolated (3 entries in preface of tested book)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Anatomy of the Upper and Lower Limbs (9783031975134)
- **Description**: Bibliography entries are generated as empty self-closing tags (`<bibliomixed id="..."/>`) with no text content. The preface body text cites references 1-3 that have no content.
- **Root Cause**: The bibliography extraction logic fails to capture content from certain preface/front-matter bibliography sections, possibly due to different HTML structure in front matter vs chapter references.
- **Fix Area**: `epub_to_structured_v2.py` — bibliography extraction for preface sections

### C-016: BibSection Content Loss ("Recommended/Further Reading" Dropped)
- **Severity**: HIGH
- **Scope**: Systemic (any chapter with bibliography sub-sections)
- **Publisher**: Springer Nature (may apply to others with similar bibliography structure)
- **First Found In**: Cancer Care in the Post-COVID World (9783031338557)
- **Description**: When a chapter's bibliography (`<aside class="Bibliography">`) contains a sub-section marked with `<div class="BibSection">` and heading "Recommended Reading", "Further Reading", or "Suggested Reading", the entire sub-section and all its citations are silently dropped during conversion. No error is logged.
- **Root Cause**: The bibliography processing in `epub_to_structured_v2.py` only processes the main `<ol class="BibliographyWrapper">` inside `<aside class="Bibliography">`. It does not recurse into `BibSection` subdivisions that contain additional `<ol class="BibliographyWrapper">` lists.
- **Fix Area**: `epub_to_structured_v2.py` — bibliography processing (handle `BibSection` divs within `<aside class="Bibliography">`)
- **Example**:
  - Source: `<div class="BibSection" id="BSec1"><div class="Heading">Recommended Reading</div><ol class="BibliographyWrapper">...(7 citations)...</ol></div>`
  - Converted: (entirely absent — 0 traces of section or its 7 citations)
- **Note**: Ch8's "Recommended Reading" uses `FormalPara` class (not `BibSection`) and IS preserved (as flat `<para>` text). The pattern is only lost when it's a `BibSection` inside `<aside class="Bibliography">`.

### C-017: Ordered Lists Converted to Itemized Lists
- **Severity**: MEDIUM
- **Scope**: Systemic (all content `<ol>` elements)
- **Publisher**: Springer Nature (likely affects all publishers)
- **First Found In**: Cancer Care in the Post-COVID World (9783031338557)
- **Description**: All HTML `<ol>` (ordered list) elements in body content are converted to DocBook `<itemizedlist>` instead of `<orderedlist>`. The numbering structure is lost as a DocBook element type.
- **Root Cause**: The list processing in `epub_to_structured_v2.py` does not distinguish between `<ol>` and `<ul>` — both are mapped to `<itemizedlist>`.
- **Fix Area**: `epub_to_structured_v2.py` — list element handling (map `<ol>` to `<orderedlist>`, `<ul>` to `<itemizedlist>`)

### C-018: Empty Alt Text on All Images
- **Severity**: MEDIUM
- **Scope**: Systemic (all images across all chapters)
- **Publisher**: Springer Nature (likely affects all publishers)
- **First Found In**: Pediatric Surgical Oncology (9783031768828)
- **Description**: All `<img>` elements in the converted output have empty `alt` attributes (`alt=""`). While the images themselves render correctly with Save/Enlarge/Direct Link buttons, the empty alt text means the content is not accessible to screen readers or when images fail to load.
- **Root Cause**: The converter does not extract or generate meaningful alt text from the source XHTML. Springer source files typically include alt text in `<img alt="Description">` attributes, but this is not preserved during conversion. Alternatively, the source may also have empty alt text (publisher source issue).
- **Fix Area**: `epub_to_structured_v2.py` — image/figure processing (preserve source alt text in `<imagedata>` or `<textobject>`)
- **Accessibility Impact**: Fails WCAG 2.1 Level A Success Criterion 1.1.1 (Non-text Content). All images must have meaningful alt text or be explicitly marked as decorative.

### C-019: Orphan Phantom Pages (Empty Content Sections)
- **Severity**: LOW
- **Scope**: Isolated (3-5 pages per book, typically at end of front matter)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Pediatric Surgical Oncology (9783031768828)
- **Description**: Several section pages (e.g., `pr0007`, `pr0008`, `pr0009`, `pr0010`) contain no content — just an empty page with header/footer. These correspond to blank separator pages in the source EPUB that should have been excluded during conversion.
- **Root Cause**: The converter creates XML section files for ALL source XHTML pages, including intentionally blank pages (part dividers, blank verso pages, etc.) that have no meaningful content. The converter does not filter these out.
- **Fix Area**: `epub_to_structured_v2.py` — section detection (skip pages with no text content, or mark as excluded in the manifest)

### C-020: Missing `<thead>` on Tables
- **Severity**: MEDIUM
- **Scope**: Systemic (some tables across all chapters)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Pediatric Surgical Oncology (9783031768828)
- **Description**: Some converted tables lack proper `<thead>` elements — all rows are placed in `<tbody>` even when the source HTML clearly has a `<thead>` section. This affects table rendering (header rows are not styled differently or repeated on page breaks) and accessibility (screen readers cannot identify header cells).
- **Root Cause**: The table conversion logic in `epub_to_structured_v2.py` may not preserve the `<thead>`/`<tbody>` distinction from the source HTML when building CALS table model elements.
- **Fix Area**: `epub_to_structured_v2.py` — table processing (preserve thead/tbody structure)

### C-021: TOC List Numbering Collapse
- **Severity**: MEDIUM
- **Scope**: Isolated (Contents/TOC page)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Pediatric Surgical Oncology (9783031768828)
- **Description**: The Table of Contents page renders chapter listings as 107 separate `<ol>` elements, each containing exactly 1 `<li>`. This causes broken numbering (each item restarts at "1" or "a") instead of a single continuous numbered list. The visual result is every chapter/section labeled "a." or "1." instead of proper sequential numbering.
- **Root Cause**: The TOC conversion creates a new `<orderedlist>` for each chapter entry instead of grouping all entries under a single list. This may be caused by treating each TOC `<li>` as a separate block element.
- **Fix Area**: `epub_to_structured_v2.py` — TOC processing (group entries into a single `<orderedlist>`)
- **Related Issue**: R-005 (Contents page sub-items all labeled "a." — the platform rendering symptom of this converter issue)

### C-022: Editor/Author Bio Text Detachment
- **Severity**: MEDIUM
- **Scope**: Isolated (About Editors/Contributors pages)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Pediatric Surgical Oncology (9783031768828)
- **Description**: On the "About the Editors" or "Contributors" pages, author biographical text is detached from the author name or credentials are fused together. For example, institution names run into country codes ("UKAbdelhafeez"), or bio paragraphs are concatenated without proper separation.
- **Root Cause**: The converter flattens structured author/editor metadata (separate `<div>` elements for name, institution, bio) into a single `<para>` without inserting proper separators. Similar root cause to C-002 (copyright metadata flattening).
- **Fix Area**: `epub_to_structured_v2.py` — author/editor metadata handling (preserve `<?lb?>` separators between fields)

### C-023: Reference Label Concatenation (CrossrefPubMed / PubMedPubMed Central)
- **Severity**: MEDIUM
- **Scope**: Systemic (all bibliography sections with external reference links)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Pediatric Surgical Oncology (9783031768828)
- **Description**: External reference link labels in bibliography entries are concatenated without separators. Examples: "CrossrefPubMed" (should be "Crossref | PubMed"), "PubMedPubMed Central" (should be "PubMed | PubMed Central"). The individual link URLs are also dropped (see C-012), leaving only concatenated plain text.
- **Root Cause**: The bibliography `<a>` tag processing strips each link down to its display text but does not add spacing or separator characters between adjacent link labels. When multiple `<span class="ExternalRef">` elements appear in sequence, their text content concatenates.
- **Fix Area**: `epub_to_structured_v2.py` — bibliography external reference link processing (add separators between labels, preserve URLs as `<ulink>`)
- **Related Issue**: C-012 (PubMed/Crossref link URLs dropped)

---

## Publisher Source Issues

### P-001: Empty Brackets in TOC Entries
- **Severity**: MEDIUM
- **Scope**: Systemic (any chapter with citation references in headings)
- **Publisher**: Springer Nature
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Springer's own in-chapter TOC (`<nav class="ArticleOrChapterToc">`) strips `CitationRef` content from heading copies but leaves bracket punctuation behind, producing `[]`, `[,]`, `[,.]`.
- **Note**: The body headings have the citations resolved. Only the TOC copy has them stripped.
- **Workaround**: Converter could strip residual empty brackets from TOC text as a post-processing step.

---

## Platform Issues (R2 Digital Library)

### R-001: Front Matter Link Redirects to Title Page
- **Severity**: MEDIUM
- **Scope**: All books tested
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: The "Front Matter" link in the TOC redirects to the book's title page (`/Resource/Title/{ISBN}`) instead of the actual front matter content pages (e.g., `pr0001`, `pr0002`).
- **Note**: This appears to be a platform routing behavior, not a conversion issue.

### R-002: Duplicate Breadcrumb Entries
- **Severity**: MEDIUM
- **Scope**: Systemic (Part/Chapter landing pages)
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Part names or chapter names appear duplicated 2-3 times in the breadcrumb trail on landing pages.
- **Note**: May be a platform rendering issue with how it constructs breadcrumbs from the XML hierarchy.

### R-003: External Link Icons on Internal Links
- **Severity**: LOW
- **Scope**: Systemic
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: Cross-reference links between sections within the same book display an external link icon, making them look like they point to external websites.
- **Note**: Platform UI rendering issue — internal cross-references should not show external link icons.

### R-004: Cover Image Shows Publisher Logo
- **Severity**: LOW
- **Scope**: Isolated
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: The cover page (`pr0001`) displays the publisher's generic logo instead of the actual book cover image.
- **Note**: May be a missing asset or incorrect image reference.

### R-005: Contents Page Sub-items All Labeled "a."
- **Severity**: MEDIUM
- **Scope**: Front matter contents page
- **First Found In**: Practical Hospitalist Guide to Inpatient Cardiology (9783032032409)
- **Description**: On the Contents page (`pr0005`), all chapter sub-items are labeled "a." instead of having proper hierarchical numbering or no bullets.
- **Note**: May be a platform CSS/rendering issue with ordered list styling.

### R-006: Part Landing Pages Empty or Missing Chapter Listing
- **Severity**: MEDIUM
- **Scope**: Systemic (all Part landing pages)
- **First Found In**: Anatomy of the Upper and Lower Limbs (9783031975134)
- **Description**: Part landing pages (e.g., `pt0001s0001`) show the Part title but no chapter listing beneath it. The page appears sparse/empty with no useful navigation.
- **Note**: May be a packaging issue (`package.py`) — the Part pages may not include child chapter references, or the platform may not render them.

### C-024: Broken Citation-to-Bibliography Links (ID Scheme Mismatch)
- **Severity**: HIGH
- **Scope**: Systemic (all citation links in all chapters)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Handbook of Endoscopic Ultrasound (9783031951794)
- **Description**: In-text citation links (e.g., `[4]`, `[12]`) fail to navigate to the corresponding bibliography entry. The body text cites `linkend="CR4"` but the bibliography entry has `id="ch0009s0013_CR4"` (chapter-prefixed). The mismatch means all citation cross-references are dead links.
- **Root Cause**: The conversion pipeline uses two different ID schemes: body text uses bare citation IDs (`CR4`) while bibliography entries get chapter-prefixed IDs (`ch0009s0013_CR4`). The deferred link resolution cannot match them.
- **Fix Area**: `epub_to_structured_v2.py` — citation ID generation (ensure consistent ID scheme between citation links and bibliography entry IDs)

### C-025: Image Fileref Collapse (All Images → Same Filename)
- **Severity**: CRITICAL
- **Scope**: Systemic (all images across all chapters)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Handbook of Endoscopic Ultrasound (9783031951794)
- **Description**: All 221 `<imagedata>` elements pointed to only 2 unique filerefs (Ch0002f12.jpg and Coverf01.jpg), with all actual images replaced by 1x1 pixel JPEG placeholders.
- **Root Cause**: Bug in a previous version of the image packaging pipeline. The `intermediate_to_final` deduplication in `package.py` was collapsing all image paths to the same final name.
- **Fix Area**: `package.py` — image deduplication logic
- **Status**: **FIXED** in current code (verified: full pipeline now produces 221 unique filerefs and 413 MultiMedia files)

### C-026: Table Cross-References Not Linked
- **Severity**: HIGH
- **Scope**: Systemic (all table cross-references)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Handbook of Endoscopic Ultrasound (9783031951794)
- **Description**: Text like "Table 9.1" appears as plain text instead of a hyperlink to the actual table. The source has `<span class="InternalRef"><a href="#Tab1">9.1</a></span>` but the converted output produces `<emphasis role="InternalRef"><phrase>9.1</phrase></emphasis>`.
- **Root Cause**: In `epub_to_structured_v2.py` line 7363, `_register_label_anchor_ids` uses keyword tuple `('table', 'tbl')` for table matching, but Springer uses `Tab1`-style anchor IDs. Neither `'table' in 'tab1'` nor `'tbl' in 'tab1'` matches, so table anchor IDs are never registered. Deferred resolution then degrades `<link>` to `<phrase>`.
- **Fix Area**: `epub_to_structured_v2.py` — `_register_label_anchor_ids()` keyword matching (add `'tab'` keyword)

### C-027: Table Captions Missing Numbers (Adjacent Caption Not Extracted)
- **Severity**: HIGH
- **Scope**: Systemic (tables with sibling `<div class="Caption">`)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Handbook of Endoscopic Ultrasound (9783031951794)
- **Description**: Table captions show only fallback text instead of the actual "Table 9.1 Description" from the source. The source `<span class="CaptionNumber">Table 9.1</span>` followed by caption text is not extracted into the table `<title>`.
- **Root Cause**: In `epub_to_structured_v2.py` lines 10180-10185, the adjacent-caption code path calls `_register_label_anchor_ids` but does NOT call `extract_inline_content` to populate the table title. This is the only caption extraction path missing this call. Compare with the working `<table><caption>` path (line 10171-10178) which correctly calls both.
- **Fix Area**: `epub_to_structured_v2.py` — table caption handling (add `extract_inline_content` call for adjacent captions)

### C-028: Chapter Metadata Formatting Broken (Superscripts, Line Breaks, Structure Lost)
- **Severity**: HIGH
- **Scope**: Systemic (all chapters with metadata blocks — authors, affiliations, contact info)
- **Publisher**: Springer Nature (may apply to others)
- **First Found In**: Clinical Ophthalmic Oncology (9783031951794)
- **Description**: Chapter metadata (copyright, editor names, author names with affiliation superscripts, contact info, keywords) is broken into many separate flat `<para>` elements losing all inline structure. Specific problems:
  1. Superscript affiliation markers (¹, ²) split into separate `<para>` instead of inline `<superscript>`
  2. Author names broken across multiple `<para>` — "Nicole Rebollo Rodriguez" / "1" / "and" / "Julian D. Perry" / "2" as 5 separate paragraphs
  3. "(eds.)" split from editor names into own `<para>`
  4. "Email:" label split from email `<ulink>` into separate `<para>`
  5. Affiliation link anchors (`<a href="#Aff3">`) lost, rendering superscripts as plain text
- **Root Cause**: The converter treats each child element of the metadata blocks (`AuthorGroup`, `ChapterContextInformation`) as a separate block-level item, creating individual `<para>` elements. The `<sup>`, `<a>`, and `<span>` children inside author name divs should be processed as inline content within a single `<para>` rather than split into separate paragraphs. The `extract_inline_content` function is not being used for these metadata containers.
- **Fix Area**: `epub_to_structured_v2.py` — `AuthorGroup`, `ChapterContextInformation`, `Contact` div handling (process as inline content within structured paras, preserve superscript/link elements)
- **Example**:
  - Source: `<span class="AuthorName">Julian D. Perry</span><sup><a href="#Aff4">2</a></sup>`
  - Converted: `<para>Julian D. Perry</para><para>2</para>`
  - Expected: `<para>Julian D. Perry<superscript><link linkend="...">2</link></superscript></para>`

---

## Issue Tracking Template

When discovering new issues, use this template:

```
### [Source]-[NNN]: [Brief Title]
- **Severity**: HIGH / MEDIUM / LOW
- **Scope**: Systemic (N instances) / Isolated (N instances)
- **Publisher**: [Springer Nature / Wiley / All / etc.]
- **First Found In**: [Book title (ISBN)]
- **Description**: [What the issue looks like]
- **Root Cause**: [Technical explanation]
- **Fix Area**: [File/module to fix]
- **Example**:
  - Source: `[original markup]`
  - Converted: `[converted markup]`
  - Rendered: [what appears on screen]
```
