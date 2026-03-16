echo off
echo -------------------------------------------------
echo -- DEPLOYING DEV.R2LIBRARY.COM ON TECHNOSERV02 --
echo -- 1 - Uses msbuild.exe to publish site        --
echo -- 2 - Copies site to technoserv02             --
echo -- 3 - Creates deployment package              --
echo -------------------------------------------------
echo.
set sourceDir=.\Web
set destDir=\\technoserv02\d$\Clients\Rittenhouse\R2Library\Site\dev.r2library.com

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
rem "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\msbuild.exe" "..\src\R2V2.sln" /p:DeployOnBuild=true /p:PublishProfile=Profile1
rem "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" "..\src\R2V2.sln" /p:DeployOnBuild=true /p:PublishProfile=Profile1
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" "..\src\R2V2.sln" /p:DeployOnBuild=true /p:PublishProfile=Profile1
pause

rem echo.
rem echo -----------------------
rem echo -- Stopping app pool --
rem echo -----------------------
rem .\psexec.exe \\technoserv02 C:\Windows\System32\inetsrv\appcmd.exe stop apppool /apppool.name:dev.r2library.com

rem echo.
rem echo --------------------
rem echo -- Deploying Code --
rem echo --------------------
rem echo.
rem set /p deleteAspx= Do you want to delete all .cshtml files from the destinations first (Y/N)?
rem if /I "%deleteAspx%" == "Y" del /S "%destDir%\Views\*.cshtml"
rem if /I "%deleteAspx%" == "Y" del /S "%destDir%\Areas\Admin\Views\*.cshtml"
rem 
rem echo.
rem 
rem echo copying "%sourcedir%\*"
rem xcopy "%sourcedir%\*" "%destdir%\" %attributes% /exclude:DeployExcludes.txt
rem 
rem echo.
rem echo -----------------------
rem echo -- Starting app pool --
rem echo -----------------------
rem .\psexec.exe \\technoserv02 C:\Windows\System32\inetsrv\appcmd.exe start apppool /apppool.name:dev.r2library.com

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
rem set /p deleteAspx= Create deployment 7zip (Y/N)?
set /p zipSuffix= Enter unique 7zip file name suffix (a thru z)?
"C:\Program Files\7-Zip\7z.exe" a -t7z -mmt=on -mx7 R2v2.Web-%currentDate%%zipSuffix%.7z %sourceDir%
rem create 7zip file - end
rem --------------------------------------------------

pause