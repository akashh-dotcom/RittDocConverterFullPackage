#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class ContentView : Entity, IDebugInfo
    {
        // contentTurnawayId as [contentViewId], institutionId, userId, resourceId, chapterSectionId, turnawayTypeId
        public virtual int InstitutionId { get; set; }
        public virtual int UserId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual string ChapterSectionId { get; set; }
        public virtual int TurnawayTypeId { get; set; }

        // ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewTimestamp
        public virtual short IpAddressOctetA { get; set; }
        public virtual short IpAddressOctetB { get; set; }
        public virtual short IpAddressOctetC { get; set; }
        public virtual short IpAddressOctetD { get; set; }
        public virtual long IpAddressInteger { get; set; }
        public virtual DateTime Timestamp { get; set; }

        // actionTypeId, foundFromSearch, searchTerm, requestId
        public virtual short ActionTypeId { get; set; }
        public virtual short FoundFromSearch { get; set; }
        public virtual string SearchTerm { get; set; }
        public virtual string RequestId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("ContentView = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", ChapterSectionId: {0}", ChapterSectionId);
            sb.AppendFormat(", TurnawayTypeId: {0}", TurnawayTypeId);
            sb.AppendFormat(", IpAddressOctetA: {0}", IpAddressOctetA);
            sb.AppendFormat(", IpAddressOctetB: {0}", IpAddressOctetB);
            sb.AppendFormat(", IpAddressOctetC: {0}", IpAddressOctetC);
            sb.AppendFormat(", IpAddressOctetD: {0}", IpAddressOctetD);
            sb.AppendFormat(", IpAddressInteger: {0}", IpAddressInteger);
            sb.AppendFormat(", Timestamp: {0}", Timestamp);
            sb.AppendFormat(", ActionTypeId: {0}", ActionTypeId);
            sb.AppendFormat(", FoundFromSearch: {0}", FoundFromSearch);
            sb.AppendFormat(", SearchTerm: {0}", SearchTerm);
            sb.AppendFormat(", RequestId: {0}", RequestId);
            sb.Append("]");
            return sb.ToString();
        }
    }
}