#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface ICartItem : IDebugInfo
    {
        int Id { get; set; }

        int CartId { get; set; }

        //ICart Cart { get; set; }
        int? ResourceId { get; set; }
        IProduct Product { get; set; }
        int? ProductId { get; set; }
        int NumberOfLicenses { get; set; }
        decimal ListPrice { get; set; }
        decimal DiscountPrice { get; set; }
        DateTime? PurchaseDate { get; set; }
        bool Include { get; set; }
        bool Agree { get; set; }
        short OriginalSourceId { get; set; }
        DateTime CreationDate { get; set; }

        string SpecialText { get; set; }
        string SpecialIconName { get; set; }

        bool PdaPromotionApplied { get; set; }

        DateTime? AddedByNewEditionDate { get; set; }

        List<string> AutomatedReasonCodes { get; set; }

        bool IsBundle { get; set; }
        //void SetDiscount(CachedSpecialResource specialDiscountResource, PdaPromotion pdaPromotion, decimal promotionDiscount, decimal cartDiscount);
        //void SetDiscount(int[] productIds, decimal promotionDiscount);
    }
}