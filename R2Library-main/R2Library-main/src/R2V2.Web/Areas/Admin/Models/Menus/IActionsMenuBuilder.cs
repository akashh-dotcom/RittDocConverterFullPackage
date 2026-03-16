#region

using System.Web.Mvc;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Menus
{
    public interface IActionsMenuBuilder
    {
        ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel);
    }
}