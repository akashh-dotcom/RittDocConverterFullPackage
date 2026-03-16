USE [RIT001_2012-03-21]
GO

/****** Object:  Table [dbo].[tCart]    Script Date: 6/19/2012 11:51:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tCart](
	[iCartId] [int] IDENTITY(1,1) NOT NULL,
	[iInstitutionId] [int] NOT NULL,
	[vchPurchaseOrderNumber] [varchar](50) NULL,
	[vchPurchaseOrderComment] [varchar](250) NULL,
	[tiProcessed] [tinyint] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tCart] PRIMARY KEY CLUSTERED 
(
	[iCartId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tCart]  WITH CHECK ADD  CONSTRAINT [FK_tCart_tInstitution] FOREIGN KEY([iInstitutionId])
REFERENCES [dbo].[tInstitution] ([iInstitutionId])
GO

ALTER TABLE [dbo].[tCart] CHECK CONSTRAINT [FK_tCart_tInstitution]
GO


alter table dbo.tCart
add dtPurchaseDate datetime null



/****** Object:  Table [dbo].[tCartItem]    Script Date: 6/19/2012 11:52:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tCartItem](
	[iCartItemId] [int] IDENTITY(1,1) NOT NULL,
	[iCartId] [int] NOT NULL,
	[iResourceId] [int] NOT NULL,
	[iNumberOfLicenses] [int] NOT NULL,
	[decPricePerLicense] [decimal](12, 2) NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tCartItem] PRIMARY KEY CLUSTERED 
(
	[iCartItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[tCartItem]  WITH CHECK ADD  CONSTRAINT [FK_tCartItem_tCart] FOREIGN KEY([iCartId])
REFERENCES [dbo].[tCart] ([iCartId])
GO

ALTER TABLE [dbo].[tCartItem] CHECK CONSTRAINT [FK_tCartItem_tCart]
GO



alter table dbo.tCartITem
add decListPrice decimal(12,2) null,
	decDiscountPrice decimal(12,2) null

	
	
	
	
	
	
CREATE TABLE [dbo].[tProduct](
	[iProductId] [int] IDENTITY(1,1) NOT NULL,
	[vchProductName] [varchar](250) NOT NULL,
	[decPrice] [decimal](12, 2) NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tProduct] PRIMARY KEY CLUSTERED 
(
	[iProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
	





alter table tcartitem
alter column iResourceId int null




/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tResource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tCartItem ADD CONSTRAINT
	FK_tCartItem_tResource FOREIGN KEY
	(
	iResourceId
	) REFERENCES dbo.tResource
	(
	iResourceId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.tCartItem ADD CONSTRAINT
	FK_tCartItem_tProduct FOREIGN KEY
	(
	iProductId
	) REFERENCES dbo.tProduct
	(
	iProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.tCartItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT





alter table tcart
add  decInstDiscount [decimal](12, 2)