@echo off
@setlocal

REM ALWAYS RUN IN E:\R2BookLoader\Job\bin
E:
cd "E:\R2BookLoader\Job\bin"

REM Initialize OLD SCHOOL Java Classpath
set LOCALCLASSPATH=

for %%i in ("..\lib\jakarta\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\jdbc\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\saxon\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\xalan\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\xerces\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\textml\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\javamail\*.jar") do call "lcp.bat" "%%i"
for %%i in ("..\lib\*.jar") do call "lcp.bat" "%%i"

if "%1" == "" goto noArgument
@echo Batch loading ISBN: %1

REM clear directory
del /F /S /Q ..\content\in\*

REM find the specified zip
if exist "E:\R2BookLoader\TestUploadPending\%1.zip" goto zipFound

:notFound
@echo %1.zip zip file not found!
exit /b 1

:zipFound
REM unpack the specified zip
"C:\Program Files\7-Zip\7z.exe" X "E:\R2BookLoader\TestUploadPending\%1.zip"
xcopy /e /i /r /y %1\* ..\content\in
rmdir /s /q %1

@echo Working on:
dir /b /S ..\content\in\book*
REM Use this switch for debugging:
REM -Djavax.net.debug=all
REM too much heap allocation at 6 gigs!! "C:\Program Files\Java\jdk1.8.0_301\bin\java.exe" -Xms6g -Xmx6g -Djava.security.policy=java.ris.policy -Dhttps.protocols=TLSv1.2 -Dhttps.cipherSuites=TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 -cp %LOCALCLASSPATH%;.\RISBackend.jar com.rittenhouse.RIS.Main
if "%2" == "-update" (
    "C:\Program Files\Java\jdk1.8.0_301\bin\java.exe" -Xms1g -Xmx2g -Djdk.xml.entityExpansionLimit=10000 -Djdk.xml.totalEntitySizeLimit=1000000 -Djava.security.policy=java.ris.policy -Dhttps.protocols=TLSv1.2 -Dhttps.cipherSuites=TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 -cp %LOCALCLASSPATH%;.\RISBackend.jar com.rittenhouse.RIS.Main -update
) else (
    "C:\Program Files\Java\jdk1.8.0_301\bin\java.exe" -Xms1g -Xmx2g -Djdk.xml.entityExpansionLimit=10000 -Djdk.xml.totalEntitySizeLimit=1000000 -Djava.security.policy=java.ris.policy -Dhttps.protocols=TLSv1.2 -Dhttps.cipherSuites=TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 -cp %LOCALCLASSPATH%;.\RISBackend.jar com.rittenhouse.RIS.Main
)
REM Check if Java command succeeded
if %ERRORLEVEL% NEQ 0 (
    @echo Java command failed with error code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

@echo Done with: %1
goto end

:noArgument
@echo No ISBN Argument Specified!
exit /b 2

:end
set LOCALCLASSPATH=
set _JAVACMD=

REM ++ Add new post processing script here (only runs on success)
"E:\R2BookLoader\App\R2Utilities.exe" -BookLoaderPostProcessingTask -isbn=%1 -includeChapterNumbersInToc=true

REM Check if R2Utilities command succeeded
if %ERRORLEVEL% NEQ 0 (
   @echo R2Utilities command failed with error code %ERRORLEVEL%
   exit /b %ERRORLEVEL%
)
