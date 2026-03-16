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
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.Promotion
{
    public class ResourcePromotionThread : ThreadBase, IR2V2Thread
    {
        private readonly EmailQueueService _emailQueueService;
        private readonly ILog<ResourcePromotionThread> _log;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly PdaRuleService _pdaRuleService;
        private readonly PromotionService _promotionService;
        private readonly IQueryable<ResourcePromoteQueue> _resourcePromoteQueues;
        private readonly ResourcePromotionService _resourcePromotionService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserService _userService;
        private readonly WindowsServiceSettings _windowsServiceSettings;

        /// <summary>
        ///     -debug -service=r2v2.promotionservice
        /// </summary>
        public ResourcePromotionThread(
            ILog<ResourcePromotionThread> log
            , IMessageQueueSettings messageQueueSettings
            , ResourcePromotionService resourcePromotionService
            , IUnitOfWorkProvider unitOfWorkProvider
            , PromotionService promotionService
            , PdaRuleService pdaRuleService
            , WindowsServiceSettings windowsServiceSettings
            , EmailQueueService emailQueueService
            , IQueryable<ResourcePromoteQueue> resourcePromoteQueues
            , UserService userService
        )
        {
            _resourcePromotionService = resourcePromotionService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _promotionService = promotionService;
            _pdaRuleService = pdaRuleService;
            _windowsServiceSettings = windowsServiceSettings;
            _emailQueueService = emailQueueService;
            _resourcePromoteQueues = resourcePromoteQueues;
            _userService = userService;
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            StopProcessing = false;
            _log.Debug("ResourcePromotionThread initialized");
        }

        public void Start()
        {
            _log.Info("ResourcePromotionThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "ResourcePromotion" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Info("Start()");
                _log.Error(ex.Message, ex);
            }

            _log.Info("ResourcePromotionThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("ResourcePromotionThread is now stopping...");
            StopProcessing = true;
            _log.Info("ResourcePromotionThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                ResourcePromotionProcessor();
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


        private void ResourcePromotionProcessor()
        {
            _log.Debug("ResourcePromotionProcessor >>");

            using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.New))
            {
                var raUsers = _userService.GetRaUsersWhoCanPromote();
                foreach (var raUser in raUsers)
                {
                    _log.DebugFormat("raUser --> {0}", raUser.ToDebugString());
                }

                _log.DebugFormat("MessageQueueSettings.ConnectionString: {0}",
                    _messageQueueSettings.EnvironmentConnectionString);
                _log.DebugFormat("MessageQueueSettings.ResourceBatchPromotionQueueName: {0}",
                    _messageQueueSettings.ResourceBatchPromotionQueueName);
                _log.DebugFormat("uow.Session.IsOpen: {0}, uow.Session.IsConnected: {1}", uow.Session.IsOpen,
                    uow.Session.IsConnected);

                using (var bus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString))
                {
                    var queue = bus.Advanced.QueueDeclare(_messageQueueSettings.ResourceBatchPromotionQueueName);

                    bus.Advanced.Consume(queue, x => x
                        .Add<InitiatePromotionBatch>((message, info) => ProcessResourcePromotionMessage(message.Body))
                        .Add<PromoteRequest>((message, info) => ProcessPromoteRequestMessage(message.Body))
                        .Add<PromotionBatchComplete>((message, info) =>
                            ProcessPromotionBatchCompleteMessage(message.Body))
                    );

                    while (!StopProcessing)
                    {
                        Thread.Sleep(1000);
                    }
                }

                _log.Info("STOP REQUESTED");
            }
        }

        /// <summary>
        ///     Send a single promotion request message for each resource in the ResourcePromoteQueue.
        /// </summary>
        private void ProcessResourcePromotionMessage(InitiatePromotionBatch initiatePromotionBatch)
        {
            _log.DebugFormat("ProcessResourcePromotionMessage() >>>>>>");
            _log.DebugFormat("initiatePromotionBatch JSON: {0}", initiatePromotionBatch.ToJsonString());
            _log.Debug(initiatePromotionBatch.ToDebugString());

            foreach (var promoteRequest in initiatePromotionBatch.PromoteRequests)
            {
                _resourcePromotionService.WritePromotionRequestToMessageQueue(promoteRequest);
            }

            var promotionBatchComplete = new PromotionBatchComplete
            {
                BatchKey = initiatePromotionBatch.BatchKey,
                BatchName = initiatePromotionBatch.BatchName,
                StartTimestamp = initiatePromotionBatch.StartTimestamp
            };

            _resourcePromotionService.WritePromotionBatchCompleteToMessageQueue(promotionBatchComplete);

            _log.DebugFormat("ProcessResourcePromotionMessage() <<<<<<");
        }

        /// <summary>
        ///     Process request to promote individual resources
        /// </summary>
        private void ProcessPromoteRequestMessage(PromoteRequest promoteRequest)
        {
            _log.DebugFormat("ProcessPromoteRequestMessage() >>>>>>");
            _log.DebugFormat("promoteRequest JSON: {0}", promoteRequest.ToJsonString());
            using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.New))
            {
                try
                {
                    _log.DebugFormat("uow.Session.IsOpen: {0}, uow.Session.IsConnected: {1}", uow.Session.IsOpen,
                        uow.Session.IsConnected);
                    _log.Debug(promoteRequest.ToDebugString());

                    var resourcePromoteQueue = SetResourcePromoteQueueStatus(promoteRequest.ResourceId,
                        promoteRequest.BatchKey,
                        ResourcePromoteStatus.PromotionStarted);

                    var resourcePromoteQueues =
                        _resourcePromotionService.GetResourcePromoteQueue(promoteRequest.BatchKey);
                    foreach (var rpq in resourcePromoteQueues)
                    {
                        _log.Debug(rpq.ToDebugString());
                    }

                    _log.DebugFormat("uow.Session.IsOpen: {0}, uow.Session.IsConnected: {1}", uow.Session.IsOpen,
                        uow.Session.IsConnected);

                    resourcePromoteQueue = SetResourcePromoteQueueStatus(resourcePromoteQueue,
                        _promotionService.Promote(promoteRequest)
                            ? ResourcePromoteStatus.CompletedSuccessfully
                            : ResourcePromoteStatus.CompletedWithErrors);


                    _log.Debug(resourcePromoteQueue.ToDebugString());

                    // send status email
                    SendStatusEmail();

                    ClearExceptionCounters();
                }
                catch (Exception ex)
                {
                    var msg = new StringBuilder()
                        .AppendFormat("EXCEPTION IN ProcessPromoteRequestMessage() - {0}", ex.Message);
                    msg.AppendLine().AppendLine(promoteRequest.ToDebugString());
                    msg.AppendLine(promoteRequest.ToJsonString());
                    msg.AppendLine().AppendLine("This message should be reprocessed, please verify!");
                    _log.Error(msg.ToString(), ex);
                    SleepThreadAfterException();
                }

                _log.DebugFormat("ProcessPromoteRequestMessage() <<<<<<");
            }
        }

        /// <summary>
        ///     Handle promotion response requests
        /// </summary>
        private void ProcessPromotionBatchCompleteMessage(PromotionBatchComplete promotionBatchComplete)
        {
            _log.DebugFormat("ProcessPromotionBatchCompleteMessage() >>>>>>");
            _log.DebugFormat("promotionBatchComplete JSON: {0}", promotionBatchComplete.ToJsonString());
            using (var uow = _unitOfWorkProvider.Start(UnitOfWorkScope.New))
            {
                uow.Session.Clear();

                _log.Debug(promotionBatchComplete.ToDebugString());

                var resourcePromoteQueues =
                    _resourcePromotionService.GetResourcePromoteQueue(promotionBatchComplete.BatchKey);

                var promotedIsbns = new List<string>();
                var completedResourceCount = 0;
                foreach (var resourcePromoteQueue in resourcePromoteQueues)
                {
                    _log.Debug(resourcePromoteQueue.ToDebugString());
                    if (resourcePromoteQueue.PromoteStatus == ResourcePromoteStatus.CompletedSuccessfully ||
                        resourcePromoteQueue.PromoteStatus == ResourcePromoteStatus.CompletedWithErrors)
                    {
                        if (resourcePromoteQueue.PromoteStatus == ResourcePromoteStatus.CompletedSuccessfully)
                        {
                            //promotedResourceIds.Add(resourcePromoteQueue.ResourceId);
                            promotedIsbns.Add(resourcePromoteQueue.Isbn);
                        }

                        completedResourceCount++;
                    }
                }

                _log.DebugFormat("resourcePromoteQueues.Count: {0}, completedResourceCount: {1}",
                    resourcePromoteQueues.Count, completedResourceCount);
                if (resourcePromoteQueues.Count == completedResourceCount)
                {
                    // send message to ongoing PDA
                    var ongoingPdaEventMessage = new OngoingPdaEventMessage(OngoingPdaEventType.Promotion);
                    //ongoingPdaEventMessage.AddResourceIds(promotedResourceIds);
                    ongoingPdaEventMessage.AddIsbns(promotedIsbns);

                    _pdaRuleService.WriteOngoingPdaEventMessageToMessageQueue(ongoingPdaEventMessage);
                }
            }

            _log.DebugFormat("ProcessPromotionBatchCompleteMessage() <<<<<<");
        }

        private void SendStatusEmail()
        {
            //bool status, string results,
            var resource = _promotionService.ResourceToPromote;
            var promoteRequest = _promotionService.PromoteRequest;
            var emailMessage = new EmailMessage
            {
                Subject =
                    $"{_promotionService.EmailMessageStatus()} - R2v2 Promotion for ISBN: {resource.Isbn}, {resource.Title}",
                Body = GetEmailMessageBody(),
                IsHtml = true,
                FromAddress = _windowsServiceSettings.PromoteFromEmailAddress,
                FromDisplayName = _windowsServiceSettings.PromoteFromDisplayName,
                ReplyToAddress = _windowsServiceSettings.PromoteFromEmailAddress,
                ReplyToDisplayName = _windowsServiceSettings.PromoteFromDisplayName
            };

            var emailAddresses = new List<string>();

            if (!string.IsNullOrWhiteSpace(promoteRequest.AddedByUser.UserEmailAddress))
            {
                emailAddresses.Add(promoteRequest.AddedByUser.UserEmailAddress);
            }

            if (promoteRequest.AddedByUser.UserId != promoteRequest.PromotedByUser.UserId)
            {
                if (!string.IsNullOrWhiteSpace(promoteRequest.PromotedByUser.UserEmailAddress))
                {
                    emailAddresses.Add(promoteRequest.PromotedByUser.UserEmailAddress);
                }
            }

            _log.DebugFormat("PromoteStatusEmailToAddresses: {0}",
                _windowsServiceSettings.PromoteStatusEmailToAddresses);
            var toAddresses = _windowsServiceSettings.PromoteStatusEmailToAddresses.Split(';');
            foreach (var address in toAddresses)
            {
                _log.DebugFormat("address: {0}", address);
                var emailAddress = emailAddresses.FirstOrDefault(x => x.ToLower() == address.ToLower());
                if (emailAddress != null)
                {
                    emailAddresses.Add(address);
                }
            }

            foreach (var emailAddress in emailAddresses)
            {
                _log.DebugFormat("emailAddress: {0}", emailAddress);
                if (!emailMessage.AddToRecipient(emailAddress))
                {
                    _log.ErrorFormat("invalid TO email address <{0}>", emailAddress);
                }
            }

            _log.DebugFormat("Subject: {0}", emailMessage.Subject);
            foreach (var toAddress in emailMessage.ToRecipients)
            {
                _log.DebugFormat("toAddress: {0}", toAddress);
            }

            _emailQueueService.QueueEmailMessage(emailMessage);
        }

        private string GetEmailMessageBody()
        {
            var resource = _promotionService.ResourceToPromote;
            var promoteRequest = _promotionService.PromoteRequest;

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
                .AppendLine(".warning { color: orange; } ")
                .AppendLine("</style></head>")
                .AppendLine("<body>")
                .AppendLine("<h1>R2v2 Promotion Status</h1>")
                .AppendFormat("<h2>Resource: {0}, {1}</h2>", resource.Isbn, resource.Title).AppendLine()
                .AppendFormat("<div class=\"status\">Status: <span class=\"{1}\">{0}</span></div>",
                    _promotionService.EmailMessageStatus(), _promotionService.EmailMessageStatus().ToLower())
                .AppendLine();

            if (promoteRequest.AddedByUser.UserId > 0)
            {
                emailBody.AppendFormat("<div>Added By: {0} {1}, <{2}></div>", promoteRequest.AddedByUser.UserNameFirst,
                        promoteRequest.AddedByUser.UserNameLast, promoteRequest.AddedByUser.UserEmailAddress)
                    .AppendLine();
            }

            if (promoteRequest.PromotedByUser.UserId > 0)
            {
                emailBody.AppendFormat("<div>Initiated By: {0} {1}, <{2}></div>",
                        promoteRequest.PromotedByUser.UserNameFirst, promoteRequest.PromotedByUser.UserNameLast,
                        promoteRequest.PromotedByUser.UserEmailAddress)
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

            if (_promotionService.OverlapResourceIsbns != null && _promotionService.OverlapResourceIsbns.Any())
            {
                emailBody.AppendLine(
                    "<div>This does not affect the promotion but the ISBN(s) for this resource conflict with the following resource(s):</div>");
                foreach (var overlapResourceIsbn in _promotionService.OverlapResourceIsbns)
                {
                    emailBody.AppendLine(
                        $"<div>{overlapResourceIsbn.ToDebugString().Replace("ResourceToPromote", "Resource")}</div>");
                }
            }

            if (_promotionService.Successful)
            {
                emailBody.AppendLine(
                    "<div>Resource is now available for browse only.  Search will not be available for this resource until the content is transformed and indexed in (1 to 3 hours).</div>");
            }

            emailBody.AppendLine("</body></html>");

            return emailBody.ToString();
        }

        private ResourcePromoteQueue SetResourcePromoteQueueStatus(ResourcePromoteQueue resourcePromoteQueue,
            ResourcePromoteStatus status)
        {
            resourcePromoteQueue.PromoteStatus = status;
            return UpdateResourcePromoteQueue(resourcePromoteQueue);
        }

        private ResourcePromoteQueue SetResourcePromoteQueueStatus(int resourceId, Guid batchKey,
            ResourcePromoteStatus status)
        {
            _log.DebugFormat("SetResourcePromoteQueueStatus(resourceId: {0}, batchKey: {1}, status: {2})", resourceId,
                batchKey, status);
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    _log.DebugFormat("uow.Session.IsOpen: {0}, uow.Session.IsConnected: {1}", uow.Session.IsOpen,
                        uow.Session.IsConnected);

                    var resourcePromoteQueue =
                        _resourcePromoteQueues.FirstOrDefault(x =>
                            x.ResourceId == resourceId && x.BatchKey == batchKey);

                    if (resourcePromoteQueue == null)
                    {
                        throw new Exception(
                            $"ResourcePromoteQueue could not be found, resourceId: {resourceId}, batchKey: {batchKey}");
                    }

                    resourcePromoteQueue.PromoteStatus = status;

                    uow.Update(resourcePromoteQueue);
                    transaction.Commit();
                    uow.Commit();
                    uow.Evict(resourcePromoteQueue);
                    return resourcePromoteQueue;
                }
            }
        }

        private ResourcePromoteQueue UpdateResourcePromoteQueue(ResourcePromoteQueue resourcePromoteQueue)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Update(resourcePromoteQueue);
                    transaction.Commit();
                    uow.Commit();
                    uow.Evict(resourcePromoteQueue);
                    return resourcePromoteQueue;
                }
            }
        }
    }
}