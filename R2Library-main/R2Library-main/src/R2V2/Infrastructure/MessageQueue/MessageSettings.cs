namespace R2V2.Infrastructure.MessageQueue
{
    public class MessageSettings
    {
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public string RouteKey { get; set; }
    }
}