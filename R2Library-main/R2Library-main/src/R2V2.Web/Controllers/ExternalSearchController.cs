#region

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Xml;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Controllers
{
    [RequestLoggerFilter(false)]
    [SessionState(SessionStateBehavior.Disabled)]
    public class ExternalSearchController : R2BaseController
    {
        private readonly ILog<ExternalSearchController> _log;
        private readonly IWebSettings _webSettings;

        public ExternalSearchController(ILog<ExternalSearchController> log, IWebSettings webSettings)
        {
            _log = log;
            _webSettings = webSettings;
        }

        public ActionResult Counts(string q)
        {
            var pubMedCount = PubMedSearchCount(q);
            var meshCount = MeshSearchCount(q);
            return Json(new { PubMed = $"{pubMedCount:#,##0}", Mesh = $"{meshCount:#,##0}" },
                JsonRequestBehavior.AllowGet);
        }

        public int PubMedSearchCount(string query)
        {
            var url = string.Format(_webSettings.ExternalSearchPubMedUrl, query);
            return GetNlmSearchCount(url);
        }

        public int MeshSearchCount(string query)
        {
            var url = string.Format(_webSettings.ExternalSearchMeshUrl, query);
            return GetNlmSearchCount(url);
        }

        public int GetNlmSearchCount(string url)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var resultCount = 0;
            try
            {
                var results = GetUrlResponse(url);

                if (!string.IsNullOrWhiteSpace(results))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(results);

                    var value = GetXmlNodeValue(xmlDoc, "//eSearchResult/Count");
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        int.TryParse(value, out resultCount);
                    }
                }
            }
            catch (Exception ex)
            {
                // log error as warning and swallow exception
                var msg = new StringBuilder();
                msg.AppendFormat("Exception: {0}", ex.Message).AppendLine();
                msg.AppendFormat("\tURL: {0}", url).AppendLine();
                _log.Warn(msg.ToString());
            }

            stopwatch.Stop();
            _log.DebugFormat("GetNlmSearchCount() time: {0} ms, count: {1}, URL: {2}", stopwatch.ElapsedMilliseconds,
                resultCount, url);
            return resultCount;
        }


        public string GetUrlResponse(string url)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string results = null;
            try
            {
                /* Initialize the web request. */
                var webRequest = (HttpWebRequest)WebRequest.Create(url);

                /* Optionally specify the User Agent. */
                webRequest.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                /* Make a synchronous request and convert the response into something we can consume. */
                //webRequest.Timeout = 2500;
                webRequest.Timeout = _webSettings.ExternalSearchTimeoutInMilliseconds;
                webRequest.Proxy =
                    null; // SJS - 9/9/2013 - http://stackoverflow.com/questions/2519655/httpwebrequest-is-extremely-slow
                using (var webResponse = webRequest.GetResponse())
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var stream = new StreamReader(responseStream))
                            {
                                results = stream.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                // log error as warning and swallow exception
                _log.WarnFormat("URL: {0}, WebException: {1}", url, webEx.Message);
            }
            catch (Exception ex)
            {
                // log error as warning and swallow exception
                _log.ErrorFormat("URL: {0}, Exception: {1}", url, ex.Message);
            }

            stopwatch.Stop();
            _log.DebugFormat("GetUrlResponse time: {0} ms, url: {1}, ServicePointManager.DefaultConnectionLimit: {2}",
                stopwatch.ElapsedMilliseconds, url,
                ServicePointManager.DefaultConnectionLimit);
            return results;
        }

        public static string GetXmlNodeValue(XmlDocument xmlDoc, string xpath)
        {
            if (null != xmlDoc.DocumentElement)
            {
                var xmlNodeList = xmlDoc.DocumentElement.SelectNodes(xpath);

                if (null == xmlNodeList)
                {
                    return string.Empty;
                }

                if (xmlNodeList.Count == 0)
                {
                    return string.Empty;
                }

                if (xmlNodeList.Count > 1)
                {
                    return string.Empty;
                }

                var value = xmlNodeList[0].InnerText;
                return value;
            }

            return "";
        }
    }
}