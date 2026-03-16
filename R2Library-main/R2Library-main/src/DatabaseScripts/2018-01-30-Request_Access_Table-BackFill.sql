drop table [tUserResourceRequest]
go 
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tUserResourceRequest](
	[iUserResourceRequestId] [int] IDENTITY(1,1) NOT NULL,
	[iUserId] [int] NULL,
	[iInstitutionId] [int] NOT NULL,
	[vchName] [varchar](50) NULL,
	[vchTitle] [varchar](250) NULL,
	[iResourceId] [int] NOT NULL,
	[vchCreatorId] [varchar](50) NOT NULL,
	[dtCreationDate] [smalldatetime] NOT NULL,
	[vchUpdaterId] [varchar](50) NULL,
	[dtLastUpdate] [smalldatetime] NULL,
	[tiRecordStatus] [tinyint] NOT NULL, 
PRIMARY KEY CLUSTERED 
(
	[iUserResourceRequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[tUserResourceRequest]  WITH CHECK ADD FOREIGN KEY([iInstitutionId])
REFERENCES [dbo].[tInstitution] ([iInstitutionId])
GO

ALTER TABLE [dbo].[tUserResourceRequest]  WITH CHECK ADD FOREIGN KEY([iResourceId])
REFERENCES [dbo].[tResource] ([iResourceId])
GO


--1002
insert into [tUserResourceRequest](iUserId, iInstitutionId, vchName, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select isnull(u.iuserId, null), institutionId
, case when u.iuserid is null then 'N/A' else u.vchFirstName + ' ' + u.vchLastName end
, r.iResourceId, 'BackFill Data', pageViewTimestamp, 1
from R2Reports_Prod..PageView pv
join tResource r on r.vchResourceISBN = substring(url, 32, 10)
left join tUser u on pv.userId = u.iUserId
where pv.url like '%/Contact/MyAdministrators?isbn%'
and pv.referrer like '%/Contact/MyAdministrators?isbn%'
and institutionId > 0

