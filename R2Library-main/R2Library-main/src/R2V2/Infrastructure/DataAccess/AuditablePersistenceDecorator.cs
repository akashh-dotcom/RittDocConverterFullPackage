#region

using System;
using R2V2.Contexts;
using R2V2.Core.SuperType;
using R2V2.Extensions;

#endregion

namespace R2V2.Infrastructure.DataAccess
{
    public class AuditablePersistenceDecorator : IPersistInstanceDecorator
    {
        private readonly IAuthenticationContext _authenticationContext;

        public AuditablePersistenceDecorator(IAuthenticationContext authenticationContext)
        {
            _authenticationContext = authenticationContext;
        }

        public void Execute<T>(T instance)
        {
            if (!(instance is IAuditable))
            {
                return;
            }

            Guard.IsTrue<InvalidOperationException>(_authenticationContext.IsAuthenticated,
                "User must be authenticated to create data");

            var auditable = instance.As<IAuditable>();
            var time = DateTime.Now;

            //AuthenticatedInstitution authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            //string by = (authenticatedInstitution != null) ? authenticatedInstitution.AuditId : "error";
            var by = _authenticationContext.AuditId;

            if (auditable.CreationDate.IsNull())
            {
                auditable.CreationDate = time;
                auditable.CreatedBy = by;
            }
            else
            {
                auditable.LastUpdated = time;
                auditable.UpdatedBy = by;
            }
        }
    }
}