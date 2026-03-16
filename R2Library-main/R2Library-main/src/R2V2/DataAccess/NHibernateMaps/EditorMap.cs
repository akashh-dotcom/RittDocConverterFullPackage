#region

using FluentNHibernate.Mapping;
using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class EditorMap : ClassMap<Editor>
    {
        public EditorMap()
        {
            // -- Table: tEditor
            // -- Fields: e.iEditorId, e.iResourceId, e.vchFirstName, e.vchLastName, e.vchMiddleName, e.vchLineage, e.vchDegree, e.tiEditorOrder

            Table("tEditor");
            Id(x => x.Id).Column("iEditorId").GeneratedBy.Identity();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.FirstName).Column("vchFirstName");
            Map(x => x.LastName).Column("vchLastName");
            Map(x => x.MiddleName).Column("vchMiddleName");
            Map(x => x.Lineage).Column("vchLineage");
            Map(x => x.Degrees).Column("vchDegree");
            Map(x => x.Order).Column("tiEditorOrder");

            HasMany(x => x.Affiliations).KeyColumn("iEditorId").Inverse().Cascade.All();
        }
    }
}