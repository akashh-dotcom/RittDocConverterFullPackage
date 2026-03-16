namespace R2V2.Infrastructure.Settings
{
    public interface IEmailSettings
    {
        string DefaultFromAddress { get; set; }
        string DefaultFromName { get; set; }
        string DefaultReplyToAddress { get; set; }
        string DefaultReplyToName { get; set; }
        bool BccAllMessages { get; set; }
        string BccEmailAddresses { get; set; }
        bool SendToCustomers { get; set; }
        string TestEmailAddresses { get; set; }
        bool AddEnvironmentPrefixToSubject { get; set; }
        string TemplatesDirectory { get; set; }
        string PdaAddToCartCcEmailAddresses { get; set; }
        string OutputPath { get; set; }
        string WebSiteBaseUrl { get; set; }

        string[] WhiteListedEmails { get; set; }
    }
}