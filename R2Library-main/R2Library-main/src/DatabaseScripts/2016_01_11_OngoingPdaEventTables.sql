IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tOngoingPdaEvent]') AND type in (N'U'))
BEGIN
/* Drop constraints script is generated to allow dropping of the table */
IF (OBJECT_ID('[dbo].[FK_tOngoingPdaEventResource_tOngoingPdaEvent]') IS NOT NULL)
  ALTER TABLE [dbo].[tOngoingPdaEventResource] 
    DROP CONSTRAINT [FK_tOngoingPdaEventResource_tOngoingPdaEvent];
DROP TABLE [dbo].[tOngoingPdaEvent];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tOngoingPdaEventResource]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tOngoingPdaEventResource];
END
GO


/****** Object: Table [dbo].[tOngoingPdaEvent]   Script Date: 1/11/2016 12:08:52 PM ******/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tOngoingPdaEvent] (
[iOngoingPdaEventId] int IDENTITY(1, 1) NOT NULL,
[iOngoingPdaEventTypeId] int NOT NULL,
[guidTransactionId] uniqueidentifier NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiProcessed] tinyint NOT NULL,
[iLicenseCountAdded] int NULL,
[vchProcessData] varchar(MAX) NULL,
CONSTRAINT [PK_tOngoingPdaEvent]
PRIMARY KEY CLUSTERED ([iOngoingPdaEventId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY];
GO
ALTER TABLE [dbo].[tOngoingPdaEvent] SET (LOCK_ESCALATION = TABLE);
GO

CREATE TABLE [dbo].[tOngoingPdaEventResource] (
[iOngoingPdaEventResourceId] int IDENTITY(1, 1) NOT NULL,
[iOngoingPdaEventId] int NOT NULL,
[iResourceId] int NOT NULL,
[vchResourceIsbn] varchar(25) NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
CONSTRAINT [PK_tOngoingPdaEventResource]
PRIMARY KEY CLUSTERED ([iOngoingPdaEventResourceId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tOngoingPdaEventResource_tOngoingPdaEvent]
FOREIGN KEY ([iOngoingPdaEventId])
REFERENCES [dbo].[tOngoingPdaEvent] ( [iOngoingPdaEventId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tOngoingPdaEventResource] SET (LOCK_ESCALATION = TABLE);
GO

