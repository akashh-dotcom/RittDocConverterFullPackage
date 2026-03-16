<# 
BatchProcessBooks.ps1
- Processes ZIPs and ISBN folders in test\input
- Runs Java BookLoader, then R2Utilities BookLoaderPostProcessingTask
- Strong logging (console + batch log + per-book logs)
- Outputs to:
    test\batchoutput\Batch_<timestamp>_<mode>\PASS\<ISBN>\...
    test\batchoutput\Batch_<timestamp>_<mode>\FAIL\<ISBN>\...
- Creates final zip archive of the batch run folder

Usage examples:
  .\BatchProcessBooks.ps1
  .\BatchProcessBooks.ps1 -Mode Normal_NoPMID
  .\BatchProcessBooks.ps1 -Mode Normal_Update
  .\BatchProcessBooks.ps1 -Mode Ultra_Fast
  .\BatchProcessBooks.ps1 -PauseAtEnd

Notes:
- Designed to replace the old .bat ergonomics but with ProcessIncomingBooks_Local_Stripped-level logging.
#>

[CmdletBinding()]
param(
    [ValidateSet('Normal_NoPMID','Normal_Update','Ultra_Fast')]
    [string]$Mode = 'Normal_NoPMID',

    [int]$HeartbeatSeconds = 15,
    [int]$TimeoutMinutes = 240,

    # R2Utilities BookLoaderPostProcessingTask
    [string]$R2UtilitiesExe = 'C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Debug\net481\R2Utilities.exe',
    [switch]$EnablePostProcess,
    [switch]$DisablePostProcess,

    # If you want to capture more artifacts per ISBN:
    [switch]$CopyMediaFolder,
    [switch]$CopyTempFolder,

    [switch]$PauseAtEnd
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --------------------------
# Path Resolution
# --------------------------
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path     # ...\test
$TestDir   = $ScriptDir
$BaseDir   = Split-Path -Parent $TestDir                         # project root

$InputDir  = Join-Path $TestDir 'input'
$BatchOutRoot = Join-Path $TestDir 'batchoutput'

$WorkTempDir = Join-Path $TestDir '.batch_temp'

$LogsDirForBackend = Join-Path $TestDir 'logs'   # where RISBackend.log tends to live (per your bat)

# Work dirs your bat cleans each run
$WorkDirsToReset = @(
    (Join-Path $TestDir 'temp'),
    (Join-Path $TestDir 'output'),
    (Join-Path $TestDir 'media')
    # (Join-Path $TestDir 'R2v2-XMLbyISBN')
)

# --------------------------
# Mode -> BookLoader flags
# --------------------------
$ModeAllowsPostProcess = $true
switch ($Mode) {
    'Normal_NoPMID' { $BookLoaderFlags = @('--normal','--skipPMID'); $ModeName = 'Normal_NoPMID'; $ModeAllowsPostProcess = $true }
    'Normal_Update' { $BookLoaderFlags = @('--update','--normal');   $ModeName = 'Normal_Update'; $ModeAllowsPostProcess = $true }
    'Ultra_Fast'    { $BookLoaderFlags = @('--noDB','--skipPMID','--skipLinks'); $ModeName = 'Ultra_Fast'; $ModeAllowsPostProcess = $false }
    'DB_Update_NoPMID_NoLinks' { $BookLoaderFlags = @('--update','--skipPMID','--skipLinks'); $ModeName = 'DB_Update_NoPMID_NoLinks'; $ModeAllowsPostProcess = $true }
    default         { throw "Unexpected mode: $Mode" }
}

# --------------------------
# Timestamp + Output folders
# --------------------------
$Timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$BatchRunDir = Join-Path $BatchOutRoot ("Batch_{0}_{1}" -f $Timestamp, $ModeName)

$PassRoot = Join-Path $BatchRunDir 'PASS'
$FailRoot = Join-Path $BatchRunDir 'FAIL'

$BatchLog = Join-Path $BatchRunDir 'BATCH.log'

# --------------------------
# Logging
# --------------------------
function Write-Log {
    param(
        [Parameter(Mandatory)][string]$Message,
        [ValidateSet('INFO','WARN','ERROR','SUCCESS')][string]$Level = 'INFO',
        [string]$LogFile = $BatchLog,
        [string]$Prefix = ''
    )

    $ts = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $line = "[{0}] {1} {2}{3}" -f $ts, $Level.PadRight(7), ($(if($Prefix){ "[$Prefix] " } else { "" })), $Message

    # Console color
    $color =
        switch ($Level) {
            'ERROR'   { 'Red' }
            'WARN'    { 'Yellow' }
            'SUCCESS' { 'Green' }
            default   { 'White' }
        }

    Write-Host $line -ForegroundColor $color
    Add-Content -Path $LogFile -Value $line
}

function Ensure-Dir([string]$Path) {
    if (-not (Test-Path $Path)) { New-Item -ItemType Directory -Path $Path -Force | Out-Null }
}

# Streams stdout/stderr live to console AND file
function Invoke-LoggedProcess {
    param(
        [Parameter(Mandatory)][string]$FilePath,
        [Parameter(Mandatory)][string[]]$ArgumentList,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$LogFile,
        [Parameter(Mandatory)][string]$Prefix,
        [int]$TimeoutMinutes = 240,
        [int]$HeartbeatSeconds = 15
    )

    Ensure-Dir (Split-Path -Parent $LogFile)

    Add-Content -Path $LogFile -Value ("`r`n==============================")
    Add-Content -Path $LogFile -Value ("START {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
    Add-Content -Path $LogFile -Value ("CMD: {0} {1}" -f $FilePath, ($ArgumentList -join ' '))
    Add-Content -Path $LogFile -Value ("WD : {0}" -f $WorkingDirectory)
    Add-Content -Path $LogFile -Value ("==============================")

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $FilePath
    $psi.WorkingDirectory = $WorkingDirectory
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.CreateNoWindow = $true
    $psi.Arguments = ($ArgumentList | ForEach-Object {
        # quote args that have spaces
        if ($_ -match '\s') { '"' + ($_ -replace '"','\"') + '"' } else { $_ }
    }) -join ' '

    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $psi
    $p.EnableRaisingEvents = $true

    $outHandler = [System.Diagnostics.DataReceivedEventHandler]{
        param($sender, $e)
        if ($null -ne $e.Data) {
            $line = $e.Data
            Write-Host ("[{0}] {1}" -f $Prefix, $line)
            Add-Content -Path $LogFile -Value $line
        }
    }
    $errHandler = [System.Diagnostics.DataReceivedEventHandler]{
        param($sender, $e)
        if ($null -ne $e.Data) {
            $line = $e.Data
            Write-Host ("[{0}][stderr] {1}" -f $Prefix, $line) -ForegroundColor Yellow
            Add-Content -Path $LogFile -Value ("[stderr] " + $line)
        }
    }

    $null = $p.add_OutputDataReceived($outHandler)
    $null = $p.add_ErrorDataReceived($errHandler)

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $timeout = [TimeSpan]::FromMinutes($TimeoutMinutes)
    $lastBeat = [DateTime]::UtcNow

    if (-not $p.Start()) { throw "Failed to start process: $FilePath" }
    $p.BeginOutputReadLine()
    $p.BeginErrorReadLine()

    while (-not $p.HasExited) {
        if ($sw.Elapsed -gt $timeout) {
            Write-Log -Level 'ERROR' -Prefix $Prefix -Message "TIMEOUT after $TimeoutMinutes minutes. Killing process tree..."
            try { & taskkill.exe /PID $p.Id /T /F | Out-Null } catch {}
            return 9999
        }

        if ((([DateTime]::UtcNow - $lastBeat).TotalSeconds) -ge $HeartbeatSeconds) {
            Write-Log -Level 'INFO' -Prefix $Prefix -Message ("Heartbeat... elapsed {0:hh\:mm\:ss}" -f $sw.Elapsed) -LogFile $LogFile
            $lastBeat = [DateTime]::UtcNow
        }

        Start-Sleep -Seconds 2
    }

    try { $p.WaitForExit() } catch {}
    $code = $p.ExitCode

    Add-Content -Path $LogFile -Value ("`r`nEND {0}  ExitCode={1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $code)
    Add-Content -Path $LogFile -Value ("==============================`r`n")

    return $code
}

# --------------------------
# Java / BookLoader resolution (mirrors your .bat)
# --------------------------
function Resolve-JavaExe {
    if ($env:JAVA_HOME) {
        $candidate = Join-Path $env:JAVA_HOME 'bin\java.exe'
        if (Test-Path $candidate) { return $candidate }
    }

    $fallback = 'C:\Program Files\Eclipse Adoptium\jdk-25.0.0.36-hotspot\bin\java.exe'
    if (Test-Path $fallback) { return $fallback }

    throw "Java not found. Set JAVA_HOME or update fallback path. Tried: JAVA_HOME\bin\java.exe and $fallback"
}

function Build-ClassPath {
    $classes = Join-Path $BaseDir 'build\classes'
    if (-not (Test-Path $classes)) {
        throw "Build\classes not found: $classes (compile Java project first)"
    }

    $cp = @($classes)

    $jarGlobs = @(
        (Join-Path $BaseDir 'lib\*.jar'),
        (Join-Path $BaseDir 'lib\jakarta\*.jar'),
        (Join-Path $BaseDir 'lib\jdbc\*.jar'),
        (Join-Path $BaseDir 'lib\saxon\*.jar'),
        (Join-Path $BaseDir 'lib\textml\*.jar'),
        (Join-Path $BaseDir 'lib\xalan\*.jar'),
        (Join-Path $BaseDir 'lib\xerces\*.jar')
    )

    foreach ($g in $jarGlobs) {
        Get-ChildItem -Path $g -ErrorAction SilentlyContinue | ForEach-Object { $cp += $_.FullName }
    }

    return ($cp -join ';')
}

# --------------------------
# Helpers
# --------------------------
function Remove-DirectoryPreservingDtd {
    param(
        [Parameter(Mandatory)][string]$Path
    )

    if (-not (Test-Path $Path)) { return }

    Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.PSIsContainer) {
            if ($_.Name -ieq 'dtd') {
                return
            }

            try {
                Remove-DirectoryPreservingDtd -Path $_.FullName
                $remaining = Get-ChildItem -LiteralPath $_.FullName -Force -ErrorAction SilentlyContinue
                if (-not $remaining) {
                    Remove-Item -LiteralPath $_.FullName -Force -ErrorAction SilentlyContinue
                }
            } catch {}
        }
        else {
            try { Remove-Item -LiteralPath $_.FullName -Force -ErrorAction SilentlyContinue } catch {}
        }
    }
}

function Reset-WorkDirs {
    foreach ($d in $WorkDirsToReset) {
        Ensure-Dir $d
        try { Remove-DirectoryPreservingDtd -Path $d } catch {}
        Ensure-Dir $d
    }
}

function Get-BooksToProcess {
    Ensure-Dir $InputDir

    $zips = Get-ChildItem -Path $InputDir -Filter *.zip -File -ErrorAction SilentlyContinue
    $folders = Get-ChildItem -Path $InputDir -Directory -ErrorAction SilentlyContinue |
               Where-Object { $_.Name -ne '.batch_temp' }

    $items = @()

    foreach ($z in $zips) {
        $isbn = [IO.Path]::GetFileNameWithoutExtension($z.Name)
        $items += [pscustomobject]@{ Type='ZIP'; ISBN=$isbn; Path=$z.FullName; Name=$z.Name }
    }

    foreach ($f in $folders) {
        # If folder is ISBN, thatâ€™s the ISBN; otherwise still process (some folks name with suffixes)
        $isbn = $f.Name
        $items += [pscustomobject]@{ Type='FOLDER'; ISBN=$isbn; Path=$f.FullName; Name=$f.Name }
    }

    return $items
}

function Isolate-CurrentBook {
    param(
        [Parameter(Mandatory)]$Current
    )

    Ensure-Dir $WorkTempDir

    # Move everything except current into .batch_temp
    Get-ChildItem -Path $InputDir -File -Filter *.zip -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.FullName -ne $Current.Path) {
            Move-Item -LiteralPath $_.FullName -Destination $WorkTempDir -Force
        }
    }

    Get-ChildItem -Path $InputDir -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.FullName -ne $Current.Path -and $_.Name -ne '.batch_temp') {
            Move-Item -LiteralPath $_.FullName -Destination $WorkTempDir -Force
        }
    }
}

function Restore-OtherBooks {
    if (Test-Path $WorkTempDir) {
        Get-ChildItem -Path $WorkTempDir -ErrorAction SilentlyContinue | ForEach-Object {
            Move-Item -LiteralPath $_.FullName -Destination $InputDir -Force
        }
    }
}

function Collect-XmlOutputs {
    param(
        [Parameter(Mandatory)][string]$Isbn,
        [Parameter(Mandatory)][string]$DestXmlDir
    )
    Ensure-Dir $DestXmlDir

    $sources = @(
        (Join-Path $TestDir 'output\*.xml'),
        (Join-Path $TestDir 'temp\*.xml')
        # (Join-Path $TestDir ("R2v2-XMLbyISBN\{0}\xml\*.xml" -f $Isbn))
    )

    foreach ($s in $sources) {
        Get-ChildItem -Path $s -ErrorAction SilentlyContinue | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $DestXmlDir -Force
        }
    }
}

# --------------------------
# Start
# --------------------------
Ensure-Dir $BatchRunDir
Ensure-Dir $PassRoot
Ensure-Dir $FailRoot
Ensure-Dir $BatchOutRoot

Write-Log "============================================================="
Write-Log "BATCH BOOK PROCESSING START"
Write-Log "Mode: $ModeName"
Write-Log "Input: $InputDir"
Write-Log "Output: $BatchRunDir"
$postProcessState = if ($DisablePostProcess) { 'DISABLED (override)' } elseif ($EnablePostProcess) { 'ENABLED (explicit)' } else { 'DISABLED (default)' }
Write-Log "PostProcess: $postProcessState  (R2Utilities: $R2UtilitiesExe)"
Write-Log "============================================================="

$books = Get-BooksToProcess
if ($books.Count -eq 0) {
    Write-Log -Level 'WARN' "No ZIPs or folders found in $InputDir"
    if ($PauseAtEnd) { Read-Host "Press Enter to exit" | Out-Null }
    return
}

Write-Log "Found $($books.Count) item(s) to process: $(@($books | Group-Object Type | ForEach-Object { "$($_.Name)=$($_.Count)" }) -join ', ')"

# Resolve tools once
$JavaExe = Resolve-JavaExe
$ClassPath = Build-ClassPath

Write-Log "Java: $JavaExe"
Write-Log "Classpath built."

$stats = [ordered]@{
    Total = $books.Count
    Success = 0
    Failed = 0
    PostProcessFailed = 0
}

# --------------------------
# Main Loop
# --------------------------
$idx = 0
foreach ($b in $books) {
    $idx++
    $isbn = $b.ISBN

    Write-Log "============================================================="
    Write-Log "[$idx/$($books.Count)] Processing $isbn ($($b.Type))"
    Write-Log "============================================================="

    # Where this ISBNâ€™s artifacts will land (PASS or FAIL later)
    $bookTempOutDir = Join-Path $BatchRunDir ("_WORK_{0}" -f $isbn)
    if (Test-Path $bookTempOutDir) { try { Remove-DirectoryPreservingDtd -Path $bookTempOutDir } catch {} }
    Ensure-Dir $bookTempOutDir

    $bookLoaderLog = Join-Path $bookTempOutDir "BookLoader.log"
    $postLog       = Join-Path $bookTempOutDir "PostProcess.log"
    $combinedLog   = Join-Path $bookTempOutDir "Combined.log"

    # Start combined log header
    Set-Content -Path $combinedLog -Value ("=== ISBN {0}  ({1}) ===`r`nStarted: {2}`r`nMode: {3}`r`nFlags: {4}`r`n" -f $isbn, $b.Type, (Get-Date), $ModeName, ($BookLoaderFlags -join ' '))

    try {
        # Isolate current (keeps old behavior: only one book in input at a time)
        Isolate-CurrentBook -Current $b

        # Reset work dirs
        Reset-WorkDirs

        # ---- Run BookLoader ----
        Write-Log -Prefix $isbn "Running BookLoader..." -LogFile $combinedLog

        $javaArgs = @(
            '-Xms1g','-Xmx2g',
            '--enable-native-access=ALL-UNNAMED',
            '-Djdk.xml.entityExpansionLimit=10000',
            '-Djdk.xml.totalEntitySizeLimit=1000000',
            '-Djava.security.policy=java.ris.policy',
            '-cp', $ClassPath,
            'com.rittenhouse.RIS.Main'
        ) + $BookLoaderFlags

        $code = Invoke-LoggedProcess `
            -FilePath $JavaExe `
            -ArgumentList $javaArgs `
            -WorkingDirectory $BaseDir `
            -LogFile $bookLoaderLog `
            -Prefix "$isbn|BookLoader" `
            -TimeoutMinutes $TimeoutMinutes `
            -HeartbeatSeconds $HeartbeatSeconds

        Add-Content -Path $combinedLog -Value ("`r`n--- BookLoader ExitCode: $code ---`r`n")
        Write-Log -Prefix $isbn -LogFile $combinedLog "BookLoader exit code: $code"

        $postCode = 0
        if ($code -eq 0 -and $ModeAllowsPostProcess -and $EnablePostProcess -and -not $DisablePostProcess) {
            # ---- Run PostProcess ----
            if (Test-Path $R2UtilitiesExe) {
                Write-Log -Prefix $isbn -LogFile $combinedLog "Running BookLoaderPostProcessingTask..."
                $ppArgs = @(
                    '-BookLoaderPostProcessingTask',
                    ("-isbn={0}" -f $isbn),
                    '-includeChapterNumbersInToc=true'
                )

                $postCode = Invoke-LoggedProcess `
                    -FilePath $R2UtilitiesExe `
                    -ArgumentList $ppArgs `
                    -WorkingDirectory $BaseDir `
                    -LogFile $postLog `
                    -Prefix "$isbn|PostProcess" `
                    -TimeoutMinutes 60 `
                    -HeartbeatSeconds 10

                Add-Content -Path $combinedLog -Value ("`r`n--- PostProcess ExitCode: $postCode ---`r`n")
                Write-Log -Prefix $isbn -LogFile $combinedLog "PostProcess exit code: $postCode"
            }
            else {
                $postCode = 7777
                Write-Log -Level 'WARN' -Prefix $isbn -LogFile $combinedLog "R2Utilities not found at: $R2UtilitiesExe (post-process skipped)"
            }
        }
        elseif ($code -eq 0 -and $ModeAllowsPostProcess -and $EnablePostProcess -and $DisablePostProcess) {
            Write-Log -Level 'INFO' -Prefix $isbn -LogFile $combinedLog "Post-processing skipped: -DisablePostProcess override supplied"
        }
        elseif ($code -eq 0 -and $ModeAllowsPostProcess -and -not $EnablePostProcess) {
            Write-Log -Level 'INFO' -Prefix $isbn -LogFile $combinedLog "Post-processing skipped: disabled by default (use -EnablePostProcess to run)"
        }
        elseif ($code -eq 0 -and -not $ModeAllowsPostProcess) {
            Write-Log -Level 'INFO' -Prefix $isbn -LogFile $combinedLog "Post-processing skipped: Mode $ModeName does not use database"
        }

        # ---- Decide PASS/FAIL ----
        $isPass = ($code -eq 0)
        $destRoot = if ($isPass) { $PassRoot } else { $FailRoot }
        $destIsbnDir = Join-Path $destRoot $isbn

        Ensure-Dir $destIsbnDir
        Ensure-Dir (Join-Path $destIsbnDir 'logs')
        Ensure-Dir (Join-Path $destIsbnDir 'xml')

        # Copy logs into final location
        Copy-Item -LiteralPath $bookLoaderLog -Destination (Join-Path $destIsbnDir 'logs\BookLoader.log') -Force -ErrorAction SilentlyContinue
        Copy-Item -LiteralPath $postLog       -Destination (Join-Path $destIsbnDir 'logs\PostProcess.log') -Force -ErrorAction SilentlyContinue
        Copy-Item -LiteralPath $combinedLog   -Destination (Join-Path $destIsbnDir 'logs\Combined.log') -Force -ErrorAction SilentlyContinue

        # Collect XMLs
        Collect-XmlOutputs -Isbn $isbn -DestXmlDir (Join-Path $destIsbnDir 'xml')

        # Optionally snapshot temp/media (useful for debugging)
        if ($CopyMediaFolder -and (Test-Path (Join-Path $TestDir 'media'))) {
            Copy-Item -Path (Join-Path $TestDir 'media') -Destination (Join-Path $destIsbnDir 'media') -Recurse -Force -ErrorAction SilentlyContinue
        }
        if ($CopyTempFolder -and (Test-Path (Join-Path $TestDir 'temp'))) {
            Copy-Item -Path (Join-Path $TestDir 'temp') -Destination (Join-Path $destIsbnDir 'temp') -Recurse -Force -ErrorAction SilentlyContinue
        }

        if (-not $isPass) {
            # Write a tight error summary
            $errSummary = Join-Path $destIsbnDir 'ERROR_SUMMARY.txt'
            $tailBackend = ''
            $backendLog = Join-Path $LogsDirForBackend 'RISBackend.log'
            if (Test-Path $backendLog) {
                try { $tailBackend = (Get-Content $backendLog -Tail 120) -join "`r`n" } catch {}
            } else {
                $tailBackend = '(RISBackend.log not found)'
            }

            Set-Content -Path $errSummary -Value @"
ISBN: $isbn
Type: $($b.Type)
Mode: $ModeName
Flags: $($BookLoaderFlags -join ' ')
BookLoader ExitCode: $code
PostProcess ExitCode: $postCode
Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

---- RISBackend.log (tail 120) ----
$tailBackend
"@
        }

        # Stats + batch log
        if ($isPass) {
            $stats.Success++
            Write-Log -Level 'SUCCESS' "$isbn PASS (BookLoader=0; PostProcess=$postCode)"
            if ($postCode -ne 0 -and $ModeAllowsPostProcess -and $EnablePostProcess -and -not $DisablePostProcess -and (Test-Path $R2UtilitiesExe)) {
                $stats.PostProcessFailed++
                Write-Log -Level 'WARN' "$isbn PostProcess FAILED (exit=$postCode) but BookLoader passed"
            }
        } else {
            $stats.Failed++
            Write-Log -Level 'ERROR' "$isbn FAIL (BookLoader exit=$code)"
        }
    }
    catch {
        $stats.Failed++
        Write-Log -Level 'ERROR' -Prefix $isbn ("Unhandled exception: {0}" -f $_.Exception.Message)

        # best-effort: write exception to FAIL bucket
        $destIsbnDir = Join-Path $FailRoot $isbn
        Ensure-Dir $destIsbnDir
        Ensure-Dir (Join-Path $destIsbnDir 'logs')
        Set-Content -Path (Join-Path $destIsbnDir 'logs\Exception.txt') -Value ($_.ToString())
    }
    finally {
        # Always restore other books
        try { Restore-OtherBooks } catch {}
        # Clean working folder
        try { if (Test-Path $bookTempOutDir) { Remove-DirectoryPreservingDtd -Path $bookTempOutDir } } catch {}
        
        Write-Log "[$idx/$($books.Count)] Finished processing $isbn - Loop continuing..."
    }
}

# Cleanup temp isolate dir
try { if (Test-Path $WorkTempDir) { Remove-DirectoryPreservingDtd -Path $WorkTempDir } } catch {}

# --------------------------
# Summary + Final Zip
# --------------------------
Write-Log "============================================================="
Write-Log "BATCH COMPLETE"
Write-Log ("Total: {0}   PASS: {1}   FAIL: {2}   PostProcessFailed: {3}" -f $stats.Total, $stats.Success, $stats.Failed, $stats.PostProcessFailed)
Write-Log "Output: $BatchRunDir"
Write-Log "============================================================="

# Create final zip archive
$FinalZip = Join-Path $BatchOutRoot ("Batch_{0}_{1}.zip" -f $Timestamp, $ModeName)
try {
    if (Test-Path $FinalZip) { Remove-Item $FinalZip -Force -ErrorAction SilentlyContinue }
    Compress-Archive -Path (Join-Path $BatchRunDir '*') -DestinationPath $FinalZip -Force
    Write-Log -Level 'SUCCESS' "Final archive created: $FinalZip"
}
catch {
    Write-Log -Level 'WARN' "Could not create final archive: $($_.Exception.Message)"
}

if ($PauseAtEnd) {
    Read-Host "Press Enter to exit" | Out-Null
}
