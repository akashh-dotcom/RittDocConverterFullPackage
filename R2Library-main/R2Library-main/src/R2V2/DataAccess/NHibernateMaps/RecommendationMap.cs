#region

using R2V2.Core.Authentication;
using R2V2.Core.Recommendations;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class RecommendationMap : BaseMap<Recommendation>
    {
        public RecommendationMap()
        {
            Table("dbo.tInstitutionRecommendation");
            Id(x => x.Id, "iRecommendationId").GeneratedBy.Identity();
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.RecommendedByUserId, "iFacultyUserId");
            Map(x => x.AlertSentDate, "dtAlertSentDate");
            Map(x => x.AddedToCartDate, "dtAddedToCartDate");
            Map(x => x.AddedToCartByUserId, "iAddedToCartByUserId");
            Map(x => x.PurchasedByUserId, "iPurchasedByUserId");
            Map(x => x.PurchaseDate, "dtPurchaseDate");
            Map(x => x.DeletedByUserId, "iDeletedByUserId");
            Map(x => x.DeletedDate, "dtDeletedDate");
            Map(x => x.Notes, "vchNotes");

            Map(x => x.DeletedNotes, "vchDeletedNote");

            Map(x => x.RecordStatus).Column("tiRecordStatus");

            References<User>(x => x.RecommendedByUser).Column("iFacultyUserId").ReadOnly();
            References<User>(x => x.AddedToCartByUser).Column("iAddedToCartByUserId").ReadOnly();
            References<User>(x => x.PurchasedByUser).Column("iPurchasedByUserId").ReadOnly();
            References<User>(x => x.DeletedByUser).Column("iDeletedByUserId").ReadOnly();
        }
    }
}