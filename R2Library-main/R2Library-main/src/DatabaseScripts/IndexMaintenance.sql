
-- list all indexes within a database that have more than 0% fragmentation.
-- [RIT001_2012-03-21]
SELECT ddips.object_id, o.[name], ddips.index_id, i.[name], ddips.avg_fragmentation_in_percent, ddips.page_count  
FROM sys.dm_db_index_physical_stats(DB_ID('RIT001_2012-03-2'), NULL, NULL, NULL , NULL) ddips
 join sys.objects o on o.object_id = ddips.object_id
 join sys.indexes i on i.index_id = ddips.index_id and i.[object_id] = o.[object_id]
where ddips.avg_fragmentation_in_percent > 20
order by ddips.avg_fragmentation_in_percent desc

-- [RIT001_2012-03-21]
SELECT ddips.object_id, o.[name], ddips.index_id, i.[name], ddips.avg_fragmentation_in_percent, ddips.page_count  
FROM sys.dm_db_index_physical_stats(DB_ID('RIT001_2012-03-2'), NULL, NULL, NULL , NULL) ddips
 join sys.objects o on o.object_id = ddips.object_id
 join sys.indexes i on i.index_id = ddips.index_id and i.[object_id] = o.[object_id]
where ddips.avg_fragmentation_in_percent > 0 and i.[name] is not null
order by ddips.avg_fragmentation_in_percent desc

-- reindex
EXEC sp_MSforeachtable @command1="print '?' DBCC DBREINDEX ('?', ' ', 80)"
GO
EXEC sp_updatestats
GO

-- alter stats
EXEC sp_MSforeachtable @command1="print '?' ALTER INDEX ALL ON ? REORGANIZE"
GO
EXEC sp_updatestats
GO


--sp_helpfile 'D:\MSSQLDATA\MSSQL.1\MSSQL\Data\RittenhouseWeb.mdf'

exec sp_spaceused

EXEC sp_MSforeachtable @command1="sp_spaceused '?'"
