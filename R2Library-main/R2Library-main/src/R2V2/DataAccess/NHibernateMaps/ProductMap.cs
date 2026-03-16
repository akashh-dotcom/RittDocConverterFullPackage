#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ProductMap : BaseMap<Product>
    {
        public ProductMap()
        {
            Table("tProduct");

            Id(x => x.Id).Column("iProductId").GeneratedBy.Identity();

            Map(x => x.Name, "vchProductName");
            Map(x => x.Price, "decPrice");
            Map(x => x.Optional, "tiOptional");
        }
    }
}