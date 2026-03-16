#region

using System.Web.Mvc;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Menus
{
    public abstract class ActionMenuBuilderBase : IActionsMenuBuilder
    {
        public abstract ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel);
    }
}