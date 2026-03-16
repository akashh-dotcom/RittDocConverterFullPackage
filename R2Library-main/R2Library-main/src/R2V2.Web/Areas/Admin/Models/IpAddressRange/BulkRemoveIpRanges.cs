#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class BulkRemoveIpRanges : AdminBaseModel
    {
        public BulkRemoveIpRanges()
        {
        }

        public BulkRemoveIpRanges(IAdminInstitution institution) : base(institution)
        {
            InstitutionId = institution.Id;
        }

        public List<string> FormattedIpAddress { get; set; }

        public string IpRangeIdsToDelete { get; set; }
        public int InstitutionId { get; set; }
    }
}