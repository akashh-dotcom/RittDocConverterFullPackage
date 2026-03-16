
select * from tUser

select * from tRole

SET IDENTITY_INSERT tRole ON

insert into tRole (iRoleId, vchRoleCode, vchRoleDesc, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
    values (7, 'FACULTY', 'Faculty User', 'sjscheider', getdate(), null, null, 1)

SET IDENTITY_INSERT tRole OFF


-- delete from tRole where vchRoleCode = 'FACULTY'



ALTER TABLE [dbo].[tUser] ADD [tiReceiveFacultyUserRecommendations] int NOT NULL DEFAULT 1
GO

ALTER TABLE [dbo].[tUser] ADD [dtFacultyUserRequestDate] smalldatetime
GO

ALTER TABLE [dbo].[tInstitution] ADD [tiFacultyUserEnabled] tinyint NOT NULL DEFAULT 1
GO

select * from [dbo].[tInstitution]

select * from tRole

select * from tUser



alter table tUser
add tiReceiveFacultyUserRequests tinyint not null default((0));



UPDATE     tUser
SET tiReceiveFacultyUserRequests = 1
FROM tUser u
JOIN tInstitution i on u.iInstitutionId = i.iInstitutionId and u.vchUserName = i.vchInstitutionAcctNum


------------------------------------------------------------------------------

/****** Object: Table [dbo].[tInstitutionRecommendation]   Script Date: 9/30/2013 2:42:13 PM ******/
--USE [DEV_RIT001];
--GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionRecommendation] (
[iRecommendationId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iResourceId] int NOT NULL,
[iFacultyUserId] int NOT NULL,
[dtAlertSentDate] smalldatetime NULL,
[iAddedToCartByUserId] int NULL,
[dtAddedToCartDate] smalldatetime NULL,
[iPurchasedByUserId] int NULL,
[dtPurchaseDate] smalldatetime NULL,
[iDeletedByUserId] int NULL,
[dtDeletedDate] smalldatetime NULL,
[vchNotes] varchar(1024) NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK___tInstit__294C9B4E56E806A3]
PRIMARY KEY CLUSTERED ([iRecommendationId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionRecommendation_Institution]
FOREIGN KEY ([iInstitutionId])
REFERENCES [dbo].[tInstitution] ( [iInstitutionId] ),
CONSTRAINT [FK_tInstitutionRecommendation_Resource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionRecommendation] SET (LOCK_ESCALATION = TABLE);
GO


/****** Object: Table [dbo].[tInstitutionReview]   Script Date: 9/30/2013 2:42:13 PM ******/
--USE [DEV_RIT001];
--GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionReview] (
[iReviewId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iCreatedByUserId] int NOT NULL,
[vchReviewName] varchar(50) NOT NULL,
[vchReviewDescription] varchar(1024) NULL,
[iDeletedByUserId] int NULL,
[dtDeletedDate] smalldatetime NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK___tInstit__2339F9884B1C91AA]
PRIMARY KEY CLUSTERED ([iReviewId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionReview] SET (LOCK_ESCALATION = TABLE);
GO


/****** Object: Table [dbo].[tInstitutionReviewResource]   Script Date: 9/30/2013 2:42:13 PM ******/
--USE [DEV_RIT001];
--GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionReviewResource] (
[iReviewResourceId] int IDENTITY(1, 1) NOT NULL,
[iReviewId] int NOT NULL,
[iResourceId] int NULL,
[iAddedByUserId] int NULL,
[tiActionTypeId] tinyint NOT NULL DEFAULT ((0)),
[iActionByUserId] int NULL,
[dtActionDate] smalldatetime NULL,
[iDeletedByUserId] int NULL,
[dtDeletedDate] smalldatetime NULL,
[vchNotes] varchar(1024) NULL,
[vchCreatorId] varchar(50) NULL,
[dtCreationDate] smalldatetime NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NULL,
CONSTRAINT [PK___tInstit__B066F9634A4A56A4]
PRIMARY KEY CLUSTERED ([iReviewResourceId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionReviewResourc_Resource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [FK_tInstitutionReviewResource_Review]
FOREIGN KEY ([iReviewId])
REFERENCES [dbo].[tInstitutionReview] ( [iReviewId] ),
CONSTRAINT [UK_tInstitutionReviewResource_Ids]
UNIQUE NONCLUSTERED ([iReviewId] ASC, [iResourceId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionReviewResource] SET (LOCK_ESCALATION = TABLE);
GO


/****** Object: Table [dbo].[tInstitutionReviewUser]   Script Date: 9/30/2013 2:42:13 PM ******/
--USE [DEV_RIT001];
--GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionReviewUser] (
[iReviewUserId] int IDENTITY(1, 1) NOT NULL,
[iReviewId] int NOT NULL,
[iUserId] int NULL,
[iAddedByUserId] int NULL,
[dtLastAlertDate] smalldatetime NULL,
[iDeletedByUserId] int NULL,
[dtDeletedDate] smalldatetime NULL,
[vchCreatorId] varchar(50) NULL,
[dtCreationDate] smalldatetime NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NULL,
CONSTRAINT [PK___Institu__B0C92F323D2A14B7]
PRIMARY KEY CLUSTERED ([iReviewUserId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionReviewUser_User]
FOREIGN KEY ([iUserId])
REFERENCES [dbo].[tUser] ( [iUserId] ),
CONSTRAINT [FK_tInstitutionReviewUser_Review]
FOREIGN KEY ([iReviewId])
REFERENCES [dbo].[tInstitutionReview] ( [iReviewId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionReviewUser] SET (LOCK_ESCALATION = TABLE);
GO




