#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.SuperType;
using AccountStatusEnum = R2V2.Core.Institution.AccountStatus;
using AccessTypeEnum = R2V2.Core.Institution.AccessType;
using HomePageEnum = R2V2.Core.Institution.HomePage;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class Institution : AuditableEntity, IInstitution, IDebugInfo
    {
        private readonly IList<InstitutionBranding> _institutionBrandings = new List<InstitutionBranding>();

        private readonly IList<InstitutionResourceLicense> _institutionResourceLicenses =
            new List<InstitutionResourceLicense>();

        private readonly IList<ProductSubscription> _productSubscriptions = new List<ProductSubscription>();

        private readonly IList<ReserveShelf.ReserveShelf> _reserveShelves = new List<ReserveShelf.ReserveShelf>();
        public virtual string AthensOrgId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Institution = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", AccountNumber: {0}", AccountNumber);
            sb.AppendFormat(", Name: {0}", Name);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual string Name { get; set; }
        public virtual string NameKey { get; set; } //KSH -3/22/2012 - Created for Paging on the Institution List
        public virtual string AccountNumber { get; set; }
        public virtual Address Address { get; set; }
        public virtual string Phone { get; set; }
        public virtual Authentication.Trial Trial { get; set; }
        public virtual bool DisplayAllProducts { get; set; }

        public virtual IEnumerable<ProductSubscription> ProductSubscriptions => _productSubscriptions;

        public virtual void AddProductSubscription(ProductSubscription productSubscription)
        {
            _productSubscriptions.Add(productSubscription);
        }

        public virtual int AccountStatusId { get; set; }

        public virtual IInstitutionAccountStatus AccountStatus
        {
            get
            {
                var accountStatus = (AccountStatusEnum)AccountStatusId;
                if (accountStatus == AccountStatusEnum.Active)
                {
                    return InstitutionAccountStatus.Active;
                }

                if (accountStatus == AccountStatusEnum.Trial)
                {
                    var now = DateTime.Now;
                    if (now >= Trial.StartDate && now <= Trial.EndDate)
                    {
                        return InstitutionAccountStatus.Trial;
                    }

                    return InstitutionAccountStatus.TrialExpired;
                }

                return InstitutionAccountStatus.Disabled;
            }
        }

        public virtual int AccessTypeId { get; set; }

        public virtual IInstitutionAccessType AccessType
        {
            get
            {
                var accessType = (AccessTypeEnum)AccessTypeId;
                switch (accessType)
                {
                    case AccessTypeEnum.IpValidationAnon:
                        return InstitutionAccessType.IpValidationAnon;
                    case AccessTypeEnum.IpValidationOpt:
                        return InstitutionAccessType.IpValidationOpt;
                    case AccessTypeEnum.IpValidationReq:
                        return InstitutionAccessType.IpValidationReq;
                    default:
                        return InstitutionAccessType.IpIndependent;
                }
            }
        }

        public virtual InstitutionBranding InstitutionBranding => _institutionBrandings.FirstOrDefault();
        public virtual IEnumerable<InstitutionBranding> InstitutionBrandings => _institutionBrandings;

        public virtual bool EULASigned { get; set; }
        public virtual bool PdaEulaSigned { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual AnnualFee AnnualFee { get; set; }
        public virtual bool HouseAccount { get; set; }
        public virtual string AthensAffiliation { get; set; }
        public virtual string LogUrl { get; set; }

        public virtual int HomePageId { get; set; }

        public virtual IInstitutionHomePage HomePage
        {
            get
            {
                var homePage = (HomePageEnum)HomePageId;
                switch (homePage)
                {
                    case HomePageEnum.Discipline:
                        return InstitutionHomePage.Discipline;
                    case HomePageEnum.Titles:
                        return InstitutionHomePage.Titles;
                    case HomePageEnum.AtoZIndex:
                        return InstitutionHomePage.AtoZIndex;
                    default:
                        return InstitutionHomePage.Titles;
                }
            }
        }

        public virtual string TrustedKey { get; set; }
        public virtual bool RecordStatus { get; set; }
        public virtual bool ExpertReviewerUserEnabled { get; set; }
        public virtual bool IncludeArchivedTitlesByDefault { get; set; }

        public virtual Territory.Territory Territory { get; set; }

        public virtual string ProxyPrefix { get; set; }

        public virtual string UrlSuffix => EnableIpPlus ? $"?accountNumber={AccountNumber}" : "";

        public virtual InstitutionType Type { get; set; }
        public virtual bool EnableIpPlus { get; set; }
        public virtual bool EnableHomePageCollectionLink { get; set; }

        public virtual string OclcSymbol { get; set; }

        public virtual IEnumerable<InstitutionResourceLicense> InstitutionResourceLicenses =>
            _institutionResourceLicenses;

        public virtual IEnumerable<ReserveShelf.ReserveShelf> ReserveShelves => _reserveShelves;

        public virtual void AddInstitutionResourceLicense(InstitutionResourceLicense institutionResourceLicense)
        {
            _institutionResourceLicenses.Add(institutionResourceLicense);
        }
    }
}