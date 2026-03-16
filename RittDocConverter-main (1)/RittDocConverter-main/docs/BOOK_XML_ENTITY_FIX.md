# Book.XML Entity Reference Fix

## Problem Description

When validating or parsing Book.XML, you may encounter this error:

```
Entity 'pr0001' not defined, line 93, column 788
```

This occurs when Book.XML contains entity references like `&pr0001;` but these entities are not declared in the DOCTYPE section.

## Root Cause

Book.XML uses entity references to include chapter files:

```xml
<book>
  &pr0001;   <!-- References preface -->
  &ch0001;   <!-- References chapter 1 -->
  &ch0002;   <!-- References chapter 2 -->
</book>
```

For these references to work, they must be declared in the DOCTYPE:

```xml
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY ch0002 SYSTEM "ch0002.xml">
]>
```

If the DOCTYPE is missing entity declarations, or if entity references were added after the DOCTYPE was created, you'll get the "Entity not defined" error.

## Solution

The `fix_book_xml_entities.py` script automatically:

1. Scans Book.XML for all entity references (`&xxx;`)
2. Checks which entities are missing from DOCTYPE
3. Adds missing entity declarations
4. Optionally removes orphan references (entities without corresponding .xml files)

## Usage

### Basic Usage

Fix Book.XML by adding missing entity declarations:

```bash
python3 fix_book_xml_entities.py /path/to/Book.XML
```

### Remove Orphans

Remove entity references that don't have corresponding .xml files:

```bash
python3 fix_book_xml_entities.py /path/to/Book.XML --remove-orphans
```

This is useful when:
- Entity references were added by mistake
- Referenced files were deleted
- You want to clean up the Book.XML

## Examples

### Example 1: Missing Entity Declarations

**Before:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
]>
<book id="book0001">
  <title>My Book</title>
  &pr0001;   <!-- ❌ ERROR: Entity not declared -->
  &ch0001;   <!-- ✓ OK: Entity declared -->
  &ch0002;   <!-- ❌ ERROR: Entity not declared -->
</book>
```

**Running the fix:**
```bash
$ python3 fix_book_xml_entities.py Book.XML

INFO: Found 3 entity reference(s) in body: ['ch0001', 'ch0002', 'pr0001']
INFO: Found 1 declared entit(ies) in DOCTYPE: ['ch0001']
INFO: Missing 2 entit(ies): ['ch0002', 'pr0001']
INFO: ✓ Updated Book.XML
INFO:   Added 2 entit(ies)

============================================================
✓ Fixed Book.XML:
  Added 2 entity declaration(s)
============================================================
```

**After:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY ch0002 SYSTEM "ch0002.xml">
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
]>
<book id="book0001">
  <title>My Book</title>
  &pr0001;   <!-- ✓ OK: Now declared -->
  &ch0001;   <!-- ✓ OK: Still declared -->
  &ch0002;   <!-- ✓ OK: Now declared -->
</book>
```

### Example 2: Orphan Entity References

**Before:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd">
<book id="book0001">
  <title>My Book</title>
  &pr0001;   <!-- File exists: pr0001.xml ✓ -->
  &ch0001;   <!-- File exists: ch0001.xml ✓ -->
  &ch0099;   <!-- File missing: ch0099.xml ❌ -->
</book>
```

**Running the fix with --remove-orphans:**
```bash
$ python3 fix_book_xml_entities.py Book.XML --remove-orphans

INFO: Found 3 entity reference(s) in body: ['ch0001', 'ch0099', 'pr0001']
INFO: Found 0 declared entit(ies) in DOCTYPE: []
WARNING: Entity &ch0099; has no corresponding file ch0099.xml
INFO: Removed orphan entity reference &ch0099;
INFO: Missing 2 entit(ies): ['ch0001', 'pr0001']
INFO: ✓ Updated Book.XML
INFO:   Added 2 entit(ies)
INFO:   Removed 1 orphan(s)

============================================================
✓ Fixed Book.XML:
  Added 2 entity declaration(s)
  Removed 1 orphan reference(s)
============================================================
```

**After:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" [
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
]>
<book id="book0001">
  <title>My Book</title>
  &pr0001;   <!-- ✓ OK: Declared and file exists -->
  &ch0001;   <!-- ✓ OK: Declared and file exists -->
  <!-- ch0099 reference removed because file doesn't exist -->
</book>
```

## How It Works

### 1. Entity Reference Detection

The script scans the XML body for entity references:

```python
pattern = r'&([a-zA-Z0-9_]+);'
```

This matches patterns like `&pr0001;`, `&ch0001;`, etc.

### 2. DOCTYPE Declaration Parsing

The script extracts existing entity declarations:

```python
pattern = r'<!ENTITY\s+([a-zA-Z0-9_]+)\s+SYSTEM\s+"[^"]*">'
```

This finds all `<!ENTITY name SYSTEM "file.xml">` declarations.

### 3. Missing Entity Detection

The script compares references with declarations:

```python
missing = entity_refs - declared
```

### 4. DOCTYPE Reconstruction

The script rebuilds the DOCTYPE with all required entities:

```xml
<!DOCTYPE book PUBLIC "..." "..." [
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY pr0001 SYSTEM "pr0001.xml">
  ...
]>
```

Entities are sorted alphabetically for consistency.

## Testing

Run the test suite to verify the fix works correctly:

```bash
python3 test_fix_book_xml_entities.py
```

The test suite covers:
- Finding entity references in XML
- Finding declared entities in DOCTYPE
- Building DOCTYPE with entities
- Fixing missing entity declarations
- Removing orphan entity references
- No changes when everything is correct

Expected output:
```
✓ Test passed: find_entity_references
✓ Test passed: find_declared_entities
✓ Test passed: build_doctype_with_entities
✓ Test passed: fix_missing_entities
✓ Test passed: remove_orphan_entities
✓ Test passed: no_changes_needed

============================================================
✓ All 6 tests passed!
```

## Common Issues

### Issue: "Entity not defined" during validation

**Cause:** DOCTYPE is missing entity declarations.

**Solution:** Run `fix_book_xml_entities.py Book.XML`

### Issue: Multiple entity declarations for the same entity

**Cause:** Manual editing created duplicates.

**Solution:** The script automatically removes duplicates by rebuilding the DOCTYPE.

### Issue: Entity reference points to missing file

**Cause:** File was deleted or never created.

**Solutions:**
1. Create the missing file, OR
2. Run with `--remove-orphans` to remove the reference

### Issue: Book.XML validates but chapters don't load

**Cause:** Entity references are declared but files don't exist.

**Solution:** Run with `--remove-orphans` to clean up invalid references.

## Integration with Processing Pipeline

### When to Run This Fix

Run `fix_book_xml_entities.py` after:
- Manually editing Book.XML
- Adding new chapters/prefaces/appendices
- Package creation/assembly
- Extracting and modifying an existing package

### Pipeline Position

```
1. epub_to_structured_v2.py (or other conversion)
2. package.py (creates Book.XML with fragments)
3. fix_book_xml_entities.py ← RUN HERE if needed
4. Validation (xmllint, dtd_compliance.py)
5. Publishing
```

## Advanced Usage

### Check What Would Change (Dry Run)

To see what the script would do without making changes:

```bash
python3 -c "
from pathlib import Path
from fix_book_xml_entities import find_entity_references, find_declared_entities

content = Path('Book.XML').read_text()
refs = find_entity_references(content)
declared = find_declared_entities(content)
missing = refs - declared

print(f'Entity references: {sorted(refs)}')
print(f'Declared entities: {sorted(declared)}')
print(f'Missing entities: {sorted(missing)}')
"
```

### Verify All Entities Have Files

Check that all entity references have corresponding .xml files:

```bash
# After running the fix
python3 fix_book_xml_entities.py Book.XML --remove-orphans

# This will report any missing files and remove their references
```

## Related Documentation

- [package.py](../package.py) - Creates Book.XML with entity declarations
- [R2_LINKEND_AND_TOC_RULESET.md](R2_LINKEND_AND_TOC_RULESET.md) - ID format rules
- [RittDocBook DTD](http://LOCALHOST/dtd/V1.1/RittDocBook.dtd) - Complete DTD specification

## Technical Details

### DOCTYPE Structure

The complete DOCTYPE structure:

```xml
<!DOCTYPE book 
  PUBLIC "-//RIS Dev//DTD DocBook V4.3 -Based Variant V1.1//EN" 
  "http://LOCALHOST/dtd/V1.1/RittDocBook.dtd" 
  [
    <!-- Internal subset with entity declarations -->
    <!ENTITY entity_name SYSTEM "filename.xml">
    ...
  ]
>
```

### Standard XML Entities

The script automatically ignores standard XML entities:
- `&lt;` (less than)
- `&gt;` (greater than)
- `&amp;` (ampersand)
- `&quot;` (quote)
- `&apos;` (apostrophe)

These don't need to be declared.

### Entity Naming Convention

Entity names follow the chapter ID pattern:
- `pr0001` - Preface 1
- `ch0001` - Chapter 1
- `ap0001` - Appendix A
- `gl0001` - Glossary
- `in0001` - Index
- `pt0001` - Part 1

See [R2_LINKEND_AND_TOC_RULESET.md](R2_LINKEND_AND_TOC_RULESET.md) for complete ID format rules.

## Summary

**Problem:** "Entity 'xxx' not defined" error when parsing Book.XML

**Root Cause:** DOCTYPE missing entity declarations for referenced entities

**Solution:** Run `fix_book_xml_entities.py` to automatically add missing declarations

**Result:** Book.XML validates and parses correctly ✓
