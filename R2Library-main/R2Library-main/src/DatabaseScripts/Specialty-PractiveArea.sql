SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [tPracticeArea] (
[iPracticeAreaId] int NOT NULL,
[vchPracticeAreaCode] varchar(20) NULL,
[vchPracticeAreaName] varchar(100) NOT NULL,
[iSequenceNumber] int NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tPracticeArea]
PRIMARY KEY CLUSTERED ([iPracticeAreaId] ASC)
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

insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (1, 'MED', 'Medicine', 10, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (2, 'NURSE', 'Nursing', 20, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (3, 'ALLIED', 'Allied Health', 30, 'sjscheider', getdate(), NULL, NULL, 1);

insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (4, 'CONSUMER', 'Consumer Health', 40, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (5, 'VET', 'Veterinary Medicine', 50, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (6, 'DENT', 'Dentistry', 60, 'sjscheider', getdate(), NULL, NULL, 1);

insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (7, 'CHIRO', 'Chiropractic Medicine', 70, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (8, 'OSTEO', 'Osteopathic Medicine', 80, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (9, 'PHARMA', 'Pharmacy', 90, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
values (10, 'PODIA', 'Podiatry', 100, 'sjscheider', getdate(), NULL, NULL, 1);

--truncate table tPracticeArea

-- select * from tPracticeArea

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [tResourcePracticeArea] (
[iResourcePracticeAreaId] int IDENTITY(1, 1) NOT NULL,
[iResourceId] int NULL,
[iPracticeAreaId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [FK_tResourcePracticeArea_tReource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [PK_tResourcePracticeArea]
PRIMARY KEY CLUSTERED ([iResourcePracticeAreaId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tResourcePracticeArea_tPracticeArea]
FOREIGN KEY ([iPracticeAreaId])
REFERENCES [dbo].[tPracticeArea] ( [iPracticeAreaId] )
)
ON [PRIMARY];
GO


-- Medicine
insert into tResourcePracticeArea (iResourceId,iPracticeAreaId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) 
   select r.iResourceId, 1, 'sjscheider', getdate(), NULL, NULL, 1
   from   tResource r
    join  dbo.tResourceDiscipline rd on rd.iResourceId = r.iResourceId and rd.tiRecordStatus = 1
    join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId and ld.tiRecordStatus = 1
   where  ld.iLibraryId = 1
     and  ld.iDisciplineId not in (196,144,68,197)

-- Nursing
insert into tResourcePracticeArea (iResourceId,iPracticeAreaId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) 
   select r.iResourceId, 2, 'sjscheider', getdate(), NULL, NULL, 1
   from   tResource r
    join  dbo.tResourceDiscipline rd on rd.iResourceId = r.iResourceId and rd.tiRecordStatus = 1
    join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId and ld.tiRecordStatus = 1
   where  ld.iLibraryId = 2
     and  ld.iDisciplineId not in (196,144,68,197)

-- Allied Health
insert into tResourcePracticeArea (iResourceId,iPracticeAreaId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) 
   select r.iResourceId, 3, 'sjscheider', getdate(), NULL, NULL, 1
   from   tResource r
    join  dbo.tResourceDiscipline rd on rd.iResourceId = r.iResourceId and rd.tiRecordStatus = 1
    join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId and ld.tiRecordStatus = 1
   where  ld.iLibraryId = 3
     and  ld.iDisciplineId not in (196,144,68,197)

-- Consumer Health
insert into tResourcePracticeArea (iResourceId,iPracticeAreaId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) 
   select r.iResourceId, 4, 'sjscheider', getdate(), NULL, NULL, 1
   from   tResource r
    join  dbo.tResourceDiscipline rd on rd.iResourceId = r.iResourceId and rd.tiRecordStatus = 1
    join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId and ld.tiRecordStatus = 1
   where  ld.iDisciplineId in (144,68)

-- Veterinary Medicine
insert into tResourcePracticeArea (iResourceId,iPracticeAreaId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) 
   select r.iResourceId, 5, 'sjscheider', getdate(), NULL, NULL, 1
   from   tResource r
    join  dbo.tResourceDiscipline rd on rd.iResourceId = r.iResourceId and rd.tiRecordStatus = 1
    join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId and ld.tiRecordStatus = 1
   where  ld.iDisciplineId in (197)

-- Dentistry
insert into tResourcePracticeArea (iResourceId,iPracticeAreaId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) 
   select r.iResourceId, 6, 'sjscheider', getdate(), NULL, NULL, 1
   from   tResource r
    join  dbo.tResourceDiscipline rd on rd.iResourceId = r.iResourceId and rd.tiRecordStatus = 1
    join  dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId and ld.tiRecordStatus = 1
   where  ld.iDisciplineId in (196)

-----------------------------------------------------------------------
-----------------------------------------------------------------------
-----------------------------------------------------------------------
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [tSpecialty] (
[iSpecialtyId] int NOT NULL,
[vchSpecialtyCode] varchar(20) NULL,
[vchSpecialtyName] varchar(100) NOT NULL,
[iSequenceNumber] int NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tSpecialty]
PRIMARY KEY CLUSTERED ([iSpecialtyId] ASC)
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

delete from tSpecialty

insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(193,null,'Pharmaceutical Chemistry',193, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(10,null,'AIDS',9, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(11,null,'Allergy',10, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(12,null,'Alternative/ Complementary Medicine',11, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(13,null,'Anatomy',12, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(14,null,'Anesthesiology',13, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(157,null,'Athletic Training',157, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(16,null,'Biochemistry',15, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(17,null,'Biology',16, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(18,null,'Biotechnology',17, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(109,null,'Cardiovascular',109, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(110,null,'Care Plans',110, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(20,null,'Chemistry',19, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(190,null,'Coding / Reimbursement',190, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(111,null,'Communication',111, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(112,null,'Public/Community Health',112, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(21,null,'Computers In Medicine',20, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(22,null,'Critical Care and Intensive Care',21, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(113,null,'Critical Care',113, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(161,null,'Dental Hygiene',161, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(196,null,'Dentistry',22, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(23,null,'Dermatology',23, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(24,null,'Diabetes Mellitus / Diabetes Insipidus',24, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(25,null,'Diagnosis',25, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(163,null,'Dictionaries / Terminology',163, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(116,null,'Nutrition and Dietetics',116, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(27,null,'Directories',27, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(117,null,'Dosages & Solutions',117, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(118,null,'Education',118, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(169,null,'Emergency Medical Services',169, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(119,null,'Emergency',119, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(30,null,'Endocrinology',30, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(31,null,'Epidemiology',31, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(32,null,'Ethics',32, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(34,null,'Family Practice',34, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(121,null,'Family Nursing',121, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(135,null,'Fluids & Electrolytes',135, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(35,null,'Forensic Medicine',35, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(123,null,'Fundamentals',123, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(36,null,'Gastroenterology',36, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(37,null,'Genetics',37, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(122,null,'Geriatrics',122, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(39,null,'Hematology',39, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(40,null,'Histology',40, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(42,null,'Hospitals & Administration',42, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(43,null,'Immunology & Serology',43, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(44,null,'Infectious Disease',44, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(45,null,'Internal Medicine',45, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(125,null,'Laboratory Diagnosis',125, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(46,null,'Laboratory Medicine',46, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(47,null,'Legal Medicine',47, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(195,null,'Massage Therapy',195, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(127,null,'Maternal Child Nursing',127, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(128,null,'Medical Surgical Nursing',128, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(171,null,'Medical Assisting',171, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(174,null,'Medical Records Administration',174, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(173,null,'Medical Technology',173, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(50,null,'Metabolism',50, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(51,null,'Microbiology',51, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(53,null,'Molecular Biology',53, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(52,null,'Musculoskeletal System',52, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(54,null,'Neonatology',54, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(55,null,'Nephrology',55, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(57,null,'Neuroanatomy / Neurophysiology',57, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(56,null,'Neurology',56, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(107,null,'Nurse Anesthetist',107, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(106,null,'Nurse Midwife',106, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(108,null,'Nurse Practitioner',108, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(136,null,'Nursing As A Profession',136, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(177,null,'Nursing Assisting',177, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(104,null,'Nursing: Administration & Management',104, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(105,null,'Nursing: Advanced Practice',105, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(59,null,'Obstetrics & Gynecology',59, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(60,null,'Occupational Medicine',60, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(178,null,'Occupational Therapy',178, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(141,null,'Oncology',141, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(180,null,'Ophthalmic Technology',180, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(62,null,'Ophthalmology',62, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(64,null,'Orthopedic Surgery',64, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(63,null,'Orthopedics',63, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(65,null,'Otorhinolaryngology',65, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(66,null,'Palliative Care',66, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(69,null,'Pathology',69, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(144,null,'Patient Education',144, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(70,null,'Pediatrics',70, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(131,null,'Perioperative',131, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(71,null,'Pharmacology',71, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(194,null,'Pharmacy Technology',194, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(192,null,'Pharmacy',192, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(72,null,'Physical Medicine and Rehabilitation',72, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(183,null,'Physical Therapy',183, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(191,null,'Physician Assistant',191, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(73,null,'Physiology',73, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(147,null,'Practical / Vocational Nursing',147, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(74,null,'Practice of Medicine',74, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(99,null,'Primary Care',99, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(137,null,'Nursing Process/Diagnosis',137, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(76,null,'Psychiatry',76, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(77,null,'Psychology',77, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(78,null,'Psychopharmacology',78, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(100,null,'Pulmonary Medicine',100, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(184,null,'Radiologic Technology',184, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(80,null,'Radiology',80, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(81,null,'Radiology: General & Diagnostic',81, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(83,null,'Radiology: MRI / CT',83, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(82,null,'Radiology: Nuclear Medicine',82, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(84,null,'Radiology: Ultrasound',84, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(149,null,'Reference',149, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(101,null,'Research',101, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(85,null,'Residency Planning',85, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(86,null,'Respiratory / Pulmonary System',86, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(186,null,'Respiratory Therapy Technology',186, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(185,null,'Respiratory Therapy',185, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(87,null,'Rheumatology',87, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(187,null,'Speech Therapy / Audiology',187, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(88,null,'Sports Medicine',88, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(95,null,'Statistics',95, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(90,null,'Surgery',90, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(91,null,'Surgery: Gastroenterologic',91, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(92,null,'Surgery: Neurologic',92, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(93,null,'Surgery: Plastic & Reconstructive',93, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(94,null,'Surgery: Thoracic / Cardiovascular',94, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(188,null,'Surgical Technology',188, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(139,null,'Nursing Theory',139, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(96,null,'Toxicology',96, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(153,null,'Transcultural Nursing',153, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(97,null,'Tropical Medicine',97, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(98,null,'Urology',98, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(197,null,'Veterinary Medicine',196, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(156,null,'Anesthesiology Assisting',156, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(15,null,'Aviation & Space Medicine',14, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(158,null,'Blood Bank Technology',158, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(159,null,'Cardiovascular Technology',159, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(160,null,'Cytotechnology',160, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(26,null,'Dictionaries',26, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(167,null,'EEG Technology',167, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(33,null,'Ethnic Medicine',33, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(41,null,'History of Medicine',41, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(124,null,'Home Health',124, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(172,null,'Medical Illustration',172, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(175,null,'Medical Transcription',175, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(48,null,'Medical Sociology',48, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(49,null,'Medical Trade',49, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(176,null,'Nuclear Medicine Assisting',176, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(181,null,'Orthotist / Prosthetist',181, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(67,null,'Parasitology',67, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(182,null,'Perfusionist',182, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(75,null,'Practice Management',75, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(89,null,'Substance Abuse',89, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(189,null,'Surgical assisting',189, 'sjscheider', getdate(), NULL, NULL, 1);
insert into tSpecialty (iSpecialtyId,vchSpecialtyCode,vchSpecialtyName,iSequenceNumber,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus) values(133,null,'Transplantation',133, 'sjscheider', getdate(), NULL, NULL, 1);


-----------------------------------------------------------------------
-----------------------------------------------------------------------
-----------------------------------------------------------------------
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE [tResourceSpecialty] (
[iResourceSpecialtyId] int IDENTITY(1, 1) NOT NULL,
[iResourceId] int NULL,
[iSpecialtyId] int NOT NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tResourceSpecialty]
PRIMARY KEY CLUSTERED ([iResourceSpecialtyId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 80,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY],
CONSTRAINT [FK_tResourceSpecialty_tResource]
FOREIGN KEY ([iResourceId])
REFERENCES [dbo].[tResource] ( [iResourceId] ),
CONSTRAINT [FK_tResourceSpecialty_tSpecialty]
FOREIGN KEY ([iSpecialtyId])
REFERENCES [dbo].[tSpecialty] ( [iSpecialtyId] )
)
ON [PRIMARY];
GO

truncate table tResourceSpecialty

insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 193, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (193);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 10, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (10,103);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 11, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (11);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 12, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (12);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 13, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (13);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 14, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (14);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 157, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (157);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 16, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (16);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 17, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (17);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 18, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (18);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 109, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (109, 19);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 110, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (110);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 20, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (20);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 190, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (190);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 111, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (111);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 112, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (112, 79);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 21, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (21);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 22, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (22);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 113, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (113);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 161, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (161);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 196, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (196);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 23, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (23);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 24, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (24);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 25, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (25);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 163, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (163, 114);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 116, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (116,165,58);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 27, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (27);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 117, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (117);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 118, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (118,166,28);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 169, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (169);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 119, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (119,29);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 30, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (30);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 31, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (31);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 32, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (32, 120);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 34, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (34);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 121, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (121);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 135, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (135);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 35, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (35);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 123, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (123);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 36, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (36);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 37, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (37);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 122, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (122,38);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 39, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (39);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 40, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (40170);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 42, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (42);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 43, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (43);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 44, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (44);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 45, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (45);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 125, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (125);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 46, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (46);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 47, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (47,126);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 195, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (195);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 127, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (127);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 128, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (128);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 171, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (171);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 174, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (174);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 173, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (173);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 50, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (50);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 51, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (51);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 53, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (53);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 52, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (52);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 54, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (54);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 55, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (55);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 57, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (57);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 56, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (56,130);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 107, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (107);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 106, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (106);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 108, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (108);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 136, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (136);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 177, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (177);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 104, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (104);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 105, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (105);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 59, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (59,140);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 60, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (60);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 178, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (178,179);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 141, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (141,61);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 180, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (180);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 62, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (62);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 64, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (64);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 63, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (63,142);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 65, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (65);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 66, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (66,143,151);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 69, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (69);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 144, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (144,68);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 70, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (70,145);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 131, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (131);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 71, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (71,146,129);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 194, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (194);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 192, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (192);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 72, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (72,150);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 183, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (183);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 191, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (191);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 73, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (73);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 147, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (147);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 74, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (74);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 99, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (99);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 137, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (137);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 76, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (76,148);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 77, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (77);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 78, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (78);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 100, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (100);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 184, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (184,162);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 80, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (80);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 81, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (81);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 83, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (83);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 82, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (82);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 84, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (84);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 149, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (149);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 101, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (101,138);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 85, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (85);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 86, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (86,132);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 186, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (186);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 185, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (185);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 87, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (87);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 187, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (187);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 88, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (88);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 95, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (95,151);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 90, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (90);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 91, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (91);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 92, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (92);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 93, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (93);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 94, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (94);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 188, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (188);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 139, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (139);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 96, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (96);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 153, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (153);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 97, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (97);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 98, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (98,134);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 197, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (197);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 156, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (156);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 15, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (15);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 158, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (158);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 159, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (159);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 160, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (160);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 26, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (26,115,164);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 167, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (167,168);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 33, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (33);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 41, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (41);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 124, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (124);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 172, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (172);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 175, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (175);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 48, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (48);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 49, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (49);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 176, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (176);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 181, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (181);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 67, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (67);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 182, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (182);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 75, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (75);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 89, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (89);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 189, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (189);
insert into tResourceSpecialty (iResourceId,iSpecialtyId,vchCreatorId,dtCreationDate,vchUpdaterId,dtLastUpdate,tiRecordStatus)  select rd.iResourceId, 133, 'sjscheider', getdate(), NULL, NULL, 1 from   tResourceDiscipline rd join dbo.tLibraryDiscipline ld on ld.iLibraryDisciplineId = rd.iLibraryDisciplineId where ld.tiRecordStatus = 1 and rd.tiRecordStatus = 1 and ld.iDisciplineId in (133);


select *
from   tDiscipline 
where  iDisciplineId not in (0,19,28,29,38,58,61,68,79,103,114,115,120,130,132,134,138,140,143,145,146,148,151,152,164,165,166)
  and  iDisciplineId not in (select iSpecialtyId from tSpecialty)

update tSpecialty set vchSpecialtyCode = 'R2D0010' where iSpecialtyId = 10
update tSpecialty set vchSpecialtyCode = 'R2D0011' where iSpecialtyId = 11
update tSpecialty set vchSpecialtyCode = 'R2D0012' where iSpecialtyId = 12
update tSpecialty set vchSpecialtyCode = 'R2D0013' where iSpecialtyId = 13
update tSpecialty set vchSpecialtyCode = 'R2D0014' where iSpecialtyId = 14
update tSpecialty set vchSpecialtyCode = 'R2D0157' where iSpecialtyId = 157
update tSpecialty set vchSpecialtyCode = 'R2D0016' where iSpecialtyId = 16
update tSpecialty set vchSpecialtyCode = 'R2D0017' where iSpecialtyId = 17
update tSpecialty set vchSpecialtyCode = 'R2D0018' where iSpecialtyId = 18
update tSpecialty set vchSpecialtyCode = 'R2D0109' where iSpecialtyId = 109
update tSpecialty set vchSpecialtyCode = 'R2D0110' where iSpecialtyId = 110
update tSpecialty set vchSpecialtyCode = 'R2D0020' where iSpecialtyId = 20
update tSpecialty set vchSpecialtyCode = 'R2D0190' where iSpecialtyId = 190
update tSpecialty set vchSpecialtyCode = 'R2D0111' where iSpecialtyId = 111
update tSpecialty set vchSpecialtyCode = 'R2D0112' where iSpecialtyId = 112
update tSpecialty set vchSpecialtyCode = 'R2D0021' where iSpecialtyId = 21
update tSpecialty set vchSpecialtyCode = 'R2D0022' where iSpecialtyId = 22
update tSpecialty set vchSpecialtyCode = 'R2D0113' where iSpecialtyId = 113
update tSpecialty set vchSpecialtyCode = 'R2D0161' where iSpecialtyId = 161
update tSpecialty set vchSpecialtyCode = 'R2D0196' where iSpecialtyId = 196
update tSpecialty set vchSpecialtyCode = 'R2D0023' where iSpecialtyId = 23
update tSpecialty set vchSpecialtyCode = 'R2D0024' where iSpecialtyId = 24
update tSpecialty set vchSpecialtyCode = 'R2D0025' where iSpecialtyId = 25
update tSpecialty set vchSpecialtyCode = 'R2D0163' where iSpecialtyId = 163
update tSpecialty set vchSpecialtyCode = 'R2D0116' where iSpecialtyId = 116
update tSpecialty set vchSpecialtyCode = 'R2D0027' where iSpecialtyId = 27
update tSpecialty set vchSpecialtyCode = 'R2D0117' where iSpecialtyId = 117
update tSpecialty set vchSpecialtyCode = 'R2D0118' where iSpecialtyId = 118
update tSpecialty set vchSpecialtyCode = 'R2D0169' where iSpecialtyId = 169
update tSpecialty set vchSpecialtyCode = 'R2D0119' where iSpecialtyId = 119
update tSpecialty set vchSpecialtyCode = 'R2D0030' where iSpecialtyId = 30
update tSpecialty set vchSpecialtyCode = 'R2D0031' where iSpecialtyId = 31
update tSpecialty set vchSpecialtyCode = 'R2D0032' where iSpecialtyId = 32
update tSpecialty set vchSpecialtyCode = 'R2D0034' where iSpecialtyId = 34
update tSpecialty set vchSpecialtyCode = 'R2D0121' where iSpecialtyId = 121
update tSpecialty set vchSpecialtyCode = 'R2D0135' where iSpecialtyId = 135
update tSpecialty set vchSpecialtyCode = 'R2D0035' where iSpecialtyId = 35
update tSpecialty set vchSpecialtyCode = 'R2D0123' where iSpecialtyId = 123
update tSpecialty set vchSpecialtyCode = 'R2D0036' where iSpecialtyId = 36
update tSpecialty set vchSpecialtyCode = 'R2D0037' where iSpecialtyId = 37
update tSpecialty set vchSpecialtyCode = 'R2D0122' where iSpecialtyId = 122
update tSpecialty set vchSpecialtyCode = 'R2D0039' where iSpecialtyId = 39
update tSpecialty set vchSpecialtyCode = 'R2D0040' where iSpecialtyId = 40
update tSpecialty set vchSpecialtyCode = 'R2D0042' where iSpecialtyId = 42
update tSpecialty set vchSpecialtyCode = 'R2D0043' where iSpecialtyId = 43
update tSpecialty set vchSpecialtyCode = 'R2D0044' where iSpecialtyId = 44
update tSpecialty set vchSpecialtyCode = 'R2D0045' where iSpecialtyId = 45
update tSpecialty set vchSpecialtyCode = 'R2D0125' where iSpecialtyId = 125
update tSpecialty set vchSpecialtyCode = 'R2D0046' where iSpecialtyId = 46
update tSpecialty set vchSpecialtyCode = 'R2D0047' where iSpecialtyId = 47
update tSpecialty set vchSpecialtyCode = 'R2D0195' where iSpecialtyId = 195
update tSpecialty set vchSpecialtyCode = 'R2D0127' where iSpecialtyId = 127
update tSpecialty set vchSpecialtyCode = 'R2D0128' where iSpecialtyId = 128
update tSpecialty set vchSpecialtyCode = 'R2D0171' where iSpecialtyId = 171
update tSpecialty set vchSpecialtyCode = 'R2D0174' where iSpecialtyId = 174
update tSpecialty set vchSpecialtyCode = 'R2D0173' where iSpecialtyId = 173
update tSpecialty set vchSpecialtyCode = 'R2D0050' where iSpecialtyId = 50
update tSpecialty set vchSpecialtyCode = 'R2D0051' where iSpecialtyId = 51
update tSpecialty set vchSpecialtyCode = 'R2D0053' where iSpecialtyId = 53
update tSpecialty set vchSpecialtyCode = 'R2D0052' where iSpecialtyId = 52
update tSpecialty set vchSpecialtyCode = 'R2D0054' where iSpecialtyId = 54
update tSpecialty set vchSpecialtyCode = 'R2D0055' where iSpecialtyId = 55
update tSpecialty set vchSpecialtyCode = 'R2D0057' where iSpecialtyId = 57
update tSpecialty set vchSpecialtyCode = 'R2D0056' where iSpecialtyId = 56
update tSpecialty set vchSpecialtyCode = 'R2D0107' where iSpecialtyId = 107
update tSpecialty set vchSpecialtyCode = 'R2D0106' where iSpecialtyId = 106
update tSpecialty set vchSpecialtyCode = 'R2D0108' where iSpecialtyId = 108
update tSpecialty set vchSpecialtyCode = 'R2D0136' where iSpecialtyId = 136
update tSpecialty set vchSpecialtyCode = 'R2D0177' where iSpecialtyId = 177
update tSpecialty set vchSpecialtyCode = 'R2D0104' where iSpecialtyId = 104
update tSpecialty set vchSpecialtyCode = 'R2D0105' where iSpecialtyId = 105
update tSpecialty set vchSpecialtyCode = 'R2D0059' where iSpecialtyId = 59
update tSpecialty set vchSpecialtyCode = 'R2D0060' where iSpecialtyId = 60
update tSpecialty set vchSpecialtyCode = 'R2D0178' where iSpecialtyId = 178
update tSpecialty set vchSpecialtyCode = 'R2D0141' where iSpecialtyId = 141
update tSpecialty set vchSpecialtyCode = 'R2D0180' where iSpecialtyId = 180
update tSpecialty set vchSpecialtyCode = 'R2D0062' where iSpecialtyId = 62
update tSpecialty set vchSpecialtyCode = 'R2D0064' where iSpecialtyId = 64
update tSpecialty set vchSpecialtyCode = 'R2D0063' where iSpecialtyId = 63
update tSpecialty set vchSpecialtyCode = 'R2D0065' where iSpecialtyId = 65
update tSpecialty set vchSpecialtyCode = 'R2D0066' where iSpecialtyId = 66
update tSpecialty set vchSpecialtyCode = 'R2D0069' where iSpecialtyId = 69
update tSpecialty set vchSpecialtyCode = 'R2D0144' where iSpecialtyId = 144
update tSpecialty set vchSpecialtyCode = 'R2D0070' where iSpecialtyId = 70
update tSpecialty set vchSpecialtyCode = 'R2D0131' where iSpecialtyId = 131
update tSpecialty set vchSpecialtyCode = 'R2D0071' where iSpecialtyId = 71
update tSpecialty set vchSpecialtyCode = 'R2D0194' where iSpecialtyId = 194
update tSpecialty set vchSpecialtyCode = 'R2D0192' where iSpecialtyId = 192
update tSpecialty set vchSpecialtyCode = 'R2D0072' where iSpecialtyId = 72
update tSpecialty set vchSpecialtyCode = 'R2D0183' where iSpecialtyId = 183
update tSpecialty set vchSpecialtyCode = 'R2D0191' where iSpecialtyId = 191
update tSpecialty set vchSpecialtyCode = 'R2D0073' where iSpecialtyId = 73
update tSpecialty set vchSpecialtyCode = 'R2D0147' where iSpecialtyId = 147
update tSpecialty set vchSpecialtyCode = 'R2D0074' where iSpecialtyId = 74
update tSpecialty set vchSpecialtyCode = 'R2D0099' where iSpecialtyId = 99
update tSpecialty set vchSpecialtyCode = 'R2D0137' where iSpecialtyId = 137
update tSpecialty set vchSpecialtyCode = 'R2D0076' where iSpecialtyId = 76
update tSpecialty set vchSpecialtyCode = 'R2D0077' where iSpecialtyId = 77
update tSpecialty set vchSpecialtyCode = 'R2D0078' where iSpecialtyId = 78
update tSpecialty set vchSpecialtyCode = 'R2D0100' where iSpecialtyId = 100
update tSpecialty set vchSpecialtyCode = 'R2D0184' where iSpecialtyId = 184
update tSpecialty set vchSpecialtyCode = 'R2D0080' where iSpecialtyId = 80
update tSpecialty set vchSpecialtyCode = 'R2D0081' where iSpecialtyId = 81
update tSpecialty set vchSpecialtyCode = 'R2D0083' where iSpecialtyId = 83
update tSpecialty set vchSpecialtyCode = 'R2D0082' where iSpecialtyId = 82
update tSpecialty set vchSpecialtyCode = 'R2D0084' where iSpecialtyId = 84
update tSpecialty set vchSpecialtyCode = 'R2D0149' where iSpecialtyId = 149
update tSpecialty set vchSpecialtyCode = 'R2D0101' where iSpecialtyId = 101
update tSpecialty set vchSpecialtyCode = 'R2D0085' where iSpecialtyId = 85
update tSpecialty set vchSpecialtyCode = 'R2D0086' where iSpecialtyId = 86
update tSpecialty set vchSpecialtyCode = 'R2D0186' where iSpecialtyId = 186
update tSpecialty set vchSpecialtyCode = 'R2D0185' where iSpecialtyId = 185
update tSpecialty set vchSpecialtyCode = 'R2D0087' where iSpecialtyId = 87
update tSpecialty set vchSpecialtyCode = 'R2D0187' where iSpecialtyId = 187
update tSpecialty set vchSpecialtyCode = 'R2D0088' where iSpecialtyId = 88
update tSpecialty set vchSpecialtyCode = 'R2D0095' where iSpecialtyId = 95
update tSpecialty set vchSpecialtyCode = 'R2D0090' where iSpecialtyId = 90
update tSpecialty set vchSpecialtyCode = 'R2D0091' where iSpecialtyId = 91
update tSpecialty set vchSpecialtyCode = 'R2D0092' where iSpecialtyId = 92
update tSpecialty set vchSpecialtyCode = 'R2D0093' where iSpecialtyId = 93
update tSpecialty set vchSpecialtyCode = 'R2D0094' where iSpecialtyId = 94
update tSpecialty set vchSpecialtyCode = 'R2D0188' where iSpecialtyId = 188
update tSpecialty set vchSpecialtyCode = 'R2D0139' where iSpecialtyId = 139
update tSpecialty set vchSpecialtyCode = 'R2D0096' where iSpecialtyId = 96
update tSpecialty set vchSpecialtyCode = 'R2D0153' where iSpecialtyId = 153
update tSpecialty set vchSpecialtyCode = 'R2D0097' where iSpecialtyId = 97
update tSpecialty set vchSpecialtyCode = 'R2D0098' where iSpecialtyId = 98
update tSpecialty set vchSpecialtyCode = 'R2D0197' where iSpecialtyId = 197

update tSpecialty set vchSpecialtyCode = 'R2D0156' where iSpecialtyId = 156
update tSpecialty set vchSpecialtyCode = 'R2D0015' where iSpecialtyId = 15
update tSpecialty set vchSpecialtyCode = 'R2D0158' where iSpecialtyId = 158
update tSpecialty set vchSpecialtyCode = 'R2D0159' where iSpecialtyId = 159
update tSpecialty set vchSpecialtyCode = 'R2D0160' where iSpecialtyId = 160

update tSpecialty set vchSpecialtyCode = 'R2D0026' where iSpecialtyId = 26
update tSpecialty set vchSpecialtyCode = 'R2D0167' where iSpecialtyId = 167

update tSpecialty set vchSpecialtyCode = 'R2D0033' where iSpecialtyId = 33

update tSpecialty set vchSpecialtyCode = 'R2D0041' where iSpecialtyId = 41
update tSpecialty set vchSpecialtyCode = 'R2D0124' where iSpecialtyId = 124


update tSpecialty set vchSpecialtyCode = 'R2D0172' where iSpecialtyId = 172
update tSpecialty set vchSpecialtyCode = 'R2D0175' where iSpecialtyId = 175
update tSpecialty set vchSpecialtyCode = 'R2D0048' where iSpecialtyId = 48
update tSpecialty set vchSpecialtyCode = 'R2D0049' where iSpecialtyId = 49
update tSpecialty set vchSpecialtyCode = 'R2D0176' where iSpecialtyId = 176


update tSpecialty set vchSpecialtyCode = 'R2D0181' where iSpecialtyId = 181
update tSpecialty set vchSpecialtyCode = 'R2D0067' where iSpecialtyId = 67
update tSpecialty set vchSpecialtyCode = 'R2D0182' where iSpecialtyId = 182
update tSpecialty set vchSpecialtyCode = 'R2D0075' where iSpecialtyId = 75

update tSpecialty set vchSpecialtyCode = 'R2D0089' where iSpecialtyId = 89
update tSpecialty set vchSpecialtyCode = 'R2D0189' where iSpecialtyId = 189
update tSpecialty set vchSpecialtyCode = 'R2D0133' where iSpecialtyId = 133

update tSpecialty set vchSpecialtyCode = 'R2D0193' where iSpecialtyId = 193


select * from tSpecialty where vchSpecialtyCode is null

---- 6/5/2012
--select * from tResourcePracticeArea where iResourceId = 2565
--delete from tResourcePracticeArea  where iResourceId = 2565 and iPracticeAreaId = 2



