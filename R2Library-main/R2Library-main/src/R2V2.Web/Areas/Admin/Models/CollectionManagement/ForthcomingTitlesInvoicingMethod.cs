#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class ForthcomingTitlesInvoicingMethod
    {
        public static ForthcomingTitlesInvoicingMethod InvoiceNow = new ForthcomingTitlesInvoicingMethod
            { Id = ForthcomingTitlesInvoicingMethodEnum.InvoiceNow, Description = "Invoice Now" };

        public static ForthcomingTitlesInvoicingMethod InvoiceWhenReleased = new ForthcomingTitlesInvoicingMethod
            { Id = ForthcomingTitlesInvoicingMethodEnum.InvoiceWhenReleased, Description = "Invoice When Released" };

        public ForthcomingTitlesInvoicingMethodEnum Id { get; protected set; }

        public string Description { get; protected set; }
    }
}