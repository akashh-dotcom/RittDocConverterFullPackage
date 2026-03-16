#region

using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.Email
{
    public class EmailQueueService
    {
        private readonly MessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;

        public EmailQueueService(MessageQueueService messageQueueService, IMessageQueueSettings messageQueueSettings)
        {
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings;
        }

        public bool QueueEmailMessage(EmailMessage emailMessage)
        {
            return _messageQueueService.WriteMessageToQueue(_messageQueueSettings.EmailMessageQueue, emailMessage);
        }
    }
}