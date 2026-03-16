#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaRules
{
    public class PdaRuleGist
    {
        public PdaRuleGist(PdaRule rule)
        {
            RuleId = rule.Id;
            Name = rule.Name;
            LastUpdated = rule.LastUpdated ?? rule.CreationDate;

            ResourcesAdded = rule.ResourcesAdded;
            ResourcesToAdd = rule.ResourcesToAdd;

            SetDescription(rule);
        }

        public int RuleId { get; set; }
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime LastUpdated { get; set; }

        public string Description { get; set; }

        public int ResourcesAdded { get; set; }
        public int ResourcesToAdd { get; set; }

        public void SetDescription(PdaRule rule)
        {
            var sb = new StringBuilder();

            if (rule.ExecuteForFuture)
            {
                sb.AppendFormat("<b>Applied to Future Titles</b>");
                sb.Append("<br/>");
            }

            if (rule.MaxPrice > 0)
            {
                sb.AppendFormat("Max Price: {0:C}", rule.MaxPrice);
                sb.Append("<br/>");
            }


            if (rule.IncludeNewEditionFirm || rule.IncludeNewEditionPda)
            {
                const string newEditionsString = "New Editions of {0} {1} {2} Resources";

                if (rule.IncludeNewEditionFirm && rule.IncludeNewEditionPda)
                {
                    sb.AppendFormat(newEditionsString, "Purchased", "and", "PDA");
                }
                else if (rule.IncludeNewEditionFirm)
                {
                    sb.AppendFormat(newEditionsString, "Purchased", "", "");
                }
                else if (rule.IncludeNewEditionPda)
                {
                    sb.AppendFormat(newEditionsString, "PDA", "", "");
                }

                sb.Append("<br/>");
            }

            if (rule.PracticeAreas != null && rule.PracticeAreas.Any())
            {
                sb.AppendFormat("Practice Areas Selected: {0} ", rule.PracticeAreas.Count);
                sb.Append("<br/>");
            }

            if (rule.Specialties != null && rule.Specialties.Any())
            {
                sb.AppendFormat("Disciplines Selected: {0} ", rule.Specialties.Count);
                sb.Append("<br/>");
            }

            if (rule.Collections != null && rule.Collections.Any())
            {
                sb.AppendFormat("Special Collections Selected: {0} ", rule.Collections.Count);
                sb.Append("<br/>");
            }

            Description = sb.ToString();
        }
    }
}