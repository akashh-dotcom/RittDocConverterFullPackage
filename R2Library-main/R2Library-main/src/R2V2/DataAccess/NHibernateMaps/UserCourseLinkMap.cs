#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserCourseLinkMap : BaseMap<UserCourseLink>
    {
        public UserCourseLinkMap()
        {
            Table("dbo.tUserCourseLinks");
            Id(x => x.Id).Column("iUserCourseLinksId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchCourseLinksTitle").CustomType("StringClob");
            Map(x => x.ChapterSectionTitle).Column("vchChapterSectionTitle");
            Map(x => x.ChapterSectionId).Column("vchChapterSectionId");
            Map(x => x.Isbn).Column("vchISBN");

            References(x => x.UserCourseLinksFolder).Column("iUserCourseLinksFolderId");
        }
    }
}