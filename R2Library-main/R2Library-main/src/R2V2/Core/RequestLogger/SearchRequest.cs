#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.RequestLogger
{
    [Serializable]
    public class SearchRequest
    {
        public int SearchTypeId { get; set; }
        public bool IsArchivedSearch { get; set; }
        public bool IsExternalSearch { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("SearchRequest = [");
            sb.AppendFormat("SearchTypeId: {0}", SearchTypeId);
            sb.AppendFormat(", IsArchivedSearch: {0}", IsArchivedSearch);
            sb.AppendFormat(", IsExternalSearch: {0}", IsExternalSearch);
            sb.Append("]");
            return sb.ToString();
        }
    }
}