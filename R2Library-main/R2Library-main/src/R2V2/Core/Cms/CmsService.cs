#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Core.Cms
{
    public class CmsService
    {
        private const string QuickNotesCacheKey = "Cms.QuickNotes";
        private const string CarouselCacheKey = "Cms.Carousel";
        private const string CmsContentCacheKey = "Cms.Content.{0}";


        private const string DashboardQuickNotesList = "dashboard_quick_notes";
        private const string DashboardQuickNotesListItems = "quick_note_item";
        private const string R2LibraryCarouselList = "r2_carousel";
        private const string R2LibraryCarouselItems = "carousel_item";
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        private readonly KenticoService _kenticoService;
        private readonly ILog<CmsService> _log;

        public CmsService(
            KenticoService kenticoService
            , ILog<CmsService> log
            , IApplicationWideStorageService applicationWideStorageService
        )
        {
            _kenticoService = kenticoService;
            _log = log;
            _applicationWideStorageService = applicationWideStorageService;
        }

        public void ClearCmsCache(List<string> cacheKeysToClear)
        {
            foreach (var key in cacheKeysToClear.Where(key => _applicationWideStorageService.Has(key)))
            {
                _applicationWideStorageService.Remove(key);
            }
        }

        public List<string> GetDashboardQuickNotes()
        {
            List<string> quickNotes = null;
            if (_applicationWideStorageService.Has(QuickNotesCacheKey))
            {
                quickNotes = _applicationWideStorageService.Get<List<string>>(QuickNotesCacheKey);
            }

            if (quickNotes == null)
            {
                try
                {
                    var cmsItems = _kenticoService.GetContentItemsText(DashboardQuickNotesList);
                    quickNotes = cmsItems?.Where(x => x.Type == DashboardQuickNotesListItems).Select(x => x.Value)
                        .ToList();
                }
                catch (Exception ex)
                {
                    var exceptionMesage = $"GetDashboardQuickNotes Error Exception: {ex.Message}";
                    _log.Error(exceptionMesage, ex);
                }

                _applicationWideStorageService.Put(QuickNotesCacheKey, quickNotes);
            }

            return quickNotes;
        }

        public R2LibraryCarousel GetR2LibraryCarousel()
        {
            R2LibraryCarousel carousel = null;
            if (_applicationWideStorageService.Has(CarouselCacheKey))
            {
                carousel = _applicationWideStorageService.Get<R2LibraryCarousel>(CarouselCacheKey);
            }

            if (carousel == null)
            {
                carousel = new R2LibraryCarousel();
                try
                {
                    var cmsItems = _kenticoService.GetContentItems(R2LibraryCarouselList);

                    var kenticoCarouselItems = cmsItems?.Select(x => x)
                        .Where(y => y.ItemName.Type == R2LibraryCarouselItems).ToList();
                    if (kenticoCarouselItems != null)
                    {
                        foreach (var kenticoCarouselItem in kenticoCarouselItems)
                        {
                            var item = new R2LibraryCarouselItem
                            {
                                DestinationUrl = kenticoCarouselItem.ItemValues.ImageDestination.Value,
                                ImageUrl = kenticoCarouselItem.ItemValues.ImageFile.Value[0].ImageUrl,
                                NavigationText = kenticoCarouselItem.ItemValues.NavigationText.Value,
                                NavigationHoverText = kenticoCarouselItem.ItemValues.NavigationHoverText.Value,
                                SortOrder = kenticoCarouselItem.ItemValues.SortOrder.Value
                            };
                            carousel.Items.Add(item);
                        }

                        carousel.Items = carousel.Items.OrderBy(x => x.SortOrder).ToList();
                    }
                }
                catch (Exception ex)
                {
                    var exceptionMesage = $"GetR2LibraryCarousel Error Exception: {ex.Message}";
                    _log.Error(exceptionMesage, ex);
                }

                //carousel.AutoplaySpeedMilliseconds = _clientSettings.CarouselAutoplaySpeedInSeconds * 1000;
                _applicationWideStorageService.Put(CarouselCacheKey, carousel);
            }

            return carousel;
        }

        public string GetHomePageText(string itemName)
        {
            return GetContentString("homepage", itemName);
        }

        public string GetRootPageText(string itemName)
        {
            return GetContentString("root", itemName);
        }

        public CmsData GetDiscoverPageText(string itemName)
        {
            return GetContent("discover", itemName);
        }

        public string GetSystemInformationText(string codeName)
        {
            return GetContentString("system_information", codeName);
        }

        public string GetAccessAndDiscoverabilityText(string codeName)
        {
            return GetContentString("access_and_discoverability", codeName);
        }

        private string GetContentString(string folderName, string itemName)
        {
            string valueToReturn = null;
            try
            {
                var name = itemName.ToLower().Replace(" ", "_");
                var folderAndName = $"{folderName}.{name}";
                var cacheKey = string.Format(CmsContentCacheKey, folderAndName);
                if (_applicationWideStorageService.Has(cacheKey))
                {
                    valueToReturn = _applicationWideStorageService.Get<string>(cacheKey);
                }

                if (valueToReturn == null)
                {
                    var dataItem = _kenticoService.GetContentItemsText(folderName, name);

                    valueToReturn = dataItem?.Value;
                    _applicationWideStorageService.Put(cacheKey, valueToReturn);
                }
            }
            catch (Exception ex)
            {
                var exceptionMesage =
                    $"GetContentString Error with folderName: {folderName} .. itemName: {itemName} || Exception: {ex.Message}";
                _log.Error(exceptionMesage, ex);
            }

            return valueToReturn;
        }

        private CmsData GetContent(string folderName, string itemName)
        {
            CmsData cmsDataItem = null;
            try
            {
                var name = itemName.ToLower().Replace(" ", "_");
                var folderAndName = $"{folderName}.{name}";
                var cacheKey = string.Format(CmsContentCacheKey, folderAndName);
                if (_applicationWideStorageService.Has(cacheKey))
                {
                    cmsDataItem = _applicationWideStorageService.Get<CmsData>(cacheKey);
                }

                if (cmsDataItem == null)
                {
                    cmsDataItem = _kenticoService.GetContentItemsText(folderName, name);
                    _applicationWideStorageService.Put(cacheKey, cmsDataItem);
                }
            }
            catch (Exception ex)
            {
                var exceptionMessage =
                    $"GetContentString Error with folderName: {folderName} .. itemName: {itemName} || Exception: {ex.Message}";
                _log.Error(exceptionMessage, ex);
            }

            return cmsDataItem;
        }
    }
}