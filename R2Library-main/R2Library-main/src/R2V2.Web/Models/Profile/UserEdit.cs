#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Models.Profile
{
    public class UserEdit
    {
        private SelectList _departmentSelectList;
        public int Id { get; set; }

        [Display(Name = @"First Name")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = @"Last Name")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = @"Current Password")] public string CurrentPassword { get; set; }

        [Display(Name = @"New Password")]
        [PasswordValidation]
        public string NewPassword { get; set; }

        [Display(Name = @"Confirm Password")]
        [PasswordCompare("User.NewPassword", "User.CurrentPassword",
            ErrorMessage = @"Current Password must be entered and/or Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = @"Email Address")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        public Role Role { get; set; }

        public bool DisplayExpertReviewerEmailOptions { get; set; }

        public int InstitutionId { get; set; }

        [Display(Name = @"Department")]
        [DepartmentValidation(ErrorMessage = @"Department is required for Expert Reviewer Users")]
        public Department Department { get; set; }

        public List<Department> Departments { get; set; }

        [Display(Name = @"Department")]
        public SelectList DepartmentSelectList
        {
            get
            {
                if (_departmentSelectList == null)
                {
                    var items = new List<Department> { new Department { Id = 0, Name = "Enter Custom Department" } };

                    if (Departments != null && Departments.Any())
                    {
                        items.AddRange(Departments);
                    }

                    _departmentSelectList = new SelectList(items, "Id", "Name");
                }

                return _departmentSelectList;
            }
        }

        [Display(Name = @"Enter your Department Name:")]
        public string CustomDepartment { get; set; }

        public int CustomDepartmentId { get; set; }

        [Display(Name = @"Athens Account:")] public string AthensTargetedId { get; set; }

        [Display(Name = @"Request Expert Reviewer User Rights: ")]
        public DateTime? ExpertReviewerRequestDate { get; set; }

        [Display(Name = @"Request Expert Reviewer User Rights: ")]
        public bool ExpertReviewerRequest { get; set; }
    }
}