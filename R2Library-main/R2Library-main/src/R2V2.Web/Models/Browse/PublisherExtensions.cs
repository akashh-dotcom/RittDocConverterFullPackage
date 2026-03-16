#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Publisher;

#endregion

namespace R2V2.Web.Models.Browse
{
    public static class PublisherExtensions
    {
        public static PublisherDetail ToPublisherDetail(this IPublisher publisher)
        {
            var publisherDetail = new PublisherDetail();

            if (publisher != null)
            {
                publisherDetail.Id = publisher.Id;
                publisherDetail.Name = publisher.Name;
            }

            return publisherDetail;
        }

        public static IEnumerable<PublisherSummary> ToPublisherSummaries(this IEnumerable<IPublisher> publishers)
        {
            return publishers.Select(ToPublisherSummary);
        }

        public static PublisherSummary ToPublisherSummary(this IPublisher publisher)
        {
            return new PublisherSummary
            {
                Id = publisher.Id,
                Name = publisher.Name,
                ResourceCount = publisher.ResourceCount
            };
        }
    }
}