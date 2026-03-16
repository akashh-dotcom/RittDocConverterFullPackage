#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource.Discipline
{
    public class SpecialtyService : ISpecialtyService
    {
        private const string SpecialtyCacheKey = "Specialty.All";
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        private readonly IQueryable<Specialty> _specialties;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        /// <param name="unitOfWorkProvider"> </param>
        public SpecialtyService(IQueryable<Specialty> specialties
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService
        )
        {
            _specialties = specialties;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
        }

        public void ClearCache()
        {
            _applicationWideStorageService.Remove(SpecialtyCacheKey);
        }

        /// <summary>
        ///     Stores ISpecialies in cache. Replacing IEnumerable[Specialty] GetAllSpecialties()
        /// </summary>
        public IEnumerable<ISpecialty> GetAllSpecialties()
        {
            var specialties = _applicationWideStorageService.Get<IList<ISpecialty>>(SpecialtyCacheKey);
            if (specialties == null)
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var specialtiesFromDb = _specialties.OrderBy(x => x.Name).ToList();

                    specialties = new List<ISpecialty>();
                    foreach (var specialty in specialtiesFromDb)
                    {
                        var cachedSpecialty = new CachedSpecialty(specialty);
                        specialties.Add(cachedSpecialty);
                    }

                    _applicationWideStorageService.Put(SpecialtyCacheKey, specialties);

                    foreach (var specialty in specialtiesFromDb)
                    {
                        uow.Evict(specialty);
                    }
                }
            }

            return specialties;
        }

        public Specialty GetSpecialtyForEdit(int specialtyId)
        {
            return _specialties.SingleOrDefault(x => x.Id == specialtyId);
        }

        public ISpecialty GetSpecialty(int specialtyId)
        {
            return GetAllSpecialties().SingleOrDefault(x => x.Id == specialtyId);
        }

        public ISpecialty GetSpecialty(string specialtyId)
        {
            int.TryParse(specialtyId, out var id);
            return id > 0 ? GetAllSpecialties().SingleOrDefault(x => x.Id == id) : null;
        }

        public ISpecialty GetSpecialtyByCode(string code)
        {
            return GetAllSpecialties().SingleOrDefault(x => x.Code == code);
        }
    }
}