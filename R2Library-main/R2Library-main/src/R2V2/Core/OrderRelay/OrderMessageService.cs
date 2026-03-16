#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.OrderRelay
{
    public class OrderMessageService
    {
        private readonly ILog<OrderMessageService> _log;
        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly ResourceService _resourceService;

        public OrderMessageService(ILog<OrderMessageService> log
            , ResourceService resourceService
            , IMessageQueueService messageQueueService
            , IMessageQueueSettings messageQueueSettings
        )
        {
            _log = log;
            _resourceService = resourceService;
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings;
        }

        /// <summary>
        ///     This is a stripped down version of the Order class from Rittenhouse.com.
        ///     SJS - 10/28/2013 - Didn't include any of the fields that are not needed.
        /// </summary>
        public OrderMessage BuildOrderMessage(Cart cart, CollectionManagement.Promotion promotion,
            IAdminInstitution institution, IUser user)
        {
            if (cart == null)
            {
                throw new Exception("Cart is null");
            }

            if (cart.PurchaseDate == null)
            {
                _log.ErrorFormat("Invalid cart, {0}", cart.ToDebugString());
                throw new Exception("invalid cart date");
            }

            if (institution == null)
            {
                throw new Exception("institution is null");
            }

            var orderMessage = new OrderMessage
            {
                AccountNumber = institution.AccountNumber,
                ContactName = user.ToFullName(),
                ConfirmationEmailAddress = user.Email,
                PurchaseOrderNumber = cart.PurchaseOrderNumber,
                WebOrderNumber = cart.OrderNumber,
                PaymentTerm = GetPaymentTerm(cart.BillingMethod),
                OrderDate = cart.PurchaseDate.Value,
                RequiredDate = cart.PurchaseDate.Value,
                AutoCancelDate = cart.PurchaseDate.Value.AddYears(1),
                Instruction1 = null,
                Instruction2 = null,
                Instruction3 = null,
                QuotationNumber = null,
                AdditionalDiscountPercentage = 0.0m
            };

            if (promotion == null || cart.Reseller != null)
            {
                orderMessage.OrderSource = 'R';
                orderMessage.PromotionCode = null;

                if (cart.Reseller != null)
                {
                    if (!string.IsNullOrWhiteSpace(cart.Reseller.AccountNumberOverride))
                    {
                        orderMessage.AccountNumber = cart.Reseller.AccountNumberOverride;
                    }
                }
            }
            else
            {
                orderMessage.OrderSource = promotion.OrderSource.Trim()[0];
                orderMessage.PromotionCode = promotion.Code;
            }

            var orderContainsAnnualFee = false;

            var list = new List<OrderMessageItem>();
            foreach (var cartItem in cart.CartItems)
            {
                if (!cartItem.Include)
                {
                    continue;
                }

                var item = new OrderMessageItem();
                if (cartItem.ResourceId != null && cartItem.ResourceId > 0)
                {
                    var resource = _resourceService.GetResource(cartItem.ResourceId.Value);
                    if (resource == null)
                    {
                        _log.Info("Resource is marked as deleted at the time of purchase and cannot be purchased.");
                        _log.InfoFormat(
                            "Cart Resource NOT found. resource id: {0}, cart item id: {1}, cart id: {2}, institution id: {3}, account: {4} - {5}",
                            cartItem.ResourceId, cartItem.Id, cart.Id, institution.Id, institution.AccountNumber,
                            institution.Name);
                        continue;
                    }

                    list.Add(item);

                    item.IsSpecialDiscount = (string.IsNullOrWhiteSpace(orderMessage.PromotionCode) &&
                                              cartItem.Discount > institution.Discount) ||
                                             (!string.IsNullOrWhiteSpace(orderMessage.PromotionCode) &&
                                              promotion != null && promotion.Discount > cartItem.Discount);
                    item.DiscountPercentage = institution.Discount == cartItem.Discount ? 0.0m : cartItem.Discount;

                    item.Quantity = cartItem.GetLicenseCount(resource);

                    item.Sku = $"R2P{resource.Isbn}";
                    if (resource.IsForthcoming())
                    {
                        item.LineNotes =
                            $"Pre-Order - {(cart.ForthcomingTitlesInvoicingMethod == ForthcomingTitlesInvoicingMethodEnum.InvoiceWhenReleased ? "Invoice When Released" : "Invoice Now")}";
                    }
                }
                else if (cartItem.Product != null && cartItem.Include)
                {
                    list.Add(item);

                    item.DiscountPercentage = institution.Discount == cartItem.Discount ? 0.0m : cartItem.Discount;
                    item.Quantity = 1;
                    item.Sku = cartItem.Product.Id == 1
                        ? $"R2YRSUBFEE{cart.PurchaseDate:yy}"
                        : cartItem.Product.Id == 2
                            ? "R2PRECISION12"
                            : "INVALID";
                    //: (cartItem.Product.Id == 2) ? string.Format("R2PRECISION12{0:YY}", cart.PurchaseDate) : "INVALID";
                    orderContainsAnnualFee = orderContainsAnnualFee || cartItem.Product.Id == 1;
                }
            }

            // set header notes for special cases
            var headerNotes = cart.PurchaseOrderComment;
            if (cart.BillingMethod == BillingMethodEnum.DepositAccount)
            {
                headerNotes =
                    $"Bill to my deposit account{(string.IsNullOrEmpty(headerNotes) ? string.Empty : " -- ")}{headerNotes}";
            }

            if (orderContainsAnnualFee)
            {
                headerNotes =
                    $"R2 Initial Purchase{(string.IsNullOrEmpty(headerNotes) ? string.Empty : " -- ")}{headerNotes}";
            }

            if (cart.Reseller != null)
            {
                headerNotes =
                    $"{cart.Reseller.Name} CUSTOMER {institution.AccountNumber}: {institution.Name}{(string.IsNullOrEmpty(headerNotes) ? string.Empty : " -- ")}{headerNotes}";

                orderMessage.ShipToNumber = institution.AccountNumber;
            }

            orderMessage.HeaderNotes = headerNotes;

            orderMessage.OrderItems = list.ToArray();
            return orderMessage;
        }

        private string GetPaymentTerm(BillingMethodEnum billingMethod)
        {
            return billingMethod == BillingMethodEnum.CreditCardOnFile ? "03" : "01";
        }

        public string SendOrderMessageToQueue(OrderMessage orderMessage)
        {
            var json = JsonConvert.SerializeObject(orderMessage);
            _log.DebugFormat("orderMessage as json: {0}", json);

            var success = _messageQueueService.WriteMessageToQueue(_messageQueueSettings.OrderProcessingQueue, json);
            if (!success)
            {
                _log.ErrorFormat("Error sending R2 order to the message queue, {0}", orderMessage.ToDebugString());
            }

            return json;
        }
    }
}