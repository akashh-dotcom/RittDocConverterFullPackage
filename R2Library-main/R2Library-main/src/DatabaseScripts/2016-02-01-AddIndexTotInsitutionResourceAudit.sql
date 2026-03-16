CREATE NONCLUSTERED INDEX [IX_tInstitutionResourceAudit_iInstitutionResourceAuditTypeId_dtCreationDate]
ON [dbo].[tInstitutionResourceAudit]
([iInstitutionResourceAuditTypeId] , [dtCreationDate])
INCLUDE ([iInstitutionId])
WITH
(
PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ONLINE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE
)
ON [PRIMARY];
GO
