--drop table tPdaPromotion;

CREATE TABLE [dbo].[tPdaPromotion](
	[iPdaPromotionId] [int] IDENTITY(1,1) NOT NULL,
	[vchPdaPromotionName] [varchar](100) NOT NULL,
	[vchPdaPromotionDiscription] [varchar](255) NOT NULL,
	[iDiscountPercentage] [int] NOT NULL,
	[dtStartDate] [smalldatetime] NOT NULL,
	[dtEndDate] [smalldatetime] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tPdaPromotion] PRIMARY KEY CLUSTERED 
(
	[iPdaPromotionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


