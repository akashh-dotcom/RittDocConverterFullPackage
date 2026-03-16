#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Models.Resource
{
    public static class ResourceExtensions
    {
        public static IEnumerable<ResourceSummary> ToResourceSummaries(this IEnumerable<IResource> resources)
        {
            return resources.Select(resource => resource.ToResourceSummary());
        }

        public static ResourceSummary ToResourceSummary(this IResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new ResourceSummary
            {
                Id = resource.Id,
                Isbn = resource.Isbn,
                Title =
                    $"{resource.Title}{(string.IsNullOrEmpty(resource.Edition) ? string.Empty : $", {resource.Edition}")}",
                SubTitle = resource.SubTitle,
                Authors = resource.Authors,
                Publisher = resource.Publisher.ToName(),
                PublicationDate = resource.PublicationDate,
                ImageFileName = resource.ImageFileName,
                ImageUrl = resource.ImageUrl,
                IsArchive = resource.IsArchive(),
                IsForthcoming = resource.IsForthcoming()
            };
        }

        public static ResourceDetail ToResourceDetail(this IResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new ResourceDetail
            {
                Id = resource.Id,
                Isbn = resource.Isbn,
                Title = resource.Title,
                SubTitle = resource.SubTitle,
                Edition = resource.Edition,
                Authors = resource.Authors,
                Publisher = resource.Publisher.ToName(),
                PublicationDate = resource.PublicationDate,
                Description = resource.Description,
                ImageFileName = resource.ImageFileName,
                ImageUrl = resource.ImageUrl,
                PracticeArea = resource.PracticeAreasToString(),
                IsArchive = resource.IsArchive(),
                IsForthcoming = resource.IsForthcoming(),
                Isbn10 = resource.Isbn10,
                Isbn13 = resource.Isbn13,
                EIsbn = resource.EIsbn,
                Include = resource.IsArchive() ? 2 : 1,
                TabersStatus = resource.TabersStatus
            };
        }

        public static IEnumerable<int> GetDistinctPracticeAreaIds(this IResource resource)
        {
            var ids = new List<int>();
            foreach (var practiceArea in resource.PracticeAreas)
            {
                if (!ids.Contains(practiceArea.Id))
                {
                    ids.Add(practiceArea.Id);
                }
            }

            return ids;
        }
    }
}