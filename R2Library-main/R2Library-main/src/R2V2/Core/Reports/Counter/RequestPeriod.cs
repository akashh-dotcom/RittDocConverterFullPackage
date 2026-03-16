#region

using System;

#endregion

namespace R2V2.Core.Reports.Counter
{
    public class RequestPeriod
    {
        public RequestPeriod()
        {
        }

        public RequestPeriod(int month, int year)
        {
            Month = month;
            Year = year;
            HitCount = 0;
        }

        public int Month { get; set; }
        public int Year { get; set; }
        public int HitCount { get; set; }

        public string ShortMonth()
        {
            return DateTime.Parse($"{Month}/1/2010").ToString("MMM");
        }

        public DateTime BeginDate()
        {
            return DateTime.Parse($"{Month}/{1}/{Year}");
        }

        public DateTime EndDate()
        {
            return DateTime.Parse($"{Month}/{DateTime.DaysInMonth(Year, Month)}/{Year}");
        }
    }
}