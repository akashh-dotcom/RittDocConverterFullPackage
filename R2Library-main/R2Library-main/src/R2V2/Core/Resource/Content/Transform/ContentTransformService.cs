#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Resource.Content.Transform
{
    public class ContentTransformService
    {
        private readonly IContentSettings _contentSettings;
        private readonly ILog<ContentTransformer> _log;

        public ContentTransformService(ILog<ContentTransformer> log, IContentSettings contentSettings)
        {
            _log = log;
            _contentSettings = contentSettings;
        }

        public ITransformResult Transform(ContentType contentType, ResourceAccess resourceAccess, string baseUrl,
            bool email, string isbn, string section)
        {
            var contentInfo =
                new ContentInfo(isbn, section, contentType, _contentSettings, resourceAccess, email, null);

            if (resourceAccess != ResourceAccess.Allowed && contentType != ContentType.Navigation &&
                contentType != ContentType.TableOfContents)
            {
                _log.InfoFormat("Access denied, resourceAccess: {0}, contentType: {1}", resourceAccess, contentType);
                return null;
            }

            if (!File.Exists(contentInfo.XmlFilePath))
            {
                _log.WarnFormat("File not found: {0}", contentInfo.XmlFilePath);

                return null;
            }


            var output = Transform(contentInfo, contentType, resourceAccess, baseUrl, email, section);

            ITransformResult transformResult;
            if (contentType == ContentType.Navigation)
            {
                transformResult = new XmlTransformResult { Result = GetXmlDocument(output) };
            }
            else
            {
                transformResult = new HtmlTransformResult { Result = output };
            }

            transformResult.Isbn = isbn;
            transformResult.Section = section;
            transformResult.OutputFilename = contentInfo.HtmlFilePath;
            return transformResult;
        }


        private string Transform(ContentInfo contentInfo, ContentType contentType, ResourceAccess resourceAccess,
            string baseUrl, bool email, string section)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var xsltArgumentList = BuildXsltArgumentList(contentType, resourceAccess, baseUrl, email, section);

            using (var xmlReader = GetXmlReader(contentInfo.XmlFilePath))
            {
                var output = Transform(contentInfo, contentType, xmlReader, xsltArgumentList);

                xmlReader.Close();

                stopwatch.Stop();
                _log.DebugFormat("Transform() time: {0} ms, file: {1}", stopwatch.ElapsedMilliseconds,
                    contentInfo.XmlFilename);

                return output;
            }
        }


        private XsltArgumentList BuildXsltArgumentList(ContentType contentType, ResourceAccess resourceAccess,
            string baseUrl, bool email, string section)
        {
            var xsltArgumentList = new XsltArgumentList();
            xsltArgumentList.AddParam("email", "", email ? "1" : "0");
            xsltArgumentList.AddParam("version", "", "2.0.0.0");
            xsltArgumentList.AddParam("baseUrl", "", baseUrl);
            xsltArgumentList.AddParam("imageBaseUrl", "", _contentSettings.ImageBaseUrl);

            if (contentType == ContentType.Navigation)
            {
                xsltArgumentList.AddParam("objectid", "", section);
            }

            xsltArgumentList.AddParam("rootid", "",
                contentType == ContentType.Glossary || contentType == ContentType.Part ||
                contentType == ContentType.Bibliography
                    ? section
                    : "");

            if (contentType == ContentType.TableOfContents)
            {
                xsltArgumentList.AddParam("disablelinks", "", true);
                xsltArgumentList.AddParam("contentlinks", "", resourceAccess == ResourceAccess.Allowed);
            }

            return xsltArgumentList;
        }

        private string Transform(ContentInfo contentInfo, ContentType contentType, XmlReader xmlReader,
            XsltArgumentList xsltArgumentList)
        {
            var xmlTransform = new XslCompiledTransform(false);
            xmlTransform.Load(contentInfo.XslType);


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

            return output;
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

            return contentType == ContentType.Navigation ? null : s;
        }

        private static string CleanContent(string document)
        {
            //Remove Inappropriate Tags
            return
                Regex.Replace(document, @"<\?xml[^>]*>|<html>|</html>|<head>|</head>|<meta[^>]*>|<body[^>]*>|</body>",
                    "", RegexOptions.IgnoreCase);
        }

        private static XmlReader GetXmlReader(string xmlFilePath)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
            return XmlReader.Create(xmlFilePath, settings);
        }

        private static XmlDocument GetXmlDocument(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
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
            return new DateTime(2016, 1, 1);
        }
    }
}