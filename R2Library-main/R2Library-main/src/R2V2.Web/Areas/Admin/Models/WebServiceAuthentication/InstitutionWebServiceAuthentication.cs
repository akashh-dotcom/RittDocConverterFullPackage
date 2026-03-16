#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.WebServiceAuthentication
{
    public class InstitutionWebServiceAuthentication
    {
        public InstitutionWebServiceAuthentication()
        {
        }

        public InstitutionWebServiceAuthentication(Core.Institution.WebServiceAuthentication dbWebServiceAuthentication)
        {
            Id = dbWebServiceAuthentication.Id;
            OctetA = dbWebServiceAuthentication.OctetA;
            OctetB = dbWebServiceAuthentication.OctetB;
            OctetC = dbWebServiceAuthentication.OctetC;
            OctetD = dbWebServiceAuthentication.OctetD;
            AuthenticationKey = dbWebServiceAuthentication.AuthenticationKey;
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
        [StringLength(24, MinimumLength = 24,
            ErrorMessage = "The Authentication Key has a required length of 24 characters.")]
        public string AuthenticationKey { get; set; }
    }
}