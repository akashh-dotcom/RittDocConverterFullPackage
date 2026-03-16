#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using R2V2.Core.Authentication;
using R2V2.Web.Models.Profile;

#endregion

namespace R2V2.Web.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DepartmentValidationAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var userEdit = validationContext.ObjectInstance as UserEdit;

            if (userEdit != null)
            {
                return null;
            }

            var adminUserEdit = validationContext.ObjectInstance as Areas.Admin.Models.User.UserEdit;
            if (adminUserEdit != null)
            {
                //Role will be null for IAs and people who edit there own profile. 
                if (adminUserEdit.Role == null)
                {
                    return null;
                }

                if (adminUserEdit.Role.Code != RoleCode.ExpertReviewer)
                {
                    return null;
                }

                if (adminUserEdit.Department.Id > 0 || !string.IsNullOrEmpty(adminUserEdit.CustomDepartment))
                {
                    return null;
                }
            }

            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, ""));
        }
    }
}