namespace R2V2.Core.CollectionManagement
{
    public interface IDiscountResource
    {
        int? ResourceId { get; set; }
        int? ProductId { get; set; }

        int CartId { get; set; }

        decimal BundlePrice { get; set; }
        bool IsBundle { get; set; }
        decimal DiscountPrice { get; set; }
        decimal ListPrice { get; set; }
        decimal Discount { get; set; }
        string SpecialText { get; set; }
        string SpecialIconName { get; set; }
        bool PdaPromotionApplied { get; set; }
        short OriginalSourceId { get; set; }

        int? PdaPromotionId { get; set; }
        int? SpecialDiscountId { get; set; }
    }
}