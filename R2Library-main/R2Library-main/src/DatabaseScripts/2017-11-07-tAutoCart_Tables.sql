CREATE TABLE [dbo].[tAutomatedCart](
	[iAutomatedCartId] 		[int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[iPeriod]				[int]  NOT NULL,
	[dtStartDate] 			[smalldatetime] NOT NULL,
	[dtEndDate] 			[smalldatetime] NOT NULL,
	[vchInstitutionTypeIds] [varchar](500) NULL,
	[vchTerritoryIds] 		[varchar](500) NULL,
	[tiNewEdition] 			[tinyint] NOT NULL,
	[tiPda] 				[tinyint] NOT NULL,
	[tiReviewed] 			[tinyint] NOT NULL,
	[tiTurnaway] 			[tinyint] NOT NULL,
	[decDiscount] 			[decimal](10, 2) NOT NULL,
	[vchAccountNumbers] 	[varchar](max) NULL,
	[vchEmailSubject] 		[varchar](255) NOT NULL,
	[vchEmailTitle] 		[varchar](255) NOT NULL,
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