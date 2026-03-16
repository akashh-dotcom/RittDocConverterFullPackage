USE [DEV_RIT001];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[tInstitutionResourceLicense] ADD [dtResourceNotSaleableDate] smalldatetime NULL
GO


select irl.iInstitutionResourceLicenseId, irl.iInstitutionId, irl.iResourceId, irl.iLicenseCount, irl.tiLicenseTypeId, irl.tiLicenseOriginalSourceId, irl.dtFirstPurchaseDate, irl.dtPdaAddedDate, irl.dtPdaAddedToCartDate, irl.vchPdaAddedToCartById, irl.iPdaViewCount, irl.iPdaMaxViews, irl.dtCreationDate, irl.vchCreatorId, irl.dtLastUpdate, irl.vchUpdaterId, irl.tiRecordStatus
from   tInstitutionResourceLicense irl
where  tiLicenseTypeId = 3
  and  irl.dtPdaAddedToCartDate is null
  and  iResourceId in (
    select iResourceId from tResource r where NotSaleable = 1
  )
  
update irl
set    dtResourceNotSaleableDate = getdate()
     , dtLastUpdate = getdate()
     , irl.vchUpdaterId = 'sjscheider sql'
from   tInstitutionResourceLicense irl
where  tiLicenseTypeId = 3
  and  irl.dtPdaAddedToCartDate is null
  and  irl.dtResourceNotSaleableDate is null
  and  iResourceId in (
    select iResourceId from tResource r where NotSaleable = 1
  )
  

  
