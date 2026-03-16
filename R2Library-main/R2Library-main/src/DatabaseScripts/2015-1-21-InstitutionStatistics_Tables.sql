--drop table InstitutionMonthlyStatisticsCount;
--drop table InstitutionMonthlyResourceStatistics;


CREATE TABLE [dbo].[InstitutionMonthlyStatisticsCount](
	[institutionMonthlyStatisticsCountId] [int] IDENTITY(1,1) NOT NULL,
	[institutionId] [int] NOT NULL,
	[aggregationDate] [datetime] not null,
	[mostAccessedResourceId] [int] NULL,
	[mostAccessedCount] [int] NULL,
	[leastAccessedResourceId] [int] NULL,
	[leastAccessedCount] [int] NULL,
	[mostTurnawayConcurrentResourceId] [int] NULL,
	[mostTurnawayConcurrentCount] [int] NULL,
	[mostTurnawayAccessResourceId] [int] NULL,
	[mostTurnawayAccessCount] [int] NULL,
	[mostPopularSpecialtyName] [varchar](100) NULL,
	[mostPopularSpecialtyCount] [int] NULL,
	[leastPopularSpecialtyName] [varchar](100) NULL,
	[leastPopularSpecialtyCount] [int] NULL,
	[totalResourceCount] [int] NULL,
	[contentCount] [int] NULL,
	[tocCount] [int] NULL,
	[sessionCount] [int] NULL,
	[printCount] [int] NULL,
	[emailCount] [int] NULL,
	[turnawayConcurrencyCount] [int] NULL,
	[turnawayAccessCount] [int] NULL,	
 CONSTRAINT [PK__InstitutionMonthlyStatisticsCount] PRIMARY KEY CLUSTERED 
(
	[institutionMonthlyStatisticsCountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
go

CREATE TABLE [dbo].[InstitutionMonthlyResourceStatistics](
	[institutionMonthlyResourceStatisticsId] [int] IDENTITY(1,1) NOT NULL,
	[institutionId] [int] NOT NULL,
	[aggregationDate] [datetime] not null,
	[resourceId] [int] NOT NULL,
	[purchased] [bit] default ((0)),
	[archivedPurchased] [bit] default ((0)),
	[newEditionPreviousPurchased] [bit] default ((0)),
	[pdaAdded] [bit] default ((0)),
	[pdaAddedToCart] [bit] default ((0)),
	[pdaNewEdition] [bit] default ((0)),
	[expertRecommended] [bit] default ((0)),
 CONSTRAINT [PK__InstitutionMonthlyResourceStatisticsId] PRIMARY KEY CLUSTERED 
(
	[institutionMonthlyResourceStatisticsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
go



CREATE VIEW [dbo].[vInstitutionResourceStatistics] AS 
SELECT institutionId
      , aggregationDate
      , resourceId
      , purchased
      , archivedPurchased
      , newEditionPreviousPurchased
      , pdaAdded
      , pdaAddedToCart
      , pdaNewEdition
      , expertRecommended
  FROM [DEV3_R2Reports].dbo.InstitutionMonthlyResourceStatistics

GO

CREATE VIEW [dbo].[vInstitutionStatistics] AS 
SELECT institutionId
      , aggregationDate
      , mostAccessedResourceId
      , mostAccessedCount
      , leastAccessedResourceId
      , leastAccessedCount
      , mostTurnawayConcurrentResourceId
      , mostTurnawayConcurrentCount
      , mostTurnawayAccessResourceId
      , mostTurnawayAccessCount
      , mostPopularSpecialtyName
      , mostPopularSpecialtyCount
      , leastPopularSpecialtyName
      , leastPopularSpecialtyCount
      , totalResourceCount
      , contentCount
      , tocCount
      , sessionCount
      , printCount
      , emailCount
      , turnawayConcurrencyCount
      , turnawayAccessCount
  FROM [DEV3_R2Reports].dbo.InstitutionMonthlyStatisticsCount

GO


alter table tuser
add [tiReceiveDashboardEmail] [tinyint] NOT NULL DEFAULT ((0));

go

update tuser
set [tiReceiveDashboardEmail] = 1
where iRoleId = 1;
