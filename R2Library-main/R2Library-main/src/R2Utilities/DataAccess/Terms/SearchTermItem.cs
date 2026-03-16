#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public class SearchTermItem : IEquatable<SearchTermItem>
    {
        public string SearchTerm { get; set; }
        public bool IsKeyword { get; set; }

        public bool Equals(SearchTermItem other)
        {
            return SearchTerm == other.SearchTerm && IsKeyword == other.IsKeyword;
        }

        public override int GetHashCode()
        {
            var hashSearchTerm = SearchTerm == null ? 0 : SearchTerm.GetHashCode();
            var hashIsKeyword = IsKeyword.GetHashCode();

            return hashSearchTerm ^ hashIsKeyword;
        }
    }

    public static class SearchTerm
    {
        public static HashSet<SearchTermItem> HashSet(HashSet<string> terms, bool isKeywords)
        {
            return new HashSet<SearchTermItem>(terms.Select(t => new SearchTermItem
                { SearchTerm = t, IsKeyword = isKeywords }));
        }
    }
}