#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserCourseLinksFolderMap : BaseMap<UserCourseLinksFolder>
    {
        public UserCourseLinksFolderMap()
        {
            Table("dbo.tUserCourseLinksFolder");
            Id(x => x.Id).Column("iUserCourseLinksFolderId").GeneratedBy.Identity();
            Map(x => x.FolderName).Column("vchCourseLinksFolderName");
            Map(x => x.DefaultFolder).Column("tiDefaultFolder");

            //References(x => x.User).Column("iUserId");
            Map(x => x.UserId).Column("iUserId");

            HasMany(x => x.UserCourseLinks).KeyColumn("iUserCourseLinksFolderId").AsBag().Inverse().Cascade
                .AllDeleteOrphan().ApplyFilter<SoftDeleteFilter>();
        }
    }
}