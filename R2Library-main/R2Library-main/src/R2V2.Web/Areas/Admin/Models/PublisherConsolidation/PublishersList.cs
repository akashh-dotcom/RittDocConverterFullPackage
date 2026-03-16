#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PublisherConsolidation
{
    public class PublishersList : AdminBaseModel
    {
        public bool DisplayAddPublisher { get; set; }

        public IEnumerable<MainPublisher> Publishers { get; private set; }

        public void SetPublishers(IList<Publisher> publishers)
        {
            IList<MainPublisher> mainPublishers =
                publishers.Where(x => x.ConsolidatedPublisher == null).Select(y => new MainPublisher
                {
                    Name = y.Name,
                    VendorNumber = y.VendorNumber,
                    City = y.City,
                    State = y.State,
                    CityAndState = y.CityAndState,
                    RecordStatus = y.RecordStatus,
                    IsFeaturedPublisher = y.IsFeaturedPublisher,
                    ResourceCount = y.ResourceCount,
                    Id = y.Id,
                    NotSaleableDate = y.NotSaleableDate
                }).ToList();
            foreach (var mainPublisher in mainPublishers)
            {
                var mainPublisherId = mainPublisher.Id;
                var childPublishers = publishers.Where(x =>
                    x.ConsolidatedPublisher != null && x.ConsolidatedPublisher.Id == mainPublisherId);

                if (childPublishers.Any())
                {
                    mainPublisher.ChildPublishers = childPublishers.ToList();
                }
            }

            Publishers = mainPublishers;
        }
    }

    public class MainPublisher : Publisher
    {
        public IEnumerable<Publisher> ChildPublishers { get; set; }

        public string RowClass()
        {
            if (IsFeaturedPublisher && !NotSaleableDate.HasValue)
            {
                return "selected";
            }

            return NotSaleableDate.HasValue ? "notsaleable" : "";
        }
    }
}