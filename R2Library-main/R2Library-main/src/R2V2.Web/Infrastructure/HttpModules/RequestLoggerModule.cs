#region

using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using log4net;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Contexts;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class RequestLoggerModule : IHttpModule
    {
        private const string RequestLoggerDataKey = "RequestLoggerData";
        private static readonly ILog<RequestLoggerModule> Log = new Log<RequestLoggerModule>();


        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
            context.PreRequestHandlerExecute += PreRequestHandlerExecute;

            context.AuthenticateRequest += AuthenticateRequest;
            context.AuthorizeRequest += AuthorizeRequest;
            context.ResolveRequestCache += ResolveRequestCache;
            context.AcquireRequestState += AcquireRequestState;
        }

        public void Dispose()
        {
        }

        private void BeginRequest(object sender, EventArgs e)
        {
            try
            {
                var requestInformation = new RequestInformation();
                SetLoggerParameters();

                if (!LogRequest())
                {
                    return;
                }

                var requestLoggerData = CreateRequestLoggerData(requestInformation.Request.RawUrl,
                    requestInformation.ClientAddress, requestInformation.Id);
                var requestInfo = new StringBuilder().AppendFormat(">>>>>> {0}, IP: {1}, {2}, [{3}]",
                    requestLoggerData.RawUrl, requestInformation.ClientAddress, requestInformation.Request.HttpMethod,
                    HttpContext.Current.Request.UserAgent);
                Log.Info(requestInfo.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void AuthenticateRequest(object sender, EventArgs e)
        {
            CheckCurrentRequestRunTime(1000, "AuthenticateRequest()");
        }

        private void AuthorizeRequest(object sender, EventArgs e)
        {
            CheckCurrentRequestRunTime(1000, "AuthorizeRequest()");
        }

        private void ResolveRequestCache(object sender, EventArgs e)
        {
            CheckCurrentRequestRunTime(1000, "ResolveRequestCache()");
        }

        private void AcquireRequestState(object sender, EventArgs e)
        {
            CheckCurrentRequestRunTime(1000, "AcquireRequestState()");
        }

        private void CheckCurrentRequestRunTime(int warningRunTime, string methodName)
        {
            try
            {
                var requestLoggerData = GetRequestLoggerData();
                if (requestLoggerData == null)
                {
                    return;
                }

                SetLoggerParameters();

                var runtime = requestLoggerData.GetCurrentRuntime().TotalMilliseconds;
                if (runtime > warningRunTime)
                {
                    Log.WarnFormat("{0} - runtime: {1}", methodName, runtime);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = new StringBuilder()
                    .Append(
                        $"rawUrl: {HttpContext.Current.Request.RawUrl}, ip: {HttpContext.Current.Request.GetHostIpAddress()}, [{HttpContext.Current.Request.UserAgent}]")
                    .AppendLine()
                    .Append($"Exception: {ex.Message}");
                Log.Error(errorMsg, ex);
                throw;
            }
        }

        private void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            RequestLoggerData requestLoggerData = null;
            try
            {
                if (!LogRequest())
                {
                    return;
                }

                requestLoggerData = GetRequestLoggerData();
                if (requestLoggerData == null)
                {
                    return;
                }

                requestLoggerData.SetAspSessionId();
                if (requestLoggerData.AspSessionId != "n/a")
                {
                    LogicalThreadContext.Properties["aspSessionId"] = requestLoggerData.AspSessionId;
                }

                var authenticationContext = ServiceLocator.Current.GetInstance<IAuthenticationContext>();
                if (authenticationContext == null)
                {
                    Log.WarnFormat("PreRequestHandlerExecute() - authenticationContext is null!!!!");
                }
                else if (authenticationContext.IsAuthenticated)
                {
                    var authenticatedInstitution = authenticationContext.AuthenticatedInstitution;
                    if (authenticatedInstitution != null)
                    {
                        if (authenticatedInstitution.User != null)
                        {
                            requestLoggerData.UserId = authenticatedInstitution.User.Id;
                            requestLoggerData.UserName = authenticatedInstitution.User.UserName;
                        }

                        requestLoggerData.IsAuthenticated = authenticationContext.IsAuthenticated;
                        requestLoggerData.InstitutionId = authenticatedInstitution.Id;
                        requestLoggerData.InstitutionAccountNumber = authenticatedInstitution.AccountNumber;
                        requestLoggerData.InstitutionName = authenticatedInstitution.Name;
                        requestLoggerData.AuthenticationType = authenticatedInstitution.AuthenticationMethod.ToString();
                    }
                    else
                    {
                        Log.Warn("identity is null"); // don't think this should happen.
                    }
                }
                else
                {
                    Log.Info("authenticationContext.IsAuthenticated is false");
                }
                //Log.InfoFormat("GetCurrentRuntime: {0} ms", requestLoggerData.GetCurrentRuntime().TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendFormat("{0}", requestLoggerData == null ? "N/A" : requestLoggerData.ToDebugString())
                    .AppendLine();
                msg.AppendFormat("Exception: {0}", ex.Message);
                Log.Error(msg.ToString(), ex);
            }
        }

        private void EndRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            try
            {
                var requestLoggerData = GetRequestLoggerData();
                if (requestLoggerData == null)
                {
                    return;
                }

                requestLoggerData.EndRequestTiming();

                var requestInfo = new StringBuilder()
                    .AppendFormat("<<<<<< {0:0.000}, {1}, IP: {2}, IsAuthenticated: {3}, StatusCode: {4}",
                        requestLoggerData.TimeSpan.TotalSeconds, requestLoggerData.RawUrl,
                        requestLoggerData.IpAddress, requestLoggerData.IsAuthenticated, context.Response.StatusCode);

                if (requestLoggerData.IsAuthenticated)
                {
                    requestInfo.Append(
                        $", User = [Id:{requestLoggerData.UserId}, UserName:{requestLoggerData.UserName}]");
                    requestInfo.Append(
                        $", Institution = [Id:{requestLoggerData.InstitutionId}, AcctNum: {requestLoggerData.InstitutionAccountNumber}, Name: {requestLoggerData.InstitutionName}]");
                    requestInfo.Append($", AuthenticationType: {requestLoggerData.AuthenticationType}");
                }

                if (requestLoggerData.TimeSpan.TotalSeconds < 5.0)
                {
                    Log.PageTimeOk(requestInfo.ToString());
                }
                else
                {
                    if (context.Handler is MvcHandler mvcHandler)
                    {
                        var routeData = mvcHandler.RequestContext.RouteData;
                        if (routeData != null)
                        {
                            var controller = routeData.Values["controller"].ToString();
                            var action = routeData.Values["action"].ToString();
                            LogicalThreadContext.Properties["controllerAndAction"] = $"{controller}-{action}";
                        }
                    }

                    if (requestLoggerData.TimeSpan.TotalSeconds < 10.0)
                    {
                        Log.PageTimeWarn(requestInfo.ToString());
                    }
                    else
                    {
                        var errorMsg = new StringBuilder()
                            .AppendLine("The requested page took more than 10 seconds to render.")
                            .AppendLine($"Page: {requestLoggerData.RawUrl}")
                            .AppendLine($"Page execution time: {requestLoggerData.TimeSpan.TotalSeconds:0.000}")
                            .AppendLine($"Request start time: {requestLoggerData.StartTime:HH:mm:ss.fff}").AppendLine()
                            .AppendLine(
                                "This message does not indicate there was an error with the site. It indicates only that requested page took an unusually long time to be rendered. Please monitor to make sure the site is performing as expected.")
                            .AppendLine(requestInfo.ToString());
                        Log.PageTimeAlert(errorMsg.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            finally
            {
                context.Items.Remove(RequestLoggerDataKey);
            }
        }

        private bool LogRequest()
        {
            var rawUrl = HttpContext.Current.Request.RawUrl;
            if (rawUrl.ToLower().Contains("/_static/") || rawUrl.ToLower().Contains("/trackback/") ||
                rawUrl.ToLower().Contains("/counter/sushiservice"))
            {
                return false;
            }

            var fileExtension = HttpContext.Current.Request.CurrentExecutionFilePathExtension;
            if (fileExtension == ".js" || fileExtension == ".css" || fileExtension == ".jpg" ||
                fileExtension == ".png" ||
                fileExtension == ".gif" || fileExtension == ".txt" || fileExtension == ".htm" ||
                fileExtension == ".html" ||
                fileExtension == ".pdf" || fileExtension == ".doc" || fileExtension == ".xls" ||
                fileExtension == ".ico")
            {
                return false;
            }

            return true;
        }

        /// <param name="requestId"> </param>
        private RequestLoggerData CreateRequestLoggerData(string rawUrl, string ipAddressV4, string requestId)
        {
            var requestLoggerData = new RequestLoggerData(rawUrl, ipAddressV4, requestId);
            try
            {
                var context = HttpContext.Current;
                if (context.Items.Contains(RequestLoggerDataKey))
                {
                    var data = (RequestLoggerData)context.Items[RequestLoggerDataKey];
                    if (data == null)
                    {
                        Log.WarnFormat("'{0}' was null", RequestLoggerDataKey);
                    }
                    else
                    {
                        Log.ErrorFormat("CreateRequestLoggerData() - THIS SHOULD NOT HAPPEN! - {0}", data);
                    }

                    context.Items.Remove(RequestLoggerDataKey);
                }

                context.Items.Add(RequestLoggerDataKey, requestLoggerData);

                requestLoggerData.Referrer = context.Request.HttpReferrer();

                requestLoggerData.CountryCode =
                    CountryCodeService.GetCountryCodeFromIpAddressFromDb(ipAddressV4, context);

                requestLoggerData.HttpMethod = context?.Request?.HttpMethod;
            }
            catch (Exception ex)
            {
                var msg = $"RequestLogger.CreateRequestLoggerDate() EXCEPTION - {ex.Message}";
                Log.Error(msg, ex);
            }

            return requestLoggerData;
        }

        public static RequestLoggerData GetRequestLoggerData()
        {
            RequestLoggerData requestLoggerData = null;
            try
            {
                var context = HttpContext.Current;
                if (context != null && context.Items.Contains(RequestLoggerDataKey))
                {
                    requestLoggerData = (RequestLoggerData)context.Items[RequestLoggerDataKey];
                }
            }
            catch (Exception ex)
            {
                var msg = $"RequestLogger.GetRequestLoggerDate() EXCEPTION - {ex.Message}";
                Log.Error(msg, ex);
            }

            return requestLoggerData;
        }

        private static void SetLoggerParameters()
        {
            if (LogicalThreadContext.Properties["requestId"] == null ||
                string.IsNullOrWhiteSpace(LogicalThreadContext.Properties["requestId"].ToString()))
            {
                var requestInformation = new RequestInformation();
                LogicalThreadContext.Properties["requestId"] = requestInformation.Id;
                LogicalThreadContext.Properties["clientAddress"] = requestInformation.ClientAddress;
                LogicalThreadContext.Properties["url"] = requestInformation.Request.Url;
                LogicalThreadContext.Properties["httpReferrer"] = requestInformation.Request.HttpReferrer();
                LogicalThreadContext.Properties["userAgent"] = requestInformation.Request.UserAgent;
            }

            if (LogicalThreadContext.Properties["aspSessionId"] == null)
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    LogicalThreadContext.Properties["aspSessionId"] = HttpContext.Current.Session.SessionID;
                }
            }
        }
    }
}