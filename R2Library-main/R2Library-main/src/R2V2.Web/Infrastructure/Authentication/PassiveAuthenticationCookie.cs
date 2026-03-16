#region

using System;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public enum PassiveAuthenticationType
    {
        IpAuthentication,
        ReferrerAuthentication,
        TrustedAuthentication
    }

    public class PassiveAuthenticationCookieWrapper
    {
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public string Value { get; set; }
    }

    public class PassiveAuthenticationCookie
    {
        public PassiveAuthenticationType PassiveAuthenticationType { get; set; }
        public DateTime Timestamp { get; set; }
        public int InstitutionId { get; set; }
        public string IpAddress { get; set; }
        public string Referrer { get; set; }
        public string AccountNumber { get; set; }
        public string TrushedAuthHash { get; set; }
        public string TrushedAuthTimestamp { get; set; }
    }
}