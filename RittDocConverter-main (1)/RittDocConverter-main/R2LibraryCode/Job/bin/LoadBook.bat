@echo off
setlocal EnableExtensions EnableDelayedExpansion

cd /d "E:\R2BookLoader\Job\bin"

if "%~1"=="" (
  echo No ISBN Argument Specified!
  exit /b 2
)

set "ISBN=%~1"
set "MODE=%~2"
echo Batch loading ISBN: %ISBN%

set LOCALCLASSPATH=
for %%i in ("..\lib\jakarta\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\jdbc\*.jar")   do call "lcp.bat" "%%i"
for %%i in ("..\lib\saxon\*.jar")  do call "lcp.bat" "%%i"
for %%i in ("..\lib\xalan\*.jar")  do call "lcp.bat" "%%i"
for %%i in ("..\lib\xerces\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\textml\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\javamail\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\*.jar")        do call "lcp.bat" "%%i"

set "DEST=E:\R2BookLoader\Job\content\in"
if not exist "%DEST%" mkdir "%DEST%" >nul 2>&1

del /F /S /Q "%DEST%\*" >nul 2>&1

set "ZIP=E:\R2BookLoader\TestUploadPending\%ISBN%.zip"
if not exist "%ZIP%" (
  echo %ISBN%.zip zip file not found at %ZIP% !
  exit /b 1
)

set "WORK=%TEMP%\isbnwork_%ISBN%_%RANDOM%%RANDOM%"
mkdir "%WORK%" >nul 2>&1
echo WORK=%WORK%
echo ZIP=%ZIP%
echo DEST=%DEST%

REM 7z: -y assume Yes, -aoa overwrite all, -o output dir
"C:\Program Files\7-Zip\7z.exe" x "%ZIP%" -y -aoa -o"%WORK%"
if errorlevel 1 (
  echo 7-Zip failed with error code %ERRORLEVEL%
  rmdir /s /q "%WORK%" >nul 2>&1
  exit /b %ERRORLEVEL%
)

echo Running robocopy...
robocopy "%WORK%" "%DEST%" /E /R:1 /W:1
set "RC=%ERRORLEVEL%"
echo robocopy exit code=%RC%

REM 0-7 are OK, 8+ are failures
if %RC% GEQ 8 (
  echo robocopy FAILED. ExitCode=%RC%
  rmdir /s /q "%WORK%" >nul 2>&1
  exit /b %RC%
)

rmdir /s /q "%WORK%" >nul 2>&1

if /I "%MODE%"=="-update" (
  "C:\Program Files\Java\jdk1.8.0_301\bin\java.exe" -Xms1g -Xmx2g -Djdk.xml.entityExpansionLimit=10000 -Djdk.xml.totalEntitySizeLimit=1000000 -Djava.security.policy=java.ris.policy -Dhttps.protocols=TLSv1.2 -Dhttps.cipherSuites=TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 -cp %LOCALCLASSPATH%;.\RISBackend.jar com.rittenhouse.RIS.Main -update
) else (
  "C:\Program Files\Java\jdk1.8.0_301\bin\java.exe" -Xms1g -Xmx2g -Djdk.xml.entityExpansionLimit=10000 -Djdk.xml.totalEntitySizeLimit=1000000 -Djava.security.policy=java.ris.policy -Dhttps.protocols=TLSv1.2 -Dhttps.cipherSuites=TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 -cp %LOCALCLASSPATH%;.\RISBackend.jar com.rittenhouse.RIS.Main
)

if errorlevel 1 (
  echo Java command failed with error code %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)

set "XMLSRC=E:\R2v2-XMLbyISBN\%ISBN%\xml"
if not exist "%XMLSRC%\" (
  echo ERROR: Expected XML folder missing: "%XMLSRC%"
  exit /b 50
)

"E:\R2BookLoader\App\R2Utilities.exe" -BookLoaderPostProcessingTask -isbn=%ISBN% -includeChapterNumbersInToc=true
if errorlevel 1 (
  echo R2Utilities command failed with error code %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)

echo Done with: %ISBN%
exit /b 0
