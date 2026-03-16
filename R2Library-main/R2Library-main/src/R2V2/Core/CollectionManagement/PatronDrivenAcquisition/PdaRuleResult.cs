#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaRuleResult : IDebugInfo
    {
        private readonly List<InstitutionResourceLicense> _pdaLicensesAdded = new List<InstitutionResourceLicense>();

        public PdaRuleResult(IEnumerable<IResource> resources, PdaRule pdaRule, IAdminInstitution adminInstitution,
            IUser user, int pdaMaxViews)
        {
            AvailableResources = resources;
            PdaRule = pdaRule;
            AdminInstitution = adminInstitution;
            User = user;
            PdaMaxViews = pdaMaxViews;
            BatchId = Guid.NewGuid();
        }

        //public int NumberOfPdaLicensesAdded { get; set; }
        public IEnumerable<IResource> AvailableResources { get; set; }

        //public IList<IResource> PdaAddedResources { get; set; }
        public PdaRule PdaRule { get; set; }
        public IAdminInstitution AdminInstitution { get; set; }
        public IUser User { get; set; }
        public int PdaMaxViews { get; set; }
        public Guid BatchId { get; }

        public IEnumerable<InstitutionResourceLicense> PdaLicensesAdded => _pdaLicensesAdded;

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PdaRuleResult = [");
            //sb.AppendFormat("NumberOfPdaLicensesAdded: {0}", NumberOfPdaLicensesAdded);
            sb.AppendFormat(", PdaMaxViews: {0}", PdaMaxViews);
            sb.AppendFormat(", BatchId: {0}", BatchId);
            sb.AppendLine().Append("\t");
            sb.AppendFormat(", AdminInstitution: {0}", AdminInstitution.ToDebugString());
            sb.AppendLine().Append("\t");
            sb.AppendFormat(", User: {0}", User.ToDebugString());
            sb.AppendLine().Append("\t");

            sb.AppendFormat(", AvailableResources: [");
            if (AvailableResources != null)
            {
                foreach (var resource in AvailableResources)
                {
                    sb.AppendLine().AppendFormat("\t\tAvailableResources = [Id: {0}, Isbn: {1}, Title: {2}],",
                        resource.Id, resource.Isbn, resource.Title);
                }
            }

            sb.AppendLine().Append("\t]");

            sb.AppendLine().Append("\t");
            sb.AppendFormat(", PdaLicensesAdded.Ids: [");
            if (_pdaLicensesAdded != null)
            {
                var ids = _pdaLicensesAdded.Select(x => x.Id).ToArray();
                sb.Append(string.Join(",", ids));
            }

            sb.Append("]");
            sb.AppendLine().Append("]");

            return sb.ToString();
        }


        public void AddPdaLicense(InstitutionResourceLicense institutionResourceLicense)
        {
            _pdaLicensesAdded.Add(institutionResourceLicense);
        }

        public void AddPdaLicenses(IEnumerable<InstitutionResourceLicense> institutionResourceLicenses)
        {
            _pdaLicensesAdded.AddRange(institutionResourceLicenses);
        }
    }
}