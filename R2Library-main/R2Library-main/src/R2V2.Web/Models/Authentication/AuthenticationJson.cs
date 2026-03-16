namespace R2V2.Web.Models.Authentication
{
    public class AuthenticationJson : JsonResponse
    {
        public string UserName { get; set; }
        public string InstitutionHomePage { get; set; }

        public bool DisplayAlert { get; set; }
    }
}