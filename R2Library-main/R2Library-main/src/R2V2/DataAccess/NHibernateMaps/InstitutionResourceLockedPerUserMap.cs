#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionResourceLockedPerUserMap : BaseMap<InstitutionResourceLockedPerUser>
    {
        public InstitutionResourceLockedPerUserMap()
        {
            Table("tInstitutionResourceLockedPerUser");

            Id(x => x.Id).Column("iInstitutionResourceLockedPerUserId").GeneratedBy.Identity();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.ResourceId).Column("iResourceId");
        }
    }
}