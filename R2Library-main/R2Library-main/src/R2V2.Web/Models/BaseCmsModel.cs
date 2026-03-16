#region

using System;
using System.Collections.Generic;
using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Models.Alerts;
using R2V2.Web.Models.Search;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public abstract class BaseCmsModel : IR2V2Model
    {
        public AuthenticationInfo AuthenticationInfo { get; set; }
        public Layout Layout { get; set; }
        public Header Header { get; set; }
        public Footer Footer { get; set; }
        public SortedList<int, HeaderTab> Tabs { get; set; }
        public string Q { get; set; }
        public string EnvironmentName { get; set; }
        public string DebugInformation { get; set; }

        public AdvancedSearchModel AdvancedSearch { get; set; }

        public AdministratorAlert Alert { get; set; }
        public TurnawayAlert ConcurrentTurnawayAlert { get; set; }

        public bool DisplayAskYourLibrarian { get; set; }
        public bool DisplayConfigurationLink { get; set; }

        public string R2CmsContentUrl { get; set; }

        public string SearchUrl { get; set; }

        public bool DisplaySearch { get; set; }
        public bool DisplayAdvancedSearch { get; set; }
        public string SearchHintPlaceHolderText { get; set; }
        public bool SearchTypeaheadEnabled { get; set; }

        public bool PurchasedOnly { get; set; }
        public bool IncludePdaResources { get; set; }
        public bool IncludePdaHistory { get; set; }
        public int CollectionListFilter { get; set; }
        public bool IncludeFreeResources { get; set; }
        public bool IncludeSpecialDiscounts { get; set; }
        public bool RecommendationsOnly { get; set; }
        public ResourceListType ResourceListType { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public int ReviewId { get; set; }
        public int ReserveShelfId { get; set; }
    }
}