USE [DEV_R2Utilities]
GO

/****** Object:  Table [dbo].[TermHighlightQueue]    Script Date: 12/20/2013 10:48:15 AM ******/
DROP TABLE [dbo].[TermHighlightQueue]
GO

/****** Object:  Table [dbo].[TermHighlightQueue]    Script Date: 12/20/2013 10:48:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TermHighlightQueue](
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

GO

SET ANSI_PADDING OFF
GO


