# Post-Fix Verification QA Report: Anatomy of the Upper and Lower Limbs

**ISBN:** 9783031975134
**Author:** Andrew Zbar
**Publisher:** Springer Nature, 2025, 1st Edition
**Platform:** R2 Digital Library (Staging)
**URL:** https://stage.r2library.com/Resource/Title/9783031975134
**Date:** 2026-03-09
**Report Type:** Post-Fix Verification — confirming previously applied XML fixes
**Chapters Sampled:** Ch 1 (ch0002), Ch 3 (ch0004), Ch 5 (ch0006), Ch 9 (ch0010), Ch 11 (ch0012), Ch 12 (ch0013), Ch 13 (ch0014), Ch 15 (ch0016)

---

## FIXES VERIFIED — CONFIRMED RESOLVED

### 1. C-005 — Zero-Width Space in DOI URLs
- **Status:** FIXED ✅
- **Verification:** Checked references in ch0004s0006. DOI URLs display as clean "doi.org" without visible ZWS characters. URL paths are clean (e.g., `10.2290/jcm7040086` — no stray spaces).
- **Pages checked:** ch0004s0006

### 2. C-006 — Double Periods Before DOI Links
- **Status:** FIXED ✅
- **Verification:** Reviewed bibliography entries in ch0004s0006. No instances of ".." before DOI links. Single periods used correctly throughout.
- **Pages checked:** ch0004s0006

### 3. C-011 — Fused Words Around Inline Formatting
- **Status:** FIXED ✅
- **Verification:** Extensive sampling across 8 chapters. Italic terms (*extensor hallucis brevis*, *Gluteal surface*, *posterior gluteal line*, *middle gluteal line*, *inferior gluteal line*, *Clunis*, *left-hand image*, *right-hand image*, *EDL*, *EHL*, *FHL*, *FDL*, *TP*) all have proper spacing with surrounding text. Bold text properly separated. No fused words detected anywhere.
- **Pages checked:** ch0002s0002, ch0006s0002, ch0010s0002, ch0012s0002, ch0013s0005, ch0014s0002, ch0016s0004, ch0016s0006

### 4. C-012 — PubMed/Crossref Links as Plain Text
- **Status:** FIXED ✅
- **Verification:** In ch0004s0006, PubMed, Crossref, and PubMed Central labels are:
  - Actual hyperlinks (not plain text) with external link icons
  - DOI URLs point to `https://doi.org/...`
  - PubMed URLs point to `http://www.ncbi.nlm.nih.gov/pubmed/...` with proper parameters
  - PubMed Central URLs point to `http://www.ncbi.nlm.nih.gov/pmc/articles/...`
- **Pages checked:** ch0004s0006

### 5. C-012 — Reference Label Concatenation ("CrossrefPubMedPubMed Central")
- **Status:** FIXED ✅
- **Verification:** Labels are properly separated as distinct hyperlinks with visual spacing between them. No concatenated "CrossrefPubMedPubMed Central" strings found.
- **Pages checked:** ch0004s0006

### 6. C-017 — Ordered List Misclassification (ol → itemizedlist)
- **Status:** FIXED ✅ (classification correct, but see new issue below)
- **Verification:** In ch0004s0005, an ordered list with 6 items renders with proper numbering (1-6) instead of bullets. The `<ol>` → `<orderedlist>` conversion is working correctly.
- **Pages checked:** ch0004s0005, ch0014s0002

### 7. DOI URL Spacing ("doi. org" → "doi.org")
- **Status:** FIXED ✅
- **Verification:** DOI URLs on copyright page (pr0003) and in references display as "doi.org" with no stray spaces.
- **Pages checked:** pr0003, ch0004s0006

### 8. Ampersand Escaping in URLs
- **Status:** FIXED ✅
- **Verification:** PubMed URLs contain properly escaped `&` in query parameters (e.g., `cmd=Retrieve&db=PubMed&dopt=Abstract&list_uids=29690525`). Links function correctly.
- **Pages checked:** ch0004s0006

### 9. Copyright Metadata Separation
- **Status:** FIXED ✅
- **Verification:** On pr0003, copyright metadata is properly formatted:
  - Author name on separate line from copyright text
  - ISBNs on separate lines, not concatenated
  - Copyright year not fused with author name
- **Pages checked:** pr0003

---

## NEW FINDINGS — ISSUES STILL PRESENT OR NEWLY DETECTED

### Finding 1: Duplicate Ordered List Numbering
- **Severity:** MEDIUM
- **Source:** Converter
- **Scope:** Systemic — affects all ordered lists where source text contains number prefixes
- **Known Issue ID:** NEW (related to C-017 fix)
- **Description:** Ordered lists display duplicate numbering: "1. 1.", "2. 2.", "3. 3.", etc. The `<orderedlist>` element causes the platform to auto-render numbers (1., 2., 3.), but the list item text ALSO contains the original number prefix from the source XHTML. The fix for C-017 correctly converted `<ol>` to `<orderedlist>`, but did not strip the leading number prefix from the list item content.
- **Example:**
  - Rendered: `1. 1. The posterior rami of the upper 3 lumbar nerves...`
  - Expected: `1. The posterior rami of the upper 3 lumbar nerves...`
- **Location:** ch0014s0002 (Chapter 13, Anatomy of the Gluteal Region)
- **Fix needed:** When converting `<ol>` list items to `<orderedlist><listitem>`, strip leading number/letter prefixes (e.g., `1.`, `2.`, `a.`, `b.`, `(i)`, `(ii)`) from the list item text content.

### Finding 2: Footnotes Consolidated Into Single Element Per Chapter
- **Severity:** MEDIUM
- **Source:** Converter
- **Scope:** Systemic — affects all 14 chapters with footnotes
- **Known Issue ID:** NEW (C-024)
- **Description:** The converter wraps ALL footnotes from a chapter's `FootnoteSection` into a single `<footnote>` element, instead of creating separate `<footnote>` elements per source footnote. The individual footnote text content IS preserved (visible via `<emphasis role="FootnoteNumber">` markers inside the single footnote), but structurally they are merged.
- **Content loss:** NO — all footnote text content is present. The FootnoteNumber markers match source noterefs (Ch 1: 2/2, Ch 2: 3/3, Ch 6: 4/4, Ch 7: 4/4, Ch 12: 7+/7, Ch 16: 5/5).
- **Structural impact:** Footnotes are placed as a single block at the end of the last content section rather than inline at their reference points.
- **Example:**
  - Source: 2 separate `<div class="Footnote">` divs (Fn1: Erb-Duchenne, Fn2: Martin-Gruber)
  - Converted: 1 `<footnote id="ch0002s0002fn0001">` containing both footnotes concatenated
- **Fix needed:** The converter should create individual `<footnote>` elements and either place them inline at their `noteref` reference points, or create separate `<footnote>` elements with proper IDs.

### Finding 3: "a. k. a." Abbreviation Spacing (Converter Issue — Confirmed)
- **Severity:** LOW
- **Source:** Converter (C-013)
- **Scope:** 5 instances across 5 chapters
- **Known Issue ID:** C-013
- **Description:** The abbreviation "a.k.a." in the source XHTML is converted to "a. k. a." with spaces after each period. File-level comparison confirms the source has "a.k.a." (correct) while the converted XML has "a. k. a." (incorrect). The converter's period-space normalization regex is inserting spaces after periods within abbreviations.
- **Affected files:**
  - `ch0002s0002.xml` (Ch 1) — "posterior femoral cutaneous nerve a. k. a."
  - `ch0003s0005.xml` (Ch 2) — "Nerve to Serratus Anterior (a. k. a. the long thoracic nerve)"
  - `ch0004s0004.xml` (Ch 3) — "thoracodorsal nerve, a. k. a. the nerve to the latissimus dorsi"
  - `ch0013s0004.xml` (Ch 12) — "the adductor canal (a. k. a. subsartorial or Hunterian canal)"
  - `ch0014s0005.xml` (Ch 13) — "ligamentum teres (a. k. a. the ligament of the head of the femur)"

---

## PHASE 2: SOURCE-TO-CONVERTED FILE VALIDATION

### Content Loss Detection Results

| Check | Status | Details |
|-------|--------|---------|
| Image counts | **OK** | Converted has equal or more images per chapter (extra are inline equations/decorative). No images lost. |
| Table counts | **MATCH** | Source Ch 11: 2 tables → Converted ch0012: 2 tables |
| Paragraph counts | **OK** | Converted has more `<para>` elements (expected — DocBook wraps list items, cells, etc.) |
| Section/heading counts | **OK** | Source h2/h3 headings match converted sect1/sect2 counts with expected structural additions |
| Bibliography counts | **OK** | 33 PubMed/Crossref mentions, all 33 inside `<ulink>` — no plain text links |
| Callout/admonition | **MATCH** | Source: 2 FormalPara instances → Converted: 2 admonition elements |
| Footnotes | **STRUCTURAL ISSUE** | Content preserved but merged into 1 `<footnote>` per chapter (see Finding 2) |
| Recommended Reading | **N/A** | Not present in this book |
| Encoding artifacts | **CLEAN** | No stray `Â` or mojibake characters |
| Zero-width spaces | **CLEAN** | No U+200B residuals |
| Fused words (emphasis) | **CLEAN** | No missing spaces before/after `<emphasis>` |
| Double periods | **CLEAN** | No `..` in bibliography |
| Empty bibliography | **CLEAN** | No empty `<bibliomixed>` entries |
| Front matter files | **MATCH** | 6 source Frontmatter + Cover → 6 converted preface files |

### Cross-Reference Integrity

| Check | Status | Details |
|-------|--------|---------|
| Dangling xrefs | **CLEAN** | 396 `linkend` targets, 457 IDs — all 396 resolve correctly, 0 dangling |
| Figure elements | **OK** | 20+ `<figure>` elements with proper IDs (format: `ch####s####fg##`) |
| Broken URLs | **CLEAN** | No URLs ending with period, no URLs with spaces |
| DOI URL format | **CLEAN** | No spaces in DOI paths |

### URL/Link Validation

| Check | Status | Details |
|-------|--------|---------|
| PubMed/Crossref in `<ulink>` | **PASS** | 33/33 mentions wrapped in `<ulink>` — none as plain text |
| URL format | **PASS** | Crossref → `doi.org`, PubMed → `ncbi.nlm.nih.gov`, ampersands properly escaped |
| Abbreviation spacing (i.e., e.g.) | **PASS** | No instances of "i. e" or "e. g" |
| Abbreviation spacing (a.k.a.) | **FAIL** | 5 instances of "a. k. a." — converter introduces spaces (see Finding 3) |
| DOI spacing (doi.org) | **PASS** | No "doi. org" instances |

---

## ADDITIONAL OBSERVATIONS

### Images and Figures
- **Status:** Working correctly ✅
- Diagrams render properly (tested Diagram 13.1, 13.2 in ch0014s0002, Diagram 15.3 in ch0016s0004)
- "Save Image", "Enlarge Image", "Direct Link Image" buttons present and functional
- Figure cross-references ("Diagram 13.1", "Diagrams 15.3 and 15.4") are hyperlinked correctly

### Navigation
- **Status:** Working correctly ✅
- Previous/Next links function between sections
- Breadcrumb trail shows correct hierarchy (Book > Part > Chapter > Section)
- No duplicate breadcrumb entries observed

### Bold/Italic Formatting
- **Status:** Working correctly ✅
- Bold text for emphasis and mnemonic letters renders correctly
- Italic text for anatomical terms, Latin terms, and figure captions renders correctly
- No fused word issues around any inline formatting

### Cross-References
- **Status:** Working correctly ✅
- In-text citation links (e.g., [1], [2]) navigate to correct bibliography sections
- Diagram cross-references are hyperlinked and resolve to correct figure anchors
- External links (DOI, PubMed, PubMed Central) open correctly

---

## SUMMARY TABLE

| # | Finding | Status | Source | Severity | Known Issue |
|---|---------|--------|--------|----------|-------------|
| 1 | C-005: ZWS in DOI URLs | FIXED ✅ | — | — | C-005 |
| 2 | C-006: Double periods in refs | FIXED ✅ | — | — | C-006 |
| 3 | C-011: Fused words | FIXED ✅ | — | — | C-011 |
| 4 | C-012: PubMed/Crossref plain text | FIXED ✅ | — | — | C-012 |
| 5 | C-012: Label concatenation | FIXED ✅ | — | — | C-012 |
| 6 | C-017: Ordered list as bullets | FIXED ✅ | — | — | C-017 |
| 7 | DOI URL spacing | FIXED ✅ | — | — | C-005 |
| 8 | Ampersand escaping in URLs | FIXED ✅ | — | — | C-012 |
| 9 | Copyright metadata separation | FIXED ✅ | — | — | C-002/C-010 |
| 10 | **Duplicate ordered list numbering** | **NEW** | Converter | MEDIUM | NEW (C-024) |
| 11 | **Footnotes merged into single element** | **NEW** | Converter | MEDIUM | NEW (C-025) |
| 12 | "a. k. a." abbreviation spacing (5 instances) | **CONVERTER** | Converter | LOW | C-013 |

---

## VERDICT

**9 of 9 targeted fixes confirmed working correctly on staging.** The XML fixes applied to ISBN 9783031975134 are rendering as expected on the R2 platform.

**Phase 2 file validation confirms: No content loss detected.** All images, tables, paragraphs, sections, bibliography entries, and cross-references are preserved. All 396 cross-reference linkends resolve to valid IDs.

**2 new structural issues found:**
1. **Duplicate ordered list numbering** (MEDIUM) — C-017 fix correctly classified `<ol>` → `<orderedlist>`, but the source list items contain inline number prefixes ("1.", "2.") that duplicate the platform's auto-numbering. The source XHTML itself has this redundancy inside `<li>` elements.
2. **Footnote consolidation** (MEDIUM) — All footnotes per chapter are merged into a single `<footnote>` element instead of individual elements. Content is preserved but structure is lost.

**1 converter issue confirmed:**
- **"a. k. a." spacing** (LOW, C-013) — Source has "a.k.a." (correct), converter introduces "a. k. a." (incorrect). 5 instances across 5 chapters.

### Recommended Next Steps
1. **Fix duplicate list numbering** — Strip leading number/letter prefixes from `<listitem>` text when source `<ol>` items contain them. Detect pattern: `^\d+\.\s*` or `^[a-z]\.\s*` at start of list item text.
2. **Fix footnote handling** — Convert each `<div class="Footnote">` in `FootnoteSection` to individual `<footnote>` elements, placed inline at their `noteref` reference points.
3. **Fix C-013 abbreviation spacing** — The converter's period-space normalization should not insert spaces within known abbreviations (a.k.a., i.e., e.g., etc.).
