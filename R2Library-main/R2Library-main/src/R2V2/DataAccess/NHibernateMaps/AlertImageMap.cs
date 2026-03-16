#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class AlertImageMap : BaseMap<AlertImage>
    {
        public AlertImageMap()
        {
            Table("tAlertImage");
            Id(x => x.Id).Column("iAlertImageId").GeneratedBy.Identity();
            Map(x => x.AlertId).Column("iAlertId");
            Map(x => x.ImageFileName).Column("vchImageFileName");
        }
    }
}