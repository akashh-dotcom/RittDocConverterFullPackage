#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaRules
{
    public class PdaRuleResourcesModel : AdminBaseModel
    {
        public PdaRuleResourcesModel(IAdminInstitution adminInstitution)
            : base(adminInstitution)
        {
        }

        public PdaRuleGist RuleGist { get; set; }

        public List<string> ResourceString { get; set; }

        public bool HideForm { get; set; }

        public bool RuleRan { get; set; }

        public string PageTitle { get; set; }

        public void PopulateRuleAndResources(PdaRule rule, IEnumerable<IResource> ruleResources)
        {
            var resources = ruleResources != null ? ruleResources.ToList() : new List<IResource>();
            RuleGist = new PdaRuleGist(rule);
            ResourceString = new List<string>();
            foreach (var resource in resources)
            {
                var discountPrice = resource.ListPrice - Institution.Discount / 100 * resource.ListPrice;
                ResourceString.Add(
                    $"{resource.Title}, ISBN: {resource.Isbn13} at {resource.DiscountPriceString(discountPrice)}");
            }
        }
    }
}