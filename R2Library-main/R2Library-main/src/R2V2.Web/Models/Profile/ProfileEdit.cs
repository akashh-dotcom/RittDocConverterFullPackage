namespace R2V2.Web.Models.Profile
{
    public class ProfileEdit : BaseModel
    {
        public UserEdit User { get; set; }

        public string UrlReferrer { get; set; }

        public bool IsExpertReviewerEnabled { get; set; }
    }
}