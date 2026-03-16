#region

using System;
using System.Web;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Helpers
{
    public static class HttpContextExtensions
    {
        public static Guid RequestId(this HttpContextBase httpContext)
        {
            if (httpContext.Items[RequestStorageKeys.HttpRequestId] == null)
            {
                httpContext.Items.Add(RequestStorageKeys.HttpRequestId, Guid.NewGuid());
            }

            return (Guid)httpContext.Items[RequestStorageKeys.HttpRequestId];
        }

        public static Guid RequestId(this HttpContext httpContext)
        {
            if (httpContext.Items[RequestStorageKeys.HttpRequestId] == null)
            {
                httpContext.Items.Add(RequestStorageKeys.HttpRequestId, Guid.NewGuid());
            }

            return (Guid)httpContext.Items[RequestStorageKeys.HttpRequestId];
        }

        public static IRequestStorageService RequestStorage(this HttpContextBase contextBase)
        {
            var log = ServiceLocator.Current.GetInstance<ILog<RequestStorageService>>();
            return new RequestStorageService(contextBase, log);
        }
    }
}