#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewModel
    {
        private readonly IList<ReviewResource> _resources = new List<ReviewResource>();

        public int Id { get; set; }

        [Required]
        [Display(Name = @"Name:")]
        [StringLength(100, ErrorMessage = @"Name Max Length is 100 characters")]
        public string Name { get; set; }

        [Required]
        [Display(Name = @"Description:")]
        [StringLength(500, ErrorMessage = @"Description Max Length is 500 characters")]
        public string Description { get; set; }

        [Display(Name = @"Expert Reviewer Users:")]
        public List<ReviewUser> ReviewUsers { get; set; }

        public int ResourceCount { get; set; }

        public string ResourceTitleRemoved { get; set; }

        //public IEnumerable<ReviewResourceSummary> Resources { get; set; }
        public IEnumerable<ReviewResource> Resources => _resources;

        public void AddReviewResource(ReviewResource reviewResource)
        {
            _resources.Add(reviewResource);
        }
    }
}