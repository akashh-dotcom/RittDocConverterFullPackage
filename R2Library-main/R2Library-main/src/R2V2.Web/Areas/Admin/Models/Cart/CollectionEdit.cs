namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class CollectionEdit : AdminBaseModel
    {
        public int NumberOfLicenses { get; set; }

        public InstitutionResource InstitutionResource { get; set; }
    }
}