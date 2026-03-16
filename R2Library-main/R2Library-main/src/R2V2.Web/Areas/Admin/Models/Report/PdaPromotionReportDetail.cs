#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class PdaPromotionReportDetail : ReportModel, IDiscountReportDetail
    {
        public PdaPromotionReportDetail()
        {
        }

        public PdaPromotionReportDetail(List<Core.CollectionManagement.PdaPromotion> pdaPromotions,
            int selectPdaPromotionId)
        {
            SetPdaPromotionList(pdaPromotions);
            SelectPdaPromotionId = selectPdaPromotionId;
        }

        [Display(Name = "PDA Promotions:")] public SelectList PdaPromotionList { get; set; }

        public int SelectPdaPromotionId { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime EndDate { get; set; }

        public string PdaPromotionName { get; set; }

        [DisplayFormat(DataFormatString = "${0:#,##0.00}")]
        public decimal TotalSale { get; set; }

        public Dictionary<string, List<DiscountResourceDetail>> DiscountResources { get; set; }

        private void SetPdaPromotionList(List<Core.CollectionManagement.PdaPromotion> pdaPromotions)
        {
            if (pdaPromotions != null && pdaPromotions.Any())
            {
                var selectListItems =
                    pdaPromotions.Select(special => new SelectListItem
                            { Text = special.Name, Value = special.Id.ToString() })
                        .ToList();
                PdaPromotionList = new SelectList(selectListItems, "Value", "Text");
            }
        }

        public void SetDiscountResources(Core.CollectionManagement.PdaPromotion promotion,
            List<DiscountResource> discountResources)
        {
            if (discountResources != null && discountResources.Any())
            {
                var accountNumbers = discountResources.Select(x => x.AccountNumber).Distinct();
                DiscountResources = new Dictionary<string, List<DiscountResourceDetail>>();
                foreach (var accountNumber in accountNumbers)
                {
                    DiscountResources.Add(accountNumber,
                        discountResources.Where(x => x.AccountNumber == accountNumber)
                            .Select(y => new DiscountResourceDetail(y))
                            .ToList());
                }

                SelectPdaPromotionId = promotion.Id;
                StartDate = promotion.StartDate;
                EndDate = promotion.EndDate;
                PdaPromotionName = promotion.Name;

                TotalSale = discountResources.Sum(x => x.Total);
            }
        }
    }
}