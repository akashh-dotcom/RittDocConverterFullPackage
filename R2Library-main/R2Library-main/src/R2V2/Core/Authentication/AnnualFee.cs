#region

using System;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class AnnualFee
    {
        public virtual DateTime? FeeDate { get; set; }
        public virtual string PO { get; set; }
        public virtual int PayType { get; set; }

        public bool HasAnnualFee => FeeDate.HasValue;
    }
}