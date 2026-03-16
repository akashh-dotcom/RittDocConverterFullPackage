#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

//using R2V2.Infrastructure.Logging;

namespace R2V2.Core.Resource.PracticeArea
{
    public class PracticeAreaService : IPracticeAreaService
    {
        private const string PracticeAreaCacheKey = "PracticeAreas.All";
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        private readonly IQueryable<PracticeArea> _practiceAreas;

        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        //private readonly ILog<PracticeAreaService> _log;

        /// <param name="unitOfWorkProvider"> </param>
        public PracticeAreaService(IQueryable<PracticeArea> practiceAreas
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService
            //, ILog<PracticeAreaService> log
        )
        {
            _practiceAreas = practiceAreas;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
            //_log = log;
        }

        public void ClearCache()
        {
            _applicationWideStorageService.Remove(PracticeAreaCacheKey);
        }

        public IEnumerable<IPracticeArea> GetAllPracticeAreas()
        {
            var practiceAreas = _applicationWideStorageService.Get<IEnumerable<IPracticeArea>>(PracticeAreaCacheKey);

            if (practiceAreas == null)
            {
                List<IPracticeArea> list;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var items = _practiceAreas.OrderBy(x => x.Name).ToList();
                    list = new List<IPracticeArea>();

                    foreach (var practiceArea in items)
                    {
                        var cachedPracticeArea = new CachedPracticeArea(practiceArea);
                        list.Add(cachedPracticeArea);
                    }

                    practiceAreas = list;
                    _applicationWideStorageService.Put(PracticeAreaCacheKey, practiceAreas);
                }
            }

            return practiceAreas;
        }

        /// <param name="practiceAreaId"> </param>
        public IPracticeArea GetPracticeAreaById(int practiceAreaId)
        {
            return GetAllPracticeAreas().SingleOrDefault(x => x.Id == practiceAreaId);
        }

        public IPracticeArea GetPracticeAreaById(string practiceAreaId)
        {
            int.TryParse(practiceAreaId, out var id);
            return id > 0 ? GetAllPracticeAreas().SingleOrDefault(x => x.Id == id) : null;
        }

        public PracticeArea GetPracticeArea(int practiceAreaId)
        {
            return _practiceAreas.FirstOrDefault(x => x.Id == practiceAreaId);
        }

        public IPracticeArea GetPracticeAreaByCode(string code)
        {
            return GetAllPracticeAreas().SingleOrDefault(x => x.Code == code);
        }
    }
}