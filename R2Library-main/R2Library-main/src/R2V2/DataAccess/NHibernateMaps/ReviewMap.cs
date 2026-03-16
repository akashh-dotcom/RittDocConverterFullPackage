#region

using R2V2.Core.Recommendations;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReviewMap : BaseMap<Review>
    {
        public ReviewMap()
        {
            Table("dbo.tInstitutionReview");
            Id(x => x.Id, "iReviewId").GeneratedBy.Identity();
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.CreatedByUserId, "iCreatedByUserId");
            Map(x => x.Name, "vchReviewName");
            Map(x => x.Description, "vchReviewDescription");
            Map(x => x.DeletedByUserId, "iDeletedByUserId");
            Map(x => x.DeletedDate, "dtDeletedDate");

            Map(x => x.RecordStatus).Column("tiRecordStatus");

            HasMany(x => x.ReviewResources).KeyColumn("iReviewId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.ReviewUsers).KeyColumn("iReviewId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}