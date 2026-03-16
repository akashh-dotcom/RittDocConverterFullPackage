#region

using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Authentication
{
    public class UserOptionService
    {
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<UserOptionRole> _userOptionRoles;
        private readonly IQueryable<UserOptionValue> _userOptionValues;
        private readonly IQueryable<User> _users;

        public UserOptionService(
            IQueryable<UserOptionRole> userOptionRoles
            , IQueryable<UserOptionValue> userOptionValues
            , IQueryable<User> users
            , IUnitOfWorkProvider unitOfWorkProvider)
        {
            _userOptionRoles = userOptionRoles;
            _userOptionValues = userOptionValues;
            _users = users;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        /// <summary>
        ///     Sets Default Values for all options available based on the users role.
        /// </summary>
        public void SetUserOptionValues(IUser user)
        {
            if (user == null)
            {
                return;
            }

            var userOptionRoles = _userOptionRoles.Fetch(x => x.Option)
                .Where(x => x.Role.Id == user.Role.Id && x.RecordStatus).ToList();

            //Adds Default values
            foreach (var userOptionRole in userOptionRoles)
            {
                if (user.OptionValues == null)
                {
                    user.OptionValues = new List<UserOptionValue>();
                }

                var userOptionValue = user.OptionValues.FirstOrDefault(x => x.Option.Id == userOptionRole.Option.Id);
                if (userOptionValue == null)
                {
                    user.OptionValues.Add(new UserOptionValue
                    {
                        Value = userOptionRole.DefaultValue, UserOptionId = userOptionRole.Option.Id,
                        Option = userOptionRole.Option, RecordStatus = true, UserId = user.Id
                    });
                }
                else if (!userOptionValue.RecordStatus)
                {
                    userOptionValue.RecordStatus = true;
                }
            }

            //Deletes Values when role changes
            foreach (var optionValue in user.OptionValues.Where(x => x.RecordStatus))
            {
                var roleOption = userOptionRoles.FirstOrDefault(x =>
                    x.Role.Id == user.Role.Id && x.Option.Id == optionValue.Option.Id && optionValue.RecordStatus);
                if (roleOption == null)
                {
                    optionValue.RecordStatus = false;
                }
            }
        }

        public void SaveUserOptionValues(List<UserOptionValue> userOptionValues)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    foreach (var userOptionValue in userOptionValues)
                    {
                        if (userOptionValue.Id > 0)
                        {
                            uow.Update(userOptionValue);
                        }
                        else
                        {
                            uow.Save(userOptionValue);
                        }
                    }

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void SaveUserOptionValues(IUser user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var userOptionValues = user.OptionValues;
                    uow.Evict(user.OptionValues);

                    foreach (var userOptionValue in userOptionValues)
                    {
                        var dbUserOptionValue = _userOptionValues.FirstOrDefault(x =>
                            x.UserId == user.Id && x.UserOptionId == userOptionValue.UserOptionId);
                        if (dbUserOptionValue == null)
                        {
                            var newUserOptionValue = new UserOptionValue
                            {
                                UserId = user.Id, Value = userOptionValue.Value,
                                UserOptionId = userOptionValue.UserOptionId, RecordStatus = true,
                                Option = userOptionValue.Option
                            };
                            uow.Save(newUserOptionValue);
                        }
                        else if (!dbUserOptionValue.RecordStatus && userOptionValue.RecordStatus ||
                                 userOptionValue.Value != dbUserOptionValue.Value)
                        {
                            uow.Evict(dbUserOptionValue);
                            uow.Update(userOptionValue);
                        }
                    }

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void InsertNewUserOptionValues(int userId, int roleId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    _users.Fetch(x => x.Role).ToFuture();
                    var user = _users.FirstOrDefault(x => x.Id == userId);

                    var roleDefaultsWithOption = _userOptionRoles.Fetch(x => x.Option).Where(x => x.Role == user.Role);

                    foreach (var roleDefault in roleDefaultsWithOption)
                    {
                        var userOptionValue = new UserOptionValue
                        {
                            UserOptionId = roleDefault.Option.Id,
                            UserId = userId,
                            RecordStatus = true,
                            Value = roleDefault.DefaultValue
                        };
                        uow.Save(userOptionValue);
                    }

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }
    }
}