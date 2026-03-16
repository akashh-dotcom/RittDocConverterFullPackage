#region

using System.Collections.Generic;
using System.Linq;
using R2Utilities.DataAccess;
using R2V2.Core.Resource;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class IndexedResource
    {
        public IndexedResource(ResourceCore resource, IDictionary<string, DocIds> docIdsList,
            IList<ResourceDocIds> allResourceDocIds, ResourceContentStatus resourceContentStatus, bool forceTransform)
        {
            Resource = resource;
            ResourceStatus = (ResourceStatus)resource.StatusId;
            SoftDeleted = resource.RecordStatus == 0;
            IndexDocIds = docIdsList[resource.Isbn];
            ResourceDocIds = allResourceDocIds.SingleOrDefault(x => x.Id == resource.Id);

            //Unknown,
            //Exception,
            //Ok,
            //XmlAndHtmlOk,
            //HtmlFilesNotInIndex,
            //MissingHtmlGlossaryFiles,


            // is resource active or archived
            if ((ResourceStatus == ResourceStatus.Active || ResourceStatus == ResourceStatus.Archived) && !SoftDeleted)
            {
                if (resourceContentStatus.Status == ResourceContentStatusType.XmlDirectoryDoesNotExist ||
                    resourceContentStatus.Status == ResourceContentStatusType.XmlDirectoryIsEmpty)
                {
                    IndexedResourceStatus = IndexedResourceStatus.XmlDirectoryMissingOrEmpty;
                }

                if (resourceContentStatus.Status == ResourceContentStatusType.MissingXmlFiles)
                {
                    // this indicates that there are more HTML file than XML so retransform
                    AddToTransformQueue = true;
                    IndexedResourceStatus = IndexedResourceStatus.XmlFilesMissing;
                }

                ;

                if (resourceContentStatus.Status == ResourceContentStatusType.HtmlDirectoryDoesNotExist ||
                    resourceContentStatus.Status == ResourceContentStatusType.HtmlDirectoryIsEmpty ||
                    resourceContentStatus.Status == ResourceContentStatusType.MissingHtmlFiles ||
                    resourceContentStatus.Status == ResourceContentStatusType.MissingHtmlGlossaryFiles)
                {
                    AddToTransformQueue = true;
                    IndexedResourceStatus = IndexedResourceStatus.HtmlFilesMissing;
                }

                if (resourceContentStatus.Status == ResourceContentStatusType.IndexContainsMissingFiles)
                {
                    DeleteMissingDocIdsFromIndex = true;
                    AddToTransformQueue = forceTransform;
                    IndexedResourceStatus = IndexedResourceStatus.IndexContainsMissingFiles;
                }
            }
            else
            {
                DeleteAllDocIdsFromDb = ResourceDocIds != null && ResourceDocIds.MaxDocId > 0;
                DeleteAllDocIdsFromIndex = IndexDocIds != null && IndexDocIds.Filenames.Count > 0;
                IndexedResourceStatus = IndexedResourceStatus.ResourceIsInactive;
            }
        }

        public ResourceCore Resource { get; set; }
        public DocIds IndexDocIds { get; set; }
        public ResourceDocIds ResourceDocIds { get; set; }
        public IndexedResourceStatus IndexedResourceStatus { get; set; }
        public ResourceStatus ResourceStatus { get; set; }
        public bool SoftDeleted { get; set; }

        // actions
        private bool DeleteAllDocIdsFromDb { get; set; }
        private bool DeleteAllDocIdsFromIndex { get; set; }
        private bool DeleteMissingDocIdsFromDb { get; set; }
        private bool DeleteMissingDocIdsFromIndex { get; set; }
        private bool AddToTransformQueue { get; set; }
        private bool AddToIndexQueue { get; set; }

        public string ActionDescription
        {
            get
            {
                switch (IndexedResourceStatus)
                {
                    case IndexedResourceStatus.NotAction:
                        return $"{Resource.Isbn} - NO ACTION REQUIRED";
                    case IndexedResourceStatus.ResourceIsInactive:
                        return
                            $"{Resource.Isbn} - RESOURCE IS INACTIVE, ResourceStatus: {ResourceStatus}, SoftDeleted: {SoftDeleted}";
                }

                return null;
            }
        }
    }

    public enum IndexedResourceStatus
    {
        NotAction,
        ResourceIsInactive,
        XmlDirectoryMissingOrEmpty,
        XmlFilesMissing,
        HtmlFilesMissing,
        IndexContainsMissingFiles
    }
}