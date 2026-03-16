#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Order;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class MarcExportController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IAdminSettings _adminSettings;
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IOrderService _orderService;
        private readonly IQueryable<IResource> _resources;

        public MarcExportController(IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IQueryable<IResource> resources
            , IOrderService orderService
            , IOrderHistoryService orderHistoryService
            , IAdminSettings adminSettings)
            : base(authenticationContext)
        {
            _adminContext = adminContext;
            _resources = resources;
            _orderService = orderService;
            _orderHistoryService = orderHistoryService;
            _adminSettings = adminSettings;
        }

        //
        // GET: /Admin/MarcExport/
        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC, RoleCode.INSTADMIN })]
        public ActionResult OrderHistory(int institutionId, int id)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var orderHistory = _orderHistoryService.GetOrderHistoryDetail(institutionId, id);
            if (orderHistory != null)
            {
                if (orderHistory.OrderHistory != null)
                {
                    var isbnList = (from item in orderHistory.OrderHistory.OrderHistoryResources
                            where item.NumberOfLicenses > 0
                            select string.IsNullOrWhiteSpace(item.Resource.Isbn13)
                                ? item.Resource.Isbn10
                                : item.Resource.Isbn13)
                        .ToList();

                    var jsonMarcRecordRequest = GetJsonMarcRecordRequest(institution, isbnList);

                    return ReturnFileView(jsonMarcRecordRequest);
                }
            }

            return Content("Sorry there was a problem getting your Marc Records");
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC, RoleCode.INSTADMIN })]
        public ActionResult ShoppingCart(int institutionId, int id)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var order = _orderService.GetOrder(id, adminInstitution);
            if (order != null)
            {
                var isbnList =
                    order.Items.OfType<ResourceOrderItem>()
                        .Where(x => x.NumberOfLicenses > 0)
                        .Select(resource =>
                            string.IsNullOrWhiteSpace(resource.Resource.Isbn13)
                                ? resource.Resource.Isbn10
                                : resource.Resource.Isbn13)
                        .ToList();


                var jsonMarcRecordRequest = GetJsonMarcRecordRequest(order.Institution, isbnList);
                return ReturnFileView(jsonMarcRecordRequest);
            }

            return Content("Sorry there was a problem getting your Marc Records");
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC, RoleCode.INSTADMIN })]
        public ActionResult Resource(int institutionId, string isbn)
        {
            if (institutionId == 0)
            {
                institutionId = AuthenticatedInstitution != null ? AuthenticatedInstitution.Id : 0;
            }

            var institution = _adminContext.GetAdminInstitution(institutionId);
            var resource = _resources.FirstOrDefault(x => x.Isbn10 == isbn || x.Isbn13 == isbn || x.EIsbn == isbn);
            if (resource != null)
            {
                var isbnList = new List<string>
                    { string.IsNullOrWhiteSpace(resource.Isbn13) ? resource.Isbn10 : resource.Isbn13 };
                var jsonMarcRecordRequest = GetJsonMarcRecordRequest(institution, isbnList);

                return ReturnFileView(jsonMarcRecordRequest);
            }

            return Content("Sorry there was a problem getting your Marc Record");
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC, RoleCode.INSTADMIN })]
        public ActionResult CollectionManagement(CollectionManagementQuery collectionManagementQuery)
        {
            var institutionResources = _orderService.GetInstitutionResources(collectionManagementQuery, CurrentUser);

            if (!string.IsNullOrWhiteSpace(collectionManagementQuery.Resources))
            {
                var resourceIds = collectionManagementQuery.GetResourceIds();
                institutionResources.InstitutionResourcesList = resourceIds.Select(resourceId =>
                    _orderService.GetInstitutionResource(collectionManagementQuery.InstitutionId, resourceId,
                        collectionManagementQuery.CartId)).ToList();
            }

            return View(institutionResources);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC, RoleCode.INSTADMIN })]
        public ActionResult CollectionManagementList(CollectionManagementQuery collectionManagementQuery,
            bool isDeleteMarcRecord = false)
        {
            var institutionResources = _orderService.GetInstitutionResources(collectionManagementQuery, CurrentUser);

            if (!string.IsNullOrWhiteSpace(collectionManagementQuery.Resources))
            {
                var resourceIds = collectionManagementQuery.GetResourceIds();
                institutionResources.InstitutionResourcesList = resourceIds.Select(resourceId =>
                    _orderService.GetInstitutionResource(collectionManagementQuery.InstitutionId, resourceId,
                        collectionManagementQuery.CartId)).ToList();
            }

            if (institutionResources != null)
            {
                var isbnList = institutionResources.InstitutionResourcesList.Select(institutionResource =>
                    string.IsNullOrWhiteSpace(institutionResource.Isbn13)
                        ? institutionResource.Isbn10
                        : institutionResource.Isbn13).ToList();

                var jsonMarcRecordRequest =
                    GetJsonMarcRecordRequest(institutionResources.Institution, isbnList, isDeleteMarcRecord);

                return ReturnFileView(jsonMarcRecordRequest);
            }

            return Content("Sorry there was a problem getting your Marc Records");
        }

        private ActionResult ReturnFileView(JsonMarcRecordRequest jsonMarcRecordRequest)
        {
            var javaScriptSerializer = new JavaScriptSerializer();
            var jsonResults = javaScriptSerializer.Serialize(jsonMarcRecordRequest);

            var marcRecordExport = new MarcRecordExport
            {
                MarcRecordRequestString = jsonResults,
                Url = _adminSettings.MarcRecordWebsite
            };

            return View("MarcExport", marcRecordExport);
        }


        private static JsonMarcRecordRequest GetJsonMarcRecordRequest(IAdminInstitution institution,
            IEnumerable<string> isbnList, bool isDeleteMarcRecord = false)
        {
            var jsonMarcRecordRequest = new JsonMarcRecordRequest
            {
                AccountNumber = institution.AccountNumber,
                Format = "mrc",
                IsbnAndCustomerFields = new List<JsonIsbnAndCustomerFields>(),
                CustomMarcFields = new List<JsonCustomMarcField>
                {
                    new JsonCustomMarcField
                    {
                        FieldNumber = 856, FieldValue = institution.ProxyPrefix
                    },
                    new JsonCustomMarcField
                    {
                        FieldNumber = 8566, FieldValue = institution.UrlSuffix
                    }
                }
            };

            foreach (var isbn in isbnList)
            {
                jsonMarcRecordRequest.IsbnAndCustomerFields.Add(new JsonIsbnAndCustomerFields
                {
                    IsbnOrSku = isbn
                });
            }

            if (isDeleteMarcRecord)
            {
                jsonMarcRecordRequest.IsDeleteFile = true;
            }

            return jsonMarcRecordRequest;
        }
    }
}