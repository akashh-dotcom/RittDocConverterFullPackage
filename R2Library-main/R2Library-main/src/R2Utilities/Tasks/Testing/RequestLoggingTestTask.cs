#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using R2Library.Data.ADO.R2Utility;
using R2V2.Core.RequestLogger;

#endregion

namespace R2Utilities.Tasks.Testing
{
    public class RequestLoggingTestTask : TaskBase
    {
        private readonly RequestLoggerService _requestLoggerService;
        private int _delay;
        private int _requestsPerThread;

        private int _threadCount;

        /// <summary>
        ///     -RequestLoggingTestTask -threadCount=5 -requestsPerThread=25 -delay=2
        ///     -RequestLoggingTestTask -threadCount=1 -requestsPerThread=250 -delay=2
        /// </summary>
        public RequestLoggingTestTask(RequestLoggerService requestLoggerService)
            : base("RequestLoggingTestTask", "-RequestLoggingTestTask", "20", TaskGroup.DiagnosticsMaintenance,
                "Task will send many test request logger messages", true)
        {
            _requestLoggerService = requestLoggerService;
        }

        public override void Run()
        {
            TaskResult.Information = TaskDescription;
            var step = new TaskResultStep { Name = "SendRequestLoggerMessages", StartTime = DateTime.Now };
            TaskResult.AddStep(step);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                int.TryParse(GetArgument("threadCount") ?? "3", out _threadCount);
                int.TryParse(GetArgument("requestsPerThread") ?? "10", out _requestsPerThread);
                int.TryParse(GetArgument("delay") ?? "1", out _delay);

                //RequestLoggerService requestLoggerService = ServiceLocator.Current.GetInstance<RequestLoggerService>();

                var threads = new Dictionary<string, Thread>();
                for (var i = 0; i < _threadCount; i++)
                {
                    var threadName = $"Thread{i:0000#}";
                    var thread = new Thread(SendRequestLoggerMessages) { Name = threadName };
                    thread.Start(); //If you wish to start them straight away and call MethodToExe
                    threads.Add(threadName, thread);
                }

                while (true)
                {
                    var aliveCount = threads.Select(keyValuePair => keyValuePair.Value).Count(thread => thread.IsAlive);

                    if (aliveCount == 0)
                    {
                        break;
                    }

                    Thread.Sleep(1000);
                }

                stopwatch.Stop();
                step.CompletedSuccessfully = true;
                step.Results =
                    $"_threadCount: {_threadCount}, _requestsPerThread: {_requestsPerThread}, _delay: {_delay}, ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        public void SendRequestLoggerMessages()
        {
            var applicationSession = new ApplicationSession
            {
                HitCount = 0,
                Referrer = "",
                SessionId = "n/a",
                SessionLastRequestTime = DateTime.Now,
                SessionStartTime = DateTime.Now
            };

            var threadName = Thread.CurrentThread.Name;

            for (var i = 0; i < _requestsPerThread; i++)
            {
                applicationSession.HitCount = applicationSession.HitCount + 1;
                applicationSession.SessionLastRequestTime = DateTime.Now;

                var requestData = new RequestData
                {
                    RequestTimestamp = DateTime.Now,
                    InstitutionId = 1,
                    UserId = 0,
                    Url = $"/RequestLoggerTesting/{threadName}/{i}",
                    IpAddress = new IpAddress("127.0.0.1"),
                    Session = applicationSession,
                    RequestId = Guid.NewGuid().ToString(),
                    SearchRequest = null,
                    Referrer = "",
                    CountryCode = "US",
                    ServerNumber = 99
                };

                if (_delay > 0)
                {
                    Thread.Sleep(_delay);
                }

                var duration = DateTime.Now.Subtract(requestData.RequestTimestamp).TotalMilliseconds;
                requestData.RequestDuration = Convert.ToInt32(duration);

                if (!_requestLoggerService.WriteRequestDataToMessageQueue(requestData))
                {
                    Log.ErrorFormat("Error writing request to message queue: {0}", requestData.ToJsonString());
                }
            }
        }
    }
}