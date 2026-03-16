#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Helpers
{
    /// <summary>
    ///     DOES NOT make the field Required
    ///     Provides a 10 year range for date validation. Will also provide an error message if triggered.
    ///     Must pass a boolean value to provide an accurate error message.
    ///     False will state the date is not required.
    /// </summary>
    public class DateTenYearsAttribute : ValidationAttribute, IClientValidatable
    {
        public DateTenYearsAttribute(object dateToValidate)
        {
            DateToValidate = dateToValidate;
        }

        public DateTenYearsAttribute(bool test)
        {
        }

        private object DateToValidate { get; }


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
            ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "datetenyearsvalid"
            };

            rule.ValidationParameters.Add("datetovalidate", DateToValidate);

            yield return rule;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return null;
            if (value != null)
            {
                var dateString = value.ToString();
                DateTime parsedDate;
                DateTime.TryParse(dateString, out parsedDate);
                if (parsedDate > DateTime.Now.AddYears(-10) && parsedDate < DateTime.Now.AddYears(10))
                {
                    return null;
                }

                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, ""));
            }

            return null;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Please specify a valid date within 10 years. (MM/DD/YYYY)";
        }
    }
}