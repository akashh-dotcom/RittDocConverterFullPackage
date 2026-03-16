namespace R2V2.Web.Areas.Admin.Models.SpecialCollectionManagement
{
    public class DeleteModel : AdminBaseModel
    {
        public int CollectionId { get; set; }
        public string Name { get; set; }
        public int ResourceCount { get; set; }
    }
}