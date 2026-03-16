#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewResource : InstitutionResource
    {
        /// <summary>
        /// </summary>
        public ReviewResource(CollectionManagementResource collectionManagementResource,
            IAdminInstitution adminInstitution,
            IEnumerable<Recommendation> recommendations, bool isSelected, int reviewResourceId)
            : base(collectionManagementResource, adminInstitution, recommendations)
        {
            IsSelected = isSelected;
            ReviewResourceId = reviewResourceId;
        }

        public bool IsSelected { get; set; }
        public int ReviewResourceId { get; set; }
    }
}