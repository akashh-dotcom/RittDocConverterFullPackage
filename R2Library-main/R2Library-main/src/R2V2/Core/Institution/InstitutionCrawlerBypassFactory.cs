#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Institution
{
    public class InstitutionCrawlerBypassFactory
    {
        private readonly IQueryable<InstitutionCrawlerBypass> _institutionCrawlerBypasses;
        private readonly ILog<InstitutionCrawlerBypassFactory> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public InstitutionCrawlerBypassFactory(
            ILog<InstitutionCrawlerBypassFactory> log
            , IQueryable<InstitutionCrawlerBypass> institutionCrawlerBypasses
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _institutionCrawlerBypasses = institutionCrawlerBypasses;
            _unitOfWorkProvider = unitOfWorkProvider;
        }


        public List<InstitutionCrawlerBypass> GetInstitutionCrawlerBypasses(int institutionId)
        {
            return _institutionCrawlerBypasses.Where(x => x.InstitutionId == institutionId).ToList();
        }

        public InstitutionCrawlerBypass GetInstitutionCrawlerBypass(int institutionId, string userAgent)
        {
            return _institutionCrawlerBypasses.FirstOrDefault(x =>
                x.InstitutionId == institutionId && userAgent.Contains(x.UserAgent));
        }

        public InstitutionCrawlerBypass GetInstitutionCrawlerBypass(long ipNumber, string userAgent)
        {
            return _institutionCrawlerBypasses.FirstOrDefault(x =>
                x.IpNumber == ipNumber && userAgent.Contains(x.UserAgent));
        }

        public InstitutionCrawlerBypass GetInstitutionCrawlerBypass(int institutionCrawlerBypassId)
        {
            return _institutionCrawlerBypasses.FirstOrDefault(x => x.Id == institutionCrawlerBypassId);
        }

        public string SaveInstitutionCrawlerBypass(InstitutionCrawlerBypass institutionCrawlerBypass)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var dbInstitutionCrawlerBypass =
                            GetInstitutionCrawlerBypass(institutionCrawlerBypass.InstitutionId,
                                institutionCrawlerBypass.UserAgent);
                        if (dbInstitutionCrawlerBypass != null)
                        {
                            if (dbInstitutionCrawlerBypass.Id > 0 &&
                                dbInstitutionCrawlerBypass.Id != institutionCrawlerBypass.Id)
                            {
                                return dbInstitutionCrawlerBypass.Institution.AccountNumber;
                            }
                        }

                        uow.SaveOrUpdate(institutionCrawlerBypass);

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

        public bool DeletInstitutionCrawlerBypass(int institutionCrawlerBypassId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var institutionCrawlerBypass = GetInstitutionCrawlerBypass(institutionCrawlerBypassId);
                        uow.Delete(institutionCrawlerBypass);

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