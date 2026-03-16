#region

using BotDetect.Web.Mvc;

#endregion

namespace R2V2.Web.Helpers
{
    public class NonGoogleCaptchaValidation : CaptchaValidationAttribute
    {
        public NonGoogleCaptchaValidation()
        {
            InputField = "CaptchaCode";
            CaptchaId = "Captcha";
            ErrorMessage = "Incorrect CAPTCHA code!";
        }
    }
}