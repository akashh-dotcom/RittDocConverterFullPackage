@echo off
REM ============================================================================
REM PostProcessOnly.bat - Run R2Utilities post-processing on a list of ISBNs
REM ============================================================================
REM Runs ONLY the post-processing step (license creation, database updates)
REM without re-processing the original books
REM
REM Usage:
REM   PostProcessOnly.bat <ISBN>                               (single ISBN)
REM   PostProcessOnly.bat 9781234567890 9789876543210         (multiple ISBNs)
REM   PostProcessOnly.bat @isbnlist.txt                        (from file with @prefix)
REM
REM ============================================================================

setlocal EnableExtensions EnableDelayedExpansion

REM Keep window open on exit
if not defined IN_SUBPROCESS (
    set IN_SUBPROCESS=1
    cmd /k "%~f0" %*
    exit
)

REM ============================================================================
REM CONFIGURATION
REM ============================================================================

REM R2Utilities executable path
set "R2UTILITIES_EXE=C:\RittenhouseRepos\R2Library\src\R2Utilities\bin\Debug\net481\R2Utilities.exe"

REM Include chapter numbers in TOC
set "INCLUDE_CHAPTER_NUMBERS=true"

REM Log directory
set "BASEDIR=%~dp0\.."
set "LOG_DIR=%BASEDIR%\logs"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

REM Generate timestamp
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set "datetime=%%I"
set "TIMESTAMP=%datetime:~0,4%%datetime:~4,2%%datetime:~6,2%_%datetime:~8,2%%datetime:~10,2%%datetime:~12,2%"

set "LOG_FILE=%LOG_DIR%\PostProcess_%TIMESTAMP%.log"

REM Initialize ISBN arguments variable
set "ISBN_ARGS="

REM ============================================================================
REM MAIN
REM ============================================================================

echo. > "%LOG_FILE%"
echo ================================================================================ >> "%LOG_FILE%"
echo R2Utilities Post-Processing ^(License/Database Updates Only^) >> "%LOG_FILE%"
echo ================================================================================ >> "%LOG_FILE%"
echo Started: %date% %time% >> "%LOG_FILE%"
echo. >> "%LOG_FILE%"

echo.
echo ================================================================================
echo              R2Utilities Post-Processing Only
echo ================================================================================
echo.

REM Check if ISBN(s) provided on command line
if "%~1"=="" (
    REM No command-line arguments, check for isbnlist.txt in same directory
    set "SCRIPT_DIR=%~dp0"
    set "DEFAULT_ISBN_LIST=%SCRIPT_DIR%isbnlist.txt"
    
    if exist "!DEFAULT_ISBN_LIST!" (
        echo Using ISBN list from: !DEFAULT_ISBN_LIST!
        echo.
        set "ISBN_ARGS=@!DEFAULT_ISBN_LIST!"
    ) else (
        echo ERROR: No ISBNs provided and isbnlist.txt not found!
        echo.
        echo Usage:
        echo   1. Create isbnlist.txt with one ISBN per line
        echo   2. Place it in the same directory as this script
        echo   3. Run: PostProcessOnly.bat
        echo.
        echo Or specify ISBNs directly:
        echo   PostProcessOnly.bat 9781234567890                    ^(single^)
        echo   PostProcessOnly.bat 9781234567890 9789876543210     ^(multiple^)
        echo   PostProcessOnly.bat @isbnlist.txt                    ^(from file^)
        echo.
        echo Press any key to exit...
        pause >nul
        exit /b 2
    )
) else (
    set "ISBN_ARGS=%*"
)

REM Validate R2Utilities path
if not exist "%R2UTILITIES_EXE%" (
    echo ERROR: R2Utilities not found at: %R2UTILITIES_EXE%
    echo.
    echo Please configure R2UTILITIES_EXE in this script or build R2Library:
    echo   cd C:\RittenhouseRepos\R2Library
    echo   msbuild R2Utilities\R2Utilities.csproj /p:Configuration=Debug
    echo.
    echo Press any key to exit...
    pause >nul
    exit /b 1
)

REM Check if ISBN(s) provided on command line
if "%~1"=="" (
    REM No command-line arguments, check for isbnlist.txt in same directory
    set "SCRIPT_DIR=%~dp0"
    set "DEFAULT_ISBN_LIST=%SCRIPT_DIR%isbnlist.txt"
    
    if exist "!DEFAULT_ISBN_LIST!" (
        echo Using ISBN list from: !DEFAULT_ISBN_LIST!
        echo.
        set "ISBN_ARGS=@!DEFAULT_ISBN_LIST!"
    ) else (
        echo ERROR: No ISBNs provided and isbnlist.txt not found!
        echo.
        echo Usage:
        echo   1. Create isbnlist.txt with one ISBN per line
        echo   2. Place it in the same directory as this script
        echo   3. Run: PostProcessOnly.bat
        echo.
        echo Or specify ISBNs directly:
        echo   PostProcessOnly.bat 9781234567890                    ^(single^)
        echo   PostProcessOnly.bat 9781234567890 9789876543210     ^(multiple^)
        echo   PostProcessOnly.bat @isbnlist.txt                    ^(from file^)
        echo.
        echo Press any key to exit...
        pause >nul
        exit /b 2
    )
) else (
    set "ISBN_ARGS=%*"
)

REM ============================================================================
REM PROCESS ISBNs
REM ============================================================================

set "TOTAL_COUNT=0"
set "SUCCESS_COUNT=0"
set "FAILURE_COUNT=0"

REM Check if it's a file reference (starts with @)
if "!ISBN_ARGS:~0,1!"=="@" (
    REM Extract filename (remove @ prefix)
    set "ISBN_FILE=!ISBN_ARGS:~1!"
    
    if not exist "!ISBN_FILE!" (
        echo ERROR: File not found: !ISBN_FILE!
        exit /b 1
    )
    
    echo Reading ISBNs from file:
    echo.
    
    for /f "usebackq tokens=*" %%L in ("!ISBN_FILE!") do (
        set "LINE=%%L"
        REM Trim leading whitespace using for loop
        for /f "tokens=*" %%T in ("!LINE!") do set "LINE=%%T"
        REM Skip empty lines and comment lines
        if not "!LINE!"=="" if not "!LINE:~0,1!"=="#" (
            call :ProcessISBN "!LINE!"
        )
    )
) else (
    REM Process command-line arguments as ISBNs
    for %%A in (!ISBN_ARGS!) do (
        if not "%%A"=="" (
            call :ProcessISBN "%%A"
        )
    )
)

goto :Summary

REM ============================================================================
REM Subroutine: Process single ISBN
REM ============================================================================
:ProcessISBN
    set "ISBN=%~1"
    set /a TOTAL_COUNT+=1
    
    echo.
    echo [%TOTAL_COUNT%] Processing ISBN: %ISBN%
    echo [%TOTAL_COUNT%] Processing ISBN: %ISBN% >> "%LOG_FILE%"
    
    if "%INCLUDE_CHAPTER_NUMBERS%"=="true" (
        set "FLAGS=-includeChapterNumbersInToc=true"
    ) else (
        set "FLAGS="
    )
    
    echo   Running: R2Utilities.exe -BookLoaderPostProcessingTask -isbn=%ISBN% %FLAGS%
    echo   Running: R2Utilities.exe -BookLoaderPostProcessingTask -isbn=%ISBN% %FLAGS% >> "%LOG_FILE%"
    
    "%R2UTILITIES_EXE%" -BookLoaderPostProcessingTask -isbn=%ISBN% %FLAGS%
    
    if errorlevel 1 (
        echo   [FAILED] Exit code: %ERRORLEVEL%
        echo   [FAILED] Exit code: %ERRORLEVEL% >> "%LOG_FILE%"
        set /a FAILURE_COUNT+=1
    ) else (
        echo   [SUCCESS]
        echo   [SUCCESS] >> "%LOG_FILE%"
        set /a SUCCESS_COUNT+=1
    )
    
    goto :EOF

REM ============================================================================
REM Summary
REM ============================================================================
:Summary

echo.
echo ================================================================================
echo                            SUMMARY
echo ================================================================================
echo Total ISBNs:    %TOTAL_COUNT%
echo Successful:     %SUCCESS_COUNT%
echo Failed:         %FAILURE_COUNT%
echo Log File:       %LOG_FILE%
echo ================================================================================
echo.

echo. >> "%LOG_FILE%"
echo ================================================================================ >> "%LOG_FILE%"
echo SUMMARY >> "%LOG_FILE%"
echo ================================================================================ >> "%LOG_FILE%"
echo Total ISBNs:    %TOTAL_COUNT% >> "%LOG_FILE%"
echo Successful:     %SUCCESS_COUNT% >> "%LOG_FILE%"
echo Failed:         %FAILURE_COUNT% >> "%LOG_FILE%"
echo Completed:      %date% %time% >> "%LOG_FILE%"
echo ================================================================================ >> "%LOG_FILE%"

if %FAILURE_COUNT% EQU 0 (
    echo Post-processing completed successfully - all ISBNs processed
) else (
    echo WARNING: Some post-processing operations failed
)

echo.
echo Press any key to exit...
pause >nul
