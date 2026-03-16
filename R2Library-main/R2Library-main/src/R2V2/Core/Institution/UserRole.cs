#region

using System;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.DependencyInjection;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    [DoNotRegisterWithContainer]
    public class UserRole
    {
        public static readonly UserRole RittenhouseAdministrator = new UserRole
            { Id = RoleCode.RITADMIN, Description = "Rittenhouse Administrator" };

        public static readonly UserRole InstitutionAdministrator = new UserRole
            { Id = RoleCode.INSTADMIN, Description = "Institution Administrator" };

        public static readonly UserRole SalesAssociate = new UserRole
            { Id = RoleCode.SALESASSOC, Description = "Sales Associate" };

        public static readonly UserRole User = new UserRole { Id = RoleCode.USERS, Description = "User" };

        public static readonly UserRole PublisherUser = new UserRole
            { Id = RoleCode.PUBUSER, Description = "Publisher User" };

        public static readonly UserRole NoUser = new UserRole { Id = RoleCode.NoUser, Description = "No User" };

        public static readonly UserRole Institution = new UserRole
            { Id = RoleCode.Institution, Description = "Institution" };

        public static readonly UserRole ExpertReviewer = new UserRole
            { Id = RoleCode.ExpertReviewer, Description = "Expert Reviewer" };

        public static readonly UserRole SubscriptionUser = new UserRole
            { Id = RoleCode.SUBUSER, Description = "Subscription User" };

        private UserRole()
        {
        }

        public RoleCode Id { get; protected set; }
        public string Description { get; protected set; }

        public static UserRole ConvertToUserRole(Role role)
        {
            switch (role.Id)
            {
                case (int)RoleCode.INSTADMIN:
                    return InstitutionAdministrator;

                case (int)RoleCode.RITADMIN:
                    return RittenhouseAdministrator;

                case (int)RoleCode.USERS:
                    return User;

                case (int)RoleCode.PUBUSER:
                    return PublisherUser;

                case (int)RoleCode.SALESASSOC:
                    return SalesAssociate;

                case (int)RoleCode.Institution:
                    return Institution;

                case (int)RoleCode.ExpertReviewer:
                    return ExpertReviewer;

                case (int)RoleCode.SUBUSER:
                    return SubscriptionUser;

                case (int)RoleCode.NoUser:
                default:
                    return NoUser;
            }
        }
    }

    [Serializable]
    public static class RoleExtensions
    {
        public static UserRole ToUserRole(this RoleCode rolecode)
        {
            switch (rolecode)
            {
                case RoleCode.INSTADMIN:
                    return UserRole.InstitutionAdministrator;

                case RoleCode.RITADMIN:
                    return UserRole.RittenhouseAdministrator;

                case RoleCode.USERS:
                    return UserRole.User;

                case RoleCode.PUBUSER:
                    return UserRole.PublisherUser;

                case RoleCode.SALESASSOC:
                    return UserRole.SalesAssociate;

                case RoleCode.ExpertReviewer:
                    return UserRole.ExpertReviewer;
                case RoleCode.SUBUSER:
                    return UserRole.SubscriptionUser;

                case RoleCode.NoUser:
                default:
                    throw new ArgumentOutOfRangeException("rolecode");
            }
        }
    }
}