#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public class VideoSectionField : SearchFieldBase
    {
        public VideoSectionField()
            : base("video-section", SearchFields.VideoSection, SearchType.Video)
        {
        }
    }
}