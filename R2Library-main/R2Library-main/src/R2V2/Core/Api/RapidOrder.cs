#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Api
{
    public class RapidOrder : Entity, IDebugInfo
    {
        public virtual string PoNumber { get; set; }
        public virtual string AccountNumber { get; set; }
        public virtual string Isbn10 { get; set; }
        public virtual string Isbn13 { get; set; }
        public virtual string EIsbn { get; set; }
        public virtual string PoStatus { get; set; }
        public virtual string StatusCode { get; set; }
        public virtual string PurchaseOption { get; set; }
        public virtual decimal ListPrice { get; set; }
        public virtual decimal RequestedPrice { get; set; }
        public virtual int Quantity { get; set; }
        public virtual DateTime CreationDate { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("RapidOrder = [");
            sb.Append($"RapidOrderId: {Id}");
            sb.Append($", PoNumber: {PoNumber}");
            sb.Append($", AccountNumber: {AccountNumber}");
            sb.Append($", Isbn10: {Isbn10}");
            sb.Append($", Isbn13: {Isbn13}");
            sb.Append($", EIsbn: {EIsbn}");
            sb.Append($", PoStatus: {PoStatus}");
            sb.Append($", StatusCode: {StatusCode}");
            sb.Append($", PurchaseOption: {PurchaseOption}");
            sb.Append($", ListPrice: {ListPrice}");
            sb.Append($", RequestedPrice: {RequestedPrice}");
            sb.Append($", Quantity: {Quantity}");
            sb.Append($", CreationDate: {CreationDate}");
            sb.Append("]");
            return sb.ToString();
        }
    }
}