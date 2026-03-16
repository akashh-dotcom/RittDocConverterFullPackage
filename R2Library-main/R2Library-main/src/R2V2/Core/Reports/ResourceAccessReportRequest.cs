#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public class ResourceAccessReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int[] TerritoryIds { get; set; }
        public int[] InstitutionTypeIds { get; set; }
        public string[] AccountNumbers { get; set; }
    }
}