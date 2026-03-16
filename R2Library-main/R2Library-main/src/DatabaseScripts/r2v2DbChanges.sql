-- fix orphan users after backup
--sp_change_users_login @Action='Report';
--EXEC sp_change_users_login 'Update_One', 'R2WebUser'
--sp_change_users_login @Action='update_one', @LoginName='R2WebUser'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tResourceFile]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tResourceFile];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tResourceFile] (
[iResourceFileId] int IDENTITY(1, 1) NOT NULL,
[iResourceId] int NOT NULL,
[vchFileNameFull] varchar(50) NOT NULL,
[vchFileNamePart1] varchar(50) NOT NULL,
[vchFileNamePart3] varchar(50) NULL,
[iDocumentId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] int NOT NULL,
CONSTRAINT [PK_tResourceFile]
PRIMARY KEY CLUSTERED ([iResourceFileId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tResourceFile_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [UK_tResourceFile_iDocumentId]
UNIQUE NONCLUSTERED ([iDocumentId] ASC)
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


-- DON"T DO THIS!!!
--update tResource
--set    vchResourceISBN = ltrim(rtrim(vchResourceISBN)) 


/****** Object: View [dbo].[vResourceFileDocIds]   Script Date: 2/3/2012 3:52:42 PM ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vResourceFileDocIds]'))
BEGIN
DROP VIEW [dbo].[vResourceFileDocIds];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE VIEW [dbo].[vResourceFileDocIds]
AS
select r.iResourceId as [iResourceId]
     , min(rf.iDocumentId) as [iMinDocumentId]
     , max(rf.iDocumentId) as [iMaxDocumentId]
from   dbo.tResource r
 join  dbo.tResourceFile rf on r.iResourceId = rf.iResourceId
group by r.iResourceId
GO


/****** Object: Table [dbo].[tAuthor]   Script Date: 2/3/2012 3:48:31 PM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tAuthor]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tAuthor];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tAuthor] (
[iAuthorId] int IDENTITY(1, 1) NOT NULL,
[iResourceId] int NOT NULL,
[vchFirstName] varchar(100) NULL,
[vchLastName] varchar(100) NOT NULL,
[vchMiddleName] varchar(20) NULL,
[vchLineage] varchar(20) NULL,
[vchDegree] varchar(255) NULL,
[tiAuthorOrder] tinyint NOT NULL,
CONSTRAINT [PK_tAuthor]
PRIMARY KEY CLUSTERED ([iAuthorId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON )
 ON [PRIMARY],
CONSTRAINT [FK_dbo_tAuthor_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] )
)
ON [PRIMARY];
GO



/****** Object: Table [dbo].[tEditor]   Script Date: 2/3/2012 3:49:12 PM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tEditor]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tEditor];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tEditor] (
[iEditorId] int IDENTITY(1, 1) NOT NULL,
[iResourceId] int NOT NULL,
[vchFirstName] varchar(100) NULL,
[vchLastName] varchar(100) NULL,
[vchMiddleName] varchar(20) NULL,
[vchLineage] varchar(20) NULL,
[vchDegree] varchar(255) NULL,
[tiEditorOrder] tinyint NOT NULL,
CONSTRAINT [PK_tEditor]
PRIMARY KEY CLUSTERED ([iEditorId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON )
 ON [PRIMARY],
CONSTRAINT [FK_tEditor_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] )
)
ON [PRIMARY];
GO


/****** Object: Table [dbo].[tEditorAffiliation]   Script Date: 2/3/2012 3:49:39 PM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tEditorAffiliation]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tEditorAffiliation];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tEditorAffiliation] (
[iEditorAffiliationId] int IDENTITY(1, 1) NOT NULL,
[iEditorId] int NOT NULL,
[vchJobTitle] varchar(1000) NULL,
[vchOrganization] varchar(1000) NULL,
[tiAffiliationOrder] tinyint NOT NULL,
CONSTRAINT [PK_tEditorAffiliation]
PRIMARY KEY CLUSTERED ([iEditorAffiliationId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON )
 ON [PRIMARY],
CONSTRAINT [FK_tEditorAffiliation_tEditor]
FOREIGN KEY ([iEditorId])
REFERENCES [dbo].[tEditor] ( [iEditorId] )
)
ON [PRIMARY];
GO



truncate table tAuthor
truncate table tEditorAffiliation
delete from tEditor
DBCC CHECKIDENT (tEditor, RESEED, 0)


/****** Object: View [dbo].[vInstitutionResourceSearchFilter]   Script Date: 2/10/2012 3:27:39 PM ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vInstitutionResourceSearchFilter]'))
BEGIN
DROP VIEW [dbo].[vInstitutionResourceSearchFilter];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE VIEW [dbo].[vInstitutionResourceSearchFilter]
AS
select r.iResourceId, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors, r.vchResourceAdditionalContributors
     , r.vchResourcePublisher, r.dtRISReleaseDate, r.dtResourcePublicationDate, r.tiBrandonHillStatus
     , r.vchResourceEdition, r.decResourcePrice, r.decPayPerView, r.decSubScriptionPrice, r.vchResourceImageName, r.vchCopyRight
     , r.tiResourceReady, r.tiAllowSubscriptions, r.iPublisherId, r.iResourceStatusId, r.tiGloballyAccessible
     , r.tiRecordStatus as [tiResourceRecordStatus], r.vchResourceNLMCall, r.tiDrugMonograph
     , r.iDCTStatusId, r.tiDoodyReview, r.vchDoodyReviewURL, r.iPrevEditResourceID, r.vchForthcomingDate
     , ir.iInstitutionResourceId, ir.iInstitutionId, ir.tiRecordStatus as [tiInstitutionResourceRecordStatus]
     , ril.iResourceInstLicenseId, ril.iNumberLicenses, ril.tiRecordStatus as [tiResourceInstLicenseRecordStatus]
     , rIsbn.vchIsbn as [vchIsbn10]
     , min(rf.iDocumentId) as [iMinDocumentId], max(rf.iDocumentId) as [iMaxDocumentId]
     , (select vchIsbn from tResourceIsbn rIsbn2 where rIsbn2.iResourceIsbnTypeId = 2 and rIsbn2.iResourceId = r.iResourceId) as [vchIsbn13]
from   tResource r
 join  dbo.tInstitutionResource ir on ir.iResourceId = r.iResourceId
 join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId
 join  dbo.tResourceIsbn rIsbn on rIsbn.iResourceId = r.iResourceId
 join  dbo.tResourceFile rf on rf.iResourceId = r.iResourceId
where  r.tiRecordStatus = 1 
  and  ir.tiRecordStatus = 1
  and  rIsbn.iResourceIsbnTypeId = 1
group by r.iResourceId, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors, r.vchResourceAdditionalContributors
     , r.vchResourcePublisher, r.dtRISReleaseDate, r.dtResourcePublicationDate, r.tiBrandonHillStatus
     , r.vchResourceEdition, r.decResourcePrice, r.decPayPerView, r.decSubScriptionPrice, r.vchResourceImageName, r.vchCopyRight
     , r.tiResourceReady, r.tiAllowSubscriptions, r.iPublisherId, r.iResourceStatusId, r.tiGloballyAccessible
     , r.tiRecordStatus, r.vchResourceNLMCall, r.tiDrugMonograph
     , r.iDCTStatusId, r.tiDoodyReview, r.vchDoodyReviewURL, r.iPrevEditResourceID, r.vchForthcomingDate
     , ir.iInstitutionResourceId, ir.iInstitutionId, ir.tiRecordStatus
     , ril.iResourceInstLicenseId, ril.iNumberLicenses, ril.tiRecordStatus
     , rIsbn.vchIsbn
GO

/****** Object: View [dbo].[vInstitutionResourceLicense]   Script Date: 4/25/2012 5:44:48 PM ******/
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vInstitutionResourceLicense]'))
BEGIN
DROP VIEW [dbo].[vInstitutionResourceLicense];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE VIEW [dbo].[vInstitutionResourceLicense]
AS
select i.iInstitutionId, r.iResourceId--, r.iResourceStatusId
     , sum(ril.iNumberLicenses) as iLicenseCount
from   tInstitution i
 join  dbo.tInstitutionResource ir on ir.iInstitutionId = i.iInstitutionId and ir.tiRecordStatus = 1
 join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId and ril.tiRecordStatus = 1
 join  dbo.tResource r on r.iResourceId = ir.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7)
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 1
group by i.iInstitutionId, r.iResourceId, r.iResourceStatusId
union
select i.iInstitutionId, r.iResourceId --,r.iResourceStatusId
     , 100 as iLicenseCount
from   tInstitution i
 join  tResource r on r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7) and r.NotSaleable = 0
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 2
  and  getdate() between i.dtTrialAcctStart and i.dtTrialAcctEnd
GO


/****** Object: Table [dbo].[tIpAddressRange]   Script Date: 2/10/2012 3:31:41 PM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tIpAddressRange]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[tIpAddressRange];
END
GO

--/****** Object: Default [dbo].[Active]   Script Date: 2/10/2012 3:31:41 PM ******/
--IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Active]') AND OBJECTPROPERTY(object_id, N'IsDefault') = 1)
--BEGIN
--DROP DEFAULT [dbo].[Active];
--END
--GO
--
--SET QUOTED_IDENTIFIER ON;
--GO
--CREATE DEFAULT [dbo].[Active] AS 1
--GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tIpAddressRange] (
[iIpAddressId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[tiOctetA] smallint NOT NULL,
[tiOctetB] smallint NOT NULL,
[tiOctetCStart] smallint NOT NULL,
[tiOctetCEnd] smallint NOT NULL,
[tiOctetDStart] smallint NOT NULL,
[tiOctetDEnd] smallint NOT NULL,
[iDecimalStart] bigint NOT NULL,
[iDecimalEnd] bigint NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK__tIpAddressRange__092A4EB5]
PRIMARY KEY CLUSTERED ([iIpAddressId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON )
 ON [PRIMARY],
CONSTRAINT [FK__tIpAddres__iInst__0A1E72EE]
FOREIGN KEY ([iInstitutionId])
REFERENCES [dbo].[tInstitution] ( [iInstitutionId] )
)
ON [PRIMARY];
GO

-- populate new ip address range table with data from tValidIpAddress
insert into tIpAddressRange (iInstitutionId
  , tiOctetA
  , tiOctetB
  , tiOctetCStart
  , tiOctetCEnd
  , tiOctetDStart
  , tiOctetDEnd
  , iDecimalStart, iDecimalEnd, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus)
    select v.iInstitutionId
         , cast(left(v.vchIpStartRange, charindex('.', v.vchIpStartRange, 0) - 1) as int) as iOctetA
         , cast(substring(v.vchIpStartRange, charindex('.', v.vchIpStartRange, 0) + 1, charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, 0) + 1) - charindex('.', v.vchIpStartRange, 0) - 1) as int) as iOctetB
         , cast(substring(v.vchIpStartRange, charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, 0) + 1) + 1, charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, 0) + 1) + 1) - charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, 0) + 1) - 1) as int) as iOctetCStart
         , cast(substring(v.vchIpEndRange, charindex('.', v.vchIpEndRange, charindex('.', v.vchIpEndRange, 0) + 1) + 1, charindex('.', v.vchIpEndRange, charindex('.', v.vchIpEndRange, charindex('.', v.vchIpEndRange, 0) + 1) + 1) - charindex('.', v.vchIpEndRange, charindex('.', v.vchIpEndRange, 0) + 1) - 1) as int) as iOctetCEnd
         , cast(right(v.vchIpStartRange, len(v.vchIpStartRange) - charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, charindex('.', v.vchIpStartRange, 1) + 1 + 1) + 1)) as int) as iOctetDStart
         , cast(right(v.vchIpEndRange, len(v.vchIpEndRange) - charindex('.', v.vchIpEndRange, charindex('.', v.vchIpEndRange, charindex('.', v.vchIpEndRange, 1) + 1 + 1) + 1)) as int) as iOctetDEnd
         , 0 as iDecimalStart, 0 as iDecimalEnd, v.vchCreatorId, v.dtCreationDate, v.vchUpdaterId, v.dtLastUpdate, v.tiRecordStatus
    from   dbo.tValidIpAddress v
    where  tiRecordStatus = 1
    order by 2, 3, 4, 5, 6, 7

-- set the decimal start and end dates
update tIpAddressRange
set    iDecimalStart = cast((cast(tiOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((tiOctetB * 256 * 256) as bigint) 
                     + cast((tiOctetCStart * 256) as bigint) 
                     + cast(tiOctetDStart as bigint)
     , iDecimalEnd   = cast((cast(tiOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((tiOctetB * 256 * 256) as bigint) 
                     + cast((tiOctetCEnd * 256) as bigint) 
                     + cast(tiOctetDEnd as bigint)


-- select * from tIpAddressRange



ALTER TABLE [dbo].[tResourceIsbn]
DROP CONSTRAINT [FK_tResourceIsbn_tResourceIsbn]
GO

-----------------------------------------------
-----------------------------------------------
-- POST PREVIEW CHANGE! 5/22/2012
-----------------------------------------------
-----------------------------------------------



-- A to Z INDEX TABLE CHANGES
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tAtoZIndexType] (
[iAtoZIndexTypeId] int NOT NULL,
[vchDescription] varchar(50) NOT NULL,
[vchTableName] varchar(50) NOT NULL,
CONSTRAINT [PK_dbo_tAtoZIndexType_1]
PRIMARY KEY CLUSTERED ([iAtoZIndexTypeId] ASC)
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


SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tAtoZIndex] (
[iAtoZIndexId] int IDENTITY(1, 1) NOT NULL,
[iParentTableId] int NOT NULL,
[vchName] varchar(250) NOT NULL,
[chrAlphaKey] char(1) NOT NULL,
[iResourceId] int NOT NULL,
[vchResourceISBN] varchar(50) NULL,
[vchChapterId] varchar(50) NULL,
[vchSectionId] varchar(50) NULL,
[iAtoZIndexTypeId] int NOT NULL,
CONSTRAINT [PK__tAtoZInd__9B90EB6844F8E12D]
PRIMARY KEY CLUSTERED ([iAtoZIndexId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK__tAtoZInde__iAtoZ__6C5905DD]
FOREIGN KEY ([iAtoZIndexTypeId])
REFERENCES [dbo].[tAtoZIndexType] ( [iAtoZIndexTypeId] )
)
ON [PRIMARY];
GO

/****** Object: Index [dbo].[tAtoZIndex].[IX_tAtoZIndex_AlphaKey]   Script Date: 5/22/2012 1:01:15 PM ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[tAtoZIndex]') AND name = N'IX_tAtoZIndex_AlphaKey')
BEGIN
DROP INDEX [IX_tAtoZIndex_AlphaKey] ON [dbo].[tAtoZIndex];

END
GO


CREATE NONCLUSTERED INDEX [IX_tAtoZIndex_AlphaKey]
ON [dbo].[tAtoZIndex]
([chrAlphaKey])
INCLUDE ([vchName], [iResourceId])
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
/****** Object: Index [dbo].[tAtoZIndex].[IX_tAtoZIndex_ResourceId]   Script Date: 5/22/2012 1:01:15 PM ******/
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[tAtoZIndex]') AND name = N'IX_tAtoZIndex_ResourceId')
BEGIN
DROP INDEX [IX_tAtoZIndex_ResourceId] ON [dbo].[tAtoZIndex];

END
GO


CREATE NONCLUSTERED INDEX [IX_tAtoZIndex_ResourceId]
ON [dbo].[tAtoZIndex]
([iResourceId])
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

-- Beta - check point 1


insert into tAtoZIndexType (iAtoZIndexTypeId, vchDescription, vchTableName) values (1, 'Disease', 'tDiseaseName')
insert into tAtoZIndexType (iAtoZIndexTypeId, vchDescription, vchTableName) values (2, 'Disease Synonym', 'tDiseaseSynonym')
insert into tAtoZIndexType (iAtoZIndexTypeId, vchDescription, vchTableName) values (3, 'Drug', 'tDrugResource')
insert into tAtoZIndexType (iAtoZIndexTypeId, vchDescription, vchTableName) values (4, 'Drug Synonym', 'tDrugSynonymResource')
insert into tAtoZIndexType (iAtoZIndexTypeId, vchDescription, vchTableName) values (5, 'Keyword', 'tKeywordResource')

--select count(*) from tAtoZIndex

--truncate table tAtoZIndex
--select * from tAtoZIndex where chrAlphaKey = '' or chrAlphaKey = ' '

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select dn.iDiseaseNameId AS Id, dn.vchDiseaseName as Name
         , upper(substring(ltrim(rtrim(dn.vchDiseaseName)), 0, 2)) as AlphaKey
         , r.iResourceId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, 1
      from tdiseasename dn
        join tDiseaseResource dr on dr.iDiseaseNameId = dn.iDiseaseNameId
        join dbo.tResource r on r.vchResourceISBN = dr.vchResourceISBN

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select ds.iDiseaseSynonymId as Id, ds.vchDiseaseSynonym as Name
         , upper(substring(ltrim(rtrim(ds.vchDiseaseSynonym)), 0, 2)) as AlphaKey
         , r.iResourceId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, 2
      from tdiseasesynonym ds
        join dbo.tDiseaseSynonymResource dsr on dsr.iDiseaseSynonymId = ds.iDiseaseSynonymId
        join dbo.tResource r on r.vchResourceISBN = dsr.vchResourceISBN

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select dl.iDrugListId as Id, dl.vchDrugName as Name
         , upper(substring(ltrim(rtrim(dl.vchDrugName)), 0, 2)) as AlphaKey
         , r.iResourceId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, 3
      from tDrugsList dl
        join dbo.tDrugResource dr on dr.iDrugListId = dl.iDrugListId
        join dbo.tResource r on r.vchResourceISBN = dr.vchResourceISBN

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select ds.iDrugSynonymId as Id, ds.vchDrugSynonymName as Name
         , upper(substring(ltrim(rtrim(ds.vchDrugSynonymName)), 0, 2)) as AlphaKey
         , r.iResourceId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, 4
      from tDrugSynonym ds
        join dbo.tDrugSynonymResource dsr on dsr.iDrugSynonymId = ds.iDrugSynonymId
        join dbo.tResource r on r.vchResourceISBN = dsr.vchResourceISBN

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select k.iKeywordId as Id, k.vchKeywordDesc as Name
         , upper(substring(ltrim(rtrim(k.vchKeywordDesc)), 0, 2)) as AlphaKey
         , r.iResourceId, kr.vchResourceISBN, kr.vchChapterId, kr.vchSectionId, 5
      from tKeyword k
        join dbo.tKeywordResource kr on kr.iKeywordId = k.iKeywordId
        join dbo.tResource r on r.vchResourceISBN = kr.vchResourceISBN

-- beta check point 2


-- Add sort title to resource
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[tResource] ADD [vchResourceSortTitle] varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[tResource] ADD [chrAlphaKey] char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

update tResource
set    vchResourceSortTitle = 
       case when (substring(ltrim(vchResourceTitle), 1, 2) = 'A ') 
                  then substring(ltrim(vchResourceTitle), 3, 250) + ', A'
            when (substring(ltrim(vchResourceTitle), 1, 3) = 'AN ') 
                  then substring(ltrim(vchResourceTitle), 4, 250) + ', AN'
            when (substring(ltrim(vchResourceTitle), 1, 4) = 'THE ') 
                  then substring(ltrim(vchResourceTitle), 5, 250) + ', THE'
       else ltrim(vchResourceTitle)
       end

update tResource
set    chrAlphaKey = substring(vchResourceSortTitle, 1, 1)


--DROP INDEX
--    IX_tResource_ThreeStatusFields ON [dbo].[tResource]

-- INDEXES TO IMPROVE PERFORMANCE
CREATE NONCLUSTERED INDEX [IX_tResource_ThreeStatusFields]
ON [dbo].[tResource] ([tiRecordStatus],[NotSaleable],[iResourceStatusId])
INCLUDE ([iResourceId],[chrAlphaKey])
GO

CREATE NONCLUSTERED INDEX [IX_tResourceInstLicense_Status]
ON [dbo].[tResourceInstLicense] ([tiRecordStatus])
INCLUDE ([iNumberLicenses],[iInstitutionResourceId])

--
-- ADD ISBN 10, ISBN 13 & EISBN to tResource
-- 6/5/2012
ALTER TABLE [dbo].[tResource] ADD [vchIsbn10] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[tResource] ADD [vchIsbn13] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[tResource] ADD [vchEIsbn] varchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

update r
set    vchIsbn10 = i.vchIsbn
from   tResource as r
 join  dbo.tResourceIsbn i on i.iResourceId = r.iResourceId and i.iResourceIsbnTypeId = 1

update r
set    vchIsbn13 = i.vchIsbn
from   tResource as r
 join  dbo.tResourceIsbn i on i.iResourceId = r.iResourceId and i.iResourceIsbnTypeId = 2

update r
set    vchEIsbn = i.vchIsbn
from   tResource as r
 join  dbo.tResourceIsbn i on i.iResourceId = r.iResourceId and i.iResourceIsbnTypeId = 3



-- beta check point 3




BEGIN TRANSACTION
GO
ALTER TABLE dbo.tResource ADD
	vchResourceSortAuthor varchar(255) NULL
GO
ALTER TABLE dbo.tResource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

update tresource 
set tresource.vchResourceSortAuthor =tAuthor.vchLastName + ', ' +tAuthor.vchFirstName
from tAuthor
where tAuthor.iresourceid =tresource.iresourceid and tAuthor.tiAuthorOrder=1


-- views for reports
CREATE VIEW [dbo].[vDailyContentTurnawayCount]
AS
select dctc.dailyContentTurnawayCountId, dctc.institutionId, dctc.userId, dctc.resourceId, dctc.chapterSectionId,
       dctc.turnawayTypeId, dctc.ipAddressOctetA, dctc.ipAddressOctetB, dctc.ipAddressOctetC, dctc.ipAddressOctetD,
       dctc.ipAddressInteger, dctc.contentTurnawayDate, dctc.contentTurnawayCount
from   R2Reports.dbo.DailyContentTurnawayCount dctc
GO

CREATE VIEW [dbo].[vDailyContentViewCount]
AS
select dcvc.dailyContentViewCountId, dcvc.institutionId, dcvc.userId, dcvc.resourceId, dcvc.chapterSectionId, dcvc.ipAddressOctetA,
       dcvc.ipAddressOctetB, dcvc.ipAddressOctetC, dcvc.ipAddressOctetD, dcvc.ipAddressInteger, dcvc.contentViewDate, dcvc.contentViewCount
from   R2Reports.dbo.DailyContentViewCount dcvc
GO

CREATE VIEW [dbo].[vDailyPageViewCount]
AS
select dpvc.dailyPageViewCountId, dpvc.institutionId, dpvc.userId, dpvc.ipAddressOctetA, dpvc.ipAddressOctetB,
       dpvc.ipAddressOctetC, dpvc.ipAddressOctetD, dpvc.ipAddressInteger, dpvc.pageViewDate, dpvc.pageViewCount
from   R2Reports.dbo.DailyPageViewCount dpvc
GO

CREATE VIEW [dbo].[vDailySearchCount]
AS
select dsc.dailySearchCountId, dsc.institutionId, dsc.userId, dsc.searchTypeId, dsc.isArchive, dsc.isExternal, dsc.ipAddressOctetA,
       dsc.ipAddressOctetB, dsc.ipAddressOctetC, dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.searchDate, dsc.searchCount
from   R2Reports.dbo.DailySearchCount dsc
GO

CREATE VIEW [dbo].[vDailySessionCount]
AS
select dsc.dailySessionCountId, dsc.institutionId, dsc.userId, dsc.ipAddressOctetA, dsc.ipAddressOctetB, dsc.ipAddressOctetC,
       dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.sessionDate, dsc.sessionCount
from   R2Reports.dbo.DailySessionCount dsc
GO


-- FullText Index for user search
-- From: User_FullText_Catalog-Indexes.sql
-- Added by Scott on 7/23/2012
/****** Object:  FullTextCatalog [[UserSearch]    Script Date: 6/28/2012 11:52:08 AM ******/
CREATE FULLTEXT CATALOG [UserSearch]WITH ACCENT_SENSITIVITY = OFF

GO

CREATE FULLTEXT INDEX ON [dbo].[tUser]
(
	[vchLastName] Language 1033,
	[vchFirstName] Language 1033,
	[vchUserEmail] Language 1033
)
KEY INDEX [tUser_PK] on [UserSearch]
GO

CREATE FULLTEXT INDEX ON [dbo].[tInstitution]
(
	[vchInstitutionName] Language 1033
)

KEY INDEX [tInstitution_PK] on [UserSearch]
GO



-- add last session date to tUser
-- From: User-LastSessionColumn and Update.sql
-- Added by Scott on 7/23/2012
ALTER TABLE tuser
  ADD dtLastSession  smalldatetime;

update tuser
set dtLastSession = (select top(1) a.dtSessionStartTime from tApplicationSession a
where u.iuserId = a.iUserId and a.iUserId is not null
order by a.dtSessionStartTime desc)
from tuser u


-- Cart Changes
-- From Cart.sql
-- Added by Scott on 7/23/2012

--drop table [RIT001_2012-03-21].[dbo].tCartItem
--drop table [RIT001_2012-03-21].[dbo].tCart
--drop table [RIT001_2012-03-21].[dbo].tProduct
--
--select * from tCart
--
--select * from tCartItem

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tCart] (
[iCartId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[vchPurchaseOrderNumber] varchar(50) NULL,
[vchPurchaseOrderComment] varchar(250) NULL,
[tiProcessed] tinyint NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
[dtPurchaseDate] datetime NULL,
[tiBillingMethod] tinyint NULL,
CONSTRAINT [PK_tCart]
PRIMARY KEY CLUSTERED ([iCartId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tCart_tInstitution]
FOREIGN KEY ([iInstitutionId])
REFERENCES [dbo].[tInstitution] ( [iInstitutionId] )
)
ON [PRIMARY];
GO


SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tProduct] (
[iProductId] int IDENTITY(1, 1) NOT NULL,
[vchProductName] varchar(250) NOT NULL,
[decPrice] decimal(12, 2) NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tProduct]
PRIMARY KEY CLUSTERED ([iProductId] ASC)
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


CREATE TABLE [dbo].[tCartItem] (
[iCartItemId] int IDENTITY(1, 1) NOT NULL,
[iCartId] int NOT NULL,
[iResourceId] int NULL,
[iNumberOfLicenses] int NOT NULL,
[decPricePerLicense] decimal(12, 2) NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
[decListPrice] decimal(12, 2) NULL,
[decDiscountPrice] decimal(12, 2) NULL,
[iProductId] int NULL,
CONSTRAINT [PK_tCartItem]
PRIMARY KEY CLUSTERED ([iCartItemId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tCartItem_tCart]
FOREIGN KEY ([iCartId])
REFERENCES [dbo].[tCart] ( [iCartId] ),
CONSTRAINT [FK_tCartItem_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [FK_tCartItem_tProduct]
FOREIGN KEY ([iProductId])
REFERENCES [dbo].[tProduct] ( [iProductId] )
)
ON [PRIMARY];
GO

insert into tProduct(vchProductName, decPrice, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) VALUES
('Annual Maintenance Fee', 1200.00, 'jharvey', '7/20/2012 11:47', NULL, NULL, 1);

--------------------------------------------------------------------------------
-- END OF BETA RELEASE CHANGES 
--------------------------------------------------------------------------------

--------------------------------------------------------------------------------
-- START OF POST-BETA RELEASE CHANGES
--------------------------------------------------------------------------------

-- Ken - 7/26/2012 - fix column name
sp_RENAME 'tPublisherUser.vchUpaterId', 'vchUpdaterId', 'COLUMN';


-- Scott - 7/27/2012 - added date of first license purchased to view
ALTER VIEW [dbo].[vInstitutionResourceLicense]
AS
select i.iInstitutionId, r.iResourceId--, r.iResourceStatusId
     , sum(ril.iNumberLicenses) as iLicenseCount
     , min(ril.dtCreationDate) as [dtFirstPurchaseDate]
from   tInstitution i
 join  dbo.tInstitutionResource ir on ir.iInstitutionId = i.iInstitutionId and ir.tiRecordStatus = 1
 join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId and ril.tiRecordStatus = 1
 join  dbo.tResource r on r.iResourceId = ir.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7)
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 1
group by i.iInstitutionId, r.iResourceId, r.iResourceStatusId
union
select i.iInstitutionId, r.iResourceId --,r.iResourceStatusId
     , 100 as iLicenseCount, '1/1/2000' as [dtFirstPurchaseDate]
from   tInstitution i
 join  tResource r on r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7) and r.NotSaleable = 0
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 2
  and  getdate() between i.dtTrialAcctStart and i.dtTrialAcctEnd
GO

------------------------------------------------------------------------------
-- 8/6/2012
-- alter view again to set trial licenses to 3 - it was 100
ALTER VIEW [dbo].[vInstitutionResourceLicense]
AS
select i.iInstitutionId, r.iResourceId--, r.iResourceStatusId
     , sum(ril.iNumberLicenses) as iLicenseCount
     , min(ril.dtCreationDate) as [dtFirstPurchaseDate]
from   tInstitution i
 join  dbo.tInstitutionResource ir on ir.iInstitutionId = i.iInstitutionId and ir.tiRecordStatus = 1
 join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId and ril.tiRecordStatus = 1
 join  dbo.tResource r on r.iResourceId = ir.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7)
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 1
group by i.iInstitutionId, r.iResourceId, r.iResourceStatusId
union
select i.iInstitutionId, r.iResourceId --,r.iResourceStatusId
     , 3 as iLicenseCount, '1/1/2000' as [dtFirstPurchaseDate]
from   tInstitution i
 join  tResource r on r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7) and r.NotSaleable = 0
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 2
  and  getdate() between i.dtTrialAcctStart and i.dtTrialAcctEnd
GO

-----------------------------------------------------
-- 8/7/2012 - Cart changes from James
alter table tcart
add  decInstDiscount [decimal](12, 2)


-------------------------------------------------------
-- 8/9/2012 - search history and saved search changes
ALTER TABLE [dbo].[tUserSearchHistory] ADD [vchSearchQuery] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[tUserSavedSearch] ADD [vchSearchQuery] varchar(2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

ALTER TABLE [dbo].[tUserSearchHistory] ADD [iResultsCount]  int default 0
GO
ALTER TABLE [dbo].[tUserSavedSearch] ADD [iResultsCount] int default 0
GO






----------------------------------------------------------
-- 8/15/2012 - Order History migration - James


ALTER TABLE dbo.tCart ADD
	iPoCommentId int NULL
GO

GO
ALTER TABLE dbo.tCartItem ADD
	iResourceInstLicenseId int NULL
GO

ALTER TABLE dbo.tCartItem
	DROP COLUMN decPricePerLicense
GO



insert into tCart (iInstitutionId, vchPurchaseOrderNumber, vchPurchaseOrderComment, tiProcessed, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, dtPurchaseDate, tiBillingMethod, decInstDiscount, iPoCommentId)
select po.iInstitutionId, po.vchPoNumber, po.vchPoComment, 1, po.vchCreatorId, po.dtCreationDate, po.vchUpdaterId, po.dtLastUpdate, po.tiRecordStatus, po.dtCreationDate, null, i.decInstDiscount, po.iPoCommentId from tpocomment po
join tInstitution i on i.iInstitutionId = po.iInstitutionId




insert tcartitem (icartid,iresourceid,iNumberOfLicenses,vchCreatorId,dtCreationDate,vchUpdaterId, dtLastUpdate,tiRecordStatus,decListPrice,decDiscountPrice,iProductId,ishoppingcartid,iResourceinstlicenseId)
select c.iCartId, ir.iResourceId, ril.iNumberLicenses, ril.vchCreatorId, ril.dtCreationDate, ril.vchUpdaterId, ril.dtLastUpdate,ril.tiRecordStatus, decLicenseAmt, decLicenseAmt, null, null, ril.iResourceInstLicenseId 
 from tPoComment po
JOIN tResourceInstLicense ril on ril.vchPoNumber = po.vchPoNumber
join tInstitutionResource ir on ir.iInstitutionResourceId = ril.iInstitutionResourceId
join tcart c on c.ipocommentid = po.iPoCommentId 
where po.dtCreationDate = ril.dtCreationDate 




update tcartitem set decDiscountPrice = decListPrice-((c.decInstDiscount/100)*decListPrice)*iNumberOfLicenses From tcart c, tcartitem ci 
where ci.icartid=c.icartid and c.ipocommentid is not null






insert tcart (iInstitutionId, tiProcessed,vchCreatorId,dtCreationDate,tiRecordStatus,decInstDiscount)
select distinct sc.iinstitutionid,0,'migration',getdate(),sc.tirecordstatus,i.decInstDiscount
from tshoppingcart sc
join tInstitution i on i.iinstitutionid = sc.iInstitutionId
where tiProcessed = 0 and sc.tiRecordStatus=1 and sc.iInstitutionId not in (select iInstitutionId From tcart where tiprocessed=0 group by iinstitutionid having count(*) > 0)



insert tcartitem (icartid,iResourceId,iNumberOfLicenses,vchCreatorId,dtCreationDate,tiRecordStatus,decListPrice,decDiscountPrice)
select c.icartid, sc.iresourceid, iNumberLicenses,'migration',getdate(), sc.tiRecordStatus,decLicenseAmt,decLicenseAmt From tshoppingcart sc
join tcart c on c.iInstitutionId = sc.iInstitutionId
where sc.tiProcessed = 0 and sc.tiRecordStatus=1  and c.tiProcessed = 0

--end order history migration changes--------------------------------------------------------

-----------------------------------------------------
-- 8/16/2012 - create view into Prelude Customer Data 
-- LINK SERVER IS REQUIRED!!!
-----------------------------------------------------
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE VIEW [dbo].[vPreludeCustomer]
AS
select accountNumber as vchAccountNumber, accountName as vchAccountName, 
       billToAddress1 as vchBillToAddress1, billToCity as vchBillToCity, 
       billToState as vchBillToState, billToZip as vchBillToZip, billToCountry as vchBillToCountry, 
       confirmEmail as vchEmailAddress, billToPhone as vchBillToPhone, billToFax as vchBillToFax, 
       isRIS as vchIsRIS, billToAddress2 as vchBillToAddress2, billToAddress3 as vchBillToAddress3 
from   [TECHNOSERV04\SQL2005].PreludeData.dbo.Customer
GO

-----------------------------------------------------
-- 8/17/2012 - Adding a default value tiAllowPPV since it is no longer 
-- 			   used and is not nullable. Causes errors when inserting 
-- 			   Trial Institutions
-----------------------------------------------------
ALTER TABLE tInstitution WITH NOCHECK
ADD CONSTRAINT DF_tiAllowPPV DEFAULT '0' FOR tiAllowPPV