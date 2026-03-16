#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public static class ReviewExtensions
    {
        public static IEnumerable<ReviewModel> ToReviews(this IEnumerable<Core.Recommendations.Review> reviews)
        {
            return reviews.Select(ToReview);
        }

        public static ReviewModel ToReview(this Core.Recommendations.Review review)
        {
            return new ReviewModel
            {
                Id = review.Id,
                Name = review.Name,
                Description = review.Description,
                ResourceCount = review.ReviewResources != null ? review.ReviewResources.Count() : 0
            };
        }
    }
}