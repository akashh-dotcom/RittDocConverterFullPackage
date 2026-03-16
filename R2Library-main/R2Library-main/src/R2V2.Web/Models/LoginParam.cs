#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class LoginParam
    {
        [Required]
        //[RegularExpression("([a-zA-Z0-9 @]+)", ErrorMessage = "@ is the only special character allowed.")]
        public string UserName { get; set; }

        [Required]
        //[RegularExpression("([a-zA-Z0-9 @]+)", ErrorMessage = "@ is the only special character allowed.")]
        public string Password { get; set; }

        public string RedirectUrl { get; set; }

        //Used to link Athens Account
        public string AthensTargetedId { get; set; }
    }
}