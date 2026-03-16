#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class MakeTocRequestsTask : TaskBase, ITask
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private string _isbns;

        private int _maxBatchSize;
        private int _maxResourceId;
        private int _minResourceId;
        private bool _orderBatchDescending;
        private ResourceCoreDataService _resourceCoreDataService;
        private string _tocurl;


        /// <summary>
        ///     -MakeTocRequestsTask -logFileSuffix=grp2 -maxBatchSize=10 -minResourceId=7000 -maxResourceId=8000
        ///     -orderBatchDescending=true -tocurl=https://dev.r2library.com/Resource/Title/
        ///     -MakeTocRequestsTask -logFileSuffix=grp2 -maxBatchSize=1000 -minResourceId=1 -maxResourceId=1000
        ///     -orderBatchDescending=true -tocurl=https://rittweb6.r2library.com/Resource/Title/
        /// </summary>
        public MakeTocRequestsTask(IR2UtilitiesSettings r2UtilitiesSettings)
            : base("MakeTocRequestsTask", "-MakeTocRequestsTask", "80", TaskGroup.ContentLoading,
                "Makes TOC requests for the resources specified via the URL specified", true)
        {
            //_resources = resources;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _maxBatchSize = GetArgumentInt32("maxBatchSize", _r2UtilitiesSettings.HtmlIndexerBatchSize);
            _minResourceId = GetArgumentInt32("minResourceId", 0);
            _maxResourceId = GetArgumentInt32("maxResourceId", 100000);
            _orderBatchDescending =
                GetArgumentBoolean("orderBatchDescending", _r2UtilitiesSettings.OrderBatchDescending);
            _isbns = GetArgument("isbn");
            _tocurl = GetArgument("tocurl");

            Log.InfoFormat("-maxBatchSize: {0}, -minResourceId: {1}, -maxResourceId: {2}, -orderBatchDescending: {3}",
                _maxBatchSize, _minResourceId, _maxResourceId, _orderBatchDescending);

            Log.InfoFormat("-tocurl: {0}", _tocurl);

            Log.InfoFormat("-isbns: {0}", _isbns);

            SetSummaryEmailSetting(true, true, 100);
        }


        public override void Run()
        {
            var taskInfo = new StringBuilder();
            var summaryStep = new TaskResultStep
                { Name = "MakeTocRequestsSummary", StartTime = DateTime.Now, Results = string.Empty };
            TaskResult.AddStep(summaryStep);
            UpdateTaskResult();

            // init
            _resourceCoreDataService = new ResourceCoreDataService();
            var summaryStepResults = new StringBuilder();

            try
            {
                taskInfo.AppendFormat("Making TOC requests for {0} resources.", _maxBatchSize);
                Log.InfoFormat("Making TOC requests for {0} resources.", _maxBatchSize);

                IList<ResourceCore> resourceCores = _resourceCoreDataService.GetActiveAndArchivedResources(true,
                    _minResourceId, _maxResourceId, _maxBatchSize,
                    _isbns?.Split(','));

                taskInfo.AppendFormat("{0} resources to process", resourceCores.Count);
                Log.InfoFormat("{0} resources to process", resourceCores.Count);

                var resourceCount = 0;

                summaryStepResults.AppendFormat("maxBatchSize: {0}, _minResourceId: {1}, _maxResourceId: {2} - ISNBs:",
                    _maxBatchSize, _minResourceId, _maxResourceId);
                summaryStepResults.AppendLine();

                foreach (var resourceCore in resourceCores)
                {
                    resourceCount++;
                    Log.InfoFormat("Request TOC for ISBN: {0}, resource {1} of {2}", resourceCore.Isbn, resourceCount,
                        resourceCores.Count);
                    summaryStepResults.AppendLine(MakeTocRequest(resourceCore.Isbn, resourceCore.Id));
                }

                if (resourceCount == 0)
                {
                    var step = new TaskResultStep
                    {
                        Name = "zero TOCs to requested.",
                        StartTime = DateTime.Now,
                        CompletedSuccessfully = true,
                        Results = "No resources",
                        EndTime = DateTime.Now
                    };
                    TaskResult.AddStep(step);
                    UpdateTaskResult();
                    summaryStepResults.Insert(0, "WARNING - NO TOCS REQUESTED!!!! ");
                }
                else
                {
                    summaryStepResults.Insert(0, $"TOC request count: {resourceCount}, ");
                }

                summaryStep.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                //summaryStep.Results = string.Format("EXCEPTION: {0}\r\n\t{1}", ex.Message, summaryStep.Results);
                summaryStepResults.Insert(0, $"EXCEPTION: {ex.Message}\r\n");
                summaryStep.CompletedSuccessfully = false;
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                TaskResult.Information = taskInfo.ToString();
                summaryStep.EndTime = DateTime.Now;
                summaryStep.Results = summaryStepResults.ToString();
                UpdateTaskResult();
            }
        }

        private string MakeTocRequest(string isbn, int resourceId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string results = null;
            var url = $"{_tocurl}{isbn}";
            Log.DebugFormat("url: {0}", url);
            long contentLength = 0;
            try
            {
                /* Initialize the web request. */
                var webRequest = (HttpWebRequest)WebRequest.Create(url);

                /* Optionally specify the User Agent. */
                webRequest.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727) - R2 Toc Crawler";

                /* Make a synchronous request and convert the response into something we can consume. */
                //webRequest.Timeout = 2500;
                webRequest.Timeout = 30000;
                webRequest.Proxy =
                    null; // SJS - 9/9/2013 - http://stackoverflow.com/questions/2519655/httpwebrequest-is-extremely-slow
                using (var webResponse = webRequest.GetResponse())
                {
                    contentLength = webResponse.ContentLength;

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
                Log.WarnFormat("URL: {0}, WebException: {1}", url, webEx.Message);
            }
            catch (Exception ex)
            {
                // log error as warning and swallow exception
                Log.ErrorFormat("URL: {0}, Exception: {1}", url, ex.Message);
            }

            stopwatch.Stop();
            var status =
                $"time: {stopwatch.ElapsedMilliseconds:#,###} ms, url: {url}, content length: {contentLength:#,###}, resourceId: {resourceId}";
            Log.Info(status);
            return status;
        }
    }
}