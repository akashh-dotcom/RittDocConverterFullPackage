#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Institution
{
    public static class QueryableInstitutionExtensions
    {
        public static IQueryable<Institution> WhereAccountStatus(this IQueryable<Institution> institutions,
            AccountStatus accountStatus) //, IQueryable<License> licenses)
        {
            var now = DateTime.Now;
            switch (accountStatus)
            {
                case AccountStatus.Active:
                    institutions = institutions.Where(x => x.AccountStatusId == (int)AccountStatus.Active);
                    break;

                case AccountStatus.Disabled:
                    institutions = institutions.Where(x => x.AccountStatusId == (int)AccountStatus.Disabled);
                    break;

                case AccountStatus.Trial:
                    institutions = institutions.Where(x =>
                        x.AccountStatusId == (int)AccountStatus.Trial && now <= x.Trial.EndDate);
                    break;

                case AccountStatus.TrialExpired:
                    institutions = institutions.Where(x =>
                        x.AccountStatusId == (int)AccountStatus.Trial && x.Trial.EndDate < now);
                    break;
                case AccountStatus.PdaOnly:
                    //From Meg - this should display ANY account that has an active PDA title(s). (Squish #1173)    -DRJ
                    institutions = institutions.Where(x =>
                        x.AccountStatusId == (int)AccountStatus.Active
                        && x.InstitutionResourceLicenses.Any(y =>
                            y.LicenseTypeId == (short)LicenseType.Pda && y.PdaAddedDate != null &&
                            y.PdaDeletedDate == null)
                    );
                    break;
            }

            return institutions;
        }

        public static IQueryable<Institution> WhereRecentInstitutions(this IQueryable<Institution> institutions,
            IInstitutionQuery institionquery, int[] recentInstitutions)
        {
            //Page == "Recent"
            if ((institionquery.Page == "Recent" || institionquery.RecentOnly) && recentInstitutions != null)
            {
                return institutions.Where(x => recentInstitutions.Contains(x.Id));
            }

            return institutions;
        }

        public static IQueryable<Institution> WhereTerritory(this IQueryable<Institution> institutions, int territoryId)
        {
            if (territoryId > 0)
            {
                institutions = institutions.Where(x => x.Territory != null && x.Territory.Id == territoryId);
            }

            return institutions;
        }

        public static IQueryable<Institution> WhereInstitutionType(this IQueryable<Institution> institutions,
            int institutionTypeId)
        {
            if (institutionTypeId > 0)
            {
                institutions = institutions.Where(x => x.Type != null && x.Type.Id == institutionTypeId);
            }

            return institutions;
        }

        public static IQueryable<Institution> WhereExpertReviewer(this IQueryable<Institution> institutions,
            bool includeExportReviewer, bool excludeExportReviewer)
        {
            if (includeExportReviewer)
            {
                institutions = institutions.Where(x => x.ExpertReviewerUserEnabled);
            }

            if (excludeExportReviewer)
            {
                institutions = institutions.Where(x => !x.ExpertReviewerUserEnabled);
            }

            return institutions;
        }

        public static IQueryable<Institution> OrderByRecent(this IQueryable<Institution> institutions,
            IInstitutionQuery institionquery, int[] recentInstitutions)
        {
            if ((institionquery.Page == "Recent" || institionquery.RecentOnly) && recentInstitutions != null)
            {
                var institutionDictionary = new Dictionary<int, Institution>();
                for (var i = 0; i < recentInstitutions.Count(); i++)
                {
                    institutionDictionary.Add(i, institutions.FirstOrDefault(x => x.Id == recentInstitutions[i]));
                }

                return institutionDictionary.OrderBy(x => x.Key).Where(x => x.Value != null).Select(x => x.Value)
                    .AsQueryable();
            }

            return institutions;
        }
    }
}