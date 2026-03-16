#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialList : AdminBaseModel
    {
        public SpecialList()
        {
        }

        public SpecialList(List<SpecialAdminModel> specialAdminModels, List<SpecialResourceModel> specialResourceModels)
        {
            if (specialAdminModels != null)
            {
                foreach (var specialAdminModel in specialAdminModels)
                {
                    if (specialAdminModel.SpecialDiscounts != null)
                    {
                        foreach (var specialDiscount in specialAdminModel.SpecialDiscounts)
                        {
                            specialDiscount.SetResourceCount(specialResourceModels);
                        }
                    }
                }

                Specials = specialAdminModels;
            }
        }

        public List<SpecialAdminModel> Specials { get; set; }
    }
}