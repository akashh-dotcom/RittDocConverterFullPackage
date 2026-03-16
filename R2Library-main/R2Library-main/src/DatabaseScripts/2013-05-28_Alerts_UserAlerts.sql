
CREATE TABLE [dbo].[tAlert](
	[iAlertId] [int] IDENTITY(1,1) NOT NULL,
	[vchTitle] [varchar](50) NULL,
	[vchText] [varchar](max) NULL,
	[tiDisplayOnce] [tinyint] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
	[iLayoutType] [int] NOT NULL,
	[dtStartDate] [smalldatetime] NULL,
	[dtEndDate] [smalldatetime] NULL,
	[vchAlertName] [varchar](100) NULL,
	[iRoleId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[iAlertId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

ALTER TABLE [dbo].[tAlert] ADD  DEFAULT ((1)) FOR [iLayoutType];

ALTER TABLE [dbo].[tAlert] ADD  DEFAULT ((1)) FOR [iRoleId];

ALTER TABLE [dbo].[tAlert]  WITH NOCHECK ADD  CONSTRAINT [tRole_tAlert_FK1] FOREIGN KEY([iRoleId])
REFERENCES [dbo].[tRole] ([iRoleId]);

ALTER TABLE [dbo].[tAlert] CHECK CONSTRAINT [tRole_tAlert_FK1];


CREATE TABLE [dbo].[tAlertImage](
	[iAlertImageId] [int] IDENTITY(1,1) NOT NULL,
	[iAlertId] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
	[vchImageFileName] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[iAlertImageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[tAlertImage]  WITH CHECK ADD FOREIGN KEY([iAlertId])
REFERENCES [dbo].[tAlert] ([iAlertId]);

CREATE TABLE [dbo].[tUserAlert](
	[iUserAlertId] [int] IDENTITY(1,1) NOT NULL,
	[iUserId] [int] NOT NULL,
	[iAlertId] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL
) ON [PRIMARY];

ALTER TABLE [dbo].[tUserAlert]  WITH NOCHECK ADD  CONSTRAINT [iAlertId_tAlert_FK1] FOREIGN KEY([iAlertId])
REFERENCES [dbo].[tAlert] ([iAlertId]);

ALTER TABLE [dbo].[tUserAlert] CHECK CONSTRAINT [iAlertId_tAlert_FK1];

ALTER TABLE [dbo].[tUserAlert]  WITH NOCHECK ADD  CONSTRAINT [iUserId_tUser_FK1] FOREIGN KEY([iUserId])
REFERENCES [dbo].[tUser] ([iUserId]);

ALTER TABLE [dbo].[tUserAlert] CHECK CONSTRAINT [iUserId_tUser_FK1];


--Scott,
--
--I needed to make a change to the tUserAlert table to take into account the user could be a publisher user and the foreign key to the user table needed to be nullable. 
--I also included a foreign key to the tPublisherUser table for those users. 
--
--Below is the script I ran to drop the tUserAlert table and recreate it. 


drop table tUserAlert

/****** Object:  Table [dbo].[tUserAlert]    Script Date: 5/29/2013 9:34:30 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tUserAlert](
       [iUserAlertId] [int] IDENTITY(1,1) NOT NULL,
       [iUserId] [int] NULL,
       [iPublisherUserId] [int] NULL,
       [iAlertId] [int] NOT NULL,
       [vchCreatorId] [varchar](50) NOT NULL,
       [dtCreationDate] [smalldatetime] NOT NULL,
       [vchUpdaterId] [varchar](50) NULL,
       [dtLastUpdate] [smalldatetime] NULL,
       [tiRecordStatus] [tinyint] NOT NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tUserAlert]  WITH NOCHECK ADD  CONSTRAINT [iAlertId_tAlert_FK1] FOREIGN KEY([iAlertId])
REFERENCES [dbo].[tAlert] ([iAlertId])
GO

ALTER TABLE [dbo].[tUserAlert] CHECK CONSTRAINT [iAlertId_tAlert_FK1]
GO

ALTER TABLE [dbo].[tUserAlert]  WITH NOCHECK ADD  CONSTRAINT [iUserId_tUser_FK1] FOREIGN KEY([iUserId])
REFERENCES [dbo].[tUser] ([iUserId])
GO

ALTER TABLE [dbo].[tUserAlert] CHECK CONSTRAINT [iUserId_tUser_FK1]
GO

ALTER TABLE [dbo].[tUserAlert]  WITH NOCHECK ADD  CONSTRAINT [iPublisherUserId_tUser_FK1] FOREIGN KEY([iPublisherUserId])
REFERENCES [dbo].[tPublisherUser] ([iPublisherUserId])
GO

ALTER TABLE [dbo].[tUserAlert] CHECK CONSTRAINT [iPublisherUserId_tUser_FK1]
GO







