#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using R2V2.Core.Recommendations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Recommendations
{
    public class Recommended
    {
        public Recommended()
        {
        }

        public Recommended(Recommendation recommendation)
        {
            Id = recommendation.Id;
            InstitutionId = recommendation.InstitutionId;
            ResourceId = recommendation.ResourceId;


            CreationDate = recommendation.CreationDate;
            AlertSentDate = recommendation.AlertSentDate;
            AddedToCartDate = recommendation.AddedToCartDate;
            PurchaseDate = recommendation.PurchaseDate;
            DeletedDate = recommendation.DeletedDate;
            DeletedNotes = recommendation.DeletedNotes;

            Notes = recommendation.Notes;

            ExpertReviewerUser = recommendation.RecommendedByUser == null
                ? null
                : new RecommendedUser(recommendation.RecommendedByUser);
            AddedToCartByUser = recommendation.AddedToCartByUser == null
                ? null
                : new RecommendedUser(recommendation.AddedToCartByUser);
            PurchasedByUser = recommendation.PurchasedByUser == null
                ? null
                : new RecommendedUser(recommendation.PurchasedByUser);
            DeletedByUser = recommendation.RecommendedByUser == null
                ? null
                : new RecommendedUser(recommendation.DeletedByUser);

            var sb = new StringBuilder();
            sb.AppendFormat("Recommended On: {0:MM/dd/yyyy}", recommendation.CreationDate);
            if (recommendation.RecommendedByUser != null)
            {
                sb.AppendFormat(" by {0} {1} ", recommendation.RecommendedByUser.FirstName,
                    recommendation.RecommendedByUser.LastName);
                if (!string.IsNullOrWhiteSpace(recommendation.RecommendedByUser.Department.Name))
                {
                    sb.AppendFormat("[{0}]", recommendation.RecommendedByUser.Department.Name);
                }
            }

            DisplayText = sb.ToString();


            //DisplayText = string.Format("Recommended On: {0:MM/dd/yyyy} by {1} {2} [{3}]", recommendation.CreationDate,
            //        recommendation.RecommendedByUser.FirstName, recommendation.RecommendedByUser.LastName,
            //        recommendation.RecommendedByUser.Department.Name);
        }

        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public int ResourceId { get; set; }

        [Display(Name = @"Recommended On: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime CreationDate { get; set; }

        public DateTime? AlertSentDate { get; set; }


        [Display(Name = @"Added to Cart On: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? AddedToCartDate { get; set; }

        [Display(Name = @"Purchased On: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = @"Deleted On: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DeletedDate { get; set; }

        [Display(Name = @"Notes:")]
        [StringLength(1000, ErrorMessage = @"Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }

        [Display(Name = @"Delete Notes:")]
        [StringLength(1000, ErrorMessage = @"Notes cannot exceed 1000 characters")]
        public string DeletedNotes { get; set; }

        public RecommendedUser ExpertReviewerUser { get; set; }
        public RecommendedUser AddedToCartByUser { get; set; }
        public RecommendedUser PurchasedByUser { get; set; }
        public RecommendedUser DeletedByUser { get; set; }

        public string DisplayText { get; set; }
    }
}