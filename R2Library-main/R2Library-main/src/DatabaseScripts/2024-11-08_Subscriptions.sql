ALTER TABLE [tUser]
ALTER COLUMN [iInstitutionId] int null;
go

CREATE TABLE tSubscription (
    iSubscriptionId INT PRIMARY KEY IDENTITY(1,1),
    vchName VARCHAR(50) NOT NULL,
	vchDescription VARCHAR(500) NULL, --Plain text description if HTML populated this will be overridden
	vchCmsName VARCHAR(50) NULL,	  --HTML description
    decMonthlyPrice decimal(12,2) NOT NULL,
	decAnnualPrice decimal(12,2) NOT NULL,
	tiAllowSubscriptions tinyint NOT NULL,	--Can turn off new Subscriptions also hide from displaying unless purchased
	vchCreatorId varchar(50) NOT NULL,
	dtCreationDate smalldatetime NOT NULL,
	vchUpdaterId varchar(50) NULL,
	dtLastUpdate smalldatetime NULL,
	tiRecordStatus tinyint NOT NULL,
);
go

--sp_rename 'tSubscription.decListPrice', 'decMonthlyPrice', 'COLUMN';
--sp_rename 'tSubscription.decDiscountPrice', 'decAnnualPrice', 'COLUMN';

CREATE TABLE tSubscriptionResource (
	iSubscriptionResourceId INT PRIMARY KEY IDENTITY(1,1),
	iSubscriptionId int NOT NULL,
	iResourceId int NOT NULL,
	vchCreatorId varchar(50) NOT NULL,
	dtCreationDate smalldatetime NOT NULL,
	vchUpdaterId varchar(50) NULL,
	dtLastUpdate smalldatetime NULL,
	tiRecordStatus tinyint NOT NULL,
	FOREIGN KEY (iResourceId) REFERENCES tResource(iResourceId),
	FOREIGN KEY (iSubscriptionId) REFERENCES tSubscription(iSubscriptionId)
);
go

CREATE TABLE tSubscriptionUser (
	iSubscriptionUserId INT PRIMARY KEY IDENTITY(1,1),
	iUserId int NOT NULL,
	iSubscriptionId int NOT NULL,
	dtExpirationDate smalldatetime NULL,
	dtTrialExpirationDate smalldatetime NULL,
	dtActivationDate smalldatetime NULL,
	--tiMonthlySubscription tinyint default(0),
	iSubscriptionType int NOT NULL default(1),
	vchCreatorId varchar(50) NOT NULL,
	dtCreationDate smalldatetime NOT NULL,
	vchUpdaterId varchar(50) NULL,
	dtLastUpdate smalldatetime NULL,
	tiRecordStatus tinyint NOT NULL,
	FOREIGN KEY (iUserId) REFERENCES tUser(iUserId),
	FOREIGN KEY (iSubscriptionId) REFERENCES tSubscription(iSubscriptionId)
);
go

--Alter Table tSubscriptionUser
--add dtTrialExpirationDate smalldatetime NULL
--Alter Table tSubscriptionUser
--add dtActivationDate smalldatetime NULL
--Alter Table tSubscriptionUser
--add iSubscriptionType int NOT NULL default(1)
--drop table tSubscriptionOrderHistory;
CREATE TABLE tSubscriptionOrderHistory (
	iSubscriptionOrderHistoryId INT PRIMARY KEY IDENTITY(1,1),
	iSubscriptionId int NOT NULL,
	iUserId int NOT NULL,
	iSubscriptionUserId int NULL,
	vchOrderNumber varchar(20) NULL,
	vchPurchaseOrderNumber varchar(50) NULL,
	vchPurchaseOrderComment varchar(250) NULL,
	vchMembershipNumber varchar(50) NULL,
	iSubscriptionType int NOT NULL,
	decPrice decimal(12,2) NOT NULL,
	vchCreatorId varchar(50) NOT NULL,
	dtCreationDate smalldatetime NOT NULL,
	vchUpdaterId varchar(50) NULL,
	dtLastUpdate smalldatetime NULL,
	tiRecordStatus tinyint NOT NULL,
	FOREIGN KEY (iUserId) REFERENCES tUser(iUserId),
	FOREIGN KEY (iSubscriptionId) REFERENCES tSubscription(iSubscriptionId),
	FOREIGN KEY (iSubscriptionUserId) REFERENCES tSubscriptionUser(iSubscriptionUserId)
);
go

--Alter Table tSubscriptionOrderHistory
--add vchMembershipNumber varchar(50) NULL

INSERT INTO [dbo].[tRole]
           ([vchRoleCode]
           ,[vchRoleDesc]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
     VALUES
           ('SUBUSER'
           , 'Subscription User'
           , 'kshaberle'
           ,getdate()
           , 1)
GO

INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES('dev','CollectionManagement','SubscriptionTrialDays','5','int value - Number of Says Subscriptions will be able to trial');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES('stage','CollectionManagement','SubscriptionTrialDays','5','int value - Number of Says Subscriptions will be able to trial');
INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES('prod','CollectionManagement','SubscriptionTrialDays','5','int value - Number of Says Subscriptions will be able to trial');


--drop table tSubscription
--drop table tSubscriptionResource
--drop table tSubscriptionUser
--drop table tSubscriptionOrderHistory





--iResourceId = 6517



INSERT INTO [dbo].[tSubscription]
           ([vchName]
           ,[vchDescription]
           ,[decListPrice]
           ,[decDiscountPrice]
           ,[tiAllowSubscriptions]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
     VALUES
           (
			'Test Subscription'
           , 'This is a testing subscription'
           , 10.00
           ,0
           ,1
           ,'kshaberle'
           ,getdate()
           ,1)
GO


INSERT INTO [dbo].[tSubscriptionResource]
           ([iSubscriptionId]
           ,[iResourceId]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
     VALUES
           (1
           ,6517
           ,'kshaberle'
           ,getdate()
           ,1)
GO

--UserId:	10019		
--UserName: TechnoKen

USE [DEV_RIT001]
GO

INSERT INTO [dbo].[tSubscriptionUser]
           ([iUserId]
           ,[iSubscriptionId]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
     VALUES
           (10019
           ,1
           ,'kshaberle'
           ,getdate()
           ,1)
GO


