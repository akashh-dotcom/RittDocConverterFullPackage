USE [R2Reports]
GO

/****** Object:  Table [dbo].[DailyResourceSessionCount]    Script Date: 8/26/2014 10:28:48 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DailyResourceSessionCount](
	[dailyResourceSessionCountId] [int] IDENTITY(1,1) NOT NULL,
	[institutionId] [int] NULL,
	[userId] [int] NULL,
	[ipAddressOctetA] [tinyint] NOT NULL,
	[ipAddressOctetB] [tinyint] NOT NULL,
	[ipAddressOctetC] [tinyint] NOT NULL,
	[ipAddressOctetD] [tinyint] NOT NULL,
	[ipAddressInteger] [bigint] NOT NULL,
	[sessionDate] [date] NOT NULL,
	[sessionCount] [int] NULL,
	[resourceId] [int] NOT NULL,
 CONSTRAINT [PK_DailyResourceSessionCount_dailyResourceSessionCountId] PRIMARY KEY CLUSTERED 
(
	[dailyResourceSessionCountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY],
 CONSTRAINT [UQ_DailyResourceSessionCount_2-3-4-5-6-8-10] UNIQUE NONCLUSTERED 
(
	[institutionId] ASC,
	[userId] ASC,
	[ipAddressOctetA] ASC,
	[ipAddressOctetB] ASC,
	[ipAddressOctetC] ASC,
	[ipAddressOctetD] ASC,
	[sessionDate] ASC,
	[resourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [RIT001]
GO

/****** Object:  View [dbo].[vDailySessionCount]    Script Date: 8/26/2014 9:06:29 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vDailyResourceSessionCount] AS 
    select drsc.dailyResourceSessionCountId, drsc.institutionId, drsc.userId, drsc.ipAddressOctetA, drsc.ipAddressOctetB, drsc.ipAddressOctetC 
         , drsc.ipAddressOctetD, drsc.ipAddressInteger, drsc.sessionDate, drsc.sessionCount, drsc.resourceId
    from   [DEV_R2Reports].dbo.DailyResourceSessionCount drsc
    union 
    select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
         , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), count(distinct pv.sessionId) 
		 , cv.resourceId
    from   [R2Reports].dbo.PageView pv 
	join   [R2Reports].dbo.ContentView cv on pv.requestId = cv.requestId
    where  pageViewTimestamp > '02/01/2014 00:00:00' 
    group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
           , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), cv.resourceId


GO


--Production
-- Insert Into [{0}]..DailyResourceSessionCount (institutionId, userId, ipAddressOctetA, ipAddressOctetB,
    -- ipAddressOctetC, ipAddressOctetD, ipAddressInteger, sessionDate, sessionCount, resourceId)
-- select pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC
     -- , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), count(distinct pv.sessionId), cv.resourceId
-- from  TEMP_R2Reports..PageView pv 
-- join TEMP_R2Reports..ContentView cv on pv.requestId = cv.requestId
-- group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
     -- , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), cv.resourceId
