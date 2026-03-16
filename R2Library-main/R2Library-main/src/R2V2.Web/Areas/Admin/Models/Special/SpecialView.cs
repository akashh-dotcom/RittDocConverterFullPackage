#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialView : AdminBaseModel
    {
        public SpecialView()
        {
        }

        /// <summary>
        ///     Used first view of Special
        /// </summary>
        public SpecialView(string specialBaseIconUrl, SpecialModel special,
            IEnumerable<SpecialDiscountModel> specialDiscounts,
            IEnumerable<SpecialDiscountResourceModel> specialDiscountResources)
        {
            SpecialBaseIconUrl = specialBaseIconUrl;
            Special = special;

            SpecialDiscountResources = specialDiscountResources;
            SpecialDiscounts = specialDiscounts;
        }

        /// <summary>
        ///     Used for Edit specific Discount
        /// </summary>
        public SpecialView(string specialBaseIconUrl, SpecialModel special,
            List<SpecialDiscountModel> specialDiscounts,
            IEnumerable<SpecialDiscountResourceModel> specialDiscountResources, int specialDiscountId)
        {
            SpecialBaseIconUrl = specialBaseIconUrl;
            Special = special;

            SpecialDiscountResources = specialDiscountResources;
            EditSpecialDiscount = specialDiscounts != null
                ? specialDiscounts.FirstOrDefault(x => x.Id == specialDiscountId)
                : null;

            if (EditSpecialDiscount == null && specialDiscountId < 0)
            {
                EditSpecialDiscount = new SpecialDiscountModel();
            }

            //SpecialDiscounts = specialDiscounts;
            if (specialDiscounts != null)
            {
                var specialDiscountsToDisplay = specialDiscounts.Where(x => x.Id != specialDiscountId);
                IEnumerable<SpecialDiscountModel> specialDiscountModels =
                    specialDiscountsToDisplay as SpecialDiscountModel[] ?? specialDiscountsToDisplay.ToArray();
                if (specialDiscountModels.Any())
                {
                    SpecialDiscounts = specialDiscountModels.ToList();
                }
            }
        }

        /// <summary>
        ///     Used for New Discounts
        /// </summary>
        public SpecialView(string specialBaseIconUrl, SpecialModel special,
            List<SpecialDiscountModel> specialDiscounts,
            IEnumerable<SpecialDiscountResourceModel> specialDiscountResources,
            List<string> iconUrls)
        {
            SpecialBaseIconUrl = specialBaseIconUrl;
            Special = special;

            SpecialDiscountResources = specialDiscountResources;

            EditSpecialDiscount = new SpecialDiscountModel { AvailableIcons = iconUrls, SpecialId = special.Id };

            if (specialDiscounts != null)
            {
                if (specialDiscounts.Any())
                {
                    SpecialDiscounts = specialDiscounts.ToList();
                }
            }
        }

        public SpecialModel Special { get; set; }
        public IEnumerable<SpecialDiscountModel> SpecialDiscounts { get; set; }
        public IEnumerable<SpecialDiscountResourceModel> SpecialDiscountResources { get; set; }

        public SpecialDiscountModel EditSpecialDiscount { get; set; }

        public string SpecialBaseIconUrl { get; set; }
        public bool IsDiscountEdit { get; set; }

        public void SetNewDiscount(List<string> iconUrls)
        {
            EditSpecialDiscount = new SpecialDiscountModel { AvailableIcons = iconUrls, SpecialId = Special.Id };
        }

        public void SetEditDiscount(int specialDiscountId)
        {
            EditSpecialDiscount = SpecialDiscounts.FirstOrDefault(x => x.Id == specialDiscountId);
            SpecialDiscounts = SpecialDiscounts.Where(x => x.Id != specialDiscountId);
        }
    }
}