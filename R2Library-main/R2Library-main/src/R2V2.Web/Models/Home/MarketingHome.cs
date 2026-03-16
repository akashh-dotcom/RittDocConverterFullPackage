#region

using System.Collections.Generic;
using R2V2.Core.Cms;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Models.Home
{
    public class MarketingHome : BaseModel
    {
        public IFeaturedTitle FeaturedTitle { get; set; }

        public IEnumerable<IResource> RecentResources { get; set; }

        public int FeaturedPublisherId { get; set; }
        public string FeaturedPublisherName { get; set; }
        public string FeaturedPublisherLogo { get; set; }
        public string FeaturedPublisherUrl { get; set; }
        public string FeaturedPublisherDescription { get; set; }

        public R2LibraryCarousel R2LibraryCarousel { get; set; }

        public string HomeIntro { get; set; }
        public string HomePromoTop { get; set; }
        public string HomeMainContent { get; set; }
        public string HomeQuestions { get; set; }
        public string HomePromoBottom { get; set; }


        public string HtmlContent { get; set; }

        public bool HasCarousel()
        {
            return R2LibraryCarousel != null && R2LibraryCarousel.Items != null && R2LibraryCarousel.Items.Count > 0;
        }
    }
}