#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionResourceAuditMap : BaseMap<InstitutionResourceAudit>
    {
        public InstitutionResourceAuditMap()
        {
            Table("tInstitutionResourceAudit");

            Id(x => x.Id).Column("iInstitutionResourceAuditId").GeneratedBy.Identity();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.UserId).Column("iUserId");
            Map(x => x.AuditTypeId).Column("iInstitutionResourceAuditTypeId");
            Map(x => x.LicenseCount).Column("iLicenseCount");

            Map(x => x.SingleLicensePrice).Column("decSingleLicensePrice");
            Map(x => x.PoNumber).Column("vchPoNumber");
            Map(x => x.CreatorId).Column("vchCreatorId");
            Map(x => x.CreationDate).Column("dtCreationDate");
            Map(x => x.EventDescription).Column("vchEventDescription");
            Map(x => x.Legacy).Column("bLegacy");
        }
    }
}