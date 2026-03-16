#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialDiscountModel
    {
        //[Display(Name = "Resource Count:")]
        //public int ResourceCount { get; set; }

        public SpecialDiscountModel()
        {
        }

        /// <summary>
        ///     Used for Viewing/Editing Discounts
        /// </summary>
        public SpecialDiscountModel(SpecialDiscount specialDiscount, List<string> iconNames) //, int resourceCount)
        {
            SpecialId = specialDiscount.SpecialId;
            AvailableIcons = iconNames;
            if (specialDiscount.Id > 0)
            {
                Id = specialDiscount.Id;
                DiscountPercentage = specialDiscount.DiscountPercentage;

                var foundIcon = iconNames.FirstOrDefault(x => x.Contains(specialDiscount.IconName));

                SelectIconIndex = iconNames.IndexOf(foundIcon); // + 1;
                IconName = foundIcon;
            }
            //ResourceCount = resourceCount;
        }

        /// <summary>
        ///     Used for new New Discounts
        /// </summary>
        public SpecialDiscountModel(int specialId, List<string> iconNames) //, int resourceCount)
        {
            SpecialId = specialId;
            AvailableIcons = iconNames;
        }

        public int Id { get; set; }

        [Display(Name = "Discount Percentage:")]
        [DisplayFormat(DataFormatString = "{0}%", ApplyFormatInEditMode = true)]
        public int DiscountPercentage { get; set; }

        [Display(Name = "Icon:")] public string IconName { get; set; }

        public List<string> AvailableIcons { get; set; }

        public int SelectIconIndex { get; set; }
        public int SpecialId { get; set; }
    }
}