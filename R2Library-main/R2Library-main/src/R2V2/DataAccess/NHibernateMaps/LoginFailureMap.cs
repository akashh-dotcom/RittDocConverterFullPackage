#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class LoginFailureMap : BaseMap<LoginFailure>
    {
        public LoginFailureMap()
        {
            Table("dbo.tLoginFailure");
            Id(x => x.Id, "iLoginFailureId").GeneratedBy.Identity();
            Map(x => x.InstitutionId, "iInstitutionId");

            Map(x => x.OctetA, "tiOctetA");
            Map(x => x.OctetB, "tiOctetB");
            Map(x => x.OctetC, "tiOctetC");
            Map(x => x.OctetD, "tiOctetD");
            Map(x => x.IpNumericValue, "iIpNumericValue");
            Map(x => x.CountryCode, "vchCountryCode");
            Map(x => x.LoginFailureDate, "dtLoginFailureDate");
            Map(x => x.Username, "vchUsername");
        }
    }
}