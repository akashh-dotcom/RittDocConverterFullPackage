namespace R2V2.Web.Models.Home
{
    public class Info : BaseModel
    {
        public string ServerName { get; set; }
        public string RequestId { get; set; }
        public string SessionId { get; set; }

        public bool IsAuthenticated { get; set; }
        public string AuthenticationMethod { get; set; }
        public string InstitutionName { get; set; }
        public string InstitutionAccountNumber { get; set; }
        public int InstitutionId { get; set; }

        public string UserDisplayName { get; set; }
        public string UserName { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserRole { get; set; }
        public int UserId { get; set; }

        public string IpAddress { get; set; }
        public string CurrentReferrer { get; set; }
        public string AuthnticationReferrer { get; set; }
        public string UserAgent { get; set; }
        public string CountryCode { get; set; }
    }
}