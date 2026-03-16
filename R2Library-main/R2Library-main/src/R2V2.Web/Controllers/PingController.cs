#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Mvc;
using R2V2.Core;
using R2V2.DataAccess.DtSearch;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Infrastructure;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.RequestLogger;
using R2V2.Web.Models.Ping;

#endregion

namespace R2V2.Web.Controllers
{
    [RequestLoggerFilter(false)]
    public class PingController : Controller
    {
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly ILog<PingController> _log;
        private readonly IQueryable<Ping> _pings;
        private readonly RequestInformation _requestInformation;
        private readonly Search _search;

        /// <param name="requestInformation"> </param>
        public PingController(ILog<PingController> log, IQueryable<Ping> pings, RequestInformation requestInformation,
            Search search, IApplicationWideStorageService applicationWideStorageService)
        {
            _pings = pings;
            _log = log;
            _requestInformation = requestInformation;
            _search = search;
            _applicationWideStorageService = applicationWideStorageService;
        }

        [IgnoreRequest]
        public ActionResult Index()
        {
            var model = new PingData { ClientIpAddress = _requestInformation.ClientAddress };

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var ping = _pings.Single(x => x.Id == 1);

                model.DatabaseStatus = ping.StatusCode;

                stopwatch.Stop();

                if (stopwatch.ElapsedMilliseconds > 250)
                {
                    _log.WarnFormat("Ping query took longer than 250 ms, query time: {0} ms",
                        stopwatch.ElapsedMilliseconds);
                }
                else if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    _log.ErrorFormat(
                        "SERVER PERFORMANCE ALERT - Ping query took longer than 2 seconds, query time: {0} ms",
                        stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                model.DatabaseStatus = "R2v2DbStatusException";
                _log.Error(ex.Message, ex);
            }


            model.AssemplyTimestamp = GetAssemblyTimestamp();
            model.AppStartTimestamp = GetAppStartTimestamp();

            //model.MachineName = Environment.MachineName;
            Session.Abandon();
            _log.DebugFormat("DatabaseStatus: {0}, ClientIpAddress: {1}, Version: {2}, MachineName: {3}",
                model.DatabaseStatus, model.ClientIpAddress, model.Version, model.MachineName);
            return View(model);
        }

        [IgnoreRequest]
        public ActionResult JustWork()
        {
            var model = new PingData { ClientIpAddress = _requestInformation.ClientAddress };

            try
            {
            }
            catch (Exception ex)
            {
                model.DatabaseStatus = "R2v2DbStatusException";
                _log.Error(ex.Message, ex);
            }

            model.MachineName = Environment.MachineName;

            Session.Abandon();
            return View("Index", model);
        }

        [IgnoreRequest]
        public ActionResult IndexStatus(string runGc)
        {
            var model = new PingIndexStatus { ClientIpAddress = _requestInformation.ClientAddress };

            try
            {
                model.SetIndexStatus(_search.GetIndexStatus());
                model.DatabaseStatus = "R2v2DbStatusOk";

                if (!string.IsNullOrWhiteSpace(runGc) && runGc == "yes-123")
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    _log.Error("GC was run called vai the web site");
                }
            }
            catch (Exception ex)
            {
                model.DatabaseStatus = "R2v2DbStatusException";
                _log.Error(ex.Message, ex);
            }

            model.MachineName = Environment.MachineName;

            return View(model);
        }

        /// <summary>
        ///     Return just a string of the status
        /// </summary>
        /// <param name="x">name of the tool used to monitor the site</param>
        /// <param name="y">lb=load balancer</param>
        /// <param name="z">additional information</param>
        /// <param name="msg">message to return if supplied and code is 100 to 599</param>
        /// <param name="code">100 to 599, http status code, used for testing</param>
        [IgnoreRequest]
        public ActionResult Min(string x, string y, string z, string msg, int code = 0)
        {
            string status;
            try
            {
                if (!string.IsNullOrEmpty(msg) && IsValidCode(code))
                {
                    Response.StatusCode = code;
                    status = msg;
                }
                else
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var ping = _pings.Single(r => r.Id == 1);

                    status = ping.StatusCode;

                    stopwatch.Stop();

                    if (stopwatch.ElapsedMilliseconds > 250)
                    {
                        _log.WarnFormat("Ping query took longer than 250 ms, query time: {0} ms",
                            stopwatch.ElapsedMilliseconds);
                    }
                    else if (stopwatch.ElapsedMilliseconds > 2000)
                    {
                        _log.ErrorFormat(
                            "SERVER PERFORMANCE ALERT - Ping query took longer than 2 seconds, query time: {0} ms",
                            stopwatch.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                status = "R2v2DbStatusException";
                Response.StatusCode = 500;
                _log.Error(ex.Message, ex);
            }

            Session.Abandon();
            _log.DebugFormat("status: {0}, Response.StatusCode: {1}", status, Response.StatusCode);
            return Content(status);
        }

        private bool IsValidCode(int code)
        {
            return code >= 100 && code <= 599;
        }

        private DateTime GetAssemblyTimestamp()
        {
            const string key = "Assembly.Timestamp";
            if (!_applicationWideStorageService.Has(key))
            {
                var assemblies = BuildManager.GetReferencedAssemblies().OfType<Assembly>()
                    .Concat(AppDomain.CurrentDomain.GetAssemblies()).Distinct().OrderBy(o => o.FullName).ToArray();
                var webAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "R2V2.Web");

                var fileDateTime = webAssembly != null
                    ? System.IO.File.GetLastWriteTime(webAssembly.Location)
                    : DateTime.MinValue;
                _applicationWideStorageService.Put(key, fileDateTime);
            }

            return _applicationWideStorageService.Get<DateTime>(key);
        }

        private DateTime GetAppStartTimestamp()
        {
            const string key = "AppStart.Timestamp";
            if (!_applicationWideStorageService.Has(key))
            {
                _applicationWideStorageService.Put(key, DateTime.Now);
            }

            return _applicationWideStorageService.Get<DateTime>(key);
        }
    }
}