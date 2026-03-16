#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core
{
    public class UserService
    {
        private const string RaPromotionUsersKey = "R2V2.RA.Promotion.Users";
        private readonly IQueryable<Department> _departments;
        private readonly ILocalStorageService _localStorageService;

        private readonly ILog<UserService> _log;
        private readonly ITerritoryService _territoryService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserOptionService _userOptionService;
        private readonly IQueryable<User> _users;
        private readonly IQueryable<UserTerritory> _userTerritories;

        public UserService(ILog<UserService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<User> users
            , IQueryable<Department> departments
            , IQueryable<UserTerritory> userTerritories
            , ITerritoryService territoryService
            , UserOptionService userOptionService
            , ILocalStorageService localStorageService
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _users = users;
            _departments = departments;
            _userTerritories = userTerritories;
            _territoryService = territoryService;
            _userOptionService = userOptionService;
            _localStorageService = localStorageService;
        }

        private string GetCleanSearchTerm(IUserQuery userQuery)
        {
            var query = !string.IsNullOrWhiteSpace(userQuery.Query) ? userQuery.Query.ToLower().Trim() : null;
            //'
            return query?.Replace("'", "''");
        }

        public List<User> GetUsers(IUserQuery userQuery, ref int userCount, bool isExport = false)
        {
            var institutionId = userQuery.InstitutionId;
            var query = GetCleanSearchTerm(userQuery);

            //_unitOfWorkProvider.IncludeSoftDeletedValues();

            var baseUsers = GetBaseUsers().Where(x => x.Institution.RecordStatus);

            if (!string.IsNullOrWhiteSpace(query))
            {
                //TODO: Need way to fulltext search by email
                if (userQuery.SearchType.ToLower() == "email")
                {
                    baseUsers = baseUsers.Where(x => x.Email.Contains(query));
                }
                else
                {
                    var fullTextQuery = GetUserFullTextQuery(query, userQuery.SearchType);
                    _log.InfoFormat("Full-Text Query: {0}", fullTextQuery);

                    using (var uow = _unitOfWorkProvider.Start())
                    {
                        var query3 = uow.Session.CreateSQLQuery(fullTextQuery)
                            .AddEntity("u", typeof(User))
                            .AddJoin("i", "u.Institution") // Needed to fetch
                            .AddJoin("r", "u.Role") // Needed to fetch
                            .List();

                        baseUsers = query3.Cast<User>().AsQueryable();
                    }
                }
            }

            if (institutionId > 0)
            {
                baseUsers = baseUsers.Where(x => x.Institution.Id == institutionId);
            }

            baseUsers = baseUsers.Where(x => x.Role.Code != RoleCode.SUBUSER);

            IQueryable<User> topUsers;
            switch (userQuery.SortBy)
            {
                case "lastname":
                    topUsers = userQuery.SortDirection == SortDirection.Ascending
                        ? baseUsers.OrderBy(x => x.LastName == null ? "z" : x.LastName.Trim())
                        : baseUsers.OrderByDescending(x => x.LastName == null ? "z" : x.LastName.Trim());
                    break;

                case "firstname":
                    topUsers = userQuery.SortDirection == SortDirection.Ascending
                        ? baseUsers.OrderBy(x => x.FirstName == null ? "z" : x.FirstName.Trim())
                        : baseUsers.OrderByDescending(x => x.FirstName == null ? "z" : x.FirstName.Trim());
                    break;

                case "email":
                    topUsers = userQuery.SortDirection == SortDirection.Ascending
                        ? baseUsers.OrderBy(x => x.Email == null ? "z" : x.Email.Trim())
                        : baseUsers.OrderByDescending(x => x.Email == null ? "z" : x.Email.Trim());
                    break;

                case "department":
                    topUsers = userQuery.SortDirection == SortDirection.Ascending
                        ? baseUsers.OrderBy(x => x.Department == null ? "z" : x.Department.Name.Trim())
                        : baseUsers.OrderByDescending(x => x.Department == null ? "z" : x.Department.Name.Trim());
                    break;

                case "institution":
                    topUsers = userQuery.SortDirection == SortDirection.Ascending
                        ? baseUsers.OrderBy(x => x.Institution == null ? "z" : x.Institution.Name.Trim())
                        : baseUsers.OrderByDescending(x => x.Institution == null ? "z" : x.Institution.Name.Trim());
                    break;
                case "role":
                    topUsers = userQuery.SortDirection == SortDirection.Ascending
                        ? baseUsers.OrderBy(x => x.Role.Code)
                        : baseUsers.OrderByDescending(x => x.Role.Code);
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(query))
                    {
                        topUsers = baseUsers
                            .OrderBy(x => x.Role.Code)
                            .ThenByDescending(x => x.RecordStatus)
                            .ThenBy(x => x.LastName == null ? "z" : x.LastName.Trim());
                    }
                    else
                    {
                        topUsers = baseUsers;
                    }

                    break;
            }

            if (userQuery.RoleCode > 0)
            {
                topUsers = topUsers.Where(x => x.Role.Code == userQuery.RoleCode);
            }

            userCount = topUsers.Count();
            if (!isExport)
            {
                if (userCount > userQuery.Page * userQuery.PageSize && userQuery.Page > 1)
                {
                    topUsers = topUsers
                        .Skip((userQuery.Page - 1) * userQuery.PageSize)
                        .Take(userQuery.PageSize);
                }
                else if (userQuery.Page == 1)
                {
                    topUsers = topUsers
                        .Take(userQuery.PageSize);
                }
                else
                {
                    topUsers = topUsers
                        .Skip((userQuery.Page - 1) * userQuery.PageSize);
                }
            }

            List<User> users;

            if (string.IsNullOrWhiteSpace(query))
            {
                users = topUsers
                    .Fetch(x => x.Department)
                    .Fetch(x => x.Role)
                    .Fetch(x => x.Institution)
                    .ToList();
            }
            else
            {
                users = topUsers.ToList();
            }

            //_unitOfWorkProvider.ExcludeSoftDeletedValues();

            return users;
        }

        public string GetUserFullTextQuery(string query, string searchType)
        {
            var sb = new StringBuilder();

            switch (searchType)
            {
                case "Name":
                    return sb.Append("SELECT        {u.*}, {i.*}, {r.*}, ct.RANK ")
                        .Append("FROM            tUser AS u INNER JOIN ")
                        .Append(
                            "                         tInstitution AS i ON i.iInstitutionId = u.iInstitutionId and i.tiRecordStatus = 1 INNER JOIN ")
                        .Append("                         tRole AS r ON u.iRoleId = r.iRoleId INNER JOIN ")
                        .AppendFormat(
                            "                         FreeTextTable(tUser, (vchfirstname, vchlastname), '{0}') AS ct ON ct.[KEY] = u.iUserId ",
                            query)
                        .Append("ORDER BY ct.RANK DESC ")
                        .ToString();
                case "Institution":
                    return sb
                        .Append("SELECT        {u.*}, {i.*}, {r.*}, ct.RANK ")
                        .Append("FROM            tUser AS u INNER JOIN ")
                        .Append(
                            "                         tInstitution AS i ON i.iInstitutionId = u.iInstitutionId and i.tiRecordStatus = 1 INNER JOIN ")
                        .Append("                         tRole AS r ON u.iRoleId = r.iRoleId INNER JOIN ")
                        .AppendFormat(
                            "                         FreeTextTable(tInstitution, *, '{0}') AS ct ON ct.[KEY] = i.iInstitutionId ",
                            query)
                        .Append("ORDER BY ct.RANK DESC ")
                        .ToString();
                case "Email":
                    return sb.Append("SELECT        {u.*}, {i.*}, {r.*}, ct.RANK ")
                        .Append("FROM            tUser AS u INNER JOIN ")
                        .Append(
                            "                         tInstitution AS i ON i.iInstitutionId = u.iInstitutionId and i.tiRecordStatus = 1 INNER JOIN ")
                        .Append("                         tRole AS r ON u.iRoleId = r.iRoleId INNER JOIN ")
                        .AppendFormat(
                            "                         FreeTextTable(tUser, vchUserEmail, '{0}') AS ct ON ct.[KEY] = i.iInstitutionId ",
                            query)
                        .Append("ORDER BY ct.RANK DESC ")
                        .ToString();
                default:
                    if (!query.Contains("@"))
                    {
                        return sb.Append("SELECT        {u.*}, {i.*}, {r.*}, ct.RANK ")
                            .Append("FROM            tUser AS u INNER JOIN ")
                            .Append(
                                "                         tInstitution AS i ON i.iInstitutionId = u.iInstitutionId and i.tiRecordStatus = 1 INNER JOIN ")
                            .Append("                         tRole AS r ON u.iRoleId = r.iRoleId INNER JOIN ")
                            .AppendFormat(
                                "                         FreeTextTable(tUser, (vchfirstname, vchlastname), '{0}') AS ct ON ct.[KEY] = u.iUserId ",
                                query)
                            .Append("UNION ")
                            .Append("SELECT        {u.*}, {i.*}, {r.*}, ct.RANK ")
                            .Append("FROM            tUser AS u INNER JOIN ")
                            .Append(
                                "                         tInstitution AS i ON i.iInstitutionId = u.iInstitutionId and i.tiRecordStatus = 1 INNER JOIN ")
                            .Append("                         tRole AS r ON u.iRoleId = r.iRoleId INNER JOIN ")
                            .AppendFormat(
                                "                         FreeTextTable(tInstitution, *, '{0}') AS ct ON ct.[KEY] = i.iInstitutionId ",
                                query)
                            .Append("ORDER BY ct.RANK DESC ")
                            .ToString();
                    }

                    return sb.Append("SELECT        {u.*}, {i.*}, {r.*}, ct.RANK ")
                        .Append("FROM            tUser AS u INNER JOIN ")
                        .Append(
                            "                         tInstitution AS i ON i.iInstitutionId = u.iInstitutionId and i.tiRecordStatus = 1 INNER JOIN ")
                        .Append("                         tRole AS r ON u.iRoleId = r.iRoleId INNER JOIN ")
                        .AppendFormat(
                            "                         FreeTextTable(tUser, vchUserEmail, '{0}') AS ct ON ct.[KEY] = i.iInstitutionId ",
                            query)
                        .Append("ORDER BY ct.RANK DESC ")
                        .ToString();
            }
        }

        public User GetUser(int id)
        {
            //_unitOfWorkProvider.IncludeSoftDeletedValues();
            var user = GetBaseUsers().FirstOrDefault(x => x.Id == id);
            //_unitOfWorkProvider.ExcludeSoftDeletedValues();

            if (user != null)
            {
                _userOptionService.SetUserOptionValues(user);
            }

            return user;
        }

        public User GetUser(int userId, int institutionId)
        {
            return GetUser(userId) ?? new User
            {
                Department = new Department(), Role = new Role(), InstitutionId = institutionId,
                Institution = new Institution.Institution()
            };
        }

        /// <summary>
        ///     Only used for forgot password. Only pulls a base user not all objects that hang off the user.
        ///     Fixes an issue if the user is disabled will throw an error.
        /// </summary>
        public User GetUser(string userName)
        {
            return _users.FirstOrDefault(x => x.UserName == userName);
        }

        public IEnumerable<User> GetLibrarianUsers(int institutionId)
        {
            return GetBaseUsers()
                .Where(x => x.InstitutionId == institutionId)
                .HasOptionSelected(UserOptionCode.LibrarianAlert);
        }

        public IEnumerable<User> GetAdminUsers(int institutionId)
        {
            return GetBaseUsers().Where(x =>
                x.InstitutionId == institutionId && x.Role != null && x.Role.Code == RoleCode.INSTADMIN);
        }

        public Dictionary<Institution.Institution, User> GetAdminUsers(List<Institution.Institution> institutions)
        {
            //Dictionary<int, string> institutionIdsAndAccountNumbers = institutions.ToDictionary(x => x.Id, y => y.AccountNumber);
            //List<User> users = GetBaseUsers().Where(x=> institutionIdsAndAccountNumbers.Values.Contains(x.UserName)).ToList();
            //return institutions.ToDictionary(x => x, y => users.FirstOrDefault(z => z.InstitutionId == y.Id));

            var users = GetBaseUsers()
                .Where(u => u.Institution.AccountNumber == u.UserName)
                .ToList();

            return institutions.ToDictionary(x => x, y => users.FirstOrDefault(z => z.InstitutionId == y.Id));
        }

        public List<InstitutionExport> GetInstitutionExportList(List<Institution.Institution> institutions)
        {
            //Dictionary<int, string> institutionIdsAndAccountNumbers = institutions.ToDictionary(x => x.Id, y => y.AccountNumber);
            //List<User> users = GetBaseUsers().Where(x=> institutionIdsAndAccountNumbers.Values.Contains(x.UserName)).ToList();
            //return institutions.ToDictionary(x => x, y => users.FirstOrDefault(z => z.InstitutionId == y.Id));

            var users = GetBaseUsers().ToList();
            users = users.Where(u => u.IsInstitutionAdmin()).ToList();

            var list = new List<InstitutionExport>();
            institutions.ForEach(x =>
            {
                var item = new InstitutionExport
                {
                    Institution = x,
                    MainAdmin = users.FirstOrDefault(y => y.UserName == x.AccountNumber && x.Id == y.InstitutionId),
                    Admins = users
                        .Where(u => u.InstitutionId == x.Id && u.UserName != x.AccountNumber && u.RecordStatus).ToList()
                };
                list.Add(item);
            });
            return list;
        }

        public bool DoesUserNameAlreadyExist(User user)
        {
            //_unitOfWorkProvider.IncludeSoftDeletedValues();
            var existingUser = _users.FirstOrDefault(x => x.UserName.ToLower() == user.UserName.ToLower());
            if (existingUser != null && user.Id == existingUser.Id)
            {
                existingUser = null;
            }
            //_unitOfWorkProvider.ExcludeSoftDeletedValues();

            return existingUser != null;
        }

        public bool DoesUserNameAlreadyExist(string userName)
        {
            //_unitOfWorkProvider.IncludeSoftDeletedValues();
            var existingUser = _users.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower());
            //_unitOfWorkProvider.ExcludeSoftDeletedValues();
            return existingUser != null;
        }

        public bool DoesAthensTargetedIdAlreadyExist(string athensTargetedId)
        {
            //_unitOfWorkProvider.IncludeSoftDeletedValues();
            var existingUser = _users.FirstOrDefault(x => x.AthensTargetedId == athensTargetedId);
            //_unitOfWorkProvider.ExcludeSoftDeletedValues();
            return existingUser != null;
        }

        public void SaveUser(User user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    if (user.Id == 0)
                    {
                        uow.Save(user);
                    }
                    else
                    {
                        uow.Update(user);
                    }

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        ///     Will create and return a generated password and save the password to the user. This will also unlock the account.
        /// </summary>
        public string UserGenerateRandomPassword(User user)
        {
            //Have to save this way instead of just saving the user because they are not authenicated.
            // TODO: SJS - 10/14/2015 - Review the use of UnitOfWorkScope.New. I believe it should be UnitOfWorkScope.NewOrCurrent. Please change and test!
            using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.New))
            {
                var newPassword = CreateRandomPassword();

                var passwordSalt = PasswordService.GenerateNewSalt();
                var passwordHash = PasswordService.GenerateSlowPasswordHash(newPassword, passwordSalt);


                var query = uow.Session.CreateSQLQuery(
                    "update tUser set iLoginAttempts = 0, vchPasswordHash = :passwordHash, vchPasswordSalt = :passwordSalt, dtLastPasswordChange = :lastPasswordChange where iUserId = :userId");

                query.SetParameter("userId", user.Id);
                query.SetParameter("passwordSalt", passwordSalt);
                query.SetParameter("passwordHash", passwordHash);
                query.SetParameter("lastPasswordChange", DateTime.Now);


                var rows = query.ExecuteUpdate();
                _log.DebugFormat("rows updated: {0}, userId: {1}, newPassword: {2}", rows, user.Id, newPassword);
                return newPassword;
            }
        }

        private static string CreateRandomPassword()
        {
            var length = 7;

            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string specialCharacters = @"`~!@#$%^&*:,./\;'|-";

            var passwordBuilder = new StringBuilder();
            var rnd = new Random();
            while (0 < length--)
            {
                passwordBuilder.Append(valid[rnd.Next(valid.Length)]);
            }

            //Inserts Special character except in the first and last spot.
            var specialCharacterIndex = rnd.Next(specialCharacters.Length);
            var specialCharacter = specialCharacters[specialCharacterIndex];
            passwordBuilder.Insert(rnd.Next(5) + 1, specialCharacter);

            return passwordBuilder.ToString();
        }

        public void SaveGuestAndSubscriptionUser(GuestUser user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(user);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void SaveSubscriptionUser(User user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(user);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public User GetInstitutionAdministrator(int id)
        {
            return _users
                .Where(x => x.Institution.Id == id && x.Role.Code == RoleCode.INSTADMIN)
                .OrderBy(x => x.CreationDate)
                .FirstOrDefault();
        }

        public List<User> GetExpertReviewerRequestUsers(int institutionId)
        {
            return
                GetBaseUsers()
                    .Where(x => x.Institution.Id == institutionId)
                    .HasOptionSelected(UserOptionCode.ExpertReviewUserRequest)
                    .OrderBy(x => x.CreationDate).ToList();
        }

        public Department GetUserEditDepartment(Department originalDepartment, Department newDepartment)
        {
            //Create New Department
            if (newDepartment.Id == 0)
            {
                newDepartment.Code = newDepartment.Name;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Save(newDepartment);
                    uow.Commit();
                }

                return newDepartment;
            }

            if (originalDepartment != null && !originalDepartment.List)
            {
                originalDepartment.Name = newDepartment.Name;
                originalDepartment.Code = newDepartment.Code;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Update(originalDepartment);
                    uow.Commit();
                }
            }

            return originalDepartment;
        }

        public Department CreateCustomDepartment(Department department)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.Save(department);
                uow.Commit();
            }

            return department;
        }

        public Department UpdateCustomDepartment(Department newDepartment, Department originalDepartment)
        {
            originalDepartment.Name = newDepartment.Name;
            originalDepartment.Code = newDepartment.Code;
            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.Update(originalDepartment);
                uow.Commit();
            }

            return originalDepartment;
        }

        public List<Department> GetListDepartments()
        {
            return _departments.Where(x => x.List).OrderBy(x => x.Name).ToList();
        }

        public List<ITerritory> GetTerritories()
        {
            var territories = _territoryService.GetAllTerritories();
            return territories.ToList();
        }

        public List<UserTerritory> GetUserTerritories()
        {
            return (from ut in _userTerritories select ut).ToList();
        }

        public void SaveUserTerritories(int[] territoryIds, int userId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                // ReSharper disable once ImplicitlyCapturedClosure
                var territoriescurrent = _userTerritories.Where(x => x.UserId == userId);

                //Will clear the users Territories
                if (territoriescurrent.Any() && territoryIds == null)
                {
                    DeleteUserTerritories(territoriescurrent);
                    uow.Commit();
                    return;
                }

                if (territoryIds == null)
                {
                    return;
                }

                //gets all territories that are being added to the user except ones it already owns.
                var territoriesTakenByOthers = _userTerritories
                    .Where(x => territoryIds.Contains(x.TerritoryId) && x.UserId != userId).ToList();

                //gets all territories for the user that need to be deleted
                var territoriesToDelete = _userTerritories
                    .Where(x => !territoryIds.Contains(x.TerritoryId) && x.UserId == userId).ToList();

                if (territoriesTakenByOthers.Any())
                {
                    DeleteUserTerritories(territoriesTakenByOthers);
                }

                if (territoriesToDelete.Any())
                {
                    DeleteUserTerritories(territoriesToDelete);
                }

                foreach (var territoryId in territoryIds)
                {
                    var territory = territoriescurrent.FirstOrDefault(x => x.TerritoryId == territoryId);

                    if (territory == null)
                    {
                        var userTerritory = new UserTerritory
                        {
                            UserId = userId,
                            TerritoryId = territoryId
                        };

                        uow.Save(userTerritory);
                    }
                }

                uow.Commit();
            }
        }

        private void DeleteUserTerritories(IEnumerable<UserTerritory> territoriesToDelete)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                foreach (var userTerritory in territoriesToDelete)
                {
                    uow.Delete(userTerritory);
                }
            }
        }

        public List<User> GetExpertReviewers(int institutionId)
        {
            IQueryable<User> users = GetBaseUsers()
                .Where(x => x.InstitutionId == institutionId && x.Role.Id == (int)UserRole.ExpertReviewer.Id)
                .OrderBy(x => x.LastName);
            return users.ToList();
        }

        public IList<User> GetRaUsersWhoCanPromote()
        {
            var users = _localStorageService.Get<IList<User>>(RaPromotionUsersKey);
            if (users == null)
            {
                using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.New))
                {
                    try
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        GetUserBaseQueryWithTerritories()
                            .Where(x => x.Role.Code == RoleCode.RITADMIN && x.EnablePromotion != null &&
                                        x.EnablePromotion.Value > 0)
                            .ToFuture();

                        GetUserBaseQueryWithOptions()
                            .Where(x => x.Role.Code == RoleCode.RITADMIN && x.EnablePromotion != null &&
                                        x.EnablePromotion.Value > 0)
                            .ToFuture();

                        users = _users.Where(x =>
                            x.Role.Code == RoleCode.RITADMIN && x.EnablePromotion != null &&
                            x.EnablePromotion.Value > 0).ToList();

                        foreach (var user in users)
                        {
                            uow.Evict(user);
                        }

                        stopwatch.Stop();
                        _localStorageService.Put(RaPromotionUsersKey, users);
                        _log.DebugFormat("GetRaUsersWhoCanPromote() << - users.Count: {0} in {1} ms", users.Count,
                            stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        throw;
                    }
                }
            }

            return users;
        }

        private IQueryable<User> GetBaseUsers()
        {
            GetUserBaseQueryWithTerritories().ToFuture();
            GetUserBaseQueryWithOptions().ToFuture();

            return _users;
        }

        private IQueryable<User> GetUserBaseQueryWithTerritories()
        {
            return _users
                .FetchMany(x => x.UserTerritories)
                .Fetch(x => x.Institution)
                .Fetch(x => x.Role)
                .Fetch(x => x.Department);
        }

        private IQueryable<User> GetUserBaseQueryWithOptions()
        {
            return _users
                .FetchMany(x => x.OptionValues)
                .ThenFetch(x => x.Option)
                .ThenFetch(x => x.Type);
        }

        public void SaveUserResourceRequest(UserResourceRequest userResourceRequest)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.Save(userResourceRequest);
            }
        }


        public List<User> GetSubscriptionUsers()
        {
            return _users.Where(x => x.Role.Code == RoleCode.SUBUSER).ToList();
        }
    }

    public class InstitutionExport
    {
        public Institution.Institution Institution { get; set; }
        public User MainAdmin { get; set; }
        public List<User> Admins { get; set; }
    }
}