#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.OrderHistory;
using R2V2.Core.Resource;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class OrderSummaryTask : EmailTaskBase, ITask
    {
        private readonly IQueryable<Cart> _carts;
        private readonly IQueryable<Institution> _institutions;
        private readonly IQueryable<DbOrderHistory> _orderHistories;
        private readonly OrderSummaryEmailBuildService _orderSummaryEmailBuildService;
        private readonly IQueryable<Product> _products;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IQueryable<IResource> _resources;
        private readonly IUnitOfWork _unitOfWork;

        private DateTime _OrderSummaryDate;

        public OrderSummaryTask(
            IR2UtilitiesSettings r2UtilitiesSettings
            , IUnitOfWork unitOfWork
            , IQueryable<Cart> carts
            , IQueryable<DbOrderHistory> orderHistories
            , IQueryable<Institution> institutions
            , IQueryable<IResource> resources
            , IQueryable<Product> products
            , OrderSummaryEmailBuildService orderSummaryEmailBuildService
        )
            : base("OrderSummaryTask", "-OrderSummaryTask", "60", TaskGroup.InternalSystemEmails,
                "Sends the system wide order summary email", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _unitOfWork = unitOfWork;
            _carts = carts;
            _orderHistories = orderHistories;
            _institutions = institutions;
            _resources = resources;
            _products = products;
            _orderSummaryEmailBuildService = orderSummaryEmailBuildService;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _OrderSummaryDate = GetArgumentDateTime("orderSummaryDate", DateTime.Now);
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            Log.Info($"-job: OrderSummaryTask, -orderSummaryDate: {_OrderSummaryDate}");
        }

        public override void Run()
        {
            TaskResult.Information = "Order Summary Task Run";
            var step = new TaskResultStep { Name = "OrderSummaryTask Run", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var orderHistories = _orderHistories.Where(x => x.PurchaseDate.Date == _OrderSummaryDate.Date).ToList();

                var succeed = ProcessOrderSummary(orderHistories);

                step.CompletedSuccessfully = succeed;
                step.Results = "Order Summary Task completed successfully";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        public bool ProcessOrderSummary(List<DbOrderHistory> orderHistories)
        {
            TaskResult.Information = "Process Order Summary";
            var step = new TaskResultStep { Name = "ProcessOrderSummary (Build Email)", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var success = false;

            try
            {
                var totalOrders = 0;
                var totalTitles = 0;
                var totalLicenses = 0;
                var totalMaintenanceFee = 0;
                decimal totalSales = 0;

                foreach (var orderHistory in orderHistories)
                {
                    decimal orderHistoryTotal = 0;
                    var institution = _institutions.FirstOrDefault(x => x.Id == orderHistory.InstitutionId);
                    var institutionInfo = "";
                    if (institution != null)
                    {
                        institutionInfo = $"{institution.Name}<br/>{institution.AccountNumber}";
                    }

                    var resourceItems = new StringBuilder();
                    var productItems = new StringBuilder();

                    var orderHistoryItems = orderHistory.OrderHistoryItems.ToList();

                    var resourceIds = orderHistoryItems.Where(x => x.ResourceId != null)
                        .Select(x => x.ResourceId.Value);
                    var productIds = orderHistoryItems.Where(x => x.ProductId != null).Select(x => x.ProductId.Value);

                    var orderHistoryResources = _resources.Where(x => resourceIds.Contains(x.Id)).ToList();
                    var orderHistoryProducts = _products.Where(x => productIds.Contains(x.Id)).ToList();

                    foreach (var orderHistoryItem in orderHistoryItems)
                    {
                        if (orderHistoryItem.ProductId != null && orderHistoryItem.ProductId > 0)
                        {
                            var product =
                                orderHistoryProducts.FirstOrDefault(x => x.Id == orderHistoryItem.ProductId.Value) ??
                                new Product { Name = "N/A" };
                            productItems.Append("<br/>");

                            productItems.AppendFormat("{0}", product.Name);
                            orderHistoryTotal += orderHistoryItem.DiscountPrice;
                            switch (product.Id)
                            {
                                case 1:
                                    totalMaintenanceFee++;
                                    break;
                            }

                            continue;
                        }

                        var resource = orderHistoryResources.FirstOrDefault(x => x.Id == orderHistoryItem.ResourceId);

                        if (resource != null)
                        {
                            if (resourceItems.Length > 0)
                            {
                                resourceItems.Append("<br/>");
                            }

                            totalTitles++;

                            resourceItems.AppendFormat("{0} ({1})", resource.Isbn, orderHistoryItem.NumberOfLicenses);
                            if (orderHistoryItem.IsBundle)
                            {
                                orderHistoryTotal += orderHistoryItem.DiscountPrice;
                            }
                            else if (!resource.IsFreeResource)
                            {
                                orderHistoryTotal += orderHistoryItem.DiscountPrice * orderHistoryItem.NumberOfLicenses;
                            }

                            totalLicenses += orderHistoryItem.NumberOfLicenses;

                            continue;
                        }

                        Log.DebugFormat(
                            $"Error with OrderHistoryItem: {orderHistoryItem.Id} in OrderHistory: {orderHistory.Id}");
                    }

                    totalSales += orderHistoryTotal;
                    totalOrders++;

                    _orderSummaryEmailBuildService.BuildItemHtml(orderHistory, institutionInfo,
                        resourceItems.ToString(), productItems.ToString(), orderHistoryTotal);
                }


                var emailMessage = _orderSummaryEmailBuildService.BuildOrderSummaryEmail(totalOrders, totalSales,
                    totalTitles, totalLicenses,
                    totalMaintenanceFee,
                    EmailSettings.TaskEmailConfig.ToAddresses.ToArray());
                if (emailMessage != null)
                {
                    // add cc & bcc
                    AddTaskCcToEmailMessage(emailMessage);
                    AddTaskBccToEmailMessage(emailMessage);

                    success = EmailDeliveryService.SendTaskReportEmail(emailMessage,
                        _r2UtilitiesSettings.DefaultFromAddress,
                        _r2UtilitiesSettings.DefaultFromAddressName);
                }

                step.CompletedSuccessfully = true;
                step.Results = "Process Order Summary completed successfully";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }

            return success;
        }
    }
}