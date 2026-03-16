#region

using System;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public static class ResourceExtensions
    {
        public static FeaturedTitle ToFeaturedTitle(this FeaturedTitle editFeaturedTitle, int resourceId,
            DateTime? startDate, DateTime? endDate, bool recordStatus)
        {
            if (editFeaturedTitle == null)
            {
                editFeaturedTitle = new FeaturedTitle();
            }

            editFeaturedTitle.ResourceId = resourceId;
            editFeaturedTitle.StartDate = startDate;
            editFeaturedTitle.EndDate = endDate;
            editFeaturedTitle.RecordStatus = recordStatus;
            return editFeaturedTitle;
        }
    }
}