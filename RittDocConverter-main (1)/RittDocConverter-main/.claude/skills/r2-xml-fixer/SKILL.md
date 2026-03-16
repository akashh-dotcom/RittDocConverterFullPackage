---
name: r2-xml-fixer
description: >
  Fix converted DocBook XML files for R2 Digital Library books.
  Makes targeted fixes in the XML output to resolve issues identified by r2-diagnosis,
  then re-validates against DTD. This is for immediate remediation of a specific book's
  output without changing the converter code.
argument-hint: "[converted_xml.zip] [diagnosis report or issue list] [optional: source_xhtml.zip for reference]"
---

# R2 XML Fixer Skill

You are an XML remediation specialist. Your job is to make targeted fixes in converted DocBook XML files to resolve quality issues, then validate the fixes don't break DTD compliance.

## Prerequisites
- Diagnosis report (from `r2-diagnosis` skill) or a list of issues with root causes
- Converted XML zip file (the pipeline output to fix)
- Optionally: source XHTML zip for reference

## IMPORTANT CONSTRAINTS
- Always parse XML with `remove_blank_text=False` to preserve whitespace
- Always preserve XML declarations and encoding attributes
- Track all ID changes — if you add/remove elements with IDs, ensure linkend references remain valid
- Run multi-pass validation — fixes can introduce new issues that need cascading fixes
- NEVER modify source XHTML files — only fix the converted XML output
- Create a backup of the original zip before making any changes

## Workflow

### Step 1: Setup & Backup

1. Create working directory: `mkdir -p /tmp/r2_xml_fix`
2. Copy the original zip as backup: `cp <converted.zip> /tmp/r2_xml_fix/original_backup.zip`
3. Extract converted XML: `unzip -o <converted.zip> -d /tmp/r2_xml_fix/working/`
4. Extract ISBN from filenames
5. If source XHTML provided, extract to `/tmp/r2_xml_fix/source/` for reference
6. Count initial validation errors:
   ```bash
   # Use xmllint or the project's validation tools
   python3 -c "
   from validate_with_entity_tracking import EntityTrackingValidator
   validator = EntityTrackingValidator()
   errors = validator.validate_zip_package('/tmp/r2_xml_fix/original_backup.zip')
   print(f'Initial error count: {len(errors)}')
   "
   ```

### Step 2: Categorize & Prioritize Fixes

Parse the diagnosis report and categorize fixes by type. Apply in this order (structural fixes first, cosmetic last):

**Priority 1: Structural Fixes** (may affect other fixes)
1. Add missing admonition/callout wrappers (`<tip>`, `<note>`, `<sidebar>`)
2. Fix invalid element nesting
3. Add missing required elements (titles, etc.)

**Priority 2: Content Fixes** (text-level corrections)
4. Fix missing whitespace (heading numbers, metadata concatenation)
5. Fix double periods in references
6. Fix zero-width space artifacts (split compound words)
7. Fix encoding artifacts (characters)

**Priority 3: Reference Fixes** (links and cross-references)
8. Fix broken URLs
9. Restore lost link labels (PubMed, CrossRef)

**Priority 4: Numbering Fixes** (table/figure numbering)
10. Fix table numbering (chapter-prefix, remove informal table numbers)

### Step 3: Apply Fixes

For each fix category, use Python with lxml for XML manipulation. Here are the fix patterns:

#### Fix: Missing Heading Spaces (C-001)
```python
from lxml import etree
import glob

for xml_file in glob.glob('/tmp/r2_xml_fix/working/**/*.xml', recursive=True):
    parser = etree.XMLParser(remove_blank_text=False)
    tree = etree.parse(xml_file, parser)

    # Find emphasis elements with HeadingNumber role
    for elem in tree.xpath("//emphasis[@role='HeadingNumber']"):
        if elem.text and not elem.text.endswith(' '):
            elem.text = elem.text + ' '
        # Also check elem.tail — if next text is directly concatenated
        if elem.tail and elem.tail[0:1].isalpha():
            # Space already added to elem.text, but check tail too
            pass

    tree.write(xml_file, xml_declaration=True, encoding='UTF-8')
```

#### Fix: Copyright/Metadata Concatenation (C-002)
```python
# Find para elements with ChapterCopyright role
for elem in tree.xpath("//para[@role='ChapterCopyright']"):
    text = ''.join(elem.itertext())
    # Look for common concatenation patterns:
    # Year immediately followed by uppercase letter: "2025M." -> "2025 M."
    import re
    # Fix year+name: "2025M. Salih" -> "2025 M. Salih"
    # Fix ISBN concatenation: digits immediately followed by digits
    # Insert spaces between known metadata boundaries
```

#### Fix: Double Periods in References (C-006)
```python
for elem in tree.xpath("//bibliomixed"):
    for text_node in elem.xpath(".//text()"):
        if '..' in text_node:
            parent = text_node.getparent()
            if text_node == parent.text:
                parent.text = parent.text.replace('..', '.')
            else:
                parent.tail = parent.tail.replace('..', '.')
```

#### Fix: Zero-Width Space Artifacts (C-005)
```python
# Global text replacement across all XML files
replacements = {
    'Stat Pearls': 'StatPearls',
    'doi. org': 'doi.org',
    # Add other known split compounds as discovered
}
for xml_file in glob.glob('/tmp/r2_xml_fix/working/**/*.xml', recursive=True):
    with open(xml_file, 'r', encoding='utf-8') as f:
        content = f.read()
    for old, new in replacements.items():
        content = content.replace(old, new)
    with open(xml_file, 'w', encoding='utf-8') as f:
        f.write(content)
```

#### Fix: Encoding Artifacts (C-007)
```python
# Replace double-encoded NBSP: "Â " -> " " (single NBSP or space)
import re
for xml_file in glob.glob('/tmp/r2_xml_fix/working/**/*.xml', recursive=True):
    with open(xml_file, 'rb') as f:
        content = f.read()
    # C3 82 C2 A0 is double-encoded NBSP -> replace with C2 A0 (single NBSP)
    content = content.replace(b'\xc3\x82\xc2\xa0', b'\xc2\xa0')
    # Or just Â followed by space
    content = content.replace('Â '.encode('utf-8'), ' '.encode('utf-8'))
    with open(xml_file, 'wb') as f:
        f.write(content)
```

#### Fix: Callout Box Styling (C-003)
```python
# This is a structural fix — wrap bare callout text in proper admonition elements
# Pattern: <para>Hospitalist Tip</para> followed by content
# Target: <tip><title>Hospitalist Tip</title><para>content</para></tip>

callout_titles = {
    'Hospitalist Tip': 'tip',
    'Pearl': 'note',
    'Important': 'important',
    'Note': 'note',
    'Caution': 'caution',
    'Warning': 'warning',
}

for elem in tree.xpath("//para"):
    text = (elem.text or '').strip()
    if text in callout_titles:
        docbook_element = callout_titles[text]
        # Create admonition wrapper
        # Move following content into it
        # Validate the new structure is DTD-compliant
```

#### Fix: Broken URLs (C-009)
```python
for elem in tree.xpath("//ulink[@url]"):
    url = elem.get('url', '')
    if url.startswith('https://.') or url.startswith('http://.'):
        # Log and either fix or remove the broken URL
        print(f"Broken URL found: {url} in {xml_file}")
        # Option 1: Remove the ulink wrapper, keep text
        # Option 2: Fix the URL if the correct one can be determined from source
```

### Step 4: Validate Fixed XML

After ALL fixes are applied:

1. **Run DTD validation**:
   ```bash
   python3 -c "
   from validate_with_entity_tracking import EntityTrackingValidator
   validator = EntityTrackingValidator()
   errors_before = validator.validate_zip_package('/tmp/r2_xml_fix/original_backup.zip')
   errors_after = validator.validate_zip_package('/tmp/r2_xml_fix/working_repackaged.zip')
   print(f'Errors before: {len(errors_before)}')
   print(f'Errors after: {len(errors_after)}')
   print(f'Delta: {len(errors_after) - len(errors_before)}')
   "
   ```

2. **If new errors introduced**: Run the project's comprehensive fixer:
   ```bash
   python3 -c "
   from comprehensive_dtd_fixer import process_zip_package
   process_zip_package('/tmp/r2_xml_fix/working_repackaged.zip', '/tmp/r2_xml_fix/final.zip')
   "
   ```

3. **Verify specific fixes worked**: For each issue fixed, grep to confirm the pattern is resolved:
   ```bash
   # Verify heading spaces fixed
   grep -r '</emphasis>[A-Z]' /tmp/r2_xml_fix/working/ --include="*.xml" -c
   # Should be 0

   # Verify double periods fixed
   grep -r '\.\.' /tmp/r2_xml_fix/working/ --include="*.xml" -c
   # Should be significantly reduced
   ```

### Step 5: Re-package & Deliver

1. Re-zip the fixed files:
   ```bash
   cd /tmp/r2_xml_fix/working && zip -r /tmp/r2_xml_fix/fixed_output.zip . -x ".*"
   ```
2. Copy to the user's desired location
3. Generate fix summary report

### Step 6: Generate Fix Report

```markdown
## XML Fix Report: [Book Title]
**ISBN:** [ISBN]
**Date:** [Date]
**Input:** [original zip path]
**Output:** [fixed zip path]

### Fixes Applied

| # | Issue | Known ID | Files Modified | Instances Fixed |
|---|-------|----------|---------------|----------------|
| 1 | Missing heading spaces | C-001 | 45 | 127 |
| 2 | Double periods | C-006 | 14 | 195 |
| ... | ... | ... | ... | ... |

### Validation Results
- Errors before: N
- Errors after: N
- New errors introduced: N (all resolved by DTD fixer)

### Verification
[For each fix, show a before/after example]
```

## Codebase References

- `comprehensive_dtd_fixer.py`: `ComprehensiveDTDFixer`, `process_zip_package()`, `fix_chapter_file()` — 40+ fix strategies
- `validate_with_entity_tracking.py`: `EntityTrackingValidator.validate_zip_package()` — DTD validation with accurate file tracking
- `validation_report.py`: `ValidationReportGenerator` — Excel report generation
- `docbook_builder.py`: `is_valid_child()`, `get_valid_children()` — content model validation
- `id_authority.py`: `get_authority()` — ID management for element additions
