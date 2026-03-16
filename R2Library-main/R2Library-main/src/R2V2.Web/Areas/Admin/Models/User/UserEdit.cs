#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public class UserEdit
    {
        public List<SelectListItem> SelectedTerritoriesSelectListItems;

        public List<SelectListItem> TerritoriesSelectListItems;
        public int Id { get; set; }

        [Display(Name = @"First Name")]
        [Required]
        [StringLength(50, ErrorMessage = @"First Name Max Length is 50 and Minimum Length is 4 characters.",
            MinimumLength = 1)]
        public string FirstName { get; set; }

        [Display(Name = @"Last Name")]
        [Required]
        [StringLength(100, ErrorMessage = @"Last Name Max Length is 100 and Minimum Length is 2 characters.",
            MinimumLength = 2)]
        public string LastName { get; set; }

        [Display(Name = @"Username")]
        [Required]
        [StringLength(50, ErrorMessage = @"User Name Max Length is 50 and Minimum Length is 4 characters.",
            MinimumLength = 4)]
        public string UserName { get; set; }

        public string Password { get; set; }

        [Display(Name = @"New Password")]
        [PasswordValidation]
        public string NewPassword { get; set; }

        [Display(Name = @"Confirm Password")]
        [PasswordCompare("User.NewPassword", "User.ConfirmPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = @"Email Address")]
        [EmailAddress]
        [Required]
        [StringLength(100, ErrorMessage = @"Email Address Length is 100 and Minimum Length is 3 characters.",
            MinimumLength = 3)]
        public string Email { get; set; }

        [Display(Name = @"Status")] public bool RecordStatus { get; set; }

        [Display(Name = @"Status")] public SelectList StatusList { get; set; }

        public Role Role { get; set; }

        [Display(Name = @"User Role")] public SelectList RoleSelectList { get; set; }

        public int InstitutionId { get; set; }

        [Display(Name = @"Department")] public Department Department { get; set; }

        [Display(Name = @"Department")]
        [DepartmentValidation(ErrorMessage = "Department is required for Expert Reviewer Users")]
        public SelectList DepartmentSelectList { get; set; }

        [Display(Name = @"Enter your Department Name:")]
        public string CustomDepartment { get; set; }

        public int CustomDepartmentId { get; set; }

        [Display(Name = @"Expires:")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [DateTenYears("User.ExpirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = @"Athens Account:")] public string AthensTargetedId { get; set; }


        [Display(Name = @"Request Expert Reviewer User Role")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:g}")]
        public DateTime? ExpertReviewerRequestDate { get; set; }


        public DateTime? LastPasswordChange { get; set; }

        [Display(Name = @"Assign Territories: ")]
        public int[] CurrentTerritoryIds { get; set; }

        public bool DisplayExpertReviewerEmailOptions { get; set; }


        public int GetRowSpan(UserQuery query)
        {
            int rowSpan;
            if (query.IsUserIa)
            {
                rowSpan = DisplayExpertReviewerEmailOptions ? 15 : 13;
            }
            else
            {
                rowSpan = 2;
            }

            if (Role.Code == RoleCode.RITADMIN || Role.Code == RoleCode.SALESASSOC)
            {
                rowSpan++;
            }

            return rowSpan;
        }

        public void PopulateSelectLists(List<Department> departments, IUser currentUser, IAdminInstitution institution)
        {
            PopulateDepartmentSelectList(departments);
            PopulateUserRoleSelectList(currentUser, institution);
            PopulateUserStatusSelectList();
        }

        public void PopulateSelectLists(
            List<Department> departments
            , IUser currentUser
            , List<UserTerritory> usersWithTerritories
            , List<ITerritory> allTerritories
            , IAdminInstitution institution
        )
        {
            PopulateTerritoriesSelectList(usersWithTerritories, allTerritories);
            PopulateSelectLists(departments, currentUser, institution);
        }

        /// <summary>
        /// </summary>
        private void PopulateDepartmentSelectList(IEnumerable<Department> departments)
        {
            var items = new List<Department> { new Department { Id = 0, Name = "Enter Custom Department" } };
            items.AddRange(departments);
            DepartmentSelectList = new SelectList(items, "Id", "Name");

            if (items.Contains(Department) || Department == null)
            {
                return;
            }

            CustomDepartment = Department.Name;
            CustomDepartmentId = Department.Id;
        }

        private void PopulateUserRoleSelectList(IUser currentUser, IAdminInstitution institution)
        {
            var userRoles = new List<UserRole> { UserRole.User };
            if (currentUser != null && currentUser.Role != null && currentUser.Role.Code > 0)
            {
                switch (currentUser.Role.Code)
                {
                    case RoleCode.INSTADMIN:
                        userRoles.Add(UserRole.InstitutionAdministrator);
                        break;
                    case RoleCode.SALESASSOC:
                        userRoles.Add(UserRole.InstitutionAdministrator);
                        break;
                    case RoleCode.RITADMIN:
                        userRoles.Add(UserRole.SalesAssociate);
                        userRoles.Add(UserRole.InstitutionAdministrator);
                        userRoles.Add(UserRole.RittenhouseAdministrator);
                        break;
                }
            }

            if (institution.ExpertReviewerUserEnabled)
            {
                userRoles.Add(UserRole.ExpertReviewer);
            }

            RoleSelectList = new SelectList(userRoles, "Id", "Description");
        }

        private void PopulateUserStatusSelectList()
        {
            var userStatuses = new List<UserStatus>
            {
                new UserStatus { Description = "Active", Value = true },
                new UserStatus { Description = "Remove", Value = false }
            };

            StatusList = new SelectList(userStatuses, "Value", "Description");
        }

        private void PopulateTerritoriesSelectList(List<UserTerritory> usersWithTerritories,
            IEnumerable<ITerritory> allTerritories)
        {
            SelectedTerritoriesSelectListItems = new List<SelectListItem>();
            TerritoriesSelectListItems = new List<SelectListItem>();

            foreach (var territory in allTerritories)
            {
                if (CurrentTerritoryIds != null && CurrentTerritoryIds.Contains(territory.Id))
                {
                    SelectedTerritoriesSelectListItems.Add(new SelectListItem
                        { Value = territory.Id.ToString(), Text = territory.Code });
                    continue;
                }

                var usersWithTerritory = usersWithTerritories.FirstOrDefault(x => x.TerritoryId == territory.Id);

                TerritoriesSelectListItems.Add(usersWithTerritory == null
                    ? new SelectListItem
                    {
                        Text = territory.Code,
                        Value = territory.Id.ToString()
                    }
                    : new SelectListItem
                    {
                        Text =
                            $"{territory.Code} - {usersWithTerritory.User.LastName}, {usersWithTerritory.User.FirstName}",
                        Value = territory.Id.ToString()
                    }
                );
            }
        }
    }
}