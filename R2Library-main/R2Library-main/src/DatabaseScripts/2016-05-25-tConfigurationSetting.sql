CREATE TABLE [dbo].[tConfigurationSetting](
	[iConfigurationSettingId] [int] IDENTITY(1,1) NOT NULL,
	[vchConfiguration] [varchar](255) NULL,
	[vchSetting] [varchar](255) NULL,
	[vchKey] [varchar](255) NULL,
	[vchValue] [varchar](255) NULL,
	[vchInstructions] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[iConfigurationSettingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UC_Configuration_Setting_Key] UNIQUE NONCLUSTERED 
(
	[vchConfiguration] ASC,
	[vchSetting] ASC,
	[vchKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


--drop table [tConfigurationSetting]
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'CollectionManagement', 'AnnualMaintenanceFeeProductId', '1', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'CollectionManagement', 'PrecisionSearchProductId', '2', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'CollectionManagement', 'PrecisionSearchAccountId', '2', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'CollectionManagement', 'PatronDriveAcquisitionMaxViews', '3', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'ContentLocation', '\\technoserv05\R2Library\Content\Production\xml', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'NewContentLocation', '\\technoserv05\R2Library\Content\Production\', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'DtSearchIndexLocation', '\\technoserv05\R2Library\Content\Production\R2HtmlIndex', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'ImageBaseFileLocation', '\\technoserv05\R2Library\Content\Production\images', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'XslLocation', 'F:\Clients\Rittenhouse\R2Library\src\R2V2.Web\_Static\Xsl\', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'DtSearchBinLocation', 'C:\Program Files (x86)\dtSearch Developer\bin', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'DtSearchLogFilePath', '', 'do not set in production, set to C:\DtSearchLog.txt for debugging purposes');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'ImageBaseUrl', 'http://images.r2library.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'BookCoverUrl', 'http://images.r2library.com/book-covers', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'MinTransformDate', '12/31/2020 11:59:59 PM', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'ResourceLockTime', '300', 'in seconds');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'ResourceMinimumListPrice', '25.00', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Content', 'SearchTypeaheadResultLimit', '0', 'Set to 0 to disable search typeahead');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'EmailMessageQueue', '.\private$\r2v2emailmessage', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'OrderProcessingQueue', '.\private$\r2v2orderpending', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'PrecisionSearchQueue', '.\private$\r2v2precisionsearch', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'EnvironmentConnectionString', 'host=10.0.0.45;virtualHost=dev-local;username=r2v2-dev-local;password=Techno2015', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'ProductionConnectionString', 'host=10.0.0.45;virtualHost=dev-local;username=r2v2-dev-local;password=Techno2015', 'Set to EnvironmentConnectionString in dev & prod, in staging, set to production');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'SendErrorDirectoryPath', 'D:\Temp\R2MessageQueueSendErrors', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'RequestLoggingRouteKey', 'dev.Local.RequestData', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'RequestLoggingExchangeName', 'E.Dev.RequestData', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'RequestLoggingQueueName', 'Q.Dev.RequestData', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'ResourceBatchPromotionRouteKey', 'dev.Local.ResourceBatchPromotion', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'ResourceBatchPromotionExchangeName', 'E.Dev.ResourceBatchPromotion', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'ResourceBatchPromotionQueueName', 'Q.Dev.ResourceBatchPromotion', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'OngoingPdaRouteKey', 'dev.Local.OngoingPda', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'OngoingPdaExchangeName', 'E.Dev.OngoingPda', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'MessageQueue', 'OngoingPdaQueueName', 'Q.Dev.OngoingPda', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'DefaultFromAddress', 'customerservice@r2library.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'DefaultFromName', 'R2 Library Customer Service', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'DefaultReplayToAddress', 'customerservice@r2library.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'DefaultReplayToName', 'R2 Library Customer Service', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'BccAllMessages', 'false', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'BccEmailAddresses', 'kenhaberle@technotects.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'SendToCustomers', 'false', 'should only be true in production as we don''t want to send testing emails to customers');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'TestEmailAddresses', 'kenhaberle@technotects.com;kenhaberle@gmail.com', 'if SendToCustomers is false, these TestEmailAddresses will be emailed instead');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'AddEnvironmentPrefixToSubject', 'true', 'should be false in production as we only want to prefix email subjects with the environment in test environments');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'TemplatesDirectory', '..\..\..\..\src\R2V2.Web\_Static\Templates\', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'PdaAddToCartCcEmailAddresses', 'KenHaberle@technotects.com', 'separated by ,');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'OutputPath', 'F:\Temp\R2v2UtilitiesEmails', 'output path where all email message, used for debug and CYA. If empty, files are not written to disk');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Email', 'WebSiteBaseUrl', 'http://r2v2.localhost/', 'base url of the site');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'IndexLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\TabersTermHighlight\Index', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'OutputLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\TabersTermHighlight\Output', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'BackupLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\TabersTermHighlight\Backup', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'BatchSize', '45', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'MaxIndexBatches', '1', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'UpdateResourceStatus', 'false', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'TabersTermHighlight', 'MaxWordCountPerDataCall', '3000', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'IndexLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\IndexTermHighlight\Index', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'OutputLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\IndexTermHighlight\Output', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'BackupLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\IndexTermHighlight\Backup', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'BatchSize', '45', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'MaxIndexBatches', '1', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'UpdateResourceStatus', 'false', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'IndexTermHighlight', 'MaxWordCountPerDataCall', '3000', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'R2ReportsConnection', 'Server=10.0.0.45;Database=DEV_R2Reports;User ID=R2ReportsUser;Password=R2Reports2012;Trusted_Connection=False', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'R2DatabaseConnection', 'Server=10.0.0.45;Database=DEV_RIT001;User ID=R2UtilitiesUser;Password=R2User2012;Trusted_Connection=False', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'R2UtilitiesDatabaseConnection', 'Server=10.0.0.45;Database=DEV_R2Utilities;User ID=R2UtilitiesUser;Password=R2User2012;Trusted_Connection=False', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'TabersDictionaryConnection', 'Server=10.0.0.32;Database=TabersDictionary;User Id=R2WebUser;Password=Web2012;Trusted_Connection=False', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'EnvironmentName', 'DEV-local', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'DefaultFromAddress', 'r2v2@r2library.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'DefaultFromAddressName', 'R2v2 Utilities', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'EmailConfigDirectory', '.\Email\Config', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'HtmlIndexerBatchSize', '1000', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'HtmlIndexerMaxIndexBatches', '1', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'HtmlIndexerForceCompression', 'true', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'HtmlIndexerFragmentationLimit', '5', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ResourceFileInsertBatchSize', '100', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'IndexStoredFields', 'r2Isbn,r2ChapterTitle,r2ChapterId,r2ChapterNumber,r2SectionId,r2SectionTitle,r2Editor,r2BookSubTitle', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'IndexEnumerableFields', 'r2BookStatus,r2BookTitle,r2Author,r2PrimaryAuthor,r2Publisher,r2Library,r2PracticeArea,r2Specialty,r2DrugMonograph,r2CopyrightYear,r2ReleaseDate', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'OrderBatchDescending', 'false', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AuthorTableName', 'tAuthor', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ResourceFileTableName', 'tResourceFile', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'DefaultSpecialtyCode', 'R2D0012', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'DefaultPracticeAreaCode', 'MED', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'BookLoaderSourceRootDirectory', 'D:\ClientsNoBackup\Rittenhouse\R2v2-XMLbyISBN', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'BookLoaderImageDestinationDirectory', '\\technoserv02\c$\Clients\Rittenhouse\R2Library\Content_STG\images', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ResourceValidationBaseUrl', 'http://dev-local.r2library.com/Resource/Title/', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'EmailTestMode', 'true', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'EmailTestModeEmailAddress', 'kenhaberle@technotects.com', 'Only accepts 1 email address');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'EmailTestNumberOfEmails', '3', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'PreludeDataLinkedServer', '', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'R2UtilitiesDatabaseName', 'DEV_R2Utilities', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'PdaAddedToCartNumberOfDaysAgo', '32', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'PdaRemovedFromCartNumberOfDays', '30', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'R2ReportsDatabaseName', 'Dev_R2Reports', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'R2DatabaseName', 'Dev_RIT001', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'TabersXmlPath', 'D:\TabersXml\Xml', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AggregateDailyCountFolder', 'F:\Clients\Rittenhouse\Temp\R2Reports', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AggregateDailyCountFolderZipLocation', '\\technoserv05\F-Drive\Backups\R2Reports', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AggregateDailyCountMonthsToGoBack', '8', '15,14,13,12,11,10,9,8,7,');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AggregateDailyCommandTimeout', '300', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AutoLicensesNumberOfLicenses', '15', 'default to 15 in staging for SCT Labs');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'GeoIPsApiKey', 'e95cd1c6f3973cda1724c2f9f4be00fa', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'SpecialIconBaseUrl', 'http://dev-images.r2library.com/special-icons/', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'Ip2LocationConnection', 'Server=10.0.0.32;Database=ip2location;User ID=R2UtilitiesUser;Password=R2User2012;Trusted_Connection=False', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'Ip2LocationWorkingFolder', 'E:\Ip2Location\Working\', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'Ip2LocationTableName', 'ip2location_db1', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'Ip2LocationDatabaseName', 'ip2location', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AutoLockUser', '180', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AutoLockExpertReviwer', '180', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AutoLockInstitutionAdmin', '18000', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AutoLockSalesAdmin', '730', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AutoLockRittenhouseAdmin', '730', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'CmsHtmlContentUrl', 'http://dev01.rittenhouse.com/Rbd/Web/R2LibraryContentService.asmx/GetHtmlContent', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'OverRideDashboardEmailQuickNotes', 'True', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'UpdateInstitutionStatisticsPreviousDays', '0', 'If set to 0 no update will be done.');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'DctUpdateEmailStartDaysAgo', '30', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'MaxActivityReportInstitutionDisplay', '5', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ContentBackupDirectory', '\\technoserv05\g$\R2Library\ContentBackup\Production', 'this value is used for both backup and restore tasks');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ContentBackupFtpServerHost', 'ftp.rittenhouse.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ContentBackupFtpServerUsername', 'R2LibraryContentbackup', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ContentBackupFtpServerPassword', 'hB0Oufcayi89kb1YDUVZ', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'ContentBackupFtpServerDirectory', '/Production/', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'TaskCompressionTempDirectory', 'D:\Temp\R2UtilitiesTaskEmailAttachments\', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AuditFilesOnDiskBackupDirectory', '\\technoserv05\g$\R2Library\Content\Production\Backups\AuditXmlContentFilesOnDiskTask\', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'R2Utilities', 'AuditXmlDateModifiedWindowInSeconds', '300', null);



--WindowsService
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Image', 'BaseUrl', 'http://images.r2library.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Image', 'LocalBookCoverLocation', 'D:\Clients\R2V2\images\book-covers', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Image', 'BookCoverLocation', 'http://images.r2library.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'R2ReportsConnectionString', 'Server=10.0.0.45;Database=DEV_R2Reports;          User ID=R2ReportsUser;  Password=R2Reports2012;Trusted_Connection=False', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'RIT001ProductionConnectionString', 'Server=10.0.0.45;Database=DEV_PROMOTE_RIT001;     User ID=R2WebUser;      Password=Web2012;      Trusted_Connection=False', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'R2UtilitiesProductionConnectionString', 'Server=10.0.0.45;Database=DEV_PROMOTE_R2Utilities;User ID=R2UtilitiesUser;Password=R2User2012;   Trusted_Connection=False', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'RIT001StagingConnectionString', 'Server=10.0.0.45;Database=DEV_RIT001;             User ID=R2WebUser;      Password=Web2012;      Trusted_Connection=False', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteXmlSourceDirectory', '\\technoserv05\R2Library\Content\Production\xml\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteXmlDestinationDirectory', '\\technoserv05\R2Library\Content\PromotionDestination\xml\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteHtmlSourceDirectory', '\\technoserv05\R2Library\Content\Production\html\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteHtmlDestinationDirectory', '\\technoserv05\R2Library\Content\PromotionDestination\html\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteImagesSourceDirectory', '\\technoserv05\R2Library\Content\Production\images\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteImagesDestinationDirectory', '\\technoserv05\R2Library\Content\PromotionDestination\images\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteCoverImageSourceDirectory', '\\technoserv05\R2Library\Content\Production\images\book-covers\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteCoverImageDestinationDirectory', '\\technoserv05\R2Library\Content\PromotionDestination\images\book-covers\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteMediaFilesSourceDirectory', '\\technoserv05\R2Library\Content\Production\media\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteMediaFilesDestinationDirectory', '\\technoserv05\R2Library\Content\PromotionDestination\media\', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteStatusEmailToAddresses', 'KenHaberle@technotects.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteFromEmailAddress', 'r2-support@technotects.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteFromDisplayName', 'R2v2 Promotion', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteSqlScriptFile', 'C:\Clients\Rittenhouse\r2library\trunk\src\R2V2.WindowsService\Threads\Promotion\Promote.sql', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteStagingDomain', 'stage.r2library.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteProductionDomain', 'dev.r2library.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'PromoteAutoLicenseCount', '3', 'automatically add 3 licenses for promoted titles to the Rittenhouse account' );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayShipToNumber', 'D', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayBlindShipFlag', 'N', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayFollettCorpFlag', 'N', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayResidentFlag', 'N', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayOrderType', '06', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayShipVia', 'R2', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayWrittenBy', 'WEB', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayAcknowledgeFlag', 'E', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayAdminHoldFlag', 'Y', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayAdminHoldManager', 'R2', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayLogDirectory', 'D:\Temp\R2Orders', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayOrderFileNamePrefix', 'dev-local_', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelaySendOrderFileToPrelude', 'false', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelaySFtpHost', '192.9.200.107', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelaySFtpUsername', 'rittweb', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelaySFtpPassword', 'techn0tec3h', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayInternalOrderEmailToAddresses', 'KenHaberle@technotects.com', null );
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WindowsService', 'OrderRelayInternalOrderEmailFromAddress', 'r2v2@technotects.net', null );


INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'DisplayAdminTab', 'true', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'PurchaseConfirmationEmail', 'KenHaberle@technotects.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'ContactUsEmail', 'KenHaberle@technotects.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'TrialInitializeEmail', 'KenHaberle@technotects.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'MarcRecordWebsite', 'http://marcrecords.r2library.com/MarcRecords/EBookDownload', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'NewAccountNotificationEmail', 'marketing@rittenhouse.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'QaApprovalEmailTo', 'KenHaberle@technotects.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'QaApprovalEmailCc', 'kenhaberle@gmail.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'AlertImageLocation', 'http://dev-images.r2library.com/alerts', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Admin', 'AlertImagePhysicalLocation', '\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev\images\alerts', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Cache', 'DefaultExpirationInHours', '24', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'GoogleAnalyticsAccount', 'UA-9995845-3', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'DisableRightClick', 'false', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'BrowsePageSize', '10', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'ResourceTimeoutInMinutes', '60', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'ResourceTimeoutModalInSeconds', '30', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'DoodyReviewLink', 'http://www.doody.com/ws/xdbrs/hostedreview.asp?ISBN=', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'R2CmsContentLink', 'http://dev01.rittenhouse.com/Rbd/Web/R2LibraryContentService.asmx/GetPageContent', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'CarouselAutoplaySpeedInSeconds', '10', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'HomePageCarouselUrl', 'http://dev01.rittenhouse.com/Rbd/Web/R2LibraryContentService.asmx/GetCarouselContent', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'CmsHtmlContentUrl', 'http://dev01.rittenhouse.com/Rbd/Web/R2LibraryContentService.asmx/GetHtmlContent', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'CmsTimeout', '5000', 'Timeout Period for CMS request (5000 = 5 seconds)');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'RecaptchaPrivateKey', '6Ld9Z_MSAAAAAEJsv2YJ56rCWDJgwu2VX2V-Q0RP ', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'RecaptchaPublicKey', '6Ld9Z_MSAAAAAGkk61OvdWqoSbZlGzK7_GjTNJlo', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'RecaptchaRequestTimeInSeconds', '120', 'Time in Milliseconds');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'RecaptchaNumberOfRequests', '50', '# of requests in within the TimeInMs will trigger recaptcha');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'AutoLockUser', '180', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'AutoLockExpertReviewer', '180', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'AutoLockInstitutionAdmin', '180', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'AutoLockSalesAdmin', '730', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'AutoLockRittenhouseAdmin', '730', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'RittenhouseBaseUrl', 'http://dev01.rittenhouse.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'FlowplayerKey', '$289122895653393', 'Production: $383706912694557');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'OoyalaKey', '4bfefee502a34b829ec235555e7c4215', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Client', 'MediaBaseUrl', 'http://dev-media.r2library.com', 'DO NOT CHANGE WITHOUT TALKING TO ME! SJS - 8/26/2013 - EVEN IN DEVELOPMENT ON YOUR LOCAL MACHINE!!!!');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Institution', 'GuestAccountNumber', '999999', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Institution', 'LocalLogoLocation', '\\TECHNOSERV02\R2Library\Content\images\logos\', 'DO NOT CHANGE WITHOUT TALKING TO ME! SJS - 10/1/2012');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Institution', 'LogoLocation', 'http://dev-images.r2library.com/logos/', 'DO NOT CHANGE WITHOUT TALKING TO ME! SJS - 10/1/2012');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Institution', 'MinimumResourceCountForPaging', '50', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'BookCoverDirectory', '\\TECHNOSERV02\R2Library\Content\images\book-covers', 'DO NOT CHANGE WITHOUT TALKING TO ME! SJS - 10/1/2012');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'PublisherImageUrl', 'http://dev-images.r2library.com/publishers/', 'DO NOT CHANGE WITHOUT TALKING TO ME! SJS - 10/1/2012');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'PublisherImageDirectory', '\\TECHNOSERV02\R2Library\Content\images\publishers\', 'DO NOT CHANGE WITHOUT TALKING TO ME! SJS - 10/1/2012');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'BookCoverMaxWidth', '400', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'BookCoverMaxHeight', '600', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'BookCoverMaxSizeInKb', '100', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'SpecialIconDirectory', '\\TECHNOSERV02\R2Library\Content\images\special-icons', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'WebImage', 'SpecialIconBaseUrl', 'http://dev-images.r2library.com/special-icons/', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'EnablePromotionToProduction', 'true', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'DisplayOngoingPdaEventLinks', 'false', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'DisplayPromotionFields', 'true', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'EnvironmentName', 'Trunk', 'DO NOT SET IN PRODUCTION');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ExternalSearchPubMedUrl', 'http://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=pubmed&amp;term={0}', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ExternalSearchMeshUrl', 'http://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=mesh&amp;term={0}', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ExternalSearchTimeoutInMilliseconds', '1500', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'EnablePatronDrivenAcquisitions', 'true', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'IpSecurityCacheTimeToLiveInMinutes', '30', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourceMinimumPromotionPrice', '50.00', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintLimitMax', '6', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintLimitMin', '1', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintLimitPercentage', '10', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintLockDurationInHours', '24', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintCheckPeriodInHours', '24', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintWarningPercentage', '75', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ResourcePrintAlertBcc', 'scott@thescheiders.com', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'Web', 'ServerNumber', '11', 'tinyint in db, use value from 0 - 255, 4=rittweb4, 5=rittweb5, 6=rittweb6, 10=rittstg, 11=local, 12=technoserv02');		
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'App', 'BaseUrl', 'http://r2library.comm', null);
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])VALUES('dev', 'App', 'ShellPath', 'D:\SCTTesting\shell.html', null);		
