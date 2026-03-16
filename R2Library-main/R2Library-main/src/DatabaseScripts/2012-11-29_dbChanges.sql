ALTER TABLE tResource
ADD vchPageCount  varchar(20) null;


CREATE TABLE [dbo].[tPing] (
[iPingId] int NOT NULL,
[vchStatusCode] varchar(50) NOT NULL,
CONSTRAINT [PK__tPing__37EC55E847FC2F28]
PRIMARY KEY CLUSTERED ([iPingId] ASC)
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
ALTER TABLE [dbo].[tPing] SET (LOCK_ESCALATION = TABLE);
GO

--truncate table tPing
insert into tPing (iPingId,vchStatusCode) values(1, 'R2v2DbStatusOk')


/*
Missing Index Details from SQLQuery1.sql - RITTDB3.RIT001 (R2LIBRARY\Scott.Scheider (125))
The Query Processor estimates that implementing the following index could improve the query cost by 16.1276%.
*/

/*
CREATE NONCLUSTERED INDEX [IX_tInstitution_Trial]
ON [dbo].[tInstitution] ([iInstitutionAcctStatusId],[tiRecordStatus],[dtTrialAcctStart],[dtTrialAcctEnd])
INCLUDE ([iInstitutionId])
GO
*/



CREATE TABLE [dbo].[tTerritory] (
[iTerritoryId] int IDENTITY(1, 1) NOT NULL,
[vchTerritoryCode] varchar(10) NOT NULL,
[vchTerritoryName] varchar(50) NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tTerritory]
PRIMARY KEY CLUSTERED ([iTerritoryId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [UK_tTerritory_Code]
UNIQUE NONCLUSTERED ([vchTerritoryCode] ASC)
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
ALTER TABLE [dbo].[tTerritory] SET (LOCK_ESCALATION = TABLE);
GO


SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [dbo].[tUserTerritory] (
[iUserTerritoryId] int IDENTITY(1, 1) NOT NULL,
[iUserId] int NOT NULL,
[iTerritoryId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [FK_tUserTerritory_tUser]
FOREIGN KEY ([iUserId])
REFERENCES [dbo].[tUser] ( [iUserId] ),
CONSTRAINT [FK_tUserTerritory_tTerritory]
FOREIGN KEY ([iTerritoryId])
REFERENCES [dbo].[tTerritory] ( [iTerritoryId] ),
CONSTRAINT [PK_tUserTer]
PRIMARY KEY CLUSTERED ([iUserTerritoryId] ASC)
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
ALTER TABLE [dbo].[tUserTerritory] SET (LOCK_ESCALATION = TABLE);
GO

CREATE TABLE [dbo].[tInstitutionTerritory] (
[iInstitutionTerritoryId] int IDENTITY(1, 1) NOT NULL,
[iInstitutionId] int NOT NULL,
[iTerritoryId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tInstitutionTerritory]
PRIMARY KEY CLUSTERED ([iInstitutionTerritoryId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tInstitutionTerritory_tInstitution]
FOREIGN KEY ([iInstitutionId])
REFERENCES [dbo].[tInstitution] ( [iInstitutionId] ),
CONSTRAINT [FK_tInstitutionTerritory_tTerriroty]
FOREIGN KEY ([iTerritoryId])
REFERENCES [dbo].[tTerritory] ( [iTerritoryId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tInstitutionTerritory] SET (LOCK_ESCALATION = TABLE);
GO


insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('A', 'Territory A', 'sjscheider', getdate(), null, null, 1)

insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('B', 'Territory B', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('C', 'Territory C', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('D', 'Territory D', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('E', 'Territory E', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('F', 'Territory F', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('G', 'Territory G', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('H', 'Territory H', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('I', 'Territory I', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('J', 'Territory J', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('K', 'Territory K', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('L', 'Territory L', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('M', 'Territory M', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('N', 'Territory N', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('R', 'Territory R', 'sjscheider', getdate(), null, null, 1)
insert into tTerritory (vchTerritoryCode, vchTerritoryName, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) values('T', 'Territory T', 'sjscheider', getdate(), null, null, 1)

CREATE TABLE [dbo].[tempTerritory] (
[code] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[accountNumber] varchar(50) NULL)
ON [PRIMARY]
WITH (DATA_COMPRESSION = NONE);
GO
ALTER TABLE [dbo].[tempTerritory] SET (LOCK_ESCALATION = TABLE);
GO

BULK INSERT [RIT001_2012-08-22].[dbo].[tempTerritory]
   FROM 'd:\Clients\Rittenhouse\R2Library\TerritoryAndAccountNumber.csv'
   WITH 
      (
         FIELDTERMINATOR =',',
         ROWTERMINATOR ='\n'
      )

insert into tInstitutionTerritory (iInstitutionId, iTerritoryId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
    select i.iInstitutionId, t.iTerritoryId, 'sjscheider', getdate(), null, null, 1
    from   tInstitution i 
     join  tempTerritory tt on i.vchInstitutionAcctNum = tt.accountNumber
     join  tTerritory t on t.vchTerritoryCode = tt.code
 
select * from tempTerritory

-------------------------------------------------------------------------
create table dbo.tProductSubscription( 
iProductSubscriptionId int IDENTITY(1,1) NOT NULL,
iProductId int NOT NULL,
iInstitutionId int NOT NULL,
dtStartDate datetime NOT NULL,
dtEndDate datetime NOT NULL,
[vchCreatorId] [varchar](50) NOT NULL,
[dtCreationDate] [smalldatetime] NOT NULL,
[vchUpdaterId] [varchar](50) NULL,
[dtLastUpdate] [smalldatetime] NULL,
[tiRecordStatus] [tinyint] NOT NULL
 CONSTRAINT [PK_tProductSubscription] PRIMARY KEY CLUSTERED 
(
	iProductSubscriptionId ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]


alter table tproductsubscription 
add iProductSubscriptionStatusId int NOT NULL

-------------------------------------------------------------------------

ALTER TABLE [dbo].[tInstitution] ADD [iTerritoryId] int NULL
GO
ALTER TABLE [dbo].[tInstitution] 
ADD  CONSTRAINT [FK_tInstitution_tTerritory]
FOREIGN KEY ([iTerritoryId])
REFERENCES [dbo].[tTerritory] ( [iTerritoryId] )
GO

update i 
set    i.iTerritoryId = t.iTerritoryId
from   tInstitution i 
 join  tempTerritory tt on i.vchInstitutionAcctNum = tt.accountNumber
 join  tTerritory t on t.vchTerritoryCode = tt.code




