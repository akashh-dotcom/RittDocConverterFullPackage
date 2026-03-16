#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserveShelfList
    {
        private SelectList _sortbySelectList;
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name:")]
        [StringLength(100, ErrorMessage = "Name Max Length is 100 characters")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description:")]
        [StringLength(500, ErrorMessage = "Description Max Length is 500 characters")]
        [AllowHtml]
        public string Description { get; set; }

        private string _defaultSortBy { get; set; }

        [Display(Name = "Default Sort By:")]
        public string DefaultSortBy
        {
            get
            {
                switch (_defaultSortBy)
                {
                    case "title":
                        return "Title";
                    case "publisher":
                        return "Publisher";
                    case "releasedate":
                        return "Release Date";
                    case "pubdate":
                        return "Publication Date";
                    case "author":
                    default:
                        return "Author";
                }
            }
            set => _defaultSortBy = value;
        }

        [Display(Name = "Sort By Direction:")] public bool IsAscending { get; set; }
        public int ResourceCount { get; set; }

        public string ResourceTitleRemoved { get; set; }

        public IEnumerable<ResourceSummary> Resources { get; set; }

        public IEnumerable<ReserveShelfUrl> Urls { get; set; }

        public string SelectedSortBy { get; set; }

        [Display(Name = "Default Sort By: ")]
        public SelectList SortbySelectList => _sortbySelectList ??
                                              (_sortbySelectList = new SelectList(new List<SelectListItem>
                                              {
                                                  new SelectListItem { Text = @"Author", Value = "author" },
                                                  new SelectListItem { Text = @"Title", Value = "title" },
                                                  new SelectListItem { Text = @"Publisher", Value = "publisher" },
                                                  new SelectListItem { Text = @"Release Date", Value = "releasedate" },
                                                  new SelectListItem { Text = @"Publication Date", Value = "pubdate" }
                                              }, "Value", "Text"));
    }
}