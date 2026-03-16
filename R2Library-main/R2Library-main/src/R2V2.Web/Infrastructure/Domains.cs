#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Infrastructure
{
    public class Domains
    {
        public bool EnableDomainVerification { get; set; }
        public List<DomainPair> DomainPairs { get; set; } = new List<DomainPair>();
    }

    public class DomainPair
    {
        public string IncomingDomain { get; set; }
        public string PrimaryDomain { get; set; }
    }
}