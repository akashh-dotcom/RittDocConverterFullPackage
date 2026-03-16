#region

using R2Utilities.DataAccess;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class InvalidDocIds
    {
        public InvalidDocIds()
        {
        }

        public InvalidDocIds(ResourceCore resource, InvalidReasonId invalidReasonId, string invalidReason,
            DocIds indexDocIds = null, ResourceDocIds resourceDocIds = null)
        {
            Resource = resource;
            IndexDocIds = indexDocIds;
            ResourceDocIds = resourceDocIds;
            InvalidReasonId = invalidReasonId;
            InvalidReason = invalidReason;
        }

        public ResourceCore Resource { get; set; }
        public DocIds IndexDocIds { get; set; }
        public ResourceDocIds ResourceDocIds { get; set; }
        public InvalidReasonId InvalidReasonId { get; set; }
        public string InvalidReason { get; set; }
    }

    public enum InvalidReasonId
    {
        NotDefined = -1,
        ResourceNotInIndex = 0,
        ResourceDocIdsNotInDatabase = 1,
        ResourceDocIdsDiffer = 2,
        XmlFilesMissingForResource = 3,
        HtmlFilesMissingForResource = 4,
        HtmlFilesNotInIndex = 5,
        IndexContainsMissingFiles = 6,
        IndexContainsResourceWithInvalidPath = 7
    }
}