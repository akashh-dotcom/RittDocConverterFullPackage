#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaResourceViewMap : BaseMap<PdaResourceView>
    {
        public PdaResourceViewMap()
        {
            // select iInstitutionPdaResourceViewId, iInstitutionId, iUserId, iResourceId, dtTimestamp, tiRecordStatus from tInstitutionPdaResourceView
            Table("tInstitutionPdaResourceView");

            Id(x => x.Id).Column("iInstitutionPdaResourceViewId").GeneratedBy.Identity();
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.UserId, "iUserId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.Timestamp, "dtTimestamp");
        }
    }
}