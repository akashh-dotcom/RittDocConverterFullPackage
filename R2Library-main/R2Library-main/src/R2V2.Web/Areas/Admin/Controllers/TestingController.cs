#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Email;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Testing;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class TestingController : R2AdminBaseController
    {
        private readonly EmailMessageSendQueueService _emailMessageSendQueueService;
        private readonly IEmailSettings _emailSettings;
        private readonly IMessageQueueSettings _messageQueueSettings;

        public TestingController(IAuthenticationContext authenticationContext, IEmailSettings emailSettings,
            EmailMessageSendQueueService emailMessageSendQueueService,
            IMessageQueueSettings messageQueueSettings) : base(authenticationContext)
        {
            _emailSettings = emailSettings;
            _emailMessageSendQueueService = emailMessageSendQueueService;
            _messageQueueSettings = messageQueueSettings;
        }

        public ActionResult Index()
        {
            var model = new IndexViewModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult Email()
        {
            var model = new EmailViewModel
            {
                Subject = "R2 Library Test Email",
                ToAddress = CurrentUser.Email,
                FromAddress = _emailSettings.DefaultFromAddress,
                FromName = _emailSettings.DefaultFromName,
                ReplyName = _emailSettings.DefaultReplyToName,
                ReplyToAddress = _emailSettings.DefaultReplyToAddress,
                Body = "This is a test email",
                IsHtml = false
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Email(EmailViewModel model)
        {
            var emailMessage = new EmailMessage
            {
                IsHtml = model.IsHtml,
                Body = model.Body,
                Subject = model.Subject,
                ReplyToAddress = model.ReplyToAddress,
                ReplyToDisplayName = model.ReplyName,
                FromAddress = model.FromAddress,
                FromDisplayName = model.FromName
            };
            emailMessage.AddCcRecipient(model.ToAddress);

            model.SentSuccessfully = _emailMessageSendQueueService.WriteEmailMessageToMessageQueue(emailMessage);
            model.StatusMessage = model.SentSuccessfully
                ? $"Email message was successfully sent to the RabbitMQ message queue, '{_messageQueueSettings.EmailMessageQueueName}'"
                : "ERROR SENDING MESSAGE TO THE QUEUE!";

            return View(model);
        }
    }
}