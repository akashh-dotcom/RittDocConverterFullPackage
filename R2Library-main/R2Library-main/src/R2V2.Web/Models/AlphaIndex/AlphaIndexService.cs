#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Topic;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Models.AlphaIndex
{
    public class AlphaIndexService
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IQueryable<AZIndex> _azIndex;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly InstitutionService _institutionService;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<AlphaIndexService> _log;
        private readonly IQueryable<Core.Resource.Resource> _resources;

        public AlphaIndexService(ILog<AlphaIndexService> log
            , IAuthenticationContext authenticationContext
            , IQueryable<AZIndex> azIndex
            , IQueryable<Core.Resource.Resource> resources
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , IInstitutionSettings institutionSettings
            , InstitutionService institutionService
        )
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _azIndex = azIndex;
            _resources = resources;
            _institutionResourceLicenses = institutionResourceLicenses;
            _institutionSettings = institutionSettings;
            _institutionService = institutionService;
        }

        public IEnumerable<Topic> GetTopics(AlphaQuery alphaQuery)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            var institutionId = authenticatedInstitution == null || authenticatedInstitution.Id <= 0
                ? _institutionService.GetGuestInstitutionId(_institutionSettings.GuestAccountNumber)
                : authenticatedInstitution.Id;

            return GetTopics(institutionId, alphaQuery.Alpha, DisplayAllProducts(authenticatedInstitution));
        }

        private IEnumerable<Topic> GetTopics(int institutionId, string alphaKey, bool displayAllProducts)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var topics = from topic in _azIndex.WhereNameStartsWith(alphaKey)
                join resource in _resources on topic.ResourceId equals resource.Id
                join irl in _institutionResourceLicenses on resource.Id equals irl.ResourceId
                where irl.InstitutionId == institutionId &&
                      ((irl.LicenseCount > 0 && irl.LicenseTypeId == (int)LicenseType.Purchased) ||
                       (irl.LicenseTypeId == (int)LicenseType.Pda && irl.PdaAddedToCartDate == null))
                orderby topic.Name
                select topic.Name;

            var list = topics
                .Distinct()
                .ToList()
                .Select(topic => new Topic { Name = topic }).ToList();

            stopwatch.Stop();

            _log.DebugFormat(
                "GetTopics(institutionId: {0}, alphaKey: {1}, displayAllProducts: {2}) - list.Count: {3} in {4} ms",
                institutionId, alphaKey, displayAllProducts, list.Count, stopwatch.ElapsedMilliseconds);
            return list;
        }


        private bool DisplayAllProducts(AuthenticatedInstitution authenticatedInstitution)
        {
            if (authenticatedInstitution != null)
            {
                if (authenticatedInstitution.IsPublisherUser())
                {
                    return true;
                }

                if (authenticatedInstitution.AccountStatus == InstitutionAccountStatus.Trial)
                {
                    return true;
                }

                return authenticatedInstitution.DisplayAllProducts;
            }

            return true;
        }
    }
}