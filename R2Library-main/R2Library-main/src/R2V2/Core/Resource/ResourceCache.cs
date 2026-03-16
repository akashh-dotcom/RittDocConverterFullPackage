#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Author;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Resource
{
    [Serializable]
    public class ResourceCache
    {
        private readonly IList<CachedResource> _resources;
        private readonly Dictionary<int, CachedResource> _resourcesById;
        private readonly Dictionary<string, CachedResource> _resourcesByIsbn;

        public ResourceCache(Dictionary<int, Resource> allResources
            , Dictionary<int, ResourceFileDocIds> resourceFileDocIds
            , IContentSettings contentSettings
            , IDictionary<int, IPublisher> publishers
            , ILookup<int, IAuthor> authors
            , ILookup<int?, ResourcePracticeArea> resourcePracticeAreas
            , ILookup<int?, ResourceSpecialty> resourceSpecialties
            , ILookup<int?, ResourceCollection> resourceCollections
        )
        {
            _resources = new List<CachedResource>();
            _resourcesById = new Dictionary<int, CachedResource>();
            _resourcesByIsbn = new Dictionary<string, CachedResource>();
            var resources = allResources.Values.ToList();

            foreach (var allResource in resources)
            {
                var cachedResource = new CachedResource(allResource, allResources, publishers, authors,
                    resourcePracticeAreas, resourceSpecialties
                    , resourceFileDocIds, contentSettings.BookCoverUrl, contentSettings.ResourceMinimumListPrice,
                    resourceCollections);

                _resources.Add(cachedResource);

                _resourcesById.Add(cachedResource.Id, cachedResource);

                if (!_resourcesByIsbn.ContainsKey(cachedResource.Isbn))
                {
                    _resourcesByIsbn.Add(cachedResource.Isbn, cachedResource);
                }

                if (!string.IsNullOrWhiteSpace(cachedResource.Isbn10) &&
                    !_resourcesByIsbn.ContainsKey(cachedResource.Isbn10))
                {
                    _resourcesByIsbn.Add(cachedResource.Isbn10, cachedResource);
                }

                if (!string.IsNullOrWhiteSpace(cachedResource.Isbn13) &&
                    !_resourcesByIsbn.ContainsKey(cachedResource.Isbn13))
                {
                    _resourcesByIsbn.Add(cachedResource.Isbn13, cachedResource);
                }

                if (!string.IsNullOrWhiteSpace(cachedResource.EIsbn) &&
                    !_resourcesByIsbn.ContainsKey(cachedResource.EIsbn))
                {
                    _resourcesByIsbn.Add(cachedResource.EIsbn, cachedResource);
                }
            }
        }

        public IEnumerable<IResource> GetAllResources()
        {
            return _resources;
        }

        public IEnumerable<IResource> GetAllNewEditionResources()
        {
            return _resources.Where(x => x.NewEditionResourceIsbn != null);
        }

        public IResource GetResourceById(int id)
        {
            if (_resourcesById.ContainsKey(id))
            {
                return _resourcesById[id];
            }

            return null;
        }


        public IResource GetResourceByIsbn(string isbn)
        {
            if (_resourcesByIsbn.ContainsKey(isbn))
            {
                return _resourcesByIsbn[isbn];
            }

            return null;
        }

        public void Clear()
        {
            foreach (var cachedResource in _resources)
            {
                cachedResource.ClearLists();
            }

            _resources.Clear();
            _resourcesById.Clear();
            _resourcesByIsbn.Clear();
        }
    }
}