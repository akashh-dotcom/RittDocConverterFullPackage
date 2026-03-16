#region

using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Order;
using R2V2.Web.Areas.Admin.Models.PdaRules;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Services;
using UserService = R2V2.Core.UserService;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC })]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class CheckoutController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IAdminSettings _adminSettings;
        private readonly EmailSiteService _emailService;
        private readonly IEmailSettings _emailSettings;
        private readonly GoogleService _googleService;
        private readonly InstitutionService _institutionService;
        private readonly ILog<CheckoutController> _log;
        private readonly WebOrderHistoryService _orderHistoryService;
        private readonly IOrderService _orderService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserService _userService;

        private bool _sendNewAccountEmail;

        public CheckoutController(ILog<CheckoutController> log
            , IAuthenticationContext authenticationContext
            , IUnitOfWorkProvider unitOfWorkProvider
            , IAdminContext adminContext
            , InstitutionService institutionService
            , IOrderService orderService
            , WebOrderHistoryService orderHistoryService
            , EmailSiteService emailService
            , IAdminSettings adminSettings
            , IEmailSettings emailSettings
            , UserService userService
            , GoogleService googleService
        )
            : base(authenticationContext)
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _adminContext = adminContext;
            _institutionService = institutionService;
            _orderService = orderService;
            _orderHistoryService = orderHistoryService;
            _emailService = emailService;
            _googleService = googleService;
            _adminSettings = adminSettings;
            _emailSettings = emailSettings;
            _userService = userService;
        }

        [NoCache]
        [HttpGet]
        public ActionResult Checkout(CollectionManagementQuery collectionManagementQuery)
        {
            var order = _orderService.GetOrderForInstitution(collectionManagementQuery.InstitutionId);
            order.CollectionManagementQuery = collectionManagementQuery;
            var excelUrl = Url.Action("ExportShoppingCart", "Cart", new { institutionId = order.InstitutionId });
            var marcUrl = Url.Action("ShoppingCart", "MarcExport",
                new { institutionId = order.InstitutionId, id = order.OrderId });
            order.ToolLinks = GetToolLinks(true, excelUrl, marcUrl);
            order.Editable = false;

            return View(order);
        }

        [NoCache]
        [HttpPost]
        public ActionResult Checkout(Order order)
        {
            if (ModelState.IsValid)
            {
                var institutionId = order.InstitutionId;

                var institution = _adminContext.GetAdminInstitution(institutionId);
                if (!institution.IsEulaSigned)
                {
                    return View("Eula", order);
                }

                var dbOrder = _orderService.GetOrderForInstitution(institutionId);

                if (dbOrder.ItemCount == 0)
                {
                    _log.Error($"No Items in Order, but they made it to Checkout POST. Investigate order {dbOrder.Id}");
                    return RedirectToAction("ShoppingCart", "Cart", new { institutionId = order.InstitutionId });
                }

                // Backend only: Set PO Number to "Not Applicable" if empty
                dbOrder.PurchaseOrderNumber = string.IsNullOrWhiteSpace(order.PurchaseOrderNumber) 
                    ? "Not Applicable" 
                    : order.PurchaseOrderNumber;
                dbOrder.BillingMethod = order.BillingMethod;
                dbOrder.ForthcomingTitlesInvoicingMethod = order.ForthcomingTitlesInvoicingMethod;
                dbOrder.PurchaseOrderComment = order.PurchaseOrderComment;

                if (_orderService.PlaceOrder(dbOrder, CurrentUser, _sendNewAccountEmail))
                {
                    if (_sendNewAccountEmail)
                    {
                        SendOrderConfirmation(dbOrder, true);
                    }

                    SendOrderConfirmation(dbOrder);

                    _googleService.LogCheckoutStep(dbOrder.PurchasableItems.ToList(), 2);

                    return RedirectToAction("OrderConfirmation", new { id = institutionId, dbOrder.OrderId });
                }
            }

            var model = _orderService.GetOrderForInstitution(order.InstitutionId);
            model.ToolLinks = GetToolLinks(true,
                @Url.Action("ExportShoppingCart", "Cart", new { institutionId = order.InstitutionId }),
                @Url.Action("ShoppingCart", "MarcExport",
                    new { institutionId = order.InstitutionId, id = order.OrderId }));
            model.OrderError = true;
            model.CollectionManagementQuery = new CollectionManagementQuery();
            return View(model);
        }

        private void SendOrderConfirmation(Order order, bool newAccountNotification = false)
        {
            var institutionId = order.InstitutionId;
            var adminUser = _userService.GetInstitutionAdministrator(institutionId);
            var currentUser = CurrentUser;

            var emailPage = new EmailPage
            {
                To = currentUser.Email,
                Bcc = _adminSettings.PurchaseConfirmationEmail,
                Subject = "R2 Library Purchase Confirmation – Thank You"
            };

            if (newAccountNotification)
            {
                if (!_emailSettings.SendToCustomers)
                {
                    emailPage.To = _emailSettings.TestEmailAddresses;
                    _log.WarnFormat("TO address overwritten to '{0}' was '{1}'", _emailSettings.TestEmailAddresses,
                        _adminSettings.NewAccountNotificationEmail);
                }
                else
                {
                    emailPage.To = _adminSettings.NewAccountNotificationEmail;
                }

                emailPage.Cc = null;
                emailPage.Bcc = null;
                emailPage.Subject = "R2 Library New account notification";
            }

            if (!_emailSettings.SendToCustomers)
            {
                _log.WarnFormat("CC address overwritten to '{0}' was '{1}'", _emailSettings.TestEmailAddresses,
                    emailPage.Cc);
                emailPage.Cc = _emailSettings.TestEmailAddresses;
                if (adminUser.Email != currentUser.Email)
                {
                    _log.WarnFormat("CC address would have been set to '{0}'", adminUser.Email);
                }
            }
            else if (adminUser.Email != currentUser.Email)
            {
                emailPage.Cc = adminUser.Email;
            }

            var messageBody = RenderRazorViewToString("OrderHistory", "_Detail",
                _orderHistoryService.GetOrderHistoryDetail(institutionId, order.OrderHistoryId));

            _emailService.SendEmailMessageToQueue(messageBody, emailPage);
        }

        [NoCache]
        public ActionResult OrderConfirmation(Order order)
        {
            _adminContext.ReloadAdminInstitution(order.InstitutionId,
                AuthenticatedInstitution.User.Id); // need to reload after successful checkout
            var orderHistoryDetail = _orderHistoryService.GetOrderHistorySummary(order.InstitutionId, order.OrderId);
            _googleService.LogCheckoutStep(orderHistoryDetail.OrderHistory, 3);
            _googleService.LogCartPurchase(orderHistoryDetail.OrderHistory);

            return View(orderHistoryDetail);
        }

        [NoCache]
        public ActionResult EulaModal(CollectionManagementQuery collectionManagementQuery)
        {
            collectionManagementQuery.EulaSigned = true;

            //PdaRuleModel
            var pdaRuleModel = TempData.GetItem<PdaRuleModel>("PdaRule");
            if (pdaRuleModel != null)
            {
                return RedirectToAction("SaveRule", "Pda",
                    new
                    {
                        collectionManagementQuery.InstitutionId, collectionManagementQuery.TrialConvert,
                        collectionManagementQuery.EulaSigned
                    });
            }

            if (!string.IsNullOrWhiteSpace(collectionManagementQuery.Resources))
            {
                //Needed because collectionManagementQuery can become large and cause a 404.15 error
                TempData.AddItem("CollectionManagementQuery", collectionManagementQuery);
                return RedirectToAction("BulkAddPda", "Pda");
            }

            return RedirectToAction("PdaAdd", "Pda", collectionManagementQuery);
        }

        [HttpPost]
        [NoCache]
        public ActionResult Eula(Order order)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var institutionId = order.InstitutionId;

                var institution = _institutionService.GetInstitutionForEdit(institutionId);
                institution.EULASigned = true;

                uow.SaveOrUpdate(institution);

                uow.Commit();

                _adminContext.ReloadAdminInstitution(institutionId,
                    AuthenticatedInstitution.User.Id); // reload cached institution after update

                _sendNewAccountEmail = true;

                return Checkout(order);
            }
        }

        [NoCache]
        public ActionResult PdaEulaModal(CollectionManagementQuery collectionManagementQuery)
        {
            collectionManagementQuery.PdaEulaSigned = true;

            if (!string.IsNullOrWhiteSpace(collectionManagementQuery.Resources))
            {
                TempData.AddItem("CollectionManagementQuery", collectionManagementQuery);
                return RedirectToAction("BulkAddPda", "Pda");
            }

            return RedirectToAction("PdaAdd", "Pda", collectionManagementQuery);
        }
    }
}