#region

using R2V2.Core.Search;

#endregion

namespace R2V2.Web.Models.Search.Fields
{
    public interface ISearchField
    {
        string Code { get; }
        SearchFields SearchField { get; }
        SearchType LegacySearchType { get; }
    }
}