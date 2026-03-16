#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ResourceCostDetail : AdminBaseModel
    {
        public ResourceCostDetail()
        {
        }

        public ResourceCostDetail(AdminInstitution institution) : base(institution)
        {
        }
    }
}