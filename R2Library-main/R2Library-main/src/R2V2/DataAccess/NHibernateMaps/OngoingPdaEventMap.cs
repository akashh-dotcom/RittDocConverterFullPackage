#region

using R2V2.Core.Promotion;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class OngoingPdaEventMap : BaseMap<OngoingPdaEvent>
    {
        public OngoingPdaEventMap()
        {
            Table("tOngoingPdaEvent");

            //Id(x => x.Id).Column("iOngoingPdaEventId");
            Id(x => x.Id).Column("iOngoingPdaEventId").GeneratedBy.Identity();
            Map(x => x.TransactionId).Column("guidTransactionId");
            Map(x => x.EventTypeId).Column("iOngoingPdaEventTypeId");
            Map(x => x.Processed).Column("tiProcessed");
            Map(x => x.LicenseCountAdded).Column("iLicenseCountAdded");
            //Map(x => x.ProcessData).Column("vchProcessData");
            Map(x => x.ProcessData).Column("vchProcessData").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            HasMany(x => x.Resources).KeyColumn("iOngoingPdaEventId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            //HasMany(x => x.CartItems).KeyColumn("iCartId").AsBag().Inverse().Cascade.AllDeleteOrphan().ApplyFilter<SoftDeleteFilter>();
        }
    }
}