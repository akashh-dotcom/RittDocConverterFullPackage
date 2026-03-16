using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

namespace R2Utilities.DataAccess
{
	public class TermToHighlight : FactoryBase, IDataEntity, IEquatable<TermToHighlight>
	{
		static TermToHighlight()
		{
			LoadTags();
		}

		#region Methods
		private static void LoadTags()
		{
			Tag = new Dictionary<TermType, HitTag>();

			Tag[TermType.Tabers] = new HitTag
			{
				Open = "<ulink type=\"tabers\" termId=\"{0}\">",
				Close = "</ulink>"
			};

			Tag[TermType.Disease] = new HitTag
			{
				Open = "<ulink type=\"disease\" url=\"link.aspx?id={0}\">",
				Close = "</ulink>"
			};

			Tag[TermType.Drug] = new HitTag
			{
				Open = "<ulink type=\"drug\" url=\"link.aspx?id={0}\">",
				Close = "</ulink>"
			};

			Tag[TermType.DrugSynonym] = new HitTag
			{
				Open = "<ulink type=\"drugsynonym\" url=\"/search/search_results_index.aspx?searchterm={0}\">",
				Close = "</ulink>"
			};

			Tag[TermType.Keyword] = new HitTag
			{
				Open = "<ulink type=\"keywords\" url=\"/search/search_results_index.aspx?searchterm={0}\">",
				Close = "</ulink>"
			};
		}

		public void Populate(SqlDataReader reader)
		{
			try
			{
				Word = GetStringValue(reader, "word");
				TermId = GetInt32Value(reader, "id", 0);
				Term = GetStringValue(reader, "term");
				Text = IsCompound ? Term : GetStringValue(reader, "word");
				TermType = GetEnumValue<TermType>(reader, "termType");
			}
			catch (Exception ex)
			{
				Log.ErrorFormat(ex.Message, ex);
				throw;
			}
		}

		public string Highlight(string text)
		{
			return String.Format(Tag[TermType].Open, LinkValue)
					+ text
					+ Tag[TermType].Close;
		}

		public bool Equals(TermToHighlight other)
		{
			return Word == other.Word && Text == other.Text && TermId == other.TermId;
		}

		public override int GetHashCode()
		{
			int hashWord = Word == null ? 0 : Word.GetHashCode();
			int hashText = Text == null ? 0 : Text.GetHashCode();
			int hashTermContentKey = TermId.GetHashCode();

			return hashWord ^ hashText ^ hashTermContentKey;
		}
		#endregion Methods

		#region Properties
		public string Word { get; set; }
		public string Text { get; set; }
		public int TermId { get; set; }
		public string Term { get; set; }
		public TermType TermType { get; set; }
		private string LinkValue
		{
			get
			{
				return TermType == TermType.DrugSynonym || TermType == TermType.Keyword ? Term : TermId.ToString();
			}
		}
		public int Rank
		{
			get
			{
				if (IsCompound || Text == Term) return 1;
				return 0;
			}
		}
		public bool IsCompound
		{
			get { return Term.Contains(" ") || Term.Contains("-"); }
		}
		private static Dictionary<TermType, HitTag> Tag { get; set; }
		#endregion

		private class HitTag
		{
			public string Open;
			public string Close;
		}
	}

	public enum TermType
	{
		Tabers, Disease, Drug, DrugSynonym, Keyword
	}
}
