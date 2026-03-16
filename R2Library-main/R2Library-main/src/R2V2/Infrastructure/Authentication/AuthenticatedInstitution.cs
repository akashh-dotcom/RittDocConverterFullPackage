#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Infrastructure.Authentication
{
    [Serializable]
    public class AuthenticatedInstitution : IDebugInfo
    {
        private readonly Dictionary<int, License> _licenses = new Dictionary<int, License>();

        private string _auditId;

        public AuthenticatedInstitution(IUserWithFolders user, IInstitution institution, bool hasReserveShelf,
            InstitutionBranding brandings
            , IEnumerable<InstitutionResourceLicense> institutionResourceLicenses, AuthenticationMethods method)
        {
            var publisherUser = user as PublisherUser;
            Init(user, publisherUser != null ? publisherUser.Institution : institution, hasReserveShelf, brandings,
                institutionResourceLicenses, method);
        }

        public AuthenticatedInstitution(PublisherUser user, IInstitution institution, bool hasReserveShelf,
            InstitutionBranding brandings
            , IEnumerable<InstitutionResourceLicense> institutionResourceLicenses, AuthenticationMethods method)
        {
            Init(user, institution, hasReserveShelf, brandings, institutionResourceLicenses, method);
            Publisher = new CachedPublisher(user.Publisher);
        }

        public AuthenticatedInstitution(IInstitution institution, bool hasReserveShelf, InstitutionBranding brandings
            , IEnumerable<InstitutionResourceLicense> institutionResourceLicenses, AuthenticationMethods method)
        {
            SetInstitution(institution, hasReserveShelf, brandings, institutionResourceLicenses);
            var role = new Role { Id = (int)RoleCode.Institution };
            UserRole = UserRole.ConvertToUserRole(role);
            AuthenticationMethod = method;
        }

        public bool IsInstitutionNameForDisplay { get; set; }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string AccountNumber { get; private set; }
        public Address Address { get; private set; }
        public string Phone { get; private set; }
        public bool DisplayAllProducts { get; private set; }
        public HomePage HomePage { get; private set; }
        public string LogoutUrl { get; private set; }
        public bool HasReservedShelf { get; private set; }

        public CachedUser User { get; private set; }

        public CachedPublisher Publisher { get; private set; }

        public UserRole UserRole { get; set; }

        public AuthenticationMethods AuthenticationMethod { get; private set; }

        public IEnumerable<License> Licenses => _licenses.Values;

        public int LicensedResourceCount => _licenses.Count;

        public string BrandingLogoFileName { get; private set; }
        public string BrandingMessage { get; private set; }
        public string BrandingInstitutionName { get; private set; }

        public AccessType AccessType { get; private set; }
        public IInstitutionAccountStatus AccountStatus { get; private set; }

        public bool IsEulaSigned { get; set; }
        public bool IsPdaEulaSigned { get; set; }
        public decimal Discount { get; private set; }
        public AnnualFee AnnualFee { get; private set; }
        public bool HouseAccount { get; private set; }
        public string AthensAffiliation { get; private set; }
        public bool ExpertReviewerUserEnabled { get; set; }
        public bool EnableHomePageCollectionLink { get; set; }
        public bool IncludeArchivedTitlesByDefault { get; private set; }

        public string ProxyPrefix { get; set; }
        public string UrlSuffix { get; set; }

        public string DisplayName { get; private set; }

        public string AuditId
        {
            get
            {
                if (_auditId == null)
                {
                    _auditId = User != null ? $"user id: {User.Id}, [{User.FirstName}]" : $"inst id: {Id}, [{Name}]";
                    if (!string.IsNullOrEmpty(_auditId) && _auditId.Length > 40)
                    {
                        _auditId = _auditId.Substring(0, 40);
                    }
                }

                return _auditId;
            }
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("AuthenticatedInstitution = [Id: {0}, AccountNumber: {1}, Name: {2}", Id, AccountNumber,
                Name);
            sb.AppendFormat(", AccountStatus: {0}", AccountStatus);
            sb.AppendFormat(", DisplayAllProducts: {0}", DisplayAllProducts);
            sb.AppendFormat(", HomePage: {0}", HomePage);
            sb.AppendFormat(", LogoutUrl: {0}", LogoutUrl);
            sb.AppendFormat(", HasReservedShelf: {0}", HasReservedShelf).AppendLine();
            sb.AppendFormat("\t, UserRole: {0}", UserRole);
            sb.AppendFormat(", User: {0}", User == null ? "null" : User.ToDebugString()).AppendLine();
            sb.AppendFormat("\t, AuthenticationMethod: {0}", AuthenticationMethod);
            sb.AppendFormat(", _resourceLicenses.Count: {0}", _licenses.Count);
            sb.AppendFormat(", DisplayName: {0}", DisplayName);
            sb.AppendFormat(", AuditId: {0}", AuditId).AppendLine();
            sb.AppendFormat("\t, Publisher: {0}", Publisher);
            sb.Append("]");
            return sb.ToString();
        }

        private void Init(IUserWithFolders user, IInstitution institution, bool hasReserveShelf,
            InstitutionBranding brandings
            , IEnumerable<InstitutionResourceLicense> institutionResourceLicenses, AuthenticationMethods method)
        {
            User = new CachedUser(user);
            SetInstitution(institution, hasReserveShelf, brandings, institutionResourceLicenses);
            UserRole = UserRole.ConvertToUserRole(user.Role);
            AuthenticationMethod = method;
        }

        private void SetInstitution(IInstitution institution, bool hasReserveShelf, InstitutionBranding brandings
            , IEnumerable<InstitutionResourceLicense> institutionResourceLicenses)
        {
            Id = institution.Id;
            Name = institution.Name;
            AccountNumber = institution.AccountNumber;
            DisplayAllProducts = institution.DisplayAllProducts;
            Address = institution.Address;
            Phone = institution.Phone;
            LogoutUrl = institution.LogUrl;
            HomePage = institution.HomePage.Id;
            AccountStatus = institution.AccountStatus;

            IsEulaSigned = institution.EULASigned;
            IsPdaEulaSigned = institution.PdaEulaSigned;
            Discount = institution.Discount;
            AnnualFee = institution.AnnualFee;
            HouseAccount = institution.HouseAccount;
            //AthensOrgId = institution.AthensOrgId;
            AthensAffiliation = institution.AthensAffiliation;
            ExpertReviewerUserEnabled = institution.ExpertReviewerUserEnabled;
            IncludeArchivedTitlesByDefault = institution.IncludeArchivedTitlesByDefault;
            EnableHomePageCollectionLink = institution.EnableHomePageCollectionLink;
            ProxyPrefix = institution.ProxyPrefix;
            UrlSuffix = institution.UrlSuffix;
            foreach (var institutionResourceLicense in institutionResourceLicenses)
            {
                if (!institutionResourceLicense.RecordStatus)
                {
                    continue;
                }

                _licenses.Add(institutionResourceLicense.ResourceId, new License(institutionResourceLicense));
            }

            if (User != null)
            {
                DisplayName = User.FirstName;
            }
            else if (brandings != null && string.IsNullOrWhiteSpace(brandings.InstitutionDisplayName))
            {
                DisplayName = brandings.InstitutionDisplayName;
                IsInstitutionNameForDisplay = true;
            }
            else
            {
                DisplayName = Name;
                IsInstitutionNameForDisplay = true;
            }

            if (brandings != null)
            {
                BrandingLogoFileName = brandings.LogoFileName;
                BrandingMessage = brandings.Message;
                BrandingInstitutionName = string.IsNullOrWhiteSpace(brandings.InstitutionDisplayName)
                    ? institution.Name
                    : brandings.InstitutionDisplayName;
            }
            else
            {
                BrandingInstitutionName = institution.Name;
            }

            HasReservedShelf = hasReserveShelf;
            AccessType = institution.AccessType.Id;
        }


        public bool IsRittenhouseAdmin()
        {
            return UserRole.Id == UserRole.RittenhouseAdministrator.Id;
        }

        public bool IsInstitutionAdmin()
        {
            return UserRole.Id == UserRole.InstitutionAdministrator.Id;
        }

        public bool IsPublisherUser()
        {
            return UserRole.Id == UserRole.PublisherUser.Id;
        }

        public bool IsSalesAssociate()
        {
            return UserRole.Id == UserRole.SalesAssociate.Id;
        }

        public bool IsExpertReviewer()
        {
            return UserRole.Id == UserRole.ExpertReviewer.Id;
        }

        public bool IsSubscriptionUser()
        {
            return UserRole.Id == UserRole.SubscriptionUser.Id;
        }

        public License GetResourceLicense(int resourceId)
        {
            return _licenses.ContainsKey(resourceId) ? _licenses[resourceId] : null;
        }

        public void AddTrialLicenses(IEnumerable<IResource> resources)
        {
            _licenses.Clear();
            foreach (var resource in resources)
            {
                //select i.iInstitutionId, r.iResourceId, i.iInstitutionAcctStatusId --,r.iResourceStatusId
                //     , 3 as iLicenseCount, '1/1/2000' as [dtFirstPurchaseDate]
                //from   tInstitution i
                // join  tResource r on r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8) and r.NotSaleable = 0
                //where  i.tiRecordStatus = 1
                //  and  i.iInstitutionAcctStatusId = 2
                //  and  getdate() between i.dtTrialAcctStart and i.dtTrialAcctEnd
                if (!resource.NotSaleable && (resource.StatusId == (int)ResourceStatus.Active ||
                                              resource.StatusId == (int)ResourceStatus.Archived ||
                                              resource.StatusId == (int)ResourceStatus.Forthcoming))
                {
                    var license = new License
                    {
                        FirstPurchaseDate = resource.ReleaseDate,
                        Id = 0,
                        InstitutionId = Id,
                        LicenseCount = 3,
                        LicenseType = LicenseType.Trial,
                        OriginalSource = LicenseOriginalSource.FirmOrder,
                        PdaAddedDate = null,
                        PdaAddedToCartById = null,
                        PdaAddedToCartDate = null,
                        PdaMaxViews = 0,
                        PdaViewCount = 0,
                        ResourceId = resource.Id,
                        RecordStatus = true
                    };
                    _licenses.Add(license.ResourceId, license);
                }
            }
        }

        public void ClearLicenses()
        {
            _licenses.Clear();
        }
    }
}