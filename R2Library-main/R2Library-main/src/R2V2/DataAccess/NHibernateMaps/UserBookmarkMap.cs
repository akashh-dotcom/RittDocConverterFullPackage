#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserBookmarkMap : BaseMap<UserBookmark>
    {
        public UserBookmarkMap()
        {
            Table("dbo.tUserBookmark");
            Id(x => x.Id).Column("iUserBookmarkId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchBookmarkTitle").CustomType("StringClob");
            Map(x => x.ChapterSectionTitle).Column("vchChapterSectionTitle");
            Map(x => x.ChapterSectionId).Column("vchChapterSectionId");
            Map(x => x.TypeId).Column("iBookmarkTypeId");

            References(x => x.UserBookmarkFolder).Column("iUserBookmarkFolderId");
        }
    }
}