#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionReferrer
{
    public static class InstitutionReferrerExtensions
    {
        public static IEnumerable<InstitutionReferrer> ToInstitutionReferrers(
            this IEnumerable<Core.Institution.InstitutionReferrer> institutionReferrers)
        {
            return institutionReferrers.Select(ToInstitutionReferrer);
        }

        public static InstitutionReferrer ToInstitutionReferrer(
            this Core.Institution.InstitutionReferrer institutionReferrer)
        {
            return new InstitutionReferrer
            {
                InstitutionId = institutionReferrer.InstitutionId,
                ValidReferer = institutionReferrer.ValidReferer,
                ValidReferrerId = institutionReferrer.Id
            };
        }
    }
}