namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialIcon : AdminBaseModel
    {
        public SpecialIcon()
        {
        }

        public SpecialIcon(int specialId)
        {
            SpecialId = specialId;
        }

        public int SpecialId { get; set; }
    }
}