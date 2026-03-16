#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class GuestUserMap : BaseMap<GuestUser>
    {
        public GuestUserMap()
        {
            Table("dbo.tUser");
            Id(x => x.Id, "iUserId").GeneratedBy.Identity();
            Map(x => x.FirstName, "vchFirstName");
            Map(x => x.LastName, "vchLastName");
            Map(x => x.UserName, "vchUserName");
            Map(x => x.Password, "vchUserPassword");
            Map(x => x.Email, "vchUserEmail");

            Map(x => x.CreatedBy, "vchCreatorId");
            Map(x => x.CreationDate, "dtCreationDate");

            Map(x => x.ReceiveLockoutInfo, "tiReceiveLockoutInfo");
            Map(x => x.ReceiveNewResourceInfo, "tiReceiveNewResourceInfo");
            Map(x => x.ReceiveNewSearchResource, "tiReceiveNewSearchResource");

            Map(x => x.ReceiveNewEditionInfo, "tiReceiveNewEditionInfo");
            Map(x => x.ReceiveCartRemind, "tiReceiveCartRemind");
            Map(x => x.ReceiveForthComingPurchase, "tiReceiveForthcomingPurchase");
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.LastPasswordChange, "dtLastPasswordChange");
            Map(x => x.LoginAttempts, "iLoginAttempts");
            Map(x => x.PasswordHash, "vchPasswordHash");
            Map(x => x.PasswordSalt, "vchPasswordSalt");
            References(x => x.Role).Column("iRoleId");
            //References(x => x.Institution).Column("iInstitutionId");
        }
    }
}