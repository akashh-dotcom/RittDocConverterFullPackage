#region

using System;
using System.IO;
using System.Threading;
using EasyNetQ;
using EasyNetQ.Topology;
using Newtonsoft.Json;
using R2V2.Core.RequestLogger;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.RequestLogger
{
    public class RequestLoggerThread : ThreadBase, IR2V2Thread
    {
        private readonly ILog<RequestLoggerThread> _log;

        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly RequestLoggerDataService _requestLoggerDataService;
        private readonly RequestLoggerService _requestLoggerService;
        private readonly DateTime _threadStartTime = DateTime.Now;
        private readonly IWindowsServiceSettings _windowsServiceSettings;
        private IBus _bus;

        private string _failureLogFile;
        private IQueue _queue;

        private long _requestCount;

        /// <summary>
        ///     -debug -service=r2v2.requestloggingservice
        /// </summary>
        public RequestLoggerThread(ILog<RequestLoggerThread> log
            , IMessageQueueSettings messageQueueSettings
            , RequestLoggerDataService requestLoggerDataService
            , RequestLoggerService requestLoggerService
            , IWindowsServiceSettings windowsServiceSettings
        )
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _requestLoggerDataService = requestLoggerDataService;
            _requestLoggerService = requestLoggerService;
            _windowsServiceSettings = windowsServiceSettings;
            StopProcessing = false;
            _log.Debug("RequestLoggerThread initialized");
        }

        public void Start()
        {
            _log.Info("RequestLoggerThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "requestLogger" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("RequestLoggerThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("RequestLoggerThread is now stopping...");
            StopProcessing = true;
            _log.Info("RequestLoggerThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                RequestLoggerProcessor();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                Thread.Sleep(1000);
                throw;
            }
            finally
            {
                _log.Info("StartProcessQueue() <<<");
            }
        }

        private void RequestLoggerProcessor()
        {
            _log.Debug("RequestLoggerProcessor >>");
            _log.DebugFormat("MessageQueueSettings.EnvironmentConnectionString: {0}",
                _messageQueueSettings.EnvironmentConnectionString);
            _log.DebugFormat("MessageQueueSettings.RequestLoggingQueueName: {0}",
                _messageQueueSettings.RequestLoggingQueueName);

            using (_bus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString))
            {
                _queue = _bus.Advanced.QueueDeclare(_messageQueueSettings.RequestLoggingQueueName);

                _bus.Advanced.Consume(_queue,
                    x => x.Add<RequestData>((message, info) => ProcessRequestDataMessage(message.Body)));

                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        private void ProcessRequestDataMessage(RequestData requestData)
        {
            _requestCount++;
            _log.DebugFormat(">>>>>>>>>> Starting to process request id: {0}", requestData.RequestId);
            _log.DebugFormat("request {0} since {1:G}, {2}", _requestCount, _threadStartTime,
                requestData.ToDebugString());
            if (_requestLoggerDataService.SaveRequest(requestData))
            {
                ClearExceptionCounters();
                return;
            }

            if (requestData.FailedSaveAttempts <= 10)
            {
                _log.WarnFormat("Error writing request data to database: {0}", requestData.ToDebugString());
                _requestLoggerService.WriteRequestDataToMessageQueue(requestData);
                SleepThreadAfterException();
            }
            else
            {
                WriteFailedMessageToFile(requestData);
                WriteRequestDataAsInsertToFile(requestData);
            }

            _log.InfoFormat("Message Count: {0}", _bus.Advanced.MessageCount(_queue));
            _log.DebugFormat("<<<<<<<<<< Finished processing request id: {0}", requestData.RequestId);
        }

        private void WriteFailedMessageToFile(RequestData requestData)
        {
            try
            {
                _log.InfoFormat("Writing RequestData JSON to file, request id: {0}", requestData.RequestId);

                DirectoryHelper.VerifyDirectory(_windowsServiceSettings.MessageFailureDirectory);
                var requestDataJsonPath =
                    Path.Combine(_windowsServiceSettings.MessageFailureDirectory, "RequestDataJson");
                DirectoryHelper.VerifyDirectory(requestDataJsonPath);

                var file = Path.Combine(requestDataJsonPath, $"{requestData.RequestId}.json");
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                File.WriteAllText(file, json);
                _log.InfoFormat("RequestData JSON written to file: {0}", file);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.Message, ex);

                // SJS - swallow exception so the process just continues
                throw;
            }
        }

        private void WriteRequestDataAsInsertToFile(RequestData requestData)
        {
            try
            {
                if (_failureLogFile == null)
                {
                    var path = Path.Combine(_windowsServiceSettings.MessageFailureDirectory, "RequestDataInserts");
                    DirectoryHelper.VerifyDirectory(path);
                    _failureLogFile = Path.Combine(path, $"RequestDataInserts_{DateTime.Now:yyyyMMdd-HHmmss}.sql");
                }

                _log.InfoFormat("Writing RequestData SQL to file: {0}", _failureLogFile);

                var url = requestData.Url.Length < 1000
                    ? requestData.Url
                    : $"{requestData.Url.Substring(0, 1000)} ... TRUNCATED!";

                var sql = _requestLoggerDataService.GetSql(requestData);
                sql = sql.Replace("@InstitutionId", requestData.InstitutionId.ToString())
                    .Replace("@UserId", requestData.UserId.ToString())
                    .Replace("@IpAddressOctetA", requestData.IpAddress.OctetA.ToString())
                    .Replace("@IpAddressOctetB", requestData.IpAddress.OctetB.ToString())
                    .Replace("@IpAddressOctetC", requestData.IpAddress.OctetC.ToString())
                    .Replace("@IpAddressOctetD", requestData.IpAddress.OctetD.ToString())
                    .Replace("@IpAddressInteger", requestData.IpAddress.IntegerValue.ToString())
                    .Replace("@PageViewTimestamp",
                        BuildDbValue(requestData.RequestTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff")))
                    .Replace("@PageViewRunTime", requestData.RequestDuration.ToString())
                    .Replace("@SessionId",
                        BuildDbValue(requestData.Session == null
                            ? "session-id-missing"
                            : requestData.Session.SessionId))
                    .Replace("@Url", BuildDbValue(url))
                    .Replace("@RequestId", BuildDbValue(requestData.RequestId))
                    .Replace("@Referrer",
                        BuildDbValue(RequestLoggerDataService.TruncateField(requestData.Referrer, 1024)))
                    .Replace("@CountryCode",
                        BuildDbValue(RequestLoggerDataService.TruncateField(requestData.CountryCode, 10)))
                    .Replace("@ServerNumber", requestData.ServerNumber.ToString());

                if (requestData.SearchRequest != null)
                {
                    sql = sql.Replace("@SearchTypeId", requestData.SearchRequest.SearchTypeId.ToString())
                        .Replace("@IsArchive", requestData.SearchRequest.IsArchivedSearch ? "1" : "0")
                        .Replace("@IsExternal", requestData.SearchRequest.IsExternalSearch ? "1" : "0")
                        .Replace("@SearchTimestamp", requestData.RequestTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                }

                if (requestData.ContentView != null)
                {
                    sql = sql.Replace("@ResourceId", requestData.ContentView.ResourceId.ToString())
                        .Replace("@ChapterSectionId",
                            BuildDbValue(
                                RequestLoggerDataService.TruncateField(requestData.ContentView.ChapterSectionId, 50)))
                        .Replace("@TurnawayTypeId", requestData.ContentView.ContentTurnawayTypeId.ToString())
                        .Replace("@ContentViewTimestamp",
                            requestData.RequestTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff"))
                        .Replace("@ActionTypeId", requestData.ContentView.ContentActionTypeId.ToString())
                        .Replace("@FoundFromSearch", requestData.ContentView.FoundFromSearch ? "1" : "0")
                        .Replace("@SearchTerm",
                            BuildDbValue(
                                RequestLoggerDataService.TruncateField(requestData.ContentView.SearchTerm, 500)))
                        .Replace("@LicenseType", requestData.ContentView.LicenseTypeId.ToString())
                        .Replace("@ResourceStatusId", requestData.ContentView.ResourceStatusId.ToString());
                }

                if (requestData.MediaView != null)
                {
                    if (requestData.ContentView == null)
                    {
                        sql = sql.Replace("@ResourceId", requestData.MediaView.ResourceId.ToString())
                            .Replace("@ChapterSectionId",
                                BuildDbValue(
                                    RequestLoggerDataService.TruncateField(requestData.MediaView.ChapterSectionId,
                                        50)));
                    }

                    sql = sql.Replace("@MediaViewTimestamp",
                            requestData.RequestTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff"))
                        .Replace("@MediaFileName",
                            BuildDbValue(
                                RequestLoggerDataService.TruncateField(requestData.MediaView.MediaFileName, 255)));
                }

                File.AppendAllText(_failureLogFile, sql);
                _log.InfoFormat("RequestData SQL written to file, SQL: {0}", sql);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.Message, ex);

                // SJS - swallow exception so the process just continues
                //throw;
            }
        }

        public string BuildDbValue(string value)
        {
            return value == null ? "null" : $"'{value.Replace("'", "''")}'";
        }
    }
}