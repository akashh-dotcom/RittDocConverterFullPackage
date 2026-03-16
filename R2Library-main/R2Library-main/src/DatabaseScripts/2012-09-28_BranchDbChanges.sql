-- CHANGES FOR PROMOTION
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[tResource] ADD [dtQaApprovalDate] smalldatetime NULL
GO
ALTER TABLE [dbo].[tResource] ADD [dtLastPromotionDate] smalldatetime NULL
GO

ALTER TABLE [dbo].[tUser] ADD [tiEnablePromotion] tinyint NULL
GO
ALTER TABLE [dbo].[tUser] ADD CONSTRAINT [D_tUser_EnablePromotion] DEFAULT 0 FOR [tiEnablePromotion]
GO

--select * from tUser where vchUserName in ('mikemalone', 'sjscheider', 'mwhite')
update tUser set tiEnablePromotion = 1 where vchUserName in ('mikemalone', 'sjscheider', 'mwhite')

select * from tResource 

--update tResource
--set    dtLastPromotionDate = case when (dtCreationDate > dtRISReleaseDate) then dtCreationDate else dtRISReleaseDate end 
--where  tiRecordStatus = 1
--  and  iResourceStatusId in (6,7)
--
--update tResource
--set    dtQaApprovalDate = dtLastPromotionDate - 1
--where  dtLastPromotionDate is not null

--select stgR.iResourceId, stgR.vchResourceISBN
--     , prodR.iResourceId, prodR.vchResourceISBN, prodR.dtRISReleaseDate, prodR.dtResourcePublicationDate, prodR.dtCreationDate, prodR.dtLastUpdate, prodR.tiRecordStatus, prodR.vchForthcomingDate, prodR.NotSaleable
--     , case when (prodR.dtRISReleaseDate is null) then prodR.dtCreationDate
--                                  when (prodR.dtRISReleaseDate < prodR.dtCreationDate) then prodR.dtCreationDate
--                                  else prodR.dtRISReleaseDate
--                             end
--from   tResource stgR
--  join [RIT001_2012-08-22].dbo.tResource prodR on stgR.vchResourceISBN = prodR.vchResourceISBN and prodR.tiRecordStatus = 1 

update stgR
set    dtLastPromotionDate = case when (prodR.dtRISReleaseDate is null) then prodR.dtCreationDate
                                  when (prodR.dtRISReleaseDate < prodR.dtCreationDate) then prodR.dtCreationDate
                                  else prodR.dtRISReleaseDate
                             end
from   tResource stgR
  join [RIT001_2012-08-22].dbo.tResource prodR on stgR.vchResourceISBN = prodR.vchResourceISBN and prodR.tiRecordStatus = 1 


update tResource
set    dtQaApprovalDate = case when ((dtLastPromotionDate - 3) > dtCreationDate) then (dtLastPromotionDate - 3) else (dtLastPromotionDate - 3) end
where  dtLastPromotionDate is not null

select * from tResource 
where dtLastPromotionDate is null
  and  tiRecordStatus = 1 and NotSaleable = 0 and iResourceStatusId = 6
order by dtCreationDate desc

update tResource 
set    dtQaApprovalDate = getdate()
where dtLastPromotionDate is null
  and  tiRecordStatus = 1 and NotSaleable = 0 and iResourceStatusId = 6





