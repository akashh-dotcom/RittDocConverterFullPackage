#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.WebServiceAuthentication
{
    public class InstitutionWebServiceAuthenticationList : AdminBaseModel
    {
        public InstitutionWebServiceAuthenticationList()
        {
        }

        public InstitutionWebServiceAuthenticationList(IAdminInstitution institution,
            IEnumerable<Core.Institution.WebServiceAuthentication> dbInstitutionTrustedAuthentications)
            : base(institution)
        {
            TrustedAuths = new List<InstitutionWebServiceAuthentication>();
            foreach (var dbInstitutionTrustedAuthentication in dbInstitutionTrustedAuthentications)
            {
                TrustedAuths.Add(new InstitutionWebServiceAuthentication(dbInstitutionTrustedAuthentication));
            }
        }

        public List<InstitutionWebServiceAuthentication> TrustedAuths { get; set; }
    }
}