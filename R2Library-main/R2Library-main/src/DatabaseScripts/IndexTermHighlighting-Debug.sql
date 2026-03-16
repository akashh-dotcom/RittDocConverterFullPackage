select * from TabersTermHighlightQueue
where jobId = 102

insert into IndexTermHighlightQueue (jobId, resourceId, isbn, termHighlightStatus, dateAdded) 
    select 103, resourceId, isbn, 'A', getdate()
    from TabersTermHighlightQueue
    where jobId = 102

select * from IndexTermHighlightQueue

select ithq.termHighlightQueueId, r.iResourceId, r.vchResourceISBN, r.vchResourceTitle
     , (select count(*) from DEV_RIT001..tDiseaseResource disr where disr.vchResourceISBN = r.vchResourceISBN) as [DiseaseResourceCount]
     , (select count(*) from DEV_RIT001..tDiseaseSynonymResource dissr where dissr.vchResourceISBN = r.vchResourceISBN) as [DiseaseSynonymResourceCount]
     , (select count(*) from DEV_RIT001..tDrugResource drgr where drgr.vchResourceISBN = r.vchResourceISBN) as [DrugResourceCount]
     , (select count(*) from DEV_RIT001..tDrugSynonymResource drgsr where drgsr.vchResourceISBN = r.vchResourceISBN) as [DrugSynonymResourceCount]
     , (select count(*) from DEV_RIT001..tKeywordResource kr where kr.vchResourceISBN = r.vchResourceISBN) as [KeywordmResourceCount]
     , (select count(*) from DEV_RIT001..tAtoZIndex azi where azi.iResourceId = r.iResourceId) as [AtoZIndexResourceCount]
from   IndexTermHighlightQueue ithq
 join  DEV_RIT001..tResource r on ithq.resourceId = r.iResourceId
where  ithq.jobId = 103
order by ithq.termHighlightQueueId

select * from DEV_RIT001..tDrugResource

select * from IndexTermHighlightQueue where termHighlightQueueId = 1050
update IndexTermHighlightQueue set termHighlightStatus = 'A', dateStarted = null, dateFinished = null, statusMessage = null where termHighlightQueueId = 1050

update IndexTermHighlightQueue set termHighlightStatus = 'A', dateStarted = null, dateFinished = null, statusMessage = null where jobId = 103
 and termHighlightStatus = 'W'

select * from IndexTermHighlightQueue where jobId = 103



select resourceId, isbn, status, count(*)
from   TransformQueue where status = 'A'
group by resourceId, isbn, status

select resourceId, isbn, status, count(*)
from   TransformQueue where dateAdded > '2/21/2014'
group by resourceId, isbn, status

