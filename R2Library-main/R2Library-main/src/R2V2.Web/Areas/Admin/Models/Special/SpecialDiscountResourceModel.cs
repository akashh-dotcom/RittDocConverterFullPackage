namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialDiscountResourceModel
    {
        public SpecialDiscountResourceModel()
        {
        }

        public SpecialDiscountResourceModel(int specialDiscountResourceId, int specialDiscountId,
            Resource.Resource resource)
        {
            SpecialDiscountResourceId = specialDiscountResourceId;
            SpecialDiscountId = specialDiscountId;
            Resource = resource;
        }

        public Resource.Resource Resource { get; set; }
        public int SpecialDiscountResourceId { get; set; }
        public int SpecialDiscountId { get; set; }
    }
}