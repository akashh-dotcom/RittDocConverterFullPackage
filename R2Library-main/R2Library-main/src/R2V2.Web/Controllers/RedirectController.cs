#region

using System;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;

#endregion

namespace R2V2.Web.Controllers
{
    public class RedirectController : R2BaseController
    {
        private readonly ILog<RedirectController> _log;
        private readonly IResourceService _resourceService;

        public RedirectController(ILog<RedirectController> log, IResourceService resourceService)
        {
            _log = log;
            _resourceService = resourceService;
        }

        public ActionResult Index(string aspxFilename, string directory = "")
        {
            try
            {
                int resourceId;
                IResource resource;

                if (string.IsNullOrEmpty(aspxFilename))
                {
                    _log.WarnFormat("aspxFilename is null or empty, URL: {0}, IP: {1}, UserAgent: {2}", Request.RawUrl,
                        Request.UserHostName, Request.UserAgent);
                    return RedirectToAction("NotFound", "Error");
                }

                switch (aspxFilename.ToLower())
                {
                    case "content_courselinks":
                    case "content_res_body":
                        int.TryParse(Request.QueryString["ResourceID"], out resourceId);

                        resource = _resourceService.GetResource(resourceId);
                        if (resource == null)
                        {
                            _log.InfoFormat("Resource Id '{0}' not found, redirecting to 'Error/NotFound'", resourceId);
                            return RedirectToAction("NotFound", "Error");
                        }

                        var section = Request.QueryString["SectionID"];

                        return string.IsNullOrWhiteSpace(section)
                            ? RedirectPermanent(Url.RouteUrl(new
                                { controller = "Resource", action = "Title", resource.Isbn }))
                            : RedirectPermanent(Url.RouteUrl(new
                                { controller = "Resource", action = "Detail", resource.Isbn, section }));

                    case "marc_frame":
                        int.TryParse(Request.QueryString["ResourceID"], out resourceId);

                        resource = _resourceService.GetResource(resourceId);
                        if (resource == null)
                        {
                            _log.InfoFormat("Resource Id '{0}' not found, redirecting to 'Error/NotFound'", resourceId);
                            return RedirectToAction("NotFound", "Error");
                        }

                        return RedirectToActionPermanent("Title", "Resource", new { resource.Isbn });

                    case "resourcedetail":
                        int.TryParse(Request.QueryString["resId"], out resourceId);

                        resource = _resourceService.GetResource(resourceId);
                        if (resource == null)
                        {
                            _log.InfoFormat("Resource Id '{0}' not found, redirecting to 'Error/NotFound'", resourceId);
                            return RedirectToAction("NotFound", "Error");
                        }

                        return RedirectToActionPermanent("Title", "Resource", new { resource.Isbn });

                    case "searchindex":
                        try
                        {
                            var q = Request.Form["SRCHFLTEXT"];
                            var title = Request.Form["SRCHTITLE"];
                            var author = Request.Form["SRCHAUTHOR"];
                            var isbn = Request.Form["SRCHISBN"];
                            _log.DebugFormat("q: '{0}', title = '{1}', author = '{2}', isbn = '{3}'", q, title, author,
                                isbn);
                            return RedirectToActionPermanent("Federated", "Search",
                                new { q, json = false, html = true, pageSize = 50, title, author, isbn });
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message, ex);
                            return RedirectToActionPermanent("Federated", "Search",
                                new { json = false, html = true, pageSize = 50 });
                        }

                    case "search_results_index":
                        return RedirectToActionPermanent("Index", "Search",
                            new { q = Request.QueryString["searchterm"] });

                    case "default":
                        _log.DebugFormat("redirect 'default.asp' to home page", aspxFilename);
                        return RedirectToActionPermanent("Index", "Home");

                    default:
                        _log.WarnFormat("R2v2 is not configured to handle this aspx page: {0}, redirect to home page",
                            aspxFilename);
                        return RedirectToActionPermanent("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder()
                    .AppendFormat("Exception handling ASPX request, '{0}'", aspxFilename)
                    .AppendLine().AppendLine("Redirecting the user to the home page").Append(ex.Message);
                _log.Error(msg.ToString(), ex);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}