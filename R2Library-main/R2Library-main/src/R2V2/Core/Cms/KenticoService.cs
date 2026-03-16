#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using RestSharp;

#endregion

namespace R2V2.Core.Cms
{
    public class KenticoService
    {
        private static IContentSettings _contentSettings;
        private readonly ILog<KenticoService> _log;
        private readonly RestApiBase _restApiBase;


        public KenticoService(ILog<KenticoService> log, IContentSettings contentSettings)
        {
            _log = log;
            _contentSettings = contentSettings;
            _restApiBase = new RestApiBase(GetRestClient(), contentSettings);
        }


        public IEnumerable<CmsData> GetContentItemsText(string folderName)
        {
            var listItems = GetContentItems(folderName);
            return listItems?.Select(x => x.ToData());
        }

        public CmsData GetContentItemsText(string folderName, string itemCodeName)
        {
            var listItems = GetContentItems(folderName);

            var item = listItems?.FirstOrDefault(x => x.ItemName.CodeName.StartsWith(itemCodeName));
            _log.Debug($"Item that starts with itemCodeName : {item?.ItemName}");
            return item?.ToData();
        }

        public IEnumerable<KenticoItem> GetContentItems(string folderName)
        {
            _log.Debug("GetContentItems");
            var kenticoObject =
                _restApiBase.Get<KenticoItems>($"/items?elements.content_requirement__folder[contains]={folderName}");
            if (kenticoObject == null || kenticoObject.Items == null)
            {
                _log.Error($"Error getting CMS content from folderName: {folderName}. THIS SHOULD NOT HAPPEN!!!");
                return null;
            }

            _log.Debug($"kenticoObject.Items count {kenticoObject?.Items?.Length}");
            var singleItem = FilterToSingleKenticoItem(kenticoObject.Items);

            IEnumerable<KenticoItem> listItems = null;
            //Item has a list of items
            if (kenticoObject.CollectionList != null && singleItem?.ItemValues?.CollectionNames != null)
            {
                listItems = singleItem.ItemValues.CollectionNames.Value
                    .Select(item => kenticoObject.CollectionList.FirstOrDefault(x => x.ItemName.CodeName == item))
                    .Where(listItem =>
                        listItem != null && listItem.ItemValues.ActiveDate.Value <= DateTime.Now &&
                        (listItem.ItemValues.ExpirationDate.Value == null ||
                         listItem.ItemValues.ExpirationDate.Value >= DateTime.Now)
                        && listItem.ItemValues.Environment.Value.Any(x => x.Name == _contentSettings.KenticoEnvironment)
                    ).ToList();
            }
            //Items do not contain a list of items
            else if (singleItem?.ItemValues?.CollectionNames == null)
            {
                listItems = kenticoObject.Items;
            }

            if (listItems != null)
            {
                listItems = FilterToKenticoItems(listItems);
            }

            _log.Debug($"listItems Count: {listItems?.Count()}");
            return listItems;
        }

        private static KenticoItem FilterToSingleKenticoItem(IEnumerable<KenticoItem> kenticoItems)
        {
            kenticoItems = kenticoItems
                //.FilterByName(contentItemName)
                .FilterByDate();

            return kenticoItems.OrderByDescending(x => x.ItemName.LastModified).FirstOrDefault();
        }

        private static IEnumerable<KenticoItem> FilterToKenticoItems(IEnumerable<KenticoItem> kenticoItems)
        {
            return kenticoItems
                //.FilterByName(contentItemName)
                .FilterByDate();
        }

        private static IRestClient GetRestClient()
        {
            var url = string.Format(_contentSettings.KenticoUrl, _contentSettings.KenticoProjectId);
            var client = new RestClient(url);
            return client;
        }
    }

    public static class KenticoExtensitons
    {
        public static IEnumerable<KenticoItem> FilterByDate(this IEnumerable<KenticoItem> items)
        {
            return items.Where(x => x.ItemValues.ActiveDate.Value.GetValueOrDefault().Date <= DateTime.Now.Date && (
                !x.ItemValues.ExpirationDate.Value.HasValue ||
                x.ItemValues.ExpirationDate.Value.Value.Date >= DateTime.Now.Date));
        }
    }
}