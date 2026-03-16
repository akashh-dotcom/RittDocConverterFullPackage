#region

using System.Diagnostics;
using System.Text;
using System.Xml;
using R2Utilities.Utilities;

#endregion

namespace R2Utilities.Tasks.ContentTasks.BookInfo
{
    public class DocSearchInfo : R2UtilitiesBase
    {
        public DocSearchInfo(BookSearchInfo bookSearchInfo, string xmlFullFilePath, string filePrefix)
        {
            BookSearchInfo = bookSearchInfo;
            XmlFullFilePath = xmlFullFilePath;
            FilePrefix = filePrefix;

            PopulateMetaTags();
        }

        public BookSearchInfo BookSearchInfo { get; }
        public string XmlFullFilePath { get; }
        public string FilePrefix { get; }
        public string MetaTags { get; private set; }

        private void PopulateMetaTags()
        {
            var modifyHtmlStopwatch = new Stopwatch();
            modifyHtmlStopwatch.Start();

            var xmlDoc = new XmlDocument { PreserveWhitespace = false, XmlResolver = null };
            xmlDoc.Load(XmlFullFilePath);

            var indexTerms = GetRisIndexTerms(xmlDoc);
            var primaryAuthor = BookSearchInfo.PrimaryAuthor.GetFullName(true);

            var sectionId = GetSectionId(xmlDoc);
            var sectionTitle = GetSectionTitle(xmlDoc);
            var chapterTitle = GetChapterTitle(xmlDoc);
            var chapterId = GetChapterId(xmlDoc);
            var chapterNumber = GetChapterNumber(xmlDoc);

            var metaTags = new StringBuilder()
                .AppendLine("<!-- r2v2 meta tags - start -->");

            AppendMetaTag(metaTags, "r2SectionId", sectionId, false);
            AppendMetaTag(metaTags, "r2SectionTitle", sectionTitle, false);
            AppendMetaTag(metaTags, "r2ChapterTitle", chapterTitle, false);
            AppendMetaTag(metaTags, "r2ChapterId", chapterId, false);
            AppendMetaTag(metaTags, "r2ChapterNumber", chapterNumber, false);
            AppendMetaTag(metaTags, "r2DrugMonograph", BookSearchInfo.IsDrugMonograph ? "DrugMonograph" : null, false);
            AppendMetaTag(metaTags, "r2BrandonHill", BookSearchInfo.IsBrandonHill ? "BrandonHill" : null, false);
            AppendMetaTag(metaTags, "r2ReleaseDate", BookSearchInfo.R2ReleaseDate, false);
            AppendMetaTag(metaTags, "r2CopyrightYear", $"{BookSearchInfo.CopyrightYear}", false);

            AppendMetaTag(metaTags, "r2BookTitle", BookSearchInfo.Title, true);
            AppendMetaTag(metaTags, "r2PrimaryAuthor", primaryAuthor, true);

            AppendMetaTag(metaTags, "r2Publisher", BookSearchInfo.Publisher, true);

            if (BookSearchInfo.AssociatedPublishers != null)
            {
                foreach (var publisher in BookSearchInfo.AssociatedPublishers)
                {
                    AppendMetaTag(metaTags, "r2AssociatedPublisher", publisher, true);
                }
            }

            AppendMetaTag(metaTags, "r2PracticeArea", BookSearchInfo.PracticeAreas, true);
            AppendMetaTag(metaTags, "r2Specialty", BookSearchInfo.Specialties, true);
            AppendMetaTag(metaTags, "r2BookStatus", BookSearchInfo.BookStatus, true);
            AppendMetaTag(metaTags, "r2IndexTerms", indexTerms, true);
            AppendMetaTag(metaTags, "r2Isbn10", BookSearchInfo.Isbn10, true);
            AppendMetaTag(metaTags, "r2Isbn13", BookSearchInfo.Isbn13, true);
            if (!string.IsNullOrWhiteSpace(BookSearchInfo.EIsbn))
            {
                AppendMetaTag(metaTags, "r2EIsbn", BookSearchInfo.EIsbn, true);
            }

            metaTags.AppendLine("<!-- r2v2 meta tags - end -->");

            MetaTags = metaTags.ToString();
        }

        private void AppendMetaTag(StringBuilder buffer, string name, string content, bool includeIfEmpty)
        {
            if (includeIfEmpty || !string.IsNullOrEmpty(content))
            {
                buffer.AppendFormat("<meta name=\"{0}\" content=\"{1}\" />", name, content).AppendLine();
            }
        }

        private string GetRisIndexTerms(XmlDocument xmlDoc)
        {
            const string xPath = "//risindex/risterm";

            var ristermNodes = XmlHelper.GetXmlNodes(xmlDoc, xPath);
            if (ristermNodes == null)
            {
                return string.Empty;
            }

            var terms = new StringBuilder();
            foreach (var ristermNode in ristermNodes)
            {
                terms.AppendFormat("{0}{1}", terms.Length > 0 ? ", " : string.Empty, ristermNode.InnerText.Trim());
            }

            return terms.ToString();
        }

        private string GetSectionId(XmlDocument xmlDoc)
        {
            var sectionNode = XmlHelper.GetXmlNode(xmlDoc, $"//{FilePrefix}");
            if (sectionNode == null)
            {
                return string.Empty;
            }

            var sectionId = XmlHelper.GetAttributeValue(sectionNode, "id");
            return sectionId;
        }

        private string GetSectionTitle(XmlDocument xmlDoc)
        {
            //oNode = oXMLDoc.SelectSingleNode("//sect1/title")
            //If Not oNode Is Nothing Then
            //    sSectionTitle = oNode.InnerText.ToUpper
            //End If
            var sectionTitleNode = XmlHelper.GetXmlNode(xmlDoc, $"//{FilePrefix}/title");
            var sectionTitle = sectionTitleNode == null ? string.Empty : sectionTitleNode.InnerText;

            if (string.IsNullOrEmpty(sectionTitle))
            {
                Log.DebugFormat("EMPTY SECTION TITLE - {0}", XmlFullFilePath);
            }

            return sectionTitle;
        }

        private string GetChapterTitle(XmlDocument xmlDoc)
        {
            var chapterTitleNode = XmlHelper.GetXmlNode(xmlDoc, "//risinfo/chaptertitle");
            var chapterTitle = chapterTitleNode == null ? string.Empty : chapterTitleNode.InnerText;

            if (string.IsNullOrEmpty(chapterTitle))
            {
                Log.DebugFormat("EMPTY CHAPTER TITLE - {0}", XmlFullFilePath);
            }

            return chapterTitle;
        }

        private string GetChapterId(XmlDocument xmlDoc)
        {
            var chapterIdNode = XmlHelper.GetXmlNode(xmlDoc, "//risinfo/chapterid");
            var chapterId = chapterIdNode == null ? string.Empty : chapterIdNode.InnerText;

            if (string.IsNullOrEmpty(chapterId))
            {
                Log.DebugFormat("EMPTY CHAPTER ID - {0}", XmlFullFilePath);
            }

            return chapterId;
        }

        private string GetChapterNumber(XmlDocument xmlDoc)
        {
            var chapterNumberNode = XmlHelper.GetXmlNode(xmlDoc, "//risinfo/chapternumber");
            var chapterNuber = chapterNumberNode == null ? string.Empty : chapterNumberNode.InnerText;

            if (string.IsNullOrEmpty(chapterNuber))
            {
                Log.DebugFormat("EMPTY CHAPTER NUMBER - {0}", XmlFullFilePath);
            }

            return chapterNuber;
        }
    }
}