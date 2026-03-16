#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Core.Resource.Author
{
    public class AuthorService : IAuthorService
    {
        private readonly IQueryable<Author> _authors;
        private readonly ILog<AuthorService> _log;
        private readonly IQueryable<Resource> _resources;

        public AuthorService(ILog<AuthorService> log, IQueryable<Author> authors, IQueryable<Resource> resources)
        {
            _log = log;
            _authors = authors;
            _resources = resources;
        }

        public IEnumerable<Author> GetAuthors(int institutionId, Include include, string practiceArea,
            bool displayAllProducts)
        {
            _log.DebugFormat(
                "GetAuthors() - institutionId: {0}, include: {1}, practiceArea: {2}, displayAllProducts: {3}",
                institutionId, include, practiceArea, displayAllProducts);
            int.TryParse(practiceArea, out var practiceAreaId);

            return (from author in _authors
                    orderby author.LastName
                    select new Author
                    {
                        Id = author.Id,
                        LastName = author.LastName,
                        ResourceCount = (from a in _authors
                            where a.Id == author.Id && a.Order == 1
                            join resource in _resources on a.ResourceId equals resource.Id
                            where (include == Include.Active && resource.StatusId == (int)ResourceStatus.Active)
                                  || (include == Include.Archive && resource.StatusId == (int)ResourceStatus.Archived)
                                  || (include == (Include.Active | Include.Archive) &&
                                      (resource.StatusId == (int)ResourceStatus.Active ||
                                       resource.StatusId == (int)ResourceStatus.Archived))
                            from resourcePracticeArea in resource.ResourcePracticeAreas
                            where resourcePracticeArea.PracticeArea.Id == practiceAreaId ||
                                  string.IsNullOrWhiteSpace(practiceArea)
                            from institutionResourceLicense in resource.InstitutionResourceLicenses
                            where displayAllProducts || (institutionResourceLicense.InstitutionId == institutionId &&
                                                         institutionResourceLicense.LicenseCount > 0)
                            select resource).Distinct().Count()
                    })
                .ToList()
                .Where(x => x.ResourceCount > 0)
                .GroupBy(a => new { a.LastName })
                .Select(g => new Author
                {
                    LastName = g.Key.LastName,
                    ResourceCount = g.Sum(s => s.ResourceCount)
                });
        }

        public Author GetAuthor(string lastName)
        {
            _log.DebugFormat("GetAuthors() - lastName: {0}", lastName);
            return _authors.FirstOrDefault(x => x.LastName == lastName);
        }
    }
}