#region

using FluentNHibernate.Mapping;
using R2V2.Core;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PingMap : ClassMap<Ping>
    {
        public PingMap()
        {
            Table("tPing");
            Id(x => x.Id).Column("iPingId").GeneratedBy.Identity();
            Map(x => x.StatusCode).Column("vchStatusCode");
        }
    }
}