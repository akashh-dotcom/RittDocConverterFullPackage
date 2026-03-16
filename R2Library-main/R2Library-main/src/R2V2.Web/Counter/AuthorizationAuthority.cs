#region

using System.Linq;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using Sushi.Core;

#endregion

namespace R2V2.Web.Counter
{
    public class AuthorizationAuthority : IAuthorizationAuthority
    {
        private readonly InstitutionService _institutionService;
        private readonly UserService _userService;

        public AuthorizationAuthority(InstitutionService institutionService, UserService userService)
        {
            _institutionService = institutionService;
            _userService = userService;
        }

        public bool IsRequestorAuthorized(Requestor requestor, CustomerReference targetCustomer)
        {
            var userCount = 0;
            var institition = _institutionService.GetInstitutionForEdit(targetCustomer.ID);
            var users = _userService.GetUsers(new UserQuery { InstitutionId = institition.Id }, ref userCount, true);

            targetCustomer.Name = institition.Name;

            return users.Any(user => user.UserName == requestor.Name && user.Email == requestor.Email);
        }
    }
}