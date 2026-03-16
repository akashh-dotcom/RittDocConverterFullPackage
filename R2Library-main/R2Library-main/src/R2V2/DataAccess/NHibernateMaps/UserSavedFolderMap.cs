#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserSavedFolderMap : BaseMap<UserSavedFolder>
    {
        public UserSavedFolderMap()
        {
            Table("dbo.tUserSavedFolders");
            Id(x => x.Id).Column("iUserSavedFolderId").GeneratedBy.Identity();
            Map(x => x.FolderName).Column("vchSavedFolderName");
            Map(x => x.DefaultFolder).Column("tiDefaultFolder");

            //References(x => x.User).Column("iUserId");
            Map(x => x.UserId).Column("iUserId");

            HasMany(x => x.SavedSearches).KeyColumn("iUserSavedFolderId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}