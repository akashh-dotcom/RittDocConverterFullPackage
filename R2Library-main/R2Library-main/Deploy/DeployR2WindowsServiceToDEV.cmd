echo off
title R2 Windows Service DEPLOY

echo -------------------------------------------------------------
echo -- DEPLOYING R2 WINDOWS SERVICE TO TECHNOSERV02            --
echo -- 1  - Uses msbuild.exe to build service                  --
echo -- 2  - Stop R2V2.AnalyticsService on technoserv02         --
echo -- 3  - Stop R2V2.EmailMessageService on technoserv02      --
echo -- 4  - Stop R2V2.OrderProcessingService on technoserv02   --
echo -- 5  - Stop R2V2.RequestLoggingService on technoserv02    --
echo -- 6  - Copies serivce to technoserv02                     --
echo -- 7  - Start R2V2.AnalyticsService on technoserv02        --
echo -- 8  - Start R2V2.EmailMessageService on technoserv02     --
echo -- 9  - Start R2V2.OrderProcessingService on technoserv02  --
echo -- 10 - Start R2V2.RequestLoggingService on technoserv02   --
echo -- 11 - Creates deployment package                         --
echo -------------------------------------------------------------
echo.
pause

rem set attributes=/K /R /D /Y /S /L
set attributes=/K /R /D /Y /S

rem buildDir is the location of the localDir from the R2Utilities.csproj
set buildDir=..\..\Deploy\R2V2.WindowsService
set localDir=.\R2V2.WindowsService
set destDir=\\technoserv02\d$\Clients\Rittenhouse\R2Library\WindowsService\Service

echo.
echo Destination: %destDir%
echo.
pause

rem set attributes=/K /R /D /Y /S /L
set attributes=/K /R /D /Y /S

rmdir %localDir% /s /q

echo.
echo ------------------------------
echo -- Building R2Utilities App --
echo ------------------------------
rem "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" "..\src\R2V2.WindowsService\R2V2.WindowsService.csproj" /p:OutDir=%buildDir% /t:Clean,Build 
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" "..\src\R2V2.WindowsService\R2V2.WindowsService.csproj" /p:OutDir=%buildDir% /t:Clean,Build 
echo.
echo.
echo R2V2.WindowsService has been copied locally, please press to stop the services
pause

REM sc \\technoserv02 stop R2V2.AnalyticsService
REM sc \\technoserv02 stop R2V2.EmailMessageService
REM rem sc \\technoserv02 stop R2V2.OngoingPdaService
REM sc \\technoserv02 stop R2V2.OrderProcessingService
REM rem sc \\technoserv02 stop R2V2.PromotionService
REM sc \\technoserv02 stop R2V2.RequestLoggingService
REM sc \\technoserv02 stop R2V2.AutomatedCartService
REM sc \\technoserv02 stop R2V2.RabbitMqEmailService

REM echo.
REM echo.
REM echo "R2v2 Windows Services" has been stopped on technoserv02, please press any key to deploy this code to DEV 
REM echo Please wait about a minute to make sure the service has stopped.
REM pause

REM xcopy "%localDir%\*" "%destDir%\" %attributes% /exclude:DeployR2WindowsServiceExcludes.txt

REM echo.
REM echo.
REM echo The code has been copied to technoserv02, please press any key to start the services 
REM pause

REM sc \\technoserv02 start R2V2.AnalyticsService
REM sc \\technoserv02 start R2V2.EmailMessageService
REM rem sc \\technoserv02 start R2V2.OngoingPdaService
REM sc \\technoserv02 start R2V2.OrderProcessingService
REM rem sc \\technoserv02 start R2V2.PromotionService
REM sc \\technoserv02 start R2V2.RequestLoggingService
REM sc \\technoserv02 start R2V2.AutomatedCartService
REM sc \\technoserv02 start R2V2.RabbitMqEmailService
rem --------------------------------------------------
rem create 7zip file - start
@echo off
For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set currentDate=%%c-%%a-%%b)
echo Current Date: %currentDate%

echo.
set /p deleteAspx= Create deployment 7zip (Y/N)?
if /I "%deleteAspx%" == "Y" set /p zipSuffix= Enter unique 7zip file name suffix (a thru z)?
if /I "%deleteAspx%" == "Y" "C:\Program Files\7-Zip\7z.exe" a -t7z R2V2.WindowsService-%currentDate%%zipSuffix%.7z %localDir%
rem create 7zip file - end
rem --------------------------------------------------

pause
