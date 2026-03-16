#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Web.Areas.Admin.Models.Special;
using R2V2.Web.Infrastructure.Settings;
using Directory = System.IO.Directory;
using Resource = R2V2.Web.Areas.Admin.Models.Resource.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class SpecialDiscountResourceService
    {
        private readonly IResourceService _resourceService;
        private readonly SpecialDiscountResourceFactory _specialDiscountResourceFactory;
        private readonly IWebImageSettings _webImageSettings;

        public SpecialDiscountResourceService(
            SpecialDiscountResourceFactory specialDiscountResourceFactory
            , IWebImageSettings webImageSettings
            , IResourceService resourceService
        )
        {
            _specialDiscountResourceFactory = specialDiscountResourceFactory;
            _webImageSettings = webImageSettings;
            _resourceService = resourceService;
        }


        public List<SpecialAdminModel> GetAvailableAdminSpecials()
        {
            var specials = _specialDiscountResourceFactory.GetNonExpiredSpecials();

            var iconNames = GetIconUrls();

            var specialAdminModels = (from special in specials
                let specialModel = new SpecialModel(special)
                let specialDiscountModels = special.Discounts.Where(y => y.RecordStatus)
                    .Select(x => new SpecialDiscountModel(x, iconNames))
                select
                    new SpecialAdminModel(specialModel,
                        specialDiscountModels.Any() ? specialDiscountModels.ToList() : null)).ToList();
            return specialAdminModels;
        }

        public List<SpecialAdminModel> GetAllAdminSpecials()
        {
            var specials = _specialDiscountResourceFactory.GetAllSpecials();

            var iconNames = GetIconUrls();

            var specialAdminModels = (from special in specials
                let specialModel = new SpecialModel(special)
                let specialDiscountModels = special.Discounts.Where(y => y.RecordStatus)
                    .Select(x => new SpecialDiscountModel(x, iconNames))
                select
                    new SpecialAdminModel(specialModel,
                        specialDiscountModels.Any() ? specialDiscountModels.ToList() : null)).ToList();
            return specialAdminModels;
        }

        //GetAllSpecials

        /// <summary>
        ///     Will Return Resource Specials Active and Future
        /// </summary>
        public List<SpecialResourceModel> GetSpecialResourcesForAdminResource()
        {
            IEnumerable<CachedSpecialResource> specialResourcesDiscount =
                _specialDiscountResourceFactory.GetSpecialResourcesDiscountForAdminResource();

            return specialResourcesDiscount != null
                ? specialResourcesDiscount
                    .Select(x => new SpecialResourceModel(x, new Uri(_webImageSettings.SpecialIconBaseUrl))).ToList()
                : null;
        }

        public List<SpecialResourceModel> GetSpecialResourcesDiscountForSpecialController()
        {
            IEnumerable<CachedSpecialResource> specialResourcesDiscount =
                _specialDiscountResourceFactory.GetCachedSpecialResourceForSpecialController();

            return specialResourcesDiscount != null
                ? specialResourcesDiscount
                    .Select(x => new SpecialResourceModel(x, new Uri(_webImageSettings.SpecialIconBaseUrl))).ToList()
                : null;
        }


        public List<SpecialResourceModel> GetResourceSpecials(int resourceId)
        {
            var specialResourceList = GetSpecialResourcesForAdminResource();
            var specialResourceModels = specialResourceList != null
                ? specialResourceList.Where(x => x.ResourceId == resourceId)
                : null;
            return specialResourceModels != null ? specialResourceModels.ToList() : null;
        }

        /// <summary>
        ///     Will Return only Active Resource Specials
        /// </summary>
        public List<SpecialResourceModel> GetSpecialResourceModels()
        {
            IEnumerable<CachedSpecialResource> specialResourcesDiscount =
                _specialDiscountResourceFactory.GetSpecialResourcesDiscount();

            return specialResourcesDiscount != null
                ? specialResourcesDiscount
                    .Select(x => new SpecialResourceModel(x, new Uri(_webImageSettings.SpecialIconBaseUrl))).ToList()
                : null;
        }

        public SpecialResourceModel GetResourceSpecial(int resourceId)
        {
            var cachedSpecialResource = _specialDiscountResourceFactory.GetCachedSpecialResource(resourceId);

            return cachedSpecialResource != null
                ? new SpecialResourceModel(cachedSpecialResource, new Uri(_webImageSettings.SpecialIconBaseUrl))
                : null;
        }

        public List<CachedSpecialResource> GetSpecialDiscountResources()
        {
            return _specialDiscountResourceFactory.GetSpecialResourcesDiscount();
        }


        public SpecialView GetSpecialView(int specialId)
        {
            var resources = _resourceService.GetAllResources().ToList();
            var specialDiscountResources =
                _specialDiscountResourceFactory.GetCachedSpecialResourceBySpecialId(specialId);
            List<SpecialDiscountResourceModel> specialDiscountResourceModels = null;

            if (specialDiscountResources != null)
            {
                var iconBaseUrl = new Uri(_webImageSettings.SpecialIconBaseUrl);
                specialDiscountResourceModels = (from item in specialDiscountResources
                    let specialResourceModel = new SpecialResourceModel(item, iconBaseUrl)
                    let resource = resources.FirstOrDefault(x => x.Id == item.ResourceId)
                    let webResource = new Resource(resource, null, specialResourceModel.SpecialText,
                        specialResourceModel.IconName)
                    where resource != null
                    select new SpecialDiscountResourceModel(item.SpecialDiscountResourceId, item.SpecialDiscountId,
                        webResource)).ToList();
            }

            var special = _specialDiscountResourceFactory.GetAllSpecials().FirstOrDefault(x => x.Id == specialId);
            var specialModel = new SpecialModel(special);

            var iconUrls = GetIconUrls();
            IEnumerable<SpecialDiscountModel> specialDiscountModels = null;
            var specialDiscounts = _specialDiscountResourceFactory.GetSpecialDiscounts(specialId);
            if (specialDiscounts != null)
            {
                specialDiscountModels = specialDiscounts.Select(x => new SpecialDiscountModel(x, iconUrls));
            }

            var specialdiscounts = specialDiscountModels != null ? specialDiscountModels.ToList() : null;

            return new SpecialView(_webImageSettings.SpecialIconBaseUrl, specialModel, specialdiscounts,
                specialDiscountResourceModels);
        }

        public SpecialView GetSpecialViewWithNewDiscount(int specialId)
        {
            var model = GetSpecialView(specialId);
            model.SetNewDiscount(GetIconUrls());
            return model;
        }

        public SpecialView GetSpecialViewWithEditDiscount(int specialId, int specialDiscountId)
        {
            var model = GetSpecialView(specialId);
            model.SetEditDiscount(specialDiscountId);
            return model;
        }

        #region Database Save, Update, Delete

        public int SaveSpecial(SpecialView specialView)
        {
            var specialId = SaveSpecialAndGetId(specialView.Special);
            return specialId;
        }

        public bool SaveSpecialAndSpecialDiscount(SpecialView specialView)
        {
            var specialSaved = SaveSpecial(specialView.Special);
            if (specialSaved && specialView.EditSpecialDiscount != null)
            {
                specialView.EditSpecialDiscount.IconName = GetIconName(specialView.EditSpecialDiscount.SelectIconIndex);
                var discountSaved = SaveSpecialDiscount(specialView.EditSpecialDiscount);
                return discountSaved;
            }

            return specialSaved;
        }

        public bool SaveSpecial(SpecialModel specialModel)
        {
            var dbSpecial = _specialDiscountResourceFactory.GetSpecial(specialModel.Id) ?? new Special();

            dbSpecial.StartDate = specialModel.StartDate;
            dbSpecial.EndDate =
                specialModel.EndDate.AddDays(1).AddMinutes(-1); //Will make it 11L59pm on the day stated.
            dbSpecial.Name = specialModel.Name;
            dbSpecial.RecordStatus = true;
            //SaveSpecialAndGetId
            return _specialDiscountResourceFactory.SaveSpecial(dbSpecial);
        }

        public int SaveSpecialAndGetId(SpecialModel specialModel)
        {
            var dbSpecial = _specialDiscountResourceFactory.GetSpecial(specialModel.Id) ?? new Special();

            dbSpecial.StartDate = specialModel.StartDate;
            dbSpecial.EndDate =
                specialModel.EndDate.AddDays(1).AddMinutes(-1); //Will make it 11L59pm on the day stated.
            dbSpecial.Name = specialModel.Name;
            dbSpecial.RecordStatus = true;

            return _specialDiscountResourceFactory.SaveSpecialAndGetId(dbSpecial);
        }

        public bool DeleteSpecial(int specialId)
        {
            var dbSpecial = _specialDiscountResourceFactory.GetSpecial(specialId);
            return _specialDiscountResourceFactory.DeleteSpecial(dbSpecial);
        }

        public bool SaveSpecialDiscount(SpecialDiscountModel specialDiscountModel)
        {
            var specialDiscount = new SpecialDiscount
            {
                DiscountPercentage = specialDiscountModel.DiscountPercentage,
                IconName = specialDiscountModel.IconName,
                Id = specialDiscountModel.Id,
                SpecialId = specialDiscountModel.SpecialId,
                RecordStatus = true
            };
            var success = _specialDiscountResourceFactory.SaveSpecialDiscount(specialDiscount);
            return success;
        }

        public bool DeleteSpecialDiscount(int specialDiscountId)
        {
            var specialDiscount = _specialDiscountResourceFactory.GetSpecialDiscount(specialDiscountId);
            return _specialDiscountResourceFactory.DeleteSpecialDiscount(specialDiscount);
        }

        public int DeleteSpecialResource(int specialDiscountId, int specialResourceId)
        {
            var success = _specialDiscountResourceFactory.DeleteSpecialResource(specialResourceId);
            if (success)
            {
                var specialDiscount = _specialDiscountResourceFactory.GetSpecialDiscount(specialDiscountId);
                return specialDiscount.SpecialId;
            }

            return 0;
        }

        public bool UpdateResourceSpecials(int resourceId, int[] specialDiscountIds)
        {
            var specialResourceDiscounts = _specialDiscountResourceFactory.GetSpecialsResource(resourceId).ToList();

            var specialResourceDiscountsToDelete = specialDiscountIds != null
                ? specialResourceDiscounts.Where(item => !specialDiscountIds.Contains(item.DiscountId)).ToList()
                : specialResourceDiscounts.ToList();

            var deleteSuccess =
                _specialDiscountResourceFactory.DeleteSpecialsResource(specialResourceDiscountsToDelete);
            var saveSuccess = true;
            if (specialDiscountIds != null)
            {
                var specialResourcesToAdd = (from specialDiscountId in specialDiscountIds
                    where specialResourceDiscounts.All(x => x.DiscountId != specialDiscountId)
                    select new SpecialResource
                        { ResourceId = resourceId, DiscountId = specialDiscountId, RecordStatus = true }).ToList();

                saveSuccess = _specialDiscountResourceFactory.SaveSpecialsResource(specialResourcesToAdd);
            }

            return saveSuccess && deleteSuccess;
        }

        public bool AddResourcesToSpecial(int[] resourceIds, int specialDiscountId, IUser currentUser)
        {
            return _specialDiscountResourceFactory.InsertSpecialResources(resourceIds, specialDiscountId, currentUser);
        }

        public List<int> GetExcludedResourceIdsForSpecial(int specialDiscountId, IEnumerable<int> resourceIds)
        {
            var currentSpecialResources = GetSpecialDiscountResources();

            if (currentSpecialResources == null)
            {
                return null;
            }

            var resourceIdsToReturn = from currentSpecialResource in currentSpecialResources
                where resourceIds.Contains(currentSpecialResource.ResourceId)
                select currentSpecialResource.ResourceId;


            return resourceIdsToReturn.Any() ? resourceIdsToReturn.ToList() : null;
        }

        #endregion

        #region Icon Area

        public string GetIconName(int iconIndex)
        {
            var iconNames = new List<string>();
            if (Directory.Exists(_webImageSettings.SpecialIconDirectory))
            {
                var iconFiles =
                    Directory.GetFiles(_webImageSettings.SpecialIconDirectory, "*.*")
                        .Where(x =>
                            x.ToLower().EndsWith("png") || x.ToLower().EndsWith("jpg") ||
                            x.ToLower().EndsWith("jpeg") || x.ToLower().EndsWith("gif"));
                iconNames.AddRange(
                    iconFiles.Select(iconFile => new FileInfo(iconFile))
                        .Select(fileInfo => fileInfo.Name));
            }

            return iconIndex >= 0 ? iconNames[iconIndex] : null;
        }

        private List<string> GetIconUrls()
        {
            var iconUrls = new List<string>();
            if (Directory.Exists(_webImageSettings.SpecialIconDirectory))
            {
                var iconFiles =
                    Directory.GetFiles(_webImageSettings.SpecialIconDirectory, "*.*")
                        .Where(x =>
                            x.ToLower().EndsWith("png") || x.ToLower().EndsWith("jpg") ||
                            x.ToLower().EndsWith("jpeg") || x.ToLower().EndsWith("gif"));
                iconUrls.AddRange(
                    iconFiles.Select(iconFile => new FileInfo(iconFile))
                        .Select(fileInfo => Path.Combine(_webImageSettings.SpecialIconBaseUrl, fileInfo.Name)));
            }

            return iconUrls;
        }

        #endregion
    }
}