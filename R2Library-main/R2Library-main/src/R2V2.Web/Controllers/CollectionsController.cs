#region

using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Models.Collections;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Controllers
{
    public class CollectionsController : R2BaseController
    {
        private readonly ICollectionService _collectionService;
        private readonly ILog<CollectionsController> _log;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourceService _resourceService;

        public CollectionsController(
            ILog<CollectionsController> log
            , IAuthenticationContext authenticationContext
            , ICollectionService collectionService
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
        ) : base(authenticationContext)
        {
            _log = log;
            _collectionService = collectionService;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
        }

        // GET: Collections
        public ActionResult Index(int? collectionId, int? publisherId)
        {
            ICollection selectedCollection = null;
            var publicCollections = _collectionService.GetAllPublicCollections();
            selectedCollection = collectionId == null
                ? publicCollections.FirstOrDefault()
                : publicCollections.Find(x => x.Id == collectionId);

            if (selectedCollection == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var resources = _resourceService.GetAllResources()
                .Where(x => x.CollectionIdsToArray().Contains(selectedCollection.Id))
                .OrderBy(x => x.SortTitle)
                .ToList();

            var totalResourceCount = resources.Count;

            var publisherDictionary = resources
                .Select(x => x.Publisher)
                .GroupBy(publisher => publisher.Id)
                .ToDictionary(x => x.Key, x => x.First());

            var countDictionary = resources.GroupBy(x => x.Publisher.Id).ToDictionary(x => x.Key, x => x.Count());

            if (publisherId != null)
            {
                resources = resources.Where(x => x.Publisher.Id == publisherId).ToList();
            }

            var resourceSummaries = resources.Select(x => x.ToResourceSummary()).ToList();
            if (AuthenticatedInstitution != null)
            {
                foreach (var resourceSummary in resourceSummaries)
                {
                    resourceSummary.IsFullTextAvailable =
                        _resourceAccessService.IsFullTextAvailable(resourceSummary.Id);
                }
            }

            var model = new CollectionListModel(
                publicCollections
                , selectedCollection
                , resourceSummaries
                , publisherDictionary
                , countDictionary
                , publisherId
                , totalResourceCount
            );

            if (CurrentUser != null)
            {
                if (CurrentUser.IsInstitutionAdmin() || CurrentUser.IsRittenhouseAdmin() ||
                    CurrentUser.IsExpertReviewer() || CurrentUser.IsSalesAssociate())
                {
                    model.InstitutionId = CurrentUser.InstitutionId;
                }
            }

            return View(model);
        }
    }
}