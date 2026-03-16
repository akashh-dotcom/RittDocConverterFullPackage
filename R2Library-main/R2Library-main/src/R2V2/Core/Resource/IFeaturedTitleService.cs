#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource
{
    public interface IFeaturedTitleService
    {
        IEnumerable<IFeaturedTitle> GetFeaturedTitles();

        IEnumerable<IFeaturedTitle> GetFeaturedTitles(bool forceReload, ResourceCache resourceCache);

        void SaveFeaturedTitle(FeaturedTitle featuredTitle);
        void DeleteFeaturedTitle(FeaturedTitle featuredTitle);

        IFeaturedTitle GetFeaturedTitle(int resourceId);
        FeaturedTitle GetFeaturedTitleForEdit(int resourceId);
    }
}