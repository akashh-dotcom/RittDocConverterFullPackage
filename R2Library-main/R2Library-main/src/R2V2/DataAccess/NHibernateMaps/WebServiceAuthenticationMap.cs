#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class WebServiceAuthenticationMap : BaseMap<WebServiceAuthentication>
    {
        public WebServiceAuthenticationMap()
        {
            Table("dbo.tInstitutionTrustedAuth");
            Id(x => x.Id, "iInstitutionTrustedAuthId").GeneratedBy.Identity();
            References(x => x.Institution).Column("iInstitutionId").ReadOnly();
            Map(x => x.InstitutionId, "iInstitutionId");

            Map(x => x.OctetA, "tiOctetA");
            Map(x => x.OctetB, "tiOctetB");
            Map(x => x.OctetC, "tiOctetC");
            Map(x => x.OctetD, "tiOctetD");
            Map(x => x.IpNumber, "iDecimal");
            Map(x => x.AuthenticationKey, "vchAuthenticationKey");
        }
    }
}