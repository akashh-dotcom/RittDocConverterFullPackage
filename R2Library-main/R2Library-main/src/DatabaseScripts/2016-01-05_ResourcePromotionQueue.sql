SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tResourcePromoteQueue] (
[iResourcePromoteQueueId] int IDENTITY(1, 1) NOT NULL,
[iResourceId] int NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
[vchPromoteBatchName] varchar(100) NULL,
[dtPromoteInitDate] datetime NULL,
[vchPromoteStatus] varchar(100) NULL,
[iAddedByUserId] int NOT NULL,
[iPromotedByUserId] int NULL,
[guidBatchKey] uniqueidentifier NULL,
[vchIsbn] varchar(20) NULL,
CONSTRAINT [FK_tResourcePromoteQueue_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [PK_tResourcePromoteQueue]
PRIMARY KEY CLUSTERED ([iResourcePromoteQueueId] ASC)
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
ALTER TABLE [dbo].[tResourcePromoteQueue] SET (LOCK_ESCALATION = TABLE);
GO

