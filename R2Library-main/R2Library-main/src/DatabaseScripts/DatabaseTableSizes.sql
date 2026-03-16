IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DatabaseTableSize]') AND type in (N'U'))
BEGIN
DROP TABLE [dbo].[DatabaseTableSize];
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[DatabaseTableSize] (
[tableName] varchar(50) NULL,
[rowCount] int NULL,
[reserver] varchar(50) NULL,
[data] varchar(50) NULL,
[indexSize] varchar(50) NULL,
[unused] varchar(50) NULL,
[reserverInKB] int NULL,
[dataInKB] int NULL,
[indexSizeInKB] int NULL,
[unusedInKB] int NULL)
ON [PRIMARY];
GO


truncate table DatabaseTableSize

exec sp_MSforeachtable @command1="
insert into DatabaseTableSize (tableName,[rowCount],reserver,data,indexSize,unused) 
    EXEC sp_spaceused '?'"

update DatabaseTableSize
set    reserverInKB = cast(replace(reserver, ' KB', '') as int)
     , dataInKB = cast(replace(data, ' KB', '') as int)
     , indexSizeInKB = cast(replace(indexSize, ' KB', '') as int)
     , unusedInKB = cast(replace(unused, ' KB', '') as int)

select * from DatabaseTableSize
order by dataInKB desc
