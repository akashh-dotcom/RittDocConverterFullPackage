#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NHibernate.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Institution
{
    public class InstitutionService
    {
        private const string GuestInstitutionIdKey = "Guest.Institution.Id";
        private const string AdminInstitutionKey = "Request.AdminInstitution";
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly IQueryable<Institution> _institutions;
        private readonly IQueryable<InstitutionType> _institutionTypes;

        private readonly ILog<InstitutionService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;


        public InstitutionService(ILog<InstitutionService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<Institution> institutions
            , IApplicationWideStorageService applicationWideStorageService
            , IQueryable<InstitutionType> institutionTypes
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutions = institutions;
            _applicationWideStorageService = applicationWideStorageService;
            _institutionTypes = institutionTypes;
        }

        public List<InstitutionType> GetInstitutionTypes()
        {
            return _institutionTypes.ToList();
        }

        public List<InstitutionType> GetInstitutionTypes(int[] institutionTypeIds)
        {
            return _institutionTypes.Where(x => institutionTypeIds.Contains(x.Id)).ToList();
        }

        public List<string> GetInstitutionNames(IInstitutionQuery query, int[] recentInstitutionIds)
        {
            var institutions = _institutions;
            return SearchAndFilterInstitutions(query, institutions, recentInstitutionIds).Select(x => x.NameKey)
                .ToList();
        }

        public IEnumerable<Institution> GetInstitutions(int[] institutionIds)
        {
            return _institutions.Where(x => institutionIds.Contains(x.Id));
        }

        public IEnumerable<Institution> GetInstitutions(IInstitutionQuery institutionQuery, int[] recentInstitutionIds,
            bool isExport = false)
        {
            var query = !string.IsNullOrWhiteSpace(institutionQuery.Query)
                ? institutionQuery.Query.ToLower().Trim()
                : null;

            var institutions = _institutions;
            var ignorePages = new[] { "All", "Recent" };

            if (
                (institutionQuery.AlphaFilter ||
                 (!ignorePages.Contains(institutionQuery.Page) && !string.IsNullOrWhiteSpace(query)))
                &&
                !isExport
                &&
                !ignorePages.Contains(institutionQuery.Page)
            )
            {
                var myInClause = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

                institutions = institutionQuery.Page == "#"
                    ? institutions.Where(x => myInClause.Contains(x.NameKey))
                    : institutions.Where(x => x.NameKey == institutionQuery.Page);
            }

            return SearchAndFilterInstitutions(institutionQuery, institutions, recentInstitutionIds);
        }

        private IQueryable<Institution> SearchAndFilterInstitutions(IInstitutionQuery institutionQuery,
            IQueryable<Institution> institutions, int[] recentInstitutionIds)
        {
            var query = !string.IsNullOrWhiteSpace(institutionQuery.Query)
                ? institutionQuery.Query.ToLower().Trim()
                : null;

            int.TryParse(query, out var id);
            _log.DebugFormat("id: {0}", id);

            if (!string.IsNullOrWhiteSpace(query))
            {
                institutions = institutions.Where(x =>
                    x.AccountNumber.StartsWith(query) ||
                    x.Name.ToLower().Contains(query) ||
                    //(x.Id == id) ||
                    x.Address.City.ToLower().Contains(query) ||
                    x.Address.State.ToLower().Contains(query) ||
                    x.Address.Zip.StartsWith(query));
            }

            return institutions
                .WhereAccountStatus(institutionQuery.AccountStatus)
                .WhereRecentInstitutions(institutionQuery, recentInstitutionIds)
                .WhereTerritory(institutionQuery.TerritoryId)
                .WhereInstitutionType(institutionQuery.InstitutionTypeId)
                .WhereExpertReviewer(institutionQuery.IncludeExpertReviewer, institutionQuery.ExcludeExpertReviewer)
                .OrderBy(x => x.Name)
                .ThenBy(x => x.AccountNumber)
                .OrderByRecent(institutionQuery, recentInstitutionIds);
        }

        public IInstitution GetInstitutionForAdmin(int id)
        {
            _log.DebugFormat("GetInstitutionForAdmin() - id: {0}", id);
            IInstitution institution;

            var key = $"{AdminInstitutionKey}.{id}";

            if (HttpContext.Current.Items.Contains(key))
            {
                institution = (IInstitution)HttpContext.Current.Items[key];
                _log.Debug("retrieved institution from request context");
                return institution;
            }

            institution = GetInstitutionForAdminNotCached(id);
            HttpContext.Current.Items.Add(key, institution);
            _log.Debug("stored institution into request context");
            return institution;
        }

        public IInstitution GetInstitutionForAdminNotCached(int id)
        {
            _log.DebugFormat("GetInstitutionForAdminNotCached() >> id: {0}", id);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var institutionResourceLicenses = _institutions
                    .Where(x => x.Id == id)
                    .FetchMany(x => x.InstitutionResourceLicenses)
                    .Fetch(x => x.AnnualFee)
                    .Fetch(x => x.Territory)
                    .ToFuture();

                var institutions = institutionResourceLicenses.ToList();
                if (!institutions.Any())
                {
                    stopwatch.Stop();
                    _log.DebugFormat("GetInstitutionForAdminNotCached() << id: {0} - {1:#,###} ms - return null", id,
                        stopwatch.ElapsedMilliseconds);
                    return null;
                }

                IInstitution institution = institutions.FirstOrDefault();

                foreach (var inst in institutions)
                {
                    _log.DebugFormat("evicting institution: {0}", inst.Id);
                    uow.Evict(inst);

                    foreach (var license in inst.InstitutionResourceLicenses)
                    {
                        uow.Evict(license);
                    }
                }

                uow.Evict(typeof(InstitutionResourceLicense));

                stopwatch.Stop();
                _log.DebugFormat("GetInstitutionForAdminNotCached() << id: {0} - {1:#,###} ms - institution: {2}", id,
                    stopwatch.ElapsedMilliseconds,
                    institution == null ? -1 : institution.Id);
                return institution;
            }
        }


        public IInstitution GetInstitutionForEdit(int id)
        {
            _log.DebugFormat("GetInstitutionForEdit() - id: {0}", id);
            return _institutions
                .Where(x => x.Id == id)
                .FetchMany(x => x.InstitutionResourceLicenses)
                .Fetch(x => x.AnnualFee)
                .Fetch(x => x.Territory)
                .SingleOrDefault();
        }

        public IInstitution GetInstitutionForEdit(string accountNumber)
        {
            _log.DebugFormat("GetInstitutionForEdit() - accountNumber: {0}", accountNumber);
            return _institutions
                .Where(x => x.AccountNumber == accountNumber)
                .FetchMany(x => x.InstitutionResourceLicenses)
                .Fetch(x => x.AnnualFee)
                .Fetch(x => x.Territory)
                .SingleOrDefault();
        }

        public bool DoesInstitutionExists(string accountNumber)
        {
            var institution = _institutions.FirstOrDefault(x => x.AccountNumber == accountNumber);
            return institution != null;
        }

        public int GetInstitutionId(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return 0;
            }

            return _institutions.Where(x => x.AccountNumber == accountNumber).Select(y => y.Id).FirstOrDefault();
        }

        /// <summary>
        ///     Cache the guest institution id, no need to query it all the time, it is not going to change.
        /// </summary>
        public int GetGuestInstitutionId(string guestAccountNumber)
        {
            if (_applicationWideStorageService.Has(GuestInstitutionIdKey))
            {
                return _applicationWideStorageService.Get<int>(GuestInstitutionIdKey);
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                var id = (from institution in _institutions
                    where institution.AccountNumber == guestAccountNumber
                    select institution.Id).SingleOrDefault();

                _applicationWideStorageService.Put(GuestInstitutionIdKey, id);

                uow.Evict(id);

                return id;
            }
        }
    }
}