#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserImageMap : BaseMap<UserImage>
    {
        public UserImageMap()
        {
            Table("dbo.tUserImage");
            Id(x => x.Id).Column("iUserImageId").GeneratedBy.Identity();
            Map(x => x.Title).Column("vchImageTitle").CustomType("StringClob");
            Map(x => x.ChapterSectionTitle).Column("vchChapterSectionTitle");
            Map(x => x.ChapterSectionId).Column("vchChapterSectionId");
            Map(x => x.Isbn).Column("vchISBN");
            Map(x => x.Filename).Column("vchImageFileName");

            References(x => x.UserImagesFolder).Column("iUserImagesFolderId");
        }
    }
}