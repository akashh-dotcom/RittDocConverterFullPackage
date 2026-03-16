#region

using R2V2.Core.Promotion;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourcePromoteQueueMap : BaseMap<ResourcePromoteQueue>
    {
        public ResourcePromoteQueueMap()
        {
            Table("tResourcePromoteQueue");

            Id(x => x.Id).Column("iResourcePromoteQueueId").GeneratedBy.Identity();
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.Isbn, "vchIsbn");
            Map(x => x.PromoteBatchName, "vchPromoteBatchName");
            Map(x => x.PromoteInitDate, "dtPromoteInitDate");
            Map(x => x.PromoteStatus, "vchPromoteStatus");
            Map(x => x.AddedByUserId, "iAddedByUserId");
            Map(x => x.PromotedByUserId, "iPromotedByUserId");
            Map(x => x.BatchKey, "guidBatchKey");
        }
    }
}