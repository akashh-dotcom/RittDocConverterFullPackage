
select count(*) from tPageViews
select count(*) from tApplicationSession
select count(*) from tContentRetrieval
select count(*) from tSearch
select count(*) from tContentTurnaway
select count(*) from tAuthor_Delete
select count(*) from tAuthor2_Delete

select max(dtCreationDate) from tPageViews
select max(dtCreationDate) from tApplicationSession
select max(dtCreationDate) from tContentRetrieval
select max(dtCreationDate) from tSearch
select max(dtCreationDate) from tContentTurnaway

select * from tAuthor_Delete
select * from tAuthor2_Delete


truncate table tPageViews
truncate table tApplicationSession
truncate table tContentRetrieval
truncate table tSearch
truncate table tContentTurnaway
truncate table tAuthor_Delete
truncate table tAuthor2_Delete

drop table tPageViews
drop table tApplicationSession
drop table tContentRetrieval
drop table tSearch
drop table tContentTurnaway
drop table tAuthor_Delete
drop table tAuthor2_Delete

