DROP VIEW [dbo].[vPreludeCustomer]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vPreludeCustomer]
AS

select c.accountNumber as vchAccountNumber, c.accountName as vchAccountName, 
       ca.addressLine1 as vchBillToAddress1, ca.city as vchBillToCity, 
       ca.[state] as vchBillToState, ca.zipCode as vchBillToZip, ca.country as vchBillToCountry, 
       ISNULL(u.preludeEmailAddress, u.webEmailAddress) as vchEmailAddress
	   , ca.phone as vchBillToPhone, ca.fax as vchBillToFax, 
	   c.isR2Customer as tiIsR2Customer
	   , ca.addressLine2 as vchBillToAddress2, ca.addressLine3 as vchBillToAddress3,
	   pc.territory as vchTerritory, ct.r2Type as vchTypeName
from RittenhouseWeb..Customer c
join RittenhouseWeb..UserCustomer uc on c.customerId = uc.customerId and uc.isCustomerPrimaryUser = 1
join RittenhouseWeb..[User] u on uc.userId = u.userId
left join PreludeData..Customer pc on c.accountNumber = pc.accountNumber
left join RittenhouseWeb..CustomerAddress ca on c.customerId = ca.customerId and ca.addressTypeId = 1
left join RittenhouseWeb..CustomerType ct on c.customerTypeId = ct.customerTypeId
where c.dateDeleted is null 
and c.accountNumber not like 'd%'
and c.accountNumber not like 'p%'

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tInstitutionType](
	[iInstitutionTypeId] [int] IDENTITY(1,1) NOT NULL,
	[vchInstitutionTypeCode] [varchar](20) NOT NULL,
	[vchInstitutionTypeName] [varchar](100) NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [tInstitutionType_PK] PRIMARY KEY CLUSTERED 
(
	[iInstitutionTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

Insert into tInstitutionType(vchInstitutionTypeCode, vchInstitutionTypeName, vchCreatorId, dtCreationDate, tiRecordStatus)
select '2YL', '2-Year Library', 'Initial Load', getdate(), 1;
Insert into tInstitutionType(vchInstitutionTypeCode, vchInstitutionTypeName, vchCreatorId, dtCreationDate, tiRecordStatus)
select '4YL', '4-Year Library', 'Initial Load', getdate(), 1;
Insert into tInstitutionType(vchInstitutionTypeCode, vchInstitutionTypeName, vchCreatorId, dtCreationDate, tiRecordStatus)
select 'B2B', 'Business to Business', 'Initial Load', getdate(), 1;
Insert into tInstitutionType(vchInstitutionTypeCode, vchInstitutionTypeName, vchCreatorId, dtCreationDate, tiRecordStatus)
select 'H', 'Hospital', 'Initial Load', getdate(), 1;
Insert into tInstitutionType(vchInstitutionTypeCode, vchInstitutionTypeName, vchCreatorId, dtCreationDate, tiRecordStatus)
select 'ML', 'Medical Library', 'Initial Load', getdate(), 1;
Insert into tInstitutionType(vchInstitutionTypeCode, vchInstitutionTypeName, vchCreatorId, dtCreationDate, tiRecordStatus)
select 'SL', 'Special Library', 'Initial Load', getdate(), 1;

go

alter table tInstitution
add iInstitutionTypeId int null;

go
Update i
set iInstitutionTypeId = it.iInstitutionTypeId
from tInstitution i
join vPreludeCustomer pc on i.vchInstitutionAcctNum = pc.vchAccountNumber
join tInstitutionType it on pc.vchTypeName = it.vchInstitutionTypeName


go
CREATE TABLE [dbo].[tAutomatedCart](
	[iAutomatedCartId] 		[int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[iPeriod]				[int]  NOT NULL,
	[dtStartDate] 			[smalldatetime] NOT NULL,
	[dtEndDate] 			[smalldatetime] NOT NULL,
	[iTerritoryId] 			[int] NOT NULL,
	[iInstitutionTypeId] 	[int] NOT NULL,
	[tiNewEdition] 			[tinyint] NOT NULL,
	[tiPda] 				[tinyint] NOT NULL,
	[tiReviewed] 			[tinyint] NOT NULL,
	[tiTurnaway] 			[tinyint] NOT NULL,
	[decDiscount] 			[decimal](10, 2) NOT NULL,
	[vchAccountNumbers] 	[varchar](max) NULL,
	[vchEmailText] 			[varchar](max) NOT NULL,
	[vchCreatorId] 			[varchar](50) NOT NULL,
	[dtCreationDate] 		[smalldatetime] NOT NULL,
	[vchUpdaterId] 			[varchar](50) NULL,
	[dtLastUpdate] 			[smalldatetime] NULL,
	[tiRecordStatus] 		[tinyint] NOT NULL
);
go

CREATE TABLE [dbo].[tAutomatedCartInstitution](
	[iAutomatedCartInstitutionId] 	[int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[iAutomatedCartId] 				[int] NOT NULL,
	[iInstitutionId] 				[int] NOT NULL,
	[iCartId] 						[int] NULL,
	[vchCreatorId] 					[varchar](50) NOT NULL,
	[dtCreationDate] 				[smalldatetime] NOT NULL,
	[vchUpdaterId] 					[varchar](50) NULL,
	[dtLastUpdate] 					[smalldatetime] NULL,
	[tiRecordStatus] 				[tinyint] NOT NULL,
	FOREIGN KEY (iAutomatedCartId) REFERENCES tAutomatedCart(iAutomatedCartId),
	FOREIGN KEY (iInstitutionId) REFERENCES tInstitution(iInstitutionId)
);
go

CREATE TABLE [dbo].[tAutomatedCartResource](
	[iAutomatedCartResourceId] 		[int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[iAutomatedCartInstitutionId] 	[int] NOT NULL,
	[iResourceId] 					[int] NOT NULL,
	[iCartItemId] 					[int] NOT NULL,
	[decListPrice] 					[decimal](12, 2) NOT NULL,
	[decDiscountPrice] 				[decimal](12, 2) NOT NULL,
	[vchCreatorId] 					[varchar](50) NOT NULL,
	[dtCreationDate] 				[smalldatetime] NOT NULL,
	[vchUpdaterId] 					[varchar](50) NULL,
	[dtLastUpdate] 					[smalldatetime] NULL,
	[tiRecordStatus] 				[tinyint] NOT NULL,
	FOREIGN KEY (iAutomatedCartInstitutionId) REFERENCES tAutomatedCartInstitution(iAutomatedCartInstitutionId),
	FOREIGN KEY (iResourceId) REFERENCES tResource(iResourceId)
);


-- INSERT INTO tConfigurationSetting(vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
-- VALUES('dev', 'Client', 'TrialServiceUrl', 'dev-trunk.rittenhouse.com/Rbd/Web/CustomerDiscovery.asmx/GetRittenhouseCustomer', 'string value - URL to authenticate Rittenhouse accounts for Trial creation')

-- INSERT INTO tConfigurationSetting(vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
-- VALUES('dev', 'MessageQueue', 'AutomatedCartExchangeName', 'E.Dev.AutomatedCart', 'string value - rabbitmq exchange name for AutomatedCart.')
-- GO

-- INSERT INTO tConfigurationSetting(vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
-- VALUES('dev', 'MessageQueue', 'AutomatedCartQueueName', 'Q.Dev.AutomatedCart', 'string value - rabbitmq queue name for AutomatedCart.')
-- GO

-- INSERT INTO tConfigurationSetting(vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
-- VALUES('dev', 'MessageQueue', 'AutomatedCartRouteKey', 'Dev.Local.AutomatedCart', 'string value - rabbitmq route key for AutomatedCart.')
-- GO