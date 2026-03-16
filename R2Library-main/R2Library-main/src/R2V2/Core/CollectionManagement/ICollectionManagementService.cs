#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface ICollectionManagementService
    {
        CollectionManagementResource
            GetCollectionManagementResource(int institutionId, int resourceId, CachedCart cart);

        CollectionManagementResource GetCollectionManagementResource(int institutionId, string isbn, CachedCart cart);

        CollectionManagementResource GetCollectionManagementResourceWithoutDatabase(int institutionId, string isbn,
            CachedCart cart);

        //CollectionManagementResource GetCollectionManagementProduct(int institutionId, Product product, CachedCart cart);

        IEnumerable<CollectionManagementResource> GetCollectionManagementResources(
            ICollectionManagementQuery collectionManagementQuery, IOrder order, bool isExpertReviewer = false);

        IEnumerable<CollectionManagementResource> GetCollectionManagementResourcesExcludeIds(
            ICollectionManagementQuery collectionManagementQuery, IOrder order, int[] resourceIds);

        bool UpdateInstitutionResourceLicenses(int institutionId, int resourceId, int numberOfLicenses);

        // bool UpdateLicensesNotSaleable(IResource resource, DateTime? notSaleableDate);

        bool UpdateFreeLicenses(int resourceId, bool isFreeResource);

        IEnumerable<CollectionManagementResource> GetCollectionManagementResources(
            IEnumerable<IResource> filteredResources, ICollectionManagementQuery collectionManagementQuery,
            IOrder order, bool isExpertReviewer = false);
    }
}