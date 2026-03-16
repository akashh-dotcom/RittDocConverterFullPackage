#region

using System;
using R2V2.Core.MyR2;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Models.MyR2
{
    [Serializable]
    public class UserContentItem
    {
        private string _imageUrl;

        public UserContentItem()
        {
        }

        public UserContentItem(Core.MyR2.UserContentItem userContentItem, UserContentType userContentType)
        {
            Id = userContentItem.Id;
            FolderId = userContentItem.UserContentFolder.Id;

            Title = userContentItem.Title;
            SectionTitle = userContentItem.ChapterSectionTitle;
            ResourceId = userContentItem.ResourceId;
            Resource = userContentItem.Resource.ToResourceDetail();
            SectionId = userContentItem.SectionId;
            Library = userContentItem.Library;
            Isbn = userContentItem.Isbn;
            Filename = userContentItem.Filename;

            Type = userContentType;

            CreationDate = userContentItem.CreationDate;
        }

        public int Id { get; set; }
        public int FolderId { get; set; }

        public string Title { get; set; }
        public string SectionTitle { get; set; }
        public int ResourceId { get; set; }
        public ResourceDetail Resource { get; set; }
        public string SectionId { get; set; }
        public string Library { get; set; }
        public string Isbn { get; set; }
        public string Filename { get; set; }

        public UserContentType Type { get; set; }

        public DateTime CreationDate { get; set; }

        public string CourseLinkUrl { get; set; }

        public string ImageUrl
        {
            get =>
                string.IsNullOrWhiteSpace(_imageUrl)
                    ? $"/{Isbn}/{Filename}"
                    : _imageUrl;
            set => _imageUrl = value;
        }

        public void SetCourseLinksUrl(string url, AuthenticatedInstitution institution)
        {
            CourseLinkUrl = url;
            if (institution == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(institution.ProxyPrefix))
            {
                CourseLinkUrl = $"{institution.ProxyPrefix}{CourseLinkUrl}";
            }

            if (!string.IsNullOrWhiteSpace(institution.UrlSuffix))
            {
                CourseLinkUrl = $"{CourseLinkUrl}{institution.UrlSuffix}";
            }
        }
    }
}