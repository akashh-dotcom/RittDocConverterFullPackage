#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Institution;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class RequestedResourcesInstitution
    {
        public RequestedResourcesInstitution()
        {
        }

        public RequestedResourcesInstitution(IInstitution institution)
        {
            Institution = new MarketingInstitutionBase(institution);
            RequestedResources = new List<RequestedResource>();
        }

        public MarketingInstitutionBase Institution { get; set; }
        public List<RequestedResource> RequestedResources { get; set; }


        public int ResourceCount { get; set; }
        public int PurchasedResourceCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPurchasedPrice { get; set; }

        public void PopulateCounts()
        {
            ResourceCount = RequestedResources.Count;

            var purchasedRequestedResources = RequestedResources.Where(y => y.PurchasePrice > 0).ToList();

            PurchasedResourceCount = purchasedRequestedResources.Count;
            TotalPurchasedPrice = purchasedRequestedResources.Sum(x => x.PurchasePrice);
        }
    }

    public class RequestedResource
    {
        public IResource Resource { get; set; }
        public int RequestedCount { get; set; }
        public DateTime? LastRequestDate { get; set; }

        public int? CartId { get; set; }

        public int? OrderHistoryId { get; set; }
        public int? AutomatedCartId { get; set; }

        //public List<int> CartIds { get; set; }
        public Dictionary<int, string> AutomatedCartIdAndNames { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? AddedDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PurchasePrice { get; set; }

        public string ToDebug()
        {
            return $@"
Resource.Id: {Resource.Id}
RequestedCount: {RequestedCount}
LastRequestDate: {LastRequestDate}
CartId: {CartId}
OrderHistoryId: {OrderHistoryId}
AutomatedCartId: {AutomatedCartId}
AutomatedCartIdAndNames: {(AutomatedCartIdAndNames != null ? string.Join(",", AutomatedCartIdAndNames.Keys) : "")}
PurchaseDate: {PurchaseDate}
AddedDate: {AddedDate}
PurchasePrice: {PurchasePrice}
";
        }
    }
}