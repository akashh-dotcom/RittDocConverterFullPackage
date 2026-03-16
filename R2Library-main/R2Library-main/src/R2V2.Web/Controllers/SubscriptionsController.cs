#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Core.Subscriptions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Resource;
using R2V2.Web.Models.Subscriptions;
using GuestUser = R2V2.Core.Authentication.GuestUser;

#endregion

namespace R2V2.Web.Controllers
{
    [RequestLoggerFilter(false)]
    public class SubscriptionsController : R2BaseController
    {
        private const string SubscriptionSecurityTokenKey = "SubscriptionSecurityTokenKey";
        private readonly IAdminSettings _adminSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly EmailSiteService _emailService;
        private readonly IEmailSettings _emailSettings;

        private readonly ILog<SubscriptionsController> _log;
        private readonly IOrderService _orderService;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourceService _resourceService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IQueryable<User> _users;
        private readonly UserService _userService;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public SubscriptionsController(
            ILog<SubscriptionsController> log
            , IAuthenticationContext authenticationContext
            , ISubscriptionService subscriptionService
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
            , IQueryable<User> users
            , UserService userService
            , IAuthenticationService authenticationService
            , IOrderService orderService
            , IAdminSettings adminSettings
            , IEmailSettings emailSettings
            , EmailSiteService emailService
            , IUserSessionStorageService userSessionStorageService
        ) : base(authenticationContext)
        {
            _log = log;
            _subscriptionService = subscriptionService;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
            _users = users;
            _userService = userService;
            _authenticationService = authenticationService;
            _orderService = orderService;
            _adminSettings = adminSettings;
            _emailSettings = emailSettings;
            _emailService = emailService;
            _userSessionStorageService = userSessionStorageService;
        }

        [RequestLoggerFilter(false)]
        public ActionResult Index(int id = 0)
        {
            var subs = _subscriptionService.GetAvailableSubscriptions();
            var user = AuthenticatedInstitution != null ? CurrentUser : null;
            var model = new SubscriptionListModel(subs, user);

            var selectedSubscription = id == 0
                ? model.SubscriptionDetailsList.FirstOrDefault()
                : model.SubscriptionDetailsList.FirstOrDefault(x => x.Subscription.Id == id);

            if (selectedSubscription == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var resourceIds = selectedSubscription.Subscription.SubResources.Select(x => x.ResourceId).ToArray();

            var resources = _resourceService.GetResourcesByIds(resourceIds).Select(x => x.ToResourceSummary()).ToList();

            foreach (var resource in resources)
            {
                resource.Url = Url.Action("Title", "Resource", new { resource.Isbn });
            }

            if (AuthenticatedInstitution != null)
            {
                foreach (var resourceSummary in resources)
                {
                    resourceSummary.IsFullTextAvailable =
                        _resourceAccessService.IsFullTextAvailable(resourceSummary.Id);
                }
            }

            selectedSubscription.Resources = resources;
            model.SelectedSubscriptionDetail = selectedSubscription;

            return View(model);
        }


        public ActionResult CreateUser()
        {
            var model = new SubscriptionUserModel();
            _userSessionStorageService.Put(SubscriptionSecurityTokenKey, model.Token1);
            return View(model);
        }

        [HttpPost]
        [NonGoogleCaptchaValidation]
        public ActionResult CreateUser(SubscriptionUserModel model, bool captchaValid, string captchaErrorMessage)
        {
            if (ModelState.IsValid && IsCaptchaValid(captchaValid) && AreSecurityTokensCorrect(model))
            {
                if (_users.Any(x => x.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", @"This email address has already been registered.");
                    return View(model);
                }

                var passwordSalt = PasswordService.GenerateNewSalt();
                var user = new GuestUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    Role = new Role { Id = (int)RoleCode.SUBUSER },
                    CreatedBy = "subscriptionUser",
                    CreationDate = DateTime.Now,
                    Password = model.NewPassword,
                    LoginAttempts = 0,
                    LastPasswordChange = DateTime.Now,
                    PasswordSalt = passwordSalt,
                    PasswordHash = PasswordService.GenerateSlowPasswordHash(model.NewPassword, passwordSalt)
                };

                _userService.SaveGuestAndSubscriptionUser(user);

                var authResult = _authenticationService.ReloadUser(user.Id);
                AuthenticationContext.Set(authResult.AuthenticatedInstitution);

                return RedirectToAction("Index", "Subscriptions");
            }

            if (!IsCaptchaValid(captchaValid))
            {
                if (!string.IsNullOrWhiteSpace(captchaErrorMessage))
                {
                    ModelState.AddModelError("RecaptchaMessage", captchaErrorMessage);
                }
            }

            return View(model);
        }

        public ActionResult PurchaseSubscription(SubscriptionPurchaseModel model)
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Checkout",
                new { subscriptionId = model.Subscription.Id, subscriptionType = model.Type });
        }


        public ActionResult Checkout(int subscriptionId, SubscriptionType subscriptionType)
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Index");
            }

            var subscription = _subscriptionService.GetSubscription(subscriptionId);
            var model = new CheckoutModel { Subscription = subscription, Type = subscriptionType };

            return View(model);
        }

        [HttpPost]
        public ActionResult Checkout(CheckoutModel model)
        {
            if (ModelState.IsValid)
            {
                model.Subscription = _subscriptionService.GetSubscription(model.Subscription.Id);
                var subOrderHistory = model.OrderHistory;
                subOrderHistory.UserId = CurrentUser.Id;
                subOrderHistory.SubscriptionId = model.Subscription.Id;
                subOrderHistory.Type = model.Type;
                switch (model.Type)
                {
                    case SubscriptionType.Monthly:
                        subOrderHistory.Price = model.Subscription.MonthlyPrice;
                        break;
                    case SubscriptionType.Annual:
                        subOrderHistory.Price = model.Subscription.AnnualPrice;
                        break;
                }


                //TODO: Create User Subscription with endDate
                //TODO: Create Order History of Subscription
                //TODO: Create Email Confirmation
                var orderHistoryId = _orderService.PlaceOrder(subOrderHistory, CurrentUser);

                if (orderHistoryId > 0)
                {
                    SendOrderConfirmation(orderHistoryId);
                    var authResult = _authenticationService.ReloadUser(CurrentUser.Id);
                    AuthenticationContext.Set(authResult.AuthenticatedInstitution);
                    return RedirectToAction("OrderConfirmation", new { orderHistoryId });
                }
                else
                {
                    //Need to show exception.
                    return View(model);
                }
            }

            return RedirectToAction("Index");
        }

        [NoCache]
        public ActionResult OrderConfirmation(int orderHistoryId)
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Index");
            }

            var model = GetOrderHistoryModel(orderHistoryId, true);

            return View(model);
        }

        private SubscriptionOrderHistoryModel GetOrderHistoryModel(int orderHistoryId, bool isWebVersion = false)
        {
            var model = new SubscriptionOrderHistoryModel
            {
                CurrentUser = CurrentUser,
                OrderHistory = _subscriptionService.GetOrderHistory(orderHistoryId, CurrentUser)
            };
            if (model.OrderHistory != null)
            {
                model.Subscription = _subscriptionService.GetSubscription(model.OrderHistory.SubscriptionId);
            }

            model.IsWebVersion = isWebVersion;
            return model;
        }

        private void SendOrderConfirmation(int orderHistoryId)
        {
            var order = GetOrderHistoryModel(orderHistoryId);
            var currentUser = CurrentUser;

            var emailPage = new EmailPage
            {
                To = currentUser.Email,
                Bcc = _adminSettings.PurchaseConfirmationEmail,
                Subject = "R2 Library Purchase Confirmation – Thank You"
            };


            if (!_emailSettings.SendToCustomers)
            {
                _log.WarnFormat("CC address overwritten to '{0}' was '{1}'", _emailSettings.TestEmailAddresses,
                    emailPage.Cc);
                emailPage.Cc = _emailSettings.TestEmailAddresses;
            }

            var messageBody = RenderRazorViewToString("Subscriptions", "_OrderSummary", order);

            _emailService.SendEmailMessageToQueue(messageBody, emailPage);
        }

        private bool AreSecurityTokensCorrect(SubscriptionUserModel model)
        {
            try
            {
                _log.Debug(
                    $"Token1: {model.Token1}, Token2: {model.Token2}, Token3: {model.Token3}, Token4: {model.Token4}");

                // check if token is session matches token in model
                var sessionToken = _userSessionStorageService.Get<string>(SubscriptionSecurityTokenKey);
                _log.Debug($"sessionToken: {sessionToken}");
                if (string.IsNullOrWhiteSpace(sessionToken) ||
                    !sessionToken.Equals(model.Token1, StringComparison.OrdinalIgnoreCase))
                {
                    model.SecurityMessage =
                        "We are sorry, but there appears to be a problem processing you request. Please try submitting the form again. (code:9001)";
                    _log.Info(
                        $"AreSecurityTokensCorrect() - session token '{sessionToken}' does NOT match model token '{model.Token1}'");
                    return false;
                }

                // check if Token4 is set. if it is, this is a bot
                if (!string.IsNullOrWhiteSpace(model.Token4))
                {
                    model.SecurityMessage =
                        "We are sorry, but there appears to be a problem processing you request. Please try submitting the form again. (code:9002)";
                    _log.Info($"AreSecurityTokensCorrect() - Token4 field is set to '{model.Token4}'");
                    return false;
                }

                // verify token 2
                DateTime token2DateTime;
                long token2Ticks;
                //if (DateTime.TryParse(model.Token2, out DateTime token2DateTime))
                if (long.TryParse(model.Token2, out token2Ticks))
                {
                    token2DateTime = new DateTime(token2Ticks);
                    if (token2DateTime < DateTime.Now.AddHours(-1))
                    {
                        model.SecurityMessage =
                            "We are sorry, but there appears to be a problem processing you request. Please try submitting the form again. (code:9003)";
                        _log.Info(
                            $"AreSecurityTokensCorrect() - Token2 as expired: Token2: '{token2DateTime}', Now + 1 hour: '{DateTime.Now.AddHours(1)}'");
                        return false;
                    }
                }
                else
                {
                    model.SecurityMessage =
                        "We are sorry, but there appears to be a problem processing you request. Please try submitting the form again. (code:9004)";
                    _log.Info($"AreSecurityTokensCorrect() - Can't parse Token2: '{model.Token2}'");
                    return false;
                }

                // verify token 3
                var token3DateTime = model.GetToken3DateTime();
                if (token2DateTime != token3DateTime)
                {
                    model.SecurityMessage =
                        "We are sorry, but there appears to be a problem processing you request. Please try submitting the form again. (code:9005)";
                    _log.Info($"AreSecurityTokensCorrect() - Token2 != Token3 => '{model.Token2}' != '{model.Token3}'");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Warn(ex.Message, ex);
                return false;
            }
        }
    }
}