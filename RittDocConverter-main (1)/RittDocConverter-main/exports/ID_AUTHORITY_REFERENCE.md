# IDAuthority Complete Reference

This document provides comprehensive documentation of the IDAuthority system - the centralized ID management system for EPUB to DocBook XML conversion.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [ID Lifecycle & State Machine](#2-id-lifecycle--state-machine)
3. [Key Classes & Components](#3-key-classes--components)
4. [Pipeline Integration](#4-pipeline-integration)
5. [Cascading Updates](#5-cascading-updates)
6. [Resolution Strategies](#6-resolution-strategies)
7. [Thread Safety & Parallel Processing](#7-thread-safety--parallel-processing)
8. [API Reference](#8-api-reference)
9. [Usage Examples](#9-usage-examples)

---

## 1. Architecture Overview

### 1.1 Design Principles

The IDAuthority system follows these principles:

1. **Single Source of Truth**: All ID management goes through one central authority
2. **Atomic Updates**: Chapter ID changes cascade to all dependent mappings
3. **Audit Trail**: Full history of ID transformations for debugging
4. **Thread Safety**: Safe for concurrent conversions in API environments
5. **Two-Phase Processing**: Prescan phase enables accurate cross-chapter linking

### 1.2 Component Architecture

```
IDAuthority (Facade)
├── IDParser
│   └── ParsedID (dataclass)
├── ChapterRegistry
│   ├── ChapterMapping
│   ├── IDRecord (with state machine)
│   └── SourceID (prescan data)
└── LinkendResolver
    ├── ResolutionResult
    └── LinkendResolution (quality tracking)
```

### 1.3 Singleton Pattern

```python
from id_authority import get_authority, reset_authority

# Get the global singleton instance
authority = get_authority()

# Reset for new conversion (clears all state)
reset_authority()
```

---

## 2. ID Lifecycle & State Machine

### 2.1 ID States

```
┌─────────────┐
│  PRESCANNED │  Found in source EPUB during pre-scan
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ REGISTERED  │  Chapter mapping established
└──────┬──────┘
       │
       ├────────────────────────┐
       ▼                        ▼
┌─────────────┐          ┌─────────────┐
│   MAPPED    │          │   DROPPED   │
│             │          │             │
│ source_id → │          │ Intentional │
│ generated_id│          │ exclusion   │
└──────┬──────┘          └─────────────┘
       │
       ▼
┌─────────────┐
│  FINALIZED  │  Final ID in output XML
└─────────────┘
```

### 2.2 State Definitions

| State | Description | Trigger |
|-------|-------------|---------|
| `PRESCANNED` | ID found in source EPUB | `prescan_epub_file()` |
| `REGISTERED` | Chapter mapping established | `register_chapter()` |
| `MAPPED` | Source → Generated mapping created | `map_id()` |
| `FINALIZED` | Final ID in output (post-processing complete) | Final validation |
| `DROPPED` | Intentionally excluded from output | `mark_dropped()` |

### 2.3 IDRecord Structure

```python
@dataclass
class IDRecord:
    source_id: str           # Original ID from EPUB (e.g., "Fig1")
    source_file: str         # EPUB filename
    chapter_id: str          # Chapter ID (e.g., "ch0001")
    element_type: str        # Element type (figure, table, etc.)
    generated_id: str        # Generated XML ID (e.g., "ch0001s0001fg0001")
    state: IDState           # Current state
    drop_reason: str         # Reason if dropped
    history: List[str]       # Audit trail
    created_at: str          # ISO timestamp
```

### 2.4 Audit Trail Example

```
[2024-01-15T10:30:00] prescanned → registered: Chapter ch0007 registered
[2024-01-15T10:30:01] registered → mapped: Fig1 → ch0007s0001fg0001
[2024-01-15T10:30:05] mapped → mapped: Chapter ID changed: ch0007→in0001, Generated: ch0007s0001fg0001→in0001s0001fg0001
```

---

## 3. Key Classes & Components

### 3.1 IDAuthority (Facade)

The main entry point that provides a unified interface:

```python
class IDAuthority:
    def __init__(self):
        self.registry = ChapterRegistry()
        self.resolver = LinkendResolver(self.registry)
        self.parser = IDParser()
```

### 3.2 ChapterRegistry

Manages chapter mappings and ID records with atomic updates:

```python
class ChapterRegistry:
    # Primary mappings
    _epub_to_chapter: Dict[str, ChapterMapping]  # epub_file → mapping
    _chapter_to_epub: Dict[str, str]             # chapter_id → epub_file

    # ID tracking
    _id_records: Dict[Tuple[str, str], IDRecord] # (chapter_id, source_id) → record
    _id_lookup: Dict[Tuple[str, str], str]       # (chapter_id, source_id) → generated_id
    _valid_ids: Set[str]                         # Valid IDs in final output
```

### 3.3 LinkendResolver

Resolves linkends with quality tracking:

```python
class LinkendResolver:
    def resolve(self, source_id, source_chapter, target_chapter) -> ResolutionResult
```

**Resolution Quality Levels:**

| Quality | Description |
|---------|-------------|
| `EXACT` | Direct match to target ID |
| `MAPPED` | Resolved via source→generated mapping |
| `CASE_INSENSITIVE` | Case-insensitive match |
| `FUZZY` | Levenshtein distance ≤ 2 |
| `DOWNGRADED_CHAPTER` | Fell back to chapter-level link |
| `DOWNGRADED_SECTION` | Fell back to section-level link |
| `RECORD_LOOKUP` | Found via ID record transformation |
| `LOST` | Could not be resolved |

### 3.4 ChapterMapping

```python
@dataclass
class ChapterMapping:
    epub_file: str      # "chapter1.xhtml"
    chapter_id: str     # "ch0001"
    xml_file: str       # "ch0001.xml"
    element_type: str   # "chapter", "appendix", "index", etc.
    created_at: str     # ISO timestamp
```

---

## 4. Pipeline Integration

### 4.1 Conversion Pipeline Stages

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         EPUB to DocBook Pipeline                              │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│  STAGE 1: PRESCAN                                                             │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ prescan_epub_file(filepath) → Dict[str, SourceID]                       │ │
│  │ register_prescanned_file(epub_filename, source_ids)                     │ │
│  │                                                                          │ │
│  │ Purpose: Extract all IDs from EPUB before conversion                    │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                   ▼                                          │
│  STAGE 2: CHAPTER REGISTRATION (Phase 1)                                     │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ register_chapter("chapter1.xhtml", "ch0001", "ch0001.xml", "chapter")   │ │
│  │                                                                          │ │
│  │ Purpose: Initial chapter mapping (may use wrong prefix initially)       │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                   ▼                                          │
│  STAGE 3: ELEMENT PROCESSING                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ For each element with an ID:                                            │ │
│  │   map_id(chapter_id, "Fig1", "ch0001s0001fg0001", "figure")             │ │
│  │   -- OR --                                                              │ │
│  │   mark_dropped(chapter_id, "pagebreak1", "pagebreak element")          │ │
│  │                                                                          │ │
│  │ register_valid_id(generated_id)  # Track IDs in actual output           │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                   ▼                                          │
│  STAGE 4: CHAPTER TYPE CORRECTION (Phase 2.5)                                │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ register_chapter("index.xhtml", "in0001", "in0001.xml", "index")        │ │
│  │                                                                          │ │
│  │ Purpose: Update chapter IDs based on actual content type                │ │
│  │ TRIGGERS: Cascading update of all dependent ID mappings                 │ │
│  │                                                                          │ │
│  │ Example: ch0007 → in0001 causes:                                        │ │
│  │   ch0007s0001fg0001 → in0001s0001fg0001                                 │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                   ▼                                          │
│  STAGE 5: LINKEND RESOLUTION                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ For each <link linkend="..."> or <xref linkend="...">:                  │ │
│  │                                                                          │ │
│  │   # Check if dropped                                                    │ │
│  │   if is_dropped(chapter_id, linkend):                                   │ │
│  │       convert_to_phrase(element)                                        │ │
│  │                                                                          │ │
│  │   # Resolve                                                             │ │
│  │   resolved = resolve(chapter_id, linkend)                               │ │
│  │   if resolved:                                                          │ │
│  │       element.set('linkend', resolved)                                  │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                   ▼                                          │
│  STAGE 6: VALIDATION & FIXING                                                │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ validate_linkends_in_document(root_element)                             │ │
│  │ apply_exact_source_id_mappings(root_element)                            │ │
│  │                                                                          │ │
│  │ Purpose: Final validation and fixing of any remaining invalid linkends  │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                               │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 File Locations

| Stage | Primary File | Key Lines |
|-------|--------------|-----------|
| Prescan | `epub_to_structured_v2.py` | ~13082-13085 |
| Chapter Registration | `epub_to_structured_v2.py` | ~13165-13252 |
| Element Processing | `epub_to_structured_v2.py` | ~180-210, 6437 |
| Linkend Resolution | `epub_to_structured_v2.py` | ~7119-7176 |
| Validation | `comprehensive_dtd_fixer.py` | ~2814-2815 |

---

## 5. Cascading Updates

### 5.1 The Problem

When a chapter's type is determined (e.g., index content discovered), the chapter ID may need to change:

```
Before: chapter1.xhtml → ch0007 (assumed chapter)
After:  chapter1.xhtml → in0001 (actually index)
```

All IDs mapped under `ch0007` must be updated to `in0001`:

```
ch0007s0001fg0001 → in0001s0001fg0001
ch0007s0001ta0001 → in0001s0001ta0001
```

### 5.2 The Solution: Atomic Cascading

When `register_chapter()` is called with a different chapter_id for an existing epub_file:

```python
def _cascade_chapter_update(self, epub_file: str, old_id: str, new_id: str) -> None:
    """Cascade chapter ID change to all dependent records."""

    # 1. Find all ID records for old chapter
    records_to_update = [
        (key, record) for key, record in self._id_records.items()
        if key[0] == old_id
    ]

    # 2. Update each record
    for (_, source_id), record in records_to_update:
        # Remove old key
        del self._id_records[(old_id, source_id)]

        # Update chapter_id in record
        record.chapter_id = new_id

        # Update generated_id prefix
        if record.generated_id and record.generated_id.startswith(old_id):
            old_gen = record.generated_id
            record.generated_id = new_id + record.generated_id[len(old_id):]

            # Update valid_ids set
            if old_gen in self._valid_ids:
                self._valid_ids.discard(old_gen)
                self._valid_ids.add(record.generated_id)

        # Add with new key
        self._id_records[(new_id, source_id)] = record

    # 3. Notify listeners
    for listener in self._update_listeners:
        listener(old_id, new_id)
```

### 5.3 Update Listeners

External components can subscribe to chapter ID changes:

```python
def my_update_handler(old_id: str, new_id: str):
    print(f"Chapter changed: {old_id} → {new_id}")

authority.add_chapter_update_listener(my_update_handler)
```

---

## 6. Resolution Strategies

### 6.1 Direct Resolution

```python
# Primary lookup
resolved = authority.resolve(chapter_id, source_id)
```

### 6.2 Fallback Resolution Chain

When direct resolution fails, the system tries:

1. **Direct source ID → generated ID mapping**
2. **Case-insensitive matching**
3. **Fuzzy matching** (Levenshtein distance ≤ 2, same chapter only)
4. **Chapter-level fallback** (DOWNGRADE WARNING)
5. **Section-level fallback** (DOWNGRADE WARNING)
6. **ID record transformation lookup**

### 6.3 Cross-Chapter Linking

For non-citation IDs, cross-chapter resolution is attempted:

```python
# Try target chapter first
resolved = registry.resolve(target_chapter, source_id)

# If not found, try source chapter
if not resolved and not is_citation:
    resolved = registry.resolve(source_chapter, source_id)
```

**Citation IDs** (matching `^(CR|Ref|bib|fn|note)\d+$`) are restricted to their own chapter.

### 6.4 Dropped ID Handling

```python
if authority.is_dropped(chapter_id, source_id):
    reason = authority.get_drop_reason(chapter_id, source_id)
    # Convert link to phrase element
```

---

## 7. Thread Safety & Parallel Processing

### 7.1 Internal Locking

The ChapterRegistry uses `threading.RLock()` for all state modifications:

```python
class ChapterRegistry:
    def __init__(self):
        self._lock = threading.RLock()

    def map_id(self, ...):
        with self._lock:
            # Thread-safe modification
```

### 7.2 Isolated Instances for API

For API/parallel processing, use `ConversionContext`:

```python
from thread_safe_context import ConversionContext

with ConversionContext() as ctx:
    # Each context gets its own IDAuthority instance
    ctx.authority.register_chapter(...)
    ctx.authority.map_id(...)
```

### 7.3 Thread-Local Storage

```python
from thread_safe_context import get_current_authority

# Get the IDAuthority for the current thread
authority = get_current_authority()
```

---

## 8. API Reference

### 8.1 Chapter Management

| Method | Signature | Description |
|--------|-----------|-------------|
| `register_chapter` | `(epub_file, chapter_id, xml_file=None, element_type="chapter")` | Register or update chapter mapping. Triggers cascade if chapter_id changes. |
| `get_chapter_id` | `(epub_file) → Optional[str]` | Get chapter ID for an EPUB file |
| `get_chapter_mapping` | `(epub_file) → Optional[ChapterMapping]` | Get full chapter mapping |

### 8.2 ID Mapping

| Method | Signature | Description |
|--------|-----------|-------------|
| `map_id` | `(chapter_id, source_id, generated_id, element_type="", source_file="")` | Map source ID to generated ID |
| `mark_dropped` | `(chapter_id, source_id, reason, element_type="", source_file="")` | Mark an ID as dropped |
| `is_dropped` | `(chapter_id, source_id) → bool` | Check if an ID is dropped |
| `get_drop_reason` | `(chapter_id, source_id) → Optional[str]` | Get drop reason |

### 8.3 Resolution

| Method | Signature | Description |
|--------|-----------|-------------|
| `resolve` | `(chapter_id, source_id) → Optional[str]` | Direct lookup |
| `resolve_linkend` | `(source_id, source_chapter, target_chapter=None) → str` | Full resolution with fallbacks (legacy) |
| `resolve_linkend_full` | `(source_id, source_chapter, target_chapter=None) → ResolutionResult` | Full resolution with details |

### 8.4 Valid ID Management

| Method | Signature | Description |
|--------|-----------|-------------|
| `register_valid_id` | `(generated_id)` | Register a valid ID in output |
| `is_valid_id` | `(generated_id) → bool` | Check if ID exists in output |
| `build_valid_ids_from_xml` | `(root_element) → int` | Build cache from XML tree |

### 8.5 Validation

| Method | Signature | Description |
|--------|-----------|-------------|
| `validate_id` | `(id_string) → Tuple[bool, Optional[str]]` | Validate ID format |
| `parse_id` | `(id_string) → ParsedID` | Parse ID into components |
| `is_xsl_resolvable` | `(id_string) → bool` | Check XSL recognition |
| `validate_linkends_in_document` | `(root_element) → LinkendValidationReport` | Validate all linkends |

### 8.6 Pre-scan

| Method | Signature | Description |
|--------|-----------|-------------|
| `prescan_epub_file` | `(filepath, content=None) → Dict[str, SourceID]` | Extract IDs from EPUB file |
| `register_prescanned_file` | `(epub_filename, source_ids)` | Register pre-scanned IDs |

### 8.7 Audit & Export

| Method | Signature | Description |
|--------|-----------|-------------|
| `get_audit_trail` | `(chapter_id, source_id) → List[str]` | Get ID history |
| `get_stats` | `() → dict` | Get statistics |
| `export_registry` | `(path: Path)` | Export to JSON |
| `export_resolution_log` | `(path: Path)` | Export resolution log |

### 8.8 Lifecycle

| Method | Signature | Description |
|--------|-----------|-------------|
| `reset` | `()` | Reset all state |
| `add_chapter_update_listener` | `(listener: Callable[[str, str], None])` | Subscribe to chapter changes |

---

## 9. Usage Examples

### 9.1 Basic Conversion Flow

```python
from id_authority import get_authority, reset_authority

# Start fresh
reset_authority()
authority = get_authority()

# Stage 1: Pre-scan
for epub_file in epub_files:
    source_ids = authority.prescan_epub_file(Path(epub_file))
    authority.register_prescanned_file(epub_file.name, source_ids)

# Stage 2: Register chapters
authority.register_chapter("chapter1.xhtml", "ch0001", "ch0001.xml", "chapter")
authority.register_chapter("index.xhtml", "ch0002", "ch0002.xml", "chapter")

# Stage 3: Map IDs during processing
authority.map_id("ch0001", "Fig1", "ch0001s0001fg0001", "figure")
authority.map_id("ch0001", "Table1", "ch0001s0001ta0001", "table")
authority.mark_dropped("ch0001", "pagebreak1", "pagebreak element")

# Stage 4: Correct chapter types (triggers cascade)
authority.register_chapter("index.xhtml", "in0001", "in0001.xml", "index")
# All ch0002* IDs are now in0001*

# Stage 5: Resolve linkends
resolved = authority.resolve("ch0001", "Fig1")
# Returns: "ch0001s0001fg0001"

# Stage 6: Validate
report = authority.validate_linkends_in_document(root_element)
print(report.get_quality_summary())
```

### 9.2 Thread-Safe API Usage

```python
from thread_safe_context import ConversionContext

def convert_epub(epub_path: str) -> str:
    with ConversionContext() as ctx:
        # Isolated authority instance
        ctx.authority.register_chapter(...)
        ctx.authority.map_id(...)

        # Process conversion
        result = process_epub(epub_path, ctx.authority)

        return result

# Safe for concurrent requests
from concurrent.futures import ThreadPoolExecutor

with ThreadPoolExecutor(max_workers=4) as executor:
    futures = [executor.submit(convert_epub, path) for path in epub_paths]
```

### 9.3 Debugging Unresolved Linkends

```python
authority = get_authority()

# Get failed resolutions
failed = authority.get_failed_resolutions()
for result in failed:
    print(f"Failed: {result.source_id}")
    print(f"  Error: {result.error}")
    print(f"  Audit: {result.audit_trail}")

# Get non-XSL-resolvable links (may not render correctly)
non_xsl = authority.get_non_xsl_resolutions()
for result in non_xsl:
    print(f"Non-XSL: {result.source_id} → {result.resolved_id}")
```

### 9.4 Export for Analysis

```python
from pathlib import Path

authority = get_authority()

# Export registry state
authority.export_registry(Path("debug/id_registry.json"))

# Export resolution log
authority.export_resolution_log(Path("debug/resolution_log.json"))

# Get statistics
stats = authority.get_stats()
print(f"Chapters: {stats['chapters_registered']}")
print(f"IDs mapped: {stats['ids_mapped']}")
print(f"IDs dropped: {stats['ids_dropped']}")
print(f"Chapter updates: {stats['chapter_updates']}")
```

---

## Appendix A: ID Format Reference

See [DOCBOOK_PROCESSING_RULES.md](./DOCBOOK_PROCESSING_RULES.md) for complete ID format rules including:

- Chapter ID formats (`ch0001`, `ap0001`, `in0001`, etc.)
- Section ID hierarchy (`ch0001s0001`, `ch0001s0001s0001`, etc.)
- Element ID formats (`ch0001s0001fg0001`, etc.)
- Maximum ID length (25 characters)

---

## Appendix B: Migration from Legacy Modules

The following legacy modules have been deprecated:

| Legacy Module | Replacement |
|--------------|-------------|
| `id_mapper.py` | `get_authority()`, `prescan_file()` |
| `id_tracker.py` | `get_authority()` methods |

Attempting to import these modules will raise `ImportError` with migration guidance.

---

## Appendix C: Statistics Tracking

The IDAuthority tracks these statistics:

```python
{
    'chapters_registered': int,    # Total chapters registered
    'ids_mapped': int,             # Total source→generated mappings
    'ids_dropped': int,            # Total IDs marked as dropped
    'chapter_updates': int,        # Chapter ID cascading updates
    'total_records': int,          # Total ID records
    'valid_ids': int,              # Valid IDs in output
    'epub_files_scanned': int,     # Files pre-scanned
    'total_source_ids': int,       # Source IDs found in pre-scan
}
```
