SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[tPublisher] ADD [vchProductStatement] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

update tPublisher
set    vchProductStatement = 'AAOS resources are available for purchase in the United States and Canada only. Orders placed for this title from all other countries cannot be accepted.'
where  iPublisherId = 114


