#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Core.Trial;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.Authentication;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Email.EmailBuilders;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Trial;
using AccountStatus = R2V2.Web.Models.Trial.AccountStatus;
using Address = R2V2.Web.Areas.Admin.Models.Institution.Address;
using Institution = R2V2.Core.Institution.Institution;
using Trial = R2V2.Web.Models.Trial.Trial;
using User = R2V2.Core.Authentication.User;

#endregion

namespace R2V2.Web.Controllers
{
    public class TrialController : R2BaseController
    {
        private readonly AccountRequestEmailBuildService _accountRequestEmailBuildService;
        private readonly IAdminSettings _adminSettings;
        private readonly EmailSiteService _emailService;
        private readonly InstitutionService _institutionService;
        private readonly IQueryable<InstitutionType> _institutionTypes;
        private readonly ILog<TrialController> _log;
        private readonly IQueryable<PreludeCustomer> _preludeCustomers;
        private readonly ITerritoryService _territoryService;
        private readonly TrialService _trialService;
        private readonly TrialFactory _trialsService;

        /// <summary>
        /// </summary>
        /// <param name="institutionService"> </param>
        /// <param name="preludeCustomers"> </param>
        /// <param name="emailService"> </param>
        /// <param name="trialsService"> </param>
        /// <param name="adminSettings"> </param>
        public TrialController(ILog<TrialController> log
            , InstitutionService institutionService
            , IQueryable<PreludeCustomer> preludeCustomers
            , EmailSiteService emailService
            , TrialFactory trialsService
            , IAdminSettings adminSettings
            , ITerritoryService territoryService
            , UserOptionService userOptionService
            , TrialService trialService
            , AccountRequestEmailBuildService accountRequestEmailBuildService
            , IAuthenticationContext authenticationContext
            , IQueryable<InstitutionType> institutionTypes
        ) : base(authenticationContext)
        {
            _log = log;
            _institutionService = institutionService;
            _preludeCustomers = preludeCustomers;
            _emailService = emailService;
            _trialsService = trialsService;
            _adminSettings = adminSettings;
            _territoryService = territoryService;
            _trialService = trialService;
            _accountRequestEmailBuildService = accountRequestEmailBuildService;
            _institutionTypes = institutionTypes;
        }

        /// <param name="email"> </param>
        public ActionResult Index(string accountNumber, string timestamp, string hash, string email)
        {
            return RedirectToAction("Index", "Home");
            _log.Debug($"accountNumber: {accountNumber}");
            _log.Debug($"timestamp: {timestamp}");
            _log.Debug($"hash: {hash}");
            if (string.IsNullOrWhiteSpace(accountNumber) || string.IsNullOrWhiteSpace(timestamp) ||
                string.IsNullOrWhiteSpace(hash))
            {
                return RedirectToAction("Index", "Browse");
            }

            _log.DebugFormat("hash: {0}", Uri.UnescapeDataString(hash));

            var model = new Trial();
            if (IsValidHash(Uri.UnescapeDataString(hash), accountNumber, timestamp))
            {
                _log.Debug("THE HASH IS VALID!");

                var preludeCustomer = _preludeCustomers.FirstOrDefault(x => x.AccountNumber == accountNumber);

                if (preludeCustomer == null)
                {
                    var msg = GetErrorMessage($"Account Number Not Found In Prelude, Account Number: {accountNumber}"
                        , hash, accountNumber, timestamp);
                    _log.Error(msg);
                    model.ErrorMessage =
                        "Sorry, but the system was unable to process you trial request at this time.  Please contact Rittenhouse for assistance. (Invalid Hash.)";
                    model.Error = true;
                }
                else
                {
                    var address = preludeCustomer.Address != null
                        ? new Address
                        {
                            Address1 = preludeCustomer.Address.Address1,
                            Address2 = preludeCustomer.Address.Address2,
                            City = preludeCustomer.Address.City,
                            State = preludeCustomer.Address.State,
                            Zip = preludeCustomer.Address.Zip
                        }
                        : new Address();

                    model.Institution = new Areas.Admin.Models.Institution.Institution
                    {
                        Address = address,
                        AccountNumber = preludeCustomer.AccountNumber,
                        InstitutionName = preludeCustomer.Name,
                        Discount = 10,
                        TrialEndDate = DateTime.Now.AddMonths(1)
                    };

                    model.User = new UserEdit
                    {
                        Email = string.IsNullOrEmpty(email) ? preludeCustomer.AdministratorEmail : email,
                        UserName = preludeCustomer.AccountNumber
                    };
                }
            }
            else
            {
                var msg = GetErrorMessage("Invalid Hash", hash, accountNumber, timestamp);
                _log.Error(msg);
                model.ErrorMessage =
                    "Sorry, but the system was unable to process you trial request at this time.  Please contact Rittenhouse for assistance. (Invalid Hash.)";
                model.Error = true;
            }

            model.FormPost = "Index";
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Trial model)
        {
            return RedirectToAction("Index", "Home");
            _log.DebugFormat("accountNumber: {0}", model.AccountNumber);
            if (ModelState.IsValid)
            {
                if (IsValidHash(Uri.UnescapeDataString(model.Hash), model.AccountNumber, model.Timestamp))
                {
                    _log.Debug("THE HASH IS VALID!");
                    _log.DebugFormat("first: {0}, last: {1}, email: {2}", model.User.FirstName, model.User.LastName,
                        model.User.Email);

                    if (SaveInstitutionForTrial(model))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var msg = GetErrorMessage("Save Failed", model.Hash, model.AccountNumber, model.Timestamp);
                    _log.Error(msg);
                    model.ErrorMessage =
                        "Sorry, but the system was unable to save you trial request at this time.  Please contact Rittenhouse for assistance. (Save failed.)";
                    model.Error = true;
                }
                else
                {
                    var msg = GetErrorMessage("Invalid Hash", model.Hash, model.AccountNumber, model.Timestamp);
                    _log.Error(msg);
                    model.ErrorMessage =
                        "Sorry, but the system was unable to save you trial request at this time.  Please contact Rittenhouse for assistance. (Invalid Hash.)";
                    model.Error = true;
                }
            }
            else
            {
                var msg = GetErrorMessage(
                    $"Validation Failed - {string.Join("||", GetErrorListFromModelState(ModelState))}", model.Hash,
                    model.AccountNumber, model.Timestamp);
                _log.Error(msg);
                model.ErrorMessage =
                    "Sorry, but the system was unable to save you trial request at this time.  Please contact Rittenhouse for assistance. (Validation Failed.)";
                model.Error = true;
            }

            return View(model);
        }

        public static List<string> GetErrorListFromModelState(ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                from error in state.Errors
                select error.ErrorMessage;

            var errorList = query.ToList();
            return errorList;
        }

        public ActionResult AccountStatus(string accountNumber)
        {
            var accountStatus = new AccountStatus
            {
                AccountNumber = accountNumber,
                Status = "",
                Successful = false
            };

            try
            {
                var institution = _institutionService.GetInstitutionForEdit(accountNumber);
                if (institution != null)
                {
                    _log.DebugFormat("AccountStatusId: {0}, Id: {1}, Description: {2}", institution.AccountStatusId,
                        institution.AccountStatus.Id, institution.AccountStatus.Description);
                    accountStatus.Status = institution.AccountStatus.Description;
                    accountStatus.Successful = true;
                }
                else
                {
                    _log.Debug("institution was null, meaning the account number doesn't exist in R2.");
                    accountStatus.Status = "NoAccount";
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                accountStatus.Status = "Error";
                accountStatus.Successful = false;
            }

            _log.InfoFormat("accountNumber: {0}, status:", accountStatus.AccountNumber, accountStatus.Status);

            return Json(accountStatus, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RittenhouseAccount(string isbn)
        {
            return RedirectToAction("Index", "Home");
            if (CurrentUser == null)
            {
                return View(new AccountVerifyModel());
            }

            return RedirectToAction("Title", "Resource", new { isbn });
        }

        [HttpPost]
        public ActionResult RittenhouseAccount(AccountVerifyModel model)
        {
            return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                var rittenhouseCustomer = _trialService.ValidateRittenhouseAccount(model.UserName, model.Password);
                //TODO: Make sure if errormessage is null

                if (!string.IsNullOrWhiteSpace(rittenhouseCustomer.AccountNumber))
                {
                    var institution = _institutionService.GetInstitutionForEdit(rittenhouseCustomer.AccountNumber);

                    if (institution == null)
                    {
                        var newModel = new Trial();

                        var address = new Address
                        {
                            Address1 = rittenhouseCustomer.AddressLine1,
                            Address2 = rittenhouseCustomer.AddressLine2,
                            City = rittenhouseCustomer.City,
                            State = rittenhouseCustomer.State,
                            Zip = rittenhouseCustomer.ZipCode
                        };

                        newModel.Institution = new Areas.Admin.Models.Institution.Institution
                        {
                            Address = address,
                            AccountNumber = rittenhouseCustomer.AccountNumber,
                            InstitutionName = rittenhouseCustomer.AccountName,
                            Discount = 10,
                            TrialEndDate = DateTime.Now.AddMonths(1)
                        };

                        newModel.User = new UserEdit
                        {
                            Email = rittenhouseCustomer.EmailAddress,
                            UserName = rittenhouseCustomer.AccountNumber
                        };
                        newModel.AccountNumber = rittenhouseCustomer.AccountNumber;
                        newModel.FormPost = "CreateR2Trial";
                        return View("Index", newModel);
                    }

                    rittenhouseCustomer.Message =
                        "Account already exists in our system. Please contact Customer Service regarding your R2 Library account.";
                }

                model.ErrorMessage = rittenhouseCustomer.Message;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateR2Trial(Trial model)
        {
            return RedirectToAction("Index", "Home");
            var rittenhouseCustomer = _trialService.ValidateRittenhouseAccount(model.AccountNumber);

            if (AreAccountsEqual(model, rittenhouseCustomer))
            {
                var institution = new Institution
                {
                    AccountNumber = rittenhouseCustomer.AccountNumber,
                    AccountStatusId = (int)Core.Institution.AccountStatus.Trial,
                    Trial = new Core.Authentication.Trial
                        { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
                    Discount = 10,
                    HouseAccount = false,
                    Name = rittenhouseCustomer.AccountName,
                    Address = new Core.Authentication.Address
                    {
                        Address1 = rittenhouseCustomer.AddressLine1,
                        Address2 = rittenhouseCustomer.AddressLine2,
                        City = rittenhouseCustomer.City,
                        State = rittenhouseCustomer.State,
                        Zip = rittenhouseCustomer.ZipCode
                    },
                    AccessTypeId = (int)AccessType.IpValidationAnon,
                    AthensOrgId = null,
                    LogUrl = null,
                    HomePageId = (int)HomePage.Titles,
                    TrustedKey = null,
                    DisplayAllProducts = false
                };
                if (!string.IsNullOrWhiteSpace(rittenhouseCustomer.Territory))
                {
                    var territories = _territoryService.GetAllTerritories();
                    var territory = territories.FirstOrDefault(x => x.Code == rittenhouseCustomer.Territory);
                    if (territory != null)
                    {
                        institution.Territory = new Territory
                            { Code = territory.Code, Id = territory.Id, Name = territory.Name };
                    }
                }

                if (!string.IsNullOrWhiteSpace(rittenhouseCustomer.R2Type))
                {
                    var institutionTypes = _institutionTypes.ToList();
                    var institutionType = institutionTypes.FirstOrDefault(x => x.Name == rittenhouseCustomer.R2Type);
                    if (institutionType != null)
                    {
                        institution.Type = institutionType;
                    }
                }

                var institutionId = _trialsService.SaveInstitution(institution);

                institution.Id = institutionId;

                var user = new User
                {
                    FirstName = model.User.FirstName,
                    LastName = model.User.LastName,
                    Email = model.User.Email,
                    Department = null,
                    Role = new Role { Code = RoleCode.INSTADMIN, Id = (int)RoleCode.INSTADMIN },
                    UserName = model.AccountNumber,
                    Password = model.NewPassword,
                    Institution = institution,
                    InstitutionId = institution.Id
                };

                var userId = _trialsService.SaveUser(user);
                user.Id = userId;

                _trialsService.SaveUserOptionValues(user);

                SendWelcomeEmail(user.Email, institution.AccountNumber);
                return RedirectToAction("Index", "Home");
            }

            var msg = GetErrorMessage("Save Failed", model.Hash, model.AccountNumber, model.Timestamp);
            _log.Error(msg);
            model.ErrorMessage =
                "Sorry, but the system was unable to save you trial request at this time.  Please contact Rittenhouse for assistance. (Save failed.)";
            model.Error = true;
            return View("Index", model);
        }

        private bool AreAccountsEqual(Trial trialModel, RittenhouseCustomer rittenhouseCustomer)
        {
            if (!string.Equals(trialModel.Institution.Address.Address1, rittenhouseCustomer.AddressLine1,
                    StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(trialModel.Institution.Address.Address2, rittenhouseCustomer.AddressLine2,
                    StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(trialModel.Institution.Address.City, rittenhouseCustomer.City,
                    StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(trialModel.Institution.Address.State, rittenhouseCustomer.State,
                    StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(trialModel.Institution.Address.Zip, rittenhouseCustomer.ZipCode,
                    StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(trialModel.Institution.AccountNumber, rittenhouseCustomer.AccountNumber,
                    StringComparison.OrdinalIgnoreCase)) return false;
            if (trialModel.Institution.TrialEndDate != null &&
                trialModel.Institution.TrialEndDate.Value.Date != DateTime.Now.AddMonths(1).Date) return false;
            if (!string.Equals(trialModel.User.UserName, rittenhouseCustomer.AccountNumber,
                    StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }


        public ActionResult RequestAccount()
        {
            return RedirectToAction("Index", "Home");
            return View(new RequestAccountModel());
        }

        [HttpPost]
        public ActionResult RequestAccount(RequestAccountModel model)
        {
            return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                var messageBody = _accountRequestEmailBuildService.GetMessageBody(model, Url);

                var emailPage = new EmailPage
                {
                    To = _adminSettings.TrialInitializeEmail,
                    Subject = "R2 Library Trial Request Form"
                };

                _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                return View("RequestFormConfirmation", model);
            }
            else
            {
                model.CurrentSection = "5";
                return View(model);
            }
        }
        //


        private bool IsValidHash(string hash, string accountNumber, string timestamp)
        {
            _log.DebugFormat("timestamp: {0}", timestamp);
            var dateTime = ParseTimestamp(timestamp);
            _log.DebugFormat("dateTime: {0}", dateTime);

            // hash is valid for 24 hours - the window can be long since the system will prevent multiple
            // trial accounts for a single customer.
            // The hash is relatively basic because of this fact
            var now = DateTime.Now;
            if (dateTime < now.AddDays(-1) || dateTime > now.AddDays(+1))
            {
                var msg = GetErrorMessage($"Invalid timestamp: {timestamp} => {dateTime}", hash, accountNumber,
                    timestamp);
                _log.Error(msg);
                return false;
            }

            var textToHash = $"[{accountNumber}]~R3d~'{dateTime:yyyy-MM-dd HH:mm:ss}'";
            _log.DebugFormat("textToHash: {0}", textToHash);
            var calculatedHash = CalculateHash(textToHash);
            _log.DebugFormat("calculatedHash: {0}", calculatedHash);
            _log.DebugFormat("hash: {0}", hash);

            var isValidHash = hash == calculatedHash;
            _log.DebugFormat("isValidHash: {0}", isValidHash);
            return isValidHash;
        }

        private DateTime ParseTimestamp(string timestamp)
        {
            try
            {
                var year = int.Parse(timestamp.Substring(0, 4));
                var month = int.Parse(timestamp.Substring(4, 2));
                var day = int.Parse(timestamp.Substring(6, 2));
                var hours = int.Parse(timestamp.Substring(8, 2));
                var minutes = int.Parse(timestamp.Substring(10, 2));
                var seconds = int.Parse(timestamp.Substring(12, 2));
                return new DateTime(year, month, day, hours, minutes, seconds);
            }
            catch (Exception ex)
            {
                _log.InfoFormat("timestamp: {0}", timestamp);
                _log.Error(ex.Message, ex);
                return new DateTime(2000, 1, 1); // return an invalid date on exception
            }
        }

        private string CalculateHash(string text)
        {
            var bytes = Encoding.Unicode.GetBytes(text);

            using (var hashAlgorithm = HashAlgorithm.Create("SHA1"))
            {
                if (hashAlgorithm != null)
                {
                    var inArray = hashAlgorithm.ComputeHash(bytes);
                    return Convert.ToBase64String(inArray);
                }
            }

            _log.ErrorFormat("HASH algorithm was null! - URL: {0}", Request.RawUrl);
            return null;
        }

        private bool SaveInstitutionForTrial(Trial model)
        {
            var preludeCustomer = _preludeCustomers.FirstOrDefault(x => x.AccountNumber == model.AccountNumber);

            if (preludeCustomer == null)
            {
                _log.Error(
                    $"Account Number Not Found Attempting to Create Trial, Account Number: {model.AccountNumber}");
                return false;
            }

            var doesInstitutionExist = _institutionService.DoesInstitutionExists(model.AccountNumber);

            if (doesInstitutionExist)
            {
                _log.Error($"Can't Create Trial, Account Already Existing, Account Number: {model.AccountNumber}");
                return false;
            }

            IInstitution institution = GetCoreInstitution(preludeCustomer);
            var institutionId = _trialsService.SaveInstitution(institution);
            institution.Id = institutionId;

            var user = GetCoreUser(model, institution);
            var userId = _trialsService.SaveUser(user);
            user.Id = userId;

            _trialsService.SaveUserOptionValues(user);

            return SendWelcomeEmail(user.Email, institution.AccountNumber);
        }

        public Institution GetCoreInstitution(PreludeCustomer preludeCustomer)
        {
            var institution = new Institution
            {
                AccountNumber = preludeCustomer.AccountNumber,
                AccountStatusId = (int)Core.Institution.AccountStatus.Trial,
                Trial = new Core.Authentication.Trial { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
                Discount = 10,
                HouseAccount = false,
                Name = preludeCustomer.Name,
                Address = new Core.Authentication.Address
                {
                    Address1 = preludeCustomer.Address.Address1,
                    Address2 = preludeCustomer.Address.Address2,
                    City = preludeCustomer.Address.City,
                    State = preludeCustomer.Address.State,
                    Zip = preludeCustomer.Address.Zip
                },
                AccessTypeId = (int)AccessType.IpValidationAnon,
                AthensOrgId = null,
                LogUrl = null,
                HomePageId = (int)HomePage.Titles,
                TrustedKey = null,
                DisplayAllProducts = false
            };

            var preludeInstitution =
                _preludeCustomers.FirstOrDefault(x => x.AccountNumber == institution.AccountNumber);
            if (preludeInstitution != null)
            {
                var territories = _territoryService.GetAllTerritories();
                var territory = territories.FirstOrDefault(x => x.Code == preludeInstitution.Territory);
                if (territory != null)
                {
                    institution.Territory = new Territory
                        { Code = territory.Code, Id = territory.Id, Name = territory.Name };
                }
            }

            return institution;
        }

        public static User GetCoreUser(Trial model, IInstitution institution)
        {
            var user = new User
            {
                FirstName = model.User.FirstName,
                LastName = model.User.LastName,
                Email = model.User.Email,
                Department = null,
                Role = new Role { Code = RoleCode.INSTADMIN, Id = (int)RoleCode.INSTADMIN },
                UserName = model.AccountNumber,
                Password = model.NewPassword,
                Institution = (Institution)institution,
                InstitutionId = institution.Id
            };
            return user;
        }

        private bool SendWelcomeEmail(string emailAddress, string accountNumber)
        {
            var emailPage = new EmailPage
            {
                To = emailAddress,
                Subject = "Welcome to R2 Library",
                Cc = _adminSettings.TrialInitializeEmail
            };

            var model = new WelcomeEmail { AccountNumber = accountNumber };

            var messageBody = RenderPartialViewToString("_WelcomeEmail", model);
            return _emailService.SendEmailMessageToQueue(messageBody, emailPage);
        }

        private string GetErrorMessage(string errorMessage, string hash, string accountNumber, string timestamp)
        {
            var msg = new StringBuilder("Trial Error: ").AppendLine()
                .AppendLine(errorMessage)
                .AppendFormat("hash: {0}", hash).AppendLine()
                .AppendFormat("accountNumber: {0}", accountNumber).AppendLine()
                .AppendFormat("timestamp: {0}", timestamp).AppendLine()
                .AppendFormat("RawUrl: {0}", Request.RawUrl).AppendLine();
            return msg.ToString();
        }
    }
}