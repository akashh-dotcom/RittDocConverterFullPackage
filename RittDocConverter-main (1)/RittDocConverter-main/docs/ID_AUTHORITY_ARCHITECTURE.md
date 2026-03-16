# ID Authority Architecture

> **Version**: 1.0
> **Last Updated**: February 2026
> **Module**: `id_authority.py`

This document describes the Centralized ID Authority system for the EPUB to DocBook XML conversion pipeline. The ID Authority provides a **Single Source of Truth** for all ID management, resolving the issues with fragmented ID tracking, stale mappings, and inconsistent validation that existed in the legacy system.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)
3. [Architecture](#3-architecture)
4. [Core Components](#4-core-components)
5. [ID Definitions (Single Source of Truth)](#5-id-definitions-single-source-of-truth)
6. [Usage Guide](#6-usage-guide)
7. [Integration Points](#7-integration-points)
8. [Migration from Legacy System](#8-migration-from-legacy-system)
9. [API Reference](#9-api-reference)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. Overview

The ID Authority is a centralized system that manages all aspects of ID creation, validation, mapping, and resolution throughout the conversion pipeline. It replaces the fragmented approach where ID logic was scattered across `id_mapper.py`, `id_tracker.py`, `id_generator.py`, and various places in the main conversion scripts.

### Key Benefits

- **Single Source of Truth**: All ID definitions (prefixes, codes, formats) in one place
- **Atomic Updates**: Chapter ID changes cascade to all dependent mappings
- **Strict Validation**: Clear error messages when IDs don't conform
- **Audit Trail**: Full history of ID resolutions for debugging
- **Thread-Safe**: Safe for concurrent access in parallel processing

---

## 2. Problem Statement

### 2.1 Issues with the Legacy System

The original ID management system had several critical problems:

#### Phase Registration Race Condition
```
Phase 1: ALL files registered as ch0001, ch0002, ch0003...
         (including preface, appendix, index files)

Phase 2: TOC extracted using wrong chapter IDs
         Links stored with ch#### format

Phase 2.5: Correct prefixes assigned:
           ch0001 → pr0001 (preface)
           ch0003 → in0001 (index)

Result: Stale mappings remain in id_mapper with old ch#### IDs
        Links point to non-existent ch#### chapters
        DTD fixer creates phantom anchors trying to resolve
```

#### Fragmented ID Logic
- `id_generator.py`: Element code definitions
- `id_mapper.py`: Chapter and element ID mappings
- `id_tracker.py`: Phase-based ID tracking
- `epub_to_structured_v2.py`: Inline validation logic
- `comprehensive_dtd_fixer.py`: More inline validation

#### Inconsistent Regex Patterns
Different files used different patterns for the same validation:
```python
# Pattern 1 (allows any 2-letter prefix)
r'^[a-z]{2}\d{4}s\d{4}'

# Pattern 2 (only allows ch/ap/pr)
r'^(ch|ap|pr)\d{4}'

# Pattern 3 (allows mixed 2/4 digit sections)
r'^[a-z]{2}\d{4}(s\d{2,4})+'
```

### 2.2 Symptoms

- "Anchor Creation for Missing ID" warnings in validation reports
- Anchors created and then removed as orphans
- Links pointing to non-existent sections (e.g., `ch0007s0010` when ch0007 has only 7 sections)
- XSL link resolution failures for non-recognized element codes

---

## 3. Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                          ID AUTHORITY                                │
│                      (Singleton Facade)                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────┐  │
│  │  ID Definitions │  │    ID Parser    │  │  Chapter Registry   │  │
│  │     (Enums)     │  │  (Validation)   │  │  (Atomic Updates)   │  │
│  ├─────────────────┤  ├─────────────────┤  ├─────────────────────┤  │
│  │ ChapterPrefix   │  │ parse_id()      │  │ register_chapter()  │  │
│  │ ElementCode     │  │ validate_id()   │  │ update_chapter_id() │  │
│  │ Constants       │  │ extract_parts() │  │ get_chapter_id()    │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────────┘  │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │                      Linkend Resolver                           │ │
│  │                   (Resolution + Audit Trail)                    │ │
│  ├─────────────────────────────────────────────────────────────────┤ │
│  │ resolve_linkend(source_id, context) → generated_id              │ │
│  │ get_resolution_history() → audit trail                          │ │
│  │ find_unresolved_links() → diagnostic report                     │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 3.1 Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| **IDAuthority** | Singleton facade providing unified API |
| **ChapterPrefix** | Enum of valid 2-letter chapter prefixes |
| **ElementCode** | Enum of valid element codes with XSL recognition flags |
| **IDParser** | Parse and validate IDs, extract components |
| **ChapterRegistry** | Atomic chapter registration and updates |
| **LinkendResolver** | Resolve source IDs to generated IDs with audit trail |

---

## 4. Core Components

### 4.1 IDAuthority (Facade)

The main entry point. Use `get_authority()` to get the singleton instance.

```python
from id_authority import get_authority

authority = get_authority()

# Register a chapter
authority.register_chapter("preface.xhtml", "pr0001")

# Map an element ID
authority.map_id("pr0001", "fig-intro", "pr0001s0001fg0001")

# Resolve a linkend
result = authority.resolve_linkend("fig-intro", "pr0001")
```

### 4.2 ChapterRegistry

Manages chapter registrations with atomic updates. When a chapter ID changes, all dependent mappings are automatically updated.

```python
# Initial registration (Phase 1)
authority.register_chapter("chapter1.xhtml", "ch0001")

# Later update (Phase 2.5) - cascades to all mappings
authority.update_chapter_id("chapter1.xhtml", "ch0001", "pr0001")
```

### 4.3 LinkendResolver

Resolves source IDs to generated IDs with full audit trail.

```python
# Get resolution history for debugging
history = authority.get_resolution_history("fig-intro")
# Returns: [ResolutionRecord(source_id="fig-intro", generated_id="pr0001s0001fg0001", ...)]

# Find all unresolved links
unresolved = authority.find_unresolved_links()
```

### 4.4 IDParser

Validates and parses IDs according to the authoritative definitions.

```python
from id_authority import IDParser

parser = IDParser()

# Parse a complete ID
result = parser.parse("ch0001s0001fg0001")
# Returns: ParsedID(chapter_prefix="ch", chapter_num=1, sections=[(1, 4)],
#                   element_code="fg", element_num=1)

# Validate an ID
is_valid, error = parser.validate("invalid-id")
# Returns: (False, "ID contains invalid characters: '-'")
```

---

## 5. ID Definitions (Single Source of Truth)

All ID-related constants are defined in `id_authority.py` and should be imported from there.

### 5.1 Chapter Prefixes

```python
from id_authority import ChapterPrefix

class ChapterPrefix(Enum):
    """Valid 2-letter chapter type prefixes."""
    CHAPTER = "ch"      # Standard chapters
    APPENDIX = "ap"     # Appendix, epilogue, afterword
    PREFACE = "pr"      # Preface, foreword, introduction, copyright, cover
    INDEX = "in"        # Index
    GLOSSARY = "gl"     # Glossary chapters
    BIBLIOGRAPHY = "bi" # Bibliography chapters
    DEDICATION = "dd"   # Dedication
    PART = "pt"         # Part containers
    SUBPART = "sp"      # Subpart/part intro
    TOC_CHAPTER = "tc"  # TOC chapter files
    COPYRIGHT = "cp"    # Copyright page
    FRONTMATTER = "fm"  # Generic front matter
    BACKMATTER = "bm"   # Generic back matter
```

### 5.2 Element Codes

```python
from id_authority import ElementCode

class ElementCode(Enum):
    """
    Valid element type codes.
    Format: (code, xsl_recognized, description)
    """
    # XSL-Recognized (work for cross-reference popup links)
    FIGURE = ("fg", True, "Figures and images")
    TABLE = ("ta", True, "Tables")
    EQUATION = ("eq", True, "Equations")
    GLOSSENTRY = ("gl", True, "Glossary entries")
    BIBLIOGRAPHY = ("bib", True, "Bibliography entries")
    QANDA = ("qa", True, "Q&A entries")
    PROCEDURE = ("pr", True, "Procedures")
    VIDEO = ("vd", True, "Videos")
    ADMONITION = ("ad", True, "Admonitions and sidebars")

    # Non-XSL (valid anchors but no popup links)
    ANCHOR = ("a", False, "Generic anchors")
    PARAGRAPH = ("p", False, "Paragraphs")
    LIST = ("l", False, "Lists")
    EXAMPLE = ("ex", False, "Examples")
    FOOTNOTE = ("fn", False, "Footnotes")
    MEDIAOBJECT = ("mo", False, "Media objects")
    INDEXTERM = ("ix", False, "Index terms")
    STEP = ("st", False, "Procedure steps")
    SUBSTEP = ("ss", False, "Substeps")
    BLOCKQUOTE = ("bq", False, "Blockquotes")
    TERM = ("tm", False, "Definition terms")
    FALLBACK = ("x", False, "Unknown element fallback")
```

### 5.3 Constants

```python
from id_authority import (
    MAX_ID_LENGTH,           # 25
    CHAPTER_ID_LENGTH,       # 6 (e.g., "ch0001")
    SECTION_MARKER,          # "s"
    SECTION_DIGITS_LEVEL_1_2, # 4 (e.g., "s0001")
    SECTION_DIGITS_LEVEL_3_5, # 2 (e.g., "s01")
    ELEMENT_DIGITS,          # 4 (e.g., "fg0001")
)
```

### 5.4 Validation Patterns

```python
from id_authority import (
    CHAPTER_ID_PATTERN,      # Matches: ch0001, pr0001, ap0001
    SECTION_ID_PATTERN,      # Matches: ch0001s0001, ch0001s0001s0002
    ELEMENT_ID_PATTERN,      # Matches: ch0001s0001fg0001
    VALID_ID_PATTERN,        # Matches any valid ID format
)
```

---

## 6. Usage Guide

### 6.1 Basic Usage

```python
from id_authority import get_authority

# Get the singleton instance
authority = get_authority()

# Phase 1: Register all chapters with initial IDs
for filename in epub_files:
    chapter_id = f"ch{counter:04d}"
    authority.register_chapter(filename, chapter_id)

# Phase 2.5: Update chapter IDs with correct prefixes
authority.update_chapter_id("preface.xhtml", "ch0001", "pr0001")
authority.update_chapter_id("index.xhtml", "ch0015", "in0001")

# Phase 3: Map element IDs during conversion
authority.map_id("pr0001", "fig-cover", "pr0001s0001fg0001")
authority.map_id("ch0001", "table-1-1", "ch0001s0001ta0001")

# Phase 4: Resolve linkends during link fixup
for link in links:
    resolved = authority.resolve_linkend(link.source_id, link.context_chapter)
    if resolved:
        link.linkend = resolved
```

### 6.2 Validation

```python
from id_authority import IDParser

parser = IDParser()

# Validate before using
is_valid, error = parser.validate(proposed_id)
if not is_valid:
    logger.warning(f"Invalid ID: {error}")

# Parse to extract components
parsed = parser.parse(id_value)
if parsed:
    print(f"Chapter: {parsed.chapter_prefix}{parsed.chapter_num:04d}")
    print(f"Sections: {parsed.sections}")
    print(f"Element: {parsed.element_code}{parsed.element_num:04d}")
```

### 6.3 Debugging

```python
# Get resolution history for a specific source ID
history = authority.get_resolution_history("fig-intro")
for record in history:
    print(f"{record.timestamp}: {record.source_id} -> {record.generated_id}")
    print(f"  Context: {record.context_chapter}, Status: {record.resolution_status}")

# Find all unresolved links
unresolved = authority.find_unresolved_links()
for source_id, attempts in unresolved.items():
    print(f"Unresolved: {source_id} (attempted {len(attempts)} times)")

# Export state for debugging
state = authority.export_state()
with open("id_authority_state.json", "w") as f:
    json.dump(state, f, indent=2)
```

---

## 7. Integration Points

### 7.1 epub_to_structured_v2.py

Replace fragmented ID logic with IDAuthority calls:

```python
# Before (fragmented)
chapter_map = {}  # Local dictionary
id_mappings = {}  # Another local dictionary
if re.match(r'^[a-z]{2}\d{4}$', id):  # Inline validation
    ...

# After (centralized)
from id_authority import get_authority

authority = get_authority()
authority.register_chapter(filename, chapter_id)
authority.map_id(chapter_id, source_id, generated_id)
resolved = authority.resolve_linkend(linkend, context)
```

### 7.2 comprehensive_dtd_fixer.py

Use IDAuthority for missing ID detection:

```python
# Before
missing_ids = set()
for linkend in all_linkends:
    if linkend not in all_defined_ids:
        missing_ids.add(linkend)

# After
from id_authority import get_authority

authority = get_authority()
unresolved = authority.find_unresolved_links()
# Only create anchors for truly unresolved links
# Avoid creating anchors that will just be removed as orphans
```

### 7.3 package.py

Use IDAuthority for chapter ID lookups:

```python
# Before
if filename in chapter_map:
    chapter_id = chapter_map[filename]

# After
from id_authority import get_authority

authority = get_authority()
chapter_id = authority.get_chapter_id(filename)
```

---

## 8. Migration from Legacy System

### 8.1 Deprecation Plan

The following modules contain redundant functionality that should be migrated:

| Module | Status | Migration Notes |
|--------|--------|-----------------|
| `id_mapper.py` | **Deprecated** | Use `IDAuthority.map_id()` and `resolve_linkend()` |
| `id_tracker.py` | **Deprecated** | Use `IDAuthority.register_chapter()` |
| `id_generator.py` | **Partial** | Keep element counter logic, import codes from IDAuthority |

### 8.2 Backward Compatibility

For gradual migration, the `id_authority.py` module provides compatibility functions:

```python
from id_authority import (
    # Legacy-compatible function signatures
    get_chapter_id,           # Like id_mapper.get_chapter_id()
    map_source_to_generated,  # Like id_mapper.map_id()
    resolve_linkend,          # Like id_mapper.resolve_linkend()
)
```

### 8.3 Migration Steps

1. **Phase 1**: Import IDAuthority alongside existing code
   - Add `from id_authority import get_authority` to affected files
   - Call IDAuthority methods in parallel with existing code (shadow mode)
   - Compare results and log discrepancies

2. **Phase 2**: Switch primary source
   - Make IDAuthority the primary source
   - Keep legacy code for verification
   - Monitor for issues

3. **Phase 3**: Remove legacy code
   - Delete redundant code from `id_mapper.py` and `id_tracker.py`
   - Keep only utility functions that don't duplicate IDAuthority

---

## 9. API Reference

### 9.1 IDAuthority Class

```python
class IDAuthority:
    """Singleton facade for centralized ID management."""

    def register_chapter(self, source_file: str, chapter_id: str) -> None:
        """Register a chapter with its ID."""

    def update_chapter_id(self, source_file: str, old_id: str, new_id: str) -> None:
        """Update chapter ID with cascading updates to all mappings."""

    def get_chapter_id(self, source_file: str) -> Optional[str]:
        """Get chapter ID for a source file."""

    def map_id(self, chapter_id: str, source_id: str, generated_id: str) -> None:
        """Map a source element ID to a generated ID."""

    def resolve_linkend(self, source_id: str, context_chapter: str = None) -> Optional[str]:
        """Resolve a source ID to its generated ID."""

    def get_resolution_history(self, source_id: str) -> List[ResolutionRecord]:
        """Get resolution history for debugging."""

    def find_unresolved_links(self) -> Dict[str, List[ResolutionRecord]]:
        """Find all links that failed to resolve."""

    def export_state(self) -> Dict:
        """Export complete state for debugging."""

    def reset(self) -> None:
        """Reset all state (for testing or new conversion)."""
```

### 9.2 IDParser Class

```python
class IDParser:
    """Parse and validate IDs according to authoritative definitions."""

    def parse(self, id_value: str) -> Optional[ParsedID]:
        """Parse an ID into its components."""

    def validate(self, id_value: str) -> Tuple[bool, Optional[str]]:
        """Validate an ID. Returns (is_valid, error_message)."""

    def extract_chapter_id(self, id_value: str) -> Optional[str]:
        """Extract the chapter ID from any ID."""

    def extract_section_path(self, id_value: str) -> Optional[str]:
        """Extract the section path (without element)."""

    def is_xsl_recognized(self, element_code: str) -> bool:
        """Check if an element code is recognized by XSL."""
```

### 9.3 Data Classes

```python
@dataclass
class ParsedID:
    """Parsed components of an ID."""
    original: str
    chapter_prefix: str
    chapter_num: int
    sections: List[Tuple[int, int]]  # (number, digit_count)
    element_code: Optional[str]
    element_num: Optional[int]
    is_valid: bool
    validation_error: Optional[str]

@dataclass
class ResolutionRecord:
    """Audit record for a linkend resolution attempt."""
    source_id: str
    context_chapter: Optional[str]
    generated_id: Optional[str]
    resolution_status: str  # "resolved", "not_found", "ambiguous"
    timestamp: str
    resolution_method: str  # "exact", "fuzzy", "fallback"
```

---

## 10. Troubleshooting

### 10.1 Common Issues

#### "Anchor Creation for Missing ID" but anchor not in output

**Cause**: Pass 2 creates anchors, Pass 3 removes them as orphans, but verification log shows intermediate state.

**Solution**: The IDAuthority tracks resolution status. Check `find_unresolved_links()` to see which links truly need anchors.

#### Phantom IDs like `ch0007s0010` when chapter has only 7 sections

**Cause**: Stale mappings from Phase 1 when all files were registered as `ch####`.

**Solution**: Use `update_chapter_id()` to cascade updates when chapter IDs change. The old mappings are automatically updated.

#### XSL link resolution failures

**Cause**: Using non-XSL-recognized element codes for cross-references.

**Solution**: Check `is_xsl_recognized(code)` before creating cross-reference links. Only `fg`, `ta`, `eq`, `gl`, `bib`, `qa`, `pr`, `vd`, `ad` codes generate popup links.

### 10.2 Debugging Commands

```python
# Export complete state
authority = get_authority()
state = authority.export_state()
print(json.dumps(state, indent=2))

# Check chapter registrations
for filename, chapter_id in authority._registry._chapter_map.items():
    print(f"{filename} -> {chapter_id}")

# Check ID mappings (keys are tuples of (chapter_id, source_id))
for key, value in authority._registry._id_lookup.items():
    chapter_id, source_id = key
    print(f"{chapter_id}:{source_id} -> {value}")

# Check resolution history
history = authority.get_resolution_history("problematic-id")
for record in history:
    print(record)
```

### 10.3 Validation Errors

| Error | Meaning | Fix |
|-------|---------|-----|
| "ID exceeds 25 characters" | Generated ID too long | Use 2-digit section counters for sect3+ |
| "Invalid chapter prefix" | Unknown 2-letter prefix | Use a valid ChapterPrefix value |
| "Invalid element code" | Unknown element code | Use a valid ElementCode value |
| "ID contains invalid characters" | Hyphens, underscores, etc. | Sanitize to lowercase alphanumeric only |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Feb 2026 | Initial architecture with IDAuthority, ChapterRegistry, LinkendResolver |

---

## See Also

- [ID_NAMING_CONVENTION.md](ID_NAMING_CONVENTION.md) - ID format specifications
- [R2_LINKEND_AND_TOC_RULESET.md](R2_LINKEND_AND_TOC_RULESET.md) - Link resolution rules
- [CHAPTER_NAMING_CONVENTIONS.md](CHAPTER_NAMING_CONVENTIONS.md) - Chapter naming rules
