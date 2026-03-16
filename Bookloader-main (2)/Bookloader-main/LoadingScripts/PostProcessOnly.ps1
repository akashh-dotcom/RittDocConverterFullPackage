#!/usr/bin/env powershell
<#
.SYNOPSIS
    Run R2Utilities post-processing on a list of ISBNs
    
.DESCRIPTION
    This script runs ONLY the post-processing step (license creation, database updates)
    without re-processing the original books. Useful for:
    - Re-licensing books that were already processed
    - Fixing licenses without full reprocessing
    - Batch-updating licenses after configuration changes
    
.PARAMETER ISBNList
    Array of ISBN strings, or path to a text file with one ISBN per line
    
.PARAMETER R2UtilitiesPath
    Path to R2Utilities.exe (default: R2Library repo location)
    
.PARAMETER IncludeChapterNumbers
    Include chapter numbers in TOC (default: true)
    
.PARAMETER LogOutput
    Directory to store processing logs (default: ./logs)
    
.EXAMPLE
    # Process single ISBN
    .\PostProcessOnly.ps1 -ISBNList "9781234567890"
    
    # Process multiple ISBNs
    .\PostProcessOnly.ps1 -ISBNList "9781234567890", "9789876543210"
    
    # Process from file
    .\PostProcessOnly.ps1 -ISBNList "C:\isbnlist.txt"
    
    # Specify R2Utilities location
    .\PostProcessOnly.ps1 -ISBNList "9781234567890" -R2UtilitiesPath "C:\path\to\R2Utilities.exe"
#>

param(
    [string[]]$ISBNList,
    
    [string]$R2UtilitiesPath = "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Debug\net481\R2Utilities.exe",
    
    [bool]$IncludeChapterNumbers = $true,
    
    [string]$LogOutput = ".\logs"
)

$ErrorActionPreference = "Stop"

# If no ISBN list provided, try to use isbnlist.txt in the same directory
if ([string]::IsNullOrWhiteSpace($ISBNList)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $defaultISBNFile = Join-Path $scriptDir "isbnlist.txt"
    
    if (Test-Path $defaultISBNFile) {
        $ISBNList = $defaultISBNFile
        Write-Host "Using ISBN list from: $ISBNList" -ForegroundColor Cyan
    } else {
        Write-Host "ERROR: No ISBNs provided and isbnlist.txt not found in script directory" -ForegroundColor Red
        Write-Host ""
        Write-Host "Usage:" -ForegroundColor Yellow
        Write-Host "  1. Create isbnlist.txt with one ISBN per line" -ForegroundColor Gray
        Write-Host "  2. Place it in the same directory as this script" -ForegroundColor Gray
        Write-Host "  3. Run: .\PostProcessOnly.ps1" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Or specify ISBNs directly:" -ForegroundColor Yellow
        Write-Host "  .\PostProcessOnly.ps1 -ISBNList '9781234567890','9789876543210'" -ForegroundColor Gray
        exit 1
    }
}

# ============================================================================
# CONFIGURATION
# ============================================================================

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logDir = if ([System.IO.Path]::IsPathRooted($LogOutput)) { $LogOutput } else { Join-Path (Get-Location) $LogOutput }
$logFile = Join-Path $logDir "PostProcess_$timestamp.log"

# Create log directory
if (!(Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

# ============================================================================
# FUNCTIONS
# ============================================================================

function Write-LogLine {
    param([string]$Message, [string]$Color = "White")
    
    $line = "[$(Get-Date -Format 'HH:mm:ss')] $Message"
    Write-Host $line -ForegroundColor $Color
    Add-Content -Path $logFile -Value $line
}

function Test-R2Utilities {
    if (!(Test-Path $R2UtilitiesPath)) {
        Write-LogLine "ERROR: R2Utilities not found at: $R2UtilitiesPath" -Color Red
        Write-LogLine "Please configure R2UtilitiesPath parameter or build R2Library" -Color Red
        Write-Host "`nExample:" -ForegroundColor Gray
        Write-Host "  .\PostProcessOnly.ps1 -ISBNList 'isbn1','isbn2' -R2UtilitiesPath 'C:\path\to\R2Utilities.exe'" -ForegroundColor Gray
        return $false
    }
    return $true
}

function Resolve-ISBNList {
    param($input)
    
    $isbns = @()
    
    # If it's a file path
    if ([System.IO.File]::Exists($input)) {
        Write-LogLine "Reading ISBNs from file: $input" -Color Cyan
        $isbns = @(Get-Content $input | 
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -and -not $_.StartsWith('#') } |
            ForEach-Object { $_.Trim() })
        Write-LogLine "Found $($isbns.Count) ISBN(s) in file" -Color Cyan
    }
    # If it's an array
    elseif ($input -is [array]) {
        $isbns = @($input | Where-Object { $_ })
    }
    # If it's a single string
    else {
        $isbns = @($input.ToString().Trim())
    }
    
    return $isbns
}

function Run-PostProcessing {
    param(
        [string]$ISBN,
        [int]$Index,
        [int]$Total
    )
    
    Write-LogLine ""
    Write-LogLine "[$Index/$Total] Processing ISBN: $ISBN" -Color Cyan
    
    $flags = ""
    if ($IncludeChapterNumbers) {
        $flags = "-includeChapterNumbersInToc=true"
    }
    
    try {
        # Run R2Utilities
        Write-LogLine "  Running: R2Utilities.exe -BookLoaderPostProcessingTask -isbn=$ISBN $flags" -Color Gray
        & $R2UtilitiesPath -BookLoaderPostProcessingTask -isbn=$ISBN $flags
        
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-LogLine "  ✓ SUCCESS: ISBN $ISBN processed" -Color Green
            return $true
        } else {
            Write-LogLine "  ✗ FAILED: ISBN $ISBN failed with exit code $exitCode" -Color Red
            return $false
        }
    }
    catch {
        Write-LogLine "  ✗ ERROR: $_" -Color Red
        return $false
    }
}

# ============================================================================
# MAIN
# ============================================================================

Write-LogLine "=======================================================================" -Color Cyan
Write-LogLine "R2Utilities Post-Processing (License/Database Updates Only)" -Color Cyan
Write-LogLine "=======================================================================" -Color Cyan
Write-LogLine ""

# Validate R2Utilities exists
if (!(Test-R2Utilities)) {
    exit 1
}

Write-LogLine "R2Utilities Path: $R2UtilitiesPath" -Color Gray
Write-LogLine "Include Chapter Numbers: $IncludeChapterNumbers" -Color Gray
Write-LogLine "Log File: $logFile" -Color Gray
Write-LogLine ""

# Parse ISBN list
$isbns = Resolve-ISBNList $ISBNList
$totalCount = $isbns.Count
$successCount = 0
$failureCount = 0

if ($totalCount -eq 0) {
    Write-LogLine "ERROR: No ISBNs provided" -Color Red
    exit 1
}

Write-LogLine "Processing $totalCount ISBN(s)..." -Color Cyan
Write-LogLine ""

# Process each ISBN
foreach ($isbn in $isbns) {
    $index = ([array]::IndexOf($isbns, $isbn)) + 1
    
    if (Run-PostProcessing $isbn $index $totalCount) {
        $successCount++
    } else {
        $failureCount++
    }
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-LogLine ""
Write-LogLine "=======================================================================" -Color Cyan
Write-LogLine "SUMMARY" -Color Cyan
Write-LogLine "=======================================================================" -Color Cyan
Write-LogLine "Total ISBNs:    $totalCount" -Color White
Write-LogLine "Successful:     $successCount" -Color Green
Write-LogLine "Failed:         $failureCount" -Color $(if ($failureCount -gt 0) { "Red" } else { "Green" })
Write-LogLine "Log File:       $logFile" -Color Gray
Write-LogLine ""

if ($failureCount -eq 0) {
    Write-LogLine "✓ All post-processing completed successfully" -Color Green
    exit 0
} else {
    Write-LogLine "⚠ Some post-processing operations failed" -Color Yellow
    exit 1
}
