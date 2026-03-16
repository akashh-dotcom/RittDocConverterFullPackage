#region

using System;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public class ContentSettings : AutoSettings, IContentSettings
    {
        public string DtSearchIndexLocation { get; set; }
        public string DtSearchBinLocation { get; set; }
        public string DtSearchLogFilePath { get; set; }
        public string ContentLocation { get; set; }
        public string NewContentLocation { get; set; }
        public string UtilitiesContentLocation { get; set; }
        public string XslLocation { get; set; }
        public string ImageBaseUrl { get; set; }
        public string BookCoverUrl { get; set; }
        public DateTime MinTransformDate { get; set; }
        public int ResourceLockTime { get; set; }
        public string ImageBaseFileLocation { get; set; }
        public decimal ResourceMinimumListPrice { get; set; }
        public int SearchTypeaheadResultLimit { get; set; }
        public int TransformInfoThresholdInMilliseconds { get; set; }
        public int TransformWarnThresholdInMilliseconds { get; set; }
        public int TransformErrorThresholdInMilliseconds { get; set; }
        public bool IgnoreExternalEntities { get; set; }

        public string IgnoredExternalEntityUriPath { get; set; }

        public string KenticoUrl { get; set; }
        public string KenticoProjectId { get; set; }
        public string KenticoEnvironment { get; set; }
    }
}