#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class Address
    {
        [Display(Name = "Address 1:")]
        [Required]
        public string Address1 { get; set; }

        [Display(Name = "Address 2:")] public string Address2 { get; set; }

        [Display(Name = "City:")] [Required] public string City { get; set; }

        [Display(Name = "State:")]
        //[Required]
        public string State { get; set; }

        [Display(Name = "Zip:")]
        //[Required]
        public string Zip { get; set; }
    }
}