# R2 Library Linkend Resolution and TOC Nesting Rules

## Document Purpose

This document provides authoritative rules for ID generation and linkend resolution based on thorough analysis of the downstream XSL processors:
- `link.ritt.xsl` - Link/xref resolution
- `ritttoc.xsl` - TOC rendering
- `rittnav.xsl` - Navigation (prev/next)
- `toctransform.xsl` - TOC generation from book structure
- `RISChunker.xsl` - File chunking

These rules are essential for ensuring proper link resolution and navigation in the R2 Library platform.

---

# PART 1: LINKEND RESOLUTION RULES

## 1.1 How link.ritt.xsl Parses Linkends

The XSL processor parses linkends to extract **three components**:

```
linkend = "ch0001s0001fg0001"
           └─────┘└───┘└────┘
           chapter section element
           link    link   (popupLink)
```

### Component Extraction Logic

**1. popupLink** (Element identifier) - extracted by finding these codes in order:
| Code | Element Type | Example |
|------|--------------|---------|
| `fg` | Figure | `fg0001` |
| `eq` | Equation | `eq0001` |
| `ta` | Table | `ta0001` |
| `gl` | Glossary | `gl0001` |
| `bib` | Bibliography | `bib0001` |
| `qa` | Q&A | `qa0001` |
| `pr` | Procedure | `pr0001` |
| `vd` | Video | `vd0001` |
| `ad` | Audio | `ad0001` |

**2. sectionlink** - extracted when linkend contains both `ch` (or `ap`) AND `s`:
- Format: `s{section_number}`
- Example: From `ch0001s0001fg0001` → `s0001`

**3. chapterlink** - extracted as:
- If sectionlink exists: everything before sectionlink
- If popupLink exists (no section): everything before popupLink
- Otherwise: the entire linkend

---

## 1.2 URL Generation Rules (CRITICAL)

The XSL generates URLs using this priority logic:

### Case 1: Appendix/Preface/Part WITH Section
**Condition:** linkend contains `ap`, `pr`, or `pt` AND contains `s`
```
URL = {before_s}#goto={linkend}
Example: pr0001s0000fg0001 → pr0001#goto=pr0001s0000fg0001
```

### Case 2: Appendix WITHOUT Section
**Condition:** linkend contains `ap` only
```
URL = {first_chapterlen_chars}#goto={linkend}
```

### Case 3: Navigation Links (chapter/section level)
**Condition:** linkend length equals chapterlen OR chaptsectlen
```
URL = {linkend}  (no fragment, direct navigation)
Example: ch0001s0001 → ch0001s0001
```

### Case 4: DEFAULT - Element Links (MOST COMMON)
**Condition:** All other cases
```
URL = {first_11_chars}#goto={linkend}
Example: ch0001s0001fg0001 → ch0001s0001#goto=ch0001s0001fg0001
```

---

## 1.3 RECOGNIZED ELEMENT PREFIXES (CRITICAL)

**Only these element prefixes are recognized by `link.ritt.xsl` for URL resolution:**

| Prefix | Element Type | Recognized | Example |
|--------|--------------|------------|---------|
| `fg` | Figure | ✅ YES | `ch0001s0001fg0001` |
| `eq` | Equation | ✅ YES | `ch0001s0001eq0001` |
| `ta` | Table | ✅ YES | `ch0001s0001ta0001` |
| `gl` | Glossary entry | ✅ YES | `gl0001s0000gl0001` |
| `bib` | Bibliography entry | ✅ YES | `bi0001s0000bib0001` |
| `qa` | Q&A entry | ✅ YES | `ch0001s0001qa0001` |
| `pr` | Procedure | ✅ YES | `ch0001s0001pr0001` |
| `vd` | Video | ✅ YES | `ch0001s0001vd0001` |
| `ad` | Admonition/Audio | ✅ YES | `ch0001s0001ad0001` |

**NOT RECOGNIZED - Will fail link resolution:**

| Prefix | Element Type | Recognized | Why It Fails |
|--------|--------------|------------|--------------|
| `li` | List item | ❌ NO | Not in popupLink patterns |
| `tb` | Table (wrong) | ❌ NO | Must use `ta` not `tb` |
| `sb` | Sidebar | ❌ NO | Not in popupLink patterns |
| `ex` | Example | ❌ NO | Not in popupLink patterns |
| `note` | Note | ❌ NO | Not in popupLink patterns |
| `f` | Figure (wrong) | ❌ NO | Must use `fg` not `f` |

### IMPORTANT: Bibliography/References Must Use `bibliomixed`

**DO NOT use lists for bibliography/references.** List items (`li####`) cannot be cross-referenced because `li` is not a recognized prefix.

**Correct:** Use `<bibliomixed>` elements with `bib` prefix IDs:
```xml
<!-- Bibliography element uses sect1-style 11-character ID format (chnnnnsnnnn) -->
<bibliography id="ch0021s0014">
  <title>References</title>
  <bibliomixed id="ch0021s0014bib01">Author, A. (2023). Title...</bibliomixed>
  <bibliomixed id="ch0021s0014bib02">Author, B. (2024). Title...</bibliomixed>
</bibliography>
```

**WRONG:** Do NOT use ordered/itemized lists:
```xml
<!-- THIS WILL NOT WORK FOR CROSS-REFERENCES -->
<orderedlist>
  <listitem id="ch0021s0014li01">Author, A. (2023)...</listitem>  <!-- ❌ FAILS -->
</orderedlist>
```

---

## 1.5 CRITICAL ID FORMAT REQUIREMENTS

### Rule 1: All Element IDs MUST Have 11-Character Base
The default case takes **exactly 11 characters** as the file identifier. This means:

```
{prefix}{4-digits}s{4-digits} = 11 characters
ch0001s0000 = 11 characters
pr0001s0000 = 11 characters
ap0001s0000 = 11 characters
```

### Rule 2: s0000 Suffix is MANDATORY
Even for content without sect1 sections (preface, dedication, glossary, etc.), the `s0000` suffix is **required**:

| Content Type | Correct ID Format | File Resolution |
|--------------|-------------------|-----------------|
| Preface figure | `pr0001s0000fg0001` | pr0001 |
| Dedication anchor | `dd0001s0000a0001` | dd0001 |
| Glossary term | `gl0001s0000gl0001` | gl0001 |
| Index item | `in0001s0000ix0001` | in0001 |
| Bibliography entry | `bi0001s0000bib0001` | bi0001 |

### Rule 3: Element IDs in Chapters/Appendices with Sections
For content WITH sect1 sections, use the actual sect1 ID:

```xml
<sect1 id="ch0001s0001">
  <figure id="ch0001s0001fg0001">  <!-- Uses sect1 ID -->
  <table id="ch0001s0001ta0001">
</sect1>
```

### Rule 4: Sect2+ Elements Still Use Sect1 Prefix
Even for elements inside sect2, sect3, etc., use the **sect1 ID** (since files are chunked at sect1 level):

```xml
<sect1 id="ch0001s0001">
  <sect2 id="ch0001s0001s0001">
    <figure id="ch0001s0001fg0001">  <!-- Still uses sect1 prefix -->
    <sect3 id="ch0001s0001s0001s0001">
      <table id="ch0001s0001ta0001">  <!-- Still uses sect1 prefix -->
    </sect3>
  </sect2>
</sect1>
```

---

## 1.6 Complete ID Format Reference

### Section/Chapter IDs (Navigation Targets)

| Content Type | ID Format | Example | Length |
|--------------|-----------|---------|--------|
| Chapter | `ch{4-digits}` | `ch0001` | 6 |
| Chapter Section | `ch{4-digits}s{4-digits}` | `ch0001s0001` | 11 |
| Appendix | `ap{4-digits}` | `ap0001` | 6 |
| Appendix Section | `ap{4-digits}s{4-digits}` | `ap0001s0001` | 11 |
| Preface | `pr{4-digits}` | `pr0001` | 6 |
| Preface (with s0000) | `pr{4-digits}s0000` | `pr0001s0000` | 11 |
| Part | `pt{4-digits}` | `pt0001` | 6 |
| Part Intro | `pt{4-digits}sp{4-digits}` | `pt0001sp0001` | 12 |
| Dedication | `dd{4-digits}` | `dd0001` | 6 |
| Glossary | `gl{4-digits}` | `gl0001` | 6 |
| Bibliography | `bi{4-digits}` | `bi0001` | 6 |
| Index | `in{4-digits}` | `in0001` | 6 |

### Element IDs (Cross-Reference Targets)

#### Popup-Linked Elements (detected by link.ritt.xsl for popup display)

| Element | Code | Full ID Format | Example | Max Length |
|---------|------|----------------|---------|------------|
| Figure | `fg` | `{sect1_id}fg{4-digits}` | `ch0001s0001fg0001` | 19 |
| Table | `ta` | `{sect1_id}ta{4-digits}` | `ch0001s0001ta0001` | 19 |
| Equation | `eq` | `{sect1_id}eq{4-digits}` | `ch0001s0001eq0001` | 19 |
| Bibliography (wrapper) | - | `{sect1_id}` (11 chars) | `ch0021s0014` | 11 |
| Bibliography Entry | `bib` | `{sect1_id}bib{4-digits}` | `ch0021s0014bib0001` | 22 |
| Glossary Entry | `gl` | `{sect1_id}gl{4-digits}` | `gl0001s0000gl0001` | 22 |
| Q&A | `qa` | `{sect1_id}qa{4-digits}` | `ch0001s0001qa0001` | 19 |
| Procedure | `pr` | `{sect1_id}pr{4-digits}` | `ch0001s0001pr0001` | 19 |
| Video | `vd` | `{sect1_id}vd{4-digits}` | `ch0001s0001vd0001` | 19 |
| Admonition/Sidebar | `ad` | `{sect1_id}ad{4-digits}` | `ch0001s0001ad0001` | 19 |

**Note:** The `<bibliography>` wrapper element uses the 11-character sect1-style ID format (`chnnnnsnnnn`) because it's treated as a section of the chapter. Individual `<bibliomixed>` entries inside use the `bib` prefix.

#### Non-Popup Elements

| Element | Code | Full ID Format | Example | Max Length |
|---------|------|----------------|---------|------------|
| Anchor | `a` | `{sect1_id}a{4-digits}` | `ch0001s0001a0001` | 17 |
| Sidebar | `sb` | `{sect1_id}sb{4-digits}` | `ch0001s0001sb0001` | 19 |
| List | `l` | `{sect1_id}l{4-digits}` | `ch0001s0001l0001` | 17 |
| Note/Tip/Warning | `n` | `{sect1_id}n{4-digits}` | `ch0001s0001n0001` | 17 |
| Example | `ex` | `{sect1_id}ex{4-digits}` | `ch0001s0001ex0001` | 19 |
| Footnote | `fn` | `{sect1_id}fn{4-digits}` | `ch0001s0001fn0001` | 19 |
| Paragraph | `p` | `{sect1_id}p{4-digits}` | `ch0001s0001p0001` | 17 |
| Index Term | `ix` | `{sect1_id}ix{4-digits}` | `ch0001s0001ix0001` | 19 |
| Blockquote | `bq` | `{sect1_id}bq{4-digits}` | `ch0001s0001bq0001` | 19 |
| Term | `tm` | `{sect1_id}tm{4-digits}` | `ch0001s0001tm0001` | 19 |

#### Section Elements

Section IDs use a consistent `sXXXX` pattern at every level. This allows determining section depth by counting `s` segments:

| Element | Pattern | Example | Length |
|---------|---------|---------|--------|
| Sect1 | `{chapter_id}s{4-digits}` | `ch0001s0001` | 11 |
| Sect2 | `{sect1_id}s{4-digits}` | `ch0001s0001s0001` | 16 |
| Sect3 | `{sect2_id}s{2-digits}` | `ch0001s0001s0001s01` | 19 |
| Sect4 | `{sect3_id}s{2-digits}` | `ch0001s0001s0001s01s01` | 22 |
| Sect5 | `{sect4_id}s{2-digits}` | `ch0001s0001s0001s01s01s01` | 25 |

**Key insight**: Count `s` segments to determine section depth:
- 1 segment (`ch0001s0001`) = sect1
- 2 segments (`ch0001s0001s0001`) = sect2
- 3 segments (`ch0001s0001s0001s01`) = sect3 (uses 2-digit to stay within 25-char limit)
- etc.

---

## 1.5 Code Collision Cases (IMPORTANT)

Some codes are used for **both** chapter prefixes AND element codes. The XSLT uses `substring-after()`
which matches ANY occurrence of the code in the linkend. Understanding these collisions is critical.

### 1.5.1 The `pr` Code Collision

| Usage | Meaning | Example |
|-------|---------|---------|
| Chapter prefix | Preface section | `pr0001`, `pr0001s0000` |
| Element code | Procedure element | `ch0001s0001pr0001` |

**Valid ID Examples:**
```
pr0001s0000fg0001    → Preface + Figure (no collision)
ch0001s0001pr0001    → Chapter + Procedure (no collision)
pr0001s0000pr0001    → Preface + Procedure (COLLISION CASE)
```

**How XSLT Handles It (link.ritt.xsl):**
The XSLT uses `substring-after(@linkend, 'pr')` which returns everything after the FIRST `pr`.
- For `pr0001s0000pr0001`: returns `0001s0000pr0001` (matches preface prefix first)
- This means procedure elements in preface sections may not trigger popup behavior correctly

**Recommendation:** Avoid procedure elements (`<procedure>`) inside preface sections when possible.
If unavoidable, the link will still resolve to the correct file, but popup behavior may not work.

### 1.5.2 The `gl` Code Collision

| Usage | Meaning | Example |
|-------|---------|---------|
| Chapter prefix | Glossary section | `gl0001`, `gl0001s0000` |
| Element code | Glossary entry | `gl0001s0000gl0001` |

**Valid ID Examples:**
```
gl0001s0000gl0001    → Glossary + Glossary Entry (EXPECTED COLLISION)
ch0001s0001gl0001    → Chapter + Glossary Entry (no collision)
```

**How XSLT Handles It:**
For `gl0001s0000gl0001`: `substring-after(@linkend, 'gl')` returns `0001s0000gl0001`
- The XSLT extracts the first `gl` as the chapter prefix
- This is the EXPECTED behavior for glossary entries in glossary sections

**This collision is intentional and handled correctly.**

### 1.5.3 The `bi`/`bib` Near-Collision

| Usage | Meaning | Example |
|-------|---------|---------|
| Chapter prefix | Bibliography section | `bi0001`, `bi0001s0000` |
| Element code | Bibliography entry | `bi0001s0000bib0001` |

**Valid ID Examples:**
```
bi0001s0000bib0001   → Bibliography + Bibliography Entry
```

**How XSLT Handles It:**
For `bi0001s0000bib0001`: `substring-after(@linkend, 'bib')` returns `0001`
- The XSLT correctly finds `bib` (not `bi`) for bibliography entries
- The 3-character `bib` code distinguishes from the 2-character `bi` prefix

**No collision issue - the 3-char element code prevents ambiguity.**

### 1.5.4 Summary of Collision Behavior

| Collision Case | XSLT Behavior | Link Resolution | Popup Works? |
|----------------|---------------|-----------------|--------------|
| `pr` prefix + `pr` element | Matches prefix first | ✅ Correct file | ⚠️ May fail |
| `gl` prefix + `gl` element | Matches prefix first | ✅ Correct file | ✅ Works |
| `bi` prefix + `bib` element | Matches element code | ✅ Correct file | ✅ Works |

---

## 1.7 Link Resolution Examples

### Example 1: Figure in Chapter Section
```xml
<figure id="ch0001s0001fg0001">...</figure>
<link linkend="ch0001s0001fg0001">Figure 1.1</link>
```
**Resolution:**
- popupLink = `fg0001`
- sectionlink = `s0001`
- chapterlink = `ch0001`
- URL = `ch0001s0001#goto=ch0001s0001fg0001`

### Example 2: Table in Preface (No Real Sections)
```xml
<table id="pr0001s0000ta0001">...</table>
<link linkend="pr0001s0000ta0001">Table P.1</link>
```
**Resolution:**
- popupLink = `ta0001`
- Contains `pr` AND `s` → Case 1
- URL = `pr0001#goto=pr0001s0000ta0001`

### Example 3: Appendix Section Navigation
```xml
<tocentry linkend="ap0001s0001">Appendix A.1</tocentry>
```
**Resolution:**
- Length = 11 (matches chaptsectlen)
- URL = `ap0001s0001` (direct navigation)

### Example 4: Glossary Term
```xml
<glossentry id="gl0001s0000gl0001">...</glossentry>
<link linkend="gl0001s0000gl0001">Term</link>
```
**Resolution:**
- popupLink = `gl0001` (from `gl` code)
- Contains `s` → extracts section
- URL = `gl0001#goto=gl0001s0000gl0001`

---

## 1.8 Validation Rules for IDs

### Rule V1: Maximum Length
```
All IDs ≤ 25 characters
```

### Rule V2: Character Set
```
Only lowercase letters (a-z) and numbers (0-9)
No hyphens, underscores, spaces, or special characters
```

### Rule V3: Prefix Match
```
Element ID must start with the sect1 ID where it lives
ch0001s0001fg0001 ✓ (in file ch0001s0001.xml)
ch0001s0002fg0001 ✗ (wrong if in ch0001s0001.xml)
```

### Rule V4: s0000 Required for Non-Sectioned Content
```
pr0001s0000fg0001 ✓
pr0001fg0001 ✗ (BREAKS LINK RESOLUTION)
```

---

# PART 2: TOC NESTING RULES

## 2.1 TOC Element Hierarchy

Based on `toctransform.xsl` and `ritttoc.xsl`:

```
toc
├── tocinfo
│   └── risinfo
│       └── isbn
├── title
├── tocfront (Front Matter - multiple allowed)
│   └── @linkend
├── tocpart (Part container)
│   ├── @role="partintro" (optional, indicates part has intro)
│   ├── tocentry
│   │   └── @linkend
│   ├── tocsubpart (Subpart container)
│   │   ├── @role="partintro" (optional)
│   │   ├── tocentry
│   │   │   └── @linkend
│   │   └── tocchap...
│   ├── tocchap (Chapter container)
│   │   ├── tocentry
│   │   │   └── @linkend
│   │   └── toclevel1 (Sect1 container)
│   │       ├── tocentry
│   │       │   └── @linkend
│   │       └── toclevel2 (Sect2 container)
│   │           ├── tocentry
│   │           │   └── @linkend
│   │           └── toclevel3...4...5
│   └── tocback (Back matter inside part)
│       └── @linkend OR tocentry/@linkend
└── tocback (Back Matter - multiple allowed)
    └── @linkend OR tocentry/@linkend
```

---

## 2.2 TOC Element Reference

| Element | Purpose | Attributes | Children |
|---------|---------|------------|----------|
| `toc` | Root container | — | tocinfo, title, tocfront*, tocpart*, tocchap*, tocback* |
| `tocinfo` | Metadata | — | risinfo |
| `title` | Book title | — | text |
| `tocfront` | Front matter entry | `@linkend` (required) | text content |
| `tocpart` | Part container | `@role="partintro"` (optional) | tocentry, tocsubpart*, tocchap*, tocback* |
| `tocsubpart` | Subpart container | `@role="partintro"` (optional) | tocentry, tocchap*, tocback* |
| `tocchap` | Chapter container | — | tocentry, toclevel1* |
| `tocentry` | Entry with link | `@linkend` (required) | text content (with sub, sup allowed) |
| `toclevel1` | Sect1 container | — | tocentry, toclevel2* |
| `toclevel2` | Sect2 container | — | tocentry, toclevel3* |
| `toclevel3` | Sect3 container | — | tocentry, toclevel4* |
| `toclevel4` | Sect4 container | — | tocentry, toclevel5* |
| `toclevel5` | Sect5 container | — | tocentry |
| `tocback` | Back matter entry | `@linkend` (optional if tocentry has it) | text OR tocentry, toclevel1* |

---

## 2.3 Front Matter Rules (`tocfront`)

### Linkend Format
```xml
<tocfront linkend="dd0001">DEDICATION</tocfront>
<tocfront linkend="pr0001">Preface</tocfront>
```

**Note:** Other front matter types (About, Acknowledgments, Colophon) use `ch` prefix as they are not explicitly supported by the XSL. They are treated as regular chapters.

### Auto-Capitalization
These titles are automatically uppercased by the XSL:
- "Dedication" → "DEDICATION"

### Front Matter Type Detection (from linkend prefix)
| Prefix | Type |
|--------|------|
| `dd` | Dedication |
| `pr` | Preface |
| `ch` | Other front matter (About, Acknowledgments, etc.) |

---

## 2.4 Part Rules (`tocpart`)

### Basic Part
```xml
<tocpart>
  <tocentry linkend="pt0001">Part I: Foundations</tocentry>
  <tocchap>...</tocchap>
</tocpart>
```

### Part with Part Intro
When a part has introductory content (`partintro` element in source), add `role="partintro"`:
```xml
<tocpart role="partintro">
  <tocentry linkend="pt0001">Part I: Foundations</tocentry>
  <!-- The part intro navigates to pt0001sp0001 or similar -->
  <tocchap>...</tocchap>
</tocpart>
```

### Direct Part Linking (No `tocchap`)
When a part links directly to content without containing any chapters, omit `<tocchap>` entirely.
The `<tocentry>` `linkend` is used directly as the navigation target:
```xml
<tocpart>
  <tocentry linkend="pt0001">Part I: Standalone Content</tocentry>
  <!-- No tocchap children — linkend on tocentry is used directly -->
</tocpart>
```
This is valid per the DTD (`tocchap*` means zero or more) and the XSLT `href.part` template
will resolve the link from `tocentry/@linkend` when no `tocchap` children are present.

### Subpart Structure
```xml
<tocpart role="partintro">
  <tocentry linkend="pt0001">Part I</tocentry>
  <tocsubpart role="partintro">
    <tocentry linkend="pt0001sp0001">Section A</tocentry>
    <tocchap>...</tocchap>
  </tocsubpart>
</tocpart>
```

---

## 2.5 Chapter Rules (`tocchap`)

### Basic Chapter
```xml
<tocchap>
  <tocentry linkend="ch0001">Chapter 1: Introduction</tocentry>
  <toclevel1>
    <tocentry linkend="ch0001s0001">Overview</tocentry>
  </toclevel1>
</tocchap>
```

### Chapter Without Sections
If chapter has no sect1 children:
```xml
<tocchap>
  <tocentry linkend="ch0001">Chapter 1: Introduction</tocentry>
  <!-- No toclevel1 children -->
</tocchap>
```

---

## 2.6 Section Levels (`toclevel1-5`)

### Nesting Rules
- `toclevel1` can contain `toclevel2`
- `toclevel2` can contain `toclevel3`
- `toclevel3` can contain `toclevel4`
- `toclevel4` can contain `toclevel5`
- `toclevel5` is terminal (no children)

### Linkend Format for Sections
```xml
<toclevel1>
  <tocentry linkend="ch0001s0001">Section 1.1</tocentry>
  <toclevel2>
    <tocentry linkend="ch0001s0001s0001">Section 1.1.1</tocentry>
  </toclevel2>
</toclevel1>
```

---

## 2.7 Back Matter Rules (`tocback`)

### Detection by Linkend Prefix
| Prefix | Label | Rendered |
|--------|-------|----------|
| `ap` | appendix | Yes |
| `gl` | glossary | Yes |
| `bi` | bibliography | Yes |
| `in` | index | **NO** (excluded) |

### Simple Back Matter (No Sections)
```xml
<tocback linkend="gl0001">Glossary</tocback>
<tocback linkend="bi0001">Bibliography</tocback>
```

### Back Matter with Sections (e.g., Appendix)
```xml
<tocback>
  <tocentry linkend="ap0001">Appendix A: Reference Data</tocentry>
  <toclevel1>
    <tocentry linkend="ap0001s0001">A.1 Tables</tocentry>
  </toclevel1>
</tocback>
```

### Index Exclusion
Index entries (`in` prefix) are **excluded from navigation rendering**:
```xml
<!-- This will NOT be rendered in the TOC -->
<tocback linkend="in0001">Index</tocback>
```

---

## 2.8 Navigation Rules (from `rittnav.xsl`)

### Prev/Next Navigation Sources

| Current Position | Previous | Next |
|------------------|----------|------|
| First tocfront | About page | Next tocfront or first content |
| tocfront | Previous tocfront | Next tocfront, part intro, or chapter |
| tocpart (with partintro) | Previous content | partintro entry |
| tocsubpart | Previous section | Next section or part content |
| toclevel1 | Previous toclevel1 or chapter | Next toclevel1 or next chapter |
| tocback | Previous content or back matter | Next back matter or end |

### Part Intro Navigation
When `role="partintro"` is set:
1. Clicking part title navigates to the partintro content
2. The next button from front matter goes to partintro
3. Partintro's next goes to first chapter section

---

## 2.9 Complete TOC Example

```xml
<?xml version="1.0" encoding="UTF-8"?>
<toc>
  <tocinfo>
    <risinfo>
      <isbn>9781234567890</isbn>
    </risinfo>
  </tocinfo>
  <title>Medical Reference, 5th Edition</title>

  <!-- Front Matter -->
  <tocfront linkend="dd0001">DEDICATION</tocfront>
  <tocfront linkend="pr0001">Preface</tocfront>
  <tocfront linkend="ab0001">ABOUT</tocfront>

  <!-- Part with Subparts -->
  <tocpart role="partintro">
    <tocentry linkend="pt0001">Part I: Foundations</tocentry>
    <tocsubpart role="partintro">
      <tocentry linkend="pt0001sp0001">Section A: Basics</tocentry>
      <tocchap>
        <tocentry linkend="ch0001">Chapter 1: Introduction</tocentry>
        <toclevel1>
          <tocentry linkend="ch0001s0001">1.1 Overview</tocentry>
          <toclevel2>
            <tocentry linkend="ch0001s0001s0001">1.1.1 History</tocentry>
          </toclevel2>
        </toclevel1>
        <toclevel1>
          <tocentry linkend="ch0001s0002">1.2 Concepts</tocentry>
        </toclevel1>
      </tocchap>
    </tocsubpart>
  </tocpart>

  <!-- Simple Part (no partintro) -->
  <tocpart>
    <tocentry linkend="pt0002">Part II: Applications</tocentry>
    <tocchap>
      <tocentry linkend="ch0002">Chapter 2: Methods</tocentry>
      <toclevel1>
        <tocentry linkend="ch0002s0001">2.1 Techniques</tocentry>
      </toclevel1>
    </tocchap>
  </tocpart>

  <!-- Back Matter -->
  <tocback>
    <tocentry linkend="ap0001">Appendix A: Data Tables</tocentry>
    <toclevel1>
      <tocentry linkend="ap0001s0001">A.1 Reference Values</tocentry>
    </toclevel1>
  </tocback>
  <tocback linkend="gl0001">Glossary</tocback>
  <tocback linkend="bi0001">Bibliography</tocback>
  <!-- Index excluded from rendering -->
  <tocback linkend="in0001">Index</tocback>
</toc>
```

---

# PART 3: VALIDATION CHECKLIST

## 3.1 ID Validation

- [ ] All IDs ≤ 25 characters
- [ ] All IDs contain only lowercase letters and numbers
- [ ] All element IDs have 11-character base (includes s0000 for non-sectioned content)
- [ ] All element IDs start with the correct sect1 ID prefix
- [ ] No duplicate IDs in the document
- [ ] All `linkend` targets exist as `id` attributes

## 3.2 TOC Validation

- [ ] Root element is `toc`
- [ ] `tocinfo/risinfo/isbn` is present and valid
- [ ] `title` element is present
- [ ] All `tocfront` have `@linkend`
- [ ] All `tocentry` have `@linkend`
- [ ] `tocpart` with partintro has `role="partintro"`
- [ ] `tocsubpart` with partintro has `role="partintro"`
- [ ] `tocback` has either `@linkend` or `tocentry/@linkend`
- [ ] Nesting follows hierarchy: tocpart > tocsubpart > tocchap > toclevel1-5
- [ ] Index entries (`in` prefix) are present but will not render

## 3.3 Link Resolution Test

For each `linkend`, verify:
1. First 11 characters identify the correct file
2. Element exists with matching `id` in that file
3. URL will resolve to `{file}#goto={linkend}`

---

# PART 4: BIBLIOGRAPHY STRUCTURE RULES

## 4.1 Required Bibliography Structure

Bibliography content MUST use `<bibliomixed>` elements, NOT lists.

### Correct Structure

```xml
<!-- Bibliography element uses sect1-style 11-character ID format -->
<bibliography id="ch0021s0014">
  <title>References</title>
  <bibliodiv>
    <title>Chapter 1 References</title>  <!-- Optional grouping title -->
    <bibliomixed id="ch0021s0014bib01">
      Smith, J. (2020). Article Title. Journal Name, 15, 123-145.
    </bibliomixed>
    <bibliomixed id="ch0021s0014bib02">
      Jones, M. (2019). Book Title. Publisher.
    </bibliomixed>
  </bibliodiv>
</bibliography>
```

### Incorrect Structure (DO NOT USE)

```xml
<!-- WRONG: Lists inside bibliography -->
<bibliography>
  <orderedlist>
    <listitem><para>1. Smith, J. (2020)...</para></listitem>
  </orderedlist>
</bibliography>
```

## 4.2 Number Stripping Rule

**CRITICAL:** When converting lists to `<bibliomixed>`, strip leading numbers from entries.

`<bibliomixed>` elements are auto-numbered by the rendering system. If source content has
embedded numbers (e.g., "1. Smith, J..."), these MUST be stripped to prevent double-numbering.

### Patterns to Strip

| Pattern | Example | Result |
|---------|---------|--------|
| `\d+[\.\)\:\s]` | "1. Smith" | "Smith" |
| `[\d+]` | "[1] Smith" | "Smith" |
| `(\d+)` | "(1) Smith" | "Smith" |
| `[a-zA-Z][\.\)]` | "a. Smith" | "Smith" |
| Roman numerals | "i. Smith", "IV. Smith" | "Smith" |

### Conversion Logic

```
Input:  <listitem><para>1. Smith, J. (2020). Article Title...</para></listitem>
Output: <bibliomixed id="ch0021s0014bib01">Smith, J. (2020). Article Title...</bibliomixed>
```

## 4.3 Bibliography ID Format

| Element | ID Format | Example |
|---------|-----------|---------|
| Bibliography container | `{chapter_id}s{4-digits}` (11 chars, sect1-style) | `ch0021s0014` |
| Bibliography entry | `{sect1_id}bib{2-digits}` | `ch0021s0014bib01` |

**Note:** The `<bibliography>` element uses the same 11-character ID format as `<sect1>` elements because it's treated as a section of the chapter. When a bibliography section is detected and converted, the bibliography takes over the section's ID.

---

# PART 5: FIXING PREVIOUSLY PROCESSED FILES

## 5.1 Common Issues to Fix

### Issue 1: Missing s0000 Suffix
**Broken:** `pr0001fg0001` (10 chars)
**Fixed:** `pr0001s0000fg0001` (17 chars)

**Fix Script Logic:**
```
For each ID without 's' after the 6-character prefix:
  If prefix in (pr, dd, gl, bi, in, ab, co, ak):
    Insert 's0000' after position 6
    Update all linkend references
```

### Issue 2: Wrong Element Code
**Broken:** `ch0001s0001f0001` (using `f` instead of `fg`)
**Fixed:** `ch0001s0001fg0001`

**Common element code migrations:**
| Old Code | New Code | Element |
|----------|----------|---------|
| `f` | `fg` | Figure |
| `t` | `ta` | Table |
| `b` | `bib` | Bibliography entry |

### Issue 3: Inconsistent Section Numbering
**Broken:** `ch0001s1fg0001` (missing padding)
**Fixed:** `ch0001s0001fg0001`

## 5.2 ID Migration Mapping

Create a mapping file:
```json
{
  "pr0001fg0001": "pr0001s0000fg0001",
  "dd0001a0001": "dd0001s0000a0001",
  "gl0001gl0001": "gl0001s0000gl0001"
}
```

Apply to all files:
1. Update `id` attributes
2. Update `linkend` attributes
3. Validate all links resolve

---

*Document Version: 1.0*
*Based on XSL analysis: link.ritt.xsl, ritttoc.xsl, rittnav.xsl, toctransform.xsl, RISChunker.xsl*
