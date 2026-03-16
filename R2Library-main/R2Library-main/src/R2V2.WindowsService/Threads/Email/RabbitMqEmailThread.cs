#region

using System;
using System.IO;
using System.Threading;
using EasyNetQ;
using EasyNetQ.Topology;
using Newtonsoft.Json;
using R2V2.Core.Email;
using R2V2.Extensions;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.Email
{
    public class RabbitMqEmailThread : ThreadBase, IR2V2Thread
    {
        private readonly EmailMessageSendQueueService _emailMessageSendQueueService;
        private readonly EmailSendService _emailService;
        private readonly ILog<RabbitMqEmailThread> _log;

        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly DateTime _threadStartTime = DateTime.Now;
        private readonly IWindowsServiceSettings _windowsServiceSettings;
        private IBus _bus;

        private long _emailMessageCount;
        private IQueue _queue;

        /// <summary>
        ///     -debug -service=R2V2.RabbitMqEmailService
        /// </summary>
        public RabbitMqEmailThread(ILog<RabbitMqEmailThread> log
            , IMessageQueueSettings messageQueueSettings
            , IWindowsServiceSettings windowsServiceSettings
            , EmailSendService emailService
            , EmailMessageSendQueueService emailMessageSendQueueService
        )
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _windowsServiceSettings = windowsServiceSettings;
            _emailService = emailService;
            _emailMessageSendQueueService = emailMessageSendQueueService;
            StopProcessing = false;
            _log.Debug("RabbitMqEmailThread initialized");
        }

        public void Start()
        {
            _log.Info("RabbitMqEmailThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "rabbitmqemail" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("RabbitMqEmailThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("RabbitMqEmailThread is now stopping...");
            StopProcessing = true;
            _log.Info("RabbitMqEmailThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                EmailMessageProcessor();
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

        private void EmailMessageProcessor()
        {
            _log.Debug("EmailMessageProcessor >>");
            _log.DebugFormat("MessageQueueSettings.EnvironmentConnectionString: {0}",
                _messageQueueSettings.EnvironmentConnectionString);
            _log.DebugFormat("MessageQueueSettings.EmailMessageQueueName: {0}",
                _messageQueueSettings.EmailMessageQueueName);

            using (_bus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString))
            {
                _queue = _bus.Advanced.QueueDeclare(_messageQueueSettings.EmailMessageQueueName);

                _bus.Advanced.Consume(_queue,
                    x => x.Add<EmailMessage>((message, info) => ProcessEmailMessage(message.Body)));

                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        private void ProcessEmailMessage(EmailMessage emailMessage)
        {
            _emailMessageCount++;
            _log.DebugFormat(">>>>>>>>>> Starting to process message id: {0}", emailMessage.MessageId);
            _log.DebugFormat("message # {0} since {1:G}, {2}", _emailMessageCount, _threadStartTime,
                emailMessage.ToDebugString());
            if (_emailService.SendViaSmtp(emailMessage))
            {
                ClearExceptionCounters();
                return;
            }

            if (emailMessage.SendAttempts <= 10)
            {
                _log.WarnFormat("Error sending email message: {0}", emailMessage.ToDebugString());
                _emailMessageSendQueueService.WriteEmailMessageToMessageQueue(emailMessage);
                //_requestLoggerService.WriteRequestDataToMessageQueue(requestData);
                SleepThreadAfterException();
            }
            else
            {
                WriteFailedMessageToFile(emailMessage);
            }

            _log.DebugFormat("<<<<<<<<<< Finished processing message id: {0}", emailMessage.MessageId);
        }


        private void WriteFailedMessageToFile(EmailMessage emailMessage)
        {
            try
            {
                _log.InfoFormat("Writing EmailMessage JSON to file, MessageId: {0}", emailMessage.MessageId);

                DirectoryHelper.VerifyDirectory(_windowsServiceSettings.MessageFailureDirectory);
                var requestDataJsonPath =
                    Path.Combine(_windowsServiceSettings.MessageFailureDirectory, "EmailMessageJson");
                DirectoryHelper.VerifyDirectory(requestDataJsonPath);

                var file = Path.Combine(requestDataJsonPath, $"{emailMessage.MessageId}.json");
                var json = JsonConvert.SerializeObject(emailMessage, Formatting.Indented);
                File.WriteAllText(file, json);
                _log.InfoFormat("EmailMessage JSON written to file: {0}", file);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.Message, ex);

                // SJS - swallow exception so the process just continues
                throw;
            }
        }
    }
}