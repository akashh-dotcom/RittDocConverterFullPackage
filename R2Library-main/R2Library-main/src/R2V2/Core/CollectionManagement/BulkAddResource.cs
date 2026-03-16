#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class BulkAddResource
    {
        public int InstitutionId { get; set; }
        public int ResourceId { get; set; }
        public int NumberOfLicenses { get; set; }
        public LicenseOriginalSource OriginalSource { get; set; }
    }
}