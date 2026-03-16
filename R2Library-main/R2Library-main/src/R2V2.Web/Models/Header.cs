#region

using System;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class Header
    {
        public Header(bool showLoginLink, string redirectUrl)
        {
            ShowLoginLink = showLoginLink;
            LoginParam = new LoginParam { RedirectUrl = redirectUrl };
        }

        public LoginParam LoginParam { get; set; }

        public bool ShowLoginLink { get; private set; }

        public string BrandingLogoDisplayUrl { get; set; }
        public string BrandingLogoFileName { get; set; }
        public string BrandingInstitutionName { get; set; }
    }
}