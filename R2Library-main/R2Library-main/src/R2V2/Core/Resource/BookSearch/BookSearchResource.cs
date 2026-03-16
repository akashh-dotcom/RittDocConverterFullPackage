#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using R2V2.Core.Resource.Collection;
using Directory = System.IO.Directory;

#endregion

namespace R2V2.Core.Resource.BookSearch
{
    public class BookSearchResource
    {
        private readonly string _contentPath;

        public BookSearchResource(string contentPath)
        {
            _contentPath = contentPath;
        }

        public BookSearchResource(IResource resource, string contentPath)
        {
            _contentPath = contentPath;
            Isbn = resource.Isbn;
            Isbn10 = resource.Isbn10;
            Isbn13 = resource.Isbn13;
            EIsbn = resource.EIsbn;
            Title = resource.Title;
            SubTitle = resource.SubTitle;
            if (resource.Publisher != null)
            {
                PublisherName = resource.Publisher.Name;
            }

            if (resource.PracticeAreas != null)
            {
                PracticeAreas = new List<string>();
                foreach (var resourcePracticeArea in resource.PracticeAreas)
                {
                    PracticeAreas.Add(resourcePracticeArea.Name);
                }
            }

            if (resource.Specialties != null)
            {
                Specialties = new List<string>();
                foreach (var specialty in resource.Specialties)
                {
                    Specialties.Add(specialty.Name);
                }
            }

            StatusString = resource.StatusToString();
            CopyRight = resource.Copyright;
            R2ReleaseDate = resource.ReleaseDate.HasValue ? $"{resource.ReleaseDate: yyyy/MM/dd}" : null;
            IsDrugMonograph = resource.DrugMonograph > 0;
            IsBrandonHill = resource.Collections != null &&
                            resource.Collections.Select(x => x.Id).Contains((int)CollectionIdentifier.BradonHill);
            if (resource.AuthorList != null && resource.AuthorList.Any())
            {
                Authors = resource.AuthorList.Select(x => x.GetFullName(true)).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(resource.Authors))
            {
                Authors = new List<string> { resource.Authors };
            }
        }

        public string Isbn { private get; set; }
        public string Isbn10 { private get; set; }
        public string Isbn13 { private get; set; }
        public string EIsbn { private get; set; }
        public string Title { private get; set; }
        public string SubTitle { private get; set; }
        public string PublisherName { private get; set; }
        public List<string> ConsolidatedPublisherNames { private get; set; }
        public List<string> PracticeAreas { private get; set; }
        public List<string> Specialties { private get; set; }
        public string StatusString { private get; set; }
        public string CopyRight { private get; set; }
        public string CopyRightHolder { private get; set; }
        public string R2ReleaseDate { private get; set; }
        public bool IsDrugMonograph { private get; set; }
        public bool IsBrandonHill { private get; set; }
        public string PrimaryAuthor { private get; set; }
        public List<string> Authors { private get; set; }
        public List<string> Editors { private get; set; }


        public void SaveR2BookSearchXml()
        {
            var path = GetFolderPath();
            //Create directory if it does not exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = GetFilePath();

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var r2BookSearchDoc = new XmlDocument();
            var r2BookSearchNode = r2BookSearchDoc.CreateNode(XmlNodeType.Element, "r2BookSearch", null);
            r2BookSearchDoc.AppendChild(r2BookSearchNode);

            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Isbn10", Isbn10);
            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Isbn", Isbn10);

            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Isbn13", Isbn13);
            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Isbn", Isbn13);

            if (!string.IsNullOrWhiteSpace(EIsbn))
            {
                AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2EIsbn", EIsbn);
                AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Isbn", EIsbn);
            }

            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2BookTitle", Title);
            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2BookSubTitle", SubTitle);

            if (PublisherName != null)
            {
                AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Publisher", PublisherName);
            }


            if (ConsolidatedPublisherNames != null)
            {
                foreach (var publisher in ConsolidatedPublisherNames)
                {
                    AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2AssociatedPublisher", publisher);
                }
            }

            if (PracticeAreas != null)
            {
                foreach (var practiceArea in PracticeAreas)
                {
                    AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2PracticeArea", practiceArea);
                }
            }

            if (Specialties != null)
            {
                foreach (var specialty in Specialties)
                {
                    AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Specialty", specialty);
                }
            }

            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2BookStatus", StatusString);
            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2CopyrightYear", CopyRight);
            if (!string.IsNullOrWhiteSpace(CopyRightHolder))
            {
                AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2CopyrightHolder", CopyRightHolder);
            }


            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2ReleaseDate", R2ReleaseDate);
            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2DrugMonograph",
                IsDrugMonograph ? "DrugMonograph" : null);
            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2BrandonHill", IsBrandonHill ? "BrandonHill" : null);

            AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2AuthorPrimary", PrimaryAuthor);

            if (Authors != null)
            {
                foreach (var author in Authors)
                {
                    AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Author", author);
                }
            }

            if (Editors != null)
            {
                foreach (var editor in Editors)
                {
                    AppendXmlNode(r2BookSearchDoc, r2BookSearchNode, "r2Editor", editor);
                }
            }

            r2BookSearchDoc.Save(filePath);
        }

        /// <summary>
        ///     Check if content already exists before
        /// </summary>
        public bool DoesR2BookSearchXmlExist()
        {
            var folderPath = GetFolderPath();
            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            var filePath = GetFilePath();
            return File.Exists(filePath);
        }

        private string GetFolderPath()
        {
            return _contentPath.Contains("html")
                ? Path.Combine(_contentPath, Isbn.Trim())
                : Path.Combine(_contentPath, "html", Isbn.Trim());
        }

        private string GetFilePath()
        {
            return Path.Combine(GetFolderPath(), $"r2BookSearch.{Isbn.Trim()}.xml");
        }

        private void AppendXmlNode(XmlDocument xmlDoc, XmlNode parentNode, string nodeName, string nodeValue)
        {
            if (!string.IsNullOrEmpty(nodeValue))
            {
                var childNode = xmlDoc.CreateNode(XmlNodeType.Element, nodeName, null);
                childNode.InnerText = nodeValue;
                //childNode.InnerXml =  nodeValue;
                parentNode.AppendChild(childNode);
            }
        }
    }
}