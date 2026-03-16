#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NHibernate.Linq;
using NHibernate.Util;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public class AthensAuthenticationService
    {
        private readonly AuthenticatedInstitutionService _authenticatedInstitutionService;
        private readonly IQueryable<Institution> _institutions;
        private readonly ILog<AthensAuthenticationService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<User> _users;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public AthensAuthenticationService(ILog<AthensAuthenticationService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<User> users
            , IQueryable<Institution> institutions
            , AuthenticatedInstitutionService authenticatedInstitutionService
            , IUserSessionStorageService userSessionStorageService
        )
        {
            _log = log;
            _userSessionStorageService = userSessionStorageService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutions = institutions;
            _authenticatedInstitutionService = authenticatedInstitutionService;
            _users = users;
        }

        public AuthenticationResult AttemptAthensUserAuthentication(HttpRequestBase request, HttpResponseBase response)
        {
            var cookie = new AthensAuthenticationCookie();

            if (!cookie.Exists)
            {
                return AuthenticationResult.Failed();
            }

            AuthenticatedInstitution authenticatedInstitution;
            User user = null;
            DateTime dateFromUser;
            DateTime.TryParse(cookie.FormatedDate, out dateFromUser); // TODO: put in the cookie class

            _log.Debug(cookie.ToDebugString());

            var passiveAuthenticationStatus = _userSessionStorageService.GetPassiveAuthenticationStatus();

            if (dateFromUser == DateTime.MinValue || dateFromUser <= DateTime.Now.AddHours(-1) ||
                dateFromUser > DateTime.Now)
            {
                _log.Debug(
                    $"AuthenticationResult.Failed - dateParsed: {dateFromUser != DateTime.MinValue}, dateFromUser: {dateFromUser}");
                AthensAuthenticationCookie.ClearAthensCookie(response);
                passiveAuthenticationStatus.ClearAthensIdentifier();
                return AuthenticationResult.Failed();
            }

            if (!string.IsNullOrWhiteSpace(cookie.TargetedId))
            {
                authenticatedInstitution = AuthenticateAthensUser(cookie.Username, cookie.TargetedId);
                passiveAuthenticationStatus.SetAthensIdentifier(cookie.ScopedAffiliation, cookie.TargetedId,
                    authenticatedInstitution != null);
                if (authenticatedInstitution != null)
                {
                    return AuthenticationResult.Successful(authenticatedInstitution);
                }
            }

            if (!string.IsNullOrWhiteSpace(cookie.ScopedAffiliation) ||
                !string.IsNullOrWhiteSpace(cookie.OrganizationId))
            {
                authenticatedInstitution =
                    AuthenticateAthensInstitution(cookie.OrganizationId, cookie.ScopedAffiliation);
                passiveAuthenticationStatus.SetAthensIdentifier(cookie.ScopedAffiliation, cookie.TargetedId,
                    authenticatedInstitution != null);
                return authenticatedInstitution == null
                    ? AuthenticationResult.Failed()
                    : AuthenticationResult.Successful(authenticatedInstitution, cookie.TargetedId);
            }

            return AuthenticationResult.Failed();
        }

        private AuthenticatedInstitution AuthenticateAthensUser(string username, string targetedId)
        {
            AuthenticatedInstitution authenticatedInstitution = null;
            List<User> users;

            if (!string.IsNullOrWhiteSpace(targetedId))
            {
                users = GetBaseUsers().Where(x => x.AthensTargetedId == targetedId).ToList();

                if (users.Count == 0 && !string.IsNullOrWhiteSpace(username))
                {
                    users = GetBaseUsers().Where(x => x.AthensUserName == username && x.AthensTargetedId == null)
                        .ToList();
                }
            }
            else
            {
                return null;
            }

            _log.DebugFormat("users.Count: {0}", users.Count);

            if (users.Count == 1)
            {
                var user = users.First();
                _log.Info($"Athens auth: User Id: {user.Id}, username: {user.UserName}");
                if (string.IsNullOrWhiteSpace(user.AthensTargetedId) && !string.IsNullOrWhiteSpace(targetedId))
                {
                    InsertAthensUserTargetedId(user, targetedId);
                }

                authenticatedInstitution =
                    _authenticatedInstitutionService.GetAuthenticatedInstitution(user,
                        AuthenticationMethods.AthensUser);
            }
            else if (users.Count > 1)
            {
                var msg = new StringBuilder("Duplicate Athens IDs:").AppendLine();
                foreach (var u in users)
                {
                    msg.Append(
                            $"UserId: {u.Id}, UserName: {u.UserName}, AthensUserName: {u.AthensUserName}, AthensTargetedId: {u.AthensTargetedId}, AthensPersistentUid: {u.AthensPersistentUid}")
                        .AppendLine();
                }

                _log.Error(msg.ToString());
            }

            return authenticatedInstitution;
        }

        private AuthenticatedInstitution AuthenticateAthensInstitution(string organizationId, string scopedAffiliation)
        {
            var affiliation = scopedAffiliation != null && scopedAffiliation.Contains("@")
                ? scopedAffiliation.Substring(scopedAffiliation.IndexOf("@", StringComparison.Ordinal) + 1)
                : null;
            IList<Institution> institutions = FindAthensInstitutions(organizationId, affiliation);

            var athensInstitution = institutions.FirstOrDefault();

            if (athensInstitution == null)
            {
                //Try to parse non-standard scopedAffiliation
                affiliation = ParseNonStandardScopedAffiliation(scopedAffiliation);

                institutions = FindAthensInstitutions(organizationId, affiliation);
                athensInstitution = institutions.FirstOrDefault();
            }

            _log.Debug($"institutions.Count: {institutions.Count}");

            if (athensInstitution != null)
            {
                _log.Info(
                    $"Athens auth: Institution Id: {athensInstitution.Id}, Account Number: {athensInstitution.AccountNumber}, Name: {athensInstitution.Name}");
                if (institutions.Count > 1)
                {
                    var msg = new StringBuilder("Duplicate Athens IDs:").AppendLine();
                    foreach (var inst in institutions)
                    {
                        msg.AppendFormat("Id: {0} - [{1}] - {2}", inst.Id, inst.AccountNumber, inst.Name).AppendLine();
                    }

                    _log.Error(msg.ToString());
                }
            }

            if (athensInstitution == null)
            {
                _log.Debug("AuthenticationResult.Failed - Insitution is null");
                return null;
            }

            return _authenticatedInstitutionService.GetAuthenticatedInstitution(athensInstitution.Id,
                AuthenticationMethods.AthensInstitution);
        }

        private static string ParseNonStandardScopedAffiliation(string scopedAffiliation)
        {
            //Squish #1216 – athens access issue

            string affiliation = null;

            //Use an email regex - this should be equivalent(-ish) to a regex for finding affiliations within a delimited (and possibly garbled) string.
            var emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);

            //Attempt to find valid affiliations within the scopedAffiliation string
            var matches = emailRegex.Matches(scopedAffiliation);

            if (matches.Any())
            {
                var firstMatch = matches[0].Value;
                affiliation = firstMatch.Substring(firstMatch.IndexOf("@", StringComparison.Ordinal) + 1);
            }

            return affiliation;
        }

        private List<Institution> FindAthensInstitutions(string organizationId, string affiliation)
        {
            return organizationId.Length > 4
                    ? _institutions.Where(x =>
                        x.AthensOrgId.Contains(organizationId) || x.AthensAffiliation.Contains(affiliation)).ToList()
                    : _institutions.Where(x => x.AthensAffiliation.Contains(affiliation)).ToList()
                ;
        }

        public void InsertAthensUserTargetedId(IUser user, string targetedId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var sql = new StringBuilder()
                        .Append("UPDATE tUser ")
                        .Append("   SET vchUpdaterId = :updaterId ")
                        .Append("      ,dtLastUpdate = getdate() ")
                        .Append("      ,vchAthensTargetedId = :targetedId ")
                        .Append(" WHERE iuserId = :userId ")
                        .ToString();

                    var query = uow.Session.CreateSQLQuery(sql);
                    query.SetParameter("updaterId", $"userId: {user.FirstName}, [{user.Id}]");
                    query.SetParameter("targetedId", targetedId);
                    query.SetParameter("userId", user.Id);

                    query.ExecuteUpdate();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void RemoveAthensUserTargetedId(IUser user)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var sql = new StringBuilder()
                        .Append("UPDATE tUser ")
                        .Append("   SET vchUpdaterId = :updaterId ")
                        .Append("      ,dtLastUpdate = getdate() ")
                        .Append("      ,vchAthensTargetedId = null ")
                        .Append(" WHERE iuserId = :userId ")
                        .ToString();

                    var query = uow.Session.CreateSQLQuery(sql);
                    query.SetParameter("updaterId", $"userId: {user.FirstName}, [{user.Id}]");
                    query.SetParameter("userId", user.Id);

                    query.ExecuteUpdate();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void InsertAthensInstitutionAffiliation(Institution institution, string affiliation)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var sql = new StringBuilder()
                        .Append("UPDATE tInstitution ")
                        .Append("   SET vchUpdaterId = :updaterId ")
                        .Append("      ,dtLastUpdate = getdate() ")
                        .Append("      ,vchAthensScopedAffiliation = :affiliation ")
                        .Append(" WHERE iInstitutionId = :institutionId ")
                        .ToString();
                    var newAffiliation = string.Join(",", institution.AthensAffiliation, affiliation);
                    var query = uow.Session.CreateSQLQuery(sql);
                    query.SetParameter("updaterId", "InsertAthensInstitutionAffiliation");
                    query.SetParameter("affiliation", newAffiliation);
                    query.SetParameter("institutionId", institution.Id);

                    query.ExecuteUpdate();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private IQueryable<User> GetBaseUsers()
        {
            return _users
                .Fetch(x => x.Role)
                .Fetch(x => x.Department)
                .FetchMany(x => x.OptionValues).ThenFetch(x => x.Option).ThenFetch(x => x.Type);
        }
    }
}