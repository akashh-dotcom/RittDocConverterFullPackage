#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class DashboardEmailBuildService : EmailBuildBaseService
    {
        private readonly ILog<EmailBuildBaseService> _log;

        public DashboardEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            _log = log;
        }

        public string HeaderAndFooter { get; private set; }
        public string Notes { get; private set; }
        public string NotesItem { get; private set; }

        public string NotesItemStatic { get; private set; }


        public string Body { get; private set; }
        public string Highlights { get; private set; }
        public string HighlightsResource { get; private set; }
        public string HighlightsNoResource { get; private set; }

        public string ResourceEvenDark { get; private set; }
        public string ResourceEvenLight { get; private set; }
        public string EvenPriceListLight { get; private set; }
        public string EvenPriceListDark { get; private set; }


        public string ResourceOddDark { get; private set; }
        public string ResourceOddLight { get; private set; }
        public string OddPriceListLight { get; private set; }
        public string OddPriceListDark { get; private set; }


        public string ResourceSectionHeaderLight { get; private set; }
        public string ResourceSectionHeaderDark { get; private set; }


        public string ResourceListFirst { get; private set; }
        public string ResourceListLight { get; private set; }
        public string ResourceListDark { get; private set; }


        public void SetTemplates()
        {
            var dashboardFolder = "Dashboard_Min";
            try
            {
                Body = GetTemplateFromFile("Body.html", dashboardFolder);

                HeaderAndFooter = GetTemplateFromFile("Header-Footer.html", dashboardFolder);
                Highlights = GetTemplateFromFile("Highlights.html", dashboardFolder);
                HighlightsNoResource = GetTemplateFromFile("Highlights_No_Resource.html", dashboardFolder);
                HighlightsResource = GetTemplateFromFile("Highlights_Resource.html", dashboardFolder);
                Notes = GetTemplateFromFile("Notes.html", dashboardFolder);
                NotesItem = GetTemplateFromFile("Notes_Item.html", dashboardFolder);

                NotesItemStatic = GetTemplateFromFile("Notes_Item_Static.html", dashboardFolder);

                ResourceOddDark = GetTemplateFromFile("Resource_Odd_Dark.html", dashboardFolder);
                ResourceOddLight = GetTemplateFromFile("Resource_Odd_Light.html", dashboardFolder);
                EvenPriceListLight = GetTemplateFromFile("Resource_Even_PriceList_Light.html", dashboardFolder);
                EvenPriceListDark = GetTemplateFromFile("Resource_Even_PriceList_Dark.html", dashboardFolder);
                OddPriceListLight = GetTemplateFromFile("Resource_Odd_PriceList_Light.html", dashboardFolder);
                OddPriceListDark = GetTemplateFromFile("Resource_Odd_PriceList_Dark.html", dashboardFolder);
                ResourceSectionHeaderLight = GetTemplateFromFile("Resource_Section_Header_Light.html", dashboardFolder);
                ResourceSectionHeaderDark = GetTemplateFromFile("Resource_Section_Header_Dark.html", dashboardFolder);

                ResourceListFirst = GetTemplateFromFile("ResourceList_Wrapper_First.html", dashboardFolder);
                ResourceListLight = GetTemplateFromFile("ResourceList_Wrapper_Light.html", dashboardFolder);
                ResourceListDark = GetTemplateFromFile("ResourceList_Wrapper_Dark.html", dashboardFolder);


                ResourceEvenDark = GetTemplateFromFile("Resource_Even_Dark.html", dashboardFolder);
                ResourceEvenLight = GetTemplateFromFile("Resource_Even_Light.html", dashboardFolder);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(ex.Message, ex);
            }
        }

        public string GetDashboardBodyBase(InstitutionEmailStatistics stats, IInstitution institution)
        {
            if (string.IsNullOrWhiteSpace(Highlights))
            {
                SetTemplates();
            }

            var highlights = GetHighlights(stats, institution);

            var body = GetAccountUsage(stats, highlights);

            var resourceLists = GetSpotLight(stats, institution);

            body = body.Replace("{ResourceLists}", resourceLists);

            return HeaderAndFooter
                    //.Replace("{Quick_Notes}", quickNotes)
                    .Replace("{Institution_Name}", institution.Name)
                    .Replace("{Institution_Number}", institution.AccountNumber)
                    .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                    .Replace("{Body}", body)
                ;
        }

        public EmailMessage BuildDashboardEmail(InstitutionEmailStatistics stats, User user, string baseBody)
        {
            var quickNotes = GetQuickNotes(stats, user);

            var emailBody = baseBody.Replace("{Quick_Notes}", quickNotes);

            emailBody = emailBody.Replace("{User_Email}", user.Email);

            return BuildEmailMessage(user, "Your Library’s R2 Activity Summary and News", emailBody);
        }


        private string GetHighlights(InstitutionEmailStatistics stats, IInstitution institution)
        {
            var mostAccessedResource = stats.MostAccessedResource == null
                    ? HighlightsNoResource
                    : HighlightsResource
                        .Replace("{Resource_Image_URL}", GetResourceImageUrl(stats.MostAccessedResource.ImageFileName))
                        .Replace("{Resource_URL}",
                            GetResourceLink(stats.MostAccessedResource.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title}", stats.MostAccessedResource.Title)
                        .Replace("{Resource_Edition}", stats.MostAccessedResource.Edition)
                        .Replace("{Resource_Author}", GetAuthorDisplay(stats.MostAccessedResource))
                        .Replace("{Resource_ISBN}", stats.MostAccessedResource.Isbn)
                        .Replace("{Resource_Count}", stats.Highlights.MostAccessedCount.ToString())
                        .Replace("{Resource_Publisher}",
                            string.IsNullOrWhiteSpace(stats.MostAccessedResource.Publisher.DisplayName)
                                ? stats.MostAccessedResource.Publisher.Name
                                : stats.MostAccessedResource.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date}",
                            stats.MostAccessedResource.PublicationDate.GetValueOrDefault(DateTime.Now).ToString("yyyy"))
                ;
            var leastAccessedResource = stats.LeastAccessedResource == null
                    ? HighlightsNoResource
                    : HighlightsResource
                        .Replace("{Resource_Image_URL}", GetResourceImageUrl(stats.LeastAccessedResource.ImageFileName))
                        .Replace("{Resource_URL}",
                            GetResourceLink(stats.LeastAccessedResource.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title}", stats.LeastAccessedResource.Title)
                        .Replace("{Resource_Edition}", stats.LeastAccessedResource.Edition)
                        .Replace("{Resource_Author}", GetAuthorDisplay(stats.LeastAccessedResource))
                        .Replace("{Resource_ISBN}", stats.LeastAccessedResource.Isbn)
                        .Replace("{Resource_Count}", stats.Highlights.LeastAccessedCount.ToString())
                        .Replace("{Resource_Publisher}",
                            string.IsNullOrWhiteSpace(stats.LeastAccessedResource.Publisher.DisplayName)
                                ? stats.LeastAccessedResource.Publisher.Name
                                : stats.LeastAccessedResource.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date}",
                            stats.LeastAccessedResource.PublicationDate.GetValueOrDefault(DateTime.Now)
                                .ToString("yyyy"))
                ;
            var mostAccessTurnawayResoruce = stats.MostTurnawayAccessResource == null
                    ? HighlightsNoResource
                    : HighlightsResource
                        .Replace("{Resource_Image_URL}",
                            GetResourceImageUrl(stats.MostTurnawayAccessResource.ImageFileName))
                        .Replace("{Resource_URL}",
                            GetResourceLink(stats.MostTurnawayAccessResource.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title}", stats.MostTurnawayAccessResource.Title)
                        .Replace("{Resource_Edition}", stats.MostTurnawayAccessResource.Edition)
                        .Replace("{Resource_Author}", GetAuthorDisplay(stats.MostTurnawayAccessResource))
                        .Replace("{Resource_ISBN}", stats.MostTurnawayAccessResource.Isbn)
                        .Replace("{Resource_Count}", stats.Highlights.MostTurnawayAccessCount.ToString())
                        .Replace("{Resource_Publisher}",
                            string.IsNullOrWhiteSpace(stats.MostTurnawayAccessResource.Publisher.DisplayName)
                                ? stats.MostTurnawayAccessResource.Publisher.Name
                                : stats.MostTurnawayAccessResource.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date}",
                            stats.MostTurnawayAccessResource.PublicationDate.GetValueOrDefault(DateTime.Now)
                                .ToString("yyyy"))
                ;
            var mostConcurrentTurnawayResoruce = stats.MostTurnawayConcurrentResource == null
                    ? HighlightsNoResource
                    : HighlightsResource
                        .Replace("{Resource_Image_URL}",
                            GetResourceImageUrl(stats.MostTurnawayConcurrentResource.ImageFileName))
                        .Replace("{Resource_URL}",
                            GetResourceLink(stats.MostTurnawayConcurrentResource.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title}", stats.MostTurnawayConcurrentResource.Title)
                        .Replace("{Resource_Edition}", stats.MostTurnawayConcurrentResource.Edition)
                        .Replace("{Resource_Author}", GetAuthorDisplay(stats.MostTurnawayConcurrentResource))
                        .Replace("{Resource_ISBN}", stats.MostTurnawayConcurrentResource.Isbn)
                        .Replace("{Resource_Count}", stats.Highlights.MostTurnawayConcurrentCount.ToString())
                        .Replace("{Resource_Publisher}",
                            string.IsNullOrWhiteSpace(stats.MostTurnawayConcurrentResource.Publisher.DisplayName)
                                ? stats.MostTurnawayConcurrentResource.Publisher.Name
                                : stats.MostTurnawayConcurrentResource.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date}",
                            stats.MostTurnawayConcurrentResource.PublicationDate.GetValueOrDefault(DateTime.Now)
                                .ToString("yyyy"))
                ;

            //dt.ToString( "MMMM" ) to get month name
            return Highlights
                    .Replace("{Report_Month}", stats.StartDate.ToString("MMMM"))
                    .Replace("{Highlight_Item_Most_Accessed}", mostAccessedResource)
                    .Replace("{Highlight_Item_Least_Accessed}", leastAccessedResource)
                    .Replace("{Highlight_Item_Most_Concurrent_Turnaway}", mostConcurrentTurnawayResoruce)
                    .Replace("{Highlight_Item_Most_Access_Turnaway}", mostAccessTurnawayResoruce)
                    .Replace("{Discipline_Month_Name}", stats.Highlights.MostPopularSpecialtyName)
                    .Replace("{Discipline_Month_Count}", stats.Highlights.MostPopularSpecialtyCount.ToString())
                    .Replace("{Discipline_Year_Name}", stats.MostPopularSpecialtyNameOfYear)
                    .Replace("{Discipline_Year_Count}", stats.MostPopularSpecialtyCountOfYear.ToString())
                    .Replace("{Discipline_Month_URL}", GetDisciplineLink(institution.Id, stats.MostPopularSpecialtyId))
                    .Replace("{Discipline_Year_URL}",
                        GetDisciplineLink(institution.Id, stats.MostPopularSpecialtyIdOfYear))
                ;
        }

        private string GetQuickNotes(InstitutionEmailStatistics stats, User user)
        {
            var notesBuilder = new StringBuilder();

            if (stats.QuickNotes == null)
            {
                notesBuilder.Append(NotesItemStatic);
            }
            else
            {
                foreach (var quickNote in stats.QuickNotes)
                {
                    notesBuilder.AppendFormat(NotesItem.Replace("{Note_Item}", quickNote));
                }
            }


            return Notes
                    .Replace("{Institution_Name}", user.Institution.Name)
                    .Replace("{User_Name}", $"{user.FirstName} {user.LastName}")
                    .Replace("{Notes}", notesBuilder.ToString())
                    .Replace("{Dashboard_Url}", GetDashboardLink(user.InstitutionId.GetValueOrDefault()))
                ;
        }

        private string GetAccountUsage(InstitutionEmailStatistics stats, string highlights)
        {
            return Body
                    .Replace("{Usage_Successful_Content_Retrievals_Month}", stats.AccountUsage.ContentCount.ToString())
                    .Replace("{Usage_Successful_Content_Retrievals_Year}",
                        stats.YearAccountUsage.ContentCount.ToString())
                    .Replace("{Usage_Toc_Month}", stats.AccountUsage.TocCount.ToString())
                    .Replace("{Usage_Toc_Year}", stats.YearAccountUsage.TocCount.ToString())
                    .Replace("{Usage_Session_Month}", stats.AccountUsage.SessionCount.ToString())
                    .Replace("{Usage_Session_Year}", stats.YearAccountUsage.SessionCount.ToString())
                    .Replace("{Usage_Print_Month}", stats.AccountUsage.PrintCount.ToString())
                    .Replace("{Usage_Print_Year}", stats.YearAccountUsage.PrintCount.ToString())
                    .Replace("{Usage_Email_Month}", stats.AccountUsage.EmailCount.ToString())
                    .Replace("{Usage_Email_Year}", stats.YearAccountUsage.EmailCount.ToString())
                    .Replace("{Usage_Concurrent_Month}", stats.AccountUsage.TurnawayConcurrencyCount.ToString())
                    .Replace("{Usage_Concurrent_Year}", stats.YearAccountUsage.TurnawayConcurrencyCount.ToString())
                    .Replace("{Usage_Access_Month}", stats.AccountUsage.TurnawayAccessCount.ToString())
                    .Replace("{Usage_Access_Year}", stats.YearAccountUsage.TurnawayAccessCount.ToString())
                    .Replace("{Usage_Question_URL}", GetContactLink())
                    .Replace("{Highlights}", highlights)
                ;
        }

        private string GetSpotLight(InstitutionEmailStatistics stats, IInstitution institution)
        {
            var spotlightBuilder = new StringBuilder();
            spotlightBuilder.Append(GetPricedTitles(stats.FeaturedTitleResources, institution, true,
                "Featured Titles"));
            spotlightBuilder.Append(GetPricedTitles(stats.CurrentSpecialResources, institution,
                stats.FeaturedTitleResources == null || !stats.FeaturedTitleResources.Any(), "Specials"));
            spotlightBuilder.Append(GetNonPricedTitles(stats.ExpertRecommendedResources, institution,
                stats.FeaturedTitleResources == null || !stats.FeaturedTitleResources.Any(),
                stats.CurrentSpecialResources == null || !stats.CurrentSpecialResources.Any(), "Recommended Titles"));
            return spotlightBuilder.ToString();
        }

        private string GetPricedTitles(List<KeyValuePair<IResource, decimal>> resourcesAndPrices,
            IInstitution institution, bool isFirst, string sectionTitle)
        {
            var resourceListTemplate = isFirst ? ResourceListFirst : ResourceListDark;
            var oddTemplate = isFirst ? ResourceOddLight : ResourceOddDark;
            var evenTemplate = isFirst ? ResourceEvenLight : ResourceEvenDark;

            var oddPriceTempate = isFirst ? OddPriceListLight : OddPriceListDark;
            var evenPriceTemplate = isFirst ? EvenPriceListLight : EvenPriceListDark;
            try
            {
                if (resourcesAndPrices != null && resourcesAndPrices.Any())
                {
                    var currentSpecialResourcesBuilder = new StringBuilder();

                    var header = isFirst
                            //? ResourceSectionHeaderLight.Replace("{Section_Title}", "Specials")
                            //: ResourceSectionHeaderDark.Replace("{Section_Title}", "Specials")
                            ? ResourceSectionHeaderLight.Replace("{Section_Title}", sectionTitle)
                            : ResourceSectionHeaderDark.Replace("{Section_Title}", sectionTitle)
                        ;

                    if (resourcesAndPrices.Count == 3 || resourcesAndPrices.Count == 1) //ODD
                    {
                        foreach (var item in resourcesAndPrices)
                        {
                            currentSpecialResourcesBuilder.Append(GetResourceString(item.Key, institution, oddTemplate,
                                oddPriceTempate,
                                item.Value));
                        }

                        return
                            resourceListTemplate.Replace("{ResourceList_Header}", header)
                                .Replace("{ResourceList_Resources}", currentSpecialResourcesBuilder.ToString());
                    }

                    var resourceList = resourceListTemplate.Replace("{ResourceList_Header}", header);


                    var resources1 = GetResourceString(
                        resourcesAndPrices[0].Key, resourcesAndPrices[1].Key
                        , institution, evenTemplate, evenPriceTemplate
                        , resourcesAndPrices[0].Value, resourcesAndPrices[1].Value
                    );

                    var wrapperBuilder = new StringBuilder();
                    wrapperBuilder.Append(resources1);

                    if (resourcesAndPrices.Count > 3)
                    {
                        var resources2 = GetResourceString(resourcesAndPrices[2].Key,
                            resourcesAndPrices[3].Key, institution, evenTemplate, evenPriceTemplate,
                            resourcesAndPrices[2].Value, resourcesAndPrices[3].Value);

                        wrapperBuilder.Append(resources2);
                    }

                    resourceList = resourceList.Replace("{ResourceList_Resources}", wrapperBuilder.ToString());
                    return resourceList;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return null;
        }

        private string GetNonPricedTitles(IList<IResource> resources, IInstitution institution, bool isFirst,
            bool isSecond, string title)
        {
            var resourceListTemplate = isFirst ? ResourceListFirst : isSecond ? ResourceListDark : ResourceListLight;
            var oddTemplate = isFirst ? ResourceOddLight : isSecond ? ResourceOddDark : ResourceOddLight;
            var evenTemplate = isFirst ? ResourceEvenLight : isSecond ? ResourceEvenDark : ResourceEvenLight;

            try
            {
                if (resources != null && resources.Any())
                {
                    var recommendedBuilder = new StringBuilder();

                    var header = isFirst && !isSecond
                        ? ResourceSectionHeaderLight.Replace("{Section_Title}", title)
                        : isSecond
                            ? ResourceSectionHeaderDark.Replace("{Section_Title}", title)
                            : ResourceSectionHeaderLight.Replace("{Section_Title}", title);

                    //? ResourceSectionHeaderLight.Replace("{Section_Title}", "Recommended Titles")
                    //: isSecond
                    //    ? ResourceSectionHeaderDark.Replace("{Section_Title}", "Recommended Titles")
                    //    : ResourceSectionHeaderLight.Replace("{Section_Title}", "Recommended Titles");

                    if (resources.Count == 3 || resources.Count == 1) //ODD
                    {
                        foreach (var resource in resources)
                        {
                            recommendedBuilder.Append(GetResourceString(resource, institution, oddTemplate));
                        }

                        return
                            resourceListTemplate.Replace("{ResourceList_Header}", header)
                                .Replace("{ResourceList_Resources}", recommendedBuilder.ToString());
                    }


                    var resourceList = resourceListTemplate.Replace("{ResourceList_Header}", header);
                    var resources1 = GetResourceString(resources[0], resources[1], institution, evenTemplate);

                    var wrapperBuilder = new StringBuilder();
                    wrapperBuilder.Append(resources1);

                    if (resources.Count > 3)
                    {
                        var resources2 = GetResourceString(resources[2], resources[3], institution, evenTemplate);

                        wrapperBuilder.Append(resources2);
                    }

                    resourceList = resourceList.Replace("{ResourceList_Resources}", wrapperBuilder.ToString());
                    return resourceList;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return null;
        }

        private string GetResourceString(IResource resource, IInstitution institution, string templateString,
            string priceTemplate = null, decimal discountPrice = 0)
        {
            var template =
                    templateString
                        .Replace("{Resource_Image_URL}", GetResourceImageUrl(resource.ImageFileName))
                        .Replace("{Resource_URL}", GetResourceLink(resource.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title}", resource.Title)
                        .Replace("{Resource_Edition}", resource.Edition)
                        .Replace("{Resource_Author}", GetAuthorDisplay(resource))
                        .Replace("{Resource_ISBN}", resource.Isbn)
                        .Replace("{Resource_Publisher}",
                            string.IsNullOrWhiteSpace(resource.Publisher.DisplayName)
                                ? resource.Publisher.Name
                                : resource.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date}",
                            resource.PublicationDate.GetValueOrDefault(DateTime.Now).ToString("yyyy"))
                ;

            if ((discountPrice > 0 || resource.IsFreeResource) && !string.IsNullOrWhiteSpace(priceTemplate))
            {
                priceTemplate = priceTemplate
                        .Replace("{Resource_ListPrice}", resource.ListPriceString(true))
                        .Replace("{Resource_DiscountPrice}", resource.DiscountPriceString(discountPrice))
                    ;
            }
            else
            {
                priceTemplate = "";
            }

            template = template.Replace("{Resource_PriceList}", priceTemplate);

            return template;
        }

        private string GetResourceString(IResource resource1, IResource resource2, IInstitution institution,
            string templateString, string priceTemplate = null, decimal discountPrice1 = 0, decimal discountPrice2 = 0)
        {
            var template =
                    templateString
                        .Replace("{Resource_Image_URL1}", GetResourceImageUrl(resource1.ImageFileName))
                        .Replace("{Resource_URL1}", GetResourceLink(resource1.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title1}", resource1.Title)
                        .Replace("{Resource_Edition1}", resource1.Edition)
                        .Replace("{Resource_Author1}", GetAuthorDisplay(resource1))
                        .Replace("{Resource_ISBN1}", resource1.Isbn)
                        .Replace("{Resource_Publisher1}",
                            string.IsNullOrWhiteSpace(resource1.Publisher.DisplayName)
                                ? resource1.Publisher.Name
                                : resource1.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date1}",
                            resource1.PublicationDate.GetValueOrDefault(DateTime.Now).ToString("yyyy"))
                        .Replace("{Resource_Image_URL2}", GetResourceImageUrl(resource2.ImageFileName))
                        .Replace("{Resource_URL2}", GetResourceLink(resource2.Isbn, institution.AccountNumber))
                        .Replace("{Resource_Title2}", resource2.Title)
                        .Replace("{Resource_Edition2}", resource2.Edition)
                        .Replace("{Resource_Author2}", GetAuthorDisplay(resource2))
                        .Replace("{Resource_ISBN2}", resource2.Isbn)
                        .Replace("{Resource_Publisher2}",
                            string.IsNullOrWhiteSpace(resource2.Publisher.DisplayName)
                                ? resource2.Publisher.Name
                                : resource2.Publisher.DisplayName)
                        .Replace("{Resource_Publication_Date2}",
                            resource2.PublicationDate.GetValueOrDefault(DateTime.Now).ToString("yyyy"))
                ;

            if ((discountPrice1 > 0 || resource1.IsFreeResource) && !string.IsNullOrWhiteSpace(priceTemplate))
            {
                priceTemplate = priceTemplate
                        .Replace("{Resource_ListPrice1}", resource1.ListPriceString(true))
                        .Replace("{Resource_DiscountPrice1}", resource1.DiscountPriceString(discountPrice1))
                        .Replace("{Resource_ListPrice2}", resource2.ListPriceString(true))
                        .Replace("{Resource_DiscountPrice2}", resource2.DiscountPriceString(discountPrice2))
                    ;
            }
            else
            {
                priceTemplate = "";
            }

            template = template.Replace("{Resource_PriceList}", priceTemplate);

            return template;
        }

        private string GetAuthorDisplay(IResource resource)
        {
            if (resource.AuthorList != null)
            {
                var author = resource.AuthorList.FirstOrDefault(x => x.Order == 1);
                if (author != null)
                {
                    return
                        $"{author.LastName}{(string.IsNullOrWhiteSpace(author.FirstName) ? "" : $", {author.FirstName}")}{(string.IsNullOrWhiteSpace(author.Degrees) ? "" : $" {(author.Degrees.Substring(0, 2).Contains(",") ? author.Degrees : $", {author.Degrees}")}")}";
                }
            }

            return resource.Authors != null
                ? $"{(resource.Authors.Length > 50 ? $"{resource.Authors.Substring(0, 50)} et al." : resource.Authors)}"
                : "";
        }
    }
}