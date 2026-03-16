#region

using System.Collections.Generic;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    public class ResourcePurchaseModel
    {
        public IEnumerable<IOrderItem> OrderItems { get; set; }
    }
}