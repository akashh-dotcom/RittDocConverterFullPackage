#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.MyR2;
using R2V2.Core.RequestLogger;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Content;
using R2V2.Core.Resource.Topic;
using R2V2.Core.Tabers;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Exceptions;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Resource;
using R2V2.Web.Services;

#endregion

namespace R2V2.Web.Controllers
{
    public class ResourceController : R2BaseController
    {
        private const string RecaptchaCacheKey = "Recaptcha.Request";
        private const string RecaptchaErrorCacheKey = "Recaptcha.Error";
        private const string PrintLogImageFile = "logging.png";
        private readonly IClientSettings _clientSettings;
        private readonly IContentService _contentService;
        private readonly EmailSiteService _emailService;
        private readonly InstitutionCrawlerBypassFactory _institutionCrawlerBypassFactory;
        private readonly InstitutionResourceEmailDataService _institutionResourceEmailDataService;
        private readonly IInstitutionSettings _institutionSettings;

        private readonly ILog<ResourceController> _log;
        private readonly MyR2Service _myR2Service;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourceLockService _resourceLockService;
        private readonly IResourceService _resourceService;
        private readonly TabersDictionaryService _tabersDictionaryService;
        private readonly TopicService _topicService;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public ResourceController(
            ILog<ResourceController> log
            , IAuthenticationContext authenticationContext
            , IContentService contentService
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
            , EmailSiteService emailService
            , IQueryable<AZIndex> azIndex
            , IClientSettings clientSettings
            , MyR2Service myR2Service
            , IUserSessionStorageService userSessionStorageService
            , TabersDictionaryService tabersDictionaryService
            , InstitutionCrawlerBypassFactory institutionCrawlerBypassFactory
            , IResourceLockService resourceLockService
            , InstitutionResourceEmailDataService institutionResourceEmailDataService
            , IInstitutionSettings institutionSettings
            , ILocalStorageService localStorageService
            , TopicService topicService
        )
            : base(authenticationContext)
        {
            _log = log;
            _contentService = contentService;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
            _emailService = emailService;
            _clientSettings = clientSettings;
            _myR2Service = myR2Service;
            _userSessionStorageService = userSessionStorageService;
            _tabersDictionaryService = tabersDictionaryService;
            _institutionCrawlerBypassFactory = institutionCrawlerBypassFactory;
            _resourceLockService = resourceLockService;
            _institutionResourceEmailDataService = institutionResourceEmailDataService;
            _institutionSettings = institutionSettings;
            _topicService = topicService;
        }

        public ActionResult Title(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new NotFoundHttpException();
            }

            if (IsTrackBack())
            {
                var urlHelper = new UrlHelper(Request.RequestContext);
                var redirectUrl = urlHelper.Action("NotFound", "Error",
                    new { aspxerrorpath = HttpContext.Request.Url?.AbsoluteUri ?? "error" });

                return new RedirectResult(redirectUrl, true);
            }

            var resource = _resourceService.GetResource(isbn);
            if (resource == null)
            {
                int resourceId;
                int.TryParse(isbn, out resourceId); // check if it's a resource Id

                resource = _resourceService.GetResource(resourceId);
                if (resource == null || resource.StatusId == (int)ResourceStatus.Inactive)
                {
                    throw new NotFoundHttpException();
                }
            }

            if (resource.StatusId == (int)ResourceStatus.Inactive)
            {
                throw new NotFoundHttpException();
            }

            if (resource.Isbn != isbn)
            {
                return RedirectToActionPermanent("Title", "Resource", new { resource.Isbn });
            }

            var resourceDetail = GetResourceDetail(resource, null, false);

            if (resourceDetail != null)
            {
                SetRequestLoggerContentView(resource, null, ContentTurnawayType.Undefined);

                if (AuthenticatedInstitution == null ||
                    AuthenticatedInstitution.AccountNumber == _institutionSettings.GuestAccountNumber)
                {
                    var specialtyId = resource.Specialties?.FirstOrDefault()?.Id ?? 0;
                    var recentlyReleasedResources =
                        _resourceService.GetRecentlyReleasedTitles(resource.Id, specialtyId, false);
                    resourceDetail.RecentlyReleasedResources = recentlyReleasedResources;
                    resourceDetail.ActionMenu = null;
                }
                else
                {
                    resourceDetail.ActionMenu = new ActionMenu
                        { BrowseSearchText = "Search this Title", ShowToc = false };
                    resourceDetail.IsInstitutionAdmin = CurrentUser?.IsInstitutionAdmin() ?? false;
                }

                //SquishList #1078 � do not create marketing pages for ACTIVE but NONSALEABLE titles
                if (resource.NotSaleable && !resourceDetail.IsFullTextAvailable)
                {
                    throw new NotFoundHttpException();
                }
            }

            return View(resourceDetail);
        }

        public ActionResult Detail(string isbn, string section, bool? showAllDictionaryTerms)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new NotFoundHttpException();
            }

            if (!IsValidSection(section))
            {
                _log.WarnFormat("Invalid request - isbn: {0}, section: {1}", isbn, section);
                throw new NotFoundHttpException();
            }

            var resource = _resourceService.GetResource(isbn);
            if (resource == null)
            {
                int resourceId;
                int.TryParse(isbn, out resourceId); // check if it's a resource Id

                resource = _resourceService.GetResource(resourceId);
                if (resource == null || resource.StatusId == (int)ResourceStatus.Inactive)
                {
                    throw new NotFoundHttpException();
                }
            }

            if (resource.StatusId == (int)ResourceStatus.Inactive)
            {
                throw new NotFoundHttpException();
            }

            if (resource.Isbn != isbn)
            {
                var url = Url.Action("Detail", "Resource", new { resource.Isbn, section });

                return new RedirectResult(url, true);
            }

            if (showAllDictionaryTerms != null)
            {
                _userSessionStorageService.Put("ShowAllDictionaryTermsKey", showAllDictionaryTerms);
            }

            var recaptchaHideContent = RecaptchaHideContent();
            var hideRecaptcha = ShouldRecaptchaBeHidden(recaptchaHideContent);

            var resourceDetail = GetResourceDetail(resource, section, false, recaptchaHideContent);
            resourceDetail.ResourceTimeoutInMinutes = _clientSettings.ResourceTimeoutInMinutes;
            resourceDetail.ResourceTimeoutModalInSeconds = _clientSettings.ResourceTimeoutModalInSeconds;

            resourceDetail.HideRecaptcha = hideRecaptcha;

            return View(resourceDetail);
        }

        [NonGoogleCaptchaValidation]
        [HttpPost]
        public ActionResult Detail(string isbn, string section, bool? showAllDictionaryTerms, bool captchaValid,
            string captchaErrorMessage)
        {
            if (IsCaptchaValid(captchaValid))
            {
                _userSessionStorageService.Remove(RecaptchaCacheKey);
                _userSessionStorageService.Remove(RecaptchaErrorCacheKey);
            }
            else if (!string.IsNullOrWhiteSpace(captchaErrorMessage))
            {
                ModelState.AddModelError("HideContent", captchaErrorMessage);
            }

            return Detail(isbn, section, showAllDictionaryTerms);
        }

        public ActionResult ReviewedTitle(string isbn, string type)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new NotFoundHttpException();
            }

            if (IsTrackBack())
            {
                var urlHelper = new UrlHelper(Request.RequestContext);
                var redirectUrl = urlHelper.Action("NotFound", "Error",
                    new { aspxerrorpath = HttpContext.Request.Url?.AbsoluteUri ?? "error" });

                return new RedirectResult(redirectUrl, true);
            }

            var resource = _resourceService.GetResource(isbn);
            if (resource == null)
            {
                int resourceId;
                int.TryParse(isbn, out resourceId); // check if it's a resource Id

                resource = _resourceService.GetResource(resourceId);
                if (resource == null || resource.StatusId == (int)ResourceStatus.Inactive)
                {
                    throw new NotFoundHttpException();
                }
            }

            if (resource.StatusId == (int)ResourceStatus.Inactive)
            {
                throw new NotFoundHttpException();
            }

            var specialtyId = resource.Specialties?.FirstOrDefault()?.Id ?? 0;
            var resourceDetail = GetResourceDetail(resource, null, false);
            var recentlyReleasedResources = _resourceService.GetRecentlyReleasedTitles(resource.Id, specialtyId, false);

            var model = new ReviewedTitleModel
            {
                ResourceDetail = resourceDetail,
                RecentlyReleasedResources = recentlyReleasedResources,
                Type = type
            };

            return View(model);
        }

        private bool RecaptchaHideContent()
        {
            try
            {
                if (Request.GetHostIpAddress() != null)
                {
                    var ipAddress = IPAddress.Parse(Request.GetHostIpAddress());
                    var clientIpNumber = ipAddress.ToIpNumber();


                    var institutionCrawlerBypass =
                        _institutionCrawlerBypassFactory.GetInstitutionCrawlerBypass(clientIpNumber, Request.UserAgent);

                    if (institutionCrawlerBypass != null &&
                        institutionCrawlerBypass.InstitutionId == CurrentUser.InstitutionId)
                    {
                        return false;
                    }
                }

                var contentRequests = _userSessionStorageService.Get<List<DateTime>>(RecaptchaCacheKey) ??
                                      new List<DateTime>();

                contentRequests.Add(DateTime.Now);

                //Only holds contentRequests that are within the timeframe.
                var newContentRequests = (from contentRequest in contentRequests
                    let contentRequestTime = (int)(DateTime.Now - contentRequest).TotalSeconds
                    where contentRequestTime <= _clientSettings.RecaptchaRequestTimeInSeconds
                    select contentRequest).ToList();

                if (newContentRequests.Count >= _clientSettings.RecaptchaNumberOfRequests)
                {
                    var recaptchaPopDateTimes =
                        _userSessionStorageService.Get<List<DateTime>>(RecaptchaErrorCacheKey) ?? new List<DateTime>();
                    recaptchaPopDateTimes.Add(DateTime.Now);

                    _log.DebugFormat("recaptchaPopDateTimes.Count: {0}, ", recaptchaPopDateTimes.Count);
                    if (recaptchaPopDateTimes.Count == 5 ||
                        recaptchaPopDateTimes.Count % _clientSettings.RecaptchaNumberOfRequests == 0)
                    {
                        var authenticatedInstitution = AuthenticatedInstitution;
                        var message = new StringBuilder()
                            .Append("The Threshold for Content Requests has been reached.").AppendLine()
                            .AppendFormat("\tInstitution: {0}, {1}, ", authenticatedInstitution.AccountNumber,
                                authenticatedInstitution.Name)
                            .AppendLine();
                        if (authenticatedInstitution.User != null)
                        {
                            message.AppendFormat("\tUser: {0}, {1}, ", authenticatedInstitution.User.Id,
                                authenticatedInstitution.User.ToFullName());
                        }
                        else
                        {
                            message.Append("\tUser: null, ");
                        }

                        message.AppendLine();
                        message.AppendFormat("\tURL: {0}, ", Request.RawUrl).AppendLine();
                        message.AppendFormat("\tIP: {0}, ", Request.GetHostIpAddress()).AppendLine();
                        message.AppendFormat("\tSessionID: {0}, ", Session.SessionID).AppendLine();
                        message.AppendFormat("\tUserAgent: {0}, ", Request.UserAgent).AppendLine();
                        message.AppendFormat("\tRecaptchaNumberOfRequests: {0}, ",
                            _clientSettings.RecaptchaNumberOfRequests).AppendLine();
                        message.AppendFormat("\tRecaptchaRequestTimeInSeconds: {0}, ",
                            _clientSettings.RecaptchaRequestTimeInSeconds).AppendLine();
                        message.AppendFormat("\tFailed Recaptcha Attempts: {0}, ",
                            newContentRequests.Count - _clientSettings.RecaptchaNumberOfRequests).AppendLine();
                        message.AppendFormat("\tRecaptcha Count: {0}", recaptchaPopDateTimes.Count);
                        _log.ContentThresholdWarn(message.ToString());
                    }

                    _userSessionStorageService.Put(RecaptchaErrorCacheKey, recaptchaPopDateTimes);

                    return true;
                }

                _userSessionStorageService.Put(RecaptchaCacheKey, newContentRequests);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.Message, ex);
            }

            return false;
        }

        private bool ShouldRecaptchaBeHidden(bool isRecaptchaShown)
        {
            if (isRecaptchaShown)
            {
                var recaptchaPopDateTimes = _userSessionStorageService.Get<List<DateTime>>(RecaptchaErrorCacheKey) ??
                                            new List<DateTime>();
                if (recaptchaPopDateTimes.Count >= 10)
                {
                    return true;
                }
            }

            return false;
        }

        private ResourceDetail GetResourceDetail(IResource resource, string section, bool isEmailRequest,
            bool hideContent = false)
        {
            var isSectionRequest = !string.IsNullOrEmpty(section);

            ResourceDetail resourceDetail = null;
            var isbn = resource.Isbn;

            try
            {
                resourceDetail = resource.ToResourceDetail();
                resourceDetail.HideContent = hideContent;
                resourceDetail.Section = section;

                resourceDetail.ProxyPrefix = AuthenticatedInstitution?.ProxyPrefix;

                var resourceUrl = Url.Action("Title", "Resource", new { resourceDetail.Isbn },
                    HttpContext.Request.IsSecureConnection ? "https" : "http");

                resourceDetail.LinkUrl = resourceUrl;

                var resourceLink = $"<a href=\"{resourceUrl}\">{resourceUrl}</a>";
                resourceDetail.Citation = _resourceService.GetCitation(resource, resourceLink);

                resourceDetail.ProCiteCitation = _resourceService.GetProciteCitation(resource, resourceDetail.LinkUrl);
                resourceDetail.EndNoteCitation = _resourceService.GetEndNoteCitation(resource, resourceDetail.LinkUrl);
                resourceDetail.RefWorksCitation =
                    _resourceService.GetRefWorksCitation(resource, resourceDetail.LinkUrl);
                resourceDetail.ApaCitation = _resourceService.GetApaFormatCitation(resource, resourceDetail.LinkUrl);

                var resourceAccess = isSectionRequest
                    ? _resourceAccessService.GetResourceAccess(isbn)
                    : _resourceAccessService.GetResourceAccessForToc(isbn);

                resourceDetail.ActionMenu = new ActionMenu
                    { BrowseSearchText = "Browse/Search this Title", ShowToc = true };

                var toc = _contentService.GetTableOfContents(isbn, GetBaseUrl(), resourceAccess, isEmailRequest);
                resourceDetail.Toc = toc.Html;
                resourceDetail.Navigation = toc.Navigation;
                //resourceDetail.Topics = GetTopics(isbn).ToList();
                resourceDetail.Topics = _topicService.GetResourceTopics(isbn);

                resourceDetail.ContainsVideo = resource.ContainsVideo;

                if (isSectionRequest)
                {
                    var contentItem = GetContentItem(isbn, section, isEmailRequest, hideContent);
                    resourceDetail.ContentHtml = contentItem.Html;

                    if (contentItem.Navigation != null && contentItem.Navigation.Current != null &&
                        section != contentItem.Navigation.Current.Id)
                    {
                        resourceDetail.Goto = section;
                    }

                    //If isSectionRequest get new APA citation to include the section title and direct URL
                    if (contentItem.Navigation != null)
                    {
                        var resourceSectionUrl = Url.Action("Title", "Resource", new { resourceDetail.Isbn, section },
                            HttpContext.Request.IsSecureConnection ? "https" : "http");
                        resourceDetail.ApaCitation = _resourceService.GetApaFormatCitation(resource, resourceSectionUrl,
                            contentItem.Navigation.Section.Name);
                    }

                    contentItem.Html = "";
                    resourceDetail.ContentJson = SerializeContentItem(contentItem);
                }

                if (AuthenticatedInstitution != null)
                {
                    resourceDetail.UserContentFolders[UserContentType.Bookmark] =
                        _myR2Service.GetUserContentFolders(UserContentType.Bookmark, UserId,
                            AuthenticatedInstitution.Id);
                    resourceDetail.UserContentFolders[UserContentType.CourseLink] =
                        _myR2Service.GetUserContentFolders(UserContentType.CourseLink, UserId,
                            AuthenticatedInstitution.Id);
                    resourceDetail.UserContentFolders[UserContentType.Reference] =
                        _myR2Service.GetUserContentFolders(UserContentType.Reference, UserId,
                            AuthenticatedInstitution.Id);
                    resourceDetail.UserContentFolders[UserContentType.Image] =
                        _myR2Service.GetUserContentFolders(UserContentType.Image, UserId, AuthenticatedInstitution.Id);
                }
                else
                {
                    //This happens when you navigate to the TOC without being authenicated.
                    // SJS - put this in here to determine if this is happening.
                    if (isSectionRequest)
                    {
                        _log.Warn("UserContentFolders NOT BEING SET!");
                    }
                }

                resourceDetail.IsFullTextAvailable = _resourceAccessService.IsFullTextAvailable(resourceDetail.Id);

                resourceDetail.IsPdaResource = _resourceAccessService.IsPdaResource(isbn);

                resourceDetail.TurnawayMessage = resourceDetail.IsPdaResource
                    ? GetPdaTurnawayMessage(resourceAccess)
                    : GetTurnawayMessage(resourceAccess, resourceDetail.Isbn);

                resourceDetail.ResourceAccess = resourceAccess;
                _log.DebugFormat("Id: {0}, isbn: {1}/{2}, IsFullTextAvailable: {3}, ResourceAccess: {4}",
                    resourceDetail.Id, resourceDetail.Isbn10, resource.Isbn13, resourceDetail.IsFullTextAvailable,
                    resourceDetail.ResourceAccess);

                if (resource.DoodyReview == 1)
                {
                    resourceDetail.DoodyReviewUrl = $"{_clientSettings.DoodyReviewLink}{resource.Isbn}";
                }

                if (CurrentUser != null)
                {
                    resourceDetail.ToolLinks.EmailPage.From = CurrentUser.Email;
                }

                if (AuthenticatedInstitution != null)
                {
                    resourceDetail.ContentProvider = AuthenticatedInstitution.BrandingInstitutionName;
                    var requestStorage = HttpContext.RequestStorage();

                    var resourcePrintLockStatus = _resourceLockService.IsPrintEnabled(AuthenticatedInstitution.Id,
                        CurrentUser?.Id, resource, requestStorage);
                    resourceDetail.IsPrintingEnabled = !resourcePrintLockStatus.LimitReached;
                    resourceDetail.MaxNumberOfPrintRequests = resourcePrintLockStatus.LimitCount;
                    resourceDetail.NumberOfPrintRequests = 0;
                    resourceDetail.PrintWarningThresholdReached = resourcePrintLockStatus.WarningThresholdReached;
                    resourceDetail.PrintWarningThresholdPercentage = resourcePrintLockStatus.WarningThresholdPercentage;

                    var resourceEmailLockStatus = _resourceLockService.IsEmailEnabled(AuthenticatedInstitution.Id,
                        CurrentUser?.Id, resource, requestStorage);
                    if (resourceEmailLockStatus.LimitReached)
                    {
                        resourceDetail.ToolLinks.EmailPage.ErrorMessage =
                            "Email is temporarily disabled for this resource.<br/>Please contact your librarian for more information.";
                    }
                    else if (resourceEmailLockStatus.WarningThresholdReached)
                    {
                        resourceDetail.ToolLinks.EmailPage.WarningMessage = new StringBuilder()
                            .AppendFormat("Warning, this resource is limited to {0} emailed sections in 24 hours.<br/>",
                                resourceEmailLockStatus.LimitCount)
                            .AppendFormat("This resource has been emailed {0} times in the last 24 hours.<br/>",
                                resourceEmailLockStatus.RequestCount)
                            .Append("Please contact your librarian for more information.")
                            .ToString();
                    }
                }
                else
                {
                    resourceDetail.IsPrintingEnabled = false;
                    resourceDetail.MaxNumberOfPrintRequests = 0;
                    resourceDetail.NumberOfPrintRequests = 0;
                }

                resourceDetail.DictionaryTerms = GetDictionaryTerms(resourceDetail.TabersStatus);

                if (isEmailRequest)
                {
                    resourceDetail.IsEmailResource = true;
                }

                resourceDetail.Affiliation = resource.Affiliation;
                resourceDetail.DoodyRating = resource.DoodyRating;
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendFormat("Exception: {0}", ex.Message).AppendLine();
                msg.AppendLine(resource.ToDebugInfo());
                msg.AppendFormat("isSectionRequest: {0}, isEmailRequest: {1}", isSectionRequest, isEmailRequest);
                _log.Error(msg.ToString(), ex);
            }

            var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
            resourceDetail.RequestId = requestLoggerData.RequestId;

            return resourceDetail;
        }

        private string GetBaseUrl()
        {
            if (Request.Url == null)
                return "";

            // Use GetLeftPart to include scheme, host, and port (if non-standard)
            // This ensures localhost:53172 works correctly in development
            return Request.Url.GetLeftPart(UriPartial.Authority);
        }

        public ActionResult Redirect(string isbn, string section)
        {
            return string.IsNullOrWhiteSpace(section)
                ? Redirect(Url.RouteUrl(new { controller = "Resource", action = "Title", isbn }))
                : Redirect(Url.RouteUrl(new { controller = "Resource", action = "Detail", isbn, section }));
        }

        public ActionResult Link(string id, string sectionId)
        {
            int parsedId;
            int.TryParse(id, out parsedId);
            if (parsedId > 0)
            {
                var resource = _resourceService.GetResource(parsedId);
                if (resource != null)
                {
                    return string.IsNullOrWhiteSpace(sectionId)
                        ? Redirect(Url.RouteUrl(new { controller = "Resource", action = "Title", resource.Isbn }))
                        : Redirect(Url.RouteUrl(new
                            { controller = "Resource", action = "Detail", resource.Isbn, section = sectionId }));
                }
            }

            throw new NotFoundHttpException();
            // return RedirectToAction("Index", "Browse");  // return to browse index if we can't find the resource
        }

        public ActionResult Section(string isbn, string section, bool email = false)
        {
            var errorMessage = new StringBuilder()
                .Append($"Resource/Section [isbn:{isbn} ")
                .Append($"| section:{section} ")
                .Append($"| email:{email}] ")
                .Append($"Institution:{CurrentUser?.InstitutionId.ToString() ?? "N/A"}, ")
                .Append($"User:{CurrentUser?.Id.ToString() ?? "N/A"}, ")
                .Append(
                    $"Referrer:{(!string.IsNullOrWhiteSpace(Request.HttpReferrer()) ? Request.HttpReferrer() : "N/A")}, ")
                .Append($"UserAgent:{Request.UserAgent}, ")
                .Append($"UserHostAddress:{Request.GetHostIpAddress()} ")
                .ToString();
            _log.Error(errorMessage);

            var contentResult = new ContentResult
            {
                Content = SerializeContentItem(isbn, section, email),
                ContentType = "application/json"
            };
            contentResult.Content.RemoveHtml();

            return contentResult;
        }

        private DictionaryTerms GetDictionaryTerms(bool resourceTabersStatus)
        {
            return new DictionaryTerms
            {
                Enable = resourceTabersStatus && _tabersDictionaryService.IsFullTextAvailable(),
                ShowAll = _userSessionStorageService.Get<bool?>("ShowAllDictionaryTermsKey") ?? true
            };
        }

        private ContentItem GetContentItem(string isbn, string section, bool email = false, bool hideContent = false)
        {
            ContentItem contentItem;
            try
            {
                if (!string.IsNullOrWhiteSpace(isbn) && !string.IsNullOrWhiteSpace(section))
                {
                    var resourceAccess = _resourceAccessService.GetResourceAccess(isbn);
                    contentItem = _contentService.GetContent(isbn, section, GetBaseUrl(), resourceAccess, email);
                    if (contentItem != null)
                    {

                        if (contentItem?.Navigation != null)
                        {
                            _log.DebugFormat("Navigation found - Previous: {0}, Next: {1}",
                                contentItem.Navigation.Previous?.Id ?? "NULL",
                                contentItem.Navigation.Next?.Id ?? "NULL");
                        }
                        else
                        {
                            _log.WarnFormat("Navigation is NULL for section: {0}", section);
                        }

                        var contentTurnawayType = ContentTurnawayType.Undefined;
                        switch (resourceAccess)
                        {
                            case ResourceAccess.Denied:
                                contentTurnawayType = ContentTurnawayType.Access;
                                break;
                            case ResourceAccess.Locked:
                                contentTurnawayType = ContentTurnawayType.Concurrency;
                                break;
                        }

                        if (resourceAccess != ResourceAccess.Allowed)
                        {
                            contentItem.Html =
                                _resourceAccessService.IsPdaResource(isbn)
                                    ? GetPdaTurnawayMessage(resourceAccess)
                                    : GetTurnawayMessage(resourceAccess, isbn);
                        }

                        var resource = _resourceService.GetResource(isbn);
                        if (!hideContent)
                        {
                            SetRequestLoggerContentView(resource, section, contentTurnawayType);
                        }
                    }
                    else
                    {
                        contentItem = new ContentItem
                        {
                            Html =
                                "Sorry, the requested content could not be rendered at this time. Please try again later."
                        };
                    }
                }
                else
                {
                    contentItem = new ContentItem
                    {
                        Html =
                            "Invalid request. The requested content could not be rendered at this time. Please try again later."
                    };
                }
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder()
                    .AppendLine(ex.Message)
                    .AppendFormat("ISBN: {0}, section: {1}, email: {2}", isbn, section, email).AppendLine()
                    .AppendFormat("URL: {0}", Request.RawUrl);
                _log.Error(msg.ToString(), ex);
                contentItem = new ContentItem
                {
                    Html = "Sorry, the requested content could not be rendered at this time. Please try again later."
                };
            }

            return contentItem;
        }

        private string SerializeContentItem(ContentItem contentItem)
        {
            return
                new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }
                    .Serialize(contentItem);
        }

        private string SerializeContentItem(string isbn, string section, bool email = false)
        {
            var contentItem = GetContentItem(isbn, section, email);
            contentItem.Html = "This method has been deprecated.";
            return SerializeContentItem(contentItem);
        }

        private string GetTurnawayMessage(ResourceAccess resourceAccess, string isbn)
        {
            switch (resourceAccess)
            {
                case ResourceAccess.Denied:
                    return string.Format(Resources.ResourceAccessDenied,
                        $"<a href=\"{Url.Action("MyAdministrators", "Contact", new { isbn })}\">contact your administrator</a>");
                case ResourceAccess.Locked:
                    return Resources.ResourceAccessLocked;
                default:
                    return $"<a href=\"{Url.Action("Index", "Contact")}\">contact us</a>";
            }
        }

        private string GetPdaTurnawayMessage(ResourceAccess resourceAccess)
        {
            switch (resourceAccess)
            {
                case ResourceAccess.Denied:
                    return CurrentUser != null && CurrentUser.IsInstitutionAdmin()
                        ? Resources.PdaAccessDeniedAdmin
                        : Resources.PdaAccessDenied;
                case ResourceAccess.Locked:
                    return Resources.ResourceAccessLocked;
                default:
                    return string.Format(Resources.ResourceAccessDenied,
                        $"<a href=\"{Url.Action("Index", "Contact")}\">contact us</a>");
            }
        }

        public ActionResult Email(string isbn, string section, EmailPage emailPage)
        {
            if (string.IsNullOrWhiteSpace(emailPage.To))
            {
                throw new NotFoundHttpException();
            }

            string messageBody;

            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new NotFoundHttpException();
            }

            var resource = _resourceService.GetResource(isbn);
            if (resource == null)
            {
                throw new NotFoundHttpException();
            }

            if (resource.Isbn != isbn)
            {
                return RedirectToActionPermanent("Email", "Resource", new { resource.Isbn, section, emailPage });
            }

            if (resource.Isbn != isbn)
            {
                var url = Url.Action("Email", "Resource", new { resource.Isbn, section });

                return new RedirectResult(url, true);
            }

            var resourceDetail = GetResourceDetail(resource, null, true);
            if (string.IsNullOrWhiteSpace(section))
            {
                resourceDetail.Navigation = null;
                messageBody = RenderRazorViewToString("Resource", "_Title", resourceDetail);
            }
            else
            {
                var resourceAccess = _resourceAccessService.GetResourceAccess(isbn);
                var contentItem = _contentService.GetContent(isbn, section, GetBaseUrl(), resourceAccess, true);
                var resourceEmail = new ResourceEmail
                {
                    Title = resourceDetail.Title,
                    Citation = resourceDetail.Citation,
                    ContentItem = contentItem,
                    ContentProvider = resourceDetail.ContentProvider
                };

                messageBody = RenderRazorViewToString("Resource", "_Detail", resourceEmail);
            }

            //bool emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var emailSendStatus = _emailService.PutEmailMessageToQueue(messageBody, emailPage);

            SetRequestLoggerContentView(resource, ContentActionType.Email, section);

            var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);

            _institutionResourceEmailDataService.Save(emailSendStatus.EmailMessage, emailPage, resource.Id, section,
                emailSendStatus.SendToQueueSuccessfully, requestData);

            var json = emailSendStatus.SendToQueueSuccessfully
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogTurnaway(int resourceId, ResourceAccess access)
        {
            _log.DebugFormat("LogTurnaway() - resourceId: {0}, access: {1}", resourceId, access);
            var contentTurnawayType = access == ResourceAccess.Locked
                ? ContentTurnawayType.Concurrency
                : ContentTurnawayType.Access;
            //SetRequestLoggerContentView(resourceId, ContentTurnawayType.Access);
            SetRequestLoggerContentView(resourceId, contentTurnawayType);

            return Json(new JsonResponse { Status = "success", Successful = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogVideoPlay(string isbn, string section, string mediaUrl)
        {
            //Send Log to DB
            SetRequestLoggerMediaView(isbn, section, mediaUrl);
            return Json(new JsonResponse { Status = "success", Successful = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintLog(string isbn, string section)
        {
            var resource = _resourceService.GetResource(isbn);
            if (resource == null)
            {
                _log.WarnFormat("resource is null, isbn: {0}, section: {1}", isbn, section);
            }

            SetRequestLoggerContentView(resource, ContentActionType.Print, section);

            var path = Server.MapPath("~/_Static/Images");
            var fullPath = Path.Combine(path, PrintLogImageFile);
            return File(fullPath, "image/png", PrintLogImageFile);
        }

        public ActionResult LogPrintRequest(string resourceId, string section)
        {
            int resourceIdFromString;
            int.TryParse(resourceId, out resourceIdFromString);
            if (resourceIdFromString == 0)
            {
                return null;
            }

            var resource = _resourceService.GetResource(resourceIdFromString);
            SetRequestLoggerContentView(resource, ContentActionType.Print, section);

            var path = Server.MapPath("~/_Static/Images");
            const string file = "logging.png";
            var fullPath = Path.Combine(path, file);
            return File(fullPath, "image/png", file);
        }

        /// <summary>
        ///     Used to log turnaways mostly toc level
        /// </summary>
        private void SetRequestLoggerContentView(int resourceId, ContentTurnawayType contentTurnawayType)
        {
            try
            {
                var resource = _resourceService.GetResource(resourceId);
                var contentView = new ContentView
                {
                    ResourceId = resource.Id,
                    ContentTurnawayTypeId = (int)contentTurnawayType,
                    ResourceStatusId = resource.StatusId,
                    LicenseTypeId = (int)_resourceAccessService.GetLicenseType(resource.Id)
                };
                var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);
                requestData.ContentView = contentView;
                SetSearchInfo(contentView);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }


        /// <summary>
        ///     Used to log turnaways with a chapter and section
        /// </summary>
        private void SetRequestLoggerContentView(IResource resource, string chapterSectionId,
            ContentTurnawayType contentTurnawayType)
        {
            var contentView = new ContentView
            {
                ResourceId = resource?.Id ?? 0,
                ChapterSectionId = chapterSectionId,
                ContentTurnawayTypeId = (int)contentTurnawayType,
                ResourceStatusId = resource?.StatusId ?? 0,
                LicenseTypeId = (int)_resourceAccessService.GetLicenseType(resource?.Id ?? 0)
            };
            var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);
            if (requestData == null)
            {
                _log.WarnFormat(
                    "SetRequestLoggerContentView(resourceId: {0}, chapterSectionId: {1}, contentTurnawayType: {2}) - requestData is null, url: {3}",
                    contentView.ResourceId, chapterSectionId, contentTurnawayType, Request.RawUrl);
                return;
            }

            requestData.ContentView = contentView;
            SetSearchInfo(contentView);
        }

        /// <summary>
        ///     Is used to log Print and Email
        /// </summary>
        private void SetRequestLoggerContentView(IResource resource, ContentActionType contentActionType,
            string chapterSectionId = null)
        {
            var contentView = new ContentView
            {
                ResourceId = resource?.Id ?? 0,
                ChapterSectionId = chapterSectionId,
                ContentActionTypeId = (int)contentActionType,
                ResourceStatusId = resource?.StatusId ?? 0,
                LicenseTypeId = (int)_resourceAccessService.GetLicenseType(resource?.Id ?? 0)
            };
            var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);
            requestData.ContentView = contentView;
            SetSearchInfo(contentView);
        }

        private void SetRequestLoggerMediaView(string isbn, string chapterSectionId, string mediaFileName)
        {
            try
            {
                var resource = _resourceService.GetResource(isbn);
                if (resource != null)
                {
                    var mediaView = new MediaView
                    {
                        ResourceId = resource.Id,
                        MediaFileName = mediaFileName,
                        ChapterSectionId = chapterSectionId
                    };
                    var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);
                    requestData.MediaView = mediaView;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void SetSearchInfo(ContentView contentView)
        {
            Uri referrer = null;
            try
            {
                referrer = Request.UrlReferrer;
            }
            catch (UriFormatException ufEx)
            {
                _log.Info(ufEx.Message, ufEx);
            }

            if (referrer != null && referrer.AbsolutePath.ToLower() == "/search")
            {
                contentView.FoundFromSearch = true;

                var queryParts = HttpUtility.ParseQueryString(referrer.Query);
                if (queryParts["q"] != null)
                {
                    contentView.SearchTerm = queryParts["q"];
                }
            }

            _log.Debug(contentView.ToDebugString());
        }

        private bool IsValidSection(string section)
        {
            if (string.IsNullOrWhiteSpace(section))
            {
                return true;
            }

            section = section.ToLower();
            if (section.EndsWith(".jpg") || section.EndsWith(".gif") || section.EndsWith(".png") ||
                section.EndsWith(".jpeg") ||
                section.Equals("trackback"))
            {
                return false;
            }

            return true;
        }

        private bool IsTrackBack()
        {
            if (HttpContext != null && HttpContext.Request.Url != null)
            {
                var url = HttpContext.Request.Url.AbsoluteUri;
                var culture = CultureInfo.CreateSpecificCulture("en-US");

                if (culture.CompareInfo.IndexOf(url, "Trackback", CompareOptions.IgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}