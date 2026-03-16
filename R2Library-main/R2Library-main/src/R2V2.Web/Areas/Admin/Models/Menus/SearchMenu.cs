#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Menus
{
    public class SearchMenu
    {
        public string Label { get; set; }

        public IQuery Query { get; set; }
    }
}