Alter table tResource
add tiAffiliationUpdatedByPrelude tinyint not null default((0));
go
update tResource
set tiAffiliationUpdatedByPrelude = 1
where vchAffiliation is not null
