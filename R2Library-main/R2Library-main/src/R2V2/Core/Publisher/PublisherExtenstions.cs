namespace R2V2.Core.Publisher
{
    public static class PublisherExtenstions
    {
        public static string ToName(this IPublisher publisher)
        {
            return publisher.ConsolidatedPublisher != null ? publisher.ConsolidatedPublisher.Name : publisher.Name;
        }
    }
}