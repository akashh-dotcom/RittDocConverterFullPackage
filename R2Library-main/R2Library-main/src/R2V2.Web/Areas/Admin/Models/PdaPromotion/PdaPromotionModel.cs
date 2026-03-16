#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaPromotion
{
    public class PdaPromotionModel : AdminBaseModel
    {
        public PdaPromotionModel()
        {
        }

        public PdaPromotionModel(CachedPdaPromotion pdaPromotion)
        {
            if (pdaPromotion == null)
            {
                return;
            }

            PdaPromotionId = pdaPromotion.Id;
            Name = pdaPromotion.Name;
            Description = pdaPromotion.Description;
            Discount = pdaPromotion.Discount;
            StartDate = pdaPromotion.StartDate;
            EndDate = pdaPromotion.EndDate;
            PdaPromotionText = pdaPromotion.PromotionText;


            if (EndDate < DateTime.Now)
            {
                Status = "Expired";
            }
            else if (StartDate > DateTime.Now)
            {
                Status = "Future";
            }
            else
            {
                Status = "Active";
            }
        }

        public int PdaPromotionId { get; set; }

        [Display(Name = "PDA Promotion Name:")]
        [Required]
        [StringLength(100, ErrorMessage = "Pda Promotion name cannot be longer than 20 characters.")]
        public string Name { get; set; }


        [Display(Name = "PDA Promotion Description:")]
        [Required]
        [StringLength(255, ErrorMessage = "Promotion description cannot be longer than 20 characters.")]
        public string Description { get; set; }

        [Display(Name = "Discount Percentage:")]
        [Required]
        public int Discount { get; set; }

        [Display(Name = "PDA Promotion StartDate:")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            NullDisplayText = "Not Set",
            ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required]
        [RegularExpression(@"^\d{1,2}\/\d{1,2}\/\d{4}$",
            ErrorMessage = "Please specify a valid start date. (MM/DD/YYYY)")]
        public DateTime StartDate { get; set; }

        [Display(Name = "PDA Promotion End Date:")]
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            NullDisplayText = "Not Set",
            ApplyFormatInEditMode = true)]
        [RegularExpression(@"^\d{1,2}\/\d{1,2}\/\d{4}$", ErrorMessage = "Please specify a valid end date. (MM/DD/YYYY)")
        ]
        public DateTime EndDate { get; set; }

        public bool RecordStatus { get; set; }

        public string Status { get; private set; }

        public string PdaPromotionText { get; private set; }
    }
}