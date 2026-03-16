#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaRules
{
    public class PdaRuleModel : AdminBaseModel
    {
        public PdaRuleModel(IAdminInstitution adminInstitution)
            : base(adminInstitution)
        {
        }

        public PdaRuleModel()
        {
        }

        [Required]
        [StringLength(255, ErrorMessage = @"Name cannot be longer than characters.")]
        [Display(Name = @"Name")]
        public string Name { get; set; }

        public bool HasMaxPrice { get; set; }

        [Display(Name = @"Max Price")]
        [DisplayFormat(DataFormatString = "{#.##}")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = @"Apply to Currently Available Titles")]
        public bool ExecuteNow { get; set; }

        [Display(Name = @"Apply to Future Titles")]
        public bool ExecuteForFuture { get; set; }

        [Display(Name = @"New Editions of Purchased Resources")]
        public bool IncludeNewEditionFirm { get; set; }

        [Display(Name = @"New Editions of PDA Resources")]
        public bool IncludeNewEditionPda { get; set; }

        [Display(Name = @"Apply Task to All Resources")]
        public bool ApplyToAllResources
        {
            get => !IncludeNewEditionFirm && !IncludeNewEditionPda;
            set
            {
                if (value)
                {
                    IncludeNewEditionFirm = false;
                    IncludeNewEditionPda = false;
                }
            }
        }

        [Display(Name = @"Practice Areas")]
        public List<SelectListItem> PracticeAreaSelectListItems { get; private set; }

        public int[] PracticeAreasSelected { get; set; }

        [Display(Name = @"Disciplines")] public List<SelectListItem> SpecialtiesSelectListItems { get; private set; }
        public int[] SpecialtiesSelected { get; set; }


        [Display(Name = @"Special Collections")]
        public List<SelectListItem> CollectionsSelectListItems { get; private set; }

        public int[] CollectionsSelected { get; set; }

        public void PopulateRule(PdaRule rule, List<IPracticeArea> practiceAreas, List<ISpecialty> specialties,
            List<ICollection> collections)
        {
            if (rule != null)
            {
                Id = rule.Id;
                Name = rule.Name;
                MaxPrice = rule.MaxPrice;
                ExecuteForFuture = rule.ExecuteForFuture;
                IncludeNewEditionFirm = rule.IncludeNewEditionFirm;
                IncludeNewEditionPda = rule.IncludeNewEditionPda;

                PracticeAreasSelected = rule.PracticeAreas.Any()
                    ? rule.PracticeAreas.Select(x => x.PracticeAreaId).ToArray()
                    : new int[0];
                SpecialtiesSelected = rule.Specialties.Any()
                    ? rule.Specialties.Select(x => x.SpecialtyId).ToArray()
                    : new int[0];
                CollectionsSelected = rule.Collections.Any()
                    ? rule.Collections.Select(x => x.CollectionId).ToArray()
                    : new int[0];

                HasMaxPrice = rule.MaxPrice > 0;
            }
            else
            {
                ExecuteForFuture = true;
            }

            PopulatePracticeAreasSelectListItems(practiceAreas);
            PopulateSpecialtiesSelectListItems(specialties);
            PopulateCollectionsSelectListItems(collections);
        }

        public void RepopulateSelectListItems(List<IPracticeArea> practiceAreas, List<ISpecialty> specialties,
            List<ICollection> collections)
        {
            PopulatePracticeAreasSelectListItems(practiceAreas);
            PopulateSpecialtiesSelectListItems(specialties);
            PopulateCollectionsSelectListItems(collections);
        }

        private void PopulatePracticeAreasSelectListItems(IEnumerable<IPracticeArea> practiceAreas)
        {
            PracticeAreaSelectListItems = new List<SelectListItem>();
            PracticeAreaSelectListItems.Add(new SelectListItem { Text = "All", Value = "All" });
            foreach (var item in practiceAreas.Select(practiceArea => new SelectListItem
                         { Text = practiceArea.Name, Value = practiceArea.Id.ToString() }))
            {
                PracticeAreaSelectListItems.Add(item);
            }
        }

        private void PopulateSpecialtiesSelectListItems(IEnumerable<ISpecialty> specialties)
        {
            SpecialtiesSelectListItems = new List<SelectListItem>();
            SpecialtiesSelectListItems.Add(new SelectListItem { Text = "All", Value = "All" });
            foreach (var item in specialties.Select(specialty => new SelectListItem
                         { Text = specialty.Name, Value = specialty.Id.ToString() }))
            {
                SpecialtiesSelectListItems.Add(item);
            }
        }

        private void PopulateCollectionsSelectListItems(IEnumerable<ICollection> collections)
        {
            CollectionsSelectListItems = new List<SelectListItem>();
            foreach (var item in collections.Select(collection => new SelectListItem
                         { Text = collection.Name, Value = collection.Id.ToString() }))
            {
                CollectionsSelectListItems.Add(item);
            }
        }
    }
}