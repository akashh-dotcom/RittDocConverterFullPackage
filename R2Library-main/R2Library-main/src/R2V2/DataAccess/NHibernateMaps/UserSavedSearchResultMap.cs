#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserSavedSearchResultMap : BaseMap<UserSavedSearchResult>
    {
        public UserSavedSearchResultMap()
        {
            Table("dbo.tUserSavedSearchResults");
            Id(x => x.Id).Column("iUserSavedSearchResultsId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchSavedSearchTitle");
            Map(x => x.SearchResultSet).Column("vchSearchResultSet").CustomType("StringClob")
                .CustomSqlType("nvarchar(max)");
            ;
            //Map(x => x.SearchQuery).Column("vchSearchQuery");
            Map(x => x.ResultsCount).Column("iResultsCount");

            References(x => x.Folder).Column("iUserSavedResultsFolderId");
        }
    }
}