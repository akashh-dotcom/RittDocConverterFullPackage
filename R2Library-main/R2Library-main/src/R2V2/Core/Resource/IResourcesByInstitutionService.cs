#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource
{
    public interface IResourcesByInstitutionService
    {
        IList<IResource> GetResourcesForActiveInstitution(string guestAccountNumber, bool tocAvailable,
            int collectionId = 0);
    }
}