#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.OrderHistory
{
    public class OrderHistoryList : AdminBaseModel
    {
        public OrderHistoryList(IAdminInstitution institution) : base(institution)
        {
        }

        public OrderHistoryList(IAdminInstitution institution,
            IEnumerable<Core.OrderHistory.OrderHistory> orderHistories)
            : base(institution)
        {
            OrderHistories = orderHistories.Select(x => x).OrderByDescending(x => x.PurchaseDate);
        }

        public IEnumerable<IOrder> Orders { get; set; }

        public IEnumerable<Core.OrderHistory.OrderHistory> OrderHistories { get; set; }
    }
}