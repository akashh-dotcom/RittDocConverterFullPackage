

declare @ResourceId as int
set @ResourceId = 1234

--tResource
select r.iResourceId, r.vchResourceDesc, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors, r.vchResourceAdditionalContributors, r.vchResourcePublisher, r.dtRISReleaseDate, r.dtResourcePublicationDate, r.tiBrandonHillStatus, r.vchResourceISBN, r.vchResourceEdition, r.decResourcePrice, r.decPayPerView, r.decSubScriptionPrice, r.vchResourceImageName, r.vchCopyRight, r.tiResourceReady, r.tiAllowSubscriptions, r.iPublisherId, r.iResourceStatusId, r.tiGloballyAccessible, r.vchCreatorId, r.dtCreationDate, r.vchUpdaterId, r.dtLastUpdate, r.tiRecordStatus, r.vchMARCRecord, r.vchResourceNLMCall, r.tiDrugMonograph, r.iDCTStatusId, r.tiDoodyReview, r.vchDoodyReviewURL, r.iPrevEditResourceID, r.vchAuthorXML, r.vchForthcomingDate, r.NotSaleable, r.vchResourceSortTitle, r.chrAlphaKey, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn, r.vchResourceSortAuthor
from   tResource r
where  iResourceId = @ResourceId

--tAuthor
select a.iAuthorId, a.iResourceId, a.vchFirstName, a.vchLastName, a.vchMiddleName, a.vchLineage, a.vchDegree, a.tiAuthorOrder
from   tAuthor a
where  iResourceId = @ResourceId

--tPublisher
select pub.iPublisherId, pub.vchPublisherName, pub.vchPublisherAddr1, pub.vchPublisherAddr2, pub.vchPublisherCity, pub.vchPublisherState, pub.vchPublisherZip, pub.decPayPerView, pub.vchCreatorId, pub.dtCreationDate, pub.vchUpdaterId, pub.dtLastUpdate, pub.tiRecordStatus, pub.iConsolidatedPublisherId, pub.vchMarcCountyCode
from   tPublisher pub
 join  dbo.tResource r on r.iPublisherId = pub.iPublisherId
where  r.iResourceId = @ResourceId

--tAtoZIndex
select *
from   tAtoZIndex az
where  iResourceId = @ResourceId

--tResourcePracticeArea
select rpa.iResourcePracticeAreaId, rpa.iResourceId, rpa.iPracticeAreaId, rpa.vchCreatorId, rpa.dtCreationDate, rpa.vchUpdaterId, rpa.dtLastUpdate, rpa.tiRecordStatus
     , pa.iPracticeAreaId, pa.vchPracticeAreaCode, pa.vchPracticeAreaName, pa.iSequenceNumber, pa.vchCreatorId, pa.dtCreationDate, pa.vchUpdaterId, pa.dtLastUpdate, pa.tiRecordStatus
from   tResourcePracticeArea rpa
 join  dbo.tPracticeArea pa on pa.iPracticeAreaId = rpa.iPracticeAreaId 
where  iResourceId = @ResourceId

--tResourceSpecialty
select rs.iResourceSpecialtyId, rs.iResourceId, rs.iSpecialtyId, rs.vchCreatorId, rs.dtCreationDate, rs.vchUpdaterId, rs.dtLastUpdate, rs.tiRecordStatus
     , s.iSpecialtyId, s.vchSpecialtyCode, s.vchSpecialtyName, s.iSequenceNumber, s.vchCreatorId, s.dtCreationDate, s.vchUpdaterId, s.dtLastUpdate, s.tiRecordStatus
from   tResourceSpecialty rs
 join  dbo.tSpecialty s on s.iSpecialtyId = rs.iSpecialtyId
where  iResourceId = @ResourceId

declare @Isbn as varchar(15)
--set @Isbn = '0521700280'
--set @Isbn = '0803628331'
set @Isbn = '0763727482'

--tDiseaseResource
select dr.iDiseaseResourceId, dr.iDiseaseNameId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, dr.vchCreatorId, dr.dtCreationDate, dr.vchUpdaterId, dr.dtLastUpdate, dr.tiRecordStatus
     , dn.iDiseaseNameId, dn.vchDiseaseName, dn.vchDiseaseDesc, dn.vchDiseaseUrl, dn.vchCreatorId, dn.dtCreationDate, dn.vchUpdaterId, dn.dtLastUpdate, dn.tiRecordStatus, dn.iParentDiseaseNameId, dn.vchRelationName
from   tDiseaseResource dr
  join tdiseasename dn on dr.iDiseaseNameId = dn.iDiseaseNameId
where  dr.vchResourceISBN = @Isbn

--tDiseaseSynonymResource
select dsn.iDiseaseSynonymResourceId, dsn.iDiseaseSynonymId, dsn.vchResourceISBN, dsn.vchChapterId, dsn.vchSectionId, dsn.vchCreatorId, dsn.dtCreationDate, dsn.vchUpdaterId, dsn.dtLastUpdate, dsn.tiRecordStatus
     , ds.iDiseaseSynonymId, ds.vchDiseaseSynonym, ds.iDiseaseNameId, ds.vchCreatorId, ds.dtCreationDate, ds.vchUpdaterId, ds.dtLastUpdate, ds.tiRecordStatus
from   tDiseaseSynonymResource dsn
 join  tdiseasesynonym ds on dsn.iDiseaseSynonymId = ds.iDiseaseSynonymId
where  dsn.vchResourceISBN = @Isbn

--tDrugResource
select dr.iDrugResourceId, dr.iDrugListId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, dr.vchCreatorId, dr.dtCreationDate, dr.vchUpdaterId, dr.dtLastUpdate, dr.tiRecordStatus, dr.tiTopicIndex, dr.vchTitle
     , dl.iDrugListId, dl.vchDrugURL, dl.vchCreatorId, dl.dtCreationDate, dl.vchUpdaterId, dl.dtLastUpdate, dl.tiRecordStatus, dl.vchDrugName
from   tDrugResource dr
 join  dbo.tDrugsList dl on dl.iDrugListId = dr.iDrugListId
where  dr.vchResourceISBN = @Isbn

--tDrugSynonymResource
select dsr.iDrugSynonymResourceId, dsr.iDrugSynonymId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, dsr.vchCreatorId, dsr.dtCreationDate, dsr.vchUpdaterId, dsr.dtLastUpdate, dsr.tiRecordStatus, dsr.tiTopicIndex, dsr.vchTitle
from   tDrugSynonymResource dsr
 join  tDrugSynonym ds on dsr.iDrugSynonymId = ds.iDrugSynonymId
where  dsr.vchResourceISBN = @Isbn

--tKeywordResource
select kr.iKeywordResourceId, kr.iKeywordId, kr.vchResourceISBN, kr.vchChapterId, kr.vchSectionId, kr.vchCreatorId, kr.dtCreationDate, kr.vchUpdaterId, kr.dtLastUpdate, kr.tiRecordStatus
     , k.iKeywordId, k.vchKeywordDesc, k.vchCreatorId, k.dtCreationDate, k.vchUpdaterId, k.dtLastUpdate, k.tiRecordStatus
from   tKeywordResource kr
 join  dbo.tKeyword k on k.iKeywordId = kr.iKeywordId
where  kr.vchResourceISBN = @Isbn


--tResourceDiscipline
select rd.iResourceDisciplineId, rd.iLibraryDisciplineId, rd.iResourceId, rd.vchCreatorId, rd.dtCreationDate, rd.vchUpdaterId, rd.dtLastUpdate, rd.tiRecordStatus
     , ld.iLibraryDisciplineId, ld.iDisciplineId, ld.iLibraryId, ld.vchCreatorId, ld.dtCreationDate, ld.vchUpdaterId, ld.dtLastUpdate, ld.tiRecordStatus
     , l.iLibraryId, l.vchLibraryCode, l.vchLibraryName, l.iSequenceNumber, l.vchCreatorId, l.dtCreationDate, l.vchUpdaterId, l.dtLastUpdate, l.tiRecordStatus
     , d.iDisciplineId, d.vchDisciplineCode, d.vchDisciplineName, d.iSequenceNumber, d.vchCreatorId, d.dtCreationDate, d.vchUpdaterId, d.dtLastUpdate, d.tiRecordStatus
from   tResourceDiscipline rd
 join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId
 join  dbo.tLibrary l on l.iLibraryId = ld.iLibraryId
 join  dbo.tDiscipline d on d.iDisciplineId = ld.iDisciplineId
where  rd.iResourceId = @ResourceId

--tResourceFile
--select rf.iResourceFileId, rf.iResourceId, rf.vchFileNameFull, rf.vchFileNamePart1, rf.vchFileNamePart3, rf.iDocumentId, rf.vchCreatorId, rf.dtCreationDate, rf.vchUpdaterId, rf.dtLastUpdate, rf.tiRecordStatus
--from   tResourceFile rf
--where  iResourceId = 1234


