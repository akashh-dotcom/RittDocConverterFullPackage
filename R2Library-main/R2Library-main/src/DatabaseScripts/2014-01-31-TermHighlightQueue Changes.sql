USE [DEV_R2Utilities]
GO


sp_rename TermHighlightQueue, TabersTermHighlightQueue
GO



/****** Object:  Table [dbo].[IndexTermHighlightQueue]    Script Date: 1/31/2014 12:00:07 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IndexTermHighlightQueue]') AND type in (N'U'))
DROP TABLE [dbo].[IndexTermHighlightQueue]
GO

/****** Object:  Table [dbo].[IndexTermHighlightQueue]    Script Date: 1/31/2014 12:00:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IndexTermHighlightQueue]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IndexTermHighlightQueue](
	[termHighlightQueueId] [int] IDENTITY(1,1) NOT NULL,
	[jobId] [int] NOT NULL,
	[resourceId] [int] NOT NULL,
	[isbn] [varchar](20) NOT NULL,
	[termHighlightStatus] [char](1) NOT NULL,
	[dateAdded] [datetime] NOT NULL,
	[dateStarted] [datetime] NULL,
	[dateFinished] [datetime] NULL,
	[firstDocumentId] [int] NULL,
	[lastDocumentId] [int] NULL,
	[statusMessage] [varchar](2000) NULL,
PRIMARY KEY CLUSTERED 
(
	[termHighlightQueueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
END
GO

SET ANSI_PADDING OFF
GO


