#region

using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Recommendations
{
    public class ReviewQuery : CollectionManagementQuery, IReviewQuery
    {
        public ReviewQuery()
        {
        }

        public ReviewQuery(IReviewQuery query)
            : base(query)
        {
            InstitutionId = query.InstitutionId;
            ReviewId = query.ReviewId;
        }

        public new bool DefaultQuery =>
            string.IsNullOrWhiteSpace(Query) && string.IsNullOrWhiteSpace(SortBy) &&
            SortDirection == SortDirection.Ascending &&
            ResourceStatus == ResourceStatus.All && ResourceFilterType == ResourceFilterType.All &&
            PracticeAreaFilter <= 0 && SpecialtyFilter <= 0 && CollectionFilter <= 0;
    }
}