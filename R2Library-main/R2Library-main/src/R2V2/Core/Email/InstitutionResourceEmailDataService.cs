#region

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Core.RequestLogger;
using R2V2.Extensions;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Email
{
    public class InstitutionResourceEmailDataService
    {
        private readonly IQueryable<InstitutionResourceEmail> _institutionResourceEmails;
        private readonly ILog<InstitutionResourceEmailDataService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public InstitutionResourceEmailDataService(ILog<InstitutionResourceEmailDataService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<InstitutionResourceEmail> institutionResourceEmails
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutionResourceEmails = institutionResourceEmails;
        }

        public void Save(EmailMessage emailMessage, IEmailData emailData, int resourceId, string section, bool queued,
            RequestData requestData)
        {
            var institutionResourceEmail = new InstitutionResourceEmail
            {
                ChapterSectionId = section.Truncate(20),
                Comment = emailData.Comments.Truncate(2000),
                CreationDate = DateTime.Now,
                InstitutionId = requestData.InstitutionId,
                Queued = queued,
                RequestId = requestData.RequestId,
                ResourceId = resourceId,
                SessionId = requestData.Session.SessionId,
                Subject = emailMessage.Subject.Truncate(255),
                UserEmailAddress = string.IsNullOrEmpty(emailData.From) ? string.Empty : emailData.From.Truncate(100),
                UserId = requestData.UserId
            };

            foreach (var toRecipient in emailMessage.ToRecipients)
            {
                institutionResourceEmail.AddRecipient(new InstitutionResourceEmailRecipient
                    { AddressType = 'T', EmailAddress = toRecipient.Truncate(100) });
            }

            foreach (var ccRecipient in emailMessage.CcRecipients)
            {
                institutionResourceEmail.AddRecipient(new InstitutionResourceEmailRecipient
                    { AddressType = 'C', EmailAddress = ccRecipient.Truncate(100) });
            }

            foreach (var bccRecipient in emailMessage.BccRecipients)
            {
                institutionResourceEmail.AddRecipient(new InstitutionResourceEmailRecipient
                    { AddressType = 'B', EmailAddress = bccRecipient.Truncate(100) });
            }

            Save(institutionResourceEmail);
        }

        public void Save(InstitutionResourceEmail institutionResourceEmail)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.SaveOrUpdate(institutionResourceEmail);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                // TODO - swallow exception for testing
                _log.Error(ex.Message, ex);
            }
        }

        public List<InstitutionResourceEmail> GetInstitutionResourceEmail(int institutionId, int resourceId,
            int resourceEmailLockDurationInHours, string sessionId)
        {
            var emails = _institutionResourceEmails
                .Where(x => x.InstitutionId == institutionId && x.ResourceId == resourceId)
                .Where(x => x.CreationDate > DateTime.Now.AddHours(-1 * resourceEmailLockDurationInHours))
                .FetchMany(x => x.Recipients)
                .ToList();
            return emails;
        }

        public List<EmailAddressCount> GetEmailAddressCount(int institutionId, int resourceId,
            int resourceEmailLockDurationInHours)
        {
            var sql =
                @"select irer.vchEmailAddress as [EmailAddress], ire.vchSessionId as [SessionId], count(*) as [Count]
                from tInstitutionResourceEmailRecipient irer
                 join tInstitutionResourceEmail ire on irer.iInstitutionResourceEmailId = ire.iInstitutionResourceEmailId
                where ire.iInstitutionId = :institutionId and ire.iResourceId = :resourceId and ire.dtCreationDate > :creationDateLimit
                group by irer.vchEmailAddress, ire.vchSessionId
                order by ire.vchSessionId, count(*) desc, irer.vchEmailAddress";

            var emails = new List<EmailAddressCount>();
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                query.SetParameter("institutionId", institutionId);
                query.SetParameter("resourceId", resourceId);
                query.SetParameter("creationDateLimit", DateTime.Now.AddHours(-1 * resourceEmailLockDurationInHours));

                var results = query.List();

                foreach (object[] result in results)
                {
                    var emailAddressCount = new EmailAddressCount();
                    emailAddressCount.EmailAddress = result[0] == null ? string.Empty : (string)result[0];
                    emailAddressCount.SessionId = result[1] == null ? string.Empty : (string)result[1];
                    emailAddressCount.Count = result[2] == null ? 0 : (int)result[2];
                    emails.Add(emailAddressCount);
                }
            }

            return emails;
        }
    }

    public class EmailAddressCount
    {
        public string EmailAddress { get; set; }
        public string SessionId { get; set; }
        public int Count { get; set; }
    }
}