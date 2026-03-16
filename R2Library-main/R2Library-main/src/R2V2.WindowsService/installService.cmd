REM - PLEASE LIST IN ALPHABETIC ORDER

REM - THIS IS FOR DEV ONLY - IN ALL ENVIRONMENT, SET "start= delay-auto"

REM Creating R2V2.AnalyticsService Windows Service then rename to "R2V2.AnalyticsService"
sc create R2V2.AutomatedCartService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.AutomatedCartService" DisplayName= "R2V2.AutomatedCartService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.AutomatedCartService "R2 Library Version 2 - Automated Cart Service"
pause

REM Creating R2V2.AnalyticsService Windows Service then rename to "R2V2.AnalyticsService"
sc create R2V2.AnalyticsService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.AnalyticsService" DisplayName= "R2V2.AnalyticsService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.AnalyticsService "R2 Library Version 2 - Analytics Service"
pause

REM Creating R2V2.EmailMessageService Windows Service then rename to "RR2V2.EmailMessageService"
sc create R2V2.EmailMessageService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.EmailMessageService" DisplayName= "R2V2.EmailMessageService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.EmailMessageService "R2 Library Version 2 - Email Message Service"
pause

REM Creating R2V2.OngoingPdaService Windows Service then rename to "RR2V2.OngoingPdaService"
sc create R2V2.OngoingPdaService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.OngoingPdaService" DisplayName= "R2V2.OngoingPdaService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.OngoingPdaService "R2 Library Version 2 - Ongoing PDA Service"
pause

REM Creating R2V2.OrderProcessingService Windows Service then rename to "RR2V2.OrderProcessingService"
sc create R2V2.OrderProcessingService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.OrderProcessingService" DisplayName= "R2V2.OrderProcessingService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.OrderProcessingService "R2 Library Version 2 - Order Processing Service"
pause

REM Creating R2V2.PromotionService Windows Service then rename to "RR2V2.PromotionService"
sc create R2V2.PromotionService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.PromotionService" DisplayName= "R2V2.PromotionService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.PromotionService "R2 Library Version 2 - Promotion Service"
pause

REM Creating R2V2.RequestLoggingService Windows Service then rename to "RR2V2.RequestLoggingService"
sc create R2V2.RequestLoggingService binPath= "%~dp0bin\Debug\R2V2.WindowsService.exe -service=R2V2.RequestLoggingService" DisplayName= "R2V2.RequestLoggingService" obj= .\R2LibraryWebUser password= W3bUs3r! start= demand
sc description R2V2.RequestLoggingService "R2 Library Version 2 - Request Logging Service"
pause