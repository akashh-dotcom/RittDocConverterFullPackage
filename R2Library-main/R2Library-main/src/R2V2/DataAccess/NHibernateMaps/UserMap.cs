#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class UserMap : BaseMap<User>
    {
        public UserMap()
        {
            Table("dbo.tUser");
            Id(x => x.Id, "iUserId").GeneratedBy.Identity();
            Map(x => x.FirstName, "vchFirstName");
            Map(x => x.LastName, "vchLastName");
            Map(x => x.UserName, "vchUserName");
            Map(x => x.Password, "vchUserPassword");
            Map(x => x.Email, "vchUserEmail");
            References(x => x.Role).Column("iRoleId");
            References(x => x.Institution).Column("iInstitutionId").ReadOnly();
            Map(x => x.InstitutionId, "iInstitutionId");
            References(x => x.Department).Column("iDeptId");
            Map(x => x.ExpirationDate, "dtExpirationDate");
            Map(x => x.AthensUserName, "vchAthensUsername");
            Map(x => x.AthensPersistentUid, "vchAthensPersistantUID");
            Map(x => x.AthensTargetedId, "vchAthensTargetedId");
            Map(x => x.LastSession, "dtLastSession").ReadOnly();
            Map(x => x.LastPasswordChange, "dtLastPasswordChange");
            Map(x => x.LoginAttempts, "iLoginAttempts");
            Map(x => x.EnablePromotion, "tiEnablePromotion");
            Map(x => x.EnablePublisherAdd, "tiEnablePublisherAdd");
            Map(x => x.PasswordHash, "vchPasswordHash");
            Map(x => x.PasswordSalt, "vchPasswordSalt");
            Map(x => x.ExpertReviewerRequestDate, "dtFacultyUserRequestDate");
            Map(x => x.ConcurrentTurnawayAlert, "dtConcurrentTurnawayAlert").ReadOnly();
            HasMany(x => x.OptionValues).KeyColumn("iUserId").AsBag().Inverse();
            HasMany(x => x.UserTerritories).KeyColumn("iUserId").AsBag().ReadOnly();
            HasMany(x => x.UserBookmarkFolders).KeyColumn("iUserId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.UserCourseLinksFolders).KeyColumn("iUserId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.UserImagesFolders).KeyColumn("iUserId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.UserReferencesFolders).KeyColumn("iUserId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.Subscriptions).KeyColumn("iUserId").AsBag().ReadOnly();
        }
    }
}