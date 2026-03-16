#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class InstitutionIpRanges : AdminBaseModel
    {
        public InstitutionIpRanges()
        {
        }

        public InstitutionIpRanges(IAdminInstitution institution) : base(institution)
        {
        }

        public string CurrentIpAddress { get; set; }
        public IEnumerable<WebIpRange> IpAddressRanges { get; set; }

        public WebIpRange EditIpAddressRange { get; set; }

        public string IpRangeIdsToDelete { get; set; }

        public IEnumerable<ConflictingIpRange> ConflictingIpRanges { get; set; }

        public bool CanMergeRanges { get; set; }

        public bool IsIpPlusEnabled { get; set; }

        public bool ConflictedIpRangesDisplayed()
        {
            return ConflictingIpRanges != null && ConflictingIpRanges.Any();
        }

        public bool DisplayFixIpRanes()
        {
            if (ConflictingIpRanges != null && ConflictingIpRanges.Any())
            {
                foreach (var conflictingIpRange in ConflictingIpRanges)
                {
                    //Has at least 1 range that can be consoidated
                    if (conflictingIpRange.IpAddressRange1.InstitutionId == InstitutionId &&
                        conflictingIpRange.IpAddressRange2.InstitutionId == InstitutionId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool AllowEditDelete()
        {
            if (ConflictedIpRangesDisplayed())
            {
                if (!(IsRittenhouseAdmin || IsSalesAssociate))
                {
                    return false;
                }
            }

            return true;
        }

        public bool DisplayConflictIpRangeLink(WebIpRange ipRange)
        {
            return (IsRittenhouseAdmin || IsSalesAssociate) && ipRange.InstitutionId != InstitutionId;
        }

        public string GetIpRangeDescription(WebIpRange ipRange)
        {
            if (IsRittenhouseAdmin || IsSalesAssociate)
            {
                if (ipRange.InstitutionId == InstitutionId)
                {
                    return "Entered Below";
                }
                else
                {
                    return ipRange.AccountNumber;
                }
            }

            return ipRange.InstitutionId != InstitutionId ? "Taken by Another Institution" : "Entered Below";
        }
    }
}