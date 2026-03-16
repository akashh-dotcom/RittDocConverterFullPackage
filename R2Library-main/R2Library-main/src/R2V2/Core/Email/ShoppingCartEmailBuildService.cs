#region

using System;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class ShoppingCartEmailBuildService : EmailBuildBaseService
    {
        private readonly ILog<EmailBuildBaseService> _log;
        int _licenseCount;
        StringBuilder _productBuilder = new StringBuilder();

        StringBuilder _resourceBuilder = new StringBuilder();
        decimal _totalDiscountPrice;
        decimal _totalListPrice;
        decimal _totalProductPrice;

        public ShoppingCartEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            _log = log;
            SetTemplates(ShoppingCartBodyTemplate, ShoppingCartResourceItemTemplate, true,
                ShoppingCartProductItemTemplate);
        }

        public EmailMessage BuildShoppingCartEmail(User user, bool displayInstitutionDiscount)
        {
            var messageBody = GetShoppingCartEmailHtml(user, displayInstitutionDiscount);

            return BuildEmailMessage(user, "R2 Library Shopping Cart Report", messageBody);
        }

        private string GetShoppingCartEmailHtml(User user, bool displayInstitutionDiscount)
        {
            //Add the products after the resources are added
            if (_productBuilder.Length > 0)
            {
                _resourceBuilder.Append(_productBuilder);
            }

            var bodyBuilder = BuildBodyHtml("{Resource_Body}", _resourceBuilder.ToString())
                .Replace("{TotalLicenses}", _licenseCount.ToString())
                .Replace("{TotalRetailPrice}", $"{_totalListPrice + _totalProductPrice:C}")
                .Replace("{InstitutionDiscount}",
                    displayInstitutionDiscount ? $"{user.Institution.Discount}%" : "Variable (specials applied)")
                .Replace("{DiscountTotal}", $"{_totalListPrice - _totalDiscountPrice:C}")
                .Replace("{TotalDiscountPrice}", $"{_totalDiscountPrice + _totalProductPrice:C}")
                .Replace("{CartUrl}", GetCartLink(user.InstitutionId.GetValueOrDefault()));

            var mainBuilder = BuildMainHtml("Shopping Cart Report", bodyBuilder, user);
            return mainBuilder;
        }

        public bool ShouldEmailBeProcessed()
        {
            return (_resourceBuilder.Length > 0 || _productBuilder.Length > 0) && _totalListPrice > 0;
        }

        public void ClearParameters()
        {
            _licenseCount = 0;
            _totalListPrice = 0;
            _totalDiscountPrice = 0;
            _totalProductPrice = 0;

            _resourceBuilder = new StringBuilder();
            _productBuilder = new StringBuilder();
        }

        public void BuildSpecialtyHeader(CartItem cartItem, Resource.Resource resource, ISpecialty specialty,
            string accountNumber)
        {
            _resourceBuilder.Append(GetTemplateFromFile("Resource_Specialty.html")
                .Replace("{Specialty_Header}", specialty.Name));
            BuildItemHtml(cartItem, resource, null, accountNumber);
        }

        public void BuildItemHtml(CartItem cartItem, IResource resource, string specialIconBaseUrl,
            string accountNumber)
        {
            if (resource == null)
            {
                //!cartItem.ResourceId.HasValue is needed because a resource could be deleted but the cart item still exists and the logic was treating it like a Product.
                if ((cartItem.Include || !cartItem.Product.Optional) && !cartItem.ResourceId.HasValue)
                {
                    _productBuilder.Append(SubItemTemplate
                        .Replace("{Product_Name}", cartItem.Product.Name)
                        .Replace("{Product_Price}", $"{cartItem.Product.Price:C}")
                    );
                    _totalProductPrice = _totalProductPrice + cartItem.Product.Price;
                }

                return;
            }

            if (resource.StatusId == (int)ResourceStatus.Archived)
            {
                _log.Info("The Resource is Archived and cannot be purchased. ");
                return;
            }

            Uri specialIconUrl = null;
            if (cartItem.SpecialIconName != null)
            {
                var test = new Uri(specialIconBaseUrl);
                specialIconUrl = new Uri(test, cartItem.SpecialIconName);
            }

            _resourceBuilder.Append(ItemTemplate
                .Replace("{Resource_StatusLower}", GetResourceStatus(resource).ToLower())
                .Replace("{Resource_Status}", GetResourceStatus(resource))
                .Replace("{Resource_Url}", GetResourceLink(resource.Isbn, accountNumber))
                .Replace("{Resource_Title}", resource.Title)
                .Replace("{Resource_Author}", resource.Authors)
                .Replace("{Resource_PracticeArea}", PopulateField("Practice Area: ", resource.PracticeAreasToString()))
                .Replace("{Resource_Publisher}", PopulateField("Publisher: ", resource.Publisher.Name))
                .Replace("{Resource_Specialties}", PopulateField("Discipline: ", resource.SpecialtiesToString()))
                .Replace("{Resource_PublicationYear}",
                    PopulateField("Publication Date: ", resource.PublicationDate.GetValueOrDefault().Year.ToString()))
                .Replace("{Resource_ISBN10}", PopulateField("ISBN 10: ", resource.Isbn10))
                .Replace("{Resource_ReleaseDate}",
                    PopulateField("R2 Release Date: ", resource.ReleaseDate.GetValueOrDefault().ToShortDateString()))
                .Replace("{Resource_ISBN13}", PopulateField("ISBN 13: ", resource.Isbn13))
                .Replace("{Resource_EISBN}", PopulateField("EISBN: ", resource.EIsbn))
                .Replace("{Resource_Edition}", PopulateField("Edition: ", resource.Edition))
                .Replace("{Resource_ImageUrl}", GetResourceImageUrl(resource.ImageFileName))
                .Replace("{Resource_License}", cartItem.GetLicenseString(resource))
                .Replace("{Resource_RetailPrice}", resource.ListPriceString())
                .Replace("{Resource_DiscountPrice}", resource.DiscountPriceString(cartItem.DiscountPrice))
                .Replace("{Resource_Icons}",
                    GetResourceIcons(resource, specialIconUrl != null ? specialIconUrl.ToString() : null))
                .Replace("{Resource_Special_Text}",
                    string.IsNullOrWhiteSpace(cartItem.SpecialText) && cartItem.AddedByNewEditionDate != null
                        ? $"Automatically added via New Edition Notification on {cartItem.AddedByNewEditionDate.Value.ToShortDateString()}"
                        : cartItem.SpecialText)
            );
            _licenseCount += cartItem.GetLicenseCount(resource);
            if (cartItem.IsBundle)
            {
                _totalListPrice += cartItem.ListPrice;
                _totalDiscountPrice += cartItem.DiscountPrice;
            }
            else
            {
                _totalListPrice += cartItem.ListPrice * cartItem.NumberOfLicenses;
                _totalDiscountPrice += cartItem.DiscountPrice * cartItem.NumberOfLicenses;
            }
        }

        private static string GetResourceStatus(IResource resource)
        {
            switch ((ResourceStatus)resource.StatusId)
            {
                case ResourceStatus.Active:
                    return "Active";
                case ResourceStatus.Archived:
                    return "Archived";
                case ResourceStatus.Forthcoming:
                    return "Pre-Order";
                case ResourceStatus.Inactive:
                    return "Not Available";
                default:
                    return "";
            }
        }
    }
}