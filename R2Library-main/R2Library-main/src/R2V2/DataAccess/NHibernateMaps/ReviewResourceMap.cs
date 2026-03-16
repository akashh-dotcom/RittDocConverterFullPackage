#region

using R2V2.Core.Authentication;
using R2V2.Core.Recommendations;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReviewResourceMap : BaseMap<ReviewResource>
    {
        public ReviewResourceMap()
        {
            Table("dbo.tInstitutionReviewResource");
            Id(x => x.Id, "iReviewResourceId").GeneratedBy.Identity();
            Map(x => x.ReviewId, "iReviewId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.AddedByUserId, "iAddedByUserId");

            Map(x => x.ActionTypeId, "tiActionTypeId");
            Map(x => x.ActionByUserId, "iActionByUserId");
            Map(x => x.ActionDate, "dtActionDate");

            Map(x => x.DeletedByUserId, "iDeletedByUserId");
            Map(x => x.DeletedDate, "dtDeletedDate");

            Map(x => x.Notes, "vchNotes");
            Map(x => x.RecordStatus).Column("tiRecordStatus");

            References<User>(x => x.AddedByUser).Column("iAddedByUserId").ReadOnly();
            References<User>(x => x.ActionByUser).Column("iActionByUserId").ReadOnly();
            References<User>(x => x.DeletedByUser).Column("iDeletedByUserId").ReadOnly();
        }
    }
}