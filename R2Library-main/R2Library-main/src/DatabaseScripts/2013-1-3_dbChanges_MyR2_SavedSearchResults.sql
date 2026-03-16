USE [RIT001_2012-08-22]
GO

/****** Object:  Table [dbo].[tUserSavedResultsFolders]    Script Date: 1/3/2013 12:56:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tUserSavedResultsFolders](
	[iUserSavedResultsFolderId] [int] IDENTITY(1,1) NOT NULL,
	[vchSavedResultsFolderName] [varchar](255) NOT NULL,
	[tiDefaultFolder] [tinyint] NOT NULL,
	[iUserId] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tUserSavedResultsFolders] PRIMARY KEY CLUSTERED 
(
	[iUserSavedResultsFolderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tUserSavedResultsFolders]  WITH CHECK ADD  CONSTRAINT [FK_tUserSavedResultsFolders_tUser] FOREIGN KEY([iUserId])
REFERENCES [dbo].[tUser] ([iUserId])
GO

ALTER TABLE [dbo].[tUserSavedResultsFolders] CHECK CONSTRAINT [FK_tUserSavedResultsFolders_tUser]
GO




USE [RIT001_2012-08-22]
GO

/****** Object:  Table [dbo].[tUserSavedSearchResults]    Script Date: 1/3/2013 12:56:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tUserSavedSearchResults](
	[iUserSavedSearchResultsId] [int] IDENTITY(1,1) NOT NULL,
	[vchSavedSearchTitle] [varchar](50) NOT NULL,
	[iUserSavedResultsFolderId] [int] NOT NULL,
	[iResultsCount] [int] NOT NULL,
	[vchSearchResultSet] [varchar](max) NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tUserSavedSearchResults] PRIMARY KEY CLUSTERED 
(
	[iUserSavedSearchResultsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tUserSavedSearchResults] ADD  DEFAULT ((0)) FOR [iResultsCount]
GO

ALTER TABLE [dbo].[tUserSavedSearchResults]  WITH CHECK ADD  CONSTRAINT [FK_tUserSavedSearchResults_tUserSavedResultsFolder] FOREIGN KEY([iUserSavedResultsFolderId])
REFERENCES [dbo].[tUserSavedResultsFolders] ([iUserSavedResultsFolderId])
GO

ALTER TABLE [dbo].[tUserSavedSearchResults] CHECK CONSTRAINT [FK_tUserSavedSearchResults_tUserSavedResultsFolder]
GO

