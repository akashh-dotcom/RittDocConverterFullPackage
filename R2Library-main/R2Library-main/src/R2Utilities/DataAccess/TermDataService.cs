using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Infrastructure.Settings;
using R2V2.Extensions;

namespace R2Utilities.DataAccess
{
	public interface ITermDataService
	{
		IEnumerable<TermToHighlight> SelectTermsToHighlight(HashSet<string> words);
	}

	public abstract class TermDataService : DataServiceBase, ITermDataService
	{
		#region Fields
		private static readonly string TermToHighlightExec = new StringBuilder()
			.Append("declare @searchTerms as dbo.SearchTermType ")
			.Append("{0} ")
			.Append("exec GetTermsToHighlight @searchTerms ")
			.ToString();

		private readonly ITermHighlightSettings _termHighlightSettings;
		#endregion Fields

		protected TermDataService(ITermHighlightSettings termHighlightSettings
			, string connectionString
		): base (connectionString)
		{
			_termHighlightSettings = termHighlightSettings;
		}

		#region Methods
		public virtual IEnumerable<TermToHighlight> SelectTermsToHighlight(HashSet<string> words)
		{
			int maxWordCount = _termHighlightSettings.MaxWordCountPerDataCall;
			IEnumerable<TermToHighlight> result = new List<TermToHighlight>();

			if (words.Count == 0) return result;

			if (words.Count > maxWordCount)
			{
				Log.WarnFormat("Warning: Word list contains {0} words!", words.Count);

				result = words.InSetsOf(maxWordCount)
							  .SelectMany(GetTermsToHighlight);
			}
			else
			{
				result = GetTermsToHighlight(words);
			}

			return result;
		}

		IEnumerable<TermToHighlight> GetTermsToHighlight(IEnumerable<string> words)
		{
			string inserts = words.Select(word => "insert into @searchTerms (searchTerm) values ('" + word.Replace("'", "''") + "')\n")
					  .Aggregate("", (current, insert) => current + insert);

			return GetEntityList<TermToHighlight>(String.Format(TermToHighlightExec, inserts), null, false);
		}
		#endregion Methods
	}
}
