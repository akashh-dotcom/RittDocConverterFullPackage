#region

using System.Text;

#endregion

namespace R2V2.Web.Models.AlphaIndex
{
    public class AlphaQuery
    {
        public string Show { get; set; }
        public string Alpha { get; set; }
        public string PracticeArea { get; set; }
        public string Disciplines { get; set; }

        public int PracticeAreaId
        {
            get
            {
                int id;
                int.TryParse(PracticeArea, out id);
                return id;
            }
        }

        public string ToSearchFragment()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(PracticeArea))
            {
                sb.AppendFormat("practice-area={0}", PracticeArea);
            }

            if (!string.IsNullOrWhiteSpace(PracticeArea) && !string.IsNullOrWhiteSpace(Disciplines))
            {
                sb.Append("&");
            }

            if (!string.IsNullOrWhiteSpace(Disciplines))
            {
                sb.AppendFormat("disciplines={0}", Disciplines);
            }

            return sb.ToString();
        }
    }
}