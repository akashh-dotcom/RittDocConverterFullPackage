#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionResourceLockMap : BaseMap<InstitutionResourceLock>
    {
        public InstitutionResourceLockMap()
        {
            Table("tInstitutionResourceLock");

            Id(x => x.Id).Column("iInstitutionResourceLockId").GeneratedBy.Identity();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.UserId).Column("iUserId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.LockType).Column("iInstitutionResourceLockTypeId").CustomType<LockType>();
            Map(x => x.LockStartDate).Column("dtLockStartDate");
            Map(x => x.LockEndDate).Column("dtLockEndDate");
            Map(x => x.LockEmailAlertTimestamp).Column("dtLockEmailAlertTimestamp");
            Map(x => x.LockData).Column("vchLockData").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.LockEmailAlertData).Column("vchLockEmailAlertData");
        }
    }
}