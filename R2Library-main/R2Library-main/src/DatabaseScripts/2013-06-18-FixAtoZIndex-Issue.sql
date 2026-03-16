EXEC sys.sp_rename @objname = N'[dbo].[tAtoZIndex].[PK__tAtoZInd__9B90EB6844F8E12D]', @newname = [PK_tAtoZIndex], @objtype = N'INDEX';
GO
EXEC sys.sp_rename @objname = N'[dbo].[tAtoZIndex].[IX_tAtoZIndex_Name>]', @newname = [IX_tAtoZIndex_Name], @objtype = N'INDEX';
GO

--CREATE UNIQUE NONCLUSTERED INDEX [UK_tAtoZIndex]
--ON [dbo].[tAtoZIndex]
--(
-- [vchResourceISBN] , [vchName] , [vchChapterId] , [vchSectionId] , [iAtoZIndexTypeId] 
--)
--WITH
--(
--PAD_INDEX = OFF,
--FILLFACTOR = 100,
--IGNORE_DUP_KEY = OFF,
--STATISTICS_NORECOMPUTE = OFF,
--ALLOW_ROW_LOCKS = ON,
--ALLOW_PAGE_LOCKS = ON,
--DATA_COMPRESSION = NONE
--)
--ON [PRIMARY];
--GO

select [vchResourceISBN] , [vchName] , [vchChapterId] , [vchSectionId] , [iAtoZIndexTypeId], count(*)
from   tAtoZIndex
where  iAtoZIndexTypeId = 5
group by [vchResourceISBN], [vchName], [vchChapterId], [vchSectionId], [iAtoZIndexTypeId]
order by count(*) desc


select *
from   tKeyword k
 join  tKeywordResource kr on kr.iKeywordId = k.iKeywordId 
 left outer join tAtoZIndex az on az.vchResourceISBN = kr.vchResourceISBN
  and az.vchName = vchKeywordDesc
  and az.vchChapterId = kr.vchChapterId
  and az.vchSectionId = kr.vchSectionId
  and az.iAtoZIndexTypeId = 5
where az.iAtoZIndexId is null

select *
from   tKeyword k
 join  tKeywordResource kr on kr.iKeywordId = k.iKeywordId 
 
select * from tAtoZIndex

-- Keywords 
insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey
                      , iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId)
    select k.iKeywordId, k.vchKeywordDesc, upper(substring(ltrim(rtrim(k.vchKeywordDesc)), 0, 2)) as AlphaKey
         , r.iResourceId, kr.vchResourceISBN, kr.vchChapterId, kr.vchSectionId, 5
    from   tKeyword k
     join  tKeywordResource kr on kr.iKeywordId = k.iKeywordId 
     join  tResource r on r.vchResourceISBN = kr.vchResourceISBN
     left outer join tAtoZIndex az on az.vchResourceISBN = kr.vchResourceISBN
      and az.vchName = vchKeywordDesc
      and az.vchChapterId = kr.vchChapterId
      and az.vchSectionId = kr.vchSectionId
      and az.iAtoZIndexTypeId = 5
    where az.iAtoZIndexId is null
 
 
insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select dn.iDiseaseNameId AS Id, dn.vchDiseaseName as Name
         , upper(substring(ltrim(rtrim(dn.vchDiseaseName)), 0, 2)) as AlphaKey
         , r.iResourceId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, 1
    from tdiseasename dn
        join tDiseaseResource dr on dr.iDiseaseNameId = dn.iDiseaseNameId
        join dbo.tResource r on r.vchResourceISBN = dr.vchResourceISBN
     left outer join tAtoZIndex az on az.vchResourceISBN = dr.vchResourceISBN
      and az.vchName = dn.vchDiseaseName
      and az.vchChapterId = dr.vchChapterId
      and az.vchSectionId = dr.vchSectionId
      and az.iAtoZIndexTypeId = 1
    where az.iAtoZIndexId is null

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select ds.iDiseaseSynonymId as Id, ds.vchDiseaseSynonym as Name
         , upper(substring(ltrim(rtrim(ds.vchDiseaseSynonym)), 0, 2)) as AlphaKey
         , r.iResourceId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, 2
    from tdiseasesynonym ds
        join dbo.tDiseaseSynonymResource dsr on dsr.iDiseaseSynonymId = ds.iDiseaseSynonymId
        join dbo.tResource r on r.vchResourceISBN = dsr.vchResourceISBN
     left outer join tAtoZIndex az on az.vchResourceISBN = dsr.vchResourceISBN
      and az.vchName = ds.vchDiseaseSynonym
      and az.vchChapterId = dsr.vchChapterId
      and az.vchSectionId = dsr.vchSectionId
      and az.iAtoZIndexTypeId = 2
    where az.iAtoZIndexId is null

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select dl.iDrugListId as Id, dl.vchDrugName as Name
         , upper(substring(ltrim(rtrim(dl.vchDrugName)), 0, 2)) as AlphaKey
         , r.iResourceId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, 3
      from tDrugsList dl
        join dbo.tDrugResource dr on dr.iDrugListId = dl.iDrugListId
        join dbo.tResource r on r.vchResourceISBN = dr.vchResourceISBN
     left outer join tAtoZIndex az on az.vchResourceISBN = dr.vchResourceISBN
      and az.vchName = dl.vchDrugName
      and az.vchChapterId = dr.vchChapterId
      and az.vchSectionId = dr.vchSectionId
      and az.iAtoZIndexTypeId = 3
    where az.iAtoZIndexId is null

insert into tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId,vchResourceISBN,vchChapterId,vchSectionId,iAtoZIndexTypeId)
    select ds.iDrugSynonymId as Id, ds.vchDrugSynonymName as Name
         , upper(substring(ltrim(rtrim(ds.vchDrugSynonymName)), 0, 2)) as AlphaKey
         , r.iResourceId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, 4
      from tDrugSynonym ds
        join dbo.tDrugSynonymResource dsr on dsr.iDrugSynonymId = ds.iDrugSynonymId
        join dbo.tResource r on r.vchResourceISBN = dsr.vchResourceISBN
     left outer join tAtoZIndex az on az.vchResourceISBN = dsr.vchResourceISBN
      and az.vchName = ds.vchDrugSynonymName
      and az.vchChapterId = dsr.vchChapterId
      and az.vchSectionId = dsr.vchSectionId
      and az.iAtoZIndexTypeId = 4
    where az.iAtoZIndexId is null
 

DBCC DBREINDEX ('tAtoZIndex', ' ', 80)
GO
EXEC sp_updatestats
GO

select r.iResourceId, r.vchResourceTitle, r.vchResourceISBN, r.vchCopyRight, r.dtRISReleaseDate, r.dtCreationDate
     , count(kr.iKeywordResourceId) as [Keyword Count]
     , (select count(*) from tDrugResource dr where dr.vchResourceISBN = r.vchResourceISBN) as [Drug Count]
     , (select count(*) from tDrugSynonymResource dsr where dsr.vchResourceISBN = r.vchResourceISBN) as [Drug Synonym Count]
     , (select count(*) from tDiseaseResource dr where dr.vchResourceISBN = r.vchResourceISBN) as [Disease Count]
     , (select count(*) from tDiseaseSynonymResource dsr where dsr.vchResourceISBN = r.vchResourceISBN) as [Disease Synonym Count]
from   tResource r
 left outer join tKeywordResource kr on kr.vchResourceISBN = r.vchResourceISBN
where  r.iResourceStatusId = 6
group by r.iResourceId, r.vchResourceTitle, r.vchResourceISBN, r.vchCopyRight, r.dtRISReleaseDate, r.dtCreationDate
order by r.iResourceId

select r.iResourceId, r.vchResourceTitle, r.vchResourceISBN, r.vchCopyRight, r.dtRISReleaseDate, r.dtCreationDate, count(dr.iDrugResourceId)
from   tResource r
 left outer join tDrugResource dr on dr.vchResourceISBN = r.vchResourceISBN
where  r.iResourceStatusId = 6
group by r.iResourceId, r.vchResourceTitle, r.vchResourceISBN, r.vchCopyRight, r.dtRISReleaseDate, r.dtCreationDate
order by count(dr.iDrugResourceId)

-- delete from tAtoZIndex where iResourceId = 

delete from tAtoZIndex where vchResourceISBN

select iAtoZIndexId, iParentTableId, vchName, chrAlphaKey, iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId
from   tAtoZIndex
where  vchResourceISBN
