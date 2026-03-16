echo off
echo -------------------------------------------------
echo -- DEPLOYING DEV.R2LIBRARY.COM ON TECHNOSERV02 --
echo -- 1 - Uses msbuild.exe to publish site        --
echo -- 2 - Copies site to technoserv02             --
echo -- 3 - Creates deployment package              --
echo -------------------------------------------------
echo.

set sourceDir=.\Athens
set destDir=\\technoserv02\d$\Clients\Rittenhouse\R2Library\Site\oa

echo Destination: %destDir%
echo.

rem set attributes=/K /R /D /Y /S /L
set attributes=/K /R /D /Y /S

pause

rmdir %sourceDir% /s /q

echo.
echo ----------------------
echo -- Building Web App --
echo ----------------------
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" "..\src\Athens\SPExample\SPExample.sln" /p:DeployOnBuild=true /p:PublishProfile=FolderProfile
pause

echo.
echo -----------------------
echo -- Stopping app pool --
echo -----------------------
.\psexec.exe \\technoserv02 C:\Windows\System32\inetsrv\appcmd.exe stop apppool /apppool.name:"Athens 32"

echo.
echo --------------------
echo -- Deploying Code --
echo --------------------
echo.
set /p deleteAspx= Do you want to delete all .aspx files from the destinations first (Y/N)?
if /I "%deleteAspx%" == "Y" del /S "%destDir%\*.aspx"
if /I "%deleteAspx%" == "Y" del /S "%destDir%\Account\*.aspx"

echo.

echo copying "%sourcedir%\*"
xcopy "%sourcedir%\*" "%destdir%\" %attributes% /exclude:DeployExcludes.txt

echo.
echo -----------------------
echo -- Starting app pool --
echo -----------------------
.\psexec.exe \\technoserv02 C:\Windows\System32\inetsrv\appcmd.exe start apppool /apppool.name:dev.r2library.com

rem --------------------------------------------------
rem create 7zip file - start
@echo off
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set currentDate=%%c-%%a-%%b)
echo Current Date: %currentDate%

echo.
echo -------------------------------
echo -- Create Deployment Package --
echo -------------------------------
echo.
set /p deleteAspx= Create deployment 7zip (Y/N)?
if /I "%deleteAspx%" == "Y" set /p zipSuffix= Enter unique 7zip file name suffix (a thru z)?
if /I "%deleteAspx%" == "Y" "C:\Program Files\7-Zip\7z.exe" a -t7z -mmt=on -mx7 R2v2.Athens-%currentDate%%zipSuffix%.7z %sourceDir%
rem create 7zip file - end
rem --------------------------------------------------

pause