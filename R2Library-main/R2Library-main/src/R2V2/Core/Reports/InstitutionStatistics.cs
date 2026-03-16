#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public class InstitutionStatistics
    {
        public int InstitutionId { get; set; }

        private DateTime _aggregationDate { get; set; }

        public DateTime StartDate
        {
            get
            {
                var minStartDate = DateTime.Parse("01/01/2009");
                if (_aggregationDate < minStartDate)
                {
                    _aggregationDate = minStartDate;
                }

                return _aggregationDate;
            }
            set => _aggregationDate = value;
        }

        public DateTime EndDate { get; set; }

        public InstitutionAccountUsage AccountUsage { get; set; }

        public InstitutionHighlights Highlights { get; set; }
    }

    public class InstitutionAccountUsage
    {
        public virtual int ContentCount { get; set; }
        public virtual int TocCount { get; set; }
        public virtual int SessionCount { get; set; }
        public virtual int PrintCount { get; set; }
        public virtual int EmailCount { get; set; }
        public virtual int TurnawayConcurrencyCount { get; set; }
        public virtual int TurnawayAccessCount { get; set; }
    }

    public class InstitutionHighlights
    {
        public virtual int MostAccessedResourceId { get; set; }
        public virtual int MostAccessedCount { get; set; }
        public virtual int LeastAccessedResourceId { get; set; }
        public virtual int LeastAccessedCount { get; set; }
        public virtual int MostTurnawayConcurrentResourceId { get; set; }
        public virtual int MostTurnawayConcurrentCount { get; set; }
        public virtual int MostTurnawayAccessResourceId { get; set; }
        public virtual int MostTurnawayAccessCount { get; set; }
        public virtual string MostPopularSpecialtyName { get; set; }
        public virtual int MostPopularSpecialtyCount { get; set; }
        public virtual string LeastPopularSpecialtyName { get; set; }
        public virtual int LeastPopularSpecialtyCount { get; set; }

        public virtual int TotalResourceCount { get; set; }
    }
}