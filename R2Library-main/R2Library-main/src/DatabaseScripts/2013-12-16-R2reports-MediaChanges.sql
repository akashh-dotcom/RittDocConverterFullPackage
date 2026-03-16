SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[PageView] ADD [serverNumber] tinyint NULL
GO

--ALTER TABLE [dbo].[ContentView] ADD [pageViewId] int NULL
--GO
--ALTER TABLE [dbo].[Search] ADD [pageViewId] int NULL
--GO
--ALTER TABLE [dbo].[ContentView] DROP COLUMN [pageViewId]
--ALTER TABLE [dbo].[Search] DROP COLUMN [pageViewId]


ALTER TABLE [dbo].[ContentView] ADD [requestId] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[Search] ADD [requestId] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

--ALTER TABLE [dbo].[Search] DROP COLUMN [searchTimestamp2]
ALTER TABLE [dbo].[Search] ADD [searchTimestamp2] datetime NULL
GO

update Search set searchTimestamp2 = searchTimestamp


--ALTER TABLE [dbo].[ContentView] DROP COLUMN [pageViewId]
--ALTER TABLE [dbo].[Search] DROP COLUMN [pageViewId]

EXEC sp_RENAME 'Search.searchTimestamp' , 'searchTimestamp_delete', 'COLUMN'
 
EXEC sp_RENAME 'Search.searchTimestamp2' , 'searchTimestamp', 'COLUMN'

ALTER TABLE [dbo].[Search] DROP COLUMN [searchTimestamp_delete]

select top 100 * from Search where searchTimestamp > getdate() - 1 order by searchTimestamp desc



SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[MediaView] (
[mediaViewId] int IDENTITY(1, 1) NOT NULL,
[institutionId] int NULL,
[userId] int NULL,
[resourceId] int NOT NULL,
[chapterSectionId] varchar(50) NULL,
[mediaFileName] varchar(255) NOT NULL,
[ipAddressOctetA] tinyint NOT NULL,
[ipAddressOctetB] tinyint NOT NULL,
[ipAddressOctetC] tinyint NOT NULL,
[ipAddressOctetD] tinyint NOT NULL,
[ipAddressInteger] bigint NOT NULL,
[mediaViewTimestamp] datetime NOT NULL,
[requestId] varchar(50) NULL,
CONSTRAINT [PK__MediaVie__AA7EF1B61738D5C9]
PRIMARY KEY CLUSTERED ([mediaViewId] ASC)
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
ALTER TABLE [dbo].[MediaView] SET (LOCK_ESCALATION = TABLE);
GO


select mediaViewId,  from MediaView

insert into MediaView (institutionId, userId, resourceId, chapterSectionId, mediaFileName, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, mediaViewTimestamp, requestId)

select * from PageView where serverNumber is not null and pageViewTimestamp > '12/12/2013'

select * from Search where requestId is not null and searchTimestamp > '12/12/2013'

select * from MediaView where requestId is not null and mediaViewTimestamp > '12/12/2013'
order by mediaViewTimestamp desc


