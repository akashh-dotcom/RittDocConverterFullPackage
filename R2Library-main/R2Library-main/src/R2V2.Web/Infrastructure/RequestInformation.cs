#region

using System.Net;
using System.Text;
using System.Web;
using R2V2.Extensions;
using R2V2.Infrastructure;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Infrastructure
{
    public class RequestInformation : IRequestInformation
    {
        public HttpRequestBase Request => new HttpRequestWrapper(HttpContext.Current.Request);

        public HttpContextBase Context => new HttpContextWrapper(HttpContext.Current);

        public override string ToString()
        {
            return "Request Id: {0}".Args(Id);
        }

        private static string GetIpAddress(HttpRequestBase request)
        {
            var hostAddress = request.GetHostIpAddress();
            return string.IsNullOrEmpty(hostAddress)
                ? IPAddress.Loopback.GetIPv4Address()
                : IPAddress.Parse(hostAddress).GetIPv4Address();
        }

        #region IRequestInformation Members

        public string Summary()
        {
            return "Request Details >> Id: {0}, Client Address: {1}, Referring Url: {2}, Session Id: {3}; User Id: {4}"
                .Args(
                    Id,
                    ClientAddress,
                    ReferringUrl,
                    SessionId,
                    Context.User != null && Context.User.Identity != null
                        ? Context.User.Identity.ToString()
                        : "Not Authenticated"
                );
        }

        public string Details()
        {
            var builder = new StringBuilder(Summary());
            builder.AppendLine();
            builder.AppendFormatedLine("\tUrl: {0}".Args(Request.Url));
            builder.AppendFormatedLine("\tRawUrl: {0}".Args(Request.RawUrl));
            builder.AppendFormatedLine("\tUserAgent: {0}".Args(Request.UserAgent));
            builder.AppendFormatedLine("\tContentType: {0}".Args(Request.ContentType));
            builder.AppendFormatedLine("\tContentEncoding: {0}".Args(Request.ContentEncoding));
            builder.AppendFormatedLine("\tParmas: ");
            foreach (var key in Request.Params.Keys)
            {
                builder.AppendFormatedLine("\t\t{0}:{1}", key, Request.Params[key.ToString()]);
            }

            return builder.ToString();
        }

        public string Id => Context.RequestId().ToString();

        public string ClientAddress => GetIpAddress(Request);

        public string ReferringUrl => Request.HttpReferrer();

        public string SessionId => Context.Session == null ? string.Empty : Context.Session.SessionID;

        public string Host => Request.Url != null ? Request.Url.Host : "";

        public string Url => $"{Request.RequestType} {Request.RawUrl} ({Request.ContentType})";

        #endregion
    }
}