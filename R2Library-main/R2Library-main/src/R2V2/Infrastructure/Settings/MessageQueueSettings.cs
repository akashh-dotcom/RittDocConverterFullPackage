namespace R2V2.Infrastructure.Settings
{
    public class MessageQueueSettings : AutoSettings, IMessageQueueSettings
    {
        public string EmailMessageQueue { get; set; }
        public string OrderProcessingQueue { get; set; }
        public string EnvironmentConnectionString { get; set; }
        public string ProductionConnectionString { get; set; }
        public string SendErrorDirectoryPath { get; set; }
        public string RequestLoggingRouteKey { get; set; }
        public string RequestLoggingExchangeName { get; set; }
        public string RequestLoggingQueueName { get; set; }
        public string ResourceBatchPromotionRouteKey { get; set; }
        public string ResourceBatchPromotionExchangeName { get; set; }
        public string ResourceBatchPromotionQueueName { get; set; }
        public string OngoingPdaRouteKey { get; set; }
        public string OngoingPdaExchangeName { get; set; }
        public string OngoingPdaQueueName { get; set; }
        public string AnalyticsRouteKey { get; set; }
        public string AnalyticsExchangeName { get; set; }
        public string AnalyticsQueueName { get; set; }
        public string EmailMessageRouteKey { get; set; }
        public string EmailMessageExchangeName { get; set; }
        public string EmailMessageQueueName { get; set; }
        public string AutomatedCartExchangeName { get; set; }
        public string AutomatedCartQueueName { get; set; }
        public string AutomatedCartRouteKey { get; set; }

        public string ConnectionString { get; set; }
        public string PromoteResponseQueueName { get; set; }
        public string PromoteRequestQueueName { get; set; }
    }
}