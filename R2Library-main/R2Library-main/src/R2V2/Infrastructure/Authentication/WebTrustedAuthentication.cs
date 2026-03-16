namespace R2V2.Infrastructure.Authentication
{
    public class WebTrustedAuthentication
    {
        public string Timestamp { get; set; }
        public string Hash { get; set; }
        public string ErrorMessage { get; set; }
    }
}