namespace R2V2.Web.Models.Browse
{
    public class AuthorSummary : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ResourceCount { get; set; }
    }
}