#region

using R2V2.Core.Publisher;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PublisherMap : BaseMap<Publisher>
    {
        public PublisherMap()
        {
            Table("tPublisher");

            Id(x => x.Id).Column("iPublisherId").GeneratedBy.Identity();
            Map(x => x.Name).Column("vchPublisherName");
            Map(x => x.Address).Column("vchPublisherAddr1");
            Map(x => x.Address2).Column("vchPublisherAddr2");
            Map(x => x.City).Column("vchPublisherCity");
            Map(x => x.State).Column("vchPublisherState");
            Map(x => x.Zip).Column("vchPublisherZip");
            Map(x => x.RecordStatus).Column("tiRecordStatus");
            Map(x => x.IsFeaturedPublisher).Column("tiFeaturedPublisher");
            Map(x => x.ImageFileName).Column("vchFeaturedImageName");
            Map(x => x.DisplayName).Column("vchFeaturedDisplayName");
            Map(x => x.Description).Column("vchFeaturedDescription").CustomType("StringClob")
                .CustomSqlType("nvarchar(max)");
            Map(x => x.ProductDescription).Column("vchProductStatement");
            Map(x => x.NotSaleableDate).Column("dtNotSaleableDate");
            Map(x => x.VendorNumber).Column("vchVendorNumber");


            References<Publisher>(x => x.ConsolidatedPublisher).Column("iConsolidatedPublisherId");
        }
    }
}