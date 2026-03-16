#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Export.FileTypes;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Infrastructure.Authentication;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using User = R2V2.Core.Authentication.User;
using UserEdit = R2V2.Web.Areas.Admin.Models.User.UserEdit;
using UserService = R2V2.Web.Services.UserService;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class UserController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly AthensAuthenticationService _athensAuthenticationService;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IClientSettings _clientSettings;
        private readonly Core.UserService _coreUserService;
        private readonly EmailSiteService _emailService;
        private readonly ILog<UserController> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserOptionService _userOptionService;
        private readonly UserService _userService;

        public UserController(IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , Core.UserService coreUserService
            , EmailSiteService emailService, UserService userService
            , IUnitOfWorkProvider unitOfWorkProvider
            , ILog<UserController> log
            , IClientSettings clientSettings
            , IAuthenticationService authenticationService
            , UserOptionService userOptionService
            , AthensAuthenticationService athensAuthenticationService
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _adminContext = adminContext;
            _coreUserService = coreUserService;
            _emailService = emailService;
            _userService = userService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _log = log;
            _clientSettings = clientSettings;
            _authenticationService = authenticationService;
            _userOptionService = userOptionService;
            _athensAuthenticationService = athensAuthenticationService;
        }

        public ActionResult List(UserQuery userQuery)
        {
            userQuery.SearchType = string.IsNullOrWhiteSpace(userQuery.Query)
                ? "All"
                : userQuery.Query.Contains("@")
                    ? "Email"
                    : "All";

            var institutionUsersView = GetUsersList(userQuery);

            if (institutionUsersView.Users.Count() == 1 && !string.IsNullOrWhiteSpace(userQuery.Query))
            {
                if (institutionUsersView.Users.First().DisplayLink)
                {
                    return RedirectToAction("Edit",
                        institutionUsersView.UserQuery.LoadAllUsers
                            ? new
                            {
                                institutionId = institutionUsersView.Users.First().InstitutionId,
                                institutionUsersView.Users.First().Id
                            }
                            : new { institutionId = userQuery.InstitutionId, institutionUsersView.Users.First().Id });
                }
            }

            SetUserPaging(institutionUsersView, userQuery, institutionUsersView.TotalCount);

            institutionUsersView.ToolLinks = GetToolLinks(true,
                Url.Action("Export", institutionUsersView.UserQuery.ToRouteValues(true)));

            return View(institutionUsersView);
        }

        [HttpPost]
        public ActionResult List(UserQuery userQuery, EmailPage emailPage)
        {
            if (emailPage.To == null)
            {
                return RedirectToAction("List", userQuery.ToRouteValues()); //return List(userQuery);
            }

            var userList = GetUsersList(userQuery);
            var messageBody = RenderRazorViewToString("User", "_List", userList);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Export(UserQuery userQuery, string export)
        {
            var userCount = 0;

            var users = _coreUserService.GetUsers(userQuery, ref userCount, true);
            users.ForEach(x => SetIsLockedForUser(x, _clientSettings));

            var excelExport = new UserListExcelExport(users, userQuery.InstitutionId == 0,
                CurrentUser.InstitutionId.GetValueOrDefault());
            var fileDownloadName = $"R2-UserList-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        private InstitutionUsers GetUsersList(IUserQuery userQuery)
        {
            var institutionId = userQuery.InstitutionId;
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var userCount = 0;

            var users = _coreUserService.GetUsers(userQuery, ref userCount);

            users.ForEach(x => SetIsLockedForUser(x, _clientSettings));

            var institutionUsers = users.ToInstitutionUsers().ToList();

            userQuery.LoadAllUsers = institutionId == 0;

            var institutionUsersView = institutionId == 0
                ? new InstitutionUsers(userQuery) { Users = institutionUsers }
                : new InstitutionUsers(institution, userQuery) { Users = institutionUsers };

            SetUserPaging(institutionUsersView, userQuery, userCount);

            institutionUsersView.UserQuery = userQuery;

            institutionUsersView.TotalCount = userCount;

            return institutionUsersView;
        }

        private void SetUserPaging(InstitutionUsers institutionUsers, IUserQuery userQuery, int userCount)
        {
            var pageCount = userCount / userQuery.PageSize + (userCount % userQuery.PageSize > 0 ? 1 : 0);

            var lastPage = 0;
            var firstPage = 0;

            SetFirstLastPage(pageCount, userQuery.Page, ref firstPage, ref lastPage);

            institutionUsers.PreviousLink = Url.PreviousPageLink(userQuery, pageCount);
            institutionUsers.NextLink = Url.NextPageLink(userQuery, pageCount);

            institutionUsers.FirstLink = Url.FirstPageLink(userQuery, pageCount);
            institutionUsers.LastLink = Url.LastPageLink(userQuery, pageCount);

            institutionUsers.PageLinks = GetPageLinks(firstPage, lastPage, userQuery.Page);

            var currentCount = (userQuery.Page - 1) * userQuery.PageSize;

            institutionUsers.ResultsFirstItem = currentCount + 1;
            institutionUsers.ResultsLastItem = currentCount + institutionUsers.Users.Count();
        }

        private static void SetFirstLastPage(int pageCount, int currentPage, ref int firstPage, ref int lastPage)
        {
            if (pageCount <= MaxPages || currentPage <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = currentPage - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }
        }

        private static IEnumerable<PageLink> GetPageLinks(int firstPage, int lastPage, int currentPage)
        {
            for (var p = firstPage; p <= lastPage; p++)
            {
                yield return new PageLink
                    { Selected = p == currentPage, Text = p.ToString(CultureInfo.InvariantCulture) };
            }
        }

        [HttpGet]
        public ActionResult Edit(int institutionId, int id)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);

            User dbUser = null;
            UserEdit userEdit;
            if (id != 0)
            {
                dbUser = _coreUserService.GetUser(id);
            }

            if (dbUser == null)
            {
                userEdit = new UserEdit
                {
                    Role = new Role { Code = RoleCode.USERS },
                    InstitutionId = institution.Id,
                    RecordStatus = true
                };
            }
            else
            {
                userEdit = dbUser.ToUserEdit();
            }

            if (userEdit == null)
            {
                return RedirectToAction("List", new UserQuery { InstitutionId = institutionId });
            }

            PopulateUserSelectLists(userEdit, institution);

            userEdit.DisplayExpertReviewerEmailOptions = institution.ExpertReviewerUserEnabled;

            var institutionUserEdit = new InstitutionUserEdit(institution as AdminInstitution)
            {
                User = userEdit,
                UserQuery = new UserQuery { InstitutionId = userEdit.InstitutionId, RoleCode = userEdit.Role.Code },
                IsSelf = CurrentUser.Id == userEdit.Id,
                IsExpertReviewerEnabled = institution.ExpertReviewerUserEnabled
            };

            return View(institutionUserEdit);
        }

        [HttpPost]
        public ActionResult Edit(InstitutionUserEdit institutionUserEdit)
        {
            if (ModelState.IsValid)
            {
                User user;
                var userSaved = false;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        user = _userService.ConvertToCoreUser(institutionUserEdit,
                            CurrentUser.Id == institutionUserEdit.User.Id);

                        if (!UserHasError(user, institutionUserEdit))
                        {
                            uow.SaveOrUpdate(user);
                            uow.Commit();
                            transaction.Commit();
                            userSaved = true;
                        }

                        if (!userSaved)
                        {
                            transaction.Rollback();
                        }
                    }
                }

                if (userSaved)
                {
                    _userOptionService.SaveUserOptionValues(user);
                    return RedirectToAction("List",
                        new UserQuery { InstitutionId = institutionUserEdit.User.InstitutionId }.ToRouteValues());
                }
            }

            var institution = _adminContext.GetAdminInstitution(institutionUserEdit.InstitutionId);

            var model = new InstitutionUserEdit(institution as AdminInstitution)
            {
                User = institutionUserEdit.User,
                UserQuery = institutionUserEdit.UserQuery,
                UrlReferrer = institutionUserEdit.UrlReferrer,
                IsSelf = institutionUserEdit.IsSelf,
                IsExpertReviewerEnabled = institution.ExpertReviewerUserEnabled
            };
            PopulateUserSelectLists(model.User, institution);
            return View(model);
        }

        public ActionResult RemoveAthensLink(int institutionId, int id)
        {
            var dbUser = _coreUserService.GetUser(id);
            _athensAuthenticationService.RemoveAthensUserTargetedId(dbUser);

            if (CurrentUser.Id == dbUser.Id)
            {
                var authResult = _authenticationService.ReloadUser(dbUser.Id);
                _authenticationContext.Set(authResult.AuthenticatedInstitution);
                AthensAuthenticationCookie.ClearAthensCookie(Response);
            }

            return RedirectToAction("Edit", new { institutionId = dbUser.InstitutionId, id = dbUser.Id });
        }


        public ActionResult Email(int institutionId, int id)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);

            var user = _coreUserService.GetUser(id);
            if (user != null)
            {
                var model = new AdminUserEmailOptions(user.OptionValues, institution, user, CurrentUser.Id == user.Id);
                return View(model);
            }

            return RedirectToAction("Edit", new { institutionId, id });
        }

        [HttpPost]
        public ActionResult Email(AdminUserEmailOptions emailOptions)
        {
            var user = _coreUserService.GetUser(emailOptions.UserId);
            _userService.SaveUserEmailOptions(user, emailOptions);
            if (emailOptions.IsSelf)
            {
                var authResult = _authenticationService.ReloadUser(user.Id);
                _authenticationContext.Set(authResult.AuthenticatedInstitution);
            }


            return RedirectToAction("Edit", new { user.InstitutionId, user.Id });
        }

        private bool UserHasError(User user, InstitutionUserEdit institutionUserEdit)
        {
            if (user == null)
            {
                ModelState.AddModelError("User.NewPassword",
                    institutionUserEdit.User.NewPassword != institutionUserEdit.User.ConfirmPassword
                        ? Resources.PasswordsDoNotMatch
                        : Resources.ReactivatePleaseFillInPassword);
                return true;
            }

            var userNameRegex = new Regex(@"[,]");

            if (userNameRegex.IsMatch(user.UserName))
            {
                ModelState.AddModelError("User.UserName", Resources.UserNameCommas);
                return true;
            }

            if (_coreUserService.DoesUserNameAlreadyExist(user))
            {
                ModelState.AddModelError("User.UserName", Resources.UserNameAlreadyExists);
                return true;
            }

            if (string.IsNullOrWhiteSpace(user.Password) && user.Id == 0)
            {
                if (institutionUserEdit.User.NewPassword != institutionUserEdit.User.ConfirmPassword)
                {
                    ModelState.AddModelError("User.NewPassword", Resources.PleaseFillInPassword);
                    return true;
                }

                user.Password = institutionUserEdit.User.NewPassword;
            }

            return false;
        }

        private void PopulateUserSelectLists(UserEdit userEdit, IAdminInstitution institution)
        {
            _log.Debug("PopulateUserSelectLists() >>>");
            if (userEdit.Role != null &&
                (userEdit.Role.Code == RoleCode.RITADMIN || userEdit.Role.Code == RoleCode.SALESASSOC))
            {
                var userTerritories = _coreUserService.GetUserTerritories();
                var territoriesUserExcluded = _coreUserService.GetTerritories();
                userEdit.PopulateSelectLists(_coreUserService.GetListDepartments(), CurrentUser, userTerritories,
                    territoriesUserExcluded, institution);
            }
            else
            {
                userEdit.PopulateSelectLists(_coreUserService.GetListDepartments(), CurrentUser, institution);
            }

            _log.Debug("PopulateUserSelectLists() <<<");
        }
    }
}