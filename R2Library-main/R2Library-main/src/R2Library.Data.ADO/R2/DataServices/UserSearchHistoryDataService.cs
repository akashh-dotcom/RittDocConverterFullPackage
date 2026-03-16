#region

using System.Collections.Generic;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2.DataServices
{
    public class UserSearchHistoryService : DataServiceBase
    {
        private const string UpdateSearchStatement =
            "update tUserSearchHistory set vchSearchQuery = @SearchQuery, iResultsCount = @ResultCount where iUserSearchHistoryId = @UserSearchHistoryId;";

        public int UpdateSearchQuery(int id, string searchQuery, int resultsCount)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("SearchQuery", searchQuery),
                new Int32Parameter("ResultCount", resultsCount),
                new Int32Parameter("UserSearchHistoryId", id)
            };

            var rows = ExecuteUpdateStatement(UpdateSearchStatement, parameters, true);
            return rows;
        }
    }
}