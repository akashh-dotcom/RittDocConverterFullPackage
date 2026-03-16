# Quick Fix: Book.XML Entity Errors

## The Error You're Seeing

```
Entity 'pr0001' not defined, line 93, column 788
```

## What This Means

Your Book.XML file is trying to use `&pr0001;` but it's not declared in the DOCTYPE header.

## The Fix (One Command!)

```bash
python3 fix_book_xml_entities.py /path/to/Book.XML
```

That's it! The script will:
1. Find all entity references in your Book.XML (like `&pr0001;`, `&ch0001;`)
2. Check which ones are missing from DOCTYPE
3. Add the missing declarations automatically
4. Save the fixed file

## Example

**Your Book.XML currently looks like this:**
```xml
<!DOCTYPE book ... [
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!-- pr0001 is MISSING! -->
]>
<book>
  &pr0001;  <!-- ❌ ERROR: Not declared -->
  &ch0001;  <!-- ✓ OK -->
</book>
```

**After running the fix:**
```xml
<!DOCTYPE book ... [
  <!ENTITY ch0001 SYSTEM "ch0001.xml">
  <!ENTITY pr0001 SYSTEM "pr0001.xml">  <!-- ✓ Added -->
]>
<book>
  &pr0001;  <!-- ✓ OK: Now declared -->
  &ch0001;  <!-- ✓ OK -->
</book>
```

## What You'll See

```bash
$ python3 fix_book_xml_entities.py Book.XML

INFO: Found 2 entity reference(s) in body: ['ch0001', 'pr0001']
INFO: Found 1 declared entit(ies) in DOCTYPE: ['ch0001']
INFO: Missing 1 entit(ies): ['pr0001']
INFO: ✓ Updated Book.XML
INFO:   Added 1 entit(ies)

============================================================
✓ Fixed Book.XML:
  Added 1 entity declaration(s)
============================================================
```

## If You Get "No such file" Warnings

If the script reports that some .xml files don't exist:

```bash
WARNING: Entity &ch0099; has no corresponding file ch0099.xml
```

You have two options:

**Option 1:** Create the missing file
```bash
# Create the missing chapter file
touch ch0099.xml
```

**Option 2:** Remove the reference (if it's a mistake)
```bash
python3 fix_book_xml_entities.py Book.XML --remove-orphans
```

The `--remove-orphans` flag will:
- Remove entity references that don't have corresponding .xml files
- Clean up your Book.XML

## Full Documentation

For more details, see [docs/BOOK_XML_ENTITY_FIX.md](docs/BOOK_XML_ENTITY_FIX.md)
