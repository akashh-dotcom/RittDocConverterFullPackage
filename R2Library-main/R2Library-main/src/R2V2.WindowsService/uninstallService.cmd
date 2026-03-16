::REM Start - This is the old code

::rem "C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" /u ".\bin\Debug\R2V2.WindowsService.exe"
::"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe" /u ".\bin\Debug\R2V2.WindowsService.exe"
::pause

::REM End - This is the old code



rem sc delete R2V2.EmailMessageService
sc delete R2V2.EmailMessageService
pause

rem sc delete R2V2.OngoingPdaService
sc delete R2V2.OngoingPdaService
pause

rem sc delete R2V2.OrderProcessingService
sc delete R2V2.OrderProcessingService
pause

rem sc delete R2V2.PrecisionSearchService
rem sc delete R2V2.PrecisionSearchService
rem pause

rem sc delete R2V2.PromotionService
sc delete R2V2.PromotionService
pause

rem sc delete R2V2.RequestLoggingService
sc delete R2V2.RequestLoggingService
pause

rem sc delete R2V2.AnalyticsService
sc delete R2V2.AnalyticsService
pause


