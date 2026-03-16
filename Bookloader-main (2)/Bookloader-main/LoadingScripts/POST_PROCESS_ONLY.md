# Post-Processing Only Guide

## Overview

The **Post-Processing Only** scripts run ONLY the R2Utilities license creation and database update step, WITHOUT re-processing the original books. This is useful when:

- You need to re-license books that were already processed
- You want to fix licenses without full reprocessing
- Database configuration changes need to be applied to existing books
- You failed post-processing but the Java processing succeeded

## Available Scripts

### PowerShell Version (Recommended)

**File**: `PostProcessOnly.ps1`

#### Single ISBN
```powershell
cd C:\RittenhouseRepos\Bookloader\LoadingScripts

# Single ISBN
.\PostProcessOnly.ps1 -ISBNList "9781234567890"
```

#### Multiple ISBNs (Command Line)
```powershell
.\PostProcessOnly.ps1 -ISBNList "9781234567890", "9789876543210", "9780000000000"
```

#### Multiple ISBNs (From File)
```powershell
# Create a text file with one ISBN per line
.\PostProcessOnly.ps1 -ISBNList "C:\path\to\isbnlist.txt"

# Or use relative path
.\PostProcessOnly.ps1 -ISBNList "./example_isbnlist.txt"

# Or absolute path to example file
.\PostProcessOnly.ps1 -ISBNList "C:\RittenhouseRepos\Bookloader\LoadingScripts\example_isbnlist.txt"
```

#### Custom R2Utilities Path
```powershell
# If R2Utilities is in a different location
.\PostProcessOnly.ps1 `
    -ISBNList "9781234567890" `
    -R2UtilitiesPath "C:\CustomPath\R2Utilities.exe"
```

#### With All Options
```powershell
.\PostProcessOnly.ps1 `
    -ISBNList "C:\RittenhouseRepos\Bookloader\LoadingScripts\example_isbnlist.txt" `
    -R2UtilitiesPath "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Debug\net481\R2Utilities.exe" `
    -IncludeChapterNumbers $true `
    -LogOutput "C:\RittenhouseRepos\Bookloader\logs"
```

### Batch Version

**File**: `PostProcessOnly.bat`

#### Single ISBN
```batch
cd C:\RittenhouseRepos\Bookloader\LoadingScripts
PostProcessOnly.bat 9781234567890
```

#### Multiple ISBNs
```batch
PostProcessOnly.bat 9781234567890 9789876543210 9780000000000
```

#### From File (Using @ Prefix)
```batch
REM Create isbnlist.txt with one ISBN per line, then:
PostProcessOnly.bat @isbnlist.txt

REM Or with full path
PostProcessOnly.bat @"C:\RittenhouseRepos\Bookloader\LoadingScripts\example_isbnlist.txt"
```

## Output

Both scripts create:
1. **Console Output** - Real-time processing status
2. **Log File** - Detailed log in `logs/PostProcess_YYYYMMDD_HHMMSS.log`

### Example Output
```
================================================================================
              R2Utilities Post-Processing Only
================================================================================

[1/3] Processing ISBN: 9781234567890
  Running: R2Utilities.exe -BookLoaderPostProcessingTask -isbn=9781234567890 -includeChapterNumbersInToc=true
  ✓ SUCCESS: ISBN 9781234567890 processed

[2/3] Processing ISBN: 9789876543210
  Running: R2Utilities.exe -BookLoaderPostProcessingTask -isbn=9789876543210 -includeChapterNumbersInToc=true
  ✓ SUCCESS: ISBN 9789876543210 processed

[3/3] Processing ISBN: 9780000000000
  Running: R2Utilities.exe -BookLoaderPostProcessingTask -isbn=9780000000000 -includeChapterNumbersInToc=true
  ✓ SUCCESS: ISBN 9780000000000 processed

================================================================================
                            SUMMARY
================================================================================
Total ISBNs:    3
Successful:     3
Failed:         0
Log File:       .\logs\PostProcess_20260219_143022.log
```

## What Gets Done

When you run post-processing, R2Utilities performs:

✅ **Resource Data Updates**
  - Normalizes ISBN format (13-digit)
  - Calculates sort title for A-Z index
  - Updates alpha keys for sorting

✅ **Database Updates**
  - Sets resource status to Active (makes it visible in R2)
  - Assigns specialty codes
  - Assigns practice area codes

✅ **License Creation**
  - Creates institutional licenses (from `AutoLicensesNumberOfLicenses` config)
  - Inserts records into `tResourceLicense` table
  - Makes book accessible to licensed users

✅ **XML Transformation**
  - Copies processed XML to R2 content directory (`C:\R2Content\<ISBN>\`)
  - Transforms content structure if needed

✅ **Index Population**
  - Populates A-to-Z index terms
  - Generates table of contents (if enabled with `-includeChapterNumbersInToc=true`)

## Prerequisites

1. **R2Utilities Built and Available**
   ```powershell
   cd C:\RittenhouseRepos\R2Library
   msbuild R2Utilities\R2Utilities.csproj /p:Configuration=Debug
   ```

2. **Database Connection Configured**
   - R2Utilities.exe.config must have valid database connection string
   - See [POST_PROCESSING_SETUP.md](../Documentation/POST_PROCESSING_SETUP.md)

3. **Books Already Processed**
   - Books must have been through Java BookLoader first
   - XML should exist in the expected location

## Troubleshooting

### Error: R2Utilities not found

**PowerShell:**
```powershell
# Verify path exists
Test-Path "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Debug\net481\R2Utilities.exe"

# Or use custom path
.\PostProcessOnly.ps1 -ISBNList "9781234567890" -R2UtilitiesPath "C:\path\to\R2Utilities.exe"
```

**Batch:**
- Edit `PostProcessOnly.bat` line 20:
  ```batch
  set "R2UTILITIES_EXE=<your path here>"
  ```

### Error: Invalid ISBN

- Ensure ISBNs are in the format: `9781234567890` (13-digit with hyphens optional)
- Check that book was processed and XML exists
- Verify `AutoLicensesNumberOfLicenses` is configured in R2Utilities.exe.config

### Post-processing succeeded but no licenses appear

1. **Check database connection in `R2Utilities.exe.config`**
   ```xml
   <add key="DatabaseConnectionString" value="..." />
   ```

2. **Verify `AutoLicensesNumberOfLicenses` setting**
   ```xml
   <add key="AutoLicensesNumberOfLicenses" value="10" />
   ```

3. **Check database manually**
   ```sql
   SELECT * FROM tResourceLicense WHERE iResourceId = (
       SELECT intResourceID FROM tResource WHERE vchisbn13 = '9781234567890'
   )
   ```

## Integration with Batch Processing

The `BatchProcessBooks.bat` script automatically runs post-processing after each book. To force re-licensing of already-processed books, use these post-processing-only scripts.

## Example Workflow

**Scenario**: You processed 20 books but licenses weren't created due to database issue. Now you've fixed the config.

```powershell
# Create ISBN list of books to re-license
$isbns = @(
    "9781234567890",
    "9789876543210", 
    "9780000000000"
)

# Export to file
$isbns | Out-File "C:\RittenhouseRepos\Bookloader\LoadingScripts\to_license.txt"

# Run post-processing
cd C:\RittenhouseRepos\Bookloader\LoadingScripts
.\PostProcessOnly.ps1 -ISBNList "./to_license.txt"

# Verify licenses were created
```

Or with batch files:
```batch
REM Create to_license.txt with ISBNs, then:
cd C:\RittenhouseRepos\Bookloader\LoadingScripts
PostProcessOnly.bat @to_license.txt
```
