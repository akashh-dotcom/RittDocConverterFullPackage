#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PasswordValidationAttribute : ValidationAttribute, IClientValidatable
    {
        public PasswordValidationAttribute(object newPassword)
        {
            NewPassword = newPassword;
        }

        public PasswordValidationAttribute()
        {
        }

        private object NewPassword { get; }


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
            ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "passwordvalid"
            };

            rule.ValidationParameters.Add("newpassword", NewPassword);

            yield return rule;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var password = value.ToString();
                if (password.Length >= 8 && password.Length <= 20)
                {
                    var upperRegex = new Regex(@"[A-Z]");
                    var lowerRegex = new Regex(@"[a-z]");
                    var numberRegex = new Regex(@"[0-9]");
                    var specialRegex = new Regex(@"[`~!@#$%^&*()<>?:,./\;'|-]");

                    var isValid = 0;
                    isValid += upperRegex.IsMatch(value.ToString()) ? 1 : 0;
                    isValid += lowerRegex.IsMatch(value.ToString()) ? 1 : 0;
                    isValid += numberRegex.IsMatch(value.ToString()) ? 1 : 0;
                    isValid += specialRegex.IsMatch(value.ToString()) ? 1 : 0;

                    return isValid >= 3 ? null : new ValidationResult(string.Format(CultureInfo.CurrentCulture, ""));
                }
            }

            return null;
        }

        public override string FormatErrorMessage(string name)
        {
            return Resources.PasswordNotValid;
        }
    }
}