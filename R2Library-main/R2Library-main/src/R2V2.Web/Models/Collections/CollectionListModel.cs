#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Collection;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Models.Collections
{
    public class CollectionListModel : BaseModel
    {
        public CollectionListModel()
        {
        }

        public CollectionListModel(IEnumerable<ICollection> collections, ICollection selectedCollection,
            IEnumerable<ResourceSummary> resources)
        {
            CollectionList = collections.Select(x => new CollectionDetail
            {
                CollectionId = x.Id, Name = x.Name, Description = x.Description,
                Selected = selectedCollection.Id == x.Id
            }).ToList();
            SelectedCollection = new CollectionDetail(selectedCollection, resources);
        }

        public CollectionListModel(
            IEnumerable<ICollection> collections
            , ICollection selectedCollection
            , IEnumerable<ResourceSummary> resources
            , Dictionary<int, IPublisher> publisherDictionary
            , Dictionary<int, int> countsDictionary
            , int? publisherId
            , int totalResourceCount)
        {
            CollectionList = collections.Select(x => new CollectionDetail
            {
                CollectionId = x.Id, Name = x.Name, Description = x.Description,
                Selected = selectedCollection.Id == x.Id
            }).ToList();
            SelectedCollection = new CollectionDetail(selectedCollection, resources);
            PublisherFilter = new List<PublisherFilter>();

            foreach (var publisherItem in publisherDictionary)
            {
                var item = new PublisherFilter
                {
                    Id = publisherItem.Key,
                    //Name = $"{publisherItem.Value.DisplayName ?? publisherItem.Value.Name} ({countsDictionary[publisherItem.Key]})",
                    Name = $"{publisherItem.Value.Name} ({countsDictionary[publisherItem.Key]})",
                    Selected = publisherId.HasValue && publisherId.Value == publisherItem.Key
                };
                PublisherFilter.Add(item);
            }

            PublisherFilter = PublisherFilter.OrderBy(x => x.Name).ToList();
            PublisherFilter.Insert(0,
                new PublisherFilter
                    { Id = null, Name = $"All ({totalResourceCount})", Selected = !publisherId.HasValue });
        }

        public List<CollectionDetail> CollectionList { get; set; }
        public CollectionDetail SelectedCollection { get; set; }

        public List<PublisherFilter> PublisherFilter { get; set; }

        //public int CollectionId { get; set; }

        public int? InstitutionId { get; set; }

        //Dictionary<int, IPublisher>
    }

    public class PublisherFilter
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}