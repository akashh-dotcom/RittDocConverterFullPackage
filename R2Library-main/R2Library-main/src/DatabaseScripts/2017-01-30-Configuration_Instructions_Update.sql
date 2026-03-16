Update tConfigurationSetting set vchInstructions = 'string value - the emails of people who will be BCCed on print lock outs. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Web' and vchKey = 'ResourcePrintAlertBcc';

Update tConfigurationSetting set vchInstructions = 'int value - percentage of when to warn of print lock outs.'
where vchSetting = 'Web' and vchKey = 'ResourcePrintWarningPercentage';

Update tConfigurationSetting set vchInstructions = 'int value - number of hours of to go back in convent views to determine when resource becomes unlocked for an institution.'
where vchSetting = 'Web' and vchKey = 'ResourcePrintCheckPeriodInHours';

Update tConfigurationSetting set vchInstructions = 'int value - duration of the print lock out for an institution of a specific resource.'
where vchSetting = 'Web' and vchKey = 'ResourcePrintLockDurationInHours';

Update tConfigurationSetting set vchInstructions = 'int value - maximum percentage of a single title prints allowed.'
where vchSetting = 'Web' and vchKey = 'ResourcePrintLimitPercentage';

Update tConfigurationSetting set vchInstructions = 'int value - the minimum maximum value of single title prints allowed.'
where vchSetting = 'Web' and vchKey = 'ResourcePrintLimitMin';

Update tConfigurationSetting set vchInstructions = 'int value - the maximum value of single title prints allowed.'
where vchSetting = 'Web' and vchKey = 'ResourcePrintLimitMax';

Update tConfigurationSetting set vchInstructions = 'decimal value - the miniumum price for resouce in order to be promoted.'
where vchSetting = 'Web' and vchKey = 'ResourceMinimumPromotionPrice';

Update tConfigurationSetting set vchInstructions = 'int value - determines how long white listed ips, black listed ips, and black listed countries are cached.'
where vchSetting = 'Web' and vchKey = 'IpSecurityCacheTimeToLiveInMinutes';

Update tConfigurationSetting set vchInstructions = 'boolean value - controls weather PDA is availale for use or not.'
where vchSetting = 'Web' and vchKey = 'EnablePatronDrivenAcquisitions';

Update tConfigurationSetting set vchInstructions = 'int value - timeout peroid for all external searchs (MESH and PubMed).'
where vchSetting = 'Web' and vchKey = 'ExternalSearchTimeoutInMilliseconds';

Update tConfigurationSetting set vchInstructions = 'string value - URL used to search MESH for external links.'
where vchSetting = 'Web' and vchKey = 'ExternalSearchMeshUrl';

Update tConfigurationSetting set vchInstructions = 'string value - URL used to search PubMed for external links.'
where vchSetting = 'Web' and vchKey = 'ExternalSearchPubMedUrl';

Update tConfigurationSetting set vchInstructions = 'string value - environment name displayed in the top left of each webpage. DO NOT SET in production.'
where vchSetting = 'Web' and vchKey = 'EnvironmentName';

Update tConfigurationSetting set vchInstructions = 'boolean value - display the Ongoing PDA events table on the Resource Detail page in the admin area.'
where vchSetting = 'Web' and vchKey = 'DisplayOngoingPdaEventLinks';

Update tConfigurationSetting set vchInstructions = 'boolean value - controls is QA approval or QA approval dates are displayed, editable, and sortable.'
where vchSetting = 'Web' and vchKey = 'DisplayPromotionFields';

Update tConfigurationSetting set vchInstructions = 'boolean value - promotion button displayed on the Resource Detail page in the admin area.'
where vchSetting = 'Web' and vchKey = 'EnablePromotionToProduction';


Update tConfigurationSetting set vchInstructions = 'boolean value - determines weather the admin tab is displayed to rittenhouse administrators'
where vchSetting = 'Admin' and vchKey = 'DisplayAdminTab';

Update tConfigurationSetting set vchInstructions = 'string value - receipt of who will be BCCed on all purchases. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Admin' and vchKey = 'PurchaseConfirmationEmail';

Update tConfigurationSetting set vchInstructions = 'string value - receipt of who the contact us email goes to. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Admin' and vchKey = 'ContactUsEmail';

Update tConfigurationSetting set vchInstructions = 'string value - receipt of who will be CCed on both trial account creation methods. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Admin' and vchKey = 'TrialInitializeEmail';

Update tConfigurationSetting set vchInstructions = 'string value - URL of our Marc Record Website'
where vchSetting = 'Admin' and vchKey = 'MarcRecordWebsite';

Update tConfigurationSetting set vchInstructions = 'string balue - receipt of the email that is gerneated when a trial makes there first purchase. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Admin' and vchKey = 'NewAccountNotificationEmail';

Update tConfigurationSetting set vchInstructions = 'string value - receipt of the email that is generated when a book is QA approved. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Admin' and vchKey = 'QaApprovalEmailTo';

Update tConfigurationSetting set vchInstructions = 'string value - receipt who is CCed of the email that is generated when a book is QA approved. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Admin' and vchKey = 'QaApprovalEmailCc';

Update tConfigurationSetting set vchInstructions = 'string value - URL to where alert images are stored.'
where vchSetting = 'Admin' and vchKey = 'AlertImageLocation';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk where alert images are stored.'
where vchSetting = 'Admin' and vchKey = 'AlertImagePhysicalLocation';

Update tConfigurationSetting set vchInstructions = 'int value - number of hours the cache will live by default.'
where vchSetting = 'Cache' and vchKey = 'DefaultExpirationInHours';


Update tConfigurationSetting set vchInstructions = 'string value - google analytics account number used to send page views'
where vchSetting = 'Client' and vchKey = 'GoogleAnalyticsAccount';

Update tConfigurationSetting set vchInstructions = 'string value - URL we use to send Google Analytics data.'
where vchSetting = 'Client' and vchKey = 'GoogleAnalyticsUrl';

Update tConfigurationSetting set vchInstructions = 'bool value - disable right click for the entire website.'
where vchSetting = 'Client' and vchKey = 'DisableRightClick';

Update tConfigurationSetting set vchInstructions = 'int value - default number of resources displayed on browse titles.'
where vchSetting = 'Client' and vchKey = 'BrowsePageSize';

Update tConfigurationSetting set vchInstructions = 'int value - amount of time in minutes before a resouce times out when browsing titles.'
where vchSetting = 'Client' and vchKey = 'ResourceTimeoutInMinutes';

Update tConfigurationSetting set vchInstructions = 'int value - amount of time in seconds before timeout the model pops up for the user to extend the timeout. '
where vchSetting = 'Client' and vchKey = 'ResourceTimeoutModalInSeconds';

Update tConfigurationSetting set vchInstructions = 'string value - URL used for links to Doody reviews.'
where vchSetting = 'Client' and vchKey = 'DoodyReviewLink';

Update tConfigurationSetting set vchInstructions = 'string value - URL for CMS html page requests.'
where vchSetting = 'Client' and vchKey = 'R2CmsContentLink';

--Update tConfigurationSetting set vchInstructions = ''
--where vchSetting = 'Client' and vchKey = 'RecaptchaPrivateKey';

--Update tConfigurationSetting set vchInstructions = ''
--where vchSetting = 'Client' and vchKey = 'RecaptchaPublicKey';

Update tConfigurationSetting set vchInstructions = 'int value - amount of time in seconds the number of requests will be counted for capthca pop-up.'
where vchSetting = 'Client' and vchKey = 'RecaptchaRequestTimeInSeconds';

Update tConfigurationSetting set vchInstructions = 'int value - number of requests within the RecaptchaRequestTimeInSeconds before the captcha pop-up.'
where vchSetting = 'Client' and vchKey = 'RecaptchaNumberOfRequests';

Update tConfigurationSetting set vchInstructions = 'string value - URL of the CMS we send the request for the home page Carousel.'
where vchSetting = 'Client' and vchKey = 'HomePageCarouselUrl';

Update tConfigurationSetting set vchInstructions = 'string value - URL for CMS html content.'
where vchSetting = 'Client' and vchKey = 'CmsHtmlContentUrl';

Update tConfigurationSetting set vchInstructions = 'int value - speed of the autoplay of the home page carousel.'
where vchSetting = 'Client' and vchKey = 'CarouselAutoplaySpeedInSeconds';

Update tConfigurationSetting set vchInstructions = 'int value - timeout of the CMS request in milliseconds.'
where vchSetting = 'Client' and vchKey = 'CmsTimeout';

Update tConfigurationSetting set vchInstructions = 'int value - number of days a user with the role user is locked out.'
where vchSetting = 'Client' and vchKey = 'AutoLockUser';

Update tConfigurationSetting set vchInstructions = 'int value - number of days a user with the role expert reviewer is locked out.'
where vchSetting = 'Client' and vchKey = 'AutoLockExpertReviewer';

Update tConfigurationSetting set vchInstructions = 'int value - number of days a user with the role institution admin is locked out.'
where vchSetting = 'Client' and vchKey = 'AutoLockInstitutionAdmin';

Update tConfigurationSetting set vchInstructions = 'int value - number of days a user with the role sales associate is locked out.'
where vchSetting = 'Client' and vchKey = 'AutoLockSalesAdmin';

Update tConfigurationSetting set vchInstructions = 'int value - number of days a user with the role rittenhouse admin is locked out.'
where vchSetting = 'Client' and vchKey = 'AutoLockRittenhouseAdmin';

Update tConfigurationSetting set vchInstructions = 'string value - URL for rittenhouse that is used to correct images that are in from CMS content.'
where vchSetting = 'Client' and vchKey = 'RittenhouseBaseUrl';

Update tConfigurationSetting set vchInstructions = 'string value - key for Ooyala player'
where vchSetting = 'Client' and vchKey = 'OoyalaKey';

Update tConfigurationSetting set vchInstructions = 'string value - key for Flow player'
where vchSetting = 'Client' and vchKey = 'FlowplayerKey';

Update tConfigurationSetting set vchInstructions = 'string value - URL for the media directory.'
where vchSetting = 'Client' and vchKey = 'MediaBaseUrl';

Update tConfigurationSetting set vchInstructions = 'int value - productId of the annual maintenance fee.'
where vchSetting = 'CollectionManagement' and vchKey = 'AnnualMaintenanceFeeProductId';

Update tConfigurationSetting set vchInstructions = 'int value - the maximum number of views for before a PDA gets added to cart.'
where vchSetting = 'CollectionManagement' and vchKey = 'PatronDriveAcquisitionMaxViews';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk where the DtSearch index is located.'
where vchSetting = 'Content' and vchKey = 'DtSearchIndexLocation';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk of the DtSearch program.'
where vchSetting = 'Content' and vchKey = 'DtSearchBinLocation';

Update tConfigurationSetting set vchInstructions = 'string value - location of the log file generated by DtSearch. Do not set in production, set to C:\DtSearchLog.txt for debugging purposes.'
where vchSetting = 'Content' and vchKey = 'DtSearchLogFilePath';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk for the XML resource files.'
where vchSetting = 'Content' and vchKey = 'ContentLocation';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk of the cached html file.'
where vchSetting = 'Content' and vchKey = 'NewContentLocation';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk of the xsl file.'
where vchSetting = 'Content' and vchKey = 'XslLocation';

Update tConfigurationSetting set vchInstructions = 'string value - URL of images located in resources.'
where vchSetting = 'Content' and vchKey = 'ImageBaseUrl';

Update tConfigurationSetting set vchInstructions = 'string value - URL of cover images for resources.'
where vchSetting = 'Content' and vchKey = 'BookCoverUrl';

Update tConfigurationSetting set vchInstructions = 'datetime value - any content that has been transformed before this date will be transformed.'
where vchSetting = 'Content' and vchKey = 'MinTransformDate';

Update tConfigurationSetting set vchInstructions = 'int value - number of seconds a resource concurrency becomes locked for an instituiton.'
where vchSetting = 'Content' and vchKey = 'ResourceLockTime';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk for images inside resources.'
where vchSetting = 'Content' and vchKey = 'ImageBaseFileLocation';

Update tConfigurationSetting set vchInstructions = 'decimal value - minimum price of a resource for it to be sold.'
where vchSetting = 'Content' and vchKey = 'ResourceMinimumListPrice';

Update tConfigurationSetting set vchInstructions = 'int value - the number of results to be displayed in the search type ahead feature.'
where vchSetting = 'Content' and vchKey = 'SearchTypeaheadResultLimit';

Update tConfigurationSetting set vchInstructions = 'string value - default email address used when sending emails.'
where vchSetting = 'Email' and vchKey = 'DefaultFromAddress';

Update tConfigurationSetting set vchInstructions = 'string value - name displayed in the email correlating to the DefaultFromAddress.'
where vchSetting = 'Email' and vchKey = 'DefaultFromName';

Update tConfigurationSetting set vchInstructions = 'string value - email address that is the auto reply for emails sent.'
where vchSetting = 'Email' and vchKey = 'DefaultReplayToAddress';

Update tConfigurationSetting set vchInstructions = 'string value - name displayed in the email correlating to the DefaultReplayToAddress.'
where vchSetting = 'Email' and vchKey = 'DefaultReplayToName';

Update tConfigurationSetting set vchInstructions = 'boolean value - all emails BCCed to BccEmailAddresses.'
where vchSetting = 'Email' and vchKey = 'BccAllMessages';

Update tConfigurationSetting set vchInstructions = 'string value - email address used when BccAllMessages is set to true. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Email' and vchKey = 'BccEmailAddresses';

Update tConfigurationSetting set vchInstructions = 'boolean value - emails sent to customers. (Testing Mode)'
where vchSetting = 'Email' and vchKey = 'SendToCustomers';

Update tConfigurationSetting set vchInstructions = 'string value - emails used when SendToCustomers is set to False. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Email' and vchKey = 'TestEmailAddresses';

Update tConfigurationSetting set vchInstructions = 'string value - prefix used in subject headings when not null.'
where vchSetting = 'Email' and vchKey = 'AddEnvironmentPrefixToSubject';

Update tConfigurationSetting set vchInstructions = 'string value - location of htm,l templates used in emails.'
where vchSetting = 'Email' and vchKey = 'TemplatesDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - emails address used when PDA titles are added to cart. Can except multiple email addressed seperated by a ;'
where vchSetting = 'Email' and vchKey = 'PdaAddToCartCcEmailAddresses';

Update tConfigurationSetting set vchInstructions = 'string value - location where emails should be written to disk.'
where vchSetting = 'Email' and vchKey = 'OutputPath';

Update tConfigurationSetting set vchInstructions = 'string value - URL for the current instance of R2Library. '
where vchSetting = 'Email' and vchKey = 'WebSiteBaseUrl';

Update tConfigurationSetting set vchInstructions = 'string value - account number for the guest account.'
where vchSetting = 'Institution' and vchKey = 'GuestAccountNumber';

Update tConfigurationSetting set vchInstructions = 'string value - URL for institution logos.'
where vchSetting = 'Institution' and vchKey = 'LogoLocation';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk where the institution logos are.'
where vchSetting = 'Institution' and vchKey = 'LocalLogoLocation';

Update tConfigurationSetting set vchInstructions = 'int value - minimum number of resources to display paging. '
where vchSetting = 'Institution' and vchKey = 'MinimumResourceCountForPaging';

Update tConfigurationSetting set vchInstructions = 'string value - message queue for emails.'
where vchSetting = 'MessageQueue' and vchKey = 'EmailMessageQueue';

Update tConfigurationSetting set vchInstructions = 'string value - message queue for orders.'
where vchSetting = 'MessageQueue' and vchKey = 'OrderProcessingQueue';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq connection for current instance'
where vchSetting = 'MessageQueue' and vchKey = 'EnvironmentConnectionString';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq connection for ''production'' instance'
where vchSetting = 'MessageQueue' and vchKey = 'ProductionConnectionString';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk where error emails are written to.'
where vchSetting = 'MessageQueue' and vchKey = 'SendErrorDirectoryPath';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq route key for request logging.'
where vchSetting = 'MessageQueue' and vchKey = 'RequestLoggingRouteKey';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq exchange name for request logging.'
where vchSetting = 'MessageQueue' and vchKey = 'RequestLoggingExchangeName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq queue name for request logging.'
where vchSetting = 'MessageQueue' and vchKey = 'RequestLoggingQueueName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq route key for resource batch promotion.'
where vchSetting = 'MessageQueue' and vchKey = 'ResourceBatchPromotionRouteKey';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq exchange name for resource batch promotion.'
where vchSetting = 'MessageQueue' and vchKey = 'ResourceBatchPromotionExchangeName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq queue name for resource batch promotion.'
where vchSetting = 'MessageQueue' and vchKey = 'ResourceBatchPromotionQueueName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq route key for ongoing pda.'
where vchSetting = 'MessageQueue' and vchKey = 'OngoingPdaRouteKey';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq exchange name for ongoing pda.'
where vchSetting = 'MessageQueue' and vchKey = 'OngoingPdaExchangeName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq queue name for ongoing pda.'
where vchSetting = 'MessageQueue' and vchKey = 'OngoingPdaQueueName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq route key for analytics.'
where vchSetting = 'MessageQueue' and vchKey = 'AnalyticsRouteKey';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq exchange name for analytics.'
where vchSetting = 'MessageQueue' and vchKey = 'AnalyticsExchangeName';

Update tConfigurationSetting set vchInstructions = 'string value - rabbitmq queue name for analytics.'
where vchSetting = 'MessageQueue' and vchKey = 'AnalyticsQueueName';

Update tConfigurationSetting set vchInstructions = 'string value - appended to every email sent from the R2Utilities.'
where vchSetting = 'R2Utilities' and vchKey = 'EnvironmentName';

Update tConfigurationSetting set vchInstructions = 'string value - from address used on every email sent.'
where vchSetting = 'R2Utilities' and vchKey = 'DefaultFromAddress';

Update tConfigurationSetting set vchInstructions = 'string value - name displayed in the email correlating to the DefaultFromAddress.'
where vchSetting = 'R2Utilities' and vchKey = 'DefaultFromAddressName';

Update tConfigurationSetting set vchInstructions = 'string value = location of the XML configurations that contain email addresses for the different status of the task.'
where vchSetting = 'R2Utilities' and vchKey = 'EmailConfigDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the r2utilities database.'
where vchSetting = 'R2Utilities' and vchKey = 'R2UtilitiesDatabaseConnection';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the ritt001 database.'
where vchSetting = 'R2Utilities' and vchKey = 'R2DatabaseConnection';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the r2reports database.'
where vchSetting = 'R2Utilities' and vchKey = 'R2ReportsConnection';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the Ip2Location database.'
where vchSetting = 'R2Utilities' and vchKey = 'Ip2LocationConnection';

Update tConfigurationSetting set vchInstructions = 'int value - batch size for the html indexer'
where vchSetting = 'R2Utilities' and vchKey = 'HtmlIndexerBatchSize';

Update tConfigurationSetting set vchInstructions = 'int value - max size of batches per run.'
where vchSetting = 'R2Utilities' and vchKey = 'HtmlIndexerMaxIndexBatches';

Update tConfigurationSetting set vchInstructions = 'int value - limit of fragmentation allowed before the index is compressed.'
where vchSetting = 'R2Utilities' and vchKey = 'HtmlIndexerFragmentationLimit';

Update tConfigurationSetting set vchInstructions = 'boolean value - overrides the HtmlIndexerFragmentationLimit to force compression.'
where vchSetting = 'R2Utilities' and vchKey = 'HtmlIndexerForceCompression';

Update tConfigurationSetting set vchInstructions = 'int value - batch size for the FixDocIdsTask.'
where vchSetting = 'R2Utilities' and vchKey = 'ResourceFileInsertBatchSize';

Update tConfigurationSetting set vchInstructions = 'string value - working directory for the Ip2Location task.'
where vchSetting = 'R2Utilities' and vchKey = 'Ip2LocationWorkingFolder';

Update tConfigurationSetting set vchInstructions = 'string value - table name for the Ip2Location data.'
where vchSetting = 'R2Utilities' and vchKey = 'Ip2LocationTableName';

Update tConfigurationSetting set vchInstructions = 'string value - database name for the Ip2Location data.'
where vchSetting = 'R2Utilities' and vchKey = 'Ip2LocationDatabaseName';

Update tConfigurationSetting set vchInstructions = 'string value - fields stored in the DtSearch seperated by ,'
where vchSetting = 'R2Utilities' and vchKey = 'IndexStoredFields';

Update tConfigurationSetting set vchInstructions = 'string value - enumerable fields stored in the DtSearch seperated by ,'
where vchSetting = 'R2Utilities' and vchKey = 'IndexEnumerableFields';

Update tConfigurationSetting set vchInstructions = 'boolean value - determines if the TermHighlightService and TransformXMLTask should batch in descending order.'
where vchSetting = 'R2Utilities' and vchKey = 'OrderBatchDescending';

Update tConfigurationSetting set vchInstructions = 'string value - table name of authors for the TransformXMLTask.'
where vchSetting = 'R2Utilities' and vchKey = 'AuthorTableName';

Update tConfigurationSetting set vchInstructions = 'string value - table name for the resource files used in the TransformXMLTask.'
where vchSetting = 'R2Utilities' and vchKey = 'ResourceFileTableName';

Update tConfigurationSetting set vchInstructions = 'string value - default specialty code for resources if none are specifed.'
where vchSetting = 'R2Utilities' and vchKey = 'DefaultSpecialtyCode';

Update tConfigurationSetting set vchInstructions = 'string value - default practice area code for resources if none are specifed.'
where vchSetting = 'R2Utilities' and vchKey = 'DefaultPracticeAreaCode';

Update tConfigurationSetting set vchInstructions = 'string value - location of the XML and Image source files for the BookLoaderPostProcessingTask.'
where vchSetting = 'R2Utilities' and vchKey = 'BookLoaderSourceRootDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - desintation location of the image files for the BookLoaderPostProcessingTask.'
where vchSetting = 'R2Utilities' and vchKey = 'BookLoaderImageDestinationDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - URL to validate resource in the BookLoaderPostProcessingTask.'
where vchSetting = 'R2Utilities' and vchKey = 'ResourceValidationBaseUrl';

Update tConfigurationSetting set vchInstructions = 'boolean value - used to prevent sending emails to clients during testing.'
where vchSetting = 'R2Utilities' and vchKey = 'EmailTestMode';

Update tConfigurationSetting set vchInstructions = 'int value - the number of emails to send if EmailTestMode is set to true.'
where vchSetting = 'R2Utilities' and vchKey = 'EmailTestNumberOfEmails';

Update tConfigurationSetting set vchInstructions = 'string value - name of the server prelude database is located on.'
where vchSetting = 'R2Utilities' and vchKey = 'PreludeDataLinkedServer';

Update tConfigurationSetting set vchInstructions = 'string value - name of the R2Utilities database name.'
where vchSetting = 'R2Utilities' and vchKey = 'R2UtilitiesDatabaseName';

Update tConfigurationSetting set vchInstructions = 'int value - number of days to go back in the PdaAddedToCartTask'
where vchSetting = 'R2Utilities' and vchKey = 'PdaAddedToCartNumberOfDaysAgo';

Update tConfigurationSetting set vchInstructions = 'int value - number of days to go back in the PdaRemovedFromCartTask'
where vchSetting = 'R2Utilities' and vchKey = 'PdaRemovedFromCartNumberOfDays';

Update tConfigurationSetting set vchInstructions = 'string value - name of R2Reports database name.'
where vchSetting = 'R2Utilities' and vchKey = 'R2ReportsDatabaseName';

Update tConfigurationSetting set vchInstructions = 'string value - name of the Rit001 database name.'
where vchSetting = 'R2Utilities' and vchKey = 'R2DatabaseName';

Update tConfigurationSetting set vchInstructions = 'string value - name of the directory the Tabers XML is located in the LoadTabersDictionaryTask.'
where vchSetting = 'R2Utilities' and vchKey = 'TabersXmlPath';

Update tConfigurationSetting set vchInstructions = 'string value - connection string of the tabers database.'
where vchSetting = 'R2Utilities' and vchKey = 'TabersDictionaryConnection';

Update tConfigurationSetting set vchInstructions = 'string value - location where aggregated data is exported to.'
where vchSetting = 'R2Utilities' and vchKey = 'AggregateDailyCountFolder';

Update tConfigurationSetting set vchInstructions = 'string value - destination of the zipped up AggregateDailyCountFolder data.'
where vchSetting = 'R2Utilities' and vchKey = 'AggregateDailyCountFolderZipLocation';

Update tConfigurationSetting set vchInstructions = 'int value - months to go back that we stop BCPing data out of the database.'
where vchSetting = 'R2Utilities' and vchKey = 'AggregateDailyCountMonthsToGoBack';

Update tConfigurationSetting set vchInstructions = 'int value - timeout period in seconds '
where vchSetting = 'R2Utilities' and vchKey = 'AggregateDailyCommandTimeout';

Update tConfigurationSetting set vchInstructions = 'int value - number of licenses to add to house accounts in the BookLoaderPostProcessingTask.'
where vchSetting = 'R2Utilities' and vchKey = 'AutoLicensesNumberOfLicenses';

Update tConfigurationSetting set vchInstructions = 'string value - the key used for api.geoips.com to get names associated with ip addresses in the WebActivityReportTask'
where vchSetting = 'R2Utilities' and vchKey = 'GeoIPsApiKey';

Update tConfigurationSetting set vchInstructions = 'string value - URL for the Special icons.'
where vchSetting = 'R2Utilities' and vchKey = 'SpecialIconBaseUrl';

Update tConfigurationSetting set vchInstructions = 'string value - URL for the CMS that is used to get Quick Notes for the InstitutionDashboardEmailTask.'
where vchSetting = 'R2Utilities' and vchKey = 'CmsHtmlContentUrl';

Update tConfigurationSetting set vchInstructions = 'boolean value - override for dashboard emails. If true exclude quick notes.'
where vchSetting = 'R2Utilities' and vchKey = 'OverRideDashboardEmailQuickNotes';

Update tConfigurationSetting set vchInstructions = 'int value - days to go back to in the UpdateInstitutionStatistics.'
where vchSetting = 'R2Utilities' and vchKey = 'UpdateInstitutionStatisticsPreviousDays';

Update tConfigurationSetting set vchInstructions = 'int value - number of days to go back for the DCT emails.'
where vchSetting = 'R2Utilities' and vchKey = 'DctUpdateEmailStartDaysAgo';

Update tConfigurationSetting set vchInstructions = 'int value - number of institutions to display on the WebActivityReportTask'
where vchSetting = 'R2Utilities' and vchKey = 'MaxActivityReportInstitutionDisplay';

Update tConfigurationSetting set vchInstructions = 'string value - location of the content backup directory.'
where vchSetting = 'R2Utilities' and vchKey = 'ContentBackupDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - ftp server used to restore resource content.'
where vchSetting = 'R2Utilities' and vchKey = 'ContentBackupFtpServerHost';

Update tConfigurationSetting set vchInstructions = 'string value - username for the ftp server used to restore resource content associated with ContentBackupFtpServerHost.'
where vchSetting = 'R2Utilities' and vchKey = 'ContentBackupFtpServerUsername';

Update tConfigurationSetting set vchInstructions = 'string value - password for the ftp server used to restore resource content associated with ContentBackupFtpServerHost.'
where vchSetting = 'R2Utilities' and vchKey = 'ContentBackupFtpServerPassword';

Update tConfigurationSetting set vchInstructions = 'string value - directory located on the FTP server associated with ContentBackupFtpServerHost.'
where vchSetting = 'R2Utilities' and vchKey = 'ContentBackupFtpServerDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - temp directory used to compress email attachments.'
where vchSetting = 'R2Utilities' and vchKey = 'TaskCompressionTempDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - directory used to compress extra content from resources.'
where vchSetting = 'R2Utilities' and vchKey = 'AuditFilesOnDiskBackupDirectory';

Update tConfigurationSetting set vchInstructions = 'int value - seconds to check if audit files on disk are within the exceeded date window.'
where vchSetting = 'R2Utilities' and vchKey = 'AuditXmlDateModifiedWindowInSeconds';

Update tConfigurationSetting set vchInstructions = 'string value - URL used to download IP2Location database.'
where vchSetting = 'R2Utilities' and vchKey = 'Ip2LocationDownloadUrl';

Update tConfigurationSetting set vchInstructions = 'string value - Locations to copy extract iplocation.csv file. Can support mulitple locations sperated with ; '
where vchSetting = 'R2Utilities' and vchKey = 'Ip2LocationFileDestinations';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the cover images.'
where vchSetting = 'WebImage' and vchKey = 'BookCoverDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - URL for publisher logo images.'
where vchSetting = 'WebImage' and vchKey = 'PublisherImageUrl';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for publisher logo images.'
where vchSetting = 'WebImage' and vchKey = 'PublisherImageDirectory';

Update tConfigurationSetting set vchInstructions = 'int value - Maximum width of a cover image the website will allow to be uploaded.'
where vchSetting = 'WebImage' and vchKey = 'BookCoverMaxWidth';

Update tConfigurationSetting set vchInstructions = 'int value - Maximum height of a cover image the website will allow to be uploaded.'
where vchSetting = 'WebImage' and vchKey = 'BookCoverMaxHeight';

Update tConfigurationSetting set vchInstructions = 'int value - Maximum size of a cover image in killibytes the website will allow to be uploaded.'
where vchSetting = 'WebImage' and vchKey = 'BookCoverMaxSizeInKb';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the special icons.'
where vchSetting = 'WebImage' and vchKey = 'SpecialIconDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - URL of special icons'
where vchSetting = 'WebImage' and vchKey = 'SpecialIconBaseUrl';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the R2Reports database.'
where vchSetting = 'WindowsService' and vchKey = 'R2ReportsConnectionString';

Update tConfigurationSetting set vchInstructions = 'string value - conneciton string for the rit001 database.'
where vchSetting = 'WindowsService' and vchKey = 'RIT001ProductionConnectionString';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the r2utilities database.'
where vchSetting = 'WindowsService' and vchKey = 'R2UtilitiesProductionConnectionString';

Update tConfigurationSetting set vchInstructions = 'string value - connection string for the staging instance of rit001. Used for in the promotion process.'
where vchSetting = 'WindowsService' and vchKey = 'RIT001StagingConnectionString';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the book XML source directory.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteXmlSourceDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the book XML destination directory.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteXmlDestinationDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the book images source directory.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteImagesSourceDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the book images destination directory.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteImagesDestinationDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the book cover images source directory.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteCoverImageSourceDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Location on disk for the book cover images destination directory.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteCoverImageDestinationDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Email addresses for the status results of promotion thread. Email addresses seperated by ;'
where vchSetting = 'WindowsService' and vchKey = 'PromoteStatusEmailToAddresses';

Update tConfigurationSetting set vchInstructions = 'string value - from and reply to email address used in the promotion thread status emails.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteFromEmailAddress';

Update tConfigurationSetting set vchInstructions = 'string value - from and reply display name used in the promotion thread status emails.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteFromDisplayName';

Update tConfigurationSetting set vchInstructions = 'string value - sql file used to promotion a resource in the promotion service'
where vchSetting = 'WindowsService' and vchKey = 'PromoteSqlScriptFile';

Update tConfigurationSetting set vchInstructions = 'string value - staging URL for the resource.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteStagingDomain';

Update tConfigurationSetting set vchInstructions = 'string value - promotion URL for the resource.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteProductionDomain';

Update tConfigurationSetting set vchInstructions = 'int value - number of licenses to give house accounts during the promotion process.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteAutoLicenseCount';

Update tConfigurationSetting set vchInstructions = 'string value - source directory for the media files associated with the resource being promoted.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteMediaFilesSourceDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - destination directory for the media files associated with the resource being promoted.'
where vchSetting = 'WindowsService' and vchKey = 'PromoteMediaFilesDestinationDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || ship to number - M = manual (changes made to address), D = default (bill to address) or numeric value from ship to file.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayShipToNumber';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || blind shipping flag.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayBlindShipFlag';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || follett corp flag - Y = yes, N = no, currently not needed for web orders, default to N for now'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayFollettCorpFlag';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || residential flag - Y = yes, N = no, currently not used, but leave in for future use, set to N'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayResidentFlag';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || order type - 01 = regular, 04 = drop ship, 06 = direct to invoice (R2 sales), set to 01'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayOrderType';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || ship via - user supplied, 01 = Standard, 02 = 2-day, 03 = Overnight'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayShipVia';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || written by - always set to WEB'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayWrittenBy';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || achnowledge flag - E=Prelude emails invoice'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayAcknowledgeFlag';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || admin hold flag - Y/N for placing orders on admin hold in Prelude.  Set to Y for initial launch.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayAdminHoldFlag';

Update tConfigurationSetting set vchInstructions = 'string value - prelude order service || admin hold mgr - Admin Hold Manager for web orders.  Set to 21 for initial launch.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayAdminHoldManager';

Update tConfigurationSetting set vchInstructions = 'string value - location on disk where orders will be written to.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayLogDirectory';

Update tConfigurationSetting set vchInstructions = 'string value - Used in the prelude order service - should not be set in production, used as a way to distinguish dev & stage orders'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayOrderFileNamePrefix';

Update tConfigurationSetting set vchInstructions = 'string value - indicates whether not to send the order file to prelude || true = send file to prelude, false = no not sent file to Prelude, used for testing'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelaySendOrderFileToPrelude';

Update tConfigurationSetting set vchInstructions = 'string value - host name of where the prelude order file is sent.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelaySFtpHost';

Update tConfigurationSetting set vchInstructions = 'string value - username for OrderRelaySFtpHost.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelaySFtpUsername';

Update tConfigurationSetting set vchInstructions = 'string value - password used for OrderRelaySFtpUsername.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelaySFtpPassword';

Update tConfigurationSetting set vchInstructions = 'string value - email addresses to send the order file receipt to. Sepearted by ,'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayInternalOrderEmailToAddresses';

Update tConfigurationSetting set vchInstructions = 'string value - from email address of the order file.'
where vchSetting = 'WindowsService' and vchKey = 'OrderRelayInternalOrderEmailFromAddress';

select * 
, 'Insert into tConfigurationSetting(vchConfiguration, vchSetting, vchKey, vchValue) values 
(''' + vchConfiguration + ''', ''' + vchSetting + ''', ''' + vchKey + ''', ''' + vchValue + ''')'
from tConfigurationSetting 
where vchInstructions is null and vchConfiguration not in ('config')
and vchSetting not in ('IndexTermHighlight','TabersTermHighlight')

--delete from tConfigurationSetting
--where vchInstructions is null and vchConfiguration not in ('config')
--and vchSetting not in ('IndexTermHighlight','TabersTermHighlight')
--and vchSetting not in ('IndexTermHighlight','TabersTermHighlight')



