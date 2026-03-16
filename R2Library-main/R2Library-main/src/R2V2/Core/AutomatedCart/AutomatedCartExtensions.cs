#region

using System;
using System.Linq;
using R2V2.Core.Institution;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public static class AutomatedCartExtensions
    {
        public static IQueryable<DbAutomatedCartEvent> FilterDate(
            this IQueryable<DbAutomatedCartEvent> automatedCartEvents, IQueryable<IInstitution> institutions,
            DateTime startDate, DateTime endDate)
        {
            return from ace in automatedCartEvents
                join i1 in institutions on ace.InstitutionId equals i1.Id
                where ace.EventDate >= startDate.Date &&
                      ace.EventDate <= endDate.Date &&
                      i1.AccountNumber != "999999"
                select ace;
        }

        public static IQueryable<DbAutomatedCartEvent> FilterAccountNumbers(
            this IQueryable<DbAutomatedCartEvent> automatedCartEvents, IQueryable<IInstitution> institutions,
            string[] accountNumbers)
        {
            if (accountNumbers == null)
            {
                return automatedCartEvents;
            }

            return from ace in automatedCartEvents
                join i2 in institutions on ace.InstitutionId equals i2.Id
                where accountNumbers.Contains(i2.AccountNumber)
                select ace;
        }

        public static IQueryable<DbAutomatedCartEvent> FilterInstitutionIds(
            this IQueryable<DbAutomatedCartEvent> automatedCartEvents, int[] institutionIds)
        {
            if (institutionIds == null)
            {
                return automatedCartEvents;
            }

            return from ace in automatedCartEvents
                where institutionIds.Contains(ace.InstitutionId)
                select ace;
        }


        public static IQueryable<DbAutomatedCartEvent> FilterInstitutionTypes(
            this IQueryable<DbAutomatedCartEvent> automatedCartEvents, IQueryable<IInstitution> institutions,
            int[] institutionTypeIds)
        {
            if (institutionTypeIds == null || !institutionTypeIds.Any())
            {
                return automatedCartEvents;
            }

            if (institutionTypeIds.Length == 1 && institutionTypeIds.First() == 0)
            {
                return automatedCartEvents;
            }

            return from ace in automatedCartEvents
                join i3 in institutions on ace.InstitutionId equals i3.Id
                where institutionTypeIds.Contains(i3.Type.Id)
                select ace;
        }


        public static IQueryable<DbAutomatedCartEvent> FilterTerritories(
            this IQueryable<DbAutomatedCartEvent> automatedCartEvents, IQueryable<ITerritory> territories,
            string[] codes)
        {
            if (codes == null || !codes.Any())
            {
                return automatedCartEvents;
            }

            if (codes.Length == 1 && string.Equals(codes.First(), "all", StringComparison.CurrentCultureIgnoreCase))
            {
                return automatedCartEvents;
            }

            return from ace in automatedCartEvents
                join t in territories on ace.TerritoryId equals t.Id
                where codes.Contains(t.Code)
                select ace;
        }
    }
}