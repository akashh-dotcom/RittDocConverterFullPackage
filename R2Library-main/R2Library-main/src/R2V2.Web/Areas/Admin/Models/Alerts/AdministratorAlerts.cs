#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Resource;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Alerts
{
    public class AdministratorAlerts : AdminBaseModel
    {
        public AdministratorAlerts(IEnumerable<IAdminAlert> alerts, string imageLocation, List<IResource> resources)
        {
            Alerts = new List<AdministratorAlert>();
            foreach (var adminAlert in alerts)
            {
                IResource resource = null;
                if (adminAlert.ResourceId.HasValue)
                {
                    resource = resources.FirstOrDefault(x => x.Id == adminAlert.ResourceId.Value);
                }

                Alerts.Add(new AdministratorAlert(adminAlert, imageLocation, resource));
            }
        }

        public List<AdministratorAlert> Alerts { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }
    }
}