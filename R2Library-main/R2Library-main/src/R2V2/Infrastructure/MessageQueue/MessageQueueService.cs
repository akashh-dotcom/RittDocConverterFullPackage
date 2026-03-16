#region

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using R2V2.Core;
using R2V2.Infrastructure.GoogleAnalytics;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.MessageQueue
{
    public class MessageQueueService : IMessageQueueService
    {
        private readonly ILog<MessageQueueService> _log;
        private readonly IMessageQueueSettings _settings;
        private readonly bool _isLocalDevelopment;

        public MessageQueueService(ILog<MessageQueueService> log, IMessageQueueSettings settings)
        {
            _log = log;
            _settings = settings;
            
            // Check if we're in local development mode
            var isLocalDevConfig = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            _isLocalDevelopment = !string.IsNullOrEmpty(isLocalDevConfig) && 
                                  isLocalDevConfig.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public bool WriteMessageToQueue(string messageQueuePath, object message)
        {
            // Skip message queue entirely in local development
            if (_isLocalDevelopment)
            {
                _log.Debug($"Message queue disabled in local dev - skipping queue '{messageQueuePath}'");
                return true;
            }
            
            try
            {
                var messageBody = message.ToString();
                var debugInfo = message as IDebugInfo;
                _log.Debug(debugInfo != null ? debugInfo.ToDebugString() : messageBody);
                using (var queue = GetMessageQueue(messageQueuePath))
                {
                    using (var tx = new MessageQueueTransaction())
                    {
                        tx.Begin();
                        queue.DefaultPropertiesToSend.Recoverable = true;
                        queue.Send(message, tx);
                        tx.Commit();
                        _log.DebugFormat("Message written to queue '{0}'", messageQueuePath);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                _log.WarnFormat("messageQueuePath: {0}", messageQueuePath);
                _log.WarnFormat("message: {0}", message);
                return false;
            }
        }

        public System.Messaging.MessageQueue GetMessageQueue(string messageQueuePath)
        {
            if (System.Messaging.MessageQueue.Exists(messageQueuePath))
            {
                return new System.Messaging.MessageQueue(messageQueuePath);
            }

            var queue = System.Messaging.MessageQueue.Create(messageQueuePath, true);
            queue.SetPermissions("Administrators",
                MessageQueueAccessRights
                    .FullControl); // always make sure to give administrators full control over the queues
            return queue;
        }

        public void WriteMessageToDisk(IR2V2Message message, string queue)
        {
            // In local dev mode, just log and return
            if (_isLocalDevelopment)
            {
                _log.Debug($"Message queue disabled in local dev - would write to disk: {queue}/{message.MessageId}.json");
                return;
            }
            
            try
            {
                _log.Info($"_settings.SendErrorDirectoryPath: {_settings.SendErrorDirectoryPath}");
                _log.Info($"queue: {queue}");
                var path = $"{_settings.SendErrorDirectoryPath}\\{queue}";
                var filePath = $"{path}\\{$"{message.MessageId}.json"}";
                if (!Directory.Exists(path))
                {
                    _log.Info($"Directory.CreateDirectory(path): {path}");
                    Directory.CreateDirectory(path);
                }

                _log.Info($"File.WriteAllText(filePath, message.ToJsonString()): {filePath}");
                File.WriteAllText(filePath, message.ToJsonString());
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to write message to disk: {ex.Message}", ex);
            }
        }

        public void WriteMessageToDisk(GoogleRequestData message, string queue)
        {
            // In local dev mode, just log and return
            if (_isLocalDevelopment)
            {
                _log.Debug($"Message queue disabled in local dev - would write to disk: {queue}/{message.MessageId}.json");
                return;
            }
            
            try
            {
                var path = Path.Combine(_settings.SendErrorDirectoryPath, queue);
                var filePath = Path.Combine(path, $"{message.MessageId}.json");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllText(filePath, message.ToJsonString());
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to write GoogleRequestData to disk: {ex.Message}", ex);
            }
        }

        public string GetFailedQueueName(string queueName)
        {
            var parts = queueName.Split('.').ToList();
            parts.Insert(parts.Count - 1, "Failed");
            return string.Join(".", parts.ToArray());
        }
    }
}