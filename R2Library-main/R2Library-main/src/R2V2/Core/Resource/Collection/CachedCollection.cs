#region

using System;
using Newtonsoft.Json;

#endregion

namespace R2V2.Core.Resource.Collection
{
    [Serializable]
    public class CachedCollection : ICollection
    {
        public CachedCollection(ICollection collection)
        {
            Id = collection.Id;
            Name = collection.Name;
            HideInFilter = collection.HideInFilter;
            Sequence = collection.Sequence;
            IsSpecialCollection = collection.IsSpecialCollection;
            SpecialCollectionSequence = collection.SpecialCollectionSequence;
            Description = collection.Description;
            IsPublic = collection.IsPublic;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool HideInFilter { get; set; }
        public int Sequence { get; set; }
        public bool IsSpecialCollection { get; set; }
        public int SpecialCollectionSequence { get; set; }
        public string Description { get; set; }

        public bool IsPublic { get; set; }

        public string ToDebugString()
        {
            return $"CachedCollection = {ToJsonString()}";
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}