#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string RecordStatusText { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public Department Department { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string AthensTargetedId { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? LastSession { get; set; }

        public DateTime? LastPasswordChange { get; set; }
        public DateTime? ExpertReviewerRequestDate { get; set; }
        public bool IsLocked { get; set; }
        public bool DisplayLink { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int InActiveSubscriptions { get; set; }

        public string DisplayName()
        {
            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
            {
                return "Not Specified";
            }

            if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
            {
                return $"{LastName}, {FirstName}";
            }

            return $"{LastName}{FirstName}";
        }
    }
}