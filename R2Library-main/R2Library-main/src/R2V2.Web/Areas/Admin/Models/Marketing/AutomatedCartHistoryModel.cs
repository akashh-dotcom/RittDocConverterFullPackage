#region

using System.Collections.Generic;
using R2V2.Core.AutomatedCart;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class AutomatedCartHistoryModel : AdminBaseModel
    {
        public List<AutomatedCartHistory> AutomatedCartHistories { get; set; }
    }
}