#region

using System;
using System.Web.Routing;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Routing
{
    public interface IOnExecuteOnProcessRequest
    {
        void Before(RequestContext requestContext);
        void After(RequestContext requestContext);
    }

    public class DisposeOfLocalStorageItems : IOnExecuteOnProcessRequest
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public DisposeOfLocalStorageItems(Func<ILocalStorageService> localStorageServiceFactory,
            Func<IUnitOfWorkProvider> unitOfWorkProvider)
        {
            _localStorageService = localStorageServiceFactory();
            _unitOfWorkProvider = unitOfWorkProvider();
        }

        public void Before(RequestContext requestContext)
        {
        }

        public void After(RequestContext requestContext)
        {
            _unitOfWorkProvider.Clear();
            _unitOfWorkProvider.Close();
            _localStorageService.Dispose();
        }
    }
}