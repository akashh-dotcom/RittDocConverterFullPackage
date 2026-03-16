#region

using System.Web.Mvc;

#endregion

namespace R2V2.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Admin";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_Resource",
                "Admin/Resource/{action}/{resourceId}",
                new { controller = "Resource", action = "Detail", resourceId = UrlParameter.Optional }
            );

            context.MapRoute(
                "Admin_Configuration",
                "Admin/Configuration/{action}",
                new { controller = "Configuration", action = "Index" }
            );

            context.MapRoute(
                "Admin_AccessAndDiscoverability",
                "Admin/AccessAndDiscoverability/{institutionId}/{codename}",
                new { controller = "AccessAndDiscoverability", action = "Index", codename = "Integration" }
            );

            context.MapRoute(
                "Admin_SystemInformation",
                "Admin/SystemInformation/{codename}",
                new { controller = "SystemInformation", action = "Index", codename = "OutReach" }
            );

            context.MapRoute(
                "Admin",
                "Admin/{controller}/{action}/{institutionId}/{id}",
                new
                {
                    controller = "Institution", action = "List", institutionId = UrlParameter.Optional,
                    id = UrlParameter.Optional
                }
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}