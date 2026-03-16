#region

using System.Text;

#endregion

namespace R2Library.Data.ADO.R2.DataServices
{
    public class TabersDataService
    {
        //private static readonly string _termContentInflectional = new StringBuilder()
        //	.Append("select Term, Content, SectionId ")
        //	.Append("from TermContent ")
        //	.Append("where contains(term, 'formsof (inflectional, {0})') and len(term) <= len('{0}') ")
        //	.Append("order by difference(term, '{1}') DESC ")
        //	.ToString();

        //private static readonly string _termContentRoot = new StringBuilder()
        //	.Append("select Term, Content, SectionId ")
        //	.Append("from TermContent ")
        //	.Append("where term like '{1}%' ")
        //	.Append("and len(term) <= len('{0}') ")
        //	.Append("order by len('{0}') - len(Term) ")
        //	.ToString();

        //private static readonly string _termContentExtension = new StringBuilder()
        //	.Append("select Term, Content, SectionId ")
        //	.Append("from TermContent ")
        //	.Append("where term like '{1}%' ")
        //	.Append("order by len('{0}') - len(Term) DESC ")
        //	.ToString();

        private static readonly string TermContentInflectional = new StringBuilder()
            .Append("select vchTerm, vchContent, vchSectionId ")
            .Append("from tDictionaryTerm ")
            .Append("where contains(vchTerm, 'formsof (inflectional, {0})') and len(vchTerm) <= len('{0}') ")
            .Append("order by difference(vchTerm, '{1}') DESC ")
            .ToString();

        private static readonly string TermContentRoot = new StringBuilder()
            .Append("select vchTerm, vchContent, vchSectionId ")
            .Append("from tDictionaryTerm ")
            .Append("where vchTerm like '{1}%' ")
            .Append("and len(vchTerm) <= len('{0}') ")
            .Append("order by len('{0}') - len(vchTerm) ")
            .ToString();

        private static readonly string TermContentExtension = new StringBuilder()
            .Append("select vchTerm, vchContent, vchSectionId ")
            .Append("from tDictionaryTerm ")
            .Append("where vchTerm like '{1}%' ")
            .Append("order by len('{0}') - len(vchTerm) DESC ")
            .ToString();


        public TermContent SelectTermContentInflectional(string term, string stem)
        {
            //List<TermContent> termContents = GetEntityList<TermContent>(String.Format(TermContentInflectional, term, stem), null, false);

            //return termContents.Count > 0 ? termContents[0] : null;
            return null;
        }

        public TermContent SelectTermContentRoot(string term, string stem)
        {
            //List<TermContent> termContents = GetEntityList<TermContent>(String.Format(TermContentRoot, term, stem), null, false);

            //return termContents.Count > 0 ? termContents[0] : null;
            return null;
        }

        public TermContent SelectTermContentExtension(string term, string stem)
        {
            //List<TermContent> termContents = GetEntityList<TermContent>(String.Format(TermContentExtension, term, stem), null, false);

            //return termContents.Count > 0 ? termContents[0] : null;
            return null;
        }
    }
}