#region

using R2V2.Infrastructure.GoogleAnalytics;

#endregion

namespace R2V2.Infrastructure.MessageQueue
{
    public interface IMessageQueueService
    {
        bool WriteMessageToQueue(string messageQueuePath, object message);

        System.Messaging.MessageQueue GetMessageQueue(string messageQueuePath);

        void WriteMessageToDisk(IR2V2Message message, string queue);

        void WriteMessageToDisk(GoogleRequestData message, string queue);

        string GetFailedQueueName(string queueName);
    }
}