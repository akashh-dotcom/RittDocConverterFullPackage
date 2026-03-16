#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ResourcePromotion
{
    public class ResourcePromotionViewModel : AdminBaseModel
    {
        public List<PromoteQueueResource> Resources { get; set; }


        [Display(Name = "Promotion Batch Name:")]
        [Required]
        [StringLength(100, ErrorMessage = @"Batch name must be between 10 and 100 characters", MinimumLength = 10)]
        public string BatchName { get; set; }

        public string ErrorMessage { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }
        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }
        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}