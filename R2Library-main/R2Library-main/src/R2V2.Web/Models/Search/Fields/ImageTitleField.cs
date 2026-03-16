#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class ImageTitleField : SearchFieldBase
    {
        public ImageTitleField()
            : base("image-title", SearchFields.ImageTitle, SearchType.Image)
        {
        }
    }
}