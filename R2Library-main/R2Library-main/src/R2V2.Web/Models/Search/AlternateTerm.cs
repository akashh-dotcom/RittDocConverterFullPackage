namespace R2V2.Web.Models.Search
{
    public class AlternateTerm
    {
        public AlternateTerm(string term)
        {
            Term = term;
        }

        public string Term { get; private set; }
    }
}