#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaRules
{
    public class PdaProfileModel : AdminBaseModel
    {
        public PdaProfileModel(IAdminInstitution adminInstitution)
            : base(adminInstitution)
        {
        }

        public List<PdaRuleGist> Rules { get; set; }

        public void PopulateRules(IEnumerable<PdaRule> rules)
        {
            Rules = new List<PdaRuleGist>();
            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    Rules.Add(new PdaRuleGist(rule));
                }
            }
        }
    }
}