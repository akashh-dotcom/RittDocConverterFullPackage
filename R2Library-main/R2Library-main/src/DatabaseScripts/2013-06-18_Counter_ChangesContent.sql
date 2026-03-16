alter table ContentView
add foundFromSearch tinyint not null default((0));


alter table ContentView
add searchTerm varchar(500) null

ALTER TABLE [dbo].[PageView] ADD [referrer] varchar(1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[PageView] ADD [countryCode] varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

