#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Models.Trial
{
    public class AccountVerifyModel : BaseModel
    {
        public string ErrorMessage { get; set; }
        [Required] public string UserName { get; set; }
        [Required] public string Password { get; set; }
    }
}