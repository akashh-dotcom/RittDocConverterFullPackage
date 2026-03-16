#region

using FluentNHibernate.Mapping;
using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class EditorAffiliationMap : ClassMap<EditorAffiliation>
    {
        public EditorAffiliationMap()
        {
            // -- Table: tEditorAffiliation
            // -- Fields: ea.iEditorAffiliationId, ea.iEditorId, ea.vchJobTitle, ea.vchOrganization, ea.tiAffiliationOrder
            Table("tEditorAffiliation");
            Id(x => x.Id).Column("iEditorAffiliationId").GeneratedBy.Identity();
            References(x => x.Editor).Column("iEditorId");
            Map(x => x.JobTitle).Column("vchJobTitle");
            Map(x => x.Organization).Column("vchOrganization");
            Map(x => x.Order).Column("tiAffiliationOrder");
        }
    }
}