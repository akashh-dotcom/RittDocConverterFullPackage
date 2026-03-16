#region

using FluentNHibernate.Mapping;
using R2V2.Core.Resource.Author;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class AuthorMap : ClassMap<Author>
    {
        public AuthorMap()
        {
            // -- Table: tAuthor
            // -- Fields: a.iAuthorId, a.iResourceId, a.vchFirstName, a.vchLastName, a.vchMiddleName, a.vchLineage, a.vchDegree, a.tiAuthorOrder
            Table("tAuthor");
            Id(x => x.Id).Column("iAuthorId").GeneratedBy.Identity();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.FirstName).Column("vchFirstName");
            Map(x => x.LastName).Column("vchLastName");
            Map(x => x.MiddleName).Column("vchMiddleName");
            Map(x => x.Lineage).Column("vchLineage");
            Map(x => x.Degrees).Column("vchDegree");
            Map(x => x.Order).Column("tiAuthorOrder");
        }
    }
}