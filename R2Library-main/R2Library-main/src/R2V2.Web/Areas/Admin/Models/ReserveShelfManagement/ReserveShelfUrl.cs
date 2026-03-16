#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserveShelfUrl
    {
        public ReserveShelfUrl()
        {
        }

        public ReserveShelfUrl(int reserveShelfUrlId, int reserveShelfId, string url, string description)
        {
            ReserveShelfUrlId = reserveShelfUrlId;
            ReserveShelfId = reserveShelfId;
            Url = url;
            Description = description;
        }

        public int ReserveShelfUrlId { get; set; }

        public int ReserveShelfId { get; set; }

        [Required]
        [Url]
        [Display(Name = "Url:")]
        [StringLength(255, ErrorMessage = "Url Max Length is 255 characters")]
        public string Url { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Description Max Length is 255 characters")]
        [Display(Name = "Description:")]
        public string Description { get; set; }
    }
}