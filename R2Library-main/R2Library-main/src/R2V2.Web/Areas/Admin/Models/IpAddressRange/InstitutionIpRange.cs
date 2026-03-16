#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class InstitutionIpRange : AdminBaseModel
    {
        public InstitutionIpRange()
        {
        }

        public InstitutionIpRange(IAdminInstitution institution) : base(institution)
        {
        }

        public IEnumerable<Core.Authentication.IpAddressRange> IpAddressRanges { get; set; }

        public Core.Authentication.IpAddressRange EditIpAddressRange { get; set; }

        public string IpRangeIdsToDelete { get; set; }
    }
}