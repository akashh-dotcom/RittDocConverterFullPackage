#region

using System;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Authentication;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Models;
using R2V2.Web.Models.Authentication;
using R2V2.Web.Models.Profile;
using UserService = R2V2.Web.Services.UserService;

#endregion

namespace R2V2.Web.Controllers
{
    public class ProfileController : R2BaseController
    {
        private readonly AthensAuthenticationService _athensAuthenticationService;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly Core.UserService _coreUserService;
        private readonly EmailSiteService _emailService;
        private readonly IEmailSettings _emailSettings;
        private readonly ExpertReviewerRequestEmailBuildService _expertReviewerRequestEmailBuildService;
        private readonly UserService _userService;

        public ProfileController(IAuthenticationContext authenticationContext
            , IAuthenticationService authenticationService
            , Core.UserService coreUserService
            , UserService userService
            , IEmailSettings emailSettings
            , EmailSiteService emailService
            , ExpertReviewerRequestEmailBuildService expertReviewerRequestEmailBuildService
            , AthensAuthenticationService athensAuthenticationService
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _authenticationService = authenticationService;
            _userService = userService;
            _emailSettings = emailSettings;
            _emailService = emailService;
            _expertReviewerRequestEmailBuildService = expertReviewerRequestEmailBuildService;
            _athensAuthenticationService = athensAuthenticationService;
            _coreUserService = coreUserService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            //string urlReferrer = HttpContext.Request.UrlReferrer != null
            //             ? HttpContext.Request.UrlReferrer.AbsolutePath
            //             : null;
            var urlReferrer = HttpContext.Request.HttpReferrer();

            var user = AuthenticatedInstitution != null ? AuthenticatedInstitution.User : null;

            if (user != null)
            {
                var userEdit = user.ToUserEdit(_coreUserService.GetListDepartments());

                userEdit.DisplayExpertReviewerEmailOptions = AuthenticatedInstitution.ExpertReviewerUserEnabled;

                if (urlReferrer != null && urlReferrer.Contains("NoAccess"))
                {
                    urlReferrer = Url.Action("Index", "Home");
                }

                var profileEdit = new ProfileEdit
                {
                    User = userEdit,
                    UrlReferrer = urlReferrer ?? Url.Action("Index", "Home"),
                    IsExpertReviewerEnabled = AuthenticatedInstitution.ExpertReviewerUserEnabled
                };

                return View(profileEdit);
            }

            return Redirect(Url.Action("NoAccess", "Authentication",
                new { accessCode = AccessCode.Unauthenticated.ToLower(), redirectUrl = Url.Action("Index") }));
        }

        [HttpPost]
        public ActionResult Index(ProfileEdit profileEdit)
        {
            if (ModelState.IsValid)
            {
                var sendRequest = profileEdit.User.ExpertReviewerRequest &&
                                  profileEdit.User.ExpertReviewerRequestDate == null;

                var user = _userService.ConvertToCoreUser(profileEdit);

                if (!string.IsNullOrWhiteSpace(profileEdit.User.CurrentPassword) &&
                    !PasswordService.IsSlowPasswordCorrect(profileEdit.User.CurrentPassword, user.PasswordHash,
                        user.PasswordSalt))
                {
                    ModelState.AddModelError("User.CurrentPassword", Resources.CurrentPasswordIncorrect);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(profileEdit.User.CurrentPassword) &&
                        //profileEdit.User.CurrentPassword == user.Password &&
                        !string.IsNullOrWhiteSpace(profileEdit.User.NewPassword) &&
                        !string.IsNullOrWhiteSpace(profileEdit.User.ConfirmPassword) &&
                        profileEdit.User.NewPassword == profileEdit.User.ConfirmPassword)
                    {
                        //user.Password = profileEdit.User.NewPassword;
                        user.LoginAttempts = 0;
                        user.LastPasswordChange = DateTime.Now;

                        user.PasswordSalt = PasswordService.GenerateNewSalt();
                        user.PasswordHash = PasswordService.GenerateSlowPasswordHash(profileEdit.User.NewPassword,
                            user.PasswordSalt);
                    }

                    _coreUserService.SaveUser(user);

                    if (sendRequest)
                    {
                        SendExpertReviewerRequest(user);
                    }

                    var authResult = _authenticationService.ReloadUser(user.Id);
                    _authenticationContext.Set(authResult.AuthenticatedInstitution);

                    if (!string.IsNullOrWhiteSpace(profileEdit.UrlReferrer))
                    {
                        //TODO: Can't redirect back into a resource. The resource gets locked. 
                        if (profileEdit.UrlReferrer.ToLower().Contains("resource"))
                        {
                            return RedirectToAction("Index", "Browse", new { Area = "" });
                        }

                        return Redirect(profileEdit.UrlReferrer);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            profileEdit.User.Departments = _coreUserService.GetListDepartments();
            profileEdit.IsExpertReviewerEnabled = AuthenticatedInstitution.ExpertReviewerUserEnabled;

            return View(profileEdit);
        }

        public ActionResult Email()
        {
            var user = AuthenticatedInstitution != null ? AuthenticatedInstitution.User : null;
            if (user != null)
            {
                var model = new UserEmailOptions(AuthenticatedInstitution);

                return View(model);
            }

            return Redirect(Url.Action("NoAccess", "Authentication",
                new { accessCode = AccessCode.Unauthenticated.ToLower(), redirectUrl = Url.Action("Index") }));
        }

        [HttpPost]
        public ActionResult Email(UserEmailOptions emailOptions)
        {
            var user = AuthenticatedInstitution != null ? AuthenticatedInstitution.User : null;
            if (user != null)
            {
                _userService.SaveUserEmailOptions(user, emailOptions);

                var authResult = _authenticationService.ReloadUser(user.Id);
                _authenticationContext.Set(authResult.AuthenticatedInstitution);

                return RedirectToAction("Index");
            }

            return Redirect(Url.Action("NoAccess", "Authentication",
                new { accessCode = AccessCode.Unauthenticated.ToLower(), redirectUrl = Url.Action("Index") }));
        }

        public void SendExpertReviewerRequest(User user)
        {
            var requestUsers = _coreUserService.GetExpertReviewerRequestUsers(AuthenticatedInstitution.Id);

            var toEmailAddresses = new StringBuilder();
            foreach (var requestUser in requestUsers)
            {
                toEmailAddresses.AppendFormat("{0};", requestUser.Email);
            }

            if (!string.IsNullOrWhiteSpace(toEmailAddresses.ToString()))
            {
                var emailBody =
                    _expertReviewerRequestEmailBuildService.BuildRequestEmailBody(user, AuthenticatedInstitution);

                var emailPage = new EmailPage
                {
                    To = _emailSettings.SendToCustomers
                        ? toEmailAddresses.ToString()
                        : _emailSettings.TestEmailAddresses,
                    Subject = "R2 Library Expert Reviewer User Request"
                };

                _emailService.SendEmailMessageToQueue(emailBody, emailPage, user);
            }
        }

        public ActionResult Add()
        {
            var institution = AuthenticatedInstitution;
            if (institution != null && (institution.AccessType == AccessType.IpValidationOpt ||
                                        institution.AccessType == AccessType.IpValidationReq))
            {
                var profileAdd = new ProfileAdd
                {
                    User = new UserEdit
                        { Departments = _coreUserService.GetListDepartments(), InstitutionId = institution.Id },
                    IsExpertReviewerEnabled = institution.ExpertReviewerUserEnabled
                };
                return View(profileAdd);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddAthens(string targetedId)
        {
            var institution = AuthenticatedInstitution;
            if (institution != null && (institution.AccessType == AccessType.IpValidationOpt ||
                                        institution.AccessType == AccessType.IpValidationReq))
            {
                var profileAdd = new ProfileAdd
                {
                    User = new UserEdit
                    {
                        Departments = _coreUserService.GetListDepartments(), InstitutionId = institution.Id,
                        AthensTargetedId = targetedId
                    },
                    IsExpertReviewerEnabled = institution.ExpertReviewerUserEnabled
                };
                return View("Add", profileAdd);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Add(ProfileAdd profileAdd)
        {
            if (ModelState.IsValid)
            {
                if (_coreUserService.DoesUserNameAlreadyExist(profileAdd.UserName))
                {
                    ModelState.AddModelError("UserName", Resources.UserNameAlreadyInUse);
                }

                if (!string.IsNullOrWhiteSpace(profileAdd.User.AthensTargetedId) &&
                    _coreUserService.DoesAthensTargetedIdAlreadyExist(profileAdd.User.AthensTargetedId))
                {
                    ModelState.AddModelError("UserName",
                        @"The Athens identifier is already associated to another account");
                }
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (ModelState.IsValid)
            {
                if (authenticatedInstitution == null ||
                    authenticatedInstitution.AccessType != AccessType.IpValidationOpt &&
                    authenticatedInstitution.AccessType != AccessType.IpValidationReq)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (profileAdd.User == null)
                {
                    profileAdd.User = new UserEdit();
                }

                profileAdd.User.InstitutionId = authenticatedInstitution.Id;

                var newUser = _userService.ConvertToNewCoreUser(profileAdd);
                //newUser.Password = profileAdd.User.NewPassword;

                _coreUserService.SaveUser(newUser);
                var sendRequest = profileAdd.User.ExpertReviewerRequest &&
                                  profileAdd.User.ExpertReviewerRequestDate == null;
                if (sendRequest)
                {
                    SendExpertReviewerRequest(newUser);
                }

                // re-authenticate as the newly created user
                var countryCode = CountryCodeService.GetCountryCodeFromIpAddressFromDb(Request.GetHostIpAddress(),
                    HttpContext.ApplicationInstance.Context);
                var authResult = _authenticationService.Login(newUser.UserName, newUser.Password, Request,
                    authenticatedInstitution.Id, countryCode);
                if (authResult.WasSuccessful)
                {
                    AuthenticationContext.Set(authResult.AuthenticatedInstitution);
                }

                return RedirectToAction("Index", "Home");
            }

            profileAdd.User.Departments = _coreUserService.GetListDepartments();
            profileAdd.IsExpertReviewerEnabled = authenticatedInstitution.ExpertReviewerUserEnabled;
            return View(profileAdd);
        }

        public ActionResult RemoveAthensLink()
        {
            if (CurrentUser != null)
            {
                _athensAuthenticationService.RemoveAthensUserTargetedId(CurrentUser);
                var authResult = _authenticationService.ReloadUser(CurrentUser.Id);
                _authenticationContext.Set(authResult.AuthenticatedInstitution);
                AthensAuthenticationCookie.ClearAthensCookie(Response);
            }

            return RedirectToAction("Index");
        }
    }
}