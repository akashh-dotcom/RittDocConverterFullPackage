#region

using System;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public interface IContentSettings
    {
        string DtSearchIndexLocation { get; set; }
        string DtSearchBinLocation { get; set; }
        string DtSearchLogFilePath { get; set; } // do not set in production!!!!
        string ContentLocation { get; set; }
        string NewContentLocation { get; set; }
        string UtilitiesContentLocation { get; set; }
        string XslLocation { get; set; }
        string ImageBaseUrl { get; set; }
        string BookCoverUrl { get; set; }
        DateTime MinTransformDate { get; set; }
        int ResourceLockTime { get; set; }
        string ImageBaseFileLocation { get; set; }
        decimal ResourceMinimumListPrice { get; set; }
        int SearchTypeaheadResultLimit { get; set; }
        int TransformInfoThresholdInMilliseconds { get; set; }
        int TransformWarnThresholdInMilliseconds { get; set; }
        int TransformErrorThresholdInMilliseconds { get; set; }
        bool IgnoreExternalEntities { get; set; }

        string IgnoredExternalEntityUriPath { get; set; }

        string KenticoUrl { get; set; }
        string KenticoProjectId { get; set; }
        string KenticoEnvironment { get; set; }
    }
}