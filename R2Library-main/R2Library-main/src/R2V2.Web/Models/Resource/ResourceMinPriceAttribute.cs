#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Areas.Admin.Models.Resource;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Models.Resource
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ResourceMinPriceAttribute : ValidationAttribute
    {
        private readonly IContentSettings _contentSettings = ServiceLocator.Current.GetInstance<IContentSettings>();
        private readonly IWebSettings _webSettings = ServiceLocator.Current.GetInstance<IWebSettings>();

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                _webSettings.ResourceMinimumPromotionPrice, _contentSettings.ResourceMinimumListPrice);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var resourceEdit = validationContext.ObjectInstance as ResourceEdit;
            if (resourceEdit == null || !resourceEdit.QaApproval)
            {
                return null;
            }

            if (resourceEdit.Resource.IsFreeResource)
            {
                return null;
            }

            return resourceEdit.Resource.ListPrice >= _webSettings.ResourceMinimumPromotionPrice
                ? null
                : new ValidationResult(string.Format(CultureInfo.CurrentCulture, ""));
        }
    }
}