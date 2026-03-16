#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Search
{
    public class SearchResource
    {
        public IResource Resource { get; set; }
        public bool FullTextAvailable { get; set; }
    }
}