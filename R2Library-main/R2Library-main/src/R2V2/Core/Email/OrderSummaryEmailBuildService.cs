#region

using System;
using System.Text;
using R2V2.Core.CollectionManagement;
using R2V2.Core.OrderHistory;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class OrderSummaryEmailBuildService : EmailBuildBaseService
    {
        readonly StringBuilder _itemBuilder = new StringBuilder();

        public OrderSummaryEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            SetTemplates(OrderSummaryBodyTemplate, OrderSummaryItemTemplate, false);
        }

        public EmailMessage BuildOrderSummaryEmail(int totalOrders, decimal totalSales, int totalTitles,
            int totalLicenses, int totalMaintenanceFee, string[] emails)
        {
            var messageBody = GetOrderSummaryEmailHtml(totalOrders, totalSales, totalTitles, totalLicenses,
                totalMaintenanceFee);

            return BuildEmailMessage(emails, "R2 Library Daily Order Summary", messageBody);
        }

        private string GetOrderSummaryEmailHtml(int totalOrders, decimal totalSales, int totalTitles, int totalLicenses,
            int totalMaintenanceFee)
        {
            var bodyBuilder = BuildBodyHtml(totalOrders, totalSales, totalTitles, totalLicenses, totalMaintenanceFee);

            var mainBuilder = BuildMainHtml("Daily Order Summary", bodyBuilder, null);

            return mainBuilder;
        }

        public void BuildItemHtml(Cart cart, string institutionInfo, string resourceItems, string productItems,
            decimal cartTotal)
        {
            _itemBuilder.Append(ItemTemplate
                .Replace("{Institution_Info}", institutionInfo)
                .Replace("{Order_PO_Number}", PopulateField("PO #: ", cart.PurchaseOrderNumber))
                .Replace("{R2_Order_Number}", PopulateField("R2 Order #: ", cart.OrderNumber))
                .Replace("{Order_PO_Comment}", PopulateField("PO Comment: ", cart.PurchaseOrderComment))
                .Replace("{Order_Billing_Method}", PopulateField("Billing Method: ", cart.BillingMethod.ToString()))
                .Replace("{Order_Isbns}", resourceItems)
                .Replace("{Order_Products}", productItems)
                .Replace("{Order_Total_Price}", $"{cartTotal:C}")
            );
        }

        public void BuildItemHtml(DbOrderHistory orderHistory, string institutionInfo, string resourceItems,
            string productItems, decimal cartTotal)
        {
            _itemBuilder.Append(ItemTemplate
                .Replace("{Institution_Info}", institutionInfo)
                .Replace("{Order_PO_Number}", PopulateField("PO #: ", orderHistory.PurchaseOrderNumber))
                .Replace("{R2_Order_Number}", PopulateField("R2 Order #: ", orderHistory.OrderNumber))
                .Replace("{Order_PO_Comment}", PopulateField("PO Comment: ", orderHistory.PurchaseOrderComment))
                .Replace("{Order_Billing_Method}",
                    PopulateField("Billing Method: ", orderHistory.BillingMethod.ToString()))
                .Replace("{Order_Isbns}", resourceItems)
                .Replace("{Order_Products}", productItems)
                .Replace("{Order_Total_Price}", $"{cartTotal:C}")
            );
        }

        public string BuildBodyHtml(int totalOrders, decimal totalSales, int totalTitles, int totalLicenses,
            int totalMaintenanceFee)
        {
            var bodyBuilder = new StringBuilder()
                .Append(BodyTemplate
                    .Replace("{Order_Date}", DateTime.Now.ToLongDateString())
                    .Replace("{Order_Count}", totalOrders.ToString())
                    .Replace("{Order_Sales}", $"{totalSales:#,##0.00}")
                    .Replace("{Order_Titles_Count}", $"{totalTitles:#,##0}")
                    .Replace("{Order_Licenses_Count}", $"{totalLicenses:#,##0}")
                    .Replace("{Total_Maintenance_Fees}", totalMaintenanceFee.ToString())
                    .Replace("{Order_Items}", _itemBuilder.ToString())
                );
            return bodyBuilder.ToString();
        }
    }
}