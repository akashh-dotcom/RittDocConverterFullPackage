#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using R2V2.Core.Resource.Content.Navigation;
using R2V2.Core.Resource.Topic;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Core.Resource.Content
{
    public class ContentService : IContentService
    {
        private const string NavPrevious = "navprev";
        private const string NavCurrent = "navcurrent";
        private const string NavNext = "navnext";
        private const string BookTitle = "booktitle";
        private const string PartTitle = "parttitle";
        private const string PartId = "partid";
        private const string ChapterTitle = "chaptertitle";
        private const string ChapterId = "chapterid";
        private const string SectionTitle = "sectiontitle";

        private const string EmptyHtmlTransformResult = "<div></div>";
        private readonly IQueryable<AZIndex> _azIndex;
        private readonly IContentTransformer _contentTransformer;
        private readonly ILog<ContentService> _log;
        private readonly TopicService _topicService;

        public ContentService(ILog<ContentService> log, IContentTransformer contentTransformer,
            IQueryable<AZIndex> azIndex, TopicService topicService)
        {
            _log = log;
            _contentTransformer = contentTransformer;
            _azIndex = azIndex;
            _topicService = topicService;
        }

        public ContentItem GetTableOfContents(string isbn, string baseUrl, ResourceAccess resourceAccess, bool email)
        {
            return GetContent(isbn, "", baseUrl, ContentType.TableOfContents, resourceAccess, email);
        }

        public ContentItem GetContent(string isbn, string section, string baseUrl, ResourceAccess resourceAccess,
            bool email)
        {
            ContentType contentType;

            try
            {
                var typeCode2Digit = section.Substring(0, 2).ToLower();
                switch (typeCode2Digit)
                {
                    case "ap":
                        contentType = ContentType.Appendix;
                        break;

                    case "dd":
                    case "de":
                        contentType = ContentType.Dedication;
                        break;

                    case "gl":
                        contentType = ContentType.Glossary;
                        break;

                    case "bi":
                        contentType = ContentType.Bibliography;
                        break;

                    case "pr":
                        contentType = ContentType.Preface;
                        break;

                    case "pt":
                        //contentType = ContentType.Part;
                        contentType = section.Length > 6 && section.Substring(6, 2) != "sp"
                            ? ContentType.Book
                            : ContentType.Part;
                        break;

                    default:
                        contentType = ContentType.Book;
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat(
                    "Cannot determine Content Type for isbn: {0} - section: {1} - resourceAccess: {2} - exception: {3}",
                    isbn, section, resourceAccess, ex.Message);
                return null;
            }

            return GetContent(isbn, section, baseUrl, contentType, resourceAccess, email);
        }

        private ContentItem GetContent(string isbn, string section, string baseUrl, ContentType contentType,
            ResourceAccess resourceAccess, bool email)
        {
            // if chapter without section, find the first section of the chapter
            if (section.StartsWith("ch") && section.Length == 6)
            {
                var chapter = section;
                section = GetSectionFor(isbn, chapter, baseUrl, resourceAccess);
            }

            _contentTransformer.Isbn = isbn;
            _contentTransformer.Section = section;

            return new ContentItem
            {
                //Topics = GetTopics(isbn, section),
                Topics = string.IsNullOrEmpty(section)
                    ? new List<string>()
                    : _topicService.GetResourceTopics(isbn, section),
                Navigation = GetNavigation(isbn, section, baseUrl, resourceAccess),
                Html = GetHtml(isbn, section, baseUrl, contentType, resourceAccess, email)
                //Html = "TESTING"
            };
        }

        private string GetHtml(string isbn, string section, string baseUrl, ContentType contentType,
            ResourceAccess resourceAccess, bool email)
        {
            var htmlTransformResult =
                _contentTransformer.Transform(contentType, resourceAccess, baseUrl, email) as HtmlTransformResult;

            //HtmlTransformResult htmlTransformResult = _contentTransformService.Transform(contentType, resourceAccess, baseUrl, email, isbn, section) as HtmlTransformResult;

            if (htmlTransformResult != null && htmlTransformResult.Result != EmptyHtmlTransformResult)
            {
                return htmlTransformResult.Result;
            }

            _contentTransformer.Section = GetSectionFor(htmlTransformResult, isbn, section, baseUrl, resourceAccess);

            htmlTransformResult =
                _contentTransformer.Transform(contentType, resourceAccess, baseUrl, email) as HtmlTransformResult;

            return htmlTransformResult != null ? htmlTransformResult.Result : "";
        }

        private string GetSectionFor(HtmlTransformResult htmlTransformResult, string isbn, string section,
            string baseUrl, ResourceAccess resourceAccess)
        {
            if (htmlTransformResult != null && htmlTransformResult.Result == EmptyHtmlTransformResult)
            {
                return GetFirstChildSectionFor(isbn, section, baseUrl, resourceAccess);
            }

            return GetSectionFor(isbn, section, baseUrl, resourceAccess); // must be a subSection within another section
        }

        private string GetSectionFor(string isbn, string id, string baseUrl, ResourceAccess resourceAccess)
        {
            _contentTransformer.Isbn = isbn;
            _contentTransformer.Section = id;

            var transformResult =
                _contentTransformer.Transform(ContentType.Navigation, resourceAccess, baseUrl, false) as
                    XmlTransformResult;
            if (transformResult != null)
            {
                var xmlNode = transformResult.Result.ChildNodes.Item(0);
                if (xmlNode != null)
                {
                    var selectSingleNode = xmlNode.SelectSingleNode(NavCurrent);
                    if (selectSingleNode != null)
                    {
                        return selectSingleNode.InnerText.CleanAndTrim();
                    }
                }
            }

            return "";
        }

        private string GetFirstChildSectionFor(string isbn, string id, string baseUrl, ResourceAccess resourceAccess)
        {
            _contentTransformer.Isbn = isbn;
            _contentTransformer.Section = id;

            var transformResult =
                _contentTransformer.Transform(ContentType.Navigation, resourceAccess, baseUrl, false) as
                    XmlTransformResult;
            if (transformResult != null)
            {
                var xmlNode = transformResult.Result.ChildNodes.Item(0);
                if (xmlNode != null)
                {
                    var selectSingleNode = xmlNode.SelectSingleNode(NavNext);
                    if (selectSingleNode != null)
                    {
                        return selectSingleNode.InnerText.CleanAndTrim();
                    }
                }
            }

            return "";
        }

        private Navigation.Navigation GetNavigation(string isbn, string section, string baseUrl,
            ResourceAccess resourceAccess)
        {
            _contentTransformer.Isbn = isbn;
            _contentTransformer.Section = section;

            var transformResult =
                _contentTransformer.Transform(ContentType.Navigation, resourceAccess, baseUrl, false) as
                    XmlTransformResult;

            return transformResult != null ? BuildNavigationItems(transformResult.Result.ChildNodes.Item(0)) : null;
        }

        private static Navigation.Navigation BuildNavigationItems(XmlNode xmlNode)
        {
            var navigationItems = new Navigation.Navigation();

            if (xmlNode != null)
            {
                navigationItems.Previous = BuildNavigationItem(xmlNode, NavPrevious);
                navigationItems.Current = BuildNavigationItem(xmlNode, NavCurrent);
                navigationItems.Next = BuildNavigationItem(xmlNode, NavNext);

                navigationItems.Book = BuildNavigationItem(xmlNode, BookTitle);

                navigationItems.Part = BuildNavigationItem(xmlNode, PartTitle);

                var selectSinglePartNode = xmlNode.SelectSingleNode(PartId);
                if (navigationItems.Part != null && selectSinglePartNode != null)
                {
                    navigationItems.Part.Id = selectSinglePartNode.InnerText.CleanAndTrim();
                }

                navigationItems.Chapter = BuildNavigationItem(xmlNode, ChapterTitle);

                var selectSingleNode = xmlNode.SelectSingleNode(ChapterId);
                if (navigationItems.Chapter != null && selectSingleNode != null)
                {
                    navigationItems.Chapter.Id = selectSingleNode.InnerText.CleanAndTrim();
                }

                navigationItems.Section = BuildNavigationItem(xmlNode, SectionTitle);
            }

            return navigationItems;
        }

        private static NavigationItem BuildNavigationItem(XmlNode rootNode, string nodeName)
        {
            var selectSingleNode = rootNode.SelectSingleNode(nodeName);
            if (selectSingleNode == null)
                return null;

            var text = selectSingleNode.InnerText.CleanAndTrim();

            return new NavigationItem { Id = text, Name = text };
        }
    }
}