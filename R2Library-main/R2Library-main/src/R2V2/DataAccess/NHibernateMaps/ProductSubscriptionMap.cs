#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ProductSubscriptionMap : BaseMap<ProductSubscription>
    {
        public ProductSubscriptionMap()
        {
            Table("tProductSubscription");

            Id(x => x.Id).Column("iProductSubscriptionId").GeneratedBy.Identity();

            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.StartDate, "dtStartDate");
            Map(x => x.EndDate, "dtEndDate");
            Map(x => x.ProductSubscriptionStatusId, "iProductSubscriptionStatusId");

            References(x => x.Product).Column("iProductId").Cascade.None();
        }
    }
}