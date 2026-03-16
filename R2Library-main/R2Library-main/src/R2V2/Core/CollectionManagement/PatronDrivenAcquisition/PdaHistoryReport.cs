#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaHistoryReport
    {
        public int InstitutionId { get; set; }
        public List<PdaHistoryCount> PdaHistoryCounts { get; set; }
    }

    public class PdaHistoryCount
    {
        public int ResourceId { get; set; }
        public int ContentRetrievalCount { get; set; }
        public int TocRetrievalCount { get; set; }
        public int SessionCount { get; set; }
        public int PrintCount { get; set; }
        public int EmailCount { get; set; }
        public int AccessTurnawayCount { get; set; }

        public PdaHistoryResource CollectionManagementResource { get; private set; }

        public void SetResource(PdaHistoryResource collectionManagementResource)
        {
            CollectionManagementResource = collectionManagementResource;
        }
    }
}