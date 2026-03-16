namespace R2V2.Web.Models
{
    public class AthensModel : BaseModel
    {
        public int InstitutionId { get; set; }

        public string AthensAffiliation { get; set; }
        public string AthensTargetedId { get; set; }

        public string ErrorMessage { get; set; }

        public LoginParam LoginInfo { get; set; }
        public string RedirectUrl { get; set; }
    }
}