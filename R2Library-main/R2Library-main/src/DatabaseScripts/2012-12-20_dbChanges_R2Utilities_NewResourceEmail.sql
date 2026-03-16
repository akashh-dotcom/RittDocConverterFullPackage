USE [R2Utilities]
GO

/****** Object:  Table [dbo].[ResourceEmails]    Script Date: 12/18/2012 12:02:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ResourceEmails](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[resourceISBN] [varchar](25) NOT NULL,
	[dateNewEditionEmail] [smalldatetime] NULL,
	[dateNewResourceEmail] [smalldatetime] NULL,
	[datePurchasedEmail] [smalldatetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY],
 CONSTRAINT [UK_resourceISBN] UNIQUE NONCLUSTERED 
(
	[resourceISBN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO;



insert into ResourceEmails (resourceISBN, dateNewEditionEmail, dateNewResourceEmail, datePurchasedEmail)

	select r.vchResourceISBN, nr.dtNewEditionEmail, nr.dtNewResourceEmail, nr.dtPurchasedEmail
	from [RIT001].[dbo].[tResource] r
	join [RIT001].[dbo].[tNewResourceQue] nr on r.iResourceId = nr.iResourceId
	where 
			r.tiRecordStatus = 1 and r.iResourceStatusId = 6 
			and r.dtQaApprovalDate is not NULL and r.dtLastPromotionDate is not null
			--and nr.dtNewEditionEmail is null and nr.dtNewResourceEmail is null and nr.dtPurchasedEmail is null 
			and nr.dtNewEditionEmail is not null and nr.dtNewResourceEmail is not null and nr.dtPurchasedEmail is not null 
	order by r.iResourceId asc