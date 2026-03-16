#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    ///
    public class ProcessLog4NetFileTask : TaskBase, ITask
    {
        private const string InsertStatement_RabbitMqSendIssue =
            "insert into _temp_RabbitMqSendIssue([timestamp], sendTime, sessionId, requestId, logLevel, [server]) values(@Timestamp, @SendTime, @SessionId, @RequestId, @LogLevel, @Server)";

        private const string InsertStatement_RequestStartEnd =
            "insert into _temp_RequestStartEnd([timestamp], url, [start], exceptionHash, sessionId, requestId, logLevel, [server]) values(@Timestamp, @Url, @Start, @ExceptionHash, @SessionId, @RequestId, @LogLevel, @Server)";

        private string _job;

        private string _path;
        private string _server;
        protected new string TaskName = "ProcessLog4NetFileTask";

        /// <summary>
        ///     -ProcessLog4NetFileTask -job=requestQ -path=D:\Temp\R2-Prod-Logs\batch01\rittweb6 -server=rittweb6
        /// </summary>
        public ProcessLog4NetFileTask()
            : base("ProcessLog4NetFileTask", "-ProcessLog4NetFileTask", "29", TaskGroup.DiagnosticsMaintenance,
                "Task for processing log4net files (uses args to specify functionality)", true)
        {
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _path = GetArgument("path");
            _server = GetArgument("server");
            _job = GetArgument("job");

            Log.InfoFormat("-job: {0}, -server: {1}, -path: {2}", _job, _server, _path);
        }

        public override void Run()
        {
            var taskInfo = new StringBuilder();

            var summaryStep = new TaskResultStep
                { Name = "ProcessLog4NetFile", StartTime = DateTime.Now, Results = string.Empty };
            TaskResult.AddStep(summaryStep);
            UpdateTaskResult();

            // init
            try
            {
                var dirInfo = new DirectoryInfo(_path);

                var startOnlyEvents = new Dictionary<string, LogEvent>();
                var endOnlyEvents = new Dictionary<string, LogEvent>();
                var nullRequestIdEvents = new List<LogEvent>();


                if (dirInfo.Exists)
                {
                    var fileInfos = dirInfo.GetFiles();

                    var totalFileCount = 0;
                    var totalFileLineCount = 0;
                    var totalMessageSendTimes = 0;

                    foreach (var fileInfo in fileInfos)
                    {
                        if (fileInfo.Extension == ".7z")
                        {
                            continue;
                        }

                        totalFileCount++;
                        Log.InfoFormat("Processing file {0} of {1} - {2}", totalFileCount, fileInfos.Length,
                            fileInfo.Name);

                        var fileLineCounter = 0;
                        string line;

                        // Read the file and display it line by line.
                        var file = new StreamReader(fileInfo.FullName);
                        while ((line = file.ReadLine()) != null)
                        {
                            fileLineCounter++;
                            totalFileLineCount++;
                            //int messageSendTime = GetMessageSendTime(line);

                            if (_job.Contains("requestQ"))
                            {
                                var logEvent = GetMessageSendTime(line);
                                if (logEvent != null)
                                {
                                    totalMessageSendTimes++;
                                    Log.DebugFormat(
                                        "messageSendTime: {0}, fileLineCounter: {1}, totalFileCount: {2}, totalMessageSendTimes: {3}, totalFileCount: {4}, totalFileLineCount: {5}",
                                        logEvent.SendTime, fileLineCounter, totalFileCount, totalMessageSendTimes,
                                        totalFileCount, totalFileLineCount);
                                    WriteToDbRabbitMqSendIssue(logEvent);
                                }
                            }

                            if (_job.Contains("start-stop"))
                            {
                                try
                                {
                                    var logEvent = GetRequestLoggerModule(line);
                                    if (logEvent != null)
                                    {
                                        if (logEvent.Start)
                                        {
                                            if (endOnlyEvents.ContainsKey(logEvent.RequestId))
                                            {
                                                endOnlyEvents.Remove(logEvent.RequestId);
                                            }
                                            else if (!startOnlyEvents.ContainsKey(logEvent.RequestId))
                                            {
                                                startOnlyEvents.Add(logEvent.RequestId, logEvent);
                                            }
                                            else
                                            {
                                                Log.WarnFormat("startOnlyEvents already contains request id: {0}",
                                                    logEvent.RequestId);
                                            }
                                        }
                                        else
                                        {
                                            if (startOnlyEvents.ContainsKey(logEvent.RequestId))
                                            {
                                                startOnlyEvents.Remove(logEvent.RequestId);
                                            }
                                            else if (logEvent.RequestId == "(null)")
                                            {
                                                nullRequestIdEvents.Add(logEvent);
                                            }
                                            else
                                            {
                                                endOnlyEvents.Add(logEvent.RequestId, logEvent);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.ErrorFormat("ERROR --> {0}", line);
                                    Log.Error(ex.Message, ex);
                                    throw;
                                }
                            }
                        }
                    }
                }

                Log.InfoFormat("----------------------------------------");
                Log.InfoFormat("Start Only Count: {0}", startOnlyEvents.Count);
                foreach (var requestId in startOnlyEvents.Keys)
                {
                    var logEvent = startOnlyEvents[requestId];
                    Log.InfoFormat("Start Only - Request Id: {0}, URL: {1}", requestId, logEvent.Url);
                    WriteToDbRequestStartEnd(logEvent);
                }

                Log.InfoFormat("End Only Count: {0}", endOnlyEvents.Count);
                foreach (var requestId in endOnlyEvents.Keys)
                {
                    var logEvent = endOnlyEvents[requestId];
                    Log.InfoFormat("End Only - Request Id: {0}, URL: {1} - [{2}] {3}", requestId, logEvent.Url,
                        logEvent.Level, logEvent.ExceptionHash);
                    WriteToDbRequestStartEnd(logEvent);
                }

                Log.InfoFormat("NULL Request Id Count: {0}", nullRequestIdEvents.Count);
                foreach (var logEvent in nullRequestIdEvents)
                {
                    Log.InfoFormat("End Only - Request Id: (null), URL: {0} - [{1}] {2}", logEvent.Url, logEvent.Level,
                        logEvent.ExceptionHash);
                    WriteToDbRequestStartEnd(logEvent);
                }

                summaryStep.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                summaryStep.Results = $"EXCEPTION: {ex.Message}\r\n\t{summaryStep.Results}";
                summaryStep.CompletedSuccessfully = false;
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                TaskResult.Information = taskInfo.ToString();
                summaryStep.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }


        private LogEvent GetMessageSendTime(string line)
        {
            var x = line.IndexOf("Message sent to Q.Prod.RequestData in", StringComparison.Ordinal);
            if (x > -1)
            {
                var y = line.IndexOf(" ms - {", x, StringComparison.Ordinal);
                var timeField = line.Substring(x + 37, y - x - 37);
                //return int.Parse(timeField);
                var sendTime = int.Parse(timeField);
                var eventLog = new LogEvent(line.Substring(0, x), sendTime, _server);
                return eventLog;
            }

            return null;
        }

        private LogEvent GetRequestLoggerModule(string line)
        {
            var logLevel = GetLogLevel(line);
            if (string.IsNullOrEmpty(logLevel))
            {
                return null;
            }

            if (logLevel.Equals("INFO"))
            {
                // 2017-10-06 23:13:37,464 [110] INFO [btl4uikgne53euiedmrp5w4x/4bec8ceb-cf75-44bc-a6d2-53c5881436a7] R2V2.Web.Infrastructure.HttpModules.RequestLoggerModule -  >>>>>> /Error/NotFound?aspxerrorpath=/wp-login.php, IP: 119.93.157.44, GET, [Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1]
                var x = line.IndexOf("RequestLoggerModule -  >>>>>>", StringComparison.Ordinal);
                if (x > -1)
                {
                    var y = line.IndexOf(", IP:", x, StringComparison.Ordinal);
                    var page = line.Substring(x + 29, y - x - 29);
                    var eventLog = new LogEvent(line.Substring(0, x), page, _server, true);
                    return eventLog;
                }

                return null;
            }

            if (logLevel.Equals("PT-OK") || logLevel.Equals("PT-WARN"))
            {
                // 2017-10-06 23:13:37,464 [110] PT-OK [btl4uikgne53euiedmrp5w4x/4bec8ceb-cf75-44bc-a6d2-53c5881436a7] R2V2.Web.Infrastructure.HttpModules.RequestLoggerModule -  <<<<<< 0.094, /Error/NotFound?aspxerrorpath=/wp-login.php, IP: 119.93.157.44, IsAuthenticated: False, StatusCode: 404
                // 2017-10-15 01:04:08,880 [99] PT-OK [0s5gasovhgo3j3y4gzu44pbf/842c3074-a174-4608-be59-c7c3caa99a60] R2V2.Web.Infrastructure.HttpModules.RequestLoggerModule -  <<<<<< 0.016, /Resource/Title/9780080457291, IP: 145.36.141.14, IsAuthenticated: False, StatusCode: 302
                // 2017-10-15 00:08:19,702 [84] PT-WARN [axrq5wwsukr0v4gc4yfy2ymu/533cfb98-8d1a-46c5-9c22-9adffb78deb6] R2V2.Web.Infrastructure.HttpModules.RequestLoggerModule - wZFNp7PjkYvXXeczLhvjQSGBKtM= <<<<<< 6.562, /ExternalSearch/Counts?q=water+AND+jones&_=1508040492724, IP: 108.168.255.203, IsAuthenticated: False, StatusCode: 200
                var x = line.IndexOf(" <<<<<< ", StringComparison.Ordinal);
                if (x > -1)
                {
                    var z = line.IndexOf(", ", x, StringComparison.Ordinal);
                    var y = line.IndexOf(", IP:", z, StringComparison.Ordinal);
                    var page = line.Substring(z + 2, y - z - 2);
                    var eventLog = new LogEvent(line.Substring(0, x), page, _server, false);
                    return eventLog;
                }
            }

            if (logLevel.Equals("PT-ALERT"))
            {
                //2017-10-15 01:04:08,239 [39] PT-ALERT [bg55slawxnvwrj5s55ykxjfs/8e53c470-110a-4220-81fe-1aaa9a784be9] R2V2.Web.Infrastructure.HttpModules.RequestLoggerModule - uRMIEHUiiJTjY2R7RhfeYkrklvo= The requested page took more than 10 seconds to render.
                //Page: /Browse
                //Page execution time: 93.609
                //Request start time: 01:02:34.631

                //This message does not indicate there was an error with the site. It indicates only that requested page took an unusually long time to be rendered. Please monitor to make sure the site is performing as expected.
                //<<<<<< 93.609, /Browse, IP: 192.231.40.19, IsAuthenticated: True, StatusCode: 200, User = [Id:0, UserName:], Institution = [Id:2013, AcctNum: 000472, Name: COLLIN COUNTY COMMUNITY COLLEGE], AuthenticationType: IP
                var x = line.IndexOf(".RequestLoggerModule - ", StringComparison.Ordinal);
                if (x > -1)
                {
                    var z = line.IndexOf("= The requested page took more than 10 seconds to render.", x,
                        StringComparison.Ordinal);
                    var exceptionHash = line.Substring(x + 23, z - x - 22);
                    var eventLog = new LogEvent(line.Substring(0, x), exceptionHash, _server);
                    return eventLog;
                }
            }

            return null;
        }

        private string GetLogLevel(string line)
        {
            var x = line.IndexOf("] ", StringComparison.Ordinal);
            if (x == -1 || x > 40)
            {
                return null;
            }

            var y = line.IndexOf(" [", x, StringComparison.Ordinal);
            if (y == -1 || y > x + 20)
            {
                return null;
            }

            return line.Substring(x + 2, y - x - 2).Trim();
        }


        private int WriteToDbRabbitMqSendIssue(LogEvent logEvent)
        {
            ISqlCommandParameter[] sqlParameters =
            {
                new DateTimeParameter("Timestamp", logEvent.Timestamp),
                new Int32Parameter("SendTime", logEvent.SendTime),
                new StringParameter("SessionId", logEvent.SessionId),
                new StringParameter("RequestId", logEvent.RequestId),
                new StringParameter("Loglevel", logEvent.Level),
                new StringParameter("Server", logEvent.Server)
            };
            return ExecuteInsertStatementReturnRowCount(InsertStatement_RabbitMqSendIssue, sqlParameters, true);
        }

        private int WriteToDbRequestStartEnd(LogEvent logEvent)
        {
            ISqlCommandParameter[] sqlParameters =
            {
                new DateTimeParameter("Timestamp", logEvent.Timestamp),
                new StringParameter("Url", logEvent.Url),
                new BooleanParameter("Start", logEvent.Start),
                new StringParameter("ExceptionHash", logEvent.ExceptionHash),
                new StringParameter("SessionId", logEvent.SessionId),
                new StringParameter("RequestId", logEvent.RequestId),
                new StringParameter("Loglevel", logEvent.Level),
                new StringParameter("Server", logEvent.Server)
            };
            return ExecuteInsertStatementReturnRowCount(InsertStatement_RequestStartEnd, sqlParameters, true);
        }
    }

    public class LogEvent : FactoryBase
    {
        public LogEvent(string data, int sendTime, string server)
        {
            SendTime = sendTime;
            Server = server;

            var parts = data.Split(' ');

            var dateParts = parts[0].Split('-');
            var timeParts = parts[1].Split(':');
            var secondParts = timeParts[2].Split(',');

            Timestamp = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]),
                int.Parse(timeParts[0]), int.Parse(timeParts[1]),
                int.Parse(secondParts[0]), int.Parse(secondParts[1]));

            Level = parts[3];

            var pair = parts[4].Replace("[", "").Replace("]", "").Split('/');
            if (pair.Length == 1)
            {
                RequestId = pair[0];
            }
            else
            {
                SessionId = pair[0];
                RequestId = pair[1];
            }
        }

        public LogEvent(string data, string url, string server, bool start)
        {
            Url = url;
            Server = server;
            Start = start;

            var parts = data.Split(' ');

            var dateParts = parts[0].Split('-');
            var timeParts = parts[1].Split(':');
            var secondParts = timeParts[2].Split(',');

            Timestamp = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]),
                int.Parse(timeParts[0]), int.Parse(timeParts[1]),
                int.Parse(secondParts[0]), int.Parse(secondParts[1]));

            Level = parts[3];

            var pair = parts[4].Replace("[", "").Replace("]", "").Split('/');
            if (pair.Length == 1)
            {
                RequestId = pair[0];
            }
            else
            {
                SessionId = pair[0];
                RequestId = pair[1];
            }
        }

        public LogEvent(string data, string exceptionHash, string server)
        {
            Server = server;
            ExceptionHash = exceptionHash;

            var parts = data.Split(' ');

            var dateParts = parts[0].Split('-');
            var timeParts = parts[1].Split(':');
            var secondParts = timeParts[2].Split(',');

            Timestamp = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]),
                int.Parse(timeParts[0]), int.Parse(timeParts[1]),
                int.Parse(secondParts[0]), int.Parse(secondParts[1]));

            Level = parts[3];

            var pair = parts[4].Replace("[", "").Replace("]", "").Split('/');
            if (pair.Length == 1)
            {
                RequestId = pair[0];
            }
            else
            {
                SessionId = pair[0];
                RequestId = pair[1];
            }
        }

        public DateTime Timestamp { get; set; }
        public string SessionId { get; set; }
        public string RequestId { get; set; }
        public string Level { get; set; }
        public string Server { get; set; }
        public int SendTime { get; set; }
        public string Url { get; set; }
        public bool Start { get; set; }
        public string ExceptionHash { get; set; }
    }
}