select ip_from, ip_to, country_code, country_name from ip2location_db1

truncate table [ip2location].[dbo].[ip2location_db1]

BULK INSERT [ip2location].[dbo].[ip2location_db1]
    FROM 'D:\R2library\DB1-IP-COUNTRY.CSV\IPCountry.CSV'
    WITH
    (
        FORMATFILE = 'D:\R2library\DB1-IP-COUNTRY.CSV\DB1.FMT'
    )
GO

select top 100 * from ip2location_db1


select count(*) from ip2location_db1
select count(*) from ip2location_db1_lite


-----------------------------------------------------------
-----------------------------------------------------------
-----------------------------------------------------------


/****** Object: Table [dbo].[tIp2Location]   Script Date: 9/30/2013 12:11:22 PM ******/
USE [DEV_RIT001];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tIp2Location] (
[iIp2LocationId] int IDENTITY(1, 1) NOT NULL,
[iIpTo] bigint NULL,
[iIpFrom] bigint NULL,
[vchCountryCode] varchar(5) NULL,
[vchCountryName] varchar(50) NULL,
CONSTRAINT [PK__tIp2Loca__E7FF37C5F88DAD89]
PRIMARY KEY CLUSTERED ([iIp2LocationId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tIp2Location] SET (LOCK_ESCALATION = TABLE);
GO

/****** Object: Index [dbo].[tIp2Location].[idx_Nonclustered_tIp2Location_iIpTo_iIpFrom]   Script Date: 9/30/2013 12:11:22 PM ******/

CREATE NONCLUSTERED INDEX [idx_Nonclustered_tIp2Location_iIpTo_iIpFrom]
ON [dbo].[tIp2Location]
([iIpTo] , [iIpFrom])
WITH
(
PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ONLINE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE
)
ON [PRIMARY];
GO

-----------------------------------------------------------
-----------------------------------------------------------
-----------------------------------------------------------

truncate table DEV_RIT001..tIp2Location

insert into DEV_RIT001..tIp2Location (iIpTo,iIpFrom,vchCountryCode,vchCountryName) 
    select ip_from, ip_to, country_code, country_name
    from   ip2location_db1
    order by  ip_from

select * from DEV_RIT001..tIp2Location

USE DEV_RIT001
GO
EXEC sp_MSforeachtable @command1="print '?' DBCC DBREINDEX ('?', ' ', 80)"
GO
EXEC sp_updatestats
GO


SELECT ddips.object_id, o.[name], ddips.index_id, i.[name], ddips.avg_fragmentation_in_percent, ddips.page_count  
FROM sys.dm_db_index_physical_stats(DB_ID('DEV_RIT001'), NULL, NULL, NULL , NULL) ddips
 join sys.objects o on o.object_id = ddips.object_id
 join sys.indexes i on i.index_id = ddips.index_id and i.[object_id] = o.[object_id] and i.[name] is not null
--where ddips.avg_fragmentation_in_percent > 0
order by ddips.avg_fragmentation_in_percent desc


