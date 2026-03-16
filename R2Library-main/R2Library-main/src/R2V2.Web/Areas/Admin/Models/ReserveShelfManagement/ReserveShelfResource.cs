#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserveShelfResource : InstitutionResource
    {
        public ReserveShelfResource(CollectionManagementResource collectionManagementResource,
            IAdminInstitution adminInstitution,
            IEnumerable<Recommendation> recommendations, bool isSelected)
            : base(collectionManagementResource, adminInstitution, recommendations)
        {
            IsSelected = isSelected;
        }

        public bool IsSelected { get; set; }
    }
}