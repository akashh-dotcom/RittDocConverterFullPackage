#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionCrawlerBypass
{
    public class InstitutionCrawlerBypassDetail : AdminBaseModel
    {
        public InstitutionCrawlerBypassDetail()
        {
        }

        public InstitutionCrawlerBypassDetail(IAdminInstitution institution) : base(institution)
        {
        }

        public InstitutionCrawlerBypassDetail(IAdminInstitution institution,
            Core.Institution.InstitutionCrawlerBypass dbInstitutionCrawlerBypass) : base(institution)
        {
            InstitutionCrawlerBypass = new InstitutionCrawlerBypassModel(dbInstitutionCrawlerBypass);
        }

        public InstitutionCrawlerBypassModel InstitutionCrawlerBypass { get; set; }
    }
}