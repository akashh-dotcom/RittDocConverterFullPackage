#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Institution
{
    public class InstitutionTypeService : IInstitutionTypeService
    {
        private const string InstitutionTypeAllKey = "InstitutionTypes.All";

        private static readonly object LockObject = new object();
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly IQueryable<InstitutionType> _institutionTypes;
        private readonly ILog<InstitutionTypeService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public InstitutionTypeService(ILog<InstitutionTypeService> log
            , IQueryable<InstitutionType> institutionTypes
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService)
        {
            _log = log;
            _institutionTypes = institutionTypes;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
        }

        public IList<IInstitutionType> GetAllInstitutionTypes()
        {
            var institutionTypes = _applicationWideStorageService.Get<IList<IInstitutionType>>(InstitutionTypeAllKey);

            if (institutionTypes == null)
            {
                _log.Debug("waiting on lock to load institutionTypes cache");
                lock (LockObject)
                {
                    institutionTypes =
                        _applicationWideStorageService.Get<IList<IInstitutionType>>(InstitutionTypeAllKey);
                    if (institutionTypes == null)
                    {
                        _log.Debug("loading institutionTypes cache");
                        institutionTypes = new List<IInstitutionType>();
                        using (var uow = _unitOfWorkProvider.Start())
                        {
                            var list = _institutionTypes.Where(x => x.RecordStatus);

                            foreach (var institutionType in list.ToList())
                            {
                                institutionTypes.Add(new CachedInstitutionType(institutionType));
                            }
                        }

                        _applicationWideStorageService.Put(InstitutionTypeAllKey, institutionTypes);
                    }
                }
            }

            return institutionTypes;
        }

        public IInstitutionType GetInstitutionType(int id)
        {
            var institutionTypes = GetAllInstitutionTypes();
            return institutionTypes.FirstOrDefault(x => x.Id == id);
        }

        public IList<IInstitutionType> GetInstitutionTypes(int[] institutionTypeIds)
        {
            var institutionTypes = GetAllInstitutionTypes();
            return institutionTypes.Where(x => institutionTypeIds.Contains(x.Id)).ToList();
        }

        public IList<IInstitutionType> GetInstitutionTypes(string[] institutionTypeCodes)
        {
            var institutionTypes = GetAllInstitutionTypes();
            return institutionTypes.Where(x => institutionTypeCodes.Contains(x.Code)).ToList();
        }
    }

    public interface IInstitutionTypeService
    {
        IList<IInstitutionType> GetAllInstitutionTypes();
        IInstitutionType GetInstitutionType(int id);
        IList<IInstitutionType> GetInstitutionTypes(int[] institutionTypeIds);
        IList<IInstitutionType> GetInstitutionTypes(string[] institutionTypeCodes);
    }
}