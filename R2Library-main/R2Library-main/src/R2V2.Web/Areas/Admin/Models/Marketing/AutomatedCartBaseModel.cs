#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class AutomatedCartBaseModel : AdminBaseModel
    {
        public WebAutomatedCartReportQuery ReportQuery { get; set; }

        [Display(Name = @"Turnaway")] public bool IncludeTurnaway => ReportQuery.IncludeTurnaway;

        [Display(Name = @"New Edition")] public bool IncludeNewEdition => ReportQuery.IncludeNewEdition;

        [Display(Name = @"Expert Reviewed")] public bool IncludeReviewed => ReportQuery.IncludeReviewed;

        [Display(Name = @"Triggered PDA")] public bool IncludeTriggeredPda => ReportQuery.IncludeTriggeredPda;

        [Display(Name = @"Requested")] public bool IncludeRequested => ReportQuery.IncludeRequested;
    }
}