#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Web.Areas.Admin.Models.ReserveShelfManagement;

#endregion

namespace R2V2.Web.Models.ReserveShelf
{
    public class ReserveShelfIndex : BaseModel
    {
        private SelectList _sortbySelectList;
        public IEnumerable<ReserveShelf> ReserveShelves { get; set; }

        public ReserveShelf SelectedReserveShelf { get; set; }

        public IEnumerable<ReserveShelfUrl> ReserveShelfUrls { get; set; }

        public string SelectedSortBy { get; set; }

        [Display(Name = "Sort By: ")]
        public SelectList SortbySelectList =>
            _sortbySelectList ??
            (_sortbySelectList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "Author", Value = "author" },
                new SelectListItem { Text = "Title", Value = "title" },
                new SelectListItem { Text = "Publisher", Value = "publisher" },
                new SelectListItem { Text = "Release Date", Value = "releasedate" },
                new SelectListItem { Text = "Publication Date", Value = "pubdate" }
            }, "Value", "Text"));

        public int SelectedReserveShelfId { get; set; }
        public bool IsAscending { get; set; }
    }
}