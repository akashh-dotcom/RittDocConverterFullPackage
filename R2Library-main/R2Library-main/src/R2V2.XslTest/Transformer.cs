#region

using System.IO;
using System.Text;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Content;

#endregion

namespace R2V2.XslTest
{
    internal static class Transformer
    {
        private const int Isbn = 1;

        private const int Section = 2;

        ////private static readonly string BaseUrl = ConfigurationManager.AppSettings["App.BaseUrl"];
        private static string _baseUrl;

        private static string[] FileName { get; set; }

        internal static void WriteAllText(string path, string baseUrl)
        {
            _baseUrl = baseUrl;
            var fileName = Path.GetFileName(path);

            File.WriteAllText(
                fileName + ".html",
                Shell.Wrap(Transform(fileName)),
                Encoding.Unicode
            );
        }

        internal static string Transform(string fileName)
        {
            FileName = fileName.TrimEnd("xml").Split('.');

            return Transform(
                FileName[Isbn],
                FileName[Section],
                _baseUrl);
        }

        private static string Transform(string isbn, string section, string baseUrl)
        {
            var service = ServiceLocator.Current.GetInstance<IContentService>();

            var content =
                section == ""
                    ? service.GetTableOfContents(isbn, baseUrl, ResourceAccess.Allowed, false)
                    : service.GetContent(isbn, section, baseUrl, ResourceAccess.Allowed, false);

            return content.Html;
        }
    }
}