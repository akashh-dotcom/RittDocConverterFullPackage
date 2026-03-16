#region

using System;
using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CollectionManagementQuery : ResourceQuery, ICollectionManagementQuery
    {
        public CollectionManagementQuery()
        {
        }

        public CollectionManagementQuery(ICollectionManagementQuery collectionManagementQuery)
            : base(collectionManagementQuery)
        {
            InstitutionId = collectionManagementQuery.InstitutionId;
            ResourceListType = collectionManagementQuery.ResourceListType;
            RecommendationsOnly = collectionManagementQuery.RecommendationsOnly;

            if (collectionManagementQuery.DateRangeStart != DateTime.MinValue &&
                collectionManagementQuery.DateRangeEnd != DateTime.MinValue)
            {
                DateRangeStart = collectionManagementQuery.DateRangeStart;
                DateRangeEnd = collectionManagementQuery.DateRangeEnd;
            }
        }

        public int InstitutionId { get; set; }

        public int CartId { get; set; }

        public string AccountNumber { get; set; }

        public string Resources { get; set; }

        public IEnumerable<int> GetResourceIds()
        {
            var resourceIds = string.IsNullOrWhiteSpace(Resources)
                ? new string[0]
                : Resources.Split(',');

            var ids = new List<int>();
            foreach (var id in resourceIds)
            {
                int.TryParse(id, out var resourceId);
                ids.Add(resourceId);
            }

            return ids;
        }

        public void SerializeResources(IEnumerable<int> resourceIds)
        {
            Resources = string.Join(",", resourceIds);
        }

        public string Isbns { get; set; }

        //public bool RecommendationsOnly { get; set; }

        public bool TrialConvert { get; set; }
        public bool EulaSigned { get; set; }
        public bool PdaEulaSigned { get; set; }

        public bool IsPdaProfile { get; set; }
        public bool IsReserveShelf { get; set; }
    }
}