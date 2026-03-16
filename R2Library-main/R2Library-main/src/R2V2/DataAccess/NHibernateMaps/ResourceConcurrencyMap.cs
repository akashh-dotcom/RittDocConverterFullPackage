#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceConcurrencyMap : BaseMap<ResourceConcurrency>
    {
        public ResourceConcurrencyMap()
        {
            Table("tResourceConcurreny");

            Id(x => x.Id).Column("iResourceConcurrenyId").GeneratedBy.Identity();

            Map(x => x.SessionId).Column("vchSessionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.InstitutionId).Column("iInstitutionId");

            //References(x => x.Resource).Column("iResourceId").Cascade.None();
            //References(x => x.User).Column("iUserId").Cascade.None();
            //References(x => x.Institution).Column("iInstitutionId").Cascade.None();
        }
    }
}