#region

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public partial class LoadTabersDictionaryTask
    {
        private const string Isbn = "080362977X";
        private const string ContentLocation = @"\\technonase1\technotects\Clients\Rittenhouse\R2v2\Content\Dev";
        private readonly string Section = string.Format(ContentLocation + @"\html\{0}\sect1.{0}.{{0}}.html", Isbn);
        private readonly string Toc = string.Format(ContentLocation + @"\xml\{0}\toc.{0}.xml", Isbn);

        private void LoadTabersContent()
        {
            var doc = XDocument.Load(Toc);
            var tocEntries = doc.Descendants("tocchap").Descendants("tocentry")
                .Where(e => e.Parent.Name == "toclevel1");

            foreach (var tocEntry in tocEntries)
            {
                var term = tocEntry.Value.Trim();
                if (term == "") continue; //Toc actually contains blank terms!
                var sectionId = tocEntry.Attribute("linkend").Value;
                var content = TermContent(sectionId);

                if (content != null)
                    _tabersDataService.InsertTermContent(term, content, sectionId);
            }
        }

        private string TermContent(string termSectionId)
        {
            XDocument doc;
            var file = SectionFile(termSectionId);

            try
            {
                doc = XDocument.Load(file); //Load will fail if file contains '&' instead of '&amp;'
            }
            catch
            {
                //Encode unencoded ampersands
                var fileContent = File.ReadAllText(file);
                fileContent = EncodeUnencodedAmpersands(fileContent);

                doc = XDocument.Load(new StringReader(fileContent));
            }

            return TermContent(doc);
        }

        private static string TermContent(XContainer doc)
        {
            var header = doc.Descendants("h2").First(e => e.Parent.Name == "body").ToString();
            var definition = doc.Descendants("p").First(e => e.Parent.Name == "body").ToString();

            return header + definition;
        }

        private string SectionFile(string id)
        {
            return string.Format(Section, id);
        }

        private static string EncodeUnencodedAmpersands(string text)
        {
            return
                Regex.Replace(text, @"
					# Match & that is not part of an HTML entity.
					&                  # Match literal &.
					(?!                # But only if it is NOT...
					\w+;               # an alphanumeric entity,
					| \#[0-9]+;        # or a decimal entity,
					| \#x[0-9A-F]+;    # or a hexadecimal entity.
					)                  # End negative lookahead.",
                    "&amp;",
                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }
    }
}