--USE [DEV_RIT001]
GO

/****** Object:  Table [dbo].[tInstitutionCrawlerBypass]    Script Date: 5/20/2014 12:52:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tInstitutionCrawlerBypass](
	[iInstitutionCrawlerBypassId] [int] IDENTITY(1,1) NOT NULL,
	[iInstitutionId] [int] NOT NULL,
	[tiOctetA] [smallint] NOT NULL,
	[tiOctetB] [smallint] NOT NULL,
	[tiOctetC] [smallint] NOT NULL,
	[tiOctetD] [smallint] NOT NULL,
	[iDecimal] [bigint] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
	[vchUserAgent] [varchar](255) NULL,
 CONSTRAINT [PK_tInstitutionCrawlerBypass_iInstitutionCrawlerBypassId] PRIMARY KEY CLUSTERED 
(
	[iInstitutionCrawlerBypassId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tInstitutionCrawlerBypass]  WITH CHECK ADD  CONSTRAINT [FK_tInstitutionCrawlerBypass_tInstitution_iInstitutionId] FOREIGN KEY([iInstitutionId])
REFERENCES [dbo].[tInstitution] ([iInstitutionId])
GO

ALTER TABLE [dbo].[tInstitutionCrawlerBypass] CHECK CONSTRAINT [FK_tInstitutionCrawlerBypass_tInstitution_iInstitutionId]
GO
