#region

using System.Linq;
using System.Text;
using System.Web;
using R2V2.Extensions;

#endregion

namespace R2V2.Web.Helpers
{
    public static class HttpRequestExtensions
    {
        public static string Details(this HttpRequestBase request)
        {
            return new StringBuilder()
                //.AppendLine("Id: {0}".Args(HttpContext.Current.Id()))
                .AppendLine("RequestId: {0}".Args(HttpContext.Current.RequestId()))
                .AppendLine("Url: {0} {1} {2}".Args(request.RequestType, request.RawUrl, request.ContentType))
                .AppendLine("Params: {0}".Args(request.Params.AllKeys.Aggregate(new StringBuilder(),
                    (s, k) => s.AppendFormat("'{0}'=>'{1}';", k, request.Params[k]))))
                .AppendLine(
                    "User Agent: {0} IP: {1} Referer: {2}".Args(
                        request.UserAgent,
                        request.GetHostIpAddress(),
                        request.HttpReferrer()))
                .AppendLine(
                    "Current User: {0}".Args(
                        HttpContext.Current.User != null
                            ? "{0} (Auth Type: {1}, Auth'ed: {2})".Args(
                                    HttpContext.Current.User.Identity.Name,
                                    HttpContext.Current.User.Identity.AuthenticationType,
                                    HttpContext.Current.User.Identity.IsAuthenticated)
                                .AppendLine("Sever Vars: {0}", request.ServerVariables)
                                .AppendLine("Headers: {0}", request.Headers)
                            : "")).ToString();
        }

        public static string Details(this HttpRequest request)
        {
            return Details(new HttpRequestWrapper(request));
        }

        public static string GetHostIpAddress(this HttpRequestBase request)
        {
            var hostAddress = request.UserHostAddress;
            var ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                hostAddress = ipAddress.Split(',').Last().Trim();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(hostAddress))
                {
                    hostAddress = request.ServerVariables["REMOTE_ADDR"];
                }
            }

            return hostAddress;
        }

        public static string GetHostIpAddress(this HttpRequest request)
        {
            var hostAddress = request.UserHostAddress;
            var ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                hostAddress = ipAddress.Split(',').Last().Trim();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(hostAddress))
                {
                    hostAddress = request.ServerVariables["REMOTE_ADDR"];
                }
            }

            return hostAddress;
        }
    }
}