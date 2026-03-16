#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Routing;
using R2V2.Core.Admin;
using R2V2.Core.Publisher;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class ActivePublisherList : AdminBaseModel
    {
        public ActivePublisherList(IAdminInstitution institution, IEnumerable<IPublisher> publishers)
            : base(institution)
        {
            var publisherList =
                publishers.Where(x => x.ConsolidatedPublisher == null).Select(y => new ActivePublisher(y));

            var publishersToReturn = publisherList.Where(x => x.ResourceCount > 0);

            Publishers = publishersToReturn;
        }

        public IEnumerable<ActivePublisher> Publishers { get; set; }

        public RouteValueDictionary ToRouteValues(int publisherId)
        {
            return new RouteValueDictionary
            {
                { "InstitutionId", InstitutionId },
                { "PublisherId", publisherId }
            };
        }
    }

    public class ActivePublisher
    {
        public ActivePublisher(IPublisher publisher)
        {
            ResourceCount = publisher.ChildrenResourceCount + publisher.ResourceCount;
            Description = publisher.Description;
            Name = publisher.DisplayName ?? publisher.Name;
            Id = publisher.Id;
        }

        public ActivePublisher(IPublisher publisher, string imageBaseUrl)
        {
            ResourceCount = publisher.ChildrenResourceCount + publisher.ResourceCount;
            Description = publisher.Description;
            Name = publisher.DisplayName ?? publisher.Name;
            Id = publisher.Id;
            ImageFileName = publisher.ImageFileName;

            if (!string.IsNullOrWhiteSpace(publisher.ImageFileName))
            {
                ImageFileName = $"{imageBaseUrl}/{publisher.ImageFileName}";
            }
        }

        [Display(Name = "Resources: ")] public int ResourceCount { get; set; }
        public int Id { get; set; }
        public string ImageFileName { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}