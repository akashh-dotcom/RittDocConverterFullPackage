#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface ICollectionManagementQuery : IResourceQuery
    {
        int InstitutionId { get; set; }

        //ResourceListType ResourceListType { get; set; }
        string Isbns { get; set; }
        string AccountNumber { get; set; }
        string Resources { get; set; }

        bool TrialConvert { get; set; }
        bool EulaSigned { get; set; }
        bool PdaEulaSigned { get; set; }

        int CartId { get; set; }

        bool IsPdaProfile { get; set; }

        bool IsReserveShelf { get; set; }
        //bool RecommendationsOnly { get; set; }

        IEnumerable<int> GetResourceIds();
        void SerializeResources(IEnumerable<int> resourceIds);
    }
}