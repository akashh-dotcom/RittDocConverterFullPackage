#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.MyR2;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Models;
using R2V2.Web.Services;

#endregion

namespace R2V2.Web.Controllers.MyR2
{
    public class UserContentFolderController : R2BaseController
    {
        private readonly ILog<UserContentFolderController> _log;
        private readonly MyR2Service _myR2Service;

        public UserContentFolderController(
            IAuthenticationContext authenticationContext
            , ILog<UserContentFolderController> log
            , MyR2Service myR2Service
        ) : base(authenticationContext)
        {
            _log = log;
            _myR2Service = myR2Service;
        }

        public ActionResult List(UserContentType type)
        {
            object json;
            try
            {
                var userContentFolders =
                    _myR2Service.GetUserContentFolders(type, UserId, AuthenticatedInstitution?.Id ?? 0);
                var folders = from userContentFolder in userContentFolders
                    select new
                    {
                        userContentFolder.Id,
                        userContentFolder.FolderName
                    };
                json = new { Folders = folders, Successful = true };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new { Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(string name, string type)
        {
            var userContentType = type.ToUserContentType();
            var userBookmarkFolderId =
                _myR2Service.CreateUserContentFolder(name, userContentType, UserId, AuthenticatedInstitution?.Id ?? 0);

            var json = userBookmarkFolderId > 0
                ? new JsonResponse { Id = userBookmarkFolderId, Status = "success", Successful = true }
                : new JsonResponse { Id = userBookmarkFolderId, Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Rename(int? id, string name, UserContentType type)
        {
            var status = false;

            if (id.HasValue && !string.IsNullOrWhiteSpace(name))
            {
                status = _myR2Service.RenameUserContentFolder(id.Value, name, type, UserId,
                    AuthenticatedInstitution?.Id ?? 0);
            }

            var json = status
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int? id, UserContentType type)
        {
            var status = false;

            if (id.HasValue)
            {
                status = _myR2Service.DeleteUserContentFolder(id.Value, type, UserId,
                    AuthenticatedInstitution?.Id ?? 0);
            }

            var json = status
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}