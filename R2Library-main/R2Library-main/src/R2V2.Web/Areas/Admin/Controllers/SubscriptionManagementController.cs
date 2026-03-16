#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Subscriptions;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.SubscriptionManagement;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Storages;
using User = R2V2.Core.Authentication.User;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
    public class SubscriptionManagementController : R2AdminBaseController
    {
        private readonly ApplicationWideStorageService _applicationWideStorageService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IQueryable<User> _users;
        private readonly UserService _userService;

        public SubscriptionManagementController(
            IAuthenticationContext authenticationContext
            , ApplicationWideStorageService applicationWideStorageService
            , ISubscriptionService subscriptionService
            , UserService userService
            , IQueryable<User> users
        ) : base(authenticationContext)
        {
            _applicationWideStorageService = applicationWideStorageService;
            _subscriptionService = subscriptionService;
            _userService = userService;
            _users = users;
        }

        public ActionResult Index()
        {
            var users = _userService.GetSubscriptionUsers();
            var model = new SubscriptionUsers();
            model.Users = users.ToSubscriptionUsers();
            return View(model);
        }

        public ActionResult Users()
        {
            var users = _userService.GetSubscriptionUsers();
            var model = new SubscriptionUsers();
            model.Users = users.ToSubscriptionUsers();
            return View(model);
        }

        public ActionResult EditUser(int userId)
        {
            var dbUser = _userService.GetUser(userId);
            var model = new SubscriptionUser(dbUser);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(SubscriptionUser model)
        {
            if (ModelState.IsValid)
            {
                var dbUser = _users.FirstOrDefault(x => x.Id == model.UserId);
                if (dbUser != null)
                {
                    if (_users.Any(x => x.Email.ToLower() == model.Email.ToLower() && x.Id != model.UserId))
                    {
                        ModelState.AddModelError("Email", @"This email address has already been registered.");
                    }
                    else
                    {
                        dbUser.FirstName = model.FirstName;
                        dbUser.LastName = model.LastName;
                        dbUser.UserName = model.Email;
                        dbUser.Email = model.Email;
                        dbUser.LoginAttempts = 0;
                        if (!string.IsNullOrWhiteSpace(model.NewPassword))
                        {
                            var passwordSalt = PasswordService.GenerateNewSalt();
                            dbUser.Password = model.NewPassword;
                            dbUser.LastPasswordChange = DateTime.Now;
                            dbUser.PasswordSalt = passwordSalt;
                            dbUser.PasswordHash =
                                PasswordService.GenerateSlowPasswordHash(model.NewPassword, passwordSalt);
                        }


                        _userService.SaveUser(dbUser);
                        return RedirectToAction("EditUser", new { model.UserId });
                    }
                }
            }

            var dbUser2 = _userService.GetUser(model.UserId);
            var newModel = new SubscriptionUser(dbUser2);
            return View(newModel);
        }

        [HttpPost]
        public ActionResult UserSubscription(AdminUserSubscription model)
        {
            if (ModelState.IsValid)
            {
                var item = new UserSubscription();
                item.Id = model.Id;
                item.UserId = model.UserId;
                item.ActivationDate = model.ActivationDate;
                item.TrialExpirationDate = model.TrialExpirationDate;
                item.ExpirationDate = model.ExpirationDate;
                _subscriptionService.SaveUserSubscription(item);
            }

            return RedirectToAction("EditUser", new { model.UserId });
        }
    }
}