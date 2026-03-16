

alter table tInstitutionResourceLicense
add dtPdaCartDeletedDate smalldatetime null;

alter table tInstitutionResourceLicense
add vchPdaCartDeletedByName varchar(150) null;

alter table tInstitutionResourceLicense
add iPdaCartDeletedById int null;

INSERT INTO [dbo].[tInstitutionResourceAuditType]
           ([iInstitutionResourceAuditTypeId]
           ,[AuditTypeDescription])
     VALUES
           ( 14
           , 'PDA resource deleted from cart')


