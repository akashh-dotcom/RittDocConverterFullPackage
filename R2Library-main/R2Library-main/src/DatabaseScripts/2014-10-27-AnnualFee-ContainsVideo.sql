

alter table tUser
add tiReceiveAnnualMaintenanceFee tinyint not null default((0));

alter table tResource
add tiContainsVideo tinyint not null default((0));


use [R2Utilities]

Create Table EmailType(
	emailTypeId int NOT NULL,
	name varchar(100) NULL,
	description varchar(500) NULL,
	PRIMARY KEY (emailTypeId)
);

Create Table EmailResult(
	emailResultId int IDENTITY(1,1) NOT NULL,
	institutionId int NOT NULL,
	userId int NULL,
	dateEmailSent smalldatetime NOT NULL,
	emailTypeId int not null,
	description varchar(2000) NULL
)
ALTER TABLE [dbo].[EmailResult]  WITH CHECK ADD  CONSTRAINT [FK_EmailResult_EmailType] FOREIGN KEY([emailTypeId])
REFERENCES [dbo].[EmailType] ([emailTypeId]);

ALTER TABLE [dbo].[EmailResult] CHECK CONSTRAINT [FK_EmailResult_EmailType];

Insert Into EmailType
 values(1, 'Annual Maintenance Fee', 'Email Report to RAs and SAs to alert them of about the that days institutions Annual Fee Renewal')