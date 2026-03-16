#region

using R2V2.Core.Resource.Content;

#endregion

namespace R2V2.Web.Models.Resource
{
    public class ResourceEmail : BaseModel
    {
        public string Title { get; set; }

        public ContentItem ContentItem { get; set; }

        public string Citation { get; set; }

        public string ContentProvider { get; set; }

        public string ContentProviderDisplay()
        {
            return string.IsNullOrWhiteSpace(ContentProvider)
                ? ""
                : $"Content Provided by {ContentProvider}";
        }
    }
}