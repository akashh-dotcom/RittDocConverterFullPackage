#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionCrawlerBypass
{
    public class InstitutionCrawlerBypassModel
    {
        public InstitutionCrawlerBypassModel()
        {
        }

        public InstitutionCrawlerBypassModel(Core.Institution.InstitutionCrawlerBypass institutionWebCrawler)
        {
            Id = institutionWebCrawler.Id;
            OctetA = institutionWebCrawler.OctetA;
            OctetB = institutionWebCrawler.OctetB;
            OctetC = institutionWebCrawler.OctetC;
            OctetD = institutionWebCrawler.OctetD;
            UserAgent = institutionWebCrawler.UserAgent;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "The First Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetA { get; set; }

        [Required(ErrorMessage = "The Second Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetB { get; set; }

        [Required(ErrorMessage = "The Third Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetC { get; set; }

        [Required(ErrorMessage = "The Fourth Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetD { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "The useragent cannot be null and not longer than 255 characters.")]
        public string UserAgent { get; set; }
    }
}