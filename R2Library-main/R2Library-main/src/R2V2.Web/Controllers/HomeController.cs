#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Cms;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models.Home;

#endregion

namespace R2V2.Web.Controllers
{
    public class HomeController : R2BaseController
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IClientSettings _clientSettings;
        private readonly CmsService _cmsService;
        private readonly IContentSettings _contentSettings;
        private readonly IFeaturedTitleService _featuredTitleService;
        private readonly ILog<HomeController> _log;
        private readonly PublisherService _publisherService;
        private readonly IResourceService _resourceService;
        private readonly IWebImageSettings _webImageSettings;

        public HomeController(
            IResourceService resourceService
            , IFeaturedTitleService featuredTitleService
            , IContentSettings contentSettings
            , IWebImageSettings webImageSettings
            , ILog<HomeController> log
            , PublisherService publisherService
            , IAuthenticationContext authenticationContext
            , IClientSettings clientSettings
            , CmsService cmsService
        )
        {
            _resourceService = resourceService;
            _featuredTitleService = featuredTitleService;
            _contentSettings = contentSettings;
            _webImageSettings = webImageSettings;
            _log = log;
            _publisherService = publisherService;
            _authenticationContext = authenticationContext;
            _clientSettings = clientSettings;
            _cmsService = cmsService;
        }

        public ActionResult Index()
        {
            try
            {
                var model = new MarketingHome { R2CmsContentUrl = _clientSettings.R2CmsContentLink };

                SetFeaturedTitle(model);
                SetFeaturedPublisher(model);
                SetRecentlyAddedTitles(model);

                model.R2LibraryCarousel = _cmsService.GetR2LibraryCarousel();

                model.HomeIntro = _cmsService.GetHomePageText("Introduction");
                model.HomePromoTop = _cmsService.GetHomePageText("Top Promotion");
                model.HomeMainContent = _cmsService.GetHomePageText("Main Content");
                model.HomeQuestions = _cmsService.GetHomePageText("Questions");
                model.HomePromoBottom = _cmsService.GetHomePageText("Bottom Promotion");

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("About");
            }
        }

        public void SetFeaturedTitle(MarketingHome model)
        {
            try
            {
                var featuredTitles = _featuredTitleService.GetFeaturedTitles()
                    .Where(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).ToList();

                var random = new Random();
                var randomNumber = random.Next(0, featuredTitles.Count);


                if (randomNumber == featuredTitles.Count)
                {
                    randomNumber = randomNumber - 1;
                }

                var featuredTitle = featuredTitles.Skip(randomNumber).Take(1).FirstOrDefault();
                if (featuredTitle == null)
                {
                    return;
                }

                model.FeaturedTitle = featuredTitle;
                //Need to make sure it is not already set or URL for the image will get messed up. 
                if (!model.FeaturedTitle.ResourceImageFileName.Contains("http"))
                {
                    model.FeaturedTitle.ResourceImageFileName =
                        featuredTitle.ResourceImageFileName.ToImageUrl(_contentSettings);
                }
            }
            catch (Exception ex)
            {
                // swallowing the exception here because we don't want shitty data or shitty code preventing the home page from displaying!
                _log.Warn(ex.Message, ex);
            }
        }

        public void SetFeaturedPublisher(MarketingHome model)
        {
            try
            {
                var featuredPublisher = _publisherService.GetFeaturedPublisher();
                model.FeaturedPublisherId = featuredPublisher.Id;
                model.FeaturedPublisherName = string.IsNullOrWhiteSpace(featuredPublisher.DisplayName)
                    ? featuredPublisher.Name
                    : featuredPublisher.DisplayName;
                model.FeaturedPublisherLogo = $"{_webImageSettings.PublisherImageUrl}{featuredPublisher.ImageFileName}";
                model.FeaturedPublisherDescription = featuredPublisher.Description;
                model.FeaturedPublisherUrl =
                    $"/Browse#include=1&publisher={featuredPublisher.Id}&type=publishers&sort-by=publication-date";
            }
            catch (Exception ex)
            {
                // swallowing the exception here because we don't want shitty data or shitty code preventing the home page from displaying!
                _log.Warn(ex.Message, ex);
            }
        }

        public void SetRecentlyAddedTitles(MarketingHome model)
        {
            var resources = _resourceService.GetAllResources().OrderByDescending(x => x.ReleaseDate).Take(5).ToList();

            foreach (var resource in resources)
            {
                resource.ImageUrl = resource.ImageFileName.ToImageUrl(_contentSettings);
            }

            if (resources.Any())
            {
                model.RecentResources = resources;
            }
        }

        public ActionResult ExportResources()
        {
            //TODO: Cannot apply ResourceStatus filter because I need Forthcoming and ACtive titles only. I also need to filter out all NotSalable titles
            var resources = _resourceService.GetResources(new ResourceQuery(), _publisherService.GetFeaturedPublisher(),
                    _featuredTitleService.GetFeaturedTitles(), _publisherService.GetPublishers())
                .Where(x => x.NotSaleable == false && (x.StatusId == (int)ResourceStatus.Active ||
                                                       x.StatusId == (int)ResourceStatus.Forthcoming)).ToList();

            var excelExport = new ResourceListExcelExport(resources,
                Url.Action("Title", "Resource", new { Area = "" },
                    HttpContext.Request.IsSecureConnection ? "https" : "http"), true);
            var fileDownloadName = $"R2-TitleList-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        public ActionResult Help()
        {
            return PartialView("_Help");
        }

        public ActionResult Faqs()
        {
            var model = new MarketingHome { HtmlContent = _cmsService.GetRootPageText("Frequent Ask Questions") };
            return View(model);
        }

        public ActionResult About()
        {
            var model = new MarketingHome { HtmlContent = _cmsService.GetRootPageText("About") };
            return View(model);
        }

        public ActionResult CopyrightPolicy()
        {
            var model = new MarketingHome { HtmlContent = _cmsService.GetRootPageText("Copyright Policy") };
            return View(model);
        }

        public ActionResult PrivacyPolicy()
        {
            var model = new MarketingHome { HtmlContent = _cmsService.GetRootPageText("Privacy Policy") };
            return View(model);
        }

        public ActionResult Info()
        {
            var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();

            foreach (var key in Request.ServerVariables.AllKeys)
            {
                _log.Info($"{key} : {Request.ServerVariables[key]}");
            }


            var info = new Info
            {
                ServerName = Environment.MachineName,
                UserAgent = Request.UserAgent,
                IpAddress = Request.GetHostIpAddress(),
                CurrentReferrer = !string.IsNullOrWhiteSpace(Request.HttpReferrer()) ? Request.HttpReferrer() : "n/a",
                IsAuthenticated = _authenticationContext.IsAuthenticated,
                AuthnticationReferrer = _authenticationContext.AuthenticationReferrer,
                RequestId = requestLoggerData.RequestId,
                //SessionId = requestLoggerData.SessionId,
                SessionId = Session.SessionID,
                CountryCode = requestLoggerData.CountryCode
            };

            if (_authenticationContext.IsAuthenticated)
            {
                info.InstitutionName = _authenticationContext.AuthenticatedInstitution.Name;
                info.InstitutionAccountNumber = _authenticationContext.AuthenticatedInstitution.AccountNumber;
                info.InstitutionId = _authenticationContext.AuthenticatedInstitution.Id;

                info.AuthenticationMethod =
                    _authenticationContext.AuthenticatedInstitution.AuthenticationMethod.ToString();

                info.UserRole = _authenticationContext.AuthenticatedInstitution.UserRole.Description;

                if (_authenticationContext.AuthenticatedInstitution.User != null)
                {
                    var user = _authenticationContext.AuthenticatedInstitution.User;
                    info.UserDisplayName = $"{user.LastName}, {user.FirstName}";
                    info.UserEmailAddress = user.Email;
                    info.UserId = user.Id;
                    info.UserName = user.UserName;
                }
            }

            return View(info);
        }
    }
}