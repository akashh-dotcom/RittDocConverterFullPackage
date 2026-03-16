#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Audit;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Institution
{
    public class IpAddressRangeFactory
    {
        private readonly AuditService _auditService;
        private readonly IQueryable<IpAddressRange> _ipAddressRanges;
        private readonly ILog<IpAddressRangeFactory> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public IpAddressRangeFactory(
            ILog<IpAddressRangeFactory> log
            , IQueryable<IpAddressRange> ipAddressRanges
            , IUnitOfWorkProvider unitOfWorkProvider
            , AuditService auditService
        )
        {
            _log = log;
            _ipAddressRanges = ipAddressRanges;
            _unitOfWorkProvider = unitOfWorkProvider;
            _auditService = auditService;
        }

        public List<IpAddressRange> GetInstitutionIpRanges(int institutionId)
        {
            return _ipAddressRanges.Where(x => x.Institution.Id == institutionId && x.RecordStatus)
                .OrderBy(x => x.OctetA)
                .ThenBy(x => x.OctetB)
                .ThenBy(x => x.OctetCStart)
                .ThenBy(x => x.OctetDStart)
                .ToList();
        }

        public IEnumerable<IpAddressRange> GetInstitutionIpRanges(int institutionId, IEnumerable<int> ipAddressRangeIds)
        {
            return _ipAddressRanges.Where(x =>
                x.Institution.Id == institutionId && x.RecordStatus && ipAddressRangeIds.Contains(x.Id));
        }


        public void SavePairIpaddressRange(PairedIpAddressRanges pairedIpAddressRanges)
        {
            var ipAddressToSave = pairedIpAddressRanges.MergedIpAddressRange;
            var saved = false;
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var ipRangesToDelete =
                            _ipAddressRanges.Where(x => x.IpNumberStart >= ipAddressToSave.IpNumberStart &&
                                                        x.IpNumberEnd <= ipAddressToSave.IpNumberEnd &&
                                                        x.InstitutionId == ipAddressToSave.InstitutionId);

                        foreach (var ipAddressRange in ipRangesToDelete)
                        {
                            uow.Delete(ipAddressRange);
                        }

                        uow.Save(ipAddressToSave);
                        uow.Commit();
                        transaction.Commit();
                        saved = true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            if (saved)
            {
                _auditService.LogInstitutionAudit(ipAddressToSave.InstitutionId, InstitutionAuditType.IpAddressInsert,
                    ipAddressToSave.ToAuditString());
            }
        }

        public void SaveIpAddressRange(IpAddressRange ipAddressRange, string auditMessage)
        {
            if (ipAddressRange.Id == 0) // Save
            {
                _log.InfoFormat("Saving a new IP address");

                SaveIpAddressRange(ipAddressRange);

                _auditService.LogInstitutionAudit(ipAddressRange.InstitutionId, InstitutionAuditType.IpAddressInsert,
                    auditMessage);
            }
            else
            {
                var dbIpAddress = _ipAddressRanges.FirstOrDefault(x => x.Id == ipAddressRange.Id);

                if (dbIpAddress != null)
                {
                    _log.Info(auditMessage);
                    dbIpAddress = UpdateIpAddress(dbIpAddress, ipAddressRange);

                    SaveIpAddressRange(dbIpAddress);

                    _auditService.LogInstitutionAudit(ipAddressRange.InstitutionId,
                        InstitutionAuditType.IpAddressUpdate, auditMessage);
                }
            }
        }

        public IpAddressRange GetInstitutionIpRange(int institutionId, int ipAddressRangeId)
        {
            using (_unitOfWorkProvider.Start())
            {
                return _ipAddressRanges.FirstOrDefault(x =>
                    x.Institution.Id == institutionId && x.RecordStatus && x.Id == ipAddressRangeId);
            }
        }

        private void SaveIpAddressRange(IpAddressRange ipAddressRange)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(ipAddressRange);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public bool Delete(int institutionId, int id)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var updateIpAddress =
                            _ipAddressRanges.FirstOrDefault(x => x.Id == id && x.Institution.Id == institutionId);
                        uow.Delete(updateIpAddress);
                        uow.Commit();
                        transaction.Commit();
                        if (updateIpAddress != null)
                        {
                            _auditService.LogInstitutionAudit(updateIpAddress.InstitutionId,
                                InstitutionAuditType.IpAddressDeleted, updateIpAddress.ToAuditString());
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<string> BulkDelete(int institutionId, string ipAddressRangeIdsString)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var ipAddressRangeAuditStrings = new List<string>();
                        var ipAddressRangeIds = ipAddressRangeIdsString.Split(',')
                            .Where(x => !string.IsNullOrWhiteSpace(x)).Select(int.Parse);

                        var ipAddressRanges = _ipAddressRanges.Where(x =>
                            x.Institution.Id == institutionId && x.RecordStatus && ipAddressRangeIds.Contains(x.Id));

                        var formattedIpAddressRanges = new List<string>(ipAddressRanges.Select(x =>
                            $"{x.GetIpAddressRangeStart()} - {x.GetIpAddressRangeEnd()}"));

                        foreach (var coreIpAddressRange in ipAddressRanges)
                        {
                            uow.Delete(coreIpAddressRange);
                            ipAddressRangeAuditStrings.Add(coreIpAddressRange.ToAuditString());
                        }

                        uow.Commit();
                        transaction.Commit();

                        foreach (var ipAddressRangeAuditString in ipAddressRangeAuditStrings)
                        {
                            _auditService.LogInstitutionAudit(institutionId, InstitutionAuditType.IpAddressDeleted,
                                ipAddressRangeAuditString);
                        }

                        return formattedIpAddressRanges;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                        return null;
                    }
                }
            }
        }

        private IpAddressRange UpdateIpAddress(IpAddressRange databaseIpRange, IpAddressRange modelIpRange)
        {
            databaseIpRange.OctetA = modelIpRange.OctetA;
            databaseIpRange.OctetB = modelIpRange.OctetB;
            databaseIpRange.OctetCStart = modelIpRange.OctetCStart;
            databaseIpRange.OctetCEnd = modelIpRange.OctetCEnd;
            databaseIpRange.OctetDStart = modelIpRange.OctetDStart;
            databaseIpRange.OctetDEnd = modelIpRange.OctetDEnd;
            databaseIpRange.Description = modelIpRange.Description;
            databaseIpRange.PopulateDecimals();

            return databaseIpRange;
        }

        public List<IpAddressRange> GetOverLappingIpRanges(IpAddressRange ipAddressRange)
        {
            var dStart = ipAddressRange.IpNumberStart;
            var dEnd = ipAddressRange.IpNumberEnd;

            var sql = new StringBuilder()
                .Append(" Select ip.iIpAddressId  ")
                .Append(" from tIpAddressRange ip ")
                .Append(
                    " join tInstitution i on ip.iInstitutionId = i.iInstitutionId and ((i.iInstitutionAcctStatusId = 1) ")
                .Append(
                    "                            or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > GetDate()) ")
                .AppendFormat("                            or (i.iInstitutionId = {0})) ", ipAddressRange.InstitutionId)
                .Append(" where i.tiRecordStatus = 1 and ip.tiRecordStatus = 1 ")
                .AppendFormat(" and (ip.iIpAddressId <> {0}) ", ipAddressRange.Id)
                .Append(" and (  ")
                .Append(SqlIpAddressEqual(dStart.ToString(), dEnd.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressStartBetween(dStart.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressEndBetween(dEnd.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressDbStartBetween(dStart.ToString(), dEnd.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressDbEndBetween(dStart.ToString(), dEnd.ToString()))
                .Append(" ) ")
                .ToString();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                var results = query.List();
                var ipRangeIds = results.Cast<object>().Where(result => result != null).Cast<int>().ToList();
                return _ipAddressRanges.Where(x => ipRangeIds.Contains(x.Id)).ToList();
            }
        }

        public List<IpAddressRange> GetOverLappingIpRanges2(IpAddressRange ipAddressRange, IInstitution institution)
        {
            var dStart = ipAddressRange.IpNumberStart;
            var dEnd = ipAddressRange.IpNumberEnd;

            var sql = new StringBuilder()
                .Append(" Select ip.iIpAddressId  ")
                .Append(" from tIpAddressRange ip ")
                .Append(
                    " join tInstitution i on ip.iInstitutionId = i.iInstitutionId and ((i.iInstitutionAcctStatusId = 1) ")
                .Append(
                    "                            or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > GetDate()) ")
                .AppendFormat("                            or (i.iInstitutionId = {0})) ", ipAddressRange.InstitutionId)
                .Append(" where i.tiRecordStatus = 1 and ip.tiRecordStatus = 1 ")
                .AppendFormat(" and (ip.iIpAddressId <> {0}) ", ipAddressRange.Id)
                .Append(" and (  ")
                .Append(SqlIpAddressEqual(dStart.ToString(), dEnd.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressStartBetween(dStart.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressEndBetween(dEnd.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressDbStartBetween(dStart.ToString(), dEnd.ToString()))
                .Append(" or ")
                .Append(SqlIpAddressDbEndBetween(dStart.ToString(), dEnd.ToString()))
                .Append(" ) ")
                .ToString();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                var results = query.List();
                var ipRangeIds = results.Cast<object>().Where(result => result != null).Cast<int>().ToList();

                var overlappingRanges = _ipAddressRanges.Where(x => ipRangeIds.Contains(x.Id));

                if (institution.EnableIpPlus)
                {
                    overlappingRanges = overlappingRanges.Where(x => !x.Institution.EnableIpPlus);
                }

                return overlappingRanges.ToList();
            }
        }

        public List<IpAddressRange> GetOverLappingIpRanges(int institutionId)
        {
            var sql = new StringBuilder()
                .Append("Select ip.iIpAddressId from tIpAddressRange ip ")
                .Append(
                    "join tInstitution i on ip.iInstitutionId = i.iInstitutionId and ((i.iInstitutionAcctStatusId = 1) ")
                .Append(
                    "                            or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > GetDate())) ")
                .AppendFormat(
                    "join (select * from tIpAddressRange where tiRecordStatus = 1 and iInstitutionId = {0}) as oip on ",
                    institutionId)
                .Append("( ")
                .Append(SqlIpAddressEqual("oip.iDecimalStart", "oip.iDecimalEnd"))
                .Append(" or ")
                .Append(SqlIpAddressStartBetween("oip.iDecimalStart"))
                .Append(" or ")
                .Append(SqlIpAddressEndBetween("oip.iDecimalEnd"))
                .Append(" or ")
                .Append(SqlIpAddressDbStartBetween("oip.iDecimalStart", "oip.iDecimalEnd"))
                .Append(" or ")
                .Append(SqlIpAddressDbEndBetween("oip.iDecimalStart", "oip.iDecimalEnd"))
                .Append(" ) ")
                .Append("where i.tiRecordStatus = 1 and ip.tiRecordStatus = 1 and oIP.iIpAddressId <> ip.iIpAddressId ")
                .Append("group by ip.iIpAddressId ")
                .ToString();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                var results = query.List();
                var ipRangeIds = results.Cast<object>().Where(result => result != null).Cast<int>().ToList();

                return _ipAddressRanges.Where(x => ipRangeIds.Contains(x.Id)).ToList();
            }
        }

        public List<PairedIpAddressRanges> GetConflictingIpAddressRanges(int institutionId,
            List<IpAddressRange> ipAddressRanges)
        {
            var sql = @"
select ip.iIpAddressId as IpAddressRangeId1, ip2.iIpAddressId as IpAddressRangeId2
from tIpAddressRange ip
join tIpAddressRange ip2 on ip.iDecimalStart between ip2.iDecimalStart and ip2.iDecimalEnd and ip2.tiRecordStatus = 1
join tInstitution i on ip2.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.iInstitutionId <> :InstitutionId
where ip.tiRecordStatus = 1
and ip.iInstitutionId = :InstitutionId
and ip.iIpAddressId <> ip2.iIpAddressId
and ((i.iInstitutionAcctStatusId = 1) or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > getdate()))
union
select ip.iIpAddressId as IpAddressRangeId1, ip2.iIpAddressId as IpAddressRangeId2
from tIpAddressRange ip
join tIpAddressRange ip2 on ip.iDecimalEnd between ip2.iDecimalStart and ip2.iDecimalEnd and ip2.tiRecordStatus = 1
join tInstitution i on ip2.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.iInstitutionId <> :InstitutionId
where ip.tiRecordStatus = 1
and ip.iInstitutionId = :InstitutionId
and ip.iIpAddressId <> ip2.iIpAddressId
and ((i.iInstitutionAcctStatusId = 1) or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > getdate()))
union
select ip.iIpAddressId as IpAddressRangeId1, ip2.iIpAddressId as IpAddressRangeId2
from tIpAddressRange ip
join tIpAddressRange ip2 on ip2.iDecimalStart between ip.iDecimalStart and ip.iDecimalEnd and ip2.tiRecordStatus = 1
join tInstitution i on ip2.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.iInstitutionId <> :InstitutionId
where ip.tiRecordStatus = 1
and ip.iInstitutionId = :InstitutionId
and ip.iIpAddressId <> ip2.iIpAddressId
and ((i.iInstitutionAcctStatusId = 1) or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > getdate()))
union
select ip.iIpAddressId as IpAddressRangeId1, ip2.iIpAddressId as IpAddressRangeId2
from tIpAddressRange ip
join tIpAddressRange ip2 on ip2.iDecimalEnd between ip.iDecimalStart and ip.iDecimalEnd and ip2.tiRecordStatus = 1
join tInstitution i on ip2.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.iInstitutionId <> :InstitutionId
where ip.tiRecordStatus = 1
and ip.iInstitutionId = :InstitutionId
and ip.iIpAddressId <> ip2.iIpAddressId
and ((i.iInstitutionAcctStatusId = 1) or (i.iInstitutionAcctStatusId = 2 and i.dtTrialAcctEnd > getdate()))
            ";

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("InstitutionId", institutionId);

                var results = query.List();

                var pairedIpAddressRanges = new List<PairedIpAddressRanges>();
                foreach (object[] result in results)
                {
                    var item = new PairedIpAddressRanges
                    {
                        IpAddressRangeId1 = ConvertObjectToInt(result[0]),
                        IpAddressRangeId2 = ConvertObjectToInt(result[1])
                    };
                    if (!pairedIpAddressRanges.Contains(item))
                    {
                        pairedIpAddressRanges.Add(item);
                    }
                }

                var ipAddressRangeId2Ids = pairedIpAddressRanges.Select(x => x.IpAddressRangeId2).ToList();
                var ipRange2List = _ipAddressRanges.Where(x => ipAddressRangeId2Ids.Contains(x.Id)).ToList();

                foreach (var conflictingIpAddressRange in pairedIpAddressRanges)
                {
                    var ipAddressRange1 =
                        ipAddressRanges.FirstOrDefault(x => x.Id == conflictingIpAddressRange.IpAddressRangeId1);
                    var ipAddressRange2 =
                        ipRange2List.FirstOrDefault(x => x.Id == conflictingIpAddressRange.IpAddressRangeId2);
                    conflictingIpAddressRange.IpAddressRange1 = ipAddressRange1;
                    conflictingIpAddressRange.IpAddressRange2 = ipAddressRange2;
                    conflictingIpAddressRange.PopulateMergedIpAddressRange();
                }

                return pairedIpAddressRanges.ToList();
            }
        }

        private int ConvertObjectToInt(object value)
        {
            if (value == null)
            {
                return 0;
            }

            return (int)value;
        }

        /// <summary>
        ///     Matches IP ranges when the start and end decimals are equal
        /// </summary>
        private static string SqlIpAddressEqual(string startDecimal, string endDecimal)
        {
            return new StringBuilder()
                .AppendFormat("    ({0} = ip.iDecimalStart or {0} = ip.iDecimalEnd) ", startDecimal)
                .AppendFormat(" or ({0} = ip.iDecimalStart or {0} = ip.iDecimalEnd) ", endDecimal)
                .ToString();
        }

        /// <summary>
        ///     When the start decimal from the institution is between Dbs decimal start and decimal end
        /// </summary>
        private static string SqlIpAddressStartBetween(string startDecimal)
        {
            return new StringBuilder()
                .AppendFormat(" ({0} between ip.iDecimalStart and ip.iDecimalEnd) ", startDecimal)
                .ToString();
        }

        /// <summary>
        ///     When the end decimal from the institution is between Dbs decimal start and decimal end
        /// </summary>
        private static string SqlIpAddressEndBetween(string endDecimal)
        {
            return new StringBuilder()
                .AppendFormat(" ({0} between ip.iDecimalStart and ip.iDecimalEnd) ", endDecimal)
                .ToString();
        }

        /// <summary>
        ///     When the start decimal from the institution is between the institution's decimal start and decimal end
        /// </summary>
        private static string SqlIpAddressDbStartBetween(string startDecimal, string endDecimal)
        {
            return new StringBuilder()
                .AppendFormat(" (ip.iDecimalStart between {0} and {1}) ", startDecimal, endDecimal)
                .ToString();
        }

        /// <summary>
        ///     When the end decimal from the institution is between the institution's decimal start and decimal end
        /// </summary>
        private static string SqlIpAddressDbEndBetween(string startDecimal, string endDecimal)
        {
            return new StringBuilder()
                .AppendFormat(" (ip.iDecimalEnd between {0} and {1}) ", startDecimal, endDecimal)
                .ToString();
        }
    }
}