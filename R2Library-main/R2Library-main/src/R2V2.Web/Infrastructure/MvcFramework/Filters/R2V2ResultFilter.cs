#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Web.Infrastructure.Contexts;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public abstract class R2V2ResultFilter : IR2V2Filter, IResultFilter
    {
        protected readonly IAuthenticationContext AuthenticationContext;

        protected R2V2ResultFilter(IAuthenticationContext authenticationContext)
        {
            AuthenticationContext = authenticationContext;
        }

        protected int UserId
        {
            get
            {
                var authenticatedInstitution = AuthenticationContext.AuthenticatedInstitution;
                if (AuthenticationContext.IsAuthenticated && authenticatedInstitution != null &&
                    authenticatedInstitution.User != null)
                {
                    return authenticatedInstitution.User.Id;
                }

                return 0;
            }
        }

        public bool AllowMultiple => true;

        public virtual int Order => 100;

        public virtual FilterScope FilterScope => FilterScope.Action;

        public virtual bool CanProcess(ActionContext actionContext)
        {
            return true;
        }

        public virtual void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //Do Nothing
        }

        public virtual void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //Do Nothing
        }
    }
}