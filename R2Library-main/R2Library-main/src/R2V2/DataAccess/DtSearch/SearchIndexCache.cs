#region

using dtSearch.Engine;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public static class SearchIndexCache
    {
        public static IndexCache IndexCache { get; } = new IndexCache(25)
        {
            AutoReopenTime = -1
        };
    }
}