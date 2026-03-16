#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using R2V2.Contexts;
using R2V2.Core.Resource.Content.Transform;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Resource.Content
{
    public class ContentTransformer : IContentTransformer
    {
        private static DateTime _htmlCacheMinDate = DateTime.MinValue;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IContentSettings _contentSettings;
        private readonly ILog<ContentTransformer> _log;

        public ContentTransformer(ILog<ContentTransformer> log, IContentSettings contentSettings,
            IAuthenticationContext authenticationContext)
        {
            _log = log;
            _contentSettings = contentSettings;
            _authenticationContext = authenticationContext;
        }

        public string OutputFilename { get; set; }

        public string Isbn { get; set; }
        public string Section { get; set; }

        public ITransformResult Transform(ContentType contentType, ResourceAccess resourceAccess, string baseUrl,
            bool email)
        {
            var contentInfo = new ContentInfo(Isbn, Section, contentType, _contentSettings, resourceAccess, email,
                _authenticationContext);
            OutputFilename = contentInfo.HtmlFilePath;

            if (resourceAccess != ResourceAccess.Allowed && contentType != ContentType.Navigation &&
                contentType != ContentType.TableOfContents)
            {
                _log.InfoFormat("Access denied, resourceAccess: {0}, contentType: {1}", resourceAccess, contentType);
                return null;
            }

            if (!File.Exists(contentInfo.XmlFilePath))
            {
                _log.WarnFormat("File not found: {0}", contentInfo.XmlFilePath);

                using (var windowsIdentity = WindowsIdentity.GetCurrent())
                {
                    _log.DebugFormat("WindowsIdentity.GetCurrent().Name: {0}",
                        windowsIdentity != null ? windowsIdentity.Name : "null");
                }

                return null;
            }

            var transform = true;

            if (File.Exists(contentInfo.HtmlFilePath))
            {
                var htmlFileInfo = new FileInfo(contentInfo.HtmlFilePath);
                if (htmlFileInfo.LastWriteTime >
                    GetHtmlCacheMinDate(_contentSettings.MinTransformDate, _contentSettings.XslLocation))
                {
                    transform = false;
                }

                var xmlFileInfo = new FileInfo(contentInfo.XmlFilePath);
                if (xmlFileInfo.LastWriteTime > htmlFileInfo.LastWriteTime)
                {
                    transform = true;
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var output = transform
                ? Transform(contentInfo, contentType, resourceAccess, baseUrl, email)
                : GetCachedOutput(contentInfo.HtmlFilePath);

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > _contentSettings.TransformErrorThresholdInMilliseconds)
            {
                _log.ErrorFormat("Transform() time [error]: {0} ms, file: {1}, threshold set at {2} ms",
                    stopwatch.ElapsedMilliseconds, contentInfo.XmlFilename,
                    _contentSettings.TransformErrorThresholdInMilliseconds);
            }
            else if (stopwatch.ElapsedMilliseconds > _contentSettings.TransformWarnThresholdInMilliseconds)
            {
                _log.WarnFormat("Transform() time [warn]: {0} ms, file: {1}, threshold set at {2} ms",
                    stopwatch.ElapsedMilliseconds, contentInfo.XmlFilename,
                    _contentSettings.TransformWarnThresholdInMilliseconds);
            }
            else if (stopwatch.ElapsedMilliseconds > _contentSettings.TransformInfoThresholdInMilliseconds)
            {
                _log.InfoFormat("Transform() time [info]: {0} ms, file: {1}, threshold set at {2} ms",
                    stopwatch.ElapsedMilliseconds, contentInfo.XmlFilename,
                    _contentSettings.TransformInfoThresholdInMilliseconds);
            }

            if (contentType == ContentType.Navigation)
            {
                return new XmlTransformResult { Result = GetXmlDocument(output) };
            }

            return new HtmlTransformResult { Result = output, TransformTime = stopwatch.ElapsedMilliseconds };
        }

        private string Transform(ContentInfo contentInfo, ContentType contentType, ResourceAccess resourceAccess,
            string baseUrl, bool email)
        {
            var xsltArgumentList = BuildXsltArgumentList(contentType, resourceAccess, baseUrl, email);

            using (var xmlReader = GetXmlReader(contentInfo.XmlFilePath, contentType))
            {
                var output = Transform(contentInfo, contentType, xmlReader, xsltArgumentList);

                xmlReader.Close();


                return output;
            }
        }

        private string Transform(ContentInfo contentInfo, ContentType contentType, XmlReader xmlReader,
            XsltArgumentList xsltArgumentList)
        {
            var xmlTransform = new XslCompiledTransform(false);
            xmlTransform.Load(contentInfo.XslType);

            InitializeHtmlDirectory(contentInfo);

            string output;
            using (var memoryStream = new MemoryStream())
            {
                using (var streamReader = new StreamReader(memoryStream))
                {
                    using (var writer = XmlWriter.Create(memoryStream, GetXmlWriterSettings(contentType, xmlTransform)))
                    {
                        var xmlUrlResolver = new R2V2XmlUrlResolver(_contentSettings);

                        xmlTransform.Transform(xmlReader, xsltArgumentList, writer, xmlUrlResolver, true);

                        memoryStream.Position = 0;
                        output = CleanContent(streamReader.ReadToEnd());
                    }
                }
            }

            SaveOutput(contentInfo.HtmlFilePath, output);
            return output;
        }

        private XmlReader GetXmlReader(string xmlFilePath, ContentType contentType)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse, ValidationType = ValidationType.None,
                XmlResolver = new R2V2XmlEntityResolver(_contentSettings)
            };
            //_log.DebugFormat("GetXmlReader() - xmlFilePath: {0}", xmlFilePath);

            return XmlReader.Create(xmlFilePath, settings);
        }

        private static XmlWriterSettings GetXmlWriterSettings(ContentType contentType,
            XslCompiledTransform xmlTransform)
        {
            //Clone the existing settings in order to have the appropriate OutputMethod set [OutputMethod is set protected!]
            var s = xmlTransform.OutputSettings.Clone();
            s.OmitXmlDeclaration = true;
            s.CloseOutput = true;
            s.ConformanceLevel = ConformanceLevel.Document;
            s.Encoding = Encoding.UTF8;

            return
                contentType == ContentType.Navigation
                    ? null
                    : s;
        }

        private static void InitializeHtmlDirectory(ContentInfo contentInfo)
        {
            if (!Directory.Exists(contentInfo.HtmlDirectory))
            {
                Directory.CreateDirectory(contentInfo.HtmlDirectory);
            }
        }

        private XsltArgumentList BuildXsltArgumentList(ContentType contentType, ResourceAccess resourceAccess,
            string baseUrl, bool email)
        {
            var xsltArgumentList = new XsltArgumentList();
            xsltArgumentList.AddParam("email", "", email ? "1" : "0");
            xsltArgumentList.AddParam("version", "", "2.0.0.0");
            xsltArgumentList.AddParam("baseUrl", "", baseUrl);
            xsltArgumentList.AddParam("imageBaseUrl", "", _contentSettings.ImageBaseUrl);

            if (contentType == ContentType.Navigation)
            {
                xsltArgumentList.AddParam("objectid", "", Section);
            }

            xsltArgumentList.AddParam("rootid", "",
                contentType == ContentType.Glossary || contentType == ContentType.Part ||
                contentType == ContentType.Bibliography
                    ? Section
                    : "");

            if (contentType == ContentType.TableOfContents)
            {
                if (_authenticationContext.AuthenticatedInstitution == null)
                {
                    xsltArgumentList.AddParam("disablelinks", "", !_authenticationContext.IsAuthenticated);
                }
                else
                {
                    if (!_authenticationContext.IsAuthenticated)
                    {
                        xsltArgumentList.AddParam("disablelinks", "", true);
                    }
                    else
                    {
                        xsltArgumentList.AddParam("disablelinks", "",
                            !_authenticationContext.AuthenticatedInstitution.DisplayAllProducts &&
                            resourceAccess != ResourceAccess.Allowed);
                    }
                }

                xsltArgumentList.AddParam("contentlinks", "", resourceAccess == ResourceAccess.Allowed);
            }

            return xsltArgumentList;
        }

        private XmlDocument GetXmlDocument(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        private static string CleanContent(string document)
        {
            //Remove Inappropriate Tags
            return Regex.Replace(document,
                @"<\?xml[^>]*>|<html>|</html>|<head>|</head>|<meta[^>]*>|<body[^>]*>|</body>", "",
                RegexOptions.IgnoreCase);
        }

        private void SaveOutput(string filename, string output)
        {
            try
            {
                File.WriteAllText(filename, output);
            }
            catch (Exception ex)
            {
                // swallow -
                _log.Warn(ex.Message, ex);
            }
        }

        private string GetCachedOutput(string filename)
        {
            try
            {
                return File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                // swallow -
                _log.Error(ex.Message, ex);
                return string.Empty;
            }
        }

        private DateTime GetHtmlCacheMinDate(DateTime configValue, string xslDllPath)
        {
            _log.DebugFormat("configValue: {0}, xslDllPath: {1}, _htmlCacheMinDate: {2}", configValue, xslDllPath,
                _htmlCacheMinDate);
            if (_htmlCacheMinDate > DateTime.MinValue)
            {
                return _htmlCacheMinDate;
            }

            if (configValue > DateTime.Now)
            {
                _htmlCacheMinDate = configValue;
                return _htmlCacheMinDate;
            }

            _htmlCacheMinDate = configValue;
            var directoryInfo = new DirectoryInfo(xslDllPath);
            if (directoryInfo.Exists)
            {
                var files = directoryInfo.GetFiles();
                foreach (var fileInfo in files.Where(fileInfo =>
                             fileInfo.Extension == ".dll" && fileInfo.LastWriteTime > _htmlCacheMinDate))
                {
                    _htmlCacheMinDate = fileInfo.LastWriteTime;
                }
            }

            return _htmlCacheMinDate;
        }

        private bool IgnoreExternalEntities(ContentType contentType)
        {
            return _contentSettings.IgnoreExternalEntities &&
                   (contentType == ContentType.Part || contentType == ContentType.Book);
        }
    }
}