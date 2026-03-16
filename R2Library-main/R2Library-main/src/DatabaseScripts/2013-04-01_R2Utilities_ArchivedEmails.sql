USE [R2Utilities]
Alter Table ResourceEmails
Add dateArchivedEmail smalldatetime NULL;

USE [R2Utilities]
update ResourceEmails
set [dateArchivedEmail] = GETDATE()
from [RIT001_2012-08-22]..tResource r
--from [RIT001]..tResource r
join ResourceEmails re on r.vchResourceISBN = re.resourceISBN
where r.iResourceStatusId = 7