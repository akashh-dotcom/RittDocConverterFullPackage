#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Core.CollectionManagement;
using R2V2.Core.SuperType;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Menus
{
    public class ActionsMenu
    {
        public int InstitutionId { get; set; }

        public IQuery Query { get; set; }

        public SearchMenu SearchMenu { get; set; }

        public PageLinkSection Sorts { get; set; }

        public IDictionary<Type, PageLinkSection> Filters { get; } = new Dictionary<Type, PageLinkSection>();

        public IDictionary<Type, PageLinkSection> SecondRowFilters { get; } = new Dictionary<Type, PageLinkSection>();

        //public Order.Order Order { get; set; }

        //public EmailPage EmailPage { get; set; }

        public int CartId { get; set; }
        public decimal CartTotal { get; set; }
        public int CartItemCount { get; set; }

        public bool DisplayCartLink { get; set; }

        public bool DisplaySavedCartLink { get; set; }

        public List<CachedCart> AllCarts { get; set; }

        public int SavedCartsCount { get; set; }

        public ToolLinks ToolLinks { get; set; } = new ToolLinks();

        public List<SelectListItem> ResellerCartList { get; set; }

        public string ResellerLinkHref { get; set; }

        public void AddFilter(Type type, PageLinkSection filterLinks)
        {
            Filters[type] = filterLinks;
            //Filters.ElementAt(1).Key = null;
        }

        public void SecondRowFilter(Type type, PageLinkSection filterLinks)
        {
            SecondRowFilters[type] = filterLinks;
        }
    }
}