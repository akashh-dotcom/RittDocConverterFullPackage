#region

using R2V2.Core.R2Utilities;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class TransformQueueMap : BaseMap<TransformQueue>
    {
        public TransformQueueMap()
        {
            Table("R2Utilities.dbo.TransformQueue");

            Id(x => x.Id).Column("transformQueueId").GeneratedBy.Identity();
            Map(x => x.ResourceId).Column("resourceId");
            Map(x => x.Isbn).Column("isbn");
            Map(x => x.Status).Column("status");
            Map(x => x.DateAdded).Column("dateAdded");
            Map(x => x.DateStarted).Column("dateStarted");
            Map(x => x.DateFinished).Column("dateFinished");
            Map(x => x.StatusMessage).Column("statusMessage");
        }
    }
}