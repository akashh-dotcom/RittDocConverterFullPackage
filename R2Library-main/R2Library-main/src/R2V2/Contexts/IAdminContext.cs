#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Contexts
{
    public interface IAdminContext
    {
        IAdminInstitution GetAdminInstitution(int institutionId);
        void ReloadAdminInstitution(int institutionId, int userId);
    }
}