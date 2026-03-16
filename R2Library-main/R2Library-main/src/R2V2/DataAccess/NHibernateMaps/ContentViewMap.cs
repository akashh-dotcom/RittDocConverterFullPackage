#region

using FluentNHibernate.Mapping;
using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ContentViewMap : ClassMap<ContentView>
    {
        public ContentViewMap()
        {
            ReadOnly();

            Table("vContentView");

            // contentTurnawayId as [contentViewId], institutionId, userId, resourceId, chapterSectionId, turnawayTypeId
            Id(x => x.Id).Column("contentViewId");
            Map(x => x.InstitutionId).Column("institutionId");
            Map(x => x.UserId).Column("userId");
            Map(x => x.ResourceId).Column("resourceId");
            Map(x => x.ChapterSectionId).Column("chapterSectionId");
            Map(x => x.TurnawayTypeId).Column("turnawayTypeId");

            // ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewTimestamp
            Map(x => x.IpAddressOctetA).Column("ipAddressOctetA");
            Map(x => x.IpAddressOctetB).Column("ipAddressOctetB");
            Map(x => x.IpAddressOctetC).Column("ipAddressOctetC");
            Map(x => x.IpAddressOctetD).Column("ipAddressOctetD");
            Map(x => x.IpAddressInteger).Column("ipAddressInteger");
            Map(x => x.Timestamp).Column("contentViewTimestamp");

            // actionTypeId, foundFromSearch, searchTerm, requestId
            Map(x => x.ActionTypeId).Column("actionTypeId");
            Map(x => x.FoundFromSearch).Column("foundFromSearch");
            Map(x => x.SearchTerm).Column("searchTerm");
            Map(x => x.RequestId).Column("requestId");
        }
    }
}