#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class InstitutionEditViewModel : Institution
    {
        private readonly IEnumerable<InstitutionType> _institutionTypes;
        private readonly IEnumerable<ITerritory> _territories;
        private SelectList _accessTypeSelectList;
        private SelectList _accountStatusSelectList;

        private SelectList _institutionTypeSelectList;

        private SelectList _territorySelectList;


        public InstitutionEditViewModel()
        {
        }

        public InstitutionEditViewModel(IEnumerable<ITerritory> territories,
            IEnumerable<InstitutionType> institutionTypes)
        {
            _territories = territories;
            _institutionTypes = institutionTypes;
        }

        public InstitutionEditViewModel(IInstitution institution, IUser user, IEnumerable<ITerritory> territories,
            IEnumerable<InstitutionType> institutionTypes)
            : base(institution, user)
        {
            AccountStatus = institution.AccountStatus.Id;
            AccessType = institution.AccessType.Id;
            HomePageId = institution.HomePageId;
            _territories = territories;
            _institutionTypes = institutionTypes;
        }

        [Display(Name = "Home Page Display:")] public new HomePage HomePage { get; set; }
        public int HomePageId { get; set; }

        public new AccountStatus AccountStatus { get; set; }

        [Display(Name = "Account Status:")]
        public SelectList AccountStatusSelectList =>
            _accountStatusSelectList ?? (_accountStatusSelectList = new SelectList(new List<InstitutionAccountStatus>
            {
                InstitutionAccountStatus.Active,
                InstitutionAccountStatus.Trial,
                InstitutionAccountStatus.Disabled
            }, "Id", "Description"));

        public new AccessType AccessType { get; set; }

        [Display(Name = "Access Type:")]
        public SelectList AccessTypeSelectList =>
            _accessTypeSelectList ?? (_accessTypeSelectList = new SelectList(new List<InstitutionAccessType>
            {
                InstitutionAccessType.IpIndependent,
                InstitutionAccessType.IpValidationAnon,
                InstitutionAccessType.IpValidationOpt,
                InstitutionAccessType.IpValidationReq
            }, "Id", "Description"));

        [Display(Name = "Institution Type:")]
        public SelectList InstitutionTypeSelectList
        {
            get
            {
                if (_institutionTypeSelectList != null)
                {
                    return _institutionTypeSelectList;
                }

                var test = new List<InstitutionType> { new InstitutionType { Id = 0, Name = "Not Set" } };

                if (_territories != null)
                {
                    test.AddRange(_institutionTypes);
                }

                return _institutionTypeSelectList ?? (_institutionTypeSelectList = new SelectList(test, "Id", "Name"));
            }
        }

        [Display(Name = "Territory:")]
        public SelectList TerritorySelectList
        {
            get
            {
                if (_territorySelectList != null)
                {
                    return _territorySelectList;
                }

                var test = new List<ITerritory> { new Territory { Id = 0, Name = "Not Set" } };

                if (_territories != null)
                {
                    test.AddRange(_territories);
                }

                return _territorySelectList ?? (_territorySelectList = new SelectList(test, "Id", "Name"));
            }
        }
    }
}