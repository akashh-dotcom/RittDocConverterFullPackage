#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public static class ReserveShelfExtensions
    {
        public static IEnumerable<ReserveShelfList> ToReserveShelves(
            this IEnumerable<ReserveShelf> reserveShelves)
        {
            return reserveShelves.Select(ToReserveShelf);
        }

        public static ReserveShelfList ToReserveShelf(this ReserveShelf reserveShelf)
        {
            return new ReserveShelfList
            {
                Id = reserveShelf.Id,
                Name = reserveShelf.Name,
                Description = reserveShelf.Description,
                DefaultSortBy = reserveShelf.DefaultSortBy,
                IsAscending = reserveShelf.IsAscending ?? true,
                ResourceCount = reserveShelf.ReserveShelfResources?.Count() ?? 0,
                Urls = reserveShelf.ReserveShelfUrls.ToReserveShelfUrls(reserveShelf.Id)
            };
        }

        public static IEnumerable<ReserveShelfUrl> ToReserveShelfUrls(
            this IEnumerable<Core.ReserveShelf.ReserveShelfUrl> reserveShelfUrls, int reserverShelfId)
        {
            List<ReserveShelfUrl> urls = null;
            if (reserveShelfUrls != null)
            {
                urls = reserveShelfUrls.Select(x => new ReserveShelfUrl(x.Id, x.ReserveShelfId, x.Url, x.Description))
                    .ToList();
                if (urls.Any())
                {
                    urls = urls.OrderBy(x => x.Description).ToList();
                }
            }

            return urls;
        }

        public static IEnumerable<IResource> SortBy(this IEnumerable<IResource> resources, string sortBy,
            bool isAscending = true)
        {
            switch (sortBy)
            {
                case "title":
                    return isAscending ? resources.OrderBy(x => x.Title) : resources.OrderByDescending(x => x.Title);
                case "pubdate":
                    return isAscending
                        ? resources.OrderBy(x => x.PublicationDate)
                        : resources.OrderByDescending(x => x.PublicationDate);
                case "releasedate":
                    return isAscending
                        ? resources.OrderBy(x => x.ReleaseDate)
                        : resources.OrderByDescending(x => x.ReleaseDate);
                case "publisher":
                    return isAscending
                        ? resources.OrderBy(x => x.Publisher.Name)
                        : resources.OrderByDescending(x => x.Publisher.Name);
                case "author":
                default:
                    return isAscending
                        ? resources.OrderBy(x => x.SortAuthor)
                        : resources.OrderByDescending(x => x.SortAuthor);
            }
        }
    }
}