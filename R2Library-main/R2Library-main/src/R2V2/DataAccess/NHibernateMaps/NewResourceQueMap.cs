#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class NewResourceQueMap : BaseMap<NewResourceQue>
    {
        public NewResourceQueMap()
        {
            Table("tNewResourceQue");

            Id(x => x.Id).Column("iNewResourceQueId").GeneratedBy.Identity();

            Map(x => x.RecordStatus).Column("tiRecordStatus");
            Map(x => x.Processed).Column("tiProcessed");

            Map(x => x.NewResourceEmailDate).Column("dtNewResourceEmail");
            Map(x => x.NewEditionEmailDate).Column("dtNewEditionEmail");
            Map(x => x.PurchasedEmailDate).Column("dtPurchasedEmail");

            Map(x => x.ResourceId).Column("iResourceId");

            Map(x => x.CreatedBy).Column("vchCreatorId");
            Map(x => x.CreationDate).Column("dtCreationDate");
            Map(x => x.UpdatedBy).Column("vchUpdaterId");
            Map(x => x.LastUpdated).Column("dtLastUpdate");
        }
    }
}