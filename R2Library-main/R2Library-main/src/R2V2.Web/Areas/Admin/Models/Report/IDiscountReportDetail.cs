#region

using System.Collections.Generic;
using R2V2.Web.Areas.Admin.Models.Menus;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public interface IDiscountReportDetail
    {
        ActionsMenu ActionsMenu { get; set; }

        Dictionary<string, List<DiscountResourceDetail>> DiscountResources { get; set; }
    }
}