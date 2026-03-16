#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public abstract class SearchFieldBase : ISearchField
    {
        protected SearchFieldBase(string code, SearchFields searchField, SearchType legacySearchType)
        {
            Code = code;
            SearchField = searchField;
            LegacySearchType = legacySearchType;
        }

        public string Code { get; }

        public SearchFields SearchField { get; }

        public SearchType LegacySearchType { get; }
    }
}