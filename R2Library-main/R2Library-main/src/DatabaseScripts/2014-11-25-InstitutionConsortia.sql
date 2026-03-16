
alter table tInstitution
add vchConsortia varchar(8) null;



alter table tUserImage
alter column vchImageTitle nvarchar (255) not null;

alter table tUserCourseLinks
alter column vchCourseLinksTitle nvarchar (255) not null;

alter table tUserBookmark
alter column vchBookmarkTitle nvarchar (255) not null;

alter table tUserReference
alter column vchReferenceTitle nvarchar (255) not null;