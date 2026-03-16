#region

using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using R2V2.Infrastructure.GoogleAnalytics;
using R2V2.Infrastructure.Logging;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.GoogleAnalytics
{
    public class AnalyticsService
    {
        private readonly ILog<AnalyticsService> _log;
        private readonly IWindowsServiceSettings _windowsServiceSettings;

        public AnalyticsService(ILog<AnalyticsService> log, IWindowsServiceSettings windowsServiceSettings)
        {
            _log = log;
            _windowsServiceSettings = windowsServiceSettings;
        }

        public bool ProcessGoogleRequestData(GoogleRequestData googleRequestData)
        {
            try
            {
                _log.InfoFormat("{0}", googleRequestData.ToDebugString());
                return SendData(googleRequestData);
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendLine("Google Analytics Data:").Append("\t").AppendLine(googleRequestData.ToDebugString());
                msg.Append(ex.Message);
                _log.Error(msg, ex);
            }

            return false;
        }

        private bool SendData(GoogleRequestData googleRequestData)
        {
            var ascii = new ASCIIEncoding();
            var postBytes = ascii.GetBytes(googleRequestData.RequestData);

            var request = (HttpWebRequest)WebRequest.Create(googleRequestData.UrlToSendData);
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            request.UserAgent = googleRequestData.UserAgent;
            request.Timeout = _windowsServiceSettings.GoogleAnalyticsTimeoutInMilliseconds;

            //_log.InfoFormat("request.UserAgent -> {0}", request.UserAgent);
            //_log.InfoFormat("googleRequestData.UserAgent-> {0}", googleRequestData.UserAgent);
            //_log.InfoFormat("PostData - request.ContentLength - {0}", request.ContentLength);

            _log.DebugFormat("ContentLength: {0}, Timeout: {1:#,###}ms, UserAgent: {1}", request.ContentLength,
                request.UserAgent, request.Timeout);

            var postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _log.Debug("Start the request");
            var webResponse = (HttpWebResponse)request.GetResponse();
            _log.DebugFormat("webResponse completed in {0:#,###}ms", stopwatch.ElapsedMilliseconds);
            webResponse.Close();
            stopwatch.Stop();

            _log.InfoFormat("StatusCode: {0}, total request time: {1:#,###}ms", webResponse.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return webResponse.StatusCode == HttpStatusCode.OK;
        }
    }
}