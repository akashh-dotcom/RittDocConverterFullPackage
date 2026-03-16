#region

using FluentNHibernate.Mapping;
using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DailySessionCountMap : ClassMap<DailySessionCount>
    {
        public DailySessionCountMap()
        {
            Table("vDailySessionCount");
            Id(x => x.Id).Column("dailySessionCountId").GeneratedBy.Identity();
            References(x => x.Institution).Column("institutionId");
            Map(x => x.UserId).Column("userId");
            Map(x => x.IpAddressOctetA).Column("ipAddressOctetA");
            Map(x => x.IpAddressOctetB).Column("ipAddressOctetB");
            Map(x => x.IpAddressOctetC).Column("ipAddressOctetC");
            Map(x => x.IpAddressOctetD).Column("ipAddressOctetD");
            Map(x => x.IpAddressInteger).Column("ipAddressInteger");
            Map(x => x.Date).Column("sessionDate");
            Map(x => x.Count).Column("sessionCount");
        }
    }
}