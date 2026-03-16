#region

using FluentNHibernate.Mapping;
using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PageContentViewMap : ClassMap<PageContentView>
    {
        public PageContentViewMap()
        {
            ReadOnly();

            Table("vPageContentView");

            // pageViewId, institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger
            Id(x => x.Id).Column("pageViewId");
            Map(x => x.InstitutionId).Column("institutionId");
            Map(x => x.UserId).Column("userId");
            Map(x => x.IpAddressOctetA).Column("ipAddressOctetA");
            Map(x => x.IpAddressOctetB).Column("ipAddressOctetB");
            Map(x => x.IpAddressOctetC).Column("ipAddressOctetC");
            Map(x => x.IpAddressOctetD).Column("ipAddressOctetD");
            Map(x => x.IpAddressInteger).Column("ipAddressInteger");

            // pageViewTimestamp, pageViewRunTime, sessionId, url, requestId, referrer, countryCode, serverNumber
            Map(x => x.Timestamp).Column("pageViewTimestamp");
            Map(x => x.RunTime).Column("pageViewRunTime");
            Map(x => x.SessionId).Column("sessionId");
            Map(x => x.Url).Column("url");
            Map(x => x.RequestId).Column("requestId");
            Map(x => x.Referrer).Column("referrer");
            Map(x => x.CountryCode).Column("countryCode");
            Map(x => x.ServerNumber).Column("serverNumber");

            // cv.contentTurnawayId, cv.resourceId, cv.chapterSectionId, cv.turnawayTypeId, cv.actionTypeId, cv.foundFromSearch, cv.searchTerm
            Map(x => x.ContentViewId).Column("contentTurnawayId");
            Map(x => x.ResourceId).Column("resourceId");
            Map(x => x.ChapterSectionId).Column("chapterSectionId");
            Map(x => x.TurnawayTypeId).Column("turnawayTypeId");
            Map(x => x.ActionTypeId).Column("actionTypeId");
            Map(x => x.FoundFromSearch).Column("foundFromSearch");
            Map(x => x.SearchTerm).Column("searchTerm");

            // u.vchFirstName, u.vchLastName, u.vchUserName, u.vchUserEmail
            Map(x => x.FirstName).Column("vchFirstName");
            Map(x => x.LastName).Column("vchLastName");
            Map(x => x.Username).Column("vchUserName");
            Map(x => x.EmailAddress).Column("vchUserEmail");
        }
    }
}