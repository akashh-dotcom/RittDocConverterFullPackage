#region

using System.Linq;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.OrderHistory
{
    public class OrderHistoryDetail : AdminBaseModel
    {
        public OrderHistoryDetail(IAdminInstitution institution) : base(institution)
        {
        }

        public OrderHistoryDetail(IAdminInstitution institution, Core.OrderHistory.OrderHistory orderHistory,
            string iconBaseUrl, bool requireSsl)
            : base(institution)
        {
            SpecialIconBaseUrl = iconBaseUrl;
            OrderHistory = new WebOrderHistory(orderHistory, institution);
            RequireSsl = requireSsl;
        }

        public OrderHistoryDetail(IAdminInstitution institution, Core.OrderHistory.OrderHistory orderHistory)
            : base(institution)
        {
            OrderHistory = new WebOrderHistory(orderHistory, institution);
        }

        public bool RequireSsl { get; set; }

        public string SpecialIconBaseUrl { get; set; }

        public WebOrderHistory OrderHistory { get; set; }

        public string GetDisplayDiscountLabel(bool doNotFloat = false)
        {
            if (OrderHistory.DiscountType == 3 &&
                OrderHistory.OrderHistoryResources.All(x => string.IsNullOrWhiteSpace(x.SpecialText)))
            {
                if (doNotFloat)
                {
                    return
                        $"<span style=\"font-weight:normal\">(Automated cart preferred discount applied)</span> Discount";
                }

                return
                    $"<div><span style=\"font-weight:normal\">(Automated cart preferred discount applied)</span> Discount</div>";
            }

            return "Discount";
        }

        public string GetDisplayDiscount()
        {
            if (OrderHistory.OrderHistoryResources.Any(x => !string.IsNullOrWhiteSpace(x.SpecialText)))
            {
                return "<div style=\"float:left\">Variable (specials applied)</div>";
            }

            return $"{OrderHistory.Discount}%";
        }

        public bool IsSingleDiscount()
        {
            return OrderHistory.OrderHistoryResources.All(test => string.IsNullOrWhiteSpace(test.SpecialText));
        }
    }
}