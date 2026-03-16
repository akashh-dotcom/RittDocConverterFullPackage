#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class WebIpRange
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The First Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetA { get; set; }

        [Required(ErrorMessage = "The Second Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetB { get; set; }

        [Required(ErrorMessage = "The Third Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetCStart { get; set; }

        [Required(ErrorMessage = "The Third Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetCEnd { get; set; }

        [Required(ErrorMessage = "The Fourth Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetDStart { get; set; }

        [Required(ErrorMessage = "The Fourth Octet of the IP address must be between 0-255")]
        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int OctetDEnd { get; set; }

        public string Description { get; set; }

        public string AccountNumber { get; set; }
        public int InstitutionId { get; set; }
    }
}