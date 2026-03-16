#region

using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace R2V2.Core.Api
{
    [XmlRoot("PurchaseOrder")]
    public class PurchaseOrder
    {
        [XmlElement("PoNumber")] public string PoNumber { get; set; }
        [XmlElement("OrderType")] public string OrderType { get; set; }
        [XmlElement("Item")] public List<Item> ItemList = new List<Item>();
        [XmlElement("VendorId")] public string VendorId { get; set; }
        [XmlElement("CustomerId")] public string CustomerId { get; set; }
    }

    public class Item
    {
        public string ItemId { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string CurrencyCode { get; set; }
        public string PurchaseOption { get; set; }
    }

    public class ApiTest
    {
        [XmlElement("Success")] public bool Success { get; set; }
        [XmlElement("Message")] public string Message { get; set; }
        [XmlElement("Timestamp")] public string Timestamp { get; set; }
    }

    public class ApiError
    {
        [XmlElement("Success")] public bool Success { get; set; }
        [XmlElement("Message")] public string Message { get; set; }
        [XmlElement("Timestamp")] public string Timestamp { get; set; }
    }
}