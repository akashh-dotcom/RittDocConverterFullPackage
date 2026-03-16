#region

using System;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class ForgotPasswordParam : LoginParam
    {
        public new string Password { get; set; }
    }
}