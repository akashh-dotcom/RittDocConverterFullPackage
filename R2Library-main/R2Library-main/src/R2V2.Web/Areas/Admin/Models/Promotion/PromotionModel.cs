#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Promotion
{
    public class PromotionModel : AdminBaseModel
    {
        public PromotionModel()
        {
        }

        public PromotionModel(IEnumerable<Product> products)
        {
            SetPromotionProductions(products.ToList());
            MaximumUses = 1;
        }

        public PromotionModel(CachedPromotion promotion, IEnumerable<Product> products)
        {
            PromotionId = promotion.Id;
            Code = promotion.Code;
            Name = promotion.Name;
            Description = promotion.Description;
            Discount = promotion.Discount;
            StartDate = promotion.StartDate;
            EndDate = promotion.EndDate;
            //OrderSource = promotion.OrderSource;
            RecordStatus = promotion.RecordStatus;
            EnableCartAlert = promotion.EnableCartAlert;
            PromotionProductIds = promotion.PromotionProductIds;
            SetPromotionProductions(products.ToList());

            MaximumUses = promotion.MaximumUses;

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

        public int PromotionId { get; set; }

        [Display(Name = @"Promotion Code:")]
        [Required]
        [StringLength(20, ErrorMessage = @"Promotion code cannot be longer than 20 characters.")]
        public string Code { get; set; }

        [Display(Name = @"Promotion Name:")]
        [Required]
        [StringLength(100, ErrorMessage = @"Promotion name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Display(Name = @"Promotion Description:")]
        [Required]
        [StringLength(255, ErrorMessage = @"Promotion description cannot be longer than 255 characters.")]
        public string Description { get; set; }

        [Display(Name = @"Discount Percentage:")]
        [Required]
        public int Discount { get; set; }

        [Display(Name = @"Promotion StartDate:")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            NullDisplayText = "Not Set", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required]
        [RegularExpression(@"^\d{1,2}\/\d{1,2}\/\d{4}$",
            ErrorMessage = @"Please specify a valid start date. (MM/DD/YYYY)")]
        public DateTime StartDate { get; set; }

        [Display(Name = @"Promotion End Date:")]
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            NullDisplayText = "Not Set", ApplyFormatInEditMode = true)]
        [RegularExpression(@"^\d{1,2}\/\d{1,2}\/\d{4}$",
            ErrorMessage = @"Please specify a valid end date. (MM/DD/YYYY)")]
        public DateTime EndDate { get; set; }

        [Display(Name = @"Prelude Order Source:")]
        public string OrderSource { get; set; }

        [Display(Name = @"Maximum Uses per Institution:")]
        public int MaximumUses { get; set; }

        [Display(Name = @"Enable Cart Alert:")]
        public bool EnableCartAlert { get; set; }

        public bool RecordStatus { get; set; }

        public string Status { get; private set; }

        //public List<Product> ProductsList { get; set; }
        private IList<int> PromotionProductIds { get; }


        [Display(Name = @"Assign Products Included: ")]
        public List<SelectListItem> ProductsSelectListItems { get; private set; }

        public List<SelectListItem> SelectedProductsSelectListItems { get; private set; }
        public int[] ProductsSelected { get; set; }

        public void PopulateProductsSelectListItems(List<Product> products)
        {
            SelectedProductsSelectListItems = new List<SelectListItem>();

            if (PromotionProductIds != null)
            {
                foreach (var promotionProduct in PromotionProductIds.Where(x =>
                             SelectedProductsSelectListItems.All(y => y.Value != x.ToString())))
                {
                    var product = products.FirstOrDefault(x => x.Id == promotionProduct);
                    if (product != null)
                    {
                        SelectedProductsSelectListItems.Add(new SelectListItem
                            { Text = product.Name, Value = product.Id.ToString() });
                    }
                }

                ProductsSelected = PromotionProductIds.Select(x => x).Distinct().ToArray();
            }


            ProductsSelectListItems = new List<SelectListItem>();
            foreach (var product in products)
            {
                var item = new SelectListItem { Text = product.Name, Value = product.Id.ToString() };
                if (!SelectedProductsSelectListItems.Select(x => x.Value).Contains(item.Value))
                {
                    ProductsSelectListItems.Add(item);
                }
            }
        }

        public void SetPromotionProductions(IEnumerable<Product> products)
        {
            PopulateProductsSelectListItems(products.ToList());
        }
    }
}