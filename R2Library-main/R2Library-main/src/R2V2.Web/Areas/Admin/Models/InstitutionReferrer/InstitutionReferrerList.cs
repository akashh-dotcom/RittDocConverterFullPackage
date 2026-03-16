#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionReferrer
{
    public class InstitutionReferrerList : AdminBaseModel
    {
        public InstitutionReferrerList()
        {
        }

        public InstitutionReferrerList(IAdminInstitution institution) : base(institution)
        {
        }

        public InstitutionReferrer EditReferrer { get; set; }

        public IEnumerable<InstitutionReferrer> InstitutionReferrers { get; set; }
    }
}