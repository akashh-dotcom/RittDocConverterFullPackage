#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class PdaEmailBuildService : EmailBuildBaseService
    {
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailBuildBaseService> _log;

        public PdaEmailBuildService(ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            _log = log;
            _emailSettings = emailSettings;
        }

        public void InitEmailTemplatesAddToCart()
        {
            SetTemplates(PdaAddedToCartBodyTemplate, PdaAddedToCartItemTemplate);
        }

        public void InitEmailTemplatesForPdaHistory()
        {
            SetTemplates(PdaHistoryBodyTemplate);
        }

        public void InitEmailTemplatesForRemovedFromCart()
        {
            SetTemplates(PdaRemovedFromCartBodyTemplate, PdaRemovedFromCartItemTemplate);
        }

        public EmailMessage BuildPdaAddToCartEmail(string itemHtml, User user, string[] ccUserArray)
        {
            var messageBody = GetPdaAddToCartEmailHtml(itemHtml, user);

            return BuildEmailMessage(user, ccUserArray, "R2 Library PDA Title Added To Cart", messageBody);
        }

        public string GetPdaAddToCartEmailHtml(string itemHtml, User user)
        {
            var bodyBuilder = BuildBodyHtml("{Resource_Items}", itemHtml)
                .Replace("{CartUrl}", GetCartLink(user.InstitutionId.GetValueOrDefault()));

            var mainBuilder = BuildMainHtml("PDA Title Added To Cart", bodyBuilder, user);

            return mainBuilder;
        }

        public string BuildPdaAddToCartItemTemplate(IResource resource, decimal institutionDiscount, DateTime addedDate,
            DateTime addedToCartDate, string pdaPromotionText, decimal discountPrice, string accountNumber)
        {
            var item = BuildCartItemTemplate(ItemTemplate, resource, addedDate, addedToCartDate, accountNumber,
                pdaPromotionText, resource.ListPriceString(true),
                resource.DiscountPriceString(discountPrice != 0
                    ? discountPrice
                    : resource.ListPrice - institutionDiscount / 100 * resource.ListPrice));
            return item;
        }

        public EmailMessage BuildPdaHistoryEmail(PdaHistoryReport pdaHistoryReport, User user, string[] ccUserArray)
        {
            var excelExport = new PdaHistoryExcelExport(pdaHistoryReport, user.Institution.ProxyPrefix,
                user.Institution.UrlSuffix,
                $"{GetWebSiteBaseUrl()}Resource/Title");

            var contentType = new ContentType
            {
                Name = $"R2_PdaHistoryReport_{DateTime.Now.ToShortDateString()}.xlsx"
            };

            var attachment = new Attachment(excelExport.Export(), contentType)
                { ContentType = { MediaType = excelExport.MimeType } };

            var messageBody = GetPdaHistoryEmail(user);

            return BuildEmailMessage(user.Email, ccUserArray, "R2 Library PDA History Report", messageBody, attachment);
        }

        private string GetPdaHistoryEmail(User user)
        {
            var bodyBuilder = BuildBodyHtml();

            var mainBuilder = BuildMainHtml("PDA History Report", bodyBuilder, user);

            return mainBuilder;
        }


        public EmailMessage BuildPdaRemovedFromCartEmail(string itemHtml, User user, string[] ccUserArray)
        {
            var messageBody = GetPdaRemovedFromCartEmailHtml(itemHtml, user);

            return BuildEmailMessage(user, ccUserArray, "R2 Library PDA Title Removed from Cart", messageBody);
        }

        public string GetPdaRemovedFromCartEmailHtml(string itemHtml, User user)
        {
            var bodyBuilder = BuildBodyHtml("{Resource_Items}", itemHtml)
                .Replace("{PurchaseEbook_Url}", GetPurchaseBooksLink(user.InstitutionId.GetValueOrDefault()))
                .Replace("{PdaHistory_Url}", GetPdaHistoryLink(user.InstitutionId.GetValueOrDefault()))
                .Replace("{Resource_Items}", itemHtml);

            var mainBuilder = BuildMainHtml("PDA Title Removed from Cart", bodyBuilder, user);

            return mainBuilder;
        }

        public string BuildPdaRemovedFromCartItemTemplate(IResource resource, decimal institutionDiscount,
            DateTime addedDate, DateTime addedToCartDate, string accountNumber, string pdaPromotionText)
        {
            var item = BuildCartItemTemplate(ItemTemplate, resource, addedDate, addedToCartDate, accountNumber,
                pdaPromotionText, resource.ListPriceString(true),
                resource.DiscountPriceString(resource.ListPrice - institutionDiscount / 100 * resource.ListPrice));
            return item;
        }

        private string BuildCartItemTemplate(string itemTemplate, IResource resource, DateTime addedDate,
            DateTime addedToCartDate, string accountNumber, string pdaPromotionText, string listPrice,
            string discountPrice)
        {
            var item = itemTemplate
                .Replace("{Resource_Url}", GetResourceLink(resource.Isbn, accountNumber))
                .Replace("{Resource_ImageUrl}", GetResourceImageUrl(resource.ImageFileName))
                .Replace("{Resource_Title}", resource.Title)
                .Replace("{Resource_Status}", resource.StatusToString())
                .Replace("{Resource_Author}", resource.Authors)
                .Replace("{Resource_Edition}", PopulateField("Edition: ", resource.Edition))
                .Replace("{Resource_PracticeArea}", PopulateField("Practice Area: ", resource.PracticeAreasToString()))
                .Replace("{Resource_Publisher}", PopulateField("Publisher: ", resource.Publisher.Name))
                .Replace("{Resource_Specialties}", PopulateField("Discipline: ", resource.SpecialtiesToString()))
                .Replace("{Resource_PublicationYear}",
                    PopulateField("Publication Date: ", resource.PublicationDate.GetValueOrDefault().Year.ToString()))
                .Replace("{Resource_ReleaseDate}",
                    PopulateField("R2 Release Date: ", resource.ReleaseDate.GetValueOrDefault().ToShortDateString()))
                .Replace("{Resource_ISBN10}", PopulateField("ISBN 10: ", resource.Isbn10))
                .Replace("{Resource_ISBN13}", PopulateField("ISBN 13: ", resource.Isbn13))
                .Replace("{Resource_EISBN}", PopulateField("EISBN: ", resource.EIsbn))
                .Replace("{Resource_License}", "1")
                .Replace("{Resource_PdaAdded}", PopulateField("Date Added: ", addedDate.ToShortDateString()))
                .Replace("{Resource_PdaAddedToCart}",
                    PopulateField("Date Added To Cart: ", addedToCartDate.ToShortDateString()))
                .Replace("{Resource_PdaRemovedFromCart}",
                    PopulateField("Removed From Cart: ", addedToCartDate.AddDays(30).ToShortDateString()))
                .Replace("{Resource_Special_Text}", pdaPromotionText);

            item = item.Replace("{Resource_RetailPrice}", listPrice)
                .Replace("{Resource_DiscountPrice}", discountPrice);
            return item;
        }


        public EmailMessage BuildTrialEndedPdaCreatedEmail(User user)
        {
            try
            {
                var mainTemplate = GetTemplateFromFile(MainHeaderFooterTemplate);
                var bodyTemplate = GetTemplateFromFile(TrialEndedPdaCreatedTemplate);

                var messageBody = bodyTemplate;

                messageBody = mainTemplate.Replace("{Body}", messageBody);

                messageBody = messageBody.Replace("{Title}", "Your R2 Digital Library PDA Account is Activated.")
                    .Replace("{Year}", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture))
                    .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                    .Replace("{User_Email}", user.Email)
                    .Replace("{Institution_Name}", user.Institution.Name)
                    .Replace("{Institution_Number}", user.Institution.AccountNumber);

                var emailMessage = new EmailMessage
                {
                    Subject =
                        $"{(_emailSettings.AddEnvironmentPrefixToSubject ? $"{Environment.MachineName} - " : "")}Trial Account Cancelled, PDA Collection Created",
                    FromDisplayName = _emailSettings.DefaultFromName,
                    FromAddress = _emailSettings.DefaultFromAddress,
                    ReplyToAddress = _emailSettings.DefaultReplyToAddress,
                    ReplyToDisplayName = _emailSettings.DefaultReplyToName,
                    IsHtml = true,
                    Body = messageBody
                };

                if (!emailMessage.AddToRecipient(user.Email))
                {
                    _log.ErrorFormat("invalid TO email address <{0}>", user.Email);
                }

                if (!string.IsNullOrWhiteSpace(_emailSettings.PdaAddToCartCcEmailAddresses))
                {
                    emailMessage.AddCcRecipients(_emailSettings.PdaAddToCartCcEmailAddresses, ';');
                }

                // log invalid email addresses
                foreach (var invalidEmailAddress in emailMessage.InvalidEmailAddresses)
                {
                    _log.ErrorFormat("invalid email address <{0}>", invalidEmailAddress);
                }

                LogMessage(emailMessage);
                return emailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        public EmailMessage BuildOngoingPdaResourceAddedEmail(User user,
            Dictionary<PdaRule, IList<InstitutionResourceLicense>> rulesWithNewLicenses
            , AdminInstitution adminInstitution, DateTime transactionTimestamp, IList<IResource> resources)
        {
            try
            {
                SetTemplates(OngoingPdaAddedBodyTemplate, OngoingPdaAddedItemTemplate);
                var pdaRuleTemplate = GetTemplateFromFile(OngoingPdaAddedRuleTemplate);

                var rulesWithItems = new StringBuilder();

                var resourceCount = 0;

                foreach (var rulesWithNewLicense in rulesWithNewLicenses)
                {
                    var pdaRule = rulesWithNewLicense.Key;
                    var institutionResourceLicenses = rulesWithNewLicense.Value;

                    var items = new StringBuilder();
                    foreach (var institutionResourceLicense in institutionResourceLicenses)
                    {
                        var resource = resources.FirstOrDefault(x => x.Id == institutionResourceLicense.ResourceId);
                        if (resource == null)
                        {
                            _log.ErrorFormat(
                                "PDA license added for resource id which was not in promotion batch! institutionResourceLicense: {0}",
                                institutionResourceLicense.ToDebugString());
                            break;
                        }

                        var item = BuildCartItemTemplate(ItemTemplate, resource, transactionTimestamp,
                            DateTime.MinValue, adminInstitution.AccountNumber,
                            null, resource.ListPriceString(true),
                            resource.DiscountPriceString(resource.ListPrice -
                                                         adminInstitution.Discount / 100 * resource.ListPrice));
                        items.AppendLine(item);
                        resourceCount++;
                    }

                    var ruleHeader =
                        $"The &#34{pdaRule.Name}&#34 task added {(resources.Count > 1 ? "these resources" : "this resource")} on {transactionTimestamp:MM/dd/yyyy}.";

                    rulesWithItems.AppendLine(pdaRuleTemplate.Replace("{PdaRuleHeader}", ruleHeader)
                        .Replace("{OngoingPdaAddedItems}", items.ToString()));
                }

                var body = BodyTemplate
                    .Replace("{PdaRuleActionMessagePrefix}",
                        resourceCount == 1 ? "A title has" : $"{resourceCount} titles have")
                    .Replace("{PdaRuleRunDate}", DateTime.Now.ToLongDateString())
                    .Replace("{PdaCollectionLink}",
                        $"{_emailSettings.WebSiteBaseUrl}Admin/Pda/PdaProfile/{adminInstitution.Id}")
                    .Replace("{OngoingPdaAddedItemsByRule}", rulesWithItems.ToString());

                var subject = $"{(resourceCount == 1 ? "Title" : "Titles")} Added to Your PDA Collection";

                var messageHtml = BuildMainHtml(subject, body, user);

                var emailMessage = new EmailMessage
                {
                    Subject =
                        $"{(_emailSettings.AddEnvironmentPrefixToSubject ? $"{Environment.MachineName} - " : "")}Title Added to Your PDA Collection",
                    FromDisplayName = _emailSettings.DefaultFromName,
                    FromAddress = _emailSettings.DefaultFromAddress,
                    ReplyToAddress = _emailSettings.DefaultReplyToAddress,
                    ReplyToDisplayName = _emailSettings.DefaultReplyToName,
                    IsHtml = true,
                    Body = messageHtml
                };

                if (!emailMessage.AddToRecipient(user.Email))
                {
                    _log.ErrorFormat("invalid TO email address <{0}>", user.Email);
                }

                if (!string.IsNullOrWhiteSpace(_emailSettings.PdaAddToCartCcEmailAddresses))
                {
                    emailMessage.AddCcRecipients(_emailSettings.PdaAddToCartCcEmailAddresses, ';');
                }

                // log invalid email addresses
                foreach (var invalidEmailAddress in emailMessage.InvalidEmailAddresses)
                {
                    _log.ErrorFormat("invalid email address <{0}>", invalidEmailAddress);
                }

                LogMessage(emailMessage);
                return emailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }
    }
}