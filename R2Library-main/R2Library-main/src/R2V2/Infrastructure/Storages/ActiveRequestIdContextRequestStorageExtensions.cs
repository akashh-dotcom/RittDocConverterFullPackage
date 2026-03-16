#region

using System;

#endregion

namespace R2V2.Infrastructure.Storages
{
    public static class ActiveRequestIdContextRequestStorageExtensions
    {
        private const string ActiveRequestIdKey = "Active.RequestId";

        public static void SetActiveRequestId(this IRequestStorageService storageService, Guid requestId)
        {
            storageService.Put(ActiveRequestIdKey, requestId);
        }

        public static Guid GetActiveRequestId(this IRequestStorageService storageService)
        {
            return storageService.Get<Guid>(ActiveRequestIdKey);
        }

        public static bool HasActiveRequestId(this IRequestStorageService storageService)
        {
            return storageService.Has(ActiveRequestIdKey);
        }
    }
}