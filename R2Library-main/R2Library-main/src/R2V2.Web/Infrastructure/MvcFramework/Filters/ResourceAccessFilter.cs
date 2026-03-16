#region

using System;
using System.Web.Mvc;
using R2V2.Core.Resource;
using R2V2.Web.Controllers;
using R2V2.Web.Controllers.MyR2;
using R2V2.Web.Infrastructure.Contexts;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class ResourceAccessFilter : IR2V2Filter, IActionFilter
    {
        private readonly Func<IResourceAccessService> _resourceAccessServiceFactory;

        public ResourceAccessFilter(Func<IResourceAccessService> resourceAccessServiceFactory)
        {
            _resourceAccessServiceFactory = resourceAccessServiceFactory;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //_log.Debug("OnActionExecuting() >>>");
            var controller = filterContext.Controller;
            if (controller is ResourceController || controller is UserContentController)
            {
                //_log.Debug("OnActionExecuting() <<< - ResourceController || UserContentController");
                return;
            }

            var resourceAccessService = _resourceAccessServiceFactory();
            resourceAccessService.ClearSessionResourceLocks();
            //_log.Debug("OnActionExecuting() <<<");
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // do nothing
        }

        public bool AllowMultiple => true;

        public int Order => 100;

        public FilterScope FilterScope => FilterScope.Global;

        public bool CanProcess(ActionContext actionContext)
        {
            return true;
        }
    }
}