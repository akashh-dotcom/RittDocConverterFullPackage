alter table tresourcecollection
add vchdata varchar(1000) null;

CREATE TABLE [dbo].[tCollection] (
[iCollectionId] int NOT NULL,
[vchCollectionName] varchar(100) NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
[tiHideInFilter] tinyint NOT NULL DEFAULT ((0)),
[iSequenceNumber] int NULL,
CONSTRAINT [PK_tCollection]
PRIMARY KEY CLUSTERED ([iCollectionId] ASC)
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
ALTER TABLE [dbo].[tCollection] SET (LOCK_ESCALATION = TABLE);
GO


Insert Into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
values(1, 'Clinical Cornerstones', 'Ken Haberle', GETDATE(), 1, 1, 5);
Insert Into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
values(2, 'Noteworthy Nursing', 'Ken Haberle', GETDATE(), 1, 1, 6);
Insert Into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
values(3, 'AJN Books of the Year', 'Ken Haberle', GETDATE(), 1, 0, 4);
Insert Into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
values(4, 'Former Brandon Hill', 'Ken Haberle', GETDATE(), 1, 0, 1);
Insert Into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
values(5, 'DCT', 'Ken Haberle', GETDATE(), 1, 0, 2);
Insert Into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
values(6, 'DCT Essentials', 'Ken Haberle', GETDATE(), 1, 0, 3);
INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
VALUES(7, 'Best of 2015', 'Ken Haberle', getdate(), 1, 1, 7);
INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
VALUES(8, 'Nursing Essentials', 'Ken Haberle', getdate(), 1, 1, 8);
INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
VALUES(9, 'Hospital Essentials', 'Ken Haberle', getdate(), 1, 1, 9);
INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber)
VALUES(10, 'Medical Essentials', 'Ken Haberle', getdate(), 1, 1, 10);

exec sp_RENAME 'tResourceCollection.iResourceCollectionTypeId' , 'iCollectionId', 'COLUMN';

ALTER TABLE tResourceCollection
DROP CONSTRAINT FK_tResourceCollection_Type;

ALTER TABLE tResourceCollection
ADD FOREIGN KEY (iCollectionId)
REFERENCES tCollection(iCollectionId);

insert into tResourceCollection (iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 5, r.iResourceId, 'Initial Load', getdate(), 1
from tResource r
where r.iDCTStatusId = 158 and r.tiRecordStatus = 1

insert into tResourceCollection (iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 6, r.iResourceId, 'Initial Load', getdate(), 1
from tResource r
where r.iDCTStatusId = 159 and r.tiRecordStatus = 1

insert into tResourceCollection (iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 4, r.iResourceId, 'Initial Load', getdate(), 1
from tResource r
where r.tiBrandonHillStatus = 1 and r.tiRecordStatus = 1

INSERT INTO [dbo].[tInstitutionResourceAuditType]
           ([iInstitutionResourceAuditTypeId]
           ,[AuditTypeDescription])
     VALUES
           ( 15
           , 'Added resource to PDA via Profile');







CREATE TABLE [dbo].[tPdaRule] (
[iPdaRuleId] int IDENTITY(1, 1) NOT NULL,
[vchRuleName] varchar(255) NOT NULL,
[decMaxPrice] decimal(18, 0) NULL,
[tiFuture] tinyint NOT NULL,
[tiNewEditionFirm] tinyint NOT NULL,
[tiNewEditionPda] tinyint NOT NULL,
[iInstitutionId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [FK_tPdaRule_iInstitutionId]
FOREIGN KEY ([iInstitutionId])
REFERENCES [dbo].[tInstitution] ( [iInstitutionId] ),
CONSTRAINT [PK_tPdaRule]
PRIMARY KEY CLUSTERED ([iPdaRuleId] ASC)
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
ALTER TABLE [dbo].[tPdaRule] SET (LOCK_ESCALATION = TABLE);
GO


CREATE TABLE [dbo].[tPdaRuleSpecialty] (
[iPdaRuleSpecialtyId] int IDENTITY(1, 1) NOT NULL,
[iPdaRuleId] int NULL,
[iSpecialtyId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [FK_tPdaRuleSpecialty_iSpecialtyId]
FOREIGN KEY ([iSpecialtyId])
REFERENCES [dbo].[tSpecialty] ( [iSpecialtyId] ),
CONSTRAINT [PK_tPdaRuleSpecialty]
PRIMARY KEY CLUSTERED ([iPdaRuleSpecialtyId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tPdaRuleSpecialty_iPdaRule]
FOREIGN KEY ([iPdaRuleId])
REFERENCES [dbo].[tPdaRule] ( [iPdaRuleId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tPdaRuleSpecialty] SET (LOCK_ESCALATION = TABLE);
GO


CREATE TABLE [dbo].[tPdaRulePracticeArea] (
[iPdaRulePracticeAreaId] int IDENTITY(1, 1) NOT NULL,
[iPdaRuleId] int NULL,
[iPracticeAreaId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [FK_tPdaRulePracticeArea_iPracticeAreaId]
FOREIGN KEY ([iPracticeAreaId])
REFERENCES [dbo].[tPracticeArea] ( [iPracticeAreaId] ),
CONSTRAINT [PK_tPdaRulePracticeArea]
PRIMARY KEY CLUSTERED ([iPdaRulePracticeAreaId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tPdaRulePracticeArea_iPdaRule]
FOREIGN KEY ([iPdaRuleId])
REFERENCES [dbo].[tPdaRule] ( [iPdaRuleId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tPdaRulePracticeArea] SET (LOCK_ESCALATION = TABLE);
GO


CREATE TABLE [dbo].[tPdaRuleCollection] (
[iPdaRuleCollectionId] int IDENTITY(1, 1) NOT NULL,
[iPdaRuleId] int NULL,
[iCollectionId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [FK_tPdaRule_iCollectionId]
FOREIGN KEY ([iCollectionId])
REFERENCES [dbo].[tCollection] ( [iCollectionId] ),
CONSTRAINT [PK_tPdaRuleCollection]
PRIMARY KEY CLUSTERED ([iPdaRuleCollectionId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tPdaRuleCollection_iPdaRuleId]
FOREIGN KEY ([iPdaRuleId])
REFERENCES [dbo].[tPdaRule] ( [iPdaRuleId] )
)
ON [PRIMARY];
GO
ALTER TABLE [dbo].[tPdaRuleCollection] SET (LOCK_ESCALATION = TABLE);
GO

alter table tInstitutionResourceLicense
add dtPdaRuleDateAdded smalldatetime null;

alter table tInstitutionResourceLicense
add iPdaRuleId int null;


alter table tResource
alter column tiBrandonHillStatus tinyint null;





