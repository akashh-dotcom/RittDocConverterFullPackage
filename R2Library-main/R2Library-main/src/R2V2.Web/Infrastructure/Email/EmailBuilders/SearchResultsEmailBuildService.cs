#region

using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Models.Search;

#endregion

namespace R2V2.Web.Infrastructure.Email.EmailBuilders
{
    public class SearchResultsEmailBuildService : ResourceEmailBuildBaseService
    {
        private readonly ILog<EmailBuildBaseService> _log;

        public SearchResultsEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            _log = log;
            SetTemplates(TurnawayBodyTemplate, TurnawayItemTemplate);
        }

        public string GetMessageBody(HttpRequestBase requestBase, SearchResultSet searchResultSet, UrlHelper urlHelper,
            AuthenticatedInstitution authenticatedInstitution, string fromAddress, string comments,
            bool isSecureConnection)
        {
            var html = new StringBuilder();
            var body = new StringBuilder();
            var items = new StringBuilder();

            SetTemplates(MainHeaderFooterTemplate, SearchResultsBodyTemplate, SearchResultsItemTemplate);

            if (requestBase.Url != null)
            {
                var urlHost = requestBase.Url.Host;
                foreach (var item in searchResultSet.SearchResults)
                {
                    items.Append(ItemTemplate
                        .Replace("{Resource_Url}", $"http://{urlHost}{item.Url}")
                        .Replace("{Resource_Title}", item.Title)
                        .Replace("{Resource_ImageUrl}", item.ImageUrl)
                        .Replace("{Resource_Authors}", item.Description)
                        .Replace("{Resource_Gist}", item.Gist)
                    );
                }

                var searchUrl = string.Format("http://{0}/{1}", urlHost,
                    searchResultSet.Query.GetSearchUrl(urlHelper.Action("Index", "Search"), urlHelper));
                _log.DebugFormat("searchUrl: {0}", searchUrl);

                body.Append(BodyTemplate
                    .Replace("{Resource_Body}", items.ToString())
                    .Replace("{Comments}", comments)
                    .Replace("{Search_Count}", $"{searchResultSet.TotalResultsCount:#,##0}")
                    .Replace("{Search_Term}", searchResultSet.Query.GetDescription())
                    .Replace("{Search_Term_Url}",
                        urlHelper.Action("Link", "Search", searchResultSet.Query.ToRouteValues(true),
                            isSecureConnection ? "https" : "http", urlHost))
                );
            }

            var userEmail = fromAddress;
            var userInstitutionName = "";
            var userAccountNumber = "";
            IUser user = authenticatedInstitution != null ? authenticatedInstitution.User : null;
            if (user != null)
            {
                userEmail = user.Email;
                if (user.InstitutionId > 0)
                {
                    userInstitutionName = authenticatedInstitution.Name;
                    userAccountNumber = authenticatedInstitution.AccountNumber;
                }
            }

            html.Append(MainTemplate
                .Replace("{Title}", "Search Results")
                .Replace("{Body}", body.ToString())
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}",
                    urlHelper.Action("Index", "Home", new { Area = "" }, isSecureConnection ? "https" : "http"))
                .Replace("{User_Email}", userEmail)
                .Replace("{Institution_Name}", userInstitutionName)
                .Replace(
                    string.IsNullOrWhiteSpace(userAccountNumber) ? "(#{Institution_Number}) -" : "{Institution_Number}",
                    string.IsNullOrWhiteSpace(userAccountNumber) ? "" : userAccountNumber
                )
            );
            return html.ToString();
        }
    }
}