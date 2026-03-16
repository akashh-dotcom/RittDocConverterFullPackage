#region

using System;
using R2V2.Extensions;

#endregion

namespace R2V2.Core.Search
{
    public class SearchHistory : ISearchHistory
    {
        private string _searchRequest;
        private string _searchTerm;
        private string _synonyms;

        public virtual int Id { get; set; }
        public virtual int ResourceCount { get; set; }
        public virtual int FileCount { get; set; }
        public virtual int HitCount { get; set; }
        public virtual long TotalSearchTime { get; set; }
        public virtual long FilterTime { get; set; }
        public virtual long BuildRequestTime { get; set; }
        public virtual long SearchJobTime { get; set; }
        public virtual long ReportJobTime { get; set; }
        public virtual long ResultsJobTime { get; set; }
        public virtual long WordListJobTime { get; set; }
        public virtual DateTime Timestamp { get; set; }

        public virtual string SearchTerm
        {
            get => _searchTerm != null ? _searchTerm.Truncate(255) : _searchTerm;
            set => _searchTerm = value;
        }

        public virtual string Synonyms
        {
            get => _synonyms != null ? _synonyms.Truncate(511) : _synonyms;
            set => _synonyms = value;
        }

        public virtual string SearchRequest
        {
            get => _searchRequest != null ? _searchRequest.Truncate(2000) : _searchRequest;
            set => _searchRequest = value;
        }
    }
}