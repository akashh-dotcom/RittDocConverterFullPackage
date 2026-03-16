



CREATE NONCLUSTERED INDEX IDX_PageView_RequestId_INCLUDE_SessionId
ON [dbo].[PageView] ([requestId])
INCLUDE ([sessionId])
GO



CREATE NONCLUSTERED INDEX IDX_DailyContentViewCount_InstitutionId_ContentViewDate
ON [dbo].[DailyContentViewCount] ([institutionId],[contentViewDate])
INCLUDE ([dailyContentViewCountId],[userId],[resourceId],[chapterSectionId],[ipAddressOctetA],[ipAddressOctetB],[ipAddressOctetC],[ipAddressOctetD],[ipAddressInteger],[contentViewCount],[actionTypeId],[foundFromSearch],[licenseType],[resourceStatusId],[uniqueContentViewCount])
GO

