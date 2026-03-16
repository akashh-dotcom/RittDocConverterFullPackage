#region

using System;
using R2V2.Contexts;

#endregion

namespace R2V2.Core.Institution
{
    public class InstitutionResourceAuditFactory
    {
        private readonly IAuthenticationContext _authenticationContext;

        public InstitutionResourceAuditFactory(IAuthenticationContext authenticationContext)
        {
            _authenticationContext = authenticationContext;
        }

        public InstitutionResourceAudit BuildAuditRecord(InstitutionResourceAuditType institutionResourceAuditType,
            int institutionId, int resourceId)
        {
            return BuildAuditRecord(institutionResourceAuditType, institutionId, resourceId, 0, null, 0, -1);
        }

        public InstitutionResourceAudit BuildAuditRecord(InstitutionResourceAuditType institutionResourceAuditType,
            int institutionId, int resourceId, int licenseCount)
        {
            return BuildAuditRecord(institutionResourceAuditType, institutionId, resourceId, licenseCount, null, 0, -1);
        }

        public InstitutionResourceAudit BuildAuditRecord(InstitutionResourceAuditType institutionResourceAuditType,
            int institutionId, int resourceId,
            int licenseCount, string poNumber, decimal singleLicensePrice, int totalLicenseCount)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            var userId = authenticatedInstitution.User != null ? authenticatedInstitution.User.Id : 0;

            var audit = new InstitutionResourceAudit
            {
                AuditTypeId = (short)institutionResourceAuditType,
                CreationDate = DateTime.Now,
                CreatorId = $"user id: {userId}",
                EventDescription =
                    totalLicenseCount < 0
                        ? institutionResourceAuditType.ToDescription()
                        : $"{institutionResourceAuditType.ToDescription()} - total license count: {totalLicenseCount}",
                Id = 0,
                InstitutionId = institutionId,
                Legacy = false,
                LicenseCount = licenseCount,
                PoNumber = poNumber,
                ResourceId = resourceId,
                SingleLicensePrice = singleLicensePrice,
                UserId = userId
            };
            return audit;
        }
    }
}