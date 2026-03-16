USE [Dev2_R2Reports]
GO

/****** Object:  Table [dbo].[DailyInstitutionResourceStatisticsCount]    Script Date: 6/9/2015 10:17:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DailyInstitutionResourceStatisticsCount](
	[dailyInstitutionResourceStatisticsCountId] [int] IDENTITY(1,1) NOT NULL,
	[institutionId] [int] NULL,
	[resourceId] [int] NOT NULL,
	[ipAddressInteger] [bigint] NOT NULL,
	[institutionResourceStatisticsDate] [date] NOT NULL,
	[contentRetrievalCount] [int] NULL,
	[tocRetrievalCount] [int] NULL,
	[sessionCount] [int] NULL,
	[printCount] [int] NULL,
	[emailCount] [int] NULL,
	[accessTurnawayCount] [int] NULL,
	[concurrentTurnawayCount] [int] NULL,
 CONSTRAINT [PK_DailyInstitutionResourceStatisticsCount] PRIMARY KEY CLUSTERED 
(
	[dailyInstitutionResourceStatisticsCountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY],
 CONSTRAINT [UI_DailyInstitutionResourceStatisticsCount] UNIQUE NONCLUSTERED 
(
	[institutionId] ASC,
	[resourceId] ASC,
	[ipAddressInteger] ASC,
	[institutionResourceStatisticsDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO





USE [DEV_RIT001]
GO

/****** Object:  View [dbo].[vDailyContentTurnawayCount]    Script Date: 6/9/2015 10:09:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vDailyInstitutionResourceStatisticsCount] AS 
    select dirsc.dailyInstitutionResourceStatisticsCountId
, dirsc.institutionId
, dirsc.resourceId
, dirsc.ipAddressInteger
, dirsc.institutionResourceStatisticsDate
, dirsc.contentRetrievalCount
, dirsc.tocRetrievalCount
, dirsc.sessionCount
, dirsc.printCount
, dirsc.emailCount
, dirsc.accessTurnawayCount
, dirsc.concurrentTurnawayCount
    from   [DEV2_R2Reports].dbo.DailyInstitutionResourceStatisticsCount dirsc 
 

GO


