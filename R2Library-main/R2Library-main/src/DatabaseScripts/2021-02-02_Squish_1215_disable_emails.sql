USE [DEV_RIT001]
GO

/****** Object:  Table [dbo].[tInstitutionResourceLockedPerUser]    Script Date: 2/9/2021 11:09:05 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tInstitutionResourceLockedPerUser](
	[iInstitutionResourceLockedPerUserId] [int] IDENTITY(1,1) NOT NULL,
	[iInstitutionId] [int] NOT NULL,
	[iResourceId] [int] NOT NULL,
 CONSTRAINT [PK_tInstitutionResourceLockedPerUser] PRIMARY KEY CLUSTERED 
(
	[iInstitutionResourceLockedPerUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[tInstitutionResourceLockedPerUser]  WITH CHECK ADD  CONSTRAINT [FK_tInstitutionResourceLockedPerUser_tInstitution] FOREIGN KEY([iInstitutionId])
REFERENCES [dbo].[tInstitution] ([iInstitutionId])
GO

ALTER TABLE [dbo].[tInstitutionResourceLockedPerUser] CHECK CONSTRAINT [FK_tInstitutionResourceLockedPerUser_tInstitution]
GO

ALTER TABLE [dbo].[tInstitutionResourceLockedPerUser]  WITH CHECK ADD  CONSTRAINT [FK_tInstitutionResourceLockedPerUser_tResource] FOREIGN KEY([iResourceId])
REFERENCES [dbo].[tResource] ([iResourceId])
GO

ALTER TABLE [dbo].[tInstitutionResourceLockedPerUser] CHECK CONSTRAINT [FK_tInstitutionResourceLockedPerUser_tResource]
GO





INSERT INTO tInstitutionResourceLockedPerUser
SELECT iInstitutionId, iResourceId
FROM tInstitution i
CROSS JOIN tResource r
WHERE i.vchInstitutionAcctNum = '000867' and r.vchIsbn10 = '0017688396'
	and r.tiRecordStatus = 1


ALTER TABLE tInstitutionResourceLock
ADD iUserId int NULL
GO


