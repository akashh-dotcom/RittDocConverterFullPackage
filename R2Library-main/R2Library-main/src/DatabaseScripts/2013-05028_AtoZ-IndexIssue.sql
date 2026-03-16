

CREATE NONCLUSTERED INDEX [IX_tAtoZIndex_chrAlphaKey_iAtoZIndexTypeId]
ON [dbo].[tAtoZIndex] ([chrAlphaKey],[iAtoZIndexTypeId])
INCLUDE ([vchName],[iResourceId])


--select distinct az.vchName 
--from   dbo.tAtoZIndex az  
-- join  dbo.tResource r on az.iResourceId = r.iResourceId and r.tiRecordStatus = 1  
-- join  dbo.tInstitutionResourceLicense irl on az.iResourceId = irl.iResourceId and irl.tiRecordStatus = 1 
--where  az.chrAlphaKey = 'A'   
--   and ((irl.iInstitutionId = 1260 and (irl.tiLicenseTypeId = 1 and irl.iLicenseCount > 0)        
--    or (irl.tiLicenseTypeId = 3 and irl.dtPdaAddedToCartDate is null))     or (1 = 1 and r.NotSaleable = 0))   
--   and  az.iAtoZIndexTypeId in (1,3,5) 
--order by az.vchName;


