#region

using System;
using System.IO;
using System.Web.Http;
using System.Xml.Serialization;
using R2V2.Infrastructure.DependencyInjection;
using Newtonsoft.Json;
using R2V2.Core.Api;
using R2V2.Core.Api.YBP;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Controllers
{
    public class YbpController : BaseApiController
    {
        private readonly ICollectionManagementSettings _collectionManagementSettings =
            ServiceLocator.Current.GetInstance<ICollectionManagementSettings>();

        private readonly EmailSiteService _emailSiteService = ServiceLocator.Current.GetInstance<EmailSiteService>();

        private readonly ILog<YbpController> _log = ServiceLocator.Current.GetInstance<ILog<YbpController>>();
        private readonly RapidOrderService _ybpService = ServiceLocator.Current.GetInstance<RapidOrderService>();

        //Cannot use DI without updating Auto fac across all projects. 

        [HttpGet]
        public IHttpActionResult GetData([FromUri] string key)
        {
            if (key != _collectionManagementSettings.YbpSecretKey)
            {
                _log.Warn("Unauthorized access attempt");
                return Unauthorized();
            }

            var data = new ApiTest
            {
                Success = true,
                Message = "YBP API is working",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            return SendXmlResponse<ApiTest>(data);
        }

        [HttpPost]
        public IHttpActionResult PostData([FromUri] string key)
        {
            if (key != _collectionManagementSettings.YbpSecretKey)
            {
                _log.Warn("Unauthorized access attempt");
                return Unauthorized();
            }

            // Read the raw request body
            string requestBody;
            try
            {
                requestBody = Request.Content.ReadAsStringAsync().Result;
                _log.Info($"Received request body: {requestBody}");
            }
            catch (Exception ex)
            {
                var m = $"Failed to read request body: {ex.Message}";
                _log.Error(m);
                return SendXmlResponse<ApiError>(m);
            }

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var m = "Request body is empty.";
                _log.Warn(m);
                return SendXmlResponse<ApiError>(m);
            }

            // Normalize XML: Replace VendorID with VendorId
            requestBody = requestBody.Replace("VendorID", "VendorId");

            // Deserialize the normalized XML
            PurchaseOrder order;
            try
            {
                using (var stringReader = new StringReader(requestBody))
                {
                    var serializer = new XmlSerializer(typeof(PurchaseOrder));
                    order = serializer.Deserialize(stringReader) as PurchaseOrder;
                }
            }
            catch (Exception ex)
            {
                var m = $"Failed to deserialize XML: {ex.Message}";
                _log.Error(m);
                return SendXmlResponse<ApiError>(m);
            }

            if (order == null || string.IsNullOrWhiteSpace(order.CustomerId) ||
                string.IsNullOrWhiteSpace(order.PoNumber) || order.ItemList == null || order.ItemList.Count == 0 ||
                order.ItemList.Count > 1)
            {
                var m = $"Invalid PurchaseOrder: {JsonConvert.SerializeObject(order)}";
                _log.Warn(m);
                return SendXmlResponse<ApiError>(m);
            }

            var processedOrder =
                _ybpService.ProcessOrder(RapidOrderType.YBP, order, out var emailMessage, out var emailSubject);
            var emailProperties = new EmailPage
            {
                From = "admin@rittenhouse.com",
                To = _collectionManagementSettings.YbpRapidOrderRecipients,
                Subject = emailSubject
            };
            _emailSiteService.SendEmailMessageToQueue(emailMessage, emailProperties);
            return SendXmlResponse<PurchaseOrderAcknowledgment>(processedOrder);
        }
    }
}