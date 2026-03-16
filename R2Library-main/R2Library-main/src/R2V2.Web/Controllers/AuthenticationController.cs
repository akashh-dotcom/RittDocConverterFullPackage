#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Authentication;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Authentication;
using R2V2.Web.Models.Error;
using UserService = R2V2.Core.UserService;

#endregion

namespace R2V2.Web.Controllers
{
    public class AuthenticationController : R2BaseController
    {
        private readonly AthensAuthenticationService _athensAuthenticationService;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly EmailSiteService _emailService;
        private readonly InstitutionService _institutionService;
        private readonly ILog<AuthenticationController> _log;
        private readonly IOidcSettings _oidcSettings;
        private readonly PassiveAuthenticationCookieService _passiveAuthenticationCookieService;
        private readonly IResourceService _resourceService;
        private readonly UserService _userService;

        public AuthenticationController(ILog<AuthenticationController> log
            , IAuthenticationContext authenticationContext
            , IAuthenticationService authenticationService
            , InstitutionService institutionService
            , IResourceService resourceService
            , EmailSiteService emailService
            , UserService userService
            , AthensAuthenticationService athensAuthenticationService
            , PassiveAuthenticationCookieService passiveAuthenticationCookieService
            , OidcSettings oidcSettings
        )
            : base(authenticationContext)
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _authenticationService = authenticationService;
            _userService = userService;
            _athensAuthenticationService = athensAuthenticationService;
            _passiveAuthenticationCookieService = passiveAuthenticationCookieService;
            _emailService = emailService;
            _institutionService = institutionService;
            _resourceService = resourceService;
            _oidcSettings = oidcSettings;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginParam());
        }

        [HttpPost]
        public ActionResult Login(LoginParam param)
        {
            Authenticate(param);

            return View(param);
        }

        public ActionResult LoginAjax(LoginParam param)
        {
            var results = new AuthenticationJson { Successful = false, InstitutionHomePage = "/" };

            try
            {
                var userNameRegex = new Regex(@"[,]");
                if (string.IsNullOrWhiteSpace(param.UserName))
                {
                    results.ErrorMessage = Resources.UserIsRequired;
                    results.UserName = "";
                    results.Successful = false;
                }
                else if (userNameRegex.IsMatch(param.UserName))
                {
                    results.ErrorMessage = Resources.UserNameCommas;
                    results.UserName = "";
                    results.Successful = false;
                }
                else
                {
                    var authenticationResult = Authenticate(param);
                    var errorMessage = GetLoginMessage(authenticationResult);

                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        if (authenticationResult.AuthenticatedInstitution != null)
                        {
                            results.InstitutionHomePage =
                                GetInstitutionHomepage(authenticationResult.AuthenticatedInstitution.HomePage);
                            results.UserName = authenticationResult.AuthenticatedInstitution.DisplayName;

                            if (authenticationResult.AuthenticatedInstitution.User != null)
                            {
                                var titleRedirect =
                                    GetTitleRedirect(authenticationResult.AuthenticatedInstitution.User);
                                results.RedirectUrl = titleRedirect ?? param.RedirectUrl;
                            }
                        }
                    }
                    else
                    {
                        results.UserName = "";
                        results.ErrorMessage = errorMessage;
                    }

                    results.Successful = authenticationResult.WasSuccessful;
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder()
                    .AppendFormat("Logic AJAX Exception - UserName: {0}", param.UserName).AppendLine()
                    .Append(ex.Message);
                _log.Error(sb.ToString(), ex);

                results.ErrorMessage = "System error, please contact administrator";
                results.UserName = "";
                results.Successful = false;
            }

            return Json(results);
        }

        private string GetLoginMessage(AuthenticationResult authenticationResult)
        {
            if (authenticationResult.WasSuccessful)
            {
                return null;
            }

            if (authenticationResult.WasBlocked)
            {
                return "Login attempt blocked, too many failed attempts";
            }

            if (authenticationResult.WasAutoLocked)
            {
                var message = new StringBuilder()
                    .Append("Login has been locked out due to inactivity.<br/>")
                    .AppendFormat(
                        "Please <a href=\"{0}\">Click Here</a> to contact your Administrator to unlock your account.",
                        Url.Action("UnlockAccount", "Contact",
                            new
                            {
                                UserId = authenticationResult.LockedUser.Id,
                                authenticationResult.LockedUser.InstitutionId
                            }))
                    .ToString();

                return message;
            }

            if (authenticationResult.WasAttemptLocked)
            {
                var message = new StringBuilder()
                    .Append("Login has been locked out due too many Failed Attempts.<br/>")
                    .Append("To unlock your account please use the Forgot Password link below")
                    .ToString();
                return message;
            }

            return "Invalid user name or password";
        }

        public ActionResult ForgotPassword(ForgotPasswordParam param)
        {
            var results = new AuthenticationJson { Successful = false, InstitutionHomePage = null };

            var user = _userService.GetUser(param.UserName);
            if (user == null)
            {
                results.ErrorMessage = "User could not be found or has been removed, please contact an administrator";
                return Json(results);
            }

            if (!user.IsInstitutionAdmin() && _authenticationService.AutoLockUser(user))
            {
                //TODO: Test This.
                var message = new StringBuilder()
                    .Append("Login has been locked out due to inactivity.<br/>")
                    .AppendFormat(
                        "Please <a href=\"{0}\">Click Here</a> to contact your Administrator to unlock your account.",
                        Url.Action("UnlockAccount", "Contact",
                            new { UserId = user.Id, InstitutionId = user.Institution.Id }))
                    .ToString();
                results.ErrorMessage = message;
            }
            else
            {
                var newPassword = _userService.UserGenerateRandomPassword(user);
                var messageBody = new StringBuilder()
                    .Append("<html><body><table>")
                    .AppendFormat("<tr><td>Your R2Library password is: {0}</td></tr>", newPassword)
                    .Append(
                        "<tr><td>You can change your password once logged in, by clicking on your name in the top right.</td></tr>")
                    .Append("</table></body><html>")
                    .ToString();
                var emailPage = new EmailPage
                {
                    Subject = "R2 Library Forgot Password",
                    To = user.Email
                };

                results.Successful = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                if (!results.Successful)
                {
                    results.ErrorMessage = "System error, please contact administrator";
                }
            }

            return Json(results);
        }

        private AuthenticationResult Authenticate(LoginParam login)
        {
            var countryCode = CountryCodeService.GetCountryCodeFromIpAddressFromDb(Request.GetHostIpAddress(),
                HttpContext.ApplicationInstance.Context);

            var institutionId = _authenticationContext.IsAuthenticated
                ? _authenticationContext.AuthenticatedInstitution.Id
                : 0;

            var authenticationResult =
                _authenticationService.Login(login.UserName, login.Password, Request, institutionId, countryCode);

            if (authenticationResult.WasSuccessful)
            {
                AuthenticationContext.Set(authenticationResult.AuthenticatedInstitution);
                if (!string.IsNullOrWhiteSpace(login.AthensTargetedId))
                {
                    _athensAuthenticationService.InsertAthensUserTargetedId(CurrentUser, login.AthensTargetedId);
                }
            }

            return authenticationResult;
        }

        private string GetInstitutionHomepage(HomePage homePage)
        {
            switch (homePage)
            {
                case HomePage.Titles:
                case HomePage.Discipline:
                    return Url.Action("Index", "Browse");
                case HomePage.AtoZIndex:
                    return Url.Action("Index", "AlphaIndex");
                default:
                    return Url.Action("Index", "Home");
            }
        }

        public ActionResult LogOut()
        {
            var redirectUrl = Url.Action<HomeController>(a => a.Index());
            if (AuthenticatedInstitution != null)
            {
                if (AuthenticatedInstitution.LogoutUrl.IsNotEmpty())
                {
                    redirectUrl = AuthenticatedInstitution.LogoutUrl;
                }
            }

            var claims = GetClaims();
            if (claims != null && Request.IsAuthenticated)
            {
                var authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                HttpContext.GetOwinContext().Authentication
                    .SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
            }

            AthensAuthenticationCookie.ClearAthensCookie(Response);
            _passiveAuthenticationCookieService.ClearCookie();

            Session.Abandon();
            return new RedirectResult(redirectUrl);
        }

        public ActionResult NoAccess(string accessCode, string redirectUrl)
        {
            var noAccess = new NoAccess();

            var code = CovertStringToAccessCode(accessCode);

            noAccess.Message = code.ToDescription();

            _log.Warn("Received request for No Access\n{0}".Args(Request.Details()));

            return View(noAccess);
        }

        private AccessCode? CovertStringToAccessCode(string codeString)
        {
            if (string.IsNullOrWhiteSpace(codeString))
            {
                return null;
            }

            var codeLower = codeString.ToLower();

            if (codeLower.Equals(AccessCode.Allowed.ToLower()))
            {
                return AccessCode.Allowed;
            }

            if (codeLower.Equals(AccessCode.Unauthenticated.ToLower()))
            {
                return AccessCode.Unauthenticated;
            }

            if (codeLower.Equals(AccessCode.Unauthorized.ToLower()))
            {
                return AccessCode.Unauthorized;
            }

            if (codeLower.Equals(AccessCode.UnauthorizedAthens.ToLower()))
            {
                return AccessCode.UnauthorizedAthens;
            }

            if (codeLower.Equals(AccessCode.UnknownParameters.ToLower()))
            {
                return AccessCode.UnknownParameters;
            }

            return null;
        }

        private string GetAthensDeepLink()
        {
            //Check if this is an Athens deep linking request

            string result = null;
            var iss = Request.QueryString["iss"];
            var targetLinkUri = Request.QueryString["target_link_uri"];

            if (!string.IsNullOrEmpty(targetLinkUri) && UriHelper.IsMatch(iss, _oidcSettings.Authority))
            {
                result = targetLinkUri;
            }

            return result;
        }

        private string GetAthensReferrer()
        {
            string result = null;
            var referer = Request.ServerVariables["http_referer"];

            if (referer != null)
            {
                //Return the HTTP referrer but only if it's not coming from the domain of the OIDC authority
                var compare = UriHelper.IsDomainMatch(referer, _oidcSettings.AuthorityDomain);

                if (!compare)
                {
                    result = referer;
                }
            }

            return result;
        }

        public void AthensLogin()
        {
            var athensDeepLink = GetAthensDeepLink();
            if (!string.IsNullOrEmpty(athensDeepLink))
            {
                Session["AthensRequestedUrl"] = GetAthensDeepLink();
            }

            if (Session["AthensRequestedUrl"] == null)
            {
                Session["AthensRequestedUrl"] = GetAthensReferrer();
            }

            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge();
                Response.SuppressFormsAuthenticationRedirect = true;
                return;
            }

            var claims = GetClaims();
            if (claims != null)
            {
                SetAthensCookie(claims);
            }
        }

        private List<Claim> GetClaims()
        {
            return (User.Identity as ClaimsIdentity)?.Claims.ToList();
        }

        private void SetAthensCookie(IReadOnlyCollection<Claim> claims)
        {
            _log.Info(">>> Attempting Athens Login");


            var claimPreferredUsername = claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "";
            var claimEduPersonScopedAffiliation =
                claims.FirstOrDefault(c => c.Type == "eduPersonScopedAffiliation")?.Value ?? "";
            var claimEduPersonTargetedId = claims.FirstOrDefault(c => c.Type == "eduPersonTargetedID")?.Value ?? "";
            var claimOrganization = claims.FirstOrDefault(c => c.Type == "organisation")?.Value ?? "";

            var timeStamp = DateTime.Now;
            var year = timeStamp.Year;
            var month = timeStamp.Month;
            var day = timeStamp.Day;
            var hour = timeStamp.Hour;
            var minute = timeStamp.Minute;
            var second = timeStamp.Second;

            var formatedTimeStamp = $"{month}/{day}/{year} {hour}:{minute}:{second}";

            var athensCookie = new HttpCookie("athensAuthentication")
            {
                Value =
                    $"organizationId={claimOrganization}|persistentUid=|username={claimPreferredUsername}|scopedAffiliation={claimEduPersonScopedAffiliation}|targetedId={claimEduPersonTargetedId}|formatedDate={formatedTimeStamp}",
                Expires = DateTime.MinValue,
                Secure = true
            };
            _log.Info("Cookie set");
            _log.Info(athensCookie.Value);
            Response.Cookies.Add(athensCookie);


            _log.Info("<<<< Athens Login Complete");

            Response.Redirect("/Authentication/Athens");
        }

        public ActionResult Athens()
        {
            try
            {
                var authenticationResult =
                    _athensAuthenticationService.AttemptAthensUserAuthentication(Request, Response);
                if (authenticationResult.WasSuccessful)
                {
                    AuthenticationContext.Set(authenticationResult.AuthenticatedInstitution);

                    var homePageUrl = GetInstitutionHomepage(authenticationResult.AuthenticatedInstitution.HomePage);

                    if (CurrentUser == null)
                    {
                        var cookie = new AthensAuthenticationCookie();
                        if (cookie.ScopedAffiliation != null)
                        {
                            var model = new AthensModel
                            {
                                AthensAffiliation = AuthenticatedInstitution.AthensAffiliation,
                                AthensTargetedId = authenticationResult.AthensTargetedId,
                                RedirectUrl = Session["AthensRequestedUrl"]?.ToString() ??
                                              (homePageUrl ?? Url.Action("Index", "Browse"))
                            };

                            return new RedirectResult(model.RedirectUrl);
                        }
                    }

                    var athensRequestedUrl = Session["AthensRequestedUrl"];
                    if (athensRequestedUrl != null)
                    {
                        _log.InfoFormat("----------***********+++++++++++  AthensRequestedUrl:{0}",
                            athensRequestedUrl.ToString());
                        return new RedirectResult(athensRequestedUrl.ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(homePageUrl))
                    {
                        return new RedirectResult(homePageUrl);
                    }

                    return RedirectToRoute(new RouteValueDictionary
                        { { "action", "Index" }, { "controller", "Browse" } });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return RedirectToAction("NoAccess",
                new { accessCode = AccessCode.UnauthorizedAthens.ToLower(), redirectUrl = "" });
        }

        [HttpPost]
        public ActionResult LinkAthens(LoginParam loginInfo)
        {
            var authenticationResult = Authenticate(loginInfo);
            if (authenticationResult.WasSuccessful)
            {
                var authResult = _authenticationService.ReloadUser(CurrentUser.Id);
                _authenticationContext.Set(authResult.AuthenticatedInstitution);
                return Redirect(loginInfo.RedirectUrl);
            }

            var model = new AthensModel
            {
                AthensAffiliation = AuthenticatedInstitution.AthensAffiliation,
                AthensTargetedId = loginInfo.AthensTargetedId,
                RedirectUrl = loginInfo.RedirectUrl,
                ErrorMessage = GetLoginMessage(authenticationResult)
            };
            return View("Athens", model);
        }

        /// <summary>
        ///     Used to allow users from Rittenhouse.com to view titles directly from Rittenhouse.com without any extra steps.
        ///     They will be challenged to login before they go the CollectionMangementController
        /// </summary>
        public ActionResult Title(string query, string accountnumber)
        {
            var resource = _resourceService.GetResource(query);
            var institutionId = _institutionService.GetInstitutionId(accountnumber);

            //If the resource is not found do not transfer them anywhere
            if (resource == null)
            {
                var errorMessage =
                    $"Sorry we were not able to locate the title with Isbn: {query}. Please use the search box to try again.";
                return View(new TitleAuthenticate(errorMessage));
            }

            var publicRouteValueDictionary = new RouteValueDictionary
                { { "Isbn", resource.Isbn }, { "area", "" } };

            var adminRouteValueDictionary = new RouteValueDictionary
            {
                { "Query", resource.Isbn },
                { "InstitutionId", institutionId },
                { "area", "Admin" }
            };
            //If the instutitution cannot be found direct them to the front end view of the book.
            if (institutionId == 0)
            {
                return RedirectToAction("Title", "Resource", publicRouteValueDictionary);
            }

            //If they are already an authenicated IA, RA, or SA direct them right away
            if (CurrentUser != null)
            {
                if (CurrentUser.IsInstitutionAdmin() || CurrentUser.IsRittenhouseAdmin() ||
                    CurrentUser.IsSalesAssociate())
                {
                    //Force the IA to go to there own institution
                    if (CurrentUser.IsInstitutionAdmin())
                    {
                        adminRouteValueDictionary.Remove("InstitutionId");
                        adminRouteValueDictionary.Add("InstitutionId", CurrentUser.InstitutionId);
                    }

                    return RedirectToAction("List", "CollectionManagement", adminRouteValueDictionary);
                }

                //They do not have access to admin area so direct them to the public view
                return RedirectToAction("Title", "Resource", publicRouteValueDictionary);
            }

            //Need to store the routes to TempData so once they login I can direct them to the proper place.
            //Without this would need to modify Login procedure to grab this URL instead of the last URL the user was on.

            TempData.AddItem("PublicRouteValueDictionary", publicRouteValueDictionary);
            TempData.AddItem("AdminRouteValueDictionary", adminRouteValueDictionary);


            return View(new TitleAuthenticate(resource, publicRouteValueDictionary));
        }

        private string GetTitleRedirect(IUser user)
        {
            var publicRouteValueDictionary = TempData.GetItem<RouteValueDictionary>("PublicRouteValueDictionary");
            var adminRouteValueDictionary = TempData.GetItem<RouteValueDictionary>("AdminRouteValueDictionary");

            TempData.DeleteItem("PublicRouteValueDictionary");
            TempData.DeleteItem("AdminRouteValueDictionary");

            if (publicRouteValueDictionary == null || adminRouteValueDictionary == null)
            {
                return null;
            }

            if (user.IsInstitutionAdmin() || user.IsRittenhouseAdmin() || user.IsSalesAssociate())
            {
                if (user.IsInstitutionAdmin())
                {
                    adminRouteValueDictionary.Remove("InstitutionId");
                    adminRouteValueDictionary.Add("InstitutionId", user.InstitutionId);
                }

                return GetAdminTitleRedirect(adminRouteValueDictionary);
            }

            return GetPublicTitleRedirect(publicRouteValueDictionary);
        }

        private string GetAdminTitleRedirect(RouteValueDictionary adminRouteValueDictionary)
        {
            var urlHelper = new UrlHelper(Request.RequestContext);
            return urlHelper.Action("List", "CollectionManagement", adminRouteValueDictionary);
        }

        private string GetPublicTitleRedirect(RouteValueDictionary publicRouteValueDictionary)
        {
            var urlHelper = new UrlHelper(Request.RequestContext);
            return urlHelper.Action("Title", "Resource", publicRouteValueDictionary);
        }
    }
}