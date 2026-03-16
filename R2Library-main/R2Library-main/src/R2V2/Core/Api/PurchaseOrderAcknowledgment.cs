#region

using System.Linq;
using System.Xml.Serialization;

#endregion

namespace R2V2.Core.Api.YBP
{
    [XmlRoot("PurchaseOrderAcknowledgment")]
    public class PurchaseOrderAcknowledgment
    {
        public PurchaseOrderAcknowledgment()
        {
        }

        public PurchaseOrderAcknowledgment(PurchaseOrder order, string statusCode)
        {
            PoNumber = order.PoNumber;
            StatusCode = statusCode;
            PoStatus = "IR";
            PurchaseOption = "1U";
            CurrencyCode = "USD";
            Price = order.ItemList.FirstOrDefault().Price;
            DiscountAmount = "0";
            DiscountPercent = "0";
        }

        [XmlElement("PoNumber")] public string PoNumber { get; set; }
        [XmlElement("PoStatus")] public string PoStatus { get; set; }
        [XmlElement("StatusCode")] public string StatusCode { get; set; }
        [XmlElement("PurchaseOption")] public string PurchaseOption { get; set; }
        [XmlElement("Price")] public string Price { get; set; }
        [XmlElement("CurrencyCode")] public string CurrencyCode { get; set; }
        [XmlElement("DiscountPercent")] public string DiscountPercent { get; set; }
        [XmlElement("DiscountAmount")] public string DiscountAmount { get; set; }
    }
}