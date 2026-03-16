#region

using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NHibernate.Linq;
using NHibernate.Transform;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Tabers
{
    public class TabersDictionaryService : ITabersDictionaryService
    {
        private const int MaxTextLength = 300;
        private const string TabersDictionaryIsbn = "080362977X";
        private const string TermFormat = "<div>{0}</div>";
        private const string TermLink = "<a href='/resource/detail/" + TabersDictionaryIsbn + "/{0}'>{1}</a>";

        private const string TermSearch =
            "<p class='tabers-search'>Search for \"<a href='/search?q=%%term%%'>%%term%%</a>\"</p>";

        private const string TermFooter = "<p><em>Powered by <a href='/resource/title/" + TabersDictionaryIsbn +
                                          "'>Taber's Dictionary</a></em></p>";


        private static readonly string TermContentInflectional = new StringBuilder()
            .Append("select vchTerm as [Term], vchContent as [Content], vchSectionId as [SectionId] ")
            .Append("from tDictionaryTerm ")
            .Append("where contains(vchTerm, 'formsof (inflectional, {0})') and len(vchTerm) <= len('{0}') ")
            .Append("order by difference(vchTerm, '{1}') DESC ")
            .ToString();

        private static readonly string TermContentRoot = new StringBuilder()
            .Append("select vchTerm as [Term], vchContent as [Content], vchSectionId as [SectionId] ")
            .Append("from tDictionaryTerm ")
            .Append("where vchTerm like '{1}%' ")
            .Append("and len(vchTerm) <= len('{0}') ")
            .Append("order by len('{0}') - len(vchTerm) ")
            .ToString();

        private static readonly string TermContentExtension = new StringBuilder()
            .Append("select vchTerm as [Term], vchContent as [Content], vchSectionId as [SectionId] ")
            .Append("from tDictionaryTerm ")
            .Append("where vchTerm like '{1}%' ")
            .Append("order by len('{0}') - len(vchTerm) DESC ")
            .ToString();

        //private readonly IQueryable<TermContent> _termContent;
        private readonly IQueryable<DictionaryTerm> _dictionaryTerms;

        private readonly ILog<TabersDictionaryService> _log;

        private readonly IQueryable<MainEntry> _mainEntries;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public TabersDictionaryService(ILog<TabersDictionaryService> log
            , IQueryable<MainEntry> mainEntries
            , IQueryable<DictionaryTerm> dictionaryTerms
            , IUnitOfWorkProvider unitOfWorkProvider
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
        )
        {
            _log = log;
            _mainEntries = mainEntries;
            _dictionaryTerms = dictionaryTerms;
            _unitOfWorkProvider = unitOfWorkProvider;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
        }

        public IMainEntry GetMainEntry(string name)
        {
            return _mainEntries
                .FetchMany(x => x.Senses)
                .First(x => x.Name == name);
        }

        public ITermContent GetTermContent(int termId)
        {
            ITermContent
                termContent = _dictionaryTerms.FirstOrDefault(dt => dt.Id == termId) /*?? GetTermContentFuzzy(term)*/;

            if (termContent == null)
            {
                _log.ErrorFormat("Taber's term not found - TermContentId: {0}", termId);
            }
            else
            {
                termContent.Content = FormatContent(termContent.Content, termContent.SectionId);
            }

            return termContent;
        }

        public ITermContent GetTermContentFuzzy(string term)
        {
            var stem = new Porter2().stem(term);

            var t = SelectTermContentInflectional(term, stem)
                    ?? SelectTermContentRoot(term, stem)
                    ?? SelectTermContentExtension(term, stem);

            //return new DictionaryTerm { Term = t.Term, Content = t.Content, SectionId = t.SectionId };
            return t;
        }

        private static string FormatContent(string content, string termSectionId)
        {
            content = string.Format(TermFormat, content);

            var doc = XDocument.Parse(content);

            var h2 = doc.Descendants("h2").First();
            var p = doc.Descendants("p").First();

            var termLink = XDocument.Parse(string.Format(TermLink, termSectionId, "")).Root;
            var moreLink = XDocument.Parse(string.Format(TermLink, termSectionId, "More")).Root;
            var space = new XText(" ");

            var term = h2.Descendants("a").FirstOrDefault() != null
                ? h2.Descendants("a").First().DescendantNodes()
                : h2.DescendantNodes();

            if (termLink != null)
            {
                termLink.Add(term);
                h2.ReplaceAll(termLink);
            }

            p.Descendants("a").ToList().ForEach(a => a.ReplaceWith(a.DescendantNodes()));
            p = LimitContentLength(p, MaxTextLength);
            p.Add(space);
            p.Add(moreLink);

            var header = h2.ToString();
            var definition = p.ToString();

            return header + TermSearch + definition + TermFooter;
        }

        private static XElement LimitContentLength(XElement content, int maxLength)
        {
            var length = 0;
            foreach (var node in content.Nodes())
            {
                if (node.NodeType != XmlNodeType.Text) continue;
                var text = (XText)node;
                var total = text.Value.Length + length;

                if (total > maxLength)
                {
                    text.Value = text.Value.Substring(0, maxLength - length) + "...";
                    node.NodesAfterSelf().Remove();
                    break;
                }

                length += text.Value.Length;
            }

            return content;
        }

        /// <returns>bool</returns>
        public bool IsFullTextAvailable()
        {
            var resourceId = _resourceService.GetResource(TabersDictionaryIsbn).Id;
            return _resourceAccessService.IsFullTextAvailable(resourceId);
        }


        public ITermContent SelectTermContentInflectional(string term, string stem)
        {
            //List<TermContent> termContents = GetEntityList<TermContent>(String.Format(TermContentInflectional, term, stem), null, false);
            //return termContents.Count > 0 ? termContents[0] : null;
            return ExecuterTermContentQuery(string.Format(TermContentInflectional, term, stem));
        }

        public ITermContent SelectTermContentRoot(string term, string stem)
        {
            //List<TermContent> termContents = GetEntityList<TermContent>(String.Format(TermContentRoot, term, stem), null, false);
            //return termContents.Count > 0 ? termContents[0] : null;
            return ExecuterTermContentQuery(string.Format(TermContentRoot, term, stem));
        }

        public ITermContent SelectTermContentExtension(string term, string stem)
        {
            //List<TermContent> termContents = GetEntityList<TermContent>(String.Format(TermContentExtension, term, stem), null, false);
            //return termContents.Count > 0 ? termContents[0] : null;
            return ExecuterTermContentQuery(string.Format(TermContentExtension, term, stem));
        }

        private ITermContent ExecuterTermContentQuery(string sql)
        {
            _log.DebugFormat("sql: {0}", sql);
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(DictionaryTerm)))
                    .List<DictionaryTerm>();

                return results.Count > 0 ? results[0] : null;
            }
        }
    }
}