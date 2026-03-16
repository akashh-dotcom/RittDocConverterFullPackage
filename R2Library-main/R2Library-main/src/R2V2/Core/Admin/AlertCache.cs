#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Core.Admin
{
    [Serializable]
    public class AlertCache
    {
        private readonly IList<CachedAlert> _alerts;


        public AlertCache(
            IEnumerable<IAdminAlert> administratorAlerts
        )
        {
            _alerts = new List<CachedAlert>();
            foreach (var cachedAlert in administratorAlerts.Select(administratorAlert =>
                         new CachedAlert(administratorAlert)))
            {
                _alerts.Add(cachedAlert);
            }
        }

        public IAdminAlert GetAlertWithExcludeAndRoleId(List<int> idsToExclude, int roleId,
            AuthenticatedInstitution institution, CachedCart cart, List<int> activeAndForthcomingResourceIds)
        {
            var alerts = _alerts.Where(x => !idsToExclude.Contains(x.Id) && x.RoleId == roleId);
            alerts = alerts.Where(x =>
                x.Layout != AlertLayout.ResourceAndText ||
                activeAndForthcomingResourceIds.Contains(x.ResourceId.GetValueOrDefault(0)));

            if (institution != null)
            {
                if (institution.AccountStatus != InstitutionAccountStatus.Trial)
                {
                    var purchasedResourceIds = institution.Licenses.Where(x => x.LicenseType == LicenseType.Purchased)
                        .Select(x => x.ResourceId).ToList();
                    alerts = alerts.Where(x =>
                        x.Layout != AlertLayout.ResourceAndText ||
                        !purchasedResourceIds.Contains(x.ResourceId.GetValueOrDefault(0)));

                    var pdaResourceIds = institution.Licenses
                        .Where(x => x.LicenseType == LicenseType.Pda && !x.PdaCartDeletedDate.HasValue)
                        .Select(x => x.ResourceId).ToList();

                    alerts = alerts.Where(x =>
                        x.Layout != AlertLayout.ResourceAndText ||
                        (!pdaResourceIds.Contains(x.ResourceId.GetValueOrDefault(0)) && x.AllowPDA) ||
                        !x.AllowPDA);
                }

                var resourceIdsInCart = cart?.CartItems?.Where(x => x.ResourceId.HasValue)
                    .Select(x => x.ResourceId.Value).ToList();
                if (resourceIdsInCart != null)
                {
                    alerts = alerts.Where(x =>
                        x.Layout != AlertLayout.ResourceAndText ||
                        !resourceIdsInCart.Contains(x.ResourceId.GetValueOrDefault(0)));
                }
            }

            return alerts.OrderByDescending(x => x.DisplayOnce).FirstOrDefault();
        }

        public IEnumerable<IAdminAlert> GetAllAlerts()
        {
            return _alerts;
        }
    }

    [Serializable]
    public class CachedAlert : IAdminAlert
    {
        public CachedAlert(IAdminAlert administratorAlert)
        {
            Id = administratorAlert.Id;
            DisplayOnce = administratorAlert.DisplayOnce;
            Title = administratorAlert.Title;
            Text = administratorAlert.Text;
            CreatedBy = administratorAlert.CreatedBy;
            UpdatedBy = administratorAlert.UpdatedBy;
            CreationDate = administratorAlert.CreationDate;
            LastUpdated = administratorAlert.LastUpdated == null
                ? administratorAlert.CreationDate
                : administratorAlert.LastUpdated.GetValueOrDefault(DateTime.Now);

            Layout = administratorAlert.Layout;
            RecordStatus = administratorAlert.RecordStatus;
            AlertImages = administratorAlert.AlertImages.Where(x => x.RecordStatus);

            StartDate = administratorAlert.StartDate;
            EndDate = administratorAlert.EndDate;
            AlertName = administratorAlert.AlertName;
            Role = administratorAlert.Role;
            RoleId = administratorAlert.RoleId;

            ResourceId = administratorAlert.ResourceId;
            AllowPurchase = administratorAlert.AllowPurchase;
            AllowPDA = administratorAlert.AllowPDA;
        }

        public bool RecordStatus { get; set; }

        public bool DisplayOnce { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public AlertLayout Layout { get; set; }
        public IEnumerable<AlertImage> AlertImages { get; set; }
        public int? ResourceId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AlertName { get; set; }
        public Role Role { get; set; }
        public int RoleId { get; set; }

        public bool AllowPurchase { get; set; }
        public bool AllowPDA { get; set; }
    }
}