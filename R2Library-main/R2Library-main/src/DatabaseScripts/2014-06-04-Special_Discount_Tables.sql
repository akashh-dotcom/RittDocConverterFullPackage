CREATE TABLE [dbo].[tSpecial](
	[iSpecialId] [int] IDENTITY(1,1) NOT NULL,
	[vchName] [varchar](255) NULL,
	[dtStartDate] [smalldatetime] NOT NULL,
	[dtEndDate] [smalldatetime] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [tSpecial_PK] PRIMARY KEY CLUSTERED 
(
	[iSpecialId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY];


CREATE TABLE [dbo].[tSpecialDiscount](
	[iSpecialDiscountId] [int] IDENTITY(1,1) NOT NULL,
	[iDiscountPercentage] [int] NOT NULL,
	[iSpecialId] [int] NULL,
	[vchIconName] [varchar](50) NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL,
 CONSTRAINT [tSpecialDiscount_PK] PRIMARY KEY CLUSTERED 
(
	[iSpecialDiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[tSpecialDiscount]  WITH CHECK ADD FOREIGN KEY([iSpecialId])
REFERENCES [dbo].[tSpecial] ([iSpecialId]);


CREATE TABLE [dbo].[tSpecialResource](
	[iSpecialResourceId] [int] IDENTITY(1,1) NOT NULL,
	[iSpecialDiscountId] [int] NULL,
	[iResourceId] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL
) ON [PRIMARY];

ALTER TABLE [dbo].[tSpecialResource]  WITH CHECK ADD FOREIGN KEY([iResourceId])
REFERENCES [dbo].[tResource] ([iResourceId]);

ALTER TABLE [dbo].[tSpecialResource]  WITH CHECK ADD FOREIGN KEY([iSpecialDiscountId])
REFERENCES [dbo].[tSpecialDiscount] ([iSpecialDiscountId]);



ALTER TABLE tCartItem
ADD vchSpecialText varchar(255) null;
ALTER TABLE tCartItem
ADD vchSpecialIconName varchar(255) null;