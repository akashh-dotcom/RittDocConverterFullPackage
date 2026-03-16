#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Institution
{
    public class WebServiceAuthenticationFactory
    {
        private readonly ILog<WebServiceAuthenticationFactory> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<WebServiceAuthentication> _webServiceAuthentications;

        public WebServiceAuthenticationFactory(
            ILog<WebServiceAuthenticationFactory> log
            , IQueryable<WebServiceAuthentication> webServiceAuthentications
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _webServiceAuthentications = webServiceAuthentications;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public List<WebServiceAuthentication> GetWebServiceAuthentications(int institutionId)
        {
            return _webServiceAuthentications.Where(x => x.InstitutionId == institutionId).ToList();
        }

        public WebServiceAuthentication GetWebServiceAuthentication(long ipNumber)
        {
            return _webServiceAuthentications.FirstOrDefault(x => x.IpNumber == ipNumber);
        }

        public IEnumerable<WebServiceAuthentication> GetWebServiceAuthentications(long ipNumber)
        {
            return _webServiceAuthentications.Where(x => x.IpNumber == ipNumber);
        }

        public WebServiceAuthentication GetWebServiceAuthentication(int trustedAuthId)
        {
            return _webServiceAuthentications.FirstOrDefault(x => x.Id == trustedAuthId);
        }

        public string SaveWebServiceAuthentication(WebServiceAuthentication webServiceAuthentication)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.SaveOrUpdate(webServiceAuthentication);

                        uow.Commit();
                        transaction.Commit();
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return "fail";
        }

        public bool DeleteWebServiceAuthentication(int institutionTrustedAuthenticationId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var webServiceAuthentication = GetWebServiceAuthentication(institutionTrustedAuthenticationId);
                        uow.Delete(webServiceAuthentication);

                        uow.Commit();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }
    }
}