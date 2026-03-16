USE [R2Utilities]

CREATE TABLE [dbo].[PdaResourceEmails](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [varchar](25) NOT NULL,
	[resourceIsbn] [varchar](25) NOT NULL,
	[dateEmailSent] [smalldatetime] NOT NULL,
	[type] [tinyint] NOT NULL
);

USE [R2Utilities]
Alter Table ResourceEmails
Add dateArchivedEmail smalldatetime NULL;
