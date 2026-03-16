#region

using R2V2.Core.Audit;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceAuditMap : BaseMap<ResourceAudit>
    {
        public ResourceAuditMap()
        {
            Table("tResourceAudit");

            Id(x => x.Id).Column("iResourceAuditId").GeneratedBy.Identity();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.ResourceAuditType).Column("tiResourceAuditTypeId").CustomType<ResourceAuditType>();
            Map(x => x.CreatedBy).Column("vchCreatorId");
            Map(x => x.CreationDate).Column("dtCreationDate");
            //Map(x => x.EventDescription).Column("vchEventDescription");
            Map(x => x.EventDescription).Column("vchEventDescription").CustomType("StringClob")
                .CustomSqlType("nvarchar(max)");
        }
    }
}