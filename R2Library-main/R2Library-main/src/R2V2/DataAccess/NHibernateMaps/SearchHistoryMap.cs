#region

using FluentNHibernate.Mapping;
using R2V2.Core.Search;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SearchHistoryMap : ClassMap<SearchHistory>
    {
        public SearchHistoryMap()
        {
            Table("tSearchHistory");

            //LazyLoad();

            Id(x => x.Id).Column("iSearchHistoryId").GeneratedBy.Identity();
            Map(x => x.SearchTerm).Column("vchSearchTerm");
            Map(x => x.Synonyms).Column("vchSynonyms");

            Map(x => x.ResourceCount).Column("iResourceCount");
            Map(x => x.FileCount).Column("iFileCount");
            Map(x => x.HitCount).Column("iHitCount");
            Map(x => x.TotalSearchTime).Column("iTotalSearchTime");

            Map(x => x.FilterTime).Column("iFilterTime");
            Map(x => x.BuildRequestTime).Column("iBuildRequestTime");
            Map(x => x.SearchJobTime).Column("iSearchJobTime");
            Map(x => x.ReportJobTime).Column("iReportJobTime");

            Map(x => x.WordListJobTime).Column("WordListJobTime");
            Map(x => x.SearchRequest).Column("SearchRequest");
            Map(x => x.Timestamp).Column("dtTimestamp");
        }
    }
}