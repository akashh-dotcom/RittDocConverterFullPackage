#region

using System;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class Trial
    {
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual DateTime? EmailWarningDate { get; set; }
        public virtual DateTime? Email3DayWarningDate { get; set; }
        public virtual DateTime? EmailFinalDate { get; set; }
    }
}