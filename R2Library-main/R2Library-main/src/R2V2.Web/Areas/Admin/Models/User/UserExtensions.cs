#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public static class UserExtensions
    {
        public static IEnumerable<User> ToInstitutionUsers(this List<Core.Authentication.User> users)
        {
            return users.Select(ToInstitutionUser);
        }


        public static User ToInstitutionUser(this Core.Authentication.User user)
        {
            return new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Password = user.Password,
                Email = user.Email,
                RecordStatusText = user.RecordStatus ? user.IsLocked ? "Locked" : "Yes" : "No",
                RoleId = user.Role.Id,
                Role = user.Role,
                InstitutionId = user.Institution.Id,
                InstitutionName = user.Institution.Name,
                Department = user.Department,
                ExpirationDate = user.ExpirationDate,
                AthensTargetedId = user.AthensTargetedId,
                LastSession = user.LastSession,
                LastPasswordChange = user.LastPasswordChange,
                DisplayLink = user.Institution.RecordStatus
            };
        }

        public static IEnumerable<User> ToSubscriptionUsers(this List<Core.Authentication.User> users)
        {
            return users.Select(ToSubscriptionUser);
        }

        public static User ToSubscriptionUser(this Core.Authentication.User user)
        {
            //user.Subscriptions
            var u = new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Password = user.Password,
                Email = user.Email,
                RecordStatusText = user.RecordStatus ? user.IsLocked ? "Locked" : "Yes" : "No",
                RoleId = user.Role.Id,
                Role = user.Role,
                ExpirationDate = user.ExpirationDate,
                LastSession = user.LastSession,
                LastPasswordChange = user.LastPasswordChange,
                DisplayLink = user.RecordStatus
            };
            if (user.Subscriptions != null)
            {
                var activeSubs = user.Subscriptions.Where(x => x.CanView());
                u.ActiveSubscriptions = activeSubs.Count();
                u.InActiveSubscriptions = user.Subscriptions.Count - u.ActiveSubscriptions;
            }

            return u;
        }

        public static UserEdit ToUserEdit(this IUser user)
        {
            if (user != null)
            {
                return new UserEdit
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Password = user.Password,
                    Email = user.Email,
                    RecordStatus = user.RecordStatus,
                    Role = user.Role,
                    InstitutionId = user.InstitutionId.GetValueOrDefault(),
                    Department = user.Department,
                    ExpirationDate = user.ExpirationDate,
                    AthensTargetedId = user.AthensTargetedId,
                    LastPasswordChange = user.LastPasswordChange,
                    CurrentTerritoryIds = user.UserTerritories != null
                        ? user.UserTerritories.Where(x => x.RecordStatus).Select(x => x.TerritoryId).ToArray()
                        : null,
                    ExpertReviewerRequestDate = user.ExpertReviewerRequestDate
                };
            }

            return null;
        }
    }
}