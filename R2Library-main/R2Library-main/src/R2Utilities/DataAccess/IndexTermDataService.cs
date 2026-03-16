using R2Utilities.Infrastructure.Settings;

namespace R2Utilities.DataAccess
{
	public class IndexTermDataService : TermDataService
	{
		public IndexTermDataService(IR2UtilitiesSettings r2UtilitiesSettings
			, IndexTermHighlightSettings indexTermHighlightSettings	/*This needs to be of type IndexTermHighlightSettings, not simply ITermHighlightSettings*/
		): base(indexTermHighlightSettings, r2UtilitiesSettings.R2DatabaseConnection)
		{
		}
	}
}
