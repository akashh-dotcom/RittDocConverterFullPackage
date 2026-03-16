#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public class TurnawayDate
    {
        public int ResourceId { get; set; }
        public DateTime TurnawayTimeStamp { get; set; }
        public bool IsAccessTurnaway { get; set; }
        public string RequestId { get; set; }
        public string SessionId { get; set; }
        public string IpAddress { get; set; }
        public int InstitutionId { get; set; }
    }
}