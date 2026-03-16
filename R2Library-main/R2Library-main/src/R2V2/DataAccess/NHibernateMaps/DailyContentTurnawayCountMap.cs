#region

using FluentNHibernate.Mapping;
using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DailyContentTurnawayCountMap : ClassMap<DailyContentTurnawayCount>
    {
        public DailyContentTurnawayCountMap()
        {
            Table("vDailyContentTurnawayCount");
            Id(x => x.Id).Column("dailyContentTurnawayCountId").GeneratedBy.Identity();
            References(x => x.Institution).Column("institutionId");
            Map(x => x.UserId).Column("userId");

            Map(x => x.ResourceId).Column("resourceId");
            Map(x => x.ChapterSectionId).Column("chapterSectionId");
            Map(x => x.TurnawayTypeId).Column("turnawayTypeId");

            Map(x => x.IpAddressOctetA).Column("ipAddressOctetA");
            Map(x => x.IpAddressOctetB).Column("ipAddressOctetB");
            Map(x => x.IpAddressOctetC).Column("ipAddressOctetC");
            Map(x => x.IpAddressOctetD).Column("ipAddressOctetD");
            Map(x => x.IpAddressInteger).Column("ipAddressInteger");
            Map(x => x.Date).Column("contentTurnawayDate");
            Map(x => x.Count).Column("contentTurnawayCount");
            Map(x => x.ActionTypeId).Column("actionTypeId");
        }
    }
}