#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserSavedSearchMap : BaseMap<UserSavedSearch>
    {
        public UserSavedSearchMap()
        {
            Table("dbo.tUserSavedSearch");
            Id(x => x.Id).Column("iUserSavedSearchId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchSavedSearchTitle");
            Map(x => x.Xml).Column("vchSearchXML");
            Map(x => x.SearchQuery).Column("vchSearchQuery");
            Map(x => x.ResultsCount).Column("iResultsCount");

            References(x => x.Folder).Column("iUserSavedFolderId");
        }
    }
}