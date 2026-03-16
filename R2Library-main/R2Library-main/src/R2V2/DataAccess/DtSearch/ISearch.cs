#region

using System.Collections.Generic;
using R2V2.Core.Resource;
using R2V2.Core.Search;
//using dtSearch.Engine;
using SearchResults = R2V2.Core.Search.SearchResults;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public interface ISearch //: ISearchStatusHandler 
    {
        SearchResults Execute(ISearchRequest searchRequest);
        List<IResource> ExecuteAdmin(ISearchRequest searchRequest);


        IndexStatus GetIndexStatus();
    }
}