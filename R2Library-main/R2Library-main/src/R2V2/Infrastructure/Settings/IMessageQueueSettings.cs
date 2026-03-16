namespace R2V2.Infrastructure.Settings
{
    public interface IMessageQueueSettings
    {
        string EmailMessageQueue { get; set; }
        string OrderProcessingQueue { get; set; }

        /// <summary>
        ///     RabbitMQ connection string for the current environment
        /// </summary>
        string EnvironmentConnectionString { get; set; }

        /// <summary>
        ///     RabbitMQ connection string for the PRODUCTION environment
        ///     This is needed to allow the staging environment to communicate with the production environment when a resource is
        ///     promoted and ongoing PDA needs to be initiated.
        /// </summary>
        string ProductionConnectionString { get; set; }

        string SendErrorDirectoryPath { get; set; }

        string RequestLoggingRouteKey { get; set; }
        string RequestLoggingExchangeName { get; set; }
        string RequestLoggingQueueName { get; set; }

        string ResourceBatchPromotionRouteKey { get; set; }
        string ResourceBatchPromotionExchangeName { get; set; }
        string ResourceBatchPromotionQueueName { get; set; }

        string OngoingPdaRouteKey { get; set; }
        string OngoingPdaExchangeName { get; set; }
        string OngoingPdaQueueName { get; set; }

        string AnalyticsRouteKey { get; set; }
        string AnalyticsExchangeName { get; set; }
        string AnalyticsQueueName { get; set; }

        string EmailMessageRouteKey { get; set; }
        string EmailMessageExchangeName { get; set; }
        string EmailMessageQueueName { get; set; }
        string AutomatedCartExchangeName { get; set; }
        string AutomatedCartQueueName { get; set; }
        string AutomatedCartRouteKey { get; set; }

        string ConnectionString { get; set; }
        string PromoteResponseQueueName { get; set; }
        string PromoteRequestQueueName { get; set; }
    }
}