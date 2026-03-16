#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.WindowsService.Threads.AutomatedCart
{
    public class AutomatedCartService
    {
        private readonly AutomatedCartFactory _automatedCartFactory;
        private readonly AutomatedCartInstitutionEmailBuildService _automatedCartInstitutionEmailBuildService;
        private readonly AutomatedCartSummaryEmailBuildService _automatedCartSummaryEmailBuildService;
        private readonly EmailMessageSendQueueService _emailMessageSendQueueService;
        private readonly IQueryable<IInstitution> _institutions;
        private readonly ILog<AutomatedCartService> _log;
        private readonly IQueryable<User> _users;

        public AutomatedCartService(
            ILog<AutomatedCartService> log
            , AutomatedCartFactory automatedCartFactory
            , IQueryable<User> users
            , IQueryable<IInstitution> institutions
            , AutomatedCartInstitutionEmailBuildService automatedCartInstitutionEmailBuildService
            , AutomatedCartSummaryEmailBuildService automatedCartSummaryEmailBuildService
            , EmailMessageSendQueueService emailMessageSendQueueService
        )
        {
            _log = log;
            _automatedCartFactory = automatedCartFactory;
            _users = users;
            _institutions = institutions;
            _automatedCartInstitutionEmailBuildService = automatedCartInstitutionEmailBuildService;
            _automatedCartSummaryEmailBuildService = automatedCartSummaryEmailBuildService;
            _emailMessageSendQueueService = emailMessageSendQueueService;
        }

        public bool ProcessAutomatedCartMessage(AutomatedCartMessage automatedCartMessage)
        {
            try
            {
                _log.InfoFormat("{0}", automatedCartMessage.ToJsonString());

                var automatedCart = _automatedCartFactory.GetAutomatedCart(automatedCartMessage.AutomatedCartId);
                if (automatedCart == null)
                {
                    _log.Error(
                        $"Automated Cart could not be found AutomatedCartId: {automatedCartMessage.AutomatedCartId}");
                    return true; //Message could not be found so stop processing the message.
                }

                var failCounter = 0;
                var automatedCartInstitutions =
                    _automatedCartFactory.GetAutoCartInstitutionsForProcessing(automatedCartMessage.AutomatedCartId);
                if (!automatedCartInstitutions.Any())
                {
                    _log.Error(
                        $"Automated Cart No institutions to process  AutomatedCartId: {automatedCartMessage.AutomatedCartId}");
                    //return true; //Message could not be found so stop processing the message.
                }
                else
                {
                    var institutionsIds = automatedCartInstitutions.Select(x => x.InstitutionId).ToArray();

                    var institutions = _institutions.Where(y => institutionsIds.Contains(y.Id)).ToList();

                    var forceResourceCacheReload = true;

                    foreach (var institution in institutions)
                    {
                        var cartId = _automatedCartFactory.CreateAutomatedCartForInstitution(automatedCart, institution,
                            forceResourceCacheReload);
                        forceResourceCacheReload = false;

                        if (cartId == 0)
                        {
                            failCounter++;
                            //Not sure we should continue if one fails.
                            if (failCounter > 3)
                            {
                                break;
                            }
                        }

                        if (cartId > 0)
                        {
                            var emailsSent = SendAutomatedCartEmailsForInstitution(automatedCart, institution, cartId);
                            _automatedCartFactory.UpdateEmailsSent(automatedCart.Id, institution.Id, emailsSent);
                        }
                    }
                }

                if (failCounter == 0)
                {
                    var automatedCartInstitutionSummaries =
                        _automatedCartFactory.GetAutomatedCartInstitutionSummaries(automatedCart.Id);
                    SendAutomatedCartSummary(automatedCart, automatedCartInstitutionSummaries);

                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendLine("Automated Cart Message:").Append("\t").AppendLine(automatedCartMessage.ToJsonString());
                msg.Append(ex.Message);
                _log.Error(msg, ex);
            }

            return false;
        }

        private List<AutomatedCartInstitutionSummary> GetInstitutionSummaries(int automatedCartId)
        {
            var automatedCartInstitutionSummaries = new List<AutomatedCartInstitutionSummary>();

            return automatedCartInstitutionSummaries;
        }

        private int SendAutomatedCartEmailsForInstitution(DbAutomatedCart automatedCart, IInstitution institution,
            int cartId)
        {
            var users = _users
                .Where(x => x.InstitutionId == institution.Id &&
                            (!x.ExpirationDate.HasValue || x.ExpirationDate.Value > DateTime.Now))
                .HasOptionSelected(UserOptionCode.AutomatedShoppingCart);
            var mainInstitutionAdmin = _users.FirstOrDefault(x =>
                x.InstitutionId == institution.Id && x.UserName == institution.AccountNumber);
            var usersForEmail = new List<User>();
            if (users.Any())
            {
                usersForEmail.AddRange(users.ToList());
            }

            if (!usersForEmail.Contains(mainInstitutionAdmin))
            {
                usersForEmail.Add(mainInstitutionAdmin);
            }

            var emailCounter = 0;
            foreach (var user in usersForEmail)
            {
                var emailMessage =
                    _automatedCartInstitutionEmailBuildService.BuildAutomatedCartEmail(automatedCart, cartId, user);

                emailCounter += _emailMessageSendQueueService.WriteEmailMessageToMessageQueue(emailMessage) ? 1 : 0;
            }

            return emailCounter;
        }

        private void SendAutomatedCartSummary(DbAutomatedCart automatedCart,
            List<AutomatedCartInstitutionSummary> automatedCartInstitutionSummaries)
        {
            var emailList = new List<string>();


            var createdBy = automatedCart.CreatedBy;

            //user id: 2907, [Ken]
            var userIdStartString = createdBy.Replace("user id: ", "");
            var indexOfIdEnd = userIdStartString.IndexOf(",", StringComparison.InvariantCultureIgnoreCase);
            var userIdString = userIdStartString.Substring(0, indexOfIdEnd);
            int.TryParse(userIdString, out var userId);
            var creator = _users.FirstOrDefault(x => x.Id == userId);
            if (creator != null)
            {
                emailList.Add(creator.Email);
            }

            emailList.Add("customerservice@r2library.com");

            var emailMessage = _automatedCartSummaryEmailBuildService.BuildAutomatedCartSummaryEmail(automatedCart,
                automatedCartInstitutionSummaries, emailList.ToArray());
            _emailMessageSendQueueService.WriteEmailMessageToMessageQueue(emailMessage);
        }
    }
}