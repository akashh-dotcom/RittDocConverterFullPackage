#region

using System.ComponentModel.DataAnnotations;
using R2V2.Core.Promotion;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ResourcePromotion
{
    public class PromoteQueueResource
    {
        public Resource.Resource Resource { get; set; }
        public ResourcePromoteQueue ResourcePromoteQueue { get; set; }

        [Display(Name = @"Added By: ")] public string AddedByUserFullName { get; set; }

        [Display(Name = @"Promoted By: ")] public string PromotedByUserFullName { get; set; }

        [Display(Name = @"ISBN 10/13/e: ")]
        public string Isbns => $"{Resource.Isbn10} / {Resource.Isbn13} / {Resource.EIsbn}";

        [Display(Name = @"Batch Name: ")] public string BatchName => ResourcePromoteQueue.PromoteBatchName;

        [Display(Name = @"Batch Key: ")] public string BatchKey => ResourcePromoteQueue.BatchKey.ToString();
    }
}