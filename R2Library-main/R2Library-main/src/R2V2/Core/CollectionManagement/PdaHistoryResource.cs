#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class PdaHistoryResource : CollectionManagementResource
    {
        public PdaHistoryResource(IResource resource) : base(resource, 0)
        {
        }

        public string DateOrNameCartWasSaved { get; set; }
    }
}