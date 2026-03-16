#region

using System.Collections.Generic;
using R2V2.Web.Models.Browse;

#endregion

namespace R2V2.Web.Models
{
    public class SpecialtySummaries
    {
        public int SelectedSpecialtyId { get; set; }
        public IEnumerable<SpecialtySummary> Specialties { get; set; }
    }

    public class PublisherSummaries
    {
        public IEnumerable<PublisherSummary> Publishers { get; set; }
    }

    public class AuthorSummaries
    {
        public IEnumerable<AuthorSummary> Authors { get; set; }
    }
}