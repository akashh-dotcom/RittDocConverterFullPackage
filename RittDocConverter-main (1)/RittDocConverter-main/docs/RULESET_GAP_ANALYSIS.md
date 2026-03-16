# Ruleset Gap Analysis: Documentation vs Implementation

> **STATUS: RESOLVED** (January 2026)
>
> All critical gaps identified in this analysis have been addressed:
> - Element codes updated (`f`→`fg`, `t`→`ta`, `b`→`bib`)
> - Missing element codes added (`vd`, `ad`, `qa`, `pr`)
> - Bibliography number stripping implemented
> - R2 ruleset updated as authoritative source
> - `id-naming-convention-rules.md` deprecated
>
> See commit history for implementation details.

---

## Executive Summary

This analysis compares the documented rulesets against the actual implementation code to identify:
- **CONFLICTS** - Rules that contradict between documentation or implementation
- **MISSING** - Features documented but not implemented
- **EXTRA** - Features implemented but not documented

---

## 1. CRITICAL CONFLICTS

### 1.1 Element Code Mismatch (BREAKS LINK RESOLUTION)

The most critical conflict exists between the XSLT link processor (`link.ritt.xsl`) and the Python ID generators.

| Element | link.ritt.xsl (XSLT) | id_generator.py | fix_duplicate_ids.py | id-naming-convention-rules.md |
|---------|----------------------|-----------------|---------------------|-------------------------------|
| **Figure** | `fg` | `f` | `f` | `f` |
| **Table** | `ta` | `t` | `t` | `t` |
| **Bibliography** | `bib` | `b` | `b` | `b` |
| Equation | `eq` | `eq` | `eq` | `eq` |
| Glossary | `gl` | - | `gl` | - |
| Video | `vd` | - | - | - |
| Audio | `ad` | - | - | - |
| Procedure | `pr` | - | - | - |
| Q&A | `qa` | - | - | - |

**Impact:** The XSLT processor (`link.ritt.xsl` lines 19-27) uses `substring-after(@linkend, 'fg')` to detect figures, but our implementation generates IDs with `f` code. This means:
- Links to `ch0001s0001f0001` will NOT be recognized as figure links
- The popupLink detection fails, potentially breaking popup/modal behavior
- URL generation may use incorrect file paths

**Source Files:**
- `pipeline_tester/xsl/link.ritt.xsl:19-27` - XSLT element detection
- `id_generator.py:32-56` - Python ELEMENT_CODES
- `fix_duplicate_ids.py:41-73` - Python ELEMENT_CODES

### 1.2 Section Number Padding Conflict

Two different padding schemes are documented:

| Source | Chapter Format | Section Format | Total Sect1 Length |
|--------|---------------|----------------|-------------------|
| **R2_LINKEND_AND_TOC_RULESET.md** | `ch{4-digit}` | `s{4-digit}` | 11 chars (ch0001s0001) |
| **id-naming-convention-rules.md** | `ch{4-digit}` | `s{6-digit}` | 13 chars (ch0011s000000) |
| **id_generator.py** | `ch{4-digit}` | `s{4-digit}` | 11 chars |

**Impact:** The `id-naming-convention-rules.md` uses 6-digit section numbers while the R2 ruleset and implementation use 4-digit. This documentation inconsistency could cause confusion.

**Note:** The implementation matches R2_LINKEND_AND_TOC_RULESET.md (4-digit), which is correct for `link.ritt.xsl` line 70 that extracts `substring(@linkend,1,11)`.

---

## 2. MISSING IMPLEMENTATIONS

### 2.1 Missing Element Codes in ID Generators

Elements defined in R2_LINKEND_AND_TOC_RULESET.md but missing from `id_generator.py`:

| Code | Element Type | Status | Impact |
|------|--------------|--------|--------|
| `vd` | Video | **NOT IMPLEMENTED** | Video elements won't get proper IDs |
| `ad` | Audio | **NOT IMPLEMENTED** | Audio elements won't get proper IDs |
| `qa` | Q&A | **NOT IMPLEMENTED** | Q&A sets won't get proper IDs |
| `pr` | Procedure | **NOT IMPLEMENTED** | Procedure elements won't get proper IDs |
| `fg` | Figure (2-char) | Uses `f` instead | Link resolution mismatch |
| `ta` | Table (2-char) | Uses `t` instead | Link resolution mismatch |
| `bib` | Bibliography (3-char) | Uses `b` instead | Link resolution mismatch |

### 2.2 Missing TOC Validation in cleanup_toc.py

Rules from R2_LINKEND_AND_TOC_RULESET.md not validated:

| Rule | Section | Status |
|------|---------|--------|
| `role="partintro"` attribute validation | 2.4 | **NOT IMPLEMENTED** |
| Index entry (`in` prefix) exclusion warning | 2.7 | **NOT IMPLEMENTED** |
| TOC entry linkend validation (targets exist) | 2.2 | **NOT IMPLEMENTED** |
| Proper nesting hierarchy validation | 2.8 | **NOT IMPLEMENTED** |

### 2.3 Missing ID Prefix Validation

Documented prefixes in R2_LINKEND_AND_TOC_RULESET.md not fully validated:

| Prefix | Content Type | Validation Status |
|--------|--------------|-------------------|
| `dd` | Dedication | Partial |
| `pr` | Preface | Partial |
| `pt` | Part | **NOT VALIDATED** |
| `sp` | Subpart/Part Intro | **NOT VALIDATED** |
| `gl` | Glossary | **NOT VALIDATED** |
| `bi` | Bibliography | **NOT VALIDATED** |
| `in` | Index | **NOT VALIDATED** |

**REMOVED (Not XSL-supported):**
- `ab` (About) - now uses `ch` prefix
- `co` (Colophon) - now uses `ch` prefix
- `ak` (Acknowledgments) - now uses `ch` prefix

### 2.4 Missing s0000 Suffix Enforcement

R2_LINKEND_AND_TOC_RULESET.md Rule 2 states:
> "s0000 Suffix is MANDATORY even for content without sect1 sections"

**Current Status:**
- The rule is documented but enforcement in `id_generator.py` is inconsistent
- No automated validation to catch IDs missing the `s0000` suffix
- Example: `pr0001fg0001` should be `pr0001s0000fg0001`

---

## 3. EXTRA/UNDOCUMENTED IMPLEMENTATIONS

### 3.1 Additional Element Codes in Implementation

Element codes in `id_generator.py` and `fix_duplicate_ids.py` NOT documented in R2_LINKEND_AND_TOC_RULESET.md:

| Code | Element | Source File | R2 Ruleset |
|------|---------|-------------|------------|
| `fn` | footnote | id_generator.py:46 | Not documented |
| `sc` | section | id_generator.py:47 | Not documented |
| `bq` | blockquote | id_generator.py:53 | Not documented |
| `vl` | variablelist | id_generator.py:54 | Not documented |
| `tm` | term | id_generator.py:55 | Not documented |
| `li` | listitem | id_generator.py:40 | Not documented |
| `ix` | indexterm | fix_duplicate_ids.py:72 | Not documented |
| `s1-s5` | sect1-sect5 | id_generator.py:48-52 | Not documented |

### 3.2 TOC Cleanup Features Not in R2 Ruleset

Features in `cleanup_toc.py` not documented in the ruleset:

| Feature | Description | Status |
|---------|-------------|--------|
| Number prefix cleanup (Rule 5) | Strips "1:" from titles | Extra feature |
| Generic title detection (Rule 6) | Flags placeholder titles | Extra feature |
| Spacing fix (Rule 7) | Adds space between digit and word | Extra feature |

These are useful features but should be documented in the ruleset.

---

## 4. DOCUMENTATION INCONSISTENCIES

### 4.1 Conflicting Element Code References

**R2_LINKEND_AND_TOC_RULESET.md Section 1.4:**
```
| Figure | `fg` | `{sect1_id}fg{4-digits}` | ch0001s0001fg0001 |
| Table | `ta` | `{sect1_id}ta{4-digits}` | ch0001s0001ta0001 |
| Bibliography | `bib` | `{sect1_id}bib{4-digits}` | ch0001s0001bib0001 |
```

Note: All element codes now use XSL-recognized prefixes (`fg`, `ta`, `bib`).

**id-naming-convention-rules.md:**
```
| Figure | `fg` | ch0011s000000fg0001 |
| Table | `ta` | ch0011s000000ta0001 |
| Bibliography/Reference | `bib` | ch0011s000000bib0001 |
```

### 4.2 Maximum ID Length

Both documents agree on 25 characters, but element code lengths differ:

| Code | Length | Examples |
|------|--------|----------|
| `fg`, `ta`, `eq`, `sb`, `ex` | 2 chars | Documented in R2 ruleset |
| `bib` | 3 chars | Only in link.ritt.xsl XSLT |
| `f`, `t`, `b`, `a`, `l`, `n`, `p` | 1 char | Documented in id-naming-convention-rules.md |

Using 1-char codes provides more headroom for sequence numbers.

---

## 5. IMPACT ASSESSMENT

### 5.1 Critical (Immediate Fix Required)

| Issue | Impact | Priority |
|-------|--------|----------|
| Figure/Table element code mismatch (`f`/`t` vs `fg`/`ta`) | Link resolution breaks | **P0** |
| Bibliography code mismatch (`b` vs `bib`) | Link resolution breaks | **P0** |

### 5.2 High (Should Fix Soon)

| Issue | Impact | Priority |
|-------|--------|----------|
| Missing video/audio element codes | Media links won't work | **P1** |
| Missing procedure/qa element codes | Special content links break | **P1** |
| s0000 suffix not enforced | Non-sectioned content links break | **P1** |

### 5.3 Medium (Should Document/Improve)

| Issue | Impact | Priority |
|-------|--------|----------|
| TOC validation incomplete | Invalid TOCs may pass | **P2** |
| Extra element codes undocumented | Confusion for maintainers | **P2** |
| Section padding inconsistency in docs | Confusion for developers | **P2** |

---

## 6. RECOMMENDATIONS

### 6.1 Immediate Actions

1. **Resolve Element Code Conflict** - Choose ONE standard:
   - **Option A:** Update `id_generator.py` and `fix_duplicate_ids.py` to use `fg`, `ta`, `bib` (match XSLT)
   - **Option B:** Update `link.ritt.xsl` to accept both `f`/`fg`, `t`/`ta`, `b`/`bib` (backwards compatible)

   **Recommendation:** Option A - align Python with existing XSLT since XSLT is likely deployed in production.

2. **Add Missing Element Codes:**
   ```python
   ELEMENT_CODES = {
       # ... existing codes ...
       'video': 'vd',
       'mediaobject': 'vd',  # for video
       'audio': 'ad',
       'qandaset': 'qa',
       'procedure': 'pr',
   }
   ```

3. **Enforce s0000 Suffix:**
   - Add validation in `id_generator.py` for non-sectioned prefixes (pr, dd, gl, bi, in, ab)
   - Add check in `fix_duplicate_ids.py` to flag IDs missing s0000

### 6.2 Documentation Updates

1. **Consolidate Element Codes:**
   - Create single authoritative element code table
   - Update `id-naming-convention-rules.md` to match R2 ruleset
   - Add changelog noting the standardization

2. **Fix Section Padding Documentation:**
   - `id-naming-convention-rules.md` should use 4-digit section numbers, not 6-digit
   - Examples should be: `ch0001s0001` not `ch0011s000000`

3. **Document Extra Features:**
   - Add cleanup_toc.py rules to R2_LINKEND_AND_TOC_RULESET.md
   - Document additional element codes (fn, sc, bq, vl, tm, li, ix)

### 6.3 Validation Improvements

1. **Add TOC Validation:**
   - Validate `role="partintro"` presence
   - Warn about index entries (won't render)
   - Validate linkend targets exist

2. **Add Comprehensive ID Validation:**
   - Check all prefixes (dd, pr, ab, pt, sp, co, ak, gl, bi, in)
   - Verify s0000 suffix for non-sectioned content
   - Validate 11-character base requirement

---

## 7. QUICK REFERENCE: CORRECT ELEMENT CODES

Based on `link.ritt.xsl` (the authoritative source for link resolution):

| Element | Correct Code | Current Implementation | Fix Required |
|---------|--------------|----------------------|--------------|
| Figure | `fg` | `f` | **YES** |
| Table | `ta` | `t` | **YES** |
| Equation | `eq` | `eq` | No |
| Glossary Entry | `gl` | `gl` | No |
| Bibliography | `bib` | `b` | **YES** |
| Q&A | `qa` | - | **ADD** |
| Procedure | `pr` | - | **ADD** |
| Video | `vd` | - | **ADD** |
| Audio | `ad` | - | **ADD** |

---

## 8. FILES REQUIRING UPDATES

### To Fix Element Codes:

1. `/home/user/RittDocConverter/id_generator.py` (lines 32-56)
2. `/home/user/RittDocConverter/fix_duplicate_ids.py` (lines 41-73)
3. `/home/user/RittDocConverter/comprehensive_dtd_fixer.py` (lines 286-320)
4. `/home/user/RittDocConverter/epub_to_structured_v2.py` (uses ELEMENT_CODES)

### To Fix Documentation:

1. `/home/user/RittDocConverter/id-naming-convention-rules.md` (section padding, element codes)
2. `/home/user/RittDocConverter/docs/R2_LINKEND_AND_TOC_RULESET.md` (add cleanup rules, extra codes)

### To Add Validation:

1. `/home/user/RittDocConverter/cleanup_toc.py` (add TOC validation)
2. Create new: `/home/user/RittDocConverter/validate_ids.py` (comprehensive ID validation)

---

*Document Version: 1.0*
*Analysis Date: January 2026*
*Based on: R2_LINKEND_AND_TOC_RULESET.md, id-naming-convention-rules.md, link.ritt.xsl, id_generator.py, fix_duplicate_ids.py, cleanup_toc.py*
