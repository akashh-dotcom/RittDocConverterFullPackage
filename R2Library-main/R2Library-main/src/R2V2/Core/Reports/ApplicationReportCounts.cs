#region

using System.Text;

#endregion

namespace R2V2.Core.Reports
{
    public class ApplicationReportCounts
    {
        public int UserSessionCount { get; set; }
        public int PageViewCount { get; set; }
        public int TotalContentRetrievalCount { get; set; }
        public int RestrictedContentRetrievalCount { get; set; }
        public int TocOnlyContentRetrievalCount { get; set; }
        public int ConcurrencyTurnawayCount { get; set; }
        public int AccessTurnawayCount { get; set; }
        public int SearchActiveCount { get; set; }
        public int SearchArchiveCount { get; set; }
        public int SearchImageCount { get; set; }
        public int SearchDrugCount { get; set; }
        public int SearchPubMedCount { get; set; }
        public int SearchMeshCount { get; set; }

        public int PdaTotalCount { get; set; }
        public int PdaActiveCount { get; set; }
        public int PdaCartCount { get; set; }
        public int PdaPurchasedCount { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder("ApplicationReportCounts = [");

            sb.AppendFormat("UserSessionCount: {0}", UserSessionCount);
            sb.AppendFormat(", PageViewCount: {0}", PageViewCount);
            sb.AppendFormat(", TotalContentRetrievalCount: {0}", TotalContentRetrievalCount);
            sb.AppendFormat(", RestrictedContentRetrievalCount: {0}", RestrictedContentRetrievalCount);
            sb.AppendFormat(", TocOnlyContentRetrievalCount: {0}", TocOnlyContentRetrievalCount);
            sb.AppendFormat(", ConcurrencyTurnawayCount: {0}", ConcurrencyTurnawayCount);
            sb.AppendFormat(", AccessTurnawayCount: {0}", AccessTurnawayCount);
            sb.AppendFormat(", SearchActiveCount: {0}", SearchActiveCount);
            sb.AppendFormat(", SearchArchiveCount: {0}", SearchArchiveCount);
            sb.AppendFormat(", SearchImageCount: {0}", SearchImageCount);
            sb.AppendFormat(", SearchDrugCount: {0}", SearchDrugCount);
            sb.AppendFormat(", SearchPubMedCount: {0}", SearchPubMedCount);
            sb.AppendFormat(", SearchMeshCount: {0}", SearchMeshCount);

            sb.AppendFormat(", PdaTotalCount: {0}", PdaTotalCount);
            sb.AppendFormat(", PdaActiveCount: {0}", PdaActiveCount);
            sb.AppendFormat(", PdaCartCount: {0}", PdaCartCount);
            sb.AppendFormat(", PdaPurchasedCount: {0}", PdaPurchasedCount);

            sb.Append("]");

            return sb.ToString();
        }
    }
}