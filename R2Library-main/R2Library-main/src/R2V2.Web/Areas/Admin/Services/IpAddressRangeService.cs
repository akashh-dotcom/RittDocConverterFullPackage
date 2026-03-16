#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models.IpAddressRange;
using R2V2.Web.Infrastructure.HttpModules;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class IpAddressRangeService
    {
        private readonly IAdminContext _adminContext;
        private readonly InstitutionService _institutionService;
        private readonly IpAddressRangeFactory _ipAddressRangeFactory;
        private readonly ILog<IpAddressRangeService> _log;

        public IpAddressRangeService(
            ILog<IpAddressRangeService> log
            , IpAddressRangeFactory ipAddressRangeFactory
            , IAdminContext adminContext
            , InstitutionService institutionService
        )
        {
            _log = log;
            _ipAddressRangeFactory = ipAddressRangeFactory;
            _adminContext = adminContext;
            _institutionService = institutionService;
        }

        public List<IpAddressRange> GetOverLappingIpRanges(IpAddressRange ipAddressRange, int institutionId)
        {
            var institution = _institutionService.GetInstitutionForAdmin(institutionId);
            return _ipAddressRangeFactory.GetOverLappingIpRanges2(ipAddressRange, institution);
        }

        public List<IpAddressRange> GetOverLappingIpRanges(IpAddressRange ipAddressRange)
        {
            return _ipAddressRangeFactory.GetOverLappingIpRanges(ipAddressRange);
        }

        public string SaveIpAddressRange(IpAddressRange ipAddressRange, bool isRa, int institutionId)
        {
            RePopulateIpAddressRange(ipAddressRange);

            var errorMessage = ValidateIpAddressRange(ipAddressRange, isRa, institutionId);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return errorMessage;
            }

            var dbIpAddress =
                _ipAddressRangeFactory.GetInstitutionIpRange(ipAddressRange.InstitutionId, ipAddressRange.Id);

            _ipAddressRangeFactory.SaveIpAddressRange(ipAddressRange,
                GetIpAddressRangeAuditMessage(dbIpAddress, ipAddressRange));

            return null;
        }

        public bool Delete(int institutionId, int id)
        {
            return _ipAddressRangeFactory.Delete(institutionId, id);
        }

        public List<string> BulkDelete(int institutionId, string ipAddressRangeIdsString)
        {
            var formattedIpAddress = _ipAddressRangeFactory.BulkDelete(institutionId, ipAddressRangeIdsString);
            if (formattedIpAddress != null && formattedIpAddress.Any())
            {
                return formattedIpAddress;
            }

            return null;
        }

        public BulkRemoveIpRanges GetBulkRemoveIpRanges(int institutionId, string ipAddressRangeIdsString)
        {
            var ipAddressRanges = GetIpAddressRanges(institutionId, ipAddressRangeIdsString);

            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var model = new BulkRemoveIpRanges(adminInstitution)
            {
                FormattedIpAddress = new List<string>(ipAddressRanges.Select(x =>
                    $"{x.GetIpAddressRangeStart()} - {x.GetIpAddressRangeEnd()}")),
                IpRangeIdsToDelete = ipAddressRangeIdsString
            };
            return model;
        }

        private IEnumerable<IpAddressRange> GetIpAddressRanges(int institutionId, string ipAddressRangeIdsString)
        {
            var ipAddressRangeIds = ipAddressRangeIdsString.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(int.Parse);
            return _ipAddressRangeFactory.GetInstitutionIpRanges(institutionId, ipAddressRangeIds);
        }

        private void RePopulateIpAddressRange(IpAddressRange ipAddressRange)
        {
            var institution = _institutionService.GetInstitutionForEdit(ipAddressRange.InstitutionId);
            ipAddressRange.Institution = (Institution)institution;
            ipAddressRange.PopulateDecimals();
        }

        private string ValidateIpAddressRange(IpAddressRange ipAddressRange, bool isRa, int institutionId)
        {
            var errorMessage = IsIpRangeValid(ipAddressRange);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return errorMessage;
            }

            var overlappingIpRanges = GetOverLappingIpRanges(ipAddressRange, institutionId);
            if (overlappingIpRanges.Any())
            {
                return GetOverLappingIpRangesErrorMessage(overlappingIpRanges, ipAddressRange.Institution.Id, isRa);
            }

            return null;
        }

        private string GetOverLappingIpRangesErrorMessage(List<IpAddressRange> overlapingIpRanges, int institutionId,
            bool isRa)
        {
            var isMultipleOverLaps = overlapingIpRanges.Count > 1;
            if (isRa)
            {
                var overlapingIpRangesAccountNumber = GetOverLapingIPsAccountNumbers(overlapingIpRanges);
                _log.InfoFormat("There was mulitple overlaps with customers: {0}", overlapingIpRangesAccountNumber);

                return isMultipleOverLaps
                    ? $"Sorry this IP Range conflicts with mulitple accounts: {overlapingIpRangesAccountNumber}"
                    : $"Sorry this IP Range conflicts with another account: {overlapingIpRangesAccountNumber}";
            }

            var isOwnIpRanges = overlapingIpRanges.Aggregate(true,
                (current, overlapingIpRange) => current && institutionId == overlapingIpRange.Institution.Id);

            return isOwnIpRanges
                ? "This IP Range conflicts with a different IP range you already have."
                : isMultipleOverLaps
                    ? "Sorry this IP Range conflicts with mulitple accounts. Please Contact Customer Service."
                    : "Sorry this IP Range conflicts with another account. Please Contact Customer Service.";
        }

        public List<IpAddressRange> GetInstitutionIpRanges(int institutionId)
        {
            return _ipAddressRangeFactory.GetInstitutionIpRanges(institutionId).ToList();
        }

        public InstitutionIpRanges GetInstitutionIpRangeWithConflicts(int institutionId)
        {
            var institution = _institutionService.GetInstitutionForAdmin(institutionId);

            var ipAddressRanges = GetInstitutionIpRanges(institutionId);
            var institutionIpRange = GetInstitutionIpRange(institutionId, ipAddressRanges);

            institutionIpRange.IsIpPlusEnabled = institution.EnableIpPlus;

            SetConflictingIpRanges(institutionIpRange, ipAddressRanges);
            return institutionIpRange;
        }

        public InstitutionIpRanges GetInstitutionIpRange(int institutionId)
        {
            var ipAddressRanges = GetInstitutionIpRanges(institutionId);

            return GetInstitutionIpRange(institutionId, ipAddressRanges);
        }

        public InstitutionIpRanges GetInstitutionIpRangeForEdit(int institutionId, int ipAddressRangeId)
        {
            var institutionIpRanges = GetInstitutionIpRange(institutionId);
            institutionIpRanges.EditIpAddressRange =
                institutionIpRanges.IpAddressRanges.FirstOrDefault(x => x.Id == ipAddressRangeId);
            return institutionIpRanges;
        }

        private InstitutionIpRanges GetInstitutionIpRange(int institutionId, List<IpAddressRange> ipAddressRanges)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);

            var institutionIpRanges = ipAddressRanges.ToIpAddressRanges();
            var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
            var currentIpAddress = requestLoggerData.IpAddress;

            var institutionIpRange = new InstitutionIpRanges(adminInstitution)
            {
                IpAddressRanges = institutionIpRanges,
                CurrentIpAddress = currentIpAddress
            };
            return institutionIpRange;
        }

        private void SetConflictingIpRanges(InstitutionIpRanges institutionIpRange,
            List<IpAddressRange> ipAddressRanges)
        {
            var conflictingIpRanges = new List<ConflictingIpRange>();
            if (!institutionIpRange.IsIpPlusEnabled)
            {
                var conflictingIpAddressRanges =
                    _ipAddressRangeFactory.GetConflictingIpAddressRanges(institutionIpRange.InstitutionId,
                        ipAddressRanges);
                if (conflictingIpAddressRanges != null && conflictingIpAddressRanges.Any())
                {
                    var i = 1;
                    foreach (var conflictingIpAddressRange in conflictingIpAddressRanges)
                    {
                        conflictingIpRanges.Add(new ConflictingIpRange(conflictingIpAddressRange, i));
                        i++;
                    }

                    institutionIpRange.CanMergeRanges = true;
                }
            }

            var overlappingConflicts = GetConflictingInstitutionIpRanges(ipAddressRanges);
            if (overlappingConflicts != null && overlappingConflicts.Any())
            {
                conflictingIpRanges.AddRange(overlappingConflicts);
            }
            else if (!institutionIpRange.CanMergeRanges)
            {
                institutionIpRange.CanMergeRanges = ConsolidateAllIpRanges(ipAddressRanges, false, true).Any();
            }

            institutionIpRange.ConflictingIpRanges = conflictingIpRanges;
        }

        public string GetIpAddressRangeAuditMessage(IpAddressRange dbIpAddress, IpAddressRange newIpAddress)
        {
            if (dbIpAddress == null)
            {
                return newIpAddress.ToAuditString();
            }

            return new StringBuilder()
                .Append("Editing an already existing IP Range ")
                .AppendFormat("(Current: {0} - {1})|", dbIpAddress.GetIpAddressRangeStart(),
                    dbIpAddress.GetIpAddressRangeEnd())
                .AppendFormat("(New: {0} - {1})", newIpAddress.GetIpAddressRangeStart(),
                    newIpAddress.GetIpAddressRangeEnd())
                .AppendFormat("|| Institution.AccountNumber: {0}", newIpAddress.Institution.AccountNumber)
                .AppendFormat(", Institution.Name: {0}", newIpAddress.Institution.Name)
                .ToString();
        }

        public string IsIpRangeValid(IpAddressRange ipAddressRange)
        {
            if (ipAddressRange.IpNumberStart == 0 || ipAddressRange.IpNumberEnd == 0)
            {
                return "Please enter a Valid IP Range.";
            }

            if (ipAddressRange.IpNumberStart > ipAddressRange.IpNumberEnd)
            {
                return "The Start IP address must be less then the End IP address.";
            }

            return null;
        }

        public string GetOverLapingIPsAccountNumbers(IEnumerable<IpAddressRange> overlapingIpRanges)
        {
            var sb = new StringBuilder();
            //Grabbing all accountnumbers of the overlaps for RAs
            var overlapingAccountNumbers = overlapingIpRanges.Select(x => x.Institution.AccountNumber).Distinct();
            foreach (var accountNumber in overlapingAccountNumbers)
            {
                sb.AppendFormat("{0}|", accountNumber);
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public InstitutionIpRangesConsolidated GetConsolidatedIpRanges(int institutionId, bool getConflicts)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var model = new InstitutionIpRangesConsolidated(adminInstitution);
            var consolidatedIpAddressRanges = GetIpRangesToConsolidate(institutionId, getConflicts);
            model.ConsolidateIpAddressRanges = consolidatedIpAddressRanges;
            model.GetConflicts = getConflicts;
            return model;
        }

        public string ConsolidateIpAddressRanages(int institutionId, int startIpAddressRangeId, int endIpAddressRangeId,
            bool isRa)
        {
            var ipAddressRanges = GetInstitutionIpRanges(institutionId);

            var startIpAddressRange = ipAddressRanges.FirstOrDefault(x => x.Id == startIpAddressRangeId);
            var endIpAddressRange = ipAddressRanges.FirstOrDefault(x => x.Id == endIpAddressRangeId);

            if (startIpAddressRange == null || endIpAddressRange == null)
            {
                return "The IP Ranges no longer exist.";
            }

            var pairedIpAddressRanges = new PairedIpAddressRanges(startIpAddressRange, endIpAddressRange);

            pairedIpAddressRanges.PopulateMergedIpAddressRange();

            var overlapingIpRanges = GetOverLappingIpRanges(pairedIpAddressRanges.MergedIpAddressRange, institutionId);

            var overlapingIpRangesNotTheres = overlapingIpRanges
                .Where(x => x.InstitutionId != pairedIpAddressRanges.MergedIpAddressRange.InstitutionId).ToList();
            if (overlapingIpRangesNotTheres.Any())
            {
                return GetOverLappingIpRangesErrorMessage(overlapingIpRangesNotTheres,
                    pairedIpAddressRanges.MergedIpAddressRange.InstitutionId, isRa);
            }

            _ipAddressRangeFactory.SavePairIpaddressRange(pairedIpAddressRanges);

            return null;
        }

        public List<ConsolidatedIpRange> GetIpRangesToConsolidate(int institutionId, bool conflictsOnly)
        {
            var ipAddressRanges = GetInstitutionIpRanges(institutionId);
            return ConsolidateAllIpRanges(ipAddressRanges, conflictsOnly, false);
        }

        private List<ConflictingIpRange> GetConflictingInstitutionIpRanges(List<IpAddressRange> ipAddressRanges)
        {
            var conflictingIpRanges = new List<ConflictingIpRange>();

            var sortedRanges = ipAddressRanges.OrderByDescending(x => x.IpNumberEnd - x.IpNumberStart)
                .ThenBy(x => x.IpNumberStart).ThenBy(x => x.IpNumberEnd).ToList();

            var baseRanges = sortedRanges.ToList();

            foreach (var range in sortedRanges)
            {
                //currentRange = range;
                var keepGoing = true;
                while (keepGoing)
                {
                    baseRanges.Remove(range);
                    IpAddressRange rangeWithin2 = null;
                    IpAddressRange rangeBetween1 = null;
                    IpAddressRange rangeBetween2 = null;

                    var foundRange = FindRangeToMerge(range, baseRanges, true);

                    if (foundRange != null)
                    {
                        conflictingIpRanges.Add(new ConflictingIpRange(new PairedIpAddressRanges(range, foundRange)));
                        baseRanges.Remove(foundRange);
                    }
                    else
                    {
                        keepGoing = false;
                    }
                }
            }

            return conflictingIpRanges;
        }

        private List<ConsolidatedIpRange> ConsolidateAllIpRanges(List<IpAddressRange> ipAddressRanges,
            bool conflictsOnly, bool containsAny)
        {
            var sortedRanges = ipAddressRanges //.OrderByDescending(x => x.IpNumberEnd - x.IpNumberStart)
                .OrderBy(x => x.IpNumberStart).ThenBy(x => x.IpNumberEnd).ToList();

            var consolidatedIpRanges = new List<ConsolidatedIpRange>();

            var ipAddressRangesEdited = new List<IpAddressRange>();

            var ipaddressRanges = sortedRanges.ToList();

            foreach (var range in sortedRanges)
            {
                if (ipAddressRangesEdited.Contains(range))
                {
                    continue;
                }

                var ipRangesToRemove = new List<IpAddressRange>();

                ipaddressRanges.Remove(range);
                var keepGoing = true;


                var pairedIpRanges = new PairedIpAddressRanges(range, range);

                while (keepGoing)
                {
                    var foundRange = FindRangeToMerge(pairedIpRanges.MergedIpAddressRange, ipaddressRanges,
                        conflictsOnly);

                    var startRange = pairedIpRanges.IpAddressRange1;
                    var endRange = pairedIpRanges.IpAddressRange2;

                    if (foundRange != null)
                    {
                        if (foundRange.IpNumberEnd > endRange.IpNumberEnd)
                        {
                            pairedIpRanges.IpAddressRange2 = foundRange;
                        }
                        else if (foundRange.IpNumberStart < startRange.IpNumberStart)
                        {
                            pairedIpRanges.IpAddressRange1 = foundRange;
                        }

                        //Remove the looped range first go around.
                        if (!ipRangesToRemove.Any())
                        {
                            ipRangesToRemove.Add(range);
                        }

                        ipRangesToRemove.Add(foundRange);
                        ipaddressRanges.Remove(foundRange);
                    }
                    else
                    {
                        keepGoing = false;
                    }

                    pairedIpRanges.PopulateMergedIpAddressRange();
                }

                if (ipRangesToRemove.Any())
                {
                    var orderedIpRanges = ipRangesToRemove //.OrderByDescending(x => x.IpNumberEnd - x.IpNumberStart)
                        .OrderBy(x => x.OctetA).ThenBy(x => x.OctetB).ThenBy(x => x.OctetCStart)
                        .ThenBy(x => x.OctetCEnd).ThenBy(x => x.OctetDStart).ThenBy(x => x.OctetDEnd);

                    var consolidatedIpRange =
                        new ConsolidatedIpRange
                        {
                            PairedIpRanges = pairedIpRanges,
                            IpAddressRanges = orderedIpRanges.Select(x => x.ToIpAddressRange()).ToList()
                        };
                    consolidatedIpRanges.Add(consolidatedIpRange);
                    ipAddressRangesEdited.AddRange(ipRangesToRemove);

                    if (containsAny)
                    {
                        return consolidatedIpRanges;
                    }
                }
            }
            return consolidatedIpRanges;
        }

        private IpAddressRange FindRangeToMerge(IpAddressRange range, List<IpAddressRange> ranges, bool onlyConflicts)
        {
            IpAddressRange foundRange;
            if (onlyConflicts)
            {
                foundRange = ranges.FirstOrDefault(x => range.IpNumberStart.Between(x.IpNumberStart, x.IpNumberEnd));
                if (foundRange == null)
                {
                    foundRange = ranges.FirstOrDefault(x => range.IpNumberEnd.Between(x.IpNumberStart, x.IpNumberEnd));
                }

                if (foundRange == null)
                {
                    foundRange = ranges.FirstOrDefault(x =>
                        x.IpNumberStart.Between(range.IpNumberStart, range.IpNumberEnd));
                }

                if (foundRange == null)
                {
                    foundRange =
                        ranges.FirstOrDefault(x => x.IpNumberEnd.Between(range.IpNumberStart, range.IpNumberEnd));
                }
            }
            else
            {
                foundRange =
                    ranges.FirstOrDefault(x => (range.IpNumberStart + 1).Between(x.IpNumberStart, x.IpNumberEnd));
                if (foundRange == null)
                {
                    foundRange = ranges.FirstOrDefault(x =>
                        (range.IpNumberEnd + 1).Between(x.IpNumberStart, x.IpNumberEnd));
                }

                if (foundRange == null)
                {
                    foundRange = ranges.FirstOrDefault(x =>
                        (x.IpNumberStart + 1).Between(range.IpNumberStart, range.IpNumberEnd));
                }

                if (foundRange == null)
                {
                    foundRange = ranges.FirstOrDefault(x =>
                        (x.IpNumberEnd + 1).Between(range.IpNumberStart, range.IpNumberEnd));
                }
            }

            return foundRange;
        }

        public string CheckIpAddressRanges(IInstitution institution, bool includeAccountNumber)
        {
            var ipAddressRangeOverLaps = _ipAddressRangeFactory.GetOverLappingIpRanges(institution.Id);

            // Filter out IP Plus institutions if this institution has IP Plus enabled
            if (institution.EnableIpPlus)
            {
                ipAddressRangeOverLaps = ipAddressRangeOverLaps.Where(x => !x.Institution.EnableIpPlus).ToList();
            }

            var sb = new StringBuilder();

            foreach (var addressRange in ipAddressRangeOverLaps)
            {
                sb.Append(includeAccountNumber ? $"(Account:{addressRange.Institution.AccountNumber} - " : "(")
                    .Append(
                        $"{addressRange.OctetA}.{addressRange.OctetB}.{addressRange.OctetCStart}.{addressRange.OctetDStart}")
                    .Append("--")
                    .Append(
                        $"{addressRange.OctetA}.{addressRange.OctetB}.{addressRange.OctetCEnd}.{addressRange.OctetDEnd}")
                    .AppendLine(")");
            }

            return sb.ToString();
        }
    }

    public static class CompareExtension
    {
        public static bool Between(this long num, long lower, long upper, bool excludeEquals = false)
        {
            return excludeEquals ? lower < num && num < upper : lower <= num && num <= upper;
        }
    }
}