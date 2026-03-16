

CREATE TABLE [dbo].[tResourceAuditType](
	[iResourceAuditTypeId] [tinyint] NOT NULL,
	[AuditTypeDescription] [varchar](50) NOT NULL,
 CONSTRAINT [PK_tResourceAuditType_932F0D0D0D19A41C] PRIMARY KEY CLUSTERED 
(
	[iResourceAuditTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

Insert into tResourceAuditType
values (1, 'Unspecificed instance of resource audit');

CREATE TABLE [dbo].[tResourceAudit](
	[iResourceAuditId] [int] IDENTITY(1,1) NOT NULL,
	[iResourceId] [int] NOT NULL,
	[tiResourceAuditTypeId] [tinyint] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchEventDescription] [varchar](1000) NOT NULL,
 CONSTRAINT [PK_tResourceAudit] PRIMARY KEY CLUSTERED 
(
	[iResourceAuditId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tResourceAudit]  WITH CHECK ADD  CONSTRAINT [FK_tResourceAudit_Type] FOREIGN KEY([tiResourceAuditTypeId])
REFERENCES [dbo].[tResourceAuditType] ([iResourceAuditTypeId])
GO

ALTER TABLE [dbo].[tResourceAudit] CHECK CONSTRAINT [FK_tResourceAudit_Type]
GO


