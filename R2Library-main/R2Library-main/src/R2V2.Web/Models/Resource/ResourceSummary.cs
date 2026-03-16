#region

using System;

#endregion

namespace R2V2.Web.Models.Resource
{
    [Serializable]
    public class ResourceSummary : ResourceSummaryBase, IResourceSummary, IAccessInfo
    {
        public int Id { get; set; }
        public string Authors { get; set; }
        public string Publisher { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string ImageFileName { get; set; }

        #region IResourceSummary

        public new string Description =>
            PublicationDate == null
                ? $"{Authors}, {Publisher}"
                : $"{Authors}, {Publisher}, {PublicationDate.Value.Year}";

        #endregion

        #region IAccessInfo

        public bool IsFullTextAvailable { get; set; }
        public bool IsArchive { get; set; }
        public bool IsForthcoming { get; set; }

        public bool IsPdaResource { get; set; }

        #endregion
    }
}