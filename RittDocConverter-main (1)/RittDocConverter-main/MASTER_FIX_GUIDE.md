# Master Fix Guide - All EPUB to DocBook Conversion Issues

This guide covers all common issues you may encounter after EPUB to DocBook conversion and how to fix them.

## 🚨 Common Issues and Quick Fixes

### Issue 1: REFERENCES Sections Have No TOC Links
**Symptom:** REFERENCES appears as plain text in TOC, not a clickable link  
**Quick Fix:**
```bash
python3 fix_bibliography_toc_links.py /path/to/xml/
```
📖 Details: `QUICK_START_BIBLIOGRAPHY_FIX.md`

---

### Issue 2: "Entity 'pr0001' not defined" Error
**Symptom:** Error when parsing Book.XML  
**Quick Fix:**
```bash
python3 fix_book_xml_entities.py Book.XML
```
📖 Details: `QUICK_FIX_ENTITY_ERRORS.md`

---

### Issue 3: Section IDs Out of Order
**Symptom:** REFERENCES has s0002 but appears as last section  
**Quick Fix:**
```bash
python3 renumber_section_ids.py /path/to/xml/
```
📖 Details: `QUICK_FIX_SECTION_ORDER.md`

---

## 📋 Complete Processing Workflow

Run these commands **in order** after EPUB conversion:

```bash
# Step 1: Fix bibliography IDs and TOC links
python3 fix_bibliography_toc_links.py extracted/

# Step 2: Renumber sections to match final order
python3 renumber_section_ids.py extracted/

# Step 3: Fix Book.XML entity references
python3 fix_book_xml_entities.py extracted/Book.XML --remove-orphans

# Step 4: Validate (optional)
xmllint --valid --noout extracted/Book.XML
```

## 🎯 What Each Script Does

| Script | Purpose | Safe to Run Multiple Times? |
|--------|---------|------------------------------|
| `fix_bibliography_toc_links.py` | Fix bibliography wrapper IDs | ✅ Yes |
| `renumber_section_ids.py` | Fix section numbering | ✅ Yes |
| `fix_book_xml_entities.py` | Fix entity declarations | ✅ Yes |

All scripts are safe to run multiple times - they only make changes when needed.


## 📊 Verification

After running all fixes, verify your output:

### Check 1: No Duplicates
```bash
python3 detect_duplicate_content.py /path/to/xml/
# Should show: "No duplicate content files found"
```

### Check 2: All Entities Declared
```bash
python3 fix_book_xml_entities.py Book.XML
# Should show: "All entity references are properly declared"
```

### Check 3: Bibliography IDs Correct
```bash
grep '<bibliography id="[^"]*bib"' *.xml
# Should return no results (no IDs ending in 'bib')
```

### Check 4: Section IDs Sequential
```bash
grep '<sect1 id=' ch0013.xml
# Should show: s0001, s0002, s0003 (in order)
```

### Check 5: XML Validates
```bash
xmllint --valid --noout Book.XML
# Should show: "Book.XML validates"
```

## 🔧 Troubleshooting

### "File still exists after removal"
- Check if file is open in an editor
- Check file permissions
- Try with sudo (if permission denied)

### "References still point to removed file"
- Run: `python3 fix_book_xml_entities.py Book.XML --remove-orphans`
- This cleans up orphan entity references

### "Duplicate detection finds false positives"
- Review the files manually
- Duplicates are based on content structure, not IDs
- Use `--verbose` to see more details

### "Want to restore removed file"
- Re-run EPUB conversion
- Or restore from backup
- Script doesn't create backups automatically

## 📚 Full Documentation

Each script has detailed documentation:

| Script | Full Documentation |
|--------|-------------------|
| `detect_duplicate_content.py` | `QUICK_FIX_DUPLICATE_FILES.md` |
| `fix_bibliography_toc_links.py` | `docs/BIBLIOGRAPHY_TOC_LINK_FIX.md` |
| `renumber_section_ids.py` | `QUICK_FIX_SECTION_ORDER.md` |
| `fix_book_xml_entities.py` | `docs/BOOK_XML_ENTITY_FIX.md` |

## 🎓 Understanding the Issues

### Why Duplicates Occur
- EPUB file path contains "preface" keyword → assigned pr****
- Same file also processed as regular content → assigned ch****
- Result: Same content, two different files

### Why Bibliography Needs Special IDs
- Bibliography wrapper uses sect1-style ID (11 chars)
- Only bibliomixed entries use 'bib' suffix
- Wrong ID format breaks TOC link resolution

### Why Sections Get Renumbered
- DTD requires bibliography at specific positions
- Code moves bibliography to comply with DTD
- IDs don't update automatically after movement

### Why Entity Declarations Matter
- Book.XML uses entities to include chapter files
- Each &pr0001; needs <!ENTITY pr0001 SYSTEM "pr0001.xml">
- Missing declarations cause validation errors

## 💡 Best Practices

1. **Always run detection before removal**
   - Review what will be kept/removed
   - Verify pr**** files are kept

2. **Run fixes in correct order**
   - Duplicates first (changes file list)
   - Bibliography IDs second (affects references)
   - Section renumbering third (updates IDs)
   - Entity cleanup last (removes orphans)

3. **Validate after each step**
   - Check that expected files exist
   - Verify no errors in logs
   - Test XML parsing

4. **Keep backups**
   - Scripts modify files in place
   - No automatic backup created
   - Keep a copy of original output

## 🆘 Need Help?

All scripts support `--help`:
```bash
python3 script_name.py --help
```

All scripts have test suites:
```bash
python3 test_script_name.py
```

## ✅ Summary

| Issue | Symptom | Fix Command |
|-------|---------|-------------|
| Missing REFERENCES links | Plain text in TOC | `fix_bibliography_toc_links.py` |
| Entity errors | "Entity not defined" | `fix_book_xml_entities.py` |
| Section order | s0002 at end | `renumber_section_ids.py` |
