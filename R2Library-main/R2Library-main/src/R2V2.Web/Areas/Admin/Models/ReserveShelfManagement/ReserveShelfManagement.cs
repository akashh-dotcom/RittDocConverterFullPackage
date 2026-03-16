#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserveShelfManagement : InstitutionResources
    {
        public ReserveShelfManagement()
        {
        }

        public ReserveShelfManagement(IAdminInstitution institution, ReserveShelfQuery reserveShelfQuery,
            IEnumerable<ReserveShelfResource> reserveShelfResources, ReserveShelf reserveShelf,
            IPracticeAreaService practiceAreaService, ISpecialtyService specialtyService,
            ICollectionService collectionService, string doodyReviewUrl, string specialIconBaseUrl)
            : base(
                institution, reserveShelfQuery, null, practiceAreaService, specialtyService, collectionService,
                doodyReviewUrl, specialIconBaseUrl)
        {
            ReserveShelfQuery = reserveShelfQuery;
            ReserveShelf = reserveShelf;
            ReserveShelfResources = reserveShelfResources;

            SelectedFilters = CollectionManagementQuery.ToSelectedFilters(practiceAreaService, specialtyService,
                collectionService, GetSortByDescription(ReserveShelfQuery.SortBy));
            //BuildSelectedFilters(practiceAreaService, specialtyService);

            ReserveShelfId = reserveShelf.Id;
        }

        public ReserveShelf ReserveShelf { get; set; }
        public ReserveShelfQuery ReserveShelfQuery { get; set; }

        public IEnumerable<ReserveShelfResource> ReserveShelfResources { get; set; }
    }
}