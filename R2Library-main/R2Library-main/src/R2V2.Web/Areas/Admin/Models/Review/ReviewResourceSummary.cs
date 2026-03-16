#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Recommendations;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewResourceSummary
    {
        public ReviewResourceSummary()
        {
        }

        public ReviewResourceSummary(ResourceSummary resourceSummary, int reviewResourceId)
        {
            ResourceSummary = resourceSummary;
            ReviewResourceId = reviewResourceId;
        }

        public ResourceSummary ResourceSummary { get; set; }
        public int ReviewResourceId { get; set; }

        public static IEnumerable<ReviewResourceSummary> ToReviewResourceSummaries(
            IEnumerable<ResourceSummary> resourceSummaries, IReview review)
        {
            IList<ReviewResourceSummary> reviewResourceSummaries = (from resourceSummary in resourceSummaries
                let reviewResouce =
                    review.ReviewResources.FirstOrDefault(x => x.ResourceId == resourceSummary.Id)
                select
                    new ReviewResourceSummary(resourceSummary,
                        (IReviewResource)reviewResouce == null
                            ? 0
                            : ((IReviewResource)reviewResouce).Id)).ToList();
            return reviewResourceSummaries;
        }
    }
}