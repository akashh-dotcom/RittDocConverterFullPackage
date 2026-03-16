#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Recommendations
{
    public class RecommendationEdit : AdminBaseModel
    {
        public RecommendationEdit(IAdminInstitution institution)
            : base(institution)
        {
            Recommended = new Recommended();
        }

        public string Notes { get; set; }

        public Recommended Recommended { get; set; }

        public IList<Recommended> RecommendedList { get; set; }
        public InstitutionResource InstitutionResource { get; set; }

        public string BackToListLink { get; set; }
        public string ViewMyRecommendationsLink { get; set; }

        public CollectionManagementQuery ResourceQuery { get; set; }

        public int ReviewId { get; set; }

        public string Action { get; set; }
    }
}