alter table tResource
add dtArchiveDate smalldatetime null;

update tresource
set dtArchiveDate = ra.dtCreationDate
from tResource r 
join tResourceAudit ra on ra.iResourceId = r.iResourceId and ra.vchEventDescription like '%to Archived(7)%'
where r.iResourceStatusId = 7
