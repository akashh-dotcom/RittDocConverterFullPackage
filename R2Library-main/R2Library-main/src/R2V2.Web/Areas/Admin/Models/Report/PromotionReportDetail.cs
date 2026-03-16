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
    public class PromotionReportDetail : ReportModel, IDiscountReportDetail
    {
        public PromotionReportDetail()
        {
        }

        public PromotionReportDetail(IEnumerable<Core.CollectionManagement.Promotion> promotions, int selectPromotionId)
        {
            SetPromotionList(promotions);
            SelectPromotionId = selectPromotionId;
        }

        [Display(Name = "Promotions:")] public SelectList PromotionList { get; set; }

        public int SelectPromotionId { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime EndDate { get; set; }

        public string PromotionName { get; set; }

        [DisplayFormat(DataFormatString = "${0:#,##0.00}")]
        public decimal TotalSale { get; set; }

        public Dictionary<string, List<DiscountResourceDetail>> DiscountResources { get; set; }

        private void SetPromotionList(IEnumerable<Core.CollectionManagement.Promotion> promotions)
        {
            var selectListItems =
                promotions.Select(special => new SelectListItem { Text = special.Name, Value = special.Id.ToString() })
                    .ToList();
            PromotionList = new SelectList(selectListItems, "Value", "Text");
        }

        public void SetDiscountResources(Core.CollectionManagement.Promotion promotion,
            List<DiscountResource> discountResources)
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

            SelectPromotionId = promotion.Id;
            StartDate = promotion.StartDate;
            EndDate = promotion.EndDate;
            PromotionName = promotion.Name;

            TotalSale = discountResources.Sum(x => x.Total);
        }
    }
}