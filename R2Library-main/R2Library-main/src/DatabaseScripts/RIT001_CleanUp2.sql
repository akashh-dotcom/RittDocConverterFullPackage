
truncate table tPageViews
truncate table tApplicationSession
truncate table tContentRetrieval
truncate table tSearch

select top 1000 * from tPageViews order by dtCreationDate desc
select top 1000 * from tApplicationSession order by dtCreationDate desc
select top 1000 * from tContentRetrieval order by dtCreationDate desc
select top 1000 * from tSearch order by dtCreationDate desc
