#region

using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.RequestLogger;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models.Search;
using R2V2.Web.Models.Search.Fields;
using SearchRequest = R2V2.Core.RequestLogger.SearchRequest;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class RequestLoggerFilter : ActionFilterAttribute
    {
        public const string ApplicationSessionKey = "R2v2ApplicationSession";
        public const string RequestDataKey = "R2v2RequestData";
        private readonly ILog<RequestLoggerFilter> _log;
        private readonly bool _logRequest;
        private readonly bool _logSearch;
        private readonly RequestLoggerService _requestLoggerService;
        private readonly IUserSessionStorageService _userSessionStorageService;
        private readonly IWebSettings _webSettings;
        private readonly bool _isLocalDevelopment;

        private bool _error;

        public RequestLoggerFilter(bool logRequest = true, bool logSearch = false)
        {
            _log = ServiceLocator.Current.GetInstance<ILog<RequestLoggerFilter>>();
            _logRequest = logRequest;
            _logSearch = logSearch;
            _userSessionStorageService = ServiceLocator.Current.GetInstance<IUserSessionStorageService>();
            _requestLoggerService = ServiceLocator.Current.GetInstance<RequestLoggerService>();
            _webSettings = ServiceLocator.Current.GetInstance<IWebSettings>();
            
            // Check if we're in local development mode
            var isLocalDevConfig = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            _isLocalDevelopment = !string.IsNullOrEmpty(isLocalDevConfig) && 
                                  isLocalDevConfig.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //_log.DebugFormat("OnActionExecuting() - _logRequest: {0}, _logSearch: {1}, url: {2}", _logRequest, _logSearch, filterContext.RequestContext.HttpContext.Request.RawUrl);
            try
            {
                if (!_logRequest)
                {
                    return;
                }

                var requestData = CreateRequestData(filterContext);
                if (requestData != null)
                {
                    //_log.Debug(requestData.ToDebugString());
                    filterContext.HttpContext.RequestStorage().Put(RequestDataKey, requestData);
                }
                else
                {
                    var msg = new StringBuilder()
                        .AppendFormat("requestData is null - URL: {0}",
                            filterContext.RequestContext.HttpContext.Request.RawUrl)
                        .AppendFormat(", IP: {0}", filterContext.RequestContext.HttpContext.Request.GetHostIpAddress())
                        .AppendFormat(", UserAgent: {0}", filterContext.RequestContext.HttpContext.Request.UserAgent);
                    _log.Warn(msg.ToString());
                }
            }
            catch (Exception ex)
            {
                // this will swallow the exception
                // no need for bad requests or bad logging code to cause errors with the site
                _error = true;
                _log.Error(ex.Message, ex);
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //_log.DebugFormat("OnResultExecuted() - _logRequest: {0}, _logSearch: {1}, url: {2}", _logRequest, _logSearch, filterContext.RequestContext.HttpContext.Request.RawUrl);
            try
            {
                //This will prevent double logging of denied requests.
                var contextResult = filterContext.Result;
                if (contextResult.GetType() == typeof(RedirectResult))
                {
                    return;
                }

                if (!_logRequest || _error)
                {
                    return;
                }

                var requestData = filterContext.HttpContext.RequestStorage().Get<RequestData>(RequestDataKey);
                if (requestData != null)
                {
                    var duration = DateTime.Now.Subtract(requestData.RequestTimestamp).TotalMilliseconds;
                    requestData.RequestDuration = Convert.ToInt32(duration);

                    if (!requestData.LogRequest)
                    {
                        _log.InfoFormat("Skip logging request, {0}", requestData.ToDebugString());
                        return;
                    }

                    // In local development, just log locally and skip message queue
                    if (_isLocalDevelopment)
                    {
                        _log.Debug($"Request completed: {requestData.Url}, Duration: {requestData.RequestDuration}ms");
                        return;
                    }

                    // For non-local environments, use async with timeout
                    var task = Task.Run(() => _requestLoggerService.WriteRequestDataToMessageQueue(requestData));
                    if (!task.Wait(TimeSpan.FromSeconds(2))) // 2-second timeout
                    {
                        _log.Warn($"Request logging timed out after 2 seconds - skipping for {requestData.Url}");
                    }
                    else if (!task.Result)
                    {
                        _log.WarnFormat("Message was NOT written to request data message queue. {0}",
                            requestData.ToJsonString());
                    }
                }
            }
            catch (Exception ex)
            {
                // this will swallow the exception
                // no need for bad requests or bad logging code to cause errors with the site
                _error = true;
                _log.Error(ex.Message, ex);
            }
        }

        private RequestData CreateRequestData(ActionExecutingContext filterContext)
        {
            var requestLoggerData = GetRequestLoggerData(filterContext);

            if (requestLoggerData == null)
            {
                return null;
            }

            var requestData = new RequestData
            {
                RequestTimestamp = requestLoggerData.StartTime,
                InstitutionId = requestLoggerData.InstitutionId,
                UserId = requestLoggerData.UserId,
                Url = requestLoggerData.RawUrl,
                IpAddress = new IpAddress(requestLoggerData.IpAddress),
                Session = GetApplicationSession(),
                RequestId = requestLoggerData.RequestId,
                SearchRequest = GetSearchRequest(filterContext),
                Referrer = requestLoggerData.Referrer,
                CountryCode = requestLoggerData.CountryCode,
                ServerNumber = _webSettings.ServerNumber,
                AuthenticationType = requestLoggerData.AuthenticationType,
                HttpMethod = requestLoggerData.HttpMethod
            };
            return requestData;
        }

        private ApplicationSession GetApplicationSession()
        {
            try
            {
                var applicationSession = _userSessionStorageService.Get<ApplicationSession>(ApplicationSessionKey);
                //ApplicationSession applicationSession = null;
                var now = DateTime.Now;
                if (applicationSession == null)
                {
                    var context = HttpContext.Current;
                    //string referrer = (context.Request.UrlReferrer != null) ? context.Request.UrlReferrer.AbsoluteUri : null;
                    var referrer = context.Request.ServerVariables["HTTP_REFERER"];
                    applicationSession = new ApplicationSession
                        { SessionId = context.Session.SessionID, SessionStartTime = now, Referrer = referrer };
                    _userSessionStorageService.Put(ApplicationSessionKey, applicationSession);
                }

                applicationSession.SessionLastRequestTime = now;
                applicationSession.HitCount++;
                return applicationSession;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return null;
            }
        }

        private RequestLoggerData GetRequestLoggerData(ActionExecutingContext filterContext)
        {
            RequestLoggerData requestLoggerData = null;
            try
            {
                //requestLoggerData = filterContext.HttpContext.RequestStorage().Get<RequestLoggerData>("RequestLoggerData");
                requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
            }
            catch (Exception ex)
            {
                var msg = $"RequestLogger.GetRequestLoggerDate() EXCEPTION - {ex.Message}";
                _log.Error(msg, ex);
            }

            return requestLoggerData;
        }

        private SearchRequest GetSearchRequest(ActionExecutingContext filterContext)
        {
            if (!_logSearch)
            {
                return null;
            }

            var searchRequest = new SearchRequest();
            try
            {
                var searchQuery = filterContext.ActionParameters.Select(actionParameter => actionParameter.Value)
                    .OfType<SearchQuery>().FirstOrDefault();

                if (searchQuery != null)
                {
                    searchRequest.IsArchivedSearch = (searchQuery.Include & 0x2) == 0x2;
                    searchRequest.IsExternalSearch = false;

                    if (searchQuery.Filter == "drug")
                    {
                        searchRequest.SearchTypeId = (int)SearchType.DrugMonograph;
                    }
                    else
                    {
                        var searchField = SearchFieldsFactory.GetSearchFieldByCode(searchQuery.Field);
                        searchRequest.SearchTypeId = searchField != null
                            ? (int)searchField.LegacySearchType
                            : (int)SearchType.FullText;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return searchRequest;
        }
    }
}