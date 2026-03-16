#region

using FluentNHibernate.Mapping;
using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DailyPageViewCountMap : ClassMap<DailyPageViewCount>
    {
        public DailyPageViewCountMap()
        {
            Table("vDailyPageViewCount");

            Id(x => x.Id).Column("dailyPageViewCountId").GeneratedBy.Identity();
            //Map(x => x.InstitutionId).Column("institutionId");

            References(x => x.Institution).Column("institutionId");

            Map(x => x.UserId).Column("userId");

            Map(x => x.IpAddressOctetA).Column("ipAddressOctetA");
            Map(x => x.IpAddressOctetB).Column("ipAddressOctetB");
            Map(x => x.IpAddressOctetC).Column("ipAddressOctetC");
            Map(x => x.IpAddressOctetD).Column("ipAddressOctetD");
            Map(x => x.IpAddressInteger).Column("ipAddressInteger");

            Map(x => x.Date).Column("pageViewDate");
            Map(x => x.Count).Column("pageViewCount");
        }
    }
}