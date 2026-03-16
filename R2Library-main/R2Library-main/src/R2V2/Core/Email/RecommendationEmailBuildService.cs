#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class RecommendationEmailBuildService : EmailBuildBaseService
    {
        private readonly IEmailSettings _emailSettings;

        private readonly ILog<EmailBuildBaseService> _log;
        private readonly IQueryable<Resource.Resource> _resources;
        private readonly ResourceService _resourceService;
        private string _reviewEmailBodyTemplate;

        private string _reviewEmailItems;
        private string _reviewEmailItemTemplate;
        private string _reviewEmailMainTemplate;

        public RecommendationEmailBuildService(ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
            , IQueryable<Resource.Resource> resources
            , ResourceService resourceService
        )
            : base(log, emailSettings, contentSettings)
        {
            _log = log;
            _emailSettings = emailSettings;
            _resources = resources;
            _resourceService = resourceService;
        }

        public void InitEmailTemplates()
        {
            SetTemplates(RecommendationBodyTemplate, RecommendationItemTemplate);
        }

        /// <summary>
        ///     Returns the html for the recommendations supplied
        /// </summary>
        private string BuildRecommendationItems(IEnumerable<Recommendation> recommendations, string accountNumber)
        {
            var itemBuilder = new StringBuilder();

            foreach (var item in recommendations)
            {
                var recommendation = item;
                // todo: replace this implementation with something less database intensive
                var resource = _resources.FirstOrDefault(x => x.Id == recommendation.ResourceId);

                if (resource != null)
                {
                    itemBuilder.Append(ItemTemplate
                        .Replace("{Resource_Url}", GetResourceLink(resource.Isbn, accountNumber))
                        .Replace("{Resource_Title}", resource.Title)
                        .Replace("{Resource_Author}", resource.Authors)
                        .Replace("{Resource_PracticeArea}",
                            PopulateField("Practice Area: ", resource.PracticeAreasToString()))
                        .Replace("{Resource_Publisher}", PopulateField("Publisher: ", resource.Publisher.Name))
                        .Replace("{Resource_Specialties}",
                            PopulateField("Discipline: ", resource.SpecialtiesToString()))
                        .Replace("{Resource_PublicationYear}",
                            PopulateField("Publication Date: ",
                                resource.PublicationDate.GetValueOrDefault().Year.ToString()))
                        .Replace("{Resource_ISBN10}", PopulateField("ISBN 10: ", resource.Isbn10))
                        .Replace("{Resource_ReleaseDate}",
                            PopulateField("R2 Release Date: ",
                                resource.ReleaseDate.GetValueOrDefault().ToShortDateString()))
                        .Replace("{Resource_ISBN13}", PopulateField("ISBN 13: ", resource.Isbn13))
                        .Replace("{Resource_EISBN}", PopulateField("EISBN: ", resource.EIsbn))
                        .Replace("{Resource_Edition}", PopulateField("Edition: ", resource.Edition))
                        .Replace("{Resource_ImageUrl}", GetResourceImageUrl(resource.ImageFileName))
                        .Replace("{Resource_RetailPrice}", resource.ListPriceString())
                        .Replace("{Recommended_By}", PopulateField("Recommended By: ",
                            $"{item.RecommendedByUser.LastName}, {item.RecommendedByUser.FirstName} ({item.RecommendedByUser.UserName})"))
                        .Replace("{Recommended_Note}", PopulateField("Notes: ", item.Notes))
                    );
                }
            }

            return itemBuilder.ToString();
        }

        public EmailMessage BuildRecommendationEmail(List<Recommendation> recommendations, User user, string[] ccEmails)
        {
            var itemHtml = BuildRecommendationItems(recommendations, user.Institution.AccountNumber);

            if (string.IsNullOrWhiteSpace(itemHtml))
            {
                return null;
            }

            var messageHtml = GetRecommendationEmailHtml(itemHtml, user);

            if (string.IsNullOrWhiteSpace(messageHtml))
            {
                return null;
            }

            return BuildEmailMessage(user, ccEmails, "R2 Library Expert Reviewer User Recommendations", messageHtml);
        }

        private string GetRecommendationEmailHtml(string itemHtml, User user)
        {
            var bodyHtml = BuildBodyHtml("{Resource_Body}", itemHtml);

            var mainHtml = BuildMainHtml("Expert Reviewer User Recommendations", bodyHtml, user);

            return mainHtml;
        }

        public EmailMessage BuildReviewEmail(Review review, User user, bool rebuildItems)
        {
            try
            {
                var messageBody = GetReviewEmailHtml(review, user, rebuildItems);
                var emailMessage = new EmailMessage
                {
                    Subject =
                        $"{(_emailSettings.AddEnvironmentPrefixToSubject ? $"{Environment.MachineName} - " : "")}R2 Library Review List Notification",
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


        private string GetReviewEmailHtml(Review review, User user, bool rebuildItems)
        {
            if (rebuildItems)
            {
                _reviewEmailItems = null;
            }

            LoadReviewEmailTemplates();
            var bodyBuilder = new StringBuilder();
            var mainBuilder = new StringBuilder();

            // build items
            var reviewItems = BuildReviewItem(_reviewEmailItemTemplate, review, user.Institution.AccountNumber);

            bodyBuilder.Append(_reviewEmailBodyTemplate.Replace("{ReviewName}", review.Name)
                .Replace("{ReviewDescription}", review.Description)
                .Replace("{Resource_Body}", reviewItems));

            mainBuilder.Append(_reviewEmailMainTemplate.Replace("{Title}", "R2 Library Review List Notification")
                .Replace("{Body}", bodyBuilder.ToString())
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                .Replace("{User_Email}", user.Email)
                .Replace("{Institution_Name}", user.Institution.Name)
                .Replace("{Institution_Number}", user.Institution.AccountNumber));

            return mainBuilder.ToString();
        }

        private void LoadReviewEmailTemplates()
        {
            if (_reviewEmailMainTemplate == null)
            {
                _reviewEmailMainTemplate = GetTemplateFromFile(MainHeaderFooterTemplate);
            }

            if (_reviewEmailItemTemplate == null)
            {
                _reviewEmailItemTemplate = GetTemplateFromFile("ReviewResource_Item.html");
            }

            if (_reviewEmailBodyTemplate == null)
            {
                _reviewEmailBodyTemplate = GetTemplateFromFile("ReviewResource_Body.html");
            }
        }

        private string BuildReviewItem(string itemTemplate, Review review, string accountNumber)
        {
            if (_reviewEmailItems != null)
            {
                return _reviewEmailItems;
            }

            var itemBuilder = new StringBuilder();

            foreach (var item in review.ReviewResources)
            {
                var resource = _resourceService.GetResource(item.ResourceId);

                if (resource != null)
                {
                    itemBuilder.Append(itemTemplate
                        .Replace("{Resource_Url}", GetResourceLink(resource.Isbn, accountNumber))
                        .Replace("{Resource_Title}", resource.Title)
                        .Replace("{Resource_Author}", resource.Authors)
                        .Replace("{Resource_PracticeArea}",
                            PopulateField("Practice Area: ", resource.PracticeAreasToString()))
                        .Replace("{Resource_Publisher}", PopulateField("Publisher: ", resource.Publisher.Name))
                        .Replace("{Resource_Specialties}",
                            PopulateField("Discipline: ", resource.SpecialtiesToString()))
                        .Replace("{Resource_PublicationYear}",
                            PopulateField("Publication Date: ",
                                resource.PublicationDate.GetValueOrDefault().Year.ToString()))
                        .Replace("{Resource_ISBN10}", PopulateField("ISBN 10: ", resource.Isbn10))
                        .Replace("{Resource_ReleaseDate}",
                            PopulateField("R2 Release Date: ",
                                resource.ReleaseDate.GetValueOrDefault().ToShortDateString()))
                        .Replace("{Resource_ISBN13}", PopulateField("ISBN 13: ", resource.Isbn13))
                        .Replace("{Resource_EISBN}", PopulateField("EISBN: ", resource.EIsbn))
                        .Replace("{Resource_Edition}", PopulateField("Edition: ", resource.Edition))
                        .Replace("{Resource_ImageUrl}", GetResourceImageUrl(resource.ImageFileName))
                        .Replace("{Resource_RetailPrice}", resource.ListPriceString())
                    );
                }
            }

            _reviewEmailItems = itemBuilder.ToString();
            return _reviewEmailItems;
        }
    }
}