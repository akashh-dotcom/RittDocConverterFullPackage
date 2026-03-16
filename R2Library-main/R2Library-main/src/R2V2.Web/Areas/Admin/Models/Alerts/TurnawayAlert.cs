#region

using System;
using System.Web.Routing;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Alerts
{
    [Serializable]
    public class TurnawayAlert : AdminBaseModel
    {
        public TurnawayAlert(int institutionId, int titleCount, DateTime? lastAlert)
        {
            AlertDate = lastAlert == null ? DateTime.Now.AddDays(-30) : lastAlert.Value;
            TitleCount = titleCount;
            OlderThen30Days = AlertDate > DateTime.Now.AddDays(-30);

            RouteValues = new RouteValueDictionary(new { Area = "Admin" })
            {
                { "InstitutionId", institutionId },
                { "TurnawayStartDate", AlertDate.ToString("d") },
                //ResourceStatus=Active
                { "ResourceStatus", ResourceStatus.Active }
            };
        }

        public DateTime AlertDate { get; set; }

        public int TitleCount { get; set; }

        public bool OlderThen30Days { get; set; }

        public RouteValueDictionary RouteValues { get; set; }
    }
}