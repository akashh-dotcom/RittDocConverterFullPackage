#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResellerMap : BaseMap<Reseller>
    {
        public ResellerMap()
        {
            Table("tCartReseller");

            Id(x => x.Id).Column("iResellerId").GeneratedBy.Identity();
            Map(x => x.Name, "vchResellerName");
            Map(x => x.DisplayName, "vchResellerDisplayName");
            Map(x => x.Discount, "decDiscount");
            Map(x => x.AccountNumberOverride, "vchAccountNumberOverride");
        }
    }
}