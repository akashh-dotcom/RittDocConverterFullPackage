USE [R2Reports]
GO

/****** Object:  Table [dbo].[ContentView]    Script Date: 10/19/2012 2:44:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

ALTER TABLE  [dbo].[ContentView]
ADD	[actionTypeId] [tinyint] NOT NULL DEFAULT((0))

GO

SET ANSI_PADDING OFF
GO


