#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource.Collection
{
    public interface ICollectionService
    {
        IEnumerable<ICollection> GetAllCollections();
        ICollection GetCollectionById(int collectionId);
        ICollection GetCollectionById(string collectionId);

        Collection GetCollection(int collectionId);

        List<ICollection> GetCollectionLists();
        void UpdateCollection(ICollection editedCollection);
        void DeleteSpecialCollection(int collectionId);
        void RemoveResourceFromCollection(int collectionId, int resourceId);
        void BulkAddResourcesToSpecialCollection(int collectionId, int[] resourceIds);
        int AddCollection(string collectionName);
        void SaveCollectionListSequence(int[] orderedSequence);
        void ClearCache();

        ICollection GetPublicCollection();
        List<ICollection> GetAllPublicCollections();
    }
}