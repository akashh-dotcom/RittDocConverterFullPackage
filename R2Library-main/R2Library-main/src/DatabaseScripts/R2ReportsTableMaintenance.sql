
select * from ContentView 

select * from DailyContentViewCount

select max(contentViewDate) from DailyContentViewCount

select max(dailyContentViewCountId) from DailyContentViewCount

select count(*) from DailyContentViewCount

select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
     , cast(cv.contentViewTimestamp as date) as hitDate, count(*)
from   R2Reports.dbo.ContentView cv
where  turnawayTypeId = 0
  and  contentViewTimestamp > (select max(contentViewDate) from DailyContentViewCount)
group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
     , cast(cv.contentViewTimestamp as date)

--select * from ContentView where contentViewTimestamp < '8/1/2012'

--delete from ContentView where contentViewTimestamp between '8/1/2012' and '9/1/2012'
--delete from ContentView where contentViewTimestamp between '9/1/2012' and '10/1/2012'
--delete from ContentView where contentViewTimestamp between '10/1/2012' and '11/1/2012'
--delete from ContentView where contentViewTimestamp between '11/1/2012' and '12/1/2012'

-- reindex
EXEC sp_MSforeachtable @command1="print '?' DBCC DBREINDEX ('?', ' ', 80)"
GO
EXEC sp_updatestats
GO

SELECT ddips.object_id, o.[name], ddips.index_id, i.[name], ddips.avg_fragmentation_in_percent, ddips.page_count
     , ddips.database_id, ddips.[object_id], ddips.index_id, ddips.partition_number, ddips.index_type_desc, ddips.alloc_unit_type_desc, ddips.index_depth, ddips.index_level, ddips.avg_fragmentation_in_percent, ddips.fragment_count, ddips.avg_fragment_size_in_pages, ddips.page_count, ddips.avg_page_space_used_in_percent, ddips.record_count, ddips.ghost_record_count, ddips.version_ghost_record_count, ddips.min_record_size_in_bytes, ddips.max_record_size_in_bytes, ddips.avg_record_size_in_bytes, ddips.forwarded_record_count, ddips.compressed_page_count
     , o.[name], o.[object_id], o.principal_id, o.[schema_id], o.parent_object_id, o.[type], o.type_desc, o.create_date, o.modify_date, o.is_ms_shipped, o.is_published, o.is_schema_published
     , i.[object_id], i.[name], i.index_id, i.[type], i.type_desc, i.is_unique, i.data_space_id, i.[ignore_dup_key], i.is_primary_key, i.is_unique_constraint, i.fill_factor, i.is_padded, i.is_disabled, i.is_hypothetical, i.[allow_row_locks], i.[allow_page_locks], i.has_filter, i.filter_definition
FROM sys.dm_db_index_physical_stats(DB_ID('R2Reports'), NULL, NULL, NULL , NULL) ddips
 join sys.objects o on o.object_id = ddips.object_id
 join sys.indexes i on i.index_id = ddips.index_id and i.[object_id] = o.[object_id] --and i.[name] is not null
--where ddips.avg_fragmentation_in_percent > 5
order by ddips.avg_fragmentation_in_percent desc

DBCC DBREINDEX ('DailyContentTurnawayCount', ' ', 80)
