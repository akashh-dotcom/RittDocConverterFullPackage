# R2 Book QA Report Template

Copy and fill in this template when generating a QA report for a book.

---

## QA Report: [Book Title]

**ISBN:** [ISBN]
**Author:** [Author(s)]
**Publisher:** [Publisher], [Year], [Edition]
**Publisher ID:** [Springer Nature / Wiley / etc.]
**Platform:** R2 Digital Library ([Staging/Production])
**URL:** [Full URL]
**QA Date:** [YYYY-MM-DD]
**QA Phases Completed:** [Phase 1 Only / Phase 1 + Phase 2]
**Chapters Sampled (Phase 1):** [e.g., Ch 1, 5, 10, 14]
**Source Files (Phase 2):** [List zip files if Phase 2 was performed]

---

### Book Structure Overview

| Element | Count/Detail |
|---------|-------------|
| Total Chapters | [N] |
| Parts (if any) | [N or "None"] |
| Front Matter Pages | [pr0001 – pr000N] |
| Has Tables | [Yes/No, approx count] |
| Has Figures | [Yes/No, approx count] |
| Has Callout Boxes | [Yes/No, type names] |
| Has Bibliography | [Yes/No, per-chapter or end-of-book] |

---

### Publisher-Specific Notes

Document any publisher-specific patterns, markup conventions, or known quirks relevant to this book's QA:

- **Publisher**: [Springer Nature / Wiley / etc.]
- **Source format**: [EPUB / XHTML / other]
- **Known publisher patterns file**: See `../_shared/publisher-patterns.md` for publisher-specific markup rules
- **Publisher-specific issues observed**: [List any issues unique to this publisher's source markup, or "None"]
- **Applicable known issues**: [List known issue IDs from `../_shared/known-issues.md` that apply to this publisher, e.g., C-001, C-003, P-001]

---

### CRITICAL / HIGH SEVERITY

#### [N]. [Finding Title]
- **Severity:** HIGH
- **Source:** [Converter / Publisher / Platform]
- **Scope:** [Systemic (affects N chapters/sections) / Isolated (N instances)]
- **Known Issue ID:** [C-NNN / P-NNN / R-NNN, if matches a known issue]
- **Description:** [Clear description of what the issue looks like to the reader]
- **Root Cause:** [Technical explanation with file evidence]
- **Example:**
  - **Original (Source XHTML):**
    ```html
    [source markup snippet]
    ```
  - **Converted (DocBook XML):**
    ```xml
    [converted markup snippet]
    ```
  - **Rendered (Browser):** [What appears on screen, or screenshot reference]
- **Affected Pages:** [List specific page IDs or "all chapters"]

---

### MEDIUM SEVERITY

#### [N]. [Finding Title]
- **Severity:** MEDIUM
- **Source:** [Converter / Publisher / Platform]
- **Scope:** [Systemic / Isolated]
- **Known Issue ID:** [if applicable]
- **Description:** [...]
- **Root Cause:** [...]
- **Example:** [...]
- **Affected Pages:** [...]

---

### LOW SEVERITY

#### [N]. [Finding Title]
- **Severity:** LOW
- **Source:** [Converter / Publisher / Platform]
- **Scope:** [Systemic / Isolated]
- **Known Issue ID:** [if applicable]
- **Description:** [...]
- **Root Cause:** [...]
- **Example:** [...]

---

### VERIFIED WORKING CORRECTLY

List items that were specifically checked and confirmed to be working as expected:

- [ ] Title page metadata displays correctly
- [ ] TOC links resolve to correct sections
- [ ] Previous/Next navigation works between sections
- [ ] Tables render as proper HTML tables (not plain text)
- [ ] Images render with Save/Enlarge/Direct Link buttons
- [ ] Cross-chapter reference links resolve correctly
- [ ] Bibliography reference links point to correct entries
- [ ] Search/index term links function correctly (intentional feature)
- [ ] [Other items checked...]

---

### SUMMARY TABLE

| # | Finding | Source | Severity | Scope | Known Issue |
|---|---------|--------|----------|-------|-------------|
| 1 | [Title] | Converter | HIGH | Systemic | C-001 |
| 2 | [Title] | Publisher | MEDIUM | Systemic | P-001 |
| 3 | [Title] | Platform | LOW | Isolated | — |
| ... | ... | ... | ... | ... | ... |

---

### STATISTICS (Phase 2 Only)

If file-level comparison was performed, include these metrics:

| Metric | Source Count | Converted Count | Delta |
|--------|------------|----------------|-------|
| Total sections | | | |
| Tables (formal) | | | |
| Tables (informal) | | | |
| Figures | | | |
| Bibliography entries | | | |
| PubMed links | | | |
| CrossRef links | | | |
| Bold elements | | | |
| Italic elements | | | |
| Superscript elements | | | |
| Callout boxes (FormalPara) | | | |
| Admonition/callout elements | | | |
| Double periods (..) | 0 | [N] | +[N] |
| Zero-width space issues | 0 | [N] | +[N] |

---

### RECOMMENDATIONS

Prioritized list of fixes:

1. **[Priority 1]**: [Brief description] — Affects: [scope] — Known Issue: [ID]
2. **[Priority 2]**: [Brief description] — Affects: [scope] — Known Issue: [ID]
3. ...

---

### APPENDIX: Files Examined

#### Source XHTML Files (Publisher Original)
```
[List key source files examined]
```

#### Converted XML Files (Pipeline Output)
```
[List key converted files examined]
```

#### File Mapping
| Source XHTML | Converted XML | Content |
|-------------|---------------|---------|
| `Cover.xhtml` | `preface.{ISBN}.pr0001.xml` | Cover |
| `Frontmatter_001.xhtml` | `preface.{ISBN}.pr0002.xml` | Title page |
| `657658_1_En_1_Chapter.xhtml` | `sect1.{ISBN}.ch0001s*.xml` | Chapter 1 |
| ... | ... | ... |

#### Publisher Pattern Reference

For publisher-specific markup patterns, class names, and conversion rules, see `../_shared/publisher-patterns.md`.

---

## Classification Reference

### Severity Levels
| Level | Criteria |
|-------|----------|
| **HIGH** | Affects readability, navigation, or content accuracy across multiple chapters |
| **MEDIUM** | Visual/formatting issues that don't block comprehension but affect quality |
| **LOW** | Minor cosmetic issues, isolated occurrences |

### Source Attribution
| Source | Meaning |
|--------|---------|
| **Converter** | Issue introduced by the XML conversion pipeline (`epub_to_structured_v2.py`) |
| **Publisher** | Issue present in the publisher's source XHTML files (specify which publisher) |
| **Platform** | Issue in the R2 rendering engine (not the XML data) |

### Scope Categories
| Scope | Meaning |
|-------|---------|
| **Systemic** | Affects all or most chapters/sections (pattern-based bug) |
| **Isolated** | Affects specific instances only (data-specific issue) |

---

## Skills Chain

The QA report is the first step in a multi-skill remediation pipeline:

1. **QA Report** (`r2-book-qa`) — Identify and classify all issues in the converted book
2. **Diagnosis** (`r2-diagnosis`) — Deep-dive into specific issues to determine exact root causes and propose fixes
3. **XML Fix** (`r2-xml-fixer`) — Apply fixes directly to the converted DocBook XML output files
4. **Code Fix** (`r2-code-fixer`) — Apply fixes to the conversion pipeline source code (`epub_to_structured_v2.py`) to prevent issues in future conversions

The flow is: **QA Report** -> **Diagnosis** (`r2-diagnosis`) -> **XML Fix** (`r2-xml-fixer`) -> **Code Fix** (`r2-code-fixer`)
