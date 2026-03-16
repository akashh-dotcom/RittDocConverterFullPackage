#region

using System.Collections.Generic;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Publisher
{
    public interface IPublisherService
    {
        IList<IPublisher> GetPublishers();
        Publisher GetPublisherForAdmin(int id);
        IPublisher GetPublisher(int id);
        void ClearPublisherCache();

        IList<IPublisher> GetActivePublishers(int resourceStatusId);

        void MarkPublisherNotSaleable(int[] publisherIds, IUser currentUser);
    }
}