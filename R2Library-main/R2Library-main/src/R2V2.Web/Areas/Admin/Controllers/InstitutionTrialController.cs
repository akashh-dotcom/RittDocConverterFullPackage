#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Institution;
using R2V2.Web.Areas.Admin.Models.TrialInstitution;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using Address = R2V2.Web.Areas.Admin.Models.Institution.Address;
using User = R2V2.Core.Authentication.User;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class InstitutionTrialController : R2AdminBaseController
    {
        private readonly IAdminSettings _adminSettings;

        private readonly EmailSiteService _emailService;

        //private readonly IQueryable<PreludeCustomer> _preludeCustomers;
        private readonly InstitutionService _institutionService;

        private readonly IQueryable<InstitutionType> _institutionTypes;
        private readonly ITerritoryService _territoryService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserOptionService _userOptionService;

        private readonly UserService _userService;
        //
        // GET: /Admin/InstitutionTrial/

        public InstitutionTrialController(
            IAuthenticationContext authenticationContext
            //, IQueryable<PreludeCustomer> preludeCustomers
            , InstitutionService institutionService
            , UserService userService
            , EmailSiteService emailService
            , IAdminSettings adminSettings
            , ITerritoryService territoryService
            , IUnitOfWorkProvider unitOfWorkProvider
            , UserOptionService userOptionService
            , IQueryable<InstitutionType> institutionTypes
        )
            : base(authenticationContext)
        {
            //_preludeCustomers = preludeCustomers;
            _institutionService = institutionService;
            _userService = userService;
            _emailService = emailService;
            _adminSettings = adminSettings;
            _territoryService = territoryService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _userOptionService = userOptionService;
            _institutionTypes = institutionTypes;
        }

        public ActionResult AddTrial()
        {
            var model = new NewTrial();
            if (_adminSettings.AdminControllAccess.Any(x =>
                    string.Equals(x, CurrentUser.Email, StringComparison.CurrentCultureIgnoreCase)))
            {
                model.DisplayTestButton = true;
            }

            return View(model);
        }

        //TestAccountNumber
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "CheckAccountNumber")]
        public ActionResult AddTrial(NewTrial newTrial)
        {
            if (ModelState.IsValid)
            {
                var existingInstitition = _institutionService.GetInstitutionForEdit(newTrial.AccountNumber);
                
                if (existingInstitition == null)
                {
                    return RedirectToAction("ProcessTrial", new { accountNumber = newTrial.AccountNumber });
                }

                ModelState.AddModelError("AccountNumber", Resources.AccountAlreadyExists);
            }

            if (_adminSettings.AdminControllAccess.Any(x =>
                    string.Equals(x, CurrentUser.Email, StringComparison.CurrentCultureIgnoreCase)))
            {
                newTrial.DisplayTestButton = true;
            }

            return View("AddTrial", newTrial);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "TestAccountNumber")]
        public ActionResult TestAccountNumber(NewTrial newTrial)
        {
            if (string.IsNullOrWhiteSpace(newTrial.AccountNumber))
            {
                ModelState.AddModelError("AccountNumber", $@"Account Number must be populated");
            }
            else
            {
                var existingInstitition = _institutionService.GetInstitutionForEdit(newTrial.AccountNumber);
                if (existingInstitition != null)
                {
                    ModelState.AddModelError("AccountNumber",
                        $@"Account Number already exists: {newTrial.AccountNumber}");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("AddTrial", newTrial);
            }

            var newTrialInstitution = new TrialInstitution();
            
            var territories = _territoryService.GetAllTerritories();
            var institutionTypes = _institutionTypes.ToList();

            var institutionTrial = new InstitutionEditViewModel(territories, institutionTypes)
            {
                Address = new Address(),
                Discount = 0,
                TrialEndDate = DateTime.Now.AddMonths(1),
                AccountNumber = newTrial.AccountNumber
            };

            var administratorUser = new UserEdit { UserName = newTrial.AccountNumber };

            newTrialInstitution.InstitutionTrial = institutionTrial;
            newTrialInstitution.User = administratorUser;
            newTrialInstitution.PopulateSelectLists(_userService.GetListDepartments());


            return View("ProcessTrial", newTrialInstitution);
        }

        public ActionResult ProcessTrial(string accountNumber)
        {
            var newTrialInstitution = new TrialInstitution();
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                var territories = _territoryService.GetAllTerritories();
                var institutionTypes = _institutionTypes.ToList();
                var institutionTrial = new InstitutionEditViewModel(territories, institutionTypes);
                institutionTrial.Address = new Address();
                institutionTrial.AccountNumber = accountNumber;
                institutionTrial.TrialEndDate = DateTime.Now.AddMonths(1);

                newTrialInstitution.User = new UserEdit { UserName = accountNumber };

                newTrialInstitution.InstitutionTrial = institutionTrial;
                newTrialInstitution.PopulateSelectLists(_userService.GetListDepartments());
            }

            return View(newTrialInstitution);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ProcessTrial")]
        public ActionResult ProcessTrial(TrialInstitution trialInstitution)
        {
            if (string.IsNullOrWhiteSpace(trialInstitution.User.NewPassword))
            {
                ModelState.AddModelError("User.NewPassword", Resources.PleaseFillInPassword);
            }

            if (string.IsNullOrWhiteSpace(trialInstitution.User.ConfirmPassword))
            {
                ModelState.AddModelError("User.ConfirmPassword", Resources.PleaseFillInPassword);
            }

            if (trialInstitution.User.NewPassword != trialInstitution.User.ConfirmPassword)
            {
                ModelState.AddModelError("User.NewPassword", Resources.PasswordsDoNotMatch);
            }

            if (ModelState.IsValid)
            {
                User user = null;
                var userSaved = false;
                IInstitution institution;

                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        trialInstitution.User.Password = trialInstitution.User.NewPassword;
                        institution = trialInstitution.InstitutionTrial.ToCoreInstitutionFromTrialInstitution();


                        if (institution != null)
                        {
                            uow.Save(institution);

                            if (trialInstitution.User.Department.Id == 0)
                            {
                                if (string.IsNullOrWhiteSpace(trialInstitution.User.Department.Name) &&
                                    string.IsNullOrWhiteSpace(trialInstitution.User.CustomDepartment))
                                {
                                    trialInstitution.User.Department = null;
                                }
                                else
                                {
                                    trialInstitution.User.Department.Name = trialInstitution.User.CustomDepartment;
                                    trialInstitution.User.Department.Id = trialInstitution.User.CustomDepartmentId;
                                    _userService.CreateCustomDepartment(trialInstitution.User.Department);
                                }
                            }

                            user = trialInstitution.User.ToCoreUserFromTrialInstitution(institution);

                            if (user != null)
                            {
                                uow.Save(user);
                                uow.Commit();
                                transaction.Commit();

                                userSaved = true;
                            }
                        }

                        if (!userSaved)
                        {
                            transaction.Rollback();
                        }
                    }
                }

                if (userSaved)
                {
                    _userOptionService.InsertNewUserOptionValues(user.Id, user.Role.Id);
                    SendWelcomeEmail(trialInstitution);
                    return RedirectToAction("Detail", "Institution",
                        new { institutionId = institution.Id, reload = true });
                }
            }

            trialInstitution.PopulateSelectLists(_userService.GetListDepartments());
            return View(trialInstitution);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "TestProcessTrial")]
        public ActionResult TestProcessTrial(TrialInstitution trialInstitution)
        {
            trialInstitution.IsTestMode = true;
            if (string.IsNullOrWhiteSpace(trialInstitution.User.NewPassword))
            {
                ModelState.AddModelError("User.NewPassword", Resources.PleaseFillInPassword);
            }

            if (string.IsNullOrWhiteSpace(trialInstitution.User.ConfirmPassword))
            {
                ModelState.AddModelError("User.ConfirmPassword", Resources.PleaseFillInPassword);
            }

            if (trialInstitution.User.NewPassword != trialInstitution.User.ConfirmPassword)
            {
                ModelState.AddModelError("User.NewPassword", Resources.PasswordsDoNotMatch);
            }

            var testSaveWorked = false;
            if (ModelState.IsValid)
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        trialInstitution.User.Password = trialInstitution.User.NewPassword;
                        var institution = trialInstitution.InstitutionTrial.ToCoreInstitutionFromTrialInstitution();


                        if (institution != null)
                        {
                            uow.Save(institution);

                            if (trialInstitution.User.Department.Id == 0)
                            {
                                if (string.IsNullOrWhiteSpace(trialInstitution.User.Department.Name) &&
                                    string.IsNullOrWhiteSpace(trialInstitution.User.CustomDepartment))
                                {
                                    trialInstitution.User.Department = null;
                                }
                                else
                                {
                                    trialInstitution.User.Department.Name = trialInstitution.User.CustomDepartment;
                                    trialInstitution.User.Department.Id = trialInstitution.User.CustomDepartmentId;
                                    _userService.CreateCustomDepartment(trialInstitution.User.Department);
                                }
                            }

                            var user = trialInstitution.User.ToCoreUserFromTrialInstitution(institution);

                            if (user != null)
                            {
                                uow.Save(user);
                                testSaveWorked = true;
                            }
                        }

                        transaction.Rollback();
                    }
                }
            }

            if (testSaveWorked)
            {
                ModelState.AddModelError("User.NewPassword",
                    @"The save worked, but rolled back because this is only a test.");
            }

            trialInstitution.PopulateSelectLists(_userService.GetListDepartments());
            return View("ProcessTrial", trialInstitution);
        }

        private void SendWelcomeEmail(TrialInstitution trialInstitution)
        {
            var emailPage = new EmailPage
            {
                To = trialInstitution.User.Email,
                Subject = "Welcome to R2 Library",
                Cc = _adminSettings.TrialInitializeEmail
            };

            var messageBody =
                RenderRazorViewToString("InstitutionTrial", "_Welcome", trialInstitution.InstitutionTrial);
            _emailService.SendEmailMessageToQueue(messageBody, emailPage);
        }
    }
}