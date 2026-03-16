
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tInstitutionResourceEmailRecipient]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tInstitutionResourceEmailRecipient];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tInstitutionResourceEmail]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tInstitutionResourceEmail];
END
GO


SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionResourceEmail] (
[iInstitutionResourceEmailId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iResourceId] int NOT NULL,
[iUserId] int NULL,
[vchChapterSectionId] varchar(20) NULL,
[vchUserEmailAddress] varchar(100) NOT NULL,
[vchSubject] varchar(255) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchSessionId] varchar(50) NOT NULL,
[vchRequestId] varchar(50) NOT NULL,
[bQueued] bit NOT NULL,
[vchComments] varchar(2000) NULL,
CONSTRAINT [PK_tInstitutionResourceEmail]
PRIMARY KEY CLUSTERED ([iInstitutionResourceEmailId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceEmail] SET (LOCK_ESCALATION = TABLE);
GO


SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionResourceEmailRecipient] (
[iInstitutionResourceEmailRecipientId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionResourceEmailId] int NOT NULL,
[vchEmailAddress] varchar(100) NOT NULL,
[vchAddressType] char(1) NOT NULL,
CONSTRAINT [PK__tInstitu__5AE08D8C10EA131E]
PRIMARY KEY CLUSTERED ([iInstitutionResourceEmailRecipientId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceEmailRecipient] SET (LOCK_ESCALATION = TABLE);
GO
EXEC [sys].[sp_addextendedproperty] @name = N'MS_Description', @value = N'T=to,C=cc,B=bcc', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tInstitutionResourceEmailRecipient', @level2type = N'COLUMN', @level2name = N'vchAddressType';
GO


