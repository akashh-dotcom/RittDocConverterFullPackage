#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Email;

#endregion

namespace R2Utilities.Tasks.EmailTasks.WeeklyEmails
{
    public class PdaRemovedFromCartTask : EmailTaskBase
    {
        private readonly EmailTaskService _emailTaskService;
        readonly StringBuilder _failureEmailAddress = new StringBuilder();
        private readonly PdaEmailBuildService _pdaEmailBuildService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ResourceDiscountService _resourceDiscountService;
        private int _failCount;

        private int _successCount;


        public PdaRemovedFromCartTask(
            EmailTaskService emailTaskService
            , PdaEmailBuildService pdaEmailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , ResourceDiscountService resourceDiscountService
        )
            : base("PdaRemovedFromCartTask", "-PdaRemovedAddedToCartTask", "47", TaskGroup.CustomerEmails,
                "Sends PDA resource removed from cart email to customers (IAs)", true)
        {
            _emailTaskService = emailTaskService;
            _pdaEmailBuildService = pdaEmailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _resourceDiscountService = resourceDiscountService;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "Pda Removed From Cart Email Task";
            var step = new TaskResultStep { Name = "PdaRemovedFromCartTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _successCount = 0;
                _failCount = 0;

                //_pdaEmailBuildService.SetTemplates(EmailTemplates.PdaRemovedFromCartBody, EmailTemplates.PdaRemovedFromCartItem);
                _pdaEmailBuildService.InitEmailTemplatesForRemovedFromCart();

                var userIds = _emailTaskService.GetPdaRemovedFromCartUserIds();

                foreach (var userId in userIds)
                {
                    var pdaResources = _emailTaskService.GetPdaRemovedFromCartResources(userId);

                    if (pdaResources.Any())
                    {
                        ProcessUser(userId, pdaResources);
                    }
                }

                step.CompletedSuccessfully = true;

                var test = new StringBuilder()
                    .AppendFormat("<div>{0} PDA Removed from Cart emails were sent</div>", _successCount).AppendLine()
                    .AppendFormat("<div>{0} emails failed</div>", _failCount).AppendLine()
                    .AppendFormat("<div>Emails that failed: {0}</div>", _failureEmailAddress).AppendLine()
                    .ToString();
                step.Results = test;
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

        public void ProcessUser(int userId, List<PdaResource> pdaResources)
        {
            var user = _emailTaskService.GetUser(userId);
            var resources =
                _emailTaskService.GetResources(pdaResources.Select(x => x.ResourceId.GetValueOrDefault()).ToList());
            var adminInstitution = new AdminInstitution(user.Institution);

            var itemBuilder = new StringBuilder();

            foreach (var resource in resources)
            {
                var pdaResource = pdaResources.FirstOrDefault(x => x.ResourceId == resource.Id);
                if (pdaResource != null)
                {
                    pdaResource.ListPrice = resource.ListPrice;
                    _resourceDiscountService.SetDiscount(pdaResource, adminInstitution);


                    itemBuilder.Append(
                        _pdaEmailBuildService.BuildPdaRemovedFromCartItemTemplate(resource,
                            user.Institution.Discount,
                            pdaResource.AddedDate,
                            pdaResource.AddedToCartDate, user.Institution.AccountNumber, pdaResource.SpecialText)
                    );
                }
            }

            var territoryusers = new List<User>();
            if (user.Institution != null)
            {
                if (user.Institution.Territory != null)
                {
                    territoryusers = _emailTaskService.GetTerritoryOwners(user.Institution.Territory.Id);
                }
            }

            var userArray = territoryusers.Any() ? territoryusers.Select(x => x.Email).ToArray() : null;

            var emailMessage =
                _pdaEmailBuildService.BuildPdaRemovedFromCartEmail(itemBuilder.ToString(), user, userArray);

            if (emailMessage != null)
            {
                var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                    _r2UtilitiesSettings.DefaultFromAddress,
                    _r2UtilitiesSettings.DefaultFromAddressName);

                if (success)
                {
                    _successCount++;
                    _emailTaskService.InsertPdaRemovedFromCartResourceEmail(userId);
                    return;
                }
            }

            _failCount++;
            _failureEmailAddress.AppendFormat("FailToSend: [InstitutionId:{0} | UserId:{1} | UserEmail: {2}] <br/>",
                user.Institution != null ? user.Institution.Id : 0, user.Id, user.Email);
        }
    }
}