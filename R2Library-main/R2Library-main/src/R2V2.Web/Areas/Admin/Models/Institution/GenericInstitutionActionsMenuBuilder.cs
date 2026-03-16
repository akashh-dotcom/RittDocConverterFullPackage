#region

using System.Web.Mvc;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class GenericInstitutionActionsMenuBuilder : IActionsMenuBuilder
    {
        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu();

            if (adminBaseModel.ToolLinks != null)
            {
                actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
            }

            return actionsMenu;
        }
    }
}