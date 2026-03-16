#region

using System.Collections.Generic;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.Admin
{
    public interface IAdminInstitution
    {
        int Id { get; }
        string Name { get; }
        string AccountNumber { get; }
        Address Address { get; }
        string Phone { get; }
        bool IsEulaSigned { get; set; }
        bool IsPdaEulaSigned { get; set; }
        decimal Discount { get; }
        AnnualFee AnnualFee { get; }
        bool HouseAccount { get; }
        string AthensAffiliation { get; }
        IInstitutionAccountStatus AccountStatus { get; }
        bool ExpertReviewerUserEnabled { get; }

        IEnumerable<License> Licenses { get; }

        string ProxyPrefix { get; set; }
        string UrlSuffix { get; set; }

        License GetLicense(int resourceId);

        void SetAccoutStatusForDebugging(IInstitutionAccountStatus accountStatus);

        string ToDebugString();
        void ClearLicenses();
    }
}