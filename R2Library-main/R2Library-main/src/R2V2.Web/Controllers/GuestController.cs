#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.Settings;
using GuestUser = R2V2.Web.Models.GuestUser;

#endregion

namespace R2V2.Web.Controllers
{
    public class GuestController : R2BaseController
    {
        private readonly InstitutionService _institutionService;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly IQueryable<User> _users;
        private readonly UserService _userService;

        public GuestController(UserService userService, InstitutionService institutionService,
            IInstitutionSettings institutionSettings, IQueryable<User> users)
        {
            _institutionService = institutionService;
            _institutionSettings = institutionSettings;
            _users = users;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new GuestUser());
        }

        [HttpPost]
        public ActionResult Index(GuestUser guestUser)
        {
            if (ModelState.IsValid)
            {
                if (_users.Any(x => x.Email.ToLower() == guestUser.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", string.Format("This email address has already been registered."));
                    return View(guestUser);
                }

                var guestInstitutionId =
                    _institutionService.GetGuestInstitutionId(_institutionSettings.GuestAccountNumber);

                var user = new Core.Authentication.GuestUser
                {
                    FirstName = guestUser.FirstName,
                    LastName = guestUser.LastName,
                    UserName = guestUser.Email,
                    Email = guestUser.Email,
                    Role = new Role { Id = (int)RoleCode.USERS },
                    InstitutionId = guestInstitutionId,
                    CreatedBy = "WebUser",
                    CreationDate = DateTime.Now,
                    Password = guestUser.NewPassword
                };

                _userService.SaveGuestAndSubscriptionUser(user);

                return RedirectToAction("Index", "Home");
            }

            return View(guestUser);
        }
    }
}