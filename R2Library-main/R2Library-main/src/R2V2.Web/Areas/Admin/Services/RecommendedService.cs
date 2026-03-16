#region

using System.Collections.Generic;
using R2V2.Contexts;
using R2V2.Core.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class RecommendedService
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly RecommendationsService _recommendationsService;

        public RecommendedService(RecommendationsService recommendationsService
            , IAuthenticationContext authenticationContext
        )
        {
            _recommendationsService = recommendationsService;
            _authenticationContext = authenticationContext;
        }

        public bool BulkRecommend(int institutionId, IEnumerable<InstitutionResource> institutionResources,
            string notes)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            foreach (var institutionResource in institutionResources)
            {
                _recommendationsService.SaveRecommendation(institutionId, authenticatedInstitution.User.Id,
                    institutionResource.Id, notes);
            }

            return true;
        }
    }
}