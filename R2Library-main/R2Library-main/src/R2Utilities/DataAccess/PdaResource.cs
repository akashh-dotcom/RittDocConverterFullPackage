#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2Utilities.DataAccess
{
    public class PdaResource : FactoryBase, IDataEntity, IDiscountResource
    {
        public DateTime AddedDate { get; set; }
        public DateTime AddedToCartDate { get; set; }

        public void Populate(SqlDataReader reader)
        {
            ResourceId = GetInt32Value(reader, "resourceId", -1);
            AddedDate = GetDateValue(reader, "addedDate");
            AddedToCartDate = GetDateValue(reader, "AddedToCartDate");
            SpecialText = GetStringValue(reader, "PromotionText");
            DiscountPrice = GetDecimalValue(reader, "DiscountPrice", 0);
            ListPrice = GetDecimalValue(reader, "ListPrice", 0);
        }

        public int? ResourceId { get; set; }
        public string SpecialText { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal BundlePrice { get; set; }
        public bool IsBundle { get; set; }

        //Added for IDiscountResource
        public int? ProductId { get; set; }
        public int CartId { get; set; }
        public string SpecialIconName { get; set; }
        public bool PdaPromotionApplied { get; set; }

        public short OriginalSourceId
        {
            get => 2;
            set => throw new NotImplementedException();
        }

        public int? PdaPromotionId { get; set; }
        public int? SpecialDiscountId { get; set; }

        public decimal Discount { get; set; }

        public decimal ListPrice { get; set; }
    }
}