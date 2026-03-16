#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Territory;
using R2V2.Web.Areas.Admin.Models.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class AutomatedCartModel : AutomatedCartBaseModel
    {
        private MultiSelectList _institutionTypeList;
        private IEnumerable<InstitutionType> _institutionTypes;

        private int[] _selectedInstitutionIds;
        private IEnumerable<ITerritory> _territories;
        
        private MultiSelectList _territorySelectList;

        public AutomatedCartModel()
        {
        }

        public AutomatedCartModel(IList<ITerritory> territories, IList<InstitutionType> institutionTypes)
        {
            PopulateSelections(territories, institutionTypes);
            ReportQuery = new WebAutomatedCartReportQuery
            {
                IncludeNewEdition = true,
                IncludeReviewed = true,
                IncludeTriggeredPda = true,
                IncludeTurnaway = true,
                IncludeRequested = true,
                IsDefaultQuery = true,
                Period = ReportPeriod.Last30Days
            };
        }

        public AutomatedCartModel(IList<ITerritory> territories, IList<InstitutionType> institutionTypes,
            WebAutomatedCartReportQuery reportQuery, IEnumerable<AutomatedCartInstitution> institutions,
            string selectedInstitutionIds)
        {
            PopulateSelections(territories, institutionTypes);
            ReportQuery = reportQuery;
            Institutions = institutions;
            SelectedInstitutionIds = selectedInstitutionIds;
        }

        public IEnumerable<AutomatedCartInstitution> Institutions { get; set; }

        [Display(Name = "Institution Type:")]
        public MultiSelectList InstitutionTypeSelectList
        {
            get
            {
                if (_institutionTypeList != null)
                {
                    return _institutionTypeList;
                }

                var institutionTypes = new List<InstitutionType> { new InstitutionType { Id = 0, Name = "All" } };

                if (_institutionTypes != null)
                {
                    institutionTypes.AddRange(_institutionTypes);
                }

                if (ReportQuery.InstitutionTypeIds == null || !ReportQuery.InstitutionTypeIds.Any())
                {
                    ReportQuery.InstitutionTypeIds = new[] { 0 };
                }

                return _institutionTypeList ?? (_institutionTypeList =
                    new MultiSelectList(institutionTypes, "Id", "Name", ReportQuery.InstitutionTypeIds));
            }
        }

        [Display(Name = "Territory:")]
        public MultiSelectList TerritorySelectList
        {
            get
            {
                if (_territorySelectList != null)
                {
                    return _territorySelectList;
                }

                var territories = new List<ITerritory> { new Territory { Code = "All", Name = "All" } };

                if (_territories != null)
                {
                    territories.AddRange(_territories);
                }

                if (ReportQuery.TerritoryCodes == null || !ReportQuery.TerritoryCodes.Any())
                {
                    ReportQuery.TerritoryCodes = new[] { "All" };
                }

                return _territorySelectList ?? (_territorySelectList =
                    new MultiSelectList(territories, "Code", "Name", ReportQuery.TerritoryCodes));
            }
        }

        public bool HideDateRangeDisplay { get; set; }

        public string SelectedInstitutionIds { get; set; }

        public bool ShowExportToolLink => Institutions != null && Institutions.Any();

        public string ExcelExportUrl => ShowExportToolLink ? "javascript:void(0)" : null;

        public void PopulateSelections(IEnumerable<ITerritory> territories, IList<InstitutionType> institutionTypes)
        {
            _territories = territories;
            _institutionTypes = institutionTypes;
        }

        public string GetToolTip(Address address)
        {
            var sb = new StringBuilder();
            sb.Append($"{address.Address1}&#013;");
            if (!string.IsNullOrWhiteSpace(address.Address2))
            {
                sb.Append($"{address.Address2}&#013;");
            }

            sb.Append($"{address.City}, {address.State} {address.Zip}");
            return sb.ToString();
        }

        public string DisplayCount()
        {
            if (Institutions != null && Institutions.Any())
            {
                return $"{Institutions.Count()} Institutions Found";
            }

            return null;
        }

        public string DateRangeDisplay()
        {
            if (HideDateRangeDisplay)
            {
                return null;
            }

            return ReportQuery.PeriodStartDate.HasValue
                ? $"From {ReportQuery.PeriodStartDate.GetValueOrDefault():MM/dd/yyyy} to {ReportQuery.PeriodEndDate.GetValueOrDefault():MM/dd/yyyy}"
                : null;
        }

        public string IsChecked(int institutionId)
        {
            if (string.IsNullOrWhiteSpace(SelectedInstitutionIds))
            {
                return "";
            }

            if (_selectedInstitutionIds == null)
            {
                var test = SelectedInstitutionIds.Split(',');
                if (test.Any())
                {
                    int i;
                    _selectedInstitutionIds = test.Select(int.Parse).ToArray();
                }
            }

            if (_selectedInstitutionIds != null)
            {
                return _selectedInstitutionIds.Contains(institutionId) ? "checked=\"checked\"" : "";
            }

            return "";
        }
    }
}