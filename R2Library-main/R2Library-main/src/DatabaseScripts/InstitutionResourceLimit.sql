CREATE VIEW [dbo].[vContentView]
AS
select contentTurnawayId as [contentViewId], institutionId, userId, resourceId, chapterSectionId, turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewTimestamp, actionTypeId, foundFromSearch, searchTerm, requestId
from DEV_R2Reports..ContentView
GO


CREATE VIEW [dbo].[vPageContentView]
AS
select pv.pageViewId, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC
     , pv.ipAddressOctetD, pv.ipAddressInteger, pv.pageViewTimestamp, pv.pageViewRunTime, pv.sessionId, pv.url
     , pv.requestId, pv.referrer, pv.countryCode, pv.serverNumber
     , cv.contentTurnawayId, cv.resourceId, cv.chapterSectionId, cv.turnawayTypeId, cv.actionTypeId
     , cv.foundFromSearch, cv.searchTerm
     , u.vchFirstName, u.vchLastName, u.vchUserName, u.vchUserEmail
from   Dev_R2Reports..PageView pv
 join  Dev_R2Reports..ContentView cv on cv.requestId = pv.requestId
 left outer join  tUser u on u.iUserId = pv.userId
GO


CREATE VIEW [dbo].[vPageView]
AS
select pv.pageViewId, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC
     , pv.ipAddressOctetD, pv.ipAddressInteger, pv.pageViewTimestamp, pv.pageViewRunTime, pv.sessionId, pv.url
     , pv.requestId, pv.referrer, pv.countryCode, pv.serverNumber
from   Dev_R2Reports..PageView pv
GO




CREATE TABLE [dbo].[tInstitutionResourceLockType] (
[iInstitutionResourceLockTypeId] smallint NOT NULL,
[vchInstitutionResourceLockType] varchar(50) NOT NULL,
CONSTRAINT [PK_tInstitutionResourceLockType]
PRIMARY KEY CLUSTERED ([iInstitutionResourceLockTypeId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceLockType] SET (LOCK_ESCALATION = TABLE);
GO



CREATE TABLE [dbo].[tInstitutionResourceLock] (
[iInstitutionResourceLockId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iResourceId] int NOT NULL,
[iInstitutionResourceLockTypeId] smallint NOT NULL,
[dtLockStartDate] datetime NOT NULL,
[dtLockEndDate] datetime NOT NULL,
[dtLockEmailAlertTimestamp] datetime NULL,
[vchLockData] varchar(2000) NULL,
[vchLockEmailAlertData] varchar(2000) NULL,
CONSTRAINT [PK_tInstitutionResourceLock]
PRIMARY KEY CLUSTERED ([iInstitutionResourceLockId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionResourceLock_Type]
FOREIGN KEY ([iInstitutionResourceLockTypeId])
REFERENCES [dbo].[tInstitutionResourceLockType] ( [iInstitutionResourceLockTypeId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceLock] SET (LOCK_ESCALATION = TABLE);
GO






insert into tInstitutionResourceLockType (iInstitutionResourceLockTypeId, vchInstitutionResourceLockType)
values (1, 'All Access')

insert into tInstitutionResourceLockType (iInstitutionResourceLockTypeId, vchInstitutionResourceLockType)
values (2, 'Print Access')

insert into tInstitutionResourceLockType (iInstitutionResourceLockTypeId, vchInstitutionResourceLockType)
values (3, 'Email Access')


