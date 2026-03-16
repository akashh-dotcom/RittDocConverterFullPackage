#region

using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.MyR2;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Models;
using R2V2.Web.Models.MyR2;
using R2V2.Web.Models.Search;
using R2V2.Web.Services;
using UserContentItem = R2V2.Web.Models.MyR2.UserContentItem;

#endregion

namespace R2V2.Web.Controllers.MyR2
{
    public class UserContentController : R2BaseController
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<UserContentController> _log;
        private readonly MyR2Service _myR2Service;
        private readonly IResourceService _resourceService;
        private readonly SearchService _searchService;
        private readonly IUserContentService _userContentService;

        public UserContentController(ILog<UserContentController> log
            , IAuthenticationContext authenticationContext
            , IUserContentService userContentService
            , IResourceService resourceService
            , SearchService searchService
            , MyR2Service myR2Service
        )
            : base(authenticationContext)
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _userContentService = userContentService;
            _resourceService = resourceService;
            _searchService = searchService;
            _myR2Service = myR2Service;
        }

        //
        // GET: /UserContent/Create
        public ActionResult Create(UserContentItem userContentItem)
        {
            var id = _myR2Service.CreateUserContentItem(userContentItem, UserId, AuthenticatedInstitution?.Id ?? 0);

            var json = id > 0
                ? new JsonResponse { Id = id, Status = "success", Successful = true }
                : new JsonResponse { Id = id, Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /UserContent/Move/5
        public ActionResult Move(UserContentItem userContentItem, int? newFolderId)
        {
            var status = false;

            if (userContentItem.Id > 0 && userContentItem.FolderId > 0 &&
                userContentItem.Type != UserContentType.None && newFolderId.HasValue)
            {
                status = _myR2Service.MoveUserContentItem(userContentItem, newFolderId.Value, UserId,
                    AuthenticatedInstitution?.Id ?? 0);
            }

            var json = status
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /UserContent/Delete/5
        public ActionResult Delete(UserContentItem userContentItem)
        {
            var status = false;

            if (userContentItem.Id > 0 && userContentItem.FolderId > 0 && userContentItem.Type != UserContentType.None)
            {
                status = _myR2Service.DeleteUserContentItem(userContentItem, UserId, AuthenticatedInstitution?.Id ?? 0);
            }

            var json = status
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Export(int id, UserContentType type)
        {
            switch (type)
            {
                case UserContentType.Image:
                    var imageResult = GetImage(id, "application/octet-stream");
                    if (imageResult != null)
                    {
                        return imageResult;
                    }

                    break;

                case UserContentType.Reference:
                    var referenceResult = GetReference(id);
                    if (referenceResult != null)
                    {
                        return referenceResult;
                    }

                    break;
                default:
                    var linkResult = GetCourseLink(id);
                    if (linkResult != null)
                    {
                        return linkResult;
                    }

                    break;
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Image(int id)
        {
            return GetImage(id, "image/jpeg");
        }

        private ActionResult GetImage(int id, string mimeType)
        {
            var userContentItem =
                _myR2Service.GetUserContentItem(UserContentType.Image, id, UserId, AuthenticatedInstitution?.Id ?? 0);

            if (userContentItem == null)
            {
                return new EmptyResult();
            }

            var resource = _resourceService.GetResource(userContentItem.ResourceId);

            userContentItem.ImageUrl = null; //Needed to reset the image
            var resourceUrl = Url.Action("Title", "Resource", new { resource.Isbn },
                HttpContext.Request.IsSecureConnection ? "https" : "http");

            var image = _userContentService.GetImageForExportWithCitation(userContentItem.ImageUrl, resource,
                resourceUrl);

            if (image == null)
            {
                return new EmptyResult();
            }

            return File(image, mimeType, $"r2-library-{id}.jpg");
        }

        private FileResult GetCourseLink(int id)
        {
            var userContentItem = _myR2Service.GetUserContentItem(UserContentType.CourseLink, id, UserId,
                AuthenticatedInstitution?.Id ?? 0);

            if (userContentItem == null || userContentItem.ResourceId == 0)
            {
                return null;
            }

            var resourceId = userContentItem.ResourceId;
            var sectionId = userContentItem.SectionId;

            var url = Url.Action("Link", "Resource", new { id = resourceId, sectionId },
                HttpContext.Request.IsSecureConnection ? "https" : "http");

            return File(new UTF8Encoding().GetBytes(url), "text/plain", $"r2-library-course-link-{id}.txt");
        }

        private FileResult GetReference(int id)
        {
            var userContentItem = _myR2Service.GetUserContentItem(UserContentType.Reference, id, UserId,
                AuthenticatedInstitution?.Id ?? 0);

            if (userContentItem == null || userContentItem.ResourceId == 0)
            {
                return null;
            }

            var resource = _resourceService.GetResource(userContentItem.ResourceId);

            var resourceUrl = Url.Action("Title", "Resource", new { resource.Isbn },
                HttpContext.Request.IsSecureConnection ? "https" : "http");

            var citation = _resourceService.GetCitation(resource, resourceUrl);

            return File(new UTF8Encoding().GetBytes(citation), "text/plain", $"r2-library-reference-{id}.txt");
        }

        public ActionResult SaveSearch(SavedSearch savedSearch)
        {
            var userId = GetUserId();
            _log.DebugFormat("userId: {0}", userId);
            var id = 0;
            if (userId > 0)
            {
                id = _searchService.SaveUserSearch(savedSearch, userId);
            }
            else if (AuthenticatedInstitution != null)
            {
                id = _myR2Service.SaveSavedSearch(savedSearch, AuthenticatedInstitution.Id);
            }

            var json = id > 0
                ? new JsonResponse { Id = id, Status = "success", Successful = true }
                : new JsonResponse { Id = id, Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSavedSearch(int savedSearchId)
        {
            var status = UserId > 0
                ? _userContentService.DeleteSavedSearch(UserId, savedSearchId)
                : _myR2Service.DeleteSavedSearch(savedSearchId, AuthenticatedInstitution.Id);

            var json = status
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSavedSearchResult(int savedSearchResultId)
        {
            var status = _userContentService.DeleteSavedSearchResult(UserId, savedSearchResultId);

            var json = status
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveSearchResult(SearchQuery searchQuery, string name)
        {
            var searchResultSet = _searchService.Search(searchQuery);

            var test = new SavedSearchResult
            {
                Title = name,
                ResultsCount = searchResultSet.TotalResultsCount,
                SavedSearchResultSet = new SavedSearchResultSet
                {
                    SearchQuery = searchResultSet.Query,
                    SearchResultList = searchResultSet.SearchResults.ToList()
                }
            };

            //searchResultSet.ResultsAsJson()
            var id = _searchService.SaveUserSearchResult(test, UserId);

            var json = id > 0
                ? new JsonResponse { Id = id, Status = "success", Successful = true }
                : new JsonResponse { Id = id, Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private int GetUserId()
        {
            if (!_authenticationContext.IsAuthenticated || AuthenticatedInstitution == null)
            {
                _log.Debug("unauthenticated");
                return -2;
            }

            if (AuthenticatedInstitution.User == null)
            {
                _log.Debug("User is null");
                return -4;
            }

            return AuthenticatedInstitution.User.Id;
        }
    }
}