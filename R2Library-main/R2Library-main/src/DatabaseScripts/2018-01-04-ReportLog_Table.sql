USE [DEV_RIT001]
GO

/****** Object:  Table [dbo].[tReportLog]    Script Date: 1/4/2018 2:02:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tReportLog](
	[iReportLogId] [int] IDENTITY(1,1) NOT NULL,
	[iReportType] [int] NOT NULL,
	[tiDefaultQuery] [tinyint] NOT NULL,
	[iInstitutionId] [int] NULL,
	[iPeriod] [int] NOT NULL,
	[dtDateRangeStart] [smalldatetime] NOT NULL,
	[dtDateRangeEnd] [smalldatetime] NOT NULL,
	[vchIpFilter] [varchar](max) NULL,
	[iPracticeAreaId] [int] NULL,
	[iPublisherId] [int] NULL,
	[iResourceId] [int] NULL,
	[tiIncludePurchasedTitles] [tinyint] NOT NULL,
	[tiIncludePdaTitles] [tinyint] NOT NULL,
	[tiIncludeTocTitles] [tinyint] NOT NULL,
	[tiIncludeTrialStats] [tinyint] NOT NULL,
	[iInstitutionTypeId] [int] NULL,
	[vchTerritoryCode] [varchar](50) NULL,
	[iSortById] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [tReportLog_PK] PRIMARY KEY CLUSTERED 
(
	[iReportLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


