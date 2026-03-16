#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Web.Areas.Admin.Models.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class RequestedResourcesModel : AdminBaseModel
    {
        private readonly IEnumerable<InstitutionType> _institutionTypes;
        private readonly IEnumerable<ITerritory> _territories;

        private MultiSelectList _institutionTypeList;

        private MultiSelectList _territorySelectList;

        public RequestedResourcesModel()
        {
            if (ReportQuery == null)
            {
                ReportQuery = new RequestedResourcesQuery();
            }
        }

        public RequestedResourcesModel(List<RequestedResourcesInstitution> requestedResourcesInstitutions,
            RequestedResourcesQuery query, IEnumerable<ITerritory> territories, IList<InstitutionType> institutionTypes)
        {
            ReportQuery = query;

            RequestedResourcesInstitutions = requestedResourcesInstitutions;
            RequestedResourcesInstitutions.ForEach(x => x.PopulateCounts());
            RequestedResourceCount = RequestedResourcesInstitutions.Sum(x => x.ResourceCount);
            PurchasedRequestedResourceCount = RequestedResourcesInstitutions.Sum(x => x.PurchasedResourceCount);
            TotalPurchasedPrice = RequestedResourcesInstitutions.Sum(x => x.TotalPurchasedPrice);

            _territories = territories;
            _institutionTypes = institutionTypes;
        }

        public RequestedResourcesQuery ReportQuery { get; set; }
        public List<RequestedResourcesInstitution> RequestedResourcesInstitutions { get; set; }
        public int RequestedResourceCount { get; set; }
        public int PurchasedRequestedResourceCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPurchasedPrice { get; set; }

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

        public bool ShowExportToolLink =>
            RequestedResourcesInstitutions != null && RequestedResourcesInstitutions.Any();

        public string ExcelExportUrl => ShowExportToolLink ? "javascript:void(0)" : null;

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
    }
}