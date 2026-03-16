#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Web.Infrastructure.Contexts;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class FilterProvider : IFilterProvider
    {
        private readonly IEnumerable<IR2V2Filter> _r2V2Filters;

        public FilterProvider(IEnumerable<IR2V2Filter> r2V2Filters)
        {
            _r2V2Filters = r2V2Filters;
        }

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var actionContext = new ActionContext(controllerContext, actionDescriptor);
            var result = _r2V2Filters.Where(f => f.CanProcess(actionContext)).ToList();

            var filters = result.Select(x => new Filter(x, x.FilterScope, x.Order)).ToList();
            return filters;
        }
    }
}