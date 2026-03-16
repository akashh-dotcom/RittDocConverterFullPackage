#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EasyNetQ;
using R2V2.Core;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Promotion;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.WindowsService.DataServices;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

//using R2V2.Infrastructure.MessageQueue;

namespace R2V2.WindowsService.Threads.Promotion
{
    public class PromoteRequestThread : PromotionThreadBase, IR2V2Thread
    {
        private readonly EmailQueueService _emailQueueService;
        private readonly ILog<PromoteRequestThread> _log;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly PdaRuleService _pdaRuleService;

        private readonly PromotionService _promotionService;
        private readonly ResourcePromotionService _resourcePromotionService;
        private readonly ResourceToPromoteDataService _resourceToPromoteDataService;
        private readonly UserService _userService;
        private readonly WindowsServiceSettings _windowsServiceSettings;

        public PromoteRequestThread(
            ILog<PromoteRequestThread> log
            , IMessageQueueSettings messageQueueSettings
            , WindowsServiceSettings windowsServiceSettings
            , ResourceToPromoteDataService resourceToPromoteDataService
            , EmailQueueService emailQueueService
            , PromotionService promotionService
            , ResourcePromotionService resourcePromotionService
            , UserService userService
            , PdaRuleService pdaRuleService
            , IUnitOfWorkProvider unitOfWorkProvider
        )
            : base(unitOfWorkProvider)
        {
            _promotionService = promotionService;
            _resourcePromotionService = resourcePromotionService;
            _userService = userService;
            _pdaRuleService = pdaRuleService;
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _windowsServiceSettings = windowsServiceSettings;
            _resourceToPromoteDataService = resourceToPromoteDataService;
            _emailQueueService = emailQueueService;
            StopProcessing = false;
            _log.Debug("PromoteRequestThread initialized");
        }

        public void Start()
        {
            _log.Info("PromoteRequestThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "PromoteRequest" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Info("Start()");
                _log.Error(ex.Message, ex);
            }

            _log.Info("PromoteRequestThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("PromoteRequestThread is now stopping...");
            StopProcessing = true;
            _log.Info("PromoteRequestThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                PromoteRequestProcessor();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                Thread.Sleep(1000);
                throw;
            }
            finally
            {
                _log.Info("StartProcessQueue() <<<");
            }
        }


        private void PromoteRequestProcessor()
        {
            _log.Debug("PromoteRequestProcessor >>");

            using (var bus = RabbitHutch.CreateBus(_messageQueueSettings.ConnectionString))
            {
                var queue = bus.Advanced.QueueDeclare(_messageQueueSettings.PromoteRequestQueueName);
                bus.Advanced.Consume(queue,
                    x => x.Add<PromoteRequest>((message, info) => ProcessPromoteRequestMessage(message.Body)));

                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        /// <summary>
        ///     Process request to promote individual resources
        /// </summary>
        private void ProcessPromoteRequestMessage(PromoteRequest promoteRequest)
        {
            try
            {
                _log.Debug(promoteRequest.ToDebugString());

                SetResourcePromoteQueueStatus(promoteRequest.ResourcePromoteQueue,
                    ResourcePromoteStatus.PromotionStarted);

                var resourceToPromote = _resourceToPromoteDataService.GetResourceToPromote(
                    promoteRequest.ResourcePromoteQueue.ResourceId, promoteRequest.ResourcePromoteQueue.Isbn);
                _log.Debug(resourceToPromote.ToDebugString());

                if (_promotionService.Promote(promoteRequest))
                {
                    SetResourcePromoteQueueStatus(promoteRequest.ResourcePromoteQueue,
                        ResourcePromoteStatus.CompletedWithErrors);
                }
                else
                {
                    SetResourcePromoteQueueStatus(promoteRequest.ResourcePromoteQueue,
                        ResourcePromoteStatus.CompletedWithErrors);
                }

                // send status email
                SendStatusEmail();
            }
            catch (Exception ex)
            {
                _log.Info("ProcessPromoteRequestMessage()");
                _log.Error(ex.Message, ex);

                if (promoteRequest != null)
                {
                    promoteRequest.ErrorCount++;
                    _log.InfoFormat("Resend promotion request: {0}", promoteRequest.ToDebugString());
                    // resend message
                    _resourcePromotionService.WritePromotionRequestToMessageQueue(promoteRequest);
                }

                SleepThreadAfterException();
            }
        }

        private void SendStatusEmail()
        {
            //bool status, string results,
            var resource = _promotionService.ResourceToPromote;
            var promoteRequest = _promotionService.PromoteRequest;
            var emailMessage = new EmailMessage
            {
                Subject =
                    $"{(_promotionService.Successful ? "Ok" : "ERROR")} - R2v2 Promotion for ISBN: {resource.Isbn}, {resource.Title}",
                Body = GetEmailMessageBody(),
                IsHtml = true,
                FromAddress = _windowsServiceSettings.PromoteFromEmailAddress,
                FromDisplayName = _windowsServiceSettings.PromoteFromDisplayName,
                ReplyToAddress = _windowsServiceSettings.PromoteFromEmailAddress,
                ReplyToDisplayName = _windowsServiceSettings.PromoteFromDisplayName
            };

            var emailAddresses = new List<string>();

            var raPromotionUsers = _userService.GetRaUsersWhoCanPromote();
            var addedByUser =
                raPromotionUsers.FirstOrDefault(x => x.Id == promoteRequest.ResourcePromoteQueue.AddedByUserId);
            if (addedByUser != null)
            {
                emailAddresses.Add(addedByUser.Email);
            }

            if (promoteRequest.ResourcePromoteQueue.AddedByUserId !=
                promoteRequest.ResourcePromoteQueue.PromotedByUserId)
            {
                var promotedByUser =
                    raPromotionUsers.FirstOrDefault(x => x.Id == promoteRequest.ResourcePromoteQueue.PromotedByUserId);
                if (promotedByUser != null)
                {
                    emailAddresses.Add(promotedByUser.Email);
                }
            }


            var toAddresses = _windowsServiceSettings.PromoteStatusEmailToAddresses.Split(';');
            foreach (var address in toAddresses)
            {
                var emailAddress = emailAddresses.FirstOrDefault(x => x.ToLower() == address.ToLower());
                if (emailAddress != null)
                {
                    emailAddresses.Add(emailAddress);
                }
            }

            foreach (var emailAddress in emailAddresses)
            {
                if (!emailMessage.AddToRecipient(emailAddress))
                {
                    _log.ErrorFormat("invalid TO email address <{0}>", emailAddress);
                }
            }

            _emailQueueService.QueueEmailMessage(emailMessage);
        }

        private string GetEmailMessageBody()
        {
            var resource = _promotionService.ResourceToPromote;
            var promoteRequest = _promotionService.PromoteRequest;

            var raPromotionUsers = _userService.GetRaUsersWhoCanPromote();
            var addedByUser =
                raPromotionUsers.FirstOrDefault(x => x.Id == promoteRequest.ResourcePromoteQueue.AddedByUserId);
            var promotedByUser =
                raPromotionUsers.FirstOrDefault(x => x.Id == promoteRequest.ResourcePromoteQueue.PromotedByUserId);

            var emailBody = new StringBuilder()
                .AppendLine("<html><body><title>R2v2 Promotion Status</title>")
                .AppendLine("<style type=\"text/css\">")
                .AppendLine("body { font-family: Sans-serif, Arial, Verdana; font-size: 100%; }")
                .AppendLine("h1 { font-size: 1.4em; font-weight:bold; }")
                .AppendLine("h2 { font-size: 1.2em; font-weight:bold; }")
                .AppendLine(".status { font-size: 1.1em; font-weight:bold; }")
                .AppendLine(".label { font-size: 1.0em; font-weight:bold; }")
                .AppendLine(".step { font-size: 1.1em; font-weight:bold; padding-top: 15px;}")
                .AppendLine(".ok { color:green; }")
                .AppendLine(".stepData { font-size: 0.8em; padding-left: 20px; }")
                .AppendLine(".error { color:red; }")
                .AppendLine("</style></head>")
                .AppendLine("<body>")
                .AppendLine("<h1>R2v2 Promotion Status</h1>")
                .AppendFormat("<h2>Resource: {0}, {1}</h2>", resource.Isbn, resource.Title).AppendLine()
                .AppendFormat("<div class=\"status\">Status: <span class=\"{1}\">{0}</span></div>",
                    _promotionService.Successful ? "Ok" : "ERROR", _promotionService.Successful ? "ok" : "error")
                .AppendLine();

            if (addedByUser != null)
            {
                emailBody.AppendFormat("<div>Added By: {0} {1}, <{2}></div>", addedByUser.FirstName,
                        addedByUser.LastName, addedByUser.Email)
                    .AppendLine();
            }

            if (promotedByUser != null)
            {
                emailBody.AppendFormat("<div>Initiated By: {0} {1}, <{2}></div>", promotedByUser.FirstName,
                        promotedByUser.LastName, promotedByUser.Email)
                    .AppendLine();
            }

            emailBody.AppendFormat("<div>Staging Front End URL: {0}</div>", _promotionService.StagingFrontEndUrl)
                .AppendLine()
                .AppendFormat("<div>Staging Admin URL: {0}</div>", _promotionService.StagingBackEndUrl).AppendLine();

            if (!string.IsNullOrWhiteSpace(_promotionService.ProductionFrontEndUrl))
            {
                emailBody.AppendFormat("<div>Production Front End URL: {0}</div>",
                    _promotionService.ProductionFrontEndUrl).AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(_promotionService.ProductionBackEndUrl))
            {
                emailBody.AppendFormat("<div>Production Admin URL: {0}</div>", _promotionService.ProductionBackEndUrl)
                    .AppendLine();
            }

            emailBody.AppendLine("<div class=\"label\">Promotion Information:</div>");

            var htmlResults = _promotionService.Results.Replace("\r\n", "<br />\r\n");

            emailBody.AppendFormat("<div class=\"stepData\">{0}</div>", htmlResults).AppendLine()
                .AppendLine("<br />");

            if (_promotionService.Successful)
            {
                emailBody.AppendLine(
                    "<div>Resource is now available for browse only.  Search will not be available for this resource until the content is transformed and indexed in (1 to 3 hours).</div>");
            }

            emailBody.AppendLine("</body></html>");

            return emailBody.ToString();
        }
    }
}