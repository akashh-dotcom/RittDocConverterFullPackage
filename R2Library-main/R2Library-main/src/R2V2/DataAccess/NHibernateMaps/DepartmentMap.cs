#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DepartmentMap : BaseMap<Department>
    {
        public DepartmentMap()
        {
            Table("dbo.tDept");
            Id(x => x.Id, "iDeptId").GeneratedBy.Identity();
            Map(x => x.Code, "vchDeptCode");
            Map(x => x.Name, "vchDeptName");
            Map(x => x.List, "tiList");
        }
    }
}