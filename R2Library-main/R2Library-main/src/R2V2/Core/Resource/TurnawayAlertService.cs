#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class TurnawayAlertService
    {
        private readonly ILog<TurnawayAlertService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public TurnawayAlertService(ILog<TurnawayAlertService> log, IUnitOfWorkProvider unitOfWorkProvider)
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public Dictionary<int, int> GetConcurrentTurnawayResourceIdsAndCount(DateTime? turnawayStartDate,
            int institutionId)
        {
            if (turnawayStartDate == null || institutionId == 0)
            {
                return null;
            }

            _log.DebugFormat("GetConcurrentTurnawayResourceIdsAndCount -- turnawayStartDate: {0} institutionId: {1}",
                turnawayStartDate.Value, institutionId);

            var sql = new StringBuilder()
                .Append(" select resourceId, sum(concurrentTurnawayCount)")
                .Append(" from vDailyInstitutionResourceStatisticsCount")
                .Append(
                    " where institutionId = :institutionId and institutionResourceStatisticsDate >= :startDate and concurrentTurnawayCount > 0")
                .Append(" group by resourceId")
                .Append(" order by 2")
                .ToString();
            var turnawayResourceIdsAndCount = new Dictionary<int, int>();
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                query.SetParameter("startDate", turnawayStartDate.Value);
                query.SetParameter("institutionId", institutionId);

                var results = query.List();


                foreach (var result in results.Cast<object[]>().Where(result => result.Count() == 2))
                {
                    turnawayResourceIdsAndCount.Add((int)result[0], (int)result[1]);
                }
            }

            return turnawayResourceIdsAndCount.Any() ? turnawayResourceIdsAndCount : null;
        }

        public int GetConcurrentTurnawayResourceCount(DateTime? turnawayStartDate, int institutionId)
        {
            if (institutionId == 0)
            {
                return 0;
            }

            if (turnawayStartDate == null)
            {
                turnawayStartDate = DateTime.Now.AddDays(-30);
            }

            _log.DebugFormat("GetConcurrentTurnawayResourceIdsAndCount -- turnawayStartDate: {0} institutionId: {1}",
                turnawayStartDate.Value, institutionId);

            var sql = new StringBuilder()
                .Append(" select dirsc.resourceId")
                .Append(" from vDailyInstitutionResourceStatisticsCount dirsc ")
                .Append(" join tResource r on dirsc.resourceId = r.iResourceId ")
                .Append(
                    " where dirsc.institutionId = :institutionId and dirsc.institutionResourceStatisticsDate >= :startDate ")
                .Append(" and dirsc.concurrentTurnawayCount > 0 and r.iResourceStatusId = 6 and r.NotSaleable = 0 ")
                .Append(" group by dirsc.resourceId")
                .ToString();
            IList results;
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                query.SetParameter("startDate", turnawayStartDate.Value);
                query.SetParameter("institutionId", institutionId);

                results = query.List();
            }

            return results == null ? 0 : results.Count;
        }

        public void UpdateUserConcurrentTurnawayDate(int userId, DateTime turnawayAlertDate)
        {
            var sql = new StringBuilder()
                .Append(
                    "update tUser set dtConcurrentTurnawayAlert = :turnawayDate, dtLastUpdate = getdate(), vchUpdaterId = 'ConcurrentTurnawayAlert Update' where iUserId = :userId ")
                .ToString();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                query.SetParameter("userId", userId);
                query.SetParameter("turnawayDate", turnawayAlertDate);

                // ReSharper disable once UnusedVariable
                var results = query.List();
            }
        }
    }
}