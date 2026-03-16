ALTER VIEW [dbo].[vInstitutionResourceLicense]
AS
select i.iInstitutionId, r.iResourceId, i.iInstitutionAcctStatusId
     , sum(ril.iNumberLicenses) as iLicenseCount, min(ril.dtCreationDate) as [dtFirstPurchaseDate]     
from   tInstitution i
 join  dbo.tInstitutionResource ir on ir.iInstitutionId = i.iInstitutionId and ir.tiRecordStatus = 1
 join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId and ril.tiRecordStatus = 1
 join  dbo.tResource r on r.iResourceId = ir.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8)
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 1
group by i.iInstitutionId, r.iResourceId, r.iResourceStatusId, i.iInstitutionAcctStatusId
union
select i.iInstitutionId, r.iResourceId, i.iInstitutionAcctStatusId --,r.iResourceStatusId
     , 3 as iLicenseCount, '1/1/2000' as [dtFirstPurchaseDate]
from   tInstitution i
 join  tResource r on r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8) and r.NotSaleable = 0
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 2
  and  getdate() between i.dtTrialAcctStart and i.dtTrialAcctEnd

GO


-- changes to tResource
ALTER TABLE [dbo].[tResource] ADD [vchResourceDescTemp] varchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

update tResource 
set    vchResourceDescTemp = vchResourceDesc

ALTER TABLE [dbo].[tResource]
DROP COLUMN [vchResourceDesc]
GO

EXECUTE sys.sp_rename @objname = N'[dbo].[tResource].[vchResourceDescTemp]', @newname = N'vchResourceDesc', @objtype = 'COLUMN'
GO

-- added field to tUser for receiving PDA add to cart emails.
ALTER TABLE [dbo].[tUser] ADD [tiReceivePdaAddToCart] tinyint NOT NULL DEFAULT 0
GO


------------------------------------------------------
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tLicenseType] (
[tiLicenseType] tinyint NOT NULL,
[LicenseTypeDescription] varchar(50) NOT NULL,
CONSTRAINT [PK__tLicense__CBFA5EFA470D43AE]
PRIMARY KEY CLUSTERED ([tiLicenseType] ASC)
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
ALTER TABLE [dbo].[tLicenseType] SET (LOCK_ESCALATION = TABLE);
GO

insert into tLicenseType (tiLicenseType, LicenseTypeDescription) values (1, 'Purchased');
insert into tLicenseType (tiLicenseType, LicenseTypeDescription) values (2, 'Trial');
insert into tLicenseType (tiLicenseType, LicenseTypeDescription) values (3, 'PDA');


CREATE TABLE [dbo].[tLicenseOriginalSource] (
[tiLicenseOriginalSourceId] tinyint NOT NULL,
[LicenseOriginalSourceDescription] varchar(50) NOT NULL,
CONSTRAINT [PK__tLicense__212EEFA2E2524D3C]
PRIMARY KEY CLUSTERED ([tiLicenseOriginalSourceId] ASC)
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
ALTER TABLE [dbo].[tLicenseOriginalSource] SET (LOCK_ESCALATION = TABLE);
GO

insert into tLicenseOriginalSource (tiLicenseOriginalSourceId, LicenseOriginalSourceDescription) values (1, 'Firm Order');
insert into tLicenseOriginalSource (tiLicenseOriginalSourceId, LicenseOriginalSourceDescription) values (2, 'PDA');

select * from tLicenseType
select * from tLicenseOriginalSource


--drop table [dbo].[tInstitutionResourceLicense]

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tInstitutionResourceLicense] (
[iInstitutionResourceLicenseId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iResourceId] int NOT NULL,
[iLicenseCount] int NOT NULL,
[tiLicenseTypeId] tinyint NOT NULL,
[tiLicenseOriginalSourceId] tinyint NOT NULL,
[dtFirstPurchaseDate] smalldatetime NULL,
[dtPdaAddedDate] smalldatetime NULL,
[dtPdaAddedToCartDate] smalldatetime NULL,
[vchPdaAddedToCartById] varchar(50) NULL,
[iPdaViewCount] int NOT NULL DEFAULT ((0)),
[iPdaMaxViews] int NOT NULL DEFAULT ((0)),
[dtCreationDate] smalldatetime NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtLastUpdate] smalldatetime NULL,
[vchUpdaterId] varchar(50) NULL,
[tiRecordStatus] tinyint NOT NULL DEFAULT ((0)),
CONSTRAINT [FK_tInstitutionResourceLicense_tInstitution]
FOREIGN KEY ([iInstitutionId])
REFERENCES [dbo].[tInstitution] ( [iInstitutionId] ),
CONSTRAINT [FK_tInstitutionResourceLicense_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [UK_tInstitutionResourceLicense_tInstitution_tResource]
UNIQUE NONCLUSTERED ([iInstitutionId] ASC, [iResourceId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionResourceLicense_tLicenseOriginalSource]
FOREIGN KEY ([tiLicenseOriginalSourceId])
REFERENCES [dbo].[tLicenseOriginalSource] ( [tiLicenseOriginalSourceId] ),
CONSTRAINT [FK_tInstitutionResourceLicense_tLicenseType]
FOREIGN KEY ([tiLicenseTypeId])
REFERENCES [dbo].[tLicenseType] ( [tiLicenseType] ),
CONSTRAINT [PK_tInstitutionResourceLicense]
PRIMARY KEY CLUSTERED ([iInstitutionResourceLicenseId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceLicense] SET (LOCK_ESCALATION = TABLE);
GO


CREATE TABLE [dbo].[tInstitutionResourceAuditType] (
[iInstitutionResourceAuditTypeId] tinyint NOT NULL,
[AuditTypeDescription] varchar(50) NOT NULL,
CONSTRAINT [PK_tInstitutionResourceAuditType]
PRIMARY KEY CLUSTERED ([iInstitutionResourceAuditTypeId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceAuditType] SET (LOCK_ESCALATION = TABLE);
GO


CREATE TABLE [dbo].[tInstitutionResourceAudit] (
[iInstitutionResourceAuditId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iResourceId] int NOT NULL,
[iUserId] int NULL,
[iInstitutionResourceAuditTypeId] tinyint NOT NULL,
[iLicenseCount] int NOT NULL,
[decSingleLicensePrice] decimal(12, 2) NOT NULL,
[vchPoNumber] varchar(20) NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchEventDescription] varchar(1000) NOT NULL,
[bLegacy] bit NOT NULL DEFAULT ((0)),
CONSTRAINT [PK_tInstitutionResourceAudit]
PRIMARY KEY CLUSTERED ([iInstitutionResourceAuditId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionResourceAudit_tInstitutionResourceAuditType]
FOREIGN KEY ([iInstitutionResourceAuditTypeId])
REFERENCES [dbo].[tInstitutionResourceAuditType] ( [iInstitutionResourceAuditTypeId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionResourceAudit] SET (LOCK_ESCALATION = TABLE);
GO


delete from tInstitutionResourceAuditType

--insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (0, 'Data migrated from t');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (1, 'Added resource to the cart');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (2, 'Deleted resource to the cart');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (3, 'Updated resource count in the cart');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (4, 'Purchased resource');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (5, 'Granted complimentary resource license');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (6, 'Updated resource license count');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (7, 'Added resource to PDA');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (8, 'Delete resource from PDA');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (9, 'Viewed PDA resource');
insert into tInstitutionResourceAuditType (iInstitutionResourceAuditTypeId,AuditTypeDescription) VALUES (10, 'PDA resource added to cart');


---------------------------------------------------

--truncate table tInstitutionResourceLicense

insert into tInstitutionResourceLicense (iInstitutionId, iResourceId, iLicenseCount
            , tiLicenseTypeId, tiLicenseOriginalSourceId, dtFirstPurchaseDate, dtPdaAddedDate
            , dtPdaAddedToCartDate, vchPdaAddedToCartById, iPdaViewCount, iPdaMaxViews
            , dtCreationDate, vchCreatorId, dtLastUpdate
            , vchUpdaterId, tiRecordStatus)
    select i.iInstitutionId, r.iResourceId, sum(ril.iNumberLicenses) as iLicenseCount
         , 1 as [tiLicenseTypeId], 1 as [tiOriginalSourceId], min(ril.dtCreationDate) as [dtFirstPurchaseDate], null as [dtPdaAddedDate]
         , null as [dtPdaAddedToCartDate], null as [vchPdaAddedToCartById], 0 as [iPdaViewCount], 0 as [iPdaMaxViews]
         , min(ir.dtCreationDate) as [dtCreationDate], ir.vchCreatorId as [vchCreatorId], max(ir.dtLastUpdate) as [dtLastUpdate]
         , ir.vchUpdaterId as [vchUpdaterId], 1 as [tiRecordStatus]
    from   tInstitution i
     join  dbo.tInstitutionResource ir on ir.iInstitutionId = i.iInstitutionId and ir.tiRecordStatus = 1
     join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId and ril.tiRecordStatus = 1
     join  dbo.tResource r on r.iResourceId = ir.iResourceId --and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8)
--    where  i.tiRecordStatus = 1
--      and  i.iInstitutionAcctStatusId = 1
    group by i.iInstitutionId, r.iResourceId--, ir.dtCreationDate, ir.vchCreatorId, ir.dtLastUpdate, ir.vchUpdaterId
            , ir.vchCreatorId, ir.vchUpdaterId
    order by i.iInstitutionId, r.iResourceId


--select * from tResourceInstLicense  -- 42,858


select iInstitutionId, iResourceId, count(*)
from   tInstitutionResourceLicense
group by iInstitutionId, iResourceId
having count(*) > 1

--select * from tInstitutionResource where iResourceId = 2321 and  iInstitutionId = 1
--select * from tInstitutionResource where iResourceId = 3444 and  iInstitutionId = 89
--select * from tInstitutionResource where iResourceId = 2201 and  iInstitutionId = 1204

select * from tInstitutionResourceLicense where iResourceId = 2321 and  iInstitutionId = 1
select * from tInstitutionResourceLicense where iResourceId = 3444 and  iInstitutionId = 89
select * from tInstitutionResourceLicense where iResourceId = 2201 and  iInstitutionId = 1204

delete from tInstitutionResourceLicense where iInstitutionResourceLicenseId in (2048, 8849, 36222)

-------------------------------------------------------------------
ALTER TABLE [dbo].[tInstitutionResourceLicense] 
ADD  CONSTRAINT [UK_tInstitutionResourceLicense_tInstitution_tResource]
UNIQUE NONCLUSTERED ([iInstitutionId] , [iResourceId] )
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
GO

--ALTER TABLE [dbo].[tInstitutionResourceLicense]
--DROP CONSTRAINT [UK_tInstitutionResourceLicense_tInstitution_tResource]
--GO

-------------------------------------------------------------------

--select * from tInstitution where tiAutoAddFreeLicenses = 1
--
--select * from tUser where iInstitutionId = 50

truncate table tInstitutionResourceAudit

-- Granted complimentary resource license
insert into tInstitutionResourceAudit (iInstitutionId, iResourceId, iUserId, iInstitutionResourceAuditTypeId
    , iLicenseCount, decSingleLicensePrice, vchPoNumber, vchCreatorId
    , dtCreationDate, vchEventDescription, bLegacy)
    select ir.iInstitutionId, ir.iResourceId, null as [iUserId], 5 as [iInstitutionResourceAuditTypeId]
         , ril.iNumberLicenses as [iLicenseCount], 0 as [decSingleLicensePrice], ril.vchPoNumber, ril.vchCreatorId
         , ril.dtCreationDate, 'Auto license converted from tResourceInstLicense, iResourceInstLicenseId=' + cast(ril.iResourceInstLicenseId as varchar(20)), 1 as [bLegacy]         
    from   dbo.tInstitutionResource ir
     join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId
     join  dbo.tInstitution i on i.iInstitutionId = ir.iInstitutionId and i.tiAutoAddFreeLicenses = 1
    where  ril.dtCreationDate > '1/1/2004'
      and  ril.iNumberLicenses = 3
    order by ril.dtCreationDate


-- Purchased resource
insert into tInstitutionResourceAudit (iInstitutionId, iResourceId, iUserId, iInstitutionResourceAuditTypeId
    , iLicenseCount, decSingleLicensePrice, vchPoNumber, vchCreatorId
    , dtCreationDate, vchEventDescription, bLegacy)
    select ir.iInstitutionId, ir.iResourceId, null as [iUserId], 4 as [iInstitutionResourceAuditTypeId]
         , ril.iNumberLicenses as [iLicenseCount], ril.decLicenseAmt as [decSingleLicensePrice], ril.vchPoNumber, ril.vchCreatorId
         , ril.dtCreationDate, 'Resource licensed purchased, iResourceInstLicenseId=' + cast(ril.iResourceInstLicenseId as varchar(20)), 1 as [bLegacy]         
--         , ril.iResourceInstLicenseId, ril.iNumberLicenses, ril.decLicenseAmt, ril.vchPoNumber, ril.iInstitutionResourceId, ril.vchCreatorId, ril.dtCreationDate, ril.vchUpdaterId, ril.dtLastUpdate, ril.tiRecordStatus
    from   dbo.tInstitutionResource ir
     join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId
     join  dbo.tInstitution i on i.iInstitutionId = ir.iInstitutionId 
    where  ril.dtCreationDate > '1/1/2004'
      and  (ril.iNumberLicenses <> 3 or i.tiAutoAddFreeLicenses = 0)
    order by ril.dtCreationDate

-- Updated resource license count
insert into tInstitutionResourceAudit (iInstitutionId, iResourceId, iUserId, iInstitutionResourceAuditTypeId
    , iLicenseCount, decSingleLicensePrice, vchPoNumber, vchCreatorId
    , dtCreationDate, vchEventDescription, bLegacy)
    select ir.iInstitutionId, ir.iResourceId, null as [iUserId], 6 as [iInstitutionResourceAuditTypeId]
         , ril.iNumberLicenses as [iLicenseCount], ril.decLicenseAmt as [decSingleLicensePrice], ril.vchPoNumber, ril.vchUpdaterId
         , ril.dtLastUpdate, 'Resource licensed count updated, iResourceInstLicenseId=' + cast(ril.iResourceInstLicenseId as varchar(20)), 1 as [bLegacy]         
--         , ril.iResourceInstLicenseId, ril.iNumberLicenses, ril.decLicenseAmt, ril.vchPoNumber, ril.iInstitutionResourceId, ril.vchCreatorId, ril.dtCreationDate, ril.vchUpdaterId, ril.dtLastUpdate, ril.tiRecordStatus
    from   dbo.tInstitutionResource ir
     join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId
     join  dbo.tInstitution i on i.iInstitutionId = ir.iInstitutionId 
    where  ril.dtCreationDate > '1/1/2004'
      and  ril.dtLastUpdate is not null
    order by ril.dtLastUpdate

select * from tInstitutionResourceAudit order by dtCreationDate

select * from tInstitutionResourceAudit where iInstitutionId = 1 and iResourceId = 15 order by dtCreationDate

-------------------------------------------------------------------
ALTER TABLE [dbo].[tReserveListResource] ADD [iResourceId] int NULL
GO

update rlr
set    iResourceId = ir.iResourceId
from   tReserveListResource rlr
 join  dbo.tInstitutionResource ir on ir.iInstitutionResourceId = rlr.iInstitutionResourceId

ALTER TABLE [dbo].[tReserveListResource]
DROP CONSTRAINT [tInstitutionResource_tReserveListResource_FK1]
GO

ALTER TABLE [dbo].[tReserveListResource] 
ADD  CONSTRAINT [FK_tReserveListResource_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] )
GO

ALTER TABLE [dbo].[tReserveListResource] ALTER COLUMN [iResourceId] int NOT NULL
GO

select * from tReserveListResource



-------------------------------------------------------------------
-- DEV ONLY 
-------------------------------------------------------------------
select * from tInstitutionPdaResource

insert into tInstitutionResourceLicense (iInstitutionId, iResourceId, iLicenseCount
            , tiLicenseTypeId, tiLicenseOriginalSourceId, dtFirstPurchaseDate, dtPdaAddedDate
            , dtPdaAddedToCartDate, vchPdaAddedToCartById, iPdaViewCount, iPdaMaxViews
            , dtCreationDate, vchCreatorId, dtLastUpdate
            , vchUpdaterId, tiRecordStatus)
    select iInstitutionId, iResourceId, 0
         , 3, 2, null, dtCreationDate
         , dtAddedToCartDate, vchAddedToCartById, iViewCount, iMaxViews
         , dtCreationDate, vchCreatorId, null
         , null, 1
    from   tInstitutionPdaResource ipr
    where  not exists 
        (select *
         from   tInstitutionResourceLicense irl
         where  irl.iInstitutionId = ipr.iInstitutionId and irl.iResourceId = ipr.iResourceId)
      and  tiRecordStatus = 1
         

--update tInstitutionResourceLicense set iLicenseCount = iPdaMaxViews where tiLicenseTypeId = 3
--update tInstitutionResourceLicense set iLicenseCount = 0 where tiLicenseTypeId = 3

-- select * from tInstitutionResourceLicense where tiLicenseTypeId = 3
-- select * from tInstitutionResourceLicense where tiLicenseTypeId = 1

---------------------------------------------------------------------------
ALTER TABLE [dbo].[tInstitution] ADD [tiPdaEulaSigned] tinyint NOT NULL DEFAULT 0
GO

CREATE TABLE [dbo].[tInstitutionAuditType] (
[iInstitutionAuditTypeId] tinyint NOT NULL,
[AuditTypeDescription] varchar(50) NOT NULL,
CONSTRAINT [PK__tInstitu__932F0D0D0D19A41C]
PRIMARY KEY CLUSTERED ([iInstitutionAuditTypeId] ASC)
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
ALTER TABLE [dbo].[tInstitutionAuditType] SET (LOCK_ESCALATION = TABLE);
GO

insert into tInstitutionAuditType (iInstitutionAuditTypeId, AuditTypeDescription) values (1, 'Signed R2 Library EULA');
insert into tInstitutionAuditType (iInstitutionAuditTypeId, AuditTypeDescription) values (2, 'Signed R2 PDA Library EULA');

CREATE TABLE [dbo].[tInstitutionAudit] (
[iInstitutionAuditId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[tiInstitutionAuditTypeId] tinyint NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchEventDescription] varchar(1000) NOT NULL,
CONSTRAINT [PK_tInstitutionAudit]
PRIMARY KEY CLUSTERED ([iInstitutionAuditId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionAudit_Type]
FOREIGN KEY ([tiInstitutionAuditTypeId])
REFERENCES [dbo].[tInstitutionResourceAuditType] ( [iInstitutionResourceAuditTypeId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionAudit] SET (LOCK_ESCALATION = TABLE);
GO

----------------------------------------------------------------
-- Cart Mods
----------------------------------------------------------------
ALTER TABLE [dbo].[tCartItem] ADD [tiLicenseOriginalSourceId] tinyint NULL DEFAULT 1
GO

update tCartItem set tiLicenseOriginalSourceId = 1 

ALTER TABLE [dbo].[tCartItem] 
ADD  CONSTRAINT [FK_tCartItem_tLicenseOriginalSource]
FOREIGN KEY ([tiLicenseOriginalSourceId])
REFERENCES [dbo].[tLicenseOriginalSource] ( [tiLicenseOriginalSourceId] )
GO

select * from tCartItem


----------------------------------------------------------------
-- Issue 238 - resource usage reports dates for Rittenhouse account
----------------------------------------------------------------


insert into tInstitutionResourceLicense (iInstitutionId, iResourceId, iLicenseCount, tiLicenseTypeId, tiLicenseOriginalSourceId
        , dtFirstPurchaseDate, dtPdaAddedDate, dtPdaAddedToCartDate, vchPdaAddedToCartById, iPdaViewCount, iPdaMaxViews
        , dtCreationDate, vchCreatorId, dtLastUpdate, vchUpdaterId, tiRecordStatus) 
        
    select 50, r.iResourceId, 3, 1, 1
            , case when (r.dtRISReleaseDate is null) then r.dtCreationDate else r.dtRISReleaseDate end
            , null, null, null, 0, 0
            , getdate(), 'R2PromoteAutoLicense', null, null, 1
    from   tResource r 
    where  r.tiRecordStatus = 1 
      and  r.iResourceStatusId in (6,7) 
      and  r.NotSaleable = 0 
      and  not exists ( 
        select * 
        from   tInstitutionResourceLicense irl2
         join  dbo.tInstitution i2 on i2.iInstitutionId = irl2.iInstitutionId 
        where  i2.tiAutoAddFreeLicenses = 1 
          and  i2.iInstitutionId = 50 
          and  irl2.tiRecordStatus = 1 
          and  irl2.iResourceId = r.iResourceId 
        )
    order by r.iResourceId

select * from tInstitutionResourceLicense
where iInstitutionId = 50
order by dtFirstPurchaseDate

select * from tInstitution where iInstitutionId = 50

select * from tInstitutionResourceLicense where iInstitutionId = 50

delete from tInstitutionResourceLicense where iInstitutionId = 50