
alter table tResource
add [vchAffiliation] [varchar](255) NULL


--select p.affiliation, r.*
--from tResource r
--join [RittenhouseWeb]..Product p on r.vchIsbn10 = p.isbn10

--UPDATE tResource
--SET vchAffiliation = p.affiliation
--FROM [RittenhouseWeb]..Product p
--INNER JOIN tResource r on p.isbn10 = r.vchIsbn10
















