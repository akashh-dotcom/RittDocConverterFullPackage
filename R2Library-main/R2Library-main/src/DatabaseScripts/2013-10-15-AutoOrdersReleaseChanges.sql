--------------------------------------------------------------------------------
-- add order number to cart table
--------------------------------------------------------------------------------
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[tCart] ADD [vchOrderNumber] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[tCart] ADD [vchPromotionCode] varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
ALTER TABLE [dbo].[tCart] ADD [decPromotionDiscount] decimal(12, 2) NOT NULL DEFAULT 0.00
GO

--select [name] AS [TableName], [create_date] AS [CreatedDate] FROM sys.tables
--order by [create_date] desc





CREATE TABLE [dbo].[tPromotion] (
[iPromotionId] int IDENTITY(1, 1) NOT NULL,
[vchPromotionCode] varchar(20) NOT NULL,
[vchPromotionName] varchar(100) NOT NULL,
[vchPromotionDiscription] varchar(255) NOT NULL,
[iDiscountPercentage] int NOT NULL,
[dtStartDate] smalldatetime NOT NULL,
[dtEndDate] smalldatetime NOT NULL,
[vchOrderSource] varchar(10) NULL,
[vchCreatorId] varchar(50) NOT NULL,
[dtCreationDate] smalldatetime NOT NULL,
[vchUpdaterId] varchar(50) NULL,
[dtLastUpdate] smalldatetime NULL,
[tiRecordStatus] tinyint NOT NULL,
CONSTRAINT [PK_tPromotion]
PRIMARY KEY CLUSTERED ([iPromotionId] ASC)
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
ALTER TABLE [dbo].[tPromotion] SET (LOCK_ESCALATION = TABLE);
GO


--select * from tInstitutionPromotion












--select * from tPromotion
--
--
--select * from tCart where iInstitutionId = 308 order by dtLastUpdate desc
--
--select * from tProduct
--
--ALTER TABLE [dbo].[tProduct] ADD [decPromotionDiscount] decimal(12, 2) NOT NULL DEFAULT 0.00
--GO


