#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public static class CollectionManagementResourceExtensions
    {
        public static IEnumerable<CollectionManagementResource> OrderBy(
            this IEnumerable<CollectionManagementResource> institutionResources, IResourceQuery resourceQuery)
        {
            switch (resourceQuery.SortBy)
            {
                case "status":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.Resource.StatusId)
                        : institutionResources.OrderBy(x => x.Resource.StatusId);

                case "releasedate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.Resource.ReleaseDate)
                        : institutionResources.OrderByDescending(x => x.Resource.ReleaseDate);

                case "title":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x =>
                            x.Resource.SortTitle == null ? "z" : x.Resource.SortTitle.Trim())
                        : institutionResources.OrderByDescending(x =>
                            x.Resource.SortTitle == null ? "a" : x.Resource.SortTitle.Trim());

                case "publisher":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x =>
                            x.Resource.Publisher == null ? "z" : x.Resource.Publisher.Name.Replace("The", "").Trim())
                        : institutionResources.OrderByDescending(x =>
                            x.Resource.Publisher == null ? "z" : x.Resource.Publisher.Name.Replace("The", "").Trim());

                case "author":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x =>
                            x.Resource.SortAuthor == null ? "z" : x.Resource.SortAuthor.Trim())
                        : institutionResources.OrderByDescending(x =>
                            x.Resource.SortAuthor == null ? "a" : x.Resource.SortAuthor.Trim());

                //? institutionResources.OrderBy(x => x.Resource.AuthorList.Any() ? (x.Resource.AuthorList.FirstOrDefault(i => i.Order == 1)).LastName : "z")
                //: institutionResources.OrderByDescending(x => x.Resource.AuthorList.Any() ? (x.Resource.AuthorList.FirstOrDefault(i => i.Order == 1)).LastName : "z");

                case "duedate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.Resource.ForthcomingDate ?? "99/99")
                        : institutionResources.OrderByDescending(x => x.Resource.ForthcomingDate ?? "01/01");

                case "price":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.Resource.ListPrice)
                        : institutionResources.OrderByDescending(x => x.Resource.ListPrice);

                case "publicationdate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.Resource.PublicationDate)
                        : institutionResources.OrderByDescending(x => x.Resource.PublicationDate);

                case "pdadateadded":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.PdaAddedDate)
                        : institutionResources.OrderByDescending(x => x.PdaAddedDate);

                case "pdaviewcount":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.PdaViewCount)
                        : institutionResources.OrderByDescending(x => x.PdaViewCount);

                case "pdadatedeleted":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? institutionResources.OrderBy(x => x.PdaDeletedDate)
                        : institutionResources.OrderByDescending(x => x.PdaDeletedDate);

                default:
                    return institutionResources.OrderByDescending(x => x.Resource.PublicationDate);
            }
        }
    }
}