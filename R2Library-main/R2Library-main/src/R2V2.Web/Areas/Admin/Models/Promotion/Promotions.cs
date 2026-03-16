#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Promotion
{
    public class Promotions : AdminBaseModel
    {
        public Promotions(IEnumerable<CachedPromotion> promotions, IEnumerable<Product> products)
        {
            List = promotions.Select(x => new PromotionModel(x, products)).ToList();
        }

        public List<PromotionModel> List { get; set; }
    }
}