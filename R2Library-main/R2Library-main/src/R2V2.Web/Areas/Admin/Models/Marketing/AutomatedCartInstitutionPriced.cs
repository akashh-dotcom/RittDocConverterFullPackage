#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class AutomatedCartInstitutionPriced : MarketingInstitutionBase
    {
        public AutomatedCartInstitutionPriced(IInstitution institution, List<AutomatedCartPricedResources> resources) :
            base(institution)
        {
            Resources = resources;
        }

        public List<AutomatedCartPricedResources> Resources { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalListPrice => Resources.Sum(x => x.ListPrice);

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalDiscountPrice => Resources.Sum(x => x.DiscountPrice);

        public int TotalResources => Resources.Count;
        public int NewEdition => Resources.Sum(x => x.NewEditionCount);
        public int TriggeredPda => Resources.Sum(x => x.TriggeredPdaCount);
        public int Reviewed => Resources.Sum(x => x.ReviewedCount);
        public int Turnaway => Resources.Sum(x => x.TurnawayCount);
        public int Requested => Resources.Sum(x => x.RequestedCount);
    }

    public class AutomatedCartPricedResources
    {
        public int ResourceId { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal ListPrice { get; set; }
        public int NewEditionCount { get; set; }
        public int TriggeredPdaCount { get; set; }
        public int ReviewedCount { get; set; }
        public int TurnawayCount { get; set; }
        public int RequestedCount { get; set; }
    }
}