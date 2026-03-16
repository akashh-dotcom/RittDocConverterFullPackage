#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Utilities;
using R2V2.Infrastructure.Compression;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class ResourceToHighlight : R2UtilitiesBase
    {
        #region Fields

        private readonly string _contentLocation;
        private readonly string _outputLocation;
        private readonly string _backupLocation;
        private readonly string _timestamp;

        private readonly ContentSettings _contentSettings = new ContentSettings();
        private readonly List<ResourceFile> _resourceFiles = new List<ResourceFile>();

        #endregion

        #region Methods

        public ResourceToHighlight(ITermHighlightSettings termHighlightSettings
            , TermHighlightQueue termHighlightQueue
            , DateTime timestamp
        )
        {
            TermHighlightQueue = termHighlightQueue;

            _contentLocation = _contentSettings.ContentLocation;
            _outputLocation = termHighlightSettings.OutputLocation;
            _backupLocation = termHighlightSettings.BackupLocation;

            _timestamp = $"{timestamp:yyyy-MM-dd_HH-mm-ss-tt}";

            Content = new List<ContentToHighlight>();
            Keywords = new HashSet<string>();

            InitializeContent();
        }

        public void AddResourceFile(ResourceFile resourceFile)
        {
            resourceFile.ResourceId = TermHighlightQueue.ResourceId;

            if (TermHighlightQueue.FirstDocumentId <= 0 || TermHighlightQueue.FirstDocumentId > resourceFile.DocumentId)
            {
                TermHighlightQueue.FirstDocumentId = resourceFile.DocumentId;
            }

            if (TermHighlightQueue.LastDocumentId <= 0 || TermHighlightQueue.LastDocumentId < resourceFile.DocumentId)
            {
                TermHighlightQueue.LastDocumentId = resourceFile.DocumentId;
            }

            _resourceFiles.Add(resourceFile);
        }

        public void LoadContent(bool removeComments = false)
        {
            foreach (var content in Content)
            {
                content.Load();
                Keywords.UnionWith(content.Keywords);
            }
        }

        public void WriteTempContent()
        {
            Directory.CreateDirectory(TempLocation);

            foreach (var content in Content)
            {
                File.WriteAllText(content.TempPath, content.TempContent);
            }
        }

        public void DeleteTempContent()
        {
            Directory.Delete(TempLocation, true);
        }

        public void WriteResourceBackup()
        {
            foreach (var content in Content)
            {
                File.Copy(content.ResourcePath, content.BackupPath);
            }
        }

        public void ZipResourceBackup()
        {
            ZipHelper.CompressDirectory(BackupLocation);
            Directory.Delete(BackupLocation, true);
        }

        private void InitializeContent()
        {
            var filePaths = Directory.GetFiles(ResourceLocation);

            foreach (var filePath in filePaths)
            {
                var fileName = new FileInfo(filePath).Name;

                var content = new ContentToHighlight
                {
                    FileName = fileName,

                    ResourcePath = filePath,
                    OutputPath = Path.Combine(OutputLocation, fileName),
                    BackupPath = Path.Combine(BackupLocation, fileName),
                    TempPath = Path.Combine(TempLocation, fileName),

                    TermHighlightType = TermHighlightQueue.TermHighlightType
                };

                Content.Add(content);
            }

            TotalFileCount = filePaths.Length;
        }

        private string GetPath(string location)
        {
            return Path.Combine(location, "Job " + TermHighlightQueue.JobId, "Batch - " + _timestamp,
                TermHighlightQueue.Isbn);
        }

        #endregion

        #region Properties

        public IEnumerable<ResourceFile> ResourceFiles => _resourceFiles;

        public TermHighlightQueue TermHighlightQueue { get; }

        public ResourceCore ResourceCore { get; set; }

        public List<ContentToHighlight> Content { get; set; }

        public HashSet<string> Words { get; set; }

        public HashSet<string> Keywords { get; set; }

        public string ResourceLocation => Path.Combine(_contentLocation, TermHighlightQueue.Isbn);

        public string OutputLocation => GetPath(_outputLocation);

        public string BackupLocation => GetPath(_backupLocation);

        public string TempLocation => Path.Combine(OutputLocation, "Temp");

        public int TotalFileCount { get; set; }

        public int HighlightedFileCount { get; set; }

        #endregion
    }

    public class ContentToHighlight : R2UtilitiesBase
    {
        private string _fileName;

        #region Methods

        public void Load(bool removeComments = false)
        {
            var xmlDoc = new XmlDocument
            {
                XmlResolver = null, /*Do this to ignore DTD errors*/
                PreserveWhitespace = true /*Do this to prevent auto-reformatting*/
            };

            var text = File.ReadAllText(ResourcePath);
            text = !IsIgnored ? PreFormat(text) : text;

            if (removeComments || XPathToStrip != null)
            {
                xmlDoc = LoadXmlDocument(xmlDoc, text);

                if (IsBook && removeComments) xmlDoc = XmlHelper.RemoveComments(xmlDoc);
                if (XPathToStrip != null) xmlDoc = XmlHelper.StripTags(xmlDoc, XPathToStrip);

                OutputContent = xmlDoc.OuterXml;
            }
            else
            {
                OutputContent = text;
            }

            Keywords = GetKeywords(xmlDoc);
        }

        public void WriteOutput()
        {
            OutputContent = !IsIgnored ? PostFormat(OutputContent) : OutputContent;
            File.WriteAllText(OutputPath, OutputContent);
        }

        private string PreFormat(string content)
        {
            return content.Replace("<?lb?>", "<lb/>")
                .Replace("<?lb ?>", "<lb />");
        }

        private string PostFormat(string content)
        {
            foreach (var entity in EntityValues.Keys)
            {
                var value = EntityValues[entity];
                content = content.Replace(value, entity);
            }

            return content.Replace("<lb/>", "<?lb?>")
                .Replace("<lb />", "<?lb ?>");
        }

        private XmlDocument LoadXmlDocument(XmlDocument xmlDoc, string text)
        {
            EntityValues.Clear();

            var isLoaded = false;
            while (!isLoaded)
            {
                try
                {
                    xmlDoc.Load(new StringReader(text));
                    isLoaded = true;
                }
                catch (XmlException ex)
                {
                    if (!ex.Message.Contains("undeclared entity"))
                    {
                        throw;
                    }

                    text = ReplaceEntity(text, ex.Message.Split('\'')[1]);
                }
            }

            return xmlDoc;
        }

        private string ReplaceEntity(string text, string entityName)
        {
            var entity = "&" + entityName + ";";
            var value = WebUtility.HtmlDecode(entity);

            if (string.Equals(entity, value)) value = "??" + entityName + "??";
            EntityValues.Add(entity, value);

            return text.Replace(entity, value);
        }

        private string GetXPathToStrip()
        {
            switch (TermHighlightType)
            {
                case TermHighlightType.Tabers:
                    return "//ulink[@type='tabers']";
                case TermHighlightType.IndexTerms:
                    return "//ulink[@type='disease' or @type='drug' or @type='drugsynonym' or @type='keywords']";
                default:
                    throw new Exception(
                        $"ResourceToHighlight error - Unexpected TermHighlightType: {TermHighlightType}");
            }
        }

        private static HashSet<string> GetKeywords(XmlDocument xmlDoc)
        {
            return new HashSet<string>(XmlHelper.GetXmlNodes(xmlDoc, "//risterm[../risrule[.='linkKeyword']]")
                .Select(n => n.InnerText.ToLower()).Distinct());
        }

        #endregion Methods

        #region Properties

        public string OutputContent { get; set; }
        public string TempContent { get; set; }

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                var parts = _fileName.Split('.');

                if (parts.Length > 3)
                {
                    SectionId = parts[2];
                    ChapterId = SectionId.Length >= 6 ? SectionId.Substring(0, 6) : SectionId;
                }
                else
                {
                    SectionId = _fileName;
                    ChapterId = _fileName;
                }
            }
        }

        public HashSet<string> Words { get; set; }
        public HashSet<string> Keywords { get; set; }

        public string ChapterId { get; private set; }
        public string SectionId { get; private set; }

        public bool IsIgnored
        {
            get { return new[] { "toc." }.Any(s => FileName.StartsWith(s)); }
        }

        public bool IsBook
        {
            get { return new[] { "book." }.Any(s => FileName.StartsWith(s)); }
        }

        public string ResourcePath { get; set; }
        public string OutputPath { get; set; }
        public string TempPath { get; set; }
        public string BackupPath { get; set; }

        public TermHighlightType TermHighlightType { get; set; }

        private string XPathToStrip => GetXPathToStrip();
        private Dictionary<string, string> EntityValues { get; } = new Dictionary<string, string>();

        #endregion Properties
    }
}