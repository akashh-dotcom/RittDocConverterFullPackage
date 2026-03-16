#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using R2V2.Infrastructure.DependencyInjection;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Routing
{
    public class R2V2RouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var processRequestHandlers = ServiceLocator.Current.GetAllInstances<IOnExecuteOnProcessRequest>().ToList();

            return new R2V2MvcHandler(requestContext, processRequestHandlers);
        }
    }

    public class R2V2MvcHandler : MvcHandler
    {
        private readonly IEnumerable<IOnExecuteOnProcessRequest> _processRequestHandlers;

        public R2V2MvcHandler(RequestContext requestContext,
            IEnumerable<IOnExecuteOnProcessRequest> processRequestHandlers) : base(requestContext)
        {
            _processRequestHandlers = processRequestHandlers;
        }

        protected override IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback,
            object state)
        {
            foreach (var onExecuteOnProcessRequest in _processRequestHandlers)
            {
                onExecuteOnProcessRequest.Before(RequestContext);
            }

            return base.BeginProcessRequest(httpContext, callback, state);
        }

        protected override void EndProcessRequest(IAsyncResult asyncResult)
        {
            base.EndProcessRequest(asyncResult);

            foreach (var onExecuteOnProcessRequest in _processRequestHandlers)
            {
                onExecuteOnProcessRequest.After(RequestContext);
            }
        }

        protected override void ProcessRequest(HttpContextBase httpContext)
        {
            foreach (var onExecuteOnProcessRequest in _processRequestHandlers)
            {
                onExecuteOnProcessRequest.Before(RequestContext);
            }

            base.ProcessRequest(httpContext);

            foreach (var onExecuteOnProcessRequest in _processRequestHandlers)
            {
                onExecuteOnProcessRequest.After(RequestContext);
            }
        }
    }
}