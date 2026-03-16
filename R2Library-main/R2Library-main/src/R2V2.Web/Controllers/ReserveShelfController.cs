#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models.ReserveShelfManagement;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Models.ReserveShelf;
using R2V2.Web.Models.Resource;
using ReserveShelf = R2V2.Web.Models.ReserveShelf.ReserveShelf;

#endregion

namespace R2V2.Web.Controllers
{
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.ExpertReviewer })]
    public class ReserveShelfController : R2BaseController
    {
        private readonly ILog<ReserveShelfController> _log;
        private readonly IQueryable<Core.ReserveShelf.ReserveShelf> _reserveShelves;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourceService _resourceService;

        public ReserveShelfController(IAuthenticationContext authenticationContext
            , IQueryable<Core.ReserveShelf.ReserveShelf> reserveShelves
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
            , ILog<ReserveShelfController> log
        )
            : base(authenticationContext)
        {
            _reserveShelves = reserveShelves;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
            _log = log;
        }

        private int[] PdaResources { get; set; }

        public ActionResult Index(int id = 0)
        {
            return View(GetReserveShelfIndex(id));
        }

        [HttpPost]
        public ActionResult Index(int selectedReserveShelfId, string selectedSortBy, bool isAscending = true)
        {
            return View(GetReserveShelfIndex(selectedReserveShelfId, selectedSortBy, isAscending));
        }

        private ReserveShelfIndex GetReserveShelfIndex(int selectedReserveShelfId, string selectedSortBy = null,
            bool? selectedIsAscending = null)
        {
            var reserveShelves = new List<ReserveShelf>();
            var reserveShelfUrls = new List<ReserveShelfUrl>();
            ReserveShelf selectedReserveShelf = null;

            string sortBy = null;
            var isAscending = true;

            if (AuthenticatedInstitution != null)
            {
                BuildPdaResources();
                int[] resourceIds = { };

                var coreReserveShelves = _reserveShelves.Where(x => x.Institution.Id == AuthenticatedInstitution.Id)
                    .OrderBy(x => x.Name);
                foreach (var reserveShelf in coreReserveShelves)
                {
                    var webReserveShelf = new ReserveShelf
                    {
                        Id = reserveShelf.Id,
                        Name = reserveShelf.Name,
                        Description = reserveShelf.Description
                    };

                    reserveShelves.Add(webReserveShelf);
                    if (selectedReserveShelfId == 0 && resourceIds.Count() == 0)
                    {
                        resourceIds = reserveShelf.ReserveShelfResources.Select(x => x.ResourceId).ToArray();
                        selectedReserveShelfId = reserveShelf.Id;
                        sortBy = reserveShelf.DefaultSortBy;
                        isAscending = selectedIsAscending != null
                            ? selectedIsAscending.Value
                            : reserveShelf.IsAscending ?? true;
                    }
                    else if (selectedReserveShelfId == reserveShelf.Id)
                    {
                        resourceIds = reserveShelf.ReserveShelfResources.Select(x => x.ResourceId).ToArray();
                        sortBy = reserveShelf.DefaultSortBy;
                        isAscending = selectedIsAscending != null
                            ? selectedIsAscending.Value
                            : reserveShelf.IsAscending ?? true;
                    }
                }

                selectedReserveShelf = selectedReserveShelfId == 0
                    ? reserveShelves.FirstOrDefault()
                    : reserveShelves.FirstOrDefault(x => x.Id == selectedReserveShelfId);

                if (selectedReserveShelf != null)
                {
                    sortBy = selectedSortBy ?? sortBy;

                    //TODO: KSH 6/27/2022 
                    //#1251 � Reserve Shelf PDA/Archive Issue
                    //Need to filter out Reserve Shelf resources that are PDA && Archived
                    var resources = _resourceService.GetResourcesByIds(resourceIds).Where(x =>
                        x.StatusId != (int)ResourceStatus.Archived ||
                        (x.StatusId == (int)ResourceStatus.Archived && !PdaResources.Contains(x.Id))
                    ).SortBy(sortBy, isAscending).Select(x => x.ToResourceSummary()).ToList();


                    //List<ResourceSummary> resources = _resourceService.GetResourcesByIds(resourceIds).SortBy(sortBy, isAscending).Select(x => x.ToResourceSummary()).ToList();

                    foreach (var resource in resources)
                    {
                        resource.Url = Url.Action("Title", "Resource", new { resource.Isbn });
                    }

                    if (AuthenticatedInstitution != null)
                    {
                        foreach (var resourceSummary in resources)
                        {
                            resourceSummary.IsFullTextAvailable =
                                _resourceAccessService.IsFullTextAvailable(resourceSummary.Id);
                        }
                    }

                    selectedReserveShelf.Resources = resources;

                    var coreReserveShelf = _reserveShelves.SingleOrDefault(x =>
                        x.Institution.Id == AuthenticatedInstitution.Id && x.Id == selectedReserveShelf.Id);

                    reserveShelfUrls = coreReserveShelf != null
                        ? coreReserveShelf.ReserveShelfUrls.ToReserveShelfUrls(selectedReserveShelf.Id).ToList()
                        : null;
                }
            }

            return new ReserveShelfIndex
            {
                ReserveShelves = reserveShelves,
                SelectedReserveShelf = selectedReserveShelf,
                ReserveShelfUrls = reserveShelfUrls,
                SelectedReserveShelfId = selectedReserveShelfId,
                SelectedSortBy = sortBy ?? "author",
                IsAscending = isAscending
            };
        }

        private void BuildPdaResources()
        {
            var pdaResources = new List<int>();
            foreach (var license in AuthenticatedInstitution.Licenses)
            {
                if (license.LicenseType == LicenseType.Pda && license.FirstPurchaseDate == null)
                {
                    pdaResources.Add(license.ResourceId);
                }
            }

            PdaResources = pdaResources.ToArray();
        }
    }
}