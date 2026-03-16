namespace R2V2.Infrastructure.Settings
{
    public class EmailSettings : AutoSettings, IEmailSettings
    {
        public string DefaultFromAddress { get; set; }
        public string DefaultFromName { get; set; }
        public string DefaultReplyToAddress { get; set; }
        public string DefaultReplyToName { get; set; }
        public bool BccAllMessages { get; set; }
        public string BccEmailAddresses { get; set; }
        public bool SendToCustomers { get; set; }
        public string TestEmailAddresses { get; set; }
        public bool AddEnvironmentPrefixToSubject { get; set; }
        public string TemplatesDirectory { get; set; }
        public string PdaAddToCartCcEmailAddresses { get; set; }
        public string OutputPath { get; set; }
        public string WebSiteBaseUrl { get; set; }

        public string[] WhiteListedEmails { get; set; }
    }
}