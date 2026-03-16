#region

using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Extensions;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public interface IExceptionResultFilter
    {
        void Execute(ExceptionContext context);
    }

    public class ExceptionResultFilter : IExceptionResultFilter
    {
        private readonly IEnumerable<IResultFilter> _resultFilters;

        public ExceptionResultFilter(IEnumerable<IResultFilter> resultFilters)
        {
            _resultFilters = resultFilters;
        }

        public void Execute(ExceptionContext context)
        {
            var resultExecutingContext =
                new ResultExecutingContext(context.Controller.ControllerContext, context.Result);

            _resultFilters.ForEach(f => f.OnResultExecuting(resultExecutingContext));

            var resultExecutedContext =
                new ResultExecutedContext(context.Controller.ControllerContext, context.Result, false, null);
            _resultFilters.ForEach(f => f.OnResultExecuted(resultExecutedContext));
        }
    }
}