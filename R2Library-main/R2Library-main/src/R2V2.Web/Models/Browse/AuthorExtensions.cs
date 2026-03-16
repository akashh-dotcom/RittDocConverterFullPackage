#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Resource.Author;

#endregion

namespace R2V2.Web.Models.Browse
{
    public static class AuthorExtensions
    {
        public static AuthorDetail ToAuthorDetail(this IAuthor author)
        {
            var authorDetail = new AuthorDetail();

            if (author != null)
            {
                authorDetail.Name = author.LastName;
            }

            return authorDetail;
        }

        public static IEnumerable<AuthorSummary> ToAuthorSummaries(this IEnumerable<GroupedAuthors> authors)
        {
            return authors.Select(ToAuthorSummary);
        }

        public static AuthorSummary ToAuthorSummary(this GroupedAuthors author)
        {
            return new AuthorSummary
            {
                // Id = author.Id,
                Name = author.LastName,
                ResourceCount = author.ResourceCount
            };
        }

        public static IEnumerable<AuthorSummary> ToAuthorSummaries(this IEnumerable<Author> authors)
        {
            return authors.Select(ToAuthorSummary);
        }

        public static AuthorSummary ToAuthorSummary(this IAuthor author)
        {
            return new AuthorSummary
            {
                // Id = author.Id,
                Name = author.LastName,
                ResourceCount = author.ResourceCount
            };
        }
    }
}