#region

using System;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public static class ForthcomingTitlesInvoicingMethodExtensions
    {
        public static ForthcomingTitlesInvoicingMethod ToForthcomingTitlesInvoicingMethod(
            this ForthcomingTitlesInvoicingMethodEnum forthcomingTitlesInvoicingMethod)
        {
            switch (forthcomingTitlesInvoicingMethod)
            {
                case ForthcomingTitlesInvoicingMethodEnum.InvoiceNow:
                    return ForthcomingTitlesInvoicingMethod.InvoiceNow;

                case ForthcomingTitlesInvoicingMethodEnum.InvoiceWhenReleased:
                    return ForthcomingTitlesInvoicingMethod.InvoiceWhenReleased;

                default:
                    throw new ArgumentOutOfRangeException("forthcomingTitlesInvoicingMethod");
            }
        }
    }
}