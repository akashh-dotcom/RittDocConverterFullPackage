/****** Object: Table [dbo].[tDictionaryTerm]   Script Date: 1/6/2014 3:41:29 PM ******/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tDictionaryTerm] (
[iDictionaryTermId] int NOT NULL,
[iDictionaryResourceId] int NOT NULL,
[vchTerm] nvarchar(500) NOT NULL,
[vchSectionId] varchar(50) NOT NULL,
[vchContent] nvarchar(MAX) NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK__tDiction__D84EC905F1654279]
PRIMARY KEY CLUSTERED ([iDictionaryTermId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY];
GO
ALTER TABLE [dbo].[tDictionaryTerm] SET (LOCK_ESCALATION = TABLE);
GO

/****** Object: Index [dbo].[tDictionaryTerm].[IX_tDictionaryTerm_vchTerm]   Script Date: 1/17/2014 4:53:36 PM ******/

CREATE NONCLUSTERED INDEX [IX_tDictionaryTerm_vchTerm]
ON [dbo].[tDictionaryTerm]
([vchTerm])
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


-- select * from tResource where vchResourceTitle like '%taber%'

truncate table tDictionaryTerm

insert into tDictionaryTerm (iDictionaryTermId, iDictionaryResourceId, vchTerm, vchSectionId, vchContent, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus)
    select TermContentKey, 4808, Term, SectionId, [Content], 'sjscheider', getdate(), null, null, 1
    from   TabersDictionary..TermContent
    order by TermContentKey


select * from tDictionaryTerm



EXEC sys.sp_rename @objname = N'[dbo].[TabersMainEntry]', @newname = [_TabersMainEntry_Delete], @objtype = N'OBJECT';
GO
EXEC sys.sp_rename @objname = N'[dbo].[TabersSense]', @newname = [_TabersSense_Delete], @objtype = N'OBJECT';
GO
EXEC sys.sp_rename @objname = N'[dbo].[TabersTermContent]', @newname = [_TabersTermContent_Delete], @objtype = N'OBJECT';
GO

CREATE FULLTEXT CATALOG [TermContentCatalog]WITH ACCENT_SENSITIVITY = ON
GO


ALTER FULLTEXT CATALOG [TermContentCatalog] REORGANIZE
GO

