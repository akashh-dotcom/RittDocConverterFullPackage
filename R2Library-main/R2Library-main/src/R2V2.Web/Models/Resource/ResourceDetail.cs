#region

using System;
using System.Collections.Generic;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.MyR2;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Content;
using R2V2.Core.Resource.Content.Navigation;
using R2V2.Web.Infrastructure.Settings;
using UserContentFolder = R2V2.Web.Models.MyR2.UserContentFolder;

#endregion

namespace R2V2.Web.Models.Resource
{
    [Serializable]
    public class ResourceDetail : ResourceSummary
    {
        public ResourceDetail()
        {
            UserContentFolders = new Dictionary<UserContentType, IEnumerable<UserContentFolder>>();
            ToolLinks = new ToolLinks { EmailPage = new EmailPage() };
            DictionaryTerms = new DictionaryTerms();

            var clientSettings = ServiceLocator.Current.GetInstance<IClientSettings>();
            MediaBaseUrl = clientSettings.MediaBaseUrl;
            FlowplayerKey = clientSettings.FlowplayerKey;
            OoyalaPCode = clientSettings.OoyalaPCode;
            OoyalaPlayerBrandingId = clientSettings.OoyalaPlayerBrandingId;
        }

        public ActionMenu ActionMenu { get; set; }

        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string EIsbn { get; set; }

        public string Edition { get; set; }

        // override Description property in ResourceSummary
        public new string Description { get; set; }

        public string PracticeArea { get; set; }

        public string Toc { get; set; }

        public Navigation Navigation { get; set; }

        public string Section { get; set; }

        public string ContentHtml { get; set; }

        public string ContentJson { get; set; }

        public IEnumerable<string> Topics { get; set; }

        public Dictionary<UserContentType, IEnumerable<UserContentFolder>> UserContentFolders { get; set; }

        //public EmailPage EmailPage { get; set; }

        public string ProCiteCitation { get; set; }
        public string EndNoteCitation { get; set; }
        public string RefWorksCitation { get; set; }
        public string ApaCitation { get; set; }

        public string LinkUrl { get; set; }

        public string Citation { get; set; }

        public int ResourceTimeoutInMinutes { get; set; }
        public int ResourceTimeoutModalInSeconds { get; set; }

        public string TurnawayMessage { get; set; }
        public ResourceAccess ResourceAccess { get; set; }

        public int Include { get; set; }
        public string Goto { get; set; }

        public string DoodyReviewUrl { get; set; }
        public short BrandonHillStatus { get; set; }
        public int DctStatusId { get; set; }

        public bool TabersStatus { get; set; }

        public string ContentProvider { get; set; }

        public bool IsEmailResource { get; set; }

        public string MediaBaseUrl { get; private set; }

        public DictionaryTerms DictionaryTerms { get; set; }

        public string Affiliation { get; set; }

        public bool HideContent { get; set; }
        public bool HideRecaptcha { get; set; }
        public string RecaptchaMessage { get; set; }

        public string RequestId { get; set; }

        // print security settings
        public bool IsPrintingEnabled { get; set; }
        public int NumberOfPrintRequests { get; set; }
        public int MaxNumberOfPrintRequests { get; set; }
        public bool PrintWarningThresholdReached { get; set; }
        public int PrintWarningThresholdPercentage { get; set; }

        //video settings 
        public byte ContainsVideo { get; set; }
        public string FlowplayerKey { get; set; }
        public string OoyalaPCode { get; set; }
        public string OoyalaPlayerBrandingId { get; set; }
        public string ProxyPrefix { get; set; }
        public int DoodyRating { get; set; }

        public List<IResource> RecentlyReleasedResources { get; set; }

        public bool IsInstitutionAdmin { get; set; }

        public string ContentProviderDisplay()
        {
            return string.IsNullOrWhiteSpace(ContentProvider)
                ? ""
                : $"Content Provided by {ContentProvider}";
        }
    }
}