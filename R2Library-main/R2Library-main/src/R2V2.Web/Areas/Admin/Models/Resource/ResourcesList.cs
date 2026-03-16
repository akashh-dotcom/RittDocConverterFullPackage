#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Resource;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public class ResourcesList : AdminBaseModel
    {
        protected const int MaxPages = 9;
        private readonly List<Resource> _resources = new List<Resource>();

        private SelectList _pageSizeSelectList;

        public IEnumerable<Resource> Resources => _resources;

        public string PageTitle { get; set; }

        public ResourceQuery ResourceQuery { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }

        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }


        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }

        public PageLink RecentLink { get; set; }

        public int TotalCount { get; set; }
        public int ResultsFirstItem { get; set; }
        public int ResultsLastItem { get; set; }

        public string SelectedFilters { get; set; }

        public bool DisplayPromotionFields { get; set; }

        [Display(Name = " results per page")]
        public SelectList PageSizeSelectList =>
            _pageSizeSelectList ??
            (_pageSizeSelectList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "10", Value = "10" },
                new SelectListItem { Text = "25", Value = "25" },
                new SelectListItem { Text = "50", Value = "50" },
                new SelectListItem { Text = "100", Value = "100" },
                new SelectListItem { Text = "250", Value = "250" }
            }, "Value", "Text"));

        public string SpecialIconBaseUrl { get; set; }

        public void AddResource(Resource resource)
        {
            _resources.Add(resource);
        }
    }
}