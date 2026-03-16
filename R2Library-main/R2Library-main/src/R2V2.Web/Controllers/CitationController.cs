#region

using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Resource;
using R2V2.Web.Controllers.SuperTypes;

#endregion

namespace R2V2.Web.Controllers
{
    public class CitationController : R2BaseController
    {
        private readonly IQueryable<Resource> _resources;
        private readonly ResourceService _resourceService;

        public CitationController(IAuthenticationContext authenticationContext, IQueryable<Resource> resources,
            ResourceService resourceService)
            : base(authenticationContext)
        {
            _resources = resources;
            _resourceService = resourceService;
        }

        public ActionResult Download(string resourceId, string url, string format)
        {
            int resId;
            int.TryParse(resourceId, out resId);
            if (resId > 0)
            {
                var resource = GetResource(resId);

                switch (format)
                {
                    case "apa":
                    {
                        var citation = _resourceService.GetApaFormatCitation(resource, url);
                        return File(Encoding.ASCII.GetBytes(citation), "text/plain",
                            $"R2-APA-Citation-{resource.Isbn10}.txt");
                    }
                    case "endnote":
                    {
                        var citation = _resourceService.GetEndNoteCitation(resource, url);
                        return File(Encoding.ASCII.GetBytes(citation), "text/plain",
                            $"R2-EndNote-Citation-{resource.Isbn10}.txt");
                    }
                    case "refworks":
                    {
                        var citation = _resourceService.GetRefWorksCitation(resource, url);
                        return File(Encoding.ASCII.GetBytes(citation), "text/plain",
                            $"R2-RefWorks-Citation-{resource.Isbn10}.txt");
                    }
                    case "procite":
                    {
                        var citation = _resourceService.GetProciteCitation(resource, url);
                        return File(Encoding.ASCII.GetBytes(citation), "text/plain",
                            $"R2-Procite-Citation-{resource.Isbn10}.txt");
                    }
                }
            }

            return RedirectToAction("Index", "Browse");
        }

        public ActionResult ExportRefWorks(string resourceId, string url)
        {
            int resId;
            int.TryParse(resourceId, out resId);
            if (resId > 0)
            {
                var resource = GetResource(resId);
                var citation = _resourceService.GetRefWorksCitation(resource, url);

                return Content(citation, "text/plain");
            }

            return RedirectToAction("Index", "Browse");
        }

        private Resource GetResource(int resourceId)
        {
            return _resources.FirstOrDefault(x => x.Id == resourceId);
        }
    }
}