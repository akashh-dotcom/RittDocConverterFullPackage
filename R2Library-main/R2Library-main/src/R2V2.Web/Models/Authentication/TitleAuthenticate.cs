#region

using System.ComponentModel.DataAnnotations;
using System.Web.Routing;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Models.Authentication
{
    public class TitleAuthenticate : BaseModel
    {
        public TitleAuthenticate(IResource resource, RouteValueDictionary publicRoute)
        {
            Title = resource.Title;
            Isbn = resource.Isbn;
            Isbn10 = resource.Isbn10;
            Isbn13 = resource.Isbn13;
            PublicRoute = publicRoute;
        }

        public TitleAuthenticate(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string Title { get; set; }
        public string Isbn { get; set; }

        [Display(Name = "ISBN 13:")] public string Isbn13 { get; set; }

        [Display(Name = "ISBN 10:")] public string Isbn10 { get; set; }

        public RouteValueDictionary PublicRoute { get; set; }

        public string ErrorMessage { get; set; }
    }
}