#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using R2V2.Core.Api.YBP;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Api
{
    public class RapidOrderService
    {
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly IQueryable<IInstitution> _institutions;
        private readonly ILog<RapidOrderService> _log;
        private readonly IQueryable<RapidOrder> _rapidOrders;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public RapidOrderService(
            ILog<RapidOrderService> log
            , ICollectionManagementSettings collectionManagementSettings
            , IResourceService resourceService
            , IQueryable<RapidOrder> rapidOrders
            , IQueryable<IInstitution> institutions
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _collectionManagementSettings = collectionManagementSettings;
            _resourceService = resourceService;
            _rapidOrders = rapidOrders;
            _institutions = institutions;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public PurchaseOrderAcknowledgment ProcessOrder(RapidOrderType orderType, PurchaseOrder order,
            out string emailMessage, out string emailSubject)
        {
            emailMessage = "";
            emailSubject = "";
            var fileName = WriteRequestToDisk(orderType, order);

            var statusCode = GetOrderStatusCode(order);
            var acknowledgment = new PurchaseOrderAcknowledgment(order, statusCode);

            if (acknowledgment.StatusCode == "FUL" && order.ItemList != null && order.ItemList.Count > 0)
            {
                var item = order.ItemList.First();
                var resources = _resourceService.GetAllResources();
                var resource = resources.FirstOrDefault(x =>
                    x.Isbn == item.ItemId || x.Isbn10 == item.ItemId || x.Isbn13 == item.ItemId ||
                    x.EIsbn == item.ItemId);
                if (resource != null)
                {
                    if (resource.NotSaleable || resource.StatusId == (int)ResourceStatus.Archived ||
                        resource.StatusId == (int)ResourceStatus.Inactive)
                    {
                        acknowledgment.PoStatus = "IR"; //case 3  -  archived or inactive
                        acknowledgment.StatusCode = "ICH";
                    }
                    else
                    {
                        decimal.TryParse(item.Price, out var requestedPrice);

                        acknowledgment.PoStatus = "AC";
                        acknowledgment.StatusCode = "FUL";
                        acknowledgment.DiscountAmount = (requestedPrice * (decimal)0.15).ToString("#.00");
                        acknowledgment.DiscountPercent = "15";

                        var rapidOrder = new RapidOrder
                        {
                            PoNumber = acknowledgment.PoNumber,
                            StatusCode = acknowledgment.StatusCode,
                            AccountNumber = order.CustomerId,
                            Isbn10 = resource.Isbn10,
                            Isbn13 = resource.Isbn13,
                            EIsbn = resource.EIsbn,
                            ListPrice = resource.ListPrice,
                            RequestedPrice = requestedPrice,
                            CreationDate = DateTime.Now,
                            PurchaseOption = acknowledgment.PurchaseOption,
                            PoStatus = acknowledgment.PoStatus,
                            Quantity = int.Parse(item.Quantity)
                        };
                        var rapidOrderId = SaveRapidOrder(rapidOrder);
                        var emailBuilder = new StringBuilder()
                            .Append($"<div>Order#: {rapidOrderId}</div>")
                            .Append($"<div>PO#: {rapidOrder.PoNumber}</div>")
                            .Append($"<div>Customer#: {rapidOrder.AccountNumber}</div>")
                            .Append($"<div>Sku: {rapidOrder.Isbn13}</div>")
                            .Append($"<div>Quantity: {rapidOrder.Quantity}</div>")
                            .Append($"<div>Price: {rapidOrder.ListPrice}</div>")
                            .Append($"<div>OrderType: {order.OrderType}</div>");
                        emailMessage = emailBuilder.ToString();
                        emailSubject = $"{orderType} Rapid Order: {rapidOrderId} received.";
                        WriteOrderToDisk(orderType, emailMessage, rapidOrderId, fileName);
                    }
                }
                else
                {
                    acknowledgment.PoStatus = "IR";
                    acknowledgment.StatusCode = "IPI";
                }
            }

            if (acknowledgment.StatusCode != "FUL")
            {
                emailSubject = $"{orderType} Rapid Order PO: {acknowledgment.PoNumber} has been rejected.";

                switch (acknowledgment.StatusCode)
                {
                    case "IPO":
                        emailMessage = $"Reason: {acknowledgment.PoStatus} - Duplicate PO#.\r\n";
                        break;
                    case "CNS":
                        emailMessage = $"Reason: {acknowledgment.PoStatus} - Customer is not setup.\r\n";
                        break;
                    case "ICH":
                        emailMessage = $"Reason: {acknowledgment.StatusCode} - Resource is Archived or Inactive.\r\n";
                        break;
                    case "IPI":
                        emailMessage = $"Reason: {acknowledgment.StatusCode} - Resource not found.\r\n";
                        break;
                    default:
                        emailMessage = $"Reason: {acknowledgment.StatusCode} - Unknown Exception.\r\n";
                        break;
                }
            }

            return acknowledgment;
        }

        private int SaveRapidOrder(RapidOrder order)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Save(order);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            return _rapidOrders.OrderByDescending(x => x.Id).First().Id;
        }

        private string GetOrderStatusCode(PurchaseOrder order)
        {
            string statusCode = null;
            var isNumerical = int.TryParse(order.CustomerId, out _);
            if (order.CustomerId.Length != 6 || !isNumerical)
            {
                statusCode = "CNS";
            }


            if (statusCode == null)
            {
                var institution = _institutions.FirstOrDefault(x => x.AccountNumber == order.CustomerId);
                if (institution == null)
                {
                    statusCode = "CNS";
                }
            }

            if (statusCode == null)
            {
                var rapidOrder = _rapidOrders.FirstOrDefault(x => x.PoNumber == order.PoNumber);
                if (rapidOrder != null)
                {
                    statusCode = "IPO";
                }
            }
            //TODO: Check if minimum qty is required
            //Skipping for now.

            //TODO: Might need to check if US Customer. 
            //Skipping for now.

            return statusCode ?? "FUL";
        }

        private string WriteRequestToDisk(RapidOrderType orderType, PurchaseOrder order)
        {
            string rootLocation;
            switch (orderType)
            {
                case RapidOrderType.YBP:
                    rootLocation = _collectionManagementSettings.YbpOrderFileLocation;
                    break;
                case RapidOrderType.OASIS:
                    rootLocation = _collectionManagementSettings.OasisOrderFileLocation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null);
            }

            var fileName = Path.Combine(rootLocation, "request",
                $"{orderType}_{DateTime.Now:yyyyMMddss}_{order.PoNumber}.xml");
            try
            {
                var directory = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _log.Info($"Created directory: {directory}");
                }

                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    var xml = new XmlSerializer(typeof(PurchaseOrder));
                    xml.Serialize(stream, order);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return fileName;
        }

        private void WriteOrderToDisk(RapidOrderType orderType, string order, int orderId, string fileName)
        {
            string rootLocation;
            switch (orderType)
            {
                case RapidOrderType.YBP:
                    rootLocation = _collectionManagementSettings.YbpOrderFileLocation;
                    break;
                case RapidOrderType.OASIS:
                    rootLocation = _collectionManagementSettings.OasisOrderFileLocation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null);
            }

            var orderFileName = Path.Combine(rootLocation, "order", $"{orderType}_{orderId}.txt");
            var newFileName = fileName.Replace(".xml", $"_{orderId}.xml");

            var directory = Path.GetDirectoryName(orderFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _log.Info($"Created directory: {directory}");
            }

            File.WriteAllText(orderFileName, order);
            File.Move(fileName, newFileName);
        }
    }
}