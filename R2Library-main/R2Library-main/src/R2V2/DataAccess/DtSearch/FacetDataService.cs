#region

using System.Collections.Generic;
using System.Linq;
using dtSearch.Engine;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Search.FacetData;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class FacetDataService
    {
        private readonly ILog<FacetDataService> _log;
        private readonly PracticeAreaService _practiceAreaService;
        private readonly SpecialtyService _specialtyService;

        public FacetDataService(ILog<FacetDataService> log, PracticeAreaService practiceAreaService,
            SpecialtyService specialtyService)
        {
            _log = log;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
        }

        public IEnumerable<IFacetData> GetFacetData(WordListBuilder wordListBuilder,
            SearchStatusHandler searchStatusHandler)
        {
            var data = new List<IFacetData>();
            data.AddRange(searchStatusHandler.GetSearchResultsCounts());

            SetBookStatusData(wordListBuilder, data);

            SetPracticeAreaData(wordListBuilder, data);
            SetSpecialtyData(wordListBuilder, data);

            SetDrugMonographData(wordListBuilder, data);

            SetYearData(wordListBuilder, data);
            return data;
        }

        /// <param name="data"> </param>
        private void SetBookStatusData(WordListBuilder wordListBuilder, List<IFacetData> data)
        {
            var count = wordListBuilder.ListFieldValues("r2bookstatus", null, 10);
            for (var i = 0; i < count; ++i)
            {
                var word = wordListBuilder.GetNthWord(i);
                IFacetData searchResultsCount;
                if (word == "Active")
                {
                    searchResultsCount = new StatusFacetData
                        { Name = "Active", Count = wordListBuilder.GetNthWordDocCount(i), Id = 6, Code = "act" };
                    data.Add(searchResultsCount);
                }
                else if (word == "Archive")
                {
                    searchResultsCount = new StatusFacetData
                        { Name = "Archive", Count = wordListBuilder.GetNthWordDocCount(i), Id = 7, Code = "arc" };
                    data.Add(searchResultsCount);
                }
                else
                {
                    _log.WarnFormat("r2bookstatus not supported: {0}", word);
                }
            }
        }

        /// <param name="data"> </param>
        private void SetPracticeAreaData(WordListBuilder wordListBuilder, List<IFacetData> data)
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToArray();
            var count = wordListBuilder.ListFieldValues("r2PracticeArea", null, 100);

            var facetDataList = new List<IFacetData>();

            for (var i = 0; i < count; ++i)
            {
                var codes = wordListBuilder.GetNthWord(i).Split(';');
                foreach (var code in codes)
                {
                    var cleanCode = code.Trim();
                    var practiceArea = practiceAreas.SingleOrDefault(p => p.Code == cleanCode);
                    var name = practiceArea == null ? cleanCode : practiceArea.Name;

                    var facetData = facetDataList.SingleOrDefault(x => x.Name == name.Trim());
                    if (facetData != null)
                    {
                        facetData.Count += wordListBuilder.GetNthWordDocCount(i);
                    }
                    else
                    {
                        facetData = new PracticeAreaFacetData
                            { Name = name.Trim(), Count = wordListBuilder.GetNthWordDocCount(i) };
                        if (practiceArea != null)
                        {
                            facetData.Id = practiceArea.Id;
                            facetData.Code = practiceArea.Code.ToLower();
                        }

                        facetDataList.Add(facetData);
                        data.Add(facetData);
                    }
                }
            }
        }

        private void SetSpecialtyData(WordListBuilder wordListBuilder, List<IFacetData> data)
        {
            var specialties = _specialtyService.GetAllSpecialties().ToArray();
            var count = wordListBuilder.ListFieldValues("r2Specialty", null, 1000);

            var facetDataList = new List<IFacetData>();

            for (var i = 0; i < count; ++i)
            {
                var codes = wordListBuilder.GetNthWord(i).Split(';');
                foreach (var code in codes)
                {
                    var cleanCode = code.Trim();
                    var specialty = specialties.SingleOrDefault(p => p.Code == cleanCode);
                    var name = specialty == null ? cleanCode : specialty.Name;

                    var facetData = facetDataList.SingleOrDefault(x => x.Name == name.Trim());
                    if (facetData != null)
                    {
                        facetData.Count += wordListBuilder.GetNthWordDocCount(i);
                    }
                    else
                    {
                        facetData = new SpecialtyFacetData
                        {
                            Name = name.Trim(), Count = wordListBuilder.GetNthWordDocCount(i), Id = specialty?.Id ?? 0
                        };
                        if (specialty != null)
                        {
                            facetData.Id = specialty.Id;
                            facetData.Code = specialty.Code.ToLower();
                        }

                        facetDataList.Add(facetData);
                        data.Add(facetData);
                    }
                }
            }
        }

        private void SetDrugMonographData(WordListBuilder wordListBuilder, List<IFacetData> data)
        {
            var count = wordListBuilder.ListFieldValues("r2DrugMonograph", null, 10);
            if (count == 0)
            {
                _log.Debug("SetDrugMonographData() - no drug monograph data");
            }
            else if (count == 1)
            {
                var facetData = new DrugMonographFacetData
                    { Name = "Drug", Count = wordListBuilder.GetNthWordDocCount(0), Id = 1, Code = "drug" };
                data.Add(facetData);
                _log.DebugFormat("SetDrugMonographData() - {0}", facetData);
            }
            else
            {
                var facetData = new DrugMonographFacetData
                    { Name = "Drug", Count = wordListBuilder.GetNthWordDocCount(0), Id = 1, Code = "drug" };
                data.Add(facetData);
                _log.DebugFormat("SetDrugMonographData() - {0}", facetData);
                for (var i = 0; i < count; ++i)
                {
                    _log.WarnFormat("SetDrugMonographData() - Drug Monograph counts warning - name: {0}, count: {1}",
                        wordListBuilder.GetNthWord(i), wordListBuilder.GetNthWordDocCount(i));
                }
            }
        }

        private void SetYearData(WordListBuilder wordListBuilder, List<IFacetData> data)
        {
            // r2BookStatus,r2BookTitle,r2Author,2PrimaryAuthor,r2Publisher,r2Library,r2PracticeArea,r2Specialty,r2DrugMonograph,r2CopyrightYear,r2ReleaseDate
            var count = wordListBuilder.ListFieldValues("r2CopyrightYear", null, 100);
            if (count == 0)
            {
                _log.Debug("no year counts ???");
            }

            var facetDataList = new List<IFacetData>();
            for (var i = 0; i < count; ++i)
            {
                //_log.DebugFormat("{0}. Year - Name: {1}, Count: {2}", i, wordListBuilder.GetNthWord(i), wordListBuilder.GetNthWordDocCount(i));

                int.TryParse(wordListBuilder.GetNthWord(i), out var year);
                if (year > 1900)
                {
                    if (year > 2100)
                    {
                        var year1 = year / 10000;
                        var year2 = year % 10000;
                        year = year1 > year2 ? year1 : year2;
                    }

                    var name = $"{year}";
                    var facetData = facetDataList.SingleOrDefault(x => x.Name == name);
                    if (facetData != null)
                    {
                        facetData.Count += wordListBuilder.GetNthWordDocCount(i);
                    }
                    else
                    {
                        facetData = new YearFacetData
                        {
                            Name = name.Trim(), Count = wordListBuilder.GetNthWordDocCount(i), Id = i,
                            Code = name.Trim()
                        };
                        facetDataList.Add(facetData);
                        data.Add(facetData);
                    }
                }
            }
        }
    }
}