#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using R2V2.Core;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public interface IAdminResource : IDebugInfo
    {
        int Id { get; set; }

        [Display(Name = "R2 Release Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "N/A",
            ApplyFormatInEditMode = true)]
        DateTime? ReleaseDate { get; set; }

        [Required]
        [Display(Name = "Resource Title: ")]
        string Title { get; set; }

        [Required]
        [Display(Name = "Primary Author: ")]
        string Authors { get; set; }

        int? PublicationDateYear { get; set; }

        [Display(Name = "Publication Date: ")]
        [DisplayFormat(DataFormatString = "{0:yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "N/A",
            ApplyFormatInEditMode = true)]
        DateTime? PublicationDate { get; set; }

        //[Display(Name = "Former Brandon Hill: ")]
        //short BrandonHillStatus { get; set; }

        //[Display(Name = "DCT Status: ")]
        //int DctStatusId { get; set; }

        [Display(Name = "Doody Review: ")] short DoodyReview { get; set; }

        int StatusId { get; set; }
        string Status { get; }

        [Required]
        [Display(Name = "Concurrent User Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        decimal ListPrice { get; set; }

        bool IsForSale { get; set; }

        [Display(Name = "Due Date: ")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "N/A")]
        string DueDate { get; set; }

        [Display(Name = "Publisher: ")] string PublisherName { get; set; }

        [Display(Name = "Publisher: ")] int PublisherId { get; set; }

        string ImageFileName { get; set; }
        string Isbn { get; set; }

        [Display(Name = "Discipline: ")] IEnumerable<ISpecialty> Specialties { get; set; }

        [Display(Name = "Practice Area: ")] IEnumerable<IPracticeArea> PracticeAreas { get; set; }

        [Required]
        [Display(Name = "Description: ")]
        string ResourceDescription { get; set; }

        [Display(Name = "Edition: ")] string Edition { get; set; }

        [Display(Name = "ISBN 10: ")] string Isbn10 { get; set; }

        [Display(Name = "ISBN 13: ")] string Isbn13 { get; set; }

        [Display(Name = "eISBN: ")] string EIsbn { get; set; }

        [Display(Name = "Secondary Contributors: ")]
        string AdditionalContributors { get; set; }

        [Display(Name = "Subtitle: ")] string Subtitle { get; set; }

        [Display(Name = "Drug Monograph: ")] short IsDrugMonograph { get; set; }

        [Display(Name = "NLM Call Number: ")] string NlmCall { get; set; }

        string ImageUrl { get; set; }

        [Display(Name = "Featured Title: ")] bool IsFeaturedTitle { get; set; }

        int FeaturedTitleId { get; set; }

        [Display(Name = "Start Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        DateTime? FeaturedTitleStartDate { get; set; }

        [Display(Name = "End Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        DateTime? FeaturedTitleEndDate { get; set; }

        [Display(Name = "Last Promotion Date: ")]
        [DisplayFormat(DataFormatString = "{0:G}", ApplyFormatInEditMode = true)]
        DateTime? LastPromotionDate { get; set; }

        [Display(Name = "QA Approval Date: ")]
        [DisplayFormat(DataFormatString = "{0:G}", ApplyFormatInEditMode = true)]
        DateTime? QaApprovalDate { get; set; }

        [Display(Name = "QA Approval: ")] bool QaApproval { get; set; }

        [Display(Name = "Page Count: ")] string PageCount { get; set; }

        [Display(Name = @"Primary Author Affiliation: ")]
        string Affiliation { get; set; }

        string SpecialText { get; set; }
        string SpecialIconName { get; set; }

        string FeaturedTitleStatus();
        string SpecialIconText();

        bool IsBrandonHill();
        bool IsDct();
        bool IsDctEssential();
    }
}