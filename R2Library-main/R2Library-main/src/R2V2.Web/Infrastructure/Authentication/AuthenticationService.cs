#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using NHibernate.Linq;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AthensAuthenticationService _athensAuthenticationService;
        private readonly AuthenticatedInstitutionService _authenticatedInstitutionService;
        private readonly IClientSettings _clientSettings;
        private readonly IQueryable<InstitutionReferrer> _institutionReferrer;
        private readonly IQueryable<IpAddressRange> _ipRanges;
        private readonly ILog<AuthenticationService> _log;
        private readonly IQueryable<LoginFailure> _loginFailures;
        private readonly PassiveAuthenticationCookieService _passiveAuthenticationCookieService;
        private readonly IQueryable<PublisherUser> _publisherUsers;
        private readonly IResourceService _resourceService;
        private readonly TrustedAuthenticationService _trustedAuthenticationService;

        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserOptionService _userOptionService;
        private readonly IQueryable<User> _users;
        private readonly UserService _userService;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public AuthenticationService(ILog<AuthenticationService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<User> users
            , IQueryable<IpAddressRange> ipRanges
            , IQueryable<InstitutionReferrer> institutionReferrer
            , IQueryable<PublisherUser> publisherUsers
            , IQueryable<LoginFailure> loginFailures
            , IResourceService resourceService
            , AuthenticatedInstitutionService authenticatedInstitutionService
            , TrustedAuthenticationService trustedAuthenticationService
            , IClientSettings clientSettings
            , UserService userService
            , UserOptionService userOptionService
            , PassiveAuthenticationCookieService passiveAuthenticationCookieService
            , IUserSessionStorageService userSessionStorageService
            , AthensAuthenticationService athensAuthenticationService
        )
        {
            _log = log;
            _institutionReferrer = institutionReferrer;
            _publisherUsers = publisherUsers;
            _trustedAuthenticationService = trustedAuthenticationService;
            _clientSettings = clientSettings;
            _userService = userService;
            _userOptionService = userOptionService;
            _passiveAuthenticationCookieService = passiveAuthenticationCookieService;
            _userSessionStorageService = userSessionStorageService;
            _athensAuthenticationService = athensAuthenticationService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _loginFailures = loginFailures;
            _resourceService = resourceService;
            _authenticatedInstitutionService = authenticatedInstitutionService;
            _users = users;
            _ipRanges = ipRanges;
        }

        public AuthenticationResult Login(string userName, string password, HttpRequestBase request, int institutionId,
            string countryCode)
        {
            var authenticationResult = AuthenticationResult.Failed();

            if (IsIpAddressBlocked(request))
            {
                return AuthenticationResult.Blocked();
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    _log.WarnFormat("username and/or password is null, '{0}'/'{1}'", userName, password);
                    LogLoginFailureAttempt(request, userName, institutionId, countryCode, false);
                    return authenticationResult;
                }

                if (userName.Length < 3 || userName.Length > 50 || password.Length < 3 || password.Length > 20)
                {
                    _log.WarnFormat(
                        "Invalid username and/or password length - username length: {0}, password length: {1}",
                        userName.Length, password.Length);
                    LogLoginFailureAttempt(request, userName, institutionId, countryCode, false);
                    return authenticationResult;
                }

                var user = GetUser(userName, password);

                if (user != null)
                {
                    if (AutoLockUser(user))
                    {
                        LogLoginFailureAttempt(request, userName, institutionId, countryCode, false);
                        return AuthenticationResult.AutoLocked(user);
                    }

                    if (IsAttemptLocked(user))
                    {
                        LogLoginFailureAttempt(request, userName, institutionId, countryCode, true);
                        return AuthenticationResult.AttemptLocked();
                    }

                    _log.InfoFormat("User has authenticated, Id: {0}, username: {1}", user.Id, user.UserName);
                    user.LastSession = DateTime.Now;

                    UpdateUserLastLoginDate(user.Id, user.LastSession.Value);
                    var authenticatedInstitution =
                        _authenticatedInstitutionService.GetAuthenticatedInstitution(user,
                            AuthenticationMethods.UsernameAndPassword);
                    if (authenticatedInstitution != null)
                    {
                        authenticationResult = AuthenticationResult.Successful(authenticatedInstitution);
                    }
                }
                else
                {
                    var publisherUser = GetPublisherUser(userName, password);
                    if (publisherUser != null)
                    {
                        var authenticatedInstitution = _authenticatedInstitutionService.GetAuthenticatedInstitution(
                            publisherUser,
                            AuthenticationMethods.UsernameAndPassword);
                        if (authenticatedInstitution != null)
                        {
                            authenticationResult = AuthenticationResult.Successful(authenticatedInstitution);
                        }
                    }
                    else
                    {
                        var userFound = LogLoginFailureAttempt(request, userName, institutionId, countryCode, true);
                        if (userFound)
                        {
                            var foundUser = _userService.GetUser(userName);
                            if (IsAttemptLocked(foundUser))
                            {
                                return AuthenticationResult.AttemptLocked();
                            }
                        }
                    }
                }

                uow.Clear();
            }

            return authenticationResult;
        }

        public PublisherUser GetPublisherUser(string userName, string password)
        {
            PublisherUser publisherUser;
            using (var uow = _unitOfWorkProvider.Start())
            {
                publisherUser = _publisherUsers
                    .Fetch(x => x.Publisher)
                    .Fetch(x => x.Role)
                    .SingleOrDefault(x =>
                        x.UserName.ToLower() == userName.Trim().ToLower() && x.Password == password.Trim());
                uow.Clear();
            }

            return publisherUser;
        }

        public bool IsIpAddressBlocked(HttpRequestBase request)
        {
            var strIpAddress = request.GetHostIpAddress();
            if (string.IsNullOrWhiteSpace(strIpAddress))
            {
                _log.Warn("IP address is null or empty");
                return true;
            }

            var ipAddress = IPAddress.Parse(strIpAddress);
            var clientIpNumber = ipAddress.ToIpNumber();
            _log.DebugFormat("IsIpAddressBlocked() - IP: {0}, IP number: {0}", strIpAddress, clientIpNumber);

            var loginFailureCount = _loginFailures
                .Where(x => x.IpNumericValue == clientIpNumber)
                .Count(x => x.LoginFailureDate > DateTime.Now.AddMinutes(-60));
            _log.DebugFormat("loginFailureCount: {0}", loginFailureCount);
            return loginFailureCount >= 25;
        }


        /// <summary>
        ///     This is needed to prevent the update fields from being set.
        /// </summary>
        public bool UpdateUserLastLoginDate(int userId, DateTime lastLoginDate)
        {
            // TODO: SJS - 10/14/2015 - Verify creating a new unit of work is needed. I think there is one case in the code where we needed to, however now that
            // code has beed cut and pasted multiple time to other locations and I'm not 100%
            // DO NOT USE UnitOfWorkScope.New UNLESS YOU REALLY NEED TO. IF YOU DON"T UNDERSTAND WHY ASK ME OR GOOGLE IT!
            using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.New))
            {
                var query = uow.Session.CreateSQLQuery(
                    "update tUser set dtLastSession = :lastSessionDate, iLoginAttempts = 0 where iUserId = :userId");
                query.SetParameter("lastSessionDate", lastLoginDate);
                query.SetParameter("userId", userId);
                var rows = query.ExecuteUpdate();
                _log.DebugFormat("rows updated: {0}, userId: {1}, lastLoginDate: {2}", rows, userId, lastLoginDate);
                return rows > 0;
            }
        }

        /// <summary>
        ///     Used to reload a user when something with the Institution changes.
        /// </summary>
        public AuthenticationResult ReloadUser(int userId)
        {
            _log.DebugFormat("ReloadUser(userId: {0})", userId);
            var authenticationResult = AuthenticationResult.Failed();
            using (var uow = _unitOfWorkProvider.Start())
            {
                IUserWithFolders user = GetBaseUsers().SingleOrDefault(x => x.Id == userId);

                if (user != null)
                {
                    var authenticatedInstitution = _authenticatedInstitutionService.GetAuthenticatedInstitution(user,
                        AuthenticationMethods
                            .UsernameAndPassword);
                    if (authenticatedInstitution != null)
                    {
                        authenticationResult = AuthenticationResult.Successful(authenticatedInstitution);
                    }
                }

                uow.Clear();
            }

            return authenticationResult;
        }

        /// <summary>
        ///     Passive authentication - Trusted authentication, Referrer authentication, and IP authentication
        /// </summary>
        public AuthenticationResult AttemptPassiveAuthentication(HttpRequestBase request, HttpResponseBase response)
        {
            var passiveAuthenticationStatus = _userSessionStorageService.GetPassiveAuthenticationStatus();

            // 1st - attempt Athens authentication
            // 2st - attempt trusted authentication since this is the preferred method, but is not used often.
            // 3nd - attempt referrer authentication before IP so referrer authentication can be used in cases where there are overlapping IPs
            // 4rd - attempt IP authentication
            _log.Debug("Attempting passive authentication");
            var attemptAthensAuthentication =
                _athensAuthenticationService.AttemptAthensUserAuthentication(request, response);
            if (attemptAthensAuthentication.WasSuccessful)
            {
                return attemptAthensAuthentication;
            }

            var method = AuthenticationMethods.Undefined;
            int institutionId;
            if ((institutionId = AttemptTrustedAuthentication(request, passiveAuthenticationStatus)) > 0)
            {
                method = AuthenticationMethods.Trusted;
            }
            else if ((institutionId = AttemptReferrerAuthentication(request, passiveAuthenticationStatus)) > 0)
            {
                method = AuthenticationMethods.Referrer;
            }
            else if ((institutionId = AttemptIpAuthentication(request, passiveAuthenticationStatus)) > 0)
            {
                method = AuthenticationMethods.IP;
            }
            else if ((institutionId = _passiveAuthenticationCookieService.GetInstitutionIdFromPassiveAuthCookie()) > 0)
            {
                method = AuthenticationMethods.PassiveReauth;
            }

            if (institutionId <= 0)
            {
                _log.Info("Passive authentication failed");
                return AuthenticationResult.Failed();
            }

            var authenticatedInstitution =
                _authenticatedInstitutionService.GetAuthenticatedInstitution(institutionId, method);
            if (authenticatedInstitution == null)
            {
                _log.InfoFormat("Passive authentication failed, institution id: {0}", institutionId);
                return AuthenticationResult.Failed();
            }

            _log.InfoFormat(
                "AttemptPassiveAuthentication() - authenticated institution, Id: {0}, name: {1}, resource license count: {2}",
                institutionId, authenticatedInstitution.Name,
                authenticatedInstitution.LicensedResourceCount);

            if (authenticatedInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id)
            {
                var resources = _resourceService.GetAllResources();
                _log.DebugFormat("calling AddTrialLicenses() - Id: {0}, account number: {1}",
                    authenticatedInstitution.Id, authenticatedInstitution.AccountNumber);
                authenticatedInstitution.AddTrialLicenses(resources);
            }

            return AuthenticationResult.Successful(authenticatedInstitution);
        }

        public bool AutoLockUser(User user)
        {
            switch (user.Role.Code)
            {
                case RoleCode.RITADMIN:
                    return IsAutoLocked(_clientSettings.AutoLockRittenhouseAdmin, user);
                case RoleCode.SALESASSOC:
                    return IsAutoLocked(_clientSettings.AutoLockSalesAdmin, user);
                case RoleCode.ExpertReviewer:
                    return IsAutoLocked(_clientSettings.AutoLockExpertReviewer, user);
                case RoleCode.INSTADMIN:
                    return IsAutoLocked(_clientSettings.AutoLockInstitutionAdmin, user);
                default:
                    return IsAutoLocked(_clientSettings.AutoLockUser, user);
            }
        }

        public bool CheckUserLoginValid(string userName, string password)
        {
            return GetUser(userName, password) != null;
        }

        private User GetUser(string userName, string password)
        {
            var user = GetBaseUsers()
                .SingleOrDefault(x => x.UserName.ToLower() == userName.Trim().ToLower() &&
                                      (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                                      (x.Institution == null || (x.Institution.RecordStatus &&
                                                                 (
                                                                     x.Institution.AccountStatusId ==
                                                                     (int)AccountStatus.Active ||
                                                                     (x.Institution.AccountStatusId ==
                                                                      (int)AccountStatus.Trial &&
                                                                      x.Institution.Trial.EndDate > DateTime.Now)
                                                                 ))));
            var isValidPassword = false;
            if (user != null)
            {
                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    user.PasswordSalt = PasswordService.GenerateNewSalt();
                    user.PasswordHash = PasswordService.GenerateSlowPasswordHash(user.Password, user.PasswordSalt);
                    SavePasswordHashAndSalt(user);
                }

                isValidPassword = PasswordService.IsSlowPasswordCorrect(password, user.PasswordHash, user.PasswordSalt);
            }

            if (isValidPassword)
            {
                _userOptionService.SetUserOptionValues(user);
            }

            return isValidPassword ? user : null;
        }

        private bool LogLoginFailureAttempt(HttpRequestBase request, string username, int institutionId,
            string countryCode, bool logAttempt)
        {
            var strIpAddress = request.GetHostIpAddress();
            if (string.IsNullOrWhiteSpace(strIpAddress))
            {
                _log.Warn("IP address is null or empty");
                return false;
            }

            var ipAddress = IPAddress.Parse(strIpAddress);
            var clientIpNumber = ipAddress.ToIpNumber();
            _log.DebugFormat("LogLoginFailureAttempt() - IP: {0}, IP number: {0}", strIpAddress, clientIpNumber);

            try
            {
                var loginFailure = LoginFailure.CreateLoginFailure(institutionId, strIpAddress, countryCode,
                    username, DateTime.Now);
                using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.NewOrCurrent))
                {
                    var sql = new StringBuilder()
                        .Append("Insert into tLoginFailure(iInstitutionId, tiOctetA, tiOctetB, tiOctetC, tiOctetD ")
                        .Append("	, iIpNumericValue, vchCountryCode, dtLoginFailureDate, vchUsername) ")
                        .Append(
                            $"values({loginFailure.InstitutionId}, {loginFailure.OctetA}, {loginFailure.OctetB}, {loginFailure.OctetC}, {loginFailure.OctetD} ")
                        .Append(
                            $"	, {loginFailure.IpNumericValue}, '{loginFailure.CountryCode}', '{loginFailure.LoginFailureDate}', '{loginFailure.Username}') ")
                        .ToString();

                    var query = uow.Session.CreateSQLQuery(sql);
                    query.ExecuteUpdate();
                }

                if (logAttempt)
                {
                    return IncrementLoginAttempt(username);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        private bool IncrementLoginAttempt(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    return false;
                }

                var foundUser = _users.FirstOrDefault(x => x.UserName == userName);
                if (foundUser != null)
                {
                    foundUser.LoginAttempts = foundUser.LoginAttempts + 1;
                    using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.NewOrCurrent))
                    {
                        var query = uow.Session
                            .CreateSQLQuery("update tUser set iLoginAttempts = :loginAttempts where iUserId = :userId")
                            .SetParameter("userId", foundUser.Id)
                            .SetParameter("loginAttempts", foundUser.LoginAttempts);

                        var rows = query.ExecuteUpdate();
                        _log.Debug(
                            $"rows updated: {rows}, userId: {foundUser.Id}, loginAttempts: {foundUser.LoginAttempts}");
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        private void SavePasswordHashAndSalt(User user)
        {
            using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.NewOrCurrent))
            {
                var query = uow.Session.CreateSQLQuery(
                    "update tUser set vchPasswordHash = :passwordHash, vchPasswordSalt = :passwordSalt where iUserId = :userId");
                query.SetParameter("userId", user.Id);
                query.SetParameter("passwordHash", user.PasswordHash);
                query.SetParameter("passwordSalt", user.PasswordSalt);
                var rows = query.ExecuteUpdate();
                _log.DebugFormat("rows updated: {0}, userId: {1}, Password Hash Set", rows, user.Id);
            }
        }

        /// <summary>
        ///     Attempt IP authentication
        /// </summary>
        public int AttemptIpAuthentication(HttpRequestBase request,
            PassiveAuthenticationStatus passiveAuthenticationStatus)
        {
            var strIpAddress = request.GetHostIpAddress();
            if (string.IsNullOrWhiteSpace(strIpAddress))
            {
                _log.Warn("IP address is null or empty");
                return 0;
            }

            var ipAddress = IPAddress.Parse(strIpAddress);
            var clientIpNumber = ipAddress.ToIpNumber();

            var accountNumber = request.QueryString["accountNumber"];

            //For IP Plus, we want to re-attempt IP authentication if the querystring contains the account number.
            //This prevents users from having future requests "locked out" of IP Plus if they log out of R2 Library,
            // or initially come to R2 Library without the account number in the URL.
            //Ideally,
            // See https://www.squishlist.com/technotects/r2v2/1229/
            if (passiveAuthenticationStatus.WasIpAddressAuthPreviouslyAttempted(strIpAddress) &&
                string.IsNullOrEmpty(accountNumber))
            {
                _log.Debug($"AttemptIpAuthentication() - ip auth was previously attempted: '{ipAddress}'");
                return 0;
            }

            _log.DebugFormat("AttemptIpAuthentication() - ip: {0}, ip number: {0}", strIpAddress, clientIpNumber);

            var ipRanges = _ipRanges
                    .Fetch(ip => ip.Institution)
                    .Where(ip => ip.IpNumberStart <= clientIpNumber && ip.IpNumberEnd >= clientIpNumber)
                    .Where(ip => ip.RecordStatus)
                    .Where(ip =>
                        ip.Institution.AccountStatusId == (int)AccountStatus.Active ||
                        (ip.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                         ip.Institution.Trial.EndDate > DateTime.Now))
                    .Where(x => !string.IsNullOrWhiteSpace(accountNumber)
                        ? x.Institution.EnableIpPlus && x.Institution.AccountNumber == accountNumber
                        : !x.Institution.EnableIpPlus)
                ;

            IList<IpAddressRange> ipAddressRanges = ipRanges.ToList();

            //IInstitution institution = null;
            var institutionId = 0;
            if (ipAddressRanges.Count > 0)
            {
                // IP address range found
                var ipAddressRange = ipAddressRanges.First();
                institutionId = ipAddressRange?.InstitutionId ?? 0;
                if (ipAddressRanges.Count > 1)
                {
                    if (!string.IsNullOrWhiteSpace(accountNumber))
                    {
                        foreach (var addressRange in ipAddressRanges)
                        {
                            var institution = addressRange.Institution;
                            if (institution.EnableIpPlus && institution.AccountNumber == accountNumber)
                            {
                                institutionId = institution.Id;
                                break;
                            }

                            institutionId = 0;
                        }
                    }
                    else
                    {
                        var msg = new StringBuilder();
                        foreach (var range in ipAddressRanges.Where(x => !x.Institution.EnableIpPlus))
                        {
                            msg.AppendLine(range.ToDebugString());
                        }

                        if (msg.Length > 0)
                        {
                            _log.Error($"Duplicate IP Ranges:\r\n{msg}");
                        }
                    }
                }
            }

            if (institutionId == 0)
            {
                _log.Info("IP authentication failed");
            }
            else
            {
                _passiveAuthenticationCookieService.SetPassiveAuthenticationCookie(institutionId, strIpAddress);
            }

            passiveAuthenticationStatus.AddIpAddress(strIpAddress, institutionId > 0);
            return institutionId;
        }

        private int AttemptTrustedAuthentication(HttpRequestBase request,
            PassiveAuthenticationStatus passiveAuthenticationStatus)
        {
            var accountNumber = request.QueryString["AcctNo"];
            var hashKey = request.QueryString["hash"];
            var timeStamp = request.QueryString["timestamp"];

            if (passiveAuthenticationStatus.WasTrustedAuthPreviouslyAttempted(hashKey))
            {
                _log.Debug($"AttemptIpAuthentication() - trusted auth was previously attempted: '{hashKey}'");
                return 0;
            }

            if (!string.IsNullOrWhiteSpace(accountNumber) && !string.IsNullOrWhiteSpace(hashKey) &&
                !string.IsNullOrWhiteSpace(timeStamp))
            {
                _log.DebugFormat("Attempting trusted auth,  accountNumber: {0}, hashKey: {1}, timeStamp: {2}",
                    accountNumber, hashKey, timeStamp);
                var institutionId =
                    _trustedAuthenticationService.AttemptTrustedAuthentication(accountNumber, hashKey, timeStamp);
                _log.DebugFormat("institutionId: {0}", institutionId);
                if (institutionId > 0)
                {
                    _passiveAuthenticationCookieService.SetPassiveAuthenticationCookie(institutionId,
                        request.GetHostIpAddress(), accountNumber, accountNumber, timeStamp);
                }

                passiveAuthenticationStatus.AddTrustedHash(hashKey, institutionId > 0);
                return institutionId;
            }

            _log.Debug("trusted auth NOT attempted, not all keys were supplied");
            return 0;
        }

        /// <summary>
        ///     Referrer Authentication
        ///     - HTTP referrer MUST start with referrer defined in database
        ///     - If multiple institutions found
        ///     if DB query string is specified, use that institution
        ///     if no DB query string, error and fail.
        /// </summary>
        private int AttemptReferrerAuthentication(HttpRequestBase request,
            PassiveAuthenticationStatus passiveAuthenticationStatus)
        {
            try
            {
                var httpReferrer = request.HttpReferrer();

                if (passiveAuthenticationStatus.WasReferrerAuthPreviouslyAttempted(httpReferrer))
                {
                    _log.Debug($"AttemptIpAuthentication() - referrer auth was previously attempted: '{httpReferrer}'");
                    return 0;
                }

                if (!string.IsNullOrWhiteSpace(httpReferrer))
                {
                    var accountNumber = request.QueryString["DB"];
                    _log.DebugFormat("referrerUrl: {0}, accountNumber: {1}", httpReferrer, accountNumber);

                    var institutionReferrers = _institutionReferrer.Where(x => httpReferrer.StartsWith(x.ValidReferer));
                    if (!string.IsNullOrWhiteSpace(accountNumber))
                    {
                        institutionReferrers =
                            institutionReferrers.Where(x => x.Institution.AccountNumber == accountNumber);
                    }

                    var referrers = institutionReferrers.ToList();
                    if (!referrers.Any())
                    {
                        return 0;
                    }

                    if (referrers.Count() == 1)
                    {
                        var referrer = referrers.FirstOrDefault();
                        if (referrer != null)
                        {
                            _log.DebugFormat("Referrer FOUND - {0}", referrer.ToDebugString());
                            if (referrer.InstitutionId > 0)
                            {
                                _passiveAuthenticationCookieService.SetPassiveAuthenticationCookie(
                                    referrer.InstitutionId, request.GetHostIpAddress(), httpReferrer, accountNumber);
                            }

                            passiveAuthenticationStatus.AddAuthReferrer(httpReferrer, referrer.InstitutionId > 0);
                            return referrer.InstitutionId;
                        }

                        _log.ErrorFormat(
                            "InstitutionReferrer is null!!! --> SHOULD NEVER HAPPEN, referrer: {0}, account number: {1}",
                            httpReferrer, accountNumber);
                        passiveAuthenticationStatus.AddAuthReferrer(httpReferrer, false);
                        return 0;
                    }

                    // check if single or multiple institutions found
                    var institutionIds = new List<int>();
                    foreach (var referrer in referrers)
                    {
                        _log.DebugFormat(referrer.ToDebugString());
                        if (!institutionIds.Contains(referrer.InstitutionId))
                        {
                            institutionIds.Add(referrer.InstitutionId);
                        }
                    }

                    _log.DebugFormat("institutionIds.Count: {0}", institutionIds.Count);
                    if (institutionIds.Count == 1)
                    {
                        _log.DebugFormat("institutionId: {0}", institutionIds[0]);
                        if (institutionIds[0] > 0)
                        {
                            _passiveAuthenticationCookieService.SetPassiveAuthenticationCookie(institutionIds[0],
                                request.GetHostIpAddress(), httpReferrer, accountNumber);
                        }

                        passiveAuthenticationStatus.AddAuthReferrer(httpReferrer, institutionIds[0] > 0);
                        return institutionIds[0];
                    }

                    _log.WarnFormat("Multiple institution found, institution ids: {0}",
                        string.Join(",", institutionIds.ToArray()));
                    passiveAuthenticationStatus.AddAuthReferrer(httpReferrer, false);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder()
                    .AppendFormat(ex.Message, ex)
                    .AppendFormat("Headers Referer: {0}", request.Headers["Referer"])
                    .AppendFormat("ServerVariables HTTP_REFERER: {0}", request.ServerVariables["HTTP_REFERER"])
                    .ToString();

                _log.Error(errorMessage);
            }

            _log.Debug("referrer was null, referrer auth not attempted");
            return 0;
        }

        private bool IsAttemptLocked(User user)
        {
            if (user.LoginAttempts >= 5)
            {
                return true;
            }

            return false;
        }

        private static bool IsAutoLocked(int daysInPast, User user)
        {
            var isLocked = false;
            if (user.LastSession != null && user.LastSession < DateTime.Now.AddDays(-daysInPast))
            {
                isLocked = true;
            }
            else if (user.LastSession == null && user.CreationDate < DateTime.Now.AddDays(-daysInPast))
            {
                isLocked = true;
            }

            if (isLocked && user.LastPasswordChange != null &&
                user.LastPasswordChange > DateTime.Now.AddDays(-daysInPast))
            {
                isLocked = false;
            }

            return isLocked;
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