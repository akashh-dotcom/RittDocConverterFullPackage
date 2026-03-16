#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class DailyCountBase : Entity<int>, IEntity, IDailyCount
    {
        public virtual Institution.Institution Institution { get; set; }
        public virtual int UserId { get; set; }
        public virtual short IpAddressOctetA { get; set; }
        public virtual short IpAddressOctetB { get; set; }
        public virtual short IpAddressOctetC { get; set; }
        public virtual short IpAddressOctetD { get; set; }
        public virtual long IpAddressInteger { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual int Count { get; set; }
    }
}