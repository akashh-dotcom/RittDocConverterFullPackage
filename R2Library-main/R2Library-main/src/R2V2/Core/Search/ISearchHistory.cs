#region

using System;

#endregion

namespace R2V2.Core.Search
{
    public interface ISearchHistory
    {
        int Id { get; set; }
        string SearchTerm { get; set; }
        string Synonyms { get; set; }
        int ResourceCount { get; set; }
        int FileCount { get; set; }
        int HitCount { get; set; }
        long TotalSearchTime { get; set; }
        long FilterTime { get; set; }
        long BuildRequestTime { get; set; }
        long SearchJobTime { get; set; }
        long ReportJobTime { get; set; }
        long ResultsJobTime { get; set; }
        long WordListJobTime { get; set; }
        string SearchRequest { get; set; }
        DateTime Timestamp { get; set; }
    }
}