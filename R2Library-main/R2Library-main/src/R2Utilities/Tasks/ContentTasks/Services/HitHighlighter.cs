#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using dtSearch.Engine;
using R2Utilities.DataAccess.Terms;
using R2Utilities.Infrastructure.Settings;
using R2V2.Extensions;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class HitHighlighter : R2UtilitiesBase
    {
        public HitHighlighter(IContentSettings contentSettings
            , TermResourceDataService termResourceDataService
        )
        {
            _contentSettings = contentSettings;
            _termResourceDataService = termResourceDataService;
            _documentIds = new Dictionary<string, int>();
            _termsToHighlight = new List<TermToHighlight>();
            //_wordsByFileName = new Dictionary<string, HashSet<string>>();
            _termResources = new HashSet<TermResource>();
        }

        #region Properties

        public ResourceToHighlight ResourceToHighlight { get; private set; }

        #endregion Properties

        #region Fields

        private const SearchFlags HighlightingSearchFlags = SearchFlags.dtsSearchTypeAnyWords;
        private const int TermResourceInsertSetSize = 262;

        private string _indexLocation; //Index required for WordListBuilder

        private ContentToHighlight _content;

        private int _currentDocId;
        private readonly Dictionary<string, int> _documentIds;
        private List<TermToHighlight> _termsToHighlight;

        private TermToHighlight _termToHighlight;

        //readonly Dictionary<string, HashSet<string>> _wordsByFileName;
        private readonly HashSet<TermResource> _termResources;
        private string _taskName;
        private bool _foundTerm;

        private readonly IContentSettings _contentSettings;
        private TermHighlightType _termHighlightType;
        private readonly TermResourceDataService _termResourceDataService;
        private ITermDataService _termDataService;

        #endregion

        #region Methods

        public void Init(ITermHighlightSettings termHighlightSettings, ITermDataService termDataService,
            string taskName)
        {
            _termHighlightType = termHighlightSettings.TermHighlightType;
            _indexLocation = termHighlightSettings.IndexLocation;
            _termDataService = termDataService;
            _taskName = taskName;
        }

        public void HighlightResource(ResourceToHighlight resource)
        {
            ClearCollections();

            Log.InfoFormat("\nResource: {0}{1}", resource.TermHighlightQueue.Isbn, " --------------------");

            ResourceToHighlight = resource;

            //Copy backup before processing!! Process may output to input directory!!
            Log.Info("Copying files to backup");
            ResourceToHighlight.WriteResourceBackup();
            Log.Info("Zipping backup");
            ResourceToHighlight.ZipResourceBackup();
            Log.Info("Loading content to memory");
            ResourceToHighlight.LoadContent(true);

            SetOptions();
            Log.Info("Building index");
            BuildIndex();

            ResourceToHighlight.Words = GetWordsFromResource();
            GetTermsToHighlight();

            var fileCount = 0;
            Log.Info("Begin Highlighting-------");
            Log.InfoFormat("{0} file to process", ResourceToHighlight.Content.Count);
            foreach (var content in ResourceToHighlight.Content)
            {
                fileCount++;
                _content = content;

                Log.InfoFormat("File: {0}, {1} of {2}", _content.FileName, fileCount,
                    ResourceToHighlight.Content.Count);

                if (!_content.IsIgnored) HighlightFile();

                _content.WriteOutput();

                ResourceToHighlight.HighlightedFileCount++;
            }

            InsertTermResources();
            InsertAtoZIndex();
        }

        void HighlightFile()
        {
            IEnumerable<TermToHighlight> termsToHighlight = _termsToHighlight.Where(IsCandidateTerm)
                .GroupBy(t => t.Text)
                .Select(t => t.First()).ToList();

            //Log.Info("[Text Count: " + termsToHighlight.Count() + "]");
            foreach (var termToHighlight in termsToHighlight)
            {
                _termToHighlight = termToHighlight;
                HighlightText(termToHighlight.Text);
            }
        }

        bool IsCandidateTerm(TermToHighlight term)
        {
            switch (term.TermType)
            {
                case TermType.Keyword:
                    return _content.Keywords.Contains(term.Text);
                default:
                    return _content.Words.Contains(term.Word) && !_content.Keywords.Contains(term.Text);
            }
        }

        void HighlightText(string term)
        {
            _foundTerm = false;
            var pattern = $@"(?<=\>[^<]*)(\b{Regex.Escape(term)}\b)";

            _content.OutputContent =
                Regex.Replace(_content.OutputContent, pattern, MatchEvaluator, RegexOptions.IgnoreCase);

            if (_foundTerm) AddTermResource();
        }

        //Used as delegate, not sure if it is safe to make this static
        string MatchEvaluator(Match match)
        {
            _foundTerm = true;

            return _termToHighlight.Highlight(match.Groups[0].ToString());
        }

        void AddTermResource()
        {
            if (_termHighlightType != TermHighlightType.IndexTerms) return;

            var termResource = _termToHighlight.ToTermResource();
            termResource.ChapterId = _content.ChapterId;
            termResource.SectionId = _content.SectionId;
            termResource.CreatorId = _taskName;
            termResource.TermId = _termToHighlight.TermId;
            //termResource.Term = _termToHighlight.Term;
            termResource.ResourceIsbn = ResourceToHighlight.ResourceCore.Isbn;
            termResource.Title = ResourceToHighlight.ResourceCore.Title;

            _termResources.Add(termResource);
        }

        HashSet<string> GetWordsFromResource()
        {
            var resourceWords = new HashSet<string>();

            var fileCount = 0;
            foreach (var content in ResourceToHighlight.Content)
            {
                fileCount++;

                using (var wlb = new WordListBuilder())
                {
                    using (var filter = new SearchFilter())
                    {
                        var indexId = filter.AddIndex(_indexLocation);

                        if (content.IsIgnored)
                        {
                            Log.InfoFormat("Ignoring file: {0}, {1} of {2}", content.FileName, fileCount,
                                ResourceToHighlight.Content.Count);
                            continue;
                        }

                        Log.InfoFormat("Scraping words from file: {0}, {1} of {2}", content.FileName, fileCount,
                            ResourceToHighlight.Content.Count);

                        _currentDocId = _documentIds[content.FileName];

                        filter.SelectItems(indexId, _currentDocId, _currentDocId, true);
                        wlb.SetFilter(filter);

                        wlb.OpenIndex(_indexLocation);

                        wlb.ListMatchingWords(".*", int.MaxValue, HighlightingSearchFlags, 0);

                        Log.DebugFormat("word count: {0}", wlb.Count);

                        if (wlb.Count == 0)
                        {
                            Log.Warn("No words found in resource file!");
                        }

                        var words = new HashSet<string>();

                        for (var n = 0; n < wlb.Count; n++)
                        {
                            var word = wlb.GetNthWord(n);
                            words.Add(word);
                        }

                        content.Words = words;
                        resourceWords.UnionWith(words);
                    }
                }
            }

            return resourceWords;
        }

        void GetTermsToHighlight()
        {
            Log.Info("Making data call for all terms in resource");
            var searchTerms = BuildSearchTerms();

            var termsToHighlight = _termDataService.SelectTermsToHighlight(searchTerms);

            _termsToHighlight = termsToHighlight.OrderByDescending(t => t.Rank)
                .GroupBy(t => new { t.Word, t.Text, t.TermType })
                .Select(t => t.First())
                .Where(t => !t.IsCompound || Regex.Split(t.Text, @"\W+").All(ResourceToHighlight.Words.Contains))
                .OrderByDescending(t => t.Text.Length)
                .ToList();
        }

        HashSet<SearchTermItem> BuildSearchTerms()
        {
            var searchTerms = SearchTerm.HashSet(ResourceToHighlight.Words, false);
            searchTerms.UnionWith(SearchTerm.HashSet(ResourceToHighlight.Keywords, true));

            return searchTerms;
        }

        void InsertTermResources()
        {
            if (_termHighlightType != TermHighlightType.IndexTerms) return;
            Log.Info("Update Term Resource Tables-------");
            _termResourceDataService.InactivateTermResources(ResourceToHighlight.TermHighlightQueue.Isbn);
            var count = 0;
            _termResources.InSetsOf(TermResourceInsertSetSize)
                .ForEach(set => count += _termResourceDataService.InsertTermResources(set));
            Log.InfoFormat("Total records inserted: {0}\n", count);
        }

        void InsertAtoZIndex()
        {
            if (_termHighlightType != TermHighlightType.IndexTerms) return;
            Log.Info("Insert A to Z Index-------");
            _termResourceDataService.DeleteAtoZIndex(ResourceToHighlight.TermHighlightQueue.Isbn);
            var count = _termResourceDataService.InsertAtoZIndex(ResourceToHighlight.ResourceCore.Id);
            Log.InfoFormat("Total records inserted: {0}", count);
        }

        void BuildIndex()
        {
            //Log.Info("Building Term Index");

            using (var ij = new IndexJob())
            {
                ij.IndexPath = _indexLocation;
                ij.FoldersToIndex.Add(ResourceToHighlight.ResourceLocation);
                ij.ActionAdd = true;
                ij.ActionCreate = true;
                //ij.ActionCompress = true;
                ij.StatusHandler = new StatusHandler(_documentIds);

                if (!ij.Execute())
                    ThrowError("Index Job", ij.Errors.Message(0));
            }
        }

        void SetOptions()
        {
            // Set the HomeDir in Options, or stemming won't work 
            // (stemming needs to find the stemming.dat file)

            //Log.Info("Setting dtSearch Options for Hit Highlighting\n\n");

            using (var options = new Options())
            {
                options.HomeDir = _contentSettings.DtSearchBinLocation;
                options.FieldFlags = FieldFlags.dtsoFfXmlSkipAttributes | FieldFlags.dtsoFfXmlHideFieldNames;
                options.Hyphens = HyphenSettings.dtsoHyphenAsHyphen;
                options.BooleanConnectors = "";
                options.Save();
            }
        }

        void ClearCollections()
        {
            _documentIds.Clear();
            _termsToHighlight.Clear();
            //_wordsByFileName.Clear();
            _termResources.Clear();
        }

        void ThrowError(string failedAction, string errorDetail)
        {
            var message =
                $"Hit Highlighting Failed During {failedAction} for File: {_content.FileName}\nError Detail:{errorDetail}";
            throw new Exception(message);
        }

        #endregion Methods
    }

    class StatusHandler : IIndexStatusHandler
    {
        public StatusHandler(Dictionary<string, int> documentIds)
        {
            DocumentIds = documentIds;
        }

        Dictionary<string, int> DocumentIds { get; }

        void IIndexStatusHandler.OnProgressUpdate(IndexProgressInfo info)
        {
            if (info.UpdateType != MessageCode.dtsnIndexFileDone)
            {
                return;
            }

            DocumentIds.Add(info.File.DisplayName, info.File.DocId);
        }

        AbortValue IIndexStatusHandler.CheckForAbort()
        {
            return AbortValue.Continue;
        }
    }
}