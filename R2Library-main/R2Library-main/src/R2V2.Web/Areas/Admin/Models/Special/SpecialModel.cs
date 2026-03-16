#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialModel
    {
        public SpecialModel()
        {
        }

        public SpecialModel(Core.Resource.Special special)
        {
            Id = special.Id;
            Name = special.Name;
            StartDate = special.StartDate;
            EndDate = special.EndDate;
        }

        public int Id { get; set; }

        [Display(Name = "Special Name:")]
        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Start Date:")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            ApplyFormatInEditMode = true)]
        [DateTenYears("StartDate")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date:")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            ApplyFormatInEditMode = true)]
        [DateTenYears("EndDate")]
        public DateTime EndDate { get; set; }
    }
}