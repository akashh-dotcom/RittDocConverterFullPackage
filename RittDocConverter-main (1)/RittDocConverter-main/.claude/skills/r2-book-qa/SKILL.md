---
name: r2-book-qa
description: >
  Publisher-agnostic QA scan for books on the R2 Digital Library platform.
  Performs browser-based visual inspection via MCP Chrome tools,
  then optionally validates findings against source HTML and converted XML files.
  Works with any publisher (Springer, Wiley, Elsevier, Oxford, etc.).
  Use when a user provides an R2 staging/production URL and optionally
  source HTML + converted XML zip files for a book.
argument-hint: "[R2 book URL] [optional: source_xhtml.zip] [optional: converted_xml.zip]"
---

# R2 Digital Library Book QA Skill

You are a QA specialist for books published on the R2 Digital Library platform. Your job is to perform a thorough, publisher-agnostic quality audit of a book's digital rendering, identify conversion issues, and produce a detailed report.

## Workflow Overview

This skill has **three phases**:

1. **Phase 1 — Browser QA Scan**: Navigate the book on the R2 platform using MCP Chrome tools to identify visual/functional issues.
2. **Phase 2 — Source File Validation** (optional): Compare the publisher's source HTML/XHTML against the converted XML to confirm root causes and find additional issues not visible in the browser.
3. **Phase 3 — Report Generation**: Compile findings into a structured QA report.

If only a URL is provided, perform Phase 1 + Phase 3. If source files are also provided, perform all three phases.

### Related Skills (QA Lifecycle)

After this skill identifies issues, the following skills continue the lifecycle:
- **`r2-diagnosis`** — Deep root-cause analysis comparing source vs converted files
- **`r2-xml-fixer`** — Fix the converted XML output for the current book
- **`r2-code-fixer`** — Fix the converter source code to prevent recurrence

---

## PHASE 1: Browser QA Scan

### Step 1: Setup and Title Page Inspection

1. **Connect to browser**: Use `tabs_context_mcp` to get available tabs, or create a new one.
2. **Navigate to the book URL**: Use `navigate` to open the R2 book URL.
3. **Screenshot the title page**: Capture and review the title page layout.
4. **Record book metadata** — note ALL of the following:
   - **Title**
   - **Author(s)**
   - **Publisher** ← CRITICAL: Record this for publisher-specific pattern lookup later
   - **ISBN**
   - **Edition / Publication date**
5. **Check title page elements**:
   - Cover image present and correct? (not a generic publisher logo)
   - All metadata fields populated?
   - "Full Text Available" status shown?
6. **Identify publisher**: Once the publisher is known, reference `../_shared/publisher-patterns.md` for that publisher's known XHTML patterns and common conversion issues.

### Step 2: Table of Contents (TOC) Inspection

1. **Scroll down** to the Table of Contents section on the title page.
2. **Use `read_page` with `filter: interactive`** to capture all TOC links and their `href` destinations.
3. **Check TOC structure**:
   - Are all chapters/parts listed?
   - Do chapter links point to correct URLs? (expected pattern: `/resource/detail/{ISBN}/{sectionId}`)
   - Does "Front Matter" link redirect to the title page instead of actual front matter content? (Known platform issue R-001)
   - Are there any broken or missing links?
   - Are there any **empty brackets** (`[]`, `[,]`, `[,.]`) in chapter titles? (Known publisher issue — see P-001)
   - Note the URL pattern for section IDs (e.g., `ch0001s0001`, `pt0001s0001`)
   - Do TOC links point to chapter landing pages or to deep section pages?
4. **Content inventory check (CRITICAL)** — Count the total number of section links per chapter in the TOC. Record this as the **expected section count** for later comparison against actual rendered sections. This is essential for detecting **content loss** — sections that were silently dropped during conversion are invisible in browser-only scanning.
   - For each chapter, note: chapter number, number of sub-section links, section titles listed
   - This inventory will be compared against actual sections found during body content sampling (Step 5) and against source files (Phase 2)

### Step 3: Front Matter Pages

1. **Navigate sequentially** through front matter pages: `pr0001`, `pr0002`, `pr0003`, etc.
2. **Wait 2-3 seconds between navigations** to respect rate limiting.
3. **Check each page for**:
   - **Cover image**: Correct book cover or just a publisher logo? (Known issue R-004)
   - **Encoding issues**: Look for stray characters (e.g., "Â" from UTF-8 double-encoding, mojibake, garbled text)
   - **Metadata formatting**: Are ISBN, copyright, and author fields properly separated? (Look for concatenation like "2025AuthorName" or "978-XXX978-YYY")
   - **Breadcrumb accuracy**: Any duplicate entries? (Known issue R-002)
   - **Inline reference links in prose**: Check any references cited in Introduction/Preface body text — are they hyperlinked or just plain text? (Known issue C-012 applies to frontmatter prose, not just bibliography sections)

### Step 4: Chapter/Part Landing Pages

1. **Navigate to each Part landing page** (if the book has Parts) or each Chapter landing page.
2. **For each landing page, check**:
   - **Breadcrumb trail**: Look for duplicate entries (part/chapter name repeated 2-3 times)
   - **Chapter/section listing**: Are titles displayed correctly?
   - **Spacing issues**: Missing spaces between numbers and titles (e.g., "1.1When to" instead of "1.1 When to")
   - **Empty brackets**: `[]`, `[,]`, `[,.]` in section titles (unresolved cross-references)
   - **Stray characters**: Special symbols, encoding artifacts, or unexpected characters before/after text
   - **External link icons**: Present on links that should be internal cross-references? (Known issue R-003)
   - **Copyright/metadata lines**: Check for concatenation issues (year+name, ISBNs merged, etc.)
   - **List formatting**: Sub-items under chapters — are they properly numbered or all showing the same label? (Known issue R-005)

### Step 5: Body Content Sampling

Sample body content from **at least 4-5 chapters** spread across the book (beginning, middle, end). For each:

1. **Navigate to a body section** (e.g., `ch0001s0002`, `ch0005s0003`, `ch0010s0002`).
2. **Screenshot and review**:
   - **Section headings**: Missing spaces between number and title?
   - **Breadcrumb**: Same heading issues propagated?
   - **Body text**: Any spurious links? Missing spaces around cross-references?
   - **Missing spaces around inline formatting**: Look carefully for words running into bold/italic text — e.g., "The**radial nerve**" instead of "The **radial nerve**", or "the*flexor digitorum*" instead of "the *flexor digitorum*". This is a HIGH-priority check. (Known issue C-011)
   - **Abbreviation spacing**: Check for split abbreviations like "i. e" (should be "i.e."), "e. g" (should be "e.g."), "vs. " with extra space. (Known issue C-013)
   - **Lists**: Proper formatting? (numbered items correctly rendered?)
   - **Ordered vs unordered lists**: Are numbered lists rendered with actual numbers (1, 2, 3...) or with bullets? If a list should be numbered but shows bullets, the converter may have misclassified `<ol>` as `<itemizedlist>`. (Known issue C-017)
   - **Callout boxes / admonitions**: Are styled boxes like "Tip", "Note", "Important", "Warning", "Pearl", "Key Point", etc. visually distinct with borders/backgrounds? Or rendered as unstyled plain text? (These names vary by publisher and book)
   - **Inline formatting**: Bold, italic, superscript, subscript rendering correctly?
   - **Stray markers**: Look for stray superscript numbers, contact icons, or affiliation markers like "¹(1)" appearing after author names in copyright blocks. (Known issue C-014)
3. **Use `read_page` with `filter: interactive`** to check all links on the page:
   - **Search/index term links** (`/search?q=term`) — these are INTENTIONAL for drugs, diseases, PMID, and MeSH headings. Do NOT report these as bugs.
   - Cross-reference links to other sections — do they resolve correctly?
   - Bibliography reference links (e.g., `(4)`, `[2]`) — do they point to the references section?
4. **Content inventory comparison (CRITICAL for detecting content loss)**:
   - For each chapter sampled, navigate to its **References section** (the last content section before chapter metadata).
   - Check if there are **additional bibliography sub-sections** AFTER the numbered references, such as "Recommended Reading", "Further Reading", "Suggested Reading", "Additional Resources". These are commonly present in Springer books as `BibSection` divisions within the bibliography.
   - If no such sections appear on the rendered page, flag as **POTENTIAL CONTENT LOSS** — these sections are often silently dropped during conversion. (Known issue C-016)
   - Compare the **rendered section count** per chapter against the TOC section count from Step 2. Any mismatch signals possible content loss.
   - **This check is ESSENTIAL** — content loss is the #1 issue that browser-only scanning misses, because you can't see what isn't there.

### Step 5a: Cross-Reference Integrity Validation (Browser-Level)

For each sampled chapter, check that internal cross-references resolve correctly:

1. **Figure cross-references**: Search body text for patterns like "Fig. N", "Figure N", "Fig. N.N":
   - Use `find` tool to locate figure references in text
   - For each reference found, verify the referenced figure actually exists on the page or in the chapter
   - Check that figure reference links (if they are hyperlinked) point to the correct figure anchor
   - Unlinked figure references (plain text "Fig. 1" with no hyperlink) are acceptable but note them

2. **Table cross-references**: Same check for "Table N", "Table N.N":
   - Verify referenced tables exist
   - Check link targets if hyperlinked

3. **Section cross-references**: Look for references to other sections/chapters:
   - "see Section N.N", "see Chapter N", "as described in..."
   - If these are hyperlinked, click 1-2 to verify they navigate to the correct section
   - If they navigate to wrong sections or broken pages, flag as cross-reference integrity issue

4. **Citation-to-bibliography links**: In 1-2 sample chapters:
   - Click 2-3 in-text citation links (e.g., "[4]", "(Smith 2020)")
   - Verify they scroll/navigate to the correct bibliography entry
   - If citations link to wrong entries or don't link at all, flag the issue

5. **Equation/formula references**: If the book contains equations:
   - Check "Eq. N" or "Equation N" references in text
   - Verify referenced equations exist and are correctly displayed

### Step 5a2: Table Cross-Reference and Caption Validation (Browser-Level)

For each sampled chapter containing tables:

1. **Table cross-reference links**: Search body text for "Table N.N" references:
   - Use `find` tool to locate "Table" references in text
   - Check if table references are hyperlinked (clickable) or plain text
   - If plain text, this indicates the cross-reference system failed (Known issue C-026)
   - Click 1-2 table reference links (if they exist) to verify they navigate to the correct table

2. **Table caption number preservation**: For each rendered table:
   - Check if the table caption starts with "Table N.N" (chapter-prefixed numbering)
   - If captions show only generic text (e.g., "Table 1" or just description text without "Table N.N"), the caption number was lost during conversion (Known issue C-027)
   - Compare with any "Table N.N" references in the body text — the numbering should match

3. **Image fileref uniqueness**: Check that different figures show different images:
   - Visually confirm that figures across chapters show unique content (not all identical)
   - If multiple figures show the same placeholder or identical image, suspect fileref collapse (Known issue C-025)

4. **Chapter metadata formatting**: Navigate to 2-3 chapter landing pages that show metadata (copyright, authors, affiliations):
   - Check if author names have superscript affiliation markers (e.g., "Catherine J. Hwang¹") rendered inline
   - Check if "(eds.)" appears inline with editor names, not on a separate line
   - Check if "Email:" label is on the same line as the email address
   - If metadata elements are broken into separate lines/paragraphs losing inline structure, flag as formatting issue (Known issue C-028)

5. **Ordered vs unordered list rendering**: In chapters with numbered steps or procedures:
   - Check if numbered lists render with actual numbers (1, 2, 3...) or with bullets
   - If a list should be numbered but shows bullets, the converter misclassified `<ol>` (Known issue C-017)
   - Check at least 3 different chapters for list rendering

### Step 5b: Content Loss Detection (Browser-Level)

This step performs systematic checks for silently dropped content — the #1 issue that browser-only scanning misses.

1. **Paragraph count spot-check**: For 2-3 sample chapters, compare the visible paragraph count on the rendered page against what you'd expect from a medical/scientific chapter of that length.
   - Use `read_page` to count `<p>` elements in the body content area
   - Chapters with fewer than 5 paragraphs (excluding references) are suspicious unless they are introductory/landing pages
   - Compare across chapters: if most chapters have 30-50 paragraphs but one has only 3, investigate

2. **Image presence verification**: For each sampled chapter:
   - Count rendered images using `find` tool with query "images" or `read_page` looking for `<img>` tags
   - Cross-reference against figure references in the text (e.g., if text says "see Fig. 1.3" but only 2 figures are visible, content may be lost)
   - Check that figure numbering is sequential (gaps like Fig 1.1, Fig 1.3 with no Fig 1.2 suggest a dropped figure)

3. **Table presence verification**: Same approach as images:
   - Count rendered tables
   - Check for sequential numbering gaps
   - Verify table references in text match actual tables on the page

4. **Bibliography completeness spot-check**: For 2 reference sections:
   - Note the highest reference number cited in the body text (e.g., body cites "[42]")
   - Navigate to the references section and check if that reference number exists
   - If body cites [42] but references only go to [35], content was dropped

5. **Callout/admonition presence**: Check if the book has any styled callout boxes (Tip, Note, Warning, Pearl, Key Point, etc.):
   - If the book topic suggests clinical tips/pearls should exist (medical books typically have these) but none appear, suspect that callout styling was lost (C-003) or entire callout blocks were dropped
   - Compare: are there any `<div>` or `<aside>` elements with callout-like styling vs plain paragraphs with callout keywords in the text?

6. **Footnote/endnote verification**: Check if any superscript numbers in body text correspond to actual footnotes:
   - Look for superscript numbers (¹, ², ³ or `<sup>1</sup>`) in body text
   - Verify that corresponding footnote content exists (either at page bottom or as endnotes)
   - Missing footnote content with orphaned superscript markers indicates content loss

### Step 6: Tables and Figures

1. **Find pages with tables** (look for "View Table" buttons or inline tables).
2. **Check tables**:
   - Does the table render as a proper HTML table with rows/columns, or as plain text?
   - Table numbering: Does it use chapter-prefixed numbering (e.g., "Table 1.1") or flat numbering ("Table 1")?
   - Are layout/informal tables incorrectly numbered? (They should have no number)
   - Table captions/titles present and correct?
3. **Find pages with figures/images**:
   - Do images render? Check for broken image icons.
   - Are "Save Image", "Enlarge Image", "Direct Link Image" buttons present and functional?
   - Alt text present?
   - Figure numbering: Chapter-prefixed or flat?

### Step 7: Bibliography and References Testing

1. **Navigate to at least 2 reference sections** from different chapters.
2. **Check bibliography entries for**:
   - **Double periods**: `..` before DOI links (Known issue C-006)
   - **PubMed/Crossref links**: Are these actual hyperlinks (`<ulink>`) or just plain text? If plain text, the URLs were dropped during conversion. (Known issue C-012)
   - **Label concatenation**: "CrossrefPubMedPubMed Central" run together without spaces/separators? (Known issue C-012)
   - **Empty entries**: Are any reference entries completely empty (just a number with no content)? (Known issue C-015)
   - **DOI URL spacing**: Does the display text show "doi. org" instead of "doi.org"? (Known issue C-013)
   - **Spaces inside DOI paths**: e.g., `s 12893-024-02618-6` instead of `s12893-024-02618-6`
3. **Use `read_page`** to verify link `href` values for any Crossref/PubMed links that appear.

### Step 8: Navigation Testing

1. **Test Previous/Next navigation** between sections — does it follow the correct sequence?
2. **Test breadcrumb links** — do they navigate to the correct parent pages?
3. **Test cross-chapter references** if any exist in the body text.
4. **Test Part landing pages**: Do they show chapter listings? Or are they empty/broken? (Known issue R-006)

---

### IMPORTANT REMINDERS for Phase 1

- **Wait 2-3 seconds between page navigations** to avoid rate limiting.
- **Search/index term links are INTENTIONAL** — links to `/search?q=term` for drugs, diseases, PMID, and MeSH headings are a platform feature, not a bug. Do NOT report these.
- **Missing index/backmatter is INTENTIONAL** — the index is intentionally dropped during conversion. Do NOT report this.
- **Use `read_page` with `filter: interactive`** as the primary tool for checking link destinations.
- **If JavaScript execution is blocked**, fall back to `read_page` and `find` tools instead.
- **Record the publisher name** — this is needed for Phase 2 pattern lookup and the report.

---

## PHASE 2: Source File Validation (Optional)

Perform this phase only if source XHTML and converted XML zip files are provided.

### Step 8: Extract and Explore Files

1. **Extract both zip files** to a temp directory:
   ```bash
   mkdir -p /tmp/book_qa/source /tmp/book_qa/converted
   unzip -o <zip1> -d /tmp/book_qa/source
   unzip -o <zip2> -d /tmp/book_qa/converted
   ```

2. **Auto-detect which zip is source vs converted**:
   - **Source (publisher XHTML)**: Contains `.xhtml` files, typically under `OEBPS/html/`, has `content.opf` or `toc.ncx`
   - **Converted (DocBook XML)**: Contains files matching `sect1.{ISBN}.ch*.xml`, `preface.{ISBN}.pr*.xml`, `book.{ISBN}.xml`
   - If naming is ambiguous, check file content: source has HTML tags (`<div>`, `<span>`, `<h2>`), converted has DocBook tags (`<sect1>`, `<para>`, `<emphasis>`)

3. **List file structures** to understand the layout of both directories.

4. **Map file correspondences**: Understand which source files map to which converted files. Reference `../_shared/publisher-patterns.md` for publisher-specific file naming conventions.

### Step 9: Validate Browser Findings Against Files

For each issue found in Phase 1, **launch parallel validation agents** to check both source and converted files.

#### General Validation Approach

For every issue, follow this pattern:
1. **Find the issue in the converted XML** — use grep/search to locate the exact element
2. **Find the corresponding content in the source XHTML** — locate the same content in the publisher's original
3. **Compare**: Is the issue present in the source? Or was it introduced during conversion?
4. **Classify**:
   - Issue already in source → **Publisher Source Issue**
   - Source correct, converted wrong → **Converter Issue**
   - Both correct, browser wrong → **Platform Issue**

#### Common Checks to Run

| Check | What to Look For |
|-------|-----------------|
| **Heading spacing** | Compare inline elements around section numbers — is whitespace preserved from source to converted? |
| **Empty brackets** | Check if brackets are empty in both source TOC and body headings, or only in TOC |
| **Metadata concatenation** | Check if separate metadata elements in source were flattened into one element in converted |
| **Callout/admonition styling** | Check if source uses styled containers (divs with semantic classes) that were lost in conversion |
| **Table numbering** | Compare table IDs and captions between source and converted — formal vs informal distinction |
| **Encoding** | Byte-level check for double-encoding (e.g., NBSP `C2 A0` → `C3 82 C2 A0`) |
| **Zero-width spaces** | Check if `\u200b` characters in source became visible spaces in converted output |

### Step 10: Broader File Comparison

Run these automated checks across ALL files to find issues not visible in the browser. **These are CRITICAL** — many issues are invisible in browser screenshots but are immediately detectable via file-level pattern matching.

#### 10a. Missing Spaces Around Inline Elements (HIGH PRIORITY — C-011)
This is often the **most widespread issue** — check for text running into `<emphasis>` tags:
```bash
# Missing space BEFORE <emphasis> — letter/digit immediately before opening tag
grep -rcP '[a-zA-Z0-9]<emphasis' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
# Missing space AFTER </emphasis> — letter/digit immediately after closing tag
grep -rcP '</emphasis>[a-zA-Z0-9]' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
Show 5+ examples with surrounding context. Count total instances and affected files.

#### 10b. Double Periods in References (C-006)
```bash
grep -rc '\.\.' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
Exclude image file paths (.jpg, .png) from counts.

#### 10c. Period-Space in Abbreviations (C-013)
```bash
# Check for split abbreviations — a period-space regex in post-processing often causes this
grep -rP 'i\. e[^.]' /tmp/book_qa/converted/ --include="*.xml"
grep -rP 'e\. g[^.]' /tmp/book_qa/converted/ --include="*.xml"
grep -r 'doi\. org' /tmp/book_qa/converted/ --include="*.xml"
```

#### 10d. Empty Bibliography Entries (C-015)
```bash
# Check for self-closing bibliomixed (empty references — content lost)
grep -r '<bibliomixed [^>]*/>' /tmp/book_qa/converted/ --include="*.xml"
# Also check bibliomixed with no text between tags
grep -rP '<bibliomixed[^>]*>\s*</bibliomixed>' /tmp/book_qa/converted/ --include="*.xml"
```

#### 10e. PubMed/Crossref Links — URLs Preserved or Dropped? (C-012)
```bash
# Count PubMed/Crossref as plain text (no ulink wrapper)
grep -rc 'PubMed\|Crossref\|CrossRef' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
# Count PubMed/Crossref inside ulink (URLs preserved)
grep -rc 'ulink.*PubMed\|ulink.*Crossref' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
If the first count >> second count, URLs were dropped.

#### 10f. Stray Affiliation/Footnote Markers (C-014)
```bash
# Check for superscript+ContactIcon pattern in copyright blocks
grep -r 'ContactIcon' /tmp/book_qa/converted/ --include="*.xml"
grep -rP '<superscript>\d+.*\(\d+\)' /tmp/book_qa/converted/ --include="*.xml"
```

#### 10g. Encoding Artifacts (C-007)
```bash
# Â character (double-encoded NBSP)
grep -rn 'Â' /tmp/book_qa/converted/ --include="*.xml"
# â€" (double-encoded em-dash), â€™ (double-encoded right single quote)
grep -rn 'â€' /tmp/book_qa/converted/ --include="*.xml"
```

#### 10h. Zero-Width Space Artifacts (C-005)
```bash
# Search for U+200B in converted files
grep -rcP '\x{200b}' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```

#### 10i. Content Comparison
- Compare image counts between source and converted files
- Compare table counts
- Compare citation/bibliography entry counts — **count source `<a>` links in references vs converted `<bibliomixed>` entries**
- Check for missing sections (but remember: **missing index is intentional**)

#### 10j. Formatting Preservation
- Count bold, italic, superscript, subscript elements in both source and converted
- Report any significant drops (>5% loss)

#### 10k. Broken URLs
```bash
grep -rP 'url="https?://\."' /tmp/book_qa/converted/ --include="*.xml"
```

#### 10l. Part Page Content
```bash
# Check if part pages have actual content or just metadata
for f in /tmp/book_qa/converted/*/part.*.xml; do wc -l "$f"; done
```

#### 10m. BibSection Content Loss — "Recommended/Further Reading" (C-016)
This is a HIGH-PRIORITY check — silently dropped content is the hardest to detect.
```bash
# Check source for BibSection (Springer's Recommended/Further Reading container)
grep -rn 'BibSection\|Recommended Reading\|Further Reading\|Suggested Reading' /tmp/book_qa/source/ --include="*.xhtml"
# Check converted for the same text — if source has it but converted doesn't, content was dropped
grep -rn 'Recommended Reading\|Further Reading\|Suggested Reading' /tmp/book_qa/converted/ --include="*.xml"
```
Compare counts. If source > converted, flag as **content loss (C-016)**.

#### 10n. Ordered List Misclassification (C-017)
```bash
# Count ordered lists in source
grep -rc '<ol ' /tmp/book_qa/source/ --include="*.xhtml" | grep -v ':0$' | grep -v 'BibliographyWrapper\|TocChapter'
# Count orderedlist in converted (should match source ol count, excluding bibliography/TOC)
grep -rc '<orderedlist' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
# Count itemizedlist in converted (check if some of these should be orderedlist)
grep -rc '<itemizedlist' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
If source has `<ol>` elements but converted has 0 `<orderedlist>`, the converter is misclassifying ordered lists.

#### 10o. Bibliography Entry Count Per Chapter (Content Loss Detection)
```bash
# Count citations per chapter in source
for f in /tmp/book_qa/source/OEBPS/html/*Chapter*.xhtml; do
  chapter=$(basename "$f")
  count=$(grep -c 'class="Citation"' "$f" 2>/dev/null || echo 0)
  echo "$chapter: $count citations"
done
# Count bibliomixed per chapter in converted
for f in /tmp/book_qa/converted/*/sect1.*s0*.xml; do
  if grep -q 'bibliomixed' "$f" 2>/dev/null; then
    count=$(grep -c 'bibliomixed' "$f")
    echo "$(basename $f): $count entries"
  fi
done
```
Compare per-chapter counts. Any source > converted difference indicates dropped bibliography entries.

#### 10p. Image Count Comparison (Content Loss Detection)
```bash
# Count images in source per chapter
for f in /tmp/book_qa/source/OEBPS/html/*Chapter*.xhtml; do
  chapter=$(basename "$f")
  count=$(grep -c '<img ' "$f" 2>/dev/null || echo 0)
  echo "$chapter: $count images"
done
# Count imagedata in converted per chapter
for f in /tmp/book_qa/converted/*/sect1.*s0*.xml; do
  if grep -q 'imagedata' "$f" 2>/dev/null; then
    count=$(grep -c 'imagedata' "$f")
    echo "$(basename $f): $count images"
  fi
done
```
Compare per-chapter image counts. Source > converted indicates dropped figures. Also check that all source image files exist in the converted output's image directory.

#### 10q. Table Count Comparison (Content Loss Detection)
```bash
# Count tables in source per chapter
for f in /tmp/book_qa/source/OEBPS/html/*Chapter*.xhtml; do
  chapter=$(basename "$f")
  formal=$(grep -c 'id="Tab[0-9]' "$f" 2>/dev/null || echo 0)
  informal=$(grep -c 'id="Tab[a-z]' "$f" 2>/dev/null || echo 0)
  echo "$chapter: $formal formal + $informal informal tables"
done
# Count tables in converted
for f in /tmp/book_qa/converted/*/*.xml; do
  tbl=$(grep -c '<table\b' "$f" 2>/dev/null || echo 0)
  itbl=$(grep -c '<informaltable\b' "$f" 2>/dev/null || echo 0)
  if [ "$tbl" -gt 0 ] || [ "$itbl" -gt 0 ]; then
    echo "$(basename $f): $tbl table + $itbl informaltable"
  fi
done
```
Compare total table counts per chapter. Also verify formal/informal distinction is preserved.

#### 10r. Callout Box / Admonition Detection (Content Loss Detection)
```bash
# Count callout boxes in source (Springer pattern)
grep -rc 'FormalPara\|ParaType\|class="note"\|class="tip"\|class="warning"\|class="sidebar"' \
  /tmp/book_qa/source/ --include="*.xhtml" | grep -v ':0$'
# Count admonition elements in converted
grep -rc '<sidebar\|<tip\|<note\|<important\|<warning\|<caution' \
  /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
If source has callout boxes but converted has 0 admonition elements, callout styling was lost (C-003) or callouts were entirely dropped.

#### 10s. Footnote/Endnote Count Comparison (Content Loss Detection)
```bash
# Count footnotes in source
grep -rc 'epub:type="footnote"\|class="Footnote"\|class="fn"' \
  /tmp/book_qa/source/ --include="*.xhtml" | grep -v ':0$'
# Count footnotes in converted
grep -rc '<footnote' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
Missing footnotes in converted output when source has them indicates content loss. Also check for orphaned superscript footnote markers without corresponding footnote content.

#### 10t. Paragraph/Word Count Comparison (Content Loss Detection)
```bash
# Rough paragraph count comparison per chapter
echo "=== Source paragraph counts ==="
for f in /tmp/book_qa/source/OEBPS/html/*Chapter*.xhtml; do
  count=$(grep -c '<p ' "$f" 2>/dev/null || echo 0)
  echo "$(basename $f): $count paragraphs"
done
echo "=== Converted paragraph counts ==="
for f in /tmp/book_qa/converted/*/sect1.*.xml; do
  count=$(grep -c '<para' "$f" 2>/dev/null || echo 0)
  if [ "$count" -gt 0 ]; then
    echo "$(basename $f): $count paras"
  fi
done
```
A chapter with significantly fewer paragraphs in converted vs source indicates content loss. Allow for some difference (converted may split or merge paragraphs), but >20% drop in paragraph count is suspicious.

#### 10u. Section/Heading Count Comparison (Content Loss Detection)
```bash
# Count headings per chapter in source
for f in /tmp/book_qa/source/OEBPS/html/*Chapter*.xhtml; do
  h2=$(grep -c '<h2' "$f" 2>/dev/null || echo 0)
  h3=$(grep -c '<h3' "$f" 2>/dev/null || echo 0)
  echo "$(basename $f): $h2 h2 + $h3 h3 headings"
done
# Count section elements in converted
for f in /tmp/book_qa/converted/*/sect1.*.xml; do
  s1=$(grep -c '<sect1' "$f" 2>/dev/null || echo 0)
  s2=$(grep -c '<sect2' "$f" 2>/dev/null || echo 0)
  echo "$(basename $f): $s1 sect1 + $s2 sect2"
done
```
Each source heading should correspond to a section element in the converted output. Missing sections indicate structural content loss.

#### 10v. Figure Reference Integrity (Cross-Reference Validation)
```bash
# Find all figure references in converted text
grep -rnoP 'Fig\.\s*\d+[\.\d]*|Figure\s+\d+[\.\d]*' /tmp/book_qa/converted/ --include="*.xml" | head -30
# Find all actual figure elements
grep -rn '<figure' /tmp/book_qa/converted/ --include="*.xml" | head -30
# Check for figure IDs
grep -roP 'id="[^"]*[Ff]ig[^"]*"' /tmp/book_qa/converted/ --include="*.xml"
```
Cross-reference: every "Fig. N" mention in text should have a corresponding `<figure>` with matching ID in the same chapter. Report any orphaned references (text mentions a figure that doesn't exist).

#### 10w. Table Reference Integrity (Cross-Reference Validation)
```bash
# Find all table references in converted text
grep -rnoP 'Table\s+\d+[\.\d]*' /tmp/book_qa/converted/ --include="*.xml" | head -30
# Find all actual table elements with IDs
grep -roP 'id="[^"]*[Tt]ab[^"]*"' /tmp/book_qa/converted/ --include="*.xml"
```
Cross-reference: every "Table N" mention in text should have a corresponding `<table>` or `<informaltable>` element. Report orphaned references.

#### 10x. Internal Cross-Reference Link Validation
```bash
# Find all internal xref/link elements in converted
grep -roP '<xref linkend="[^"]*"' /tmp/book_qa/converted/ --include="*.xml" | head -30
# Find all anchor IDs that xrefs point to
grep -roP 'id="[^"]*"' /tmp/book_qa/converted/ --include="*.xml" > /tmp/book_qa/all_ids.txt
# Check for dangling references (xref targets that don't exist)
grep -roP 'linkend="([^"]*)"' /tmp/book_qa/converted/ --include="*.xml" | \
  sed 's/.*linkend="//;s/"//' | sort -u > /tmp/book_qa/xref_targets.txt
```
Compare xref targets against available IDs. Any target not found in the ID list is a dangling cross-reference.

#### 10y. Citation-to-Bibliography Link Validation
```bash
# Check that citation links in body text point to valid bibliography entries
# Find all in-text citation links
grep -roP 'linkend="CR\d+"' /tmp/book_qa/converted/ --include="*.xml" | \
  sed 's/.*linkend="//;s/"//' | sort -u > /tmp/book_qa/citation_links.txt
# Find all bibliography entry IDs
grep -roP '<bibliomixed[^>]*id="([^"]*)"' /tmp/book_qa/converted/ --include="*.xml" | \
  sed 's/.*id="//;s/"//' | sort -u > /tmp/book_qa/bib_ids.txt
# Show citations that don't have matching bibliography entries
comm -23 /tmp/book_qa/citation_links.txt /tmp/book_qa/bib_ids.txt
```
Any citation link without a matching bibliography entry ID indicates a broken reference chain. The reader clicks a citation number but it doesn't resolve to the reference.

#### 10z. Equation Reference Integrity (Cross-Reference Validation)
```bash
# Find equation references in text
grep -rnoP 'Eq\.\s*\d+[\.\d]*|Equation\s+\d+[\.\d]*' /tmp/book_qa/converted/ --include="*.xml" | head -20
# Find actual equation elements (may be in mediaobject, informalequation, or equation tags)
grep -rc '<equation\|<informalequation\|role="equation"' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
Books with mathematical content should have equation references that resolve to actual equation elements. Report orphaned references.

#### 10aa. Image Fileref Uniqueness (C-025)
```bash
# Count unique filerefs across all XML files
grep -rho 'fileref="[^"]*"' /tmp/book_qa/converted/ --include="*.xml" | sort -u | wc -l
# Count total imagedata elements
grep -rc 'imagedata' /tmp/book_qa/converted/ --include="*.xml" | awk -F: '{sum+=$2} END {print sum}'
# Check for suspiciously small image files (1x1 placeholders are ~631 bytes)
find /tmp/book_qa/converted/ -name "*.jpg" -size -1k -exec ls -la {} \;
```
If unique filerefs << total imagedata elements, images have collapsed to shared filenames. Check if MultiMedia files are 1x1 pixel placeholders (631 bytes = placeholder).

#### 10ab. Table Cross-Reference Registration (C-026)
```bash
# Check for table references degraded to <phrase> (should be <link>)
grep -rc '<emphasis role="InternalRef"><phrase>' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
# Check for table references that ARE properly linked
grep -rc '<emphasis role="InternalRef"><link' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
# Check source for InternalRef patterns pointing to tables
grep -rn 'InternalRef.*Tab\|InternalRef.*tab' /tmp/book_qa/source/ --include="*.xhtml" | head -10
```
If `<phrase>` count >> `<link>` count for InternalRef elements, table cross-references are being degraded to plain text.

#### 10ac. Table Caption Content (C-027)
```bash
# Check for empty table titles (missing caption extraction)
grep -rn '<table[^>]*>.*<title/>' /tmp/book_qa/converted/ --include="*.xml" | head -10
grep -rn '<table[^>]*>.*<title></title>' /tmp/book_qa/converted/ --include="*.xml" | head -10
# Check for generic fallback titles
grep -rn '<title>Table [0-9]*</title>' /tmp/book_qa/converted/ --include="*.xml" | head -10
# Compare with source CaptionNumber spans
grep -rc 'CaptionNumber' /tmp/book_qa/source/ --include="*.xhtml" | grep -v ':0$'
```
If source has CaptionNumber spans but converted has empty/generic titles, caption extraction failed for the adjacent-caption path.

#### 10ad. Ordered List Preservation (C-017)
```bash
# Count ordered lists in source (excluding bibliography/TOC wrappers)
grep -rc '<ol ' /tmp/book_qa/source/ --include="*.xhtml" | grep -v ':0$' | grep -v 'BibliographyWrapper\|TocChapter'
# Count orderedlist in converted
grep -rc '<orderedlist' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
# Count itemizedlist in converted
grep -rc '<itemizedlist' /tmp/book_qa/converted/ --include="*.xml" | grep -v ':0$'
```
If source has many `<ol>` elements but converted has 0 `<orderedlist>`, the converter is misclassifying ordered lists as itemized lists.

#### 10ae. Chapter Metadata Formatting Quality (C-028)
```bash
# Check metadata sect1 content — are inline elements preserved or flattened to separate paras?
for f in /tmp/book_qa/converted/*/ch*.xml; do
  # Find metadata sect1 and count para elements inside it
  meta_paras=$(grep -A100 'role="auto-generated\|ChapterCopyright\|AuthorGroup' "$f" | grep -c '<para' 2>/dev/null || echo 0)
  if [ "$meta_paras" -gt 10 ]; then
    echo "SUSPECT: $(basename $f) metadata sect1 has $meta_paras paras (likely fragmented)"
  fi
done
# Check for superscript markers that should be inline but are separate paras
grep -rn '<para>.*<superscript>' /tmp/book_qa/converted/ --include="*.xml" | \
  grep -v '<para>.*[A-Za-z].*<superscript>' | head -10
# Check for "(eds.)" or "Email:" as standalone paras (should be inline)
grep -rn '<para>\s*(eds\.)\|<para>\s*Email:' /tmp/book_qa/converted/ --include="*.xml" | head -10
```
If metadata elements (author names, superscript affiliations, editor markers, email addresses) are broken into separate `<para>` elements instead of being inline within a single paragraph, the converter lost inline structure during metadata block processing.

#### 10af. Citation-to-Bibliography ID Scheme Match (C-024)
```bash
# Extract citation link targets from body text
grep -roP 'linkend="CR\d+"' /tmp/book_qa/converted/ --include="*.xml" | head -10
# Extract bibliography entry IDs
grep -roP '<bibliomixed[^>]*id="[^"]*"' /tmp/book_qa/converted/ --include="*.xml" | head -10
# Check for ID scheme mismatch (bare CR4 vs ch0009s0013_CR4)
# Citation links use: linkend="CR4"
# Bibliography entries use: id="ch0009s0013_CR4"
```
If citation linkends are bare (e.g., `CR4`) but bibliography IDs are prefixed (e.g., `ch0009s0013_CR4`), the ID scheme mismatch breaks all citation navigation.

---

## PHASE 3: Report Generation

### Step 11: Compile the QA Report

Generate a structured report using the template from `../_shared/report-template.md`:

```markdown
## QA Report: [Book Title]
**ISBN:** [ISBN]
**Author:** [Author]
**Publisher:** [Publisher], [Year], [Edition]
**Publisher ID:** [springer / wiley / elsevier / etc.]
**Platform:** R2 Digital Library (Staging/Production)
**URL:** [URL]
**Date:** [Date]
**Chapters Sampled:** [List]

---

### CRITICAL / HIGH SEVERITY

#### [N]. [Finding Title]
- **Severity:** HIGH
- **Source:** [Converter / Publisher (specify) / Platform]
- **Scope:** [Systemic / Isolated] ([details])
- **Known Issue ID:** [C-NNN / P-NNN / R-NNN from known-issues.md, or "NEW"]
- **Description:** [What the issue looks like]
- **Root Cause:** [Technical explanation with file evidence]
- **Example:**
  - Original: `[source markup]`
  - Converted: `[converted markup]`
  - Rendered: [what appears on screen]

### MEDIUM SEVERITY
[Same structure]

### LOW SEVERITY
[Same structure]

### VERIFIED WORKING CORRECTLY
- [List items confirmed working]

### SUMMARY TABLE
| # | Finding | Source | Severity | Scope | Known Issue |
|---|---------|--------|----------|-------|-------------|
| 1 | ... | Converter | HIGH | Systemic | C-001 |
```

### Classification Guidelines

**Severity:**
- **HIGH**: Affects readability, navigation, or content accuracy across multiple chapters
- **MEDIUM**: Visual/formatting issues that don't block comprehension
- **LOW**: Minor cosmetic issues, isolated occurrences

**Source Attribution:**
- **Converter**: Issue introduced by the XML conversion pipeline
- **Publisher** (specify which): Issue present in the publisher's source files
- **Platform**: Issue in the R2 rendering engine (not the XML data)

**Known Non-Issues (DO NOT REPORT):**
- Search/index term links (`/search?q=term`) — these are intentional
- Missing index/backmatter — intentionally dropped during conversion
- Zero-width spaces in the original source used for line-break hints (but DO report if they become visible spaces in the conversion)

### After the QA Report

Recommend next steps based on findings:
- **If converter issues found** → Suggest running `r2-diagnosis` skill with source + converted files for deep root-cause analysis
- **If only platform issues found** → Escalate to platform team
- **If only publisher issues found** → Document for publisher feedback

---

## Reference Files

For publisher-specific patterns, known issues, and report templates, see:
- [`../_shared/publisher-patterns.md`](../_shared/publisher-patterns.md) — Multi-publisher XHTML pattern reference
- [`../_shared/known-issues.md`](../_shared/known-issues.md) — Known platform and converter issues catalog
- [`../_shared/report-template.md`](../_shared/report-template.md) — Full QA report template
