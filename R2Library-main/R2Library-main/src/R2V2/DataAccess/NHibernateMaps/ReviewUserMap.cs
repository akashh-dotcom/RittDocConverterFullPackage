#region

using R2V2.Core.Authentication;
using R2V2.Core.Recommendations;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReviewUserMap : BaseMap<ReviewUser>
    {
        public ReviewUserMap()
        {
            Table("dbo.tInstitutionReviewUser");
            Id(x => x.Id, "iReviewUserId").GeneratedBy.Identity();
            Map(x => x.ReviewId, "iReviewId");
            Map(x => x.UserId, "iUserId");
            Map(x => x.AddedByUserId, "iAddedByUserId");
            Map(x => x.LastAlertDate, "dtLastAlertDate");
            Map(x => x.DeletedByUserId, "iDeletedByUserId");
            Map(x => x.DeletedDate, "dtDeletedDate");
            Map(x => x.RecordStatus).Column("tiRecordStatus");
            References<User>(x => x.User).Column("iUserId").ReadOnly().Cascade.None();
        }
    }
}