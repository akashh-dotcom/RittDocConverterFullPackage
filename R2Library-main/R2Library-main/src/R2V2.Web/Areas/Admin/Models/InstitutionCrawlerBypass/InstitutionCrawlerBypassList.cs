#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionCrawlerBypass
{
    public class InstitutionCrawlerBypassList : AdminBaseModel
    {
        public InstitutionCrawlerBypassList()
        {
        }

        public InstitutionCrawlerBypassList(IAdminInstitution institution,
            IEnumerable<Core.Institution.InstitutionCrawlerBypass> dbInstitutionWebCrawlerList)
            : base(institution)
        {
            InstitutionCrawlerBypassModels = new List<InstitutionCrawlerBypassModel>();
            foreach (var dbInstitutionTrustedAuthentication in dbInstitutionWebCrawlerList)
            {
                InstitutionCrawlerBypassModels.Add(
                    new InstitutionCrawlerBypassModel(dbInstitutionTrustedAuthentication));
            }
        }

        public List<InstitutionCrawlerBypassModel> InstitutionCrawlerBypassModels { get; set; }
    }
}