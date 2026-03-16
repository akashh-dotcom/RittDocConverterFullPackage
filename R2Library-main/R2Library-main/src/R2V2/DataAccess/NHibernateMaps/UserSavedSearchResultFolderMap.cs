#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserSavedSearchResultFolderMap : BaseMap<UserSavedSearchResultFolder>
    {
        public UserSavedSearchResultFolderMap()
        {
            Table("dbo.tUserSavedResultsFolders");
            Id(x => x.Id).Column("iUserSavedResultsFolderId").GeneratedBy.Identity();

            Map(x => x.FolderName).Column("vchSavedResultsFolderName");
            Map(x => x.DefaultFolder).Column("tiDefaultFolder");

            //References(x => x.User).Column("iUserId");
            Map(x => x.UserId).Column("iUserId");

            HasMany(x => x.SavedSearchResults).KeyColumn("iUserSavedResultsFolderId").AsBag().Inverse().Cascade
                .AllDeleteOrphan().ApplyFilter<SoftDeleteFilter>();
        }
    }
}