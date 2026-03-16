#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class PageContentView : Entity, IDebugInfo
    {
        // pageViewId, institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger
        public virtual int InstitutionId { get; set; }
        public virtual int UserId { get; set; }
        public virtual short IpAddressOctetA { get; set; }
        public virtual short IpAddressOctetB { get; set; }
        public virtual short IpAddressOctetC { get; set; }
        public virtual short IpAddressOctetD { get; set; }
        public virtual long IpAddressInteger { get; set; }

        // pageViewTimestamp, pageViewRunTime, sessionId, url, requestId, referrer, countryCode, serverNumber
        public virtual DateTime Timestamp { get; set; }
        public virtual int RunTime { get; set; }
        public virtual string SessionId { get; set; }
        public virtual string Url { get; set; }
        public virtual string RequestId { get; set; }
        public virtual string Referrer { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual short ServerNumber { get; set; }

        // cv.contentTurnawayId, cv.resourceId, cv.chapterSectionId, cv.turnawayTypeId, cv.actionTypeId, cv.foundFromSearch, cv.searchTerm
        public virtual int ContentViewId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual string ChapterSectionId { get; set; }
        public virtual int TurnawayTypeId { get; set; }
        public virtual short ActionTypeId { get; set; }
        public virtual short FoundFromSearch { get; set; }
        public virtual string SearchTerm { get; set; }

        // u.vchFirstName, u.vchLastName, u.vchUserName, u.vchUserEmail
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Username { get; set; }
        public virtual string EmailAddress { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PageContentView = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", IpAddressOctetA: {0}", IpAddressOctetA);
            sb.AppendFormat(", IpAddressOctetB: {0}", IpAddressOctetB);
            sb.AppendFormat(", IpAddressOctetC: {0}", IpAddressOctetC);
            sb.AppendFormat(", IpAddressOctetD: {0}", IpAddressOctetD);
            sb.AppendFormat(", IpAddressInteger: {0}", IpAddressInteger);
            sb.AppendFormat(", Timestamp: {0}", Timestamp);
            sb.AppendFormat(", RunTime: {0}", RunTime);
            sb.AppendFormat(", SessionId: {0}", SessionId);
            sb.AppendFormat(", Url: {0}", Url);
            sb.AppendFormat(", RequestId: {0}", RequestId);
            sb.AppendFormat(", CountryCode: {0}", CountryCode);
            sb.AppendFormat(", ServerNumber: {0}", ServerNumber);
            sb.AppendFormat(", RequestId: {0}", RequestId);

            sb.AppendFormat(", ContentViewId: {0}", ContentViewId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", ChapterSectionId: {0}", ChapterSectionId);
            sb.AppendFormat(", TurnawayTypeId: {0}", TurnawayTypeId);
            sb.AppendFormat(", ActionTypeId: {0}", ActionTypeId);
            sb.AppendFormat(", FoundFromSearch: {0}", FoundFromSearch);
            sb.AppendFormat(", SearchTerm: {0}", SearchTerm);
            sb.AppendFormat(", FirstName: {0}", FirstName);
            sb.AppendFormat(", LastName: {0}", LastName);
            sb.AppendFormat(", Username: {0}", Username);
            sb.AppendFormat(", EmailAddress: {0}", EmailAddress);
            sb.Append("]");
            return sb.ToString();
        }
    }
}