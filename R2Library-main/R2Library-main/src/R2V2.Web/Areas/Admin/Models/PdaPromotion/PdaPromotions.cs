#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaPromotion
{
    public class PdaPromotions : AdminBaseModel
    {
        public PdaPromotions(IEnumerable<CachedPdaPromotion> pdaPromotions)
        {
            List = pdaPromotions.Select(x => new PdaPromotionModel(x)).ToList();
        }

        public List<PdaPromotionModel> List { get; set; }
    }
}