#region

using FluentNHibernate.Mapping;
using R2V2.Core.RequestLogger;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class Ip2LocationMap : ClassMap<Ip2Location>
    {
        public Ip2LocationMap()
        {
            Table("tIp2Location");
            Id(x => x.Id).Column("iIp2LocationId").GeneratedBy.Identity();
            Map(x => x.IpFrom, "iIpFrom").ReadOnly();
            Map(x => x.IpTo, "iIpTo").ReadOnly();
            Map(x => x.CountryCode, "vchCountryCode").ReadOnly();
            Map(x => x.CountryName, "vchCountryName").ReadOnly();
        }
    }
}