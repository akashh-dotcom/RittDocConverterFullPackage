#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class SavedReportDetail : ReportModel //AdminBaseModel
    {
        private SelectList _frequencyList;


        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SavedReportDetail()
        {
        }

        public SavedReportDetail(ReportQuery reportQuery, IAdminInstitution institution) : base(institution)
        {
            ReportQuery = new ReportQuery
            {
                DateRangeEnd = reportQuery.DateRangeEnd,
                DateRangeStart = reportQuery.DateRangeStart,
                FilterByIpRanges = reportQuery.FilterByIpRanges,
                Page = reportQuery.Page,
                Period = reportQuery.Period,
                PracticeAreaId = reportQuery.PracticeAreaId,
                PublisherId = reportQuery.PublisherId,
                ResourceId = reportQuery.ResourceId,
                SelectedIpAddressRangeIds = reportQuery.SelectedIpAddressRangeIds,
                IncludePurchasedTitles = reportQuery.IncludePurchasedTitles,
                IncludePdaTitles = reportQuery.IncludePdaTitles,
                IncludeTocTitles = reportQuery.IncludeTocTitles,
                IncludeTrialStats = reportQuery.IncludeTrialStats,
                InstitutionId = reportQuery.InstitutionId,
                EditableIpAddressRange = reportQuery.EditableIpAddressRange,
                ReportTypeId = reportQuery.ReportTypeId
            };
            Frequency = reportQuery.Frequency;
            InstitutionId = reportQuery.InstitutionId;
            ReportId = reportQuery.ReportId;
            Type = (ReportType)reportQuery.ReportTypeId;
            IsLicenseTypeEnabled = true;
        }

        [Display(Name = @"Report Name: ")]
        [Required(ErrorMessage = @"Report name is required", AllowEmptyStrings = false)]
        [StringLength(50, ErrorMessage = @"Report name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        [Display(Name = @"Email Address: ")]
        [Required(ErrorMessage = @"Email address is required", AllowEmptyStrings = false)]
        [StringLength(250, ErrorMessage = @"Email address cannot be longer than 250 characters.")]
        [EmailAddress(ErrorMessage = @"Invalid email address")]
        public string EmailAddress { get; set; }

        [Display(Name = @"Description: ")]
        [StringLength(250, ErrorMessage = @"Description cannot be longer than 250 characters.")]
        public string Description { get; set; }

        [Display(Name = @"Email Schedule:")]
        public SelectList FrequencyList => _frequencyList ??
                                           (_frequencyList = new SelectList(new List<SelectListItem>
                                           {
                                               new SelectListItem
                                               {
                                                   Text = ReportFrequency.None.ToDescription(),
                                                   Value = $"{ReportFrequency.None}"
                                               },
                                               new SelectListItem
                                               {
                                                   Text = ReportFrequency.Weekly.ToDescription(),
                                                   Value = $"{ReportFrequency.Weekly}"
                                               },
                                               new SelectListItem
                                               {
                                                   Text = ReportFrequency.BiWeekly.ToDescription(),
                                                   Value = $"{ReportFrequency.BiWeekly}"
                                               },
                                               new SelectListItem
                                               {
                                                   Text = ReportFrequency.Monthly.ToDescription(),
                                                   Value = $"{ReportFrequency.Monthly}"
                                               }
                                           }, "Value", "Text", (int)Frequency));
    }
}