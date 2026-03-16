
--drop table tUserOptionValue;
--drop table tUserOptionRole;
--drop table tUserOption;
--drop table tUserOptionType;
--go


--iUserOtptionTypeId int IDENTITY(1,1) PRIMARY KEY,
Create Table tUserOptionType(
	iUserOptionTypeId int PRIMARY KEY,
	vchUserOptionTypeCode varchar(25) not null,
	vchUserOptionTypeDescription varchar (255) not null,
	vchCreatorId varchar(50) not null,
	dtCreationDate smalldatetime not null,
	vchUpdaterId varchar(50) null,
	dtLastUpdate smalldatetime null,
	tiRecordStatus tinyint not null
);

go 

insert into tUserOptionType (iUserOptionTypeId, vchUserOptionTypeCode, vchUserOptionTypeDescription, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(1, 'EMAIL', 'This type is for email options', 'Initial Load', getdate(), 1);
insert into tUserOptionType (iUserOptionTypeId, vchUserOptionTypeCode, vchUserOptionTypeDescription, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(2, 'RITTADMIN', 'This type is for Rittenhouse Administrators', 'Initial Load', getdate(), 1);
insert into tUserOptionType (iUserOptionTypeId, vchUserOptionTypeCode, vchUserOptionTypeDescription, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(3, 'UI', 'This type is for UI specific options', 'Initial Load', getdate(), 1);
insert into tUserOptionType (iUserOptionTypeId, vchUserOptionTypeCode, vchUserOptionTypeDescription, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(4, 'ALERT', 'This type is for alert options', 'Initial Load', getdate(), 1);
insert into tUserOptionType (iUserOptionTypeId, vchUserOptionTypeCode, vchUserOptionTypeDescription, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(5, 'NOTIFICATION', 'This type is for notification options', 'Initial Load', getdate(), 1);

Create Table tUserOption(
	iUserOptionId int PRIMARY KEY,
	vchUserOptionCode varchar(25) not null,
	vchUserOptionDescription varchar (255) not null,
	iUserOptionTypeId int not null,
	vchCreatorId varchar(50) not null,
	dtCreationDate smalldatetime not null,
	vchUpdaterId varchar(50) null,
	dtLastUpdate smalldatetime null,
	tiRecordStatus tinyint not null
	Constraint fk_iUserOptionTypeId_tUserOptionType foreign key (iUserOptionTypeId) references tUserOptionType(iUserOptionTypeId),
)
go 
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(1, 'NewResource', 'New Resource Email', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(2, 'NewEdition', 'New Edition Resource Email based on Resources Owned', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(3, 'CartRemind', 'Cart email sent on 7, 15, and 30 days after no activity', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(4, 'ForthcomingPurchase', 'New Resource Email when purchased forthcoming title', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(5, 'DctMedical', 'New DCT Medical resources released', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(6, 'DctNursing', 'New DCT Nursing resources released', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(7, 'DctAlliedHealth', 'New DCT Allied Health resources released', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(8, 'PdaAddToCart', 'PDA Resource added to cart via trigger', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(9, 'PdaReport', 'PDA History over the past month', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(10, 'ArchivedAlert', 'Purchased resources that have been archived', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(11, 'LibrarianAlert', 'Ask your librarian recipient', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(12, 'ExpertReviewRecommend', 'Resources recommented by Expert Reviews', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(13, 'ExpertReviewUserRequest', 'User requesting the Expert Review Role', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(14, 'AnnualMaintenanceFee', 'List of institutions whose annual maintenance fee is due', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(15, 'Dashboard', 'The dashaboard email sent weekly', 1, 'Initial Load', getdate(), 1);
insert into tUserOption (iUserOptionId, vchUserOptionCode, vchUserOptionDescription, iUserOptionTypeId, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values(16, 'AccessDenied', 'Resource Access Denied emails send daily', 1, 'Initial Load', getdate(), 1);
--tiReceiveNewResourceInfo				-	1	-	NewResource					-	'New Resource Email'
--tiReceiveNewEditionInfo				-	2	-	NewEdition					-	'New Edition Resource Email based on Resources Owned'
--tiReceiveCartRemind					-	3	-	CartRemind					-	'Cart email sent on 7, 15, and 30 days after no activity'
--tiReceiveForthcomingPurchase			-	4	-	ForthcomingPurchase			-	'New Resource Email when purchased forthcoming title'
--tiReceiveDctMedicalUpdate				-	5	-	DctMedical					-	'New DCT Medical resources released'
--tiReceiveDctNursingUpdate				-	6	-	DctNursing					-	'New DCT Nursing resources released'
--tiReceiveDctAlliedHealthUpdate		-	7	-	DctAlliedHealth				-	'New DCT Allied Health resources released'
--tiReceivePdaAddToCart					-	8	-	PdaAddToCart				-	'PDA Resource added to cart via trigger'
--tiReceivePdaReport					-	9	-	PdaReport					-	'PDA History over the past month'
--tiReceiveArchivedAlert				-	10	-	ArchivedAlert				-	'Purchased resources that have been archived'
--tiReceiveLibrarianAlert				-	11	-	LibrarianAlert				-	'Ask your librarian recipient'
--tiReceiveFacultyUserRecommendations	-	12	-	ExpertReviewRecommend		-	'Resources recommented by Expert Reviews'
--tiReceiveFacultyUserRequests			-	13	-	ExpertReviewUserRequest		-	'User requesting the Expert Review Role'
--tiReceiveAnnualMaintenanceFee			-	14	-	AnnualMaintenanceFee		-	'List of institutions whose annual maintenance fee is due'
--tiReceiveDashboardEmail				-	15	-	Dashboard					-	'The dashaboard email sent weekly'
--tiReceiveLockoutInfo					-	16	-	Turnaway					-	'Turnaway '

Create Table tUserOptionRole(
	iUserOptionRoleId int IDENTITY(1,1) PRIMARY KEY,
	iRoleId int not null,
	iUserOptionId int not null,
	vchDefaultValue varchar(255) null,
	vchCreatorId varchar(50) not null,
	dtCreationDate smalldatetime not null,
	vchUpdaterId varchar(50) null,
	dtLastUpdate smalldatetime null,
	tiRecordStatus tinyint not null
	Constraint fk_iRoleId_tRole  foreign key (iRoleId) references tRole(iRoleId),
	Constraint fk_iUserOptionId_tUserOption  foreign key (iUserOptionId) references tUserOption(iUserOptionId)
)
go 
--1	INSTADMIN
--2	RITADMIN
--3	USERS
--6	SALESASSOC
--7	ExpertReviewer
--INSTADMIN
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values (1, 1, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus) 
	values (1, 2, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 3, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 4, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 5, '0', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 6, '0', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 7, '0', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 8, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 9, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 10, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 11, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 12, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 13, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 15, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (1, 16, '1', 'Initial Load', getdate(), 1);
--RITADMIN
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (2, 1, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (2, 2, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (2, 14, '1', 'Initial Load', getdate(), 1);
--USERS
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (3, 1, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (3, 2, '1', 'Initial Load', getdate(), 1);
--SALESASSOC
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (6, 1, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (6, 2, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (6, 14, '1', 'Initial Load', getdate(), 1);
--ExpertReviewer
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (7, 1, '1', 'Initial Load', getdate(), 1);
insert into tUserOptionRole (iRoleId, iUserOptionId, vchDefaultValue, vchCreatorId, dtCreationDate, tiRecordStatus)
	values (7, 2, '1', 'Initial Load', getdate(), 1);

Create Table tUserOptionValue(
	iUserOptionValue int IDENTITY(1,1) PRIMARY KEY,
	iUserId int not null,
	iUserOptionId int not null,
	vchUserOptionValue varchar(255) not null,
	vchCreatorId varchar(50) not null,
	dtCreationDate smalldatetime not null,
	vchUpdaterId varchar(50) null,
	dtLastUpdate smalldatetime null,
	tiRecordStatus tinyint not null
	Constraint fk_iUserId_tUser foreign key (iUserId) references tUser(iUserId),
	Constraint fk_iUserOptionId_tUserOptionValue_tUserOption foreign key (iUserOptionId) references tUserOption(iUserOptionId)
)
go 
insert into tUserOptionValue(iUserId, iUserOptionId, vchUserOptionValue, vchCreatorId, dtCreationDate, tiRecordStatus)
select u.iUserId, uo.iUserOptionId, 
case uo.iUserOptionId
	when 1 then CAST (u.tiReceiveNewResourceInfo AS varchar) 
	when 2 then CAST (u.tiReceiveNewEditionInfo AS varchar) 
	when 3 then CAST (u.tiReceiveCartRemind AS varchar) 
	when 4 then CAST (u.tiReceiveForthcomingPurchase AS varchar) 
	when 5 then CAST (u.tiReceiveDctMedicalUpdate AS varchar) 
	when 6 then CAST (u.tiReceiveDctNursingUpdate AS varchar) 
	when 7 then CAST (u.tiReceiveDctAlliedHealthUpdate AS varchar) 
	when 8 then CAST (u.tiReceivePdaAddToCart AS varchar) 
	when 9 then CAST (u.tiReceivePdaReport AS varchar) 
	when 10 then CAST (u.tiReceiveArchivedAlert AS varchar) 
	when 11 then CAST (u.tiReceiveLibrarianAlert AS varchar) 
	when 12 then CAST (u.tiReceiveFacultyUserRecommendations AS varchar) 
	when 13 then CAST (u.tiReceiveFacultyUserRequests AS varchar) 
	when 14 then CAST (u.tiReceiveAnnualMaintenanceFee AS varchar) 
	when 15 then CAST (u.tiReceiveDashboardEmail AS varchar) 
	when 16 then CAST (u.tiReceiveLockoutInfo AS varchar) 
end as vchUserOptionValue
, 'Initial Load', getdate(), 1
from tUser u
join tUserOptionRole uor on u.iRoleId = uor.iRoleId
join tUserOption uo on uor.iUserOptionId = uo.iUserOptionId
--where u.iRoleId = 1 and u.vchUserName = 'arose'


--select uov.* 
--from tUser u
--join tUserOptionValue uov on u.iUserId = uov.iUserId
--where u.vchUserName = 'kenhaberle'


ALTER TABLE [dbo].[tUser] ADD  DEFAULT ((0)) FOR [tiReceiveLockoutInfo]
ALTER TABLE [dbo].[tUser] ADD  DEFAULT ((0)) FOR [tiReceiveNewResourceInfo]
ALTER TABLE [dbo].[tUser] ADD  DEFAULT ((0)) FOR tiReceiveNewSearchResource