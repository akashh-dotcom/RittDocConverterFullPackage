
--drop table [tOrderHistoryItem]
--go
--drop table [tOrderHistory]
--go
--drop table [tDiscountType]
--go
CREATE TABLE [dbo].[tDiscountType](
	[iDiscountTypeId] [int]  NOT NULL,
	[vchDiscountTypeName] varchar(50) not null,
	[vchDiscountTypeDescription] varchar(250) not null,
CONSTRAINT [PK_tDiscountType_iDiscountTypeId] PRIMARY KEY NONCLUSTERED 
(
	iDiscountTypeId ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 85) ON [PRIMARY]
) ON [PRIMARY]
go
Insert Into [tDiscountType]
values(1, 'Institution', 'Institution Discount is applied to the Order');
Insert Into [tDiscountType]
values(2, 'Promotion', 'Promotion Discount is applied to the Order');
Insert Into [tDiscountType]
values(3, 'PDA Promotion', 'PDA Promotion Discount is applied to this Order Item');
Insert Into [tDiscountType]
values(4, 'Special', 'Special Discount is applied to this Order Item');
Insert Into [tDiscountType]
values(5, 'Reseller', 'Reseller Discount is applied to the Order');

CREATE TABLE [dbo].[tOrderHistory](
	[iOrderHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[iCartId] [int] NOT NULL,
	[iInstitutionId] [int] NOT NULL,
	[vchOrderNumber] [varchar](20) NULL,
	[vchPurchaseOrderNumber] [varchar](50) NULL,
	[vchPurchaseOrderComment] [varchar](250) NULL,
	[decDiscount] [decimal](12, 2) NULL ,
	[vchPromotionCode] [varchar](20) NULL,
	[vchPromotionDescription] [varchar](255) NULL,
	[iPromotionId] [int] null,
	[dtPurchaseDate] [datetime] NOT NULL,
	[tiBillingMethod] [tinyint] NULL,
	[tiForthcomingTitlesInvoicingMethod] [tinyint] NULL,
	[vchCartName] [varchar](100) NULL,
	[iResellerId] [int] NULL,
	[vchResellerName] [varchar](100) NULL,
	[decResellerDiscount] [decimal](12, 2) NULL,
	[vchOrderFile] [varchar](max) NULL,
	[iDiscountTypeId] int null,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,

 CONSTRAINT [PK_tOrderHistory] PRIMARY KEY CLUSTERED 
(
	[iOrderHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
go
ALTER TABLE [dbo].[tOrderHistory] ADD CONSTRAINT [FK_tOrderHistory_tCart] FOREIGN KEY([iCartId])
REFERENCES [dbo].[tCart] ([iCartId])
go
ALTER TABLE [dbo].[tOrderHistory] ADD CONSTRAINT [FK_tOrderHistory_tInstitution] FOREIGN KEY([iInstitutionId])
REFERENCES [dbo].[tInstitution] ([iInstitutionId])
go
ALTER TABLE [dbo].[tOrderHistory] ADD CONSTRAINT [FK_tOrderHistory_tPromotion] FOREIGN KEY([iPromotionId])
REFERENCES [dbo].[tPromotion] ([iPromotionId])
go
ALTER TABLE [dbo].[tOrderHistory] ADD CONSTRAINT [FK_tOrderHistory_tCartReseller] FOREIGN KEY([iResellerId])
REFERENCES [dbo].[tCartReseller] ([iResellerId])
go
ALTER TABLE [dbo].[tOrderHistory] ADD CONSTRAINT [FK_tOrderHistory_tDiscountType] FOREIGN KEY([iDiscountTypeId])
REFERENCES [dbo].[tDiscountType] ([iDiscountTypeId])


CREATE TABLE [dbo].[tOrderHistoryItem](
	[iOrderHistoryItemId] [int] IDENTITY(1,1) NOT NULL,
	[iOrderHistoryId] [int] NULL,
	[iResourceId] [int] NULL,
	[iProductId] [int] NULL,
	[iInstitutionResourceLicenseId] [int] NULL,
	[iNumberOfLicenses] [int] NOT NULL,
	[decListPrice] [decimal](12, 2) NULL,
	[decDiscountPrice] [decimal](12, 2) NULL,
	[decDiscount] [decimal](12, 2) NULL,
	[vchSpecialText] [varchar](255) NULL,
	[vchSpecialIconName] [varchar](255) NULL,
	[iSpecialDiscountId] [int] null,
	[iPdaPromotionId] [int] null,
	[iDiscountTypeId] int null,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [PK_tOrderHistoryItem] PRIMARY KEY CLUSTERED 
(
	[iOrderHistoryItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tOrderHistory] FOREIGN KEY([iOrderHistoryId])
REFERENCES [dbo].[tOrderHistory] ([iOrderHistoryId])
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tResource] FOREIGN KEY([iResourceId])
REFERENCES [dbo].[tResource] ([iResourceId])
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tProduct] FOREIGN KEY([iProductId])
REFERENCES [dbo].[tProduct] ([iProductId])
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tInstitutionResourceLicense] FOREIGN KEY([iInstitutionResourceLicenseId])
REFERENCES [dbo].[tInstitutionResourceLicense] ([iInstitutionResourceLicenseId])
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tSpecialDiscount] FOREIGN KEY([iSpecialDiscountId])
REFERENCES [dbo].[tSpecialDiscount] ([iSpecialDiscountId])
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tPdaPromotion] FOREIGN KEY([iPdaPromotionId])
REFERENCES [dbo].[tPdaPromotion] ([iPdaPromotionId])
go
ALTER TABLE [dbo].[tOrderHistoryItem] ADD CONSTRAINT [FK_tOrderHistoryItem_tDiscountType] FOREIGN KEY([iDiscountTypeId])
REFERENCES [dbo].[tDiscountType] ([iDiscountTypeId])
go

INSERT INTO tOrderHistory
           (iCartId,iInstitutionId,vchOrderNumber,vchPurchaseOrderNumber,vchPurchaseOrderComment,decDiscount,vchPromotionCode,vchPromotionDescription
           ,iPromotionId,dtPurchaseDate,tiBillingMethod,tiForthcomingTitlesInvoicingMethod
           ,vchCartName,iResellerId,vchResellerName,decResellerDiscount,vchCreatorId,dtCreationDate,tiRecordStatus,iDiscountTypeId)
select c.iCartId, c.iInstitutionId, c.vchOrderNumber, ISNULL(c.vchPurchaseOrderNumber, po.vchPoNumber) as vchPurchaseOrderNumber, ISNULL(c.vchPurchaseOrderComment, po.vchPoComment) as vchPurchaseOrderComment
, (case 
	when c.iResellerId is null then 
		case 
			when ISNULL(c.decPromotionDiscount, promo.iDiscountPercentage) is null or ISNULL(c.decPromotionDiscount, promo.iDiscountPercentage) = 0 
				then c.decInstDiscount 
			else ISNULL(c.decPromotionDiscount, promo.iDiscountPercentage) 
		end
	else 0 
 end) as decDiscount
, ISNULL(c.vchPromotionCode, promo.vchPromotionCode) as vchPromotionCode, promo.vchPromotionDiscription, promo.iPromotionId
, c.dtPurchaseDate, ISNULL(c.tiBillingMethod, null), ISNULL(c.tiForthcomingTitlesInvoicingMethod, null), c.vchCartName, c.iResellerId, cr.vchResellerDisplayName
, cr.decDiscount, ISNULL(c.vchUpdaterId,c.vchCreatorId) as vchCreatorId, ISNULL(c.dtLastUpdate, c.dtCreationDate) as dtCreationDate, c.tiRecordStatus
, (case 
	when c.iResellerId is null then 
		case 
			when ISNULL(c.decPromotionDiscount, promo.iDiscountPercentage) is null or ISNULL(c.decPromotionDiscount, promo.iDiscountPercentage) = 0 
				then 1
			else 2
		end
	else 5 
 end) as iDiscountTypeId
from tCart c
join tInstitution i on c.iInstitutionId = i.iInstitutionId
left join tPoComment po on c.iPoCommentId = po.iPoCommentId
left join tPromotion promo on c.vchPromotionCode = promo.vchPromotionCode
left join tCartReseller cr on c.iResellerId = cr.iResellerId
where c.tiProcessed = 1 and c.tiRecordStatus = 1

go

INSERT INTO tOrderHistoryItem
           (iOrderHistoryId,iResourceId,iProductId,iInstitutionResourceLicenseId,iNumberOfLicenses,decListPrice,decDiscountPrice
           ,decDiscount,vchSpecialText,vchSpecialIconName,iSpecialDiscountId,iPdaPromotionId,vchCreatorId,dtCreationDate,tiRecordStatus,iDiscountTypeId)
select oh.iOrderHistoryId, ci.iResourceId, ci.iProductId, irl.iInstitutionResourceLicenseId, ci.iNumberOfLicenses, ci.decListPrice
, ci.decDiscountPrice
, (select max(v) from (values (oh.decDiscount), (pda.iDiscountPercentage), (sd.iDiscountPercentage))as value(v)) as discountpercentage
, ci.vchSpecialText, ci.vchSpecialIconName, sd.iSpecialDiscountId, pda.iPdaPromotionId, ISNULL(ci.vchUpdaterId,ci.vchCreatorId), ISNULL(ci.dtLastUpdate, ci.dtCreationDate)
, 1
, (case 
	when ci.vchSpecialText is not null and ci.tiLicenseOriginalSourceId = 2 and pda.iPdaPromotionId is not null then 3
	when ci.vchSpecialText is not null and sd.iSpecialDiscountId is not null then 4
 end) as iDiscountTypeId
from tOrderHistory oh
join tCartItem ci on oh.iCartId = ci.iCartId and ci.tiRecordStatus = 1
left join tInstitutionResourceLicense irl on ci.iResourceId = irl.iResourceId and oh.iInstitutionId = irl.iInstitutionId
left join tPdaPromotion pda on ci.dtPurchaseDate between pda.dtStartDate and pda.dtEndDate and ci.vchSpecialIconName is null and ci.tiLicenseOriginalSourceId = 2

left join tSpecialResource sr on sr.iResourceId = ci.iResourceId and sr.tiRecordStatus = 1 and ci.vchSpecialIconName is not null
left join tSpecialDiscount sd on sr.iSpecialDiscountId = sd.iSpecialDiscountId
left join tSpecial sp on sd.iSpecialId = sp.iSpecialId and ci.dtPurchaseDate between sp.dtStartDate and sp.dtEndDate

where ci.dtPurchaseDate is not null
and ((ci.iNumberOfLicenses > 0  and ci.iResourceId > 0) or (ci.iProductId is not null and ci.tiInclude = 1))