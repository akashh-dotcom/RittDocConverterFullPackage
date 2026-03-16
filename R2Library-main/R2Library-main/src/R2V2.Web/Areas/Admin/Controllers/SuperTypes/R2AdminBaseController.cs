#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Optimization;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource.Collection;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.SuperTypes
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC })]
    [RequestLoggerFilter]
    public abstract class R2AdminBaseController : Controller, IR2AdminBaseController
    {
        protected const int MaxPages = 9;

        private readonly Random _random = new Random(Environment.TickCount);
        protected readonly AuthenticatedInstitution AuthenticatedInstitution;

        public readonly IAuthenticationContext AuthenticationContext;

        protected R2AdminBaseController(IAuthenticationContext authenticationContext)
        {
            AuthenticationContext = authenticationContext;
            AuthenticatedInstitution = authenticationContext.AuthenticatedInstitution;
        }

        protected IUser CurrentUser
        {
            get
            {
                if (AuthenticationContext.IsAuthenticated && AuthenticatedInstitution != null &&
                    AuthenticatedInstitution.User != null)
                {
                    return AuthenticatedInstitution.User;
                }

                return null;
            }
        }

        protected int UserId
        {
            get
            {
                var currentUser = CurrentUser;
                return currentUser != null ? currentUser.Id : 0;
            }
        }

        protected bool IsRittenhouseAdmin()
        {
            return AuthenticationContext.IsRittenhouseAdmin();
        }

        protected bool IsInstitutionAdmin()
        {
            return AuthenticationContext.IsInstitutionAdmin();
        }

        protected bool IsPublisherUser()
        {
            return AuthenticationContext.IsPublisherUser();
        }

        protected bool IsSalesAssociate()
        {
            return AuthenticationContext.IsSalesAssociate();
        }

        public string RenderRazorViewToString(string controllerName, string viewName, object model)
        {
            var view = $"~/Areas/Admin/Views/{controllerName}/{viewName}.cshtml";

            return RenderViewToString(view, model);
        }

        /// <summary>
        ///     Only Used for Views Not in the ADMIN area
        /// </summary>
        public string RenderWebRazorViewToString(string controllerName, string viewName, object model)
        {
            var view = $"~/Views/{controllerName}/{viewName}.cshtml";

            return RenderViewToString(view, model);
        }


        /// <summary>
        ///     Utilizes the BundleResolver to get all the CSS files specfied in the BundleInitializer to apply them to the email.
        /// </summary>
        private string RenderViewToString(string view, object model)
        {
            var baseModel = model as IR2V2Model;
            if (baseModel != null)
            {
                baseModel.Footer = new Footer();
            }

            var cssFiles = @BundleResolver.Current.GetBundleContents("~/_Static/Css/r2.email.css");

            var sb = new StringBuilder();
            foreach (var cssFile in cssFiles)
            {
                sb.Append(System.IO.File.ReadAllText(Server.MapPath(cssFile)));
            }

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult =
                    ViewEngines.Engines.FindView(ControllerContext, view, "~/Views/Shared/_Layout.Email.cshtml");
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);

                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                var messageBody = sw.GetStringBuilder().ToString().Replace("[[CSS]]", sb.ToString());
                return messageBody;
            }
        }

        public ToolLinks GetToolLinks(bool isEmailable, string excelUrl = null, string marcUrl = null,
            bool hidePrint = false)
        {
            var toolLinks = new ToolLinks();

            if (isEmailable)
            {
                //toolLinks.EmailPage = new EmailPage(R2User.Email ?? "");

                var emailAddress = AuthenticatedInstitution != null && AuthenticatedInstitution.User != null &&
                                   string.IsNullOrWhiteSpace(AuthenticatedInstitution.User.Email)
                    ? AuthenticatedInstitution.User.Email
                    : "";

                toolLinks.EmailPage = new EmailPage(emailAddress);
            }

            if (excelUrl != null)
            {
                toolLinks.ExcelLink = new PageLink
                {
                    Active = true,
                    Href = excelUrl,
                    Text = "Export to Excel"
                };
            }

            if (marcUrl != null)
            {
                toolLinks.MarcLink = new PageLink
                {
                    Active = true,
                    Href = marcUrl,
                    Text = "Export to Marc"
                };
            }

            if (hidePrint)
            {
                toolLinks.HidePrint = true;
            }

            return toolLinks;
        }

        public ToolLinks GetToolLinks(string excelUrl, CollectionManagementQuery collectionManagementQuery,
            ICollectionService collectionService)
        {
            var toolLinks = new ToolLinks();

            var emailAddress = AuthenticatedInstitution != null && AuthenticatedInstitution.User != null &&
                               string.IsNullOrWhiteSpace(AuthenticatedInstitution.User.Email)
                ? AuthenticatedInstitution.User.Email
                : "";

            toolLinks.EmailPage = new EmailPage(emailAddress);


            toolLinks.ExcelLink = new PageLink
            {
                Active = true,
                Href = excelUrl,
                Text = "Export to Excel"
            };

            var marcExportAllName = GetMarcExportName(collectionManagementQuery, collectionService);

            toolLinks.MarcLinks = new MarcLinks
            {
                Links = new List<MarcLink>
                {
                    new MarcLink
                    {
                        Active = true,
                        Href = @Url.Action("CollectionManagementList", "MarcExport",
                            collectionManagementQuery.ToMarcRouteValues(false, false)),
                        Text = "Export Page to Marc",
                        IsDeleteLink = false
                    },
                    new MarcLink
                    {
                        Active = true,
                        Href = @Url.Action("CollectionManagementList", "MarcExport",
                            collectionManagementQuery.ToMarcRouteValues(true, false)),
                        Text = $"Export {marcExportAllName} to Marc",
                        IsDeleteLink = false
                    },
                    new MarcLink
                    {
                        Active = true,
                        Href = @Url.Action("CollectionManagementList", "MarcExport",
                            collectionManagementQuery.ToMarcRouteValues(false, true)),
                        Text = "Export Page to Marc Delete",
                        IsDeleteLink = true
                    },
                    new MarcLink
                    {
                        Active = true,
                        Href = @Url.Action("CollectionManagementList", "MarcExport",
                            collectionManagementQuery.ToMarcRouteValues(true, true)),
                        Text = $"Export {marcExportAllName} to Marc Delete",
                        IsDeleteLink = true
                    }
                }
            };

            return toolLinks;
        }

        private string GetMarcExportName(CollectionManagementQuery collectionManagementQuery,
            ICollectionService collectionService)
        {
            if (collectionManagementQuery.PurchasedOnly)
            {
                return "my Collection";
            }

            if (collectionManagementQuery.IncludePdaResources && !collectionManagementQuery.IncludePdaHistory)
            {
                return "my PDA Collection";
            }

            if (collectionManagementQuery.IncludePdaResources && collectionManagementQuery.IncludePdaHistory)
            {
                return "my PDA History";
            }

            if (collectionManagementQuery.ResourceListType == ResourceListType.FeaturedTitles)
            {
                return "all Featured titles";
            }

            if (collectionManagementQuery.ResourceListType == ResourceListType.FeaturedPublisher)
            {
                return "all Featured Publisher titles";
            }

            if (collectionManagementQuery.IncludeFreeResources)
            {
                return "all Open Access Resources";
            }

            if (collectionManagementQuery.CollectionFilter > 0)
            {
                var collection = collectionService.GetCollectionById(collectionManagementQuery.CollectionFilter);
                return $"all {collection.Name} titles";
            }

            if (collectionManagementQuery.CollectionListFilter > 0)
            {
                var collection = collectionService.GetCollectionById(collectionManagementQuery.CollectionListFilter);
                return $"all {collection.Name} titles";
            }

            return "all eBooks";
        }

        /// <summary>
        ///     Do NOT include slashes with fileName. This function will check and add it if needed.
        /// </summary>
        public string GetTemplateFromFile(string directory, string fileName)
        {
            string fullPathFile;
            if (directory.Contains("\\"))
            {
                fullPathFile = directory.Substring(directory.Length - 1) == "\\" ? directory : $"{directory}\\";
            }
            else
            {
                fullPathFile = directory.Substring(directory.Length - 1) == "/" ? directory : $"{directory}/";
            }

            return System.IO.File.ReadAllText($"{fullPathFile}{fileName}");
        }

        public string GetRandomKey(int length)
        {
            //Removed O. When it renders it looks too similar to 0
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNPQRSTUVWYXZ";
            var builder = new StringBuilder(length);

            for (var i = 0; i < length; ++i)
                builder.Append(chars[_random.Next(chars.Length)]);

            return builder.ToString();
        }

        public void SetIsLockedForUser(User user, IClientSettings clientSettings)
        {
            var lockOutDays = 0;
            switch (user.Role.Code)
            {
                case RoleCode.ExpertReviewer:
                    lockOutDays = clientSettings.AutoLockExpertReviewer;
                    break;
                case RoleCode.INSTADMIN:
                    lockOutDays = clientSettings.AutoLockInstitutionAdmin;
                    break;
                case RoleCode.RITADMIN:
                    lockOutDays = clientSettings.AutoLockRittenhouseAdmin;
                    break;
                case RoleCode.SALESASSOC:
                    lockOutDays = clientSettings.AutoLockSalesAdmin;
                    break;
                case RoleCode.USERS:
                    lockOutDays = clientSettings.AutoLockUser;
                    break;
            }

            var isLocked = false;
            if (user.LastSession != null && user.LastSession < DateTime.Now.AddDays(-lockOutDays))
            {
                isLocked = true;
            }
            else if (user.LastSession == null && user.CreationDate < DateTime.Now.AddDays(-lockOutDays))
            {
                isLocked = true;
            }

            if (isLocked && user.LastPasswordChange != null &&
                user.LastPasswordChange > DateTime.Now.AddDays(-lockOutDays))
            {
                isLocked = false;
            }

            user.IsLocked = isLocked;
        }
    }
}