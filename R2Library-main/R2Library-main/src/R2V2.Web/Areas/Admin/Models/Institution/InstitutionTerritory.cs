#region

using System.ComponentModel.DataAnnotations;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class InstitutionTerritory
    {
        public InstitutionTerritory(ITerritory territory)
        {
            if (territory != null)
            {
                Name = territory.Name;
                Code = territory.Code;
                TerritoryId = territory.Id;
            }
        }

        public InstitutionTerritory()
        {
        }

        //Territory
        [Display(Name = "Territory:")] public string Name { get; set; }
        public string Code { get; set; }

        public int TerritoryId { get; set; }
    }
}