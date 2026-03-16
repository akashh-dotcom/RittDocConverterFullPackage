#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class RecentCookieService
    {
        const string InstitutionCookieName = "R2V2.Recent.Institutions";
        const string ResourceCookieName = "R2V2.Recent.Resources";
        private static ILog<RecentCookieService> _log;

        public RecentCookieService(ILog<RecentCookieService> log)
        {
            _log = log;
        }

        public void SetRecentInstitutionsCookie(int institutionId, HttpResponseBase response, HttpRequestBase request)
        {
            SetRecentCookie(InstitutionCookieName, institutionId, response, request);
        }

        public int[] GetRecentInstitutionIds(HttpRequestBase request)
        {
            return GetRecentIds(InstitutionCookieName, request);
        }

        public void SetRecentResourcesCookie(int resourceId, HttpResponseBase response, HttpRequestBase request)
        {
            SetRecentCookie(ResourceCookieName, resourceId, response, request);
        }

        public int[] GetRecentResourceIds(HttpRequestBase request)
        {
            return GetRecentIds(ResourceCookieName, request);
        }


        private static void SetRecentCookie(string cookieName, int value, HttpResponseBase response,
            HttpRequestBase request)
        {
            string valueString = null;
            if (request.Cookies[cookieName] != null)
            {
                valueString = request.Cookies[cookieName].Value;
                var oldCookie = new HttpCookie(cookieName) { Expires = DateTime.Now.AddDays(-1) };
                response.Cookies.Add(oldCookie);
            }

            var cookie = new HttpCookie(cookieName) { Expires = DateTime.Now.AddDays(365) };
            var valuesList = new List<string>();

            if (!string.IsNullOrWhiteSpace(valueString))
            {
                var valueIds = valueString.Split(',');
                if (valueIds.Contains(value.ToString()))
                {
                    valuesList.Add(value.ToString());
                    valuesList.AddRange(valueIds.Where(x => x != value.ToString()).Select(x => x));
                }
                else
                {
                    valuesList.Add(value.ToString());
                    valuesList.AddRange(valueIds.Select(x => x));
                }
            }
            else
            {
                valuesList.Add(value.ToString());
            }

            var recentCount = valuesList.Count() > 10 ? 10 : valuesList.Count();

            var sb = new StringBuilder();
            for (var i = 0; i < recentCount; i++)
            {
                sb.AppendFormat("{0}{1}", valuesList[i], i + 1 != valuesList.Count() ? "," : "");
            }

            cookie.Value = sb.ToString();
            response.Cookies.Add(cookie);
        }

        private static int[] GetRecentIds(string cookieName, HttpRequestBase request)
        {
            if (request.Cookies[cookieName] == null)
            {
                return new int[0];
            }

            string valueIdsString = null;
            try
            {
                valueIdsString = request.Cookies[cookieName].Value;
                var idList = new List<int>();

                if (!string.IsNullOrWhiteSpace(valueIdsString))
                {
                    var test = valueIdsString.Split(',');
                    if (test.Any())
                    {
                        foreach (var s in test)
                        {
                            int x;
                            int.TryParse(s, out x);
                            if (x > 0)
                            {
                                idList.Add(x);
                            }
                        }
                    }
                }

                return idList.ToArray();
            }
            catch (Exception ex)
            {
                _log.Info(ex.Message, ex);
                _log.InfoFormat("valueIdsString : {0}", valueIdsString);
                return new int[0];
            }
        }
    }
}