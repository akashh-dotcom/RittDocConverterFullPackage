#region

using System;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Email
{
    public class EmailResultDataService
    {
        private readonly ILog<ReportService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public EmailResultDataService(ILog<ReportService> log
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public void InsertEmailResult(User user, EmailType emailType, string description, string databaseName)
        {
            var sql = new StringBuilder()
                .AppendFormat(
                    "insert into {0}..EmailResult (institutionId, userId, dateEmailSent, emailTypeId, description) ",
                    databaseName)
                .Append("values (:institutionId, :userId, :dateEmailSent, :emailTypeId, :description)").ToString();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("institutionId", user.InstitutionId);
                query.SetParameter("userId", user.Id);
                query.SetParameter("dateEmailSent", DateTime.Now);
                query.SetParameter("emailTypeId", (int)emailType);
                query.SetParameter("description", description);

                query.List();
            }
        }
    }

    public enum EmailType
    {
        AnnualFee = 1
    }
}