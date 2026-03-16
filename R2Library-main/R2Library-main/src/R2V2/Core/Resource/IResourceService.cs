#region

using System.Collections.Generic;
using R2V2.Core.Publisher;

#endregion

namespace R2V2.Core.Resource
{
    public interface IResourceService
    {
        IResource GetResource(int resourceId);
        Resource GetResourceForEdit(int resourceId);
        Resource GetSoftDeletedResource(int resourceId);
        IResource GetResource(string isbn);
        IEnumerable<IResource> GetAllResources();
        IEnumerable<IResource> GetAllResources(bool forceReload);

        IEnumerable<IResource> GetResources(IResourceQuery resourceQuery, IPublisher featuredPublisher,
            IEnumerable<IFeaturedTitle> allFeaturedTitles, IList<IPublisher> publishers);

        IEnumerable<IResource> GetResources(IEnumerable<IResource> filteredResources, IResourceQuery resourceQuery,
            IPublisher featuredPublisher, IEnumerable<IFeaturedTitle> allFeaturedTitles);

        IEnumerable<IResource> GetResources(IEnumerable<IResource> filteredResources, IResourceQuery resourceQuery,
            IPublisher featuredPublisher, IEnumerable<IFeaturedTitle> allFeaturedTitles, bool includeFurtureSpecials,
            int[] recentResourceIds);

        IList<Resource> GetResources(IEnumerable<int> resourceIds);
        IList<IResource> GetResources(string[] isbns);


        IEnumerable<IResource> GetResources(IResourceQuery resourceQuery, IPublisher featuredPublisher,
            IEnumerable<IFeaturedTitle> allFeaturedTitles, bool includeFurtureSpecials, int[] recentResourceIds,
            IList<IPublisher> publishers);

        IEnumerable<IResource> GetResourcesExcludeIds(IResourceQuery resourceQuery, int[] ids,
            IList<IPublisher> publishers);

        int[] GetResourcePublicationYears();

        void SaveResource(Resource resource);
        void DeleteResource(Resource resource);
        void AddToTransformQueue(IResource resource);
        void AddToTransformQueue(IEnumerable<IResource> resource);

        void UpdateAdminSearchFile(IResource resource);
        string GetCitation(IResource resource, string link);
        string GetProciteCitation(IResource resource, string url);
        string GetEndNoteCitation(IResource resource, string url);
        string GetRefWorksCitation(IResource resource, string url);
        string GetApaFormatCitation(IResource resource, string url);
        string GetApaFormatCitation(IResource resource, string url, string sectionTitle);

        IEnumerable<IResource> GetResourcesByIds(int[] resourceIds);

        string ConvertIsbn10To13(string isbn10);
        bool IsValidateIsbn(string isbn);

        void ValidateAllResourceIsbns();
        IResource GetResourceWithoutDatabase(string isbn);

        List<IResource> GetDuplicateIsbns(string isbn10, string isbn13, string eIsbn, int resourceId);

        List<IResource> GetRecentlyReleasedTitles(int resourceId, int specialtyId, bool isDci);
        void ReloadResourceCache();
        IList<IResource> GetResourcesForOngoingPda(string[] resourceIsbns);
    }
}