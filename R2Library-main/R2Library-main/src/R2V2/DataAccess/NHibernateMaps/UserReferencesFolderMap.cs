#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserReferencesFolderMap : BaseMap<UserReferencesFolder>
    {
        public UserReferencesFolderMap()
        {
            Table("dbo.tUserReferencesFolder");
            Id(x => x.Id).Column("iUserReferencesFolderId").GeneratedBy.Identity();
            Map(x => x.FolderName).Column("vchReferencesFolderName");
            Map(x => x.DefaultFolder).Column("tiDefaultFolder");

            //References(x => x.User).Column("iUserId");
            Map(x => x.UserId).Column("iUserId");

            HasMany(x => x.UserReferences).KeyColumn("iUserReferenceFolderId").AsBag().Inverse().Cascade
                .AllDeleteOrphan().ApplyFilter<SoftDeleteFilter>();
        }
    }
}