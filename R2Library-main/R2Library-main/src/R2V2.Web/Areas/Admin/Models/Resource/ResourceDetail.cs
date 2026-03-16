#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.Promotion;
using R2V2.Core.Resource;
using R2V2.Web.Areas.Admin.Models.Special;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public class ResourceDetail : AdminBaseModel
    {
        /// <summary>
        /// </summary>
        public ResourceDetail()
        {
            Resource = new Resource();
        }

        /// <param name="webSettings"> </param>
        /// <param name="user"> </param>
        public ResourceDetail(IResource resource, IFeaturedTitle featuredTitle, ResourceQuery resourceQuery,
            IWebSettings webSettings, IUser user,
            SpecialResourceModel specialResource, IList<ResourcePromoteQueue> resourcePromoteQueues,
            IEnumerable<Core.Authentication.User> raPromotionUsers)
        {
            Init(resource, featuredTitle, resourceQuery, webSettings, user, specialResource, resourcePromoteQueues,
                raPromotionUsers);
        }

        public Resource Resource { get; private set; }

        public ResourceQuery ResourceQuery { get; set; }

        public SpecialResourceModel SpecialResource { get; set; }

        public bool DisplayPromotionFields { get; private set; }
        public bool DisplayAddToPromoteQueueButtons { get; private set; }
        public bool DisplayRemovedFromPromoteQueueButtons { get; private set; }
        public bool PromotionStatus { get; set; }
        public string ActionMessage { get; set; }

        public bool DisplayOngoingPdaEventLinks { get; private set; }

        public void Init(IResource resource, IFeaturedTitle featuredTitle, ResourceQuery resourceQuery,
            IWebSettings webSettings, IUser user,
            SpecialResourceModel specialResource, IList<ResourcePromoteQueue> resourcePromoteQueues,
            IEnumerable<Core.Authentication.User> raPromotionUsers)
        {
            Resource = specialResource != null
                ? new Resource(resource, featuredTitle, specialResource.SpecialText, specialResource.IconName)
                : new Resource(resource, featuredTitle, null, null);

            SetResourcePromotionQueueStatus(resourcePromoteQueues, raPromotionUsers);

            ResourceQuery = resourceQuery;

            DisplayPromotionFields = user.IsRittenhouseAdmin() && webSettings.DisplayPromotionFields;

            var resourcePromotionQueue = resourcePromoteQueues == null
                ? null
                : resourcePromoteQueues.FirstOrDefault(x => x.ResourceId == resource.Id);

            var showPromoteButton = user.IsRittenhouseAdmin() && user.EnablePromotion != null &&
                                    user.EnablePromotion.Value > 0 && webSettings.EnablePromotionToProduction &&
                                    (resource.ListPrice >= webSettings.ResourceMinimumPromotionPrice ||
                                     resource.IsFreeResource);

            if (resourcePromotionQueue == null)
            {
                DisplayAddToPromoteQueueButtons = showPromoteButton;
                DisplayRemovedFromPromoteQueueButtons = false;
            }
            else
            {
                DisplayRemovedFromPromoteQueueButtons = showPromoteButton;
                DisplayAddToPromoteQueueButtons = false;
            }

            DisplayOngoingPdaEventLinks = user.IsRittenhouseAdmin() && webSettings.DisplayOngoingPdaEventLinks &&
                                          resource.IsActive() &&
                                          (resource.ListPrice >= webSettings.ResourceMinimumPromotionPrice ||
                                           resource.IsFreeResource);
        }

        private void SetResourcePromotionQueueStatus(IEnumerable<ResourcePromoteQueue> resourcePromoteQueues,
            IEnumerable<Core.Authentication.User> raPromotionUsers)
        {
            if (resourcePromoteQueues == null || raPromotionUsers == null)
            {
                return;
            }

            var resourcePromotionQueue = resourcePromoteQueues.FirstOrDefault(x => x.ResourceId == Resource.Id);

            if (resourcePromotionQueue == null)
            {
                return;
            }

            var user = raPromotionUsers.FirstOrDefault(x => x.Id == resourcePromotionQueue.AddedByUserId);
            Resource.PromotionQueueStatus = string.Format("Add to queue by {0} on {1}",
                user == null ? $"User Id: {resourcePromotionQueue.AddedByUserId}" : user.ToFullName(),
                resourcePromotionQueue.CreationDate);
        }
    }
}