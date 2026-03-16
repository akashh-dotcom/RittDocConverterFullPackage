#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.Resource
{
    public interface IResourceAccessService
    {
        ResourceAccess GetResourceAccess(string isbn);
        ResourceAccess GetResourceAccessForToc(string isbn);
        bool IsPdaResource(string isbn);

        void ClearSessionResourceLocks(); // Remove any resource locks for the current session.
        void CleanupResourceLocks(); // Remove any resource locks for expired sessions. 

        bool IsFullTextAvailable(int resourceId);
        bool IsFullTextAvailable(License license);

        LicenseType GetLicenseType(int resourceId);
    }
}