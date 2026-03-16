#region

using System;
using System.Linq;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Models.Profile;
using User = R2V2.Core.Authentication.User;

#endregion

namespace R2V2.Web.Services
{
    public class UserService
    {
        private readonly Core.UserService _institutionService;
        private readonly UserOptionService _userOptionService;

        public UserService(Core.UserService institutionService, UserOptionService userOptionService)
        {
            _institutionService = institutionService;
            _userOptionService = userOptionService;
        }

        /// <summary>
        ///     Returns NULL if the user is reactivated and the password is not changed.
        /// </summary>
        public User ConvertToCoreUser(InstitutionUserEdit institutionUserEdit, bool isOwnProfile)
        {
            var coreUser =
                _institutionService.GetUser(institutionUserEdit.User.Id, institutionUserEdit.User.InstitutionId);

            if (coreUser.RecordStatus != institutionUserEdit.User.RecordStatus && institutionUserEdit.User.RecordStatus)
            {
                if ((string.IsNullOrWhiteSpace(institutionUserEdit.User.NewPassword) &&
                     string.IsNullOrWhiteSpace(institutionUserEdit.User.ConfirmPassword)) ||
                    institutionUserEdit.User.NewPassword != institutionUserEdit.User.ConfirmPassword)
                {
                    return null;
                }
            }

            if (isOwnProfile || (coreUser.UserName == coreUser.Institution.AccountNumber &&
                                 institutionUserEdit.User.Id != 0))
            {
                institutionUserEdit.User.Role = coreUser.Role;
                institutionUserEdit.User.UserName = coreUser.UserName;

                if (isOwnProfile)
                {
                    institutionUserEdit.User.ExpirationDate = coreUser.ExpirationDate;
                }
            }
            //The else is needed because you cannot delete yourself and also the Main IA cannot be deleted
            else
            {
                coreUser.RecordStatus = institutionUserEdit.User.RecordStatus;
            }

            if (institutionUserEdit.User.Department.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(institutionUserEdit.User.Department.Name) &&
                    string.IsNullOrWhiteSpace(institutionUserEdit.User.CustomDepartment))
                {
                    institutionUserEdit.User.Department = null;
                }
                else
                {
                    institutionUserEdit.User.Department.Name = institutionUserEdit.User.CustomDepartment;
                    institutionUserEdit.User.Department.Id = institutionUserEdit.User.CustomDepartmentId;

                    if (coreUser.Department != null && !coreUser.Department.List && coreUser.Department.Id > 0)
                    {
                        _institutionService.UpdateCustomDepartment(institutionUserEdit.User.Department,
                            coreUser.Department);
                    }
                    else
                    {
                        _institutionService.CreateCustomDepartment(institutionUserEdit.User.Department);
                    }
                }
            }

            if (institutionUserEdit.User.Role == null)
            {
                institutionUserEdit.User.Role = coreUser.Role ?? new Role { Code = RoleCode.USERS };
            }

            if (institutionUserEdit.User.Role.Code == RoleCode.RITADMIN ||
                institutionUserEdit.User.Role.Code == RoleCode.SALESASSOC)
            {
                _institutionService.SaveUserTerritories(institutionUserEdit.User.CurrentTerritoryIds,
                    institutionUserEdit.User.Id);
            }

            var hasRoleChanged = institutionUserEdit.User.Role.Id != coreUser.Role?.Id;

            coreUser = institutionUserEdit.User.ToCoreUser(coreUser);

            if (!string.IsNullOrWhiteSpace(institutionUserEdit.User.NewPassword) &&
                !string.IsNullOrWhiteSpace(institutionUserEdit.User.ConfirmPassword) &&
                institutionUserEdit.User.NewPassword == institutionUserEdit.User.ConfirmPassword)
            {
                //coreUser.Password = institutionUserEdit.User.NewPassword;
                coreUser.LastPasswordChange = DateTime.Now;
                coreUser.LoginAttempts = 0;

                coreUser.PasswordSalt = PasswordService.GenerateNewSalt();
                coreUser.PasswordHash =
                    PasswordService.GenerateSlowPasswordHash(institutionUserEdit.User.NewPassword,
                        coreUser.PasswordSalt);
            }

            if (hasRoleChanged || coreUser.Id == 0)
            {
                _userOptionService.SetUserOptionValues(coreUser);
            }

            return coreUser;
        }

        public User ConvertToCoreUser(ProfileEdit profileEdit)
        {
            var coreUser = _institutionService.GetUser(profileEdit.User.Id, profileEdit.User.InstitutionId);

            profileEdit.User.Role = coreUser.Role;

            if (profileEdit.User.Department.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(profileEdit.User.Department.Name) &&
                    string.IsNullOrWhiteSpace(profileEdit.User.CustomDepartment))
                {
                    profileEdit.User.Department = null;
                }
                else
                {
                    profileEdit.User.Department.Name = profileEdit.User.CustomDepartment;
                    profileEdit.User.Department.Id = profileEdit.User.CustomDepartmentId;

                    if (coreUser.Department != null && !coreUser.Department.List && coreUser.Department.Id > 0)
                    {
                        _institutionService.UpdateCustomDepartment(profileEdit.User.Department, coreUser.Department);
                    }
                    else
                    {
                        _institutionService.CreateCustomDepartment(profileEdit.User.Department);
                    }
                }
            }

            coreUser = profileEdit.User.ToCoreUser(coreUser);

            return coreUser;
        }

        public User ConvertToNewCoreUser(ProfileAdd profileAdd)
        {
            var coreUser = new User
            {
                InstitutionId = profileAdd.User.InstitutionId,
                UserName = profileAdd.UserName,
                Password = profileAdd.NewPassword
            };

            profileAdd.User.Role = new Role { Code = RoleCode.USERS, Id = (int)RoleCode.USERS };

            if (profileAdd.User.Department.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(profileAdd.User.Department.Name) &&
                    string.IsNullOrWhiteSpace(profileAdd.User.CustomDepartment))
                {
                    profileAdd.User.Department = null;
                }
                else
                {
                    profileAdd.User.Department.Name = profileAdd.User.CustomDepartment;
                    profileAdd.User.Department.Id = profileAdd.User.CustomDepartmentId;
                    _institutionService.CreateCustomDepartment(profileAdd.User.Department);
                }
            }

            coreUser = profileAdd.User.ToCoreUser(coreUser);
            return coreUser;
        }


        public bool SaveUserEmailOptions(IUser user, IUserEmailOptions emailOptions)
        {
            var subscribedEmailOptions =
                emailOptions.SubscribedOptions != null ? emailOptions.SubscribedOptions.Split(',') : null;
            var unSubscribedEmailOptions = emailOptions.UnSubscribedOptions != null
                ? emailOptions.UnSubscribedOptions.Split(',')
                : null;

            foreach (var userOptionValue in user.OptionValues.Where(x => x.Option.Type.Id == (int)OptionTypeCode.EMAIL))
            {
                if (subscribedEmailOptions != null &&
                    subscribedEmailOptions.Contains(Enum.GetName(typeof(UserOptionCode), userOptionValue.Option.Code)))
                {
                    userOptionValue.Value = "1";
                }
                else if (unSubscribedEmailOptions != null &&
                         unSubscribedEmailOptions.Contains(Enum.GetName(typeof(UserOptionCode),
                             userOptionValue.Option.Code)))
                {
                    userOptionValue.Value = "0";
                }
            }

            _userOptionService.SaveUserOptionValues(user);

            //_userOptionService.SaveUserOptionValues(optionValuesToSave);
            return true;
        }
    }
}