#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PublisherConsolidation
{
    public class Publisher : AdminBaseModel
    {
        [Display(Name = "Name:")]
        [Required(ErrorMessage = "Name is Required for Purchase by Publisher.")]
        [StringLength(255, ErrorMessage = "Name has a max length of 255 characters.")]
        public string Name { get; set; }

        [Display(Name = "Vendor Number:")]
        [StringLength(50, ErrorMessage = "Vendor Number has a max length of 50 characters.")]
        public string VendorNumber { get; set; }

        [Display(Name = "City:")]
        [Required(ErrorMessage = "City is Required for citations.")]
        [StringLength(100, ErrorMessage = "City has a max length of 100 characters.")]
        public string City { get; set; }

        [Display(Name = "State:")]
        [Required(ErrorMessage = "State is Required for citations.")]
        [StringLength(3, ErrorMessage = "State has a max length of 3 characters.")]
        public string State { get; set; }

        public string CityAndState { get; set; }

        public string RecordStatus { get; set; }

        [Display(Name = "Resource Count:")] public int ResourceCount { get; set; }

        [Display(Name = "Featured Publisher:")]
        public bool IsFeaturedPublisher { get; set; }

        [StringLength(100, ErrorMessage = "Image File Name has a max length of 100 characters.")]
        public string ImageFileName { get; set; }

        public string ImageUrl { get; set; }

        [Display(Name = "Display Name:")]
        [StringLength(100, ErrorMessage = "Display Name has a max length of 100 characters.")]
        public string DisplayName
        {
            get => _displayName ?? Name;
            set => _displayName = value;
        }

        private string _displayName { get; set; }

        [AllowHtml]
        [Display(Name = "Description:")]
        public string Description { get; set; }

        [Display(Name = "Product Statement:")]
        [StringLength(2000, ErrorMessage = "Product Statement has a max length of 2000 characters.")]
        public string ProductStatement { get; set; }

        public Publisher ConsolidatedPublisher { get; set; }

        public bool DisplayPublisherDelete { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? NotSaleableDate { get; set; }
    }
}