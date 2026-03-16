#region

using System;
using R2V2.Contexts;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Audit
{
    public class AuditService
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<AuditService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AuditService(
            IUnitOfWorkProvider unitOfWorkProvider
            , ILog<AuditService> log
            , IAuthenticationContext authenticationContext
        )
        {
            _unitOfWorkProvider = unitOfWorkProvider;
            _log = log;
            _authenticationContext = authenticationContext;
        }

        public void LogInstitutionAudit(int institutionId, InstitutionAuditType auditType, string eventDescription)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            var by = authenticatedInstitution != null ? authenticatedInstitution.AuditId : "error";


            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var institutionAudit = new InstitutionAudit
                        {
                            InstitutionAuditType = auditType,
                            InstitutionId = institutionId,
                            EventDescription = eventDescription,
                            CreatedBy = by,
                            CreationDate = DateTime.Now
                        };


                        _log.Debug(institutionAudit.ToDebugString());
                        uow.Save(institutionAudit);

                        uow.Commit();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(ex.Message, ex);
                        throw;
                    }
                }
            }
        }

        public void LogResourceAudit(int resourceId, ResourceAuditType auditType, string eventDescription)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            var by = authenticatedInstitution != null ? authenticatedInstitution.AuditId : "error";


            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var audit = new ResourceAudit
                        {
                            ResourceAuditType = auditType,
                            ResourceId = resourceId,
                            EventDescription = eventDescription,
                            CreatedBy = by,
                            CreationDate = DateTime.Now
                        };


                        _log.Debug(audit.ToDebugString());
                        uow.Save(audit);

                        uow.Commit();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(ex.Message, ex);
                        throw;
                    }
                }
            }
        }
    }
}