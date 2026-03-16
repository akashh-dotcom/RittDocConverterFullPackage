/*
Missing Index Details from SQLQuery5.sql - RITTDB3.RIT001 (R2LIBRARY\Scott.Scheider (129))
The Query Processor estimates that implementing the following index could improve the query cost by 96.1243%.
*/

/*
USE [RIT001]
GO
CREATE NONCLUSTERED INDEX [<Name of Missing Index, sysname,>]
ON [dbo].[tInstitutionResourceLicense] ([iInstitutionId])
INCLUDE ([iInstitutionResourceLicenseId],[iResourceId],[iLicenseCount],[tiLicenseTypeId],[tiLicenseOriginalSourceId],[dtFirstPurchaseDate],[dtPdaAddedDate],[dtPdaAddedToCartDate],[vchPdaAddedToCartById],[iPdaViewCount],[iPdaMaxViews],[dtCreationDate],[vchCreatorId],[dtLastUpdate],[vchUpdaterId],[tiRecordStatus])
GO
*/
CREATE NONCLUSTERED INDEX [<Name of Missing Index, sysname,>]
ON [dbo].[tInstitutionResourceLicense] ([iInstitutionId])
INCLUDE ([iInstitutionResourceLicenseId],[iResourceId],[iLicenseCount],[tiLicenseTypeId],[tiLicenseOriginalSourceId],[dtFirstPurchaseDate],[dtPdaAddedDate],[dtPdaAddedToCartDate],[vchPdaAddedToCartById],[iPdaViewCount],[iPdaMaxViews],[dtCreationDate],[vchCreatorId],[dtLastUpdate],[vchUpdaterId],[tiRecordStatus])
GO

EXEC sys.sp_rename @objname = N'[dbo].[tInstitutionResourceLicense].[<Name of Missing Index, sysname,>]', @newname = [IX_tInstitutionResourceLicense_InstitutionId], @objtype = N'INDEX';
GO


CREATE NONCLUSTERED INDEX [IX_tAtoZIndex_ResourceIsbn]
ON [dbo].[tAtoZIndex] ([vchResourceISBN])
INCLUDE ([vchName],[vchChapterId],[vchSectionId])
GO

CREATE NONCLUSTERED INDEX [IX_tAtoZIndex_Name>]
ON [dbo].[tAtoZIndex] ([vchName])
INCLUDE ([iResourceId])



CREATE NONCLUSTERED INDEX [IX_tInstitutionResourceLicense_ResourceId_RecordStatus]
ON [dbo].[tInstitutionResourceLicense] ([iResourceId],[tiRecordStatus])
INCLUDE ([iInstitutionId],[iLicenseCount],[tiLicenseTypeId],[dtPdaAddedToCartDate])
GO



