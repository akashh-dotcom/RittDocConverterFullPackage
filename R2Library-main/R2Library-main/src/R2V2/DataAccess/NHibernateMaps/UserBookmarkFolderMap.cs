#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserBookmarkFolderMap : BaseMap<UserBookmarkFolder>
    {
        public UserBookmarkFolderMap()
        {
            Table("dbo.tUserBookmarkFolder");
            Id(x => x.Id).Column("iUserBookmarkFolderId").GeneratedBy.Identity();
            Map(x => x.FolderName).Column("vchBookmarkFolderName");
            Map(x => x.DefaultFolder).Column("tiDefaultFolder");
            Map(x => x.UserId).Column("iUserId");

            HasMany(x => x.UserBookmarks).KeyColumn("iUserBookmarkFolderId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}