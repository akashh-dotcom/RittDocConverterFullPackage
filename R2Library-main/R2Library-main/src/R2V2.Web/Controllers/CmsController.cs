#region

using System;
using System.Web.Mvc;
using HtmlAgilityPack;
using R2V2.Core.Cms;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Cms;
using R2V2.Web.Models.Contact;

#endregion

namespace R2V2.Web.Controllers
{
    public class CmsController : R2BaseController
    {
        private const string CmsSecurityTokenKey = "CmsSecurityToken";
        private readonly IClientSettings _clientSettings;
        private readonly CmsService _cmsService;
        private readonly EmailSiteService _emailService;
        private readonly ILog<CmsController> _log;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public CmsController(CmsService cmsService
            , IUserSessionStorageService userSessionStorageService
            , ILog<CmsController> log
            , EmailSiteService emailService
        )
        {
            _cmsService = cmsService;
            _userSessionStorageService = userSessionStorageService;
            _log = log;
            _emailService = emailService;
        }

        private string FixImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var images = doc.DocumentNode.ChildNodes.Elements("img");
            if (images != null)
            {
                foreach (var htmlNode in images)
                {
                    var originalHtml = htmlNode.OuterHtml;
                    htmlNode.Attributes.Add("style", "max-width:820px");
                    var newHtml = htmlNode.OuterHtml;
                    html = html.Replace(originalHtml, newHtml);
                }
            }

            return html;
        }

        public ActionResult Index(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return RedirectToAction("NotFound", "Error");
            }

            var cmsHtml = new CmsHtml
            {
                Title = title
            };
            var cmsData = _cmsService.GetDiscoverPageText(title) ?? _cmsService.GetDiscoverPageText("Discover Error");

            if (cmsData.Value != null)
            {
                cmsHtml.Html = FixImages(cmsData.Value);
                if (string.IsNullOrWhiteSpace(cmsHtml.Html))
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
            else if (cmsData.FormHeader != null)
            {
                cmsHtml.FormHeader = FixImages(cmsData.FormHeader);
                cmsHtml.FormRecipients = cmsData.FormRecipients;
                cmsHtml.ContactInfo = new ContactUs(); //(AuthenticatedInstitution, CurrentUser, null);
                _userSessionStorageService.Put(CmsSecurityTokenKey, cmsHtml.ContactInfo.Token1);
            }

            return View(cmsHtml);
        }

        [HttpPost]
        [NonGoogleCaptchaValidation]
        public ActionResult Index(CmsHtml cmsHtml, bool captchaValid)
        {
            if (ModelState.IsValid && IsCaptchaValid(captchaValid) && AreSecurityTokensCorrect(cmsHtml.ContactInfo))
            {
                cmsHtml.ContactInfo.IsEmailView = true;
                var emailPage = new EmailPage
                {
                    To = cmsHtml.FormRecipients,
                    Subject = $"R2Library.com {cmsHtml.Title.Replace("_", " ")}"
                };

                var messageBody = RenderRazorViewToString("Cms", "_ContactForm", cmsHtml);
                _emailService.SendEmailMessageToQueue(messageBody, emailPage);
                return View("~/Views/Contact/Submitted.cshtml");
            }

            if (!captchaValid)
            {
                ModelState.AddModelError("RecaptchaMessage", @"Incorrect CAPTCHA code!");
            }

            return View(cmsHtml);
        }

        private bool AreSecurityTokensCorrect(ContactUs model)
        {
            try
            {
                _log.Debug(
                    $"Token1: {model.Token1}, Token2: {model.Token2}, Token3: {model.Token3}, Token4: {model.Token4}");

                // check if token is session matches token in model
                var sessionToken = _userSessionStorageService.Get<string>(CmsSecurityTokenKey);
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