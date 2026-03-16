#region

using R2V2.Core.Api;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class RapidOrderMap : BaseMap<RapidOrder>
    {
        public RapidOrderMap()
        {
            Table("tRapidOrder");
            Id(x => x.Id).Column("iRapidOrderId").GeneratedBy.Identity();
            Map(x => x.PoNumber).Column("vchPoNumber");
            Map(x => x.AccountNumber).Column("vchAccountNumber");
            Map(x => x.Isbn10).Column("vchIsbn10");
            Map(x => x.Isbn13).Column("vchIsbn13");
            Map(x => x.EIsbn).Column("vchEIsbn");
            Map(x => x.PoStatus).Column("vchPoStatus");
            Map(x => x.StatusCode).Column("vchStatusCode");
            Map(x => x.PurchaseOption).Column("vchPurchaseOption");
            Map(x => x.ListPrice).Column("decListPrice");
            Map(x => x.RequestedPrice).Column("decRequestedPrice");
            Map(x => x.Quantity).Column("iQuantity");
            Map(x => x.CreationDate).Column("dtCreationDate");
        }
    }
}