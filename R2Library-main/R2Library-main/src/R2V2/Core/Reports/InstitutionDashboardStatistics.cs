#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Reports
{
    public class InstitutionDashboardStatistics : InstitutionStatistics
    {
        public InstitutionDashboardStatistics(int institutionId, DateTime startDate, DateTime endDate,
            InstitutionAccountUsage accountUsage, InstitutionHighlights highlights)
        {
            AccountUsage = accountUsage;
            Highlights = highlights;

            InstitutionId = institutionId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public InstitutionDashboardStatistics(int institutionId, DateTime startDate, DateTime endDate,
            List<InstitutionResourceStatistics> resourceStatisticsList, int recountCount)
        {
            InstitutionId = institutionId;
            StartDate = startDate;
            EndDate = endDate;
            InstitutionResourceStatistics = resourceStatisticsList;

            Highlights = new InstitutionHighlights { TotalResourceCount = recountCount };
        }

        public List<InstitutionResourceStatistics> InstitutionResourceStatistics { get; set; }

        public string ToDebugString(IUser user = null)
        {
            var sb = new StringBuilder("InstitutionDashboardStatistics = [");
            sb.AppendFormat("InstitutionId: {0}", InstitutionId);
            if (user != null)
            {
                sb.AppendFormat(", UserId: {0}", user.Id);
                sb.AppendFormat(", User Role: {0}", user.Role.Code);
            }

            sb.AppendFormat(", MostAccessedResourceId: {0}", Highlights.MostAccessedResourceId);
            sb.AppendFormat(", MostAccessedCount: {0}", Highlights.MostAccessedCount);
            sb.AppendFormat(", LeastAccessedResourceId: {0}", Highlights.LeastAccessedResourceId);
            sb.AppendFormat(", LeastAccessedCount: {0}", Highlights.LeastAccessedCount);
            sb.AppendFormat(", MostTurnawayAccessResourceId: {0}", Highlights.MostTurnawayAccessResourceId);
            sb.AppendFormat(", MostTurnawayAccessCount: {0}", Highlights.MostTurnawayAccessCount);
            sb.AppendFormat(", MostTurnawayConcurrentResourceId: {0}", Highlights.MostTurnawayConcurrentResourceId);
            sb.AppendFormat(", MostTurnawayConcurrentCount: {0}", Highlights.MostTurnawayConcurrentCount);
            sb.AppendFormat(", MostPopularSpecialtyName: {0}", Highlights.MostPopularSpecialtyName);
            sb.AppendFormat(", MostPopularSpecialtyCount: {0}", Highlights.MostPopularSpecialtyCount);
            sb.AppendFormat(", LeastPopularSpecialtyName: {0}", Highlights.LeastPopularSpecialtyName);
            sb.AppendFormat(", LeastPopularSpecialtyCount: {0}", Highlights.LeastPopularSpecialtyCount);
            sb.AppendFormat(", TotalResourceCount: {0}", Highlights.TotalResourceCount).AppendLine().Append("\t");
            ;
            sb.AppendFormat(", ContentCount: {0}", AccountUsage.ContentCount);
            sb.AppendFormat(", TocCount: {0}", AccountUsage.TocCount);
            sb.AppendFormat(", SessionCount: {0}", AccountUsage.SessionCount);
            sb.AppendFormat(", PrintCount: {0}", AccountUsage.PrintCount);
            sb.AppendFormat(", EmailCount: {0}", AccountUsage.EmailCount);
            sb.AppendFormat(", TurnawayConcurrencyCount: {0}", AccountUsage.TurnawayConcurrencyCount);
            sb.AppendFormat(", TurnawayAccessCount: {0}", AccountUsage.TurnawayAccessCount);
            return sb.ToString();
        }


        public int[] GetAllResourceIds()
        {
            var resourceIds = InstitutionResourceStatistics != null
                ? InstitutionResourceStatistics.Select(item => item.ResourceId).ToList()
                : new List<int>();

            if (Highlights != null)
            {
                resourceIds.Add(Highlights.MostAccessedResourceId);
                resourceIds.Add(Highlights.LeastAccessedResourceId);
                resourceIds.Add(Highlights.MostTurnawayConcurrentResourceId);
                resourceIds.Add(Highlights.MostTurnawayAccessResourceId);
            }

            return resourceIds.Any() ? resourceIds.Distinct().ToArray() : null;
        }
    }
}