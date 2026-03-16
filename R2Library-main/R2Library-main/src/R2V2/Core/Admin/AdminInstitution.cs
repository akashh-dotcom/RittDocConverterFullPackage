#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Core.Admin
{
    [Serializable]
    public class AdminInstitution : IAdminInstitution, IDebugInfo
    {
        private readonly List<License> _licenses = new List<License>();


        public AdminInstitution(AuthenticatedInstitution authenticatedInstitution)
        {
            Id = authenticatedInstitution.Id;
            Name = authenticatedInstitution.Name;
            AccountNumber = authenticatedInstitution.AccountNumber;
            Address = authenticatedInstitution.Address;
            Phone = authenticatedInstitution.Phone;

            IsEulaSigned = authenticatedInstitution.IsEulaSigned;
            IsPdaEulaSigned = authenticatedInstitution.IsPdaEulaSigned;
            Discount = authenticatedInstitution.Discount;
            AnnualFee = authenticatedInstitution.AnnualFee;
            HouseAccount = authenticatedInstitution.HouseAccount;
            AthensAffiliation = authenticatedInstitution.AthensAffiliation;
            AccountStatus = authenticatedInstitution.AccountStatus;
            ExpertReviewerUserEnabled = authenticatedInstitution.ExpertReviewerUserEnabled;

            ProxyPrefix = authenticatedInstitution.ProxyPrefix;
            UrlSuffix = authenticatedInstitution.UrlSuffix;
            _licenses.AddRange(authenticatedInstitution.Licenses);
        }

        public AdminInstitution(IInstitution institution)
        {
            if (institution == null)
            {
                return;
            }

            Id = institution.Id;
            Name = institution.Name;
            AccountNumber = institution.AccountNumber;
            Address = institution.Address;
            Phone = institution.Phone;

            IsEulaSigned = institution.EULASigned;
            IsPdaEulaSigned = institution.PdaEulaSigned;
            Discount = institution.Discount;
            AnnualFee = institution.AnnualFee;
            HouseAccount = institution.HouseAccount;
            AthensAffiliation = institution.AthensAffiliation;
            AccountStatus = institution.AccountStatus;
            ExpertReviewerUserEnabled = institution.ExpertReviewerUserEnabled;
            ProxyPrefix = institution.ProxyPrefix;
            UrlSuffix = institution.UrlSuffix;
            foreach (var institutionResourceLicense in institution.InstitutionResourceLicenses)
            {
                if (!institutionResourceLicense.RecordStatus)
                {
                    continue;
                }

                _licenses.Add(new License(institutionResourceLicense));
            }
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string AccountNumber { get; private set; }
        public Address Address { get; private set; }
        public string Phone { get; private set; }
        public bool IsEulaSigned { get; set; }
        public bool IsPdaEulaSigned { get; set; }
        public decimal Discount { get; private set; }
        public AnnualFee AnnualFee { get; private set; }
        public bool HouseAccount { get; private set; }
        public string AthensAffiliation { get; private set; }
        public IInstitutionAccountStatus AccountStatus { get; private set; }
        public bool ExpertReviewerUserEnabled { get; private set; }

        public IEnumerable<License> Licenses => _licenses;

        public string ProxyPrefix { get; set; }
        public string UrlSuffix { get; set; }

        public License GetLicense(int resourceId)
        {
            return _licenses.FirstOrDefault(x => x.ResourceId == resourceId);
        }

        public void SetAccoutStatusForDebugging(IInstitutionAccountStatus accountStatus)
        {
            AccountStatus = accountStatus;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("AdminInstitution = [Id: {0}, AccountNumber: {1}, Name: {2}", Id, AccountNumber, Name);
            sb.AppendFormat(", AccountStatus: {0}", AccountStatus);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", IsEulaSigned: {0}", IsEulaSigned);
            sb.AppendFormat(", IsPdaEulaSigned: {0}", IsPdaEulaSigned);
            sb.AppendFormat(", AnnualFee: {0}", AnnualFee);
            sb.AppendFormat(", HouseAccount: {0}", HouseAccount);
            sb.AppendFormat(", AthensAffiliation: {0}", AthensAffiliation);
            sb.AppendFormat(", ExpertReviewerUserEnabled: {0}", ExpertReviewerUserEnabled);
            sb.AppendFormat(", Phone: {0}", Phone);
            sb.AppendLine().AppendFormat("\t, {0}", Address == null ? "Address = null" : Address.ToDebugString());
            sb.AppendLine().AppendFormat("\t, License.Count: {0}", Licenses.Count());
            sb.Append("]");
            return sb.ToString();
        }

        public void ClearLicenses()
        {
            _licenses.Clear();
        }
    }
}