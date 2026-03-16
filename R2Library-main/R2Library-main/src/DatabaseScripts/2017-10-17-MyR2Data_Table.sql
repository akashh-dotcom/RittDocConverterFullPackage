
/****** Object:  Table [dbo].[tMyR2Data]    Script Date: 10/17/2017 2:20:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tMyR2Data](
	[iMyR2DataId] [int] IDENTITY(1,1) NOT NULL,
	[vchGuidCookieValue] [varchar](50) NULL,
	[iMyR2Type] [int] NOT NULL,
	[vchFolderName] [varchar](255) NOT NULL,
	[tiDefaultFolder] [tinyint] NOT NULL,
	[iInstitutionId] [int] NOT NULL,
	[vchJson] [varchar](2000) NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [datetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [datetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [tMyR2Data_PK] PRIMARY KEY CLUSTERED 
(
	[iMyR2DataId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tMyR2Data]  WITH NOCHECK ADD  CONSTRAINT [tMyR2Data_tInstitution_FK1] FOREIGN KEY([iInstitutionId])
REFERENCES [dbo].[tInstitution] ([iInstitutionId])
GO

ALTER TABLE [dbo].[tMyR2Data] CHECK CONSTRAINT [tMyR2Data_tInstitution_FK1]
GO


