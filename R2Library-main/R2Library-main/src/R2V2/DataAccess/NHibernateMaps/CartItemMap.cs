#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class CartItemMap : BaseMap<CartItem>
    {
        public CartItemMap()
        {
            Table("tCartItem");

            Id(x => x.Id).Column("iCartItemId").GeneratedBy.Identity();

            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.ProductId, "iProductId");
            Map(x => x.NumberOfLicenses, "iNumberOfLicenses");
            Map(x => x.ListPrice, "decListPrice");
            Map(x => x.DiscountPrice, "decDiscountPrice");
            Map(x => x.PurchaseDate, "dtPurchaseDate");
            Map(x => x.Include, "tiInclude");
            Map(x => x.Agree, "tiAgree");
            Map(x => x.OriginalSourceId, "tiLicenseOriginalSourceId");
            Map(x => x.SpecialText, "vchSpecialText");
            Map(x => x.SpecialIconName, "vchSpecialIconName");
            Map(x => x.AddedByNewEditionDate, "dtAddedByNewEdition");
            Map(x => x.IsBundle, "tiBundle");
            Map(x => x.BundlePrice, "decBundlePrice");

            References(x => x.Cart).Column("iCartId");
            References<Product>(x => x.Product).Column("iProductId").ReadOnly().Cascade.None();
        }
    }
}