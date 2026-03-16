#region

using System;
using System.Collections.Generic;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Models.Profile
{
    public static class UserExtensions
    {
        public static UserEdit ToUserEdit(this IUser user, List<Department> departments)
        {
            var userEdit = new UserEdit
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                InstitutionId = user.InstitutionId.GetValueOrDefault(),
                Department = user.Department,
                Departments = departments,
                AthensTargetedId = user.AthensTargetedId
            };

            if (userEdit.Department != null && userEdit.Department.Code == null && !userEdit.Department.List)
            {
                userEdit.CustomDepartment = userEdit.Department.Name;
                userEdit.CustomDepartmentId = userEdit.Department.Id;
            }

            return userEdit;
        }

        public static User ToCoreUser(this UserEdit userEdit, User user)
        {
            user.FirstName = userEdit.FirstName;
            user.LastName = userEdit.LastName;
            user.Email = userEdit.Email;

            user.Department = userEdit.Department;

            user.Role = new Role { Code = userEdit.Role.Code, Id = (int)userEdit.Role.Code };


            if (userEdit.ExpertReviewerRequest && userEdit.ExpertReviewerRequestDate == null)
            {
                user.ExpertReviewerRequestDate = DateTime.Now;
            }

            user.AthensTargetedId = userEdit.AthensTargetedId;

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                user.PasswordSalt = PasswordService.GenerateNewSalt();
                user.PasswordHash = PasswordService.GenerateSlowPasswordHash(user.Password, user.PasswordSalt);
            }

            return user;
        }
    }
}