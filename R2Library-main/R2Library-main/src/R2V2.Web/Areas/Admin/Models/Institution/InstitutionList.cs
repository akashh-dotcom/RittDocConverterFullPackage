#region

using System.Collections.Generic;
using System.Text;
using R2V2.Core.Institution;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class InstitutionList : AdminBaseModel
    {
        public IInstitutionQuery InstitutionQuery { get; set; }

        public IEnumerable<InstitutionListItem> Institutions { get; set; }
        public IEnumerable<PageLink> PageLinks { get; set; }

        public int TotalCount { get; set; }
        public int ResultsFirstItem { get; set; }
        public int ResultsLastItem { get; set; }

        public string SelectedTerritoryName { get; set; }
        public string SelectedInstitutionTypeName { get; set; }

        public string SelectedFilters
        {
            get
            {
                var selectedFilters = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(InstitutionQuery.Query))
                {
                    selectedFilters.AppendFormat("<li>Query: {0}</li>", InstitutionQuery.Query);
                }

                if (!string.IsNullOrWhiteSpace(InstitutionQuery.SortBy))
                {
                    selectedFilters.AppendFormat("<li>Sorting by: {0} - {1}</li>",
                        GetSortByDescription(InstitutionQuery.SortBy), InstitutionQuery.SortDirection);
                }

                if (InstitutionQuery.AccountStatus != AccountStatus.All)
                {
                    selectedFilters.AppendFormat("<li>Showing Status: {0}</li>",
                        InstitutionQuery.AccountStatus.ToInstitutionAccountStatus().Description);
                }

                if (InstitutionQuery.TerritoryId != 0)
                {
                    selectedFilters.AppendFormat("<li>Showing Territory: {0}</li>", SelectedTerritoryName);
                }

                if (InstitutionQuery.InstitutionTypeId != 0)
                {
                    selectedFilters.AppendFormat("<li>Showing Institution Type: {0}</li>", SelectedInstitutionTypeName);
                }

                if (InstitutionQuery.IncludeExpertReviewer)
                {
                    selectedFilters.Append("<li>Showing Expert Reviewer: Include</li>");
                }

                if (InstitutionQuery.ExcludeExpertReviewer)
                {
                    selectedFilters.Append("<li>Showing Expert Reviewer: Exclude</li>");
                }

                if (InstitutionQuery.RecentOnly || InstitutionQuery.Page == "Recent")
                {
                    selectedFilters.Append("<li>Showing only: Recently Viewed</li>");
                }

                return selectedFilters.ToString();
            }
        }
    }
}