#region

using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Contact;

#endregion

namespace R2V2.Web.Controllers
{
    public class ContactController : R2BaseController
    {
        private const string ContactUsSecurityTokenKey = "ContactUsSecurityToken";
        private readonly IAdminSettings _adminSettings;

        private readonly EmailSiteService _emailService;
        private readonly IEmailSettings _emailSettings;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<ContactController> _log;
        private readonly IResourceService _resourceService;
        private readonly UserService _userService;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public ContactController(
            EmailSiteService emailService
            , IAdminSettings adminSettings
            , IAuthenticationContext authenticationContext
            , IEmailSettings emailSettings
            , UserService userService
            , IResourceService resourceService
            , IInstitutionSettings institutionSettings
            , ILog<ContactController> log
            , IUserSessionStorageService userSessionStorageService
        )
            : base(authenticationContext)
        {
            _emailService = emailService;
            _adminSettings = adminSettings;
            _emailSettings = emailSettings;
            _userService = userService;
            _resourceService = resourceService;
            _institutionSettings = institutionSettings;
            _log = log;
            _userSessionStorageService = userSessionStorageService;
        }

        public ActionResult Index()
        {
            var model = new ContactUs(AuthenticatedInstitution, CurrentUser, "Contact Us");
            _userSessionStorageService.Put(ContactUsSecurityTokenKey, model.Token1);
            return View(model);
        }

        [HttpPost]
        [NonGoogleCaptchaValidation]
        public ActionResult Index(ContactUs contactUs, bool captchaValid)
        {
            if (ModelState.IsValid && IsCaptchaValid(captchaValid) && AreSecurityTokensCorrect(contactUs))
            {
                contactUs.IsEmailView = true;
                var emailPage = new EmailPage
                {
                    To = _adminSettings.ContactUsEmail,
                    Subject = "R2Library.com Contact Us"
                };

                var messageBody = RenderRazorViewToString("Contact", "_Index", contactUs);
                _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                return View("Submitted");
            }

            if (!captchaValid)
            {
                ModelState.AddModelError("RecaptchaMessage", @"Incorrect CAPTCHA code!");
            }

            return View(contactUs);
        }

        public ActionResult AskYourLibrarian()
        {
            return View(new ContactUs(AuthenticatedInstitution, CurrentUser, "Ask Your Librarian", true));
        }

        [HttpPost]
        public ActionResult AskYourLibrarian(ContactUs contactUs)
        {
            if (AuthenticatedInstitution != null)
            {
                if (ModelState.IsValid)
                {
                    var users = _userService.GetLibrarianUsers(AuthenticatedInstitution.Id);

                    contactUs.InstitutionName = AuthenticatedInstitution.Name;
                    contactUs.AccountNumber = AuthenticatedInstitution.AccountNumber;

                    var sb = new StringBuilder();

                    foreach (var user in users)
                    {
                        sb.AppendFormat("{0};", user.Email);
                    }

                    var emailToString = sb.ToString();

                    contactUs.IsEmailView = true;
                    var emailPage = new EmailPage
                    {
                        To = emailToString,
                        Subject = "Ask Your Librarian",
                        Bcc = _emailSettings.DefaultReplyToAddress
                    };

                    var messageBody = RenderRazorViewToString("Contact", "_Index", contactUs);
                    _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                    return View("SubmittedToLibrarian");
                }
            }
            else
            {
                ModelState.AddModelError("Name", @"You must be authenticated in to use this feature.");
            }

            return View(contactUs);
        }

        public ActionResult MyAdministrators(string isbn)
        {
            if (AuthenticatedInstitution != null &&
                AuthenticatedInstitution.AccountNumber != _institutionSettings.GuestAccountNumber)
            {
                var modal = new ContactUs(AuthenticatedInstitution, CurrentUser,
                    "Let Your Library Administrator Know that You Are Interested in this Title");

                if (!string.IsNullOrWhiteSpace(isbn))
                {
                    var resource = _resourceService.GetResource(isbn);
                    modal.ResourceTitle = resource.Title;
                    modal.ResourceIsbn = resource.Isbn10;
                    modal.ResourceIsbn13 = resource.Isbn13;
                }

                modal.IsAskYourLibrarian = true;
                return View(modal);
            }

            return RedirectToAction("Title", "Resource", new { isbn });
        }

        [HttpPost]
        public ActionResult MyAdministrators(ContactUs contactUs)
        {
            if (AuthenticatedInstitution != null)
            {
                if (ModelState.IsValid)
                {
                    var users = _userService.GetAdminUsers(AuthenticatedInstitution.Id);

                    contactUs.InstitutionName = AuthenticatedInstitution.Name;
                    contactUs.AccountNumber = AuthenticatedInstitution.AccountNumber;

                    var sb = new StringBuilder();

                    foreach (var user in users.Where(user => !string.IsNullOrWhiteSpace(user.Email)))
                    {
                        sb.AppendFormat("{0};", user.Email);
                    }

                    var emailToString = sb.ToString();

                    contactUs.IsEmailView = true;
                    var emailPage = new EmailPage
                    {
                        To = emailToString,
                        Subject = "Request Access to this Title"
                    };

                    var messageBody = RenderRazorViewToString("Contact", "_Index", contactUs);
                    _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                    try
                    {
                        var resource = _resourceService.GetResource(contactUs.ResourceIsbn);
                        var comment = string.IsNullOrWhiteSpace(contactUs.Comment)
                            ? null
                            : contactUs.Comment.Length > 1000
                                ? contactUs.Comment.Substring(0, 1000)
                                : contactUs.Comment;
                        var userResourceRequest = new UserResourceRequest
                        {
                            ResourceId = resource.Id,
                            Name = contactUs.Name,
                            InstitutionId = AuthenticatedInstitution.Id,
                            Title = contactUs.Title,
                            Comment = comment,
                            RecordStatus = true
                        };
                        if (CurrentUser != null)
                        {
                            userResourceRequest.UserId = CurrentUser.Id;
                        }

                        _userService.SaveUserResourceRequest(userResourceRequest);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }

                    return View("SubmittedTitleAccessRequest", contactUs);
                }
            }
            else
            {
                ModelState.AddModelError("Name", @"You must be authenticated in to use this feature.");
            }

            return View(contactUs);
        }

        public ActionResult UnlockAccount(int userId, int institutionId)
        {
            if (userId > 0 && institutionId > 0)
            {
                var user = _userService.GetUser(userId, institutionId);
                if (user != null && user.Id > 0)
                {
                    var modal = new ContactUs(user);
                    return View(modal);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [NonGoogleCaptchaValidation]
        public ActionResult UnlockAccount(ContactUs contactUs, bool captchaValid, string captchaErrorMessage)
        {
            if (ModelState.IsValid && IsCaptchaValid(captchaValid))
            {
                string emailToString;

                if (!contactUs.IsAdmin)
                {
                    var users = _userService.GetAdminUsers(contactUs.InstitutionId);

                    var emailStringBuilder = new StringBuilder();

                    foreach (var user in users.Where(user => !string.IsNullOrWhiteSpace(user.Email)))
                    {
                        emailStringBuilder.AppendFormat("{0};", user.Email);
                    }

                    emailToString = emailStringBuilder.ToString();
                }
                else
                {
                    emailToString = _adminSettings.ContactUsEmail;
                }

                contactUs.IsEmailView = true;
                var emailPage = new EmailPage
                {
                    To = emailToString,
                    Subject = "Contact Administrators",
                    Bcc = _emailSettings.DefaultReplyToAddress
                };

                var messageBody = RenderRazorViewToString("Contact", "_UnlockAccount", contactUs);
                _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                return View("SubmittedToAdministrators");
            }

            if (!captchaValid)
            {
                if (!string.IsNullOrWhiteSpace(captchaErrorMessage))
                {
                    ModelState.AddModelError("RecaptchaMessage", captchaErrorMessage);
                }
            }

            return View(contactUs);
        }

        /// <summary>
        ///     Verify tokens
        ///     Token1 is a guid that is stored in the session and must match
        ///     Token2 is the timestamp in ticks. this value should only be valid for 1 hour
        ///     Token3 is the timestamp encrypted, this make it very difficult to spoof. If toekns 1 & 2 pass the test, token 3 is
        ///     decrypted to get the value in token 2
        ///     Token4 should always be empty, if it is set we know that it is a bot
        /// </summary>
        private bool AreSecurityTokensCorrect(ContactUs model)
        {
            try
            {
                _log.Debug(
                    $"Token1: {model.Token1}, Token2: {model.Token2}, Token3: {model.Token3}, Token4: {model.Token4}");

                // check if token is session matches token in model
                var sessionToken = _userSessionStorageService.Get<string>(ContactUsSecurityTokenKey);
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