# Deployment Guide: BookLoaderPostProcessingTask Enhanced Logging

## Document Information
- **Date Created**: January 2025
- **Modified File**: `R2Utilities\Tasks\ContentTasks\BookLoaderPostProcessingTask.cs`
- **Change Type**: Enhanced Logging
- **Target Environment**: Staging
- **Project**: R2Utilities

---

## Table of Contents
1. [Overview](#overview)
2. [Changes Summary](#changes-summary)
3. [Pre-Deployment Checklist](#pre-deployment-checklist)
4. [Deployment Steps](#deployment-steps)
5. [Post-Deployment Verification](#post-deployment-verification)
6. [Rollback Procedure](#rollback-procedure)
7. [Troubleshooting](#troubleshooting)

---

## Overview

### What Was Changed?
Enhanced logging throughout the `BookLoaderPostProcessingTask` class to provide comprehensive visibility into each phase of the book loading post-processing pipeline.

### Why This Change?
The BookLoader post-processing task is brittle and runs after the Java-based book loader. Previously, when failures occurred, it was difficult to:
- Determine which phase failed
- Understand why it failed
- Know if resources were updated
- Verify if licenses were added
- Track how far the pipeline progressed before stopping

### Benefits
- ✅ Clear phase-by-phase execution tracking
- ✅ Detailed error messages with full exception details
- ✅ Explicit logging when pipeline stops early
- ✅ Database operation result tracking
- ✅ File copy operation visibility
- ✅ License generation confirmation
- ✅ Execution timing for each phase

---

## Changes Summary

### Modified Files
1. **R2Utilities\Tasks\ContentTasks\BookLoaderPostProcessingTask.cs**
   - Enhanced `Run()` method with comprehensive phase logging
   - Enhanced `UpdateResourceData()` with detailed sub-operation logging
   - Enhanced `CopyContent()` with file operation tracking
   - Enhanced `TransformXmlContent()` with transformation result logging
   - Enhanced `CopyDirectory()` with detailed file copy tracking
   - Enhanced `UpdateResourcePracticeAreas()` with database operation logging
   - Enhanced `UpdateResourceSpecialties()` with database operation logging
   - Enhanced `UpdateAtoIndexTerms()` with detailed index update logging
   - Enhanced `UpdateTocXml()` with TOC update result logging

### Key Logging Additions

#### Phase Markers
- Clear start/end delimiters: `========== PHASE X: Description ==========`
- Success/failure status for each phase
- Duration tracking for each operation
- Explicit "pipeline stopped" messages when phases fail

#### Exception Handling
- Full exception type and message logging
- Stack trace capture
- Inner exception details
- Contextual information (ISBN, Resource ID, etc.)

#### Database Operations
- Row counts for all UPDATE/INSERT operations
- Warnings when operations return 0 rows
- Detailed A-to-Z index operation counts

#### File Operations
- Source and destination paths
- Directory existence checks
- File counts and sizes
- Individual file copy status

#### Licensing
- Number of institutions processed
- Licenses added per institution
- Warnings if no licenses were added
- Total license count

---

## Pre-Deployment Checklist

### 1. Code Review
- [ ] Review all changes in `BookLoaderPostProcessingTask.cs`
- [ ] Ensure no business logic was altered (only logging added)
- [ ] Verify C# 7.3 and .NET Framework 4.8.1 compatibility
- [ ] Check that all nullable TimeSpan operations handle `HasValue` properly

### 2. Local Testing
- [ ] Build solution successfully
- [ ] Run the task locally with a test ISBN
- [ ] Verify log output shows all new logging statements
- [ ] Test failure scenarios to ensure error logging works
- [ ] Verify existing functionality unchanged

### 3. Source Control
- [ ] Commit changes to Git with clear commit message
- [ ] Create Pull Request (if using PR workflow)
- [ ] Get code review approval (if required)
- [ ] Merge to main/master branch

### 4. Build Preparation
- [ ] Ensure clean working directory
- [ ] Update from latest main/master branch
- [ ] Verify no merge conflicts
- [ ] Ensure all dependencies are available

---

## Deployment Steps

### Step 1: Build the Solution

#### Option A: Visual Studio
1. Open Solution in Visual Studio 2019 or later
2. Select **Build Configuration**: `Release`
3. Select **Platform**: `Any CPU`
4. Right-click on **R2Utilities** project
5. Select **Build**
6. Verify build succeeds with no errors
7. Check Output window for build path (typically: `C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\`)

#### Option B: MSBuild Command Line
```powershell
# Navigate to solution directory
cd C:\RittenhouseRepos\R2Library\src

# Build R2Utilities project in Release mode
msbuild R2Utilities\R2Utilities.csproj /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild
```

#### Option C: Build Entire Solution
```powershell
# Build entire solution
msbuild R2Library.sln /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild
```

### Step 2: Identify Built Assemblies

After build, locate these files in the R2Utilities output directory:
- `R2Utilities.exe` (or .dll if library)
- `R2Utilities.pdb` (debug symbols - optional but recommended)
- Any dependent assemblies that changed

**Build Output Location**:
```
C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\
```

### Step 3: Prepare Staging Deployment Package

1. **Create Deployment Folder**:
   ```powershell
   # Create deployment package folder
   $deployDate = Get-Date -Format "yyyyMMdd_HHmmss"
   $deployFolder = "C:\Deploy\R2Utilities_EnhancedLogging_$deployDate"
   New-Item -Path $deployFolder -ItemType Directory
   ```

2. **Copy Required Files**:
   ```powershell
   # Copy main assemblies
   Copy-Item "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\R2Utilities.exe" -Destination $deployFolder
   Copy-Item "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\R2Utilities.pdb" -Destination $deployFolder
   
   # Copy any updated dependencies (if R2V2.dll or other libraries changed)
   Copy-Item "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\R2V2.dll" -Destination $deployFolder -ErrorAction SilentlyContinue
   Copy-Item "C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\R2Library.Data.ADO.dll" -Destination $deployFolder -ErrorAction SilentlyContinue
   ```

3. **Create Deployment Documentation**:
   ```powershell
   # Create deployment manifest
   @"
   Deployment Package: R2Utilities Enhanced Logging
   Date: $(Get-Date)
   Files:
   $(Get-ChildItem $deployFolder | Select-Object Name, Length, LastWriteTime | Out-String)
   "@ | Out-File "$deployFolder\DEPLOYMENT_MANIFEST.txt"
   ```

### Step 4: Access Staging Environment

**Staging Environment Details** (update with actual values):
- **Server**: `staging-server.yourdomain.com` or IP address
- **Application Path**: `C:\Applications\R2Utilities\` or similar
- **Service Name**: R2Utilities Windows Service (if applicable)
- **Access Method**: RDP, SSH, or file share

**Connect to Staging**:
```powershell
# Remote Desktop
mstsc /v:staging-server.yourdomain.com

# Or use PowerShell Remoting
Enter-PSSession -ComputerName staging-server.yourdomain.com -Credential (Get-Credential)
```

### Step 5: Backup Existing Staging Files

**CRITICAL**: Always backup before deployment!

```powershell
# On staging server
$backupFolder = "C:\Backups\R2Utilities_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$appFolder = "C:\Applications\R2Utilities"

# Create backup
New-Item -Path $backupFolder -ItemType Directory
Copy-Item "$appFolder\R2Utilities.exe" -Destination $backupFolder
Copy-Item "$appFolder\R2Utilities.pdb" -Destination $backupFolder -ErrorAction SilentlyContinue
Copy-Item "$appFolder\*.config" -Destination $backupFolder

# Document backup
@"
Backup Created: $(Get-Date)
Location: $backupFolder
Files: $(Get-ChildItem $backupFolder | Select-Object Name | Out-String)
"@ | Out-File "$backupFolder\BACKUP_INFO.txt"
```

### Step 6: Stop Running Services (If Applicable)

If R2Utilities runs as a Windows Service or scheduled task:

```powershell
# Check if service exists and is running
$serviceName = "R2UtilitiesService"
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service) {
    if ($service.Status -eq "Running") {
        Write-Host "Stopping service: $serviceName"
        Stop-Service -Name $serviceName -Force
        
        # Wait for service to stop
        $service.WaitForStatus('Stopped', '00:01:00')
        Write-Host "Service stopped successfully"
    }
}

# Check for running processes
$process = Get-Process -Name "R2Utilities" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Stopping R2Utilities process..."
    Stop-Process -Name "R2Utilities" -Force
}
```

### Step 7: Deploy Files to Staging

```powershell
# On staging server
$appFolder = "C:\Applications\R2Utilities"
$sourceFolder = "\\network-share\deploy\R2Utilities_EnhancedLogging_20250123"

# Copy new files
Copy-Item "$sourceFolder\R2Utilities.exe" -Destination $appFolder -Force
Copy-Item "$sourceFolder\R2Utilities.pdb" -Destination $appFolder -Force -ErrorAction SilentlyContinue

# Verify files copied
Write-Host "Deployed files:"
Get-ChildItem $appFolder -Filter "R2Utilities.*" | Select-Object Name, Length, LastWriteTime
```

### Step 8: Restart Services

```powershell
# Start the service
$serviceName = "R2UtilitiesService"
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service) {
    Write-Host "Starting service: $serviceName"
    Start-Service -Name $serviceName
    
    # Wait for service to start
    $service.WaitForStatus('Running', '00:01:00')
    Write-Host "Service started successfully"
    
    # Check service status
    Get-Service -Name $serviceName | Select-Object Name, Status, DisplayName
}
```

### Step 9: Verify Deployment

```powershell
# Check assembly version
$assemblyPath = "C:\Applications\R2Utilities\R2Utilities.exe"
$assembly = [System.Reflection.Assembly]::LoadFile($assemblyPath)
Write-Host "Deployed Version: $($assembly.GetName().Version)"

# Check file modified date
Get-Item $assemblyPath | Select-Object FullName, LastWriteTime, Length
```

---

## Post-Deployment Verification

### 1. Immediate Verification (First 5 Minutes)

#### Check Application Logs
```powershell
# View recent log entries (adjust path to your log location)
$logPath = "C:\Logs\R2Utilities\*.log"
Get-Content $logPath -Tail 50 | Where-Object { $_ -match "BookLoaderPostProcessingTask" }
```

#### Look for New Log Patterns
Expected log entries:
```
========== STARTING BookLoaderPostProcessingTask ==========
ISBN: [ISBN_NUMBER]
Include Chapter Numbers in TOC: [true/false]
Validation URL: [URL]
```

#### Check Service Status
```powershell
# Verify service is running
Get-Service -Name "R2UtilitiesService" | Select-Object Name, Status, StartType

# Check Windows Event Log for errors
Get-EventLog -LogName Application -Source "R2Utilities" -Newest 20 -EntryType Error, Warning
```

### 2. Functional Testing (First 30 Minutes)

#### Test 1: Run Task Manually
```powershell
# Navigate to application folder
cd C:\Applications\R2Utilities

# Run task with test ISBN
.\R2Utilities.exe -BookLoaderPostProcessingTask -isbn=TEST_ISBN_NUMBER -includeChapterNumbersInToc=false
```

#### Test 2: Verify Enhanced Logging

Check log file for these new entries:

✅ **Phase Headers**:
- `========== PHASE 1: Update Resource Data ==========`
- `========== PHASE 2: Copy Content ==========`
- `========== PHASE 3: Transform XML Content ==========`
- `========== PHASE 4: Set Resource Status to Active ==========`
- `========== PHASE 5: Add Missing Auto Licenses ==========`

✅ **Success Messages**:
- `PHASE X COMPLETED SUCCESSFULLY: [description]`
- `STEP X Duration: X.XX seconds, Success: True`

✅ **Resource Information**:
- `Resource found successfully - ID: [ID], Title: [TITLE]`
- `Sort Title Calculation - Original: '[TITLE]', Sort: '[SORT]', AlphaChar: '[CHAR]'`

✅ **File Operations**:
- `XML Copy - Source: [PATH]`
- `XML Copy - Destination: [PATH]`
- `Copy completed - X of Y files copied, X.XXX MB total`

✅ **Database Operations**:
- `UpdateNewResourceFields succeeded - X row(s) updated`
- `Specialty insert completed - X row(s) inserted`
- `Practice area insert completed - X row(s) inserted`
- `A-to-Z index update completed - Total: X records inserted`

✅ **Licensing Information**:
- `AddMissingAutoLicenses returned X institution(s)`
- `PHASE 5 COMPLETED SUCCESSFULLY: Total of X license(s) added across X institution(s)`

✅ **Completion**:
- `========== TASK COMPLETED SUCCESSFULLY ==========`
- `Total execution time: X.XX seconds`

#### Test 3: Verify Error Handling

Intentionally trigger an error (e.g., invalid ISBN):
```powershell
.\R2Utilities.exe -BookLoaderPostProcessingTask -isbn=INVALID_ISBN
```

Check for error logging:
- `========== TASK FAILED WITH EXCEPTION ==========`
- `Exception Type: [TYPE]`
- `Exception Message: [MESSAGE]`
- `Exception StackTrace: [TRACE]`

### 3. Extended Monitoring (First 24 Hours)

#### Create Monitoring Checklist
- [ ] Monitor log file size (new logging may increase file growth)
- [ ] Check disk space on log drive
- [ ] Verify no performance degradation
- [ ] Monitor scheduled task executions
- [ ] Review log entries for any unexpected patterns

#### Set Up Log Monitoring
```powershell
# Create a monitoring script
$script = @'
# Monitor R2Utilities logs for new patterns
$logPath = "C:\Logs\R2Utilities\R2Utilities_$(Get-Date -Format 'yyyyMMdd').log"
$patterns = @(
    "TASK COMPLETED SUCCESSFULLY",
    "TASK FAILED",
    "PHASE [0-9] FAILED",
    "COPY FAILED",
    "CRITICAL FAILURE"
)

foreach ($pattern in $patterns) {
    $matches = Select-String -Path $logPath -Pattern $pattern
    if ($matches) {
        Write-Host "$($matches.Count) occurrence(s) of: $pattern"
    }
}
'@

# Save and run daily
$script | Out-File "C:\Scripts\Monitor_R2Utilities.ps1"
```

---

## Rollback Procedure

If issues are detected after deployment, follow this rollback procedure:

### Quick Rollback Steps

1. **Stop the Service**:
   ```powershell
   Stop-Service -Name "R2UtilitiesService" -Force
   ```

2. **Restore Backup Files**:
   ```powershell
   $backupFolder = "C:\Backups\R2Utilities_[BACKUP_TIMESTAMP]"
   $appFolder = "C:\Applications\R2Utilities"
   
   # Restore previous version
   Copy-Item "$backupFolder\R2Utilities.exe" -Destination $appFolder -Force
   Copy-Item "$backupFolder\R2Utilities.pdb" -Destination $appFolder -Force -ErrorAction SilentlyContinue
   ```

3. **Restart Service**:
   ```powershell
   Start-Service -Name "R2UtilitiesService"
   Get-Service -Name "R2UtilitiesService" | Select-Object Name, Status
   ```

4. **Verify Rollback**:
   ```powershell
   # Check file dates
   Get-Item "$appFolder\R2Utilities.exe" | Select-Object FullName, LastWriteTime
   
   # Test basic functionality
   & "$appFolder\R2Utilities.exe" -help
   ```

5. **Document Rollback**:
   - Record reason for rollback
   - Document issues encountered
   - Note timestamp of rollback
   - Update deployment tracking system

---

## Troubleshooting

### Issue 1: Build Failures

**Symptoms**: Solution won't build, compilation errors

**Solutions**:
1. Clean solution: `Build > Clean Solution`
2. Rebuild: `Build > Rebuild Solution`
3. Check for missing NuGet packages: `Tools > NuGet Package Manager > Restore`
4. Verify .NET Framework 4.8.1 SDK is installed

### Issue 2: Deployment Files Not Copying

**Symptoms**: Access denied, file in use

**Solutions**:
1. Ensure service/application is fully stopped
2. Check file permissions on staging server
3. Verify antivirus isn't blocking
4. Use `Handle.exe` from Sysinternals to find locks:
   ```powershell
   handle.exe R2Utilities.exe
   ```

### Issue 3: Service Won't Start After Deployment

**Symptoms**: Service fails to start, error in Event Log

**Solutions**:
1. Check Windows Event Log:
   ```powershell
   Get-EventLog -LogName Application -Source "R2Utilities" -Newest 10
   ```
2. Verify all dependencies are deployed
3. Check .NET Framework version compatibility
4. Review service configuration
5. Test running executable manually:
   ```powershell
   & "C:\Applications\R2Utilities\R2Utilities.exe" -help
   ```

### Issue 4: Logs Not Showing New Entries

**Symptoms**: New logging statements not appearing in logs

**Possible Causes**:
1. **Wrong log file**: Check log4net or logging configuration
2. **Log level too high**: Ensure INFO level is enabled
3. **Old version still running**: Verify correct assembly version deployed
4. **Logging framework issue**: Check logging configuration file

**Solutions**:
```powershell
# Check logging configuration
$configPath = "C:\Applications\R2Utilities\R2Utilities.exe.config"
Get-Content $configPath | Select-String -Pattern "log4net|logging|level"

# Verify log file location
$logConfig = [xml](Get-Content $configPath)
$logConfig.configuration.log4net.appender | Select-Object name, file

# Check log file permissions
$logPath = "C:\Logs\R2Utilities"
icacls $logPath
```

### Issue 5: Excessive Log File Growth

**Symptoms**: Log files growing too large, disk space issues

**Solutions**:
1. Review log rotation settings in log4net config
2. Adjust log levels if too verbose
3. Implement log archival strategy:
   ```powershell
   # Archive old logs
   $logPath = "C:\Logs\R2Utilities"
   $archivePath = "C:\Logs\Archive\R2Utilities"
   
   # Move logs older than 30 days
   Get-ChildItem $logPath -Filter "*.log" | 
       Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
       Move-Item -Destination $archivePath
   ```

### Issue 6: Performance Degradation

**Symptoms**: Task runs slower after deployment

**Investigation**:
1. Compare execution times from logs:
   ```powershell
   # Extract duration from logs
   $logPath = "C:\Logs\R2Utilities\*.log"
   Select-String -Path $logPath -Pattern "Total execution time: (\d+\.\d+) seconds" |
       ForEach-Object { $_.Matches.Groups[1].Value }
   ```

2. Check if logging to network drive (slow)
3. Review log file write performance
4. Consider async logging if needed

**Solutions**:
- Adjust log levels (reduce DEBUG to INFO)
- Use async appenders in log4net
- Ensure logs write to local disk, not network

---

## Additional Resources

### Configuration Files to Review

1. **R2Utilities.exe.config** - Application configuration
2. **log4net.config** - Logging configuration
3. **Web.config** (if applicable) - Web application settings

### Important Paths

| Path Type | Location |
|-----------|----------|
| Source Code | `C:\RittenhouseRepos\R2Library\src\R2Utilities\` |
| Build Output | `C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Release\` |
| Staging App | `[STAGING_SERVER_APP_PATH]` |
| Staging Logs | `[STAGING_SERVER_LOG_PATH]` |
| Backups | `C:\Backups\R2Utilities\` |

### Key Contacts

| Role | Name | Contact |
|------|------|---------|
| Developer | [Your Name] | [Email/Phone] |
| DevOps Lead | [Name] | [Email/Phone] |
| QA Lead | [Name] | [Email/Phone] |
| Operations | [Name] | [Email/Phone] |

### Related Documentation

- **GitHub Repository**: https://github.com/Rittenhouse-Digital/R2Library
- **Task Documentation**: [Link to BookLoader docs]
- **Logging Standards**: [Link to logging standards]
- **Deployment Process**: [Link to general deployment guide]

---

## Deployment Checklist Summary

### Pre-Deployment
- [ ] Code reviewed and approved
- [ ] Local testing completed
- [ ] Changes committed to Git
- [ ] Build succeeds in Release mode
- [ ] Deployment package created
- [ ] Backup procedures documented

### Deployment
- [ ] Staging environment accessed
- [ ] Current files backed up
- [ ] Services/processes stopped
- [ ] New files deployed
- [ ] Services restarted
- [ ] Deployment verified

### Post-Deployment
- [ ] Logs showing new entries
- [ ] Manual test run completed successfully
- [ ] Error handling verified
- [ ] Performance baseline established
- [ ] Monitoring configured
- [ ] 24-hour observation period scheduled

### Sign-Off
- [ ] Deployment completed successfully
- [ ] All tests passed
- [ ] Stakeholders notified
- [ ] Documentation updated

**Deployed By**: __________________ **Date**: __________ **Time**: __________

**Verified By**: __________________ **Date**: __________ **Time**: __________

---

## Appendix A: Sample Log Output

### Successful Execution
```
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - ========== STARTING BookLoaderPostProcessingTask ==========
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - ISBN: 1433820579
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Include Chapter Numbers in TOC: False
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Validation URL: http://staging.example.com/resource/1433820579
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Retrieving resource by ISBN: 1433820579
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Resource found successfully - ID: 12345, Title: Sample Book Title
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - ========== PHASE 1: Update Resource Data ==========
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - >+++> STEP 1 - Update Resource Data for ISBN: 1433820579
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Processing resource - ID: 12345, Title: Sample Book Title
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Sort Title Calculation - Original: 'Sample Book Title', Sort: 'Sample Book Title', AlphaChar: 'S'
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Updating resource fields in database for Resource ID: 12345
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - UpdateNewResourceFields succeeded - 1 row(s) updated
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Updating resource specialties for Resource ID: 12345
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Checking existing specialties for Resource ID: 12345
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Found 0 existing specialty(ies)
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Inserting default specialty 'MED' for Resource ID: 12345
2025-01-23 10:30:15 INFO  BookLoaderPostProcessingTask - Specialty insert completed - 1 row(s) inserted
[... additional log entries ...]
2025-01-23 10:30:45 INFO  BookLoaderPostProcessingTask - ========== TASK COMPLETED SUCCESSFULLY ==========
2025-01-23 10:30:45 INFO  BookLoaderPostProcessingTask - All phases completed for ISBN: 1433820579
2025-01-23 10:30:45 INFO  BookLoaderPostProcessingTask - Total execution time: 30.25 seconds
```

### Failed Execution
```
2025-01-23 10:35:10 INFO  BookLoaderPostProcessingTask - ========== STARTING BookLoaderPostProcessingTask ==========
2025-01-23 10:35:10 INFO  BookLoaderPostProcessingTask - ISBN: INVALID123
2025-01-23 10:35:10 INFO  BookLoaderPostProcessingTask - Retrieving resource by ISBN: INVALID123
2025-01-23 10:35:10 ERROR BookLoaderPostProcessingTask - CRITICAL FAILURE: Resource not found by ISBN: INVALID123
2025-01-23 10:35:10 ERROR BookLoaderPostProcessingTask - ========== TASK FAILED - Resource Lookup ==========
2025-01-23 10:35:10 ERROR BookLoaderPostProcessingTask - ========== TASK FAILED WITH EXCEPTION ==========
2025-01-23 10:35:10 ERROR BookLoaderPostProcessingTask - Exception Type: Exception
2025-01-23 10:35:10 ERROR BookLoaderPostProcessingTask - Exception Message: CRITICAL FAILURE: Resource not found by ISBN: INVALID123
2025-01-23 10:35:10 ERROR BookLoaderPostProcessingTask - Exception StackTrace: [stack trace details]
2025-01-23 10:35:10 INFO  BookLoaderPostProcessingTask - Total execution time: 0.15 seconds
```

---

## Document Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-01-23 | GitHub Copilot | Initial deployment guide created |

---

**END OF DOCUMENT**
