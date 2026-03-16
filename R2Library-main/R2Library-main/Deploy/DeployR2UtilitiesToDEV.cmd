echo off
title R2Utilities BUILD Deployment Package 

echo -------------------------------------------------------------
echo -- BUILDING R2Utilities                                    --
echo -- 1 - Uses msbuild.exe to build application               --
REM echo -- 2 - Copies application to technoserv02 and technoserv05 --
echo -- 3 - Creates deployment package                          --
echo -------------------------------------------------------------
echo.
pause
rem set attributes=/K /R /D /Y /S /L
set attributes=/K /R /D /Y /S

rem buildDir is the location of the localDir from the R2Utilities.csproj
set buildDir=..\..\Deploy\R2Utilities
set localDir=.\R2Utilities

REM set destDir1=\\technoserv02\d$\Clients\Rittenhouse\R2Library\Utilities\App
REM set destDir2=\\technoserv02\d$\Clients\Rittenhouse\R2Library\Utilities\App2
REM set destDir3=\\technoserv05\Utilities\App
REM set destDir4=\\technoserv05\Utilities\App2

echo.
REM echo Destination: %destDir1%
REM echo Destination: %destDir2%
REM echo Destination: %destDir3%
REM echo Destination: %destDir4%
REM echo.
REM echo Destination: NONE!!!
echo.
REM pause

rem set attributes=/K /R /D /Y /S /L
set attributes=/K /R /D /Y /S

rmdir %localDir% /s /q

echo.
echo ------------------------------
echo -- Building R2Utilities App --
echo ------------------------------
rem "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" "..\src\R2Utilities\R2Utilities.csproj" /p:OutDir=%buildDir% /t:Clean,Build 
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" "..\src\R2Utilities\R2Utilities.csproj" /p:OutDir=%buildDir% /t:Clean,Build 
xcopy "..\src\R2V2.Web\_Static\Templates\*" "%localDir%\EmailTemplates\" /S
xcopy "..\src\R2V2.Web\_Static\Xsl\*" "%localDir%\Xsl\" /S


echo.
echo.
echo -------------------------------------------------------------------------
echo --                 R2Utilities has been built                          --
echo -------------------------------------------------------------------------
pause

REM echo ------------------------------------------------------------------
REM echo -- Copying to technoserv02 App
REM echo ------------------------------------------------------------------
REM xcopy "%localDir%\*" "%destDir1%\" %attributes% /exclude:DeployR2UtilitiesExcludes.txt
REM xcopy "%localDir%\Email\Config\*" "%destDir1%\Email\Config\" /K /D /-Y

REM echo.
REM echo ------------------------------------------------------------------
REM echo -- Copying to technoserv02 App2
REM echo ------------------------------------------------------------------
REM xcopy "%localDir%\*" "%destDir2%\" %attributes% /exclude:DeployR2UtilitiesExcludes.txt
REM xcopy "%localDir%\Email\Config\*" "%destDir2%\Email\Config\" /K /D /-Y

REM echo.
REM echo ------------------------------------------------------------------
REM echo -- Copying to technoserv05 App
REM echo ------------------------------------------------------------------
REM xcopy "%localDir%\*" "%destDir3%\" %attributes% /exclude:DeployR2UtilitiesExcludes.txt
REM xcopy "%localDir%\Email\Config\*" "%destDir3%\Email\Config\" /K /D /-Y
REM echo.

REM echo.
REM echo ------------------------------------------------------------------
REM echo -- Copying to technoserv05 App2
REM echo ------------------------------------------------------------------
REM xcopy "%localDir%\*" "%destDir4%\" %attributes% /exclude:DeployR2UtilitiesExcludes.txt
REM xcopy "%localDir%\Email\Config\*" "%destDir4%\Email\Config\" /K /D /-Y
REM echo.

rem --------------------------------------------------
rem create 7zip file - start
@echo off
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set currentDate=%%c-%%a-%%b)
echo Current Date: %currentDate%

echo.
REM set /p deleteAspx= Create deployment 7zip (Y/N)?
REM if /I "%deleteAspx%" == "Y" set /p zipSuffix= Enter unique 7zip file name suffix (a thru z)?
REM if /I "%deleteAspx%" == "Y" "C:\Program Files\7-Zip\7z.exe" a -t7z R2Utilities-%currentDate%%zipSuffix%.7z %localDir%
rem create 7zip file - end
rem --------------------------------------------------
set /p zipSuffix= Enter unique 7zip file name suffix (a thru z)?
"C:\Program Files\7-Zip\7z.exe" a -t7z R2Utilities-%currentDate%%zipSuffix%.7z %localDir%

echo BUILD Deployment Package COMPLETE!!!
pause




