#region

using System;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Email;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class ShoppingCartTask : EmailTaskBase
    {
        private readonly EmailTaskService _emailTaskService;

        readonly StringBuilder _failureEmailAddress = new StringBuilder();
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ResourceDiscountService _resourceDiscountService;
        private readonly ShoppingCartEmailBuildService _shoppingCartEmailBuildService;

        int _failureToBuild;

        public ShoppingCartTask(
            EmailTaskService emailTaskService
            , ShoppingCartEmailBuildService shoppingCartEmailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , ResourceDiscountService resourceDiscountService
        )
            : base("ShoppingCartTask", "-SendShoppingCartEmails", "43", TaskGroup.CustomerEmails,
                "Sends the shopping cart emails to customers (IAs)", true)
        {
            _emailTaskService = emailTaskService;
            _shoppingCartEmailBuildService = shoppingCartEmailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _resourceDiscountService = resourceDiscountService;
        }

        public override void Run()
        {
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "Shopping Cart Email Task";
            var step = new TaskResultStep { Name = "ShoppingCartTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var userCount = 0;
            var successEmails = 0;
            var failureEmails = 0;

            try
            {
                var shoppingCartUsers = _emailTaskService.GetShoppingCartUsers();

                foreach (var shoppingCartUser in shoppingCartUsers)
                {
                    userCount++;
                    Log.InfoFormat("Processing {0} of {1} users - Id: {2}, username: {3}, email: {4}", userCount,
                        shoppingCartUsers.Count(), shoppingCartUser.UserId, shoppingCartUser.UserName,
                        shoppingCartUser.Email);

                    var cart = _emailTaskService.GetCartForShoppingCartTask(shoppingCartUser.CartId);

                    if (cart == null || !cart.CartItems.Any())
                    {
                        continue;
                    }

                    //This will catch the case of precision search not being selected and nothing else in cart.
                    if (cart.CartItems.Count(result => result.ResourceId.HasValue) < 1)
                    {
                        Log.InfoFormat(" 1/3/1013 Fix - Caught a cart that does not have any resources in it.");
                        continue;
                    }

                    Log.InfoFormat("Items in Cart for Institution: {0}", shoppingCartUser.InstitutionId);
                    //Needed to set current price for cart items.
                    var adminInstitution = _emailTaskService.GetAdminInstitution(shoppingCartUser.InstitutionId);

                    foreach (var item in cart.CartItems)
                    {
                        Log.InfoFormat("ResourceId: {0} || ProductId: {1}", item.ResourceId, item.ProductId);
                        Log.InfoFormat("Resource Discount Price: {0}", item.DiscountPrice);

                        _resourceDiscountService.SetDiscount(item, adminInstitution);
                    }

                    var emailMessage = ProcessCartEmail(shoppingCartUser, cart, adminInstitution);

                    if (emailMessage != null)
                    {
                        var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                            _r2UtilitiesSettings.DefaultFromAddress,
                            _r2UtilitiesSettings.DefaultFromAddressName);

                        if (success)
                        {
                            successEmails++;
                        }
                        else
                        {
                            failureEmails++;
                        }
                    }
                }

                step.CompletedSuccessfully = failureEmails == 0 && _failureToBuild == 0;
                step.Results =
                    $"{successEmails} shopping cart report emails sent, {_failureToBuild} emails failed to build, {failureEmails} emails failed to send. Failed Emails information: {_failureEmailAddress}";
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

        public EmailMessage ProcessCartEmail(ShoppingCartUser shoppingCartUser, Cart cart,
            AdminInstitution adminInstitution)
        {
            _shoppingCartEmailBuildService.ClearParameters();

            EmailMessage emailMessage = null;
            var displayInstitutionDiscount = true;
            try
            {
                foreach (var cartItem in cart.CartItems)
                {
                    IResource resource = null;
                    if (cartItem.ResourceId.HasValue)
                    {
                        resource = _emailTaskService.GetResource(cartItem.ResourceId.Value);

                        //This will prevent a fail to build error if the users cart has a deleted (ResourceStatus of 72) resource.
                        if (resource == null)
                        {
                            continue;
                        }
                    }

                    displayInstitutionDiscount =
                        displayInstitutionDiscount && cartItem.Discount == adminInstitution.Discount;
                    _shoppingCartEmailBuildService.BuildItemHtml(cartItem, resource,
                        _r2UtilitiesSettings.SpecialIconBaseUrl, shoppingCartUser.AccountNumber);
                }


                if (_shoppingCartEmailBuildService.ShouldEmailBeProcessed())
                {
                    var user = _emailTaskService.GetUser(shoppingCartUser.UserId);
                    emailMessage =
                        _shoppingCartEmailBuildService.BuildShoppingCartEmail(user, displayInstitutionDiscount);
                }


                _shoppingCartEmailBuildService.ClearParameters();
                return emailMessage;
            }
            catch (Exception ex)
            {
                _failureToBuild++;
                _failureEmailAddress.AppendFormat(
                    "FailToBuild: [InstitutionId:{0} | UserId:{1} | UserEmail: {2}] <br/>",
                    shoppingCartUser.InstitutionId, shoppingCartUser.UserId, shoppingCartUser.Email);

                Log.InfoFormat("Failed to Build Cart | cartId : {0} || error: {1}", cart.Id, ex);
            }

            return emailMessage;
        }
    }
}