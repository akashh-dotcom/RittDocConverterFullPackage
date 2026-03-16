#region

using R2V2.Core.Audit;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionAuditMap : BaseMap<InstitutionAudit>
    {
        public InstitutionAuditMap()
        {
            Table("tInstitutionAudit");

            Id(x => x.Id).Column("iInstitutionAuditId").GeneratedBy.Identity();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.InstitutionAuditType).Column("tiInstitutionAuditTypeId").CustomType<InstitutionAuditType>();


            Map(x => x.CreatedBy).Column("vchCreatorId");
            Map(x => x.CreationDate).Column("dtCreationDate");

            Map(x => x.EventDescription).Column("vchEventDescription");
        }
    }
}