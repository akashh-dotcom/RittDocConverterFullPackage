#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PublisherConsolidation
{
    public class PublisherDetail : AdminBaseModel
    {
        public PublisherDetail()
        {
        }

        public PublisherDetail(Publisher publisherToConsolidate, IEnumerable<Publisher> activePublishers,
            IEnumerable<Publisher> childPublishers, IEnumerable<PublisherUser> publisherUsers,
            PublisherUser editPubUser)
        {
            EditPublisher = publisherToConsolidate;

            PublisherUsers = publisherUsers;

            EditPublisherUser = editPubUser;

            ChildPublishers = childPublishers;

            PopulateSelectList(activePublishers);

            ConsolidatedPublisherId = 0;
        }

        public bool DisplayPublisherDelete { get; set; }
        public IEnumerable<Publisher> ChildPublishers { get; set; }

        public Publisher EditPublisher { get; set; }

        public int ConsolidatedPublisherId { get; set; }

        public IEnumerable<PublisherUser> PublisherUsers { get; set; }

        public PublisherUser EditPublisherUser { get; set; }

        [Display(Name = "New publisher name:")]
        public SelectList PublisherSelectList { get; set; }

        private void PopulateSelectList(IEnumerable<Publisher> activePublishers)
        {
            var items = new List<Publisher> { new Publisher { Id = 0, Name = "Select Publisher" } };


            items.AddRange(activePublishers.Select(publisher => new Publisher
            {
                Id = publisher.Id, Name =
                    $"{publisher.Name} -- ResourceCount: {publisher.ResourceCount}"
            }));

            //items.AddRange(activePublishers);
            PublisherSelectList = new SelectList(items, "Id", "Name");
        }
    }
}