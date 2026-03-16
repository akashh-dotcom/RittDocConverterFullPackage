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
    public class SpecialReportDetail : ReportModel, IDiscountReportDetail
    {
        public SpecialReportDetail()
        {
        }

        public SpecialReportDetail(List<Core.Resource.Special> specials, int selectSpecialId)
        {
            SetSpecialList(specials);
            SelectSpecialId = selectSpecialId;
        }

        [Display(Name = "Specials:")] public SelectList SpecialList { get; set; }

        public int SelectSpecialId { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime EndDate { get; set; }

        public string SpecialName { get; set; }

        [DisplayFormat(DataFormatString = "${0:#,##0.00}")]
        public decimal TotalSale { get; set; }

        public Dictionary<string, List<DiscountResourceDetail>> DiscountResources { get; set; }

        private void SetSpecialList(List<Core.Resource.Special> specials)
        {
            if (specials != null && specials.Any())
            {
                var selectListItems =
                    specials.Select(special =>
                            new SelectListItem
                            {
                                Text =
                                    special.Name.Length > 100
                                        ? $"{special.Name.Substring(0, 100)}..."
                                        : special.Name,
                                Value = special.Id.ToString()
                            })
                        .ToList();
                SpecialList = new SelectList(selectListItems, "Value", "Text");
            }
        }

        public void SetDiscountResources(Core.Resource.Special special, List<DiscountResource> discountResources)
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

            SelectSpecialId = special.Id;
            StartDate = special.StartDate;
            EndDate = special.EndDate;
            SpecialName = special.Name;

            TotalSale = discountResources.Sum(x => x.Total);
        }
    }
}