#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserImagesFolderMap : BaseMap<UserImagesFolder>
    {
        public UserImagesFolderMap()
        {
            Table("dbo.tUserImagesFolder");
            Id(x => x.Id).Column("iUserImagesFolderId").GeneratedBy.Identity();
            Map(x => x.FolderName).Column("vchImagesFolderName");
            Map(x => x.DefaultFolder).Column("tiDefaultFolder");

            //References(x => x.User).Column("iUserId");
            Map(x => x.UserId).Column("iUserId");

            HasMany(x => x.UserImages).KeyColumn("iUserImagesFolderId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}