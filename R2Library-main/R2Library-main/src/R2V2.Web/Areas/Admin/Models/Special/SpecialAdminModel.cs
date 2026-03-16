#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialAdminModel
    {
        public SpecialAdminModel(SpecialModel specialModel, IEnumerable<SpecialDiscountModel> specialDiscounts)
        {
            SpecialId = specialModel.Id;
            Name = specialModel.Name;
            StartDate = specialModel.StartDate;
            EndDate = specialModel.EndDate;

            if (specialDiscounts != null)
            {
                SpecialDiscounts = specialDiscounts.Select(x => new SpecialDiscountAdminModel(x)).ToList();
            }
        }

        public int SpecialId { get; set; }
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            ApplyFormatInEditMode = true)]
        [DateTenYears("StartDate")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            ApplyFormatInEditMode = true)]
        [DateTenYears("EndDate")]
        public DateTime EndDate { get; set; }

        public List<SpecialDiscountAdminModel> SpecialDiscounts { get; set; }
    }

    public class SpecialDiscountAdminModel
    {
        public SpecialDiscountAdminModel(SpecialDiscountModel specialDiscount)
        {
            SpecialDiscountId = specialDiscount.Id;
            DiscountPercentage = specialDiscount.DiscountPercentage;
            IconUrl = specialDiscount.IconName;
        }

        public int SpecialDiscountId { get; set; }

        [DisplayFormat(DataFormatString = "{0}%", ApplyFormatInEditMode = true)]
        public int DiscountPercentage { get; set; }

        public string IconUrl { get; set; }

        public int ResourceCount { get; private set; }

        /// <summary>
        ///     Sets the Resource Count for the discount. This will only count a resource if it belongs to this discount (List does
        ///     not have to filtered to this specific discount).
        /// </summary>
        public void SetResourceCount(List<SpecialResourceModel> specialResources)
        {
            if (SpecialDiscountId > 0 && specialResources != null)
            {
                ResourceCount = specialResources.Count(x => x.SpecialDiscountId == SpecialDiscountId);
            }
        }
    }
}