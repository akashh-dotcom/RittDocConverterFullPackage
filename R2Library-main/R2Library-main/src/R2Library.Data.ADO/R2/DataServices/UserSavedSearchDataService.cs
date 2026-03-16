#region

using System.Collections.Generic;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2.DataServices
{
    public class UserSavedSearchService : DataServiceBase
    {
        private const string UpdateSearchStatement =
            "update tUserSavedSearch set vchSearchQuery = @SearchQuery, iResultsCount = @ResultCount where iUserSavedSearchId = @UserSavedSearchId;";

        public int UpdateSearchQuery(int id, string searchQuery, int resultsCount)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("SearchQuery", searchQuery),
                new Int32Parameter("ResultCount", resultsCount),
                new Int32Parameter("UserSavedSearchId", id)
            };

            var rows = ExecuteUpdateStatement(UpdateSearchStatement, parameters, true);
            return rows;
        }
    }
}