#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using R2V2.Web.Areas.Admin.Models.SubscriptionManagement;
using R2V2.Web.Models.Profile;

#endregion

namespace R2V2.Web.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PasswordCompareAttribute : ValidationAttribute, IClientValidatable
    {
        public PasswordCompareAttribute(object newPassword, object currentPassword)
        {
            NewPassword = newPassword;
            CurrentPassword = currentPassword;
        }

        private object NewPassword { get; }
        private object CurrentPassword { get; }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
            ControllerContext context)
        {
            yield return new ModelClientValidationPasswordCompareRule(ErrorMessage, NewPassword, CurrentPassword);
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, NewPassword);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // todo:  clean this up/make more genenric - JAH
            var userEdit = validationContext.ObjectInstance as UserEdit;
            if (userEdit != null && userEdit.Id != 0)
            {
                if (string.IsNullOrWhiteSpace(userEdit.CurrentPassword) &&
                    string.IsNullOrWhiteSpace(userEdit.NewPassword) &&
                    string.IsNullOrWhiteSpace(userEdit.ConfirmPassword))
                    return null;

                if (!string.IsNullOrWhiteSpace(userEdit.CurrentPassword) &&
                    userEdit.NewPassword == userEdit.ConfirmPassword)
                    return null;
            }
            else if (userEdit != null)
            {
                if (string.IsNullOrWhiteSpace(userEdit.CurrentPassword) &&
                    userEdit.NewPassword == userEdit.ConfirmPassword)
                    return null;
            }
            else
            {
                var adminUserEdit = validationContext.ObjectInstance as Areas.Admin.Models.User.UserEdit;
                if (adminUserEdit != null)
                {
                    if (string.IsNullOrWhiteSpace(adminUserEdit.NewPassword) &&
                        string.IsNullOrWhiteSpace(adminUserEdit.ConfirmPassword))
                        return null;

                    if (adminUserEdit.NewPassword == adminUserEdit.ConfirmPassword)
                        return null;
                }

                var subscriptionUser = validationContext.ObjectInstance as SubscriptionUser;
                if (subscriptionUser != null)
                {
                    if (string.IsNullOrWhiteSpace(subscriptionUser.NewPassword) &&
                        string.IsNullOrWhiteSpace(subscriptionUser.ConfirmPassword))
                        return null;

                    if (subscriptionUser.NewPassword == subscriptionUser.ConfirmPassword)
                        return null;
                }
            }

            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, ""));
        }

        public class ModelClientValidationPasswordCompareRule : ModelClientValidationEqualToRule
        {
            public ModelClientValidationPasswordCompareRule(string errorMessage, object newPassword,
                object currentPassword)
                : base(errorMessage, newPassword)
            {
                ErrorMessage = errorMessage;
                ValidationType = "passwordcompare";
                ValidationParameters["newpassword"] = newPassword;
                ValidationParameters["currentpassword"] = currentPassword;
            }
        }
    }
}