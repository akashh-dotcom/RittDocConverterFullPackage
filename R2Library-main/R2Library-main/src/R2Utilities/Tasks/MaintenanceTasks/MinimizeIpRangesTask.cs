#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class MinimizeIpRangesTask : TaskBase
    {
        private readonly IQueryable<IpAddressRange> _ipAddressRanges;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public MinimizeIpRangesTask(
            IQueryable<IpAddressRange> ipAddressRanges
            , IUnitOfWorkProvider unitOfWorkProvider
        )
            : base("MinimizeIpRangesTask", "-MinimizeIpRangesTask", "13", TaskGroup.ContentLoading,
                "Task to minimize or group institution IP ranges", true)
        {
            _ipAddressRanges = ipAddressRanges;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public override void Run()
        {
            TaskResult.Information =
                "This task will condense the number of IP ranges for institutions if the ranges are consecetive.";
            var step = new TaskResultStep { Name = "MinimizeIpRangesTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();
            var ipRangesMerged = 0;
            var ipRangesDeleted = 0;
            try
            {
                var sb = new StringBuilder();
                var results = new StringBuilder();
                var institutions = _ipAddressRanges.Select(x => x.Institution).Distinct().ToList();

                foreach (var institution in institutions)
                {
                    var ipRangesChanged = ProcessIpRanges(institution.Id);

                    if (ipRangesChanged.Any() && ipRangesChanged.Count > 0)
                    {
                        results.AppendFormat("<div>Institution: {0} has had {1} Ip Ranges changed</div>",
                            institution.Id, ipRangesChanged.Count);
                    }


                    if (ipRangesChanged.Any())
                    {
                        foreach (var pair in ipRangesChanged)
                        {
                            if (pair.Value)
                            {
                                ipRangesMerged++;
                            }
                            else
                            {
                                ipRangesDeleted++;
                            }

                            sb.AppendFormat("Institution {2} has had the following {0} : {1}",
                                    pair.Value ? "Updated" : "Deleted", pair.Key.ToAuditString(),
                                    pair.Key.InstitutionId)
                                .AppendLine();
                        }
                    }
                }

                Log.Info(sb.ToString());

                Log.InfoFormat("{0} IPs updated and {1} deleted", ipRangesMerged, ipRangesDeleted);

                step.Results = results.ToString();
                step.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        private Dictionary<IpAddressRange, bool> ProcessIpRanges(int institutionId)
        {
            IQueryable<IpAddressRange> ipRanges = _ipAddressRanges.Where(x => x.InstitutionId == institutionId)
                .OrderBy(x => x.IpNumberStart);

            var ipAddressRanges = new Dictionary<IpAddressRange, bool>();

            IpAddressRange lastIpAddressRange = null;
            var lastWasChanged = false;
            var saveLast = false;

            foreach (var ipAddressRange in ipRanges)
            {
                if (lastIpAddressRange == null)
                {
                    lastIpAddressRange = ipAddressRange;
                    continue;
                }

                if (ipAddressRange.IpNumberStart == lastIpAddressRange.IpNumberEnd + 1)
                {
                    lastIpAddressRange.OctetCEnd = ipAddressRange.OctetCEnd;
                    lastIpAddressRange.OctetDEnd = ipAddressRange.OctetDEnd;
                    lastIpAddressRange.IpNumberEnd = ipAddressRange.IpNumberEnd;
                    ipAddressRanges.Add(ipAddressRange, false);
                    lastWasChanged = true;

                    saveLast = true;
                }
                else
                {
                    if (lastWasChanged)
                    {
                        ipAddressRanges.Add(lastIpAddressRange, true);
                    }

                    lastWasChanged = false;
                    lastIpAddressRange = ipAddressRange;
                    saveLast = false;
                }
            }

            if (saveLast)
            {
                ipAddressRanges.Add(lastIpAddressRange, true);
            }

            SaveDeleteIpRanges(ipAddressRanges.OrderByDescending(x => x.Value));

            return ipAddressRanges;
        }

        private void SaveDeleteIpRanges(IEnumerable<KeyValuePair<IpAddressRange, bool>> ipRangesToSaveOrDelete)
        {
            foreach (var pair in ipRangesToSaveOrDelete)
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    try
                    {
                        var dbIpAddressRange = _ipAddressRanges.FirstOrDefault(x => x.Id == pair.Key.Id);
                        if (dbIpAddressRange != null)
                        {
                            if (pair.Value)
                            {
                                var sql = new StringBuilder()
                                    .Append("UPDATE tIpAddressRange ")
                                    .Append("   SET tiOctetCEnd = :tiOctetCEnd ")
                                    .Append("      ,tiOctetDEnd = :tiOctetDEnd ")
                                    .Append("      ,iDecimalEnd = :iDecimalEnd ")
                                    .Append("      ,vchUpdaterId = :vchUpdaterId ")
                                    .Append("      ,dtLastUpdate = :dtLastUpdate ")
                                    .Append(" WHERE iIpAddressId = :iIpAddressId ")
                                    .ToString();


                                var query = uow.Session.CreateSQLQuery(sql);
                                query.SetParameter("tiOctetCEnd", pair.Key.OctetCEnd);
                                query.SetParameter("tiOctetDEnd", pair.Key.OctetDEnd);
                                query.SetParameter("iDecimalEnd", pair.Key.IpNumberEnd);
                                query.SetParameter("iIpAddressId", pair.Key.Id);
                                query.SetParameter("vchUpdaterId", "MinimizeIpRangesTask");
                                query.SetParameter("dtLastUpdate", DateTime.Now);

                                query.ExecuteUpdate();
                            }
                            else
                            {
                                var sql = new StringBuilder()
                                    .Append("UPDATE tIpAddressRange ")
                                    .Append("   SET tiRecordStatus = :tiRecordStatus ")
                                    .Append("      ,vchUpdaterId = :vchUpdaterId ")
                                    .Append("      ,dtLastUpdate = :dtLastUpdate ")
                                    .Append(" WHERE iIpAddressId = :iIpAddressId ")
                                    .ToString();

                                var query = uow.Session.CreateSQLQuery(sql);

                                query.SetParameter("tiRecordStatus", 0);
                                query.SetParameter("iIpAddressId", pair.Key.Id);
                                query.SetParameter("vchUpdaterId", "MinimizeIpRangesTask");
                                query.SetParameter("dtLastUpdate", DateTime.Now);
                                query.ExecuteUpdate();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat(ex.Message, ex);
                    }
                }
            }
        }
    }
}