/****** Object: Table [dbo].[tLoginFailure]   Script Date: 1/13/2014 5:11:57 PM ******/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tLoginFailure] (
[iLoginFailureId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NULL,
[tiOctetA] smallint NOT NULL,
[tiOctetB] smallint NOT NULL,
[tiOctetC] smallint NOT NULL,
[tiOctetD] smallint NOT NULL,
[iIpNumericValue] bigint NOT NULL,
[vchCountryCode] char(2) NOT NULL,
[dtLoginFailureDate] datetime NOT NULL,
[vchUsername] varchar(50) NOT NULL,
CONSTRAINT [PK_tLoginFailure]
PRIMARY KEY CLUSTERED ([iLoginFailureId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tLoginFailure] SET (LOCK_ESCALATION = TABLE);
GO

/****** Object: Index [dbo].[tLoginFailure].[IX_tLoginFailure_iIpNumericValue_dtLoginFailureDate]   Script Date: 1/13/2014 5:11:57 PM ******/

CREATE NONCLUSTERED INDEX [IX_tLoginFailure_iIpNumericValue_dtLoginFailureDate]
ON [dbo].[tLoginFailure]
([iIpNumericValue] , [dtLoginFailureDate])
WITH
(
PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ONLINE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE
)
ON [PRIMARY];
GO
