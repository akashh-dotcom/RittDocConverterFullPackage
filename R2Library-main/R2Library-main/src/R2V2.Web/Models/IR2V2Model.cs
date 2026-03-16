#region

using System;
using System.Collections.Generic;
using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Models.Alerts;
using R2V2.Web.Models.Search;

#endregion

namespace R2V2.Web.Models
{
    public interface IR2V2Model
    {
        AuthenticationInfo AuthenticationInfo { get; set; }
        Layout Layout { get; set; }
        Header Header { get; set; }
        Footer Footer { get; set; }
        SortedList<int, HeaderTab> Tabs { get; set; }
        string Q { get; set; }

        string EnvironmentName { get; set; }
        string DebugInformation { get; set; }

        AdvancedSearchModel AdvancedSearch { get; set; }

        AdministratorAlert Alert { get; set; }

        TurnawayAlert ConcurrentTurnawayAlert { get; set; }

        bool DisplayAskYourLibrarian { get; set; }

        bool DisplayConfigurationLink { get; set; }

        string SearchUrl { get; set; }

        string R2CmsContentUrl { get; set; }

        bool DisplaySearch { get; set; }
        bool DisplayAdvancedSearch { get; set; }
        string SearchHintPlaceHolderText { get; set; }
        bool SearchTypeaheadEnabled { get; set; }

        bool PurchasedOnly { get; set; }
        bool IncludePdaResources { get; set; }
        bool IncludePdaHistory { get; set; }
        int CollectionListFilter { get; set; }
        bool IncludeFreeResources { get; set; }
        bool IncludeSpecialDiscounts { get; set; }
        bool RecommendationsOnly { get; set; }
        ResourceListType ResourceListType { get; set; }

        DateTime DateRangeStart { get; set; }
        DateTime DateRangeEnd { get; set; }

        int ReviewId { get; set; }

        int ReserveShelfId { get; set; }
    }
}