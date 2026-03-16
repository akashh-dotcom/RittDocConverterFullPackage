#region

using System.Collections.Generic;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    public interface IInstitution : ISoftDeletable
    {
        int Id { get; set; }
        string Name { get; set; }
        string NameKey { get; set; }
        string AccountNumber { get; set; }

        bool DisplayAllProducts { get; set; }
        bool EULASigned { get; set; }
        bool PdaEulaSigned { get; set; }
        decimal Discount { get; set; }

        bool HouseAccount { get; set; }
        string LogUrl { get; set; }
        int HomePageId { get; set; }
        IInstitutionHomePage HomePage { get; }
        string TrustedKey { get; set; }

        Address Address { get; set; }
        string Phone { get; set; }
        Authentication.Trial Trial { get; set; }
        AnnualFee AnnualFee { get; set; }

        int AccessTypeId { get; set; }
        IInstitutionAccessType AccessType { get; }

        int AccountStatusId { get; set; }
        IInstitutionAccountStatus AccountStatus { get; }

        InstitutionBranding InstitutionBranding { get; }
        IEnumerable<InstitutionBranding> InstitutionBrandings { get; }

        IEnumerable<InstitutionResourceLicense> InstitutionResourceLicenses { get; }

        IEnumerable<ReserveShelf.ReserveShelf> ReserveShelves { get; }

        IEnumerable<ProductSubscription> ProductSubscriptions { get; }

        //InstitutionTerritory InstitutionTerritory { get; set; }
        Territory.Territory Territory { get; set; }

        bool ExpertReviewerUserEnabled { get; set; }
        bool IncludeArchivedTitlesByDefault { get; set; }
        bool EnableHomePageCollectionLink { get; set; }

        string ProxyPrefix { get; set; }
        string UrlSuffix { get; }
        string AthensAffiliation { get; set; }
        string OclcSymbol { get; set; }
        InstitutionType Type { get; set; }

        bool EnableIpPlus { get; set; }
        void AddInstitutionResourceLicense(InstitutionResourceLicense institutionResourceLicense);

        void AddProductSubscription(ProductSubscription productSubscription);
    }
}