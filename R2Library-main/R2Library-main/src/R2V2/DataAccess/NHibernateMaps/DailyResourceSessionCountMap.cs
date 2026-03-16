#region

using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DailyResourceSessionCountMap : BaseMap<DailyResourceSessionCount>
    {
        public DailyResourceSessionCountMap()
        {
            Table("vDailyResourceSessionCount");
            Id(x => x.Id).Column("dailyResourceSessionCountId").GeneratedBy.Identity();
            References(x => x.Institution).Column("institutionId");
            Map(x => x.UserId).Column("userId");
            Map(x => x.IpAddressOctetA).Column("ipAddressOctetA");
            Map(x => x.IpAddressOctetB).Column("ipAddressOctetB");
            Map(x => x.IpAddressOctetC).Column("ipAddressOctetC");
            Map(x => x.IpAddressOctetD).Column("ipAddressOctetD");
            Map(x => x.IpAddressInteger).Column("ipAddressInteger");
            Map(x => x.Date).Column("sessionDate");
            Map(x => x.Count).Column("sessionCount");
            Map(x => x.ResourceId).Column("resourceId");
        }
    }
}