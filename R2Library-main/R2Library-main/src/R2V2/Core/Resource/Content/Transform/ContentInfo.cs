#region

using System;
using System.IO;
using System.Reflection;
using System.Text;
using R2V2.Contexts;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Resource.Content.Transform
{
    class ContentInfo
    {
        public ContentInfo(string isbn, string section, ContentType contentType, IContentSettings contentSettings,
            ResourceAccess resourceAccess, bool email, IAuthenticationContext authenticationContext)
        {
            //_isbn = isbn;
            //_section = section;
            //_contentType = contentType;

            ContentSource = GetContentSource(contentType, section);

            XmlFilename = GetXmlFilename(ContentSource, isbn, section);

            XslType = GetXslType(contentType, contentSettings.XslLocation);

            if (contentType == ContentType.TableOfContents)
            {
                if (authenticationContext.AuthenticatedInstitution == null)
                {
                    DisableLinks = !authenticationContext.IsAuthenticated;
                }
                else
                {
                    if (!authenticationContext.IsAuthenticated)
                    {
                        DisableLinks = true;
                    }
                    else
                    {
                        DisableLinks = !authenticationContext.AuthenticatedInstitution.DisplayAllProducts &&
                                       resourceAccess != ResourceAccess.Allowed;
                    }
                }

                ContentLinks = resourceAccess == ResourceAccess.Allowed;
            }

            XmlFilePath = $@"{contentSettings.ContentLocation}\{isbn}\{XmlFilename}";

            HtmlDirectory = $@"{contentSettings.NewContentLocation}\Html\{isbn}";

            HtmlFilePath = contentType == ContentType.TableOfContents
                ? $@"{contentSettings.NewContentLocation}\Html\{isbn}\{section}-{contentType}-{resourceAccess}-{(email ? "email" : "noemail")}-{(DisableLinks ? "linksDisabled" : "linksEnabled")}-{(ContentLinks ? "contentLinksTrue" : "contentLinksFalse")}.html"
                : $@"{contentSettings.NewContentLocation}\Html\{isbn}\{section}-{contentType}.html";
        }
        //private readonly string _isbn;
        //private readonly string _section;
        //private readonly ContentType _contentType;

        public string XmlFilename { get; }

        public ContentSource ContentSource { get; }

        public Type XslType { get; private set; }

        public string XmlFilePath { get; private set; }

        public string HtmlDirectory { get; private set; }

        public string HtmlFilePath { get; private set; }

        public bool DisableLinks { get; }
        public bool ContentLinks { get; }

        public static string GetXmlFilename(ContentSource contentSource, string isbn, string section)
        {
            switch (contentSource)
            {
                case ContentSource.TableOfContents:
                    return $"toc.{isbn}.xml";

                case ContentSource.Preface:
                    return $"preface.{isbn}.{section}.xml";

                case ContentSource.Glossary:
                case ContentSource.Part:
                case ContentSource.Bibliography:
                    return $"book.{isbn}.xml";

                case ContentSource.Dedication:
                    return $"dedication.{isbn}.{section}.xml";

                case ContentSource.Appendix:
                    return $"appendix.{isbn}.{section}.xml";

                // case ContentSource.Book:
                default:
                    return $"sect1.{isbn}.{section}.xml";
            }
        }


        public static ContentSource GetContentSource(ContentType contentType, string section)
        {
            if (contentType == ContentType.Navigation)
            {
                return ContentSource.TableOfContents;
            }

            var typeCode = string.IsNullOrWhiteSpace(section) ? "" : section.Substring(0, 2).ToLower();
            switch (typeCode)
            {
                case "ap":
                    return ContentSource.Appendix;

                case "dd":
                case "de":
                    return ContentSource.Dedication;

                case "gl":
                    return ContentSource.Glossary;

                case "pt":
                    return section != null && (
                        (section.Length > 6 && section.Substring(6, 2) != "sp")
                        || (section.Length > 2 && section.ToLower().StartsWith("pte"))
                    )
                        ? ContentSource.Book
                        : ContentSource.Part;

                case "pr":
                    return ContentSource.Preface;

                case "bi":
                    return section != null && section.Length > 4 && section.StartsWith("bibs")
                        ? ContentSource.Book
                        : ContentSource.Bibliography;

                case "":
                    return ContentSource.TableOfContents;

                default:
                    return ContentSource.Book;
            }
        }

        public static Type GetXslType(ContentType contentType, string xslLocation)
        {
            string assemblyName;
            switch (contentType)
            {
                case ContentType.TableOfContents:
                    assemblyName = "ritttoc";
                    break;

                case ContentType.Navigation:
                    assemblyName = "rittnav";
                    break;

                //case ContentType.Part:
                //case ContentType.Preface:
                //case ContentType.Dedication:
                //case ContentType.Appendix:
                //case ContentType.Book:
                default:
                    assemblyName = "RittBook";
                    break;
            }

            var assemblyFullPath = Path.Combine(xslLocation, $"{assemblyName}.dll");

            try
            {
                return Assembly.LoadFile(assemblyFullPath).GetType(assemblyName);
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder()
                    .AppendFormat("EXCEPTION: {0}", ex.Message)
                    .AppendLine()
                    .AppendFormat("assemblyName: {0}", assemblyName)
                    .AppendFormat("assemblyFullPath: {0}", assemblyFullPath)
                    .AppendLine();
                //_log.Error(msg.ToString(), ex);

                throw;
            }
        }
    }
}