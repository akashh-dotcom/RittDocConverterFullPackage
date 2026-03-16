#region

using System.Linq;
using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.Core.Resource
{
    public static class QueryableResourceExtensions
    {
        const string AllKey = "All";
        const string StartsWithNumber = "09";

        static readonly string[] Numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

        public static IQueryable<Resource> WhereResourceStatus(this IQueryable<Resource> resources, Include include)
        {
            if (include == (Include.Active | Include.Archive))
            {
                return resources.Where(x =>
                    x.StatusId == (int)ResourceStatus.Active || x.StatusId == (int)ResourceStatus.Archived);
            }

            if (include == Include.Archive)
            {
                return resources.Where(x => x.StatusId == (int)ResourceStatus.Archived);
            }

            // by default return Active resources
            return resources.Where(x => x.StatusId == (int)ResourceStatus.Active);
        }

        public static IQueryable<Resource> WhereTitleStartsWith(this IQueryable<Resource> resources, string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == AllKey)
            {
                return resources;
            }

            return s == StartsWithNumber
                ? resources.Where(x => Numbers.Contains(x.AlphaKey))
                : resources.Where(x => x.AlphaKey == s);
        }

        public static IQueryable<AZIndex> WhereNameStartsWith(this IQueryable<AZIndex> topics, string s)
        {
            if (s == StartsWithNumber)
            {
                return topics.Where(x => Numbers.Contains(x.AlphaKey));
            }

            return string.IsNullOrWhiteSpace(s) ? topics : topics.Where(x => x.AlphaKey == s);
        }

        public static IQueryable<Resource> OrderBy(this IQueryable<Resource> resources, string sortOrder)
        {
            switch (sortOrder)
            {
                case "author":
                    return resources.OrderBy(x => x.SortAuthor);

                case "publication-date":
                    return resources.OrderByDescending(x => x.PublicationDate);

                case "publisher":
                    return resources.OrderBy(x => x.Publisher.Name);

                case "release-date":
                    return resources.OrderBy(x => x.ReleaseDate);

                case "title":
                default:
                    return resources.OrderBy(x => x.SortTitle);
            }
        }
    }
}