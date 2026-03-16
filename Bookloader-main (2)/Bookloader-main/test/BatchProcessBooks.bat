@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM ============================================================================
REM BatchProcessBooks.bat - Process multiple books in test\input folder
REM (This script lives in the \test folder)
REM ============================================================================
REM Processes all ZIP files AND unzipped folders in test\input
REM Outputs to test\batchoutput
REM Creates timestamped final archive with all processed ISBNs
REM Includes R2Utilities post-processing to create licenses (if configured)
REM ============================================================================

REM Ensure window stays open on ANY error
if not defined IN_SUBPROCESS (
    set IN_SUBPROCESS=1
    cmd /k "%~f0" %*
    exit
)

REM --- Resolve directories based on script location ---
REM SCRIPT_DIR = ...\test\
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

REM TESTDIR = ...\test
set "TESTDIR=%SCRIPT_DIR%"

REM BASEDIR = parent of test (project root)
pushd "%TESTDIR%\.." >nul 2>&1
if errorlevel 1 (
    echo ERROR: Could not navigate to parent directory
    echo Script directory: %TESTDIR%
    echo.
    echo Press any key to exit...
    pause >nul
    exit /b 1
)
set "BASEDIR=%CD%"
popd >nul

REM Always run from BASEDIR so relative tool behavior is stable
pushd "%BASEDIR%" >nul 2>&1
if errorlevel 1 (
    echo ERROR: Could not navigate to base directory: %BASEDIR%
    echo.
    echo Press any key to exit...
    pause >nul
    exit /b 1
)

echo.
echo ================================================================================
echo                    BATCH BOOK PROCESSING TOOL
echo ================================================================================
echo.
echo This tool will process all ZIP files AND unzipped folders in: test\input
echo Output will be saved to: test\batchoutput\[timestamped-folder]
echo.
echo ================================================================================
echo CONFIGURATION
echo ================================================================================
echo.
echo Post-processing default: DISABLED
echo   Use /postprocess to enable R2Utilities post-processing for this run.
echo.

set "POSTPROCESS_REQUESTED=0"
for %%A in (%*) do (
    if /I "%%~A"=="/postprocess" set "POSTPROCESS_REQUESTED=1"
)

REM Configure R2Utilities post-processing
set "R2UTILITIES_EXE=C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Debug\net481\R2Utilities.exe"
set "R2UTILITIES_ENABLED=true"

if exist "%R2UTILITIES_EXE%" (
    echo Post-Processing:  ENABLED ^(R2Utilities found^)
) else (
    echo Post-Processing:  DISABLED ^(R2Utilities not found^)
    echo   Configure R2UTILITIES_EXE in BatchProcessBooks.bat to enable
    set "R2UTILITIES_ENABLED=false"
)

echo.
echo Select processing mode:
echo   [1] Normal + No PMID ^(DEFAULT - Update + DB, no PubMed lookups^)
echo   [2] Normal + PMID ^(Full processing with database + PubMed lookups^)
echo   [3] Ultra Fast ^(Skip All - No DB, No PMID, No Linking^)
echo   [4] DB Staging ^(DB, No PMID, No Linking^)
echo.
set /p MODE_CHOICE="Enter choice [1-4] (default=1): "

REM Default to mode 1 if empty
if "!MODE_CHOICE!"=="" set MODE_CHOICE=1

REM Set flags based on choice
if "!MODE_CHOICE!"=="1" (
    set "FLAGS=--normal --skipPMID"
    set "MODE_NAME=Normal_No_PMID"
    set "ENABLE_POST_PROCESS=0"
    echo.
    echo Selected: Normal + No PMID ^(Database, No PubMed^)
) else if "!MODE_CHOICE!"=="2" (
    set "FLAGS=--update --normal"
    set "MODE_NAME=Normal_Update"
    set "ENABLE_POST_PROCESS=0"
    echo.
    echo Selected: Normal + Update ^(Full Database Mode with PMID^)
) else if "!MODE_CHOICE!"=="3" (
    set "FLAGS=--noDB --skipPMID --skipLinks"
    set "MODE_NAME=Ultra_Fast"
    set "ENABLE_POST_PROCESS=0"
    echo.
    echo Selected: Ultra Fast ^(Skip All - No DB, No PMID, No Linking^)
) else if "!MODE_CHOICE!"=="4" (
    set "FLAGS=--update --skipPMID --skipLinks"
    set "MODE_NAME=DB_Update_NoPMID_NoLinks"
    set "ENABLE_POST_PROCESS=0"
    echo.
    echo Selected: DB Update + Skip PMID + Skip Links ^(DB, No PMID, No Linking^)
)else (
    echo Invalid choice. Using default: Normal + No PMID
    set "FLAGS=--update --normal --skipPMID"
    set "MODE_NAME=Normal_No_PMID"
    set "ENABLE_POST_PROCESS=0"
)

if "!POSTPROCESS_REQUESTED!"=="1" (
    if "!MODE_CHOICE!"=="3" (
        echo Post-processing requested via /postprocess but ignored for Ultra Fast mode.
    ) else (
        if not "!ENABLE_POST_PROCESS!"=="1" (
            echo Post-processing requested via /postprocess
        )
        set "ENABLE_POST_PROCESS=1"
    )
)

echo ================================================================================
echo.

REM Verify input directory exists
if not exist "%TESTDIR%\input" (
    echo.
    echo ERROR: Input directory does not exist: %TESTDIR%\input
    echo.
    echo Please ensure the 'input' folder exists in the test directory.
    echo.
    echo Press any key to exit...
    pause >nul
    popd >nul
    exit /b 1
)

REM Create timestamp for this batch run
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set "datetime=%%I"
set "TIMESTAMP=%datetime:~0,4%%datetime:~4,2%%datetime:~6,2%_%datetime:~8,2%%datetime:~10,2%%datetime:~12,2%"

REM Create batch output directory
set "BATCH_OUTPUT_ROOT=%TESTDIR%\batchoutput"
set "BATCH_RUN_DIR=%BATCH_OUTPUT_ROOT%\Batch_%TIMESTAMP%_%MODE_NAME%"
if not exist "%BATCH_RUN_DIR%" mkdir "%BATCH_RUN_DIR%"

REM Create batch log file
set "BATCH_LOG=%BATCH_RUN_DIR%\batch_processing_log.txt"

echo Batch Processing Started: %date% %time% > "%BATCH_LOG%"
echo Mode: %MODE_NAME% >> "%BATCH_LOG%"
echo Flags: %FLAGS% >> "%BATCH_LOG%"
echo ================================================================================== >> "%BATCH_LOG%"
echo. >> "%BATCH_LOG%"

REM Initialize counters
set "TOTAL_BOOKS=0"
set "SUCCESS_COUNT=0"
set "FAILURE_COUNT=0"

REM Build list of all books FIRST (ZIPs + folders, excluding special folders)
echo Scanning for books to process...
set "BOOK_LIST="

REM First, collect all ZIP files
for %%F in ("%TESTDIR%\input\*.zip") do (
    set /a TOTAL_BOOKS+=1
    REM Store full path and type
    set "BOOK_!TOTAL_BOOKS!=%%~fF"
    set "BOOKNAME_!TOTAL_BOOKS!=%%~nxF"
    set "BOOKTYPE_!TOTAL_BOOKS!=ZIP"
)

REM Then, collect all folders (excluding special folders like .batch_temp)
for /d %%D in ("%TESTDIR%\input\*") do call :CheckAndAddFolder "%%~fD" "%%~nxD"

echo Processing !TOTAL_BOOKS! books ^(ZIPs and folders^)...
echo.
echo [!date! !time!] INFO: Starting batch processing of !TOTAL_BOOKS! books >> "%BATCH_LOG%"

REM Protect TOTAL_BOOKS from being modified during loop
set "_LOOP_MAX=!TOTAL_BOOKS!"

REM Now process each book from our captured list
for /L %%N in (1,1,!_LOOP_MAX!) do (
    echo.
    echo ============================================================================
    echo [DEBUG] Starting book %%N of !_LOOP_MAX!
    echo ============================================================================
    echo [!date! !time!] INFO: Starting book %%N of !_LOOP_MAX! >> "%BATCH_LOG%"
    
    call :ProcessOneBook %%N
    set "_LAST_RESULT=!ERRORLEVEL!"
    
    echo [!date! !time!] INFO: Completed book %%N ^(returned: !_LAST_RESULT!^) >> "%BATCH_LOG%"
    echo [DEBUG] Completed book %%N of !_LOOP_MAX! ^(result: !_LAST_RESULT!^)
)

goto :BatchComplete

:CheckAndAddFolder
    set "FOLDER_PATH=%~1"
    set "FOLDER_NAME=%~2"
    if not "%FOLDER_NAME%"==".batch_temp" (
        set /a TOTAL_BOOKS+=1
        set "BOOK_!TOTAL_BOOKS!=%FOLDER_PATH%"
        set "BOOKNAME_!TOTAL_BOOKS!=%FOLDER_NAME%"
        set "BOOKTYPE_!TOTAL_BOOKS!=FOLDER"
    )
goto :EOF

goto :BatchComplete

:ProcessOneBook
setlocal EnableDelayedExpansion
    set "BOOK_INDEX=%~1"
    set "BOOK_PATH=!BOOK_%BOOK_INDEX%!"
    set "BOOK_NAME=!BOOKNAME_%BOOK_INDEX%!"
    set "BOOK_TYPE=!BOOKTYPE_%BOOK_INDEX%!"
    
    REM Extract ISBN from name (first part before underscore or full name)
    for /f "tokens=1 delims=_" %%I in ("!BOOK_NAME!") do set "ISBN=%%I"
    
    REM For folders, the folder name itself is likely the ISBN
    if "!BOOK_TYPE!"=="FOLDER" (
        set "ISBN=!BOOK_NAME!"
    )

    echo ================================================================================
    echo Processing [!BOOK_TYPE!]: !BOOK_NAME!
    echo ISBN: !ISBN!
    echo ================================================================================

    echo. >> "%BATCH_LOG%"
    echo [!date! !time!] Processing [!BOOK_TYPE!]: !BOOK_NAME! ^(ISBN: !ISBN!^) >> "%BATCH_LOG%"

    REM CRITICAL: Isolate this book for processing
    echo Isolating this book for processing...
    if not exist "%TESTDIR%\.batch_temp" mkdir "%TESTDIR%\.batch_temp"
    
    REM Move/restore this specific book back to input if needed
    if "!BOOK_TYPE!"=="ZIP" (
        if exist "%TESTDIR%\.batch_temp\!BOOK_NAME!" (
            move "%TESTDIR%\.batch_temp\!BOOK_NAME!" "%TESTDIR%\input\" >nul 2>&1
        )
    ) else (
        if exist "%TESTDIR%\.batch_temp\!BOOK_NAME!" (
            move "%TESTDIR%\.batch_temp\!BOOK_NAME!" "%TESTDIR%\input\" >nul 2>&1
        )
    )
    
    REM Move all OTHER items (ZIPs and folders) to batch_temp
    for %%Z in ("%TESTDIR%\input\*.zip") do call :MoveIfNotCurrent "%%Z" "%%~nxZ"
    
    for /d %%D in ("%TESTDIR%\input\*") do call :MoveFolderIfNotCurrent "%%D" "%%~nxD"

    REM Clean ALL work directories before processing THIS book
    echo Cleaning work directories...
    REM Clean test\temp contents but PRESERVE the dtd subfolder
    if exist "%TESTDIR%\temp" (
        for %%F in ("%TESTDIR%\temp\*") do del /f /q "%%F" 2>nul
        for /d %%D in ("%TESTDIR%\temp\*") do (
            if /I not "%%~nxD"=="dtd" rmdir /s /q "%%D" 2>nul
        )
    )
    if exist "%TESTDIR%\output" rmdir /s /q "%TESTDIR%\output" 2>nul
    if exist "%TESTDIR%\media" rmdir /s /q "%TESTDIR%\media" 2>nul
    if exist "%TESTDIR%\R2v2-XMLbyISBN" rmdir /s /q "%TESTDIR%\R2v2-XMLbyISBN" 2>nul

    mkdir "%TESTDIR%\temp" 2>nul
    mkdir "%TESTDIR%\output" 2>nul
    mkdir "%TESTDIR%\media" 2>nul
    mkdir "%TESTDIR%\R2v2-XMLbyISBN" 2>nul

    REM Run the Java bookloader
    REM For ZIPs: it will auto-extract
    REM For folders: it will use the folder directly
    if "!BOOK_TYPE!"=="ZIP" (
        echo Running bookloader for ISBN: !ISBN! ^(from ZIP^)...
    ) else (
        echo Running bookloader for ISBN: !ISBN! ^(from folder^)...
    )

    call :RunBookloader "!ISBN!" "!FLAGS!"
    set "BOOK_EXIT_CODE=!ERRORLEVEL!"

    if !BOOK_EXIT_CODE! NEQ 0 goto :BookFailed

    REM === SUCCESS PATH ===
    echo [SUCCESS] !ISBN! processed successfully
    echo [!date! !time!] SUCCESS: !ISBN! >> "%BATCH_LOG%"

    REM Create output directory with _PASS suffix
    set "ISBN_OUTPUT=%BATCH_RUN_DIR%\!ISBN!_PASS"
    if not exist "!ISBN_OUTPUT!" mkdir "!ISBN_OUTPUT!"

    REM Copy ONLY XML files from all possible locations to ISBN directory
    echo Collecting XML output files...

    REM From test\output (main processing output)
    if exist "%TESTDIR%\output\*.xml" (
        echo   Copying from test\output...
        xcopy /Y "%TESTDIR%\output\*.xml" "!ISBN_OUTPUT!\" >nul 2>&1
    )

    REM From R2v2-XMLbyISBN (final content location)
    if exist "%TESTDIR%\R2v2-XMLbyISBN\!ISBN!\xml" (
        echo   Copying from R2v2-XMLbyISBN\!ISBN!\xml...
        xcopy /Y "%TESTDIR%\R2v2-XMLbyISBN\!ISBN!\xml\*.xml" "!ISBN_OUTPUT!\" >nul 2>&1
    )

    REM From temp (book.isbn.xml and processed files)
    if exist "%TESTDIR%\temp\*.xml" (
        echo   Copying from test\temp...
        xcopy /Y "%TESTDIR%\temp\*.xml" "!ISBN_OUTPUT!\" >nul 2>&1
    )

    REM Count XML files collected
    set "XML_COUNT=0"
    for %%X in ("!ISBN_OUTPUT!\*.xml") do set /a XML_COUNT+=1
    echo   Total XML files collected: !XML_COUNT!

    REM === POST-PROCESSING STEP ===
    REM This creates licenses in tResourceLicense table and activates the book
    REM Only run if this mode includes database operations
    if "!ENABLE_POST_PROCESS!"=="1" (
        if "%R2UTILITIES_ENABLED%"=="true" (
            if exist "%R2UTILITIES_EXE%" (
                echo.
                echo Running post-processing with R2Utilities...
                echo [!date! !time!] INFO : ^(BatchProcessBooks.RunR2UtilitiesPostProcess^) About to run BookLoaderPostProcessingTask for ISBN !ISBN! >> "%BATCH_LOG%"
                echo [!date! !time!] INFO : ^(BatchProcessBooks.RunR2UtilitiesPostProcess^) CMD: "!R2UTILITIES_EXE!" -BookLoaderPostProcessingTask -isbn=!ISBN! -includeChapterNumbersInToc=true >> "%BATCH_LOG%"
                
                REM Run R2Utilities directly - it has its own log4net logging configuration
                REM Let output flow naturally to console (R2Utilities logs to its own files)
                "%R2UTILITIES_EXE%" -BookLoaderPostProcessingTask -isbn=!ISBN! -includeChapterNumbersInToc=true
                set "PP_EXIT_CODE=!ERRORLEVEL!"
                
                echo [!date! !time!] R2Utilities completed with exit code: !PP_EXIT_CODE! >> "%BATCH_LOG%"
                
                if !PP_EXIT_CODE! NEQ 0 (
                    echo   WARNING: Post-processing failed with exit code !PP_EXIT_CODE!
                    echo   [!date! !time!] WARNING: Post-processing failed for !ISBN! ^(Exit Code: !PP_EXIT_CODE!^) >> "%BATCH_LOG%"
                ) else (
                    echo   Post-processing completed successfully
                    echo   [!date! !time!] Post-processing completed for !ISBN! >> "%BATCH_LOG%"
                )
            ) else (
                echo.
                echo Skipping post-processing: R2Utilities not found at:
                echo   %R2UTILITIES_EXE%
                echo [!date! !time!] WARNING: Post-processing SKIPPED - R2Utilities not found >> "%BATCH_LOG%"
                set "PP_EXIT_CODE=0"
            )
        ) else (
            echo.
            echo Skipping post-processing: R2Utilities disabled
            echo [!date! !time!] WARNING: Post-processing SKIPPED - disabled >> "%BATCH_LOG%"
            set "PP_EXIT_CODE=0"
        )
    ) else (
        echo.
        echo Skipping post-processing: Mode does not use database ^(!MODE_NAME!^)
        echo [!date! !time!] INFO : Post-processing SKIPPED - Mode !MODE_NAME! does not use database >> "%BATCH_LOG%"
        set "PP_EXIT_CODE=0"
    )

    goto :AfterBookProcessing

    :BookFailed
    REM === FAILURE PATH ===
    echo [FAILURE] !ISBN! processing failed with exit code !BOOK_EXIT_CODE!
    echo [!date! !time!] FAILURE: !ISBN! ^(Exit Code: !BOOK_EXIT_CODE!^) >> "%BATCH_LOG%"

    REM Create output directory with _FAIL suffix
    set "ISBN_OUTPUT=%BATCH_RUN_DIR%\!ISBN!_FAIL"
    if not exist "!ISBN_OUTPUT!" mkdir "!ISBN_OUTPUT!"

    REM Create detailed error log file
    set "ERROR_LOG=!ISBN_OUTPUT!\ERROR_LOG_!ISBN!.txt"
    echo ================================================================================ > "!ERROR_LOG!"
    echo PROCESSING FAILURE DETAILS >> "!ERROR_LOG!"
    echo ================================================================================ >> "!ERROR_LOG!"
    echo. >> "!ERROR_LOG!"
    echo ISBN: !ISBN! >> "!ERROR_LOG!"
    echo Type: !BOOK_TYPE! >> "!ERROR_LOG!"
    echo Exit Code: !BOOK_EXIT_CODE! >> "!ERROR_LOG!"
    echo Timestamp: !date! !time! >> "!ERROR_LOG!"
    echo Mode: %MODE_NAME% >> "!ERROR_LOG!"
    echo Flags: !FLAGS! >> "!ERROR_LOG!"
    echo. >> "!ERROR_LOG!"
    echo ================================================================================ >> "!ERROR_LOG!"
    echo DIAGNOSTICS >> "!ERROR_LOG!"
    echo ================================================================================ >> "!ERROR_LOG!"
    echo. >> "!ERROR_LOG!"
    
    REM Check if Java log exists and append it
    if exist "%TESTDIR%\logs\RISBackend.log" (
        echo --- RISBackend.log ^(last 100 lines^) --- >> "!ERROR_LOG!"
        powershell -NoProfile -Command "Get-Content '%TESTDIR%\logs\RISBackend.log' -Tail 100" >> "!ERROR_LOG!" 2>&1
        echo. >> "!ERROR_LOG!"
    ) else (
        echo RISBackend.log not found >> "!ERROR_LOG!"
        echo. >> "!ERROR_LOG!"
    )
    
    REM Check for book directory
    if exist "%TESTDIR%\input\!ISBN!" (
        echo Book directory exists: %TESTDIR%\input\!ISBN! >> "!ERROR_LOG!"
    ) else (
        echo WARNING: Book directory not found >> "!ERROR_LOG!"
    )
    echo. >> "!ERROR_LOG!"
    
    REM List any files created
    echo Files in test\output: >> "!ERROR_LOG!"
    if exist "%TESTDIR%\output\*.*" (
        dir /b "%TESTDIR%\output\*.*" >> "!ERROR_LOG!" 2>&1
    ) else (
        echo ^(none^) >> "!ERROR_LOG!"
    )
    echo. >> "!ERROR_LOG!"
    
    echo Files in test\temp: >> "!ERROR_LOG!"
    if exist "%TESTDIR%\temp\*.*" (
        dir /b "%TESTDIR%\temp\*.*" >> "!ERROR_LOG!" 2>&1
    ) else (
        echo ^(none^) >> "!ERROR_LOG!"
    )
    echo. >> "!ERROR_LOG!"
    
    echo ================================================================================ >> "!ERROR_LOG!"
    echo END OF ERROR LOG >> "!ERROR_LOG!"
    echo ================================================================================ >> "!ERROR_LOG!"
    
    echo   ^> Detailed error log saved to: !ERROR_LOG!
    echo [!date! !time!] Error log created: !ERROR_LOG! >> "%BATCH_LOG%"

    REM Even on failure, try to collect any XML files that were created
    if exist "%TESTDIR%\output\*.xml" xcopy /Y "%TESTDIR%\output\*.xml" "!ISBN_OUTPUT!\" >nul 2>&1
    if exist "%TESTDIR%\R2v2-XMLbyISBN\!ISBN!\xml\*.xml" xcopy /Y "%TESTDIR%\R2v2-XMLbyISBN\!ISBN!\xml\*.xml" "!ISBN_OUTPUT!\" >nul 2>&1
    if exist "%TESTDIR%\temp\*.xml" xcopy /Y "%TESTDIR%\temp\*.xml" "!ISBN_OUTPUT!\" >nul 2>&1

    :AfterBookProcessing
    REM Restore other books back to input directory
    echo Restoring other books to input folder...
    if exist "%TESTDIR%\.batch_temp\*.zip" (
        move "%TESTDIR%\.batch_temp\*.zip" "%TESTDIR%\input\" >nul 2>&1
    )
    if exist "%TESTDIR%\.batch_temp\*" (
        for /d %%D in ("%TESTDIR%\.batch_temp\*") do (
            move "%%D" "%TESTDIR%\input\" >nul 2>&1
        )
    )

    REM Clean up - if this was a folder, we keep it; if ZIP was extracted, clean that
    echo Cleaning up...
    if "!BOOK_TYPE!"=="ZIP" (
        REM Clean up any extracted folder from this ZIP
        if exist "%TESTDIR%\input\!ISBN!" rmdir /s /q "%TESTDIR%\input\!ISBN!" 2>nul
    )
    REM Note: We DON'T delete the original folder-based books

    echo.
    echo [DEBUG] Finishing processing for ISBN: !ISBN! ^(BOOK_EXIT_CODE=!BOOK_EXIT_CODE!^)
    echo [!date! !time!] DEBUG: Finishing ISBN !ISBN! ^(BookLoader ExitCode: !BOOK_EXIT_CODE!^) >> "%BATCH_LOG%"
    
    REM Exit local scope and increment appropriate counter in parent scope
    if !BOOK_EXIT_CODE! EQU 0 (
        endlocal & set "_BOOK_SUCCESS=1"
    ) else (
        endlocal & set "_BOOK_SUCCESS=0"
    )
    
    REM Now update counters in parent scope
    if "%_BOOK_SUCCESS%"=="1" (
        set /a SUCCESS_COUNT+=1
    ) else (
        set /a FAILURE_COUNT+=1
    )
    set "_BOOK_SUCCESS="
    
    REM CRITICAL: Explicitly return to caller to ensure loop continues
    exit /b 0

:BatchComplete
REM Write summary
echo ================================================================================
echo                           BATCH PROCESSING COMPLETE
echo ================================================================================
echo Total Books:    !TOTAL_BOOKS!
echo Successful:     !SUCCESS_COUNT!
echo Failed:         !FAILURE_COUNT!
echo ================================================================================
echo.
echo Output Location: %BATCH_RUN_DIR%
echo.

echo. >> "%BATCH_LOG%"
echo ================================================================================== >> "%BATCH_LOG%"
echo BATCH SUMMARY >> "%BATCH_LOG%"
echo ================================================================================== >> "%BATCH_LOG%"
echo Total Books:    !TOTAL_BOOKS! >> "%BATCH_LOG%"
echo Successful:     !SUCCESS_COUNT! >> "%BATCH_LOG%"
echo Failed:         !FAILURE_COUNT! >> "%BATCH_LOG%"
echo Completed:      !date! !time! >> "%BATCH_LOG%"
echo ================================================================================== >> "%BATCH_LOG%"

REM Clean up batch temp directory
if exist "%TESTDIR%\.batch_temp" rmdir /s /q "%TESTDIR%\.batch_temp" 2>nul

REM Create final ZIP archive of all processed books
echo Creating final archive...
set "FINAL_ZIP=%BATCH_OUTPUT_ROOT%\Batch_%TIMESTAMP%_%MODE_NAME%.zip"

if exist "%FINAL_ZIP%" del /f /q "%FINAL_ZIP%" >nul 2>&1

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%BATCH_RUN_DIR%\*' -DestinationPath '%FINAL_ZIP%' -Force"


if exist "%FINAL_ZIP%" (
    echo.
    echo Final archive created: %FINAL_ZIP%
    echo. >> "%BATCH_LOG%"
    echo Final Archive: %FINAL_ZIP% >> "%BATCH_LOG%"
) else (
    echo.
    echo Warning: Could not create final ZIP archive
)

echo.
echo Press any key to exit...
pause >nul

popd >nul
goto :EOF

REM ============================================================================
REM Helper Subroutines
REM ============================================================================
:MoveIfNotCurrent
    if not "%~2"=="!BOOK_NAME!" (
        move "%~1" "%TESTDIR%\.batch_temp\" >nul 2>&1
    )
goto :EOF

:MoveFolderIfNotCurrent
    set "OTHER_FOLDER=%~2"
    if not "%OTHER_FOLDER%"=="!BOOK_NAME!" (
        if not "%OTHER_FOLDER%"==".batch_temp" (
            move "%~1" "%TESTDIR%\.batch_temp\" >nul 2>&1
        )
    )
goto :EOF

REM ============================================================================
REM Subroutine: Run Bookloader for a single ISBN
REM ============================================================================
:RunBookloader
    setlocal EnableExtensions EnableDelayedExpansion
    set "RUN_ISBN=%~1"
    set "RUN_FLAGS=%~2"

    REM Find Java executable
    if defined JAVA_HOME (
        set "JAVA_EXE=%JAVA_HOME%\bin\java.exe"
    ) else (
        set "JAVA_EXE=C:\Program Files\Eclipse Adoptium\jdk-25.0.0.36-hotspot\bin\java.exe"
    )

    if not exist "!JAVA_EXE!" (
        echo ================================================================================
        echo ERROR: Java executable not found at: !JAVA_EXE!
        echo ================================================================================
        echo.
        echo Please check:
        echo   1. Java is installed
        echo   2. JAVA_HOME environment variable is set correctly
        echo   3. Or update the hardcoded path in this script
        echo.
        echo Press any key to continue...
        pause >nul
        endlocal & exit /b 1
    )

    echo.
    echo Java Executable: !JAVA_EXE!
    echo.

    REM Build classpath (BASEDIR points to project root that contains lib\ and build\)
    set "CP=%BASEDIR%\build\classes"
    
    REM Verify build directory exists
    if not exist "%BASEDIR%\build\classes" (
        echo ================================================================================
        echo ERROR: Build directory not found: %BASEDIR%\build\classes
        echo ================================================================================
        echo.
        echo The Java classes directory is missing. Please ensure:
        echo   1. The project has been compiled
        echo   2. The build\classes directory exists
        echo   3. You are running from the correct location
        echo.
        echo Current BASEDIR: %BASEDIR%
        echo Current TESTDIR: %TESTDIR%
        echo.
        echo Press any key to continue...
        pause >nul
        endlocal & exit /b 1
    )
    
    for %%J in ("%BASEDIR%\lib\*.jar") do set "CP=!CP!;%%~fJ"
    for %%J in ("%BASEDIR%\lib\jakarta\*.jar") do set "CP=!CP!;%%~fJ"
    for %%J in ("%BASEDIR%\lib\jdbc\*.jar") do set "CP=!CP!;%%~fJ"
    for %%J in ("%BASEDIR%\lib\saxon\*.jar") do set "CP=!CP!;%%~fJ"
    for %%J in ("%BASEDIR%\lib\textml\*.jar") do set "CP=!CP!;%%~fJ"
    for %%J in ("%BASEDIR%\lib\xalan\*.jar") do set "CP=!CP!;%%~fJ"
    for %%J in ("%BASEDIR%\lib\xerces\*.jar") do set "CP=!CP!;%%~fJ"

    REM Run the bookloader with proper JVM arguments
    REM Output is shown on console but also logged for diagnostics on failure
    "!JAVA_EXE!" -Xms1g -Xmx2g ^
        --enable-native-access=ALL-UNNAMED ^
        -Djdk.xml.entityExpansionLimit=10000 ^
        -Djdk.xml.totalEntitySizeLimit=1000000 ^
        -Djava.security.policy=java.ris.policy ^
        -cp "!CP!" ^
        com.rittenhouse.RIS.Main !RUN_FLAGS!

    REM Preserve ERRORLEVEL and exit with it
    endlocal & exit /b %ERRORLEVEL%
