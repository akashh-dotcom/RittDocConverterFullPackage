#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using ReviewResource = R2V2.Web.Areas.Admin.Models.Review.ReviewResource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public static class InstitutionResourceExtensions
    {
        public static IEnumerable<InstitutionResource> ToInstitutionResources(
            this IEnumerable<CollectionManagementResource> collectionManagementResources, IAdminInstitution institution,
            IEnumerable<Recommendation> recommendations)
        {
            var institutionResources = new List<InstitutionResource>();
            var counter = 1;
            var managementResources = collectionManagementResources as CollectionManagementResource[] ??
                                      collectionManagementResources.ToArray();
            foreach (var institutionResource in managementResources.Select(x =>
                         x.ToInstitutionResource(institution, recommendations)))
            {
                institutionResource.ListIndex = counter;
                institutionResources.Add(institutionResource);
                counter++;
            }

            return institutionResources;
        }

        public static IEnumerable<InstitutionResource> ToInstitutionResources(
            this IEnumerable<CollectionManagementResource> collectionManagementResources, IAdminInstitution institution,
            IEnumerable<Recommendation> recommendations, IUser user)
        {
            var institutionResources = new List<InstitutionResource>();
            var counter = 1;
            var managementResources = collectionManagementResources as CollectionManagementResource[] ??
                                      collectionManagementResources.ToArray();
            foreach (var institutionResource in managementResources.Select(x =>
                         x.ToInstitutionResource(institution, recommendations, user)))
            {
                institutionResource.ListIndex = counter;
                institutionResources.Add(institutionResource);
                counter++;
            }

            return institutionResources;
        }

        public static InstitutionResource ToInstitutionResource(
            this CollectionManagementResource collectionManagementResource, IAdminInstitution institution,
            IEnumerable<Recommendation> recommendations)
        {
            return new InstitutionResource(collectionManagementResource, institution, recommendations);
        }

        public static InstitutionResource ToInstitutionResource(
            this CollectionManagementResource collectionManagementResource, IAdminInstitution institution,
            IEnumerable<Recommendation> recommendations, IUser user)
        {
            var institutionResource =
                new InstitutionResource(collectionManagementResource, institution, recommendations);
            institutionResource.SetRecommendationId(user);
            return institutionResource;
        }

        public static ReviewResource ToReviewResource(
            this CollectionManagementResource collectionManagementResource,
            IAdminInstitution institution, IEnumerable<Recommendation> recommendations, bool isSelected,
            int reviewResourceId)
        {
            return new ReviewResource(collectionManagementResource, institution, recommendations, isSelected,
                reviewResourceId);
        }
    }
}