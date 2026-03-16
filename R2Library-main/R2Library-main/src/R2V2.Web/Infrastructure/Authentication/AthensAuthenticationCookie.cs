#region

using System;
using System.Linq;
using System.Text;
using System.Web;
using R2V2.Core;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public class AthensAuthenticationCookie : IDebugInfo
    {
        private const string AthensCookieName = "athensAuthentication";

        public AthensAuthenticationCookie()
        {
            var athensCookie = HttpContext.Current.Request.Cookies[AthensCookieName];
            if (athensCookie == null)
            {
                return;
            }

            var athensCookieValue = athensCookie.Value;
            var athensCookieValueDictionary =
                athensCookieValue.Split('|').ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

            OrganizationId = athensCookieValueDictionary["organizationId"];
            Username = athensCookieValueDictionary["username"];
            PersistentUid = athensCookieValueDictionary["persistentUid"];
            FormatedDate = athensCookieValueDictionary["formatedDate"];
            ScopedAffiliation = athensCookieValueDictionary["scopedAffiliation"];
            TargetedId = athensCookieValueDictionary["targetedId"];

            Exists = true;
        }

        public bool Exists { get; }
        public string OrganizationId { get; }
        public string Username { get; }
        public string PersistentUid { get; }
        public string FormatedDate { get; }
        public string ScopedAffiliation { get; }
        public string TargetedId { get; }

        public string ToDebugString()
        {
            var sb = new StringBuilder().Append("AthensAuthenticationCookie = [");
            sb.AppendFormat("Exists: {0}", Exists);
            sb.AppendFormat(", ScopedAffiliation: {0}", ScopedAffiliation);
            sb.AppendFormat(", TargetedId: {0}", TargetedId);
            sb.AppendFormat(", OrganizationId: {0}", OrganizationId);
            sb.AppendFormat(", Username: {0}", Username);
            sb.AppendFormat(", PersistentUid: {0}", PersistentUid);
            sb.AppendFormat(", FormatedDate: {0}", FormatedDate);
            sb.Append("]");
            return sb.ToString();
        }

        public static void ClearAthensCookie(HttpResponseBase response)
        {
            if (response.Cookies[AthensCookieName] != null)
            {
                var deleteCookie = response.Cookies[AthensCookieName];
                response.Cookies.Remove(AthensCookieName);
                deleteCookie.Expires = DateTime.Now.AddDays(-1);
                deleteCookie.Value = null;
                response.Cookies.Add(deleteCookie);
            }
        }
    }
}