#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Infrastructure.Settings;
using R2V2.Extensions;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public interface ITermDataService
    {
        IEnumerable<TermToHighlight> SelectTermsToHighlight(HashSet<SearchTermItem> terms);
    }

    public abstract class TermDataService : DataServiceBase, ITermDataService
    {
        protected TermDataService(ITermHighlightSettings termHighlightSettings
            , string connectionString
        ) : base(connectionString)
        {
            _termHighlightSettings = termHighlightSettings;
        }

        #region Fields

        private static readonly string TermToHighlightExec = new StringBuilder()
            .Append("declare @searchTerms as dbo.SearchTermType ")
            .Append("{0} ")
            .Append("exec GetTermsToHighlight @searchTerms ")
            .ToString();

        private readonly ITermHighlightSettings _termHighlightSettings;

        #endregion Fields

        #region Methods

        public virtual IEnumerable<TermToHighlight> SelectTermsToHighlight(HashSet<SearchTermItem> terms)
        {
            var maxWordCount = _termHighlightSettings.MaxWordCountPerDataCall;
            IEnumerable<TermToHighlight> result = new List<TermToHighlight>();

            if (terms.Count == 0) return result;

            if (terms.Count > maxWordCount)
            {
                Log.WarnFormat("Warning: Word list contains {0} words!", terms.Count);

                result = terms.InSetsOf(maxWordCount)
                    .SelectMany(GetTermsToHighlight);
            }
            else
            {
                result = GetTermsToHighlight(terms);
            }

            return result;
        }

        IEnumerable<TermToHighlight> GetTermsToHighlight(IEnumerable<SearchTermItem> terms)
        {
            const string format = "insert into @searchTerms (searchTerm, isKeyword) values ('{0}', {1})\n";
            var inserts = terms.Select(term =>
                    string.Format(format, term.SearchTerm.Replace("'", "''"), Convert.ToInt32(term.IsKeyword)))
                .Aggregate(new StringBuilder(), (current, insert) => current.Append(insert)).ToString();

            return GetEntityList<TermToHighlight>(string.Format(TermToHighlightExec, inserts), null, false);
        }

        #endregion Methods
    }
}