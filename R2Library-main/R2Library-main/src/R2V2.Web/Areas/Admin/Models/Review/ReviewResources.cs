#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewResources : InstitutionResources
    {
        public ReviewResources()
        {
        }

        public ReviewResources(IAdminInstitution institution, ReviewQuery reviewQuery,
            IEnumerable<ReviewResource> reviewResources, Core.Recommendations.Review review,
            IPracticeAreaService practiceAreaService, ISpecialtyService specialtyService,
            ICollectionService collectionService, string doodyReviewUrl, string specialIconBaseUrl)
            : base(institution, reviewQuery, null, practiceAreaService, specialtyService, collectionService,
                doodyReviewUrl, specialIconBaseUrl)
        {
            ReviewQuery = reviewQuery;
            Review = review;
            Resources = reviewResources;
            //Need to set the review Id for BaseModel so we can search.
            ReviewId = review.Id;
            SelectedFilters = CollectionManagementQuery.ToSelectedFilters(practiceAreaService, specialtyService,
                collectionService, GetSortByDescription(reviewQuery.SortBy));
        }

        public Core.Recommendations.Review Review { get; set; }
        public ReviewQuery ReviewQuery { get; set; }

        public IEnumerable<ReviewResource> Resources { get; set; }
    }
}