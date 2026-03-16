namespace R2V2.Web.Models
{
    public class JsonResponse
    {
        public int? Id { get; set; }

        public string Status { get; set; }

        // new, standard properties
        public bool Successful { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorMessage { get; set; }
        public string InformationMessage { get; set; }

        public bool Timeout { get; set; }
    }
}