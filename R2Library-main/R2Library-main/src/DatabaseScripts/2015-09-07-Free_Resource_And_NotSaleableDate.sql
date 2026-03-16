

alter table tResource
add tiFreeResource tinyint not null default((0));

go 

alter table tResource
add dtNotSaleableDate datetime null;
go 
Insert Into tInstitutionResourceAuditType
values (12, 'Resource has become Saleable');
go 
Insert Into tInstitutionResourceAuditType
values (13, 'Resource has become NOT Saleable');
go 

update tResource
set dtNotSaleableDate = tt.NotSaleableDate
from tResource r
join 
(
	select ISNULL(min(irl.dtResourceNotSaleableDate), '2012-08-14 12:44:00') as 'NotSaleableDate', r.iResourceId
	from tResource r
	join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId
	join tPublisher p on r.iPublisherId = p.iPublisherId
	where (irl.dtResourceNotSaleableDate is not null and r.dtNotSaleableDate is null)
	or
	(irl.dtResourceNotSaleableDate is null and r.dtNotSaleableDate is null and r.NotSaleable = 1 )
	group by r.iResourceId, r.iPublisherId, p.vchPublisherName
) tt on r.iResourceId = tt.iResourceId
go 



Alter table tuser
add tiEnablePublisherAdd tinyint null;
go

update tUser
set tiEnablePublisherAdd = 1
where vchUserName in (
'davejones'
,'kenhaberle'
,'mwhite'
,'sjscheider'
,'jasonhafer'
,'mikemalone')

go


alter table tuser
add [dtConcurrentTurnawayAlert] [datetime] NULL

go