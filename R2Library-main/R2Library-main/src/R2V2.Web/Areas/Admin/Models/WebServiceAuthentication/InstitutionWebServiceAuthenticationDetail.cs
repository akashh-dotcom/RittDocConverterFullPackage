#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.WebServiceAuthentication
{
    public class InstitutionWebServiceAuthenticationDetail : AdminBaseModel
    {
        public InstitutionWebServiceAuthenticationDetail()
        {
        }

        public InstitutionWebServiceAuthenticationDetail(IAdminInstitution institution) : base(institution)
        {
        }

        public InstitutionWebServiceAuthenticationDetail(IAdminInstitution institution,
            Core.Institution.WebServiceAuthentication dbWebServiceAuthentication)
            : base(institution)
        {
            WebServiceAuthentication = new InstitutionWebServiceAuthentication(dbWebServiceAuthentication);
        }

        public InstitutionWebServiceAuthentication WebServiceAuthentication { get; set; }
    }
}