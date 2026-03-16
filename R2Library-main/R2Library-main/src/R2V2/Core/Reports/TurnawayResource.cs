#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Reports
{
    public class TurnawayResource
    {
        public int InstitutionId { get; set; }
        public int ResourceId { get; set; }
        public IResource Resource { get; set; }

        public List<TurnawayDate> TurnawayDates { get; set; }
    }
}