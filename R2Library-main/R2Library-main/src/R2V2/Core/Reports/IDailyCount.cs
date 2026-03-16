#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public interface IDailyCount
    {
        Institution.Institution Institution { get; set; }
        int UserId { get; set; }
        short IpAddressOctetA { get; set; }
        short IpAddressOctetB { get; set; }
        short IpAddressOctetC { get; set; }
        short IpAddressOctetD { get; set; }
        long IpAddressInteger { get; set; }
        DateTime Date { get; set; }
        int Count { get; set; }
    }
}