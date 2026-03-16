#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class InstitutionIpRangesConsolidated : AdminBaseModel
    {
        public InstitutionIpRangesConsolidated()
        {
        }

        public InstitutionIpRangesConsolidated(IAdminInstitution institution) : base(institution)
        {
        }

        public string ErrorMessage { get; set; }
        public bool GetConflicts { get; set; }
        public List<ConsolidatedIpRange> ConsolidateIpAddressRanges { get; set; }
    }
}