#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserReferenceMap : BaseMap<UserReference>
    {
        public UserReferenceMap()
        {
            Table("dbo.tUserReference");
            Id(x => x.Id).Column("iUserReferenceId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchReferenceTitle").CustomType("StringClob");
            Map(x => x.ChapterSectionTitle).Column("vchChapterSectionTitle");
            Map(x => x.ChapterSectionId).Column("vchChapterSectionId");
            Map(x => x.Isbn).Column("vchISBN");

            References(x => x.UserReferencesFolder).Column("iUserReferenceFolderId");
        }
    }
}