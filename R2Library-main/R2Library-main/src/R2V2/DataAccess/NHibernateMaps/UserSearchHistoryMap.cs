#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserSearchHistoryMap : BaseMap<UserSearchHistory>
    {
        public UserSearchHistoryMap()
        {
            // select top 1000 iUserSearchHistoryId, vchSearchXML, iUserId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus 
            // from tUserSearchHistory order by dtCreationDate desc
            Table("dbo.tUserSearchHistory");
            Id(x => x.Id).Column("iUserSearchHistoryId").GeneratedBy.Identity();
            Map(x => x.UserId).Column("iUserId");
            Map(x => x.SearchQuery).Column("vchSearchQuery");
            Map(x => x.SearchXml).Column("vchSearchXML");
            Map(x => x.ResultsCount).Column("iResultsCount");
        }
    }
}