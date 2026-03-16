#region

using System;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    public class OrderHistoryController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly EmailSiteService _emailService;
        private readonly IOrderHistoryService _orderHistoryService;

        public OrderHistoryController(IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IOrderHistoryService orderHistoryService
            , EmailSiteService emailService
        )
            : base(authenticationContext)
        {
            _adminContext = adminContext;
            _orderHistoryService = orderHistoryService;
            _emailService = emailService;
        }

        public ActionResult List(int institutionId)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                institutionId != CurrentUser.InstitutionId)
            {
                institutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("List", new { institutionId });
            }

            var orderHistoryList = _orderHistoryService.GetOrderHistoryList(institutionId);

            orderHistoryList.ToolLinks = GetToolLinks(true,
                @Url.Action("ExportList", new { institutionId = orderHistoryList.InstitutionId }));

            return View(orderHistoryList);
        }

        [HttpPost]
        public ActionResult List(int institutionId, EmailPage emailPage)
        {
            if (emailPage.To == null)
            {
                return RedirectToAction("List", new { institutionId }); //return List(institutionId);
            }

            var orderHistoryList = _orderHistoryService.GetOrderHistoryList(institutionId);

            var messageBody = RenderRazorViewToString("OrderHistory", "_List", orderHistoryList);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportList(int institutionId)
        {
            var excelExport = _orderHistoryService.GetOrderHistoryListExcelExport(institutionId);
            var fileDownloadName =
                $"R2-OrderHistoryList-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        public ActionResult Detail(int institutionId, int id)
        {
            var model = _orderHistoryService.GetOrderHistoryDetail(institutionId, id);
            if (model != null && model.OrderHistory != null && model.OrderHistory.OrderHistoryId > 0)
            {
                model.ToolLinks = GetToolLinks(true,
                    @Url.Action("Export",
                        new { id = model.OrderHistory.OrderHistoryId, institutionId = model.InstitutionId }),
                    @Url.Action("OrderHistory", "MarcExport",
                        new { id = model.OrderHistory.OrderHistoryId, model.InstitutionId }));
            }
            else
            {
                return RedirectToAction("List", new { institutionId });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Detail(int institutionId, int id, EmailPage emailPage)
        {
            if (emailPage.To == null)
            {
                return Detail(institutionId, id);
            }

            var orderHistoryDetail = _orderHistoryService.GetOrderHistoryDetail(institutionId, id);

            var messageBody = RenderRazorViewToString("OrderHistory", "_Detail", orderHistoryDetail);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Export(int institutionId, int id)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var excelExport = _orderHistoryService.GetOrderHistoryDetailExcelExport(id,
                Url.Action("Title", "Resource", new { Area = "" },
                    HttpContext.Request.IsSecureConnection ? "https" : "http"), adminInstitution);
            var fileDownloadName = $"R2-OrderHistory-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }
    }
}