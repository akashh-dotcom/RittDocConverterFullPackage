#region

using System.Web;

#endregion

namespace R2V2.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string HttpReferrer(this HttpRequest request)
        {
            var httpReferrer = request.ServerVariables["HTTP_REFERER"];
            return string.IsNullOrWhiteSpace(httpReferrer) ? string.Empty : httpReferrer;
        }

        public static string HttpReferrer(this HttpRequestBase request)
        {
            var httpReferrer = request.ServerVariables["HTTP_REFERER"];
            return string.IsNullOrWhiteSpace(httpReferrer) ? string.Empty : httpReferrer;
        }
    }
}