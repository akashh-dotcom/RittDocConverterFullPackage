CREATE TABLE [dbo].[tPromotionProduct](
	[iPromotionProductId] [int] IDENTITY(1,1) NOT NULL,
	[iPromotionId] [int] NULL,
	[iProductId] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL
) ON [PRIMARY];

ALTER TABLE [dbo].[tPromotionProduct]  WITH CHECK ADD FOREIGN KEY([iProductId])
REFERENCES [dbo].[tProduct] ([iProductId]);

ALTER TABLE [dbo].[tPromotionProduct]  WITH CHECK ADD FOREIGN KEY([iPromotionId])
REFERENCES [dbo].[tPromotion] ([iPromotionId]);


